using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class WeatherApiService
    {
        public const string OpenWeatherMap = "OpenWeatherMap";
        public const string WeatherBit = "WeatherBit";
        public const string AccuWeather = "AccuWeather";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] AllowedValues =
            {
                OpenWeatherMap,
                WeatherBit,
                AccuWeather
            };

            private static readonly string[] AllowedLabels =
            {
                "OpenWeatherMap",
                "WeatherBit",
                "AccuWeather"
            };

            public ListAttribute()
                : base(AllowedValues, AllowedLabels)
            {
            }
        }
    }
}
