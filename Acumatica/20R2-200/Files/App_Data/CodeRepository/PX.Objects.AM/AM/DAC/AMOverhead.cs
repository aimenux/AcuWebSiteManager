using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(OvhdMaint))]
    [PXCacheName(Messages.Overhead)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]

    public class AMOverhead : IBqlTable, INotable
    {
        internal string DebuggerDisplay => $"OvhdID = {OvhdID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMOverhead>.By<ovhdID>
        {
            public static AMOverhead Find(PXGraph graph, string ovhdID) => FindBy(graph, ovhdID);
        }

        #endregion

        #region OvhdID
        public abstract class ovhdID : PX.Data.BQL.BqlString.Field<ovhdID> { }

	    protected String _OvhdID;
	    [OverheadIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
	    [PXDefault]
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
        #region CostRate
        public abstract class costRate : PX.Data.BQL.BqlDecimal.Field<costRate> { }

		protected Decimal? _CostRate;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Cost Rate")]
		public virtual Decimal? CostRate
		{
			get
			{
				return this._CostRate;
			}
			set
			{
				this._CostRate = value;
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
		#region AcctID
		public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

		protected Int32? _AcctID;
		[PXDefault]
        [Account]
		public virtual Int32? AcctID
		{
			get
			{
                return this._AcctID;
			}
			set
			{
                this._AcctID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

		protected Int32? _SubID;
		[PXDefault]
        [SubAccount(typeof(AMOverhead.acctID),Visibility = PXUIVisibility.Visible)]
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
		#region OvhdType
		public abstract class ovhdType : PX.Data.BQL.BqlString.Field<ovhdType> { }

		protected String _OvhdType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Type", Required=true, Visibility = PXUIVisibility.SelectorVisible)]
        [OverheadType.List]
		public virtual String OvhdType
		{
			get
			{
				return this._OvhdType;
			}
			set
			{
				this._OvhdType = value;
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
