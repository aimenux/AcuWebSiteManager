using PX.Data;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentSourceTypeAttribute : PXStringListAttribute
    {
        public const string ApBill = "AP Bill";
        public const string PoSub = "PO/Sub";
        public const string Project = "Project";
        public const string Customer = "Customer";

        public ComplianceDocumentSourceTypeAttribute()
            : base(new[]
                {
                    ApBill,
                    PoSub,
                    Customer,
                    Project
                },
                new[]
                {
                    ApBill,
                    PoSub,
                    Customer,
                    Project
                })
        {
        }
    }
}