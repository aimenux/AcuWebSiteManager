using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PO
{
	[PXProjection(typeof(
		Select2<POReceiptLine,
			LeftJoin<POReceipt, On<POReceipt.receiptType, Equal<POReceiptLine.receiptType>, And<POReceipt.receiptNbr, Equal<POReceiptLine.receiptNbr>>>,
			LeftJoin<POOrderReceiptLink, On<POOrderReceiptLink.receiptNbr, Equal<POReceiptLine.receiptNbr>, And<POOrderReceiptLink.pOType, Equal<POReceiptLine.pOType>, And<POOrderReceiptLink.pONbr, Equal<POReceiptLine.pONbr>>>>,
			LeftJoin<APTran, On<APTran.released, NotEqual<True>,
				And<APTran.pOAccrualType, Equal<POReceiptLine.pOAccrualType>,
				And<APTran.pOAccrualRefNoteID, Equal<POReceiptLine.pOAccrualRefNoteID>,
				And<APTran.pOAccrualLineNbr, Equal<POReceiptLine.pOAccrualLineNbr>>>>>>>>,
			Where2<
				Where<POReceiptLine.pOType, In3<POOrderType.regularOrder, POOrderType.dropShip>, Or<POReceiptLine.pOType, IsNull>>,
				And<POReceiptLine.unbilledQty, Greater<decimal0>, And<POReceipt.released, Equal<True>, And<APTran.refNbr, IsNull>>>>>),
		Persistent = false)]
	[Serializable]
	public partial class LinkLineReceipt : IBqlTable
	{
		#region Selected
			public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region POAccrualType
			public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType>
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(POReceiptLine.pOAccrualType))]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region POAccrualRefNoteID
			public abstract class pOAccrualRefNoteID : PX.Data.BQL.BqlGuid.Field<pOAccrualRefNoteID>
		{
		}
		[PXDBGuid(BqlField = typeof(POReceiptLine.pOAccrualRefNoteID))]
		public virtual Guid? POAccrualRefNoteID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualLineNbr
			public abstract class pOAccrualLineNbr : PX.Data.BQL.BqlInt.Field<pOAccrualLineNbr>
		{
		}
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualLineNbr))]
		public virtual int? POAccrualLineNbr
		{
			get;
			set;
		}
		#endregion
		#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual String OrderType
		{
			get
			{
				return this._OrderType;
			}
			set
			{
				this._OrderType = value;
			}
		}


		#endregion
		#region OrderNbr
			public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr>
		{
		}
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<orderType>>>>))]
		public virtual String OrderNbr
		{
			get
			{
				return this._OrderNbr;
			}
			set
			{
				this._OrderNbr = value;
			}
		}
		#endregion
		#region OrderLineNbr
			public abstract class orderLineNbr : PX.Data.BQL.BqlInt.Field<orderLineNbr>
		{
		}
		protected Int32? _OrderLineNbr;
		[PXUIField(DisplayName = "PO Line", Visible = false)]
		[PXDBInt(BqlField = typeof(POReceiptLine.pOLineNbr))]
		public virtual Int32? OrderLineNbr
		{
			get
			{
				return this._OrderLineNbr;
			}
			set
			{
				this._OrderLineNbr = value;
			}
		}
		#endregion


		#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[POLineInventoryItem(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
		public virtual Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}
		protected Int32? _VendorID;
		[PXDBInt(BqlField = typeof(POReceipt.vendorID))]
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
		#region VendorLocationID
			public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID>
		{
		}
		protected Int32? _VendorLocationID;
		[PXDBInt(BqlField = typeof(POReceipt.vendorLocationID))]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion


		#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;
		[INUnit(typeof(inventoryID), DisplayName = "UOM", BqlField = typeof(POReceiptLine.uOM))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion

		#region ReceiptType
			public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptType))]
		[PXDefault(POReceiptType.POReceipt)]
		[POReceiptType.List()]
		[PXUIField(DisplayName = "Type")]
		public virtual String ReceiptType
		{
			get
			{
				return this._ReceiptType;
			}
			set
			{
				this._ReceiptType = value;
			}
		}
		#endregion
		#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<receiptType>>>>), ValidateValue = false)]
		[PXUIField(DisplayName = "Receipt Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIVerify(typeof(Where<receiptCuryID, Equal<Current<APInvoice.curyID>>>), PXErrorLevel.RowWarning, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, true)]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region ReceiptLineNbr
			public abstract class receiptLineNbr : PX.Data.BQL.BqlInt.Field<receiptLineNbr>
		{
		}
		protected Int32? _ReceiptLineNbr;
		[PXUIField(DisplayName = "PO Receipt Line", Visible = false)]
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXDefault(1)]
		public virtual Int32? ReceiptLineNbr
		{
			get
			{
				return this._ReceiptLineNbr;
			}
			set
			{
				this._ReceiptLineNbr = value;
			}
		}
		#endregion
		#region ReceiptSortOrder
			public abstract class receiptSortOrder : PX.Data.BQL.BqlInt.Field<receiptSortOrder>
		{
		}
		protected Int32? _ReceiptSortOrder;
		[PXDBInt(BqlField = typeof(POReceiptLine.sortOrder))]
		public virtual Int32? ReceiptSortOrder
		{
			get
			{
				return this._ReceiptSortOrder;
			}
			set
			{
				this._ReceiptSortOrder = value;
			}
		}
		#endregion

		#region ReceiptQty
			public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty>
		{
		}
		protected Decimal? _ReceiptQty;

		[PXDBQuantity(BqlField = typeof(POReceiptLine.receiptQty))]
		[PXUIField(DisplayName = "Receipt Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ReceiptQty
		{
			get
			{
				return this._ReceiptQty;
			}
			set
			{
				this._ReceiptQty = value;
			}
		}

		#endregion

		#region ReceiptCuryID
			public abstract class receiptCuryID : PX.Data.BQL.BqlString.Field<receiptCuryID>
		{
		}
		protected String _ReceiptCuryID;
		[PXUIField(DisplayName = "Order Currency")]
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrderReceiptLink.curyID))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String ReceiptCuryID
		{
			get
			{
				return this._ReceiptCuryID;
			}
			set
			{
				this._ReceiptCuryID = value;
			}
		}
		#endregion

		#region ReceiptReceiptUnbilledQty
			public abstract class receiptUnbilledQty : PX.Data.BQL.BqlDecimal.Field<receiptUnbilledQty>
		{
		}
		protected Decimal? _ReceiptUnbilledQty;
		[PXDBQuantity(typeof(uOM), typeof(receiptBaseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.unbilledQty))]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual Decimal? ReceiptUnbilledQty
		{
			get
			{
				return this._ReceiptUnbilledQty;
			}
			set
			{
				this._ReceiptUnbilledQty = value;
			}
		}

		#endregion
		#region ReceiptBaseUnbilledQty
			public abstract class receiptBaseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<receiptBaseUnbilledQty>
		{
		}
		protected Decimal? _ReceiptBaseUnbilledQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseUnbilledQty))]
		public virtual Decimal? ReceiptBaseUnbilledQty
		{
			get
			{
				return this._ReceiptBaseUnbilledQty;
			}
			set
			{
				this._ReceiptBaseUnbilledQty = value;
			}
		}
		#endregion

		#region ReceiptSiteID
			public abstract class receiptSiteID : PX.Data.BQL.BqlInt.Field<receiptSiteID>
		{
		}
		protected Int32? _ReceiptSiteID;
		[Site(BqlField = typeof(POReceiptLine.siteID))]
		public virtual Int32? ReceiptSiteID
		{
			get
			{
				return this._ReceiptSiteID;
			}
			set
			{
				this._ReceiptSiteID = value;
			}
		}
		#endregion
		#region ReceiptSubItemID
			public abstract class receiptSubItemID : PX.Data.BQL.BqlInt.Field<receiptSubItemID>
		{
		}
		protected Int32? _ReceiptSubItemID;
		[SubItem(typeof(inventoryID), BqlField = typeof(POReceiptLine.subItemID))]
		public virtual Int32? ReceiptSubItemID
		{
			get
			{
				return this._ReceiptSubItemID;
			}
			set
			{
				this._ReceiptSubItemID = value;
			}
		}
		#endregion

		#region POAccrualAcctID
			public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID>
		{
		}
		protected Int32? _POAccrualAcctID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualAcctID))]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
			public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID>
		{
		}
		protected Int32? _POAccrualSubID;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOAccrualSubID))]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion

		#region ReceiptExpenseAcctID
			public abstract class receiptExpenseAcctID : PX.Data.BQL.BqlInt.Field<receiptExpenseAcctID>
		{
		}
		protected Int32? _ReceiptExpenseAcctID;
		[PXDBInt(BqlField = typeof(POReceiptLine.expenseAcctID))]
		public virtual Int32? ReceiptExpenseAcctID
		{
			get
			{
				return this._ReceiptExpenseAcctID;
			}
			set
			{
				this._ReceiptExpenseAcctID = value;
			}
		}
		#endregion
		#region ReceiptExpenseSubID
			public abstract class receiptExpenseSubID : PX.Data.BQL.BqlInt.Field<receiptExpenseSubID>
		{
		}
		protected Int32? _ReceiptExpenseSubID;
		[PXDBInt(BqlField = typeof(POReceiptLine.expenseSubID))]
		public virtual Int32? ReceiptExpenseSubID
		{
			get
			{
				return this._ReceiptExpenseSubID;
			}
			set
			{
				this._ReceiptExpenseSubID = value;
			}
		}
		#endregion

		#region ReciptTranDesc
			public abstract class reciptTranDesc : PX.Data.BQL.BqlString.Field<reciptTranDesc>
		{
		}
		protected String _ReciptTranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POReceiptLine.tranDesc))]
		[PXUIField(DisplayName = "Transaction Descr.")]
		public virtual String ReciptTranDesc
		{
			get
			{
				return this._ReciptTranDesc;
			}
			set
			{
				this._ReciptTranDesc = value;
			}
		}
		#endregion


		#region ReceiptVendorRefNbr
			public abstract class receiptVendorRefNbr : PX.Data.BQL.BqlString.Field<receiptVendorRefNbr>
		{
		}
		protected String _ReceiptVendorRefNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(POReceipt.invoiceNbr))]
		[PXUIField(DisplayName = "Vendor Ref.")]
		public virtual String ReceiptVendorRefNbr
		{
			get
			{
				return this._ReceiptVendorRefNbr;
			}
			set
			{
				this._ReceiptVendorRefNbr = value;
			}
		}
		#endregion

		#region PayToVendorID
			public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		[PXDBInt(BqlField = typeof(POOrderReceiptLink.payToVendorID))]
		public virtual int? PayToVendorID { get; set; }
		#endregion

		#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		[ProjectBase(BqlField = typeof(POReceipt.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
	}
}
