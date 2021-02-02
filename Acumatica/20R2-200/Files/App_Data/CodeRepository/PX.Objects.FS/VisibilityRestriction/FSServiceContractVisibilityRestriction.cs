using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class FSServiceContractVisibilityRestriction : PXCacheExtension<FSServiceContract>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
        }

        #region CustomerID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictCustomerByUserBranches]
        public virtual int? CustomerID { get; set; }
        #endregion

        #region BillCustomerID
        [PXMergeAttributes(Method = MergeMethod.Append)]
        [RestrictCustomerByBranch(typeof(FSServiceContract.branchID), ResetCustomer = true)]
        public virtual int? BillCustomerID { get; set; }
        #endregion
    }
}
