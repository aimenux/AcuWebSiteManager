using System.Collections;
using System.Reflection;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CN.Subcontracts.SM.Extension;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.CN.Subcontracts.SM.GraphExtensions
{
    public class SmNotificationMaintExt : PXGraphExtension<SMNotificationMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected IEnumerable entityItems(string parent)
        {
			IEnumerable cacheEntityItems = Base.entityItems(parent); 

            var siteMap = PXSiteMap.Provider.FindSiteMapNodeByScreenID(Base.Notifications.Current.ScreenID);

            foreach (CacheEntityItem cacheEntityItem in cacheEntityItems)
            {
                if (siteMap.GraphType == typeof(SubcontractEntry).FullName)
                {
                    cacheEntityItem.Name = cacheEntityItem.GetSubcontractViewName();
                }
                yield return cacheEntityItem;
            }
        }
    }
}