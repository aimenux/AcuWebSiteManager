using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AP
{
	/// <summary>
	/// Provides a selector for the <see cref="InventoryItem"> items, which may be put into the <see cref="APInvoice"> line <br/>
	/// The list is filtered by the user access rights and Inventory Item status - <see cref="InventoryItemStatus.Inactive"> <br/>
	/// and marked to delete items are not shown. If the Purchase order <see cref="APTran.PONbr"> or PO Receipt <br/>
	/// <see cref="APTran.receiptNbr"> is specified - all the items are shown, restriction is made in other place. <br/>
	/// May be used only on DAC derived from <see cref="APTran">. <br/>
	/// <example>
	/// [APTranInventoryItem(Filterable = true)]
	/// </example>
	/// </summary>
	[PXDBInt]
	[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible)]
	[PXRestrictor(typeof(Where<
		Current<APSetup.migrationMode>, Equal<True>,
		Or<InventoryItem.stkItem, Equal<False>,
		Or<Current<APTran.pONbr>, IsNotNull,
		Or<Current<APTran.receiptNbr>, IsNotNull,
		Or<Current<APTran.tranType>, Equal<APInvoiceType.invoice>,
		Or<Current<APInvoice.isRetainageDocument>, Equal<True>>>>>>>), Messages.CannotStockItemInAPBillDirectly)]
	[PXRestrictor(typeof(Where<
		Current<APSetup.migrationMode>, Equal<True>,
		Or<Current<APTran.pONbr>, IsNotNull,
		Or<InventoryItem.stkItem, NotEqual<False>,
		Or<InventoryItem.kitItem, NotEqual<True>>>>>), Messages.CannotAddNonStockKitInAPBillDirectly)]
	public class APTranInventoryItemAttribute : InventoryAttribute
	{
		public APTranInventoryItemAttribute()
			: base(typeof(Search<InventoryItem.inventoryID, Where<Match<Current<AccessInfo.userName>>>>),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
		{
		}
	}
}