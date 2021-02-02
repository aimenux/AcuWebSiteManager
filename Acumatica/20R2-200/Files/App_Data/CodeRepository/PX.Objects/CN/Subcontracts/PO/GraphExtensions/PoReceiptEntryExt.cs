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
        protected virtual void _(Events.CacheAttached<POReceiptLine.pONbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<POOrder.orderNbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<SO.SOLineSplit.pONbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<POLine.pONbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<POOrderFilter.orderNbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<POReceiptLineS.pONbr> e)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRestrictor(typeof(Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>),
            Messages.OnlyPurchaseOrdersAreAllowedMessage)]
        protected virtual void _(Events.CacheAttached<POOrderEntry.POOrderS.orderNbr> e)
        {
        }
    }
}
