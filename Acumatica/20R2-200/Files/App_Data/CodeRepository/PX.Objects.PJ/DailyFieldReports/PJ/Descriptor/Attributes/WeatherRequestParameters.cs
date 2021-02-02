using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class WeatherRequestParameters
    {
        public const string ZipCodeAndCountry = "Zip Code And Country";
        public const string CityAndCountry = "City And Country";
        public const string GeographicLocation = "Geographic Location";

        private static readonly string[] RequestParameters =
        {
            ZipCodeAndCountry,
            CityAndCountry,
            GeographicLocation
        };

        private static readonly string[] AccuWeatherParameters =
        {
            GeographicLocation
        };

        public class ListAttribute : PXStringListAttribute, IPXRowSelectedSubscriber
        {
            public void RowSelected(PXCache cache, PXRowSelectedEventArgs args)
            {
                if (args.Row is WeatherIntegrationSetup weatherSetup)
                {
                    switch (weatherSetup.WeatherApiService)
                    {
                        case WeatherApiService.OpenWeatherMap:
                        case WeatherApiService.WeatherBit:
                            _AllowedLabels = RequestParameters;
                            _AllowedValues = RequestParameters;
                            break;
                        case WeatherApiService.AccuWeather:
                            _AllowedLabels = AccuWeatherParameters;
                            _AllowedValues = AccuWeatherParameters;
                            break;
                    }
                }
            }
        }
    }
}
