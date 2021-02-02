using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class BAccountLocationVisibilityRestriction : PXCacheExtension<BAccountLocation>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
        }

        #region CustomerID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictCustomerByUserBranches(typeof(BAccount.cOrgBAccountID))]
        public virtual int? CustomerID { get; set; }
        #endregion
    }
}
