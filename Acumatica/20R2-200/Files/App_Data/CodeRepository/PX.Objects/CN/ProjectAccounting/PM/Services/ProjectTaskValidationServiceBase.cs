using System.Linq;
using PX.Data;

namespace PX.Objects.CN.ProjectAccounting.PM.Services
{
    public abstract class ProjectTaskValidationServiceBase
    {
        private readonly PXGraph graph;

        protected ProjectTaskValidationServiceBase()
        {
			graph = PXGraph.CreateInstance(typeof(PXGraph));
		}

        protected abstract bool IsTaskUsedInCostDocument(int? taskId);

        protected abstract bool IsTaskUsedInRevenueDocument(int? taskId);

        protected bool IsTaskUsed<TTable, TProjectTaskField>(int? projectTaskId)
            where TTable : class, IBqlTable, new()
            where TProjectTaskField : IBqlField
        {
            return new PXSelect<TTable,
                    Where<TProjectTaskField, Equal<Required<TProjectTaskField>>>>(graph)
                .Select(projectTaskId).FirstTableItems.Any();
        }

        protected bool IsTaskUsed<TTable, TProjectTaskField, TBudgetTypeField>(int? projectTaskId, string budgetType)
            where TTable : class, IBqlTable, new()
            where TProjectTaskField : IBqlField
            where TBudgetTypeField : IBqlField
        {
            return new PXSelect<TTable,
                    Where<TProjectTaskField, Equal<Required<TProjectTaskField>>,
                        And<TBudgetTypeField, Equal<Required<TBudgetTypeField>>>>>(graph)
                .Select(projectTaskId, budgetType).FirstTableItems.Any();
        }
    }
}