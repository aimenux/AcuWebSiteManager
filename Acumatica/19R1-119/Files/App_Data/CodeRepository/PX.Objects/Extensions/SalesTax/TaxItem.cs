using PX.Data;
using PX.Objects.TX;

namespace PX.Objects.Extensions.SalesTax
{
    /// <summary>A supplementary class that represents a tax item.</summary>
    public class TaxItem : ITaxDetail
    {
        #region TaxID
        /// <exclude />
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

        /// <summary>The identifier of the tax.</summary>
        public virtual string TaxID { get; set; }
        #endregion        
    }
}
