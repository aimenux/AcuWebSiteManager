using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.AM.GraphExtensions
{
    public class ProjectTaskEntryAMExtension : PXGraphExtension<ProjectTaskEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public virtual void PMTask_RowSelected(PXCache cache, PXRowSelectedEventArgs e, PXRowSelected del)
        {
            del?.Invoke(cache, e);

            if (e.Row != null)
            {
                var setupExt = PXCache<PMSetup>.GetExtension<PMSetupExt>(Base.Setup.Current);
                if (setupExt != null)
                    PXUIFieldAttribute.SetEnabled<PMTaskExt.visibleInPROD>(cache, e.Row, setupExt.VisibleInPROD == true);
            }
        }
    }
}