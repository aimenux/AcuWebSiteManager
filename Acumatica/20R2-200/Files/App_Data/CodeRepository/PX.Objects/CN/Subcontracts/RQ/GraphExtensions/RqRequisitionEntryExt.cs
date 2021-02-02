using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.RQ;
using Messages = PX.Objects.CN.Subcontracts.RQ.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.RQ.GraphExtensions
{
    public class RqRequisitionEntryExt : PXGraphExtension<RQRequisitionEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
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
        protected virtual void _(Events.CacheAttached<POLine.pONbr> e)
        {
        }
    }
}
