using System.Linq;
using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Objects.PJ.ProjectManagement.PJ.DAC;
using PX.Objects.PJ.ProjectManagement.PJ.Services;
using PX.Objects.PJ.ProjectsIssue.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services;
using PX.Objects.CS;

namespace PX.Objects.PJ.ProjectManagement.PJ.Graphs
{
    public class ProjectManagementClassMaint : PXGraph<ProjectManagementClassMaint, ProjectManagementClass>
    {
        public PXSelect<ProjectManagementClass> ProjectManagementClasses;

        public PXSelect<ProjectManagementClassPriority,
            Where<ProjectManagementClassPriority.classId,
                Equal<Current<ProjectManagementClass.projectManagementClassId>>>,
            OrderBy<Asc<ProjectManagementClassPriority.sortOrder>>> ProjectManagementClassPriority;

        public PXSelect<ProjectManagementClass,
            Where<ProjectManagementClass.projectManagementClassId,
                Equal<Current<ProjectManagementClass.projectManagementClassId>>>> ProjectManagementClassesCurrent;

        public PXSelectJoin<CSAnswers,
            RightJoin<RequestForInformation, On<CSAnswers.refNoteID, Equal<RequestForInformation.noteID>>,
            RightJoin<ProjectIssue, On<CSAnswers.refNoteID, Equal<ProjectIssue.noteID>>>>> Answers;

        public PXSelectJoin<CSAttributeGroup,
            InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
            Where<CSAttributeGroup.entityType, Equal<RequestForInformation.typeName>,
                And<CSAttributeGroup.entityClassID,
                    Equal<Current<ProjectManagementClass.projectManagementClassId>>>>,
            OrderBy<Asc<CSAttributeGroup.sortOrder>>> Attributes;

        private readonly CommonAttributesService commonAttributesService;
        private readonly ProjectManagementClassPriorityService projectManagementClassPriorityService;

        public ProjectManagementClassMaint()
        {
	        commonAttributesService = new CommonAttributesService(this, Attributes);
            projectManagementClassPriorityService = new ProjectManagementClassPriorityService(this);
        }

        [InjectDependency]
        public IProjectManagementClassUsageService ProjectManagementClassUsageService
        {
            get;
            set;
        }

        [InjectDependency]
        public IProjectManagementClassDataProvider ProjectManagementClassDataProvider
        {
            get;
            set;
        }

        public virtual void _(Events.FieldSelecting<ProjectManagementClass,
            ProjectManagementClass.projectManagementClassId> args)
        {
            var projectManagementClass = args.Row;
            if (projectManagementClass != null)
            {
                ProjectManagementClassUsageService.SetEnabledClassUsageIndicators(projectManagementClass);
            }
        }

        public virtual void _(Events.FieldUpdated<ProjectManagementClass.projectManagementClassId> args)
        {
            if (args.Row is ProjectManagementClass projectManagementClass)
            {
                InsertDefaultPrioritiesToViewIfRequired(projectManagementClass.ProjectManagementClassId);
            }
        }

        public virtual void _(Events.RowPersisting<ProjectManagementClassPriority> args)
        {
            var projectManagementClassPriority = args.Row;
            if (projectManagementClassPriority != null)
            {
                var projectManagementClassPriorities = ProjectManagementClassPriority.Select().FirstTableItems;
                projectManagementClassPriorityService
                    .ValidateDefaultPrioritySelected(args.Cache, projectManagementClassPriorities.ToList());
                projectManagementClassPriorityService.ValidateDefaultPriority(args.Cache,
                    projectManagementClassPriority);
            }
        }

        public virtual void _(Events.FieldUpdating<ProjectManagementClass,
            ProjectManagementClass.useForRequestForInformation> args)
        {
            ProjectManagementClassUsageService.ValidateUseForRequestForInformation(args.Row);
        }

        public virtual void _(Events.FieldUpdating<ProjectManagementClass,
            ProjectManagementClass.useForProjectIssue> args)
        {
            ProjectManagementClassUsageService.ValidateUseForProjectIssue(args.Row);
        }

        public virtual void _(Events.RowPersisting<ProjectManagementClass> args)
        {
            ProjectManagementClassUsageService.ValidateUseForProjectIssue(args.Row);
            ProjectManagementClassUsageService.ValidateUseForRequestForInformation(args.Row);
        }

