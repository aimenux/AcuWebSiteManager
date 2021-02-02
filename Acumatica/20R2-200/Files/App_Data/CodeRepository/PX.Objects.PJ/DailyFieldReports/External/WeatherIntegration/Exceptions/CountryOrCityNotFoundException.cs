using System;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions
{
    public class CountryOrCityNotFoundException : Exception
    {
        private const string CountryOrCityIsNotFound =
            "The country or city specified for the daily field report is not found.";

        public CountryOrCityNotFoundException()
            : base(CountryOrCityIsNotFound)
        {
        }
    }
}