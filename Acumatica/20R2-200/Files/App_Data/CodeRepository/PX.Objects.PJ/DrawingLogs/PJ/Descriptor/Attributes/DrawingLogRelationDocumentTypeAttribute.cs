using PX.Objects.PJ.Common.Descriptor;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.PJ.DrawingLogs.PJ.Descriptor.Attributes
{
    public class DrawingLogRelationDocumentTypeAttribute : PXStringListAttribute
    {
        public DrawingLogRelationDocumentTypeAttribute()
            : base(new[]
            {
                CacheNames.RequestForInformation,
                CacheNames.ProjectIssue
            }, new[]
            {
                CacheNames.RequestForInformation,
                CacheNames.ProjectIssue
            })
        {
        }

        public sealed class requestForInformation : BqlString.Constant<requestForInformation>
        {
            public requestForInformation()
                : base(CacheNames.RequestForInformation)
            {
            }
        }

        public sealed class projectIssue : BqlString.Constant<requestForInformation>
        {
            public projectIssue()
                : base(CacheNames.ProjectIssue)
            {
            }
        }
    }
}
