using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class DebitAdjustmentsValidationService : ValidationServiceBase
    {
        public DebitAdjustmentsValidationService(APPaymentEntry graph)
            : base(graph)
        {
        }

        private IEnumerable<APAdjust> DebitAdjustments =>
            ActualAdjustments.Where(adjust => adjust.AdjdDocType == APDocType.DebitAdj);

        public void ValidateDebitAdjustmentsIfRequired()
        {
            if (IsJointCheck() && DebitAdjustments.Any())
            {
                var allowableDebitAmount = GetAllowableDebitAdjustmentsAmount();
                var totalDebitAmountsPaid = DebitAdjustments.Sum(adjustment => adjustment.CuryAdjgAmt);
                if (totalDebitAmountsPaid > allowableDebitAmount)
                {
                    ShowErrorMessageOnDebitAdjustments(allowableDebitAmount);
                }
            }
        }

        private void ShowErrorMessageOnDebitAdjustments(decimal? totalVendorPreparedBalance)
        {
            foreach (var adjustment in DebitAdjustments)
            {
                ShowErrorMessage<APAdjust.curyAdjgAmt>(adjustment,
                    JointCheckMessages.MaxDebitAmountApplication, totalVendorPreparedBalance);
            }
            ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, true);
        }

        private bool IsJointCheck()
        {
            var paymentExtension = PXCache<APPayment>.GetExtension<ApPaymentExt>(Graph.Document.Current);
            return paymentExtension.IsJointCheck == true;
        }

        private decimal GetAllowableDebitAdjustmentsAmount()
        {
            var paymentExtension = PXCache<APPayment>.GetExtension<ApPaymentExt>(Graph.Document.Current);
            var vendorBalance = GetAllAmountsToPayByBillAndCreditAdjustments() - paymentExtension.JointPaymentAmount;
            return Math.Max(vendorBalance.GetValueOrDefault(), decimal.Zero);
        }

        private decimal? GetAllAmountsToPayByBillAndCreditAdjustments()
        {
            return ActualAdjustments.Where(adjust => adjust.AdjdDocType.IsIn(APDocType.Invoice, APDocType.CreditAdj))
                .Sum(adjustment => adjustment.CuryAdjgAmt);
        }
    }
}
