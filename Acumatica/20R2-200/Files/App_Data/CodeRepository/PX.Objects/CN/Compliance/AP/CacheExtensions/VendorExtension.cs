using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class VendorExtension : PXCacheExtension<Vendor>
    {
        [PXDBBool]
        [PXUIField(DisplayName = "Generate Lien Waivers based on Project settings")]
        [PXUIVisible(typeof(Where<LienWaiverSetup.shouldGenerateConditional.FromCurrent.IsEqual<True>
            .Or<LienWaiverSetup.shouldGenerateUnconditional.FromCurrent.IsEqual<True>>>))]
        [PXDefault(typeof(SearchFor<VendorClassExtension.shouldGenerateLienWaivers>
                .Where<VendorClass.vendorClassID.IsEqual<Vendor.vendorClassID.FromCurrent>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? ShouldGenerateLienWaivers
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class shouldGenerateLienWaivers : BqlBool.Field<shouldGenerateLienWaivers>
        {
        }
    }
}