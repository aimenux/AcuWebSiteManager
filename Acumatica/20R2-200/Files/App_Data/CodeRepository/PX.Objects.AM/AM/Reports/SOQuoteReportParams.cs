using System;
using System.Collections.Generic;

namespace PX.Objects.AM.Reports
{
    /// <summary>
    /// Estimate Report class for Sales order report (SO6410AM)
    /// </summary>
    public class SOQuoteReportParams
    {
        public const string ReportID = "SO641000";
        public const string ReportName = Messages.Quote;

        /// <summary>
        /// Make the parameters dictionary based on the supplied values
        /// </summary>
        /// <param name="orderType">Sales Order Type,  Only SO for now</param>
        /// <param name="refNbr">Sales order number</param>
        /// <returns>report parameters dictionary</returns>
        public static Dictionary<string, string> FromEstimateId(string orderType, string refNbr)
        {
            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(orderType))
            {
                parametersDictionary[Parameters.OrderType] = orderType;
            }

            if (!string.IsNullOrWhiteSpace(refNbr))
            {
                parametersDictionary[Parameters.RefNbr] = refNbr;
            }

            return parametersDictionary;
        }

        public static class Parameters
        {
            public const string OrderType = "OrderType";
            public const string RefNbr = "RefNbr";
        }
    }
}
