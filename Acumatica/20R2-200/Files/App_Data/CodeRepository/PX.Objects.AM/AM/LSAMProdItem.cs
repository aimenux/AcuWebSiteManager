using PX.Objects.IN;
using PX.Data;
using System.Collections.Generic;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMProdItem : LSSelect<AMProdItem, AMProdItemSplit, Where<AMProdItemSplit.orderType, Equal<Current<AMProdItem.orderType>>, 
        And<AMProdItemSplit.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
    {
        public LSAMProdItem(PXGraph graph) : base(graph)
        {
            graph.FieldDefaulting.AddHandler<AMProdItemSplit.subItemID>(new PXFieldDefaulting(this.AMProdItemSplit_SubItemID_FieldDefaulting));
            graph.FieldDefaulting.AddHandler<AMProdItemSplit.locationID>(new PXFieldDefaulting(this.AMProdItemSplit_LocationID_FieldDefaulting));
            graph.FieldDefaulting.AddHandler<AMProdItemSplit.invtMult>(new PXFieldDefaulting(this.AMProdItemSplit_InvtMult_FieldDefaulting));
            graph.FieldVerifying.AddHandler<AMProdItem.qtyRemaining>(new PXFieldVerifying(this.AMProdItem_QtytoProd_FieldVerifying));
            graph.RowSelected.AddHandler<AMProdItemSplit>(AMProdItemSplit_RowSelected);
        }

        protected int _detailsRequested = 0;

        public virtual bool IsLotSerialItem(PXCache sender, ILSMaster line)
        {
            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, line.InventoryID);

            if (item == null)
                return false;

            return INLotSerialNbrAttribute.IsTrack(item, line.TranType, line.InvtMult);
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            e.Cancel = true;
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
                var copy = Row as AMProdItemSplit;
                if (copy == null)
                {
                    copy = Convert(Row as AMProdItem);

                    PXParentAttribute.SetParent(DetailCache, copy, typeof(AMProdItem), Row);

                    if (string.IsNullOrEmpty(Row.LotSerialNbr) == false)
                    {
                        DefaultLotSerialNbr(sender.Graph.Caches[typeof(AMProdItemSplit)], copy);
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.TryOptimize) && _detailsRequested++ == 5)
                    {
                        foreach (PXResult<AMProdItem, INUnit, INSiteStatus> res in
                            PXSelectReadonly2<AMProdItem,
                            InnerJoin<INUnit, On<
                                INUnit.inventoryID, Equal<AMProdItem.inventoryID>,
                                And<INUnit.fromUnit, Equal<AMProdItem.uOM>>>,
                            InnerJoin<INSiteStatus, On<
                                AMProdItem.inventoryID, Equal<INSiteStatus.inventoryID>,
                                And<AMProdItem.subItemID, Equal<INSiteStatus.subItemID>,
                                And<AMProdItem.siteID, Equal<INSiteStatus.siteID>>>>>>,
                            Where<AMProdItem.orderType, Equal<Current<AMProdItem.orderType>>,
                                And<AMProdItem.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>
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

                        foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<AMProdItem.noteID>>>>.Select(this._Graph))
                        {
                            PXSelect<INItemPlan,
                                Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>
                                .StoreCached(this._Graph, new PXCommandKey(new object[] { plan.PlanID }), new List<object> { plan });
                        }
                    }

                    if (fetchMode.HasFlag(AvailabilityFetchMode.ExcludeCurrent))
                        return AvailabilityFetch(sender, (AMProdItem)Row, copy);
                }
                return AvailabilityFetch(sender, copy, fetchMode);
            }
            return null;
        }

#if DEBUG
        // Copy/update from PX.Objects.SO.LSSOLine (18.112.0019)
