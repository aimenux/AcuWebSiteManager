using System;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.TM;


namespace PX.Objects.CA
{
    /// <summary>
    /// Contains the main properties of batch payments and their classes.
    /// Batch payments are edited on the Batch Payments (AP305000) form (which corresponds to the <see cref="CABatchEntry"/> graph).
    /// </summary>
    [PXPrimaryGraph(typeof(CABatchEntry))]
	[PXCacheName(Messages.CABatch)]
    [Serializable]
	public partial class CABatch : IBqlTable
	{
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }

        /// <summary>
        /// The user-friendly unique identifier of the batch number.
        /// This field is the key field.
        /// </summary>
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[CABatchType.Numbering]
		[CABatchType.RefNbr(typeof(Search<CABatch.batchNbr, Where<CABatch.origModule, Equal<GL.BatchModule.moduleAP>>>))]
		public virtual string BatchNbr
		{
			get;
	        set;
		}
		#endregion
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }

		/// <summary>
		/// Module from which the document originates.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(GL.BatchModule.AP)]
		[PXStringList(new string[] { GL.BatchModule.AP, GL.BatchModule.AR, GL.BatchModule.PR }, new string[] { BatchModule.AP, BatchModule.AR, BatchModule.PR })]
		[PXUIField(DisplayName = "Module", Enabled = false)]
		public virtual string OrigModule
		{
			get;
			set;
		}
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }

        /// <summary>
        /// The cash account used for payment.
        /// Corresponds to the <see cref="CashAccount.CashAccountID"/> field.
        /// </summary>
		[CashAccount(null, typeof(Search<CashAccount.cashAccountID, Where2<Match<Current<AccessInfo.userName>>, And<CashAccount.clearingAccount, Equal<CS.boolFalse>>>>), DisplayName = "Cash Account", Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(CashAccount.descr))]
		public virtual int? CashAccountID
		{
			get;
            set;
        }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

        /// <summary>
        /// The payment method associated with the cash account. Only payment methods that allow batch creation appear in the list.
        /// Payment methods are defined on the Payment Methods (CA204000) form.
        /// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.aPCreateBatchPayment, Equal<True>>>))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region ReferenceID
		public abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

        /// <summary>
        /// The unique identifier of the bank that will process the batch of payments.
        /// </summary>
		[PXDBInt]
		[PXDefault(typeof(Search<CashAccount.referenceID, Where<CashAccount.cashAccountID, Equal<Current<CABatch.cashAccountID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(BAccount.bAccountID),
						SubstituteKey = typeof(BAccount.acctCD),
					 DescriptionField = typeof(BAccount.acctName))]
		[PXUIField(DisplayName = "Bank", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? ReferenceID
		{
			get;
	        set;
		}
		#endregion
		#region BatchSeqNbr
		public abstract class batchSeqNbr : PX.Data.BQL.BqlString.Field<batchSeqNbr> { }

        /// <summary>
        /// The unique number automatically assigned to the batch.
        /// </summary>
		[PXDBString(15, IsUnicode = true)]		
        [AP.BatchRef(typeof(CABatch.cashAccountID), typeof(CABatch.paymentMethodID))]
		[PXUIField(DisplayName = "Batch Seq. Number", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string BatchSeqNbr
		{
			get;
			set;
		}
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }

        /// <summary>
        /// Any document that holds information about the batch as required by your company's internal policies.
        /// </summary>
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Document Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ExtRefNbr
		{
			get;
			set;
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        /// <summary>
        /// The date when the batch was created.
        /// </summary>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Batch Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion
		#region TranDesc
		public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        /// <summary>
        /// A description of the batch, which may help to identify it.
        /// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TranDesc
		{
			get;
			set;
		}
		#endregion
		#region DateSeqNbr
		public abstract class dateSeqNbr : PX.Data.BQL.BqlShort.Field<dateSeqNbr> { }

        /// <summary>
        /// The unique number automatically assigned to the batch to distinguish it from other batches generated during the same day.
        /// </summary>
		[PXDBShort]
		[PXDefault((short)0)]
        [PXUIField(DisplayName = "Seq. Number Within Day", Enabled = false)]
		public virtual short? DateSeqNbr
		{
			get;
			set;
		}
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the batch is on hold and cannot be exported.
        /// </summary>
		[PXDBBool]
		[PXDefault(typeof(Search<CASetup.holdEntry>))]
		[PXUIField(DisplayName = "Hold")]
		public virtual bool? Hold
		{
			get;
			set;
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the batch is released.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = true)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		/// <summary>
		/// The status of the batch, which the system assigns automatically.
		/// This is a virtual field and it has no representation in the database.
		/// A batch can have one of the following statuses: 
		/// <c>"H"</c>: On Hold,
		/// <c>"B"</c>: Balanced,
		/// <c>"R"</c>: Released.
		/// </summary>
		[PXString(1, IsFixed = true)]
		[PXDefault(CABatchStatus.Balanced, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[CABatchStatus.List]
		public virtual string Status
		{
			[PXDependsOnFields(typeof(hold), typeof(released))]
			get
			{
				if (Hold.HasValue && Hold == true)
				{
					return CABatchStatus.Hold;
				}
				if (Released.HasValue && Released == true)
				{
                    return CABatchStatus.Released;
				}
                return CABatchStatus.Balanced;
			}

			set
			{
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        /// <summary>
        /// The currency of the payment.
        /// Corresponds to the currency of the cash account.
        /// Depends on the <see cref="CABatch.CashAccountID"/> field.
        /// </summary>
        [PXString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXDBScalar(typeof(Search<CashAccount.curyID, Where<CashAccount.cashAccountID, Equal<CABatch.cashAccountID>>>))]
        [PXUIField(DisplayName = "Currency")]
		public virtual string CuryID
		{
			get;
			set;
		}
		#endregion
		#region CuryDetailTotal
		public abstract class curyDetailTotal : PX.Data.BQL.BqlDecimal.Field<curyDetailTotal> { }

        /// <summary>
        /// The total amount for the batch, calculated as the sum of all payment amounts in the selected currency.
        /// </summary>
		[PXDBCury(typeof(CABatch.curyID))]
		[PXUIField(DisplayName = "Batch Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? CuryDetailTotal
		{
			get;
			set;
		}
		#endregion
		#region DetailTotal
		public abstract class detailTotal : PX.Data.BQL.BqlDecimal.Field<detailTotal> { }

        /// <summary>
        /// The total amount for the batch, calculated as the sum of all payment amounts in the base currency.
        /// </summary>
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DetailTotal
		{
			get;
			set;
		}
		#endregion
		#region Cleared
		public abstract class cleared : PX.Data.BQL.BqlBool.Field<cleared> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document was cleared with the reconciliation source, generally based on preliminary information.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cleared", Visible = false)]
		public virtual bool? Cleared
		{
			get;
			set;
		}
		#endregion
		#region ClearDate
		public abstract class clearDate : PX.Data.BQL.BqlDateTime.Field<clearDate> { }

        /// <summary>
        /// The date when the document was cleared.
        /// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Clear Date", Visible = false)]
		public virtual DateTime? ClearDate
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.AP, Messages.SearchTitleBatchPayment, new Type[] { typeof(CABatch.batchNbr), typeof(CABatch.referenceID), typeof(BAccount.acctName) },
			new Type[] { typeof(CABatch.paymentMethodID), typeof(CABatch.batchSeqNbr), typeof(CABatch.tranDesc), typeof(CABatch.extRefNbr), typeof(BAccount.acctCD) },
			NumberFields = new Type[] { typeof(CABatch.batchNbr) },
			Line1Format = "{0}{1:d}{2}", Line1Fields = new Type[] { typeof(CABatch.extRefNbr), typeof(CABatch.tranDate), typeof(CABatch.batchSeqNbr) },
			Line2Format = "{0}", Line2Fields = new Type[] { typeof(CABatch.tranDesc) }
		)]
		[PXNote(new Type[0])]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region ExportFileName
		public abstract class exportFileName : PX.Data.BQL.BqlString.Field<exportFileName> { }

        /// <summary>
        /// The name of the exported file.
        /// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Exported File Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual string ExportFileName
		{
			get;
			set;
		}
		#endregion
		#region ExportTime
		public abstract class exportTime : PX.Data.BQL.BqlDateTime.Field<exportTime> { }

        /// <summary>
        /// The time when the export was performed.
        /// </summary>
		[PXDBDate(PreserveTime = true)]
		[PXUIField(DisplayName = "File Export Time", Enabled = false)]
		public virtual DateTime? ExportTime
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion	
		#region DetailTotal
		public abstract class total : PX.Data.BQL.BqlDecimal.Field<total> { }

		[PXDecimal(4)]		
		public virtual decimal? Total
		{
			get;
			set;
		}
		#endregion
		#region Reconciled
		public abstract class reconciled : PX.Data.BQL.BqlBool.Field<reconciled> { }

        /// <summary>
        /// Specifies (if set to <c>true</c>) that the document is included in the reconciliation statement as a reconciled document.
        /// This field can be set to <c>true</c> only for batches for the cash accounts that have <see cref="CashAccount.MatchToBatch"/> set to true.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Reconciled
		{
			get;
			set;
		}
		#endregion
		#region ReconDate
		public abstract class reconDate : PX.Data.BQL.BqlDateTime.Field<reconDate> { }

        /// <summary>
        /// The date when the document was reconciled.
        /// </summary>
		[PXDBDate]
		public virtual DateTime? ReconDate
		{
			get;
			set;
		}
		#endregion
		#region ReconNbr
		public abstract class reconNbr : PX.Data.BQL.BqlString.Field<reconNbr> { }

        /// <summary>
        /// If the document was reconciled, the field contains the number of the reconciliation that includes this document.
        /// </summary>
		[PXDBString(15, IsUnicode = true)]
		public virtual string ReconNbr
		{
			get;
			set;
		}
		#endregion

		public CATran CopyTo(CATran destination)
		{
			destination.TranDate = this.TranDate;
			destination.OrigModule = BatchModule.AP;
			destination.OrigRefNbr = this.BatchNbr;
			destination.OrigTranType = CATranType.CABatch;
			destination.ExtRefNbr = this.ExtRefNbr;
			destination.TranDesc = this.TranDesc;
			destination.TranAmt = -this.DetailTotal;
			destination.CuryTranAmt = -this.CuryDetailTotal;
			destination.Released = this.Released;
			destination.Hold = this.Hold;
			destination.Status = this.Status;
			destination.Cleared = this.Cleared;
			destination.ClearDate = this.ClearDate;
			destination.ReconNbr = this.ReconNbr;
			destination.ReconDate = this.ReconDate;
			destination.Reconciled = this.Reconciled;
			destination.DrCr = DrCr.Credit;
			destination.TranDesc = this.TranDesc;
			destination.ExtRefNbr = this.ExtRefNbr;
			destination.CashAccountID = this.CashAccountID;
			destination.CuryID = this.CuryID;
			return destination;
		}
	}

	public class CABatchType
	{
        /// <summary>
        /// Specialized selector for CABatch RefNbr.<br/>
        /// By default, defines the following set of columns for the selector:<br/>
        /// CABatch.batchNbr, CABatch.tranDate, CABatch.cashAccountID,
		///	CABatch.paymentMethodID, CABatch.curyDetailTotal, CABatch.extRefNbr
        /// <example>
        /// [CABatchType.RefNbr(typeof(Search/<CABatch.batchNbr/>))]
        /// </example>
        /// </summary>
		public class RefNbrAttribute : PXSelectorAttribute
		{
			public RefNbrAttribute(Type searchType)
				: base(searchType,
				typeof(CABatch.batchNbr),
				typeof(CABatch.tranDate),
				typeof(CABatch.cashAccountID),
				typeof(CABatch.paymentMethodID),
				typeof(CABatch.curyDetailTotal),
				typeof(CABatch.extRefNbr)) {}
		}

        /// <summary>
        /// Specialized for CABatch version of the <see cref="AutoNumberAttribute"/><br/>
        /// It defines how the new numbers are generated for the AR Invoice. <br/>
        /// References CABatch.docType and CABatch.tranDate fields of the document,<br/>
        /// and also define a link between  numbering ID's defined in CASetup (namely CASetup.cABatchNumberingID)<br/>
        /// and CABatch: <br/>
        /// </summary>
		public class NumberingAttribute : CS.AutoNumberAttribute
		{
			public NumberingAttribute()
				: base(typeof(CASetup.cABatchNumberingID), typeof(CABatch.tranDate)) { }
		}
	}

	public class CABatchStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new string[] { Balanced, Hold, Released, Exported },
				new string[] { Messages.Balanced, Messages.Hold, Messages.Released, Messages.Exported }) { }
		}

		public const string Balanced = "B";
		public const string Hold = "H";
		public const string Released = "R";
		public const string Exported = "P";

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { }
		}

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { }
		}

		public class released : PX.Data.BQL.BqlString.Constant<released>
		{
			public released() : base(Released) { }
		}

		public class exported : PX.Data.BQL.BqlString.Constant<exported>
		{
			public exported() : base(Exported) { }
		}
	}
}
