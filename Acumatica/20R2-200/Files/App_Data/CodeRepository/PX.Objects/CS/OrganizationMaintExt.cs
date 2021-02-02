using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.CS.DAC;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.CS
{

	[Serializable]
	public class OrganizationMaintExt : PXGraphExtension<OrganizationMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.multipleBaseCurrencies>();
		}

		[Serializable]
		[PXHidden]
		public sealed class StateExt : PXCacheExtension<OrganizationMaint.State>
		{
			public abstract class isGroup : PX.Data.BQL.BqlBool.Field<isGroup> { }
			[PXBool]
			public bool? IsGroup { get; set; }
		}

		protected virtual void OrganizationBAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Ledger actualLedger = PXSelectJoin<Ledger,
					InnerJoin<OrganizationLedgerLink,
						On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
					InnerJoin<Organization,
						On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
					Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
						And<Organization.organizationID, Equal<Current<Organization.organizationID>>>>>.Select(Base);

			// TODO: Redesign to persist before actions and eliminate this code (except existing actual ledger check)
			Organization org = Base.OrganizationView.Current;
			bool isPersistedOrganization = !Base.IsPrimaryObjectInserted();
			Base.createLedger.SetEnabled(actualLedger == null && isPersistedOrganization && org.OrganizationType != OrganizationTypes.Group);
			Base.AddBranch.SetEnabled(isPersistedOrganization && org.OrganizationType != OrganizationTypes.Group);
			Base.newContact.SetEnabled(isPersistedOrganization && org.OrganizationType != OrganizationTypes.Group);

			PXUIFieldAttribute.SetVisible<Organization.organizationGroupID>(Base.OrganizationView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			PXUIFieldAttribute.SetVisible<Organization.roleName>(Base.OrganizationView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			PXUIFieldAttribute.SetVisible<Organization.reporting1099>(Base.OrganizationView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			PXUIFieldAttribute.SetVisible<Organization.countryID>(Base.OrganizationView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			PXUIFieldAttribute.SetVisible<OrganizationBAccount.legalName>(Base.BAccount.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			PXUIFieldAttribute.SetVisible<OrganizationBAccount.taxRegistrationID>(Base.BAccount.Cache, null, org != null && org.OrganizationType != OrganizationTypes.Group);
			Base.Caches<CR.Location>().AllowSelect = org != null && org.OrganizationType != OrganizationTypes.Group;
			Base.Commonsetup.Cache.AllowSelect = org != null && org.OrganizationType != OrganizationTypes.Group;
			Base.Company.Cache.AllowSelect = org != null && org.OrganizationType != OrganizationTypes.Group;

			OrganizationMaint.State state = Base.StateView.Current;
			StateExt stateExt = Base.StateView.Current.GetExtension<StateExt>();

			if (org != null)
			{
				state.IsBranchTabVisible = org.OrganizationType != OrganizationTypes.WithoutBranches &&
										   org.OrganizationType != OrganizationTypes.Group &&
										   Base.OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(org) as string != OrganizationTypes.WithoutBranches;

				stateExt.IsGroup = org.OrganizationType == OrganizationTypes.Group;
			}
			else
			{
				stateExt.IsGroup = false;
			}
		}
	}
}
