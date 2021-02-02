using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
	public abstract class PaymentRevertBaseExtension : PXGraphExtension<ApPaymentEntryExt, APPaymentEntry>
    {
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		protected void AddRevertingJointPayeePayments(APAdjust originalAdjustment, APAdjust revertedAdjustment)
        {
            var originalJointPayeePayments =
                JointPayeePaymentDataProvider.GetJointPayeePayments(Base, originalAdjustment);
            foreach (var jointPayeePayment in originalJointPayeePayments)
            {
                AddRevertingJointPayeePayment(jointPayeePayment, revertedAdjustment);
            }
        }

        private void AddRevertingJointPayeePayment(JointPayeePayment originalJointPayeePayment, APAdjust adjustment)
        {
            var jointPayeePayment = CreateRevertingJointPayeePayment(originalJointPayeePayment, adjustment);
            Base1.JointPayeePayments.Insert(jointPayeePayment);
        }

        private static JointPayeePayment CreateRevertingJointPayeePayment(JointPayeePayment originalJointPayeePayment,
            APAdjust adjustment)
        {
            return new JointPayeePayment
            {
                JointPayeeId = originalJointPayeePayment.JointPayeeId,
                JointAmountToPay = -originalJointPayeePayment.JointAmountToPay,
                PaymentDocType = adjustment.AdjgDocType,
                PaymentRefNbr = adjustment.AdjgRefNbr,
                InvoiceRefNbr = adjustment.AdjdRefNbr,
                InvoiceDocType = adjustment.AdjdDocType,
                AdjustmentNumber = adjustment.AdjNbr,
                BillLineNumber = originalJointPayeePayment.BillLineNumber
            };
        }
    }
}