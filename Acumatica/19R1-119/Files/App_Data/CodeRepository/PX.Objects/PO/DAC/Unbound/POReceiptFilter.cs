using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using System;

namespace PX.Objects.PO.DAC.Unbound
{
	[Serializable]
	public partial class POReceiptFilter : IBqlTable
	{
		#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}

		protected Int32? _VendorID;
		[VendorActive(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
		[PXDefault(typeof(APInvoice.vendorID))]
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion

		#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(
			typeof(Search5<POReceiptLineS.pONbr,
			InnerJoin<POReceipt, On<POReceipt.receiptNbr, Equal<POReceiptLineS.receiptNbr>>,
			InnerJoin<POOrder, On<POOrder.orderNbr, Equal<POReceiptLineS.pONbr>>,
			LeftJoin<POOrderReceiptLink, On<POOrderReceiptLink.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
				And<POOrderReceiptLink.pOType, Equal<POReceiptLineS.pOType>,
					And<POOrderReceiptLink.pONbr, Equal<POReceiptLineS.pONbr>>>>,
			LeftJoin<APTran, On<APTran.receiptNbr, Equal<POReceiptLineS.receiptNbr>,
				And<APTran.receiptLineNbr, Equal<POReceiptLineS.lineNbr>,
				And<APTran.released, Equal<False>>>>>>>>,
				Where2<
			Where<POReceipt.vendorID, Equal<Current<APInvoice.vendorID>>,
							And<POReceipt.vendorLocationID, Equal<Current<APInvoice.vendorLocationID>>,
							And2<Not<FeatureInstalled<FeaturesSet.vendorRelations>>,
						Or2<FeatureInstalled<FeaturesSet.vendorRelations>,
							And<POReceipt.vendorID, Equal<Current<APInvoice.suppliedByVendorID>>,
							And<POReceipt.vendorLocationID, Equal<Current<APInvoice.suppliedByVendorLocationID>>,
							And<POOrderReceiptLink.payToVendorID, Equal<Current<APInvoice.vendorID>>>>>>>>>,
				 And<POReceipt.hold, Equal<False>,
				 And<POReceipt.released, Equal<True>,
				 And<APTran.refNbr, IsNull,
				 And<POReceiptLineS.unbilledQty, Greater<decimal0>,
					And<Where<POReceiptLineS.receiptType, Equal<POReceiptType.poreceipt>,
							And<Optional<APInvoice.docType>, Equal<APInvoiceType.invoice>,
						Or<POReceiptLineS.receiptType, Equal<POReceiptType.poreturn>,
							And<Optional<APInvoice.docType>, Equal<APInvoiceType.debitAdj>>>>>>>>>>>,
			Aggregate<GroupBy<POReceiptLineS.pONbr>>>),
		   typeof(POReceiptLineS.pONbr),
		   typeof(POOrder.orderDate),
		   typeof(POOrder.vendorID),
		   typeof(POOrder.vendorID_Vendor_acctName),
		   typeof(POOrder.vendorLocationID),
		   typeof(POOrder.curyID),
		   typeof(POOrder.curyOrderTotal), Filterable = true)]
		public virtual string OrderNbr { get; set; }
		#endregion
	}
}
