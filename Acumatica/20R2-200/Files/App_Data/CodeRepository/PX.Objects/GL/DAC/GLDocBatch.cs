using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.GL
{
	public class GLDocBatchStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Hold, Balanced, Completed, Voided, Released  },
			new string[] { Messages.Hold, Messages.Balanced, Messages.Completed, Messages.Voided, Messages.Released }) { }
		}

		public const string Hold = "H";
		public const string Balanced = "B";
		public const string Completed = "C";
		public const string Voided = "V";
		public const string Released = "R";
		
		

		public class hold : PX.Data.BQL.BqlString.Constant<hold>
		{
			public hold() : base(Hold) { ;}
		}

		public class balanced : PX.Data.BQL.BqlString.Constant<balanced>
		{
			public balanced() : base(Balanced) { ;}
		}
		
		public class completed : PX.Data.BQL.BqlString.Constant<completed>
		{
			public completed() : base(Completed) { ;}
		}

		public class voided : PX.Data.BQL.BqlString.Constant<voided>
		{
			public voided() : base(Voided) { ;}
		}
		
	}
	
    /// <summary>
    /// Represents a batch of documents edited through the Journal Vouchers screen (GL.30.40.00).
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.GLDocBatch)]
	[PXPrimaryGraph(typeof(JournalWithSubEntry))]
	public partial class GLDocBatch : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

        /// <summary>
        /// Indicates whether the record is selected for mass processing or not.
        /// </summary>
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

        /// <summary>
        /// Identifier of the <see cref="Branch"/>, to which the batch belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Branch.BranchID"/> field.
        /// </value>
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
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		protected String _Module;

        /// <summary>
        /// The code of the module, to which the batch belongs.
        /// </summary>
        /// <value>
        /// Defaults to "GL".
        /// Allowed values are:
        /// "GL", "AP", "AR", "CM", "CA", "IN", "DR", "FA", "PM", "TX", "SO", "PO".
        /// </value>
		[PXDBString(2, IsKey = true, IsFixed = true)]
		[PXDefault(GL.BatchModule.GL)]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.SelectorVisible)]
		[BatchModule.List()]
		[PXFieldDescription]
		public virtual String Module
		{
			get
			{
				return this._Module;
			}
			set
			{
				this._Module = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;

        /// <summary>
        /// Key field.
        /// Auto-generated unique number of the document batch.
        /// </summary>
        /// <value>
        /// The number is generated from the <see cref="Numbering">Numbering Sequence</see> specified in the
        /// <see cref="GLSetup.DocBatchNumberingID"/> field of the GL preferences record.
        /// </value>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXSelector(typeof(Search<GLDocBatch.batchNbr, Where<GLDocBatch.module, Equal<Current<GLDocBatch.module>>>, OrderBy<Desc<GLDocBatch.batchNbr>>>), Filterable = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.docBatchNumberingID),typeof(GLDocBatch.dateEntered))]
		[PXFieldDescription]
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
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;

        /// <summary>
        /// Identifier of the <see cref="Ledger"/>, to which the batch belongs.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Ledger.LedgerID"/> field.
        /// </value>
		[PXDBInt()]
		[PXDefault(typeof(Search<Branch.ledgerID, Where<Branch.branchID, Equal<Current<GLDocBatch.branchID>>>>))]
		[PXUIField(DisplayName = "Ledger", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		[PXSelector(typeof(Search2<Ledger.ledgerID, LeftJoin<Branch, On<Branch.ledgerID, Equal<Ledger.ledgerID>>>, Where<Ledger.balanceType, NotEqual<BudgetLedger>, And<Where<Ledger.balanceType, NotEqual<LedgerBalanceType.actual>, Or<Branch.branchID, Equal<Current<GLDocBatch.branchID>>>>>>>),
						SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region DateEntered
		public abstract class dateEntered : PX.Data.BQL.BqlDateTime.Field<dateEntered> { }
		protected DateTime? _DateEntered;

        /// <summary>
        /// The date of the batch, specified by user.
        /// </summary>
        /// <value>
        /// Defaults to the current <see cref="AccessInfo.BusinessDate">Business Date</see>.
        /// </value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? DateEntered
		{
			get
			{
				return this._DateEntered;
			}
			set
			{
				this._DateEntered = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;

        /// <summary>
        /// Identifier of the <see cref="OrganizationFinPeriod">Financial Period</see>, to which the batch belongs.
        /// </summary>
        /// <value>
        /// By default the period is deducted from the <see cref="DateEntered">date of the batch</see>.
        /// Can be overriden by user.
        /// </value>
		[OpenPeriod(null, typeof(GLDocBatch.dateEntered), typeof(branchID), 
			masterFinPeriodIDType: typeof(tranPeriodID),
			IsHeader = true)]
        [PXDefault()]
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
		#region BatchType
		public abstract class batchType : PX.Data.BQL.BqlString.Field<batchType> { }
		protected String _BatchType;

        /// <summary>
        /// The type of the batch.
        /// </summary>
        /// <value>
        /// Allowed values are: 
        /// <c>"H"</c> - Normal,
        /// <c>"R"</c> - Recurring,
        /// <c>"C"</c> - Consolidation,
        /// <c>"T"</c> - Trial Balance,
		/// <c>"RCL"</c> - Reclassification.
        /// Defaults to <c>"H"</c> - Normal.
        /// </value>
		[PXDBString(3)]
		[PXDefault(BatchTypeCode.Normal)]
		[PXUIField(DisplayName = "Type")]
		[BatchTypeCode.List()]
		public virtual String BatchType
		{
			get
			{
				return this._BatchType;
			}
			set
			{
				this._BatchType = value;
			}
		}
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		protected String _Status;

        /// <summary>
        /// Read-only field reflecting the status of the batch.
        /// </summary>
        /// <value>
        /// The value of the field can not be changed directly and depends on the status flags of the batch:
        /// <see cref="Posted"/>, <see cref="Voided"/>, <see cref="Released"/>, <see cref="Hold"/>.
        /// Possible values are:
        /// <c>"H"</c> - Hold,
        /// <c>"B"</c> - Balanced,
        /// <c>"C"</c> - Completed,
        /// <c>"V"</c> - Voided,
        /// <c>"R"</c> - Released.
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(GLDocBatchStatus.Hold)]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[GLDocBatchStatus.List()]
		public virtual String Status
		{
			[PXDependsOnFields(typeof(posted), typeof(voided), typeof(released), typeof(hold))]
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
		#region CuryDebitTotal
		public abstract class curyDebitTotal : PX.Data.BQL.BqlDecimal.Field<curyDebitTotal> { }
		protected Decimal? _CuryDebitTotal;

        /// <summary>
        /// The total debit amount of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// <value>
        /// See also <see cref="DebitTotal"/>.
        /// </value>
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.debitTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryDebitTotal
		{
			get
			{
				return this._CuryDebitTotal;
			}
			set
			{
				this._CuryDebitTotal = value;
			}
		}
		#endregion
		#region CuryCreditTotal
		public abstract class curyCreditTotal : PX.Data.BQL.BqlDecimal.Field<curyCreditTotal> { }
        protected Decimal? _CuryCreditTotal;

        /// <summary>
        /// The total credit amount of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// <value>
        /// See also <see cref="CreditTotal"/>.
        /// </value>
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.creditTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual Decimal? CuryCreditTotal
		{
			get
			{
				return this._CuryCreditTotal;
			}
			set
			{
				this._CuryCreditTotal = value;
			}
		}
		#endregion
		#region CuryControlTotal
		public abstract class curyControlTotal : PX.Data.BQL.BqlDecimal.Field<curyControlTotal> { }
        protected Decimal? _CuryControlTotal;

        /// <summary>
        /// The control total of the batch in its <see cref="CuryID">currency</see>.
        /// </summary>
        /// See also <see cref="ControlTotal"/>.
		[PXDBCurrency(typeof(GLDocBatch.curyInfoID), typeof(GLDocBatch.controlTotal))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? CuryControlTotal
		{
			get
			{
				return this._CuryControlTotal;
			}
			set
			{
				this._CuryControlTotal = value;
			}
		}
		#endregion
		#region DebitTotal
		public abstract class debitTotal : PX.Data.BQL.BqlDecimal.Field<debitTotal> { }
        protected Decimal? _DebitTotal;

        /// <summary>
        /// The total debit amount of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryDebitTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? DebitTotal
		{
			get
			{
				return this._DebitTotal;
			}
			set
			{
				this._DebitTotal = value;
			}
		}
		#endregion
		#region CreditTotal
		public abstract class creditTotal : PX.Data.BQL.BqlDecimal.Field<creditTotal> { }
        protected Decimal? _CreditTotal;

        /// <summary>
        /// The total credit amount of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryCreditTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? CreditTotal
		{
			get
			{
				return this._CreditTotal;
			}
			set
			{
				this._CreditTotal = value;
			}
		}
		#endregion
		#region ControlTotal
		public abstract class controlTotal : PX.Data.BQL.BqlDecimal.Field<controlTotal> { }
        protected Decimal? _ControlTotal;

        /// <summary>
        /// The control total of the batch in the <see cref="Company.BaseCuryID">base currency</see> of the company.
        /// </summary>
        /// <value>
        /// See also <see cref="CuryControlTotal"/>.
        /// </value>
		[PXDBBaseCury(typeof(GLDocBatch.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Control Total")]
		public virtual Decimal? ControlTotal
		{
			get
			{
				return this._ControlTotal;
			}
			set
			{
				this._ControlTotal = value;
			}
		}
		#endregion
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        protected Int64? _CuryInfoID;

        /// <summary>
        /// Identifier of the <see cref="PX.Objects.CM.CurrencyInfo">CurrencyInfo</see> record associated with the batch.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PCurrencyInfo.CurrencyInfoID"/> field.
        /// </value>
		[PXDBLong()]
		[PX.Objects.CM.CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		
		#region OrigModule
		public abstract class origModule : PX.Data.BQL.BqlString.Field<origModule> { }
        protected String _OrigModule;

        /// <summary>
        /// The code of the module, from which the batch originates.
        /// </summary>
		[PXDBString(2, IsFixed = true)]
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
		#region OrigBatchNbr
		public abstract class origBatchNbr : PX.Data.BQL.BqlString.Field<origBatchNbr> { }
		protected String _OrigBatchNbr;

        /// <summary>
        /// The number of the batch, from which this batch originates.
        /// </summary>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Orig. Batch Number", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual String OrigBatchNbr
		{
			get
			{
				return this._OrigBatchNbr;
			}
			set
			{
				this._OrigBatchNbr = value;
			}
		}
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;

        /// <summary>
        /// Indicates whether the batch has been released.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Released
		{
			get
			{
				return this._Released;
			}
			set
			{
				this._Released = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Posted
		public abstract class posted : PX.Data.BQL.BqlBool.Field<posted> { }
        protected Boolean? _Posted;

        /// <summary>
        /// Indicates whether the batch has been posted.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Posted
		{
			get
			{
				return this._Posted;
			}
			set
			{
				this._Posted = value;
				this.SetStatus();
			}
		}
		#endregion
		#region TranPeriodID
		public abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }
        protected String _TranPeriodID;

		/// <summary>
		/// Identifier of the <see cref="OrganizationFinPeriod">Financial Period</see> of the batch.
		/// </summary>
		/// <value>
		/// Determined by the <see cref="DateEntered">date of the batch</see>. Unlike the <see cref="FinPeriodID"/> field,
		/// the value of this field can't be overriden by user and always reflects the period corresponding to the date of the batch.
		/// </value>
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
		#region LineCntr
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
        protected Int32? _LineCntr;

        /// <summary>
        /// The counter of the document lines, used <i>internally</i> to assign consistent numbers to newly created lines.
        /// It is not recommended to rely on this field to determine the exact count of lines, because it might not reflect the latter under some conditions.
        /// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected String _CuryID;

        /// <summary>
        /// The code of the <see cref="Currency">Currency</see> of the batch.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see>.
        /// </value>
		[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(typeof(Search<Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion		
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXNote(DescriptionField = typeof(GLDocBatch.batchNbr))]
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
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;

        /// <summary>
        /// Indicates whether the batch is on hold.
        /// </summary>
        /// <value>
        /// Defaults to <c>true</c>, if the <see cref="GLSetup.VouchersHoldEntry"/> flag is set in the preferences of the module,
        /// and to <c>false</c> otherwise.
        /// </value>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
        [PXDefault(true,typeof(Search<GLSetup.vouchersHoldEntry>))]
		public virtual Boolean? Hold
		{
			get
			{
				return this._Hold;
			}
			set
			{
				this._Hold = value;
				this.SetStatus();
			}
		}
		#endregion		
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
        protected Boolean? _Voided;

        /// <summary>
        /// Indicates whether the batch has been voided.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		public virtual Boolean? Voided
		{
			get
			{
				return this._Voided;
			}
			set
			{
				this._Voided = value;
				this.SetStatus();
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The description of the batch.
        /// </summary>
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible)]
		[PXDBString(255, IsUnicode = true)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region Methods
		protected virtual void SetStatus()
		{
			if (this._Voided != null && (bool)this._Voided)
			{
				this._Status = GLDocBatchStatus.Voided;
			}
			else if (this._Hold != null && (bool)this._Hold)
			{
				this._Status = GLDocBatchStatus.Hold;
			}			
			else if (this._Released != null && (bool)this._Released == false)
			{
				this._Status = GLDocBatchStatus.Balanced;
			}
			else if (this._Released != null && (bool)this._Released ==true)
			{
				this._Status = GLDocBatchStatus.Released;
			}			
		}
		#endregion
	}
}
