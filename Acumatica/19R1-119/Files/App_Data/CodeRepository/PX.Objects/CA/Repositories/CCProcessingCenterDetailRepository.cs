using PX.Data;
using System;


namespace PX.Objects.CA.Repositories
{
	public class CCProcessingCenterDetailRepository
	{
		protected readonly PXGraph _graph;

		public CCProcessingCenterDetailRepository(PXGraph graph)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));

			_graph = graph;
		}

		public PXResultset<CCProcessingCenterDetail> FindAllProcessingCenterDetails(string processingCenterID)
		{
			return PXSelect<CCProcessingCenterDetail, Where<CCProcessingCenterDetail.processingCenterID,
						Equal<Required<CCProcessingCenterDetail.processingCenterID>>>>.Select(_graph, processingCenterID);
		}
	}
}
