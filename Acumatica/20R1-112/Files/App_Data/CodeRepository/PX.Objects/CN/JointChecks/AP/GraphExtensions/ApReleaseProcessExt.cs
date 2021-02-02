using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common.Abstractions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions
{
	public class ApReleaseProcessExt : PXGraphExtension<APReleaseProcess>
	{
		public PXSelect<JointPayeePayment> JointPayeePayments;
		public PXSelect<JointPayee> JointPayees;
		public PXSetup<LienWaiverSetup> LienWaiverSetup;

		private JointPayeePaymentService jointPayeePaymentService;
		private LienWaiverProcessingValidationService lienWaiverProcessingValidationService;

		public delegate List<APRegister> ReleaseDocProcDelegate(
			JournalEntry journalEntry, APRegister document, bool isPreBooking, out List<INRegister> documents);

		public override void Initialize()
		{
			jointPayeePaymentService = new JointPayeePaymentService(Base);
			lienWaiverProcessingValidationService = new LienWaiverProcessingValidationService(Base);
		}

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>();
		}

		[PXOverride]
		public virtual void Persist(Action baseHandler)
		{
			var adjustments = GetReleasedAdjustments();
			baseHandler();
			adjustments.ForEach(jointPayeePaymentService.ClosePaymentCycleWorkflowIfNeeded);

			// base graph overrides persist() logic and saves only specific caches.
			// you need manually save all custom changes in database for custom caches.
			JointPayees.Cache.GetAttributes<JointPayee.jointAmountOwed>()
				.OfType<PXUIVerifyAttribute>()
				.ForEach(attr => attr.CheckOnRowPersisting = false);
			JointPayees.Cache.Persist(PXDBOperation.Update);
			JointPayees.Cache.Persisted(false);
		}

		[PXOverride]
		public virtual List<APRegister> ReleaseDocProc(JournalEntry journalEntry, APRegister document,
			bool isPreBooking, out List<INRegister> documents, ReleaseDocProcDelegate baseHandler)
		{
			ValidateEmptyInvoiceJointPayees(document);
			return IsCheckOrVoidCheck(document)
				? ReleaseCheckOrVoidCheck(journalEntry, document, isPreBooking, out documents, baseHandler)
				: baseHandler(journalEntry, document, isPreBooking, out documents);
		}

		private void ValidateEmptyInvoiceJointPayees(IDocumentKey document)
		{
			if (document.DocType == APDocType.Invoice && IsJointPayeesInvoice(document) &&
				!DoesAnyInvoiceJointPayeeExist(document))
			{
				throw new PXException(JointCheckMessages.JointPayeesRequired);
			}
		}

		private List<APRegister> ReleaseCheckOrVoidCheck(JournalEntry journalEntry, APRegister document,
			bool isPreBooking, out List<INRegister> documents, ReleaseDocProcDelegate baseHandler)
		{
			lienWaiverProcessingValidationService.ValidateLienWaivers(document,
				LienWaiverSetup.Current?.ShouldStopPayments);
			using (var scope = new PXTransactionScope())
			{
				ReleaseJointPayeePayments(document);
				var result = baseHandler(journalEntry, document, isPreBooking, out documents);
				scope.Complete();
				Base.Persist();
				return result;
			}
		}

		private static bool IsCheckOrVoidCheck(IDocumentKey document)
		{
			return document.DocType.IsIn(APDocType.Check, APDocType.VoidCheck);
		}

		private List<APAdjust> GetReleasedAdjustments()
		{
			return Base.Caches<APAdjust>().Cached.Cast<APAdjust>().Where(adj => adj.Released == true && !adj.IsSelfAdjustment()).ToList();
		}

		private void ReleaseJointPayeePayments(APRegister document)
		{
			var jointPayeePaymentsAndJointPayees = JointPayeePaymentDataProvider
				.GetNonReleasedJointPayeePaymentsAndJointPayees(Base, document).ToList();
			foreach (var jointPayeePaymentAndJointPayee in jointPayeePaymentsAndJointPayees)
			{
				var jointPayee = jointPayeePaymentAndJointPayee.GetItem<JointPayee>();
				var jointPayeePayments = jointPayeePaymentsAndJointPayees.RowCast<JointPayeePayment>()
					.Where(jpp => jpp.JointPayeeId == jointPayee.JointPayeeId);
				RecalculateJointPayee(jointPayeePayments, jointPayee);
			}
		}

		private void RecalculateJointPayee(IEnumerable<JointPayeePayment> jointPayeePayments,
			JointPayee jointPayee)
		{
			var jointAmountToPay = jointPayeePayments.Sum(jpp => jpp.JointAmountToPay);
			jointPayee.JointBalance -= jointAmountToPay;
			jointPayee.JointAmountPaid += jointAmountToPay;
			Base.Caches<JointPayee>().Update(jointPayee);
		}

		private bool DoesAnyInvoiceJointPayeeExist(IDocumentKey document)
		{
			var noteId = InvoiceDataProvider.GetOriginalInvoice(Base, document.RefNbr, document.DocType)?.NoteID;
			return SelectFrom<JointPayee>
				.Where<JointPayee.billId.IsEqual<P.AsGuid>>.View
				.Select(Base, noteId).Any();
		}

		private bool IsJointPayeesInvoice(IDocumentKey document)
		{
			var invoice = InvoiceDataProvider.GetInvoice(Base, document.DocType, document.RefNbr);
			var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
			return invoiceExtension?.IsJointPayees == true;
		}
	}
}