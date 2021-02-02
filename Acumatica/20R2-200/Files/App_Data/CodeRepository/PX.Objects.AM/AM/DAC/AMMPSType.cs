using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("MPSTypeID = {MPSTypeID}, Dependent = {Dependent}")]
	[Serializable]
    [PXCacheName("Master Production Schedule Type")]
    [PXPrimaryGraph(typeof(MPSType))]
	public class AMMPSType : IBqlTable, INotable
    {
        #region Dependent
        public abstract class dependent : PX.Data.BQL.BqlBool.Field<dependent> { }

        protected Boolean? _Dependent;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Dependent")]
        public virtual Boolean? Dependent
        {
            get
            {
                return this._Dependent;
            }
            set
            {
                this._Dependent = value;
            }
        }
        #endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
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
        #region MPSNumberingID

        public abstract class mpsNumberingID : PX.Data.BQL.BqlString.Field<mpsNumberingID> { }

        protected String _MPSNumberingID;
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
        [PXUIField(DisplayName = "Numbering Sequence", Visibility = PXUIVisibility.Visible)]
        public virtual String MPSNumberingID
        {
            get
            {
                return this._MPSNumberingID;
            }
            set
            {
                this._MPSNumberingID = value;
            }
        }
        #endregion
		#region MPSTypeID
		public abstract class mPSTypeID : PX.Data.BQL.BqlString.Field<mPSTypeID> { }

		protected String _MPSTypeID;
		[PXDBString(20, IsUnicode= true, IsKey=true, InputMask = ">AAAAAAAAAAAAAAAAAAAA")]
		[PXUIField(DisplayName = "Type ID", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
		public virtual String MPSTypeID
		{
			get
			{
				return this._MPSTypeID;
			}
			set
			{
				this._MPSTypeID = value;
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
