using System;
using PX.Data;

namespace PX.Objects.Extensions.SalesTax
{
    /// <summary>A mapped cache extension that represents the tax total amount.</summary>
    public class TaxTotal : PXMappedCacheExtension
    {
        #region RefTaxID
        /// <exclude />
        public abstract class refTaxID : PX.Data.BQL.BqlString.Field<refTaxID> { }

        /// <summary>The ID of the tax calculated on the document.</summary>
        public virtual string RefTaxID { get; set; }
        #endregion
        #region TaxRate
        /// <exclude />
        public abstract class taxRate : PX.Data.BQL.BqlDecimal.Field<taxRate> { }

        /// <summary>The tax rate used for tax calculation.</summary>
        public virtual decimal? TaxRate { get; set; }
        #endregion
        #region TaxZoneID
        /// <exclude />
        public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }

        /// <summary>The identifier of the tax zone.</summary>
        public virtual string TaxZoneID { get; set; }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }

        /// <summary>Identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.</summary>
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region CuryTaxableAmt
        /// <exclude />
        public abstract class curyTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxableAmt> { }

        /// <summary>The taxable amount (tax base), in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryTaxableAmt { get; set; }
        #endregion
		#region CuryExemptedAmt
		/// <exclude />
		public abstract class curyExemptedAmt : IBqlField
		{
		}

		/// <summary>The exempted amount, in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual decimal? CuryExemptedAmt { get; set; }
		#endregion
        #region CuryTaxAmt
        /// <exclude />
        public abstract class curyTaxAmt : PX.Data.BQL.BqlDecimal.Field<curyTaxAmt> { }

        /// <summary>The tax amount, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryTaxAmt { get; set; }
        #endregion
        #region CuryTaxAmtSumm
        /// <exclude />
        public abstract class curyTaxAmtSumm : PX.Data.BQL.BqlDecimal.Field<curyTaxAmtSumm> { }

        /// <summary>The tax amount, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual decimal? CuryTaxAmtSumm { get; set; }
        #endregion
        #region NonDeductibleTaxRate
        /// <exclude />
        public abstract class nonDeductibleTaxRate : PX.Data.BQL.BqlDecimal.Field<nonDeductibleTaxRate> { }

        /// <summary>The percent of deduction that applies to the tax amount paid to the vendor for specific purchases.</summary>
        public virtual Decimal? NonDeductibleTaxRate { get; set; }
        #endregion
        #region ExpenseAmt
        /// <exclude />
        public abstract class expenseAmt : PX.Data.BQL.BqlDecimal.Field<expenseAmt> { }

        /// <summary>The percentage that is deducted from the tax amount paid to the vendor for specific purchases, in the base currency of the company.</summary>
        public virtual Decimal? ExpenseAmt { get; set; }
        #endregion
        #region CuryExpenseAmt
        /// <exclude />
        public abstract class curyExpenseAmt : PX.Data.BQL.BqlDecimal.Field<curyExpenseAmt> { }

        /// <summary>The percentage that is deducted from the tax amount paid to the vendor for specific purchases, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual Decimal? CuryExpenseAmt { get; set; }
        #endregion
        #region CuryOrigTaxableAmt
        /// <exclude />
        public abstract class curyOrigTaxableAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigTaxableAmt> { }


        public virtual decimal? CuryOrigTaxableAmt { get; set; }
        #endregion

        /// <summary>Converts TaxTotal to <see cref="TaxItem" />.</summary>
        public static explicit operator TaxItem(TaxTotal i)
        {
            return new TaxItem { TaxID = i.RefTaxID };
        }
    }
}
