using System;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class VendorPaymentAmountValidationService : ValidationServiceBase
    {
        private readonly JointAmountToPayCalculationService jointAmountToPayCalculationService;

        public VendorPaymentAmountValidationService(APPaymentEntry graph)
            : base(graph)
        {
            jointAmountToPayCalculationService = new JointAmountToPayCalculationService(graph);
        }

        public void Validate(string errorMessage)
        {
            foreach (var adjustment in ActualBillAdjustments)
            {
                ValidateVendorPaymentAmount(adjustment, errorMessage);
            }
        }

        public void ValidateVendorPaymentAmount(APAdjust adjustment, string errorMessage)
        {
            InitializeServices(adjustment.AdjdLineNbr != 0);
            var totalJointAmountToPay = jointAmountToPayCalculationService.GetTotalJointAmountToPay(adjustment);
            var vendorPaymentAmount = adjustment.CuryAdjgAmt - totalJointAmountToPay;
            var vendorPreparedBalance =
                VendorPreparedBalanceCalculationService.GetVendorPreparedBalance(adjustment);
            if (vendorPaymentAmount > vendorPreparedBalance)
            {
                var totalNonReleasedCashDiscountTaken =
                    CashDiscountCalculationService.GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(adjustment) +
                    adjustment.CuryAdjgPPDAmt;
                var allowableAmountPaid =
                    vendorPreparedBalance + totalJointAmountToPay - totalNonReleasedCashDiscountTaken;
                allowableAmountPaid = Math.Max(allowableAmountPaid.GetValueOrDefault(), 0);
                ShowErrorMessage<APAdjust.curyAdjgAmt>(adjustment, errorMessage, allowableAmountPaid);
                ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, true);
            }
        }
    }
}
