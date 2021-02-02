using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class InvoiceMapping: IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Invoice);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        /// <summary>Creates the default mapping of the <see cref="DocumentWithLines" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public InvoiceMapping(Type table)
        {
            _table = table;
        }

        /// <exclude />
        public Type BranchID = typeof(Invoice.branchID);

        /// <exclude />
        public Type HeaderDocDate = typeof(Invoice.headerDocDate);

        /// <exclude />
        public Type HeaderTranPeriodID = typeof(Invoice.headerTranPeriodID);
    }
}
