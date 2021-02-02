using PX.Data;
using System;

namespace PX.Objects.CA.Repositories
{
	public class CashAccountRepository
	{
		protected readonly PXGraph _graph;

		public CashAccountRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CashAccount GetCashAccount(int? cashAccountId)
		{
			return PXSelect<CashAccount, Where<CashAccount.cashAccountID,
				Equal<Required<CashAccount.cashAccountID>>>>.Select(_graph, cashAccountId);
		}
	}
}
