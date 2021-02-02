using PX.Data;

namespace PX.Objects.PR
{
	public class IncomeCalculationType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SpecificList, Formula },
				new string[] { Messages.SpecificList, Messages.Formula })
			{ }
		}

		public const string SpecificList = "SPC";
		public const string Formula = "FRM";
	}
}
