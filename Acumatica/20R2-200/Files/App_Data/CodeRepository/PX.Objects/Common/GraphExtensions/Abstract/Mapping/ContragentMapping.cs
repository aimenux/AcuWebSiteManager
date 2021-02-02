using System;
using PX.Data;
using PX.Objects.Common.GraphExtensions.Abstract.DAC;

namespace PX.Objects.Common.GraphExtensions.Abstract.Mapping
{
    public class ContragentMapping : IBqlMapping
    {
        /// <exclude />
        public Type Extension => typeof(Contragent);
        /// <exclude />
        protected Type _table;
        /// <exclude />
        public Type Table => _table;

        public ContragentMapping(Type table)
        {
            _table = table;
        }
    }
}
