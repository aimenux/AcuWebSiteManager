using PX.Objects.PJ.ProjectManagement.PJ.DAC;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    public interface IProjectManagementClassDataProvider
    {
        bool IsClassInUse(ProjectManagementClass projectManagementClass);

        bool DoesAnyRequestForInformationExist(ProjectManagementClass projectManagementClass);

        bool DoesAnyProjectIssueExist(ProjectManagementClass projectManagementClass);

        ProjectManagementClass GetProjectManagementClass(string projectManagementClassId);
    }
}