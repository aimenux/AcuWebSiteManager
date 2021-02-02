using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class PaymentMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Payment);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        /// <summary>Creates the default mapping of the <see cref="DocumentWithLines" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public PaymentMapping(Type table)
        {
            _table = table;
        }

        public virtual Type BranchID => typeof(Payment.branchID);

        public virtual Type AdjDate => typeof(Payment.adjDate);

        public virtual Type AdjFinPeriodID => typeof(Payment.adjFinPeriodID);

        public virtual Type AdjTranPeriodID => typeof(Payment.adjTranPeriodID);
    }
}
