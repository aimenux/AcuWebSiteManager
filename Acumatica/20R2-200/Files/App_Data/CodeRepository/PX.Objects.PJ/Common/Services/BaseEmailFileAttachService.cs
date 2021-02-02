using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.Services;
using PX.Objects.PJ.RequestsForInformation.CR.Services;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.PJ.Common.GraphExtensions;
using PX.SM;

namespace PX.Objects.PJ.Common.Services
{
	public abstract class BaseEmailFileAttachService<TEntity>: BaseEmailFileAttachService
		where TEntity : class, IBqlTable, new()
	{
		public BaseEmailFileAttachService(CREmailActivityMaint graph) : base(graph)
		{

		}

		public TEntity GetParentEntity()
		{
			var refNoteId = Graph.Message.Current.RefNoteID;
			return (TEntity)new EntityHelper(Graph).GetEntityRow(refNoteId);
		}
	}

	public abstract class BaseEmailFileAttachService
	{
        protected readonly CREmailActivityMaint Graph;
        protected readonly EmailActivityDataProvider EmailActivityDataProvider;
        protected readonly DrawingLogDataProvider DrawingLogDataProvider;

        protected BaseEmailFileAttachService(CREmailActivityMaint graph)
        {
            Graph = graph;
            EmailActivityDataProvider = new EmailActivityDataProvider(graph);
            DrawingLogDataProvider = new DrawingLogDataProvider(graph);
        }

        public virtual void FillRequiredFields(NoteDoc file)
        {
            SetFileName(file);
            SetFileStatus(file);
        }

        public IEnumerable<NoteDoc> GetAttachedFiles()
        {
            var alreadyLinkedFiles = GetAlreadyLinkedFiles().ToList();
            var alreadyLinkedFileIds = alreadyLinkedFiles.Select(noteDoc => noteDoc.FileID);
            var notAttachedFiles = GetFilesLinkedToRelatedEntities()
                .Where(noteDoc => !alreadyLinkedFileIds.Contains(noteDoc.FileID));
            return alreadyLinkedFiles.Concat(notAttachedFiles);
        }

        public void MaintainFilesReferences(PXSelect<NoteDoc> attachedFilesView)
        {
            var files = (IEnumerable<NoteDoc>) attachedFilesView.Cache.Updated;
            foreach (var file in files)
            {
                var fileExtension = PXCache<NoteDoc>.GetExtension<NoteDocExt>(file);
                if (fileExtension.IsAttached.GetValueOrDefault())
                {
                    AddFileEmailLink(file, fileExtension, attachedFilesView);
                }
                else
                {
                    RemoveFileEmailLink(file, attachedFilesView);
                }
            }
        }

        public void AttachDrawingLogArchive(UploadFileMaintenance uploadFileMaintenance)
        {
            var fileNoteIds = PXNoteAttribute.GetFileNotes(Graph.Message.Cache, Graph.Message.Current).ToList();
            var drawingLogFileNotes = DrawingLogDataProvider.GetDrawingLogFileIds(fileNoteIds).ToList();
            if (drawingLogFileNotes.Any())
            {
                var fileName = GetZipFileFullName();
                var zipArchiveFile = DrawingLogZipService.GetZipFile(drawingLogFileNotes, fileName);
                uploadFileMaintenance.SaveFile(zipArchiveFile);
                PXNoteAttribute.SetFileNotes(Graph.Message.Cache,
                    Graph.Message.Current, zipArchiveFile.UID.GetValueOrDefault().SingleToArray());
                DeleteDrawingLogFiles(drawingLogFileNotes, uploadFileMaintenance);
            }
        }

        protected abstract IEnumerable<NoteDoc> GetFilesLinkedToRelatedEntities();

        private void SetFileName(NoteDoc file)
        {
            var uploadFile = EmailActivityDataProvider.GetFile(file.FileID);
            PXCache<NoteDoc>.GetExtension<NoteDocExt>(file).FileName =
                PXCache<UploadFile>.GetExtension<UploadFileExt>(uploadFile).FileName;
        }

        private void SetFileStatus(NoteDoc file)
        {
            var uploadFileRevision = EmailActivityDataProvider.GetFileRevision(file.FileID);
            var uploadFileRevisionCache = PXCache<UploadFileRevision>
                .GetExtension<UploadFileRevisionExt>(uploadFileRevision);
            PXCache<NoteDoc>.GetExtension<NoteDocExt>(file).IsDrawingLogCurrentFile = uploadFileRevisionCache.IsDrawingLogCurrentFile;
        }

        private List<NoteDoc> GetAlreadyLinkedFiles()
        {
	        var filesIds = PXNoteAttribute.GetFileNotes(Graph.Message.Cache, Graph.Message.Current);
	        if (!filesIds.Any())
		        return new List<NoteDoc>();

	        List<NoteDoc> noteDocs = EmailActivityDataProvider.GetFilesLinkedToEmail(Graph.Message.Current.NoteID, filesIds).ToList();

			var cache = Graph.FindImplementation<CrEmailActivityMaintExt>().AttachedFiles.Cache;

			foreach (NoteDoc noteDoc in noteDocs)
			{
				cache.SetValueExt<NoteDocExt.isAttached>(noteDoc, true);
			}

			return noteDocs;
        }

        private void AddFileEmailLink(NoteDoc file, NoteDocExt fileExtension, PXSelect<NoteDoc> attachedFilesView)
        {
            var reference = CreateReference(file);
            var referenceExtension = attachedFilesView.Cache.GetExtension<NoteDocExt>(reference);
            referenceExtension.IsAttached = true;
            attachedFilesView.Update(reference);
            fileExtension.IsAttached = false;
            attachedFilesView.Update(file);
        }

        private void RemoveFileEmailLink(NoteDoc file, PXSelect<NoteDoc> attachedFilesView)
        {
            var reference = EmailActivityDataProvider.GetFileReference(file.FileID, Graph.Message.Current.NoteID);
            attachedFilesView.Delete(reference);
        }

        private NoteDoc CreateReference(NoteDoc file)
        {
            return new NoteDoc
            {
                FileID = file.FileID,
                NoteID = Graph.Message.Current.NoteID
            };
        }

        private void DeleteDrawingLogFiles(IEnumerable<Guid> drawingLogFileNotes,
            UploadFileMaintenance uploadFileMaintenance)
        {
            var drawingLogFilesToDelete = EmailActivityDataProvider
                .GetFilesLinkedToEmail(Graph.Message.Current.NoteID, drawingLogFileNotes);
            uploadFileMaintenance.Caches<NoteDoc>().DeleteAll(drawingLogFilesToDelete);
            uploadFileMaintenance.Caches<NoteDoc>().Persist(PXDBOperation.Delete);
        }

        private string GetZipFileFullName()
        {
            return $"{Graph.Message.Current.NoteID}\\{Constants.DrawingLogsZipFileName}";
        }
    }
}