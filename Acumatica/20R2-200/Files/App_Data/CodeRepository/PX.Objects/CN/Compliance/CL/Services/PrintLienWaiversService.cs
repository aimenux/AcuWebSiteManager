using System.Collections.Generic;
using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public class PrintLienWaiversService : PrintEmailLienWaiverBaseService, IPrintLienWaiversService
    {
        private PXReportRequiredException reportRequiredException;

        public PrintLienWaiversService(PXGraph graph)
            : base(graph)
        {
        }

        public override void Process(List<ComplianceDocument> complianceDocuments)
        {
            base.Process(complianceDocuments);
            if (reportRequiredException != null)
            {
                reportRequiredException.Mode = PXBaseRedirectException.WindowMode.New;
                throw reportRequiredException;
            }
        }

        protected override void ProcessLienWaiver(NotificationSourceModel notificationSourceModel,
            ComplianceDocument complianceDocument)
        {
            base.ProcessLienWaiver(notificationSourceModel, complianceDocument);
            ConfigurePrintActionParameters(notificationSourceModel.NotificationSource.ReportID,
                notificationSourceModel.NotificationSource.NBranchID);
            UpdateLienWaiverProcessedStatus(complianceDocument);

            PXProcessing.SetProcessed();
		}

        private void ConfigurePrintActionParameters(string reportId, int? branchId)
        {
            reportRequiredException = PXReportRequiredException.CombineReport(
                reportRequiredException, reportId,
                LienWaiverReportGenerationModel.Parameters, false);
            var reportParametersForDeviceHub = GetReportParametersForDeviceHub();
            var reportToPrint = new Dictionary<PrintSettings, PXReportRequiredException>();
            reportToPrint = SMPrintJobMaint.AssignPrintJobToPrinter(
                reportToPrint, reportParametersForDeviceHub,
                PrintEmailLienWaiversProcess.Filter.Current,
                new NotificationUtility(PrintEmailLienWaiversProcess).SearchPrinter,
                Constants.ComplianceNotification.LienWaiverNotificationSourceCd,
                reportId, reportId, branchId);
            SMPrintJobMaint.CreatePrintJobGroups(reportToPrint);
        }

        private Dictionary<string, string> GetReportParametersForDeviceHub()
        {
            return new Dictionary<string, string>
            {
                [Constants.LienWaiverReportParameters.DeviceHubComplianceDocumentId] = LienWaiverReportGenerationModel
                    .Parameters[Constants.LienWaiverReportParameters.ComplianceDocumentId],
                [Constants.LienWaiverReportParameters.IsJointCheck] = LienWaiverReportGenerationModel
                    .Parameters[Constants.LienWaiverReportParameters.IsJointCheck]
            };
        }
    }
}