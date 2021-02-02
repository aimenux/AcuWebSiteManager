using System;
using System.Diagnostics;

using PX.Data;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.TX;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.DR;
using PX.Objects.PM;
using PX.Objects.CR;
using PX.Objects.Common;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.GL.DAC.Abstract;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a line of an Accounts Receivable invoice or memo. The record
	/// contains such information as the inventory item name, price and quantity,
	/// line discounts, and tax category. Entities of this type are edited 
	/// on the Invoices and Memos (AR301000) form, which corresponds 
	/// to the <see cref="ARInvoiceEntry"/> graph.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("LineType={LineType} TranAmt={CuryTranAmt}")]
	[PXCacheName(Messages.ARTran)]
	public partial class ARTran : IBqlTable, IHasMinGrossProfit, DR.Descriptor.IDocumentLine, ISortOrder, ILSPrimary, IAccountable
	{
        #region Keys
        public class PK : PrimaryKeyOf<ARTran>.By<tranType, refNbr, lineNbr>
        {
            public static ARTran Find(PXGraph graph, string tranType, string refNbr, int? lineNbr) => FindBy(graph, tranType, refNbr, lineNbr);
        }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        protected bool? _Selected = false;
        [PXBool()]
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
		[Branch(typeof(ARRegister.branchID))]
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(ARRegister.docType))]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(ARRegister.refNbr))]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXParent(typeof(Select<ARRegister, Where<ARRegister.docType, Equal<Current<ARTran.tranType>>, And<ARRegister.refNbr, Equal<Current<ARTran.refNbr>>>>>))]
		[PXParent(typeof(Select<SO.SOInvoice, Where<SO.SOInvoice.docType, Equal<Current<ARTran.tranType>>, And<SO.SOInvoice.refNbr, Equal<Current<ARTran.refNbr>>>>>))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXLineNbr(typeof(ARRegister.lineCntr))]
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
		[PXDBInt]
		[PXUIField(DisplayName = AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
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
		#region SOOrderType
		public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
		protected String _SOOrderType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Order Type", Enabled = false)]
		[PXSelector(typeof(Search<SO.SOOrderType.orderType>), CacheGlobal = true)]
		public virtual String SOOrderType
		{
			get
			{
				return this._SOOrderType;
			}
			set
			{
				this._SOOrderType = value;
			}
		}
		#endregion
		#region SOOrderNbr
		public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
		protected String _SOOrderNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Order Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SO.SOOrder.orderNbr, Where<SO.SOOrder.orderType, Equal<Current<ARTran.sOOrderType>>>>))]
		public virtual String SOOrderNbr
		{
			get
			{
				return this._SOOrderNbr;
			}
			set
			{
				this._SOOrderNbr = value;
			}
		}
		#endregion
		#region SOOrderLineNbr
		public abstract class sOOrderLineNbr : PX.Data.BQL.BqlInt.Field<sOOrderLineNbr> { }
		protected Int32? _SOOrderLineNbr;
		[PXDBInt()]
		[PXParent(typeof(Select<SO.SOLine2, Where<SO.SOLine2.orderType, Equal<Current<ARTran.sOOrderType>>, And<SO.SOLine2.orderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<SO.SOLine2.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>), LeaveChildren = true)]
		[PXParent(typeof(Select<SO.SOMiscLine2, Where<SO.SOMiscLine2.orderType, Equal<Current<ARTran.sOOrderType>>, And<SO.SOMiscLine2.orderNbr, Equal<Current<ARTran.sOOrderNbr>>, And<SO.SOMiscLine2.lineNbr, Equal<Current<ARTran.sOOrderLineNbr>>>>>>), LeaveChildren = true)]
		[PXUIField(DisplayName = "Order Line Nbr", Visible = false, Enabled = false)]
		public virtual Int32? SOOrderLineNbr
		{
			get
			{
				return this._SOOrderLineNbr;
			}
			set
			{
				this._SOOrderLineNbr = value;
			}
		}
		#endregion
		#region SOOrderLineOperation
		public abstract class sOOrderLineOperation : PX.Data.IBqlField
		{
		}
		protected String _SOOrderLineOperation;
		[PXDBString(1, IsFixed = true, InputMask = ">a")]
		public virtual String SOOrderLineOperation
		{
			get
			{
				return this._SOOrderLineOperation;
			}
			set
			{
				this._SOOrderLineOperation = value;
			}
		}
		#endregion
		#region SOOrderSortOrder
		public abstract class soOrderSortOrder : PX.Data.BQL.BqlInt.Field<soOrderSortOrder> { }
		protected Int32? _SOOrderSortOrder;
		[PXDBInt]
		[PXUIField(DisplayName = "Order Sort Order", Visible = false, Enabled = false)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? SOOrderSortOrder
		{
			get
			{
				return this._SOOrderSortOrder;
			}
			set
			{
				this._SOOrderSortOrder = value;
			}
		}
		#endregion
		#region SOShipmentType
		public abstract class sOShipmentType : PX.Data.BQL.BqlString.Field<sOShipmentType> { }
		protected String _SOShipmentType;
		[PXDBString(1, IsFixed = true)]
		public virtual String SOShipmentType
		{
			get
			{
				return this._SOShipmentType;
			}
			set
			{
				this._SOShipmentType = value;
			}
		}
		#endregion
		#region SOShipmentNbr
		public abstract class sOShipmentNbr : PX.Data.BQL.BqlString.Field<sOShipmentNbr> { }
		protected String _SOShipmentNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Shipment Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SO.Navigate.SOOrderShipment.shipmentNbr,
			Where<SO.Navigate.SOOrderShipment.orderType, Equal<Current<ARTran.sOOrderType>>,
			And<SO.Navigate.SOOrderShipment.orderNbr, Equal<Current<ARTran.sOOrderNbr>>,
			And<SO.Navigate.SOOrderShipment.shipmentType, Equal<Current<ARTran.sOShipmentType>>>>>>))]
		public virtual String SOShipmentNbr
		{
			get
			{
				return this._SOShipmentNbr;
			}
			set
			{
				this._SOShipmentNbr = value;
			}
		}
		#endregion
		#region SOShipmentLineNbr
		public abstract class sOShipmentLineNbr : PX.Data.BQL.BqlInt.Field<sOShipmentLineNbr> { }
		protected Int32? _SOShipmentLineNbr;
		[PXDBInt()]
		public virtual Int32? SOShipmentLineNbr
		{
			get
			{
				return this._SOShipmentLineNbr;
			}
			set
			{
				this._SOShipmentLineNbr = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer()]
		[PXDBDefault(typeof(ARRegister.customerID))]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault("")]
		public virtual String BatchNbr
		{
			get
			{
				return this._BatchNbr;
			}
			set
			{
				this._BatchNbr = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName="Line Type", Visible=false, Enabled=false)]
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
        #region IsFree
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
        protected Boolean? _IsFree;
        [PXBool()]
        public virtual Boolean? IsFree
        {
            get
            {
                return this._IsFree ?? false;
            }
            set
            {
                this._IsFree = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		protected Int32? _ProjectID;
		[PXDBInt()]
		[PXDefault(typeof(ARInvoice.projectID), PersistingCheck=PXPersistingCheck.Nothing)]
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
		#region PMDeltaOption
		public abstract class pMDeltaOption : PX.Data.BQL.BqlString.Field<pMDeltaOption>
		{
			public const string CompleteLine = "C";
			public const string BillLater = "U";
		}
		protected string _PMDeltaOption;
		[PXDefault(pMDeltaOption.CompleteLine)]
		[PXDBString()]
		public virtual string PMDeltaOption
		{
			get
			{
				return this._PMDeltaOption;
			}
			set
			{
				this._PMDeltaOption = value;
			}
		}
		#endregion
		#region ExpenseDate
		public abstract class expenseDate : PX.Data.BQL.BqlDateTime.Field<expenseDate> { }
		protected DateTime? _ExpenseDate;
		[PXDBDate()]
		public virtual DateTime? ExpenseDate
		{
			get
			{
				return this._ExpenseDate;
			}
			set
			{
				this._ExpenseDate = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[CurrencyInfo(typeof(ARRegister.curyInfoID))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[ARTranInventoryItem(Filterable = true)]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? InventoryID
		{
			get;
			set;
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
		#region DeferredCode
		public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
		protected String _DeferredCode;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code")]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID, Where<DRDeferredCode.accountType, Equal<DeferredAccountType.income>>>))]
		[PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
        [PXDefault(typeof(Search2<InventoryItem.deferredCode, InnerJoin<DRDeferredCode, On<DRDeferredCode.deferredCodeID, Equal<InventoryItem.deferredCode>>>, Where<DRDeferredCode.accountType, Equal<DeferredAccountType.income>, And<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DeferredCode
		{
			get
			{
				return this._DeferredCode;
			}
			set
			{
				this._DeferredCode = value;
			}
		}
		#endregion
		#region InvtMult
		public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

		[PXDBShort]
		[PXDefault(typeof(short0))]
		[PXUIField(DisplayName = "Multiplier")]
		public virtual short? InvtMult { get; set; }
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;

		[PXDBInt]
		[PXDimensionSelector(SiteAttribute.DimensionName, typeof(INSite.siteID), typeof(INSite.siteCD))]
		[PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
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
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(ARTran.inventoryID))]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		[PXDBQuantity(typeof(uOM), typeof(baseQty), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? Qty
			{
			get;
			set;
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		//Invert sign of BaseQty for BilledQty and UnbilledQty calculation in case AR Invoice type (credit/debit) and SOLine operation (issue/receipt) have opposite signs 
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>, 
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.baseQty, shortMinus1>>, ARTran.baseQty>), typeof(AddCalc<SO.SOLine2.baseBilledQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>,
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.baseQty, shortMinus1>>, ARTran.baseQty>), typeof(SubCalc<SO.SOLine2.baseUnbilledQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>,
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.baseQty, shortMinus1>>, ARTran.baseQty>), typeof(AddCalc<SO.SOMiscLine2.baseBilledQty>))]
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>,
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.baseQty, shortMinus1>>, ARTran.baseQty>), typeof(SubCalc<SO.SOMiscLine2.baseUnbilledQty>))]
		[PXUIField(DisplayName = "Base Qty.", Visible = false, Enabled = false)]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXPriceCost()]
		[PXDBCalced(typeof(Switch<Case<Where<ARTran.qty, NotEqual<decimal0>>, Div<ARTran.tranCost, ARTran.qty>>, decimal0>), typeof(Decimal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region TranCost
		public abstract class tranCost : PX.Data.BQL.BqlDecimal.Field<tranCost> { }
		protected Decimal? _TranCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ext. Cost")]
		public virtual Decimal? TranCost
		{
			get
			{
				return this._TranCost;
			}
			set
			{
				this._TranCost = value;
			}
		}
		#endregion
		#region TranCostOrig
		public abstract class tranCostOrig : PX.Data.BQL.BqlDecimal.Field<tranCostOrig> { }
		protected Decimal? _TranCostOrig;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Orig. Ext. Cost")]
		public virtual Decimal? TranCostOrig
		{
			get
			{
				return this._TranCostOrig;
			}
			set
			{
				this._TranCostOrig = value;
			}
		}
		#endregion
		#region IsTranCostFinal
		public abstract class isTranCostFinal : PX.Data.BQL.BqlBool.Field<isTranCostFinal> { }
		protected Boolean? _IsTranCostFinal;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsTranCostFinal
		{
			get
			{
				return this._IsTranCostFinal;
			}
			set
			{
				this._IsTranCostFinal = value;
			}
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(ARTran.curyInfoID), typeof(ARTran.unitPrice))]
		[PXUIField(DisplayName = "Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryUnitPrice
			{
			get;
			set;
		}
		#endregion

		#region UnitPrice
		public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

		[PXDBPriceCost]
		//[PXDefault(typeof(Search<InventoryItem.basePrice, Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>))]
		public virtual decimal? UnitPrice
		{
			get;
			set;
		}
		#endregion
		#region CuryExtPrice
		public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
		protected Decimal? _CuryExtPrice;
		[PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.extPrice))]
		[PXUIField(DisplayName = "Ext. Price")]
		[PXFormula(typeof(Mult<ARTran.qty, ARTran.curyUnitPrice>))]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.discAmt))]
        [PXUIField(DisplayName = "Discount Amount")]
        //[PXFormula(typeof(Div<Mult<Mult<ARTran.qty, ARTran.curyUnitPrice>, ARTran.discPct>, decimal100>))]
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
		#region ManualDisc
		public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
		protected Boolean? _ManualDisc;
        [ManualDiscountMode(typeof(ARTran.curyDiscAmt), typeof(ARTran.curyTranAmt), typeof(ARTran.discPct), typeof(ARTran.freezeManualDisc), DiscountFeatureType.CustomerDiscount)]
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
        #region ManualPrice
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
        protected Boolean? _ManualPrice;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price", Visible=false)]
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
		#region OrigGroupDiscountRate
		public abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		protected Decimal? _OrigGroupDiscountRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? OrigGroupDiscountRate
		{
			get
			{
				return this._OrigGroupDiscountRate;
			}
			set
			{
				this._OrigGroupDiscountRate = value;
			}
		}
		#endregion
		#region OrigDocumentDiscountRate
		public abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		protected Decimal? _OrigDocumentDiscountRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "1.0")]
		public virtual Decimal? OrigDocumentDiscountRate
		{
			get
			{
				return this._OrigDocumentDiscountRate;
			}
			set
			{
				this._OrigDocumentDiscountRate = value;
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

		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }
		
		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual decimal? RetainagePct
		{
			get;
			set;
		}
		#endregion
		#region CuryRetainageAmt
		public abstract class curyRetainageAmt : PX.Data.BQL.BqlDecimal.Field<curyRetainageAmt> { }
		
		[PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.retainageAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(Visibility = PXUIVisibility.Invisible)]
		public virtual decimal? CuryRetainageAmt
		{
			get;
			set;
		}
		#endregion
		#region RetainageAmt
		public abstract class retainageAmt : PX.Data.BQL.BqlDecimal.Field<retainageAmt> { }

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? RetainageAmt
		{
			get;
			set;
        }
        #endregion

		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Decimal? _CuryTranAmt;
		[PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.tranAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		[PXFormula(typeof(Sub<ARTran.curyExtPrice, Add<ARTran.curyDiscAmt, ARTran.curyRetainageAmt>>))]
		[PXFormula(null, typeof(CountCalc<ARSalesPerTran.refCntr>))]
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>,
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.curyTranAmt, shortMinus1>>, ARTran.curyTranAmt>), typeof(AddCalc<SO.SOMiscLine2.curyBilledAmt>))]
		[PXUnboundFormula(typeof(Switch<Case<Where2<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.receipt>, And<ARTran.drCr, Equal<DrCr.credit>>>,
			Or<Where<ARTran.sOOrderLineOperation, Equal<SO.SOOperation.issue>, And<ARTran.drCr, Equal<DrCr.debit>>>>>, Mult<ARTran.curyTranAmt, shortMinus1>>, ARTran.curyTranAmt>), typeof(SubCalc<SO.SOMiscLine2.curyUnbilledAmt>))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CuryTranAmt
		{
			get
			{
				return this._CuryTranAmt;
			}
			set
			{
				this._CuryTranAmt = value;
			}
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }
		protected Decimal? _TranAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TranAmt
		{
			get
			{
				return this._TranAmt;
			}
			set
			{
				this._TranAmt = value;
			}
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		protected Decimal? _CuryTaxableAmt;
		[PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.taxableAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Net Amount", Enabled = false)]
		public virtual Decimal? CuryTaxableAmt
		{
			get
			{
				return this._CuryTaxableAmt;
			}
			set
			{
				this._CuryTaxableAmt = value;
			}
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }
		protected Decimal? _TaxableAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxableAmt
		{
			get
			{
				return this._TaxableAmt;
			}
			set
			{
				this._TaxableAmt = value;
			}
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		protected Decimal? _CuryTaxAmt;
		[PXDBCurrency(typeof(ARTran.curyInfoID), typeof(ARTran.taxAmt))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "VAT", Enabled = false)]
		public virtual Decimal? CuryTaxAmt
		{
			get
			{
				return this._CuryTaxAmt;
			}
			set
			{
				this._CuryTaxAmt = value;
			}
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }
		protected Decimal? _TaxAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TaxAmt
		{
			get
			{
				return this._TaxAmt;
			}
			set
			{
				this._TaxAmt = value;
			}
		}
		#endregion
		#region TranClass
		public abstract class tranClass : PX.Data.BQL.BqlString.Field<tranClass> { }
		protected String _TranClass;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("")]
		public virtual String TranClass
		{
			get
			{
				return this._TranClass;
			}
			set
			{
				this._TranClass = value;
			}
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		protected String _DrCr;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(ARInvoice.drCr))]
		public virtual String DrCr
		{
			get
			{
				return this._DrCr;
			}
			set
			{
				this._DrCr = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate]
		[PXDBDefault(typeof(ARRegister.docDate))]
		[PXUIField(DisplayName = Common.Messages.DocumentDate, Visible = false)]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region OrigInvoiceDate
		public abstract class origInvoiceDate : PX.Data.BQL.BqlDateTime.Field<origInvoiceDate> { }
		protected DateTime? _OrigInvoiceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Original Invoice date")]
		public virtual DateTime? OrigInvoiceDate
		{
			get
			{
				return this._OrigInvoiceDate;
			}
			set
			{
				this._OrigInvoiceDate = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

	    [FinPeriodID(
	        branchSourceType: typeof(ARTran.branchID),
	        masterFinPeriodIDType: typeof(ARTran.tranPeriodID),
	        headerMasterFinPeriodIDType: typeof(ARRegister.tranPeriodID))]
        public virtual String FinPeriodID
		{
			get
			{
				return this._FinPeriodID;
			}
			set
			{
				this._FinPeriodID = value;
			}
		}
        #endregion
	    #region TranPeriodID
	    public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
	    [PeriodID]
	    public virtual String TranPeriodID { get; set; }
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
		protected String _TranDesc;
		[PXDBString(256, IsUnicode = true)]
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected Int32? _SalesPersonID;
		[SalesPerson()]
		[PXParent(typeof(Select<ARSalesPerTran, Where<ARSalesPerTran.docType, Equal<Current<ARTran.tranType>>, And<ARSalesPerTran.refNbr, Equal<Current<ARTran.refNbr>>, And<ARSalesPerTran.salespersonID, Equal<Current2<ARTran.salesPersonID>>, And<Current<ARTran.commissionable>, Equal<True>>>>>>), LeaveChildren = true, ParentCreate = true)]
		[PXDefault(typeof(ARRegister.salesPersonID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Switch<Case<Where<ARTran.lineType, Equal<SO.SOLineType.freight>>, Null>, ARTran.salesPersonID>))]
		[PXForeignReference(typeof(Field<ARTran.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
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
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        protected Int32? _EmployeeID;
        [PXDBInt]
        [PXDefault(typeof(Search<EP.EPEmployee.bAccountID, Where<EP.EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck=PXPersistingCheck.Nothing)]
        public virtual Int32? EmployeeID
        {
            get
            {
                return this._EmployeeID;
            }
            set
            {
                this._EmployeeID = value;
            }
        }
        #endregion
		#region CommnPct
		public abstract class commnPct : PX.Data.BQL.BqlDecimal.Field<commnPct> { }
		protected Decimal? _CommnPct;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		[PXDefault(TypeCode.Decimal, "0.0")]
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
		#region DefScheduleID
		public abstract class defScheduleID : PX.Data.BQL.BqlInt.Field<defScheduleID> { }
		protected int? _DefScheduleID;
		[PXDBInt]
		[PXUIField(DisplayName = "Original Deferral Schedule")]
		[PXSelector(
			typeof(Search<DR.DRSchedule.scheduleID, 
				Where<DR.DRSchedule.bAccountID, Equal<Current<ARInvoice.customerID>>,
					And<DR.DRSchedule.docType, NotEqual<Current<ARTran.tranType>>>>>),
			SubstituteKey = typeof(DRSchedule.scheduleNbr))]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(Search<InventoryItem.taxCategoryID,
			Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>),
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
		#region ReasonCode
		public abstract class reasonCode : PX.Data.BQL.BqlString.Field<reasonCode> { }
		protected String _ReasonCode;
		[PXDBString(CS.ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID,
			Where<ReasonCode.usage, Equal<ReasonCodeUsages.sales>, Or<ReasonCode.usage, Equal<ReasonCodeUsages.issue>>>>), DescriptionField = typeof(ReasonCode.descr))]
		[PXUIField(DisplayName = "Reason Code")]
		[PXForeignReference(typeof(Field<ARTran.reasonCode>.IsRelatedTo<ReasonCode.reasonCodeID>))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(typeof(ARTran.branchID), DisplayName = "Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault(typeof(Search<InventoryItem.salesAcctID, Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(typeof(ARTran.accountID), typeof(ARTran.branchID), true, DisplayName = "Subaccount", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Search<InventoryItem.salesSubID, Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>>>))]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		protected Int32? _TaskID;
		[PXDefault(typeof(Coalesce<Search<PMAccountTask.taskID, Where<PMAccountTask.projectID, Equal<Current<ARTran.projectID>>, And<PMAccountTask.accountID, Equal<Current<ARTran.accountID>>>>>,
			Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProjectTask(typeof(ARTran.projectID), BatchModule.AR, DisplayName = "Project Task")]
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
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(typeof(accountID), typeof(taskID), GL.AccountType.Income, SkipVerificationForDefault = true, ReleasedField = typeof(released))]
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
		[PXFormula(typeof(Switch<Case<Where<ARTran.inventoryID, IsNotNull>, Selector<ARTran.inventoryID, InventoryItem.commisionable>>, True>))]
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
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		/// <summary>
		/// Reference Date. May be an original expense date that is billed to the customer.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = Common.Messages.ExpenseDate, Visible = false)]
		public virtual DateTime? Date
		{
			get;
			set;
		}
		#endregion
		#region CaseID
		public abstract class caseID : PX.Data.BQL.BqlInt.Field<caseID> { }
		protected Int32? _CaseID;
		[PXSelector(typeof(Search<CRCase.caseID>), SubstituteKey = typeof(CRCase.caseCD))]
		[PXDBInt]
		[PXUIField(DisplayName = "Case ID", Visible = false, Enabled=false)]
		public virtual Int32? CaseID
		{
			get
			{
				return this._CaseID;
			}
			set
			{
				this._CaseID = value;
			}
		}
		#endregion
        #region RequireINUpdate
        public abstract class requireINUpdate : PX.Data.BQL.BqlBool.Field<requireINUpdate> { }
        [PXBool()]
        public bool? RequireINUpdate
        {
            get;
            set;
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

        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        protected String _DiscountID;
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouse>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomer>, 
              And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndCustomerPrice>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventory>, And<ARDiscount.applicableTo, NotEqual<DiscountTarget.warehouseAndInventoryPrice>>>>>>>>))]
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
		#region RequiresTerms
		public abstract class requiresTerms : PX.Data.BQL.BqlBool.Field<requiresTerms> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the <see cref="DRTermStartDate"/> and <see cref="DRTermEndDate"/>
		/// fields are enabled and should be filled for the line.
		/// </summary>
		/// <value>
		/// The value of this field is set by the <see cref="ARInvoiceEntry"/> and <see cref="ARCashSaleEntry"/> graphs
		/// based on the settings of the <see cref="InventoryID">item</see> and the <see cref="DeferredCode">Deferral Code</see> selected
		/// for the line. In other contexts it is not populated.
		/// See the attribute on the <see cref="ARInvoiceEntry.ARTran_RequiresTerms_CacheAttached"/> handler for details.
		/// </value>
		[PXBool]
		public virtual bool? RequiresTerms
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
		#region DiscPctDR
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
		#region ItemHasResidual
		public abstract class itemHasResidual : PX.Data.BQL.BqlBool.Field<itemHasResidual> { }

		[PXBool]
		[DR.DRTerms.VerifyResidual(typeof(inventoryID), typeof(deferredCode), typeof(curyUnitPriceDR), typeof(curyExtPrice))]
		public virtual bool? ItemHasResidual
		{
			get;
			set;
		}
		#endregion

		#region Sales Reporting
		#region GroupDiscountAmount
		public abstract class groupDiscountAmount : PX.Data.BQL.BqlDecimal.Field<groupDiscountAmount> { }
		protected decimal? _GroupDiscountAmount;
		[PXUIField(DisplayName = "Group Discount Amount", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Add<Mult<ARTran.tranAmt, Sub<decimal1, ARTran.groupDiscountRate>>, Mult<ARTran.tranAmt, Sub<decimal1, ARTran.origGroupDiscountRate>>>), typeof(Decimal))]
		public virtual decimal? GroupDiscountAmount
		{
			get
			{
				return this._GroupDiscountAmount;
			}
			set
			{
				this._GroupDiscountAmount = value;
			}
		}
		#endregion
		#region DocumentDiscountAmount
		public abstract class documentDiscountAmount : PX.Data.BQL.BqlDecimal.Field<documentDiscountAmount> { }
		[PXUIField(DisplayName = "Document Discount Amount", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Add<Mult<ARTran.tranAmt, Sub<decimal1, ARTran.documentDiscountRate>>, Mult<ARTran.tranAmt, Sub<decimal1, ARTran.origDocumentDiscountRate>>>), typeof(Decimal))]
		public virtual decimal? DocumentDiscountAmount
		{
			get;
			set;
        }
        #endregion        

        #region GrossSalesAmount
        public abstract class grossSalesAmount : PX.Data.BQL.BqlDecimal.Field<grossSalesAmount> { }
		protected decimal? _GrossSalesAmount;
		[PXUIField(DisplayName = "Gross Sales Amount", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Add<ARTran.discAmt, Switch<Case<Where<ARTran.taxableAmt, Equal<decimal0>>, ARTran.tranAmt>, ARTran.taxableAmt>>), typeof(Decimal))]
		public virtual decimal? GrossSalesAmount
		{
			get
			{
				return this._GrossSalesAmount;
			}
			set
			{
				this._GrossSalesAmount = value;
			}
		}
		#endregion
		#region Cost
		public abstract class cost : PX.Data.BQL.BqlDecimal.Field<cost> { }
		[PXUIField(DisplayName = "Cost", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Mult<Switch<Case<Where<ARTran.drCr, Equal<DrCr.debit>>, Minus<decimal1>>, decimal1>, Switch<Case<Where<ARTran.isTranCostFinal, Equal<False>>, ARTran.tranCostOrig>, ARTran.tranCost>>), typeof(Decimal))]
		public virtual decimal? Cost
		{
			get;
			set;
		}
		#endregion
		#region NetSalesAmount
		public abstract class netSalesAmount : PX.Data.BQL.BqlDecimal.Field<netSalesAmount> { }
		protected decimal? _NetSalesAmount;
		[PXUIField(DisplayName = "Net Sales Amount", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Mult<Switch<Case<Where<ARTran.drCr, Equal<DrCr.debit>>, Minus<decimal1>>, decimal1>, Sub<Sub<Sub<ARTran.grossSalesAmount, ARTran.discAmt>, ARTran.groupDiscountAmount>, ARTran.documentDiscountAmount>>), typeof(Decimal))]
		public virtual decimal? NetSalesAmount
		{
			get
			{
				return this._NetSalesAmount;
			}
			set
			{
				this._NetSalesAmount = value;
			}
		}
		#endregion
		#region Margin
		public abstract class margin : PX.Data.BQL.BqlDecimal.Field<margin> { }
		[PXUIField(DisplayName = "Margin", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Sub<ARTran.netSalesAmount, ARTran.cost>), typeof(Decimal))]
		public virtual decimal? Margin
		{
			get;
			set;
		}
		#endregion
		#region MarginPercent
		public abstract class marginPercent : PX.Data.BQL.BqlDecimal.Field<marginPercent> { }
		[PXUIField(DisplayName = "Margin Percent", Enabled = false)]
		[PXDecimal]
		[PXDBCalced(typeof(Switch<Case<Where<ARTran.netSalesAmount, NotEqual<decimal0>>, Mult<Div<ARTran.margin, ARTran.netSalesAmount>, decimal100>>, decimal0>), typeof(Decimal))]
		public virtual decimal? MarginPercent
		{
			get;
			set;
		}
		#endregion



		#endregion

		#region DR Interface Implementation
		string DR.Descriptor.IDocumentLine.Module => BatchModule.AR;
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

		[PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
			Where<InventoryItem.inventoryID, Equal<Current<ARTran.inventoryID>>,
			And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[IN.SubItem(
			typeof(ARTran.inventoryID),
			typeof(LeftJoin<INSiteStatus,
				On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
				And<INSiteStatus.inventoryID, Equal<Optional<ARTran.inventoryID>>,
				And<INSiteStatus.siteID, Equal<Optional<ARTran.siteID>>>>>>))]
		[PXFormula(typeof(Default<ARTran.inventoryID>))]
		public virtual int? SubItemID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		[IN.LocationAvail(typeof(ARTran.inventoryID), typeof(ARTran.subItemID), typeof(ARTran.siteID), typeof(ARTran.tranType), typeof(ARTran.invtMult))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LocationID { get; set; }
		#endregion

		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

		[SO.LSARTran.ARLotSerialNbr(typeof(ARTran.inventoryID), typeof(ARTran.subItemID), typeof(ARTran.locationID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
		public virtual string LotSerialNbr { get; set; }
		#endregion

		#region ExpireDate
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

		[SO.LSARTran.ARExpireDate(typeof(ARTran.inventoryID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
		public virtual DateTime? ExpireDate { get; set; }
		#endregion

		#region UnassignedQty
		public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

		[PXDecimal(6)]
		[PXFormula(typeof(decimal0))]
		public virtual decimal? UnassignedQty { get; set; }
		#endregion

		#region HasMixedProjectTasks
		public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

		[PXBool]
		[PXFormula(typeof(boolFalse))]
		public virtual bool? HasMixedProjectTasks { get; set; }
		#endregion

		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

		[PXDBLong(IsImmutable = true)]
		public virtual long? PlanID { get; set; }
		#endregion

		#region OrigInvoiceType
		public abstract class origInvoiceType : PX.Data.BQL.BqlString.Field<origInvoiceType> { }

		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Inv. Type", Enabled = false)]
		[ARDocType.List()]
		public virtual string OrigInvoiceType { get; set; }
		#endregion

		#region OrigInvoiceNbr
		public abstract class origInvoiceNbr : PX.Data.BQL.BqlString.Field<origInvoiceNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Inv. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<SO.SOInvoice.refNbr, Where<SO.SOInvoice.docType, Equal<Current<ARTran.origInvoiceType>>>>))]
		public virtual string OrigInvoiceNbr { get; set; }
		#endregion

		#region OrigInvoiceLineNbr
		public abstract class origInvoiceLineNbr : PX.Data.BQL.BqlInt.Field<origInvoiceLineNbr> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Orig. Inv. Line Nbr.", Enabled = false, Visible = false)]
		public virtual int? OrigInvoiceLineNbr
		{
			get;
			set;
		}
		#endregion

		#region InvtDocType
		public abstract class invtDocType : PX.Data.BQL.BqlString.Field<invtDocType> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Inventory Doc. Type", Enabled = false)]
		[INDocType.List()]
		public virtual string InvtDocType { get; set; }
		#endregion

		#region InvtRefNbr
		public abstract class invtRefNbr : PX.Data.BQL.BqlString.Field<invtRefNbr> { }

		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Inventory Ref. Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<ARTran.invtDocType>>>>))]
		public virtual string InvtRefNbr { get; set; }
		#endregion

		#region IDocumentTran
		public decimal? CuryCashDiscBal { get; set; }
		public decimal? CashDiscBal { get; set; }
		public decimal? CuryTranBal { get; set; }
		public decimal? TranBal { get; set; }
		#endregion
	}

	public class decimalPct : PX.Data.BQL.BqlDecimal.Constant<decimalPct>
	{
        public decimalPct() : base(0.01m) { }
	}
}
