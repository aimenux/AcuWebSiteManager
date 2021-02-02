using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.IN.RelatedItems
{
	public class InventoryRelation
	{
		public const string CrossSell = "CSELL";
		public const string UpSell = "USELL";
		public const string Related = "RLTD";
		public const string Substitute = "SUBST";
		public const string Alternative = "ALT";

		[PXLocalizable]
		public static class Desc
		{
			public const string CrossSell = "Cross-Sell";
			public const string UpSell = "Up-Sell";
			public const string Related = "Related";
			public const string Substitute = "Substitute";
			public const string Alternative = "Alternative";
		}

		public class ListAttribute: PXStringListAttribute
		{
			public ListAttribute()
			: base((CrossSell, Desc.CrossSell),
				  (UpSell, Desc.UpSell),
				  (Related, Desc.Related),
				  (Substitute, Desc.Substitute),
				  (Alternative, Desc.Alternative))
			{ }
		}

		public class crossSell : BqlString.Constant<crossSell> { public crossSell() : base(CrossSell) { } };
		public class upSell : BqlString.Constant<upSell> { public upSell() : base(UpSell) { } };
		public class related : BqlString.Constant<related> { public related() : base(Related) { } };
		public class substitute : BqlString.Constant<substitute> { public substitute() : base(Substitute) { } };
		public class alternative : BqlString.Constant<alternative> { public alternative() : base(Alternative) { } };
	}
}
