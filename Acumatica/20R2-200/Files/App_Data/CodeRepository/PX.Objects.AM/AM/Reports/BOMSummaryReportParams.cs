using System.Collections.Generic;

namespace PX.Objects.AM.Reports
{
    /// <summary>
    /// Manufacturing Report class for bom summary report (AM611000)
    /// </summary>
    public class BOMSummaryReportParams
    {
        public const string ReportID = "AM611000";
        public const string ReportName = Messages.BOMSummary;

        /// <summary>
        /// Make the parameters dictionary based on the supplied values
        /// </summary>
        /// <param name="bomId">Bom ID</param>
        /// <param name="revisionId">BOM Revision</param>
        /// <returns>report parameters dictionary</returns>
        public static Dictionary<string, string> FromBomId(string bomId, string revisionId)
        {
            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(bomId))
            {
                parametersDictionary[Parameters.BOMID] = bomId.PadRight(15);
            }

            if (!string.IsNullOrWhiteSpace(revisionId))
            {
                parametersDictionary[Parameters.RevisionID] = revisionId.PadRight(10);
            }

            //TODO (2108R2) implement RevisionID parameter

            return parametersDictionary;
        }

        public static class Parameters
        {
            public const string BOMID = "BOMID";
            public const string RevisionID = "RevisionID";
            public const string InventoryID = "InventoryID";       //NOTE: if you use inventory ID i think it must be the "CD" that gets supplied and not the "ID"
        }
    }
}
