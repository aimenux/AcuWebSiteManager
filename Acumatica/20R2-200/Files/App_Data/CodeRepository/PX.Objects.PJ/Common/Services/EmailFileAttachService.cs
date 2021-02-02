using PX.Objects.PJ.Common.Services;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.SM;
using System.Collections.Generic;
using PX.Objects.Common.Tools;

namespace PX.Objects.PJ.Common.Services
{
    public class EmailFileAttachService<TEntity> : BaseEmailFileAttachService<TEntity>
        where TEntity : class, IBqlTable, new()
    {
        protected ReportGeneratorBase<TEntity> ReportGenerator { get; }

        public EmailFileAttachService(CREmailActivityMaint graph, ReportGeneratorBase<TEntity> reportGenerator) 
            : base(graph)
        {
            ReportGenerator = reportGenerator;
        }

        protected override IEnumerable<NoteDoc> GetFilesLinkedToRelatedEntities()
        {
            return EmailActivityDataProvider.GetFileNotesAttachedToEntity(Graph.Message.Current.RefNoteID);
        }

        public virtual FileInfo GenerateAndAttachReport(TEntity document, UploadFileMaintenance uploadFileMaintenance)
        {
            var file = ReportGenerator.GenerateReportFile(document);
            uploadFileMaintenance.SaveFile(file);

            // attach generated report to Email
            var fileId = file.UID.GetValueOrDefault();
            PXNoteAttribute.SetFileNotes(Graph.Message.Cache, Graph.Message.Current, fileId.SingleToArray());

            return file;
        }

        #region ReportGenerator
        public abstract class ReportGeneratorBase<T>
        {
            public CRSMEmail Email { get; }
            public PXCache EmailCache { get; }

            public ReportGeneratorBase(CRSMEmail email, PXCache emailCache)
            {
                Email = email;
                EmailCache = emailCache;
            }

            public FileInfo GenerateReportFile(T document)
            {
                var parameters = GetParameters(document);
                var fileName = GenerateReportName(document);
                var file = ReportGeneratorHelper.GenerateReport(ReportScreenId, parameters, fileName);
                return file;
            }

            protected abstract string ReportScreenId { get; }


            protected abstract Dictionary<string, string> GetParameters(T document);

            protected abstract string GenerateReportName(T document);
        }
        #endregion ReportGenerator
    }
}
