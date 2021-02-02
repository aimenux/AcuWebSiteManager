using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Extensions.SalesPrice
{
    /// <summary>A mapped cache extension that provides information about the source of the price class.</summary>
    public class PriceClassSource : PXMappedCacheExtension
    {
        #region PriceClassID
        /// <exclude />
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
        /// <exclude />
        protected String _PriceClassID;
        /// <summary>The identifier of the price class in the system.</summary>
        public virtual String PriceClassID
        {
            get
            {
                return _PriceClassID;
            }
            set
            {
                _PriceClassID = value;
            }
        }
        #endregion
    }
}
