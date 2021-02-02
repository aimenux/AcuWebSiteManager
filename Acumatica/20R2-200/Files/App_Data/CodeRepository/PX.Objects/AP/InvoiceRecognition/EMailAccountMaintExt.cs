using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.CS;
using PX.SM;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class EMailAccountMaintExt : PXGraphExtension<EMailAccountMaint>
	{
		[InjectDependency]
		internal IDocumentRecognitionClient InvoiceRecognitionClient { get; set; }

		protected void _(Events.RowSelected<EMailAccount> e, PXRowSelected baseEvent)
		{
			baseEvent(e.Cache, e.Args);

			if (!(e.Args.Row is EMailAccount row))
			{
				return;
			}

			PXUIFieldAttribute.SetVisible<EMailAccountExt.submitToIncomingAPDocuments>(e.Cache, row,
				PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>() && InvoiceRecognitionClient.IsConfigured());

			PXUIFieldAttribute.SetEnabled<EMailAccountExt.submitToIncomingAPDocuments>(e.Cache, row, row.IncomingProcessing == true);
		}
	}
}
