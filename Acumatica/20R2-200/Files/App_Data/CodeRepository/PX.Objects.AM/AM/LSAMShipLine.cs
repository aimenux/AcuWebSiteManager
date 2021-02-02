using System;
using PX.Objects.IN;
using PX.Data;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using PX.Objects.AM.Attributes;
using PX.Objects.Common.Exceptions;

namespace PX.Objects.AM
{
    public class LSAMShipLine : LSSelect<AMVendorShipLine, AMVendorShipLineSplit, Where<AMVendorShipLineSplit.shipmentNbr, Equal<Current<AMVendorShipment.shipmentNbr>>>>
    {
        public LSAMShipLine(PXGraph graph)
            : base(graph)
        {
            this.MasterQtyField = typeof(AMVendorShipLine.qty);
            graph.FieldDefaulting.AddHandler<AMVendorShipLineSplit.subItemID>(AMVendorShipLineSplit_SubItemID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMVendorShipLineSplit.locationID>(AMVendorShipLineSplit_LocationID_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMVendorShipLineSplit.invtMult>(AMVendorShipLineSplit_InvtMult_FieldDefaulting);
            graph.FieldDefaulting.AddHandler<AMVendorShipLineSplit.lotSerialNbr>(AMVendorShipLineSplit_LotSerialNbr_FieldDefaulting);
            graph.FieldVerifying.AddHandler<AMVendorShipLineSplit.qty>(AMVendorShipLineSplit_Qty_FieldVerifying);
            graph.RowUpdated.AddHandler<AMVendorShipment>(AMVendorShipment_RowUpdated);
        }

        /// <summary>
        /// Translates AvailabilityFetch call QtyOnHand, Avail, NotAvail, Hard Avail numbers to transaction UOM numbers in returned QtyAllocated
        /// </summary>
        public virtual IQtyAllocated AvailabilityFetchTranUom(PXCache sender, AMVendorShipLine Row, bool ExcludeCurrent)
        {
            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetch(sender, Row, Row != null && Row.Released.GetValueOrDefault() ? AvailabilityFetchMode.None : AvailabilityFetchMode.ExcludeCurrent);

                if (availability != null)
                {
                    decimal unitRate = INUnitAttribute.ConvertFromBase<AMVendorShipLine.inventoryID, AMVendorShipLine.uOM>(sender, Row, 1m, INPrecision.NOROUND);
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
            var row = (AMVendorShipLine)e.Row;
            e.ReturnValue = string.Empty;
            if (row == null
                || row.InventoryID.GetValueOrDefault() == 0
                || row.SiteID.GetValueOrDefault() == 0)
            {
                return;
            }

            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetchTranUom(sender, (AMVendorShipLine)e.Row, !(e.Row != null && (((AMVendorShipLine)e.Row).Released == true)));

                if (availability != null)
                {
                    e.ReturnValue = PXMessages.LocalizeFormatNoPrefix(
                        Messages.LSTranStatus,
                        sender.GetValue<AMVendorShipLine.uOM>(e.Row),
                        FormatQty(availability.QtyOnHand.GetValueOrDefault()),
                        FormatQty(availability.QtyAvail.GetValueOrDefault()),
                        FormatQty(availability.QtyHardAvail.GetValueOrDefault()));
                }
                else
                {
                    //handle missing UOM
                    INUnitAttribute.ConvertFromBase<AMVendorShipLine.inventoryID, AMVendorShipLine.uOM>(sender, e.Row, 0m, INPrecision.QUANTITY);
                }
            }

            base.Availability_FieldSelecting(sender, e);
        }

        public override AMVendorShipLineSplit Convert(AMVendorShipLine item)
        {
            using (InvtMultScope<AMVendorShipLine> ms = new InvtMultScope<AMVendorShipLine>(item))
            {
                AMVendorShipLineSplit ret = item;
                ret.BaseQty = item.BaseQty - item.UnassignedQty;
                return ret;
            }
        }

        protected virtual void AMVendorShipment_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            if (sender.ObjectsEqual<AMVendorShipment.hold>(e.Row, e.OldRow))
            {
                return;
            }
            bool? nullable = (bool?)sender.GetValue<AMVendorShipment.hold>(e.Row);
            if ((nullable.GetValueOrDefault() ? 0 : (nullable.HasValue ? 1 : 0)) == 0)
            {
                return;
            }
            PXCache cache = sender.Graph.Caches[typeof(AMVendorShipLine)];
            foreach (AMVendorShipLine tran in PXParentAttribute.SelectSiblings(cache, null, typeof(AMVendorShipment)))
            {
                if (Math.Abs(tran.BaseQty.Value) >= new Decimal(5, 0, 0, false, (byte)7))
                {
                    Decimal? unassignedQty = tran.UnassignedQty;
                    if ((!(unassignedQty.GetValueOrDefault() >= new Decimal(5, 0, 0, false, (byte)7)) ? 0 : (unassignedQty.HasValue ? 1 : 0)) != 0)
                    {
                        cache.RaiseExceptionHandling<AMVendorShipLine.qty>((object)tran, (object)tran.Qty, (Exception)new PXSetPropertyException(Messages.LSAMMTranLinesUnassigned));
                        if (cache.GetStatus((object)tran) == PXEntryStatus.Notchanged)
                        {
                            cache.SetStatus((object)tran, PXEntryStatus.Updated);
                        }
                    }
                }
            }
        }

        public virtual void AMVendorShipLineSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var cache = sender.Graph.Caches[typeof(AMVendorShipLine)];
            if (cache.Current == null || (AMVendorShipLineSplit)e.Row == null ||
                ((AMVendorShipLine)cache.Current).LineNbr != ((AMVendorShipLineSplit)e.Row).LineNbr)
            {
                return;
            }

//#if DEBUG
//            AMDebug.TraceWriteMethodName($"TranType = {((AMVendorShipLineSplit)e.Row).TranType} [{((AMVendorShipLine)cache.Current).TranType}]; InvtMult = {((AMVendorShipLineSplit)e.Row).InvtMult} [{((AMVendorShipLine)cache.Current).InvtMult}]; [{((AMVendorShipLine)cache.Current).DebuggerDisplay}]");
//#endif
            //Not sure why we would ever want ot use InvtMultScope since it is changing the InvtMult value incorrectly on us when qty < 0
            using (InvtMultScope<AMVendorShipLine> ms = new InvtMultScope<AMVendorShipLine>((AMVendorShipLine)cache.Current))
            {
                e.NewValue = ((AMVendorShipLine)cache.Current).InvtMult;
                e.Cancel = true;
            }
        }

