using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class AdjustMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Adjust);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;
        /// <summary>Creates the default mapping of the <see cref="AdjustMapping" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public AdjustMapping(Type table) { _table = table; }

        public Type AdjgBranchID = typeof(Adjust.adjgBranchID);

        public Type AdjgFinPeriodID = typeof(Adjust.adjgFinPeriodID);

        public Type AdjgTranPeriodID = typeof(Adjust.adjgTranPeriodID);

        public Type AdjdBranchID = typeof(Adjust.adjdBranchID);

        public Type AdjdFinPeriodID = typeof(Adjust.adjdFinPeriodID);

        public Type AdjdTranPeriodID = typeof(Adjust.adjdTranPeriodID);
    }
}
