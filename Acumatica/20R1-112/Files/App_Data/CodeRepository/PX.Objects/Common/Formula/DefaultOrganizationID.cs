using System;
using System.Collections.Generic;

using PX.Data;

using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.Common
{
	public class DefaultOrganizationID : BqlFormulaEvaluator
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> parameters)
		{
			Organization organization = PXSelectJoin<Organization,
				InnerJoin<Branch,
					On<Organization.organizationID, Equal<Branch.organizationID>>>,
				Where<Organization.organizationType, NotEqual<OrganizationTypes.withoutBranches>,
					And<Branch.branchID, Equal<Current<AccessInfo.branchID>>>>>.Select(cache.Graph);
			return organization?.OrganizationID;
		}
	}
}
