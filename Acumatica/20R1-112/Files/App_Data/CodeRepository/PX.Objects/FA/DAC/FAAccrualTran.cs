using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.GL.Attributes;

namespace PX.Objects.FA
{
	[Serializable]
    [PXProjection(typeof(Select2<GLTran,
                LeftJoin<FAAccrualTran, On<GLTran.tranID, Equal<FAAccrualTran.tranID>>>,
                Where<GLTran.module, NotEqual<BatchModule.moduleFA>, And<GLTran.released, Equal<True>>>>), new Type[]{typeof(FAAccrualTran)})]
	[PXCacheName(Messages.FAAccrualTran)]
	public partial class FAAccrualTran : IBqlTable, IFALocation
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
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
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlInt.Field<tranID> { }
		protected int? _TranID;
		[PXDBInt]
		[PXDefault]
		[PXExtraKey]
		public virtual int? TranID
		{
			get
			{
                return _TranID;
			}
			set
			{
    		    _TranID = value;
			}
		}
		#endregion
        #region GLTranID
        public abstract class gLTranID : PX.Data.BQL.BqlInt.Field<gLTranID> { }
		[PXDBInt(BqlField = typeof(GLTran.tranID), IsKey = true)]
		[PXDependsOnFields(typeof(FAAccrualTran.tranID))]
        public virtual int? GLTranID
        {
            get
            {
                return _TranID;
            }
            set
            {
                _TranID = value;
            }
        }
        #endregion
        #region GLTranAccountID
        public abstract class gLTranAccountID : PX.Data.BQL.BqlInt.Field<gLTranAccountID> { }
        protected int? _GLTranAccountID;
        [PXDBInt(BqlField = typeof(GLTran.accountID))]
        public virtual int? GLTranAccountID
        {
            get
            {
                return _GLTranAccountID;
            }
            set
            {
                _GLTranAccountID = value;
            }
        }
        #endregion
        #region GLTranSubID
        public abstract class gLTranSubID : PX.Data.BQL.BqlInt.Field<gLTranSubID> { }
        protected int? _GLTranSubID;
        [PXDBInt(BqlField = typeof(GLTran.subID))]
        public virtual int? GLTranSubID
        {
            get
            {
                return _GLTranSubID;
            }
            set
            {
                _GLTranSubID = value;
            }
        }
        #endregion
		#region GLTranQty
		public abstract class gLTranQty : PX.Data.BQL.BqlDecimal.Field<gLTranQty> { }
		protected Decimal? _GLTranQty;
		[PXDBQuantity]
		[PXDefault(typeof(FAAccrualTran.gLTranOrigQty))]
		[PXUIField(DisplayName = "Orig. Quantity", Enabled = false)]
		public virtual Decimal? GLTranQty
		{
			get
			{
				return _GLTranQty;
			}
			set
			{
				_GLTranQty = value;
			}
		}
		#endregion
		#region GLTranAmt
		public abstract class gLTranAmt : PX.Data.BQL.BqlDecimal.Field<gLTranAmt> { }
		protected Decimal? _GLTranAmt;
		[PXDBBaseCury]
		[PXDefault(typeof(FAAccrualTran.gLTranDebitAmt))]
		[PXUIField(DisplayName = "Orig. Amount", Enabled = false)]
		public virtual Decimal? GLTranAmt
		{
			get
			{
				return _GLTranAmt;
			}
			set
			{
				_GLTranAmt = value;
			}
		}
		#endregion
		#region SelectedQty
		public abstract class selectedQty : PX.Data.BQL.BqlDecimal.Field<selectedQty> { }
		protected Decimal? _SelectedQty;
		[PXQuantity]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selected Quantity")]
		public virtual Decimal? SelectedQty
		{
			get
			{
				return _SelectedQty;
			}
			set
			{
				_SelectedQty = value;
			}
		}
		#endregion
		#region SelectedAmt
		public abstract class selectedAmt : PX.Data.BQL.BqlDecimal.Field<selectedAmt> { }
		protected Decimal? _SelectedAmt;
		[PXBaseCury]
		[PXUnboundDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Selected Amount")]
		public virtual Decimal? SelectedAmt
		{
			get
			{
				return _SelectedAmt;
			}
			set
			{
				_SelectedAmt = value;
			}
		}
		#endregion
		#region OpenQty
		public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
		protected Decimal? _OpenQty;
		[PXDBQuantity]
		[PXFormula(typeof(Sub<gLTranQty, Add<selectedQty, closedQty>>))]
		[PXUIField(DisplayName = "Open Quantity", Enabled = false)]
		public virtual Decimal? OpenQty
		{
			get
			{
				return _OpenQty;
			}
			set
			{
				_OpenQty = value;
			}
		}
		#endregion
		#region OpenAmt
		public abstract class openAmt : PX.Data.BQL.BqlDecimal.Field<openAmt> { }
		protected Decimal? _OpenAmt;
		[PXDBBaseCury]
		[PXFormula(typeof(Sub<gLTranAmt, Add<selectedAmt, closedAmt>>))]
		[PXUIField(DisplayName = "Open Amount", Enabled = false)]
		public virtual Decimal? OpenAmt
		{
			get
			{
				return _OpenAmt;
			}
			set
			{
				_OpenAmt = value;
			}
		}
		#endregion
		#region ClosedAmt
		public abstract class closedAmt : PX.Data.BQL.BqlDecimal.Field<closedAmt> { }
		protected Decimal? _ClosedAmt;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ClosedAmt
		{
			get
			{
				return _ClosedAmt;
			}
			set
			{
				_ClosedAmt = value;
			}
		}
		#endregion
		#region ClosedQty
		public abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }
		protected Decimal? _ClosedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? ClosedQty
		{
			get
			{
				return _ClosedQty;
			}
			set
			{
				_ClosedQty = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBBaseCury]
		[PXFormula(typeof(Switch<Case<Where<gLTranQty, LessEqual<decimal0>>, gLTranAmt>, Div<gLTranAmt, gLTranQty>>))]
		[PXUIField(DisplayName = "Unit Cost", Enabled = false)]
		public virtual Decimal? UnitCost
		{
			get
			{
				return _UnitCost;
			}
			set
			{
				_UnitCost = value;
			}
		}
		#endregion
        #region Reconciled
        public abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Reconciled")]
        [PXDefault(false)]
        public bool? Reconciled { get; set; }
        #endregion



        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        
        #region ClassID
		public abstract class classID : PX.Data.BQL.BqlInt.Field<classID> { }
		protected Int32? _ClassID;
		[PXInt]
		[PXSelector(typeof(Search2<FAClass.assetID,
			LeftJoin<FABookSettings, On<FAClass.assetID, Equal<FABookSettings.assetID>>,
			LeftJoin<FABook, On<FABookSettings.bookID, Equal<FABook.bookID>>>>,
			Where<FAClass.recordType, Equal<FARecordType.classType>,
			And<FABook.updateGL, Equal<True>>>>),
					SubstituteKey = typeof(FAClass.assetCD),
					DescriptionField = typeof(FAClass.description))]
		[PXUIField(DisplayName = "Asset Class")]
		public virtual Int32? ClassID
		{
			get
			{
				return _ClassID;
			}
			set
			{
				_ClassID = value;
			}
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        protected Int32? _BranchID;
        
		[Branch(useDefaulting: false, IsDBField = false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? BranchID
        {
            get
            {
                return _BranchID;
            }
            set
            {
                _BranchID = value;
            }
        }
        #endregion
        #region GLTranBranchID

		public abstract class gLTranBranchID : PX.Data.BQL.BqlInt.Field<gLTranBranchID> { }

        [Branch(DisplayName = "Transaction Branch", BqlField = typeof(GLTran.branchID), Enabled = false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? GLTranBranchID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected int? _EmployeeID;
		[PXInt]
		[PXSelector(typeof(EPEmployee.bAccountID), SubstituteKey = typeof(EPEmployee.acctCD), DescriptionField = typeof(EPEmployee.acctName))]
		[PXUIField(DisplayName = "Custodian")]
		public virtual int? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
		#endregion
		#region Department
		public abstract class department : PX.Data.BQL.BqlString.Field<department> { }
		protected String _Department;
		[PXString(10, IsUnicode = true)]
		[PXSelector(typeof(EPDepartment.departmentID), DescriptionField = typeof(EPDepartment.description))]
		[PXUIField(DisplayName = "Department")]
		public virtual String Department
		{
			get
			{
				return _Department;
			}
			set
			{
				_Department = value;
			}
		}
		#endregion
		#region Component
		public abstract class component : PX.Data.BQL.BqlBool.Field<component> { }
		protected Boolean? _Component = false;
		[PXBool]
		[PXUIField(DisplayName = "Component")]
		public virtual Boolean? Component
		{
			get
			{
				return _Component;
			}
			set
			{
				_Component = value;
			}
		}
		#endregion
        #region GLTranDebitAmt
        public abstract class gLTranDebitAmt : PX.Data.BQL.BqlDecimal.Field<gLTranDebitAmt> { }
        protected Decimal? _GLTranDebitAmt;
        [PXDBBaseCury(BqlField = typeof(GLTran.debitAmt))]
        public virtual Decimal? GLTranDebitAmt
        {
            get
            {
                return this._GLTranDebitAmt;
            }
            set
            {
                this._GLTranDebitAmt = value;
            }
        }
        #endregion
        #region GLTranCreditAmt
        public abstract class gLTranCreditAmt : PX.Data.BQL.BqlDecimal.Field<gLTranCreditAmt> { }
        protected Decimal? _GLTranCreditAmt;
        [PXDBBaseCury(BqlField = typeof(GLTran.creditAmt))]
        public virtual Decimal? GLTranCreditAmt
        {
            get
            {
                return this._GLTranCreditAmt;
            }
            set
            {
                this._GLTranCreditAmt = value;
            }
        }
        #endregion
        #region GLTranOrigQty
        public abstract class gLTranOrigQty : PX.Data.BQL.BqlDecimal.Field<gLTranOrigQty> { }
        protected Decimal? _GLTranOrigQty;
        [IN.PXDBQuantity(BqlField = typeof(GLTran.qty))]
        public virtual Decimal? GLTranOrigQty
        {
            get
            {
                return this._GLTranOrigQty;
            }
            set
            {
                this._GLTranOrigQty = value;
            }
        }
        #endregion
        #region GLTranInventoryID
        public abstract class gLTranInventoryID : PX.Data.BQL.BqlInt.Field<gLTranInventoryID> { }
        protected Int32? _GLTranInventoryID;
        [IN.Inventory(Enabled = false, BqlField = typeof(GLTran.inventoryID))]
        public virtual Int32? GLTranInventoryID
        {
            get
            {
                return this._GLTranInventoryID;
            }
            set
            {
                this._GLTranInventoryID = value;
            }
        }
        #endregion
        #region GLTranModule
        public abstract class gLTranModule : PX.Data.BQL.BqlString.Field<gLTranModule> { }
        protected String _GLTranModule;
        [PXDBString(2, IsFixed = true, BqlField = typeof(GLTran.module))]
        [PXUIField(DisplayName = "Module", Enabled = false)]
        public virtual String GLTranModule
        {
            get
            {
                return this._GLTranModule;
            }
            set
            {
                this._GLTranModule = value;
            }
        }
        #endregion
        #region GLTranBatchNbr
        public abstract class gLTranBatchNbr : PX.Data.BQL.BqlString.Field<gLTranBatchNbr> { }
        protected String _GLTranBatchNbr;
        //[PXDBCalced(typeof(Switch<Case<Where<True, Equal<True>>, GLTran.batchNbr>>), typeof(string))]
        [PXDBString(15, IsUnicode = true, BqlField = typeof(GLTran.batchNbr))]
        [PXUIField(DisplayName = "Batch Number", Enabled = false)]
        public virtual String GLTranBatchNbr
        {
            get
            {
                return this._GLTranBatchNbr;
            }
            set
            {
                this._GLTranBatchNbr = value;
            }
        }
        #endregion
        #region GLTranUOM
        public abstract class gLTranUOM : PX.Data.BQL.BqlString.Field<gLTranUOM> { }
        protected String _GLTranUOM;
        [PXDBString(6, IsUnicode = true, InputMask = ">aaaaaa", BqlField = typeof(GLTran.uOM))]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
        public virtual String GLTranUOM
        {
            get
            {
                return this._GLTranUOM;
            }
            set
            {
                this._GLTranUOM = value;
            }
        }
        #endregion
        #region GLTranDate
        public abstract class gLTranDate : PX.Data.BQL.BqlDateTime.Field<gLTranDate> { }
        protected DateTime? _GLTranDate;
        [PXDBDate(BqlField = typeof(GLTran.tranDate))]
        [PXUIField(DisplayName = "Transaction Date", Enabled = false)]
        public virtual DateTime? GLTranDate
        {
            get
            {
                return this._GLTranDate;
            }
            set
            {
                this._GLTranDate = value;
            }
        }
        #endregion
        #region GLTranRefNbr
        public abstract class gLTranRefNbr : PX.Data.BQL.BqlString.Field<gLTranRefNbr> { }
        protected String _GLTranRefNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(GLTran.refNbr))]
        [PXUIField(DisplayName = "Ref. Number", Enabled = false)]
        public virtual String GLTranRefNbr
        {
            get
            {
                return this._GLTranRefNbr;
            }
            set
            {
                this._GLTranRefNbr = value;
            }
        }
        #endregion
        #region GLTranReferenceID
        public abstract class gLTranReferenceID : PX.Data.BQL.BqlInt.Field<gLTranReferenceID> { }
        protected Int32? _GLTranReferenceID;
        [PXDBInt(BqlField = typeof(GLTran.referenceID))]
        [PXSelector(typeof(Search<BAccountR.bAccountID>), SubstituteKey = typeof(BAccountR.acctCD), CacheGlobal = true)]
		[CustomerVendorRestrictor]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = false)]
        public virtual Int32? GLTranReferenceID
        {
            get
            {
                return this._GLTranReferenceID;
            }
            set
            {
                this._GLTranReferenceID = value;
            }
        }
        #endregion
        #region GLTranDesc
        public abstract class gLTranDesc : PX.Data.BQL.BqlString.Field<gLTranDesc> { }
        protected String _GLTranDesc;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(GLTran.tranDesc))]
        [PXUIField(DisplayName = "Transaction Description", Enabled = false)]
        public virtual String GLTranDesc
        {
            get
            {
                return this._GLTranDesc;
            }
            set
            {
                this._GLTranDesc = value;
            }
        }
        #endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[Inventory(Enabled = false, BqlField = typeof(GLTran.inventoryID))]
		public virtual int? InventoryID { get; set; }
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[INUnit(typeof(GLTran.inventoryID), Enabled = false, BqlField = typeof(GLTran.uOM))]
		public virtual string UOM { get; set; }
		#endregion

    }
}
