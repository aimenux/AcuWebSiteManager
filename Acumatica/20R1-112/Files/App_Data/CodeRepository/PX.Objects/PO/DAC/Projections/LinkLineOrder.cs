using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PO
{
	[PXProjection(typeof(
				Select2<POLine
					, LeftJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, And<POOrder.orderNbr, Equal<POLine.orderNbr>>>>
					, Where<POLine.closed, NotEqual<True>
						, And<POOrder.status, In3<POOrderStatus.open, POOrderStatus.completed>
							, And<POLine.pOAccrualType, Equal<POAccrualType.order>>
							>
						>
					>
				), Persistent = false)]
	[Serializable]
	public partial class LinkLineOrder : IBqlTable
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

		#region OrderType
			public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
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
		[PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(POLine.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
		[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<orderType>>>>))]
		[PXUIVerify(typeof(Where<orderCuryID, Equal<Current<APInvoice.curyID>>>), PXErrorLevel.RowWarning, AP.Messages.APDocumentCurrencyDiffersFromSourceDocument, true)]
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
		[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
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

		#region POAccrualType
			public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType>
		{
		}
		[PXDBString(1, IsFixed = true, BqlField = typeof(POLine.pOAccrualType))]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region OrderNoteID
			public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID>
		{
		}
		[PXDBGuid(BqlField = typeof(POLine.orderNoteID))]
		public virtual Guid? OrderNoteID
		{
			get;
			set;
		}
		#endregion

		#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[POLineInventoryItem(Filterable = true, BqlField = typeof(POLine.inventoryID))]
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
		[PXDBInt(BqlField = typeof(POOrder.vendorID))]
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
		[PXDBInt(BqlField = typeof(POOrder.vendorLocationID))]
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


		[INUnit(typeof(POLine.inventoryID), DisplayName = "UOM", BqlField = typeof(POLine.uOM))]
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

		#region OrderBaseQty
			public abstract class orderBaseQty : PX.Data.BQL.BqlDecimal.Field<orderBaseQty>
		{
		}
		protected Decimal? _OrderBaseQty;
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOrderQty))]
		public virtual Decimal? OrderBaseQty
		{
			get
			{
				return this._OrderBaseQty;
			}
			set
			{
				this._OrderBaseQty = value;
			}
		}
		#endregion
		#region OrderQty
			public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty>
		{
		}
		protected Decimal? _OrderQty;
		[PXDBQuantity(BqlField = typeof(POLine.orderQty))]
		[PXUIField(DisplayName = "Order Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? OrderQty
		{
			get
			{
				return this._OrderQty;
			}
			set
			{
				this._OrderQty = value;
			}
		}
		#endregion
		#region OrderCuryInfoID
			public abstract class orderCuryInfoID : PX.Data.BQL.BqlLong.Field<orderCuryInfoID>
		{
		}
		protected Int64? _OrderCuryInfoID;

		[PXDBLong(BqlField = typeof(POLine.curyInfoID))]
		[CurrencyInfo(CuryIDField = "OrderCuryID")]
		public virtual Int64? OrderCuryInfoID
		{
			get
			{
				return this._OrderCuryInfoID;
			}
			set
			{
				this._OrderCuryInfoID = value;
			}
		}
		#endregion
		#region OrderAmount
			public abstract class orderAmount : PX.Data.BQL.BqlDecimal.Field<orderAmount>
		{
		}
		protected Decimal? _OrderAmount;
		[PXDBDecimal(BqlField = typeof(POLine.lineAmt))]
		public virtual Decimal? OrderAmount
		{
			get
			{
				return this._OrderAmount;
			}
			set
			{
				this._OrderAmount = value;
			}
		}
		#endregion
		#region OrderCuryAmount
			public abstract class orderCuryAmount : PX.Data.BQL.BqlDecimal.Field<orderCuryAmount>
		{
		}
		protected Decimal? _OrderCuryAmount;
		[PXDBCurrency(typeof(orderCuryInfoID), typeof(orderAmount), BqlField = typeof(POLine.curyLineAmt))]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? OrderCuryAmount
		{
			get
			{
				return this._OrderCuryAmount;
			}
			set
			{
				this._OrderCuryAmount = value;
			}
		}
		#endregion
		#region OrderCuryID
			public abstract class orderCuryID : PX.Data.BQL.BqlString.Field<orderCuryID>
		{
		}
		protected String _OrderCuryID;
		[PXUIField(DisplayName = "Currency")]
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String OrderCuryID
		{
			get
			{
				return this._OrderCuryID;
			}
			set
			{
				this._OrderCuryID = value;
			}
		}
		#endregion

		#region OrderSiteID
			public abstract class orderSiteID : PX.Data.BQL.BqlInt.Field<orderSiteID>
		{
		}
		protected Int32? _OrderSiteID;
		[Site(BqlField = typeof(POLine.siteID))]
		public virtual Int32? OrderSiteID
		{
			get
			{
				return this._OrderSiteID;
			}
			set
			{
				this._OrderSiteID = value;
			}
		}
		#endregion
		#region OrderSubItemID
			public abstract class orderSubItemID : PX.Data.BQL.BqlInt.Field<orderSubItemID>
		{
		}
		protected Int32? _OrderSubItemID;
		[SubItem(typeof(inventoryID), BqlField = typeof(POLine.subItemID))]
		public virtual Int32? OrderSubItemID
		{
			get
			{
				return this._OrderSubItemID;
			}
			set
			{
				this._OrderSubItemID = value;
			}
		}
		#endregion
		#region OrderExpenseAcctID
			public abstract class orderExpenseAcctID : PX.Data.BQL.BqlInt.Field<orderExpenseAcctID>
		{
		}
		protected Int32? _OrderExpenseAcctID;
		[PXDBInt(BqlField = typeof(POLine.expenseAcctID))]
		public virtual Int32? OrderExpenseAcctID
		{
			get
			{
				return this._OrderExpenseAcctID;
			}
			set
			{
				this._OrderExpenseAcctID = value;
			}
		}
		#endregion
		#region OrderExpenseSubID
			public abstract class orderExpenseSubID : PX.Data.BQL.BqlInt.Field<orderExpenseSubID>
		{
		}
		protected Int32? _OrderExpenseSubID;
		[PXDBInt(BqlField = typeof(POLine.expenseSubID))]
		public virtual Int32? OrderExpenseSubID
		{
			get
			{
				return this._OrderExpenseSubID;
			}
			set
			{
				this._OrderExpenseSubID = value;
			}
		}
		#endregion
		#region OrderTranDesc
			public abstract class orderTranDesc : PX.Data.BQL.BqlString.Field<orderTranDesc>
		{
		}
		protected String _OrderTranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POLine.tranDesc))]
		[PXUIField(DisplayName = "Transaction Descr.")]
		public virtual String OrderTranDesc
		{
			get
			{
				return this._OrderTranDesc;
			}
			set
			{
				this._OrderTranDesc = value;
			}
		}
		#endregion

		#region VendorRefNbr
			public abstract class vendorRefNbr : PX.Data.BQL.BqlString.Field<vendorRefNbr>
		{
		}
		protected String _VendorRefNbr;
		[PXDBString(40, IsUnicode = true, BqlField = typeof(POOrder.vendorRefNbr))]
		[PXUIField(DisplayName = "Vendor Ref.")]
		public virtual String VendorRefNbr
		{
			get
			{
				return this._VendorRefNbr;
			}
			set
			{
				this._VendorRefNbr = value;
			}
		}
		#endregion

		#region PayToVendorID
			public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		[PXDBInt(BqlField = typeof(POOrder.payToVendorID))]
		public virtual int? PayToVendorID { get; set; }
		#endregion

		#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		[ProjectBase(BqlField = typeof(POOrder.projectID))]
		public virtual int? ProjectID
		{
			get;
			set;
		}
		#endregion
	}
}
