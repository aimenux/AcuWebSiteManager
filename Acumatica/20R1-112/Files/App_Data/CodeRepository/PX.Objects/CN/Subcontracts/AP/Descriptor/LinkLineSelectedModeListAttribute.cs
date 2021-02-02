using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.AP.Descriptor
{
    public class LinkLineSelectedModeListAttribute : PXStringListAttribute
    {
        public LinkLineSelectedModeListAttribute()
            : base(new[]
            {
                LinkLineFilter.selectedMode.Order,
                LinkLineFilter.selectedMode.Receipt,
                LinkLineFilter.selectedMode.LandedCost
            }, new[]
            {
                Messages.LinkLineFilterMode.PurchaseOrderOrSubcontract,
                PX.Objects.AP.Messages.POReceiptMode,
                PX.Objects.AP.Messages.POLandedCostMode
            })
        {
        }
    }
}
