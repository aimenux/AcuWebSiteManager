using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class DailyFieldReportStatus
    {
        public const string Hold = "On Hold";
        public const string PendingApproval = "Pending Approval";
        public const string Rejected = "Rejected";
        public const string Completed = "Completed";

        public class ListAttribute : PXStringListAttribute
        {
            private static readonly string[] AllowedValues =
            {
                Hold,
                PendingApproval,
                Rejected,
                Completed
            };

            public ListAttribute()
                : base(AllowedValues, AllowedValues)
            {
            }
        }

        public sealed class hold : BqlString.Constant<hold>
        {
            public hold()
                : base(Hold)
            {
            }
        }

        public sealed class pendingApproval : BqlString.Constant<pendingApproval>
        {
            public pendingApproval()
                : base(PendingApproval)
            {
            }
        }

        public sealed class rejected : BqlString.Constant<rejected>
        {
            public rejected()
                : base(Rejected)
            {
            }
        }

        public sealed class completed : BqlString.Constant<completed>
        {
            public completed()
                : base(Completed)
            {
            }
        }
    }
}