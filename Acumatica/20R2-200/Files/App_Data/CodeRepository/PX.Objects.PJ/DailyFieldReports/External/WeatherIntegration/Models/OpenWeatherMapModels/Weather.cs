using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels
{
    public class Weather
    {
        [JsonProperty("description")]
        public string SiteCondition
        {
            get;
            set;
        }

        public string Icon
        {
            get;
            set;
        }
    }
}
