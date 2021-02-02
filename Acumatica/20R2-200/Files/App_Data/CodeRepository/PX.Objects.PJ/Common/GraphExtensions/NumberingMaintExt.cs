using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.Common.GraphExtensions
{
    public class NumberingMaintExt : PXGraphExtension<NumberingMaint>
    {
        [InjectDependency]
        public INumberingSequenceUsage NumberingSequenceUsage
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        protected virtual void _(Events.RowDeleting<Numbering> args)
        {
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.projectIssueNumberingId>(
                    args.Row, Base, CacheNames.ProjectIssue);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup,
                    ProjectManagementSetup.requestForInformationNumberingId>(
                    args.Row, Base, CacheNames.RequestForInformation);
            NumberingSequenceUsage
                .CheckForNumberingUsage<DrawingLogSetup, DrawingLogSetup.drawingLogNumberingSequenceId>(
                    args.Row, Base, CacheNames.DrawingLog);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.dailyFieldReportNumberingId>(
                    args.Row, Base, CacheNames.DailyFieldReport);
            NumberingSequenceUsage
                .CheckForNumberingUsage<ProjectManagementSetup, ProjectManagementSetup.submittalNumberingId>(
                    args.Row, Base, CacheNames.Submittal);
        }
    }
}