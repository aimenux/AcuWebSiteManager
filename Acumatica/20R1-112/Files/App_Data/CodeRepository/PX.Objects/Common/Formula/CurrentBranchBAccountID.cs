using System;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.GL;

namespace PX.Objects.Common
{
	public class CurrentBranchBAccountID : BqlFormulaEvaluator
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			Branch currentBranch = PXSelect<Branch, Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>.Select(cache.Graph);
			return currentBranch?.BAccountID;
		}
	}
}
