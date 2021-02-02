using PX.Data;
using PX.TaxProvider;

namespace PX.Objects.TX
{
	public static class TXAvalaraCustomerUsageType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FederalGovt, Messages.FederalGovt),
					Pair(StateLocalGovt, Messages.StateLocalGovt),
					Pair(TribalGovt, Messages.TribalGovt),
					Pair(ForeignDiplomat, Messages.ForeignDiplomat),
					Pair(CharitableOrg, Messages.CharitableOrg),
					Pair(Religious, Messages.Religious),
					Pair(Resale, Messages.Resale),
					Pair(AgriculturalProd, Messages.AgriculturalProd),
					Pair(IndustrialProd, Messages.IndustrialProd),
					Pair(DirectPayPermit, Messages.DirectPayPermit),
					Pair(DirectMail, Messages.DirectMail),
					Pair(Other, Messages.Other),
					Pair(Education, Messages.Education),
					Pair(LocalGovt, Messages.LocalGovt),
					Pair(ComAquaculture, Messages.ComAquaculture),
					Pair(ComFishery, Messages.ComFishery),
					Pair(NonResident, Messages.NonResident),
					Pair(Default, Messages.Default)
				}) { }
		}

		public const string FederalGovt = "A";
		public const string StateLocalGovt = "B";
		public const string TribalGovt = "C";
		public const string ForeignDiplomat = "D";
		public const string CharitableOrg = "E";
		public const string Religious = "F";
		public const string Resale = "G";
		public const string AgriculturalProd = "H";
		public const string IndustrialProd = "I";
		public const string DirectPayPermit = "J";
		public const string DirectMail = "K";
		public const string Other = "L";
		public const string Education = "M";
		public const string LocalGovt = "N";
		public const string ComAquaculture = "P";
		public const string ComFishery = "Q";
		public const string NonResident = "R";
		public const string Default = EntityUsageType.Default;
	}
}
