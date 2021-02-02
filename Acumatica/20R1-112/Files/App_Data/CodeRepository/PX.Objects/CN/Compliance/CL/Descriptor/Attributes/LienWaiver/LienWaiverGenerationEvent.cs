using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
    public class LienWaiverGenerationEvent
    {
        public const string PayingBill = "Paying AP Bill";

        private static readonly string[] GenerationEvents =
        {
            PayingBill
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(GenerationEvents, GenerationEvents)
            {
            }
        }

        public sealed class payingBill : BqlString.Constant<payingBill>
        {
            public payingBill()
                : base(PayingBill)
            {
            }
        }
    }
}
