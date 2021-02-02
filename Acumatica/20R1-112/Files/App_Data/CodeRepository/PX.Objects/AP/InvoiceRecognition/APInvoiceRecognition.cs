using Newtonsoft.Json;
using PX.CloudServices.DocumentRecognition.InvoiceRecognition;
using PX.Common;
using PX.Data;
using PX.Objects.CM;
using PX.SM;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace PX.Objects.AP.InvoiceRecognition
{
	public class APInvoiceRecognitionEntry : PXGraph<APInvoiceRecognitionEntry, APRecognizedInvoice>
	{
		public const string FileIdParameter = "AttachmentFileId";
		public const string RecognitionUriParameter = "RecognitionUriParameter";
		public const string RecognitionTokenParameter = "RecognitionTokenParameter";
		public const string RecognitionResultJsonKey = "RecognitionResultJsonKey";
		public const string RecognitionFileIdKey = "RecognitionFileIdKey";

		[InjectDependency]
		public IInvoiceRecognitionClient InvoiceRecognitionClient { get; private set; }

		public PXSetup<APSetup> APSetup;

		public PXSelect<APRecognizedInvoice> Document;
        public PXSelect<APTran> Transactions;

		public PXAction<APRecognizedInvoice> SaveContinue;

		public IEnumerable transactions()
		{
			return Transactions.Cache.Cached;
		}

		public UploadFile GetSystemFile(Guid fileId)
		{
			var result = (PXResult<UploadFile, UploadFileRevision>)
				PXSelectJoin<UploadFile,
			    InnerJoin<UploadFileRevision,
				On<UploadFile.fileID, Equal<UploadFileRevision.fileID>, And<
					UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
				Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>.Select(this, fileId);
			if (result == null)
			{
				return null;
			}

			var file = (UploadFile)result;
			var fileRevision = (UploadFileRevision)result;
			file.Data = fileRevision.Data;

			return file;
		}

		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.docType> e)
		{
			e.NewValue = APDocType.Invoice;
		}

		protected virtual void _(Events.RowInserted<APRecognizedInvoice> e)
		{
			LoadRecognitionResult(e.Row);

			if (e.Row.RecognitionResult == null)
			{
				return;
			}

			var _invoiceDataLoader = new InvoiceDataLoader(e.Row.RecognitionResult, Document.Cache, Transactions.Cache);
			_invoiceDataLoader.Load(e.Row);
		}

		private void SendFeedback()
		{
			if (Document.Current == null)
			{
				return;
			}

			var uri = Document.Current.RecognitionResult?.Links?.FieldBound;
			if (uri == null)
			{
				return;
			}

			// var fileIdGuid = Guid.Parse("7CAB739D-3887-4D4F-8373-6BC704DE02C7");
			// Guid? fileId = fileIdGuid;

			var fileId = Document.Current.FileId;
			if (fileId == null)
			{
				return;
			}

			var jsonString = Document.Current.DocumentBoundingInfoJson;
			if (string.IsNullOrEmpty(jsonString))
			{
				return;
			}

			Task.Run(() => InvoiceRecognitionClient.Feedback(fileId.Value, uri, jsonString));
		}

        [PXUIField(DisplayName = "Save and Continue")]
        [PXButton]
        public void saveContinue()
        {
			SendFeedback();

			Document.Cache.IsDirty = false;
			Transactions.Cache.IsDirty = false;

			var invoiceEntryGraph = CreateInstance<APInvoiceEntry>();

			invoiceEntryGraph.Document.Insert(Document.Current);

			var defaultCurrencyInfo = Caches[typeof(CurrencyInfo)].Current as CurrencyInfo;
			invoiceEntryGraph.currencyinfo.Insert(defaultCurrencyInfo);

			foreach (var t in Transactions.Select())
			{
				invoiceEntryGraph.Transactions.Insert(t);
			}

			throw new PXRedirectRequiredException(invoiceEntryGraph, false, null);
		}

		public void StoreRecognitionResultIntoSession(InvoiceRecognitionResult recognitionResult, string fileId)
		{
			if (recognitionResult == null || string.IsNullOrEmpty(fileId))
			{
				return;
			}

			var recognizedResultJson = JsonConvert.SerializeObject(recognitionResult);

			PXContext.Session.SetString(RecognitionResultJsonKey, recognizedResultJson);
			PXContext.Session.SetString(RecognitionFileIdKey, fileId);
		}

		public string GetRecognitionResultJsonFromSession()
		{
			if (!(PXContext.Session[RecognitionResultJsonKey] is string json))
			{
				return null;
			}

			//var json = System.IO.File.ReadAllText(@"C:\repos\code\WebSites\Pure\Site\Scripts\output.json");

			return json;
		}

		private Guid? GetRecognitionFileIdFromSession()
		{
			if (!(PXContext.Session[RecognitionFileIdKey] is string fileIdString))
			{
				return null;
			}

			if (!Guid.TryParse(fileIdString, out var fileId))
			{
				return null;
			}

			return fileId;
		}

		private void LoadRecognitionResult(APRecognizedInvoice apInvoice)
		{
			if (apInvoice.RecognitionResult != null)
			{
				return;
			}

			var json = GetRecognitionResultJsonFromSession();
			if (string.IsNullOrEmpty(json))
			{
				return;
			}

			apInvoice.RecognitionResult = JsonConvert.DeserializeObject<InvoiceRecognitionResult>(json);
			apInvoice.FileId = GetRecognitionFileIdFromSession();
		}
    }
}
