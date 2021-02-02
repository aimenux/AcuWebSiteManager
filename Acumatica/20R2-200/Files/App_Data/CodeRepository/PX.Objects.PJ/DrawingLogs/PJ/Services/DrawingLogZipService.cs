using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.Descriptor;
using PX.Objects.PJ.DrawingLogs.PJ.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.DrawingLogs.PJ.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;

namespace PX.Objects.PJ.DrawingLogs.PJ.Services
{
    public class DrawingLogZipService : BaseZipService<DrawingLog>
    {
        private readonly bool? shouldUseOnlyCurrentFiles;

        public DrawingLogZipService(PXCache cache, bool? shouldUseOnlyCurrentFiles)
            : base(cache)
        {
            this.shouldUseOnlyCurrentFiles = shouldUseOnlyCurrentFiles;
        }

        protected override string ZipFileName => Constants.DrawingLogsZipFileName;

        protected override string NoDocumentsSelectedMessage => DrawingLogMessages.NoRecordsWereSelected;

        protected override string NoAttachedFilesInDocumentMessage => DrawingLogMessages.NoAttachedFilesAreAvailableForDrawingLog;

        protected override string NoAttachedFilesInDocumentsMessage => DrawingLogMessages.NoAttachedFilesAreAvailableForSelectedDrawingLogs;

        protected override IEnumerable<Guid> GetFileIdsOfDocument(DrawingLog document)
        {
            var fileIds = PXNoteAttribute.GetFileNotes(Cache, document).ToList();
            return shouldUseOnlyCurrentFiles == true
                ? GetFileIdsWithCurrentValue(fileIds)
                : fileIds;
        }

        private IEnumerable<Guid> GetFileIdsWithCurrentValue(IEnumerable<Guid> fileIds)
        {
            return SelectFrom<UploadFileRevision>
                .Where<UploadFileRevision.fileID.IsIn<P.AsGuid>
                    .And<UploadFileRevisionExt.isDrawingLogCurrentFile.IsEqual<True>>>.View
                .Select(Cache.Graph, fileIds.ToArray()).FirstTableItems
                .Select(revision => revision.FileID.GetValueOrDefault());
        }
    }
}
