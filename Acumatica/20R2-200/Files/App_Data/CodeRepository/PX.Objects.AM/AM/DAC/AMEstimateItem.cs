using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.Search;
using PX.Objects.CM;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [EstimateItemProjection(typeof(Select2<Standalone.AMEstimateItem,
        LeftJoin<Standalone.AMEstimatePrimary,
            On<Standalone.AMEstimatePrimary.estimateID, Equal<Standalone.AMEstimateItem.estimateID>>>>))]
    [PXCopyPasteHiddenFields(typeof(AMEstimateItem.body))]
    [PXCacheName(Messages.EstimateItem)]
    [PXPrimaryGraph(typeof(EstimateMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMEstimateItem : IBqlTable, IEstimateInventory, INotable
    {
#if DEBUG
        //
        //  Developer Note: if adding new fields to this extension, add the same fields to PX.Objects.AMStandalone.AMEstimateItem
        //
#endif
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateItem>.By<estimateID, revisionID>
        {
            public static AMEstimateItem Find(PXGraph graph, string estimateID, string revisionID)
                => FindBy(graph, estimateID, revisionID);
            public static AMEstimateItem FindDirty(PXGraph graph, string estimateID, string revisionID)
                => PXSelect<AMEstimateItem,
                        Where<estimateID, Equal<Required<estimateID>>,
                            And<revisionID, Equal<Required<revisionID>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID);
        }

        public static class FK
        {
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMEstimateItem>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMEstimateItem>.By<siteID> { }
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
        [EstimateID(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(Standalone.AMEstimateItem.estimateID))]
        [PXDefault]
        [Rev.Key(typeof(AMEstimateSetup.estimateNumberingID),
                    typeof(AMEstimateItem.estimateID),
                    typeof(AMEstimateItem.revisionID),
                    typeof(AMEstimateItem.estimateID),
                    typeof(AMEstimateItem.revisionID),
                    typeof(AMEstimateItem.inventoryCD),
                    typeof(AMEstimateItem.itemDesc),
                    typeof(AMEstimateItem.estimateClassID),
                    typeof(AMEstimateItem.estimateStatus))]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", IsKey = true, BqlField = typeof(Standalone.AMEstimateItem.revisionID))]
        [PXUIField(DisplayName = "Revision", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Rev.ID(typeof(AMEstimateSetup.defaultRevisionID), 
            typeof(AMEstimateItem.estimateID), 
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionID),
            typeof(AMEstimateItem.revisionDate),
            typeof(AMEstimateItem.isPrimary))]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region Inventory ID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(Standalone.AMEstimateItem.inventoryID))]
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
        #region Inventory CD
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }

        protected String _InventoryCD;
        [PXDefault]
        [EstimateInventoryRaw(Filterable = true, BqlField = typeof(Standalone.AMEstimateItem.inventoryCD))]
        [PXRestrictor(typeof(Where<InventoryItem.stkItem, Equal<True>>), PX.Objects.AM.Messages.EstimateCannotBeNonStock, typeof(InventoryItem.inventoryCD))]
        public virtual String InventoryCD
        {
            get
            {
                return this._InventoryCD;
            }
            set
            {
                this._InventoryCD = value;
            }
        }
        #endregion
        #region Is Non Inventory
        public abstract class isNonInventory : PX.Data.BQL.BqlBool.Field<isNonInventory> { }

        protected Boolean? _IsNonInventory;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.isNonInventory))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Non-Inventory", Enabled = false)]
        public virtual Boolean? IsNonInventory
        {
            get
            {
                return this._IsNonInventory;
            }
            set
            {
                this._IsNonInventory = value;
            }
        }
        #endregion
        #region Item Description
        public abstract class itemDesc : PX.Data.BQL.BqlString.Field<itemDesc> { }

        protected String _ItemDesc;
        [PXDBString(256, IsUnicode = true, BqlField = typeof(Standalone.AMEstimateItem.itemDesc))]
        [PXUIField(DisplayName = "Item Description")]
        public virtual String ItemDesc
        {
            get { return this._ItemDesc; }
            set { this._ItemDesc = value; }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMEstimateItem.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMEstimateItem.inventoryID), BqlField = typeof(Standalone.AMEstimateItem.subItemID))]
        [PXFormula(typeof(Default<AMEstimateItem.inventoryID>))]
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
        #region Estimate Class ID
        public abstract class estimateClassID : PX.Data.BQL.BqlString.Field<estimateClassID> { }

        protected String _EstimateClassID;
        [PXDefault(typeof(AMEstimateSetup.defaultEstimateClassID))]
        [PXDBString(20, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAA", BqlField = typeof(Standalone.AMEstimateItem.estimateClassID))]
        [PXUIField(DisplayName = "Estimate Class", Required = true)]
        [PXSelector(typeof(Search<AMEstimateClass.estimateClassID>))]
        public virtual String EstimateClassID
        {
            get
            {
                return this._EstimateClassID;
            }
            set
            {
                this._EstimateClassID = value;
            }
        }
        #endregion
        #region Revision Date
        public abstract class revisionDate : PX.Data.BQL.BqlDateTime.Field<revisionDate> { }

        protected DateTime? _RevisionDate;
        [PXDBDate(BqlField = typeof(Standalone.AMEstimateItem.revisionDate))]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Revision Date", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? RevisionDate
        {
            get
            {
                return this._RevisionDate;
            }
            set
            {
                this._RevisionDate = value;
            }
        }
        #endregion
        #region Fixed Labor Override
        public abstract class fixedLaborOverride : PX.Data.BQL.BqlBool.Field<fixedLaborOverride> { }

        protected Boolean? _FixedLaborOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.fixedLaborOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? FixedLaborOverride
        {
            get
            {
                return this._FixedLaborOverride;
            }
            set
            {
                this._FixedLaborOverride = value;
            }
        }
        #endregion
        #region Fixed Labor Calculated Cost
        public abstract class fixedLaborCalcCost : PX.Data.BQL.BqlDecimal.Field<fixedLaborCalcCost> { }

        protected Decimal? _FixedLaborCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.fixedLaborCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Labor Calc", Enabled = false, Visible = false)]
        public virtual Decimal? FixedLaborCalcCost
        {
            get
            {
                return this._FixedLaborCalcCost;
            }
            set
            {
                this._FixedLaborCalcCost = value;
            }
        }
        #endregion
        #region Fixed Labor Cost
        public abstract class fixedLaborCost : PX.Data.BQL.BqlDecimal.Field<fixedLaborCost> { }

        protected Decimal? _FixedLaborCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.fixedLaborCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fix Labor Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.fixedLaborOverride, Equal<True>>, AMEstimateItem.fixedLaborCost>,
            AMEstimateItem.fixedLaborCalcCost>))]
        public virtual Decimal? FixedLaborCost
        {
            get { return this._FixedLaborCost; }
            set { _FixedLaborCost = value; }
        }
        #endregion
        #region Variable Labor Override
        public abstract class variableLaborOverride : PX.Data.BQL.BqlBool.Field<variableLaborOverride> { }

        protected Boolean? _VariableLaborOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.variableLaborOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? VariableLaborOverride
        {
            get
            {
                return this._VariableLaborOverride;
            }
            set
            {
                this._VariableLaborOverride = value;
            }
        }
        #endregion
        #region Variable Labor Calculated Cost
        public abstract class variableLaborCalcCost : PX.Data.BQL.BqlDecimal.Field<variableLaborCalcCost> { }

        protected Decimal? _VariableLaborCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.variableLaborCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Var Labor Calc", Enabled = false, Visible = false)]
        public virtual Decimal? VariableLaborCalcCost
        {
            get
            {
                return this._VariableLaborCalcCost;
            }
            set
            {
                this._VariableLaborCalcCost = value;
            }
        }
        #endregion
        #region Variable Labor Cost
        public abstract class variableLaborCost : PX.Data.BQL.BqlDecimal.Field<variableLaborCost> { }

        protected Decimal? _VariableLaborCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.variableLaborCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Var Labor Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.variableLaborOverride, Equal<True>>, AMEstimateItem.variableLaborCost,
            Case<Where<AMEstimateItem.variableLaborOverride, Equal<False>>, variableLaborCalcCost>>>))]
        public virtual Decimal? VariableLaborCost
        {
            get
            {
                return this._VariableLaborCost;
            }
            set
            {
                this._VariableLaborCost = value;
            }
        }
        #endregion
        #region Machine Override
        public abstract class machineOverride : PX.Data.BQL.BqlBool.Field<machineOverride> { }

        protected Boolean? _MachineOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.machineOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? MachineOverride
        {
            get
            {
                return this._MachineOverride;
            }
            set
            {
                this._MachineOverride = value;
            }
        }
        #endregion
        #region Machine Calculated Cost
        public abstract class machineCalcCost : PX.Data.BQL.BqlDecimal.Field<machineCalcCost> { }

        protected Decimal? _MachineCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.machineCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine Cost Calc", Enabled = false, Visible = false)]
        public virtual Decimal? MachineCalcCost
        {
            get
            {
                return this._MachineCalcCost;
            }
            set
            {
                this._MachineCalcCost = value;
            }
        }
        #endregion
        #region Machine Cost
        public abstract class machineCost : PX.Data.BQL.BqlDecimal.Field<machineCost> { }

        protected Decimal? _MachineCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.machineCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.machineOverride, Equal<True>>, AMEstimateItem.machineCost,
            Case<Where<AMEstimateItem.machineOverride, Equal<False>>, AMEstimateItem.machineCalcCost>>>))]
        public virtual Decimal? MachineCost
        {
            get
            {
                return this._MachineCost;
            }
            set
            {
                this._MachineCost = value;
            }
        }
        #endregion
        #region Material Override
        public abstract class materialOverride : PX.Data.BQL.BqlBool.Field<materialOverride> { }

        protected Boolean? _MaterialOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.materialOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? MaterialOverride
        {
            get
            {
                return this._MaterialOverride;
            }
            set
            {
                this._MaterialOverride = value;
            }
        }
        #endregion
        #region Material Calculated Cost
        public abstract class materialCalcCost : PX.Data.BQL.BqlDecimal.Field<materialCalcCost> { }

        protected Decimal? _MaterialCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.materialCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material Cost Calc", Enabled = false, Visible = false)]
        public virtual Decimal? MaterialCalcCost
        {
            get
            {
                return this._MaterialCalcCost;
            }
            set
            {
                this._MaterialCalcCost = value;
            }
        }
        #endregion
        #region Material Cost
        public abstract class materialCost : PX.Data.BQL.BqlDecimal.Field<materialCost> { }

        protected Decimal? _MaterialCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.materialCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Material Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.materialOverride, Equal<True>>, AMEstimateItem.materialCost,
            Case<Where<AMEstimateItem.materialOverride, Equal<False>>, AMEstimateItem.materialCalcCost>>>))]
        public virtual Decimal? MaterialCost
        {
            get
            {
                return this._MaterialCost;
            }
            set
            {
                this._MaterialCost = value;
            }
        }
        #endregion
        #region Tool Override
        public abstract class toolOverride : PX.Data.BQL.BqlBool.Field<toolOverride> { }

        protected Boolean? _ToolOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.toolOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? ToolOverride
        {
            get
            {
                return this._ToolOverride;
            }
            set
            {
                this._ToolOverride = value;
            }
        }
        #endregion
        #region Tool Calculated Cost
        public abstract class toolCalcCost : PX.Data.BQL.BqlDecimal.Field<toolCalcCost> { }

        protected Decimal? _ToolCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.toolCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Tool Cost Calc", Enabled = false, Visible = false)]
        public virtual Decimal? ToolCalcCost
        {
            get
            {
                return this._ToolCalcCost;
            }
            set
            {
                this._ToolCalcCost = value;
            }
        }
        #endregion
        #region Tool Cost
        public abstract class toolCost : PX.Data.BQL.BqlDecimal.Field<toolCost> { }

        protected Decimal? _ToolCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.toolCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tool Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.toolOverride, Equal<True>>, AMEstimateItem.toolCost,
            Case<Where<AMEstimateItem.toolOverride, Equal<False>>, AMEstimateItem.toolCalcCost>>>))]
        public virtual Decimal? ToolCost
        {
            get
            {
                return this._ToolCost;
            }
            set
            {
                this._ToolCost = value;
            }
        }
        #endregion
        #region Fixed Overhead Override
        public abstract class fixedOverheadOverride : PX.Data.BQL.BqlBool.Field<fixedOverheadOverride> { }

        protected Boolean? _FixedOverheadOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.fixedOverheadOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? FixedOverheadOverride
        {
            get
            {
                return this._FixedOverheadOverride;
            }
            set
            {
                this._FixedOverheadOverride = value;
            }
        }
        #endregion
        #region Fixed Overhead Calculated Cost
        public abstract class fixedOverheadCalcCost : PX.Data.BQL.BqlDecimal.Field<fixedOverheadCalcCost> { }

        protected Decimal? _FixedOverheadCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.fixedOverheadCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Overhead Calc Cost", Enabled = false, Visible = false)]
        public virtual Decimal? FixedOverheadCalcCost
        {
            get
            {
                return this._FixedOverheadCalcCost;
            }
            set
            {
                this._FixedOverheadCalcCost = value;
            }
        }
        #endregion
        #region Fixed Overhead Cost
        public abstract class fixedOverheadCost : PX.Data.BQL.BqlDecimal.Field<fixedOverheadCost> { }

        protected Decimal? _FixedOverheadCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.fixedOverheadCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.fixedOverheadOverride, Equal<True>>, AMEstimateItem.fixedOverheadCost,
            Case<Where<AMEstimateItem.fixedOverheadOverride, Equal<False>>, AMEstimateItem.fixedOverheadCalcCost>>>))]
        public virtual Decimal? FixedOverheadCost
        {
            get
            {
                return this._FixedOverheadCost;
            }
            set
            {
                this._FixedOverheadCost = value;
            }
        }
        #endregion
        #region Variable Overhead Override
        public abstract class variableOverheadOverride : PX.Data.BQL.BqlBool.Field<variableOverheadOverride> { }

        protected Boolean? _VariableOverheadOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.variableOverheadOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? VariableOverheadOverride
        {
            get
            {
                return this._VariableOverheadOverride;
            }
            set
            {
                this._VariableOverheadOverride = value;
            }
        }
        #endregion
        #region Variable Overhead Calculated Cost
        public abstract class variableOverheadCalcCost : PX.Data.BQL.BqlDecimal.Field<variableOverheadCalcCost>{ }

        protected Decimal? _VariableOverheadCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.variableOverheadCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Var Overhead Cost Calc", Enabled = false, Visible = false)]
        public virtual Decimal? VariableOverheadCalcCost
        {
            get
            {
                return this._VariableOverheadCalcCost;
            }
            set
            {
                this._VariableOverheadCalcCost = value;
            }
        }
        #endregion
        #region Variable Overhead Cost
        public abstract class variableOverheadCost : PX.Data.BQL.BqlDecimal.Field<variableOverheadCost> { }

        protected Decimal? _VariableOverheadCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.variableOverheadCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Var Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.variableOverheadOverride, Equal<True>>, AMEstimateItem.variableOverheadCost,
            Case<Where<AMEstimateItem.variableOverheadOverride, Equal<False>>, AMEstimateItem.variableOverheadCalcCost>>>))]
        public virtual Decimal? VariableOverheadCost
        {
            get
            {
                return this._VariableOverheadCost;
            }
            set
            {
                this._VariableOverheadCost = value;
            }
        }
        #endregion
        #region Subcontract Override
        public abstract class subcontractOverride : PX.Data.BQL.BqlBool.Field<subcontractOverride> { }

        protected Boolean? _SubcontractOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.subcontractOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? SubcontractOverride
        {
            get
            {
                return this._SubcontractOverride;
            }
            set
            {
                this._SubcontractOverride = value;
            }
        }
        #endregion
        #region Subcontract Calculated Cost
        public abstract class subcontractCalcCost : PX.Data.BQL.BqlDecimal.Field<subcontractCalcCost> { }

        protected Decimal? _SubcontractCalcCost;
        [PXDBDecimal(6, BqlField = typeof(Standalone.AMEstimateItem.subcontractCalcCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract Cost Calc", Enabled = false, Visible = false)]
        public virtual Decimal? SubcontractCalcCost
        {
            get
            {
                return this._SubcontractCalcCost;
            }
            set
            {
                this._SubcontractCalcCost = value;
            }
        }
        #endregion
        #region Subcontract Cost
        public abstract class subcontractCost : PX.Data.BQL.BqlDecimal.Field<subcontractCost> { }

        protected Decimal? _SubcontractCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.subcontractCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Subcontract Cost")]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.subcontractOverride, Equal<True>>, AMEstimateItem.subcontractCost,
            Case<Where<AMEstimateItem.subcontractOverride, Equal<False>>, AMEstimateItem.subcontractCalcCost>>>))]
        public virtual Decimal? SubcontractCost
        {
            get
            {
                return this._SubcontractCost;
            }
            set
            {
                this._SubcontractCost = value;
            }
        }
        #endregion
        #region ReferenceMaterialCost
        public abstract class referenceMaterialCost : PX.Data.BQL.BqlDecimal.Field<referenceMaterialCost> { }
        protected Decimal? _ReferenceMaterialCost;
        [PXDBPriceCost(BqlField = typeof(Standalone.AMEstimateItem.referenceMaterialCost))]
        [PXDefault(TypeCode.Decimal, "0")]
        [PXUIField(DisplayName = "Ref. Material Cost", Enabled = false)]
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
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt(BqlField = typeof(Standalone.AMEstimateItem.itemClassID))]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        [PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<True>>), PX.Objects.AM.Messages.EstimateItemClassMustBeStockItem, typeof(INItemClass.itemClassCD))]
        [PXDefault(typeof(Search<AMEstimateClass.itemClassID, Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(BqlField = typeof(Standalone.AMEstimateItem.siteID))]
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
        #region Owner ID
        public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

        protected int? _OwnerID;
        [Owner(BqlField = typeof(Standalone.AMEstimateItem.ownerID))]
        [PXDefault(typeof(AccessInfo.contactID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? OwnerID
        {
            get
            {
                return this._OwnerID;
            }
            set
            {
                this._OwnerID = value;
            }
        }
        #endregion
        #region EngineerID
        public abstract class engineerID : PX.Data.BQL.BqlInt.Field<engineerID> { }

        protected int? _EngineerID;
        [Owner(BqlField = typeof(Standalone.AMEstimateItem.engineerID), DisplayName = "Engineer")]
        [PXDefault(typeof(Search<AMEstimateClass.engineerID,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        public virtual int? EngineerID
        {
            get
            {
                return this._EngineerID;
            }
            set
            {
                this._EngineerID = value;
            }
        }
        #endregion
        #region Request Date
        public abstract class requestDate : PX.Data.BQL.BqlDateTime.Field<requestDate> { }

        protected DateTime? _RequestDate;
        [PXDBDate(BqlField = typeof(Standalone.AMEstimateItem.requestDate))]
        [PXUIField(DisplayName = "Request Date")]
        public virtual DateTime? RequestDate
        {
            get
            {
                return this._RequestDate;
            }
            set
            {
                this._RequestDate = value;
            }
        }
        #endregion
        #region Promise Date
        public abstract class promiseDate : PX.Data.BQL.BqlDateTime.Field<promiseDate> { }

        protected DateTime? _PromiseDate;
        [PXDBDate(BqlField = typeof(Standalone.AMEstimateItem.promiseDate))]
        [PXUIField(DisplayName = "Promise Date")]
        public virtual DateTime? PromiseDate
        {
            get
            {
                return this._PromiseDate;
            }
            set
            {
                this._PromiseDate = value;
            }
        }
        #endregion
        #region Lead Time
        public abstract class leadTime : PX.Data.BQL.BqlInt.Field<leadTime> { }

        protected int? _LeadTime;
        [PXDBInt(BqlField = typeof(Standalone.AMEstimateItem.leadTime))]
        [PXDefault(TypeCode.Int32, "0", typeof(Search<AMEstimateClass.leadTime,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.leadTime, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Lead Time (Days)")]
        public virtual int? LeadTime
        {
            get
            {
                return this._LeadTime;
            }
            set
            {
                this._LeadTime = value;
            }
        }
        #endregion
        #region Lead Time Override
        public abstract class leadTimeOverride : PX.Data.BQL.BqlBool.Field<leadTimeOverride> { }

        protected Boolean? _LeadTimeOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.leadTimeOverride))]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? LeadTimeOverride
        {
            get
            {
                return this._LeadTimeOverride;
            }
            set
            {
                this._LeadTimeOverride = value;
            }
        }
        #endregion
        #region Order Qty
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        protected Decimal? _OrderQty;
        [EstimateDBQuantity(typeof(AMEstimateItem.uOM), typeof(AMEstimateItem.baseOrderQty), typeof(AMEstimateItem.inventoryID), HandleEmptyKey = true, BqlField = typeof(Standalone.AMEstimateItem.orderQty))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.orderQty,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.orderQty, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Order Qty")]
        public virtual Decimal? OrderQty
        {
            get
            {
                return this._OrderQty;
            }
            set
            {
                this._OrderQty = value;
            }
        }
        #endregion
        #region Base Order Qty
        public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }

        protected Decimal? _BaseOrderQty;
        [PXDBQuantity(BqlField = typeof(Standalone.AMEstimateItem.baseOrderQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Order Qty", Visible = false)]
        [UpdateChildOnFieldUpdated(typeof(AMEstimateOper), typeof(AMEstimateOper.baseOrderQty))]
        public virtual Decimal? BaseOrderQty
        {
            get
            {
                return this._BaseOrderQty;
            }
            set
            {
                this._BaseOrderQty = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<AMEstimateItem.itemClassID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [INUnit(typeof(AMEstimateItem.inventoryID), BqlField = typeof(Standalone.AMEstimateItem.uOM))]
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
        #region Cury ID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

        protected String _CuryID;
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(Standalone.AMEstimateItem.curyID))]
        [PXUIField(DisplayName = "Currency")]
        [PXDefault(typeof(Search<Company.baseCuryID>))]
        [PXSelector(typeof(Currency.curyID))]
        public virtual String CuryID
        {
            get
            {
                return this._CuryID;
            }
            set
            {
                this._CuryID = value;
            }
        }
        #endregion
        #region Cury Info ID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        protected Int64? _CuryInfoID;
        [PXDBLong(BqlField = typeof(Standalone.AMEstimateItem.curyInfoID))]
        [CurrencyInfo]
        public virtual Int64? CuryInfoID
        {
            get
            {
                return this._CuryInfoID;
            }
            set
            {
                this._CuryInfoID = value;
            }
        }
        #endregion
        #region ExtCostDisplay (Unbound)
        public abstract class extCostDisplay : PX.Data.BQL.BqlDecimal.Field<extCostDisplay> { }

        /// <summary>
        /// For use in totals display without a currency view impact
        /// (hiding from currency toggle)
        /// </summary>
        [PXPriceCost]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        public virtual Decimal? ExtCostDisplay
        {
            get
            {
                return this._ExtCost;
            }
            set
            {
                this._ExtCost = value;
            }
        }
        #endregion
        #region Ext Cost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        protected Decimal? _ExtCost;
        [AMDBCurrencyBase(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.curyExtCost), BqlField = typeof(Standalone.AMEstimateItem.extCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        [PXFormula(typeof(Add<AMEstimateItem.fixedLaborCost, Add<AMEstimateItem.variableLaborCost, Add<AMEstimateItem.machineCost,
            Add<AMEstimateItem.materialCost, Add<AMEstimateItem.subcontractCost, Add<AMEstimateItem.toolCost, Add<AMEstimateItem.fixedOverheadCost,
            AMEstimateItem.variableOverheadCost>>>>>>>))]
        public virtual Decimal? ExtCost
        {
            get
            {
                return this._ExtCost;
            }
            set
            {
                this._ExtCost = value;
            }
        }
        #endregion
        #region Cury Ext Cost
        public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        protected Decimal? _CuryExtCost;
        [PXDBCurrency(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.extCost), BaseCalc = false, BqlField = typeof(Standalone.AMEstimateItem.curyExtCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        public virtual Decimal? CuryExtCost
        {
            get
            {
                return this._CuryExtCost;
            }
            set
            {
                this._CuryExtCost = value;
            }
        }
        #endregion
        #region Cury Unit Cost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        protected Decimal? _CuryUnitCost;
        [PXDBCurrency(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.unitCost), BaseCalc = false, BqlField = typeof(Standalone.AMEstimateItem.curyUnitCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        public virtual Decimal? CuryUnitCost
        {
            get
            {
                return this._CuryUnitCost ?? 0m;
            }
            set
            {
                this._CuryUnitCost = value ?? 0m;
            }
        }
        #endregion
        #region Unit Cost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [AMDBCurrencyBase(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.curyUnitCost), BqlField = typeof(Standalone.AMEstimateItem.unitCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.orderQty, Equal<decimal0>>, decimal0,
            Case<Where<AMEstimateItem.orderQty, NotEqual<decimal0>>, Div<AMEstimateItem.extCost, AMEstimateItem.orderQty>>>>))]
        public virtual Decimal? UnitCost
        {
            get
            {
                return this._UnitCost ?? 0m;
            }
            set
            {
                this._UnitCost = value ?? 0m;
            }
        }
        #endregion
        #region Labor Markup Percent
        public abstract class laborMarkupPct : PX.Data.BQL.BqlDecimal.Field<laborMarkupPct> { }

        protected Decimal? _LaborMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.laborMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.laborMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.laborMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Labor Markup Pct.")]
        public virtual Decimal? LaborMarkupPct
        {
            get
            {
                return this._LaborMarkupPct;
            }
            set
            {
                this._LaborMarkupPct = value;
            }
        }
        #endregion
        #region Machine Markup Percent
        public abstract class machineMarkupPct : PX.Data.BQL.BqlDecimal.Field<machineMarkupPct> { }

        protected Decimal? _MachineMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.machineMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.machineMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.machineMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Machine Markup Pct.")]
        public virtual Decimal? MachineMarkupPct
        {
            get
            {
                return this._MachineMarkupPct;
            }
            set
            {
                this._MachineMarkupPct = value;
            }
        }
        #endregion
        #region Material Markup Percent
        public abstract class materialMarkupPct : PX.Data.BQL.BqlDecimal.Field<materialMarkupPct> { }

        protected Decimal? _MaterialMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.materialMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.materialMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.materialMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Material Markup Pct.")]
        public virtual Decimal? MaterialMarkupPct
        {
            get
            {
                return this._MaterialMarkupPct;
            }
            set
            {
                this._MaterialMarkupPct = value;
            }
        }
        #endregion
        #region Tool Markup Percent
        public abstract class toolMarkupPct : PX.Data.BQL.BqlDecimal.Field<toolMarkupPct> { }

        protected Decimal? _ToolMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.toolMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.toolMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.toolMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Tool Markup Pct.")]
        public virtual Decimal? ToolMarkupPct
        {
            get
            {
                return this._ToolMarkupPct;
            }
            set
            {
                this._ToolMarkupPct = value;
            }
        }
        #endregion
        #region Overhead Markup Percent
        public abstract class overheadMarkupPct : PX.Data.BQL.BqlDecimal.Field<overheadMarkupPct> { }

        protected Decimal? _OverheadMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.overheadMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.overheadMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.overheadMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Overhead Markup Pct.")]
        public virtual Decimal? OverheadMarkupPct
        {
            get
            {
                return this._OverheadMarkupPct;
            }
            set
            {
                this._OverheadMarkupPct = value;
            }
        }
        #endregion
        #region Subcontract Markup Percent
        public abstract class subcontractMarkupPct : PX.Data.BQL.BqlDecimal.Field<subcontractMarkupPct> { }

        protected Decimal? _SubcontractMarkupPct;
        [PXDBDecimal(2, MinValue = 0.0, BqlField = typeof(Standalone.AMEstimateItem.subcontractMarkupPct))]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.subcontractMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.subcontractMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AMEstimateItem.estimateClassID>))]
        [PXUIField(DisplayName = "Subcontract Markup Pct.")]
        public virtual Decimal? SubcontractMarkupPct
        {
            get
            {
                return this._SubcontractMarkupPct;
            }
            set
            {
                this._SubcontractMarkupPct = value;
            }
        }
        #endregion
        #region Markup Percent
        public abstract class markupPct : PX.Data.BQL.BqlDecimal.Field<markupPct> { }

        protected Decimal? _MarkupPct;
        [PXDBDecimal(2, BqlField = typeof(Standalone.AMEstimateItem.markupPct))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overall Markup Pct.", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.extCost, Equal<decimal0>>, decimal0>,
            Case<Where<AMEstimateItem.extCost, NotEqual<decimal0>>, Mult<Div<Sub<AMEstimateItem.extPrice, 
                AMEstimateItem.extCost>, AMEstimateItem.extCost>, decimal100>>>))]
        public virtual Decimal? MarkupPct
        {
            get
            {
                return this._MarkupPct;
            }
            set
            {
                this._MarkupPct = value;
            }
        }
        #endregion
        #region Cury Ext Price
        public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }

        protected Decimal? _CuryExtPrice;
        [PXDBCurrency(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.extPrice), BaseCalc = false, BqlField = typeof(Standalone.AMEstimateItem.curyExtPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Price", Enabled = false)]
        public virtual Decimal? CuryExtPrice
        {
            get
            {
                return this._CuryExtPrice ?? 0m;
            }
            set
            {
                this._CuryExtPrice = value ?? 0m;
            }
        }
        #endregion
        #region Ext Price
        public abstract class extPrice : PX.Data.BQL.BqlDecimal.Field<extPrice> { }

        protected Decimal? _ExtPrice;
        [AMDBCurrencyBase(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.curyExtPrice), BqlField = typeof(Standalone.AMEstimateItem.extPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Price", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.priceOverride, Equal<True>>, Mult<AMEstimateItem.curyUnitPrice, AMEstimateItem.orderQty>>,
            Case<Where<AMEstimateItem.priceOverride, Equal<False>>, Add<Mult<Add<AMEstimateItem.fixedLaborCost, AMEstimateItem.variableLaborCost>,
            Add<decimal1, Div<AMEstimateItem.laborMarkupPct, decimal100>>>, Add<Mult<AMEstimateItem.machineCost, Add<decimal1, 
            Div<AMEstimateItem.machineMarkupPct, decimal100>>>,
            Add<Mult<AMEstimateItem.materialCost, Add<decimal1, Div<AMEstimateItem.materialMarkupPct, decimal100>>>,
            Add<Mult<AMEstimateItem.toolCost, Add<decimal1, Div<AMEstimateItem.toolMarkupPct, decimal100>>>,
            Add<Mult<AMEstimateItem.subcontractCost, Add<decimal1, Div<AMEstimateItem.subcontractMarkupPct, decimal100>>>,
            Mult<Add<AMEstimateItem.fixedOverheadCost, AMEstimateItem.variableOverheadCost>,
            Add<decimal1, Div<AMEstimateItem.overheadMarkupPct, decimal100>>>>>>>>>>))]
        public virtual Decimal? ExtPrice
        {
            get
            {
                return this._ExtPrice ?? 0m;
            }
            set
            {
                this._ExtPrice = value ?? 0m;
            }
        }
        #endregion
        #region Cury Unit Price
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        protected Decimal? _CuryUnitPrice;
        [PXDBCurrency(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.unitPrice), BaseCalc = false, BqlField = typeof(Standalone.AMEstimateItem.curyUnitPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Price")]
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return this._CuryUnitPrice ?? 0m;
            }
            set
            {
                this._CuryUnitPrice = value ?? 0m;
            }
        }
        #endregion
        #region Unit Price
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        protected Decimal? _UnitPrice;
        [AMDBCurrencyBase(typeof(AMEstimateItem.curyInfoID), typeof(AMEstimateItem.curyUnitPrice), BqlField = typeof(Standalone.AMEstimateItem.unitPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Price", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateItem.orderQty, Equal<decimal0>>, decimal0,
            Case<Where<AMEstimateItem.orderQty, NotEqual<decimal0>>, Div<AMEstimateItem.extPrice, AMEstimateItem.orderQty>>>>))]
        public virtual Decimal? UnitPrice
        {
            get
            {
                return this._UnitPrice ?? 0m;
            }
            set
            {
                this._UnitPrice = value ?? 0m;
            }
        }
        #endregion
        #region Price Override
        public abstract class priceOverride : PX.Data.BQL.BqlBool.Field<priceOverride> { }

        protected Boolean? _PriceOverride;
        [PXDBBool(BqlField = typeof(Standalone.AMEstimateItem.priceOverride))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual Boolean? PriceOverride
        {
            get
            {
                return this._PriceOverride;
            }
            set
            {
                this._PriceOverride = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXSearchable(PX.Objects.SM.SearchCategory.All, Messages.EstimateSearchableTitleDocument, new [] { typeof(AMEstimateItem.estimateID), typeof(AMEstimateItem.revisionID), typeof(AMEstimateItem.inventoryCD) },
            new [] { typeof(AMEstimateItem.estimateID), typeof(AMEstimateItem.revisionID), typeof(AMEstimateItem.inventoryCD) },
            NumberFields = new Type[] { typeof(AMEstimateItem.estimateID), typeof(AMEstimateItem.revisionID) },
            Line1Format = "{0:d}{1}{2}", Line1Fields = new Type[] { typeof(AMEstimateItem.revisionDate), typeof(AMEstimateItem.estimateStatus), typeof(AMEstimateItem.estimateClassID) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(AMEstimateItem.itemDesc) })]
        [PXNote(BqlField = typeof(Standalone.AMEstimateItem.noteID))]
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
        #region Image URL
        public abstract class imageURL : PX.Data.BQL.BqlString.Field<imageURL> { }

        protected String _ImageURL;
        [PXDBString(255, BqlField = typeof(Standalone.AMEstimateItem.imageURL))]
        public virtual String ImageURL
        {
            get
            {
                return this._ImageURL;
            }
            set
            {
                this._ImageURL = value;
            }
        }
        #endregion
        #region Body
        // Used for the Detailed Description in field "Body"
        private string _plainText;
        public abstract class body : PX.Data.BQL.BqlString.Field<body> { }

        protected String _Body;
        /// <summary>
		/// Rich text description of the item/estimate
		/// </summary>
        [PXDBText(IsUnicode = true, BqlField = typeof(Standalone.AMEstimateItem.body))]
        [PXUIField(DisplayName = "Description")]
        public virtual String Body
        {
            get
            {
                return this._Body;
            }
            set
            {
                this._Body = value;
                this._plainText = (string)null;
            }
        }
        [PXString(IsUnicode = true)]
        [PXUIField(Visible = false)]
        public virtual string DescriptionAsPlainText
        {
            get
            {
                return this._plainText ?? (this._plainText = SearchService.Html2PlainText(this.Body));
            }
        }
        #endregion
        #region Line Cntr Oper
        public abstract class lineCntrOper : PX.Data.BQL.BqlInt.Field<lineCntrOper> { }

        protected Int32? _LineCntrOper;
        [PXDBInt(BqlField = typeof(Standalone.AMEstimateItem.lineCntrOper))]
        [PXDefault(0)]
        public virtual Int32? LineCntrOper
        {
            get
            {
                return this._LineCntrOper;
            }
            set
            {
                this._LineCntrOper = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(BqlField = typeof(Standalone.AMEstimateItem.createdByID))]
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
        [PXDBCreatedByScreenID(BqlField = typeof(Standalone.AMEstimateItem.createdByScreenID))]
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
        [PXDBCreatedDateTime(BqlField = typeof(Standalone.AMEstimateItem.createdDateTime))]
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
        [PXDBLastModifiedByID(BqlField = typeof(Standalone.AMEstimateItem.lastModifiedByID))]
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
        [PXDBLastModifiedByScreenID(BqlField = typeof(Standalone.AMEstimateItem.lastModifiedByScreenID))]
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
        [PXDBLastModifiedDateTime(BqlField = typeof(Standalone.AMEstimateItem.lastModifiedDateTime))]
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
        [PXDBTimestamp(BqlField = typeof(Standalone.AMEstimateItem.Tstamp))]
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
        #region PEstimateID
        /// <summary>
        /// EstimateID for AMEstimatePrimary
        /// </summary>
        public abstract class pEstimateID : PX.Data.BQL.BqlString.Field<pEstimateID> { }

        /// <summary>
        /// EstimateID for AMEstimatePrimary
        /// </summary>
        [PXExtraKey]
        [EstimateID(Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible, BqlField = typeof(Standalone.AMEstimatePrimary.estimateID))]
        public virtual String PEstimateID
        {
            get { return EstimateID; }
            set { }
        }
        #endregion
        #region PrimaryRevisionID
        /// <summary>
        /// Primary revision of current estimate
        /// </summary>
        public abstract class primaryRevisionID : PX.Data.BQL.BqlString.Field<primaryRevisionID> { }

        protected String _PrimaryRevisionID;
        /// <summary>
        /// Primary revision of current estimate
        /// </summary>
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", BqlField = typeof(Standalone.AMEstimatePrimary.primaryRevisionID))]
        [PXUIField(DisplayName = "Primary Revision")]
        public virtual String PrimaryRevisionID
        {
            get
            {
                return this._PrimaryRevisionID;
            }
            set
            {
                this._PrimaryRevisionID = value;
            }
        }
        #endregion
        #region QuoteSource
        public abstract class quoteSource : PX.Data.BQL.BqlInt.Field<quoteSource> { }

        [PXDBInt(BqlField = typeof(Standalone.AMEstimatePrimary.quoteSource))]
        [PXDefault(EstimateSource.Estimate)]
        [PXUIField(DisplayName = "Quote Source", Enabled = false)]
        [EstimateSource.List]
        public virtual int? QuoteSource { get; set; }
        #endregion
        #region EstimateStatus
        /// <summary>
        /// Overall estimate status
        /// </summary>
        public abstract class estimateStatus : PX.Data.BQL.BqlInt.Field<estimateStatus> { }

        /// <summary>
        /// Overall estimate status
        /// </summary>
        [PXDBInt(BqlField = typeof(Standalone.AMEstimatePrimary.estimateStatus))]
        [PXDefault(Attributes.EstimateStatus.NewStatus)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
        [Attributes.EstimateStatus.List]
        public virtual int? EstimateStatus { get; set; }
        #endregion
        #region IsLockedByQuote
        /// <summary>
        /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
        /// </summary>
        public abstract class isLockedByQuote : PX.Data.BQL.BqlBool.Field<isLockedByQuote> { }

        protected Boolean? _IsLockedByQuote;
        /// <summary>
        /// When the estimate is linked to specific quote orders, the quote order will drive some fields such as mark as primary which should prevent the user from making changes on the estimate directly
        /// </summary>
        [PXDBBool(BqlField = typeof(Standalone.AMEstimatePrimary.isLockedByQuote))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Locked by Quote", Enabled = false, Visible = false)]
        public virtual Boolean? IsLockedByQuote
        {
            get { return _IsLockedByQuote; }
            set { _IsLockedByQuote = value; }
        }
        #endregion
        #region LineCntrHistory
        public abstract class lineCntrHistory : PX.Data.BQL.BqlInt.Field<lineCntrHistory> { }

        protected int? _LineCntrHistory;
        [PXDBInt(BqlField = typeof(Standalone.AMEstimatePrimary.lineCntrHistory))]
        [PXDefault(0)]
        [PXUIField(DisplayName = "History Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrHistory
        {
            get
            {
                return this._LineCntrHistory;
            }
            set
            {
                this._LineCntrHistory = value;
            }
        }
        #endregion
        #region CreatedByID

        public abstract class pCreatedByID : PX.Data.BQL.BqlGuid.Field<pCreatedByID> { }

        [PXDBCreatedByID(BqlField = typeof(Standalone.AMEstimatePrimary.createdByID))]
        public virtual Guid? PCreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class pCreatedByScreenID : PX.Data.BQL.BqlString.Field<pCreatedByScreenID> { }
        [PXDBCreatedByScreenID(BqlField = typeof(Standalone.AMEstimatePrimary.createdByScreenID))]
        public virtual String PCreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class pCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<pCreatedDateTime> { }
        [PXDBCreatedDateTime(BqlField = typeof(Standalone.AMEstimatePrimary.createdDateTime))]
        public virtual DateTime? PCreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class pLastModifiedByID : PX.Data.BQL.BqlGuid.Field<pLastModifiedByID> { }
        [PXDBLastModifiedByID(BqlField = typeof(Standalone.AMEstimatePrimary.lastModifiedByID))]
        public virtual Guid? PLastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class pLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<pLastModifiedByScreenID> { }
        [PXDBLastModifiedByScreenID(BqlField = typeof(Standalone.AMEstimatePrimary.lastModifiedByScreenID))]
        public virtual String PLastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class pLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<pLastModifiedDateTime> { }
        [PXDBLastModifiedDateTime(BqlField = typeof(Standalone.AMEstimatePrimary.lastModifiedDateTime))]
        public virtual DateTime? PLastModifiedDateTime { get; set; }
        #endregion
        #region Is Primary (UnBound)
        public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }
        [PXBool]
        [PXUIField(DisplayName = "Primary", Enabled = false)]
        [PXDependsOnFields(typeof(AMEstimateItem.revisionID), typeof(AMEstimateItem.primaryRevisionID))]
        public virtual Boolean? IsPrimary => RevisionID != null && PrimaryRevisionID == null || RevisionID.EqualsWithTrim(PrimaryRevisionID);

        #endregion

        internal string DebuggerDisplay =>
#if DEBUG
         $"EstimateID = {EstimateID}; RevisionID = {RevisionID}; PrimaryRevisionID = {PrimaryRevisionID}; EstimateStatus = {Attributes.EstimateStatus.GetDescription(EstimateStatus)}; QuoteSource = {Attributes.EstimateSource.GetDescription(QuoteSource)}";
#else
         $"EstimateID = {EstimateID}; RevisionID = {RevisionID}; PrimaryRevisionID = {PrimaryRevisionID}; EstimateStatus = {EstimateStatus}; QuoteSource = {QuoteSource}";
#endif

        public static AMEstimateItem ResetChildCalcValues(AMEstimateItem amEstimateItem)
        {
            amEstimateItem.FixedLaborCost = !amEstimateItem.FixedLaborOverride.GetValueOrDefault() ? 0 : amEstimateItem.FixedLaborCost;
            amEstimateItem.FixedLaborCalcCost = 0;
            amEstimateItem.VariableLaborCost = !amEstimateItem.VariableLaborOverride.GetValueOrDefault()
                ? 0
                : amEstimateItem.VariableLaborCost;
            amEstimateItem.VariableLaborCalcCost = 0;
            amEstimateItem.MachineCost = !amEstimateItem.MachineOverride.GetValueOrDefault() ? 0 : amEstimateItem.MachineCost;
            amEstimateItem.MachineCalcCost = 0;
            amEstimateItem.MaterialCost = !amEstimateItem.MaterialOverride.GetValueOrDefault() ? 0 : amEstimateItem.MaterialCost;
            amEstimateItem.MaterialCalcCost = 0;
            amEstimateItem.ToolCost = !amEstimateItem.ToolOverride.GetValueOrDefault() ? 0 : amEstimateItem.ToolCost;
            amEstimateItem.ToolCalcCost = 0;
            amEstimateItem.FixedOverheadCost = !amEstimateItem.FixedOverheadOverride.GetValueOrDefault()
                ? 0
                : amEstimateItem.FixedOverheadCost;
            amEstimateItem.FixedOverheadCalcCost = 0;
            amEstimateItem.VariableOverheadCost = !amEstimateItem.VariableOverheadOverride.GetValueOrDefault()
                ? 0
                : amEstimateItem.VariableOverheadCost;
            amEstimateItem.VariableOverheadCalcCost = 0;
            amEstimateItem.SubcontractCost = !amEstimateItem.SubcontractOverride.GetValueOrDefault()
                ? 0
                : amEstimateItem.SubcontractCost;
            amEstimateItem.SubcontractCalcCost = 0;
            return amEstimateItem;
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding created by, last mod by, and timestamps fields
        /// (optionally to exclude cost/price values)
        /// </summary>
        /// <param name="excludeChildCalcValues">when true the fields that are calculated as a result of child row values are excluded</param>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy & AMEstimateItem.ResetChildCalcValues. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateItem Copy(bool excludeChildCalcValues)
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            var amEstimateItem = Copy();

            if (excludeChildCalcValues)
            {
                amEstimateItem = ResetChildCalcValues(amEstimateItem);
            }

            return amEstimateItem;
        }

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateItem Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMEstimateItem
            {
                EstimateID = this.EstimateID,
                RevisionID = this.RevisionID,
                PrimaryRevisionID = this.PrimaryRevisionID, 
                IsLockedByQuote = this.IsLockedByQuote,
                QuoteSource = this.QuoteSource,
                EstimateStatus = this.EstimateStatus,
                SiteID = this.SiteID,
                CuryInfoID = this.CuryInfoID,
                CuryID = this.CuryID,
                InventoryID = this.InventoryID,
                InventoryCD = this.InventoryCD,
                IsNonInventory = this.IsNonInventory,
                ItemDesc = this.ItemDesc,
                SubItemID = this.SubItemID,
                EstimateClassID = this.EstimateClassID,
                RevisionDate = this.RevisionDate,
                FixedLaborCost = this.FixedLaborCost,
                FixedLaborOverride = this.FixedLaborOverride,
                FixedLaborCalcCost = this.FixedLaborCalcCost,
                VariableLaborCost = this.VariableLaborCost,
                VariableLaborOverride = this.VariableLaborOverride,
                VariableLaborCalcCost = this.VariableLaborCalcCost,
                MachineCost =  this.MachineCost,
                MachineOverride = this.MachineOverride,
                MachineCalcCost = this.MachineCalcCost,
                MaterialCost = this.MaterialCost,
                MaterialOverride = this.MaterialOverride,
                MaterialCalcCost = this.MaterialCalcCost,
                ToolCost = this.ToolCost,
                ToolOverride = this.ToolOverride,
                ToolCalcCost = this.ToolCalcCost,
                FixedOverheadCost = this.FixedOverheadCost,
                FixedOverheadOverride = this.FixedOverheadOverride,
                FixedOverheadCalcCost = this.FixedOverheadCalcCost,
                VariableOverheadCost = this.VariableOverheadCost,
                VariableOverheadOverride = this.VariableOverheadOverride,
                VariableOverheadCalcCost = this.VariableOverheadCalcCost,
                SubcontractCost = this.SubcontractCost,
                SubcontractOverride = this.SubcontractOverride,
                SubcontractCalcCost = this.SubcontractCalcCost,
                ItemClassID = this.ItemClassID,
                OwnerID = this.OwnerID,
                EngineerID = this.EngineerID,
                RequestDate = this.RequestDate,
                PromiseDate = this.PromiseDate,
                LeadTime = this.LeadTime,
                LeadTimeOverride = this.LeadTimeOverride,
                OrderQty = this.OrderQty,
                BaseOrderQty = this.BaseOrderQty,
                UOM = this.UOM,
                CuryUnitCost = this.CuryUnitCost,
                UnitCost = this.UnitCost,
                CuryExtCost = this.CuryExtCost, 
                ExtCost = this.ExtCost,
                LaborMarkupPct = this.LaborMarkupPct,
                MachineMarkupPct = this.MachineMarkupPct,
                MaterialMarkupPct = this.MaterialMarkupPct,
                ToolMarkupPct = this.ToolMarkupPct,
                SubcontractMarkupPct = this.SubcontractMarkupPct,
                OverheadMarkupPct = this.OverheadMarkupPct,
                CuryUnitPrice = this.CuryUnitPrice,
                UnitPrice = this.UnitPrice,
                CuryExtPrice = this.CuryExtPrice,
                ExtPrice = this.ExtPrice,
                PriceOverride = this.PriceOverride,
                ImageURL = this.ImageURL,
                Body = this.Body,
                LineCntrOper = this.LineCntrOper,
                NoteID = this.NoteID
            };
        }
    }
}