using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    [PXTable(typeof(INItemClass.itemClassID), IsOptional = true)]
    public class FSxServiceClass : PXCacheExtension<INItemClass>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region DfltBillingRule
        public abstract class dfltBillingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [dfltBillingRule.ListAttribute]
        [PXDefault(ID.BillingRule.TIME)]
        [PXUIField(DisplayName = "Default Billing Rule")]
        public virtual string DfltBillingRule { get; set; }
        #endregion
        #region RequireRoute
        public abstract class requireRoute : PX.Data.BQL.BqlBool.Field<requireRoute> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Route Service Class", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? RequireRoute { get; set; }
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

        #region Memory Fields

        #region Mem_RouteService
        // This memory field exists to show the RequireRoute value in selectors with a different header (DisplayName)
        public abstract class mem_RouteService : PX.Data.BQL.BqlBool.Field<mem_RouteService> { }

        [PXBool]
        [PXUIField(DisplayName = "Route Service", FieldClass = FSRouteSetup.RouteManagementFieldClass)]
        public virtual bool? Mem_RouteService
        {
            get
            {
                return RequireRoute;
            }
        }
        #endregion

        #endregion
    }
}
