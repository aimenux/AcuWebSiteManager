using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CS;

namespace PX.Objects.CN.Compliance.AP.CacheExtensions
{
    public sealed class VendorClassExtension : PXCacheExtension<VendorClass>
    {
        [PXDBBool]
        [PXDefault(typeof(LienWaiverSetup.shouldGenerateConditional.FromCurrent.IsEqual<True>
                .Or<LienWaiverSetup.shouldGenerateUnconditional.FromCurrent.IsEqual<True>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Generate Lien Waivers Automatically")]
        [PXUIVisible(typeof(Where<LienWaiverSetup.shouldGenerateConditional.FromCurrent.IsEqual<True>
            .Or<LienWaiverSetup.shouldGenerateUnconditional.FromCurrent.IsEqual<True>>>))]
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