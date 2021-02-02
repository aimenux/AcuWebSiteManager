using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class PaidInvoiceMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(PaidInvoice);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        public PaidInvoiceMapping(Type table)
        {
            _table = table;
        }

        /// <exclude />
        public Type BranchID = typeof(PaidInvoice.branchID);

        /// <exclude />
        public Type HeaderDocDate = typeof(PaidInvoice.headerDocDate);

        /// <exclude />
        public Type HeaderTranPeriodID = typeof(PaidInvoice.headerTranPeriodID);
    }
}
