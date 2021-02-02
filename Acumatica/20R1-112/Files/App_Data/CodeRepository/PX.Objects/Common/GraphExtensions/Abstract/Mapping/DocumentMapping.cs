using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class DocumentMapping: IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Document);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        /// <summary>Creates the default mapping of the <see cref="Document" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public DocumentMapping(Type table)
        {
            _table = table;
        }

        /// <exclude />
        public Type BranchID = typeof(Document.branchID);

        /// <exclude />
        public Type HeaderTranPeriodID = typeof(Document.headerTranPeriodID);

        /// <exclude />
        public Type HeaderDocDate = typeof(Document.headerDocDate);
    }
}
