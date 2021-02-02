using PX.Data;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.FS
{
    public class SM_SMAccessPersonalMaint : PXGraphExtension<MyProfileMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }
    }
}
