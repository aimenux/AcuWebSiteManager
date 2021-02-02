using System;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Cost Roll History")]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMBomCostHistory : IBqlTable, IBomRevision
    {
        internal string DebuggerDisplay => $"BOMID = {BOMID}, RevisionID = {RevisionID}, StartDate = {StartDate}";

        #region Keys

        public class PK : PrimaryKeyOf<AMBomCostHistory>.By<bOMID, revisionID, startDate>
        {
            public static AMBomCostHistory Find(PXGraph graph, string bOMID, string revisionID, DateTime startDate)
                => FindBy(graph, bOMID, revisionID, startDate);


            public static AMBomCostHistory FindDirty(PXGraph graph, string bOMID, string revisionID, DateTime startDate)
                => PXSelect<AMBomCostHistory,
                    Where<bOMID, Equal<Required<bOMID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<startDate, Equal<Required<startDate>>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, revisionID, startDate);
        }
        
        public static class FK
        {
            public class BOM : AMBomItem.PK.ForeignKeyOf<AMBomCostHistory>.By<bOMID, revisionID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMBomCostHistory>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMBomCostHistory>.By<siteID> { }
        }
        
        #endregion

        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected String _BOMID;
        [BomID(IsKey = true, Enabled = false)]
        public virtual String BOMID
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
            protected String _RevisionID;
            [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
            [PXUIField(DisplayName = "Revision", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
            [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
            public virtual String RevisionID
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
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        protected DateTime? _StartDate;
        [PXDBDate(IsKey = true, UseTimeZone = true)]
        [PXUIField(DisplayName = "Start Date")]
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
        [PXUIField(DisplayName = "End Date")]
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
        #region MatlManufacturedCost
        public abstract class matlManufacturedCost : PX.Data.BQL.BqlDecimal.Field<matlManufacturedCost> { }
        protected Decimal? _MatlManufacturedCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        #region MatlCost
        public abstract class matlCost : PX.Data.BQL.BqlDecimal.Field<matlCost> { }
        protected Decimal? _MatlCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Material", Enabled = false)]
        [PXFormula(typeof(Add<AMBomCostHistory.matlManufacturedCost, AMBomCostHistory.matlNonManufacturedCost>))]
        public virtual Decimal? MatlCost
        {
            get
            {
                return this._MatlCost;
            }
            set
            {
                this._MatlCost = value;
            }
        }
        #endregion
        #region Fixed Labor Cost
        public abstract class fLaborCost : PX.Data.BQL.BqlDecimal.Field<fLaborCost> { }
        protected Decimal? _FLaborCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Labor", Enabled = false)]
        public virtual Decimal? FLaborCost
        {
            get
            {
                return this._FLaborCost;
            }
            set
            {
                this._FLaborCost = value;
            }
        }
        #endregion
        #region Variable Labor Cost
        public abstract class vlaborCost : PX.Data.BQL.BqlDecimal.Field<vlaborCost> { }
        protected Decimal? _VLaborCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Labor", Enabled = false)]
        public virtual Decimal? VLaborCost
        {
            get
            {
                return this._VLaborCost;
            }
            set
            {
                this._VLaborCost = value;
            }
        }
        #endregion
        #region MachCost
        public abstract class machCost : PX.Data.BQL.BqlDecimal.Field<machCost> { }
        protected Decimal? _MachCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Machine", Enabled = false)]
        public virtual Decimal? MachCost
        {
            get
            {
                return this._MachCost;
            }
            set
            {
                this._MachCost = value;
            }
        }
        #endregion
        #region OutsideCost
        public abstract class outsideCost : PX.Data.BQL.BqlDecimal.Field<outsideCost> { }
        protected Decimal? _OutsideCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Outside", Enabled = false)]
        public virtual Decimal? OutsideCost
        {
            get
            {
                return this._OutsideCost;
            }
            set
            {
                this._OutsideCost = value;
            }
        }
        #endregion
        #region DirectCost
        public abstract class dirCost : PX.Data.BQL.BqlDecimal.Field<dirCost> { }
        protected Decimal? _DirectCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Direct", Enabled = false)]
        public virtual Decimal? DirectCost
        {
            get
            {
                return this._DirectCost;
            }
            set
            {
                this._DirectCost = value;
            }
        }
        #endregion
        #region FOvdCost
        public abstract class fOvdCost : PX.Data.BQL.BqlDecimal.Field<fOvdCost> { }
        protected Decimal? _FOvdCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Fixed Overhead", Enabled = false)]
        public virtual Decimal? FOvdCost
        {
            get
            {
                return this._FOvdCost;
            }
            set
            {
                this._FOvdCost = value;
            }
        }
        #endregion
        #region VOvdCost
        public abstract class vOvdCost : PX.Data.BQL.BqlDecimal.Field<vOvdCost> { }
        protected Decimal? _VOvdCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Variable Overhead", Enabled = false)]
        public virtual Decimal? VOvdCost
        {
            get
            {
                return this._VOvdCost;
            }
            set
            {
                this._VOvdCost = value;
            }
        }
        #endregion
        #region SubcontractMaterialCost
        public abstract class subcontractMaterialCost : PX.Data.BQL.BqlDecimal.Field<subcontractMaterialCost> { }
        protected Decimal? _SubcontractMaterialCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Subcontract", Enabled = false)]
        public virtual Decimal? SubcontractMaterialCost
        {
            get
            {
                return this._SubcontractMaterialCost;
            }
            set
            {
                this._SubcontractMaterialCost = value;
            }
        }
        #endregion
        #region ReferenceMaterialCost
        public abstract class referenceMaterialCost : PX.Data.BQL.BqlDecimal.Field<referenceMaterialCost> { }
        protected Decimal? _ReferenceMaterialCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Ref. Material", Enabled = false)]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [InventoryItemNoRestrict]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(typeof(AMBomCost.inventoryID), Enabled = false)]
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
        // Do not use the SiteAttribute as it causes issues when access by warehouse is configured
        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(INSite.siteID), SubstituteKey = typeof(INSite.siteCD), DescriptionField = typeof(INSite.descr), ValidateValue = false)]
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
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        protected Decimal? _UnitCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Unit Cost", Enabled = false)]
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
        #region ToolCost
        public abstract class toolCost : PX.Data.BQL.BqlDecimal.Field<toolCost> { }
        protected Decimal? _ToolCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0000")]
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
        #region LotSize
        public abstract class lotSize : PX.Data.BQL.BqlDecimal.Field<lotSize> { }
        protected decimal? _LotSize;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0000")]
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
        #region TotalCost
        public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        protected decimal? _TotalCost;
        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        public virtual decimal? TotalCost
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
        #region MultiLevelProcess 
        public abstract class multiLevelProcess : PX.Data.BQL.BqlBool.Field<multiLevelProcess> { }
        protected Boolean? _MultiLevelProcess;
        /// <summary>
        /// Indicates if the record was processed using Multi level
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Multi Level", Enabled = false, Visible = false)]
        public virtual Boolean? MultiLevelProcess
        {
            get
            {

                return this._MultiLevelProcess;
            }
            set
            {
                this._MultiLevelProcess = value;
            }
        }
        #endregion
        #region Level
        public abstract class level : PX.Data.BQL.BqlInt.Field<level> { }
        protected Int32? _Level;
        /// <summary>
        /// BOM Level based on the entered cost roll filter criteria (not max low level).
        /// This field is important to the order of processing cost roll boms. Highest value (lowest level) are calculated first
        /// </summary>
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Level")]
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
        #region IsDefaultBom 
        public abstract class isDefaultBom : PX.Data.BQL.BqlBool.Field<isDefaultBom> { }
        protected Boolean? _IsDefaultBom;
        /// <summary>
        /// Is the given BOM a default BOM
        /// </summary>
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Default BOM", Enabled = false, Visible = false)]
        public virtual Boolean? IsDefaultBom
        {
            get
            {

                return this._IsDefaultBom;
            }
            set
            {
                this._IsDefaultBom = value;
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
        [PXUIField(DisplayName = "Created Date Time")]
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
        #region FixedLaborTime
        public abstract class fixedLaborTime : PX.Data.IBqlField
        {
        }
        protected Int32? _FixedLaborTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
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
        public abstract class variableLaborTime : PX.Data.IBqlField
        {
        }
        protected Int32? _VariableLaborTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
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
        public abstract class machineTime : PX.Data.IBqlField
        {
        }
        protected Int32? _MachineTime;
        [OperationDBTime]
        [PXDefault(TypeCode.Int32, "0")]
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
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
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
        #region StdCost

        public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
        protected Decimal? _StdCost;
        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Current Cost", Enabled = false)]
        public virtual Decimal? StdCost
        {
            get
            {
                return this._StdCost;
            }
            set
            {
                this._StdCost = value;
            }
        }
        #endregion
        #region PendingStdCost

        public abstract class pendingStdCost : PX.Data.BQL.BqlDecimal.Field<pendingStdCost> { }
        protected Decimal? _PendingStdCost;
        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Pending Cost")]
        public virtual Decimal? PendingStdCost
        {
            get
            {
                return this._PendingStdCost;
            }
            set
            {
                this._PendingStdCost = value;
            }
        }
        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "Last Updated Date Time")]
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
