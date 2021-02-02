using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.CN.Common.Services.DataProviders
{
	public interface IBusinessAccountDataProvider
	{
		BAccount GetBusinessAccount(PXGraph graph, int? accountId);

		BAccountR GetBusinessAccountReceivable(PXGraph graph, int? accountId);
	}
}