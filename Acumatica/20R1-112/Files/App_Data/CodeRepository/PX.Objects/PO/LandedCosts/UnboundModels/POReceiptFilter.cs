using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts
{
	//PONbr
	//POType
	//InventoryID
	//
	public class POReceiptFilter : IBqlTable
	{
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

		[VendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		//[PXDefault(typeof(POLandedCostDoc.vendorID))]
		public virtual Int32? VendorID
		{
			get;
			set;
		}
		#endregion

		#region ReceiptType
		public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType> { }
		[PXString(2, IsFixed = true, InputMask = "")]
		[PXDefault(POReceiptType.POReceipt)]
		[PXStringList(new[] { POReceiptType.POReceipt, POReceiptType.TransferReceipt }, new[] { "Receipt", "Transfer Receipt" })]
		//[POReceiptType.List()]
		[PXUIField(DisplayName = "Type")]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ReceiptType
		{
			get;
			set;
		}
		#endregion

		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXDefault()]
		[POReceiptType.RefNbr(typeof(Search2<POReceipt.receiptNbr,
			LeftJoinSingleTable<Vendor, On<Vendor.bAccountID, Equal<POReceipt.vendorID>>>,
			Where<POReceipt.receiptType, Equal<Optional<POReceiptFilter.receiptType>>,
				And2<Where<Current<POReceiptFilter.vendorID>, IsNull, Or<POReceipt.vendorID, Equal<Current<POReceiptFilter.vendorID>>>>,
				And<Where<Vendor.bAccountID, IsNull, Or<Match<Vendor, Current<AccessInfo.userName>>>>>>>,
			OrderBy<Desc<POReceipt.receiptNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ReceiptNbr
		{
			get;
			set;
		}
		#endregion

		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		[PXString(2, IsFixed = true)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual String OrderType
		{
			get;
			set;
		}
		#endregion

		#region OrderNbr
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		[PXString(15, IsUnicode = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible, Required = false)]
		//[PO.RefNbr(typeof(Search<POOrder.orderNbr>), Filterable = true)]
		[PO.RefNbr(
				typeof(Search2<POOrder.orderNbr,
				CrossJoin<APSetup,
				LeftJoin<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>>>>,
				Where<Current<POReceiptFilter.orderType>, IsNull, Or<POOrder.orderType, Equal<Current<POReceiptFilter.orderType>>,
				And<Where<Current<POReceiptFilter.vendorID>, IsNull, Or<POOrder.vendorID, Equal<Current<POReceiptFilter.vendorID>>>>>>>>),
				Filterable = true)]
		public virtual String OrderNbr
		{
			get;
			set;
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[Inventory]
		public virtual Int32? InventoryID
		{
			get;
			set;
		}
		#endregion
	}
}
