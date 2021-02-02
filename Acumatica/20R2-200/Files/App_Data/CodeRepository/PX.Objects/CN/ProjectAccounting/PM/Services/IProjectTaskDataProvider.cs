using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public interface IProjectTaskDataProvider
    {
        PMTask GetProjectTask(PXGraph graph, int? projectTaskId);

        IEnumerable<PMTask> GetProjectTasks(PXGraph graph, int? projectId);

        IEnumerable<PMTask> GetProjectTasks<TTaskType>(PXGraph graph, int? projectId)
            where TTaskType : BqlString.Constant<TTaskType>, new();
    }
}