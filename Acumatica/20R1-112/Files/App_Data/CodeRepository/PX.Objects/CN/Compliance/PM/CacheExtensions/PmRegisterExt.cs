using PX.Data;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.Compliance.PM.CacheExtensions
{
    public sealed class PmRegisterExt : PXCacheExtension<PMRegister>
    {
        [PXString]
        public string ClDisplayName
        {
            get { return string.Format("{0}, {1}", Base.Module, Base.RefNbr); }
            set { }
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class clDisplayName : IBqlField
        {
        }
    }
}