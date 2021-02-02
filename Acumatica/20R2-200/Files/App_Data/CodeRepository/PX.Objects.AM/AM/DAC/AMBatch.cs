using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.IN;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [PXPrimaryGraph(
        new[] {
	        typeof (MoveEntry), 
	        typeof (LaborEntry), 
	        typeof (MaterialEntry), 
	        typeof (WIPAdjustmentEntry), 
	        typeof (ProductionCostEntry),
            typeof (DisassemblyEntry),
            typeof (VendorShipmentEntry)}
        ,
        new[] {
	    	typeof (Where<AMBatch.docType, Equal<AMDocType.move>>), 
	    	typeof (Where<AMBatch.docType, Equal<AMDocType.labor>>), 
	    	typeof (Where<AMBatch.docType, Equal<AMDocType.material>>), 
	    	typeof (Where<AMBatch.docType, Equal<AMDocType.wipAdjust>>), 
	    	typeof (Where<AMBatch.docType, Equal<AMDocType.prodCost>>),
            typeof (Select<AMDisassembleBatch, Where<AMDisassembleBatch.docType, Equal<AMDocType.disassembly>, And<AMDisassembleBatch.batchNbr, Equal<Current<AMBatch.batNbr>>>>>),
            typeof (Select<AMVendorShipment, Where<Current<AMBatch.origDocType>, Equal<AMDocType.vendorShipment>, And<AMVendorShipment.shipmentNbr, Equal<Current<AMBatch.origBatNbr>>>>>)
        })]
    [AMBatchCacheName("AM Batch")]
    [System.Diagnostics.DebuggerDisplay("DocType = {DocType}, BatNbr = {BatNbr}, Status = {Status}, OrigDocType = {OrigDocType}, OrigBatNbr = {OrigBatNbr}")]
    [Serializable]
	public class AMBatch : IBqlTable, IAMBatch, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMBatch>.By<docType, batNbr>
        {
            public static AMBatch Find(PXGraph graph, string docType, string refNbr) =>
                FindBy(graph, docType, refNbr);
        }
        public static class FK
        {
            public class OrigBatch : AMBatch.PK.ForeignKeyOf<AMBatch>.By<origDocType, origBatNbr> { }
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

        public abstract class docType : PX.Data.BQL.BqlString.Field<docType>  { }
        protected String _DocType;
        [PXDBString(1, IsKey = true, IsFixed = true)]
        [PXDefault]
        [AMDocType.List]
        [PXUIField(DisplayName = "Document Type", Enabled = false)]
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
		public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr>  { }
		protected String _BatNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask=">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Batch Nbr", Visibility=PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Optional<AMBatch.docType>>>, OrderBy<Desc<AMBatch.batNbr>>>), Filterable = true)]
        [AMDocType.Numbering]
        [PXFieldDescription]
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
        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        protected String _FinPeriodID;

        [INOpenPeriod(
            sourceType: typeof(AMBatch.tranDate),
            branchSourceType: typeof(AMBatch.branchID),
            masterFinPeriodIDType: typeof(AMBatch.tranPeriodID),
            IsHeader = true)]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Post Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        protected String _Status;
        [PXDBString(1, IsFixed = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
        [DocStatus.List] 
        public virtual String Status
        {
            [PXDependsOnFields(typeof(released), typeof(hold))]
            get
            {
                return this._Status;
            }
            set
            {
                //this._Status = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool]
        [PXDefault(typeof(AMPSetup.holdEntry))]
        [PXUIField(DisplayName = "Hold")]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
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
        #region ControlAmount

        public abstract class controlAmount : PX.Data.BQL.BqlDecimal.Field<controlAmount> { }
        protected Decimal? _ControlAmount;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Amount")]
        public virtual Decimal? ControlAmount
        {
            get
            {
                return this._ControlAmount;
            }
            set
            {
                this._ControlAmount = value;
            }
        }
        #endregion
        #region ControlQty
        public abstract class controlQty : PX.Data.BQL.BqlDecimal.Field<controlQty> { }
        protected Decimal? _ControlQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Qty.")]
        public virtual Decimal? ControlQty
        {
            get
            {
                return this._ControlQty;
            }
            set
            {
                this._ControlQty = value;
            }
        }
        #endregion
        #region ControlCost
        public abstract class controlCost : PX.Data.BQL.BqlDecimal.Field<controlCost> { }
        protected Decimal? _ControlCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Cost")]
        public virtual Decimal? ControlCost
        {
            get
            {
                return this._ControlCost;
            }
            set
            {
                this._ControlCost = value;
            }
        }
        #endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote(DescriptionField = typeof(AMBatch.batNbr))]
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
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Orig Batch Nbr", Visibility = PXUIVisibility.Visible, Enabled = false)]
        [PXSelector(typeof(Search<AMBatch.batNbr, Where<AMBatch.docType, Equal<Current<AMBatch.origDocType>>>>), ValidateValue = false)]
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
        #region TranPeriodID
        public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
		protected String _TranPeriodID;

        [PeriodID]
        [PXDefault(MapErrorTo = typeof(AMBatch.tranDate))]
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
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
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
        #region TotalAmount

        public abstract class totalAmount : PX.Data.BQL.BqlDecimal.Field<totalAmount> { }
        protected Decimal? _TotalAmount;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Amount", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? TotalAmount
        {
            get
            {
                return this._TotalAmount;
            }
            set
            {
                this._TotalAmount = value;
            }
        }
        #endregion
        #region TotalQty
        public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }
        protected Decimal? _TotalQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Qty.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual Decimal? TotalQty
        {
            get
            {
                return this._TotalQty;
            }
            set
            {
                this._TotalQty = value;
            }
        }
                #endregion
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        protected Decimal? _TotalCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Cost", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        protected int? _LineCntr;
	    [PXDBInt]
        [PXDefault(0)]
        public virtual int? LineCntr
        {
            get
            {
                return this._LineCntr;
            }
            set
            {
                this._LineCntr = value;
            }
        }
        #endregion
	    #region RefLineNbr
	    public abstract class refLineNbr : PX.Data.BQL.BqlInt.Field<refLineNbr> { }
	    protected Int32? _RefLineNbr;
	    [PXDBInt]
	    [PXUIField(DisplayName = "Ref Line Nbr.", Visible = false, Enabled = false)]
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
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }
        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true)]
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
        #region EditableBatch
        public abstract class editableBatch : PX.Data.BQL.BqlBool.Field<editableBatch> { }
        [PXBool]
        [PXUIField(DisplayName = "Editable Batch")]
        [PXDependsOnFields(typeof(AMBatch.released), typeof(AMBatch.origBatNbr))]
        public virtual Boolean? EditableBatch => _Released != true && string.IsNullOrWhiteSpace(_OrigBatNbr);

        #endregion

        #region DAC Methods
#pragma warning disable PX1031 // DACs cannot contain instance methods
        protected virtual void SetStatus()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            if (this._Released != null && this._Released == true)
            {
                this._Status = DocStatus.Released;
                return;
            }

            if (this._Hold != null && this._Hold == true)
            {
                this._Status = DocStatus.Hold;
                return;
            }

            this._Status = DocStatus.Balanced;
        }
        #endregion
    }
}
