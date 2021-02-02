using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Subcontracts.SC.DAC;
using PX.Objects.IN;

namespace PX.Objects.CN.Subcontracts.SC.Descriptor.Attributes
{
    [PXDBInt]
    [PXUIField(DisplayName = BusinessMessages.InventoryID, Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noPurchases>>),
        Messages.SubcontractLineInventoryItemAttribute.LineItemNotPurchased)]
    [PXRestrictor(typeof(Where<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>>),
        Messages.SubcontractLineInventoryItemAttribute.LineItemReserved)]
    public class SubcontractLineInventoryItemAttribute : CrossItemAttribute
    {
        public SubcontractLineInventoryItemAttribute()
            : base(typeof(Search<SubcontractInventoryItem.inventoryID,
                    Where<Match<Current<AccessInfo.userName>>>>),
                typeof(SubcontractInventoryItem.inventoryCD),
                typeof(SubcontractInventoryItem.descr), INPrimaryAlternateType.VPN)
        {
        }
    }
}