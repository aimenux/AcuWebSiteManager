using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Relations
{
    public class DrawingLogsMaintRelationsExtension : DrawingLogRelationsBaseExtension<DrawingLogsMaint>
    {
        public new static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected override DrawingLog GetCurrentDrawingLog()
        {
            return Base.DrawingLogs.Current;
        }

        protected override WebDialogResult ShowConfirmationDialog(string message)
        {
            return Base.DrawingLogs.Ask(SharedMessages.Warning, message, MessageButtons.OKCancel);
        }
    }
}