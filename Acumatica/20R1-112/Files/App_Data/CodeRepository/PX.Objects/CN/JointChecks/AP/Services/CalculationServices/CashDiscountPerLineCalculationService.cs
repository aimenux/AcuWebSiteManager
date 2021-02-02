using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
	public class CashDiscountPerLineCalculationService : CashDiscountCalculationService
	{
		public CashDiscountPerLineCalculationService(PXGraph graph)
			: base(graph)
		{
		}

		public override decimal? GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(APAdjust apAdjust)
		{
			var nonReleasedCashDiscountTaken = AdjustmentDataProvider
				.GetAdjustmentsForInvoiceGroup(Graph, apAdjust.AdjdRefNbr, apAdjust.AdjdLineNbr)
				.Where(adjust => !adjust.Released.GetValueOrDefault())
				.Sum(adjust => adjust.CuryAdjgPPDAmt);
			return nonReleasedCashDiscountTaken - apAdjust.CuryAdjgPPDAmt;
		}

		protected override decimal GetAllowableCashDiscountConsiderBillBalance(APAdjust apAdjust)
		{
			var billLineBalance = TransactionDataProvider
				.GetTransaction(Graph, apAdjust.AdjdDocType, apAdjust.AdjdRefNbr, apAdjust.AdjdLineNbr).CuryTranBal;
			var cashDiscount = billLineBalance - apAdjust.CuryAdjgPPDAmt;
			return Math.Max(cashDiscount.GetValueOrDefault(), 0);
		}

		protected override decimal? GetCashDiscountBalance(APAdjust adjustment)
		{
			return TransactionDataProvider
				.GetTransaction(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr, adjustment.AdjdLineNbr)
				.CuryCashDiscBal;
		}
	}
}