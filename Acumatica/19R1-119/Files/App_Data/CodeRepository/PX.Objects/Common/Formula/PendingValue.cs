using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.Common
{
	public class PendingValue<Field> : BqlFormulaEvaluator, IBqlOperand
		where Field : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			return cache.GetValuePending<Field>(item);
		}
	}
}
