using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;
using static PX.Objects.FS.AppointmentInq;

namespace PX.Objects.FS
{
    public class AppointmentInqFilterVisibilityRestriction : PXCacheExtension<AppointmentInqFilter>
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
    }
}
