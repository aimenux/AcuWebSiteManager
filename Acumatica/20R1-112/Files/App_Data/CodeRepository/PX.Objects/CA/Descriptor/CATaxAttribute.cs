using System;
using System.Linq;
using System.Collections.Generic;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;

namespace PX.Objects.CA
{
	public class CATaxAttribute : ManualVATAttribute
	{
		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }
		protected override bool AskRecalculationOnCalcModeChange { get => true; set => base.AskRecalculationOnCalcModeChange = value; }

		public CATaxAttribute(Type parentType, Type taxType, Type taxSumType, Type calcMode = null, Type parentBranchIDField = null)
			: base(parentType, taxType, taxSumType, calcMode, parentBranchIDField)
		{
			Init();
		}
		
		private void Init()
		{
			this.CuryDocBal = typeof(CAAdj.curyTranAmt);
			this.CuryLineTotal = typeof(CAAdj.curySplitTotal);
			this.DocDate = typeof(CAAdj.tranDate);
		}

		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			object[] currents = new object[] { row, ((CATranEntry)graph).CAAdjRecords.Current };

			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<
						Where<Current<CAAdj.drCr>, Equal<CADrCr.cACredit>, And<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>,
						Or<Current<CAAdj.drCr>, Equal<CADrCr.cACredit>, And<TaxRev.taxType, Equal<TaxType.sales>, And2<Where<Tax.reverseTax, Equal<boolTrue>, 
							Or<Tax.taxType, Equal<CSTaxType.use>>>,
						Or<Current<CAAdj.drCr>, Equal<CADrCr.cADebit>, And<TaxRev.taxType, Equal<TaxType.sales>, And<Tax.reverseTax, Equal<boolFalse>, 
							And<Tax.taxType, NotEqual<CSTaxType.withholding>, And<Tax.taxType, NotEqual<CSTaxType.use>>>>>>>>>>>>,
					And<Current<CAAdj.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
				Where>
				.SelectMultiBound(graph, currents, parameters))
			{
				Tax adjdTax = AdjustTaxLevel(graph, (Tax)record);
				tail[((Tax)record).TaxID] = new PXResult<Tax, TaxRev>(adjdTax, (TaxRev)record);
			}

			List<object> ret = new List<object>();

			switch (taxchk)
			{
				case PXTaxCheck.Line:
					foreach (CATax record in PXSelect<CATax,
						Where<CATax.adjTranType, Equal<Current<CASplit.adjTranType>>,
							And<CATax.adjRefNbr, Equal<Current<CASplit.adjRefNbr>>,
							And<CATax.lineNbr, Equal<Current<CASplit.lineNbr>>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx = CalculateIndex<CATax>(ret, line, taxByCalculationLevelComparer);
							ret.Insert(idx, new PXResult<CATax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;

				case PXTaxCheck.RecalcLine:
					foreach (CATax record in PXSelect<CATax,
						Where<CATax.adjTranType, Equal<Current<CAAdj.adjTranType>>,
							And<CATax.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (tail.TryGetValue(record.TaxID, out line))
						{
							int idx = CalculateIndex<CATax>(ret, line, taxByCalculationLevelComparer);
							ret.Insert(idx, new PXResult<CATax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;

				case PXTaxCheck.RecalcTotals:
					foreach (CATaxTran record in PXSelect<CATaxTran,
						Where<CATaxTran.module, Equal<BatchModule.moduleCA>,
							And<CATaxTran.tranType, Equal<Current<CAAdj.adjTranType>>,
							And<CATaxTran.refNbr, Equal<Current<CAAdj.adjRefNbr>>>>>>
						.SelectMultiBound(graph, currents))
					{
						PXResult<Tax, TaxRev> line;
						if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
						{
							int idx = CalculateIndex<CATaxTran>(ret, line, taxByCalculationLevelComparer);
							ret.Insert(idx, new PXResult<CATaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}
					return ret;

				default:
					return ret;
			}
		}

		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<CASplit,
								Where<CASplit.adjTranType, Equal<Current<CAAdj.adjTranType>>,
									And<CASplit.adjRefNbr, Equal<Current<CAAdj.adjRefNbr>>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<CASplit>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}

		private int CalculateIndex<T>(List<object> ret, PXResult<Tax, TaxRev> line, IComparer<Tax> taxByCalculationLevelComparer)
			where T : class, IBqlTable, new()
		{
			int idx;
			for (idx = ret.Count;
				(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<T, Tax, TaxRev>)ret[idx - 1], line) > 0;
				idx--) ;
			return idx;
		}

		public override void CacheAttached(PXCache sender)
		{
			if (sender.Graph is CATranEntry)
			{
				base.CacheAttached(sender);
			}
			else
			{
				this.TaxCalc = TaxCalc.NoCalc;
			}
		}

		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			if (sender.Graph is CATranEntry)
			{
				decimal curyLineTotal = 0m;
				foreach (CASplit detrow in ((CATranEntry)sender.Graph).CASplitRecords.View.SelectMultiBound(new object[1] { row }))
				{
					curyLineTotal += detrow.CuryTranAmt.GetValueOrDefault();
				}
				return curyLineTotal;
			}
			else
			{
				return base.CalcLineTotal(sender, row);
			}
		}

		protected override void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<CASplit.curyTaxableAmt>(row, value);
		}

		protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<CASplit.curyTaxAmt>(row, value);
		}

		protected override decimal? GetTaxableAmt(PXCache sender, object row)
		{
			return (decimal?)sender.GetValue<CASplit.curyTaxableAmt>(row);
		}

		protected override decimal? GetTaxAmt(PXCache sender, object row)
		{
			return (decimal?)sender.GetValue<CASplit.curyTaxAmt>(row);
		}

		protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
		{
			CASplit row = child as CASplit;
			if (row != null)
			{
				row.CuryTranAmt = value;
				sender.Update(row);
			}
		}

		protected override string GetExtCostLabel(PXCache sender, object row)
		{
			return ((PXDecimalState) sender.GetValueExt<CASplit.curyTranAmt>(row)).DisplayName;
		}
		
		protected override bool isControlTaxTotalRequired(PXCache sender)
		{
			CASetup setup = new PXSetup<CASetup>(sender.Graph).Select();
			return setup != null && setup.RequireControlTaxTotal == true;
		}

		protected override bool isControlTotalRequired(PXCache sender)
		{
			CASetup setup = new PXSetup<CASetup>(sender.Graph).Select();
			return setup != null && setup.RequireControlTotal == true;
		}

	} 
}
