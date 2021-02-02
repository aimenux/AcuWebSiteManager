using PX.Objects.PJ.ProjectAccounting.PM.Services;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectManagement.PJ.GraphExtensions
{
    public class ProjectTaskEntryExt : PXGraphExtension<ProjectTaskEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
	    }

		protected virtual void _(Events.RowPersisting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskTypeUsageService = new ProjectTaskTypeUsageInProjectManagementValidationService();
                projectTaskTypeUsageService.ValidateProjectTaskType(args.Cache, projectTask);
            }
        }

        protected virtual void _(Events.RowDeleting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskUsageService = new ProjectTaskUsageInProjectManagementValidationService();
                projectTaskUsageService.ValidateProjectTask(projectTask);
            }
        }
    }
}