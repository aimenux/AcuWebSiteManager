using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    [PXTable(typeof(Vendor.bAccountID), IsOptional = true)]
    public class FSxVendor : PXCacheExtension<Vendor>
	{
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region SDEnabled
        public abstract class sDEnabled : PX.Data.BQL.BqlBool.Field<sDEnabled> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Staff Member in " + TX.ModuleName.SERVICE_DISPATCH)]
        public virtual bool? SDEnabled { get; set; }
        #endregion
	}
}
