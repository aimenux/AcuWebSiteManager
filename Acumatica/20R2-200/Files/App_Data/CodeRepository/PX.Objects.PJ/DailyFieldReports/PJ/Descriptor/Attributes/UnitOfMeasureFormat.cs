using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class UnitOfMeasureFormat
    {
        public const string Metric = "Metric";
        public const string Imperial = "Imperial";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] AllowedValues =
            {
                Metric,
                Imperial
            };

            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }
    }
}
