using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.AP.GraphExtensions;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PreparePayments;
using PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices;

namespace PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices
{
	public class JointCheckPreparePaymentCreator : JointChecksCreator
	{
		private readonly ApPayBillsExt apPayBillsExt;

		public JointCheckPreparePaymentCreator(APInvoiceEntry invoiceEntry, APPaymentEntry paymentEntry,
			ApPayBillsExt apPayBillsExt)
			: base(invoiceEntry)
		{
			this.apPayBillsExt = apPayBillsExt;
			CreatePaymentEntry(paymentEntry);
		}

		private IEnumerable<int?> LineNumbers
		{
			get;
			set;
		}

		public void GenerateChecks(APInvoice invoice, IReadOnlyCollection<APAdjust> adjustments, PayBillsFilter filter)
		{
			using (var scope = new PXTransactionScope())
			{
				LineNumbers = adjustments.Select(adj => adj.AdjdLineNbr);
				GenerateVendorCheck(invoice, adjustments);
				GenerateJointChecks(adjustments, filter, invoice);
				ComplianceDocumentsService.ClearLinkToPaymentFlag(ComplianceDocuments);
				OpenPaymentCycleWorkflow();
				InvoiceEntry.Caches<ComplianceDocumentReference>().Persist(PXDBOperation.Delete);
				scope.Complete();
			}
		}

		public override APPayment CreateJointChecks(List<JointPayeePayment> jointPayeePayments)
		{
			JointPayeePayments.Cache.Clear();
			var jointChecks = GenerateJointChecks();
			InvoiceEntry.Views.Caches.Add(typeof(JointPayeePayment));
			InvoiceEntry.Persist();
			MarkPaymentForLienWaiverGeneration(true);
			PaymentEntry.Persist();
			return jointChecks.FirstOrDefault();
		}

		protected override IEnumerable<int?> GetBillLineNumbersForVendorCheck()
		{
			return base.GetBillLineNumbersForVendorCheck().Where(ln => LineNumbers.Contains(ln));
		}

		private void GenerateVendorCheck(APInvoice invoice, IEnumerable<APAdjust> adjustments)
		{
			InvoiceEntry.Clear();
			SetVendorAmountToPay(invoice, adjustments);
			InvoiceJointPayeePayments = GetInsertedInvoiceJointPayeePayments(invoice.DocType, invoice.RefNbr).ToList();
			InvoiceEntry.Document.Current = invoice;
			InitializeComplianceDocuments();
			GenerateVendorCheck();
			InvoiceEntry.Views.Caches.Add(typeof(JointPayeePayment));
			InvoiceEntry.Persist();
		}

		private void GenerateJointChecks(IEnumerable<APAdjust> adjustments,
			PayBillsFilter filter, APInvoice invoice)
		{
			adjustments.ForEach(a => InitializeAdjustments(a, filter));
			InvoiceEntry.Clear();
			InvoiceJointPayeePayments = GetInsertedInvoiceJointPayeePayments(
				invoice.DocType, invoice.RefNbr).ToList();
			InvoiceEntry.Document.Current = invoice;
			InvoiceEntry.GetExtension<ApInvoiceEntryExt>().Initialize();
			CreateJointChecks(InvoiceJointPayeePayments);
		}

		private void SetVendorAmountToPay(APInvoice invoice, IEnumerable<APAdjust> adjustments)
		{
			var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
			invoiceExtension.AmountToPay = adjustments.Sum(adj => adj.CuryAdjgAmt);
		}

		private static void InitializeAdjustments(APAdjust adjustment, PayBillsFilter filter)
		{
			PXProcessing<APAdjust>.SetCurrentItem(adjustment);
			adjustment.AdjgDocDate = filter.PayDate;
			adjustment.AdjgFinPeriodID = filter.PayFinPeriodID;
			PXProcessing<APAdjust>.SetProcessed();
		}

		private void InitializeComplianceDocuments()
		{
			ComplianceDocumentsService = new ComplianceDocumentsService(InvoiceEntry, InvoiceJointPayeePayments);
			ComplianceDocuments = ComplianceDocumentsService.GetComplianceDocumentsToLink();
		}

		private IEnumerable<JointPayeePayment> GetInsertedInvoiceJointPayeePayments(string documentType,
			string referenceNumber)
		{
			return apPayBillsExt.JointPayeePayments.Cache.Inserted.Cast<JointPayeePayment>()
				.Where(jpp => jpp.PaymentDocType == documentType && jpp.PaymentRefNbr == referenceNumber);
		}
	}
}