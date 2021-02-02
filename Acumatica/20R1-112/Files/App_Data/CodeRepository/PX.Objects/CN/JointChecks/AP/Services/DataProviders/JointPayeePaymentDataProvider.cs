using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.Comparers;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
	public class JointPayeePaymentDataProvider
	{
		public static bool DoesAnyNonReleasedJointPayeePaymentExist(PXGraph graph, APInvoice invoice)
		{
			var query = CreateGetNonReleasedJointPayeePaymentsQuery(graph);
			query.WhereAnd<Where<JointPayeePayment.invoiceRefNbr, Equal<Required<APInvoice.refNbr>>,
				And<JointPayeePayment.invoiceDocType, Equal<Required<APInvoice.docType>>>>>();
			var nonReleasedPayments = query.Select(invoice.RefNbr, invoice.DocType).FirstTableItems.ToList();
			return RefreshJointPayeePayments(graph, nonReleasedPayments).Any();
		}

		public static IEnumerable<JointPayeePayment> GetNonReleasedJointPayeePayments(PXGraph graph,
			JointPayee jointPayee)
		{
			return GetNonReleasedJointPayeePayments(graph, jointPayee.SingleToListOrNull());
		}

		public static IEnumerable<JointPayeePayment> GetNonReleasedJointPayeePayments(PXGraph graph,
			List<JointPayee> jointPayees)
		{
			return GetNonReleasedJointPayeePayments(graph, jointPayees, false);
		}

		public static IEnumerable<JointPayeePayment> GetNonReleasedJointPayeePayments(PXGraph graph,
			List<JointPayee> jointPayees, bool skipInserted)
		{
			if (jointPayees == null || jointPayees.Count == 0)
				return new JointPayeePayment[] { };

			var query = CreateGetNonReleasedJointPayeePaymentsQuery(graph);
			query.WhereAnd<Where<JointPayeePayment.jointPayeeId.IsIn<P.AsInt>
							.And<JointPayeePayment.paymentDocType.IsNotEqual<APDocType.invoice>
							.And<JointPayee.billLineNumber.IsEqual<P.AsInt>>>>>();//TODO PANK Why?

			int?[] jointPayeeIds = jointPayees.Select(jp => jp.JointPayeeId).ToArray();
			int? billLineNumber = jointPayees.Select(jp => jp.BillLineNumber).Distinct().Single();

			var nonReleasedPayments = query.Select(jointPayeeIds, billLineNumber)
				.FirstTableItems.ToList();
			var updatedNonReleasedPayments = RefreshJointPayeePayments(graph, nonReleasedPayments).ToList();

			if (!skipInserted)
			{
				var insertedPayments = (IEnumerable<JointPayeePayment>)query.Cache.Inserted;
				updatedNonReleasedPayments.AddRange(insertedPayments.Where(x => jointPayeeIds.Contains(x.JointPayeeId)));
			}
			
			return updatedNonReleasedPayments.Distinct();
		}

		public static IEnumerable<List<JointPayeePayment>> GetCurrentJointPayeePaymentsByVendorGroups(PXGraph graph,
			APPayment payment)
		{
			return GetNonReleasedJointPayeePaymentsAndJointPayees(graph, payment)
				.GroupBy(result => result.GetItem<JointPayee>(),
					result => result.GetItem<JointPayeePayment>(), new JointPayeeComparer())
				.Select(group => RefreshJointPayeePayments(graph, group).ToList());
		}

		public static IEnumerable<PXResult<JointPayeePayment>> GetNonReleasedJointPayeePaymentsAndJointPayees(
			PXGraph graph, APRegister document)
		{
			return GetJointPayeePaymentsAndJointPayees(graph, document)
				.Where(x => !IsJointPayeePaymentReleased(graph, x.GetItem<JointPayeePayment>()));
		}

		public static IEnumerable<JointPayeePayment> GetJointPayeePayments(PXGraph graph, APAdjust adjustment)
		{
			return GetJointPayeePaymentsAndJointPayees(graph, adjustment).FirstTableItems;
		}

		public static IEnumerable<JointPayeePayment> GetJointPayeePayments(PXGraph graph, APRegister document)
		{
			return GetJointPayeePaymentsAndJointPayees(graph, document).FirstTableItems;
		}

		public static JointPayeePayment GetJointPayeePayment(PXGraph graph, int? jointPayeeId)
		{
			return SelectFrom<JointPayeePayment>
				.Where<JointPayeePayment.jointPayeeId.IsEqual<P.AsInt>>.View.Select(graph, jointPayeeId);
		}

		public static PXResultset<JointPayeePayment> GetJointPayeePaymentsAndJointPayees(PXGraph graph, APRegister document)
		{
			var query = new PXSelectJoin<JointPayeePayment,
				InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>,
				Where<JointPayeePayment.paymentDocType, Equal<Required<APPayment.docType>>,
					And<JointPayeePayment.paymentRefNbr, Equal<Required<APPayment.refNbr>>>>>(graph);
			return query.Select(document.DocType, document.RefNbr);
		}

		public static PXResultset<JointPayeePayment> GetJointPayeePaymentsAndJointPayees(PXGraph graph, APAdjust adjustment)
		{
			var query = new PXSelectJoin<JointPayeePayment,
				InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>,
				Where<JointPayeePayment.paymentDocType, Equal<Required<APAdjust.adjgDocType>>,
					And<JointPayeePayment.paymentRefNbr, Equal<Required<APAdjust.adjgRefNbr>>,
					And<JointPayeePayment.invoiceDocType, Equal<Required<APAdjust.adjdDocType>>,
					And<JointPayeePayment.invoiceRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
					And<JointPayeePayment.adjustmentNumber, Equal<Required<APAdjust.adjNbr>>,
					And<JointPayee.billLineNumber, Equal<Required<APAdjust.adjdLineNbr>>>>>>>>>(graph);
			return query.Select(adjustment.AdjgDocType, adjustment.AdjgRefNbr, adjustment.AdjdDocType,
				adjustment.AdjdRefNbr, adjustment.AdjNbr, adjustment.AdjdLineNbr);
		}

		public static IEnumerable<JointPayeePayment> GetJointPayeePayments(PXGraph graph, string referenceNumber,
			string documentType, int? lineNumber)
		{
			return SelectFrom<JointPayeePayment>
				.Where<JointPayeePayment.paymentRefNbr.IsEqual<P.AsString>
					.And<JointPayeePayment.paymentDocType.IsEqual<P.AsString>>
					.And<JointPayeePayment.billLineNumber.IsEqual<P.AsInt>>>.View
				.Select(graph, referenceNumber, documentType, lineNumber).FirstTableItems;
		}

		public static IEnumerable<JointPayeePayment> RefreshJointPayeePayments(PXGraph graph,
			IEnumerable<JointPayeePayment> nonReleasedPayments)
		{
			var paymentIds = nonReleasedPayments.Select(p => p.JointPayeePaymentId).ToArray();
			return paymentIds.Any()
				? new PXSelect<JointPayeePayment,
					Where<JointPayeePayment.jointPayeePaymentId,
						In<Required<JointPayeePayment.jointPayeePaymentId>>>>(graph).Select(paymentIds).FirstTableItems
				: Enumerable.Empty<JointPayeePayment>();
		}

		public static PXResultset<JointPayeePayment> GetCurrentJointPayeePayments(PXGraph graph, APInvoice originalInvoice,
			APInvoice currentInvoice)
		{
			var query = new PXSelectJoin<JointPayeePayment,
				LeftJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>,
				Where<JointPayee.billId, Equal<Required<JointPayee.billId>>,
					And<JointPayeePayment.paymentDocType, Equal<Required<APInvoice.docType>>,
					And<JointPayeePayment.paymentRefNbr, Equal<Required<APInvoice.refNbr>>>>>>(graph);
			return query.Select(originalInvoice.NoteID, currentInvoice.DocType, currentInvoice.RefNbr);
		}

		private static bool IsJointPayeePaymentReleased(PXGraph graph, JointPayeePayment jointPayeePayment)
		{
			return AdjustmentDataProvider.GetAdjustment(graph, jointPayeePayment)?.Released == true;
		}

		private static PXSelectBase<JointPayeePayment> CreateGetNonReleasedJointPayeePaymentsQuery(PXGraph graph)
		{
			return new PXSelectJoinGroupBy<JointPayeePayment,
				InnerJoin<APAdjust, On<APAdjust.adjgRefNbr, Equal<JointPayeePayment.paymentRefNbr>,
					And<APAdjust.adjgDocType, Equal<JointPayeePayment.paymentDocType>,
					And<APAdjust.adjdRefNbr, Equal<JointPayeePayment.invoiceRefNbr>,
					And<APAdjust.adjdDocType, Equal<JointPayeePayment.invoiceDocType>,
					And<APAdjust.adjNbr, Equal<JointPayeePayment.adjustmentNumber>>>>>>,
				InnerJoin<APInvoice, On<APAdjust.adjdRefNbr, Equal<APInvoice.refNbr>,
					And<APAdjust.adjdDocType, Equal<APInvoice.docType>>>,
				InnerJoin<JointPayee, On<JointPayee.jointPayeeId, Equal<JointPayeePayment.jointPayeeId>>>>>,
				Where<APAdjust.released, Equal<False>>,
				Aggregate<GroupBy<JointPayeePayment.jointPayeePaymentId>>>(graph);
		}
	}
}