using PX.Data;
using System;
using System.Collections.Generic;

using static PX.Objects.GL.FinPeriodStatusProcess;

namespace PX.Objects.GL.Formula
{
	public class IsDirectFinPeriodAction<Action> : BqlFormulaEvaluator<Action>, IBqlOperand
		where Action : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			string action = (string)parameters[typeof(Action)];
			return action == null ||
				action == FinPeriodStatusProcessParameters.action.Undefined ||
				FinPeriodStatusProcessParameters.action.GetDirection(action) == FinPeriodStatusProcessParameters.action.Direction.Direct;
		}
	}
}
