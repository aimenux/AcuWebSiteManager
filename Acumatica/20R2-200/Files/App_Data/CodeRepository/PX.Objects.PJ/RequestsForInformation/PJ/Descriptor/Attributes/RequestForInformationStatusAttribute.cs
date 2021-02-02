using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationStatusAttribute : PXStringListAttribute
    {
        public const string NewStatus = "N";
        public const string OpenStatus = "O";
        public const string ClosedStatus = "C";
        public const string NewStatusLabel = "New";
        public const string OpenStatusLabel = "Open";
        public const string ClosedStatusLabel = "Closed";

        public RequestForInformationStatusAttribute()
            : base(new[]
            {
                NewStatus,
                OpenStatus,
                ClosedStatus
            }, new[]
            {
                NewStatusLabel,
                OpenStatusLabel,
                ClosedStatusLabel
            })
        {
        }

        public sealed class openStatus : BqlString.Constant<openStatus>
        {
            public openStatus()
                : base(OpenStatus)
            {
            }
        }

        public sealed class newStatus : BqlString.Constant<newStatus>
        {
            public newStatus()
                : base(NewStatus)
            {
            }
        }

        public sealed class closedStatus : BqlString.Constant<closedStatus>
        {
            public closedStatus()
                : base(ClosedStatus)
            {
            }
        }
    }
}
