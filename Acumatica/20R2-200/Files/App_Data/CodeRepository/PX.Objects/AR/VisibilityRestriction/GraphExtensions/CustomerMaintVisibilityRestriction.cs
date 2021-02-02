using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.CS;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.AR
{

	public class CustomerMaintVisibilityRestriction : PXGraphExtension<CustomerMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
		}

		public override void Initialize()
		{
			base.Initialize();

			Base.BAccount.WhereAnd<Where<Customer.cOrgBAccountID, RestrictByUserBranches<Current<AccessInfo.userName>>>>();
		}

		[PXHidden]
		public PXSelect<CRLocation> DummyLocations;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXUIField(DisplayName = "Restrict Visibility To", FieldClass = nameof(FeaturesSet.VisibilityRestriction))]
		public void Customer_COrgBAccountID_CacheAttached(PXCache sender) { }

		public void _(Events.RowPersisting<Location> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				//Persist only Location table because the projection Location may have
				//duplicate Address part when IsAddressSameAsMain == true
				CRLocation loc = Common.Utilities.Clone<Location, CRLocation>(Base, e.Row);
				DummyLocations.Cache.MarkUpdated(loc);
				e.Cancel = true;
			}
		}

		public void _(Events.CommandPreparing<CRLocation.noteID> e)
		{
			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
			{
				e.Args.ExcludeFromInsertUpdate();
			}
		}

		public void _(Events.RowUpdating<Customer> e)
		{
			ResetLocationBranch = e.Row.COrgBAccountID != e.NewRow.COrgBAccountID;
			if (e.Row.COrgBAccountID != 0 && e.NewRow.COrgBAccountID == 0 &&
				Base.GetExtension<CustomerMaint.LocationDetailsExt>().Locations.Select<CRLocation>().ToList().Any(l => l.CBranchID != null))
			{
				ResetLocationBranch =
					Base.GetExtension<CustomerMaint.LocationDetailsExt>().Locations.Ask(Messages.Warning, Messages.KeepLocationBranchConfirmation, MessageButtons.YesNo) ==
					WebDialogResult.No;
			}
		}

		private bool? ResetLocationBranch = null;
		public void _(Events.RowUpdated<Customer> e)
		{
			if (ResetLocationBranch != true) return;

			var branchList = PXAccess.GetOrganizationByBAccountID(e.Row.COrgBAccountID)?.ChildBranches ??
							PXAccess.GetBranchByBAccountID(e.Row.COrgBAccountID)?.SingleToList();

			var branches = branchList != null
				? new HashSet<int>(branchList.Select(_ => _.BranchID))
				: new HashSet<int>();

			var defaultBranch = branches.Count == 1
					? (int?)branches.First()
					: null;
			
			foreach (var location in Base.GetExtension<CustomerMaint.LocationDetailsExt>().Locations.Select()
					.ToList()
					.RowCast<CRLocation>()
					.Where(l=> defaultBranch != null || (l.CBranchID != null && !branches.Contains(l.CBranchID.Value))))
			{
				location.CBranchID = defaultBranch;
				if (Base.Caches<CRLocation>().GetStatus(location) == PXEntryStatus.Notchanged)
					Base.Caches<CRLocation>().MarkUpdated(location);
			}
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		//have to join Customer table for restrictor message parameter
		[Branch(searchType: typeof(Search2<Branch.branchID,
					InnerJoin<Organization,
						On<Branch.organizationID, Equal<Organization.organizationID>>,
					InnerJoin<Customer,
						On<Customer.bAccountID, Equal<Current<Customer.bAccountID>>>>>,
					Where<MatchWithBranch<Branch.branchID>>>),
				useDefaulting: false,
				IsDetail = false,
				DisplayName = "Default Branch",
				BqlField = typeof(CRLocation.cBranchID),
				PersistingCheck = PXPersistingCheck.Nothing,
				IsEnabledWhenOneBranchIsAccessible = true)]
		[PXRestrictor(typeof(Where<Branch.branchID, Inside<Current<Customer.cOrgBAccountID>>>),
			Messages.BranchRestrictedByCustomer, new[] { typeof(Customer.acctCD), typeof(Branch.branchCD) })]
		[PXDefault(typeof(Search<Branch.branchID,
					Where<Branch.bAccountID, Equal<Current<Customer.cOrgBAccountID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public void _(Events.CacheAttached<CRLocation.cBranchID> e)
		{
		}
	}
}
