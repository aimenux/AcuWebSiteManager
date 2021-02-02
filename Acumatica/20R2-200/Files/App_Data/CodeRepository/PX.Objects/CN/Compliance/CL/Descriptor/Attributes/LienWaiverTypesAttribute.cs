using PX.Data;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class LienWaiverTypesAttribute : PXStringListAttribute
    {
        private static readonly string[] LienWaiverTypes =
        {
            Constants.LienWaiverDocumentTypeValues.ConditionalPartial,
            Constants.LienWaiverDocumentTypeValues.ConditionalFinal,
            Constants.LienWaiverDocumentTypeValues.UnconditionalPartial,
            Constants.LienWaiverDocumentTypeValues.UnconditionalFinal
        };

        public LienWaiverTypesAttribute()
            : base(LienWaiverTypes, LienWaiverTypes)
        {
        }
    }
}