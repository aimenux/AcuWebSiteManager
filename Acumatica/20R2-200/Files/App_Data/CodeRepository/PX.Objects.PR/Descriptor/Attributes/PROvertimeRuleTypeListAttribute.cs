using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PR
{
	public class PROvertimeRuleType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Daily, Weekly },
				new string[] { Messages.DailyRule, Messages.WeeklyRule })
			{ }
		}

		public class daily : BqlString.Constant<daily>
		{
			public daily() : base(Daily) { }
		}

		public class weekly : BqlString.Constant<weekly>
		{
			public weekly() : base(Weekly) { }
		}

		public const string Daily = "DAY";
		public const string Weekly = "WEK";
	}
}
