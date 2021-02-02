using System;
using PX.Data;

namespace PX.Objects.CM.Extensions
{
	public interface IPXCurrencyRateType
	{
		string CuryRateTypeID { get; set; }
		string Descr { get; set; }
	}
	/// <summary>
	/// Represents the types of currency rates, which can be used to distinguish rates by their sources and by the accounting routines the rates are used for.
	/// The records of this type are edited on the Currency Rate Types (CM201000) form (corresponds to the <see cref="CurrencyRateTypeMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(
		new Type[] { typeof(CurrencyRateTypeMaint)},
		new Type[] { typeof(Select<CurrencyRateType, 
			Where<CurrencyRateType.curyRateTypeID, Equal<Current<CurrencyRateType.curyRateTypeID>>>>)
		})]
	[PXCacheName(Messages.CurrencyRateType)]
	public partial class CurrencyRateType : PX.Data.IBqlTable, IPXCurrencyRateType
	{
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
        /// <summary>
        /// Key field.
        /// The unique identifier of the rate type.
        /// </summary>
		[PXDBString(6, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Rate Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryRateTypeID
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        /// <summary>
        /// The description of the rate type.
        /// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get;
			set;
		}
		#endregion
		#region RateEffDays
		public abstract class rateEffDays : PX.Data.BQL.BqlShort.Field<rateEffDays> { }
        /// <summary>
        /// The number of days, during which the rate of this type is considered current.
        /// </summary>
        /// <value>
        /// Defaults to <c>0</c>, meaning that the rate remains effective until the date when a new rate is specified.
        /// </value>
		[PXDBShort()]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = "Days Effective")]
		public virtual short? RateEffDays
		{
			get;
			set;
		}
		#endregion
		#region RefreshOnline
		public abstract class refreshOnline : PX.Data.BQL.BqlBool.Field<refreshOnline> { }
        /// <summary>
        /// Identifies that this currency rate should be automatically refreshed online.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Refresh Online")]
		public virtual bool? RefreshOnline 
		{
			get;
			set;
		}

		#endregion
		#region OnlineRateAdjustment
		public abstract class onlineRateAdjustment : PX.Data.BQL.BqlDecimal.Field<onlineRateAdjustment> { }
        /// <summary>
        /// Adjustment percentage to apply to the rate retrieved online.
        /// </summary>
        /// <value>
        /// Defaults to <c>0.00</c>, meaning that the rate actual exchange rate will be used without any adjustment.
        /// </value>
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Online Rate Adjustment (%)")]
		public virtual decimal? OnlineRateAdjustment
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
}
