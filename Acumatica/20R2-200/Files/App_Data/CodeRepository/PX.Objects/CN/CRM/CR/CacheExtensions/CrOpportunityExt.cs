using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.CRM.CR.CacheExtensions
{
    public sealed class CrOpportunityExt : PXCacheExtension<CROpportunity>
    {
        [PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.cost))]
        [PXUIField(DisplayName = "Cost")]
        public decimal? Cost
        {
            get;
            set;
        }

        [PXBaseCury]
        [PXFormula(typeof(Sub<quotedAmount, cost>))]
        [PXUIField(DisplayName = "Gross Margin", Enabled = false)]
        public decimal? GrossMarginAbsolute
        {
            get;
            set;
        }

        [PXDecimal]
        [PXFormula(typeof(
            Switch<
                Case<Where<quotedAmount, NotEqual<decimal0>>,
                    Mult<
                        Div<
                            Sub<quotedAmount, cost>,
                            quotedAmount>,
                        decimal100>>,
                decimal0>))]
        [PXUIField(DisplayName = "Gross Margin %", Enabled = false)]
        public decimal? GrossMarginPercentage
        {
            get;
            set;
        }

        [PXDBBool(BqlField = typeof(CrStandaloneOpportunityExt.multipleAccounts))]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Multiple Customers")]
        public bool? MultipleAccounts
        {
            get;
            set;
        }

        [PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.quotedAmount))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Amount")]
        public decimal? QuotedAmount
        {
            get;
            set;
        }

        [PXDBBaseCury(BqlField = typeof(CrStandaloneOpportunityExt.totalAmount))]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Sub<quotedAmount, CROpportunity.curyDiscTot>))]
        [PXUIField(DisplayName = "Total", Enabled = false)]
        public decimal? TotalAmount
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class cost : BqlDecimal.Field<cost>
        {
        }

        public abstract class grossMarginAbsolute : BqlDecimal.Field<grossMarginAbsolute>
        {
        }

        public abstract class grossMarginPercentage : BqlDecimal.Field<grossMarginPercentage>
        {
        }

        public abstract class multipleAccounts : BqlBool.Field<multipleAccounts>
        {
        }

        public abstract class quotedAmount : BqlDecimal.Field<quotedAmount>
        {
        }

        public abstract class totalAmount : BqlDecimal.Field<totalAmount>
        {
        }
    }
}