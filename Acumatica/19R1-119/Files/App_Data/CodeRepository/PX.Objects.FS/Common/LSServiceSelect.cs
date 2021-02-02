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
    public class LSFSSODetServiceLine : LSSelectSOBase<FSSODetService, FSSODetServiceSplit,
        Where<FSSODetServiceSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
        And<FSSODetServiceSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>,
        And<
            Where<FSSODetService.lineType, Equal<FSSODetService.lineType.Service>,
            Or<FSSODetService.lineType, Equal<FSSODetService.lineType.NonStockItem>>>>>>>
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
        public LSFSSODetServiceLine(PXGraph graph)
            : base(graph)
        {
            MasterQtyField = typeof(FSSODetService.orderQty);
            graph.FieldDefaulting.AddHandler<FSSODetServiceSplit.subItemID>(FSSODetServiceSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetServiceSplit.locationID>(FSSODetServiceSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<FSSODetServiceSplit.invtMult>(FSSODetServiceSplit_InvtMult_FieldDefaulting);
            graph.RowSelected.AddHandler<FSServiceOrder>(Parent_RowSelected);
            graph.RowUpdated.AddHandler<FSServiceOrder>(FSServiceOrder_RowUpdated);
            graph.RowSelected.AddHandler<FSSODetServiceSplit>(FSSODetServiceSplit_RowSelected);
            graph.RowPersisting.AddHandler<FSSODetServiceSplit>(FSSODetServiceSplit_RowPersisting);
        }
        #endregion

        #region Implementation
        public override IEnumerable BinLotSerial(PXAdapter adapter)
        {
            if (IsLSEntryEnabled || IsAllocationEntryEnabled)
            {
                if (MasterCache.Current != null && ((IsLSEntryEnabled && ((FSSODetService)MasterCache.Current).SOLineType != SOLineType.Inventory) || ((FSSODetService)MasterCache.Current).SOLineType == SOLineType.MiscCharge))
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
                PXCache cache = sender.Graph.Caches[typeof(FSSODetService)];

                foreach (FSSODetService item in PXParentAttribute.SelectSiblings(cache, null, typeof(FSServiceOrder)))
                {
                    if (Math.Abs((decimal)item.BaseQty) >= 0.0000005m && (item.UnassignedQty >= 0.0000005m || item.UnassignedQty <= -0.0000005m))
                    {
                        cache.RaiseExceptionHandling<FSSODetService.orderQty>(item, item.Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned));

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
                var row = (FSSODetService)e.Row;
                if ((row.SOLineType == SOLineType.Inventory || row.SOLineType == SOLineType.NonInventory && row.InvtMult == (short)-1) && row.TranType != INTranType.NoUpdate && row.BaseQty < 0m)
                {
                    if (sender.RaiseExceptionHandling<FSSODetService.orderQty>(e.Row, ((FSSODetService)e.Row).Qty, new PXSetPropertyException(CS.Messages.Entry_GE, ((int)0).ToString())))
                    {
                        throw new PXRowPersistingException(typeof(FSSODetService.orderQty).Name, ((FSSODetService)e.Row).Qty, CS.Messages.Entry_GE, ((int)0).ToString());
                    }
                    return;
                }

                if (IsLSEntryEnabled)
                {
                    PXCache cache = sender.Graph.Caches[typeof(FSServiceOrder)];
                    object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(FSServiceOrder)) ?? cache.Current;

                    bool? OnHold = (bool?)cache.GetValue<FSServiceOrder.hold>(doc);

                    if (OnHold == false && Math.Abs((decimal)((FSSODetService)e.Row).BaseQty) >= 0.0000005m && (((FSSODetService)e.Row).UnassignedQty >= 0.0000005m || ((FSSODetService)e.Row).UnassignedQty <= -0.0000005m))
                    {
                        if (sender.RaiseExceptionHandling<FSSODetService.orderQty>(e.Row, ((FSSODetService)e.Row).Qty, new PXSetPropertyException(SO.Messages.BinLotSerialNotAssigned)))
                        {
                            throw new PXRowPersistingException(typeof(FSSODetService.orderQty).Name, ((FSSODetService)e.Row).Qty, SO.Messages.BinLotSerialNotAssigned);
                        }
                    }
                }
            }

            //for normal orders there are only when received numbers which do not require any additional processing
            if (!IsLSEntryEnabled)
            {
                if (((FSSODetService)e.Row).TranType == INTranType.Transfer && DetailCounters.ContainsKey((FSSODetService)e.Row))
                {
                    //keep Counters when adding splits to Transfer order
                    DetailCounters[(FSSODetService)e.Row].UnassignedNumber = 0;
                }
                else
                {
                    DetailCounters[(FSSODetService)e.Row] = new Counters { UnassignedNumber = 0 };
                }
            }

            base.Master_RowPersisting(sender, e);
        }

        protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
        {
            InventoryItem item = (InventoryItem)PXSelectorAttribute.Select(sender, null, typeof(FSSODetService.inventoryID).Name, InventoryID);

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
                FSSODetServiceSplit copy = Row as FSSODetServiceSplit;
                if (copy == null)
                {
                    copy = Convert(Row as FSSODetService);

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODetService), Row);

                    if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                    {
                        DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetServiceSplit)], copy);
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
                    {
                        if (_detailsRequested++ == 5)
                        {
                            foreach (PXResult<FSSODetService, INUnit, INSiteStatus> res in 
                                PXSelectReadonly2<
                                    FSSODetService, 
                                    InnerJoin<INUnit, On<INUnit.inventoryID, Equal<FSSODetService.inventoryID>, And<INUnit.fromUnit, Equal<FSSODetService.uOM>>>,
                                    InnerJoin<INSiteStatus, On<FSSODetService.inventoryID, Equal<INSiteStatus.inventoryID>, And<FSSODetService.subItemID, Equal<INSiteStatus.subItemID>, And<FSSODetService.siteID, Equal<INSiteStatus.siteID>>>>>>,
                                Where<
                                    FSSODetService.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>, 
                                    And<FSSODetService.refNbr, Equal<Current<FSServiceOrder.refNbr>>,
                                    And<
                                        Where<FSSODetService.lineType, Equal<FSSODetService.lineType.Service>,
                                            Or<FSSODetService.lineType, Equal<FSSODetService.lineType.NonStockItem>>>>>>>.Select(sender.Graph))
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
                        return DeductAllocated(sender, (FSSODetService)Row, result);
                    }
                }

                return AvailabilityFetch(sender, copy, fetchMode);
            }
            return null;
        }

        public virtual IQtyAllocated DeductAllocated(PXCache sender, FSSODetService soLine, IQtyAllocated result)
        {
            if (result == null) return null;
            decimal? lineQtyAvail = (decimal?)sender.GetValue<FSSODetService.lineQtyAvail>(soLine);
            decimal? lineQtyHardAvail = (decimal?)sender.GetValue<FSSODetService.lineQtyHardAvail>(soLine);

            if (lineQtyAvail == null || lineQtyHardAvail == null)
            {
                lineQtyAvail = 0m;
                lineQtyHardAvail = 0m;

                foreach (FSSODetServiceSplit split in SelectDetail(DetailCache, soLine))
                {
                    FSSODetServiceSplit copy = split;
                    if (copy.PlanID != null)
                    {
                        INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, copy.PlanID);
                        if (plan != null)
                        {
                            copy = PXCache<FSSODetServiceSplit>.CreateCopy(copy);
                            copy.PlanType = plan.PlanType;
                        }
                    }

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(FSSODetService), soLine);

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

                sender.SetValue<FSSODetService.lineQtyAvail>(soLine, lineQtyAvail);
                sender.SetValue<FSSODetService.lineQtyHardAvail>(soLine, lineQtyHardAvail);
            }

            result.QtyAvail += lineQtyAvail;
            result.QtyHardAvail += lineQtyHardAvail;
            result.QtyNotAvail = -lineQtyAvail;

            return result;
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            IQtyAllocated availability = AvailabilityFetch(sender, (FSSODetService)e.Row, !(e.Row != null && (((FSSODetService)e.Row).Completed == true)) ? AvailabilityFetchMode.ExcludeCurrent : AvailabilityFetchMode.None);

            if (availability != null)
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((FSSODetService)e.Row).InventoryID);

                decimal unitRate = INUnitAttribute.ConvertFromBase<FSSODetService.inventoryID, FSSODetService.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
                availability.QtyOnHand = PXDBQuantityAttribute.Round((decimal)availability.QtyOnHand * unitRate);
                availability.QtyAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyAvail * unitRate);
                availability.QtyNotAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyNotAvail * unitRate);
                availability.QtyHardAvail = PXDBQuantityAttribute.Round((decimal)availability.QtyHardAvail * unitRate);

                if (IsAllocationEntryEnabled)
                {
                    Decimal? allocated = PXDBQuantityAttribute.Round((decimal)(((FSSODetService)e.Row).LineQtyHardAvail ?? 0m) * unitRate); ;
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_AllocatedInfo,
                            sender.GetValue<FSSODetService.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));
                }
                else
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(SO.Messages.Availability_Info,
                            sender.GetValue<FSSODetService.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail));


                AvailabilityCheck(sender, (FSSODetService)e.Row, availability);
            }
            else
            {
                //handle missing UOM
                INUnitAttribute.ConvertFromBase<FSSODetService.inventoryID, FSSODetService.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

        protected FSServiceOrder _LastSelected;

        protected virtual void Parent_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (_LastSelected == null || !object.ReferenceEquals(_LastSelected, e.Row))
            {
                PXUIFieldAttribute.SetRequired<FSSODetService.locationID>(this.MasterCache, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetService.locationID>(this.MasterCache, null, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetService.lotSerialNbr>(this.MasterCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetService.expireDate>(this.MasterCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.inventoryID>(this.DetailCache, null, IsLSEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.locationID>(this.DetailCache, null, IsLocationEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.expireDate>(this.DetailCache, null, IsLSEntryEnabled);

                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.shipDate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.isAllocated>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.completed>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.shippedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.shipmentNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.pOType>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.pONbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.pOReceiptNbr>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.pOSource>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.pOCreate>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.receivedQty>(this.DetailCache, null, IsAllocationEntryEnabled);
                PXUIFieldAttribute.SetVisible<FSSODetServiceSplit.refNoteID>(this.DetailCache, null, IsAllocationEntryEnabled);

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

        protected virtual void IssueAvailable(PXCache sender, FSSODetService Row, decimal? BaseQty)
        {
            IssueAvailable(sender, Row, BaseQty, false);
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODetService Row, decimal? BaseQty, bool isUncomplete)
        {
            DetailCounters.Remove(Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
            foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
                Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
                OrderBy<Asc<INLocation.pickPriority>>>.Select(this._Graph, Row.InventoryID, Row.SubItemID, Row.SiteID))
            {
                FSSODetServiceSplit split = (FSSODetServiceSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
                split.SplitLineNbr = null;
                split.IsAllocated = Row.RequireAllocation;
                split.SiteID = Row.SiteID;

                object newval;
                DetailCache.RaiseFieldDefaulting<FSSODetServiceSplit.allocatedPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetServiceSplit.allocatedPlanType>(split, newval);

                DetailCache.RaiseFieldDefaulting<FSSODetServiceSplit.backOrderPlanType>(split, out newval);
                DetailCache.SetValue<FSSODetServiceSplit.backOrderPlanType>(split, newval);

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
                FSSODetServiceSplit split = (FSSODetServiceSplit)Row;
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

                DetailCache.Insert(PXCache<FSSODetServiceSplit>.CreateCopy(split));
            }
        }

        public override void UpdateParent(PXCache sender, FSSODetService Row)
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

        public override void UpdateParent(PXCache sender, FSSODetServiceSplit Row, FSSODetServiceSplit OldRow)
        {
            FSSODetService parent = (FSSODetService)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(FSSODetService));

            if (parent != null && parent.RequireShipping == true)
            {
                if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
                {
                    FSSODetService oldrow = PXCache<FSSODetService>.CreateCopy(parent);
                    decimal BaseQty;

                    UpdateParent(sender, parent, (Row != null && Row.Completed == false ? Row : null), (OldRow != null && OldRow.Completed == false ? OldRow : null), out BaseQty);

                    using (InvtMultScope<FSSODetService> ms = new InvtMultScope<FSSODetService>(parent))
                    {
                        if (IsLotSerialRequired && Row != null)
                        {
                            parent.UnassignedQty = 0m;
                            if (IsLotSerialItem(sender, Row))
                            {
                                object[] splits = SelectDetail(sender, Row);
                                foreach (FSSODetServiceSplit split in splits)
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

                    if (sender.Graph.Caches[typeof(FSSODetService)].GetStatus(parent) == PXEntryStatus.Notchanged)
                    {
                        sender.Graph.Caches[typeof(FSSODetService)].SetStatus(parent, PXEntryStatus.Updated);
                    }

                    sender.Graph.Caches[typeof(FSSODetService)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
                    if (sender.Graph.Caches[typeof(FSSODetService)].RaiseRowUpdating(oldrow, parent))
                    {
                        sender.Graph.Caches[typeof(FSSODetService)].RaiseRowUpdated(parent, oldrow);
                    }
                    else
                    {
                        sender.Graph.Caches[typeof(FSSODetService)].RestoreCopy(parent, oldrow);
                    }
                }
            }
            else
            {
                base.UpdateParent(sender, Row, OldRow);
            }
        }

        public static void ResetAvailabilityCounters(FSSODetService row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

        public override void UpdateParent(PXCache sender, FSSODetService Row, FSSODetServiceSplit Det, FSSODetServiceSplit OldDet, out decimal BaseQty)
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

        protected override void UpdateCounters(PXCache sender, Counters counters, FSSODetServiceSplit detail)
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

        protected override object[] SelectDetail(PXCache sender, FSSODetServiceSplit row)
        {
            return SelectDetail(sender, row, true);
        }

        protected virtual object[] SelectDetail(PXCache sender, FSSODetServiceSplit row, bool ExcludeCompleted = true)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, row, typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetServiceSplit)a, row) && ((FSSODetServiceSplit)a).LineNbr == row.LineNbr && (((FSSODetServiceSplit)a).Completed == false || ExcludeCompleted == false && ((FSSODetServiceSplit)a).PONbr == null && ((FSSODetServiceSplit)a).SOOrderNbr == null));
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => (((FSSODetServiceSplit)a).Completed == false || ExcludeCompleted == false && ((FSSODetServiceSplit)a).PONbr == null && ((FSSODetServiceSplit)a).SOOrderNbr == null));
        }


        /*protected override object[] SelectDetail(PXCache sender, FSSODetService row)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, Convert(row), typeof(FSServiceOrder));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((FSSODetServiceSplit)a, row) && ((FSSODetServiceSplit)a).LineNbr == row.LineNbr && ((FSSODetServiceSplit)a).Completed == false);
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => ((FSSODetServiceSplit)a).Completed == false);
        }*/

        protected override object[] SelectDetailOrdered(PXCache sender, FSSODetServiceSplit row)
        {
            return SelectDetailOrdered(sender, row, true);
        }

        protected virtual object[] SelectDetailOrdered(PXCache sender, FSSODetServiceSplit row, bool ExcludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, ExcludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetServiceSplit)a).Completed == true ? 0 : ((FSSODetServiceSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetServiceSplit)b).Completed == true ? 0 : ((FSSODetServiceSplit)b).IsAllocated == true ? 1 : 2;

                int res = ((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetServiceSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetServiceSplit)b).SplitLineNbr;

                return ((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        protected override object[] SelectDetailReversed(PXCache sender, FSSODetServiceSplit row)
        {
            return SelectDetailReversed(sender, row, true);
        }

        protected virtual object[] SelectDetailReversed(PXCache sender, FSSODetServiceSplit row, bool ExcludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, ExcludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((FSSODetServiceSplit)a).Completed == true ? 0 : ((FSSODetServiceSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((FSSODetServiceSplit)b).Completed == true ? 0 : ((FSSODetServiceSplit)b).IsAllocated == true ? 1 : 2;

                int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((FSSODetServiceSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((FSSODetServiceSplit)b).SplitLineNbr;

                return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        public virtual void UncompleteSchedules(PXCache sender, FSSODetService Row)
        {
            DetailCounters.Remove(Row);

            decimal? UnshippedQty = Row.BaseOpenQty;

            foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
            {
                if (((FSSODetServiceSplit)detail).ShipmentNbr == null)
                {
                    UnshippedQty -= ((FSSODetServiceSplit)detail).BaseQty;

                    FSSODetServiceSplit newdetail = PXCache<FSSODetServiceSplit>.CreateCopy((FSSODetServiceSplit)detail);
                    newdetail.Completed = false;

                    DetailCache.Update(newdetail);
                }
            }

            IssueAvailable(sender, Row, (decimal)UnshippedQty, true);
        }

        public virtual void CompleteSchedules(PXCache sender, FSSODetService Row)
        {
            DetailCounters.Remove(Row);

            string LastShipmentNbr = null;
            decimal? LastUnshippedQty = 0m;
            foreach (object detail in SelectDetailReversed(DetailCache, Row, false))
            {
                if (LastShipmentNbr == null && ((FSSODetServiceSplit)detail).ShipmentNbr != null)
                {
                    LastShipmentNbr = ((FSSODetServiceSplit)detail).ShipmentNbr;
                }

                if (LastShipmentNbr != null && ((FSSODetServiceSplit)detail).ShipmentNbr == LastShipmentNbr)
                {
                    LastUnshippedQty += ((FSSODetServiceSplit)detail).BaseOpenQty;
                }
            }

            TruncateSchedules(sender, Row, (decimal)LastUnshippedQty);

            foreach (object detail in SelectDetailReversed(DetailCache, Row))
            {
                FSSODetServiceSplit newdetail = PXCache<FSSODetServiceSplit>.CreateCopy((FSSODetServiceSplit)detail);
                newdetail.Completed = true;

                DetailCache.Update(newdetail);
            }
        }

        public virtual void TruncateSchedules(PXCache sender, FSSODetService Row, decimal BaseQty)
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
                    FSSODetServiceSplit newdetail = PXCache<FSSODetServiceSplit>.CreateCopy((FSSODetServiceSplit)detail);
                    newdetail.BaseQty -= BaseQty;
                    newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

                    DetailCache.Update(newdetail);
                    break;
                }
            }
        }

        protected virtual void IssueAvailable(PXCache sender, FSSODetService Row)
        {
            IssueAvailable(sender, Row, Row.BaseOpenQty);
        }

        protected override void _Master_RowInserted(PXCache sender, PXRowInsertedEventArgs<FSSODetService> e)
        {
            ILSMaster iLSMasterRow = (ILSMaster)e.Row;
            FSSODetService row = (FSSODetService)e.Row;
            if (iLSMasterRow.InventoryID == null || row.IsPrepaid == true)
            {
                return;
            }

            bool skipSplitCreating = false;

            if (IsLocationEnabled)
            {
                InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODetService.inventoryID>(sender, e.Row);
                if (ii != null && ii.StkItem == false && ii.KitItem == false && ii.NonStockShip == false)
                    skipSplitCreating = true;
            }

            if (!skipSplitCreating && (IsLocationEnabled || (IsLotSerialRequired && e.ExternalCall && IsLotSerialItem(sender, e.Row))))
            {
                base._Master_RowInserted(sender, e);
            }
            else
            {
                //sender.SetValue<FSSODetService.locationID>(e.Row, null);
                //sender.SetValue<FSSODetService.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODetService.expireDate>(e.Row, null);

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
                if (!PXUIFieldAttribute.GetErrors(sender, e.Row, PXErrorLevel.Error).Keys.Any(a => string.Compare(a, typeof(FSSODetService.uOM).Name, StringComparison.InvariantCultureIgnoreCase) == 0))
                    sender.RaiseExceptionHandling<FSSODetService.uOM>(e.Row, null, ex);
            }
        }

        protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<FSSODetService> e)
        {
            FSSODetService row = e.Row as FSSODetService;
            FSSODetService oldRow = e.OldRow as FSSODetService;
            if (row == null || (oldRow.InventoryID == null && row.InventoryID == null) || row.IsPrepaid == true) return;

            bool skipSplitCreating = false;
            InventoryItem ii = (InventoryItem)PXSelectorAttribute.Select<FSSODetService.inventoryID>(sender, row);

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
                    AvailabilityCheck(sender, (FSSODetService)e.Row);
                }
            }
            else
            {
                //sender.SetValue<FSSODetService.locationID>(e.Row, null);
                //sender.SetValue<FSSODetService.lotSerialNbr>(e.Row, null);
                sender.SetValue<FSSODetService.expireDate>(e.Row, null);

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

                        if (!sender.ObjectsEqual<FSSODetService.pOCreate, FSSODetService.pOSource, FSSODetService.vendorID, FSSODetService.poVendorLocationID, FSSODetService.pOSiteID>(e.Row, e.OldRow))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetServiceSplit split = detail as FSSODetServiceSplit;
                                if (split.IsAllocated == false && split.Completed == false && split.PONbr == null)
                                {
                                    split.POCreate = row.POCreate;
                                    split.POSource = row.POSource;
                                    split.VendorID = row.VendorID;
                                    split.POSiteID = row.POSiteID;

                                    DetailCache.Update(split);
                                }
                            }
                        }

                        if (!sender.ObjectsEqual<FSSODetService.shipDate>(e.Row, e.OldRow) ||
                            (!sender.ObjectsEqual<FSSODetService.shipComplete>(e.Row, e.OldRow) && ((FSSODetService)e.Row).ShipComplete != SOShipComplete.BackOrderAllowed))
                        {
                            foreach (object detail in SelectDetail(DetailCache, row))
                            {
                                FSSODetServiceSplit split = detail as FSSODetServiceSplit;
                                split.ShipDate = row.ShipDate;

                                DetailCache.Update(split);
                            }
                        }
                    }
                }

                AvailabilityCheck(sender, (FSSODetService)e.Row);
            }
        }

        protected virtual bool SchedulesEqual(FSSODetServiceSplit a, FSSODetServiceSplit b)
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
                    if (((FSSODetServiceSplit)e.Row).LineType != SOLineType.Inventory)
                    {
                        throw new PXSetPropertyException(ErrorMessages.CantInsertRecord);
                    }
                }

                base.Detail_RowInserting(sender, e);

                if (e.Row != null && !IsLocationEnabled && ((FSSODetServiceSplit)e.Row).LocationID != null)
                {
                    ((FSSODetServiceSplit)e.Row).LocationID = null;
                }
            }
            else if (IsAllocationEntryEnabled)
            {
                FSSODetServiceSplit a = (FSSODetServiceSplit)e.Row;

                if (!e.ExternalCall && _Operation == PXDBOperation.Update)
                {
                    foreach (object item in SelectDetail(sender, (FSSODetServiceSplit)e.Row))
                    {
                        FSSODetServiceSplit detailitem = (FSSODetServiceSplit)item;

                        if (SchedulesEqual((FSSODetServiceSplit)e.Row, detailitem))
                        {
                            object old_item = PXCache<FSSODetServiceSplit>.CreateCopy(detailitem);
                            detailitem.BaseQty += ((FSSODetServiceSplit)e.Row).BaseQty;
                            detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, (decimal)detailitem.BaseQty, INPrecision.QUANTITY);

                            detailitem.BaseUnreceivedQty += ((FSSODetServiceSplit)e.Row).BaseQty;
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

                if (((FSSODetServiceSplit)e.Row).InventoryID == null || string.IsNullOrEmpty(((FSSODetServiceSplit)e.Row).UOM))
                {
                    e.Cancel = true;
                }
            }
        }

        protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
        {
            FSSODetServiceSplit split = (FSSODetServiceSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
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
                    FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy(split);
                    if (split.BaseQty + accum.QtyHardAvail > 0m)
                    {
                        split.BaseQty += accum.QtyHardAvail;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<FSSODetServiceSplit.isAllocated>(split, true, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
                    }

                    sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), split, copy.IsAllocated);
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

            FSSODetServiceSplit split = (FSSODetServiceSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            FSSODetServiceSplit oldsplit = (FSSODetServiceSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).OldRow : ((PXRowInsertedEventArgs)e).Row);

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
                FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy(split);
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
                        sender.RaiseExceptionHandling<FSSODetServiceSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(IN.Messages.SerialNumberAlreadyReceived, ((InventoryItem)item).InventoryCD, split.LotSerialNbr)));
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
                            sender.RaiseExceptionHandling<FSSODetServiceSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
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

                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), split, copy.IsAllocated);
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

                        FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy(split);
                        split.IsAllocated = true;
                        sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), split, copy.IsAllocated);
                        sender.RaiseRowUpdated(split, copy);

                        return true;
                    }

                    //Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
                    if (split.IsAllocated == true)
                    {
                        FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy(split);
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
                                copy = (FSSODetServiceSplit)sender.Insert(copy);
                                if (copy.LotSerialNbr != null && copy.IsAllocated != true)
                                {
                                    sender.SetValue<FSSODetServiceSplit.lotSerialNbr>(copy, null);
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

        protected virtual bool NegativeInventoryError(PXCache sender, FSSODetServiceSplit split)
        {
            split.IsAllocated = false;
            split.LotSerialNbr = null;
            sender.RaiseExceptionHandling<FSSODetServiceSplit.lotSerialNbr>(split, null, new PXSetPropertyException(IN.Messages.Inventory_Negative2));
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

            if ((_InternallCall == false || !string.IsNullOrEmpty(((FSSODetServiceSplit)e.Row).LotSerialNbr) && ((FSSODetServiceSplit)e.Row).IsAllocated != true) && IsAllocationEntryEnabled)
            {
                if (((FSSODetServiceSplit)e.Row).IsAllocated == true || (!string.IsNullOrEmpty(((FSSODetServiceSplit)e.Row).LotSerialNbr) && ((FSSODetServiceSplit)e.Row).IsAllocated != true))
                {
                    Allocated_Updated(sender, e);

                    sender.RaiseExceptionHandling<FSSODetServiceSplit.qty>(e.Row, null, null);
                    AvailabilityCheck(sender, (FSSODetServiceSplit)e.Row);
                }
            }
        }

        protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Detail_RowUpdated(sender, e);

            if (_InternallCall == false && IsAllocationEntryEnabled)
            {
                if (!sender.ObjectsEqual<FSSODetServiceSplit.isAllocated>(e.Row, e.OldRow) || !sender.ObjectsEqual<FSSODetServiceSplit.pOLineNbr>(e.Row, e.OldRow) && ((FSSODetServiceSplit)e.Row).POLineNbr == null && ((FSSODetServiceSplit)e.Row).IsAllocated == false)
                {
                    if (((FSSODetServiceSplit)e.Row).IsAllocated == true)
                    {
                        Allocated_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetServiceSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetServiceSplit)e.Row);
                    }
                    else
                    {
                        //clear link to created transfer
                        FSSODetServiceSplit row = (FSSODetServiceSplit)e.Row;
                        row.SOOrderType = null;
                        row.SOOrderNbr = null;
                        row.SOLineNbr = null;
                        row.SOSplitLineNbr = null;

                        foreach (FSSODetServiceSplit s in this.SelectDetailReversed(sender, (FSSODetServiceSplit)e.Row))
                        {
                            if (s.SplitLineNbr != ((FSSODetServiceSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetServiceSplit)e.Row))
                            {
                                ((FSSODetServiceSplit)e.Row).Qty += s.Qty;
                                ((FSSODetServiceSplit)e.Row).BaseQty += s.BaseQty;

                                ((FSSODetServiceSplit)e.Row).UnreceivedQty += s.Qty;
                                ((FSSODetServiceSplit)e.Row).BaseUnreceivedQty += s.BaseQty;

                                if (((FSSODetServiceSplit)e.Row).LotSerialNbr != null)
                                {
                                    FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy((FSSODetServiceSplit)e.Row);
                                    ((FSSODetServiceSplit)e.Row).LotSerialNbr = null;
                                    //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), s, copy.IsAllocated);
                                    sender.RaiseRowUpdated((FSSODetServiceSplit)e.Row, copy);
                                }
                                sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCache();

                                PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                                INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(sender.Graph, ((FSSODetServiceSplit)e.Row).PlanID);
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
                            else if (s.SplitLineNbr == ((FSSODetServiceSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetServiceSplit)e.Row) && ((FSSODetServiceSplit)e.Row).LotSerialNbr != null)
                            {
                                FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy((FSSODetServiceSplit)e.Row);
                                ((FSSODetServiceSplit)e.Row).LotSerialNbr = null;
                                sender.RaiseRowUpdated((FSSODetServiceSplit)e.Row, copy);
                            }
                        }
                    }
                }

                if (!sender.ObjectsEqual<FSSODetServiceSplit.lotSerialNbr>(e.Row, e.OldRow))
                {
                    if (((FSSODetServiceSplit)e.Row).LotSerialNbr != null)
                    {
                        LotSerialNbr_Updated(sender, e);

                        sender.RaiseExceptionHandling<FSSODetServiceSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (FSSODetServiceSplit)e.Row); //???
                    }
                    else
                    {
                        foreach (FSSODetServiceSplit s in this.SelectDetailReversed(sender, (FSSODetServiceSplit)e.Row))
                        {
                            if (s.SplitLineNbr == ((FSSODetServiceSplit)e.Row).SplitLineNbr &&
                                SchedulesEqual(s, (FSSODetServiceSplit)e.Row))
                            {
                                FSSODetServiceSplit copy = PXCache<FSSODetServiceSplit>.CreateCopy(s);
                                ((FSSODetServiceSplit)e.Row).IsAllocated = false;
                                sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), (FSSODetServiceSplit)e.Row, ((FSSODetServiceSplit)e.Row).IsAllocated);
                                //sender.RaiseFieldUpdated(sender.GetField(typeof(FSSODetServiceSplit.isAllocated)), s, copy.IsAllocated);
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
                VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(FSSODetServiceSplit.qty).Name);
            }
        }



        public override FSSODetServiceSplit Convert(FSSODetService item)
        {
            using (InvtMultScope<FSSODetService> ms = new InvtMultScope<FSSODetService>(item))
            {
                FSSODetServiceSplit ret = (FSSODetServiceSplit)item;
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

        public virtual void FSSODetServiceSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            FSSODetServiceSplit split = e.Row as FSSODetServiceSplit;

            if (split != null)
            {
                bool isLineTypeInventory = (split.LineType == SOLineType.Inventory);
                object val = sender.GetValueExt<FSSODetServiceSplit.isAllocated>(e.Row);
                bool isAllocated = split.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
                bool isCompleted = split.Completed == true;
                bool isIssue = split.Operation == SOOperation.Issue;
                bool IsLinked = split.PONbr != null || split.SOOrderNbr != null && split.IsAllocated == true;

                FSSODetService parent = (FSSODetService)PXParentAttribute.SelectParent(sender, split, typeof(FSSODetService));
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.subItemID>(sender, e.Row, isLineTypeInventory);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.completed>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.shippedQty>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.shipmentNbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.isAllocated>(sender, e.Row, isLineTypeInventory && isIssue && !isCompleted);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.siteID>(sender, e.Row, isLineTypeInventory && isAllocated && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.qty>(sender, e.Row, !isCompleted && !IsLinked);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.shipDate>(sender, e.Row, !isCompleted && parent.ShipComplete == SOShipComplete.BackOrderAllowed);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.pONbr>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<FSSODetServiceSplit.pOReceiptNbr>(sender, e.Row, false);

                if (split.Completed == true)
                {
                    PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
                }
            }
        }

        public virtual void FSSODetServiceSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                bool RequireLocationAndSubItem = ((FSSODetServiceSplit)e.Row).RequireLocation == true && ((FSSODetServiceSplit)e.Row).IsStockItem == true && ((FSSODetServiceSplit)e.Row).BaseQty != 0m;

                PXDefaultAttribute.SetPersistingCheck<FSSODetServiceSplit.subItemID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                PXDefaultAttribute.SetPersistingCheck<FSSODetServiceSplit.locationID>(sender, e.Row, RequireLocationAndSubItem ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
            }
        }

        public virtual void FSSODetServiceSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetService)];
            if (cache.Current != null && (e.Row == null || ((FSSODetService)cache.Current).LineNbr == ((FSSODetServiceSplit)e.Row).LineNbr && ((FSSODetServiceSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSSODetService)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void FSSODetServiceSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetService)];
            if (cache.Current != null && (e.Row == null || ((FSSODetService)cache.Current).LineNbr == ((FSSODetServiceSplit)e.Row).LineNbr && ((FSSODetServiceSplit)e.Row).IsStockItem == true))
            {
                e.NewValue = ((FSSODetService)cache.Current).LocationID;
                e.Cancel = (_InternallCall == true || e.NewValue != null || !IsLocationEnabled);
            }
        }

        public virtual void FSSODetServiceSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(FSSODetService)];
            if (cache.Current != null && (e.Row == null || ((FSSODetService)cache.Current).LineNbr == ((FSSODetServiceSplit)e.Row).LineNbr))
            {
                using (InvtMultScope<FSSODetService> ms = new InvtMultScope<FSSODetService>((FSSODetService)cache.Current))
                {
                    e.NewValue = ((FSSODetService)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo ei)
        {
            if (row is FSSODetService)
            {
                sender.RaiseExceptionHandling<FSSODetService.orderQty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODetService.inventoryID>(row), sender.GetStateExt<FSSODetService.subItemID>(row), sender.GetStateExt<FSSODetService.siteID>(row), sender.GetStateExt<FSSODetService.locationID>(row), sender.GetValue<FSSODetService.lotSerialNbr>(row)));
            }
            else
            {
                sender.RaiseExceptionHandling<FSSODetServiceSplit.qty>(row, newValue, new PXSetPropertyException(ei.MessageFormat, PXErrorLevel.Warning, sender.GetStateExt<FSSODetServiceSplit.inventoryID>(row), sender.GetStateExt<FSSODetServiceSplit.subItemID>(row), sender.GetStateExt<FSSODetServiceSplit.siteID>(row), sender.GetStateExt<FSSODetServiceSplit.locationID>(row), sender.GetValue<FSSODetServiceSplit.lotSerialNbr>(row)));
            }
        }

        public virtual bool MemoAvailabilityCheck(PXCache sender, FSSODetService Row)
        {
            bool success = MemoAvailabilityCheckQty(sender, Row);
            
            return success;
        }

        protected virtual bool MemoAvailabilityCheckQty(PXCache sender, FSSODetService row)
        {
            return true;
        }

        protected virtual bool MemoAvailabilityCheck<Target>(PXCache sender, FSSODetService Row, ILSMaster Split)
            where Target : class, ILSMaster
        {
            bool success = true;
            return success;
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster Row)
        {
            base.AvailabilityCheck(sender, Row);

            if (Row is FSSODetService)
            {
                MemoAvailabilityCheck(sender, (FSSODetService)Row);

                FSSODetServiceSplit copy = Convert(Row as FSSODetService);

                if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                {
                    DefaultLotSerialNbr(sender.Graph.Caches[typeof(FSSODetServiceSplit)], copy);
                }

                MemoAvailabilityCheck<FSSODetService>(sender, (FSSODetService)Row, copy);

                if (copy.LotSerialNbr == null)
                {
                    Row.LotSerialNbr = null;
                }
            }
            else
            {
                object parent = PXParentAttribute.SelectParent(sender, Row, typeof(FSSODetService));
                MemoAvailabilityCheck(sender.Graph.Caches[typeof(FSSODetService)], (FSSODetService)parent);
                MemoAvailabilityCheck<FSSODetServiceSplit>(sender.Graph.Caches[typeof(FSSODetService)], (FSSODetService)parent, Row);
            }
        }

        public override void DefaultLotSerialNbr(PXCache sender, FSSODetServiceSplit row)
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

        public override PXSelectBase<INLotSerialStatus> GetSerialStatusCmd(PXCache sender, FSSODetService Row, PXResult<InventoryItem, INLotSerClass> item)
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

    public class FSSODetServiceSplitPlanIDAttribute : INItemPlanIDAttribute
    {
        #region State
        protected Type _ParentOrderDate;
        #endregion
        #region Ctor
        public FSSODetServiceSplitPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry, Type ParentOrderDate)
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
                PXCache splitcache = sender.Graph.Caches[typeof(FSSODetServiceSplit)];

                SOOrderType ordertype = PXSetup<SOOrderType>.Select(sender.Graph);

                var splitsByPlan = new Dictionary<long?, FSSODetServiceSplit>();
                foreach (FSSODetServiceSplit split in PXSelect<FSSODetServiceSplit,
                    Where<FSSODetServiceSplit.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>, And<FSSODetServiceSplit.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>>
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

                PXCache linecache = sender.Graph.Caches[typeof(FSSODetService)];

                foreach (FSSODetService line in PXSelect<FSSODetService,
                                                Where<FSSODetService.srvOrdType, 
                                                    Equal<Current<FSServiceOrder.srvOrdType>>, 
                                                And<FSSODetService.refNbr, 
                                                    Equal<Current<FSServiceOrder.refNbr>>,
                                                And<
                                                    Where<FSSODetService.lineType, Equal<FSSODetService.lineType.Service>,
                                                    Or<FSSODetService.lineType, Equal<FSSODetService.lineType.NonStockItem>>>>>>>
                    .SelectMultiBound(sender.Graph, new[] { e.Row }))
                {
                    if (Cancelled)
                    {
                        FSSODetService old_row = PXCache<FSSODetService>.CreateCopy(line);
                        //line.UnbilledQty -= line.OpenQty;
                        line.OpenQty = 0m;
                        //linecache.RaiseFieldUpdated<FSSODetService.unbilledQty>(line, 0m);
                        linecache.RaiseFieldUpdated<FSSODetService.openQty>(line, 0m);

                        line.Completed = true;
                        LSFSSODetServiceLine.ResetAvailabilityCounters(line);

                        //SOOrderEntry_SOOrder_RowUpdated should execute later to correctly update balances
                        //+++//TaxAttribute.Calculate<FSSODetService.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

                        linecache.MarkUpdated(line);
                    }
                    else
                    {
                        if ((string)sender.GetValue<FSServiceOrder.status>(e.OldRow) == ID.Status_ServiceOrder.CANCELED)
                        {
                            FSSODetService old_row = PXCache<FSSODetService>.CreateCopy(line);
                            line.OpenQty = line.OrderQty;
                            /*line.UnbilledQty += line.OpenQty;
                            object value = line.UnbilledQty;
                            linecache.RaiseFieldVerifying<FSSODetService.unbilledQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODetService.unbilledQty>(line, value);*/

                            object value = line.OpenQty;
                            linecache.RaiseFieldVerifying<FSSODetService.openQty>(line, ref value);
                            linecache.RaiseFieldUpdated<FSSODetService.openQty>(line, value);

                            line.Completed = false;

                            //+++++//
                            //TaxAttribute.Calculate<FSSODetService.taxCategoryID>(linecache, new PXRowUpdatedEventArgs(line, old_row, false));

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
                            LSFSSODetServiceLine.ResetAvailabilityCounters(line);
                        }
                    }
                }

                if (Cancelled)
                {
                    //PXFormulaAttribute.CalcAggregate<FSSODetService.unbilledQty>(linecache, e.Row);
                    PXFormulaAttribute.CalcAggregate<FSSODetService.openQty>(linecache, e.Row);
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

                        FSSODetServiceSplit split;
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
                foreach (FSSODetServiceSplit split in PXSelect<FSSODetServiceSplit, Where<FSSODetServiceSplit.srvOrdType, Equal<Required<FSServiceOrder.srvOrdType>>, And<FSSODetServiceSplit.refNbr, Equal<Required<FSServiceOrder.refNbr>>>>>.Select(sender.Graph, ((FSServiceOrder)e.Row).SrvOrdType, _KeyToAbort))
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
            var IsLinked = IsLineLinked((FSSODetServiceSplit)e.Row);

            InitPlan = InitPlanRequired(sender, e) && !IsLinked;
            FSSODetService parent = (FSSODetService)PXParentAttribute.SelectParent(sender, e.Row, typeof(FSSODetService));

            InitVendor = !sender.ObjectsEqual<FSSODetServiceSplit.siteID, FSSODetServiceSplit.subItemID, FSSODetServiceSplit.vendorID, FSSODetServiceSplit.pOCreate>(e.Row, e.OldRow) &&
                !IsLinked;
            ResetSupplyPlanID = !IsLinked;

            InitVendor = InitVendor || parent.POVendorLocationID != null;

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
                .ObjectsEqual<FSSODetServiceSplit.isAllocated,
                    FSSODetServiceSplit.siteID,
                    FSSODetServiceSplit.pOCreate,
                    FSSODetServiceSplit.pOSource,
                    FSSODetServiceSplit.operation>(e.Row, e.OldRow);
        }

        protected virtual bool IsLineLinked(FSSODetServiceSplit soLineSplit)
        {
            return soLineSplit != null && (soLineSplit.PONbr != null || soLineSplit.SOOrderNbr != null && soLineSplit.IsAllocated == true);
        }

        public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
        {
            if (((FSSODetServiceSplit)orig_Row).Completed == true || ((FSSODetServiceSplit)orig_Row).POCompleted == true || ((FSSODetServiceSplit)orig_Row).LineType == SOLineType.MiscCharge || ((FSSODetServiceSplit)orig_Row).LineType == SOLineType.NonInventory && ((FSSODetServiceSplit)orig_Row).RequireShipping == false)
            {
                return null;
            }
            FSSODetService parent = (FSSODetService)PXParentAttribute.SelectParent(sender, orig_Row, typeof(FSSODetService));
            FSServiceOrder order = (FSServiceOrder)PXParentAttribute.SelectParent(sender, orig_Row, typeof(FSServiceOrder));

            FSSODetServiceSplit split_Row = (FSSODetServiceSplit)orig_Row;

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

        protected virtual string CalcPlanType(PXCache sender, INItemPlan plan, FSServiceOrder order, FSSODetServiceSplit split, bool? backOrdered = null)
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

        protected virtual string CalcPlanType(INItemPlan plan, FSSODetServiceSplit split, SOOrderType ordertype, bool isOrderOnHold)
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
