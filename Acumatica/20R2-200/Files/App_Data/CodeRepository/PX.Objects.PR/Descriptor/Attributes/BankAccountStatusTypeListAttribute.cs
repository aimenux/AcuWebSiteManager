using PX.Data;

namespace PX.Objects.PR
{
	public class BankAccountStatusType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Active, Inactive },
				new string[] { Messages.Active, Messages.Inactive })
			{ }
		}

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active)
			{
			}
		}

		public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
		{
			public inactive() : base(Inactive)
			{
			}
		}

		public const string Active = "A";
		public const string Inactive = "I";
	}
}
