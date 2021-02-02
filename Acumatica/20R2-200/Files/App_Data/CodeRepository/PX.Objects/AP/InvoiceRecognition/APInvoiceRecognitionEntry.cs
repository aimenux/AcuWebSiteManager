using Newtonsoft.Json;
using PX.CloudServices.DAC;
using PX.CloudServices.DocumentRecognition;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Search;
using PX.Data.Wiki.Parser;
using PX.Metadata;
using PX.Objects.AP.InvoiceRecognition.DAC;
using PX.Objects.AP.InvoiceRecognition.Feedback;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.SM;
using Serilog.Events;
using SerilogTimings.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PX.Objects.AP.InvoiceRecognition
{
	[PXInternalUseOnly]
	public class APInvoiceRecognitionEntry : PXGraph<APInvoiceRecognitionEntry, APRecognizedInvoice>
	{
		private const string _documentRecognitionModel = "tex-document-01";
		private const int _recognitionTimeoutMinutes = 10;

		internal const string PdfExtension = ".pdf";

		public const string RefNbrNavigationParam = nameof(APRecognizedInvoice.RecognizedRecordRefNbr);
		public const string StatusNavigationParam = nameof(APRecognizedInvoice.RecognitionStatus);
		public const string NoteIdNavigationParam = nameof(APRecognizedInvoice.NoteID);

        private JsonSerializer _jsonSerializer = JsonSerializer.CreateDefault(VersionedFeedback._settings);

		[InjectDependency]
		internal IScreenInfoProvider ScreenInfoProvider { get; set; }

		[InjectDependency]
		internal Serilog.ILogger _logger { get; set; }

		[InjectDependency]
		internal IEntitySearchService EntitySearchService { get; set; }

		[InjectDependency]
		public IDocumentRecognitionClient InvoiceRecognitionClient { get; set; }

		public PXSetup<APSetup> APSetup;

        public SelectFrom<APInvoice>.Where<APInvoice.docType.IsEqual<APRecognizedInvoice.docType.FromCurrent>.And<
            APInvoice.refNbr.IsEqual<APRecognizedInvoice.refNbr.FromCurrent>>> Invoices;
		
        public PXSelect<APRecognizedInvoice> Document;

        protected virtual IEnumerable document()
        {
            if (Document.Current != null&&Caches[typeof(CurrencyInfo)].Current!=null)
            {
				if(Caches[typeof(APRegister)].Current!=Document.Current)
                    Caches[typeof(APRegister)].Current = Document.Current;
                yield return Document.Current;
            }
            else
            {
                var records = this.QuickSelect(Document.View.BqlSelect);
                foreach (APRecognizedInvoice record in records)
                {
                    if (record.RefNbr == null && record.DocType == null)
                    {
                        DefaultInvoiceValues(record);
                        Document.Cache.SetStatus(record, PXEntryStatus.Held);
                        Caches[typeof(APRegister)].Current = record;
                    }

                    if (Document.Current == null) record.IsRedirect = true;
                    yield return record;
                }
            }
        }

        private void DefaultInvoiceValues(APRecognizedInvoice record)
        {
            record.DocType = record.EntityType;
            var inserted = Caches[typeof(APInvoice)].Insert(record);
            Caches[typeof(APInvoice)].Remove(inserted);
            Caches[typeof(APInvoice)].RestoreCopy(record, inserted);
        }
		
		public PXSelect<APTran> Transactions;

		public PXSelect<VendorR> Vendors;

		public SelectFrom<APRecognizedRecord>
			  .Where<APRecognizedRecord.refNbr.IsEqual<APRecognizedInvoice.recognizedRecordRefNbr.FromCurrent>>
			  .View RecognizedRecords;

		public PXFilter<BoundFeedback> BoundFeedback;

		public PXAction<APRecognizedInvoice> ContinueSave;

		public PXAction<APRecognizedInvoice> ProcessRecognition;
		public PXAction<APRecognizedInvoice> OpenDocument;
		public PXAction<APRecognizedInvoice> OpenDuplicate;

		public PXAction<APRecognizedInvoice> DumpTableFeedback;

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(IsKey = false)]
		protected virtual void APRecognizedInvoice_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(IsKey = false)]
		protected virtual void APRecognizedInvoice_DocType_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PopupMessage]
		[VendorActiveOrHoldPayments(
			Visibility = PXUIVisibility.SelectorVisible,
			DescriptionField = typeof(Vendor.acctName),
			CacheGlobal = true,
			Filterable = true)]
		[PXDefault]
		protected virtual void APRecognizedInvoice_VendorID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
		[PXFormula(typeof(
			IIf<Where<Current<APInvoice.docType>, NotEqual<APDocType.debitAdj>,
					And<Current<APInvoice.docType>, NotEqual<APDocType.prepayment>>>,
				Selector<APInvoice.vendorID, Vendor.termsID>,
				Null>))]
		protected virtual void APRecognizedInvoice_TermsID_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXParent(typeof(
			Select<APRecognizedInvoice,
			Where<APTran.refNbr, Equal<Current<APRecognizedInvoice.refNbr>>>>
		))]
		protected virtual void APTran_RefNbr_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(
			null,
			typeof(SumCalc<APRecognizedInvoice.curyLineTotal>)
		)]
		protected virtual void APTran_CuryTranAmt_CacheAttached(PXCache sender)
		{
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(RecognizedRecordEntityTypeListAttribute.APDocument)]
		protected virtual void APRecognizedRecord_EntityType_CacheAttached(PXCache sender)
		{
		}

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXDefault(RecognizedRecordEntityTypeListAttribute.APDocument)]
        protected virtual void APRecognizedInvoice_EntityType_CacheAttached(PXCache sender)
        {
        }

        protected virtual void _(Events.FieldUpdated<APRecognizedInvoice.vendorID> e)
        {
			LoadRecognizedData();
        }



		public IEnumerable transactions()
		{
			return Transactions.Cache.Cached;
		}

		[PXSaveButton]
		[PXUIField(DisplayName = ActionsMessages.Save)]
		public void save()
		{
			var overrideFile = Document.Current?.AllowUploadFile;
			if (overrideFile == false)
			{
				return;
			}

			RemoveAttachedFile();
		}

		private void RemoveAttachedFile()
		{
			if (Document.Current?.FileID == null)
			{
				return;
			}

			var fileMaint = CreateInstance<UploadFileMaintenance>();

			var fileLink = (NoteDoc)fileMaint.FileNoteDoc.Select(Document.Current.FileID, Document.Current.NoteID);
			if (fileLink == null)
			{
				return;
			}

			fileMaint.FileNoteDoc.Delete(fileLink);
			fileMaint.Persist();
			PXNoteAttribute.ResetFileListCache(Document.Cache);

			Document.Current.FileID = null;
		}

		[PXCancelButton]
		[PXUIField(DisplayName = ActionsMessages.Cancel)]
		public void cancel()
		{
			var recognizedRecordRefNbr = Document.Current.RecognizedRecordRefNbr;
			var fileId = Document.Current.FileID;

			Clear(PXClearOption.ClearAll);

			Document.Current.RecognizedRecordRefNbr = recognizedRecordRefNbr;
			Document.Current.FileID = fileId;

			LoadRecognizedData();
		}

		[PXUIField(DisplayName = "Save and Continue")]
		[PXButton]
		public void continueSave()
        {
            var invoiceEntryGraph = CreateInstance<APInvoiceEntry>();
            using (var tran = new PXTransactionScope())
            {
                SaveFeedback();

                EnsureTransactions();

                Document.Cache.IsDirty = false;
                Transactions.Cache.IsDirty = false;

                // Clear Details Total. To avoid double calculation after inserting the document into the graph below.
                Document.Current.CuryLineTotal = 0;
                invoiceEntryGraph.SelectTimeStamp();
                InsertInvoiceData(invoiceEntryGraph);
				tran.Complete();
            }
            

            throw new PXRedirectRequiredException(invoiceEntryGraph, false, null);
		}

        private void SaveFeedback()
        {
            var recognizedRecord = RecognizedRecords.Current ?? RecognizedRecords.SelectSingle();
			var sb = new StringBuilder(recognizedRecord.RecognitionFeedback);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            var feedbackList = Document.Current.FeedbackBuilder.ToTableFeedbackList(Transactions.View.Name);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw){Formatting = Formatting.None})
            {
                foreach (var feedbackItem in feedbackList)
                {
                    sb.AppendLine();
                    _jsonSerializer.Serialize(jsonWriter,feedbackItem);
                }
            }
            RecognizedRecords.Cache.SetValue<RecognizedRecord.recognitionFeedback>(recognizedRecord, sw.ToString());
            RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
        }

        [PXButton]
		[PXUIField(DisplayName = "Recognize")]
		public virtual IEnumerable processRecognition(PXAdapter adapter)
		{
			var refNbr = Document.Current.RecognizedRecordRefNbr.Value;
			var fileId = Document.Current.FileID.Value;
			var noteId = Document.Current.NoteID.Value;

            var logger = _logger; //to avoid closing over graph
            PXLongOperation.StartOperation(this, method: () => RecognizeInvoiceData(refNbr, fileId, noteId, logger));
            return adapter.Get();
		}

		[PXButton]
		[PXUIField(DisplayName = "Open Document")]
		public virtual void openDocument()
		{
			Document.Cache.IsDirty = false;
			Transactions.Cache.IsDirty = false;

			var recognizedInvoice = (APInvoice)
				SelectFrom<APInvoice>
				.Where<APInvoice.noteID.IsEqual<@P.AsGuid>>
				.View.ReadOnly
				.SelectSingleBound(this, null, Document.Current.DocumentLink);

			var graph = CreateInstance<APInvoiceEntry>();
			graph.Document.Current = recognizedInvoice;

			throw new PXRedirectRequiredException(graph, null);
		}

		[PXButton]
		[PXUIField(DisplayName = "Open Duplicate Document")]
		public virtual void openDuplicate()
		{
			var duplicatedRecognizedInvoice = (APRecognizedInvoice)
				SelectFrom<APRecognizedInvoice>
				.Where<APRecognizedInvoice.recognizedRecordRefNbr.IsEqual<APRecognizedInvoice.duplicateLink.FromCurrent>>
				.View.ReadOnly
				.SelectSingleBound(this, null);

			var graph = CreateInstance<APInvoiceRecognitionEntry>();
			graph.Document.Current = duplicatedRecognizedInvoice;

			throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		[PXButton]
		[PXUIField]
		public virtual void dumpTableFeedback()
		{
			Document.Current.FeedbackBuilder?.DumpTableFeedback();
		}

		protected virtual void _(Events.RowDeleting<APRecognizedInvoice> e)
		{
			if (e.Row == null)
			{
				return;
			}

			var recognizedRecord = RecognizedRecords.SelectSingle();
			if (recognizedRecord == null)
			{
				return;
			}

			// Keep attachments for invoice
			if (recognizedRecord.Status == RecognizedRecordStatusListAttribute.Processed)
			{
				PXNoteAttribute.ForceRetain<APRecognizedRecord.noteID>(RecognizedRecords.Cache);
				PXNoteAttribute.ForceRetain<APRecognizedInvoice.noteID>(e.Cache);
			}

			RecognizedRecords.Delete(recognizedRecord);
			UpdateDuplicates(recognizedRecord.RefNbr);

			Transactions.Cache.Clear();
		}

		protected virtual void _(Events.RowPersisting<APRecognizedInvoice> e)
		{
			e.Cancel = true;
		}

		// Clear <NEW> value to avoid ValueDoesntExist exception
		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.refNbr> e)
		{
			e.NewValue = null;
			e.Cancel = true;
		}

		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.docType> e)
		{
			e.NewValue = APDocType.Invoice;
		}

		protected virtual void _(Events.FieldDefaulting<APRecognizedInvoice.allowFilesMsg> e)
		{
			e.NewValue = PXMessages.LocalizeFormatNoPrefixNLA(Web.UI.Msg.ErrFileTypesAllowed, PdfExtension);
		}

		protected virtual void _(Events.FieldDefaulting<BAccountR.type> e)
		{
			e.NewValue = BAccountType.VendorType;
		}

		protected virtual void _(Events.RowInserted<APRecognizedInvoice> e)
		{
            if (e.Row != null)
                Caches[typeof(APRegister)].Current = e.Row;
        }

		protected virtual void _(Events.RowSelected<APRecognizedInvoice> e)
		{
			Document.View.SetAnswer(null, WebDialogResult.OK);

            if (!(e.Row is APRecognizedInvoice document)) return;
            if(e.Row.DocType==null||e.Row.CuryInfoID==null)
                DefaultInvoiceValues(document);
            var recognizedRecord = RecognizedRecords.Current??RecognizedRecords.SelectSingle();
            if (recognizedRecord != null)
                e.Row.RecognitionStatus = recognizedRecord.Status;
            if (e.Row.IsRedirect == true)
            {
				if (recognizedRecord != null)
				{
					recognizedRecord.IsDataLoaded = false;
				}

				e.Row.IsRedirect = false;
            }

            if (e.Row.RecognizedRecordRefNbr != null && recognizedRecord?.IsDataLoaded != true)
            {
                RecognizedRecords.Cache.SetValue<APRecognizedRecord.recognitionFeedback>(recognizedRecord, null);
                LoadRecognizedData();
            }

            if (e.Row.NoteID != null)
            {
                ProcessFile(e.Cache, e.Row);
            }

            e.Row.AllowUploadFile = e.Row.RecognitionStatus ==
                                    APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile ||
                                    e.Row.RecognitionStatus ==
                                    RecognizedRecordStatusListAttribute.PendingRecognition ||
                                    e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;

            var showCancel = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            Cancel.SetVisible(showCancel);

			var showDelete = e.Row.RecognitionStatus != APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile;
            Delete.SetVisible(showDelete);

            var showSaveContinue = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized ||
                                   e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            ContinueSave.SetVisible(showSaveContinue);

            var showProcessRecognition = e.Row.FileID != null &&
                                         e.Row.RecognitionStatus ==
                                         RecognizedRecordStatusListAttribute.PendingRecognition ||
                                         e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            ProcessRecognition.SetVisible(showProcessRecognition);

            var showOpenDocument = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Processed;
            OpenDocument.SetVisible(showOpenDocument);

            var showOpenDuplicate = e.Row.DuplicateLink != null;
            OpenDuplicate.SetVisible(showOpenDuplicate);
            if (showOpenDuplicate)
            {
                if (recognizedRecord != null)
                {
                    var duplicate = CheckForDuplicates(recognizedRecord.RefNbr, recognizedRecord.FileHash);
                    var warning = PXMessages.LocalizeFormatNoPrefixNLA(Messages.DuplicateFileForRecognitionTooltip,
                        duplicate.Subject);

                    PXUIFieldAttribute.SetWarning<APRecognizedInvoice.recognitionStatus>(e.Cache, e.Row, warning);
                }
            }

            var allowEdit = e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Recognized ||
                            e.Row.RecognitionStatus == RecognizedRecordStatusListAttribute.Error;
            Document.AllowInsert = allowEdit;
            Document.AllowUpdate = allowEdit;
            Document.AllowDelete = allowEdit;
            Transactions.AllowInsert = allowEdit;
            Transactions.AllowUpdate = allowEdit;
            Transactions.AllowDelete = allowEdit;
        }

		protected virtual void _(Events.FieldUpdated<APRecognizedInvoice.docType> e)
		{
			if (!(e.Args.Row is APRecognizedInvoice row))
			{
				return;
			}

			var docType = row.DocType;

			foreach (APTran tran in Transactions.Select())
			{
				Transactions.Cache.SetValue<APTran.tranType>(tran, docType);
			}
		}

		protected virtual void _(Events.FieldUpdating<BoundFeedback.tableRelated> e)
		{
			var document = Document.Current;

			var unsupportedDocType = !APDocType.Invoice.Equals(document.DocType, StringComparison.Ordinal);
			if (unsupportedDocType)
			{
				return;
			}

			var feedbackBuilder = document.FeedbackBuilder;
			if (feedbackBuilder == null)
			{
				return;
			}

			var cellBoundJsonEncoded = e.NewValue as string;
			if (string.IsNullOrWhiteSpace(cellBoundJsonEncoded))
			{
				return;
			}

			var cellBoundJson = HttpUtility.UrlDecode(cellBoundJsonEncoded);
			feedbackBuilder.ProcessCellBound(cellBoundJson);

			e.NewValue = null;
		}

		protected virtual void _(Events.FieldUpdating<BoundFeedback.fieldBound> e)
		{
			var document = Document.Current;
            var recognizedRecord = RecognizedRecords.Current;
            var unsupportedDocType = !APDocType.Invoice.Equals(document.DocType, StringComparison.Ordinal);
			if (unsupportedDocType)
			{
				return;
			}

			var feedbackBuilder = document.FeedbackBuilder;
			if (feedbackBuilder == null)
			{
				return;
			}

			var documentJsonEncoded = e.NewValue as string;
			if (string.IsNullOrWhiteSpace(documentJsonEncoded))
			{
				return;
			}

			var documentJson = HttpUtility.UrlDecode(documentJsonEncoded);
			var fieldBoundFeedback = feedbackBuilder.ToFieldBoundFeedback(documentJson);
			if (fieldBoundFeedback == null)
			{
				return;
			}

            var sb = new StringBuilder(recognizedRecord.RecognitionFeedback);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw){Formatting = Formatting.None})
            {
                sb.AppendLine();
                _jsonSerializer.Serialize(jsonWriter,fieldBoundFeedback);
            }
            RecognizedRecords.Cache.SetValue<APRecognizedRecord.recognitionFeedback>(recognizedRecord, sw.ToString());
            e.NewValue = null;
		}

		protected virtual void _(Events.RowPersisting<APTran> e)
		{
			e.Cancel = true;
		}

		internal static bool IsAllowedFile(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return false;
			}

			var fileExtension = Path.GetExtension(name);

			return string.Equals(fileExtension, PdfExtension, StringComparison.OrdinalIgnoreCase);
		}

		private void LoadRecognizedData()
		{
			var recognizedRecord = RecognizedRecords.SelectSingle();
			if(recognizedRecord==null)
				return;

			if (string.IsNullOrEmpty(recognizedRecord.RecognitionResult))
			{
				Document.Current.RecognitionStatus = recognizedRecord.Status;
				Document.Current.DuplicateLink = recognizedRecord.DuplicateLink;

				return;
			}

			var recognitionResult = JsonConvert.DeserializeObject<DocumentRecognitionResult>(recognizedRecord.RecognitionResult);

			LoadRecognizedDataToGraph(this, recognizedRecord, recognitionResult);
		}

		private void ProcessFile(PXCache cache, APRecognizedInvoice invoice)
        {
            var fileNotes = PXNoteAttribute.GetFileNotes(cache, invoice);

			if (fileNotes == null || fileNotes.Length == 0)
			{
				if (invoice.FileID != null)
				{
					RemoveAttachedFile();
					UpdateFileInfo(null);

					invoice.FileID = null;
				}

				return;
			}

			var fileId = fileNotes[0];
			var file = GetFile(this, fileId);

			if (invoice.RecognitionStatus == APRecognizedInvoiceRecognitionStatusListAttribute.PendingFile)
			{
				invoice.RecognitionStatus = RecognizedRecordStatusListAttribute.PendingRecognition;

				var recognizedRecord = CreateRecognizedRecord(file.Name, file.Data, invoice);

                invoice.EntityType = recognizedRecord.EntityType;
                invoice.FileHash = recognizedRecord.FileHash;
                invoice.RecognitionStatus = recognizedRecord.Status;
				invoice.DuplicateLink = recognizedRecord.DuplicateLink;
			}
			else if (invoice.FileID != null && invoice.FileID != fileId)
			{
				UpdateFileInfo(file);
			}

			invoice.FileID = fileId;

			// To load restricted file by page via GetFile.ashx
			var fileInfoInMemory = new PX.SM.FileInfo(fileId, file.Name, null, file.Data);
			PXContext.SessionTyped<PXSessionStatePXData>().FileInfo[fileInfoInMemory.UID.ToString()] = fileInfoInMemory;
		}

        

        private void UpdateFileInfo(UploadFile file)
		{
			var recognizedRecord = RecognizedRecords.SelectSingle();
			if (recognizedRecord == null)
			{
				return;
			}

			if (file == null)
			{
				recognizedRecord.Subject = null;
				recognizedRecord.FileHash = null;
				recognizedRecord.DuplicateLink = null;
			}
			else
			{
				recognizedRecord.Subject = GetRecognizedSubject(null, file.Name);
				recognizedRecord.FileHash = ComputeFileHash(file.Data);

				SetDuplicateInfo(recognizedRecord);
			}

			recognizedRecord.Owner = PXAccess.GetContactID();

			Caches[typeof(Note)].Clear();
			RecognizedRecords.Cache.PersistUpdated(recognizedRecord);
			RecognizedRecords.Cache.Clear();
			RecognizedRecords.Cache.IsDirty = false;
			SelectTimeStamp();
		}

		public RecognizedRecord CreateRecognizedRecord(string fileName, byte[] fileData, string description = null, string mailFrom = null, string messageId = null,
			int? owner = null, Guid? noteId = null)
		{
			fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
			fileData.ThrowOnNull(nameof(fileData));

			var recognizedRecord = RecognizedRecords.Insert();
			var originalFileName = PX.SM.FileInfo.GetShortName(fileName);

			recognizedRecord.Subject = description ?? GetRecognizedSubject(null, originalFileName);
			recognizedRecord.MailFrom = mailFrom;
			recognizedRecord.MessageID = string.IsNullOrWhiteSpace(messageId) ?
				messageId :
				NormalizeMessageId(messageId);
			recognizedRecord.FileHash = ComputeFileHash(fileData);
			recognizedRecord.Owner = owner ?? PXAccess.GetContactID();

			if (noteId != null)
			{
				recognizedRecord.NoteID = noteId;
			}

			SetDuplicateInfo(recognizedRecord);

			//Caches[typeof(Note)].Clear();
			RecognizedRecords.Cache.PersistInserted(recognizedRecord);
			//RecognizedRecords.Cache.Clear();
			//RecognizedRecords.Cache.IsDirty = false;
			//SelectTimeStamp();

			return recognizedRecord;
		}

        private RecognizedRecord CreateRecognizedRecord(string fileName, byte[] fileData, APRecognizedInvoice recognizedInvoice)
        {
            fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
            fileData.ThrowOnNull(nameof(fileData));
            var recognizedRecord = (RecognizedRecord) RecognizedRecords.Cache.CreateInstance();
            recognizedRecord.NoteID = recognizedInvoice.NoteID;
            recognizedRecord.CustomInfo = recognizedInvoice.CustomInfo;
            recognizedRecord.DocumentLink = recognizedInvoice.DocumentLink;
            recognizedRecord.DuplicateLink = recognizedInvoice.DuplicateLink;
            recognizedRecord.EntityType = recognizedInvoice.EntityType;
            recognizedRecord.FileHash = ComputeFileHash(fileData);
            recognizedRecord.MailFrom = recognizedInvoice.MailFrom;
            recognizedRecord.MessageID = recognizedInvoice.MessageID;
            recognizedRecord.Owner = recognizedInvoice.Owner;
            recognizedRecord.RecognitionResult = recognizedInvoice.RecognitionResult;
            recognizedRecord.RecognitionStarted = recognizedInvoice.RecognitionStarted;
            recognizedRecord.RefNbr = recognizedInvoice.RecognizedRecordRefNbr;
            recognizedRecord.Status = recognizedInvoice.RecognitionStatus;
            var originalFileName = PX.SM.FileInfo.GetShortName(fileName);
            recognizedRecord.Subject = GetRecognizedSubject(null, originalFileName);
            SetDuplicateInfo(recognizedRecord);
            recognizedRecord = (RecognizedRecord)RecognizedRecords.Cache.Insert(recognizedRecord);
            RecognizedRecords.Cache.PersistInserted(recognizedRecord);
            return recognizedRecord;
        }

		internal static byte[] ComputeFileHash(byte[] data)
		{
			using (var provider = new MD5CryptoServiceProvider())
			{
				return provider.ComputeHash(data);
			}
		}

		private void SetDuplicateInfo(RecognizedRecord recognizedRecord)
		{
			var duplicate = CheckForDuplicates(recognizedRecord.RefNbr, recognizedRecord.FileHash);

			if (duplicate.RefNbr == null)
			{
				return;
			}

			recognizedRecord.DuplicateLink = duplicate.RefNbr;
		}

		private void EnsureTransactions()
		{
			var detailsNotEmpty = Transactions.Cache.Cached
				.Cast<object>()
				.Any();
			if (detailsNotEmpty)
			{
                foreach (APTran tran in Transactions.Cache.Cached)
                {
                    var documentCuryInfo = Document.Current?.CuryInfoID;
                    if(tran.CuryInfoID!=documentCuryInfo)
                        Transactions.Cache.SetValue<APTran.curyInfoID>(tran, documentCuryInfo);
                }
				return;
			}

			var document = Document.Current;
			if (document == null)
			{
				return;
			}

			var summaryDetail = Transactions.Insert();
			if (summaryDetail == null)
			{
				return;
			}

			summaryDetail.TranDesc = document.DocDesc;
			summaryDetail.CuryLineAmt = document.CuryOrigDocAmt;

			Transactions.Update(summaryDetail);
		}

		private void InsertInvoiceData(APInvoiceEntry graph)
		{
			graph.Document.Insert(Document.Current);

			var invoiceEntryExt = graph.GetExtension<APInvoiceEntryExt>();
			invoiceEntryExt.FeedbackParameters.Current.FeedbackBuilder = Document.Current.FeedbackBuilder;
			invoiceEntryExt.FeedbackParameters.Current.Links = Document.Current.Links;
			invoiceEntryExt.FeedbackParameters.Current.FeedbackFileID = Document.Current.FileID;

			var defaultCurrencyInfo = Caches[typeof(CurrencyInfo)].Current as CurrencyInfo;
			graph.currencyinfo.Insert(defaultCurrencyInfo);

			foreach (var t in Transactions.Select())
			{
				graph.Transactions.Insert(t);
			}

			var recognizedRecord = PXSelect<RecognizedRecord, Where<RecognizedRecord.refNbr, Equal<PX.Data.Required<RecognizedRecord.refNbr>>>>.Select(graph, Document.Current.RecognizedRecordRefNbr).FirstTableItems.FirstOrDefault();
			if(recognizedRecord==null)
				return;
            recognizedRecord.DocumentLink = Document.Current.NoteID;
			recognizedRecord.Status = RecognizedRecordStatusListAttribute.Processed;

			var recognizedRecordCache = graph.Caches[typeof(RecognizedRecord)];
			recognizedRecordCache.Update(recognizedRecord);

			RecognizedRecords.View.Clear();
		}

		internal static string NormalizeMessageId(string rawMessageId)
		{
			rawMessageId.ThrowOnNullOrWhiteSpace(nameof(rawMessageId));

			var braceIndex = rawMessageId.IndexOf('>');
			if (braceIndex == -1 || braceIndex == rawMessageId.Length - 1)
			{
				return rawMessageId;
			}

			return rawMessageId.Substring(0, braceIndex + 1);
		}

		private static string GetRecognizedSubject(string emailSubject, string fileName)
		{
			if (string.IsNullOrWhiteSpace(emailSubject))
			{
				return fileName;
			}

			return $"{emailSubject}: {fileName}";
		}

		public (Guid? RefNbr, string Subject) CheckForDuplicates(Guid? recognizedRefNbr, byte[] fileHash)
		{
			var duplicateRecord = (RecognizedRecord)
				SelectFrom<RecognizedRecord>
				.Where<RecognizedRecord.refNbr.IsNotEqual<@P.AsGuid>.And<
					   RecognizedRecord.fileHash.IsEqual<@P.AsByteArray>>>
				.OrderBy<RecognizedRecord.createdDateTime.Asc>
				.View.ReadOnly.Select(this, recognizedRefNbr, fileHash);

			if (duplicateRecord == null)
			{
				return (null, null);
			}

			return (duplicateRecord.RefNbr, duplicateRecord.Subject);
		}

		public void UpdateDuplicates(Guid? refNbr)
		{
			var duplicatesView = new SelectFrom<APRecognizedRecord>
				.Where<RecognizedRecord.duplicateLink.IsEqual<@P.AsGuid>>
				.OrderBy<RecognizedRecord.createdDateTime.Asc>
				.View(this);
			var newDuplicateLink = default(Guid?);

			foreach (APRecognizedRecord record in duplicatesView.Select(refNbr))
			{
				if (newDuplicateLink == null)
				{
					newDuplicateLink = record.RefNbr;
					record.DuplicateLink = null;
				}
				else
				{
					record.DuplicateLink = newDuplicateLink;
				}

				RecognizedRecords.Update(record);
			}
		}

		public static void RecognizeInvoiceData(Guid recognizedRecordRefNbr, Guid fileId, Guid noteId, Serilog.ILogger logger)
		{
			var graph = CreateInstance<APInvoiceRecognitionEntry>();
            var document = (APRecognizedInvoice)graph.Document.Cache.CreateInstance();
            document.RecognizedRecordRefNbr = recognizedRecordRefNbr;
            int maxrows = 1;
            int startrow = 0;
            var result = graph.Document.View.Select(null, null, new object[] {recognizedRecordRefNbr},
                new string[] {nameof(APRecognizedInvoice.recognizedRecordRefNbr)}, new[] {false}, null, ref startrow, 1, ref maxrows).FirstOrDefault();
            if (result!=null)
                graph.Document.Current = result as APRecognizedInvoice;
			var recognizedRecord = graph.RecognizedRecords.SelectSingle();

			document.RecognitionStatus = recognizedRecord.Status;

			try
			{
				var file = GetFile(graph, fileId);

				if (!IsAllowedFile(file.Name))
				{
					var message = PXMessages.LocalizeFormatNoPrefixNLA(Messages.InvalidFileForRecognition, PdfExtension);

					throw new PXArgumentException(nameof(file), message);
				}

				if (recognizedRecord.RecognitionStarted != true)
				{
					MarkRecognitionStarted(graph, recognizedRecord);
				}

                DocumentRecognitionResult recognitionResult = null;
                try
                {
                    recognitionResult = GetRecognitionInfo(graph.InvoiceRecognitionClient, file, logger).Result;
                }
                finally
                {
                    UpdateRecognizedRecord(graph, recognizedRecord, recognitionResult);
                }

    //            LoadRecognizedDataToGraph(graph, recognizedRecord, recognitionResult);

				//graph.Document.Cache.IsDirty = true;
				//graph.Transactions.Cache.IsDirty = true;
			}
			catch
			{
				document.RecognitionStatus = RecognizedRecordStatusListAttribute.Error;

				throw;
			}
			//finally
			//{
			//	PXLongOperation.SetCustomInfo(graph);
			//}
		}

		private static UploadFile GetFile(PXGraph graph, Guid fileId)
		{
			var result = (PXResult<UploadFile, UploadFileRevision>)
				PXSelectJoin<UploadFile,
				InnerJoin<UploadFileRevision,
				On<UploadFile.fileID, Equal<UploadFileRevision.fileID>, And<
				   UploadFile.lastRevisionID, Equal<UploadFileRevision.fileRevisionID>>>>,
				Where<UploadFile.fileID, Equal<Required<UploadFile.fileID>>>>.Select(graph, fileId);
			if (result == null)
			{
				return null;
			}

			var file = (UploadFile)result;
			var fileRevision = (UploadFileRevision)result;

			file.Data = fileRevision.Data;

			return file;
		}

		private static void MarkRecognitionStarted(APInvoiceRecognitionEntry graph, APRecognizedRecord record)
		{
			record.RecognitionStarted = true;
			record.Status = RecognizedRecordStatusListAttribute.InProgress;

			graph.RecognizedRecords.Update(record);
			graph.Persist();
		}

		private static void UpdateRecognizedRecord(APInvoiceRecognitionEntry graph, APRecognizedRecord record,
			DocumentRecognitionResult recognitionResult)
		{
			var isError = recognitionResult == null;

			record.Status = isError ?
				RecognizedRecordStatusListAttribute.Error :
				RecognizedRecordStatusListAttribute.Recognized;
			record.RecognitionResult = JsonConvert.SerializeObject(recognitionResult);

			record = graph.RecognizedRecords.Update(record);
			graph.RecognizedRecords.Cache.PersistUpdated(record);
		}

        private static async Task<DocumentRecognitionResult> GetRecognitionInfo(IDocumentRecognitionClient client,
			UploadFile file, Serilog.ILogger logger)
		{
            var extension = Path.GetExtension(file.Name);
            var mimeType = MimeTypes.GetMimeType(extension);

            using (var op = logger.OperationAt(LogEventLevel.Verbose, LogEventLevel.Error)
                .Begin("Recognizing document"))
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(_recognitionTimeoutMinutes)))
                {
                    try
                    {
                        var result = await client.Recognize(file.FileID.Value, file.Data, mimeType, _documentRecognitionModel, logger, cancellationTokenSource.Token);
						op.Complete();
                        return result;
                    }
                    catch (Exception e)
                    {
                        op.SetException(e);
						throw e;
                    }
                }
            }
        }

        private static void LoadRecognizedDataToGraph(APInvoiceRecognitionEntry graph, RecognizedRecord record,
			DocumentRecognitionResult recognitionResult)
		{
			var document = graph.Document.Current;
            var cache = graph.Document.Cache;
            cache.SetValue<APRecognizedInvoice.recognitionStatus>(document, record.Status);
            cache.SetValue<APRecognizedInvoice.duplicateLink>(document, record.DuplicateLink);
            cache.SetValue<APRecognizedInvoice.recognizedDataJson>(document, HttpUtility.UrlEncode(record.RecognitionResult));
            cache.SetValue<APRecognizedInvoice.noteID>(document, record.NoteID);
            graph.RecognizedRecords.Cache.SetValue<APRecognizedRecord.isDataLoaded>(record, true);

			if (recognitionResult == null)
			{
				return;
			}

			// To avoid double calculation
			cache.SetValue<APRecognizedInvoice.curyLineTotal>(document, decimal.Zero);
			var invoiceDataLoader = new InvoiceDataLoader(recognitionResult, graph);
			invoiceDataLoader.Load(document);

			var vendorTermJson = JsonConvert.SerializeObject(invoiceDataLoader.VendorTerm);
			var vendorTermEncodedJson = HttpUtility.UrlEncode(vendorTermJson);

            cache.SetValue<APRecognizedInvoice.vendorTermJson>(document, vendorTermEncodedJson);
			document.FeedbackBuilder = graph.GetFeedbackBuilder();
			document.Links = recognitionResult.Links;

			graph.Document.Cache.IsDirty = false;
			graph.Transactions.Cache.IsDirty = false;
		}

		private FeedbackBuilder GetFeedbackBuilder()
		{
			var siteMapNode = PXSiteMap.Provider.FindSiteMapNodesByGraphType(GetType().FullName).FirstOrDefault();
			if (siteMapNode == null)
			{
				return null;
			}

			var screenInfo = ScreenInfoProvider.GetWithInvariantLocale(siteMapNode.ScreenID);
			if (screenInfo == null)
			{
				return null;
			}

			if (!screenInfo.Containers.TryGetValue(nameof(Document), out var primaryContainer))
			{
				return null;
			}

			var primaryFields = primaryContainer.Fields
				.Select(f => f.FieldName)
				.ToHashSet();

			if (!screenInfo.Containers.TryGetValue(nameof(Transactions), out var detailContainer))
			{
				return null;
			}

			var detailFields = detailContainer.Fields
				.Select(f => f.FieldName)
				.ToHashSet();

			return new FeedbackBuilder(Document.Cache, primaryFields, detailFields);
		}

		internal static void RecognizeFile(string fileName, byte[] fileData, Guid? fileId = null, string subject = null,
			string mailFrom = null, string messageId = null, int? ownerId = null)
		{
			fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
			fileData.ThrowOnNull(nameof(fileData));

			if (fileId == null)
			{
				fileId = Guid.NewGuid();

				var fileInfoDb = new PX.SM.FileInfo(fileId.Value, fileName, null, fileData);

				var uploadFileGraph = CreateInstance<UploadFileMaintenance>();
				if (!uploadFileGraph.SaveFile(fileInfoDb))
				{
					throw new PXException(Messages.FileCannotBeSaved, fileName);
				}
			}

			fileName = PX.SM.FileInfo.GetShortName(fileName);
			subject = GetRecognizedSubject(subject, fileName);

			if (!string.IsNullOrWhiteSpace(messageId))
			{
				messageId = NormalizeMessageId(messageId);
			}

			var recognitionGraph = CreateInstance<APInvoiceRecognitionEntry>();
			var recognizedRecord = recognitionGraph.CreateRecognizedRecord(fileName, fileData, subject, mailFrom,
				messageId, ownerId);

			PXNoteAttribute.ForcePassThrow<RecognizedRecord.noteID>(recognitionGraph.RecognizedRecords.Cache);
			PXNoteAttribute.SetFileNotes(recognitionGraph.RecognizedRecords.Cache, recognizedRecord, fileId.Value);

			var refNbr = recognizedRecord.RefNbr.Value;
			var noteId = recognizedRecord.NoteID.Value;

			PXLongOperation.StartOperation(recognitionGraph, () => RecognizeInvoiceData(refNbr, fileId.Value, noteId, PXTrace.Logger));
		}

		internal static bool IsRecognitionInProgress(string messageId)
		{
			messageId.ThrowOnNullOrWhiteSpace(nameof(messageId));
			messageId = NormalizeMessageId(messageId);

			using (var record = PXDatabase.SelectSingle<RecognizedRecord>(
				new PXDataField<RecognizedRecord.refNbr>(),
				new PXDataFieldValue<RecognizedRecord.messageID>(messageId),
				new PXDataFieldValue<RecognizedRecord.status>(RecognizedRecordStatusListAttribute.InProgress)))
			{
				return record != null;
			}
		}

		internal static string GetRecognitionStatus(string messageId, string fileName, string subject)
		{
			messageId.ThrowOnNullOrWhiteSpace(nameof(messageId));
			fileName.ThrowOnNullOrWhiteSpace(nameof(fileName));
			subject.ThrowOnNullOrWhiteSpace(nameof(subject));

			messageId = NormalizeMessageId(messageId);
			subject = GetRecognizedSubject(subject, fileName);

			using (var record = PXDatabase.SelectSingle<RecognizedRecord>(
				new PXDataField<RecognizedRecord.status>(),
				new PXDataFieldValue<RecognizedRecord.messageID>(messageId),
				new PXDataFieldValue<RecognizedRecord.subject>(subject)))
			{
				return record?.GetString(0);
			}
		}

		internal static bool RecognizeInvoices(PXGraph graph, CRSMEmail message)
		{
			graph.ThrowOnNull(nameof(graph));
			message.ThrowOnNull(nameof(message));

			var cache = graph.Caches[typeof(CRSMEmail)];

			var allFiles = PXNoteAttribute.GetFileNotes(cache, message);
			if (allFiles == null || allFiles.Length == 0)
			{
				return false;
			}

			var filesToProcess = allFiles
				.Select(fileId => GetFile(graph, fileId))
				.Where(uploadFile => IsAllowedFile(uploadFile.Name))
				.ToArray();

			if (filesToProcess.Length == 0)
			{
				return false;
			}

			foreach (var file in filesToProcess)
			{
				RecognizeFile(file.Name, file.Data, file.FileID,
					message.Subject, message.MailFrom, message.MessageId, message.OwnerID);
			}

			return true;
		}
	}
}
