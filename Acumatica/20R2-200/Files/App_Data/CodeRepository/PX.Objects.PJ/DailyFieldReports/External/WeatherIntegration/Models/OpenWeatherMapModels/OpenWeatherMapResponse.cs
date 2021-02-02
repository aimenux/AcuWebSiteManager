using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels
{
    public class OpenWeatherMapResponse
    {
        public Main Main
        {
            get;
            set;
        }

        public Wind Wind
        {
            get;
            set;
        }

        public Rain Rain
        {
            get;
            set;
        }

        public Snow Snow
        {
            get;
            set;
        }

        public Clouds Clouds
        {
            get;
            set;
        }

        public List<Weather> Weather
        {
            get;
            set;
        }

        [JsonProperty("dt")]
        public int TimeObserved
        {
            get;
            set;
        }
    }
}