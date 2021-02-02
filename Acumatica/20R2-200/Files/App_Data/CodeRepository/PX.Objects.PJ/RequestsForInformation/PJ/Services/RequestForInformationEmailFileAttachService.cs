using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.Common.Services;
using PX.Objects.PJ.DrawingLogs.CR.CacheExtensions;
using PX.Objects.PJ.DrawingLogs.PJ.DAC;
using PX.Objects.PJ.RequestsForInformation.CR.CacheExtensions;
using PX.Objects.PJ.RequestsForInformation.CR.Services;
using PX.Objects.PJ.RequestsForInformation.Descriptor;
using PX.Objects.PJ.RequestsForInformation.PJ.DAC;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.PM;
using PX.SM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Services
{
    public class RequestForInformationEmailFileAttachService : EmailFileAttachService<RequestForInformation>
    {
        public RequestForInformationEmailFileAttachService(CREmailActivityMaint graph)
            : base(graph, new RequestForInformationReportGenerator(graph.Message.Current, graph.Message.Cache))
        {
        }

        public override void FillRequiredFields(NoteDoc file)
        {
            base.FillRequiredFields(file);
            SetFileSource(file);
            SetDrawingLogFields(file);
        }

        public override FileInfo GenerateAndAttachReport(RequestForInformation document, UploadFileMaintenance uploadFileMaintenance)
        {
            var file = base.GenerateAndAttachReport(document, uploadFileMaintenance);

            // attach generated report to RFI
            var fileId = file.UID.GetValueOrDefault();
            PXNoteAttribute.SetFileNotes(Graph.Caches<RequestForInformation>(), document, fileId.SingleToArray());
            Graph.Persist();
            Graph.Caches<RequestForInformation>().Clear();

            return file;
        }

        protected override IEnumerable<NoteDoc> GetFilesLinkedToRelatedEntities()
        {
            var requestForInformationFiles =
                EmailActivityDataProvider.GetFileNotesAttachedToEntity(Graph.Message.Current.RefNoteID);
            RequestForInformation requestForInformation = GetParentEntity();
            var drawingLogFiles = GetDrawingLogFiles(requestForInformation.RequestForInformationId);
            var projectFiles = GetProjectFiles(requestForInformation.RequestForInformationId);
            return requestForInformationFiles.Concat(projectFiles).Concat(drawingLogFiles);
        }

        private void SetFileSource(NoteDoc file)
        {
            var fileExtension = PXCache<NoteDoc>.GetExtension<NoteDocRequestForInformationExt>(file);
            if (file.NoteID == Graph.Message.Current.RefNoteID)
            {
                fileExtension.FileSource = EmailActivityFileSource.RequestForInformation;
            }
            else if (DrawingLogDataProvider.IsAttachedToEntity<PMProject, PMProject.noteID>(file.FileID))
            {
                fileExtension.FileSource = EmailActivityFileSource.Project;
            }
            else if (DrawingLogDataProvider.IsAttachedToEntity<DrawingLog, DrawingLog.noteID>(file.FileID))
            {
                fileExtension.FileSource = EmailActivityFileSource.DrawingLogs;
            }
            else
            {
                fileExtension.FileSource = EmailActivityFileSource.Email;
            }
        }

        private void SetDrawingLogFields(NoteDoc file)
        {
            var fileExtension = PXCache<NoteDoc>.GetExtension<NoteDocRequestForInformationExt>(file);
            if (fileExtension.FileSource == EmailActivityFileSource.DrawingLogs)
            {
                var drawingLog = DrawingLogDataProvider.GetDrawingLog(file.FileID);
                if (drawingLog != null)
                {
                    var drawingLogExtension = PXCache<NoteDoc>.GetExtension<NoteDocDrawingLogExt>(file);
                    drawingLogExtension.DrawingLogCd = drawingLog.DrawingLogCd;
                    drawingLogExtension.Number = drawingLog.Number;
                    drawingLogExtension.Revision = drawingLog.Revision;
                }
            }
        }

        private IEnumerable<NoteDoc> GetProjectFiles(int? requestForInformationId)
        {
            var relatedProject =
                RequestForInformationDataProvider.GetRelatedProject(Graph, requestForInformationId);
            return EmailActivityDataProvider
                .GetFileNotesAttachedToEntity(relatedProject.NoteID);
        }

        private IEnumerable<NoteDoc> GetDrawingLogFiles(int? requestForInformationId)
        {
            var requestForInformationDrawingLogs =
                DrawingLogDataProvider.GetRequestForInformationDrawingLogs(requestForInformationId);
            return requestForInformationDrawingLogs.SelectMany(drawingLog =>
                EmailActivityDataProvider.GetFileNotesAttachedToEntity(drawingLog.NoteID));
        }

        #region ReportGenerator
        public class RequestForInformationReportGenerator : ReportGeneratorBase<RequestForInformation>
        {
            public RequestForInformationReportGenerator(CRSMEmail email, PXCache emailCache)
                : base(email, emailCache)
            {
            }

            protected override string ReportScreenId { get => ScreenIds.RequestForInformationReport; }

            protected override Dictionary<string, string> GetParameters(RequestForInformation document)
            {
                var parameters = new Dictionary<string, string>
                {
                    [RequestForInformationConstants.Print.RequestForInformationId] = document.RequestForInformationCd,
                    [RequestForInformationConstants.Print.EmailId] = Email.NoteID.ToString()
                };
                return parameters;
            }

            protected override string GenerateReportName(RequestForInformation document)
            {
                var relatedEntityReference = document.RequestForInformationCd;
                var emailLastModified = Email.EmailLastModifiedDateTime.GetValueOrDefault();
                return string.Format(RequestForInformationMessages.RequestForInformationReportNamePattern, relatedEntityReference,
                    emailLastModified.ToString("MM.dd.yy H:mm:ss"));
            }
        }
        #endregion ReportGenerator
    }
}