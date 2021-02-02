using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.DAC;
using PX.Objects.RUTROT.DAC;

namespace PX.Objects.RUTROT
{
	public class OrganizationMaintRUTROT : ConfigurationMaintRUTROTBase<OrganizationMaint>
	{
		protected override RUTROTConfigurationHolderMapping GetDocumentMapping()
		{
			return new RUTROTConfigurationHolderMapping(typeof(Organization));
		}

		[PXOverride]
		public virtual bool ShouldInvokeOrganizationBranchSync(PXEntryStatus status, Func<PXEntryStatus, bool> baseMethod)
		{
			return status == PXEntryStatus.Updated | baseMethod(status);
		}

		[PXOverride]
		public virtual void OnOrganizationBranchSync(BranchMaint branchMaint,
			Organization organization,
			BranchMaint.BranchBAccount branchBaccountCopy,
			Action<BranchMaint, Organization, BranchMaint.BranchBAccount> baseMethod)
		{
			baseMethod(branchMaint, organization, branchBaccountCopy);

			if (!PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>())
				return;

			OrganizationRUTROT organizationRutRot = Base.OrganizationView.Cache.GetExtension<OrganizationRUTROT>(organization);

			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.allowsRUTROT>(branchBaccountCopy, organizationRutRot.AllowsRUTROT);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTROTCuryID>(branchBaccountCopy, organizationRutRot.RUTROTCuryID);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.balanceOnProcess>(branchBaccountCopy, organizationRutRot.BalanceOnProcess);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.defaultRUTROTType>(branchBaccountCopy, organizationRutRot.DefaultRUTROTType);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rOTDeductionPct>(branchBaccountCopy, organizationRutRot.ROTDeductionPct);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rOTExtraAllowanceLimit>(branchBaccountCopy, organizationRutRot.ROTExtraAllowanceLimit);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rOTPersonalAllowanceLimit>(branchBaccountCopy, organizationRutRot.ROTPersonalAllowanceLimit);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTDeductionPct>(branchBaccountCopy, organizationRutRot.RUTDeductionPct);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTExtraAllowanceLimit>(branchBaccountCopy, organizationRutRot.RUTExtraAllowanceLimit);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTPersonalAllowanceLimit>(branchBaccountCopy, organizationRutRot.RUTPersonalAllowanceLimit);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTROTClaimNextRefNbr>(branchBaccountCopy, organizationRutRot.RUTROTClaimNextRefNbr);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.rUTROTOrgNbrValidRegEx>(branchBaccountCopy, organizationRutRot.RUTROTOrgNbrValidRegEx);
			branchMaint.BAccount.Cache.SetValue<BranchBAccountRUTROT.taxAgencyAccountID>(branchBaccountCopy, organizationRutRot.TaxAgencyAccountID);
		}
	}
}
