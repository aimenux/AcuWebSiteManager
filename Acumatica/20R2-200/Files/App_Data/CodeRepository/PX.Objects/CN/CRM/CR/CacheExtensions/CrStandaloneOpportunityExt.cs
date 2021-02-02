using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR.Standalone;
using PX.Objects.CS;

namespace PX.Objects.CN.CRM.CR.CacheExtensions
{
    public sealed class CrStandaloneOpportunityExt : PXCacheExtension<CROpportunity>
    {
        [PXDBBaseCury]
        public decimal? Cost
        {
            get;
            set;
        }

        [PXDBBool]
        public bool? MultipleAccounts
        {
            get;
            set;
        }

        [PXDBBaseCury]
        public decimal? QuotedAmount
        {
            get;
            set;
        }

        [PXDBBaseCury]
        public decimal? TotalAmount
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class cost : IBqlField
        {
        }

        public abstract class multipleAccounts : IBqlField
        {
        }

        public abstract class quotedAmount : IBqlField
        {
        }

        public abstract class totalAmount : IBqlField
        {
        }
    }
}