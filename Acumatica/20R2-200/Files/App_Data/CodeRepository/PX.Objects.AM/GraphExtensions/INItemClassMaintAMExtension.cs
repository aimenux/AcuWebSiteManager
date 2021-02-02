using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.IN;

namespace PX.Objects.AM.GraphExtensions
{
    public class INItemClassMaintAMExtension : PXGraphExtension<INItemClassMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>();
        }

        public override void Initialize()
        {
            base.Initialize();

            PXUIFieldAttribute.SetVisible<INItemClassExt.aMReplenishmentSource>(Base.itemclass.Cache, null, !AM.InventoryHelper.FullReplenishmentsEnabled);
        }
    }
}