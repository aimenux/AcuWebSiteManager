using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.ProductionTool)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXPrimaryGraph(
        new Type[] { typeof(ProdDetail) },
        new Type[] { typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdTool.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdTool.prodOrdID>>>>>)})]
    public class AMProdTool : IBqlTable, IProdOper, IPhantomBomReference, INotable
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] OperationID = {OperationID}, LineID = {LineID}, ToolID = {ToolID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdTool>.By<orderType, prodOrdID, operationID, lineID>
        {
            public static AMProdTool Find(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID) 
                => FindBy(graph, orderType, prodOrdID, operationID, lineID);
            public static AMProdTool FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID)
                => PXSelect<AMProdTool,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID, lineID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdTool>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdTool>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMProdTool>.By<orderType, prodOrdID, operationID> { }
            public class Tool : AMToolMst.PK.ForeignKeyOf<AMProdTool>.By<toolID> { }
        }

        #endregion

        #region OrderType

        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }


        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.orderType))]
        public virtual String OrderType
        {
            get { return this._OrderType; }
            set { this._OrderType = value; }
        }

        #endregion
        #region ProdOrdID

        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }


        protected String _ProdOrdID;

        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.prodOrdID))]
        public virtual String ProdOrdID
        {
            get { return this._ProdOrdID; }
            set { this._ProdOrdID = value; }
        }

        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.operationID))]
        [PXParent(typeof(Select<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdTool.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMProdTool.prodOrdID>>,
                    And<AMProdOper.operationID, Equal<Current<AMProdTool.operationID>>>>>>))]
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
        [PXLineNbr(typeof(AMProdOper.lineCntrTool))]
        public virtual Int32? LineID
        {
            get { return this._LineID; }
            set { this._LineID = value; }
        }

        #endregion
        #region ToolID

        public abstract class toolID : PX.Data.BQL.BqlString.Field<toolID> { }


        protected String _ToolID;

        [ToolIDField]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Search<AMToolMst.toolID>))]
        public virtual String ToolID
        {
            get { return this._ToolID; }
            set { this._ToolID = value; }
        }

        #endregion
        #region Descr

        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }


        protected String _Descr;

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual String Descr
        {
            get { return this._Descr; }
            set { this._Descr = value; }
        }

        #endregion
        #region QtyReq

        public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }


        protected decimal? _QtyReq;

        [PXDBQuantity(MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Required")]
        public virtual decimal? QtyReq
        {
            get { return this._QtyReq; }
            set { this._QtyReq = value; }
        }

        #endregion
        #region UnitCost

        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }


        protected decimal? _UnitCost;

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Cost")]
        public virtual decimal? UnitCost
        {
            get { return this._UnitCost; }
            set { this._UnitCost = value; }
        }

        #endregion
        #region TotActCost

        public abstract class totActCost : PX.Data.BQL.BqlDecimal.Field<totActCost> { }

        protected Decimal? _TotActCost;

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Actual Cost", Enabled = false)]
        public virtual Decimal? TotActCost
        {
            get { return this._TotActCost; }
            set { this._TotActCost = value; }
        }

        #endregion
        #region NoteID

        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
        public virtual Guid? NoteID
        {
            get { return this._NoteID; }
            set { this._NoteID = value; }
        }

        #endregion
        #region PhtmBOMID
        public abstract class phtmBOMID : PX.Data.BQL.BqlString.Field<phtmBOMID> { }

        protected String _PhtmBOMID;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Phantom BOM ID", Visible = false, Enabled = false)]
        public virtual String PhtmBOMID
        {
            get
            {
                return this._PhtmBOMID;
            }
            set
            {
                this._PhtmBOMID = value;
            }
        }
        #endregion
        #region PhtmBOMRevisionID
        public abstract class phtmBOMRevisionID : PX.Data.BQL.BqlString.Field<phtmBOMRevisionID> { }

        protected string _PhtmBOMRevisionID;
        [RevisionIDField(DisplayName = "Phantom BOM Revision", Visible = false, Enabled = false)]
        public virtual string PhtmBOMRevisionID
        {
            get
            {
                return this._PhtmBOMRevisionID;
            }
            set
            {
                this._PhtmBOMRevisionID = value;
            }
        }
        #endregion
        #region PhtmBOMLineRef
        public abstract class phtmBOMLineRef : PX.Data.BQL.BqlInt.Field<phtmBOMLineRef> { }

        protected Int32? _PhtmBOMLineRef;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom BOM Ref Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? PhtmBOMLineRef
        {
            get
            {
                return this._PhtmBOMLineRef;
            }
            set
            {
                this._PhtmBOMLineRef = value;
            }
        }
        #endregion
        #region PhtmBOMOperationID
        public abstract class phtmBOMOperationID : PX.Data.BQL.BqlInt.Field<phtmBOMOperationID> { }

        protected int? _PhtmBOMOperationID;
        [OperationIDField(DisplayName = "Phantom Operation ID", Visible = false, Enabled = false)]
        public virtual int? PhtmBOMOperationID
        {
            get
            {
                return this._PhtmBOMOperationID;
            }
            set
            {
                this._PhtmBOMOperationID = value;
            }
        }
        #endregion
        #region PhtmLevel
        public abstract class phtmLevel : PX.Data.BQL.BqlInt.Field<phtmLevel> { }

        protected Int32? _PhtmLevel;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom Level", Visible = false, Enabled = false)]
        public virtual Int32? PhtmLevel
        {
            get
            {
                return this._PhtmLevel;
            }
            set
            {
                this._PhtmLevel = value;
            }
        }
        #endregion
        #region PhtmMatlBOMID
        public abstract class phtmMatlBOMID : PX.Data.BQL.BqlString.Field<phtmMatlBOMID> { }

        protected String _PhtmMatlBOMID;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Phantom Matl BOM ID", Visible = false, Enabled = false)]
        public virtual String PhtmMatlBOMID
        {
            get
            {
                return this._PhtmMatlBOMID;
            }
            set
            {
                this._PhtmMatlBOMID = value;
            }
        }
        #endregion
        #region PhtmMatlRevisionID
        public abstract class phtmMatlRevisionID : PX.Data.BQL.BqlString.Field<phtmMatlRevisionID> { }

        protected string _PhtmMatlRevisionID;
        [RevisionIDField(DisplayName = "Phantom Matl Revision", Visible = false, Enabled = false)]
        public virtual string PhtmMatlRevisionID
        {
            get
            {
                return this._PhtmMatlRevisionID;
            }
            set
            {
                this._PhtmMatlRevisionID = value;
            }
        }
        #endregion
        #region PhtmMatlLineRef
        public abstract class phtmMatlLineRef : PX.Data.BQL.BqlInt.Field<phtmMatlLineRef> { }

        protected Int32? _PhtmMatlLineRef;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom Matl Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? PhtmMatlLineRef
        {
            get
            {
                return this._PhtmMatlLineRef;
            }
            set
            {
                this._PhtmMatlLineRef = value;
            }
        }
        #endregion
        #region PhtmMatlOperationID
        public abstract class phtmMatlOperationID : PX.Data.BQL.BqlInt.Field<phtmMatlOperationID> { }

        protected int? _PhtmMatlOperationID;
        [OperationIDField(DisplayName = "Phantom Matl Operation ID", Visible = false, Enabled = false)]
        public virtual int? PhtmMatlOperationID
        {
            get
            {
                return this._PhtmMatlOperationID;
            }
            set
            {
                this._PhtmMatlOperationID = value;
            }
        }
        #endregion
        #region TotActUses

        public abstract class totActUses : PX.Data.BQL.BqlDecimal.Field<totActUses> { }

        protected Decimal? _TotActUses;

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Actual Uses", Enabled = false)]
        public virtual Decimal? TotActUses
        {
            get { return this._TotActUses; }
            set { this._TotActUses = value; }
        }

        #endregion
        #region tstamp

        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        protected Byte[] _tstamp;

        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
        }

        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        protected Guid? _CreatedByID;

        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get { return this._CreatedByID; }
            set { this._CreatedByID = value; }
        }

        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        protected String _CreatedByScreenID;

        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get { return this._CreatedByScreenID; }
            set { this._CreatedByScreenID = value; }
        }

        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        protected DateTime? _CreatedDateTime;

        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get { return this._CreatedDateTime; }
            set { this._CreatedDateTime = value; }
        }

        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        protected Guid? _LastModifiedByID;

        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get { return this._LastModifiedByID; }
            set { this._LastModifiedByID = value; }
        }

        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        protected String _LastModifiedByScreenID;

        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get { return this._LastModifiedByScreenID; }
            set { this._LastModifiedByScreenID = value; }
        }

        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        protected DateTime? _LastModifiedDateTime;

        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get { return this._LastModifiedDateTime; }
            set { this._LastModifiedDateTime = value; }
        }

        #endregion
	}
}