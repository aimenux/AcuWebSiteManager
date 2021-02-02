using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PM;

namespace PX.Objects.CN.Common.Services.DataProviders
{
    public class BudgetedCostCodeDataProvider
    {
		public static IEnumerable<PMBudgetedCostCode> GetBudgetedCostCodes(PXGraph graph, int? projectId, int? projectTaskId)
		{
			return SelectFrom<PMBudgetedCostCode>
				.Where<PMBudgetedCostCode.projectID.IsEqual<P.AsInt>
					.And<PMBudgetedCostCode.projectTaskID.IsEqual<P.AsInt>>>.View
				.Select(graph, projectId, projectTaskId).FirstTableItems;
		}
	}
}