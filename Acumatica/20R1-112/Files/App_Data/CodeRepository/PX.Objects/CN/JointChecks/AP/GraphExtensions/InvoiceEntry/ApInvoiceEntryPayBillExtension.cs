using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.CS;
using ComplianceDocumentsService = PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices.ComplianceDocumentsService;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
    public class ApInvoiceEntryPayBillExtension : PXGraphExtension<ApInvoiceEntryExt, APInvoiceEntry>
    {
        [PXCopyPasteHiddenView]
        public PXSelect<JointPayeePayment> JointPayeePayments;

        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private ComplianceDocumentsService complianceDocumentsService;
        private PayBillAmountsToPayValidator payBillAmountsToPayValidator;
        private VendorBalanceCalculationService vendorBalanceCalculationService;
        private JointChecksCreator jointChecksCreator;
        private LienWaiverWarningDialogService lienWaiverWarningDialogService;

        private APInvoiceJCExt InvoiceExtension =>
            PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(Base.Document.Current);

        public override void Initialize()
        {
            var jointPayeePayments = JointPayeePayments.Select().FirstTableItems;
            lienWaiverWarningDialogService = new LienWaiverWarningDialogService(Base, this);
            complianceDocumentsService = new ComplianceDocumentsService(Base, jointPayeePayments);
            jointChecksCreator = new JointChecksCreator(Base, JointPayeePayments);
            payBillAmountsToPayValidator = new PayBillAmountsToPayValidator(Base, JointPayeePayments, Base1.JointPayees);
            vendorBalanceCalculationService = Base.Document.Current?.PaymentsByLinesAllowed == true
                ? new VendorBalancePerLineCalculationService(Base, Base1.JointPayees.SelectMain())
                : new VendorBalanceCalculationService(Base, Base1.JointPayees.SelectMain());
            Base.Views.Caches.Remove(typeof(JointPayeePayment));
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        public IEnumerable jointPayeePayments()
        {
            return SelectFrom<JointPayeePayment>
                .LeftJoin<JointPayee>.On<JointPayee.jointPayeeId.IsEqual<JointPayeePayment.jointPayeeId>>
                .Where<JointPayee.billId.IsEqual<P.AsGuid>
                    .And<JointPayeePayment.paymentDocType.IsEqual<APInvoice.docType.FromCurrent>>
                    .And<JointPayeePayment.paymentRefNbr.IsEqual<APInvoice.refNbr.FromCurrent>>>.View
                .Select(Base, Base1.OriginalBillNoteId);
        }

        [PXOverride]
        public IEnumerable PayInvoice(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            InvoiceExtension.IsPaymentCycleWorkflow = JointPayeePaymentDataProvider
                .DoesAnyNonReleasedJointPayeePaymentExist(Base, Base.Document.Current);
            if (!IsPayInvoiceAvailable())
            {
                return adapter.Get();
            }
            lienWaiverWarningDialogService.ShowWarningIfNeeded();
            if (InvoiceExtension.IsJointPayees == false)
            {
                return PayBillWithoutJointPayees(adapter, baseMethod);
            }
            PayBillWithJointPayees();
            return adapter.Get();
        }

        public virtual void _(Events.RowSelected<APInvoice> args)
        {
            if (args.Row != null)
            {
                PXUIFieldAttribute.SetEnabled<APInvoiceJCExt.amountToPay>(Base.Document.Cache, null, true);
            }
        }

        public virtual void _(Events.FieldSelecting<APInvoice, APInvoiceJCExt.vendorBalance> args)
        {
            var invoice = args.Row;
            if (invoice?.DocType == null || InvoiceExtension.IsJointPayees != true)
            {
                return;
            }
            InvoiceExtension.VendorBalance = vendorBalanceCalculationService.GetVendorBalancePerBill(invoice);
            args.ReturnValue = InvoiceExtension.VendorBalance;
            var jointCheckVendorBalanceService = new JointCheckVendorBalanceService();
            jointCheckVendorBalanceService.UpdateVendorBalanceDisplayName(invoice, Base.Document.Cache);
        }

        public virtual void _(Events.RowSelected<JointPayeePayment> args)
        {
            PXUIFieldAttribute.SetEnabled<JointPayeePayment.jointAmountToPay>(args.Cache, null, true);
        }

        private void PayBillWithJointPayees()
        {
            if (!IsWorkflowValidatedAndChecksGenerationConfirmed() || AreAmountsToPayEqualToZero())
            {
                return;
            }
            payBillAmountsToPayValidator.ValidateAllAmountsToPay();
            GenerateChecks();
        }

        private bool IsWorkflowValidatedAndChecksGenerationConfirmed()
        {
            return Base1.JointPayees.AskExt((graph, viewName) => ValidateWorkflowAndPrepareDialogWindow()).IsPositive();
        }

        private IEnumerable PayBillWithoutJointPayees(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseMethod)
        {
            try
            {
                return baseMethod(adapter);
            }
            catch (PXRedirectRequiredException exception)
            {
                if (exception.Graph is APPaymentEntry graph)
                {
					using (PXTransactionScope scope = new PXTransactionScope())
					{
						LinkComplianceDocumentsToCheck(graph);
						var currentApPayment = graph.Document.Current;
						TakePaymentsOffHoldIfNeeded(graph);
						graph.Document.Current = currentApPayment;

						Base.Caches<ComplianceDocumentReference>().Persist(PXDBOperation.Delete);

						scope.Complete();
					}
				}
                throw;
            }
        }

        private void ValidateWorkflowAndPrepareDialogWindow()
        {
            var graph = PXGraph.CreateInstance<APInvoiceEntry>();
            RefreshCurrentInvoice(graph);
            CheckPaymentLifeCycle(graph);
            CreateJointPayeePayments();
            ResetAmountsToPay();
        }

        private void GenerateChecks()
        {
            var jointPayeePayments = JointPayeePayments.Select().FirstTableItems.ToList();
            var payment = jointChecksCreator.CreateJointChecks(jointPayeePayments);
            RedirectToFirstCheck(payment);
        }

        private void RedirectToFirstCheck(APPayment payment)
        {
            var paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
            TakePaymentsOffHoldIfNeeded(paymentEntry);
            paymentEntry.Document.Current = payment;
            paymentEntry.Actions.PressSave();
            throw new PXRedirectRequiredException(paymentEntry, JointCheckActions.PayInvoice);
        }

        private void TakePaymentsOffHoldIfNeeded(PXGraph graph)
        {
            if (Base.APSetup.Current.HoldEntry == false)
            {
                var lienWaiverHoldPaymentService = new LienWaiverHoldPaymentService(graph);
                lienWaiverHoldPaymentService.HoldPaymentsIfNeeded(Base.Document.Current.RefNbr,
                    LienWaiverSetup.Current.ShouldStopPayments);
            }
        }

        private bool AreAmountsToPayEqualToZero()
        {
            var jointAmountToPay = JointPayeePayments.Select().FirstTableItems.Sum(jpp => jpp.JointAmountToPay);
            return InvoiceExtension.AmountToPay == 0 && jointAmountToPay == 0;
        }

        private void ResetAmountsToPay()
        {
            var jointPayeePayments = JointPayeePayments.Select().FirstTableItems;
            InvoiceExtension.AmountToPay = 0;
            foreach (var jointPayeePayment in jointPayeePayments)
            {
                jointPayeePayment.JointAmountToPay = 0;
                JointPayeePayments.Update(jointPayeePayment);
            }
        }

        private void CreateJointPayeePayments()
        {
            JointPayeePayments.Cache.Clear();
            foreach (var jointPayee in GetJointPayeesToCreateJointPayeePayments())
            {
                InsertTemporaryJointPayeePaymentForInvoice(jointPayee);
            }
        }

        private IEnumerable<JointPayee> GetJointPayeesToCreateJointPayeePayments()
        {
            var jointPayeePayments = JointPayeePayments.Select().FirstTableItems;
            return Base1.JointPayees.Select().FirstTableItems
                .Where(jointPayee => jointPayeePayments.All(jpp => !IsJointPayeePaymentForInvoice(jpp, jointPayee)));
        }

        private bool IsJointPayeePaymentForInvoice(JointPayeePayment jointPayeePayment, JointPayee jointPayee)
        {
            return jointPayeePayment.JointPayeeId == jointPayee.JointPayeeId
                && jointPayeePayment.PaymentDocType == Base.Document.Current.DocType
                && jointPayeePayment.PaymentRefNbr == Base.Document.Current.RefNbr;
        }

        private void InsertTemporaryJointPayeePaymentForInvoice(JointPayee jointPayee)
        {
            var jointPayment = CreateJointPayeePayment(jointPayee);
            JointPayeePayments.Insert(jointPayment);
        }

        private JointPayeePayment CreateJointPayeePayment(JointPayee jointPayee)
        {
            return new JointPayeePayment
            {
                JointPayeeId = jointPayee.JointPayeeId,
                PaymentDocType = Base.Document.Current.DocType,
                PaymentRefNbr = Base.Document.Current.RefNbr,
                JointAmountToPay = 0,
                AdjustmentNumber = 0
            };
        }

        private void CheckPaymentLifeCycle(PXGraph graph)
        {
            var currentBill = Base.Document.Current;
            var allRelatedBills =
                InvoiceDataProvider.GetAllBillsFromRetainageGroup(graph, currentBill.RefNbr, currentBill.DocType);
            if (allRelatedBills.Select(retainageBill => retainageBill.GetExtension<APInvoiceJCExt>())
                .Any(ext => ext.IsPaymentCycleWorkflow == true))
            {
                throw new PXException(JointCheckMessages.PaymentCycleWorkflowIsStarted);
            }
        }

        private void RefreshCurrentInvoice(PXGraph graph)
        {
            var currentBill = Base.Document.Current;
            currentBill = InvoiceDataProvider.GetOriginalInvoice(graph, currentBill.RefNbr, currentBill.DocType);
            if (currentBill.PaymentsByLinesAllowed.GetValueOrDefault())
            {
                RecalculateTotalJointAmountOwedPerLine();
            }
        }

        private void RecalculateTotalJointAmountOwedPerLine()
        {
            var jointAmountOwedValidationService =
                new JointAmountOwedPerLineValidationService(Base1.JointPayees, Base);
            jointAmountOwedValidationService.RecalculateTotalJointAmount();
        }

        private void LinkComplianceDocumentsToCheck(APPaymentEntry paymentEntry)
        {
			paymentEntry.Persist();
			Base.Persist();

			Base.Caches<ComplianceDocument>().ClearQueryCache();
			Base.Caches<ComplianceDocument>().Clear();

			PXDBTimestampAttribute timestampAttribute = Base.Caches<ComplianceDocument>()
				.GetAttributesOfType<PXDBTimestampAttribute>(null, nameof(ComplianceDocument.Tstamp))
				.First();

			timestampAttribute.RecordComesFirst = true;

			var complianceDocuments = complianceDocumentsService.GetComplianceDocumentsToLink();

			complianceDocumentsService.UpdateComplianceDocumentsForVendorCheck(complianceDocuments,
				paymentEntry.Document.Current);
			complianceDocumentsService.ClearLinkToPaymentFlag(complianceDocuments);
		}

        private bool IsPayInvoiceAvailable()
        {
            return Base.Document.Current != null &&
                (Base.Document.Current.Released == true || Base.Document.Current.Prebooked == true);
        }
    }
}