using PX.Data;
using PX.Objects.PM;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM.GraphExtensions
{
    public class TemplateMaintAMExtension : PXGraphExtension<TemplateMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public virtual void PMProject_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if(e.Row != null)
            {
                var setupExt = PXCache<PMSetup>.GetExtension<PMSetupExt>(Base.Setup.Current);
                if(setupExt != null)
                    PXUIFieldAttribute.SetEnabled<PMProjectExt.visibleInPROD>(cache, e.Row, setupExt.VisibleInPROD == true);
            }
        }
    }
}