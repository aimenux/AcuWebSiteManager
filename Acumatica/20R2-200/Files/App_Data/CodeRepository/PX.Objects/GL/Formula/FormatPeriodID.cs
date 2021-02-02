using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.GL
{
	public class FormatDirection
	{
		public const string Display = "D";
		public const string Store = "S";
		public const string Error = "E";

		public sealed class display : PX.Data.BQL.BqlString.Constant<display>
		{
			public display() : base(Display) { }
		}

		public sealed class store : PX.Data.BQL.BqlString.Constant<store>
		{
			public store() : base(Store) { }
		}

		public sealed class error : PX.Data.BQL.BqlString.Constant<error>
		{
			public error() : base(Error) { }
		}
	}

	public class FormatPeriodID<Direction, PeriodID> : BqlFormulaEvaluator<Direction, PeriodID>, IBqlOperand
		where Direction : IBqlOperand
		where PeriodID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			string direction = (string)parameters[typeof(Direction)];
			string periodID = (string)parameters[typeof(PeriodID)];

			switch (direction)
			{
				case FormatDirection.Display:
					return FinPeriodIDFormattingAttribute.FormatForDisplay(periodID);
				case FormatDirection.Store:
					return FinPeriodIDFormattingAttribute.FormatForStoring(periodID);
				case FormatDirection.Error:
					return FinPeriodIDFormattingAttribute.FormatForError(periodID);
				default:
					return null;
			}
		}
	}
}
