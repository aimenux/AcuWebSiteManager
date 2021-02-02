using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists
{
    public class SkyState
    {
        public const string Clear = "Clear";
        public const string Cloudy = "Cloudy";
        public const string FewClouds = "Few Clouds";
        public const string Overcast = "Overcast";
        public const string Mist = "Mist";
        public const string Smoke = "Smoke";
        public const string Haze = "Haze";
        public const string Fog = "Fog";

        private static readonly string[] AllowedValues =
        {
            Clear,
            Cloudy,
            FewClouds,
            Overcast,
            Mist,
            Smoke,
            Haze,
            Fog
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
