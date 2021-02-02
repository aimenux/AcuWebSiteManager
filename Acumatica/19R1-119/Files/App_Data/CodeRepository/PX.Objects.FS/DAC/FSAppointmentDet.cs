using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
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
    [PXCacheName(TX.TableName.APPOINTMENTDET)]
    public class FSAppointmentDet : IBqlTable, IFSSODetBase, IDocLine
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentDet.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentDet.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBIdentity]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region ApptLineNbr
        public abstract class apptLineNbr : PX.Data.BQL.BqlInt.Field<apptLineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment.lineCntr))]
        public virtual int? ApptLineNbr { get; set; }
        #endregion

        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Line Ref.")]
        [FSSelectorSODetIDService]
        public virtual int? SODetID { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.branchID))]
        [PXUIField(DisplayName = "Branch ID", Visible = false)]
        public virtual int? BranchID { get; set; }
        #endregion

        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSAppointment.curyInfoID))]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public abstract class lineType : ListField_LineType_ALL
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [lineType.ListAtrribute]
        [PXDefault]
        public virtual string LineType { get; set; }
        #endregion
        #region IsPrepaid
        public abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false)]
        public virtual bool? IsPrepaid { get; set; }
        #endregion
        #region IsBillable
        public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Billable")]
        public virtual bool? IsBillable { get; set; }
        #endregion
        #region ManualPrice
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Price")]
        public virtual bool? ManualPrice { get; set; }
        #endregion

        #region InventoryID
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem")]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                            Where<
                                InventoryItem.inventoryID, Equal<Current<FSAppointmentDet.inventoryID>>,
                                And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public virtual int? SubItemID { get; set; }
        #endregion

        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        public string _UOM;
        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
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

        [PXString(4, IsFixed = true)]
        [billingRule.List]
        [PXDefault(ID.BillingRule.FLAT_RATE)]
        [PXUIField(DisplayName = "Billing Rule", Enabled = false)]
        public virtual string BillingRule { get; set; }
        #endregion
        #region ServiceType
        public abstract class serviceType : ListField_Appointment_Service_Action_Type
        {
        }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Service_Action_Type.NO_ITEMS_RELATED, PersistingCheck = PXPersistingCheck.Nothing)]
        [serviceType.List]
        [PXUIField(DisplayName = "Pickup/Delivery Action", Enabled = false)]
        public virtual string ServiceType { get; set; }
        #endregion

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [FSSiteAvailAttribute(typeof(FSAppointmentDet.inventoryID), typeof(FSAppointmentDet.subItemID), DisplayName = "Warehouse")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? SiteID { get; set; }
        #endregion
        #region SiteLocationID
        public abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [Location(typeof(FSAppointmentDet.siteID), DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
        public virtual int? SiteLocationID { get; set; }
        #endregion
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        protected String _LotSerialNbr;

        [PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSINLotSerialNbr(typeof(FSAppointmentDet.inventoryID), typeof(FSAppointmentDet.subItemID), typeof(FSAppointmentDet.siteLocationID))]
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

        #region EstimatedDuration
        public abstract class estimatedDuration : PX.Data.BQL.BqlInt.Field<estimatedDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Estimated Duration")]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Service>,
                                            And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>>>,
                                        FSAppointmentDet.estimatedDuration>,
                                    SharedClasses.int_0>),
                          typeof(SumCalc<FSAppointment.estimatedDurationTotal>))]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Estimated Quantity")]
        public virtual decimal? EstimatedQty { get; set; }
        #endregion

        #region Status
        public abstract class status : ListField_Status_AppointmentDet
        {
        }

        [PXDBString(1, IsFixed = true)]
        [status.ListAtrribute]
        [PXDefault(ID.Status_AppointmentDet.OPEN)]
        [PXUIField(DisplayName = "Line Status")]
        public virtual string Status { get; set; }
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
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region ExpenseEmployeeID
        public abstract class expenseEmployeeID : PX.Data.BQL.BqlInt.Field<expenseEmployeeID> { }

        [PXInt]
        [PXUIField(DisplayName = "Expense Staff Member ID")]
        [FSSelector_StaffMember_All]
        public virtual int? ExpenseEmployeeID { get; set; }
        #endregion
        
        #region TranDate
        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Transaction date")]
        public virtual DateTime? TranDate { get; set; }
        #endregion

        #region IsFree
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Is Free")]
        public virtual bool? IsFree { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region UnitPrice
        public abstract class unitPrice : PX.Data.BQL.BqlDecimal.Field<unitPrice> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Unit Price", Enabled = false)]
        public virtual decimal? UnitPrice { get; set; }
        #endregion
        #region CuryUnitPrice
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitPrice))]
        [PXFormula(typeof(Default<FSAppointmentDet.contractRelated>))]
        [PXUIField(DisplayName = "Unit Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryUnitPrice { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region EstimatedTranAmt
        public abstract class estimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<estimatedTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Amount", Enabled = false)]
        public virtual decimal? EstimatedTranAmt { get; set; }
        #endregion
        #region CuryEstimatedTranAmt
        public abstract class curyEstimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEstimatedTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<lineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0>,
                                Mult<curyUnitPrice, estimatedQty>>),
                        typeof(SumCalc<FSAppointment.curyEstimatedLineTotal>))]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
        public virtual decimal? CuryEstimatedTranAmt { get; set; }
        #endregion

        #region ActualDuration
        public abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Actual Duration")]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Service>,
                                            And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>>>,
                                        FSAppointmentDet.actualDuration>,
                                    SharedClasses.int_0>),
                          typeof(SumCalc<FSAppointment.actualDurationTotal>))]
        [PXDefault(typeof(Switch<
                                Case<Where<
                                        FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Service>,
                                        And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                        And<
                                            Where<
                                                Current<FSAppointment.status>, Equal<FSAppointment.status.InProcess>,
                                                Or<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>>>>>>,
                                    FSAppointmentDet.estimatedDuration>,
                                SharedClasses.int_0>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? ActualDuration { get; set; }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        [PXDBQuantity]
        [PXDefault(typeof(Switch<
                                Case<Where<
                                        FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                        And<
                                            Where<
                                                Current<FSAppointment.status>, Equal<FSAppointment.status.InProcess>,
                                                Or<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>>>>>,
                                    FSAppointmentDet.estimatedQty>,
                                SharedClasses.decimal_0>))]
        [PXFormula(typeof(Default<status>))]
        [PXUIField(DisplayName = "Actual Quantity")]
        public virtual decimal? Qty { get; set; }
        #endregion

        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ TODO: check the usage and event names
        #region TranAmt
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Actual Amount", Enabled = false)]
        public virtual decimal? TranAmt { get; set; }
        #endregion
        #region CuryTranAmt
        public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(tranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        lineType, Equal<lineType.Service>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0>,
                                Mult<curyUnitPrice, qty>>),
                        typeof(SumCalc<FSAppointment.curyLineTotal>))]
        [PXUIField(DisplayName = "Actual Amount", Enabled = false)]
        public virtual Decimal? CuryTranAmt { get; set; }
        #endregion

        #region BillableTranAmt
        public abstract class billableTranAmt : PX.Data.BQL.BqlDecimal.Field<billableTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public virtual Decimal? BillableTranAmt { get; set; }
        #endregion
        #region CuryBillableTranAmt
        public abstract class curyBillableTranAmt : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmt> { }

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
                        typeof(SumCalc<FSAppointment.curyBillableLineTotal>))]
        [PXUIField(DisplayName = "Amount", Enabled = false)]
        public virtual Decimal? CuryBillableTranAmt { get; set; }
        #endregion

        #region PostID
        public abstract class postID : PX.Data.BQL.BqlInt.Field<postID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Post ID")]
        public virtual int? PostID { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt]
        [PXDefault(typeof(FSServiceOrder.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(Visible = false)]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXDBInt]
        [PXDefault(typeof(FSAppointment.dfltProjectTaskID), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task")]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<FSAppointmentDet.projectID>>>))]
        public virtual int? ProjectTaskID { get; set; }
        #endregion
        #region ScheduleDetID
        public abstract class scheduleDetID : PX.Data.BQL.BqlInt.Field<scheduleDetID> { }

        [PXDBInt]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual int? ScheduleDetID { get; set; }
        #endregion

        #region BillableQty
        public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [PXDBQuantity(typeof(FSAppointmentDet.uOM), typeof(FSAppointmentDet.baseBillableQty))]
        [PXFormula(typeof(Default<FSAppointmentDet.isBillable, FSAppointmentDet.contractRelated>))]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<
                                        FSAppointmentDet.isPrepaid, Equal<True>>,
                                    SharedClasses.decimal_0,
                                Case<
                                    Where<
                                        FSAppointmentDet.contractRelated, Equal<True>>,
                                    FSAppointmentDet.extraUsageQty,
                                Case<
                                    Where2<
                                        Where<
                                            Current<FSAppointment.status>, Equal<FSAppointment.status.AutomaticScheduled>,
                                            Or<Current<FSAppointment.status>, Equal<FSAppointment.status.ManualScheduled>>>,
                                        And<contractRelated, Equal<False>>>,
                                    FSAppointmentDet.estimatedQty>>>,
                            FSAppointmentDet.qty>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
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

        #region SMEquipmentID
        public abstract class SMequipmentID : PX.Data.BQL.BqlInt.Field<SMequipmentID> { }

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
        public virtual int? SMEquipmentID { get; set; }
        #endregion
        #region ActualDateTimeBegin
        public abstract class actualDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBegin> { }

        protected DateTime? _ActualDateTimeBegin;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Start Time")]
        public virtual DateTime? ActualDateTimeBegin
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
        public abstract class actualDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEnd> { }

        protected DateTime? _ActualDateTimeEnd;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual End Time")]
        public virtual DateTime? ActualDateTimeEnd
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
        public abstract class sourceSalesOrderRefNbr : PX.Data.BQL.BqlString.Field<sourceSalesOrderRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Sales Order Ref. Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<FSAppointmentDet.sourceSalesOrderType>>>>))]
        public virtual string SourceSalesOrderRefNbr { get; set; }
        #endregion
        #region SourceSalesOrderType
        public abstract class sourceSalesOrderType : PX.Data.BQL.BqlString.Field<sourceSalesOrderType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Source Type", Enabled = false, Visible = false)]
        public virtual string SourceSalesOrderType { get; set; }
        #endregion
        #region StaffID
        public abstract class staffID : PX.Data.BQL.BqlInt.Field<staffID> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member ID")]
        public virtual int? StaffID { get; set; }
        #endregion
        #region StaffActualDuration
        public abstract class staffActualDuration : PX.Data.BQL.BqlInt.Field<staffActualDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Staff Actual Duration")]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? StaffActualDuration { get; set; }
        #endregion
        #region EnableStaffID
        public abstract class enableStaffID : PX.Data.BQL.BqlBool.Field<enableStaffID> { }

        [PXBool]
        public virtual bool? EnableStaffID { get; set; }
        #endregion
        #region StaffRelated
        public abstract class staffRelated : PX.Data.BQL.BqlBool.Field<staffRelated> { }

        [PXBool]
        public virtual bool? StaffRelated { get; set; }
        #endregion
        #region PriceType
        public abstract class priceType : ListField_PriceType
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXDefault(ID.PriceType.BASE, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price Type", Enabled = false)]
        [priceType.ListAtrribute]
        public virtual string PriceType { get; set; }
        #endregion
        #region PriceCode
        public abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }

        [PXDBString(30, InputMask = ">aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Price Code", Enabled = false)]
        public virtual string PriceCode { get; set; }
        #endregion

        #region AcctID
        public abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        [PXFormula(typeof(Default<FSAppointmentDet.inventoryID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false)]
        public virtual int? AcctID { get; set; }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [PXFormula(typeof(Default<FSAppointmentDet.acctID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(FSAppointmentDet.acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
        public virtual int? SubID { get; set; }
        #endregion
        #region KeepActualDateTimes
        public abstract class keepActualDateTimes : PX.Data.BQL.BqlBool.Field<keepActualDateTimes> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manually Handle Time")]
        public virtual bool? KeepActualDateTimes { get; set; }
        #endregion

        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        public virtual int? ScheduleID { get; set; }
        #endregion

        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), typeof(acctID), typeof(projectTaskID))]
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

        #region Mem_SODetRow
        public virtual FSSODet Mem_SODetRow { get; set; }
        #endregion

        #region UTC Fields
        #region ActualDateTimeBeginUTC
        public abstract class actualDateTimeBeginUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBeginUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXUIField(DisplayName = "Actual Start Time")]
        public virtual DateTime? ActualDateTimeBeginUTC { get; set; }
        #endregion
        #region ActualDateTimeEndUTC
        public abstract class actualDateTimeEndUTC : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEndUTC> { }

        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXUIField(DisplayName = "Actual End Time")]
        public virtual DateTime? ActualDateTimeEndUTC { get; set; }
        #endregion
        #endregion

        #region AuxEstimatedDuration
        public abstract class auxEstimatedDuration : PX.Data.BQL.BqlInt.Field<auxEstimatedDuration> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<lineType, Equal<lineType.Service>>, estimatedDuration>,
                            SharedClasses.int_0>))]
        public virtual int? AuxEstimatedDuration { get; set; }
        #endregion
        #region AuxActualDuration
        public abstract class auxActualDuration : PX.Data.BQL.BqlInt.Field<auxActualDuration> { }

        [PXInt]
        [PXFormula(typeof(Switch<
                            Case<Where<lineType, Equal<lineType.Service>>, actualDuration>,
                            SharedClasses.int_0>))]
        public virtual int? AuxActualDuration { get; set; }
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
        [PXUIField(DisplayName = "Inventory ID", Enabled = false)]
        public virtual int? InventoryIDReport { get; set; }
        #endregion
        #region StaffRelatedCount
        public abstract class staffRelatedCount : PX.Data.BQL.BqlInt.Field<staffRelatedCount>
		{
        }

        [PXInt]
        public virtual int? StaffRelatedCount { get; set; }
        #endregion

        #region Equipment

        #region NewTargetEquipmentLineNbr
        public abstract class newTargetEquipmentLineNbr : PX.Data.BQL.BqlString.Field<newTargetEquipmentLineNbr> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Model Equipment Line Nbr.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region SuspendedTargetEquipmentID
        public abstract class suspendedTargetEquipmentID : PX.Data.BQL.BqlInt.Field<suspendedTargetEquipmentID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Suspended Target Equipment ID", Enabled = false, Visible = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? SuspendedTargetEquipmentID { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region EquipmentLineRef
        public abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Line Ref.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual int? EquipmentLineRef { get; set; }
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
        #region Warranty
        public abstract class warranty : PX.Data.BQL.BqlBool.Field<warranty> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Warranty", Enabled = false, FieldClass = FSSetup.EquipmentManagementFieldClass)]
        public virtual bool? Warranty { get; set; }
        #endregion
        #region SONewTargetEquipmentLineNbr
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
        #endregion

        #region Tax Fields
        #region TaxCategoryID
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
            Where<InventoryItem.inventoryID, Equal<Current<FSAppointmentDet.inventoryID>>>>),
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

        #region ContractRelated
        public abstract class contractRelated : PX.Data.BQL.BqlBool.Field<contractRelated> { }

        [PXDBBool]
        [PXFormula(typeof(Default<FSAppointmentDet.billingRule, FSAppointmentDet.SMequipmentID, FSAppointmentDet.qty, FSAppointmentDet.inventoryID>))]
        [PXFormula(typeof(Default<FSAppointmentDet.estimatedQty>))]
        [PXUIField(DisplayName = "Service Contract Item", IsReadOnly = true, FieldClass = "FSCONTRACT")]
        public virtual bool? ContractRelated { get; set; }
        #endregion
        #region CoveredQty 
        public abstract class coveredQty : PX.Data.BQL.BqlDecimal.Field<coveredQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<FSAppointmentDet.contractRelated, FSAppointmentDet.estimatedQty, FSAppointmentDet.qty>))]
        [PXUIField(DisplayName = "Covered Quantity", IsReadOnly = true, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual decimal? CoveredQty { get; set; }
        #endregion
        #region ExtraUsageQty  
        public abstract class extraUsageQty : PX.Data.BQL.BqlDecimal.Field<extraUsageQty> { }

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
        [PXFormula(typeof(Default<FSAppointmentDet.contractRelated>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overage Unit Price", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual Decimal? CuryExtraUsageUnitPrice { get; set; }
        #endregion
        #endregion

        #region CuryUnitCost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryUnitCost { get; set; }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ExtCost { get; set; }
        #endregion
        #region EnablePurchaseOrder
        public abstract class enablePO : PX.Data.BQL.BqlBool.Field<enablePO> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(Visible = false, Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? EnablePO { get; set; }
        #endregion

        #region Methods
        public virtual bool needToBePosted()
        {
            return (LineType == ID.LineType_All.SERVICE
                        || LineType == ID.LineType_All.NONSTOCKITEM
                        || LineType == ID.LineType_All.INVENTORY_ITEM)
                    && IsPrepaid == false
                    && Status != ID.Status_AppointmentDet.CANCELED;
        }
        #endregion

        #region IDocLine unbound properties
        public int? DocID
        {
            get
            {
                return this.AppointmentID;
            }
        }

        public int? LineID
        {
            get
            {
                return this.AppDetID;
            }
        }

        public int? PostAppointmentID
        {
            get
            {
                return this.AppointmentID;
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
                return this.AppDetID;
            }
        }

        public string BillingBy
        {
            get
            {
                return ID.Billing_By.APPOINTMENT;
            }
        }

        public string SourceTable
        {
            get
            {
                return ID.TablePostSource.FSAPPOINTMENT_DET;
            }
        }

        public decimal? OverageItemPrice
        {
            get
            {
                return CuryExtraUsageUnitPrice;
            }

            set
            {
                CuryExtraUsageUnitPrice = value;
            }
        }
        #endregion

        #region Methods
        public int? GetPrimaryDACDuration()
        {
            return ActualDuration;
        }

        public decimal? GetPrimaryDACQty()
        {
            return Qty;
        }

        public decimal? GetPrimaryDACTranAmt()
        {
            return TranAmt;
        }

        public int? GetDuration(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return EstimatedDuration;
            }
            else if(fieldType == FieldType.ActualField)
            {
                return ActualDuration;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public decimal? GetQty(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return EstimatedQty;
            }
            else if (fieldType == FieldType.ActualField)
            {
                return Qty;
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

        public decimal? GetBaseQty(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                throw new InvalidOperationException();
            }
            else if (fieldType == FieldType.ActualField)
            {
                throw new InvalidOperationException();
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
            else if (fieldType == FieldType.ActualField)
            {
                return CuryTranAmt;
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
            else if (fieldType == FieldType.ActualField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<actualDuration>(this, duration);
                }
                else
                {
                    ActualDuration = duration;
                }
            }
            else if (fieldType == FieldType.BillableField)
            {
                //TODO: review all the usage of BillableFields
                if (raiseEvents == true)
                {
                    cache.SetValueExt<actualDuration>(this, duration);
                }
                else
                {
                    ActualDuration = duration;
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
            else if (fieldType == FieldType.ActualField)
            {
                if (raiseEvents == true)
                {
                    cache.SetValueExt<qty>(this, qty);
                }
                else
                {
                    Qty = qty;
                }
            }
            else if (fieldType == FieldType.BillableField)
            {
                //TODO: review all the usage of BillableFields
                if (raiseEvents == true)
                {
                    cache.SetValueExt<qty>(this, qty);
                }
                else
                {
                    Qty = qty;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool IsService
        {
            get
            {
                return LineType == ID.LineType_All.SERVICE || LineType == ID.LineType_All.NONSTOCKITEM;
            }
        }
        #endregion
    }
}
