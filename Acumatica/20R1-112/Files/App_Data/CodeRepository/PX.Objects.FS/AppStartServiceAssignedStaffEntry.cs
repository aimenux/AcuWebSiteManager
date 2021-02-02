using PX.Data;
using System.Collections;

namespace PX.Objects.FS
{
    public class AppStartServiceAssignedStaffEntry : AppLogActionsEntry
    {
        #region CacheAttached

        #region Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion


        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(ID.Type_Log.SERV_BASED_ASSIGMENT)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }

        #endregion
    }
}