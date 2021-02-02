using System;
using System.Collections;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.Objects.Common;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Bql;
using PX.Objects.IN.Matrix.Interfaces;

namespace PX.Objects.SO
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.SOLine)]
	public partial class SOLine : PX.Data.IBqlTable, ILSPrimary, IHasMinGrossProfit, ISortOrder, IMatrixItemLine
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOLine>.By<orderType, orderNbr, lineNbr>
		{
			public static SOLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}
		public static class FK
		{
			public class Order : SOOrder.PK.ForeignKeyOf<SOLine>.By<orderType, orderNbr> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<SOLine>.By<orderType> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<SOLine>.By<inventoryID> { }
			public class OrigLine : SOLine.PK.ForeignKeyOf<SOLine>.By<origOrderType, origOrderNbr, origLineNbr> { }
			public class POSite : INSite.PK.ForeignKeyOf<SOLine>.By<pOSiteID> { }
			public class OrderTypeOperation : SOOrderTypeOperation.PK.ForeignKeyOf<SOLine>.By<orderType, operation> { }
			public class Site : INSite.PK.ForeignKeyOf<SOLine>.By<siteID> { }
			public class ReasonCode : CS.ReasonCode.PK.ForeignKeyOf<SOLine>.By<reasonCode> { }
		}
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(SOOrder.branchID))]
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
		public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }
		protected String _OrderType;
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(typeof(SOOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<SOOrderType.orderType>), CacheGlobal = true)]
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
		public abstract class orderNbr : PX.Data.BQL.BqlString.Field<orderNbr> { }
		protected String _OrderNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBDefault(typeof(SOOrder.orderNbr), DefaultForUpdate = false)]
		[PXParent(typeof(FK.Order))]
		[PXUIField(DisplayName = "Order Nbr.", Visible = false, Enabled = false)]
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
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(SOOrder.lineCntr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
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
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
		protected Int32? _SortOrder;
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
		[PXDBInt]
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
		#region Behavior
		public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }
		protected String _Behavior;
		[PXDBString(2, IsFixed = true, InputMask = ">aa")]
		[PXDefault(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<orderType>>>>))]
		public virtual String Behavior
		{
			get
			{
				return this._Behavior;
			}
			set
			{
				this._Behavior = value;
			}
		}
		#endregion
		#region Operation
		public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
		protected String _Operation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		[PXUIField(DisplayName = "Operation", Visibility = PXUIVisibility.Dynamic)]
		[PXDefault(typeof(SOOrderType.defaultOperation))]
		[SOOperation.List]
		[PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<SOLine.orderType>>>>))]
		public virtual String Operation
		{
			get
			{
				return this._Operation;
			}
			set
			{
				this._Operation = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected String _ShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(SOOrder.shipComplete))]
		[SOShipComplete.List()]
		[PXUIField(DisplayName="Shipping Rule")]
		public virtual String ShipComplete
		{
			get
			{
				return this._ShipComplete;
			}
			set
			{
				this._ShipComplete = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		protected Boolean? _Completed;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = true)]
		public virtual Boolean? Completed
		{
			get
			{
				return this._Completed;
			}
			set
			{
				this._Completed = value;
			}
		}
		#endregion
        #region OpenLine
        public abstract class openLine : PX.Data.BQL.BqlBool.Field<openLine> { }
        protected Boolean? _OpenLine;
        [PXDBBool()]
        [DirtyFormula(typeof(Switch<Case<Where<SOLine.requireShipping, Equal<True>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>, And<SOLine.completed, NotEqual<True>, And<SOLine.orderQty, Greater<decimal0>>>>>, True>, False>), typeof(OpenLineCalc<SOOrder.openLineCntr>))]
        [PXUIField(DisplayName = "Open Line", Enabled = false)]
        public virtual Boolean? OpenLine
        {
            get
            {
                return this._OpenLine;
            }
            set
            {
                this._OpenLine = value;
            }
        }
        #endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt()]
		[PXDefault(typeof(SOOrder.customerID))]
		public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate()]
		[PXDBDefault(typeof(SOOrder.orderDate))]
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
		#region CancelDate
		public abstract class cancelDate : PX.Data.BQL.BqlDateTime.Field<cancelDate> { }
		protected DateTime? _CancelDate;
		[PXDBDate()]
		[PXDefault(typeof(SOOrder.cancelDate))]
		[PXUIField(DisplayName = "Cancel By", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? CancelDate
		{
			get
			{
				return this._CancelDate;
			}
			set
			{
				this._CancelDate = value;
			}
		}
		#endregion
		#region RequestDate
		public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }
		protected DateTime? _RequestDate;
		[PXDBDate()]
		[PXDefault(typeof(SOOrder.requestDate))]
		[PXUIField(DisplayName = "Requested On", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? RequestDate
		{
			get
			{
				return this._RequestDate;
			}
			set
			{
				this._RequestDate = value;
			}
		}
		#endregion
		#region ShipDate
		public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
		protected DateTime? _ShipDate;
		[PXDBDate()]
		//[PXDefault(typeof(SOOrder.shipDate))]
		[PXFormula(typeof(DateMinusDaysNotLessThenDate<SOLine.requestDate, IsNull<Selector<Current<SOOrder.customerLocationID>,Location.cLeadTime>, decimal0>, Current<SOOrder.orderDate>>))]
		[PXUIField(DisplayName = "Ship On", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? ShipDate
		{
			get
			{
				return this._ShipDate;
			}
			set
			{
				this._ShipDate = value;
			}
		}
		#endregion
		#region InvoiceType
		public abstract class invoiceType : PX.Data.BQL.BqlString.Field<invoiceType> { }

		/// <summary>
		/// Type of the Invoice to which the return SO line is applied.
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Invoice Type", Enabled = false, Visibility = PXUIVisibility.Dynamic)]
		[ARDocType.List()]
		public virtual string InvoiceType { get; set; }
		#endregion
		#region InvoiceNbr
		public abstract class invoiceNbr : PX.Data.BQL.BqlString.Field<invoiceNbr> { }
		protected String _InvoiceNbr;
		/// <summary>
		/// Number of the Invoice to which the return SO line is applied.
		/// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Invoice Nbr.", Enabled = false, Visibility = PXUIVisibility.Dynamic)]
		public virtual String InvoiceNbr
		{
			get
			{
				return this._InvoiceNbr;
			}
			set
			{
				this._InvoiceNbr = value;
			}
		}
		#endregion
		#region InvoiceLineNbr
		public abstract class invoiceLineNbr : PX.Data.BQL.BqlInt.Field<invoiceLineNbr> { }
		/// <summary>
		/// Number of the Invoice line to which the return SO line is applied.
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Invoice Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? InvoiceLineNbr
		{
			get;
			set;
		}
		#endregion
		#region InvoiceDate
		public abstract class invoiceDate : PX.Data.BQL.BqlDateTime.Field<invoiceDate> { }
		protected DateTime? _InvoiceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Original Sale Date")]
		public virtual DateTime? InvoiceDate
		{
			get
			{
				return this._InvoiceDate;
			}
			set
			{
				this._InvoiceDate = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
		protected Int16? _InvtMult;
		[PXDBShort()]
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.invtMult>))]
		[PXUIField(DisplayName = "Inventory Multiplier")]
		public virtual Int16? InvtMult
		{
			get
			{
				return this._InvtMult;
			}
			set
			{
				this._InvtMult = value;
			}
		}
		#endregion
		#region ManualPrice
		public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
		protected Boolean? _ManualPrice;
		[PXDBBool()]
		[PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
		public virtual Boolean? ManualPrice
		{
			get
			{
				return this._ManualPrice;
			}
			set
			{
				this._ManualPrice = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule : 
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<SOLine,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And<lineType, In3<SOLineType.inventory, SOLineType.nonInventory>,
						And<completed, NotEqual<True>>>>>>
			{ }
		}
		protected Int32? _InventoryID;
		[SOLineInventoryItem(Filterable=true)]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventoryItem))]
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
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true)]
		[SOLineType.List()]
		[PXUIField(DisplayName = "Line Type", Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<SOLine.inventoryID, Switch<
			Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
			Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>, 
			SOLineType.miscCharge>>))]
		[PXFormula(null, typeof(CountCalc<SOOrderSite.lineCntr>))]
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
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXBool]
        [PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<SOLine.inventoryID, InventoryItem.stkItem>))]
        public virtual Boolean? IsStockItem
        {
            get;
            set;
        }
        #endregion
		#region IsKit
		public abstract class isKit : PX.Data.BQL.BqlBool.Field<isKit> { }
		[PXBool]
		[PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXFormula(typeof(Selector<SOLine.inventoryID, InventoryItem.kitItem>))]
		public virtual Boolean? IsKit
		{
			get;
			set;
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<SOLine.inventoryID>))]
		[SubItem(typeof(SOLine.inventoryID))]
		[SubItemStatusVeryfier(typeof(SOLine.inventoryID), typeof(SOLine.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.iNDocType>))]
		[PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
		public virtual String TranType
		{
			get
			{
				return this._TranType;
			}
			set
			{
				this._TranType = value;
			}
		}
		#endregion
		#region TranDate
		public virtual DateTime? TranDate
		{
			get { return this._OrderDate; }
		}
		#endregion
		#region PlanType
		public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
		protected String _PlanType;
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.orderPlanType>))]
		[PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
		public virtual String PlanType
		{
			get
			{
				return this._PlanType;
			}
			set
			{
				this._PlanType = value;
			}
		}
		#endregion
		#region OrigPlanType
		public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
		[PXDBString(2, IsFixed = true)]
		[PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
		public virtual String OrigPlanType
		{
			get;
			set;
		}
		#endregion   
		#region RequireReasonCode
		public abstract class requireReasonCode : PX.Data.BQL.BqlBool.Field<requireReasonCode> { }
		protected Boolean? _RequireReasonCode;
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.requireReasonCode>))]
		[PXBool]
		public virtual Boolean? RequireReasonCode
		{
			get
			{
				return this._RequireReasonCode;
			}
			set
			{
				this._RequireReasonCode = value;
			}
		}
		#endregion
		#region RequireShipping
		public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
		protected bool? _RequireShipping;
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireShipping>))]
		public virtual bool? RequireShipping
		{
			get
			{
				return this._RequireShipping;
			}
			set
			{
				this._RequireShipping = value;
			}
		}
		#endregion
		#region RequireAllocation
		public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
		protected bool? _RequireAllocation;
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireAllocation>))]
		public virtual bool? RequireAllocation
		{
			get
			{
				return this._RequireAllocation;
			}
			set
			{
				this._RequireAllocation = value;
			}
		}
		#endregion
		#region RequireLocation
		public abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }
		protected bool? _RequireLocation;
        [PXBool()]
        [PXFormula(typeof(Selector<SOLine.orderType, SOOrderType.requireLocation>))]
		public virtual bool? RequireLocation
		{
			get
			{
				return this._RequireLocation;
			}
			set
			{
				this._RequireLocation = value;
			}
		}
		#endregion
		#region LineQtyAvail
		public abstract class lineQtyAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyAvail> { }
		[PXDecimal(6)]
		public decimal? LineQtyAvail
		{
			get;
			set;
		}
		#endregion
		#region LineQtyHardAvail
		public abstract class lineQtyHardAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyHardAvail> { }
		[PXDecimal(6)]
		public decimal? LineQtyHardAvail
		{
			get;
			set;
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[SiteAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID))]
		[PXParent(typeof(Select<SOOrderSite, Where<SOOrderSite.orderType, Equal<Current<SOLine.orderType>>, And<SOOrderSite.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOOrderSite.siteID, Equal<Current2<SOLine.siteID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(IIf<Where<SOLine.lineType, NotEqual<SOLineType.miscCharge>>, True, False>))]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<SOOrder.branchID>>,
			Or<Current<SOOrder.behavior>, Equal<SOBehavior.qT>>>))]
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
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		protected Int32? _LocationID;
		[SOLocationAvail(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.siteID), typeof(SOLine.tranType), typeof(SOLine.invtMult))]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[INLotSerialNbr(typeof(SOLine.inventoryID), typeof(SOLine.subItemID), typeof(SOLine.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region OrigOrderType
		public abstract class origOrderType : PX.Data.BQL.BqlString.Field<origOrderType> { }
		protected String _OrigOrderType;
		[PXDBString(2, IsFixed = true)]
		public virtual String OrigOrderType
		{
			get
			{
				return this._OrigOrderType;
			}
			set
			{
				this._OrigOrderType = value;
			}
		}
		#endregion
		#region OrigOrderNbr
		public abstract class origOrderNbr : PX.Data.BQL.BqlString.Field<origOrderNbr> { }
		protected String _OrigOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String OrigOrderNbr
		{
			get
			{
				return this._OrigOrderNbr;
			}
			set
			{
				this._OrigOrderNbr = value;
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
		protected Int32? _OrigLineNbr;
		[PXDBInt()]
		public virtual Int32? OrigLineNbr
		{
			get
			{
				return this._OrigLineNbr;
			}
			set
			{
				this._OrigLineNbr = value;
			}
		}
		#endregion		
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(SOLine.inventoryID), DisplayName="UOM")]     
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
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
		#region ClosedQty
		public abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }
		protected Decimal? _ClosedQty;
		[PXDBCalced(typeof(Sub<SOLine.orderQty, SOLine.openQty>), typeof(decimal))]
		[PXQuantity(typeof(SOLine.uOM), typeof(SOLine.baseClosedQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ClosedQty
		{
			get
			{
				return this._ClosedQty;
			}
			set
			{
				this._ClosedQty = value;
			}
		}
		#endregion
		#region BaseClosedQty
		public abstract class baseClosedQty : PX.Data.BQL.BqlDecimal.Field<baseClosedQty> { }
		protected Decimal? _BaseClosedQty;
		[PXDBCalced(typeof(Sub<SOLine.baseOrderQty, SOLine.baseOpenQty>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseClosedQty
		{
			get
			{
				return this._BaseClosedQty;
			}
			set
			{
				this._BaseClosedQty = value;
			}
		}
		#endregion
		#region OrderQty
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseOrderQty), InventoryUnitType.SalesUnit)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		//[PXFormula(null, typeof(SumCalc<SOOrder.orderQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where<SOLine.operation, Equal<Parent<SOOrder.defaultOperation>>, 
																					 And<SOLine.lineType, NotEqual<SOLineType.miscCharge>>>, 
																				 SOLine.orderQty>, 
																				 decimal0>),
			typeof(SumCalc<SOOrder.orderQty>))]
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
		public virtual Decimal? Qty
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
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;
		[PXDBDecimal(6, MinValue=0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
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
		public virtual Decimal? BaseQty
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
		#region UnassignedQty
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }
		protected Decimal? _UnassignedQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnassignedQty
		{
			get
			{
				return this._UnassignedQty;
			}
			set
			{
				this._UnassignedQty = value;
			}
		}
		#endregion
		#region ShippedQty
		public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
		protected Decimal? _ShippedQty;
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseShippedQty), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Shipments", Enabled = false)]
		public virtual Decimal? ShippedQty
		{
			get
			{
				return this._ShippedQty;
			}
			set
			{
				this._ShippedQty = value;
			}
		}
		#endregion
		#region BaseShippedQty
		public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
		protected Decimal? _BaseShippedQty;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseShippedQty
		{
			get
			{
				return this._BaseShippedQty;
			}
			set
			{
				this._BaseShippedQty = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseOpenQty), MinValue = 0)]
		[PXFormula(typeof(Switch<Case<Where<SOLine.requireShipping, Equal<True>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>, And<SOLine.completed, NotEqual<True>>>>, Sub<SOLine.orderQty, SOLine.closedQty>>, decimal0>), typeof(SumCalc<SOOrder.openOrderQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get
			{
				return this._OpenQty;
			}
			set
			{
				this._OpenQty = value;
			}
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		protected Decimal? _BaseOpenQty;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Open Qty.")]
		public virtual Decimal? BaseOpenQty
		{
			get
			{
				return this._BaseOpenQty;
			}
			set
			{
				this._BaseOpenQty = value;
			}
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty> { }
		protected Decimal? _BilledQty;
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseBilledQty), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Quantity", Enabled = false)]
		public virtual Decimal? BilledQty
		{
			get
			{
				return this._BilledQty;
			}
			set
			{
				this._BilledQty = value;
			}
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty> { }
		protected Decimal? _BaseBilledQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseBilledQty
		{
			get
			{
				return this._BaseBilledQty;
			}
			set
			{
				this._BaseBilledQty = value;
			}
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		protected Decimal? _UnbilledQty;
		[PXDBQuantity(typeof(SOLine.uOM), typeof(SOLine.baseUnbilledQty), MinValue = 0)]
		[PXFormula(typeof(Switch<Case<Where<SOLine.requireShipping, Equal<True>, And<SOLine.lineType, NotEqual<SOLineType.miscCharge>, And<SOLine.completed, Equal<True>>>>, Sub<SOLine.shippedQty, SOLine.billedQty>>, Sub<SOLine.orderQty, SOLine.billedQty>>), typeof(SumCalc<SOOrder.unbilledOrderQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Quantity", Enabled = false)]
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
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		protected Decimal? _BaseUnbilledQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CompleteQtyMin
		public abstract class completeQtyMin : PX.Data.BQL.BqlDecimal.Field<completeQtyMin> { }
		protected Decimal? _CompleteQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			SourceField = typeof(InventoryItem.undershipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Undership Threshold (%)")]
		public virtual Decimal? CompleteQtyMin
		{
			get
			{
				return this._CompleteQtyMin;
			}
			set
			{
				this._CompleteQtyMin = value;
			}
		}
		#endregion
		#region CompleteQtyMax
		public abstract class completeQtyMax : PX.Data.BQL.BqlDecimal.Field<completeQtyMax> { }
		protected Decimal? _CompleteQtyMax;
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			SourceField = typeof(InventoryItem.overshipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Overship Threshold (%)")]
		public virtual Decimal? CompleteQtyMax
		{
			get
			{
				return this._CompleteQtyMax;
			}
			set
			{
				this._CompleteQtyMax = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(SOOrder.curyInfoID))]
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
		#region PriceType
		public abstract class priceType : IBqlField
		{
		}
		[PXDBString(1, IsFixed = true)]
		[PriceTypes.List]
		[PXUIField(DisplayName = "Price Type", Visible = false, Enabled = false)]
		public virtual string PriceType
		{
			get;
			set;
		}
		#endregion
		#region IsPromotionalPrice
		public abstract class isPromotionalPrice : IBqlField
		{
		}
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotional Price", Visible = false, Enabled = false)]
		public virtual bool? IsPromotionalPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.unitPrice))]
		[PXUIField(DisplayName = "Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion
		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }
		protected Decimal? _UnitPrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Unit Price", Enabled = false)]
		public virtual Decimal? UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this._UnitPrice = value;
			}
		}
		#endregion
		#region CuryUnitCost
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		protected Decimal? _CuryUnitCost;
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.unitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.Dynamic)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemSite.tranUnitCost, Where<INItemSite.inventoryID, Equal<Current<SOLine.inventoryID>>, And<INItemSite.siteID, Equal<Current<SOLine.siteID>>>>>))]
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
		#region CuryExtPrice
		public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
		protected Decimal? _CuryExtPrice;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.extPrice))]
		[PXUIField(DisplayName = "Ext. Price")]
		[PXFormula(typeof(Mult<SOLine.orderQty, SOLine.curyUnitPrice>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryExtPrice
		{
			get
			{
				return this._CuryExtPrice;
			}
			set
			{
				this._CuryExtPrice = value;
			}
		}
		#endregion
		#region ExtPrice
		public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }
		protected Decimal? _ExtPrice;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? ExtPrice
		{
			get
			{
				return this._ExtPrice;
			}
			set
			{
				this._ExtPrice = value;
			}
		}
		#endregion
		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.extCost))]
		[PXUIField(DisplayName = "Extended Cost")]
		[PXFormula(typeof(Mult<SOLine.orderQty, SOLine.curyUnitCost>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
		protected Decimal? _ExtCost;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[SOTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), typeof(SOOrder.taxCalcMode), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.orderQty))]
		[SOOpenTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.openQty))]
		[SOUnbilledTax(typeof(SOOrder), typeof(SOTax), typeof(SOTaxTran), TaxCalc = TaxCalc.ManualLineCalc,
			   //Per Unit Tax settings
			   Inventory = typeof(SOLine.inventoryID), UOM = typeof(SOLine.uOM), LineQty = typeof(SOLine.unbilledQty))]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing, SearchOnDefault = false)]
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
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }

		[PXDefault(TXAvalaraCustomerUsageType.Default, typeof(SOOrder.avalaraCustomerUsageType))]
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type")]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[AlternativeItem(INPrimaryAlternateType.CPN, typeof(customerID), typeof(inventoryID), typeof(subItemID), typeof(uOM))]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6, MinValue = 0, MaxValue=100)]
		public virtual Decimal? CommnPct
		{
			get
			{
				return this._CommnPct;
			}
			set
			{
				this._CommnPct = value;
			}
		}
		#endregion
		#region CuryCommnAmt
		public abstract class curyCommnAmt : PX.Data.BQL.BqlDecimal.Field<curyCommnAmt> { }
		protected Decimal? _CuryCommnAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? CuryCommnAmt
		{
			get
			{
				return this._CuryCommnAmt;
			}
			set
			{
				this._CuryCommnAmt = value;
			}
		}
		#endregion
		#region CommnAmt
		public abstract class commnAmt : PX.Data.BQL.BqlDecimal.Field<commnAmt> { }
		protected Decimal? _CommnAmt;
		[PXDBDecimal(4)]
		public virtual Decimal? CommnAmt
		{
			get
			{
				return this._CommnAmt;
			}
			set
			{
				this._CommnAmt = value;
			}
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Line Description")]
		[PXLocalizableDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>),
			typeof(Customer.localeName), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region UnitWeigth
		public abstract class unitWeigth : PX.Data.BQL.BqlDecimal.Field<unitWeigth> { }
		protected Decimal? _UnitWeigth;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseWeight, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
		[PXUIField(DisplayName = "Unit Weight")]
		public virtual Decimal? UnitWeigth
		{
			get
			{
				return this._UnitWeigth;
			}
			set
			{
				this._UnitWeigth = value;
			}
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }
		protected Decimal? _UnitVolume;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseVolume, Where<InventoryItem.inventoryID, Equal<Current<SOLine.inventoryID>>>>))]
		public virtual Decimal? UnitVolume
		{
			get
			{
				return this._UnitVolume;
			}
			set
			{
				this._UnitVolume = value;
			}
		}
		#endregion
		#region ExtWeight
		public abstract class extWeight : PX.Data.BQL.BqlDecimal.Field<extWeight> { }
		protected Decimal? _ExtWeight;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(typeof(Mult<Row<SOLine.baseOrderQty, SOLine.orderQty>, SOLine.unitWeigth>), typeof(SumCalc<SOOrder.orderWeight>))]
		[PXUIField(DisplayName = "Ext. Weight")]
		public virtual Decimal? ExtWeight
		{
			get
			{
				return this._ExtWeight;
			}
			set
			{
				this._ExtWeight = value;
			}
		}
		#endregion
		#region ExtVolume
		public abstract class extVolume : PX.Data.BQL.BqlDecimal.Field<extVolume> { }
		protected Decimal? _ExtVolume;
		[PXDBDecimal(6, MinValue = 0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXFormula(typeof(Mult<Row<SOLine.baseOrderQty, SOLine.orderQty>, SOLine.unitVolume>), typeof(SumCalc<SOOrder.orderVolume>))]
		[PXUIField(DisplayName = "Ext. Volume")]
		public virtual Decimal? ExtVolume
		{
			get
			{
				return this._ExtVolume;
			}
			set
			{
				this._ExtVolume = value;
			}
		}
		#endregion
		#region IsFree
		public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
		protected Boolean? _IsFree;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Free Item")]
		public virtual Boolean? IsFree
		{
			get
			{
				return this._IsFree;
			}
			set
			{
				this._IsFree = value;
			}
		}
		#endregion
		#region CalculateDiscountsOnImport
		public abstract class calculateDiscountsOnImport : PX.Data.BQL.BqlBool.Field<calculateDiscountsOnImport> { }
		protected Boolean? _CalculateDiscountsOnImport;
		[PXBool()]
		[PXUIField(DisplayName = "Calculate automatic discounts on import")]
		public virtual Boolean? CalculateDiscountsOnImport
		{
			get
			{
				return this._CalculateDiscountsOnImport;
			}
			set
			{
				this._CalculateDiscountsOnImport = value;
			}
		}
		#endregion
		#region DiscPct
		public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
		protected Decimal? _DiscPct;
		[PXDBDecimal(6, MinValue = -100, MaxValue=100)]
		[PXUIField(DisplayName = "Discount Percent")]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
		protected Decimal? _CuryDiscAmt;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.discAmt))]
		[PXUIField(DisplayName = "Discount Amount")]
		//[PXFormula(typeof(Div<Mult<Mult<SOLine.orderQty, SOLine.curyUnitPrice>, SOLine.discPct>, decimal100>))]->Causes SetValueExt for CuryDiscAmt 
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }
		protected Decimal? _DiscAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region ManualDisc
		public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
		protected Boolean? _ManualDisc;
		[ManualDiscountMode(typeof(SOLine.curyDiscAmt), typeof(SOLine.discPct), DiscountFeatureType.CustomerDiscount)]
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? ManualDisc
		{
			get
			{
				return this._ManualDisc;
			}
			set
			{
				this._ManualDisc = value;
			}
		}
		#endregion
		#region FreezeManualDisc
		public abstract class freezeManualDisc : PX.Data.BQL.BqlBool.Field<freezeManualDisc> { }
		protected Boolean? _FreezeManualDisc;
		[PXBool()]
		public virtual Boolean? FreezeManualDisc
		{
			get
			{
				return this._FreezeManualDisc;
			}
			set
			{
				this._FreezeManualDisc = value;
			}
		}
		#endregion
		#region SkipDisc
		public abstract class skipDisc : PX.Data.IBqlField
		{
		}
		protected Boolean? _SkipDisc;
		[PXBool()]
		public virtual Boolean? SkipDisc
		{
			get
			{
				return this._SkipDisc;
			}
			set
			{
				this._SkipDisc = value;
			}
		}
		#endregion
		#region CuryLineAmt
		public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
		protected Decimal? _CuryLineAmt;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.lineAmt))]
		[PXUIField(DisplayName = "Amount", Enabled = false)]
		[PXFormula(typeof(Sub<SOLine.curyExtPrice, SOLine.curyDiscAmt>))]
		[PXFormula(null, typeof(CountCalc<SOSalesPerTran.refCntr>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		public abstract class lineAmt : PX.Data.BQL.BqlDecimal.Field<lineAmt> { }
		protected Decimal? _LineAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CuryOpenAmt
		public abstract class curyOpenAmt : PX.Data.BQL.BqlDecimal.Field<curyOpenAmt> { }
		protected Decimal? _CuryOpenAmt;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.openAmt))]
		[PXFormula(typeof(SOLine.openQty.Multiply<SOLine.curyLineAmt.Divide<SOLine.orderQty.When<SOLine.orderQty.IsNotEqual<decimal0>>.Else<decimal1>>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Amount")]
		public virtual Decimal? CuryOpenAmt
		{
			get
			{
				return this._CuryOpenAmt;
			}
			set
			{
				this._CuryOpenAmt = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return this._OpenAmt;
			}
			set
			{
				this._OpenAmt = value;
			}
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt> { }
		protected Decimal? _CuryBilledAmt;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryBilledAmt
		{
			get
			{
				return this._CuryBilledAmt;
			}
			set
			{
				this._CuryBilledAmt = value;
			}
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt> { }
		protected Decimal? _BilledAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BilledAmt
		{
			get
			{
				return this._BilledAmt;
			}
			set
			{
				this._BilledAmt = value;
			}
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		protected Decimal? _CuryUnbilledAmt;
		[PXDBCurrency(typeof(SOLine.curyInfoID), typeof(SOLine.unbilledAmt))]
		[PXFormula(typeof(SOLine.unbilledQty.When<SOLine.orderQty.IsNotEqual<decimal0>>.Else<decimal1>.Multiply<SOLine.curyLineAmt.Divide<SOLine.orderQty.When<SOLine.orderQty.IsNotEqual<decimal0>>.Else<decimal1>>>))]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryUnbilledAmt
		{
			get
			{
				return this._CuryUnbilledAmt;
			}
			set
			{
				this._CuryUnbilledAmt = value;
			}
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		protected Decimal? _UnbilledAmt;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledAmt
		{
			get
			{
				return this._UnbilledAmt;
			}
			set
			{
				this._UnbilledAmt = value;
			}
		}
		#endregion
		#region CuryDiscPrice
		public abstract class curyDiscPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscPrice> { }
		protected Decimal? _CuryDiscPrice;
		[PXDBPriceCostCalced(typeof(Sub<SOLine.curyUnitPrice, SOLine.curyUnitPrice.Multiply<discPct.Divide<decimal100>>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXFormula(typeof(Sub<SOLine.curyUnitPrice, PX.Data.Round<SOLine.curyUnitPrice.Multiply<discPct.Divide<decimal100>>, Current<CommonSetup.decPlPrcCst>>>))]
		[PXCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(SOLine.curyInfoID), typeof(SOLine.discPrice))]
		[PXUIField(DisplayName = "Disc. Unit Price", Enabled = false, Visible = false)]
		public virtual Decimal? CuryDiscPrice
		{
			get
			{
				return this._CuryDiscPrice;
			}
			set
			{
				this._CuryDiscPrice = value;
			}
		}
		#endregion
		#region DiscPrice
		public abstract class discPrice : PX.Data.BQL.BqlDecimal.Field<discPrice> { }
		protected Decimal? _DiscPrice;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? DiscPrice
		{
			get
			{
				return this._DiscPrice;
			}
			set
			{
				this._DiscPrice = value;
			}
		}
		#endregion
		#region DiscountsAppliedToLine
		public abstract class discountAppliedToLine : PX.Data.BQL.BqlString.Field<discountAppliedToLine> { }
		protected ushort[] _DiscountsAppliedToLine;
		[PXDBPackedIntegerArray()]
		public virtual ushort[] DiscountsAppliedToLine
		{
			get
			{
				return this._DiscountsAppliedToLine;
			}
			set
			{
				this._DiscountsAppliedToLine = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
        protected Decimal? _GroupDiscountRate;
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
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
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
        protected Decimal? _DocumentDiscountRate;
        [PXDBDecimal(18)]
        [PXDefault(TypeCode.Decimal, "1.0")]
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
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXQuantity()]
		[PXFormula(typeof(Selector<SOLine.siteID, INItemStats.qtyOnHand>))]
		[PXFormula(typeof(Default<SOLine.inventoryID, SOLine.siteID>))]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected decimal? _TotalCost;
		[PXBaseCury()]
		[PXFormula(typeof(Selector<SOLine.siteID, INItemStats.totalCost>))]
		[PXFormula(typeof(Default<SOLine.inventoryID, SOLine.siteID>))]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;
		[PXPriceCost()]
		[PXUIField(DisplayName = "Average Cost", Enabled = false, Visible = false)]
		[PXFormula(typeof(Div<SOLine.totalCost, NullIf<SOLine.qtyOnHand, decimal0>>))]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
		[PXDefault(typeof(SOOrder.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
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
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID,
			Where<Current<SOLine.tranType>, Equal<INTranType.transfer>, And<ReasonCode.usage, Equal<ReasonCodeUsages.transfer>,
			   Or<Current<SOLine.tranType>, NotEqual<INTranType.transfer>, And<ReasonCode.usage, In3<ReasonCodeUsages.sales, ReasonCodeUsages.issue>>>>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName="Reason Code")]
		[PXForeignReference(typeof(FK.ReasonCode))]
		public virtual String ReasonCode
		{
			get
			{
				return this._ReasonCode;
			}
			set
			{
				this._ReasonCode = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson()]
		[PXParent(typeof(Select<SOSalesPerTran, Where<SOSalesPerTran.orderType, Equal<Current<SOLine.orderType>>, And<SOSalesPerTran.orderNbr, Equal<Current<SOLine.orderNbr>>, And<SOSalesPerTran.salespersonID, Equal<Current2<SOLine.salesPersonID>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(typeof(SOOrder.salesPersonID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<SOLine.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual Int32? SalesPersonID
		{
			get
			{
				return this._SalesPersonID;
			}
			set
			{
				this._SalesPersonID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(typeof(SOLine.branchID),Visible = false)]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;
		[PXFormula(typeof(Default<SOLine.branchID>))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(SOLine.salesAcctID), typeof(SOLine.branchID), Visible = false)]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Coalesce<Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<SOLine.projectID>>, And<PMAccountTask.accountID, Equal<Current<SOLine.salesAcctID>>>>>,
			Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveOrInPlanningProjectTask(typeof(SOLine.projectID), BatchModule.SO, DisplayName = "Project Task")]
		[PXForeignReference(typeof(Field<taskID>.IsRelatedTo<PMTask.taskID>))]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(typeof(salesAcctID), typeof(taskID), GL.AccountType.Income, DescriptionField = typeof(PMCostCode.description))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
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
		#region CommitmentID
		public abstract class commitmentID : PX.Data.BQL.BqlGuid.Field<commitmentID> { }
		protected Guid? _CommitmentID;
		[PXDBGuid]
		public virtual Guid? CommitmentID
		{
			get
			{
				return this._CommitmentID;
			}
			set
			{
				this._CommitmentID = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion
		#region Commissionable
		public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }
		protected bool? _Commissionable;
		[PXDBBool()]
		[PXFormula(typeof(Switch<Case<Where<SOLine.inventoryID, IsNotNull>, Selector<SOLine.inventoryID, InventoryItem.commisionable>>, True>))]
		[PXUIField(DisplayName = "Commissionable")]
		public bool? Commissionable
		{
			get
			{
				return _Commissionable;
			}
			set
			{
				_Commissionable = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
		#region AutoCreateIssueLine
		public abstract class autoCreateIssueLine : PX.Data.BQL.BqlBool.Field<autoCreateIssueLine> { }
		protected bool? _AutoCreateIssueLine;
		[PXDBBool()]
		[PXFormula(typeof(Selector<SOLine.operation, SOOrderTypeOperation.autoCreateIssueLine>))]
		[PXUIField(DisplayName = "Auto Create Issue", Visibility = PXUIVisibility.Dynamic)]
		public virtual bool? AutoCreateIssueLine
		{
			get
			{
				return this._AutoCreateIssueLine;
			}
			set
			{
				this._AutoCreateIssueLine = value;
			}
		}
		#endregion
		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		protected DateTime? _ExpireDate;
		[INExpireDate(typeof(SOLine.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual DateTime? ExpireDate
		{
			get
			{
				return this._ExpireDate;
			}
			set
			{
				this._ExpireDate = value;
			}
		}
		#endregion

        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
        [PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = true)]
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
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
        protected String _DiscountSequenceID;
        [PXDBString(10, IsUnicode = true)]
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
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		[PXDBBool()]
		[PXDefault(false, 
			typeof(Search<INItemSiteSettings.pOCreate,
							Where<INItemSiteSettings.inventoryID, Equal<Current<SOLine.inventoryID>>,
								 And<INItemSiteSettings.siteID, Equal<Current<SOLine.siteID>>>>>))]
		[PXUIField(DisplayName = "Mark for PO")]
		public virtual Boolean? POCreate
		{
			get;
			set;
		}
		#endregion
		#region POSource
		public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
		protected string _POSource;
		[PXDBString()]
		[PXDefault(INReplenishmentSource.PurchaseToOrder, PersistingCheck = PXPersistingCheck.Nothing)]
		[INReplenishmentSource.SOList]
		[PXUIField(DisplayName = "PO Source", Enabled = false)]
		public virtual string POSource
		{
			get
			{
				return this._POSource;
			}
			set
			{
				this._POSource = value;
			}
		}
		#endregion
		#region POCreated
		public abstract class pOCreated : PX.Data.BQL.BqlBool.Field<pOCreated> { }
		protected Boolean? _POCreated;
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? POCreated
		{
			get
			{
				return this._POCreated;
			}
			set
			{
				this._POCreated = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[AP.Vendor(typeof(Search<BAccountR.bAccountID,
			Where<AP.Vendor.type, NotEqual<BAccountType.employeeType>>>))]
		[PXRestrictor(typeof(Where<AP.Vendor.status, IsNull,
								Or<AP.Vendor.status, Equal<BAccount.status.active>,
								Or<AP.Vendor.status, Equal<BAccount.status.oneTime>>>>), AP.Messages.VendorIsInStatus, typeof(AP.Vendor.status))]
		[PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
			Where<INItemSiteSettings.inventoryID, Equal<Current<SOLine.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<SOLine.siteID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<SOLine.siteID>))]
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
        #region POSiteID
        public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }
        protected Int32? _POSiteID;
        [Site(DisplayName = "Purchase Warehouse")]
		[PXForeignReference(typeof(FK.POSite))]
		public virtual Int32? POSiteID
        {
            get
            {
                return this._POSiteID;
            }
            set
            {
                this._POSiteID = value;
            }
        }
        #endregion
		#region DRTermStartDate
		public abstract class dRTermStartDate : PX.Data.BQL.BqlDateTime.Field<dRTermStartDate> { }

		protected DateTime? _DRTermStartDate;

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term Start Date")]
		public DateTime? DRTermStartDate
		{
			get { return _DRTermStartDate; }
			set { _DRTermStartDate = value; }
		}
		#endregion
		#region DRTermEndDate
		public abstract class dRTermEndDate : PX.Data.BQL.BqlDateTime.Field<dRTermEndDate> { }

		protected DateTime? _DRTermEndDate;

		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Term End Date")]
		public DateTime? DRTermEndDate
		{
			get { return _DRTermEndDate; }
			set { _DRTermEndDate = value; }
		}
		#endregion
		#region ItemRequiresTerms
		public abstract class itemRequiresTerms : PX.Data.BQL.BqlBool.Field<itemRequiresTerms> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the <see cref="DRTermStartDate"/> and <see cref="DRTermEndDate"/>
		/// fields are enabled for the line.
		/// </summary>
		/// <value>
		/// The value of this field is set by the <see cref="SOOrderEntry"/> graph based on the settings of the
		/// <see cref="InventoryID">item</see> selected for the line. In other contexts it is not populated.
		/// See the attribute on the <see cref="SOOrderEntry.SOLine_ItemRequiresTerms_CacheAttached"/> handler for details.
		/// </value>
		[PXBool]
		public virtual bool? ItemRequiresTerms
		{
			get;
			set;
		}
		#endregion
		#region ItemHasResidual
		public abstract class itemHasResidual : PX.Data.BQL.BqlBool.Field<itemHasResidual> { }

		[PXBool]
		[DR.DRTerms.VerifyResidual(typeof(inventoryID), null, typeof(curyUnitPriceDR), typeof(curyExtPrice))]
		public virtual bool? ItemHasResidual
		{
			get;
			set;
		}
		#endregion
		#region CuryUnitPriceDR
		public abstract class curyUnitPriceDR : PX.Data.BQL.BqlDecimal.Field<curyUnitPriceDR> { }

		protected decimal? _CuryUnitPriceDR;

		[PXUIField(DisplayName = "Unit Price for DR", Visible = false)]
		[PXDBDecimal(typeof(Search<CommonSetup.decPlPrcCst>))]
		public virtual decimal? CuryUnitPriceDR
		{
			get { return _CuryUnitPriceDR; }
			set { _CuryUnitPriceDR = value; }
		}
		#endregion
		#region LineDiscountDR
		public abstract class discPctDR : PX.Data.BQL.BqlDecimal.Field<discPctDR> { }

		protected decimal? _DiscPctDR;

		[PXUIField(DisplayName = "Discount Percent for DR", Visible = false)]
		[PXDBDecimal(6, MinValue = -100, MaxValue = 100)]
		public virtual decimal? DiscPctDR
		{
			get { return _DiscPctDR; }
			set { _DiscPctDR = value; }
		}
		#endregion
		#region DefScheduleID
		public abstract class defScheduleID : PX.Data.BQL.BqlInt.Field<defScheduleID> { }
		protected int? _DefScheduleID;
		[PXDBInt]
		public virtual int? DefScheduleID
		{
			get
			{
				return this._DefScheduleID;
			}
			set
			{
				this._DefScheduleID = value;
			}
		}
		#endregion
		#region HasMixedProjectTasks
		public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }
		protected bool? _HasMixedProjectTasks;
		/// <summary>
		/// Returns true if the splits associated with the line has mixed ProjectTask values.
		/// This field is used to validate the record on persist. 
		/// </summary>
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? HasMixedProjectTasks
		{
			get
			{
				return _HasMixedProjectTasks;
			}
			set
			{
				_HasMixedProjectTasks = value;
			}
		}
		#endregion
		#region IsCut
		public abstract class isCut : PX.Data.BQL.BqlBool.Field<isCut> { }
		protected bool? _IsCut;
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? IsCut
		{
			get
			{
				return _IsCut;
			}
			set
			{
				_IsCut = value;
			}
		}
		#endregion

		#region Methods
		public static implicit operator SOLineSplit(SOLine item)
		{
			SOLineSplit ret = new SOLineSplit();
			ret.OrderType = item.OrderType;
			ret.OrderNbr = item.OrderNbr;
			ret.LineNbr = item.LineNbr;
			ret.Operation = item.Operation;
			ret.SplitLineNbr = 1;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.ExpireDate = item.ExpireDate;
			ret.Qty = item.Qty;
			ret.UOM = item.UOM;
			ret.OrderDate = item.OrderDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.PlanType = item.PlanType;
			//check for ordered qty not to get problems in LSSelect_Detail_RowInserting which will retain Released = true flag while merging LSDetail
			ret.Completed = (item.RequireShipping == true && item.OrderQty > 0m && item.OpenQty == 0m || item.Completed == true);
			ret.ShipDate = item.ShipDate;
			ret.RequireAllocation = item.RequireAllocation;
			ret.RequireLocation = item.RequireLocation;
			ret.RequireShipping = item.RequireShipping;

			return ret;
		}
		public static implicit operator SOLine(SOLineSplit item)
		{
			SOLine ret = new SOLine();
			ret.OrderType = item.OrderType;
			ret.OrderNbr = item.OrderNbr;
			ret.LineNbr = item.LineNbr;
			ret.Operation = item.Operation;
			ret.InventoryID = item.InventoryID;
			ret.SiteID = item.SiteID;
			ret.SubItemID = item.SubItemID;
			ret.LocationID = item.LocationID;
			ret.LotSerialNbr = item.LotSerialNbr;
			ret.Qty = item.Qty;
			ret.OpenQty = item.Qty;
			ret.BaseOpenQty = item.BaseQty;
			ret.UOM = item.UOM;
			ret.OrderDate = item.OrderDate;
			ret.BaseQty = item.BaseQty;
			ret.InvtMult = item.InvtMult;
			ret.PlanType = item.PlanType;
			ret.ShipDate = item.ShipDate;
			ret.RequireAllocation = item.RequireAllocation;
			ret.RequireLocation = item.RequireLocation;
			ret.RequireShipping = item.RequireShipping;
			return ret;
		}
		#endregion
	}

	public class SOLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Inventory, Messages.Inventory),
					Pair(NonInventory, Messages.NonInventory),
					Pair(MiscCharge, Messages.MiscCharge),
				}) {}
		}

		public const string Inventory = "GI";
		public const string NonInventory = "GN";
		public const string MiscCharge = "MI";
		public const string Freight = "FR";
		public const string Discount = "DS";
		public const string Reallocation = "RA";

		public class inventory : PX.Data.BQL.BqlString.Constant<inventory>
		{
			public inventory() : base(Inventory) { ;}
		}

		public class nonInventory : PX.Data.BQL.BqlString.Constant<nonInventory>
		{
			public nonInventory() : base(NonInventory) { ;}
		}

		public class miscCharge : PX.Data.BQL.BqlString.Constant<miscCharge>
		{
			public miscCharge() : base(MiscCharge) {}
		}

		public class freight : PX.Data.BQL.BqlString.Constant<freight>
		{
			public freight() : base(Freight) { ;}
		}

		public class discount : PX.Data.BQL.BqlString.Constant<discount>
		{
			public discount() : base(Discount) { ; }
		}

		public class reallocation : PX.Data.BQL.BqlString.Constant<reallocation>
		{
			public reallocation() : base(Reallocation) { ; }
		}
	}

	public class SOShipComplete
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(ShipComplete, Messages.ShipComplete),
					Pair(BackOrderAllowed, Messages.BackOrderAllowed),
					Pair(CancelRemainder, Messages.CancelRemainder),
				}) {}
		}

		public const string ShipComplete = "C";
		public const string BackOrderAllowed = "B";
		public const string CancelRemainder = "L";

		public class shipComplete : PX.Data.BQL.BqlString.Constant<shipComplete>
		{
			public shipComplete() : base(ShipComplete) { ;}
		}

		public class backOrderAllowed : PX.Data.BQL.BqlString.Constant<backOrderAllowed>
		{
			public backOrderAllowed() : base(BackOrderAllowed) { ;}
		}

		public class cancelRemainder : PX.Data.BQL.BqlString.Constant<cancelRemainder>
		{
			public cancelRemainder() : base(CancelRemainder) { ;}
		}
	}
}