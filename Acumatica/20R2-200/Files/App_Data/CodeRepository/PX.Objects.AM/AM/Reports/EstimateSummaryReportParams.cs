using System;
using System.Collections.Generic;

namespace PX.Objects.AM.Reports
{
    /// <summary>
    /// Estimate Report class for Estimate Summary report (AM641000)
    /// </summary>
    public class EstimateSummaryReportParams
    {
        public const string ReportID = "AM641000";
        public const string ReportName = Messages.Summary;

        /// <summary>
        /// Make the parameters dictionary based on the supplied values
        /// </summary>
        /// <param name="estimateID">EstimateID,  Only SO for now</param>
        /// <param name="revisionID">RevisionID</param>
        /// <param name="printMaterial">Indicates if material will be printed on the report</param>
        /// <returns>report parameters dictionary</returns>
        public static Dictionary<string, string> FromEstimateId(string estimateID, string revisionID, bool printMaterial)
        {
            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(estimateID))
            {
                parametersDictionary[Parameters.EstimateID] = estimateID;
            }

            if (!string.IsNullOrWhiteSpace(revisionID))
            {
                parametersDictionary[Parameters.RevisionID] = revisionID;
            }

            parametersDictionary[Parameters.PrintMaterial] = Convert.ToString(printMaterial);

            return parametersDictionary;
        }

        public static class Parameters
        {
            public const string EstimateID = "EstimateID";
            public const string RevisionID = "RevisionID";
            public const string PrintMaterial = "PrintMaterial";
        }
    }
}
