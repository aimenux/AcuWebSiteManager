using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateHistory)]
    public class AMEstimateHistory : IBqlTable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateHistory>.By<estimateID>
        {
            public static AMEstimateHistory Find(PXGraph graph, string estimateID)
                => FindBy(graph, estimateID);
            public static AMEstimateHistory FindDirty(PXGraph graph, string estimateID)
                => PXSelect<AMEstimateHistory,
                    Where<estimateID, Equal<Required<estimateID>>>>
                    .SelectWindowed(graph, 0, 1, estimateID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateHistory>.By<estimateID, revisionID> { }
        }

        #endregion
        #region Record ID
        [Obsolete("Use AMEstimateHistory.lineNbr")]
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected Int64? _RecordID;
        [Obsolete("Use AMEstimateHistory.LineNbr")]
        [PXDBLong]
        [PXUIField(DisplayName = "Record ID", Visibility = PXUIVisibility.Invisible, Enabled = false, Visible = false)]
        public virtual Int64? RecordID
        {
            get { return this._RecordID; }
            set { this._RecordID = value; }
        }
        #endregion
        #region Estimate ID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;
        [PXDBDefault(typeof(AMEstimateItem.estimateID))]
        [EstimateID(IsKey = true, Enabled = false, Visible = false)]
        [PXParent(typeof(Select<AMEstimateItem,
            Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateHistory.estimateID>>>>))]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXLineNbr(typeof(AMEstimateItem.lineCntrHistory))]
        [PXUIField(DisplayName = "History Line Number", Enabled = false, Visible = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXDBDefault(typeof(AMEstimateItem.revisionID))]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA")]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        [PXDefault]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(DontOverrideValue = true)]
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
        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "Created At", Enabled = false)]
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
    }
}