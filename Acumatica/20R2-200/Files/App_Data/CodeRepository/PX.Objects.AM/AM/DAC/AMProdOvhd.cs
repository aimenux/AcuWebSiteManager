using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.ProductionOvhd)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXPrimaryGraph(
	    new Type[] { typeof(ProdDetail) },
	    new Type[] { typeof(Select<AMProdItem,
	        Where<AMProdItem.orderType, Equal<Current<AMProdOvhd.orderType>>,
	            And<AMProdItem.prodOrdID, Equal<Current<AMProdOvhd.prodOrdID>>>>>)})]
    public class AMProdOvhd : IBqlTable, IProdOper, IPhantomBomReference, INotable
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] OperationID = {OperationID}, LineID = {LineID}, OvhdID = {OvhdID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdOvhd>.By<orderType, prodOrdID, operationID, lineID>
        {
            public static AMProdOvhd Find(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID) 
                => FindBy(graph, orderType, prodOrdID, operationID, lineID);
            public static AMProdOvhd FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID)
                => PXSelect<AMProdOvhd,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID, lineID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdOvhd>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdOvhd>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMProdOvhd>.By<orderType, prodOrdID, operationID> { }
            public class Overhead : AMOverhead.PK.ForeignKeyOf<AMProdOvhd>.By<ovhdID> { }
        }

        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.orderType))]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.prodOrdID))]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
	    #region OperationID
	    public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

	    protected int? _OperationID;
	    [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
	    [PXDBDefault(typeof(AMProdOper.operationID))]
	    [PXParent(typeof(Select<AMProdOper,
	        Where<AMProdOper.orderType, Equal<Current<AMProdOvhd.orderType>>,
	            And<AMProdOper.prodOrdID, Equal<Current<AMProdOvhd.prodOrdID>>,
	                And<AMProdOper.operationID, Equal<Current<AMProdOvhd.operationID>>>>>>))]
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
        [PXUIField(DisplayName = "Line ID", Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMProdOper.lineCntrOvhd))]
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
        #region OvhdID
        public abstract class ovhdID : PX.Data.BQL.BqlString.Field<ovhdID> { }

        protected String _OvhdID;
        [OverheadIDField(Visibility = PXUIVisibility.SelectorVisible)]
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
        #region OFactor
        public abstract class oFactor : PX.Data.BQL.BqlDecimal.Field<oFactor> { }

        protected Decimal? _OFactor;
        [PXDBDecimal(3)]
        [PXDefault(TypeCode.Decimal,"0.0")]
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
        #region TotActCost
        public abstract class totActCost : PX.Data.BQL.BqlDecimal.Field<totActCost> { }

        protected Decimal? _TotActCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Actual Cost", Enabled=false)]
        public virtual Decimal? TotActCost
        {
            get
            {
                return this._TotActCost;
            }
            set
            {
                this._TotActCost = value;
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
        #region WCFlag
        public abstract class wCFlag : PX.Data.BQL.BqlBool.Field<wCFlag> { }

        protected Boolean? _WCFlag;
        [PXDBBool]
        [PXDefault(false)]
		[PXUIField(DisplayName = "WC Flag", Visible = true, Enabled = false)]
        public virtual Boolean? WCFlag
		{
			get
			{
				return this._WCFlag;
			}
			set
			{
				this._WCFlag = value;
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
