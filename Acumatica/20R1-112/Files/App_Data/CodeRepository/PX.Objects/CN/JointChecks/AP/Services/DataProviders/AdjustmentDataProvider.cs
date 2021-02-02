using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.DAC;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
	public class AdjustmentDataProvider
	{
		public static IEnumerable<APAdjust> GetInvoiceAdjustments(PXGraph graph, string referenceNumber,
			int? lineNumber = 0)
		{
			return SelectFrom<APAdjust>
				.Where<APAdjust.adjdRefNbr.IsEqual<P.AsString>
					.And<APAdjust.adjdDocType.IsEqual<APDocType.invoice>>
					.And<APAdjust.adjdLineNbr.IsEqual<P.AsInt>>>.View
				.Select(graph, referenceNumber, lineNumber).FirstTableItems;
		}

		public static IEnumerable<APAdjust> GetPaymentAdjustments(PXGraph graph, string paymentReferenceNumber,
			string paymentDocumentType)
		{
			return SelectFrom<APAdjust>
				.Where<APAdjust.adjgRefNbr.IsEqual<P.AsString>
					.And<APAdjust.adjgDocType.IsEqual<P.AsString>>
					.And<APAdjust.adjdDocType.IsEqual<APDocType.invoice>>>.View
				.Select(graph, paymentReferenceNumber, paymentDocumentType).FirstTableItems;
		}

		public static APAdjust GetAdjustment(PXGraph graph, JointPayeePayment jointPayeePayment)
		{
			return SelectFrom<APAdjust>
				.Where<APAdjust.adjgDocType.IsEqual<P.AsString>
					.And<APAdjust.adjgRefNbr.IsEqual<P.AsString>>
					.And<APAdjust.adjdDocType.IsEqual<P.AsString>>
					.And<APAdjust.adjdRefNbr.IsEqual<P.AsString>>
					.And<APAdjust.adjNbr.IsEqual<P.AsInt>>
					.And<APAdjust.adjdLineNbr.IsEqual<P.AsInt>>
					.And<APAdjust.adjgDocType.IsNotEqual<APDocType.invoice>>>.View
				.Select(graph, jointPayeePayment.PaymentDocType, jointPayeePayment.PaymentRefNbr,
					jointPayeePayment.InvoiceDocType, jointPayeePayment.InvoiceRefNbr,
					jointPayeePayment.AdjustmentNumber, jointPayeePayment.BillLineNumber);
		}

		public static IEnumerable<APAdjust> GetAdjustmentsForInvoiceGroup(PXGraph graph, string referenceNumber,
			int? lineNumber = 0)
		{
			if (lineNumber == 0)
			{
				return SelectFrom<APAdjust>
					.InnerJoin<APRetainageInvoice>.On<APRetainageInvoice.refNbr.IsEqual<APAdjust.adjdRefNbr>
						.And<APRetainageInvoice.docType.IsEqual<APAdjust.adjdDocType>>>
					.Where<Brackets<APRetainageInvoice.refNbr.IsEqual<P.AsString>
								.And<APRetainageInvoice.docType.IsEqual<APDocType.invoice>>>
							.Or<Brackets<APRetainageInvoice.origRefNbr.IsEqual<P.AsString>
								.And<APRetainageInvoice.origDocType.IsEqual<APDocType.invoice>
									.And<APRetainageInvoice.isRetainageDocument.IsEqual<True>>>>>>
					.View
					.Select(graph, referenceNumber, referenceNumber).FirstTableItems;
			}
			else
			{
				return SelectFrom<APAdjust>
					.InnerJoin<APRetainageInvoice>
						.On<APRetainageInvoice.refNbr.IsEqual<APAdjust.adjdRefNbr>
							.And<APRetainageInvoice.docType.IsEqual<APAdjust.adjdDocType>>>
					.InnerJoin<APTran>
						.On<APTran.refNbr.IsEqual<APRetainageInvoice.refNbr>
							.And<APTran.tranType.IsEqual<APRetainageInvoice.docType>
							.And<APTran.lineNbr.IsEqual<APAdjust.adjdLineNbr>>>>
					.Where<Brackets<APRetainageInvoice.refNbr.IsEqual<P.AsString>
								.And<APRetainageInvoice.docType.IsEqual<APDocType.invoice>
								.And<APTran.lineNbr.IsEqual<P.AsInt>>>>
							.Or<Brackets<APRetainageInvoice.origRefNbr.IsEqual<P.AsString>
								.And<APRetainageInvoice.origDocType.IsEqual<APDocType.invoice>
								.And<APTran.origLineNbr.IsEqual<P.AsInt>>
								.And<APRetainageInvoice.isRetainageDocument.IsEqual<True>>>>>>
					.View
					.Select(graph, referenceNumber, lineNumber, referenceNumber, lineNumber).FirstTableItems;
			}
		}

		public static APAdjust GetReversedAdjustment(PXGraph graph, APAdjust originalAdjustment)
		{
			return SelectFrom<APAdjust>
				.Where<APAdjust.adjdRefNbr.IsEqual<P.AsString>
					.And<APAdjust.adjdDocType.IsEqual<P.AsString>>
					.And<APAdjust.adjgRefNbr.IsEqual<P.AsString>>
					.And<APAdjust.adjgDocType.IsEqual<P.AsString>>
					.And<APAdjust.adjdLineNbr.IsEqual<P.AsInt>>
					.And<APAdjust.released.IsEqual<False>>
					.And<APAdjust.voided.IsEqual<True>>
					.And<APAdjust.voidAdjNbr.IsEqual<P.AsInt>>>.View
				.Select(graph, originalAdjustment.AdjdRefNbr, originalAdjustment.AdjdDocType,
					originalAdjustment.AdjgRefNbr, originalAdjustment.AdjgDocType, originalAdjustment.AdjdLineNbr,
					originalAdjustment.AdjNbr);
		}
	}
}