using PX.Data.BQL;
using PX.Web.UI;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor
{
    public class WeatherIntegrationConstants
    {
        public const string NoContentResponse = "null";

        public class TestConnectionUrls
        {
            public const string OpenWeatherMap =
                "http://api.openweathermap.org/data/2.5/weather?lat=51.510000&lon=-0.130000&APPID={0}";

            public const string WeatherBit =
                "https://api.weatherbit.io/v2.0/current?lat=51.510000&lon=-0.130000&key={0}";

            public const string AccuWeather =
                "http://dataservice.accuweather.com/currentconditions/v1/323562?apikey={0}&details=True";
        }

        public class ProcessingLogFilter
        {
            public const int DefaultRequestDateFromDifference = -4;
        }

        public class WeatherIntegrationSetup
        {
            public const int DefaultWeatherProcessingLogTerm = 5;
        }

        public class UnitOfMeasures
        {
            public const string Fahrenheit = "imperial";
            public const string WeatherBitFahrenheit = "I";
            public const decimal NumberOfMetersPerMile = 2.237M;
            public const decimal NumberOfMillimetersPerInch = 25.4M;
        }

        public class RequestStatusIcons
        {
            public const string RequestStatusHeaderIcon = Sprite.AliasControl + "@" + Sprite.Control.Complete;
            public const string RequestStatusSuccessIcon = Sprite.AliasMain + "@" + Sprite.Main.Success;
            public const string RequestStatusFailIcon = Sprite.AliasMain + "@" + Sprite.Main.Fail;

            public class requestStatusFailIcon : BqlString.Constant<requestStatusFailIcon>
            {
                public requestStatusFailIcon()
                    : base(RequestStatusFailIcon)
                {
                }
            }
        }

        public class WeatherBitServiceWeatherCodes
        {
            public const string StartCodeOfSnowRange = "6";
        }
    }
}