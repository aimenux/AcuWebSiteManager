using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.TX;

namespace PX.Objects.EP
{
	public abstract class EPTaxBaseAttribute : TaxAttribute
	{
		public abstract bool IsTaxTipAttribute();
		protected override bool CalcGrossOnDocumentLevel { get => true; set => base.CalcGrossOnDocumentLevel = value; }
		protected override bool AskRecalculationOnCalcModeChange { get => true; set => base.AskRecalculationOnCalcModeChange = value; }

		public EPTaxBaseAttribute()
			: base(typeof(EPExpenseClaimDetails), typeof(EPTax), typeof(EPTaxTran), typeof(EPExpenseClaimDetails.taxCalcMode))
		{
		}
		public EPTaxBaseAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode)
			: base(ParentType, TaxType, TaxSumType, CalcMode)

		{
			Init();
		}

		private void Init()
		{
			_LineNbr = "ClaimDetailID";
		}

		protected override object InitializeTaxDet(object data)
		{
			data = base.InitializeTaxDet(data);
			if (data is EPTax)
			{
				((EPTax)data).IsTipTax = IsTaxTipAttribute();
			}
			else if (data is EPTaxTran)
			{
				((EPTaxTran)data).IsTipTax = IsTaxTipAttribute();
			}
			return data;
		}
		protected override bool AskRecalculate(PXCache sender, PXCache detailCache, object detail)
		{
			return false;
		}
		protected override List<object> ChildSelect(PXCache cache, object data)
		{
			return new List<object>() { data };
		}
		protected override decimal CalcLineTotal(PXCache sender, object row)
		{
			return (decimal?)ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m;
		}
		protected override decimal? GetTaxableAmt(PXCache sender, object row)
		{
			return (decimal?)sender.GetValue<EPExpenseClaimDetails.curyTaxableAmt>(row);
		}

		protected override decimal? GetTaxAmt(PXCache sender, object row)
		{
			return (decimal?)sender.GetValue<EPExpenseClaimDetails.curyTaxTotal>(row);
		}
		protected virtual string GetRefNbr(PXCache sender, object row)
		{
			return (string)sender.GetValue<EPExpenseClaimDetails.refNbr>(row);
		}

