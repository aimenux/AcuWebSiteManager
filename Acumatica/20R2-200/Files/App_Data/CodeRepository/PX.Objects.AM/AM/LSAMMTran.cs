using System;
using PX.Objects.IN;
using PX.Data;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using PX.Objects.AM.Attributes;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMMTran : LSSelect<AMMTran, AMMTranSplit, Where<AMMTranSplit.docType, Equal<Current<AMBatch.docType>>, And<AMMTranSplit.batNbr, Equal<Current<AMBatch.batNbr>>>>>
    {
        public LSAMMTran(PXGraph graph)
            : base(graph)
        {
            this.MasterQtyField = typeof (AMMTran.qty);
            graph.FieldDefaulting.AddHandler<AMMTranSplit.subItemID>(AMMTranSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMMTranSplit.locationID>(AMMTranSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMMTranSplit.invtMult>(AMMTranSplit_InvtMult_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMMTranSplit.lotSerialNbr>(AMMTranSplit_LotSerialNbr_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMMTranSplit.qty>(AMMTranSplit_Qty_FieldVerifying);
            graph.RowUpdated.AddHandler<AMBatch>(AMBatch_RowUpdated);
        }

        /// <summary>
        /// Translates AvailabilityFetch call QtyOnHand, Avail, NotAvail, Hard Avail numbers to transaction UOM numbers in returned QtyAllocated
        /// </summary>
        public virtual IQtyAllocated AvailabilityFetchTranUom(PXCache sender, AMMTran Row, bool ExcludeCurrent)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetch(sender, Row, Row != null && Row.Released.GetValueOrDefault() ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    decimal unitRate = INUnitAttribute.ConvertFromBase<AMMTran.inventoryID, AMMTran.uOM>(sender, Row, 1m, INPrecision.NOROUND);
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
            var row = (AMMTran)e.Row;
            e.ReturnValue = string.Empty;
            if (row == null
                || row.InventoryID.GetValueOrDefault() == 0
                || row.SiteID.GetValueOrDefault() == 0)
            {
                return;
            }
            
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetchTranUom(sender, (AMMTran)e.Row, !(e.Row != null && (((AMMTran)e.Row).Released == true)));

                if (availability != null)
                {
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(
                        Messages.LSTranStatus,
                        sender.GetValue<AMMTran.uOM>(e.Row), 
                        FormatQty(availability.QtyOnHand.GetValueOrDefault()), 
                        FormatQty(availability.QtyAvail.GetValueOrDefault()), 
                        FormatQty(availability.QtyHardAvail.GetValueOrDefault()));
                }
                else
                {
                    //handle missing UOM
                    INUnitAttribute.ConvertFromBase<AMMTran.inventoryID, AMMTran.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                }
            }

            base.Availability_FieldSelecting(sender, e);
        }

        public override AMMTranSplit Convert(AMMTran item)
        {
            using (InvtMultScope<AMMTran> ms = new InvtMultScope<AMMTran>(item))
            {
                AMMTranSplit ret = item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
            }
        }

        protected virtual void AMBatch_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (sender.ObjectsEqual<AMBatch.hold>(e.Row, e.OldRow))
            {
                return;
            }
            bool? nullable = (bool?)sender.GetValue<AMBatch.hold>(e.Row);
            if ((nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
            {
                return;
            }
            PXCache cache = sender.Graph.Caches[typeof(AMMTran)];
            foreach (AMMTran tran in PXParentAttribute.SelectSiblings(cache, null, typeof(AMBatch)))
            {
                if (Math.Abs(tran.BaseQty.Value) >= new Decimal(5, 0, 0, false, (byte)7))
                {
                    Decimal? unassignedQty = tran.UnassignedQty;
                    if ((!(unassignedQty.GetValueOrDefault() >= new Decimal(5, 0, 0, false, (byte)7)) ? 0 : (unassignedQty.HasValue ? 1 : 0)) != 0)
                    {
                        cache.RaiseExceptionHandling<AMMTran.qty>((object)tran, (object)tran.Qty, (Exception)new PXSetPropertyException(Messages.LSAMMTranLinesUnassigned));
                        if (cache.GetStatus((object)tran) == PXEntryStatus.Notchanged)
                        {
                            cache.SetStatus((object)tran, PXEntryStatus.Updated);
                        }
                    }
                }
            }
        }

        public virtual void AMMTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var cache = sender.Graph.Caches[typeof(AMMTran)];
            if (cache.Current == null || (AMMTranSplit)e.Row == null ||
                ((AMMTran) cache.Current).LineNbr != ((AMMTranSplit) e.Row).LineNbr)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"TranType = {((AMMTranSplit)e.Row).TranType} [{((AMMTran)cache.Current).TranType}]; InvtMult = {((AMMTranSplit)e.Row).InvtMult} [{((AMMTran)cache.Current).InvtMult}]; [{((AMMTran)cache.Current).DebuggerDisplay}]");
#endif
            //Not sure why we would ever want ot use InvtMultScope since it is changing the InvtMult value incorrectly on us when qty < 0
            using (InvtMultScope<AMMTran> ms = new InvtMultScope<AMMTran>((AMMTran)cache.Current))
            {
                e.NewValue = ((AMMTran)cache.Current).InvtMult;
                e.Cancel = true;
            }
        }

        public virtual void AMMTranSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMMTran)];
            if (cache.Current != null && (e.Row == null || ((AMMTran)cache.Current).LineNbr == ((AMMTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMMTran)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        public virtual void AMMTranSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMMTranSplit) e.Row;
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
                sender.RaiseFieldDefaulting<AMMTranSplit.invtMult>(e.Row, out InvtMult);
            }

            object TranType = row.TranType;
            if (TranType == null)
            {
                sender.RaiseFieldDefaulting<AMMTranSplit.tranType>(e.Row, out TranType);
            }

            INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
            if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
            {
                foreach (AMMTranSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<AMMTranSplit>(sender, item, mode, 1m))
                {
                    e.NewValue = lssplit.LotSerialNbr;
                    e.Cancel = true;
                }
            }
        }

        public void AMMTranSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> result = base.ReadInventoryItem(sender, ((AMMTranSplit)e.Row).InventoryID);
            if ((((result != null) && (((INLotSerClass)result).LotSerTrack == INLotSerTrack.SerialNumbered)) && (((INLotSerClass)result).LotSerAssign == INLotSerAssign.WhenReceived)) && (((e.NewValue != null) && (e.NewValue is decimal)) && ((((decimal)e.NewValue) != 0M) && (((decimal)e.NewValue) != 1M))))
            {
                e.NewValue = 1M;
            }
        }

        public void AMMTranSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((AMMTranSplit)e.Row).BaseQty != 0m && ((AMMTranSplit)e.Row).LocationID == null)
                {
                    ThrowFieldIsEmpty<AMMTranSplit.locationID>(sender, e.Row);
                }
            }
        }

        public void AMMTranSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMMTran)];
            if (cache.Current != null && (e.Row == null || ((AMMTran)cache.Current).LineNbr == ((AMMTranSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMMTran)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            if (((AMMTran)e.Row).InvtMult != 0)
            {
                base.Master_RowDeleted(sender, e);
            }
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (((AMMTran)e.Row).InvtMult != 0)
            {
                base.Master_RowInserted(sender, e);
            }
            else
            {
                sender.SetValue<AMMTran.lotSerialNbr>(e.Row, null);
                sender.SetValue<AMMTran.expireDate>(e.Row, null);
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                PXCache cache = sender.Graph.Caches[typeof(AMBatch)];
                object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(AMBatch)) ?? cache.Current;

                bool? OnHold = (bool?)cache.GetValue<AMBatch.hold>(doc);

                if (OnHold == false && Math.Abs((decimal)((AMMTran)e.Row).BaseQty) >= 0.0000005m && (((AMMTran)e.Row).UnassignedQty >= 0.0000005m || ((AMMTran)e.Row).UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<AMMTran.qty>(e.Row, ((AMMTran)e.Row).Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(AMMTran.qty).Name, ((AMMTran)e.Row).Qty, PX.Objects.IN.Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMMTran) e.Row;
            if (row?.InventoryID == null || row.OperationID == null || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            var amProdItem = (AMProdItem) PXSelectorAttribute.Select<AMMTran.prodOrdID>(sender, e.Row);
            if (amProdItem == null)
            {
                return;
            }

            var cache = sender.Graph.Caches[typeof(AMMTranSplit)];
            if (((AMMTran) e.OldRow).InventoryID != null && row.InventoryID == null || row.InventoryID != ((AMMTran) e.OldRow).InventoryID)
            {
                foreach (AMMTranSplit split in PXParentAttribute.SelectSiblings(cache, (AMMTranSplit)row, typeof(AMMTran)))
                {
                    cache.Delete(split); //Change of item will need a change of splits
                }
            }

            if (row.InvtMult != 0 && row.IsStockItem.GetValueOrDefault())
            {
                if (!sender.ObjectsEqual<AMMTran.tranType>(row, e.OldRow))
                {
                    SyncSplitTranType(sender, row, cache);
                }

                var lastOper = amProdItem.LastOperationID.GetValueOrDefault() == row.OperationID;
                var validItemEntry = lastOper || row.DocType == AMDocType.Material || row.IsScrap == true;

                if (validItemEntry
                    && (((AMMTran)e.Row).TranType == AMTranType.Receipt
                        || ((AMMTran)e.Row).TranType == AMTranType.Issue
                        || ((AMMTran)e.Row).TranType == AMTranType.Return
                        || ((AMMTran)e.Row).TranType == AMTranType.Adjustment))
                {
                    base.Master_RowUpdated(sender, e);
                }

                return;
            }

            sender.SetValue<AMMTran.lotSerialNbr>(e.Row, null);
            sender.SetValue<AMMTran.expireDate>(e.Row, null);
        }

        protected virtual void SyncSplitTranType(PXCache ammTranCache, AMMTran ammTran, PXCache splitCache)
        {
            ammTranCache.SetDefaultExt<AMMTran.invtMult>(ammTran);
            foreach (AMMTranSplit split in PXParentAttribute.SelectSiblings(splitCache, (AMMTranSplit)ammTran, typeof(AMMTran)))
            {
                var copy = PXCache<AMMTranSplit>.CreateCopy(split);
                if (split.TranType == ammTran.TranType)
                {
                    continue;
                }
                split.TranType = ammTran.TranType;
                //if the qty also changed, update the split
                split.Qty = Math.Abs(ammTran.Qty.GetValueOrDefault());
                if (splitCache.GetStatus(split) == PXEntryStatus.Notchanged)
                {
                    splitCache.SetStatus(split, PXEntryStatus.Updated);
                }
                splitCache.RaiseRowUpdated(split, copy);
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMMTran)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName(e.MessageFormat, sender.GetStateExt<AMMTran.inventoryID>(row), sender.GetStateExt<AMMTran.subItemID>(row), sender.GetStateExt<AMMTran.siteID>(row), sender.GetStateExt<AMMTran.locationID>(row), sender.GetValue<AMMTran.lotSerialNbr>(row));
#endif
                sender.RaiseExceptionHandling<AMMTran.qty>(row, newValue, 
                    new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, 
                    sender.GetStateExt<AMMTran.inventoryID>(row), 
                    sender.GetStateExt<AMMTran.subItemID>(row), 
                    sender.GetStateExt<AMMTran.siteID>(row), 
                    sender.GetStateExt<AMMTran.locationID>(row), 
                    sender.GetValue<AMMTran.lotSerialNbr>(row)));

                return;
            }

            sender.RaiseExceptionHandling<AMMTranSplit.qty>(row, newValue, 
                new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning, 
                    sender.GetStateExt<AMMTranSplit.inventoryID>(row), 
                    sender.GetStateExt<AMMTranSplit.subItemID>(row), 
                    sender.GetStateExt<AMMTranSplit.siteID>(row), 
                    sender.GetStateExt<AMMTranSplit.locationID>(row), 
                    sender.GetValue<AMMTranSplit.lotSerialNbr>(row)));
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data) where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name })))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name });
            }
        }
    }
}
