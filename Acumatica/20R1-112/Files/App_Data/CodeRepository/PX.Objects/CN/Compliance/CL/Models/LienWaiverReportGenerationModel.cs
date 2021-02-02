using System.Collections.Generic;
using PX.Reports.Controls;
using PX.SM;

namespace PX.Objects.CN.Compliance.CL.Models
{
    public class LienWaiverReportGenerationModel
    {
        public Report Report
        {
            get;
            set;
        }

        public FileInfo ReportFileInfo
        {
            get;
            set;
        }

        public Dictionary<string, string> Parameters
        {
            get;
            set;
        }
    }
}