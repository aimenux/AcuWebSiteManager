using PX.Data;
using PX.Objects.PM;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes
{
    public class ProjectWithoutVerificationAttribute : ProjectAttribute
    {
        public override void FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
        }
    }
}
