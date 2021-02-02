using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.Extensions.SalesPrice
{
    /// <summary>A mapped cache extension that represents a detail line of the document.</summary>
    public class Detail : PXMappedCacheExtension
    {
        #region BranchID
        /// <exclude />
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        /// <exclude />
        protected Int32? _BranchID;
        /// <summary>The identifier of the branch associated with the detail line.</summary>
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
        #region Descr
        /// <exclude />
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        /// <summary>The description of the inventory item.</summary>
        public virtual string Descr { get; set; }
        #endregion
		#region SiteID
		/// <exclude />
		public abstract class siteID : IBqlField { }
		/// <exclude />
		protected Int32? _SiteID;
		/// <summary>The identifier of the Warehouse of the item.</summary>
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
        #region UOM
        /// <exclude />
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        /// <exclude />
        protected String _UOM;
        /// <summary>The unit of measure for the inventory item.</summary>
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
        #region Quantity
        /// <exclude />
        public abstract class quantity : PX.Data.BQL.BqlDecimal.Field<quantity> { }
        /// <exclude />
        protected Decimal? _Quantity;
        /// <summary>The quantity of the inventory item.</summary>
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
        /// <summary>The price per unit of the line item, in the currency of the document.</summary>
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
        #region CuryLineAmount
        /// <exclude />
        public abstract class curyLineAmount : PX.Data.BQL.BqlDecimal.Field<curyLineAmount> { }
        /// <exclude />
        protected Decimal? _CuryLineAmount;
        /// <summary>The total amount for the line item, in the currency of the document.</summary>
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
        #region IsFree
        /// <exclude />
        public abstract class isFree : PX.Data.BQL.BqlBool.Field<isFree> { }
        /// <exclude />
        protected Boolean? _IsFree;
        /// <summary>A property that indicates (if set to <tt>true</tt>) that the inventory item specified in the row is a free item.</summary>
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
        #region ManualPrice
        /// <exclude />
        public abstract class manualPrice : PX.Data.BQL.BqlBool.Field<manualPrice> { }
        /// <exclude />
        protected Boolean? _ManualPrice;
        /// <summary>A property that indicates (if set to <tt>true</tt>) that the price of this inventory item was changed manually.</summary>
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
    }
}
