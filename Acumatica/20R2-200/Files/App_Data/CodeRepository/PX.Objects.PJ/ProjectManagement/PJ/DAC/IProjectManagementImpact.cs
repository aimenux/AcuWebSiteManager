namespace PX.Objects.PJ.ProjectManagement.PJ.DAC
{
    public interface IProjectManagementImpact
    {
        bool? IsScheduleImpact
        {
            get;
            set;
        }

        int? ScheduleImpact
        {
            get;
            set;
        }

        bool? IsCostImpact
        {
            get;
            set;
        }

        decimal? CostImpact
        {
            get;
            set;
        }
    }
}