		protected virtual string GetTaxZoneLocal(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxZoneID);
		}

		protected override string GetExtCostLabel(PXCache sender, object row)
		{
			return ((PXDecimalState)sender.GetValueExt<EPExpenseClaimDetails.curyExtCost>(row)).DisplayName;
		}
		public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EPExpenseClaimDetails row = (EPExpenseClaimDetails)e.Row;
			if (row.LegacyReceipt == true)
			{
				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)) ||
					!object.Equals(GetCuryTranAmt(sender, e.OldRow), GetCuryTranAmt(sender, e.Row)) ||
					!object.Equals(GetTaxZoneLocal(sender, e.OldRow), GetTaxZoneLocal(sender, e.Row)) ||
					!object.Equals(GetRefNbr(sender, e.OldRow), GetRefNbr(sender, e.Row)) ||
					!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{

					ExpenseClaimDetailEntry.ExpenseClaimDetailEntryExt.DeleteLegacyTaxRows(sender.Graph, row.RefNbr);

					PXCache cache = sender.Graph.Caches[_ChildType];
					Preload(sender);

					List<object> details = this.ChildSelect(cache, e.Row);
					ReDefaultTaxes(cache, details);

					_ParentRow = e.Row;
					CalcTaxes(cache, null);
					_ParentRow = null;
					row.LegacyReceipt = false;
				}
			}
			base.RowUpdated(sender, e);

		}
		protected override void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Tax_RowUpdated(sender, e);
			TaxDetail taxSum = e.Row as TaxDetail;
			TaxDetail taxSumOld = e.OldRow as TaxDetail;
			if (e.ExternalCall && (!sender.ObjectsEqual<curyTaxAmt>(taxSum, taxSumOld) || !sender.ObjectsEqual<curyExpenseAmt>(taxSum, taxSumOld)))
			{
				PXCache parentCache = ParentCache(sender.Graph);

				if (parentCache.Current != null)
				{
					ParentUpdate(sender.Graph);
				}
			}

		}
		protected virtual void ParentUpdate(PXGraph graph)
		{
			PXCache cache = ParentCache(graph);

			if (_ParentRow == null)
			{
				cache.Update(cache.Current);
			}
			else
			{
				cache.Update(_ParentRow);
			}
		}
		protected override void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache cache = ParentCache(graph);

			if (_ParentRow == null)
			{
				object copy = cache.CreateCopy(cache.Current);
				cache.SetValueExt(cache.Current, fieldname, value);
			}
			else
			{
				cache.SetValueExt(_ParentRow, fieldname, value);
			}
		}
		protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
		{
			IComparer<Tax> taxByCalculationLevelComparer = GetTaxByCalculationLevelComparer();
			taxByCalculationLevelComparer.ThrowOnNull(nameof(taxByCalculationLevelComparer));

			Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
			object[] currents = new object[] { row };

			foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
				LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
					And<TaxRev.outdated, Equal<boolFalse>,
					And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<boolFalse>,
						Or<TaxRev.taxType, Equal<TaxType.sales>, And<Tax.reverseTax, Equal<boolTrue>,
						Or<Tax.taxType, Equal<CSTaxType.use>,
						Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>,
					And<Current<EPExpenseClaimDetails.expenseDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>,
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
				case PXTaxCheck.RecalcLine:
					PXSelectBase<EPTax> cmdEPTax = new PXSelect<EPTax,
										Where<EPTax.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>>(graph);
					if (IsTaxTipAttribute())
					{
						cmdEPTax.WhereAnd<Where<EPTax.isTipTax, Equal<True>>>();
					}
					else
					{
						cmdEPTax.WhereAnd<Where<EPTax.isTipTax, Equal<False>>>();
					}

					foreach (EPTax record in cmdEPTax.View.SelectMultiBound(currents))
					{
						if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<EPTax, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;

							ret.Insert(idx, new PXResult<EPTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}

					break;

				case PXTaxCheck.RecalcTotals:
					PXSelectBase<EPTaxTran> cmdEPTaxTran = new PXSelect<EPTaxTran,
										Where<EPTaxTran.claimDetailID, Equal<Current<EPExpenseClaimDetails.claimDetailID>>>>(graph);
					if (IsTaxTipAttribute())
					{
						cmdEPTaxTran.WhereAnd<Where<EPTaxTran.isTipTax, Equal<True>>>();
					}
					else
					{
						cmdEPTaxTran.WhereAnd<Where<EPTaxTran.isTipTax, Equal<False>>>();
					}

					foreach (EPTaxTran record in cmdEPTaxTran.View.SelectMultiBound(currents))
					{
						if (tail.TryGetValue(record.TaxID, out PXResult<Tax, TaxRev> line))
						{
							int idx;
							for (idx = ret.Count;
								(idx > 0) && taxByCalculationLevelComparer.Compare((PXResult<EPTaxTran, Tax, TaxRev>)ret[idx - 1], line) > 0;
								idx--) ;

							ret.Insert(idx, new PXResult<EPTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
						}
					}

					break;
			}
			return ret;
		}
		protected override List<object> SelectDocumentLines(PXGraph graph, object row)
		{
			List<object> ret = PXSelect<EPExpenseClaimDetails,
								Where<EPExpenseClaimDetails.refNbr, Equal<Current<EPExpenseClaim.refNbr>>>>
									.SelectMultiBound(graph, new object[] { row })
									.RowCast<EPExpenseClaimDetails>()
									.Select(_ => (object)_)
									.ToList();
			return ret;
		}
	}

	public class EPTaxAttribute : EPTaxBaseAttribute
	{
		public override bool IsTaxTipAttribute()
		{
			return false;
		}
		public EPTaxAttribute()
			: base(typeof(EPExpenseClaimDetails), typeof(EPTax), typeof(EPTaxTran), typeof(EPExpenseClaimDetails.taxCalcMode))
		{
			DocDate = typeof(EPExpenseClaimDetails.expenseDate);
			CuryLineTotal = typeof(EPExpenseClaimDetails.curyTaxableAmt);
			CuryTranAmt = typeof(EPExpenseClaimDetails.curyTaxableAmt);
			CuryDocBal = typeof(EPExpenseClaimDetails.curyAmountWithTaxes);
		}
		protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
		{
			sender.SetValueExt<EPExpenseClaimDetails.curyExtCost>(child, value);
		}
		protected override void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<EPExpenseClaimDetails.curyTaxableAmtFromTax>(row, value);
		}
		protected override void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
			sender.SetValue<EPExpenseClaimDetails.curyTaxAmt>(row, value);
		}
	}

	public class EPTaxTipAttribute : EPTaxBaseAttribute
	{
		public override bool IsTaxTipAttribute()
		{
			return true;
		}
		public EPTaxTipAttribute()
			: base(typeof(EPExpenseClaimDetails), typeof(EPTax), typeof(EPTaxTran), typeof(EPExpenseClaimDetails.taxCalcMode))
		{
			DocDate = typeof(EPExpenseClaimDetails.expenseDate);
			CuryLineTotal = typeof(EPExpenseClaimDetails.curyTipAmt);
			CuryTranAmt = typeof(EPExpenseClaimDetails.curyTipAmt);
			CuryDocBal = typeof(EPExpenseClaimDetails.curyTipAmt);
			TaxCategoryID = typeof(EPExpenseClaimDetails.taxTipCategoryID);
			CuryTaxTotal = typeof(EPExpenseClaimDetails.curyTaxTipTotal);
            CuryTaxRoundDiff = typeof(EPExpenseClaimDetails.curyTaxTipTotal);
			TaxRoundDiff = typeof(EPExpenseClaimDetails.curyTaxTipTotal);
		}
	}
}
