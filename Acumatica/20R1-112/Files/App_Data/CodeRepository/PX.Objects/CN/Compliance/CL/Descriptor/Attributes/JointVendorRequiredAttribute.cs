using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class JointVendorRequiredAttribute : PXUIVerifyAttribute
    {
        public JointVendorRequiredAttribute()
            : base(
                typeof(Where<ComplianceDocument.jointVendorInternalId, IsNotNull,
                    And<ComplianceDocument.jointVendorExternalName, IsNull,
                        Or<ComplianceDocument.jointVendorInternalId, IsNull,
                    And<ComplianceDocument.jointVendorExternalName, IsNotNull,
                        Or<ComplianceDocument.jointVendorInternalId, IsNull,
                    And<ComplianceDocument.jointVendorExternalName, IsNull>>>>>>),
                PXErrorLevel.Error, ComplianceMessages.OnlyOneVendorIsAllowed)
        {
            CheckOnInserted = false;
            CheckOnRowSelected = false;
            CheckOnVerify = false;
        }
    }
}