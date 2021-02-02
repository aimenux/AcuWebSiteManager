using System;
using PX.Data;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// BOM Tool
    /// </summary>
	[Serializable]
    [PXCacheName(Messages.BOMTool)]
    public class AMBomTool : IBqlTable, IBomOper, INotable, IBomDetail
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMBomTool>.By<bOMID, revisionID, operationID, lineID>
        {
            public static AMBomTool Find(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID)
                => FindBy(graph, bOMID, revisionID, operationID, lineID);
            public static AMBomTool FindDirty(PXGraph graph, string bOMID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMBomTool,
                    Where<bOMID, Equal<Required<bOMID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomTool>.By<bOMID, revisionID> { }
            public class Operation : AMBomOper.PK.ForeignKeyOf<AMBomTool>.By<bOMID, revisionID, operationID> { }
            public class Tool : AMToolMst.PK.ForeignKeyOf<AMBomTool>.By<toolID> { }
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
	        Where<AMBomOper.bOMID, Equal<Current<AMBomTool.bOMID>>,
	            And<AMBomOper.revisionID, Equal<Current<AMBomTool.revisionID>>,
	                And<AMBomOper.operationID, Equal<Current<AMBomTool.operationID>>>>>>))]
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
	    #region ToolID
	    public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }

	    protected String _ToolID;
	    [PXDBString(30, IsUnicode = true)]
	    [PXUIField(DisplayName = "Tool ID")]
	    [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
	    [PXSelector(typeof(Search<AMToolMst.toolID, Where<AMToolMst.active, Equal<True>>>))]
	    public virtual String ToolID
	    {
	        get
	        {
	            return this._ToolID;
	        }
	        set
	        {
	            this._ToolID = value;
	        }
	    }
	    #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXDefault(typeof(Search<AMToolMst.descr, Where<AMToolMst.toolID, Equal<Current<AMToolMst.toolID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Description")]
        [PXFormula(typeof(Default<AMBomTool.toolID>))]
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
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

		protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line ID", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMBomOper.lineCntrTool))]
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
		#region QtyReq
		public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

		protected decimal? _QtyReq;
		[PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty Required")]
		public virtual decimal? QtyReq
		{
			get
			{
				return this._QtyReq;
			}
			set
			{
				this._QtyReq = value;
			}
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

		protected decimal? _UnitCost;
		[PXDBPriceCost ]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMToolMst.unitCost, Where<AMToolMst.toolID, Equal<Current<AMToolMst.toolID>>>>))]
		[PXUIField(DisplayName = "Unit Cost")]
		[PXFormula(typeof(Default<AMBomTool.toolID>))]
        public virtual decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
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
