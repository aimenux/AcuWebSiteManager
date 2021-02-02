using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PX.Api;
using System.Collections;
using PX.Data.SQLTree;
using System.Reflection;
using PX.Common;

namespace PX.Objects.TX
{
	public abstract class TaxAttribute : TaxBaseAttribute
	{
		#region CuryTaxRoundDiff
		protected string _CuryTaxRoundDiff = "CuryTaxRoundDiff";
		public Type CuryTaxRoundDiff
		{
			set
			{
				_CuryTaxRoundDiff = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxRoundDiff
		protected string _TaxRoundDiff = "TaxRoundDiff";
		public Type TaxRoundDiff
		{
			set
			{
				_TaxRoundDiff = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion

		public TaxAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode = null, Type parentBranchIDField = null)
			: base(ParentType, TaxType, TaxSumType, CalcMode, parentBranchIDField)
		{
		}

		protected override IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row)
		{
			List<ITaxDetail> ret = new List<ITaxDetail>();

			foreach (PXResult res in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				ret.Add((ITaxDetail)res[0]);
			}
			return ret;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.RowInserted.AddHandler(_TaxSumType, Tax_RowInserted);
			sender.Graph.RowDeleted.AddHandler(_TaxSumType, Tax_RowDeleted);
			sender.Graph.RowUpdated.AddHandler(_TaxSumType, Tax_RowUpdated);
			sender.Graph.FieldUpdated.AddHandler(_TaxSumType, _CuryTaxableAmt, Tax_CuryTaxableAmt_FieldUpdated);
		}

		protected bool _NoSumTotals = false;
		public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }
		public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }
		public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }
		public abstract class curyExemptedAmt : IBqlField { }

		protected virtual void Tax_CuryTaxableAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxDetail taxdetOrig = e.Row as TaxDetail;
			if (taxdetOrig == null) return;

			decimal newValue = (decimal)(sender.GetValue(e.Row, _CuryTaxableAmt) ?? 0m);
			decimal oldValue = (decimal)(e.OldValue ?? 0m);

