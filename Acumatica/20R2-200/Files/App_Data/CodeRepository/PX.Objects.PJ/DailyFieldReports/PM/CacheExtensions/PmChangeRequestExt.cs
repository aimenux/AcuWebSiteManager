using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions
{
    public sealed class PmChangeRequestExt : PXCacheExtension<PMChangeRequest>
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