using System;

namespace PX.Objects.FS
{
    public interface IPostLine
    {
        #region Bound Fields
        int? SOID { get; set; }

        int? AppointmentID { get; set; }

        string PostTo { get; set; }

        string PostOrderType { get; set; }

        string PostOrderTypeNegativeBalance { get; set; }

        bool? PostNegBalanceToAP { get; set; }

        string DfltTermIDARSO { get; set; }

        int? BillingCycleID { get; set; }

        int? ProjectID { get; set; }

        string FrequencyType { get; set; }

        int? WeeklyFrequency { get; set; }

        int? MonthlyFrequency { get; set; }

        string SendInvoicesTo { get; set; }

        string BillingBy { get; set; }

        bool? GroupBillByLocations { get; set; }

        string BillingCycleCD { get; set; }

        string BillingCycleType { get; set; }

        bool? InvoiceOnlyCompletedServiceOrder { get; set; }

        string TimeCycleType { get; set; }

        int? TimeCycleWeekDay { get; set; }

        int? TimeCycleDayOfMonth { get; set; }

        int? BillLocationID { get; set; }

        DateTime? FilterDate { get; set; }

        string CustWorkOrderRefNbr { get; set; }

        string CustPORefNbr { get; set; }

        int? BillCustomerID { get; set; }

        int? BranchID { get; set; }

        string DocType { get; set; }

        String CuryID { get; set; }

        String TaxZoneID { get; set; }
        #endregion

        #region Unbound Fields
        int? RowIndex { get; set; }

        string GroupKey { get; set; }

        int? BatchID { get; set; }

        bool? ErrorFlag { get; set; }

        string EntityType { get; }

        #endregion
    }
}