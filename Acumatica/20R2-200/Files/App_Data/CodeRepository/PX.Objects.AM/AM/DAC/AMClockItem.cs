using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing Clock Transaction
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.ClockItem)]
    [PXPrimaryGraph(typeof(ClockEntry))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMClockItem : IBqlTable, INotable, ILSPrimary
    {
        internal string DebuggerDisplay => $"EmployeeID = {EmployeeID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMClockItem>.By<employeeID>
        {
            public static AMClockItem Find(PXGraph graph, int? employeeID) => FindBy(graph, employeeID);

            public static AMClockItem FindDirty(PXGraph graph, int? employeeID)
                => PXSelect<AMClockItem, Where<AMClockItem.employeeID, Equal<Required<AMClockItem.employeeID>>>>
                    .SelectWindowed(graph, 0, 1, employeeID);
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
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        protected Int32? _BranchID;
        [Branch]
        public virtual Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        protected Int32? _EmployeeID;
        [PXDBInt(IsKey = true)]
        [ProductionEmployeeSelector]
        [PXDefault]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual Int32? EmployeeID
        {
            get
            {
                return this._EmployeeID;
            }
            set
            {
                this._EmployeeID = value;
            }
        }
        #endregion
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [AMTranType.List]
        [PXDefault(typeof(AMTranType.labor))]
        [PXUIField(DisplayName = "Tran. Type", Enabled = false, Visible = false)]
        public virtual String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
        #region LaborType
        public abstract class laborType : PX.Data.BQL.BqlString.Field<laborType> { }

        protected String _LaborType;
        [PXDBString(1, IsFixed = true)]
        [AMLaborType.List]
        [PXUIField(DisplayName = "Labor Type")]
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
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        [PXRestrictor(typeof(Where<AMOrderType.function, NotEqual<OrderTypeFunction.planning>>), Messages.IncorrectOrderTypeFunction)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
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
        [ProductionNbr(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        [PXDefault]
        [ProductionOrderSelector(typeof(orderType), true)]
        [PXFormula(typeof(Validate<orderType>))]
        [PXRestrictor(typeof(Where<AMProdItem.hold, NotEqual<True>,
            And<Where<AMProdItem.statusID, Equal<ProductionOrderStatus.released>,
                Or<AMProdItem.statusID, Equal<ProductionOrderStatus.inProcess>,
                    Or<AMProdItem.statusID, Equal<ProductionOrderStatus.completed>>>>>>),
            Messages.ProdStatusInvalidForProcess, typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID), typeof(AMProdItem.statusID))]
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
        [OperationIDField(PersistingCheck = PXPersistingCheck.Nothing, Required = false)]
        [PXDefault(typeof(Search<
            AMProdOper.operationID,
            Where<AMProdOper.orderType, Equal<Current<orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<prodOrdID>>>>,
            OrderBy<
                Asc<AMProdOper.operationCD>>>))]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        [PXFormula(typeof(Validate<prodOrdID>))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDefault(typeof(Search<AMProdItem.inventoryID,
            Where<AMProdItem.orderType, Equal<Current<AMClockItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMClockItem.prodOrdID>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Inventory(Enabled = false, Required = false)]
        [PXFormula(typeof(Default<AMClockItem.prodOrdID>))]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region LaborCodeID

        public abstract class laborCodeID : PX.Data.BQL.BqlString.Field<laborCodeID> { }

        protected String _LaborCodeID;
        [PXDBString(15, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Labor Code")]
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
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(
            typeof(inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatus.inventoryID, Equal<Optional<inventoryID>>,
                And<INSiteStatus.siteID, Equal<Optional<siteID>>>>>>))]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDefault(typeof(Search<AMProdItem.siteID,
            Where<AMProdItem.orderType, Equal<Current<AMClockItem.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMClockItem.prodOrdID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SiteAvail(typeof(AMClockItem.inventoryID), typeof(AMClockItem.subItemID))]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(inventoryID), typeof(subItemID), typeof(siteID), false, true)]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(uOM), typeof(baseQty), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Quantity")]
        public virtual Decimal? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<AMProdItem.uOM, Where<AMProdItem.prodOrdID, Equal<Current<AMClockItem.prodOrdID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(AMClockItem.inventoryID))]
        [PXFormula(typeof(Default<AMClockItem.prodOrdID>))]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region LotSerFG
        public abstract class lotSerFG : PX.Data.BQL.BqlString.Field<lotSerFG> { }

        protected String _LotSerFG;
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent Lot/Serial Nbr", Visible = false, FieldClass = "LotSerial")]
        public virtual String LotSerFG
        {
            get
            {
                return this._LotSerFG;
            }
            set
            {
                this._LotSerFG = value;
            }
        }
        #endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty")]
        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
        #region Closeflg
        public abstract class closeflg : PX.Data.BQL.BqlBool.Field<closeflg> { }

        protected Boolean? _Closeflg;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Complete")]
        public virtual Boolean? Closeflg
        {
            get
            {
                return this._Closeflg;
            }
            set
            {
                this._Closeflg = value;
            }
        }
        #endregion
        #region StartTime

        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }

        protected DateTime? _StartTime;
        [PXDBDateAndTime(DisplayMask = "t", UseTimeZone = true)]
        [PXUIField(DisplayName = "Start Time", Enabled = false)]
        public virtual DateTime? StartTime
        {
            get
            {
                return this._StartTime;
            }
            set
            {
                this._StartTime = value;
            }
        }
        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }

        protected DateTime? _EndTime;
        [PXDBDateAndTime(DisplayMask = "t", UseTimeZone = true)]
        [PXUIField(DisplayName = "End Time")]
        public virtual DateTime? EndTime
        {
            get
            {
                return this._EndTime;
            }
            set
            {
                this._EndTime = value;
            }
        }
        #endregion
        #region LaborTime
        public abstract class laborTime : PX.Data.BQL.BqlInt.Field<laborTime> { }

        protected Int32? _LaborTime;
        [PXTimeList]
        [PXUIField(DisplayName = "Duration", Enabled = false)]
        [PXUnboundDefault(0)]
        [ClockTime(typeof(startTime),typeof(endTime))]
        public virtual Int32? LaborTime
        {
            get
            {
                return this._LaborTime;
            }
            set
            {
                this._LaborTime = value;
            }
        }
        #endregion
        #region LastOper
        public abstract class lastOper : PX.Data.BQL.BqlBool.Field<lastOper> { }

        protected Boolean? _LastOper;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Is Last Oper")]
        public virtual Boolean? LastOper
        {
            get
            {
                return this._LastOper;
            }
            set
            {
                this._LastOper = value;
            }
        }
        #endregion
        #region LotSerCntr
        public abstract class lotSerCntr : PX.Data.BQL.BqlInt.Field<lotSerCntr> { }

        protected Int32? _LotSerCntr;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "LotSerCntr")]
        public virtual Int32? LotSerCntr
        {
            get
            {
                return this._LotSerCntr;
            }
            set
            {
                this._LotSerCntr = value;
            }
        }
        #endregion
        #region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(inventoryID), typeof(subItemID), typeof(locationID),
            PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String LotSerialNbr
        {
            get
            {
                return this._LotSerialNbr;
            }
            set
            {
                this._LotSerialNbr = value;
            }
        }
        #endregion
        #region MultDiv
        public abstract class multDiv : PX.Data.BQL.BqlString.Field<multDiv> { }

        protected String _MultDiv;
        [PXDBString(1, IsFixed = true)]
        [PXDefault("M")]
        [PXUIField(DisplayName = "MultDiv")]
        public virtual String MultDiv
        {
            get
            {
                return this._MultDiv;
            }
            set
            {
                this._MultDiv = value;
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
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released", Visible = false, Enabled = false)]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region ShiftID
        public abstract class shiftID : PX.Data.BQL.BqlString.Field<shiftID> { }

        protected String _ShiftID;
        [PXDBString(4, InputMask = "####")]
        [PXUIField(DisplayName = "Shift")]
        [PXSelector(typeof(Search<AMShiftMst.shiftID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Multiplier")]
        public virtual Int16? InvtMult
        {
            get
            {
                return this._InvtMult;
            }
            set
            {
                this._InvtMult = value;
            }
        }
        #endregion
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(inventoryID), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
        public virtual DateTime? ExpireDate
        {
            get
            {
                return this._ExpireDate;
            }
            set
            {
                this._ExpireDate = value;
            }
        }
        #endregion
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        protected String _TranDesc;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Tran Description", Visible = false)]
        public virtual String TranDesc
        {
            get
            {
                return this._TranDesc;
            }
            set
            {
                this._TranDesc = value;
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
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        [ProjectBase]
        [ProjectDefault(BatchModule.IN, typeof(Search<AMProdItem.projectID, Where<AMProdItem.orderType, Equal<Current<orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>))]
        [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
        [PXRestrictor(typeof(Where<PMProject.visibleInIN, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
        [PXFormula(typeof(Default<prodOrdID>))]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        [PXDefault(typeof(Search<AMProdItem.taskID, Where<AMProdItem.orderType, Equal<Current<orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<prodOrdID>>, And<AMProdItem.updateProject, Equal<True>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<projectID>))]
        [BaseProjectTask(typeof(projectID))]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
            }
        }
        #endregion
        #region HasMixedProjectTasks
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        protected bool? _HasMixedProjectTasks;
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public virtual bool? HasMixedProjectTasks
        {
            get
            {
                return _HasMixedProjectTasks;
            }
            set
            {
                _HasMixedProjectTasks = value;
            }
        }
        #endregion
        #region UnassignedQty

        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        protected Decimal? _UnassignedQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnassignedQty
        {
            get
            {
                return this._UnassignedQty;
            }
            set
            {
                this._UnassignedQty = value;
            }
        }
        #endregion
        #region IsClockedIn
        public abstract class isClockedIn : PX.Data.BQL.BqlBool.Field<isClockedIn> { }
        [PXBool]
        [PXUIField(DisplayName = "Is Clocked In")]
        [PXDependsOnFields(typeof(startTime), typeof(endTime))]
        public Boolean? IsClockedIn => StartTime != null && EndTime == null;
        #endregion

        #region Methods

        public static implicit operator AMClockItemSplit(AMClockItem item)
        {
            AMClockItemSplit ret = new AMClockItemSplit();
            ret.EmployeeID = item.EmployeeID;
            ret.TranType = item.TranType;
            ret.LineNbr = (int)0;
            ret.SplitLineNbr = (int)1;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ExpireDate = item.ExpireDate;
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault());
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault());
            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }

        public static implicit operator AMClockItem(AMClockItemSplit item)
        {
            AMClockItem ret = new AMClockItem();
            ret.EmployeeID = item.EmployeeID;
            ret.TranType = item.TranType;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;

            //Split recs show a positive qty - AMClockItem shows a positive or negative qty
            ret.Qty = Math.Abs(item.Qty.GetValueOrDefault()) * (item.InvtMult * -1);
            ret.BaseQty = Math.Abs(item.BaseQty.GetValueOrDefault()) * (item.InvtMult * -1);

            ret.UOM = item.UOM;
            ret.TranDate = item.TranDate;
            ret.InvtMult = item.InvtMult;
            ret.Released = item.Released;

            return ret;
        }

        #endregion
    }
}