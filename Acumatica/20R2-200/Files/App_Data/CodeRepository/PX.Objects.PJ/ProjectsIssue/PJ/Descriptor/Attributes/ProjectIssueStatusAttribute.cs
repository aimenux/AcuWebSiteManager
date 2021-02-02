using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.ProjectsIssue.PJ.Descriptor.Attributes
{
    public class ProjectIssueStatusAttribute : PXStringListAttribute
    {
        public const string Open = "O";
        public const string Closed = "C";
        public const string ConvertedToRfi = "R";
        public const string ConvertedToChangeRequest = "Q";

        public ProjectIssueStatusAttribute()
            : base(new[]
            {
                Open,
                Closed,
                ConvertedToRfi,
                ConvertedToChangeRequest
            }, new[]
            {
                "Open",
                "Closed",
                "Converted to RFI",
                "Converted to CR"
            })
        {
        }

        public sealed class convertedToChangeRequest : BqlString.Constant<convertedToChangeRequest>
        {
            public convertedToChangeRequest()
                : base(ConvertedToChangeRequest)
            {
            }
        }

        public sealed class convertedToRfi : BqlString.Constant<convertedToRfi>
        {
            public convertedToRfi()
                : base(ConvertedToRfi)
            {
            }
        }

        public sealed class open : BqlString.Constant<open>
        {
            public open()
                : base(Open)
            {
            }
        }

        public sealed class closed : BqlString.Constant<closed>
        {
            public closed()
                : base(Closed)
            {
            }
        }
    }
}
