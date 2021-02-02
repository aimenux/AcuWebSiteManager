using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels
{
    public class Main
    {
        [JsonProperty("Temp")]
        public decimal TemperatureLevel
        {
            get;
            set;
        }

        public int Humidity
        {
            get;
            set;
        }
    }
}