using System.Collections.Generic;
using System.Linq;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class ReversedJointPayeePaymentsValidationService : ValidationServiceBase
    {
        public ReversedJointPayeePaymentsValidationService(APPaymentEntry graph)
            : base(graph)
        {
        }

        public void Validate()
        {
            var atLeastOneVoidAdjustmentExist = Adjustments.Any(adjustment => adjustment.Voided == true);
            if (atLeastOneVoidAdjustmentExist)
            {
                var showErrorOnPersist = JointPayeePaymentDataProvider
                    .GetCurrentJointPayeePaymentsByVendorGroups(Graph, Graph.Document.Current)
                    .Select(ValidateReversedJointPayeePayments).Any(isValid => !isValid);
                ShowErrorOnPersistIfRequired(Graph.Caches[typeof(JointPayeePayment)], showErrorOnPersist);
            }
        }

        private bool ValidateReversedJointPayeePayments(List<JointPayeePayment> jointPayeePayments)
        {
            if (jointPayeePayments.Sum(jpp => jpp.JointAmountToPay) == 0)
            {
                return true;
            }
            foreach (var jointPayeePayment in jointPayeePayments)
            {
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(
                    jointPayeePayment, JointCheckMessages.JointPayeeAmountIsNotEqualToTheOriginalAmount);
            }
            return false;
        }
    }
}
