using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    /// <summary>
    /// BOM Item (Master BOM Header Record)
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.BOMItem)]
    [PXPrimaryGraph(typeof(BOMMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMBomItem : IBqlTable, INotable
    {
        internal string DebuggerDisplay => $"BOMID = {BOMID}, RevisionID = {RevisionID}, InventoryID = {InventoryID}, SiteID = {SiteID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMBomItem>.By<bOMID, revisionID>
        {
            public static AMBomItem Find(PXGraph graph, string bOMID, string revisionID)
                => FindBy(graph, bOMID, revisionID);
            public static AMBomItem FindDirty(PXGraph graph, string bOMID, string revisionID)
                => PXSelect<AMBomItem,
                        Where<bOMID, Equal<Required<bOMID>>,
                            And<revisionID, Equal<Required<revisionID>>>>>
                    .SelectWindowed(graph, 0, 1, bOMID, revisionID);
        }

        public static class FK
        {
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMBomItem>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMBomItem>.By<siteID> { }
        }

        #endregion

        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected string _BOMID;
        [BomID(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXReferentialIntegrityCheck]
        [PXSelector(typeof(Search2<AMBomItem.bOMID,
            InnerJoin<AMBomItemBomAggregate,
                On<AMBomItem.bOMID, Equal<AMBomItemBomAggregate.bOMID>,
                    And<AMBomItem.revisionID, Equal<AMBomItemBomAggregate.revisionID>>>>>))]
        public virtual string BOMID
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
        protected string _RevisionID;
        [RevisionIDField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, Required = true)]
        [PXDefault(typeof(AMBSetup.defaultRevisionID))]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
            Where<AMBomItem.bOMID, Equal<Optional<AMBomItem.bOMID>>,
                Or<AMBomItem.bOMID, Equal<Current<AMBomItem.bOMID>>>>>),
            typeof(AMBomItem.revisionID),
            typeof(AMBomItem.status),
            typeof(AMBomItem.descr),
            typeof(AMBomItem.effStartDate),
            typeof(AMBomItem.effEndDate))]
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
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
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
        #region EffStartDate
        public abstract class effStartDate : PX.Data.BQL.BqlDateTime.Field<effStartDate> { }
        protected DateTime? _EffStartDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Start Date")]
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
        #region EffEndDate
        public abstract class effEndDate : PX.Data.BQL.BqlDateTime.Field<effEndDate> { }
        protected DateTime? _EffEndDate;
        [PXDBDate]
        [PXUIField(DisplayName = "End Date")]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [StockItem(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMBomItem.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<PX.Objects.CS.boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMBomItem.inventoryID), Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Default<AMBomItem.inventoryID>))]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXSearchable(PX.Objects.SM.SearchCategory.IN, Messages.BOMSearchableTitleDocument, new[] { typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID) },
            new Type[] { typeof(AMBomItem.descr) },
            NumberFields = new Type[] { typeof(AMBomItem.bOMID) },
            Line1Format = "{1}{2:d}", Line1Fields = new Type[] { typeof(AMBomItem.inventoryID), typeof(InventoryItem.inventoryCD), typeof(AMBomItem.effStartDate) },
            Line2Format = "{0}", Line2Fields = new Type[] { typeof(AMBomItem.descr) }
        )]
        [PXNote(DescriptionField = typeof(bOMID), Selector = typeof(bOMID))]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        [Site(Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
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
        #region LineCntrOperation
        public abstract class lineCntrOperation : PX.Data.BQL.BqlInt.Field<lineCntrOperation> { }
        protected int? _LineCntrOperation;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Operation Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrOperation
        {
            get
            {
                return this._LineCntrOperation;
            }
            set
            {
                this._LineCntrOperation = value;
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
        #region Status
        public abstract class status : PX.Data.BQL.BqlInt.Field<status> { }
        protected int? _Status;
        [PXDBInt]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [AMBomStatus.List]
        [PXDefault(AMBomStatus.Hold)]
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
        #region OwnerID
        public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
        protected int? _OwnerID;
        [PX.TM.Owner]
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
        #region WorkgroupID
        public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
        protected int? _WorkgroupID;
        [PXDBInt]
        [PX.TM.PXCompanyTreeSelector]
        [PXUIField(DisplayName = "Workgroup", Enabled = false)]
        public virtual int? WorkgroupID
        {
            get
            {
                return this._WorkgroupID;
            }
            set
            {
                this._WorkgroupID = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
        protected Boolean? _Hold;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Hold")]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region Approved
        public abstract class approved : PX.Data.BQL.BqlBool.Field<approved> { }
        protected Boolean? _Approved;
        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Approved", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Boolean? Approved
        {
            get
            {
                return this._Approved;
            }
            set
            {
                this._Approved = value;
            }
        }
        #endregion
        #region Rejected
        public abstract class rejected : PX.Data.BQL.BqlBool.Field<rejected> { }
        protected bool? _Rejected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public bool? Rejected
        {
            get
            {
                return _Rejected;
            }
            set
            {
                _Rejected = value;
            }
        }
        #endregion
        #region ActiveFlg (Unbound)
        // 2018R2 - KEEP FIELD AS UNBOUND TO SUPPORT CONTRACT ENDPOINT VERSION 18R1
        /// <summary>
        /// Unbound field indicating BOM Status = Active
        /// </summary>
        public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }
        /// <summary>
        /// Unbound field indicating BOM Status = Active
        /// </summary>
        [PXBool]
        [PXUIField(DisplayName = "Active", Enabled = false, Visible = false)]
        [PXDependsOnFields(typeof(AMBomItem.status))]
        public virtual Boolean? ActiveFlg => this._Status == AMBomStatus.Active;

        #endregion
    }

    /// <summary>
    /// BOM item inventory only
    /// </summary>
    [PXProjection(typeof(Select2<InventoryItem, InnerJoin<AMBomItemByInventoryID, On<AMBomItemByInventoryID.inventoryID, Equal<InventoryItem.inventoryID>>>>), Persistent = false)]
    [Serializable]
    [PXCacheName("BOM Inventory Item")]
    public class BomInventoryItem : InventoryItem
    {
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
    }

    /// <summary>
    /// Aggregate of inventory items with BOMs
    /// </summary>
    [PXProjection(typeof(Select4<AMBomItem, Where<AMBomItem.status, NotEqual<AMBomStatus.archived>>, Aggregate<GroupBy<AMBomItem.inventoryID>>>), Persistent = false)]
    [Serializable]
    [PXHidden]
    public class AMBomItemByInventoryID : IBqlTable
    {
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        [PXDBInt(BqlField = typeof(AMBomItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
        public virtual Int32? InventoryID { get; set; }
        #endregion
    }

    /// <summary>
    /// Projection for max valid revision and to display unique bom ids (excludes Archived boms)
    /// </summary>
    [Serializable]
    [PXCacheName("BOM Item Active")]
    [PXProjection(typeof(Select2<AMBomItem,
        InnerJoin<AMBomItemNotArchivedAggregate,
            On<AMBomItem.bOMID, Equal<AMBomItemNotArchivedAggregate.bOMID>,
                And<AMBomItem.revisionID, Equal<AMBomItemNotArchivedAggregate.revisionID>>>>>))]
    public class AMBomItemActive : AMBomItem
    {
        public new abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
}

    /// <summary>
    /// Projection for max valid revision and to display unique bom ids for Active BOMs only
    /// </summary>
    [Serializable]
    [PXCacheName("BOM Item Active 2")]
    [PXProjection(typeof(Select2<AMBomItem,
        InnerJoin<AMBomItemActiveAggregate,
            On<AMBomItem.bOMID, Equal<AMBomItemActiveAggregate.bOMID>,
                And<AMBomItem.revisionID, Equal<AMBomItemActiveAggregate.revisionID>>>>>))]
    public class AMBomItemActive2 : AMBomItem
    {
        public new abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        public new abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        public new abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
    }

    /// <summary>
    /// Aggregate of Revisions. Use this is a sub-query on BOM's to get Max Revision that are not Archived
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select4<AMBomItem,
        Where<AMBomItem.status, NotEqual<AMBomStatus.archived>>,
        Aggregate<
            GroupBy<AMBomItem.bOMID>>>), Persistent = false)]
    public class AMBomItemNotArchivedAggregate : IBqlTable
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected String _BOMID;
        [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
        [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
    }

    /// <summary>
    /// Aggregate of Revisions. Use this is a sub-query on BOM's to get Max Revision and display only BOMs that are active
    /// </summary>
    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select4<AMBomItem,
        Where<AMBomItem.status, Equal<AMBomStatus.active>>,
        Aggregate<
            GroupBy<AMBomItem.bOMID>>>), Persistent = false)]
    public class AMBomItemActiveAggregate : IBqlTable
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected String _BOMID;
        [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
        [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
    }

    [PXHidden]
    [Serializable]
    [PXProjection(typeof(Select4<AMBomItem,
        Aggregate<
            GroupBy<AMBomItem.bOMID>>>), Persistent = false)]
    public class AMBomItemBomAggregate : IBqlTable
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected String _BOMID;
        [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
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
        [RevisionIDField(BqlField = typeof(AMBomItem.revisionID))]
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
    }

    /// <summary>
    /// Projection of <see cref="AMBomItem"/> that includes fields indicating default boms
    /// </summary>
    [PXProjection(typeof(Select2<AMBomItem, 
        LeftJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMBomItem.inventoryID>>,
        LeftJoin<INItemSite, On<INItemSite.inventoryID, Equal<AMBomItem.inventoryID>,
            And<INItemSite.siteID, Equal<AMBomItem.siteID>>>>>>), Persistent = false)]
    [Serializable]
    [PXCacheName("BOM Item BOM Default")]
    public class AMBomItemBomDefaults : IBqlTable
    {
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }
        protected string _BOMID;
        [BomID(IsKey = true, BqlField = typeof(AMBomItem.bOMID))]
        public virtual string BOMID
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
        protected string _RevisionID;
        [RevisionIDField(IsKey = true, BqlField = typeof(AMBomItem.revisionID))]
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

        #region ItemBOMID
        /// <summary>
        /// Default InventoryItem BOM ID
        /// </summary>
        public abstract class itemBOMID : PX.Data.BQL.BqlString.Field<itemBOMID> { }
        /// <summary>
        /// Default InventoryItem BOM ID
        /// </summary>
        [BomID(DisplayName = "Item Default BOM ID", BqlField = typeof(InventoryItemExt.aMBOMID))]
        public string ItemBOMID { get; set; }
        #endregion
        #region ItemPlanningBOMID
        /// <summary>
        /// InventoryItem Planning BOM ID
        /// </summary>
        public abstract class itemPlanningBOMID : PX.Data.BQL.BqlString.Field<itemPlanningBOMID> { }
        /// <summary>
        /// InventoryItem Planning BOM ID
        /// </summary>
        [BomID(DisplayName = "Item Planning BOM ID", BqlField = typeof(InventoryItemExt.aMPlanningBOMID))]
        public string ItemPlanningBOMID { get; set; }
        #endregion

        #region ItemSiteBOMID
        /// <summary>
        /// Default InventoryItem BOM ID
        /// </summary>
        public abstract class itemSiteBOMID : PX.Data.BQL.BqlString.Field<itemSiteBOMID> { }
        /// <summary>
        /// Default InventoryItem BOM ID
        /// </summary>
        [BomID(DisplayName = "Site Default BOM ID", BqlField = typeof(INItemSiteExt.aMBOMID))]
        public string ItemSiteBOMID { get; set; }
        #endregion
        #region ItemSitePlanningBOMID
        /// <summary>
        /// InventoryItem Planning BOM ID
        /// </summary>
        public abstract class itemSitePlanningBOMID : PX.Data.BQL.BqlString.Field<itemSitePlanningBOMID> { }
        /// <summary>
        /// InventoryItem Planning BOM ID
        /// </summary>
        [BomID(DisplayName = "Site Planning BOM ID", BqlField = typeof(INItemSiteExt.aMPlanningBOMID))]
        public string ItemSitePlanningBOMID { get; set; }
        #endregion

        #region IsItemDefaultBOM
        public abstract class isItemDefaultBOM : PX.Data.BQL.BqlBool.Field<isItemDefaultBOM> { }

        [PXBool]
        [PXUIField(DisplayName = "Item Default BOM")]
        [PXDependsOnFields(typeof(bOMID), typeof(itemBOMID))]
        public Boolean? IsItemDefaultBOM => ItemBOMID != null && BOMID != null && BOMID.Equals(ItemBOMID);

        #endregion

        #region IsItemSiteDefaultBOM
        public abstract class isItemSiteDefaultBOM : PX.Data.BQL.BqlBool.Field<isItemSiteDefaultBOM> { }

        [PXBool]
        [PXUIField(DisplayName = "Site Default BOM")]
        [PXDependsOnFields(typeof(bOMID), typeof(itemSiteBOMID))]
        public Boolean? IsItemSiteDefaultBOM => ItemSiteBOMID != null && BOMID != null && BOMID.Equals(ItemSiteBOMID);

        #endregion

        #region IsDefaultBOM
        public abstract class isDefaultBOM : PX.Data.BQL.BqlBool.Field<isDefaultBOM> { }

        [PXBool]
        [PXUIField(DisplayName = "Default BOM")]
        [PXDependsOnFields(typeof(bOMID), typeof(itemBOMID), typeof(itemSiteBOMID), typeof(isItemSiteDefaultBOM), typeof(isItemDefaultBOM))]
        public Boolean? IsDefaultBOM => IsItemDefaultBOM == true || IsItemSiteDefaultBOM == true;

        #endregion
    }
}
