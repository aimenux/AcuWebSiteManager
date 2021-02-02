namespace PX.Objects.FS
{
    public interface IDocLine
    {
        #region Common Bound fields

        string LineType { get; }

        string LineRef { get; }

        string SrvOrdType { get; }

        string RefNbr { get; }

        int? InventoryID { get; set; }

        int? SubItemID { get; }

        string UOM { get; }

        string TranDesc { get; }

        bool? IsBillable { get; }

        bool? ManualPrice { get; }

        decimal? CuryUnitPrice { get; }

        decimal? CuryBillableExtPrice { get; }

        int? SiteID { get; }

        int? SiteLocationID { get; }

        int? ProjectID { get; }

        int? ProjectTaskID { get; }

        int? CostCodeID { get; }

        int? AcctID { get; set; }

        int? SubID { get; }

        int? PostID { get; }

        string EquipmentAction { get; }

        int? SMEquipmentID { get; }

        int? ComponentID { get; }

        int? EquipmentLineRef { get; }

        string NewTargetEquipmentLineNbr { get; }

        string Comment { get; }

        string LotSerialNbr { get; }
        
        string TaxCategoryID { get; }

        int? BranchID { get; }

        int? TabOrigin { get; }

        int? SortOrder { get; }

        decimal? CuryDiscAmt { get; }

        decimal? DiscPct { get; }
        #endregion

        #region Fields with different sources/values
        int? DocID { get; }

        int? LineID { get; }

        int? PostAppointmentID { get; }

        int? PostSODetID { get; }

        int? PostAppDetID { get; }

        string BillingBy { get; }

        string SourceTable { get; }

        bool IsService { get; }
        #endregion

        #region Methods
        decimal? GetQty(FieldType fieldType);

        decimal? GetBaseQty(FieldType fieldType);

        decimal? GetTranAmt(FieldType fieldType);
        #endregion
    }
}