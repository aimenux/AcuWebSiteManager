using System;
using PX.Data;
using PX.Objects.CM.Extensions;

namespace PX.Objects.Extensions.Discount
{
    /// <summary>A mapped cache extension that represents a document that supports discounts.</summary>
	public class Document : PXMappedCacheExtension
	{
		#region BranchID
        /// <exclude />
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <exclude />
		protected Int32? _BranchID;

        /// <summary>The identifier of the branch associated with the document.</summary>
		public virtual Int32? BranchID
		{
			get
			{
				return _BranchID;
			}
			set
			{
				_BranchID = value;
			}
		}
        #endregion
        #region CustomerID
        /// <exclude />
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        /// <exclude />
        protected Int32? _CustomerID;
        /// <summary>The identifier of a customer account to whom this document belongs.</summary>
        public virtual Int32? CustomerID
        {
            get
            {
                return _CustomerID;
            }
            set
            {
                _CustomerID = value;
            }
        }
        #endregion
        #region CuryID
        /// <exclude />
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
        /// <exclude />
        protected String _CuryID;

        /// <summary>The identifier of the currency of the document.</summary>
        public virtual String CuryID
        {
            get
            {
                return _CuryID;
            }
            set
            {
                _CuryID = value;
            }
        }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        /// <exclude />
		protected Int64? _CuryInfoID;

        /// <summary>The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.</summary>
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
		#region CuryOrigDiscAmt
        /// <exclude />
		public abstract class curyOrigDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyOrigDiscAmt> { }
        /// <exclude />
		protected Decimal? _CuryOrigDiscAmt;

        /// <summary>The cash discount allowed for the document in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual Decimal? CuryOrigDiscAmt
		{
			get
			{
				return _CuryOrigDiscAmt;
			}
			set
			{
				_CuryOrigDiscAmt = value;
			}
		}
		#endregion
		#region OrigDiscAmt
        /// <exclude />
		public abstract class origDiscAmt : PX.Data.BQL.BqlDecimal.Field<origDiscAmt> { }
        /// <exclude />
		protected Decimal? _OrigDiscAmt;

        /// <summary>The cash discount allowed for the document in the currency of the company.</summary>
		public virtual Decimal? OrigDiscAmt
		{
			get
			{
				return _OrigDiscAmt;
			}
			set
			{
				_OrigDiscAmt = value;
			}
		}
		#endregion
		#region CuryDiscTaken
        /// <exclude />
		public abstract class curyDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyDiscTaken> { }
        /// <exclude />
		protected Decimal? _CuryDiscTaken;

        /// <summary>The cash discount applied to the document, in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual Decimal? CuryDiscTaken
		{
			get
			{
				return _CuryDiscTaken;
			}
			set
			{
				_CuryDiscTaken = value;
			}
		}
		#endregion
		#region DiscTaken
        /// <exclude />
		public abstract class discTaken : PX.Data.BQL.BqlDecimal.Field<discTaken> { }
        /// <exclude />
		protected Decimal? _DiscTaken;

        /// <summary>The cash discount actually applied to the document, in the base currency of the company.</summary>
		public virtual Decimal? DiscTaken
		{
			get
			{
				return _DiscTaken;
			}
			set
			{
				_DiscTaken = value;
			}
		}
		#endregion
		#region CuryDiscBal
        /// <exclude />
		public abstract class curyDiscBal : PX.Data.BQL.BqlDecimal.Field<curyDiscBal> { }
        /// <exclude />
		protected Decimal? _CuryDiscBal;

        /// <summary>The cash discount balance of the document, in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual Decimal? CuryDiscBal
		{
			get
			{
				return _CuryDiscBal;
			}
			set
			{
				_CuryDiscBal = value;
			}
		}
		#endregion
		#region DiscBal
        /// <exclude />
		public abstract class discBal : PX.Data.BQL.BqlDecimal.Field<discBal> { }
        /// <exclude />
		protected Decimal? _DiscBal;

