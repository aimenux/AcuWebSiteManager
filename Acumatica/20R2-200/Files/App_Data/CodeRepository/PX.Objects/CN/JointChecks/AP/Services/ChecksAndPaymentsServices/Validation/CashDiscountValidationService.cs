using PX.Objects.AP;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation
{
    public class CashDiscountValidationService : ValidationServiceBase
    {
        public CashDiscountValidationService(APPaymentEntry graph)
            : base(graph)
        {
        }

        public void Validate()
        {
            foreach (var adjustment in ActualAdjustments)
            {
                InitializeServices(adjustment.AdjdLineNbr != 0);
                var allowableCashDiscount = CashDiscountCalculationService.GetAllowableCashDiscount(adjustment);
                if (adjustment.CuryAdjgPPDAmt > allowableCashDiscount)
                {
                    ShowErrorMessage<APAdjust.curyAdjgPPDAmt>(adjustment,
                        JointCheckMessages.AmountPaidWithCashDiscountTakenExceedsVendorBalance, allowableCashDiscount);
                    ShowErrorOnPersistIfRequired(Graph.Adjustments.Cache, true);
                }
            }
        }
    }
}
