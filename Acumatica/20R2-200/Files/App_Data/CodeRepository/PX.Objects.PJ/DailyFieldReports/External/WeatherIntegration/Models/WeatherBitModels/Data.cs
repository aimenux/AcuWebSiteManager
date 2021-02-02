using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.WeatherBitModels
{
    public class Data
    {
        [JsonProperty("Temp")]
        public decimal TemperatureLevel
        {
            get;
            set;
        }

        [JsonProperty("rh")]
        public int? Humidity
        {
            get;
            set;
        }

        [JsonProperty("wind_spd")]
        public decimal WindSpeed
        {
            get;
            set;
        }

        [JsonProperty("precip")]
        public decimal? Rain
        {
            get;
            set;
        }

        [JsonProperty("snow")]
        public decimal? Snowfall
        {
            get;
            set;
        }

        [JsonProperty("clouds")]
        public int Cloudiness
        {
            get;
            set;
        }

        [JsonProperty("ts")]
        public int TimeObserved
        {
            get;
            set;
        }

        public Weather Weather
        {
            get;
            set;
        }
    }
}
