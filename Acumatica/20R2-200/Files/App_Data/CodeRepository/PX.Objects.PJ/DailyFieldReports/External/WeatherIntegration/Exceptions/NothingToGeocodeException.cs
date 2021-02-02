using System;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions
{
    public class NothingToGeocodeException : Exception
    {
        private const string NothingCanBeFoundForTheProjectSiteLocation =
            "Nothing can be found for the project site location specified for the daily field report.";

        public NothingToGeocodeException()
            : base(NothingCanBeFoundForTheProjectSiteLocation)
        {
        }
    }
}