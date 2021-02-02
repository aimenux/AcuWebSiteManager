using PX.Data;
using System;
using System.Collections.Generic;
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

		public CCProcTran FindReferencedCCProcTran(int? pMInstanceID, string refTranNbr)
		{
			return PXSelect<CCProcTran,
				Where<CCProcTran.pMInstanceID, Equal<Required<CCProcTran.pMInstanceID>>,
					And<CCProcTran.pCTranNumber, Equal<Required<CCProcTran.pCTranNumber>>,
						And<Where<CCProcTran.tranType, Equal<CCTranTypeCode.authorizeAndCapture>,
							Or<CCProcTran.tranType, Equal<CCTranTypeCode.priorAuthorizedCapture>>>>>>,
							OrderBy<Desc<CCProcTran.tranNbr>>>.Select(_graph, pMInstanceID, refTranNbr);
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
