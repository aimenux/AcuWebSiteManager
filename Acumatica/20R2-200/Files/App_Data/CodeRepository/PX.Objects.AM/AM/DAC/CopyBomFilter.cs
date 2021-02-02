using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Copy bom filter DAC (Non-table)
    /// </summary>
    [Serializable]
    [PXCacheName("Copy BOM Filter")]
    public class CopyBomFilter : IBqlTable 
    {
        #region FromBOMID
        public abstract class fromBOMID : PX.Data.BQL.BqlString.Field<fromBOMID> { }

        protected String _FromBOMID;
        [BomID(DisplayName = "From BOM ID")]
        public virtual String FromBOMID
        {
            get
            {
                return this._FromBOMID;
            }
            set
            {
                this._FromBOMID = value;
            }
        }
        #endregion
        #region FromRevisionID
        public abstract class fromRevisionID : PX.Data.BQL.BqlString.Field<fromRevisionID> { }

        protected String _FromRevisionID;
        [RevisionIDField(DisplayName = "From Revision")]
        public virtual String FromRevisionID
        {
            get
            {
                return this._FromRevisionID;
            }
            set
            {
                this._FromRevisionID = value;
            }
        }
        #endregion
        #region FromInventoryID

        public abstract class fromInventoryID : PX.Data.BQL.BqlInt.Field<fromInventoryID> { }

        protected Int32? _FromInventoryID;
        [StockItem(DisplayName = "From Inventory ID")]
        [PXDefault]
        public virtual Int32? FromInventoryID
        {
            get
            {
                return this._FromInventoryID;
            }
            set
            {
                this._FromInventoryID = value;
            }
        }
        #endregion
        #region FromSubItemID
        public abstract class fromSubItemID : PX.Data.BQL.BqlInt.Field<fromSubItemID> { }

        protected Int32? _FromSubItemID;
        [SubItem(typeof(CopyBomFilter.fromInventoryID), DisplayName = "From Subitem")]
        public virtual Int32? FromSubItemID
        {
            get
            {
                return this._FromSubItemID;
            }
            set
            {
                this._FromSubItemID = value;
            }
        }
        #endregion
        #region FromSiteID
        public abstract class fromSiteID : PX.Data.BQL.BqlInt.Field<fromSiteID> { }

        protected Int32? _FromSiteID;
        [Site(DisplayName = "From Warehouse")]
        [PXDefault]
        public virtual Int32? FromSiteID
        {
            get
            {
                return this._FromSiteID;
            }
            set
            {
                this._FromSiteID = value;
            }
        }
        #endregion
        
        #region ToBOMID
        public abstract class toBOMID : PX.Data.BQL.BqlString.Field<toBOMID> { }

        protected String _ToBOMID;
        [BomID(DisplayName = "To BOM ID")]
        [PXDefault]
        public virtual String ToBOMID
        {
            get
            {
                return this._ToBOMID;
            }
            set
            {
                this._ToBOMID = value;
            }
        }
        #endregion
        #region ToRevisionID
        public abstract class toRevisionID : PX.Data.BQL.BqlString.Field<toRevisionID> { }

        protected String _ToRevisionID;
        [RevisionIDField(DisplayName = "To Revision")]
        [PXDefault(typeof(AMBSetup.defaultRevisionID))]
        public virtual String ToRevisionID
        {
            get
            {
                return this._ToRevisionID;
            }
            set
            {
                this._ToRevisionID = value;
            }
        }
        #endregion
        #region ToInventoryID

        public abstract class toInventoryID : PX.Data.BQL.BqlInt.Field<toInventoryID> { }

        protected Int32? _ToInventoryID;
        [StockItem(DisplayName = "To Inventory ID")]
        [PXDefault]
        public virtual Int32? ToInventoryID
        {
            get
            {
                return this._ToInventoryID;
            }
            set
            {
                this._ToInventoryID = value;
            }
        }
        #endregion
        #region ToSubItemID
        public abstract class toSubItemID : PX.Data.BQL.BqlInt.Field<toSubItemID> { }

        protected Int32? _ToSubItemID;
        [SubItem(typeof(CopyBomFilter.toInventoryID), Visible = false)]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<CopyBomFilter.toInventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<PX.Objects.CS.boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<CopyBomFilter.toInventoryID>))]
        public virtual Int32? ToSubItemID
        {
            get
            {
                return this._ToSubItemID;
            }
            set
            {
                this._ToSubItemID = value;
            }
        }
        #endregion
        #region ToSubItemCD
        // Needed CD value because the entered combination might not exist and allow on the fly entry could be true
        /// <summary>
        /// User entered CD value used to lookup the ToSubItemID int value
        /// </summary>
        public abstract class toSubItemCD : PX.Data.BQL.BqlString.Field<toSubItemCD> { }

        protected String _ToSubItemCD;
        /// <summary>
        /// User entered CD value used to lookup the ToSubItemID int value.
        /// </summary>
        [AMSubItemRaw(typeof(INSiteStatusFilter.inventoryID), DisplayName = "To Subitem")]
        [PXDefault(typeof(Search2<INSubItem.subItemCD,
            InnerJoin<InventoryItem, On<INSubItem.subItemID, Equal<InventoryItem.defaultSubItemID>>>, 
            Where<InventoryItem.inventoryID, Equal<Current<CopyBomFilter.toInventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<PX.Objects.CS.boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<CopyBomFilter.toInventoryID>))]
        public virtual String ToSubItemCD
        {
            get
            {
                return this._ToSubItemCD;
            }
            set
            {
                this._ToSubItemCD = value;
            }
        }
        #endregion
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }

        protected Int32? _ToSiteID;
        [Site(DisplayName = "To Warehouse")]
        [PXDefault]
        public virtual Int32? ToSiteID
        {
            get
            {
                return this._ToSiteID;
            }
            set
            {
                this._ToSiteID = value;
            }
        }
        #endregion
        #region  UpdateMaterialWarehouse
        public abstract class updateMaterialWarehouse : PX.Data.BQL.BqlBool.Field<updateMaterialWarehouse> { }

        protected Boolean? _UpdateMaterialWarehouse;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Update material warehouse")]
        public virtual Boolean? UpdateMaterialWarehouse
        {
            get
            {
                return this._UpdateMaterialWarehouse;
            }
            set
            {
                this._UpdateMaterialWarehouse = value;
            }
        }
        #endregion

        #region EffStartDate
        public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }

        protected DateTime? _EffStartDate;
        [PXDate(IsKey = true)]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Eff. Start Date", Enabled = false, Visible = false)]
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

        #region CopyNotesItem
        public abstract class copyNotesItem : PX.Data.BQL.BqlBool.Field<copyNotesItem> { }

        protected Boolean? _CopyNotesItem;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "BOM Header")]
        public virtual Boolean? CopyNotesItem
        {
            get
            {
                return this._CopyNotesItem;
            }
            set
            {
                this._CopyNotesItem = value;
            }
        }
        #endregion
        #region CopyNotesOper
        public abstract class copyNotesOper : PX.Data.BQL.BqlBool.Field<copyNotesOper> { }

        protected Boolean? _CopyNotesOper;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Operation")]
        public virtual Boolean? CopyNotesOper
        {
            get
            {
                return this._CopyNotesOper;
            }
            set
            {
                this._CopyNotesOper = value;
            }
        }
        #endregion
        #region CopyNotesMatl
        public abstract class copyNotesMatl : PX.Data.BQL.BqlBool.Field<copyNotesMatl> { }

        protected Boolean? _CopyNotesMatl;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Material")]
        public virtual Boolean? CopyNotesMatl
        {
            get
            {
                return this._CopyNotesMatl;
            }
            set
            {
                this._CopyNotesMatl = value;
            }
        }
        #endregion
        #region CopyNotesStep
        public abstract class copyNotesStep : PX.Data.BQL.BqlBool.Field<copyNotesStep> { }

        protected Boolean? _CopyNotesStep;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Step")]
        public virtual Boolean? CopyNotesStep
        {
            get
            {
                return this._CopyNotesStep;
            }
            set
            {
                this._CopyNotesStep = value;
            }
        }
        #endregion
        #region CopyNotesTool
        public abstract class copyNotesTool : PX.Data.BQL.BqlBool.Field<copyNotesTool> { }

        protected Boolean? _CopyNotesTool;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Tool")]
        public virtual Boolean? CopyNotesTool
        {
            get
            {
                return this._CopyNotesTool;
            }
            set
            {
                this._CopyNotesTool = value;
            }
        }
        #endregion
        #region CopyNotesOvhd
        public abstract class copyNotesOvhd : PX.Data.BQL.BqlBool.Field<copyNotesOvhd> { }

        protected Boolean? _CopyNotesOvhd;
        [PXBool]
        [PXUnboundDefault(true)]
        [PXUIField(DisplayName = "Overhead")]
        public virtual Boolean? CopyNotesOvhd
        {
            get
            {
                return this._CopyNotesOvhd;
            }
            set
            {
                this._CopyNotesOvhd = value;
            }
        }
        #endregion
    }
}
