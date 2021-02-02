using System.Collections.Generic;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Comparers
{
    public class JointPayeeComparer : IEqualityComparer<JointPayee>
    {
        public bool Equals(JointPayee jointPayee1, JointPayee jointPayee2)
        {
            return jointPayee1?.JointPayeeInternalId != null
                && jointPayee1.JointPayeeInternalId == jointPayee2?.JointPayeeInternalId
                || jointPayee1?.JointPayeeExternalName != null
                && jointPayee1.JointPayeeExternalName == jointPayee2?.JointPayeeExternalName;
        }

        public int GetHashCode(JointPayee jointPayee)
        {
            return jointPayee.JointPayeeInternalId != null
                ? jointPayee.JointPayeeInternalId.GetHashCode()
                : jointPayee.JointPayeeExternalName.GetHashCode();
        }
    }
}