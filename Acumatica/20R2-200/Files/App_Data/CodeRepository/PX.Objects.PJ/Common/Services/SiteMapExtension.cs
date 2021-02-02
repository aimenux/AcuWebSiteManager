using PX.Objects.PJ.Common.Descriptor;
using PX.Common;

namespace PX.Objects.PJ.Common.Services
{
    public class SiteMapExtension
    {
        public static bool IsDailyFieldReportScreen()
        {
            return GetScreenId() == ScreenIds.DailyFieldReport;
        }

        public static bool IsClearDailyFieldReportWeatherProcessingLogScreen()
        {
            return GetScreenId() == ScreenIds.ClearDfrWeatherProcessingLog;
        }

        public static string GetScreenId()
        {
            return PXContext.GetScreenID().Replace(".", string.Empty);
        }
    }
}