        public virtual void _(Events.RowDeleting<ProjectManagementClass> args)
        {
            var projectManagementClass = args.Row;
            if (projectManagementClass != null &&
                ProjectManagementClassDataProvider.IsClassInUse(projectManagementClass))
            {
                throw new PXException(ProjectManagementMessages.CannotDeleteProjectManagementClassInUse);
            }
        }

        public virtual void _(Events.FieldUpdated<ProjectManagementClassPriority.isDefault> args)
        {
            if (args.Row is ProjectManagementClassPriority projectManagementClassPriority)
            {
                SetPriorityAsDefaultIfRequired(projectManagementClassPriority);
            }
        }

        public virtual void _(Events.RowInserting<CSAttributeGroup> args)
        {
            var attributeGroup = args.Row;
            if (attributeGroup == null || attributeGroup.EntityType == typeof(ProjectIssue).FullName)
            {
                return;
            }
            commonAttributesService.InitializeInsertedAttribute<RequestForInformation>(attributeGroup,
                ProjectManagementClasses.Current.ProjectManagementClassId);
            commonAttributesService.CreateRelatedEntityAttribute<ProjectIssue>(attributeGroup);
        }

        public virtual void _(Events.RowUpdated<CSAttributeGroup> args)
        {
            var attributeGroup = args.Row;
            if (attributeGroup == null || attributeGroup.EntityType == typeof(ProjectIssue).FullName)
            {
                return;
            }
            commonAttributesService.UpdateRelatedEntityAttribute<ProjectIssue>(attributeGroup);
        }

        public virtual void _(Events.RowDeleted<CSAttributeGroup> args)
        {
            var attributeGroup = args.Row;
            if (attributeGroup == null || attributeGroup.EntityType == typeof(ProjectIssue).FullName)
            {
                return;
            }
            commonAttributesService.DeleteRelatedEntityAttribute<ProjectIssue>(attributeGroup);
        }

        public virtual void _(Events.RowDeleting<CSAttributeGroup> args)
        {
            var attributeGroup = args.Row;
            if (attributeGroup == null)
            {
                return;
            }
            commonAttributesService.DeleteAnswersIfRequired<RequestForInformation>(args);
            if (!args.Cancel && attributeGroup.EntityType != typeof(ProjectIssue).FullName)
            {
                commonAttributesService.DeleteRelatedEntityAnswer<ProjectIssue>(args.Row);
            }
        }

        public virtual void _(Events.RowDeleting<ProjectManagementClassPriority> args)
        {
            if (args.Row is ProjectManagementClassPriority projectManagementClassPriority)
            {
                var projectManagementClass = ProjectManagementClassDataProvider
                    .GetProjectManagementClass(projectManagementClassPriority.ClassId);
                if (projectManagementClass != null)
                {
                    projectManagementClassPriorityService.ValidateSystemPriority(projectManagementClassPriority);
                    projectManagementClassPriorityService.ValidateActivePriority(projectManagementClassPriority);
                }
                projectManagementClassPriorityService.ValidateUsedPriority(projectManagementClassPriority);
            }
        }

        public virtual void _(Events.FieldSelecting<CSAttributeGroup.defaultValue> args)
        {
            if (args.Row is CSAttributeGroup attributeGroup)
            {
                args.ReturnState = commonAttributesService.GetNewReturnState(args.ReturnState, attributeGroup);
            }
        }

        private void SetPriorityAsDefaultIfRequired(ProjectManagementClassPriority projectManagementClassPriority)
        {
            if (projectManagementClassPriority.IsDefault == true)
            {
                var priority = ProjectManagementClassPriority.Select().FirstTableItems.SingleOrDefault(
                    p => p.IsDefault == true && p.PriorityId != projectManagementClassPriority.PriorityId);
                ProjectManagementClassPriority.Cache.SetValue<
                    ProjectManagementClassPriority.isDefault>(priority, false);
                ProjectManagementClassPriority.Update(priority);
                ProjectManagementClassPriority.View.RequestRefresh();
            }
        }

        private void InsertDefaultPrioritiesToViewIfRequired(string classId)
        {
            if (!projectManagementClassPriorityService.DoesAnyPriorityExistForClass(classId))
            {
                var priorities = projectManagementClassPriorityService
                    .GetDefaultProjectManagementClassPriorities(classId);
                ProjectManagementClassPriority.Cache.InsertAll(priorities);
            }
        }
    }
}