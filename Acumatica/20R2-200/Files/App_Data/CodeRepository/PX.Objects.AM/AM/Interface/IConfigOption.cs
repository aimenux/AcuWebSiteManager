namespace PX.Objects.AM
{
    /// <summary>
    /// Manufacturing configuration option
    /// </summary>
    public interface IConfigOption
    {
        string Label { get; set; }
        int? InventoryID { get; set; }
        int? SubItemID { get; set; }
        string Descr { get; set; }
        bool? FixedInclude { get; set; }
        bool? QtyEnabled { get; set; }
        string QtyRequired { get; set; }
        string UOM { get; set; }
        string MinQty { get; set; }
        string MaxQty { get; set; }
        string LotQty { get; set; }
        string ScrapFactor { get; set; }
        string PriceFactor { get; set; }
        bool? ResultsCopy { get; set; }
    }
}