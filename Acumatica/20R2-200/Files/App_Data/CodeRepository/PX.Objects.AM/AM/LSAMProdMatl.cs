using System;
using PX.Objects.IN;
using PX.Data;
using System.Collections.Generic;
using PX.Objects.AM.Attributes;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using SiteLotSerial = PX.Objects.IN.Overrides.INDocumentRelease.SiteLotSerial;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMProdMatl : LSSelect<AMProdMatl, AMProdMatlSplit,
        Where<AMProdMatlSplit.orderType, Equal<Current<AMProdMatl.orderType>>,
        And<AMProdMatlSplit.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
        And<AMProdMatlSplit.operationID, Equal<Current<AMProdMatl.operationID>>,
        And<AMProdMatlSplit.lineID, Equal<Current<AMProdMatl.lineID>>>>>>>
    {
        public LSAMProdMatl(PXGraph graph) : base(graph)
        {
            MasterQtyField = typeof(AMProdMatl.qtyRemaining);
            graph.FieldDefaulting.AddHandler<AMProdMatlSplit.subItemID>(new PXFieldDefaulting(this.AMProdMatlSplit_SubItemID_FieldDefaulting));
            graph.FieldDefaulting.AddHandler<AMProdMatlSplit.locationID>(new PXFieldDefaulting(this.AMProdMatlSplit_LocationID_FieldDefaulting));
            graph.FieldDefaulting.AddHandler<AMProdMatlSplit.invtMult>(new PXFieldDefaulting(this.AMProdMatlSplit_InvtMult_FieldDefaulting));
            graph.RowSelected.AddHandler<AMProdMatlSplit>(AMProdMatlSplit_RowSelected);
        }

        public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(AMProdMatlSplit.qty).Name);
        }

        public override AMProdMatlSplit Convert(AMProdMatl item)
        {
            using (new InvtMultScope<AMProdMatl>(item))
            {
                AMProdMatlSplit ret = (AMProdMatlSplit)item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
            }
        }

#if DEBUG
        // Copy from PX.Objects.SO.LSSOLine (18.112.0019)
#endif
        public bool AvailabilityFetching { get; private set; }
        public override IQtyAllocated AvailabilityFetch(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
        {
            try
            {
                AvailabilityFetching = true;
                return AvailabilityFetchImpl(sender, Row, fetchMode);
            }
            finally
            {
                AvailabilityFetching = false;
            }
        }

#if DEBUG
        // Copy/update from PX.Objects.SO.LSSOLine (18.112.0019)
#endif
        public virtual IQtyAllocated AvailabilityFetchImpl(PXCache sender, ILSMaster Row, AvailabilityFetchMode fetchMode)
        {
            if (Row != null)
            {
                AMProdMatlSplit copy = Row as AMProdMatlSplit;
                if (copy == null)
                {
                    copy = Convert(Row as AMProdMatl);

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(AMProdMatl), Row);

                    if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                    {
                        DefaultLotSerialNbr(sender.Graph.Caches[typeof(AMProdMatlSplit)], copy);
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.TryOptimize) && _detailsRequested++ == 5)
                    {
                        foreach (PXResult<AMProdMatl, INUnit, INSiteStatus> res in 
                            PXSelectReadonly2<AMProdMatl,
                            InnerJoin<INUnit, On<
                                INUnit.inventoryID, Equal<AMProdMatl.inventoryID>,
                                And<INUnit.fromUnit, Equal<AMProdMatl.uOM>>>,
                            InnerJoin<INSiteStatus, On<
                                AMProdMatl.inventoryID, Equal<INSiteStatus.inventoryID>,
                                And<AMProdMatl.subItemID, Equal<INSiteStatus.subItemID>,
                                And<AMProdMatl.siteID, Equal<INSiteStatus.siteID>>>>>>,
                            Where<AMProdMatl.orderType, Equal<Current<AMProdMatl.orderType>>,
                                And<AMProdMatl.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>>>>
                            .Select(sender.Graph))
                        {
                            INSiteStatus status = res;
                            INUnit unit = res;

                            PXSelectReadonly<INUnit,
                                Where<INUnit.unitType, Equal<INUnitType.inventoryItem>,
                                And<INUnit.inventoryID, Equal<Required<INUnit.inventoryID>>,
                                And<INUnit.toUnit, Equal<Required<INUnit.toUnit>>,
                                And<INUnit.fromUnit, Equal<Required<INUnit.fromUnit>>>>>>>
                                .StoreCached(sender.Graph, new PXCommandKey(new object[] { unit.InventoryID, unit.ToUnit, unit.FromUnit }), new List<object> { unit });

                            INSiteStatus.PK.StoreCached(sender.Graph, status);
                        }

                        foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<AMProdMatl.noteID>>>>.Select(this._Graph))
                        {
                            PXSelect<INItemPlan,
                                Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                                .StoreCached(this._Graph, new PXCommandKey(new object[] { plan.PlanID }), new List<object> { plan });
                        }
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
                        return AvailabilityFetch(sender, (AMProdMatl)Row, copy);
                }
                return AvailabilityFetch(sender, copy, fetchMode);
            }
            return null;
        }

#if DEBUG
        // Copy/update from PX.Objects.SO.LSSOLine (18.112.0019)
