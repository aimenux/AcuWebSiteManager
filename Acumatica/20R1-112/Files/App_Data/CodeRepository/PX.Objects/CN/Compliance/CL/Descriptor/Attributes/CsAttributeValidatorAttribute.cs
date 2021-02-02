using PX.Data;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CR;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class CsAttributeValidatorAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
    {
        public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            var attribute = CRAttribute.Attributes[(string) e.NewValue];
            if (attribute == null)
            {
                throw new PXSetPropertyException(ComplianceMessages.AttributeNotFoundMessage);
            }
        }
    }
}