using PX.Objects.PJ.ProjectAccounting.PM.Services;
using PX.Objects.PJ.RequestsForInformation.PM.DAC;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
        public PXSelect<ProjectContact,
            Where<ProjectContact.projectId, Equal<Current<PMProject.contractID>>>> ProjectContacts;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.FieldUpdated<ProjectContact.contactId> args)
        {
            if (args.Row is ProjectContact projectContact)
            {
                args.Cache.SetDefaultExt<ProjectContact.email>(projectContact);
                args.Cache.SetDefaultExt<ProjectContact.phone>(projectContact);
            }
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