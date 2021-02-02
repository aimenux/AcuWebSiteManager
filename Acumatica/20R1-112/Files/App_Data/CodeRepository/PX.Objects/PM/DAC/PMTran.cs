using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.IN;
using System;

namespace PX.Objects.PM
{
	/// <exclude/>
	[PXPrimaryGraph(
		new Type[] { typeof(RegisterEntry) },
		new Type[] { typeof(Select<PMRegister, Where<PMRegister.refNbr, Equal<Current<PMTran.refNbr>>>>)
		})]
	[SerializableAttribute()]
	[PXCacheName(Messages.PMTran)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMTran : IBqlTable, IProjectFilter, IQuantify
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected>
		{
		}
		protected bool? _Selected = false;
		[PXBool]
		[PXUnboundDefault(false)]
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
		[Branch()]
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
		#region TranID
		public abstract class tranID : PX.Data.BQL.BqlLong.Field<tranID>
		{
		}
		protected Int64? _TranID;
		[PXUIField(DisplayName = "Tran. ID", Visible = false, Enabled = false)]
		[PXDBLongIdentity(IsKey = true)]
		public virtual Int64? TranID
		{
			get
			{
				return this._TranID;
			}
			set
			{
				this._TranID = value;
			}
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType>
		{
		}
		protected String _TranType;
		[PXDefault(typeof(PMRegister.module))]
		[PXDBString(2, IsFixed = true)]
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
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr>
		{
		}
		protected String _RefNbr;
		[PXUIField(DisplayName = "Ref. Number")]
		[PXDBDefault(typeof(PMRegister.refNbr))]
		[PXDBString(PMRegister.refNbr.Length, IsUnicode = true)]
		[PXParent(typeof(Select<PMRegister, Where<PMRegister.module, Equal<Current<PMTran.tranType>>, And<PMRegister.refNbr, Equal<Current<PMTran.refNbr>>>>>))]
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
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date>
		{
		}
		protected DateTime? _Date;
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID>
		{
		}
		protected String _FinPeriodID;
		[OpenPeriod(
			null,
			typeof(PMTran.date),
			branchSourceType: typeof(PMTran.branchID),
			masterFinPeriodIDType: typeof(PMTran.tranPeriodID),
			redefaultOrRevalidateOnOrganizationSourceUpdated: false
			)]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate>
		{
		}
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
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
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID>
		{
		}
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
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID>
		{
		}
		protected Int32? _ProjectID;
		[PXDefault]
		[PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
		[ActiveProjectOrContractBase]
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
		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<projectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[BaseProjectTaskAttribute(typeof(PMTran.projectID), AllowInactive = false)]
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID>
		{
		}
		protected Int32? _AccountGroupID;
		[PXForeignReference(typeof(Field<accountGroupID>.IsRelatedTo<PMAccountGroup.groupID>))]
		[AccountGroup]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion

		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID>
		{
		}
		protected Int32? _CostCodeID;
		[CostCode(null, typeof(taskID), GL.AccountType.Expense, typeof(accountGroupID), ReleasedField = typeof(released))]
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

		#region OffsetAccountGroupID
		public abstract class offsetAccountGroupID : PX.Data.BQL.BqlInt.Field<offsetAccountGroupID>
		{
		}
		protected Int32? _OffsetAccountGroupID;
		[PXDBInt]
		public virtual Int32? OffsetAccountGroupID
		{
			get
			{
				return this._OffsetAccountGroupID;
			}
			set
			{
				this._OffsetAccountGroupID = value;
			}
		}
		#endregion
		#region ResourceID
		public abstract class resourceID : PX.Data.BQL.BqlInt.Field<resourceID>
		{
		}
		protected Int32? _ResourceID;
		[PXEPEmployeeSelector]
		[PXDBInt()]
		[PXUIField(DisplayName = "Employee")]
		[PXForeignReference(typeof(Field<resourceID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual Int32? ResourceID
		{
			get
			{
				return this._ResourceID;
			}
			set
			{
				this._ResourceID = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID>
		{
		}
		protected Int32? _BAccountID;
		[PXDBInt()]
		[PXUIField(DisplayName = "Customer/Vendor")]
		[PXSelector(typeof(Search<BAccountR.bAccountID>),
			typeof(BAccountR.acctCD),
			typeof(BAccountR.acctName),
			typeof(BAccountR.type),
			typeof(BAccountR.parentBAccountID),
			typeof(BAccount.ownerID),
			typeof(BAccount.acctReferenceNbr), SubstituteKey = typeof(BAccountR.acctCD), DescriptionField = typeof(BAccountR.acctName))]
		[CustomerVendorRestrictor]
		[PXForeignReference(typeof(Field<bAccountID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID>
		{
		}
		protected Int32? _LocationID;
		[PXDefault(typeof(Search<BAccount.defLocationID, Where<BAccount.bAccountID, Equal<Current<PMTran.bAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<PMTran.bAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID>
		{
		}
		protected Int32? _InventoryID;
		[PXUIField(DisplayName = "Inventory ID")]
		[PXDBInt()]
		[PMInventorySelector]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description>
		{
		}
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		[PXFieldDescription]
		public virtual String Description
		{ get; set; }
		#endregion
		#region InvoicedDescription
		public abstract class invoicedDescription : PX.Data.BQL.BqlString.Field<invoicedDescription>
		{
		}
		[PXString(255, IsUnicode = true)]
		public virtual String InvoicedDescription
		{ get; set; }
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM>
		{
		}
		protected String _UOM;
		[PMUnit(typeof(PMTran.inventoryID))]
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
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty>
		{
		}
		protected Decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
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
		#region Billable
		public abstract class billable : PX.Data.BQL.BqlBool.Field<billable>
		{
		}
		protected Boolean? _Billable;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Billable")]
		public virtual Boolean? Billable
		{
			get
			{
				return this._Billable;
			}
			set
			{
				this._Billable = value;
			}
		}
		#endregion
		#region UseBillableQty
		public abstract class useBillableQty : PX.Data.BQL.BqlBool.Field<useBillableQty>
		{
		}
		protected Boolean? _UseBillableQty;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Billable Quantity in Amount Formula")]
		public virtual Boolean? UseBillableQty
		{
			get
			{
				return this._UseBillableQty;
			}
			set
			{
				this._UseBillableQty = value;
			}
		}
		#endregion
		#region BillableQty
		public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty>
		{
		}
		protected Decimal? _BillableQty;
		[PXDBQuantity]
		[PXDefault(typeof(PMTran.qty))]
		[PXUIField(DisplayName = "Billable Quantity")]
		public virtual Decimal? BillableQty
		{
			get
			{
				return this._BillableQty;
			}
			set
			{
				this._BillableQty = value;
			}
		}
		#endregion
		#region InvoicedQty
		public abstract class invoicedQty : PX.Data.BQL.BqlDecimal.Field<invoicedQty>
		{
		}
		protected Decimal? _InvoicedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Quantity", Enabled = false)]
		public virtual Decimal? InvoicedQty
		{
			get
			{
				return this._InvoicedQty;
			}
			set
			{
				this._InvoicedQty = value;
			}
		}
		#endregion
		#region TranCuryID
		public abstract class tranCuryID : PX.Data.BQL.BqlString.Field<tranCuryID> { }
		[PXDefault]
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>),
			typeof(CurrencyList.curyID),
			typeof(CurrencyList.description),
			CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency", FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual string TranCuryID
		{
			get;
			set;
		}
		#endregion
		#region ProjectCuryID
		public abstract class projectCuryID : PX.Data.BQL.BqlString.Field<projectCuryID> { }
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Project Currency", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual string ProjectCuryID
		{
			get;
			set;
		}
		#endregion
		#region BaseCuryInfoID
		public abstract class baseCuryInfoID : PX.Data.BQL.BqlLong.Field<baseCuryInfoID> { }
		[PXDBLong]
		[CurrencyInfo(CuryIDField = nameof(TranCuryID), CuryRateField = nameof(BaseCuryRate), ModuleCode = BatchModule.PM)]
		public virtual long? BaseCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region ProjectCuryInfoID
		public abstract class projectCuryInfoID : PX.Data.BQL.BqlLong.Field<projectCuryInfoID> { }
		[PXDBLong]
		[CurrencyInfo(CuryIDField = nameof(TranCuryID), ModuleCode = BatchModule.PM)]
		public virtual long? ProjectCuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region BaseCuryRate
		public abstract class baseCuryRate : PX.Data.BQL.BqlDecimal.Field<baseCuryRate>
		{
		}
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Base Currency Rate", Enabled = false, Visible = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? BaseCuryRate
		{
			get;
			set;
		}
		#endregion
		#region ProjectCuryRate
		public abstract class projectCuryRate : PX.Data.BQL.BqlDecimal.Field<projectCuryRate>
		{
		}
		[PXDecimal(8)]
		[PXUIField(DisplayName = "Project Currency Rate", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? ProjectCuryRate
		{
			get;
			set;
		}
		#endregion
		#region TranCuryUnitRate
		public abstract class tranCuryUnitRate : PX.Data.BQL.BqlDecimal.Field<tranCuryUnitRate> { }
		[PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(PMTran.baseCuryInfoID), typeof(PMTran.unitRate))]
		[PXUIField(DisplayName = "Unit Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranCuryUnitRate
		{
			get;
			set;
		}
		#endregion
		#region UnitRate
		public abstract class unitRate : PX.Data.BQL.BqlDecimal.Field<unitRate> { }
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? UnitRate
		{
			get;
			set;
		}
		#endregion
		#region TranCuryAmount
		public abstract class tranCuryAmount : PX.Data.BQL.BqlDecimal.Field<tranCuryAmount> { }
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		[PXFormula(typeof(Mult<Switch<Case<Where<PMTran.useBillableQty, Equal<True>>, PMTran.billableQty>, PMTran.qty>, PMTran.tranCuryUnitRate>))]
		[PXDBCurrency(typeof(PMTran.baseCuryInfoID), typeof(PMTran.amount))]
		public virtual decimal? TranCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? Amount
		{
			get;
			set;
		}
		#endregion
		#region TranCuryAmountCopy
		public abstract class tranCuryAmountCopy : PX.Data.BQL.BqlDecimal.Field<tranCuryAmountCopy> { }
		[PXFormula(typeof(PMTran.tranCuryAmount))]
		[PXCurrency(typeof(PMTran.projectCuryInfoID), typeof(PMTran.projectCuryAmount))]
		public virtual decimal? TranCuryAmountCopy
		{
			get;
			set;
		}
		#endregion
		#region ProjectCuryAmount
		public abstract class projectCuryAmount : PX.Data.BQL.BqlDecimal.Field<projectCuryAmount> { }
		[PXDBProjectCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Project Currency Amount", Enabled = false, FieldClass = nameof(FeaturesSet.ProjectMultiCurrency))]
		public virtual decimal? ProjectCuryAmount
		{
			get;
			set;
		}
		#endregion
		#region ProjectCuryInvoicedAmount
		public abstract class projectCuryInvoicedAmount : PX.Data.BQL.BqlDecimal.Field<projectCuryInvoicedAmount> { }
		[PXDBProjectCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Billed Amount", Enabled = false)]
		public virtual decimal? ProjectCuryInvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region InvoicedAmount
		public abstract class invoicedAmount : PX.Data.BQL.BqlDecimal.Field<invoicedAmount> { }
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? InvoicedAmount
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID>
		{
		}
		protected Int32? _AccountID;
		//[PXDefault] can be null for Off-balance account group
		[Account(null, typeof(Search2<Account.accountID,
			LeftJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Current<PMTran.accountGroupID>>>>,
			Where<PMAccountGroup.type, NotEqual<PMAccountType.offBalance>, And<Account.accountGroupID, Equal<Current<PMTran.accountGroupID>>,
			Or<PMAccountGroup.type, Equal<PMAccountType.offBalance>,
			Or<PMAccountGroup.groupID, IsNull>>>>>),
			DisplayName = "Debit Account",
			AvoidControlAccounts = true)]
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
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID>
		{
		}
		protected Int32? _SubID;
		//[PXDefault] can be null for Off-balance account group
		[SubAccount(typeof(PMTran.accountID), DisplayName = "Debit Subaccount")]
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
		#region OffsetAccountID
		public abstract class offsetAccountID : PX.Data.BQL.BqlInt.Field<offsetAccountID>
		{
		}
		protected Int32? _OffsetAccountID;
		//[PXDefault]
		[Account(DisplayName = "Credit Account", AvoidControlAccounts = true)]
		public virtual Int32? OffsetAccountID
		{
			get
			{
				return this._OffsetAccountID;
			}
			set
			{
				this._OffsetAccountID = value;
			}
		}
		#endregion
		#region OffsetSubID
		public abstract class offsetSubID : PX.Data.BQL.BqlInt.Field<offsetSubID>
		{
		}
		protected Int32? _OffsetSubID;
		//[PXDefault]
		[SubAccount(typeof(PMTran.offsetAccountID), DisplayName = "Credit Subaccount")]
		public virtual Int32? OffsetSubID
		{
			get
			{
				return this._OffsetSubID;
			}
			set
			{
				this._OffsetSubID = value;
			}
		}
		#endregion
		#region Allocated
		public abstract class allocated : PX.Data.BQL.BqlBool.Field<allocated>
		{
		}
		protected Boolean? _Allocated;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allocated", Enabled = false)]
		public virtual Boolean? Allocated
		{
			get
			{
				return this._Allocated;
			}
			set
			{
				this._Allocated = value;
			}
		}
		#endregion
		#region ExcludedFromAllocation
		public abstract class excludedFromAllocation : PX.Data.BQL.BqlBool.Field<excludedFromAllocation> { }
		/// <summary>
		/// Excluded from Allocation
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Excluded From Allocation", Visible = false, Enabled = false)]
		public virtual bool? ExcludedFromAllocation
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released>
		{
		}
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
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
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr>
		{
		}
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "GL Batch Nbr.", Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<Current<PMTran.tranType>>>>))]
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
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule>
		{
		}
		protected String _OrigModule;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "OrigModule")]
		public virtual String OrigModule
		{
			get
			{
				return this._OrigModule;
			}
			set
			{
				this._OrigModule = value;
			}
		}
		#endregion
		#region OrigTranType
		public abstract class origTranType : PX.Data.BQL.BqlString.Field<origTranType>
		{
		}
		protected String _OrigTranType;
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "OrigTranType")]
		public virtual String OrigTranType
		{
			get
			{
				return this._OrigTranType;
			}
			set
			{
				this._OrigTranType = value;
			}
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr>
		{
		}
		protected String _OrigRefNbr;
		[PXDBString(PMRegister.refNbr.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "OrigRefNbr")]
		public virtual String OrigRefNbr
		{
			get
			{
				return this._OrigRefNbr;
			}
			set
			{
				this._OrigRefNbr = value;
			}
		}
		#endregion
		#region OrigLineNbr
		public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr>
		{
		}
		protected Int32? _OrigLineNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "OrigLineNbr")]
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
		#region BillingID
		public abstract class billingID : PX.Data.BQL.BqlString.Field<billingID>
		{
		}
		protected String _BillingID;
		[PXDBString(PMBilling.billingID.Length, IsUnicode = true)]
		public virtual String BillingID
		{
			get
			{
				return this._BillingID;
			}
			set
			{
				this._BillingID = value;
			}
		}
		#endregion
		#region AllocationID
		public abstract class allocationID : PX.Data.BQL.BqlString.Field<allocationID>
		{
		}
		protected String _AllocationID;
		[PXDBString(PMAllocation.allocationID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "AllocationID")]
		public virtual String AllocationID
		{
			get
			{
				return this._AllocationID;
			}
			set
			{
				this._AllocationID = value;
			}
		}
		#endregion
		#region Billed
		public abstract class billed : PX.Data.BQL.BqlBool.Field<billed>
		{
		}
		protected Boolean? _Billed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billed", Enabled = false)]
		public virtual Boolean? Billed
		{
			get
			{
				return this._Billed;
			}
			set
			{
				this._Billed = value;
			}
		}
		#endregion
		#region ExcludedFromBilling
		public abstract class excludedFromBilling : PX.Data.BQL.BqlBool.Field<excludedFromBilling> { }
		/// <summary>
		/// Excluded from Billing
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Excluded From Billing", Visible = false, Enabled = false)]
		public virtual bool? ExcludedFromBilling
		{
			get;
			set;
		}
		#endregion
		#region ExcludedFromBillingReason
		public abstract class excludedFromBillingReason : PX.Data.BQL.BqlString.Field<excludedFromBillingReason> { }
		/// <summary>
		/// Reason of exclusion from Billing 
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Excluded From Billing Reason", Visible = false, Enabled = false)]
		[PXFieldDescription]
		public virtual string ExcludedFromBillingReason
		{ 
			get;
			set;
		}
		#endregion
		#region Reversed 
		public abstract class reversed : PX.Data.BQL.BqlBool.Field<reversed>
		{
		}
		protected Boolean? _Reversed;
		/// <summary>
		/// When Transaction is marked as reverse it means that it should not be billed.
		/// </summary>
		[Obsolete("Will be removed in 2020R2")]
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Reversed to Non-Billable", Visible = false, Enabled = false)]
		public virtual Boolean? Reversed
		{
			get
			{
				return this._Reversed;
			}
			set
			{
				this._Reversed = value;
			}
		}
		#endregion
		#region ExcludedFromBalance
		public abstract class excludedFromBalance : Data.BQL.BqlBool.Field<excludedFromBalance> { }
		/// <summary>
		/// Excluded From Balance
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Excluded From Balance", Visible = false, Enabled = false)]
		public virtual bool? ExcludedFromBalance
		{
			get;
			set;
		}
		#endregion
		#region BilledDate
		public abstract class billedDate : PX.Data.BQL.BqlDateTime.Field<billedDate>
		{
		}
		protected DateTime? _BilledDate;
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "Billed Date")]
		public virtual DateTime? BilledDate
		{
			get
			{
				return this._BilledDate;
			}
			set
			{
				this._BilledDate = value;
			}
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate>
		{
		}
		protected DateTime? _StartDate;
		[PXDefault(typeof(PMTran.date), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDate()]
		[PXUIField(DisplayName = "Start Date", Visible = false)]
		public virtual DateTime? StartDate
		{
			get
			{
				return this._StartDate;
			}
			set
			{
				this._StartDate = value;
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate>
		{
		}
		protected DateTime? _EndDate;
		[PXDefault(typeof(PMTran.date), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDate()]
		[PXUIField(DisplayName = "End Date", Visible = false)]
		public virtual DateTime? EndDate
		{
			get
			{
				return this._EndDate;
			}
			set
			{
				this._EndDate = value;
			}
		}
		#endregion
		#region OrigRefID
		public abstract class origRefID : PX.Data.BQL.BqlGuid.Field<origRefID>
		{
		}
		protected Guid? _OrigRefID;
		/// <summary>
		/// Case.NoteID for Contracts and CRActivity.NoteID for Timecards/Timesheets
		/// </summary>
		[PXDBGuid]
		public virtual Guid? OrigRefID
		{
			get
			{
				return this._OrigRefID;
			}
			set
			{
				this._OrigRefID = value;
			}
		}
		#endregion
		#region IsNonGL
		public abstract class isNonGL : PX.Data.BQL.BqlBool.Field<isNonGL>
		{
		}
		protected Boolean? _IsNonGL;
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsNonGL
		{
			get
			{
				return this._IsNonGL;
			}
			set
			{
				this._IsNonGL = value;
			}
		}
		#endregion
		#region IsQtyOnly
		public abstract class isQtyOnly : PX.Data.BQL.BqlBool.Field<isQtyOnly>
		{
		}
		protected Boolean? _IsQtyOnly;

		/// <summary>
		/// Returns True if the transaction contains only qty data and no price/amount data.
		/// Ex: CRM records only usage without price information. Price is determined later while the billing process.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? IsQtyOnly
		{
			get
			{
				return this._IsQtyOnly;
			}
			set
			{
				this._IsQtyOnly = value;
			}
		}
		#endregion
		#region Reverse
		public abstract class reverse : PX.Data.BQL.BqlString.Field<reverse>
		{
		}
		protected String _Reverse;
		[PMReverse.List]
		[PXDefault(PMReverse.Never)]
		[PXDBString(1)]
		[PXUIField(DisplayName = "Reverse")]
		public virtual String Reverse
		{
			get
			{
				return this._Reverse;
			}
			set
			{
				this._Reverse = value;
			}
		}
		#endregion
		#region EarningTypeID
		public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }
		[PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
		[PXSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIField(DisplayName = "Earning Type", Enabled = false)]
		[PXForeignReference(typeof(Field<earningType>.IsRelatedTo<EPEarningType.typeCD>))]
		public virtual string EarningType { get; set; }
		#endregion
		#region OvertimeMultiplier
		public abstract class overtimeMultiplier : PX.Data.BQL.BqlDecimal.Field<overtimeMultiplier>
		{
		}
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Multiplier", Enabled = false)]
		public virtual Decimal? OvertimeMultiplier { get; set; }
		#endregion
		#region CaseCD
		public abstract class caseCD : PX.Data.BQL.BqlString.Field<caseCD> { }
		[PXSelector(typeof(Search<CRCase.caseCD>))]
		[PXDBString(10)]
		[PXUIField(DisplayName = "Case ID", Visible = false, Enabled = false)]
		public virtual string CaseCD { get; set; }
		#endregion
		#region UnionID
		public abstract class unionID : PX.Data.BQL.BqlString.Field<unionID>
		{
		}
		[PXForeignReference(typeof(Field<unionID>.IsRelatedTo<PMUnion.unionID>))]
		[PXDBString(PMUnion.unionID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Union Local", FieldClass = nameof(FeaturesSet.Construction), Enabled = false)]
		public virtual String UnionID
		{
			get;
			set;
		}
		#endregion
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID>
		{
		}
		[PXForeignReference(typeof(Field<workCodeID>.IsRelatedTo<PMWorkCode.workCodeID>))]
		[PXDBString(PMWorkCode.workCodeID.Length)]
		[PXUIField(DisplayName = "WCC Code", FieldClass = nameof(FeaturesSet.Construction), Enabled = false)]
		public virtual String WorkCodeID
		{
			get; set;
		}
		#endregion

		//Reference to ARInvoice: after ContractBilling
		#region ARTranType
		public abstract class aRTranType : PX.Data.BQL.BqlString.Field<aRTranType>
		{
		}
		protected String _ARTranType;

		[AR.ARInvoiceType.List]
		[PXDBString(3, IsFixed = true)]
		public virtual String ARTranType
		{
			get
			{
				return this._ARTranType;
			}
			set
			{
				this._ARTranType = value;
			}
		}
		#endregion
		#region ARRefNbr
		public abstract class aRRefNbr : PX.Data.BQL.BqlString.Field<aRRefNbr>
		{
		}
		protected String _ARRefNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "AR Reference Nbr.")]
		[PXSelector(typeof(Search<PX.Objects.AR.ARInvoice.refNbr>))]
		public virtual String ARRefNbr
		{
			get
			{
				return this._ARRefNbr;
			}
			set
			{
				this._ARRefNbr = value;
			}
		}
		#endregion
		#region RefLineNbr
		public abstract class refLineNbr : PX.Data.BQL.BqlInt.Field<refLineNbr>
		{
		}
		protected Int32? _RefLineNbr;
		[PXDBInt()]
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

		//Reference to Original Task for Budget Allocation 
		#region OrigProjectID
		public abstract class origProjectID : PX.Data.BQL.BqlInt.Field<origProjectID>
		{
		}
		protected Int32? _OrigProjectID;
		[PXDBInt()]
		public virtual Int32? OrigProjectID
		{
			get
			{
				return this._OrigProjectID;
			}
			set
			{
				this._OrigProjectID = value;
			}
		}
		#endregion
		#region OrigTaskID
		public abstract class origTaskID : PX.Data.BQL.BqlInt.Field<origTaskID>
		{
		}
		protected Int32? _OrigTaskID;
		[PXDBInt()]
		public virtual Int32? OrigTaskID
		{
			get
			{
				return this._OrigTaskID;
			}
			set
			{
				this._OrigTaskID = value;
			}
		}
		#endregion
		#region OrigAccountGroupID
		public abstract class origAccountGroupID : PX.Data.BQL.BqlInt.Field<origAccountGroupID>
		{
		}
		protected Int32? _OrigAccountGroupID;
		[PXDBInt()]
		public virtual Int32? OrigAccountGroupID
		{
			get
			{
				return this._OrigAccountGroupID;
			}
			set
			{
				this._OrigAccountGroupID = value;
			}
		}
		#endregion

		//Reference to ProformaLine:
		#region ProformaRefNbr
		public abstract class proformaRefNbr : PX.Data.BQL.BqlString.Field<proformaRefNbr> { }

		[PXUIField(DisplayName = "Pro Forma Ref. Nbr.")]
		[PXDBString(PMProforma.refNbr.Length, IsUnicode = true)]
		public virtual String ProformaRefNbr
		{
			get; set;
		}
		#endregion
		#region ProformaLineNbr
		public abstract class proformaLineNbr : PX.Data.BQL.BqlInt.Field<proformaLineNbr>
		{
		}
		[PXDBInt()]
		public virtual Int32? ProformaLineNbr
		{
			get; set;
		}
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr>
		{
		}
		protected String _ExtRefNbr;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Nbr.")]
		public virtual String ExtRefNbr
		{
			get
			{
				return this._ExtRefNbr;
			}
			set
			{
				this._ExtRefNbr = value;
			}
		}
		#endregion

		#region OrigTranID
		public abstract class origTranID : PX.Data.BQL.BqlLong.Field<origTranID> { }
		/// <summary>
		/// When reversal is created it holds the reference to the original transaction.
		/// </summary>
		[PXDBLong]
		public virtual long? OrigTranID
		{
			get;
			set;
		}
		#endregion
		#region RemainderOfTranID
		public abstract class remainderOfTranID : PX.Data.BQL.BqlLong.Field<remainderOfTranID> { }
		/// <summary>
		/// When remainder is created it holds the reference to the original transaction.
		/// </summary>
		[PXDBLong]
		public virtual long? RemainderOfTranID
		{
			get;
			set;
		}
		#endregion
		#region DuplicateOfTranID
		public abstract class duplicateOfTranID : PX.Data.BQL.BqlLong.Field<duplicateOfTranID> { }
		/// <summary>
		/// When CreditMemo is created as a result of Project's Invoice reversal. A copy of original billable transaction is created so that it can be billed again.
		/// </summary>
		[Obsolete("Will be removed in 2020R2")]
		[PXDBLong]
		public virtual long? DuplicateOfTranID
		{
			get;
			set;
		}
		#endregion

		#region IsFree
		public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree>
		{
		}
		protected bool? _IsFree = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsFree
		{
			get
			{
				return _IsFree;
			}
			set
			{
				_IsFree = value;
			}
		}
		#endregion
		#region Proportion
		public abstract class proportion : PX.Data.BQL.BqlDecimal.Field<proportion>
		{
		}
		protected decimal? _Proportion;
		[PXDecimal]
		public virtual decimal? Proportion
		{
			get
			{
				return _Proportion;
			}
			set
			{
				_Proportion = value;
			}
		}
		#endregion
		#region Skip
		public abstract class skip : PX.Data.BQL.BqlBool.Field<skip>
		{
		}
		protected bool? _Skip = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField]
		public virtual bool? Skip
		{
			get
			{
				return _Skip;
			}
			set
			{
				_Skip = value;
			}
		}
		#endregion
		#region Prefix
		public abstract class prefix : PX.Data.BQL.BqlString.Field<prefix>
		{
		}
		protected String _Prefix;
		[PXString(255, IsUnicode = true)]
		public virtual String Prefix
		{
			get
			{
				return this._Prefix;
			}
			set
			{
				this._Prefix = value;
			}
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID>
		{
		}
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp>
		{
		}
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
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID>
		{
		}
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
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID>
		{
		}
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
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime>
		{
		}
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
		{
		}
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
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
		{
		}
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
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
		{
		}
		protected DateTime? _LastModifiedDateTime;
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#endregion

		[PXBool]
		public bool? IsInverted { get; set; }

		[PXBool]
		public bool? IsCreditPair { get; set; }

		/// <summary>
		/// Gets or sets Rate
		/// </summary>
		public decimal? Rate { get; set; }

		#region CreatedByCurrentAllocation
		public abstract class createdByCurrentAllocation : PX.Data.BQL.BqlBool.Field<createdByCurrentAllocation>
		{
		}
		protected bool? _CreatedByCurrentAllocation = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? CreatedByCurrentAllocation
		{
			get
			{
				return _CreatedByCurrentAllocation;
			}
			set
			{
				_CreatedByCurrentAllocation = value;
			}
		}
		#endregion
	}

	public class PXDBProjectCuryAttribute : PXDBDecimalAttribute
	{
		public PXDBProjectCuryAttribute() : base(typeof(
			Search2<Currency.decimalPlaces,
			InnerJoin<PMProject, On<PMProject.curyID, Equal<Currency.curyID>>>,
			Where<PMProject.contractID, Equal<Current<PMTran.projectID>>>>))
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}

	}
}