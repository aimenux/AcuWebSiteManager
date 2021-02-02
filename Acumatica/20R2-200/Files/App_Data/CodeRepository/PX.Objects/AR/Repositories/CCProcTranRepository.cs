using PX.Data;
using System;
using System.Collections.Generic;
using PX.Objects.Extensions.PaymentTransaction;
namespace PX.Objects.AR.Repositories
{
	public class CCProcTranRepository
	{
		protected readonly PXGraph _graph;

		public CCProcTranRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public CCProcTran GetCCProcTran(int? tranID)
		{
			return PXSelect<CCProcTran, Where<CCProcTran.tranNbr, Equal<Required<CCProcTran.tranNbr>>>>.Select(_graph, tranID);
		}

		public CCProcTran InsertCCProcTran(CCProcTran transaction)
		{
			return (CCProcTran)_graph.Caches[typeof(CCProcTran)].Insert(transaction);
		}

		public CCProcTran UpdateCCProcTran(CCProcTran transaction)
		{
			return (CCProcTran)_graph.Caches[typeof(CCProcTran)].Update(transaction);
		}

		public CCProcTran FindVerifyingCCProcTran(int? pMInstanceID)
		{
			return PXSelect<CCProcTran, Where<CCProcTran.pMInstanceID, Equal<Required<CCProcTran.pMInstanceID>>,
					And<CCProcTran.procStatus, Equal<CCProcStatus.finalized>,
						And<CCProcTran.tranStatus, Equal<CCTranStatusCode.approved>,
						And<CCProcTran.cVVVerificationStatus, Equal<CVVVerificationStatusCode.match>
							>>>>, OrderBy<Desc<CCProcTran.tranNbr>>>.Select(_graph, pMInstanceID);
		}

		public IEnumerable<CCProcTran> GetCCProcTranByTranID(int? transactionId)
		{
			return PXSelect<CCProcTran, Where<CCProcTran.transactionID, Equal<Required<CCProcTran.transactionID>>>,OrderBy<Asc<CCProcTran.tranNbr>>>
				.Select(_graph, transactionId).RowCast<CCProcTran>();
		}
	}
}
