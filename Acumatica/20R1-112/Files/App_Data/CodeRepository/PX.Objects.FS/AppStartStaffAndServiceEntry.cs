using PX.Data;
using System.Collections;

namespace PX.Objects.FS
{
    public class AppStartStaffAndServiceEntry : AppLogActionsEntry
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
        [PXUnboundDefault(ID.Type_Log.STAFF_ASSIGMENT)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }


        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBString(4, IsFixed = true, BqlField = typeof(FSAppointmentDet.lineRef))]
        [PXUIField(DisplayName = "Detail Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSStaffLogActionDetail_DetLineRef_CacheAttached(PXCache sender)
        {
        }

        #endregion
    }
}