using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices;
using PX.Objects.CS;
using Constants = PX.Objects.CN.Common.Descriptor.Constants;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryLienWaiverExtension : PXGraphExtension<ApPaymentEntryExt, APPaymentEntry>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private LienWaiverValidationService lienWaiverValidationService;
        private LienWaiverProcessingValidationService lienWaiverProcessingValidationService;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        public override void Initialize()
        {
            lienWaiverValidationService = new LienWaiverValidationService(Base, ProjectDataProvider);
            lienWaiverProcessingValidationService = new LienWaiverProcessingValidationService(Base);
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void _(Events.RowPersisting<APPayment> args)
        {
            if (args.Row is APPayment payment)
            {
                StopPaymentIfOutstandingLienWaiversExist(args.Cache, payment);
                DeleteAutomaticallyGeneratedLienWaiversIfRequired(args.Cache, payment);
            }
        }

        protected virtual void _(Events.RowSelected<JointPayeePayment> args)
        {
            var jointPayeePayment = args.Row;
            if (jointPayeePayment?.JointAmountToPay > 0 && LienWaiverSetup.Current.ShouldWarnOnPayment == true &&
                Base.Document.Current != null && !IsCheckReleasedOrVoided())
            {
                var jointPayees = Base1.JointPayeePayments.Select<JointPayee>()
                    .Where(jp => jp.JointPayeeId == jointPayeePayment.JointPayeeId).ToList();
                var adjustment = Base.Adjustments.SelectMain().Single(adj =>
                    adj.AdjdDocType == jointPayeePayment.InvoiceDocType &&
                    adj.AdjdRefNbr == jointPayeePayment.InvoiceRefNbr &&
                    adj.AdjdLineNbr == jointPayeePayment.BillLineNumber);
                var projectIds = LienWaiverProjectDataProvider.GetProjectIds(adjustment, Base).ToList();
                jointPayees.ForEach(jp => lienWaiverValidationService.ValidateJointPayee(Base.Caches<JointPayee>(),
                    jp, projectIds, ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver));
            }
        }

        protected virtual void _(Events.FieldUpdating<APRegister.approved> args)
        {
            if (args.Row is APRegister payment && IsPaymentApproved(args.NewValue))
            {
                lienWaiverProcessingValidationService.ValidateLienWaivers(payment,
                    LienWaiverSetup.Current?.ShouldStopPayments);
            }
        }

        protected virtual void _(Events.RowSelected<APAdjust> args)
        {
            var adjustment = args.Row;
            if (adjustment != null && LienWaiverSetup.Current.ShouldWarnOnPayment == true &&
                Base.Document.Current != null && !IsCheckReleasedOrVoided())
            {
                var jointPayeePayments =
                    Base1.JointPayeePayments.SelectMain().Where(jpp => jpp.JointAmountToPay > 0).ToList();
                var jointPayees =
                    JointPayeeDataProvider.GetJointPayees(Base, jointPayeePayments, adjustment.AdjdLineNbr).ToList();
                lienWaiverValidationService.ValidateVendorAndJointPayees(args.Cache, adjustment, jointPayees);
            }
        }

        private bool IsCheckReleasedOrVoided()
        {
            return Base.Document.Current.Status.IsIn(APDocStatus.Closed, APDocStatus.Voided);
        }

        private void StopPaymentIfOutstandingLienWaiversExist(PXCache cache, APRegister document)
        {
            var originalDocument = cache.GetOriginal(document) as APRegister;
            if (originalDocument?.Status != document.Status && document.Hold == false
                && PXContext.GetScreenID() == Constants.ScreenIds.ChecksAndPayments)
            {
                if (originalDocument?.Status == APDocStatus.Hold)
                {
                    lienWaiverProcessingValidationService.ValidateLienWaiversOnManualStatusChange(document,
                        LienWaiverSetup.Current?.ShouldStopPayments);
                }
                else
                {
                    lienWaiverProcessingValidationService.ValidateLienWaivers(document,
                        LienWaiverSetup.Current?.ShouldStopPayments);
                }
            }
        }

        private void DeleteAutomaticallyGeneratedLienWaiversIfRequired(PXCache cache, APRegister document)
        {
            var originalDocument = cache.GetOriginal(document) as APRegister;
            if (originalDocument?.Hold == false && document.Hold == true)
            {
                var paymentEntryExtension = Base.GetExtension<PX.Objects.CN.Compliance.AP.GraphExtensions.ApPaymentEntryExt>();
                var lienWaiverTypeId = Base.Select<ComplianceAttributeType>()
                    .Single(type => type.Type == ComplianceDocumentType.LienWaiver).ComplianceAttributeTypeID;
                var lienWaivers = paymentEntryExtension.ComplianceDocuments.SelectMain()
                    .Where(cd => cd.DocumentType == lienWaiverTypeId && cd.IsCreatedAutomatically == true);
                paymentEntryExtension.ComplianceDocuments.Cache.DeleteAll(lienWaivers);
            }
        }

        private static bool IsPaymentApproved(object isApproved)
        {
            return isApproved is bool approved
                ? approved
                : Convert.ToBoolean((string) isApproved);
        }
    }
}