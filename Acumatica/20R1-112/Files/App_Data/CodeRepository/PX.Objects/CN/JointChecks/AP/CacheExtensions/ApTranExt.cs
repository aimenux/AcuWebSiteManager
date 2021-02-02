using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.CacheExtensions
{
    public sealed class ApTranExt : PXCacheExtension<APTran>
    {
        [PXDecimal]
        public decimal? TotalJointAmountPerLine
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class totalJointAmountPerLine : BqlDecimal.Field<totalJointAmountPerLine>
        {
        }
    }
}