using PX.Data;
using PX.Objects.CN.Subcontracts.PO.DAC;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.Descriptor.Attributes
{
    public class PurchaseOrderTypeRestrictorAttribute : PXRestrictorAttribute
    {
        public PurchaseOrderTypeRestrictorAttribute()
            : base(typeof(
                    Where<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type1>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type2>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type3>>,
                        Or<POOrder.orderType, Equal<Current<PurchaseOrderTypeFilter.type4>>>>>>),
                Messages.OnlyPurchaseOrdersAreAllowedMessage)
        {
        }
    }
}