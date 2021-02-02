using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.TX;
using System;

namespace PX.Objects.FS
{
    #region PXProjection
    [Serializable]
    [PXProjection(typeof(
            Select2<FSAppointment,
                InnerJoin<FSServiceOrder, 
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                InnerJoin<FSSrvOrdType,
                    On<
                        FSSrvOrdType.srvOrdType, Equal<FSAppointment.srvOrdType>>,
                InnerJoin<FSCustomerBillingSetup,
                    On<
                        FSCustomerBillingSetup.cBID, Equal<FSServiceOrder.cBID>>,
                InnerJoin<FSBillingCycle,
                    On<
                        FSBillingCycle.billingCycleID, Equal<FSCustomerBillingSetup.billingCycleID>>>>>>,
                Where2<
                    Where2<
                        Where<
                            FSAppointment.pendingAPARSOPost, Equal<True>,
                            And<FSAppointment.billServiceContractID, IsNull,
                            And<FSAppointment.billContractPeriodID, IsNull,
                            And<FSServiceOrder.billServiceContractID, IsNull,
                            And<FSServiceOrder.billContractPeriodID, IsNull>>>>>,
                        And<
                            FSAppointment.status, Equal<FSAppointment.status.Closed>,
                            Or<
                                Where<
                                        FSSrvOrdType.allowInvoiceOnlyClosedAppointment, Equal<False>,
                                    And<FSAppointment.status, Equal<FSAppointment.status.Completed>,
                                        And<FSAppointment.timeRegistered, Equal<True>>>>>>>,
                    And<
                        Where<
                            FSServiceOrder.status, Equal<FSServiceOrder.status.Closed>,
                            Or<FSServiceOrder.status, Equal<FSServiceOrder.status.Completed>,
                            Or<
                                Where<FSBillingCycle.invoiceOnlyCompletedServiceOrder, Equal<False>,
                                    And<FSServiceOrder.status, Equal<FSServiceOrder.status.Open>>>>>>>>>))]
    #endregion
    public class AppointmentToPost : FSAppointment, IPostLine
    {
        #region PostTo
        public abstract class postTo : PX.Data.BQL.BqlString.Field<postTo> { }

        [PXDBString(2, BqlField = typeof(FSSrvOrdType.postTo))]
        [FSPostTo.List]
        [PXUIField(DisplayName = "Post To")]
        public virtual string PostTo { get; set; }
        #endregion
        #region PostOrderType
        public abstract class postOrderType : PX.Data.BQL.BqlString.Field<postOrderType> { }

        [PXDBString(2, BqlField = typeof(FSSrvOrdType.postOrderType))]
        public virtual string PostOrderType { get; set; }
        #endregion
        #region PostOrderTypeNegativeBalance
        public abstract class postOrderTypeNegativeBalance : PX.Data.BQL.BqlString.Field<postOrderTypeNegativeBalance> { }

        [PXDBString(2, BqlField = typeof(FSSrvOrdType.postOrderTypeNegativeBalance))]
        public virtual string PostOrderTypeNegativeBalance { get; set; }
        #endregion
        #region PostNegBalanceToAP
        public abstract class postNegBalanceToAP : PX.Data.BQL.BqlBool.Field<postNegBalanceToAP> { }

