using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Services;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual IEnumerable CreateTemplate(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            try
            {
                return baseHandler(adapter);
            }
            catch (PXRedirectRequiredException exception)
            {
                CopyTaskTypes((TemplateMaint) exception.Graph);
                throw;
            }
        }

        [PXOverride]
        public virtual PMTask CopyTask(PMTask originalTask, int projectId,
            ProjectEntry.DefaultFromTemplateSettings settings,
            Func<PMTask, int, ProjectEntry.DefaultFromTemplateSettings, PMTask> baseHandler)
        {
            var targetTask = baseHandler(originalTask, projectId, settings);
            targetTask.Type = originalTask.Type;
            return targetTask;
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMRevenueBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.cost>>>>>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(PMRevenueBudget.projectTaskID),
            Message = ProjectAccountingMessages.RevenueTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Cost)]
        protected virtual void PMRevenueBudget_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<PMCostBudget.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>))]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(PMCostBudget.projectTaskID),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
        protected virtual void PMCostBudget_ProjectTaskID_CacheAttached(PXCache cache)
        {
        }

        protected virtual void _(Events.RowPersisting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskTypeUsageService = new ProjectTaskTypeUsageInConstructionValidationService();
                projectTaskTypeUsageService.ValidateProjectTaskType(args.Cache, projectTask);
            }
        }

        protected virtual void _(Events.RowDeleting<PMTask> args)
        {
            var projectTask = args.Row;
            if (projectTask != null)
            {
                var projectTaskUsageService = new ProjectTaskUsageInConstructionValidationService();
                projectTaskUsageService.ValidateProjectTask(projectTask);
            }
        }

        private void CopyTaskTypes(TemplateMaint templateMaint)
        {
            var originalTasks = Base.Tasks.Select().FirstTableItems.ToList();
            var insertedTasks = (IEnumerable<PMTask>) templateMaint.Tasks.Cache.Inserted;
            insertedTasks.ForEach(task => CopyTaskType(task, originalTasks));
        }

        private static void CopyTaskType(PMTask insertedTask, IEnumerable<PMTask> originalTasks)
        {
            var originalTask = originalTasks.Single(t => t.TaskCD == insertedTask.TaskCD);
            insertedTask.Type = originalTask.Type;
        }
    }
}