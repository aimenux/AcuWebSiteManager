using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.CR
{
	public sealed class LocationVisibilityRestriction : PXCacheExtension<Location>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		#region BAccountID
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[RestrictCustomerByUserBranches]
		public int? BAccountID { get; set; }
		#endregion

		#region CBranchID
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		//have to join BAccountR table for restrictor message parameter
		[Branch(searchType: typeof(Search2<Branch.branchID,
					InnerJoin<Organization,
						On<Branch.organizationID, Equal<Organization.organizationID>>,
					InnerJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<Current<BAccountR.bAccountID>>>>>,
					Where<MatchWithBranch<Branch.branchID>>>),
				useDefaulting: false,
				IsDetail = false,
				PersistingCheck = PXPersistingCheck.Nothing,
				DisplayName = "Shipping Branch",
				IsEnabledWhenOneBranchIsAccessible = true)]
		[PXRestrictor(typeof(Where<Branch.branchID, Inside<Current<BAccountR.cOrgBAccountID>>>),
			AR.Messages.BranchRestrictedByCustomer, new[] { typeof(BAccountR.acctCD), typeof(Branch.branchCD) })]
		[PXDefault(typeof(Search2<Branch.branchID,
				InnerJoin<BAccountR, On<BAccountR.bAccountID, Equal<Current<Location.bAccountID>>,
					And<Branch.bAccountID, Equal<BAccountR.cOrgBAccountID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public int? CBranchID { get; set; }
		#endregion
	}
}
