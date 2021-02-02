using System.Collections.Generic;
using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Comparers
{
    public class LienWaiverGenerationKeyComparer : IEqualityComparer<LienWaiverGenerationKey>
    {
        public bool Equals(LienWaiverGenerationKey key, LienWaiverGenerationKey otherKey)
        {
            return key != null && otherKey != null &&
                key.ProjectId == otherKey.ProjectId &&
                key.VendorId == otherKey.VendorId &&
                key.JointPayeeVendorId == otherKey.JointPayeeVendorId &&
                key.OrderNumber == otherKey.OrderNumber;
        }

        public int GetHashCode(LienWaiverGenerationKey lienWaiverGenerationKey)
        {
            return lienWaiverGenerationKey.ProjectId.GetHashCode() ^ lienWaiverGenerationKey.VendorId.GetHashCode() ^
                lienWaiverGenerationKey.OrderNumber.GetHashCode() ^
                lienWaiverGenerationKey.JointPayeeVendorId.GetHashCode();
        }
    }
}