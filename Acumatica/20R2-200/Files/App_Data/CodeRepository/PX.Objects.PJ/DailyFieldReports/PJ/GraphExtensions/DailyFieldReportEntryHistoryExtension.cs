using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.Reports;
using PX.Reports;
using PX.Reports.Data;
using PX.Reports.Mail;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryHistoryExtension : PXGraphExtension<DailyFieldReportEntry>
    {
        [PXViewName(ViewNames.History)]
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportHistory>
            .Where<DailyFieldReportHistory.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View History;

        public PXAction<DailyFieldReport> ViewAttachedReport;

        private bool IsHistoryLogInstalled => Base.ProjectManagementSetup.Current.IsHistoryLogEnabled == true;

        [PXButton]
        [PXUIField]
        public virtual void viewAttachedReport()
        {
            var reportFileId = PXNoteAttribute.GetFileNotes(History.Cache, History.Current).FirstOrDefault();
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
            var reportFile = uploadFileMaintenance.GetFile(reportFileId);
            throw new PXRedirectToFileException(reportFile.UID, true);
        }

        public virtual void _(Events.RowSelected<DailyFieldReport> args)
        {
            History.AllowSelect = IsHistoryLogInstalled;
        }

        public virtual void _(Events.RowPersisting<DailyFieldReport> args)
        {
            var originalStatus = args.Cache.GetValueOriginal<DailyFieldReport.status>(args.Row)?.ToString();
            var shouldNewReportBeAttached = ShouldNewReportBeAttached(originalStatus, args.Row.Status, args.Operation);
            if (shouldNewReportBeAttached)
            {
                AttachPdfReportToHistory();
            }
        }

        private bool ShouldNewReportBeAttached(string originalStatus, string newStatus, PXDBOperation operation)
        {
            var isOriginalStatusNotCompleted = originalStatus != DailyFieldReportStatus.Completed;
            var isNewStatusCompleted = newStatus == DailyFieldReportStatus.Completed;
            return IsHistoryLogInstalled &&
                operation == PXDBOperation.Update && isOriginalStatusNotCompleted && isNewStatusCompleted;
        }

        private void InsertDailyFieldReportHistory(string fileName)
        {
            var dailyFieldReportHistory = new DailyFieldReportHistory
            {
                DailyFieldReportId = Base.DailyFieldReport.Current.DailyFieldReportId,
                FileName = fileName
            };
            History.Insert(dailyFieldReportHistory);
        }

        private void AttachPdfReportToHistory()
        {
            var reportName = CreateReportFileName();
            InsertDailyFieldReportHistory(reportName);
            var report = GenerateReport();
            var fileInfo = new FileInfo(reportName, string.Empty, report);
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
            uploadFileMaintenance.SaveFile(fileInfo);
            PXNoteAttribute.SetFileNotes(History.Cache, History.Current, fileInfo.UID.GetValueOrDefault());
            History.Cache.Persist(PXDBOperation.Insert);
        }

        private byte[] GenerateReport()
        {
            var parameters = new Dictionary<string, string>
            {
                [DailyFieldReportConstants.Print.DailyFieldReportId] = Base.DailyFieldReport.Current.DailyFieldReportCd
            };
            var report = PXReportTools.LoadReport(ScreenIds.DailyFieldReportForm, null);
            PXReportTools.InitReportParameters(report, parameters, SettingsProvider.Instance.Default);
            var reportNode = ReportProcessor.ProcessReport(report);
            return Message.GenerateReport(reportNode, ReportProcessor.FilterPdf).First();
        }

        private string CreateReportFileName()
        {
            var date = Base.DailyFieldReport.Current.Date.GetValueOrDefault().ToString(
                Constants.FilesDateFormat);
            var historyCount = History.Select().Count;
            return $"Daily Field Report#{Base.DailyFieldReport.Current.DailyFieldReportCd}_{date} ({++historyCount}).pdf";
        }
    }
}
