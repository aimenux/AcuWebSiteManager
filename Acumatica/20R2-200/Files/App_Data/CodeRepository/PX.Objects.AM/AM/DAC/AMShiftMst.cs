using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Shift Master")]
	public class AMShiftMst : IBqlTable, INotable
    {
        #region Keys
        public class PK : PrimaryKeyOf<AMShiftMst>.By<shiftID>
        {
            public static AMShiftMst Find(PXGraph graph, string shiftID) => FindBy(graph, shiftID);
        }

        #endregion

        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

	    protected String _ShiftID;
	    [PXDBString(4, IsKey = true, InputMask = "####")]
	    [PXDefault]
	    [PXUIField(DisplayName = "Shift", Visibility = PXUIVisibility.SelectorVisible)]
        [PXReferentialIntegrityCheck]
        public virtual String ShiftID
	    {
	        get
	        {
	            return this._ShiftID;
	        }
	        set
	        {
	            this._ShiftID = value;
	        }
	    }
	    #endregion
        #region DiffType
        public abstract class diffType : PX.Data.BQL.BqlString.Field<diffType> { }

		protected String _DiffType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ShiftDiffType.Amount)]
		[PXUIField(DisplayName = "Diff Type", Required=true)]
        [ShiftDiffType.List]
		public virtual String DiffType
		{
			get
			{
				return this._DiffType;
			}
			set
			{
				this._DiffType = value;
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
		#region ShftDesc
		public abstract class shftDesc : PX.Data.BQL.BqlString.Field<shftDesc> { }

		protected String _ShftDesc;
		[PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ShftDesc
		{
			get
			{
				return this._ShftDesc;
			}
			set
			{
				this._ShftDesc = value;
			}
		}
		#endregion
		#region ShftDiff
		public abstract class shftDiff : PX.Data.BQL.BqlDecimal.Field<shftDiff> { }

		protected Decimal? _ShftDiff;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Shift Diff")]
		public virtual Decimal? ShftDiff
		{
			get
			{
				return this._ShftDiff;
			}
			set
			{
				this._ShftDiff = value;
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
