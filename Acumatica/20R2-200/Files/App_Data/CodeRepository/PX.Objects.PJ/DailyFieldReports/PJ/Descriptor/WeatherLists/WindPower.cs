using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists
{
    public class WindPower
    {
        public const string None = "None";
        public const string Calm = "Calm";
        public const string LightWind = "Light Wind";
        public const string HighWind = "High Wind";

        private static readonly string[] AllowedValues =
        {
            None,
            Calm,
            LightWind,
            HighWind
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
