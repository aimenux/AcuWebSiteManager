using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [PXCacheName(Messages.ProductionItemSplit)]
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMProdItemSplit : IBqlTable, ILSDetail, IProdOrder
    {
        internal string DebuggerDisplay => $"OrderType = {OrderType}, ProdOrdID = {ProdOrdID}, SplitLineNbr = {SplitLineNbr}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdItemSplit>.By<orderType, prodOrdID, splitLineNbr>
        {
            public static AMProdItemSplit Find(PXGraph graph, string orderType, string prodOrdID, int? splitLineNbr) 
                => FindBy(graph, orderType, prodOrdID, splitLineNbr);
            public static AMProdItemSplit FindDirty(PXGraph graph, string orderType, string prodOrdID, int? splitLineNbr)
                => PXSelect<AMProdItemSplit,
                        Where<orderType, Equal<Required<orderType>>,
                            And<prodOrdID, Equal<Required<prodOrdID>>,
                                And<splitLineNbr, Equal<Required<splitLineNbr>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, splitLineNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdItemSplit>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdItemSplit>.By<orderType, prodOrdID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMProdItemSplit>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMProdItemSplit>.By<siteID> { }
        }

        #endregion

        #region Selected

        //public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        //{
        //}
        //protected bool? _Selected = false;
        //[PXBool]
        //[PXDefault(false)]
        //[PXUIField(DisplayName = "Selected")]
        //public virtual bool? Selected
        //{
        //    get
        //    {
        //        return _Selected;
        //    }
        //    set
        //    {
        //        _Selected = value;
        //    }
        //}
        #endregion
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(INTranType.Issue)]
        [INTranType.List]
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
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.orderType))]
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
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdItem.prodOrdID))]
        [PXParent(typeof(Select<AMProdItem, 
            Where<AMProdItem.orderType, Equal<Current<AMProdItemSplit.orderType>>, 
                And<AMProdItem.prodOrdID, Equal<Current<AMProdItemSplit.prodOrdID>>>>>))]
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
        #region SplitLineNbr

        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(AMProdItem.splitLineCntr))]
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
        [PXDBDate]
        [PXDefault(typeof(AMProdItem.prodDate))]
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
        //Cannot use AnyInventoryAttribute as this causes LSSelect class to fail with Obj Ref Error
        [StockItem(Visible = false, Enabled = false)]
        [PXDefault(typeof(AMProdItem.inventoryID))]
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
        [SubItem(typeof(AMProdItemSplit.inventoryID))]
        [PXDefault(typeof(AMProdItem.inventoryID))]
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
        [Site]
        [PXDefault(typeof(AMProdItem.siteID))]
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
        [MfgLocationAvail(typeof(AMProdItemSplit.inventoryID), typeof(AMProdItemSplit.subItemID),
            typeof(AMProdItemSplit.siteID), false, true, null, typeof(AMProdItem), Enabled = false)]
        [PXDefault(typeof(AMProdItem.locationID))]
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
        [AMProdItemLotSerialNbr(typeof(AMProdItemSplit.inventoryID), typeof(AMProdItemSplit.subItemID), typeof(AMProdItemSplit.locationID), typeof(AMProdItem.lotSerialNbr), 
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
        #region LotSerClassID

        public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        protected String _LotSerClassID;
        [PXString]
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

        public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr>{ }

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
        [INExpireDate(typeof(AMProdItemSplit.inventoryID), Enabled = false, FieldClass = "LotSerial", PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXDBShort]
        [PXDefault(typeof(AMProdItem.invtMult))]
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
        [PXDBBool]
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
        [INUnit(typeof(AMProdItemSplit.inventoryID), Enabled = false)]
        [PXDefault(typeof(AMProdItem.uOM))]
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
        [PXDBQuantity(typeof(AMProdItemSplit.uOM), typeof(AMProdItemSplit.baseQty))]
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
        [PXDBDecimal(6)]
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
        [PXDBLong(IsImmutable = true)]
        [PXUIField(DisplayName = "Plan ID", Visible = false, Enabled = false)]
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
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

        protected bool? _IsStockItem;
        [PXDBBool]
        //Because production items are all stock items, there is no need for the formula (or the field really)
        [PXDefault(true)]
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
        #region StatusID
        public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

        protected String _StatusID;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProductionOrderStatus.Planned)]
        [PXUIField(DisplayName = "Status")]
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
        #region IsAllocated
        public abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }

        protected Boolean? _IsAllocated;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allocated")]
        public virtual Boolean? IsAllocated
        {
            get
            {
                return this._IsAllocated;
            }
            set
            {
                this._IsAllocated = value;
            }
        }
        #endregion
        
        #region Methods

        public static implicit operator INCostSubItemXRef(AMProdItemSplit item)
        {
            INCostSubItemXRef ret = new INCostSubItemXRef();
            ret.SubItemID = item.SubItemID;
            ret.CostSubItemID = item.CostSubItemID;

            return ret;
        }

        public static implicit operator INLotSerialStatus(AMProdItemSplit item)
        {
            INLotSerialStatus ret = new INLotSerialStatus();
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.LocationID = item.LocationID;
            ret.SubItemID = item.SubItemID;
            ret.LotSerialNbr = item.LotSerialNbr;

            return ret;
        }

        public static implicit operator INCostSite(AMProdItemSplit item)
        {
            INCostSite ret = new INCostSite();
            ret.CostSiteID = item.CostSiteID;

            return ret;
        }

        public static implicit operator INCostStatus(AMProdItemSplit item)
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