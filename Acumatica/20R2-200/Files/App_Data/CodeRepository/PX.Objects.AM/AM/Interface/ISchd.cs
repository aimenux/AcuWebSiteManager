using System;

namespace PX.Objects.AM
{
    public interface ISchd
    {
        string ResourceID { get; set; }
        DateTime? SchdDate { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        int? WorkTime { get; set; }
        int? TotalBlocks { get; set; }
        int? SchdTime { get; set; }
        int? SchdEfficiencyTime { get; set; }
        int? PlanBlocks { get; set; }
        int? SchdBlocks { get; set; }
        int? AvailableBlocks { get; set; }
    }
}