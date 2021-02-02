using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.Common.Discount;
using PX.Objects.Common.Discount.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.TX;
using System;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.SODET)]
    public class FSSODet : IBqlTable, IFSSODetBase, IDocLine, ILSPrimary, ISortOrder
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSSODet>.By<srvOrdType, refNbr, lineNbr>
        {
            public static FSSODet Find(PXGraph graph, string srvOrdType, string refNbr, int? lineNbr) => FindBy(graph, srvOrdType, refNbr, lineNbr);
        }

        public static class FK
        {
            public class ServiceOrder : FSServiceOrder.PK.ForeignKeyOf<FSSODet>.By<srvOrdType, refNbr> { }
        }
        #endregion

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

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODet.srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<FSSODet.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SOID
        public abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "SOID")]
        [PXDBLiteDefault(typeof(FSServiceOrder.sOID))]
        public virtual int? SOID { get; set; }
        #endregion
        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public virtual int? SODetID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSServiceOrder.lineCntr))]
        [PXCheckUnique(Where = typeof(Where<FSSODet.srvOrdType, Equal<Current<FSServiceOrder.srvOrdType>>,
                                        And<FSSODet.refNbr, Equal<Current<FSServiceOrder.refNbr>>>>),
                       UniqueKeyIsPartOfPrimaryKey = true, ClearOnDuplicate = false)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [Branch(typeof(FSServiceOrder.branchID))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region Operation
        public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }

        private String _Operation;

        [PXDBString(1, IsFixed = true, InputMask = ">a")]
        [PXUIField(DisplayName = "Operation", Visibility = PXUIVisibility.Dynamic)]
        [PXDefault(typeof(SOOperation.issue))]
        [SOOperation.List]
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
        #region Behavior
        public abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }

        private String _Behavior;

        [PXString(2, IsFixed = true, InputMask = ">aa")]
        [PXFormula(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public virtual String Behavior
        {
            get
            {
                return this._Behavior;
            }
            set
            {
                this._Behavior = value;
            }
        }
        #endregion
        #region ShipComplete
        public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }

        private String _ShipComplete;

        [PXDBString(1, IsFixed = true)]
        [PXDefault(SOShipComplete.CancelRemainder)]
        [SOShipComplete.List()]
        [PXUIField(DisplayName = "Shipping Rule")]
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
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        public String _TranType;

        [PXFormula(typeof(Selector<FSSODet.operation, SOOrderTypeOperation.iNDocType>))]
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
        #region InvtMult
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        private Int16? _InvtMult;

        [PXDBShort()]
        [PXFormula(typeof(Switch<
                                Case<Where<
                                        Current<FSSrvOrdType.behavior>, Equal<FSSrvOrdType.behavior.Quote>>,
                                    short0>,
                                shortMinus1>))]
        [PXUIField(DisplayName = "Inventory Multiplier")]
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
        #region Completed
        public abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        private Boolean? _Completed;

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = true)]
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
        #region BillCustomerID
        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        private Int32? _BillCustomerID;

        [PXDBInt()]
        [PXDefault(typeof(FSServiceOrder.billCustomerID), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Int32? BillCustomerID
        {
            get
            {
                return this._BillCustomerID;
            }
            set
            {
                this._BillCustomerID = value;
            }
        }
        #endregion

        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
        {
        }

        private string _LineType;

        [PXDBString(5, IsFixed = true)]
        [PXDefault(ID.LineType_AppSrvOrd.SERVICE)]
        [PXUIField(DisplayName = "Line Type")]
        [FSLineType.List]
        public virtual string LineType
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
        #region SOLineType
        public abstract class sOLineType : PX.Data.BQL.BqlString.Field<sOLineType> { }

        private string _SOLineType;

        [PXString(2, IsFixed = true)]
        [SOLineType.List()]
        [PXUIField(DisplayName = "SO Line Type", Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODet.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
        public virtual string SOLineType
        {
            get
            {
                return this._SOLineType;
            }
            set
            {
                this._SOLineType = value;
            }
        }

        #endregion
        #region IsBillable
        public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public virtual bool? IsBillable { get; set; }
        #endregion
        #region IsPrepaid
        public abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false, Visible = false, FieldClass = "DISTINV")]
        public virtual bool? IsPrepaid { get; set; }
        #endregion
        #region ManualPrice
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
        [PXUIVisible(typeof(Where<
                                Current<FSSrvOrdType.postTo>, NotEqual<ListField_PostTo.PM>,
                            Or<
                                Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo.PM>,
                                And<
                                    Current<FSSrvOrdType.billingType>, NotEqual<FSSrvOrdType.billingType.CostAsCost>>>>))]
        public virtual bool? ManualPrice { get; set; }
        #endregion
        #region IsFree
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Free")]
        public virtual bool? IsFree { get; set; }
        #endregion

        #region InventoryID
        // @Warning: HAS ISSUES. Review how to pass the header depending on the line type. Also review the
        // selector and if the Restrictors are OK. The diff tool does not throws any difference between the
        // Restrictors in FSSODetService and FSSODetPart
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<lineType>))]
        [InventoryIDByLineType(typeof(lineType), Filterable = true)]

        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<True>,
                            Or<Current<FSSrvOrdType.requireRoute>, Equal<False>>>>),
                TX.Error.NONROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_ROUTE_SRVORDTYPE)]
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>,
                            Or<FSxServiceClass.requireRoute, Equal<False>,
                            Or<Current<FSSrvOrdType.requireRoute>, Equal<True>>>>),
                TX.Error.ROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_NONROUTE_SRVORDTYPE)]
        public virtual int? InventoryID { get; set; }
        #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

        [PXBool]
        [PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODet.inventoryID, InventoryItem.stkItem>))]
        public virtual Boolean? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region IsKit
        public abstract class isKit : PX.Data.BQL.BqlBool.Field<isKit> { }

        [PXBool]
        [PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODet.inventoryID, InventoryItem.kitItem>))]
        public virtual Boolean? IsKit
        {
            get;
            set;
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem")]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                            Where<
                                InventoryItem.inventoryID, Equal<Current<FSSODet.inventoryID>>,
                                And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIEnabled(typeof(Where<lineType, Equal<FSLineType.Service>>))]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public virtual int? SubItemID { get; set; }
        #endregion  

        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        private string _UOM;

        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXUIEnabled(typeof(Where<lineType, Equal<FSLineType.Service>>))]
        [PXDefault]
        public virtual string UOM
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
        #region BillingRule
        public abstract class billingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.FLAT_RATE)]
        [PXUIField(DisplayName = "Billing Rule")]
        public virtual string BillingRule { get; set; }
        #endregion

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [FSSiteAvail(typeof(FSSODet.inventoryID), typeof(FSSODet.subItemID), DisplayName = "Warehouse")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? SiteID { get; set; }
        #endregion
        #region SiteLocationID
        public abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [SOLocationAvail(typeof(FSSODet.inventoryID), typeof(FSSODet.subItemID), typeof(FSSODet.siteID), typeof(FSSODet.tranType), typeof(FSSODet.invtMult))]
        public virtual int? SiteLocationID { get; set; }

        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        public virtual int? LocationID
        {
            get
            {
                return SiteLocationID;
            }
            set
            {
                SiteLocationID = value;
            }
        }
        #endregion
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        private String _LotSerialNbr;

        [SOLotSerialNbr(typeof(FSSODet.inventoryID), typeof(FSSODet.subItemID), typeof(FSSODet.siteLocationID), PersistingCheck = PXPersistingCheck.Nothing)]
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

        #region ContractRelated
        public abstract class contractRelated : PX.Data.BQL.BqlBool.Field<contractRelated> { }

        [PXDBBool]
        [PXFormula(typeof(Default<FSSODet.billingRule, FSSODet.SMequipmentID, FSSODet.estimatedQty, FSSODet.inventoryID>))]
        [PXUIField(DisplayName = "Service Contract Item", IsReadOnly = true, FieldClass = "FSCONTRACT")]
        public virtual bool? ContractRelated { get; set; }
        #endregion

        #region UnassignedQty
        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        private Decimal? _UnassignedQty;

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnassignedQty
        {
            get
            {
                return this._UnassignedQty;
            }
            set
            {
                this._UnassignedQty = value;
            }
        }
        #endregion

        #region EstimatedDuration
        public abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXUnboundFormula(typeof(Switch<
                                    Case<
                                        Where<
                                            lineType, Equal<FSLineType.Service>,
                                        And<
                                            status, NotEqual<status.Canceled>,
                                        And<
                                            isTravelItem, Equal<False>>>>,
                                        estimatedDuration>,
                                    SharedClasses.int_0>),
                            typeof(SumCalc<FSServiceOrder.estimatedDurationTotal>))]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        private decimal? _EstimatedQty;

        [PXDBQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseEstimatedQty))]
        [PXDefault(typeof(Switch<
                            Case<
                                Where<lineType, Equal<FSLineType.Comment>,
                                    Or<lineType, Equal<FSLineType.Instruction>>>,
                                SharedClasses.decimal_0>,
                            SharedClasses.decimal_1>))]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public virtual decimal? EstimatedQty
        {
            get
            {
                return this._EstimatedQty;
            }
            set
            {
                this._EstimatedQty = value;
            }
        }

        #endregion
        #region BaseEstimatedQty
        public abstract class baseEstimatedQty : PX.Data.BQL.BqlDecimal.Field<baseEstimatedQty> { }

        private Decimal? _BaseEstimatedQty;

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseEstimatedQty
        {
            get
            {
                return this._BaseEstimatedQty;
            }
            set
            {
                this._BaseEstimatedQty = value;
            }
        }
        #endregion

        #region BillableQty
        public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseBillableQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<FSSODet.isBillable, FSSODet.lineType>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSSODet.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSSODet.status, Equal<FSSODet.status.Canceled>>,
                                    SharedClasses.decimal_0,
                                Case<Where<
                                        FSSODet.contractRelated, Equal<True>>,
                                    FSSODet.extraUsageQty>>>,
                            FSSODet.estimatedQty>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSSODet.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSSODet.contractRelated, Equal<True>>,
                                    FSSODet.extraUsageQty>>,
                            FSSODet.orderQty>))]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
        public virtual decimal? BillableQty { get; set; }
        #endregion
        #region BaseBillableQty
        public abstract class baseBillableQty : PX.Data.BQL.BqlDecimal.Field<baseBillableQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseBillableQty { get; set; }
        #endregion

        #region OrderQty
        public abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        [PXQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseOrderQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Allocation Quantity")]
        public virtual Decimal? OrderQty
        {
            get
            {
                return Status == ID.Status_SODet.CANCELED ? BillableQty : EstimatedQty;
            }
            set
            {
                EstimatedQty = value;
            }
        }
        public virtual Decimal? Qty
        {
            get
            {
                return OrderQty;
            }
            set
            {
                OrderQty = value;
            }
        }
        #endregion
        #region BaseOrderQty
        public abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }

        [PXDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseOrderQty
        {
            get
            {
                return BaseEstimatedQty;
            }
            set
            {
                BaseEstimatedQty = value;
            }
        }
        public virtual Decimal? BaseQty
        {
            get
            {
                return BaseOrderQty;
            }
            set
            {
                BaseOrderQty = value;
            }
        }
        #endregion

        #region DeductQty
        public abstract class deductQty : PX.Data.BQL.BqlDecimal.Field<deductQty> { }

        private Decimal? _DeductQty;
        [PXQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseDeductQty))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? DeductQty
        {
            get
            {
                return _DeductQty;
            }
            set
            {
                _DeductQty = value;
            }
        }
        #endregion
        #region BaseDeductQty
        public abstract class baseDeductQty : PX.Data.BQL.BqlDecimal.Field<baseDeductQty> { }

        private Decimal? _BaseDeductQty;
        [PXDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual Decimal? BaseDeductQty
        {
            get
            {
                return _BaseDeductQty;
            }
            set
            {
                _BaseDeductQty = value;
            }
        }
        #endregion

        #region ShippedQty
        public abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }

        private Decimal? _ShippedQty;

        [PXDBQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseShippedQty), MinValue = 0)]
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

        private Decimal? _BaseShippedQty;

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

        #region OpenQty
        public abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }

        private Decimal? _OpenQty;

        [PXDBQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseOpenQty), MinValue = 0)]
        [PXFormula(typeof(Sub<orderQty, shippedQty>))]
        [PXFormula(typeof(Sub<billableQty, shippedQty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Open Qty.", Enabled = false)]
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

        private Decimal? _BaseOpenQty;

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Open Qty.")]
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
        #region ClosedQty
        public abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }

        private Decimal? _ClosedQty;

        // DBCalced needs FSSODet FSSODet as parameter
        [PXDBCalced(typeof(Sub<FSSODet.billableQty, FSSODet.openQty>), typeof(decimal))]
        [PXQuantity(typeof(FSSODet.uOM), typeof(FSSODet.baseClosedQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ClosedQty
        {
            get
            {
                return this._ClosedQty;
            }
            set
            {
                this._ClosedQty = value;
            }
        }
        #endregion
        #region BaseClosedQty
        public abstract class baseClosedQty : PX.Data.BQL.BqlDecimal.Field<baseClosedQty> { }

        private Decimal? _BaseClosedQty;

        // DBCalced needs FSSODet FSSODet as parameter
        [PXDBCalced(typeof(Sub<FSSODet.baseBillableQty, FSSODet.baseOpenQty>), typeof(decimal))]
        [PXQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseClosedQty
        {
            get
            {
                return this._BaseClosedQty;
            }
            set
            {
                this._BaseClosedQty = value;
            }
        }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        [PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXDefault(typeof(Switch<
                                Case<Where<
                                    lineType, Equal<FSLineType.Comment>,
                                    Or<lineType, Equal<FSLineType.Instruction>>>,
                                    Null>,
                                    Current<FSServiceOrder.dfltProjectTaskID>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSSODet.projectID>>>))]
        [PXForeignReference(typeof(Field<projectTaskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? ProjectTaskID { get; set; }
        public virtual int? TaskID { get { return ProjectTaskID; } set { ProjectTaskID = value; } }
        #endregion
        #region HasMixedProjectTasks
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        private bool? _HasMixedProjectTasks;

        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public virtual bool? HasMixedProjectTasks
        {
            get
            {
                return _HasMixedProjectTasks;
            }
            set
            {
                _HasMixedProjectTasks = value;
            }
        }
        #endregion

        #region SourceLineID
        public abstract class sourceLineID : PX.Data.BQL.BqlInt.Field<sourceLineID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Source Line ID", Enabled = false)]
        public virtual int? SourceLineID { get; set; }
        #endregion
        #region SourceNoteID
        public abstract class sourceNoteID : PX.Data.BQL.BqlGuid.Field<sourceNoteID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Source Note ID", Enabled = false)]
        public virtual Guid? SourceNoteID { get; set; }
        #endregion
        #region SourceLineNbr
        public abstract class sourceLineNbr : PX.Data.BQL.BqlInt.Field<sourceLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Source Line Nbr.", Enabled = false)]
        public virtual int? SourceLineNbr { get; set; }
        #endregion
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        private DateTime? _TranDate;

        [PXDBDate]
        [PXDBDefault(typeof(FSServiceOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Transaction Date")]
        public virtual DateTime? TranDate
        {
            get
            {
                return _TranDate;
            }
            set
            {
                _TranDate = value;
            }
        }
        #endregion
        #region PlanType
        public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }

        private String _PlanType;

        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
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
        #region RequireShipping
        public abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }

        private bool? _RequireShipping;

        [PXBool()]
        [PXFormula(typeof(Current<SOOrderType.requireShipping>))]
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

        private bool? _RequireAllocation;

        [PXBool()]
        [PXFormula(typeof(Current<SOOrderType.requireAllocation>))]
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

        private bool? _RequireLocation;

        [PXBool()]
        [PXFormula(typeof(Current<SOOrderType.requireLocation>))]
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
        #region LineQtyAvail
        public abstract class lineQtyAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyAvail> { }
        [PXDecimal(6)]
        public virtual decimal? LineQtyAvail
        {
            get;
            set;
        }
        #endregion
        #region LineQtyHardAvail
        public abstract class lineQtyHardAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyHardAvail> { }

        private decimal? _LineQtyHardAvail;
        [PXDecimal(6)]
        public virtual decimal? LineQtyHardAvail
        {
            get
            {
                return _LineQtyHardAvail;
            }
            set
            {
                _LineQtyHardAvail = value;
            }
        }
        #endregion
        #region OrderDate
        public virtual DateTime? OrderDate
        {
            get { return this.TranDate; }
            set { TranDate = value; }
        }
        #endregion
        #region ShipDate
        public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }

        private DateTime? _ShipDate;

        [PXDBDate()]
        [PXDefault(typeof(FSServiceOrder.orderDate))]
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
        #region TranDesc
        public abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description")]
        public virtual string TranDesc { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion

        #region ApptQty
        public abstract class apptQty : PX.Data.BQL.BqlDecimal.Field<apptQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Appointment Quantity", Enabled = false)]
        public virtual decimal? ApptQty { get; set; }
        #endregion

        #region Status
        public abstract class status : ListField_Status_SODet
        {
        }

        [PXDBString(2, IsFixed = true)]
        [status.ListAtrribute]
        [PXUIField(DisplayName = "Line Status", Visibility = PXUIVisibility.SelectorVisible)]
        [PXFormula(typeof(Default<lineType, apptQty, estimatedQty, estimatedDuration>))]
        [PXFormula(typeof(Default<apptEstimatedDuration>))]
        public virtual string Status { get; set; }
        #endregion
        #region ScheduleDetID
        public abstract class scheduleDetID : PX.Data.BQL.BqlInt.Field<scheduleDetID> { }

        [PXDBInt]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual int? ScheduleDetID { get; set; }
        #endregion

        #region EquipmentAction
        public abstract class equipmentAction : ListField_EquipmentAction
        {
        }

        [PXDBString(2, IsFixed = true)]
        [equipmentAction.ListAtrribute]
        [PXDefault(ID.Equipment_Action.NONE)]
        [PXUIField(DisplayName = "Equipment Action", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual string EquipmentAction { get; set; }
        #endregion

        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [FSSelectorMaintenanceEquipment(typeof(FSServiceOrder.srvOrdType),
                                        typeof(FSServiceOrder.billCustomerID),
                                        typeof(FSServiceOrder.customerID),
                                        typeof(FSServiceOrder.locationID),
                                        typeof(FSServiceOrder.branchID),
                                        typeof(FSServiceOrder.branchLocationID))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                        TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public virtual int? SMEquipmentID { get; set; }
        #endregion

        #region NewTargetEquipmentLineNbr
        public abstract class newTargetEquipmentLineNbr : PX.Data.BQL.BqlString.Field<newTargetEquipmentLineNbr> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Model Equipment Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorNewTargetEquipmentServiceOrder]
        public virtual string NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [FSSelectorComponentIDServiceOrder(typeof(FSSODet), typeof(FSSODet))]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region ComponentLineRef
        public abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [FSSelectorEquipmentLineRefServiceOrderAppointment(
                typeof(inventoryID),
                typeof(SMequipmentID),
                typeof(componentID),
                typeof(equipmentAction))]
        public virtual int? EquipmentLineRef { get; set; }
        #endregion

        #region PostID
        public abstract class postID : PX.Data.BQL.BqlInt.Field<postID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Post ID")]
        public virtual int? PostID { get; set; }
        #endregion
        #region AcctID
        public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        [PXFormula(typeof(Default<inventoryID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false, AvoidControlAccounts = true)]
        public virtual int? AcctID { get; set; }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [PXFormula(typeof(Default<FSSODet.acctID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(FSSODet.acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
        public virtual int? SubID { get; set; }
        #endregion
        #region Scheduled
        public abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Service Scheduled", Enabled = false, Visible = false)]
        public virtual bool? Scheduled { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region EnablePurchaseOrder
        public abstract class enablePO : PX.Data.BQL.BqlBool.Field<enablePO> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for PO", FieldClass = "DISTINV", Visible = false)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            enablePO, Equal<True>,
                                            And<poNbr, IsNull>>,
                                        SharedClasses.int_1>,
                                    SharedClasses.int_0>),
                            typeof(SumCalc<FSServiceOrder.pOLineCntr>))]
        public virtual bool? EnablePO { get; set; }

        public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        [PXBool]
        [PXUIField(DisplayName = "Mark for PO", FieldClass = "DISTINV", Visible = false)]
        public virtual bool? POCreate
        {
            get
            {
                return EnablePO;
            }
            set
            {
                EnablePO = value;
            }
        }
        #endregion
        #region POVendorID
        public abstract class poVendorID : PX.Data.BQL.BqlInt.Field<poVendorID> { }

        [VendorNonEmployeeActive(DisplayName = "Vendor ID", DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, FieldClass = "DISTINV")]
        [PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
            Where<INItemSiteSettings.inventoryID, Equal<Current<FSSODet.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<FSSODet.siteID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<enablePO>))]
        public virtual int? POVendorID { get; set; }

        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        [PXInt]
        [PXUIField(DisplayName = "Vendor ID", FieldClass = "DISTINV")]
        public virtual int? VendorID
        {
            get
            {
                return POVendorID;
            }
            set
            {
                POVendorID = value;
            }
        }
        #endregion
        #region POVendorLocationID
        public abstract class poVendorLocationID : PX.Data.BQL.BqlInt.Field<poVendorLocationID> { }

        [PXFormula(typeof(Default<FSSODet.poVendorID>))]
        [PXDefault(typeof(Coalesce<
            Search<INItemSiteSettings.preferredVendorLocationID,
            Where<INItemSiteSettings.inventoryID, Equal<Current<FSSODet.inventoryID>>,
                    And<INItemSiteSettings.preferredVendorID, Equal<Current<FSSODet.poVendorID>>>>>,
            Search2<Vendor.defLocationID,
                InnerJoin<CRLocation,
                    On<CRLocation.locationID, Equal<Vendor.defLocationID>,
                    And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
                Where<Vendor.bAccountID, Equal<Current<FSSODet.poVendorID>>,
                    And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSSODet.poVendorID>>,
                            And<MatchWithBranch<Location.vBranchID>>>),
                DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location ID", FieldClass = "DISTINV")]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        public virtual int? POVendorLocationID { get; set; }
        #endregion

        #region POType
        public abstract class poType : PX.Data.BQL.BqlString.Field<poType> { }

        [PXDBString(2)]
        [PXUIField(DisplayName = "Order Type", FieldClass = "DISTINV")]
        public virtual String POType { get; set; }
        #endregion
        #region PONbr
        public abstract class poNbr : PX.Data.BQL.BqlString.Field<poNbr> { }

        [PXDBString]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false, FieldClass = "DISTINV")]
        [PO.PO.RefNbr(typeof(
            Search2<POOrder.orderNbr,
            LeftJoinSingleTable<Vendor,
                On<POOrder.vendorID, Equal<Vendor.bAccountID>,
                And<Match<Vendor, Current<AccessInfo.userName>>>>>,
            Where<
                POOrder.orderType, Equal<POOrderType.regularOrder>,
                And<Vendor.bAccountID, IsNotNull>>,
            OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
        public virtual string PONbr { get; set; }
        #endregion
        #region POSource
        public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }

        private string _POSource;

        [PXDBString()]
        [PXDefault(INReplenishmentSource.PurchaseToOrder, PersistingCheck = PXPersistingCheck.Nothing)]
        [INReplenishmentSource.SOList]
        [PXUIField(DisplayName = "PO Source", Enabled = false, FieldClass = "DISTINV")]
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
        #region POStatus
        public abstract class poStatus : PX.Data.BQL.BqlString.Field<poStatus> { }

        [PXDBString]
        [POOrderStatus.List]
        [PXUIField(DisplayName = "PO Status", Enabled = false, FieldClass = "DISTINV", Visible = false)]
        public virtual string POStatus { get; set; }
        #endregion
        #region POSiteID
        public abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }

        private Int32? _POSiteID;

        [Site(DisplayName = "Purchase Warehouse", FieldClass = "DISTINV")]
        [PXForeignReference(typeof(Field<pOSiteID>.IsRelatedTo<INSite.siteID>))]
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
        #region POLineNbr
        public abstract class poLineNbr : PX.Data.BQL.BqlInt.Field<poLineNbr> { }
        [PXDBInt]
        [PXUIField(DisplayName = "PO Line Nbr.", FieldClass = "DISTINV")]
        public virtual Int32? POLineNbr { get; set; }
        #endregion
        #region POCompleted
        public abstract class poCompleted : PX.Data.BQL.BqlBool.Field<poCompleted> { }

        [PXDBBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "PO Completed", Enabled = false, Visible = false, FieldClass = "DISTINV")]
        public virtual bool? POCompleted { get; set; }
        #endregion

        #region ManualCost
        public abstract class manualCost : PX.Data.BQL.BqlBool.Field<manualCost> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Cost", Visible = false)]
        public virtual bool? ManualCost { get; set; }
        #endregion

        #region UnitPrice
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Unit Price", Enabled = false)]
        public virtual Decimal? UnitPrice { get; set; }
        #endregion
        #region CuryUnitPrice
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitPrice))]
        [PXUIField(DisplayName = "Unit Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryUnitPrice { get; set; }
        #endregion

        #region EstimatedTranAmt
        public abstract class estimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<estimatedTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Amount", Enabled = false)]
        public virtual Decimal? EstimatedTranAmt { get; set; }
        #endregion
        #region CuryEstimatedTranAmt
        public abstract class curyEstimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEstimatedTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<FSLineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<status, Equal<FSSODet.status.Canceled>>,
                                    SharedClasses.decimal_0>>,
                                Mult<curyUnitPrice, estimatedQty>>),
                        typeof(SumCalc<FSServiceOrder.curyEstimatedOrderTotal>))]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
        public virtual Decimal? CuryEstimatedTranAmt { get; set; }
        #endregion

        #region CuryBillableExtPrice
        public abstract class curyBillableExtPrice : PX.Data.BQL.BqlDecimal.Field<curyBillableExtPrice> { }

        public Decimal? _CuryBillableExtPrice;

        [PXDBCurrency(typeof(FSSODet.curyInfoID), typeof(FSSODet.billableExtPrice))]
        [PXUIField(DisplayName = "Ext. Price")]
        [PXUIVisible(typeof(Where<
                                Current<FSSrvOrdType.postTo>, NotEqual<ListField_PostTo.PM>,
                            Or<
                                Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo.PM>,
                                And<
                                    Current<FSSrvOrdType.billingType>, NotEqual<FSSrvOrdType.billingType.CostAsCost>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        isBillable, Equal<False>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        contractRelated, Equal<False>,
                                        And<lineType, Equal<FSLineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        contractRelated, Equal<True>,
                                        And<isBillable, Equal<True>>>,
                                    Mult<curyExtraUsageUnitPrice, billableQty>>>>,
                                Mult<curyUnitPrice, billableQty>>))]
        public virtual Decimal? CuryBillableExtPrice
        {
            get
            {
                return _CuryBillableExtPrice;
            }
            set
            {
                _CuryBillableExtPrice = value;
            }
        }
        #endregion
        #region CuryExtPrice
        public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }

        [PXDecimal]
        [PXUIField(DisplayName = "Ext. Price")]
        public virtual Decimal? CuryExtPrice
        {
            get
            {
                return CuryBillableExtPrice;
            }
        }
        #endregion
        #region BillableExtPrice
        public abstract class billableExtPrice : PX.Data.BQL.BqlDecimal.Field<billableExtPrice> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BillableExtPrice { get; set; }
        #endregion

        #region Discount Fields
        #region ManualDisc
        public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }

        [PXDBBool]
        [ManualDiscountMode(typeof(FSSODet.curyDiscAmt), typeof(FSSODet.discPct), DiscountFeatureType.CustomerDiscount)]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? ManualDisc { get; set; }
        #endregion
        #region DiscPct
        public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
        public Decimal? _DiscPct;

        [PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Discount Percent")]
        [PXUIVisible(typeof(Where<
                                Current<FSSrvOrdType.postTo>, NotEqual<ListField_PostTo.PM>,
                            Or<
                                Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo.PM>,
                                And<
                                    Current<FSSrvOrdType.billingType>, NotEqual<FSSrvOrdType.billingType.CostAsCost>>>>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<Case<Where<isBillable, Equal<False>>, decimal0>, discPct>))]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual Decimal? DiscPct
        {
            get
            {
                return _DiscPct;
            }
            set
            {
                _DiscPct = value;
            }
        }
        #endregion
        #region CuryDiscAmt
        public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }

        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(CommonSetup.decPlPrcCst), typeof(FSSODet.curyInfoID), typeof(FSSODet.discAmt))]
        [PXFormula(typeof(Div<Mult<FSSODet.curyBillableExtPrice, FSSODet.discPct>, decimal100>))]
        [PXUIField(DisplayName = "Discount Amount")]
        [PXUIVisible(typeof(Where<
                                Current<FSSrvOrdType.postTo>, NotEqual<ListField_PostTo.PM>,
                            Or<
                                Current<FSSrvOrdType.postTo>, Equal<ListField_PostTo.PM>,
                                And<
                                    Current<FSSrvOrdType.billingType>, NotEqual<FSSrvOrdType.billingType.CostAsCost>>>>))]
        public virtual Decimal? CuryDiscAmt { get; set; }
        #endregion
        #region DiscAmt
        public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscAmt { get; set; }
        #endregion
        #region DiscountID
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXSelector(typeof(Search<ARDiscount.discountID, Where<ARDiscount.type, Equal<DiscountType.LineDiscount>>>))]
        [PXUIField(DisplayName = "Discount Code", Visible = true, Enabled = true)]
        public virtual String DiscountID { get; set; }
        #endregion
        #region DiscountSequenceID
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Discount Sequence", Visible = false, Enabled = false)]
        public virtual String DiscountSequenceID { get; set; }
        #endregion
        #endregion

        #region CuryBillableTranAmt
        public abstract class curyBillableTranAmt : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmt> { }

        private Decimal? _CuryBillableTranAmt;

        [PXDBCurrency(typeof(curyInfoID), typeof(billableTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount", Enabled = false)]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        isPrepaid, Equal<True>,
                                        Or<contractRelated, Equal<True>>>,
                                    curyBillableExtPrice>,
                                Sub<curyBillableExtPrice, curyDiscAmt>>),
                            typeof(SumCalc<FSServiceOrder.curyBillableOrderTotal>))]
        public virtual Decimal? CuryBillableTranAmt
        {
            get
            {
                return _CuryBillableTranAmt;
            }
            set
            {
                _CuryBillableTranAmt = value;
            }
        }
        #endregion

        #region CuryLineAmt
        public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount", Enabled = false)]

        public virtual Decimal? CuryLineAmt
        {
            get { return CuryBillableTranAmt; }
        }
        #endregion
        #region BillableTranAmt
        public abstract class billableTranAmt : PX.Data.BQL.BqlDecimal.Field<billableTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public virtual Decimal? BillableTranAmt { get; set; }
        #endregion

        #region Appointment Summary fields
        #region ApptCntr
        public abstract class apptCntr : PX.Data.BQL.BqlInt.Field<apptCntr> { }

        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Appointment Count", Enabled = false)]
        public virtual int? ApptCntr { get; set; }
        #endregion
        #region ApptDuration
        public abstract class apptDuration : PX.Data.BQL.BqlInt.Field<apptDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Appointment Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUnboundFormula(typeof(Switch<
                                        Case<
                                             Where<
                                                 lineType, Equal<FSLineType.Service>,
                                             And<
                                                 isTravelItem, Equal<False>>>,
                                             apptDuration>,
                                        SharedClasses.int_0>),
                          typeof(SumCalc<FSServiceOrder.apptDurationTotal>))]
        public virtual int? ApptDuration { get; set; }
        #endregion
        #region ApptEstimatedDuration
        public abstract class apptEstimatedDuration : PX.Data.BQL.BqlInt.Field<apptEstimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Appointment Estimated Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]

        public virtual int? ApptEstimatedDuration { get; set; }
        #endregion
        #region ApptTranAmt
        public abstract class apptTranAmt : PX.Data.BQL.BqlDecimal.Field<apptTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Appointment Amount", Enabled = false)]
        [PXUnboundFormula(typeof(apptTranAmt), typeof(SumCalc<FSServiceOrder.apptOrderTotal>))]
        public virtual Decimal? ApptTranAmt { get; set; }
        #endregion
        #region CuryApptTranAmt
        public abstract class curyApptTranAmt : PX.Data.BQL.BqlDecimal.Field<curyApptTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(apptTranAmt))]
        [PXUIField(DisplayName = "Appointment Amount", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUnboundFormula(typeof(curyApptTranAmt), typeof(SumCalc<FSServiceOrder.curyApptOrderTotal>))]
        public virtual Decimal? CuryApptTranAmt { get; set; }
        #endregion
        #endregion

        #region StaffID
        public abstract class staffID : PX.Data.BQL.BqlInt.Field<staffID> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member ID")]
        public virtual int? StaffID { get; set; }
        #endregion

        #region Warranty
        public abstract class warranty : PX.Data.BQL.BqlBool.Field<warranty> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Warranty", Enabled = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual bool? Warranty { get; set; }
        #endregion
        #region SONewTargetEquipmentLineNbr
        public abstract class sONewTargetEquipmentLineNbr : PX.Data.BQL.BqlInt.Field<sONewTargetEquipmentLineNbr> { }

        [PXInt]
        [PXUIField(DisplayName = "SO NewTargetEquipmentLineNbr", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual int? SONewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region Comment
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Equipment Action Comment", FieldClass = FSSetup.EquipmentManagementFieldClass, Visible = false)]
        public virtual string Comment { get; set; }
        #endregion
        #region EquipmentItemClass
        public abstract class equipmentItemClass : PX.Data.BQL.BqlString.Field<equipmentItemClass>
        {
        }

        [PXString(2, IsFixed = true)]
        public virtual string EquipmentItemClass { get; set; }
        #endregion

        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), typeof(acctID), typeof(projectTaskID))]
        [PXFormula(typeof(Default<inventoryID, isPrepaid>))]
        [PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
        public virtual int? CostCodeID { get; set; }
        #endregion
        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<Where2<
                                Where<lineType, Equal<ListField_LineType_ALL.Service>,
                                   Or<lineType, Equal<ListField_LineType_ALL.NonStockItem>,
                                   Or<lineType, Equal<ListField_LineType_ALL.Inventory_Item>>>>,
                                And<
                                    Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Sort Order", Visible = false, Enabled = false)]
        public virtual Int32? SortOrder { get; set; }
        #endregion

        #region Tax Fields
        #region TaxCategoryID
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<FSSODet.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual String TaxCategoryID { get; set; }
        #endregion
        #region GroupDiscountRate
        public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? GroupDiscountRate { get; set; }
        #endregion
        #region DocumentDiscountRate
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? DocumentDiscountRate { get; set; }
        #endregion
        #endregion

        #region Contract related fields

        #region CoveredQty 
        public abstract class coveredQty : PX.Data.BQL.BqlDecimal.Field<coveredQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<FSSODet.contractRelated>))]
        [PXUIField(DisplayName = "Covered Quantity", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual decimal? CoveredQty { get; set; }
        #endregion
        #region ExtraUsageQty  
        public abstract class extraUsageQty : PX.Data.BQL.BqlDecimal.Field<extraUsageQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        contractRelated, Equal<True>>,
                                    Sub<estimatedQty, coveredQty>>,
                            SharedClasses.decimal_0>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Overage Quantity", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual decimal? ExtraUsageQty { get; set; }
        #endregion
        #region ExtraUsageUnitPrice 
        public abstract class extraUsageUnitPrice : PX.Data.BQL.BqlDecimal.Field<extraUsageUnitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Overage Unit Price", Enabled = false, FieldClass = "FSCONTRACT")]
        public virtual Decimal? ExtraUsageUnitPrice { get; set; }
        #endregion
        #region CuryExtraUsageUnitPrice
        public abstract class curyExtraUsageUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyExtraUsageUnitPrice> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extraUsageUnitPrice))]
        [PXFormula(typeof(Default<FSSODet.contractRelated>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overage Unit Price", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual Decimal? CuryExtraUsageUnitPrice { get; set; }
        #endregion

        #endregion

        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        private DateTime? _ExpireDate;

        [INExpireDate(typeof(FSSODet.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region CuryUnitCost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "DISTINV")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<FSSODet.enablePO>))]
        public virtual Decimal? CuryUnitCost { get; set; }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost()]
        public virtual Decimal? UnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(DisplayName = "Ext. Cost")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ExtCost { get; set; }

        #endregion

        #region Mem_LastReferencedBy
        public abstract class mem_LastReferencedBy : PX.Data.BQL.BqlString.Field<mem_LastReferencedBy> { }

        [PXString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Last Reference", Enabled = false, Visible = false)]
        [PXSelector(typeof(FSAppointment.refNbr))]
        public virtual string Mem_LastReferencedBy { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion
        #region EstimatedDurationReport
        public abstract class estimatedDurationReport : PX.Data.BQL.BqlInt.Field<estimatedDurationReport> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<lineType,
                                    NotEqual<ListField_LineType_ALL.Inventory_Item>>,
                                    estimatedDuration>,
                                /*default case*/
                                SharedClasses.int_0>))]
        public virtual int? EstimatedDurationReport { get; set; }
        #endregion
        #region CostCodeDescr
        public abstract class costCodeDescr : PX.Data.BQL.BqlBool.Field<costCodeDescr> { }

        [PXString]
        [PXUIField(DisplayName = "Cost Code Description", FieldClass = CostCodeAttribute.COSTCODE)]
        public virtual string CostCodeDescr { get; set; }
        #endregion

        #region TabOrigin
        public abstract class tabOrigin : PX.Data.BQL.BqlString.Field<tabOrigin> { }
        [PXInt]
        public virtual int? TabOrigin
        {
            get
            {
                if (LineType == ID.LineType_AppSrvOrd.INVENTORY_ITEM)
                {
                    return (int)SharedFunctions.SOAPDetOriginTab.InventoryItems;
                }
                else
                {
                    return (int)SharedFunctions.SOAPDetOriginTab.Services;
                }
            }
        }
        #endregion

        #region SkipUnitPriceCalc
        public abstract class skipUnitPriceCalc : PX.Data.BQL.BqlBool.Field<skipUnitPriceCalc> { }
        [PXBool]
        public virtual bool? SkipUnitPriceCalc { get; set; }
        #endregion
        #region AlreadyCalculatedUnitPrice
        public abstract class alreadyCalculatedUnitPrice : PX.Data.BQL.BqlDecimal.Field<alreadyCalculatedUnitPrice> { }
        [PXPriceCost]
        public virtual decimal? AlreadyCalculatedUnitPrice { get; set; }
        #endregion

        #region IsTravelItem
        public abstract class isTravelItem : Data.BQL.BqlBool.Field<isTravelItem> { }
        [PXBool]
        [PXUIField(DisplayName = "Is a Travel Item", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXFormula(typeof(Selector<FSSODet.inventoryID, FSxService.isTravelItem>))]
        public virtual bool? IsTravelItem { get; set; }
        #endregion

        #region EnableStaffID
        public abstract class enableStaffID : PX.Data.BQL.BqlBool.Field<enableStaffID> { }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? EnableStaffID { get; set; }
        #endregion

        #region InventoryIDReport
        public abstract class inventoryIDReport : PX.Data.BQL.BqlInt.Field<inventoryIDReport> { }

        [PXInt]
        [PXSelector(typeof(Search<InventoryItem.inventoryID,
                           Where<
                                InventoryItem.itemStatus, NotEqual<InventoryItemStatus.inactive>,
                                And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.markedForDeletion>,
                                And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.noSales>,
                                And<InventoryItem.itemType, Equal<INItemTypes.serviceItem>,
                                And<Match<Current<AccessInfo.userName>>>>>>>>),
                           SubstituteKey = typeof(InventoryItem.inventoryCD),
                           DescriptionField = typeof(InventoryItem.descr))]
        public virtual int? InventoryIDReport { get; set; }
        #endregion

        #region Methods
        public int? GetPrimaryDACDuration()
        {
            return EstimatedDuration;
        }

        public decimal? GetPrimaryDACQty()
        {
            return EstimatedQty;
        }

        public decimal? GetPrimaryDACTranAmt()
        {
            return CuryEstimatedTranAmt;
        }

        public int? GetDuration(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return EstimatedDuration;
            }
            else if (fieldType == FieldType.BillableField)
            {
                return EstimatedDuration;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public int? GetApptDuration()
        {
            return ApptEstimatedDuration;
        }

        public decimal? GetQty(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return EstimatedQty;
            }
            else if (fieldType == FieldType.BillableField)
            {
                return BillableQty;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public decimal? GetApptQty()
        {
            return ApptQty;
        }

        public decimal? GetBaseQty(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return BaseEstimatedQty;
            }
            else if (fieldType == FieldType.BillableField)
            {
                return BaseBillableQty;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public decimal? GetTranAmt(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return CuryEstimatedTranAmt;
            }
            else if (fieldType == FieldType.BillableField)
            {
                return CuryBillableTranAmt;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SetDuration(FieldType fieldType, int? duration, PXCache cache, bool raiseEvents)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<estimatedDuration>(this, duration);
                }
                else
                {
                    EstimatedDuration = duration;
                }
            }
            else if (fieldType == FieldType.BillableField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<estimatedDuration>(this, duration);
                }
                else
                {
                    EstimatedDuration = duration;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SetQty(FieldType fieldType, decimal? qty, PXCache cache, bool raiseEvents)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<estimatedQty>(this, qty);
                }
                else
                {
                    EstimatedQty = qty;
                }
            }
            else if (fieldType == FieldType.BillableField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<billableQty>(this, qty);
                }
                else
                {
                    BillableQty = qty;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public virtual bool needToBePosted()
        {
            return (LineType == ID.LineType_ALL.SERVICE
                        || LineType == ID.LineType_ALL.NONSTOCKITEM
                        || LineType == ID.LineType_ALL.INVENTORY_ITEM)
                    && IsPrepaid == false
                    && Status != ID.Status_AppointmentDet.CANCELED;
        }

        public virtual bool waitingForParts()
        {
            return EnablePO == true
                    && POCompleted != true;
        }

        #region Methods
        public static implicit operator FSSODetSplit(FSSODet item)
        {
            FSSODetSplit ret = new FSSODetSplit();
            ret.SrvOrdType = item.SrvOrdType;
            ret.RefNbr = item.RefNbr;
            ret.LineNbr = item.LineNbr;
            ret.Operation = item.Operation;
            ret.SplitLineNbr = 1;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.SiteLocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ExpireDate = item.ExpireDate;
            ret.Qty = item.Qty;
            ret.UOM = item.UOM;
            ret.OrderDate = item.OrderDate;
            ret.BaseQty = item.BaseQty;
            ret.InvtMult = item.InvtMult;
            ret.PlanType = item.PlanType;
            //check for ordered qty not to get problems in LSSelect_Detail_RowInserting which will retain Released = true flag while merging LSDetail
            ret.Completed = (item.RequireShipping == true && item.OrderQty > 0m && item.OpenQty == 0m || item.Completed == true);
            ret.ShipDate = item.ShipDate;
            ret.RequireAllocation = item.RequireAllocation;
            ret.RequireLocation = item.RequireLocation;
            ret.RequireShipping = item.RequireShipping;

            return ret;
        }
        public static implicit operator FSSODet(FSSODetSplit item)
        {
            FSSODet ret = new FSSODet();
            ret.SrvOrdType = item.SrvOrdType;
            ret.RefNbr = item.RefNbr;
            ret.LineNbr = item.LineNbr;
            ret.Operation = item.Operation;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.SiteLocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.Qty = item.Qty;
            ret.OpenQty = item.Qty;
            ret.BaseOpenQty = item.BaseQty;
            ret.UOM = item.UOM;
            ret.OrderDate = item.OrderDate;
            ret.BaseQty = item.BaseQty;
            ret.InvtMult = item.InvtMult;
            ret.PlanType = item.PlanType;
            ret.ShipDate = item.ShipDate;
            ret.RequireAllocation = item.RequireAllocation;
            ret.RequireLocation = item.RequireLocation;
            ret.RequireShipping = item.RequireShipping;
            return ret;
        }
        #endregion
        #endregion

        #region IDocLine unbound properties
        public int? DocID
        {
            get
            {
                return this.SOID;
            }
        }

        public int? LineID
        {
            get
            {
                return this.SODetID;
            }
        }

        public int? PostAppointmentID
        {
            get
            {
                return null;
            }
        }

        public int? PostSODetID
        {
            get
            {
                return this.SODetID;
            }
        }

        public int? PostAppDetID
        {
            get
            {
                return null;
            }
        }

        public string BillingBy
        {
            get
            {
                return ID.Billing_By.SERVICE_ORDER;
            }
        }

        public string SourceTable
        {
            get
            {
                return ID.TablePostSource.FSSO_DET;
            }
        }

        public bool IsService
        {
            get
            {
                return LineType == ID.LineType_AppSrvOrd.SERVICE || LineType == ID.LineType_AppSrvOrd.NONSTOCKITEM;
            }
        }

        public bool IsInventoryItem
        {
            get
            {
                return LineType == ID.LineType_AppSrvOrd.INVENTORY_ITEM;
            }
        }

        public bool IsCommentInstruction
        {
            get
            {
                return LineType == ID.LineType_AppSrvOrd.COMMENT || LineType == ID.LineType_AppSrvOrd.INSTRUCTION;
            }
        }
        #endregion
    }
}
