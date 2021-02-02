using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class FSxARPayment : PXCacheExtension<ARPayment>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region ChkServiceManagement
        public abstract class ChkServiceManagement : PX.Data.BQL.BqlBool.Field<ChkServiceManagement> { }

        [PXBool]
        [PXUIField(Visible = false)]
        public virtual bool? chkServiceManagement
        {
            get
            {
                return true;
            }
        }
        #endregion
    }
}