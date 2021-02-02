using System.Collections.Generic;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public interface ILienWaiverGenerationKeyCreator
    {
        IEnumerable<LienWaiverGenerationKey> CreateGenerationKeys(IEnumerable<APTran> transactions,
            IReadOnlyCollection<JointPayee> jointPayees, APRegister payment);
    }
}