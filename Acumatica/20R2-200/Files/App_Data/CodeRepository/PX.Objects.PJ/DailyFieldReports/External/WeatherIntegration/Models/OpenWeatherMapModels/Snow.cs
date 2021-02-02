using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels
{
    public class Snow
    {
        [JsonProperty("1h")]
        public decimal? PrecipitationAmount
        {
            get;
            set;
        }
    }
}