#endif
        private IQtyAllocated AvailabilityFetch(PXCache sender, AMProdItem line, AMProdItemSplit detail)
        {
            var result = AvailabilityFetch(sender, detail, AvailabilityFetchMode.None);
            if (result == null)
            {
                return null;
            }

            decimal? lineQtyAvail = (decimal?)sender.GetValue<AMProdItem.lineQtyAvail>(line);
            decimal? lineQtyHardAvail = (decimal?)sender.GetValue<AMProdItem.lineQtyHardAvail>(line);

            if (lineQtyAvail == null || lineQtyHardAvail == null)
            {
                lineQtyAvail = 0m;
                lineQtyHardAvail = 0m;

                foreach (AMProdItemSplit split in SelectDetail(DetailCache, line))
                {
                    detail = split;
                    if (detail.PlanID != null)
                    {
                        INItemPlan plan = PXSelect<INItemPlan, Where<INItemPlan.planID, Equal<Required<INItemPlan.planID>>>>.Select(this._Graph, detail.PlanID);
                        if (plan != null)
                        {
                            detail = PXCache<AMProdItemSplit>.CreateCopy(detail);
                            //detail.PlanType = plan.PlanType;
                        }
                    }

                    PXParentAttribute.SetParent(DetailCache, detail, typeof(AMProdItem), line);

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

                sender.SetValue<AMProdItem.lineQtyAvail>(line, lineQtyAvail);
                sender.SetValue<AMProdItem.lineQtyHardAvail>(line, lineQtyHardAvail);
            }

            result.QtyAvail += lineQtyAvail;
            result.QtyHardAvail += lineQtyHardAvail;
            result.QtyNotAvail = -lineQtyAvail;

            return result;
        }

        public override void Detail_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            VerifySNQuantity(sender, e, (ILSDetail)e.Row, typeof(AMProdItemSplit.qty).Name);
        }

        public override AMProdItemSplit Convert(AMProdItem item)
        {
            using (new InvtMultScope<AMProdItem>(item))
            {
                AMProdItemSplit ret = (AMProdItemSplit)item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
            }
        }

        protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
        {
            return INLotSerTrack.Mode.None;
        }

        public virtual void AMProdItem_QtytoProd_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            if ((decimal?)e.NewValue < 0M)
            {
                throw new PXSetPropertyException(Messages.EntryGreaterEqualZero, PXErrorLevel.Error, new object[] { 0 });
            }
        }

        public void AMProdItemSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdItem)];
            if (cache.Current != null && (e.Row == null || ((AMProdItem)cache.Current).ProdOrdID == ((AMProdItemSplit)e.Row).ProdOrdID))
            {
                e.NewValue = ((AMProdItem)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public void AMProdItemSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdItem)];
            if (cache.Current != null && (e.Row == null || ((AMProdItem)cache.Current).ProdOrdID == ((AMProdItemSplit)e.Row).ProdOrdID))
            {
                using (new InvtMultScope<AMProdItem>((AMProdItem)cache.Current))
                {
                    e.NewValue = ((AMProdItem)cache.Current).InvtMult;
                    e.Cancel = true;
                }
            }
        }
        
        public void AMProdItemSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMProdItem)];
            if ((cache.Current != null) && (e.Row == null || (((AMProdItem)cache.Current).ProdOrdID == ((AMProdItemSplit)e.Row).ProdOrdID)))
            {
                e.NewValue = ((AMProdItem)cache.Current).LocationID;
                e.Cancel = true;
            }
        }
        
        public void AMProdItemSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Delete) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Delete) == PXDBOperation.Update) && ((AMProdItemSplit)e.Row).BaseQty != 0M && !((AMProdItemSplit)e.Row).LocationID.HasValue)
            {
                this.ThrowFieldIsEmpty<AMProdItemSplit.locationID>(sender, e.Row);
            }
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (((AMProdItem)e.Row).InvtMult != 0)
            {
                base.Master_RowInserted(sender, e);
                return;
            }

            sender.SetValue<AMProdItem.lotSerialNbr>(e.Row, null);
            sender.SetValue<AMProdItem.expireDate>(e.Row, null);
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Master_RowUpdated(sender, e);
            
            sender.SetValue<AMProdItem.lotSerialNbr>(e.Row, null);
            sender.SetValue<AMProdItem.expireDate>(e.Row, null);
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            var row = (AMProdItemSplit)e.Row;

            // _Master_RowUpdated calls into CreateNumbers which does an insert for the new qty difference. This logic is a copy of Acumatica except removing the check on serial items so it will merge the insert into an update
            if (row != null && !e.ExternalCall && _Operation == PXDBOperation.Update)
            {
                foreach (var item in SelectDetail(sender, row))
                {
                    var detailitem = (AMProdItemSplit)item;

                    if (Detail_ObjectsEqual(row, detailitem))
                    {
                        object oldDetailItem = PXCache<AMProdItemSplit>.CreateCopy(detailitem);
#if DEBUG
                        AMDebug.TraceWriteMethodName($"Changing insert to an update. Adding {row.BaseQty} to {detailitem.BaseQty}");
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

            if (row != null && !e.Cancel && (row.InventoryID == null || string.IsNullOrEmpty(row.UOM)))
            {
                e.Cancel = true;
            }

            if (e.Cancel)
            {
                return;
            }

            base.Detail_RowInserting(sender, e);
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMProdItem)
            {
                sender.RaiseExceptionHandling<AMProdItem.qtytoProd>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, new object[] { sender.GetStateExt<AMProdItem.inventoryID>(row), sender.GetStateExt<AMProdItem.subItemID>(row), sender.GetStateExt<AMProdItem.siteID>(row), sender.GetStateExt<AMProdItem.locationID>(row), sender.GetValue<AMProdItem.lotSerialNbr>(row) }));
            }
            else
            {
                sender.RaiseExceptionHandling<AMProdItemSplit.qty>(row, null, new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, new object[] { sender.GetStateExt<AMProdItemSplit.inventoryID>(row), sender.GetStateExt<AMProdItemSplit.subItemID>(row), sender.GetStateExt<AMProdItemSplit.siteID>(row), sender.GetStateExt<AMProdItemSplit.locationID>(row), sender.GetValue<AMProdItemSplit.lotSerialNbr>(row) }));
            }
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data) where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name })))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name });
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            //for normal orders there are only when received numbers which do not require any additional processing
            DetailCounters[(AMProdItem)e.Row] = new Counters { UnassignedNumber = 0 };

            base.Master_RowPersisting(sender, e);
        }

        // Copy logic from 19.105 LSSOLine / Remove previous set of INLotSerClass which was caching the changed value
        protected override PXResult<InventoryItem, INLotSerClass> ReadInventoryItem(PXCache sender, int? InventoryID)
        {
            var item = (InventoryItem)PXSelectorAttribute.Select(sender, null, typeof(AMProdItem.inventoryID).Name, InventoryID);

            if (item != null)
            {
                var lsclass = INLotSerClass.PK.Find(sender.Graph, item.LotSerClassID);

                return new PXResult<InventoryItem, INLotSerClass>(item, lsclass ?? new INLotSerClass());
            }

            return null;
        }

        public override void UpdateParent(PXCache sender, AMProdItem Row)
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

        public override void UpdateParent(PXCache sender, AMProdItemSplit Row, AMProdItemSplit OldRow)
        {
            AMProdItem parent = (AMProdItem)LSParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(AMProdItem));

            if (parent != null)
            {
                if ((Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
                {
                    var oldrow = PXCache<AMProdItem>.CreateCopy(parent);
                    using (InvtMultScope<AMProdItem> ms = new InvtMultScope<AMProdItem>(parent))
                    {
                        if (Row != null) //IsLotSerialRequired in Check
                        {
                            parent.UnassignedQty = 0m;
                            if (IsLotSerialItem(sender, Row))
                            {
                                object[] splits = SelectDetail(sender, Row);
                                foreach (AMProdItemSplit split in splits)
                                {
                                    if (split.LotSerialNbr == null)
                                    {
                                        parent.UnassignedQty += split.BaseQty;
                                    }
                                }
                            }
                        }
                    }

                    if (sender.Graph.Caches[typeof(AMProdItem)].GetStatus(parent) == PXEntryStatus.Notchanged)
                    {
                        sender.Graph.Caches[typeof(AMProdItem)].SetStatus(parent, PXEntryStatus.Updated);
                    }

                    sender.Graph.Caches[typeof(AMProdItem)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
                    if (sender.Graph.Caches[typeof(AMProdItem)].RaiseRowUpdating(oldrow, parent))
                    {
                        sender.Graph.Caches[typeof(AMProdItem)].RaiseRowUpdated(parent, oldrow);
                    }
                    else
                    {
                        sender.Graph.Caches[typeof(AMProdItem)].RestoreCopy(parent, oldrow);
                    }
                }
            }
            else
            {
                base.UpdateParent(sender, Row, OldRow);
            }
        }

        public override void UpdateParent(PXCache sender, AMProdItem Row, AMProdItemSplit Det, AMProdItemSplit OldDet, out decimal BaseQty)
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

        public static void ResetAvailabilityCounters(AMProdItem row)
        {
            row.LineQtyAvail = null;
            row.LineQtyHardAvail = null;
        }

        public virtual void AMProdItemSplit_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            AMProdItemSplit split = e.Row as AMProdItemSplit;

            if (split != null)
            {
                object val = sender.GetValueExt<AMProdItemSplit.isAllocated>(e.Row);
                bool isAllocated = split.IsAllocated == true || (bool?)PXFieldState.UnwrapValue(val) == true;
                //bool IsLinked = split.PONbr != null || split.SOOrderNbr != null && split.IsAllocated == true;

                AMProdItem parent = (AMProdItem)PXParentAttribute.SelectParent(sender, split, typeof(AMProdItem));
                PXUIFieldAttribute.SetEnabled<AMProdItemSplit.subItemID>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<AMProdItemSplit.isAllocated>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<AMProdItemSplit.siteID>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<AMProdItemSplit.qty>(sender, e.Row, false);
                PXUIFieldAttribute.SetEnabled<AMProdItemSplit.lotSerialNbr>(sender, e.Row, false);
            }
        }

        protected override void LotSerOptions_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            LotSerOptions opt = (LotSerOptions)e.Row;

            PXUIFieldAttribute.SetEnabled<LotSerOptions.startNumVal>(sender, opt, false);
            PXUIFieldAttribute.SetEnabled<LotSerOptions.qty>(sender, opt, false);
            PXDBDecimalAttribute.SetPrecision(sender, opt, "Qty", (opt.IsSerial == true ? 0 : CommonSetupDecPl.Qty));
            _Graph.Actions[Prefixed("generateLotSerial")].SetEnabled(false);
        }
    }
}