        /// <summary>The cash discount balance of the document, in the base currency of the company.</summary>
		public virtual Decimal? DiscBal
		{
			get
			{
				return _DiscBal;
			}
			set
			{
				_DiscBal = value;
			}
		}
		#endregion
		#region DiscTot
        /// <exclude />
		public abstract class discTot : PX.Data.BQL.BqlDecimal.Field<discTot> { }
        /// <exclude />
		protected Decimal? _DiscTot;

        /// <summary>The total group and document discount for the document, in the base currency of the company.</summary>
		public virtual Decimal? DiscTot
		{
			get
			{
				return _DiscTot;
			}
			set
			{
				_DiscTot = value;
			}
		}
		#endregion
		#region CuryDiscTot
        /// <exclude />
		public abstract class curyDiscTot : PX.Data.BQL.BqlDecimal.Field<curyDiscTot> { }
        /// <exclude />
		protected Decimal? _CuryDiscTot;

        /// <summary>The total group and document discount for the document. The discount is in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual Decimal? CuryDiscTot
		{
			get
			{
				return _CuryDiscTot;
			}
			set
			{
				_CuryDiscTot = value;
			}
		}
		#endregion
		#region DocDisc
        /// <exclude />
		public abstract class docDisc : PX.Data.BQL.BqlDecimal.Field<docDisc> { }
        /// <exclude />
		protected Decimal? _DocDisc;

        /// <summary>The document discount amount (without group discounts) for the document. The amount is in the base currency of the company.</summary>
		public virtual Decimal? DocDisc
		{
			get
			{
				return _DocDisc;
			}
			set
			{
				_DocDisc = value;
			}
		}
		#endregion
		#region CuryDocDisc
        /// <exclude />
		public abstract class curyDocDisc : PX.Data.BQL.BqlDecimal.Field<curyDocDisc> { }
        /// <exclude />
		protected Decimal? _CuryDocDisc;

        /// <summary>The document discount amount (without group discounts) for the document. The discount is in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual Decimal? CuryDocDisc
		{
			get
			{
				return _CuryDocDisc;
			}
			set
			{
				_CuryDocDisc = value;
			}
		}
		#endregion
		#region CuryDiscountedDocTotal
        /// <exclude />
		public abstract class curyDiscountedDocTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedDocTotal> { }
        /// <exclude />
		protected decimal? _CuryDiscountedDocTotal;

        /// <summary>The discounted amount of the document, in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual decimal? CuryDiscountedDocTotal
		{
			get
			{
				return _CuryDiscountedDocTotal;
			}
			set
			{
				_CuryDiscountedDocTotal = value;
			}
		}
		#endregion
		#region DiscountedDocTotal
        /// <exclude />
		public abstract class discountedDocTotal : PX.Data.BQL.BqlDecimal.Field<discountedDocTotal> { }
        /// <exclude />
		protected decimal? _DiscountedDocTotal;

        /// <summary>The discounted amount of the document, in the base currency of the company.</summary>
		public virtual decimal? DiscountedDocTotal
		{
			get
			{
				return _DiscountedDocTotal;
			}
			set
			{
				_DiscountedDocTotal = value;
			}
		}
		#endregion
		#region CuryDiscountedTaxableTotal
        /// <exclude />
		public abstract class curyDiscountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<curyDiscountedTaxableTotal> { }
        /// <exclude />
		protected decimal? _CuryDiscountedTaxableTotal;

        /// <summary>The total taxable amount reduced on early payment according to cash discount. The amount is in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual decimal? CuryDiscountedTaxableTotal
		{
			get
			{
				return _CuryDiscountedTaxableTotal;
			}
			set
			{
				_CuryDiscountedTaxableTotal = value;
			}
		}
		#endregion
		#region DiscountedTaxableTotal
        /// <exclude />
		public abstract class discountedTaxableTotal : PX.Data.BQL.BqlDecimal.Field<discountedTaxableTotal> { }
        /// <exclude />
		protected decimal? _DiscountedTaxableTotal;

