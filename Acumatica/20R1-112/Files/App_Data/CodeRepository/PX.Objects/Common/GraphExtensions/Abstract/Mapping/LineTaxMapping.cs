using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class LineTaxMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(LineTax);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        public LineTaxMapping(Type table) { _table = table; }

        public Type LineNbr = typeof(LineTax.lineNbr);

        public Type TaxID = typeof(LineTax.taxID);

        public Type TaxRate = typeof(LineTax.taxRate);

        public Type CuryTaxableAmt = typeof(LineTax.curyTaxableAmt);

        public Type CuryTaxAmt = typeof(LineTax.curyTaxAmt);

        public Type CuryExpenseAmt = typeof(LineTax.curyExpenseAmt);
    }
}
