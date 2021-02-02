using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    public class INSiteMaintAMExtension : PXGraphExtension<INSiteMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        protected virtual void INSite_AMScrapSiteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (INSite)e.Row;
            var rowExt = row.GetExtension<INSiteExt>();

            if (row == null)
            {
                return;
            }

            rowExt.AMScrapLocationID = null;
        }
    }
}
