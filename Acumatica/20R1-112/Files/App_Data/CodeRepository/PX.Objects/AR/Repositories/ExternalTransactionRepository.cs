using System;
using PX.Data;

namespace PX.Objects.AR.Repositories
{
	public class ExternalTransactionRepository
	{
		protected readonly PXGraph graph;
		public ExternalTransactionRepository(PXGraph graph)
		{
			this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
		}

		public ExternalTransaction GetExternalTransaction(int? tranId)
		{
			return PXSelect<ExternalTransaction, Where<ExternalTransaction.transactionID, Equal<Required<ExternalTransaction.transactionID>>>>
				.Select(graph, tranId);
		}

		public ExternalTransaction InsertExternalTransaction(ExternalTransaction extTran)
		{
			return graph.Caches[typeof(ExternalTransaction)].Insert(extTran) as ExternalTransaction;
		}

		public ExternalTransaction UpdateExternalTransaction(ExternalTransaction extTran)
		{
			return graph.Caches[typeof(ExternalTransaction)].Update(extTran) as ExternalTransaction;
		}
	}
}
