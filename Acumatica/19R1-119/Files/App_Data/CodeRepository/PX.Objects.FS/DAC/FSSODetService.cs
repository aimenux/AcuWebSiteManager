using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
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
    [PXBreakInheritance]
    [PXCacheName(TX.TableName.SODET_SERVICE)]
    [PXProjection(typeof(Select<FSSODet,
                        Where<
                            FSSODet.lineType, Equal<ListField_LineType_ALL.Service>,
                            Or<FSSODet.lineType, Equal<ListField_LineType_ALL.Comment_Service>,
                            Or<FSSODet.lineType, Equal<ListField_LineType_ALL.Instruction_Service>,
                            Or<FSSODet.lineType, Equal<ListField_LineType_ALL.NonStockItem>>>>>>), Persistent = true)]
    public class FSSODetService : FSSODet
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

        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Service Order Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSServiceOrder.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSServiceOrder,
                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetService.srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<FSSODetService.refNbr>>>>>))]
        public override string RefNbr { get; set; }
        #endregion
        #region SOID
        public new abstract class sOID : PX.Data.BQL.BqlInt.Field<sOID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "SOID")]
        [PXDBLiteDefault(typeof(FSServiceOrder.sOID))]
        public override int? SOID { get; set; }
        #endregion

        #region SODetID
        public new abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBIdentity]
        [PXUIField(Enabled = false, Visibility = PXUIVisibility.Invisible)]
        public override int? SODetID { get; set; }
        #endregion
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSServiceOrder.lineCntr))]
        public override int? LineNbr { get; set; }
        #endregion
        #region LineRef
        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public override string LineRef { get; set; }
        #endregion

        #region BranchID
        public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [Branch(typeof(FSServiceOrder.branchID))]
        public override int? BranchID { get; set; }
        #endregion
        #region Operation
        public new abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }

        [PXDBString(1, IsFixed = true, InputMask = ">a")]
        [PXUIField(DisplayName = "Operation", Visibility = PXUIVisibility.Dynamic)]
        [PXDefault(typeof(SOOperation.issue))]
        [SOOperation.List]
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
        #region Behavior
        public new abstract class behavior : PX.Data.BQL.BqlString.Field<behavior> { }

        [PXString(2, IsFixed = true, InputMask = ">aa")]
        [PXFormula(typeof(Search<SOOrderType.behavior, Where<SOOrderType.orderType, Equal<Current<FSSrvOrdType.allocationOrderType>>>>))]
        public override String Behavior
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
        public new abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(SOShipComplete.CancelRemainder)]
        [SOShipComplete.List()]
        [PXUIField(DisplayName = "Shipping Rule")]
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
        #region TranType
        public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        [PXFormula(typeof(Selector<FSSODetService.operation, SOOrderTypeOperation.iNDocType>))]
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
        #region InvtMult
        public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        [PXDBShort()]
        [PXFormula(typeof(Switch<
                                Case<Where<
                                        Current<FSSrvOrdType.behavior>, Equal<FSSrvOrdType.behavior.Quote>>,
                                    short0>,
                                shortMinus1>))]
        [PXUIField(DisplayName = "Inventory Multiplier")]
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
        #region Completed
        public new abstract class completed : PX.Data.BQL.BqlBool.Field<completed> { }

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Completed", Enabled = true)]
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
        #region BillCustomerID
        public new abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt()]
        [PXDefault(typeof(FSServiceOrder.billCustomerID), PersistingCheck = PXPersistingCheck.Nothing)]
        public override Int32? BillCustomerID
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
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
        public override Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public new abstract class lineType : ListField_LineType_Service_ServiceTemplate
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXDefault(ID.LineType_ServiceTemplate.SERVICE)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        public override string LineType
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
        public new abstract class sOLineType : PX.Data.BQL.BqlString.Field<sOLineType> { }


        [PXString(2, IsFixed = true)]
        [SOLineType.List()]
        [PXUIField(DisplayName = "SO Line Type", Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODetService.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
        public override string SOLineType
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
        public new abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public override bool? IsBillable { get; set; }
        #endregion
        #region IsPrepaid
        public new abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false)]
        public override bool? IsPrepaid { get; set; }
        #endregion
        #region ManualPrice
        public new abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
        public override bool? ManualPrice { get; set; }
        #endregion
        #region IsFree
        public new abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Free")]
        public override bool? IsFree { get; set; }
        #endregion

        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

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
        public override int? InventoryID { get; set; }
        #endregion
        #region IsStockItem
        public new abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXBool]
        [PXUIField(DisplayName = "Is stock", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODetService.inventoryID, InventoryItem.stkItem>))]
        public override Boolean? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region IsKit
        public new abstract class isKit : PX.Data.BQL.BqlBool.Field<isKit> { }
        [PXBool]
        [PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSSODetService.inventoryID, InventoryItem.kitItem>))]
        public override Boolean? IsKit
        {
            get;
            set;
        }
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem", Visible = false)]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                            Where<
                                InventoryItem.inventoryID, Equal<Current<FSSODetService.inventoryID>>,
                                And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public override int? SubItemID { get; set; }
        #endregion  

        #region UOM
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false, Visible = false)]
        [PXDefault]
        public override string UOM
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
        public new abstract class billingRule : ListField_BillingRule
        {
        }

        [PXDBString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.FLAT_RATE)]
        [PXUIField(DisplayName = "Billing Rule")]
        public override string BillingRule { get; set; }
        #endregion

        #region SiteID
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [PXDefault(typeof(Search<FSBranchLocation.dfltSiteID,
                            Where<FSBranchLocation.branchLocationID, Equal<Current<FSServiceOrder.branchLocationID>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSiteAvailAttribute(typeof(FSSODetService.inventoryID), typeof(FSSODetService.subItemID), DisplayName = "Warehouse", Visible = false)]
        public override int? SiteID { get; set; }
        #endregion
        #region SiteLocationID
        public new abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [PXFormula(typeof(Default<FSSODetService.siteID>))]
        [Location(typeof(FSSODetService.siteID), DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
        public override int? SiteLocationID { get; set; }

        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        public override int? LocationID
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
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        [INLotSerialNbr(typeof(FSSODetService.inventoryID), typeof(FSSODetService.subItemID), typeof(FSSODetService.siteLocationID), PersistingCheck = PXPersistingCheck.Nothing)]
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

        #region ContractRelated
        public new abstract class contractRelated : PX.Data.BQL.BqlBool.Field<contractRelated> { }

        [PXDBBool]
        [PXFormula(typeof(Default<FSSODetService.billingRule, FSSODetService.SMequipmentID, FSSODetService.estimatedQty, FSSODetService.inventoryID>))]
        [PXUIField(DisplayName = "Contract Item", IsReadOnly = true, FieldClass = "FSCONTRACT")]
        public override bool? ContractRelated { get; set; }
        #endregion

        #region UnassignedQty
        public new abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? UnassignedQty
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
        public new abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            lineType, Equal<lineType.Service>,
                                            And<status, NotEqual<status.Canceled>>>,
                                        estimatedDuration>,
                                    SharedClasses.int_0>),
                            typeof(SumCalc<FSServiceOrder.estimatedDurationTotal>))]
        public override int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public new abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }


        [PXDBQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseEstimatedQty))]
        [PXDefault(typeof(Switch<
                            Case<
                                Where<
                                    status, Equal<status.Canceled>,
                                    Or<lineType, Equal<ListField_LineType_ALL.Comment_Service>,
                                    Or<lineType, Equal<ListField_LineType_ALL.Instruction_Service>>>>,
                                SharedClasses.decimal_0>,
                            SharedClasses.decimal_1>))]
        [PXFormula(typeof(Default<status>))]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public override decimal? EstimatedQty
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
        public new abstract class baseEstimatedQty : PX.Data.BQL.BqlDecimal.Field<baseEstimatedQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Qty.", Visible = false, Enabled = false)]
        public override Decimal? BaseEstimatedQty
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
        #region OrderQty
        public new abstract class orderQty : PX.Data.BQL.BqlDecimal.Field<orderQty> { }

        [PXQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseOrderQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Allocation Quantity")]
        public override Decimal? OrderQty
        {
            get
            {
                return EstimatedQty;
            }
            set
            {
                EstimatedQty = value;
            }
        }
        public override Decimal? Qty
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
        public new abstract class baseOrderQty : PX.Data.BQL.BqlDecimal.Field<baseOrderQty> { }

        [PXDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Order Qty.", Visible = false, Enabled = false)]
        public override Decimal? BaseOrderQty
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
        public override Decimal? BaseQty
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
        #region OpenQty
        public new abstract class openQty : PX.Data.BQL.BqlDecimal.Field<openQty> { }

        [PXDBQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseOpenQty), MinValue = 0)]
        [PXFormula(typeof(Sub<orderQty, shippedQty>))]
        [PXFormula(typeof(Sub<estimatedQty, shippedQty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Open Qty.", Enabled = false)]
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

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Open Qty.")]
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
        #region ClosedQty
        public new abstract class closedQty : PX.Data.BQL.BqlDecimal.Field<closedQty> { }

        // DBCalced needs FSSODet FSSODet as parameter
        [PXDBCalced(typeof(Sub<FSSODet.estimatedQty, FSSODet.openQty>), typeof(decimal))]
        [PXQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseClosedQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? ClosedQty
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
        public new abstract class baseClosedQty : PX.Data.BQL.BqlDecimal.Field<baseClosedQty> { }

        // DBCalced needs FSSODet FSSODet as parameter
        [PXDBCalced(typeof(Sub<FSSODet.baseEstimatedQty, FSSODet.baseOpenQty>), typeof(decimal))]
        [PXQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? BaseClosedQty
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

        #region ShippedQty
        public new abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }

        [PXDBQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseShippedQty), MinValue = 0)]
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

        #region BillableQty
        public new abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity(typeof(FSSODetService.uOM), typeof(FSSODetService.baseBillableQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<FSSODetService.isBillable, FSSODetService.lineType>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSSODetService.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSSODetService.status, Equal<FSSODet.status.Canceled>>,
                                    SharedClasses.decimal_0,
                                Case<Where<
                                        FSSODetService.contractRelated, Equal<True>>,
                                    FSSODetService.extraUsageQty>>>,
                            FSSODetService.estimatedQty>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSSODetService.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSSODetService.contractRelated, Equal<True>>,
                                    FSSODetService.extraUsageQty>>,
                            FSSODetService.orderQty>))]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
        public override decimal? BillableQty { get; set; }
        #endregion
        #region BaseBillableQty
        public new abstract class baseBillableQty : PX.Data.BQL.BqlDecimal.Field<baseBillableQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Qty.", Visible = false, Enabled = false)]
        public override Decimal? BaseBillableQty { get; set; }
        #endregion

        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public override int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.dfltProjectTaskID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task")]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSSODetService.projectID>>>))]
        public override int? ProjectTaskID { get; set; }
        public override int? TaskID { get { return ProjectTaskID; } set { ProjectTaskID = value; } }
        #endregion
        #region HasMixedProjectTasks
        public new abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public override bool? HasMixedProjectTasks
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
        public new abstract class sourceLineID : PX.Data.BQL.BqlInt.Field<sourceLineID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Source Line ID", Enabled = false)]
        public override int? SourceLineID { get; set; }
        #endregion
        #region SourceNoteID
        public new abstract class sourceNoteID : PX.Data.BQL.BqlGuid.Field<sourceNoteID> { }

        [PXDBGuid]
        [PXUIField(DisplayName = "Source Note ID", Enabled = false)]
        public override Guid? SourceNoteID { get; set; }
        #endregion
        #region SourceLineNbr
        public new abstract class sourceLineNbr : PX.Data.BQL.BqlInt.Field<sourceLineNbr> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Source Line Nbr.", Enabled = false)]
        public override int? SourceLineNbr { get; set; }
        #endregion
        #region TranDate
        public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        [PXDBDate]
        [PXDBDefault(typeof(FSServiceOrder.orderDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Transaction Date")]
        public override DateTime? TranDate
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
        public new abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }

        [PXDBScalar(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>))]
        [PXDefault(typeof(Search<INPlanType.planType, Where<INPlanType.inclQtyFSSrvOrdBooked, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXString(SOOrderTypeOperation.orderPlanType.Length, IsFixed = true)]
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
        #region RequireShipping
        public new abstract class requireShipping : PX.Data.BQL.BqlBool.Field<requireShipping> { }

        [PXBool()]
        [PXFormula(typeof(Current<SOOrderType.requireShipping>))]
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
        [PXFormula(typeof(Current<SOOrderType.requireAllocation>))]
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
        [PXFormula(typeof(Current<SOOrderType.requireLocation>))]
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
        #region LineQtyAvail
        public new abstract class lineQtyAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyAvail> { }
        [PXDecimal(6)]
        public override decimal? LineQtyAvail
        {
            get;
            set;
        }
        #endregion
        #region LineQtyHardAvail
        public new abstract class lineQtyHardAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyHardAvail> { }

        private decimal? _LineQtyHardAvail;
        [PXDecimal(6)]
        public override decimal? LineQtyHardAvail
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
        public override DateTime? OrderDate
        {
            get { return this.TranDate; }
            set { TranDate = value; }
        }
        #endregion
        #region ShipDate
        public new abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }

        [PXDBDate()]
        [PXDefault(typeof(FSServiceOrder.orderDate))]
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
        #region TranDesc
        public new abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        [PXDBString(255, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description")]
        public override string TranDesc { get; set; }
        #endregion

        #region NoteID
        public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public override Guid? NoteID { get; set; }
        #endregion
        #region CreatedByID
        public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public override Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public override string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public override DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public override Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public override string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public override DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public override byte[] tstamp { get; set; }
        #endregion

        #region Status
        public new abstract class status : ListField_Status_AppointmentDet
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Status_AppointmentDet.OPEN)]
        [status.ListAtrribute]
        [PXUIField(DisplayName = "Line Status", Visibility = PXUIVisibility.SelectorVisible)]
        public override string Status { get; set; }
        #endregion
        #region ScheduleDetID
        public new abstract class scheduleDetID : PX.Data.BQL.BqlInt.Field<scheduleDetID> { }

        [PXDBInt]
        [PXUIField(Enabled = false, Visible = false)]
        public override int? ScheduleDetID { get; set; }
        #endregion
        #region SMEquipmentID
        public new abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorMaintenanceEquipment(typeof(FSServiceOrder.srvOrdType),
                                        typeof(FSServiceOrder.billCustomerID),
                                        typeof(FSServiceOrder.customerID),
                                        typeof(FSServiceOrder.locationID),
                                        typeof(FSServiceOrder.branchLocationID))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                        TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public override int? SMEquipmentID { get; set; }
        #endregion
        #region PostID
        public new abstract class postID : PX.Data.BQL.BqlInt.Field<postID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Post ID")]
        public override int? PostID { get; set; }
        #endregion
        #region AcctID
        public new abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        [PXFormula(typeof(Default<inventoryID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false)]
        public override int? AcctID { get; set; }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [PXFormula(typeof(Default<FSSODetService.acctID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(FSSODetService.acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
        public override int? SubID { get; set; }
        #endregion
        #region Scheduled
        public new abstract class scheduled : PX.Data.BQL.BqlBool.Field<scheduled> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Service Scheduled", Enabled = false, Visible = false)]
        public override bool? Scheduled { get; set; }
        #endregion
        #region ScheduleID
        public new abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        public override int? ScheduleID { get; set; }
        #endregion
        #region EnablePurchaseOrder
        public new abstract class enablePO : PX.Data.BQL.BqlBool.Field<enablePO> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for PO", FieldClass = "DISTINV")]
        public override bool? EnablePO { get; set; }

        public new abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        [PXBool]
        [PXUIField(DisplayName = "Mark for PO")]
        public override bool? POCreate
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
        public new abstract class poVendorID : PX.Data.BQL.BqlInt.Field<poVendorID> { }

        [VendorNonEmployeeActive(DisplayName = "Vendor ID", DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, Visible = false, FieldClass = "DISTINV")]
        [PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
            Where<INItemSiteSettings.inventoryID, Equal<Current<FSSODetService.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<FSSODetService.siteID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<enablePO>))]
        public override int? POVendorID { get; set; }
        public new abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        [PXInt]
        [PXUIField(DisplayName = "Vendor ID")]
        public override int? VendorID
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
        public new abstract class poVendorLocationID : PX.Data.BQL.BqlInt.Field<poVendorLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSSODetService.poVendorID>>,
                            And<MatchWithBranch<Location.vBranchID>>>),
                DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location ID", Visible = false, FieldClass = "DISTINV")]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        [PXFormula(typeof(Default<FSSODetService.poVendorID>))]
        [PXDefault(typeof(Coalesce<
            Search<INItemSiteSettings.preferredVendorLocationID,
            Where<INItemSiteSettings.inventoryID, Equal<Current<FSSODetService.inventoryID>>, 
                    And<INItemSiteSettings.preferredVendorID, Equal<Current<FSSODetService.poVendorID>>>>>,
            Search2<Vendor.defLocationID,
                InnerJoin<CRLocation,
                    On<CRLocation.locationID, Equal<Vendor.defLocationID>,
                    And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
                Where<Vendor.bAccountID, Equal<Current<FSSODetService.poVendorID>>,
                    And<CRLocation.isActive, Equal<True>, And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? POVendorLocationID { get; set; }
        #endregion

        #region POType
        public new abstract class poType : PX.Data.BQL.BqlString.Field<poType> { }
        [PXDBString(2)]
        [PXUIField(DisplayName = "Order Type", FieldClass = "DISTINV")]
        public override String POType { get; set; }
        #endregion
        #region PONbr
        public new abstract class poNbr : PX.Data.BQL.BqlString.Field<poNbr> { }

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
        public override string PONbr { get; set; }
        #endregion
        #region POSource
        public new abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource> { }

        [PXDBString()]
        [PXDefault(INReplenishmentSource.PurchaseToOrder, PersistingCheck = PXPersistingCheck.Nothing)]
        [INReplenishmentSource.SOList]
        [PXUIField(DisplayName = "PO Source", Enabled = false, FieldClass = "DISTINV")]
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
        #region POStatus
        public new abstract class poStatus : PX.Data.BQL.BqlString.Field<poStatus> { }

        [PXDBString]
        [POOrderStatus.List]
        [PXUIField(DisplayName = "PO Status", Enabled = false, FieldClass = "DISTINV")]
        public override string POStatus { get; set; }
        #endregion
        #region POSiteID
        public new abstract class pOSiteID : PX.Data.BQL.BqlInt.Field<pOSiteID> { }

        [Site(DisplayName = "Purchase Warehouse", FieldClass = "DISTINV")]
        [PXForeignReference(typeof(Field<pOSiteID>.IsRelatedTo<INSite.siteID>))]
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
        #region POLineNbr
        public new abstract class poLineNbr : PX.Data.BQL.BqlInt.Field<poLineNbr> { }
        [PXDBInt]
        [PXUIField(DisplayName = "PO Line Nbr.", FieldClass = "DISTINV")]
        public override Int32? POLineNbr { get; set; }
        #endregion
        #region POCompleted
        public new abstract class poCompleted : PX.Data.BQL.BqlBool.Field<poCompleted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "PO Completed", Enabled = false, Visible = false, FieldClass = "DISTINV")]
        public override bool? POCompleted { get; set; }
        #endregion

        #region ManualCost
        public new abstract class manualCost : PX.Data.BQL.BqlBool.Field<manualCost> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Cost", Visible = false)]
        public override bool? ManualCost { get; set; }
        #endregion

        #region UnitPrice
        public new abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Unit Price", Enabled = false)]
        public override Decimal? UnitPrice { get; set; }
        #endregion
        #region CuryUnitPrice
        public new abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        [PXFormula(typeof(Default<FSSODetService.contractRelated>))]
        [PXDBCurrency(typeof(curyInfoID), typeof(unitPrice))]
        [PXUIField(DisplayName = "Unit Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? CuryUnitPrice { get; set; }
        #endregion

        #region EstimatedTranAmt
        public new abstract class estimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<estimatedTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Amount", Enabled = false)]
        public override Decimal? EstimatedTranAmt { get; set; }
        #endregion
        #region CuryEstimatedTranAmt
        public new abstract class curyEstimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEstimatedTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<lineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<status, Equal<FSSODet.status.Canceled>>,
                                    SharedClasses.decimal_0>>,
                                Mult<curyUnitPrice, estimatedQty>>),
                        typeof(SumCalc<FSServiceOrder.curyEstimatedOrderTotal>))]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
        public override Decimal? CuryEstimatedTranAmt { get; set; }
        #endregion
        #region BillableTranAmt
        public new abstract class billableTranAmt : PX.Data.BQL.BqlDecimal.Field<billableTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public override Decimal? BillableTranAmt { get; set; }
        #endregion
        #region CuryBillableTranAmt
        public new abstract class curyBillableTranAmt : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(billableTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        isBillable, Equal<False>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        contractRelated, Equal<False>,
                                        And<lineType, Equal<lineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        contractRelated, Equal<True>,
                                        And<isBillable, Equal<True>>>,
                                    Mult<curyExtraUsageUnitPrice, billableQty>>>>,
                                Mult<curyUnitPrice, billableQty>>),
                        typeof(SumCalc<FSServiceOrder.curyBillableOrderTotal>))]
        [PXUIField(DisplayName = "Amount", Enabled = false)]
        public override Decimal? CuryBillableTranAmt { get; set; }
        #endregion

        #region Appointment Summary fields
        #region ApptNumber
        public new abstract class apptNumber : PX.Data.BQL.BqlInt.Field<apptNumber> { }

        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Appointment Count", Enabled = false)]
        public override int? ApptNumber { get; set; }
        #endregion
        #region ApptDuration
        public new abstract class apptDuration : PX.Data.BQL.BqlInt.Field<apptDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Appointment Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? ApptDuration { get; set; }
        #endregion
        #region ApptQty
        public new abstract class apptQty : PX.Data.BQL.BqlDecimal.Field<apptQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Appointment Quantity", Enabled = false)]
        public override decimal? ApptQty { get; set; }
        #endregion


        #region ApptTranAmt
        public new abstract class apptTranAmt : PX.Data.BQL.BqlDecimal.Field<apptTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Appointment Amount", Enabled = false)]
        public override Decimal? ApptTranAmt { get; set; }
        #endregion
        #region CuryApptTranAmt
        public new abstract class curyApptTranAmt : PX.Data.BQL.BqlDecimal.Field<curyApptTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(apptTranAmt))]
        [PXUIField(DisplayName = "Appointment Amount", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUnboundFormula(typeof(curyApptTranAmt), typeof(SumCalc<FSServiceOrder.curyApptOrderTotal>))]
        public override Decimal? CuryApptTranAmt { get; set; }
        #endregion
        #endregion

        #region EquipmentAction
        public new abstract class equipmentAction : ListField_EquipmentAction
        {
        }

        [PXDBString(2, IsFixed = true)]
        [equipmentAction.ListAtrribute]
        [PXDefault(ID.Equipment_Action.NONE)]
        [PXUIField(DisplayName = "Equipment Action", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public override string EquipmentAction { get; set; }
        #endregion
        #region NewTargetEquipmentLineNbr
        public new abstract class newTargetEquipmentLineNbr : PX.Data.BQL.BqlString.Field<newTargetEquipmentLineNbr> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Model Equipment Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorNewTargetEquipmentServiceOrder]
        public override string NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region SuspendedTargetEquipmentID
        public new abstract class suspendedTargetEquipmentID : PX.Data.BQL.BqlInt.Field<suspendedTargetEquipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Suspended Target Equipment ID", Enabled = false, Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorMaintenanceEquipment(typeof(FSServiceOrder.srvOrdType),
                                        typeof(FSServiceOrder.billCustomerID),
                                        typeof(FSServiceOrder.customerID),
                                        typeof(FSServiceOrder.locationID),
                                        typeof(FSServiceOrder.branchLocationID))]
        public override int? SuspendedTargetEquipmentID { get; set; }
        #endregion
        #region ComponentID
        public new abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorComponentIDServiceOrder(typeof(FSSODetService), typeof(FSSODetPart))]
        public override int? ComponentID { get; set; }
        #endregion
        #region ComponentLineRef
        public new abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorEquipmentLineRefServiceOrderAppointment(
                typeof(inventoryID),
                typeof(SMequipmentID),
                typeof(componentID),
                typeof(equipmentAction))]
        public override int? EquipmentLineRef { get; set; }
        #endregion
        #region Warranty
        public new abstract class warranty : PX.Data.BQL.BqlBool.Field<warranty> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Warranty", Enabled = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public override bool? Warranty { get; set; }
        #endregion
        #region SONewTargetEquipmentLineNbr
        public new abstract class sONewTargetEquipmentLineNbr : PX.Data.BQL.BqlInt.Field<sONewTargetEquipmentLineNbr> { }

        [PXInt]
        [PXUIField(DisplayName = "SO NewTargetEquipmentLineNbr", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public override int? SONewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region Comment
        public new abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Equipment Action Comment", FieldClass = FSSetup.EquipmentManagementFieldClass, Visible = false)]
        public override string Comment { get; set; }
        #endregion

        #region CostCodeID
        public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), typeof(acctID), typeof(projectTaskID), Visible = false)]
        public override int? CostCodeID { get; set; }
        #endregion
        #region SkipCostCodeValidation
        public new abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<Where2<
                                Where<lineType, Equal<ListField_LineType_ALL.Service>,
                                   Or<lineType, Equal<ListField_LineType_ALL.NonStockItem>,
                                   Or<lineType, Equal<ListField_LineType_ALL.Inventory_Item>>>>,
                                And<
                                    Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                    And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>>>, False, True>))]
        public override bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region Tax Fields
        #region TaxCategoryID
        public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<FSSODetService.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<inventoryID>))]
        public override String TaxCategoryID { get; set; }
        #endregion
        #region GroupDiscountRate
        public new abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public override Decimal? GroupDiscountRate { get; set; }
        #endregion
        #region DocumentDiscountRate
        public new abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }

        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "1.0")]
        public override Decimal? DocumentDiscountRate { get; set; }
        #endregion
        #endregion

        #region Contract related fields

        #region CoveredQty 
        public new abstract class coveredQty : PX.Data.BQL.BqlDecimal.Field<coveredQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<FSSODetService.contractRelated>))]
        [PXUIField(DisplayName = "Covered Quantity", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override decimal? CoveredQty { get; set; }
        #endregion
        #region ExtraUsageQty  
        public new abstract class extraUsageQty : PX.Data.BQL.BqlDecimal.Field<extraUsageQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        contractRelated, Equal<True>>,
                                    Sub<estimatedQty, coveredQty>>,
                            SharedClasses.decimal_0>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Overage Quantity", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override decimal? ExtraUsageQty { get; set; }
        #endregion
        #region ExtraUsageUnitPrice 
        public new abstract class extraUsageUnitPrice : PX.Data.BQL.BqlDecimal.Field<extraUsageUnitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Overage Unit Price", Enabled = false, FieldClass = "FSCONTRACT")]
        public override Decimal? ExtraUsageUnitPrice { get; set; }
        #endregion
        #region CuryExtraUsageUnitPrice
        public new abstract class curyExtraUsageUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyExtraUsageUnitPrice> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extraUsageUnitPrice))]
        [PXFormula(typeof(Default<FSSODetService.contractRelated>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overage Unit Price", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override Decimal? CuryExtraUsageUnitPrice { get; set; }
        #endregion

        #endregion

        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        [INExpireDate(typeof(FSSODetService.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region CuryUnitCost
        public new abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "DISTINV")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<FSSODetService.enablePO>))]
        public override Decimal? CuryUnitCost { get; set; }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Coalesce<
                                                Search<InventoryItem.stdCost, 
                                                        Where<InventoryItem.inventoryID, Equal<Current<FSSODetService.inventoryID>>,
                                                            And<InventoryItem.stdCostDate, LessEqual<Current<FSServiceOrder.orderDate>>>>>,
                                                Search<InventoryItem.lastStdCost,
                                                        Where<InventoryItem.inventoryID, Equal<Current<FSSODetService.inventoryID>>,
                                                            And<InventoryItem.stdCostDate, Greater<Current<FSServiceOrder.orderDate>>>>>>))]
        public override Decimal? UnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public new abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(DisplayName = "Ext. Cost")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public new abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? ExtCost { get; set; }

        #endregion

        #region Mem_LastReferencedBy
        public new abstract class mem_LastReferencedBy : PX.Data.BQL.BqlString.Field<mem_LastReferencedBy> { }

        [PXString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Last Reference", Enabled = false)]
        [PXSelector(typeof(FSAppointment.refNbr))]
        public override string Mem_LastReferencedBy { get; set; }
        #endregion
        #region Selected
        public new abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public override bool? Selected { get; set; }
        #endregion
        #region EstimatedDurationReport
        public new abstract class estimatedDurationReport : PX.Data.BQL.BqlInt.Field<estimatedDurationReport> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<lineType,
                                    NotEqual<ListField_LineType_ALL.Inventory_Item>>,
                                    estimatedDuration>,
                                /*default case*/
                                SharedClasses.int_0>))]
        public override int? EstimatedDurationReport { get; set; }
        #endregion
        #region EnableStaffID
        public abstract class enableStaffID : PX.Data.BQL.BqlBool.Field<enableStaffID> { }

        [PXBool]
        public virtual bool? EnableStaffID { get; set; }
        #endregion
        #region StaffID
        public new abstract class staffID : PX.Data.BQL.BqlInt.Field<staffID> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member ID")]
        public override int? StaffID { get; set; }
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
    }
}