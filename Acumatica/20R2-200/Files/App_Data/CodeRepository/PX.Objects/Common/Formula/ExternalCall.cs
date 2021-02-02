using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.Common
{
	public class ExternalCall : BqlFormulaEvaluator, IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			return IsExternalCall;
		}
	}
}
