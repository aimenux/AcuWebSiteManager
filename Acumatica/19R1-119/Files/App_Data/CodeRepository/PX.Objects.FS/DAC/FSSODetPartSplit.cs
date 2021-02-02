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

    [Serializable]
    [PXBreakInheritance]
    [PXProjection(typeof(Select2<FSSODetSplit,
                        InnerJoin<FSSODet, On<FSSODet.srvOrdType, Equal<FSSODetSplit.srvOrdType>,
                                    And<FSSODet.refNbr, Equal<FSSODetSplit.refNbr>,
                                    And<FSSODet.lineNbr, Equal<FSSODetSplit.lineNbr>>>>>,
                            Where<FSSODet.lineType, Equal<ListField_LineType_ALL.Inventory_Item>,
                            Or<FSSODet.lineType, Equal<ListField_LineType_ALL.Comment_Part>,
                            Or<FSSODet.lineType, Equal<ListField_LineType_ALL.Instruction_Part>>>>>), Persistent = true)]
    public class FSSODetPartSplit : FSSODetSplit
    {
        #region SrvOrdType
        public new abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSServiceOrder.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public override string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public new abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder, Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetPartSplit.srvOrdType>>, And<FSServiceOrder.refNbr, Equal<Current<FSSODetPartSplit.refNbr>>>>>))]
        [PXParent(typeof(Select<FSSODetPart, Where<FSSODetPart.srvOrdType, Equal<Current<FSSODetPartSplit.srvOrdType>>, And<FSSODetPart.refNbr, Equal<Current<FSSODetPartSplit.refNbr>>, And<FSSODetPart.lineNbr, Equal<Current<FSSODetPartSplit.lineNbr>>>>>>))]
        public override String RefNbr
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
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(FSSODetPart.lineNbr))]
        public override Int32? LineNbr
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
        public new abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSServiceOrder.splitLineCntr))]
        [PXUIField(DisplayName = "Allocation ID", Visible = false, IsReadOnly = true)]
        public override Int32? SplitLineNbr
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
        public new abstract class parentSplitLineNbr : PX.Data.BQL.BqlInt.Field<parentSplitLineNbr> { }

        [PXDBInt()]
        [PXUIField(DisplayName = "Parent Allocation ID", Visible = false, IsReadOnly = true)]
        public override Int32? ParentSplitLineNbr
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
        public new abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(FSSODetPart.operation))]
        [PXSelectorMarker(typeof(Search<SOOrderTypeOperation.operation, Where<SOOrderTypeOperation.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public override String Operation
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
        public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        [PXDBShort()]
        [PXDefault(typeof(FSSODetPart.invtMult))]
        public override Int16? InvtMult
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
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [Inventory(Enabled = false, Visible = true)]
        [PXDefault(typeof(FSSODetPart.inventoryID))]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public override Int32? InventoryID
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
        public new abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(Selector<FSSODetPartSplit.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
        public override String LineType
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
        public new abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXDBBool()]
        [PXFormula(typeof(Selector<FSSODetPartSplit.inventoryID, InventoryItem.stkItem>))]
        public override bool? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region IsAllocated
        public new abstract class isAllocated : PX.Data.BQL.BqlBool.Field<isAllocated> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Allocated")]
        public override Boolean? IsAllocated
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
        public new abstract class isMergeable : PX.Data.BQL.BqlBool.Field<isMergeable> { }

        [PXBool()]
        [PXFormula(typeof(True))]
        public override Boolean? IsMergeable
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
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [SiteAvail(typeof(FSSODetPartSplit.inventoryID), typeof(FSSODetPartSplit.subItemID), new Type[] { typeof(INSite.siteCD), typeof(INSiteStatus.qtyOnHand), typeof(INSiteStatus.qtyAvail), typeof(INSiteStatus.active), typeof(INSite.descr) }, DisplayName = "Alloc. Warehouse")]
        [PXFormula(typeof(Switch<Case<Where<FSSODetPartSplit.isAllocated, Equal<False>>, Current<FSSODetPart.siteID>>, FSSODetPartSplit.siteID>))]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public override Int32? SiteID
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
        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        [SOLocationAvail(typeof(FSSODetPartSplit.inventoryID), typeof(FSSODetPartSplit.subItemID), typeof(FSSODetPartSplit.siteID), typeof(FSSODetPartSplit.tranType), typeof(FSSODetPartSplit.invtMult), PersistingCheck = PXPersistingCheck.Nothing)]
        public override Int32? LocationID
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
        public new abstract class toSiteID : PX.Data.BQL.BqlInt.Field<toSiteID> { }

        [IN.Site(DisplayName = "Orig. Warehouse")]
        [PXDefault(typeof(FSSODetPart.siteID))]
        public override Int32? ToSiteID
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
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [IN.SubItem(typeof(FSSODetPartSplit.inventoryID))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(FSSODetPartSplit.inventoryID), typeof(FSSODetPartSplit.siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public override Int32? SubItemID
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
        public new abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }

        [PXDBDate()]
        [PXDefault(typeof(FSServiceOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ship On", Visibility = PXUIVisibility.SelectorVisible)]
        public override DateTime? ShipDate
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
        public new abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(typeof(FSSODetPart.shipComplete), PersistingCheck = PXPersistingCheck.Nothing)]
        public override String ShipComplete
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
        public new abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = false)]
        public override Boolean? Completed
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
        public new abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }

        [PXDBString(IsUnicode = true)]
        [PXUIFieldAttribute(DisplayName = "Shipment Nbr.", Enabled = false)]
        public override string ShipmentNbr
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
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        [SOLotSerialNbr(typeof(FSSODetPartSplit.inventoryID), typeof(FSSODetPartSplit.subItemID), typeof(FSSODetPartSplit.locationID), typeof(FSSODetPart.lotSerialNbr), PersistingCheck = PXPersistingCheck.Nothing, FieldClass = "LotSerial")]
        public override String LotSerialNbr
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
        public new abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        [PXString(10, IsUnicode = true)]
        public override String LotSerClassID
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
        public new abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }

        [PXString(30, IsUnicode = true)]
        public override String AssignedNbr
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
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        [INExpireDate(typeof(FSSODetPartSplit.inventoryID))]
        public override DateTime? ExpireDate
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
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [INUnit(typeof(FSSODetPartSplit.inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(typeof(FSSODetPart.uOM))]
        public override String UOM
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
        public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        [PXDBQuantity(typeof(FSSODetPartSplit.uOM), typeof(FSSODetPartSplit.baseQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity")]
        public override Decimal? Qty
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
        public new abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        [PXDBDecimal(6)]
        public override Decimal? BaseQty
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
        public new abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }

        [PXDBQuantity(typeof(FSSODetPartSplit.uOM), typeof(FSSODetPartSplit.baseShippedQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. On Shipments", Enabled = false)]
        public override Decimal? ShippedQty
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
        public new abstract class baseShippedQty : PX.Data.BQL.BqlDecimal.Field<baseShippedQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? BaseShippedQty
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
        public new abstract class receivedQty : PX.Data.BQL.BqlDecimal.Field<receivedQty> { }

        [PXDBQuantity(typeof(FSSODetPartSplit.uOM), typeof(FSSODetPartSplit.baseReceivedQty), MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Received", Enabled = false)]
        public override Decimal? ReceivedQty
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
        public new abstract class baseReceivedQty : PX.Data.BQL.BqlDecimal.Field<baseReceivedQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? BaseReceivedQty
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
        public new abstract class unreceivedQty : PX.Data.BQL.BqlDecimal.Field<unreceivedQty> { }

        [PXQuantity(typeof(FSSODetPartSplit.uOM), typeof(FSSODetPartSplit.baseUnreceivedQty), MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetPartSplit.qty, FSSODetPartSplit.receivedQty>))]
        public override Decimal? UnreceivedQty
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
        public new abstract class baseUnreceivedQty : PX.Data.BQL.BqlDecimal.Field<baseUnreceivedQty> { }

        [PXDecimal(6, MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetPartSplit.baseQty, FSSODetPartSplit.baseReceivedQty>))]
        public override Decimal? BaseUnreceivedQty
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
        public new abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }

        [PXQuantity(typeof(FSSODetPartSplit.uOM), typeof(FSSODetPartSplit.baseOpenQty), MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetPartSplit.qty, FSSODetPartSplit.shippedQty>))]
        public override Decimal? OpenQty
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
        public new abstract class baseOpenQty : PX.Data.BQL.BqlDecimal.Field<baseOpenQty> { }

        [PXDecimal(6, MinValue = 0)]
        [PXFormula(typeof(Sub<FSSODetPartSplit.baseQty, FSSODetPartSplit.baseShippedQty>))]
        public override Decimal? BaseOpenQty
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
        public new abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }

        [PXDBDate()]
        [PXDBDefault(typeof(FSServiceOrder.orderDate))]
        public override DateTime? OrderDate
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
        public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        [PXFormula(typeof(Selector<FSSODetPartSplit.operation, SOOrderTypeOperation.iNDocType>))]
        [PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
        public override String TranType
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
        public override DateTime? TranDate
        {
            get { return this._OrderDate; }
        }
        #endregion
        #region PlanType
        public new abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }


        [PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public override String PlanType
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
        #region AllocatedPlanType
        public new abstract class allocatedPlanType : PX.Data.BQL.BqlString.Field<allocatedPlanType> { }


        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdAllocated, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdAllocated, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public override String AllocatedPlanType
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
        public new abstract class backOrderPlanType : PX.Data.BQL.BqlString.Field<backOrderPlanType> { }

        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtySOBackOrdered, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtySOBackOrdered, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public override String BackOrderPlanType
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
        public new abstract class origPlanType : PX.Data.BQL.BqlString.Field<origPlanType> { }
        [PXDBString(2, IsFixed = true)]
        [PXSelector(typeof(Search<INPlanType.planType>), CacheGlobal = true)]
        public override String OrigPlanType
        {
            get;
            set;
        }
        #endregion
        #region RequireShipping
        public new abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }

        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireShipping, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public override bool? RequireShipping
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
        public new abstract class requireAllocation : PX.Data.BQL.BqlBool.Field<requireAllocation> { }

        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireAllocation, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public override bool? RequireAllocation
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
        public new abstract class requireLocation : PX.Data.BQL.BqlBool.Field<requireLocation> { }

        [PXBool()]
        [PXFormula(typeof(Search<SOOrderType.requireLocation, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public override bool? RequireLocation
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
        public new abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        [PXDBBool()]
        [PXDefault()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetPartSplit.isAllocated, Equal<False>, And<FSSODetPartSplit.pOReceiptNbr, IsNull>>, Current<FSSODetPart.pOCreate>>, False>))]
        [PXUIField(DisplayName = "Mark for PO", Visible = true, Enabled = false)]
        public override Boolean? POCreate
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
        public new abstract class pOCompleted : PX.Data.BQL.BqlBool.Field<pOCompleted> { }

        [PXDBBool()]
        [PXDefault(false)]
        public override Boolean? POCompleted
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
        public new abstract class pOCancelled : PX.Data.BQL.BqlBool.Field<pOCancelled> { }

        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public override Boolean? POCancelled
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
        public new abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }

        [PXDBString()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetPartSplit.isAllocated, Equal<False>>, Current<FSSODetPart.pOSource>>, Null>))]
        public override string POSource
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
        public new abstract class fixedSource : PX.Data.BQL.BqlString.Field<fixedSource> { }

        [PXString(1, IsFixed = true)]
        [PXDBCalced(typeof(Switch<Case<Where<FSSODetPartSplit.pOCreate, Equal<True>>, INReplenishmentSource.purchased, Case<Where<FSSODetPartSplit.siteID, NotEqual<FSSODetPartSplit.toSiteID>>, INReplenishmentSource.transfer>>, INReplenishmentSource.none>), typeof(string))]
        public override String FixedSource
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
        public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        [PXDBInt()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetPartSplit.isAllocated, Equal<False>>, Current<FSSODetPart.vendorID>>, Null>))]
        public override Int32? VendorID
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
        public new abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }

        [PXDBInt()]
        [PXFormula(typeof(Switch<Case<Where<FSSODetPartSplit.isAllocated, Equal<False>>, Current<FSSODetPart.pOSiteID>>, Null>))]
        public override Int32? POSiteID
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
        public new abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Type", Enabled = false)]
        [POOrderType.RBDList]
        public override String POType
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
        public new abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.orderType, Equal<Current<FSSODetPartSplit.pOType>>>>), DescriptionField = typeof(POOrder.orderDesc))]
        public override String PONbr
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
        public new abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }

        [PXDBInt()]
        [PXUIField(DisplayName = "PO Line Nbr.", Enabled = false)]
        public override Int32? POLineNbr
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
        public new abstract class pOReceiptType : PX.Data.BQL.BqlString.Field<pOReceiptType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "PO Receipt Type", Enabled = false)]
        public override String POReceiptType
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
        public new abstract class pOReceiptNbr : PX.Data.BQL.BqlString.Field<pOReceiptNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Receipt Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<POReceipt.receiptNbr, Where<POReceipt.receiptType, Equal<Current<FSSODetPartSplit.pOReceiptType>>>>), DescriptionField = typeof(POReceipt.invoiceNbr))]
        public override String POReceiptNbr
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
        public new abstract class sOOrderType : PX.Data.BQL.BqlString.Field<sOOrderType> { }

        [PXDBString(2, IsFixed = true)]
        public override String SOOrderType
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
        public new abstract class sOOrderNbr : PX.Data.BQL.BqlString.Field<sOOrderNbr> { }

        [PXDBString(15, IsUnicode = true)]
        public override String SOOrderNbr
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
        public new abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }

        [PXDBInt()]
        public override Int32? SOLineNbr
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
        public new abstract class sOSplitLineNbr : PX.Data.BQL.BqlInt.Field<sOSplitLineNbr> { }

        [PXDBInt()]
        public override Int32? SOSplitLineNbr
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
        public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote()]
        public override Guid? RefNoteID
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
        public new class PXRefNoteAttribute : PX.Objects.Common.PXRefNoteBaseAttribute
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
        public new abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

        [PXDBLong(IsImmutable = true)]
        public override Int64? PlanID
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
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXFormula(typeof(Selector<FSSODetPartSplit.locationID, INLocation.projectID>))]
        [PXInt]
        public override Int32? ProjectID
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
        public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        [PXFormula(typeof(Selector<FSSODetPartSplit.locationID, INLocation.taskID>))]
        [PXInt]
        public override Int32? TaskID
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
        public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID()]
        public override Guid? CreatedByID
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
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID()]
        public override String CreatedByScreenID
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
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime()]
        public override DateTime? CreatedDateTime
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
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID()]
        public override Guid? LastModifiedByID
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
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID()]
        public override String LastModifiedByScreenID
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
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime()]
        public override DateTime? LastModifiedDateTime
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
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp()]
        public override Byte[] tstamp
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
