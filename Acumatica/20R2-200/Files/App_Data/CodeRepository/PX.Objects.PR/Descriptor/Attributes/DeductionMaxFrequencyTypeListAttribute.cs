using PX.Data;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public class DeductionMaxFrequencyType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { NoMaximum, PerPayPeriod, PerCalendarYear },
				new string[] { Messages.NoMaximum, Messages.PerPayPeriod, Messages.PerCalendarYear })
			{ }
		}

		public class noMaximum : PX.Data.BQL.BqlString.Constant<noMaximum>
		{
			public noMaximum()
				: base(NoMaximum)
			{
			}
		}

		public static PRBenefitMaximumFrequency ToEnum(string value)
		{
			PRBenefitMaximumFrequency enumValue = PRBenefitMaximumFrequency.NoMaximum;

			if (value == PerPayPeriod)
			{
				enumValue = PRBenefitMaximumFrequency.PerPayPeriod;
			}
			else if (value == PerCalendarYear)
			{
				enumValue = PRBenefitMaximumFrequency.PerCalendarYear;
			}

			return enumValue;
		}

		public const string NoMaximum = "NOM";
		public const string PerPayPeriod = "PAY";
		public const string PerCalendarYear = "CAL";
	}
}
