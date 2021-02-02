using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public class BusinessAccountDataProvider : IBusinessAccountDataProvider
	{
		public BAccount GetBusinessAccount(PXGraph graph, int? accountId)
		{
			return SelectFrom<BAccount>
				.Where<BAccount.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, accountId);
		}

		public BAccountR GetBusinessAccountReceivable(PXGraph graph, int? accountId)
		{
			return SelectFrom<BAccountR>
				.Where<BAccountR.bAccountID.IsEqual<P.AsInt>>.View.Select(graph, accountId);
		}
	}
}