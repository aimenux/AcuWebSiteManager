using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.PJ.DrawingLogs.PJ.GraphExtensions.Revisions
{
    public class DrawingLogsMaintRevisionsExtension : DrawingLogRevisionsBaseExtension<DrawingLogsMaint>
    {
        public new static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public void _(Events.RowDeleting<DrawingLog> args)
        {
            CheckRevisionsAvailability(args.Row?.NoteID);
        }

        protected override WebDialogResult ShowConfirmationDialog(string message)
        {
            return Base.DrawingLogs.Ask(SharedMessages.Warning, message, MessageButtons.OKCancel);
        }
    }
}