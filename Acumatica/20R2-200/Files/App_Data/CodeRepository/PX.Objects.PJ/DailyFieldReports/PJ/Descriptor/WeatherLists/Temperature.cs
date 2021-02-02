using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.WeatherLists
{
    public class Temperature
    {
        public const string VeryHot = "Very Hot";
        public const string Hot = "Hot";
        public const string Warm = "Warm";
        public const string Mild = "Mild";
        public const string Cool = "Cool";
        public const string Chilly = "Chilly";
        public const string Cold = "Cold";
        public const string Frosty = "Frosty";
        public const string VeryCold = "Very Cold";

        private static readonly string[] AllowedValues =
        {
            VeryHot,
            Hot,
            Warm,
            Mild,
            Cool,
            Chilly,
            Cold,
            Frosty,
            VeryCold
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
