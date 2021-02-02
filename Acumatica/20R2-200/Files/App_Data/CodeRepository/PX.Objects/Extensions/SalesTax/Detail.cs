using System;
using PX.Data;

namespace PX.Objects.Extensions.SalesTax
{
    /// <summary>A mapped cache extension that represents a detail line of the document.</summary>
    public class Detail : PXMappedCacheExtension
    {
        #region TaxCategoryID
        /// <exclude />
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        /// <summary>Identifier of the tax category associated with the line.</summary>

        public virtual string TaxCategoryID { get; set; }
        #endregion
        #region TaxID
        /// <exclude />
        public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }

        /// <summary>The identifier of the tax applied to the detail line.</summary>
        public virtual string TaxID { get; set; }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        /// <exclude />
        protected Int64? _CuryInfoID;

        /// <summary>
        /// Identifier of the CurrencyInfo object associated with the document.
        /// </summary>

        public virtual Int64? CuryInfoID
        {
            get
            {
                return _CuryInfoID;
            }
            set
            {
                _CuryInfoID = value;
            }
        }
        #endregion
        #region CuryTranDiscount
        /// <exclude />
        public abstract class curyTranDiscount : PX.Data.BQL.BqlDecimal.Field<curyTranDiscount> { }

        /// <summary>The total discount for the line item, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public decimal? CuryTranDiscount { get; set; }
        #endregion

        #region CuryTranExtPrice
        /// <exclude />
        public abstract class curyTranExtPrice : PX.Data.BQL.BqlDecimal.Field<curyTranExtPrice> { }

        /// <summary>The total amount without discount for the line item, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public decimal? CuryTranExtPrice { get; set; }
        #endregion

        #region CuryTranAmt
        /// <exclude />
        public abstract class curyTranAmt : PX.Data.BQL.BqlDecimal.Field<curyTranAmt> { }

        /// <summary>The total amount for the line item, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public decimal? CuryTranAmt { get; set; }
        #endregion

        #region GroupDiscountRate
        /// <exclude />
        public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }

        /// <summary>The Group-level discount rate.</summary>
        public virtual decimal? GroupDiscountRate { get; set; }
        #endregion
        #region DocumentDiscountRate
        /// <exclude />
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }

        /// <summary>The Document-level discount rate.</summary>
        public virtual decimal? DocumentDiscountRate { get; set; }
        #endregion
    }
}
