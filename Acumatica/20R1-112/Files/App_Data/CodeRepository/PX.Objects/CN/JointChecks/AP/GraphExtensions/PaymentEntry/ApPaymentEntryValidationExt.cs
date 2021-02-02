using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices.Validation;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryValidationExt : PXGraphExtension<APPaymentEntry>
    {
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		private AdjustmentAmountPaidValidationService adjustmentAmountPaidValidationService;
        private CashDiscountValidationService cashDiscountValidationService;
        private DebitAdjustmentsValidationService debitAdjustmentsValidationService;
        private JointAmountToPayValidationService jointAmountToPayValidationService;
        private PaymentCycleWorkflowValidationService paymentCycleWorkflowValidationService;
        private ReversedJointPayeePaymentsValidationService reversedJointPayeePaymentsValidationService;
        private VendorPaymentAmountValidationService vendorPaymentAmountValidationService;

        public override void Initialize()
        {
            adjustmentAmountPaidValidationService = new AdjustmentAmountPaidValidationService(Base);
            cashDiscountValidationService = new CashDiscountValidationService(Base);
            debitAdjustmentsValidationService = new DebitAdjustmentsValidationService(Base);
            jointAmountToPayValidationService = new JointAmountToPayValidationService(Base);
            paymentCycleWorkflowValidationService = new PaymentCycleWorkflowValidationService(Base);
            reversedJointPayeePaymentsValidationService = new ReversedJointPayeePaymentsValidationService(Base);
            vendorPaymentAmountValidationService = new VendorPaymentAmountValidationService(Base);
        }

        public virtual void APPayment_RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (args.Operation != PXDBOperation.Delete && SiteMapExtension.IsChecksAndPaymentsScreenId())
            {
                switch (Base.Document.Current?.DocType)
                {
                    case APDocType.Check:
                        ValidateCheck();
                        break;
                    case APDocType.DebitAdj:
                        ValidateDebitAdjustment();
                        break;
                    case APDocType.Prepayment:
                        ValidatePrepayment();
                        break;
                }
            }
        }

        public virtual void _(Events.RowInserted<APAdjust> args)
        {
            if (args.Row?.AdjdDocType == APDocType.Invoice && IsCheck())
            {
                paymentCycleWorkflowValidationService.Validate(args.Row.SingleToList());
            }
        }

        public virtual void APAdjust_CuryAdjdAmt_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs args)
        {
            if (args.Row is APAdjust adjust && adjust.Voided == false && IsCheck())
            {
                adjustmentAmountPaidValidationService.ValidateAmountPaid(adjust, (decimal) args.NewValue, false);
            }
        }

        private bool IsCheck()
        {
            return Base.Document.Current?.DocType == APDocType.Check;
        }

        private void ValidateCheck()
        {
            paymentCycleWorkflowValidationService.Validate();
            reversedJointPayeePaymentsValidationService.Validate();
            jointAmountToPayValidationService.ValidateJointAmountToPayExceedBalance();
            cashDiscountValidationService.Validate();
            vendorPaymentAmountValidationService.Validate(JointCheckMessages.AmountPaidExceedsVendorBalance);
            adjustmentAmountPaidValidationService.ValidateAmountsPaid();
            debitAdjustmentsValidationService.ValidateDebitAdjustmentsIfRequired();
        }

        private void ValidatePrepayment()
        {
            vendorPaymentAmountValidationService.Validate(JointCheckMessages.AmountPaidExceedsVendorBalanceForPrepayment);
        }

        private void ValidateDebitAdjustment()
        {
            vendorPaymentAmountValidationService.Validate(JointCheckMessages.AmountPaidExceedsVendorBalanceForDebitAdjustment);
            cashDiscountValidationService.Validate();
        }
	}
}
