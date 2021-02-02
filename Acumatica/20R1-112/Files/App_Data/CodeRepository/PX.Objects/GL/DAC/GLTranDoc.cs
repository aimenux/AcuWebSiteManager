using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.PM;
using PX.Objects.TX;

namespace PX.Objects.GL
{
	/// <summary>
	/// Represents a line of a <see cref="GLDocBatch">GL document batch</see> and contains all information
	/// required to create the corresponding document or transaction.
	/// Records of this type appear in the details grid of the Journal Vouchers (GL.30.40.00) page.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.TransactionDoc)]
	public partial class GLTranDoc : IBqlTable, IInvoice
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		/// <summary>
		/// Identifier of the <see cref="Branch"/>, to which the parent <see cref="GLTranDoc">document batch</see> belongs.
		/// </summary>
		/// <value>
		/// The value of this field is obtained from the <see cref="GLDocBatch.BranchID"/> field of the parent batch.
		/// Corresponds to the <see cref="Branch.BranchID"/> field.
		/// </value>
		[Branch(typeof(GLDocBatch.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }

		/// <summary>
		/// Key field.
		/// The code of the module, to which the parent <see cref="GLDocBatch">batch</see> belongs.
		/// </summary>
		/// <value>
		/// Defaults to the value of the <see cref="GLDocBatch.Module"/> field of the parent batch.
		/// This field is not supposed to be changed directly.
		/// Possible values are:
		/// "GL", "AP", "AR", "CM", "CA", "IN", "DR", "FA", "PM", "TX", "SO", "PO".
		/// </value>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDBDefault(typeof(GLDocBatch))]
		[PXUIField(DisplayName = "Batch Module", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

		/// <summary>
		/// Key field.
		/// Auto-generated unique number of the parent <see cref="GLDocBatch">document batch</see>.
		/// </summary>
		/// <value>
		/// The number is obtained from the <see cref="GLDocBatch.BatchNbr"/> field of the parent batch.
		/// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(GLDocBatch))]
		[PXParent(typeof(Select<GLDocBatch, Where<GLDocBatch.module, Equal<Current<GLTranDoc.module>>, And<GLDocBatch.batchNbr, Equal<Current<GLTranDoc.batchNbr>>>>>))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string BatchNbr
		{
			get;
			set;
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

		/// <summary>
		/// Key field.
		/// The number of the document/transaction line inside the <see cref="GLDocBatch">document batch</see>.
		/// </summary>
		/// <value>
		/// Note that the sequence of line numbers of the transactions belonging to a single document may include gaps.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXLineNbr(typeof(GLDocBatch.lineCntr))]
		public virtual int? LineNbr
		{
			get;
			set;
		}
		#endregion
		#region ImportRefNbr
		public abstract class importRefNbr : PX.Data.BQL.BqlString.Field<importRefNbr> { }
		[PXString(15, IsUnicode = true)]
		public virtual string ImportRefNbr
		{
			get;
			set;
		}
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }

		/// <summary>
		/// Identifier of the <see cref="Ledger"/>, to which the parent <see cref="GLDocBatch">document batch</see> belongs.
		/// </summary>
		/// <value>
		/// The value of this field is obtained from the <see cref="GLDocBatch.LedgerID"/> field of the parent batch.
		/// Corresponds to the <see cref="Ledger.LedgerID"/> field.
		/// </value>
		[PXDBInt]
		[PXDBDefault(typeof(GLDocBatch))]
		public virtual int? LedgerID
		{
			get;
			set;
		}
		#endregion
		#region ParentLineNbr
		public abstract class parentLineNbr : PX.Data.BQL.BqlInt.Field<parentLineNbr> { }

		/// <summary>
		/// The <see cref="LineNbr">line number</see> of the line, which represents the parent document for this line of the batch.
		/// This field is used to define the details (lines) of a document included in the <see cref="GLDocBatch"/>.
		/// For more information see <see cref="Split"/> field and the documentation for the Journal Vouchers (GL.30.40.00) page.
		/// </summary>
		[PXDBInt]
		[PXParent(typeof(Select<GLTranDoc, Where<GLTranDoc.lineNbr, Equal<Current<GLTranDoc.parentLineNbr>>,
											And<GLTranDoc.module, Equal<Current<GLTranDoc.module>>,
											And<GLTranDoc.batchNbr, Equal<Current<GLTranDoc.batchNbr>>>>>>))]
		[PXUIField(DisplayName = "Parent Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
		public virtual int? ParentLineNbr
		{
			get;
			set;
		}
		#endregion

		#region Split
		public abstract class split : PX.Data.BQL.BqlBool.Field<split> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the document or transaction created from this line should include
		/// the details defined by other <see cref="GLTranDoc">lines</see> of the batch.
		/// See also the <see cref="ParentLineNbr"/> field.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Split")]
		public bool? Split
		{
			get;
			set;
		}
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		[CostCode(ReleasedField = typeof(released))]
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
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		/// <summary>
		/// Identifier of the <see cref="Currency"/> of the document or transaction that the system creates from the line of the batch.
		/// </summary>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visible = true, Enabled = false)]
		[PXDefault(typeof(GLDocBatch.curyID))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region TranCode
		public abstract class tranCode : PX.Data.BQL.BqlString.Field<tranCode> { }

		/// <summary>
		/// Entry code that defines the <see cref="GLTranCode.Module">Module</see>
		/// and the <see cref="GLTranCode.TranType">Type</see> of the document or transaction that the system creates from this line of the batch.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLTranCode.TranCode"/> field.
		/// </value>
		[PXString(5, IsUnicode = true, InputMask = ">aaaaa")]
		[PXDBScalar(typeof(Search<GLTranCode.tranCode, Where<GLTranCode.module, Equal<GLTranDoc.tranModule>,
							And<GLTranCode.tranType, Equal<GLTranDoc.tranType>>>>))]
		[PXSelector(typeof(Search<GLTranCode.tranCode, Where<GLTranCode.active, Equal<True>>>),
				typeof(GLTranCode.tranCode),
				typeof(GLTranCode.module),
				typeof(GLTranCode.tranType),
				typeof(GLTranCode.descr))]
		[PXUIField(DisplayName = "Tran Code", Required = true)]
		public virtual string TranCode
		{
			get;
			set;
		}
		#endregion
		#region TranModule
		public abstract class tranModule : PX.Data.BQL.BqlString.Field<tranModule> { }

		/// <summary>
		/// The module of the transaction or document that the system creates from this line of the batch.
		/// </summary>
		/// <value>
		/// The value of this field is defined by the <see cref="GLTranCode">Entry Code</see> specified in the <see cref="TranCode"/> field.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(typeof(Search<GLTranCode.module, Where<GLTranCode.tranCode, Equal<Current<GLTranDoc.tranCode>>>>))]
		[PXUIField(DisplayName = "Tran. Module", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string TranModule
		{
			get;
			set;
		}
		#endregion
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

		/// <summary>
		/// The type of the transaction or document that the system creates from this line of the batch.
		/// </summary>
		/// <value>
		/// The value of this field is defined by the <see cref="GLTranCode">Entry Code</see> specified in the <see cref="TranCode"/> field.
		/// </value>
		[PXDBString(3, IsFixed = true)]
		[PXDefault(typeof(Search<GLTranCode.tranType, Where<GLTranCode.tranCode, Equal<Current<GLTranDoc.tranCode>>>>))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = true)]
		public virtual string TranType
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

		/// <summary>
		/// The date of the transaction or document that the system creates from this line of the batch.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="GLDocBatch.DateEntered">date of the batch</see>.
		/// </value>
		[PXDBDate]
		[PXDefault(typeof(GLDocBatch.dateEntered))]
		[PXUIField(DisplayName = "Transaction Date")]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.AP.Vendor">Vendor</see> or <see cref="PX.Objects.AR.Customer">Customer</see>,
		/// for whom the system will create a document from this line.
		/// </summary>
		/// <value>
		/// The field corresponds to the <see cref="BAccount.BAccountID"/> field and is relevant only for the documents of AP and AR modules.
		/// </value>
		[PXDBInt]
		[PXVendorCustomerSelector(typeof(GLTranDoc.tranModule), typeof(GLTranDoc.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = true, Visible = true)]
		public virtual int? BAccountID
		{
			get;
			set;
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		/// <summary>
		/// Identifier of the <see cref="Location">Location</see> of the <see cref="BAccountID">Vendor or
		/// Customer</see> associated with this line of the batch.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field.
		/// Defaults to vendor's or customer's <see cref="BAccountR.DefLocationID">default location</see>.
		/// </value>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<GLTranDoc.bAccountID>>>), DisplayName = "Location", DescriptionField = typeof(Location.descr))]
		[PXDefault(typeof(Search<BAccountR.defLocationID, Where<BAccountR.bAccountID, Equal<Current<GLTranDoc.bAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? LocationID
		{
			get;
			set;
		}
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        /// <summary>
        /// Identifier of the <see cref="PMProject">Project</see> associated with the document or transaction that the system creates from the line,
        /// or the <see cref="PMSetup.NonProjectCode">non-project code</see> indicating that the document or transaction is not associated with any particular project.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PMProject.ProjectID"/> field.
        /// </value>
        [ProjectDefault(null)]
        [ActiveProjectOrContractForGL()]
        public virtual int? ProjectID
        {
            get;
            set;
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        /// <summary>
        /// Identifier of the <see cref="PMTask">Task</see> associated with the document or transaction that the system creates from the line.
        /// The field is relevant only if the Projects module has been activated and integrated with the module of the document or transaction.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PMTask.TaskID"/> field.
        /// </value>
        [ActiveProjectTask(typeof(GLTranDoc.projectID), typeof(GLTranDoc.tranModule), NeedTaskValidationField = typeof(needTaskValidation), DisplayName = "Project Task")]
        public virtual int? TaskID
        {
            get;
            set;
        }
        #endregion


        #region EntryTypeID
        public abstract class entryTypeID : PX.Data.BQL.BqlString.Field<entryTypeID> { }

		/// <summary>
		/// Identifier of the <see cref="CAEntryType">Entry Type</see> of the CA transaction
		/// that the system creates from this line of the batch.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CAEntryType.EntryTypeID"/> field.
		/// This field is relevant only for the lines defining the transactions of the CA module.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CAEntryType.entryTypeId,
							  Where<CAEntryType.module, Equal<Current<GLTranDoc.tranModule>>,
							  And<CAEntryType.useToReclassifyPayments, NotEqual<True>>>>),
					  DescriptionField = typeof(CAEntryType.descr))]
		[PXUIField(DisplayName = "Entry Type ID")]
		public virtual string EntryTypeID
		{
			get;
			set;
		}
		#endregion
		#region CADrCr
		public abstract class cADrCr : PX.Data.BQL.BqlString.Field<cADrCr> { }

		/// <summary>
		/// Indicates whether the CA transaction that the system will create from this line is Receipt or Disbursement.
		/// </summary>
		/// <value>
		/// The value of this field is determined by the <see cref="EntryType"/> of the line.
		/// Possible values are: <c>"D"</c> for Receipt and <c>"C"</c> for Disbursement.
		/// </value>
		[PXString(1, IsFixed = true)]
		[PXDefault(typeof(Search<CAEntryType.drCr, Where<Current<GLTranDoc.tranModule>, Equal<GL.BatchModule.moduleCA>,
							And<CAEntryType.entryTypeId, Equal<Current<GLTranDoc.entryTypeID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBScalar(typeof(Search<CAEntryType.drCr, Where<CAEntryType.entryTypeId, Equal<GLTranDoc.entryTypeID>,
								And<GLTranDoc.tranModule, Equal<GL.BatchModule.moduleCA>>>>))]
		public string CADrCr
		{
			get;
			set;
		}
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		/// <summary>
		/// Identifier of the <see cref="PaymentMethod">Payment Method</see> used for the document created from the line.
		/// </summary>
		/// <value>
		/// This field is relevant only for the lines defining documents of AP and AR modules.
		/// Corresponds to the <see cref="PaymentMethod.PaymentMethodID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.isActive, Equal<True>,
							And<Where2<Where<Current<GLTranDoc.tranModule>, Equal<GL.BatchModule.moduleAP>,
									And<PaymentMethod.useForAP, Equal<True>,
									And<Where2<Where<PaymentMethod.aPPrintChecks, Equal<False>,
									And<PaymentMethod.aPCreateBatchPayment, Equal<False>>>,
									Or<Current<GLTranDoc.tranType>, Equal<AP.APDocType.invoice>,
									Or<Current<GLTranDoc.tranType>, Equal<AP.APDocType.debitAdj>>>>>>>,
								Or<Where<Current<GLTranDoc.tranModule>, Equal<GL.BatchModule.moduleAR>,
									And<PaymentMethod.useForAR, Equal<True>,
									And<Where<PaymentMethod.aRIsProcessingRequired, Equal<False>,
									Or<Current<GLTranDoc.tranType>, Equal<AR.ARDocType.invoice>,
									Or<Current<GLTranDoc.tranType>, Equal<AR.ARDocType.debitMemo>>>>>>>>>>>>),
									DescriptionField = typeof(PaymentMethod.descr))]
		[PXUIField(DisplayName = "Payment Method", Visible = true)]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		/// <summary>
		/// Identifier of the <see cref="TaxCategory">Tax Category</see> associated with the document or transaction defined by the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[GLTax(typeof(GLTranDoc), typeof(GLTax), typeof(GLTaxTran))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string TaxCategoryID
		{
			get;
			set;
		}
		#endregion

		#region DebitAccountID
		public abstract class debitAccountID : PX.Data.BQL.BqlInt.Field<debitAccountID> { }

		/// <summary>
		/// Identifier of the debit <see cref="Account"/> associated with the line of the batch.
		/// </summary>
		/// <value>
		/// For the lines defining GL transactions the value of this field is entered by user.
		/// For the lines used to create AP or AR documents the account is selected automatically once the <see cref="BAccountID">vendor or customer</see> is chosen.
		/// For CA transactions the account is either entered by user (for Receipts) or selected automatically 
		/// based on the <see cref="EntryTypeID">Entry Type</see> (for Disbursements).
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(typeof(GLTranDoc.branchID),
			typeof(Search2<Account.accountID,
										LeftJoin<CashAccount, On<CashAccount.accountID, Equal<Account.accountID>,
											And<CashAccount.branchID, Equal<Current<GLTranDoc.branchID>>,
											And<CashAccount.curyID, Equal<Optional<GLTranDoc.curyID>>>>>,
										LeftJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<Optional<GLTranDoc.entryTypeID>>>,
										LeftJoin<CashAccountETDetail, On<CashAccountETDetail.accountID, Equal<CashAccount.cashAccountID>,
											And<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>>>>,
										Where<Where2<Where<Optional<GLTranDoc.tranModule>, Equal<GL.BatchModule.moduleCA>,
												And<CAEntryType.entryTypeId, IsNotNull,
												And<Where<CAEntryType.drCr, Equal<CADrCr.cACredit>,
													Or<CashAccountETDetail.accountID, IsNotNull>>>>>,
												Or<Where<Optional<GLTranDoc.tranModule>, NotEqual<GL.BatchModule.moduleCA>,
													And<Where<Optional<GLTranDoc.needsDebitCashAccount>, Equal<False>,
													Or<CashAccount.accountID, IsNotNull>>>>>>>>),
											LedgerID = typeof(GLTranDoc.ledgerID),
											DescriptionField = typeof(Account.description),
											DisplayName = "Debit Account")]

		[PXDefault]
		public virtual int? DebitAccountID
		{
			get;
			set;
		}
		#endregion
		#region DebitSubID
		public abstract class debitSubID : PX.Data.BQL.BqlInt.Field<debitSubID> { }

		/// <summary>
		/// Identifier of the debit <see cref="Sub">Subaccount</see> associated with the line of the batch.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// For the information related to defaulting of this field see <see cref="DebitAccountID"/>.
		/// </value>
		[SubAccount(typeof(GLTranDoc.debitAccountID), typeof(GLTranDoc.branchID), DisplayName = "Debit Subaccount")]
		[PXDefault]
		public virtual int? DebitSubID
		{
			get;
			set;
		}
		#endregion
		#region CreditAccountID
		public abstract class creditAccountID : PX.Data.BQL.BqlInt.Field<creditAccountID> { }

		/// <summary>
		/// Identifier of the credit <see cref="Account"/> associated with the line of the batch.
		/// </summary>
		/// <value>
		/// For the lines defining GL transactions the value of this fields is entered by user.
		/// For the lines used to create AP or AR documents the account is selected automatically once the <see cref="BAccountID">vendor or customer</see> is chosen.
		/// For CA transactions the account is either entered by user (for Disbursements) or selected automatically
		/// based on the <see cref="EntryTypeID">Entry Type</see> (for Receipts).
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(typeof(GLTranDoc.branchID),
				typeof(Search2<Account.accountID,
										LeftJoin<CashAccount, On<CashAccount.accountID, Equal<Account.accountID>,
											And<CashAccount.branchID, Equal<Current<GLTranDoc.branchID>>,
											And<CashAccount.curyID, Equal<Optional<GLTranDoc.curyID>>>>>,
										LeftJoin<CAEntryType, On<CAEntryType.entryTypeId, Equal<Optional<GLTranDoc.entryTypeID>>>,
										LeftJoin<CashAccountETDetail, On<CashAccountETDetail.accountID, Equal<CashAccount.cashAccountID>,
											And<CashAccountETDetail.entryTypeID, Equal<CAEntryType.entryTypeId>>>>>>,
										Where<Where2<Where<Optional<GLTranDoc.tranModule>, Equal<GL.BatchModule.moduleCA>,
												And<CAEntryType.entryTypeId, IsNotNull,
												And<Where<CAEntryType.drCr, Equal<CADrCr.cADebit>,
													Or<CashAccountETDetail.accountID, IsNotNull>>>>>,
												Or<Where<Optional<GLTranDoc.tranModule>, NotEqual<GL.BatchModule.moduleCA>,
													And<Where<Optional<GLTranDoc.needsCreditCashAccount>, Equal<False>,
													Or<CashAccount.accountID, IsNotNull>>>>>>>>),
											LedgerID = typeof(GLTranDoc.ledgerID),
											DescriptionField = typeof(Account.description),
											DisplayName = "Credit Account")]
		[PXDefault]
		public virtual int? CreditAccountID
		{
			get;
			set;
		}
		#endregion
		#region CreditSubID
		public abstract class creditSubID : PX.Data.BQL.BqlInt.Field<creditSubID> { }

		/// <summary>
		/// Identifier of the credit <see cref="Sub">Subaccount</see> associated with the line of the batch.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// For the information related to defaulting of this field see <see cref="CreditAccountID"/>.
		/// </value>
		[SubAccount(typeof(GLTranDoc.creditAccountID), typeof(GLTranDoc.branchID), DisplayName = "Credit Subaccount")]
		[PXDefault]
		public virtual int? CreditSubID
		{
			get;
			set;
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// Reference number of the document or transaction created from the line.
		/// </summary>
		/// <value>
		/// For the lines defining GL transactions the field corresponds to the <see cref="Batch.BatchNbr"/> field.
		/// For the lines used to create documents in AP and AR modules the field corresponds to the 
		/// <see cref="PX.Objects.AP.APRegister.RefNbr"/> and <see cref="PX.Objects.AR.ARRegister.RefNbr"/> fields, respectively.
		/// For the lines defining CA transactions the field corresponds to the <see cref="CAAdj.AdjRefNbr"/> field.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Null)]
		[PXUIField(DisplayName = "Ref. Number", Visible = true)]
		public virtual string RefNbr
		{
			get;
			set;
		}
		#endregion
		#region DocCreated
		public abstract class docCreated : PX.Data.BQL.BqlBool.Field<docCreated> { }

		/// <summary>
		/// When <c>true</c>, indicates that the document or transaction defined by the line has been created.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Doc. Created", Enabled = false)]
		public virtual bool? DocCreated
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

		/// <summary>
		/// The reference number of the original vendor or customer document.
		/// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Ext. Ref.Number", Visibility = PXUIVisibility.Visible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region TranAmt
		public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

		/// <summary>
		/// The sum of the document or transaction details or lines. Does not include tax amount.
		/// For the batch lines representing details of the document to be created (see <see cref="Split"/> field),
		/// the value of this field defines the line amount.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryTranAmt"/>.
		/// </summary>
		[PXDBBaseCury(typeof(GLTranDoc.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranAmt
		{
			get;
			set;
		}
		#endregion

		#region TranTotal
		public abstract class tranTotal : PX.Data.BQL.BqlDecimal.Field<tranTotal> { }

		/// <summary>
		/// The amount of the document or transaction, including the total tax amount.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryTotalAmt"/>.
		/// </summary>
		[PXDBBaseCury(typeof(GLTranDoc.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TranTotal
		{
			get;
			set;
		}
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> object associated with the line.
		/// </summary>
		/// <value>
		/// Aut-generated. Corresponds to the <see cref="PX.Objects.CM.CurrencyInfo.CurrencyInfoID"/> field.
		/// </value>
		[PXDBLong]
		[CurrencyInfo(typeof(GLDocBatch.curyInfoID))]
		public virtual long? CuryInfoID
		{
			get;
			set;
		}
		#endregion
		#region CuryTranTotal
		public abstract class curyTranTotal : PX.Data.BQL.BqlDecimal.Field<curyTranTotal> { }

		/// <summary>
		/// The amount of the document or transaction, including the total tax amount.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="TotalAmt"/>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Total Amount", Visibility = PXUIVisibility.Visible)]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.tranTotal))]
		public virtual decimal? CuryTranTotal
		{
			get;
			set;
		}
		#endregion
		#region CuryTranAmt
		public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

		/// <summary>
		/// The sum of the document or transaction details or lines. Does not include tax amount.
		/// For the batch lines representing details of the document to be created (see <see cref="Split"/> field),
		/// the value of this field defines the line amount.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="TranAmt"/>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Subtotal Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.tranAmt))]
		public virtual decimal? CuryTranAmt
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

		/// <summary>
		/// Indicates whether the document defined by the line has been released.
		/// The system releases the documents once they have been generated.
		/// </summary>
		/// <value>
		/// <c>true</c> if released, otherwise <c>false</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion

		#region TranClass
		public abstract class tranClass : PX.Data.BQL.BqlString.Field<tranClass> { }

		/// <summary>
		/// Reserved for internal use.
		/// The class of the document or transaction defined by the line.
		/// This field affects posting of documents and transactions to GL.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault("N")]
		public virtual string TranClass
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

		/// <summary>
		/// The description of the document or transaction defined by the line.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Transaction Description", Visibility = PXUIVisibility.Visible)]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion

		#region TranLineNbr
		public abstract class tranLineNbr : PX.Data.BQL.BqlInt.Field<tranLineNbr> { }

		/// <summary>
		/// Reserved for internal use.
		/// </summary>
		[PXDBInt]
		public virtual int? TranLineNbr
		{
			get;
			set;
		}
		#endregion

		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }

		/// <summary>
		/// Identifier of the <see cref="CustomerPaymentMethod">Customer Payment Method</see> (card or account number) associated with the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CustomerPaymentMethod.PMInstanceID"/> field.
		/// Relevant only for the lines representing AR documents.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID,
									Where<CustomerPaymentMethod.bAccountID, Equal<Current<GLTranDoc.bAccountID>>,
									  And<CustomerPaymentMethod.paymentMethodID, Equal<Current<GLTranDoc.paymentMethodID>>,
									  And<CustomerPaymentMethod.isActive, Equal<True>>>>>),
									  DescriptionField = typeof(CustomerPaymentMethod.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PMInstanceID
		{
			get;
			set;
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

		/// <summary>
		/// <see cref="OrganizationFinPeriod">Financial Period</see> of the document or transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLDocBatch.TranPeriodID">period of the batch</see>,
		/// which is defined by its <see cref="GLDocBatch.DateEntered">date</see> and can't be changed by user.
		/// </value>
		[PeriodID]
		[PXDBDefault(typeof(GLDocBatch.tranPeriodID))]
        public virtual string TranPeriodID
		{
			get;
			set;
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		/// <summary>
		/// <see cref="OrganizationFinPeriod">Financial Period</see> of the document or transaction.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="GLDocBatch.FinPeriodID">period of the batch</see>,
		/// which is defined by its <see cref="GLDocBatch.DateEntered">date</see>, but can be overriden by user.
		/// </value>
		[CAAPAROpenPeriod(typeof(tranModule), typeof(tranDate), typeof(branchID), 
			masterFinPeriodIDType: typeof(tranPeriodID),
            redefaultOnDateChanged: false,
			errorLevel = PXErrorLevel.RowError)]
        [PXDBDefault(typeof(GLDocBatch.finPeriodID))]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
		#region PMTranID
		public abstract class pMTranID : PX.Data.BQL.BqlLong.Field<pMTranID> { }

		/// <summary>
		/// Identifier of the <see cref="PMTran">Project Transactions</see> assoicated with the document or transactions, represented by the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PMTran.TranID"/> field.
		/// </value>
		[PXDBChildIdentity(typeof(PMTran.tranID))]
		[PXDBLong]
		public virtual long? PMTranID
		{
			get;
			set;
		}
		#endregion
		#region LedgerBalanceType
		public abstract class ledgerBalanceType : PX.Data.BQL.BqlString.Field<ledgerBalanceType> { }

		/// <summary>
		/// The type of balance of the <see cref="Ledger"/>, associated with the line.
		/// </summary>
		/// <value>
		/// Possible values are:
		/// <c>"A"</c> - Actual,
		/// <c>"R"</c> - Reporting,
		/// <c>"S"</c> - Statistical,
		/// <c>"B"</c> - Budget.
		/// Corresponds to the <see cref="Ledger.BalanceType"/> field.
		/// </value>
		[PXString(1, IsFixed = true, InputMask = "")]
		public virtual string LedgerBalanceType
		{
			get;
			set;
		}
		#endregion
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }

		/// <summary>
		/// Identifier of the <see cref="Terms">Credit Terms</see> record associated with the line.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Terms.TermsID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.all>, Or<Terms.visibleTo, Equal<TermsVisibleTo.customer>>>>), DescriptionField = typeof(Terms.descr), Filterable = true)]
		[Terms(typeof(GLTranDoc.tranDate), typeof(GLTranDoc.dueDate), typeof(GLTranDoc.discDate), typeof(GLTranDoc.curyTranTotal), typeof(GLTranDoc.curyDiscAmt))]
		public virtual string TermsID
		{
			get;
			set;
		}
		#endregion
		#region DueDate
		public abstract class dueDate : PX.Data.BQL.BqlDateTime.Field<dueDate> { }

		/// <summary>
		/// The due date of the document defined by the line, if applicable.
		/// </summary>
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Due Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DueDate
		{
			get;
			set;
		}
		#endregion
		#region DiscDate
		public abstract class discDate : PX.Data.BQL.BqlDateTime.Field<discDate> { }

		/// <summary>
		/// The date of the cash discount for the document, if applicable.
		/// </summary>
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cash Discount Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DiscDate
		{
			get;
			set;
		}
		#endregion
		#region CuryDiscAmt
		public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }

		/// <summary>
		/// The amount of the cash discount for the document (if applicable),
		/// given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="DiscAmt"/>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.discAmt))]
		[PXUIField(DisplayName = "Cash Discount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryDiscAmt
		{
			get;
			set;
		}
		#endregion
		#region DiscAmt
		public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }

		/// <summary>
		/// The amount of the cash discount for the document (if applicable),
		/// given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="DiscAmt"/>.
		/// </summary>
		[PXDBBaseCury]
		public virtual decimal? DiscAmt
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote]
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
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion

		#region CuryBalanceAmt


		/// <summary>
		/// The open balance of the parent document (represented by another line of the batch) for the moment when
		/// the child line is inserted.
		/// This field is used to determine the default value of the <see cref="CuryTranAmt"/> for the lines,
		/// representing details of a document to be created from another line of the batch.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// </summary>
		[PXDecimal(4)]
		[PXUIField(DisplayName = "Tran Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryBalanceAmt
		{
			get;
			set;
		}
		#endregion

		#region GroupTranID
		public abstract class groupTranID : PX.Data.BQL.BqlInt.Field<groupTranID> { }

		/// <summary>
		/// Read-only identifier of the group of batch lines.
		/// </summary>
		/// <value>
		/// Batch lines are grouped by document or transaction, so that for the lines describing different documents
		/// the values of this field are different, and for the lines defining header or lines of the same document
		/// (see the <see cref="Split"/> field) the GroupTranID value is the same.
		/// Depends only on the <see cref="LineNbr"/> and <see cref="ParentLineNbr"/> fields.
		/// </value>
		[PXInt]
		[PXUIField(FieldName = "Group Tran ID", Visible = false, Enabled = false)]
		public virtual int? GroupTranID
		{
			[PXDependsOnFields(typeof(parentLineNbr), typeof(lineNbr))]
			get
			{
				int? id = this.ParentLineNbr.HasValue ? this.ParentLineNbr.Value : this.LineNbr;
				return id * 10000;
			}

			set
			{
			}
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

		/// <summary>
		/// Read-only identifier of the <see cref="CashAccount">Cash Account</see> associated with the document or transaction defined by the line.
		/// </summary>
		/// <value>
		/// Depending on the <see cref="TranModule"/> and <see cref="TranType"/> returns either <see cref="DebitCashAccountID"/> or <see cref="CreditCashAccountID"/>.
		/// </value>
		[PXInt]
		public virtual int? CashAccountID
		{
			[PXDependsOnFields(typeof(tranModule), typeof(tranType), typeof(creditCashAccountID), typeof(debitCashAccountID), typeof(cADrCr))]
			get
			{
				int? acctID = null;
				if (this.TranModule == BatchModule.AP)
				{
					if (this.TranType == AP.APPaymentType.Check 
						|| this.TranType == AP.APPaymentType.Prepayment
						|| this.TranType == AP.APPaymentType.QuickCheck)
					{
						acctID = this.CreditCashAccountID;
					}
					if (this.TranType == AP.APPaymentType.Refund 
						|| this.TranType == AP.APPaymentType.VoidCheck
						|| this.TranType == AP.APPaymentType.VoidQuickCheck)
					{
						acctID = this.DebitCashAccountID;
					}
				}

				if (this.TranModule == BatchModule.AR)
				{
					if (this.TranType == AR.ARPaymentType.Payment
						|| this.TranType == AR.ARPaymentType.Prepayment
						|| this.TranType == AR.ARPaymentType.CashSale)
					{
						acctID = this.DebitCashAccountID;
					}
					if (this.TranType == AR.ARPaymentType.Refund
						|| this.TranType == AR.ARPaymentType.VoidPayment
						|| this.TranType == AR.ARPaymentType.CashReturn)
					{
						acctID = this.CreditCashAccountID;
					}
				}
				if (this.TranModule == BatchModule.CA && this.IsChildTran == false)
				{
					if (string.IsNullOrEmpty(this.CADrCr) == false)
					{
						acctID = (this.CADrCr == CA.CADrCr.CADebit) ? this.DebitCashAccountID : this.CreditCashAccountID;
					}
				}
				return acctID;
			}

			set
			{
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

		/// <summary>
		/// Identifier of the <see cref="TaxZone">Tax Zone</see> associated with the document, if applicable.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="TaxZone.TaxZoneID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
		public virtual string TaxZoneID
		{
			get;
			set;
		}
		#endregion

		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

		/// <summary>
		/// Identifier of the <see cref="Tax"/> associated with the document, if applicable.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Tax.TaxID"/> field.
		/// </value>
		[PXDBString(Tax.taxID.Length, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax ID", Visible = false)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
		public virtual string TaxID
		{
			get;
			set;
		}
		#endregion
		#region TaxRate
		public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }

		/// <summary>
		/// The tax rate for the document, if applicable.
		/// </summary>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? TaxRate
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxableAmt
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

		/// <summary>
		/// The amount that is subjected to the tax for the line.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="TaxableAmt"/>.
		/// </summary>
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.taxableAmt))]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? CuryTaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxableAmt
		public abstract class taxableAmt : PX.Data.BQL.BqlDecimal.Field<taxableAmt> { }

		/// <summary>
		/// The amount that is subjected to the tax for the line.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryTaxableAmt"/>.
		/// </summary>    
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Taxable Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? TaxableAmt
		{
			get;
			set;
		}
		#endregion
		#region CuryTaxAmt
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

		/// <summary>
		/// The resulting amount of the tax associated with the line.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="TaxAmt"/>.
		/// </summary>
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.taxAmt))]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? CuryTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region TaxAmt
		public abstract class taxAmt : PX.Data.BQL.BqlDecimal.Field<taxAmt> { }

		/// <summary>
		/// The resulting amount of the tax associated with the line.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryTaxAmt"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? TaxAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryInclTaxAmt
		public abstract class curyInclTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyInclTaxAmt> { }

		/// <summary>
		/// The amount of tax that is included in the <see cref="CuryTranTotal">document total</see>.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="InclTaxAmt"/>.
		/// </summary>
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.inclTaxAmt))]
		[PXUIField(DisplayName = "Included Tax Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? CuryInclTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region InclTaxAmt
		public abstract class inclTaxAmt : PX.Data.BQL.BqlDecimal.Field<inclTaxAmt> { }

		/// <summary>
		/// The amount of tax that is included in the <see cref="TranTotal">document total</see>.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryInclTaxAmt"/>.
		/// </summary>
		[PXDBDecimal(4)]
		[PXUIField(DisplayName = "Included Tax Amount", Visibility = PXUIVisibility.Visible)]
		public virtual decimal? InclTaxAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryOrigWhTaxAmt
		public abstract class curyOrigWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigWhTaxAmt> { }

		/// <summary>
		/// The tax amount withheld on the document.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="OrigWhTaxAmt"/>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.origWhTaxAmt))]
		[PXUIField(DisplayName = "With. Tax", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual decimal? CuryOrigWhTaxAmt
		{
			get;
			set;
		}
		#endregion
		#region OrigWhTaxAmt
		public abstract class origWhTaxAmt : PX.Data.BQL.BqlDecimal.Field<origWhTaxAmt> { }

		/// <summary>
		/// The tax amount withheld on the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="OrigWhTaxAmt"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? OrigWhTaxAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryDocAmt -Total of the document
		public abstract class curyDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDocTotal> { }

		/// <summary>
		/// Read-only document total.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// </summary>
		/// <value>
		/// For the lines representing document headers gives the total amount of the document, including taxes amount.
		/// For the lines representing details of a document returns <c>null</c>.
		/// Calculated from <see cref="CuryTaxAmt"/>, <see cref="CuryTranAmt"/> and <see cref="CuryInclTaxAmt"/> fields.
		/// </value>
		[PXCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.docTotal))]
		[PXUIField(DisplayName = "Doc Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? CuryDocTotal
		{
			[PXDependsOnFields(typeof(curyTaxAmt), typeof(curyTranAmt), typeof(curyInclTaxAmt))]
			get
			{
				if (this.IsChildTran)
				{
					return null;
				}
				return this.CuryTranAmt + (this.CuryTaxAmt ?? decimal.Zero) - (this.CuryInclTaxAmt ?? decimal.Zero);
			}

			set
			{
			}
		}

		public abstract class docTotal : PX.Data.BQL.BqlDecimal.Field<docTotal> { }

		/// <summary>
		/// Read-only document total.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		/// <value>
		/// For the lines representing document headers gives the total amount of the document, including taxes amount.
		/// For the lines representing details of a document returns <c>null</c>.
		/// Calculated from <see cref="TaxAmt"/>, <see cref="TranAmt"/> and <see cref="InclTaxAmt"/> fields.
		/// </value>
		public virtual decimal? DocTotal
		{
			[PXDependsOnFields(typeof(taxAmt), typeof(tranAmt), typeof(inclTaxAmt))]
			get
			{
				if (this.IsChildTran)
				{
					return null;
				}
				return this.TranAmt + this.TaxAmt - this.InclTaxAmt;
			}

			set
			{
			}
		}
		#endregion

		#region CuryTaxTotal -Total Tax Total of the document
		public abstract class curyTaxTotal : PX.Data.BQL.BqlDecimal.Field<curyTaxTotal> { }

		/// <summary>
		/// Read-only total amount of tax associated with the document or transaction.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// </summary>
		/// <value>
		/// Calculated from the <see cref="CuryTaxAmt"/> and <see cref="CuryInclTaxAmt"/> fields.
		/// </value>
		[PXCury(typeof(GLTranDoc.curyID))]
		[PXUIField(DisplayName = "Tax Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual decimal? CuryTaxTotal
		{
			[PXDependsOnFields(typeof(curyTaxAmt), typeof(curyInclTaxAmt))]
			get
			{
				return this.CuryTaxAmt - this.CuryInclTaxAmt;
			}

			set
			{
			}
		}

		/// <summary>
		/// Read-only total amount of tax associated with the document or transaction.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// </summary>
		/// <value>
		/// Calculated from the <see cref="TaxAmt"/> and <see cref="InclTaxAmt"/> fields.
		/// </value>
		public virtual decimal? TaxTotal
		{
			[PXDependsOnFields(typeof(taxAmt), typeof(inclTaxAmt))]
			get
			{
				return this.TaxAmt - this.InclTaxAmt;
			}

			set
			{
			}
		}
		#endregion

		#region CuryApplAmt
		public abstract class curyApplAmt : PX.Data.BQL.BqlDecimal.Field<curyApplAmt> { }

		/// <summary>
		/// The amount of the application.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="ApplAmt"/>.
		/// </summary>
		/// <value>
		/// The value of this field should be set if a line describes an <see cref="PX.Objects.AP.APPayment">APPayment</see> or
		/// an <see cref="ARPayment">ARPayment</see>, which is applied to existing documents.
		/// Journal Vouchers (GL.30.40.00) screen allows to enter application information through the AP Payment Applications 
		/// and AR Payment Applications tabs.
		/// </value>
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.applAmt))]
		[PXUIField(DisplayName = "Application Amount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryApplAmt
		{
			get;
			set;
		}
		#endregion
		#region ApplAmt
		public abstract class applAmt : PX.Data.BQL.BqlDecimal.Field<applAmt> { }

		/// <summary>
		/// The amount of the application.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// For more infor see <see cref="CuryApplAmt"/>.
		/// </summary>
		[PXDBDecimal(4)]
		public virtual decimal? ApplAmt
		{
			get;
			set;
		}
		#endregion

		#region CuryDiscTaken
		public abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }

		/// <summary>
		/// The amount of the cash discount taken on the document.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="DiscTaken"/>.
		/// </summary>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.discTaken))]
		public virtual decimal? CuryDiscTaken
		{
			get;
			set;
		}
		#endregion
		#region DiscTaken
		public abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }

		/// <summary>
		/// The amount of the cash discount taken on the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryDiscTaken"/>.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DiscTaken
		{
			get;
			set;
		}
		#endregion

		#region CuryTaxWheld
		public abstract class curyTaxWheld : PX.Data.BQL.BqlDecimal.Field<curyTaxWheld> { }

		/// <summary>
		/// The actual amount of tax withheld on the applications of the document.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="TaxWheld"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the values of the <see cref="PX.Objects.AP.APAdjust.CuryAdjdWhTaxAmt">APAdjust.CuryAdjdWhTaxAmt</see> or
		/// <see cref="PX.Objects.AR.ARAdjust.CuryAdjdWhTaxAmt">ARAdjust.CuryAdjdWhTaxAmt</see> fields of the applications assoicated with the line.
		/// </value>
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.taxWheld))]
		public virtual decimal? CuryTaxWheld
		{
			get;
			set;
		}
		#endregion
		#region TaxWheld
		public abstract class taxWheld : PX.Data.BQL.BqlDecimal.Field<taxWheld> { }

		/// <summary>
		/// The actual amount of tax withheld on the applications of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryTaxWheld"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the values of the <see cref="PX.Objects.AP.APAdjust.CuryAdjdWhTaxAmt">APAdjust.CuryAdjdWhTaxAmt</see> or
		/// <see cref="PX.Objects.AR.ARAdjust.CuryAdjdWhTaxAmt">ARAdjust.CuryAdjdWhTaxAmt</see> fields of the applications assoicated with the line.
		/// </value>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? TaxWheld
		{
			get;
			set;
		}
		#endregion

		#region ApplCount
		public abstract class applCount : PX.Data.BQL.BqlInt.Field<applCount> { }

		/// <summary>
		/// The number of applications specified for the document.
		/// The applications are represented by <see cref="PX.Objects.AP.APAdjust">APAdjust</see> or
		/// <see cref="PX.Objects.AR.ARAdjust">ARAdjust</see> records.
		/// </summary>
		[PXDBInt]
		[PXDefault(0)]
		public virtual int? ApplCount
		{
			get;
			set;
		}
		#endregion

		#region CuryUnappliedBal
		public abstract class curyUnappliedBal : PX.Data.BQL.BqlDecimal.Field<curyUnappliedBal> { }

		/// <summary>
		/// Read-only unapplied balance of the document.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="UnappliedBalance"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="CuryTranTotal"/> and <see cref="CuryApplAmt"/> fields.
		/// </value>
		[PXCurrency(typeof(GLTranDoc.curyInfoID), typeof(GLTranDoc.unappliedBal))]
		[PXUIField(DisplayName = "Unapplied Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryUnappliedBal
		{
			[PXDependsOnFields(typeof(curyTranTotal), typeof(curyApplAmt))]
			get
			{
				return ((this.CuryTranTotal ?? decimal.Zero) - (this.CuryApplAmt ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion
		#region UnappliedBal
		public abstract class unappliedBal : PX.Data.BQL.BqlDecimal.Field<unappliedBal> { }

		/// <summary>
		/// Read-only unapplied balance of the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryUnappliedBalance"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="TranTotal"/> and <see cref="ApplAmt"/> fields.
		/// </value>
		[PXDecimal(4)]
		public virtual decimal? UnappliedBal
		{
			[PXDependsOnFields(typeof(tranTotal), typeof(applAmt))]
			get
			{
				return ((this.TranTotal ?? decimal.Zero) - (this.ApplAmt ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion

		#region CuryDiscBal
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }

		/// <summary>
		/// The read-only discount balance of the document - the amount of the cash discount that has not been used.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also <see cref="DiscBal"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="CuryDiscAmt"/> and <see cref="CuryDiscTaken"/> fields.
		/// </value>
		[PXCury(typeof(GLTranDoc.curyID))]
		[PXUIField(DisplayName = "Disc. Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryDiscBal
		{
			[PXDependsOnFields(typeof(curyDiscAmt), typeof(curyDiscTaken))]
			get
			{
				return ((this.CuryDiscAmt ?? decimal.Zero) - (this.CuryDiscTaken ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion
		#region DiscBal
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }

		/// <summary>
		/// The read-only discount balance of the document - the amount of the cash discount that has not been used.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also <see cref="CuryDiscBal"/>.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="DiscAmt"/> and <see cref="DiscTaken"/> fields.
		/// </value>
		[PXDecimal(4)]
		public virtual decimal? DiscBal
		{
			[PXDependsOnFields(typeof(discAmt), typeof(discTaken))]
			get
			{
				return ((this.DiscAmt ?? decimal.Zero) - (this.DiscTaken ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion

		#region CuryWhTaxBal
		public abstract class curyWhTaxBal : PX.Data.BQL.BqlDecimal.Field<curyWhTaxBal> { }

		/// <summary>
		/// The read-only balance of the tax withheld on the document.
		/// Given in the <see cref="CuryID">currency</see> of the line.
		/// See also the <see cref="WhTaxBal"/> field.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="CuryOrigWhTaxAmt"/> and <see cref="CuryTaxWheld"/> fields.
		/// </value>
		[PXCury(typeof(GLTranDoc.curyID))]
		[PXUIField(DisplayName = "Wh. Tax Balance", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? CuryWhTaxBal
		{
			[PXDependsOnFields(typeof(curyOrigWhTaxAmt), typeof(curyTaxWheld))]
			get
			{
				return ((this.CuryOrigWhTaxAmt ?? decimal.Zero) - (this.CuryTaxWheld ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion
		#region WhTaxBal
		public abstract class whTaxBal : PX.Data.BQL.BqlDecimal.Field<whTaxBal> { }

		/// <summary>
		/// The read-only balance of the tax withheld on the document.
		/// Given in the <see cref="Company.BaseCuryID">base currency</see> of the company.
		/// See also the <see cref="CuryWhTaxBal"/> field.
		/// </summary>
		/// <value>
		/// The value of this field is calculated from the <see cref="OrigWhTaxAmt"/> and <see cref="TaxWheld"/> fields.
		/// </value>
		[PXDecimal(4)]
		public virtual decimal? WhTaxBal
		{
			[PXDependsOnFields(typeof(origWhTaxAmt), typeof(taxWheld))]
			get
			{
				return ((this.OrigWhTaxAmt ?? decimal.Zero) - (this.TaxWheld ?? decimal.Zero));
			}

			set
			{
			}
		}
		#endregion

		#region NeedsDebitCashAccount
		public abstract class needsDebitCashAccount : PX.Data.BQL.BqlBool.Field<needsDebitCashAccount> { }

		/// <summary>
		/// The read-only field, indicating whether the <see cref="DebitCashAccount">Debit Cash Account</see>
		/// is required to define the document described by the batch line.
		/// </summary>
		/// <value>
		/// The value of this field is determined by the <see cref="TranModule"/>, <see cref="TranType"/>,
		/// <see cref="EntryTypeID"/> and <see cref="CADrCr"/> fields.
		/// </value>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NeedsDebitCashAccount
		{
			[PXDependsOnFields(typeof(tranModule), typeof(tranType), typeof(entryTypeID), typeof(cADrCr))]
			get
			{
				bool? result = false;
				if (this.TranModule == BatchModule.AP)
				{
					if (this.TranType == AP.APPaymentType.Refund
						|| this.TranType == AP.APPaymentType.VoidCheck
						|| this.TranType == AP.APPaymentType.VoidQuickCheck)
					{
						result = true;
					}
				}

				if (this.TranModule == BatchModule.AR)
				{
					if (this.TranType == AR.ARPaymentType.Payment 
						|| this.TranType == AR.ARPaymentType.Prepayment
						|| this.TranType == AR.ARPaymentType.CashSale)
					{
						result = true;
					}
				}

				if (this.TranModule == BatchModule.CA)
				{
					if (!string.IsNullOrEmpty(this.EntryTypeID))
					{
						result = (this.CADrCr == CA.CADrCr.CADebit);
					}
				}
				return result;
			}

			set
			{
			}
		}
		#endregion
		#region NeedsCreditCashAccount
		public abstract class needsCreditCashAccount : PX.Data.BQL.BqlBool.Field<needsCreditCashAccount> { }

		/// <summary>
		/// The read-only field, indicating whether the <see cref="CreditCashAccount">Credit Cash Account</see>
		/// is required to define the document described by the batch line.
		/// </summary>
		/// <value>
		/// The value of this field is determined by the <see cref="TranModule"/>, <see cref="TranType"/>,
		/// <see cref="EntryTypeID"/> and <see cref="CADrCr"/> fields.
		/// </value>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NeedsCreditCashAccount
		{
			[PXDependsOnFields(typeof(tranModule), typeof(tranType), typeof(entryTypeID), typeof(cADrCr))]
			get
			{
				bool? result = false;
				if (this.TranModule == BatchModule.AP)
				{
					if (this.TranType == AP.APPaymentType.Check
						|| this.TranType == AP.APPaymentType.Prepayment
						|| this.TranType == AP.APPaymentType.QuickCheck)
					{
						result = true;
					}
				}

				if (this.TranModule == BatchModule.AR)
				{
					if (this.TranType == AR.ARPaymentType.Refund
						|| this.TranType == AR.ARPaymentType.VoidPayment
						|| this.TranType == AR.ARPaymentType.CashReturn)
					{
						result = true;
					}
				}

				if (this.TranModule == BatchModule.CA)
				{
					if (!string.IsNullOrEmpty(this.EntryTypeID))
					{
						result = (this.CADrCr == CA.CADrCr.CACredit);
					}
				}
				return result;
			}

			set
			{
			}
		}
		#endregion

		#region NeedTaskValidation
		public abstract class needTaskValidation : PX.Data.BQL.BqlBool.Field<needTaskValidation> { }

		/// <summary>
		/// Indicates whether validation for the presence of the correct <see cref="TaskID"/> must be performed for the line before it is persisted to the database.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? NeedTaskValidation
		{
			[PXDependsOnFields(typeof(tranModule), typeof(split), typeof(parentLineNbr))]
			get
			{
				if (this.Split == true && this.IsChildTran == false)
				{
					return false;
				}
				return true;
			}

			set
			{
			}
		}
		#endregion
		internal bool IsBalanced
		{
			[PXDependsOnFields(typeof(creditAccountID), typeof(debitAccountID))]
			get
			{
				return this.CreditAccountID.HasValue && this.DebitAccountID.HasValue;
			}
		}

		internal bool IsChildTran
		{
			[PXDependsOnFields(typeof(parentLineNbr))]
			get { return this.ParentLineNbr.HasValue; }
		}

		#region DebitCashAccountID
		public abstract class debitCashAccountID : PX.Data.BQL.BqlInt.Field<debitCashAccountID> { }

		/// <summary>
		/// The identifier of the <see cref="CashAccount">Debit Cash Account</see> associated with the document.
		/// Debit Cash Account is required for some types of the documents, that can be included in a <see cref="GLDocBatch">document batch</see>
		/// - see <see cref="NeedsDebitCashAccount"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[PXDBInt]
		public virtual int? DebitCashAccountID
		{
			get;
			set;
		}
		#endregion
		#region CreditCashAccountID
		public abstract class creditCashAccountID : PX.Data.BQL.BqlInt.Field<creditCashAccountID> { }

		/// <summary>
		/// The identifier of the <see cref="CashAccount">Credit Cash Account</see> associated with the document.
		/// Credit Cash Account is required for some types of the documents, that can be included in a <see cref="GLDocBatch">document batch</see>
		/// - see <see cref="NeedsCreditCashAccount"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
		/// </value>
		[PXDBInt]
		public virtual int? CreditCashAccountID
		{
			get;
			set;
		}
		#endregion
		#region IInvoice Members

		decimal? IInvoice.CuryDocBal
		{
			get
			{
				return this.CuryUnappliedBal;
			}

			set
			{
				this.CuryUnappliedBal = value;
			}
		}

		decimal? IInvoice.DocBal
		{
			get
			{
				return UnappliedBal;
			}

			set
			{
				this.UnappliedBal = value;
			}
		}

		decimal? IInvoice.CuryDiscBal
		{
			get
			{
				return this.CuryDiscBal;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		decimal? IInvoice.DiscBal
		{
			get
			{
				return this.DiscBal;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		decimal? IInvoice.CuryWhTaxBal
		{
			get
			{
				return this.CuryWhTaxBal;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		decimal? IInvoice.WhTaxBal
		{
			get
			{
				return this.WhTaxBal;
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		long? CM.IRegister.CuryInfoID
		{
			get
			{
				return this.CuryInfoID;
			}

			set
			{
			}
		}

		DateTime? IInvoice.DiscDate
		{
			get
			{
				return this.DiscDate;
			}

			set
			{
			}
		}

		public string DocType
		{
			get
			{
				return this.TranType;
			}

			set
			{
			}
		}

		public string OrigModule
		{
			get
			{
				return this.Module;
			}

			set
			{
			}
		}

		public DateTime? DocDate
		{
			get
			{
				return null;
			}

			set
			{
			}
		}

		public string DocDesc
		{
			get
			{
				return null;
			}

			set
			{
			}
		}

		public decimal? CuryOrigDocAmt
		{
			get
			{
				return null;
			}

			set
			{
			}
		}

		public decimal? OrigDocAmt
		{
			get
			{
				return null;
			}

			set
			{
			}
		}

		#endregion
	}

	public interface IRegister
	{
		int? BAccountID
		{
			get;
			set;
		}

		int? LocationID
		{
			get;
			set;
		}

		int? BranchID
		{
			get;
			set;
		}

		int? AccountID
		{
			get;
			set;
		}

		int? SubID
		{
			get;
			set;
		}

		string FinPeriodID
		{
			get;
			set;
		}

		long? CuryInfoID
		{
			get;
			set;
		}
	}
}
