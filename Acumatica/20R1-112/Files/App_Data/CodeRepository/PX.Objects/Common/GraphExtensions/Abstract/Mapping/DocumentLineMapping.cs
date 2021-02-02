using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    /// <summary>Defines the default mapping of the <see cref="DocumentLine" /> mapped cache extension to a DAC.</summary>
    public class DocumentLineMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(DocumentLine);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;
        /// <summary>Creates the default mapping of the <see cref="DocumentLine" /> mapped cache extension to the specified table.</summary>
        /// <param name="table">A DAC.</param>
        public DocumentLineMapping(Type table) { _table = table; }
    }
}
