using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Estimate Steps
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.EstimateStep)]
    public class AMEstimateStep : IBqlTable, IEstimateOper, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateStep>.By<estimateID, revisionID, operationID, lineID>
        {
            public static AMEstimateStep Find(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => FindBy(graph, estimateID, revisionID, operationID, lineID);
            public static AMEstimateStep FindDirty(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMEstimateStep,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateStep>.By<estimateID, revisionID> { }
            public class Operation : AMEstimateOper.PK.ForeignKeyOf<AMEstimateStep>.By<estimateID, revisionID, operationID> { }
        }

        #endregion

        #region Estimate ID

        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }


        protected String _EstimateID;

        [PXDBDefault(typeof(AMEstimateOper.estimateID))]
        [EstimateID(IsKey = true, Enabled = false, Visible = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        #endregion
        #region Revision ID

        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }


        protected String _RevisionID;

        [PXDBDefault(typeof(AMEstimateOper.revisionID))]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }

        #endregion
        #region Operation ID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }


        protected Int32? _OperationID;

        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMEstimateOper.operationID))]
        [PXParent(typeof(Select<AMEstimateOper,
            Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateMatl.estimateID>>,
            And<AMEstimateOper.revisionID, Equal<Current<AMEstimateMatl.revisionID>>,
            And<AMEstimateOper.operationID, Equal<Current<AMEstimateMatl.operationID>>>>>>))]
        [PXParent(typeof(Select<AMEstimateItem,
            Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateMatl.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateMatl.revisionID>>>>>))]
        public virtual Int32? OperationID
        {
            get { return this._OperationID; }
            set { this._OperationID = value; }
        }

        #endregion
        #region Line ID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMEstimateOper.lineCntrStep))]
        public virtual Int32? LineID
        {

            get { return this._LineID; }
            set { this._LineID = value; }
        }

        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(256, IsUnicode = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Description")]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        protected Int32? _SortOrder;
        [PXUIField(DisplayName = PX.Objects.AP.APTran.sortOrder.DispalyName)]
        [PXDBInt]
        public virtual Int32? SortOrder
        {
            get
            {
                return this._SortOrder;
            }
            set
            {
                this._SortOrder = value;
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
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        protected Byte[] _tstamp;

        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
    }
}
