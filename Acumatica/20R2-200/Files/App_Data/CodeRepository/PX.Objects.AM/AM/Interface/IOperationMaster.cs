namespace PX.Objects.AM
{
    public interface IOperationMaster
    {
        int? OperationID { get; set; }
        string OperationCD { get; set; }
        int? SetupTime { get; set; }
        int? RunUnitTime { get; set; }
        decimal? RunUnits { get; set; }
        int? MachineUnitTime { get; set; }
        decimal? MachineUnits { get; set; }
        string WcID { get; set; }
    }
}