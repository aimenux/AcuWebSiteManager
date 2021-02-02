using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.CacheExtensions
{
    public sealed class PmProjectExtension : PXCacheExtension<PMProject>
    {
        [PXDBString]
        [PXDefault(typeof(LienWaiverSetup.throughDateSourceConditional), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<LienWaiverSetup.shouldGenerateConditional.FromCurrent.IsEqual<True>>))]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public string ThroughDateSourceConditional
        {
            get;
            set;
        }

        [PXDBString]
        [PXDefault(typeof(LienWaiverSetup.throughDateSourceUnconditional), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<LienWaiverSetup.shouldGenerateUnconditional.FromCurrent.IsEqual<True>>))]
        [LienWaiverThroughDateSource.List]
        [PXUIField(DisplayName = ComplianceLabels.LienWaiverSetup.ThroughDate)]
        public string ThroughDateSourceUnconditional
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class throughDateSourceConditional : BqlString.Field<throughDateSourceConditional>
        {
        }

        public abstract class throughDateSourceUnconditional : BqlString.Field<throughDateSourceUnconditional>
        {
        }
    }
}