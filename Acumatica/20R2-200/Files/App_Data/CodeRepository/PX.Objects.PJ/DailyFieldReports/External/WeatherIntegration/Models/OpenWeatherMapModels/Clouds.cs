using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels
{
    public class Clouds
    {
        [JsonProperty("All")]
        public int Cloudiness
        {
            get;
            set;
        }
    }
}