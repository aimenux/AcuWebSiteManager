using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.FS
{
    [PXTable(typeof(EPEmployee.bAccountID), IsOptional = true)]
    public class FSxEPEmployee : PXCacheExtension<EPEmployee>
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
        #region IsDriver
        public abstract class isDriver : PX.Data.BQL.BqlBool.Field<isDriver> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Driver", Enabled = false, FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? IsDriver { get; set; }
        #endregion
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

        #region MemoryHelper
        #region Mem_UnassignedDriver
        public abstract class mem_UnassignedDriver : PX.Data.BQL.BqlBool.Field<mem_UnassignedDriver> { }

        [PXBool]
        [PXUIField(DisplayName = "Already Assigned", Enabled = false, FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? Mem_UnassignedDriver { get; set; }
        #endregion
        #endregion
    }
}
