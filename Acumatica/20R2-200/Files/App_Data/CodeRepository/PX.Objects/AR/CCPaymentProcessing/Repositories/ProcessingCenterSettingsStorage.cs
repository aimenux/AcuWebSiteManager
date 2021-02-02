using System;
using System.Collections.Generic;
using PX.CCProcessingBase;
using PX.Data;
using PX.Objects.CA;

namespace PX.Objects.AR.CCPaymentProcessing.Repositories
{
	public class ProcessingCenterSettingsStorage : IProcessingCenterSettingsStorage
	{
		private readonly PXGraph _graph;
		private readonly string _processingCenterID;

		public ProcessingCenterSettingsStorage(PXGraph graph, string processingCenterID)
		{
			if (graph == null) throw new ArgumentNullException(nameof(graph));
			if (String.IsNullOrEmpty(processingCenterID)) throw new ArgumentNullException(nameof(processingCenterID));

			_graph = graph;
			_processingCenterID = processingCenterID;
		}

		void IProcessingCenterSettingsStorage.ReadSettings(Dictionary<string, string> aSettings)
		{
			PXSelectBase<CCProcessingCenterDetail> settings = new PXSelect<CCProcessingCenterDetail, Where<CCProcessingCenterDetail.processingCenterID,
						Equal<Required<CCProcessingCenterDetail.processingCenterID>>>>(_graph);
			foreach (CCProcessingCenterDetail it in settings.Select(_processingCenterID))
			{
				aSettings[it.DetailID] = it.Value;
			}
		}		
	}
}
