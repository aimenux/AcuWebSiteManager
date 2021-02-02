using System;
using System.Collections.Generic;
using PX.Objects.IN;
using PX.Data;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using PX.Objects.AM.Attributes;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMClockTran : LSSelect<AMClockTran, AMClockTranSplit, Where<AMClockTranSplit.employeeID, Equal<Current<AMClockTran.employeeID>>, 
        And<AMClockTranSplit.lineNbr, Equal<Current<AMClockTran.lineNbr>>>>>
    {
        public LSAMClockTran(PXGraph graph)
     : base(graph)
        {
            this.MasterQtyField = typeof(AMClockTran.qty);
            graph.FieldUpdated.AddHandler<AMClockTran.lastOper>(AMClockTran_LastOper_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMClockTran.qty>(AMClockTran_Qty_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMClockTran.operationID>(AMClockTran_OperationID_FieldUpdated);
            graph.RowSelected.AddHandler<AMClockTran>(AMClockTran_RowSelected);
            graph.FieldUpdated.AddHandler<AMClockTranSplit.invtMult>(AMClockTranSplit_InvtMult_FieldUpdated);
            graph.FieldDefaulting.AddHandler<AMClockTranSplit.invtMult>(AMClockTranSplit_InvtMult_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMClockTranSplit.qty>(AMClockTranSplit_Qty_FieldVerifying);
            graph.FieldDefaulting.AddHandler<AMClockTranSplit.subItemID>(AMClockTranSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMClockTranSplit.locationID>(AMClockTranSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMClockTranSplit.lotSerialNbr>(AMClockTranSplit_LotSerialNbr_FieldDefaulting);
        }

        #region Handlers
        protected virtual void AMClockTran_LastOper_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetTranTypeInvtMult(sender, (AMClockTran)e.Row);
        }

        protected virtual void AMClockTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetTranTypeInvtMult(sender, (AMClockTran)e.Row);
        }

        protected virtual void AMClockTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var tran = (AMClockTran)e.Row;
            if (!string.IsNullOrWhiteSpace(tran?.ProdOrdID) && tran.OperationID != null)
            {
                SetTranTypeInvtMult(sender, tran);
            }
        }

        protected virtual void AMClockTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMClockTran)e.Row;
            if (row == null || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            AllowDetail(row.Qty > 0 && row.LastOper.GetValueOrDefault());
            if(row.Qty > 0 )
                cache.RaiseFieldUpdated(_MasterQtyField, row, row.Qty);
        }

        protected virtual void AMClockTranSplit_InvtMult_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PXCache pxCache = sender.Graph.Caches[typeof(AMClockTran)];
            if (pxCache.Current == null)
            {
                return;
            }
            if (e.Row != null)
            {
                int? lineNbr1 = ((AMClockTran)pxCache.Current).LineNbr;
                int? lineNbr2 = ((AMClockTranSplit)e.Row).LineNbr;
                if ((lineNbr1.GetValueOrDefault() != lineNbr2.GetValueOrDefault() ? 0 : (lineNbr1.HasValue == lineNbr2.HasValue ? 1 : 0)) == 0)
                {
                    return;
                }
                ((AMClockTranSplit)e.Row).TranType = ((AMClockTranSplit)e.Row).InvtMult < 1 ? AMTranType.Adjustment : AMTranType.Receipt;
            }
        }

        public virtual void AMClockTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var cache = sender.Graph.Caches[typeof(AMClockTran)];
            if (cache.Current == null || (AMClockTranSplit)e.Row == null ||
                ((AMClockTran)cache.Current).LineNbr != ((AMClockTranSplit)e.Row).LineNbr)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"TranType = {((AMClockTranSplit)e.Row).TranType} [{((AMClockTran)cache.Current).TranType}]; InvtMult = {((AMClockTranSplit)e.Row).InvtMult} [{((AMClockTran)cache.Current).InvtMult}]; [{((AMClockTran)cache.Current).DebuggerDisplay}]");
