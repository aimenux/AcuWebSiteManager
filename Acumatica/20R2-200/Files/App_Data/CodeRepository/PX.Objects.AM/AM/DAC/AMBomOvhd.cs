using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// BOM Overhead
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.BOMOvhd)]
    public class AMBomOvhd : IBqlTable, IBomOper, INotable, IBomDetail
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMBomOvhd>.By<bOMID, revisionID, operationID, lineID>
        {
            public static AMBomOvhd Find(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID)
                => FindBy(graph, bOMID, revisionID, operationID, lineID);
            public static AMBomOvhd FindDirty(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMBomOvhd,
                    Where<bOMID, Equal<Required<bOMID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomOvhd>.By<bOMID, revisionID> { }
            public class Operation : AMBomOper.PK.ForeignKeyOf<AMBomOvhd>.By<bOMID, revisionID, operationID> { }
            public class Overhead : AMOverhead.PK.ForeignKeyOf<AMBomOvhd>.By<ovhdID> { }
        }

        #endregion

        #region Selected

        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }

        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected string _BOMID;
        [BomID(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMBomOper.bOMID))]
        public virtual string BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
	    #region RevisionID
	    public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

	    protected string _RevisionID;
        [RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMBomOper.revisionID))]
	    public virtual string RevisionID
	    {
	        get
	        {
	            return this._RevisionID;
	        }
	        set
	        {
	            this._RevisionID = value;
	        }
	    }
	    #endregion
	    #region OperationID
	    public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

	    protected int? _OperationID;
	    [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
	    [PXDBDefault(typeof(AMBomOper.operationID))]
	    [PXParent(typeof(Select<AMBomOper,
	        Where<AMBomOper.bOMID, Equal<Current<AMBomOvhd.bOMID>>,
	            And<AMBomOper.revisionID, Equal<Current<AMBomOvhd.revisionID>>,
	                And<AMBomOper.operationID, Equal<Current<AMBomOvhd.operationID>>>>>>))]
	    public virtual int? OperationID
	    {
	        get
	        {
	            return this._OperationID;
	        }
	        set
	        {
	            this._OperationID = value;
	        }
	    }
	    #endregion
        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "LineID", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMBomOper.lineCntrOvhd))]
        public virtual Int32? LineID
        {
            get
            {
                return this._LineID;
            }
            set
            {
                this._LineID = value;
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
		#region OFactor
		public abstract class oFactor : PX.Data.BQL.BqlDecimal.Field<oFactor> { }

		protected Decimal? _OFactor;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
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
		[PXDefault]
		[OverheadIDField(Visibility = PXUIVisibility.SelectorVisible)]
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
        #region RowStatus
        public abstract class rowStatus : PX.Data.BQL.BqlInt.Field<rowStatus> { }
        protected int? _RowStatus;
        [PXDBInt]
        [PXUIField(DisplayName = "Change Status", Enabled = false)]
        [AMRowStatus.List]
        public virtual int? RowStatus
        {
            get
            {
                return this._RowStatus;
            }
            set
            {
                this._RowStatus = value;
            }
        }
        #endregion
    }
}
