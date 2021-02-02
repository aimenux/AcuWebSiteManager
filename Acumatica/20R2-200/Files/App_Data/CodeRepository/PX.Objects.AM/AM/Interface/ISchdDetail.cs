using System;

namespace PX.Objects.AM
{
    /// <summary>
    /// Schedule Detail Interface
    /// </summary>
    public interface ISchdDetail
    {
        Int64? RecordID { get; set; }
        string ResourceID { get; set; }
        DateTime? SchdDate { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        /// <summary>
        /// Required due to overnight times. We need to know what is morning of or still apart of an overnight shift (next day but still apart of the previous date)
        /// </summary>
        DateTime? OrderByDate { get; set; }
        int? SchdTime { get; set; }
        int? SchdEfficiencyTime { get; set; }
        int? SchdBlocks { get; set; }
        bool? IsBreak { get; set; }
    }

    /// <summary>
    /// Schedule Detail Interface
    /// </summary>
    public interface ISchdDetail<T> : ISchdDetail
    {
        T Copy();
    }

    public interface ISchdReference
    {
        Guid? SchdKey { get; set; }
    }
}