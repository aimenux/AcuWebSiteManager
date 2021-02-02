using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverJointPayeesProvider : LienWaiverGenerationDataProviderBase, ILienWaiverJointPayeesProvider
    {
        public LienWaiverJointPayeesProvider(PXGraph graph)
            : base(graph)
        {
        }

        public IEnumerable<JointPayee> GetValidJointPayees()
        {
            Transactions = TransactionRetriever.GetTransactions().ToList();
            var extension = Graph.GetExtension<ApPaymentEntryExt>();
            var insertedJointPayeePayments = extension.JointPayeePayments.Cache.Inserted.RowCast<JointPayeePayment>();
            var jointPayeePayments = extension.JointPayeePayments.Select().FirstTableItems
                .Concat(insertedJointPayeePayments).ToList();
            var jointPayeePaymentsToPay = jointPayeePayments.Where(jpp => jpp.JointAmountToPay > 0);
            var jointPayeesToPay = JointPayeeDataProvider.GetJointPayees(Graph, jointPayeePaymentsToPay);
            return jointPayeesToPay.Where(IsValidJointPayee);
        }

        private bool IsValidJointPayee(JointPayee jointPayee)
        {
            return IsVendorInternal(jointPayee) &&
                AreThereAnyLienWaiverRecipientsForVendorClass(jointPayee) &&
                !(LienWaiverSetup.ShouldStopPayments == true && AreThereAnyOutstandingLienWaivers(jointPayee)) &&
                DoesCommitmentAmountExceedMinimumCommitmentAmount(jointPayee);
        }

        private static bool IsVendorInternal(JointPayee jointPayee)
        {
            return jointPayee.JointPayeeExternalName == null;
        }

        private bool AreThereAnyLienWaiverRecipientsForVendorClass(JointPayee jointPayee)
        {
            var projectId = GetProjectId(jointPayee);
            return projectId != null &&
                AreThereAnyLienWaiverRecipientsForVendorClass(jointPayee.JointPayeeInternalId, projectId);
        }

        private bool AreThereAnyOutstandingLienWaivers(JointPayee jointPayee)
        {
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(Graph, jointPayee);
            return LienWaiverDataProvider.DoesAnyOutstandingComplianceExistForJointVendor(
                jointPayee.JointPayeeInternalId, projectIds);
        }

        private bool DoesCommitmentAmountExceedMinimumCommitmentAmount(JointPayee jointPayee)
        {
            return Transactions.All(tran =>
                DoesCommitmentAmountExceedMinimumCommitmentAmount(jointPayee.JointPayeeInternalId, tran));
        }

        private int? GetProjectId(JointPayee jointPayee)
        {
            var transaction = jointPayee.BillLineNumber != 0
                ? TransactionDataProvider.GetTransaction(Graph, jointPayee)
                : Transactions.Select(tran => tran).Distinct().SingleOrNull();
            return TransactionDataProvider.GetOriginalTransaction(Graph, transaction).ProjectID;
        }
    }
}