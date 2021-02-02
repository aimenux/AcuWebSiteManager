using System;
using PX.Data;

namespace PX.Objects.CM.Extensions
{
	public interface IPXCurrencyRate
	{
		string FromCuryID { get; set; }
		DateTime? CuryEffDate { get; set; }
		string CuryMultDiv { get; set; }
		Decimal? CuryRate { get; set; }
		Decimal? RateReciprocal { get; set; }
		string ToCuryID { get; set; }
	}
	/// <summary>
	/// Represents an exchange rate of a particular type for a pair of currencies on a particular date.
	/// The records of this type are added and edited on the Currency Rates (CM301000) form
	/// (corresponds to the <see cref="CuryRateMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
    [PXCacheName(Messages.CurrencyRate)]
	public partial class CurrencyRate : PX.Data.IBqlTable, IPXCurrencyRate
	{
		#region CuryRateID
		public abstract class curyRateID : PX.Data.BQL.BqlInt.Field<curyRateID> { }
        /// <summary>
        /// Key field. Database identity.
        /// The unique identifier of the currency rate record.
        /// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "CuryRate ID", Visibility = PXUIVisibility.Visible)]
		public virtual Int32? CuryRateID
		{
			get;
			set;
		}
		#endregion
		#region FromCuryID
		public abstract class fromCuryID : PX.Data.BQL.BqlString.Field<fromCuryID> { }
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
			get;
			set;
		}
		#endregion
		#region CuryRateType
		public abstract class curyRateType : PX.Data.BQL.BqlString.Field<curyRateType> { }
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
			get;
			set;
		}
		#endregion
		#region CuryEffDate
		public abstract class curyEffDate : PX.Data.BQL.BqlDateTime.Field<curyEffDate> { }
        /// <summary>
        /// The date, starting from which the exchange rate is considered current.
        /// </summary>
		[PXDBDate()]
		[PXDefault(typeof(CuryRateFilter.effDate))]
		[PXUIField(DisplayName = "Currency Effective Date", Visibility = PXUIVisibility.Visible, Required = true)]
		public virtual DateTime? CuryEffDate
		{
			get;
			set;
		}
		#endregion
		#region CuryMultDiv
		public abstract class curyMultDiv : PX.Data.BQL.BqlString.Field<curyMultDiv> { }
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
		[PXUIField(DisplayName = "Mult/Div", Visibility = PXUIVisibility.Visible, Required = true)]
		[PXStringList("M;Multiply,D;Divide")]
		public virtual String CuryMultDiv
		{
			get;
			set;
		}
		#endregion
		#region CuryRate
		public abstract class curyRate : PX.Data.BQL.BqlDecimal.Field<curyRate> { }
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
			get;
			set;
		}
		#endregion
		#region RateReciprocal
		public abstract class rateReciprocal : PX.Data.BQL.BqlDecimal.Field<rateReciprocal> { }
        /// <summary>
        /// The inverse of the <see cref="CuryRate">exchange rate</see>, which is calculated automatically.
        /// </summary>
		[PXDBDecimal(8)]
		[PXUIField(DisplayName = "Rate Reciprocal", Visibility = PXUIVisibility.SelectorVisible, Enabled = false, Required = true)]
		public virtual Decimal? RateReciprocal
		{
			get;
			set;
		}
		#endregion
		#region ToCuryID
		public abstract class toCuryID : PX.Data.BQL.BqlString.Field<toCuryID> { }
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
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
	}

	[System.SerializableAttribute()]
	public partial class CuryRateFilter : PX.Data.IBqlTable
	{
		#region ToCurrency
		public abstract class toCurrency : PX.Data.BQL.BqlString.Field<toCurrency> { }
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search<PX.Objects.GL.Company.baseCuryID>))]
		[PXUIField(DisplayName = "To Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isActive, Equal<True>>>))]
		public virtual string ToCurrency
		{
			get;
			set;
		}
		#endregion
		#region EffDate
		public abstract class effDate : PX.Data.BQL.BqlDateTime.Field<effDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date")]
		[PXDefault(typeof(AccessInfo.businessDate))]
		public virtual DateTime? EffDate
		{
			get;
			set;
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
