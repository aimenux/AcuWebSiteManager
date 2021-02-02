using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class VisitorType
    {
        public const string Owner = "Owner";
        public const string Inspector = "Inspector";
        public const string Customer = "Customer";
        public const string SalesAgent = "Sales Agent";
        public const string Other = "Other";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] AllowedValues =
            {
                Owner,
                Inspector,
                Customer,
                SalesAgent,
                Other
            };

            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }
    }
}