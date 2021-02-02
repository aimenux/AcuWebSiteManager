using PX.Data;

namespace PX.Objects.Common.Discount
{
	public static class DiscountOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Percent, AR.Messages.Percent),
					Pair(Amount, AR.Messages.Amount),
					Pair(FreeItem, AR.Messages.FreeItem),
				}) { }
		}

		public const string Percent = "P";
		public const string Amount = "A";
		public const string FreeItem = "F";

		public class PercentDiscount : PX.Data.BQL.BqlString.Constant<PercentDiscount> { public PercentDiscount() : base(Percent) { } }
		public class AmountDiscount : PX.Data.BQL.BqlString.Constant<AmountDiscount> { public AmountDiscount() : base(Amount) { } }
		public class FreeItemDiscount : PX.Data.BQL.BqlString.Constant<FreeItemDiscount> { public FreeItemDiscount() : base(FreeItem) { } }
	}
}