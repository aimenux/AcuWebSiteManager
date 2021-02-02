using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver
{
    public class LienWaiverAmountSource
    {
        public const string BillAmount = "AP Bill Amount";
        public const string AmountPaid = "Amount Paid";

        private static readonly string[] ConditionalAmountSources =
        {
            BillAmount
        };

        private static readonly string[] UnconditionalAmountSources =
        {
            AmountPaid
        };

        public class ListConditionalAttribute : PXStringListAttribute
        {
            public ListConditionalAttribute()
                : base(ConditionalAmountSources, ConditionalAmountSources)
            {
            }
        }

        public class ListUnconditionalAttribute : PXStringListAttribute
        {
            public ListUnconditionalAttribute()
                : base(UnconditionalAmountSources, UnconditionalAmountSources)
            {
            }
        }

        public sealed class billAmount : BqlString.Constant<billAmount>
        {
            public billAmount()
                : base(BillAmount)
            {
            }
        }

        public sealed class amountPaid : BqlString.Constant<amountPaid>
        {
            public amountPaid()
                : base(AmountPaid)
            {
            }
        }
    }
}
