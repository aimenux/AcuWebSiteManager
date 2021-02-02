using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class Adjust2Mapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Adjust2);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;
        /// <summary>Creates the default mapping of the <see cref="Adjust2Mapping" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public Adjust2Mapping(Type table) { _table = table; }

        public Type AdjgBranchID = typeof(Adjust2.adjgBranchID);

        public Type AdjgFinPeriodID = typeof(Adjust2.adjgFinPeriodID);

        public Type AdjgTranPeriodID = typeof(Adjust2.adjgTranPeriodID);

        public Type AdjdBranchID = typeof(Adjust2.adjdBranchID);

        public Type AdjdFinPeriodID = typeof(Adjust2.adjdFinPeriodID);

        public Type AdjdTranPeriodID = typeof(Adjust2.adjdTranPeriodID);
    }
}
