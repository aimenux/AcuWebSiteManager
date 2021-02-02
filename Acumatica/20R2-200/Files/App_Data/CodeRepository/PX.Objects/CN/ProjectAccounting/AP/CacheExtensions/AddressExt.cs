using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.ProjectAccounting.AP.CacheExtensions
{
    public sealed class AddressExt : PXCacheExtension<Address>
    {
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "County")]
        public string County
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class county : IBqlField
        {
        }
    }
}