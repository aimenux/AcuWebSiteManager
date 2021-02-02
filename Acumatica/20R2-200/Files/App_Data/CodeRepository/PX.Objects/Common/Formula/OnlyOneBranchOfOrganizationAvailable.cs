using System;
using System.Collections.Generic;

using PX.Data;

namespace PX.Objects.Common.Formula
{
	/// <exclude/>
	public sealed class OnlyOneBranchOfOrganizationAvailable<OrganizationID> : BqlFormulaEvaluator<OrganizationID>, IBqlOperand
		where OrganizationID : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			int? organizationID = (int?)parameters[typeof(OrganizationID)];
			return (PXAccess.GetChildBranchIDs(organizationID).Length < 2);
		}
	}
}
