using System;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.Common;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Bql;
using PX.Objects.IN.Matrix.Interfaces;

namespace PX.Objects.PO
{
	[System.SerializableAttribute()]
    [PXCacheName(Messages.POLineShort)]
	public partial class POLine : PX.Data.IBqlTable, IItemPlanMaster, ISortOrder, ITaxableDetail, IMatrixItemLine
	{
		#region Keys
		public class PK : PrimaryKeyOf<POLine>.By<orderType, orderNbr, lineNbr>
		{
			public static POLine Find(PXGraph graph, string orderType, string orderNbr, int? lineNbr) => FindBy(graph, orderType, orderNbr, lineNbr);
		}
		public static class FK
		{
			public class Order : POOrder.PK.ForeignKeyOf<POLine>.By<orderType, orderNbr> { }
			public class BlanketOrder : POOrder.PK.ForeignKeyOf<POLine>.By<pOType, pONbr> { }
			public class BlanketLine : POLine.PK.ForeignKeyOf<POLine>.By<pOType, pONbr, pOLineNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<POLine>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<POLine>.By<siteID> { }
			public class RQRequisition : RQ.RQRequisition.PK.ForeignKeyOf<POLine>.By<rQReqNbr> { }
			public class RQRequisitionLine : RQ.RQRequisitionLine.PK.ForeignKeyOf<POLine>.By<rQReqNbr, rQReqLineNbr> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
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
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(typeof(POOrder.branchID))]
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
		[PXDBDefault(typeof(POOrder.orderType))]
		[PXUIField(DisplayName = "Order Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		[PXDBDefault(typeof(POOrder.orderNbr))]
		[PXParent(typeof(FK.Order))]
		[PXUIField(DisplayName = "Order Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
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
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(POOrder.lineCntr))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
			public class InventoryBaseUnitRule :
				InventoryItem.baseUnit.PreventEditIfExists<
					Select<POLine,
					Where<inventoryID, Equal<Current<InventoryItem.inventoryID>>,
						And2<Where2<POLineType.Goods.Contains<lineType>, Or<POLineType.NonStocks.Contains<lineType>>>,
						And<completed, NotEqual<True>>>>>>
			{ }
		}
		protected Int32? _InventoryID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[POLineInventoryItem(Filterable = true)]
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
		[PXDefault(POLineType.Service)]
		[PXDBString(2, IsFixed = true)]
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
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost> { }
		protected Boolean? _AccrueCost;
		/// <summary>
		/// When set to <c>true</c>, indicates that cost will be processed using expense accrual account.
		/// </summary>
		[PXDBBool()]
		[PXDefault(typeof(IsNull<Selector<POLine.inventoryID, InventoryItem.accrueCost>, False>))]
		[PXUIField(DisplayName = "Accrue Cost", Enabled = false, Visible = false)]
		public virtual Boolean? AccrueCost
		{
			get
			{
				return this._AccrueCost;
			}
			set
			{
				this._AccrueCost = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong(IsImmutable = true)]
		[PXUIField(DisplayName = "Plan ID", Visible = false, Enabled = false)]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion
		#region ClearPlanID
		public abstract class clearPlanID : BqlBool.Field<clearPlanID> { }
		[PXBool]
		public virtual bool? ClearPlanID
		{
			get;
			set;
		}
		#endregion

		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true)]
		[POOrderType.List()]
		[PXUIField(DisplayName = "Blanket PO Type", Enabled = false)]
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
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		protected String _PONbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Blanket PO Nbr.", Enabled = false)]
		[PO.RefNbr(typeof(Search2<POOrder.orderNbr,
			InnerJoinSingleTable<Vendor, On<POOrder.vendorID, Equal<Vendor.bAccountID>>>,
			 Where<POOrder.orderType, Equal<Current<POLine.pOType>>,
			 And<Match<Vendor, Current<AccessInfo.userName>>>>,
			OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
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
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
		protected Int32? _POLineNbr;
		[PXDBInt()]
		[PXParent(typeof(Select<POLineR, 
			Where<POLineR.orderType, Equal<Current<POLine.pOType>>,
										And<POLineR.orderType,Equal<POOrderType.blanket>,
										And<POLineR.orderNbr, Equal<Current<POLine.pONbr>>,
										And<POLineR.lineNbr, Equal<Current<POLine.pOLineNbr>>>>>>>))]
		[PXUIField(DisplayName = "Blanket PO Line Nbr.", Enabled = false)]
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

		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[POVendor()]
		[PXDBDefault(typeof(POOrder.vendorID))]
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
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;
		[PXInt()]
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
		#region ShipToBAccountID
		public abstract class shipToBAccountID : PX.Data.BQL.BqlInt.Field<shipToBAccountID> { }
		protected Int32? _ShipToBAccountID;
		[PXInt()]
		public virtual Int32? ShipToBAccountID
		{
			get
			{
				return this._ShipToBAccountID;
			}
			set
			{
				this._ShipToBAccountID = value;
			}
		}
		#endregion
		#region ShipToLocationID
		public abstract class shipToLocationID : PX.Data.BQL.BqlInt.Field<shipToLocationID> { }
		protected Int32? _ShipToLocationID;
		[PXInt()]
		public virtual Int32? ShipToLocationID
		{
			get
			{
				return this._ShipToLocationID;
			}
			set
			{
				this._ShipToLocationID = value;
			}
		}
		#endregion
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate()]
		[PXDBDefault(typeof(POOrder.orderDate))]
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
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[SubItem(typeof(POLine.inventoryID))]
		//[SubItemStatusVeryfier(typeof(POLine.inventoryID), typeof(POLine.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoPurchases)]
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
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		[POSiteAvail(typeof(POLine.inventoryID), typeof(POLine.subItemID))]
		[PXDefault(typeof(Coalesce<
				Search<POOrder.siteID,
					Where<POOrder.orderType, Equal<Current<POOrder.orderType>>,
						And<POOrder.orderNbr, Equal<Current<POOrder.orderNbr>>,
						And<POOrder.shipDestType, Equal<POShippingDestination.site>>>>>,
				Search<Location.vSiteID,
					Where<Location.locationID, Equal<Current<POOrder.vendorLocationID>>,
						And<Location.bAccountID, Equal<Current<POOrder.vendorID>>>>>,
				Search<InventoryItem.dfltSiteID,Where<InventoryItem.inventoryID,Equal<Current<POLine.inventoryID>>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(FK.Site))]
		[InterBranchRestrictor(typeof(Where2<SameOrganizationBranch<INSite.branchID, Current<POOrder.branchID>>,
			Or<Current<POOrder.orderType>, Equal<POOrderType.standardBlanket>>>))]
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
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Lot Serial Number",Visible=false )]
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

		#region BLType
		public abstract class bLType : PX.Data.BQL.BqlString.Field<bLType> { }
		protected String _BLType;
		[PXDBString(2, IsFixed = true)]
		public virtual String BLType
		{
			get
			{
				return this._BLType;
			}
			set
			{
				this._BLType = value;
			}
		}
		#endregion
		#region BLOrderNbr
		public abstract class bLOrderNbr : PX.Data.BQL.BqlString.Field<bLOrderNbr> { }
		protected String _BLOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String BLOrderNbr
		{
			get
			{
				return this._BLOrderNbr;
			}
			set
			{
				this._BLOrderNbr = value;
			}
		}
		#endregion
		#region BLLineNbr
		public abstract class bLLineNbr : PX.Data.BQL.BqlInt.Field<bLLineNbr> { }
		protected Int32? _BLLineNbr;
		[PXDBInt()]
		public virtual Int32? BLLineNbr
		{
			get
			{
				return this._BLLineNbr;
			}
			set
			{
				this._BLLineNbr = value;
			}
		}
		#endregion

		#region RQReqNbr
		public abstract class rQReqNbr : PX.Data.BQL.BqlString.Field<rQReqNbr> { }
		protected String _RQReqNbr;
		[PXDBString(15, IsUnicode = true)]
		public virtual String RQReqNbr
		{
			get
			{
				return this._RQReqNbr;
			}
			set
			{
				this._RQReqNbr = value;
			}
		}
		#endregion
		#region RQReqLineNbr
		public abstract class rQReqLineNbr : PX.Data.BQL.BqlInt.Field<rQReqLineNbr> { }
		protected Int32? _RQReqLineNbr;
		[PXDBInt()]
		public virtual Int32? RQReqLineNbr
		{
			get
			{
				return this._RQReqLineNbr;
			}
			set
			{
				this._RQReqLineNbr = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
        
		
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(POLine.inventoryID),DisplayName="UOM")]
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
		public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }
		protected Decimal? _OrderQty;
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseOrderQty), InventoryUnitType.PurchaseUnit, HandleEmptyKey = true, MinValue = 0, ConvertToDecimalVerifyUnits = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<POOrder.orderQty>))] //		
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
		#region OrigOrderQty
		public abstract class origOrderQty : PX.Data.BQL.BqlDecimal.Field<origOrderQty> { }
		[PXDBDecimal]
		public virtual Decimal? OrigOrderQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOrderQty
		public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }
		protected Decimal? _BaseOrderQty;
		[PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(null, typeof(SumCalc<POLineR.baseReceivedQty>))] //
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
        #region ReceivedQty
        public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
		protected Decimal? _ReceivedQty;
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseReceivedQty),HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Receipts", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? ReceivedQty
		{
			get
			{
				return this._ReceivedQty;
			}
			set
			{
				this._ReceivedQty = value;
			}
		}
		#endregion
		#region BaseReceivedQty
		public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
		protected Decimal? _BaseReceivedQty;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Base Received Qty.", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? BaseReceivedQty
		{
			get
			{
				return this._BaseReceivedQty;
			}
			set
			{
				this._BaseReceivedQty = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		
		[PXDBLong()]
		[CurrencyInfo(typeof(POOrder.curyInfoID))]
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
		public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }
		protected Decimal? _CuryUnitCost;

		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(POLine.curyInfoID), typeof(POLine.unitCost))]
		[PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
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
		[PXDefault(typeof(Search<INItemCost.lastCost, Where<INItemCost.inventoryID, Equal<Current<POLine.inventoryID>>>>))]
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
        [PXDBDecimal(6, MinValue = -100, MaxValue = 100)]
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
        [PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.discAmt))]
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
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        #region ManualPrice
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
        protected Boolean? _ManualPrice;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Cost", Visible = false)]
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
        #region ManualDisc
        public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
        protected Boolean? _ManualDisc;
        [ManualDiscountMode(typeof(POLine.curyDiscAmt), typeof(POLine.curyExtCost), typeof(POLine.discPct), DiscountFeatureType.VendorDiscount)]
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
        #region CuryLineAmt
        public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }
        protected Decimal? _CuryLineAmt;
        [PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.lineAmt))]
        [PXUIField(DisplayName = "Ext. Cost")]
        [PXFormula(typeof(Mult<POLine.orderQty, POLine.curyUnitCost>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIVerify(typeof(Where<curyLineAmt, GreaterEqual<decimal0>, Or<Not<POLineType.Goods.Contains<lineType>>>>), PXErrorLevel.Error, CS.Messages.FieldShouldNotBeNegative, typeof(curyLineAmt), MessageArgumentsAreFieldNames = true, CheckOnRowSelected = true)]
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
        #region CuryDiscCost
        public abstract class curyDiscCost : PX.Data.BQL.BqlDecimal.Field<curyDiscCost> { }
        protected Decimal? _CuryDiscCost;
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<POLine.discPct, NotEqual<decimal0>>, Mult<POLine.curyUnitCost, Sub<decimal1, Div<discPct, decimal100>>>,
										   Case<Where<POLine.orderQty, Equal<decimal0>>, decimal0,
										   Case<Where<POLine.manualPrice, Equal<False>>, POLine.curyUnitCost>>>,
										   Div<Sub<POLine.curyLineAmt, POLine.curyDiscAmt>, POLine.orderQty>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXFormula(typeof(Switch<Case<Where<POLine.discPct, NotEqual<decimal0>>, Mult<POLine.curyUnitCost, Sub<decimal1, Div<discPct, decimal100>>>,
								 Case<Where<POLine.manualPrice, Equal<False>>, POLine.curyUnitCost>>,
								 Div<Sub<POLine.curyLineAmt, POLine.curyDiscAmt>, NullIf<POLine.orderQty, decimal0>>>))]
		[PXCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(POLine.curyInfoID), typeof(POLine.discCost))]
        [PXUIField(DisplayName = "Disc. Unit Cost", Enabled = false, Visible = false)]
        public virtual Decimal? CuryDiscCost
        {
            get
            {
                return this._CuryDiscCost;
            }
            set
            {
                this._CuryDiscCost = value;
            }
        }
        #endregion
        #region DiscCost
        public abstract class discCost : PX.Data.BQL.BqlDecimal.Field<discCost> { }
        protected Decimal? _DiscCost;
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<POLine.orderQty, Equal<decimal0>>, decimal0>, Div<POLine.lineAmt, POLine.orderQty>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
        [PXFormula(typeof(Div<Row<POLine.lineAmt, POLine.curyLineAmt>, NullIf<POLine.orderQty, decimal0>>))]
        public virtual Decimal? DiscCost
        {
            get
            {
                return this._DiscCost;
            }
            set
            {
                this._DiscCost = value;
            }
        }
        #endregion

		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		[DBRetainagePercent(
			typeof(POOrder.retainageApply),
			typeof(POOrder.defRetainagePct),
			typeof(Sub<Current<POLine.curyLineAmt>, Current<POLine.curyDiscAmt>>),
			typeof(POLine.curyRetainageAmt),
			typeof(POLine.retainagePct))]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageAmt
		public abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt> { }
		[DBRetainageAmount(
			typeof(POLine.curyInfoID),
			typeof(Sub<POLine.curyLineAmt, POLine.curyDiscAmt>),
			typeof(POLine.curyRetainageAmt),
			typeof(POLine.retainageAmt),
			typeof(POLine.retainagePct))]
		public virtual decimal? CuryRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmt
		public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? RetainageAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryExtCost
		public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }
		protected Decimal? _CuryExtCost;
		[PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.extCost),MinValue=0.0)]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXFormula(typeof(Sub<POLine.curyLineAmt, Add<POLine.curyDiscAmt, POLine.curyRetainageAmt>>))]
		[PXFormula(null, typeof(SumCalc<POLineR.curyBLOrderedCost>))]
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

		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region OrigExtCost
		public abstract class origExtCost : PX.Data.BQL.BqlDecimal.Field<origExtCost> { }
		
		[PXDBDecimal]
		public virtual Decimal? OrigExtCost
		{
			get;
			set;
		}
		#endregion
		#region DiscountsAppliedToLine
		public abstract class discountsAppliedToLine : PX.Data.BQL.BqlByteArray.Field<discountsAppliedToLine> { }
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[POTax(typeof(POOrder), typeof(POTax), typeof(POTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(POLine.inventoryID), UOM = typeof(POLine.uOM), LineQty = typeof(POLine.orderQty))]
		[POUnbilledTax(typeof(POOrder), typeof(POTax), typeof(POTaxTran),
			   //Per Unit Tax settings
			   Inventory = typeof(POLine.inventoryID), UOM = typeof(POLine.uOM), LineQty = typeof(POLine.unbilledQty))]
		[PORetainedTax(typeof(POOrder), typeof(POTax), typeof(POTaxTran))]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
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
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected String _TaxID;
		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
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
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[Account(typeof(POLine.branchID),DisplayName = "Account", Visibility = PXUIVisibility.Visible, Filterable = false, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[POProjectDefault(typeof(POLine.lineType), AccountType = typeof(POLine.expenseAcctID))]
		[PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
		[PXRestrictor(typeof(Where<PMProject.visibleInPO, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
		[ProjectBaseAttribute()]
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
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveOrInPlanningProjectTaskAttribute(typeof(POLine.projectID), BatchModule.PO, DisplayName = "Project Task")]
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
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		[SubAccount(typeof(POLine.expenseAcctID), typeof(POLine.branchID), DisplayName = "Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
		[Account(typeof(POLine.branchID), DescriptionField = typeof(Account.description), DisplayName = "Accrual Account", Filterable = false, ControlAccountForModule = ControlAccountModule.PO)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? POAccrualAcctID
		{
			get;
			set;
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }
		[SubAccount(typeof(POLine.pOAccrualAcctID), typeof(POLine.branchID), DisplayName = "Accrual Sub.", Filterable = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? POAccrualSubID
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;		
		[AlternativeItem(INPrimaryAlternateType.VPN, typeof(vendorID), typeof(inventoryID), typeof(subItemID), typeof(uOM))]
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
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
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
		#region UnitWeight
		public abstract class unitWeight : PX.Data.BQL.BqlDecimal.Field<unitWeight> { }
		protected Decimal? _UnitWeight;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseWeight, Where<InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>, And<InventoryItem.baseWeight, IsNotNull>>>))]
		[PXUIField(DisplayName="Unit Weight")]
		public virtual Decimal? UnitWeight
		{
			get
			{
				return this._UnitWeight;
			}
			set
			{
				this._UnitWeight = value;
			}
		}
		#endregion
		#region UnitVolume
		public abstract class unitVolume : PX.Data.BQL.BqlDecimal.Field<unitVolume> { }
		protected Decimal? _UnitVolume;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.baseVolume, Where<InventoryItem.inventoryID, Equal<Current<POLine.inventoryID>>, And<InventoryItem.baseVolume, IsNotNull>>>))]
		[PXUIField(DisplayName = "Unit Volume")]
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
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<baseOrderQty>.WithDependencies<uOM, orderQty>, unitWeight>), typeof(SumCalc<POOrder.orderWeight>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Weight", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Mult<Row<baseOrderQty>.WithDependencies<uOM, orderQty>, unitVolume>), typeof(SumCalc<POOrder.orderVolume>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(typeof(expenseAcctID), typeof(taskID), GL.AccountType.Expense, DescriptionField = typeof(PMCostCode.description))]
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
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;		
		[PXNote]
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
		#region RcptQtyMin
		public abstract class rcptQtyMin : PX.Data.BQL.BqlDecimal.Field<rcptQtyMin> { }
		protected Decimal? _RcptQtyMin;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(typeof(Search<Location.vRcptQtyMin, 
			Where<Location.locationID,Equal<Current<POOrder.vendorLocationID>>, 
			  And<Location.bAccountID, Equal<Current<POOrder.vendorID>>>>>))]
		[PXUIField(DisplayName = "Min. Receipt (%)")]
		public virtual Decimal? RcptQtyMin
		{
			get
			{
				return this._RcptQtyMin;
			}
			set
			{
				this._RcptQtyMin = value;
			}
		}
		#endregion
		#region RcptQtyMax
		public abstract class rcptQtyMax : PX.Data.BQL.BqlDecimal.Field<rcptQtyMax> { }
		protected Decimal? _RcptQtyMax;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(typeof(Search<Location.vRcptQtyMax, 
			Where<Location.locationID,Equal<Current<POOrder.vendorLocationID>>, 
			  And<Location.bAccountID, Equal<Current<POOrder.vendorID>>>>>))]
		[PXUIField(DisplayName = "Max. Receipt (%)")]
		public virtual Decimal? RcptQtyMax
		{
			get
			{
				return this._RcptQtyMax;
			}
			set
			{
				this._RcptQtyMax = value;
			}
		}
		#endregion
		#region RcptQtyThreshold
		public abstract class rcptQtyThreshold : PX.Data.BQL.BqlDecimal.Field<rcptQtyThreshold> { }
		protected Decimal? _RcptQtyThreshold;
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 999.0)]
		[PXDefault(typeof(Search<Location.vRcptQtyThreshold,
			Where<Location.locationID, Equal<Current<POOrder.vendorLocationID>>,
				And<Location.bAccountID, Equal<Current<POOrder.vendorID>>>>>))]
		[PXUIField(DisplayName = "Complete On (%)")]
		public virtual Decimal? RcptQtyThreshold
		{
			get
			{
				return this._RcptQtyThreshold;
			}
			set
			{
				this._RcptQtyThreshold = value;
			}
		}
		#endregion
		#region RcptQtyAction
		public abstract class rcptQtyAction : PX.Data.BQL.BqlString.Field<rcptQtyAction> { }
		protected String _RcptQtyAction;
		[PXDBString(1, IsFixed = true)]		
		[POReceiptQtyAction.List()]
		[PXDefault(typeof(Search<Location.vRcptQtyAction, 
			Where<Location.locationID,Equal<Current<POOrder.vendorLocationID>>, 
			  And<Location.bAccountID, Equal<Current<POOrder.vendorID>>>>>))]
		[PXUIField(DisplayName="Receipt Action")]
		public virtual String RcptQtyAction
		{
			get
			{
				return this._RcptQtyAction;
			}
			set
			{
				this._RcptQtyAction = value;
			}
		}
		#endregion
		#region ReceiptStatus
		public abstract class receiptStatus : PX.Data.BQL.BqlString.Field<receiptStatus> { }
		protected String _ReceiptStatus;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("")]
		public virtual String ReceiptStatus
		{
			get
			{
				return this._ReceiptStatus;
			}
			set
			{
				this._ReceiptStatus = value;
			}
		}
		#endregion
		#region CuryBLOrderedCost
		public abstract class curyBLOrderedCost : PX.Data.BQL.BqlDecimal.Field<curyBLOrderedCost> { }
		[PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.bLOrderedCost))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Received Cost", Visible = false)]
		public virtual decimal? CuryBLOrderedCost
		{
			get;
			set;
		}
		#endregion
		#region BLOrderedCost
		public abstract class bLOrderedCost : PX.Data.BQL.BqlDecimal.Field<bLOrderedCost> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BLOrderedCost
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp(RecordComesFirst = true)]
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

		#region CompletedQty
		public abstract class completedQty : PX.Data.BQL.BqlDecimal.Field<completedQty> { }
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseCompletedQty), HandleEmptyKey = true, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Received Qty.", Enabled = false)]
		public virtual decimal? CompletedQty
		{
			get;
			set;
		}
		#endregion
		#region BaseCompletedQty
		public abstract class baseCompletedQty : PX.Data.BQL.BqlDecimal.Field<baseCompletedQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseCompletedQty
		{
			get;
			set;
		}
		#endregion
		#region BilledQty
		public abstract class billedQty : PX.Data.BQL.BqlDecimal.Field<billedQty> { }
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseBilledQty), HandleEmptyKey = true, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Qty.", Enabled = false)]
		public virtual decimal? BilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseBilledQty
		public abstract class baseBilledQty : PX.Data.BQL.BqlDecimal.Field<baseBilledQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseBilledQty
		{
			get;
			set;
		}
		#endregion
		#region CuryBilledAmt
		public abstract class curyBilledAmt : PX.Data.BQL.BqlDecimal.Field<curyBilledAmt> { }
		[PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.billedAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Amount", Enabled = false)]
		public virtual decimal? CuryBilledAmt
		{
			get;
			set;
		}
		#endregion
		#region BilledAmt
		public abstract class billedAmt : PX.Data.BQL.BqlDecimal.Field<billedAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BilledAmt
		{
			get;
			set;
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseOpenQty), HandleEmptyKey = true)]
		[PXFormula(typeof(Switch<Case<Where<POLine.completed, Equal<True>, Or<POLine.cancelled, Equal<True>>>, decimal0>,
			Maximum<Sub<POLine.orderQty, POLine.completedQty>, decimal0>>),
			typeof(SumCalc<POOrder.openOrderQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Qty.", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOpenQty
		public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseOpenQty
		{
			get;
			set;
		}
		#endregion
		#region UnbilledQty
		public abstract class unbilledQty : PX.Data.BQL.BqlDecimal.Field<unbilledQty> { }
		[PXDBQuantity(typeof(POLine.uOM), typeof(POLine.baseUnbilledQty), HandleEmptyKey = true)]
		[PXFormula(typeof(Switch<Case<Where<POLine.closed, Equal<True>, Or<POLine.cancelled, Equal<True>>>, decimal0>,
			Maximum<Sub<Maximum<POLine.orderQty, POLine.completedQty>, POLine.billedQty>, decimal0>>),
			typeof(SumCalc<POOrder.unbilledOrderQty>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Qty.", Enabled = false)]
		public virtual Decimal? UnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region BaseUnbilledQty
		public abstract class baseUnbilledQty : PX.Data.BQL.BqlDecimal.Field<baseUnbilledQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseUnbilledQty
		{
			get;
			set;
		}
		#endregion
		#region CuryUnbilledAmt
		public abstract class curyUnbilledAmt : PX.Data.BQL.BqlDecimal.Field<curyUnbilledAmt> { }
		[PXDBCurrency(typeof(POLine.curyInfoID), typeof(POLine.unbilledAmt))]
		[PXFormula(typeof(Switch<Case<Where<POLine.closed, Equal<True>, Or<POLine.cancelled, Equal<True>>>, decimal0,
			Case<Where<POLine.curyLineAmt, GreaterEqual<decimal0>>,
				Maximum<Sub<Sub<POLine.curyLineAmt, POLine.curyDiscAmt>, POLine.curyBilledAmt>, decimal0>>>,
			Minimum<Sub<Sub<POLine.curyLineAmt, POLine.curyDiscAmt>, POLine.curyBilledAmt>, decimal0>>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Unbilled Amount", Enabled = false)]
		public virtual Decimal? CuryUnbilledAmt
		{
			get;
			set;
		}
		#endregion
		#region UnbilledAmt
		public abstract class unbilledAmt : PX.Data.BQL.BqlDecimal.Field<unbilledAmt> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? UnbilledAmt
		{
			get;
			set;
		}
		#endregion

		#region LeftToReceiveQty
		public abstract class leftToReceiveQty : PX.Data.BQL.BqlDecimal.Field<leftToReceiveQty> { }
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Open Qty.", Visibility = PXUIVisibility.Invisible)]
		public virtual Decimal? LeftToReceiveQty
		{
			[PXDependsOnFields(typeof(orderQty),typeof(receivedQty))]
			get
			{
				return (this._OrderQty - this._ReceivedQty);
			}
		}
		#endregion
		#region LeftToReceiveBaseQty
		public abstract class leftToReceiveBaseQty : PX.Data.BQL.BqlDecimal.Field<leftToReceiveBaseQty> { }

		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? LeftToReceiveBaseQty
		{
			[PXDependsOnFields(typeof(baseOrderQty),typeof(baseReceivedQty))]
			get
			{
				return (this._BaseOrderQty - this._BaseReceivedQty);
			}
		}
		#endregion

		#region ReqPrepaidQty
		public abstract class reqPrepaidQty : BqlDecimal.Field<reqPrepaidQty>
		{
		}
		[PXDBQuantity(typeof(uOM), typeof(baseReqPrepaidQty), HandleEmptyKey = true, MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ReqPrepaidQty
		{
			get;
			set;
		}
		#endregion
		#region BaseReqPrepaidQty
		public abstract class baseReqPrepaidQty : BqlDecimal.Field<baseReqPrepaidQty>
		{
		}
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? BaseReqPrepaidQty
		{
			get;
			set;
		}
		#endregion
		#region DisplayReqPrepaidQty
		public abstract class displayReqPrepaidQty : BqlDecimal.Field<displayReqPrepaidQty>
		{
		}
		[PXQuantity]
		[PXFormula(typeof(Minimum<orderQty, reqPrepaidQty>))]
		[PXUIField(DisplayName = "Prepaid Qty.", Enabled = false)]
		public virtual decimal? DisplayReqPrepaidQty
		{
			get;
			set;
		}
		#endregion

		#region CuryReqPrepaidAmt
		public abstract class curyReqPrepaidAmt : BqlDecimal.Field<curyReqPrepaidAmt>
		{
		}
		[PXDBCurrency(typeof(curyInfoID), typeof(reqPrepaidAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Prepaid Amount", Enabled = false)]
		public virtual decimal? CuryReqPrepaidAmt
		{
			get;
			set;
		}
		#endregion
		#region ReqPrepaidAmt
		public abstract class reqPrepaidAmt : BqlDecimal.Field<reqPrepaidAmt>
		{
		}
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? ReqPrepaidAmt
		{
			get;
			set;
		}
		#endregion

		#region RequestedDate
		public abstract class requestedDate : PX.Data.BQL.BqlDateTime.Field<requestedDate> { }
		protected DateTime? _RequestedDate;
		[PXDBDate()]
		[PXDefault(typeof(POOrder.orderDate), PersistingCheck= PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Requested")]
		public virtual DateTime? RequestedDate
		{
			get
			{
				return this._RequestedDate;
			}
			set
			{
				this._RequestedDate = value;
			}
		}
			#endregion
		#region PromisedDate
		public abstract class promisedDate : PX.Data.BQL.BqlDateTime.Field<promisedDate> { }
		protected DateTime? _PromisedDate;
		[PXDBDate()]
		[PXDefault(typeof(POOrder.expectedDate), PersistingCheck= PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Promised")]
		public virtual DateTime? PromisedDate
		{
			get
			{
				return this._PromisedDate;
			}
			set
			{
				this._PromisedDate = value;
			}
		}
		#endregion
		#region Cancelled
		public abstract class cancelled : PX.Data.BQL.BqlBool.Field<cancelled> { }
		protected Boolean? _Cancelled;
		[PXDBBool()]
		[PXUIField(DisplayName = "Cancelled", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
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
		#region CompletePOLine
		public abstract class completePOLine : PX.Data.BQL.BqlString.Field<completePOLine> { }
		protected String _CompletePOLine;
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PXFormula(typeof(
			Switch<Case<Where<POLine.lineType, In3<POLineType.nonStockForDropShip, POLineType.nonStockForSalesOrder, POLineType.nonStockForServiceOrder>>, CompletePOLineTypes.quantity>,
				IsNull<Selector<POLine.inventoryID, InventoryItem.completePOLine>, CompletePOLineTypes.amount>>))]
		[PXUIField(DisplayName = "Close PO Line", Enabled = false, Visible = false)]
		[CompletePOLineTypes.List()]
		public virtual String CompletePOLine
		{
			get
			{
				return this._CompletePOLine;
			}
			set
			{
				this._CompletePOLine = value;
			}
		}
		#endregion
		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Completed", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual bool? Completed
		{
			get;
			set;
		}
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Closed", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual bool? Closed
		{
			get;
			set;
		}
		#endregion

		#region POAccrualType
		public abstract class pOAccrualType : PX.Data.BQL.BqlString.Field<pOAccrualType> { }
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<
			Case<Where<Current<POOrder.pOAccrualType>, Equal<POAccrualType.order>, Or<Where<POLine.lineType, NotEqual<POLineType.service>>>>, Current<POOrder.pOAccrualType>>,
			Switch<Case<Where<Current<POSetup.addServicesFromNormalPOtoPR>, Equal<True>, And<Current<POLine.orderType>, Equal<POOrderType.regularOrder>,
				Or<Current<POSetup.addServicesFromDSPOtoPR>, Equal<True>, And<Current<POLine.orderType>, Equal<POOrderType.dropShip>>>>>, POAccrualType.receipt>, POAccrualType.order>>))]
		[POAccrualType.List]
		[PXUIField(DisplayName = "Billing Based On", Enabled = false)]
		public virtual string POAccrualType
		{
			get;
			set;
		}
		#endregion
		#region OrderNoteID
		public abstract class orderNoteID : PX.Data.BQL.BqlGuid.Field<orderNoteID> { }
		[PXDBGuid]
		[PXDefault(typeof(POOrder.noteID))]
		public virtual Guid? OrderNoteID
		{
			get;
			set;
		}
		#endregion

		#region AllowComplete
		public abstract class allowComplete : PX.Data.BQL.BqlBool.Field<allowComplete> { }
		protected Boolean? _AllowComplete;
		[PXDBBool()]
		[PXUIField(DisplayName = "Allow Complete", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual Boolean? AllowComplete
		{
			get
			{
				return this._AllowComplete;
			}
			set
			{
				this._AllowComplete = value;
			}
		}
		#endregion
		#region IsStockItem
		public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
		[PXBool]
		[PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
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
		public virtual Boolean? IsKit
		{
			get;
			set;
		}
		#endregion

        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<APDiscount.discountID, Where<APDiscount.bAccountID, Equal<Current<POLine.vendorID>>, And<APDiscount.type, Equal<DiscountType.LineDiscount>>>>))]
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

		#region OrderedQtyAltered
		public abstract class orderedQtyAltered : PX.Data.BQL.BqlBool.Field<orderedQtyAltered> { }
		[PXBool]
		[PXFormula(typeof(False))]
		public virtual bool? OrderedQtyAltered
		{
			get; set;
		}
		#endregion
		#region OverridenUOM
		public abstract class overridenUOM : IBqlField
		{
		}
		[PXString(6, IsUnicode = true)]
		public virtual string OverridenUOM
		{
			get;
			set;
		}
		#endregion
		#region OverridenQty
		public abstract class overridenQty : IBqlField
		{
		}
		[PXDecimal]
		public virtual decimal? OverridenQty
		{
			get;
			set;
		}
		#endregion
		#region BaseOverridenQty
		public abstract class baseOverridenQty : IBqlField
		{
		}
		[PXDecimal]
		public virtual decimal? BaseOverridenQty
		{
			get;
			set;
		}
		#endregion

		#region ITaxableDetail
		decimal? ITaxableDetail.CuryTranAmt
		{
			get
			{
				return this.CuryExtCost;
			}
		}

		decimal? ITaxableDetail.TranAmt
		{
			get
			{
				return this.ExtCost;
			}
		}
		#endregion

		#region CuryReceivedCost
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class curyReceivedCost : PX.Data.BQL.BqlDecimal.Field<curyReceivedCost> { }
		[PXDecimal(4)]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual decimal? CuryReceivedCost
		{
			get { return null; }
			set { }
		}
		#endregion

		#region IMatrixItem members
		decimal? IMatrixItemLine.Qty
		{
			get => OrderQty;
			set => OrderQty = value;
		}
		#endregion
	}

	public static class POLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(GoodsForInventory, Messages.GoodsForInventory),
					Pair(GoodsForSalesOrder, Messages.GoodsForSalesOrder),
                    Pair(GoodsForServiceOrder, Messages.GoodsForServiceOrder),
					Pair(GoodsForReplenishment, Messages.GoodsForReplenishment),
					Pair(GoodsForDropShip, Messages.GoodsForDropShip),
					Pair(NonStockForDropShip, Messages.NonStockForDropShip),
					Pair(NonStockForSalesOrder, Messages.NonStockForSalesOrder),
                    Pair(NonStockForServiceOrder, Messages.NonStockForServiceOrder),
					Pair(NonStock, Messages.NonStockItem),
					Pair(Service, Messages.Service),
					Pair(Freight, Messages.Freight),
					Pair(Description, Messages.Description),
				}) {}
		}

		/// <summary>
		/// Selector. Provides a Default List of PO Line Types <br/>
		/// i.e. GoodsForInventory, NonStock, Service 
		/// </summary>
		public class DefaultListAttribute : PXStringListAttribute
		{
			public DefaultListAttribute() : base(
				new[]
				{
					Pair(GoodsForInventory, Messages.GoodsForInventory),
					Pair(NonStock, Messages.NonStockItem),
					Pair(Service, Messages.Service),
				}) {}
		}

		public const string GoodsForInventory = "GI";
		public const string GoodsForSalesOrder = "GS";
        public const string GoodsForServiceOrder = "GF";
		public const string GoodsForReplenishment = "GR";
		public const string GoodsForDropShip = "GP";
		public const string NonStockForDropShip = "NP";
		public const string NonStockForSalesOrder = "NO";
        public const string NonStockForServiceOrder = "NF";
		public const string NonStock = "NS";
		public const string Service = "SV";
		public const string Freight = "FT";
		public const string MiscCharges = "MC";
		public const string Description = "DN";

	    public const string GoodsForManufacturing = "GM";
	    public const string NonStockForManufacturing = "NM";

		public class goodsForInventory : PX.Data.BQL.BqlString.Constant<goodsForInventory>
		{
			public goodsForInventory() : base(GoodsForInventory) { ;}
		}

		public class goodsForSalesOrder : PX.Data.BQL.BqlString.Constant<goodsForSalesOrder>
		{
			public goodsForSalesOrder() : base(GoodsForSalesOrder) { ;}
		}

        public class goodsForServiceOrder : PX.Data.BQL.BqlString.Constant<goodsForServiceOrder>
		{
            public goodsForServiceOrder() : base(GoodsForServiceOrder) {; }
        }
		public class goodsForReplenishment : PX.Data.BQL.BqlString.Constant<goodsForReplenishment>
		{
			public goodsForReplenishment() : base(GoodsForReplenishment) { ;}
		}

		public class goodsForDropShip : PX.Data.BQL.BqlString.Constant<goodsForDropShip>
		{
			public goodsForDropShip() : base(GoodsForDropShip) { ;}
		}

		public class nonStockForDropShip : PX.Data.BQL.BqlString.Constant<nonStockForDropShip>
		{
			public nonStockForDropShip() : base(NonStockForDropShip) { ;}
		}
		public class nonStockForSalesOrder : PX.Data.BQL.BqlString.Constant<nonStockForSalesOrder>
		{
			public nonStockForSalesOrder() : base(NonStockForSalesOrder) { ;}
		}

        public class nonStockForServiceOrder : PX.Data.BQL.BqlString.Constant<nonStockForServiceOrder>
		{
            public nonStockForServiceOrder() : base(NonStockForServiceOrder) {; }
        }

		public class nonStock : PX.Data.BQL.BqlString.Constant<nonStock>
		{
			public nonStock() : base(NonStock) { ;}
		}

		public class service : PX.Data.BQL.BqlString.Constant<service>
		{
			public service() : base(Service) { ;}
		}

		public class freight : PX.Data.BQL.BqlString.Constant<freight>
		{
			public freight() : base(Freight) { ;}
		}

		public class miscCharges : PX.Data.BQL.BqlString.Constant<miscCharges>
		{
			public miscCharges() : base(MiscCharges) { ;}
		}

		public class description : PX.Data.BQL.BqlString.Constant<description>
		{
			public description() : base(Description) { ;}
		}

	    public class goodsForManufacturing : PX.Data.BQL.BqlString.Constant<goodsForManufacturing>
		{
	        public goodsForManufacturing() : base(GoodsForManufacturing) {; }
	    }

	    public class nonStockForManufacturing : PX.Data.BQL.BqlString.Constant<nonStockForManufacturing>
		{
	        public nonStockForManufacturing() : base(NonStockForManufacturing) {; }
	    }

		public static bool IsStock(string lineType) => Goods.ContainsValue(lineType);
		public class Goods : SetOf.Strings.FilledWith<
			goodsForInventory,
			goodsForDropShip,
			goodsForSalesOrder,
            goodsForServiceOrder,
			goodsForReplenishment,
			goodsForManufacturing>
		{ }

		public static bool IsNonStock(string lineType) => NonStocks.ContainsValue(lineType);
		public class NonStocks : SetOf.Strings.FilledWith<
			nonStock,
			nonStockForDropShip,
			nonStockForSalesOrder,
            nonStockForServiceOrder,
			service,
			nonStockForManufacturing>
		{ }

		public static bool IsService(string lineType)
		{
			return
				lineType == POLineType.Service;
		}

		public static bool IsDropShip(string lineType)
		{
			return lineType.IsIn(POLineType.GoodsForDropShip, POLineType.NonStockForDropShip);
		}

		public static bool IsDefault(string lineType)
		{
			return
				lineType == POLineType.GoodsForInventory ||
				lineType == POLineType.NonStock ||
				lineType == POLineType.Service ||
				lineType == POLineType.Freight ||
				lineType == POLineType.Description;
		}

		public static bool UsePOAccrual(string lineType)
		{
			return IsStock(lineType) || IsNonStock(lineType) && !IsService(lineType);
		}
	}

	public class POReceiptQtyAction
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Reject, Messages.Reject),
					Pair(AcceptButWarn, Messages.AcceptButWarn),
					Pair(Accept, Messages.Accept),
				}) {}
		}

		public const string Accept = "A";
		public const string AcceptButWarn = "W";
		public const string Reject = "R";
	}

	//This class is used for reports
    [System.SerializableAttribute()]
    [PXHidden]
	public partial class POLineFilter : PX.Data.IBqlTable 
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Filterable = true)]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[SubItem(typeof(POLineFilter.inventoryID))]
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
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site()]
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
	}
}
