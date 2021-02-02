using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.AccuWeatherModels
{
    public class AccuWeatherResponse
    {
        [JsonProperty("WeatherIcon")]
        public string Icon
        {
            get;
            set;
        }

        [JsonProperty("PrecipitationSummary")]
        public PrecipitationSummary Precipitation
        {
            get;
            set;
        }

        public Wind Wind
        {
            get;
            set;
        }

        [JsonProperty("CloudCover")]
        public int? Cloudiness
        {
            get;
            set;
        }

        public Temperature Temperature
        {
            get;
            set;
        }

        [JsonProperty("EpochTime")]
        public long TimeObserved
        {
            get;
            set;
        }

        [JsonProperty("RelativeHumidity")]
        public int? Humidity
        {
            get;
            set;
        }

        [JsonProperty("WeatherText")]
        public string SiteCondition
        {
            get;
            set;
        }
    }
}