        /// <summary>The total taxable amount reduced on early payment according to cash discount. The amount is in the base currency of the company.</summary>
		public virtual decimal? DiscountedTaxableTotal
		{
			get
			{
				return _DiscountedTaxableTotal;
			}
			set
			{
				_DiscountedTaxableTotal = value;
			}
		}
		#endregion
		#region CuryDiscountedPrice
        /// <exclude />
		public abstract class curyDiscountedPrice : PX.Data.BQL.BqlDecimal.Field<curyDiscountedPrice> { }
        /// <exclude />
		protected decimal? _CuryDiscountedPrice;

        /// <summary>The total tax amount reduced on early payment according to cash discount. The amount is in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual decimal? CuryDiscountedPrice
		{
			get
			{
				return _CuryDiscountedPrice;
			}
			set
			{
				_CuryDiscountedPrice = value;
			}
		}
		#endregion
		#region DiscountedPrice
        /// <exclude />
		public abstract class discountedPrice : PX.Data.BQL.BqlDecimal.Field<discountedPrice> { }
        /// <exclude />
		protected decimal? _DiscountedPrice;

        /// <summary>The total tax amount reduced on early payment according to cash discount. The amount is in the base currency of the company.</summary>
		public virtual decimal? DiscountedPrice
		{
			get
			{
				return _DiscountedPrice;
			}
			set
			{
				_DiscountedPrice = value;
			}
		}
		#endregion
		#region LocationID
        /// <exclude />
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        /// <exclude />
		protected Int32? _LocationID;

        /// <summary>The identifier of the location of the customer.</summary>
		public virtual Int32? LocationID
		{
			get
			{
				return _LocationID;
			}
			set
			{
				_LocationID = value;
			}
		}
		#endregion
		#region DocumentDate
        /// <exclude />
		public abstract class documentDate : PX.Data.BQL.BqlDateTime.Field<documentDate> { }
        /// <exclude />
		protected DateTime? _DocumentDate;
        /// <summary>The date of the document.</summary>
		public virtual DateTime? DocumentDate
		{
			get
			{
				return _DocumentDate;
			}
			set
			{
				_DocumentDate = value;
			}
		}
		#endregion
		#region CuryLineTotal
        /// <exclude />
		public abstract class curyLineTotal : PX.Data.BQL.BqlDecimal.Field<curyLineTotal> { }
        /// <exclude />
		protected Decimal? _CuryLineTotal;

        /// <summary>The total amount of the lines of the document, in the currency of the document (<see cref="CuryID" />).</summary>
		public virtual Decimal? CuryLineTotal
		{
			get
			{
				return _CuryLineTotal;
			}
			set
			{
				_CuryLineTotal = value;
			}
		}
		#endregion
		#region LineTotal
        /// <exclude />
		public abstract class lineTotal : PX.Data.BQL.BqlDecimal.Field<lineTotal> { }
        /// <exclude />
		protected Decimal? _LineTotal;

        /// <summary>The total amount of the lines of the document, in the base currency of the company.</summary>
		public virtual Decimal? LineTotal
		{
			get
			{
				return _LineTotal;
			}
			set
			{
				_LineTotal = value;
			}
		}
        #endregion
        #region CuryMiscTot
        /// <exclude />
        public abstract class curyMiscTot : PX.Data.BQL.BqlDecimal.Field<curyMiscTot> { }
        /// <exclude />
        protected Decimal? _CuryMiscTot;
        /// <summary>The miscellaneous total amount, in the currency of the document (<see cref="CuryID" />).</summary>
        public virtual Decimal? CuryMiscTot
        {
            get
            {
                return _CuryMiscTot;
            }
            set
            {
                _CuryMiscTot = value;
            }
        }
        #endregion
        
    }
}
