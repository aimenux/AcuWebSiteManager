using PX.Data;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    [PXCacheName(TX.TableName.APPOINTMENTDET_PART)]
    public class FSAppointmentDetPart : FSAppointmentDet
    {
        #region AppointmentID
        public new abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        public override int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public new abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBIdentity(IsKey = true)]
        public override int? AppDetID { get; set; }
        #endregion

        #region SODetID
        public new abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        [PXCheckUnique(Where = typeof(Where<FSAppointmentDetPart.appointmentID, Equal<Current<FSAppointment.appointmentID>>>))]
        [PXUIField(DisplayName = "Line Ref.")]
        [FSSelectorSODetIDPart]
        public override int? SODetID { get; set; }
        #endregion
        #region LineRef
        public new abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public override string LineRef { get; set; }
        #endregion
        #region BranchID
        public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [Branch(typeof(FSAppointment.branchID))]
        public override int? BranchID { get; set; }
        #endregion

        #region CuryInfoID
        public new abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSAppointment.curyInfoID))]
        public override Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public new abstract class lineType : ListField_LineType_Part_ALL
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        [PXDefault(ID.LineType_ServiceTemplate.INVENTORY_ITEM)]
        public override string LineType { get; set; }
        #endregion
        #region IsPrepaid
        public new abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false)]
        public override bool? IsPrepaid { get; set; }
        #endregion
        #region IsBillable
        public new abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public override bool? IsBillable { get; set; }
        #endregion
        #region ManualPrice
        public new abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
        public override bool? ManualPrice { get; set; }
        #endregion

        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<lineType>))]
        [InventoryIDByLineType(typeof(lineType), headers: new Type[]
                                {
                                    typeof(InventoryItem.inventoryCD),
                                    typeof(InventoryItem.itemClassID),
                                    typeof(InventoryItem.itemStatus),
                                    typeof(InventoryItem.descr),
                                    typeof(InventoryItem.itemType),
                                    typeof(InventoryItem.baseUnit),
                                    typeof(InventoryItem.salesUnit),
                                    typeof(InventoryItem.purchaseUnit),
                                    typeof(InventoryItem.basePrice),
                                    typeof(FSxService.actionType)
                                }, Filterable = true)]
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
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem")]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                            Where<
                                InventoryItem.inventoryID, Equal<Current<FSAppointmentDetPart.inventoryID>>,
                                And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public override int? SubItemID { get; set; }
        #endregion

        #region UOM
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
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

        [PXString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.NONE, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Rule", Enabled = false)]
        public override string BillingRule { get; set; }
        #endregion
        #region ServiceType
        public new abstract class serviceType : ListField_Appointment_Service_Action_Type
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Service_Action_Type.NO_ITEMS_RELATED, PersistingCheck = PXPersistingCheck.Nothing)]
        [serviceType.List]
        [PXUIField(DisplayName = "Pickup/Delivery Action", Enabled = false)]
        public override string ServiceType { get; set; }
        #endregion

        #region SiteID
        // SiteID and SiteLocationID should be together in the same DAC in order to LocationAvail attribute works.
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [FSSiteAvailAttribute(typeof(inventoryID), typeof(subItemID), DisplayName = "Warehouse")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? SiteID { get; set; }
        #endregion
        #region SiteLocationID
        // SiteID and SiteLocationID should be together in the same DAC in order to LocationAvail attribute works.
        public new abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [PXFormula(typeof(Default<FSAppointmentDetPart.inventoryID, FSAppointmentDetPart.subItemID, FSAppointmentDetPart.siteID>))]
        [LocationAvail(typeof(FSAppointmentDetPart.inventoryID), typeof(FSAppointmentDetPart.subItemID), typeof(FSAppointmentDetPart.siteID), true, false, false)]
        public override int? SiteLocationID { get; set; }
        #endregion

        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        [PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
        [PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
        [FSINLotSerialNbr(typeof(FSAppointmentDetPart.inventoryID), typeof(FSAppointmentDetPart.subItemID), typeof(FSAppointmentDetPart.siteLocationID))]
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

        #region EstimatedDuration
        public new abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public new abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public override decimal? EstimatedQty { get; set; }
        #endregion

        #region Status
        public new abstract class status : ListField_Status_Parts
        {
        }

        [PXDBString(1, IsFixed = true)]
        [status.ListAtrribute]
        [PXDefault(ID.Status_Parts.OPEN)]
        [PXUIField(DisplayName = "Status")]
        public override string Status { get; set; }
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
        [PXUIField(DisplayName = "CreatedByID")]
        public override Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public override string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public override DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public override Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public override string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public override DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public override byte[] tstamp { get; set; }
        #endregion   
        #region ExpenseEmployeeID
        public new abstract class expenseEmployeeID : PX.Data.BQL.BqlInt.Field<expenseEmployeeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Expense Staff Member ID")]
        [FSSelector_StaffMember_All]
        public override int? ExpenseEmployeeID { get; set; }
        #endregion  

        #region TranDate
        public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Transaction date")]
        public override DateTime? TranDate { get; set; }
        #endregion

        #region IsFree
        public new abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Free")]
        public override bool? IsFree { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region UnitPrice
        public new abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Price")]
        public override decimal? UnitPrice { get; set; }
        #endregion
        #region CuryUnitPrice
        public new abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitPrice))]
        [PXFormula(typeof(Default<FSAppointmentDetPart.contractRelated>))]
        [PXUIField(DisplayName = "Unit Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override decimal? CuryUnitPrice { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region EstimatedTranAmt
        public new abstract class estimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<estimatedTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false, Visible = false)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<FSAppointmentDetPart.isBillable, Equal<True>>,
                                            FSAppointmentDetPart.estimatedTranAmt>,
                                        /*default case*/
                                        SharedClasses.decimal_0>),
                          typeof(SumCalc<FSAppointment.estimatedLineTotal>))]
        public override decimal? EstimatedTranAmt { get; set; }
        #endregion
        #region CuryEstimatedTranAmt
        public new abstract class curyEstimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEstimatedTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<lineType.Inventory_Item>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<status, Equal<ListField_Status_AppointmentDet.Canceled>>,
                                    SharedClasses.decimal_0>>,
                                Mult<curyUnitPrice, estimatedQty>>),
                        typeof(SumCalc<FSAppointment.curyEstimatedLineTotal>))]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
        public override decimal? CuryEstimatedTranAmt { get; set; }
        #endregion

        #region ActualDuration
        public new abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXUIField(DisplayName = "Actual Duration")]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<FSAppointmentDetPart.lineType,
                                            Equal<ListField_LineType_ALL.Service>,
                                            And<FSAppointmentDetPart.status, NotEqual<ListField_Status_AppointmentDet.Canceled>>>,
                                            FSAppointmentDetPart.actualDuration>,
                                        /*default case*/
                                        SharedClasses.int_0>),
                          typeof(SumCalc<FSAppointment.actualDurationTotal>))]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? ActualDuration { get; set; }
        #endregion       
        #region Qty
        public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        [PXDBQuantity]
        [PXDefault(typeof(Switch<
                                Case<Where<
                                        status, NotEqual<FSAppointmentDet.status.Canceled>,
                                        And<
                                            Where<
                                                Current<FSAppointment.status>, Equal<FSAppointment.status.InProcess>,
                                                Or<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>>>>>,
                                    estimatedQty>,
                                SharedClasses.decimal_0>))]
        [PXFormula(typeof(Default<status>))]
        [PXUIField(DisplayName = "Actual Quantity")]
        public override decimal? Qty { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region TranAmt
        public new abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Transaction Amount", Enabled = false)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<FSAppointmentDetPart.isBillable, Equal<True>,
                                                And<FSAppointmentDetPart.status, NotEqual<FSAppointmentDetPart.status.Canceled>>>,
                                            FSAppointmentDetPart.tranAmt>,
                                        /*default case*/
                                        SharedClasses.decimal_0>),
                          typeof(SumCalc<FSAppointment.lineTotal>))]
        public override decimal? TranAmt { get; set; }
        #endregion
        #region CuryTranAmt
        public new abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(tranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<lineType.Inventory_Item>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0>,
                                Mult<curyUnitPrice, qty>>),
                        typeof(SumCalc<FSAppointment.curyLineTotal>))]
        [PXUIField(DisplayName = "Actual Amount", Enabled = false)]
        public override Decimal? CuryTranAmt { get; set; }
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
                                        And<lineType, Equal<lineType.Inventory_Item>,
                                        And<billingRule, Equal<billingRule.None>>>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        contractRelated, Equal<True>,
                                        And<isBillable, Equal<True>>>,
                                    Mult<curyExtraUsageUnitPrice, billableQty>>>>,
                                Mult<curyUnitPrice, billableQty>>),
                        typeof(SumCalc<FSAppointment.curyBillableLineTotal>))]
        [PXUIField(DisplayName = "Billable Amount", Enabled = false)]
        public override Decimal? CuryBillableTranAmt { get; set; }
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
        [PXDefault(typeof(FSAppointment.dfltProjectTaskID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task")]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSAppointmentDetPart.projectID>>>))]
        public override int? ProjectTaskID { get; set; }
        #endregion
        #region ScheduleDetID
        public new abstract class scheduleDetID : PX.Data.BQL.BqlInt.Field<scheduleDetID> { }

        [PXDBInt]
        [PXUIField(Enabled = false, Visible = false)]
        public override int? ScheduleDetID { get; set; }
        #endregion

        #region BillableQty
        public new abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity(typeof(FSAppointmentDetPart.uOM), typeof(FSAppointmentDetPart.baseBillableQty))]
        [PXFormula(typeof(Default<FSAppointmentDetPart.isBillable, FSAppointmentDetPart.contractRelated, FSAppointmentDetPart.lineType>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSAppointmentDetPart.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSAppointmentDetPart.contractRelated, Equal<True>>,
                                    FSAppointmentDetPart.extraUsageQty,
                                Case<
                                    Where<
                                        FSAppointmentDetPart.status, Equal<FSAppointmentDet.status.Canceled>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where2<
                                        Where<
                                            Current<FSAppointment.status>, Equal<FSAppointment.status.AutomaticScheduled>,
                                            Or<Current<FSAppointment.status>, Equal<FSAppointment.status.ManualScheduled>>>,
                                        And<contractRelated, Equal<False>>>,
                                    FSAppointmentDetPart.estimatedQty>>>>,
                            FSAppointmentDetPart.qty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Billable Quantity", Enabled = false)]
        public override decimal? BillableQty { get; set; }
        #endregion
        #region BaseBillableQty
        public new abstract class baseBillableQty : PX.Data.BQL.BqlDecimal.Field<baseBillableQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Qty.", Visible = false, Enabled = false)]
        public override Decimal? BaseBillableQty { get; set; }
        #endregion

        #region SMEquipmentID
        public new abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Target Equipment ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(typeof(FSAppointment.mem_SMequipmentID), PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorMaintenanceEquipment(typeof(FSServiceOrder.srvOrdType),
                                        typeof(FSServiceOrder.billCustomerID),
                                        typeof(FSServiceOrder.customerID),
                                        typeof(FSServiceOrder.locationID),
                                        typeof(FSServiceOrder.branchLocationID))]
        [PXRestrictor(typeof(Where<FSEquipment.status, Equal<EPEquipmentStatus.EquipmentStatusActive>>),
                        TX.Messages.EQUIPMENT_IS_INSTATUS, typeof(FSEquipment.status))]
        public override int? SMEquipmentID { get; set; }
        #endregion
        #region ActualDateTimeBegin
        public new abstract class actualDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBegin> { }

        protected new DateTime? _ActualDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Start Time")]
        public override DateTime? ActualDateTimeBegin
        {
            get
            {
                return this._ActualDateTimeBegin;
            }

            set
            {
                this.ActualDateTimeBeginUTC = value;
                this._ActualDateTimeBegin = value;
            }
        }
        #endregion
        #region ActualDateTimeEnd
        public new abstract class actualDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEnd> { }

        protected new DateTime? _ActualDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual End Time")]
        public override DateTime? ActualDateTimeEnd
        {
            get
            {
                return this._ActualDateTimeEnd;
            }

            set
            {
                this.ActualDateTimeEndUTC = value;
                this._ActualDateTimeEnd = value;
            }
        }
        #endregion
        #region SourceSalesOrderRefNbr
        public new abstract class sourceSalesOrderRefNbr : PX.Data.BQL.BqlString.Field<sourceSalesOrderRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Sales Order Ref. Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<FSAppointmentDetPart.sourceSalesOrderType>>>>))]
        public override string SourceSalesOrderRefNbr { get; set; }
        #endregion
        #region SourceSalesOrderType
        public new abstract class sourceSalesOrderType : PX.Data.BQL.BqlString.Field<sourceSalesOrderType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Source Type", Enabled = false, Visible = false)]
        public override string SourceSalesOrderType { get; set; }
        #endregion
        #region StaffID
        public new abstract class staffID : PX.Data.BQL.BqlInt.Field<staffID> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member ID")]
        public override int? StaffID { get; set; }
        #endregion
        #region StaffActualDuration
        public new abstract class staffActualDuration : PX.Data.BQL.BqlInt.Field<staffActualDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXUIField(DisplayName = "Staff Actual Duration")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? StaffActualDuration { get; set; }
        #endregion
        #region EnableStaffID
        public new abstract class enableStaffID : PX.Data.BQL.BqlBool.Field<enableStaffID> { }

        [PXBool]
        public override bool? EnableStaffID { get; set; }
        #endregion
        #region StaffRelated
        public new abstract class staffRelated : PX.Data.BQL.BqlBool.Field<staffRelated> { }

        [PXBool]
        public override bool? StaffRelated { get; set; }
        #endregion
        #region PriceType
        public new abstract class priceType : ListField_PriceType
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXDefault(ID.PriceType.BASE, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price Type", Enabled = false)]
        [priceType.ListAtrribute]
        public override string PriceType { get; set; }
        #endregion
        #region PriceCode
        public new abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }

        [PXDBString(30, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price Code", Enabled = false)]
        public override string PriceCode { get; set; }
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

        [PXFormula(typeof(Default<FSAppointmentDetPart.acctID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(FSAppointmentDetPart.acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
        public override int? SubID { get; set; }
        #endregion
        #region Tax Fields
        #region TaxCategoryID
        public new abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<FSAppointmentDetPart.inventoryID>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<inventoryID>))]
        public override String TaxCategoryID { get; set; }

        #endregion
        #endregion
        #region KeepActualDateTimes
        public new abstract class keepActualDateTimes : PX.Data.BQL.BqlBool.Field<keepActualDateTimes> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manually Handle Time")]
        public override bool? KeepActualDateTimes { get; set; }
        #endregion

        #region ScheduleID
        public new abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        public override int? ScheduleID { get; set; }
        #endregion

        #region CostCodeID
        public new abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), typeof(acctID), typeof(projectTaskID))]
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

        #region UTC Fields
        #region ActualDateTimeBeginUTC
        public new abstract class actualDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Start Time")]
        public override DateTime? ActualDateTimeBeginUTC { get; set; }
        #endregion
        #region ActualDateTimeEndUTC
        public new abstract class actualDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual End Time")]
        public override DateTime? ActualDateTimeEndUTC { get; set; }
        #endregion
        #endregion

        #region AuxEstimatedDuration
        public new abstract class auxEstimatedDuration : PX.Data.BQL.BqlInt.Field<auxEstimatedDuration> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<FSAppointmentDetPart.inventoryID,
                                    IsNull>,
                                    FSAppointmentDetPart.estimatedDuration>,
                                /*default case*/
                                SharedClasses.int_0>))]
        public override int? AuxEstimatedDuration { get; set; }
        #endregion
        #region AuxActualDuration
        public new abstract class auxActualDuration : PX.Data.BQL.BqlInt.Field<auxActualDuration> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<FSAppointmentDetPart.inventoryID,
                                    IsNull>,
                                    FSAppointmentDetPart.actualDuration>,
                                /*default case*/
                                SharedClasses.int_0>))]
        public override int? AuxActualDuration { get; set; }
        #endregion

        #region Equipment
        #region NewTargetEquipmentLineNbr
        public new abstract class newTargetEquipmentLineNbr : PX.Data.BQL.BqlString.Field<newTargetEquipmentLineNbr> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Model Equipment Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorNewTargetEquipmentAppointment]
        public override string NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region SuspendedTargetEquipmentID
        public new abstract class suspendedTargetEquipmentID : PX.Data.BQL.BqlInt.Field<suspendedTargetEquipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Suspended Target Equipment ID", Enabled = false, Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public override int? SuspendedTargetEquipmentID { get; set; }
        #endregion
        #region ComponentID
        public new abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [FSSelectorComponentIDAppointment(typeof(FSAppointmentDetPart), typeof(FSAppointmentDetPart))]
        public override int? ComponentID { get; set; }
        #endregion
        #region EquipmentLineRef
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
        #region Warranty
        public new abstract class warranty : PX.Data.BQL.BqlBool.Field<warranty> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Warranty", Enabled = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public override bool? Warranty { get; set; }
        #endregion
        #region SONewTargetEquipmentLineNbr
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
        #endregion

        #region Contract related fields

        #region ContractRelated
        public new abstract class contractRelated : PX.Data.BQL.BqlBool.Field<contractRelated> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Service Contract Item", IsReadOnly = true, FieldClass = "FSCONTRACT")]
        public override bool? ContractRelated { get; set; }
        #endregion
        #region CoveredQty 
        public new abstract class coveredQty : PX.Data.BQL.BqlDecimal.Field<coveredQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<FSAppointmentDetPart.contractRelated, FSAppointmentDetPart.estimatedQty, FSAppointmentDetPart.qty>))]
        [PXUIField(DisplayName = "Covered Quantity", IsReadOnly = true, Visible = false, FieldClass = "FSCONTRACT")]
        public override decimal? CoveredQty { get; set; }
        #endregion
        #region ExtraUsageQty  
        public new abstract class extraUsageQty : PX.Data.BQL.BqlDecimal.Field<extraUsageQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where2<
                                        Where<
                                            Current<FSAppointment.status>, Equal<FSAppointment.status.AutomaticScheduled>,
                                            Or<Current<FSAppointment.status>, Equal<FSAppointment.status.ManualScheduled>>>,
                                        And<contractRelated, Equal<True>>>,
                                        Sub<estimatedQty, coveredQty>,
                                Case<
                                    Where<contractRelated, Equal<True>>,
                                    Sub<qty, coveredQty>>>,
                            SharedClasses.decimal_0>))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Overage Quantity", IsReadOnly = true, Visible = false, FieldClass = "FSCONTRACT")]
        public override decimal? ExtraUsageQty { get; set; }
        #endregion

        #region CuryUnitCost
        public new abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? CuryUnitCost { get; set; }
        #endregion
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0", typeof(Search<INItemSite.tranUnitCost, Where<INItemSite.inventoryID, Equal<Current<FSAppointmentDetPart.inventoryID>>, And<INItemSite.siteID, Equal<Current<FSAppointmentDetPart.siteID>>>>>))]
        public override Decimal? UnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public new abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        status, NotEqual<status.Canceled>>,
                                    Mult<curyUnitCost, billableQty>>,
                                SharedClasses.decimal_0>),
                    typeof(SumCalc<FSAppointment.curyCostTotal>))]
        public override Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public new abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public override Decimal? ExtCost { get; set; }
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

        #region LotSerTrack
        public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }

        [PXString(1, IsFixed = true)]
        [PXDBScalar(typeof(Search2<INLotSerClass.lotSerTrack, 
                            InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>,
                            Where<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>>))]
        [PXDefault(typeof(Search2<INLotSerClass.lotSerTrack,
                            InnerJoin<InventoryItem, On<InventoryItem.lotSerClassID, Equal<INLotSerClass.lotSerClassID>>>,
                            Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<inventoryID>))]
        public string LotSerTrack { get; set; }
        #endregion
    }
}