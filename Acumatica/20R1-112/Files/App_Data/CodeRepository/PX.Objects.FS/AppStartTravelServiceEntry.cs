using PX.Data;

namespace PX.Objects.FS
{
    public class AppStartTravelServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached

        #region Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #endregion

        protected virtual void _(Events.RowSelected<FSLogActionMobileFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLogActionMobileFilter filterRow = e.Row;

            FSLogTypeAction.SetLineTypeList<FSLogActionMobileFilter.type>(e.Cache, filterRow, string.Empty);
        }
    }
}
