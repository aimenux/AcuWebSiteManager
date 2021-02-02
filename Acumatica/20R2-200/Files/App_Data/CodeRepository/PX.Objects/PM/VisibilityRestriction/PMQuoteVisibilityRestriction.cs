using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PM
{
	public sealed class PMQuoteVisibilityRestriction : PXCacheExtension<PMQuote>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BranchID
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[Branch(IsDetail = false, TabOrder = 0, BqlField = typeof(CROpportunityRevision.branchID))]
		[PXFormula(typeof(Switch<
				Case<Where<CROpportunityRevision.locationID, IsNotNull,
					   And<Selector<CROpportunityRevision.locationID, CR.Location.cBranchID>, IsNotNull>>,
					Selector<CROpportunityRevision.locationID, CR.Location.cBranchID>,
				Case<Where<CROpportunityRevision.bAccountID, IsNotNull,
					   And<Not<Selector<CROpportunityRevision.bAccountID, BAccountR.cOrgBAccountID>, RestrictByBranch<Current2<CROpportunityRevision.branchID>>>>>,
					Null,
				Case<Where<Current2<CROpportunityRevision.branchID>, IsNotNull>,
					Current2<CROpportunityRevision.branchID>>>>,
				Current<AccessInfo.branchID>>))]
		public int? BranchID { get; set; }

		#endregion
		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByBranch(typeof(BAccountR.cOrgBAccountID), typeof(PMQuote.branchID))]
		public int? BAccountID { get; set; }
		#endregion
	}
}
