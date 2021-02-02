using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.Subcontracts.AP.Descriptor;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages.Subcontract;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoOrderFilterExt : PXCacheExtension<POOrderFilter>
    {
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            PO.Descriptor.Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        public string OrderNbr
        {
            get;
            set;
        }

        [PXString]
        [PXUIField(DisplayName = Messages.SubcontractNumber)]
        [SubcontractNumberSelector]
        public string SubcontractNumber
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNumber : BqlString.Field<subcontractNumber>
        {
        }
    }
}