        [PXDBBool(BqlField = typeof(FSSrvOrdType.postNegBalanceToAP))]
        [PXUIField(DisplayName = "Create a Bill Document in AP for Negative Balances")]
        public virtual bool? PostNegBalanceToAP { get; set; }
        #endregion
        #region DfltTermIDARSO
        public abstract class dfltTermIDARSO : PX.Data.BQL.BqlString.Field<dfltTermIDARSO> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(FSSrvOrdType.dfltTermIDARSO))]
        public virtual string DfltTermIDARSO { get; set; }
        #endregion

        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }

        [PXDBInt(BqlField = typeof(FSCustomerBillingSetup.billingCycleID))]
        [PXSelector(
                    typeof(Search<FSBillingCycle.billingCycleID>),
                    SubstituteKey = typeof(FSBillingCycle.billingCycleCD),
                    DescriptionField = typeof(FSBillingCycle.descr))]
        [PXUIField(DisplayName = "Billing Cycle ID", Enabled = false)]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region FrequencyType
        public abstract class frequencyType : ListField_Frequency_Type
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSCustomerBillingSetup.frequencyType))]
        [frequencyType.ListAtrribute]
        [PXUIField(DisplayName = "Frequency Type", Enabled = false)]
        public virtual string FrequencyType { get; set; }
        #endregion
        #region WeeklyFrequency
        public abstract class weeklyFrequency : ListField_WeekDaysNumber
        {
        }

        [PXDBInt(BqlField = typeof(FSCustomerBillingSetup.weeklyFrequency))]
        [PXUIField(DisplayName = "Frequency Week Day")]
        [weeklyFrequency.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WeeklyFrequency { get; set; }
        #endregion
        #region MonthlyFrequency
        public abstract class monthlyFrequency : PX.Data.BQL.BqlInt.Field<monthlyFrequency> { }

        [PXDBInt(BqlField = typeof(FSCustomerBillingSetup.monthlyFrequency))]
        [PXUIField(DisplayName = "Frequency Month Day")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? MonthlyFrequency { get; set; }
        #endregion
        #region SendInvoicesTo
        public abstract class sendInvoicesTo : ListField_Send_Invoices_To
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSCustomerBillingSetup.sendInvoicesTo))]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Send Invoices to")]
        public virtual string SendInvoicesTo { get; set; }
        #endregion

        #region BillingBy
        public abstract class billingBy : ListField_Billing_By
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSBillingCycle.billingBy))]
        [billingBy.ListAtrribute]
        public virtual string BillingBy { get; set; }
        #endregion
        #region GroupBillByLocations
        public abstract class groupBillByLocations : PX.Data.BQL.BqlBool.Field<groupBillByLocations> { }

        [PXDBBool(BqlField = typeof(FSBillingCycle.groupBillByLocations))]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Create Separate Invoices for Customer Locations")]
        public virtual bool? GroupBillByLocations { get; set; }
        #endregion

        #region BillCustomerID
        public new abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.billCustomerID))]
        [PXUIField(DisplayName = "Billing Customer ID")]
        [FSSelectorCustomer]
        public override int? BillCustomerID { get; set; }
        #endregion
        #region BillLocationID
        public abstract class billLocationID : PX.Data.BQL.BqlInt.Field<billLocationID> { }

        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<AppointmentToPost.billCustomerID>>>),
                    DescriptionField = typeof(Location.descr),
                    BqlField = typeof(FSServiceOrder.billLocationID), DisplayName = "Billing Location", DirtyRead = true)]
        public virtual int? BillLocationID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.branchLocationID))]
        [PXDefault(typeof(
            Search<FSxUserPreferences.dfltBranchLocationID,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>,
                And<PX.SM.UserPreferences.defBranchID, Equal<Current<FSServiceOrder.branchID>>>>>))]
        [PXUIField(DisplayName = "Branch Location ID")]
        [PXSelector(typeof(
            Search<FSBranchLocation.branchLocationID,
            Where<
                FSBranchLocation.branchID, Equal<Current<FSServiceOrder.branchID>>>>),
            SubstituteKey = typeof(FSBranchLocation.branchLocationCD),
            DescriptionField = typeof(FSBranchLocation.descr))]
        [PXFormula(typeof(Default<FSServiceOrder.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region Status
        public abstract class serviceOrderStatus : ListField_Status_ServiceOrder
        {
        }

        [PXDBString(1, IsFixed = true, BqlField = typeof(FSServiceOrder.status))]
        [PXUIField(DisplayName = "Status")]
        [status.ListAtrribute]
        public virtual string ServiceOrderStatus { get; set; }
        #endregion
        #region CustWorkOrderRefNbr
        public abstract class custWorkOrderRefNbr : PX.Data.BQL.BqlString.Field<custWorkOrderRefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", BqlField = typeof(FSServiceOrder.custWorkOrderRefNbr))]
        [PXUIField(DisplayName = "Customer Work Order Ref. Nbr.")]
        public virtual string CustWorkOrderRefNbr { get; set; }
        #endregion
        #region CustPORefNbr
        public abstract class custPORefNbr : PX.Data.BQL.BqlString.Field<custPORefNbr> { }

        [PXDBString(40, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", BqlField = typeof(FSServiceOrder.custPORefNbr))]
        [PXUIField(DisplayName = "Customer Purchase Order Ref. Nbr.")]
        public virtual string CustPORefNbr { get; set; }
        #endregion
        #region PostedBy
        public abstract class postedBy : PX.Data.BQL.BqlString.Field<postedBy> { }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSServiceOrder.postedBy))]
        public virtual string PostedBy { get; set; }
        #endregion

        #region BillingCycleCD
        public abstract class billingCycleCD : PX.Data.BQL.BqlString.Field<billingCycleCD> { }

        [PXDBString(15, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAA", BqlField = typeof(FSBillingCycle.billingCycleCD))]
        [PXUIField(DisplayName = "Billing Cycle ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSBillingCycle.billingCycleCD))]
        public virtual string BillingCycleCD { get; set; }
        #endregion
        #region BillingCycleType
        public abstract class billingCycleType : ListField_Billing_Cycle_Type
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSBillingCycle.billingCycleType))]
        [billingCycleType.ListAtrribute]
        [PXDefault(ID.Billing_Cycle_Type.TIME_FRAME)]
        public virtual string BillingCycleType { get; set; }
        #endregion
        #region InvoiceOnlyCompletedServiceOrder
        public abstract class invoiceOnlyCompletedServiceOrder : PX.Data.BQL.BqlBool.Field<invoiceOnlyCompletedServiceOrder> { }

        [PXDBBool(BqlField = typeof(FSBillingCycle.invoiceOnlyCompletedServiceOrder))]
        [PXUIField(DisplayName = "Invoice only completed or closed Service Orders")]
        public virtual bool? InvoiceOnlyCompletedServiceOrder { get; set; }
        #endregion
        #region TimeCycleType
        public abstract class timeCycleType : ListField_Time_Cycle_Type
        {
        }

        [PXDBString(2, IsFixed = true, BqlField = typeof(FSBillingCycle.timeCycleType))]
        [PXDefault(ID.Time_Cycle_Type.DAY_OF_MONTH)]
        [timeCycleType.ListAtrribute]
        [PXUIField(DisplayName = "Time Cycle Type")]
        public virtual string TimeCycleType { get; set; }
        #endregion
        #region TimeCycleWeekDay
        public abstract class timeCycleWeekDay : ListField_WeekDaysNumber
        {
        }

        [PXDBInt(BqlField = typeof(FSBillingCycle.timeCycleWeekDay))]
        [PXUIField(DisplayName = "Day of Week", Visible = false)]
        [timeCycleWeekDay.ListAtrribute]
        public virtual int? TimeCycleWeekDay { get; set; }
        #endregion
        #region TimeCycleDayOfMonth
        public abstract class timeCycleDayOfMonth : PX.Data.BQL.BqlInt.Field<timeCycleDayOfMonth> { }

        [PXDBInt(BqlField = typeof(FSBillingCycle.timeCycleDayOfMonth))]
        [PXUIField(DisplayName = "Day of Month")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]        
        public virtual int? TimeCycleDayOfMonth { get; set; }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDBInt(BqlField = typeof(FSServiceOrder.projectID))]
        [PXUIField(DisplayName = "Project ID")]
        public virtual int? ProjectID { get; set; }
        #endregion

        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        [PXDBString(4, BqlField = typeof(FSSrvOrdType.srvOrdType))]
        public virtual string DocType { get; set; }
        #endregion

        #region TaxZoneID
        public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        [PXDBString(10, IsUnicode = true, BqlField = typeof(FSAppointment.taxZoneID))]
        [PXUIField(DisplayName = "Customer Tax Zone")]
        [PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr), Filterable = true)]
        public override String TaxZoneID { get; set; }
        #endregion

        #region FilterDate
        public abstract class filterDate : PX.Data.BQL.BqlDateTime.Field<filterDate> { }

        [PXDBDate(BqlField = typeof(FSAppointment.executionDate))]
        [PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? FilterDate { get; set; }
        #endregion

        #region RowIndex
        public abstract class rowIndex : PX.Data.BQL.BqlInt.Field<rowIndex> { }

        [PXInt]
        public virtual int? RowIndex { get; set; }
        #endregion
        #region GroupKey
        public abstract class groupKey : PX.Data.BQL.BqlString.Field<groupKey> { }

        [PXString]
        public virtual string GroupKey { get; set; }
        #endregion

        #region BatchID
        public abstract class batchID : PX.Data.BQL.BqlInt.Field<batchID> { }

        [PXInt]
        [PXUIField(DisplayName = "Batch Nbr.", Enabled = false)]
        [PXSelector(typeof(Search<FSPostBatch.batchID>), SubstituteKey = typeof(FSPostBatch.batchNbr))]
        public virtual int? BatchID { get; set; }
        #endregion
        #region ErrorFlag
        public abstract class errorFlag : PX.Data.BQL.BqlBool.Field<errorFlag> { }

        [PXBool]
        public virtual bool? ErrorFlag { get; set; }
        #endregion

        #region Unbound fields
        public string EntityType
        {
            get
            {
                return ID.PostDoc_EntityType.APPOINTMENT;
            }
        }
        #endregion
    }
}
