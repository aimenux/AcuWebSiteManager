using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CM
{
	/// <summary>
	/// Stores the settings of the Currency Management module.
	/// The system holds one record of this type per company. The record is edited on the
	/// Currency Management Preferences (CM101000) form, which corresponds to the <see cref="CMSetupMaint"/> graph.
	/// </summary>
    [PXPrimaryGraph(typeof(CMSetupMaint))]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CMSetup)]
	public partial class CMSetup : PX.Data.IBqlTable
	{
		#region BatchNumberingID
		public abstract class batchNumberingID : PX.Data.BQL.BqlString.Field<batchNumberingID> { }
		protected String _BatchNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used to assign <see cref="Btach.BatchNbr">Btach Numbers</see>
        /// to the batches originating from the Currency Mangement module.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault("BATCH")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Batch Numbering Sequence", Visibility = PXUIVisibility.Visible)]
		public virtual String BatchNumberingID
		{
			get
			{
				return _BatchNumberingID;
			}
			set
			{
				_BatchNumberingID = value;
			}
		}
		#endregion
        #region ExtRefNbrNumberingID
        public abstract class extRefNbrNumberingID : PX.Data.BQL.BqlString.Field<extRefNbrNumberingID> { }
        protected String _ExtRefNbrNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used to assign
        /// <see cref="GLTran.RefNbr">reference numbers</see> to the transactions generated during the process
        /// of revaluation of AP and AR accounts.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Batch Ref. Number Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String ExtRefNbrNumberingID
        {
            get
            {
                return _ExtRefNbrNumberingID;
            }
            set
            {
                _ExtRefNbrNumberingID = value;
            }
        }
        #endregion
		#region APCuryOverride
		public abstract class aPCuryOverride : PX.Data.BQL.BqlBool.Field<aPCuryOverride> { }
		protected Boolean? _APCuryOverride;

        /// <summary>
        /// When set to <c>true</c>, indicates that the system will allow to enter AP documents in the currency different from
        /// the <see cref="Vendor.CuryID">currency of the vendor</see>.
        /// The value of this field is used as a default for the <see cref="VendorClass.AllowOverrideCury"/> field of the
        /// <see cref="APSetup.DfltVendorClassID">default Vendor Class</see>. Its value in turn affects the values of the same fied of
        /// other vendor classes.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Vendor CurrencyID Override")]
		public virtual Boolean? APCuryOverride
		{
			get
			{
				return this._APCuryOverride;
			}
			set
			{
				this._APCuryOverride = value;
			}
		}
		#endregion
		#region APRateTypeDflt
		public abstract class aPRateTypeDflt : PX.Data.BQL.BqlString.Field<aPRateTypeDflt> { }
		protected String _APRateTypeDflt;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for Accounts Payable documents by default.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AP Rate Type")]
		public virtual String APRateTypeDflt
		{
			get
			{
				return this._APRateTypeDflt;
			}
			set
			{
				this._APRateTypeDflt = value;
			}
		}
		#endregion
		#region APRateTypeOverride
		public abstract class aPRateTypeOverride : PX.Data.BQL.BqlBool.Field<aPRateTypeOverride> { }
		protected Boolean? _APRateTypeOverride;

        /// <summary>
        /// When set to <c>true</c>, indicates that for AP documents the system allows to specify the rate type different from the <see cref="APRateTypeDflt"/>.
        /// Similarly to the <see cref="APCuryOverride"/> field, this field affects actual vendors and documents through the <see cref="VendorClass.AllowOverrideRate"/> field of
        /// the <see cref="APSetup.DfltVendorClass"/>.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Vendor Rate Type Override")]
		public virtual Boolean? APRateTypeOverride
		{
			get
			{
				return this._APRateTypeOverride;
			}
			set
			{
				this._APRateTypeOverride = value;
			}
		}
		#endregion
		#region APRateTypeReval
		public abstract class aPRateTypeReval : PX.Data.BQL.BqlString.Field<aPRateTypeReval> { }
        protected String _APRateTypeReval;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for revaluation of Accounts Payable.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AP Revaluation Rate Type")]
		public virtual String APRateTypeReval
		{
			get
			{
				return this._APRateTypeReval;
			}
			set
			{
				this._APRateTypeReval = value;
			}
		}
		#endregion
		#region ARCuryOverride
		public abstract class aRCuryOverride : PX.Data.BQL.BqlBool.Field<aRCuryOverride> { }
        protected Boolean? _ARCuryOverride;

        /// <summary>
        /// When set to <c>true</c>, indicates that the system will allow to enter AR documents in the currency different from
        /// the <see cref="Customer.CuryID">currency of the customer</see>.
        /// The value of this field is used as a default for the <see cref="CustomerClass.AllowOverrideCury"/> field of the
        /// <see cref="ARSetup.DfltCustomerClassID">default Customer Class</see>. Its value in turn affects the values of the same fied of
        /// other customer classes.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Customer Currency ID Override")]
		public virtual Boolean? ARCuryOverride
		{
			get
			{
				return this._ARCuryOverride;
			}
			set
			{
				this._ARCuryOverride = value;
			}
		}
		#endregion
		#region ARRateTypeDflt
		public abstract class aRRateTypeDflt : PX.Data.BQL.BqlString.Field<aRRateTypeDflt> { }
        protected String _ARRateTypeDflt;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for Accounts Receivable documents by default.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AR Rate Type")]
		public virtual String ARRateTypeDflt
		{
			get
			{
				return this._ARRateTypeDflt;
			}
			set
			{
				this._ARRateTypeDflt = value;
			}
		}
		#endregion
		#region ARRateTypePrc
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class aRRateTypePrc : PX.Data.BQL.BqlString.Field<aRRateTypePrc> { }

        /// <summary>
        /// Obsolete field.
        /// Replace by the <see cref="AR.ARSetup.DefaultRateTypeID">ARSetup.DefaultRateTypeID</see> in the <see cref="AR.ARSetup">AR Preferences</see>.
        /// </summary>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Sales Price Rate Type ")]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual string ARRateTypePrc
		{
			get;
			set;
		}
		#endregion
		#region ARRateTypeOverride
		public abstract class aRRateTypeOverride : PX.Data.BQL.BqlBool.Field<aRRateTypeOverride> { }
        protected Boolean? _ARRateTypeOverride;

        /// <summary>
        /// When set to <c>true</c>, indicates that for AR documents the system allows to specify the rate type different from the <see cref="ARRateTypeDflt"/>.
        /// Similarly to the <see cref="ARCuryOverride"/>, this field affects actual customers and documents through the <see cref="CustomerClass.AllowOverrideRate"/> field of
        /// the <see cref="ARSetup.DfltCustomerClass"/>.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Customer Rate Type Override")]
		public virtual Boolean? ARRateTypeOverride
		{
			get
			{
				return this._ARRateTypeOverride;
			}
			set
			{
				this._ARRateTypeOverride = value;
			}
		}
		#endregion
		#region ARRateTypeReval
		public abstract class aRRateTypeReval : PX.Data.BQL.BqlString.Field<aRRateTypeReval> { }
		protected String _ARRateTypeReval;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for revaluation of Accounts Receivable.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
        [PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "AR Revaluation Rate Type")]
		public virtual String ARRateTypeReval
		{
			get
			{
				return this._ARRateTypeReval;
			}
			set
			{
				this._ARRateTypeReval = value;
			}
		}
		#endregion
		
		#region CARateTypeDflt
		public abstract class cARateTypeDflt : PX.Data.BQL.BqlString.Field<cARateTypeDflt> { }
        protected String _CARateTypeDflt;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for Cash Management documents by default.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "CA Rate Type")]
		public virtual String CARateTypeDflt
		{
			get
			{
				return this._CARateTypeDflt;
			}
			set
			{
				this._CARateTypeDflt = value;
			}
		}
		#endregion
		
		#region GLRateTypeDflt
		public abstract class gLRateTypeDflt : PX.Data.BQL.BqlString.Field<gLRateTypeDflt> { }
        protected String _GLRateTypeDflt;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used for General Ledger transactions and for translations by default.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "GL Rate Type")]
		public virtual String GLRateTypeDflt
		{
			get
			{
				return this._GLRateTypeDflt;
			}
			set
			{
				this._GLRateTypeDflt = value;
			}
		}
		#endregion
		#region GLRateTypeReval
		public abstract class gLRateTypeReval : PX.Data.BQL.BqlString.Field<gLRateTypeReval> { }
        protected String _GLRateTypeReval;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Rate Type</see> used by default for revaluations performed for currency denominated accounts.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "GL Revaluation Rate Type")]
		public virtual String GLRateTypeReval
		{
			get
			{
				return this._GLRateTypeReval;
			}
			set
			{
				this._GLRateTypeReval = value;
			}
		}
		#endregion

		#region PMRateTypeDflt
		public abstract class pMRateTypeDflt : PX.Data.BQL.BqlString.Field<pMRateTypeDflt> { }
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(typeof(Search<CurrencyRateType.curyRateTypeID>))]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "PM Rate Type")]
		public virtual string PMRateTypeDflt
		{
			get;
			set;
		}
		#endregion

		#region RateVariance
		public abstract class rateVariance : PX.Data.BQL.BqlDecimal.Field<rateVariance> { }
		protected Decimal? _RateVariance;

        /// <summary>
        /// The maximum rate variance allowed for manually entered exchange rates expressed as a percent.
        /// If the currency rate entered by user differs from the applicable currency rate stored in the system by more than
        /// the specified percent of the latter, and the <see cref="RateVarianceWarn"/> field is set to <c>true</c>
        /// the system will display a warning.
        /// </summary>
        /// <value>
        /// Defaults to <c>0.0</c>, which means that the checks for rate variance are disabled.
        /// </value>
		[PXDBDecimal(0)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Rate Variance Allowed, %")]
		public virtual Decimal? RateVariance
		{
			get
			{
				return this._RateVariance;
			}
			set
			{
				this._RateVariance = value;
			}
		}
		#endregion
		#region RateVarianceWarn
		public abstract class rateVarianceWarn : PX.Data.BQL.BqlBool.Field<rateVarianceWarn> { }
		protected Boolean? _RateVarianceWarn;

        /// <summary>
        /// When set to <c>true</c> indicates that the system should display warnings about exceeding
        /// the <see cref="RateVariance">allowed rate variance</see> for manually entered currency rates.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Warn About Rate Variance")]
		public virtual Boolean? RateVarianceWarn
		{
			get
			{
				return this._RateVarianceWarn;
			}
			set
			{
				this._RateVarianceWarn = value;
			}
		}
		#endregion
		#region AutoPostOption
		public abstract class autoPostOption : PX.Data.BQL.BqlBool.Field<autoPostOption> { }
		protected Boolean? _AutoPostOption;

        /// <summary>
        /// When set to <c>true</c>, indicates that the batches originating from the Currency Management module
        /// will be automatically posted to the General Ledger on release.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Automatically Post to GL on Release", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? AutoPostOption
		{
			get
			{
				return this._AutoPostOption;
			}
			set
			{
				this._AutoPostOption = value;
			}
		}
		#endregion

		#region Translation Setup
        #region TranslDefId
        public abstract class translDefId : PX.Data.BQL.BqlString.Field<translDefId> { }
        protected String _TranslDefId;

        /// <summary>
        /// The identifier of the default <see cref="TransDef">type of translations</see> performed in the system.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="TranslDef.TranslDefId"/> field.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Default Translation ID")]
        [PXSelector(typeof(TranslDef.translDefId),
           DescriptionField = typeof(TranslDef.description))]
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
        #region RetainPeriodsNumber
        public abstract class retainPeriodsNumber : PX.Data.BQL.BqlShort.Field<retainPeriodsNumber> { }
        protected Int16? _RetainPeriodsNumber;

        /// <summary>
        /// Defines for how many periods the translation records should be stored in the system.
        /// </summary>
        /// <value>
        /// Defaults to <c>99</c>.
        /// </value>
        [PXDBShort()]
		[PXDefault((short)99)]
        [PXUIField(DisplayName = "Keep History For Periods")]
        public virtual Int16? RetainPeriodsNumber
        {
            get
            {
                return this._RetainPeriodsNumber;
            }
            set
            {
                this._RetainPeriodsNumber = value;
            }
        }
        #endregion
        #region TranslNumberingID
        public abstract class translNumberingID : PX.Data.BQL.BqlString.Field<translNumberingID> { }
        protected String _TranslNumberingID;

        /// <summary>
        /// The identifier of the <see cref="Numbering">Numbering Sequence</see> used to assign the
        /// <see cref="TranslationHistory.ReferenceNbr">reference numbers</see> to the
        /// <see cref="TranslationHistory">Translation History</see> records.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Numbering.NumberingID"/> field.
        /// Defaults to <c>"TRANSLAT"</c>.
        /// </value>
        [PXDBString(10, IsUnicode = true)]
    	[PXDefault("TRANSLAT")]
        [PXUIField(DisplayName = "Translation Numbering Sequence")]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        public virtual String TranslNumberingID
        {
            get
            {
                return this._TranslNumberingID;
            }
            set
            {
                this._TranslNumberingID = value;
            }
        }
        #endregion
		#endregion
        #region GainLossSubMask
        public abstract class gainLossSubMask : PX.Data.BQL.BqlString.Field<gainLossSubMask> { }
        protected String _GainLossSubMask;

        /// <summary>
        /// The subaccount mask that defines the rules of composing subaccounts for recording realized and unrealized gains and losses
        /// resulting from rounding and translations.
        /// </summary>
        /// <value>
        /// For the rules related to setting the value of this field see the documentation for the Currency Management Preferences (CM.10.10.00) screen.
        /// </value>
        [PXDefault()]
        [GainLossSubAccountMask(DisplayName = "Combine Gain/Loss Sub. from")]
        public virtual String GainLossSubMask
        {
            get
            {
                return this._GainLossSubMask;
            }
            set
            {
                this._GainLossSubMask = value;
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
