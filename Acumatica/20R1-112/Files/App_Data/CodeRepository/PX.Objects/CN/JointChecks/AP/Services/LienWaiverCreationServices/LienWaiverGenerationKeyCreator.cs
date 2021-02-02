using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Comparers;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
    public class LienWaiverGenerationKeyCreator : LienWaiverGenerationDataProviderBase, ILienWaiverGenerationKeyCreator
    {
        public LienWaiverGenerationKeyCreator(PXGraph graph)
            : base(graph)
        {
        }

        public IEnumerable<LienWaiverGenerationKey> CreateGenerationKeys(IEnumerable<APTran> transactions,
            IReadOnlyCollection<JointPayee> jointPayees, APRegister payment)
        {
            return transactions.SelectMany(tran => CreateLienWaiverGroupingKeys(jointPayees, payment, tran))
                .Distinct(new LienWaiverGenerationKeyComparer());
        }

        private IEnumerable<LienWaiverGenerationKey> CreateLienWaiverGroupingKeys(
            IReadOnlyCollection<JointPayee> jointPayees, APRegister payment, APTran transaction)
        {
            var adjustments = AdjustmentDataProvider.GetInvoiceAdjustments(Graph, transaction.RefNbr);
            return adjustments.Any()
                ? CreateGroupingKeys(jointPayees, payment, transaction)
                : CreateGroupingKeysForPaymentByLines(jointPayees, payment, transaction);
        }

        private IEnumerable<LienWaiverGenerationKey> CreateGroupingKeys(IReadOnlyCollection<JointPayee> jointPayees,
            APRegister payment, APTran transaction)
        {
            return jointPayees.IsEmpty()
                ? GetLienWaiverGroupingKey(transaction, null, payment).AsSingleEnumerable()
                : jointPayees.Select(jp => GetLienWaiverGroupingKey(transaction, jp.JointPayeeInternalId, payment));
        }

        private IEnumerable<LienWaiverGenerationKey> CreateGroupingKeysForPaymentByLines(
            IEnumerable<JointPayee> jointPayees, APRegister payment, APTran transaction)
        {
            var payees = jointPayees.Where(jp => jp.BillLineNumber == transaction.LineNbr).ToList();
            return CreateGroupingKeys(payees, payment, transaction);
        }

        private LienWaiverGenerationKey GetLienWaiverGroupingKey(APTran transaction, int? jointPayeeVendorId,
            APRegister payment)
        {
            var originalTransaction = TransactionDataProvider.GetOriginalTransaction(Graph, transaction);
            return new LienWaiverGenerationKey
            {
                ProjectId = originalTransaction.ProjectID,
                VendorId = payment.VendorID,
                JointPayeeVendorId = jointPayeeVendorId,
                OrderNumber = GetCommitment(originalTransaction).OrderNbr
            };
        }
    }
}