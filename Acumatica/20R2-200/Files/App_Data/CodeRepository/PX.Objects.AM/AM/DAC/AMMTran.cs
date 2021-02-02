using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing AM Transaction record
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.AMTransactionLine)]
	[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMMTran : IBqlTable, ILSPrimary, IProdOper, IAMBatch, INotable
    {
	    internal string DebuggerDisplay => $"DocType = {DocType}, BatNbr = {BatNbr}, LineNbr = {LineNbr}, [{OrderType}:{ProdOrdID}:{OperationID}]";

        #region Keys
        public class PK : PrimaryKeyOf<AMMTran>.By<docType, batNbr, lineNbr>
        {
            public static AMMTran Find(PXGraph graph, string docType, string refNbr, int? lineNbr) =>
                FindBy(graph, docType, refNbr, lineNbr);
        }
        public static class FK
        {
            public class Batch : AMBatch.PK.ForeignKeyOf<AMMTran>.By<docType, batNbr> { }
            public class InventoryRegister : INRegister.PK.ForeignKeyOf<AMMTran>.By<iNDocType, iNBatNbr> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMMTran>.By<inventoryID> { }
            public class SubItem : INSubItem.PK.ForeignKeyOf<AMMTran>.By<subItemID> { }
            public class Site : INSite.PK.ForeignKeyOf<AMMTran>.By<siteID> { }
            public class Location : INLocation.PK.ForeignKeyOf<AMMTran>.By<locationID> { }
            public class INTran : PX.Objects.IN.INTran.PK.ForeignKeyOf<AMMTran>.By<iNDocType, iNBatNbr, iNLineNbr> { }
            public class ReasonCode : PX.Objects.CS.ReasonCode.PK.ForeignKeyOf<AMMTran>.By<reasonCodeID> { }
            public class OrigTran : AMMTran.PK.ForeignKeyOf<AMMTran>.By<origDocType, origBatNbr, origLineNbr> { }
        }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

	    protected bool? _Selected = false;
	    [PXBool]
	    [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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
        [Branch]
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
        #region DocType

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true)]
        [PXDefault(typeof(AMBatch.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true)]
        [PXUIField(DisplayName = "Batch Nbr", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDBDefault(typeof(AMBatch.batNbr))]
        [PXParent(typeof(Select<AMBatch, Where<AMBatch.docType, Equal<Current<AMMTran.docType>>, And<AMBatch.batNbr, Equal<Current<AMMTran.batNbr>>>>>))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXLineNbr(typeof(AMBatch.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
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
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [AMTranType.List]
        [PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
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
        #region LaborType
        public abstract class laborType : PX.Data.BQL.BqlString.Field<laborType> { }

        protected String _LaborType;
        [PXDBString(1, IsFixed = true)]
        [AMLaborType.List]
        [PXUIField(DisplayName = "Labor Type")]
        public virtual String LaborType
        {
            get
            {
                return this._LaborType;
            }
            set
            {
                this._LaborType = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        [PXFormula(typeof(Selector<AMMTran.prodOrdID, AMProdItem.orderType>))]
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
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr]
        [PXDefault]
        [ProductionOrderSelector(typeof(AMMTran.orderType), true)]
        [PXFormula(typeof(Validate<AMMTran.orderType>))]
        [PXRestrictor(typeof(Where<AMProdItem.hold, NotEqual<True>,
            And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>), 
            Messages.ProdStatusInvalidForProcess, typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID), typeof(AMProdItem.statusID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
	    #region OperationID
	    public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

	    protected int? _OperationID;
	    [OperationIDField]
	    [PXDefault(typeof(Search<
	        AMProdOper.operationID, 
	        Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>, 
	            And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>, 
	        OrderBy<
	            Asc<AMProdOper.operationCD>>>))]
        [PXSelector(typeof(Search<AMProdOper.operationID,
	            Where<AMProdOper.orderType, Equal<Current<AMMTran.orderType>>,
	                And<AMProdOper.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>),
	        SubstituteKey = typeof(AMProdOper.operationCD))]
	    [PXFormula(typeof(Validate<AMMTran.prodOrdID>))]
        public virtual int? OperationID
	    {
	        get
	        {
	            return this._OperationID;
	        }
	        set
	        {
	            this._OperationID = value;
	        }
	    }
	    #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDefault]
        [Inventory]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
        #region LaborCodeID

        public abstract class laborCodeID : PX.Data.BQL.BqlString.Field<laborCodeID> { }

        protected String _LaborCodeID;
        [PXDBString(15, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Labor Code")]
        public virtual String LaborCodeID
        {
            get
            {
                return this._LaborCodeID;
        }
            set
            {
                this._LaborCodeID = value;
            }
        }
        #endregion
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMMTran.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(
            typeof(AMMTran.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatus.inventoryID, Equal<Optional<AMMTran.inventoryID>>,
                And<INSiteStatus.siteID, Equal<Optional<AMMTran.siteID>>>>>>))]
        [PXFormula(typeof(Default<AMMTran.inventoryID>))]
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
        [PXDefault(typeof(Search<AMProdItem.siteID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>))]
        [SiteAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID))]
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
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(AMMTran.siteID), false, true, typeof(AMMTran.isScrap))]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
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
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMBatch.tranDate))]
        public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMMTran.uOM), typeof(AMMTran.baseQty), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal,"0.0")]
        [PXUIField(DisplayName = "Quantity")]
        [PXFormula(null, typeof(SumCalc<AMBatch.totalQty>))]
        public virtual Decimal? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region QtyScrapped
        public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

        protected Decimal? _QtyScrapped;
        [PXDBQuantity(typeof(AMMTran.uOM), typeof(AMMTran.baseQtyScrapped), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Scrapped")]
        public virtual Decimal? QtyScrapped
        {
            get
            {
                return this._QtyScrapped;
            }
            set
            {
                this._QtyScrapped = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMMTran.inventoryID>>>>))]
        [INUnit(typeof(AMMTran.inventoryID))]
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
        #region LotSerFG
        public abstract class lotSerFG : PX.Data.BQL.BqlString.Field<lotSerFG> { }

        protected String _LotSerFG;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent Lot/Serial Nbr", Visible = false, FieldClass = "LotSerial")]
        public virtual String LotSerFG
        {
            get
            {
                return this._LotSerFG;
            }
            set
            {
                this._LotSerFG = value;
            }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
            Search<INItemSite.tranUnitCost, Where<INItemSite.inventoryID, Equal<Current<AMMTran.inventoryID>>, And<INItemSite.siteID, Equal<Current<AMMTran.siteID>>>>>,
            Search<INItemCost.tranUnitCost, Where<INItemCost.inventoryID, Equal<Current<AMMTran.inventoryID>>>>>))]
        [PXUIField(DisplayName = "Unit Cost", Enabled=false)]
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
        #region TranAmt
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        protected Decimal? _TranAmt;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ext. Cost" , Enabled = false)] 
        [PXFormula(typeof(Mult<qty, unitCost>), typeof(SumCalc<AMBatch.totalAmount>))]
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
		#region AcctID
        public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        protected Int32? _AcctID;
        [Account]
        public virtual Int32? AcctID
		{
			get
			{
                return this._AcctID;
			}
			set
			{
                this._AcctID = value;
			}
		}
		#endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty")]
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
        #region BaseQtyScrapped
        public abstract class baseQtyScrapped : PX.Data.BQL.BqlDecimal.Field<baseQtyScrapped> { }

        protected Decimal? _BaseQtyScrapped;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty Scrapped")]
        public virtual Decimal? BaseQtyScrapped
        {
            get
            {
                return this._BaseQtyScrapped;
            }
            set
            {
                this._BaseQtyScrapped = value;
            }
        }
        #endregion
        #region Closeflg
        public abstract class closeflg : PX.Data.BQL.BqlBool.Field<closeflg> { }

        protected Boolean? _Closeflg;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Complete")]
        public virtual Boolean? Closeflg
        {
            get
            {
                return this._Closeflg;
            }
            set
            {
                this._Closeflg = value;
            }
        }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        protected Int32? _EmployeeID;
        [PXDBInt]
        [ProductionEmployeeSelector]
        [PXUIField(DisplayName = "Employee ID")]
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
		#region ExtCost
		public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

		protected Decimal? _ExtCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Labor Amount", Enabled=false)]
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
        #region GLBatNbr
        public abstract class gLBatNbr : PX.Data.BQL.BqlString.Field<gLBatNbr> { }

        protected String _GLBatNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "GL Batch Nbr", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>), ValidateValue = false)]
        public virtual String GLBatNbr
        {
            get
            {
                return this._GLBatNbr;
            }
            set
            {
                this._GLBatNbr = value;
            }
        }
        #endregion
        #region GLLineNbr
        public abstract class gLLineNbr : PX.Data.BQL.BqlInt.Field<gLLineNbr> { }

        protected Int32? _GLLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "GL Batch Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? GLLineNbr
        {
            get
            {
                return this._GLLineNbr;
            }
            set
            {
                this._GLLineNbr = value;
            }
        }
        #endregion
        #region INDocType

        public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

        protected String _INDocType;
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "IN Doc Type", Visible = false, Enabled = false)]
        [INDocType.List]
        public virtual String INDocType
        {
            get
            {
                return this._INDocType;
            }
            set
            {
                this._INDocType = value;
            }
        }
        #endregion
		#region INBatNbr
		public abstract class iNBatNbr : PX.Data.BQL.BqlString.Field<iNBatNbr> { }

		protected String _INBatNbr;
		[PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "IN Ref Nbr", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<AMMTran.iNDocType>>>>), ValidateValue = false)]
		public virtual String INBatNbr
		{
			get
			{
				return this._INBatNbr;
			}
			set
			{
				this._INBatNbr = value;
			}
		}
		#endregion
		#region INLineNbr
		public abstract class iNLineNbr : PX.Data.BQL.BqlInt.Field<iNLineNbr> { }

		protected Int32? _INLineNbr;
		[PXDBInt]
        [PXUIField(DisplayName = "IN Line Nbr", Visible = false, Enabled = false)]
		public virtual Int32? INLineNbr
		{
			get
			{
				return this._INLineNbr;
			}
			set
			{
				this._INLineNbr = value;
			}
		}
        #endregion
        #region ReceiptNbr
        /// <summary>
        /// Record the original receipt number for negative move adjustments
        /// </summary>
        public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }

        protected String _ReceiptNbr;
        /// <summary>
        /// Record the original receipt number for negative move adjustments
        /// </summary>
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Receipt Nbr.", Visible = false, Enabled = false)]
        [PXVerifySelector(typeof(Search2<INCostStatus.receiptNbr, 
            InnerJoin<INCostSubItemXRef, 
                On<INCostSubItemXRef.costSubItemID, Equal<INCostStatus.costSubItemID>>, 
            InnerJoin<INLocation, 
                On<INLocation.locationID, Equal<Optional<locationID>>>,
            LeftJoin<INRegister, 
                On<INRegister.docType, Equal<INDocType.receipt>,
                And<INRegister.refNbr, Equal<INCostStatus.receiptNbr>>>>>>, 
            Where<INCostStatus.inventoryID, Equal<Optional<inventoryID>>, 
                And<INCostSubItemXRef.subItemID, Equal<Optional<subItemID>>, 
                And<
                    Where<INCostStatus.costSiteID, Equal<Optional<siteID>>,
                        And<INLocation.isCosted, Equal<False>, 
                        Or<INCostStatus.costSiteID, Equal<Optional<locationID>>>>>>>>>), VerifyField = false)]
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
        #region StartTime

        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }

        protected DateTime? _StartTime;
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "Start Time")]
        public virtual DateTime? StartTime
        {
            get
            {
                return this._StartTime;
            }
            set
            {
                this._StartTime = value;
            }
        }
        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        protected DateTime? _EndTime;
        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "End Time")]
        public virtual DateTime? EndTime
        {
            get
            {
                return this._EndTime;
            }
            set
            {
                this._EndTime = value;
            }
        }
        #endregion
        #region LaborHrs Obsolete
	    [Obsolete("Use AMMTran.laborTime")]
        public abstract class laborHrs : PX.Data.BQL.BqlDecimal.Field<laborHrs> { }

        protected Decimal? _LaborHrs;
	    [Obsolete("Use AMMTran.LaborTime")]
        [PXDBDecimal(3)]
        [PXUIField(DisplayName = "Labor Hours", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Decimal? LaborHrs
        {
            get
            {
                return this._LaborHrs;
            }
            set
            {
                this._LaborHrs = value;
            }
        }
        #endregion
	    #region LaborTime
	    public abstract class laborTime : PX.Data.BQL.BqlInt.Field<laborTime> { }

	    protected Int32? _LaborTime;
	    [PXDBInt]
        [PXTimeList]
        [PXUIField(DisplayName = "Labor Time")]
	    public virtual Int32? LaborTime
        {
	        get
	        {
	            return this._LaborTime;
	        }
	        set
	        {
	            this._LaborTime = value;
	        }
	    }
	    #endregion
        #region LaborRate
        public abstract class laborRate : PX.Data.BQL.BqlDecimal.Field<laborRate> { }

        protected Decimal? _LaborRate;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Labor Rate", Enabled = false)]
        public virtual Decimal? LaborRate
        {
            get
            {
                return this._LaborRate;
            }
            set
            {
                this._LaborRate = value;
            }
        }
        #endregion
        #region LastOper
        public abstract class lastOper : PX.Data.BQL.BqlBool.Field<lastOper> { }

        protected Boolean? _LastOper;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Last Oper")]
        public virtual Boolean? LastOper
        {
            get
            {
                return this._LastOper;
            }
            set
            {
                this._LastOper = value;
            }
        }
        #endregion
		#region LotSerCntr
		public abstract class lotSerCntr : PX.Data.BQL.BqlInt.Field<lotSerCntr> { }

		protected Int32? _LotSerCntr;
		[PXDBInt]
        [PXDefault(0)]
		[PXUIField(DisplayName = "LotSerCntr")]
		public virtual Int32? LotSerCntr
		{
			get
			{
				return this._LotSerCntr;
			}
			set
			{
				this._LotSerCntr = value;
			}
		}
		#endregion
        #region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMMTran.inventoryID), typeof(AMMTran.subItemID), typeof(AMMTran.locationID), 
            PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region MatlLineId
		public abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

		protected Int32? _MatlLineId;
		[PXDBInt]
		[PXUIField(DisplayName = "Material Line ID")]
		public virtual Int32? MatlLineId
		{
			get
			{
				return this._MatlLineId;
			}
			set
			{
				this._MatlLineId = value;
			}
		}
		#endregion
		#region MultDiv
		public abstract class multDiv : PX.Data.BQL.BqlString.Field<multDiv> { }

		protected String _MultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName = "MultDiv")]
		public virtual String MultDiv
		{
			get
			{
				return this._MultDiv;
			}
			set
			{
				this._MultDiv = value;
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
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        protected String _FinPeriodID;
        [FinPeriodID(
            branchSourceType: typeof(AMMTran.branchID),
            masterFinPeriodIDType: typeof(AMMTran.tranPeriodID),
            headerMasterFinPeriodIDType: typeof(AMBatch.tranPeriodID))]
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

        protected String _TranPeriodID;
        [PeriodID]
        public virtual String TranPeriodID
		{
			get
			{
                return this._TranPeriodID;
			}
			set
			{
                this._TranPeriodID = value;
			}
		}
		#endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
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
        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

        protected String _ShiftID;
        [PXDBString(4, InputMask = "####")]
        [PXUIField(DisplayName = "Shift")]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        public virtual String ShiftID
        {
            get
            {
                return this._ShiftID;
            }
            set
            {
                this._ShiftID = value;
            }
        }
        #endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		protected Int32? _SubID;
        [SubAccount(typeof(AMMTran.acctID))]
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
		#region WIPAcctID
		public abstract class wIPAcctID : PX.Data.BQL.BqlInt.Field<wIPAcctID> { }

		protected Int32? _WIPAcctID;
		[PXDBInt]
        [PXDefault(typeof(Search<AMProdItem.wIPAcctID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Account")]
        [PXFormula(typeof(Default<AMMTran.prodOrdID>))]
        [PXFormula(typeof(Default<AMMTran.orderType>))]
        public virtual Int32? WIPAcctID
		{
			get
			{
                return this._WIPAcctID;
			}
			set
			{
                this._WIPAcctID = value;
			}
		}
		#endregion
		#region WIPSubID
		public abstract class wIPSubID : PX.Data.BQL.BqlInt.Field<wIPSubID> { }

		protected Int32? _WIPSubID;
		[PXDBInt]
        [PXDefault(typeof(Search<AMProdItem.wIPSubID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Subaccount")]
        [PXFormula(typeof(Default<AMMTran.prodOrdID>))]
        [PXFormula(typeof(Default<AMMTran.orderType>))]
        public virtual Int32? WIPSubID
		{
			get
			{
                return this._WIPSubID;
			}
			set
			{
                this._WIPSubID = value;
			}
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp]
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
        #region UnassignedQty

        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        protected Decimal? _UnassignedQty;
        [PXDBQuantity]
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
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Multiplier")]
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
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMMTran.inventoryID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
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
		#region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Tran Description", Visible = false)]
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
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
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
        [PXDBCreatedByScreenID]
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
        [PXDBCreatedDateTime]
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
        [PXDBLastModifiedByID]
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
        [PXDBLastModifiedByScreenID]
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
        [PXDBLastModifiedDateTime]
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
        #region HasReference
        public abstract class hasReference : PX.Data.BQL.BqlBool.Field<hasReference> { }

        [PXBool]
        public virtual Boolean? HasReference
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.INBatNbr) || !string.IsNullOrWhiteSpace(this.GLBatNbr);
            }
        }
        #endregion
        #region OrigDocType
        public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        protected String _OrigDocType;
        [PXDBString(1, IsFixed = true)]
        [AMDocType.List]
        [PXUIField(DisplayName = "Orig Doc Type", Visible = true, Enabled = false)]
        public virtual String OrigDocType
        {
            get
            {
                return this._OrigDocType;
            }
            set
            {
                this._OrigDocType = value;
            }
        }
        #endregion
        #region OrigBatNbr
        public abstract class origBatNbr : PX.Data.BQL.BqlString.Field<origBatNbr> { }

        protected String _OrigBatNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Orig Batch Nbr", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<AMBatch.origDocType>>>), ValidateValue = false)]
        public virtual String OrigBatNbr
        {
            get
            {
                return this._OrigBatNbr;
            }
            set
            {
                this._OrigBatNbr = value;
            }
        }
        #endregion
        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }

        protected Int32? _OrigLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "Orig Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
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
        #region IsByproduct
        public abstract class isByproduct : PX.Data.BQL.BqlBool.Field<isByproduct> { }

	    protected Boolean? _IsByproduct;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "By-product", Enabled = false, Visible = false)]
        public virtual Boolean? IsByproduct
        {
            get
            {
                return this._IsByproduct;
            }
            set
            {
                this._IsByproduct = value;
            }
        }
        #endregion
	    #region IsStockItem
	    [PXFormula(typeof (Selector<AMMTran.inventoryID, InventoryItem.stkItem>))]
	    [PXBool]
	    [PXUIField(DisplayName = "Is stock", Enabled = false, Visibility = PXUIVisibility.Invisible, Visible = false)]
	    public virtual bool? IsStockItem { get; set; }
        #endregion
	    #region ProjectID
	    public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

	    protected Int32? _ProjectID;
        [ProjectBase]
        [ProjectDefault(BatchModule.IN,typeof(Search<AMProdItem.projectID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>, 
            And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>))]
	    [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
	    [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
	    [PXRestrictor(typeof(Where<PMProject.visibleInIN, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
        [PXFormula(typeof(Default<AMMTran.prodOrdID>))]
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
        [PXDefault(typeof(Search<AMProdItem.taskID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMMTran.projectID>))]
        [BaseProjectTask(typeof(AMMTran.projectID))]
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
        [PXDBInt]
        [PXUIField(DisplayName = "Cost Code", FieldClass = "COSTCODE")]
        [PXDefault(typeof(Search<AMProdItem.costCodeID, Where<AMProdItem.orderType, Equal<Current<AMMTran.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMMTran.prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMMTran.taskID>))]
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
        #region TimeCardStatus
        public abstract class timeCardStatus : PX.Data.BQL.BqlInt.Field<timeCardStatus> { }

        protected Int32? _TimeCardStatus;
        [PXDBInt]
        [PXUIField(DisplayName = "Time Card Status")]
        [TimeCardStatus.List]
        public virtual Int32? TimeCardStatus
        {
            get
            {
                return this._TimeCardStatus;
            }
            set
            {
                this._TimeCardStatus = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "LineCntrAttribute")]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion
        #region HasMixedProjectTasks
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        protected bool? _HasMixedProjectTasks;
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
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
        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlInt.Field<scrapAction> { }

        protected int? _ScrapAction;
        [PXDBInt]
        [PXUIField(DisplayName = "Scrap Action", Enabled = true, Visible = false)]
        [ScrapAction.List]
        public virtual int? ScrapAction
        {
            get
            {
                return this._ScrapAction;
            }
            set
            {
                this._ScrapAction = value;
            }
        }
        #endregion
        #region ReasonCodeID
        public abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID> { }


        protected String _ReasonCodeID;
        [PXDBString(20, InputMask = ">aaaaaaaaaaaaaaaaaaaa")]
        [PXUIField(DisplayName = "Reason Code")]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID,
            Where<ReasonCode.usage, Equal<ReasonCodeExt.production>>>))]
        public virtual String ReasonCodeID
        {
            get
            {
                return this._ReasonCodeID;
            }
            set
            {
                this._ReasonCodeID = value;
            }
        }
        #endregion
	    #region IsScrap
	    public abstract class isScrap : PX.Data.BQL.BqlBool.Field<isScrap> { }

	    protected Boolean? _IsScrap;
	    [PXDBBool]
	    [PXDefault(false)]
	    [PXUIField(DisplayName = "Qty is Scrap", Visible = false)]
	    public virtual Boolean? IsScrap
	    {
	        get
	        {
	            return this._IsScrap;
	        }
	        set
	        {
	            this._IsScrap = value;
	        }
	    }
        #endregion
	    #region TranOverride
	    public abstract class tranOverride : PX.Data.BQL.BqlBool.Field<tranOverride> { }

	    protected Boolean? _TranOverride;
	    [PXDBBool]
	    [PXDefault(false)]
	    [PXUIField(DisplayName = "Override")]
	    public virtual Boolean? TranOverride
        {
	        get
	        {
	            return this._TranOverride;
	        }
	        set
	        {
	            this._TranOverride = value;
	        }
	    }
        #endregion
        #region SubcontractSource
        public abstract class subcontractSource : PX.Data.BQL.BqlInt.Field<subcontractSource> { }

        protected int? _SubcontractSource;
        [PXDBInt]
        [PXUIField(DisplayName = "Subcontract Source", Enabled = false)]
        [AMSubcontractSource.List]
        public virtual int? SubcontractSource
        {
            get
            {
                return this._SubcontractSource;
            }
            set
            {
                this._SubcontractSource = value;
            }
        }
        #endregion
        #region ReferenceCostID
        /// <summary>
        /// Cost ID value such as Tool ID, Overhead ID, Machine ID, etc.
        /// </summary>
        public abstract class referenceCostID : PX.Data.BQL.BqlString.Field<referenceCostID> { }

        protected String _ReferenceCostID;
        /// <summary>
        /// Cost ID value such as Tool ID, Overhead ID, Machine ID, etc.
        /// </summary>
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Ref. Cost ID", Enabled = false)]
        public virtual String ReferenceCostID
        {
            get
            {
                return this._ReferenceCostID;
            }
            set
            {
                this._ReferenceCostID = value;
            }
        }
        #endregion

        #region Methods

        public static implicit operator AMMTranSplit(AMMTran item)
        {
            AMMTranSplit ret = new AMMTranSplit();
            ret.DocType = item.DocType;
            ret.TranType = item.TranType;
            ret.BatNbr = item.BatNbr;
            ret.LineNbr = item.LineNbr;
            ret.SplitLineNbr = (int)1;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ExpireDate = item.ExpireDate;
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault());
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault());
            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }

        public static implicit operator AMMTran(AMMTranSplit item)
        {   
            AMMTran ret = new AMMTran();
            ret.DocType = item.DocType;
            ret.TranType = item.TranType;
            ret.BatNbr = item.BatNbr;
            ret.LineNbr = item.LineNbr;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;

            //Split recs show a positive qty - ammtran shows a positive or negative qty
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault()) * (item.InvtMult * -1);
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault()) * (item.InvtMult * -1);

            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }
        
        public static bool IsSameOrigLine(AMMTran tran1, AMMTran tran2)
	    {
	        return tran1.OrigDocType == tran2.OrigDocType &&
	               tran1.OrigBatNbr == tran2.OrigBatNbr &&
	               tran1.OrigLineNbr == tran2.OrigLineNbr;
	    }
        
        #endregion
    }

    /// <summary>
    /// Projection of <see cref="AMMTran"/> used to lookup unreleased WIP amounts by order operation.
    /// Uses index AMMTran_IX_UnreleasedWipByOperation
    /// </summary>
    [PXHidden]
    [Serializable]
