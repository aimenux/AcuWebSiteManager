using PX.Objects.PJ.ProjectManagement.PJ.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public interface IProjectManagementImpactService
    {
        void ClearScheduleAndCostImpactIfRequired(IProjectManagementImpact projectManagementImpact);
    }
}