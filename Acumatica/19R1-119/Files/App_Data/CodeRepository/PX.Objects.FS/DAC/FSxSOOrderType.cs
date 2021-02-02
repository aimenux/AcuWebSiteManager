using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    [PXTable(IsOptional = true)]
    public sealed class FSxSOOrderType : PXCacheExtension<SOOrderType>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        #region EnableFSIntegration
        public abstract class enableFSIntegration : PX.Data.BQL.BqlBool.Field<enableFSIntegration> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Field Services Integration")]
        public bool? EnableFSIntegration { get; set; }
        #endregion
    }
}
