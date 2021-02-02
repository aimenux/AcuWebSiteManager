using System;

namespace PX.Objects.AM
{
    public interface IEstimateOper
    {
        string EstimateID { get; set; }
        string RevisionID { get; set; }
        int? OperationID { get; set; }
    }
}