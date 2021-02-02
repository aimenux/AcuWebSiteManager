namespace PX.Objects.AR
{
	using System;
	using PX.Data;
	using PX.Objects.IN;
	using PX.Objects.CM;
	using PX.Objects.TX;
	using PX.Objects.CS;
	using PX.Objects.GL;

	/// <summary>
	/// Represents an Accounts Receivable sales price. Depending on the
	/// specified <see cref="PriceType">price type</see>, a sales price record
	/// can define a base price, a price for a given <see cref="Customer">
	/// customer</see>, or a price for a certain <see cref="ARPriceClass">customer 
	/// price class</see>. The entities of this type can be edited on the Sales Prices 
	/// (AR202000) form, which corresponds to the <see cref="ARSalesPricesMaint"/> graph.
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.ARSalesPrice)]
	[ARSalesPriceProjection]
	public partial class ARSalesPrice : PX.Data.IBqlTable
	{		
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return this._RecordID;
			}
			set
			{
				this._RecordID = value;
			}
		}
		#endregion
		#region PriceType
		public abstract class priceType : PX.Data.BQL.BqlString.Field<priceType> { }
		protected String _PriceType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(PriceTypes.CustomerPriceClass)]
		[PriceTypes.List]
		[PXUIField(DisplayName = "Price Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String PriceType
		{
			get
			{
				return this._PriceType;
			}
			set
			{
				this._PriceType = value;
			}
		}
		#endregion
		#region PriceCode
		public abstract class priceCode : PX.Data.BQL.BqlString.Field<priceCode> { }
		protected String _PriceCode;
		[PXString(30, InputMask = ">CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Price Code", Visibility = PXUIVisibility.SelectorVisible)]
        [PXPriceCodeSelector(typeof(ARSalesPrice.priceCode), new Type[] { typeof(ARSalesPrice.priceCode), typeof(ARSalesPrice.description) }, 
			ValidateValue = false, DescriptionField = typeof(ARSalesPrice.description))]
		public virtual String PriceCode
		{
			get
			{
				return this._PriceCode;
			}
			set
			{
				this._PriceCode = value;
			}
		}
		#endregion
        #region CustPriceClassID
        public abstract class custPriceClassID : PX.Data.BQL.BqlString.Field<custPriceClassID> { }
        protected String _CustPriceClassID;
        [PXDBString(10, InputMask = ">aaaaaaaaaa")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Customer Price Class", Visibility = PXUIVisibility.SelectorVisible)]
        [CustomerPriceClass]
        public virtual String CustPriceClassID
        {
            get
            {
                return this._CustPriceClassID;
            }
            set
            {
                this._CustPriceClassID = value;
            }
        }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        protected Int32? _CustomerID;
        [Customer(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<ARSalesPrice.customerID>>>>))]
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
		#region CustomerCD
		public abstract class customerCD : PX.Data.BQL.BqlString.Field<customerCD> { }
		[PXString(30, IsUnicode = true)]
		public virtual String CustomerCD
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXString(256, IsUnicode = true)]
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
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[ARCrossItem(BAccountField = typeof(customerID), WarningOnNonUniqueSubstitution = true, AllowTemplateItems = true)]
		[PXParent(typeof(Select<InventoryItem, Where<InventoryItem.inventoryID, Equal<Current<ARSalesPrice.inventoryID>>>>))] 
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
		[PXString(IsUnicode = true)]
		public virtual String InventoryCD
		{
			get;
			set;
		}
		#endregion
		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXUIField(DisplayName = "Alternate ID", Visible = false, Visibility = PXUIVisibility.Dynamic)]
		[PXDBString(50, IsUnicode = true, InputMask = "")]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected string _CuryID;
		[PXDBString(5)]
        [PXDefault(typeof(Search<GL.Company.baseCuryID>))]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency")]
		public virtual string CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[INUnit(typeof(ARSalesPrice.inventoryID))]
		[PXFormula(typeof(Selector<ARSalesPrice.inventoryID, InventoryItem.salesUnit>))]
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
		#region IsPromotionalPrice
		public abstract class isPromotionalPrice : PX.Data.BQL.BqlBool.Field<isPromotionalPrice> { }
		protected bool? _IsPromotionalPrice;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Promotion")]
		public virtual bool? IsPromotionalPrice
		{
			get
			{
				return _IsPromotionalPrice;
			}
			set
			{
				_IsPromotionalPrice = value;
			}
		}
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : PX.Data.BQL.BqlDateTime.Field<effectiveDate> { }
		protected DateTime? _EffectiveDate;
		[PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Visibility = PXUIVisibility.Visible)]
		public virtual DateTime? EffectiveDate
		{
			get
			{
				return this._EffectiveDate;
			}
			set
			{
				this._EffectiveDate = value;
			}
		}
		#endregion
		#region SalesPrice
		public abstract class salesPrice : PX.Data.BQL.BqlDecimal.Field<salesPrice> { }
		protected Decimal? _SalesPrice;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBPriceCost]
		[PXUIField(DisplayName = "Price", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? SalesPrice
		{
			get
			{
				return this._SalesPrice;
			}
			set
			{
				this._SalesPrice = value;
			}
		}
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlString.Field<taxID> { }
		protected String _TaxID;
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.Visible)]
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
		#region BreakQty
		public abstract class breakQty : PX.Data.BQL.BqlDecimal.Field<breakQty> { }
		protected Decimal? _BreakQty;
		[PXDBQuantity(MinValue=0)]
		[PXUIField(DisplayName = "Break Qty", Visibility = PXUIVisibility.Visible)]
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
		#region ExpirationDate
		public abstract class expirationDate : PX.Data.BQL.BqlDateTime.Field<expirationDate> { }
		protected DateTime? _ExpirationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
		//[PXFormula(typeof(Switch<Case<Where<ARSalesPrice.isPromotionalPrice, Equal<False>>, Null>, ARSalesPrice.expirationDate>))]
		public virtual DateTime? ExpirationDate
		{
			get
			{
				return this._ExpirationDate;
			}
			set
			{
				this._ExpirationDate = value;
			}
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
		#region IsFairValue
		public abstract class isFairValue : PX.Data.BQL.BqlBool.Field<isFairValue> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fair Value", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsFairValue { get; set; }
		#endregion
		#region IsProrated
		public abstract class isProrated : PX.Data.BQL.BqlBool.Field<isProrated> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prorated", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsProrated { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote()]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region ItemStatus
		public abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus> { }
		[PXString(2, IsFixed = true)]
		public virtual String ItemStatus
		{
			get;
			set;
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		[PXInt]
		public virtual int? ItemClassID
		{
			get;
			set;
		}
		#endregion
		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
		[PXString(30, IsUnicode = true)]
		public virtual String PriceClassID
		{
			get;
			set;
		}
		#endregion
		#region PriceWorkgroupID
		public abstract class priceWorkgroupID : PX.Data.BQL.BqlInt.Field<priceWorkgroupID> { }
		[PXInt]
		public virtual int? PriceWorkgroupID
		{
			get;
			set;
		}
		#endregion
		#region PriceManagerID
		public abstract class priceManagerID : PX.Data.BQL.BqlGuid.Field<priceManagerID> { }
		[PXGuid]
		public virtual Guid? PriceManagerID
		{
			get;
			set;
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
	}

    [PXHidden]
	public partial class ARSalesPrice2 : ARSalesPrice
	{
        #region CustPriceClassID
        public new abstract class custPriceClassID : PX.Data.BQL.BqlString.Field<custPriceClassID> { }
        #endregion
        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region BreakQty
		public new abstract class breakQty : PX.Data.BQL.BqlDecimal.Field<breakQty> { }
		#endregion
		#region SalesPrice
		public new abstract class salesPrice : PX.Data.BQL.BqlDecimal.Field<salesPrice> { }
		#endregion
	}

	public static class PriceTypes
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
					new[]
					{
					  Pair(BasePrice, Messages.BasePrice),
					  Pair(Customer, Messages.Customer),
					  Pair(CustomerPriceClass, Messages.CustomerPriceClass)
					})
			{
				SortByValues = true;
			}
		}

		public class ListWithAllAttribute : PXStringListAttribute
		{
			public ListWithAllAttribute() : base(
					new[]
					{
					  Pair(AllPrices, Messages.AllPrices),
					  Pair(BasePrice, Messages.BasePrice),
					  Pair(Customer, Messages.Customer),
					  Pair(CustomerPriceClass, Messages.CustomerPriceClass)
					})
			{
				SortByValues = true;
			}
		}
		public const string Customer = "C";
		public const string CustomerPriceClass = "P";
        public const string BasePrice = "B";
		public const string AllPrices = "A";

		public class customer : PX.Data.BQL.BqlString.Constant<customer>
		{
			public customer() : base(PriceTypes.Customer) { ;}
		}

		public class customerPriceClass : PX.Data.BQL.BqlString.Constant<customerPriceClass>
		{
			public customerPriceClass() : base(PriceTypes.CustomerPriceClass) { ;}
		}

        public class basePrice : PX.Data.BQL.BqlString.Constant<basePrice>
		{
            public basePrice() : base(PriceTypes.BasePrice) { ;}
        }

		public class allPrices : PX.Data.BQL.BqlString.Constant<allPrices>
		{
			public allPrices() : base(PriceTypes.AllPrices) { ;}
		}
	}
}
