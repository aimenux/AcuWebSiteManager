using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Report In Memory DAC (Not an actual table) for the MLB reports
    /// </summary>
    [PXCacheName("Multi Level BOM Report Detail")]
    [Serializable]
    public class AMMultiLevelBomData : IBqlTable
	{
        #region ParentBOMID
        public abstract class parentBOMID : PX.Data.BQL.BqlString.Field<parentBOMID> { }

        protected String _ParentBOMID;
        [BomID(IsKey = true)]
        public virtual String ParentBOMID
        {
            get
            {
                return this._ParentBOMID;
            }
            set
            {
                this._ParentBOMID = value;
            }
        }
        #endregion
        #region ActiveFlg
        public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

        protected Boolean? _ActiveFlg;
        [PXBool]
        public virtual Boolean? ActiveFlg
        {
            get
            {
                return this._ActiveFlg;
            }
            set
            {
                this._ActiveFlg = value;
            }
        }
        #endregion
        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXInt(IsKey = true)]
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
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }

        protected Int32? _Level;
        [PXInt(IsKey = true)]
        public virtual Int32? Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory]
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
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXString(255, IsFixed = false)]
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
        #region ParentInventoryID
        public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

        protected Int32? _ParentInventoryID;
        [Inventory]
        public virtual Int32? ParentInventoryID
        {
            get
            {
                return this._ParentInventoryID;
            }
            set
            {
                this._ParentInventoryID = value;
            }
        }
        #endregion
        #region ParentSubItemID
        public abstract class parentSubItemID : PX.Data.BQL.BqlInt.Field<parentSubItemID> { }

        protected Int32? _ParentSubItemID;
        [SubItem]
        public virtual Int32? ParentSubItemID
        {
            get
            {
                return this._ParentSubItemID;
            }
            set
            {
                this._ParentSubItemID = value;
            }
        }
        #endregion
        #region ParentDescr
        public abstract class parentDescr : PX.Data.BQL.BqlString.Field<parentDescr> { }

        protected String _ParentDescr;
        [PXString(255, IsFixed = false)]
        public virtual String ParentDescr
        {
            get
            {
                return this._ParentDescr;
            }
            set
            {
                this._ParentDescr = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit]
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
        #region ScrapFactor
        public abstract class scrapFactor : PX.Data.BQL.BqlDecimal.Field<scrapFactor> { }

        protected Decimal? _ScrapFactor;
        [PXDecimal(6)]
        public virtual Decimal? ScrapFactor
        {
            get
            {
                return this._ScrapFactor;
            }
            set
            {
                this._ScrapFactor = value;
            }
        }
        #endregion
        #region QtyReq
        public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

        protected Decimal? _QtyReq;
        [PXQuantity]
        public virtual Decimal? QtyReq
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
        #region BaseQtyReq
        public abstract class baseQtyReq : PX.Data.BQL.BqlDecimal.Field<baseQtyReq> { }

        protected Decimal? _BaseQtyReq;
        [PXQuantity]
        public virtual Decimal? BaseQtyReq
        {
            get
            {
                return this._BaseQtyReq;
            }
            set
            {
                this._BaseQtyReq = value;
            }
        }
        #endregion
        #region TotalQtyReq
        public abstract class totalQtyReq : PX.Data.BQL.BqlDecimal.Field<totalQtyReq> { }

        protected Decimal? _TotalQtyReq;
        [PXQuantity]
        public virtual Decimal? TotalQtyReq
        {
            get
            {
                return this._TotalQtyReq;
            }
            set
            {
                this._TotalQtyReq = value;
            }
        }
        #endregion
        #region BaseTotalQtyReq
        public abstract class baseTotalQtyReq : PX.Data.BQL.BqlDecimal.Field<baseTotalQtyReq> { }

        protected Decimal? _BaseTotalQtyReq;
        [PXQuantity]
        public virtual Decimal? BaseTotalQtyReq
        {
            get
            {
                return this._BaseTotalQtyReq;
            }
            set
            {
                this._BaseTotalQtyReq = value;
            }
        }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXPriceCost]
        public virtual Decimal? UnitCost
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
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        protected Decimal? _ExtCost;
        [PXPriceCost]
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
        #region TotalExtCost
        public abstract class totalExtCost : PX.Data.BQL.BqlDecimal.Field<totalExtCost> { }

        protected Decimal? _TotalExtCost;
        [PXPriceCost]
        public virtual Decimal? TotalExtCost
        {
            get
            {
                return this._TotalExtCost;
            }
            set
            {
                this._TotalExtCost = value;
            }
        }
        #endregion
        #region LineBOMID
        public abstract class lineBOMID : PX.Data.BQL.BqlString.Field<lineBOMID> { }

        protected String _LineBOMID;
        [BomID(IsKey = true)]
        public virtual String LineBOMID
        {
            get
            {
                return this._LineBOMID;
            }
            set
            {
                this._LineBOMID = value;
            }
        }
        #endregion
        #region OperationCD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD> { }

        protected string _OperationCD;
        [OperationCDField(IsKey = true)]
        public virtual string OperationCD
        {
            get { return this._OperationCD; }
            set { this._OperationCD = value; }
        }

        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

	    protected int? _OperationID;
	    [OperationIDField]
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
        #region EffEndDate
        public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }

        protected DateTime? _EffEndDate;
        [PXDate]
        public virtual DateTime? EffEndDate
        {
            get
            {
                return this._EffEndDate;
            }
            set
            {
                this._EffEndDate = value;
            }
        }
        #endregion
        #region EffStartDate
        public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

        protected DateTime? _EffStartDate;
        [PXDate]
        public virtual DateTime? EffStartDate
        {
            get
            {
                return this._EffStartDate;
            }
            set
            {
                this._EffStartDate = value;
            }
        }
        #endregion
	    #region RevisionID
	    public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }

	    protected string _RevisionID;
	    [RevisionIDField(IsKey = true)]
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
        #region IncludeCosts
        [Obsolete]
        public abstract class includeCosts : PX.Data.BQL.BqlBool.Field<includeCosts> { }

        protected bool? _IncludeCosts;
        [Obsolete]
        [PXBool]
        [PXUIField(DisplayName = "Include Material Costs", Visibility = PXUIVisibility.Invisible)]
        public virtual bool? IncludeCosts
        {
            get { return this._IncludeCosts; }
            set { this._IncludeCosts = value; }
        }

        #endregion
	    #region BOMQtyReq
	    public abstract class bOMQtyReq : PX.Data.BQL.BqlDecimal.Field<bOMQtyReq> { }

	    protected Decimal? _BOMQtyReq;
	    [PXQuantity]
	    public virtual Decimal? BOMQtyReq
        {
	        get
	        {
	            return this._BOMQtyReq;
	        }
	        set
	        {
	            this._BOMQtyReq = value;
	        }
	    }
        #endregion
	    #region BatchSize
	    public abstract class batchSize : PX.Data.BQL.BqlDecimal.Field<batchSize> { }

	    protected Decimal? _BatchSize;
	    [PXQuantity]
	    public virtual Decimal? BatchSize
        {
	        get
	        {
	            return this._BatchSize;
	        }
	        set
	        {
	            this._BatchSize = value;
	        }
	    }
        #endregion
        #region Status
        public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }

        protected int? _Status;
        [PXInt]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [AMBomStatus.List]
        [PXDefault(AMBomStatus.Hold, PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region MaterialStatus
        public abstract class materialStatus : PX.Data.BQL.BqlInt.Field<materialStatus> { }

        protected int? _MaterialStatus;
        [PXInt]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [AMBomStatus.List]
        public virtual int? MaterialStatus
        {
            get
            {
                return this._MaterialStatus;
            }
            set
            {
                this._MaterialStatus = value;
            }
        }
        #endregion
        #region LineRevisionID
        public abstract class lineRevisionID : PX.Data.BQL.BqlString.Field<lineRevisionID> { }

        protected string _LineRevisionID;
        [RevisionIDField(IsKey = true)]
        public virtual string LineRevisionID
        {
            get
            {
                return this._LineRevisionID;
            }
            set
            {
                this._LineRevisionID = value;
            }
        }
        #endregion
        #region LineStatus
        public abstract class lineStatus : PX.Data.BQL.BqlInt.Field<status> { }

        protected int? _LineStatus;
        [PXInt]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        [AMBomStatus.List]
        public virtual int? LineStatus
        {
            get
            {
                return this._LineStatus;
            }
            set
            {
                this._LineStatus = value;
            }
        }
        #endregion
        #region LotSize
        public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }

        protected decimal? _LotSize;
        [PXQuantity]
        [PXUnboundDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Lot Size", Enabled = false)]
        public virtual decimal? LotSize
        {
            get
            {
                return this._LotSize;
            }
            set
            {
                this._LotSize = value;
            }
        }
        #endregion
        #region FixedLaborTime
        public abstract class fixedLaborTime : PX.Data.BQL.BqlInt.Field<fixedLaborTime> { }

        protected Int32? _FixedLaborTime;
        [OperationNonDBTime]
        [PXUnboundDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Fixed Labor Time")]
        public virtual Int32? FixedLaborTime
        {
            get
            {
                return this._FixedLaborTime;
            }
            set
            {
                this._FixedLaborTime = value;
            }
        }
        #endregion
        #region VariableLaborTime
        public abstract class variableLaborTime : PX.Data.BQL.BqlInt.Field<variableLaborTime> { }

        protected Int32? _VariableLaborTime;
        [OperationNonDBTime]
        [PXUnboundDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Variable Labor Time")]
        public virtual Int32? VariableLaborTime
        {
            get
            {
                return this._VariableLaborTime;
            }
            set
            {
                this._VariableLaborTime = value;
            }
        }
        #endregion
        #region MachineTime
        public abstract class machineTime : PX.Data.BQL.BqlInt.Field<machineTime> { }

        protected Int32? _MachineTime;
        [OperationNonDBTime]
        [PXUnboundDefault(TypeCode.Int32, "0")]
        [PXUIField(DisplayName = "Machine Time")]
        public virtual Int32? MachineTime
        {
            get
            {
                return this._MachineTime;
            }
            set
            {
                this._MachineTime = value;
            }
        }
        #endregion
        #region HasCostRoll
        public abstract class hasCostRoll : PX.Data.BQL.BqlBool.Field<hasCostRoll> { }

        protected bool? _HasCostRoll;
        [PXBool]
        [PXUIField(DisplayName = "Has Cost Roll", Visible = false)]
        public virtual bool? HasCostRoll
        {
            get { return this._HasCostRoll; }
            set { this._HasCostRoll = value; }
        }

        #endregion
        #region ManufacturingBOMID
        public abstract class manufacturingBOMID : PX.Data.BQL.BqlString.Field<manufacturingBOMID> { }

        protected string _ManufacturingBOMID;
        [BomID]
        public virtual string ManufacturingBOMID
        {
            get
            {
                return this._ManufacturingBOMID;
            }
            set
            {
                this._ManufacturingBOMID = value;
            }
        }
        #endregion
        #region ManufacturingRevisionID
        public abstract class manufacturingRevisionID : PX.Data.BQL.BqlString.Field<manufacturingRevisionID> { }

        protected string _ManufacturingRevisionID;
        [RevisionIDField]
        public virtual string ManufacturingRevisionID
        {
            get
            {
                return this._ManufacturingRevisionID;
            }
            set
            {
                this._ManufacturingRevisionID = value;
            }
        }
        #endregion
        #region MatlManufacturedCost
        public abstract class matlManufacturedCost : PX.Data.BQL.BqlDecimal.Field<matlManufacturedCost> { }
        protected Decimal? _MatlManufacturedCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Manufactured Material", Enabled = false, Visible = false)]
        public virtual Decimal? MatlManufacturedCost
        {
            get
            {
                return this._MatlManufacturedCost;
            }
            set
            {
                this._MatlManufacturedCost = value;
            }
        }
        #endregion
        #region MatlNonManufacturedCost
        public abstract class matlNonManufacturedCost : PX.Data.BQL.BqlDecimal.Field<matlNonManufacturedCost> { }
        protected Decimal? _MatlNonManufacturedCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Purchase Material", Enabled = false, Visible = false)]
        public virtual Decimal? MatlNonManufacturedCost
        {
            get
            {
                return this._MatlNonManufacturedCost;
            }
            set
            {
                this._MatlNonManufacturedCost = value;
            }
        }
        #endregion
        #region MaterialCost
        public abstract class materialCost : PX.Data.BQL.BqlDecimal.Field<materialCost> { }

        protected Decimal? _MaterialCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        [PXDependsOnFields(typeof(matlManufacturedCost), typeof(matlNonManufacturedCost))]
        public virtual Decimal? MaterialCost
        {           
            get
            {
                return this.MatlManufacturedCost.GetValueOrDefault() + this.MatlNonManufacturedCost.GetValueOrDefault();
            }
            set
            {
                this._MaterialCost = value;
            }
        }
        #endregion
        #region Fixed Labor Cost
        public abstract class fixedLaborCost : PX.Data.BQL.BqlDecimal.Field<fixedLaborCost> { }

        protected Decimal? _FixedLaborCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fixed Labor", Enabled = false)]
        public virtual Decimal? FixedLaborCost
        {
            get
            {
                return this._FixedLaborCost;
            }
            set
            {
                this._FixedLaborCost = value;
            }
        }
        #endregion
        #region Variable Labor Cost
        public abstract class variableLaborCost : PX.Data.BQL.BqlDecimal.Field<variableLaborCost> { }

        protected Decimal? _VariableLaborCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Variable Labor", Enabled = false)]
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
        #region MachineCost
        public abstract class machineCost : PX.Data.BQL.BqlDecimal.Field<machineCost> { }

        protected Decimal? _MachineCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
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
        #region FixedOvdCost
        public abstract class fixedOvdCost : PX.Data.BQL.BqlDecimal.Field<fixedOvdCost> { }

        protected Decimal? _FixedOvdCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? FixedOvdCost
        {
            get
            {
                return this._FixedOvdCost;
            }
            set
            {
                this._FixedOvdCost = value;
            }
        }
        #endregion
        #region VariableOvdCost
        public abstract class variableOvdCost : PX.Data.BQL.BqlDecimal.Field<variableOvdCost> { }

        protected Decimal? _VariableOvdCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? VariableOvdCost
        {
            get
            {
                return this._VariableOvdCost;
            }
            set
            {
                this._VariableOvdCost = value;
            }
        }
        #endregion
        #region ToolCost
        public abstract class toolCost : PX.Data.BQL.BqlDecimal.Field<toolCost> { }

        protected Decimal? _ToolCost;
        [PXPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tools", Enabled = false)]
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
        #region IsHeaderRecord
        public abstract class isHeaderRecord : PX.Data.BQL.BqlBool.Field<isHeaderRecord> { }

        protected bool? _IsHeaderRecord;
        [PXBool]
        [PXUIField(DisplayName = "Header Record")]
        public virtual bool? IsHeaderRecord
        {
            get { return this._IsHeaderRecord; }
            set { this._IsHeaderRecord = value; }
        }

        #endregion
        #region TotalCost (Unit Cost * Lot Size)
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }

        protected Decimal? _TotalCost;
        [PXPriceCost]
        public virtual Decimal? TotalCost
        {
            get
            {
                return this._TotalCost;
            }
            set
            {
                this._TotalCost = value;
            }
        }
        #endregion
        #region OperationDescription
        public abstract class operationDescription : PX.Data.BQL.BqlString.Field<operationDescription> { }

        protected string _OperationDescription;
        [PXString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Operation Description")]
        public virtual string OperationDescription
        {
            get
            {
                return this._OperationDescription;
            }
            set
            {
                this._OperationDescription = value;
            }
        }
        #endregion
        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID> { }

        protected string _WcID;
        [WorkCenterIDField]
        public virtual string WcID
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
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        protected Int32? _SortOrder;
        [PXUIField(Enabled = false)]
        [PXDBInt(IsKey = true)]
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
    }
}
