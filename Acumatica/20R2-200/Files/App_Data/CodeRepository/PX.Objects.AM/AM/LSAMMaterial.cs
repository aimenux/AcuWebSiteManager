using PX.Data;
using PX.Objects.IN;
using IQtyAllocated = PX.Objects.IN.Overrides.INDocumentRelease.IQtyAllocated;
using SiteStatus = PX.Objects.IN.Overrides.INDocumentRelease.SiteStatus;
using LocationStatus = PX.Objects.IN.Overrides.INDocumentRelease.LocationStatus;
using LotSerialStatus = PX.Objects.IN.Overrides.INDocumentRelease.LotSerialStatus;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class LSAMMaterial : LSAMMTran
    {
        public LSAMMaterial(PXGraph graph) : base(graph)
        {
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

        protected override void Master_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            base.Master_RowUpdated(sender, e);

            var row = (AMMTran)e.Row;
            if (row == null
                || row.InventoryID.GetValueOrDefault() == 0
                || row.SiteID.GetValueOrDefault() == 0
                || string.IsNullOrWhiteSpace(row.ProdOrdID)
                || row.OperationID == null)
            {
                return;
            }

            if (!PXLongOperation.Exists(sender.Graph.UID))
            {
                IQtyAllocated availability = AvailabilityFetchTranUom(sender, row, !row.Released.GetValueOrDefault());
                if (availability != null)
                {
                    AvailabilityCheck(sender, row, availability);
                }
            }
        }

        private void RaiseQtyRowExceptionHandling(PXCache sender, object row, object newValue, PXSetPropertyException e)
        {
            if (row is AMMTran)
            {
                sender.RaiseExceptionHandling<AMMTran.qty>(row, newValue,
                    e == null ? e : new PXSetPropertyException(e.MessageNoPrefix, PXErrorLevel.RowWarning,
                    sender.GetStateExt<AMMTran.inventoryID>(row),
                    sender.GetStateExt<AMMTran.subItemID>(row),
                    sender.GetStateExt<AMMTran.siteID>(row),
                    sender.GetStateExt<AMMTran.locationID>(row),
                    sender.GetValue<AMMTran.lotSerialNbr>(row)));
                return;
            }

            sender.RaiseExceptionHandling<AMMTranSplit.qty>(row, newValue,
                e == null ? e : new PXSetPropertyException(e.MessageNoPrefix, PXErrorLevel.RowWarning,
                    sender.GetStateExt<AMMTranSplit.inventoryID>(row),
                    sender.GetStateExt<AMMTranSplit.subItemID>(row),
                    sender.GetStateExt<AMMTranSplit.siteID>(row),
                    sender.GetStateExt<AMMTranSplit.locationID>(row),
                    sender.GetValue<AMMTranSplit.lotSerialNbr>(row)));
        }

        public override void AMMTranSplit_InvtMult_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var cache = sender.Graph.Caches[typeof(AMMTran)];
            if (cache.Current == null || (AMMTranSplit)e.Row == null ||
                ((AMMTran)cache.Current).LineNbr != ((AMMTranSplit)e.Row).LineNbr)
            {
                return;
            }

#if DEBUG
            AMDebug.TraceWriteMethodName($"TranType = {((AMMTranSplit)e.Row).TranType} [{((AMMTran)cache.Current).TranType}]; InvtMult = {((AMMTranSplit)e.Row).InvtMult} [{((AMMTran)cache.Current).InvtMult}]; [{((AMMTran)cache.Current).DebuggerDisplay}]");
#endif
            if (e.Row != null && ((AMMTranSplit)e.Row).DocType == AMDocType.Material && ((AMMTran)cache.Current).IsByproduct.GetValueOrDefault())
            {
                e.NewValue = AMTranType.InvtMult(((AMMTranSplit)e.Row).TranType ?? AMTranType.Issue);
                e.Cancel = true;
                return;
            }

            e.NewValue = ((AMMTran)cache.Current).InvtMult;
        }
    }
}