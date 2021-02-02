using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.Common.EntityInUse;
using PX.Objects.IN;

namespace PX.Objects.CS
{
	[PXCacheName(Messages.FeaturesSet)]
	[PXPrimaryGraph(typeof(FeaturesMaint))]
    [Serializable]
	public class FeaturesSet : IBqlTable
	{
		#region LicenseID
		public abstract class licenseID : PX.Data.BQL.BqlString.Field<licenseID> { }
		protected String _LicenseID;
		[PXString(64, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "License ID", Visible = false)]
		public virtual String LicenseID
		{
			get
			{
				return this._LicenseID;
			}
			set
			{
				this._LicenseID = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(3)]
		[PXIntList(
			new int[] { 0, 1, 2, 3 },
			new string[] { "Validated", "Failed Validation", "Pending Validation", "Pending Activation" }
		)]
		[PXUIField(DisplayName = "Status", Enabled = false)]
		public int? Status
		{
			get;
			set;
		}
		#endregion
		#region ValidUntill
		public abstract class validUntill : PX.Data.BQL.BqlDateTime.Field<validUntill> { }
		protected DateTime? _ValidUntill;
		[PXDBDate()]
		[PXUIField(DisplayName = "Next Validation Date", Enabled = false, Visible = false)]
		public virtual DateTime? ValidUntill
		{
			get
			{
				return this._ValidUntill;
			}
			set
			{
				this._ValidUntill = value;
			}
		}
		#endregion		
		#region ValidationCode
		public abstract class validationCode : PX.Data.BQL.BqlString.Field<validationCode> { }
		protected String _ValidationCode;
		[PXString(500, IsUnicode = true, InputMask = "")]
		public virtual String ValidationCode
		{
			get
			{
				return this._ValidationCode;
			}
			set
			{
				this._ValidationCode = value;
			}
		}
		#endregion

        #region FinancialModule
        public abstract class financialModule : PX.Data.BQL.BqlBool.Field<financialModule> { }
        protected bool? _FinancialModule;
        [Feature(true, null, typeof(Select<GL.GLSetup>), DisplayName = "Finance", Enabled = false)]
        public virtual bool? FinancialModule
		{
			get
			{
                return this._FinancialModule;
			}
			set
			{
                this._FinancialModule = value;
			}
		}
		#endregion
        #region FinancialStandard
        public abstract class financialStandard : PX.Data.BQL.BqlBool.Field<financialStandard> { }
        protected bool? _FinancialStandard;
        [Feature(true, typeof(FeaturesSet.financialModule), null, DisplayName = "Standard Financials", SyncToParent = true)]
        public virtual bool? FinancialStandard
		{
			get
			{
                return this._FinancialStandard;
			}
			set
			{
                this._FinancialStandard = value;
			}
		}
		#endregion
		#region Branch
		public abstract class branch : PX.Data.BQL.BqlBool.Field<branch> { }
		protected bool? _Branch;
        [Feature(typeof(FeaturesSet.financialStandard), typeof(Select2<CS.BranchMaint.BranchBAccount,
			InnerJoin<GL.Branch, On<GL.Branch.bAccountID, NotEqual<CS.BranchMaint.BranchBAccount.bAccountID>>>>), 
			DisplayName = "Multi-Branch Support")]
		public virtual bool? Branch
		{
			get
			{
				return this._Branch;
			}
			set
			{
				this._Branch = value;
			}
		}
		#endregion
        #region AccountLocations
        public abstract class accountLocations : PX.Data.BQL.BqlBool.Field<accountLocations> { }
        protected bool? _AccountLocations;
        [Feature(false, typeof(FeaturesSet.financialStandard), DisplayName = "Business Account Locations")]
        public virtual bool? AccountLocations
        {
            get
            {
                return this._AccountLocations;
            }
            set
		{
                this._AccountLocations = value;
            }
		}
		#endregion
		#region Multicurrency
		public abstract class multicurrency : PX.Data.BQL.BqlBool.Field<multicurrency> { }
		protected bool? _Multicurrency;
        [Feature(typeof(FeaturesSet.financialStandard), typeof(Select<CM.CMSetup>), DisplayName = "Multi-Currency Accounting")]
		public virtual bool? Multicurrency
		{
			get
			{
				return this._Multicurrency;
			}
			set
			{
				this._Multicurrency = value;
			}
		}
		#endregion		
		#region CentralizedPeriodsManagement

		public abstract class centralizedPeriodsManagement : PX.Data.BQL.BqlBool.Field<centralizedPeriodsManagement> { }
		[Feature(true, typeof(FeaturesSet.financialStandard), DisplayName = "Centralized Period Management")]
		[PXFormula(typeof(IIf<Where<FeaturesSet.branch, NotEqual<True>>, True, FeaturesSet.centralizedPeriodsManagement>))]
		public virtual bool? CentralizedPeriodsManagement { get; set; }
		#endregion
		#region SupportBreakQty
		public abstract class supportBreakQty : PX.Data.BQL.BqlBool.Field<supportBreakQty> { }
        protected bool? _SupportBreakQty;
        [Feature(false, typeof(FeaturesSet.financialStandard), DisplayName = "Volume Pricing")]
        public virtual bool? SupportBreakQty
        {
            get
            {
                return this._SupportBreakQty;
            }
            set
            {
                this._SupportBreakQty = value;
            }
        }
        #endregion
        #region Prebooking
        public abstract class prebooking : PX.Data.BQL.BqlBool.Field<prebooking> { }
        protected bool? _Prebooking;
        [Feature(typeof(FeaturesSet.financialStandard), typeof(Select<AP.APRegister, Where<AP.APRegister.prebookBatchNbr, IsNotNull>>), DisplayName = "Expense Reclassification")]
        public virtual bool? Prebooking
        {
            get;
            set;
        }
        #endregion
        #region TaxEntryFromGL
        public abstract class taxEntryFromGL : PX.Data.BQL.BqlBool.Field<taxEntryFromGL>
		{
			public static readonly string EntityInUseKey = typeof(taxEntryFromGL).FullName;

			public class entityInUseKey : Constant<string>
			{
				public entityInUseKey() : base(EntityInUseKey) { }
			}
		}

        protected bool? _TaxEntryFromGL;
        [Feature(false, typeof(FeaturesSet.financialStandard), typeof(Select<FeatureInUse, Where<FeatureInUse.featureName, Equal<taxEntryFromGL.entityInUseKey>>>), 
			DisplayName = "Tax Entry from GL Module")]
        public virtual bool? TaxEntryFromGL
		{
			get
			{
                return this._TaxEntryFromGL;
			}
			set
			{
                this._TaxEntryFromGL = value;
			}
		}
		#endregion
        #region VATReporting
        public abstract class vATReporting : PX.Data.BQL.BqlBool.Field<vATReporting> { }
        protected bool? _VATReporting;
        [Feature(typeof(FeaturesSet.financialStandard), typeof(Select<TX.Tax, Where<TX.Tax.taxType, Equal<TX.CSTaxType.vat>>>), DisplayName = "VAT Reporting")]
        public virtual bool? VATReporting
		{
			get
			{
                return this._VATReporting;
			}
			set
			{
                this._VATReporting = value;
			}
		}
		#endregion
		#region VATReporting
		public abstract class reporting1099 : PX.Data.BQL.BqlBool.Field<reporting1099> { }
		protected bool? _Reporting1099;
		[Feature(typeof(FeaturesSet.financialStandard), DisplayName = "1099 Reporting")]
		public virtual bool? Reporting1099
		{
			get
			{
				return this._Reporting1099;
			}
			set
			{
				this._Reporting1099 = value;
			}
		}
		#endregion
		#region NetGrossEntryMode
		public abstract class netGrossEntryMode : PX.Data.BQL.BqlBool.Field<netGrossEntryMode> { }
		protected bool? _NetGrossEntryMode;
		[Feature(typeof(FeaturesSet.financialStandard), DisplayName = "Net/Gross Entry Mode")]
		public virtual bool? NetGrossEntryMode
		{
			get
			{
				return this._NetGrossEntryMode;
			}
			set
			{
				this._NetGrossEntryMode = value;
			}
		}
		#endregion
		#region ManualVATEntryMode
		public abstract class manualVATEntryMode : PX.Data.BQL.BqlBool.Field<manualVATEntryMode> { }
		protected bool? _ManualVATEntryMode;
		[Feature(typeof(FeaturesSet.netGrossEntryMode), typeof(Select<TX.TaxZone,Where<TX.TaxZone.isManualVATZone, Equal<True>>>), DisplayName = "Manual VAT Entry Mode", Visible = false)]
		public virtual bool? ManualVATEntryMode
		{
			get
			{
				return this._ManualVATEntryMode;
			}
			set
			{
				this._ManualVATEntryMode = value;
			}
		}
		#endregion
        #region InvoiceRounding
        public abstract class invoiceRounding : PX.Data.BQL.BqlBool.Field<invoiceRounding> { }
        protected bool? _InvoiceRounding;
        [Feature(typeof(FeaturesSet.financialStandard), DisplayName = "Invoice Rounding")]
        public virtual bool? InvoiceRounding
        {
            get;
            set;
        }
		#endregion
		#region ExpenseManagement
		public abstract class expenseManagement : PX.Data.BQL.BqlBool.Field<expenseManagement> { }

		[Feature(false, typeof(FeaturesSet.financialStandard), DisplayName = "Expense Management")]
		[FeatureRestrictor(typeof(Select<EP.EPExpenseClaimDetails>))]
		[FeatureRestrictor(typeof(Select<EP.EPExpenseClaim>))]
		public virtual bool? ExpenseManagement { get; set; }
		#endregion
		#region RutRotDeduction
		public abstract class rutRotDeduction : PX.Data.BQL.BqlBool.Field<rutRotDeduction> { }
        protected bool? _RutRotDeduction;
        [Feature(false, typeof(FeaturesSet.financialStandard), typeof(Select<RUTROT.RUTROTSetup>), DisplayName = "ROT & RUT Deduction", Visible = false)]
        public virtual bool? RutRotDeduction
        {
            get
            {
                return this._RutRotDeduction;
            }
            set
            {
                this._RutRotDeduction = value;
            }
        }
        #endregion		
		#region GLWorkBooks
		public abstract class gLWorkBooks : PX.Data.BQL.BqlBool.Field<gLWorkBooks> { }
		protected bool? _GLWorkBooks;
		[Feature(false, typeof(FeaturesSet.financialStandard), typeof(Select<GL.GLWorkBook>), DisplayName = "GL Workbooks", Visible =false)]
		public virtual bool? GLWorkBooks
		{
			get
			{
				return this._GLWorkBooks;
			}
			set
			{
				this._GLWorkBooks = value;
			}
		}
		#endregion		
        

        #region FinancialAdvanced
        public abstract class financialAdvanced : PX.Data.BQL.BqlBool.Field<financialAdvanced> { }
        protected bool? _FinancialAdvanced;
        [Feature(typeof(FeaturesSet.financialModule), null, DisplayName = "Advanced Financials")]
        public virtual bool? FinancialAdvanced
		{
			get
			{
                return this._FinancialAdvanced;
			}
			set
			{
                this._FinancialAdvanced = value;
			}
		}
		#endregion
        #region SubAccount
        public abstract class subAccount : PX.Data.BQL.BqlBool.Field<subAccount> { }
        protected bool? _SubAccount;
        [Feature(typeof(FeaturesSet.financialAdvanced), typeof(Select<GL.Sub, Where<GL.Sub.subCD, NotEqual<IN.INSubItem.Zero>>>), DisplayName = "Subaccounts")]
        public virtual bool? SubAccount
		{
			get
			{
                return this._SubAccount;
			}
			set
			{
                this._SubAccount = value;
			}
		}
		#endregion
        #region AllocationTemplates
        public abstract class allocationTemplates : PX.Data.BQL.BqlBool.Field<allocationTemplates> { }
        protected bool? _AllocationTemplates;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "General Ledger Allocation Templates")]
        public virtual bool? AllocationTemplates
        {
            get
		{
                return this._AllocationTemplates;
		}
            set
		{
                this._AllocationTemplates = value;
            }
		}
		#endregion
        #region Inter-Branch Accounting
        public abstract class interBranch : PX.Data.BQL.BqlBool.Field<interBranch> { }
        //[FeatureRestrictor(typeof(Select2<GL.Ledger, InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<GL.Ledger.defBranchID>>>>))]
        //[FeatureRestrictor(typeof(Select2<GL.Ledger, InnerJoin<GL.GLHistory, On<GL.GLHistory.ledgerID, Equal<GL.Ledger.ledgerID>>>, 
        //	Where<GL.Ledger.balanceType,Equal<GL.LedgerBalanceType.actual>, And<GL.Ledger.postInterCompany, Equal<True>>>>))]
        [FeatureRestrictor(typeof(Select2<GL.BranchAcctMap, InnerJoin<GL.Branch, On<GL.Branch.branchID, Equal<GL.BranchAcctMap.branchID>, And<GL.Branch.active, Equal<True>>>>>))]
        [FeatureDependency(typeof(FeaturesSet.branch))]
        [Feature(typeof(FeaturesSet.financialAdvanced), null, DisplayName = "Inter-Branch Transactions")]        
        public virtual bool? InterBranch { get; set; }
		#endregion
		#region MultipleCalendarsSupport
		public abstract class multipleCalendarsSupport :  PX.Data.BQL.BqlBool.Field<multipleCalendarsSupport> { }
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Multiple Calendar Support")]
		public virtual bool? MultipleCalendarsSupport { get; set; }
        #endregion
        #region GLConsolidation
        public abstract class gLConsolidation : PX.Data.BQL.BqlBool.Field<gLConsolidation> { }
        protected bool? _GLConsolidation;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "General Ledger Consolidation")]
        public virtual bool? GLConsolidation
        {
            get
        {
                return this._GLConsolidation;
        }
            set
        {
                this._GLConsolidation = value;
            }
        }
        #endregion
        #region FinStatementCurTranslation
        public abstract class finStatementCurTranslation : PX.Data.BQL.BqlBool.Field<finStatementCurTranslation> { }
        protected bool? _FinStatementCurTranslation;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Translation of Financial Statements")]
        [FeatureDependency(typeof(FeaturesSet.multicurrency))]
        public virtual bool? FinStatementCurTranslation
        {
            get
		{
                return this._FinStatementCurTranslation;
		}
            set
            {
                this._FinStatementCurTranslation = value;
            }
        }
        #endregion
		#region CustomerDiscounts
		public abstract class customerDiscounts : PX.Data.BQL.BqlBool.Field<customerDiscounts> { }
		protected bool? _CustomerDiscounts;
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Customer Discounts")]
		public virtual bool? CustomerDiscounts
		{
			get
			{
				return this._CustomerDiscounts;
			}
			set
			{
				this._CustomerDiscounts = value;
			}
		}
		#endregion
        #region VendorDiscounts
        public abstract class vendorDiscounts : PX.Data.BQL.BqlBool.Field<vendorDiscounts> { }
        protected bool? _VendorDiscounts;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Vendor Discounts")]
        public virtual bool? VendorDiscounts
		{
			get
			{
                return this._VendorDiscounts;
			}
			set
			{
                this._VendorDiscounts = value;
			}
		}
		#endregion
        #region Commissions
        public abstract class commissions : PX.Data.BQL.BqlBool.Field<commissions> { }
        protected bool? _Commissions;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Commissions")]
        public virtual bool? Commissions
		{
			get
			{
                return this._Commissions;
			}
			set
			{
                this._Commissions = value;
			}
		}
		#endregion
        #region OverdueFinCharges
        public abstract class overdueFinCharges : PX.Data.BQL.BqlBool.Field<overdueFinCharges> { }
        protected bool? _OverdueFinCharges;
        [Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Overdue Charges")]
        public virtual bool? OverdueFinCharges
		{
			get
			{
                return this._OverdueFinCharges;
			}
			set
			{
                this._OverdueFinCharges = value;
			}
		}
		#endregion
        #region DunningLetter
        public abstract class dunningLetter : PX.Data.BQL.BqlBool.Field<dunningLetter> { }
        protected bool? _DunningLetter;
        [Feature(typeof(FeaturesSet.financialAdvanced), typeof(Select<AR.ARDunningSetup>), DisplayName = "Dunning Letter Management")]
        public virtual bool? DunningLetter
		{
			get
			{
                return this._DunningLetter;
			}
			set
			{
                this._DunningLetter = value;
			}
		}
		#endregion
        #region DefferedRevenue
        public abstract class defferedRevenue : PX.Data.BQL.BqlBool.Field<defferedRevenue> { }
        protected bool? _DefferedRevenue;
        [Feature(typeof(FeaturesSet.financialAdvanced), typeof(Select<DR.DRSchedule>), DisplayName = "Deferred Revenue Management")]
        public virtual bool? DefferedRevenue
        {
            get
            {
                return this._DefferedRevenue;
            }
            set
		{
                this._DefferedRevenue = value;
            }
		}
		#endregion
		#region ASC606
		public abstract class aSC606 : PX.Data.BQL.BqlBool.Field<aSC606> { }

		[Feature(false, typeof(FeaturesSet.defferedRevenue), DisplayName = "Revenue Recognition by IFRS 15/ASC 606")]
		public virtual bool? ASC606 { get; set; }
		#endregion
		#region ConsolidatedPosting
		public abstract class consolidatedPosting : PX.Data.BQL.BqlBool.Field<consolidatedPosting> { }
		protected bool? _ConsolidatedPosting;

		[Obsolete("ConsolidatedPosting setting was moved to GLSetup. FeatureSet.ConsolidatedPosting will be fully eliminated in the future version.")]
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Consolidated Posting to GL", Visible = false)]
		public virtual bool? ConsolidatedPosting
		{
			get
			{
				return this._ConsolidatedPosting;
			}
			set
			{
				this._ConsolidatedPosting = value;
			}
		}
		#endregion
		#region ParentChildAccount
		public abstract class parentChildAccount : PX.Data.BQL.BqlBool.Field<parentChildAccount> { }
		protected bool? _ParentChildAccount;

		[Feature(false, typeof(FeaturesSet.financialAdvanced), typeof(Select<AR.Customer, 
			Where<AR.Customer.consolidatingBAccountID, NotEqual<AR.Customer.bAccountID>, 
				Or<AR.Customer.statementCustomerID, NotEqual<AR.Customer.bAccountID>,
				Or<AR.Customer.sharedCreditCustomerID, NotEqual<AR.Customer.bAccountID>>>>>), DisplayName = "Parent-Child Customer Relationship")]
		public virtual bool? ParentChildAccount
		{
			get { return _ParentChildAccount; }
			set { _ParentChildAccount = value; }
		}
		#endregion
		#region Retainage
		public abstract class retainage : PX.Data.BQL.BqlBool.Field<retainage> { }
		
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Retainage Support")]
		[FeatureRestrictor(typeof(Select<AP.APRegister, Where<AP.APRegister.isRetainageDocument, Equal<True>, Or<AP.APRegister.retainageApply, Equal<True>>>>))]
		[FeatureRestrictor(typeof(Select<AR.ARRegister, Where<AR.ARRegister.isRetainageDocument, Equal<True>, Or<AR.ARRegister.retainageApply, Equal<True>>>>))]
		[FeatureRestrictor(typeof(Select<PO.POOrder, Where<PO.POOrder.retainageApply, Equal<True>>>))]
		[FeatureRestrictor(typeof(Select<PM.PMProject, Where<PM.PMProject.retainagePct, Greater<decimal0>>>))]
		[FeatureRestrictor(typeof(Select<PM.PMBudget, Where<PM.PMBudget.retainagePct, Greater<decimal0>>>))]
		[FeatureRestrictor(typeof(Select<PM.PMProforma, Where<PM.PMProforma.curyRetainageDetailTotal, Greater<decimal0>,Or<PM.PMProforma.curyRetainageTaxTotal, Greater<decimal0>>>>))]
		public virtual bool? Retainage
		{
			get;
			set;
		}
		#endregion

		#region PerUnitTaxSupport
		public abstract class perUnitTaxSupport : PX.Data.BQL.BqlBool.Field<perUnitTaxSupport> { }

		[Feature(defValue: false, parent: typeof(FeaturesSet.financialAdvanced),
				 checkUsage: typeof(Select<TX.Tax, Where<TX.Tax.taxType, Equal<TX.CSTaxType.perUnit>>>),
				 DisplayName = "Per Unit/Specific Tax Support", Visible = false)]
		public virtual bool? PerUnitTaxSupport
		{
			get;
			set;
		}
		#endregion

		#region PaymentsByLines
		public abstract class paymentsByLines : PX.Data.BQL.BqlBool.Field<paymentsByLines> { }

		[FeatureRestrictor(typeof(Select<AP.APRegister, Where<AP.APRegister.paymentsByLinesAllowed, Equal<True>>>))]
		[FeatureRestrictor(typeof(Select<AR.ARRegister, Where<AR.ARRegister.paymentsByLinesAllowed, Equal<True>>>))]
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Payment Application by Line", Visible = true)]
		public virtual bool? PaymentsByLines { get; set; }
		#endregion
		#region ExemptedTaxReporting
		public abstract class exemptedTaxReporting : IBqlField { }

		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Exempted Tax Reporting", Visible = false)]
		public virtual bool? ExemptedTaxReporting
		{
			get;
			set;
		}
		#endregion
		#region Contract
		public abstract class contractManagement : PX.Data.BQL.BqlBool.Field<contractManagement> { }
        protected bool? _ContractManagement;
        [Feature(typeof(FeaturesSet.financialModule), typeof(Select<CT.Contract, Where<CT.Contract.nonProject, Equal<False>, And<CT.Contract.baseType, Equal<CT.CTPRType.contract>>>>), DisplayName = "Contract Management")]
        public virtual bool? ContractManagement
        {
            get;
            set;
        }
        #endregion
		#region FixedAsset
		public abstract class fixedAsset : PX.Data.BQL.BqlBool.Field<fixedAsset> { }
		protected bool? _FixedAsset;
		[Feature(typeof(FeaturesSet.financialModule), typeof(Select<FA.FASetup>), DisplayName = "Fixed Asset Management")]
		public virtual bool? FixedAsset
		{
			get
			{
				return this._FixedAsset;
			}
			set
			{
				this._FixedAsset = value;
			}
		}
		#endregion
        
		#region IncomingPayments
		public abstract class incomingPayments : PX.Data.BQL.BqlBool.Field<incomingPayments> { }
		[Feature(false, typeof(FeaturesSet.financialModule), DisplayName = "Incoming Payments", Visible = false)]
		public virtual bool? IncomingPayments
		{
			get;
			set;
		}
		#endregion
		#region MultipleMatchingForBankTransactions
		public abstract class multipleMatchingForBankTransactions : PX.Data.BQL.BqlBool.Field<multipleMatchingForBankTransactions> { }
		[Feature(false, typeof(FeaturesSet.financialAdvanced), DisplayName = "Multiple Matching for Bank Transactions", Visible = false)]
		public virtual bool? MultipleMatchingForBankTransactions
		{
			get;
			set;
		}
		#endregion


		#region MiscModule
		public abstract class miscModule : PX.Data.BQL.BqlBool.Field<miscModule> { }
		protected bool? _MiscModule;
        [Feature(true, DisplayName = "Monitoring & Automation", Enabled = false)]
		public virtual bool? MiscModule
		{
			get
			{
				return this._MiscModule;
			}
			set
			{
				this._MiscModule = value;
			}
		}
		#endregion
        #region TimeReportingModule
        public abstract class timeReportingModule : PX.Data.BQL.BqlBool.Field<timeReportingModule> { }
        protected bool? _TimeReportingModule;
        [FeatureRestrictor(typeof(Select<CR.PMTimeActivity, Where<CR.PMTimeActivity.trackTime, Equal<boolTrue>>>))]
        [Feature(false, typeof(FeaturesSet.miscModule), DisplayName = "Time Reporting on Activity")]
        public virtual bool? TimeReportingModule
		{
			get
			{
                return this._TimeReportingModule;
			}
			set
			{
                this._TimeReportingModule = value;
			}
		}
		#endregion
		#region ApprovalWorkflow
        public abstract class approvalWorkflow : PX.Data.BQL.BqlBool.Field<approvalWorkflow> { }
        protected bool? _ApprovalWorkflow;
        [Feature(typeof(FeaturesSet.miscModule), DisplayName = "Approval Workflow")]
        public virtual bool? ApprovalWorkflow
		{
			get
			{
                return this._ApprovalWorkflow;
			}
			set
			{
                this._ApprovalWorkflow = value;
			}
		}
		#endregion
        #region FieldLevelLogging
        public abstract class fieldLevelLogging : PX.Data.BQL.BqlBool.Field<fieldLevelLogging> { }
        protected bool? _FieldLevelLogging;
        [Feature(typeof(FeaturesSet.miscModule), DisplayName = "Field-Level Audit")]
        public virtual bool? FieldLevelLogging
		{
			get
			{
                return this._FieldLevelLogging;
			}
			set
			{
                this._FieldLevelLogging = value;
			}
		}
		#endregion
		#region RowLevelSecurity
		public abstract class rowLevelSecurity : PX.Data.BQL.BqlBool.Field<rowLevelSecurity> { }
		protected bool? _RowLevelSecurity;
		[Feature(typeof(FeaturesSet.miscModule), typeof(Select<PX.SM.RelationGroup>), DisplayName = "Row-Level Security")]
		public virtual bool? RowLevelSecurity
		{
			get
			{
				return this._RowLevelSecurity;
			}
			set
			{
				this._RowLevelSecurity = value;
			}
		}
		#endregion
        #region ScheduleModule
        public abstract class scheduleModule : PX.Data.BQL.BqlBool.Field<scheduleModule> { }
        protected bool? _ScheduleModule;
        [Feature(true, typeof(FeaturesSet.miscModule), typeof(Select<PX.SM.AUSchedule, Where<PX.SM.AUSchedule.isActive, Equal<True>>>), DisplayName = "Scheduled Processing")]
        public virtual bool? ScheduleModule
		{
			get
			{
                return this._ScheduleModule;
			}
			set
			{
                this._ScheduleModule = value;
			}
		}
		#endregion
        #region NotificationModule
        public abstract class notificationModule : PX.Data.BQL.BqlBool.Field<notificationModule> { }
        protected bool? _NotificationModule;
        [Feature(false, typeof(FeaturesSet.miscModule), typeof(Select<PX.SM.AUNotification, Where<PX.SM.AUNotification.isActive, Equal<True>>>), DisplayName = "Change Notifications")]
        public virtual bool? NotificationModule
		{
			get
			{
                return this._NotificationModule;
			}
			set
			{
                this._NotificationModule = value;
			}
		}
		#endregion
        #region AutomationModule
        public abstract class automationModule : PX.Data.BQL.BqlBool.Field<automationModule> { }
        protected bool? _AutomationModule;
        [Feature(true, typeof(FeaturesSet.miscModule), DisplayName = "Workflow Automation")]
        public virtual bool? AutomationModule
		{
			get
			{
                return this._AutomationModule;
			}
			set
			{
                this._AutomationModule = value;
			}
		}
		#endregion
		#region DeviceHub
		public abstract class deviceHub : PX.Data.BQL.BqlBool.Field<deviceHub> { }
		protected bool? _DeviceHub;
		[Feature(typeof(FeaturesSet.miscModule), DisplayName = "DeviceHub")]
		public virtual bool? DeviceHub
		{
			get
			{
				return this._DeviceHub;
			}
			set
			{
				this._DeviceHub = value;
			}
		}
		#endregion
		#region GDPRCompliance
		public abstract class gDPRCompliance : PX.Data.BQL.BqlBool.Field<gDPRCompliance>
		{
			public const string FieldClass = "GDPR";
		}

		[Feature(true, typeof(FeaturesSet.miscModule), DisplayName = "GDPR Compliance Tools")]
		public virtual bool? GDPRCompliance { get; set; }
        #endregion
        #region SecureBusinessDate
        public abstract class secureBusinessDate : PX.Data.BQL.BqlBool.Field<secureBusinessDate> { }
	    [Feature(false, typeof(FeaturesSet.miscModule), DisplayName = "Secure Business Date")]
	    public virtual bool? SecureBusinessDate { get; set; }
		#endregion
		

		#region DistributionModule
		public abstract class distributionModule : PX.Data.BQL.BqlBool.Field<distributionModule> { }
        protected bool? _DistributionModule;
        [Feature(typeof(FeaturesSet.financialModule), typeof(Select<IN.INSetup>), Top = true, DisplayName = "Inventory and Order Management")]
        public virtual bool? DistributionModule
		{
			get
			{
                return this._DistributionModule;
			}
			set
			{
                this._DistributionModule = value;
			}
		}
		#endregion
		#region Inventory
		public abstract class inventory : PX.Data.BQL.BqlBool.Field<inventory> { }
		protected bool? _Inventory;
		[Feature(typeof(FeaturesSet.distributionModule), null, DisplayName = "Inventory", SyncToParent = false)]
		public virtual bool? Inventory
		{
			get
			{
				return this._Inventory;
			}
			set
			{
				this._Inventory = value;
			}
		}
		#endregion
		#region MultipleUnitMeasure
		public abstract class multipleUnitMeasure : PX.Data.BQL.BqlBool.Field<multipleUnitMeasure> { }
		protected bool? _MultipleUnitMeasure;
		[FeatureRestrictor(typeof(Select<IN.InventoryItem, 
			Where<IN.InventoryItem.baseUnit, NotEqual<IN.InventoryItem.salesUnit>,
				Or<IN.InventoryItem.baseUnit, NotEqual<IN.InventoryItem.purchaseUnit>,
				Or<IN.InventoryItem.decimalSalesUnit, NotEqual<IN.InventoryItem.decimalBaseUnit>,
				Or<IN.InventoryItem.decimalPurchaseUnit, NotEqual<IN.InventoryItem.decimalBaseUnit>>>>>>))]
		[FeatureRestrictor(typeof(Select<IN.INItemClass, 
			Where<IN.INItemClass.baseUnit, NotEqual<IN.INItemClass.salesUnit>, 
				Or<IN.INItemClass.baseUnit, NotEqual<IN.INItemClass.purchaseUnit>,
				Or<IN.INItemClass.decimalSalesUnit, NotEqual<IN.INItemClass.decimalBaseUnit>,
				Or<IN.INItemClass.decimalPurchaseUnit, NotEqual<IN.INItemClass.decimalBaseUnit>>>>>>))]
        [Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Multiple Units of Measure")]
		public virtual bool? MultipleUnitMeasure
		{
			get
			{
				return this._MultipleUnitMeasure;
			}
			set
			{
				this._MultipleUnitMeasure = value;
			}
		}
		#endregion
		#region LotSerialTracking
		public abstract class lotSerialTracking : PX.Data.BQL.BqlBool.Field<lotSerialTracking> { }
		protected bool? _LotSerialTracking;
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Lot and Serial Tracking")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? LotSerialTracking
		{
			get
			{
				return this._LotSerialTracking;
			}
			set
			{
				this._LotSerialTracking = value;
			}
		}
		#endregion
        #region BlanketPO
        public abstract class blanketPO : PX.Data.BQL.BqlBool.Field<blanketPO> { }
        protected bool? _BlanketPO;
        [Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<PO.POOrder, Where<PO.POOrder.orderType, Equal<PO.POOrderType.blanket>, Or<PO.POOrder.orderType, Equal<PO.POOrderType.standardBlanket>>>>), DisplayName = "Blanket and Standard Purchase Orders")]
        public virtual bool? BlanketPO
		{
			get
			{
                return this._BlanketPO;
			}
			set
			{
                this._BlanketPO = value;
			}
		}
		#endregion		
        #region DropShipments
        public abstract class dropShipments : PX.Data.BQL.BqlBool.Field<dropShipments> { }
        protected bool? _DropShipments;
        [Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<PO.POOrder, Where<PO.POOrder.orderType, Equal<PO.POOrderType.dropShip>>>), DisplayName = "Drop Shipments")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? DropShipments
		{
			get
			{
                return this._DropShipments;
			}
			set
			{
                this._DropShipments = value;
			}
		}
		#endregion

        #region Warehouse
        public abstract class warehouse : PX.Data.BQL.BqlBool.Field<warehouse> { }
        protected bool? _Warehouse;
		[FeatureRestrictor(typeof(Select<INItemSite, Where<INItemSite.replenishmentSourceSiteID, IsNotNull>>))]
		[FeatureRestrictor(typeof(Select<INItemRep, Where<INItemRep.replenishmentSourceSiteID, IsNotNull>>))]
		[FeatureRestrictor(typeof(Select<INItemClassRep, Where<INItemClassRep.replenishmentSourceSiteID, IsNotNull>>))]
		[Feature(
			false,
			typeof(FeaturesSet.distributionModule),
			typeof(Select<INSite, Where<INSite.siteCD, NotEqual<INSite.main>, And<Where<SiteAttribute.transitSiteID, IsNull, Or<INSite.siteID, NotEqual<SiteAttribute.transitSiteID>>>>>>),
			DisplayName = "Multiple Warehouses")]
		[FeatureDependency(typeof(inventory))]
		public virtual bool? Warehouse
		{
			get
			{
                return this._Warehouse;
			}
			set
			{
                this._Warehouse = value;
			}
		}
		#endregion
        #region WarehouseLocation
        public abstract class warehouseLocation : PX.Data.BQL.BqlBool.Field<warehouseLocation> { }
        protected bool? _WarehouseLocation;
        [Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<IN.INLocation, Where<IN.INLocation.locationCD, NotEqual<IN.INLocation.main>>>), DisplayName = "Multiple Warehouse Locations")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? WarehouseLocation
		{
			get
			{
                return this._WarehouseLocation;
			}
			set
			{
                this._WarehouseLocation = value;
			}
		}
		#endregion
        #region Replenishment
        public abstract class replenishment : PX.Data.BQL.BqlBool.Field<replenishment> { }
        protected bool? _Replenishment;
        [Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Inventory Replenishment")]
        [FeatureDependency(typeof(FeaturesSet.warehouse), typeof(FeaturesSet.warehouseLocation))]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? Replenishment
		{
			get
			{
                return this._Replenishment;
			}
			set
			{
                this._Replenishment = value;
			}
		}
		#endregion
		#region MatrixItem
		public abstract class matrixItem : Data.BQL.BqlBool.Field<matrixItem> { }
		[Feature(typeof(distributionModule), typeof(Select<InventoryItem, Where<InventoryItem.isTemplate, Equal<True>>>), DisplayName = "Matrix Items")]
		public virtual bool? MatrixItem
		{
			get;
			set;
		}
		#endregion
        #region SubItem
        public abstract class subItem : PX.Data.BQL.BqlBool.Field<subItem> { }
        protected bool? _SubItem;
        [Feature(typeof(FeaturesSet.distributionModule), typeof(Select<IN.INSubItem, Where<IN.INSubItem.subItemCD, NotEqual<IN.INSubItem.Zero>>>), DisplayName = "Inventory Subitems")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? SubItem
		{
			get
			{
                return this._SubItem;
			}
			set
			{
                this._SubItem = value;
			}
		}
		#endregion		
        #region AutoPackaging
        public abstract class autoPackaging : PX.Data.BQL.BqlBool.Field<autoPackaging> { }
        protected bool? _AutoPackaging;
        [Feature(typeof(FeaturesSet.distributionModule), DisplayName = "Automatic Packaging")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? AutoPackaging
		{
			get
			{
                return this._AutoPackaging;
			}
			set
			{
                this._AutoPackaging = value;
			}
		}
		#endregion
        #region KitAssemblies
        public abstract class kitAssemblies : PX.Data.BQL.BqlBool.Field<kitAssemblies> { }
        protected bool? _KitAssemblies;
        [Feature(false, typeof(FeaturesSet.distributionModule), typeof(Select<IN.InventoryItem, Where<IN.InventoryItem.kitItem, Equal<True>, And<IN.InventoryItem.itemStatus, NotEqual<IN.InventoryItemStatus.inactive>, And<IN.InventoryItem.itemStatus, NotEqual<IN.InventoryItemStatus.markedForDeletion>>>>>), DisplayName = "Kit Assembly")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? KitAssemblies
		{
			get
			{
                return this._KitAssemblies;
			}
			set
			{
                this._KitAssemblies = value;
			}
		}
		#endregion
        #region AdvancedPhysicalCounts
        public abstract class advancedPhysicalCounts : PX.Data.BQL.BqlBool.Field<advancedPhysicalCounts> { }
        protected bool? _AdvancedPhysicalCounts;
        [Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Advanced Physical Count")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? AdvancedPhysicalCounts
        {
            get
            {
                return this._AdvancedPhysicalCounts;
            }
            set
            {
                this._AdvancedPhysicalCounts = value;
            }
        }
        #endregion		
		#region SOToPOLink
		public abstract class sOToPOLink : PX.Data.BQL.BqlBool.Field<sOToPOLink> { }
		protected bool? _SOToPOLink;
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Sales Order to Purchase Order Link")]
		[FeatureDependency(typeof(FeaturesSet.inventory))]
		public virtual bool? SOToPOLink
		{
			get
			{
				return this._SOToPOLink;
			}
			set
			{
				this._SOToPOLink = value;
			}
		}
		#endregion
        #region UserDefinedOrderTypes
        public abstract class userDefinedOrderTypes : PX.Data.BQL.BqlBool.Field<userDefinedOrderTypes> { }
        protected bool? _UserDefinedOrderTypes;
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Custom Order Types")]
        public virtual bool? UserDefinedOrderTypes
        {
            get
            {
                return this._UserDefinedOrderTypes;
            }
            set
            {
                this._UserDefinedOrderTypes = value;
            }
        }
		#endregion
		#region PurchaseRequisitions
		public abstract class purchaseRequisitions : PX.Data.BQL.BqlBool.Field<purchaseRequisitions> { }
		protected bool? _PurchaseRequisitions;
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Purchase Requisitions")]
		public virtual bool? PurchaseRequisitions
		{
			get
			{
				return this._PurchaseRequisitions;
			}
			set
			{
				this._PurchaseRequisitions = value;
			}
		}
		#endregion
		#region AdvancedSOInvoices
		public abstract class advancedSOInvoices : PX.Data.BQL.BqlBool.Field<advancedSOInvoices> { }

		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Advanced SO Invoices")]
		public virtual bool? AdvancedSOInvoices { get; set; }
		#endregion
		#region CrossReferenceUniqueness
		public abstract class crossReferenceUniqueness : PX.Data.BQL.BqlBool.Field<crossReferenceUniqueness> { }
		protected bool? _CrossReferenceUniqueness;
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Cross-Reference Uniqueness", Visible = false)]
		public virtual bool? CrossReferenceUniqueness
		{
			get
			{
				return this._CrossReferenceUniqueness;
			}
			set
			{
				this._CrossReferenceUniqueness = value;
			}
		}
		#endregion

		#region VendorRelations
		public abstract class vendorRelations : PX.Data.BQL.BqlBool.Field<vendorRelations> { }
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Vendor Relations")]
		public virtual bool? VendorRelations { get; set; }
		#endregion

		#region AdvancedFulfillment
		public abstract class advancedFulfillment : PX.Data.BQL.BqlBool.Field<advancedFulfillment> { }
		[Feature(false, typeof(FeaturesSet.distributionModule), DisplayName = "Warehouse Management")]
		public virtual bool? AdvancedFulfillment { get; set; }
		#endregion
		#region WMSFulfillment
		public abstract class wMSFulfillment : PX.Data.BQL.BqlBool.Field<wMSFulfillment> { }
		[Feature(false, typeof(FeaturesSet.advancedFulfillment), DisplayName = "Fulfillment")]
		public virtual bool? WMSFulfillment { get; set; }
		#endregion
		#region WMSAdvancedPicking
		public abstract class wMSAdvancedPicking : PX.Data.BQL.BqlBool.Field<wMSAdvancedPicking> { }
		[Feature(false, typeof(FeaturesSet.wMSFulfillment), DisplayName = "Advanced Picking")]
		public virtual bool? WMSAdvancedPicking { get; set; }
		#endregion
		#region WMSReceiving
		public abstract class wMSReceiving : PX.Data.BQL.BqlBool.Field<wMSReceiving> { }
		[Feature(false, typeof(FeaturesSet.advancedFulfillment), DisplayName = "Receiving")]
		public virtual bool? WMSReceiving { get; set; }
		#endregion
		#region WMSInventory
		public abstract class wMSInventory : PX.Data.BQL.BqlBool.Field<wMSInventory> { }
		[Feature(false, typeof(FeaturesSet.advancedFulfillment), DisplayName = "Inventory Operations")]
		public virtual bool? WMSInventory { get; set; }
		#endregion
		#region WMSCartTracking
		public abstract class wMSCartTracking : PX.Data.BQL.BqlBool.Field<wMSCartTracking> { }
		[Feature(false, typeof(FeaturesSet.advancedFulfillment), typeof(Select<INCartSplit>), DisplayName = "Cart Tracking")]
		public virtual bool? WMSCartTracking { get; set; }
		#endregion

		#region OrganizationModule
		public abstract class organizationModule : PX.Data.BQL.BqlBool.Field<organizationModule> { }
        protected bool? _OrganizationModule;
        [Feature(true, DisplayName = "Organization", Enabled = false, Visible=false)]
        public virtual bool? OrganizationModule
        {
            get
            {
                return this._OrganizationModule;
            }
            set
            {
                this._OrganizationModule = value;
            }
        }
        #endregion

        #region CustomerModule
        public abstract class customerModule : PX.Data.BQL.BqlBool.Field<customerModule>
		{
            public const string FieldClass = "CRM";
            }
        protected bool? _CustomerModule;
        [Feature(false, null, typeof(Select<CR.CRSetup>), DisplayName = "Customer Management")]
        public virtual bool? CustomerModule
		{
			get
			{
                return this._CustomerModule;
			}
			set
			{
                this._CustomerModule = value;
			}
		}
        #endregion

        #region CaseManagement
        public abstract class caseManagement : PX.Data.BQL.BqlBool.Field<caseManagement> { }
	    protected bool? _CaseManagement;
	    [Feature(typeof(FeaturesSet.customerModule), DisplayName = "Case Management")]
	    public virtual bool? CaseManagement
        {
	        get
	        {
	            return this._CaseManagement;
	        }
	        set
	        {
	            this._CaseManagement = value;
	        }
	    }
	    #endregion


        #region ContactDuplicate
        public abstract class contactDuplicate : PX.Data.BQL.BqlBool.Field<contactDuplicate>
		{
            public const string FieldClass = "DUPLICATE";
		}
        protected bool? _ContactDuplicate;
        [Feature(typeof(FeaturesSet.customerModule), DisplayName = "Lead/Contact Duplicate Validation")]
        public virtual bool? ContactDuplicate
		{
			get
			{
                return this._ContactDuplicate;
			}
			set
			{
                this._ContactDuplicate = value;
			}
		}
        #endregion

        #region SalesQuotes
        public abstract class salesQuotes : PX.Data.BQL.BqlBool.Field<salesQuotes> { }
        protected bool? _SalesQuotes;
        [Feature(typeof(FeaturesSet.customerModule), DisplayName = "Sales Quotes")]
        public virtual bool? SalesQuotes
        {
            get
            {
                return this._SalesQuotes;
            }
            set
            {
                this._SalesQuotes = value;
            }
        }
		#endregion

		#region Outlook Integration
		public abstract class outlookIntegration : PX.Data.BQL.BqlBool.Field<outlookIntegration> { }
		[Feature(typeof(FeaturesSet.customerModule), DisplayName = "Outlook Integration")]
		public virtual bool? OutlookIntegration { get; set; }
		#endregion

		#region Project Accounting
		public abstract class projectAccounting : PX.Data.BQL.BqlBool.Field<projectAccounting> { }
		[Feature(false, null, DisplayName = "Projects")]
		public virtual bool? ProjectAccounting
		{
			get;
			set;
		}
		#endregion

		#region ProjectModule
		public abstract class projectModule : PX.Data.BQL.BqlBool.Field<projectModule> { }
		[Feature(false, typeof(FeaturesSet.projectAccounting), typeof(Select<PM.PMProject, Where<PM.PMProject.baseType, Equal<CT.CTPRType.project>, And<PM.PMProject.nonProject, Equal<False>>>>), DisplayName = "Project Accounting", SyncToParent = true)]
        public virtual bool? ProjectModule
		{
			get;set;
		}
		#endregion
		 
		#region ChangeOrder
		public abstract class changeOrder : PX.Data.BQL.BqlBool.Field<changeOrder> { }
		
		[Feature(typeof(FeaturesSet.projectAccounting), typeof(Select<PM.PMChangeOrder>), DisplayName = "Change Orders")]
		public virtual bool? ChangeOrder
		{
			get;set;
		}
		#endregion

		#region ChangeRequest
		public abstract class changeRequest : PX.Data.BQL.BqlBool.Field<changeRequest> { }

		[Feature(typeof(FeaturesSet.changeOrder), typeof(Select<PM.PMChangeRequest>), DisplayName = "Change Requests")]
		public virtual bool? ChangeRequest
		{
			get; set;
		}
		#endregion

		#region BudgetForecast
		public abstract class budgetForecast : PX.Data.BQL.BqlBool.Field<budgetForecast>
		{
		}

		[Feature(typeof(FeaturesSet.projectAccounting), typeof(Select<PM.PMForecast>), DisplayName = "Budget Forecast")]
		public virtual bool? BudgetForecast
		{
			get; set;
		}
		#endregion
		#region CostCodes
		public abstract class costCodes : PX.Data.BQL.BqlBool.Field<costCodes> { }
		
		[Feature(typeof(FeaturesSet.projectAccounting),
			typeof(Select2<PM.PMBudget,
				InnerJoin<PM.PMCostCode, On<PM.PMBudget.costCodeID, Equal<PM.PMCostCode.costCodeID>>>,
				Where<PM.PMCostCode.isDefault, Equal<False>>>), DisplayName = "Cost Codes")]
		public virtual bool? CostCodes
		{
			get;set;
		}
		#endregion
		#region ProjectQuotes
		public abstract class projectQuotes : PX.Data.BQL.BqlBool.Field<projectQuotes> { }
		[Feature(typeof(FeaturesSet.projectAccounting), DisplayName = "Project Quotes")]
		public virtual bool? ProjectQuotes { get; set; }
		#endregion
		#region ProjectMultiCurrency
		public abstract class projectMultiCurrency : PX.Data.BQL.BqlBool.Field<projectMultiCurrency>
		{
		}
		[FeatureRestrictor(typeof(Select2<PM.PMProject, InnerJoin<GL.Company, On<Not<GL.Company.baseCuryID, Equal<PM.PMProject.curyID>>>>>))]
		[FeatureRestrictor(typeof(Select2<PM.PMTran, InnerJoin<GL.Company, On<Not<GL.Company.baseCuryID, Equal<PM.PMTran.tranCuryID>>>>>))]
		[Feature(typeof(FeaturesSet.projectAccounting), DisplayName = "Multi-Currency Projects")]
		[FeatureDependency(typeof(FeaturesSet.multicurrency))]
		public virtual bool? ProjectMultiCurrency
		{
			get; set;
		}
		#endregion
		#region Construction
		public abstract class construction : PX.Data.BQL.BqlBool.Field<construction> { }
		[Feature(typeof(FeaturesSet.projectAccounting), DisplayName = "Construction")]
		public virtual bool? Construction
		{
			get;set;
		}
		#endregion
		#region ConstructionProjectManagement
		public abstract class constructionProjectManagement : PX.Data.BQL.BqlBool.Field<constructionProjectManagement> { }
		[Feature(typeof(FeaturesSet.construction), DisplayName = "Construction Project Management")]
		public virtual bool? ConstructionProjectManagement
		{
			get; set;
		}
		#endregion

		#region PortalModules
		#region PortalModule
		public abstract class portalModule : PX.Data.BQL.BqlBool.Field<portalModule> { }
        protected bool? _PortalModule;
        [Feature(false, DisplayName = "Customer Portal")]
        public virtual bool? PortalModule
        {
            get
            {
                return this._PortalModule;
            }
            set
            {
                this._PortalModule = value;
            }
        }
        #endregion

        #region B2BOrdering
        public abstract class b2BOrdering : PX.Data.BQL.BqlBool.Field<b2BOrdering> { }
        protected bool? _B2BOrdering;
        [Feature(false, typeof(portalModule), DisplayName = "B2B Ordering")]
        [FeatureDependency(typeof(FeaturesSet.distributionModule))]
        public virtual bool? B2BOrdering
        {
            get
            {
                return this._B2BOrdering;
            }
            set
            {
                this._B2BOrdering = value;
            }
        }
        #endregion

        #region PortalCaseManagement
        public abstract class portalCaseManagement : PX.Data.BQL.BqlBool.Field<portalCaseManagement> { }
        protected bool? _PortalCaseManagement;
        [Feature(false, typeof(portalModule), DisplayName = "Case Management on Portal")]
        [FeatureDependency(typeof(FeaturesSet.caseManagement))]
        public virtual bool? PortalCaseManagement
        {
            get
            {
                return this._PortalCaseManagement;
            }
            set
            {
                this._PortalCaseManagement = value;
            }
        }
        #endregion

        #region Financials
        public abstract class portalFinancials : PX.Data.BQL.BqlBool.Field<portalFinancials> { }
        protected bool? _PortalFinancials;
        [Feature(false, typeof(portalModule), DisplayName = "Financials on Portal")]
        [FeatureDependency(typeof(FeaturesSet.financialAdvanced))]
        public virtual bool? PortalFinancials
        {
            get
            {
                return this._PortalFinancials;
            }
            set
            {
                this._PortalFinancials = value;
            }
        }
        #endregion
        #endregion
        
		#region ServiceModules
        #region ServiceManagementModule
        public abstract class serviceManagementModule : PX.Data.BQL.BqlBool.Field<serviceManagementModule> { }
        // We need to add the usage check. In order to do that it's necessary to move the Field-Service's DACs into PX.Objects
        [Feature(false, null, null, DisplayName = "Service Management")]
        public virtual bool? ServiceManagementModule { get; set; }
        #endregion
        #region ServiceManagementStaffMembersPack
        public abstract class serviceManagementStaffMembersPack : PX.Data.BQL.BqlBool.Field<serviceManagementStaffMembersPack> { }
        protected bool? _ServiceManagementStaffMembersPack;
        [Feature(false, typeof(FeaturesSet.serviceManagementModule), null, DisplayName = "Staff Member Pack", SyncToParent = true)]
        public virtual bool? ServiceManagementStaffMembersPack
        {
            get
            {
                return _ServiceManagementStaffMembersPack;
            }
            set
            {
                SetValueToPackGroup(ref _ServiceManagementStaffMembersPack, value,
                                ref _ServiceManagementStaffMembersPack10,
                                ref _ServiceManagementStaffMembersPack50,
                                ref _ServiceManagementStaffMembersPackUnlimited);
            }
        }
        #endregion
        #region ServiceManagementStaffMembersPack10
        public abstract class serviceManagementStaffMembersPack10 : PX.Data.BQL.BqlBool.Field<serviceManagementStaffMembersPack10> { }
        protected bool? _ServiceManagementStaffMembersPack10;
        [Feature(false, typeof(FeaturesSet.serviceManagementStaffMembersPack), DisplayName = "10 Staff Members")]
        public virtual bool? ServiceManagementStaffMembersPack10
        {
            get
            {
                return _ServiceManagementStaffMembersPack10;
            }
            set
            {
                SetValueToPackOption(ref _ServiceManagementStaffMembersPack10, value, _ServiceManagementStaffMembersPack,
                                ref _ServiceManagementStaffMembersPack50,
                                ref _ServiceManagementStaffMembersPackUnlimited);
            }
        }
        #endregion
        #region ServiceManagementStaffMembersPack50
        public abstract class serviceManagementStaffMembersPack50 : PX.Data.BQL.BqlBool.Field<serviceManagementStaffMembersPack50> { }
        protected bool? _ServiceManagementStaffMembersPack50;
        [Feature(false, typeof(FeaturesSet.serviceManagementStaffMembersPack), DisplayName = "50 Staff Members")]
        public virtual bool? ServiceManagementStaffMembersPack50
        {
            get
            {
                return _ServiceManagementStaffMembersPack50;
            }
            set
            {
                SetValueToPackOption(ref _ServiceManagementStaffMembersPack50, value, _ServiceManagementStaffMembersPack,
                                ref _ServiceManagementStaffMembersPack10,
                                ref _ServiceManagementStaffMembersPackUnlimited);
            }
        }
        #endregion
        #region ServiceManagementStaffMembersPackUnlimited
        public abstract class serviceManagementStaffMembersPackUnlimited : PX.Data.BQL.BqlBool.Field<serviceManagementStaffMembersPackUnlimited> { }
        protected bool? _ServiceManagementStaffMembersPackUnlimited;
        [Feature(false, typeof(FeaturesSet.serviceManagementStaffMembersPack), DisplayName = "Unlimited Staff Members")]
        public virtual bool? ServiceManagementStaffMembersPackUnlimited
        {
            get
            {
                return _ServiceManagementStaffMembersPackUnlimited;
            }
            set
            {
                SetValueToPackOption(ref _ServiceManagementStaffMembersPackUnlimited, value, _ServiceManagementStaffMembersPack,
                                ref _ServiceManagementStaffMembersPack10,
                                ref _ServiceManagementStaffMembersPack50);
            }
        }
        #endregion

        #region EquipmentManagementModule
        public abstract class equipmentManagementModule : PX.Data.BQL.BqlBool.Field<equipmentManagementModule> { }
        // We need to add the usage check. In order to do that it's necessary to move the Field-Service's DACs into PX.Objects
        [Feature(false, typeof(FeaturesSet.serviceManagementModule), null, DisplayName = "Equipment Management")]
        [FeatureDependency(typeof(FeaturesSet.serviceManagementModule))]
        public virtual bool? EquipmentManagementModule { get; set; }
        #endregion

        #region RouteManagementModule
        public abstract class routeManagementModule : PX.Data.BQL.BqlBool.Field<routeManagementModule> { }
        // We need to add the usage check. In order to do that it's necessary to move the Field-Service's DACs into PX.Objects
        [Feature(false, typeof(FeaturesSet.serviceManagementModule), null, DisplayName = "Route Management")]
        [FeatureDependency(typeof(FeaturesSet.serviceManagementModule))]
        public virtual bool? RouteManagementModule { get; set; }
        #endregion
        #region RouteManagementVehiclesPack
        public abstract class routeManagementVehiclesPack : PX.Data.BQL.BqlBool.Field<routeManagementVehiclesPack> { }
        protected bool? _RouteManagementVehiclesPack;
        [Feature(false, typeof(FeaturesSet.routeManagementModule), null, DisplayName = "Vehicle Pack", SyncToParent = true)]
        public virtual bool? RouteManagementVehiclesPack
        {
            get
            {
                return _RouteManagementVehiclesPack;
            }
            set
            {
                SetValueToPackGroup(ref _RouteManagementVehiclesPack, value,
                                ref _RouteManagementVehiclesPack10,
                                ref _RouteManagementVehiclesPack50,
                                ref _RouteManagementVehiclesPackUnlimited);
            }
        }
        #endregion
        #region RouteManagementVehiclesPack10
        public abstract class routeManagementVehiclesPack10 : PX.Data.BQL.BqlBool.Field<routeManagementVehiclesPack10> { }
        protected bool? _RouteManagementVehiclesPack10;
        [Feature(false, typeof(FeaturesSet.routeManagementVehiclesPack), DisplayName = "10 Vehicles")]
        public virtual bool? RouteManagementVehiclesPack10
        {
            get
            {
                return _RouteManagementVehiclesPack10;
            }
            set
            {
                SetValueToPackOption(ref _RouteManagementVehiclesPack10, value, _RouteManagementVehiclesPack,
                                ref _RouteManagementVehiclesPack50,
                                ref _RouteManagementVehiclesPackUnlimited);
            }
        }
        #endregion
        #region RouteManagementVehiclesPack50
        public abstract class routeManagementVehiclesPack50 : PX.Data.BQL.BqlBool.Field<routeManagementVehiclesPack50> { }
        protected bool? _RouteManagementVehiclesPack50;
        [Feature(false, typeof(FeaturesSet.routeManagementVehiclesPack), DisplayName = "50 Vehicles")]
        public virtual bool? RouteManagementVehiclesPack50
        {
            get
            {
                return _RouteManagementVehiclesPack50;
            }
            set
            {
                SetValueToPackOption(ref _RouteManagementVehiclesPack50, value, _RouteManagementVehiclesPack,
                                ref _RouteManagementVehiclesPack10,
                                ref _RouteManagementVehiclesPackUnlimited);
            }
        }
        #endregion
        #region RouteManagementVehiclesPackUnlimited
        public abstract class routeManagementVehiclesPackUnlimited : PX.Data.BQL.BqlBool.Field<routeManagementVehiclesPackUnlimited> { }
        protected bool? _RouteManagementVehiclesPackUnlimited;
        [Feature(false, typeof(FeaturesSet.routeManagementVehiclesPack), DisplayName = "Unlimited Vehicles")]
        public virtual bool? RouteManagementVehiclesPackUnlimited
        {
            get
            {
                return _RouteManagementVehiclesPackUnlimited;
            }
            set
            {
                SetValueToPackOption(ref _RouteManagementVehiclesPackUnlimited, value, _RouteManagementVehiclesPack,
                                ref _RouteManagementVehiclesPack10,
                                ref _RouteManagementVehiclesPack50);
            }
        }
        #endregion
        #endregion

		#region CommerceIntegration
		public abstract class commerceIntegration : PX.Data.BQL.BqlBool.Field<commerceIntegration> { }

		[Feature(false, DisplayName = "Commerce Integration")]
		public virtual bool? CommerceIntegration
		{
			get;
			set;
		}
		#endregion

        #region PayrollModules
        #region PayrollModule

        public abstract class payrollModule : PX.Data.BQL.BqlBool.Field<payrollModule> { }
		[FeatureRestrictor(typeof(Select<PR.Standalone.PREarningType>))]
		[Feature(false, null, null, DisplayName = "Payroll")]
		public virtual bool? PayrollModule { get; set; }

        #endregion
        #endregion PayrollModules

        #region IntegrationModule
        public abstract class integrationModule : PX.Data.BQL.BqlBool.Field<integrationModule> { }
        protected bool? _IntegrationModule;
        [Feature(true, DisplayName = "Third Party Integrations")]
        public virtual bool? IntegrationModule
		{
			get
			{
                return this._IntegrationModule;
			}
			set
			{
                this._IntegrationModule = value;
			}
		}
		#endregion
        #region CarrierIntegration
        public abstract class carrierIntegration : PX.Data.BQL.BqlBool.Field<carrierIntegration> { }
        protected bool? _CarrierIntegration;
        [Feature(false, typeof(FeaturesSet.integrationModule), DisplayName = "Shipping Carrier Integration")]
        [FeatureDependency(true, typeof(FeaturesSet.distributionModule), typeof(FeaturesSet.inventory))]
        public virtual bool? CarrierIntegration
		{
			get
			{
                return this._CarrierIntegration;
			}
			set
			{
                this._CarrierIntegration = value;
			}
		}
		#endregion
        #region ExchangeIntegration
        public abstract class exchangeIntegration : PX.Data.BQL.BqlBool.Field<exchangeIntegration>
        {
            public const string FieldClass = "ExchangeIntegration";
        }
        protected bool? _ExchangeIntegration;
        [Feature(typeof(FeaturesSet.integrationModule), DisplayName = "Exchange Integration")]
        public virtual bool? ExchangeIntegration
		{
			get
			{
                return this._ExchangeIntegration;
			}
			set
			{
                this._ExchangeIntegration = value;
			}
		}
		#endregion
		#region External Tax Provider
		public abstract class avalaraTax : PX.Data.BQL.BqlBool.Field<avalaraTax> { }
        protected bool? _AvalaraTax;
        [Feature(typeof(FeaturesSet.integrationModule), DisplayName = "External Tax Calculation Integration")]
        public virtual bool? AvalaraTax
		{
			get
			{
                return this._AvalaraTax;
			}
			set
			{
                this._AvalaraTax = value;
			}
		}
		#endregion
        #region AddressValidation
        public abstract class addressValidation : PX.Data.BQL.BqlBool.Field<addressValidation> { }
        protected bool? _AddressValidation;
        [Feature(typeof(FeaturesSet.integrationModule), DisplayName = "Address Validation Integration")]
        public virtual bool? AddressValidation
		{
			get
			{
                return this._AddressValidation;
			}
			set
			{
                this._AddressValidation = value;
			}
		}
		#endregion

        #region SalesforceIntegration
        public abstract class salesforceIntegration : PX.Data.BQL.BqlBool.Field<salesforceIntegration> { }
        protected bool? _SalesforceIntegration;
        [Feature(false, typeof(FeaturesSet.integrationModule), DisplayName = "Salesforce Integration")]
        [FeatureDependency(typeof(FeaturesSet.customerModule))]
        public virtual bool? SalesforceIntegration
        {
            get
            {
                return this._SalesforceIntegration;
            }
            set
            {
                this._SalesforceIntegration = value;
            }
        }
		#endregion

		#region HubSpotIntegration
		public abstract class hubSpotIntegration : PX.Data.BQL.BqlBool.Field<hubSpotIntegration> { }
		protected bool? _HubSpotIntegration;
		[Feature(false, typeof(FeaturesSet.integrationModule), DisplayName = "HubSpot Integration")]
		[FeatureDependency(typeof(FeaturesSet.customerModule))]
		public virtual bool? HubSpotIntegration
		{
			get
			{
				return this._HubSpotIntegration;
			}
			set
			{
				this._HubSpotIntegration = value;
			}
		}
		#endregion

		#region ProcoreIntegration
		public abstract class procoreIntegration : PX.Data.BQL.BqlBool.Field<procoreIntegration> { }
		[Feature(false, typeof(FeaturesSet.integrationModule), DisplayName = "Procore Integration")]
		public virtual bool? ProcoreIntegration
		{
			get;
			set;
		}
		#endregion

		#region Protected Methods
		protected virtual void SetValueToPackGroup(ref bool? packGroup, bool? value, ref bool? option1, ref bool? option2, ref bool? option3)
        {
            packGroup = value;

            if (value == false)
            {
                option1 = false;
                option2 = false;
                option3 = false;
            }
            else
            {
                if (option1 == false && option2 == false && option3 == false)
                {
                    option1 = true;
                }
            }
        }

        protected virtual void SetValueToPackOption(ref bool? packOption, bool? value, bool? packGroup, ref bool? otherOption1, ref bool? otherOption2)
        {
            if (value == true)
            {
                otherOption1 = false;
                otherOption2 = false;

                packOption = true;
            }
            else
            {
                if (packGroup == false || otherOption1 == true || otherOption2 == true)
                {
                    packOption = false;
                }
            }
        }
        #endregion


        #region Manufacturing
        public abstract class manufacturing : PX.Data.BQL.BqlBool.Field<manufacturing> { }

	    [Feature(false, DisplayName = "Manufacturing Suite")]
	    public virtual bool? Manufacturing { get; set; }
        #endregion

        #region ManufacturingMRP
        public abstract class manufacturingMRP : PX.Data.BQL.BqlBool.Field<manufacturingMRP> { }

	    [Feature(typeof(FeaturesSet.manufacturing), DisplayName = "Material Requirements Planning")]
	    public virtual bool? ManufacturingMRP { get; set; }
        #endregion

        #region ManufacturingProductConfigurator
        public abstract class manufacturingProductConfigurator : PX.Data.BQL.BqlBool.Field<manufacturingProductConfigurator> { }

	    [Feature(typeof(FeaturesSet.manufacturing), DisplayName = "Product Configurator")]
	    public virtual bool? ManufacturingProductConfigurator { get; set; }
        #endregion

        #region ManufacturingEstimating
        public abstract class manufacturingEstimating : PX.Data.BQL.BqlBool.Field<manufacturingEstimating> { }

	    [Feature(typeof(FeaturesSet.manufacturing), DisplayName = "Estimating")]
	    public virtual bool? ManufacturingEstimating { get; set; }
        #endregion

        #region ManufacturingAdvancedPlanning
        public abstract class manufacturingAdvancedPlanning : PX.Data.BQL.BqlBool.Field<manufacturingAdvancedPlanning> { }

	    [Feature(typeof(FeaturesSet.manufacturing), DisplayName = "Advanced Planning and Scheduling")]
	    public virtual bool? ManufacturingAdvancedPlanning { get; set; }
	    #endregion


        #region ImageRecognition
        public abstract class imageRecognition : PX.Data.BQL.BqlBool.Field<imageRecognition> { }
        protected bool? _ImageRecognition;
        [Feature(typeof(FeaturesSet.integrationModule), DisplayName = "Image Recognition Service")]
        public virtual bool? ImageRecognition
        {
            get
            {
                return this._ImageRecognition;
            }
            set
            {
                this._ImageRecognition = value;
            }
        }
        #endregion

        #region RouteOptimizer
        public abstract class routeOptimizer : PX.Data.BQL.BqlBool.Field<routeOptimizer> { }
        protected bool? _RouteOptimizer;
        [Feature(typeof(FeaturesSet.integrationModule), DisplayName = "Workwave Route Optimization")]
        public virtual bool? RouteOptimizer
        {
            get
            {
                return this._RouteOptimizer;
            }
            set
            {
                this._RouteOptimizer = value;
            }
        }
        #endregion

        #region AdvancedAuthentication
        public abstract class advancedAuthentication : Data.BQL.BqlBool.Field<advancedAuthentication> { }
		[Feature(true, DisplayName = "Advanced Authentication", Top = true)]
		public virtual bool? AdvancedAuthentication { get; set; }
		#endregion

		#region TwoFactorAuthentication
		public abstract class twoFactorAuthentication : Data.BQL.BqlBool.Field<twoFactorAuthentication> { }

		// can be used by string constant "PX.Objects.CS.FeaturesSet+TwoFactorAuthentication" in PX.Data
		[Feature(true, typeof(advancedAuthentication), DisplayName = "Two-Factor Authentication")]
		public virtual bool? TwoFactorAuthentication { get; set; }
		#endregion

		#region GoogleAndMicrosoftSSO
		public abstract class googleAndMicrosoftSSO : Data.BQL.BqlBool.Field<googleAndMicrosoftSSO> { }

		[Feature(true, typeof(advancedAuthentication), DisplayName = "Google and Microsoft SSO")]
		public virtual bool? GoogleAndMicrosoftSSO { get; set; }
		#endregion

		#region ActiveDirectoryAndOtherExternalSSO
		public abstract class activeDirectoryAndOtherExternalSSO : Data.BQL.BqlBool.Field<activeDirectoryAndOtherExternalSSO> { }

		[Feature(true, typeof(advancedAuthentication), DisplayName = "Active Directory and Other External SSO")]
		public virtual bool? ActiveDirectoryAndOtherExternalSSO { get; set; }
		#endregion
	}
}
