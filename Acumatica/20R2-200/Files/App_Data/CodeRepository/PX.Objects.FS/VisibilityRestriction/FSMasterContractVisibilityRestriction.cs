using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class MasterContractMaintVisibilityRestriction : PXGraphExtension<MasterContractMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
        }

        public PXSetup<BAccount, Where<BAccount.bAccountID, Equal<Current<FSMasterContract.customerID>>>> currentBAccount;

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictBranchByCustomer(typeof(FSMasterContract.customerID), typeof(BAccount.cOrgBAccountID), ResetBranch = true)]
        public virtual void FSMasterContract_BranchID_CacheAttached(PXCache sender)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictCustomerByUserBranches]
        public virtual void FSMasterContract_CustomerID_CacheAttached(PXCache sender)
        {
        }
    }
}