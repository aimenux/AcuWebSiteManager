using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data.Reports;
using PX.Reports;
using PX.Reports.Data;
using PX.SM;

namespace PX.Objects.Common.Tools
{
	public class ReportGeneratorHelper
	{
		public static FileInfo GenerateReport(string reportId, Dictionary<string, string> parameters, string fileName)
		{
			var reportNode = ProcessReport(reportId, parameters);
			return GenerateReportFile(reportNode, fileName);
		}

		private static ReportNode ProcessReport(string reportId, IDictionary<string, string> parameters)
		{
			var report = PXReportTools.LoadReport(reportId, null);
			PXReportTools.InitReportParameters(report, parameters, SettingsProvider.Instance.Default);
			return ReportProcessor.ProcessReport(report);
		}

		private static FileInfo GenerateReportFile(ReportNode reportNode, string fileName)
		{
			var data = PX.Reports.Mail.Message.GenerateReport(reportNode, ReportProcessor.FilterPdf).First();
			return new FileInfo(fileName, null, data);
		}
	}
}
