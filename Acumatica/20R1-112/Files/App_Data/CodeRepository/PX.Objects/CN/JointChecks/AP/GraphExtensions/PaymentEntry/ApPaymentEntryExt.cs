using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Extensions;
using PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryExt : PXGraphExtension<APPaymentEntry>
    {
        public SelectFrom<JointPayeePayment>
            .InnerJoin<JointPayee>
            .On<JointPayee.jointPayeeId.IsEqual<JointPayeePayment.jointPayeeId>>.View JointPayeePayments;

        private JointPayeePaymentService jointPayeePaymentService;
        private ComplianceDocumentsService complianceDocumentsService;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable jointPayeePayments()
        {
	        return SelectFrom<JointPayeePayment>
                .InnerJoin<JointPayee>.On<JointPayee.jointPayeeId.IsEqual<JointPayeePayment.jointPayeeId>>
                .Where<JointPayeePayment.paymentDocType.IsEqual<APPayment.docType.FromCurrent>
                    .And<JointPayeePayment.paymentRefNbr.IsEqual<APPayment.refNbr.FromCurrent>>>
                .View.Select(Base);
        }

        public override void Initialize()
        {
            jointPayeePaymentService = new JointPayeePaymentService(Base);
            complianceDocumentsService = new ComplianceDocumentsService(Base);
        }

        public virtual void _(Events.RowSelected<APPayment> args)
        {
            if (args.Row != null && SiteMapExtension.IsChecksAndPaymentsScreenId())
            {
                UpdateJointCheckAvailability(args.Row);
                SetBillLineNumberVisibility();
                UpdateVendorAndJointPaymentAmounts(args.Row);
            }
        }

        public virtual void _(Events.RowSelected<JointPayeePayment> args)
        {
            var jointPayeePayment = args.Row;
            if (jointPayeePayment?.JointPayeeId != null && SiteMapExtension.IsChecksAndPaymentsScreenId())
            {
                UpdateJointAmountToPayAvailability(jointPayeePayment);
                PXUIFieldAttribute.SetDisplayName<JointPayee.billLineNumber>(Base.Caches<JointPayee>(),
                    JointCheckLabels.ApBillLineNumber);
            }
        }

        public virtual void APAdjust_RowInserted(PXCache cache, PXRowInsertedEventArgs args)
        {
            var adjustment = args.Row as APAdjust;
            if (adjustment?.AdjdDocType == APDocType.Invoice && IsCheck() &&
                SiteMapExtension.IsChecksAndPaymentsScreenId())
            {
                jointPayeePaymentService.AddJointPayeePayments(adjustment);
            }
        }

        public virtual void _(Events.RowDeleted<APPayment> args)
        {
            jointPayeePaymentService.DeleteJointPayeePayments(args.Row);
        }

        public virtual void _(Events.RowDeleted<APAdjust> args)
        {
            if (args.Row.AdjdDocType == APDocType.Invoice && !IsCurrentPaymentDeleted())
            {
                jointPayeePaymentService.DeleteJointPayeePayments(args.Row);
            }
        }

        public virtual void _(Events.RowPersisting<JointPayeePayment> args)
        {
            if (args.Operation != PXDBOperation.Delete)
            {
                args.Row.PaymentRefNbr = Base.Document.Current.RefNbr;
            }
        }

        public virtual void APAdjust_RowPersisting(PXCache cache, PXRowPersistingEventArgs args)
        {
            if (args.Row is APAdjust adjustment)
            {
                switch (args.Operation)
                {
                    case PXDBOperation.Delete:
                        jointPayeePaymentService.ClosePaymentCycleWorkflowIfNeeded(adjustment);
                        break;
                    case PXDBOperation.Insert:
                        jointPayeePaymentService.InitializePaymentCycleWorkflowIfRequired(adjustment);
                        complianceDocumentsService.UpdateComplianceDocumentsIfRequired(adjustment);
                        break;
                }
            }
        }

        private bool IsCheck()
        {
            return Base.Document.Current?.DocType == APDocType.Check;
        }

        private bool IsVoidCheck()
        {
            return Base.Document.Current?.DocType == APDocType.VoidCheck;
        }

        private void UpdateJointAmountToPayAvailability(JointPayeePayment jointPayeePayment)
        {
            var jointPayee = JointPayeeDataProvider.GetJointPayee(Base, jointPayeePayment);
            var adjustment = AdjustmentDataProvider.GetAdjustment(Base, jointPayeePayment);
            var hasReversedAdjustments = DoesCheckContainReversedAdjustments(jointPayeePayment);
            var isZeroJointBalance = jointPayee.JointBalance == 0 && !hasReversedAdjustments;
            var isReleased = adjustment?.Released == true;
            var isVoidAdjustment = !IsVoidCheck() && adjustment?.Voided == true;
            var isReadOnly = isZeroJointBalance || isReleased || isVoidAdjustment;
            PXUIFieldAttribute.SetReadOnly<JointPayeePayment.jointAmountToPay>(
                JointPayeePayments.Cache, jointPayeePayment, isReadOnly);
        }

        private bool DoesCheckContainReversedAdjustments(JointPayeePayment jointPayeePayment)
        {
            return JointPayeePayments.SelectMain()
                .Any(jpp => jpp.JointAmountToPay < 0 && jpp.JointPayeeId == jointPayeePayment.JointPayeeId);
        }

        private void UpdateJointCheckAvailability(APPayment payment)
        {
            var extension = PXCache<APPayment>.GetExtension<ApPaymentExt>(payment);
            extension.IsJointCheck = JointPayeePayments.SelectMain().Any();
            UpdateJointCheckVisibility(extension.IsJointCheck == true);
            JointPayeePayments.AllowUpdate = Base.Adjustments.AllowUpdate;
        }

        private void UpdateJointCheckVisibility(bool isJointCheck)
        {
            var isExpectedType = IsCheck() || IsVoidCheck();
            PXUIFieldAttribute.SetVisible<ApPaymentExt.isJointCheck>(Base.Document.Cache, null, isExpectedType);
            PXUIFieldAttribute.SetVisible(JointPayeePayments.Cache, null, isExpectedType);
            PXUIFieldAttribute.SetVisible<ApPaymentExt.jointPaymentAmount>(Base.Document.Cache, null,
                isExpectedType && isJointCheck);
            PXUIFieldAttribute.SetVisible<ApPaymentExt.vendorPaymentAmount>(Base.Document.Cache, null,
                isExpectedType && isJointCheck);
        }

        private void SetBillLineNumberVisibility()
        {
            var shouldShowBillLineNumber = JointPayeePayments.SelectMain().Any(jpp => jpp.IsPaymentByline());
            PXUIFieldAttribute.SetVisible<JointPayee.billLineNumber>(JointPayeePayments.Cache, null,
                shouldShowBillLineNumber);
        }

        private void UpdateVendorAndJointPaymentAmounts(APPayment payment)
        {
            var extension = PXCache<APPayment>.GetExtension<ApPaymentExt>(payment);
            extension.JointPaymentAmount = JointPayeePayments.SelectMain().Sum(jpp => jpp.JointAmountToPay);
            extension.VendorPaymentAmount = payment.CuryOrigDocAmt - extension.JointPaymentAmount;
        }

        private bool IsCurrentPaymentDeleted()
        {
            var status = Base.Document.Cache.GetStatus(Base.Document.Current);
            return status.IsIn(PXEntryStatus.Deleted, PXEntryStatus.InsertedDeleted);
        }
    }
}