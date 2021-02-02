using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using PX.Objects.PO;
using PX.Objects.AR;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// Used as a transfer object between source screens and order creation screens.
    /// </summary>
    [Serializable]
    [PXCacheName(Messages.OrderCrossRef)]
    public class AMOrderCrossRef : IBqlTable
    {
        #region ProcessSource
        public abstract class processSource : PX.Data.BQL.BqlInt.Field<processSource> { }

        protected Int32? _ProcessSource;
        [PXDBInt]
        [PXUIField(DisplayName = "Process Source", Visible = false)]
        [OrderCrossRefProcessSource.List]
        public virtual Int32? ProcessSource
        {
            get
            {
                return this._ProcessSource;
            }
            set
            {
                this._ProcessSource = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
        public virtual Int32? LineNbr
        {
            get
            {
                return this._LineNbr;
            }
            set
            {
                this._LineNbr = value;
            }
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

        protected String _BOMID;
        [BomID]
        public virtual String BOMID
        {
            get
            {
                return this._BOMID;
            }
            set
            {
                this._BOMID = value;
            }
        }
        #endregion
        #region DmdDetRecNbr
        public abstract class dmdDetRecNbr : PX.Data.BQL.BqlInt.Field<dmdDetRecNbr> { }

        protected Int32? _DmdDetRecNbr;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Demand Detail Rec Nbr")]
        public virtual Int32? DmdDetRecNbr
        {
            get
            {
                return this._DmdDetRecNbr;
            }
            set
            {
                this._DmdDetRecNbr = value;
            }
        }
        #endregion
        #region DmdDetGroupedRecNbrs
        /// <summary>
        /// Storing all grouped demand nbrs in a single column with a delimited value
        /// </summary>
        public abstract class dmdDetGroupedRecNbrs : PX.Data.BQL.BqlString.Field<dmdDetGroupedRecNbrs> { }

        protected string _DmdDetGroupedRecNbrs;
        /// <summary>
        /// Delimiter for values stored in <see cref="DmdDetGroupedRecNbrs"/>
        /// </summary>
        internal const string DmdDetGroupedRecNbrsDelimiter = ";";
        /// <summary>
        /// Storing all grouped demand nbrs in a single column with a delimited value
        /// </summary>
        [PXString]
        [PXUIField(DisplayName = "Demand Detail Group Rec Nbrs")]
        public virtual string DmdDetGroupedRecNbrs
        {
            get
            {
                return this._DmdDetGroupedRecNbrs;
            }
            set
            {
                this._DmdDetGroupedRecNbrs = value;
            }
        }
        #endregion
        #region ExplodeBOM
        public abstract class explodeBOM : PX.Data.BQL.BqlBool.Field<explodeBOM> { }

        protected bool? _ExplodeBOM;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Explode BOM")]
        public virtual bool? ExplodeBOM
        {
            get
            {
                return this._ExplodeBOM;
            }
            set
            {
                this._ExplodeBOM = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory(Enabled = false)]
        public virtual Int32? InventoryID
        {
            get
            {
                return this._InventoryID;
            }
            set
            {
                this._InventoryID = value;
            }
        }
        #endregion
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [SubItem(typeof(AMOrderCrossRef.inventoryID),Enabled = false)]
        public virtual Int32? SubItemID
        {
            get
            {
                return this._SubItemID;
            }
            set
            {
                this._SubItemID = value;
            }
        }
        #endregion
        #region ParentOrderType
        public abstract class parentOrderType : PX.Data.BQL.BqlString.Field<parentOrderType> { }

        protected String _ParentOrderType;
        [AMOrderTypeField]
        public virtual String ParentOrderType
        {
            get
            {
                return this._ParentOrderType;
            }
            set
            {
                this._ParentOrderType = value;
            }
        }
        #endregion
        #region ParentProdOrdID
        public abstract class parentProdOrdID : PX.Data.BQL.BqlString.Field<parentProdOrdID> { }

        protected String _ParentProdOrdID;
        [ProductionNbr(DisplayName = "Parent Production Order Nbr")]
        public virtual String ParentProdOrdID
        {
            get
            {
                return this._ParentProdOrdID;
            }
            set
            {
                this._ParentProdOrdID = value;
            }
        }
        #endregion  
        #region PlanDate
        public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Plan Date", Enabled = false)]
        public virtual DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity]
        [PXUIField(DisplayName = "Base Qty")]
        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQty;
            }
            set
            {
                this._BaseQty = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected decimal? _Qty;
        [PXDBQuantity(typeof(AMOrderCrossRef.uOM), typeof(AMOrderCrossRef.baseQty), HandleEmptyKey = true)]
        [PXUIField(DisplayName = "Quantity", Enabled = true)]
        public virtual decimal? Qty
        {
            get
            {
                return this._Qty;
            }
            set
            {
                this._Qty = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }

        protected Boolean? _Released;
        [PXDBBool]
        [PXDefault(false)]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region ReferenceOrderType
        public abstract class referenceOrderType : PX.Data.BQL.BqlString.Field<referenceOrderType> { }

        protected String _ReferenceOrderType;
        [AMOrderTypeField(DisplayName = "Reference Order Type")]
        public virtual String ReferenceOrderType
        {
            get
            {
                return this._ReferenceOrderType;
            }
            set
            {
                this._ReferenceOrderType = value;
            }
        }
        #endregion
        #region ReferenceNbr
        public abstract class referenceNbr : PX.Data.BQL.BqlString.Field<referenceNbr> { }

        protected String _ReferenceNbr;
        [PXDBString(15)]
        [PXUIField(DisplayName = "Reference Nbr")]
        public virtual String ReferenceNbr
        {
            get
            {
                return this._ReferenceNbr;
            }
            set
            {
                this._ReferenceNbr = value;
            }
        }
        #endregion
        #region ReferenceType
        public abstract class referenceType : PX.Data.BQL.BqlString.Field<referenceType> { }

        protected String _ReferenceType;
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Reference Type", Enabled = false)]
        public virtual String ReferenceType
        {
            get
            {
                return this._ReferenceType;
            }
            set
            {
                this._ReferenceType = value;
            }
        }
        #endregion
        #region ReferenceLineNbr
        public abstract class referenceLineNbr : PX.Data.BQL.BqlInt.Field<referenceLineNbr> { }

        protected int? _ReferenceLineNbr;
        [PXDBInt]
        [PXUIField(DisplayName = "Refference Line Nbr", Visible = false)]
        public virtual int? ReferenceLineNbr
        {
            get
            {
                return this._ReferenceLineNbr;
            }
            set
            {
                this._ReferenceLineNbr = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Enabled = false)]
        public virtual Int32? SiteID
        {
            get
            {
                return this._SiteID;
            }
            set
            {
                this._SiteID = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [Location(typeof(AMOrderCrossRef.siteID))]
        public virtual Int32? LocationID
        {
            get
            {
                return this._LocationID;
            }
            set
            {
                this._LocationID = value;
            }
        }
        #endregion
        #region Source
        public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

        protected String _Source;
        [PXDBString(1)]
        [PXUIField(DisplayName = "Source", Enabled = false)]
        [INReplenishmentSource.List]
        public virtual String Source
        {
            get
            {
                return this._Source;
            }
            set
            {
                this._Source = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit(typeof(AMOrderCrossRef.inventoryID), BqlField = typeof(AMOrderCrossRef.uOM), Enabled = false)]
        public virtual String UOM
        {
            get
            {
                return this._UOM;
            }
            set
            {
                this._UOM = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [POVendor(CacheGlobal = true, DescriptionField = typeof(Vendor.acctName))]
        public virtual Int32? VendorID
        {
            get
            {
                return this._VendorID;
            }
            set
            {
                this._VendorID = value;
            }
        }
        #endregion
        #region GroupNumber
        public abstract class groupNumber : PX.Data.BQL.BqlString.Field<groupNumber> { }

        protected Int32? _GroupNumber;
        [PXDBInt]
        [PXUIField(DisplayName = "Group Nbr")]
        public Int32? GroupNumber
        {
            get
            {
                return this._GroupNumber;
            }
            set
            {
                this._GroupNumber = value;
            }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        protected Int32? _CustomerID;
        [Customer(DescriptionField = typeof(Customer.acctName), Enabled = true)]
        public virtual Int32? CustomerID
        {
            get
            {
                return this._CustomerID;
            }
            set
            {
                this._CustomerID = value;
            }
        }
        #endregion
        #region UserID
        public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

        protected Guid? _UserID;
        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "User ID")]
        public virtual Guid? UserID
        {
            get
            {
                return this._UserID;
            }
            set
            {
                this._UserID = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        [ProductionNbr]
        public virtual String ProdOrdID
        {
            get
            {
                return this._ProdOrdID;
            }
            set
            {
                this._ProdOrdID = value;
            }
        }
        #endregion
        #region ManualNumbering
        public abstract class manualNumbering : PX.Data.BQL.BqlBool.Field<manualNumbering> { }

        protected Boolean? _ManualNumbering;
        [PXDBBool]
        [PXDefault(false)]
        public virtual Boolean? ManualNumbering
        {
            get
            {
                return this._ManualNumbering;
            }
            set
            {
                this._ManualNumbering = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
    }
}
