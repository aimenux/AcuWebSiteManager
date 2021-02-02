using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.PO.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoReceiptEntryExt : PXGraphExtension<POReceiptEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POReceiptLine_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POOrder_OrderNbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void SOLineSplit_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POLine_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POOrderFilter_OrderNbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POReceiptLineS_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POLineS_PONbr_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void POOrderS_OrderNbr_CacheAttached(PXCache cache)
        {
        }
    }
}