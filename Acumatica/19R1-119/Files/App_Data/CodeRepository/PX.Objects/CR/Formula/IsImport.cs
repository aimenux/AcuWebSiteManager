using System.Collections.Generic;
using PX.Data;
using PX.Data.SQLTree;

namespace PX.Objects.CR
{
	class IsImport : IBqlOperand, IBqlCreator
	{
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			value = cache.Graph.IsImport && !cache.Graph.IsMobile;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) 
		{
			return true;
		}
	}
}
