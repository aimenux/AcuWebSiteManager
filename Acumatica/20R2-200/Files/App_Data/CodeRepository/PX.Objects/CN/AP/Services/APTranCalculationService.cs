using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.Common;
using PX.Objects.TX;

namespace PX.Objects.CN.AP.Services
{
	public class APTranCalculationService
	{
		protected readonly PXGraph Graph;

		public APTranCalculationService(PXGraph graph)
		{
			Graph = graph;
		}

		public Dictionary<int?, decimal?> CalcCuryOrigTranAmts(string docType, string refNbr, int?[] lineNbrs, List<APTran> trans = null)
		{
			IEnumerable<APTran> transToProcess;

			if (trans == null)
			{
				PXSelectBase<APTran> tranQuery = new PXSelect<APTran,
					Where<APTran.tranType, Equal<Required<APTran.tranType>>,
						And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>(Graph);

				if (lineNbrs != null)
				{
					tranQuery.WhereAnd<Where<APTran.lineNbr, In<Required<APTran.lineNbr>>>>();
				}

				transToProcess = tranQuery.Select(docType, refNbr, lineNbrs)
											.FirstTableItems;
			}
			else
			{
				transToProcess = trans;
			}

			PXSelectBase<APTax> lineTaxQuery = new PXSelect<APTax,
														Where<APTax.tranType, Equal<Required<APTax.tranType>>,
															And<APTax.refNbr, Equal<Required<APTax.refNbr>>>>>(Graph);

			if (lineNbrs != null)
			{
				lineTaxQuery.WhereAnd<Where<APTax.lineNbr, In<Required<APTax.lineNbr>>>>();
			}

			Dictionary<int?, APTran> apTransByLine = transToProcess.ToDictionary(row => row.LineNbr, row => row);

			Dictionary<int?, decimal?> amountsByLineNbr = new Dictionary<int?, decimal?>();

			foreach (APTran tran in apTransByLine.Values)
			{
				amountsByLineNbr[tran.LineNbr] = tran.CuryTranAmt;
			}

			PXResultset<APTax> apTaxes = lineTaxQuery.Select(docType, refNbr, lineNbrs);

			foreach (var group in apTaxes.AsEnumerable().GroupBy(row => ((APTax) row).LineNbr, row => (APTax) row))
			{
				APTran tran = apTransByLine[group.Key];

				decimal? curyTaxAmtAddition = 0m;

				foreach (APTax apTax in group.RowCast<APTax>())
				{
					Tax tax = PXSelect<Tax, Where<Tax.taxID, Equal<Required<Tax.taxID>>>>.Select(Graph, apTax.TaxID);

					bool includeBalance =
						apTax?.TaxID != null &&
						APReleaseProcess.IncludeTaxInLineBalance(tax);

					if (includeBalance)
					{
						decimal sign = tax.ReverseTax == true ? -1m : 1m;
						decimal curyTaxAmt = (apTax.CuryTaxAmt ?? 0m) + (apTax.CuryExpenseAmt ?? 0m);
						curyTaxAmt *= sign;
						curyTaxAmtAddition += curyTaxAmt;
					}
				}

				amountsByLineNbr[tran.LineNbr] += curyTaxAmtAddition;
			}

			return amountsByLineNbr;
		}
	}
}
