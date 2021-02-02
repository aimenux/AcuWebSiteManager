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
    public class FSDBTimeSpanLongAttribute : PXDBTimeSpanLongAttribute
    {
        public FSDBTimeSpanLongAttribute()
        {
            Format = TimeSpanFormatType.LongHoursMinutes;
        }

        public override void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
        {
            if (e.Operation.Command() == PXDBOperation.Select &&
                   (e.Operation.Option() == PXDBOperation.External) &&
                   (e.Value == null || e.Value is string))
            {
                return;
            }
            base.CommandPreparing(sender, e);
        }
    }

    [Serializable]
    [PXCacheName(TX.TableName.APPOINTMENTDET)]
    public class FSAppointmentDet : IBqlTable, IFSSODetBase, IDocLine, ISortOrder, ILSPrimary
    {
        #region Keys
        public class PK : PrimaryKeyOf<FSAppointmentDet>.By<srvOrdType, refNbr, lineNbr>
        {
            public static FSAppointmentDet Find(PXGraph graph, string srvOrdType, string refNbr, int? lineNbr) => FindBy(graph, srvOrdType, refNbr, lineNbr);
        }

        public static class FK
        {
            public class Appointment : FSAppointment.PK.ForeignKeyOf<FSAppointmentDet>.By<srvOrdType, refNbr> { }
            public class ServiceOrder : FSServiceOrder.PK.ForeignKeyOf<FSAppointmentDet>.By<srvOrdType, origSrvOrdNbr> { }
            public class SrvOrdLine : FSSODet.PK.ForeignKeyOf<FSAppointmentDet>.By<srvOrdType, origSrvOrdNbr, origLineNbr> { }
        }
        #endregion

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
                         Where<
                             FSAppointment.srvOrdType, Equal<Current<FSAppointmentDet.srvOrdType>>,
                         And<
                             FSAppointment.refNbr, Equal<Current<FSAppointmentDet.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region AppDetID
        public abstract class appDetID : PX.Data.BQL.BqlInt.Field<appDetID> { }

        [PXDBIdentity]
        public virtual int? AppDetID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment.lineCntr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXFormula(null, typeof(MaxCalc<FSAppointment.maxLineNbr>))]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Sort Order", Visible = false, Enabled = false)]
        public virtual Int32? SortOrder { get; set; }
        #endregion

        #region SODetID
        public abstract class sODetID : PX.Data.BQL.BqlInt.Field<sODetID> { }

        [PXDBInt]
        [PXCheckUnique(Where = typeof(Where<appointmentID, Equal<Current<FSAppointment.appointmentID>>>))]
        [PXUIField(DisplayName = "Service Order Details Ref. Nbr.", Visible = false)]
        [FSSelectorSODetID(ValidateValue = false)]
        public virtual int? SODetID { get; set; }
        #endregion

        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion

        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [Branch(typeof(FSAppointment.branchID))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSAppointment.curyInfoID))]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion

        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType>
        {
        }

        [PXDBString(5, IsFixed = true)]
        [PXUIField(DisplayName = "Line Type")]
        [FSLineType.List]
        [PXDefault]
        public virtual string LineType { get; set; }
        #endregion
        #region IsPrepaid
        public abstract class isPrepaid : PX.Data.BQL.BqlBool.Field<isPrepaid> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Prepaid Item", Enabled = false, Visible = false)]
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

        #region PickupDeliveryAppLineRef
        public abstract class pickupDeliveryAppLineRef : PX.Data.BQL.BqlInt.Field<pickupDeliveryAppLineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Pickup/Delivery Ref. Nbr.", FieldClass = FSSetup.RouteManagementFieldClass)]
        [FSSelectorServiceInAppointment]
        [PXFormula(typeof(Default<lineType>))]
        public virtual string PickupDeliveryAppLineRef { get; set; }
        #endregion
        #region PickupDeliveryServiceID
        public abstract class pickupDeliveryServiceID : PX.Data.BQL.BqlInt.Field<pickupDeliveryServiceID> { }

        [Service(Enabled = false)]
        [PXUIField(DisplayName = "Pickup/Delivery Service ID", Visible = false, FieldClass = FSSetup.RouteManagementFieldClass)]
        [PXFormula(typeof(Selector<pickupDeliveryAppLineRef, FSAppointmentDet.inventoryID>))]
        public virtual int? PickupDeliveryServiceID { get; set; }
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
        [PXRestrictor(typeof(
                        Where<
                            InventoryItem.stkItem, Equal<False>,
                            Or<Current<FSSrvOrdType.postToSOSIPM>, Equal<True>,
                            Or<Current<lineType>, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>>>>),
                TX.Error.STKITEM_CANNOT_BE_HANDLED_WITH_SRVORDTYPE)]
        public virtual int? InventoryID { get; set; }
        #endregion

        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        [SubItem(typeof(inventoryID), DisplayName = "Subitem")]
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
                          Where<
                              InventoryItem.inventoryID, Equal<Current<inventoryID>>,
                          And<
                              InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
                    PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItemStatusVeryfier(typeof(inventoryID), typeof(siteID), InventoryItemStatus.Inactive, InventoryItemStatus.NoSales)]
        public virtual int? SubItemID { get; set; }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;

        [PXDefault(typeof(Search<InventoryItem.salesUnit,
                          Where<
                              InventoryItem.inventoryID, Equal<Current<inventoryID>>>>),
                   PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
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
        [PXDefault(ID.BillingRule.NONE, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Rule", Enabled = false)]
        public virtual string BillingRule { get; set; }
        #endregion
        #region ServiceType
        public abstract class serviceType : ListField_Appointment_Service_Action_Type
        {
        }

        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.routeManagementModule>))]
        [PXDBString(1, IsFixed = true)]
        [serviceType.List]
        [PXDefault(ID.Service_Action_Type.NO_ITEMS_RELATED, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Selector<pickupDeliveryAppLineRef, serviceType>))]
        [PXUIField(DisplayName = "Pickup/Delivery Action", Enabled = false, Visible = false, FieldClass = FSSetup.RouteManagementFieldClass)]
        public virtual string ServiceType { get; set; }
        #endregion

        // SiteID and SiteLocationID should be together in the same DAC in order to LocationAvail attribute works.
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        [FSSiteAvail(typeof(inventoryID), typeof(subItemID), DisplayName = "Warehouse")]
        public virtual int? SiteID { get; set; }
        #endregion

        #region SiteLocationID
        public abstract class siteLocationID : PX.Data.BQL.BqlInt.Field<siteLocationID> { }

        [PXFormula(typeof(Default<inventoryID, subItemID, siteID>))]
        [LocationAvail(typeof(inventoryID), typeof(subItemID), typeof(siteID), true, false, false)]
        public virtual int? SiteLocationID { get; set; }
        #endregion

        #region IsTravelItem
        public abstract class isTravelItem : Data.BQL.BqlBool.Field<isTravelItem> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Is a Travel Item", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXDefault(false, typeof(Search<FSxService.isTravelItem, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual bool? IsTravelItem { get; set; }
        #endregion
        #region SOLineType
        public abstract class sOLineType : PX.Data.BQL.BqlString.Field<sOLineType> { }

        [PXString(2, IsFixed = true)]
        [SOLineType.List()]
        [PXUIField(DisplayName = "SO Line Type", Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSAppointmentDet.inventoryID, Switch<
            Case<Where<InventoryItem.stkItem, Equal<True>, Or<InventoryItem.kitItem, Equal<True>>>, SOLineType.inventory,
            Case<Where<InventoryItem.nonStockShip, Equal<True>>, SOLineType.nonInventory>>,
            SOLineType.miscCharge>>))]
        public virtual string SOLineType { get; set; }
        #endregion

        #region Status
        public abstract class status : ListField_Status_AppointmentDet
        {
        }

        public string _Status;

        [PXDBString(2, IsFixed = true)]
        [status.List]
        [PXDefault(typeof(Switch<
                            Case<Where<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>,
                                And<isTravelItem, Equal<False>>>,
                            status.Completed>,
                        status.NotStarted>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            status, Equal<ListField_Status_AppointmentDet.waitingForPO>>,
                                        int1>,
                                    int0>),
                            typeof(SumCalc<FSAppointment.pendingPOLineCntr>))]
        [PXUIField(DisplayName = "Line Status", Enabled = false)]
        public virtual string Status 
        { 
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                UIStatus = value;
            }
        }
        #endregion
        #region UIStatus
        public abstract class uiStatus : PX.Data.BQL.BqlString.Field<uiStatus>
        {
            public abstract class Values : ListField_Status_AppointmentDet { }
        }

        protected string _UIStatus;

        [PXString(2, IsFixed = true)]
        [status.List]
        [PXUIField(DisplayName = "Line Status")]
        public virtual string UIStatus
        {
            get
            {
                return _UIStatus;
            }
            set
            {
                _UIStatus = value;
            }
        }
        #endregion

        #region IsCanceledNotPerformed
        public abstract class isCanceledNotPerformed : PX.Data.BQL.BqlBool.Field<isCanceledNotPerformed> { }

        [PXBool]
        [PXDBCalced(typeof(IIf<Where<status, Equal<status.Canceled>, 
                                    Or<status, Equal<status.NotPerformed>, 
                                    Or<status, Equal<status.requestForPO>>>>,
                                    True, False>), typeof(bool))]
        [PXDefault(typeof(IIf<Where<status, Equal<status.Canceled>,
                                    Or<status, Equal<status.NotPerformed>,
                                    Or<status, Equal<status.requestForPO>>>>,
                                    True, False>))]
        public virtual bool? IsCanceledNotPerformed { get; set; }
        #endregion

        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;

        [PXDBString(INLotSerialStatus.lotSerialNbr.LENGTH, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
        [PXDefault("", PersistingCheck = PXPersistingCheck.Nothing)]
        [FSINLotSerialNbr(typeof(FSAppointmentDet.siteID), typeof(FSAppointmentDet.inventoryID), typeof(FSAppointmentDet.subItemID), typeof(FSAppointmentDet.siteLocationID), typeof(FSAppointmentDet.sODetID))]
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

        [FSDBTimeSpanLong]
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
                                    //default case
                                    SharedClasses.int_0>),
                          typeof(SumCalc<FSAppointment.estimatedDurationTotal>))]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? EstimatedDuration { get; set; }
        #endregion
        #region EstimatedQty
        public abstract class estimatedQty : PX.Data.BQL.BqlDecimal.Field<estimatedQty> { }

        [FSDBQuantity(typeof(uOM), typeof(baseEstimatedQty))]
        [PXUIField(DisplayName = "Estimated Quantity")]
        [PXFormula(typeof(Default<sODetID>))]
        public virtual decimal? EstimatedQty { get; set; }
        #endregion
        #region BaseEstimatedQty
        public abstract class baseEstimatedQty : PX.Data.BQL.BqlDecimal.Field<baseEstimatedQty> { }
        protected Decimal? _BaseEstimatedQty;
        [PXDBDecimal(6)]
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
        [PXDBDefault(typeof(FSAppointment.effDocDate))]
        [PXUIField(DisplayName = "Transaction Date")]
        public virtual DateTime? TranDate { get; set; }
        #endregion

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
        [PXUIField(DisplayName = "Unit Price")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? CuryUnitPrice { get; set; }
        #endregion
        #region EstimatedTranAmt
        public abstract class estimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<estimatedTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Estimated Amount", Enabled = false, Visible = false)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<
                                        Where<
                                            isFree, Equal<False>>,
                                        estimatedTranAmt>,
                                        //default case
                                        SharedClasses.decimal_0>),
                          typeof(SumCalc<FSAppointment.estimatedLineTotal>))]
        public virtual decimal? EstimatedTranAmt { get; set; }
        #endregion
        #region CuryEstimatedTranAmt
        public abstract class curyEstimatedTranAmt : PX.Data.BQL.BqlDecimal.Field<curyEstimatedTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<Where<isCanceledNotPerformed, Equal<True>>,
                                    decimal0,
                                Case<Where2<
                                        Where<lineType, Equal<FSLineType.Service>,
                                            Or<lineType, Equal<FSLineType.Inventory_Item>>>,
                                        And<billingRule, Equal<billingRule.None>>>,
                                    decimal0>>,
                                Mult<curyUnitPrice, estimatedQty>>),
                        typeof(SumCalc<FSAppointment.curyEstimatedLineTotal>))]
        [PXUIField(DisplayName = "Estimated Amount", Enabled = false)]
        public virtual decimal? CuryEstimatedTranAmt { get; set; }
        #endregion
        #region LogActualDuration
        public abstract class logActualDuration : PX.Data.BQL.BqlInt.Field<logActualDuration> { }

        private int? _LogActualDuration;

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Log Actual Duration", Enabled = false)]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? LogActualDuration
        {
            get
            {
                return _LogActualDuration;
            }
            set
            {
                _LogActualDuration = value;
            }
        }
        #endregion
        #region AreActualFieldsActive
        public abstract class areActualFieldsActive : PX.Data.BQL.BqlBool.Field<areActualFieldsActive> { }

        [PXBool]
        [PXUnboundDefault(typeof(Switch<
                            Case<Where<
                                    isCanceledNotPerformed, Equal<True>>,
                                False,
                            Case<Where<
                                    Current<FSAppointment.status>, Equal<FSAppointment.status.InProcess>,
                                    Or<Current<FSAppointment.status>, Equal<FSAppointment.status.Paused>,
                                    Or<Current<FSAppointment.status>, Equal<FSAppointment.status.Completed>,
                                    Or<isTravelItem, Equal<True>>>>>,
                                True>>,
                            False>))]
        [PXFormula(typeof(Default<isCanceledNotPerformed, isTravelItem>))]
        public virtual bool? AreActualFieldsActive { get; set; }
        #endregion
        #region ActualDuration
        public abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

        private int? _ActualDuration;

        [FSDBTimeSpanLong]
        [PXUIField(DisplayName = "Actual Duration")]
        [PXDefault(typeof(Switch<
                            Case<Where<
                                    lineType, NotEqual<ListField_LineType_ALL.Service>,
                                    And<lineType, NotEqual<ListField_LineType_ALL.NonStockItem>>>,
                                int0,
                            Case<Where<areActualFieldsActive, Equal<True>>,
                                IIf<Where<logRelatedCount, Greater<int0>>, logActualDuration, estimatedDuration>>>,
                            int0>))]
        [PXFormula(typeof(Default<lineType, areActualFieldsActive, logRelatedCount, logActualDuration>))]
        [PXUnboundFormula(typeof(Switch<
                                    Case<Where<
                                            lineType, Equal<FSLineType.Service>,
                                            And<isTravelItem, Equal<False>>>,
                                        actualDuration>,
                                    int0>),
                          typeof(SumCalc<FSAppointment.actualDurationTotal>))]
        public virtual int? ActualDuration
        {
            get
            {
                return _ActualDuration;
            }
            set
            {
                _ActualDuration = value;
            }
        }
        #endregion
        #region ActualQty
        public abstract class actualQty : PX.Data.BQL.BqlDecimal.Field<actualQty> { }

        [FSDBQuantity(typeof(uOM), typeof(baseActualQty))]
        [PXDefault(typeof(Switch<
                            Case<Where<
                                    lineType, NotEqual<ListField_LineType_ALL.Service>,
                                    And<lineType, NotEqual<ListField_LineType_ALL.NonStockItem>,
                                    And<lineType, NotEqual<ListField_LineType_ALL.Inventory_Item>>>>,
                                decimal0,
                            Case<Where<
                                    billingRule, Equal<billingRule.Time>>,
                                actualQty /* this is to avoid overwriting the value calculated by ActualDuration */,
                            Case<Where<areActualFieldsActive, Equal<True>>,
                                estimatedQty>>>,
                            decimal0>))]
        [PXFormula(typeof(Default<lineType, billingRule, areActualFieldsActive>))]
        [PXUIField(DisplayName = "Actual Quantity")]
        public virtual decimal? ActualQty { get; set; }
        #endregion
        #region BaseActualQty
        public abstract class baseActualQty : PX.Data.BQL.BqlDecimal.Field<baseActualQty> { }
        protected Decimal? _BaseActualQty;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Actual Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseActualQty
        {
            get
            {
                return this._BaseActualQty;
            }
            set
            {
                this._BaseActualQty = value;
            }
        }
        #endregion

        #region TranAmt
        public abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Actual Amount", Enabled = false)]
        [PXUnboundFormula(typeof(Switch<
                                    Case<
                                        Where<isFree, Equal<False>,
                                        And<isCanceledNotPerformed, NotEqual<True>>>,
                                        FSAppointmentDet.tranAmt>,
                                    //default case
                                    SharedClasses.decimal_0>),
                          typeof(SumCalc<FSAppointment.lineTotal>))]
        public virtual decimal? TranAmt { get; set; }
        #endregion
        #region CuryTranAmt
        public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(tranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where2<
                                        Where<lineType, Equal<FSLineType.Service>,
                                        Or<lineType, Equal<FSLineType.Inventory_Item>>>,
                                    And<billingRule, Equal<billingRule.None>>>,
                                    SharedClasses.decimal_0>,
                                //default case
                                Mult<curyUnitPrice, actualQty>>),
                        typeof(SumCalc<FSAppointment.curyLineTotal>))]
        [PXUIField(DisplayName = "Actual Amount", Enabled = false)]
        public virtual Decimal? CuryTranAmt { get; set; }
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

        #region EffTranQty
        public abstract class effTranQty : PX.Data.BQL.BqlDecimal.Field<effTranQty> { }
        protected Decimal? _EffTranQty;
        [FSDBQuantity(typeof(FSAppointmentDet.uOM), typeof(FSAppointmentDet.baseEffTranQty), InventoryUnitType.SalesUnit)]
        [PXUIField(DisplayName = "Transaction Qty.", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(IIf<Where<areActualFieldsActive, Equal<True>>, actualQty, estimatedQty>))]
        public virtual Decimal? EffTranQty
        {
            get
            {
                return this._EffTranQty;
            }
            set
            {
                this._EffTranQty = value;
            }
        }

        public virtual Decimal? Qty
        {
            get
            {
                return this._EffTranQty;
            }
            set
            {
                this._EffTranQty = value;
            }
        }
        #endregion
        #region BaseEffTranQty
        public abstract class baseEffTranQty : PX.Data.BQL.BqlDecimal.Field<baseEffTranQty> { }
        protected Decimal? _BaseEffTranQty;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Transaction Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseEffTranQty
        {
            get
            {
                return this._BaseEffTranQty;
            }
            set
            {
                this._BaseEffTranQty = value;
            }
        }

        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseEffTranQty;
            }
            set
            {
                this._BaseEffTranQty = value;
            }
        }
        #endregion

        #region IsBillable
        public abstract class isBillable : PX.Data.BQL.BqlBool.Field<isBillable> { }

        [PXDBBool]
        [PXDefault(typeof(IIf<Where<lineType, Equal<FSLineType.Comment>,
                                 Or<lineType, Equal<FSLineType.Instruction>,
                                 Or<isCanceledNotPerformed, Equal<True>,
                                 Or<isPrepaid, Equal<True>>>>>, False, True>))]
        [PXUIField(DisplayName = "Billable")]
        [PXFormula(typeof(Default<isPrepaid, isCanceledNotPerformed>))]
        public virtual bool? IsBillable { get; set; }
        #endregion
        #region IsFree
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }

        [PXDBBool]
        [PXDefault(typeof(IIf<Where<lineType, Equal<FSLineType.Comment>,
                                 Or<lineType, Equal<FSLineType.Instruction>,
                                 Or<billingRule, Equal<billingRule.None>>>>, True, False>))]
        [PXUIField(DisplayName = "Free Item")]
        public virtual bool? IsFree { get; set; }
        #endregion
        #region BillableQty
        public abstract class billableQty : PX.Data.BQL.BqlDecimal.Field<billableQty> { }

        [FSDBQuantity(typeof(uOM), typeof(baseBillableQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<Where<isPrepaid, Equal<True>,
                                        Or<isBillable, Equal<False>,
                                        Or<isCanceledNotPerformed, Equal<True>>>>,
                                    decimal0,
                                Case<Where<contractRelated, Equal<True>>,
                                    extraUsageQty,
                                Case<Where<areActualFieldsActive, Equal<True>>,
                                    actualQty>>>,
                                estimatedQty>))]
        [PXUIField(DisplayName = "Billable Quantity", Enabled = false)]
        public virtual decimal? BillableQty { get; set; }
        #endregion
        #region BaseBillableQty
        public abstract class baseBillableQty : PX.Data.BQL.BqlDecimal.Field<baseBillableQty> { }

        [PXDBDecimal(6, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? BaseBillableQty { get; set; }
        #endregion
        #region CuryBillableExtPrice
        public abstract class curyBillableExtPrice : PX.Data.BQL.BqlDecimal.Field<curyBillableExtPrice> { }

        private decimal? _CuryBillableExtPrice;

        [PXDBCurrency(typeof(curyInfoID), typeof(billableExtPrice))]
        [PXUIField(DisplayName = "Ext. Price")]
        [PXUIEnabled(typeof(Where<isCanceledNotPerformed, Equal<False>>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<Where<isFree, Equal<True>>,
                                    IsNull<curyBillableExtPrice, decimal0>,
                                Case<Where<contractRelated, Equal<False>,
                                        And<billingRule, Equal<billingRule.None>,
                                        And<
                                            Where<lineType, Equal<FSLineType.Service>,
                                            Or<lineType, Equal<FSLineType.Inventory_Item>,
                                            Or<lineType, Equal<ListField_LineType_Pickup_Delivery.Pickup_Delivery>>>>>>>,
                                    decimal0,
                                Case<Where<contractRelated, Equal<True>>,
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
        #region BillableExtPrice
        public abstract class billableExtPrice : PX.Data.BQL.BqlDecimal.Field<billableExtPrice> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BillableExtPrice { get; set; }
        #endregion

        #region Discount Fields
        #region ManualDisc
        public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }

        [ManualDiscountMode(typeof(curyDiscAmt), typeof(discPct), DiscountFeatureType.CustomerDiscount)]
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Manual Discount", Visibility = PXUIVisibility.Visible)]
        public virtual Boolean? ManualDisc { get; set; }
        #endregion
        #region DiscPct
        public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }

        [PXUIEnabled(typeof(Where<status, NotEqual<status.NotPerformed>>))]
        [PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
        [PXUIField(DisplayName = "Discount Percent")]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<inventoryID>))]
        public virtual Decimal? DiscPct { get; set; }
        #endregion
        #region CuryDiscAmt
        public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }

        [PXUIEnabled(typeof(Where<status, NotEqual<status.NotPerformed>>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBCurrency(typeof(CommonSetup.decPlPrcCst), typeof(curyInfoID), typeof(discAmt))]
        [PXFormula(typeof(Div<Mult<curyBillableExtPrice, discPct>, decimal100>))]
        [PXUIField(DisplayName = "Discount Amount")]
        public virtual Decimal? CuryDiscAmt { get; set; }
        #endregion
        #region DiscAmt
        public abstract class discAmt : PX.Data.BQL.BqlDecimal.Field<discAmt> { }

        [PXDBDecimal(4)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? DiscAmt { get; set; }
        #endregion
        #region DiscountsAppliedToLine
        public abstract class discountsAppliedToLine : PX.Data.BQL.BqlByteArray.Field<discountsAppliedToLine> { }

        [PXDBPackedIntegerArray()]
        public virtual ushort[] DiscountsAppliedToLine { get; set; }
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

        #region CuryLineAmt
        public abstract class curyLineAmt : PX.Data.BQL.BqlDecimal.Field<curyLineAmt> { }

        [PXDecimal]
        [PXUnboundDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Amount", Enabled = false)]

        public virtual Decimal? CuryLineAmt
        {
            get { return CuryBillableTranAmt; }
        }
        #endregion

        #region CuryBillableTranAmt
        public abstract class curyBillableTranAmt : PX.Data.BQL.BqlDecimal.Field<curyBillableTranAmt> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(billableTranAmt))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<Where<isPrepaid, Equal<True>,
                                        Or<contractRelated, Equal<True>>>,
                                    curyBillableExtPrice>,
                                Sub<curyBillableExtPrice, curyDiscAmt>>),
                            typeof(SumCalc<FSAppointment.curyBillableLineTotal>))]
        [PXUIField(DisplayName = "Billable Amount", Enabled = false)]
        public virtual Decimal? CuryBillableTranAmt { get; set; }
        #endregion
        #region BillableTranAmt
        public abstract class billableTranAmt : PX.Data.BQL.BqlDecimal.Field<billableTranAmt> { }

        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Billable Amount", Enabled = false)]
        public virtual Decimal? BillableTranAmt { get; set; }
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
        [PXDefault(typeof(FSAppointment.projectID), PersistingCheck = PXPersistingCheck.Nothing)]
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
                                    Current<FSAppointment.dfltProjectTaskID>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Project Task", FieldClass = ProjectAttribute.DimensionName)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(Field<projectTaskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? ProjectTaskID { get; set; }
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
        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.inventory>))]
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
        [PXDefault(typeof(FSAppointment.mem_SMequipmentID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.inventory>))]
        [PXUIField(DisplayName = "Model Equipment Ref. Nbr.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorNewTargetEquipmentAppointment]
        public virtual string NewTargetEquipmentLineNbr { get; set; }
        #endregion
        #region ComponentID
        public abstract class componentID : PX.Data.BQL.BqlInt.Field<componentID> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component ID", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [FSSelectorComponentIDAppointment(typeof(FSAppointmentDet), typeof(FSAppointmentDet))]
        public virtual int? ComponentID { get; set; }
        #endregion
        #region EquipmentLineRef
        public abstract class equipmentLineRef : PX.Data.BQL.BqlInt.Field<equipmentLineRef> { }

        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Component Ref. Nbr.", FieldClass = FSSetup.EquipmentManagementFieldClass)]
        [PXUIEnabled(typeof(Where<Current<isTravelItem>, NotEqual<True>>))]
        [FSSelectorEquipmentLineRefServiceOrderAppointment(typeof(inventoryID), typeof(SMequipmentID), typeof(componentID), typeof(equipmentAction))]
        public virtual int? EquipmentLineRef { get; set; }
        #endregion

        #region StaffID
        public abstract class staffID : PX.Data.BQL.BqlInt.Field<staffID> { }

        [PXDBInt]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member ID")]
        public virtual int? StaffID { get; set; }
        #endregion
        #region LogRelatedCount
        public abstract class logRelatedCount : PX.Data.BQL.BqlInt.Field<logRelatedCount>
        {
        }

        private int? _LogRelatedCount;

        [PXInt]
        public virtual int? LogRelatedCount
        {
            get
            {
                return _LogRelatedCount;
            }
            set
            {
                _LogRelatedCount = value;
            }
        }
        #endregion
        #region StaffRelatedCount
        public abstract class staffRelatedCount : PX.Data.BQL.BqlInt.Field<staffRelatedCount>
        {
        }

        [PXInt]
        public virtual int? StaffRelatedCount { get; set; }
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

        [PXFormula(typeof(Default<inventoryID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Visible = false, AvoidControlAccounts = true)]
        public virtual int? AcctID { get; set; }
        #endregion
        #region SubID
        public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        [PXFormula(typeof(Default<acctID>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(acctID), Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Visible = false)]
        public virtual int? SubID { get; set; }
        #endregion
        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }

        [PXDBInt]
        public virtual int? ScheduleID { get; set; }
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
        [PXFormula(typeof(IIf<
                            Where<inventoryID, IsNotNull>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion
        #region FSSODetRow
        public virtual FSSODet FSSODetRow { get; set; }
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
        [PXUIVisible(typeof(FeatureInstalled<FeaturesSet.inventory>))]
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
        #region Tax Fields
        #region TaxCategoryID
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXDefault(typeof(Search<InventoryItem.taxCategoryID,
                          Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>),
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
        [PXFormula(typeof(Default<billingRule, SMequipmentID, actualQty, inventoryID>))]
        [PXFormula(typeof(Default<estimatedQty>))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Service Contract Item", IsReadOnly = true, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual bool? ContractRelated { get; set; }
        #endregion
        #region CoveredQty 
        public abstract class coveredQty : PX.Data.BQL.BqlDecimal.Field<coveredQty> { }

        [PXDBQuantity]
        [PXFormula(typeof(Default<contractRelated, estimatedQty, actualQty>))]
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
                                    Sub<actualQty, coveredQty>>>,
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
        [PXFormula(typeof(Default<contractRelated>))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Overage Unit Price", Enabled = false, Visible = false, FieldClass = "FSCONTRACT")]
        public virtual Decimal? CuryExtraUsageUnitPrice { get; set; }
        #endregion
        #endregion

        #region CuryUnitCost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(Search<CommonSetup.decPlPrcCst>), typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible, FieldClass = "DISTINV")]
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
        [PXUIField(DisplayName="Ext. Cost", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<linkedEntityType, Equal<linkedEntityType.Values.expenseReceipt>>,
                                    IsNull<curyExtCost, decimal0>,
                                Case<
                                    Where<
                                        isCanceledNotPerformed, NotEqual<True>,
                                    And<
                                        Where<
                                            lineType, Equal<FSLineType.NonStockItem>,
                                            Or<lineType, Equal<FSLineType.Inventory_Item>,
                                            Or<
                                                Where<
                                                    lineType, Equal<FSLineType.Service>,
                                                    And<curyUnitCost, Greater<SharedClasses.decimal_0>>>>>>>>,
                                    Mult<curyUnitCost,  actualQty>>>,
                                SharedClasses.decimal_0>),
                    typeof(SumCalc<FSAppointment.curyCostTotal>))]
        public virtual Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ExtCost { get; set; }
        #endregion
        #region CuryEstimatedExtCost
        public abstract class curyEstimatedExtCost : PX.Data.BQL.BqlDecimal.Field<curyEstimatedExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(estimatedExtCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Switch<
                                Case<
                                    Where<linkedEntityType, Equal<linkedEntityType.Values.expenseReceipt>>,
                                    curyExtCost,
                                Case<
                                    Where<
                                        isCanceledNotPerformed, NotEqual<True>,
                                    And<
                                        Where<
                                            lineType, Equal<FSLineType.NonStockItem>,
                                            Or<lineType, Equal<FSLineType.Inventory_Item>,
                                            Or<
                                                Where<
                                                    lineType, Equal<FSLineType.Service>,
                                                    And<curyUnitCost, Greater<SharedClasses.decimal_0>>>>>>>>,
                                    Mult<curyUnitCost, estimatedQty>>>,
                                SharedClasses.decimal_0>),
                    typeof(SumCalc<FSAppointment.curyEstimatedCostTotal>))]
        public virtual Decimal? CuryEstimatedExtCost { get; set; }
        #endregion
        #region EstimatedExtCost
        public abstract class estimatedExtCost : PX.Data.BQL.BqlDecimal.Field<estimatedExtCost> { }

        [PXDBPriceCost]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? EstimatedExtCost { get; set; }
        #endregion

        #region PO Fields
        #region EnablePO
        public abstract class enablePO : PX.Data.BQL.BqlBool.Field<enablePO> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Mark for PO", Visible = false, FieldClass = "DISTINV")]
        [PXUIEnabled(typeof(Where<lineType, NotEqual<ListField_LineType_ALL.Comment>, And<lineType, NotEqual<ListField_LineType_ALL.Instruction>>>))]
        public virtual bool? EnablePO { get; set; }
        #endregion
        #region CanChangeMarkForPO
        public abstract class canChangeMarkForPO : PX.Data.BQL.BqlBool.Field<canChangeMarkForPO> { }

        [PXBool]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? CanChangeMarkForPO { get; set; }
        #endregion
        #region POCompleted
        public abstract class poCompleted : PX.Data.BQL.BqlBool.Field<poCompleted> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "PO Completed", Visible = false, Enabled = false, FieldClass = "DISTINV")]
        public virtual bool? POCompleted { get; set; }
        #endregion
        #region POType
        public abstract class poType : PX.Data.BQL.BqlString.Field<poType> { }

        [PXDBString(2)]
        [PXUIField(DisplayName = "Order Type", Enabled = false, FieldClass = "DISTINV")]
        public virtual String POType { get; set; }
        #endregion
        #region POSource
        public abstract class pOSource : PX.Data.BQL.BqlString.Field<pOSource>
        {
            public abstract class Values : ListField_FSPOSource { }
        }

        private string _POSource;

        [PXDBString()]
        [PXDefault(typeof(IIf<Where<enablePO, Equal<True>>, pOSource.Values.purchaseToAppointment, Null>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<enablePO>))]
        [pOSource.Values.List]
        [PXUIEnabled(typeof(Where<enablePO, Equal<True>>))]
        [PXUIField(DisplayName = "PO Source", FieldClass = "DISTINV")]
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
        #region POVendorID
        public abstract class poVendorID : PX.Data.BQL.BqlInt.Field<poVendorID> { }

        [VendorNonEmployeeActive(DisplayName = "Vendor ID", DescriptionField = typeof(Vendor.acctName), CacheGlobal = true, Filterable = true, FieldClass = "DISTINV")]
        [PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
                          Where<
                              INItemSiteSettings.inventoryID, Equal<Current<FSAppointmentDet.inventoryID>>,
                          And<
                              INItemSiteSettings.siteID, Equal<Current<FSAppointmentDet.siteID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<enablePO>))]
        [PXUIEnabled(typeof(Where<enablePO, Equal<True>, And<canChangeMarkForPO, Equal<True>>>))]
        public virtual int? POVendorID { get; set; }
        #endregion
        #region POVendorLocationID
        public abstract class poVendorLocationID : PX.Data.BQL.BqlInt.Field<poVendorLocationID> { }

        [PXFormula(typeof(Default<poVendorID>))]
        [PXDefault(typeof(Coalesce<
            Search<INItemSiteSettings.preferredVendorLocationID,
            Where<
                INItemSiteSettings.inventoryID, Equal<Current<inventoryID>>,
            And<
                INItemSiteSettings.preferredVendorID, Equal<Current<poVendorID>>>>>,
            Search2<Vendor.defLocationID,
            InnerJoin<CRLocation,
                On<CRLocation.locationID, Equal<Vendor.defLocationID>,
                And<CRLocation.bAccountID, Equal<Vendor.bAccountID>>>>,
            Where<
                Vendor.bAccountID, Equal<Current<poVendorID>>,
               And<
                   CRLocation.isActive, Equal<True>,
               And<MatchWithBranch<CRLocation.vBranchID>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [LocationID(typeof(Where<
                                Location.bAccountID, Equal<Current<poVendorID>>,
                           And<
                                   MatchWithBranch<Location.vBranchID>>>),
                DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Vendor Location ID", FieldClass = "DISTINV")]
        [PXRestrictor(typeof(Where<Location.isActive, Equal<True>>), IN.Messages.InactiveLocation, typeof(Location.locationCD))]
        [PXUIEnabled(typeof(Where<enablePO, Equal<True>, And<canChangeMarkForPO, Equal<True>>>))]
        public virtual int? POVendorLocationID { get; set; }
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
        #region POStatus
        public abstract class poStatus : PX.Data.BQL.BqlString.Field<poStatus> { }

        [PXDBString]
        [POOrderStatus.List]
        [PXUIField(DisplayName = "PO Status", Enabled = false, FieldClass = "DISTINV")]
        public virtual string POStatus { get; set; }
        #endregion

        #region AllocatedFromSrvOrdPOQty
        public abstract class allocatedFromSrvOrdPOQty : PX.Data.BQL.BqlDecimal.Field<allocatedFromSrvOrdPOQty> { }

        [FSDBQuantity(typeof(FSAppointmentDet.uOM), typeof(FSAppointmentDet.baseAllocatedFromSrvOrdPOQty), InventoryUnitType.BaseUnit)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Allocated from Service Order PO Qty.", Enabled = false)]
        public virtual Decimal? AllocatedFromSrvOrdPOQty { get; set; }
        #endregion
        #region BaseAllocatedFromSrvOrdPOQty
        public abstract class baseAllocatedFromSrvOrdPOQty : PX.Data.BQL.BqlDecimal.Field<baseAllocatedFromSrvOrdPOQty> { }

        [PXDBDecimal(6)]
        public virtual Decimal? BaseAllocatedFromSrvOrdPOQty { get; set; }
        #endregion
        #endregion

        #region TabOrigin
        public abstract class tabOrigin : PX.Data.BQL.BqlString.Field<tabOrigin> { }
        [PXInt]
        public virtual int? TabOrigin
        {
            get
            {
                if (LineType == ID.LineType_ALL.INVENTORY_ITEM)
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
        public virtual string LotSerTrack { get; set; }
        #endregion

        #region LinkedEntityType
        public abstract class linkedEntityType : PX.Data.BQL.BqlString.Field<linkedEntityType>
        {
            public abstract class Values : ListField_Linked_Entity_Type { }
        }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Related Doc. Type", Enabled = false)]
        [linkedEntityType.Values.List]
        public virtual string LinkedEntityType { get; set; }
        #endregion
        #region LinkedDocType
        public abstract class linkedDocType : PX.Data.BQL.BqlString.Field<linkedDocType> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual string LinkedDocType { get; set; }
        #endregion
        #region LinkedDocRefNbr
        public abstract class linkedDocRefNbr : PX.Data.BQL.BqlString.Field<linkedDocRefNbr> { }

        [PXDBString(15, IsUnicode = true)]
        [PXUIField(Enabled = false, Visible = false)]
        public virtual string LinkedDocRefNbr { get; set; }
        #endregion
        #region LinkedDisplayRefNbr
        public abstract class linkedDisplayRefNbr : PX.Data.BQL.BqlString.Field<linkedDisplayRefNbr> { }
        [PXString]
        [PXUIField(DisplayName = "Related Doc. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string LinkedDisplayRefNbr
        {
            get
            {
                return (LinkedDocType != null ? LinkedDocType.Trim() + ", " : "") +
                       (LinkedDocRefNbr != null ? LinkedDocRefNbr.Trim() : "");
            }
        }
        #endregion

        #region SODetCreate
        public abstract class sODetCreate : PX.Data.BQL.BqlBool.Field<sODetCreate> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(Visible = false, Enabled = false)]
        public virtual bool? SODetCreate { get; set; }
        #endregion
        
        #region Mem_BatchNbr
        public abstract class mem_BatchNbr : PX.Data.BQL.BqlString.Field<mem_BatchNbr> { }

        [PXString(15, IsFixed = true)]
        [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
        public virtual string Mem_BatchNbr { get; set; }
        #endregion
        #region Mem_ServiceType
        public abstract class mem_ServiceType : ListField_Appointment_Service_Action_Type
        {
        }

        [PXString(1, IsFixed = true)]
        [mem_ServiceType.List]
        [PXFormula(typeof(Selector<pickupDeliveryAppLineRef, FSAppointmentDet.serviceType>))]
        [PXUIField(DisplayName = "Pickup/Deliver Items", Enabled = false)]
        public virtual string Mem_ServiceType { get; set; }
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
            return ActualQty;
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
            else if (fieldType == FieldType.ActualField)
            {
                return ActualDuration;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public int? GetApptDuration()
        {
            return 0;
        }

        public decimal? GetQty(FieldType fieldType)
        {
            if (fieldType == FieldType.EstimatedField)
            {
                return EstimatedQty;
            }
            else if (fieldType == FieldType.ActualField)
            {
                return ActualQty;
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
            return 0m;
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
                    cache.SetValueExt<actualQty>(this, qty);
                }
                else
                {
                    ActualQty = qty;
                }
            }
            else if (fieldType == FieldType.BillableField)
            {
                //TODO: review all the usage of BillableFields
                if (raiseEvents == true)
                {
                    cache.SetValueExt<actualQty>(this, qty);
                }
                else
                {
                    ActualQty = qty;
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public virtual bool IsService
        {
            get
            {
                return LineType == ID.LineType_ALL.SERVICE || LineType == ID.LineType_ALL.NONSTOCKITEM;
            }
        }

        public virtual bool IsInventoryItem
        {
            get
            {
                return LineType == ID.LineType_ALL.INVENTORY_ITEM;
            }
        }

        public virtual bool IsPickupDelivery
        {
            get
            {
                return LineType == ID.LineType_ALL.PICKUP_DELIVERY;
            }
        }

        public virtual bool needToBePosted()
        {
            return (LineType == ID.LineType_ALL.SERVICE
                        || LineType == ID.LineType_ALL.NONSTOCKITEM
                        || LineType == ID.LineType_ALL.INVENTORY_ITEM)
                    && IsPrepaid == false
                    && Status != ID.Status_AppointmentDet.CANCELED
                    && Status != ID.Status_AppointmentDet.NOT_PERFORMED;
        }

        public virtual bool IsExpenseReceiptItem
        {
            get
            {
                return LinkedEntityType == linkedEntityType.Values.ExpenseReceipt 
                        && string.IsNullOrEmpty(LinkedDocRefNbr) == false;
            }
        }

        public virtual bool ShouldBeWaitingPO
        {
            get
            {
                return EnablePO == true
                        && (BaseEffTranQty - BaseAllocatedFromSrvOrdPOQty > 0m)
                        && (CanChangeMarkForPO != true
                            || POSource == FSAppointmentDet.pOSource.Values.PurchaseToAppointment);
            }
        }

        public virtual bool ShouldBeRequestPO
        {
            get
            {
                return EnablePO == true
                        && POCompleted != true
                        && (CanChangeMarkForPO == true
                            && POSource == FSAppointmentDet.pOSource.Values.PurchaseToServiceOrder);
            }
        }
        #endregion

        #region InventoryCD
        public abstract class inventoryCD : PX.Data.BQL.BqlBool.Field<inventoryCD> { }

        [PXString(InputMask = "", IsUnicode = true)]
        [PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Selector<FSAppointmentDet.inventoryID, InventoryItem.inventoryCD>))]
        public virtual string InventoryCD
        {
            get;
            set;
        }
        #endregion

        #region OrigSrvOrdNbr
        public abstract class origSrvOrdNbr : PX.Data.BQL.BqlString.Field<origSrvOrdNbr> { }
        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof(FSAppointment.soRefNbr), DefaultForUpdate = false)]
        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        [PXParent(typeof(FK.ServiceOrder))]
        public virtual String OrigSrvOrdNbr { get; set; }
        #endregion
        #region OrigLineNbr
        public abstract class origLineNbr : PX.Data.BQL.BqlInt.Field<origLineNbr> { }
        [PXDBInt()]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Order Line Nbr.", Visible = false, Enabled = false)]
        [PXParent(typeof(FK.SrvOrdLine))]
        public virtual Int32? OrigLineNbr { get; set; }
        #endregion

        #region Fields for Split
        #region UnassignedQty
        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        [PXDecimal(6)]
        [PXFormula(typeof(decimal0))]
        [PXUIField(DisplayName = "Unassigned Qty.", Visible = false, Enabled = false)]
        public virtual Decimal? UnassignedQty { get; set; }
        #endregion
        #region HasMixedProjectTasks
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public virtual bool? HasMixedProjectTasks { get; set; }
        #endregion
        #region Operation
        public abstract class operation : PX.Data.BQL.BqlString.Field<operation> { }
        protected String _Operation;
        [PXString(1, IsFixed = true, InputMask = ">a")]
        [PXUnboundDefault(typeof(SO.SOOperation.issue))]
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
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        protected String _TranType;
        [PXString(SOOrderTypeOperation.iNDocType.Length, IsFixed = true)]
        [PXUnboundDefault(typeof(INTranType.issue))]
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
        [PXShort()]
        [PXFormula(typeof(shortMinus1))]
        [PXUIField(DisplayName = "Inventory Multiplier")]
        public virtual Int16? InvtMult { get; set; }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [PXInt]
        public virtual Int32? LocationID
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
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        [PXDate(InputMask = "d", DisplayMask = "d")]
        [PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
        public virtual DateTime? ExpireDate { get; set; }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
        [PXInt()]
        public virtual Int32? TaskID
        {
            get
            {
                return this.ProjectTaskID;
            }
            set
            {
                this.ProjectTaskID = value;
            }
        }
        #endregion
        #region IsLotSerialRequired
        public abstract class isLotSerialRequired : PX.Data.BQL.BqlBool.Field<isLotSerialRequired> { }

        [PXBool]
        public virtual bool? IsLotSerialRequired { get; set; }
        #endregion
        #endregion
        #region Methods
        public static implicit operator FSApptLineSplit(FSAppointmentDet item)
        {
            FSApptLineSplit ret = new FSApptLineSplit();
            ret.SrvOrdType = item.SrvOrdType;
            ret.ApptNbr = item.RefNbr;
            ret.LineNbr = item.LineNbr;
            ret.Operation = item.Operation;
            ret.SplitLineNbr = 1;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ExpireDate = item.ExpireDate;
            ret.Qty = item.ActualQty;
            ret.BaseQty = item.BaseQty;
            ret.UOM = item.UOM;
            ret.InvtMult = item.InvtMult;

            return ret;
        }
        public static implicit operator FSAppointmentDet(FSApptLineSplit item)
        {
            FSAppointmentDet ret = new FSAppointmentDet();
            ret.SrvOrdType = item.SrvOrdType;
            ret.RefNbr = item.ApptNbr;
            ret.LineNbr = item.LineNbr;
            ret.LineType = "GI";
            ret.Operation = item.Operation;
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.SubItemID = item.SubItemID;
            ret.LocationID = item.LocationID;
            ret.LotSerialNbr = item.LotSerialNbr;
            ret.ActualQty = item.Qty;
            ret.UOM = item.UOM;
            ret.BaseQty = item.BaseQty;
            ret.InvtMult = item.InvtMult;

            return ret;
        }
        #endregion
    }
}
