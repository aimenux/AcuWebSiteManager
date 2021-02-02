using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM.Extensions;
using PX.Objects.CS;
using PX.Objects.TX;
using Messages = PX.Objects.TX.Messages;
using PX.Objects.Extensions.MultiCurrency;

namespace PX.Objects.Extensions.SalesTax
{
    /// <summary>The generic graph extension that provides the sales tax functionality.</summary>
    public abstract class TaxBaseGraph<TGraph, TPrimary> : PXGraphExtension<TGraph>
         where TGraph : PXGraph
         where TPrimary : class, IBqlTable, new()
    {
        /// <summary>Defines the default mapping of the <see cref="Document" /> mapped cache extension to a DAC.</summary>
        protected class DocumentMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(Document);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="Document" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public DocumentMapping(Type table)
            {
                _table = table;
            }
            /// <exclude />
            public Type BranchID = typeof(Document.branchID);
            /// <exclude />
            public Type CuryID = typeof(Document.curyID);
            /// <exclude />
            public Type CuryInfoID = typeof(Document.curyInfoID);
            /// <exclude />
            public Type DocumentDate = typeof(Document.documentDate);
            /// <exclude />
            public Type FinPeriodID = typeof(Document.finPeriodID);
            /// <exclude />
            public Type TaxZoneID = typeof(Document.taxZoneID);
            /// <exclude />
            public Type TermsID = typeof(Document.termsID);
            /// <exclude />
            public Type CuryLinetotal = typeof(Document.curyLineTotal);
            /// <exclude />
            public Type CuryDiscountLineTotal = typeof(Document.curyDiscountLineTotal);
            /// <exclude />
            public Type CuryExtPriceTotal = typeof(Document.curyExtPriceTotal);
            /// <exclude />
            public Type CuryDocBal = typeof(Document.curyDocBal);
            /// <exclude />
            public Type CuryTaxTotal = typeof(Document.curyTaxTotal);
            /// <exclude />
            public Type CuryDiscTot = typeof(Document.curyDiscTot);
            /// <exclude />
            public Type CuryDiscAmt = typeof(Document.curyDiscAmt);
            /// <exclude />
            public Type CuryOrigWhTaxAmt = typeof(Document.curyOrigWhTaxAmt);
            /// <exclude />
            public Type CuryTaxRoundDiff = typeof(Document.curyTaxRoundDiff);
            /// <exclude />
            public Type TaxRoundDiff = typeof(Document.taxRoundDiff);
            /// <exclude />
            public Type IsTaxSaved = typeof(Document.isTaxSaved);
            /// <exclude />
            public Type TaxCalcMode = typeof(Document.taxCalcMode);
        }
        /// <summary>Defines the default mapping of the <see cref="Detail" /> mapped cache extension to a DAC.</summary>
        protected class DetailMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(Detail);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;
            /// <summary>Creates the default mapping of the <see cref="Detail" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            public DetailMapping(Type table) { _table = table; }
            /// <exclude />
            public Type CuryInfoID = typeof(Detail.curyInfoID);
            /// <exclude />
            public Type TaxCategoryID = typeof(Detail.taxCategoryID);
            /// <exclude />
            public Type GroupDiscountRate = typeof(Detail.groupDiscountRate);
            /// <exclude />
            public Type DocumentDiscountRate = typeof(Detail.documentDiscountRate);
            /// <exclude />
            public Type CuryTranAmt = typeof(Detail.curyTranAmt);
            /// <exclude />
            public Type CuryTranDiscount = typeof(Detail.curyTranDiscount);
            /// <exclude />
            public Type CuryTranExtPrice = typeof(Detail.curyTranExtPrice);
        }
        /// <summary>Defines the default mapping of the <see cref="TaxDetail" /> mapped cache extension to a DAC.</summary>
        protected class TaxDetailMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(TaxDetail);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;
            /// <summary>Creates the default mapping of the <see cref="TaxDetail" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            /// <param name="TaxID">A DAC class field with tax ID.</param>
            public TaxDetailMapping(Type table, Type TaxID) { _table = table; RefTaxID = TaxID; }
            /// <exclude />
            public Type RefTaxID;
            /// <exclude />
            public Type TaxRate = typeof(TaxDetail.taxRate);
            /// <exclude />
            public Type CuryInfoID = typeof(TaxDetail.curyInfoID);
            /// <exclude />
            public Type NonDeductibleTaxRate = typeof(TaxDetail.nonDeductibleTaxRate);
            /// <exclude />
            public Type CuryTaxableAmt = typeof(TaxDetail.curyTaxableAmt);
            /// <exclude />
			public Type CuryExemptedAmt = typeof(TaxDetail.curyExemptedAmt);
			/// <exclude />
            public Type CuryTaxAmt = typeof(TaxDetail.curyTaxAmt);
            /// <exclude />
            public Type ExpenseAmt = typeof(TaxDetail.expenseAmt);
            /// <exclude />
            public Type CuryExpenseAmt = typeof(TaxDetail.curyExpenseAmt);
            /// <exclude />
            public Type CuryOrigTaxableAmt = typeof(TaxDetail.curyOrigTaxableAmt);

        }
        /// <summary>Defines the default mapping of the <see cref="TaxTotal" /> mapped cache extension to a DAC.</summary>
        protected class TaxTotalMapping : IBqlMapping
        {
            /// <exclude />
            public Type Extension => typeof(TaxTotal);
            /// <exclude />
            protected Type _table;
            /// <exclude />
            public Type Table => _table;

            /// <summary>Creates the default mapping of the <see cref="TaxTotal" /> mapped cache extension to the specified table.</summary>
            /// <param name="table">A DAC.</param>
            /// <param name="TaxID">A DAC class field with tax ID.</param>
            public TaxTotalMapping(Type table, Type TaxID) { _table = table; RefTaxID = TaxID; }
            /// <exclude />
            public Type RefTaxID;
            /// <exclude />
            public Type TaxRate = typeof(TaxTotal.taxRate);
            /// <exclude />
            public Type TaxZoneID = typeof(TaxTotal.taxZoneID);
            /// <exclude />
            public Type CuryInfoID = typeof(TaxTotal.curyInfoID);
            /// <exclude />
            public Type NonDeductibleTaxRate = typeof(TaxTotal.nonDeductibleTaxRate);
            /// <exclude />
            public Type CuryTaxableAmt = typeof(TaxTotal.curyTaxableAmt);
            /// <exclude />
			public Type CuryExemptedAmt = typeof(TaxTotal.curyExemptedAmt);
			/// <exclude />
            public Type CuryTaxAmt = typeof(TaxTotal.curyTaxAmt);
            /// <exclude />
            public Type ExpenseAmt = typeof(TaxTotal.expenseAmt);
            /// <exclude />
            public Type CuryExpenseAmt = typeof(TaxTotal.curyExpenseAmt);
            /// <exclude />
            public Type CuryOrigTaxableAmt = typeof(TaxTotal.curyOrigTaxableAmt);
        }

        /// <summary>Returns the mapping of the <see cref="Document" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDocumentMapping() method in the implementation class. The method overrides the default mapping of the %Document% mapped cache extension to a DAC: For the CROpportunity DAC, mapping of three fields of the mapped cache extension is overridden." lang="CS">
        /// protected override DocumentMapping GetDocumentMapping()
        /// {
        ///     return new DocumentMapping(typeof(CROpportunity)) {DocumentDate = typeof(CROpportunity.closeDate), CuryDocBal = typeof(CROpportunity.curyProductsAmount), CuryDiscAmt = typeof(CROpportunity.curyDiscTot) };
        /// }</code>
        /// </example>
        protected abstract DocumentMapping GetDocumentMapping();
        /// <summary>Returns the mapping of the <see cref="Detail" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetDetailMapping() method in the implementation class. The method overrides the default mapping of the %Detail% mapped cache extension to a DAC: For the CROpportunityProducts DAC, the CuryTranAmt field of the mapped cache extension is mapped to the curyAmount of the DAC; other fields of the extension are mapped by default." lang="CS">
        /// protected override DetailMapping GetDetailMapping()
        /// {
        ///     return new DetailMapping(typeof(CROpportunityProducts)) { CuryTranAmt = typeof(CROpportunityProducts.curyAmount) };
        /// }</code>
        /// </example>
        protected abstract DetailMapping GetDetailMapping();
        /// <summary>Returns the mapping of the <see cref="TaxDetail" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetTaxDetailMapping() method in the implementation class. The method returns the defaul mapping of the %TaxDetail% mapped cache extension to the CROpportunityTax DAC." lang="CS">
        /// protected override TaxDetailMapping GetTaxDetailMapping(
        /// {
        ///     return new TaxDetailMapping(typeof(CROpportunityTax), typeof(CROpportunityTax.taxID));
        /// }</code>
        /// </example>
        protected abstract TaxDetailMapping GetTaxDetailMapping();
        /// <summary>Returns the mapping of the <see cref="TaxTotal" /> mapped cache extension to a DAC. This method must be overridden in the implementation class of the base graph.</summary>
        /// <remarks>In the implementation graph for a particular graph, you can either return the default mapping or override the default mapping in this method.</remarks>
        /// <example>
        ///   <code title="Example" description="The following code shows the method that overrides the GetTaxTotalMapping() method in the implementation class. The method returns the defaul mapping of the %TaxTotal% mapped cache extension to the CRTaxTran DAC." lang="CS">
        /// protected override TaxTotalMapping GetTaxTotalMapping()
        /// {
        ///     return new TaxTotalMapping(typeof(CRTaxTran), typeof(CRTaxTran.taxID));
        /// }</code>
        /// </example>
        protected abstract TaxTotalMapping GetTaxTotalMapping();

        /// <summary>A mapping-based view of the <see cref="Document" /> data.</summary>
        public PXSelectExtension<Document> Documents;
        /// <summary>A mapping-based view of the <see cref="Detail" /> data.</summary>
        public PXSelectExtension<Detail> Details;
        /// <summary>A mapping-based view of the <see cref="TaxDetail" /> data.</summary>
        public PXSelectExtension<TaxDetail> Taxes;
        /// <summary>A mapping-based view of the <see cref="TaxTotal" /> data.</summary>
        public PXSelectExtension<TaxTotal> TaxTotals;

        /// <summary>The current document.</summary>
        protected virtual Document CurrentDocument => _ParentRow ?? (Document)Documents.Cache.Current;

        /// <summary>If the value is <tt>true</tt>, indicates that tax calculation is enabled for the document.</summary>
        protected bool IsTaxCalcModeEnabled => GetDocumentMapping().TaxCalcMode != typeof(Document.taxCalcMode) && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>();

        /// <exclude />
        protected virtual bool CalcGrossOnDocumentLevel { get; set; }
        /// <exclude />
        protected bool _NoSumTaxable;

        /// <summary>Inserts the specified record in the specified cache object.</summary>
        /// <param name="cache">The cache object to which the record should be inserted.</param>
        /// <param name="item">The record that should be inserted.</param>
        public virtual object Insert(PXCache cache, object item)
        {
            return cache.Insert(item);
        }

        /// <summary>Deletes the specified record from the specified cache object.</summary>
        /// <param name="cache">The cache object from which the record should be deleted.</param>
        /// <param name="item">The record that should be deleted.</param>
        public virtual object Delete(PXCache cache, object item)
        {
            return cache.Delete(item);
        }

        public static void Calculate<TaxExtension>(PXCache sender, PXRowInsertedEventArgs e)
            where TaxExtension : TaxBaseGraph<TGraph, TPrimary>
        {
            var taxextesion = sender.Graph.GetExtension<TaxExtension>();
            taxextesion?.Calculate(sender, e);
        }

        public static void Calculate<TaxExtension>(PXCache sender, PXRowUpdatedEventArgs e)
                   where TaxExtension : TaxBaseGraph<TGraph, TPrimary>
        {
            var taxextesion = sender.Graph.GetExtension<TaxExtension>();
            taxextesion?.Calculate(sender, e);
        }

        public void Calculate(PXCache sender, PXRowInsertedEventArgs e)
        {
            Document doc = CurrentDocument;

            if (doc.TaxCalc == TaxCalc.ManualLineCalc)
            {
                doc.TaxCalc = TaxCalc.Calc;

                try
                {
                    Detail_RowInserted(sender, e);
                }
                finally
                {
                    doc.TaxCalc = TaxCalc.ManualLineCalc;
                }
            }

            if (doc.TaxCalc == TaxCalc.ManualCalc)
            {
                object copy;
                if (inserted.TryGetValue(e.Row, out copy))
                {
                    Detail_RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
                    inserted.Remove(e.Row);

                    if (updated.TryGetValue(e.Row, out copy))
                    {
                        updated.Remove(e.Row);
                    }
                }
            }
        }
        public void Calculate(PXCache sender, PXRowUpdatedEventArgs e)
        {
            Document doc = CurrentDocument;
            if (doc.TaxCalc == TaxCalc.ManualLineCalc)
            {
                doc.TaxCalc = TaxCalc.Calc;

                try
                {
                    Detail_RowUpdated(sender, e);
                }
                finally
                {
                    doc.TaxCalc = TaxCalc.ManualLineCalc;
                }
            }

            if (doc.TaxCalc == TaxCalc.ManualCalc)
            {
                object copy;
                if (updated.TryGetValue(e.Row, out copy))
                {
                    Detail_RowUpdated(sender, new PXRowUpdatedEventArgs(e.Row, copy, false));
                    updated.Remove(e.Row);
                }
            }
        }


        protected virtual TaxDetail InitializeTaxDet(TaxDetail data)
        {
            return data;
        }

        /// <summary>Adds a tax to the specified detail line.</summary>
        /// <param name="detrow">The detail line.</param>
        /// <param name="taxitem">The tax item.</param>
        protected virtual void AddOneTax(Detail detrow, ITaxDetail taxitem)
        {
            if (taxitem != null)
            {
                object origTaxdet;
                object origDetail = Details.Cache.GetMain(detrow);
                PXCache taxCache = Base.Caches[GetTaxDetailMapping().Table];
                TaxParentAttribute.NewChild(taxCache, origDetail, origDetail.GetType(), out origTaxdet);
                TaxDetail newdet = Taxes.Cache.GetExtension<TaxDetail>(origTaxdet);
                newdet.RefTaxID = taxitem.TaxID;
                newdet = InitializeTaxDet(newdet);
                TaxDetail insdet = (TaxDetail)Insert(Taxes.Cache, newdet);
                origTaxdet = Taxes.Cache.GetMain(insdet);
                if (insdet != null) PXParentAttribute.SetParent(taxCache, origTaxdet, origDetail.GetType(), origDetail);
            }
        }

        public virtual ITaxDetail MatchesCategory(Detail row, ITaxDetail zoneitem)
        {
            string taxcat = row.TaxCategoryID; ;
            string taxid = row.TaxID;
            DateTime? docdate = CurrentDocument.DocumentDate;

            TaxRev rev = PXSelect<TaxRev,
                Where<TaxRev.taxID, Equal<Required<TaxRev.taxID>>,
                And<Required<TaxRev.startDate>, Between<TaxRev.startDate, TaxRev.endDate>,
                And<TaxRev.outdated, Equal<False>>>>>.Select(Base, zoneitem.TaxID, docdate);

            if (rev == null)
            {
                return null;
            }

            if (string.Equals(taxid, zoneitem.TaxID))
            {
                return zoneitem;
            }

            TaxCategory cat = PXSelect<TaxCategory, Where<TaxCategory.taxCategoryID, Equal<Required<TaxCategory.taxCategoryID>>>>.Select(Base, taxcat);

            if (cat == null)
            {
                return null;
            }
            return MatchesCategory(row, new[] { zoneitem }).FirstOrDefault();
        }

        public virtual IEnumerable<ITaxDetail> MatchesCategory(Detail row, IEnumerable<ITaxDetail> zonetaxlist)
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


        protected abstract IEnumerable<ITaxDetail> ManualTaxes(Detail row);

        /// <exclude />
        protected virtual void DefaultTaxes(Detail row, bool DefaultExisting)
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

            string taxID;
            if ((taxID = row.TaxID) != null)
            {
                AddOneTax(row, new TaxZoneDet { TaxID = taxID });
                applicableTaxes.Add(taxID);
            }

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
        protected virtual void DefaultTaxes(Detail row)
        {
            DefaultTaxes(row, true);
        }


        protected List<object> SelectTaxes(object row, PXTaxCheck taxchk)
        {
            return SelectTaxes<Where<True, Equal<True>>>(Base, row, taxchk);
        }

        protected abstract List<Object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            where Where : IBqlWhere, new();

        protected abstract List<Object> SelectDocumentLines(PXGraph graph, object row);

        protected Tax AdjustTaxLevel(Tax taxToAdjust)
        {
            if (IsTaxCalcModeEnabled && taxToAdjust.TaxCalcLevel != CSTaxCalcLevel.CalcOnItemAmtPlusTaxAmt)
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
            PXCache cache = Taxes.Cache;            
            foreach (object taxrow in SelectTaxes(row, PXTaxCheck.Line))
            {
                Delete(cache, ((PXResult)taxrow)[0]);
            }
        }

        private decimal Sum(List<object> list, Type field)
        {
            decimal ret = 0.0m;
            list.ForEach(a => ret += (decimal?)Taxes.Cache.GetValue(Taxes.Cache.GetExtension<TaxDetail>(((PXResult)a)[0]), field.Name) ?? 0m);
            return ret;
        }

        /// <exclude />
        protected virtual void AddTaxTotals(string taxID, object row)
        {
            PXCache cache = TaxTotals.Cache;
            TaxTotal newdet = (TaxTotal)cache.CreateInstance();
            newdet.RefTaxID = taxID;
            object insdet = Insert(cache, newdet);
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

        /// <summary>Sets the value of the</summary>
        protected virtual void SetTaxAmt(object row, decimal? value)
        {
        }

		protected virtual bool IsExemptTaxCategory(object row)
		{
			return IsExemptTaxCategory(Details.Cache, row);
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

        protected abstract decimal? GetTaxableAmt(object row);

        protected abstract decimal? GetTaxAmt(object row);

        protected List<object> SelectInclusiveTaxes(object row)
        {
            List<object> res = new List<object>();

			if (IsExemptTaxCategory(row))
			{
				return res;
			}

            string calcMode = IsTaxCalcModeEnabled ? GetTaxCalcMode() : TaxCalculationMode.TaxSetting;

            if (!IsTaxCalcModeEnabled || calcMode == TaxCalculationMode.TaxSetting)
            {
                res = SelectTaxes<Where<
                        Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                        And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                        And<Tax.directTax, Equal<False>>>>>(Base, row, PXTaxCheck.Line);
            }
            else if (calcMode == TaxCalculationMode.Gross)
            {
                res = SelectTaxes<Where<
                        Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
                        And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                        And<Tax.directTax, Equal<False>>>>>(Base, row, PXTaxCheck.Line);
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

        private void TaxSetLineDefault(object taxrow, Detail row)
        {
            if (taxrow == null)
            {
                throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
            }

            PXCache cache = Taxes.Cache;

            TaxDetail taxdet = cache.GetExtension<TaxDetail>(((PXResult)taxrow)[0]);
            Tax tax = PXResult.Unwrap<Tax>(taxrow);
            TaxRev taxrev = PXResult.Unwrap<TaxRev>(taxrow);

            decimal CuryTranAmt = GetCuryTranAmt(Details.Cache, row) ?? 0m;

            if (taxrev.TaxID == null)
            {
                taxrev.TaxableMin = 0m;
                taxrev.TaxableMax = 0m;
                taxrev.TaxRate = 0m;
            }

            Terms terms = SelectTerms();
            List<object> incltaxes = SelectInclusiveTaxes(row);

            decimal InclRate = SumWithReverseAdjustment(incltaxes, typeof(TaxRev), typeof(TaxRev.taxRate));

            decimal CuryInclTaxAmt = SumWithReverseAdjustment(incltaxes, GetTaxDetailMapping().Table, GetTaxDetailMapping().CuryTaxAmt);

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

                    decimal CuryLevel1TaxAmt = SumWithReverseAdjustment(lvl1Taxes, GetTaxDetailMapping().Table, GetTaxDetailMapping().CuryTaxAmt);

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
                if (GetTaxDetailMapping().CuryOrigTaxableAmt != typeof(TaxDetail.curyOrigTaxableAmt))
                {
                    cache.SetValue<TaxDetail.curyOrigTaxableAmt>(taxdet, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt));
                }

				AdjustMinMaxTaxableAmt(taxrev, ref CuryTaxableAmt, ref TaxableAmt);

				CuryTaxAmt = CuryTaxableAmt * (decimal)taxrev.TaxRate / 100;

                if (tax.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxAmount)
                {
                    CuryTaxAmt = CuryTaxAmt * (1 - (DiscPercent ?? terms.DiscPercent ?? 0m) / 100);
                }

            }
            else if (tax.TaxCalcType != CSTaxCalcType.Item)
            {
                //CuryTaxAmt = 0.0m;
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
                cache.Update(taxdet);
                if (tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive &&
                    (Details.Cache.GetStatus(row) == PXEntryStatus.Notchanged
                    || Details.Cache.GetStatus(row) == PXEntryStatus.Held))
                {
                    Details.Cache.SetStatus(row, PXEntryStatus.Updated);
                }
            }
            else
            {
                Delete(cache, taxdet);
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

        #region CiryOrigDiscAmt TaxRecalculation

        /// <exclude />
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

        private readonly Dictionary<object, bool> OrigDiscAmtExtCallDict = new Dictionary<object, bool>();
	    private readonly Dictionary<object, decimal?> DiscPercentsDict = new Dictionary<object, decimal?>();

        /// <summary>The FieldUpdated2 event handler for the <see cref="Document.CuryDiscAmt" /> field.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.FieldUpdated<Document, Document.curyDiscAmt> e)
        {
            if (e.Row == null)
                return;

            OrigDiscAmtExtCallDict[e.Row] = e.ExternalCall;
        }

        /// <summary>The RowUpdated event handler for the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void _(Events.RowUpdated<Document> e)
        {
            if (e.Row == null)
                return;

            bool externallCall;
            OrigDiscAmtExtCallDict.TryGetValue(e.Row, out externallCall);
            if (!externallCall)
                return;

            decimal newDiscAmt = e.Row.CuryDiscAmt.GetValueOrDefault();
            decimal oldDiscAmt = e.OldRow.CuryDiscAmt.GetValueOrDefault();

            if (newDiscAmt != oldDiscAmt && !DiscPercentsDict.ContainsKey(e.Row))
            {
                DiscPercentsDict.Add(e.Row, 0m);
                PXFieldUpdatedEventArgs args = new PXFieldUpdatedEventArgs(e.Row, oldDiscAmt, false);

                using (new TermsAttribute.UnsubscribeCalcDiscScope(e.Cache))
                {
                    try
                    {
                        if (newDiscAmt == 0m)
                            return;

                        ParentFieldUpdated(e.Cache, args);
                        DiscPercentsDict[e.Row] = null;

                        decimal reducedTaxAmt = 0m;
                        PXCache cache = TaxTotals.Cache;

                        foreach (object taxitem in SelectTaxes(e.Row, PXTaxCheck.RecalcTotals))
                        {
                            object taxsum = ((PXResult)taxitem)[0];
                            Tax tax = PXResult.Unwrap<Tax>(taxitem);

                            if (tax?.TaxCalcLevel != CSTaxCalcLevel.Inclusive &&
                                tax?.TaxApplyTermsDisc == CSTaxTermsDiscount.ToTaxableAmount)
                            {
                                reducedTaxAmt += (tax.ReverseTax == true ? -1m : 1m) * (((decimal?)cache.GetValue<TaxTotal.curyTaxAmt>(taxsum)).GetValueOrDefault() +
                                    (tax.DeductibleVAT == true ? ((decimal?)cache.GetValue<TaxTotal.curyExpenseAmt>(taxsum)).GetValueOrDefault() : 0m));
                            }
                        }

                        if (reducedTaxAmt != 0m)
                        {
                            decimal CuryDocBal = e.Row.CuryDocBal.GetValueOrDefault();
                            Pair<double, double> result = SolveQuadraticEquation((double)reducedTaxAmt, -(double)CuryDocBal, (double)newDiscAmt);

                            DiscPercentsDict[e.Row] = result?.first >= 0 && result?.first <= 1
                                ? (decimal)Math.Round(result.first * 100, 2)
                                : result?.second >= 0 && result?.second <= 1
                                    ? (decimal)Math.Round(result.second * 100, 2)
                                    : (decimal?)null;
                        }
                    }
                    catch
                    {
                        DiscPercentsDict[e.Row] = null;
                    }
                    finally
                    {
                        ParentFieldUpdated(e.Cache, args);
                        e.Cache.RaiseRowUpdated(e.Row, e.OldRow);

                        OrigDiscAmtExtCallDict.Remove(e.Row);
                        DiscPercentsDict.Remove(e.Row);
                    }
                }
            }
        }

        #endregion

        protected virtual void AdjustTaxableAmount(object row, List<object> taxitems, ref decimal CuryTaxableAmt, string TaxCalcType)
        {
        }


        protected virtual TaxTotal CalculateTaxSum(object taxrow, object row)
        {
            if (taxrow == null)
            {
                throw new PXArgumentException("taxrow", ErrorMessages.ArgumentNullException);
            }

            PXCache cache = Taxes.Cache;
            PXCache sumcache = TaxTotals.Cache;

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
            decimal CuryTaxAmtSumm = 0m;
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
                if (GetTaxDetailMapping().CuryOrigTaxableAmt != typeof(TaxDetail.curyOrigTaxableAmt))
                {
                    curyOrigTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyOrigTaxableAmt));
                }

                CuryTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyTaxableAmt));

                AdjustTaxableAmount(row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

                CuryTaxAmt = CuryTaxAmtSumm = Sum(taxitems, typeof(TaxDetail.curyTaxAmt));

                CuryExpenseAmt = Sum(taxitems, typeof(TaxDetail.curyExpenseAmt));
            }
            else if (tax.TaxType != CSTaxType.Withholding && (
                CalcGrossOnDocumentLevel && IsTaxCalcModeEnabled && GetTaxCalcMode() == TaxCalculationMode.Gross ||
                tax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && (!IsTaxCalcModeEnabled || GetTaxCalcMode() != TaxCalculationMode.Net)))
            {
                CuryTaxableAmt = Sum(taxitems, typeof(TaxDetail.curyTaxableAmt));
                CuryTaxAmt = CuryTaxAmtSumm = Sum(taxitems, typeof(TaxDetail.curyTaxAmt));

                var docLines = SelectDocumentLines(Base, row);
                if (docLines.Any())
                {
                    var docLineCache = docLines.Any() ? Base.Caches[docLines[0].GetType()] : Details.Cache;
                    var realLineAmounts = docLines.ToDictionary(
                        _ => (int)docLineCache.GetValue(_, "LineNbr"),
                        _ => GetCuryTranAmt(Details.Cache, _) ?? 0.0m);

                    List<object> alltaxitems = SelectTaxes(row, PXTaxCheck.RecalcLine);
                    var taxLines = alltaxitems.Select(_ => new
                    {
                        LineNbr = (int)cache.GetValue(((PXResult)_)[0], "LineNbr"),
                        TaxID = PXResult.Unwrap<Tax>(_).TaxID,
                        TaxRate = PXResult.Unwrap<TaxRev>(_).TaxRate ?? 0,
                        TaxRateMultiplier = PXResult.Unwrap<Tax>(_).ReverseTax == true ? -1.0M : 1.0M
                    });

                    var currentTaxRate = (taxdet.TaxRate != 0.0m) ? taxdet.TaxRate : taxLines.First(_ => _.TaxID == taxdet.RefTaxID).TaxRate;
                    var currentTaxLines = taxLines.Where(_ => _.TaxID == taxdet.RefTaxID).Select(_ => _.LineNbr).ToList();

                    var groups = new List<InclusiveTaxGroup>();
                    foreach (var lineNbr in currentTaxLines)
                    {
                        var lineTaxes = taxLines.Where(_ => _.LineNbr == lineNbr).OrderBy(_ => _.TaxID).ToList();
                        var groupKey = string.Join("::", lineTaxes.Select(_ => _.TaxID));
                        var sumTaxRate = lineTaxes.Sum(_ => _.TaxRate);
                        var lineAmt = realLineAmounts[lineNbr];
                        if (groups.Any(g => g.Key == groupKey))
                        {
                            groups.Single(g => g.Key == groupKey).TotalAmount += lineAmt;
                        }
                        else
                        {
                            groups.Add(new InclusiveTaxGroup() { Key = groupKey, Rate = sumTaxRate, TotalAmount = lineAmt });
                        }
                    }

                    CuryTaxAmt = groups.Sum(g => Base.FindImplementation<IPXCurrencyHelper>().RoundCury(
                        (g.TotalAmount / (1 + g.Rate / 100.0m) * currentTaxRate / 100.0m) ?? 0.0m));

                    CuryTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(
                        groups.Sum(g => g.TotalAmount / (1 + g.Rate / 100.0m)));
                }

                if (tax.DeductibleVAT == true)
                    CuryExpenseAmt = CuryTaxAmt * (1.0M - (taxrev.NonDeductibleTaxRate ?? 0.0M) / 100.0M);
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

                    CuryTaxAmtSumm = Sum(taxitems, typeof(TaxDetail.curyTaxAmt));
                    CuryTaxAmtSumm = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmtSumm);

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

                if (CuryTaxAmt != CuryTaxAmtSumm)
                {
                    var discrepancy = CuryTaxAmtSumm - CuryTaxAmt;
                }

                AdjustExpenseAmt(tax, taxrev, CuryTaxAmt, ref CuryExpenseAmt);
				AdjustTaxAmtOnDiscount(tax, ref CuryTaxAmt);
			}

			CuryExemptedAmt = Sum(taxitems, typeof(TaxDetail.curyExemptedAmt));

            taxdet = (TaxTotal)sumcache.CreateCopy(taxdet);

            if (GetTaxTotalMapping().CuryOrigTaxableAmt != typeof(TaxTotal.curyOrigTaxableAmt))
            {
                taxdet.CuryOrigTaxableAmt = curyOrigTaxableAmt;
            }

            taxdet.TaxRate = taxrev.TaxRate;
            taxdet.NonDeductibleTaxRate = taxrev.NonDeductibleTaxRate;
            taxdet.CuryTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxableAmt);
            taxdet.CuryExemptedAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryExemptedAmt);
            taxdet.CuryTaxAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmt);
            taxdet.CuryTaxAmtSumm = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryTaxAmtSumm);
            taxdet.CuryExpenseAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(CuryExpenseAmt);
            if (tax.DeductibleVAT == true && tax.TaxCalcType != CSTaxCalcType.Item)
            {
                taxdet.CuryTaxAmt = taxdet.CuryTaxAmt - taxdet.CuryExpenseAmt;
            }

            return taxdet;
        }

		protected virtual void CalculateTaxSumTaxAmt(
			TaxTotal taxdet,
			Tax tax,
			TaxRev taxrev)
		{
			PXCache sumcache = TaxTotals.Cache;

			decimal taxableAmt = 0.0m;
			decimal curyExpenseAmt = 0.0m;

			decimal curyTaxableAmt = (decimal)sumcache.GetValue(taxdet, typeof(TaxTotal.curyTaxableAmt).Name);
			decimal curyOrigTaxableAmt = Base.FindImplementation<IPXCurrencyHelper>().RoundCury(curyTaxableAmt);

			decimal taxRate = taxrev.TaxRate ?? 0;

			AdjustMinMaxTaxableAmt(taxrev, ref curyTaxableAmt, ref taxableAmt);

			decimal curyTaxAmt = curyTaxableAmt * taxRate / 100;

			AdjustExpenseAmt(tax, taxrev, curyTaxAmt, ref curyExpenseAmt);
			AdjustTaxAmtOnDiscount(tax, ref curyTaxAmt);

			if (GetTaxTotalMapping().CuryOrigTaxableAmt != typeof(TaxTotal.curyOrigTaxableAmt))
			{
				sumcache.SetValue(taxdet, typeof(TaxTotal.curyOrigTaxableAmt).Name, curyOrigTaxableAmt);
			}

			sumcache.SetValue(taxdet, typeof(TaxTotal.taxRate).Name, taxRate);
			sumcache.SetValue(taxdet, typeof(TaxTotal.nonDeductibleTaxRate).Name, taxrev.NonDeductibleTaxRate);
			sumcache.SetValue(taxdet, typeof(TaxTotal.curyTaxableAmt).Name, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(curyTaxableAmt));
			sumcache.SetValue(taxdet, typeof(TaxTotal.curyTaxAmt).Name, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(curyTaxAmt));
			sumcache.SetValue(taxdet, typeof(TaxTotal.curyExpenseAmt).Name, Base.FindImplementation<IPXCurrencyHelper>().RoundCury(curyExpenseAmt));

			if (tax.DeductibleVAT == true && tax.TaxCalcType != CSTaxCalcType.Item)
			{
				sumcache.SetValue(taxdet, typeof(TaxTotal.curyTaxAmt).Name, taxdet.CuryTaxAmt - taxdet.CuryExpenseAmt);
			}
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

            PXCache sumcache = TaxTotals.Cache;
            TaxTotal taxSum = CalculateTaxSum(taxrow, row);

            if (taxSum != null)
            {
                return (TaxTotal)sumcache.Update(taxSum);
            }
            TaxTotal taxdet = TaxTotals.Cache.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
            Delete(sumcache, taxdet);
            return null;
        }

        protected virtual void CalcTaxes(object row)
        {
            CalcTaxes(row, PXTaxCheck.RecalcLine);
        }

		protected virtual void RecalcTaxes()
		{
			Document row = Documents.Current;
			if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
			{
				_ParentRow = row;
				CalcTaxes(null);
				_ParentRow = null;
			}
			else if (row.TaxCalc == TaxCalc.ManualCalc)
			{
				_ParentRow = row;
				CalcTotals(null, false);
				_ParentRow = null;
			}
		}

        protected virtual object SelectParent(PXCache cache, object row)
        {
            return PXParentAttribute.SelectParent(cache, row, GetDetailMapping().Table);
        }

        protected virtual void CalcTaxes(object row, PXTaxCheck taxchk)
        {
            PXCache cache = Taxes.Cache;

            object detrow = row;

            foreach (object taxrow in SelectTaxes(row, taxchk))
            {
                if (row == null)
                {
                    detrow = SelectParent(cache, ((PXResult)taxrow)[0]);
                }

                if (detrow != null)
                {
                    TaxSetLineDefault(taxrow, Details.Cache.GetExtension<Detail>(detrow));
                }
            }
            CalcTotals(row, true);
        }

        protected virtual void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            _CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
        }

        protected virtual decimal CalcLineTotal(object row)
        {
            decimal CuryLineTotal = 0m;
            Detail orig_row = Details.Cache.GetExtension<Detail>(row);
            foreach (Detail detrow in Details.Select())
            {                
                CuryLineTotal += (Details.Cache.ObjectsEqual(detrow, orig_row) ? orig_row : detrow)?.CuryTranAmt ?? 0m;
            }
            /*
            object[] details = PXParentAttribute.SelectSiblings(Details.Cache, null);

            if (details != null)
            {
                foreach (object detrow in details)
                {
                    CuryLineTotal += Details.Cache.GetExtension<Detail>(Details.Cache.ObjectsEqual(detrow, row) ? row : detrow)?.CuryTranAmt ?? 0m;
                }
            }
            */
            return CuryLineTotal;
        }

        protected virtual decimal CalcDiscountLineTotal(object row)
        {
            decimal DiscountLineTotal = 0m;
            Detail orig_row = Details.Cache.GetExtension<Detail>(row);
            foreach (Detail detrow in Details.Select())
            {
                DiscountLineTotal += (Details.Cache.ObjectsEqual(detrow, orig_row) ? orig_row : detrow)?.CuryTranDiscount ?? 0m;
            }
            /*
            object[] details = PXParentAttribute.SelectSiblings(Details.Cache, null);

            if (details != null)
            {
                foreach (object detrow in details)
                {
                    CuryLineTotal += Details.Cache.GetExtension<Detail>(Details.Cache.ObjectsEqual(detrow, row) ? row : detrow)?.CuryTranAmt ?? 0m;
                }
            }
            */
            return DiscountLineTotal;
        }

        protected virtual decimal CalcTranExtPriceTotal(object row)
        {
            decimal TranExtPriceTotal = 0m;
            Detail orig_row = Details.Cache.GetExtension<Detail>(row);
            foreach (Detail detrow in Details.Select())
            {
                TranExtPriceTotal += (Details.Cache.ObjectsEqual(detrow, orig_row) ? orig_row : detrow)?.CuryTranExtPrice ?? 0m;
            }
            /*
            object[] details = PXParentAttribute.SelectSiblings(Details.Cache, null);

            if (details != null)
            {
                foreach (object detrow in details)
                {
                    CuryLineTotal += Details.Cache.GetExtension<Detail>(Details.Cache.ObjectsEqual(detrow, row) ? row : detrow)?.CuryTranAmt ?? 0m;
                }
            }
            */
            return TranExtPriceTotal;
        }

        /// <exclude />
        protected virtual void _CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
        {
            decimal CuryLineTotal = CalcLineTotal(row);
            decimal DiscountLineTotal = CalcDiscountLineTotal(row);
            decimal TranExtPriceTotal = CalcTranExtPriceTotal(row);

            decimal CuryDocTotal = CuryLineTotal + CuryTaxTotal - CuryInclTaxTotal;

            decimal doc_CuryLineTotal = CurrentDocument.CuryLineTotal ?? 0m;
            decimal doc_CuryTaxTotal = CurrentDocument.CuryTaxTotal ?? 0m;

            if (Equals(CuryLineTotal, doc_CuryLineTotal) == false ||
                Equals(CuryTaxTotal, doc_CuryTaxTotal) == false)
            {
                ParentSetValue<Document.curyLineTotal>(CuryLineTotal);
                ParentSetValue<Document.curyDiscountLineTotal>(DiscountLineTotal);
                ParentSetValue<Document.curyExtPriceTotal>(TranExtPriceTotal);
                ParentSetValue<Document.curyTaxTotal>(CuryTaxTotal);
                if (GetDocumentMapping().CuryDocBal != typeof(Document.curyDocBal))
                {
                    ParentSetValue<Document.curyDocBal>(CuryDocTotal);
                    return;
                }
            }

            if (GetDocumentMapping().CuryDocBal != typeof(Document.curyDocBal))
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
                    taxdet = TaxTotals.Cache.GetExtension<TaxTotal>(((PXResult)taxrow)[0]);
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
                Documents.Cache.RaiseExceptionHandling<Document.curyTaxTotal>(CurrentDocument, CuryTaxTotal,
                    new PXSetPropertyException(Messages.UseTaxExcludedFromTotals, PXErrorLevel.Warning));
            }
        }


        protected virtual PXEntryStatus ParentGetStatus()
        {
            return Documents.Cache.GetStatus(CurrentDocument);
        }

        protected virtual void ParentSetValue(string fieldname, object value)
        {
            PXCache cache = Documents.Cache;

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
            return Documents.Cache.GetValue(CurrentDocument, fieldname);
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

        /// <summary>The RowInserted event handler for the <see cref="Detail" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        public virtual void Detail_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
        {
            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc != TaxCalc.NoCalc && taxCalc != TaxCalc.ManualLineCalc)
            {
                object copy;
                if (!inserted.TryGetValue(e.Row, out copy))
                {
                    inserted[e.Row] = sender.CreateCopy(e.Row);
                }
            }

            Detail row = sender.GetExtension<Detail>(e.Row);
            if (row.TaxCategoryID == null && row.CuryTranAmt.GetValueOrDefault() == 0m)
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

		/// <summary>The RowUpdated event handler</summary>
		/// <param name="sender">The cache object that raised the event.</param>
		/// <param name="e">Parameters of the event.</param>
		public virtual void Detail_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)		
		{			
            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc != TaxCalc.NoCalc && taxCalc != TaxCalc.ManualLineCalc)
            {
                object copy;
                if (!updated.TryGetValue(e.Row, out copy))
                {
                    updated[e.Row] = sender.CreateCopy(e.Row);
                }
            }
            Detail row = sender.GetExtension<Detail>(e.Row);
            Detail oldRow = sender.GetExtension<Detail>(e.OldRow);

            if (taxCalc == TaxCalc.Calc)
            {
                if (!Equals(oldRow.TaxCategoryID, row.TaxCategoryID))
                {
                    Preload();
                    ReDefaultTaxes(oldRow, row);
                }
                else if (!Equals(oldRow.TaxID, row.TaxID))
                {
                    PXCache cache = Taxes.Cache;
                    TaxDetail taxDetail = (TaxDetail)cache.CreateInstance();
                    taxDetail.RefTaxID = oldRow.TaxID;
                    DelOneTax(row, taxDetail.RefTaxID);
                    AddOneTax(row, new TaxZoneDet { TaxID = row.TaxID });
                }

                bool calculated = false;

                if (!Equals(oldRow.TaxCategoryID, row.TaxCategoryID) ||
                    !Equals(oldRow.CuryTranAmt, row.CuryTranAmt) ||
					!Equals(oldRow.TaxID, row.TaxID))
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

        /// <summary>The RowDeleted event handler for the <see cref="Detail" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        public virtual void Detail_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)

        {
            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;

            PXEntryStatus parentStatus = ParentGetStatus();
            if (parentStatus == PXEntryStatus.Deleted || parentStatus == PXEntryStatus.InsertedDeleted) return;
            Detail row = sender.GetExtension<Detail>(e.Row);

            decimal? val;
            if (row.TaxCategoryID == null && ((val = row.CuryTranAmt) == null || val == 0m))
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

        /// <summary>The RowPersisted event handler.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
        {
            if (e.TranStatus == PXTranStatus.Completed)
            {
                inserted?.Clear();
                updated?.Clear();
            }
        }


        /// <exclude />
        protected Document _ParentRow;

        /// <summary>The RowUpdated event handler for the CurrencyInfo DAC.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        {
            TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
            if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
            {
                if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
                {
                    PXView siblings = Base.FindImplementation<IPXCurrencyHelper>().GetView(GetDetailMapping().Table, GetDetailMapping().CuryInfoID);
                    if (siblings != null && siblings.SelectSingle() != null)
                    {
                        CalcTaxes(null);
                    }
                }
            }
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.CuryID" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_CuryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ParentFieldUpdated(sender, e);
        }
        /// <summary>The FieldUpdated event handler for the <see cref="Document.TermsID" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_TermsID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            ParentFieldUpdated(sender, e);
        }

        protected virtual void ParentFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Document row = sender.GetExtension<Document>(e.Row);
            if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
            {
                //TODO: Check it later.
                if (e.Row.GetType() == sender.GetItemType())
                {
                    _ParentRow = row;
                }
                CalcTaxes(null);
                _ParentRow = null;
            }
        }

	    /// <summary>The FieldUpdated event handler for the <see cref="Document.IsTaxSaved" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_IsTaxSaved_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Document row = sender.GetExtension<Document>(e.Row);
            CalcDocTotals(row, row.CuryTaxTotal.GetValueOrDefault(), 0, row.CuryWhTaxTotal.GetValueOrDefault());
        }

        protected virtual List<object> ChildSelect(PXCache cache, object data)
        {
            return TaxParentAttribute.ChildSelect(cache, data, GetDocumentMapping().Table);
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.TaxZoneID" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_TaxZoneID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
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
                    ReDefaultTaxes(Details.Select());

                    _ParentRow = row;
                    CalcTaxes(null);
                    _ParentRow = null;
                }
            }
        }

        protected virtual void ReDefaultTaxes(PXResultset<Detail> details)
        {
            foreach (Detail det in details)
            {
                ClearTaxes(det);
                ClearChildTaxAmts(det);
            }

            foreach (Detail det in details)
            {
                DefaultTaxes(det, false);
            }
        }

        protected virtual void ClearChildTaxAmts(Detail childRow)
        {
            PXCache childCache = Details.Cache;
            SetTaxableAmt(childRow, 0);
            SetTaxAmt(childRow, 0);
            if (childCache.Locate(childRow) != null && //if record is not in cache then it is just being inserted - no need for manual update
                (childCache.GetStatus(childRow) == PXEntryStatus.Notchanged
                || childCache.GetStatus(childRow) == PXEntryStatus.Held))
            {
                childCache.Update(childRow);
            }
        }

        protected virtual void ReDefaultTaxes(Detail clearDet, Detail defaultDet, bool defaultExisting = true)
        {
            ClearTaxes(clearDet);
            ClearChildTaxAmts(clearDet);
            DefaultTaxes(defaultDet, defaultExisting);
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.DocumentDate" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_DocumentDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Document row = sender.GetExtension<Document>(e.Row);
            if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
            {
                Preload();

                foreach (Detail det in Details.Select())
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

        /// <summary>The FieldUpdated event handler for the <see cref="Document.FinPeriodID" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_FinPeriodID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Document row = sender.GetExtension<Document>(e.Row);
            if (row.TaxCalc == TaxCalc.Calc || row.TaxCalc == TaxCalc.ManualLineCalc)
            {
                PXCache cache = TaxTotals.Cache;
                List<object> details = TaxParentAttribute.ChildSelect(cache, e.Row, sender.GetItemType());
                foreach (object det in details)
                {
                    if (cache.GetStatus(det) == PXEntryStatus.Notchanged || cache.GetStatus(det) == PXEntryStatus.Held)
                    {
                        cache.SetStatus(det, PXEntryStatus.Updated);
                    }
                }
            }
        }

        protected abstract void SetExtCostExt(PXCache sender, object child, decimal? value);

        protected abstract string GetExtCostLabel(PXCache sender, object row);

        protected string GetTaxCalcMode()
        {
            if (!IsTaxCalcModeEnabled)
            {
                throw new PXException(Messages.DocumentTaxCalculationModeNotEnabled);
            }
            return (string)ParentGetValue<Document.taxCalcMode>();
        }

        /// <summary>The FieldUpdated event handler for the <see cref="Document.TaxCalcMode" /> field of the <see cref="Document" /> mapped cache extension.</summary>
        /// <param name="sender">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void Document_TaxCalcMode_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            Document row = sender.GetExtension<Document>(e.Row);
            string newValue = row.TaxCalcMode;
            if (newValue != (string)e.OldValue)
            {
                PXCache cache = Details.Cache;
                PXResultset<Detail> details = Details.Select();

                var supressAmountRecalculation = CalcGrossOnDocumentLevel;
                decimal? taxTotal = row.CuryTaxTotal; ;
                PXView view = sender.Graph.Views[sender.Graph.PrimaryView];
                if (details != null && details.Count != 0 && !supressAmountRecalculation)
                {
                    string askMessage = PXLocalizer.LocalizeFormat(Messages.RecalculateExtCost, GetExtCostLabel(cache, details[0]));
                    if (taxTotal.HasValue && taxTotal.Value != 0 && view.Ask(askMessage, MessageButtons.YesNo) == WebDialogResult.Yes)
                    {
                        PXCache taxDetCache = Taxes.Cache;
                        foreach (PXResult<Detail> det in details)
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
                foreach (Detail det in details)
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

        protected class InclusiveTaxGroup
        {
            public string Key { get; set; }
            public decimal Rate { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private TaxDetail TaxSummarizeOneLine(object row, SummType summType)
        {
            List<object> taxitems = new List<object>();
            switch (summType)
            {
                case SummType.All:
                    if (CalcGrossOnDocumentLevel && IsTaxCalcModeEnabled)
                    {
                        taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.directTax, Equal<False>>>>>(Base, row, PXTaxCheck.Line);
                    }
                    else
                    {
                        taxitems = SelectTaxes<Where<Tax.taxCalcLevel, NotEqual<CSTaxCalcLevel.calcOnItemAmtPlusTaxAmt>,
                            And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
                                And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                    And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line);
                    }
                    break;
                case SummType.Inclusive:
                    if (CalcGrossOnDocumentLevel && IsTaxCalcModeEnabled)
                    {
                        taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.directTax, Equal<False>>>>>(Base, row, PXTaxCheck.Line);
                    }
                    else
                    {
                        taxitems = SelectTaxes<Where<Tax.taxCalcLevel, Equal<CSTaxCalcLevel.inclusive>,
                            And<Tax.taxCalcType, Equal<CSTaxCalcType.item>,
                                And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                                    And<Tax.directTax, Equal<False>>>>>>(Base, row, PXTaxCheck.Line);
                    }
                    break;
            }

            if (taxitems.Count == 0) return null;

            PXCache taxDetCache = Taxes.Cache;
            TaxDetail taxLineSumDet = (TaxDetail)taxDetCache.CreateInstance();

            decimal? CuryTaxableAmt = taxDetCache.GetExtension<TaxDetail>(((PXResult)taxitems[0])[0]).CuryTaxableAmt;

            //AdjustTaxableAmount(sender, row, taxitems, ref CuryTaxableAmt, tax.TaxCalcType);

            decimal? CuryTaxAmt = SumWithReverseAdjustment(
                taxitems,
                GetTaxDetailMapping().Table, 
                GetTaxDetailMapping().CuryTaxAmt);

            decimal? CuryExpenseAmt = SumWithReverseAdjustment(
                taxitems,
                GetTaxDetailMapping().Table, 
                GetTaxDetailMapping().CuryExpenseAmt);
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

        /// <summary>The RowInserting event handler for the <see cref="TaxTotal" /> mapped cache extension.</summary>
        /// <param name="cache">The cache object that raised the event.</param>
        /// <param name="e">Parameters of the event.</param>
        protected virtual void TaxTotal_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            TaxTotal newdet = cache.GetExtension<TaxTotal>(e.Row);
            if (newdet == null) return;
            Dictionary<string, object> newdetKeys = GetKeyFieldValues(cache, newdet);
            bool insertNewTaxTran = true;

            if (ExternalTax.IsExternalTax(cache.Graph, newdet.TaxZoneID) != true)
            {
                foreach (object cacheddet in cache.Cached)
                {
                    Dictionary<string, object> cacheddetKeys = new Dictionary<string, object>();
                    cacheddetKeys = GetKeyFieldValues(cache, (TaxTotal)cacheddet);
                    bool recordsEqual = true;
                    PXEntryStatus status = cache.GetStatus(cacheddet);

                    if (status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
                    {
                        foreach (KeyValuePair<string, object> keyValue in newdetKeys)
                        {
                            if (cacheddetKeys.ContainsKey(keyValue.Key) && !Equals(cacheddetKeys[keyValue.Key], keyValue.Value))
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
        //TODO: Find what for
        /// <exclude />
        protected string _RecordID = "RecordID";
        private Dictionary<string, object> GetKeyFieldValues(PXCache extCache, TaxTotal extrow)
        {
	        object row = extCache.GetMain<TaxTotal>(extrow);
	        PXCache cache = extCache.Graph.Caches[row.GetType()];

            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            foreach (string key in cache.Keys)
            {
                if (key != _RecordID)
                    keyValues.Add(key, cache.GetValue(row, key));
            }
            return keyValues;
        }

        protected virtual void DelOneTax(Detail detrow, string taxID)
        {
            foreach (object rec in SelectTaxes(detrow, PXTaxCheck.Line))
            {
                TaxDetail taxdet = Taxes.Cache.GetExtension<TaxDetail>(((PXResult)rec)[0]);
                if (taxdet.RefTaxID == taxID)
                {
                    Taxes.Delete(taxdet);
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
            if (GetDetailMapping().CuryInfoID != typeof(Detail.curyInfoID))
                Base.RowUpdated.AddHandler<CurrencyInfo>(CurrencyInfo_RowUpdated);

            PXUIFieldAttribute.SetVisible<Tax.exemptTax>(Base.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.statisticalTax>(Base.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.reverseTax>(Base.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.pendingTax>(Base.Caches[typeof(Tax)], null, false);
            PXUIFieldAttribute.SetVisible<Tax.taxType>(Base.Caches[typeof(Tax)], null, false);

            inserted = new Dictionary<object, object>();
            updated = new Dictionary<object, object>();
        }

        /// <summary>The dictionary of inserted records.</summary>
        protected Dictionary<object, object> inserted;
        /// <summary>The dictionary of updated records.</summary>
        protected Dictionary<object, object> updated;

        protected virtual decimal? GetCuryTranAmt(PXCache sender, object row)
        {
            decimal? CuryTranAmt = (decimal?)sender.GetValue(row, typeof(Detail.curyTranAmt).Name) * (decimal?)sender.GetValue(row, typeof(Detail.groupDiscountRate).Name) * (decimal?)sender.GetValue(row, typeof(Detail.documentDiscountRate).Name);
			return sender.Graph.FindImplementation<IPXCurrencyHelper>().RoundCury((decimal)CuryTranAmt);
        }        

		protected virtual string GetTaxCategory(PXCache sender, object row)
		{
			return (string)sender.GetValue(row, typeof(Detail.taxCategoryID).Name);
		}

		protected virtual IComparer<Tax> GetTaxByCalculationLevelComparer() => TaxByCalculationLevelComparer.Instance;
	}
}

