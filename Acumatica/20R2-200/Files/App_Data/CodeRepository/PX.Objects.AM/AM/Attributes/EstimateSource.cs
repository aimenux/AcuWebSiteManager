using PX.Data;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Estimate Source Types
    /// </summary>
    public class EstimateSource
    {
        /// <summary>
        /// Estimate
        /// </summary>
        public const int Estimate = 0;
        /// <summary>
        /// Sales Order
        /// </summary>
        public const int SalesOrder = 1;
        /// <summary>
        /// Opportunity
        /// </summary>
        public const int Opportunity = 2;

        /// <summary>
        /// Descriptions/labels for identifiers
        /// </summary>
        public class Desc
        {
            public static string Estimate => Messages.GetLocal(Messages.Estimate);
            public static string SalesOrder => Messages.GetLocal(Messages.SalesOrder);
            public static string Opportunity => PX.Objects.CR.Messages.GetLocal(PX.Objects.CR.Messages.Opportunity);
        }

        public static string GetDescription(int? id)
        {
            if (id == null)
            {
                return string.Empty;
            }

            try
            {
                var x = new ListAttribute();
                return x.ValueLabelDic[id.GetValueOrDefault()];
            }
            catch
            {
                return string.Empty;
            }
        }

        public class estimate : PX.Data.BQL.BqlInt.Constant<estimate>
        {
            public estimate() : base(Estimate) { }
        }

        public class salesOrder : PX.Data.BQL.BqlInt.Constant<salesOrder>
        {
            public salesOrder() : base(SalesOrder) { }
        }

        public class opportunity : PX.Data.BQL.BqlInt.Constant<opportunity>
        {
            public opportunity() : base(Opportunity) { }
        }

        public class ListAttribute : PXIntListAttribute
        {
            public ListAttribute()
                : base(
                new int[] { Estimate, SalesOrder, Opportunity },
                new string[] { Messages.Estimate, Messages.SalesOrder, CR.Messages.Opportunity })
            { }
        }

        public static int GetEstimateQuoteSource(PXGraph graph)
        {
            if (graph is PX.Objects.SO.SOOrderEntry)
            {
                return SalesOrder;
            }

            if (graph is PX.Objects.CR.OpportunityMaint)
            {
                return Opportunity;
            }

            if (graph is PX.Objects.CR.QuoteMaint)
            {
                return Opportunity;
            }

            return Estimate;
        }
    }
}