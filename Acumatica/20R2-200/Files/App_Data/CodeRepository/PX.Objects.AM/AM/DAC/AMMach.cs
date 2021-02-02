using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.AM
{	
    /// <summary>
    /// Production machine
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.Machine)]
    [PXPrimaryGraph(typeof(MachMaint))]
	public class AMMach : IBqlTable, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMMach>.By<machID>
        {
            public static AMMach Find(PXGraph graph, string machID)
                => FindBy(graph, machID);
        }
        
        #endregion

        #region MachID
        public abstract class machID : PX.Data.BQL.BqlString.Field<machID> { }

        protected String _MachID;
        [PXDBString(30, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Machine ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMMach.machID>))]
        public virtual String MachID
        {
            get
            {
                return this._MachID;
            }
            set
            {
                this._MachID = value;
            }
        }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(120, IsUnicode = true)]
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
		#region ActiveFlg
		public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

		protected Boolean? _ActiveFlg;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? ActiveFlg
		{
			get
			{
				return this._ActiveFlg;
			}
			set
			{
				this._ActiveFlg = value;
			}
		}
		#endregion
        #region DownFlg
        public abstract class downFlg : PX.Data.BQL.BqlBool.Field<downFlg> { }

        protected Boolean? _DownFlg;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Down")]
        public virtual Boolean? DownFlg
        {
            get
            {
                return this._DownFlg;
            }
            set
            {
                this._DownFlg = value;
            }
        }
        #endregion
        #region AssetID
        public abstract class assetID : PX.Data.BQL.BqlString.Field<assetID> { }

        protected String _AssetID;
        [PXDBString(30)]
        [PXUIField(DisplayName = "Asset ID")]
        public virtual String AssetID
        {
            get
            {
                return this._AssetID;
            }
            set
            {
                this._AssetID = value;
            }
        }
        #endregion
        #region StdCost
        public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

        protected Decimal? _StdCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Standard Cost")]
        public virtual Decimal? StdCost
        {
            get
            {
                return this._StdCost;
            }
            set
            {
                this._StdCost = value;
            }
        }
        #endregion
        #region CalendarID
        public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }

        protected String _CalendarID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Calendar ID")]
        [PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
        [PXForeignReference(typeof(Field<AMMach.calendarID>.IsRelatedTo<CSCalendar.calendarID>))]
        public virtual String CalendarID
        {
            get
            {
                return this._CalendarID;
            }
            set
            {
                this._CalendarID = value;
            }
        }
        #endregion
        #region MachEff
        public abstract class machEff : PX.Data.BQL.BqlDecimal.Field<machEff> { }

        protected Decimal? _MachEff;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Efficiency")]
        public virtual Decimal? MachEff
        {
            get
            {
                return this._MachEff;
            }
            set
            {
                this._MachEff = value;
            }
        }
        #endregion
        #region MachAcctID
        public abstract class machAcctID : PX.Data.BQL.BqlInt.Field<machAcctID> { }

        protected Int32? _MachAcctID;
        [Account]
        [PXDefault]
        public virtual Int32? MachAcctID
        {
            get
            {
                return this._MachAcctID;
            }
            set
            {
                this._MachAcctID = value;
            }
        }
        #endregion
        #region MachSubID
        public abstract class machSubID : PX.Data.BQL.BqlInt.Field<machSubID> { }

        protected Int32? _MachSubID;
        [SubAccount(typeof(AMMach.machAcctID))]
        [PXDefault]
        public virtual Int32? MachSubID
        {
            get
            {
                return this._MachSubID;
            }
            set
            {
                this._MachSubID = value;
            }
        }
        #endregion
		#region ActRunTime
		public abstract class actRunTime : PX.Data.BQL.BqlDecimal.Field<actRunTime> { }

        protected Decimal? _ActRunTime;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Actual Run Time", Visible = true, Enabled = false)]
        public virtual Decimal? ActRunTime
		{
			get
			{
				return this._ActRunTime;
			}
			set
			{
				this._ActRunTime = value;
			}
		}
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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
		[PXDBTimestamp]
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
        [PXDBCreatedByID]
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
        [PXDBCreatedByScreenID]
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
        [PXDBLastModifiedByScreenID]
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
