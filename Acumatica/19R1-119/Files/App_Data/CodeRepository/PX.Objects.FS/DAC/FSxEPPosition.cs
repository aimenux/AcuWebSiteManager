using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public class FSxEPPosition : PXCacheExtension<EPPosition>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region SDEnabled
        public abstract class sDEnabled : PX.Data.BQL.BqlBool.Field<sDEnabled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Staff for " + TX.ModuleName.SERVICE_DISPATCH)]
        public virtual bool? SDEnabled { get; set; }
        #endregion

        #region SDEnabledModified
        [PXBool]
        [PXDefault(false)]
        public virtual bool SDEnabledModified { get; set; }
        #endregion
    }
}