using PX.Data;

namespace PX.Objects.PR
{
	public class BankAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Checking, Savings },
				new string[] { Messages.Checking, Messages.Savings })
			{ }
		}

		public const string Checking = "CHK";
		public const string Savings = "SAV";
	}
}
