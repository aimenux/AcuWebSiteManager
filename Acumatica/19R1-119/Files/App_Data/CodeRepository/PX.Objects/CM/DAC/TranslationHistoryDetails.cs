namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;

	public class TranslationLineType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
			new string[] { Translation, GainLoss },
			new string[] { Messages.Translation, Messages.GainLoss }) { }
		}

		public const string Translation = "T";
		public const string GainLoss = "G";

		public class translation : PX.Data.BQL.BqlString.Constant<translation>
		{
			public translation() : base(Translation) { ;}
		}

		public class gainLoss : PX.Data.BQL.BqlString.Constant<gainLoss>
		{
			public gainLoss() : base(GainLoss) { ;}
		}
	}

	/// <summary>
	/// Represents the details of the currency translation history. The records of this type are created together with the header (<see cref="TranslationHistory"/>)
	/// by the currency translation process (<see cref="TranslationProcess"/>).
	/// The detail records store the results of currency translation for a particular account and subaccount, including the currency and rate information,
	/// link to the resulting <see cref="GLTran">GL transaction</see>, status, and amounts.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.TranslationHistoryDetails)]
	public partial class TranslationHistoryDetails : IBqlTable, GL.DAC.Abstract.IAccountable
	{
		#region ReferenceNbr
		public abstract class referenceNbr : PX.Data.BQL.BqlString.Field<referenceNbr> { }
		protected String _ReferenceNbr;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(TranslationHistory))]
		[PXParent(typeof(Select<TranslationHistory,Where<TranslationHistory.referenceNbr,Equal<Current<TranslationHistoryDetails.referenceNbr>>>>))]
		[PXUIField(DisplayName = "Translation Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String ReferenceNbr
		{
			get
			{
				return this._ReferenceNbr;
			}
			set
			{
				this._ReferenceNbr = value;
			}
		}
		#endregion
		#region TranslDefId
		public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
		protected String _TranslDefId;
		[PXDBString(10, IsUnicode = true)]
		[PXDBDefault(typeof(TranslationHistory.translDefId))]
		[PXSelector(typeof(TranslDef.translDefId))]
		public virtual String TranslDefId
		{
			get
			{
				return this._TranslDefId;
			}
			set
			{
				this._TranslDefId = value;
			}
		}
		#endregion
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt()]
		[PXDBDefault(typeof(TranslationHistory.ledgerID))]
		[PXSelector(typeof(Ledger.ledgerID), SubstituteKey = typeof(Ledger.ledgerCD))]
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
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true, IsDetail = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey= true, DescriptionField = typeof(Account.description))]
		[PXDefault()]
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
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount(IsKey = true)]
		[PXDefault()]
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
		#region CalcMode
		public abstract class calcMode : PX.Data.BQL.BqlShort.Field<calcMode> { }
		protected Int16? _CalcMode;
		[PXUIField(DisplayName = "Translation Method")]
		[PXDBShort()]
        [PXDefault((short)1)]
		[PXIntList("0;Gain or Loss,1;YTD Balance,2;PTD Balance")]
		public virtual Int16? CalcMode
		{
			get
			{
				return this._CalcMode;
			}
			set
			{
				this._CalcMode = value;
			}
		}
		#endregion
		#region SourceAmt
		public abstract class sourceAmt : PX.Data.BQL.BqlDecimal.Field<sourceAmt> { }
		protected Decimal? _SourceAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Source Amount", Required = false)]
		public virtual Decimal? SourceAmt
		{
			get
			{
				return this._SourceAmt;
			}
			set
			{
				this._SourceAmt = value;
			}
		}
		#endregion
		#region TranslatedAmt
		public abstract class translatedAmt : PX.Data.BQL.BqlDecimal.Field<translatedAmt> { }
		protected Decimal? _TranslatedAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Translated Amount", Required = false)]
		public virtual Decimal? TranslatedAmt
		{
			get
			{
				return this._TranslatedAmt;
			}
			set
			{
				this._TranslatedAmt = value;
			}
		}
		#endregion
		#region OrigTranslatedAmt
		public abstract class origTranslatedAmt : PX.Data.BQL.BqlDecimal.Field<origTranslatedAmt> { }
		protected Decimal? _OrigTranslatedAmt;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Orig. Translated Amount", Required = false)]
		public virtual Decimal? OrigTranslatedAmt
		{
			get
			{
				return this._OrigTranslatedAmt;
			}
			set
			{
				this._OrigTranslatedAmt = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(TranslationHistory.finPeriodID))]
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
		#region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        protected String _CuryID;
        [PXDBString(5, IsUnicode = true)]
        [PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault()]
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
		#region RateTypeID
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		protected String _RateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Rate Type")]
		public virtual String RateTypeID
		{
			get
			{
				return this._RateTypeID;
			}
			set
			{
				this._RateTypeID = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		protected String _CuryMultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName="Mult/Div")]
		[PXStringList("M;Multiply,D;Divide")]
		public virtual String CuryMultDiv
		{
			get
			{
				return this._CuryMultDiv;
			}
			set
			{
				this._CuryMultDiv = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;
		[PXDBDate()]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName="Currency Effective Date")]
		public virtual DateTime? CuryEffDate
		{
			get
			{
				return this._CuryEffDate;
			}
			set
			{
				this._CuryEffDate = value;
			}
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		protected Decimal? _CuryRate;
		[PXDBDecimal(8)]
		[PXDefault(TypeCode.Decimal,"1.0")]
		[PXUIField(DisplayName = "Currency Rate")]
		public virtual Decimal? CuryRate
		{
			get
			{
				return this._CuryRate;
			}
			set
			{
				this._CuryRate = value;
			}
		}
		#endregion
		#region LineType
		public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
		protected String _LineType;
		[PXDBString(1, IsFixed = true, IsKey=true)]
		[PXDefault()]
		[TranslationLineType.List()]
		[PXUIField(DisplayName = "Line Type", Enabled = false)]

		public virtual String LineType
		{
			get
			{
				return this._LineType;
			}
			set
			{
				this._LineType = value;
			}
		}
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		protected String _BatchNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Translation Batch Number", Visible = false)]
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
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt()]
		[PXDefault()]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public virtual Int32? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion
		#region DebitAmt
		public abstract class debitAmt : PX.Data.BQL.BqlDecimal.Field<debitAmt> { }
		protected Decimal? _DebitAmt;
		[PXDBBaseCury(typeof(TranslationHistoryDetails.ledgerID))]
		[PXDefault(TypeCode.Decimal,"0.0", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Transaction Debit Amount")]
		[PXFormula(null, typeof(SumCalc<TranslationHistory.debitTot>))]
		public virtual Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		public abstract class creditAmt : PX.Data.BQL.BqlDecimal.Field<creditAmt> { }
		protected Decimal? _CreditAmt;
		[PXDBBaseCury(typeof(TranslationHistoryDetails.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Transaction Credit Amount")]
		[PXFormula(null, typeof(SumCalc<TranslationHistory.creditTot>))]
		public virtual Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
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
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		protected Boolean? _Released;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Enabled = false, Visible = false)]
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
	}
}
