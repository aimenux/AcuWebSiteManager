using System;
using PX.Data;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType
{
    public class ActiveOrPlanningProjectTaskWithTypeAttribute : ProjectTaskWithTypeAttribute
    {
        public ActiveOrPlanningProjectTaskWithTypeAttribute(Type projectField)
            : base(projectField)
        {
            _Attributes.Add(new PXRestrictorAttribute(
                typeof(Where<PMTask.type.IsNotEqual<ProjectTaskType.revenue>>),
                ProjectAccountingMessages.CostTaskTypeIsNotValid));
        }

        protected override Type GetSearchType(Type projectId)
        {
            return BqlCommand.Compose(typeof(Search<,>), typeof(PMTask.taskID),
                typeof(Where<,,>), typeof(PMTask.projectID), typeof(Equal<>), typeof(Optional<>), projectId,
                typeof(And<PMTask.type, NotEqual<ProjectTaskType.revenue>,
                    And<PMTask.status, In3<ProjectTaskStatus.active, ProjectTaskStatus.planned>>>));
        }

        protected override PXSelectBase<PMTask> GetRequiredDefaultProjectTaskQuery(PXGraph graph)
        {
            var query = base.GetRequiredDefaultProjectTaskQuery(graph);
            query.WhereAnd<Where<PMTask.status, In3<ProjectTaskStatus.active, ProjectTaskStatus.planned>>>();
            return query;
        }
    }
}