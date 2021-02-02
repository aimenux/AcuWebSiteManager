using System.Linq;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class AdjustmentAmountPaidValidationService : ValidationServiceBase
    {
        private readonly JointAmountToPayCalculationService jointAmountToPayCalculationService;

        public AdjustmentAmountPaidValidationService(APPaymentEntry graph)
            : base(graph)
        {
            jointAmountToPayCalculationService = new JointAmountToPayCalculationService(graph);
        }

        public void ValidateAmountsPaid()
        {
            foreach (var adjustment in ActualAdjustments)
            {
                ValidateAmountPaid(adjustment, adjustment.CuryAdjgAmt, true);
            }
        }

        public void ValidateAmountPaid(APAdjust adjustment, decimal? amountPaid, bool doNeedShowErrorOnPersist)
        {
            InitializeServices(adjustment.AdjdLineNbr != 0);
            ValidateAmountPaidGreaterThanTotalJointAmountToPay(adjustment, amountPaid, doNeedShowErrorOnPersist);
            ValidateAmountPaidExceedsBillBalance(adjustment, amountPaid, doNeedShowErrorOnPersist);
            ValidateAmountPaidExceedsVendorBalanceWithCashDiscountTaken(adjustment, amountPaid, doNeedShowErrorOnPersist);
        }

        private void ValidateAmountPaidGreaterThanTotalJointAmountToPay(APAdjust adjustment, decimal? amountPaid,
            bool doNeedShowErrorOnPersist)
        {
            var totalJointAmountToPay = jointAmountToPayCalculationService.GetTotalJointAmountToPay(adjustment);
            if (amountPaid < totalJointAmountToPay)
            {
                ShowErrorMessage<APAdjust.curyAdjgAmt>(adjustment, amountPaid,
                    JointCheckMessages.AmountPaidShouldBeEqualOrGreaterErrorTemplate, totalJointAmountToPay);
                ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, doNeedShowErrorOnPersist);
            }
        }

        private void ValidateAmountPaidExceedsVendorBalanceWithCashDiscountTaken(APAdjust adjustment, decimal? amountPaid,
            bool doNeedShowErrorOnPersist)
        {
            var vendorPreparedBalance =
                VendorPreparedBalanceCalculationService.GetVendorPreparedBalance(adjustment);
            var totalJointAmountToPay = jointAmountToPayCalculationService.GetTotalJointAmountToPay(adjustment);
            var cashDiscountTakenFromOtherNonReleasedChecks =
                CashDiscountCalculationService.GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(adjustment);
            if (amountPaid > vendorPreparedBalance - cashDiscountTakenFromOtherNonReleasedChecks -
                adjustment.CuryAdjgPPDAmt + totalJointAmountToPay)
            {
                var amountPaidLimit = vendorPreparedBalance - cashDiscountTakenFromOtherNonReleasedChecks +
                    totalJointAmountToPay;
                ShowErrorMessage<APAdjust.curyAdjgAmt>(adjustment, amountPaid,
                    JointCheckMessages.AmountPaidWithCashDiscountTakenExceedsVendorBalance, amountPaidLimit);
                ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, doNeedShowErrorOnPersist);
            }
        }

        private void ValidateAmountPaidExceedsBillBalance(APAdjust adjustment, decimal? amountPaid,
            bool doNeedShowErrorOnPersist)
        {
            var invoiceAdjustments = AdjustmentDataProvider.GetInvoiceAdjustments(Graph, adjustment.AdjdRefNbr);
            var unappliedBalance = invoiceAdjustments
                .Sum(adjust => adjust.CuryAdjgAmt + adjust.CuryAdjgPPDAmt);
            var billBalance = InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr)
                .CuryOrigDocAmt.GetValueOrDefault();
            var allowableAmountPaid = billBalance - unappliedBalance + amountPaid;
            if (amountPaid > allowableAmountPaid)
            {
                ShowErrorMessage<APAdjust.curyAdjgAmt>(adjustment, amountPaid, adjustment.AdjdLineNbr != 0
                    ? JointCheckMessages.AmountPaidExceedsBillLineBalance
                    : JointCheckMessages.AmountPaidExceedsBillBalance, allowableAmountPaid);
                ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, doNeedShowErrorOnPersist);
            }
        }
    }
}