#endif
        private IQtyAllocated AvailabilityFetch(PXCache sender, AMProdMatl line, AMProdMatlSplit detail)
        {
            var result = AvailabilityFetch(sender, detail, AvailabilityFetchMode.None);
            if (result == null)
            {
                return null;
            }

            decimal? lineQtyAvail = (decimal?)sender.GetValue<AMProdMatl.lineQtyAvail>(line);
            decimal? lineQtyHardAvail = (decimal?)sender.GetValue<AMProdMatl.lineQtyHardAvail>(line);

            if (lineQtyAvail == null || lineQtyHardAvail == null)
            {
                lineQtyAvail = 0m;
                lineQtyHardAvail = 0m;

                foreach (AMProdMatlSplit split in SelectDetail(DetailCache, line))
                {
                    detail = split;
                    if (detail.PlanID != null)
                    {
                        INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, detail.PlanID);
                        if (plan != null)
                        {
                            detail = PXCache<AMProdMatlSplit>.CreateCopy(detail);
                            //detail.PlanType = plan.PlanType;
                        }
                    }

                    PXParentAttribute.SetParent(DetailCache, detail, typeof(AMProdMatl), line);

                    decimal signQtyAvail;
                    decimal signQtyHardAvail;
                    INItemPlanIDAttribute.GetInclQtyAvail<SiteStatus>(DetailCache, detail, out signQtyAvail, out signQtyHardAvail);

                    if (signQtyAvail != 0m)
                    {
                        lineQtyAvail -= signQtyAvail * (detail.BaseQty ?? 0m);
                    }

                    if (signQtyHardAvail != 0m)
                    {
                        lineQtyHardAvail -= signQtyHardAvail * (detail.BaseQty ?? 0m);
                    }
                }

                sender.SetValue<AMProdMatl.lineQtyAvail>(line, lineQtyAvail);
                sender.SetValue<AMProdMatl.lineQtyHardAvail>(line, lineQtyHardAvail);
            }

            result.QtyAvail += lineQtyAvail;
            result.QtyHardAvail += lineQtyHardAvail;
            result.QtyNotAvail = -lineQtyAvail;

            return result;
        }

#if DEBUG
        // Copy/update from PX.Objects.SO.LSSOLine (18.112.0019)
