using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;

namespace PX.Objects.PO
{
	/// <summary>
	/// Read-only class for selector
	/// </summary>
	[PXProjection(typeof(Select2<POReceiptLine,
		LeftJoin<POLine, On<POLine.orderType, Equal<POReceiptLine.pOType>, And<POLine.orderNbr, Equal<POReceiptLine.pONbr>, And<POLine.lineNbr, Equal<POReceiptLine.pOLineNbr>>>>,
		LeftJoin<POOrder, On<POOrder.orderType, Equal<POReceiptLine.pOType>, And<POOrder.orderNbr, Equal<POReceiptLine.pONbr>>>>>>),
		Persistent = false)]
	[PXCacheName(Messages.POReceiptLine)]
	[Serializable]
	public partial class POReceiptLineS : IBqlTable, IAPTranSource, ISortOrder
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
		#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID>
		{
		}
		protected Int32? _BranchID;
		[Branch(BqlField = typeof(POReceiptLine.branchID))]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region ReceiptNbr
			public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr>
		{
		}
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POReceiptLine.receiptNbr))]
		[PXUIField(DisplayName = "Receipt Nbr.")]
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
		#region ReceiptType
			public abstract class receiptType : PX.Data.BQL.BqlString.Field<receiptType>
		{
		}
		protected String _ReceiptType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.receiptType))]
		[PXUIField(DisplayName = "Receipt Type")]
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
		#region LineNbr
			public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POReceiptLine.lineNbr))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region SortOrder
			public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder>
		{
		}
		protected Int32? _SortOrder;
		[PXDBInt(BqlField = typeof(POReceiptLine.sortOrder))]
		public virtual Int32? SortOrder
		{
			get
			{
				return this._SortOrder;
			}
			set
			{
				this._SortOrder = value;
			}
		}
		#endregion
		#region LineType
			public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.lineType))]
		[POLineType.List()]
		[PXUIField(DisplayName = "Line Type")]
		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region InventoryID
			public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[Inventory(Filterable = true, BqlField = typeof(POReceiptLine.inventoryID))]
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
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost>
		{
		}
		[PXDBBool(BqlField = typeof(POReceiptLine.accrueCost))]
		public virtual bool? AccrueCost
		{
			get;
			set;
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}
		protected Int32? _VendorID;
		[Vendor(BqlField = typeof(POReceiptLine.vendorID), Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region ReceiptDate
			public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate>
		{
		}

		protected DateTime? _ReceiptDate;
		[PXDBDate(BqlField = typeof(POReceiptLine.receiptDate))]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region SubItemID
			public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
		{
		}
		protected Int32? _SubItemID;
		[SubItem(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.subItemID))]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
			public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID>
		{
		}
		protected Int32? _SiteID;
		[IN.SiteAvail(typeof(POReceiptLineS.inventoryID), typeof(POReceiptLineS.subItemID), BqlField = typeof(POReceiptLine.siteID))]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region UOM
			public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;
		[INUnit(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POReceiptLine.uOM))]
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
		#region ReceiptQty
			public abstract class receiptQty : PX.Data.BQL.BqlDecimal.Field<receiptQty>
		{
		}

		protected Decimal? _ReceiptQty;
		[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseReceiptQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.receiptQty))]
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
		#region BaseReceiptQty
			public abstract class baseReceiptQty : PX.Data.BQL.BqlDecimal.Field<baseReceiptQty>
		{
		}

		protected Decimal? _BaseReceiptQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseReceiptQty))]
		public virtual Decimal? BaseReceiptQty
		{
			get
			{
				return this._BaseReceiptQty;
			}
			set
			{
				this._BaseReceiptQty = value;
			}
		}
		#endregion
		#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Order Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region CuryInfoID
			public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID>
		{
		}
		protected Int64? _CuryInfoID;
		[PXDBLong(BqlField = typeof(POLine.curyInfoID))]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region CuryUnitCost
			public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost>
		{
		}
		protected Decimal? _CuryUnitCost;

		[PXDBDecimal(6, BqlField = typeof(POLine.curyUnitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryUnitCost
		{
			get
			{
				return this._CuryUnitCost;
			}
			set
			{
				this._CuryUnitCost = value;
			}
		}
		#endregion
		#region UnitCost
			public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost>
		{
		}
		protected Decimal? _UnitCost;

		[PXDBDecimal(6, BqlField = typeof(POLine.unitCost))]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region CuryExtCost
			public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost>
		{
		}
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.extCost), BqlField = typeof(POLine.curyExtCost))]
		[PXUIField(DisplayName = "Order Line Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryExtCost
		{
			get
			{
				return this._CuryExtCost;
			}
			set
			{
				this._CuryExtCost = value;
			}
		}
		#endregion
		#region ExtCost
			public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost>
		{
		}
		protected Decimal? _ExtCost;
		[PXDBBaseCury(BqlField = typeof(POLine.extCost))]
		public virtual Decimal? ExtCost
		{
			get
			{
				return this._ExtCost;
			}
			set
			{
				this._ExtCost = value;
			}
		}
		#endregion
		#region CuryLineAmt
			public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.lineAmt), BqlField = typeof(POLine.curyLineAmt))]
		public virtual decimal? CuryLineAmt
		{
			get;
			set;
		}
		#endregion
		#region LineAmt
			public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.lineAmt))]
		public virtual decimal? LineAmt
		{
			get;
			set;
		}
		#endregion
		#region DiscPct
			public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct>
		{
		}
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(POLine.discPct))]
		[PXUIField(DisplayName = "Discount Percent")]
		public virtual Decimal? DiscPct
		{
			get
			{
				return this._DiscPct;
			}
			set
			{
				this._DiscPct = value;
			}
		}
		#endregion
		#region CuryDiscAmt
			public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.discAmt), BqlField = typeof(POLine.curyDiscAmt))]
		public virtual decimal? CuryDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region DiscAmt
			public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.discAmt))]
		public virtual decimal? DiscAmt
		{
			get;
			set;
		}
		#endregion

		#region ReceiptUnitCost
			public abstract class receiptUnitCost : PX.Data.BQL.BqlDecimal.Field<receiptUnitCost>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.unitCost))]
		public virtual decimal? ReceiptUnitCost
		{
			get;
			set;
		}
		#endregion
		#region ReceiptTranCost
			public abstract class receiptTranCost : PX.Data.BQL.BqlDecimal.Field<receiptTranCost>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POReceiptLine.tranCost))]
		public virtual decimal? ReceiptTranCost
		{
			get;
			set;
		}
		#endregion

		#region UnbilledQty
			public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty>
		{
		}
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(POReceiptLineS.uOM), typeof(POReceiptLineS.baseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POReceiptLine.unbilledQty))]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual Decimal? UnbilledQty
		{
			get
			{
				return this._UnbilledQty;
			}
			set
			{
				this._UnbilledQty = value;
			}
		}
		#endregion
		#region BaseUnbilledQty
			public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty>
		{
		}
		protected Decimal? _BaseUnbilledQty;
		[PXDBDecimal(6, BqlField = typeof(POReceiptLine.baseUnbilledQty))]
		public virtual Decimal? BaseUnbilledQty
		{
			get
			{
				return this._BaseUnbilledQty;
			}
			set
			{
				this._BaseUnbilledQty = value;
			}
		}
		#endregion

		#region OrderUOM
			public abstract class orderUOM : PX.Data.BQL.BqlString.Field<orderUOM>
		{
		}
		[INUnit(typeof(POReceiptLineS.inventoryID), BqlField = typeof(POLine.uOM))]
		public virtual string OrderUOM
		{
			get;
			set;
		}
		#endregion
		#region OrderQty
			public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty>
		{
		}
		[PXDBQuantity(typeof(POReceiptLineS.orderUOM), typeof(POReceiptLineS.baseOrderQty), BqlField = typeof(POLine.orderQty))]
		[PXUIField(DisplayName = "Ordered Qty.")]
		public virtual decimal? OrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderQty
			public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOrderQty))]
		public virtual decimal? BaseOrderQty
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
			public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100, BqlField = typeof(POLine.retainagePct))]
		[PXUIField(DisplayName = "Retainage Percent", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageAmt
			public abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt>
		{
		}
		[PXDBCurrency(typeof(POReceiptLineS.curyInfoID), typeof(POReceiptLineS.retainageAmt), BqlField = typeof(POLine.curyRetainageAmt))]
		[PXUIField(DisplayName = "Retainage Amount", FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual decimal? CuryRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmt
			public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.retainageAmt))]
		public virtual decimal? RetainageAmt
		{
			get;
			set;
		}
		#endregion

		#region GroupDiscountRate
			public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate>
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(POLine.groupDiscountRate))]
		public virtual Decimal? GroupDiscountRate
		{
			get
			{
				return this._GroupDiscountRate;
			}
			set
			{
				this._GroupDiscountRate = value;
			}
		}
		#endregion
		#region DocumentDiscountRate
			public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate>
		{
		}
		protected Decimal? _DocumentDiscountRate;
		[PXDBDecimal(18, BqlField = typeof(POLine.documentDiscountRate))]
		public virtual Decimal? DocumentDiscountRate
		{
			get
			{
				return this._DocumentDiscountRate;
			}
			set
			{
				this._DocumentDiscountRate = value;
			}
		}
		#endregion

		#region TaxCategoryID
			public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID>
		{
		}
		protected String _TaxCategoryID;

		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.taxCategoryID))]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion
		#region ExpenseAcctID
			public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID>
		{
		}
		protected Int32? _ExpenseAcctID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseAcctID))]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
			public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID>
		{
		}
		protected Int32? _ExpenseSubID;

		[PXDBInt(BqlField = typeof(POReceiptLine.expenseSubID))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
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
		#region TranDesc
			public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc>
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POReceiptLine.tranDesc))]
		[PXUIField(DisplayName = "Transaction Descr.", Visibility = PXUIVisibility.Visible)]
		public virtual String TranDesc
		{
			get
			{
				return this._TranDesc;
			}
			set
			{
				this._TranDesc = value;
			}
		}
		#endregion
		#region TaxID
			public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID>
		{
		}
		protected String _TaxID;
		[PXDBString(TX.Tax.taxID.Length, IsUnicode = true, BqlField = typeof(POLine.taxID))]
		[PXUIField(DisplayName = "Tax ID", Visible = false)]
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion

		#region ProjectID
			public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected int? _ProjectID;
		[PXDBInt(BqlField = typeof(POReceiptLine.projectID))]
		public virtual int? ProjectID
		{
			get
			{
				return this._ProjectID;
			}
			set
			{
				this._ProjectID = value;
			}
		}
		#endregion
		#region TaskID
			public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID>
		{
		}
		protected int? _TaskID;
		[PXDBInt(BqlField = typeof(POReceiptLine.taskID))]
		public virtual int? TaskID
		{
			get
			{
				return this._TaskID;
			}
			set
			{
				this._TaskID = value;
			}
		}
		#endregion
		#region CostCodeID
			public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		[PXDBInt(BqlField = typeof(POReceiptLine.costCodeID))]
		public virtual int? CostCodeID
		{
			get;
			set;
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

		#region POType
			public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType>
		{
		}
		protected String _POType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POReceiptLine.pOType))]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
			public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr>
		{
		}
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true, BqlField = typeof(POReceiptLine.pONbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
			public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr>
		{
		}
		protected Int32? _POLineNbr;
		[PXDBInt(BqlField = typeof(POReceiptLine.pOLineNbr))]
		[PXUIField(DisplayName = "PO Line Nbr.")]
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion

		#region DiscountID
			public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID>
		{
		}
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.discountID))]
		public virtual String DiscountID
		{
			get
			{
				return this._DiscountID;
			}
			set
			{
				this._DiscountID = value;
			}
		}
		#endregion
		#region DiscountSequenceID
			public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID>
		{
		}
		protected String _DiscountSequenceID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.discountSequenceID))]
		public virtual String DiscountSequenceID
		{
			get
			{
				return this._DiscountSequenceID;
			}
			set
			{
				this._DiscountSequenceID = value;
			}
		}
		#endregion

		#region IAPTranSource Members

		protected bool IsDirectReceipt
		{
			get { return this.OrderQty == null; }
		}

		string IAPTranSource.OrigUOM
		{
			get
			{
				return this.IsDirectReceipt ? this.UOM : this.OrderUOM;
			}
		}

		public virtual decimal? OrigQty
		{
			get
			{
				return this.IsDirectReceipt ? this.ReceiptQty : this.OrderQty;
			}
		}

		public virtual decimal? BaseOrigQty
		{
			get
			{
				return this.IsDirectReceipt ? this.BaseReceiptQty : this.BaseOrderQty;
			}
		}

		decimal? IAPTranSource.BillQty
		{
			get
			{
				return this.UnbilledQty;
			}
		}

		decimal? IAPTranSource.BaseBillQty
		{
			get
			{
				return this.BaseUnbilledQty;
			}
		}

		decimal? IAPTranSource.LineAmt
		{
			get
			{
				return this.IsDirectReceipt ? this.ReceiptTranCost : this.LineAmt;
			}
		}

		decimal? IAPTranSource.UnitCost
		{
			get
			{
				return this.IsDirectReceipt ? this.ReceiptUnitCost : this.UnitCost;
			}
		}

		bool IAPTranSource.IsReturn
		{
			get
			{
				return this.ReceiptType == POReceiptType.POReturn;
			}
		}

		bool IAPTranSource.IsPartiallyBilled
		{
			get
			{
				return this.BaseOrigQty != this.BaseUnbilledQty;
			}
		}

		bool IAPTranSource.AggregateWithExistingTran
		{
			get
			{
				return this.POAccrualType == Objects.PO.POAccrualType.Order;
			}
		}

		public virtual bool CompareReferenceKey(APTran aTran)
		{
			return aTran.POAccrualType == this.POAccrualType
				&& aTran.POAccrualRefNoteID == this.POAccrualRefNoteID
				&& aTran.POAccrualLineNbr == this.POAccrualLineNbr;
		}
		public virtual void SetReferenceKeyTo(APTran aTran)
		{
			bool orderPOAccrual = (this.POAccrualType == Objects.PO.POAccrualType.Order);
			aTran.POAccrualType = this.POAccrualType;
			aTran.POAccrualRefNoteID = this.POAccrualRefNoteID;
			aTran.POAccrualLineNbr = this.POAccrualLineNbr;
			aTran.ReceiptType = orderPOAccrual ? null : this.ReceiptType;
			aTran.ReceiptNbr = orderPOAccrual ? null : this.ReceiptNbr;
			aTran.ReceiptLineNbr = orderPOAccrual ? null : this.LineNbr;
			//TODO: move to IAPTranSource, current implementation to preserve exact behavior when SubItem was shown from joined POReceiptLine
			aTran.SubItemID = orderPOAccrual ? null : this.SubItemID;
			aTran.POOrderType = this.POType;
			aTran.PONbr = this.PONbr;
			aTran.POLineNbr = this.POLineNbr;
		}

		#endregion
	}
}
