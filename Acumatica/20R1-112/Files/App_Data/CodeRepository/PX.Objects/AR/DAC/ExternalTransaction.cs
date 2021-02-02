using System;
using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Interfaces;
using PX.Objects.SO;
using PX.Objects.CS;

namespace PX.Objects.AR
{
	[PXCacheName("External Transaction")]
	[PXPrimaryGraph(typeof(ExternalTransactionMaint))]
	public class ExternalTransaction : PX.Data.IBqlTable, IExternalTransaction
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion

		#region TransactionID
		public abstract class transactionID : PX.Data.BQL.BqlInt.Field<transactionID> { }
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Ext. Tran. ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<ExternalTransaction.transactionID>),
							typeof(ExternalTransaction.transactionID),
							typeof(ExternalTransaction.tranNumber), 
							typeof(ExternalTransaction.authNumber),
							typeof(ExternalTransaction.amount),
							typeof(ExternalTransaction.lastActivityDate), 
							typeof(ExternalTransaction.processingStatus),
							typeof(ExternalTransaction.docType),
							typeof(ExternalTransaction.refNbr))]
		public virtual int? TransactionID { get;set; }
		#endregion

		#region PMInstanceID
		public abstract class pMInstanceID : PX.Data.BQL.BqlInt.Field<pMInstanceID> { }
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Card/Account No")]
		[PXSelector(typeof(Search<CustomerPaymentMethod.pMInstanceID,
			Where<CustomerPaymentMethod.isActive, Equal<boolTrue>>>), DescriptionField = typeof(CustomerPaymentMethod.descr), ValidateValue = false)]
		public virtual int? PMInstanceID { get; set; }
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Doc. Type", Visibility = PXUIVisibility.SelectorVisible)]
		[ARDocType.List()]
		public virtual string DocType { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Doc. Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<ARRegister.refNbr, Where<ARRegister.docType, Equal<Optional<ExternalTransaction.docType>>>>))]
		public virtual string RefNbr { get; set; }
		#endregion

		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Orig. Doc. Type")]
		[PXSelector(typeof(Search4<SOOrderType.orderType, Aggregate<GroupBy<SOOrderType.orderType>>>), DescriptionField = typeof(SOOrderType.descr))]
		public virtual string OrigDocType { get; set; }
		#endregion

		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Doc. Ref. Nbr.")]
		[PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Optional<ExternalTransaction.origDocType>>>>))]
		public virtual string OrigRefNbr { get; set; }
		#endregion

		#region TranNumber
		public abstract class tranNumber : PX.Data.BQL.BqlString.Field<tranNumber> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Tran. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TranNumber { get; set; }
		#endregion

		#region AuthNumber
		public abstract class authNumber : PX.Data.BQL.BqlString.Field<authNumber> { }
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Proc. Center Auth. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string AuthNumber { get; set; }
		#endregion

		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tran. Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? Amount { get; set; }
		#endregion

		#region ProcessingStatus
		public abstract class processingStatus : PX.Data.BQL.BqlString.Field<processingStatus> { }
		[PXDBString(3, IsFixed = true)]
		[ExtTransactionProcStatusCode.List()]
		[PXUIField(DisplayName = "Proc. Status", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ProcessingStatus { get; set; }
		#endregion

		#region LastActivityDate
		public abstract class lastActivityDate : PX.Data.BQL.BqlDateTime.Field<lastActivityDate> { }
		[PXDBDate(PreserveTime = true, DisplayMask = "d")]
		[PXUIField(DisplayName = "Last Activity Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? LastActivityDate { get; set; }
		#endregion

		#region Direction
		public abstract class direction : PX.Data.BQL.BqlString.Field<direction> { }
		[PXDBString(1, IsFixed = true)]
		public virtual string Direction { get; set; }
		#endregion

		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? Active { get; set; }
		#endregion

		#region Completed
		public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Completed")]
		public virtual bool? Completed { get; set; }
		#endregion

		#region ParentTranID
		public abstract class parentTranID : PX.Data.BQL.BqlInt.Field<parentTranID> { }
		[PXDBInt]
		public virtual int? ParentTranID { get; set; }
		#endregion

		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		[PXDBDate(PreserveTime = true, DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date")]
		public virtual DateTime? ExpirationDate { get; set; }
		#endregion

		#region CVVVerification
		public abstract class cVVVerification : PX.Data.BQL.BqlString.Field<cVVVerification> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(CVVVerificationStatusCode.RequiredButNotVerified)]
		[CVVVerificationStatusCode.List()]
		[PXUIField(DisplayName = "CVV Verification")]
		public virtual string CVVVerification { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual byte[] tstamp { get; set; }
		#endregion
		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }


		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote()]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		public static class TransactionDirection
		{
			public const string Debet = "D";
			public const string Credit = "C";
		}
	}
}
