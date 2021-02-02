using PX.Common;
using PX.Data;

namespace PX.Objects.IN.RelatedItems
{
	public class InventoryRelationTag
	{
		public const string Complementary = "COMP";
		public const string Interested = "INTS";
		public const string UsersBought = "USER";
		public const string Accessory = "ESNT";
		public const string Service = "SERV";
		public const string Premium = "PREM";
		public const string Custom = "CUST";
		public const string Option = "OPTN";
		public const string Promotional = "PROM";
		public const string Popular = "POPL";
		public const string Seasonal = "SEAS";
		public const string Related = "RLTD";
		public const string Substitute = "SUBS";
		public const string Alternative = "ALTN";

		[PXLocalizable]
		public static class Desc
		{
			public const string Complementary = "Complementary Items";
			public const string Interested = "Items of Interest";
			public const string UsersBought = "Other Users Bought";
			public const string Accessory = "Essential Related Products";
			public const string Service = "Services";
			public const string Premium = "Premium";
			public const string Custom = "Customization";
			public const string Option = "Options";
			public const string Promotional = "Promotional";
			public const string Popular = "Popular";
			public const string Seasonal = "Seasonal";
			public const string Related = "Related";
			public const string Substitute = "Substitute";
			public const string Alternative = "Alternative";
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
			: base(
				  (Complementary,	Desc.Complementary),
				  (Interested,		Desc.Interested),
				  (UsersBought,		Desc.UsersBought),
				  (Accessory,		Desc.Accessory),
				  (Service,			Desc.Service),
				  (Premium,			Desc.Premium),
				  (Custom,			Desc.Custom),
				  (Option,			Desc.Option),
				  (Promotional,		Desc.Promotional),
				  (Popular,			Desc.Popular),
				  (Seasonal,		Desc.Seasonal),
				  (Related,			Desc.Related),
				  (Substitute,		Desc.Substitute),
				  (Alternative,		Desc.Alternative))
			{ }
		}
	}
}
