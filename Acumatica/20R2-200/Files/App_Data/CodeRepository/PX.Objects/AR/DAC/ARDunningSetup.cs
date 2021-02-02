namespace PX.Objects.AR
{
	using System;
	using PX.Data;

	/// <summary>
	/// Header of Remainder notification message
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARDunningSetup)]
	public partial class ARDunningSetup : PX.Data.IBqlTable
    {
        #region DunningLetterLevel
        public abstract class dunningLetterLevel : PX.Data.BQL.BqlInt.Field<dunningLetterLevel> { }
        protected Int32? _DunningLetterLevel;
        [PXDBInt(IsKey = true)]
		[PXDefault(0)]
        [PXUIField(DisplayName = Messages.DunningLetterLevel, Enabled = false)]
        [PXLineNbr(typeof(ARSetup))]
        [PXParent(typeof(Select<ARSetup>))]
        public virtual Int32? DunningLetterLevel
		{
			get
			{
                return this._DunningLetterLevel;
			}
			set
			{
                this._DunningLetterLevel = value;
			}
		}
		#endregion
        #region DueDays
        public abstract class dueDays : PX.Data.BQL.BqlInt.Field<dueDays> { }
        protected Int32? _DueDays;
        [PXDBInt(MinValue=0, MaxValue=365)]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Days Past Due")]
        public virtual Int32? DueDays
        {
            get
            {
                return this._DueDays;
            }
            set
            {
                this._DueDays = value;
            }
        }
        #endregion
        #region DaysToSettle
        public abstract class daysToSettle : PX.Data.BQL.BqlInt.Field<daysToSettle> { }
        protected Int32? _DaysToSettle;
        [PXDBInt(MinValue=0, MaxValue = 365)]
        [PXDefault(3)]
        [PXUIField(DisplayName = "Days to Settle")]
        public virtual Int32? DaysToSettle
        {
            get
            {
                return this._DaysToSettle;
            }
            set
            {
                this._DaysToSettle = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(60, IsUnicode = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Description")]
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
        #region DunningFee
        public abstract class dunningFee : PX.Data.BQL.BqlDecimal.Field<dunningFee> { }
        protected Decimal? _DunningFee;
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Dunning Fee", Visibility = PXUIVisibility.Visible, Enabled = true)]
        public virtual Decimal? DunningFee
        {
            get
            {
                return this._DunningFee;
            }
            set
            {
                this._DunningFee = value;
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