#endif
        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            var fetchMode = IsMaterialCompleted((AMProdMatl)e.Row)
                ? AvailabilityFetchMode.None
                : AvailabilityFetchMode.ExcludeCurrent;
            IQtyAllocated availability = AvailabilityFetch(sender, (AMProdMatl)e.Row, fetchMode | AvailabilityFetchMode.TryOptimize);

            if (availability != null)
            {
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((AMProdMatl)e.Row).InventoryID);

                var unitRate = INUnitAttribute.ConvertFromBase<AMProdMatl.inventoryID, AMProdMatl.uOM>(sender, e.Row, 1m, INPrecision.NOROUND);
                availability.QtyOnHand = PXDBQuantityAttribute.Round(availability.QtyOnHand.GetValueOrDefault() * unitRate);
                availability.QtyAvail = PXDBQuantityAttribute.Round(availability.QtyAvail.GetValueOrDefault() * unitRate);
                availability.QtyNotAvail = PXDBQuantityAttribute.Round(availability.QtyNotAvail.GetValueOrDefault() * unitRate);
                availability.QtyHardAvail = PXDBQuantityAttribute.Round(availability.QtyHardAvail.GetValueOrDefault() * unitRate);

                var allocated = PXDBQuantityAttribute.Round((((AMProdMatl)e.Row).LineQtyHardAvail ?? 0m) * unitRate);
                e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(PX.Objects.SO.Messages.Availability_AllocatedInfo,
                            sender.GetValue<AMProdMatl.uOM>(e.Row), FormatQty(availability.QtyOnHand), FormatQty(availability.QtyAvail), FormatQty(availability.QtyHardAvail), FormatQty(allocated));

                AvailabilityCheck(sender, (AMProdMatl)e.Row, availability);
            }
            else
            {
                //handle missing UOM
                INUnitAttribute.ConvertFromBase<AMProdMatl.inventoryID, AMProdMatl.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                e.ReturnValue = string.Empty;
            }

            base.Availability_FieldSelecting(sender, e);
        }

        protected virtual bool IsMaterialCompleted(AMProdMatl row)
        {
            return row != null && (row.QtyRemaining.GetValueOrDefault() == 0m || row.StatusID == ProductionOrderStatus.Completed || row.StatusID == ProductionOrderStatus.Closed || row.StatusID == ProductionOrderStatus.Cancel);
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            var row = (AMProdMatl)e.Row;
            if (row == null)
            {
                return;
            }

            if (row.InvtMult != 0)
            {
                base.Master_RowInserted(sender, e);

                // Field defaults on POCreate and ProdCreate not setting the splits... need this
                UpdateNonAllocatedSplits(row, typeof(AMProdMatlSplit.pOCreate), typeof(AMProdMatlSplit.prodCreate));
            }
            else
            {
                sender.SetValue<AMProdMatl.lotSerialNbr>(e.Row, null);
                sender.SetValue<AMProdMatl.expireDate>(e.Row, null);
            }
        }

        protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
        {
            return INLotSerTrack.Mode.None;
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMProdMatl)e.Row;
            var rowOld = (AMProdMatl)e.OldRow;
            if (row == null || rowOld == null)
            {
                return;
            }

            if (!sender.ObjectsEqual<AMProdMatl.statusID, AMProdMatl.pOCreate, AMProdMatl.prodCreate, AMProdMatl.tranType, AMProdMatl.invtMult, AMProdMatl.siteID, AMProdMatl.locationID>(row, rowOld))
            {
                UpdateNonAllocatedSplits(row, typeof(AMProdMatlSplit.pOCreate), typeof(AMProdMatlSplit.prodCreate), typeof(AMProdMatlSplit.tranType),
                    typeof(AMProdMatlSplit.invtMult), typeof(AMProdMatlSplit.siteID), typeof(AMProdMatlSplit.locationID));
            }

            base.Master_RowUpdated(sender, e);
        }

        protected override void _Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs<AMProdMatl> e)
        {
            if (e.Row == null)
            {
                return;
            }

            var skipSplitCreating = false;
            var ii = (InventoryItem)PXSelectorAttribute.Select<AMProdMatl.inventoryID>(sender, e.Row);

            if (ii != null && ii.StkItem == false)
            {
                skipSplitCreating = true;
            }

            if (!skipSplitCreating && IsLotSerialItem(sender, e.Row)) //check condition
            {
                base._Master_RowUpdated(sender, e);

                if (ii != null && (ii.KitItem == true || ii.StkItem == true))
                {
                    AvailabilityCheck(sender, (AMProdMatl)e.Row);
                }
            }
            else
            {
                sender.SetValue<AMProdMatl.lotSerialNbr>(e.Row, null);
                sender.SetValue<AMProdMatl.expireDate>(e.Row, null);
                                
                PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, e.Row.InventoryID);
                
                if (e.OldRow != null && (e.OldRow.InventoryID != e.Row.InventoryID || e.OldRow.SubItemID != e.Row.SubItemID
                    || e.OldRow.InvtMult != e.Row.InvtMult || e.OldRow.UOM != e.Row.UOM))
                {
                    RaiseRowDeleted(sender, e.OldRow);
                    RaiseRowInserted(sender, e.Row);
                }
                else if (e.OldRow != null && e.OldRow.SiteID != e.Row.SiteID)
                {
                    UpdateNonAllocatedSplits(e.Row, typeof(AMProdMatlSplit.siteID), typeof(AMProdMatlSplit.locationID));
                }
                else if (item != null)
                {
                    if (e.Row.BaseQtyRemaining > (e.OldRow?.BaseQtyRemaining ?? 0m))
                    {
                        UpdateSplits(sender, e.Row, (decimal)e.Row.BaseQtyRemaining - (decimal)e.OldRow.BaseQtyRemaining);
                        UpdateParent(sender, e.Row);
                    }
                    if (e.Row.BaseQtyRemaining < (e.OldRow?.BaseQtyRemaining ?? 0m))
                    {
                        TruncateSplits(sender, e.Row, (decimal)e.OldRow.BaseQtyRemaining - (decimal)e.Row.BaseQtyRemaining);
                        UpdateParent(sender, e.Row);
                    }
                }

                AvailabilityCheck(sender, e.Row);
            }
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            var row = (AMProdMatlSplit)e.Row;
            if (row != null && _InternallCall && _Operation == PXDBOperation.Update)
            {
                foreach (var item in SelectDetail(sender, (AMProdMatlSplit)e.Row))
                {
                    var detailitem = (AMProdMatlSplit)item;

                    if (Detail_ObjectsEqual(row, detailitem))
                    {
                        object oldDetailItem = PXCache<AMProdMatlSplit>.CreateCopy(detailitem);
#if DEBUG
                        // PX1048 - Only the DAC instance that is passed in the event arguments can be modified in the RowInserting
                        // Ignoring as this is just a copy and paste from the base class to get serial tracked items to also merge as an update (PX1048 exists in base code)
#endif
#pragma warning disable PX1048
                        detailitem.BaseQty += row.BaseQty.GetValueOrDefault();
                        detailitem.Qty = INUnitAttribute.ConvertFromBase(sender, detailitem.InventoryID, detailitem.UOM, detailitem.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
#pragma warning restore PX1048
                        sender.Current = detailitem;
                        sender.RaiseRowUpdated(detailitem, oldDetailItem);
                        sender.MarkUpdated(detailitem);
                        e.Cancel = true;
                        break;
                    }
                }
            }

            if(e.Cancel)
            {
                return;
            }

            base.Detail_RowInserting(sender, e);
        }

        protected virtual void UpdateNonAllocatedSplits(AMProdMatl row, params Type[] fieldTypes)
        {
            foreach (AMProdMatlSplit split in SelectDetail(DetailCache, row))
            {
                if (split.IsAllocated.GetValueOrDefault())
                {
                    continue;
                }

                UpdateSplit(row, split, fieldTypes);
            }
        }

        protected virtual void UpdateSplit(AMProdMatl masterRow, AMProdMatlSplit split, params Type[] fieldTypes)
        { 
            if (fieldTypes == null)
            {
                return;
            }

            var detailRow = PXCache<AMProdMatlSplit>.CreateCopy(split);
            foreach (var fieldType in fieldTypes)
            {
                var masterCacheValue = MasterCache.GetValue(masterRow, fieldType.Name);
#if DEBUG
                AMDebug.TraceWriteMethodName($"fieldType.Name = {fieldType.Name}; AMProdMatl value = {masterCacheValue}; AMProdMatlSplit value = {DetailCache.GetValue(detailRow, fieldType.Name)}");
#endif
                DetailCache.SetValue(detailRow, fieldType.Name, masterCacheValue);
            }
            DetailCache.Update(detailRow);
        }

        protected virtual void UpdateSplits(PXCache cache, AMProdMatl row, decimal? baseQtyChange)
        {
            if (baseQtyChange.GetValueOrDefault() == 0 || row?.InventoryID == null)
            {
                return;
            }

            DetailCounters.Remove(row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(cache, row.InventoryID);

            var remainingChangeQty = baseQtyChange;
            foreach (AMProdMatlSplit split in SelectDetail(DetailCache, row))
            {
                if (split == null || split.IsAllocated == true || split.BaseQty.GetValueOrDefault() < 0)
                {
                    continue;
                }

                var newBaseQty = split.BaseQty.GetValueOrDefault() + remainingChangeQty;
                if (newBaseQty < 0m)
                {
                    remainingChangeQty = newBaseQty;
                    newBaseQty = 0m;
                }
                else
                {
                    remainingChangeQty = 0m;
                }

                split.BaseQty = newBaseQty;
                if ((InventoryItem)item == null || split.UOM == ((InventoryItem)item).BaseUnit)
                {
                    split.Qty = split.BaseQty;
                }
                else
                {
                    split.Qty = INUnitAttribute.ConvertFromBase(DetailCache, split.InventoryID, split.UOM, split.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
                }

                DetailCache.Update(split);

                if (remainingChangeQty == 0m)
                {
                    return;
                }
            }
        }

        protected virtual void IssueAvailable(PXCache sender, AMProdMatl Row, decimal? BaseQty)
        {
            DetailCounters.Remove(Row);
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);

            foreach (INSiteStatus avail in PXSelectReadonly<INSiteStatus,
                Where<INSiteStatus.inventoryID, Equal<Required<INSiteStatus.inventoryID>>,
                And<INSiteStatus.subItemID, Equal<Required<INSiteStatus.subItemID>>,
                And<INSiteStatus.siteID, Equal<Required<INSiteStatus.siteID>>>>>,
                OrderBy<Asc<INLocation.pickPriority>>>.Select(this._Graph, Row.InventoryID, Row.SubItemID, Row.SiteID))
            {
                AMProdMatlSplit split = (AMProdMatlSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
                split.SplitLineNbr = null;
                split.IsAllocated = true; // Does row require allocation => true;
                split.SiteID = Row.SiteID;

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

            if (BaseQty > 0m && Row.InventoryID != null && Row.SiteID != null && (Row.SubItemID != null || (Row.SubItemID == null && Row.IsStockItem != true)))
            {
                AMProdMatlSplit split = (AMProdMatlSplit)Row;
                if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
                {
                    split.UOM = ((InventoryItem)item).BaseUnit;
                }
                split.SplitLineNbr = null;
                split.IsAllocated = false;
                split.BaseQty = BaseQty;
                split.Qty = INUnitAttribute.ConvertFromBase(MasterCache, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);

                BaseQty = 0m;

                DetailCache.Insert(PXCache<AMProdMatlSplit>.CreateCopy(split));
            }
        }

        public virtual void TruncateSplits(PXCache sender, AMProdMatl Row, decimal BaseQty)
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
                    AMProdMatlSplit newdetail = PXCache<AMProdMatlSplit>.CreateCopy((AMProdMatlSplit)detail);
                    newdetail.BaseQty -= BaseQty;
                    newdetail.Qty = INUnitAttribute.ConvertFromBase(sender, newdetail.InventoryID, newdetail.UOM, (decimal)newdetail.BaseQty, INPrecision.QUANTITY);

                    DetailCache.Update(newdetail);
                    break;
                }
            }
        }

        public virtual void AMProdMatl_QtyRemaining_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if (((decimal?)e.NewValue) < 0M)
            {
                throw new PXSetPropertyException(Messages.EntryGreaterEqualZero, PXErrorLevel.Error, new object[] { 0 });
            }
        }

        public void AMProdMatlSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdMatl)];
            if ((cache.Current != null) && ((e.Row == null) || (((AMProdMatl)cache.Current).ProdOrdID == ((AMProdMatlSplit)e.Row).ProdOrdID)))
            {
                e.NewValue = ((AMProdMatl)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public void AMProdMatlSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdMatl)];
            if (cache.Current != null && (e.Row == null || ((AMProdMatl)cache.Current).ProdOrdID == ((AMProdMatlSplit)e.Row).ProdOrdID))
            {
                using (new InvtMultScope<AMProdMatl>((AMProdMatl)cache.Current))
                {
                    e.NewValue = ((AMProdMatl)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }

        public void AMProdMatlSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdMatl)];
            if ((cache.Current != null) && ((e.Row == null) || (((AMProdMatl)cache.Current).ProdOrdID == ((AMProdMatlSplit)e.Row).ProdOrdID)))
            {
                e.NewValue = ((AMProdMatl)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMProdMatl)
            {
                sender.RaiseExceptionHandling<AMProdMatl.qtyRemaining>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, new object[] 
                    { sender.GetStateExt<AMProdMatl.inventoryID>(row), sender.GetStateExt<AMProdMatl.subItemID>(row),
                        sender.GetStateExt<AMProdMatl.siteID>(row), sender.GetStateExt<AMProdMatl.locationID>(row), sender.GetValue<AMProdMatl.lotSerialNbr>(row) }));
            }
            else
            {
                sender.RaiseExceptionHandling<AMProdMatlSplit.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, new object[] 
                    { sender.GetStateExt<AMProdMatlSplit.inventoryID>(row), sender.GetStateExt<AMProdMatlSplit.subItemID>(row),
                        sender.GetStateExt<AMProdMatlSplit.siteID>(row), sender.GetStateExt<AMProdMatlSplit.locationID>(row),
                            sender.GetValue<AMProdMatlSplit.lotSerialNbr>(row) }));
            }
        }

        protected override void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Detail_RowUpdated(sender, e);

            if (_InternallCall == false)
            {
                if (!sender.ObjectsEqual<AMProdMatlSplit.isAllocated>(e.Row, e.OldRow))
                {
                    if (((AMProdMatlSplit)e.Row).IsAllocated == true)
                    {
                        Allocated_Updated(sender, e);

                        sender.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (AMProdMatlSplit)e.Row);
                    }
                    else
                    {
                        foreach (AMProdMatlSplit s in this.SelectDetailReversed(sender, (AMProdMatlSplit)e.Row))
                        {
                            if (s.SplitLineNbr != ((AMProdMatlSplit)e.Row).SplitLineNbr)
                            {
                                var baseQty = (s.BaseQty - s.BaseQtyReceived).NotLessZero();
                                ((AMProdMatlSplit)e.Row).Qty += (s.Qty - s.QtyReceived).NotLessZero();
                                ((AMProdMatlSplit)e.Row).BaseQty += baseQty;
                                ((AMProdMatlSplit)e.Row).RefNoteID = null;
                                ((AMProdMatlSplit)e.Row).POReceiptNbr = null;
                                ((AMProdMatlSplit)e.Row).POReceiptType = null;
                                ((AMProdMatlSplit)e.Row).POOrderType = null;
                                ((AMProdMatlSplit)e.Row).POOrderNbr = null;
                                ((AMProdMatlSplit)e.Row).POLineNbr = null;
                                ((AMProdMatlSplit)e.Row).VendorID = null;

                                if (!string.IsNullOrWhiteSpace(((AMProdMatlSplit)e.Row).LotSerialNbr))
                                {
                                    AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy((AMProdMatlSplit)e.Row);
                                    ((AMProdMatlSplit)e.Row).LotSerialNbr = null;
                                    
                                    sender.RaiseRowUpdated((AMProdMatlSplit)e.Row, copy);
                                }
                                sender.SetStatus(s, sender.GetStatus(s) == PXEntryStatus.Inserted ? PXEntryStatus.InsertedDeleted : PXEntryStatus.Deleted);
                                sender.ClearQueryCache();

                                PXCache cache = sender.Graph.Caches[typeof(INItemPlan)];
                                INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>
                                    >>.Select(sender.Graph, ((AMProdMatlSplit)e.Row).PlanID);
                                if (plan != null)
                                {
                                    plan.PlanQty += baseQty;
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
                            else if (s.SplitLineNbr == ((AMProdMatlSplit)e.Row).SplitLineNbr && !string.IsNullOrWhiteSpace(((AMProdMatlSplit)e.Row).LotSerialNbr))
                            {
                                AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy((AMProdMatlSplit)e.Row);
                                ((AMProdMatlSplit)e.Row).LotSerialNbr = null;
                                sender.RaiseRowUpdated((AMProdMatlSplit)e.Row, copy);
                            }
                        }
                    }
                }

                if (!sender.ObjectsEqual<AMProdMatlSplit.lotSerialNbr>(e.Row, e.OldRow))
                {
                    if (((AMProdMatlSplit)e.Row).LotSerialNbr != null)
                    {
                        LotSerialNbr_Updated(sender, e);

                        sender.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
                        AvailabilityCheck(sender, (AMProdMatlSplit)e.Row); 
                    }
                    else
                    {
                        foreach (AMProdMatlSplit s in this.SelectDetailReversed(sender, (AMProdMatlSplit)e.Row))
                        {
                            if (s.SplitLineNbr == ((AMProdMatlSplit)e.Row).SplitLineNbr)
                            {
                                AMProdMatlSplit copy = PXCache<AMProdMatlSplit>.CreateCopy(s);
                                ((AMProdMatlSplit)e.Row).IsAllocated = false;
                                sender.RaiseFieldUpdated(sender.GetField(typeof(AMProdMatlSplit.isAllocated)), (AMProdMatlSplit)e.Row, ((AMProdMatlSplit)e.Row).IsAllocated);
                                sender.RaiseRowUpdated(s, copy);
                            }
                        }
                    }
                }

            }
        }

        protected virtual bool Allocated_Updated(PXCache sender, EventArgs e)
        {
            var split = (AMProdMatlSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            if (split == null)
            {
                return false;
            }

            var accum = new SiteStatus
            {
                InventoryID = split.InventoryID,
                SiteID = split.SiteID,
                SubItemID = split.SubItemID
            };

            accum = (SiteStatus)sender.Graph.Caches[typeof(SiteStatus)].Insert(accum);
            accum = PXCache<SiteStatus>.CreateCopy(accum);

            var stat = INSiteStatus.PK.Find(sender.Graph, split.InventoryID, split.SubItemID, split.SiteID);
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
                    var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
                    if (split.BaseQty + accum.QtyHardAvail > 0m)
                    {
                        split.BaseQty += accum.QtyHardAvail;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        sender.RaiseFieldUpdated(sender.GetField(typeof(AMProdMatlSplit.qty)), split, split.Qty);
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<AMProdMatlSplit.isAllocated>(split, true, new PXSetPropertyException(PX.Objects.IN.Messages.Inventory_Negative2));
                    }

                    sender.RaiseFieldUpdated(sender.GetField(typeof(AMProdMatlSplit.isAllocated)), split, copy.IsAllocated);
                    sender.RaiseRowUpdated(split, copy);

                    if (split.IsAllocated == true)
                    {
                        copy.SplitLineNbr = null;
                        copy.PlanID = null;
                        copy.IsAllocated = false;
                        copy.BaseQty = -accum.QtyHardAvail;
                        copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, copy.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
                        copy.QtyComplete = null;
                        copy.BaseQtyComplete = null;
                        copy.QtyReceived = null;
                        copy.BaseQtyReceived = null;

                        sender.Insert(copy);
                    }
                    RefreshView(sender);

                    return true;
                }
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

        protected override object[] SelectDetailReversed(PXCache sender, AMProdMatlSplit row)
        {
            return SelectDetailReversed(sender, row, true);
        }

        protected virtual object[] SelectDetailReversed(PXCache sender, AMProdMatlSplit row, bool ExcludeCompleted = true)
        {
            object[] ret = SelectDetail(sender, row, ExcludeCompleted);

            Array.Sort<object>(ret, new Comparison<object>(delegate (object a, object b)
            {
                object aIsAllocated = ((AMProdMatlSplit)a).Completed == true ? 0 : ((AMProdMatlSplit)a).IsAllocated == true ? 1 : 2;
                object bIsAllocated = ((AMProdMatlSplit)b).Completed == true ? 0 : ((AMProdMatlSplit)b).IsAllocated == true ? 1 : 2;

                int res = -((IComparable)aIsAllocated).CompareTo(bIsAllocated);

                if (res != 0)
                {
                    return res;
                }

                object aSplitLineNbr = ((AMProdMatlSplit)a).SplitLineNbr;
                object bSplitLineNbr = ((AMProdMatlSplit)b).SplitLineNbr;

                return -((IComparable)aSplitLineNbr).CompareTo(bSplitLineNbr);
            }));

            return ret;
        }

        protected int _detailsRequested = 0;

        protected override object[] SelectDetail(PXCache sender, AMProdMatlSplit row)
        {
            return SelectDetail(sender, row, true);
        }

        protected virtual object[] SelectDetail(PXCache sender, AMProdMatlSplit row, bool ExcludeCompleted = true)
        {
            object[] ret;
            if (_detailsRequested > 5)
            {
                ret = PXParentAttribute.SelectSiblings(sender, row, typeof(AMProdOper));

                return Array.FindAll(ret, a =>
                    SameInventoryItem((AMProdMatlSplit)a, row) && ((AMProdMatlSplit)a).LineID == row.LineID && ((AMProdMatlSplit)a).Completed == false);
            }

            ret = base.SelectDetail(sender, row);
            return Array.FindAll<object>(ret, a => (((AMProdMatlSplit)a).Completed == false));
        }

        protected override void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            base.Detail_RowInserted(sender, e);

            if ((_InternallCall == false || !string.IsNullOrEmpty(((AMProdMatlSplit)e.Row).LotSerialNbr) && ((AMProdMatlSplit)e.Row).IsAllocated != true))
            {
                if (((AMProdMatlSplit)e.Row).IsAllocated == true || (!string.IsNullOrEmpty(((AMProdMatlSplit)e.Row).LotSerialNbr) 
                    && ((AMProdMatlSplit)e.Row).IsAllocated != true))
                {
                    Allocated_Updated(sender, e);

                    sender.RaiseExceptionHandling<AMProdMatlSplit.qty>(e.Row, null, null);
                    AvailabilityCheck(sender, (AMProdMatlSplit)e.Row);
                }
            }

            PXCache cache = sender.Graph.Caches[typeof(AMProdMatl)];
            if (cache.Current != null && e.Row != null && ((AMProdMatl)cache.Current).ProdCreate == true)
            {
                ((AMProdMatlSplit)e.Row).ProdCreate = true;
                DetailCache.Update(e.Row);
            }
        }

        protected virtual bool LotSerialNbr_Updated(PXCache sender, EventArgs e)
        {
            var split = (AMProdMatlSplit)(e is PXRowUpdatedEventArgs ? ((PXRowUpdatedEventArgs)e).Row : ((PXRowInsertedEventArgs)e).Row);
            var parent = (AMProdMatl)LSParentAttribute.SelectParent(sender, split, typeof(AMProdMatl));

            if (split == null || parent == null)
            {
                return false;
            }

            var accum = new SiteLotSerial
            {
                InventoryID = split.InventoryID,
                SiteID = split.SiteID,
                LotSerialNbr = split.LotSerialNbr
            };
            
            accum = (SiteLotSerial)sender.Graph.Caches[typeof(SiteLotSerial)].Insert(accum);
            accum = PXCache<SiteLotSerial>.CreateCopy(accum);

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, split.InventoryID);

            INSiteLotSerial siteLotSerial = PXSelectReadonly<
                INSiteLotSerial, 
                Where<INSiteLotSerial.inventoryID, Equal<Required<INSiteLotSerial.inventoryID>>, 
                    And<INSiteLotSerial.siteID, Equal<Required<INSiteLotSerial.siteID>>,
                    And<INSiteLotSerial.lotSerialNbr, Equal<Required<INSiteLotSerial.lotSerialNbr>>>>>>
                .Select(sender.Graph, split.InventoryID, split.SiteID, split.LotSerialNbr);

            if (siteLotSerial != null)
            {
                accum.QtyAvail += siteLotSerial.QtyAvail;
                accum.QtyHardAvail += siteLotSerial.QtyHardAvail;
            }

            var isIssue = !parent.IsByproduct.GetValueOrDefault();

            //Serial-numbered items
            if (INLotSerialNbrAttribute.IsTrackSerial(item, split.TranType, split.InvtMult) && split.LotSerialNbr != null)
            {
                var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
                if (siteLotSerial != null && siteLotSerial.QtyAvail > 0 && siteLotSerial.QtyHardAvail > 0)
                {
                    if (isIssue)
                    {
                        split.BaseQty = 1;
                        split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        split.IsAllocated = true;
                    }
                    else
                    {
                        split.IsAllocated = false;
                        sender.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefixNLA(PX.Objects.IN.Messages.SerialNumberAlreadyReceived, ((InventoryItem)item).InventoryCD, split.LotSerialNbr)));
                    }
                }
                else
                {

                    if (isIssue)
                    {
                        var extCall = false;
                        if (e is PXRowUpdatedEventArgs)
                        {
                            extCall = ((PXRowUpdatedEventArgs)e).ExternalCall;
                        }
                        if (e is PXRowInsertedEventArgs)
                        {
                            extCall = ((PXRowInsertedEventArgs)e).ExternalCall;
                        }
                        if (extCall)
                        {
                            split.IsAllocated = false;
                            split.LotSerialNbr = null;
                            sender.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PX.Objects.IN.Messages.Inventory_Negative2));
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

                sender.RaiseFieldUpdated(sender.GetField(typeof(AMProdMatlSplit.isAllocated)), split, copy.IsAllocated);
                sender.RaiseRowUpdated(split, copy);

                if (copy.BaseQty - 1 > 0m)
                {
                    if (split.IsAllocated == true || (split.IsAllocated != true && !isIssue))
                    {
                        copy.SplitLineNbr = null;
                        copy.PlanID = null;
                        copy.IsAllocated = false;
                        copy.LotSerialNbr = null;
                        copy.BaseQty -= 1;
                        copy.Qty = INUnitAttribute.ConvertFromBase(MasterCache, copy.InventoryID, copy.UOM, copy.BaseQty.GetValueOrDefault(), INPrecision.QUANTITY);
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
                        if (siteLotSerial == null || (((siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail <= 0m) || siteLotSerial.QtyOnHand <= 0m) && isIssue))
                        {
                            return NegativeInventoryError(sender, split);
                        }

                        var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
                        split.IsAllocated = true;
                        sender.RaiseFieldUpdated(sender.GetField(typeof(AMProdMatlSplit.isAllocated)), split, copy.IsAllocated);
                        sender.RaiseRowUpdated(split, copy);

                        return true;
                    }

                    //Lot/Serial Nbr. selected on allocated line. Available Qty. verification procedure 
                    if (split.IsAllocated == true)
                    {
                        var copy = PXCache<AMProdMatlSplit>.CreateCopy(split);
                        if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail >= 0m && isIssue)
                        {
                            split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        }
                        else if (siteLotSerial != null && siteLotSerial.QtyOnHand > 0 && accum.QtyHardAvail < 0m && isIssue)
                        {
                            split.BaseQty += accum.QtyHardAvail;
                            if (split.BaseQty <= 0m)
                            {
                                if (NegativeInventoryError(sender, split)) return false;
                            }
                            split.Qty = INUnitAttribute.ConvertFromBase(sender, split.InventoryID, split.UOM, (decimal)split.BaseQty, INPrecision.QUANTITY);
                        }
                        else if (siteLotSerial == null || (siteLotSerial.QtyOnHand <= 0m && isIssue))
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
                                copy = (AMProdMatlSplit)sender.Insert(copy);
                                if (copy.LotSerialNbr != null && copy.IsAllocated != true)
                                {
                                    sender.SetValue<AMProdMatlSplit.lotSerialNbr>(copy, null);
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

        protected virtual bool NegativeInventoryError(PXCache sender, AMProdMatlSplit split)
        {
            split.IsAllocated = false;
            split.LotSerialNbr = null;
            sender.RaiseExceptionHandling<AMProdMatlSplit.lotSerialNbr>(split, null, new PXSetPropertyException(PX.Objects.IN.Messages.Inventory_Negative2));
            if (split.IsAllocated == true)
            {
                return true;
            }
            return false;
        }

        public static void ResetAvailabilityCounters(AMProdMatl row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

        public virtual bool IsLotSerialItem(PXCache sender, ILSMaster line)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, line.InventoryID);
            return item != null && INLotSerialNbrAttribute.IsTrack(item, line.TranType, line.InvtMult);
        }

        public override void UpdateParent(PXCache sender, AMProdMatl Row)
        {
            if (Row != null)
            {
                decimal BaseQty;
                UpdateParent(sender, Row, null, null, out BaseQty);
            }
            else
            {
                base.UpdateParent(sender, Row);
            }
        }

        public override void UpdateParent(PXCache sender, AMProdMatlSplit Row, AMProdMatlSplit OldRow)
        {
            var parent = (AMProdMatl)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(AMProdMatl));

            if (parent != null)
            {
                if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
                {
                    var oldrow = PXCache<AMProdMatl>.CreateCopy(parent);

                    UpdateParent(sender, parent, Row != null && Row.Completed == false ? Row : null, OldRow != null && OldRow.Completed == false ? OldRow : null, out var baseQty);

                    using (var ms = new InvtMultScope<AMProdMatl>(parent))
                    {
                        if (Row != null) //IsLotSerialRequired in Check
                        {
                            parent.UnassignedQty = 0m;
                            if (IsLotSerialItem(sender, Row))
                            {
                                object[] splits = SelectDetail(sender, Row);
                                foreach (AMProdMatlSplit split in splits)
                                {
                                    if (split.LotSerialNbr == null)
                                    {
                                        parent.UnassignedQty += split.BaseQty;
                                    }
                                }
                            }
                        }
                        parent.BaseQty = baseQty; // + parent.BaseClosedQty;
                        parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
                    }

                    if (sender.Graph.Caches[typeof(AMProdMatl)].GetStatus(parent) == PXEntryStatus.Notchanged)
                    {
                        sender.Graph.Caches[typeof(AMProdMatl)].SetStatus(parent, PXEntryStatus.Updated);
                    }

                    sender.Graph.Caches[typeof(AMProdMatl)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
                    if (sender.Graph.Caches[typeof(AMProdMatl)].RaiseRowUpdating(oldrow, parent))
                    {
                        sender.Graph.Caches[typeof(AMProdMatl)].RaiseRowUpdated(parent, oldrow);
                    }
                    else
                    {
                        sender.Graph.Caches[typeof(AMProdMatl)].RestoreCopy(parent, oldrow);
                    }
                }
            }
            else
            {
                base.UpdateParent(sender, Row, OldRow);
            }
        }

        public override void UpdateParent(PXCache sender, AMProdMatl Row, AMProdMatlSplit Det, AMProdMatlSplit OldDet, out decimal BaseQty)
        {
            ResetAvailabilityCounters(Row);

            bool counted = DetailCounters.ContainsKey(Row);

            base.UpdateParent(sender, Row, Det, OldDet, out BaseQty);

            if (!counted && OldDet != null)
            {
                Counters counters;
                if (DetailCounters.TryGetValue(Row, out counters))
                {
                    BaseQty = counters.BaseQty;
                }
            }
        }

        public virtual void AMProdMatlSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var split = (AMProdMatlSplit) e.Row;

            if (split == null)
            {
                return;
            }

            PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.subItemID>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.siteID>(sender, e.Row, false);
            PXUIFieldAttribute.SetEnabled<AMProdMatlSplit.qty>(sender, e.Row, split.IsAllocated.GetValueOrDefault());
            PXUIFieldAttribute.SetVisible<AMProdMatlSplit.lotSerialNbr>(sender, e.Row, split.IsAllocated.GetValueOrDefault());

            if (split.Completed.GetValueOrDefault())
            {
                PXUIFieldAttribute.SetEnabled(sender, e.Row, false);
            }
        }

        protected override void LotSerOptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var opt = (LotSerOptions)e.Row;

            PXUIFieldAttribute.SetEnabled<LotSerOptions.startNumVal>(sender, opt, false);
            PXUIFieldAttribute.SetEnabled<LotSerOptions.qty>(sender, opt, false);
            PXUIFieldAttribute.SetEnabled<LotSerOptions.unassignedQty>(sender, opt, false);
            PXDBDecimalAttribute.SetPrecision(sender, opt, "Qty", opt?.IsSerial == true ? 0 : CommonSetupDecPl.Qty);
            _Graph.Actions[Prefixed("generateLotSerial")].SetEnabled(false);
        }

        public override void Detail_UOM_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, ((ILSDetail)e.Row).InventoryID);

            if (item != null && ((INLotSerClass)item).LotSerTrack == INLotSerTrack.SerialNumbered)
            {
                e.NewValue = ((InventoryItem)item).BaseUnit;
                e.Cancel = true;
            }
            else
            {
                base.Detail_UOM_FieldDefaulting(sender, e);
            }
        }

        // Copy logic from 19.105 LSSOLine / Remove previous set of INLotSerClass which was caching the changed value
        protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
        {
            var item = (InventoryItem)PXSelectorAttribute.Select(sender, null, typeof(AMProdMatl.inventoryID).Name, InventoryID);

            if (item != null)
            {
                var lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

                return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
            }

            return null;
        }
    }
}