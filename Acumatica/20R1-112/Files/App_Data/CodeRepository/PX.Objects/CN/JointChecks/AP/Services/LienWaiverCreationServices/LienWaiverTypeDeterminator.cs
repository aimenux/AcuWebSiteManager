using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.LienWaiver;
using PX.Objects.CN.Compliance.CL.Services;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverCreationServices
{
	public class LienWaiverTypeDeterminator : ILienWaiverTypeDeterminator
	{
		private readonly APPaymentEntry graph;
		private readonly LienWaiverDataProvider lienWaiverDataProvider;
		private readonly LienWaiverTransactionRetriever lienWaiverTransactionRetriever;

		public LienWaiverTypeDeterminator(PXGraph graph)
		{
			lienWaiverDataProvider = new LienWaiverDataProvider(graph);
			lienWaiverTransactionRetriever = new LienWaiverTransactionRetriever(graph);
			this.graph = (APPaymentEntry)graph;
		}

		private LienWaiverSetup LienWaiverSetup =>
			graph.GetExtension<ApPaymentEntryLienWaiverExtension>().LienWaiverSetup.Current;

		public bool IsLienWaiverFinal(LienWaiverGenerationKey generationKey, bool isConditional)
		{
			graph.Caches<ComplianceDocument>().ClearQueryCache();
			var lienWaivers = lienWaiverDataProvider.GetNotVoidedLienWaivers(generationKey).ToList();
			var lienNoticeAmounts = lienWaivers.Select(lw => lw.LienNoticeAmount)
				.Where(lna => lna != null).Distinct().ToList();

			if (lienNoticeAmounts.IsSingleElement())
			{
				try
				{
					var totalAmount = GetTotalAmount(lienWaivers);
					var lienWaiverAmount = GetLienWaiverAmount(generationKey, isConditional);

					return totalAmount + lienWaiverAmount == lienNoticeAmounts.First();
				}
				catch (Exception e)
				{
					var waiverIDs = lienWaivers?.Where(w => w.LienNoticeAmount != null)
						.Select(w => w.ComplianceDocumentID).ToArray();

					throw new PXException(ComplianceMessages.LienWaiver.LienWaiverGenerationFailed, waiverIDs?.JoinIntoStringForMessageNoQuotes());
				}

			}

			return false;
		}

		private decimal? GetLienWaiverAmount(LienWaiverGenerationKey generationKey, bool isConditional)
		{
			var transactions = lienWaiverTransactionRetriever.GetTransactions(generationKey);
			var insertedAdjustments = graph.Caches<APAdjust>().Inserted.RowCast<APAdjust>().ToList();
			return isConditional && LienWaiverSetup.FinalAmountSourceConditional == LienWaiverAmountSource.BillAmount
				? LienWaiverAmountCalculationService.GetBillAmount(transactions)
				: LienWaiverAmountCalculationService.GetAmountPaid(graph.Adjustments.SelectMain()
					.Concat(insertedAdjustments));
		}

		private decimal? GetTotalAmount(IReadOnlyCollection<ComplianceDocument> lienWaivers)
		{
			var lienWaiversRelatedToCheck = lienWaivers.Where(lw => lw.ApCheckID != null).ToList();
			return lienWaiversRelatedToCheck.IsEmpty()
				? lienWaivers.Sum(lw => lw.LienWaiverAmount)
				: GetTotalAmount(lienWaivers, lienWaiversRelatedToCheck);
		}

		private decimal? GetTotalAmount(IEnumerable<ComplianceDocument> lienWaivers,
			IReadOnlyCollection<ComplianceDocument> lienWaiversRelatedToCheck)
		{
			var lienWaiverNotRelatedToCheck = lienWaivers.Except(lienWaiversRelatedToCheck);
			return GetTotalAmountForRelatedToCheckLienWaivers(lienWaiversRelatedToCheck) +
				lienWaiverNotRelatedToCheck.Sum(lw => lw.LienWaiverAmount);
		}

		private decimal? GetTotalAmountForRelatedToCheckLienWaivers(
			IEnumerable<ComplianceDocument> lienWaiversRelatedToCheck)
		{
			var lienWaiversCheckReferences = lienWaiversRelatedToCheck.ToDictionary(lw => lw,
				lw => ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(graph, lw.ApCheckID)
					.RefNoteId);
			var referenceGroups = lienWaiversCheckReferences.GroupBy(checkRef => checkRef.Value);
			return referenceGroups.Sum(rg => rg.Max(group => group.Key.LienWaiverAmount));
		}
	}
}