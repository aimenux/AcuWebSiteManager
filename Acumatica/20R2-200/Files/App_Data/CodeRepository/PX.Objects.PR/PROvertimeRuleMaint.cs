using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PR
{

	public class PROvertimeRuleMaint : PXGraph<PROvertimeRuleMaint>
	{
		#region Views

		public SelectFrom<PROvertimeRule>.View OvertimeRules;

		#endregion

		#region Actions

		public PXSave<PROvertimeRule> Save;
		public PXCancel<PROvertimeRule> Cancel;

		#endregion
	}
}
