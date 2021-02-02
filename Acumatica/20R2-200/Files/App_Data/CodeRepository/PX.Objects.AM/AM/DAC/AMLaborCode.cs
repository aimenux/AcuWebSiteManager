using System;
using PX.Data;
using PX.Objects.GL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Labor Code")]
    [PXPrimaryGraph(typeof(LaborCodeMaint))]
	public class AMLaborCode : IBqlTable, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMLaborCode>.By<laborCodeID>
        {
            public static AMLaborCode Find(PXGraph graph, string laborCodeID)
                => FindBy(graph, laborCodeID);
        }
                
        #endregion

        #region LaborType
        public abstract class laborType : PX.Data.BQL.BqlString.Field<laborType> { }

        protected String _LaborType;
        [PXDBString(1, IsFixed = true)]
        [AMLaborType.List]
        [PXDefault(AMLaborType.Direct, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Type")]
        public virtual String LaborType
        {
            get
            {
                return this._LaborType;
            }
            set
            {
                this._LaborType = value;
            }
        }
        #endregion
        #region LaborCodeID

	    public abstract class laborCodeID : PX.Data.BQL.BqlString.Field<laborCodeID> { }

        protected String _LaborCodeID;
        [PXDBString(15, IsKey = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Labor Code", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String LaborCodeID
        {
            get
            {
                return this._LaborCodeID;
            }
            set
            {
                this._LaborCodeID = value;
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
        #region LaborAccountID
        public abstract class laborAccountID : PX.Data.BQL.BqlInt.Field<laborAccountID> { }

        protected Int32? _LaborAccountID;
        [Account(DescriptionField = typeof(Account.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Labor Account")]
        [PXDefault]
        public virtual Int32? LaborAccountID
        {
            get
            {
                return this._LaborAccountID;
            }
            set
            {
                this._LaborAccountID = value;
            }
        }
        #endregion
        #region LaborSubID
        public abstract class laborSubID : PX.Data.BQL.BqlInt.Field<laborSubID> { }

        protected Int32? _LaborSubID;
        [SubAccount(DescriptionField = typeof(Sub.description), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Labor Sub")]
        [PXDBDefault]
        public virtual Int32? LaborSubID
        {
            get
            {
                return this._LaborSubID;
            }
            set
            {
                this._LaborSubID = value;
            }
        }
        #endregion
        #region OverheadAccountID
        public abstract class overheadAccountID : PX.Data.BQL.BqlInt.Field<overheadAccountID> { }

        protected Int32? _OverheadAccountID;
        [Account(DescriptionField = typeof(Account.description), DisplayName = "Overhead Account")]
        public virtual Int32? OverheadAccountID
        {
            get
            {
                return this._OverheadAccountID;
            }
            set
            {
                this._OverheadAccountID = value;
            }
        }
        #endregion
        #region OverheadSubID
        public abstract class overheadSubID : PX.Data.BQL.BqlInt.Field<overheadSubID> { }

        protected Int32? _OverheadSubID;
        [SubAccount(DescriptionField = typeof(Sub.description), DisplayName = "Overhead Sub")]
        public virtual Int32? OverheadSubID
        {
            get
            {
                return this._OverheadSubID;
            }
            set
            {
                this._OverheadSubID = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
