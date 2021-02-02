using PX.Objects.PJ.DailyFieldReports.PM.CacheExtensions;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.DailyFieldReports.EP.CacheExtensions
{
    public sealed class EpExpenseClaimDetailsExt : PXCacheExtension<EPExpenseClaimDetails>
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

        public abstract class dailyFieldReportId : BqlInt.Field<EpExpenseClaimDetailsExt.dailyFieldReportId>
        {
        }
    }
}
