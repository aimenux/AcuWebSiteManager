using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.Comparers;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryReverseExtension : PaymentRevertBaseExtension
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual IEnumerable ReverseApplication(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            var paymentEntryValidationExtension = Base.GetExtension<ApPaymentEntryValidationExt>();
            Base.RowInserted.RemoveHandler<APAdjust>(Base1.APAdjust_RowInserted);
            Base.FieldVerifying.RemoveHandler<APAdjust.curyAdjdAmt>(paymentEntryValidationExtension
                .APAdjust_CuryAdjdAmt_FieldVerifying);
            try
            {
                VoidAutomaticallyCreatedLienWaivers();
                return ReverseAdjustment(adapter, baseHandler);
            }
            finally
            {
                Base.RowInserted.AddHandler<APAdjust>(Base1.APAdjust_RowInserted);
                Base.FieldVerifying.AddHandler<APAdjust.curyAdjdAmt>(paymentEntryValidationExtension
                    .APAdjust_CuryAdjdAmt_FieldVerifying);
            }
        }

        public virtual void JointPayeePayment_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            baseHandler(cache, args);
            var jointPayeePayment = args.Row as JointPayeePayment;
            var hasAnyReversedAdjustments = Base.Adjustments.SelectMain()
                .Any(adjustment => adjustment.Released == false && adjustment.Voided == true);
            if (jointPayeePayment?.JointPayeeId != null && hasAnyReversedAdjustments)
            {
                var jointPayee = JointPayeeDataProvider.GetJointPayee(Base, jointPayeePayment);
                PXUIFieldAttribute.SetEnabled<JointPayeePayment.jointAmountToPay>(
                    cache, jointPayeePayment, HasSameVendorAsReversedJointPayeePayment(jointPayee));
            }
        }

        private void VoidAutomaticallyCreatedLienWaivers()
        {
            var lienWaiverVoidService = new LienWaiverVoidService();
            var paymentEntryExtension = Base.GetExtension<PX.Objects.CN.Compliance.AP.GraphExtensions.ApPaymentEntryExt>();
            var complianceDocuments = GetComplianceDocumentsFromAdjustmentHistory().ToList();
            if (complianceDocuments.Any() && lienWaiverVoidService.IsVoidOfAutomaticallyGeneratedLienWaiverConfirmed(
                Base, LienWaiverReferencedDocument.ApBill))
            {
                lienWaiverVoidService.VoidAutomaticallyCreatedLienWaivers(
                    paymentEntryExtension.ComplianceDocuments.Cache, complianceDocuments);
            }
        }

        private bool HasSameVendorAsReversedJointPayeePayment(JointPayee jointPayee)
        {
            var jointPayeeComparer = new JointPayeeComparer();
            var reversedJointPayeesGroups = Base1.JointPayeePayments.Select().RowCast<JointPayee>()
                .GroupBy(jpp => jpp.JointPayeeId).Where(group => group.HasAtLeastTwoItems());
            return reversedJointPayeesGroups.Any(reversedJointPayeePaymentsGroup =>
                jointPayeeComparer.Equals(reversedJointPayeePaymentsGroup.FirstOrDefault(), jointPayee));
        }

        private IEnumerable ReverseAdjustment(PXAdapter adapter, Func<PXAdapter, IEnumerable> reverse)
        {
            var originalAdjustment = Base.Adjustments_History.Current;
            var reverseResult = reverse(adapter);
            if (originalAdjustment != null)
            {
                ReverseJointPayeePayments(originalAdjustment);
            }
            return reverseResult;
        }

        private void ReverseJointPayeePayments(APAdjust originalAdjustment)
        {
            var reversedAdjustment = AdjustmentDataProvider.GetReversedAdjustment(Base, originalAdjustment);
            if (reversedAdjustment == null)
            {
                return;
            }
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(Base, reversedAdjustment);
            if (jointPayeePayments.IsEmpty())
            {
                AddRevertingJointPayeePayments(originalAdjustment, reversedAdjustment);
            }
        }

        private IEnumerable<ComplianceDocument> GetComplianceDocumentsFromAdjustmentHistory()
        {
            var billReferenceNumbers = Base.Adjustments_History.SelectMain()
                .Select(adj => adj.DisplayRefNbr).Cast<object>().ToArray();
            return SelectFrom<ComplianceDocument>
                .LeftJoin<ComplianceDocumentReference>
                    .On<ComplianceDocument.billID.IsEqual<ComplianceDocumentReference.complianceDocumentReferenceId>>
                .Where<ComplianceDocumentReference.referenceNumber.IsIn<P.AsString>
                    .And<ComplianceDocument.isCreatedAutomatically.IsEqual<True>>>.View
                .Select(Base, billReferenceNumbers).FirstTableItems;
        }
    }
}