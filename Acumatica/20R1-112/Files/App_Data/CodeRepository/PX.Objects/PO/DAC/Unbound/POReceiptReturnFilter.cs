using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	[Serializable]
	public partial class POReceiptReturnFilter : IBqlTable
	{
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		[PXDBString(2, IsFixed = true)]
		[POOrderType.RegularDropShipList]
		[PXUIField(DisplayName = "Order Type")]
		public virtual string OrderType
		{
			get;
			set;
		}
		#endregion
		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PO.RefNbr(
			typeof(Search2<POOrder.orderNbr,
			CrossJoin<APSetup>,
			Where2<
				Where<Current<POReceiptReturnFilter.orderType>, IsNull,
					Or<POOrder.orderType, Equal<Current<POReceiptReturnFilter.orderType>>>>,
				And<POOrder.curyID, Equal<Current<POReceipt.curyID>>,
				And<POOrder.hold, Equal<boolFalse>,
				And2<Where<Current<POReceipt.vendorID>, IsNull,
					Or<POOrder.vendorID, Equal<Current<POReceipt.vendorID>>>>,
				And2<Where<Current<POReceipt.vendorLocationID>, IsNull,
					Or<POOrder.vendorLocationID, Equal<Current<POReceipt.vendorLocationID>>>>,
				And<Where<APSetup.requireSingleProjectPerDocument, Equal<boolFalse>,
					Or<POOrder.projectID, Equal<Current<POReceipt.projectID>>>>>>>>>>,
			OrderBy<Asc<POOrder.orderType, Desc<POOrder.orderNbr>>>>),
			Filterable = true)]
		public virtual string OrderNbr
		{
			get;
			set;
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[POReceiptType.RefNbr(typeof(Search<POReceipt.receiptNbr,
			Where<POReceipt.receiptType, Equal<POReceiptType.poreceipt>, And<POReceipt.released, Equal<True>>>,
			OrderBy<Desc<POReceipt.receiptNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = "Receipt Nbr.")]
		public virtual string ReceiptNbr
		{
			get;
			set;
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[Inventory]
		public virtual int? InventoryID
		{
			get;
			set;
		}
		#endregion
	}
}
