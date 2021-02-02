using PX.Data;

namespace PX.Objects.CN.Common.Extensions
{
    public static class ViewExtensions
    {
        public static void Enable(this PXSelectBase view, bool isEnabled)
        {
            view.AllowUpdate = isEnabled;
            view.AllowInsert = isEnabled;
            view.AllowDelete = isEnabled;
        }
    }
}
