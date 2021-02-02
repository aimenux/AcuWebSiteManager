using PX.Api.Payroll;
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace PX.Objects.PR
{
	[Serializable]
	public class PRStateInitializer : PXPayrollAssemblyScope<PayrollMetaProxy>, IStateDefinitionInitializer
	{
		public List<PRState> InitializeStateDefinitions(Func<Assembly, List<PRState>> stateDefinitionDelegate)
		{
			return CreateProxy().InitializeStateDefinitions(stateDefinitionDelegate);
		}
	}
}
