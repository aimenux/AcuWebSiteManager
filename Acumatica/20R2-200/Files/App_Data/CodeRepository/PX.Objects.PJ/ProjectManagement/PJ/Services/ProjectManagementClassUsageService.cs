using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Graphs;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.ProjectManagement.PJ.Services
{
    /// <summary>
    /// Provides common logic for "Use For" fields of a <see cref="ProjectManagementClass"/>.
    /// </summary>
    internal class ProjectManagementClassUsageService : IProjectManagementClassUsageService
    {
        private readonly ProjectManagementClassMaint graph;
        private readonly IProjectManagementClassDataProvider projectManagementClassDataProvider;

        public ProjectManagementClassUsageService(PXGraph graph)
        {
            this.graph = (ProjectManagementClassMaint) graph;
            projectManagementClassDataProvider = graph.GetService<IProjectManagementClassDataProvider>();
        }

        public void SetEnabledClassUsageIndicators(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClassDataProvider.DoesAnyRequestForInformationExist(projectManagementClass))
            {
                projectManagementClass.UseForRequestForInformation = true;
                DisableClass<ProjectManagementClass.useForRequestForInformation>();
            }
            if (projectManagementClassDataProvider.DoesAnyProjectIssueExist(projectManagementClass))
            {
                projectManagementClass.UseForProjectIssue = true;
                DisableClass<ProjectManagementClass.useForProjectIssue>();
            }
        }

        public void ValidateUseForProjectIssue(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClass?.UseForProjectIssue == false &&
                projectManagementClassDataProvider.DoesAnyProjectIssueExist(projectManagementClass))
            {
                throw new PXException(ProjectManagementMessages.ProjectManagementClassAlreadyInUse);
            }
        }

        public void ValidateUseForRequestForInformation(ProjectManagementClass projectManagementClass)
        {
            if (projectManagementClass?.UseForRequestForInformation == false &&
                projectManagementClassDataProvider.DoesAnyRequestForInformationExist(projectManagementClass))
            {
                throw new PXException(ProjectManagementMessages.ProjectManagementClassAlreadyInUse);
            }
        }

        private void DisableClass<TCache>()
            where TCache : IBqlField
        {
            PXUIFieldAttribute.SetEnabled<TCache>(graph.ProjectManagementClassesCurrent.Cache, null, false);
            graph.Delete.SetEnabled(false);
        }
    }
}