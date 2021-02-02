using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.TX;

namespace PX.Objects.AP
{
    [Serializable]
	[PXCacheName(Messages.APPriceWorksheetDetail)]
	[PXProjection(typeof(Select2<APPriceWorksheetDetail,
		InnerJoin<InventoryItem,
			On<InventoryItem.inventoryID, Equal<inventoryID>>>>),
		new[] { typeof(APPriceWorksheetDetail) })]
	public partial class APPriceWorksheetDetail : IBqlTable
    {
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        protected String _RefNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDBDefault(typeof(APPriceWorksheet.refNbr))]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
        [PXParent(typeof(Select<APPriceWorksheet, Where<APPriceWorksheet.refNbr, Equal<Current<APPriceWorksheetDetail.refNbr>>>>))]
		public virtual String RefNbr
        {
            get
            {
                return this._RefNbr;
            }
            set
            {
                this._RefNbr = value;
            }
        }
        #endregion
        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }
        protected Int32? _LineID;
        [PXDBIdentity(IsKey = true)]
        public virtual Int32? LineID
        {
            get
            {
                return this._LineID;
            }
            set
            {
                this._LineID = value;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        protected Int32? _VendorID;
        [Vendor]
        [PXDefault]
        [PXParent(typeof(Select<Vendor, Where<Vendor.bAccountID, Equal<Current<APPriceWorksheetDetail.vendorID>>>>))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
		[PriceWorksheetInventory(typeof(vendorID), typeof(alternateID), typeof(INAlternateType.vPN), typeof(restrictInventoryByAlternateID), DisplayName = "Inventory ID")]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<inventoryID>>>>))]
		[PXDefault]
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
		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		[PXDBString(IsUnicode = true, BqlField = typeof(InventoryItem.inventoryCD))]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.inventoryCD>))]
		public virtual String InventoryCD
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PriceWorksheetAlternateItem]
		public virtual String AlternateID
		{
			get
			{
				return this._AlternateID;
			}
			set
			{
				this._AlternateID = value;
			}
		}
		#endregion
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [SubItem(typeof(APPriceWorksheetDetail.inventoryID))]
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
        #region Description
        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
        protected String _Description;
		[PXDBString(256, IsUnicode = true, BqlField = typeof(InventoryItem.descr))]
		[PXFormula(typeof(Selector<inventoryID, InventoryItem.descr>))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual String Description
        {
            get
            {
                return this._Description;
            }
            set
            {
                this._Description = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        [PXDefault]
        [INUnit(typeof(APPriceWorksheetDetail.inventoryID))]
        [PXFormula(typeof(Selector<APPriceWorksheetDetail.inventoryID, InventoryItem.purchaseUnit>))]
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
        #region BreakQty
        public abstract class breakQty : PX.Data.BQL.BqlDecimal.Field<breakQty> { }
        protected Decimal? _BreakQty;
        [PXDBQuantity(MinValue = 0)]
        [PXUIField(DisplayName = "Break Qty", Visibility = PXUIVisibility.Visible, Enabled = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BreakQty
        {
            get
            {
                return this._BreakQty;
            }
            set
            {
                this._BreakQty = value;
            }
        }
        #endregion
        #region CurrentPrice
        public abstract class currentPrice : PX.Data.BQL.BqlDecimal.Field<currentPrice> { }
        protected Decimal? _CurrentPrice;
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBPriceCost]
        [PXUIField(DisplayName = "Source Price", Visibility = PXUIVisibility.Visible, Enabled = false)]
        public virtual Decimal? CurrentPrice
        {
            get
            {
                return this._CurrentPrice;
            }
            set
            {
                this._CurrentPrice = value;
            }
        }
        #endregion
        #region PendingPrice
        public abstract class pendingPrice : PX.Data.BQL.BqlDecimal.Field<pendingPrice> { }

		[PXDBDecimal(6)]
        [PXUIField(DisplayName = "Pending Price", Visibility = PXUIVisibility.Visible)]
        public virtual decimal? PendingPrice
        {
			get;
			set;
        }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		[PXDBString(5)]
        [PXDefault(typeof(Search<GL.Company.baseCuryID>))]
        [PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
        [PXUIField(DisplayName = "Currency")]
        public virtual string CuryID
        {
			get;
			set;
        }
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[NullableSite]
		public virtual Int32? SiteID
		{
			get { return this._SiteID; }
			set { this._SiteID = value; }
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
        protected String _TaxID;
        [PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.Visible, Enabled = true)]
		[PXSelector(typeof(Tax.taxID), DescriptionField = typeof(Tax.descr))]
        [PXDBString(Tax.taxID.Length)]
        public virtual String TaxID
        {
            get
            {
                return this._TaxID;
            }
            set
            {
                this._TaxID = value;
            }
        }
        #endregion
        #region System Columns
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        protected Byte[] _tstamp;
        [PXDBTimestamp()]
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
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        protected Guid? _CreatedByID;
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get
            {
                return this._CreatedByID;
            }
            set
            {
                this._CreatedByID = value;
            }
        }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
        protected String _CreatedByScreenID;
        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get
            {
                return this._CreatedByScreenID;
            }
            set
            {
                this._CreatedByScreenID = value;
            }
        }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
        protected DateTime? _CreatedDateTime;
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get
            {
                return this._CreatedDateTime;
            }
            set
            {
                this._CreatedDateTime = value;
            }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
		#endregion
		#endregion
		#region RestrictInventoryByAlternateID
		public abstract class restrictInventoryByAlternateID : PX.Data.BQL.BqlBool.Field<restrictInventoryByAlternateID> { }
		[PXBool, PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? RestrictInventoryByAlternateID { get; set; }
	    #endregion
	}
}