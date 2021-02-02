using PX.Objects.PJ.ProjectManagement.PJ.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public class ProjectManagementImpactService : IProjectManagementImpactService
    {
        public void ClearScheduleAndCostImpactIfRequired(IProjectManagementImpact projectManagementImpact)
        {
            if (projectManagementImpact.IsScheduleImpact != true)
            {
                projectManagementImpact.ScheduleImpact = null;
            }
            if (projectManagementImpact.IsCostImpact != true)
            {
                projectManagementImpact.CostImpact = null;
            }
        }
    }
}