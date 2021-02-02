using System;
using System.Collections.Generic;

namespace PX.Objects.AM.Reports
{
    /// <summary>
    /// Estimate Report class for Opportunity Quote report (CR604500)
    /// </summary>
    public class CRQuoteReportParams
    {
        public const string ReportID = "CR604500";
        public const string ReportName = Messages.Quote;

        /// <summary>
        /// Make the parameters dictionary based on the supplied values
        /// </summary>
        /// <param name="opportunityID"> Opportunity ID</param>
        /// <param name="quoteNbr"> Quote number</param>
        /// <returns>report parameters dictionary</returns>
        public static Dictionary<string, string> FromEstimateId(string opportunityID, string quoteNbr)
        {
            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(opportunityID))
            {
                parametersDictionary[Parameters.OpportunityID] = opportunityID;
            }

            if (!string.IsNullOrWhiteSpace(quoteNbr))
            {
                parametersDictionary[Parameters.QuoteNbr] = quoteNbr;
            }

            return parametersDictionary;
        }

        public static class Parameters
        {
            public const string OpportunityID = "OpportunityID";
            public const string QuoteNbr = "QuoteNbr";
        }
    }
}
