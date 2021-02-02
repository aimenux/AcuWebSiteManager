using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.CR
{
	public class OpportunityMaintVisibilityRestriction : PXGraphExtension<OpportunityMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		[Branch(typeof(AccessInfo.branchID), IsDetail = false, TabOrder = 0,
			BqlField = typeof(Standalone.CROpportunityRevision.branchID))]
		[PXFormula(typeof(Switch<
							Case<Where<CROpportunity.locationID, IsNotNull,
									And<Selector<CROpportunity.locationID, Location.cBranchID>, IsNotNull>>,
								Selector<CROpportunity.locationID, Location.cBranchID>,
							Case<Where<CROpportunity.bAccountID, IsNotNull,
									And<Not<Selector<CROpportunity.bAccountID, BAccountCRM.cOrgBAccountID>, RestrictByBranch<Current2<CROpportunity.branchID>>>>>,
								Null,
							Case<Where<Current2<CROpportunity.branchID>, IsNotNull>,
								Current2<CROpportunity.branchID>>>>,
							Current<AccessInfo.branchID>>))]
		public virtual void _(Events.CacheAttached<CROpportunity.branchID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountCRM.cOrgBAccountID), branchID: typeof(CROpportunity.branchID))]
		public virtual void _(Events.CacheAttached<CROpportunity.bAccountID> e) { }

		public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

		[PXOverride]
		public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers,
			CopyPasteGetScriptDelegate baseMethod)
		{
			// We need to process fields together that are related to the Branch and Customer for proper validation. For this:
			// 1) set the right order of the fields
			// 2) insert dependent fields after the BranchID field
			// 3) all fields must belong to the same view.
			
			string branchViewName = nameof(OpportunityMaint.OpportunityCurrent);
			string customerViewName = nameof(OpportunityMaint.Opportunity);

			(string name, string viewName) branch = (nameof(CROpportunity.BranchID), branchViewName);

			List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
			fieldList.Add((nameof(CROpportunity.BAccountID), customerViewName));
			fieldList.Add((nameof(CROpportunity.LocationID), customerViewName));

			Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
		}
	}
}
