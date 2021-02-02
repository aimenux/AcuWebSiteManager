using PX.Data;
using PX.Objects.CS;
using PX.Objects.CT;
using Messages = PX.Objects.CN.Externals.Messages;

namespace PX.Objects.CN.CacheExtensions
{
    public sealed class ContractExt : PXCacheExtension<Contract>
    {
        [PXDBBool]
        [PXUIField(DisplayName = Messages.Contract.AllowItemIsNotPresentedInTheProjectBudget)]
        public bool? AllowNonProjectAccountGroups
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class allowNonProjectAccountGroups : IBqlField
        {
        }
    }
}