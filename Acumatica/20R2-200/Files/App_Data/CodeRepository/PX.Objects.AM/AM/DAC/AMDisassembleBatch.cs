using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(DisassemblyEntry))]
    [PXCacheName(Messages.AMDisassembleBatch)]
    [PXProjection(typeof(Select2<AMBatch,
        LeftJoin<AMMTran, On<AMBatch.refLineNbr, Equal<AMMTran.lineNbr>,
            And<AMBatch.docType, Equal<AMMTran.docType>,
                And<AMBatch.batNbr, Equal<AMMTran.batNbr>>>>>>), Persistent = true)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMDisassembleBatch : IBqlTable, ILSPrimary, IAMBatch, INotable
    {
        internal string DebuggerDisplay => $"BatNbr = {BatchNbr}, Production Order = {OrderType} {ProdOrdID}, OperationID = {OperationID}, Qty = {Qty} {UOM}";

        #region AMBatch Fields
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        protected Int32? _BranchID;
        [Branch(BqlField = typeof(AMBatch.branchID), Enabled = false)]
        [PXDependsOnFields(typeof(AMDisassembleBatch.tranBranchID))]
        public virtual Int32? BranchID
        {
            get { return this._BranchID; }
            set { this._BranchID = value; }
        }
        #endregion
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsKey = true, IsFixed = true, BqlField = typeof(AMBatch.docType))]
        [PXDefault(AMDocType.Disassembly)]
        [AMDocType.List]
        [PXUIField(DisplayName = "Document Type", Enabled = false, Visible = false)]
        [PXDependsOnFields(typeof(AMDisassembleBatch.tranDocType))]
        public virtual String DocType
        {
            get { return this._DocType; }
            set { this._DocType = value; }
        }
        #endregion
        #region BatchNbr
        public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

        protected String _BatchNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(AMBatch.batNbr))]
        [PXUIField(DisplayName = "Batch Nbr", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMDisassembleBatch.batchNbr, Where<AMDisassembleBatch.docType, Equal<AMDocType.disassembly>>>), Filterable = true)]
        [AMDocType.Numbering(typeof(AMDisassembleBatch.docType), typeof(AMDisassembleBatch.tranDate))]
        [PXFieldDescription]
        [PXDependsOnFields(typeof(AMDisassembleBatch.tranBatchNbr))]
        public virtual String BatchNbr
        {
            get { return this._BatchNbr; }
            set { this._BatchNbr = value; }
        }

        //Satisfies IAMBatch
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Bat Nbr", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual string BatNbr
        {
            get { return this._BatchNbr; }
            set { this._BatchNbr = value; }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected bool? _Released;
        [PXDBBool(BqlField = typeof(AMBatch.released))]
        [PXDefault(false)]
        public virtual bool? Released
        {
            get { return this._Released; }
            set
            {
                this._Released = value;
#pragma warning disable PX1032 // DAC properties cannot contain method invocations
                // Same implementation as found in INRegister
                this.SetStatus();
#pragma warning restore PX1032 // DAC properties cannot contain method invocations
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        protected Boolean? _Hold;
        [PXDBBool(BqlField = typeof(AMBatch.hold))]
        [PXDefault(typeof(AMPSetup.holdEntry))]
        [PXUIField(DisplayName = "Hold")]
        public virtual Boolean? Hold
        {
            get { return this._Hold; }
            set
            {
                this._Hold = value;
#pragma warning disable PX1032 // DAC properties cannot contain method invocations
                // Same implementation as found in INRegister
                this.SetStatus();
#pragma warning restore PX1032 // DAC properties cannot contain method invocations
            }
        }
        #endregion
        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }

        protected int? _LineCntr;
        [PXDBInt(BqlField = typeof(AMBatch.lineCntr))]
        [PXDefault(1)]
        public virtual int? LineCntr
        {
            get { return this._LineCntr; }
            set { this._LineCntr = value; }
        }
        #endregion
        #region RefLineNbr
        public abstract class refLineNbr : PX.Data.BQL.BqlInt.Field<refLineNbr> { }

        protected Int32? _RefLineNbr;
        [PXDBInt(BqlField = typeof(AMBatch.refLineNbr))]
        [PXUIField(DisplayName = "Ref Line Nbr.", Visible = false, Enabled = false)]
        [PXDefault(0)]
        [PXDependsOnFields(typeof(AMDisassembleBatch.lineNbr))]
        public virtual Int32? RefLineNbr
        {
            get
            {
                return this._RefLineNbr;
            }
            set
            {
                this._RefLineNbr = value;
            }
        }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

        protected String _Status;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMBatch.status))]
        [PXDefault(DocStatus.Hold)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [DocStatus.List]
        [PXDependsOnFields(typeof(AMDisassembleBatch.released), typeof(AMDisassembleBatch.hold))]
        public virtual String Status
        {
            get { return this._Status; }
            set { }
        }
        #endregion
        #region Date
        public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }

        protected DateTime? _Date;
        [PXDBDate(BqlField = typeof(AMBatch.tranDate))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDependsOnFields(new System.Type[] {typeof(AMDisassembleBatch.tranDate)})]
        public virtual DateTime? Date
        {
            get { return this._Date; }
            set { this._Date = value; }
        }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(AMBatch.tranDesc))]
        [PXDefault(Messages.ProdEntry_Disassembly, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description")]
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
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        protected String _FinPeriodID;
        [INOpenPeriod(
            sourceType: typeof(AMDisassembleBatch.tranDate),
            branchSourceType: typeof(AMDisassembleBatch.branchID),
            masterFinPeriodIDType: typeof(AMDisassembleBatch.tranPeriodID),
            IsHeader = true, BqlField = typeof(AMBatch.finPeriodID))]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDependsOnFields(new System.Type[] {typeof(AMDisassembleBatch.tranFinPeriodID)})]
        public virtual String FinPeriodID
        {
            get { return this._FinPeriodID; }
            set { this._FinPeriodID = value; }
        }
        #endregion
        #region TranPeriodID
        public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

        protected String _TranPeriodID;
        [PeriodID(BqlField = typeof(AMBatch.tranPeriodID))]
        public virtual String TranPeriodID
        {
            get { return this._TranPeriodID; }
            set { this._TranPeriodID = value; }
        }
        #endregion
        #region ControlAmount
        public abstract class controlAmount : PX.Data.BQL.BqlDecimal.Field<controlAmount> { }

        protected Decimal? _ControlAmount;
        [PXDBBaseCury(BqlField = typeof(AMBatch.controlAmount))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Amount")]
        public virtual Decimal? ControlAmount
        {
            get { return this._ControlAmount; }
            set { this._ControlAmount = value; }
        }
        #endregion
        #region ControlQty
        public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }

        protected Decimal? _ControlQty;
        [PXDBQuantity(BqlField = typeof(AMBatch.controlQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Qty.")]
        public virtual Decimal? ControlQty
        {
            get { return this._ControlQty; }
            set { this._ControlQty = value; }
        }
        #endregion
        #region ControlCost
        public abstract class controlCost : PX.Data.BQL.BqlDecimal.Field<controlCost> { }

        protected Decimal? _ControlCost;
        [PXDBBaseCury(BqlField = typeof(AMBatch.controlCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Cost")]
        public virtual Decimal? ControlCost
        {
            get { return this._ControlCost; }
            set { this._ControlCost = value; }
        }
        #endregion
        #region TotalAmount
        public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }

        protected Decimal? _TotalAmount;
        [PXDBBaseCury(BqlField = typeof(AMBatch.totalAmount))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? TotalAmount
        {
            get { return this._TotalAmount; }
            set { this._TotalAmount = value; }
        }
        #endregion
        #region TotalQty
        public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }

        protected Decimal? _TotalQty;
        [PXDBQuantity(BqlField = typeof(AMBatch.totalQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? TotalQty
        {
            get { return this._TotalQty; }
            set { this._TotalQty = value; }
        }
        #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }

        protected Decimal? _TotalCost;
        [PXDBBaseCury(BqlField = typeof(AMBatch.totalCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? TotalCost
        {
            get { return this._TotalCost; }
            set { this._TotalCost = value; }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(DescriptionField = typeof(AMDisassembleBatch.batchNbr), BqlField = typeof(AMBatch.noteID))]
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
        #region OrigBatNbr
        public abstract class origBatNbr : PX.Data.BQL.BqlString.Field<origBatNbr> { }

        protected String _OrigBatNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(AMBatch.origBatNbr))]
        [PXUIField(DisplayName = "Orig Batch Nbr", Visibility = PXUIVisibility.Visible, Enabled = false)]
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
        #region OrigDocType
        public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }

        protected String _OrigDocType;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMBatch.origDocType))]
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
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(BqlField = typeof(AMBatch.createdByID))]
        public virtual Guid? CreatedByID
        {
            get { return this._CreatedByID; }
            set { this._CreatedByID = value; }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        protected string _CreatedByScreenID;
        [PXDBCreatedByScreenID(BqlField = typeof(AMBatch.createdByScreenID))]
        public virtual string CreatedByScreenID
        {
            get { return this._CreatedByScreenID; }
            set { this._CreatedByScreenID = value; }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime(BqlField = typeof(AMBatch.createdDateTime))]
        public virtual DateTime? CreatedDateTime
        {
            get { return this._CreatedDateTime; }
            set { this._CreatedDateTime = value; }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID(BqlField = typeof(AMBatch.lastModifiedByID))]
        public virtual Guid? LastModifiedByID
        {
            get { return this._LastModifiedByID; }
            set { this._LastModifiedByID = value; }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected string _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID(BqlField = typeof(AMBatch.lastModifiedByScreenID))]
        public virtual string LastModifiedByScreenID
        {
            get { return this._LastModifiedByScreenID; }
            set { this._LastModifiedByScreenID = value; }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime(BqlField = typeof(AMBatch.lastModifiedDateTime))]
        public virtual DateTime? LastModifiedDateTime
        {
            get { return this._LastModifiedDateTime; }
            set { this._LastModifiedDateTime = value; }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp(BqlField = typeof(AMBatch.Tstamp))]
        public virtual Byte[] tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }
        #endregion
        #endregion
        #region AMMTran Fields
        #region TranBranchID
        public abstract class tranBranchID : PX.Data.BQL.BqlInt.Field<tranBranchID> { }

        [PXDBInt(BqlField = typeof(AMMTran.branchID))]
        public virtual int? TranBranchID
        {
            get { return this._BranchID; }
            set { this._BranchID = value; }
        }
        #endregion
        #region TranDocType
        public abstract class tranDocType : PX.Data.BQL.BqlString.Field<tranDocType> { }

        [PXDBString(1, IsFixed = true, BqlField = typeof(AMMTran.docType))]
        [PXDefault]
        [PXRestriction]
        public virtual String TranDocType
        {
            get { return this._DocType; }
            set { this._DocType = value; }
        }
        #endregion
        #region TranBatchNbr
        public abstract class tranBatchNbr : PX.Data.BQL.BqlString.Field<tranBatchNbr> { }

        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMMTran.batNbr))]
        [PXDefault]
        [PXRestriction]
        public virtual String TranBatchNbr
        {
            get { return this._BatchNbr; }
            set { this._BatchNbr = value; }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(BqlField = typeof(AMMTran.lineNbr))]
        [PXDefault]
        [PXRestriction]
        public virtual Int32? LineNbr
        {
            get { return this._RefLineNbr; }
            set { this._RefLineNbr = value; }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected string _OrderType;
        [PXDefault(typeof(AMPSetup.defaultDisassembleOrderType))]
        [AMOrderTypeField(BqlField = typeof(AMMTran.orderType))]
        [PXRestrictor(typeof(Where<AMOrderType.function, Equal<OrderTypeFunction.disassemble>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        [PXFormula(typeof(Selector<AMDisassembleBatch.prodOrdID, AMProdItem.orderType>))]
        public virtual string OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected string _ProdOrdID;
        [ProductionNbr(BqlField = typeof(AMMTran.prodOrdID))]
        [PXDefault]
        [ProductionOrderSelector(typeof(AMDisassembleBatch.orderType))]
        [PXFormula(typeof(Validate<AMDisassembleBatch.orderType>))]
        public virtual string ProdOrdID
        {
            get { return this._ProdOrdID; }
            set { this._ProdOrdID = value; }
        }
        #endregion
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        [PXDBDate(BqlField = typeof(AMMTran.tranDate))]
        public virtual DateTime? TranDate
        {
            get { return this._Date; }
            set { this._Date = value; }
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true, BqlField = typeof(AMMTran.tranType))]
        [AMTranType.DisassembleBatchList]
        [PXDefault(typeof(AMTranType.disassembly))]
        [PXUIField(DisplayName = "Tran. Type")]
        public virtual String TranType
        {
            get { return this._TranType; }
            set { this._TranType = value; }
        }
        #endregion
        #region TranAmt
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        protected Decimal? _TranAmt;
        [PXDBBaseCury(BqlField = typeof(AMMTran.tranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
        [PXFormula(typeof(Mult<qty, unitCost>))]
        public virtual Decimal? TranAmt
        {
            get { return this._TranAmt; }
            set { this._TranAmt = value; }
        }
        #endregion
        #region TranFinPeriodID
        public abstract class tranFinPeriodID : PX.Data.BQL.BqlString.Field<tranFinPeriodID> { }

        [FinPeriodID(BqlField = typeof(AMMTran.finPeriodID))]
#pragma warning disable PX1030 // Consider setting PersistingCheck to Nothing or replacing PXDefault with PXUnboundDefault
        [PXDefault]
#pragma warning restore PX1030
        public virtual string TranFinPeriodID
        {
            get { return this._FinPeriodID; }
            set { this._FinPeriodID = value; }
        }
        #endregion
        #region TranTranPeriodID
        public abstract class tranTranPeriodID : PX.Data.BQL.BqlString.Field<tranTranPeriodID> { }

        protected String _TranTranPeriodID;
        [FinPeriodID(BqlField = typeof(AMMTran.tranPeriodID))]
        public virtual String TranTranPeriodID
        {
            get { return this._TranPeriodID; }
            set { this._TranPeriodID = value; }
        }
        #endregion
        #region Closeflg
        public abstract class closeflg : PX.Data.BQL.BqlBool.Field<closeflg> { }

        protected Boolean? _Closeflg;
        [PXDBBool(BqlField = typeof(AMMTran.closeflg))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Complete", Visible = false)]
        public virtual Boolean? Closeflg
        {
            get { return this._Closeflg; }
            set { this._Closeflg = value; }
        }
        #endregion
        #region TranNoteID
        public abstract class tranNoteID : PX.Data.BQL.BqlGuid.Field<tranNoteID> { }

        protected Guid? _TranNoteID;
        [PXNote(BqlField = typeof(AMMTran.noteID))]
        public virtual Guid? TranNoteID
        {
            get { return this._TranNoteID; }
            set { this._TranNoteID = value; }
        }
        #endregion
        #region TranReleased
        public abstract class tranReleased : PX.Data.BQL.BqlBool.Field<tranReleased> { }

        protected Boolean? _TranReleased;
        [PXDBBool(BqlField = typeof(AMMTran.released))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
        public virtual Boolean? TranReleased
        {
            get { return this._TranReleased; }
            set { this._TranReleased = value; }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDefault(typeof(Search<AMProdItem.inventoryID,
                Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>,
                    And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [Inventory(BqlField = typeof(AMMTran.inventoryID), Enabled = false)]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        public virtual Int32? InventoryID
        {
            get { return this._InventoryID; }
            set { this._InventoryID = value; }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<AMProdItem.subItemID, 
            Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [SubItem(
            typeof(AMDisassembleBatch.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                    And<INSiteStatus.inventoryID, Equal<Optional<AMDisassembleBatch.inventoryID>>,
                        And<INSiteStatus.siteID, Equal<Optional<AMDisassembleBatch.siteID>>>>>>),
            Enabled = false, BqlField = typeof(AMMTran.subItemID))]
        [PXFormula(typeof(Default<AMDisassembleBatch.inventoryID, AMDisassembleBatch.prodOrdID>))]
        public virtual Int32? SubItemID
        {
            get { return this._SubItemID; }
            set { this._SubItemID = value; }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(AMMTran.tranDesc))]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Selector<AMDisassembleBatch.inventoryID, InventoryItem.descr>))]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [SiteAvail(typeof(AMDisassembleBatch.inventoryID), typeof(AMDisassembleBatch.subItemID), 
            BqlField = typeof(AMMTran.siteID))]
        [PXDefault(typeof(Search<AMProdItem.siteID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, 
            And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(AMDisassembleBatch.inventoryID), typeof(AMDisassembleBatch.subItemID), typeof(AMDisassembleBatch.siteID),
            false, true, BqlField = typeof(AMMTran.locationID))]
        [PXDefault(typeof(Search<AMProdItem.locationID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        public virtual Int32? LocationID
        {
            get { return this._LocationID; }
            set { this._LocationID = value; }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit(typeof(AMDisassembleBatch.inventoryID), BqlField = typeof(AMMTran.uOM))]
        [PXDefault(typeof(Search<AMProdItem.uOM, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        public virtual String UOM
        {
            get { return this._UOM; }
            set { this._UOM = value; }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXDBPriceCost(BqlField = typeof(AMMTran.unitCost))]
        [MatlUnitCostDefault(
            typeof(AMDisassembleBatch.inventoryID),
            typeof(AMDisassembleBatch.siteID),
            typeof(AMDisassembleBatch.uOM))]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        [PXFormula(typeof(Default<AMDisassembleBatch.inventoryID, AMDisassembleBatch.siteID, AMDisassembleBatch.uOM>))]
        public virtual Decimal? UnitCost
        {
            get { return this._UnitCost; }
            set { this._UnitCost = value; }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMDisassembleBatch.uOM), typeof(AMDisassembleBatch.baseQty), HandleEmptyKey = true,
            BqlField = typeof(AMMTran.qty))]
        [PXDefault(typeof(Search<AMProdItem.qtyRemaining,
            Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "Quantity")]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        public virtual Decimal? Qty
        {
            get { return this._Qty; }
            set { this._Qty = value; }
        }
        #endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity(BqlField = typeof(AMMTran.baseQty))]
        public virtual Decimal? BaseQty
        {
            get { return this._BaseQty; }
            set { this._BaseQty = value; }
        }
        #endregion
        #region InvtMult
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort(BqlField = typeof(AMMTran.invtMult))]
        [PXDefault((short) 0)]
        [PXUIField(DisplayName = "Batch Multiplier", Visible = false, Enabled = false)]
        [PXFormula(typeof(Default<AMDisassembleBatch.tranType, AMDisassembleBatch.inventoryID, AMDisassembleBatch.uOM>))]
        public virtual Int16? InvtMult
        {
            get { return this._InvtMult; }
            set { this._InvtMult = value; }
        }
        #endregion
        #region UnassignedQty
        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        protected Decimal? _UnassignedQty;

        [PXDBQuantity(BqlField = typeof(AMMTran.unassignedQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnassignedQty
        {
            get { return this._UnassignedQty; }
            set { this._UnassignedQty = value; }
        }
        #endregion
        #region MatlLineId
        public abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

        protected Int32? _MatlLineId;
        [PXDBInt(BqlField = typeof(AMMTran.matlLineId))]
        [PXUIField(DisplayName = "Material Line ID")]
        public virtual Int32? MatlLineId
        {
            get { return this._MatlLineId; }
            set { this._MatlLineId = value; }
        }
        #endregion
        #region MultDiv
        public abstract class multDiv : PX.Data.BQL.BqlString.Field<multDiv> { }

        protected String _MultDiv;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMMTran.multDiv))]
        [PXDefault("M")]
        [PXUIField(DisplayName = "MultDiv")]
        public virtual String MultDiv
        {
            get { return this._MultDiv; }
            set { this._MultDiv = value; }
        }
        #endregion
        #region AcctID
        public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        protected Int32? _AcctID;
        [Account(BqlField = typeof(AMMTran.acctID))]
        public virtual Int32? AcctID
        {
            get { return this._AcctID; }
            set { this._AcctID = value; }
        }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        protected Int32? _SubID;
        [SubAccount(typeof(AMDisassembleBatch.acctID), BqlField = typeof(AMMTran.subID))]
        public virtual Int32? SubID
        {
            get { return this._SubID; }
            set { this._SubID = value; }
        }
        #endregion
        #region WIPAcctID
        public abstract class wIPAcctID : PX.Data.BQL.BqlInt.Field<wIPAcctID> { }

        protected Int32? _WIPAcctID;
        [PXDBInt(BqlField = typeof(AMMTran.wIPAcctID))]
        [PXDefault(typeof(Search<AMProdItem.wIPAcctID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, 
            And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Account")]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.orderType>))]
        public virtual Int32? WIPAcctID
        {
            get { return this._WIPAcctID; }
            set { this._WIPAcctID = value; }
        }
        #endregion
        #region WIPSubID
        public abstract class wIPSubID : PX.Data.BQL.BqlInt.Field<wIPSubID> { }

        protected Int32? _WIPSubID;
        [PXDBInt(BqlField = typeof(AMMTran.wIPSubID))]
        [PXDefault(typeof(Search<AMProdItem.wIPSubID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Subaccount")]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.orderType>))]
        public virtual Int32? WIPSubID
        {
            get { return this._WIPSubID; }
            set { this._WIPSubID = value; }
        }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        protected Decimal? _ExtCost;
        [PXDBBaseCury(BqlField = typeof(AMMTran.extCost))]
        [PXDefault(TypeCode.Decimal, "0.0")] //Labor Cost
        public virtual Decimal? ExtCost
        {
            get { return this._ExtCost; }
            set { this._ExtCost = value; }
        }
        #endregion
        #region GLBatNbr
        public abstract class gLBatNbr : PX.Data.BQL.BqlString.Field<gLBatNbr> { }

        protected String _GLBatNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMMTran.gLBatNbr))]
        [PXUIField(DisplayName = "GL Batch Nbr", Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.moduleGL>>>), ValidateValue =
            false)]
        public virtual String GLBatNbr
        {
            get { return this._GLBatNbr; }
            set { this._GLBatNbr = value; }
        }

        #endregion
        #region GLLineNbr
        public abstract class gLLineNbr : PX.Data.BQL.BqlInt.Field<gLLineNbr> { }

        protected Int32? _GLLineNbr;
        [PXDBInt(BqlField = typeof(AMMTran.gLLineNbr))]
        [PXUIField(DisplayName = "GL Batch Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? GLLineNbr
        {
            get { return this._GLLineNbr; }
            set { this._GLLineNbr = value; }
        }
        #endregion
        #region INDocType
        public abstract class iNDocType : PX.Data.BQL.BqlString.Field<iNDocType> { }

        protected String _INDocType;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMMTran.iNDocType))]
        [PXUIField(DisplayName = "IN Doc Type", Enabled = false)]
        [INDocType.List]
        public virtual String INDocType
        {
            get { return this._INDocType; }
            set { this._INDocType = value; }
        }
        #endregion
        #region INBatNbr
        public abstract class iNBatNbr : PX.Data.BQL.BqlString.Field<iNBatNbr> { }

        protected String _INBatNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMMTran.iNBatNbr))]
        [PXUIField(DisplayName = "IN Ref Nbr", Enabled = false)]
        [PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<AMDisassembleBatch.iNDocType>>>>),
            ValidateValue = false)]
        public virtual String INBatNbr
        {
            get { return this._INBatNbr; }
            set { this._INBatNbr = value; }
        }
        #endregion
        #region INLineNbr
        public abstract class iNLineNbr : PX.Data.BQL.BqlInt.Field<iNLineNbr> { }

        protected Int32? _INLineNbr;
        [PXDBInt(BqlField = typeof(AMMTran.iNLineNbr))]
        [PXUIField(DisplayName = "IN Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? INLineNbr
        {
            get { return this._INLineNbr; }
            set { this._INLineNbr = value; }
        }
        #endregion
        #region TranOverride
        public abstract class tranOverride : PX.Data.BQL.BqlBool.Field<tranOverride> { }

        protected Boolean? _TranOverride;
        [PXDBBool(BqlField = typeof(AMMTran.tranOverride))]
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
        #region TranOrigDocType
        public abstract class tranOrigDocType : PX.Data.BQL.BqlString.Field<tranOrigDocType> { }

        protected String _TranOrigDocType;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMMTran.origDocType))]
        [AMDocType.List]
        [PXUIField(DisplayName = "Orig Doc Type", Visible = true, Enabled = false)]
        public virtual String TranOrigDocType
        {
            get { return this._TranOrigDocType; }
            set { this._TranOrigDocType = value; }
        }
        #endregion
        #region TranOrigBatNbr
        public abstract class tranOrigBatNbr : PX.Data.BQL.BqlString.Field<tranOrigBatNbr> { }

        protected String _TranOrigBatNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(AMMTran.origBatNbr))]
        [PXUIField(DisplayName = "Orig Batch Nbr", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual String TranOrigBatNbr
        {
            get { return this._TranOrigBatNbr; }
            set { this._TranOrigBatNbr = value; }
        }
        #endregion
        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }

        protected Int32? _OrigLineNbr;
        [PXDBInt(BqlField = typeof(AMMTran.origLineNbr))]
        [PXUIField(DisplayName = "Orig Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false,
            Enabled = false)]
        public virtual Int32? OrigLineNbr
        {
            get { return this._OrigLineNbr; }
            set { this._OrigLineNbr = value; }
        }
        #endregion
        #region TranCreatedByID
        public abstract class tranCreatedByID : PX.Data.BQL.BqlGuid.Field<tranCreatedByID> { }

        protected Guid? _TranCreatedByID;
        [PXDBCreatedByID(BqlField = typeof(AMMTran.createdByID))]
        public virtual Guid? TranCreatedByID
        {
            get { return this._TranCreatedByID; }
            set { this._TranCreatedByID = value; }
        }
        #endregion
        #region TranCreatedByScreenID
        public abstract class tranCreatedByScreenID : PX.Data.BQL.BqlString.Field<tranCreatedByScreenID> { }

        protected string _TranCreatedByScreenID;
        [PXDBCreatedByScreenID(BqlField = typeof(AMMTran.createdByScreenID))]
        public virtual string TranCreatedByScreenID
        {
            get { return this._TranCreatedByScreenID; }
            set { this._TranCreatedByScreenID = value; }
        }
        #endregion
        #region TranCreatedDateTime
        public abstract class tranCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<tranCreatedDateTime> { }

        protected DateTime? _TranCreatedDateTime;
        [PXDBCreatedDateTime(BqlField = typeof(AMMTran.createdDateTime))]
        public virtual DateTime? TranCreatedDateTime
        {
            get { return this._TranCreatedDateTime; }
            set { this._TranCreatedDateTime = value; }
        }
        #endregion
        #region TranLastModifiedByID
        public abstract class tranLastModifiedByID : PX.Data.BQL.BqlGuid.Field<tranLastModifiedByID> { }

        protected Guid? _TranLastModifiedByID;
        [PXDBLastModifiedByID(BqlField = typeof(AMMTran.lastModifiedByID))]
        public virtual Guid? TranLastModifiedByID
        {
            get { return this._TranLastModifiedByID; }
            set { this._TranLastModifiedByID = value; }
        }
        #endregion
        #region TranLastModifiedByScreenID
        public abstract class tranLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<tranLastModifiedByScreenID> { }

        protected string _TranLastModifiedByScreenID;
        [PXDBLastModifiedByScreenID(BqlField = typeof(AMMTran.lastModifiedByScreenID))]
        public virtual string TranLastModifiedByScreenID
        {
            get { return this._TranLastModifiedByScreenID; }
            set { this._TranLastModifiedByScreenID = value; }
        }
        #endregion
        #region TranLastModifiedDateTime
        public abstract class tranLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<tranLastModifiedDateTime> { }

        protected DateTime? _TranLastModifiedDateTime;
        [PXDBLastModifiedDateTime(BqlField = typeof(AMMTran.lastModifiedDateTime))]
        public virtual DateTime? TranLastModifiedDateTime
        {
            get { return this._TranLastModifiedDateTime; }
            set { this._TranLastModifiedDateTime = value; }
        }
        #endregion
        #region TranTstamp
        public abstract class tranTstamp : PX.Data.BQL.BqlByte.Field<tranTstamp> { }

        protected byte[] _TranTstamp;
        [PXDBTimestamp(BqlField = typeof(AMMTran.Tstamp))]
        public virtual byte[] TranTstamp
        {
            get { return this._TranTstamp; }
            set { this._TranTstamp = value; }
        }
        #endregion
        #region LotSerCntr
        public abstract class lotSerCntr : PX.Data.BQL.BqlInt.Field<lotSerCntr> { }

        protected Int32? _LotSerCntr;
        [PXDBInt(BqlField = typeof(AMMTran.lotSerCntr))]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Lot Serial Cntr")]
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
        [AMLotSerialNbr(typeof(AMDisassembleBatch.inventoryID), typeof(AMDisassembleBatch.subItemID), 
            typeof(AMDisassembleBatch.locationID), PersistingCheck = PXPersistingCheck.Nothing, 
            BqlField = typeof(AMMTran.lotSerialNbr))]
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
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMMTran.inventoryID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial", BqlField = typeof(AMMTran.expireDate))]
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
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        [ProjectBase(IsDBField = true, BqlField = typeof(AMMTran.projectID))]
        [ProjectDefault(BatchModule.IN, typeof(Search<AMProdItem.projectID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.visibleInIN, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
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
        [PXDefault(typeof(Search<AMProdItem.taskID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMDisassembleBatch.projectID>))]
        [BaseProjectTask(typeof(AMDisassembleBatch.projectID), BqlField = typeof(AMMTran.taskID))]
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
        [PXDBInt(BqlField = typeof(AMMTran.costCodeID))]
        [PXUIField(DisplayName = "Cost Code", FieldClass = "COSTCODE")]
        [PXDefault(typeof(Search<AMProdItem.costCodeID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>, And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMDisassembleBatch.taskID>))]
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
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

        protected Int32? _LineCntrAttribute;
        [PXDBInt(BqlField = typeof(AMMTran.lineCntrAttribute))]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Line Cntr Attribute")]
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
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        protected bool? _HasMixedProjectTasks;
        [PXBool(BqlTable = typeof(AMMTran.hasMixedProjectTasks))]
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
        #region QtyScrapped
        public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

        protected Decimal? _QtyScrapped;
        [PXDBQuantity(typeof(AMDisassembleBatch.uOM), typeof(AMDisassembleBatch.baseQtyScrapped), HandleEmptyKey = true, BqlField = typeof(AMMTran.qtyScrapped))]
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
        #region BaseQtyScrapped
        public abstract class baseQtyScrapped : PX.Data.BQL.BqlDecimal.Field<baseQtyScrapped> { }

        protected Decimal? _BaseQtyScrapped;
        [PXDBQuantity(BqlField = typeof(AMMTran.baseQtyScrapped))]
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
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(BqlField = typeof(AMMTran.operationID), Visible = false, Enabled = false)]
        [PXSelector(typeof(Search<
            AMProdOper.operationID, 
            Where<AMProdOper.orderType, Equal<Current<AMDisassembleBatch.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXDefault(typeof(Search<
            AMProdItem.lastOperationID, 
            Where<AMProdItem.orderType, Equal<Current<AMDisassembleBatch.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleBatch.prodOrdID>>>>>))]
        [PXFormula(typeof(Default<AMDisassembleBatch.prodOrdID>))]
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
        #region LastOper
        public abstract class lastOper : PX.Data.BQL.BqlBool.Field<lastOper> { }

        protected Boolean? _LastOper;
        [PXDBBool(BqlField = typeof(AMMTran.lastOper))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Last Oper")]
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
        #region IsByproduct
        public abstract class isByproduct : PX.Data.BQL.BqlBool.Field<isByproduct> { }

        protected Boolean? _IsByproduct;
        [PXDBBool(BqlField = typeof(AMMTran.isByproduct))]
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
        #region IsScrap
        public abstract class isScrap : PX.Data.BQL.BqlBool.Field<isScrap> { }

        protected Boolean? _IsScrap;
        [PXDBBool(BqlField = typeof(AMMTran.isScrap))]
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
        #endregion

        #region EditableBatch
        public abstract class editableBatch : PX.Data.BQL.BqlBool.Field<editableBatch> { }
        [PXBool]
        [PXUIField(DisplayName = "Editable Batch")]
        [PXDependsOnFields(typeof(AMBatch.released), typeof(AMBatch.origBatNbr))]
        public virtual Boolean? EditableBatch => _Released != true && string.IsNullOrWhiteSpace(_OrigBatNbr);
        #endregion

#pragma warning disable PX1031 // DACs cannot contain instance methods
        protected virtual void SetStatus()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            if (this._Hold != null && (bool)this._Hold)
            {
                this._Status = INDocStatus.Hold;
                return;
            }
            if (this._Released != null && this._Released == false)
            {
                this._Status = INDocStatus.Balanced;
                return;
            }
            this._Status = INDocStatus.Released;
        }

        public static explicit operator AMBatch(AMDisassembleBatch disassembleDoc)
        {
            return Convert(disassembleDoc);
        }

        public static AMBatch Convert(AMDisassembleBatch disassembleDoc)
        {
            return new AMBatch
            {
                BatNbr = disassembleDoc.BatchNbr,
                DocType = disassembleDoc.DocType,
                ControlAmount = disassembleDoc.ControlAmount,
                ControlCost = disassembleDoc.ControlCost,
                ControlQty = disassembleDoc.ControlQty,
                TranDate = disassembleDoc.Date,
                Hold = disassembleDoc.Hold,
                LineCntr = disassembleDoc.LineCntr,
                TranDesc = disassembleDoc.TranDesc,
                TranPeriodID = disassembleDoc.TranPeriodID,
                FinPeriodID = disassembleDoc.FinPeriodID,
                OrigBatNbr = disassembleDoc.OrigBatNbr,
                Released = disassembleDoc.Released,
                Status = disassembleDoc.Status,
                TotalAmount = disassembleDoc.TotalAmount,
                TotalCost = disassembleDoc.TotalCost,
                TotalQty = disassembleDoc.TotalQty,
                NoteID = disassembleDoc.NoteID,
                OrigDocType = disassembleDoc.OrigDocType,
                CreatedByID = disassembleDoc.CreatedByID,
                CreatedByScreenID = disassembleDoc.CreatedByScreenID,
                CreatedDateTime = disassembleDoc.CreatedDateTime,
                LastModifiedByID = disassembleDoc.LastModifiedByID,
                LastModifiedByScreenID = disassembleDoc.LastModifiedByScreenID,
                LastModifiedDateTime = disassembleDoc.LastModifiedDateTime,
                RefLineNbr = disassembleDoc.RefLineNbr
            };
        }

        public static explicit operator AMMTran(AMDisassembleBatch item)
        {
            return new AMMTran
            {
                DocType = item.DocType,
                TranType = item.TranType,
                BatNbr = item.BatchNbr,
                LineNbr = item.LineNbr,
                InventoryID = item.InventoryID,
                SubItemID = item.SubItemID,
                SiteID = item.SiteID,
                LocationID = item.LocationID,
                OrderType = item.OrderType,
                ProdOrdID = item.ProdOrdID,
                OperationID = item.OperationID,
                LotSerialNbr = item.LotSerialNbr,
                ExpireDate = item.ExpireDate,
                Qty = item.Qty,
                BaseQty = item.BaseQty,
                UOM = item.UOM,
                UnitCost = item.UnitCost,
                TranDate = item.TranDate,
                InvtMult = item.InvtMult,
                MultDiv = item.MultDiv,
                Released = item.Released,
                Closeflg = item.Closeflg,
                IsScrap = item.IsScrap,
                TranDesc = item.TranDesc,
                TranPeriodID = item.TranPeriodID,
                FinPeriodID = item.FinPeriodID,
                MatlLineId = item.MatlLineId,
                OrigDocType = item.OrigDocType,
                OrigBatNbr = item.OrigBatNbr,
                OrigLineNbr = item.OrigLineNbr,
                GLBatNbr = item.GLBatNbr,
                GLLineNbr = item.GLLineNbr,
                INDocType = item.INDocType,
                INBatNbr = item.INBatNbr,
                INLineNbr = item.INLineNbr,
                NoteID = item.NoteID,
                LineCntrAttribute = item.LineCntrAttribute,
                TranOverride = item.TranOverride,
                AcctID = item.AcctID,
                SubID = item.SubID,
                WIPAcctID = item.WIPAcctID,
                WIPSubID = item.WIPSubID,
                UnassignedQty = item.UnassignedQty,
                CreatedByID = item.CreatedByID,
                CreatedByScreenID = item.CreatedByScreenID,
                CreatedDateTime = item.CreatedDateTime,
                LastModifiedByID = item.LastModifiedByID,
                LastModifiedByScreenID = item.LastModifiedByScreenID,
                LastModifiedDateTime = item.LastModifiedDateTime,
                QtyScrapped = 0m,
                BaseQtyScrapped = 0m
            };
        }

        public static implicit operator AMDisassembleBatchSplit(AMDisassembleBatch item)
        {
            AMDisassembleBatchSplit ret = new AMDisassembleBatchSplit();
            ret.DocType = item.DocType;
            ret.TranType = item.TranType;
            ret.BatNbr = item.BatchNbr;
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
            //ret.IsScrap = item.IsScrap;

            return ret;
        }
        public static implicit operator AMDisassembleBatch(AMDisassembleBatchSplit item)
        {
            AMDisassembleBatch ret = new AMDisassembleBatch();
            ret.DocType = item.DocType;
            ret.TranType = item.TranType;
            ret.BatchNbr = item.BatNbr;
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
            //ret.IsScrap = item.IsScrap;

            return ret;
        }

        /// <summary>
        /// Is the given standard manufacturing batch and transaction records represented as a AMDisassembleBatch row?
        /// </summary>
        public static bool IsDisassembleBatchTran(AMBatch batch, AMMTran tran)
        {
            return batch?.RefLineNbr != null && batch.DocType == AMDocType.Disassembly && tran?.LineNbr != null &&
                   batch.RefLineNbr == tran.LineNbr;
        }
    }
}
