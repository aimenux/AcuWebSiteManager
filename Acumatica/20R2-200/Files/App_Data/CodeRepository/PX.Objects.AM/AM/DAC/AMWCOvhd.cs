using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{	
	[Serializable]
    [PXCacheName(Messages.WcOverheads)]
    public class AMWCOvhd : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMWCOvhd>.By<ovhdID, wcID>
        {
            public static AMWCOvhd Find(PXGraph graph, string ovhdID, string wcID) => FindBy(graph, ovhdID, wcID);
        }

        public static class FK
        {
            public class Overhead : AMOverhead.PK.ForeignKeyOf<AMWCOvhd>.By<ovhdID> { }
            public class WorkCenter : AMWC.PK.ForeignKeyOf<AMWCOvhd>.By<wcID> { }
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
		#region OFactor
		public abstract class oFactor : PX.Data.BQL.BqlDecimal.Field<oFactor> { }

		protected Decimal? _OFactor;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Factor")]
        public virtual Decimal? OFactor
		{
			get
			{
				return this._OFactor;
			}
			set
			{
				this._OFactor = value;
			}
		}
		#endregion
		#region OvhdID
		public abstract class ovhdID : PX.Data.BQL.BqlString.Field<ovhdID> { }

		protected String _OvhdID;
	    [OverheadIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<AMOverhead.ovhdID>))]
		public virtual String OvhdID
		{
			get
			{
				return this._OvhdID;
			}
			set
			{
				this._OvhdID = value;
			}
		}
		#endregion
		#region WcID
		public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

		protected String _WcID;
	    [WorkCenterIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMWC.wcID))]
        [PXParent(typeof(Select<AMWC, Where<AMWC.wcID, Equal<Current<AMWCOvhd.wcID>>>>))]
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
