using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PR
{
	public class EarningDetailSourceType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { TimeActivity, QuickPay, SalesCommission },
				new string[] { Messages.TimeActivity, Messages.QuickPay, Messages.SalesCommission })
			{ }
		}

		public class timeActivity : BqlString.Constant<timeActivity>
		{
			public timeActivity() : base(TimeActivity) { }
		}

		public const string TimeActivity = "TMA";
		public const string QuickPay = "QPY";
		public const string SalesCommission = "SPC"; //Sales Person Commission
	}
}
