using PX.Data;

namespace PX.Objects.PR
{
	public class CarryoverType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, Partial, Total, PaidOnTimeLimit },
				new string[] { Messages.None, Messages.Partial, Messages.Total, Messages.PaidOnTimeLimit })
			{ }
		}

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(None) { }
		}

		public class partial : PX.Data.BQL.BqlString.Constant<partial>
		{
			public partial() : base(Partial) { }
		}

		public class total : PX.Data.BQL.BqlString.Constant<total>
		{
			public total() : base(Total) { }
		}

		public class paidOnTimeLimit : PX.Data.BQL.BqlString.Constant<paidOnTimeLimit>
		{
			public paidOnTimeLimit() : base(PaidOnTimeLimit) { }
		}

		public const string None = "N";
		public const string Partial = "P";
		public const string Total = "T";
		public const string PaidOnTimeLimit = "L";
	}
}
