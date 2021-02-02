using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;

namespace PX.Objects.PJ.DailyFieldReports.PJ.CacheExtensions.PJ
{
    public sealed class ProjectIssueExt : PXCacheExtension<ProjectIssue>
    {
        [PXInt]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }
    }
}