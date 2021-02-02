using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.PO;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class CommitmentDataProvider
	{
		public static POOrder GetCommitment(PXGraph graph, string orderNumber, string orderType)
		{
			return SelectFrom<POOrder>
				.Where<POOrder.orderNbr.IsEqual<P.AsString>.And<POOrder.orderType.IsEqual<P.AsString>>>.View
				.Select(graph, orderNumber, orderType);
		}

		public static IEnumerable<POLine> GetCommitmentLines(PXGraph graph, string orderNumber, string orderType,
			int? projectId)
		{
			return SelectFrom<POLine>
				.Where<POLine.orderNbr.IsEqual<P.AsString>
					.And<POLine.orderType.IsEqual<P.AsString>>
					.And<POLine.projectID.IsEqual<P.AsInt>>>.View
				.Select(graph, orderNumber, orderType, projectId).FirstTableItems;
		}
	}
}