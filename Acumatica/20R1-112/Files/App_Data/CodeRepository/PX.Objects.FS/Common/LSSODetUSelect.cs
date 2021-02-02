using PX.Common;
using PX.Data;
using PX.Objects.Common.Exceptions;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.SO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;

namespace PX.Objects.FS
{
    public class LSFSSODetLine : LSSelectSOBase<FSSODet, FSSODetSplit,
        Where<FSSODetSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
        And<FSSODetSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
    {
        #region State
        public bool IsLocationEnabled
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);

                if (ordertype == null || (ordertype.RequireShipping == false && ordertype.RequireLocation == true && ordertype.INDocType != INTranType.NoUpdate))
                    return true;
                else
                    return false;
            }
        }

        public bool IsLSEntryEnabled
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);

                return (ordertype == null || ordertype.RequireLocation == true || ordertype.RequireLotSerial == true);
            }
        }

        public bool IsLotSerialRequired
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);

                return (ordertype == null || ordertype.RequireLotSerial == true);
            }
        }

        public bool IsAllocationEntryEnabled
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);

                return (ordertype == null || ordertype.RequireShipping == true);
            }
        }

        public bool IsAllocationRequired
        {
            get
            {
                SOOrderType ordertype = PXSetup<SOOrderType>.Select(this._Graph);

                return (ordertype == null || ordertype.RequireAllocation == true);
            }
        }
        #endregion
        #region Ctor
        public LSFSSODetLine(PXGraph graph)
            : base(graph)
        {
            MasterQtyField = typeof(FSSODet.orderQty);
            graph.FieldDefaulting.AddHandler<FSSODetSplit.subItemID>(FSSODetSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetSplit.locationID>(FSSODetSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetSplit.invtMult>(FSSODetSplit_InvtMult_FieldDefaulting);
            graph.RowSelected.AddHandler<FSServiceOrder>(Parent_RowSelected);
            graph.RowUpdated.AddHandler<FSServiceOrder>(FSServiceOrder_RowUpdated);
            graph.RowSelected.AddHandler<FSSODetSplit>(FSSODetSplit_RowSelected);
            graph.RowPersisting.AddHandler<FSSODetSplit>(FSSODetSplit_RowPersisting);
        }
        #endregion

        #region Implementation
        public override IEnumerable BinLotSerial(PXAdapter adapter)
        {
            if (IsLSEntryEnabled || IsAllocationEntryEnabled)
            {
                if (MasterCache.Current != null 
                    && ((FSSODet)MasterCache.Current).SOLineType != SOLineType.Inventory)
                {
                    throw new PXSetPropertyException(TX.Error.CANNOT_USE_ALLOCATIONS_FOR_NONSTOCK_ITEMS);
                }

                View.AskExt(true);
            }
            return adapter.Get();
        }

        protected virtual void FSServiceOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (IsLSEntryEnabled && !sender.ObjectsEqual<FSServiceOrder.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<FSServiceOrder.hold>(e.Row) == false)
            {
                PXCache cache = sender.Graph.Caches[typeof(FSSODet)];

                foreach (FSSODet item in PXParentAttribute.SelectSiblings(cache, null, typeof(FSServiceOrder)))
                {
                    if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
                    {
                        cache.RaiseExceptionHandling<FSSODet.orderQty>(item, item.Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned));

                        if (cache.GetStatus(item) == PXEntryStatus.Notchanged)
                        {
                            cache.SetStatus(item, PXEntryStatus.Updated);
                        }
                    }
                }
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                var row = (FSSODet)e.Row;

                if ((row.SOLineType == SOLineType.Inventory || row.SOLineType == SOLineType.NonInventory && row.InvtMult == (short)-1) && row.TranType != INTranType.NoUpdate && row.BaseQty < 0m)
                {
                    if (sender.RaiseExceptionHandling<FSSODet.orderQty>(e.Row, ((FSSODet)e.Row).Qty, new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString())))
                    {
                        throw new PXRowPersistingException(typeof(FSSODet.orderQty).Name, ((FSSODet)e.Row).Qty, CS.Messages.Entry_GE, ((int)0).ToString());
                    }
                    return;
                }

                if (IsLSEntryEnabled)
                {
                    PXCache cache = sender.Graph.Caches[typeof(FSServiceOrder)];
                    object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(FSServiceOrder)) ?? cache.Current;

                    bool? onHold = (bool?)cache.GetValue<FSServiceOrder.hold>(doc);

                    if (onHold == false && Math.Abs((decimal)((FSSODet)e.Row).BaseQty) >= 0.0000005m && (((FSSODet)e.Row).UnassignedQty >= 0.0000005m || ((FSSODet)e.Row).UnassignedQty <= -0.0000005m))
                    {
                        if (sender.RaiseExceptionHandling<FSSODet.orderQty>(e.Row, ((FSSODet)e.Row).Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned)))
                        {
                            throw new PXRowPersistingException(typeof(FSSODet.orderQty).Name, ((FSSODet)e.Row).Qty, SO.Messages.BinLotSerialNotAssigned);
                        }
                    }
                }
            }

            //for normal orders there are only when received numbers which do not require any additional processing
            if (!IsLSEntryEnabled)
            {
                if (((FSSODet)e.Row).TranType == INTranType.Transfer && DetailCounters.ContainsKey((FSSODet)e.Row))
                {
                    //keep Counters when adding splits to Transfer order
                    DetailCounters[(FSSODet)e.Row].UnassignedNumber = 0;
                }
                else
                {
                    DetailCounters[(FSSODet)e.Row] = new Counters { UnassignedNumber = 0 };
                }
            }

            base.Master_RowPersisting(sender, e);
        }

        protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
        {
            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, null, typeof(FSSODet.inventoryID).Name, InventoryID);

            if (item != null)
            {
                INLotSerClass lsclass = PXSelectReadonly<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.Select(sender.Graph, item.LotSerClassID);

                return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
            }

            return null;
        }

        public override IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster row, AvailabilityFetchMode fetchMode)
        {
            if (row != null)
            {
                FSSODetSplit copy = row as FSSODetSplit;

                if (copy == null)
                {
                    copy = Convert(row as FSSODet);

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODet), row);

                    if (string.IsNullOrEmpty(row.LotSerialNbr) == false)
                    {
                        DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetSplit)], copy);
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
                    {
                        if (_detailsRequested++ == 50)
                        {
                            foreach (PXResult<FSSODet, INUnit, INSiteStatus> res in
                                PXSelectReadonly2<FSSODet,
                                InnerJoin<INUnit,
                                On<
                                    INUnit.inventoryID, Equal<FSSODet.inventoryID>, 
                                    And<INUnit.fromUnit, Equal<FSSODet.uOM>>>,
                                InnerJoin<INSiteStatus,
                                On<FSSODet.inventoryID, 
                                    Equal<INSiteStatus.inventoryID>, 
                                    And<FSSODet.subItemID, Equal<INSiteStatus.subItemID>, 
                                    And<FSSODet.siteID, Equal<INSiteStatus.siteID>>>>>>,
                                Where<
                                    FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                    And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                               .Select(sender.Graph))
                            {
                                INSiteStatus status = res;
                                INUnit unit = res;

                                PXSelectReadonly<INUnit,
                                Where<
                                    INUnit.unitType, Equal<INUnitType.inventoryItem>,
                                And<
                                    INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
                                And<
                                    INUnit.toUnit, Equal<Required<INUnit.toUnit>>,
                                And<
                                    INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>>
                                .StoreCached(sender.Graph, new PXCommandKey(new object[] { unit.InventoryID, unit.ToUnit, unit.FromUnit }), new List<object> { unit });

                                PXSelectReadonly<INSiteStatus,
                                Where<
                                    INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                                And<
                                    INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                                And<
                                    INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>>
                                .StoreCached(sender.Graph, new PXCommandKey(new object[] { status.InventoryID, status.SubItemID, status.SiteID }), new List<object> { status });
                            }

                            foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<FSServiceOrder.noteID>>>>.Select(this._Graph))
                            {
                                PXSelect<INItemPlan,
                                Where<
                                    INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                                .StoreCached(this._Graph, new PXCommandKey(new object[] { plan.PlanID }), new List<object> { plan });
                            }
                        }

                        IQtyAllocated result = AvailabilityFetch(sender, copy, AvailabilityFetchMode.None);
                        return DeductAllocated(sender, (FSSODet)row, result);
                    }
                }

                return AvailabilityFetch(sender, copy, fetchMode);
            }
            return null;
        }

        public virtual IQtyAllocated DeductAllocated(PXCache sender, FSSODet fsSODetRow, IQtyAllocated result)
        {
            if (result == null) return null;

            decimal? lineQtyAvail = (decimal?)sender.GetValue<FSSODet.lineQtyAvail>(fsSODetRow);
            decimal? lineQtyHardAvail = (decimal?)sender.GetValue<FSSODet.lineQtyHardAvail>(fsSODetRow);

            if (lineQtyAvail == null || lineQtyHardAvail == null)
            {
                lineQtyAvail = 0m;
                lineQtyHardAvail = 0m;

                foreach (FSSODetSplit split in SelectDetail(DetailCache, fsSODetRow))
                {
                    FSSODetSplit copy = split;

                    if (copy.PlanID != null)
                    {
                        INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, copy.PlanID);

                        if (plan != null)
                        {
                            copy = PXCache<FSSODetSplit>.CreateCopy(copy);
                            copy.PlanType = plan.PlanType;
                        }
                    }

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODet), fsSODetRow);

                    decimal signQtyAvail;
                    decimal signQtyHardAvail;
                    INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(DetailCache, copy, out signQtyAvail, out signQtyHardAvail);

                    if (signQtyAvail != 0m)
                    {
                        lineQtyAvail -= signQtyAvail * (copy.BaseQty ?? 0m);
                    }

                    if (signQtyHardAvail != 0m)
                    {
                        lineQtyHardAvail -= signQtyHardAvail * (copy.BaseQty ?? 0m);
                    }
                }

                sender.SetValue<FSSODet.lineQtyAvail>(fsSODetRow, lineQtyAvail);
                sender.SetValue<FSSODet.lineQtyHardAvail>(fsSODetRow, lineQtyHardAvail);
            }

            result.QtyAvail += lineQtyAvail;
            result.QtyHardAvail += lineQtyHardAvail;
            result.QtyNotAvail = -lineQtyAvail;

            return result;
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            IQtyAllocated availability = AvailabilityFetch(sender, (FSSODet)e.Row, !(e.Row != null && (((FSSODet)e.Row).Completed == true)) ? AvailabilityFetchMode.ExcludeCurrent : AvailabilityFetchMode.None);

            if (availability != null)
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((FSSODet)e.Row).InventoryID);

                decimal unitRate = INUnitAttribute.ConvertFromBase<FSSODet.inventoryID, FSSODet.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
                availability.QtyOnHand = PXDBQuantityAttribute.Round((decimal)availability.QtyOnHand * unitRate);
                availability.QtyAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyAvail * unitRate);
                availability.QtyNotAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyNotAvail * unitRate);
                availability.QtyHardAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyHardAvail * unitRate);

                if (IsAllocationEntryEnabled)
                {
                    Decimal? allocated = PXDBQuantityAttribute.Round((decimal)(((FSSODet)e.Row).LineQtyHardAvail ?? 0m) * unitRate); ;
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_AllocatedInfo,
                            sender.GetValue<FSSODet.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
                }
                else
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_Info,
                            sender.GetValue<FSSODet.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));


                AvailabilityCheck(sender, (FSSODet)e.Row, availability);
            }
            else
            {
                //handle missing UOM
                INUnitAttribute.ConvertFromBase<FSSODet.inventoryID, FSSODet.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

        protected FSServiceOrder _LastSelected;

        protected virtual void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (_LastSelected == null || !object.ReferenceEquals(_LastSelected, e.Row))
            {
                PXUIFieldAttribute.SetRequired<FSSODet.locationID>(this.MasterCache, IsLocationEnabled);

                PXUIFieldAttribute.SetVisible<FSSODet.locationID>(this.MasterCache, null, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODet.lotSerialNbr>(this.MasterCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODet.expireDate>(this.MasterCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<FSSODetSplit.inventoryID>(this.DetailCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.expireDate>(this.DetailCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.shipDate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.isAllocated>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.completed>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.shippedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.shipmentNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.pOType>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.pONbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.pOReceiptNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.pOSource>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.pOCreate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.receivedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetSplit.refNoteID>(this.DetailCache, null, IsAllocationEntryEnabled);

                PXView view;

                if (sender.Graph.Views.TryGetValue(Prefixed("lotseropts"), out view))
                {
                    view.AllowSelect = IsLSEntryEnabled;
                }

                if (e.Row is FSServiceOrder)
                {
                    _LastSelected = (FSServiceOrder)e.Row;
                }
            }

            this.SetEnabled(IsLSEntryEnabled || IsAllocationEntryEnabled);
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODet row, decimal? baseQty)
        {
            IssueAvailable(sender, row, baseQty, false);
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODet row, decimal? baseQty, bool isUncomplete)
        {
            DetailCounters.Remove(row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

            foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
                                           Where<
                                               INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                                           And<
                                               INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                                           And<
                                               INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
                                           OrderBy<
                                               Asc<INLocation.pickPriority>>>
                                           .Select(this._Graph, row.InventoryID, row.SubItemID, row.SiteID))
            {
                FSSODetSplit split = (FSSODetSplit)row;

                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }

                split.SplitLineNbr = null;
                split.IsAllocated = row.RequireAllocation;
                split.SiteID = row.SiteID;

                object newval;
                DetailCache.RaiseFieldDefaulting<FSSODetSplit.allocatedPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetSplit.allocatedPlanType>(split, newval);

                DetailCache.RaiseFieldDefaulting<FSSODetSplit.backOrderPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetSplit.backOrderPlanType>(split, newval);

                decimal SignQtyAvail;
                decimal SignQtyHardAvail;
                INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(DetailCache, split, out SignQtyAvail, out SignQtyHardAvail);

                if (SignQtyHardAvail < 0m)
                {
                    SiteStatus accumavail = new SiteStatus();
                    PXCache<INSiteStatus>.RestoreCopy(accumavail, avail);

                    accumavail = (SiteStatus)this._Graph.Caches[typeof(SiteStatus)].Insert(accumavail);

                    decimal? AvailableQty = avail.QtyHardAvail + accumavail.QtyHardAvail;

                    if (AvailableQty <= 0m)
                    {
                        continue;
                    }

                    if (AvailableQty < baseQty)
                    {
                        split.BaseQty = AvailableQty;
                        split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);
                        DetailCache.Insert(split);
                        baseQty -= AvailableQty;
                    }
                    else
                    {
                        split.BaseQty = baseQty;
                        split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)baseQty, INPrecision.QUANTITY);
                        DetailCache.Insert(split);
                        baseQty = 0m;
                        break;
                    }
                }
            }

            if (baseQty > 0m && row.InventoryID != null && row.SiteID != null && (row.SubItemID != null || (row.SubItemID == null && row.IsStockItem != true && row.IsKit == true) || row.SOLineType == SOLineType.NonInventory))
            {
                FSSODetSplit split = (FSSODetSplit)row;

                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }

                split.SplitLineNbr = null;
                split.IsAllocated = false;
                split.BaseQty = baseQty;
                split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

                baseQty = 0m;

                if (isUncomplete)
                {
                    split.POCreate = false;
                    split.POSource = null;
                }

                DetailCache.Insert(PXCache<FSSODetSplit>.CreateCopy(split));
            }
        }

        public override void UpdateParent(PXCache sender, FSSODet row)
        {
            if (row != null && row.RequireShipping == true)
            {
                decimal BaseQty;
                UpdateParent(sender, row, null, null, out BaseQty);
            }
            else
            {
                base.UpdateParent(sender, row);
            }
        }

        public override void UpdateParent(PXCache sender, FSSODetSplit row, FSSODetSplit oldRow)
        {
            FSSODet parent = (FSSODet)LSParentAttribute.SelectParent(sender, row ?? oldRow, typeof(FSSODet));

            if (parent != null && parent.RequireShipping == true)
            {
                if ((row ?? oldRow) != null && SameInventoryItem((ILSMaster)(row ?? oldRow), (ILSMaster)parent))
                {
                    FSSODet oldrow = PXCache<FSSODet>.CreateCopy(parent);
                    decimal BaseQty;

                    UpdateParent(sender, parent, (row != null && row.Completed == false ? row : null), (oldRow != null && oldRow.Completed == false ? oldRow : null), out BaseQty);

                    using (InvtMultScope<FSSODet> ms = new InvtMultScope<FSSODet>(parent))
                    {
                        if (IsLotSerialRequired && row != null)
                        {
                            parent.UnassignedQty = 0m;

                            if (IsLotSerialItem(sender, row))
                            {
                                object[] splits = SelectDetail(sender, row);

                                foreach (FSSODetSplit split in splits)
                                {
                                    if (split.LotSerialNbr == null)
                                    {
                                        parent.UnassignedQty += split.BaseQty;
                                    }
                                }
                            }
                        }      
                        
                        parent.BaseQty = BaseQty;
                        parent.Qty = INUnitAttribute.ConvertFromBase(sender.Graph.Caches[typeof(FSSODet)], parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
                    }

                    if (sender.Graph.Caches[typeof(FSSODet)].GetStatus(parent) == PXEntryStatus.Notchanged)
                    {
                        sender.Graph.Caches[typeof(FSSODet)].SetStatus(parent, PXEntryStatus.Updated);
                    }

                    sender.Graph.Caches[typeof(FSSODet)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);

                    if (sender.Graph.Caches[typeof(FSSODet)].RaiseRowUpdating(oldrow, parent))
                    {
                        sender.Graph.Caches[typeof(FSSODet)].RaiseRowUpdated(parent, oldrow);
                    }
                    else
                    {
                        sender.Graph.Caches[typeof(FSSODet)].RestoreCopy(parent, oldrow);
                    }
                }
            }
            else
            {
                base.UpdateParent(sender, row, oldRow);
            }
        }

        public static void ResetAvailabilityCounters(FSSODet row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

        public override void UpdateParent(PXCache sender, FSSODet row, FSSODetSplit splitDet, FSSODetSplit oldSplitDet, out decimal baseQty)
        {
            ResetAvailabilityCounters(row);

            bool counted = DetailCounters.ContainsKey(row);

            base.UpdateParent(sender, row, splitDet, oldSplitDet, out baseQty);

            if (!counted && oldSplitDet != null)
            {
                Counters counters;

                if (DetailCounters.TryGetValue(row, out counters))
                {
                    if (oldSplitDet.POCreate == true)
                    {
                        counters.BaseQty += (decimal)oldSplitDet.BaseReceivedQty + (decimal)oldSplitDet.BaseShippedQty;
                    }
                    if (oldSplitDet.ShipmentNbr != null)
                    {
                        counters.BaseQty += (decimal)(oldSplitDet.BaseQty - oldSplitDet.BaseShippedQty);
                    }

                    baseQty = counters.BaseQty;
                }
            }
        }

        protected override void UpdateCounters(PXCache sender, Counters counters, FSSODetSplit splitDetail)
        {
            base.UpdateCounters(sender, counters, splitDetail);

            if (splitDetail.POCreate == true)
            {
                //base shipped qty in context of purchase for so is meaningless and equals zero, so it's appended for dropship context
                counters.BaseQty -= (decimal)splitDetail.BaseReceivedQty + (decimal)splitDetail.BaseShippedQty;
            }

            if (IsAllocationEntryEnabled)
            {
                counters.LotSerNumbersNull = -1;
                counters.LotSerNumber = null;
                counters.LotSerNumbers.Clear();
            }

            //if (detail.ShipmentNbr != null)
            //{
            //    counters.BaseQty -= (decimal)(detail.BaseQty - detail.BaseShippedQty);
            //}
        }

        protected int _detailsRequested = 0;

        protected override object[] SelectDetail(PXCache sender, FSSODetSplit row)
        {
            return SelectDetail(sender, row, true);
        }

        protected virtual object[] SelectDetail(PXCache sender, FSSODetSplit row, bool excludeCompleted = true)
        {
            object[] ret;

            if (_detailsRequested > 50)
            {
                ret = PXParentAttribute.SelectSiblings(sender, row, typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetSplit)a, row) && ((FSSODetSplit)a).LineNbr == row.LineNbr && (((FSSODetSplit)a).Completed == false || excludeCompleted == false && ((FSSODetSplit)a).PONbr == null && ((FSSODetSplit)a).SOOrderNbr == null));
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => (((FSSODetSplit)a).Completed == false || excludeCompleted == false && ((FSSODetSplit)a).PONbr == null && ((FSSODetSplit)a).SOOrderNbr == null));
        }


        /*protected override object[] SelectDetail(PXCache sender, FSSODet row)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, Convert(row), typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetSplit)a, row) && ((FSSODetSplit)a).LineNbr == row.LineNbr && ((FSSODetSplit)a).Completed == false);
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => ((FSSODetSplit)a).Completed == false);
        }*/

        protected override object[] SelectDetailOrdered(PXCache sender, FSSODetSplit row)
        {
            return SelectDetailOrdered(sender, row, true);
        }

        protected virtual object[] SelectDetailOrdered(PXCache sender, FSSODetSplit row, bool excludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, excludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetSplit)a).Completed == true ? 0 : ((FSSODetSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetSplit)b).Completed == true ? 0 : ((FSSODetSplit)b).IsAllocated == true ? 1 : 2;

                int res = ((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetSplit)b).SplitLineNbr;

                return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        protected override object[] SelectDetailReversed(PXCache sender, FSSODetSplit row)
        {
            return SelectDetailReversed(sender, row, true);
        }

        protected virtual object[] SelectDetailReversed(PXCache sender, FSSODetSplit row, bool excludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, excludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetSplit)a).Completed == true ? 0 : ((FSSODetSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetSplit)b).Completed == true ? 0 : ((FSSODetSplit)b).IsAllocated == true ? 1 : 2;

                int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetSplit)b).SplitLineNbr;

                return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        public virtual void UncompleteSchedules(PXCache sender, FSSODet row)
        {
            DetailCounters.Remove(row);

            decimal? UnshippedQty = row.BaseOpenQty;

            foreach (object detail in SelectDetailReversed(DetailCache, row, false))
            {
                if (((FSSODetSplit)detail).ShipmentNbr == null)
                {
                    UnshippedQty -= ((FSSODetSplit)detail).BaseQty;

                    FSSODetSplit newdetail = PXCache<FSSODetSplit>.CreateCopy((FSSODetSplit)detail);
                    newdetail.Completed = false;

                    DetailCache.Update(newdetail);
                }
            }

            IssueAvailable(sender, row, (decimal)UnshippedQty, true);
        }

        public virtual void CompleteSchedules(PXCache sender, FSSODet row)
        {
            DetailCounters.Remove(row);

            string LastShipmentNbr = null;
            decimal? LastUnshippedQty = 0m;
            foreach (object detail in SelectDetailReversed(DetailCache, row, false))
            {
                if (LastShipmentNbr == null && ((FSSODetSplit)detail).ShipmentNbr != null)
                {
                    LastShipmentNbr = ((FSSODetSplit)detail).ShipmentNbr;
                }

                if (LastShipmentNbr != null && ((FSSODetSplit)detail).ShipmentNbr == LastShipmentNbr)
                {
                    LastUnshippedQty += ((FSSODetSplit)detail).BaseOpenQty;
                }
            }

            TruncateSchedules(sender, row, (decimal)LastUnshippedQty);

            foreach (object detail in SelectDetailReversed(DetailCache, row))
            {
                FSSODetSplit newdetail = PXCache<FSSODetSplit>.CreateCopy((FSSODetSplit)detail);
                newdetail.Completed = true;

                DetailCache.Update(newdetail);
            }
        }

        public virtual void TruncateSchedules(PXCache sender, FSSODet row, decimal baseQty)
        {
            DetailCounters.Remove(row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

            foreach (object detail in SelectDetailReversed(DetailCache, row))
            {
                if (baseQty >= ((ILSDetail)detail).BaseQty)
                {
                    baseQty -= (decimal)((ILSDetail)detail).BaseQty;
                    DetailCache.Delete(detail);
                }
                else
                {
                    FSSODetSplit newdetail = PXCache<FSSODetSplit>.CreateCopy((FSSODetSplit)detail);
                    newdetail.BaseQty -= baseQty;
                    newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

                    DetailCache.Update(newdetail);
                    break;
                }
            }
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODet row)
        {
            IssueAvailable(sender, row, row.BaseOpenQty);
        }

        protected override void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<FSSODet> e)
        {
            ILSMaster iLSMasterRow = (ILSMaster)e.Row;
            FSSODet row = (FSSODet)e.Row;

            if (iLSMasterRow.InventoryID == null || row.IsPrepaid == true)
            {
                return;
            }

            bool skipSplitCreating = false;

            if (IsLocationEnabled)
            {
                InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODet.inventoryID>(sender, e.Row);
                if (ii != null && ii.StkItem == false && ii.KitItem == false && ii.NonStockShip == false)
                    skipSplitCreating = true;
            }

            if (!skipSplitCreating && (IsLocationEnabled || (IsLotSerialRequired && e.ExternalCall && IsLotSerialItem(sender, e.Row))))
            {
                base._Master_RowInserted(sender, e);
            }
            else
            {
                //sender.SetValue<FSSODet.locationID>(e.Row, null);
                //sender.SetValue<FSSODet.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODet.expireDate>(e.Row, null);

                if (IsAllocationEntryEnabled && e.Row != null && e.Row.BaseOpenQty != 0m)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

                    //if (e.Row.InvtMult == -1 && item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
                    if (item != null && (e.Row.SOLineType == SOLineType.Inventory || e.Row.SOLineType == SOLineType.NonInventory))
                    {
                        IssueAvailable(sender, e.Row);

                    }
                }

                AvailabilityCheck(sender, e.Row);
            }
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            try
            {
                base.Master_RowUpdated(sender, e);
            }
            catch (PXUnitConversionException ex)
            {
                if (!PXUIFieldAttribute.GetErrors(sender, e.Row, PXErrorLevel.Error).Keys.Any(a => string.Compare(a, typeof(FSSODet.uOM).Name, StringComparison.InvariantCultureIgnoreCase) == 0))
                    sender.RaiseExceptionHandling<FSSODet.uOM>(e.Row, null, ex);
            }
        }

        protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<FSSODet> e)
        {
            FSSODet row = e.Row as FSSODet;
            FSSODet oldRow = e.OldRow as FSSODet;

            if (row == null || (oldRow.InventoryID == null && row.InventoryID == null) || row.IsPrepaid == true) return;

            bool skipSplitCreating = false;
            InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODet.inventoryID>(sender, row);

            if (IsLocationEnabled && ii != null && ii.StkItem == false && ii.KitItem == false && ii.NonStockShip == false)
            {
                skipSplitCreating = true;
            }

            if (ii != null && ii.StkItem == false && ii.KitItem == true && row.Behavior != SOBehavior.CM && row.Behavior != SOBehavior.IN)
            {
                skipSplitCreating = true;
            }

            if (!skipSplitCreating && (IsLocationEnabled || (IsLotSerialRequired && e.ExternalCall && row.POCreate != true && IsLotSerialItem(sender, e.Row)))) //check condition
            {
                base._Master_RowUpdated(sender, e);

                if (ii != null && (ii.KitItem == true || ii.StkItem == true))
                {
                    AvailabilityCheck(sender, (FSSODet)e.Row);
                }
            }
            else
            {
                //sender.SetValue<FSSODet.locationID>(e.Row, null);
                //sender.SetValue<FSSODet.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODet.expireDate>(e.Row, null);

                if (IsAllocationEntryEnabled)
                {
                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);

                    if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.SiteID != e.Row.SiteID || e.OldRow.SubItemID != e.Row.SubItemID || e.OldRow.InvtMult != e.Row.InvtMult || e.OldRow.UOM != e.Row.UOM))
                    {
                        RaiseRowDeleted(sender, e.OldRow);
                        RaiseRowInserted(sender, e.Row);
                    }
                    //else if (e.Row.InvtMult == -1 && item != null && (e.Row.LineType == SOLineType.Inventory || e.Row.LineType == SOLineType.NonInventory))
                    else if (item != null && (e.Row.SOLineType == SOLineType.Inventory || e.Row.SOLineType == SOLineType.NonInventory))
                    {
                        // prevent setting null to quantity from mobile app
                        if (this._Graph.IsMobile && e.Row.OrderQty == null)
                        {
                            e.Row.OrderQty = e.OldRow.OrderQty;
                        }

                        //ConfirmShipment(), CorrectShipment() use SuppressedMode and never end up here.
                        //OpenQty is calculated via formulae, ExternalCall is used to eliminate duplicating formula arguments here
                        //direct OrderQty for AddItem()
                        if (e.Row.OrderQty != e.OldRow.OrderQty || e.Row.Completed != e.OldRow.Completed)
                        {
                            e.Row.BaseOpenQty = INUnitAttribute.ConvertToBase(sender, e.Row.InventoryID, e.Row.UOM, (decimal)e.Row.OpenQty, e.Row.BaseOpenQty, INPrecision.QUANTITY);

                            //mimic behavior of Shipment Confirmation where at least one schedule will always be present for processed line
                            //but additional schedules will never be created and thus should be truncated when ShippedQty > 0
                            if (e.Row.Completed == true && e.OldRow.Completed == false)
                            {
                                CompleteSchedules(sender, e.Row);
                                UpdateParent(sender, e.Row);
                            }
                            else if (e.Row.Completed == false && e.OldRow.Completed == true)
                            {
                                UncompleteSchedules(sender, e.Row);
                                UpdateParent(sender, e.Row);
                            }
                            else if (e.Row.BaseOpenQty > e.OldRow.BaseOpenQty)
                            {
                                IssueAvailable(sender, e.Row, (decimal)e.Row.BaseOpenQty - (decimal)e.OldRow.BaseOpenQty);
                                UpdateParent(sender, e.Row);
                            }
                            else if (e.Row.BaseOpenQty < e.OldRow.BaseOpenQty)
                            {
                                TruncateSchedules(sender, e.Row, (decimal)e.OldRow.BaseOpenQty - (decimal)e.Row.BaseOpenQty);
                                UpdateParent(sender, e.Row);
                            }
                        }

                        if (!sender.ObjectsEqual<FSSODet.pOCreate, FSSODet.pOSource, FSSODet.vendorID, FSSODet.poVendorLocationID, FSSODet.pOSiteID, FSSODet.locationID, FSSODet.siteLocationID>(e.Row, e.OldRow))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetSplit split = detail as FSSODetSplit;
                                if (split.IsAllocated == false && split.Completed == false && split.PONbr == null)
                                {
                                    split.POCreate = row.POCreate;
                                    split.POSource = row.POSource;
                                    split.VendorID = row.VendorID;
                                    split.POSiteID = row.POSiteID;
                                    split.LocationID = row.SiteLocationID;

                                    if (row.SiteLocationID != null)
                                    {
                                        split.LocationID = row.SiteLocationID;
                                    }

                                    DetailCache.Update(split);
                                }
                            }
                        }

                        if (!sender.ObjectsEqual<FSSODet.shipDate>(e.Row, e.OldRow) ||
                            (!sender.ObjectsEqual<FSSODet.shipComplete>(e.Row, e.OldRow) && ((FSSODet)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetSplit split = detail as FSSODetSplit;
                                split.ShipDate = row.ShipDate;

                                DetailCache.Update(split);
                            }
                        }
                    }
                }

                AvailabilityCheck(sender, (FSSODet)e.Row);
            }
        }

        protected virtual bool SchedulesEqual(FSSODetSplit a, FSSODetSplit b)
        {
            if (a != null && b != null)
            {
                return (a.InventoryID == b.InventoryID
                        && a.SubItemID == b.SubItemID
                        && a.SiteID == b.SiteID
                        && a.ToSiteID == b.ToSiteID
                        && a.ShipDate == b.ShipDate
                        && a.IsAllocated == b.IsAllocated
                        && a.IsMergeable != false
                        && b.IsMergeable != false
                        && a.ShipmentNbr == b.ShipmentNbr
                        && a.Completed == b.Completed
                        && a.POCreate == b.POCreate
                        && a.POCompleted == b.POCompleted
                        && a.PONbr == b.PONbr
                        && a.POLineNbr == b.POLineNbr
                        && a.SOOrderType == b.SOOrderType
                        && a.SOOrderNbr == b.SOOrderNbr
                        && a.SOLineNbr == b.SOLineNbr
                        && a.SOSplitLineNbr == b.SOSplitLineNbr);
            }
            else
            {
                return (a != null);
            }
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            if (IsLSEntryEnabled)
            {
                if (e.ExternalCall)
                {
                    if (((FSSODetSplit)e.Row).LineType != SOLineType.Inventory)
                    {
                        throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
                    }
                }

                base.Detail_RowInserting(sender, e);

                if (e.Row != null && !IsLocationEnabled && ((FSSODetSplit)e.Row).LocationID != null)
                {
                    ((FSSODetSplit)e.Row).LocationID = null;
                }
            }
            else if (IsAllocationEntryEnabled)
            {
                FSSODetSplit a = (FSSODetSplit)e.Row;

                if (!e.ExternalCall && _Operation == PXDBOperation.Update)
                {
                    foreach (object item in SelectDetail(sender, (FSSODetSplit)e.Row))
                    {
                        FSSODetSplit detailitem = (FSSODetSplit)item;

                        if (SchedulesEqual((FSSODetSplit)e.Row, detailitem))
                        {
                            object old_item = PXCache<FSSODetSplit>.CreateCopy(detailitem);
                            detailitem.BaseQty += ((FSSODetSplit)e.Row).BaseQty;
                            detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

                            detailitem.BaseUnreceivedQty += ((FSSODetSplit)e.Row).BaseQty;
                            detailitem.UnreceivedQty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseUnreceivedQty, INPrecision.QUANTITY);

                            sender.Current = detailitem;
                            sender.RaiseRowUpdated(detailitem, old_item);

                            if (sender.GetStatus(detailitem) == PXEntryStatus.Notchanged)
                            {
                                sender.SetStatus(detailitem, PXEntryStatus.Updated);
                            }

                            e.Cancel = true;
                            break;
                        }
                    }
                }

                if (((FSSODetSplit)e.Row).InventoryID == null || string.IsNullOrEmpty(((FSSODetSplit)e.Row).UOM))
                {
                    e.Cancel = true;
                }
            }
        }

        protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
        {
            FSSODetSplit split = (FSSODetSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            SiteStatus accum = new SiteStatus();
            accum.InventoryID = split.InventoryID;
            accum.SiteID = split.SiteID;
            accum.SubItemID = split.SubItemID;

            accum = (SiteStatus)sender.Graph.Caches[typeof(SiteStatus)].Insert(accum);
            accum = PXCache<SiteStatus>.CreateCopy(accum);

            INSiteStatus stat = PXSelectReadonly<INSiteStatus, Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>, And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>, And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.SubItemID);
            if (stat != null)
            {
                accum.QtyAvail += stat.QtyAvail;
                accum.QtyHardAvail += stat.QtyHardAvail;
            }

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

            if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult))
            {
                if (split.LotSerialNbr != null)
                {
                    LotSerialNbr_Updated(sender, e);
                    return true;
                }
            }
            else
            {
                if (accum.QtyHardAvail < 0m)
                {
                    FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy(split);

                    if (split.BaseQty + accum.QtyHardAvail > 0m)
                    {
                        split.BaseQty += accum.QtyHardAvail;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<FSSODetSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
                    }

                    sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), split, copy.IsAllocated);
                    sender.RaiseRowUpdated(split, copy);

                    if (split.IsAllocated == true)
                    {
                        copy.SplitLineNbr = null;
                        copy.PlanID = null;
                        copy.IsAllocated = false;
                        copy.BaseQty = -accum.QtyHardAvail;
                        copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);

                        sender.Insert(copy);
                    }

                    RefreshView(sender);

                    return true;
                }
            }
            return false;
        }

        protected virtual bool LotSerialNbr_Updated(PXCache sender, EventArgs e)
        {

            FSSODetSplit split = (FSSODetSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            FSSODetSplit oldsplit = (FSSODetSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).OldRow : ((PXRowInsertedEventArgs)e).Row);

            SiteLotSerial accum = new SiteLotSerial();

            accum.InventoryID = split.InventoryID;
            accum.SiteID = split.SiteID;
            accum.LotSerialNbr = split.LotSerialNbr;

            accum = (SiteLotSerial)sender.Graph.Caches[typeof(SiteLotSerial)].Insert(accum);
            accum = PXCache<SiteLotSerial>.CreateCopy(accum);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

            INSiteLotSerial siteLotSerial = PXSelectReadonly<INSiteLotSerial, Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>, And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>,
                And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>.Select(sender.Graph, split.InventoryID, split.SiteID, split.LotSerialNbr);

            if (siteLotSerial != null)
            {
                accum.QtyAvail += siteLotSerial.QtyAvail;
                accum.QtyHardAvail += siteLotSerial.QtyHardAvail;
            }

            //Serial-numbered items
            if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
            {
                FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy(split);

                if (siteLotSerial != null && siteLotSerial.QtyAvail > 0 && siteLotSerial.QtyHardAvail > 0)
                {
                    if (split.Operation != SOOperation.Receipt)
                    {
                        split.BaseQty = 1;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        split.IsAllocated = true;
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<FSSODetSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(IN.Messages.SerialNumberAlreadyReceived, ((InventoryItem)item).InventoryCD, split.LotSerialNbr)));
                    }
                }
                else
                {

                    if (split.Operation != SOOperation.Receipt)
                    {
                        bool extCall = false;
                        if (e is PXRowUpdatedEventArgs) extCall = ((PXRowUpdatedEventArgs)e).ExternalCall;
                        if (e is PXRowInsertedEventArgs) extCall = ((PXRowInsertedEventArgs)e).ExternalCall;
                        if (extCall)
                        {
                            split.IsAllocated = false;
                            split.LotSerialNbr = null;
                            sender.RaiseExceptionHandling<FSSODetSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
                            if (split.IsAllocated == true)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        split.BaseQty = 1;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                    }
                }

                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), split, copy.IsAllocated);
                sender.RaiseRowUpdated(split, copy);

                if (copy.BaseQty - 1 > 0m)
                {
                    if (split.IsAllocated == true || (split.IsAllocated != true && split.Operation == SOOperation.Receipt))
                    {
                        copy.SplitLineNbr = null;
                        copy.PlanID = null;
                        copy.IsAllocated = false;
                        copy.LotSerialNbr = null;
                        copy.BaseQty -= 1;
                        copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
                        sender.Insert(copy);
                    }
                    RefreshView(sender);
                    return true;
                }
            }
            //Lot-numbered items
            else if (INLotSerialNbrAttribute.IsTrack(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null && !INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult))
            {
                if (split.BaseQty > 0m)
                {
                    //Lot/Serial Nbr. selected on non-allocated line. Trying to allocate line first. Verification of Qty. available for allocation will be performed on the next pass-through
                    if (split.IsAllocated == false)
                    {
                        if (siteLotSerial == null || (((siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail <= 0m) || siteLotSerial.QtyOnHand <= 0m) && split.Operation != SOOperation.Receipt))
                        {
                            return NegativeInventoryError(sender, split);
                        }

                        FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy(split);
                        split.IsAllocated = true;
                        sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), split, copy.IsAllocated);
                        sender.RaiseRowUpdated(split, copy);

                        return true;
                    }

                    //Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
                    if (split.IsAllocated == true)
                    {
                        FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy(split);
                        if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail >= 0m && split.Operation != SOOperation.Receipt)
                        {
                            split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        }
                        else if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail < 0m && split.Operation != SOOperation.Receipt)
                        {
                            split.BaseQty += accum.QtyHardAvail;
                            if (split.BaseQty <= 0m)
                            {
                                if (NegativeInventoryError(sender, split)) return false;
                            }
                            split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        }
                        else if (siteLotSerial == null || (siteLotSerial.QtyOnHand <= 0m && split.Operation != SOOperation.Receipt))
                        {
                            if (NegativeInventoryError(sender, split)) return false;
                        }

                        INItemPlanIDAttribute.RaiseRowUpdated(sender, split, copy);

                        if ((copy.BaseQty - split.BaseQty) > 0m && split.IsAllocated == true)
                        {
                            _InternallCall = true;
                            try
                            {
                                copy.SplitLineNbr = null;
                                copy.PlanID = null;
                                copy.IsAllocated = false;
                                copy.LotSerialNbr = null;
                                copy.BaseQty -= split.BaseQty;
                                copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, (decimal)copy.BaseQty, INPrecision.QUANTITY);
                                copy = (FSSODetSplit)sender.Insert(copy);
                                if (copy.LotSerialNbr != null && copy.IsAllocated != true)
                                {
                                    sender.SetValue<FSSODetSplit.lotSerialNbr>(copy, null);
                                }
                            }
                            finally
                            {
                                _InternallCall = false;
                            }
                        }
                        RefreshView(sender);

                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool NegativeInventoryError(PXCache sender, FSSODetSplit split)
        {
            split.IsAllocated = false;
            split.LotSerialNbr = null;
            sender.RaiseExceptionHandling<FSSODetSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));

            if (split.IsAllocated == true)
            {
                return true;
            }
            return false;
        }

        private void RefreshView(PXCache sender)
        {
            foreach (KeyValuePair<string, PXView> pair in sender.Graph.Views)
            {
                PXView view = pair.Value;

                if (view.IsReadOnly == false && view.GetItemType() == sender.GetItemType())
                {
                    view.RequestRefresh();
                }
            }
        }

        protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            base.Detail_RowInserted(sender, e);

            if ((_InternallCall == false || !string.IsNullOrEmpty(((FSSODetSplit)e.Row).LotSerialNbr) && ((FSSODetSplit)e.Row).IsAllocated != true) && IsAllocationEntryEnabled)
            {
                if (((FSSODetSplit)e.Row).IsAllocated == true || (!string.IsNullOrEmpty(((FSSODetSplit)e.Row).LotSerialNbr) && ((FSSODetSplit)e.Row).IsAllocated != true))
                {
                    Allocated_Updated(sender, e);

                    sender.RaiseExceptionHandling<FSSODetSplit.qty>(e.Row, null, null);
                    AvailabilityCheck(sender, (FSSODetSplit)e.Row);
                }
            }
        }

        protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Detail_RowUpdated(sender, e);

            if (_InternallCall == false && IsAllocationEntryEnabled)
            {
                if (!sender.ObjectsEqual<FSSODetSplit.isAllocated>(e.Row, e.OldRow) || !sender.ObjectsEqual<FSSODetSplit.pOLineNbr>(e.Row, e.OldRow) && ((FSSODetSplit)e.Row).POLineNbr == null && ((FSSODetSplit)e.Row).IsAllocated == false)
                {
                    if (((FSSODetSplit)e.Row).IsAllocated == true)
                    {
                        Allocated_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetSplit)e.Row);
                    }
                    else
                    {
                        //clear link to created transfer
                        FSSODetSplit row = (FSSODetSplit)e.Row;
                        row.SOOrderType = null;
                        row.SOOrderNbr = null;
                        row.SOLineNbr = null;
                        row.SOSplitLineNbr = null;

                        foreach (FSSODetSplit s in this.SelectDetailReversed(sender, (FSSODetSplit)e.Row))
                        {
                            if (s.SplitLineNbr != ((FSSODetSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetSplit)e.Row))
                            {
                                ((FSSODetSplit)e.Row).Qty += s.Qty;
                                ((FSSODetSplit)e.Row).BaseQty += s.BaseQty;

                                ((FSSODetSplit)e.Row).UnreceivedQty += s.Qty;
                                ((FSSODetSplit)e.Row).BaseUnreceivedQty += s.BaseQty;

                                if (((FSSODetSplit)e.Row).LotSerialNbr != null)
                                {
                                    FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy((FSSODetSplit)e.Row);
                                    ((FSSODetSplit)e.Row).LotSerialNbr = null;
                                    //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), s, copy.IsAllocated);
                                    sender.RaiseRowUpdated((FSSODetSplit)e.Row, copy);
                                }
                                sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCacheObsolete();

                                PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                                INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, ((FSSODetSplit)e.Row).PlanID);

                                if (plan != null)
                                {
                                    plan.PlanQty += s.BaseQty;
                                    if (cache.GetStatus(plan) != PXEntryStatus.Inserted)
                                    {
                                        cache.SetStatus(plan, PXEntryStatus.Updated);
                                    }
                                }

                                INItemPlan old_plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, s.PlanID);

                                if (old_plan != null)
                                {
                                    cache.SetStatus(old_plan, cache.GetStatus(old_plan) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                    cache.ClearQueryCacheObsolete();

                                }

                                RefreshView(sender);
                            }
                            else if (s.SplitLineNbr == ((FSSODetSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetSplit)e.Row) && ((FSSODetSplit)e.Row).LotSerialNbr != null)
                            {
                                FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy((FSSODetSplit)e.Row);
                                ((FSSODetSplit)e.Row).LotSerialNbr = null;
                                sender.RaiseRowUpdated((FSSODetSplit)e.Row, copy);
                            }
                        }
                    }
                }

                if (!sender.ObjectsEqual<FSSODetSplit.lotSerialNbr>(e.Row, e.OldRow))
                {
                    if (((FSSODetSplit)e.Row).LotSerialNbr != null)
                    {
                        LotSerialNbr_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetSplit)e.Row); //???
                    }
                    else
                    {
                        foreach (FSSODetSplit s in this.SelectDetailReversed(sender, (FSSODetSplit)e.Row))
                        {
                            if (s.SplitLineNbr == ((FSSODetSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetSplit)e.Row))
                            {
                                FSSODetSplit copy = PXCache<FSSODetSplit>.CreateCopy(s);
                                ((FSSODetSplit)e.Row).IsAllocated = false;
                                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), (FSSODetSplit)e.Row, ((FSSODetSplit)e.Row).IsAllocated);
                                //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetSplit.isAllocated)), s, copy.IsAllocated);
                                sender.RaiseRowUpdated(s, copy);
                            }
                        }
                    }
                }

            }
        }

        public override void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

            if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
            {
                e.NewValue = ((InventoryItem)item).BaseUnit;
                e.Cancel = true;
            }
            else if (!IsAllocationEntryEnabled)
            {
                base.Detail_UOM_FieldDefaulting(sender, e);
            }
        }

        public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (!IsAllocationEntryEnabled)
            {
                base.Detail_Qty_FieldVerifying(sender, e);
            }
            else
            {
                VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(FSSODetSplit.qty).Name);
            }
        }



        public override FSSODetSplit Convert(FSSODet item)
        {
            using (InvtMultScope<FSSODet> ms = new InvtMultScope<FSSODet>(item))
            {
                FSSODetSplit ret = (FSSODetSplit)item;
                //baseqty will be overriden in all cases but AvailabilityFetch
                ret.BaseQty = item.BaseQty - item.UnassignedQty;

                return ret;
            }
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data)
            where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(Field).Name)))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, typeof(Field).Name);
            }
        }

        public virtual void FSSODetSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            FSSODetSplit split = e.Row as FSSODetSplit;

            if (split != null)
            {
                bool isLineTypeInventory = (split.LineType == SOLineType.Inventory);
                object val = sender.GetValueExt<FSSODetSplit.isAllocated>(e.Row);
                bool isAllocated = split.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
                bool isCompleted = split.Completed == true;
                bool isIssue = split.Operation == SOOperation.Issue;
                bool IsLinked = split.PONbr != null || split.SOOrderNbr != null && split.IsAllocated == true;

                FSSODet parent = (FSSODet)PXParentAttribute.SelectParent(sender, split, typeof(FSSODet));

                PXUIFieldAttribute.SetEnabled<FSSODetSplit.subItemID>(sender, e.Row, isLineTypeInventory);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.completed>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.shippedQty>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.shipmentNbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.isAllocated>(sender, e.Row, isLineTypeInventory && isIssue && !isCompleted);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.siteID>(sender, e.Row, isLineTypeInventory && isAllocated && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.qty>(sender, e.Row, !isCompleted && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.shipDate>(sender, e.Row, !isCompleted && parent?.ShipComplete == SOShipComplete.BackOrderAllowed);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.pONbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetSplit.pOReceiptNbr>(sender, e.Row, false);

                if (split.Completed == true)
                {
                    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                }
            }
        }

        public virtual void FSSODetSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                bool RequireLocationAndSubItem = ((FSSODetSplit)e.Row).RequireLocation == true && ((FSSODetSplit)e.Row).IsStockItem == true && ((FSSODetSplit)e.Row).BaseQty != 0m;

                PXDefaultAttribute.SetPersistingCheck<FSSODetSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSODetSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            }
        }

        public virtual void FSSODetSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODet)];

            if (cache.Current != null && (e.Row == null || ((FSSODet)cache.Current).LineNbr == ((FSSODetSplit)e.Row).LineNbr && ((FSSODetSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSSODet)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void FSSODetSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODet)];
            FSSODet parentRow = cache.Current as FSSODet;
            FSSODetSplit splitRow = e.Row as FSSODetSplit;

            if (cache.Current != null && parentRow.LocationID != null && (e.Row == null || (parentRow.LineNbr == splitRow.LineNbr && splitRow.IsStockItem == true)))
            {
                e.NewValue = ((FSSODet)cache.Current).LocationID;
                e.NewValue = parentRow.LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null || !IsLocationEnabled);
            }
        }

        public virtual void FSSODetSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODet)];

            if (cache.Current != null && (e.Row == null || ((FSSODet)cache.Current).LineNbr == ((FSSODetSplit)e.Row).LineNbr))
            {
                using (InvtMultScope<FSSODet> ms = new InvtMultScope<FSSODet>((FSSODet)cache.Current))
                {
                    e.NewValue = ((FSSODet)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
        {
            if (row is FSSODet)
            {
                sender.RaiseExceptionHandling<FSSODet.orderQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODet.inventoryID>(row), sender.GetStateExt<FSSODet.subItemID>(row), sender.GetStateExt<FSSODet.siteID>(row), sender.GetStateExt<FSSODet.locationID>(row), sender.GetValue<FSSODet.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<FSSODetSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODetSplit.inventoryID>(row), sender.GetStateExt<FSSODetSplit.subItemID>(row), sender.GetStateExt<FSSODetSplit.siteID>(row), sender.GetStateExt<FSSODetSplit.locationID>(row), sender.GetValue<FSSODetSplit.lotSerialNbr>(row)));
            }
        }

        public virtual bool MemoAvailabilityCheck(PXCache sender, FSSODet Row)
        {
            bool success = MemoAvailabilityCheckQty(sender, Row);

            return success;
        }

        protected virtual bool MemoAvailabilityCheckQty(PXCache sender, FSSODet row)
        {
            return true;
        }

        protected virtual bool MemoAvailabilityCheck<Target>(PXCache sender, FSSODet Row, ILSMaster Split)
            where Target : class, ILSMaster
        {
            bool success = true;
            return success;
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
        {
            base.AvailabilityCheck(sender, Row);

            if (Row is FSSODet)
            {
                MemoAvailabilityCheck(sender, (FSSODet)Row);

                FSSODetSplit copy = Convert(Row as FSSODet);

                if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                {
                    DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetSplit)], copy);
                }

                MemoAvailabilityCheck<FSSODet>(sender, (FSSODet)Row, copy);

                if (copy.LotSerialNbr == null)
                {
                    Row.LotSerialNbr = null;
                }
            }
            else
            {
                object parent = PXParentAttribute.SelectParent(sender, Row, typeof(FSSODet));
                MemoAvailabilityCheck(sender.Graph.Caches[typeof(FSSODet)], (FSSODet)parent);
                MemoAvailabilityCheck<FSSODetSplit>(sender.Graph.Caches[typeof(FSSODet)], (FSSODet)parent, Row);
            }
        }

        public override void DefaultLotSerialNbr(PXCache sender, FSSODetSplit row)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

            if (item != null)
            {
                if (IsAllocationEntryEnabled && ((INLotSerClass)item).LotSerAssign == INLotSerAssign.WhenUsed)
                    return;
                else
                    base.DefaultLotSerialNbr(sender, row);
            }
        }
        #endregion

        public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, FSSODet row, PXResult<InventoryItem, INLotSerClass> item)
        {
            PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus,
                                                      InnerJoin<INLocation,
                                                      On<
                                                          INLocation.locationID, Equal<INLotSerialStatus.locationID>>>,
                                                      Where<
                                                          INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
                                                      And<
                                                          INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
                                                      And<
                                                          INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);

            if (!IsLocationEnabled && IsLotSerialRequired)
            {
                cmd = new PXSelectJoin<INLotSerialStatus,
                          InnerJoin<INLocation,
                          On<
                              INLocation.locationID, Equal<INLotSerialStatus.locationID>>,
                          InnerJoin<INSiteLotSerial,
                          On<
                              INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
                              And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>,
                              And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
                          Where<
                              INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
                          And<
                              INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
                          And<
                              INLotSerialStatus.qtyOnHand, Greater<decimal0>,
                          And<
                              INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>(sender.Graph);
            }

            if (row.SubItemID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
            }
            if (row.LocationID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
            }
            else
            {
                switch (row.TranType)
                {
                    case INTranType.Transfer:
                        cmd.WhereAnd<Where<INLocation.transfersValid, Equal<True>>>();
                        break;
                    default:
                        cmd.WhereAnd<Where<INLocation.salesValid, Equal<True>>>();
                        break;
                }
            }

            switch (((INLotSerClass)item).LotSerIssueMethod)
            {
                case INLotSerIssueMethod.FIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.LIFO:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Desc<INLotSerialStatus.receiptDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Expiration:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.expireDate, Asc<INLotSerialStatus.lotSerialNbr>>>>>();
                    break;
                case INLotSerIssueMethod.Sequential:
                    cmd.OrderByNew<OrderBy<Asc<INLocation.pickPriority, Asc<INLotSerialStatus.lotSerialNbr>>>>();
                    break;
                case INLotSerIssueMethod.UserEnterable:
                    cmd.WhereAnd<Where<boolTrue, Equal<False>>>();
                    break;
                default:
                    throw new PXException();
            }

            return cmd;
        }

        public virtual bool IsLotSerialItem(PXCache sender, ILSMaster line)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, line.InventoryID);

            if (item == null)
                return false;

            return INLotSerialNbrAttribute.IsTrack(item, line.TranType, line.InvtMult);
        }
    }

    public class FSSODetSplitPlanIDAttribute : INItemPlanIDAttribute
    {
        #region State
        protected Type _ParentOrderDate;
        #endregion
        #region Ctor
        public FSSODetSplitPlanIDAttribute(Type parentNoteID, Type parentHoldEntry, Type parentOrderDate)
            : base(parentNoteID, parentHoldEntry)
        {
            _ParentOrderDate = parentOrderDate;
        }
        #endregion
        #region Implementation
        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            sender.Graph.FieldDefaulting.AddHandler<SiteStatus.negAvailQty>(SiteStatus_NegAvailQty_FieldDefaulting);
        }

        protected virtual void SiteStatus_NegAvailQty_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);

            if (e.Cancel == false && ordertype != null && ordertype.RequireAllocation == true)
            {
                e.NewValue = false;
                e.Cancel = true;
            }
        }

        public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Parent_RowUpdated(sender, e);

            PXView view;
            WebDialogResult answer = sender.Graph.Views.TryGetValue("Document", out view) ? view.Answer : WebDialogResult.None;

            //bool DatesUpdated = !sender.ObjectsEqual<FSServiceOrder.shipDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);
            //bool RequestOnUpdated = !sender.ObjectsEqual<FSServiceOrder.requestDate>(e.Row, e.OldRow) && (answer == WebDialogResult.Yes || ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed);
            //bool CreditHoldApprovedUpdated = !sender.ObjectsEqual<FSServiceOrder.creditHold>(e.Row, e.OldRow) || !sender.ObjectsEqual<FSServiceOrder.approved>(e.Row, e.OldRow);
            bool CustomerUpdated = !sender.ObjectsEqual<FSServiceOrder.billCustomerID>(e.Row, e.OldRow);
            FSBillingCycle billingCycleRow = ((ServiceOrderEntry)sender.Graph).BillingCycleRelated.Current;


            if (CustomerUpdated
                || !sender.ObjectsEqual<FSServiceOrder.hold, FSServiceOrder.status>(e.Row, e.OldRow))
            {
                //DatesUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;
                //RequestOnUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

                bool cancelled = (string)sender.GetValue<FSServiceOrder.status>(e.Row) == ID.Status_ServiceOrder.CANCELED;
                //bool? BackOrdered = (bool?)sender.GetValue<FSServiceOrder.backOrdered>(e.Row);

                PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
                PXCache fsSODetCache = sender.Graph.Caches[typeof(FSSODet)];
                PXCache splitcache = sender.Graph.Caches[typeof(FSSODetSplit)];

                SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);

                var splitsByPlan = new Dictionary<long?, FSSODetSplit>();

                foreach (FSSODetSplit split in PXSelect<FSSODetSplit,
                                               Where<
                                                   FSSODetSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                               And<
                                                   FSSODetSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                                               .SelectMultiBound(sender.Graph, new[] { e.Row }))
                {

                    FSSODet soDet = PXSelect<FSSODet,
                                               Where<
                                                   FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
                                               And<
                                                   FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                                                And<
                                                    FSSODet.lineNbr, Equal<Required<FSSODet.refNbr>>>>>>
                                               .Select(sender.Graph, split.SrvOrdType, split.RefNbr, split.LineNbr);

                    if (cancelled)
                    {
                        plancache.Inserted.RowCast<INItemPlan>()
                                          .Where(_ => _.PlanID == split.PlanID)
                                          .ForEach(_ => plancache.Delete(_));

                        split.PlanID = null;
                        split.Completed = true;

                        splitcache.MarkUpdated(split);
                    }
                    else
                    {
                        if ((string)sender.GetValue<FSServiceOrder.status>(e.OldRow) == ID.Status_ServiceOrder.CANCELED)
                        {
                            if (string.IsNullOrEmpty(split.ShipmentNbr) 
                                    && split.POCompleted == false)
                            {
                                split.Completed = false;
                            }

                            INItemPlan plan = DefaultValues(splitcache, split);

                            if (plan != null)
                            {
                                plan = (INItemPlan)sender.Graph.Caches[typeof(INItemPlan)].Insert(plan);
                                split.PlanID = plan.PlanID;
                            }

                            splitcache.MarkUpdated(split);
                        }

                        if ((string)sender.GetValue<FSServiceOrder.status>(e.OldRow) == ID.Status_ServiceOrder.CLOSED)
                        {
                            if (string.IsNullOrEmpty(split.ShipmentNbr)
                                    && split.POCompleted == false
                                    && split.Completed == true
                                    && split.LastModifiedByScreenID == ID.ScreenID.SERVICE_ORDER
                                    && billingCycleRow.BillingBy == ID.Billing_By.APPOINTMENT)
                            {
                                soDet.BaseShippedQty -= split.BaseShippedQty;
                                soDet.ShippedQty -= split.ShippedQty;
                                soDet.OpenQty = soDet.OrderQty - soDet.ShippedQty;
                                soDet.BaseOpenQty = soDet.BaseOrderQty - soDet.BaseShippedQty;
                                soDet.ClosedQty = soDet.ShippedQty;
                                soDet.BaseClosedQty = soDet.BaseShippedQty;

                                fsSODetCache.MarkUpdated(soDet);

                                split.Completed = false;
                                split.ShippedQty = 0;

                                INItemPlan plan = DefaultValues(splitcache, split);
                                if (plan != null)
                                {
                                    plan = (INItemPlan)sender.Graph.Caches[typeof(INItemPlan)].Insert(plan);
                                    split.PlanID = plan.PlanID;
                                }

                                splitcache.MarkUpdated(split);
                            }
                        }

                        if (split.PlanID != null)
                        {
                            splitsByPlan[split.PlanID] = split;
                        }
                    }
                }

                PXCache linecache = sender.Graph.Caches[typeof(FSSODet)];

                foreach (FSSODet line in PXSelect<FSSODet,
                                         Where<
                                             FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                         And<
                                             FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                                         .SelectMultiBound(sender.Graph, new[] { e.Row }))
                {
                    if (cancelled)
                    {
                        FSSODet old_row = PXCache<FSSODet>.CreateCopy(line);
                        //line.UnbilledQty -= line.OpenQty;
                        line.OpenQty = 0m;
                        //linecache.RaiseFieldUpdated<FSSODet.unbilledQty>(line, 0m);
                        linecache.RaiseFieldUpdated<FSSODet.openQty>(line, 0m);

                        line.Completed = true;
                        LSFSSODetLine.ResetAvailabilityCounters(line);

                        //SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
                        //+++//TaxAttribute.Calculate<FSSODet.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

                        linecache.MarkUpdated(line);
                    }
                    else
                    {
                        if ((string)sender.GetValue<FSServiceOrder.status>(e.OldRow) == ID.Status_ServiceOrder.CANCELED)
                        {
                            FSSODet old_row = PXCache<FSSODet>.CreateCopy(line);
                            line.OpenQty = line.OrderQty;
                            /*line.UnbilledQty += line.OpenQty;
                            object value = line.UnbilledQty;
                            linecache.RaiseFieldVerifying<FSSODet.unbilledQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODet.unbilledQty>(line, value);*/

                            object value = line.OpenQty;
                            linecache.RaiseFieldVerifying<FSSODet.openQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODet.openQty>(line, value);

                            line.Completed = false;

                            //+++++//
                            //TaxAttribute.Calculate<FSSODet.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

                            linecache.MarkUpdated(line);
                        }
                        /*if (DatesUpdated)
                        {
                            line.ShipDate = (DateTime?)sender.GetValue<FSServiceOrder.shipDate>(e.Row);
                            linecache.SmartSetStatus(line, PXEntryStatus.Updated);
                        }
                        if (RequestOnUpdated)
                        {
                            line.RequestDate = (DateTime?)sender.GetValue<FSServiceOrder.requestDate>(e.Row);
                            linecache.SmartSetStatus(line, PXEntryStatus.Updated);
                        }*/
                        if (/*CreditHoldApprovedUpdated ||*/ !sender.ObjectsEqual<FSServiceOrder.hold>(e.Row, e.OldRow))
                        {
                            LSFSSODetLine.ResetAvailabilityCounters(line);
                        }
                    }
                }

                if (cancelled)
                {
                    //PXFormulaAttribute.CalcAggregate<FSSODet.unbilledQty>(linecache, e.Row);
                    PXFormulaAttribute.CalcAggregate<FSSODet.openQty>(linecache, e.Row);
                }

                PXSelectBase<INItemPlan> cmd = new PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<FSServiceOrder.noteID>>>>(sender.Graph);

                //BackOrdered is tri-state
                /*if (BackOrdered == true && sender.GetValue<FSServiceOrder.lastSiteID>(e.Row) != null && sender.GetValue<FSServiceOrder.lastShipDate>(e.Row) != null)
                {
                    cmd.WhereAnd<Where<INItemPlan.siteID, Equal<Current<FSServiceOrder.lastSiteID>>, And<INItemPlan.planDate, LessEqual<Current<FSServiceOrder.lastShipDate>>>>>();
                }

                if (BackOrdered == false)
                {
                    sender.SetValue<FSServiceOrder.lastSiteID>(e.Row, null);
                    sender.SetValue<FSServiceOrder.lastShipDate>(e.Row, null);
                }*/

                foreach (INItemPlan plan in cmd.View.SelectMultiBound(new[] { e.Row }))
                {
                    if (cancelled)
                    {
                        plancache.Delete(plan);
                    }
                    else
                    {
                        INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

                        /*if (DatesUpdated)
                        {
                            plan.PlanDate = (DateTime?)sender.GetValue<FSServiceOrder.shipDate>(e.Row);
                        }*/
                        if (CustomerUpdated)
                        {
                            plan.BAccountID = (int?)sender.GetValue<FSServiceOrder.customerID>(e.Row);
                        }
                        plan.Hold = IsOrderOnHold((FSServiceOrder)e.Row);

                        FSSODetSplit split;

                        if (splitsByPlan.TryGetValue(plan.PlanID, out split))
                        {
                            plan.PlanType = CalcPlanType(sender, plan, (FSServiceOrder)e.Row, split/*, BackOrdered*/);

                            if (!string.Equals(copy.PlanType, plan.PlanType))
                            {
                                plancache.RaiseRowUpdated(plan, copy);
                            }
                        }

                        if (plancache.GetStatus(plan).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Held))
                        {
                            plancache.SetStatus(plan, PXEntryStatus.Updated);
                        }
                    }
                }
                // FSServiceOrder.BackOrdered value should be handled only single time and only in this method
                // sender.SetValue<FSServiceOrder.backOrdered>(e.Row, null);
            }
        }

        bool initPlan = false;
        bool initVendor = false;
        bool resetSupplyPlanID = false;

        public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //respond only to GUI operations
            var isLinked = IsLineLinked((FSSODetSplit)e.Row);

            initPlan = InitPlanRequired(sender, e) && !isLinked;

            FSSODet parent = (FSSODet)PXParentAttribute.SelectParent(sender, e.Row, typeof(FSSODet));

            initVendor = !sender.ObjectsEqual<FSSODetSplit.siteID, FSSODetSplit.subItemID, FSSODetSplit.vendorID, FSSODetSplit.pOCreate>(e.Row, e.OldRow) && !isLinked;

            initVendor = initVendor || parent.POVendorLocationID != null;

            resetSupplyPlanID = !isLinked;

            try
            {
                base.RowUpdated(sender, e);
            }
            finally
            {
                initPlan = false;
                resetSupplyPlanID = false;
            }
        }

        protected virtual bool InitPlanRequired(PXCache cache, PXRowUpdatedEventArgs e)
        {
            return !cache
                .ObjectsEqual<FSSODetSplit.isAllocated,
                    FSSODetSplit.siteID,
                    FSSODetSplit.pOCreate,
                    FSSODetSplit.pOSource,
                    FSSODetSplit.operation>(e.Row, e.OldRow);
        }

        protected virtual bool IsLineLinked(FSSODetSplit soLineSplit)
        {
            return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan planRow, object origRow)
        {
            if (((FSSODetSplit)origRow).Completed == true || ((FSSODetSplit)origRow).POCompleted == true || ((FSSODetSplit)origRow).LineType == SOLineType.MiscCharge || ((FSSODetSplit)origRow).LineType == SOLineType.NonInventory && ((FSSODetSplit)origRow).RequireShipping == false)
            {
                return null;
            }

            FSSODet parent = (FSSODet)PXParentAttribute.SelectParent(sender, origRow, typeof(FSSODet));
            FSServiceOrder order = (FSServiceOrder)PXParentAttribute.SelectParent(sender, origRow, typeof(FSServiceOrder));

            FSSODetSplit split_Row = (FSSODetSplit)origRow;

            if (string.IsNullOrEmpty(planRow.PlanType) || initPlan)
            {
                planRow.PlanType = CalcPlanType(sender, planRow, order, split_Row);

                if (split_Row.POCreate == true)
                {
                    planRow.FixedSource = INReplenishmentSource.Purchased;

                    if (split_Row.POType != PO.POOrderType.Blanket && split_Row.POType != PO.POOrderType.DropShip && split_Row.POSource == INReplenishmentSource.PurchaseToOrder)
                        planRow.SourceSiteID = split_Row.SiteID;
                    else
                        planRow.SourceSiteID = split_Row.SiteID;
                }
                else
                {
                    planRow.Reverse = (split_Row.Operation == SOOperation.Receipt);
                    planRow.FixedSource = (split_Row.SiteID != split_Row.ToSiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None);
                    planRow.SourceSiteID = split_Row.SiteID;
                }
            }

            if (resetSupplyPlanID)
            {
                planRow.SupplyPlanID = null;
            }

            planRow.VendorID = split_Row.VendorID;

            if (initVendor || split_Row.POCreate == true && planRow.VendorID != null && planRow.VendorLocationID == null)
            {
                planRow.VendorLocationID = parent?.POVendorLocationID;

                if (planRow.VendorLocationID == null)
                {
                    planRow.VendorLocationID = PO.POItemCostManager.FetchLocation(sender.Graph,
                                                                                  split_Row.VendorID,
                                                                                  split_Row.InventoryID,
                                                                                  split_Row.SubItemID,
                                                                                  split_Row.SiteID);
                }
            }

            planRow.BAccountID = parent == null ? null : parent.BillCustomerID;
            planRow.InventoryID = split_Row.InventoryID;
            planRow.SubItemID = split_Row.SubItemID;
            planRow.SiteID = split_Row.SiteID;
            planRow.LocationID = split_Row.LocationID;
            planRow.LotSerialNbr = split_Row.LotSerialNbr;

            if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
            {
                planRow.LotSerialNbr = null;
            }

            planRow.PlanDate = split_Row.ShipDate;
            planRow.PlanQty = (split_Row.POCreate == true ? split_Row.BaseUnreceivedQty - split_Row.BaseShippedQty : split_Row.BaseQty);

            PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
            planRow.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
            planRow.Hold = IsOrderOnHold(order);

            if (string.IsNullOrEmpty(planRow.PlanType))
            {
                return null;
            }

            return planRow;
        }

        protected virtual bool IsOrderOnHold(FSServiceOrder order)
        {
            return (order != null) && ((order.Hold ?? false)) /*|| (order.CreditHold ?? false) || (!order.Approved ?? false))*/;
        }

        protected virtual string CalcPlanType(PXCache sender, INItemPlan plan, FSServiceOrder order, FSSODetSplit split, bool? backOrdered = null)
        {
            if (split.POCreate == true)
            {
                return INPlanConstants.PlanF6;
            }

            SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);
            bool isAllocation = (split.IsAllocated == true) || INPlanConstants.IsAllocated(plan.PlanType) || INPlanConstants.IsFixed(plan.PlanType);
            bool isOrderOnHold = IsOrderOnHold(order) && ordertype.RequireAllocation != true;

            string calcedPlanType = CalcPlanType(plan, split, ordertype, isOrderOnHold);
            bool putOnSOPrepared = (calcedPlanType == INPlanConstants.PlanF0);

            if (!initPlan && !putOnSOPrepared && !isAllocation)
            {
                if (backOrdered == true || backOrdered == null && plan.PlanType == INPlanConstants.Plan68)
                {
                    return INPlanConstants.Plan68;
                }
            }

            return calcedPlanType;
        }

        protected virtual string CalcPlanType(INItemPlan plan, FSSODetSplit split, SOOrderType ordertype, bool isOrderOnHold)
        {
            if (ordertype == null || ordertype.RequireShipping == true)
            {
                return (split.IsAllocated == true) ? split.AllocatedPlanType
                    : isOrderOnHold ? INPlanConstants.PlanF0
                    : (split.RequireAllocation != true || split.IsStockItem != true) ? split.PlanType : split.BackOrderPlanType;
            }
            else
            {
                return (isOrderOnHold != true || split.IsStockItem != true) ? split.PlanType : INPlanConstants.PlanF0;
            }
        }
        #endregion
    }
}
