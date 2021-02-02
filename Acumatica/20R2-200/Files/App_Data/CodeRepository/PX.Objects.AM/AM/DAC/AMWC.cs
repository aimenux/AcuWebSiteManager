using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Work Center Master Record
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.WorkCenter)]
    [PXPrimaryGraph(typeof(WCMaint))]
	public class AMWC : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMWC>.By<wcID>
        {
            public static AMWC Find(PXGraph graph, string wcID) => FindBy(graph, wcID);
        }

        public static class FK
        {
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMWC>.By<siteID> { }
        }
        #endregion

        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXReferentialIntegrityCheck]
        public virtual String WcID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
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
		#region BflushLbr
		public abstract class bflushLbr : PX.Data.BQL.BqlBool.Field<bflushLbr> { }

		protected Boolean? _BflushLbr;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Backflush Labor")]
		public virtual Boolean? BflushLbr
		{
			get
			{
				return this._BflushLbr;
			}
			set
			{
				this._BflushLbr = value;
			}
		}
		#endregion
		#region BflushMatl
		public abstract class bflushMatl : PX.Data.BQL.BqlBool.Field<bflushMatl> { }

		protected Boolean? _BflushMatl;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Backflush Materials")]
		public virtual Boolean? BflushMatl
		{
			get
			{
				return this._BflushMatl;
			}
			set
			{
				this._BflushMatl = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		[PXDBString(256, IsUnicode = true)]
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
		#region OutsideFlg
		public abstract class outsideFlg : PX.Data.BQL.BqlBool.Field<outsideFlg> { }

		protected Boolean? _OutsideFlg;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Outside Process")]
		public virtual Boolean? OutsideFlg
		{
			get
			{
				return this._OutsideFlg;
			}
			set
			{
				this._OutsideFlg = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected Int32? _SiteID;
        [Site(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
		{
			get
			{
                return this._SiteID;
			}
			set
			{
                this._SiteID = value;
			}
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }

		protected Decimal? _StdCost;
		[PXDBPriceCost]
		[PXDefault(TypeCode.Decimal,"0.0")]
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
		#region WcBasis
		public abstract class wcBasis : PX.Data.BQL.BqlString.Field<wcBasis> { }

		protected String _WcBasis;
		[PXDBString(1, IsFixed=true)]
		[PXDefault(BasisForCapacity.CrewSize)]
		[PXUIField(DisplayName = "Basis for Capacity")]
        [BasisForCapacity.List]
		public virtual String WcBasis
		{
			get
			{
				return this._WcBasis;
			}
			set
			{
				this._WcBasis = value;
			}
		}
		#endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationId;
        [Location(typeof(AMWC.siteID), Visible = false)]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationId;
            }
            set
            {
                this._LocationId = value;
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
        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlInt.Field<scrapAction> { }

        protected int? _ScrapAction;
        [PXDBInt]
        [PXDefault(Attributes.ScrapAction.NoAction)]
        [PXUIField(DisplayName = "Scrap Action Default")]
        [ScrapAction.List]
        public virtual int? ScrapAction
        {
            get
            {
                return this._ScrapAction;
            }
            set
            {
                this._ScrapAction = value;
            }
        }
        #endregion

        #region Methods/Attributes
        public static class BasisForCapacity
        {
            //Constants declaration 
            public const string CrewSize = "1";
            public const string Machines = "0";

            //List attribute 
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                    new string[] { CrewSize, Machines },
                    new string[] { "Crew Size", "Machines" }) { ; }
            }

            //BQL constants declaration
            public class crewSize : PX.Data.BQL.BqlString.Constant<crewSize>
            {
                public crewSize() : base(CrewSize) { ;}
            }
            public class machines : PX.Data.BQL.BqlString.Constant<machines>
            {
                public machines() : base(Machines) { ;}
            }

        }
        #endregion

    }
}
