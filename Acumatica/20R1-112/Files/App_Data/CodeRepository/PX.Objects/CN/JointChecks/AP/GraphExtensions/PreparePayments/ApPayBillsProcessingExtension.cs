using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Abstractions;
using PX.Objects.CS;
using PX.Objects.GL;
using ApMessages = PX.Objects.AP.Messages;
using Messages = PX.Objects.AP.Messages;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PreparePayments
{
    public class ApPayBillsProcessingExtension : PXGraphExtension<ApPayBillsExt, APPayBills>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;
        private LienWaiverHoldPaymentService lienWaiverHoldPaymentService;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            lienWaiverHoldPaymentService = new LienWaiverHoldPaymentService(Base);
        }

        public virtual void PayBillsFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
	        PXRowSelected baseHandler)
        {
	        if (args.Row is PayBillsFilter filter)
	        {
		        ClearPaymentInformationIfNeeded(filter);
		        var currencyInfo = Base.CurrencyInfo_CuryInfoID.SelectSingle(filter.CuryInfoID);
		        var paymentMethod = Base.paymenttype.Current;
		        Base.APDocumentList.SetProcessDelegate(list =>
			        ProcessPayments(list, filter, currencyInfo, paymentMethod));
	        }

	        PXUIFieldAttribute.SetDisplayName<APAdjust.vendorID>(Base.APDocumentList.Cache, Messages.VendorID);
	        PXUIFieldAttribute.SetVisible<APAdjust.vendorID>(Base.APDocumentList.Cache, null, true);
	        DisableProcessingButtonsIfErrorExists(cache);
        }

        private void DisableProcessingButtonsIfErrorExists(PXCache cache)
        {
            var errorsOnForm =
                PXUIFieldAttribute.GetErrors(cache, null, PXErrorLevel.Error, PXErrorLevel.RowError).Any();
            Base.APDocumentList.SetProcessEnabled(!errorsOnForm);
            Base.APDocumentList.SetProcessAllEnabled(!errorsOnForm);
        }

        private void ClearPaymentInformationIfNeeded(PayBillsFilter filter)
        {
            if (Base.cashaccount.Current != null &&
                Equals(Base.cashaccount.Current.CashAccountID, filter.PayAccountID) == false)
            {
                Base.cashaccount.Current = null;
            }
            if (Base.cashaccountdetail.Current != null &&
                (Equals(Base.cashaccountdetail.Current.CashAccountID, filter.PayAccountID) == false ||
                    Equals(Base.cashaccountdetail.Current.PaymentMethodID, filter.PayTypeID) == false))
            {
                Base.cashaccountdetail.Current = null;
            }
            if (Base.paymenttype.Current != null &&
                Equals(Base.paymenttype.Current.PaymentMethodID, filter.PayTypeID) == false)
            {
                Base.paymenttype.Current = null;
            }
        }

        private void ProcessPayments(List<APAdjust> adjustments, PayBillsFilter filter, CurrencyInfo currencyInfo,
            PaymentMethod paymentMethod)
        {
            var jointAdjustments = adjustments.Where(IsJointPayment).ToList();
            var defaultAdjustments = adjustments.Except(jointAdjustments).ToList();
            PreparePaymentsAdjustmentsCache.Add(jointAdjustments);
            PreparePaymentsAdjustmentsCache.Add(defaultAdjustments);
            ProcessDefaultPayments(defaultAdjustments, filter, currencyInfo, paymentMethod);
            ProcessJointPayments(jointAdjustments, filter);
            PreparePaymentsAdjustmentsCache.ClearStoredAdjustments();
            RedirectToResult(paymentMethod, filter, adjustments);
        }

        private bool IsJointPayment(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
            return invoiceExtension.IsJointPayees == true;
        }

        private void ProcessJointPayments(IEnumerable<APAdjust> jointAdjustments, PayBillsFilter filter)
        {
            CheckRunningFlagScope();
            bool allPaymentsWereProcessed;
            using (new RunningFlagScope<APPayBills>())
            {
                var invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                var paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
                var jointAdjustmentGroups = jointAdjustments.GroupBy(adj => adj.AdjdRefNbr);
                allPaymentsWereProcessed = jointAdjustmentGroups.Select(
                        adj => ProcessJointPayment(invoiceEntry, paymentEntry, adj.ToList(), filter))
                    .ToList().All(isValid => isValid);
            }
            if (!allPaymentsWereProcessed)
            {
                throw new Exception(JointCheckMessages.PaymentCycleWorkflowIsStarted);
            }
        }

        private bool ProcessJointPayment(APInvoiceEntry invoiceEntry, APPaymentEntry paymentEntry,
            IReadOnlyCollection<APAdjust> adjustments, PayBillsFilter filter)
        {
            var invoice = InvoiceDataProvider.GetInvoice(
                Base, adjustments.First().AdjdDocType, adjustments.First().AdjdRefNbr);
            if (IsPaymentCycleWorkflow(invoice))
            {
                PXProcessing<APAdjust>.SetError(JointCheckMessages.PaymentCycleWorkflowIsStarted);
                return false;
            }
            paymentEntry.Clear();
            var jointCheckPreparePaymentCreator = new JointCheckPreparePaymentCreator(invoiceEntry, paymentEntry, Base1);
            jointCheckPreparePaymentCreator.GenerateChecks(invoice, adjustments, filter);
            if (Base.APSetup.Current.HoldEntry == false)
            {
                lienWaiverHoldPaymentService.HoldPaymentsIfNeeded(invoice.RefNbr,
                    LienWaiverSetup.Current.ShouldStopPayments);
                Base.Caches<APPayment>().Persist(PXDBOperation.Update);
            }
            return true;
        }

		private bool IsPaymentCycleWorkflow(IDocumentKey invoice)
		{
			var allRelatedBills =
				InvoiceDataProvider.GetAllBillsFromRetainageGroup(Base, invoice.RefNbr, invoice.DocType);
			return allRelatedBills.Any(inv =>
				JointPayeePaymentDataProvider.DoesAnyNonReleasedJointPayeePaymentExist(Base, inv));
		}

		private static void CheckRunningFlagScope()
        {
            if (RunningFlagScope<APPayBills>.IsRunning)
            {
                throw new PXSetPropertyException(ApMessages.AnotherPayBillsRunning, PXErrorLevel.Warning);
            }
        }

        private void ProcessDefaultPayments(List<APAdjust> defaultPayments, PayBillsFilter filter,
            CurrencyInfo currencyInfo, PaymentMethod paymentMethod)
        {
            try
            {
                var graph = PXGraph.CreateInstance<APPayBills>();
                graph.CreatePayments(defaultPayments, filter, currencyInfo, paymentMethod);
            }
            finally
            {
                var payments = GetGeneratedPayments(defaultPayments);
                payments.ForEach(p =>
                    lienWaiverHoldPaymentService.HoldPaymentIfNeeded(p, LienWaiverSetup.Current.ShouldStopPayments));
                Base.Caches<APPayment>().Persist(PXDBOperation.Update);
            }
        }

        private void RedirectToResult(PaymentMethod paymentMethod, PayBillsFilter filter,
            IEnumerable<APAdjust> adjustments)
        {
            var targetGraph = paymentMethod?.PrintOrExport == true
                ? GetPrintChecksGraph(filter, adjustments)
                : GetReleaseChecksGraph(filter, adjustments);
            throw new PXRedirectRequiredException(targetGraph, JointCheckActions.NextProcessing);
        }

        private PXGraph GetReleaseChecksGraph(PayBillsFilter filter, IEnumerable<APAdjust> adjustments)
        {
            var releaseChecksGraph = PXGraph.CreateInstance<APReleaseChecks>();
            var filterCopy = PXCache<ReleaseChecksFilter>.CreateCopy(releaseChecksGraph.Filter.Current);
            filterCopy.PayTypeID = filter.PayTypeID;
            filterCopy.PayAccountID = filter.PayAccountID;
            filterCopy.CuryID = filter.CuryID;
            releaseChecksGraph.Filter.Cache.Update(filterCopy);
            UpdateGraphCache(releaseChecksGraph, releaseChecksGraph.APPaymentList, adjustments);
            return releaseChecksGraph;
        }

        private PXGraph GetPrintChecksGraph(PayBillsFilter filter, IEnumerable<APAdjust> adjustments)
        {
            var printChecksGraph = PXGraph.CreateInstance<APPrintChecks>();
            var filterCopy = PXCache<PrintChecksFilter>.CreateCopy(printChecksGraph.Filter.Current);
            filterCopy.BranchID = filter.BranchID;
            filterCopy.PayTypeID = filter.PayTypeID;
            filterCopy = (PrintChecksFilter) printChecksGraph.Filter.Cache.Update(filterCopy);
            filterCopy.PayAccountID = filter.PayAccountID;
            filterCopy.CuryID = filter.CuryID;
            printChecksGraph.Filter.Cache.Update(filterCopy);
            UpdateGraphCache(printChecksGraph, printChecksGraph.APPaymentList, adjustments);
            return printChecksGraph;
        }

		private void UpdateGraphCache(PXGraph targetGraph,
			PXSelectBase<APPayment> paymentView, IEnumerable<APAdjust> adjustments)
		{
			if (targetGraph != null && paymentView != null)
			{
				var references = adjustments.Select(adjustment =>
					(adjustment.AdjdRefNbr, adjustment.AdjdDocType)).Distinct();
				var payments = references.SelectMany(reference =>
						PaymentDataProvider.GetGeneratedPayments(Base, reference.AdjdRefNbr))
					.Where(p => p != null).Distinct();
				payments.ForEach(p => UpdatePayment(paymentView, p));
				paymentView.Cache.IsDirty = false;
			}
		}

		private void UpdatePayment(PXSelectBase<APPayment> paymentView, IDocumentKey payment)
		{
			var resultPayment = (APPayment)paymentView
				.Search<APPayment.docType, APPayment.refNbr>(payment.DocType, payment.RefNbr);
			if (resultPayment != null)
			{
				resultPayment.Selected = true;
				paymentView.Cache.Update(resultPayment);
			}
		}

		private IEnumerable<APPayment> GetGeneratedPayments(IEnumerable<APAdjust> adjustments)
        {
            var references = adjustments.Select(adjustment => (adjustment.AdjgRefNbr, adjustment.AdjgDocType)).Distinct();
            return references.Select(reference =>
                PaymentDataProvider.GetPayment(Base, reference.AdjgRefNbr, reference.AdjgDocType));
        }
    }
}