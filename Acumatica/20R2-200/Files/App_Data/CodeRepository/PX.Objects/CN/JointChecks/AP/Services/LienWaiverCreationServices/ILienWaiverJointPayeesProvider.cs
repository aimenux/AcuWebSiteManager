using System.Collections.Generic;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public interface ILienWaiverJointPayeesProvider
    {
        IEnumerable<JointPayee> GetValidJointPayees();
    }
}