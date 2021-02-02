using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Models;
using PX.Reports.Data;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public interface ILienWaiverReportCreator
    {
        LienWaiverReportGenerationModel CreateReport(string reportId, ComplianceDocument complianceDocument,
            bool isCheckForJointVendor, string format = ReportProcessor.FilterPdf, bool shouldLinkToLienWaiver = true);
    }
}