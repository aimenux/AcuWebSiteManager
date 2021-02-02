using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Common.GraphExtensions.Abstract.DAC
{
	[PXHidden]
    /// <exclude />
    public class GenericTaxTran: PXMappedCacheExtension
    {
        public String TaxID { get; set; }
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

        public Decimal? TaxRate { get; set; }
        public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }

        public Decimal? CuryTaxableAmt { get; set; }
        public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

        public Decimal? CuryTaxAmt { get; set; }
        public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

        public Decimal? CuryTaxAmtSumm { get; set; }
        public abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }

        public Decimal? CuryExpenseAmt { get; set; }
        public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }

        public Decimal? NonDeductibleTaxRate { get; set; }
        public abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }
    }
}
