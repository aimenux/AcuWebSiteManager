using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA.BankStatementHelpers;

namespace PX.Objects.CA
{
	[PXCacheName(Messages.CashAccount)]
	[Serializable]
	[PXPrimaryGraph(
		new Type[] { typeof(CashAccountMaint) },
		new Type[] { typeof(Select<CashAccount,
			Where<CashAccount.cashAccountID, Equal<Current<CashAccount.cashAccountID>>>>)
		})]
	public partial class CashAccount : IBqlTable, IMatchSettings
	{
	    #region Keys
	    public class PK : PrimaryKeyOf<CashAccount>.By<cashAccountID>
	    {
	        public static CashAccount Find(PXGraph graph, int? cashAccountID) => FindBy(graph, cashAccountID);
	    }
	    #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
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
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual bool? Active
		{
			get;
			set;
		}
		#endregion

		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		[PXDBIdentity]
		[PXUIField(Enabled = false)]
		[PXReferentialIntegrityCheck]
		public virtual int? CashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CashAccountCD
		public abstract class cashAccountCD : PX.Data.BQL.BqlString.Field<cashAccountCD> { }

		[CashAccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		public virtual string CashAccountCD
		{
			get;
			set;
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[Account(Required = true, Visibility = PXUIVisibility.SelectorVisible, AvoidControlAccounts = true)]
		[PXRestrictor(typeof(Where<Account.controlAccountModule, IsNull>), Messages.OnlyNonControlAccountCanBeCashAccount)]
		public virtual int? AccountID
		{
			get;
			set;
		}
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[SubAccount(typeof(CashAccount.accountID), DisplayName = "Subaccount", DescriptionField = typeof(Sub.description),
			Required = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual int? SubID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string Descr
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		[PXSelector(typeof(CM.Currency.curyID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }

		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CM.CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Curr. Rate Type")]
		public virtual string CuryRateTypeID
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "External Ref. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region Reconcile
		public abstract class reconcile : PX.Data.BQL.BqlBool.Field<reconcile> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Requires Reconciliation")]
		public virtual bool? Reconcile
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[Vendor(DescriptionField = typeof(Vendor.acctName), DisplayName = "Bank ID")]
		[PXUIField(DisplayName = "Bank ID")]
		public virtual int? ReferenceID
		{
			get;
			set;
		}
		#endregion
		#region ReconNumberingID
		public abstract class reconNumberingID : PX.Data.BQL.BqlString.Field<reconNumberingID> { }

		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID),
					 DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Reconciliation Numbering Sequence", Required = false)]
		public virtual string ReconNumberingID
		{
			get;
			set;
		}
		#endregion
		#region ClearingAccount
		public abstract class clearingAccount : PX.Data.BQL.BqlBool.Field<clearingAccount> { }
		
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Clearing Account")]
		public virtual bool? ClearingAccount
		{
			get;
			set;
		}
		#endregion
		#region Signature
		public abstract class signature : PX.Data.BQL.BqlString.Field<signature> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Signature")]
		public virtual string Signature
		{
			get;
			set;
		}
		#endregion
		#region SignatureDescr
		public abstract class signatureDescr : PX.Data.BQL.BqlString.Field<signatureDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		public virtual string SignatureDescr
		{
			get;
			set;
		}
		#endregion
		#region StatementImportTypeName
		public abstract class statementImportTypeName : PX.Data.BQL.BqlString.Field<statementImportTypeName> { }

		[PXDBString(255)]
		[PXUIField(DisplayName = "Statement Import Service")]
		[PXProviderTypeSelector(typeof(IStatementReader))]
		public virtual string StatementImportTypeName
		{
			get;
			set;
		}
		#endregion
		#region RestrictVisibilityWithBranch
		public abstract class restrictVisibilityWithBranch : PX.Data.BQL.BqlBool.Field<restrictVisibilityWithBranch> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Restrict Visibility with Branch")]
		public virtual bool? RestrictVisibilityWithBranch
		{
			get;
			set;
		}
		#endregion
		#region PTInstanceAllowed
		public abstract class pTInstancesAllowed : PX.Data.BQL.BqlBool.Field<pTInstancesAllowed> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cards Allowed", Visible = false, Enabled = false)]
		public virtual bool? PTInstancesAllowed
		{
			get;
			set;
		}
		#endregion
		#region AcctSettingsAllowed
		public abstract class acctSettingsAllowed : PX.Data.BQL.BqlBool.Field<acctSettingsAllowed> { }

		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Account Settings Allowed", Visible = false, Enabled = false)]
		public virtual bool? AcctSettingsAllowed
		{
			get;
			set;
		}
		#endregion
		#region MatchToBatch
		public abstract class matchToBatch : PX.Data.BQL.BqlBool.Field<matchToBatch> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Match Bank Transactions to Batch Payments")]
		public virtual bool? MatchToBatch
		{
			get;
			set;
		}
		#endregion
		#region UseForCorpCard
		public abstract class useForCorpCard : Data.BQL.BqlBool.Field<useForCorpCard> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use for Corporate Cards")]
		public virtual bool? UseForCorpCard { get; set; }
        #endregion

        #region ReceiptTranDaysBefore
        /// <summary>
        /// Gets sets ReceiptTranDaysBefore
        /// </summary>
        public abstract class receiptTranDaysBefore : PX.Data.BQL.BqlInt.Field<receiptTranDaysBefore> { }
		/// <summary>
		/// Gets sets ReceiptTranDaysBefore
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.receiptTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysBefore { get; set; }
        #endregion
        #region ReceiptTranDaysAfter
        /// <summary>
        /// Gets sets ReceiptTranDaysAfter
        /// </summary>
        public abstract class receiptTranDaysAfter : PX.Data.BQL.BqlInt.Field<receiptTranDaysAfter> { }
		/// <summary>
		/// Gets sets ReceiptTranDaysAfter
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.receiptTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? ReceiptTranDaysAfter { get; set; }
        #endregion
        #region DisbursementTranDaysBefore
        /// <summary>
        /// Gets sets DisbursementTranDaysBefore
        /// </summary>
        public abstract class disbursementTranDaysBefore : PX.Data.BQL.BqlInt.Field<disbursementTranDaysBefore> { }
		/// <summary>
		/// Gets sets DisbursementTranDaysBefore
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(5, typeof(CASetup.disbursementTranDaysBefore), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days Before Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysBefore { get; set; }
        #endregion
        #region DisbursementTranDaysAfter
        /// <summary>
        /// Gets sets DisbursementTranDaysAfter
        /// </summary>
        public abstract class disbursementTranDaysAfter : PX.Data.BQL.BqlInt.Field<disbursementTranDaysAfter> { }
		/// <summary>
		/// Gets sets DisbursementTranDaysAfter
		/// </summary>
		[PXDBInt(MinValue = 0, MaxValue = 365)]
		[PXDefault(2, typeof(CASetup.disbursementTranDaysAfter), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Days After Bank Transaction Date")]
		public virtual Int32? DisbursementTranDaysAfter { get; set; }

        #endregion
        #region AllowMatchingCreditMemo
        /// <summary>
        /// Gets sets AllowMatchingCreditMemo
        /// </summary>
        public abstract class allowMatchingCreditMemo : PX.Data.BQL.BqlBool.Field<allowMatchingCreditMemo> { }
		/// <summary>
		/// Gets sets AllowMatchingCreditMemo
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.allowMatchingCreditMemo), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Allow Matching to Credit Memo")]
		public virtual bool? AllowMatchingCreditMemo
		{
			get;
			set;
		}
        #endregion

        #region RefNbrCompareWeight
        /// <summary>
        /// Gets sets RefNbrCompareWeight
        /// </summary>
        public abstract class refNbrCompareWeight : PX.Data.BQL.BqlDecimal.Field<refNbrCompareWeight> { }
		/// <summary>
		/// Gets sets RefNbrCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "70.0", typeof(CASetup.refNbrCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Ref. Nbr. Weight")]
		public virtual Decimal? RefNbrCompareWeight { get; set; }

        #endregion
        #region DateCompareWeight
        /// <summary>
        /// Gets sets DateCompareWeight
        /// </summary>
        public abstract class dateCompareWeight : PX.Data.BQL.BqlDecimal.Field<dateCompareWeight> { }
		/// <summary>
		/// Gets sets DateCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "20.0",
			typeof(CASetup.dateCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Date Weight")]
		public virtual Decimal? DateCompareWeight { get; set; }

        #endregion
        #region PayeeCompareWeight
        /// <summary>
        /// Gets sets PayeeCompareWeight
        /// </summary>
        public abstract class payeeCompareWeight : PX.Data.BQL.BqlDecimal.Field<payeeCompareWeight> { }
		/// <summary>
		/// Gets sets PayeeCompareWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.payeeCompareWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Doc. Payee Weight")]
		public virtual Decimal? PayeeCompareWeight { get; set; }

		#endregion
		protected Decimal TotalWeight
		{
			get
			{
				decimal total = (this.DateCompareWeight ?? Decimal.Zero)
								+ (this.RefNbrCompareWeight ?? Decimal.Zero)
								+ (this.PayeeCompareWeight ?? Decimal.Zero);
				return total;
			}

		}
        #region RefNbrComparePercent
        /// <summary>
        /// Gets sets RefNbrComparePercent
        /// </summary>
        public abstract class refNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<refNbrComparePercent> { }
		/// <summary>
		/// Gets sets RefNbrComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? RefNbrComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region EmptyRefNbrMatching
        /// <summary>
        /// Gets sets EmptyRefNbrMatching
        /// </summary>
        public abstract class emptyRefNbrMatching : PX.Data.BQL.BqlBool.Field<emptyRefNbrMatching> { }
		/// <summary>
		/// Gets sets EmptyRefNbrMatching
		/// </summary>
		[PXDBBool]
		[PXDefault(false, typeof(CASetup.emptyRefNbrMatching), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Consider Empty Ref. Nbr. as Matching", Visibility = PXUIVisibility.Visible)]
		public virtual bool? EmptyRefNbrMatching
		{
			get;
			set;
		}
        #endregion EmptyRefNbrMatching
        #region DateComparePercent
        /// <summary>
        /// Gets sets DateComparePercent
        /// </summary>
        public abstract class dateComparePercent : PX.Data.BQL.BqlDecimal.Field<dateComparePercent> { }
		/// <summary>
		/// Gets sets DateComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? DateComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region PayeeComparePercent
        /// <summary>
        /// Gets sets PayeeComparePercent
        /// </summary>
        public abstract class payeeComparePercent : PX.Data.BQL.BqlDecimal.Field<payeeComparePercent> { }
		/// <summary>
		/// Gets sets PayeeComparePercent
		/// </summary>
		[PXDecimal()]
		[PXUIField(DisplayName = "%", Enabled = false)]
		public virtual Decimal? PayeeComparePercent
		{
			get
			{
				Decimal total = this.TotalWeight;
				return ((total != Decimal.Zero ? (this.PayeeCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region DateMeanOffset
        /// <summary>
        /// Gets sets DateMeanOffset
        /// </summary>
        public abstract class dateMeanOffset : PX.Data.BQL.BqlDecimal.Field<dateMeanOffset> { }
		/// <summary>
		/// Gets sets DateMeanOffset
		/// </summary>
		[PXDBDecimal(MinValue = -365, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.dateMeanOffset),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Clearing Average Delay")]
		public virtual Decimal? DateMeanOffset { get; set; }

        #endregion
        #region DateSigma
        /// <summary>
        /// Gets sets DateSigma
        /// </summary>
        public abstract class dateSigma : PX.Data.BQL.BqlDecimal.Field<dateSigma> { }
		/// <summary>
		/// Gets sets DateSigma
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 365)]
		[PXDefault(TypeCode.Decimal, "5.0",
			typeof(CASetup.dateSigma),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Estimated Deviation (Days)")]
		public virtual Decimal? DateSigma { get; set; }
        #endregion

        #region SkipVoided
        /// <summary>
        /// Gets sets SkipVoided
        /// </summary>
        public abstract class skipVoided : PX.Data.BQL.BqlBool.Field<skipVoided> { }
		protected Boolean? _SkipVoided;
		/// <summary>
		/// Gets sets SkipVoided
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(CASetup.skipVoided),
				PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Skip Voided Transactions During Matching")]
		public virtual Boolean? SkipVoided
		{
			get
			{
				return this._SkipVoided;
			}
			set
			{
				this._SkipVoided = value;
			}
		}
        #endregion
        #region CuryDiffThreshold
        /// <summary>
        /// Gets sets CuryDiffThreshold
        /// </summary>
        public abstract class curyDiffThreshold : PX.Data.BQL.BqlDecimal.Field<curyDiffThreshold> { }
		/// <summary>
		/// Gets sets CuryDiffThreshold
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "5.0",
			typeof(CASetup.curyDiffThreshold),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount Difference Threshold (%)")]
		public virtual Decimal? CuryDiffThreshold { get; set; }
        #endregion
        #region AmountWeight
        /// <summary>
        /// Gets sets AmountWeight
        /// </summary>
        public abstract class amountWeight : PX.Data.BQL.BqlDecimal.Field<amountWeight> { }
		/// <summary>
		/// Gets sets AmountWeight
		/// </summary>
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXDefault(TypeCode.Decimal, "10.0",
			typeof(CASetup.amountWeight),
			PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount Weight")]
		public virtual Decimal? AmountWeight { get; set; }

		#endregion
		protected Decimal ExpenseReceiptTotalWeight
		{
			get
			{
				decimal total = (this.DateCompareWeight ?? Decimal.Zero)
								+ (this.RefNbrCompareWeight ?? Decimal.Zero)
								+ (this.AmountWeight ?? Decimal.Zero);
				return total;
			}

		}
        #region ExpenseReceiptRefNbrComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptRefNbrComparePercent
        /// </summary>
        public abstract class expenseReceiptRefNbrComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptRefNbrComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptRefNbrComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptRefNbrComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.RefNbrCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region ExpenseReceiptDateComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptDateComparePercent
        /// </summary>
        public abstract class expenseReceiptDateComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptDateComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptDateComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptDateComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.DateCompareWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region ExpenseReceiptAmountComparePercent
        /// <summary>
        /// Gets sets ExpenseReceiptAmountComparePercent
        /// </summary>
        public abstract class expenseReceiptAmountComparePercent : PX.Data.BQL.BqlDecimal.Field<expenseReceiptAmountComparePercent> { }
		/// <summary>
		/// Gets sets ExpenseReceiptAmountComparePercent
		/// </summary>
		[PXDecimal()]
		public virtual Decimal? ExpenseReceiptAmountComparePercent
		{
			get
			{
				Decimal total = this.ExpenseReceiptTotalWeight;
				return ((total != Decimal.Zero ? (this.AmountWeight / total) : Decimal.Zero) * 100.0m);
			}
			set
			{

			}
		}
        #endregion
        #region RatioInRelevanceCalculationLabel
        /// <summary>
        /// Gets sets RatioInRelevanceCalculationLabel
        /// </summary>
        public abstract class ratioInRelevanceCalculationLabel : PX.Data.BQL.BqlDecimal.Field<ratioInRelevanceCalculationLabel> { }
		/// <summary>
		/// Gets sets RatioInRelevanceCalculationLabel
		/// </summary>
		[PXString]
		[PXUIField]
		public virtual string RatioInRelevanceCalculationLabel
		{
			get
			{
				return PXMessages.LocalizeFormatNoPrefix(CA.Messages.RatioInRelevanceCalculation,
					ExpenseReceiptRefNbrComparePercent,
					ExpenseReceiptDateComparePercent,
					ExpenseReceiptAmountComparePercent);
			}
			set
			{

			}
		}
        #endregion
        #region MatchSettingsPerAccount
        /// <summary>
        /// Gets sets MatchSettingsPerAccount
        /// </summary>
        public abstract class matchSettingsPerAccount : PX.Data.BQL.BqlBool.Field<matchSettingsPerAccount> { }
		/// <summary>
		/// Gets sets MatchSettingsPerAccount
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? MatchSettingsPerAccount { get; set; }

		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(CashAccount.cashAccountCD))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}
}
