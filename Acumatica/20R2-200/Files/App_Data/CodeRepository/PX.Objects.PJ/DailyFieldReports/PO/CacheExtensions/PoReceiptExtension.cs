using PX.Data;
using PX.Data.BQL;
using PX.Objects.PO;

namespace PX.Objects.PJ.DailyFieldReports.PO.CacheExtensions
{
    public sealed class PoReceiptExtension : PXCacheExtension<POReceipt>
    {
        [PXInt]
        public int? DailyFieldReportId
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return false;
        }

        public abstract class dailyFieldReportId : BqlInt.Field<dailyFieldReportId>
        {
        }
    }
}