using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Data.Search;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.TM;

namespace PX.Objects.AM.Standalone
{
    [PXHidden]
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("EstimateID = {EstimateID}; RevisionID = {RevisionID}")]
    public class AMEstimateItem : IBqlTable, IEstimateInventory, INotable
    {
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
        [EstimateID(IsKey = true)]
        [PXDefault]
        [Rev.Key(typeof(AMEstimateSetup.estimateNumberingID),
                    typeof(AM.AMEstimateItem.estimateID),
                    typeof(AM.AMEstimateItem.revisionID),
                    typeof(AM.AMEstimateItem.estimateID),
                    typeof(AM.AMEstimateItem.revisionID),
                    typeof(AM.AMEstimateItem.inventoryCD),
                    typeof(AM.AMEstimateItem.itemDesc),
                    typeof(AM.AMEstimateItem.estimateClassID))]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region Revision ID
        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

        protected String _RevisionID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Rev.ID(typeof(AMEstimateSetup.defaultRevisionID),
            typeof(AM.AMEstimateItem.estimateID),
            typeof(AM.AMEstimateItem.revisionID),
            typeof(AM.AMEstimateItem.revisionID),
            typeof(AM.AMEstimateItem.revisionDate),
            typeof(AM.AMEstimateItem.isPrimary))]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }
        #endregion
        #region Status
        [Obsolete("Dropping revision status for controlling revisions by the primary revision (IsPrimary)")]
        public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

        protected int? _Status;
        [Obsolete("Dropping revision status for controlling revisions by the primary revision (IsPrimary)")]
        [PXDBInt]
        [PXUIField(DisplayName = "Status", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual int? Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                this._Status = value;
            }
        }
        #endregion
        #region Inventory ID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [PXDBInt]
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
        [EstimateInventoryRaw(Filterable = true)]
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
        [PXDBBool]
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
        [PXDBString(255, IsUnicode = true)]
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
            Where<InventoryItem.inventoryID, Equal<Current<AM.AMEstimateItem.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AM.AMEstimateItem.inventoryID))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.inventoryID>))]
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
        [PXDBString(20, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Estimate Class")]
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
        [PXDBDate]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Fix Labor Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.fixedLaborOverride, Equal<True>>, AM.AMEstimateItem.fixedLaborCost>,
            AM.AMEstimateItem.fixedLaborCalcCost>))]
        public virtual Decimal? FixedLaborCost
        {
            get { return this._FixedLaborCost; }
            set { _FixedLaborCost = value; }
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Var Labor Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.variableLaborOverride, Equal<True>>, AM.AMEstimateItem.variableLaborCost,
            Case<Where<AM.AMEstimateItem.variableLaborOverride, Equal<False>>, AM.AMEstimateItem.variableLaborCalcCost>>>))]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Machine Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.machineOverride, Equal<True>>, AM.AMEstimateItem.machineCost,
            Case<Where<AM.AMEstimateItem.machineOverride, Equal<False>>, AM.AMEstimateItem.machineCalcCost>>>))]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Material Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.materialOverride, Equal<True>>, AM.AMEstimateItem.materialCost,
            Case<Where<AM.AMEstimateItem.materialOverride, Equal<False>>, AM.AMEstimateItem.materialCalcCost>>>))]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Tool Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.toolOverride, Equal<True>>, AM.AMEstimateItem.toolCost,
            Case<Where<AM.AMEstimateItem.toolOverride, Equal<False>>, AM.AMEstimateItem.toolCalcCost>>>))]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fix Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.fixedOverheadOverride, Equal<True>>, AM.AMEstimateItem.fixedOverheadCost,
            Case<Where<AM.AMEstimateItem.fixedOverheadOverride, Equal<False>>, AM.AMEstimateItem.fixedOverheadCalcCost>>>))]
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
        [PXDBBool]
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
        public abstract class variableOverheadCalcCost : PX.Data.BQL.BqlDecimal.Field<variableOverheadCalcCost> { }

        protected Decimal? _VariableOverheadCalcCost;
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Var Overhead Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.variableOverheadOverride, Equal<True>>, AM.AMEstimateItem.variableOverheadCost,
            Case<Where<AM.AMEstimateItem.variableOverheadOverride, Equal<False>>, AM.AMEstimateItem.variableOverheadCalcCost>>>))]
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
        [PXDBBool]
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
        [PXDBDecimal(6)]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Subcontract Cost")]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.subcontractOverride, Equal<True>>, AM.AMEstimateItem.subcontractCost,
            Case<Where<AM.AMEstimateItem.subcontractOverride, Equal<False>>, AM.AMEstimateItem.subcontractCalcCost>>>))]
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
        [PXDBPriceCost]
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
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        [PXRestrictor(typeof(Where<INItemClass.stkItem, Equal<True>>), PX.Objects.AM.Messages.EstimateItemClassMustBeStockItem, typeof(INItemClass.itemClassCD))]
        [PXDefault(typeof(Search<AMEstimateClass.itemClassID, Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>>>)
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
        [Site]
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
        [Owner]
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
        [Owner(DisplayName = "Engineer")]
        [PXDefault(typeof(Search<AMEstimateClass.engineerID,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDate]
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
        [PXDBDate]
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
        [PXDBInt]
        [PXDefault(TypeCode.Int32, "0", typeof(Search<AMEstimateClass.leadTime,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.leadTime, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBBool]
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
        [EstimateDBQuantity(typeof(AM.AMEstimateItem.uOM), typeof(AM.AMEstimateItem.baseOrderQty), typeof(AM.AMEstimateItem.inventoryID), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.orderQty,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.orderQty, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBQuantity]
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
        [PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<AM.AMEstimateItem.itemClassID>>>>))]
        [INUnit(typeof(AM.AMEstimateItem.inventoryID))]
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
        [PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
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
        [PXDBLong]
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
        [AMDBCurrencyBase(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.curyExtCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        [PXFormula(typeof(Add<AM.AMEstimateItem.fixedLaborCost, Add<AM.AMEstimateItem.variableLaborCost, Add<AM.AMEstimateItem.machineCost,
            Add<AM.AMEstimateItem.materialCost, Add<AM.AMEstimateItem.toolCost, Add<AM.AMEstimateItem.fixedOverheadCost,
            AM.AMEstimateItem.variableOverheadCost>>>>>>))]
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
        [PXDBCurrency(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.extCost), BaseCalc = false)]
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
        [PXDBCurrency(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.unitCost), BaseCalc = false)]
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
        [AMDBCurrencyBase(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.curyUnitCost))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.orderQty, Equal<decimal0>>, decimal0,
            Case<Where<AM.AMEstimateItem.orderQty, NotEqual<decimal0>>, Div<AM.AMEstimateItem.extCost, AM.AMEstimateItem.orderQty>>>>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.laborMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.laborMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.machineMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.machineMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.materialMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.materialMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.toolMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.toolMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.overheadMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.overheadMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<AMEstimateClass.subcontractMarkupPct,
            Where<AMEstimateClass.estimateClassID, Equal<Current<AM.AMEstimateItem.estimateClassID>>,
                And<AMEstimateClass.subcontractMarkupPct, NotEqual<decimal0>>>>))]
        [PXFormula(typeof(Default<AM.AMEstimateItem.estimateClassID>))]
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
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overall Markup Pct.", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.extCost, Equal<decimal0>>, decimal0>,
            Case<Where<AM.AMEstimateItem.extCost, NotEqual<decimal0>>, Mult<Div<Sub<AM.AMEstimateItem.extPrice,
                AM.AMEstimateItem.extCost>, AM.AMEstimateItem.extCost>, decimal100>>>))]
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
        [PXDBCurrency(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.extPrice), BaseCalc = false)]
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
        [AMDBCurrencyBase(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.curyExtPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Price", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.priceOverride, Equal<True>>, Mult<AM.AMEstimateItem.curyUnitPrice, AM.AMEstimateItem.orderQty>>,
            Case<Where<AM.AMEstimateItem.priceOverride, Equal<False>>, Add<Mult<Add<AM.AMEstimateItem.fixedLaborCost, AM.AMEstimateItem.variableLaborCost>,
            Add<decimal1, Div<AM.AMEstimateItem.laborMarkupPct, decimal100>>>, Add<Mult<AM.AMEstimateItem.machineCost, Add<decimal1,
            Div<AM.AMEstimateItem.machineMarkupPct, decimal100>>>,
            Add<Mult<AM.AMEstimateItem.materialCost, Add<decimal1, Div<AM.AMEstimateItem.materialMarkupPct, decimal100>>>,
            Add<Mult<AM.AMEstimateItem.toolCost, Add<decimal1, Div<AM.AMEstimateItem.toolMarkupPct, decimal100>>>,
            Add<Mult<AM.AMEstimateItem.subcontractCost, Add<decimal1, Div<AM.AMEstimateItem.subcontractMarkupPct, decimal100>>>,
            Mult<Add<AM.AMEstimateItem.fixedOverheadCost, AM.AMEstimateItem.variableOverheadCost>,
            Add<decimal1, Div<AM.AMEstimateItem.overheadMarkupPct, decimal100>>>>>>>>>>))]
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
        [PXDBCurrency(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.unitPrice), BaseCalc = false)]
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
        [AMDBCurrencyBase(typeof(AM.AMEstimateItem.curyInfoID), typeof(AM.AMEstimateItem.curyUnitPrice))]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Price", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AM.AMEstimateItem.orderQty, Equal<decimal0>>, decimal0,
            Case<Where<AM.AMEstimateItem.orderQty, NotEqual<decimal0>>, Div<AM.AMEstimateItem.extPrice, AM.AMEstimateItem.orderQty>>>>))]
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
        [PXDBBool]
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
        #region Image URL
        public abstract class imageURL : PX.Data.BQL.BqlString.Field<imageURL> { }

        protected String _ImageURL;
        [PXDBString(255)]
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
        [PXDBText(IsUnicode = true)]
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
        [PXDBInt]
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
    }
}