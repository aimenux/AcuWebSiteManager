using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.WeatherBitModels
{
    public class Weather
    {
        public string Icon
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        [JsonProperty("description")]
        public string SiteCondition
        {
            get;
            set;
        }
    }
}