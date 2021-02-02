using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
	public class VendorPreparedBalanceCalculationService : CalculationServiceBase
	{
		private readonly JointAmountToPayCalculationService jointAmountToPayCalculationService;

		public VendorPreparedBalanceCalculationService(PXGraph graph)
			: base(graph)
		{
			jointAmountToPayCalculationService = new JointAmountToPayCalculationService(graph);
		}

		public virtual decimal? GetVendorPreparedBalance(APAdjust adjustment)
		{
			FinDocumentExtKey actualDocLineKey = InvoiceDataProvider.GetSourceEntityKeyByRetainage(Graph,
				adjustment.AdjdDocType, adjustment.AdjdRefNbr, adjustment.AdjdLineNbr);

			decimal? fullAmount;

			if (adjustment.AdjdLineNbr == 0)
			{
				APInvoice bill = InvoiceDataProvider.GetInvoice(Graph, actualDocLineKey.Type, actualDocLineKey.RefNbr);

				fullAmount = bill.CuryOrigDocAmt + bill.CuryRetainageTotal;
			}
			else
			{
				APTran tran = TransactionDataProvider.GetTransaction(Graph, actualDocLineKey.Type, actualDocLineKey.RefNbr, actualDocLineKey.LineNbr);

				fullAmount = tran.CuryOrigTranAmt + tran.CuryRetainageAmt;
			}

			var totalJointAmountOwed = JointPayeeDataProvider.GetJointPayees(Graph, actualDocLineKey.RefNbr, actualDocLineKey.LineNbr)
															.Sum(jp => jp.JointAmountOwed.GetValueOrDefault());
			var totalVendorPaymentAmount = GetTotalVendorPaymentAmount(adjustment, actualDocLineKey.RefNbr, actualDocLineKey.LineNbr);
			var currentVendorPaymentAmount = GetVendorPaymentAmount(adjustment);
			var reversedRetainageAmount = GetReversedRetainageAmount(actualDocLineKey.RefNbr, actualDocLineKey.LineNbr);
			return fullAmount + reversedRetainageAmount - totalJointAmountOwed
				- (totalVendorPaymentAmount - currentVendorPaymentAmount);
		}

		protected decimal? GetVendorPaymentAmount(APAdjust adjustment)
		{
			return adjustment.CuryAdjgAmt - jointAmountToPayCalculationService.GetTotalJointAmountToPay(adjustment);
		}

		protected virtual decimal? GetTotalVendorPaymentAmount(APAdjust adjustment, string origRefNbr, int? origLineNbr)
		{
			return AdjustmentDataProvider
				.GetAdjustmentsForInvoiceGroup(Graph, origRefNbr, origLineNbr)
				.Sum(adj => GetVendorPaymentAmount(adj).GetValueOrDefault());
		}

		protected virtual decimal? GetReversedRetainageAmount(string referenceNumber, int? lineNumber)
		{
			if (lineNumber == 0)
			{
				return InvoiceDataProvider.GetReversedRetainageDebitAdjustments(Graph, referenceNumber)
					.Sum(inv => inv.CuryOrigDocAmt);
			}
			else
			{
				return InvoiceDataProvider.GetReversedRetainageDebitAdjustmentLines(Graph, referenceNumber, lineNumber)
					.Sum(inv => inv.CuryOrigTranAmt);

			}
		}
	}
}