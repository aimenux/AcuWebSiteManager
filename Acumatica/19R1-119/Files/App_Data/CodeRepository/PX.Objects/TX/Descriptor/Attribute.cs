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

namespace PX.Objects.TX
{
	public class TaxParentAttribute : PXParentAttribute
	{
		public TaxParentAttribute(Type selectParent)
			: base(selectParent)
		{
			throw new PXArgumentException();
		}

		public static void NewChild(PXCache cache, object parentrow, Type ParentType, out object child)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					Type childType = cache.GetItemType();

					PXView parentView = ((PXParentAttribute)attr).GetParentSelect(cache);
					Type parentType = parentView.BqlSelect.GetFirstTable();

					PXView childView = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					BqlCommand selectChild = childView.BqlSelect;

					IBqlParameter[] pars = selectChild.GetParameters();
					Type[] refs = selectChild.GetReferencedFields(false);

					child = Activator.CreateInstance(childType);
					PXCache parentcache = cache.Graph.Caches[parentType];

					for (int i = 0; i < Math.Min(pars.Length, refs.Length); i++)
					{
						Type partype = pars[i].GetReferencedType();
						object val = parentcache.GetValue(parentrow, partype.Name);

						cache.SetValue(child, refs[i].Name, val);
					}
					return;
				}
			}
			child = null;
		}

		public static object ParentSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView parentview = ((PXParentAttribute)attr).GetParentSelect(cache);
					return parentview.SelectSingleBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute)
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

		public static List<object> ChildSelect(PXCache cache, object row, Type ParentType)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXParentAttribute && ((PXParentAttribute)attr).ParentType.IsAssignableFrom(ParentType))
				{
					PXView view = ((PXParentAttribute)attr).GetChildrenSelect(cache);
					return view.SelectMultiBound(new object[] { row });
				}
			}
			return null;
		}

	}

	public enum PXTaxCheck
	{
		Line,
		RecalcLine,
		RecalcTotals,
	}

	public enum TaxCalc
	{
		NoCalc,
		Calc,
		ManualCalc,
		ManualLineCalc
	}

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
					Where <Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcTotals, taxdetOrig.TaxID))
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
			PXRowDeleting del = delegate(PXCache _sender, PXRowDeletingEventArgs _e) { nomatch |= object.ReferenceEquals(row, _e.Row); };
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

			public CalcTaxable(bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType)
			{
				_aSalesOrPurchaseSwitch = aSalesOrPurchaseSwitch;
				_enforcing = enforceType;
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
				Decimal result = Decimal.Zero;
				PXGraph graph = cache.Graph;
				Dictionary<string, PXResult<Tax, TaxRev>> taxRates = GetTaxRevisionList(graph, aDocDate);
				List<PXResult<Tax, TaxRev>> orderedTaxList = new List<PXResult<Tax, TaxRev>>(taxList.Count);
				foreach (string taxID in taxList)
				{
					PXResult<Tax, TaxRev> line;
					if (taxRates.TryGetValue(taxID, out line))
					{
						int idx;
						for (idx = orderedTaxList.Count;
							(idx > 0)
							&& String.Compare(((Tax)orderedTaxList[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
							idx--) ;
						orderedTaxList.Insert(idx, new PXResult<Tax, TaxRev>((Tax)line, (TaxRev)line));
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
					taxRevSelect = new PXSelectReadonly2<Tax,
												InnerJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
													And<TaxRev.outdated, Equal<False>,
													And<Tax.directTax, Equal<False>,
													And2<Where<TaxRev.taxType, Equal<TaxType.purchase>, And<Tax.reverseTax, Equal<False>,
														Or<TaxRev.taxType, Equal<TaxType.sales>, And<Where<Tax.reverseTax, Equal<True>,
														Or<Tax.taxType, Equal<CSTaxType.use>, Or<Tax.taxType, Equal<CSTaxType.withholding>>>>>>>>,
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

	public abstract class TaxBaseAttribute : PXAggregateAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowDeletedSubscriber, IPXRowPersistedSubscriber, IComparable
	{
		protected string _CuryOrigTaxableAmt = "CuryOrigTaxableAmt";
		protected string _CuryTaxAmt = "CuryTaxAmt";
		protected string _CuryTaxDiscountAmt = "CuryTaxDiscountAmt";
		protected string _CuryTaxableAmt = "CuryTaxableAmt";
		protected string _CuryExemptedAmt = "CuryExemptedAmt";
		protected string _CuryTaxableDiscountAmt = "CuryTaxableDiscountAmt";
		protected string _CuryExpenseAmt = "CuryExpenseAmt";
		protected string _CuryRateTypeID = "CuryRateTypeID";
		protected string _CuryEffDate = "CuryEffDate";
		protected string _CuryRate = "SampleCuryRate";
		protected string _RecipRate = "SampleRecipRate";
		protected string _IsTaxSaved = "IsTaxSaved";
		protected string _RecordID = "RecordID";

		protected Type _ParentType;
		protected Type _ChildType;
		protected Type _TaxType;
		protected Type _TaxSumType;
		protected Type _CuryKeyField = null;

		protected Dictionary<object, object> inserted = null;
		protected Dictionary<object, object> updated = null;

		#region TaxID
		protected string _TaxID = "TaxID";
		public Type TaxID
		{
			set
			{
				_TaxID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxCategoryID
		protected string _TaxCategoryID = "TaxCategoryID";
		public Type TaxCategoryID
		{
			set
			{
				_TaxCategoryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region TaxZoneID
		protected string _TaxZoneID = "TaxZoneID";
		public Type TaxZoneID
		{
			set
			{
				_TaxZoneID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region DocDate
		protected string _DocDate = "DocDate";
		public Type DocDate
		{
			set
			{
				_DocDate = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
        public Type ParentBranchIDField { get; set; }
		#region FinPeriodID
		protected string _FinPeriodID = "FinPeriodID";
		public Type FinPeriodID
		{
			set
			{
				_FinPeriodID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTranAmt
		protected abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }
		protected Type CuryTranAmt = typeof(curyTranAmt);
		protected string _CuryTranAmt
		{
			get
			{
				return CuryTranAmt.Name;
			}
		}
		#endregion
		#region OrigGroupDiscountRate
		protected abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		protected Type OrigGroupDiscountRate = typeof(origGroupDiscountRate);
		protected string _OrigGroupDiscountRate
		{
			get
			{
				return OrigGroupDiscountRate.Name;
			}
		}
		#endregion
		#region OrigDocumentDiscountRate
		protected abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		protected Type OrigDocumentDiscountRate = typeof(origDocumentDiscountRate);
		protected string _OrigDocumentDiscountRate
		{
			get
			{
				return OrigDocumentDiscountRate.Name;
			}
		}
		#endregion
		#region GroupDiscountRate
		protected abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
		protected Type GroupDiscountRate = typeof(groupDiscountRate);
		protected string _GroupDiscountRate
		{
			get
			{
				return GroupDiscountRate.Name;
			}
		}
		#endregion
		#region DocumentDiscountRate
		protected abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
		protected Type DocumentDiscountRate = typeof(documentDiscountRate);
		protected string _DocumentDiscountRate
		{
			get
			{
				return DocumentDiscountRate.Name;
			}
		}
		#endregion
		#region TermsID
		protected string _TermsID = "TermsID";
		public Type TermsID
		{
			set
			{
				_TermsID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryID
		protected string _CuryID = "CuryID";
		public Type CuryID
		{
			set
			{
				_CuryID = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryDocBal
		protected string _CuryDocBal = "CuryDocBal";
		public Type CuryDocBal
		{
			set
			{
				_CuryDocBal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxDiscountTotal
		protected string _CuryTaxDiscountTotal = "CuryOrigTaxDiscAmt";
		public Type CuryDocBalUndiscounted
		{
			set
			{
				_CuryTaxDiscountTotal = (value != null) ? value.Name : null;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryTaxTotal
		protected string _CuryTaxTotal = "CuryTaxTotal";
		public Type CuryTaxTotal
		{
			set
			{
				_CuryTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryOrigDiscAmt
		protected string _CuryOrigDiscAmt = "CuryOrigDiscAmt";
		public Type CuryOrigDiscAmt
		{
			set
			{
				_CuryOrigDiscAmt = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryWhTaxTotal
		protected string _CuryWhTaxTotal = "CuryOrigWhTaxAmt";
		public Type CuryWhTaxTotal
		{
			set
			{
				_CuryWhTaxTotal = value.Name;
			}
			get
			{
				return null;
			}
		}
		#endregion
		#region CuryLineTotal
		protected abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
		public Type CuryLineTotal = typeof(curyLineTotal);
		protected string _CuryLineTotal
		{
			get
			{
				return CuryLineTotal.Name;
			}
		}
		#endregion
		#region CuryDiscTot
		protected abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
		protected Type CuryDiscTot = typeof(curyDiscTot);
		protected string _CuryDiscTot
		{
			get
			{
				return CuryDiscTot.Name;
			}
		}
		#endregion
		#region TaxCalc
		protected TaxCalc _TaxCalc = TaxCalc.Calc;
		public TaxCalc TaxCalc
		{
			set
			{
				_TaxCalc = value;
			}
			get
			{
				return _TaxCalc;
			}
		}
		#endregion
		#region TaxCalcMode
		protected string _TaxCalcMode = null;
		public Type TaxCalcMode
		{
			set
			{
				_TaxCalcMode = value.Name;
			}
			get
			{
				return null;
			}
		}
		protected bool _isTaxCalcModeEnabled
		{
			get { return !String.IsNullOrEmpty(_TaxCalcMode) && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>(); }
		}

		#endregion
		#region
		public int? Precision { get; set; }
		#endregion

	    public Type ChildBranchIDField { get; set; }
        public Type ChildFinPeriodIDField { get; set; }

		protected bool _NoSumTaxable = false;

		public static List<PXEventSubscriberAttribute> GetAttributes<Field, Target>(PXCache sender, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool exactfind = false;

			var res = new List<PXEventSubscriberAttribute>();
			var q = sender.GetAttributes<Field>(data).Where(
				(attr) => (!exactfind || data == null) && ((exactfind = attr.GetType() == typeof(Target))
				|| attr is TaxAttribute && typeof(Target) == typeof(TaxAttribute)));

			foreach (var a in q)
			{
				a.IsDirty = true;
				res.Add(a);
			}


			res.Sort((a, b) => ((IComparable)a).CompareTo(b));

			return res;
		}

		public static void SetTaxCalc<Field>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
		{
			SetTaxCalc<Field, TaxAttribute>(cache, data, isTaxCalc);
		}

		public static void SetTaxCalc<Field, Target>(PXCache cache, object data, TaxCalc isTaxCalc)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				((TaxAttribute)attr).TaxCalc = isTaxCalc;
			}
		}

		public static TaxCalc GetTaxCalc<Field>(PXCache cache, object data)
			where Field : IBqlField
		{
			return GetTaxCalc<Field, TaxAttribute>(cache, data);
		}

		public static TaxCalc GetTaxCalc<Field, Target>(PXCache cache, object data)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			if (data == null)
			{
				cache.SetAltered<Field>(true);
			}
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(cache, data))
			{
				if (((TaxAttribute)attr).TaxCalc != TaxCalc.NoCalc)
				{
					return TaxCalc.Calc;
				}
			}
			return TaxCalc.NoCalc;
		}

		public virtual object Insert(PXCache cache, object item)
		{
			return cache.Insert(item);
		}

		public virtual object Delete(PXCache cache, object item)
		{
			return cache.Delete(item);
		}

		public static void Calculate<Field>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowInsertedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowInsertedSubscriber)attr).RowInserted(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).inserted.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).inserted.Remove(e.Row);

						if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
						{
							((TaxAttribute)attr).updated.Remove(e.Row);
						}
					}
				}
			}

			if (!isCalcedByAttribute)
			{
				var tgraph = sender.Graph.GetType();
				var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
				var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

				if (ext == null) return;

				var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
				method?.Invoke(ext, null);
			}
		}

		public static void Calculate<Field>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
		{
			Calculate<Field, TaxAttribute>(sender, e);
		}

		public static void Calculate<Field, Target>(PXCache sender, PXRowUpdatedEventArgs e)
			where Field : IBqlField
			where Target : TaxAttribute
		{
			bool isCalcedByAttribute = false;
			foreach (PXEventSubscriberAttribute attr in GetAttributes<Field, Target>(sender, e.Row))
			{
				isCalcedByAttribute = true;

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualLineCalc)
				{
					((TaxAttribute)attr).TaxCalc = TaxCalc.Calc;

					try
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, e);
					}
					finally
					{
						((TaxAttribute)attr).TaxCalc = TaxCalc.ManualLineCalc;
					}
				}

				if (((TaxAttribute)attr).TaxCalc == TaxCalc.ManualCalc)
				{
					object copy;
					if (((TaxAttribute)attr).updated.TryGetValue(e.Row, out copy))
					{
						((IPXRowUpdatedSubscriber)attr).RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
						((TaxAttribute)attr).updated.Remove(e.Row);
					}
				}
			}

			if (!isCalcedByAttribute)
			{
				var tgraph = sender.Graph.GetType();
				var extensions = tgraph.GetField("Extensions", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(sender.Graph) as PXGraphExtension[];
				var ext = extensions?.FirstOrDefault(extension => IsInstanceOfGenericType(typeof(Extensions.SalesTax.TaxBaseGraph<,>), extension));

				if (ext == null) return;

				var method = ext.GetType().GetMethod("RecalcTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
				method?.Invoke(ext, null);
			}
		}
		
		private static bool IsInstanceOfGenericType(Type genericType, object instance)
		{
			Type type = instance.GetType();
			while (type != null)
			{
				if (type.IsGenericType &&
					type.GetGenericTypeDefinition() == genericType)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		protected virtual string GetTaxZone(PXCache sender, object row)
		{
			return (string)ParentGetValue(sender.Graph, _TaxZoneID);
		}

		protected virtual DateTime? GetDocDate(PXCache sender, object row)
		{
			return (DateTime?)ParentGetValue(sender.Graph, _DocDate);
		}

		protected virtual string GetTaxCategory(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxCategoryID);
		}

		protected virtual decimal? GetCuryTranAmt(PXCache sender, object row, string TaxCalcType = "I")
		{
			return (decimal?)sender.GetValue(row, _CuryTranAmt);
		}

		protected virtual string GetTaxID(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, _TaxID);
		}

		protected virtual object InitializeTaxDet(object data)
		{
			return data;
		}

		protected virtual void AddOneTax(PXCache cache, object detrow, ITaxDetail taxitem)
		{
			if (taxitem != null)
			{
				object newdet;
				TaxParentAttribute.NewChild(cache, detrow, _ChildType, out newdet);
				((ITaxDetail)newdet).TaxID = taxitem.TaxID;
				newdet = InitializeTaxDet(newdet);
				object insdet = Insert(cache, newdet);

				if (insdet != null) PXParentAttribute.SetParent(cache, insdet, _ChildType, detrow);
			}
		}

		public virtual ITaxDetail MatchesCategory(PXCache sender, object row, ITaxDetail zoneitem)
		{
			string taxcat = GetTaxCategory(sender, row);
			string taxid = GetTaxID(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			TaxRev rev = PXSelect<TaxRev, Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>, And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>>>>>.Select(sender.Graph, zoneitem.TaxID, docdate);

			if (rev == null)
			{
				return null;
			}

			if (string.Equals(taxid, zoneitem.TaxID))
			{
				return zoneitem;
			}

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return null;
			}
			else
			{
				return MatchesCategory(sender, row, new ITaxDetail[] { zoneitem }).FirstOrDefault();
			}

		}

		public virtual IEnumerable<ITaxDetail> MatchesCategory(PXCache sender, object row, IEnumerable<ITaxDetail> zonetaxlist)
		{
			string taxcat = GetTaxCategory(sender, row);

			List<ITaxDetail> ret = new List<ITaxDetail>();

			TaxCategory cat = (TaxCategory)PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(sender.Graph, taxcat);

			if (cat == null)
			{
				return ret;
			}

			HashSet<string> cattaxlist = new HashSet<string>();
			foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(sender.Graph, taxcat))
			{
				cattaxlist.Add(detail.TaxID);
			}

			foreach (ITaxDetail zoneitem in zonetaxlist)
			{
				bool zonematchestaxcat = cattaxlist.Contains(zoneitem.TaxID);
				if (cat.TaxCatFlag == false && zonematchestaxcat || cat.TaxCatFlag == true && !zonematchestaxcat)
				{
					ret.Add(zoneitem);
				}
			}

			return ret;
		}


		protected abstract IEnumerable<ITaxDetail> ManualTaxes(PXCache sender, object row);

		protected virtual void DefaultTaxes(PXCache sender, object row, bool DefaultExisting)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			string taxzone = GetTaxZone(sender, row);
			string taxcat = GetTaxCategory(sender, row);
			DateTime? docdate = GetDocDate(sender, row);

			var applicableTaxes = new HashSet<string>();

			foreach (PXResult<TaxZoneDet, TaxCategory, TaxRev, TaxCategoryDet> r in PXSelectJoin<TaxZoneDet,
				CrossJoin<TaxCategory,
				InnerJoin<TaxRev, On<TaxRev.taxID, Equal<TaxZoneDet.taxID>>,
				LeftJoin<TaxCategoryDet, On<TaxCategoryDet.taxID, Equal<TaxZoneDet.taxID>,
					And<TaxCategoryDet.taxCategoryID, Equal<TaxCategory.taxCategoryID>>>>>>,
				Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>,
					And<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>,
					And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>, And<TaxRev.outdated, Equal<False>,
					And<Where<TaxCategory.taxCatFlag, Equal<False>, And<TaxCategoryDet.taxCategoryID, IsNotNull,
						Or<TaxCategory.taxCatFlag, Equal<True>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(sender.Graph, taxzone, taxcat, docdate))
			{
				AddOneTax(cache, row, (TaxZoneDet)r);
				applicableTaxes.Add(((TaxZoneDet)r).TaxID);
			}

			string taxID;
			if ((taxID = GetTaxID(sender, row)) != null)
			{
				AddOneTax(cache, row, new TaxZoneDet() { TaxID = taxID });
				applicableTaxes.Add(taxID);
			}

			foreach (ITaxDetail r in ManualTaxes(sender, row))
			{
				if (applicableTaxes.Contains(r.TaxID))
					applicableTaxes.Remove(r.TaxID);
			}

			foreach (string applicableTax in applicableTaxes)
			{
				AddTaxTotals(cache, applicableTax, row);
			}

			if (DefaultExisting)
			{
				foreach (ITaxDetail r in MatchesCategory(sender, row, ManualTaxes(sender, row)))
				{
					AddOneTax(cache, row, r);
				}
			}
		}

		protected virtual void DefaultTaxes(PXCache sender, object row)
		{
			DefaultTaxes(sender, row, true);
		}

		private Type GetFieldType(PXCache cache, string FieldName)
		{
			List<Type> fields = cache.BqlFields;
			for (int i = 0; i < fields.Count; i++)
			{
				if (String.Compare(fields[i].Name, FieldName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return fields[i];
				}
			}
			return null;
		}

		private Type GetTaxIDType(PXCache cache)
		{
			foreach (PXEventSubscriberAttribute attr in cache.GetAttributes(null))
			{
				if (attr is PXSelectorAttribute)
				{
					if (((PXSelectorAttribute)attr).Field == typeof(Tax.taxID))
					{
						return GetFieldType(cache, ((PXSelectorAttribute)attr).FieldName);
					}
				}
			}
			return null;
		}

		private Type AddWhere(Type command, Type where)
		{
			if (command.IsGenericType)
			{
				Type[] args = command.GetGenericArguments();
				Type[] pars = new Type[args.Length + 1];
				pars[0] = command.GetGenericTypeDefinition();
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].IsGenericType && (
						args[i].GetGenericTypeDefinition() == typeof(Where<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where2<,>) ||
						args[i].GetGenericTypeDefinition() == typeof(Where<,,>)))
					{
						pars[i + 1] = typeof(Where2<,>).MakeGenericType(args[i], typeof(And<>).MakeGenericType(where));
					}
					else
					{
						pars[i + 1] = args[i];
					}
				}
				return BqlCommand.Compose(pars);
			}
			return null;
		}

		protected List<object> SelectTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			return SelectTaxes<Where<True, Equal<True>>>(sender.Graph, row, taxchk);
		}

		protected abstract List<Object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
			where Where : IBqlWhere, new();

		protected Tax AdjustTaxLevel(PXGraph graph, Tax taxToAdjust)
		{
			if (_isTaxCalcModeEnabled && taxToAdjust.TaxCalcType == CSTaxCalcType.Item && taxToAdjust.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
			{
				string TaxCalcMode = GetTaxCalcMode(graph);
				if (!String.IsNullOrEmpty(TaxCalcMode))
				{
					Tax adjdTax = (Tax)graph.Caches[typeof(Tax)].CreateCopy(taxToAdjust);
					switch (TaxCalcMode)
					{
						case TaxCalculationMode.Gross:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
							break;
						case TaxCalculationMode.Net:
							adjdTax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
							break;
						case TaxCalculationMode.TaxSetting:
							break;
					}
					return adjdTax;
				}
			}
			return taxToAdjust;
		}

		protected virtual void ClearTaxes(PXCache sender, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];
			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.Line))
			{
				Delete(cache, ((PXResult)taxrow)[0]);
			}
		}

		private decimal Sum(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			if (field != null)
			{
				list.ForEach(new Action<Object>(delegate (object a)
				{
					decimal? val = (decimal?)graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult)a)[BqlCommand.GetItemType(field)], field.Name);
					ret += (val ?? 0m);
				}
				));
			}

			return ret;
		}

		protected virtual void AddTaxTotals(PXCache sender, string taxID, object row)
		{
			PXCache cache = sender.Graph.Caches[_TaxSumType];

			object newdet = Activator.CreateInstance(_TaxSumType);
			((TaxDetail)newdet).TaxID = taxID;
			newdet = InitializeTaxDet(newdet);
			object insdet = Insert(cache, newdet);
		}

		protected Terms SelectTerms(PXGraph graph)
		{
			string TermsID = (string)ParentGetValue(graph, _TermsID);
			Terms ret = TermsAttribute.SelectTerms(graph, TermsID);
			ret = ret ?? new Terms();

			return ret;
		}

		protected virtual void SetTaxableAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual void SetTaxAmt(PXCache sender, object row, decimal? value)
		{
		}

		protected virtual bool IsDeductibleVATTax(Tax tax)
		{
			return tax?.DeductibleVAT == true;
		}

		protected virtual bool IsExemptTaxCategory(PXGraph graph, object row)
		{
			PXCache sender = graph.Caches[_ChildType];
			return IsExemptTaxCategory(sender, row);
		}

		protected virtual bool IsExemptTaxCategory(PXCache sender, object row)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.exemptedTaxReporting>() != true)
				return false;

			bool isExemptTaxCategory = false;
			string taxCategory = GetTaxCategory(sender, row);

			if (!string.IsNullOrEmpty(taxCategory))
			{
				TaxCategory category = (TaxCategory)PXSelect<
					TaxCategory,
					Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>
					.Select(sender.Graph, taxCategory);

				isExemptTaxCategory = category?.Exempt == true;
			}

			return isExemptTaxCategory;
		}

		protected abstract decimal? GetTaxableAmt(PXCache sender, object row);

		protected abstract decimal? GetTaxAmt(PXCache sender, object row);

		protected List<object> SelectInclusiveTaxes(PXGraph graph, object row)
		{
			List<object> res = new List<object>();

			if (IsExemptTaxCategory(graph, row))
			{
				return res;
			}

			if (!_isTaxCalcModeEnabled || GetTaxCalcMode(graph) == TaxCalculationMode.TaxSetting)
			{
				res = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
					And<Tax.taxType, NotEqual<CSTaxType.withholding>,
						And<Tax.directTax, Equal<False>>>>>(graph, row, PXTaxCheck.Line);
			}
			else
			{
				string CalcMode = GetTaxCalcMode(graph);
				if (CalcMode == TaxCalculationMode.Gross)
				{
					res = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
						And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
								And<Tax.directTax, Equal<False>>>>>>(graph, row, PXTaxCheck.Line);
				}
				res.AddRange(SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
						And<Tax.taxCalcType, Equal<CSTaxCalcType.doc>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
								And<Tax.directTax, Equal<False>>>>>>(graph, row, PXTaxCheck.Line));
			}
			return res;
		}

		protected List<object> SelectLvl1Taxes(PXGraph graph, object row)
		{
			return 
				IsExemptTaxCategory(graph, row) 
					? new List<object>()
					: SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>,
				And<Tax.taxCalcLevel2Exclude, Equal<False>>>>(graph, row, PXTaxCheck.Line);
		}

		protected virtual void TaxSetLineDefault(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException(nameof(taxrow), ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

			decimal CuryTranAmt = (decimal)GetCuryTranAmt(sender, row, tax.TaxCalcType);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			Terms terms = SelectTerms(sender.Graph);
			List<object> incltaxes = SelectInclusiveTaxes(sender.Graph, row);

			decimal InclRate = SumWithReverseAdjustment(sender.Graph,
				incltaxes,
				typeof(TaxRev.taxRate));

			decimal CuryInclTaxAmt = SumWithReverseAdjustment(sender.Graph,
				incltaxes,
				GetFieldType(cache, _CuryTaxAmt));

			decimal CuryInclTaxDiscountAmt = 0m;
			Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
			if (curyTaxDiscountAmtField != null)
			{
				CuryInclTaxDiscountAmt = SumWithReverseAdjustment(sender.Graph,	incltaxes, curyTaxDiscountAmtField);
			}

			decimal CuryTaxableAmt = 0.0m;
			decimal CuryTaxableDiscountAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;
			decimal CuryTaxDiscountAmt = 0.0m;			

			decimal? DiscPercent = null;
			DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out DiscPercent);

			decimal CalculatedTaxRate = (decimal)taxrev.TaxRate / 100;
			decimal UndiscountedPercent = 1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100;

			switch (tax.TaxCalcLevel)
			{
				case CSTaxCalcLevel.Inclusive:
					CuryTaxableAmt = CuryTranAmt / (1 + InclRate / 100);
					CuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableAmt * CalculatedTaxRate, Precision);
					CuryInclTaxAmt = 0m;

					incltaxes.ForEach(delegate(object a)
					{
						decimal? TaxRate = (decimal?)sender.Graph.Caches[typeof(TaxRev)].GetValue<TaxRev.taxRate>(((PXResult)a)[typeof(TaxRev)]);
						decimal multiplier = ((Tax)(((PXResult)a)[typeof(Tax)])).ReverseTax == true ? Decimal.MinusOne : Decimal.One;
						CuryInclTaxAmt += PXDBCurrencyAttribute.RoundCury(cache, taxdet, (CuryTaxableAmt * TaxRate / 100m) ?? 0m, Precision) * multiplier;
					});

					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt;
					SetTaxableAmt(sender, row, CuryTaxableAmt);
					SetTaxAmt(sender, row, CuryInclTaxAmt);
					break;
				case CSTaxCalcLevel.CalcOnItemAmt:
					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt - CuryInclTaxDiscountAmt;
					break;
				case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
					List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

					decimal CuryLevel1TaxAmt = SumWithReverseAdjustment(sender.Graph, lvl1Taxes, GetFieldType(cache, _CuryTaxAmt));

					CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt + CuryLevel1TaxAmt - CuryInclTaxDiscountAmt;
					break;
			}

			if (ConsiderDiscount(tax))
			{
				CuryTaxableAmt *= UndiscountedPercent;
			}
			else if (ConsiderEarlyPaymentDiscountDetail(sender, row, tax))
			{
				CuryTaxableDiscountAmt = CuryTaxableAmt * (1m - UndiscountedPercent);
				CuryTaxableAmt *= UndiscountedPercent;
				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * CalculatedTaxRate;
			}
			else if (ConsiderInclusiveDiscountDetail(sender, row, tax))
			{
				CuryTaxableDiscountAmt = CuryTaxableAmt * (1m - UndiscountedPercent);
				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * CalculatedTaxRate;
				CuryTaxableAmt *= UndiscountedPercent;
				CuryTaxAmt *= UndiscountedPercent;
			}

			if (tax.TaxCalcType == CSTaxCalcType.Item
				&& (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt 
					|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt))
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					cache.SetValue(taxdet, _CuryOrigTaxableAmt, PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableAmt, Precision));
				}

				AdjustMinMaxTaxableAmt(cache, taxdet, taxrev, ref CuryTaxableAmt, ref TaxableAmt);

				CuryTaxAmt = CuryTaxableAmt * CalculatedTaxRate;
				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * CalculatedTaxRate;

				if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
				{
					CuryTaxAmt *= UndiscountedPercent;
				}
			}
			else if (tax.TaxCalcType != CSTaxCalcType.Item)
			{
				CuryTaxAmt = 0.0m;
				CuryTaxDiscountAmt = 0.0m;
			}

			taxdet.TaxRate = taxrev.TaxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
			SetValueOptional(cache, taxdet, PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			SetValueOptional(cache, taxdet, PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxDiscountAmt), _CuryTaxDiscountAmt);

			decimal roundedCuryTaxableAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxableAmt, Precision);

			bool isExemptTaxCategory = IsExemptTaxCategory(sender, row);
			if (isExemptTaxCategory)
			{
				SetTaxDetailExemptedAmount(cache, taxdet, roundedCuryTaxableAmt);
			}
			else
			{
				SetTaxDetailTaxableAmount(cache, taxdet, roundedCuryTaxableAmt);
			}

			decimal roundedCuryTaxAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxAmt, Precision);

			if (IsDeductibleVATTax(tax))
			{
				taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(cache, taxdet, CuryTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100), Precision);
				CuryTaxAmt = roundedCuryTaxAmt - (decimal)taxdet.CuryExpenseAmt;

				decimal expenseAmt;
				PXDBCurrencyAttribute.CuryConvBase(cache, taxdet, taxdet.CuryExpenseAmt.Value, out expenseAmt);
				taxdet.ExpenseAmt = expenseAmt;
			}
			else
			{
				CuryTaxAmt = roundedCuryTaxAmt;
			}

			if (!isExemptTaxCategory)
			{
				SetTaxDetailTaxAmount(cache, taxdet, CuryTaxAmt);
			}

			if (taxrev.TaxID != null && tax.DirectTax != true)
			{
				cache.Update(taxdet);
				if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive)
				{
					sender.MarkUpdated(row);
				}
			}
			else
			{
				Delete(cache, taxdet);
			}
		}

		protected virtual bool ConsiderDiscount(Tax tax)
		{
			return (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
								|| tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
							&& tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}
		private bool ConsiderEarlyPaymentDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent  = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderEarlyPaymentDiscount(sender, parent, tax);
		}
		private bool ConsiderInclusiveDiscountDetail(PXCache sender, object detail, Tax tax)
		{
			object parent = PXParentAttribute.SelectParent(sender, detail, _ParentType);
			return ConsiderInclusiveDiscount(sender, parent, tax);
		}
		protected virtual bool ConsiderEarlyPaymentDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}
		protected virtual bool ConsiderInclusiveDiscount(PXCache sender, object parent, Tax tax)
		{
			return false;
		}

		protected virtual void SetTaxDetailTaxableAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxableAmt)
		{
			cache.SetValue(taxdet, _CuryTaxableAmt, curyTaxableAmt);
		}

		protected virtual void SetTaxDetailExemptedAmount(PXCache cache, TaxDetail taxdet, decimal? curyExemptedAmt)
		{
			if (!string.IsNullOrEmpty(_CuryExemptedAmt))
			{
				cache.SetValue(taxdet, _CuryExemptedAmt, curyExemptedAmt);
			}
		}

		protected virtual void SetTaxDetailTaxAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxAmt)
		{
			cache.SetValue(taxdet, _CuryTaxAmt, curyTaxAmt);
		}

		#region CiryOrigDiscAmt TaxRecalculation

		public static Pair<double, double> SolveQuadraticEquation(double a, double b, double c)
		{
			double x1, x2;
			Pair<double, double> result = null;

			double d = b * b - 4 * a * c;
			if (d == 0)
			{
				x1 = x2 = -(b / 2 * a);
				result = new Pair<double, double>(x1, x2);
			}
			else if (d > 0)
			{
				double sqrtD = Math.Sqrt(d);
				x1 = (-b + sqrtD) / (2 * a);
				x2 = (-b - sqrtD) / (2 * a);
				result = new Pair<double, double>(x1, x2);
			}

			return result;
		}

		private Dictionary<object, bool> OrigDiscAmtExtCallDict = new Dictionary<object, bool>();
		private Dictionary<object, decimal?> DiscPercentsDict = new Dictionary<object, decimal?>();
		
		protected virtual void CuryOrigDiscAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

			OrigDiscAmtExtCallDict[e.Row] = e.ExternalCall;
		}

	    protected virtual bool ShouldUpdateFinPeriodID(PXCache sender, object oldRow, object newRow)
	    {
	        return (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
	               && (string) sender.GetValue(oldRow, _FinPeriodID) != (string) sender.GetValue(newRow, _FinPeriodID);
	    }

		protected virtual void ParentRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (e.Row == null)
				return;

		    int? oldBranchID = null;
		    int? newBranchID = null;

            if (ParentBranchIDField != null)
		    {
		        oldBranchID = (int?)sender.GetValue(e.OldRow, ParentBranchIDField.Name);
		        newBranchID = (int?)sender.GetValue(e.Row, ParentBranchIDField.Name);
            }

		    if (oldBranchID != newBranchID
		        || ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
            {
		            PXCache cache = sender.Graph.Caches[_TaxSumType];
		            List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, _ParentType);
		            foreach (object det in details)
		            {
		                if (oldBranchID != newBranchID)
		                {
                            cache.SetDefaultExt(det, ChildBranchIDField.Name);
                        }

		                if (ShouldUpdateFinPeriodID(sender, e.OldRow, e.Row))
		                {
		                    cache.SetDefaultExt(det, ChildFinPeriodIDField.Name);
		                }

		                cache.MarkUpdated(det);
                    }
            }

			bool externallCall = false;
			OrigDiscAmtExtCallDict.TryGetValue(e.Row, out externallCall);
			if (!externallCall)
				return;

			decimal newDiscAmt = ((decimal?)sender.GetValue(e.Row, _CuryOrigDiscAmt)).GetValueOrDefault();
			decimal oldDiscAmt = ((decimal?)sender.GetValue(e.OldRow, _CuryOrigDiscAmt)).GetValueOrDefault();

			if (newDiscAmt != oldDiscAmt && !DiscPercentsDict.ContainsKey(e.Row))
			{
				DiscPercentsDict.Add(e.Row, 0m);
				PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(e.Row, oldDiscAmt, false);

				using (new TermsAttribute.UnsubscribeCalcDiscScope(sender))
				{
					try
					{
						if (newDiscAmt == 0m)
							return;

						ParentFieldUpdated(sender, args);
						DiscPercentsDict[e.Row] = null;

						bool considerEarlyPaymentDiscount = false;
						decimal reducedTaxAmt = 0m;
						PXCache cache = sender.Graph.Caches[_TaxSumType];

						foreach (object taxitem in SelectTaxes(sender, e.Row, PXTaxCheck.RecalcTotals))
						{
							object taxsum = ((PXResult)taxitem)[0];
							Tax tax = PXResult.Unwrap<Tax>(taxitem);
							
							if (RecalcTaxableRequired(tax))
							{
								reducedTaxAmt += (tax.ReverseTax == true ? -1m : 1m) * ( ((decimal?)cache.GetValue(taxsum, _CuryTaxAmt)).GetValueOrDefault() + 
									(IsDeductibleVATTax(tax) ? ((decimal?)cache.GetValue(taxsum, _CuryExpenseAmt)).GetValueOrDefault() : 0m));
							}
							else if (ConsiderEarlyPaymentDiscount(sender, e.Row, tax) || ConsiderInclusiveDiscount(sender, e.Row, tax))
							{
								considerEarlyPaymentDiscount = true;
								break; //as combination of reduce taxable and reduce taxable on early payment is forbidde, we can skip further calculation
							}
						}
						if (considerEarlyPaymentDiscount)
						{
							decimal CuryDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							DiscPercentsDict[e.Row] = 100 * newDiscAmt / CuryDocBal;
						}
						else if (reducedTaxAmt != 0m)
						{
							decimal CuryDocBal = ((decimal?)sender.GetValue(e.Row, _CuryDocBal)).GetValueOrDefault();
							{
							Pair<double, double> result = SolveQuadraticEquation((double)reducedTaxAmt, -(double)CuryDocBal, (double)newDiscAmt);

							DiscPercentsDict[e.Row] = result?.first >= 0 && result?.first <= 1 
								? (decimal)Math.Round(result.first * 100, 2)
								: result?.second >= 0 && result?.second <= 1
									? (decimal)Math.Round(result.second * 100, 2)
									: (decimal?)null;
						}
					}
					}
					catch
					{
						DiscPercentsDict[e.Row] = null;
					}
					finally
					{
						ParentFieldUpdated(sender, args);
						sender.RaiseRowUpdated(e.Row, e.OldRow);

						OrigDiscAmtExtCallDict.Remove(e.Row);
						DiscPercentsDict.Remove(e.Row);
					}
				}
			}
		}

		protected virtual bool RecalcTaxableRequired(Tax tax)
		{
			return tax?.TaxCalcLevel != CSTaxCalcLevel.Inclusive &&
											tax?.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount;
		}

		#endregion

		protected virtual void AdjustTaxableAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
		{
		}

		protected virtual void AdjustExemptedAmount(PXCache cache, object row, List<object> taxitems, ref decimal CuryExemptedAmt, string TaxCalcType)
		{
		}

		protected virtual TaxDetail CalculateTaxSum(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache cache = sender.Graph.Caches[_TaxType];
			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
			Tax tax = PXResult.Unwrap<Tax>(taxrow);
			TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

			if (taxrev.TaxID == null)
			{
				taxrev.TaxableMin = 0m;
				taxrev.TaxableMax = 0m;
				taxrev.TaxRate = 0m;
			}

			decimal curyOrigTaxableAmt = 0m;
			decimal CuryTaxableAmt = 0.0m;
			decimal CuryTaxableDiscountAmt = 0.0m;
			decimal TaxableAmt = 0.0m;
			decimal CuryTaxAmt = 0.0m;
			decimal CuryTaxDiscountAmt = 0.0m;
			decimal CuryLevel1TaxAmt = 0.0m;
			decimal CuryExpenseAmt = 0.0m;
			decimal CuryExemptedAmt = 0.0m;

			List<object> taxitems = SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, row, PXTaxCheck.RecalcLine, taxdet.TaxID);

			if (taxitems.Count == 0 || taxrev.TaxID == null)
			{
				return null;
			}

			if (tax.TaxCalcType == CSTaxCalcType.Item)
			{
				if (cache.Fields.Contains(_CuryOrigTaxableAmt))
				{
					curyOrigTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryOrigTaxableAmt));
				}

				CuryTaxableAmt = Sum(sender.Graph,
					taxitems,
					GetFieldType(cache, _CuryTaxableAmt));

				Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
				if (curyTaxableDiscountAmtField != null)
				{
					CuryTaxableDiscountAmt = Sum(sender.Graph,
					taxitems,
					curyTaxableDiscountAmtField);
				}

				AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

				CuryTaxAmt = Sum(sender.Graph, 
					taxitems, 
					GetFieldType(cache, _CuryTaxAmt));

				Type curyTaxDiscountAmtField = GetFieldType(cache, _CuryTaxDiscountAmt);
				if (curyTaxDiscountAmtField != null)
				{
					CuryTaxDiscountAmt = Sum(sender.Graph, taxitems, curyTaxDiscountAmtField);
				}

				CuryExpenseAmt = Sum(sender.Graph, 
					taxitems, 
					GetFieldType(cache, _CuryExpenseAmt));
			}
			else
			{
				List<object> lvl1Taxes = SelectLvl1Taxes(sender.Graph, row);

				if (_NoSumTaxable && (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || lvl1Taxes.Count == 0))
				{
					// When changing doc date will 
					// not recalculate taxable amount
					//
					CuryTaxableAmt = (decimal)sumcache.GetValue(taxdet, _CuryTaxableAmt);
					CuryTaxableDiscountAmt = GetOptionalDecimalValue(sumcache, taxdet, _CuryTaxableDiscountAmt);
				}
				else
				{
					CuryTaxableAmt = Sum(sender.Graph,
						taxitems,
						GetFieldType(cache, _CuryTaxableAmt));

					Type curyTaxableDiscountAmtField = GetFieldType(cache, _CuryTaxableDiscountAmt);
					if (curyTaxableDiscountAmtField != null)
					{
						CuryTaxableDiscountAmt = Sum(sender.Graph, taxitems, curyTaxableDiscountAmtField);
					}

					AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

					if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
					{
						CuryLevel1TaxAmt = Sum(sender.Graph, lvl1Taxes, GetFieldType(sumcache, _CuryTaxAmt));
						CuryTaxableAmt += CuryLevel1TaxAmt;
					}
				}

				curyOrigTaxableAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision);

				AdjustMinMaxTaxableAmt(sumcache, taxdet, taxrev, ref CuryTaxableAmt, ref TaxableAmt);

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;
				CuryTaxDiscountAmt = CuryTaxableDiscountAmt * (decimal)taxrev.TaxRate / 100;

				AdjustExpenseAmt(tax, taxrev, CuryTaxAmt, ref CuryExpenseAmt);
				AdjustTaxAmtOnDiscount(sender, tax, ref CuryTaxAmt);
			}

			taxdet = (TaxDetail)sumcache.CreateCopy(taxdet);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}
			
			CuryExemptedAmt = Sum(sender.Graph,
				taxitems,
				GetFieldType(cache, _CuryExemptedAmt));

			AdjustExemptedAmount(sender, row, taxitems, ref CuryExemptedAmt, tax.TaxCalcType);

			taxdet.TaxRate = taxrev.TaxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryExemptedAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryExemptedAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxAmt, Precision));
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryExpenseAmt, Precision);
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, CuryTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt, 
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) - 
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}

			return taxdet;
		}

		protected virtual void CalculateTaxSumTaxAmt(
			PXCache sender, 
			TaxDetail taxdet, 
			Tax tax, 
			TaxRev taxrev)
		{
			PXCache sumcache = sender.Graph.Caches[_TaxSumType];

			decimal taxableAmt = 0.0m;
			decimal curyExpenseAmt = 0.0m;

			decimal curyTaxableAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableAmt);
			decimal curyTaxableDiscountAmt = GetOptionalDecimalValue(sender, taxdet, _CuryTaxableDiscountAmt);
			decimal curyOrigTaxableAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision);

			decimal taxRate = taxrev.TaxRate??0.0m;

			AdjustMinMaxTaxableAmt(sender, taxdet, taxrev, ref curyTaxableAmt, ref taxableAmt);

			decimal curyTaxAmt = curyTaxableAmt * taxRate / 100;
			decimal curyTaxDiscountAmt = curyTaxableDiscountAmt * taxRate / 100;

			AdjustExpenseAmt(tax, taxrev, curyTaxAmt, ref curyExpenseAmt);
			AdjustTaxAmtOnDiscount(sender, tax, ref curyTaxAmt);

			if (sumcache.Fields.Contains(_CuryOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, _CuryOrigTaxableAmt, curyOrigTaxableAmt);
			}

			taxdet.TaxRate = taxRate;
			taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate??0.0m;
			sumcache.SetValue(taxdet, _CuryTaxableAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableAmt, Precision));
			sumcache.SetValue(taxdet, _CuryTaxAmt, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxAmt, Precision));
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxableDiscountAmt), _CuryTaxableDiscountAmt);
			taxdet.CuryExpenseAmt = PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyExpenseAmt, Precision);
			SetValueOptional(sumcache, taxdet, PXDBCurrencyAttribute.RoundCury(sumcache, taxdet, curyTaxDiscountAmt), _CuryTaxDiscountAmt);

			if (IsDeductibleVATTax(tax) && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, _CuryTaxAmt,
					(decimal)(sumcache.GetValue(taxdet, _CuryTaxAmt) ?? 0m) -
					(decimal)(sumcache.GetValue(taxdet, _CuryExpenseAmt) ?? 0m));
			}
		}

		private void AdjustExpenseAmt(
			Tax tax, 
			TaxRev taxrev, 
			decimal curyTaxAmt, 
			ref decimal curyExpenseAmt)
		{
			if (IsDeductibleVATTax(tax))
			{
				curyExpenseAmt = curyTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100);
			}
		}

		private void AdjustTaxAmtOnDiscount(
			PXCache sender, 
			Tax tax, 
			ref decimal curyTaxAmt)
		{
			if ((tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt) &&
				tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
			{
				decimal? DiscPercent = null;
				DiscPercentsDict.TryGetValue(ParentRow(sender.Graph), out DiscPercent);

				Terms terms = SelectTerms(sender.Graph);

				curyTaxAmt = curyTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
			}
		}

		private void AdjustMinMaxTaxableAmt(
			PXCache sumcache, 
			TaxDetail taxdet, 
			TaxRev taxrev, 
			ref decimal curyTaxableAmt,
			ref decimal taxableAmt)
		{
			PXDBCurrencyAttribute.CuryConvBase(sumcache, taxdet, curyTaxableAmt, out taxableAmt);

			if (taxrev.TaxableMin != 0.0m)
			{
				if (taxableAmt < taxrev.TaxableMin)
				{
					curyTaxableAmt = 0.0m;
					taxableAmt = 0.0m;
				}
			}

			if (taxrev.TaxableMax != 0.0m)
			{
				if (taxableAmt > taxrev.TaxableMax)
				{
					PXDBCurrencyAttribute.CuryConvCury(sumcache, taxdet, (decimal)taxrev.TaxableMax, out curyTaxableAmt);
					taxableAmt = (decimal)taxrev.TaxableMax;
				}
			}
		}

		private static void SetValueOptional(PXCache cache, object data, object CuryTaxableDiscountAmt, string field)
		{
			int ordinal = cache.GetFieldOrdinal(field);
			if (ordinal > 0)
			{
				cache.SetValue(data, ordinal, CuryTaxableDiscountAmt);
			}
		}

		private TaxDetail TaxSummarize(PXCache sender, object taxrow, object row)
		{
			if (taxrow == null)
			{
				throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
			}

			PXCache sumcache = sender.Graph.Caches[_TaxSumType];
			TaxDetail taxSum = CalculateTaxSum(sender, taxrow, row);

			if (taxSum != null)
			{
				return (TaxDetail)sumcache.Update(taxSum);
			}
			else
			{
				TaxDetail taxdet = (TaxDetail)((PXResult)taxrow)[0];
				Delete(sumcache, taxdet);
				return null;
			}
		}

		protected virtual void CalcTaxes(PXCache sender, object row)
		{
			CalcTaxes(sender, row, PXTaxCheck.RecalcLine);
		}

		/// <summary>
		/// This method is intended to select document line for given tax row.
		/// Do not use it to select parent document foir given line.
		/// </summary>
		/// <param name="cache">Cache of the tax row.</param>
		/// <param name="row">Tax row for which line will be returned.</param>
		/// <returns>Document line object.</returns>
		protected virtual object SelectParent(PXCache cache, object row)
		{
			return PXParentAttribute.SelectParent(cache, row, _ChildType);
		}

		protected virtual void CalcTaxes(PXCache sender, object row, PXTaxCheck taxchk)
		{
			PXCache cache = sender.Graph.Caches[_TaxType];

			object detrow = row;

			foreach (object taxrow in SelectTaxes(sender, row, taxchk))
			{
				if (row == null)
				{
					detrow = SelectParent(cache, ((PXResult)taxrow)[0]);
				}

				if (detrow != null)
				{
					TaxSetLineDefault(sender, taxrow, detrow);
				}
			}
			CalcTotals(sender, row, true);
		}

		protected virtual void CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			_CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
		}

		protected virtual decimal CalcLineTotal(PXCache sender, object row)
		{
			decimal CuryLineTotal = 0m;

			object[] details = PXParentAttribute.SelectSiblings(sender, null);

			if (details != null)
			{
				foreach (object detrow in details)
				{
					CuryLineTotal += GetCuryTranAmt(sender, sender.ObjectsEqual(detrow, row) ? row : detrow) ?? 0m;
				}
			}
			return CuryLineTotal;
		}

		protected virtual void _CalcDocTotals(
			PXCache sender, 
			object row, 
			decimal CuryTaxTotal, 
			decimal CuryInclTaxTotal, 
			decimal CuryWhTaxTotal,
			decimal CuryTaxDiscountTotal)
		{
			decimal CuryLineTotal = CalcLineTotal(sender, row);

			decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal;

			decimal doc_CuryLineTotal = (decimal)(ParentGetValue(sender.Graph, _CuryLineTotal) ?? 0m);
			decimal doc_CuryTaxTotal = (decimal)(ParentGetValue(sender.Graph, _CuryTaxTotal) ?? 0m);

			if (!Equals(CuryLineTotal, doc_CuryLineTotal)||
				!Equals(CuryTaxTotal, doc_CuryTaxTotal))
			{
				ParentSetValue(sender.Graph, _CuryLineTotal, CuryLineTotal);
				ParentSetValue(sender.Graph, _CuryTaxTotal, CuryTaxTotal);

				if (!string.IsNullOrEmpty(_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
					return;
				}
			}

			if (!string.IsNullOrEmpty(_CuryDocBal))
			{
				decimal doc_CuryDocBal = (decimal)(ParentGetValue(sender.Graph, _CuryDocBal) ?? 0m);

				if (!Equals(CuryDocTotal, doc_CuryDocBal))
				{
					ParentSetValue(sender.Graph, _CuryDocBal, CuryDocTotal);
				}
			}
		}

		protected virtual void CalcTotals(PXCache sender, object row, bool CalcTaxes)
		{
			bool IsUseTax = false;

			decimal CuryTaxTotal = 0m;
			decimal CuryTaxDiscountTotal = 0m;
			decimal CuryInclTaxTotal = 0m;
			decimal CuryInclTaxDiscountTotal = 0m;
			decimal CuryWhTaxTotal = 0m;

			foreach (object taxrow in SelectTaxes(sender, row, PXTaxCheck.RecalcTotals))
			{
				TaxDetail taxdet = null;
				if (CalcTaxes)
				{
					taxdet = TaxSummarize(sender, taxrow, row);
				}
				else
				{
					taxdet = (TaxDetail)((PXResult)taxrow)[0];
				}

				if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Use)
				{
					IsUseTax = true;
				}
				else if (taxdet != null)
				{
					PXCache taxDetCache = sender.Graph.Caches[taxdet.GetType()];
					decimal CuryTaxAmt = (decimal)taxDetCache.GetValue(taxdet, _CuryTaxAmt);
					decimal CuryTaxDiscountAmt = GetOptionalDecimalValue(taxDetCache, taxdet, _CuryTaxDiscountAmt);
					//assuming that tax cannot be withholding and reverse at the same time
					Decimal multiplier = PXResult.Unwrap<Tax>(taxrow).ReverseTax == true ? Decimal.MinusOne : Decimal.One;
					if (PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Withholding)
					{
						CuryWhTaxTotal += multiplier * CuryTaxAmt;
					}
					if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
					{
						CuryInclTaxTotal += multiplier * CuryTaxAmt;
						CuryInclTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;
					}

					CuryTaxTotal += multiplier * CuryTaxAmt;
					CuryTaxDiscountTotal += multiplier * CuryTaxDiscountAmt;

					if (IsDeductibleVATTax(PXResult.Unwrap<Tax>(taxrow)))
					{
						CuryTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
						if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
						{
							CuryInclTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
						}
					}
				}
			}

			if (ParentGetStatus(sender.Graph) != PXEntryStatus.Deleted && ParentGetStatus(sender.Graph) != PXEntryStatus.InsertedDeleted)
			{
				CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal + CuryInclTaxDiscountTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);
			}

			if (IsUseTax)
			{
				ParentCache(sender.Graph).RaiseExceptionHandling(_CuryTaxTotal, ParentRow(sender.Graph), CuryTaxTotal, 
					new PXSetPropertyException(Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
			}
		}

		private decimal GetOptionalDecimalValue(PXCache cache, object data, string field)
		{
			decimal value = 0m;
			int fieldOrdinal = cache.GetFieldOrdinal(field);
			if (fieldOrdinal > 0)
			{
				value = (decimal)(cache.GetValue(data, fieldOrdinal) ?? 0m);
			}
			return value;
		}

		protected virtual PXCache ParentCache(PXGraph graph)
		{
			return graph.Caches[_ParentType];
		}

		protected virtual object ParentRow(PXGraph graph)
		{
			if (_ParentRow == null)
			{
				return ParentCache(graph).Current;
			}
			else
			{
				return _ParentRow;
			}
		}

		protected virtual PXEntryStatus ParentGetStatus(PXGraph graph)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetStatus(cache.Current);
			}
			else
			{
				return cache.GetStatus(_ParentRow);
			}
		}

		protected virtual void ParentSetValue(PXGraph graph, string fieldname, object value)
		{
			PXCache cache = ParentCache(graph);
			 
			if (_ParentRow == null)
			{
				object copy = cache.CreateCopy(cache.Current);
				cache.SetValueExt(cache.Current, fieldname, value);
				cache.MarkUpdated(cache.Current);
				cache.RaiseRowUpdated(cache.Current, copy);
			}
			else
			{
				cache.SetValueExt(_ParentRow, fieldname, value);
			}
		}

		protected virtual object ParentGetValue(PXGraph graph, string fieldname)
		{
			PXCache cache = ParentCache(graph);
			if (_ParentRow == null)
			{
				return cache.GetValue(cache.Current, fieldname);
			}
			else
			{
				return cache.GetValue(_ParentRow, fieldname);
			}
		}

		protected object ParentGetValue<Field>(PXGraph graph)
			where Field : IBqlField
		{
			return ParentGetValue(graph, typeof(Field).Name.ToLower());
		}

		protected void ParentSetValue<Field>(PXGraph graph, object value)
			where Field : IBqlField 
		{
			ParentSetValue(graph, typeof(Field).Name.ToLower(), value);
		}

		protected virtual bool CompareZone(PXGraph graph, string zoneA, string zoneB)
		{
			if (!string.Equals(zoneA, zoneB, StringComparison.OrdinalIgnoreCase))
			{
				foreach (PXResult<TaxZoneDet> r in PXSelectGroupBy<TaxZoneDet, Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>, Or<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>, Aggregate<GroupBy<TaxZoneDet.taxID, Count>>>.Select(graph, zoneA, zoneB))
				{
					if (r.RowCount == 1)
					{
						return false;
					}
				}
			}
			return true;
		}
		
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) == typeof(IPXRowInsertedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowUpdatedSubscriber) ||
				typeof(ISubscriber) == typeof(IPXRowDeletedSubscriber))
			{
				subscribers.Add(this as ISubscriber);
			}
			else
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}

		public virtual void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowInsertedSubscriber)
					{
						((IPXRowInsertedSubscriber)_Attributes[i]).RowInserted(sender, e);
					}
				}

				object copy;
				if (!inserted.TryGetValue(e.Row, out copy))
				{
					inserted[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				Preload(sender);

				DefaultTaxes(sender, e.Row);
				CalcTaxes(sender, e.Row, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		public virtual void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc && _TaxCalc != TaxCalc.ManualLineCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowUpdatedSubscriber)
					{
						((IPXRowUpdatedSubscriber)_Attributes[i]).RowUpdated(sender, e);
					}
				}

				object copy;
				if (!updated.TryGetValue(e.Row, out copy))
				{
					updated[e.Row] = sender.CreateCopy(e.Row);
				}
			}

			if (_TaxCalc == TaxCalc.Calc)
			{
				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)))
				{
					Preload(sender);
					ReDefaultTaxes(sender, e.OldRow, e.Row);
				}
				else if (!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					PXCache cache = sender.Graph.Caches[_TaxType];
					TaxDetail taxDetail = (TaxDetail)cache.CreateInstance();
					taxDetail.TaxID = GetTaxID(sender, e.OldRow);
					DelOneTax(cache, e.Row, taxDetail);
					AddOneTax(cache, e.Row, new TaxZoneDet() { TaxID = GetTaxID(sender, e.Row) });
				}

				bool calculated = false;

				if (!object.Equals(GetTaxCategory(sender, e.OldRow), GetTaxCategory(sender, e.Row)) ||
					!object.Equals(GetCuryTranAmt(sender, e.OldRow), GetCuryTranAmt(sender, e.Row)) ||
					!object.Equals(GetTaxID(sender, e.OldRow), GetTaxID(sender, e.Row)))
				{
					CalcTaxes(sender, e.Row, PXTaxCheck.Line);
					calculated = true;
				}

				if (!calculated)
				{
					CalcTotals(sender, e.Row, false);
				}
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		public virtual void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (_TaxCalc != TaxCalc.NoCalc)
			{
				for (int i = 0; i < _Attributes.Count; i++)
				{
					if (_Attributes[i] is IPXRowDeletedSubscriber)
					{
						((IPXRowDeletedSubscriber)_Attributes[i]).RowDeleted(sender, e);
					}
				}
			}

			PXEntryStatus parentStatus = ParentGetStatus(sender.Graph);
			if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;		

			decimal? val;
			if (GetTaxCategory(sender, e.Row) == null && ((val = GetCuryTranAmt(sender, e.Row)) == null || val == 0m))
			{
				return;
			}

			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				ClearTaxes(sender, e.Row);
				CalcTaxes(sender, null, PXTaxCheck.Line);
			}
			else if (_TaxCalc == TaxCalc.ManualCalc)
			{
				CalcTotals(sender, e.Row, false);
			}
		}

		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus == PXTranStatus.Completed)
			{
				if (inserted != null)
					inserted.Clear();
				if (updated != null)
					updated.Clear();
			}
		}


		protected object _ParentRow;

		protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
				{
					PXView siblings = CurrencyInfoAttribute.GetView(sender.Graph, _ChildType, _CuryKeyField);
					if (siblings != null && siblings.SelectSingle() != null)
					{
						CalcTaxes(siblings.Cache, null);
					}
				}
			}
 		}

		protected virtual void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				if (e.Row.GetType() == _ParentType)
				{
					_ParentRow = e.Row;
				}
				CalcTaxes(sender.Graph.Caches[_ChildType], null);
				_ParentRow = null;
			}
		}

		protected virtual void IsTaxSavedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			decimal? curyTaxTotal = (decimal?) sender.GetValue(e.Row, _CuryTaxTotal);
			decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);

			CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
		}

        protected virtual List<object> ChildSelect(PXCache cache, object data)
        {
            return TaxParentAttribute.ChildSelect(cache, data, this._ParentType);
        }

		protected virtual void ZoneUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var originalTaxCalc = TaxCalc;
			try
			{
				TaxZone newTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)sender.GetValue(e.Row, _TaxZoneID));
				TaxZone oldTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)e.OldValue);

				if (oldTaxZone != null && oldTaxZone.IsExternal == true)
				{
					TaxCalc = TaxCalc.Calc;
				}

				if (newTaxZone != null && newTaxZone.IsExternal == true)
				{
					TaxCalc = TaxCalc.ManualCalc;
				}


				if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
				{
					PXCache cache = sender.Graph.Caches[_ChildType];
					if (!CompareZone(sender.Graph, (string)e.OldValue, (string)sender.GetValue(e.Row, _TaxZoneID)) || sender.GetValue(e.Row, _TaxZoneID) == null)
					{
						Preload(sender);

						List<object> details = this.ChildSelect(cache, e.Row);
						ReDefaultTaxes(cache, details);

						_ParentRow = e.Row;
						CalcTaxes(cache, null);
						_ParentRow = null;
					}
				}
			}
			finally
			{
				TaxCalc = originalTaxCalc;
			}
		}

		protected virtual void ReDefaultTaxes(PXCache cache, List<object> details)
		{
					foreach (object det in details)
					{
						ClearTaxes(cache, det);
				ClearChildTaxAmts(cache, det);
					}

					foreach (object det in details)
					{
						DefaultTaxes(cache, det, false);
					}
			}

		protected virtual void ClearChildTaxAmts(PXCache cache, object childRow)
		{
			PXCache childCache = cache.Graph.Caches[_ChildType];
			SetTaxableAmt(childCache, childRow, 0);
			SetTaxAmt(childCache, childRow, 0);
			if (childCache.Locate(childRow) != null && //if record is not in cache then it is just being inserted - no need for manual update
				(childCache.GetStatus(childRow) == PXEntryStatus.Notchanged
				|| childCache.GetStatus(childRow) == PXEntryStatus.Held))
			{
				childCache.Update(childRow);
			}
		}

		protected virtual void ReDefaultTaxes(PXCache cache, object clearDet, object defaultDet, bool defaultExisting = true)
		{
			ClearTaxes(cache, clearDet);
			ClearChildTaxAmts(cache, clearDet);
			DefaultTaxes(cache, defaultDet, defaultExisting);
		}

		protected virtual void DateUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			if (_TaxCalc == TaxCalc.Calc || _TaxCalc == TaxCalc.ManualLineCalc)
			{
				Preload(sender);

				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);
				foreach (object det in details)
				{
					ReDefaultTaxes(cache, det, det, true);
				}
				_ParentRow = e.Row;
				_NoSumTaxable = true;
				try
				{
				CalcTaxes(cache, null);
				}
				finally
				{
				_ParentRow = null;
					_NoSumTaxable = false;
				}
			}
		}

		protected abstract void SetExtCostExt(PXCache sender, object child, decimal? value);

		protected abstract string GetExtCostLabel(PXCache sender, object row);

		protected string GetTaxCalcMode(PXGraph graph)
		{
			if (!_isTaxCalcModeEnabled)
			{
				throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
			}
			return (string)ParentGetValue(graph, _TaxCalcMode);
		}
		protected virtual bool AskRecalculate(PXCache sender, PXCache detailCache, object detail)
		{
			PXView view = sender.Graph.Views[sender.Graph.PrimaryView];
			string askMessage = PXLocalizer.LocalizeFormat(Messages.RecalculateExtCost, GetExtCostLabel(detailCache, detail));
			return view.Ask(askMessage, MessageButtons.YesNo) == WebDialogResult.Yes;
		}

		protected virtual void TaxCalcModeUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			string newValue = sender.GetValue(e.Row, _TaxCalcMode) as string;
			if (newValue != (string)e.OldValue)
			{
				PXCache cache = sender.Graph.Caches[_ChildType];
				List<object> details = this.ChildSelect(cache, e.Row);

				decimal? taxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
				if (details != null && details.Count != 0)
				{
					if (taxTotal.HasValue && taxTotal.Value != 0 && AskRecalculate(sender, cache, details[0]))
					{
						PXCache taxDetCache = cache.Graph.Caches[_TaxType];
						foreach (object det in details)
						{
							TaxDetail taxSum = TaxSummarizeOneLine(cache, det, SummType.All);
							if (taxSum == null) continue;
							decimal? taxableAmount;
							decimal? taxAmount;
							switch (newValue)
							{
								case TaxCalculationMode.Net:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, taxableAmount.Value, Precision));
									break;
								case TaxCalculationMode.Gross:
									taxableAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									taxAmount = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxAmt);
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, taxableAmount.Value + taxAmount.Value, Precision));
									break;
								case TaxCalculationMode.TaxSetting:
									TaxDetail taxSumInclusive = TaxSummarizeOneLine(cache, det, SummType.Inclusive);
									decimal? ExtCost;
									if (taxSumInclusive != null)
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxableAmt) + (decimal?)taxDetCache.GetValue(taxSumInclusive, _CuryTaxAmt);
									}
									else
									{
										ExtCost = (decimal?)taxDetCache.GetValue(taxSum, _CuryTaxableAmt);
									}
									SetExtCostExt(cache, det, PXDBCurrencyAttribute.RoundCury(cache, det, ExtCost.Value, Precision));
									break;
							}
						}
					}
				}

				Preload(sender);
				foreach (object det in details)
				{
					ReDefaultTaxes(cache, det, det, false);
				}
				_ParentRow = e.Row;
				CalcTaxes(cache, null);
				_ParentRow = null;
			}
		}

		private enum SummType
		{
			Inclusive, All
		}

		private TaxDetail TaxSummarizeOneLine(PXCache cache, object row, SummType summType)
		{
			List<object> taxitems = new List<object>();
			switch (summType)
			{
				case SummType.All:
					taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
						And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
								And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					break;
				case SummType.Inclusive:
					taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
						And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
							And<Tax.taxType, NotEqual<CSTaxType.withholding>,
								And<Tax.directTax, Equal<False>>>>>>(cache.Graph, row, PXTaxCheck.Line);
					break;
			} 
			
			if (taxitems.Count == 0) return null;

			PXCache taxDetCache = cache.Graph.Caches[_TaxType];
			TaxDetail taxLineSumDet = (TaxDetail)taxDetCache.CreateInstance();
			decimal? CuryTaxableAmt = (decimal?)taxDetCache.GetValue(((PXResult)taxitems[0])[0], _CuryTaxableAmt);

			//AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

			decimal? CuryTaxAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryTaxAmt));

			decimal? CuryExpenseAmt = SumWithReverseAdjustment(cache.Graph,
				taxitems,
				GetFieldType(taxDetCache, _CuryExpenseAmt));

			taxDetCache.SetValue(taxLineSumDet, _CuryTaxableAmt, CuryTaxableAmt);
			taxDetCache.SetValue(taxLineSumDet, _CuryTaxAmt, CuryTaxAmt + CuryExpenseAmt);

			return taxLineSumDet;
		}

		private decimal SumWithReverseAdjustment(PXGraph graph, List<Object> list, Type field)
		{
			decimal ret = 0.0m;
			list.ForEach(a =>
			{
				decimal? val = (decimal?) graph.Caches[BqlCommand.GetItemType(field)].GetValue(((PXResult) a)[BqlCommand.GetItemType(field)], field.Name);
				Tax tax = (Tax) ((PXResult) a)[typeof (Tax)];
				decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
				ret += (val ?? 0m) * multiplier;
			}
			);
			return ret;
		}

		protected virtual void TaxSum_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
			object newdet = e.Row;
            if (newdet == null) return;
            Dictionary<string, object> newdetKeys = GetKeyFieldValues(cache, newdet);
            bool insertNewTaxTran = true;

            if (ExternalTax.IsExternalTax(cache.Graph, (string)cache.GetValue(newdet, _TaxZoneID)) != true)
            {
                foreach (object cacheddet in cache.Cached)
                {
					Dictionary<string, object> cacheddetKeys = new Dictionary<string, object>();
						cacheddetKeys = GetKeyFieldValues(cache, cacheddet);
                    bool recordsEqual = true;
					PXEntryStatus status = cache.GetStatus(cacheddet);
					
					if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                    {
                        foreach (KeyValuePair<string, object> keyValue in newdetKeys)
                        {
							if (cacheddetKeys.ContainsKey(keyValue.Key) && !Object.Equals(cacheddetKeys[keyValue.Key], keyValue.Value))
							{
								recordsEqual = false;
								break;
							}
                        }
						if (recordsEqual)
						{
						    if (cache.Graph.IsMobile) // if inserting from mobile - override old detail
						    {
						        cache.Delete(cacheddet);
						    }
						    else
						    {
						        insertNewTaxTran = false;
						        break;
						    }
						}
                    }
                }
                if (!insertNewTaxTran)
                    e.Cancel = true;
            }
        }


        private Dictionary<string, object> GetKeyFieldValues(PXCache cache, object row)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            foreach (string key in cache.Keys)
            {
                if (key != _RecordID)
                    keyValues.Add(key, cache.GetValue(row, key));
            }
            return keyValues;
        }

		protected virtual void DelOneTax(PXCache sender, object detrow, object taxrow)
		{
			PXCache cache = sender.Graph.Caches[_ChildType];
			foreach (object taxdet in SelectTaxes(cache, detrow, PXTaxCheck.Line))
			{
				if (object.Equals(((TaxDetail)((PXResult)taxdet)[0]).TaxID, ((TaxDetail)taxrow).TaxID))
				{
					sender.Delete(((PXResult)taxdet)[0]);
				}
			}
		}

		protected virtual void Preload(PXCache sender)
		{
			SelectTaxes(sender, null, PXTaxCheck.RecalcTotals);
		}

		/// <summary>
		/// During the import process, some fields may not have a default value.
		/// </summary>
		private static void InvokeExceptForExcelImport(PXCache cache, Action action)
		{
			if (!cache.Graph.IsImportFromExcel)
			{
				action.Invoke();
			}
		}

		public override void CacheAttached(PXCache sender)
		{
			_ChildType = sender.GetItemType();

			inserted = new Dictionary<object, object>();
			updated = new Dictionary<object, object>();

				PXCache cache = sender.Graph.Caches[_TaxType];

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _DocDate, (s, e) => InvokeExceptForExcelImport(s, () => DateUpdated(s, e)));   
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxZoneID, (s, e) => InvokeExceptForExcelImport(s, () => ZoneUpdated(s, e)));
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _IsTaxSaved, (s, e) => InvokeExceptForExcelImport(s, () => IsTaxSavedFieldUpdated(s, e)));

			sender.Graph.FieldUpdated.AddHandler(_ParentType, _TermsID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryID, ParentFieldUpdated);
			sender.Graph.FieldUpdated.AddHandler(_ParentType, _CuryOrigDiscAmt, CuryOrigDiscAmt_FieldUpdated);

			sender.Graph.RowUpdated.AddHandler(_ParentType, ParentRowUpdated);		

            sender.Graph.RowInserting.AddHandler(_TaxSumType, TaxSum_RowInserting);

			foreach (PXEventSubscriberAttribute attr in sender.GetAttributesReadonly(null))
			{
				if (attr is CurrencyInfoAttribute)
				{
					_CuryKeyField = sender.GetBqlField(attr.FieldName);
					break;
				}
			}

			if (_CuryKeyField != null)
			{
				sender.Graph.RowUpdated.AddHandler<CurrencyInfo>(CurrencyInfo_RowUpdated);
			}

		    sender.Graph.Caches.SubscribeCacheCreated<Tax>(delegate
		    {
            PXUIFieldAttribute.SetVisible<Tax.exemptTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.statisticalTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.reverseTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.pendingTax>(sender.Graph.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.taxType>(sender.Graph.Caches[typeof(Tax)], null, false);
		    });

			if (_isTaxCalcModeEnabled)
			{
				sender.Graph.FieldUpdated.AddHandler(_ParentType, _TaxCalcMode, (s, e) => InvokeExceptForExcelImport(s, () => TaxCalcModeUpdated(s, e)));
			}

		}

	    public TaxBaseAttribute(Type ParentType, Type TaxType, Type TaxSumType, Type CalcMode = null, Type parentBranchIDField = null)
		{
	        ParentBranchIDField = parentBranchIDField;

	        ChildFinPeriodIDField = typeof(TaxTran.finPeriodID);
	        ChildBranchIDField = typeof(TaxTran.branchID);

			_ParentType = ParentType;
			_TaxType = TaxType;
			_TaxSumType = TaxSumType;

		    if (CalcMode != null)
		{
			if (!typeof(IBqlField).IsAssignableFrom(CalcMode))
			{
				throw new PXArgumentException("CalcMode", ErrorMessages.ArgumentException);
			}
			TaxCalcMode = CalcMode;
		}
		}

		public virtual int CompareTo(object other)
		{
			return 0;
		}
	}

	[Obsolete("This class is obsolete as mutiple installments are now supported with Pending taxes.")]
	public class TaxTranSelect<InvoiceTable, TermsID, InvoiceTaxTran, TaxID, WhereSelect>
		: PXSelectJoin<InvoiceTaxTran, LeftJoin<Tax, On<Tax.taxID, Equal<TaxID>>>, WhereSelect>
		where InvoiceTable : class, IBqlTable, new()
		where TermsID : IBqlField
		where InvoiceTaxTran : class, IBqlTable, new()
		where TaxID : IBqlField
		where WhereSelect : IBqlWhere, new()
	{
		#region Ctor
		public TaxTranSelect(PXGraph graph)
			: base(graph)
		{
			AddHandlers(graph);
		}

		public TaxTranSelect(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			AddHandlers(graph);
		}
		#endregion

		#region Implementation
		private void AddHandlers(PXGraph graph)
		{
			graph.RowPersisting.AddHandler<InvoiceTable>(RowPersisting);
		}

		protected virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() || e.Row == null) return;

			string termsID = (string)sender.GetValue<TermsID>(e.Row);
			Terms terms = TermsAttribute.SelectTerms(sender.Graph, termsID);

			if (terms?.InstallmentType == TermsInstallmentType.Multiple)
			{
				foreach (PXResult<InvoiceTaxTran, Tax> taxtran in View.SelectMulti())
				{
					Tax tax = taxtran;
					if (tax?.PendingTax == true)
					{
						sender.RaiseExceptionHandling<TermsID>(e.Row, termsID,
							new PXSetPropertyException(Messages.MultInstallmentTermsWithSVAT));
						break;
					}
				}
			}
		}
		#endregion
	}

	public class WhereTaxBase<TaxID, TaxFlag> : IBqlWhere
		where TaxID : IBqlOperand
		where TaxFlag : IBqlField
	{
		readonly IBqlCreator _where = new Where<Selector<TaxID, TaxFlag>, Equal<True>, 
			And<Selector<TaxID, Tax.statisticalTax>, Equal<False>, 
			And<Selector<TaxID, Tax.reverseTax>, Equal<False>, 
			And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> _where.AppendExpression(ref exp, graph, info, selection);

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_where.Verify(cache, item, pars, ref result, ref value);
		}
	}

	public class WhereExempt<TaxID> : WhereTaxBase<TaxID, Tax.exemptTax>
		where TaxID : IBqlOperand
	{
	}

	public class WhereTaxable<TaxID> : WhereTaxBase<TaxID, Tax.includeInTaxable>
		where TaxID : IBqlOperand
		{
	}

	public class WhereAPPPDTaxable<TaxID> : IBqlWhere
		where TaxID : IBqlOperand
	{
		readonly IBqlCreator _where = new Where<Selector<TaxID, Tax.includeInTaxable>, Equal<True>,
			And<Selector<TaxID, Tax.statisticalTax>, Equal<False>,
			And<Selector<TaxID, Tax.taxType>, Equal<CSTaxType.vat>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;
			status &= _where.AppendExpression(ref exp, graph, info, selection);
			return status;
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			_where.Verify(cache, item, pars, ref result, ref value);
		}
	}

	[System.SerializableAttribute()]
	public abstract class TaxDetail : ITaxDetail
	{		
		#region TaxID
		protected String _TaxID;
		public virtual String TaxID
		{
			get
			{
				return this._TaxID;
			}
			set
			{
				this._TaxID = value;
			}
		}
		#endregion
		#region TaxRate
		protected Decimal? _TaxRate;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? TaxRate
		{
			get
			{
				return this._TaxRate;
			}
			set
			{
				this._TaxRate = value;
			}
		}
		#endregion
		#region CuryInfoID
		protected Int64? _CuryInfoID;
		[PXDBLong()]
		[PXDefault()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion
		#region NonDeductibleTaxRate
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "100.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Deductible Tax Rate", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? NonDeductibleTaxRate { get; set; }
		#endregion
		#region ExpenseAmt
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? ExpenseAmt { get; set; }
		#endregion
		#region CuryExpenseAmt
        [PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expense Amount", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? CuryExpenseAmt { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	public class VendorTaxPeriodType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { SemiMonthly, Monthly, BiMonthly, Quarterly, SemiAnnually, Yearly, FiscalPeriod },
				new string[] { Messages.HalfMonth, Messages.Month, Messages.TwoMonths, Messages.Quarter, Messages.HalfYear, Messages.Year, Messages.FinancialPeriod }) { }
		}

		public const string Monthly = "M";
		public const string SemiMonthly = "B";
		public const string Quarterly = "Q";
		public const string Yearly = "Y";
		public const string FiscalPeriod = "F";
        public const string BiMonthly = "E";
		public const string SemiAnnually = "H";

		public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
		{
			public monthly() : base(Monthly) { ;}
		}
		public class semiMonthly : PX.Data.BQL.BqlString.Constant<semiMonthly>
		{
			public semiMonthly() : base(SemiMonthly) { ;}
		}
		public class quarterly : PX.Data.BQL.BqlString.Constant<quarterly>
		{
			public quarterly() : base(Quarterly) { ;}
		}
		public class yearly : PX.Data.BQL.BqlString.Constant<yearly>
		{
			public yearly() : base(Yearly) { ;}
		}
		public class fiscalPeriod : PX.Data.BQL.BqlString.Constant<fiscalPeriod>
		{
			public fiscalPeriod() : base(FiscalPeriod) { ;}
		}
        public class biMonthly : PX.Data.BQL.BqlString.Constant<biMonthly>
        {
            public biMonthly() : base(BiMonthly) { ;}
        }
		public class semiAnnually : PX.Data.BQL.BqlString.Constant<semiAnnually>
		{
			public semiAnnually() : base(SemiAnnually) { ;}
		}
	}

	public class VendorSVATTaxEntryRefNbr
	{
		public class InputListAttribute : PXStringListAttribute
		{
			public InputListAttribute()
				: base(
				new string[] { DocumentRefNbr, PaymentRefNbr, ManuallyEntered },
				new string[] { Messages.DocumentRefNbr, Messages.PaymentRefNbr, Messages.ManuallyEntered })
			{ }
		}

		public class OutputListAttribute : PXStringListAttribute
		{
			public OutputListAttribute()
				: base(
				new string[] { DocumentRefNbr, PaymentRefNbr, TaxInvoiceNbr, ManuallyEntered },
				new string[] { Messages.DocumentRefNbr, Messages.PaymentRefNbr, Messages.TaxInvoiceNbr, Messages.ManuallyEntered })
			{ }
		}

		public const string DocumentRefNbr = "D";
		public const string PaymentRefNbr = "P";
		public const string TaxInvoiceNbr = "T";
		public const string ManuallyEntered = "M";
		
		public class documentRefNbr : PX.Data.BQL.BqlString.Constant<documentRefNbr>
		{
			public documentRefNbr() : base(DocumentRefNbr) { }
		}

		public class paymentRefNbr : PX.Data.BQL.BqlString.Constant<paymentRefNbr>
		{
			public paymentRefNbr() : base(PaymentRefNbr) { }
		}

		public class taxInvoiceNbr : PX.Data.BQL.BqlString.Constant<taxInvoiceNbr>
		{
			public taxInvoiceNbr() : base(TaxInvoiceNbr) { }
		}

		public class manuallyEntered : PX.Data.BQL.BqlString.Constant<manuallyEntered>
		{
			public manuallyEntered() : base(ManuallyEntered) { }
		}
	}

	public class TaxReportLineSelector : PXSelectorAttribute
	{		
		public TaxReportLineSelector(Type search, params Type[] fields) : base(search, fields)
		{
			this.DescriptionField = typeof (TaxReportLine.descr);
			_UnconditionalSelect = 
				BqlCommand.CreateInstance(typeof(Search<TaxReportLine.lineNbr, 
													Where<TaxReportLine.vendorID, Equal<Current<TaxReportLine.vendorID>>, 
													And<TaxReportLine.lineNbr, Equal<Required<TaxReportLine.lineNbr>>>>>));
			_CacheGlobal = false;
		}
	}
}
