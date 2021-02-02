using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.PhotoLogs.Descriptor;
using PX.Objects.PJ.PhotoLogs.PJ.DAC;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.Common.Extensions;
using PX.SM;
using Constants = PX.Objects.PJ.Common.Descriptor.Constants;

namespace PX.Objects.PJ.PhotoLogs.PJ.Services
{
    public abstract class PhotoLogZipServiceBase : BaseZipService<PhotoLog>
    {
        protected readonly PXGraph Graph;

        protected PhotoLogZipServiceBase(PXGraph graph)
            : base(graph.Caches<PhotoLog>())
        {
            Graph = graph;
        }

        protected PhotoLog CurrentPhotoLog => Graph.Caches<PhotoLog>().Current as PhotoLog;

        protected override string ZipFileName => GetPhotoLogZipFileName(CurrentPhotoLog);

        protected override string NoDocumentsSelectedMessage => PhotoLogMessages.NoRecordsWereSelected;

        protected override string NoAttachedFilesInDocumentsMessage => PhotoLogMessages.NoPhotosInSelectedPhotoLogs;

        public override void DownloadZipFile(PhotoLog document)
        {
            var fileIds = GetFileIdsIfAttachedExist(document);
            CreateZipArchive(fileIds, AddDocumentsFilesToZipArchive);
        }

        public abstract void DownloadPhotoLogZip();

        public abstract FileInfo GetPhotoLogZip();

        protected override IEnumerable<Guid> GetFileIdsOfDocument(PhotoLog document)
        {
            var photosNotes = GetPhotoNotes(document.PhotoLogId);
            var photoLogNotes = PXNoteAttribute.GetFileNotes(Cache, document);
            var fileIds = photosNotes.Select(p => p.FileID.GetValueOrDefault()).ToList();
            return fileIds.IsEmpty()
                ? Enumerable.Empty<Guid>()
                : photoLogNotes.Concat(fileIds);
        }

        protected virtual string GetPhotoLogZipFileName(PhotoLog photoLog)
        {
            if (photoLog == null)
            {
                return string.Empty;
            }
            var date = photoLog.Date.GetValueOrDefault().ToString(Constants.FilesDateFormat);
            var projectDataProvider = Graph.GetService<IProjectDataProvider>();
            var projectCd = projectDataProvider.GetProject(Graph, photoLog.ProjectId)?.ContractCD;
            return $"Photo log #{photoLog.PhotoLogCd} {date} {projectCd}.zip";
        }

        private IEnumerable<NoteDoc> GetPhotoNotes(int? photoLogId)
        {
            return SelectFrom<NoteDoc>
                .InnerJoin<Photo>.On<NoteDoc.noteID.IsEqual<Photo.noteID>>
                .Where<Photo.photoLogId.IsEqual<P.AsInt>>.View.Select(Cache.Graph, photoLogId).FirstTableItems;
        }
    }
}