using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common.Exceptions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;

namespace PX.Objects.FS
{
    public class LSFSSODetPartLine : LSSelectSOBase<FSSODetPart, FSSODetPartSplit,
        Where<FSSODetPartSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
        And<FSSODetPartSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>,
        And<FSSODetPart.lineType, Equal<FSSODetPart.lineType.Inventory_Item>>>>>
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
        public LSFSSODetPartLine(PXGraph graph)
            : base(graph)
        {
            MasterQtyField = typeof(FSSODetPart.orderQty);
            graph.FieldDefaulting.AddHandler<FSSODetPartSplit.subItemID>(FSSODetPartSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetPartSplit.locationID>(FSSODetPartSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetPartSplit.invtMult>(FSSODetPartSplit_InvtMult_FieldDefaulting);
            graph.RowSelected.AddHandler<FSServiceOrder>(Parent_RowSelected);
            graph.RowUpdated.AddHandler<FSServiceOrder>(FSServiceOrder_RowUpdated);
            graph.RowSelected.AddHandler<FSSODetPartSplit>(FSSODetPartSplit_RowSelected);
            graph.RowPersisting.AddHandler<FSSODetPartSplit>(FSSODetPartSplit_RowPersisting);
        }
        #endregion

        #region Implementation
        public override IEnumerable BinLotSerial(PXAdapter adapter)
        {
            if (IsLSEntryEnabled || IsAllocationEntryEnabled)
            {
                if (MasterCache.Current != null && ((IsLSEntryEnabled && ((FSSODetPart)MasterCache.Current).SOLineType != SOLineType.Inventory) || ((FSSODetPart)MasterCache.Current).SOLineType == SOLineType.MiscCharge))
                {
                    throw new PXSetPropertyException(PX.Objects.SO.Messages.BinLotSerialInvalid);
                }

                View.AskExt(true);
            }
            return adapter.Get();
        }

        protected virtual void FSServiceOrder_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (IsLSEntryEnabled && !sender.ObjectsEqual<FSServiceOrder.hold>(e.Row, e.OldRow) && (bool?)sender.GetValue<FSServiceOrder.hold>(e.Row) == false)
            {
                PXCache cache = sender.Graph.Caches[typeof(FSSODetPart)];

                foreach (FSSODetPart item in PXParentAttribute.SelectSiblings(cache, null, typeof(FSServiceOrder)))
                {
                    if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
                    {
                        cache.RaiseExceptionHandling<FSSODetPart.orderQty>(item, item.Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned));

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
                var row = (FSSODetPart)e.Row;
                if ((row.SOLineType == SOLineType.Inventory || row.SOLineType == SOLineType.NonInventory && row.InvtMult == (short)-1) && row.TranType != INTranType.NoUpdate && row.BaseQty < 0m)
                {
                    if (sender.RaiseExceptionHandling<FSSODetPart.orderQty>(e.Row, ((FSSODetPart)e.Row).Qty, new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString())))
                    {
                        throw new PXRowPersistingException(typeof(FSSODetPart.orderQty).Name, ((FSSODetPart)e.Row).Qty, CS.Messages.Entry_GE, ((int)0).ToString());
                    }
                    return;
                }

                if (IsLSEntryEnabled)
                {
                    PXCache cache = sender.Graph.Caches[typeof(FSServiceOrder)];
                    object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(FSServiceOrder)) ?? cache.Current;

                    bool? OnHold = (bool?)cache.GetValue<FSServiceOrder.hold>(doc);

                    if (OnHold == false && Math.Abs((decimal)((FSSODetPart)e.Row).BaseQty) >= 0.0000005m && (((FSSODetPart)e.Row).UnassignedQty >= 0.0000005m || ((FSSODetPart)e.Row).UnassignedQty <= -0.0000005m))
                    {
                        if (sender.RaiseExceptionHandling<FSSODetPart.orderQty>(e.Row, ((FSSODetPart)e.Row).Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned)))
                        {
                            throw new PXRowPersistingException(typeof(FSSODetPart.orderQty).Name, ((FSSODetPart)e.Row).Qty, SO.Messages.BinLotSerialNotAssigned);
                        }
                    }
                }
            }

            //for normal orders there are only when received numbers which do not require any additional processing
            if (!IsLSEntryEnabled)
            {
                if (((FSSODetPart)e.Row).TranType == INTranType.Transfer && DetailCounters.ContainsKey((FSSODetPart)e.Row))
                {
                    //keep Counters when adding splits to Transfer order
                    DetailCounters[(FSSODetPart)e.Row].UnassignedNumber = 0;
                }
                else
                {
                    DetailCounters[(FSSODetPart)e.Row] = new Counters { UnassignedNumber = 0 };
                }
            }

