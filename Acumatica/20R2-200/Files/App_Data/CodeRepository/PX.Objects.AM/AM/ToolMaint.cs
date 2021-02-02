using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Tool Maintenance
    /// </summary>
    public class ToolMaint : PXGraph<ToolMaint, AMToolMst>
    {
        public PXSelect<AMToolMst> Tools;
        public PXSelect<AMToolMst, Where<AMToolMst.toolID, Equal<Current<AMToolMst.toolID>>>> ToolSelected;

        protected virtual void AMToolMst_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            var toolRec = (AMToolMst) e.Row;
            if (toolRec == null)
            {
                return;
            }

            AMBomTool ambomtool = PXSelect<AMBomTool, 
                Where<AMBomTool.toolID, Equal<Required<AMBomTool.toolID>>>
                >.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (ambomtool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                     Messages.GetLocal(Messages.Tool_NotDeleted_OnBOM,
                            ambomtool.BOMID));
                e.Cancel = true;
                return;
            }

            AMProdTool amprodtool = PXSelect<AMProdTool, 
                Where<AMProdTool.toolID, Equal<Required<AMProdTool.toolID>>>
                >.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (amprodtool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                    Messages.GetLocal(Messages.Tool_NotDeleted_OnProd,
                        amprodtool.OrderType.TrimIfNotNullEmpty(),
                        amprodtool.ProdOrdID.TrimIfNotNullEmpty()));
                e.Cancel = true;
                return;
            }

            AMEstimateTool amestimatetool = PXSelect<AMEstimateTool, 
                Where<AMEstimateTool.toolID, Equal<Required<AMEstimateTool.toolID>>
                    >>.SelectWindowed(this, 0, 1, toolRec.ToolID);
            if (amestimatetool != null)
            {
                RaiseToolIDExceptionHandling(sender, toolRec,
                    Messages.GetLocal(Messages.Tool_NotDeleted_OnEstimate,
                        amestimatetool.EstimateID, amestimatetool.RevisionID));
                e.Cancel = true;
                return;
            }
        }

        protected virtual void RaiseToolIDExceptionHandling(PXCache cache, AMToolMst toolMst, string message)
        {
            cache.RaiseExceptionHandling<AMToolMst.toolID>(toolMst, toolMst.ToolID, new PXSetPropertyException(message,PXErrorLevel.Error));
        }
    }
}