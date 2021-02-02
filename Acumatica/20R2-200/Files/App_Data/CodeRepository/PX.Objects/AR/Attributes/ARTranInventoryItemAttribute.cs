using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AR
{
	/// <summary>
	/// Provides a selector for the <see cref="InventoryItem"> items, which may be put into the <see cref="ARInvoice"> line <br/>
	/// The list is filtered by the user access rights and Inventory Item status - <see cref="InventoryItemStatus.Inactive"> <br/>
	/// and marked to delete items are not shown. If the Purchase order <see cref="ARTran.sOOrderNbr"> is specified - <br/>
	/// all the items are shown, restriction is made in other place. <br/>
	/// May be used only on DAC derived from <see cref="ARTran">. <br/>
	/// <example>
	/// [ARTranInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, NotEqual<True>>>), AP.Messages.CannotFindInventoryItem)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<Current<ARTran.sOOrderNbr>, IsNotNull,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>>), SO.Messages.CannotAddNonStockKitDirectly)]
	[PXRestrictor(typeof(Where<
		Current<ARSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>), IN.Messages.CannotAddNonStockKit)]
	public class ARTranInventoryItemAttribute : InventoryAttribute
	{
		public ARTranInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
		{
		}
	}
}