#if DEBUG
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
#endif
    [PXProjection(typeof(Select4<
        AMMTran,
        Where<AMMTran.released, Equal<False>,
            And<AMMTran.tranType, NotEqual<AMTranType.operWIPComplete>,
                /* Excluding Actual Labor because the operation at the time this query is needed will already have the Actual labor amounts calculated */
            And<AMMTran.tranType, NotEqual<AMTranType.labor>,
            And<AMMTran.docType, NotEqual<AMDocType.wipAdjust>>>>>,
        Aggregate<
            Sum<AMMTran.tranAmt,
            GroupBy<AMMTran.origDocType,
            GroupBy<AMMTran.origBatNbr,
            GroupBy<AMMTran.orderType,
            GroupBy<AMMTran.prodOrdID,
            GroupBy<AMMTran.operationID>>>>>>>>), Persistent = false)]
    public class AMMTranUnreleasedWipByOperation : IBqlTable
    {
#if DEBUG
        internal string DebuggerDisplay => $"OrigDocType = {OrigDocType}, OrigBatNbr = {OrigBatNbr}, OrderType = {OrderType}, ProdOrdID = {ProdOrdID}, OperationID = {OperationID}, TranAmt = {TranAmt}";
#endif
        #region OrigDocType (IsKey/GroupBy)
        public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        protected String _OrigDocType;
        [PXDBString(1, IsKey = true, IsFixed = true, BqlField = typeof(AMMTran.origDocType))]
        [PXUIField(DisplayName = "Orig Doc Type", Visible = true, Enabled = false)]
        public virtual String OrigDocType
        {
            get
            {
                return this._OrigDocType;
            }
            set
            {
                this._OrigDocType = value;
            }
        }
        #endregion
        #region OrigBatNbr (IsKey/GroupBy)
        public abstract class origBatNbr : PX.Data.BQL.BqlString.Field<origBatNbr> { }

        protected String _OrigBatNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, BqlField = typeof(AMMTran.origBatNbr))]
        [PXUIField(DisplayName = "Orig Batch Nbr", Enabled = false)]
        public virtual String OrigBatNbr
        {
            get
            {
                return this._OrigBatNbr;
            }
            set
            {
                this._OrigBatNbr = value;
            }
        }
        #endregion
        #region OrderType (IsKey/GroupBy)
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, BqlField = typeof(AMMTran.orderType))]
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
        #region ProdOrdID (IsKey/GroupBy)
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, BqlField = typeof(AMMTran.prodOrdID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region OperationID  (IsKey/GroupBy)
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(IsKey = true, BqlField = typeof(AMMTran.operationID))]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion

        #region TranAmt (Sum)
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        protected Decimal? _TranAmt;
        [PXDBBaseCury(BqlField = typeof(AMMTran.tranAmt))]
        [PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
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
    }

    //Created for improved SQL lookup when matching the transaction to production queries
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select<AMMTran>), Persistent = false)]
    public class AMMTranProdMatl : IBqlTable
    {
        #region DocType

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMMTran.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMMTran.batNbr))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(AMMTran.lineNbr))]
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
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(BqlField = typeof(AMMTran.orderType))]
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
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(BqlField = typeof(AMMTran.prodOrdID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(BqlField = typeof(AMMTran.operationID))]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region MatlLineId
        public abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

        protected Int32? _MatlLineId;
        [PXDBInt(BqlField = typeof(AMMTran.matlLineId))]
        public virtual Int32? MatlLineId
        {
            get
            {
                return this._MatlLineId;
            }
            set
            {
                this._MatlLineId = value;
            }
        }
        #endregion
    }
}
