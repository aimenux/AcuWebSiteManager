using System;
using PX.Data;

namespace PX.Objects.RUTROT
{
	public class RUTROTTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { RUT, ROT }, new string[] { RUTROTMessages.RUTType, RUTROTMessages.ROTType }) { }
		}

		public const string RUT = "U";

		public const string ROT = "O";

		public class rut : PX.Data.BQL.BqlString.Constant<rut>
		{
			public rut() : base(RUT) { }
		}

		public class rot : PX.Data.BQL.BqlString.Constant<rot>
		{
			public rot() : base(ROT) { }
		}
	}

	public class RUTROTItemTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(new string[] { Service, MaterialCost, OtherCost }, 
					  new string[] { RUTROTMessages.Service, RUTROTMessages.MaterialCost, RUTROTMessages.OtherCost }) { }
		}

        public class CostsListAttribute : PXStringListAttribute
        {
            public CostsListAttribute()
                : base(new string[] { MaterialCost, OtherCost }, new string[] { RUTROTMessages.MaterialCost, RUTROTMessages.OtherCost }) {; }
        }

        public const string Service = "S";

		public const string MaterialCost = "M";

		public const string OtherCost = "O";

		public class service : PX.Data.BQL.BqlString.Constant<service>
		{
			public service() : base(Service) { }
		}

		public class materialCost : PX.Data.BQL.BqlString.Constant<materialCost>
		{
			public materialCost() : base(MaterialCost) { }
		}

		public class otherCost : PX.Data.BQL.BqlString.Constant<otherCost>
		{
			public otherCost() : base(OtherCost) { }
		}
	}

	public class WorkTypeSelectorAttribute : PXSelectorAttribute
	{
		internal static Type ComposeSearchCommand(Type dateField)
		{
			if (!typeof(IBqlField).IsAssignableFrom(dateField))
			{
				throw new PXArgumentException(nameof(dateField));
			}
			return BqlCommand.Compose(
				typeof(Search<,>),
				typeof(RUTROTWorkType.workTypeID),
				typeof(Where2<,>),
				typeof(Where<,,>),
				typeof(RUTROTWorkType.endDate),
				typeof(Greater<>),
				typeof(Current<>),
				dateField,
				typeof(Or<,>),
				typeof(RUTROTWorkType.endDate),
				typeof(IsNull),
				typeof(And<,,>),
				typeof(RUTROTWorkType.startDate),
				typeof(LessEqual<>),
				typeof(Current<>),
				dateField,
				typeof(And<RUTROTWorkType.rUTROTType, Equal<Current2<RUTROT.rUTROTType>>, Or<Current2<RUTROT.rUTROTType>, IsNull>>));
		}

		public WorkTypeSelectorAttribute(Type date) : base(ComposeSearchCommand(date))
		{
			SubstituteKey = typeof(RUTROTWorkType.description);
			DescriptionField = typeof(RUTROTWorkType.xmlTag);
		}
	}

    public class RUTROTBalanceOn
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(new string[] { Release, Claim }, new string[] { RUTROTMessages.Release, RUTROTMessages.Claim }) { }
        }

        public const string Release = "R";

        public const string Claim = "C";

        public class release : PX.Data.BQL.BqlString.Constant<release>
		{
            public release() : base(Release) { }
        }

        public class claim : PX.Data.BQL.BqlString.Constant<claim>
		{
            public claim() : base(Claim) { }
        }
    }
}
