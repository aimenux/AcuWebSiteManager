using System.Collections.Generic;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Models;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public interface ILienWaiverTransactionRetriever
    {
        IEnumerable<APTran> GetTransactions();

        IEnumerable<APTran> GetTransactions(LienWaiverGenerationKey generationKey);
    }
}