			if (_TaxCalc != TaxCalc.NoCalc && e.ExternalCall &&
				newValue != oldValue)
			{
				foreach (object taxrow in SelectTaxes<
					Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcTotals, taxdetOrig.TaxID))
				{
					TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
					Tax tax = PXResult.Unwrap<Tax>(taxrow);
					TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

					CalculateTaxSumTaxAmt(sender, taxdet, tax, taxrev);
				}
			}
		}

		protected virtual void Tax_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			TaxDetail taxSum = e.Row as TaxDetail;
			TaxDetail taxSumOld = e.OldRow as TaxDetail;

			if (e.ExternalCall && IsTaxRowAmountUpdated(sender, e))
			{
				PXCache parentCache = ParentCache(sender.Graph);

				if (parentCache.Current != null)
				{
					decimal discrepancy = CalculateTaxDiscrepancy(parentCache, parentCache.Current);
					decimal discrepancyBase;
					PXDBCurrencyAttribute.CuryConvBase(ParentCache(sender.Graph), ParentRow(sender.Graph), discrepancy, out discrepancyBase);
					ParentSetValue(sender.Graph, _CuryTaxRoundDiff, discrepancy);
					ParentSetValue(sender.Graph, _TaxRoundDiff, discrepancyBase);
				}
			}

			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				if (e.OldRow != null && e.Row != null)
				{
					if (taxSumOld.TaxID != taxSum.TaxID)
					{
						VerifyTaxID(sender, e.Row, e.ExternalCall);
					}
					if (IsTaxRowAmountUpdated(sender, e))
					{
						CalcTotals(sender.Graph.Caches[_ChildType], null, false);
					}
				}
			}
		}

		protected virtual bool IsTaxRowAmountUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			return !sender.ObjectsEqual<curyTaxAmt, curyExpenseAmt>(e.Row, e.OldRow);
		}

		protected virtual void Tax_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				VerifyTaxID(sender, e.Row, e.ExternalCall);
			}
		}

		protected virtual void VerifyTaxID(PXCache sender, object row, bool externalCall)
		{
			bool nomatch = false;
			PXCache cache = sender.Graph.Caches[_ChildType];
			PXCache taxcache = sender.Graph.Caches[_TaxType];

			//TODO: move details to parameter
			List<object> details = ChildSelect(cache, row);
			foreach (object det in details)
			{
				ITaxDetail taxzonedet = MatchesCategory(cache, det, (ITaxDetail)row);
				AddOneTax(taxcache, det, taxzonedet);
			}
			_NoSumTotals = (_TaxCalc == TaxCalc.ManualCalc && ((TaxDetail)row).TaxRate != 0m && externalCall == false);
			PXRowDeleting del = delegate (PXCache _sender, PXRowDeletingEventArgs _e) { nomatch |= object.ReferenceEquals(row, _e.Row); };
			sender.Graph.RowDeleting.AddHandler(_TaxSumType, del);
			try
			{
				CalcTaxes(cache, null);
			}
			finally
			{
				sender.Graph.RowDeleting.RemoveHandler(_TaxSumType, del);
			}
			_NoSumTotals = false;

			if (nomatch)
			{
				sender.RaiseExceptionHandling("TaxID", row, ((TaxDetail)row).TaxID, new PXSetPropertyException(TX.Messages.NoLinesMatchTax, PXErrorLevel.RowError));
			}
			sender.Current = row;
		}

		protected virtual void Tax_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if ((_TaxCalc != TaxCalc.NoCalc && e.ExternalCall || _TaxCalc == TaxCalc.ManualCalc))
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				PXCache taxcache = sender.Graph.Caches[_TaxType];

				List<object> details = ChildSelect(cache, e.Row);
				foreach (object det in details)
				{
					DelOneTax(taxcache, det, e.Row);
				}
				CalcTaxes(cache, null);
			}
		}

		protected override List<Object> SelectDocumentLines(PXGraph graph, object row)
		{
			throw new PXException(Messages.MethodMustBeOverridden);
		}

		protected override void CalcTotals(PXCache sender, object row, bool CalcTaxes)
		{
			string taxZondeID = GetTaxZone(sender, row);
			bool isExternalTax = ExternalTax.IsExternalTax(sender.Graph, taxZondeID);

			bool doCalc = _NoSumTotals == false && CalcTaxes && !isExternalTax;
			if (doCalc && (ParentCache(sender.Graph).Current != null || _ParentRow != null))
			{
				ResetDiscrepancy(sender.Graph);
			}
			base.CalcTotals(sender, row, doCalc);
		}

		protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
		{
			throw new PXException(Messages.MethodMustBeOverridden);
		}

		protected override string GetExtCostLabel(PXCache sender, object row)
		{
			throw new PXException(Messages.MethodMustBeOverridden);
		}

		protected override decimal? GetTaxableAmt(PXCache sender, object row)
		{
			throw new PXException(Messages.MethodMustBeOverridden);
		}

		protected override decimal? GetTaxAmt(PXCache sender, object row)
		{
			throw new PXException(Messages.MethodMustBeOverridden);
		}

		public static Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal)
		{
			CalcTaxable calcClass = new CalcTaxable(true, TaxCalcLevelEnforcing.None);
			return calcClass.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID, aTaxCategoryID, aDocDate, aCuryTotal);
		}

		public static Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal, bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType)
		{
			CalcTaxable calcClass = new CalcTaxable(aSalesOrPurchaseSwitch, enforceType);
			return calcClass.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID, aTaxCategoryID, aDocDate, aCuryTotal);
		}

		public static Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal, bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType, string taxCalcMode)
		{
			CalcTaxable calcClass = new CalcTaxable(aSalesOrPurchaseSwitch, enforceType, taxCalcMode);
			return calcClass.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID, aTaxCategoryID, aDocDate, aCuryTotal);
		}

		public static Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal, int customPrecision)
		{
			CalcTaxable calcClass = new CalcTaxable(true, TaxCalcLevelEnforcing.None, customPrecision);
			return calcClass.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID, aTaxCategoryID, aDocDate, aCuryTotal);
		}

		public static Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal, bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType, int customPrecision)
		{
			CalcTaxable calcClass = new CalcTaxable(aSalesOrPurchaseSwitch, enforceType, customPrecision);
			return calcClass.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID, aTaxCategoryID, aDocDate, aCuryTotal);
		}

		public enum TaxCalcLevelEnforcing
		{
			None, EnforceCalcOnItemAmount, EnforceInclusive
		}

		public class CalcTaxable
		{
			private bool _aSalesOrPurchaseSwitch;
			private TaxCalcLevelEnforcing _enforcing;
			private int? _precision;
			private string _taxCalcMode;

			public CalcTaxable(bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType)
			{
				_aSalesOrPurchaseSwitch = aSalesOrPurchaseSwitch;
				_enforcing = enforceType;
			}

			public CalcTaxable(bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType, string taxCalcMode)
				: this(aSalesOrPurchaseSwitch, enforceType)
			{
				_taxCalcMode = taxCalcMode;
			}

			public CalcTaxable(bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType, int precision)
			{
				_aSalesOrPurchaseSwitch = aSalesOrPurchaseSwitch;
				_enforcing = enforceType;
				_precision = precision;
			}

			public Decimal CalcTaxableFromTotalAmount(
				PXCache cache,
				object row,
				HashSet<string> taxList,
				DateTime aDocDate,
				Decimal aCuryTotal)
			{
				IComparer<Tax> taxComparer = GetTaxComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Decimal result = Decimal.Zero;
				PXGraph graph = cache.Graph;
				Dictionary<string, PXResult<Tax, TaxRev>> taxRates = GetTaxRevisionList(graph, aDocDate);
				List<PXResult<Tax, TaxRev>> orderedTaxList = new List<PXResult<Tax, TaxRev>>(taxList.Count);

				foreach (string taxID in taxList)
				{
					if (taxRates.TryGetValue(taxID, out PXResult<Tax, TaxRev> line))
					{
						int idx;
						for (idx = orderedTaxList.Count;
							(idx > 0) && taxComparer.Compare(orderedTaxList[idx - 1], line) > 0;
							idx--)
							;

						var tax = (Tax)line;
						if (_taxCalcMode != null && tax.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
						{
							switch (_taxCalcMode)
							{
								case TaxCalculationMode.Net:
									tax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
									break;

								case TaxCalculationMode.Gross:
									tax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
									break;
							}
						}
						orderedTaxList.Insert(idx, new PXResult<Tax, TaxRev>(tax, (TaxRev)line));
					}
				}

				Decimal rateInclusive = Decimal.Zero;
				Decimal rateLvl1 = Decimal.Zero;
				Decimal rateLvl2 = Decimal.Zero;

				foreach (PXResult<Tax, TaxRev> iRes in orderedTaxList)
				{
					Tax tax = iRes;
					TaxRev taxRev = iRes;
					Decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;

					if (tax.TaxType == CSTaxType.PerUnit)
					{
						PXTrace.WriteError(Messages.PerUnitTaxesNotSupportedOperation);
						throw new PXException(Messages.PerUnitTaxesNotSupportedOperation);
					}

					switch (tax.TaxCalcLevel)
					{
						case CSTaxCalcLevel.Inclusive:
							rateInclusive += multiplier * taxRev.TaxRate.Value;
							break;
						case CSTaxCalcLevel.CalcOnItemAmt:
							rateLvl1 += multiplier * taxRev.TaxRate.Value;
							break;
						case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
							rateLvl2 += multiplier * taxRev.TaxRate.Value;
							break;
					}
				}

				decimal baseLvl2 = PXDBCurrencyAttribute.RoundCury(cache, row, aCuryTotal / (1 + rateLvl2 / 100), _precision);
				decimal baseLvl1 = PXDBCurrencyAttribute.RoundCury(cache, row, baseLvl2 / (1 + (rateLvl1 + rateInclusive) / 100), _precision);
				Decimal curyTaxTotal = decimal.Zero;
				Decimal curyTax2Total = decimal.Zero;

				foreach (PXResult<Tax, TaxRev> iRes in orderedTaxList)
				{
					Tax tax = iRes;
					TaxRev taxRev = iRes;
					Decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
					switch (tax.TaxCalcLevel)
					{
						case CSTaxCalcLevel.Inclusive:
							break;
						case CSTaxCalcLevel.CalcOnItemAmt:
							curyTaxTotal += multiplier * PXDBCurrencyAttribute.RoundCury(cache, row, (baseLvl1 * taxRev.TaxRate / 100m) ?? 0m, _precision);
							break;
						case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
							curyTax2Total += multiplier * PXDBCurrencyAttribute.RoundCury(cache, row, (baseLvl2 * taxRev.TaxRate / 100m) ?? 0m, _precision);
							break;
					}
				}
				result = aCuryTotal - curyTaxTotal - curyTax2Total;
				return result;
			}

			public Decimal CalcTaxableFromTotalAmount(
				PXCache cache,
				object row,
				string aTaxZoneID,
				string aTaxCategoryID,
				DateTime aDocDate,
				Decimal aCuryTotal)
			{
				PXGraph graph = cache.Graph;
				HashSet<string> taxList = GetApplicableTaxList(graph, aTaxZoneID, aTaxCategoryID, aDocDate);
				return CalcTaxableFromTotalAmount(cache, row, taxList, aDocDate, aCuryTotal);
			}

			private HashSet<string> GetApplicableTaxList(PXGraph aGraph, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate)
			{
				HashSet<string> collected = new HashSet<string>();
				foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet> r in PXSelectJoin<TaxZoneDet,
					CrossJoin<TaxCategory,
					InnerJoin<TaxRev, On<TaxRev.taxID, Equal<TaxZoneDet.taxID>>,
					LeftJoin<TaxCategoryDet, On<TaxCategoryDet.taxID, Equal<TaxZoneDet.taxID>,
						And<TaxCategoryDet.taxCategoryID, Equal<TaxCategory.taxCategoryID>>>>>>,
					Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>,
						And<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>,
						And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>,
						And<Where<TaxCategory.taxCatFlag, Equal<False>, And<TaxCategoryDet.taxCategoryID, IsNotNull,
							Or<TaxCategory.taxCatFlag, Equal<True>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(aGraph, aTaxZoneID, aTaxCategoryID, aDocDate))
				{
					TaxZoneDet tzd = ((TaxZoneDet)r);
					if (collected.Contains(tzd.TaxID))
					{
						continue;
					}
					else
					{
						collected.Add(tzd.TaxID);
					}
				}
				return collected;
			}

			private Dictionary<string, PXResult<Tax, TaxRev>> GetTaxRevisionList(PXGraph aGraph, DateTime aDocDate)
			{
				PXSelectBase<Tax> taxRevSelect = null;
				if (_aSalesOrPurchaseSwitch == true)
				{
					taxRevSelect = new PXSelectReadonly2<Tax,
					InnerJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
						And<TaxRev.outdated, Equal<False>,
						And<TaxRev.taxType, Equal<TaxType.sales>,
						And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.taxType, NotEqual<CSTaxType.use>,
						And<Tax.reverseTax, Equal<False>,
						And<Tax.directTax, Equal<False>,
						And<Current<GLTranDoc.tranDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>>>(aGraph);
				}
				else
				{
					taxRevSelect =
						new PXSelectReadonly2<Tax,
								InnerJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
									And<TaxRev.outdated, Equal<False>,
									And<Tax.directTax, Equal<False>,
									And2<
										Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<False>,
											Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<True>,
											Or<Tax.taxType, Equal<CSTaxType.use>,
											Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>,
										And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>(aGraph);
				}
				Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
				foreach (PXResult<Tax, TaxRev> record in taxRevSelect.Select(aDocDate))
				{
					tail[((Tax)record).TaxID] = record;
					Tax tax = (Tax)record;
					if (tax.TaxCalcType == CSTaxCalcType.Item)
					{
						switch (_enforcing)
						{
							case TaxCalcLevelEnforcing.EnforceCalcOnItemAmount:
								if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
								{
									tax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
								}
								break;
							case TaxCalcLevelEnforcing.EnforceInclusive:
								if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt)
								{
									tax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
								}
								break;
							case TaxCalcLevelEnforcing.None:
								break;
						}
					}
				}
				return tail;
			}

			protected virtual IComparer<Tax> GetTaxComparer() => TaxByCalculationLevelComparer.Instance;
		}

		public static decimal CalcResidualAmt(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate,
			string TaxCalcMode, decimal ControlTotalAmt, decimal LinesTotal, decimal TaxTotal)
		{
			decimal taxableAmount = 0.0m;
			TaxZone zone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.SelectWindowed(cache.Graph, 0, 1, aTaxZoneID);
			if (PXAccess.FeatureInstalled<FeaturesSet.manualVATEntryMode>() && zone != null && zone.IsManualVATZone == true)
			{
				taxableAmount = ControlTotalAmt - LinesTotal - TaxTotal;
			}
			else
			{
				switch (TaxCalcMode)
				{
					case TaxCalculationMode.Gross:
						taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID,
							aTaxCategoryID, aDocDate, ControlTotalAmt - LinesTotal, false, GLTaxAttribute.TaxCalcLevelEnforcing.EnforceInclusive);
						break;
					case TaxCalculationMode.Net:
						taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID,
							aTaxCategoryID, aDocDate, ControlTotalAmt - LinesTotal - TaxTotal, false, GLTaxAttribute.TaxCalcLevelEnforcing.EnforceCalcOnItemAmount);
						break;
					case TaxCalculationMode.TaxSetting:
						//disabled for now
						//taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID,
						//	aTaxCategoryID, aDocDate, ControlTotalAmt - LinesTotal, false, GLTaxAttribute.TaxCalcLevelEnforcing.None);
						break;
				}
			}
			return taxableAmount;
		}
		protected decimal CalculateTaxDiscrepancy(PXCache sender, object row)
		{
			decimal discrepancy = 0m;
			SelectTaxes(sender, row, PXTaxCheck.RecalcTotals).
				FindAll(taxrow =>
				{
					Tax tax = (Tax)((PXResult)taxrow)[1];
					return tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive;
				}).
				ForEach(taxrow =>
				{
					TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
					TaxDetail taxSum = CalculateTaxSum(sender, taxrow, row);
					if (taxSum != null)
					{
						PXCache sumcache = sender.Graph.Caches[_TaxSumType];
						discrepancy += (decimal)sumcache.GetValue(taxdet, _CuryTaxAmt) + taxdet.CuryExpenseAmt.Value - (decimal)sumcache.GetValue(taxSum, _CuryTaxAmt) - taxSum.CuryExpenseAmt.Value;
					}
				});
			return discrepancy;
		}

		private void ResetDiscrepancy(PXGraph graph)
		{
			ParentSetValue(graph, _CuryTaxRoundDiff, 0m);
			ParentSetValue(graph, _TaxRoundDiff, 0m);
		}
	}
}