            base.Master_RowPersisting(sender, e);
        }

        protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
        {
            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, null, typeof(FSSODetPart.inventoryID).Name, InventoryID);

            if (item != null)
            {
                INLotSerClass lsclass = PXSelectReadonly<INLotSerClass, Where<INLotSerClass.lotSerClassID, Equal<Required<INLotSerClass.lotSerClassID>>>>.Select(sender.Graph, item.LotSerClassID);

                return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
            }

            return null;
        }

        public override IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
        {
            if (Row != null)
            {
                FSSODetPartSplit copy = Row as FSSODetPartSplit;
                if (copy == null)
                {
                    copy = Convert(Row as FSSODetPart);

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODetPart), Row);

                    if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                    {
                        DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetPartSplit)], copy);
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
                    {
                        if (_detailsRequested++ == 5)
                        {
                            foreach (PXResult<FSSODetPart, INUnit, INSiteStatus> res in 
                                PXSelectReadonly2<
                                    FSSODetPart, 
                                    InnerJoin<INUnit, On<INUnit.inventoryID, Equal<FSSODetPart.inventoryID>, And<INUnit.fromUnit, Equal<FSSODetPart.uOM>>>,
                                    InnerJoin<INSiteStatus, On<FSSODetPart.inventoryID, Equal<INSiteStatus.inventoryID>, And<FSSODetPart.subItemID, Equal<INSiteStatus.subItemID>, And<FSSODetPart.siteID, Equal<INSiteStatus.siteID>>>>>>,
                                Where<
                                    FSSODetPart.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>, 
                                    And<FSSODetPart.refNbr, Equal<Current<FSServiceOrder.refNbr>>,
                                    And<FSSODetPart.lineType, Equal<FSSODetPart.lineType.Inventory_Item>>>>>.Select(sender.Graph))
                            {
                                INSiteStatus status = res;
                                INUnit unit = res;

                                PXSelectReadonly<INUnit,
                                    Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
                                    And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
                                    And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>,
                                    And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>>.StoreCached(sender.Graph, new PXCommandKey(new object[] { unit.InventoryID, unit.ToUnit, unit.FromUnit }), new List<object> { unit });

                                PXSelectReadonly<INSiteStatus,
                                    Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                                    And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                                    And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>>.StoreCached(sender.Graph, new PXCommandKey(new object[] { status.InventoryID, status.SubItemID, status.SiteID }), new List<object> { status });
                            }

                            foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<FSServiceOrder.noteID>>>>.Select(this._Graph))
                            {
                                PXSelect<INItemPlan,
                                    Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.StoreCached(this._Graph, new PXCommandKey(new object[] { plan.PlanID }), new List<object> { plan });
                            }
                        }

                        IQtyAllocated result = AvailabilityFetch(sender, copy, AvailabilityFetchMode.None);
                        return DeductAllocated(sender, (FSSODetPart)Row, result);
                    }
                }

                return AvailabilityFetch(sender, copy, fetchMode);
            }
            return null;
        }

        public virtual IQtyAllocated DeductAllocated(PXCache sender, FSSODetPart soLine, IQtyAllocated result)
        {
            if (result == null) return null;
            decimal? lineQtyAvail = (decimal?)sender.GetValue<FSSODetPart.lineQtyAvail>(soLine);
            decimal? lineQtyHardAvail = (decimal?)sender.GetValue<FSSODetPart.lineQtyHardAvail>(soLine);

            if (lineQtyAvail == null || lineQtyHardAvail == null)
            {
                lineQtyAvail = 0m;
                lineQtyHardAvail = 0m;

                foreach (FSSODetPartSplit split in SelectDetail(DetailCache, soLine))
                {
                    FSSODetPartSplit copy = split;
                    if (copy.PlanID != null)
                    {
                        INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, copy.PlanID);
                        if (plan != null)
                        {
                            copy = PXCache<FSSODetPartSplit>.CreateCopy(copy);
                            copy.PlanType = plan.PlanType;
                        }
                    }

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODetPart), soLine);

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

                sender.SetValue<FSSODetPart.lineQtyAvail>(soLine, lineQtyAvail);
                sender.SetValue<FSSODetPart.lineQtyHardAvail>(soLine, lineQtyHardAvail);
            }

            result.QtyAvail += lineQtyAvail;
            result.QtyHardAvail += lineQtyHardAvail;
            result.QtyNotAvail = -lineQtyAvail;

            return result;
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            IQtyAllocated availability = AvailabilityFetch(sender, (FSSODetPart)e.Row, !(e.Row != null && (((FSSODetPart)e.Row).Completed == true)) ? AvailabilityFetchMode.ExcludeCurrent : AvailabilityFetchMode.None);

            if (availability != null)
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((FSSODetPart)e.Row).InventoryID);

                decimal unitRate = INUnitAttribute.ConvertFromBase<FSSODetPart.inventoryID, FSSODetPart.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
                availability.QtyOnHand = PXDBQuantityAttribute.Round((decimal)availability.QtyOnHand * unitRate);
                availability.QtyAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyAvail * unitRate);
                availability.QtyNotAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyNotAvail * unitRate);
                availability.QtyHardAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyHardAvail * unitRate);

                if (IsAllocationEntryEnabled)
                {
                    Decimal? allocated = PXDBQuantityAttribute.Round((decimal)(((FSSODetPart)e.Row).LineQtyHardAvail ?? 0m) * unitRate); ;
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_AllocatedInfo,
                            sender.GetValue<FSSODetPart.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
                }
                else
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_Info,
                            sender.GetValue<FSSODetPart.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));


                AvailabilityCheck(sender, (FSSODetPart)e.Row, availability);
            }
            else
            {
                //handle missing UOM
                INUnitAttribute.ConvertFromBase<FSSODetPart.inventoryID, FSSODetPart.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

        protected FSServiceOrder _LastSelected;

        protected virtual void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (_LastSelected == null || !object.ReferenceEquals(_LastSelected, e.Row))
            {
                PXUIFieldAttribute.SetRequired<FSSODetPart.locationID>(this.MasterCache, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPart.locationID>(this.MasterCache, null, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPart.lotSerialNbr>(this.MasterCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPart.expireDate>(this.MasterCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.inventoryID>(this.DetailCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.expireDate>(this.DetailCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.shipDate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.isAllocated>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.completed>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.shippedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.shipmentNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pOType>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pONbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pOReceiptNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pOSource>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.pOCreate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.receivedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetPartSplit.refNoteID>(this.DetailCache, null, IsAllocationEntryEnabled);

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

        protected virtual void IssueAvailable(PXCache sender, FSSODetPart Row, decimal? BaseQty)
        {
            IssueAvailable(sender, Row, BaseQty, false);
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODetPart Row, decimal? BaseQty, bool isUncomplete)
        {
            DetailCounters.Remove(Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
            foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
                Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
                OrderBy<Asc<INLocation.pickPriority>>>.Select(this._Graph, Row.InventoryID, Row.SubItemID, Row.SiteID))
            {
                FSSODetPartSplit split = (FSSODetPartSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
                split.SplitLineNbr = null;
                split.IsAllocated = Row.RequireAllocation;
                split.SiteID = Row.SiteID;

                object newval;
                DetailCache.RaiseFieldDefaulting<FSSODetPartSplit.allocatedPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetPartSplit.allocatedPlanType>(split, newval);

                DetailCache.RaiseFieldDefaulting<FSSODetPartSplit.backOrderPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetPartSplit.backOrderPlanType>(split, newval);

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

                    if (AvailableQty < BaseQty)
                    {
                        split.BaseQty = AvailableQty;
                        split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)AvailableQty, INPrecision.QUANTITY);
                        DetailCache.Insert(split);

                        BaseQty -= AvailableQty;
                    }
                    else
                    {
                        split.BaseQty = BaseQty;
                        split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)BaseQty, INPrecision.QUANTITY);
                        DetailCache.Insert(split);

                        BaseQty = 0m;
                        break;
                    }
                }
            }

            if (BaseQty > 0m && Row.InventoryID != null && Row.SiteID != null && (Row.SubItemID != null || (Row.SubItemID == null && Row.IsStockItem != true && Row.IsKit == true) || Row.SOLineType == SOLineType.NonInventory))
            {
                FSSODetPartSplit split = (FSSODetPartSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
                split.SplitLineNbr = null;
                split.IsAllocated = false;
                split.BaseQty = BaseQty;
                split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

                BaseQty = 0m;

                if (isUncomplete)
                {
                    split.POCreate = false;
                    split.POSource = null;
                }

                DetailCache.Insert(PXCache<FSSODetPartSplit>.CreateCopy(split));
            }
        }

        public override void UpdateParent(PXCache sender, FSSODetPart Row)
        {
            if (Row != null && Row.RequireShipping == true)
            {
                decimal BaseQty;
                UpdateParent(sender, Row, null, null, out BaseQty);
            }
            else
            {
                base.UpdateParent(sender, Row);
            }
        }

        public override void UpdateParent(PXCache sender, FSSODetPartSplit Row, FSSODetPartSplit OldRow)
        {
            FSSODetPart parent = (FSSODetPart)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(FSSODetPart));

            if (parent != null && parent.RequireShipping == true)
            {
                if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
                {
                    FSSODetPart oldrow = PXCache<FSSODetPart>.CreateCopy(parent);
                    decimal BaseQty;

                    UpdateParent(sender, parent, (Row != null && Row.Completed == false ? Row : null), (OldRow != null && OldRow.Completed == false ? OldRow : null), out BaseQty);

                    using (InvtMultScope<FSSODetPart> ms = new InvtMultScope<FSSODetPart>(parent))
                    {
                        if (IsLotSerialRequired && Row != null)
                        {
                            parent.UnassignedQty = 0m;
                            if (IsLotSerialItem(sender, Row))
                            {
                                object[] splits = SelectDetail(sender, Row);
                                foreach (FSSODetPartSplit split in splits)
                                {
                                    if (split.LotSerialNbr == null)
                                    {
                                        parent.UnassignedQty += split.BaseQty;
                                    }
                                }
                            }
                        }
                        parent.BaseQty = BaseQty + parent.BaseClosedQty;
                        parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
                    }

                    if (sender.Graph.Caches[typeof(FSSODetPart)].GetStatus(parent) == PXEntryStatus.Notchanged)
                    {
                        sender.Graph.Caches[typeof(FSSODetPart)].SetStatus(parent, PXEntryStatus.Updated);
                    }

                    sender.Graph.Caches[typeof(FSSODetPart)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
                    if (sender.Graph.Caches[typeof(FSSODetPart)].RaiseRowUpdating(oldrow, parent))
                    {
                        sender.Graph.Caches[typeof(FSSODetPart)].RaiseRowUpdated(parent, oldrow);
                    }
                    else
                    {
                        sender.Graph.Caches[typeof(FSSODetPart)].RestoreCopy(parent, oldrow);
                    }
                }
            }
            else
            {
                base.UpdateParent(sender, Row, OldRow);
            }
        }

        public static void ResetAvailabilityCounters(FSSODetPart row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

        public override void UpdateParent(PXCache sender, FSSODetPart Row, FSSODetPartSplit Det, FSSODetPartSplit OldDet, out decimal BaseQty)
        {
            ResetAvailabilityCounters(Row);

            bool counted = DetailCounters.ContainsKey(Row);

            base.UpdateParent(sender, Row, Det, OldDet, out BaseQty);

            if (!counted && OldDet != null)
            {
                Counters counters;
                if (DetailCounters.TryGetValue(Row, out counters))
                {
                    if (OldDet.POCreate == true)
                    {
                        counters.BaseQty += (decimal)OldDet.BaseReceivedQty + (decimal)OldDet.BaseShippedQty;
                    }
                    if (OldDet.ShipmentNbr != null)
                    {
                        counters.BaseQty += (decimal)(OldDet.BaseQty - OldDet.BaseShippedQty);
                    }
                    BaseQty = counters.BaseQty;
                }
            }
        }

        protected override void UpdateCounters(PXCache sender, Counters counters, FSSODetPartSplit detail)
        {
            base.UpdateCounters(sender, counters, detail);

            if (detail.POCreate == true)
            {
                //base shipped qty in context of purchase for so is meaningless and equals zero, so it's appended for dropship context
                counters.BaseQty -= (decimal)detail.BaseReceivedQty + (decimal)detail.BaseShippedQty;
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

        protected override object[] SelectDetail(PXCache sender, FSSODetPartSplit row)
        {
            return SelectDetail(sender, row, true);
        }

        protected virtual object[] SelectDetail(PXCache sender, FSSODetPartSplit row, bool ExcludeCompleted = true)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, row, typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetPartSplit)a, row) && ((FSSODetPartSplit)a).LineNbr == row.LineNbr && (((FSSODetPartSplit)a).Completed == false || ExcludeCompleted == false && ((FSSODetPartSplit)a).PONbr == null && ((FSSODetPartSplit)a).SOOrderNbr == null));
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => (((FSSODetPartSplit)a).Completed == false || ExcludeCompleted == false && ((FSSODetPartSplit)a).PONbr == null && ((FSSODetPartSplit)a).SOOrderNbr == null));
        }


        /*protected override object[] SelectDetail(PXCache sender, FSSODetPart row)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, Convert(row), typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetPartSplit)a, row) && ((FSSODetPartSplit)a).LineNbr == row.LineNbr && ((FSSODetPartSplit)a).Completed == false);
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => ((FSSODetPartSplit)a).Completed == false);
        }*/

        protected override object[] SelectDetailOrdered(PXCache sender, FSSODetPartSplit row)
        {
            return SelectDetailOrdered(sender, row, true);
        }

        protected virtual object[] SelectDetailOrdered(PXCache sender, FSSODetPartSplit row, bool ExcludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, ExcludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetPartSplit)a).Completed == true ? 0 : ((FSSODetPartSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetPartSplit)b).Completed == true ? 0 : ((FSSODetPartSplit)b).IsAllocated == true ? 1 : 2;

                int res = ((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetPartSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetPartSplit)b).SplitLineNbr;

                return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        protected override object[] SelectDetailReversed(PXCache sender, FSSODetPartSplit row)
        {
            return SelectDetailReversed(sender, row, true);
        }

        protected virtual object[] SelectDetailReversed(PXCache sender, FSSODetPartSplit row, bool ExcludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, ExcludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetPartSplit)a).Completed == true ? 0 : ((FSSODetPartSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetPartSplit)b).Completed == true ? 0 : ((FSSODetPartSplit)b).IsAllocated == true ? 1 : 2;

                int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetPartSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetPartSplit)b).SplitLineNbr;

                return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        public virtual void UncompleteSchedules(PXCache sender, FSSODetPart Row)
        {
            DetailCounters.Remove(Row);

            decimal? UnshippedQty = Row.BaseOpenQty;

            foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
            {
                if (((FSSODetPartSplit)detail).ShipmentNbr == null)
                {
                    UnshippedQty -= ((FSSODetPartSplit)detail).BaseQty;

                    FSSODetPartSplit newdetail = PXCache<FSSODetPartSplit>.CreateCopy((FSSODetPartSplit)detail);
                    newdetail.Completed = false;

                    DetailCache.Update(newdetail);
                }
            }

            IssueAvailable(sender, Row, (decimal)UnshippedQty, true);
        }

        public virtual void CompleteSchedules(PXCache sender, FSSODetPart Row)
        {
            DetailCounters.Remove(Row);

            string LastShipmentNbr = null;
            decimal? LastUnshippedQty = 0m;
            foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
            {
                if (LastShipmentNbr == null && ((FSSODetPartSplit)detail).ShipmentNbr != null)
                {
                    LastShipmentNbr = ((FSSODetPartSplit)detail).ShipmentNbr;
                }

                if (LastShipmentNbr != null && ((FSSODetPartSplit)detail).ShipmentNbr == LastShipmentNbr)
                {
                    LastUnshippedQty += ((FSSODetPartSplit)detail).BaseOpenQty;
                }
            }

            TruncateSchedules(sender, Row, (decimal)LastUnshippedQty);

            foreach (object detail in SelectDetailReversed(DetailCache, Row))
            {
                FSSODetPartSplit newdetail = PXCache<FSSODetPartSplit>.CreateCopy((FSSODetPartSplit)detail);
                newdetail.Completed = true;

                DetailCache.Update(newdetail);
            }
        }

        public virtual void TruncateSchedules(PXCache sender, FSSODetPart Row, decimal BaseQty)
        {
            DetailCounters.Remove(Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

            foreach (object detail in SelectDetailReversed(DetailCache, Row))
            {
                if (BaseQty >= ((ILSDetail)detail).BaseQty)
                {
                    BaseQty -= (decimal)((ILSDetail)detail).BaseQty;
                    DetailCache.Delete(detail);
                }
                else
                {
                    FSSODetPartSplit newdetail = PXCache<FSSODetPartSplit>.CreateCopy((FSSODetPartSplit)detail);
                    newdetail.BaseQty -= BaseQty;
                    newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

                    DetailCache.Update(newdetail);
                    break;
                }
            }
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODetPart Row)
        {
            IssueAvailable(sender, Row, Row.BaseOpenQty);
        }

        protected override void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<FSSODetPart> e)
        {
            ILSMaster iLSMasterRow = (ILSMaster)e.Row;
            FSSODetPart row = (FSSODetPart)e.Row;
            if (iLSMasterRow.InventoryID == null || row.IsPrepaid == true)
            {
                return;
            }

            bool skipSplitCreating = false;

            if (IsLocationEnabled)
            {
                InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODetPart.inventoryID>(sender, e.Row);
                if (ii != null && ii.StkItem == false && ii.KitItem == false && ii.NonStockShip == false)
                    skipSplitCreating = true;
            }

            if (!skipSplitCreating && (IsLocationEnabled || (IsLotSerialRequired && e.ExternalCall && IsLotSerialItem(sender, e.Row))))
            {
                base._Master_RowInserted(sender, e);
            }
            else
            {
                //sender.SetValue<FSSODetPart.locationID>(e.Row, null);
                //sender.SetValue<FSSODetPart.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODetPart.expireDate>(e.Row, null);

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
                if (!PXUIFieldAttribute.GetErrors(sender, e.Row, PXErrorLevel.Error).Keys.Any(a => string.Compare(a, typeof(FSSODetPart.uOM).Name, StringComparison.InvariantCultureIgnoreCase) == 0))
                    sender.RaiseExceptionHandling<FSSODetPart.uOM>(e.Row, null, ex);
            }
        }

        protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<FSSODetPart> e)
        {
            FSSODetPart row = e.Row as FSSODetPart;
            FSSODetPart oldRow = e.OldRow as FSSODetPart;
            if (row == null || (oldRow.InventoryID == null && row.InventoryID == null) || row.IsPrepaid == true) return;

            bool skipSplitCreating = false;
            InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODetPart.inventoryID>(sender, row);

            if (IsLocationEnabled && ii != null && ii.StkItem == false && ii.KitItem == false && ii.NonStockShip == false )
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
                    AvailabilityCheck(sender, (FSSODetPart)e.Row);
                }
            }
            else
            {
                //sender.SetValue<FSSODetPart.locationID>(e.Row, null);
                //sender.SetValue<FSSODetPart.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODetPart.expireDate>(e.Row, null);

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

                        if (!sender.ObjectsEqual<FSSODetPart.pOCreate, FSSODetPart.pOSource, FSSODetPart.vendorID, FSSODetPart.poVendorLocationID, FSSODetPart.pOSiteID, FSSODetPart.locationID, FSSODetPart.siteLocationID>(e.Row, e.OldRow))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetPartSplit split = detail as FSSODetPartSplit;
                                if (split.IsAllocated == false && split.Completed == false && split.PONbr == null)
                                {
                                    split.POCreate = row.POCreate;
                                    split.POSource = row.POSource;
                                    split.VendorID = row.VendorID;
                                    split.POSiteID = row.POSiteID;

                                    if (row.SiteLocationID != null)
                                    {
                                    split.LocationID = row.SiteLocationID;
                                    }

                                    DetailCache.Update(split);
                                }
                            }
                        }

                        if (!sender.ObjectsEqual<FSSODetPart.shipDate>(e.Row, e.OldRow) ||
                            (!sender.ObjectsEqual<FSSODetPart.shipComplete>(e.Row, e.OldRow) && ((FSSODetPart)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetPartSplit split = detail as FSSODetPartSplit;
                                split.ShipDate = row.ShipDate;

                                DetailCache.Update(split);
                            }
                        }
                    }
                }

                AvailabilityCheck(sender, (FSSODetPart)e.Row);
            }
        }

        protected virtual bool SchedulesEqual(FSSODetPartSplit a, FSSODetPartSplit b)
        {
            if (a != null && b != null)
            {
                return (a.InventoryID == b.InventoryID &&
                                a.SubItemID == b.SubItemID &&
                                a.SiteID == b.SiteID &&
                                a.ToSiteID == b.ToSiteID &&
                                a.ShipDate == b.ShipDate &&
                                a.IsAllocated == b.IsAllocated &&
                                a.IsMergeable != false && b.IsMergeable != false &&
                                a.ShipmentNbr == b.ShipmentNbr &&
                                a.Completed == b.Completed &&
                                a.POCreate == b.POCreate &&
                                a.POCompleted == b.POCompleted &&
                                a.PONbr == b.PONbr &&
                                a.POLineNbr == b.POLineNbr &&
                                a.SOOrderType == b.SOOrderType &&
                                a.SOOrderNbr == b.SOOrderNbr &&
                                a.SOLineNbr == b.SOLineNbr &&
                                a.SOSplitLineNbr == b.SOSplitLineNbr);
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
                    if (((FSSODetPartSplit)e.Row).LineType != SOLineType.Inventory)
                    {
                        throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
                    }
                }

                base.Detail_RowInserting(sender, e);

                if (e.Row != null && !IsLocationEnabled && ((FSSODetPartSplit)e.Row).LocationID != null)
                {
                    ((FSSODetPartSplit)e.Row).LocationID = null;
                }
            }
            else if (IsAllocationEntryEnabled)
            {
                FSSODetPartSplit a = (FSSODetPartSplit)e.Row;

                if (!e.ExternalCall && _Operation == PXDBOperation.Update)
                {
                    foreach (object item in SelectDetail(sender, (FSSODetPartSplit)e.Row))
                    {
                        FSSODetPartSplit detailitem = (FSSODetPartSplit)item;

                        if (SchedulesEqual((FSSODetPartSplit)e.Row, detailitem))
                        {
                            object old_item = PXCache<FSSODetPartSplit>.CreateCopy(detailitem);
                            detailitem.BaseQty += ((FSSODetPartSplit)e.Row).BaseQty;
                            detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

                            detailitem.BaseUnreceivedQty += ((FSSODetPartSplit)e.Row).BaseQty;
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

                if (((FSSODetPartSplit)e.Row).InventoryID == null || string.IsNullOrEmpty(((FSSODetPartSplit)e.Row).UOM))
                {
                    e.Cancel = true;
                }
            }
        }

        protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
        {
            FSSODetPartSplit split = (FSSODetPartSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
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
                    FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy(split);
                    if (split.BaseQty + accum.QtyHardAvail > 0m)
                    {
                        split.BaseQty += accum.QtyHardAvail;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<FSSODetPartSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
                    }

                    sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), split, copy.IsAllocated);
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

            FSSODetPartSplit split = (FSSODetPartSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            FSSODetPartSplit oldsplit = (FSSODetPartSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).OldRow : ((PXRowInsertedEventArgs)e).Row);

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
                FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy(split);
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
                        sender.RaiseExceptionHandling<FSSODetPartSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(IN.Messages.SerialNumberAlreadyReceived, ((InventoryItem)item).InventoryCD, split.LotSerialNbr)));
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
                            sender.RaiseExceptionHandling<FSSODetPartSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
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

                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), split, copy.IsAllocated);
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

                        FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy(split);
                        split.IsAllocated = true;
                        sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), split, copy.IsAllocated);
                        sender.RaiseRowUpdated(split, copy);

                        return true;
                    }

                    //Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
                    if (split.IsAllocated == true)
                    {
                        FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy(split);
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
                                copy = (FSSODetPartSplit)sender.Insert(copy);
                                if (copy.LotSerialNbr != null && copy.IsAllocated != true)
                                {
                                    sender.SetValue<FSSODetPartSplit.lotSerialNbr>(copy, null);
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

        protected virtual bool NegativeInventoryError(PXCache sender, FSSODetPartSplit split)
        {
            split.IsAllocated = false;
            split.LotSerialNbr = null;
            sender.RaiseExceptionHandling<FSSODetPartSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
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

            if ((_InternallCall == false || !string.IsNullOrEmpty(((FSSODetPartSplit)e.Row).LotSerialNbr) && ((FSSODetPartSplit)e.Row).IsAllocated != true) && IsAllocationEntryEnabled)
            {
                if (((FSSODetPartSplit)e.Row).IsAllocated == true || (!string.IsNullOrEmpty(((FSSODetPartSplit)e.Row).LotSerialNbr) && ((FSSODetPartSplit)e.Row).IsAllocated != true))
                {
                    Allocated_Updated(sender, e);

                    sender.RaiseExceptionHandling<FSSODetPartSplit.qty>(e.Row, null, null);
                    AvailabilityCheck(sender, (FSSODetPartSplit)e.Row);
                }
            }
        }

        protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Detail_RowUpdated(sender, e);

            if (_InternallCall == false && IsAllocationEntryEnabled)
            {
                if (!sender.ObjectsEqual<FSSODetPartSplit.isAllocated>(e.Row, e.OldRow) || !sender.ObjectsEqual<FSSODetPartSplit.pOLineNbr>(e.Row, e.OldRow) && ((FSSODetPartSplit)e.Row).POLineNbr == null && ((FSSODetPartSplit)e.Row).IsAllocated == false)
                {
                    if (((FSSODetPartSplit)e.Row).IsAllocated == true)
                    {
                        Allocated_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetPartSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetPartSplit)e.Row);
                    }
                    else
                    {
                        //clear link to created transfer
                        FSSODetPartSplit row = (FSSODetPartSplit)e.Row;
                        row.SOOrderType = null;
                        row.SOOrderNbr = null;
                        row.SOLineNbr = null;
                        row.SOSplitLineNbr = null;

                        foreach (FSSODetPartSplit s in this.SelectDetailReversed(sender, (FSSODetPartSplit)e.Row))
                        {
                            if (s.SplitLineNbr != ((FSSODetPartSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetPartSplit)e.Row))
                            {
                                ((FSSODetPartSplit)e.Row).Qty += s.Qty;
                                ((FSSODetPartSplit)e.Row).BaseQty += s.BaseQty;

                                ((FSSODetPartSplit)e.Row).UnreceivedQty += s.Qty;
                                ((FSSODetPartSplit)e.Row).BaseUnreceivedQty += s.BaseQty;

                                if (((FSSODetPartSplit)e.Row).LotSerialNbr != null)
                                {
                                    FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy((FSSODetPartSplit)e.Row);
                                    ((FSSODetPartSplit)e.Row).LotSerialNbr = null;
                                    //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), s, copy.IsAllocated);
                                    sender.RaiseRowUpdated((FSSODetPartSplit)e.Row, copy);
                                }
                                sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCache();

                                PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                                INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, ((FSSODetPartSplit)e.Row).PlanID);
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
                                    cache.ClearQueryCache();

                                }
                                RefreshView(sender);
                            }
                            else if (s.SplitLineNbr == ((FSSODetPartSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetPartSplit)e.Row) && ((FSSODetPartSplit)e.Row).LotSerialNbr != null)
                            {
                                FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy((FSSODetPartSplit)e.Row);
                                ((FSSODetPartSplit)e.Row).LotSerialNbr = null;
                                sender.RaiseRowUpdated((FSSODetPartSplit)e.Row, copy);
                            }
                        }
                    }
                }

                if (!sender.ObjectsEqual<FSSODetPartSplit.lotSerialNbr>(e.Row, e.OldRow))
                {
                    if (((FSSODetPartSplit)e.Row).LotSerialNbr != null)
                    {
                        LotSerialNbr_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetPartSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetPartSplit)e.Row); //???
                    }
                    else
                    {
                        foreach (FSSODetPartSplit s in this.SelectDetailReversed(sender, (FSSODetPartSplit)e.Row))
                        {
                            if (s.SplitLineNbr == ((FSSODetPartSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetPartSplit)e.Row))
                            {
                                FSSODetPartSplit copy = PXCache<FSSODetPartSplit>.CreateCopy(s);
                                ((FSSODetPartSplit)e.Row).IsAllocated = false;
                                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), (FSSODetPartSplit)e.Row, ((FSSODetPartSplit)e.Row).IsAllocated);
                                //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetPartSplit.isAllocated)), s, copy.IsAllocated);
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
                VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(FSSODetPartSplit.qty).Name);
            }
        }



        public override FSSODetPartSplit Convert(FSSODetPart item)
        {
            using (InvtMultScope<FSSODetPart> ms = new InvtMultScope<FSSODetPart>(item))
            {
                FSSODetPartSplit ret = (FSSODetPartSplit)item;
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

        public virtual void FSSODetPartSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            FSSODetPartSplit split = e.Row as FSSODetPartSplit;

            if (split != null)
            {
                bool isLineTypeInventory = (split.LineType == SOLineType.Inventory);
                object val = sender.GetValueExt<FSSODetPartSplit.isAllocated>(e.Row);
                bool isAllocated = split.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
                bool isCompleted = split.Completed == true;
                bool isIssue = split.Operation == SOOperation.Issue;
                bool IsLinked = split.PONbr != null || split.SOOrderNbr != null && split.IsAllocated == true;

                FSSODetPart parent = (FSSODetPart)PXParentAttribute.SelectParent(sender, split, typeof(FSSODetPart));
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.subItemID>(sender, e.Row, isLineTypeInventory);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.completed>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.shippedQty>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.shipmentNbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.isAllocated>(sender, e.Row, isLineTypeInventory && isIssue && !isCompleted);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.siteID>(sender, e.Row, isLineTypeInventory && isAllocated && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.qty>(sender, e.Row, !isCompleted && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.shipDate>(sender, e.Row, !isCompleted && parent.ShipComplete == SOShipComplete.BackOrderAllowed);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.pONbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetPartSplit.pOReceiptNbr>(sender, e.Row, false);

                if (split.Completed == true)
                {
                    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                }
            }
        }

        public virtual void FSSODetPartSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                bool RequireLocationAndSubItem = ((FSSODetPartSplit)e.Row).RequireLocation == true && ((FSSODetPartSplit)e.Row).IsStockItem == true && ((FSSODetPartSplit)e.Row).BaseQty != 0m;

                PXDefaultAttribute.SetPersistingCheck<FSSODetPartSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSODetPartSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            }
        }

        public virtual void FSSODetPartSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetPart)];
            if (cache.Current != null && (e.Row == null || ((FSSODetPart)cache.Current).LineNbr == ((FSSODetPartSplit)e.Row).LineNbr && ((FSSODetPartSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSSODetPart)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void FSSODetPartSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetPart)];

            FSSODetPart parentRow = cache.Current as FSSODetPart;
            FSSODetPartSplit splitRow = e.Row as FSSODetPartSplit;

            if (cache.Current != null && parentRow.LocationID != null && (e.Row == null || (parentRow.LineNbr == splitRow.LineNbr && splitRow.IsStockItem == true)))
            {
                e.NewValue = parentRow.LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null || !IsLocationEnabled);
            }
        }

        public virtual void FSSODetPartSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetPart)];
            if (cache.Current != null && (e.Row == null || ((FSSODetPart)cache.Current).LineNbr == ((FSSODetPartSplit)e.Row).LineNbr))
            {
                using (InvtMultScope<FSSODetPart> ms = new InvtMultScope<FSSODetPart>((FSSODetPart)cache.Current))
                {
                    e.NewValue = ((FSSODetPart)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
        {
            if (row is FSSODetPart)
            {
                sender.RaiseExceptionHandling<FSSODetPart.orderQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODetPart.inventoryID>(row), sender.GetStateExt<FSSODetPart.subItemID>(row), sender.GetStateExt<FSSODetPart.siteID>(row), sender.GetStateExt<FSSODetPart.locationID>(row), sender.GetValue<FSSODetPart.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<FSSODetPartSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODetPartSplit.inventoryID>(row), sender.GetStateExt<FSSODetPartSplit.subItemID>(row), sender.GetStateExt<FSSODetPartSplit.siteID>(row), sender.GetStateExt<FSSODetPartSplit.locationID>(row), sender.GetValue<FSSODetPartSplit.lotSerialNbr>(row)));
            }
        }

        public virtual bool MemoAvailabilityCheck(PXCache sender, FSSODetPart Row)
        {
            bool success = MemoAvailabilityCheckQty(sender, Row);
            
            return success;
        }

        protected virtual bool MemoAvailabilityCheckQty(PXCache sender, FSSODetPart row)
        {
            return true;
        }

        protected virtual bool MemoAvailabilityCheck<Target>(PXCache sender, FSSODetPart Row, ILSMaster Split)
            where Target : class, ILSMaster
        {
            bool success = true;
            return success;
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
        {
            base.AvailabilityCheck(sender, Row);

            if (Row is FSSODetPart)
            {
                MemoAvailabilityCheck(sender, (FSSODetPart)Row);

                FSSODetPartSplit copy = Convert(Row as FSSODetPart);

                if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                {
                    DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetPartSplit)], copy);
                }

                MemoAvailabilityCheck<FSSODetPart>(sender, (FSSODetPart)Row, copy);

                if (copy.LotSerialNbr == null)
                {
                    Row.LotSerialNbr = null;
                }
            }
            else
            {
                object parent = PXParentAttribute.SelectParent(sender, Row, typeof(FSSODetPart));
                MemoAvailabilityCheck(sender.Graph.Caches[typeof(FSSODetPart)], (FSSODetPart)parent);
                MemoAvailabilityCheck<FSSODetPartSplit>(sender.Graph.Caches[typeof(FSSODetPart)], (FSSODetPart)parent, Row);
            }
        }

        public override void DefaultLotSerialNbr(PXCache sender, FSSODetPartSplit row)
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

        public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, FSSODetPart Row, PXResult<InventoryItem, INLotSerClass> item)
        {
            PXSelectBase<INLotSerialStatus> cmd = new PXSelectJoin<INLotSerialStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>>,
                Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
                And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
                And<INLotSerialStatus.qtyOnHand, Greater<decimal0>>>>>(sender.Graph);
            if (!IsLocationEnabled && IsLotSerialRequired)
                cmd = new PXSelectJoin<INLotSerialStatus, InnerJoin<INLocation, On<INLocation.locationID, Equal<INLotSerialStatus.locationID>>,
                InnerJoin<INSiteLotSerial, On<INSiteLotSerial.inventoryID, Equal<INLotSerialStatus.inventoryID>,
                And<INSiteLotSerial.siteID, Equal<INLotSerialStatus.siteID>, And<INSiteLotSerial.lotSerialNbr, Equal<INLotSerialStatus.lotSerialNbr>>>>>>,
                Where<INLotSerialStatus.inventoryID, Equal<Current<INLotSerialStatus.inventoryID>>,
                And<INLotSerialStatus.siteID, Equal<Current<INLotSerialStatus.siteID>>,
                And<INLotSerialStatus.qtyOnHand, Greater<decimal0>,
                And<INSiteLotSerial.qtyHardAvail, Greater<decimal0>>>>>>(sender.Graph);

            if (Row.SubItemID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.subItemID, Equal<Current<INLotSerialStatus.subItemID>>>>();
            }
            if (Row.LocationID != null)
            {
                cmd.WhereAnd<Where<INLotSerialStatus.locationID, Equal<Current<INLotSerialStatus.locationID>>>>();
            }
            else
            {
                switch (Row.TranType)
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

    public class FSSODetPartSplitPlanIDAttribute : INItemPlanIDAttribute
    {
        #region State
        protected Type _ParentOrderDate;
        #endregion
        #region Ctor
        public FSSODetPartSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
            : base(ParentNoteID, ParentHoldEntry)
        {
            _ParentOrderDate = ParentOrderDate;
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

            if (CustomerUpdated 
                || !sender.ObjectsEqual<FSServiceOrder.hold, FSServiceOrder.status>(e.Row, e.OldRow))
            {
                //DatesUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;
                //RequestOnUpdated |= !sender.ObjectsEqual<FSServiceOrder.shipComplete>(e.Row, e.OldRow) && ((FSServiceOrder)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed;

                bool Cancelled = (string)sender.GetValue<FSServiceOrder.status>(e.Row) == ID.Status_ServiceOrder.CANCELED;
                //bool? BackOrdered = (bool?)sender.GetValue<FSServiceOrder.backOrdered>(e.Row);

                PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
                PXCache splitcache = sender.Graph.Caches[typeof(FSSODetPartSplit)];

                SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);

                var splitsByPlan = new Dictionary<long?, FSSODetPartSplit>();
                foreach (FSSODetPartSplit split in PXSelect<FSSODetPartSplit,
                    Where<FSSODetPartSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>, And<FSSODetPartSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
                    .SelectMultiBound(sender.Graph, new[] { e.Row }))
                {
                    if (Cancelled)
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
                            if (string.IsNullOrEmpty(split.ShipmentNbr) && split.POCompleted == false)
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

                        /*if (DatesUpdated)
                        {
                            split.ShipDate = (DateTime?)sender.GetValue<FSServiceOrder.shipDate>(e.Row);
                            splitcache.SmartSetStatus(split, PXEntryStatus.Updated);
                        }*/

                        if (split.PlanID != null)
                        {
                            splitsByPlan[split.PlanID] = split;
                        }
                    }
                }

                PXCache linecache = sender.Graph.Caches[typeof(FSSODetPart)];

                foreach (FSSODetPart line in PXSelect<FSSODetPart,
                                                Where<FSSODetPart.srvOrdType, 
                                                    Equal<Current<FSServiceOrder.srvOrdType>>, 
                                                And<FSSODetPart.refNbr, 
                                                    Equal<Current<FSServiceOrder.refNbr>>,
                                                And<
                                                    FSSODetPart.lineType, Equal<FSSODetPart.lineType.Inventory_Item>>>>>
                    .SelectMultiBound(sender.Graph, new[] { e.Row }))
                {
                    if (Cancelled)
                    {
                        FSSODetPart old_row = PXCache<FSSODetPart>.CreateCopy(line);
                        //line.UnbilledQty -= line.OpenQty;
                        line.OpenQty = 0m;
                        //linecache.RaiseFieldUpdated<FSSODetPart.unbilledQty>(line, 0m);
                        linecache.RaiseFieldUpdated<FSSODetPart.openQty>(line, 0m);

                        line.Completed = true;
                        LSFSSODetPartLine.ResetAvailabilityCounters(line);

                        //SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
                        //+++//TaxAttribute.Calculate<FSSODetPart.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

                        linecache.MarkUpdated(line);
                    }
                    else
                    {
                        if ((string)sender.GetValue<FSServiceOrder.status>(e.OldRow) == ID.Status_ServiceOrder.CANCELED)
                        {
                            FSSODetPart old_row = PXCache<FSSODetPart>.CreateCopy(line);
                            line.OpenQty = line.OrderQty;
                            /*line.UnbilledQty += line.OpenQty;
                            object value = line.UnbilledQty;
                            linecache.RaiseFieldVerifying<FSSODetPart.unbilledQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODetPart.unbilledQty>(line, value);*/

                            object value = line.OpenQty;
                            linecache.RaiseFieldVerifying<FSSODetPart.openQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODetPart.openQty>(line, value);

                            line.Completed = false;

                            //+++++//
                            //TaxAttribute.Calculate<FSSODetPart.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

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
                            LSFSSODetPartLine.ResetAvailabilityCounters(line);
                        }
                    }
                }

                if (Cancelled)
                {
                    //PXFormulaAttribute.CalcAggregate<FSSODetPart.unbilledQty>(linecache, e.Row);
                    PXFormulaAttribute.CalcAggregate<FSSODetPart.openQty>(linecache, e.Row);
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
                    if (Cancelled)
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

                        FSSODetPartSplit split;
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

        public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Operation == PXDBOperation.Insert)
            {
                _KeyToAbort = sender.GetValue<FSServiceOrder.refNbr>(e.Row);
            }
        }

        public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
            {
                foreach (FSSODetPartSplit split in PXSelect<FSSODetPartSplit, Where<FSSODetPartSplit.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>, And<FSSODetPartSplit.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>.Select(sender.Graph, ((FSServiceOrder)e.Row).SrvOrdType, _KeyToAbort))
                {
                    foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
                    {
                        if (object.Equals(plan.PlanID, split.PlanID))
                        {
                            plan.RefNoteID = (Guid?)sender.GetValue(e.Row, _ParentNoteID.Name);
                        }
                    }
                }
            }
            _KeyToAbort = null;
        }

        bool InitPlan = false;
        bool InitVendor = false;
        bool ResetSupplyPlanID = false;
        public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            //respond only to GUI operations
            var IsLinked = IsLineLinked((FSSODetPartSplit)e.Row);

            InitPlan = InitPlanRequired(sender, e) && !IsLinked;
            FSSODetPart parent = (FSSODetPart)PXParentAttribute.SelectParent(sender, e.Row, typeof(FSSODetPart));

            InitVendor = !sender.ObjectsEqual<FSSODetPartSplit.siteID, FSSODetPartSplit.subItemID, FSSODetPartSplit.vendorID, FSSODetPartSplit.pOCreate>(e.Row, e.OldRow) &&
                !IsLinked;

            InitVendor = InitVendor || parent.POVendorLocationID != null;

            ResetSupplyPlanID = !IsLinked;

            try
            {
                base.RowUpdated(sender, e);
            }
            finally
            {
                InitPlan = false;
                ResetSupplyPlanID = false;
            }
        }

        protected virtual bool InitPlanRequired(PXCache cache, PXRowUpdatedEventArgs e)
        {
            return !cache
                .ObjectsEqual<FSSODetPartSplit.isAllocated,
                    FSSODetPartSplit.siteID,
                    FSSODetPartSplit.pOCreate,
                    FSSODetPartSplit.pOSource,
                    FSSODetPartSplit.operation>(e.Row, e.OldRow);
        }

        protected virtual bool IsLineLinked(FSSODetPartSplit soLineSplit)
        {
            return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
        {
            if (((FSSODetPartSplit)orig_Row).Completed == true || ((FSSODetPartSplit)orig_Row).POCompleted == true || ((FSSODetPartSplit)orig_Row).LineType == SOLineType.MiscCharge || ((FSSODetPartSplit)orig_Row).LineType == SOLineType.NonInventory && ((FSSODetPartSplit)orig_Row).RequireShipping == false)
            {
                return null;
            }
            FSSODetPart parent = (FSSODetPart)PXParentAttribute.SelectParent(sender, orig_Row, typeof(FSSODetPart));
            FSServiceOrder order = (FSServiceOrder)PXParentAttribute.SelectParent(sender, orig_Row, typeof(FSServiceOrder));

            FSSODetPartSplit split_Row = (FSSODetPartSplit)orig_Row;

            if (string.IsNullOrEmpty(plan_Row.PlanType) || InitPlan)
            {
                plan_Row.PlanType = CalcPlanType(sender, plan_Row, order, split_Row);
                if (split_Row.POCreate == true)
                {
                    plan_Row.FixedSource = INReplenishmentSource.Purchased;
                    if (split_Row.POType != PO.POOrderType.Blanket && split_Row.POType != PO.POOrderType.DropShip && split_Row.POSource == INReplenishmentSource.PurchaseToOrder)
                        plan_Row.SourceSiteID = split_Row.SiteID;
                    else
                        plan_Row.SourceSiteID = split_Row.SiteID;
                }
                else
                {
                    plan_Row.Reverse = (split_Row.Operation == SOOperation.Receipt);

                    plan_Row.FixedSource = (split_Row.SiteID != split_Row.ToSiteID ? INReplenishmentSource.Transfer : INReplenishmentSource.None);
                    plan_Row.SourceSiteID = split_Row.SiteID;
                }
            }

            if (ResetSupplyPlanID)
            {
                plan_Row.SupplyPlanID = null;
            }

            plan_Row.VendorID = split_Row.VendorID;

            if (InitVendor || split_Row.POCreate == true && plan_Row.VendorID != null && plan_Row.VendorLocationID == null)
            {
                plan_Row.VendorLocationID = parent?.POVendorLocationID;

                if (plan_Row.VendorLocationID == null)
                {
                    plan_Row.VendorLocationID =
                        PX.Objects.PO.POItemCostManager.FetchLocation(
                        sender.Graph,
                        split_Row.VendorID,
                        split_Row.InventoryID,
                        split_Row.SubItemID,
                        split_Row.SiteID);
                }
            }

            plan_Row.BAccountID = parent == null ? null : parent.BillCustomerID;
            plan_Row.InventoryID = split_Row.InventoryID;
            plan_Row.SubItemID = split_Row.SubItemID;
            plan_Row.SiteID = split_Row.SiteID;
            plan_Row.LocationID = split_Row.LocationID;
            plan_Row.LotSerialNbr = split_Row.LotSerialNbr;
            if (string.IsNullOrEmpty(split_Row.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(split_Row.AssignedNbr, split_Row.LotSerialNbr))
            {
                plan_Row.LotSerialNbr = null;
            }
            plan_Row.PlanDate = split_Row.ShipDate;
            plan_Row.PlanQty = (split_Row.POCreate == true ? split_Row.BaseUnreceivedQty - split_Row.BaseShippedQty : split_Row.BaseQty);

            PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
            plan_Row.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
            plan_Row.Hold = IsOrderOnHold(order);

            if (plan_Row.RefNoteID == Guid.Empty)
            {
                plan_Row.RefNoteID = null;
            }

            if (string.IsNullOrEmpty(plan_Row.PlanType))
            {
                return null;
            }
            return plan_Row;
        }

        protected virtual bool IsOrderOnHold(FSServiceOrder order)
        {
            return (order != null) && ((order.Hold ?? false)) /*|| (order.CreditHold ?? false) || (!order.Approved ?? false))*/;
        }

        protected virtual string CalcPlanType(PXCache sender, INItemPlan plan, FSServiceOrder order, FSSODetPartSplit split, bool? backOrdered = null)
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

            if (!InitPlan && !putOnSOPrepared && !isAllocation)
            {
                if (backOrdered == true || backOrdered == null && plan.PlanType == INPlanConstants.Plan68)
                {
                    return INPlanConstants.Plan68;
                }
            }

            return calcedPlanType;
        }

        protected virtual string CalcPlanType(INItemPlan plan, FSSODetPartSplit split, SOOrderType ordertype, bool isOrderOnHold)
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
