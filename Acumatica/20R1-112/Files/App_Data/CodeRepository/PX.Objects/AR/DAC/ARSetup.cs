using System;
using System.Globalization;

using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

using PX.Objects.Common;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.GL;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents the company-level Accounts Receivable preferences record. 
	/// The parameters defined by this record include various numbering sequence 
	/// settings and document processing options. The single record of this type 
	/// is created and edited on the Accounts Receivable Preferences (AR101000) form, 
	/// which corresponds to the <see cref="ARSetupMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(ARSetupMaint))]
	[PXCacheName(Messages.ARSetup)]
	public partial class ARSetup : PX.Data.IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.BQL.BqlString.Field<batchNumberingID> { }
		protected String _BatchNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "GL Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String BatchNumberingID
		{
			get
			{
				return this._BatchNumberingID;
			}
			set
			{
				this._BatchNumberingID = value;
			}
		}
		#endregion
		#region DfltCustomerClassID
		public abstract class dfltCustomerClassID : PX.Data.BQL.BqlString.Field<dfltCustomerClassID> { }
		protected String _DfltCustomerClassID;
		[PXDBString(10, IsUnicode = true)]
		//[PXDefault("DEFAULT")]
		[PXUIField(DisplayName = "Default Customer Class ID", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(CustomerClass.customerClassID), CacheGlobal = true)]
		public virtual String DfltCustomerClassID
		{
			get
			{
				return this._DfltCustomerClassID;
			}
			set
			{
				this._DfltCustomerClassID = value;
			}
		}
		#endregion
		#region PerRetainTran
		public abstract class perRetainTran : PX.Data.BQL.BqlShort.Field<perRetainTran> { }
		protected Int16? _PerRetainTran;
		[PXDBShort()]
		[PXDefault((short)99)]
		[PXUIField(DisplayName = "Keep Transactions for", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? PerRetainTran
		{
			get
			{
				return this._PerRetainTran;
			}
			set
			{
				this._PerRetainTran = value;
			}
		}
		#endregion
		#region PerRetainHist
		public abstract class perRetainHist : PX.Data.BQL.BqlShort.Field<perRetainHist> { }
		protected Int16? _PerRetainHist;
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Periods to Retain History", Visibility = PXUIVisibility.Invisible)]
		public virtual Int16? PerRetainHist
		{
			get
			{
				return this._PerRetainHist;
			}
			set
			{
				this._PerRetainHist = value;
			}
		}
		#endregion
		#region InvoiceNumberingID
		public abstract class invoiceNumberingID : PX.Data.BQL.BqlString.Field<invoiceNumberingID> { }
		protected String _InvoiceNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Invoice Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String InvoiceNumberingID
		{
			get
			{
				return this._InvoiceNumberingID;
			}
			set
			{
				this._InvoiceNumberingID = value;
			}
		}
		#endregion
		#region PaymentNumberingID
		public abstract class paymentNumberingID : PX.Data.BQL.BqlString.Field<paymentNumberingID> { }
		protected String _PaymentNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARPAYMENT")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Payment Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String PaymentNumberingID
		{
			get
			{
				return this._PaymentNumberingID;
			}
			set
			{
				this._PaymentNumberingID = value;
			}
		}
		#endregion
		#region CreditAdjNumberingID
		public abstract class creditAdjNumberingID : PX.Data.BQL.BqlString.Field<creditAdjNumberingID> { }
		protected String _CreditAdjNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Credit Memo Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String CreditAdjNumberingID
		{
			get
			{
				return this._CreditAdjNumberingID;
			}
			set
			{
				this._CreditAdjNumberingID = value;
			}
		}
		#endregion
		#region DebitAdjNumberingID
		public abstract class debitAdjNumberingID : PX.Data.BQL.BqlString.Field<debitAdjNumberingID> { }
		protected String _DebitAdjNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Debit Memo Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String DebitAdjNumberingID
		{
			get
			{
				return this._DebitAdjNumberingID;
			}
			set
			{
				this._DebitAdjNumberingID = value;
			}
		}
		#endregion
		#region WriteOffNumberingID
		public abstract class writeOffNumberingID : PX.Data.BQL.BqlString.Field<writeOffNumberingID> { }
		protected String _WriteOffNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Write-Off Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String WriteOffNumberingID
		{
			get
			{
				return this._WriteOffNumberingID;
			}
			set
			{
				this._WriteOffNumberingID = value;
			}
		}
		#endregion
		#region UsageNumberingID
		public abstract class usageNumberingID : PX.Data.BQL.BqlString.Field<usageNumberingID> { }
		[PXDefault("PMTRAN")]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Usage Transaction Numbering Sequence")]
		public virtual String UsageNumberingID
		{
			get;
			set;
		}
		#endregion
		#region PriceWSNumberingID
		public abstract class priceWSNumberingID : PX.Data.BQL.BqlString.Field<priceWSNumberingID> { }
		protected String _PriceWSNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARPRICEWS")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Price Worksheet Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String PriceWSNumberingID
		{
			get
			{
				return this._PriceWSNumberingID;
			}
			set
			{
				this._PriceWSNumberingID = value;
			}
		}
		#endregion
		#region DunningFeeNumberingID
		public abstract class dunningFeeNumberingID : PX.Data.BQL.BqlString.Field<dunningFeeNumberingID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, Equal<False>>), GL.Messages.ManualNumberingDisabled)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Dunning Fee Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual string DunningFeeNumberingID
		{
			get;
			set;
		}
		#endregion
		#region DefaultTranDesc
		public abstract class defaultTranDesc : PX.Data.BQL.BqlString.Field<defaultTranDesc> { }
		protected String _DefaultTranDesc;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("C")]
		[PXStringList(new string[] { "C", "I", "N", "U" }, new string[] { "Combination ID and Name", "Vendor ID", "Vendor Name", "User Entered Description" })]
		[PXUIField(DisplayName = "Default Transaction Description", Visibility = PXUIVisibility.Invisible)]
		public virtual String DefaultTranDesc
		{
			get
			{
				return this._DefaultTranDesc;
			}
			set
			{
				this._DefaultTranDesc = value;
			}
		}
		#endregion
		#region SalesSubMask
		public abstract class salesSubMask : PX.Data.BQL.BqlString.Field<salesSubMask> { }
		protected String _SalesSubMask;
		[PXDefault()]
		[SubAccountMask(DisplayName = "Combine Sales Sub. from")]
		public virtual String SalesSubMask
		{
			get
			{
				return this._SalesSubMask;
			}
			set
			{
				this._SalesSubMask = value;
			}
		}
		#endregion
		#region AutoPost
		public abstract class autoPost : PX.Data.BQL.BqlBool.Field<autoPost> { }
		protected bool? _AutoPost;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Post on Release", Visibility = PXUIVisibility.Visible)]
		public virtual bool? AutoPost
		{
			get
			{
				return this._AutoPost;
			}
			set
			{
				this._AutoPost = value;
			}
		}
		#endregion
		#region TransactionPosting
		public abstract class transactionPosting : PX.Data.BQL.BqlString.Field<transactionPosting> { }
		protected String _TransactionPosting;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("D")]
		[PXUIField(DisplayName = "Transaction Posting", Visibility = PXUIVisibility.Invisible)]
		[PXStringList(new string[] { "S", "D" }, new string[] { "Summary", "Detail" })]
		public virtual String TransactionPosting
		{
			get
			{
				return this._TransactionPosting;
			}
			set
			{
				this._TransactionPosting = value;
			}
		}
		#endregion
		#region FinChargeNumberingID
		public abstract class finChargeNumberingID : PX.Data.BQL.BqlString.Field<finChargeNumberingID> { }
		protected String _FinChargeNumberingID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("ARINVOICE")]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Overdue Charge Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String FinChargeNumberingID
		{
			get
			{
				return this._FinChargeNumberingID;
			}
			set
			{
				this._FinChargeNumberingID = value;
			}
		}
		#endregion
		#region FinChargeOnCharge
		public abstract class finChargeOnCharge : PX.Data.BQL.BqlBool.Field<finChargeOnCharge> { }
		protected bool? _FinChargeOnCharge;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Messages.CalculateOnOverdueChargeDocuments, Visibility = PXUIVisibility.Visible)]
		public virtual bool? FinChargeOnCharge
		{
			get
			{
				return this._FinChargeOnCharge;
			}
			set
			{
				this._FinChargeOnCharge = value;
			}
		}
		#endregion
		#region AgeCredits
		public abstract class ageCredits : PX.Data.BQL.BqlBool.Field<ageCredits> { }
		protected Boolean? _AgeCredits;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Age Credits", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AgeCredits
		{
			get
			{
				return this._AgeCredits;
			}
			set
			{
				this._AgeCredits = value;
			}
		}
		#endregion
		#region ValidateDataConsistencyOnRelease
		[Obsolete("This field is not used anymore and will be removed in 2018R2. Use " + nameof(DataInconsistencyHandlingMode) + "instead.")]
		public abstract class validateDataConsistencyOnRelease : PX.Data.BQL.BqlBool.Field<validateDataConsistencyOnRelease> { }
		[Obsolete("This field is not used anymore and will be removed in 2018R2. Use " + nameof(DataInconsistencyHandlingMode) + "instead.")]
		public virtual bool? ValidateDataConsistencyOnRelease
		{
			get;
			set;
		}
		#endregion
		#region DataInconsistencyHandlingMode
		public abstract class dataInconsistencyHandlingMode : PX.Data.BQL.BqlString.Field<dataInconsistencyHandlingMode> { }
		[PXDBString(1)]
		[PXDefault(Common.DataIntegrity.InconsistencyHandlingMode.Log)]
		[PXUIField(DisplayName = Common.Messages.ExtraDataIntegrityValidation)]
		[LabelList(typeof(Common.DataIntegrity.InconsistencyHandlingMode))]
		public virtual string DataInconsistencyHandlingMode
		{
			get;
			set;
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
		#region HoldEntry
		public abstract class holdEntry : PX.Data.BQL.BqlBool.Field<holdEntry> { }
		protected Boolean? _HoldEntry;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Documents on Entry")]
		public virtual Boolean? HoldEntry
		{
			get
			{
				return this._HoldEntry;
			}
			set
			{
				this._HoldEntry = value;
			}
		}
		#endregion
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.BQL.BqlBool.Field<requireControlTotal> { }
		protected Boolean? _RequireControlTotal;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Document Totals on Entry")]
		public virtual Boolean? RequireControlTotal
		{
			get
			{
				return this._RequireControlTotal;
			}
			set
			{
				this._RequireControlTotal = value;
			}
		}
		#endregion
		#region RequireExtRef
		public abstract class requireExtRef : PX.Data.BQL.BqlBool.Field<requireExtRef> { }
		protected Boolean? _RequireExtRef;

		/// <summary>
		/// When set to <c>true</c>, indicates that users must fill Ext. Ref. Number box (<see cref="PX.Objects.GL.GLTranDoc.ExtRefNbr">GLTranDoc.ExtRefNbr</see>).
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Payment Reference on Entry")]
		public virtual Boolean? RequireExtRef
		{
			get
			{
				return this._RequireExtRef;
			}
			set
			{
				this._RequireExtRef = value;
			}
		}
		#endregion
		#region SummaryPost
		public abstract class summaryPost : PX.Data.BQL.BqlBool.Field<summaryPost> { }
		[PXBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Post Summary on Updating GL", Visibility = PXUIVisibility.Visible)]
		public virtual bool? SummaryPost
		{
			get
			{
				return (this._TransactionPosting == "S");
			}
			set
			{
				this._TransactionPosting = (value == true) ? "S" : "D";
			}
		}
		#endregion
		#region CreditCheckError
		public abstract class creditCheckError : PX.Data.BQL.BqlBool.Field<creditCheckError> { }
		protected Boolean? _CreditCheckError;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Hold Document on Failed Credit Check", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? CreditCheckError
		{
			get
			{
				return this._CreditCheckError;
			}
			set
			{
				this._CreditCheckError = value;
			}
		}
		#endregion

		#region SPCommnCalcType
		public abstract class sPCommnCalcType : PX.Data.BQL.BqlString.Field<sPCommnCalcType> { }
		protected String _SPCommnCalcType;
		[PXDBString(1)]
		[PXDefault(SPCommnCalcTypes.ByInvoice)]
		[PXUIField(DisplayName = "Salesperson Commission by", Visibility = PXUIVisibility.Visible)]
		[SPCommnCalcTypes.List()]
		public virtual String SPCommnCalcType
		{
			get
			{
				return this._SPCommnCalcType;
			}
			set
			{
				this._SPCommnCalcType = value;
			}
		}
		#endregion
		#region SPCommnPeriodType
		public abstract class sPCommnPeriodType : PX.Data.BQL.BqlString.Field<sPCommnPeriodType> { }
		protected String _SPCommnPeriodType;
		[PXDBString(1)]
		[PXDefault(SPCommnPeriodTypes.Monthly)]
		[PXUIField(DisplayName = "Commission Period Type")]
		[SPCommnPeriodTypes.List()]
		public virtual String SPCommnPeriodType
		{
			get
			{
				return this._SPCommnPeriodType;
			}
			set
			{
				this._SPCommnPeriodType = value;
			}
		}
		#endregion
		#region DefFinChargeFromCycle
		public abstract class defFinChargeFromCycle : PX.Data.BQL.BqlBool.Field<defFinChargeFromCycle> { }
		protected Boolean? _DefFinChargeFromCycle;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Set Default Overdue Charges by Statement Cycle")]
		public virtual Boolean? DefFinChargeFromCycle
		{
			get
			{
				return this._DefFinChargeFromCycle;
			}
			set
			{
				this._DefFinChargeFromCycle = value;
			}
		}
		#endregion
		#region FinChargeFirst
		public abstract class finChargeFirst : PX.Data.BQL.BqlBool.Field<finChargeFirst> { }
		protected Boolean? _FinChargeFirst;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Apply Payments to Overdue Charges First")]
		public virtual Boolean? FinChargeFirst
		{
			get
			{
				return this._FinChargeFirst;
			}
			set
			{
				this._FinChargeFirst = value;
			}
		}
		#endregion
		#region PrintBeforeRelease
		public abstract class printBeforeRelease : PX.Data.BQL.BqlBool.Field<printBeforeRelease> { }
		protected Boolean? _PrintBeforeRelease;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Invoice/Memo Printing Before Release")]
		public virtual Boolean? PrintBeforeRelease
		{
			get
			{
				return this._PrintBeforeRelease;
			}
			set
			{
				this._PrintBeforeRelease = value;
			}
		}
		#endregion
		#region EmailBeforeRelease
		public abstract class emailBeforeRelease : PX.Data.BQL.BqlBool.Field<emailBeforeRelease> { }
		protected Boolean? _EmailBeforeRelease;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Invoice/Memo Emailing Before Release")]
		public virtual Boolean? EmailBeforeRelease
		{
			get
			{
				return this._EmailBeforeRelease;
			}
			set
			{
				this._EmailBeforeRelease = value;
			}
		}
		#endregion

		#region IntegratedCCProcessing
		public abstract class integratedCCProcessing : PX.Data.BQL.BqlBool.Field<integratedCCProcessing> { }
		protected Boolean? _IntegratedCCProcessing;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Enable Integrated CC Processing", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? IntegratedCCProcessing
		{
			get
			{
				return this._IntegratedCCProcessing;
			}
			set
			{
				this._IntegratedCCProcessing = value;
			}
		}
		#endregion

		#region ConsolidatedStatement
		public abstract class consolidatedStatement : PX.Data.BQL.BqlBool.Field<consolidatedStatement> { }
		protected Boolean? _ConsolidatedStatement;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Statements for all Branches")]
		public virtual Boolean? ConsolidatedStatement
		{
			get
			{
				return this._ConsolidatedStatement;
			}
			set
			{
				this._ConsolidatedStatement = value;
			}
		}
		#endregion
		#region StatementBranchID
		public abstract class statementBranchID : PX.Data.BQL.BqlInt.Field<statementBranchID> { }
		protected Int32? _StatementBranchID;
		[GL.Branch(DisplayName="Statement from Branch", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? StatementBranchID
		{
			get
			{
				return this._StatementBranchID;
			}
			set
			{
				this._StatementBranchID = value;
			}
		}
		#endregion
		#region ConsolidatedDunningLetter
		public abstract class consolidatedDunningLetter : PX.Data.BQL.BqlBool.Field<consolidatedDunningLetter> { }
		protected Boolean? _ConsolidatedDunningLetter;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Dunning Letters for all Branches")]
		public virtual Boolean? ConsolidatedDunningLetter
		{
			get
			{
				return this._ConsolidatedDunningLetter;
			}
			set
			{
				this._ConsolidatedDunningLetter = value;
			}
		}
		#endregion
		#region DunningLetterBranchID
		public abstract class dunningLetterBranchID : PX.Data.BQL.BqlInt.Field<dunningLetterBranchID> { }
		protected Int32? _DunningLetterBranchID;
		[GL.Branch(DisplayName ="Dunning Letter from Branch", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Int32? DunningLetterBranchID
		{
			get
			{
				return this._DunningLetterBranchID;
			}
			set
			{
				this._DunningLetterBranchID = value;
			}
		}
		#endregion
		#region DunningLetterProcessType
		public abstract class dunningLetterProcessType : PX.Data.BQL.BqlInt.Field<dunningLetterProcessType> { }
		protected Int32? _DunningLetterProcessType;
		[PXDBInt()]
		[PXDefault(0)]
		[DunningProcessType.List]
		[PXUIField(DisplayName = "Dunning Process", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? DunningLetterProcessType
		{
			get
			{
				return this._DunningLetterProcessType;
			}
			set
			{
				this._DunningLetterProcessType = value;
			}
		}
		#endregion
		#region AutoReleaseDunningFee
		public abstract class autoReleaseDunningFee : PX.Data.BQL.BqlBool.Field<autoReleaseDunningFee> { }
		protected bool? _AutoReleaseDunningFee;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Release Dunning Fee Documents", Visibility = PXUIVisibility.Visible)]
		public virtual bool? AutoReleaseDunningFee
		{
			get
			{
				return this._AutoReleaseDunningFee;
			}
			set
			{
				this._AutoReleaseDunningFee = value;
			}
		}
		#endregion
		#region AutoReleaseDunningLetter
		public abstract class autoReleaseDunningLetter : PX.Data.BQL.BqlBool.Field<autoReleaseDunningLetter> { }
		protected bool? _AutoReleaseDunningLetter;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Release Dunning Letters", Visibility = PXUIVisibility.Visible, Enabled=false)]
		public virtual bool? AutoReleaseDunningLetter
		{
			get
			{
				return this._AutoReleaseDunningLetter;
			}
			set
			{
				this._AutoReleaseDunningLetter = value;
			}
		}
		#endregion
		#region IncludeNonOverdueDunning
		public abstract class includeNonOverdueDunning : PX.Data.BQL.BqlBool.Field<includeNonOverdueDunning> { }
		protected bool? _IncludeNonOverdueDunning;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = Messages.IncludeNonOverdue, Visibility = PXUIVisibility.Visible)]
		public virtual bool? IncludeNonOverdueDunning
		{
			get
			{
				return this._IncludeNonOverdueDunning;
			}
			set
			{
				this._IncludeNonOverdueDunning = value;
			}
		}
		#endregion

		#region DunningFeeInventoryID
		public abstract class dunningFeeInventoryID : PX.Data.BQL.BqlInt.Field<dunningFeeInventoryID> { }
		protected Int32? _DunningFeeInventoryID;
		[PXDefault(PersistingCheck=PXPersistingCheck.Nothing)]
		[IN.NonStockItem(DisplayName = "Dunning Fee Item")]
		[PXForeignReference(typeof(Field<dunningFeeInventoryID>.IsRelatedTo<IN.InventoryItem.inventoryID>))]
		public virtual Int32? DunningFeeInventoryID
		{
			get
			{
				return this._DunningFeeInventoryID;
			}
			set
			{
				this._DunningFeeInventoryID = value;
			}
		}
		#endregion

		#region DunningFeeTermID
		public abstract class dunningFeeTermID : PX.Data.BQL.BqlString.Field<dunningFeeTermID> { }

		/// <summary>
		/// The identifier of the Dunning Fee <see cref="Terms"/> object that can be set for all dunning fees as default one. 
		/// Optinal field that defaults the credit terms for dunning fee invoice.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.Visible)]
		[ARTermsSelector]
		public virtual string DunningFeeTermID
		{
			get;
			set;
		}
		#endregion


		#region InvoicePrecision
		public abstract class invoicePrecision : PX.Data.BQL.BqlDecimal.Field<invoicePrecision> { }
		protected decimal? _InvoicePrecision;
		[PXDBDecimalString(2)]
		[InvoicePrecision.List]
		[PXDefault(TypeCode.Decimal, CS.InvoicePrecision.m01)]
		[PXUIField(DisplayName = "Rounding Precision")]
		public virtual decimal? InvoicePrecision
		{
			get
			{
				return this._InvoicePrecision;
			}
			set
			{
				this._InvoicePrecision = value;
			}
		}
		#endregion
		#region InvoiceRounding
		public abstract class invoiceRounding : PX.Data.BQL.BqlString.Field<invoiceRounding> { }
		protected String _InvoiceRounding;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(RoundingType.Currency)]
		[PXUIField(DisplayName = "Rounding Rule for Invoices")]
		[InvoiceRounding.List]
		public virtual String InvoiceRounding
		{
			get
			{
				return this._InvoiceRounding;
			}
			set
			{
				this._InvoiceRounding = value;
			}
		}
		#endregion

		#region BalanceWriteOff
		public abstract class balanceWriteOff : PX.Data.BQL.BqlString.Field<balanceWriteOff> { }
		protected String _BalanceWriteOff;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.balanceWriteOff>>>))]
		[PXUIField(DisplayName = "Balance Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<ARSetup.balanceWriteOff>.IsRelatedTo<ReasonCode.reasonCodeID>))]
		public virtual String BalanceWriteOff
		{
			get
			{
				return this._BalanceWriteOff;
			}
			set
			{
				this._BalanceWriteOff = value;
			}
		}
		#endregion
		#region CreditWriteOff
		public abstract class creditWriteOff : PX.Data.BQL.BqlString.Field<creditWriteOff> { }
		protected String _CreditWriteOff;
		[PXDBString(ReasonCode.reasonCodeID.Length, IsUnicode = true)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.creditWriteOff>>>))]
		[PXUIField(DisplayName = "Credit Write-Off Reason Code", Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<ARSetup.creditWriteOff>.IsRelatedTo<ReasonCode.reasonCodeID>))]
		public virtual String CreditWriteOff
		{
			get
			{
				return this._CreditWriteOff;
			}
			set
			{
				this._CreditWriteOff = value;
			}
		}
		#endregion

		#region DefaultRateTypeID
		public abstract class defaultRateTypeID : PX.Data.BQL.BqlString.Field<defaultRateTypeID> { }
		protected String _DefaultRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(PX.Objects.CM.CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Default Rate Type")]
		public virtual String DefaultRateTypeID
		{
			get
			{
				return this._DefaultRateTypeID;
			}
			set
			{
				this._DefaultRateTypeID = value;
			}
		}
		#endregion
		#region AlwaysFromBaseCury
		public abstract class alwaysFromBaseCury : PX.Data.BQL.BqlBool.Field<alwaysFromBaseCury> { }
		protected Boolean? _AlwaysFromBaseCury;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Always Calculate Price from Base Currency Price")]
		public virtual Boolean? AlwaysFromBaseCury
		{
			get
			{
				return this._AlwaysFromBaseCury;
			}
			set
			{
				this._AlwaysFromBaseCury = value;
			}
		}
		#endregion
		#region LoadSalesPricesUsingAlternateID
		public abstract class loadSalesPricesUsingAlternateID : PX.Data.BQL.BqlBool.Field<loadSalesPricesUsingAlternateID> { }

		/// <summary>
		/// When set to <c>true</c>, makes it possible to load 
		/// <see cref="ARSalesPrice">Sales Prices</see> by
		/// alternate ID
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Load Sales Prices by Alternate ID")]
		public virtual Boolean? LoadSalesPricesUsingAlternateID { get; set; }
		#endregion
		#region LineDiscountTarget
		public abstract class lineDiscountTarget : PX.Data.BQL.BqlString.Field<lineDiscountTarget> { }
		protected String _LineDiscountTarget;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(LineDiscountTargetType.ExtendedPrice)]
		[LineDiscountTargetType.List()]
		[PXUIField(DisplayName = "Line Discount Basis", Visibility = PXUIVisibility.Visible)]
		public virtual String LineDiscountTarget
		{
			get
			{
				return this._LineDiscountTarget;
			}
			set
			{
				this._LineDiscountTarget = value;
			}
		}
		#endregion
		#region ApplyQuantityDiscountBy
		public abstract class applyQuantityDiscountBy : PX.Data.BQL.BqlString.Field<applyQuantityDiscountBy> { }
		protected String _ApplyQuantityDiscountBy;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ApplyQuantityDiscountType.DocumentLineUOM, PersistingCheck = PXPersistingCheck.Null)]
		[ApplyQuantityDiscountType.List]
		[PXUIField(DisplayName = "Apply Quantity Discounts To", Visibility = PXUIVisibility.Visible)]
		public virtual String ApplyQuantityDiscountBy
		{
			get
			{
				return this._ApplyQuantityDiscountBy;
			}
			set
			{
				this._ApplyQuantityDiscountBy = value;
			}
		}
		#endregion

		#region ApplyDiscountToLabelOnly
		public abstract class applyDiscountToLabelOnly : PX.Data.BQL.BqlBool.Field<applyDiscountToLabelOnly> { }
		[PXString()]
		[PXUIField(DisplayName = "Apply Line Discount to Prices Specific To", Enabled = false, IsReadOnly = true)]
		public virtual Boolean? ApplyDiscountToLabelOnly { get; }
		#endregion
		#region ApplyLineDiscountsIfCustomerPriceDefined
		public abstract class applyLineDiscountsIfCustomerPriceDefined : PX.Data.BQL.BqlBool.Field<applyLineDiscountsIfCustomerPriceDefined> { }
		protected Boolean? _ApplyLineDiscountsIfCustomerPriceDefined;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Customer")]
		public virtual Boolean? ApplyLineDiscountsIfCustomerPriceDefined
		{
			get
			{
				return this._ApplyLineDiscountsIfCustomerPriceDefined;
			}
			set
			{
				this._ApplyLineDiscountsIfCustomerPriceDefined = value;
			}
		}
		#endregion
		#region ApplyLineDiscountsIfCustomerClassPriceDefined
		public abstract class applyLineDiscountsIfCustomerClassPriceDefined : PX.Data.BQL.BqlBool.Field<applyLineDiscountsIfCustomerClassPriceDefined> { }
		protected Boolean? _ApplyLineDiscountsIfCustomerClassPriceDefined;
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Customer Price Class")]
		public virtual Boolean? ApplyLineDiscountsIfCustomerClassPriceDefined
		{
			get
			{
				return this._ApplyLineDiscountsIfCustomerClassPriceDefined;
			}
			set
			{
				this._ApplyLineDiscountsIfCustomerClassPriceDefined = value;
			}
		}
		#endregion

		#region RetentionType
		public abstract class retentionType : PX.Data.BQL.BqlString.Field<retentionType> { }
		protected String _RetentionType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(RetentionTypeList.LastPrice)]
		[RetentionTypeList.List()]
		[PXUIField(DisplayName = "Retention Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String RetentionType
		{
			get
			{
				return this._RetentionType;
			}
			set
			{
				this._RetentionType = value;
			}
		}
		#endregion
		#region NumberOfMonths
		public abstract class numberOfMonths : PX.Data.BQL.BqlInt.Field<numberOfMonths> { }
		protected Int32? _NumberOfMonths;
		[PXDBInt()]
		[PXDefault(12)]
		[PXUIField(DisplayName = "Number of Months", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? NumberOfMonths
		{
			get
			{
				return this._NumberOfMonths;
			}
			set
			{
				this._NumberOfMonths = value;
			}
		}
		#endregion

		#region AutoReleasePPDCreditMemo
		public abstract class autoReleasePPDCreditMemo : PX.Data.BQL.BqlBool.Field<autoReleasePPDCreditMemo> { }
		protected bool? _AutoReleasePPDCreditMemo;

		/// <summary>
		/// When <c>true</c>, indicates that the credit memo generated on the "Generate VAT Credit Memos" (AR.50.45.00) form will be released automatically.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release Credit Memos")]
		public virtual bool? AutoReleasePPDCreditMemo
		{
			get
			{
				return _AutoReleasePPDCreditMemo;
			}
			set
			{
				_AutoReleasePPDCreditMemo = value;
			}
		}
		#endregion
		#region PPDCreditMemoDescr
		public abstract class pPDCreditMemoDescr : PX.Data.BQL.BqlString.Field<pPDCreditMemoDescr> { }
		protected string _PPDCreditMemoDescr;

		/// <summary>
		/// The description of the credit memo generated on the "Generate VAT Credit Memos" (AR.50.45.00) form.
		/// </summary>
		[PXDBLocalizableString(150, IsUnicode = true)]
		[PXUIField(DisplayName = "Credit Memo Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string PPDCreditMemoDescr
		{
			get
			{
				return _PPDCreditMemoDescr;
			}
			set
			{
				_PPDCreditMemoDescr = value;
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
		#region MigrationMode
		public abstract class migrationMode : PX.Data.BQL.BqlBool.Field<migrationMode> { }
		/// <summary>
		/// Specifies (if set to <c>true</c>) that migration mode is activated for the AR module.
		/// In other words, this gives an ability to create the document with starting balance without any applications.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = Common.Messages.ActivateMigrationMode)]
		public virtual bool? MigrationMode
		{
			get;
			set;
		}
		#endregion

		#region RetainTaxes
		public abstract class retainTaxes : PX.Data.BQL.BqlBool.Field<retainTaxes> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Retain Taxes", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual bool? RetainTaxes
		{
			get;
			set;
		}
		#endregion
		#region RetainageInvoicesAutoRelease
		public abstract class retainageInvoicesAutoRelease : PX.Data.BQL.BqlBool.Field<retainageInvoicesAutoRelease> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Automatically Release Retainage Invoices", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		public virtual bool? RetainageInvoicesAutoRelease
		{
			get;
			set;
		}
		#endregion

		#region AutoLoadMaxDocs
		public abstract class autoLoadMaxDocs : PX.Data.BQL.BqlShort.Field<perRetainHist> { }
		[PXDBShort()]
		[PXDefault((short)100)]
		[PXUIField(DisplayName = "Max. Number of Documents by Auto Loading", Visibility = PXUIVisibility.Invisible)]
		public virtual Int16? AutoLoadMaxDocs
		{
			get;
			set;
		}
		#endregion
	}

	public class SPCommnCalcTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ByInvoice, ByPayment },
				new string[] { Messages.ByInvoice, Messages.ByPayment }) { }
		}

		public const string ByInvoice = "I";
		public const string ByPayment = "P";

		public class byInvoice : PX.Data.BQL.BqlString.Constant<byInvoice>
		{
			public byInvoice() : base(ByInvoice) { ;}
		}

		public class byPayment : PX.Data.BQL.BqlString.Constant<byPayment>
		{
			public byPayment() : base(ByPayment) { ;}
		}
	}

	public class SPCommnPeriodTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Monthly, Quarterly, Yearly, FiscalPeriod },
				new string[] { Messages.Monthly, Messages.Quarterly, Messages.Yearly, Messages.FiscalPeriod }) { }
		}

		public const string Monthly = "M";
		public const string Quarterly = "Q";
		public const string Yearly = "Y";
		public const string FiscalPeriod = "F";

		public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
		{
			public monthly() : base(Monthly) { ;}
		}
		public class quarterly : PX.Data.BQL.BqlString.Constant<quarterly>
		{
			public quarterly() : base(Quarterly) { ;}
		}
		public class yearly : PX.Data.BQL.BqlString.Constant<yearly>
		{
			public yearly() : base(Yearly) { ;}
		}
		public class fiscalPeriod : PX.Data.BQL.BqlString.Constant<fiscalPeriod>
		{
			public fiscalPeriod() : base(FiscalPeriod) { ;}
		}
	}

	public static class LineDiscountTargetType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ExtendedPrice, SalesPrice },
				new string[] { Messages.ExtendedPrice, Messages.SalesPrice }) { ; }
		}
		public const string ExtendedPrice = "E";
		public const string SalesPrice = "S";
	}

	public static class ApplyQuantityDiscountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { DocumentLineUOM, BaseUOM },
				new string[] { Messages.DocumentLineUOM, Messages.BaseUOM })
			{; }
		}
		public const string DocumentLineUOM = "L";
		public const string BaseUOM = "B";
	}

	public static class RetentionTypeList
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { LastPrice, FixedNumOfMonths },
				new string[] { Messages.LastPrice, Messages.FixedNumberOfMonths }) { ; }
		}
		public const string LastPrice = "L";
		public const string FixedNumOfMonths = "F";

		public class lastPrice : PX.Data.BQL.BqlString.Constant<lastPrice>
		{
			public lastPrice() : base(RetentionTypeList.LastPrice) { ;}
		}

		public class fixedNumOfMonths : PX.Data.BQL.BqlString.Constant<fixedNumOfMonths>
		{
			public fixedNumOfMonths() : base(RetentionTypeList.FixedNumOfMonths) { ;}
		}
	}

	public class DunningProcessType
	{
		public const int ProcessByCustomer = 0;
		public const int ProcessByDocument = 1;
		public class ListAttribute : PXIntListAttribute
		{
			public ListAttribute() :
				base(new int[] { ProcessByCustomer, ProcessByDocument },
					new string[] { Messages.DunningProcessTypeCustomer, Messages.DunningProcessTypeDocument })
			{ }
		}
	}
}
