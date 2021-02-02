using PX.Data;

namespace PX.Objects.PR
{
	public class BatchStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Hold, Balanced, Open, Closed },
				new string[] { Messages.Hold, Messages.Balanced, Messages.Open, Messages.Closed })
			{ }
		}

		public class hold : Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { }
		}

		public class balanced : Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { }
		}

		public class open : Data.BQL.BqlString.Constant<open>
		{
			public open() : base(Open) { }
		}

		public class closed : Data.BQL.BqlString.Constant<closed>
		{
			public closed() : base(Closed) { }
		}

		public const string Hold = "HLD";
		public const string Balanced = "BLN";
		public const string Open = "OPN";
		public const string Closed = "CLS";
	}
}