        public virtual void AMVendorShipLineSplit_LocationID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMVendorShipLine)];
            if (cache.Current != null && (e.Row == null || ((AMVendorShipLine)cache.Current).LineNbr == ((AMVendorShipLineSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMVendorShipLine)cache.Current).LocationID;
                e.Cancel = true;
            }
        }

        public virtual void AMVendorShipLineSplit_LotSerialNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var row = (AMVendorShipLineSplit)e.Row;
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
                sender.RaiseFieldDefaulting<AMVendorShipLineSplit.invtMult>(e.Row, out InvtMult);
            }

            object TranType = row.TranType;
            if (TranType == null)
            {
                sender.RaiseFieldDefaulting<AMVendorShipLineSplit.tranType>(e.Row, out TranType);
            }

            //don't default in a lot/serial number for WIP transactions
            if ((string)TranType == INTranType.Receipt)
                return;

            INLotSerTrack.Mode mode = GetTranTrackMode((ILSMaster)e.Row, item);
            if (mode == INLotSerTrack.Mode.None || (mode & INLotSerTrack.Mode.Create) > 0)
            {
                foreach (AMVendorShipLineSplit lssplit in INLotSerialNbrAttribute.CreateNumbers<AMVendorShipLineSplit>(sender, item, mode, 1m))
                {
                    e.NewValue = lssplit.LotSerialNbr;
                    e.Cancel = true;
                }
            }
        }

        public void AMVendorShipLineSplit_Qty_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            PXResult<InventoryItem, INLotSerClass> result = base.ReadInventoryItem(sender, ((AMVendorShipLineSplit)e.Row).InventoryID);
            if ((((result != null) && (((INLotSerClass)result).LotSerTrack == INLotSerTrack.SerialNumbered)) && (((INLotSerClass)result).LotSerAssign == INLotSerAssign.WhenReceived)) && (((e.NewValue != null) && (e.NewValue is decimal)) && ((((decimal)e.NewValue) != 0M) && (((decimal)e.NewValue) != 1M))))
            {
                e.NewValue = 1M;
            }
        }

        public void AMVendorShipLineSplit_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if (e.Row != null && ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update))
            {
                if (((AMVendorShipLineSplit)e.Row).BaseQty != 0m && ((AMVendorShipLineSplit)e.Row).LocationID == null)
                {
                    ThrowFieldIsEmpty<AMVendorShipLineSplit.locationID>(sender, e.Row);
                }
            }
        }

        public void AMVendorShipLineSplit_SubItemID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            PXCache cache = sender.Graph.Caches[typeof(AMVendorShipLine)];
            if (cache.Current != null && (e.Row == null || ((AMVendorShipLine)cache.Current).LineNbr == ((AMVendorShipLineSplit)e.Row).LineNbr))
            {
                e.NewValue = ((AMVendorShipLine)cache.Current).SubItemID;
                e.Cancel = true;
            }
        }

        protected override void Master_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            if (((AMVendorShipLine)e.Row).InvtMult != 0)
            {
                base.Master_RowDeleted(sender, e);
            }
        }

        protected override void Master_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            if (((AMVendorShipLine)e.Row).InvtMult != 0)
            {
                base.Master_RowInserted(sender, e);
            }
            else
            {
                sender.SetValue<AMVendorShipLine.lotSerialNbr>(e.Row, null);
                sender.SetValue<AMVendorShipLine.expireDate>(e.Row, null);
            }
        }

        protected override void Master_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert || (e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                PXCache cache = sender.Graph.Caches[typeof(AMVendorShipment)];
                object doc = PXParentAttribute.SelectParent(sender, e.Row, typeof(AMVendorShipment)) ?? cache.Current;

                bool? OnHold = (bool?)cache.GetValue<AMVendorShipment.hold>(doc);

                if (OnHold == false && Math.Abs((decimal)((AMVendorShipLine)e.Row).BaseQty) >= 0.0000005m && (((AMVendorShipLine)e.Row).UnassignedQty >= 0.0000005m || ((AMVendorShipLine)e.Row).UnassignedQty <= -0.0000005m))
                {
                    if (sender.RaiseExceptionHandling<AMVendorShipLine.qty>(e.Row, ((AMVendorShipLine)e.Row).Qty, new PXSetPropertyException(PX.Objects.IN.Messages.BinLotSerialNotAssigned)))
                    {
                        throw new PXRowPersistingException(typeof(AMVendorShipLine.qty).Name, ((AMVendorShipLine)e.Row).Qty, PX.Objects.IN.Messages.BinLotSerialNotAssigned);
                    }
                }
            }
            base.Master_RowPersisting(sender, e);
        }

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            var row = (AMVendorShipLine)e.Row;
            if (row?.InventoryID == null || row.OperationID == null || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            var amProdItem = (AMProdItem)PXSelectorAttribute.Select<AMVendorShipLine.prodOrdID>(sender, e.Row);
            if (amProdItem == null)
            {
                return;
            }

            var cache = sender.Graph.Caches[typeof(AMVendorShipLineSplit)];
            if (((AMVendorShipLine)e.OldRow).InventoryID != null && row.InventoryID == null || row.InventoryID != ((AMVendorShipLine)e.OldRow).InventoryID)
            {
                foreach (AMVendorShipLineSplit split in PXParentAttribute.SelectSiblings(cache, (AMVendorShipLineSplit)row, typeof(AMVendorShipLine)))
                {
                    cache.Delete(split); //Change of item will need a change of splits
                }
            }

            if (row.InvtMult != 0) //&& row.IsStockItem.GetValueOrDefault())
            {
                if (!sender.ObjectsEqual<AMVendorShipLine.tranType>(row, e.OldRow))
                {
                    SyncSplitTranType(sender, row, cache);
                }

                if ((((AMVendorShipLine)e.Row).TranType == AMTranType.Receipt
                        || ((AMVendorShipLine)e.Row).TranType == AMTranType.Issue
                        || ((AMVendorShipLine)e.Row).TranType == AMTranType.Return
                        || ((AMVendorShipLine)e.Row).TranType == AMTranType.Adjustment))
                {
                    base.Master_RowUpdated(sender, e);
                }

                return;
            }

            sender.SetValue<AMVendorShipLine.lotSerialNbr>(e.Row, null);
            sender.SetValue<AMVendorShipLine.expireDate>(e.Row, null);
        }

        protected virtual void SyncSplitTranType(PXCache AMVendorShipLineCache, AMVendorShipLine AMVendorShipLine, PXCache splitCache)
        {
            AMVendorShipLineCache.SetDefaultExt<AMVendorShipLine.invtMult>(AMVendorShipLine);
            foreach (AMVendorShipLineSplit split in PXParentAttribute.SelectSiblings(splitCache, (AMVendorShipLineSplit)AMVendorShipLine, typeof(AMVendorShipLine)))
            {
                var copy = PXCache<AMVendorShipLineSplit>.CreateCopy(split);
                if (split.TranType == AMVendorShipLine.TranType)
                {
                    continue;
                }
                split.TranType = AMVendorShipLine.TranType;
                if (splitCache.GetStatus(split) == PXEntryStatus.Notchanged)
                {
                    splitCache.SetStatus(split, PXEntryStatus.Updated);
                }
                splitCache.RaiseRowUpdated(split, copy);
            }
        }

        protected override void RaiseQtyExceptionHandling(PXCache sender, object row, object newValue, PXExceptionInfo e)
        {
            if (row is AMVendorShipLine)
            {
#if DEBUG
                AMDebug.TraceWriteMethodName(e.MessageFormat, sender.GetStateExt<AMVendorShipLine.inventoryID>(row), sender.GetStateExt<AMVendorShipLine.subItemID>(row), sender.GetStateExt<AMVendorShipLine.siteID>(row), sender.GetStateExt<AMVendorShipLine.locationID>(row), sender.GetValue<AMVendorShipLine.lotSerialNbr>(row));
#endif
                sender.RaiseExceptionHandling<AMVendorShipLine.qty>(row, newValue,
                    new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning,
                    sender.GetStateExt<AMVendorShipLine.inventoryID>(row),
                    sender.GetStateExt<AMVendorShipLine.subItemID>(row),
                    sender.GetStateExt<AMVendorShipLine.siteID>(row),
                    sender.GetStateExt<AMVendorShipLine.locationID>(row),
                    sender.GetValue<AMVendorShipLine.lotSerialNbr>(row)));

                return;
            }

            sender.RaiseExceptionHandling<AMVendorShipLineSplit.qty>(row, newValue,
                new PXSetPropertyException(e.MessageFormat, PXErrorLevel.Warning,
                    sender.GetStateExt<AMVendorShipLineSplit.inventoryID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.subItemID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.siteID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.locationID>(row),
                    sender.GetValue<AMVendorShipLineSplit.lotSerialNbr>(row)));
        }

        public void ThrowFieldIsEmpty<Field>(PXCache sender, object data) where Field : IBqlField
        {
            if (sender.RaiseExceptionHandling<Field>(data, null, new PXSetPropertyException(ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name })))
            {
                throw new PXRowPersistingException(typeof(Field).Name, null, ErrorMessages.FieldIsEmpty, new object[] { typeof(Field).Name });
            }
        }

        public override void AvailabilityCheck(PXCache sender, ILSMaster row, IQtyAllocated availability)
        {
            AMBatch doc = (AMBatch)sender.Graph.Caches[typeof(AMBatch)].Current;
            if (doc == null
                || doc.Released.GetValueOrDefault()
                || availability == null
                || row == null
                || row.BaseQty.GetValueOrDefault() == 0)
            {
                return;
            }

            //QtyOnHand should have already been converted to the Trans UOM so don't use BaseQty - use Qty to compare
            if (row.InvtMult == -1
                && row.Qty.GetValueOrDefault() > 0m
                && availability.QtyOnHand.GetValueOrDefault() - row.Qty.GetValueOrDefault() < 0m)
            {
                if (availability is LotSerialStatus)
                {
                    RaiseQtyRowExceptionHandling(sender, row, row.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.StatusCheck_QtyLotSerialOnHandNegative));
                }
                else if (availability is LocationStatus)
                {
                    RaiseQtyRowExceptionHandling(sender, row, row.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.StatusCheck_QtyLocationOnHandNegative));
                }
                else if (availability is SiteStatus)
                {
                    RaiseQtyRowExceptionHandling(sender, row, row.Qty, new PXSetPropertyException(PX.Objects.IN.Messages.StatusCheck_QtyOnHandNegative));
                }
            }

            //base.AvailabilityCheck(sender, row, availability);
        }

        private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
        {
            if (row is AMVendorShipLine)
            {
                sender.RaiseExceptionHandling<AMVendorShipLine.qty>(row, newValue,
                    e == null ? e : new PXSetPropertyException(e.MessageNoPrefix, PXErrorLevel.RowWarning,
                    sender.GetStateExt<AMVendorShipLine.inventoryID>(row),
                    sender.GetStateExt<AMVendorShipLine.subItemID>(row),
                    sender.GetStateExt<AMVendorShipLine.siteID>(row),
                    sender.GetStateExt<AMVendorShipLine.locationID>(row),
                    sender.GetValue<AMVendorShipLine.lotSerialNbr>(row)));
                return;
            }

            sender.RaiseExceptionHandling<AMVendorShipLineSplit.qty>(row, newValue,
                e == null ? e : new PXSetPropertyException(e.MessageNoPrefix, PXErrorLevel.RowWarning,
                    sender.GetStateExt<AMVendorShipLineSplit.inventoryID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.subItemID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.siteID>(row),
                    sender.GetStateExt<AMVendorShipLineSplit.locationID>(row),
                    sender.GetValue<AMVendorShipLineSplit.lotSerialNbr>(row)));
        }

        protected override INLotSerTrack.Mode GetTranTrackMode(ILSMaster row, INLotSerClass lotSerClass)
        {
            if (row.TranType == INTranType.Receipt)
                return INLotSerTrack.Mode.Manual; 

            return INLotSerialNbrAttribute.TranTrackMode(lotSerClass, row.TranType, row.InvtMult);
        }
    }
}
