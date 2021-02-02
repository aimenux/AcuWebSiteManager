using PX.Data;

namespace PX.Objects.FS
{
    public class AppCompleteServiceEntry : AppLogActionsEntry
    {
        public AppCompleteServiceEntry()
        {
        }

        #region CacheAttached
        #region FSLogActionMobileFilter_Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.COMPLETE)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region FSLogActionMobileFilter_Type
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(ID.Type_Log.SERVICE)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}
