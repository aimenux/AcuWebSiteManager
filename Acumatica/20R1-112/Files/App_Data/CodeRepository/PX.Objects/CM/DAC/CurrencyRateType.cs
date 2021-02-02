namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	
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
	public partial class CurrencyRateType : PX.Data.IBqlTable
	{
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;

        /// <summary>
        /// Key field.
        /// The unique identifier of the rate type.
        /// </summary>
		[PXDBString(6, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Rate Type ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

        /// <summary>
        /// The description of the rate type.
        /// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region RateEffDays
		public abstract class rateEffDays : PX.Data.BQL.BqlShort.Field<rateEffDays> { }
		protected short? _RateEffDays;

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
			get
			{
				return this._RateEffDays;
			}
			set
			{
				this._RateEffDays = value;
			}
		}
		#endregion
		#region RefreshOnline
		public abstract class refreshOnline : PX.Data.BQL.BqlBool.Field<refreshOnline> { }
		private bool? _RefreshOnline;

        /// <summary>
        /// Identifies that this currency rate should be automatically refreshed online.
        /// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Refresh Online")]
		public virtual bool? RefreshOnline 
		{
			get
			{
				return _RefreshOnline;
			}
			set
			{
				_RefreshOnline = value;
			}
		}

		#endregion
		#region OnlineRateAdjustment
		public abstract class onlineRateAdjustment : PX.Data.BQL.BqlDecimal.Field<onlineRateAdjustment> { }
        private decimal? _OnlineRateAdjustment;

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
			get
			{
				return _OnlineRateAdjustment;
			}
			set
			{
				_OnlineRateAdjustment = value;
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
