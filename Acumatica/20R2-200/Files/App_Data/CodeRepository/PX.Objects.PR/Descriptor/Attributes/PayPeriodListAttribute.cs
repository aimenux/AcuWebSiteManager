using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PayPeriodType
	{
		public const string SemiMonth = "SM";
		public const string Month = FinPeriodType.Month;
		public const string BiMonth = FinPeriodType.BiMonth;
		public const string Quarter = FinPeriodType.Quarter;
		public const string Week = FinPeriodType.Week;
		public const string BiWeek = FinPeriodType.BiWeek;
		public const string CustomPeriodsNumber = FinPeriodType.CustomPeriodsNumber;

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SemiMonth, Month, BiMonth, Quarter, Week, BiWeek, CustomPeriodsNumber },
				new string[] { Messages.Semimonthly, Messages.Monthly, Messages.BiMonthly, Messages.Quarterly, Messages.Weekly, Messages.Biweekly, GL.Messages.PT_CustomPeriodsNumber }
				)
			{ }
		}

		public class week : PX.Data.BQL.BqlString.Constant<week>
		{
			public week() : base(Week)
			{
			}
		}

		public class biWeek : PX.Data.BQL.BqlString.Constant<biWeek>
		{
			public biWeek() : base(BiWeek)
			{
			}
		}

		public static FiscalPeriodSetupCreator.FPType GetFPType(string aFinPeriodType)
		{
			switch (aFinPeriodType)
			{
				case SemiMonth:
					return FiscalPeriodSetupCreator.FPType.Custom;
				default:
					return FinPeriodType.GetFPType(aFinPeriodType);
			}
		}

		public static bool IsSemiMonth(string aFinPeriodType)
		{
			return aFinPeriodType == SemiMonth;
		}

		public static bool IsCustom(PRPayGroupYearSetup periodSetup)
		{
			return periodSetup.FPType == FiscalPeriodSetupCreator.FPType.Custom && !IsSemiMonth(periodSetup.PeriodType);
		}
	}
}
