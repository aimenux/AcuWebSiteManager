using System;

namespace PX.Objects.AM
{
    public interface IEstimateInventory
    {
        Int32? InventoryID { get; set; }
        String InventoryCD { get; set; }
        Boolean? IsNonInventory { get; set; }
        String ItemDesc { get; set; }
        String UOM { get; set; }
        int? ItemClassID { get; set; }
    }
}