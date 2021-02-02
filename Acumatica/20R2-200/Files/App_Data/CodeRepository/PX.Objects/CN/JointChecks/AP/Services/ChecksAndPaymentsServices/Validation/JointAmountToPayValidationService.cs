using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Extensions;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class JointAmountToPayValidationService : ValidationServiceBase
    {
        private readonly JointPayeeAmountsCalculationService jointPayeeAmountsCalculationService;
        private readonly PXCache jointPayeePaymentCache;

        public JointAmountToPayValidationService(APPaymentEntry graph)
            : base(graph)
        {
            jointPayeeAmountsCalculationService = new JointPayeeAmountsCalculationService(graph);
            jointPayeePaymentCache = graph.Caches<JointPayeePayment>();
        }

        public void ValidateJointAmountToPayExceedBalance()
        {
            var actualJointPayeePayments = ActualAdjustments
                .SelectMany(adjustment => JointPayeePaymentDataProvider.GetJointPayeePayments(Graph, adjustment))
                .ToList();
            foreach (var jointPayeePayment in actualJointPayeePayments)
            {
                ValidateJointAmountToPayExceedJointBalance(jointPayeePayment);
                ValidateJointAmountToPayExceedBillBalance(jointPayeePayment);
                ValidateJointAmountToPayExceedJointPreparedBalance(jointPayeePayment);
                ValidateJointPayeePaymentTotalAmountToPayExceedBillAmount(actualJointPayeePayments, jointPayeePayment);
            }
        }

        private void ValidateJointAmountToPayExceedJointBalance(JointPayeePayment jointPayeePayment)
        {
            var controlAmountToPay = GetControlAmountToPay(jointPayeePayment);
            if (jointPayeePayment.JointAmountToPay > controlAmountToPay)
            {
                var message = jointPayeePayment.IsPaymentByline()
                    ? JointCheckMessages.JointAmountToPayCannotExceedJointPayeeLineBalance
                    : JointCheckMessages.JointAmountToPayCannotExceedJointPayeeBalance;
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment, message);
                ShowErrorOnPersistIfRequired(jointPayeePaymentCache, true);
            }
        }

        private void ValidateJointAmountToPayExceedBillBalance(JointPayeePayment jointPayeePayment)
        {
            var billAmount = GetBillAmount(jointPayeePayment);
            if (jointPayeePayment.JointAmountToPay > billAmount)
            {
                var message = jointPayeePayment.IsPaymentByline()
                    ? JointCheckMessages.JointAmountToPayExceedsBillLineBalance
                    : JointCheckMessages.JointAmountToPayExceedsBillBalance;
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment, message, billAmount);
                ShowErrorOnPersistIfRequired(jointPayeePaymentCache, true);
            }
        }

        private void ValidateJointAmountToPayExceedJointPreparedBalance(JointPayeePayment jointPayeePayment)
        {
            var jointPayee = JointPayeeDataProvider.GetJointPayee(Graph, jointPayeePayment);
            var jointPreparedBalance = jointPayeeAmountsCalculationService.GetJointPreparedBalance(jointPayee) +
                jointPayeePayment.JointAmountToPay.GetValueOrDefault();
            if (jointPayeePayment.JointAmountToPay > jointPreparedBalance)
            {
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment,
                    JointCheckMessages.JointAmountToPayCannotExceedJointPayeePreparedBalance, jointPreparedBalance);
                ShowErrorOnPersistIfRequired(jointPayeePaymentCache, true);
            }
        }

        private void ValidateJointPayeePaymentTotalAmountToPayExceedBillAmount(
            IEnumerable<JointPayeePayment> jointPayeePayments, JointPayeePayment jointPayeePayment)
        {
            var totalAmountToPayForBill = jointPayeePayments
                .Where(jpp => jpp.IsRelatedToSameInvoice(jointPayeePayment))
                .Sum(jpp => jpp.JointAmountToPay);
            var billAmount = GetBillAmount(jointPayeePayment);
            if (totalAmountToPayForBill > billAmount)
            {
                var message = jointPayeePayment.IsPaymentByline()
                    ? JointCheckMessages.TotalJointAmountToPayExceedsBillLineBalance
                    : JointCheckMessages.TotalJointAmountToPayExceedsBillBalance;
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment, message);
                ShowErrorOnPersistIfRequired(jointPayeePaymentCache, true);
            }
        }

        private decimal? GetBillAmount(JointPayeePayment jointPayeePayment)
        {
            var adjustment = Adjustments.Single(adjust => adjust.IsRelatedJointPayeePayment(jointPayeePayment));
            return InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr).CuryOrigDocAmt;
        }

        private decimal? GetControlAmountToPay(JointPayeePayment jointPayeePayment)
        {
            var jointPayee = JointPayeeDataProvider.GetJointPayee(Graph, jointPayeePayment);
            var reversedPayments = GetReversedJointPayeePayments(jointPayeePayment.JointPayeeId).ToList();
            return reversedPayments.Any()
                ? -reversedPayments.Sum(jpp => jpp.JointAmountToPay)
                : jointPayee.JointBalance;
        }

        private IEnumerable<JointPayeePayment> GetReversedJointPayeePayments(int? jointPayeeId)
        {
            var paymentEntryExtension = Graph.GetExtension<ApPaymentEntryExt>();
            var jointPayeePayments = paymentEntryExtension.JointPayeePayments.SelectMain();
            return jointPayeePayments.Where(jpp => jpp.JointAmountToPay < 0 && jpp.JointPayeeId == jointPayeeId);
        }
    }
}
