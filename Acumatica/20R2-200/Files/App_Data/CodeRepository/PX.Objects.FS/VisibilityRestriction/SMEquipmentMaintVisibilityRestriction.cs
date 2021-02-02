using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    public class SMEquipmentMaintVisibilityRestriction : PXGraphExtension<SMEquipmentMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
        }
        public PXSetup<BAccount, Where<BAccount.bAccountID, Equal<Current<FSEquipment.ownerID>>>> currentBAccount;

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictBranchByCustomer(typeof(FSEquipment.ownerID), typeof(BAccount.cOrgBAccountID), ResetBranch = true)]
        public void _(Events.CacheAttached<FSEquipment.branchID> e)
        {
        }
    }
}
