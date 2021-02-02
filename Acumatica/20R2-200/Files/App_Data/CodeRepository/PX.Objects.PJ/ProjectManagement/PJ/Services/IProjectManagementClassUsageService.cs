using PX.Objects.PJ.ProjectManagement.PJ.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public interface IProjectManagementClassUsageService
    {
        void SetEnabledClassUsageIndicators(ProjectManagementClass projectManagementClass);

        void ValidateUseForProjectIssue(ProjectManagementClass projectManagementClass);

        void ValidateUseForRequestForInformation(ProjectManagementClass projectManagementClass);
    }
}