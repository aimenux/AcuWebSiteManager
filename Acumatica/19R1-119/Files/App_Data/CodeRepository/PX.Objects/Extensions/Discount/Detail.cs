using System;
using PX.Data;

namespace PX.Objects.Extensions.Discount
{
    /// <summary>A mapped cache extension that represents a detail line of the document.</summary>
    public class Detail : PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <exclude />
        protected Int32? _BranchID;
        /// <summary>The identifier of the branch.</summary>
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
        #region InventoryID
        /// <exclude />
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        /// <exclude />
        protected Int32? _InventoryID;
        /// <summary>The identifier of the inventory item.</summary>
        public virtual Int32? InventoryID
        {
            get
            {
                return _InventoryID;
            }
            set
            {
                _InventoryID = value;
            }
        }
        #endregion
        #region SiteID
        /// <exclude />
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        /// <exclude />
        protected Int32? _SiteID;
        /// <summary>The warehouse ID.</summary>
        public virtual Int32? SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                _SiteID = value;
            }
        }
        #endregion
        #region CustomerID
        /// <exclude />
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        /// <exclude />
        protected Int32? _CustomerID;
        /// <summary>The identifier of the customer account.</summary>
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
        #region VendorID
        /// <exclude />
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        /// <exclude />
        protected Int32? _VendorID;
        /// <summary>The identifier of the vendor account associated with the detail line.</summary>
        public virtual Int32? VendorID
        {
            get
            {
                return _VendorID;
            }
            set
            {
                _VendorID = value;
            }
        }
        #endregion
        #region CuryInfoID
        /// <exclude />
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        /// <exclude />
        protected Int64? _CuryInfoID;

        /// <summary>The identifier of the <see cref="CurrencyInfo">CurrencyInfo</see> object associated with the document.</summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyInfoID" /> field.
        /// </value>
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
        #region Quantity
        /// <exclude />
        public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
        /// <exclude />
        protected Decimal? _Quantity;
        /// <summary>The quantity of the line item.</summary>
        public virtual Decimal? Quantity
        {
            get
            {
                return _Quantity;
            }
            set
            {
                _Quantity = value;
            }
        }
        #endregion
        #region CuryUnitPrice
        /// <exclude />
        public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
        /// <exclude />
        protected Decimal? _CuryUnitPrice;
        /// <summary>
        ///   <para>The price per unit of the line item, in the currency of the document (<see cref="Document.CuryID" />).</para>
        /// </summary>
        public virtual Decimal? CuryUnitPrice
        {
            get
            {
                return _CuryUnitPrice;
            }
            set
            {
                _CuryUnitPrice = value;
            }
        }
        #endregion
        #region CuryExtPrice
        /// <exclude />
        public abstract class curyExtPrice : PX.Data.BQL.BqlDecimal.Field<curyExtPrice> { }
        /// <exclude />
        protected Decimal? _CuryExtPrice;
        /// <summary>The extended price of the line item, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public virtual Decimal? CuryExtPrice
        {
            get
            {
                return _CuryExtPrice;
            }
            set
            {
                _CuryExtPrice = value;
            }
        }
        #endregion
        #region CuryLineAmount
        /// <exclude />
        public abstract class curyLineAmount : PX.Data.BQL.BqlDecimal.Field<curyLineAmount> { }
        /// <exclude />
        protected Decimal? _CuryLineAmount;
        /// <summary>The total amount for the line item, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public virtual Decimal? CuryLineAmount
        {
            get
            {
                return _CuryLineAmount;
            }
            set
            {
                _CuryLineAmount = value;
            }
        }
        #endregion
        #region UOM
        /// <exclude />
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        /// <exclude />
        protected String _UOM;
        /// <summary>The unit of measure (UOM) used for the detail line.</summary>
        public virtual String UOM
        {
            get
            {
                return _UOM;
            }
            set
            {
                _UOM = value;
            }
        }
		#endregion
		#region DiscountsAppliedToLine
		/// <exclude />
		public abstract class discountsAppliedToLine : PX.Data.BQL.BqlByteArray.Field<discountsAppliedToLine> { }
		/// <exclude />
		protected ushort[] _DiscountsAppliedToLine;
		/// <summary>Array of line numbers of discount lines applied to the current lint</summary>
		public virtual ushort[] DiscountsAppliedToLine
		{
			get
			{
				return this._DiscountsAppliedToLine;
			}
			set
			{
				this._DiscountsAppliedToLine = value;
			}
		}
		#endregion
		#region OrigGroupDiscountRate
		/// <exclude />
		public abstract class origGroupDiscountRate : PX.Data.BQL.BqlDecimal.Field<origGroupDiscountRate> { }
		/// <exclude />
		protected Decimal? _OrigGroupDiscountRate;
		/// <summary>Group-level discount rate for the set of discounts from original document.</summary>
		public virtual Decimal? OrigGroupDiscountRate
		{
			get
			{
				return this._OrigGroupDiscountRate;
			}
			set
			{
				this._OrigGroupDiscountRate = value;
			}
		}
		#endregion
		#region OrigDocumentDiscountRate
		/// <exclude />
		public abstract class origDocumentDiscountRate : PX.Data.BQL.BqlDecimal.Field<origDocumentDiscountRate> { }
		/// <exclude />
		protected Decimal? _OrigDocumentDiscountRate;
		/// <summary>Document-level discount rate for the set of discounts from original document.</summary>
		public virtual Decimal? OrigDocumentDiscountRate
		{
			get
			{
				return this._OrigDocumentDiscountRate;
			}
			set
			{
				this._OrigDocumentDiscountRate = value;
			}
		}
		#endregion
		#region GroupDiscountRate
		/// <exclude />
		public abstract class groupDiscountRate : PX.Data.BQL.BqlDecimal.Field<groupDiscountRate> { }
        /// <exclude />
        protected Decimal? _GroupDiscountRate;
        /// <summary>The Group-level discount rate.</summary>
        public virtual Decimal? GroupDiscountRate
        {
            get
            {
                return _GroupDiscountRate;
            }
            set
            {
                _GroupDiscountRate = value;
            }
        }
        #endregion
        #region DocumentDiscountRate
        /// <exclude />
        public abstract class documentDiscountRate : PX.Data.BQL.BqlDecimal.Field<documentDiscountRate> { }
        /// <exclude />
        protected Decimal? _DocumentDiscountRate;
        /// <summary>The Document-level discount rate.</summary>
        public virtual Decimal? DocumentDiscountRate
        {
            get
            {
                return _DocumentDiscountRate;
            }
            set
            {
                _DocumentDiscountRate = value;
            }
        }
        #endregion

        #region CuryDiscAmt
        /// <exclude />
        public abstract class curyDiscAmt : PX.Data.BQL.BqlDecimal.Field<curyDiscAmt> { }
        /// <exclude />
        protected Decimal? _CuryDiscAmt;
        /// <summary>The amount of the discount applied to the detail line, in the currency of the document (<see cref="Document.CuryID" />).</summary>
        public virtual Decimal? CuryDiscAmt
        {
            get
            {
                return _CuryDiscAmt;
            }
            set
            {
                _CuryDiscAmt = value;
            }
        }
        #endregion
        #region DiscPct
        /// <exclude />
        public abstract class discPct : PX.Data.BQL.BqlDecimal.Field<discPct> { }
        /// <exclude />
        protected Decimal? _DiscPct;
        /// <summary>The percent of the line-level discount that has been applied manually or automatically to this line item.</summary>
        public virtual Decimal? DiscPct
        {
            get
            {
                return _DiscPct;
            }
            set
            {
                _DiscPct = value;
            }
        }
        #endregion
        #region DiscountID
        /// <exclude />
        public abstract class discountID : PX.Data.BQL.BqlString.Field<discountID> { }
        /// <exclude />
        protected String _DiscountID;
        /// <summary>The identifier (code) of the line discount that has been applied to this line.</summary>
        public virtual String DiscountID
        {
            get
            {
                return _DiscountID;
            }
            set
            {
                _DiscountID = value;
            }
        }
        #endregion
        #region DiscountSequenceID
        /// <exclude />
        public abstract class discountSequenceID : PX.Data.BQL.BqlString.Field<discountSequenceID> { }
        /// <exclude />
        protected String _DiscountSequenceID;
        /// <summary>The identifier of a discount sequence that has been applied to this line.</summary>
        public virtual String DiscountSequenceID
        {
            get
            {
                return _DiscountSequenceID;
            }
            set
            {
                _DiscountSequenceID = value;
            }
        }
        #endregion

        #region IsFree
        /// <exclude />
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
        /// <exclude />
        protected Boolean? _IsFree;
        /// <summary>Indicates (if set to <tt>true</tt>) that the line item is free.</summary>
        public virtual Boolean? IsFree
        {
            get
            {
                return _IsFree;
            }
            set
            {
                _IsFree = value;
            }
        }
        #endregion
        #region ManualDisc
        /// <exclude />
        public abstract class manualDisc : PX.Data.BQL.BqlBool.Field<manualDisc> { }
        /// <exclude />
        protected Boolean? _ManualDisc;
        /// <summary>Indicates (if set to <tt>true</tt>) that the discount has been applied manually for this line item.</summary>
        public virtual Boolean? ManualDisc
        {
            get
            {
                return _ManualDisc;
            }
            set
            {
                _ManualDisc = value;
            }
        }
        #endregion
        #region ManualPrice
        /// <exclude />
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
        /// <exclude />
        protected Boolean? _ManualPrice;
        /// <summary>Indicates (if set to <tt>true</tt>) that the unit price has been specified for this line item manually</summary>
        public virtual Boolean? ManualPrice
        {
            get
            {
                return _ManualPrice;
            }
            set
            {
                _ManualPrice = value;
            }
        }
        #endregion
        #region LineType
        /// <exclude />
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
        /// <exclude />
        protected String _LineType;
        /// <summary>The type of the line.</summary>
        public virtual String LineType
        {
            get
            {
                return _LineType;
            }
            set
            {
                _LineType = value;
            }
        }
        #endregion
        #region TaxCategoryID
        /// <exclude />
        public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        /// <exclude />
        protected String _TaxCategoryID;
        /// <summary>The identifier of the tax category.</summary>
        public virtual String TaxCategoryID
        {
            get
            {
                return _TaxCategoryID;
            }
            set
            {
                _TaxCategoryID = value;
            }
        }
        #endregion
        #region FreezeManualDisc
        /// <exclude />
        public abstract class freezeManualDisc : PX.Data.BQL.BqlBool.Field<freezeManualDisc> { }
        /// <exclude />
        protected Boolean? _FreezeManualDisc;
        public virtual Boolean? FreezeManualDisc
        {
            get
            {
                return _FreezeManualDisc;
            }
            set
            {
                _FreezeManualDisc = value;
            }
        }
        #endregion
    }
}
