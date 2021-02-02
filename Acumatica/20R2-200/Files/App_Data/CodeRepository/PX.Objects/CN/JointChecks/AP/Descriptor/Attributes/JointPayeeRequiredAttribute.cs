using PX.Data;
using PX.Data.BQL;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Descriptor.Attributes
{
	public class JointPayeeRequiredAttribute : PXUIVerifyAttribute
	{
		public JointPayeeRequiredAttribute()
			: base(typeof(Where<Brackets<JointPayee.jointPayeeInternalId.IsNotNull
						.And<JointPayee.jointPayeeExternalName.IsNull>>
					.Or<JointPayee.jointPayeeInternalId.IsNull.And<JointPayee.jointPayeeExternalName.IsNotNull>>
					.And<JointPayee.jointPayeeExternalName.IsNotEqual<StringEmpty>>>),
				PXErrorLevel.Error, JointCheckMessages.OnlyOneVendorIsAllowed)
		{
			CheckOnInserted = false;
			CheckOnRowSelected = false;
			CheckOnVerify = false;
		}
	}
}