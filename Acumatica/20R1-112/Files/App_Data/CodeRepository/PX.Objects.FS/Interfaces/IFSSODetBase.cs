using PX.Data;
using System;

namespace PX.Objects.FS
{
    public enum FieldType
    {
        EstimatedField,
        ActualField,
        BillableField
    }

    public interface IFSSODetBase
    {
        int? DocID { get; }

        int? LineID { get; }

        int? LineNbr { get; }

        string LineRef { get; set; }

        int? BranchID { get; set; }

        long? CuryInfoID { get; set; }

        string LineType { get; set; }

        bool? IsPrepaid { get; set; }

        int? InventoryID { get; set; }

        int? SubItemID { get; set; }

        string UOM { get; set; }

        string TranDesc { get; set; }

        bool? ManualPrice { get; set; }

        bool? IsFree { get; set; }

        string BillingRule { get; set; }

        bool? IsBillable { get; set; }

        decimal? CuryUnitPrice { get; set; }

        decimal? UnitPrice { get; set; }

        decimal? CuryBillableExtPrice { get; set; }

        decimal? BillableExtPrice { get; set; }

        int? SiteID { get; set; }

        int? SiteLocationID { get; set; }

        int? GetDuration(FieldType fieldType);

        int? GetApptDuration();

        decimal? GetQty(FieldType fieldType);

        decimal? GetApptQty();

        void SetDuration(FieldType fieldType, int? duration, PXCache cache, bool raiseEvents);

        void SetQty(FieldType fieldType, decimal? qty, PXCache cache, bool raiseEvents);

        decimal? GetTranAmt(FieldType fieldType);

        int? GetPrimaryDACDuration();

        decimal? GetPrimaryDACQty();

        decimal? GetPrimaryDACTranAmt();

        int? ProjectID { get; set; }

        int? ProjectTaskID { get; set; }

        int? CostCodeID { get; set; }

        int? AcctID { get; set; }

        int? SubID { get; set; }

        string Status { get; set; }

        string EquipmentAction { get; set; }

        int? SMEquipmentID { get; set; }

        int? ComponentID { get; set; }

        int? EquipmentLineRef {get; set;}

        bool? Warranty { get; set; }

        bool IsService { get; }

        bool IsInventoryItem { get; }

        bool? ContractRelated { get; set; }

        bool? EnablePO { get; set; }

        decimal? UnitCost { get; set; }

        decimal? CuryUnitCost { get; set; }

        String TaxCategoryID { get; set; }

        String LotSerialNbr { get; set; }

        decimal? DiscPct { get; set; }

        decimal? CuryDiscAmt { get; set; }
    }
}