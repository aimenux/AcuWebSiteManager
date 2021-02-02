using System;
using PX.Data;
using System.Text;

namespace PX.Objects.CS
{
	public class AUSetMaint : PXGraph<AUSetMaint, AUSet>
	{
		public PXSelect<AUSet> Set;

		public PXSelect<AUStep,
			Where<AUStep.setCode, Equal<Current<AUSet.setCode>>>>
			Steps;

		public PXSelect<AUFilter,
			Where<AUFilter.setCode, Equal<Current<AUSet.setCode>>,
			And<AUFilter.stepCode, Equal<Current<AUSet.setCode>>,
			And<AUFilter.isDefault, Equal<boolTrue>>>>>
			Filters;
	}
}
