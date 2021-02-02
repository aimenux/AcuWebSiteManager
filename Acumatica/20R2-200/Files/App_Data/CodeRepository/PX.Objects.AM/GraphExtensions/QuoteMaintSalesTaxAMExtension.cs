using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.TX;
using Document = PX.Objects.Extensions.SalesTax.Document;
using TaxDetail = PX.Objects.Extensions.SalesTax.TaxDetail;

namespace PX.Objects.AM.GraphExtensions
{
    public class QuoteMaintSalesTaxAMExtension : PXGraphExtension<QuoteMaint.SalesTax, QuoteMaint>
    {
        internal bool AllowEstimates;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.manufacturingEstimating>();
        }

        internal decimal GetCuryEstimateTotal()
        {
            return GetCuryEstimateTotal(Base.Quote.Current);
        }

        internal decimal GetCuryEstimateTotal(CRQuote quote)
        {
            var total = 0m;
            if (!AllowEstimates || quote == null)
            {
                return total;
            }

            // Query from OpportunityEstimateRecords
            foreach (AMEstimateReference row in PXSelect<
                    AMEstimateReference,
                    Where<AMEstimateReference.opportunityQuoteID, Equal<Required<AMEstimateReference.opportunityQuoteID>
                        >>>
                .Select(Base, quote.QuoteID))
            {
                total += row?.CuryExtPrice ?? 0m;
            }

            return total;
        }

