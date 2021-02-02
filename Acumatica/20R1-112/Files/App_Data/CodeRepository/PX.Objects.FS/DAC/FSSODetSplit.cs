using PX.Data.ReferentialIntegrity.Attributes;
using System;
using System.Text;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PO;
using PX.Objects.SO;

namespace PX.Objects.FS
{
    [Serializable()]
    public class FSSODetSplit : PX.Data.IBqlTable, ILSDetail
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        protected String _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetSplit.srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Current<FSSODetSplit.refNbr>>>>>))]
        [PXParent(typeof(Select<FSSODet, Where<FSSODet.srvOrdType, Equal<Current<FSSODetSplit.srvOrdType>>, And<FSSODet.refNbr, Equal<Current<FSSODetSplit.refNbr>>, And<FSSODet.lineNbr, Equal<Current<FSSODetSplit.lineNbr>>>>>>))]
        public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(FSSODet.lineNbr))]
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
        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSServiceOrder.splitLineCntr))]
        [PXUIField(DisplayName = "Allocation ID", Visible = false, IsReadOnly = true)]
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
        #region ParentSplitLineNbr
        public abstract class parentSplitLineNbr : PX.Data.BQL.BqlInt.Field<parentSplitLineNbr> { }
        protected Int32? _ParentSplitLineNbr;
        [PXDBInt()]
        [PXUIField(DisplayName = "Parent Allocation ID", Visible = false, IsReadOnly = true)]
        public virtual Int32? ParentSplitLineNbr
        {
            get
            {
                return this._ParentSplitLineNbr;
            }
            set
            {
                this._ParentSplitLineNbr = value;
            }
        }
        #endregion
        #region Operation
        public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
        protected String _Operation;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(FSSODet.operation))]
        [PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public virtual String Operation
        {
            get
            {
                return this._Operation;
            }
            set
            {
                this._Operation = value;
            }
        }
        #endregion
        #region InvtMult
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
        protected Int16? _InvtMult;
        [PXDBShort()]
        [PXDefault(typeof(FSSODet.invtMult))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [Inventory(Enabled = false, Visible = true)]
        [PXDefault(typeof(FSSODet.inventoryID))]
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
        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
        protected String _LineType;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(Selector<FSSODetSplit.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
        public virtual String LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXDBBool()]
        [PXFormula(typeof(Selector<FSSODetSplit.inventoryID, InventoryItem.stkItem>))]
        public virtual bool? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region IsAllocated
        public abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }
        protected Boolean? _IsAllocated;
        [PXDBBool()]
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
        #region IsMergeable
        public abstract class isMergeable : PX.Data.BQL.BqlBool.Field<isMergeable> { }
        protected Boolean? _IsMergeable;
        [PXBool()]
        [PXFormula(typeof(True))]
        public virtual Boolean? IsMergeable
        {
            get
            {
                return this._IsMergeable;
            }
            set
            {
                this._IsMergeable = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [SiteAvail(typeof(FSSODetSplit.inventoryID), typeof(FSSODetSplit.subItemID), new Type[] { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.qtyAvail), typeof(INSiteStatus.active), typeof(INSite.descr) }, DisplayName = "Alloc. Warehouse")]
        [PXFormula(typeof(Switch<Case<Where<FSSODetSplit.isAllocated, Equal<False>>, Current<FSSODet.siteID>>, FSSODetSplit.siteID>))]
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
        [SOLocationAvail(typeof(FSSODetSplit.inventoryID), typeof(FSSODetSplit.subItemID), typeof(FSSODetSplit.siteID), typeof(FSSODetSplit.tranType), typeof(FSSODetSplit.invtMult))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region ToSiteID
        public abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }
        protected Int32? _ToSiteID;
        [IN.Site(DisplayName = "Orig. Warehouse")]
        [PXDefault(typeof(FSSODet.siteID))]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [IN.SubItem(typeof(FSSODetSplit.inventoryID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(FSSODetSplit.inventoryID), typeof(FSSODetSplit.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
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
        #region ShipDate
        public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
        protected DateTime? _ShipDate;
        [PXDBDate()]
        [PXDefault(typeof(FSServiceOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship On", Visibility = PXUIVisibility.SelectorVisible)] 
        public virtual DateTime? ShipDate
        {
            get
            {
                return this._ShipDate;
            }
            set
            {
                this._ShipDate = value;
            }
        }
        #endregion
        #region ShipComplete
        public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
        protected String _ShipComplete;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(FSSODet.shipComplete), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String ShipComplete
        {
            get
            {
                return this._ShipComplete;
            }
            set
            {
                this._ShipComplete = value;
            }
        }
        #endregion
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }
        protected Boolean? _Completed;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public virtual Boolean? Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
            }
        }
        #endregion
        #region ShipmentNbr
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        protected string _ShipmentNbr;
        [PXDBString(IsUnicode = true)]
        [PXUIFieldAttribute(DisplayName = "Shipment Nbr.", Enabled = false)]
        public virtual string ShipmentNbr
        {
            get
            {
                return this._ShipmentNbr;
            }
            set
            {
                this._ShipmentNbr = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        protected String _LotSerialNbr;
        [SOLotSerialNbr(typeof(FSSODetSplit.inventoryID), typeof(FSSODetSplit.subItemID), typeof(FSSODetSplit.locationID), typeof(FSSODet.lotSerialNbr), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
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
        [INExpireDate(typeof(FSSODetSplit.inventoryID))]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        [INUnit(typeof(FSSODetSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(typeof(FSSODet.uOM))]
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
        [PXDBQuantity(typeof(FSSODetSplit.uOM), typeof(FSSODetSplit.baseQty))]
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
        #region ShippedQty
        public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }
        protected Decimal? _ShippedQty;
        [PXDBQuantity(typeof(FSSODetSplit.uOM), typeof(FSSODetSplit.baseShippedQty))]
        //[PXFormula(null, typeof(SumCalc<FSSODet.shippedQty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. On Shipments", Enabled = false)]
        public virtual Decimal? ShippedQty
        {
            get
            {
                return this._ShippedQty;
            }
            set
            {
                this._ShippedQty = value;
            }
        }
        #endregion
        #region BaseShippedQty
        public abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }
        protected Decimal? _BaseShippedQty;
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseShippedQty
        {
            get
            {
                return this._BaseShippedQty;
            }
            set
            {
                this._BaseShippedQty = value;
            }
        }
        #endregion
        #region ReceivedQty
        public abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }
        protected Decimal? _ReceivedQty;
        [PXDBQuantity(typeof(FSSODetSplit.uOM), typeof(FSSODetSplit.baseReceivedQty), MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Received", Enabled = false)]
        public virtual Decimal? ReceivedQty
        {
            get
            {
                return this._ReceivedQty;
            }
            set
            {
                this._ReceivedQty = value;
            }
        }
        #endregion
        #region BaseReceivedQty
        public abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }
        protected Decimal? _BaseReceivedQty;
        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseReceivedQty
        {
            get
            {
                return this._BaseReceivedQty;
            }
            set
            {
                this._BaseReceivedQty = value;
            }
        }
        #endregion
        #region UnreceivedQty
        public abstract class unreceivedQty : PX.Data.BQL.BqlDecimal.Field<unreceivedQty> { }
        protected Decimal? _UnreceivedQty;
        [PXQuantity(typeof(FSSODetSplit.uOM), typeof(FSSODetSplit.baseUnreceivedQty), MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetSplit.qty, FSSODetSplit.receivedQty>))]
        public virtual Decimal? UnreceivedQty
        {
            get
            {
                return this._UnreceivedQty;
            }
            set
            {
                this._UnreceivedQty = value;
            }
        }
        #endregion
        #region BaseUnreceivedQty
        public abstract class baseUnreceivedQty : PX.Data.BQL.BqlDecimal.Field<baseUnreceivedQty> { }
        protected Decimal? _BaseUnreceivedQty;
        [PXDecimal(6, MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetSplit.baseQty, FSSODetSplit.baseReceivedQty>))]
        public virtual Decimal? BaseUnreceivedQty
        {
            get
            {
                return this._BaseUnreceivedQty;
            }
            set
            {
                this._BaseUnreceivedQty = value;
            }
        }
        #endregion
        #region OpenQty
        public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }
        protected Decimal? _OpenQty;
        [PXQuantity(typeof(FSSODetSplit.uOM), typeof(FSSODetSplit.baseOpenQty), MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetSplit.qty, FSSODetSplit.shippedQty>))]
        public virtual Decimal? OpenQty
        {
            get
            {
                return this._OpenQty;
            }
            set
            {
                this._OpenQty = value;
            }
        }
        #endregion
        #region BaseOpenQty
        public abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }
        protected Decimal? _BaseOpenQty;
        [PXDecimal(6, MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetSplit.baseQty, FSSODetSplit.baseShippedQty>))]
        public virtual Decimal? BaseOpenQty
        {
            get
            {
                return this._BaseOpenQty;
            }
            set
            {
                this._BaseOpenQty = value;
            }
        }
        #endregion
        #region OrderDate
        public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
        protected DateTime? _OrderDate;
        [PXDBDate()]
        [PXDBDefault(typeof(FSServiceOrder.orderDate))]
        public virtual DateTime? OrderDate
        {
            get
            {
                return this._OrderDate;
            }
            set
            {
                this._OrderDate = value;
            }
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        protected String _TranType;
        [PXFormula(typeof(Selector<FSSODetSplit.operation, SOOrderTypeOperation.iNDocType>))]
        [PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
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
        #region TranDate
        public virtual DateTime? TranDate
        {
            get { return this._OrderDate; }
        }
        #endregion
        #region PlanType
        public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }
        protected String _PlanType;
        
        [PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String PlanType
        {
            get
            {
                return this._PlanType;
            }
            set
            {
                this._PlanType = value;
            }
        }
        #endregion
        //+Edit with FS buckets+//
        #region AllocatedPlanType
        public abstract class allocatedPlanType : PX.Data.BQL.BqlString.Field<allocatedPlanType> { }
        protected String _AllocatedPlanType;

        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdAllocated, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdAllocated, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String AllocatedPlanType
        {
            get
            {
                return this._AllocatedPlanType;
            }
            set
            {
                this._AllocatedPlanType = value;
            }
        }
        #endregion
        #region BackOrderPlanType
        public abstract class backOrderPlanType : PX.Data.BQL.BqlString.Field<backOrderPlanType> { }
        protected String _BackOrderPlanType;
        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtySOBackOrdered, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtySOBackOrdered, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String BackOrderPlanType
        {
            get
            {
                return this._BackOrderPlanType;
            }
            set
            {
                this._BackOrderPlanType = value;
            }
        }
        #endregion
        #region OrigPlanType
        public abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
        [PXDBString(2, IsFixed = true)]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        public virtual String OrigPlanType
        {
            get;
            set;
        }
        #endregion
        //+++//
        #region RequireShipping
        public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }
        protected bool? _RequireShipping;
        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireShipping, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public virtual bool? RequireShipping
        {
            get
            {
                return this._RequireShipping;
            }
            set
            {
                this._RequireShipping = value;
            }
        }
        #endregion
        #region RequireAllocation
        public abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }
        protected bool? _RequireAllocation;
        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireAllocation, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public virtual bool? RequireAllocation
        {
            get
            {
                return this._RequireAllocation;
            }
            set
            {
                this._RequireAllocation = value;
            }
        }
        #endregion
        #region RequireLocation
        public abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }
        protected bool? _RequireLocation;
        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireLocation, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public virtual bool? RequireLocation
        {
            get
            {
                return this._RequireLocation;
            }
            set
            {
                this._RequireLocation = value;
            }
        }
        #endregion

        #region POCreate
        public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
        protected Boolean? _POCreate;
        [PXDBBool()]
        [PXDefault()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetSplit.isAllocated, Equal<False>, And<FSSODetSplit.pOReceiptNbr, IsNull>>, Current<FSSODet.pOCreate>>, False>))]
        [PXUIField(DisplayName = "Mark for PO", Visible = true, Enabled = false)]
        public virtual Boolean? POCreate
        {
            get
            {
                return this._POCreate;
            }
            set
            {
                this._POCreate = value ?? false;
            }
        }
        #endregion
        #region POCompleted
        public abstract class pOCompleted : PX.Data.BQL.BqlBool.Field<pOCompleted> { }
        protected Boolean? _POCompleted;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? POCompleted
        {
            get
            {
                return this._POCompleted;
            }
            set
            {
                this._POCompleted = value;
            }
        }
        #endregion
        #region POCancelled
        public abstract class pOCancelled : PX.Data.BQL.BqlBool.Field<pOCancelled> { }
        protected Boolean? _POCancelled;
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Boolean? POCancelled
        {
            get
            {
                return this._POCancelled;
            }
            set
            {
                this._POCancelled = value;
            }
        }
        #endregion
        #region POSource
        public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }
        protected string _POSource;
        [PXDBString()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetSplit.isAllocated, Equal<False>>, Current<FSSODet.pOSource>>, Null>))]
        public virtual string POSource
        {
            get
            {
                return this._POSource;
            }
            set
            {
                this._POSource = value;
            }
        }
        #endregion

        #region FixedSource
        public abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }
        protected String _FixedSource;
        [PXString(1, IsFixed = true)]
        [PXDBCalced(typeof(Switch<Case<Where<FSSODetSplit.pOCreate, Equal<True>>, INReplenishmentSource.purchased, Case<Where<FSSODetSplit.siteID, NotEqual<FSSODetSplit.toSiteID>>, INReplenishmentSource.transfer>>, INReplenishmentSource.none>), typeof(string))]
        public virtual String FixedSource
        {
            get
            {
                return this._FixedSource;
            }
            set
            {
                this._FixedSource = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [PXDBInt()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetSplit.isAllocated, Equal<False>>, Current<FSSODet.vendorID>>, Null>))]
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
        #region POSiteID
        public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }
        protected Int32? _POSiteID;
        [PXDBInt()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetSplit.isAllocated, Equal<False>>, Current<FSSODet.pOSiteID>>, Null>))]
        public virtual Int32? POSiteID
        {
            get
            {
                return this._POSiteID;
            }
            set
            {
                this._POSiteID = value;
            }
        }
        #endregion
        #region POType
        public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
        protected String _POType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Type", Enabled = false)]
        [POOrderType.RBDList]
        public virtual String POType
        {
            get
            {
                return this._POType;
            }
            set
            {
                this._POType = value;
            }
        }
        #endregion
        #region PONbr
        public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
        protected String _PONbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<FSSODetSplit.pOType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
        public virtual String PONbr
        {
            get
            {
                return this._PONbr;
            }
            set
            {
                this._PONbr = value;
            }
        }
        #endregion
        #region POLineNbr
        public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
        protected Int32? _POLineNbr;
        [PXDBInt()]
        [PXUIField(DisplayName = "PO Line Nbr.", Enabled = false)]
        public virtual Int32? POLineNbr
        {
            get
            {
                return this._POLineNbr;
            }
            set
            {
                this._POLineNbr = value;
            }
        }
        #endregion
        #region POReceiptType
        public abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }
        protected String _POReceiptType;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Receipt Type", Enabled = false)]
        public virtual String POReceiptType
        {
            get
            {
                return this._POReceiptType;
            }
            set
            {
                this._POReceiptType = value;
            }
        }
        #endregion
        #region POReceiptNbr
        public abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }
        protected String _POReceiptNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<FSSODetSplit.pOReceiptType>>>>), DescriptionField = typeof(POReceipt.invoiceNbr))]
        public virtual String POReceiptNbr
        {
            get
            {
                return this._POReceiptNbr;
            }
            set
            {
                this._POReceiptNbr = value;
            }
        }
        #endregion

        #region SOOrderType
        public abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }
        protected String _SOOrderType;
        [PXDBString(2, IsFixed = true)]
        public virtual String SOOrderType
        {
            get
            {
                return this._SOOrderType;
            }
            set
            {
                this._SOOrderType = value;
            }
        }
        #endregion
        #region SOOrderNbr
        public abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }
        protected String _SOOrderNbr;
        [PXDBString(15, IsUnicode = true)]
        public virtual String SOOrderNbr
        {
            get
            {
                return this._SOOrderNbr;
            }
            set
            {
                this._SOOrderNbr = value;
            }
        }
        #endregion
        #region SOLineNbr
        public abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }
        protected Int32? _SOLineNbr;
        [PXDBInt()]
        public virtual Int32? SOLineNbr
        {
            get
            {
                return this._SOLineNbr;
            }
            set
            {
                this._SOLineNbr = value;
            }
        }
        #endregion
        #region SOSplitLineNbr
        public abstract class sOSplitLineNbr : PX.Data.BQL.BqlInt.Field<sOSplitLineNbr> { }
        protected Int32? _SOSplitLineNbr;
        [PXDBInt()]
        public virtual Int32? SOSplitLineNbr
        {
            get
            {
                return this._SOSplitLineNbr;
            }
            set
            {
                this._SOSplitLineNbr = value;
            }
        }
        #endregion

        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote()]
        public virtual Guid? RefNoteID
        {
            get
            {
                return this._RefNoteID;
            }
            set
            {
                this._RefNoteID = value;
            }
        }
        public class PXRefNoteAttribute : PX.Objects.Common.PXRefNoteBaseAttribute
        {
            public PXRefNoteAttribute()
                : base()
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                base.CacheAttached(sender);

                PXButtonDelegate del = delegate (PXAdapter adapter)
                {
                    PXCache cache = adapter.View.Graph.Caches[typeof(SOLineSplit)];
                    if (cache.Current != null)
                    {
                        object val = cache.GetValueExt(cache.Current, _FieldName);

                        PXLinkState state = val as PXLinkState;
                        if (state != null)
                        {
                            helper.NavigateToRow(state.target.FullName, state.keys, PXRedirectHelper.WindowMode.NewWindow);
                        }
                        else
                        {
                            helper.NavigateToRow((Guid?)cache.GetValue(cache.Current, _FieldName), PXRedirectHelper.WindowMode.NewWindow);
                        }
                    }

                    return adapter.Get();
                };

                string ActionName = sender.GetItemType().Name + "$" + _FieldName + "$Link";
                sender.Graph.Actions[ActionName] = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(typeof(SOOrder)), new object[] { sender.Graph, ActionName, del, new PXEventSubscriberAttribute[] { new PXUIFieldAttribute { MapEnableRights = PXCacheRights.Select } } });
            }

            public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
            {
                SOLineSplit row = e.Row as SOLineSplit;

                if (row != null && !string.IsNullOrEmpty(row.PONbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(POOrder)], new object[] { row.POType, row.PONbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POOrder), new object[] { row.POType, row.PONbr });
                }
                else if (row != null && !string.IsNullOrEmpty(row.ShipmentNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(SOShipment)], new object[] { row.ShipmentNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(SOShipment), new object[] { row.ShipmentNbr });
                }
                else if (row != null && !string.IsNullOrEmpty(row.SOOrderNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(SOOrder)], new object[] { row.SOOrderType, row.SOOrderNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(SOOrder), new object[] { row.SOOrderType, row.SOOrderNbr });
                }
                else if (row != null && !string.IsNullOrEmpty(row.POReceiptNbr))
                {
                    e.ReturnValue = GetEntityRowID(sender.Graph.Caches[typeof(POReceipt)], new object[] { row.POReceiptType, row.POReceiptNbr });
                    e.ReturnState = PXLinkState.CreateInstance(e.ReturnState, typeof(POReceipt), new object[] { row.POReceiptType, row.POReceiptNbr });
                }
                else
                {
                    base.FieldSelecting(sender, e);
                }
            }
        }

        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong(IsImmutable = true)]
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
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        protected Int32? _ProjectID;
        [PXFormula(typeof(Selector<FSSODetSplit.locationID, INLocation.projectID>))]
        [PXInt]
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
        [PXFormula(typeof(Selector<FSSODetSplit.locationID, INLocation.taskID>))]
        [PXInt]
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

        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
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
        [PXDBCreatedByScreenID()]
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
        [PXDBCreatedDateTime()]
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
        [PXDBLastModifiedByID()]
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
        [PXDBLastModifiedByScreenID()]
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
        [PXDBLastModifiedDateTime()]
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
        [PXDBTimestamp()]
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
