using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public class PRDocGraphCreator : IDocGraphCreator
	{
		public PXGraph Create(GLTran tran)
		{
			return Create(tran.TranType, tran.RefNbr, null);
		}

		public PXGraph Create(string aTranType, string aRefNbr, int? referenceID)
		{
			PRPayChecksAndAdjustments graph = PXGraph.CreateInstance<PRPayChecksAndAdjustments>();
			graph.Document.Current = graph.Document.Search<PRPayment.refNbr>(aRefNbr, aTranType);
			return graph;
		}
	}
}
