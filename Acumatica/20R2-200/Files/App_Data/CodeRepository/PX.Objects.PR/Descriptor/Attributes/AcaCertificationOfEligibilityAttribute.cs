using PX.Data;
using System;

namespace PX.Objects.PR
{
	public class AcaCertificationOfEligibility
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { QualifyingOfferMethod, NinetyEightPctMethod },
				new string[]
				{
					Messages.QualifyingOfferMethod, 
					Messages.NinetyEightPctOfferMethod
				})
			{
			}
		}

		public class qualifyingOfferMethod : PX.Data.BQL.BqlString.Constant<qualifyingOfferMethod>
		{
			public qualifyingOfferMethod() : base(QualifyingOfferMethod) { }
		}

		public class ninetyEightPctMethod : PX.Data.BQL.BqlString.Constant<ninetyEightPctMethod>
		{
			public ninetyEightPctMethod() : base(NinetyEightPctMethod) { }
		}

		public const string QualifyingOfferMethod = "QOM";
		public const string NinetyEightPctMethod = "98P";
	}
}
