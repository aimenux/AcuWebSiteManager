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

		public ExternalTransaction FindCapturedExternalTransaction(int? pMInstanceID, string tranNbr)
		{
			if (pMInstanceID == null)
			{
				throw new ArgumentNullException(nameof(pMInstanceID));
			}

			if (string.IsNullOrEmpty(tranNbr))
			{
				throw new ArgumentException(nameof(tranNbr));
			}

			var query = new PXSelect<ExternalTransaction,
				Where<ExternalTransaction.pMInstanceID, Equal<Required<ExternalTransaction.pMInstanceID>>,
					And<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>,
						Or<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureHeldForReview>>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			return query.SelectSingle(pMInstanceID, tranNbr);
		}

		public ExternalTransaction FindCapturedExternalTransaction(string procCenterId, string tranNbr)
		{
			if (string.IsNullOrEmpty(procCenterId))
			{
				throw new ArgumentException(nameof(procCenterId));
			}

			if (string.IsNullOrEmpty(tranNbr))
			{
				throw new ArgumentException(nameof(tranNbr));
			}

			var query = new PXSelect<ExternalTransaction,
				Where<ExternalTransaction.tranNumber, Equal<Required<ExternalTransaction.tranNumber>>,
					And<ExternalTransaction.processingCenterID, Equal<Required<ExternalTransaction.processingCenterID>>,
					And<Where<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureSuccess>, 
						Or<ExternalTransaction.procStatus, Equal<ExtTransactionProcStatusCode.captureHeldForReview>>>>>>,
				OrderBy<Desc<ExternalTransaction.transactionID>>>(graph);
			return query.SelectSingle(tranNbr, procCenterId);
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
