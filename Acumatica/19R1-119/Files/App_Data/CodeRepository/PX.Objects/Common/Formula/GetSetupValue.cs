using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.Common
{
	public class GetSetupValue<Field> : BqlFormulaEvaluator<Field>, IBqlOperand
		where Field : IBqlField
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			Type table = BqlCommand.GetItemType(typeof(Field));
			PXCache cacheSetup = cache.Graph.Caches[table];
			string fieldName = cacheSetup.GetField(typeof(Field));

			object setup = new PXView(cache.Graph, true, BqlCommand.CreateInstance(typeof(Select<>), table)).SelectSingle();

			return cacheSetup.GetValue(setup, fieldName);
		}
	}
}
