using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.TX;
using ARCashSale = PX.Objects.AR.Standalone.ARCashSale;

namespace PX.Objects.AR
{
	public class ARCashSaleTaxAttribute : ARTaxAttribute
	{
		public ARCashSaleTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type parentBranchIDField = null)
			: base(ParentType, TaxType, TaxSumType, parentBranchIDField: parentBranchIDField)
		{
			DocDate = typeof(ARCashSale.adjDate);
			FinPeriodID = typeof(ARCashSale.adjFinPeriodID);
			CuryLineTotal = typeof(ARCashSale.curyLineTotal);
			this._Attributes.Add(new PXUnboundFormulaAttribute(typeof(Switch<Case<Where<ARTran.lineType, NotEqual<SO.SOLineType.discount>>, ARTran.curyTranAmt>, Minus<ARTran.curyTranAmt>>), typeof(SumCalc<ARCashSale.curyLineTotal>)));
		}

		protected override void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			ARCashSale doc;
			if (e.Row is ARCashSale && ((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.ParentFieldUpdated(sender, e);
			}
			else if (e.Row is CurrencyInfo && (doc = PXSelect<ARCashSale, Where<ARCashSale.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(sender.Graph, ((CurrencyInfo)e.Row).CuryInfoID)) != null && doc.DocType != ARDocType.CashReturn)
			{
				base.ParentFieldUpdated(sender, e);
			}
		}

		protected override void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.ZoneUpdated(sender, e);
			}
		}

		protected override void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (((ARCashSale)e.Row).DocType != ARDocType.CashReturn)
			{
				base.DateUpdated(sender, e);
			}
		}
		protected override bool IsRetainedTaxes(PXGraph graph)
		{
			return false;
		}

		protected override bool ConsiderEarlyPaymentDiscount(PXCache sender, object parent, Tax tax)
		{
			return
				(tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
				|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
							&&
				tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment;
		}
		protected override bool ConsiderInclusiveDiscount(PXCache sender, object parent, Tax tax)
		{
			return (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToPromtPayment);
		}

		protected override void _CalcDocTotals(
			PXCache sender,
			object row,
			decimal CuryTaxTotal,
			decimal CuryInclTaxTotal,
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryDiscountTotal = (decimal)(ParentGetValue(sender.Graph, _CuryDiscTot) ?? 0m);
			decimal CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal + CuryTaxDiscountTotal - CuryInclTaxTotal - CuryDiscountTotal;

			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (!Equals(CuryTaxTotal, doc_CuryTaxTotal))
			{
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);
			}

			if (!string.IsNullOrEmpty(_CuryTaxDiscountTotal))
			{
				ParentSetValue(sender.Graph, _CuryTaxDiscountTotal, CuryTaxDiscountTotal);
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
			}
		}
	}
}
