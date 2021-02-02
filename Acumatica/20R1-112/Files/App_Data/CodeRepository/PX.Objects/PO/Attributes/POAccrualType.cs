using PX.Data;

namespace PX.Objects.PO
{
	public static class POAccrualType
	{
		public const string Receipt = "R";
		public const string Order = "O";

		public class receipt : PX.Data.BQL.BqlString.Constant<receipt>
		{
			public receipt()
				: base(Receipt)
			{; }
		}

		public class order : PX.Data.BQL.BqlString.Constant<order>
		{
			public order()
				: base(Order)
			{; }
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new[]
				{
					Pair(Receipt, Messages.Receipt),
					Pair(Order, Messages.Order),
				})
			{; }
		}
	}
}
