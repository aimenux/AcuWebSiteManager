using PX.Data;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    public class LSAMMove : LSAMMTran
    {
        public LSAMMove(PXGraph graph) : base(graph)
        {
            graph.FieldDefaulting.AddHandler<AMMTran.tranType>(AMMTran_TranType_FieldDefaulting);
            graph.FieldUpdated.AddHandler<AMMTran.lastOper>(AMMTran_LastOper_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMMTran.qty>(AMMTran_Qty_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMMTran.operationID>(AMMTran_OperationID_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMMTran.isScrap>(AMMTran_IsScrap_FieldUpdated);
            graph.FieldUpdated.AddHandler<AMMTranSplit.invtMult>(AMMTranSplit_InvtMult_FieldUpdated);
            graph.RowSelected.AddHandler<AMMTran>(AMMTran_RowSelected);
        }

        protected virtual void AMMTranSplit_InvtMult_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            PXCache pxCache = sender.Graph.Caches[typeof(AMMTran)];
            if (pxCache.Current == null)
            {
                return;
            }
            if (e.Row != null)
            {
                int? lineNbr1 = ((AMMTran)pxCache.Current).LineNbr;
                int? lineNbr2 = ((AMMTranSplit)e.Row).LineNbr;
                if ((lineNbr1.GetValueOrDefault() != lineNbr2.GetValueOrDefault() ? 0 : (lineNbr1.HasValue == lineNbr2.HasValue ? 1 : 0)) == 0)
                {
                    return;
                }
                ((AMMTranSplit)e.Row).TranType = ((AMMTranSplit)e.Row).InvtMult < 1 ? AMTranType.Adjustment : AMTranType.Receipt;
            }
        }

        protected override void Detail_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
        {
            base.Detail_RowInserting(sender, e);
            var row = (AMMTranSplit) e.Row;
            if (row == null)
            {
                return;
            }

            var rowParent = (AMMTran)PXParentAttribute.SelectParent(sender, row, typeof(AMMTran));
            if (rowParent == null)
            {
                return;
            }

            row.TranType = rowParent.TranType ?? row.TranType;
            row.InvtMult = AMTranType.InvtMult(row.TranType, rowParent.Qty);
        }

        protected virtual void AMMTran_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (AMMTran)e.Row;
            if (row == null || string.IsNullOrWhiteSpace(row.ProdOrdID))
            {
                return;
            }

            AllowDetail(!IsNegativeMove(row) && !IsScrap(row) && row.LastOper.GetValueOrDefault());
        }

        protected virtual void AllowDetail(bool allow)
        {
            DetailCache.AllowInsert = allow && MasterCache.AllowInsert;
            DetailCache.AllowUpdate = allow && MasterCache.AllowUpdate;
        }

        protected static bool IsNegativeMove(AMMTran ammTran)
        {
            return ammTran?.InventoryID != null && ammTran.Qty.GetValueOrDefault() < 0;
        }

        protected virtual bool IsNegativeMove(AMMTranSplit ammTranSplit)
        {
            var ammTran = GetAMMTran(ammTranSplit);
            return ammTran?.InventoryID != null && ammTran.Qty.GetValueOrDefault() < 0;
        }

        protected static bool IsScrap(AMMTran ammTran)
        {
            return ammTran?.InventoryID != null && ammTran.IsScrap == true;
        }

        protected virtual bool IsScrap(AMMTranSplit ammTranSplit)
        {
            var ammTran = GetAMMTran(ammTranSplit);
            return ammTran?.InventoryID != null && ammTran.IsScrap == true;
        }

        protected virtual AMMTran GetAMMTran(AMMTranSplit ammTranSplit)
        {
            return GetAMMTran(ammTranSplit.DocType, ammTranSplit.BatNbr, ammTranSplit.LineNbr.GetValueOrDefault());
        }

        protected virtual AMMTran GetAMMTran(string docType, string batNbr, int lineNbr)
        {
            var ammTran =
                (AMMTran)MasterCache.Locate(new AMMTran()
                {
                    DocType = docType,
                    BatNbr = batNbr,
                    LineNbr = lineNbr
                });

            if (ammTran != null)
            {
                return ammTran;
            }

            return PXSelect<AMMTran,
                Where<AMMTran.docType, Equal<Required<AMMTran.docType>>,
                    And<AMMTran.batNbr, Equal<Required<AMMTran.batNbr>>,
                    And<AMMTran.lineNbr, Equal<Required<AMMTran.lineNbr>>>>>>.Select(_Graph, docType, batNbr, lineNbr);
        }

        protected virtual void AMMTran_LastOper_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetTranTypeInvtMult(sender, (AMMTran)e.Row);
        }

        protected virtual void AMMTran_Qty_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetTranTypeInvtMult(sender, (AMMTran)e.Row);
        }

        protected virtual void AMMTran_TranType_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            SetTranTypeInvtMult(cache, (AMMTran)e.Row);
        }

        protected virtual void AMMTran_OperationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var ammTran = (AMMTran)e.Row;
            if (!string.IsNullOrWhiteSpace(ammTran?.ProdOrdID) && ammTran.OperationID != null)
            {
                SetTranTypeInvtMult(sender, ammTran);
            }
        }

        protected virtual void AMMTran_IsScrap_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            SetTranTypeInvtMult(sender, (AMMTran)e.Row);
        }

        protected virtual void SetTranTypeInvtMult(PXCache cache, AMMTran ammTran)
        {
            if (ammTran == null)
            {
                return;
            }
#if DEBUG
            var tranTypeOld = ammTran.TranType;
            var invtMultOld = ammTran.InvtMult;
#endif
            var tranTypeNew = ammTran.Qty.GetValueOrDefault() < 0 ?
                AMTranType.Adjustment : AMTranType.Receipt;
            var invtMultNew = ammTran.LastOper.GetValueOrDefault() || ammTran.IsScrap == true
                ? AMTranType.InvtMult(tranTypeNew, ammTran.Qty)
                : 0;

#if DEBUG
            AMDebug.TraceWriteMethodName($"TranType = {tranTypeNew} (old value = {tranTypeOld}); InvtMult = {invtMultNew} (old value = {invtMultOld})");
#endif
            var syncSplits = false;
            if (invtMultNew != ammTran.InvtMult)
            {
                syncSplits |= ammTran.InvtMult != null;
                cache.SetValueExt<AMMTran.invtMult>(ammTran, invtMultNew);
            }

            if (tranTypeNew != ammTran.TranType)
            {
                syncSplits |= ammTran.TranType != null;
                cache.SetValueExt<AMMTran.tranType>(ammTran, tranTypeNew);
            }

            if (syncSplits)
            {
                SyncSplitTranType(cache, ammTran, cache.Graph.Caches[typeof(AMMTranSplit)]);
            }
        }
    }
}