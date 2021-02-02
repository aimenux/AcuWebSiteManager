using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.Common.Discount;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.TX;

namespace PX.Objects.PO
{
	[PXProjection(typeof(Select2<POLine,
			InnerJoin<POOrder, On<POLine.orderType, Equal<POOrder.orderType>,
						And<POLine.orderNbr, Equal<POOrder.orderNbr>>>,
			LeftJoin<POAccrualStatus, On<POAccrualStatus.type, Equal<POAccrualType.order>,
				And<POAccrualStatus.refNoteID, Equal<POLine.orderNoteID>,
				And<POAccrualStatus.lineNbr, Equal<POLine.lineNbr>>>>>>>),
			Persistent = false)]
	[PXCacheName(Messages.POLineShort)]
	[Serializable]
	public partial class POLineRS : IBqlTable, IAPTranSource, ISortOrder
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
		[Branch(BqlField = typeof(POLine.branchID))]
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
		#region OrderType
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType>
		{
		}
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true, BqlField = typeof(POLine.orderType))]
		[PXUIField(DisplayName = "Order Type")]
		[POOrderType.List]
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

		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "", BqlField = typeof(POLine.orderNbr))]
		[PXUIField(DisplayName = "Order Nbr.")]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr>
		{
		}
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true, BqlField = typeof(POLine.lineNbr))]
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
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
		[PXDBInt(BqlField = typeof(POLine.sortOrder))]
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
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost>
		{
		}
		[PXDBBool(BqlField = typeof(POLine.accrueCost))]
		public virtual bool? AccrueCost
		{
			get;
			set;
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
		{
		}
		protected String _LineType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(POLine.lineType))]
		[POLineTypeList2(typeof(POLine.orderType), typeof(POLine.inventoryID))]
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
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
		}
		protected String _Status;
		[PXDBString(1, IsFixed = true, BqlField = typeof(POOrder.status))]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[POOrderStatus.List()]
		public virtual String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID>
		{
		}
		protected Int32? _VendorID;
		[POVendor(BqlField = typeof(POLine.vendorID))]
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
		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor, whom the AP bill will belong to. 
		/// </value>
		[PXFormula(typeof(Validate<POLineRS.curyID>))]
		[POOrderPayToVendor(CacheGlobal = true, Filterable = true, BqlField = typeof(POOrder.payToVendorID))]
		[Data.ReferentialIntegrity.Attributes.PXForeignReference(typeof(Field<POLineRS.payToVendorID>.IsRelatedTo<Vendor.bAccountID>))]
		public virtual int? PayToVendorID { get; set; }
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID>
		{
		}
		protected Int32? _VendorLocationID;
		[LocationID(typeof(Where<
			Location.bAccountID.IsEqual<vendorID.FromCurrent>.
			And<Location.isActive.IsEqual<True>>.
			And<MatchWithBranch<Location.vBranchID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(POOrder.vendorLocationID))]
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
		#region TaxZoneID
		public abstract class taxZoneID : Data.BQL.BqlString.Field<taxZoneID>
		{
		}
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POOrder.taxZoneID))]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion

		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate>
		{
		}
		protected DateTime? _OrderDate;
		[PXDBDate(BqlField = typeof(POLine.orderDate))]
		[PXUIField(DisplayName = "Order Date", Enabled = false)]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID>
		{
		}
		protected Int32? _SubItemID;
		[SubItem(typeof(POLineRS.inventoryID), BqlField = typeof(POLine.subItemID))]
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

		[POSiteAvail(typeof(POLineRS.inventoryID), typeof(POLineRS.subItemID), BqlField = typeof(POLine.siteID))]
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
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr>
		{
		}
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(POLine.lotSerialNbr))]
		[PXUIField(DisplayName = "Lot Serial Number", Visible = false)]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
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
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty>
		{
		}
		protected Decimal? _OrderQty;
		[PXDBQuantity(typeof(POLineRS.uOM), typeof(POLineRS.baseOrderQty), BqlField = typeof(POLine.orderQty))]
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
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty>
		{
		}
		protected Decimal? _BaseOrderQty;
		[PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
		[PXDBDecimal(6, BqlField = typeof(POLine.baseOrderQty))]
		public virtual Decimal? BaseOrderQty
		{
			get
			{
				return this._BaseOrderQty;
			}
			set
			{
				this._BaseOrderQty = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID>
		{
		}
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(POOrder.curyID))]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
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
		[CurrencyInfo]
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

		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(POLineRS.curyInfoID), typeof(POLineRS.unitCost), BqlField = typeof(POLine.curyUnitCost))]
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

		[PXDBPriceCost(BqlField = typeof(POLine.unitCost))]
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

		#region DiscPct
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct>
		{
		}
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(POLine.discPct))]
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
		protected Decimal? _CuryDiscAmt;
		[PXDBCurrency(typeof(POLineRS.curyInfoID), typeof(POLineRS.discAmt), BqlField = typeof(POLine.curyDiscAmt))]
		[PXUIField(DisplayName = "Discount Amount")]
		public virtual Decimal? CuryDiscAmt
		{
			get
			{
				return this._CuryDiscAmt;
			}
			set
			{
				this._CuryDiscAmt = value;
			}
		}
		#endregion
		#region DiscAmt
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt>
		{
		}
		protected Decimal? _DiscAmt;
		[PXDBDecimal(4, BqlField = typeof(POLine.discAmt))]
		public virtual Decimal? DiscAmt
		{
			get
			{
				return this._DiscAmt;
			}
			set
			{
				this._DiscAmt = value;
			}
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt>
		{
		}
		protected Decimal? _CuryLineAmt;
		[PXDBCurrency(typeof(POLineRS.curyInfoID), typeof(POLineRS.lineAmt), BqlField = typeof(POLine.curyLineAmt))]
		[PXUIField(DisplayName = "Ext. Cost")]
		public virtual Decimal? CuryLineAmt
		{
			get
			{
				return this._CuryLineAmt;
			}
			set
			{
				this._CuryLineAmt = value;
			}
		}
		#endregion
		#region LineAmt
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt>
		{
		}
		protected Decimal? _LineAmt;
		[PXDBDecimal(4, BqlField = typeof(POLine.lineAmt))]
		public virtual Decimal? LineAmt
		{
			get
			{
				return this._LineAmt;
			}
			set
			{
				this._LineAmt = value;
			}
		}
		#endregion

		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct>
		{
		}
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100, BqlField = typeof(POLine.retainagePct))]
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
		[PXDBCurrency(typeof(POLineRS.curyInfoID), typeof(POLineRS.retainageAmt), BqlField = typeof(POLine.curyRetainageAmt))]
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

		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost>
		{
		}
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(POLineRS.curyInfoID), typeof(POLineRS.extCost), MinValue = 0.0, BqlField = typeof(POLine.curyExtCost))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
		[PXUIField(DisplayName = "Amount")]
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
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate>
		{
		}
		protected Decimal? _GroupDiscountRate;
		[PXDBDecimal(6, BqlField = typeof(POLine.groupDiscountRate))]
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
		[PXDBDecimal(6, BqlField = typeof(POLine.documentDiscountRate))]
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
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID>
		{
		}
		protected String _TaxID;
		[PXDBString(Tax.taxID.Length, IsUnicode = true, BqlField = typeof(POLine.taxID))]
		[PXUIField(DisplayName = "Tax ID", Visible = false)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
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
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID>
		{
		}
		protected Int32? _ExpenseAcctID;
		[Account(typeof(POLineRS.branchID), DisplayName = "Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description), BqlField = typeof(POLine.expenseAcctID))]
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
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;
		[ProjectBase(BqlField = typeof(POLine.projectID))]
		public virtual Int32? ProjectID
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
		protected Int32? _TaskID;
		[ActiveOrInPlanningProjectTask(typeof(POLineRS.projectID), BatchModule.PO, DisplayName = "Project Task", BqlField = typeof(POLine.taskID))]
		public virtual Int32? TaskID
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
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID>
		{
		}
		protected Int32? _ExpenseSubID;
		[SubAccount(typeof(POLineRS.expenseAcctID), typeof(POLineRS.branchID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible, Filterable = true, BqlField = typeof(POLine.expenseSubID))]
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
		[Account(typeof(POLineRS.branchID), DescriptionField = typeof(Account.description), DisplayName = "Accrual Account", Filterable = false, BqlField = typeof(POLine.pOAccrualAcctID))]
		public virtual int? POAccrualAcctID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID>
		{
		}
		[SubAccount(typeof(POLineRS.pOAccrualAcctID), typeof(POLineRS.branchID), DisplayName = "Accrual Sub.", Filterable = true, BqlField = typeof(POLine.pOAccrualSubID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? POAccrualSubID
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc>
		{
		}
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(POLine.tranDesc))]
		[PXUIField(DisplayName = "Line Description", Visibility = PXUIVisibility.Visible)]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		protected Int32? _CostCodeID;
		[CostCode(typeof(expenseAcctID), typeof(taskID), GL.AccountType.Expense, BqlField = typeof(POLine.costCodeID))]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion

		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled>
		{
		}
		protected Boolean? _Cancelled;
		[PXDBBool(BqlField = typeof(POLine.cancelled))]
		[PXUIField(DisplayName = "Cancelled", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Cancelled
		{
			get
			{
				return this._Cancelled;
			}
			set
			{
				this._Cancelled = value;
			}
		}
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed>
		{
		}
		[PXDBBool(BqlField = typeof(POLine.closed))]
		[PXUIField(DisplayName = "Closed", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Closed
		{
			get;
			set;
		}
		#endregion
		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed>
		{
		}
		/// <summary>
		/// A flag that indicates (if set to <c>true</c>) that a PO Line is fully billed.
		/// </summary>
		[PXBool]
		[PXDBCalced(typeof(
			Switch<Case<Where<POLine.completedQty, Greater<POLine.billedQty>>, False>,
			Switch<Case<Where<POLine.completePOLine, Equal<CompletePOLineTypes.quantity>>,
				Switch<Case<Where<POLine.orderQty, LessEqual<decimal0>, Or<Div<Mult<POLine.orderQty, POLine.rcptQtyThreshold>, decimal100>, Greater<POLine.billedQty>>>, False>, True>>,
				Switch<Case<Where<Div<Mult<Add<POLine.curyExtCost, POLine.curyRetainageAmt>, POLine.rcptQtyThreshold>, decimal100>, Greater<POLine.curyBilledAmt>>, False>, True>>>),
			typeof(bool))]
		public virtual bool? Billed
		{
			get;
			set;
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

		#region DiscountID
		public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID>
		{
		}
		protected String _DiscountID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(POLine.discountID))]
		[PXSelector(typeof(Search<APDiscount.discountID, Where<APDiscount.bAccountID, Equal<Current<POLineRS.vendorID>>, And<APDiscount.type, Equal<DiscountType.LineDiscount>>>>))]
		[PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = false)]
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
		[PXUIField(DisplayName = "Discount Sequence", Visible = false, Enabled = false)]
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

		#region RefNoteID
		public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID>
		{
		}
		[PXDBGuid(BqlField = typeof(POAccrualStatus.refNoteID))]
		public virtual Guid? RefNoteID
		{
			get;
			set;
		}
		#endregion
		#region BilledUOM
		public abstract class billedUOM : PX.Data.BQL.BqlString.Field<billedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POAccrualStatus.billedUOM))]
		public virtual string BilledUOM
		{
			get;
			set;
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(POAccrualStatus.billedQty))]
		public virtual decimal? BilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.baseBilledQty, decimal0>), typeof(decimal))]
		[PXQuantity]
		public virtual decimal? BaseBilledQty
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.curyBilledAmt, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? CuryBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.billedAmt, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? BilledAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledCost
		public abstract class curyBilledCost : PX.Data.BQL.BqlDecimal.Field<curyBilledCost>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.curyBilledCost, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? CuryBilledCost
		{
			get;
			set;
		}
		#endregion
		#region BilledCost
		public abstract class billedCost : PX.Data.BQL.BqlDecimal.Field<billedCost>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.billedCost, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? BilledCost
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledDiscAmt
		public abstract class curyBilledDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledDiscAmt>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.curyBilledDiscAmt, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? CuryBilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledDiscAmt
		public abstract class billedDiscAmt : PX.Data.BQL.BqlDecimal.Field<billedDiscAmt>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.billedDiscAmt, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? BilledDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region ReceivedUOM
		public abstract class receivedUOM : PX.Data.BQL.BqlString.Field<receivedUOM>
		{
		}
		[PXDBString(6, IsUnicode = true, BqlField = typeof(POAccrualStatus.receivedUOM))]
		public virtual string ReceivedUOM
		{
			get;
			set;
		}
		#endregion
		#region ReceivedQty
		public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(POAccrualStatus.receivedQty))]
		public virtual decimal? ReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.baseReceivedQty, decimal0>), typeof(decimal))]
		[PXQuantity]
		public virtual decimal? BaseReceivedQty
		{
			get;
			set;
		}
		#endregion
		#region ReceivedCost
		public abstract class receivedCost : PX.Data.BQL.BqlDecimal.Field<receivedCost>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.receivedCost, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? ReceivedCost
		{
			get;
			set;
		}
		#endregion
		#region PPVAmt
		public abstract class pPVAmt : PX.Data.BQL.BqlDecimal.Field<pPVAmt>
		{
		}
		[PXDBCalced(typeof(IsNull<POAccrualStatus.pPVAmt, decimal0>), typeof(decimal))]
		[PXBaseCury]
		public virtual decimal? PPVAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryOrderBilledAmt
		public abstract class curyOrderBilledAmt : Data.BQL.BqlDecimal.Field<curyOrderBilledAmt>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(orderBilledAmt), BqlField = typeof(POLine.curyBilledAmt))]
		public virtual decimal? CuryOrderBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region OrderBilledAmt
		public abstract class orderBilledAmt : Data.BQL.BqlDecimal.Field<orderBilledAmt>
		{
		}
		[PXDBBaseCury(BqlField = typeof(POLine.billedAmt))]
		public virtual decimal? OrderBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region OrderBilledQty
		public abstract class orderBilledQty : Data.BQL.BqlDecimal.Field<orderBilledQty>
		{
		}
		[PXDBQuantity(typeof(uOM), typeof(baseOrderBilledQty), HandleEmptyKey = true, BqlField = typeof(POLine.billedQty))]
		public virtual decimal? OrderBilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderBilledQty
		public abstract class baseOrderBilledQty : Data.BQL.BqlDecimal.Field<baseOrderBilledQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POLine.baseBilledQty))]
		public virtual decimal? BaseOrderBilledQty
		{
			get;
			set;
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty>
		{
		}
		[PXDBQuantity(typeof(uOM), typeof(baseUnbilledQty), HandleEmptyKey = true, BqlField = typeof(POLine.unbilledQty))]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual decimal? UnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty>
		{
		}
		[PXDBDecimal(6, BqlField = typeof(POLine.baseUnbilledQty))]
		public virtual decimal? BaseUnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(unbilledAmt), BqlField = typeof(POLine.curyUnbilledAmt))]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual decimal? CuryUnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt>
		{
		}

		[PXDBBaseCury(BqlField = typeof(POLine.unbilledAmt))]
		public virtual decimal? UnbilledAmt
		{
			get;
			set;
		}
		#endregion

		#region ReqPrepaidQty
		public abstract class reqPrepaidQty : Data.BQL.BqlDecimal.Field<reqPrepaidQty>
		{
		}
		[PXDBQuantity(BqlField = typeof(POLine.reqPrepaidQty))]
		public virtual decimal? ReqPrepaidQty
		{
			get;
			set;
		}
		#endregion
		#region CuryReqPrepaidAmt
		public abstract class curyReqPrepaidAmt : Data.BQL.BqlDecimal.Field<curyReqPrepaidAmt>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(reqPrepaidAmt), BqlField = typeof(POLine.curyReqPrepaidAmt))]
		public virtual decimal? CuryReqPrepaidAmt
		{
			get;
			set;
		}
		#endregion
		#region ReqPrepaidAmt
		public abstract class reqPrepaidAmt : Data.BQL.BqlDecimal.Field<reqPrepaidAmt>
		{
		}
		[PXDBDecimal(4, BqlField = typeof(POLine.reqPrepaidAmt))]
		public virtual decimal? ReqPrepaidAmt
		{
			get;
			set;
		}
		#endregion

		#region IAPTranSource Members

		string IAPTranSource.OrigUOM
		{
			get
			{
				return this.UOM;
			}
		}

		decimal? IAPTranSource.OrigQty
		{
			get
			{
				return this.OrderQty;
			}
		}

		decimal? IAPTranSource.BaseOrigQty
		{
			get
			{
				return this.BaseOrderQty;
			}
		}

		decimal? IAPTranSource.BillQty
		{
			get
			{
				if (this.RefNoteID == null)
				{
					return this.OrderQty;
				}
				decimal? receivedQty = (this.ReceivedQty == 0m || this.ReceivedUOM == this.UOM) ? this.ReceivedQty : null;
				if (receivedQty == null)
				{
					return null;
				}
				decimal? billedQty = (this.BilledQty == 0m || this.BilledUOM == this.UOM) ? this.BilledQty : null;
				if (billedQty == null)
				{
					return null;
				}
				decimal? qtyToBill = ((this.OrderQty < receivedQty) ? receivedQty : this.OrderQty) - billedQty;
				return qtyToBill < 0m ? 0m : qtyToBill;
			}
		}

		decimal? IAPTranSource.BaseBillQty
		{
			get
			{
				if (this.RefNoteID == null)
				{
					return this.BaseOrderQty;
				}
				decimal? baseQtyToBill = ((this.BaseOrderQty < this.BaseReceivedQty) ? this.BaseReceivedQty : this.BaseOrderQty) - this.BaseBilledQty;
				return baseQtyToBill < 0m ? 0m : baseQtyToBill;
			}
		}

		bool IAPTranSource.IsPartiallyBilled
		{
			[PXDependsOnFields(typeof(baseBilledQty), typeof(curyBilledCost))]
			get
			{
				return this.BaseBilledQty != 0m || this.CuryBilledCost != 0m;
			}
		}

		Guid? IAPTranSource.POAccrualRefNoteID
		{
			get
			{
				return this.POAccrualType == Objects.PO.POAccrualType.Order ? this.OrderNoteID : null;
			}
		}

		int? IAPTranSource.POAccrualLineNbr
		{
			get
			{
				return this.POAccrualType == Objects.PO.POAccrualType.Order ? this.LineNbr : null;
			}
		}

		public virtual bool CompareReferenceKey(APTran aTran)
		{
			return (aTran.POAccrualType == this.POAccrualType
				&& aTran.POAccrualRefNoteID == ((IAPTranSource)this).POAccrualRefNoteID
				&& aTran.POAccrualLineNbr == ((IAPTranSource)this).POAccrualLineNbr);
		}

		public virtual void SetReferenceKeyTo(APTran aTran)
		{
			aTran.POAccrualType = this.POAccrualType;
			aTran.PONbr = this.OrderNbr;
			aTran.POOrderType = this.OrderType;
			aTran.POLineNbr = this.LineNbr;
			aTran.POAccrualRefNoteID = ((IAPTranSource)this).POAccrualRefNoteID;
			aTran.POAccrualLineNbr = ((IAPTranSource)this).POAccrualLineNbr;
		}

		public virtual bool IsReturn
		{
			get { return false; }
		}

		public virtual bool AggregateWithExistingTran
		{
			get { return false; }
		}

		#endregion
	}
}
