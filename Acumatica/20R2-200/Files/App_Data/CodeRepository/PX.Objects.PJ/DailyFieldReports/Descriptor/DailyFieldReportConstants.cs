using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.Descriptor
{
    public static class DailyFieldReportConstants
    {
        public static class Print
        {
            public const string DailyFieldReportId = "DFRID";
        }

        public static class Notification
        {
	        public const string Name = "Daily Field Report";

	        public sealed class name : BqlString.Constant<name>
	        {
		        public name()
			        : base(Name)
		        {
		        }
	        }
        }
	}
}