using System.Collections.Generic;
using PX.Objects.AP;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public interface ILienWaiverTransactionsProvider
    {
        IEnumerable<APTran> GetTransactions(APRegister payment);
    }
}