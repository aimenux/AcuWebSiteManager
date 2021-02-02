using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.Creators;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using Constants = PX.Objects.CN.Compliance.CL.Descriptor.Constants;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryLienWaiverGenerationExtension : PXGraphExtension<ApPaymentEntryLienWaiverExtension,
        ApPaymentEntryExt, APPaymentEntry>
    {
        private readonly LienWaiverGenerationContext context = new LienWaiverGenerationContext();

        [InjectDependency]
        public ILienWaiverCreator LienWaiverCreator
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverDataProvider LienWaiverDataProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverTypeDeterminator LienWaiverTypeDeterminator
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverConfirmationService LienWaiverConfirmationService
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverGenerationKeyCreator LienWaiverGenerationKeyCreator
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverTransactionsProvider LienWaiverTransactionsProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public ILienWaiverJointPayeesProvider LienWaiverJointPayeesProvider
        {
            get;
            set;
        }

        [InjectDependency]
        public ICacheService CacheService
        {
            get;
            set;
        }

        private LienWaiverSetup LienWaiverSetup => Base2.LienWaiverSetup.Current;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

		public virtual void _(Events.RowPersisted<APPayment> args)
		{
			if (args.TranStatus == PXTranStatus.Open
			    && args.Row is APPayment payment
			    && IsPaymentTypeCheckOrPrepayment(payment.DocType)
			    && IsPaymentStatusPendingPrintingOrBalanced(payment.Status)
			    && HasPaymentStatusChangedSinceLastSave(args.Cache, payment))
			{
				context.LienWaiversForPayment = LienWaiverDataProvider.GetLienWaivers(payment.NoteID);
				if (!context.PaymentHasGeneratedLienWaivers && ShouldLienWaiverBeGeneratedOnPayingBill())
				{
					GenerateLienWaiver(payment);
				}
			}
		}

		private static bool IsPaymentTypeCheckOrPrepayment(string paymentType)
        {
            return paymentType.IsIn(APDocType.Check, APDocType.Prepayment);
        }

        private static bool IsPaymentStatusPendingPrintingOrBalanced(string paymentStatus)
        {
            return paymentStatus.IsIn(APDocStatus.PendingPrint, APDocStatus.Balanced);
        }

        /// <summary>
        /// Payment status change verification should be ignored in case of creating joint checks. The payment will be
        /// persisted second time to save <see cref="JointPayeePayment"/> records.
        /// </summary>
        private bool HasPaymentStatusChangedSinceLastSave(PXCache cache, APPayment payment)
        {
            var paymentExtension = PXCache<APPayment>.GetExtension<ApPaymentExt>(payment);
            if (paymentExtension.ShouldCreateLienWaivers == false)
            {
                return false;
            }
            var originalStatus = CacheService.GetValueOriginal<APPayment.status>(cache, payment)?.ToString();
            return payment.Status != originalStatus && originalStatus != APDocStatus.PendingPrint ||
                paymentExtension.ShouldCreateLienWaivers == true;
        }

        private bool ShouldLienWaiverBeGeneratedOnPayingBill()
        {
            return LienWaiverSetup.ShouldGenerateConditional == true &&
                LienWaiverSetup.GenerationEventConditional == LienWaiverGenerationEvent.PayingBill ||
                LienWaiverSetup.ShouldGenerateUnconditional == true &&
                LienWaiverSetup.GenerationEventUnconditional == LienWaiverGenerationEvent.PayingBill;
        }

        private void GenerateLienWaiver(APPayment payment)
        {
            if (DoManuallyCreatedLienWaiversExistAndNotIgnored())
            {
                return;
            }
            var transactions = LienWaiverTransactionsProvider.GetTransactions(payment).ToList();
            if (transactions.IsEmpty())
            {
                return;
            }
            var jointPayees = LienWaiverJointPayeesProvider.GetValidJointPayees().ToList();
            var lienWaiverGroupingKeys = LienWaiverGenerationKeyCreator.CreateGenerationKeys(
                transactions, jointPayees, payment);
            lienWaiverGroupingKeys.ForEach(groupingKey => CreateLienWaivers(groupingKey, payment));

            Base.Caches<ComplianceDocumentReference>().Persist(PXDBOperation.Delete);
		}

        private bool DoManuallyCreatedLienWaiversExistAndNotIgnored()
        {
            return context.PaymentHasManuallyCreatedLienWaivers &&
                !LienWaiverConfirmationService.IsCreationOfAdditionalLienWaiversConfirmed(Base.Document);
        }

        private void CreateLienWaivers(LienWaiverGenerationKey generationKey, APPayment payment)
        {
            if (LienWaiverSetup.ShouldGenerateConditional == true)
            {
                CreateConditionalLienWaiver(generationKey, payment);
            }
            if (LienWaiverSetup.ShouldGenerateUnconditional == true)
            {
                CreateUnconditionalLienWaiver(generationKey, payment);
            }
        }

        private void CreateConditionalLienWaiver(LienWaiverGenerationKey generationKey, APPayment payment)
        {
            var lienWaiverDocumentTypeValue = LienWaiverTypeDeterminator.IsLienWaiverFinal(generationKey, true)
                ? Constants.LienWaiverDocumentTypeValues.ConditionalFinal
                : Constants.LienWaiverDocumentTypeValues.ConditionalPartial;
            CreateLienWaiver(generationKey, payment, lienWaiverDocumentTypeValue);
        }

        private void CreateUnconditionalLienWaiver(LienWaiverGenerationKey generationKey, APPayment payment)
        {
            var lienWaiverDocumentTypeValue = LienWaiverTypeDeterminator.IsLienWaiverFinal(generationKey, false)
                ? Constants.LienWaiverDocumentTypeValues.UnconditionalFinal
                : Constants.LienWaiverDocumentTypeValues.UnconditionalPartial;
            CreateLienWaiver(generationKey, payment, lienWaiverDocumentTypeValue);
        }

        private void CreateLienWaiver(LienWaiverGenerationKey generationKey, APPayment payment,
            string lienWaiverDocumentTypeValue)
        {
            var complianceAttribute = LienWaiverDataProvider.GetComplianceAttribute(lienWaiverDocumentTypeValue);
            LienWaiverCreator.CreateLienWaiver(generationKey, payment, complianceAttribute);
        }
    }
}