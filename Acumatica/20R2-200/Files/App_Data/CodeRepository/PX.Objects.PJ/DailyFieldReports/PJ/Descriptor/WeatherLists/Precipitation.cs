using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists
{
    public class Precipitation
    {
        public const string None = "None";
        public const string Thunderstorm = "Thunderstorm";
        public const string Drizzle = "Drizzle";
        public const string LightRain = "Light Rain";
        public const string ModerateRain = "Moderate Rain";
        public const string HeavyRain = "Heavy Rain";
        public const string ShowerRain = "Shower Rain";
        public const string LightSnow = "Light Snow";
        public const string Snow = "Snow";
        public const string HeavySnow = "Heavy Snow";
        public const string MixSnowRain = "Mix Snow/Rain";

        private static readonly string[] AllowedValues =
        {
            None,
            Thunderstorm,
            Drizzle,
            LightRain,
            ModerateRain,
            HeavyRain,
            ShowerRain,
            LightSnow,
            Snow,
            HeavySnow,
            MixSnowRain
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }
    }
}