#endif
            //Not sure why we would ever want ot use InvtMultScope since it is changing the InvtMult value incorrectly on us when qty < 0
            using (InvtMultScope<AMClockTran> ms = new InvtMultScope<AMClockTran>((AMClockTran)cache.Current))
            {
                e.NewValue = ((AMClockTran)cache.Current).InvtMult;
                e.Cancel = true;
            }
        }

        public void AMClockTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> result = base.ReadInventoryItem(sender, ((AMClockTranSplit)e.Row).InventoryID);
            if ((((result != null) && (((INLotSerClass)result).LotSerTrack == INLotSerTrack.SerialNumbered)) && (((INLotSerClass)result).LotSerAssign == INLotSerAssign.WhenReceived)) && (((e.NewValue != null) && (e.NewValue is decimal)) && ((((decimal)e.NewValue) != 0M) && (((decimal)e.NewValue) != 1M))))
            {
                e.NewValue = 1M;
            }
        }

        public void AMClockTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMClockTran)];
            if (cache.Current != null && (e.Row == null || ((AMClockTran)cache.Current).LineNbr == ((AMClockTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMClockTran)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        public virtual void AMClockTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMClockTran)];
            if (cache.Current != null && (e.Row == null || ((AMClockTran)cache.Current).LineNbr == ((AMClockTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMClockTran)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        public virtual void AMClockTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMClockTranSplit)e.Row;
            if (row == null)
            {
                return;
            }

            PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, row.InventoryID);

            if (item == null)
            {
                return;
            }

            object InvtMult = row.InvtMult;
            if (InvtMult == null)
            {
                sender.RaiseFieldDefaulting<AMClockTranSplit.invtMult>(e.Row, out InvtMult);
            }

            object TranType = row.TranType;
            if (TranType == null)
            {
                sender.RaiseFieldDefaulting<AMClockTranSplit.tranType>(e.Row, out TranType);
            }

            INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
            if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
            {
                foreach (AMClockTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<AMClockTranSplit>(sender, item, mode, 1m))
                {
                    e.NewValue = lssplit.LotSerialNbr;
                    e.Cancel = true;
                }
            }
        }
        #endregion

        public void AMClockTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((AMClockTranSplit)e.Row).BaseQty != 0m && ((AMClockTranSplit)e.Row).LocationID == null)
                {
                    ThrowFieldIsEmpty<AMClockTranSplit.locationID>(sender, e.Row);
                }
            }
        }

        protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            if (((AMClockTran)e.Row).InvtMult != 0)
            {
                base.Master_RowDeleted(sender, e);
            }
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (((AMClockTran)e.Row).InvtMult != 0)
            {
                base.Master_RowInserted(sender, e);
            }
            else
            {
                sender.SetValue<AMClockTran.lotSerialNbr>(e.Row, null);
                sender.SetValue<AMClockTran.expireDate>(e.Row, null);
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            //don't need to check this if it's not the last operation
            AMClockTran row = (AMClockTran)e.Row;

            if (row.LastOper == true && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (Math.Abs((decimal)row.BaseQty) >= 0.0000005m && (row.UnassignedQty >= 0.0000005m || row.UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<AMClockTran.qty>(e.Row, row.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(AMClockTran.qty).Name, row.Qty, PX.Objects.IN.Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMClockTran)e.Row;
            if (row?.InventoryID == null || row.OperationID == null || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            var amProdItem = (AMProdItem)PXSelectorAttribute.Select<AMClockTran.prodOrdID>(sender, e.Row);
            if (amProdItem == null)
            {
                return;
            }

            var cache = sender.Graph.Caches[typeof(AMClockTranSplit)];
            if (((AMClockTran)e.OldRow).InventoryID != null && row.InventoryID == null || row.InventoryID != ((AMClockTran)e.OldRow).InventoryID)
            {
                foreach (AMClockTranSplit split in PXParentAttribute.SelectSiblings(cache, (AMClockTranSplit)row, typeof(AMClockTran)))
                {
                    cache.Delete(split); //Change of item will need a change of splits
                }
            }

            if (row.InvtMult != 0 && row.IsStockItem.GetValueOrDefault())
            {
                if (!sender.ObjectsEqual<AMClockTran.tranType>(row, e.OldRow))
                {
                    SyncSplitTranType(sender, row, cache);
                }

                var lastOper = amProdItem.LastOperationID.GetValueOrDefault() == row.OperationID;
                var validItemEntry = lastOper;

                if (validItemEntry)
                {
                    base.Master_RowUpdated(sender, e);
                }

                return;
            }

            sender.SetValue<AMClockTran.lotSerialNbr>(e.Row, null);
            sender.SetValue<AMClockTran.expireDate>(e.Row, null);
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            base.Detail_RowInserting(sender, e);
            var row = (AMClockTranSplit)e.Row;
            if (row == null)
            {
                return;
            }

            var rowParent = (AMClockTran)PXParentAttribute.SelectParent(sender, row, typeof(AMClockTran));
            if (rowParent == null)
            {
                return;
            }

            row.TranType = rowParent.TranType ?? row.TranType;
            row.InvtMult = AMTranType.InvtMult(row.TranType, rowParent.Qty);
        }

        protected virtual void AllowDetail(bool allow)
        {
            DetailCache.AllowInsert = allow;
            DetailCache.AllowUpdate = allow && MasterCache.AllowUpdate;
        }

        public virtual IQtyAllocated AvailabilityFetchTranUom(PXCache sender, AMClockTran Row, bool ExcludeCurrent)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetch(sender, Row, Row != null && Row.Released.GetValueOrDefault() ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    decimal unitRate = INUnitAttribute.ConvertFromBase<AMClockTran.inventoryID, AMClockTran.uOM>(sender, Row, 1m, INPrecision.NOROUND);
                    availability.QtyOnHand = PXDBQuantityAttribute.Round(availability.QtyOnHand.GetValueOrDefault() * unitRate);
                    availability.QtyAvail = PXDBQuantityAttribute.Round(availability.QtyAvail.GetValueOrDefault() * unitRate);
                    availability.QtyNotAvail = PXDBQuantityAttribute.Round(availability.QtyNotAvail.GetValueOrDefault() * unitRate);
                    availability.QtyHardAvail = PXDBQuantityAttribute.Round(availability.QtyHardAvail.GetValueOrDefault() * unitRate);

                    return availability;
                }
            }
            return null;
        }

        public override void Availability_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            var row = (AMClockTran)e.Row;
            e.ReturnValue = string.Empty;
            if (row == null
                || row.InventoryID.GetValueOrDefault() == 0
                || row.SiteID.GetValueOrDefault() == 0)
            {
                return;
            }

            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetchTranUom(sender, (AMClockTran)e.Row, !(e.Row != null && (((AMClockTran)e.Row).Released == true)));

                if (availability != null)
                {
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(
                        Messages.LSTranStatus,
                        sender.GetValue<AMClockTran.uOM>(e.Row),
                        FormatQty(availability.QtyOnHand.GetValueOrDefault()),
                        FormatQty(availability.QtyAvail.GetValueOrDefault()),
                        FormatQty(availability.QtyHardAvail.GetValueOrDefault()));
                }
                else
                {
                    //handle missing UOM
                    INUnitAttribute.ConvertFromBase<AMClockTran.inventoryID, AMClockTran.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                }
            }

            base.Availability_FieldSelecting(sender, e);
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data) where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name })))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name });
            }
        }

        protected virtual void SyncSplitTranType(PXCache cache, AMClockTran tran, PXCache splitCache)
        {
            //cache.SetDefaultExt<AMClockTran.invtMult>(tran);
            foreach (AMClockTranSplit split in PXParentAttribute.SelectSiblings(splitCache, (AMClockTranSplit)tran, typeof(AMClockTran)))
            {
                var copy = PXCache<AMClockTranSplit>.CreateCopy(split);
                if (split.TranType == tran.TranType)
                {
                    continue;
                }
                split.TranType = tran.TranType;
                if (splitCache.GetStatus(split) == PXEntryStatus.Notchanged)
                {
                    splitCache.SetStatus(split, PXEntryStatus.Updated);
                }
                splitCache.RaiseRowUpdated(split, copy);
            }
        }

        protected virtual void SetTranTypeInvtMult(PXCache cache, AMClockTran tran)
        {
            if (tran == null)
            {
                return;
            }
#if DEBUG
            var tranTypeOld = tran.TranType;
            var invtMultOld = tran.InvtMult;
#endif
            var tranTypeNew = tran.Qty.GetValueOrDefault() < 0 ?
                AMTranType.Adjustment : AMTranType.Receipt;
            var invtMultNew = tran.LastOper.GetValueOrDefault() 
                ? AMTranType.InvtMult(tranTypeNew, tran.Qty)
                : 0;

#if DEBUG
            AMDebug.TraceWriteMethodName($"TranType = {tranTypeNew} (old value = {tranTypeOld}); InvtMult = {invtMultNew} (old value = {invtMultOld})");
#endif
            var syncSplits = false;
            if (invtMultNew != tran.InvtMult)
            {
                syncSplits |= tran.InvtMult != null;
                cache.SetValueExt<AMClockTran.invtMult>(tran, invtMultNew);
            }

            if (tranTypeNew != tran.TranType)
            {
                syncSplits |= tran.TranType != null;
                cache.SetValueExt<AMClockTran.tranType>(tran, tranTypeNew);
            }

            if (syncSplits)
            {
                SyncSplitTranType(cache, tran, cache.Graph.Caches[typeof(AMClockTranSplit)]);
            }
        }

        #region Overrides
        public override AMClockTranSplit Convert(AMClockTran item)
        {
            using (InvtMultScope<AMClockTran> ms = new InvtMultScope<AMClockTran>(item))
            {
                AMClockTranSplit ret = item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMClockTran)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName(e.MessageFormat, sender.GetStateExt<AMClockTran.inventoryID>(row), sender.GetStateExt<AMClockTran.subItemID>(row), sender.GetStateExt<AMClockTran.siteID>(row), sender.GetStateExt<AMClockTran.locationID>(row), sender.GetValue<AMClockTran.lotSerialNbr>(row));
#endif
                sender.RaiseExceptionHandling<AMClockTran.qty>(row, newValue,
                    new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning,
                    sender.GetStateExt<AMClockTran.inventoryID>(row),
                    sender.GetStateExt<AMClockTran.subItemID>(row),
                    sender.GetStateExt<AMClockTran.siteID>(row),
                    sender.GetStateExt<AMClockTran.locationID>(row),
                    sender.GetValue<AMClockTran.lotSerialNbr>(row)));

                return;
            }

            sender.RaiseExceptionHandling<AMClockTranSplit.qty>(row, newValue,
                new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning,
                    sender.GetStateExt<AMClockTranSplit.inventoryID>(row),
                    sender.GetStateExt<AMClockTranSplit.subItemID>(row),
                    sender.GetStateExt<AMClockTranSplit.siteID>(row),
                    sender.GetStateExt<AMClockTranSplit.locationID>(row),
                    sender.GetValue<AMClockTranSplit.lotSerialNbr>(row)));
        }

        public override void UpdateParent(PXCache sender, AMClockTran Row, AMClockTranSplit Det, AMClockTranSplit OldDet, out decimal BaseQty)
        {
            counters = null;
            if (!DetailCounters.TryGetValue(Row, out counters))
            {
                DetailCounters[Row] = counters = new Counters();
                foreach (AMClockTranSplit detail in SelectDetail(sender.Graph.Caches[typeof(AMClockTranSplit)], Row))
                {
                    UpdateCounters(sender, counters, detail);
                }
            }
            else
            {
                if (Det != null)
                {
                    UpdateCounters(sender, counters, Det);
                }
                if (OldDet != null)
                {
                    AMClockTranSplit detail = OldDet;
                    counters.RecordCount -= 1;
                    detail.BaseQty = INUnitAttribute.ConvertToBase(sender, detail.InventoryID, detail.UOM, (decimal)detail.Qty, detail.BaseQty, INPrecision.QUANTITY);
                    counters.BaseQty -= (decimal)detail.BaseQty;
                    if (detail.ExpireDate == null)
                    {
                        counters.ExpireDatesNull -= 1;
                    }
                    else if (counters.ExpireDates.ContainsKey(detail.ExpireDate))
                    {
                        if ((counters.ExpireDates[detail.ExpireDate] -= 1) == 0)
                        {
                            counters.ExpireDates.Remove(detail.ExpireDate);
                        }
                    }
                    if (detail.SubItemID == null)
                    {
                        counters.SubItemsNull -= 1;
                    }
                    else if (counters.SubItems.ContainsKey(detail.SubItemID))
                    {
                        if ((counters.SubItems[detail.SubItemID] -= 1) == 0)
                        {
                            counters.SubItems.Remove(detail.SubItemID);
                        }
                    }
                    if (detail.LocationID == null)
                    {
                        counters.LocationsNull -= 1;
                    }
                    else if (counters.Locations.ContainsKey(detail.LocationID))
                    {
                        if ((counters.Locations[detail.LocationID] -= 1) == 0)
                        {
                            counters.Locations.Remove(detail.LocationID);
                        }
                    }
                    if (detail.TaskID == null)
                    {
                        counters.ProjectTasksNull -= 1;
                    }
                    else
                    {
                        var kv = new KeyValuePair<int?, int?>(detail.ProjectID, detail.TaskID);
                        if (counters.ProjectTasks.ContainsKey(kv))
                        {
                            if ((counters.ProjectTasks[kv] -= 1) == 0)
                            {
                                counters.ProjectTasks.Remove(kv);
                            }
                        }
                    }
                    if (detail.LotSerialNbr == null)
                    {
                        counters.LotSerNumbersNull -= 1;
                    }
                    else if (counters.LotSerNumbers.ContainsKey(detail.LotSerialNbr))
                    {
                        if (string.IsNullOrEmpty(detail.AssignedNbr) == false && INLotSerialNbrAttribute.StringsEqual(detail.AssignedNbr, detail.LotSerialNbr))
                        {
                            counters.UnassignedNumber--;
                        }
                        if ((counters.LotSerNumbers[detail.LotSerialNbr] -= 1) == 0)
                        {
                            counters.LotSerNumbers.Remove(detail.LotSerialNbr);
                        }
                    }
                }
                if (Det == null && OldDet != null)
                {
                    if (counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0)
                    {
                        foreach (DateTime? key in counters.ExpireDates.Keys)
                        {
                            counters.ExpireDate = key;
                        }
                    }
                    if (counters.SubItems.Count == 1 && counters.SubItemsNull == 0)
                    {
                        foreach (int? key in counters.SubItems.Keys)
                        {
                            counters.SubItem = key;
                        }
                    }
                    if (counters.Locations.Count == 1 && counters.LocationsNull == 0)
                    {
                        foreach (int? key in counters.Locations.Keys)
                        {
                            counters.Location = key;
                        }
                    }
                    if (counters.ProjectTasks.Count == 1 && counters.ProjectTasksNull == 0)
                    {
                        foreach (KeyValuePair<int?, int?> key in counters.ProjectTasks.Keys)
                        {
                            counters.ProjectID = key.Key;
                            counters.TaskID = key.Value;
                        }
                    }
                    if (counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0)
                    {
                        foreach (string key in counters.LotSerNumbers.Keys)
                        {
                            counters.LotSerNumber = key;
                        }
                    }
                }
            }

            BaseQty = counters.BaseQty;

            switch (counters.RecordCount)
            {
                case 0:
                    Row.LotSerialNbr = string.Empty;
                    Row.HasMixedProjectTasks = false;
                    break;
                case 1:
                    Row.ExpireDate = counters.ExpireDate;
                    Row.SubItemID = counters.SubItem;
                    Row.LocationID = counters.Location;
                    Row.LotSerialNbr = counters.LotSerNumber;
                    Row.HasMixedProjectTasks = false;
                    if (counters.ProjectTasks.Count > 0 && Det != null && counters.ProjectID != null)
                    {
                        Row.ProjectID = counters.ProjectID;
                        Row.TaskID = counters.TaskID;
                    }
                    break;
                default:
                    Row.ExpireDate = counters.ExpireDates.Count == 1 && counters.ExpireDatesNull == 0 ? counters.ExpireDate : null;
                    Row.SubItemID = counters.SubItems.Count == 1 && counters.SubItemsNull == 0 ? counters.SubItem : null;
                    Row.LocationID = counters.Locations.Count == 1 && counters.LocationsNull == 0 ? counters.Location : null;
                    Row.HasMixedProjectTasks = counters.ProjectTasks.Count + (counters.ProjectTasks.Count > 0 ? counters.ProjectTasksNull : 0) > 1;
                    if (Row.HasMixedProjectTasks != true && Det != null && counters.ProjectID != null)
                    {
                        Row.ProjectID = counters.ProjectID;
                        Row.TaskID = counters.TaskID;
                    }

                    PXResult<InventoryItem, INLotSerClass> item = ReadInventoryItem(sender, Row.InventoryID);
                    INLotSerTrack.Mode mode = GetTranTrackMode(Row, item);
                    if (mode == INLotSerTrack.Mode.None)
                    {
                        Row.LotSerialNbr = string.Empty;
                    }
                    else if ((mode & INLotSerTrack.Mode.Create) > 0 || (mode & INLotSerTrack.Mode.Issue) > 0)
                    {
                        //if more than 1 split exist at lotserial creation time ignore equilness and display <SPLIT>
                        Row.LotSerialNbr = null;
                    }
                    else
                    {
                        Row.LotSerialNbr = counters.LotSerNumbers.Count == 1 && counters.LotSerNumbersNull == 0 ? counters.LotSerNumber : null;
                    }
                    break;
            }
        }

        public override void UpdateParent(PXCache sender, AMClockTran Row)
        {
            decimal BaseQty;
            UpdateParent(sender, Row, null, null, out BaseQty);
            Row.UnassignedQty = PXDBQuantityAttribute.Round((decimal)(Row.BaseQty - BaseQty));
        }

        public override void UpdateParent(PXCache sender, AMClockTranSplit Row, AMClockTranSplit OldRow)
        {
            AMClockTran parent = (AMClockTran)PXParentAttribute.SelectParent(sender, Row ?? OldRow, typeof(AMClockTran));

            if (parent != null && (Row ?? OldRow) != null && SameInventoryItem((ILSMaster)(Row ?? OldRow), (ILSMaster)parent))
            {
                AMClockTran oldrow = PXCache<AMClockTran>.CreateCopy(parent);
                decimal BaseQty;

                UpdateParent(sender, parent, Row, OldRow, out BaseQty);

                using (InvtMultScope<AMClockTran> ms = new InvtMultScope<AMClockTran>(parent))
                {
                    if (BaseQty < parent.BaseQty)
                    {
                        parent.UnassignedQty = PXDBQuantityAttribute.Round((decimal)(parent.BaseQty - BaseQty));
                    }
                    else
                    {
                        parent.UnassignedQty = 0m;
                        parent.BaseQty = BaseQty;
                        parent.Qty = INUnitAttribute.ConvertFromBase(sender, parent.InventoryID, parent.UOM, (decimal)parent.BaseQty, INPrecision.QUANTITY);
                    }
                }

                sender.Graph.Caches[typeof(AMClockTran)].MarkUpdated(parent);

                if (Math.Abs((Decimal)oldrow.Qty - (Decimal)parent.Qty) >= 0.0000005m)
                {
                    sender.Graph.Caches[typeof(AMClockTran)].RaiseFieldUpdated(_MasterQtyField, parent, oldrow.Qty);
                    sender.Graph.Caches[typeof(AMClockTran)].RaiseRowUpdated(parent, oldrow);
                }
            }
        }
        #endregion
    }
}
