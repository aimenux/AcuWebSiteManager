using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.GL.Formula
{
	public class OrganizationOfBranch<BranchID> : BqlFormulaEvaluator<BranchID>, IBqlOperand
		where BranchID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			int? branchID = (int?)parameters[typeof(BranchID)];
			return PXAccess.GetParentOrganizationID(branchID);
		}
	}
}
