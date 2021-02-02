using System;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.SO;

namespace PX.Objects.AM.Attributes
{
    public class SOOrderTaxAMExtensionAttribute : SOOrderTaxAttribute
    {
        public SOOrderTaxAMExtensionAttribute(Type ParentType, Type TaxType, Type TaxSumType)
            : base(ParentType, TaxType, TaxSumType)
        {
        }

        protected override void CalcDocTotals(PXCache sender, object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal, decimal CuryTaxDiscountTotal)
        {
            base.CalcDocTotals(sender, row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal, CuryTaxDiscountTotal);

            decimal CuryEstimatetotal = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

            if (CuryEstimatetotal == 0)
            {
                return;
            }

            decimal CuryDocTotal = CuryEstimatetotal + (decimal)(ParentGetValue<SOOrder.curyOrderTotal>(sender.Graph) ?? 0m);
            ParentSetValue<SOOrder.curyOrderTotal>(sender.Graph, CuryDocTotal);
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);

            if (sender.Graph is SOOrderEntry)
            {
                sender.Graph.FieldUpdated.AddHandler(typeof(SOOrder), "AMCuryEstimateTotal", SOOrder_AMCuryEstimateTotal_FieldUpdated);
            }
        }

        protected virtual void SOOrder_AMCuryEstimateTotal_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            var row = (SOOrder)e.Row;
            if (row == null)
            {
                return;
            }

            var rowStatus = sender.GetStatus(row);
            if (rowStatus == PXEntryStatus.Deleted || rowStatus == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            var oldValue = (decimal)(e.OldValue ?? 0m);
            var newValue = (decimal)(ParentGetValue<SOOrderExt.aMCuryEstimateTotal>(sender.Graph) ?? 0m);

            var diff = newValue - oldValue;

            if (diff == 0)
            {
                return;
            }

            decimal? curyTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryTaxTotal);
            decimal? curyWhTaxTotal = (decimal?)sender.GetValue(e.Row, _CuryWhTaxTotal);
            _ParentRow = row;
            CalcDocTotals(sender, e.Row, curyTaxTotal.GetValueOrDefault(), 0, curyWhTaxTotal.GetValueOrDefault(), 0m);
            _ParentRow = null;
        }
    }
}