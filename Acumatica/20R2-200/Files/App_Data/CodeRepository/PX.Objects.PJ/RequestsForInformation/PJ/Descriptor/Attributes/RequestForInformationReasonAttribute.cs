using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class RequestForInformationReasonAttribute : PXStringListAttribute
    {
        public const string Unassigned = "NA";
        public const string Unanswered = "UA";
        public const string FollowUpNeeded = "FN";
        public const string WaitingInformation = "WI";
        public const string Answered = "AN";
        public const string NoResponseNeeded = "NR";
        public const string ConvertedToChangeRequest = "CR";

        public RequestForInformationReasonAttribute()
            : base(new[]
            {
                Unassigned,
                Unanswered,
                FollowUpNeeded,
                WaitingInformation,
                Answered,
                NoResponseNeeded,
                ConvertedToChangeRequest
            }, new[]
            {
                "N/A",
                "Unanswered",
                "Follow-up Needed",
                "Waiting For Additional Info",
                "Answered",
                "No Response Needed",
                "Converted To Change Request"
            })
        {
        }

        public sealed class unassigned : BqlString.Constant<unassigned>
        {
            public unassigned()
                : base(Unassigned)
            {
            }
        }

        public sealed class unanswered : BqlString.Constant<unanswered>
        {
            public unanswered()
                : base(Unanswered)
            {
            }
        }

        public sealed class followUpNeeded : BqlString.Constant<followUpNeeded>
        {
            public followUpNeeded()
                : base(FollowUpNeeded)
            {
            }
        }

        public sealed class waitingInformation : BqlString.Constant<waitingInformation>
        {
            public waitingInformation()
                : base(WaitingInformation)
            {
            }
        }

        public sealed class answered : BqlString.Constant<answered>
        {
            public answered()
                : base(Answered)
            {
            }
        }

        public sealed class noResponseNeeded : BqlString.Constant<noResponseNeeded>
        {
            public noResponseNeeded()
                : base(NoResponseNeeded)
            {
            }
        }

        public sealed class convertedToChangeRequest : BqlString.Constant<convertedToChangeRequest>
        {
            public convertedToChangeRequest()
                : base(ConvertedToChangeRequest)
            {
            }
        }
    }
}