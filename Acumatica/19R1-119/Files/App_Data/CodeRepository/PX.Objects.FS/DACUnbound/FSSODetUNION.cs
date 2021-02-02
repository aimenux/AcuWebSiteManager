using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXHidden]
    [PXBreakInheritance]
    [PXProjection(typeof(Select<FSSODet>), Persistent = false)]
    public class FSSODetUNION : FSSODet
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
                            Where<FSServiceOrder.srvOrdType, Equal<Current<FSSODetUNION.srvOrdType>>,
                                And<FSServiceOrder.refNbr, Equal<Current<FSSODetUNION.refNbr>>>>>))]
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

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.branchID))]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Search<Branch.branchID>), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public override int? BranchID { get; set; }
        #endregion

        #region CuryInfoID
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSServiceOrder.curyInfoID))]
        public override Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public new abstract class lineType : ListField_LineType_ALL
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXDefault]
        [lineType.ListAtrribute]
        [PXUIField(DisplayName = "Line Type")]
        public override string LineType { get; set; }
        #endregion
        #region IsPrepaid
        public new abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false)]
        public override bool? IsPrepaid { get; set; }
        #endregion

        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXUIField(DisplayName = "Inventory")]
        public override int? InventoryID { get; set; }
        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem")]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                            Where<
                                InventoryItem.inventoryID, Equal<Current<FSSODetUNION.inventoryID>>,
                                And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public override int? SubItemID { get; set; }
        #endregion

        #region UOM
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public override string UOM { get; set; }
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

        [PXDBInt]
        [PXUIField(DisplayName = "Warehouse")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIRequired(typeof(IIf<Where<
                                    lineType, NotEqual<lineType.Comment_Service>,
                                    And<lineType, NotEqual<lineType.Comment_Part>,
                                    And<lineType, NotEqual<lineType.Instruction_Service>,
                                    And<lineType, NotEqual<lineType.Instruction_Part>,
                                    And<lineType, NotEqual<lineType.Service_Template>>>>>>, True, False>))]
        public override int? SiteID { get; set; }
        #endregion
        #region SiteLocationID
        public new abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [PXDBInt()]
        [PXUIField(DisplayName = "Location")]
        public override int? SiteLocationID { get; set; }
        #endregion

        #region ContractRelated
        public new abstract class contractRelated : PX.Data.BQL.BqlBool.Field<contractRelated> { }

        [PXDBBool]
        [PXUIField(DisplayName = "Service Contract Item", IsReadOnly = true, FieldClass = "FSCONTRACT")]
        public override bool? ContractRelated { get; set; }
        #endregion

        #region EstimatedDuration
        public new abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Estimated Duration")]
        public override int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public new abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        [PXDBQuantity]
        [PXDefault(typeof(Switch<
                            Case<
                                Where<
                                    status, Equal<status.Canceled>>,
                                SharedClasses.decimal_0>,
                            SharedClasses.decimal_1>))]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public override decimal? EstimatedQty { get; set; }
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

        [PXQuantity]
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

        [PXDBQuantity]
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
        #region ShippedQty
        public new abstract class shippedQty : PX.Data.BQL.BqlDecimal.Field<shippedQty> { }

        [PXDBQuantity]
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

        #region IsBillable
        public new abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public override bool? IsBillable { get; set; }
        #endregion
        #region BillableQty
        public new abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Billable Quantity", Enabled = false)]
        public override decimal? BillableQty { get; set; }
        #endregion

        #region ProjectID
        public new abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.projectID))]
        [PXUIField(Visible = false)]
        public override int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public new abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.dfltProjectTaskID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task")]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSSODetUNION.projectID>>>))]
        public override int? ProjectTaskID { get; set; }
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
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Transaction Date")]
        public override DateTime? TranDate { get; set; }
        #endregion
        #region TranDesc
        public new abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        [PXDBString(Common.Constants.TranDescLength, IsUnicode = true)]
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

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false)]
        public override int? AcctID { get; set; }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(FSSODetUNION.acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
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
        [PXUIField(DisplayName = "Mark for PO")]
        public override bool? EnablePO { get; set; }
        #endregion
        #region POVendorID
        public new abstract class poVendorID : PX.Data.BQL.BqlInt.Field<poVendorID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Vendor ID")]
        public override int? POVendorID { get; set; }
        #endregion
        #region POVendorLocationID
        public new abstract class poVendorLocationID : PX.Data.BQL.BqlInt.Field<poVendorLocationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Vendor Location ID")]
        public override int? POVendorLocationID { get; set; }
        #endregion
        #region PONbr
        public new abstract class poNbr : PX.Data.BQL.BqlString.Field<poNbr> { }

        [PXDBString]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false)]
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
        #region POStatus
        public new abstract class poStatus : PX.Data.BQL.BqlString.Field<poStatus> { }

        [PXDBString]
        [POOrderStatus.List]
        [PXUIField(DisplayName = "PO Status", Enabled = false)]
        public override string POStatus { get; set; }
        #endregion
        #region POCompleted
        public new abstract class poCompleted : PX.Data.BQL.BqlBool.Field<poCompleted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "PO Completed", Enabled = false, Visible = false)]
        public override bool? POCompleted { get; set; }
        #endregion

        #region ManualPrice
        public new abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price", Visible = false)]
        public override bool? ManualPrice { get; set; }
        #endregion
        #region IsFree
        public new abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Free")]
        public override bool? IsFree { get; set; }
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
        [PXUIField(DisplayName = "Billable Amount", Enabled = false)]
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

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
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
        [PXUIField(DisplayName = "Model Equipment Line Nbr.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
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
        public override int? ComponentID { get; set; }
        #endregion
        #region ComponentLineRef
        public new abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
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
        public override bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region Tax Fields
        #region TaxCategoryID
        public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<FSSODetUNION.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXUIField(DisplayName = "Covered Quantity", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override decimal? CoveredQty { get; set; }
        #endregion
        #region ExtraUsageQty  
        public new abstract class extraUsageQty : PX.Data.BQL.BqlDecimal.Field<extraUsageQty> { }

        [PXDBQuantity]
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
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overage Unit Price", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public override Decimal? CuryExtraUsageUnitPrice { get; set; }
        #endregion

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
    }
}
