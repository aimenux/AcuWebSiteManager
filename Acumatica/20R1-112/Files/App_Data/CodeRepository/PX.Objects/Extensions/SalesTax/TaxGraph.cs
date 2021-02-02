using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.TX;
using Messages = PX.Objects.TX.Messages;
using PX.Objects.Extensions.MultiCurrency;
using PX.Common;

namespace PX.Objects.Extensions.SalesTax
{
    public abstract class TaxGraph<TGraph, TPrimary> : TaxBaseGraph<TGraph, TPrimary>
        where TGraph : PXGraph
        where TPrimary : class, IBqlTable, new()
    {
        protected override IEnumerable<ITaxDetail> ManualTaxes(Detail row)
        {
            List<ITaxDetail> ret = new List<ITaxDetail>();

            foreach (PXResult res in SelectTaxes(row, PXTaxCheck.RecalcTotals))
            {
                TaxTotal item = TaxTotals.Cache.GetExtension<TaxTotal>(res[0]);
                ret.Add((TaxItem)item);
            }
            return ret;
        }


        /// <exclude />
        protected bool _NoSumTotals;

		protected virtual void TaxTotal_CuryTaxableAmt_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			TaxTotal taxdetOrig = sender.GetExtension<TaxTotal>(e.Row);
			if (taxdetOrig == null) return;

			decimal newValue = (decimal)(sender.GetValue(e.Row, typeof(TaxTotal.curyTaxableAmt).Name) ?? 0m);
			decimal oldValue = (decimal)(e.OldValue ?? 0m);

			if (CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall &&
				newValue != oldValue)
			{
				foreach (object taxrow in SelectTaxes<
					Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(sender.Graph, null, PXTaxCheck.RecalcTotals, taxdetOrig.RefTaxID))
				{
					TaxTotal taxdet = sender.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
					Tax tax = PXResult.Unwrap<Tax>(taxrow);
					TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

					CalculateTaxSumTaxAmt(taxdet, tax, taxrev);
				}
			}
		}

