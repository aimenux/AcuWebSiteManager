using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.Reports;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Graphs;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.CR;
using PX.Objects.CT;
using PX.Reports.Data;
using PX.SM;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public class LienWaiverReportCreator : ILienWaiverReportCreator
    {
        private readonly PrintEmailLienWaiversProcess printEmailLienWaiversProcess;
        private readonly UploadFileMaintenance uploadFileMaintenance;
        private bool isLinkToLienWaiverNeeded;
        private bool isJointCheck;

        public LienWaiverReportCreator(PXGraph graph)
        {
            printEmailLienWaiversProcess = (PrintEmailLienWaiversProcess) graph;
            uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
        }

        public LienWaiverReportGenerationModel CreateReport(string reportId, ComplianceDocument complianceDocument,
            bool isCheckForJointVendor, string format = ReportProcessor.FilterPdf, bool shouldLinkToLienWaiver = true)
        {
            isLinkToLienWaiverNeeded = shouldLinkToLienWaiver;
            isJointCheck = isCheckForJointVendor;
            var lienWaiverReportGenerationModel = GetLienWaiverReportGenerationModel(reportId, complianceDocument);
            PXReportTools.InitReportParameters(lienWaiverReportGenerationModel.Report,
                lienWaiverReportGenerationModel.Parameters, Reports.SettingsProvider.Instance.Default);
            using (var streamManager = new StreamManager())
            {
                var reportNode = ReportProcessor.ProcessReport(lienWaiverReportGenerationModel.Report);
                ReportProcessor.GetRenderer(format).Render(reportNode, null, streamManager);
                lienWaiverReportGenerationModel.ReportFileInfo =
                    SaveReportFile(streamManager, complianceDocument, format);
            }
            return lienWaiverReportGenerationModel;
        }

        private LienWaiverReportGenerationModel GetLienWaiverReportGenerationModel(string reportId,
            ComplianceDocument complianceDocument)
        {
            return new LienWaiverReportGenerationModel
            {
                Report = PXReportTools.LoadReport(reportId, null),
                Parameters = GetReportParameters(complianceDocument)
            };
        }

        private Dictionary<string, string> GetReportParameters(
            ComplianceDocument complianceDocument)
        {
            return new Dictionary<string, string>
            {
                [Constants.LienWaiverReportParameters.ComplianceDocumentId] =
                    complianceDocument.ComplianceDocumentID.ToString(),
                [Constants.LienWaiverReportParameters.IsJointCheck] = isJointCheck.ToString()
            };
        }

        private FileInfo SaveReportFile(StreamManager streamManager,
            ComplianceDocument complianceDocument, string format)
        {
            var projectCd = printEmailLienWaiversProcess.Select<Contract>()
                .SingleOrDefault(ct => ct.ContractID == complianceDocument.ProjectID)?.ContractCD;
            var vendorCd = GetVendorCd(complianceDocument);
            DeleteFileIfNeeded(complianceDocument.NoteID, projectCd, vendorCd);
            var fileName = GetNameForLienWaiverReport(complianceDocument, format, projectCd, vendorCd);
            var reportFileInfo = new FileInfo(fileName, null, streamManager.MainStream.GetBytes());
            uploadFileMaintenance.SaveFile(reportFileInfo, FileExistsAction.CreateVersion);
            LinkToLienWaiverIfNeeded(reportFileInfo.UID, complianceDocument.NoteID);
            return reportFileInfo;
        }

        private void LinkToLienWaiverIfNeeded(Guid? fileNoteId, Guid? lienWaiverNoteId)
        {
            if (isLinkToLienWaiverNeeded)
            {
                var noteDoc = CreateNoteDoc(fileNoteId, lienWaiverNoteId);
                printEmailLienWaiversProcess.Caches[typeof(NoteDoc)].Insert(noteDoc);
                printEmailLienWaiversProcess.Caches[typeof(NoteDoc)].Persist(PXDBOperation.Insert);
            }
        }

        private void DeleteFileIfNeeded(Guid? complianceDocumentNoteId, string projectCd, string vendorCd)
        {
            var lienWaiverReportFileQuery = GetLienWaiverReportFileQuery(
                complianceDocumentNoteId, projectCd, vendorCd);
            var lienWaiverReportFile = lienWaiverReportFileQuery.RowCast<UploadFile>().SingleOrDefault();
            if (lienWaiverReportFile != null)
            {
                var lienWaiverReportNoteDocFile = lienWaiverReportFileQuery.RowCast<NoteDoc>().Single();
                printEmailLienWaiversProcess.Caches[typeof(UploadFile)].PersistDeleted(lienWaiverReportFile);
                printEmailLienWaiversProcess.Caches[typeof(NoteDoc)].PersistDeleted(lienWaiverReportNoteDocFile);
            }
        }

        private string GetNameForLienWaiverReport(ComplianceDocument complianceDocument,
            string format, string projectCd, string vendorCd)
        {
            var date = printEmailLienWaiversProcess.Accessinfo.BusinessDate;
            var fileExtension = GetReportFileExtension(format);
            return string.Format(Constants.LienWaiverReportFileNamePattern,
                complianceDocument.NoteID, projectCd, vendorCd, date, fileExtension);
        }

        private string GetVendorCd(ComplianceDocument complianceDocument)
        {
            var vendorId = isJointCheck
                ? complianceDocument.JointVendorInternalId
                : complianceDocument.VendorID;
            return printEmailLienWaiversProcess.Select<BAccount>()
                .SingleOrDefault(ven => ven.BAccountID == vendorId)?.AcctCD;
        }

        private static string GetReportFileExtension(string format)
        {
            return format == ReportProcessor.FilterExcel
                ? Common.Descriptor.Constants.ExcelFileExtension
                : Common.Descriptor.Constants.PdfFileExtension;
        }

        private PXResultset<UploadFile> GetLienWaiverReportFileQuery(
            Guid? complianceDocumentNoteId, string projectCd, string vendorCd)
        {
            var searchPattern = string.Format(
                Constants.LienWaiverReportFileNameSearchPattern, complianceDocumentNoteId, projectCd, vendorCd);
            return SelectFrom<UploadFile>
                .InnerJoin<NoteDoc>
                .On<UploadFile.fileID.IsEqual<NoteDoc.fileID>>
                .Where<NoteDoc.noteID.IsEqual<P.AsGuid>
                    .And<UploadFile.name.Contains<P.AsString>>>.View
                .Select(printEmailLienWaiversProcess, complianceDocumentNoteId, searchPattern);
        }

        private static NoteDoc CreateNoteDoc(Guid? fileId, Guid? complianceDocumentNoneId)
        {
            return new NoteDoc
            {
                FileID = fileId,
                NoteID = complianceDocumentNoneId
            };
        }
    }
}