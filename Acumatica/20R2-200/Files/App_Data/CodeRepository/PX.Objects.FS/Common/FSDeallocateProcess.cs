using PX.Common;
using PX.Data;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class FSDeallocateProcess : PXGraph<FSDeallocateProcess>
    {
        private static readonly Lazy<FSDeallocateProcess> _fsDeallocateProcess = new Lazy<FSDeallocateProcess>(CreateInstance<FSDeallocateProcess>);
        public static FSDeallocateProcess SingleFSDeallocateProcess => _fsDeallocateProcess.Value;

        public static void DeallocateServiceOrders(ServiceOrderEntry docgraph, List<FSSODetSplit> splitsToDeallocate)
            => SingleFSDeallocateProcess.DeallocateServiceOrdersInt(docgraph, splitsToDeallocate);

        public virtual void DeallocateServiceOrdersInt(ServiceOrderEntry docgraph, List<FSSODetSplit> splitsToDeallocate)
        {
            PXRowUpdating cancel_handler = new PXRowUpdating((sender, e) => { e.Cancel = true; });
            docgraph.RowUpdating.AddHandler<FSSODet>(cancel_handler);

            IEnumerable<IGrouping<(string, string), FSSODetSplit>> orderGroups = splitsToDeallocate.GroupBy(x => (x.SrvOrdType, x.RefNbr));

            foreach (IGrouping<(string, string), FSSODetSplit> orderGroup in orderGroups)
            {
                FSSODetSplit firstSplit = orderGroup.First();

                docgraph.Clear();
                FSServiceOrder currentServiceOrder = docgraph.ServiceOrderRecords.Current = docgraph.ServiceOrderRecords.Search<FSServiceOrder.refNbr>(firstSplit.RefNbr, firstSplit.SrvOrdType);

                if (currentServiceOrder.SrvOrdType != firstSplit.SrvOrdType || currentServiceOrder.RefNbr != firstSplit.RefNbr)
                {
                    throw new PXException(TX.Error.SERVICE_ORDER_NOT_FOUND);
                }

                IEnumerable<IGrouping<(string, string, int?), FSSODetSplit>> lineGroups = orderGroup.GroupBy(x => (x.SrvOrdType, x.RefNbr, x.LineNbr));

                foreach (IGrouping<(string, string, int?), FSSODetSplit> lineGroup in lineGroups)
                {
                    FSSODetSplit firstSplit2 = lineGroup.First();

                    FSSODet soDetLine = docgraph.ServiceOrderDetails.Current = docgraph.ServiceOrderDetails.Search<FSSODet.lineNbr>(firstSplit2.LineNbr);

                    if (soDetLine.LineNbr != firstSplit2.LineNbr)
                    {
                        throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODet)));
                    }

                    foreach (FSSODetSplit forSplit in lineGroup)
                    {
                        FSSODetSplit realSplit = docgraph.Splits.Current = docgraph.Splits.Search<FSSODetSplit.splitLineNbr>(forSplit.SplitLineNbr);

                        if (forSplit.SplitLineNbr != realSplit.SplitLineNbr)
                        {
                            throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSSODetSplit)));
                        }

                        if (CanUpdateRealSplit(realSplit, forSplit) == true)
                        {
                            decimal? baseDeallocateQty = realSplit.BaseQty - forSplit.BaseQty;
                            DeallocateSplitLine(docgraph, soDetLine, baseDeallocateQty, realSplit);
                        }
                    }
                }

                //docgraph.SelectTimeStamp(); check
                docgraph.SkipTaxCalcAndSave();
            }

            docgraph.RowUpdating.RemoveHandler<FSSODet>(cancel_handler);
        }

        public virtual bool CanUpdateRealSplit(FSSODetSplit realSplit, FSSODetSplit forSplit)
        {
            if (realSplit.Completed == true)
            {
                return false;
            }

            if (string.IsNullOrEmpty(realSplit.LotSerialNbr) == false
                    && realSplit.LotSerialNbr != forSplit.LotSerialNbr)
            {
                return false;
            }

            if (realSplit.SiteID != null && realSplit.SiteID != forSplit.SiteID)
            {
                return false;
            }

            if (realSplit.LocationID != null && realSplit.LocationID != forSplit.LocationID)
            {
                return false;
            }

            return true;
        }

        public virtual decimal? DeallocateSplitLine(ServiceOrderEntry docgraph,
                                                                           FSSODet line,
                                                                           decimal? baseDeallocationQty,
                                                                           FSSODetSplit split)
        {
            if (baseDeallocationQty <= 0m)
            {
                return 0m;
            }

            PXCache splitsCache = docgraph.Splits.Cache;

            decimal? baseOpenQty = split.BaseQty - split.BaseShippedQty;
            decimal? baseNewSplitQty;
            if (baseOpenQty <= baseDeallocationQty)
            {
                baseNewSplitQty = 0m;
                split.BaseShippedQty += baseOpenQty;
                baseDeallocationQty -= baseOpenQty;
            }
            else
            {
                baseNewSplitQty = baseOpenQty - baseDeallocationQty;
                split.BaseQty = baseDeallocationQty;
                split.Qty = INUnitAttribute.ConvertFromBase(splitsCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                split.BaseShippedQty = baseDeallocationQty;
                baseDeallocationQty = 0;
            }

            split.ShippedQty = INUnitAttribute.ConvertFromBase(splitsCache, split.InventoryID, split.UOM, (decimal)split.BaseShippedQty, INPrecision.QUANTITY);
            split.Completed = true;

            INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(docgraph, split.PlanID);
            if (plan != null)
            {
                docgraph.Caches[typeof(INItemPlan)].Delete(plan);
                split.PlanID = null;
            }

            docgraph.lsFSSODetSelect.SuppressedMode = true;
            split = docgraph.Splits.Update(split);
            docgraph.lsFSSODetSelect.SuppressedMode = false;

            if (baseNewSplitQty > 0m)
            {
                FSSODetSplit newSplit = (FSSODetSplit)docgraph.Splits.Cache.CreateCopy(split);

                FSPOReceiptProcess.ClearScheduleReferences(ref newSplit);
                newSplit = (FSSODetSplit)docgraph.Splits.Cache.CreateCopy(docgraph.Splits.Insert(newSplit));

                newSplit.PlanType = split.PlanType;
                newSplit.IsAllocated = false;
                newSplit.ShipmentNbr = null;
                newSplit.LotSerialNbr = null;
                newSplit.VendorID = null;

                newSplit.BaseReceivedQty = 0m;
                newSplit.ReceivedQty = 0m;
                newSplit.BaseShippedQty = 0m;
                newSplit.ShippedQty = 0m;

                newSplit.BaseQty = baseNewSplitQty;
                newSplit.Qty = INUnitAttribute.ConvertFromBase(splitsCache, newSplit.InventoryID, newSplit.UOM, (decimal)newSplit.BaseQty, INPrecision.QUANTITY);

                docgraph.Splits.Update(newSplit);
            }

            ConfirmSingleLine(docgraph, split, line);

            return baseDeallocationQty;
        }

        public virtual void ConfirmSingleLine(ServiceOrderEntry docgraph, FSSODetSplit splitLine, FSSODet line)
        {
            FSSODet newLine = line;

            docgraph.lsFSSODetSelect.SuppressedMode = true;
            PXCache cache = docgraph.Caches[typeof(FSSODet)];

            if (newLine.BaseShippedQty < newLine.BaseOrderQty && newLine.IsFree == false)
            {
                newLine.BaseShippedQty += splitLine.BaseShippedQty;
                newLine.ShippedQty += splitLine.ShippedQty;
                newLine.OpenQty = newLine.OrderQty - newLine.ShippedQty;
                newLine.BaseOpenQty = newLine.BaseOrderQty - newLine.BaseShippedQty;
                newLine.ClosedQty = newLine.ShippedQty;
                newLine.BaseClosedQty = newLine.BaseShippedQty;

                cache.Update(newLine);
            }
            else
            {
                newLine.OpenQty = 0m;
                newLine.ClosedQty = newLine.OrderQty;
                newLine.ShippedQty += splitLine.ShippedQty;
                newLine.BaseOpenQty = newLine.BaseOrderQty - newLine.BaseShippedQty;
                newLine.BaseClosedQty = newLine.BaseOrderQty;
                newLine.Completed = true;

                cache.Update(newLine);
                docgraph.lsFSSODetSelect.CompleteSchedules(cache, line);
            }

            docgraph.lsFSSODetSelect.SuppressedMode = false;
        }
    }
}
