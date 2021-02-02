using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(DisassemblyEntry))]
    [PXCacheName(Messages.AMDisassembleBatchSplit)]
    [PXProjection(typeof(Select<AMMTranSplit>), Persistent = true)]
    [System.Diagnostics.DebuggerDisplay("BatNbr = {BatNbr}, LineNbr = {LineNbr}, SplitLineNbr = {SplitLineNbr}, TranType = {TranType}, LotSerialNbr = {LotSerialNbr}")]
    public class AMDisassembleBatchSplit : IBqlTable, ILSDetail, IAMBatch
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
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true, BqlField = typeof(AMMTranSplit.tranType))]
        [PXDefault(typeof(AMDisassembleBatch.tranType))]
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
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMMTranSplit.docType))]
        [PXDBDefault(typeof(AMDisassembleBatch.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMMTranSplit.batNbr))]
        [PXDBDefault(typeof(AMDisassembleBatch.batchNbr))]
        [PXParent(typeof(Select<AMDisassembleBatch, 
            Where<AMDisassembleBatch.docType, Equal<Current<AMDisassembleBatchSplit.docType>>, 
            And<AMDisassembleBatch.batchNbr, Equal<Current<AMDisassembleBatchSplit.batNbr>>, 
            And<AMDisassembleBatch.refLineNbr, Equal<Current<AMDisassembleBatchSplit.lineNbr>>>>>>))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.lineNbr))]
        [PXDBDefault(typeof(AMDisassembleBatch.refLineNbr))]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.splitLineNbr))]
        [PXLineNbr(typeof(AMDisassembleBatch.lotSerCntr))]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate(BqlField = typeof(AMMTranSplit.tranDate))]
        [PXDBDefault(typeof(AMDisassembleBatch.tranDate))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory(Visible = false, BqlField = typeof(AMMTranSplit.inventoryID))]
        [PXDefault(typeof(AMDisassembleBatch.inventoryID))]
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
        [SubItem(
            typeof(AMDisassembleBatchSplit.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatus.inventoryID, Equal<Optional<AMDisassembleBatchSplit.inventoryID>>,
                And<INSiteStatus.siteID, Equal<Optional<AMDisassembleBatchSplit.siteID>>>>>>),
            BqlField = typeof(AMMTranSplit.subItemID))]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMDisassembleBatchSplit.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>))]
        [PXFormula(typeof(Default<AMDisassembleBatchSplit.inventoryID>))]
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
        #region CostSubItemID
        public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }

        protected Int32? _CostSubItemID;
        [PXInt]
        public virtual Int32? CostSubItemID
        {
            get
            {
                return this._CostSubItemID;
            }
            set
            {
                this._CostSubItemID = value;
            }
        }
        #endregion
        #region CostSiteID
        public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }

        protected Int32? _CostSiteID;
        [PXInt]
        public virtual Int32? CostSiteID
        {
            get
            {
                return this._CostSiteID;
            }
            set
            {
                this._CostSiteID = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(BqlField = typeof(AMMTranSplit.siteID))]
        [PXDefault(typeof(AMDisassembleBatch.siteID))]
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
        [MfgLocationAvail(typeof(AMDisassembleBatchSplit.inventoryID), typeof(AMDisassembleBatchSplit.subItemID), 
            typeof(AMDisassembleBatchSplit.siteID), false, true, typeof(AMDisassembleTran.isScrap), 
            typeof(AMDisassembleTran), BqlField = typeof(AMMTranSplit.locationID))]
        [PXDefault]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMDisassembleBatchSplit.inventoryID), typeof(AMDisassembleBatchSplit.subItemID), 
            typeof(AMDisassembleBatchSplit.locationID), typeof(AMDisassembleBatch.lotSerialNbr), 
            FieldClass = "LotSerial", BqlField = typeof(AMMTranSplit.lotSerialNbr))]
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
        #region LotSerClassID
        public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        protected String _LotSerClassID;
        [PXString(10, IsUnicode = true)]
        public virtual String LotSerClassID
        {
            get
            {
                return this._LotSerClassID;
            }
            set
            {
                this._LotSerClassID = value;
            }
        }
        #endregion
        #region AssignedNbr
        public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }

        protected String _AssignedNbr;
        [PXString(30, IsUnicode = true)]
        public virtual String AssignedNbr
        {
            get
            {
                return this._AssignedNbr;
            }
            set
            {
                this._AssignedNbr = value;
            }
        }
        #endregion
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMDisassembleBatchSplit.inventoryID), BqlField = typeof(AMMTranSplit.expireDate))]
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
        #region InvtMult
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort(BqlField = typeof(AMMTranSplit.invtMult))]
        [PXDefault(typeof(AMDisassembleBatch.invtMult))]
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
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool(BqlField = typeof(AMMTranSplit.released))]
        [PXDefault(false)]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit(typeof(AMDisassembleBatchSplit.inventoryID), DisplayName = "UOM", Enabled = false, 
            BqlField = typeof(AMMTranSplit.uOM))]
        [PXDefault(typeof(AMDisassembleBatch.uOM))]
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
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMDisassembleBatchSplit.uOM), typeof(AMDisassembleBatchSplit.baseQty),
            BqlField = typeof(AMMTranSplit.qty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity(BqlField = typeof(AMMTranSplit.baseQty))]
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
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        protected Int64? _PlanID;
        [PXDBLong(BqlField = typeof(AMMTranSplit.planID))]
        public virtual Int64? PlanID
        {
            get
            {
                return this._PlanID;
            }
            set
            {
                this._PlanID = value;
            }
        }
        #endregion
        #region OrigSource
        public abstract class origSource : PX.Data.BQL.BqlString.Field<origSource> { }

        protected String _OrigSource;
        [PXDBString(2, IsUnicode = true, BqlField = typeof(AMMTranSplit.origSource))]
        public virtual String OrigSource
        {
            get
            {
                return this._OrigSource;
            }
            set
            {
                this._OrigSource = value;
            }
        }
        #endregion
        #region OrigBatNbr
        public abstract class origBatNbr : PX.Data.BQL.BqlString.Field<origBatNbr> { }

        protected String _OrigBatNbr;
        [PXDBString(15, IsUnicode = true, BqlField = typeof(AMMTranSplit.origBatNbr))]
        public virtual String OrigBatNbr
        {
            get
            {
                return this._OrigBatNbr;
            }
            set
            {
                this._OrigBatNbr = value;
            }
        }
        #endregion
        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }

        protected Int32? _OrigLineNbr;
        [PXDBInt(BqlField = typeof(AMMTranSplit.origLineNbr))]
        public virtual Int32? OrigLineNbr
        {
            get
            {
                return this._OrigLineNbr;
            }
            set
            {
                this._OrigLineNbr = value;
            }
        }
        #endregion
        #region OrigSplitLineNbr
        public abstract class origSplitLineNbr : PX.Data.BQL.BqlInt.Field<origSplitLineNbr> { }

        protected Int32? _OrigSplitLineNbr;
        [PXDBInt(BqlField = typeof(AMMTranSplit.origSplitLineNbr))]
        public virtual Int32? OrigSplitLineNbr
        {
            get
            {
                return this._OrigSplitLineNbr;
            }
            set
            {
                this._OrigSplitLineNbr = value;
            }
        }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID(BqlField = typeof(AMMTranSplit.createdByID))]
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
        [PXDBCreatedByScreenID(BqlField = typeof(AMMTranSplit.createdByScreenID))]
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
        [PXDBCreatedDateTime(BqlField = typeof(AMMTranSplit.createdDateTime))]
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
        [PXDBLastModifiedByID(BqlField = typeof(AMMTranSplit.lastModifiedByID))]
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
        [PXDBLastModifiedByScreenID(BqlField = typeof(AMMTranSplit.lastModifiedByScreenID))]
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
        [PXDBLastModifiedDateTime(BqlField = typeof(AMMTranSplit.lastModifiedDateTime))]
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
        [PXDBTimestamp(BqlField = typeof(AMMTranSplit.Tstamp))]
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
        #region Is Stock Item
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

        protected bool? _IsStockItem;
        [PXDBBool(BqlField = typeof(AMMTranSplit.isStockItem))]
        [PXUIField(DisplayName = "Stock Item")]
        [PXFormula(typeof(Selector<AMDisassembleBatchSplit.inventoryID, InventoryItem.stkItem>))]
        public virtual bool? IsStockItem
        {
            get
            {
                return this._IsStockItem;
            }
            set
            {
                this._IsStockItem = value;
            }
        }
        #endregion
        #region ProjectID
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Project", Visible = false, Enabled = false)]
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
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Task", Visible = false, Enabled = false)]
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

        #region Methods

        public static implicit operator INCostSubItemXRef(AMDisassembleBatchSplit item)
        {
            INCostSubItemXRef ret = new INCostSubItemXRef();
            ret.SubItemID = item.SubItemID;
            ret.CostSubItemID = item.CostSubItemID;

            return ret;
        }

        public static implicit operator INLotSerialStatus(AMDisassembleBatchSplit item)
        {
            INLotSerialStatus ret = new INLotSerialStatus();
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.LocationID = item.LocationID;
            ret.SubItemID = item.SubItemID;
            ret.LotSerialNbr = item.LotSerialNbr;

            return ret;
        }

        public static implicit operator INCostSite(AMDisassembleBatchSplit item)
        {
            INCostSite ret = new INCostSite();
            ret.CostSiteID = item.CostSiteID;

            return ret;
        }

        public static implicit operator INCostStatus(AMDisassembleBatchSplit item)
        {
            INCostStatus ret = new INCostStatus();
            ret.InventoryID = item.InventoryID;
            ret.CostSubItemID = item.CostSubItemID;
            ret.CostSiteID = item.CostSiteID;
            ret.LayerType = INLayerType.Normal;

            return ret;
        }
        #endregion
    }
}
