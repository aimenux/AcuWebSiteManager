using System;
using PX.Common;

namespace PX.Objects.CN.JointChecks.Descriptor
{
    [PXLocalizable]
    public class JointCheckMessages
    {
        public const string OnlyVendorsShouldBeAllowed = "Only Vendors should be allowed.";

        public const string UnlinkedApBillLineFromJointPayees = "AP Bill Line will be unlinked from the Joint " +
            "Payees from the Joint Payees Tab. Do you want to continue?";

        public const string JointPayeeAmountIsNotEqualToTheOriginalAmount =
            "Amount is not paid to the originally paid Payee or Amount is not equal to the originally paid Amount.";

        public const string PaymentCycleWorkflowIsStarted = "Process unreleased checks for the bill to proceed.";
        public const string JointPayeesRequired = "At least one Joint Payee is required to be specified.";
        public const string TotalAmountToPayExceedsBalance = "Total Amount To Pay exceeds the Bill Balance.";

		[Obsolete]
        public const string CorrectionToThisAmountIsNotAvailable =
            "Correction to this amount is not available. Min. value of the correction is {0}. " +
            "Delete non-released checks for this Joint Payee to make the correction to the total unpaid amount.";

        public const string EnteredJointAmountOwedIsIncorrect = 
			"The entered Joint Amount Owed is incorrect. It must be greater than or equal to {0} and less than or equal to {1}. (Unreleased payments are included in the minimum amount and reduce the maximum amount.)";
        public const string TotalAmountOwedToAllJointPayeesIsIncorrect =
	        "The total amount owed to all joint payees is incorrect. It must be greater than or equal to {0} and less than or equal to {1}. (Unreleased payments are included in the minimum amount and reduce the maximum amount.)";


        public const string OnlyOneVendorIsAllowed = "Must specified Joint Payee (Vendor) or Joint Payee.";
        public const string ValueCanNotBeNegative = "The value can't be negative.";
        public const string TheValueMustBeGreaterThanZero = "The value must be greater than zero.";
		public const string TheValueMustNotBeLesserThanZero = "The value must not be lesser than zero.";
		public const string MaxDebitAmountApplication = "Max Debit Amount Application is {0}";

        public const string AmountPaidShouldBeEqualOrGreaterErrorTemplate =
            "Amount Paid should be greater or equal than {0}.";

        public const string AmountOwedExceedsBalance =
            "Joint Amounts Owed to Joint Payees exceed Balance of AP Bill.";

        public const string AmountOwedExceedsBalanceWithCashDiscount =
            "Joint Amounts Owed to Joint Payees exceed Balance of AP Bill with Cash Discount.";

        public const string AmountPaidWithCashDiscountTakenExceedsVendorBalance =
            "The sum of the amounts in Amount Paid and Cash Discount Taken should not exceed {0}.";

        public const string DeleteJointPayeesWarning =
            "All data will be deleted from the Joint Payees Tab. Do you want to continue?";

        public const string JointAmountToPayExceedsJointPayeeBalance =
            "Joint Amount to Pay exceeds the Joint Payee Balance of the paid AP Bill.";

        public const string JointAmountToPayExceedsBillBalance =
            "Joint Amount to Pay exceeds the Balance of the paid AP Bill.";

        public const string JointAmountToPayExceedsBillLineBalance =
            "Joint Amount to Pay exceeds the Balance of the paid AP Bill Line.";

        public const string TotalJointAmountToPayExceedsBillBalance =
            "Total Joint Amount to Pay exceeds the Balance of the paid AP Bill.";

        public const string TotalJointAmountToPayExceedsBillLineBalance =
            "Total Joint Amount to Pay exceeds the Balance of the paid AP Bill Line.";

        public const string TotalJointAmountPaidExceedsBillLineBalance =
            "Total Joint Paid Amount per Bill Line exceeds Bill Line Balance.";

        public const string JointAmountToPayCannotExceedJointPayeeBalance =
            "Joint Amount to Pay exceeds the Joint Payee Balance of the paid AP Bill.";

        public const string JointAmountToPayCannotExceedJointPayeeLineBalance =
            "Joint Amount to Pay exceeds the Joint Payee Balance of the paid AP Bill Line.";

        public const string TotalJointAmountOwedExceedsApBillLineAmount =
            "Total Joint Amount Owed per Bill Line exceeds AP Bill Line Amount.";

        public const string TotalJointAmountOwedExceedsApBillLineAmountWithCashDiscount =
            "Total Joint Amount Owed per Bill Line exceeds AP Bill Line Amount with Cash Discount.";

        public const string JointAmountOwedExceedsApBillLineAmount =
            "Joint Amount Owed for Joint Payee exceeds AP Bill Line Amount.";

        public const string JointAmountOwedExceedsApBillLineAmountWithCashDiscount =
            "Joint Amount Owed per Bill Line exceeds AP Bill Line Amount with Cash Discount.";

        public const string AmountPaidExceedsVendorBalance =
            "Amount Paid exceeds the Vendor Balance of the AP Bill Line. " + AmountPaidCanBeEqualOrLess;

        public const string AmountPaidExceedsBillBalance =
            "Amount Paid exceeds the Balance of the AP Bill. " + AmountPaidCanBeEqualToOrLessThen;

        public const string AmountPaidExceedsBillLineBalance =
            "Amount Paid exceeds the Vendor Balance of the AP Bill Line. " + AmountPaidCanBeEqualToOrLessThen;

        public const string TotalAmountExceedsVendorBalance =
            "Total Amount To Pay exceeds the Bill Balance. " + AmountPaidCanBeEqualToOrLessThen;

        public const string AmountPaidExceedsVendorBalanceForDebitAdjustment =
            "The debit adjustment" + CannotBeAppliedToTheJointAmounts;

        public const string AmountPaidExceedsVendorBalanceForPrepayment =
            "The prepayment" + CannotBeAppliedToTheJointAmounts;

        public const string JointAmountToPayCannotExceedJointPayeePreparedBalance =
            "The Joint Amount To Pay cannot exceed {0}. " +
            "Delete the non-released payment amounts for this Joint Payee to increase the possible Amount To Pay.";

        private const string CannotBeAppliedToTheJointAmounts =
            " cannot be applied to the Joint Amounts. The amount available for application is {0}.";

        private const string AmountPaidCanBeEqualToOrLessThen = "Amount Paid can be equal to or less than {0}.";
        private const string AmountPaidCanBeEqualOrLess = "Amount Paid can be equal or less {0}.";
    }
}