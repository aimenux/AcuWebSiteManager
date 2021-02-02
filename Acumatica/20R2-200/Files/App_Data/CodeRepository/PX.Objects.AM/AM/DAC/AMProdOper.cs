using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.PO;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM   
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ProductionOper)]
    [PXPrimaryGraph(
        new Type[] { typeof(ProdDetail) },
        new Type[] { typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdOper.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>>>>)})]
    public class AMProdOper : IBqlTable, IOperationMaster, IProdOper, IPhantomBomReference, INotable
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] OperationCD = {OperationCD} ({OperationID}), WcID = {WcID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdOper>.By<orderType, prodOrdID, operationID>
        {
            public static AMProdOper Find(PXGraph graph, string orderType, string prodOrdID, int? operationID) 
                => FindBy(graph, orderType, prodOrdID, operationID);
            public static AMProdOper FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID)
                => PXSelect<AMProdOper, 
                    Where<orderType, Equal<Required<orderType>>, 
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID);
        }

        public class UK : PrimaryKeyOf<AMProdOper>.By<orderType, prodOrdID, operationCD>
        {
            public static AMProdOper Find(PXGraph graph, string orderType, string prodOrdID, string operationCD) => FindBy(graph, orderType, prodOrdID, operationCD);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdOper>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdOper>.By<orderType, prodOrdID> { }
            public class Workcenter : AMWC.PK.ForeignKeyOf<AMProdOper>.By<wcID> { }
        }

        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected string _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.orderType))]
        public virtual string OrderType
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

        protected string _ProdOrdID;
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdOper.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>>>>))]
        [PXParent(typeof(Select<AMProdTotal,
            Where<AMProdTotal.orderType, Equal<Current<AMProdOper.orderType>>,
                And<AMProdTotal.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>>>>))]
        public virtual string ProdOrdID
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
        [PXLineNbr(typeof(AMProdItem.lineCntrOperation))]
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
        #region OperationCD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

        protected string _OperationCD;
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [OperationCDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(AMProdOper.orderType), typeof(AMProdOper.prodOrdID))]
        public virtual string OperationCD
        {
            get { return this._OperationCD; }
            set { this._OperationCD = value; }
        }

        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Operation Description", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected String _WcID;
        [WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<AMBSetup.wcID>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXForeignReference(typeof(Field<AMProdOper.wcID>.IsRelatedTo<AMWC.wcID>))]
        [PXRestrictor(typeof(Where<AMWC.activeFlg, Equal<True>>), Messages.WorkCenterNotActive)]
        public virtual String WcID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
            }
        }
        #endregion
        #region SetupTime
        public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }

        protected Int32? _SetupTime;
        [OperationDBTime]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Setup Time")]
        public virtual Int32? SetupTime
        {
            get
            {
                return this._SetupTime;
            }
            set
            {
                this._SetupTime = value;
            }
        }
        #endregion
        #region RunUnitTime
        public abstract class runUnitTime : PX.Data.BQL.BqlInt.Field<runUnitTime> { }

        protected Int32? _RunUnitTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "60")]
        [PXUIField(DisplayName = "Run Time")]
        public virtual Int32? RunUnitTime
        {
            get
            {
                return this._RunUnitTime;
            }
            set
            {
                this._RunUnitTime = value;
            }
        }
        #endregion
        #region RunUnits
        public abstract class runUnits : PX.Data.BQL.BqlDecimal.Field<runUnits> { }

        protected Decimal? _RunUnits;
        [PXDBQuantity(MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Run Units")]
        public virtual Decimal? RunUnits
        {
            get
            {
                return this._RunUnits;
            }
            set
            {
                this._RunUnits = value;
            }
        }
        #endregion
        #region MachineUnitTime
        public abstract class machineUnitTime : PX.Data.BQL.BqlInt.Field<machineUnitTime> { }

        protected Int32? _MachineUnitTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "60")]
        [PXUIField(DisplayName = "Machine Time")]
        public virtual Int32? MachineUnitTime
        {
            get
            {
                return this._MachineUnitTime;
            }
            set
            {
                this._MachineUnitTime = value;
            }
        }
        #endregion
        #region MachineUnits
        public abstract class machineUnits : PX.Data.BQL.BqlDecimal.Field<machineUnits> { }

        protected Decimal? _MachineUnits;
        [PXDBQuantity(MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Units")]
        public virtual Decimal? MachineUnits
        {
            get
            {
                return this._MachineUnits;
            }
            set
            {
                this._MachineUnits = value;
            }
        }
        #endregion
        #region QueueTime
        public abstract class queueTime : PX.Data.BQL.BqlInt.Field<queueTime> { }

        protected Int32? _QueueTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Queue Time")]
        public virtual Int32? QueueTime
        {
            get
            {
                return this._QueueTime;
            }
            set
            {
                this._QueueTime = value;
            }
        }
        #endregion
        #region MoveTime
        public abstract class moveTime : PX.Data.BQL.BqlInt.Field<moveTime> { }

        protected Int32? _MoveTime;
        [OperationDBTime]
        [PXDefault(0, typeof(AMPSetup.defaultMoveTime))]
        [PXUIField(DisplayName = "Move Time")]
        public virtual Int32? MoveTime
        {
            get
            {
                return this._MoveTime;
            }
            set
            {
                this._MoveTime = value;
            }
        }
        #endregion
        #region StatusID
        public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

        protected String _StatusID;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(Search<AMProdItem.statusID,
            Where<AMProdItem.orderType, Equal<Current<AMProdOper.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdOper.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [ProductionOrderStatus.List]
        public virtual String StatusID
        {
            get
            {
                return this._StatusID;
            }
            set
            {
                this._StatusID = value;
            }
        }
        #endregion
        #region BFlush
        public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

        protected Boolean? _BFlush;
        [PXDBBool]
        [PXDefault(false, typeof(Search<AMWC.bflushLbr, Where<AMWC.wcID, Equal<Current<AMProdOper.wcID>>>>))]
        [PXUIField(DisplayName = "Backflush")]
        public virtual Boolean? BFlush
        {
            get
            {
                return this._BFlush;
            }
            set
            {
                this._BFlush = value;
            }
        }
        #endregion
        #region QtytoProd
        public abstract class qtytoProd : PX.Data.BQL.BqlDecimal.Field<qtytoProd> { }

        protected Decimal? _QtytoProd;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty to Produce", Enabled = false)]
        public virtual Decimal? QtytoProd
        {
            get
            {
                return this._QtytoProd;
            }
            set
            {
                this._QtytoProd = value;
            }
        }
        #endregion
        #region BaseQtytoProd
        public abstract class baseQtytoProd : PX.Data.BQL.BqlDecimal.Field<baseQtytoProd> { }

        protected Decimal? _BaseQtytoProd;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty to Produce", Enabled = false, Visible = false)]
        public virtual Decimal? BaseQtytoProd
        {
            get
            {
                return this._BaseQtytoProd;
            }
            set
            {
                this._BaseQtytoProd = value;
            }
        }
        #endregion
        #region QtyComplete
        public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

        protected Decimal? _QtyComplete;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Complete", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? QtyComplete
        {
            get
            {
                return this._QtyComplete;
            }
            set
            {
                this._QtyComplete = value;
            }
        }
        #endregion
        #region BaseQtyComplete
        public abstract class baseQtyComplete : PX.Data.BQL.BqlDecimal.Field<baseQtyComplete> { }

        protected Decimal? _BaseQtyComplete;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty Complete", Enabled = false, Visible = false)]
        public virtual Decimal? BaseQtyComplete
        {
            get
            {
                return this._BaseQtyComplete;
            }
            set
            {
                this._BaseQtyComplete = value;
            }
        }
        #endregion
        #region QtyRemaining  (Unbound)
        public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

        protected Decimal? _QtyRemaining;
        [PXQuantity]
        [PXFormula(typeof(SubNotLessThanZero<AMProdOper.totalQty, Add<AMProdOper.qtyComplete, AMProdOper.qtyScrapped>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty Remaining", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? QtyRemaining
        {
            get
            {
                return this._QtyRemaining;
            }
            set
            {
                this._QtyRemaining = value;
            }
        }
        #endregion
	    #region BaseQtyRemaining  (Unbound)
	    public abstract class baseQtyRemaining : PX.Data.BQL.BqlDecimal.Field<baseQtyRemaining> { }

	    protected Decimal? _BaseQtyRemaining;
	    [PXQuantity]
	    [PXFormula(typeof(SubNotLessThanZero<AMProdOper.baseTotalQty, Add<AMProdOper.baseQtyComplete, AMProdOper.baseQtyScrapped>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Base Qty Remaining", Enabled = false)]
        public virtual Decimal? BaseQtyRemaining
        {
            get
            {
                return this._BaseQtyRemaining;
            }
            set
            {
                this._BaseQtyRemaining = value;
            }
        }
        #endregion
        #region QtyScrapped
        public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

        protected Decimal? _QtyScrapped;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Scrapped", Enabled = false)]
        public virtual Decimal? QtyScrapped
        {
            get
            {
                return this._QtyScrapped;
            }
            set
            {
                this._QtyScrapped = value;
            }
        }
        #endregion
        #region BaseQtyScrapped
        public abstract class baseQtyScrapped : PX.Data.BQL.BqlDecimal.Field<baseQtyScrapped> { }

        protected Decimal? _BaseQtyScrapped;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty Scrapped", Enabled = false, Visible = false)]
        public virtual Decimal? BaseQtyScrapped
        {
            get
            {
                return this._BaseQtyScrapped;
            }
            set
            {
                this._BaseQtyScrapped = value;
            }
        }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

        protected DateTime? _StartDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Start Date", Enabled = false)]
        public virtual DateTime? StartDate
        {
            get
            {
                return this._StartDate;
            }
            set
            {
                this._StartDate = value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

        protected DateTime? _EndDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "End Date", Enabled = false)]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region ActEndDate
        public abstract class actEndDate : PX.Data.BQL.BqlDateTime.Field<actEndDate> { }

        protected DateTime? _ActEndDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Actual End Date", Visible = false, Enabled = false)]
        public virtual DateTime? ActEndDate
        {
            get
            {
                return this._ActEndDate;
            }
            set
            {
                this._ActEndDate = value;
            }
        }
        #endregion
        #region LineCntrMatl
        public abstract class lineCntrMatl : PX.Data.BQL.BqlInt.Field<lineCntrMatl> { }

        protected Int32? _LineCntrMatl;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrMatl
        {
            get
            {
                return this._LineCntrMatl;
            }
            set
            {
                this._LineCntrMatl = value;
            }
        }
        #endregion
        #region LineCntrOvhd
        public abstract class lineCntrOvhd : PX.Data.BQL.BqlInt.Field<lineCntrOvhd> { }

        protected Int32? _LineCntrOvhd;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrOvhd
        {
            get
            {
                return this._LineCntrOvhd;
            }
            set
            {
                this._LineCntrOvhd = value;
            }
        }
        #endregion
        #region LineCntrStep
        public abstract class lineCntrStep : PX.Data.BQL.BqlInt.Field<lineCntrStep> { }

        protected Int32? _LineCntrStep;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrStep
        {
            get
            {
                return this._LineCntrStep;
            }
            set
            {
                this._LineCntrStep = value;
            }
        }
        #endregion
        #region LineCntrTool
        public abstract class lineCntrTool : PX.Data.BQL.BqlInt.Field<lineCntrTool> { }

        protected Int32? _LineCntrTool;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrTool
        {
            get
            {
                return this._LineCntrTool;
            }
            set
            {
                this._LineCntrTool = value;
            }
        }
        #endregion
        #region ActStartDate
        public abstract class actStartDate : PX.Data.BQL.BqlDateTime.Field<actStartDate> { }

        protected DateTime? _ActStartDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Actual Start Date", Visible = false, Enabled = false)]
        public virtual DateTime? ActStartDate
        {
            get
            {
                return this._ActStartDate;
            }
            set
            {
                this._ActStartDate = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [AutoNote]
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
        #region PhtmPriorLevelQty
        public abstract class phtmPriorLevelQty : PX.Data.BQL.BqlDecimal.Field<phtmPriorLevelQty> { }

        protected decimal? _PhtmPriorLevelQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Phantom Prior Level Qty", Visible = false, Enabled = false)]
        public virtual decimal? PhtmPriorLevelQty
        {
            get
            {
                return this._PhtmPriorLevelQty;
            }
            set
            {
                this._PhtmPriorLevelQty = value;
            }
        }
        #endregion
        #region FirmSchedule
        public abstract class firmSchedule : PX.Data.BQL.BqlBool.Field<firmSchedule> { }

        protected bool? _FirmSchedule;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Firm Schedule", Visible = false, Enabled = false)]
        public virtual bool? FirmSchedule
        {
            get
            {
                return this._FirmSchedule;
            }
            set
            {
                this._FirmSchedule = value;
            }
        }
        #endregion

        //Planned Values
        #region PlanLabor

        public abstract class planLabor : PX.Data.BQL.BqlDecimal.Field<planLabor> { }

        protected Decimal? _PlanLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planLabor>))]
        public virtual Decimal? PlanLabor
        {
            get
            {
                return _PlanLabor;
            }
            set
            {
                _PlanLabor = value;
            }
        }
        #endregion
        #region PlanLaborTime
        public abstract class planLaborTime : PX.Data.BQL.BqlInt.Field<planLaborTime> { }

        protected Int32? _PlanLaborTime;
        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planLaborTime>))]
        public virtual Int32? PlanLaborTime
        {
            get
            {
                return this._PlanLaborTime;
            }
            set
            {
                this._PlanLaborTime = value;
            }
        }
        #endregion
        #region PlanMachine

        public abstract class planMachine : PX.Data.BQL.BqlDecimal.Field<planMachine> { }

        protected Decimal? _PlanMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planMachine>))]
        public virtual Decimal? PlanMachine
        {
            get
            {
                return _PlanMachine;
            }
            set
            {
                _PlanMachine = value;
            }
        }
        #endregion
        #region PlanMachineTime
        public abstract class planMachineTime : PX.Data.BQL.BqlInt.Field<planMachineTime> { }

        protected Int32? _PlanMachineTime;
        [OperationDBTime]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Plan Machine Time", Enabled = false)]
        public virtual Int32? PlanMachineTime
        {
            get
            {
                return this._PlanMachineTime;
            }
            set
            {
                this._PlanMachineTime = value;
            }
        }
        #endregion
        #region PlanMaterial

        public abstract class planMaterial : PX.Data.BQL.BqlDecimal.Field<planMaterial> { }

        protected Decimal? _PlanMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planMaterial>))]
        public virtual Decimal? PlanMaterial
        {
            get
            {
                return _PlanMaterial;
            }
            set
            {
                _PlanMaterial = value;
            }
        }
        #endregion
        #region PlanTool

        public abstract class planTool : PX.Data.BQL.BqlDecimal.Field<planTool> { }

        protected Decimal? _PlanTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planTool>))]
        public virtual Decimal? PlanTool
        {
            get
            {
                return _PlanTool;
            }
            set
            {
                _PlanTool = value;
            }
        }
        #endregion
        #region PlanFixedOverhead

        public abstract class planFixedOverhead : PX.Data.BQL.BqlDecimal.Field<planFixedOverhead> { }

        protected Decimal? _PlanFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planFixedOverhead>))]
        public virtual Decimal? PlanFixedOverhead
        {
            get
            {
                return _PlanFixedOverhead;
            }
            set
            {
                _PlanFixedOverhead = value;
            }
        }
        #endregion
        #region PlanVariableOverhead

        public abstract class planVariableOverhead : PX.Data.BQL.BqlDecimal.Field<planVariableOverhead> { }

        protected Decimal? _PlanVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planVariableOverhead>))]
        public virtual Decimal? PlanVariableOverhead
        {
            get
            {
                return _PlanVariableOverhead;
            }
            set
            {
                _PlanVariableOverhead = value;
            }
        }
        #endregion
        #region PlanSubcontract

        public abstract class planSubcontract : PX.Data.BQL.BqlDecimal.Field<planSubcontract> { }

        protected Decimal? _PlanSubcontract;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planSubcontract>))]
        public virtual Decimal? PlanSubcontract
        {
            get
            {
                return _PlanSubcontract;
            }
            set
            {
                _PlanSubcontract = value;
            }
        }
        #endregion
        #region PlanQtyToProduce

        public abstract class planQtyToProduce : PX.Data.BQL.BqlDecimal.Field<planQtyToProduce> { }

        protected Decimal? _PlanQtyToProduce;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Plan Qty", Enabled = false)]
        public virtual Decimal? PlanQtyToProduce
        {
            get
            {
                return this._PlanQtyToProduce;
            }
            set
            {
                this._PlanQtyToProduce = value;
            }
        }
        #endregion
        #region PlanTotal
        public abstract class planTotal : PX.Data.BQL.BqlDecimal.Field<planTotal> { }

        protected Decimal? _PlanTotal;
        [PXBaseCury]
        [PXFormula(typeof(Add<AMProdOper.planLabor, Add<AMProdOper.planMachine, Add<AMProdOper.planMaterial, Add<AMProdOper.planTool,
            Add<AMProdOper.planFixedOverhead, Add<AMProdOper.planVariableOverhead, AMProdOper.planSubcontract>>>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Planned Total", Enabled = false)]
        public virtual Decimal? PlanTotal
        {
            get
            {
                return _PlanTotal;
            }
            set
            {
                _PlanTotal = value;
            }
        }
        #endregion
        #region Plan Cost Date
        public abstract class planCostDate : PX.Data.BQL.BqlDateTime.Field<planCostDate> { }

        protected DateTime? _PlanCostDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Plan Cost Date", Enabled = false)]
        public virtual DateTime? PlanCostDate
        {
            get
            {
                return this._PlanCostDate;
            }
            set
            {
                this._PlanCostDate = value;
            }
        }
        #endregion
        #region PlanReferenceMaterial
        public abstract class planReferenceMaterial : PX.Data.BQL.BqlDecimal.Field<planReferenceMaterial> { }
        protected Decimal? _PlanReferenceMaterial;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0")]
        [PXUIField(DisplayName = "Ref. Material", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.planReferenceMaterial>))]
        public virtual Decimal? PlanReferenceMaterial
        {
            get
            {
                return this._PlanReferenceMaterial;
            }
            set
            {
                this._PlanReferenceMaterial = value;
            }
        }
        #endregion

        //Actual Values
        #region TotActCost (Obsolete)
        [Obsolete("Use AMProdOper.actualLabor")]
        public abstract class totActCost : PX.Data.BQL.BqlDecimal.Field<totActCost> { }

        protected Decimal? _TotActCost;
        [Obsolete("Use AMProdOper.ActualLabor")]
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Labor Cost Obsolete", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
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
        #region ActualLabor
        public abstract class actualLabor : PX.Data.BQL.BqlDecimal.Field<actualLabor> { }

        protected Decimal? _ActualLabor;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualLabor>))]
        public virtual Decimal? ActualLabor
        {
            get
            {
                return _ActualLabor;
            }
            set
            {
                _ActualLabor = value;
            }
        }
        #endregion
        #region TotHours (Obsolete)
        [Obsolete("Use AMProdOper.actualLaborTime")]
        public abstract class totHours : PX.Data.BQL.BqlDecimal.Field<totHours> { }

        protected Decimal? _TotHours;
        [Obsolete("Use AMProdOper.ActualLaborTime")]
        [PXDBDecimal(3)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Labor Hours Obsolete", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Decimal? TotHours
        {
            get
            {
                return this._TotHours;
            }
            set
            {
                this._TotHours = value;
            }
        }
        #endregion
        #region ActualLaborTime
        public abstract class actualLaborTime : PX.Data.BQL.BqlInt.Field<actualLaborTime> { }

        protected Int32? _ActualLaborTime;
        [ProductionTotalTimeDB]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualLaborTime>))]
        public virtual Int32? ActualLaborTime
        {
            get
            {
                return this._ActualLaborTime;
            }
            set
            {
                this._ActualLaborTime = value;
            }
        }
        #endregion
        #region TotMachCost (Obsolete)
        [Obsolete("Use AMProdOper.actualMachine")]
        public abstract class totMachCost : PX.Data.BQL.BqlDecimal.Field<totMachCost> { }

        protected Decimal? _TotMachCost;
        [Obsolete("Use AMProdOper.ActualMachine")]
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Cost Obsolete", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Decimal? TotMachCost
        {
            get
            {
                return this._TotMachCost;
            }
            set
            {
                this._TotMachCost = value;
            }
        }
        #endregion
        #region ActualMachine
        public abstract class actualMachine : PX.Data.BQL.BqlDecimal.Field<actualMachine> { }

        protected Decimal? _ActualMachine;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualMachine>))]
        public virtual Decimal? ActualMachine
        {
            get
            {
                return _ActualMachine;
            }
            set
            {
                _ActualMachine = value;
            }
        }
        #endregion
        #region TotMachHrs (Obsolete)
        [Obsolete("Use AMProdOper.actualMachineTime")]
        public abstract class totMachHrs : PX.Data.BQL.BqlDecimal.Field<totMachHrs> { }

        protected Decimal? _TotMachHrs;
        [Obsolete("Use AMProdOper.ActualMachineTime")]
        [PXDBDecimal(3)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Hours Obsolete", Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Decimal? TotMachHrs
        {
            get
            {
                return this._TotMachHrs;
            }
            set
            {
                this._TotMachHrs = value;
            }
        }
        #endregion
        #region ActualMachineTime
        public abstract class actualMachineTime : PX.Data.BQL.BqlInt.Field<actualMachineTime> { }

        protected Int32? _ActualMachineTime;
        [OperationDBTime]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Actual Machine Time", Enabled = false)]
        public virtual Int32? ActualMachineTime
        {
            get
            {
                return this._ActualMachineTime;
            }
            set
            {
                this._ActualMachineTime = value;
            }
        }
        #endregion
        #region ActualMaterial

        public abstract class actualMaterial : PX.Data.BQL.BqlDecimal.Field<actualMaterial> { }

        protected Decimal? _ActualMaterial;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualMaterial>))]
        public virtual Decimal? ActualMaterial
        {
            get
            {
                return _ActualMaterial;
            }
            set
            {
                _ActualMaterial = value;
            }
        }
        #endregion
        #region ActualTool

        public abstract class actualTool : PX.Data.BQL.BqlDecimal.Field<actualTool> { }

        protected Decimal? _ActualTool;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualTool>))]
        public virtual Decimal? ActualTool
        {
            get
            {
                return _ActualTool;
            }
            set
            {
                _ActualTool = value;
            }
        }
        #endregion
        #region ActualFixedOverhead

        public abstract class actualFixedOverhead : PX.Data.BQL.BqlDecimal.Field<actualFixedOverhead> { }

        protected Decimal? _ActualFixedOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualFixedOverhead>))]
        public virtual Decimal? ActualFixedOverhead
        {
            get
            {
                return _ActualFixedOverhead;
            }
            set
            {
                _ActualFixedOverhead = value;
            }
        }
        #endregion
        #region ActualVariableOverhead

        public abstract class actualVariableOverhead : PX.Data.BQL.BqlDecimal.Field<actualVariableOverhead> { }

        protected Decimal? _ActualVariableOverhead;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualVariableOverhead>))]
        public virtual Decimal? ActualVariableOverhead
        {
            get
            {
                return _ActualVariableOverhead;
            }
            set
            {
                _ActualVariableOverhead = value;
            }
        }
        #endregion
        #region ActualSubcontract

        public abstract class actualSubcontract : PX.Data.BQL.BqlDecimal.Field<actualSubcontract> { }

        protected Decimal? _ActualSubcontract;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.actualSubcontract>))]
        public virtual Decimal? ActualSubcontract
        {
            get
            {
                return _ActualSubcontract;
            }
            set
            {
                _ActualSubcontract = value;
            }
        }
        #endregion
        #region ScrapAmount
        public abstract class scrapAmount : PX.Data.BQL.BqlDecimal.Field<scrapAmount> { }

        protected Decimal? _ScrapAmount;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Scrap", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.scrapAmount>))]
        public virtual Decimal? ScrapAmount
        {
            get
            {
                return _ScrapAmount;
            }
            set
            {
                _ScrapAmount = value;
            }
        }
        #endregion
	    #region WIPAdjustment
	    public abstract class wIPAdjustment : PX.Data.BQL.BqlDecimal.Field<wIPAdjustment> { }

	    protected Decimal? _WIPAdjustment;
	    [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Adjustments", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMProdTotal.wIPAdjustment>))]
        public virtual Decimal? WIPAdjustment
        {
            get
            {
                return this._WIPAdjustment;
            }
            set
            {
                this._WIPAdjustment = value;
            }
        }
        #endregion
        #region WIPTotal
        public abstract class wIPTotal : PX.Data.BQL.BqlDecimal.Field<wIPTotal> { }

        [PXUIField(DisplayName = "WIP Total", Enabled = false)]
        [PXBaseCury]
        //Necessary to use formula in property return vs formula as formula doesn't always fire correctly
        [PXFormula(null, typeof(SumCalc<AMProdItem.wIPTotal>))]
        [PXDependsOnFields(typeof(AMProdOper.actualLabor), typeof(AMProdOper.actualMachine), typeof(AMProdOper.actualMaterial),
            typeof(AMProdOper.actualTool), typeof(AMProdOper.actualFixedOverhead), typeof(AMProdOper.actualVariableOverhead),
            typeof(AMProdOper.wIPAdjustment), typeof(AMProdOper.scrapAmount), typeof(AMProdOper.actualSubcontract))]
        public virtual Decimal? WIPTotal => ActualLabor.GetValueOrDefault() +
                                            ActualMachine.GetValueOrDefault() +
                                            ActualMaterial.GetValueOrDefault() +
                                            ActualTool.GetValueOrDefault() +
                                            ActualFixedOverhead.GetValueOrDefault() +
                                            ActualVariableOverhead.GetValueOrDefault() +
                                            ActualSubcontract.GetValueOrDefault() +
                                            WIPAdjustment.GetValueOrDefault() -
                                            ScrapAmount.GetValueOrDefault();

        #endregion
        #region WIPComp
        public abstract class wIPComp : PX.Data.BQL.BqlDecimal.Field<wIPComp> { }

        protected Decimal? _WIPComp;
        [PXUIField(DisplayName = "MFG to Inventory", Enabled = false)]
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(null, typeof(SumCalc<AMProdItem.wIPComp>))]
        public virtual Decimal? WIPComp
        {
            get
            {
                return this._WIPComp;
            }
            set
            {
                this._WIPComp = value;
            }
        }
        #endregion
        #region HasActualCost
        public abstract class hasActualCost : PX.Data.BQL.BqlBool.Field<hasActualCost> { }

        [PXBool]
        [PXUIField(DisplayName = "Has Actual cost", Enabled = false, Visible = false)]
        //Not wired to WIPTotal as the +/- could result in a zero - but we want to show costs are against operation
        [PXDependsOnFields(typeof(AMProdOper.actualLabor), typeof(AMProdOper.actualMachine), typeof(AMProdOper.actualMaterial),
            typeof(AMProdOper.actualTool), typeof(AMProdOper.actualFixedOverhead), typeof(AMProdOper.actualVariableOverhead),
            typeof(AMProdOper.wIPAdjustment), typeof(AMProdOper.wIPComp), typeof(AMProdOper.scrapAmount))]
        public virtual Boolean? HasActualCost => ActualLabor.GetValueOrDefault() != 0
                                                 || ActualMachine.GetValueOrDefault() != 0
                                                 || ActualMaterial.GetValueOrDefault() != 0
                                                 || ActualTool.GetValueOrDefault() != 0
                                                 || ActualFixedOverhead.GetValueOrDefault() != 0
                                                 || ActualVariableOverhead.GetValueOrDefault() != 0
                                                 || WIPAdjustment.GetValueOrDefault() != 0
                                                 || WIPComp.GetValueOrDefault() != 0
                                                 || ScrapAmount.GetValueOrDefault() != 0;

        #endregion

        //Variance Values
        #region VarianceLabor (Unbound)

        public abstract class varianceLabor : PX.Data.BQL.BqlDecimal.Field<varianceLabor> { }

        protected Decimal? _VarianceLabor;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualLabor, AMProdOper.planLabor>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor", Enabled = false)]
        public virtual Decimal? VarianceLabor
        {
            get
            {
                return _VarianceLabor;
            }
            set
            {
                _VarianceLabor = value;
            }
        }
        #endregion
        #region VarianceLaborTime (Unbound)
        public abstract class varianceLaborTime : PX.Data.BQL.BqlInt.Field<varianceLaborTime> { }

        [ProductionTotalTimeNonDB]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Labor Time", Enabled = false)]
        [PXDependsOnFields(typeof(AMProdOper.actualLaborTime), typeof(AMProdOper.planLaborTime))]
        public virtual Int32? VarianceLaborTime
        {
            get
            {
                // The display format doesn't work for negative time values. Work around to make sure value will not calculate as negative.
                return Math.Abs(ActualLaborTime.GetValueOrDefault() - PlanLaborTime.GetValueOrDefault());
            }
        }
        #endregion
        #region VarianceMachine (Unbound)

        public abstract class varianceMachine : PX.Data.BQL.BqlDecimal.Field<varianceMachine> { }

        protected Decimal? _VarianceMachine;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualMachine, AMProdOper.planMachine>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? VarianceMachine
        {
            get
            {
                return _VarianceMachine;
            }
            set
            {
                _VarianceMachine = value;
            }
        }
        #endregion
        #region VarianceMaterial   (Unbound)

        public abstract class varianceMaterial : PX.Data.BQL.BqlDecimal.Field<varianceMaterial> { }

        protected Decimal? _VarianceMaterial;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualMaterial, AMProdOper.planMaterial>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        public virtual Decimal? VarianceMaterial
        {
            get
            {
                return _VarianceMaterial;
            }
            set
            {
                _VarianceMaterial = value;
            }
        }
        #endregion
        #region VarianceTool   (Unbound)

        public abstract class varianceTool : PX.Data.BQL.BqlDecimal.Field<varianceTool> { }

        protected Decimal? _VarianceTool;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualTool, AMProdOper.planTool>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tool", Enabled = false)]
        public virtual Decimal? VarianceTool
        {
            get
            {
                return _VarianceTool;
            }
            set
            {
                _VarianceTool = value;
            }
        }
        #endregion
        #region VarianceFixedOverhead (Unbound)

        public abstract class varianceFixedOverhead : PX.Data.BQL.BqlDecimal.Field<varianceFixedOverhead> { }

        protected Decimal? _VarianceFixedOverhead;
        [PXBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        [PXFormula(typeof(Sub<AMProdOper.actualFixedOverhead, AMProdOper.planFixedOverhead>))]
        public virtual Decimal? VarianceFixedOverhead
        {
            get
            {
                return _VarianceFixedOverhead;
            }
            set
            {
                _VarianceFixedOverhead = value;
            }
        }
        #endregion
        #region VarianceVariableOverhead (Unbound)

        public abstract class varianceVariableOverhead : PX.Data.BQL.BqlDecimal.Field<varianceVariableOverhead> { }

        protected Decimal? _VarianceVariableOverhead;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualVariableOverhead, AMProdOper.planVariableOverhead>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? VarianceVariableOverhead
        {
            get
            {
                return _VarianceVariableOverhead;
            }
            set
            {
                _VarianceVariableOverhead = value;
            }
        }
        #endregion
        #region VarianceSubcontract (Unbound)

        public abstract class varianceSubcontract : PX.Data.BQL.BqlDecimal.Field<varianceSubcontract> { }

        protected Decimal? _VarianceSubcontract;
        [PXBaseCury]
        [PXFormula(typeof(Sub<AMProdOper.actualSubcontract, AMProdOper.planSubcontract>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        public virtual Decimal? VarianceSubcontract
        {
            get
            {
                return _VarianceSubcontract;
            }
            set
            {
                _VarianceSubcontract = value;
            }
        }
        #endregion
        #region VarianceTotal (Unbound)

        public abstract class varianceTotal : PX.Data.BQL.BqlDecimal.Field<varianceTotal> { }

        protected Decimal? _VarianceTotal;
        [PXBaseCury]
        [PXFormula(typeof(Add<AMProdOper.varianceLabor, Add<AMProdOper.varianceMachine, Add<AMProdOper.varianceMaterial,
            Add<AMProdOper.varianceTool, Add<AMProdOper.varianceFixedOverhead, Add<AMProdOper.varianceVariableOverhead, AMProdOper.varianceSubcontract>>>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Total Variance", Enabled = false)]
        public virtual Decimal? VarianceTotal
        {
            get
            {
                return _VarianceTotal;
            }
            set
            {
                _VarianceTotal = value;
            }
        }
        #endregion
        #region WIPBalance (Unbound)
        public abstract class wIPBalance : PX.Data.BQL.BqlDecimal.Field<wIPBalance> { }

        protected Decimal? _WIPBalance;
        [PXBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "WIP Balance", Enabled = false)]
        [PXDependsOnFields(typeof(AMProdOper.wIPTotal), typeof(AMProdOper.wIPComp))]
        public virtual Decimal? WIPBalance => WIPTotal.GetValueOrDefault() - WIPComp.GetValueOrDefault();
        #endregion
        #region WaitTime
        public abstract class waitTime : PX.Data.BQL.BqlInt.Field<waitTime> { }

        protected Int32? _WaitTime;
        [OperationDBTime]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Wait Time", Enabled = false)]
        public virtual Int32? WaitTime
        {
            get
            {
                return this._WaitTime;
            }
            set
            {
                this._WaitTime = value;
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
        #region TotalQty
        public abstract class totalQty : PX.Data.BQL.BqlDecimal.Field<totalQty> { }

        protected Decimal? _TotalQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Qty", Enabled = false, Visible = false)]
        public virtual Decimal? TotalQty
        {
            get
            {
                return this._TotalQty;
            }
            set
            {
                this._TotalQty = value;
            }
        }
        #endregion
        #region BaseTotalQty
        public abstract class baseTotalQty : PX.Data.BQL.BqlDecimal.Field<baseTotalQty> { }

        protected Decimal? _BaseTotalQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Total Qty", Enabled = false, Visible = false)]
        public virtual Decimal? BaseTotalQty
        {
            get
            {
                return this._BaseTotalQty;
            }
            set
            {
                this._BaseTotalQty = value;
            }
        }
        #endregion
        #region ScrapAction
        public abstract class scrapAction : PX.Data.BQL.BqlInt.Field<scrapAction> { }

        protected int? _ScrapAction;
        [PXDBInt]
        [PXDefault(Attributes.ScrapAction.NoAction, typeof(Search<AMWC.scrapAction, Where<AMWC.wcID,
            Equal<Current<AMProdOper.wcID>>>>))]
        [PXUIField(DisplayName = "Scrap Action")]
        [ScrapAction.List]
        public virtual int? ScrapAction
        {
            get
            {
                return this._ScrapAction;
            }
            set
            {
                this._ScrapAction = value;
            }
        }
        #endregion

        //Outside Processing Values
        #region OutsideProcess
        public abstract class outsideProcess : PX.Data.BQL.BqlBool.Field<outsideProcess> { }

        protected Boolean? _OutsideProcess;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Outside Process")]
        public virtual Boolean? OutsideProcess
        {
            get
            {
                return this._OutsideProcess;
            }
            set
            {
                this._OutsideProcess = value;
            }
        }
        #endregion
        #region DropShippedToVendor
        public abstract class dropShippedToVendor : PX.Data.BQL.BqlBool.Field<dropShippedToVendor> { }

        protected Boolean? _DropShippedToVendor;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Drop Shipped to Vendor")]
        public virtual Boolean? DropShippedToVendor
        {
            get
            {
                return this._DropShippedToVendor;
            }
            set
            {
                this._DropShippedToVendor = value;
            }
        }
        #endregion
        #region VendorID

        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [POVendor(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true)]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region VendorLocationID

        public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
        protected Int32? _VendorLocationID;
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AMProdOper.vendorID>>,
            And<Location.isActive, Equal<True>,
            And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, 
            DisplayName = "Vendor Location")]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<AMProdOper.vendorID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<AMProdOper.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMProdOper.vendorID>))]
        [PXForeignReference(typeof(Field<vendorLocationID>.IsRelatedTo<Location.locationID>))]
        public virtual Int32? VendorLocationID
        {
            get
            {
                return this._VendorLocationID;
            }
            set
            {
                this._VendorLocationID = value;
            }
        }
        #endregion
        #region POOrderNbr

        public abstract class pOOrderNbr : PX.Data.BQL.BqlString.Field<pOOrderNbr> { }
        protected String _POOrderNbr;

        [PXDBString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "PO Order Nbr.", Enabled = false)]
        public virtual String POOrderNbr
        {
            get
            {
                return this._POOrderNbr;
            }
            set
            {
                this._POOrderNbr = value;
            }
        }
        #endregion
        #region POLineNbr

        public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
        protected Int32? _POLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "PO Line Nbr.", Enabled = false)]
        public virtual Int32? POLineNbr
        {
            get
            {
                return this._POLineNbr;
            }
            set
            {
                this._POLineNbr = value;
            }
        }
        #endregion
        #region ShippedQuantity
        public abstract class shippedQuantity : PX.Data.BQL.BqlDecimal.Field<shippedQuantity> { }

        protected Decimal? _ShippedQuantity;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Shipped Quantity", Enabled = false)]
        public virtual Decimal? ShippedQuantity
        {
            get
            {
                return this._ShippedQuantity;
            }
            set
            {
                this._ShippedQuantity = value;
            }
        }
        #endregion
        #region BaseShippedQuantity
        public abstract class baseShippedQuantity : PX.Data.BQL.BqlDecimal.Field<baseShippedQuantity> { }

        protected Decimal? _BaseShippedQuantity;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Shipped Quantity", Enabled = false)]
        public virtual Decimal? BaseShippedQuantity
        {
            get
            {
                return this._BaseShippedQuantity;
            }
            set
            {
                this._BaseShippedQuantity = value;
            }
        }
        #endregion
        #region ShipRemainingQty
        public abstract class shipRemainingQty : PX.Data.BQL.BqlDecimal.Field<shipRemainingQty> { }

        protected Decimal? _ShipRemainingQty;
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ship Remaining Qty", Enabled = false)]
        [PXFormula(typeof(SubNotLessThanZero<AMProdOper.qtytoProd, AMProdOper.shippedQuantity>))]
        public virtual Decimal? ShipRemainingQty
        {
            get
            {
                return this._ShipRemainingQty;
            }
            set
            {
                this._ShipRemainingQty = value;
            }
        }
        #endregion
        #region BaseShipRemainingQty
        public abstract class baseShipRemainingQty : PX.Data.BQL.BqlDecimal.Field<baseShipRemainingQty> { }

        protected Decimal? _BaseShipRemainingQty;
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Ship Remaining Qty", Enabled = false)]
        [PXFormula(typeof(SubNotLessThanZero<AMProdOper.baseQtytoProd, AMProdOper.baseShippedQuantity>))]
        public virtual Decimal? BaseShipRemainingQty
        {
            get
            {
                return this._BaseShipRemainingQty;
            }
            set
            {
                this._BaseShipRemainingQty = value;
            }
        }
        #endregion
        #region AtVendorQuantity
        public abstract class atVendorQuantity : PX.Data.BQL.BqlDecimal.Field<atVendorQuantity> { }

        protected Decimal? _AtVendorQuantity;
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "At Vendor Quantity", Enabled = false)]
        [PXFormula(typeof(SubNotLessThanZero<AMProdOper.shippedQuantity, AMProdOper.qtyComplete>))]
        public virtual Decimal? AtVendorQuantity
        {
            get
            {
                return this._AtVendorQuantity;
            }
            set
            {
                this._AtVendorQuantity = value;
            }
        }
        #endregion
        #region BaseAtVendorQuantity
        public abstract class baseAtVendorQuantity : PX.Data.BQL.BqlDecimal.Field<baseAtVendorQuantity> { }

        protected Decimal? _BaseAtVendorQuantity;
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base At Vendor Quantity", Enabled = false)]
        [PXFormula(typeof(SubNotLessThanZero<AMProdOper.baseShippedQuantity, AMProdOper.baseQtyComplete>))]
        public virtual Decimal? BaseAtVendorQuantity
        {
            get
            {
                return this._BaseAtVendorQuantity;
            }
            set
            {
                this._BaseAtVendorQuantity = value;
            }
        }
        #endregion

        #region BaseMaterialQty (Unbound)
        /// <summary>
        /// Unbound field used only during the processing of operation status
        /// </summary>
        public abstract class baseMaterialQty : PX.Data.BQL.BqlDecimal.Field<baseMaterialQty> { }

        /// <summary>
        /// Unbound field used only during the processing of operation status
        /// </summary>
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Total Qty", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual Decimal? BaseMaterialQty { get; set; }

        #endregion
    }
}