		protected virtual void TaxTotal_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            TaxTotal taxSum = sender.GetExtension<TaxTotal>(e.Row);
            TaxTotal taxSumOld = sender.GetExtension<TaxTotal>(e.OldRow);
            if (e.ExternalCall && (!TaxTotals.Cache.ObjectsEqual<TaxTotal.curyTaxAmt>(taxSum, taxSumOld) || !TaxTotals.Cache.ObjectsEqual<TaxTotal.curyExpenseAmt>(taxSum, taxSumOld)))
            {
                PXCache parentCache = Documents.Cache;

                if (parentCache.Current != null)
                {
                    decimal discrepancy = CalculateTaxDiscrepancy(parentCache.Current);
                    decimal discrepancyBase;
					Base.FindImplementation<IPXCurrencyHelper>().CuryConvBase(discrepancy, out discrepancyBase);
                    ParentSetValue<Document.curyTaxRoundDiff>(discrepancy);
                    ParentSetValue<Document.taxRoundDiff>(discrepancyBase);
                }
            }

            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                if (e.OldRow != null && e.Row != null)
                {
                    if (taxSumOld.RefTaxID != taxSum.RefTaxID)
                    {
                        VerifyTaxID(taxSum, e.ExternalCall);
                    }
                    if (!TaxTotals.Cache.ObjectsEqual<TaxTotal.curyTaxAmt>(taxSum, taxSumOld) || !TaxTotals.Cache.ObjectsEqual<TaxTotal.curyExpenseAmt>(taxSum, taxSumOld))
                    {
                        CalcTotals(null, false);
                    }
                }
            }
        }

        protected virtual void TaxTotal_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            TaxTotal taxSum = sender.GetExtension<TaxTotal>(e.Row);
            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                VerifyTaxID(taxSum, e.ExternalCall);
            }
        }        

        protected virtual void TaxTotal_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                TaxTotal row = sender.GetExtension<TaxTotal>(e.Row);
                foreach (Detail det in Details.Select())
                {
                    DelOneTax(det, row.RefTaxID);
                }
                CalcTaxes(null);
            }
        }

        protected override List<Object> SelectDocumentLines(PXGraph graph, object row)
        {
            throw new PXException(Messages.MethodMustBeOverridden);
        }

        protected virtual void VerifyTaxID(TaxTotal row, bool externalCall)
        {
            bool nomatch = false;
            //TODO: move details to parameter
            
            foreach (Detail det in Details.Select())
            {
                ITaxDetail taxzonedet = MatchesCategory(det, (TaxItem)row);
                AddOneTax(det, taxzonedet);
            }
            object originalRow = TaxTotals.Cache.GetMain(row);

            _NoSumTotals = (CurrentDocument.TaxCalc == TaxCalc.ManualCalc && row.TaxRate != 0m && externalCall == false);
            PXRowDeleting del = delegate (PXCache _sender, PXRowDeletingEventArgs _e) { nomatch |= ReferenceEquals(originalRow, _e.Row); };

            Base.RowDeleting.AddHandler(originalRow.GetType(), del);
            try
            {
                CalcTaxes(null);
            }
            finally
            {
                Base.RowDeleting.RemoveHandler(originalRow.GetType(), del);
            }
            _NoSumTotals = false;

            if (nomatch)
            {
                TaxTotals.Cache.RaiseExceptionHandling<TaxTotal.refTaxID>(row, row.RefTaxID, 
                    new PXSetPropertyException(Messages.NoLinesMatchTax, PXErrorLevel.RowError));
            }
            TaxTotals.Cache.Current = row;
        }

        protected override void CalcTotals(object row, bool CalcTaxes)
        {
            string taxZondeID = CurrentDocument?.TaxZoneID;
            bool isExternalTax = ExternalTax.IsExternalTax(Base, taxZondeID);

            bool doCalc = _NoSumTotals == false && CalcTaxes && !isExternalTax;
            if (doCalc && CurrentDocument != null)
            {
                ResetDiscrepancy(Base);
            }
            base.CalcTotals(row, doCalc);
        }

        protected override void SetExtCostExt(PXCache sender, object child, decimal? value)
        {
            throw new PXException(Messages.MethodMustBeOverridden);
        }

        protected override string GetExtCostLabel(PXCache sender, object row)
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

        public enum TaxCalcLevelEnforcing
        {
            None, EnforceCalcOnItemAmount, EnforceInclusive
        }

        public class CalcTaxable
        {
            private bool _aSalesOrPurchaseSwitch;
            private TaxCalcLevelEnforcing _enforcing;

            public CalcTaxable(bool aSalesOrPurchaseSwitch, TaxCalcLevelEnforcing enforceType)
            {
                _aSalesOrPurchaseSwitch = aSalesOrPurchaseSwitch;
                _enforcing = enforceType;
            }

            public Decimal CalcTaxableFromTotalAmount(PXCache cache, object row, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate, Decimal aCuryTotal)
            {
				IComparer<Tax> taxComparer = GetTaxComparer();
				taxComparer.ThrowOnNull(nameof(taxComparer));

				Decimal result = Decimal.Zero;
                PXGraph graph = cache.Graph;
                List<TaxZoneDet> taxList = GetApplicableTaxList(graph, aTaxZoneID, aTaxCategoryID, aDocDate);
                Dictionary<string, PXResult<Tax, TaxRev>> taxRates = GetTaxRevisionList(graph, aDocDate);
                List<PXResult<Tax, TaxRev>> orderedTaxList = new List<PXResult<Tax, TaxRev>>(taxList.Count);

                foreach (TaxZoneDet iDet in taxList)
                {
                    if (taxRates.TryGetValue(iDet.TaxID, out PXResult<Tax, TaxRev> line))
                    {
                        int idx;
                        for (idx = orderedTaxList.Count;
                            (idx > 0) && taxComparer.Compare(orderedTaxList[idx - 1], line) > 0;
                            idx--) ;

                        orderedTaxList.Insert(idx, new PXResult<Tax, TaxRev>(line, line));
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

                decimal baseLvl2 = cache.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury(aCuryTotal / (1 + rateLvl2 / 100));
                decimal baseLvl1 = cache.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury(baseLvl2 / (1 + (rateLvl1 + rateInclusive) / 100));
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
                            curyTaxTotal += multiplier * cache.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury((baseLvl1 * taxRev.TaxRate / 100m) ?? 0m);
                            break;
                        case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
                            curyTax2Total += multiplier * cache.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury((baseLvl2 * taxRev.TaxRate / 100m) ?? 0m);
                            break;
                    }
                }
                result = aCuryTotal - curyTaxTotal - curyTax2Total;
                return result;
            }

            private List<TaxZoneDet> GetApplicableTaxList(PXGraph aGraph, string aTaxZoneID, string aTaxCategoryID, DateTime aDocDate)
            {
                List<TaxZoneDet> taxList = new List<TaxZoneDet>();
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
                    TaxZoneDet tzd = r;
                    if (collected.Contains(tzd.TaxID))
                    {
                    }
                    else
                    {
                        collected.Add(tzd.TaxID);
                        taxList.Add(tzd);
                    }
                }
                return taxList;
            }

            private Dictionary<string, PXResult<Tax, TaxRev>> GetTaxRevisionList(PXGraph aGraph, DateTime aDocDate)
            {
                PXSelectBase<Tax> taxRevSelect = null;
                if (_aSalesOrPurchaseSwitch)
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
                    Tax tax = record;
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
                            aTaxCategoryID, aDocDate, ControlTotalAmt - LinesTotal, false, TaxAttribute.TaxCalcLevelEnforcing.EnforceInclusive);
                        break;
                    case TaxCalculationMode.Net:
                        taxableAmount = TaxAttribute.CalcTaxableFromTotalAmount(cache, row, aTaxZoneID,
                            aTaxCategoryID, aDocDate, ControlTotalAmt - LinesTotal - TaxTotal, false, TaxAttribute.TaxCalcLevelEnforcing.EnforceCalcOnItemAmount);
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
        protected decimal CalculateTaxDiscrepancy(object row)
        {
            decimal discrepancy = 0m;
            SelectTaxes(row, PXTaxCheck.RecalcTotals).
                FindAll(taxrow =>
                {
                    Tax tax = (Tax)((PXResult)taxrow)[1];
                    return tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive;
                }).
                ForEach(taxrow =>
                {
                    TaxDetail taxdet = Taxes.Cache.GetExtension<TaxDetail>(((PXResult)taxrow)[0]);
                    TaxTotal taxSum = CalculateTaxSum(taxrow, row);
                    if (taxSum != null)
                    {
                        PXCache sumcache = TaxTotals.Cache;
                        discrepancy += taxdet.CuryTaxAmt.Value + taxdet.CuryExpenseAmt.Value - taxSum.CuryTaxAmt.Value - taxSum.CuryExpenseAmt.Value;
                    }
                });
            return discrepancy;
        }

        private void ResetDiscrepancy(PXGraph graph)
        {
            ParentSetValue<Document.curyTaxRoundDiff>(0m);
            ParentSetValue<Document.taxRoundDiff>(0m);
        }

        protected override decimal? GetTaxableAmt(object row)
        {
            throw new PXException(Messages.MethodMustBeOverridden);
        }

        protected override decimal? GetTaxAmt(object row)
        {
            throw new PXException(Messages.MethodMustBeOverridden);
        }
    }
}
