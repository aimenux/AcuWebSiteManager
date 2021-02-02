using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class AcaHealthPlanType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { MeetsEssentialCoverageAndValue, MeetsEssentialCoverage, SelfInsured, NoneOfTheAbove },
				new string[]
				{
					Messages.MeetsEssentialCoverageAndValue,
					Messages.MeetsEssentialCoverage,
					Messages.SelfInsured,
					Messages.NoneOfTheAbove,
				})
			{
			}
		}

		public class meetsEssentialCoverageAndValue : PX.Data.BQL.BqlString.Constant<meetsEssentialCoverageAndValue>
		{
			public meetsEssentialCoverageAndValue() : base(MeetsEssentialCoverageAndValue) { }
		}

		public class meetsEssentialCoverage : PX.Data.BQL.BqlString.Constant<meetsEssentialCoverage>
		{
			public meetsEssentialCoverage() : base(MeetsEssentialCoverage) { }
		}

		public class selfInsured : PX.Data.BQL.BqlString.Constant<selfInsured>
		{
			public selfInsured() : base(SelfInsured) { }
		}

		public class noneOfTheAbove : PX.Data.BQL.BqlString.Constant<noneOfTheAbove>
		{
			public noneOfTheAbove() : base(NoneOfTheAbove) { }
		}

		public const string MeetsEssentialCoverageAndValue = "MCV";
		public const string MeetsEssentialCoverage = "MEC";
		public const string SelfInsured = "SEL";
		public const string NoneOfTheAbove = "NON";
	}
}
