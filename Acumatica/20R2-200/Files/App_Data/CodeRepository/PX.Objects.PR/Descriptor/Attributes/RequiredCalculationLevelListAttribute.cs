using PX.Data;

namespace PX.Objects.PR
{
	public class RequiredCalculationLevel
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { CalculateAll, CalculateTaxesAndNetPay, CalculateNetPayOnly,
					NoCalculationRequired },
				new string[] { Messages.CalculateAll, Messages.CalculateTaxesAndNetPay, Messages.CalculateNetPayOnly,
					Messages.NoCalculationRequired })
			{ }
		}

		public const string CalculateAll = "ALL";
		public const string CalculateTaxesAndNetPay = "TAX";
		public const string CalculateNetPayOnly = "NET";
		public const string NoCalculationRequired = "NON";
	}
}
