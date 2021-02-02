using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.IN;
using CRLocation = PX.Objects.CR.Standalone.Location;
using PX.Objects.PO;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateOperations)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMEstimateOper : IBqlTable, IOperationMaster, IEstimateOper, INotable
    {
        internal string DebuggerDisplay => $"EstimateID = {EstimateID}, RevisionID = {RevisionID}, OperationCD = {OperationCD} ({OperationID}), WorkCenterID = {WorkCenterID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateOper>.By<estimateID, revisionID, operationID>
        {
            public static AMEstimateOper Find(PXGraph graph, string estimateID, string revisionID, int? operationID)
                => FindBy(graph, estimateID, revisionID, operationID);
            public static AMEstimateOper FindDirty(PXGraph graph, string estimateID, string prodOrdID, int? operationID)
                => PXSelect<AMEstimateOper,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, prodOrdID, operationID);
        }

        public class UK : PrimaryKeyOf<AMEstimateOper>.By<estimateID, revisionID, operationCD>
        {
            public static AMEstimateOper Find(PXGraph graph, string estimateID, string revisionID, string operationCD) => FindBy(graph, estimateID, revisionID, operationCD);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateOper>.By<estimateID, revisionID> { }
            public class Workcenter : AMWC.PK.ForeignKeyOf<AMEstimateOper>.By<workCenterID> { }
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
        #region Estimate ID

        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }


        protected String _EstimateID;

        [PXDBDefault(typeof(AMEstimateItem.estimateID))]
        [EstimateID(IsKey = true)]
        [EstimateIDSelectPrimary(ValidateValue = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }


        protected String _RevisionID;

        [PXDBDefault(typeof(AMEstimateItem.revisionID))]
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateOper.estimateID>>>>),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.isPrimary), ValidateValue = false)]
        [PXParent(typeof(Select<AMEstimateItem, Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateOper.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateOper.revisionID>>>>>))]
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
        public virtual Int32? OperationID
        {
            get { return this._OperationID; }
            set { this._OperationID = value; }
        }

        #endregion
        #region Operation CD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

        protected String _OperationCD;
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [OperationCDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXCheckUnique(typeof(AMEstimateOper.estimateID), typeof(AMEstimateOper.revisionID))]
        public virtual String OperationCD
        {
            get { return this._OperationCD; }
            set { this._OperationCD = value; }
        }

        #endregion
        #region Base Order Qty
        public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }

        protected Decimal? _BaseOrderQty;

        [PXDBQuantity]
        [PXDBDefault(typeof(AMEstimateItem.baseOrderQty))]
        [PXUIField(DisplayName = "Base Order Qty", Enabled = false, Visible = false)]
        [TriggerChildFormula(typeof(AMEstimateMatl), typeof(AMEstimateMatl.batchSize))]
        public virtual Decimal? BaseOrderQty
        {
            get { return this._BaseOrderQty; }
            set { this._BaseOrderQty = value; }
        }

        #endregion
        #region Work Center ID
        public abstract class workCenterID : PX.Data.BQL.BqlString.Field<workCenterID> { }

        protected String _WorkCenterID;
        [WorkCenterIDField(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(typeof(Search<AMEstimateSetup.defaultWorkCenterID>))]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        [PXForeignReference(typeof(Field<AMEstimateOper.workCenterID>.IsRelatedTo<AMWC.wcID>))]
        [PXRestrictor(typeof(Where<AMWC.activeFlg, Equal<True>>), Messages.WorkCenterNotActive)]
        public virtual String WorkCenterID
        {
            get { return this._WorkCenterID; }
            set { this._WorkCenterID = value; }
        }

        #endregion
        #region WcID (Unbound for IOperationMaster)
        [PXString(20, IsUnicode = true)]
        [PXUIField(DisplayName = "Work Center", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual String WcID
        {
            get
            {
                return this._WorkCenterID;
            }
            set
            {
                this._WorkCenterID = value;
            }
        }
        #endregion
        #region Work Center Standard Cost
        public abstract class workCenterStdCost : PX.Data.BQL.BqlDecimal.Field<workCenterStdCost> { }

        protected Decimal? _WorkCenterStdCost;

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Work Center Standard Cost", Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<AMEstimateOper.workCenterID, AMWC.stdCost>))]
        public virtual Decimal? WorkCenterStdCost
        {
            get { return this._WorkCenterStdCost; }
            set { this._WorkCenterStdCost = value; }
        }

        #endregion
        #region Machine Standard Cost
        public abstract class machineStdCost : PX.Data.BQL.BqlDecimal.Field<machineStdCost> { }

        protected Decimal? _MachineStdCost;

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine StdCost", Visible = false, Enabled = false)]
        public virtual Decimal? MachineStdCost
        {
            get { return this._MachineStdCost; }
            set { this._MachineStdCost = value; }
        }

        #endregion
        #region Line Cntr Matl
        public abstract class lineCntrMatl : PX.Data.BQL.BqlInt.Field<lineCntrMatl> { }

        protected Int32? _LineCntrMatl;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrMatl
        {
            get { return this._LineCntrMatl; }
            set { this._LineCntrMatl = value; }
        }

        #endregion
        #region Line Cntr Ovhd
        public abstract class lineCntrOvhd : PX.Data.BQL.BqlInt.Field<lineCntrOvhd> { }


        protected Int32? _LineCntrOvhd;

        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrOvhd
        {
            get { return this._LineCntrOvhd; }
            set { this._LineCntrOvhd = value; }
        }

        #endregion
        #region Line Cntr Tool
        public abstract class lineCntrTool : PX.Data.BQL.BqlInt.Field<lineCntrTool> { }


        protected Int32? _LineCntrTool;

        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrTool
        {
            get { return this._LineCntrTool; }
            set { this._LineCntrTool = value; }
        }

        #endregion
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }


        protected String _Description;

        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Operation Desc")]
        [PXDefault(typeof(Search<AMWC.descr, Where<AMWC.wcID, Equal<Current<AMEstimateOper.workCenterID>>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMEstimateOper.workCenterID>))]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }

        #endregion
        #region SetupTime
        public abstract class setupTime : PX.Data.BQL.BqlInt.Field<setupTime> { }

        protected Int32? _SetupTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
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
        #region RunUnitsPerHour
        public abstract class runUnitsPerHour : PX.Data.BQL.BqlDecimal.Field<runUnitsPerHour> { }

        protected Decimal? _RunUnitsPerHour;
        [PXDecimal(10)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Units Per Hour", Enabled = false, Visible = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.runUnitTime, NotEqual<int0>>, Mult<Common.BQLConstants.decimal60,
            Div<AMEstimateOper.runUnits, AMEstimateOper.runUnitTime>>>, decimal0>))]
        public virtual Decimal? RunUnitsPerHour
        {
            get
            {
                return this._RunUnitsPerHour;
            }
            set
            {
                this._RunUnitsPerHour = value;
            }
        }
        #endregion
        #region Run Time Hours
        public abstract class runTimeHours : PX.Data.BQL.BqlDecimal.Field<runTimeHours> { }

        protected Decimal? _RunTimeHours;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Run Time Hours")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.runUnitsPerHour, NotEqual<decimal0>>, 
            Div<AMEstimateOper.baseOrderQty, AMEstimateOper.runUnitsPerHour>>, decimal0>))]
        public virtual Decimal? RunTimeHours
        {
            get { return this._RunTimeHours; }
            set { this._RunTimeHours = value; }
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
        #region MachineUnitsPerHour
        public abstract class machineUnitsPerHour : PX.Data.BQL.BqlDecimal.Field<machineUnitsPerHour> { }

        protected Decimal? _MachineUnitsPerHour;
        [PXDecimal(10)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Machine Units Per Hour", Enabled = false, Visible = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.machineUnitTime, NotEqual<int0>>, Mult<Common.BQLConstants.decimal60,
            Div<AMEstimateOper.machineUnits, AMEstimateOper.machineUnitTime>>>, decimal0>))]
        public virtual Decimal? MachineUnitsPerHour
        {
            get
            {
                return this._MachineUnitsPerHour;
            }
            set
            {
                this._MachineUnitsPerHour = value;
            }
        }
        #endregion
        #region Machine Time Hours
        public abstract class machineTimeHours : PX.Data.BQL.BqlDecimal.Field<machineTimeHours> { }

        protected Decimal? _MachineTimeHours;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine Time Hours")]
        [TriggerEstimateOvhdFormula]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.machineUnitsPerHour, NotEqual<decimal0>>,
            Div<AMEstimateOper.baseOrderQty, AMEstimateOper.machineUnitsPerHour>>, decimal0>))]
        public virtual Decimal? MachineTimeHours
        {
            get { return this._MachineTimeHours; }
            set { this._MachineTimeHours = value; }
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
        #region Back Flush Labor
        public abstract class backFlushLabor : PX.Data.BQL.BqlBool.Field<backFlushLabor> { }

        protected Boolean? _BackFlushLabor;

        [PXDBBool]
        [PXDefault(false, typeof(Search<AMWC.bflushLbr, Where<AMWC.wcID, Equal<Current<AMEstimateOper.workCenterID>>>>)
        )]
        [PXUIField(DisplayName = "Backflush Labor")]
        public virtual Boolean? BackFlushLabor
        {
            get { return this._BackFlushLabor; }
            set { this._BackFlushLabor = value; }
        }

        #endregion
        #region Fixed Labor Override
        public abstract class fixedLaborOverride : PX.Data.BQL.BqlBool.Field<fixedLaborOverride> { }


        protected Boolean? _FixedLaborOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? FixedLaborOverride
        {
            get { return this._FixedLaborOverride; }
            set { this._FixedLaborOverride = value; }
        }

        #endregion
        #region Fixed Labor Calculated Cost
        public abstract class fixedLaborCalcCost : PX.Data.BQL.BqlDecimal.Field<fixedLaborCalcCost> { }

        protected Decimal? _FixedLaborCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(Mult<AMEstimateOper.workCenterStdCost,
            Div<Mult<decimal1 /*Make sure Div is in minutes*/, AMEstimateOper.setupTime>,Common.BQLConstants.decimal60>>))]
        public virtual Decimal? FixedLaborCalcCost
        {
            get { return this._FixedLaborCalcCost; }
            set { this._FixedLaborCalcCost = value; }
        }
        #endregion
        #region Fixed Labor Cost
        public abstract class fixedLaborCost : PX.Data.BQL.BqlDecimal.Field<fixedLaborCost> { }

        protected Decimal? _FixedLaborCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Labor Cost")]
        [TriggerEstimateOvhdFormula]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.fixedLaborOverride, Equal<True>>, AMEstimateOper.fixedLaborCost>,
            AMEstimateOper.fixedLaborCalcCost>), typeof(SumCalc<AMEstimateItem.fixedLaborCalcCost>))]
        public virtual Decimal? FixedLaborCost
        {
            get { return this._FixedLaborCost; }
            set { this._FixedLaborCost = value; }
        }

        #endregion
        #region Variable Labor Override
        public abstract class variableLaborOverride : PX.Data.BQL.BqlBool.Field<variableLaborOverride> { }


        protected Boolean? _VariableLaborOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? VariableLaborOverride
        {
            get { return this._VariableLaborOverride; }
            set { this._VariableLaborOverride = value; }
        }

        #endregion
        #region Variable Labor Calculated Cost
        public abstract class variableLaborCalcCost : PX.Data.BQL.BqlDecimal.Field<variableLaborCalcCost> { }

        protected Decimal? _VariableLaborCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(Mult<AMEstimateOper.runTimeHours, AMEstimateOper.workCenterStdCost>))]
        public virtual Decimal? VariableLaborCalcCost
        {
            get { return this._VariableLaborCalcCost; }
            set { this._VariableLaborCalcCost = value; }
        }

        #endregion
        #region Variable Labor Cost
        public abstract class variableLaborCost : PX.Data.BQL.BqlDecimal.Field<variableLaborCost> { }

        protected Decimal? _VariableLaborCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Var Labor Cost")]
        [TriggerEstimateOvhdFormula]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.variableLaborOverride, Equal<True>>, AMEstimateOper.variableLaborCost>,
            AMEstimateOper.variableLaborCalcCost>), typeof(SumCalc<AMEstimateItem.variableLaborCalcCost>))]
        public virtual Decimal? VariableLaborCost
        {
            get { return this._VariableLaborCost; }
            set { this._VariableLaborCost = value; }
        }

        #endregion
        #region Machine Override
        public abstract class machineOverride : PX.Data.BQL.BqlBool.Field<machineOverride> { }


        protected Boolean? _MachineOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? MachineOverride
        {
            get { return this._MachineOverride; }
            set { this._MachineOverride = value; }
        }

        #endregion
        #region Machine Calculated Cost
        public abstract class machineCalcCost : PX.Data.BQL.BqlDecimal.Field<machineCalcCost> { }

        protected Decimal? _MachineCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(Mult<AMEstimateOper.machineStdCost, AMEstimateOper.machineTimeHours>))]
        public virtual Decimal? MachineCalcCost
        {
            get { return this._MachineCalcCost; }
            set { this._MachineCalcCost = value; }
        }

        #endregion
        #region Machine Cost
        public abstract class machineCost : PX.Data.BQL.BqlDecimal.Field<machineCost> { }

        protected Decimal? _MachineCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.machineOverride, Equal<True>>, AMEstimateOper.machineCost>,
            AMEstimateOper.machineCalcCost>), typeof(SumCalc<AMEstimateItem.machineCalcCost>))]
        public virtual Decimal? MachineCost
        {
            get { return this._MachineCost; }
            set { this._MachineCost = value; }
        }

        #endregion
        #region Material Override
        public abstract class materialOverride : PX.Data.BQL.BqlBool.Field<materialOverride> { }

        protected Boolean? _MaterialOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? MaterialOverride
        {
            get { return this._MaterialOverride; }
            set { this._MaterialOverride = value; }
        }

        #endregion
        #region Material Unit Cost
        public abstract class materialUnitCost : PX.Data.BQL.BqlDecimal.Field<materialUnitCost> { }


        protected Decimal? _MaterialUnitCost;

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? MaterialUnitCost
        {
            get { return this._MaterialUnitCost; }
            set { this._MaterialUnitCost = value; }
        }

        #endregion
        #region Material Calculated Cost
        public abstract class materialCalcCost : PX.Data.BQL.BqlDecimal.Field<materialCalcCost> { }

        protected Decimal? _MaterialCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(AMEstimateOper.materialUnitCost))]
        public virtual Decimal? MaterialCalcCost
        {
            get { return this._MaterialCalcCost; }
            set { this._MaterialCalcCost = value; }
        }

        #endregion
        #region Material Cost
        public abstract class materialCost : PX.Data.BQL.BqlDecimal.Field<materialCost> { }

        protected Decimal? _MaterialCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material Cost")]
        [TriggerEstimateOvhdFormula]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.materialOverride, Equal<True>>, AMEstimateOper.materialCost>,
            AMEstimateOper.materialCalcCost>), typeof(SumCalc<AMEstimateItem.materialCalcCost>))]
        public virtual Decimal? MaterialCost
        {
            get { return this._MaterialCost; }
            set { this._MaterialCost = value; }
        }
        #endregion
        #region Tool Override
        public abstract class toolOverride : PX.Data.BQL.BqlBool.Field<toolOverride> { }


        protected Boolean? _ToolOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? ToolOverride
        {
            get { return this._ToolOverride; }
            set { this._ToolOverride = value; }
        }

        #endregion
        #region Tool Unit Cost
        public abstract class toolUnitCost : PX.Data.BQL.BqlDecimal.Field<toolUnitCost> { }


        protected Decimal? _ToolUnitCost;

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? ToolUnitCost
        {
            get { return this._ToolUnitCost; }
            set { this._ToolUnitCost = value; }
        }

        #endregion
        #region Tool Calculated Cost
        public abstract class toolCalcCost : PX.Data.BQL.BqlDecimal.Field<toolCalcCost> { }

        protected Decimal? _ToolCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(Mult<AMEstimateOper.baseOrderQty, AMEstimateOper.toolUnitCost>))]
        public virtual Decimal? ToolCalcCost
        {
            get { return this._ToolCalcCost; }
            set { this._ToolCalcCost = value; }
        }

        #endregion
        #region Tool Cost
        public abstract class toolCost : PX.Data.BQL.BqlDecimal.Field<toolCost> { }

        protected Decimal? _ToolCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.toolOverride, Equal<True>>, AMEstimateOper.toolCost>,
            AMEstimateOper.toolCalcCost>), typeof(SumCalc<AMEstimateItem.toolCalcCost>))]
        public virtual Decimal? ToolCost
        {
            get { return this._ToolCost; }
            set { this._ToolCost = value; }
        }
        #endregion
        #region Fixed Overhead Override
        public abstract class fixedOverheadOverride : PX.Data.BQL.BqlBool.Field<fixedOverheadOverride> { }


        protected Boolean? _FixedOverheadOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? FixedOverheadOverride
        {
            get { return this._FixedOverheadOverride; }
            set { this._FixedOverheadOverride = value; }
        }

        #endregion
        #region Fixed Overhead Calculated Cost
        public abstract class fixedOverheadCalcCost : PX.Data.BQL.BqlDecimal.Field<fixedOverheadCalcCost> { }

        protected Decimal? _FixedOverheadCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? FixedOverheadCalcCost
        {
            get { return this._FixedOverheadCalcCost; }
            set { this._FixedOverheadCalcCost = value; }
        }
        #endregion
        #region Fixed Overhead Cost
        public abstract class fixedOverheadCost : PX.Data.BQL.BqlDecimal.Field<fixedOverheadCost> { }

        protected Decimal? _FixedOverheadCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.fixedOverheadOverride, Equal<True>>, AMEstimateOper.fixedOverheadCost>,
            AMEstimateOper.fixedOverheadCalcCost>), typeof(SumCalc<AMEstimateItem.fixedOverheadCalcCost>))]
        public virtual Decimal? FixedOverheadCost
        {
            get { return this._FixedOverheadCost; }
            set { this._FixedOverheadCost = value; }
        }
        #endregion
        #region Variable Overhead Override
        public abstract class variableOverheadOverride : PX.Data.BQL.BqlBool.Field<variableOverheadOverride> { }


        protected Boolean? _VariableOverheadOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? VariableOverheadOverride
        {
            get { return this._VariableOverheadOverride; }
            set { this._VariableOverheadOverride = value; }
        }

        #endregion
        #region Variable Overhead Calculated Cost
        public abstract class variableOverheadCalcCost : PX.Data.BQL.BqlDecimal.Field<variableOverheadCalcCost>{ }

        protected Decimal? _VariableOverheadCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? VariableOverheadCalcCost
        {
            get { return this._VariableOverheadCalcCost; }
            set { this._VariableOverheadCalcCost = value; }
        }
        #endregion
        #region Variable Overhead Cost
        public abstract class variableOverheadCost : PX.Data.BQL.BqlDecimal.Field<variableOverheadCost> { }

        protected Decimal? _VariableOverheadCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Var Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.variableOverheadOverride, Equal<True>>,
                AMEstimateOper.variableOverheadCost>, AMEstimateOper.variableOverheadCalcCost>),
            typeof(SumCalc<AMEstimateItem.variableOverheadCalcCost>))]
        public virtual Decimal? VariableOverheadCost
        {
            get { return this._VariableOverheadCost; }
            set { this._VariableOverheadCost = value; }
        }

        #endregion
        #region Subcontract Override
        public abstract class subcontractOverride : PX.Data.BQL.BqlBool.Field<subcontractOverride> { }

        protected Boolean? _SubcontractOverride;

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? SubcontractOverride
        {
            get { return this._SubcontractOverride; }
            set { this._SubcontractOverride = value; }
        }

        #endregion
        #region Subcontract Unit Cost
        public abstract class subcontractUnitCost : PX.Data.BQL.BqlDecimal.Field<subcontractUnitCost> { }


        protected Decimal? _SubcontractUnitCost;

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        public virtual Decimal? SubcontractUnitCost
        {
            get { return this._SubcontractUnitCost; }
            set { this._SubcontractUnitCost = value; }
        }

        #endregion
        #region Subcontract Calculated Cost
        public abstract class subcontractCalcCost : PX.Data.BQL.BqlDecimal.Field<subcontractCalcCost> { }

        protected Decimal? _SubcontractCalcCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXFormula(typeof(AMEstimateOper.subcontractUnitCost))]
        public virtual Decimal? SubcontractCalcCost
        {
            get { return this._SubcontractCalcCost; }
            set { this._SubcontractCalcCost = value; }
        }

        #endregion
        #region Subcontract Cost
        public abstract class subcontractCost : PX.Data.BQL.BqlDecimal.Field<subcontractCost> { }

        protected Decimal? _SubcontractCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract Cost")]
        [TriggerEstimateOvhdFormula]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateOper.subcontractOverride, Equal<True>>, AMEstimateOper.subcontractCost>,
            AMEstimateOper.subcontractCalcCost>), typeof(SumCalc<AMEstimateItem.subcontractCalcCost>))]
        public virtual Decimal? SubcontractCost
        {
            get { return this._SubcontractCost; }
            set { this._SubcontractCost = value; }
        }
        #endregion
        #region ReferenceMaterialCost
        public abstract class referenceMaterialCost : PX.Data.BQL.BqlDecimal.Field<referenceMaterialCost> { }
        protected Decimal? _ReferenceMaterialCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0")]
        [PXUIField(DisplayName = "Ref. Material Cost", Enabled = false)]
        [PXFormula(null, typeof(SumCalc<AMEstimateItem.referenceMaterialCost>))]
        public virtual Decimal? ReferenceMaterialCost
        {
            get
            {
                return this._ReferenceMaterialCost;
            }
            set
            {
                this._ReferenceMaterialCost = value;
            }
        }
        #endregion
        #region Ext Cost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        protected Decimal? _ExtCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        [PXFormula(typeof(Add<AMEstimateOper.fixedLaborCost, Add<AMEstimateOper.variableLaborCost, Add<AMEstimateOper.machineCost,
            Add<AMEstimateOper.materialCost, Add<AMEstimateOper.toolCost, Add<AMEstimateOper.fixedOverheadCost,
            Add<AMEstimateOper.variableOverheadCost, AMEstimateOper.subcontractCost>>>>>>>))]
        public virtual Decimal? ExtCost
        {
            get { return this._ExtCost; }
            set { this._ExtCost = value; }
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
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        protected Guid? _CreatedByID;

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID
        {
            get { return this._CreatedByID; }
            set { this._CreatedByID = value; }
        }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        protected String _CreatedByScreenID;

        [PXDBCreatedByScreenID]
        public virtual String CreatedByScreenID
        {
            get { return this._CreatedByScreenID; }
            set { this._CreatedByScreenID = value; }
        }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        protected DateTime? _CreatedDateTime;

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime
        {
            get { return this._CreatedDateTime; }
            set { this._CreatedDateTime = value; }
        }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        protected Guid? _LastModifiedByID;

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID
        {
            get { return this._LastModifiedByID; }
            set { this._LastModifiedByID = value; }
        }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        protected String _LastModifiedByScreenID;

        [PXDBLastModifiedByScreenID]
        public virtual String LastModifiedByScreenID
        {
            get { return this._LastModifiedByScreenID; }
            set { this._LastModifiedByScreenID = value; }
        }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        protected DateTime? _LastModifiedDateTime;

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime
        {
            get { return this._LastModifiedDateTime; }
            set { this._LastModifiedDateTime = value; }
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
        #region Line Cntr Step
        public abstract class lineCntrStep : PX.Data.BQL.BqlInt.Field<lineCntrStep> { }

        protected Int32? _LineCntrStep;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrStep
        {
            get { return this._LineCntrStep; }
            set { this._LineCntrStep = value; }
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
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AMEstimateOper.vendorID>>,
            And<Location.isActive, Equal<True>,
            And<MatchWithBranch<Location.vBranchID>>>>), DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible,
            DisplayName = "Vendor Location")]
        [PXDefault(typeof(Coalesce<Search2<BAccountR.defLocationID,
            InnerJoin<CRLocation, On<CRLocation.bAccountID, Equal<BAccountR.bAccountID>, And<CRLocation.locationID, Equal<BAccountR.defLocationID>>>>,
            Where<BAccountR.bAccountID, Equal<Current<AMEstimateOper.vendorID>>,
                And<CRLocation.isActive, Equal<True>,
                And<MatchWithBranch<CRLocation.vBranchID>>>>>,
            Search<CRLocation.locationID,
            Where<CRLocation.bAccountID, Equal<Current<AMEstimateOper.vendorID>>,
            And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMEstimateOper.vendorID>))]
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

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding created by, last mod by, and timestamps fields
        /// (optionally to exclude cost/price values)
        /// </summary>
        /// <param name="excludeChildCalcValues">when true the fields that are calculated as a result of child row values are excluded</param>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy & static ClearCalcValues. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateOper Copy(bool excludeChildCalcValues)
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            var amEstimateOper = Copy();

            if (excludeChildCalcValues)
            {
                amEstimateOper.ClearCalcValues();
            }

            return amEstimateOper;
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        [Obsolete("Use static ClearCalcValues. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual void ClearCalcValues()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            MaterialCost = !MaterialOverride.GetValueOrDefault() ? 0 : MaterialCost;
            MaterialUnitCost = 0;
            MaterialCalcCost = 0;
            ToolCost = !ToolOverride.GetValueOrDefault() ? 0 : ToolCost;
            ToolUnitCost = 0;
            ToolCalcCost = 0;
            FixedOverheadCost = !FixedOverheadOverride.GetValueOrDefault() ? 0 : FixedOverheadCost;
            FixedOverheadCalcCost = 0;
            VariableOverheadCost = !VariableOverheadOverride.GetValueOrDefault() ? 0 : VariableOverheadCost;
            VariableOverheadCalcCost = 0;
            SubcontractCost = !SubcontractOverride.GetValueOrDefault() ? 0 : SubcontractCost;
            SubcontractUnitCost = 0;
            SubcontractCalcCost = 0;
            ReferenceMaterialCost = 0;
        }

        public static AMEstimateOper ClearCalcValues(AMEstimateOper row)
        {
            if (row == null)
            {
                return null;
            }

            row.MaterialCost = !row.MaterialOverride.GetValueOrDefault() ? 0 : row.MaterialCost;
            row.MaterialUnitCost = 0;
            row.MaterialCalcCost = 0;
            row.ToolCost = !row.ToolOverride.GetValueOrDefault() ? 0 : row.ToolCost;
            row.ToolUnitCost = 0;
            row.ToolCalcCost = 0;
            row.FixedOverheadCost = !row.FixedOverheadOverride.GetValueOrDefault() ? 0 : row.FixedOverheadCost;
            row.FixedOverheadCalcCost = 0;
            row.VariableOverheadCost = !row.VariableOverheadOverride.GetValueOrDefault() ? 0 : row.VariableOverheadCost;
            row.VariableOverheadCalcCost = 0;
            row.SubcontractCost = !row.SubcontractOverride.GetValueOrDefault() ? 0 : row.SubcontractCost;
            row.SubcontractUnitCost = 0;
            row.SubcontractCalcCost = 0;
            row.ReferenceMaterialCost = 0;

            return row;
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateOper Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMEstimateOper
            {
                EstimateID = this.EstimateID,
                RevisionID = this.RevisionID,
                OperationID = this.OperationID,
                OperationCD = this.OperationCD,
                BaseOrderQty = this.BaseOrderQty,
                LineCntrMatl = this.LineCntrMatl,
                LineCntrOvhd = this.LineCntrOvhd,
                LineCntrTool = this.LineCntrTool,
                Description = this.Description,
                WorkCenterID = this.WorkCenterID,
                WorkCenterStdCost = this.WorkCenterStdCost,
                MachineStdCost = this.MachineStdCost,
                SetupTime = this.SetupTime,
                RunUnits = this.RunUnits,
                RunUnitTime = this.RunUnitTime,
                MachineUnits = this.MachineUnits,
                MachineUnitTime = this.MachineUnitTime,
                QueueTime = this.QueueTime,
                BackFlushLabor = this.BackFlushLabor,
                FixedLaborCost = this.FixedLaborCost,
                FixedLaborOverride = this.FixedLaborOverride,
                FixedLaborCalcCost = this.FixedLaborCalcCost,
                VariableLaborCost = this.VariableLaborCost,
                VariableLaborOverride = this.VariableLaborOverride,
                VariableLaborCalcCost = this.VariableLaborCalcCost,
                MachineCost = this.MachineCost,
                MachineOverride = this.MachineOverride,
                MachineCalcCost = this.MachineCost,
                MaterialCost = this.MaterialCost,
                MaterialOverride = this.MaterialOverride,
                MaterialUnitCost = this.MaterialUnitCost,
                MaterialCalcCost = this.MaterialCalcCost,
                ToolCost = this.ToolCost,
                ToolOverride = this.ToolOverride,
                ToolUnitCost = this.ToolUnitCost,
                ToolCalcCost = this.ToolCalcCost,
                FixedOverheadCost = this.FixedOverheadCost,
                FixedOverheadOverride = this.FixedOverheadOverride,
                FixedOverheadCalcCost = this.FixedOverheadCalcCost,
                VariableOverheadCost = this.VariableOverheadCost,
                VariableOverheadOverride = this.VariableOverheadOverride,
                VariableOverheadCalcCost = this.VariableOverheadCalcCost,
                ExtCost = this.ExtCost,
                NoteID = this.NoteID,
                SubcontractCost = this.SubcontractCost,
                SubcontractOverride = this.SubcontractOverride,
                SubcontractUnitCost = this.SubcontractUnitCost,
                SubcontractCalcCost = this.SubcontractCalcCost,
                ReferenceMaterialCost = this.ReferenceMaterialCost
            };

        }
    }
}