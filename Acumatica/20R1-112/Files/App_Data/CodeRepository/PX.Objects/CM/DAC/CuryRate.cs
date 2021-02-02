namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Objects.CM;

    [Serializable]
    [PXHidden]
	public partial class CurrencyRateByDate2 : CurrencyRate
	{
		#region FromCuryID
		public new abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		#endregion
		#region ToCuryID
		public new abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
		#endregion
		#region CuryRateType
		public new abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		#endregion
		#region NextEffDate
		public abstract class nextEffDate : PX.Data.BQL.BqlDateTime.Field<nextEffDate> { }
		protected DateTime? _NextEffDate;
		[PXDate()]
		[PXDBScalar(typeof(Search<CurrencyRate2.curyEffDate,
			Where<CurrencyRate2.fromCuryID, Equal<CurrencyRateByDate2.fromCuryID>,
			And<CurrencyRate2.toCuryID, Equal<CurrencyRateByDate2.toCuryID>,
			And<CurrencyRate2.curyRateType, Equal<CurrencyRateByDate2.curyRateType>,
			And<CurrencyRate2.curyEffDate, Greater<CurrencyRateByDate2.curyEffDate>>>>>,
			OrderBy<Asc<CurrencyRate2.curyEffDate>>>))]
		public virtual DateTime? NextEffDate
		{
			get
			{
				return this._NextEffDate;
			}
			set
			{
				this._NextEffDate = value;
			}
		}
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		#endregion
	}

	/// <summary>
	/// An alias for the <see cref="CurrencyRate"/> DAC.
	/// </summary>
    [PXCacheName(Messages.CurrencyRate2)]
    [Serializable]
	public partial class CurrencyRate2 : CurrencyRate
	{
		public new abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		public new abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		public new abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
		public new abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
	}
	/// <summary>
	/// Acts as a view into the base <see cref="CurrencyRate"/> entity and provides the additional <see cref="NextEffDate"/> field.
	/// The DAC can be used to select a currency rate for a particular date: 
	/// The desired date must be between <see cref="curyEffDate"/> (inclusive) and <see cref="NextEffDate"/> (exclusive).
	/// CurrencyRateByDateForVendor is different from CurrencyRateByDate by the presence of additional "Where" to optimize performance.
	/// </summary>
	[PXProjection(typeof(Select5<CurrencyRate,
						LeftJoin<CurrencyRate2,
							On<CurrencyRate2.fromCuryID, Equal<CurrencyRate.fromCuryID>,
								And<CurrencyRate2.toCuryID, Equal<CurrencyRate.toCuryID>,
								And<CurrencyRate2.curyRateType, Equal<CurrencyRate.curyRateType>,
								And<CurrencyRate2.curyEffDate, Greater<CurrencyRate.curyEffDate>>>>>>,
						Where<CurrencyRate.curyRateType, Equal<CurrentValue<AP.Vendor.curyRateTypeID>>,
							And<CurrencyRate.toCuryID, Equal<CurrentValue<AP.Vendor.curyID>>>>,
						Aggregate<
							GroupBy<CurrencyRate.curyRateID,
							Min<CurrencyRate2.curyEffDate>>>>))]
	[Serializable]
	[PXCacheName(Messages.CurrencyRateByDate)]
	[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica8)]
	public partial class CurrencyRateByDateForVendor : CurrencyRate
	{
		#region FromCuryID
		public new abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		#endregion
		#region ToCuryID
		public new abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
		#endregion
		#region CuryRateType
		public new abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		#endregion
		#region NextEffDate
		public abstract class nextEffDate : PX.Data.BQL.BqlDateTime.Field<nextEffDate> { }
		[PXDBDate(BqlField = typeof(CurrencyRate2.curyEffDate))]
		public virtual DateTime? NextEffDate { get; set; }
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		#endregion
	}

	/// <exclude />
	[PXHidden]
	public class CurrencyFilter : IBqlTable
	{
		public abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		[PXDBString(5, IsUnicode = true)]
		public virtual string FromCuryID { get; set; }

		public abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
		[PXDBString(5, IsUnicode = true)]
		public virtual string ToCuryID { get; set; }
	}

	/// <summary>
	/// Acts as a view into the base <see cref="CurrencyRate"/> entity and provides the additional <see cref="NextEffDate"/> field.
	/// The DAC can be used to select a currency rate for a particular date: 
	/// The desired date must be between <see cref="curyEffDate"/> (inclusive) and <see cref="NextEffDate"/> (exclusive).
	/// </summary>
	[PXProjection(typeof(Select5<CurrencyRate,
						LeftJoin<CurrencyRate2,
							On<CurrencyRate2.fromCuryID, Equal<CurrencyRate.fromCuryID>,
								And<CurrencyRate2.toCuryID, Equal<CurrencyRate.toCuryID>,
								And<CurrencyRate2.curyRateType, Equal<CurrencyRate.curyRateType>,
								And<CurrencyRate2.curyEffDate, Greater<CurrencyRate.curyEffDate>>>>>>,
						Where<CurrencyRate.fromCuryID, Equal<CurrentValue<CurrencyFilter.fromCuryID>>,
							And<CurrencyRate.toCuryID, Equal<CurrentValue<CurrencyFilter.toCuryID>>>>,
						Aggregate<
							GroupBy<CurrencyRate.curyRateID,
							Min<CurrencyRate2.curyEffDate>>>>))]
	[Serializable]
	[PXCacheName(Messages.CurrencyRateByDate)]
	public partial class CurrencyRateByDate : CurrencyRate
	{
		#region FromCuryID
		public new abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		#endregion
		#region ToCuryID
		public new abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
		#endregion
		#region CuryRateType
		public new abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
		#endregion
		#region CuryEffDate
		public new abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		#endregion
		#region NextEffDate
		public abstract class nextEffDate : PX.Data.BQL.BqlDateTime.Field<nextEffDate> { }
		[PXDBDate(BqlField = typeof(CurrencyRate2.curyEffDate))]
		public virtual DateTime? NextEffDate { get; set; }
		#endregion
		#region CuryMultDiv
		public new abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
		#endregion
		#region CuryRate
		public new abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
		#endregion
	}

    /// <summary>
    /// Represents an exchange rate of a particular type for a pair of currencies on a particular date.
    /// The records of this type are added and edited on the Currency Rates (CM301000) form
    /// (corresponds to the <see cref="CuryRateMaint"/> graph).
    /// </summary>
	[System.SerializableAttribute()]
    [PXCacheName(Messages.CurrencyRate)]
	public partial class CurrencyRate : PX.Data.IBqlTable
	{
		#region CuryRateID
		public abstract class curyRateID : PX.Data.BQL.BqlInt.Field<curyRateID> { }
		protected Int32? _CuryRateID;

        /// <summary>
        /// Key field. Database identity.
        /// The unique identifier of the currency rate record.
        /// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "CuryRate ID", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CuryRateID
		{
			get
			{
				return this._CuryRateID;
			}
			set
			{
				this._CuryRateID = value;
			}
		}
		#endregion
		#region FromCuryID
		public abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
		protected String _FromCuryID;

        /// <summary>
        /// The identifier of the source <see cref="Currency"/>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "From Currency", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.curyID, NotEqual<Current<CuryRateFilter.toCurrency>>, And<CurrencyList.isActive, Equal<True>>>>))]
		public virtual String FromCuryID
		{
			get
			{
				return this._FromCuryID;
			}
			set
			{
				this._FromCuryID = value;
			}
		}
		#endregion
		#region CuryRateType
		public abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
		protected String _CuryRateType;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">type of the rate</see>.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate Type", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID), DescriptionField = typeof(CurrencyRateType.descr))]
		public virtual String CuryRateType
		{
			get
			{
				return this._CuryRateType;
			}
			set
			{
				this._CuryRateType = value;
			}
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
		protected DateTime? _CuryEffDate;

        /// <summary>
        /// The date, starting from which the exchange rate is considered current.
        /// </summary>
		[PXDBDate()]
		[PXDefault(typeof(CuryRateFilter.effDate))]
		[PXUIField(DisplayName = "Currency Effective Date", Visibility = PXUIVisibility.Visible, Required = true)]
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
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
        protected String _CuryMultDiv;

        /// <summary>
        /// The operation required for currency conversion: Divide or Multiply.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"M"</c> - Multiply,
        /// <c>"D"</c> - Divide.
        /// Defaults to <c>"M"</c>.
        /// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName = "Mult./Div.", Visibility = PXUIVisibility.Visible, Required = true)]
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
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
        protected Decimal? _CuryRate;

        /// <summary>
        /// The currency rate. For the purposes of conversion the value of this field is used
        /// together with the operation selected in the <see cref="CuryMultDiv"/> field.
        /// </summary>
        /// <value>
        /// Defaults to <c>1.0</c>.
        /// </value>
		[PXDBDecimal(8, MinValue = 0d)]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency Rate", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region RateReciprocal
		public abstract class rateReciprocal : PX.Data.BQL.BqlDecimal.Field<rateReciprocal> { }
        protected Decimal? _RateReciprocal;

        /// <summary>
        /// The inverse of the <see cref="CuryRate">exchange rate</see>, which is calculated automatically.
        /// </summary>
		[PXDBDecimal(8)]
		[PXUIField(DisplayName = "Rate Reciprocal", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		public virtual Decimal? RateReciprocal
		{
			get
			{
				return this._RateReciprocal;
			}
			set
			{
				this._RateReciprocal = value;
			}
		}
		#endregion
		#region ToCuryID
		public abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
        protected String _ToCuryID;

        /// <summary>
        /// The identifier of the destination <see cref="Currency"/>.
        /// </summary>
        /// <value>
        /// Defaults to the <see cref="Company.BaseCuryID">base currency of the company</see> through
        /// the <see cref="CuryRateFilter.ToCurrency"/> field.
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "To Currency", Visibility = PXUIVisibility.Invisible)]
		[PXDefault(typeof(CuryRateFilter.toCurrency))]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<True>>>))]
		public virtual String ToCuryID
		{
			get
			{
				return this._ToCuryID;
			}
			set
			{
				this._ToCuryID = value;
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
		#region Methods
		public static implicit operator CurrencyInfo(CurrencyRate item)
		{
			CurrencyInfo ret	= new CurrencyInfo();
			ret.BaseCuryID		= item.ToCuryID;
			ret.CuryRateTypeID	= item.CuryRateType;
			ret.CuryEffDate		= item.CuryEffDate == null ? (DateTime?)null : ((DateTime)item.CuryEffDate).AddDays(-1);
			ret.CuryID			= item.FromCuryID;
			ret.CuryMultDiv		= item.CuryMultDiv;
			ret.CuryRate		= item.CuryRate;
			ret.RecipRate		= item.RateReciprocal;

			return ret;
		}
		#endregion
	}

	[System.SerializableAttribute()]
	public partial class CuryRateFilter : PX.Data.IBqlTable
	{
		#region ToCurrency
		public abstract class toCurrency : PX.Data.BQL.BqlString.Field<toCurrency> { }
		protected string _ToCurrency;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<PX.Objects.GL.Company.baseCuryID>))]
		[PXUIField(DisplayName = "To Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<True>>>))]
		public virtual string ToCurrency
		{
			get
			{
				return this._ToCurrency;
			}
			set
			{
				this._ToCurrency = value;
			}
		}
		#endregion
		#region EffDate
		public abstract class effDate : PX.Data.BQL.BqlDateTime.Field<effDate> { }
		protected DateTime? _EffDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EffDate
		{
			get
			{
				return this._EffDate;
			}
			set
			{
				this._EffDate = value;
			}
		}
		#endregion
	}

	public class CuryMultDivType
	{
		public const string Mult = "M";
		public const string Div = "D";

		public class mult : PX.Data.BQL.BqlString.Constant<mult>
		{
			public mult():base(Mult)
			{
			}
		}
		public class div : PX.Data.BQL.BqlString.Constant<div>
		{
			public div()
				: base(Div)
			{
			}
		}
	}
}
