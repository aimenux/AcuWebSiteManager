using System;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.Common;

namespace PX.Objects.AR
{
	public class ARStatementScheduleType : ILabelProvider
	{
		public const string Weekly = "W";
		public const string TwiceAMonth = "C";
		public const string FixedDayOfMonth = "F";
		public const string EndOfMonth = "E";
		public const string EndOfPeriod = "P";
		[Obsolete("This constant is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(TwiceAMonth) + "instead.")]
		public const string Custom = TwiceAMonth;

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ Weekly, Messages.Weekly },
			{ TwiceAMonth, Messages.TwiceAMonth },
			{ FixedDayOfMonth, Messages.FixedDayOfMonth },
			{ EndOfMonth, Messages.EndOfMonth },
			{ EndOfPeriod, Messages.EndOfPeriod },
		};

		public class weekly : PX.Data.BQL.BqlString.Constant<weekly>
		{
			public weekly() : base(Weekly) { }
		}

		public class twiceAMonth : PX.Data.BQL.BqlString.Constant<twiceAMonth>
		{
			public twiceAMonth() : base(TwiceAMonth) { }
		}

		public class fixedDayOfMonth : PX.Data.BQL.BqlString.Constant<fixedDayOfMonth>
		{
			public fixedDayOfMonth() : base(FixedDayOfMonth) { }
		}

		public class endOfMonth : PX.Data.BQL.BqlString.Constant<endOfMonth>
		{
			public endOfMonth() : base(EndOfMonth) { }
		}

		public class endOfPeriod : PX.Data.BQL.BqlString.Constant<endOfPeriod>
		{
			public endOfPeriod() : base(EndOfPeriod) { }
		}

		[Obsolete("This attribute is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(LabelListAttribute) + " instead.")]
		public class ListAttribute : PXStringListAttribute { }
	}

	[Obsolete("This type is not used anymore and will be removed in Acumatica ERP 8.0. Please use " + nameof(ARStatementScheduleType) + " instead.")]
	public class PrepareOnType : ARStatementScheduleType { }
}
