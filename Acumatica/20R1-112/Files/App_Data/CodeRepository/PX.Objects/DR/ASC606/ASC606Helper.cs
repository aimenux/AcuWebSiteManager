using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.TX;
using Amount = PX.Objects.AR.ARReleaseProcess.Amount;


namespace PX.Objects.DR
{
	internal class ASC606Helper
	{
		public static Amount CalculateSalesPostingAmount(PXGraph graph, DRSchedule customSchedule)
		{
			var netLinesAmount = new Amount(0m, 0m);

			var processedTrans = new HashSet<ARTran>();

			foreach (PXResult<ARTran, ARRegister, ARTax, Tax> item in PXSelectJoin<
					ARTran,
					InnerJoin<ARRegister, On<ARTran.tranType, Equal<ARRegister.docType>,
						And<ARTran.refNbr, Equal<ARRegister.refNbr>>>,
					LeftJoin<ARTax,
						On<ARTran.tranType, Equal<ARTax.tranType>,
						And<ARTran.refNbr, Equal<ARTax.refNbr>,
						And<ARTran.lineNbr, Equal<ARTax.lineNbr>>>>,
					LeftJoin<Tax,
						On<Tax.taxID, Equal<ARTax.taxID>>>>>,
					Where<ARTran.tranType, Equal<Required<ARInvoice.docType>>,
						And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
						And<ARTran.deferredCode, IsNull,
						And<Where<ARTran.lineType, IsNull, 
							Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>>
					.Select(graph, customSchedule.DocType, customSchedule.RefNbr))
			{
				ARTran line = item;
				ARRegister register = item;
				ARTax artax = item;
				Tax tax = item;

				if (processedTrans.Contains(line))
				{
					continue;
				}

				var netLineAmount = ARReleaseProcess.GetSalesPostingAmount(graph, register, line, artax, tax,
					amnt => PXDBCurrencyAttribute.Round(graph.Caches[typeof(ARTran)], line, amnt, CMPrecision.TRANCURY));

				netLinesAmount += netLineAmount;

				processedTrans.Add(line);
			}

			return netLinesAmount;
		}

		public static Amount CalculateNetAmount(PXGraph graph, ARRegister document) => CalculateNetAmountForDeferredLines(graph, document.DocType, document.RefNbr);
		public static Amount CalculateNetAmount(PXGraph graph, DRSchedule document) => CalculateNetAmountForDeferredLines(graph, document.DocType, document.RefNbr);

		public static Amount CalculateNetAmount(PXGraph graph, ARInvoice document, out decimal deferredNetDiscountRate, out int? defScheduleID) => CalculateNetAmountForDeferredLines(graph, document.DocType, document.RefNbr, out deferredNetDiscountRate, out defScheduleID, invoiceCuryDiscTotal: document.CuryDiscTot);

		private static Amount CalculateNetAmountForDeferredLines(PXGraph graph, string docType, string refNbr)
		{
			decimal deferredNetDiscountRate = 0m;
			int? defScheduleID = null;

			return CalculateNetAmountForDeferredLines(graph, docType, refNbr, out deferredNetDiscountRate, out defScheduleID);
		}

		private static Amount CalculateNetAmountForDeferredLines(PXGraph graph, string docType, string refNbr, 
						out decimal deferredNetDiscountRate, out int? defScheduleID, decimal? invoiceCuryDiscTotal = null)
		{
			var netLinesAmount = new Amount(0m, 0m);
			decimal deferredDistributedDiscountAmount = 0m;
			deferredNetDiscountRate = 0m;
			defScheduleID = null;

			foreach (ARTran line in PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Required<ARInvoice.docType>>,
						And<ARTran.refNbr, Equal<Required<ARInvoice.refNbr>>,
						And<ARTran.inventoryID, IsNotNull,
						And<ARTran.deferredCode, IsNotNull,
						And<Where<
							ARTran.lineType, IsNull,
							Or<ARTran.lineType, NotEqual<SO.SOLineType.discount>>>>>>>>>.Select(graph, docType, refNbr))
			{
				Func<decimal, decimal> round = amount => PXDBCurrencyAttribute.Round(graph.Caches[typeof(ARTran)], line, amount, CMPrecision.TRANCURY);

				var origCuryDiscountAmount = (decimal)(line.CuryTranAmt * line.OrigGroupDiscountRate * line.OrigDocumentDiscountRate);
				var currentCuryDiscountAmount = round((decimal)(line.CuryTranAmt * line.DocumentDiscountRate * line.GroupDiscountRate));
				var currentBaseDiscountAmount = round((decimal)(line.TranAmt * line.DocumentDiscountRate * line.GroupDiscountRate));

				decimal? curyAmt = (line.CuryTaxableAmt == 0m) ? currentCuryDiscountAmount : line.CuryTaxableAmt;
				decimal? baseAmt = (line.TaxableAmt == 0m) ? currentBaseDiscountAmount : line.TaxableAmt;
				decimal? discountAmountPerLine = line.CuryTranAmt - origCuryDiscountAmount + line.CuryTranAmt - currentCuryDiscountAmount;

				deferredDistributedDiscountAmount += discountAmountPerLine ?? 0m;

				netLinesAmount += new Amount(curyAmt, baseAmt);
			}

			if (invoiceCuryDiscTotal.HasValue && invoiceCuryDiscTotal > 0m)
			{
				deferredNetDiscountRate = deferredDistributedDiscountAmount / invoiceCuryDiscTotal.Value;
			}

			return netLinesAmount;
		}
	}
}