        [PXOverride]
        public virtual void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal,
            Action<object, decimal, decimal, decimal> del)
        {
            del?.Invoke(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);

            var oRow = (CRQuote)Base1.Documents.Cache.GetMain(Base1.Documents.Current);
            if (oRow == null)
            {
                return;
            }

            var locatedRow = Base.Quote.Cache.LocateElse(oRow);
            if (locatedRow == null)
            {
                return;
            }

            //var oppExt = PXCache<CRQuote>.GetExtension<CRQuoteExt>(locatedRow);
            //if (oppExt == null || oppExt.AMCuryEstimateTotal.GetValueOrDefault() == 0)
            //{
            //    return;
            //}

            var curyEstimateTotal = GetCuryEstimateTotal();
            if (curyEstimateTotal == 0)
            {
                return;
            }

            var curyDocTotal =
                locatedRow.ManualTotalEntry.GetValueOrDefault()
                    ? locatedRow.CuryAmount.GetValueOrDefault()
                    : locatedRow.CuryProductsAmount.GetValueOrDefault();

            var docTotalWithEstimate = curyDocTotal + curyEstimateTotal; //oppExt.AMCuryEstimateTotal.GetValueOrDefault();

            ParentSetValue<CRQuote.curyProductsAmount>(locatedRow, docTotalWithEstimate);
        }


        protected void ParentSetValue<Field>(object row, object value)
            where Field : IBqlField
        {
            ParentSetValue(row, typeof(Field).Name.ToLower(), value);
        }

        // of course the TaxBaseGraph hides the calls to set and get the parent value - recopy to somewhat use here
        protected virtual void ParentSetValue(object row, string fieldname, object value)
        {
            PXCache cache = Base1.Documents.Cache;
            var copy = cache.CreateCopy(cache.Current);
            cache.SetValueExt(row, fieldname, value);
            if (cache.GetStatus(row) == PXEntryStatus.Notchanged)
            {
                cache.SetStatus(row, PXEntryStatus.Updated);
            }
            cache.RaiseRowUpdated(row, copy);
        }

        protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.curyAmount> e)
        {
            if (e.Row == null || e.Row.ManualTotalEntry != true)
            {
                return;
            }

            SetProductsAmount(e.Row);
        }

        protected virtual void _(Events.FieldUpdated<CRQuote, CRQuote.curyDiscTot> e)
        {
            if (e.Row == null || e.Row.ManualTotalEntry != true)
            {
                return;
            }

            SetProductsAmount(e.Row);
        }

        protected virtual void SetProductsAmount(CRQuote row)
        {
            var estimateTotal = GetCuryEstimateTotal(row); //PXCache<CRQuote>.GetExtension<CRQuoteExt>(row)?.AMCuryEstimateTotal ?? 0m;
            row.CuryProductsAmount = row.CuryAmount - row.CuryDiscTot + estimateTotal;
        }

        protected virtual PXResultset<AMEstimateReference> GetDetails()
        {
            return PXSelectJoin<AMEstimateReference, InnerJoin<AMEstimateItem, On<AMEstimateReference.estimateID,
                    Equal<AMEstimateItem.estimateID>,
                    And<AMEstimateReference.revisionID, Equal<AMEstimateItem.revisionID>>>>,
                Where<AMEstimateReference.opportunityQuoteID, Equal<Current<CRQuote.quoteID>>>>.Select(Base);
        }

        // -------------------------------------------------------------------------------------------------------
        // PASTE FULL COPY FROM TaxBaseGraph BELOW

        protected Type DocumentTypeCuryDocBal = typeof(CRQuote.curyProductsAmount);

        protected Type TaxDetailType = typeof(CROpportunityTax);
        protected Type TaxDetailTypeCuryTaxAmt = typeof(CROpportunityTax.curyTaxAmt);
        protected Type TaxDetailTypeCuryExpenseAmt = typeof(CROpportunityTax.curyExpenseAmt);

        /// <summary>The current document.</summary>
        protected virtual Document CurrentDocument => _ParentRow ?? (Document)Base1.Documents.Cache.Current;

        /// <summary>If the value is <tt>true</tt>, indicates that tax calculation is enabled for the document.</summary>
        protected bool IsTaxCalcModeEnabled => false; //Base1.GetDocumentMapping().TaxCalcMode != typeof(Document.taxCalcMode) && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();
        /// <exclude />
        protected bool _NoSumTaxable;

        /// <summary>Adds a tax to the specified detail line.</summary>
        /// <param name="detrow">The detail line.</param>
        /// <param name="taxitem">The tax item.</param>
        protected virtual void AddOneTax(/*Detail*/ AMEstimateReference detrow, ITaxDetail taxitem)
        {
            if (taxitem != null)
            {
                object origTaxdet;
                object origDetail = detrow; //object origDetail = Details.Cache.GetMain(detrow);
                PXCache taxCache = Base.Caches<CROpportunityTax>(); //Base.Caches[GetTaxDetailMapping().Table];
                TaxParentAttribute.NewChild(taxCache, origDetail, origDetail.GetType(), out origTaxdet);
                TaxDetail newdet = Base1.Taxes.Cache.GetExtension<TaxDetail>(origTaxdet);
                newdet.RefTaxID = taxitem.TaxID;
                //newdet = InitializeTaxDet(newdet); 
                TaxDetail insdet = (TaxDetail)Base1.Insert(Base1.Taxes.Cache, newdet);
                origTaxdet = Base1.Taxes.Cache.GetMain(insdet);
                if (insdet != null) PXParentAttribute.SetParent(taxCache, origTaxdet, origDetail.GetType(), origDetail);
            }
        }

        public virtual ITaxDetail MatchesCategory(/*Detail*/ AMEstimateReference row, ITaxDetail zoneitem)
        {
            string taxcat = row.TaxCategoryID;
            //string taxid = row.TaxID;     //[BH] why would the detail record of the order have TaxID? I assume this is not necessary for Opportunity
            DateTime? docdate = CurrentDocument.DocumentDate;

            TaxRev rev = PXSelect<TaxRev,
                Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>,
                And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>,
                And<TaxRev.outdated, Equal<False>>>>>.Select(Base, zoneitem.TaxID, docdate);

            if (rev == null)
            {
                return null;
            }

            //[BH] why would the detail record of the order have TaxID? I assume this is not necessary for Opportunity
            //if (string.Equals(taxid, zoneitem.TaxID))
            //{
            //    return zoneitem;
            //}

            TaxCategory cat = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(Base, taxcat);

            if (cat == null)
            {
                return null;
            }
            return MatchesCategory(row, new[] { zoneitem }).FirstOrDefault();
        }

        public virtual IEnumerable<ITaxDetail> MatchesCategory(/*Detail*/ AMEstimateReference row, IEnumerable<ITaxDetail> zonetaxlist)
        {
            string taxcat = row.TaxCategoryID;

            List<ITaxDetail> ret = new List<ITaxDetail>();

            TaxCategory cat = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(Base, taxcat);

            if (cat == null)
            {
                return ret;
            }

            HashSet<string> cattaxlist = new HashSet<string>();
            foreach (TaxCategoryDet detail in PXSelect<TaxCategoryDet, Where<TaxCategoryDet.taxCategoryID, Equal<Required<TaxCategoryDet.taxCategoryID>>>>.Select(Base, taxcat))
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

        /// <exclude />
        protected virtual void DefaultTaxes(/*Detail*/ AMEstimateReference row, bool DefaultExisting)
        {
            Document doc = CurrentDocument;
            string taxzone = doc.TaxZoneID;
            DateTime? docdate = doc.DocumentDate;
            string taxcat = row.TaxCategoryID;


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
                        Or<TaxCategory.taxCatFlag, Equal<True>, And<TaxCategoryDet.taxCategoryID, IsNull>>>>>>>>>>.Select(Base, taxzone, taxcat, docdate))
            {
                AddOneTax(row, (TaxZoneDet)r);
                applicableTaxes.Add(((TaxZoneDet)r).TaxID);
            }

            //[BH] why would the detail record of the order have TaxID? I assume this is not necessary for Opportunity
            //string taxID;
            //if ((taxID = row.TaxID) != null)
            //{
            //    AddOneTax(row, new TaxZoneDet { TaxID = taxID });
            //    applicableTaxes.Add(taxID);
            //}

            foreach (ITaxDetail r in ManualTaxes(row))
            {
                if (applicableTaxes.Contains(r.TaxID))
                    applicableTaxes.Remove(r.TaxID);
            }

            foreach (string applicableTax in applicableTaxes)
            {
                AddTaxTotals(applicableTax, row);
            }

            if (DefaultExisting)
            {
                foreach (ITaxDetail r in MatchesCategory(row, ManualTaxes(row)))
                {
                    AddOneTax(row, r);
                }
            }
        }


        /// <summary>Assigns default taxes for the specified row.</summary>
        /// <param name="row">The detail line.</param>
        protected virtual void DefaultTaxes(/*Detail*/ AMEstimateReference row)
        {
            DefaultTaxes(row, true);
        }


        protected List<object> SelectTaxes(object row, PXTaxCheck taxchk)
        {
            return SelectTaxes<Where<True, Equal<True>>>(Base, row, taxchk);
        }

        // Copied from QuoteMaint.SalesTax
        protected virtual List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            where Where : IBqlWhere, new()
        {
            Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
            var currents = new[]
            {
                    row,
                    ((QuoteMaint)graph).Quote.Current
                };

            foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                        LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                            And<TaxRev.outdated, Equal<boolFalse>,
                                And<TaxRev.taxType, Equal<TaxType.sales>,
                                    And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                        And<Tax.taxType, NotEqual<CSTaxType.use>,
                                            And<Tax.reverseTax, Equal<boolFalse>,
                                                And<Current<CRQuote.documentDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
                        Where>
                    .SelectMultiBound(graph, currents, parameters))
            {
                tail[((Tax)record).TaxID] = record;
            }
            List<object> ret = new List<object>();
            switch (taxchk)
            {
                case PXTaxCheck.Line:
                    foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
                                    And<CROpportunityTax.quoteID, Equal<Current<CROpportunityProducts.quoteID>>,
                                        And<CROpportunityTax.lineNbr, Equal<Current<AMEstimateReference.taxLineNbr>>>>>>
                        .SelectMultiBound(graph, currents))
                    {
                        PXResult<Tax, TaxRev> line;
                        if (tail.TryGetValue(record.TaxID, out line))
                        {
                            int idx;
                            for (idx = ret.Count;
                                (idx > 0)
                                && String.Compare(((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                idx--) ;
                            ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                        }
                    }
                    return ret;
                case PXTaxCheck.RecalcLine:
                    foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
                            Where<CROpportunityTax.quoteID, Equal<Current<CRQuote.quoteID>>,
                                    And<CROpportunityTax.lineNbr, Less<intMax>>>>
                        .SelectMultiBound(graph, currents))
                    {
                        PXResult<Tax, TaxRev> line;
                        if (tail.TryGetValue(record.TaxID, out line))
                        {
                            int idx;
                            for (idx = ret.Count;
                                (idx > 0)
                                && ((CROpportunityTax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                && String.Compare(((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                idx--) ;
                            ret.Insert(idx, new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                        }
                    }
                    return ret;
                case PXTaxCheck.RecalcTotals:
                    foreach (CRTaxTran record in PXSelect<CRTaxTran,
                            Where<CRTaxTran.quoteID, Equal<Current<CRQuote.quoteID>>>,
                            OrderBy<Asc<CRTaxTran.lineNbr, Asc<CRTaxTran.taxID>>>>
                        .SelectMultiBound(graph, currents))
                    {
                        PXResult<Tax, TaxRev> line;
                        if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
                        {
                            int idx;
                            for (idx = ret.Count;
                                (idx > 0)
                                && String.Compare(((Tax)(PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                idx--) ;
                            ret.Insert(idx, new PXResult<CRTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                        }
                    }
                    return ret;
                default:
                    return ret;
            }
        }

        protected Tax AdjustTaxLevel(Tax taxToAdjust)
        {
            if (IsTaxCalcModeEnabled && taxToAdjust.TaxCalcType == CSTaxCalcType.Item && taxToAdjust.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
            {
                string TaxCalcMode = GetTaxCalcMode();
                if (!String.IsNullOrEmpty(TaxCalcMode))
                {
                    Tax adjdTax = (Tax)Base.Caches[typeof(Tax)].CreateCopy(taxToAdjust);
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

        protected virtual void ClearTaxes(object row)
        {
            PXCache cache = Base1.Taxes.Cache;
            foreach (object taxrow in SelectTaxes(row, PXTaxCheck.Line))
            {
                Base1.Delete(cache, ((PXResult)taxrow)[0]);
            }
        }

        private decimal Sum(List<object> list, Type field)
        {
            decimal ret = 0.0m;
            list.ForEach(a => ret += (decimal?)Base1.Taxes.Cache.GetValue(Base1.Taxes.Cache.GetExtension<TaxDetail>(((PXResult)a)[0]), field.Name) ?? 0m);
            return ret;
        }

        /// <exclude />
        protected virtual void AddTaxTotals(string taxID, object row)
        {
            PXCache cache = Base1.TaxTotals.Cache;
            TaxTotal newdet = (TaxTotal)cache.CreateInstance();
            newdet.RefTaxID = taxID;
            object insdet = Base1.Insert(cache, newdet);
        }

        protected Terms SelectTerms()
        {
            string TermsID = CurrentDocument.TermsID;
            Terms ret = TermsAttribute.SelectTerms(Base, TermsID);
            ret = ret ?? new Terms();
            return ret;
        }

        protected virtual void SetTaxableAmt(object row, decimal? value)
        {
        }

        protected virtual void SetTaxAmt(object row, decimal? value)
        {
        }

        protected virtual bool IsExemptTaxCategory(object row)
        {
            return IsExemptTaxCategory(Base.Caches<AMEstimateReference>(), row);
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

        protected List<object> SelectInclusiveTaxes(object row)
        {
            List<object> res = new List<object>();

            if (IsExemptTaxCategory(row))
            {
                return res;
            }

            if (!IsTaxCalcModeEnabled || GetTaxCalcMode() == TaxCalculationMode.TaxSetting)
            {
                res = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                    And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                        And<Tax.directTax, Equal<False>>>>>(Base, row, PXTaxCheck.Line);
            }
            else
            {
                string CalcMode = GetTaxCalcMode();
                if (CalcMode == TaxCalculationMode.Gross)
                {
                    res = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
                        And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line);
                }
                res.AddRange(SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                        And<Tax.taxCalcType, Equal<CSTaxCalcType.doc>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line));
            }
            return res;
        }

        protected List<object> SelectLvl1Taxes(object row)
        {
            return
                IsExemptTaxCategory(row)
                    ? new List<object>()
                    : SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.calcOnItemAmt>,
                        And<Tax.taxCalcLevel2Exclude, Equal<False>>>>(Base, row, PXTaxCheck.Line);
        }

        private void TaxSetLineDefault(object taxrow, AMEstimateReference row)
        {
            if (taxrow == null)
            {
                throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
            }

            PXCache cache = Base1.Taxes.Cache;

            TaxDetail taxdet = cache.GetExtension<TaxDetail>(((PXResult)taxrow)[0]);
            Tax tax = PXResult.Unwrap<Tax>(taxrow);
            TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

            decimal CuryTranAmt = GetCuryTranAmt(Base.Caches<AMEstimateReference>(), row) ?? 0m;

            if (taxrev.TaxID == null)
            {
                taxrev.TaxableMin = 0m;
                taxrev.TaxableMax = 0m;
                taxrev.TaxRate = 0m;
            }

            Terms terms = SelectTerms();
            List<object> incltaxes = SelectInclusiveTaxes(row);

            decimal InclRate = SumWithReverseAdjustment(incltaxes, typeof(TaxRev), typeof(TaxRev.taxRate));

            decimal CuryInclTaxAmt = SumWithReverseAdjustment(incltaxes, TaxDetailType, TaxDetailTypeCuryTaxAmt);

            decimal CuryTaxableAmt = 0.0m;
            decimal TaxableAmt = 0.0m;
            decimal CuryTaxAmt = 0.0m;

            decimal? DiscPercent = null;
            DiscPercentsDict.TryGetValue(CurrentDocument, out DiscPercent);

            switch (tax.TaxCalcLevel)
            {
                case CSTaxCalcLevel.Inclusive:
                    CuryTaxableAmt = CuryTranAmt / (1 + InclRate / 100);
                    CuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt * (decimal)taxrev.TaxRate / 100);
                    CuryInclTaxAmt = 0m;

                    incltaxes.ForEach(delegate (object a)
                    {
                        decimal? TaxRate = (decimal?)Base.Caches[typeof(TaxRev)].GetValue<TaxRev.taxRate>(((PXResult)a)[typeof(TaxRev)]);
                        decimal multiplier = ((Tax)(((PXResult)a)[typeof(Tax)])).ReverseTax == true ? Decimal.MinusOne : Decimal.One;
                        CuryInclTaxAmt += Base.FindImplementation<IPXCurrencyHelper>().RoundCury((CuryTaxableAmt * TaxRate / 100m) ?? 0m) * multiplier;
                    });

                    CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt;
                    SetTaxableAmt(row, CuryTaxableAmt);
                    SetTaxAmt(row, CuryInclTaxAmt);
                    break;
                case CSTaxCalcLevel.CalcOnItemAmt:
                    CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt;
                    break;
                case CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt:
                    List<object> lvl1Taxes = SelectLvl1Taxes(row);

                    decimal CuryLevel1TaxAmt = SumWithReverseAdjustment(lvl1Taxes, TaxDetailType, TaxDetailTypeCuryTaxAmt);

                    CuryTaxableAmt = CuryTranAmt - CuryInclTaxAmt + CuryLevel1TaxAmt;
                    break;
            }

            if ((tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
                    || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
                && tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
            {
                CuryTaxableAmt = CuryTaxableAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
            }

            if (tax.TaxCalcType == CSTaxCalcType.Item
                && (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt
                    || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt))
            {
                AdjustMinMaxTaxableAmt(taxrev, ref CuryTaxableAmt, ref TaxableAmt);

                CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;

                if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
                {
                    CuryTaxAmt = CuryTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
                }

            }
            else if (tax.TaxCalcType != CSTaxCalcType.Item)
            {
                CuryTaxAmt = 0.0m;
            }

            taxdet.TaxRate = taxrev.TaxRate;
            taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;

            decimal roundedCuryTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt);

            bool isExemptTaxCategory = IsExemptTaxCategory(row);
            if (isExemptTaxCategory)
            {
                SetTaxDetailExemptedAmount(cache, taxdet, roundedCuryTaxableAmt);
            }
            else
            {
                SetTaxDetailTaxableAmount(cache, taxdet, roundedCuryTaxableAmt);
            }

            decimal roundedCuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmt);

            if (tax.DeductibleVAT == true)
            {
                taxdet.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100));
                CuryTaxAmt = roundedCuryTaxAmt - (decimal)taxdet.CuryExpenseAmt;
                decimal expenseAmt;
                Base.FindImplementation<IPXCurrencyHelper>().CuryConvBase(taxdet.CuryExpenseAmt.Value, out expenseAmt);
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
                var detailCache = Base.Caches<AMEstimateReference>();
                cache.Update(taxdet);
                if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive &&
                    (detailCache.GetStatus(row) == PXEntryStatus.Notchanged
                    || detailCache.GetStatus(row) == PXEntryStatus.Held))
                {
                    detailCache.SetStatus(row, PXEntryStatus.Updated);
                }
            }
            else
            {
                Base1.Delete(cache, taxdet);
            }
        }

        protected virtual void SetTaxDetailTaxableAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxableAmt)
        {
            cache.SetValue(taxdet, typeof(TaxDetail.curyTaxableAmt).Name, curyTaxableAmt);
        }

        protected virtual void SetTaxDetailExemptedAmount(PXCache cache, TaxDetail taxdet, decimal? curyExemptedAmt)
        {
            cache.SetValue(taxdet, typeof(TaxDetail.curyExemptedAmt).Name, curyExemptedAmt);
        }

        protected virtual void SetTaxDetailTaxAmount(PXCache cache, TaxDetail taxdet, decimal? curyTaxAmt)
        {
            cache.SetValue(taxdet, typeof(TaxDetail.curyTaxAmt).Name, curyTaxAmt);
        }

        //private readonly Dictionary<object, bool> OrigDiscAmtExtCallDict = new Dictionary<object, bool>();
        private readonly Dictionary<object, decimal?> DiscPercentsDict = new Dictionary<object, decimal?>();

        protected virtual void AdjustTaxableAmount(object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
        {
        }


        protected virtual TaxTotal CalculateTaxSum(object taxrow, object row)
        {
            if (taxrow == null)
            {
                throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
            }

            PXCache sumcache = Base1.TaxTotals.Cache;

            TaxTotal taxdet = sumcache.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
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
            decimal TaxableAmt = 0.0m;
            decimal CuryTaxAmt = 0.0m;
            decimal CuryLevel1TaxAmt = 0.0m;
            decimal CuryExpenseAmt = 0.0m;
            decimal CuryExemptedAmt = 0.0m;

            List<object> taxitems = SelectTaxes<Where<Tax.taxID, Equal<Required<Tax.taxID>>>>(Base, row, PXTaxCheck.RecalcLine, taxdet.RefTaxID);

            if (taxitems.Count == 0 || taxrev.TaxID == null)
            {
                return null;
            }

            if (tax.TaxCalcType == CSTaxCalcType.Item)
            {
                // [BH] Opp tax detail does not use the CuryOrigTaxableAmt field
                //if (GetTaxDetailMapping().CuryOrigTaxableAmt != typeof(TaxDetail.curyOrigTaxableAmt))
                //{
                //    curyOrigTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyOrigTaxableAmt));
                //}

                CuryTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyTaxableAmt));

                AdjustTaxableAmount(row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

                CuryTaxAmt = Sum(taxitems, typeof(TaxDetail.curyTaxAmt));

                CuryExpenseAmt = Sum(taxitems, typeof(TaxDetail.curyExpenseAmt));
            }
            else
            {
                List<object> lvl1Taxes = SelectLvl1Taxes(row);

                if (_NoSumTaxable && (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || lvl1Taxes.Count == 0))
                {
                    //when changing doc date will not recalc taxable amount
                    CuryTaxableAmt = taxdet.CuryTaxableAmt.GetValueOrDefault();
                }
                else
                {
                    CuryTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyTaxableAmt));

                    AdjustTaxableAmount(row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

                    if (tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
                    {
                        CuryLevel1TaxAmt = Sum(lvl1Taxes, typeof(TaxDetail.curyTaxAmt));
                        CuryTaxableAmt += CuryLevel1TaxAmt;
                    }
                }

                curyOrigTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt);
                AdjustMinMaxTaxableAmt(taxrev, ref CuryTaxableAmt, ref TaxableAmt);

                CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;

                AdjustExpenseAmt(tax, taxrev, CuryTaxAmt, ref CuryExpenseAmt);
                AdjustTaxAmtOnDiscount(tax, ref CuryTaxAmt);
            }

            CuryExemptedAmt = Sum(taxitems, typeof(TaxDetail.curyExemptedAmt));

            taxdet = (TaxTotal)sumcache.CreateCopy(taxdet);

            // [BH] Opp tax detail does not use the CuryOrigTaxableAmt field
            //if (GetTaxTotalMapping().CuryOrigTaxableAmt != typeof(TaxTotal.curyOrigTaxableAmt))
            //{
            //    taxdet.CuryOrigTaxableAmt = curyOrigTaxableAmt;
            //}

            taxdet.TaxRate = taxrev.TaxRate;
            taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
            taxdet.CuryTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt);
            taxdet.CuryExemptedAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryExemptedAmt);
            taxdet.CuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmt);
            taxdet.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryExpenseAmt);
            if (tax.DeductibleVAT == true && tax.TaxCalcType != CSTaxCalcType.Item)
            {
                taxdet.CuryTaxAmt = taxdet.CuryTaxAmt - taxdet.CuryExpenseAmt;
            }

            return taxdet;
        }

        private void AdjustExpenseAmt(
            Tax tax,
            TaxRev taxrev,
            decimal curyTaxAmt,
            ref decimal curyExpenseAmt)
        {
            if (tax.DeductibleVAT == true)
            {
                curyExpenseAmt = curyTaxAmt * (1 - (taxrev.NonDeductibleTaxRate ?? 0m) / 100);
            }
        }

        private void AdjustTaxAmtOnDiscount(
            Tax tax,
            ref decimal curyTaxAmt)
        {
            if ((tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmt || tax.TaxCalcLevel == CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt) &&
                tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
            {
                decimal? DiscPercent = null;
                DiscPercentsDict.TryGetValue(CurrentDocument, out DiscPercent);

                Terms terms = SelectTerms();

                curyTaxAmt = curyTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
            }
        }

        private void AdjustMinMaxTaxableAmt(
            TaxRev taxrev,
            ref decimal curyTaxableAmt,
            ref decimal taxableAmt)
        {
            Base.FindImplementation<IPXCurrencyHelper>().CuryConvBase(curyTaxableAmt, out taxableAmt);

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
                    Base.FindImplementation<IPXCurrencyHelper>().CuryConvCury((decimal)taxrev.TaxableMax, out curyTaxableAmt);
                    taxableAmt = (decimal)taxrev.TaxableMax;
                }
            }
        }

        private TaxTotal TaxSummarize(object taxrow, object row)
        {
            if (taxrow == null)
            {
                throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
            }

            PXCache sumcache = Base1.TaxTotals.Cache;
            TaxTotal taxSum = CalculateTaxSum(taxrow, row);

            if (taxSum != null)
            {
                return (TaxTotal)sumcache.Update(taxSum);
            }
            TaxTotal taxdet = Base1.TaxTotals.Cache.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
            Base1.Delete(sumcache, taxdet);
            return null;
        }

        protected virtual void CalcTaxes(AMEstimateReference row)
        {
            CalcTaxes(row, PXTaxCheck.RecalcLine);
        }

        protected virtual object SelectParent(PXCache cache, object row)
        {
            return PXParentAttribute.SelectParent(cache, row, typeof(AMEstimateReference));
        }

        protected virtual void CalcTaxes(AMEstimateReference row, PXTaxCheck taxchk)
        {
            PXCache cache = Base1.Taxes.Cache;

            object detrow = row;

            foreach (object taxrow in SelectTaxes(row, taxchk))
            {
                if (row == null)
                {
                    detrow = SelectParent(cache, ((PXResult)taxrow)[0]);
                }

                if (detrow != null && detrow is AMEstimateReference)
                {
                    TaxSetLineDefault(taxrow, (AMEstimateReference)detrow);
                }
            }
            CalcTotals(row, true);
        }

        protected virtual void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            _CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
        }

        protected virtual decimal CalcLineTotalBase1()
        {
            var curyLineTotal = 0m;
            foreach (Detail detrow in Base1.Details.Select())
            {
                curyLineTotal += detrow?.CuryTranAmt ?? 0m;
            }
            return curyLineTotal;
        }

        protected virtual decimal CalcDiscountLineTotalBase1()
        {
            var discountLineTotal = 0m;
            foreach (Detail detrow in Base1.Details.Select())
            {
                discountLineTotal += detrow?.CuryTranDiscount ?? 0m;
            }
            return discountLineTotal;
        }

        protected virtual decimal CalcTranExtPriceTotalBase1()
        {
            var tranExtPriceTotal = 0m;
            foreach (Detail detrow in Base1.Details.Select())
            {
                tranExtPriceTotal += detrow?.CuryTranExtPrice ?? 0m;
            }
            return tranExtPriceTotal;
        }

        protected virtual decimal CalcEstimateTotal(AMEstimateReference row)
        {
            decimal totalEstimate = 0m;
            var orig_row = row;
            foreach (AMEstimateReference detrow in GetDetails())
            {
                totalEstimate += (Base.Caches<AMEstimateReference>().ObjectsEqual(detrow, orig_row) ? orig_row : detrow)?.CuryExtPrice ?? 0m;
            }

            return totalEstimate;
        }

        protected virtual void _CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            decimal CuryLineTotal = CalcLineTotalBase1();
            decimal DiscountLineTotal = CalcDiscountLineTotalBase1();
            decimal TranExtPriceTotal = CalcTranExtPriceTotalBase1();
            var estimateTotal = CalcEstimateTotal((AMEstimateReference)row);

            decimal CuryDocTotal = CuryLineTotal + estimateTotal + CuryTaxTotal - CuryInclTaxTotal;

            decimal doc_CuryLineTotal = CurrentDocument.CuryLineTotal ?? 0m;
            decimal doc_CuryTaxTotal = CurrentDocument.CuryTaxTotal ?? 0m;

            if (Equals(CuryLineTotal, doc_CuryLineTotal) == false ||
                Equals(CuryTaxTotal, doc_CuryTaxTotal) == false)
            {
                ParentSetValue<Document.curyLineTotal>(CuryLineTotal);
                ParentSetValue<Document.curyDiscountLineTotal>(DiscountLineTotal);
                ParentSetValue<Document.curyExtPriceTotal>(TranExtPriceTotal);
                ParentSetValue<Document.curyTaxTotal>(CuryTaxTotal);
                //ParentSetValue<CRQuoteExt.aMCuryEstimateTotal>(estimateTotal);
                if (DocumentTypeCuryDocBal != typeof(Document.curyDocBal))
                {
                    ParentSetValue<Document.curyDocBal>(CuryDocTotal);
                    return;
                }
            }

            if (DocumentTypeCuryDocBal != typeof(Document.curyDocBal))
            {
                decimal doc_CuryDocBal = CurrentDocument.CuryDocBal ?? 0m;
                if (Equals(CuryDocTotal, doc_CuryDocBal) == false)
                {
                    ParentSetValue<Document.curyDocBal>(CuryDocTotal);
                }
            }
        }

        protected virtual void CalcTotals(object row, bool CalcTaxes)
        {
            bool IsUseTax = false;

            decimal CuryTaxTotal = 0m;
            decimal CuryInclTaxTotal = 0m;
            decimal CuryWhTaxTotal = 0m;

            foreach (object taxrow in SelectTaxes(row, PXTaxCheck.RecalcTotals))
            {
                TaxTotal taxdet = null;
                if (CalcTaxes)
                {
                    taxdet = TaxSummarize(taxrow, row);
                }
                else
                {
                    taxdet = Base1.TaxTotals.Cache.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
                }

                if (taxdet != null && PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Use)
                {
                    IsUseTax = true;
                }
                else if (taxdet != null)
                {
                    decimal CuryTaxAmt = taxdet.CuryTaxAmt.Value;
                    //assuming that tax cannot be withholding and reverse at the same time
                    Decimal multiplier = PXResult.Unwrap<Tax>(taxrow).ReverseTax == true ? Decimal.MinusOne : Decimal.One;
                    if (PXResult.Unwrap<Tax>(taxrow).TaxType == CSTaxType.Withholding)
                    {
                        CuryWhTaxTotal += multiplier * CuryTaxAmt;
                    }
                    if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
                    {
                        CuryInclTaxTotal += multiplier * CuryTaxAmt;
                    }
                    CuryTaxTotal += multiplier * CuryTaxAmt;
                    if (PXResult.Unwrap<Tax>(taxrow).DeductibleVAT == true)
                    {
                        CuryTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
                        if (PXResult.Unwrap<Tax>(taxrow).TaxCalcLevel == "0")
                        {
                            CuryInclTaxTotal += multiplier * (decimal)taxdet.CuryExpenseAmt;
                        }
                    }
                }
            }

            if (ParentGetStatus() != PXEntryStatus.Deleted && ParentGetStatus() != PXEntryStatus.InsertedDeleted)
            {
                CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
            }

            if (IsUseTax)
            {
                Base1.Documents.Cache.RaiseExceptionHandling<Document.curyTaxTotal>(CurrentDocument, CuryTaxTotal,
                    new PXSetPropertyException(PX.Objects.TX.Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
            }
        }


        protected virtual PXEntryStatus ParentGetStatus()
        {
            return Base1.Documents.Cache.GetStatus(CurrentDocument);
        }

        protected virtual void ParentSetValue(string fieldname, object value)
        {
            PXCache cache = Base1.Documents.Cache;

            if (_ParentRow == null)
            {
                object copy = cache.CreateCopy(cache.Current);
                cache.SetValueExt(cache.Current, fieldname, value);
                if (cache.GetStatus(cache.Current) == PXEntryStatus.Notchanged)
                {
                    cache.SetStatus(cache.Current, PXEntryStatus.Updated);
                }
                cache.RaiseRowUpdated(cache.Current, copy);
            }
            else
            {
                cache.SetValueExt(_ParentRow, fieldname, value);
            }
        }

        protected virtual object ParentGetValue(string fieldname)
        {
            return Base1.Documents.Cache.GetValue(CurrentDocument, fieldname);
        }


        protected object ParentGetValue<Field>()
            where Field : IBqlField
        {
            return ParentGetValue(typeof(Field).Name.ToLower());
        }

        protected void ParentSetValue<Field>(object value)
            where Field : IBqlField
        {
            ParentSetValue(typeof(Field).Name.ToLower(), value);
        }

        protected virtual bool CompareZone(string zoneA, string zoneB)
        {
            if (!string.Equals(zoneA, zoneB, StringComparison.OrdinalIgnoreCase))
            {
                foreach (PXResult<TaxZoneDet> r in PXSelectGroupBy<TaxZoneDet, Where<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>, Or<TaxZoneDet.taxZoneID, Equal<Required<TaxZoneDet.taxZoneID>>>>, Aggregate<GroupBy<TaxZoneDet.taxID, Count>>>.Select(Base, zoneA, zoneB))
                {
                    if (r.RowCount == 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Copy changed from Detail_RowInserted
        public virtual void AMEstimateReference_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            del?.Invoke(sender, e);

            if (!AllowEstimates)
            {
                return;
            }

            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc != TaxCalc.NoCalc && taxCalc != TaxCalc.ManualLineCalc)
            {
                object copy;
                if (!inserted.TryGetValue(e.Row, out copy))
                {
                    inserted[e.Row] = sender.CreateCopy(e.Row);
                }
            }

            var row = (AMEstimateReference)e.Row;
            if (row.TaxCategoryID == null && row.CuryExtPrice.GetValueOrDefault() == 0m)
            {
                return;
            }

            if (taxCalc == TaxCalc.Calc)
            {
                Preload();
                DefaultTaxes(row);
                CalcTaxes(row, PXTaxCheck.Line);
            }
            else if (taxCalc == TaxCalc.ManualCalc)
            {
                CalcTotals(row, false);
            }
        }

        // Copy changed from Detail_RowUpdated
        public virtual void AMEstimateReference_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            del?.Invoke(sender, e);

            if (!AllowEstimates)
            {
                return;
            }

            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc != TaxCalc.NoCalc && taxCalc != TaxCalc.ManualLineCalc)
            {
                object copy;
                if (!updated.TryGetValue(e.Row, out copy))
                {
                    updated[e.Row] = sender.CreateCopy(e.Row);
                }
            }
            var row = (AMEstimateReference)e.Row;
            var oldRow = (AMEstimateReference)e.OldRow;

            if (taxCalc == TaxCalc.Calc)
            {
                if (!Equals(oldRow.TaxCategoryID, row.TaxCategoryID))
                {
                    Preload();
                    ReDefaultTaxes(oldRow, row);
                }
                //else if (!Equals(oldRow.TaxID, row.TaxID))
                //{
                //    PXCache cache = Base1.Taxes.Cache;
                //    TaxDetail taxDetail = (TaxDetail)cache.CreateInstance();
                //    taxDetail.RefTaxID = oldRow.TaxID;
                //    DelOneTax(row, taxDetail.RefTaxID);
                //    AddOneTax(row, new TaxZoneDet { TaxID = row.TaxID });
                //}

                bool calculated = false;

                if (!Equals(oldRow.TaxCategoryID, row.TaxCategoryID) ||
                    !Equals(oldRow.CuryExtPrice, row.CuryExtPrice) /*||
                    !Equals(oldRow.TaxID, row.TaxID)*/)
                {
                    CalcTaxes(row, PXTaxCheck.Line);
                    calculated = true;
                }

                if (!calculated)
                {
                    CalcTotals(row, false);
                }
            }
            else if (taxCalc == TaxCalc.ManualCalc)
            {
                CalcTotals(row, false);
            }
        }

        // Copy changed from Detail_RowDeleted
        public virtual void AMEstimateReference_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            del?.Invoke(sender, e);

            if (!AllowEstimates)
            {
                return;
            }

            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;

            PXEntryStatus parentStatus = ParentGetStatus();
            if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;
            var row = (AMEstimateReference)e.Row;

            decimal? val;
            if (row.TaxCategoryID == null && ((val = row.CuryExtPrice) == null || val == 0m))
            {
                return;
            }

            if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
            {
                ClearTaxes(row);
                CalcTaxes(null, PXTaxCheck.Line);
            }
            else if (taxCalc == TaxCalc.ManualCalc)
            {
                CalcTotals(row, false);
            }
        }

        protected Document _ParentRow;

        // Copy of TaxBaseGraph.CurrencyInfo_RowUpdated
        protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            del?.Invoke(sender, e);

            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
            {
                if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
                {
                    var graphExt = Base.GetExtension<QuoteMaintAMExtension>();
                    PXView siblings = graphExt?.OpportunityEstimateRecords?.View;

                    // Cannot use the generic GetView as it is not able to find the Graph Extension views for the given type
                    //PXView siblings = Base.FindImplementation<IPXCurrencyHelper>().GetView(typeof(AMEstimateReference), typeof(AMEstimateReference.curyInfoID));

                    if (siblings != null && siblings.SelectSingle() != null)
                    {
                        CalcTaxes(null);
                    }
                }
            }
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.TaxZoneID" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        // need to change from Document_* or you will get a failed to subscribe event error during runtime/page load
        protected virtual void CRQuote_TaxZoneID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            Document row = sender.GetExtension<Document>(e.Row);
            TaxZone newTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, row.TaxZoneID);
            TaxZone oldTaxZone = PXSelect<TaxZone, Where<TaxZone.taxZoneID, Equal<Required<TaxZone.taxZoneID>>>>.Select(sender.Graph, (string)e.OldValue);

            if (oldTaxZone != null && oldTaxZone.IsExternal == true)
            {
                row.TaxCalc = TaxCalc.Calc;
            }

            if (newTaxZone != null && newTaxZone.IsExternal == true)
            {
                row.TaxCalc = TaxCalc.ManualCalc;
            }


            if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
            {
                if (!CompareZone((string)e.OldValue, row.TaxZoneID) || row.TaxZoneID == null)
                {
                    Preload();
                    ReDefaultTaxes(GetDetails());

                    _ParentRow = row;
                    CalcTaxes(null);
                    _ParentRow = null;
                }
            }
        }

        protected virtual void ReDefaultTaxes(PXResultset<AMEstimateReference> details)
        {
            foreach (var det in details)
            {
                ClearTaxes(det);
                ClearChildTaxAmts(det);
            }

            foreach (var det in details)
            {
                DefaultTaxes(det, false);
            }
        }

        protected virtual void ClearChildTaxAmts(AMEstimateReference childRow)
        {
            PXCache childCache = Base.Caches<AMEstimateReference>();
            SetTaxableAmt(childRow, 0);
            SetTaxAmt(childRow, 0);
            if (childCache.Locate(childRow) != null && //if record is not in cache then it is just being inserted - no need for manual update
                (childCache.GetStatus(childRow) == PXEntryStatus.Notchanged
                || childCache.GetStatus(childRow) == PXEntryStatus.Held))
            {
                childCache.Update(childRow);
            }
        }

        protected virtual void ReDefaultTaxes(AMEstimateReference clearDet, AMEstimateReference defaultDet, bool defaultExisting = true)
        {
            ClearTaxes(clearDet);
            ClearChildTaxAmts(clearDet);
            DefaultTaxes(defaultDet, defaultExisting);
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.DocumentDate" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        // need to change from Document_* or you will get a failed to subscribe event error during runtime/page load
        protected virtual void CRQuote_DocumentDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            Document row = sender.GetExtension<Document>(e.Row);
            if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
            {
                Preload();

                foreach (AMEstimateReference det in GetDetails())
                {
                    ReDefaultTaxes(det, det, true);
                }
                _ParentRow = row;
                _NoSumTaxable = true;
                try
                {
                    CalcTaxes(null);
                }
                finally
                {
                    _ParentRow = null;
                    _NoSumTaxable = false;
                }
            }
        }

        protected virtual void SetExtCostExt(PXCache sender, object child, decimal? value)
        {
            // [BH] Base1 doesn't use this call for anything so lets leave commented out for now
            //sender.SetValueExt<AMEstimateReference.curyExtPrice>(child, value);
        }

        //protected abstract string GetExtCostLabel(PXCache sender, object row);
        protected virtual string GetExtCostLabel(PXCache sender, object row)
        {
            // [BH] Base1 doesn't use this call for anything so lets leave commented out for now
            return ((PXDecimalState)sender.GetValueExt<AMEstimateReference.curyExtPrice>(row)).DisplayName;
        }

        protected string GetTaxCalcMode()
        {
            if (!IsTaxCalcModeEnabled)
            {
                throw new PXException(PX.Objects.TX.Messages.DocumentTaxCalculationModeNotEnabled);
            }
            return (string)ParentGetValue<Document.taxCalcMode>();
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.TaxCalcMode" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        // need to change from Document_ or you will get a failed to subscribe event error during runtime/page load
        protected virtual void CRQuote_TaxCalcMode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e, PXFieldUpdated del)
        {
            del?.Invoke(sender, e);

            Document row = sender.GetExtension<Document>(e.Row);
            string newValue = row.TaxCalcMode;
            if (newValue != (string)e.OldValue)
            {
                PXCache cache = Base.Caches<AMEstimateReference>();
                PXResultset<AMEstimateReference> details = GetDetails();

                decimal? taxTotal = row.CuryTaxTotal; ;
                PXView view = sender.Graph.Views[sender.Graph.PrimaryView];
                if (details != null && details.Count != 0)
                {
                    string askMessage = PXLocalizer.LocalizeFormat(PX.Objects.TX.Messages.RecalculateExtCost, GetExtCostLabel(cache, details[0]));
                    if (taxTotal.HasValue && taxTotal.Value != 0 && view.Ask(askMessage, MessageButtons.YesNo) == WebDialogResult.Yes)
                    {
                        PXCache taxDetCache = Base1.Taxes.Cache;
                        foreach (AMEstimateReference det in details)
                        {
                            TaxDetail taxSum = TaxSummarizeOneLine(det, SummType.All);
                            if (taxSum == null) continue;
                            decimal? taxableAmount;
                            switch (newValue)
                            {
                                case TaxCalculationMode.Net:
                                    taxableAmount = taxSum.CuryTaxableAmt;
                                    SetExtCostExt(cache, det, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(taxableAmount.Value));
                                    break;
                                case TaxCalculationMode.Gross:
                                    taxableAmount = taxSum.CuryTaxableAmt;
                                    var taxAmount = taxSum.CuryTaxAmt; ;
                                    SetExtCostExt(cache, det, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(taxableAmount.Value + taxAmount.Value));
                                    break;
                                case TaxCalculationMode.TaxSetting:
                                    TaxDetail taxSumInclusive = TaxSummarizeOneLine(det, SummType.Inclusive);
                                    decimal? ExtCost = taxSumInclusive != null
                                        ? taxSumInclusive.CuryTaxableAmt + taxSumInclusive.CuryTaxAmt
                                        : taxSum.CuryTaxableAmt;
                                    SetExtCostExt(cache, det, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(ExtCost.Value));
                                    break;
                            }
                        }
                    }
                }

                Preload();
                foreach (AMEstimateReference det in details)
                {
                    ReDefaultTaxes(det, det, false);
                }
                _ParentRow = row;
                CalcTaxes(null);
                _ParentRow = null;
            }
        }

        /// <exclude />
        private enum SummType
        {
            Inclusive, All
        }

        private TaxDetail TaxSummarizeOneLine(object row, SummType summType)
        {
            List<object> taxitems = new List<object>();
            switch (summType)
            {
                case SummType.All:
                    taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
                        And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line);
                    break;
                case SummType.Inclusive:
                    taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                        And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line);
                    break;
            }

            if (taxitems.Count == 0) return null;

            PXCache taxDetCache = Base1.Taxes.Cache;
            TaxDetail taxLineSumDet = (TaxDetail)taxDetCache.CreateInstance();

            decimal? CuryTaxableAmt = taxDetCache.GetExtension<TaxDetail>(((PXResult)taxitems[0])[0]).CuryTaxableAmt;

            //AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

            decimal? CuryTaxAmt = SumWithReverseAdjustment(
                taxitems,
                TaxDetailType,
                TaxDetailTypeCuryTaxAmt);

            decimal? CuryExpenseAmt = SumWithReverseAdjustment(
                taxitems,
                TaxDetailType,
                TaxDetailTypeCuryExpenseAmt);
            taxLineSumDet.CuryTaxableAmt = CuryTaxableAmt;
            taxLineSumDet.CuryTaxAmt = CuryTaxAmt + CuryExpenseAmt;

            return taxLineSumDet;
        }

        private decimal SumWithReverseAdjustment(List<Object> list, Type table, Type field)
        {
            decimal ret = 0.0m;
            list.ForEach(a =>
            {
                decimal? val = (decimal?)Base.Caches[table].GetValue(((PXResult)a)[table], field.Name);
                Tax tax = (Tax)((PXResult)a)[typeof(Tax)];
                decimal multiplier = tax.ReverseTax == true ? Decimal.MinusOne : Decimal.One;
                ret += (val ?? 0m) * multiplier;
            }
            );
            return ret;
        }

        protected virtual void DelOneTax(AMEstimateReference detrow, string taxID)
        {
            foreach (object rec in SelectTaxes(detrow, PXTaxCheck.Line))
            {
                TaxDetail taxdet = Base1.Taxes.Cache.GetExtension<TaxDetail>(((PXResult)rec)[0]);
                if (taxdet.RefTaxID == taxID)
                {
                    Base1.Taxes.Delete(taxdet);
                }
            }
        }

        protected virtual void Preload()
        {
            SelectTaxes(null, PXTaxCheck.RecalcTotals);
        }

        /// <summary>Overrides the <tt>Initialize</tt> method of the base graph.</summary>
        public override void Initialize()
        {
            base.Initialize();

            // Copy of base as these are protected
            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();

            AllowEstimates = Base.GetExtension<QuoteMaintAMExtension>()?.AllowEstimates == true;
        }

        /// <summary>The dictionary of inserted records.</summary>
        protected Dictionary<object, object> inserted;
        /// <summary>The dictionary of updated records.</summary>
        protected Dictionary<object, object> updated;

        protected virtual decimal? GetCuryTranAmt(PXCache sender, AMEstimateReference row)
        {
            //decimal? CuryTranAmt = (decimal?)sender.GetValue(row, typeof(Detail.curyTranAmt).Name) * (decimal?)sender.GetValue(row, typeof(Detail.groupDiscountRate).Name) * (decimal?)sender.GetValue(row, typeof(Detail.documentDiscountRate).Name);
            decimal? CuryTranAmt = row.CuryExtPrice.GetValueOrDefault();
            return sender.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury((decimal)CuryTranAmt);
        }

        protected virtual string GetTaxCategory(PXCache sender, object row)
        {
            return (string)sender.GetValue(row, typeof(AMEstimateReference.taxCategoryID).Name);
        }

        // ----------------------------------------------------------------------
        // copy in everything from TaxGraph not in TaxGraphBase or overrides

        protected virtual IEnumerable<ITaxDetail> ManualTaxes(/*Detail*/ AMEstimateReference row)
        {
            List<ITaxDetail> ret = new List<ITaxDetail>();

            foreach (PXResult res in SelectTaxes(row, PXTaxCheck.RecalcTotals))
            {
                TaxTotal item = Base1.TaxTotals.Cache.GetExtension<TaxTotal>(res[0]);
                ret.Add((TaxItem)item);
            }
            return ret;
        }

        protected bool _NoSumTotals;
        // Change from TaxTotal_*
        public virtual void CRTaxTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e, PXRowUpdated del)
        {
            del?.Invoke(sender, e);

            TaxTotal taxSum = sender.GetExtension<TaxTotal>(e.Row);
            TaxTotal taxSumOld = sender.GetExtension<TaxTotal>(e.OldRow);

            // [BH] Letting base call do the commented out code below. We still need to make sure verifyTaxID and CalcTotals works correctly for our details

            //if (e.ExternalCall && (!Base1.TaxTotals.Cache.ObjectsEqual<TaxTotal.curyTaxAmt>(taxSum, taxSumOld) || !Base1.TaxTotals.Cache.ObjectsEqual<TaxTotal.curyExpenseAmt>(taxSum, taxSumOld)))
            //{
            //    PXCache parentCache = Base1.Documents.Cache;

            //    if (parentCache.Current != null)
            //    {
            //        decimal discrepancy = CalculateTaxDiscrepancy(parentCache.Current);
            //        decimal discrepancyBase;
            //        Base.FindImplementation<IPXCurrencyHelper>().CuryConvBase(discrepancy, out discrepancyBase);
            //        ParentSetValue<Document.curyTaxRoundDiff>(discrepancy);
            //        ParentSetValue<Document.taxRoundDiff>(discrepancyBase);
            //    }
            //}

            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                if (e.OldRow != null && e.Row != null)
                {
                    if (taxSumOld.RefTaxID != taxSum.RefTaxID)
                    {
                        VerifyTaxID(taxSum, e.ExternalCall);
                    }
                    if (!Base1.TaxTotals.Cache.ObjectsEqual<TaxTotal.curyTaxAmt>(taxSum, taxSumOld) || !Base1.TaxTotals.Cache.ObjectsEqual<TaxTotal.curyExpenseAmt>(taxSum, taxSumOld))
                    {
                        CalcTotals(null, false);
                    }
                }
            }
        }

        // Change from TaxTotal_*
        protected virtual void CRTaxTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e, PXRowInserted del)
        {
            del?.Invoke(sender, e);

            TaxTotal taxSum = sender.GetExtension<TaxTotal>(e.Row);
            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                VerifyTaxID(taxSum, e.ExternalCall);
            }
        }

        // Change from TaxTotal_*
        public virtual void CRTaxTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e, PXRowDeleted del)
        {
            if ((CurrentDocument.TaxCalc != TaxCalc.NoCalc && e.ExternalCall || CurrentDocument.TaxCalc == TaxCalc.ManualCalc))
            {
                TaxTotal row = sender.GetExtension<TaxTotal>(e.Row);
                foreach (AMEstimateReference det in GetDetails())
                {
                    DelOneTax(det, row.RefTaxID);
                }
                CalcTaxes(null);
            }

            del?.Invoke(sender, e);
        }

        protected virtual void VerifyTaxID(TaxTotal row, bool externalCall)
        {
            var nomatch = false;

            foreach (AMEstimateReference det in GetDetails())
            {
                ITaxDetail taxzonedet = MatchesCategory(det, (TaxItem)row);
                AddOneTax(det, taxzonedet);
            }
            object originalRow = Base1.TaxTotals.Cache.GetMain(row);

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
                Base1.TaxTotals.Cache.RaiseExceptionHandling<TaxTotal.refTaxID>(row, row.RefTaxID,
                    new PXSetPropertyException(PX.Objects.TX.Messages.NoLinesMatchTax, PXErrorLevel.RowError));
            }
            Base1.TaxTotals.Cache.Current = row;
        }
    }
}