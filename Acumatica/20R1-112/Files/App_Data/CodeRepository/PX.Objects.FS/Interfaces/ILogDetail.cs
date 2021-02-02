using System;

namespace PX.Objects.FS
{
    public interface ILogDetail
    {
        int? DocID { get; set; }

        string LineRef { get; set; }
        
        string DetLineRef { get; set; }
        
        string Descr { get; set; }
        
        int? BAccountID { get; set; }
        
        string Type { get; set; }
        
        DateTime? DateTimeBegin { get; set; }
        
        bool? Travel { get; set; }
        
        int? InventoryID { get; set; }
        
        Guid? UserID { get; set; }
        
        bool? Selected { get; set; }
    }
}