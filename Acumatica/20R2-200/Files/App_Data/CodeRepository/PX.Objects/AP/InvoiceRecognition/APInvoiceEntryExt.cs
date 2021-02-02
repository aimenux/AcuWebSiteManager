using PX.CloudServices.DAC;
using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Data.Search;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PX.Data.BQL.Fluent;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
        internal const string FEEDBACK_FIELD_BOUND_KEY = "feedback:field-bound";
        internal const string FEEDBACK_RECORD_SAVED_KEY = "feedback:record-saved";

		[InjectDependency]
		internal IEntitySearchService EntitySearchService { get; set; }

		[InjectDependency]
		public IDocumentRecognitionClient InvoiceRecognitionClient { get; set; }

		[InjectDependency]
		public IConfiguration Configuration { get; set; }
		
		public PXFilter<FeedbackParameters> FeedbackParameters;

		public SelectFrom<APRecognizedInvoice>
			   .Where<APRecognizedInvoice.documentLink.IsEqual<APInvoice.noteID.FromCurrent>>
			   .View.ReadOnly SourceDocument;

		public PXAction<APInvoice> viewSourceDocument;
		[PXUIField(DisplayName = "View Source Document")]
		[PXLookupButton]
		public virtual void ViewSourceDocument()
		{
			var recognizedSourceDocument = SourceDocument.SelectSingle();
			if (recognizedSourceDocument == null)
			{
				return;
			}

			var graph = PXGraph.CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = recognizedSourceDocument;

			throw new PXRedirectRequiredException(graph, null);
		}


		public override void Initialize()
		{
			var isRecognitionEnabled = PXAccess.FeatureInstalled<FeaturesSet.apDocumentRecognition>() && InvoiceRecognitionClient.IsConfigured();
			if (!isRecognitionEnabled)
			{
				return;
			}

			var recognizedRecordCache = Base.Caches[typeof(RecognizedRecord)];
			Base.Views.Caches.Add(recognizedRecordCache.GetItemType());

			Base.OnAfterPersist += SendFeedback;
			
			var inquiryAction = Base.Actions["Inquiry"];
			if (inquiryAction != null)
			{
				inquiryAction.AddMenuAction(viewSourceDocument);
			}
		}

		protected void _(Events.RowSelected<APInvoice> e, PXRowSelected baseEvent)
		{
			baseEvent(e.Cache, e.Args);

			RecognizedRecord recognizedRecord =
				PXSelect<RecognizedRecord, Where<RecognizedRecord.documentLink,
					Equal<Required<RecognizedRecord.documentLink>>>>.SelectWindowed(Base, 0, 1, e.Row.NoteID);

			viewSourceDocument.SetEnabled(recognizedRecord != null);
		}

		private void SendFeedback(PXGraph graph)
		{
			if (!(graph is APInvoiceEntry invoiceEntry))
			{
				return;
			}

			var primaryRow = invoiceEntry.Document.Current;
			if (primaryRow == null || !APDocType.Invoice.Equals(primaryRow.DocType, StringComparison.Ordinal))
			{
				return;
			}

            var feedbackBuilder = FeedbackParameters.Current.FeedbackBuilder;
            if (feedbackBuilder == null)
            {
                return;
            }

			var primaryView = invoiceEntry.Document.View;
			var detailView = invoiceEntry.Transactions.View;
			var detailRows = invoiceEntry.Transactions.Select().Select(t => (APTran)t);
			var feedback = feedbackBuilder.ToRecordSavedFeedback(primaryView, primaryRow, detailView, detailRows,
				EntitySearchService);
			
            var docNoteId = primaryRow.NoteID;

            var fileId = FeedbackParameters.Current.FeedbackFileID;
			if (fileId == null)
			{
				return;
			}

			var links = FeedbackParameters.Current.Links;
			if (links == null)
			{
				return;
			}

			FeedbackParameters.Reset();
			
            var client = InvoiceRecognitionClient;
			if(Configuration.GetValue<bool>("SendDocumentInboxFeedback", false))
                PXLongOperation.StartOperation(Guid.NewGuid(), () => SendFeedbackAsync(fileId.Value, links, client, docNoteId, feedback).Wait());
		}

        private static async Task SendFeedbackAsync(Guid fileId, Dictionary<string, Uri> links,
            IDocumentRecognitionClient client,
            Guid? documentLink, VersionedFeedback recordSavedFeedback)
        {
            if (links == null)
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - links are not found");
                return;
            }
            await SendBoundFeedbackAsync(fileId, links, client, documentLink);
            await SendRecordSavedFeedbackAsync(fileId, links, client, recordSavedFeedback);
        }

        private static async Task SendRecordSavedFeedbackAsync(Guid fileId, Dictionary<string, Uri> links, IDocumentRecognitionClient client,
            VersionedFeedback recordSavedFeedback)
        {
            if (recordSavedFeedback == null)
                return;
            if (!links.TryGetValue(FEEDBACK_RECORD_SAVED_KEY, out var recordSavedLink))
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - link is not found:{LinkKey}",
                    FEEDBACK_RECORD_SAVED_KEY);
                return;
            }

            var formatter = new JsonMediaTypeFormatter {SerializerSettings = VersionedFeedback._settings};
            await client.Feedback(fileId, recordSavedLink,
                new ObjectContent(recordSavedFeedback.GetType(), recordSavedFeedback, formatter));
        }

        private static async Task SendBoundFeedbackAsync(Guid fileId, Dictionary<string, Uri> links, IDocumentRecognitionClient client,
            Guid? documentLink)
        {
            if ( !links.TryGetValue(FEEDBACK_FIELD_BOUND_KEY, out var link))
            {
                PXTrace.WriteError("IDocumentRecognitionClient: Unable to send feedback - link is not found:{LinkKey}",
                    FEEDBACK_FIELD_BOUND_KEY);
                return;
            }

            var graph = PXGraph.CreateInstance<PXGraph>();
            var recognizedRecord =
                PXSelect<RecognizedRecord, Where<RecognizedRecord.documentLink,
                        Equal<Required<RecognizedRecord.documentLink>>>>.Select(graph, documentLink).FirstOrDefault()
                    ?.GetItem<RecognizedRecord>();
            if (recognizedRecord?.RecognitionFeedback == null)
                return;
            var reader = new System.IO.StringReader(recognizedRecord.RecognitionFeedback);

            while (true)
            {
                var item = await reader.ReadLineAsync();
                if (item == null) break;
                if(string.IsNullOrWhiteSpace(item)) continue;
                await client.Feedback(fileId, link, new StringContent(item, Encoding.UTF8, "application/json"));
            }

            return;
        }
    }
}
