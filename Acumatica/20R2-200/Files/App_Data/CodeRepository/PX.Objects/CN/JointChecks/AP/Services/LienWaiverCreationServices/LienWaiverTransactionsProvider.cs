using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverTransactionsProvider : LienWaiverGenerationDataProviderBase, ILienWaiverTransactionsProvider
    {
        public LienWaiverTransactionsProvider(PXGraph graph)
            : base(graph)
        {
        }

        public IEnumerable<APTran> GetTransactions(APRegister payment)
        {
            Transactions = TransactionRetriever.GetTransactions().ToList();
            return Transactions.IsEmpty() || ShouldStopForOutstandingLienWaivers(payment)
                ? Enumerable.Empty<APTran>()
                : Transactions.Where(transaction => IsApplicable(transaction, payment));
        }

        private bool ShouldStopForOutstandingLienWaivers(APRegister payment)
        {
            return LienWaiverSetup.ShouldStopPayments == true && AreThereAnyOutstandingLienWaivers(payment.VendorID);
        }

        private bool IsApplicable(APTran transaction, APRegister payment)
        {
            var projectId = LienWaiverProjectDataProvider.GetProjectId(Graph, transaction);
            return AreThereAnyLienWaiverRecipientsForVendorClass(payment.VendorID, projectId) &&
                DoesCommitmentAmountExceedMinimumCommitmentAmount(payment.VendorID, transaction);
        }

        private bool AreThereAnyOutstandingLienWaivers(int? vendorId)
        {
            var projectIds = Transactions.Select(tran => LienWaiverProjectDataProvider.GetProjectId(Graph, tran))
                .Distinct();
            return LienWaiverDataProvider.DoesAnyOutstandingComplianceExistForPrimaryVendor(vendorId, projectIds);
        }
    }
}