using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class GenericTaxTranMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(GenericTaxTran);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        public GenericTaxTranMapping(Type table) { _table = table; }

        public Type TaxID = typeof(GenericTaxTran.taxID);

        public Type TaxRate = typeof(GenericTaxTran.taxRate);

        public Type CuryTaxableAmt = typeof(GenericTaxTran.curyTaxableAmt);

        public Type CuryTaxAmt = typeof(GenericTaxTran.curyTaxAmt);

        public Type CuryTaxAmtSumm = typeof(GenericTaxTran.curyTaxAmtSumm);

        public Type CuryExpenseAmt = typeof(GenericTaxTran.curyExpenseAmt);

        public Type NonDeductibleTaxRate = typeof(GenericTaxTran.nonDeductibleTaxRate);
    }
}
