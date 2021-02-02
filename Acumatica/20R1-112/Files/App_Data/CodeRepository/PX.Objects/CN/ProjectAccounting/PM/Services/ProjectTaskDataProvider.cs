using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public class ProjectTaskDataProvider : IProjectTaskDataProvider
    {
        public PMTask GetProjectTask(PXGraph graph, int? projectTaskId)
        {
            return new PXSelect<PMTask,
                Where<PMTask.taskID, Equal<Required<PMTask.taskID>>>>(graph).SelectSingle(projectTaskId);
        }

        public IEnumerable<PMTask> GetProjectTasks(PXGraph graph, int? projectId)
        {
            return SelectFrom<PMTask>.Where<PMTask.projectID.IsEqual<P.AsInt>>.View.Select(graph, projectId)
                .FirstTableItems;
        }

        public IEnumerable<PMTask> GetProjectTasks<TTaskType>(PXGraph graph, int? projectId)
            where TTaskType : BqlString.Constant<TTaskType>, new()
        {
            return SelectFrom<PMTask>
                .Where<PMTask.projectID.IsEqual<P.AsInt>
                    .And<PMTask.type.IsEqual<TTaskType>>>.View.Select(graph, projectId).FirstTableItems;
        }
    }
}