using System;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions
{
    public class NumberOfRequestsHasBeenExceededException : Exception
    {
        private const string NumberOfRequestsHasBeenExceeded = "The allowed number of requests has been exceeded";

        public NumberOfRequestsHasBeenExceededException()
            : base(NumberOfRequestsHasBeenExceeded)
        {
        }
    }
}
