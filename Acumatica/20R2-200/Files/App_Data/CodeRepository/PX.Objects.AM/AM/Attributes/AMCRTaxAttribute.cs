//using System;
//using System.Collections.Generic;
//using PX.Data;
//using PX.Objects.CR;
//using PX.Objects.CS;
//using PX.Objects.TX;

//namespace PX.Objects.AM.Attributes
//{
//    /// <summary>
//    /// Manufacturing extension similar to CRTaxAttribute to calc taxes on estimates.
//    /// Focus: Get the estimate numbers to roll up into the LineTotal with Products.
//    /// </summary>
//    public class AMCRTaxAttribute : TaxAttribute
//    {
//        public AMCRTaxAttribute(Type ParentType, Type TaxType, Type TaxSumType)
//            : base(ParentType, TaxType, TaxSumType)
//        {
//            CuryDocBal = typeof(CROpportunity.curyProductsAmount);
//            CuryLineTotal = typeof(CROpportunity.curyLineTotal);
//            DocDate = typeof(CROpportunity.closeDate);
//            CuryTranAmt = typeof(AMEstimateReference.curyExtPrice);
//        }

//        protected override decimal CalcLineTotal(PXCache sender, object row)
//        {
//            // Base value will by default calculate the total estimates
//            var baseValue = base.CalcLineTotal(sender, row);

//            //Calculate Total Products...
//            var productsValue = GetTotalCuryProducts(sender);

//            return baseValue + productsValue;
//        }

//        /// <summary>
//        /// Calculate the current products total amount
//        /// </summary>
//        protected virtual decimal GetTotalCuryProducts(PXCache sender)
//        {
//            bool isOpMaint = sender.Graph is OpportunityMaint;
//            if (!isOpMaint)
//            {
//                return 0;
//            }

//            decimal productsTotal = 0;

//            //foreach (CROpportunityProducts opportunityProducts in PXSelect<CROpportunityProducts,
//            //    Where<CROpportunityProducts.cROpportunityID, Equal<Current<CROpportunity.opportunityID>>>>.Select(
//            //    sender.Graph))
//            //{
//            //    productsTotal += opportunityProducts.CuryAmount.GetValueOrDefault();
//            //}

//            return productsTotal;
//        }

//        protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
//        {
//            Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
//            object[] currents = new object[] { row, ((OpportunityMaint)graph).Opportunity.Current };
//            foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
//                    LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
//                        And<TaxRev.outdated, Equal<boolFalse>,
//                            And<TaxRev.taxType, Equal<TaxType.sales>,
//                                And<Tax.taxType, NotEqual<CSTaxType.withholding>,
//                                    And<Tax.taxType, NotEqual<CSTaxType.use>,
//                                        And<Tax.reverseTax, Equal<boolFalse>,
//                                            And
//                                            <Current<CROpportunity.closeDate>, Between<TaxRev.startDate, TaxRev.endDate>
//                                            >>>>>>>>,
//                    Where>
//                .SelectMultiBound(graph, currents, parameters))
//            {
//                tail[((Tax)record).TaxID] = record;
//            }
//            List<object> ret = new List<object>();
//            switch (taxchk)
//            {
//                case PXTaxCheck.Line:
//                    foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
//                            Where<CROpportunityTax.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
//                                //Replaced for line to calc on estimates
//                                And<CROpportunityTax.lineNbr, Equal<Current<AMEstimateReference.taxLineNbr>>>>
//                        >.SelectMultiBound(graph, currents))
//                    {
//                        PXResult<Tax, TaxRev> line;
//                        if (tail.TryGetValue(record.TaxID, out line))
//                        {
//                            int idx;
//                            for (idx = ret.Count;
//                                (idx > 0)
//                                &&
//                                String.Compare(
//                                    ((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel,
//                                    ((Tax)line).TaxCalcLevel) > 0;
//                                idx--) ;
//                            ret.Insert(idx,
//                                new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
//                        }
//                    }
//                    return ret;
//                case PXTaxCheck.RecalcLine:
//                    foreach (CROpportunityTax record in PXSelect<CROpportunityTax,
//                            Where<CROpportunityTax.opportunityID, Equal<Current<CROpportunity.opportunityID>>,
//                                And<CROpportunityTax.lineNbr, Less<intMax>>>>
//                        .SelectMultiBound(graph, currents))
//                    {
//                        PXResult<Tax, TaxRev> line;
//                        if (tail.TryGetValue(record.TaxID, out line))
//                        {
//                            int idx;
//                            for (idx = ret.Count;
//                                (idx > 0)
//                                &&
//                                ((CROpportunityTax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1])
//                                    .LineNbr == record.LineNbr
//                                &&
//                                String.Compare(
//                                    ((Tax)(PXResult<CROpportunityTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel,
//                                    ((Tax)line).TaxCalcLevel) > 0;
//                                idx--) ;
//                            ret.Insert(idx,
//                                new PXResult<CROpportunityTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
//                        }
//                    }
//                    return ret;
//                case PXTaxCheck.RecalcTotals:
//                    foreach (CRTaxTran record in PXSelect<CRTaxTran,
//                            Where<CRTaxTran.opportunityID, Equal<Current<CROpportunity.opportunityID>>>>
//                        .SelectMultiBound(graph, currents))
//                    {
//                        PXResult<Tax, TaxRev> line;
//                        if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
//                        {
//                            int idx;
//                            for (idx = ret.Count;
//                                (idx > 0)
//                                &&
//                                String.Compare(((Tax)(PXResult<CRTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel,
//                                    ((Tax)line).TaxCalcLevel) > 0;
//                                idx--) ;
//                            ret.Insert(idx, new PXResult<CRTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
//                        }
//                    }
//                    return ret;
//                default:
//                    return ret;
//            }
//        }

//        public override void RowInserted(PXCache sender, PXRowInsertedEventArgs e)
//        {
//            base.RowInserted(sender, e);

//            if (TaxCalc == TaxCalc.ManualLineCalc)
//            {
//                DefaultTaxes(sender, e.Row);
//                CalcTaxes(sender, e.Row, PXTaxCheck.Line);
//            }
//        }

//        public override void RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
//        {
//            base.RowUpdated(sender, e);

//            if (TaxCalc == TaxCalc.ManualLineCalc)
//            {
//                CalcTaxes(sender, e.Row, PXTaxCheck.Line);
//            }
//        }

//        public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
//        {
//            //Test, confirm delete is working?
//            sender.Graph.Caches[_ParentType].Current = PXParentAttribute.SelectParent(sender, e.Row);
//            base.RowDeleted(sender, e);
//        }

//        public override void CacheAttached(PXCache sender)
//        {
//            if (sender.Graph is OpportunityMaint)
//            {
//                base.CacheAttached(sender);
//            }
//            else
//            {
//                TaxCalc = TaxCalc.NoCalc;
//            }
//        }
//    }
//}