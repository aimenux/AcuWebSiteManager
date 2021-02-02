using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
    public class LienWaiverThroughDateSource
    {
        public const string BillDate = "AP Bill Date";
        public const string PostingPeriodEndDate = "Posting Period End Date";
        public const string PaymentDate = "AP Check Date";

        private static readonly string[] ThroughDateSources =
        {
            BillDate,
            PostingPeriodEndDate,
            PaymentDate
        };

        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(ThroughDateSources, ThroughDateSources)
            {
            }
        }

        public sealed class billDate : BqlString.Constant<billDate>
        {
            public billDate()
                : base(BillDate)
            {
            }
        }

        public sealed class postingPeriodEndDate : BqlString.Constant<postingPeriodEndDate>
        {
            public postingPeriodEndDate()
                : base(PostingPeriodEndDate)
            {
            }
        }

        public sealed class paymentDate : BqlString.Constant<paymentDate>
        {
            public paymentDate()
                : base(PaymentDate)
            {
            }
        }
    }
}
