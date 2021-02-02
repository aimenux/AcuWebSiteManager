using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.TM;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.AP;
using PX.Objects.CR;

namespace PX.Objects.IN.S
{
	[Serializable]
	public partial class INItemSite : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INItemSite>.By<inventoryID, siteID>
		{
			public static INItemSite Find(PXGraph graph, int? inventoryID, int? siteID) => FindBy(graph, inventoryID, siteID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemSite>.By<inventoryID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemSite>.By<siteID> { }
			public class PreferredVendor : Vendor.PK.ForeignKeyOf<INItemSite>.By<preferredVendorID> { }
		}
        #endregion
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected = false;
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		public virtual Boolean? Selected
		{
			get
			{
				return this._Selected;
			}
			set
			{
				this._Selected = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem(IsKey = true, DirtyRead = true, DisplayName="Inventory ID")]
		[PXParent(typeof(FK.InventoryItem))]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[IN.Site(IsKey = true)]
		[PXDefault()]
		[PXParent(typeof(FK.Site))]
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
		#region SiteStatus
		public abstract class siteStatus : PX.Data.BQL.BqlString.Field<siteStatus> { }
		protected String _SiteStatus;
		[PXDBString(2, IsFixed = true)]
		[PXDefault("AC")]
		[PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible)]
		[PXStringList(new string[] { "AC", "IN" }, new string[] { "Active", "Inactive" })]
		public virtual String SiteStatus
		{
			get
			{
				return this._SiteStatus;
			}
			set
			{
				this._SiteStatus = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXDefault()]
		public virtual Int32? InvtAcctID
		{
			get
			{
				return this._InvtAcctID;
			}
			set
			{
				this._InvtAcctID = value;
			}
		}
		#endregion
		#region InvtSubID
		public abstract class invtSubID : PX.Data.BQL.BqlInt.Field<invtSubID> { }
		protected Int32? _InvtSubID;
		[SubAccount(typeof(INItemSite.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault()]
		public virtual Int32? InvtSubID
		{
			get
			{
				return this._InvtSubID;
			}
			set
			{
				this._InvtSubID = value;
			}
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>))]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region DfltShipLocationID
		public abstract class dfltShipLocationID : PX.Data.BQL.BqlInt.Field<dfltShipLocationID> { }
		protected Int32? _DfltShipLocationID;
		[IN.Location(typeof(INItemSite.siteID), DisplayName = "Default Issue From", DescriptionField = typeof(INLocation.descr))]
		public virtual Int32? DfltShipLocationID
		{
			get
			{
				return this._DfltShipLocationID;
			}
			set
			{
				this._DfltShipLocationID = value;
			}
		}
		#endregion
		#region DfltReceiptLocationID
		public abstract class dfltReceiptLocationID : PX.Data.BQL.BqlInt.Field<dfltReceiptLocationID> { }
		protected Int32? _DfltReceiptLocationID;
		[IN.Location(typeof(INItemSite.siteID), DisplayName = "Default Receipt To", DescriptionField = typeof(INLocation.descr))]
		public virtual Int32? DfltReceiptLocationID
		{
			get
			{
				return this._DfltReceiptLocationID;
			}
			set
			{
				this._DfltReceiptLocationID = value;
			}
		}
		#endregion
		#region DfltSalesUnit
		public abstract class dfltSalesUnit : PX.Data.BQL.BqlString.Field<dfltSalesUnit> { }
		protected String _DfltSalesUnit;
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Sales Unit")]
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DfltSalesUnit
		{
			get
			{
				return this._DfltSalesUnit;
			}
			set
			{
				this._DfltSalesUnit = value;
			}
		}
		#endregion
		#region DfltPurchaseUnit
		public abstract class dfltPurchaseUnit : PX.Data.BQL.BqlString.Field<dfltPurchaseUnit> { }
		protected String _DfltPurchaseUnit;
		[INUnit(null, typeof(InventoryItem.baseUnit), DisplayName = "Purchase Unit")]
		[PXDefault(typeof(Search<InventoryItem.purchaseUnit, Where<InventoryItem.inventoryID, Equal<Current<INItemSite.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String DfltPurchaseUnit
		{
			get
			{
				return this._DfltPurchaseUnit;
			}
			set
			{
				this._DfltPurchaseUnit = value;
			}
		}
		#endregion
		#region LastStdCost
		public abstract class lastStdCost : PX.Data.BQL.BqlDecimal.Field<lastStdCost> { }
		protected Decimal? _LastStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled = false)]
		public virtual Decimal? LastStdCost
		{
			get
			{
				return this._LastStdCost;
			}
			set
			{
				this._LastStdCost = value;
			}
		}
		#endregion
		#region PendingStdCost
		public abstract class pendingStdCost : PX.Data.BQL.BqlDecimal.Field<pendingStdCost> { }
		protected Decimal? _PendingStdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Cost")]
		public virtual Decimal? PendingStdCost
		{
			get
			{
				return this._PendingStdCost;
			}
			set
			{
				this._PendingStdCost = value;
			}
		}
		#endregion
		#region PendingStdCostDate
		public abstract class pendingStdCostDate : PX.Data.BQL.BqlDateTime.Field<pendingStdCostDate> { }
		protected DateTime? _PendingStdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Cost Date")]
		public virtual DateTime? PendingStdCostDate
		{
			get
			{
				return this._PendingStdCostDate;
			}
			set
			{
				this._PendingStdCostDate = value;
			}
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
		protected Decimal? _StdCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region StdCostDate
		public abstract class stdCostDate : PX.Data.BQL.BqlDateTime.Field<stdCostDate> { }
		protected DateTime? _StdCostDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? StdCostDate
		{
			get
			{
				return this._StdCostDate;
			}
			set
			{
				this._StdCostDate = value;
			}
		}
		#endregion
		#region LastBasePrice
		public abstract class lastBasePrice : PX.Data.BQL.BqlDecimal.Field<lastBasePrice> { }
		protected Decimal? _LastBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Price", Enabled = false)]
		public virtual Decimal? LastBasePrice
		{
			get
			{
				return this._LastBasePrice;
			}
			set
			{
				this._LastBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePrice
		public abstract class pendingBasePrice : PX.Data.BQL.BqlDecimal.Field<pendingBasePrice> { }
		protected Decimal? _PendingBasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Pending Price")]
		public virtual Decimal? PendingBasePrice
		{
			get
			{
				return this._PendingBasePrice;
			}
			set
			{
				this._PendingBasePrice = value;
			}
		}
		#endregion
		#region PendingBasePriceDate
		public abstract class pendingBasePriceDate : PX.Data.BQL.BqlDateTime.Field<pendingBasePriceDate> { }
		protected DateTime? _PendingBasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Price Date", Enabled = false)]
		public virtual DateTime? PendingBasePriceDate
		{
			get
			{
				return this._PendingBasePriceDate;
			}
			set
			{
				this._PendingBasePriceDate = value;
			}
		}
		#endregion
		#region BasePrice
		public abstract class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		protected Decimal? _BasePrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Current Price", Enabled = false)]
		public virtual Decimal? BasePrice
		{
			get
			{
				return this._BasePrice;
			}
			set
			{
				this._BasePrice = value;
			}
		}
		#endregion
		#region BasePriceDate
		public abstract class basePriceDate : PX.Data.BQL.BqlDateTime.Field<basePriceDate> { }
		protected DateTime? _BasePriceDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Effective Date", Enabled = false)]
		public virtual DateTime? BasePriceDate
		{
			get
			{
				return this._BasePriceDate;
			}
			set
			{
				this._BasePriceDate = value;
			}
		}
		#endregion						
		
		#region PreferredVendorOverride
		public abstract class preferredVendorOverride : PX.Data.BQL.BqlBool.Field<preferredVendorOverride> { }
		protected Boolean? _PreferredVendorOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Preferred Vendor Override")]
		public virtual Boolean? PreferredVendorOverride
		{
			get
			{
				return this._PreferredVendorOverride;
			}
			set
			{
				this._PreferredVendorOverride = value;
			}
		}
		#endregion
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }
		protected Int32? _PreferredVendorID;
		[AP.VendorActive(DisplayName = "Preferred Vendor", Required = false, DescriptionField = typeof(Vendor.acctName))]
		public virtual Int32? PreferredVendorID
		{
			get
			{
				return this._PreferredVendorID;
			}
			set
			{
				this._PreferredVendorID = value;
			}
		}
		#endregion
		#region PreferredVendorLocationID
		public abstract class preferredVendorLocationID : PX.Data.BQL.BqlInt.Field<preferredVendorLocationID> { }
		protected Int32? _PreferredVendorLocationID;
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<INItemSite.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Preferred Location")]
		[PXDefault(typeof(Search<Vendor.defLocationID, Where<Vendor.bAccountID, Equal<Current<INItemSite.preferredVendorID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<INItemSite.preferredVendorID>))]
		public virtual Int32? PreferredVendorLocationID
		{
			get
			{
				return this._PreferredVendorLocationID;
			}
			set
			{
				this._PreferredVendorLocationID = value;
			}
		}
		#endregion

		#region ProductWorkgroupID
		public abstract class productWorkgroupID : PX.Data.BQL.BqlInt.Field<productWorkgroupID> { }
		protected Int32? _ProductWorkgroupID;
		[PXDBInt()]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Product Workgroup")]
		public virtual Int32? ProductWorkgroupID
		{
			get
			{
				return this._ProductWorkgroupID;
			}
			set
			{
				this._ProductWorkgroupID = value;
			}
		}
		#endregion
		#region ProductManagerID
		public abstract class productManagerID : PX.Data.BQL.BqlGuid.Field<productManagerID> { }
		protected Guid? _ProductManagerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(INItemSite.productWorkgroupID))]
		[PXUIField(DisplayName = "Product Manager")]
		public virtual Guid? ProductManagerID
		{
			get
			{
				return this._ProductManagerID;
			}
			set
			{
				this._ProductManagerID = value;
			}
		}
		#endregion
		#region PriceWorkgroupID
		public abstract class priceWorkgroupID : PX.Data.BQL.BqlInt.Field<priceWorkgroupID> { }
		protected Int32? _PriceWorkgroupID;
		[PXDBInt()]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Price Workgroup")]
		public virtual Int32? PriceWorkgroupID
		{
			get
			{
				return this._PriceWorkgroupID;
			}
			set
			{
				this._PriceWorkgroupID = value;
			}
		}
		#endregion
		#region PriceManagerID
		public abstract class priceManagerID : PX.Data.BQL.BqlGuid.Field<priceManagerID> { }
		protected Guid? _PriceManagerID;
		[PXDBGuid()]
		[PXOwnerSelector(typeof(INItemSite.priceWorkgroupID))]
		[PXUIField(DisplayName = "Price Manager")]
		public virtual Guid? PriceManagerID
		{
			get
			{
				return this._PriceManagerID;
			}
			set
			{
				this._PriceManagerID = value;
			}
		}
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected Boolean? _IsDefault;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public virtual Boolean? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
		#region StdCostOverride
		public abstract class stdCostOverride : PX.Data.BQL.BqlBool.Field<stdCostOverride> { }
		protected Boolean? _StdCostOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Std. Cost Override")]
		public virtual Boolean? StdCostOverride
		{
			get
			{
				return this._StdCostOverride;
			}
			set
			{
				this._StdCostOverride = value;
			}
		}
		#endregion
		#region BasePriceOverride
		public abstract class basePriceOverride : PX.Data.BQL.BqlBool.Field<basePriceOverride> { }
		protected Boolean? _BasePriceOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Price Override")]
		public virtual Boolean? BasePriceOverride
		{
			get
			{
				return this._BasePriceOverride;
			}
			set
			{
				this._BasePriceOverride = value;
			}
		}
		#endregion
		#region Commissionable
		public abstract class commissionable : PX.Data.BQL.BqlBool.Field<commissionable> { }
		protected Boolean? _Commissionable;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Subject to Commission", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Commissionable
		{
			get
			{
				return this._Commissionable;
			}
			set
			{
				this._Commissionable = value;
			}
		}
		#endregion	
		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.BQL.BqlString.Field<aBCCodeID> { }
		protected String _ABCCodeID;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "ABC Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INABCCode.aBCCodeID), DescriptionField = typeof(INABCCode.descr))]
		public virtual String ABCCodeID
		{
			get
			{
				return this._ABCCodeID;
			}
			set
			{
				this._ABCCodeID = value;
			}
		}
		#endregion
		#region ABCCodeIsFixed
		public abstract class aBCCodeIsFixed : PX.Data.BQL.BqlBool.Field<aBCCodeIsFixed> { }
		protected Boolean? _ABCCodeIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed ABC Code")]
		public virtual Boolean? ABCCodeIsFixed
		{
			get
			{
				return this._ABCCodeIsFixed;
			}
			set
			{
				this._ABCCodeIsFixed = value;
			}
		}
		#endregion
		#region MovementClassID
		public abstract class movementClassID : PX.Data.BQL.BqlString.Field<movementClassID> { }
		protected String _MovementClassID;
		[PXDBString(1)]
		[PXUIField(DisplayName = "Movement Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(INMovementClass.movementClassID), DescriptionField = typeof(INMovementClass.descr))]
		public virtual String MovementClassID
		{
			get
			{
				return this._MovementClassID;
			}
			set
			{
				this._MovementClassID = value;
			}
		}
		#endregion
		#region MovementClassIsFixed
		public abstract class movementClassIsFixed : PX.Data.BQL.BqlBool.Field<movementClassIsFixed> { }
		protected Boolean? _MovementClassIsFixed;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Fixed Movement Class")]
		public virtual Boolean? MovementClassIsFixed
		{
			get
			{
				return this._MovementClassIsFixed;
			}
			set
			{
				this._MovementClassIsFixed = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
		#region POCreate
		public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }
		protected Boolean? _POCreate;
		[PXDBCalced(
			typeof(Switch<Case<Where<INItemSite.replenishmentSource, Equal<INReplenishmentSource.purchaseToOrder>,
														Or<INItemSite.replenishmentSource, Equal<INReplenishmentSource.dropShipToOrder>>>, boolTrue>, boolFalse>), typeof(bool))]
		public virtual Boolean? POCreate
		{
			get
			{
				return this._POCreate;
			}
			set
			{
				this._POCreate = value ?? false;
			}
		}
		#endregion

		#region MarkupPct
		public abstract class markupPct : PX.Data.BQL.BqlDecimal.Field<markupPct> { }
		protected Decimal? _MarkupPct;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Markup %", Enabled = false)]
		public virtual Decimal? MarkupPct
		{
			get
			{
				return this._MarkupPct;
			}
			set
			{
				this._MarkupPct = value;
			}
		}
		#endregion
		#region MarkupPctOverride
		public abstract class markupPctOverride : PX.Data.BQL.BqlBool.Field<markupPctOverride> { }
		protected Boolean? _MarkupPctOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Markup % Override")]
		public virtual Boolean? MarkupPctOverride
		{
			get
			{
				return this._MarkupPctOverride;
			}
			set
			{
				this._MarkupPctOverride = value;
			}
		}
		#endregion

		#region RecPrice
		public abstract class recPrice : PX.Data.BQL.BqlDecimal.Field<recPrice> { }
		protected Decimal? _RecPrice;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "MSRP", Enabled = false)]
		public virtual Decimal? RecPrice
		{
			get
			{
				return this._RecPrice;
			}
			set
			{
				this._RecPrice = value;
			}
		}
		#endregion
		#region RecPriceOverride
		public abstract class recPriceOverride : PX.Data.BQL.BqlBool.Field<recPriceOverride> { }
		protected Boolean? _RecPriceOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Price Override")]
		public virtual Boolean? RecPriceOverride
		{
			get
			{
				return this._RecPriceOverride;
			}
			set
			{
				this._RecPriceOverride = value;
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

		#region ReplenishmentPolicyOverride
		public abstract class replenishmentPolicyOverride : PX.Data.BQL.BqlBool.Field<replenishmentPolicyOverride> { }
		protected Boolean? _ReplenishmentPolicyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Replenishment Policy Override")]
		public virtual Boolean? ReplenishmentPolicyOverride
		{
			get
			{
				return this._ReplenishmentPolicyOverride;
			}
			set
			{
				this._ReplenishmentPolicyOverride = value;
			}
		}
		#endregion
		#region ReplenishmentPolicyID
		public abstract class replenishmentPolicyID : PX.Data.BQL.BqlString.Field<replenishmentPolicyID> { }
		protected String _ReplenishmentPolicyID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Seasonality")]
		[PXSelector(typeof(Search<INReplenishmentPolicy.replenishmentPolicyID>), DescriptionField = typeof(INReplenishmentPolicy.descr))]
		public virtual String ReplenishmentPolicyID
		{
			get
			{
				return this._ReplenishmentPolicyID;
			}
			set
			{
				this._ReplenishmentPolicyID = value;
			}
		}
		#endregion		
		#region ReplenishmentSource
		public abstract class replenishmentSource : PX.Data.BQL.BqlString.Field<replenishmentSource> { }
		protected string _ReplenishmentSource;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Replenishment Source")]
		[INReplenishmentSource.List]				
		public virtual string ReplenishmentSource
		{
			get
			{
				return this._ReplenishmentSource;
			}
			set
			{
				this._ReplenishmentSource = value;
			}
		}
		#endregion
		#region ReplenishmentSourceSiteID
		public abstract class replenishmentSourceSiteID : PX.Data.BQL.BqlInt.Field<replenishmentSourceSiteID> { }
		protected Int32? _ReplenishmentSourceSiteID;
		[IN.Site(DisplayName = "Replenishment Warehouse", DescriptionField = typeof(INSite.descr))]
		public virtual Int32? ReplenishmentSourceSiteID
		{
			get
			{
				return this._ReplenishmentSourceSiteID;
			}
			set
			{
				this._ReplenishmentSourceSiteID = value;
			}
		}
		#endregion
		#region ReplenishmentMethod
		public abstract class replenishmentMethod : PX.Data.BQL.BqlString.Field<replenishmentMethod> { }
		protected String _ReplenishmentMethod;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Replenishment Method", Enabled = false)]
		[INReplenishmentMethod.List]
		public virtual String ReplenishmentMethod
		{
			get
			{
				return this._ReplenishmentMethod;
			}
			set
			{
				this._ReplenishmentMethod = value;
			}
		}
		#endregion

		#region MaxShelfLifeOverride
		public abstract class maxShelfLifeOverride : PX.Data.BQL.BqlBool.Field<maxShelfLifeOverride> { }
		protected Boolean? _MaxShelfLifeOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MaxShelfLifeOverride
		{
			get
			{
				return this._MaxShelfLifeOverride;
			}
			set
			{
				this._MaxShelfLifeOverride = value;
			}
		}
		#endregion
		#region MaxShelfLife
		public abstract class maxShelfLife : PX.Data.BQL.BqlInt.Field<maxShelfLife> { }
		protected Int32? _MaxShelfLife;
		[PXDBInt()]
		[PXUIField(DisplayName = "Max. Shelf Life (Days)")]
		public virtual Int32? MaxShelfLife
		{
			get
			{
				return this._MaxShelfLife;
			}
			set
			{
				this._MaxShelfLife = value;
			}
		}
		#endregion
		#region LaunchDateOverride
		public abstract class launchDateOverride : PX.Data.BQL.BqlBool.Field<launchDateOverride> { }
		protected Boolean? _LaunchDateOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? LaunchDateOverride
		{
			get
			{
				return this._LaunchDateOverride;
			}
			set
			{
				this._LaunchDateOverride = value;
			}
		}
		#endregion
		#region LaunchDate
		public abstract class launchDate : PX.Data.BQL.BqlDateTime.Field<launchDate> { }
		protected DateTime? _LaunchDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Launch Date")]
		public virtual DateTime? LaunchDate
		{
			get
			{
				return this._LaunchDate;
			}
			set
			{
				this._LaunchDate = value;
			}
		}
		#endregion
		#region TerminationDateOverride
		public abstract class terminationDateOverride : PX.Data.BQL.BqlBool.Field<terminationDateOverride> { }
		protected Boolean? _TerminationDateOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? TerminationDateOverride
		{
			get
			{
				return this._TerminationDateOverride;
			}
			set
			{
				this._TerminationDateOverride = value;
			}
		}
		#endregion
		#region TerminationDate
		public abstract class terminationDate : PX.Data.BQL.BqlDateTime.Field<terminationDate> { }
		protected DateTime? _TerminationDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Termination Date")]
		public virtual DateTime? TerminationDate
		{
			get
			{
				return this._TerminationDate;
			}
			set
			{
				this._TerminationDate = value;
			}
		}
		#endregion
		#region SafetyStockOverride
		public abstract class safetyStockOverride : PX.Data.BQL.BqlBool.Field<safetyStockOverride> { }
		protected Boolean? _SafetyStockOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? SafetyStockOverride
		{
			get
			{
				return this._SafetyStockOverride;
			}
			set
			{
				this._SafetyStockOverride = value;
			}
		}
		#endregion
		#region SafetyStock
		public abstract class safetyStock : PX.Data.BQL.BqlDecimal.Field<safetyStock> { }
		protected Decimal? _SafetyStock;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Safety Stock")]
		public virtual Decimal? SafetyStock
		{
			get
			{
				return this._SafetyStock;
			}
			set
			{
				this._SafetyStock = value;
			}
		}
		#endregion
		#region MinQtyOverride
		public abstract class minQtyOverride : PX.Data.BQL.BqlBool.Field<minQtyOverride> { }
		protected Boolean? _MinQtyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MinQtyOverride
		{
			get
			{
				return this._MinQtyOverride;
			}
			set
			{
				this._MinQtyOverride = value;
			}
		}
		#endregion
		#region MinQty
		public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }
		protected Decimal? _MinQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Reorder Point")]
		public virtual Decimal? MinQty
		{
			get
			{
				return this._MinQty;
			}
			set
			{
				this._MinQty = value;
			}
		}
		#endregion
		#region MaxQtyOverride
		public abstract class maxQtyOverride : PX.Data.BQL.BqlBool.Field<maxQtyOverride> { }
		protected Boolean? _MaxQtyOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? MaxQtyOverride
		{
			get
			{
				return this._MaxQtyOverride;
			}
			set
			{
				this._MaxQtyOverride = value;
			}
		}
		#endregion
		#region MaxQty
		public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty> { }
		protected Decimal? _MaxQty;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Max Qty.")]
		public virtual Decimal? MaxQty
		{
			get
			{
				return this._MaxQty;
			}
			set
			{
				this._MaxQty = value;
			}
		}
		#endregion
		#region TransferERQOverride
		public abstract class transferERQOverride : PX.Data.BQL.BqlBool.Field<transferERQOverride> { }
		protected Boolean? _TransferERQOverride;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override")]
		public virtual Boolean? TransferERQOverride
		{
			get
			{
				return this._TransferERQOverride;
			}
			set
			{
				this._TransferERQOverride = value;
			}
		}
		#endregion
		#region TransferERQ
		public abstract class transferERQ : PX.Data.BQL.BqlDecimal.Field<transferERQ> { }
		protected Decimal? _TransferERQ;
		[PXDBQuantity]
		[PXUIField(DisplayName = "Transfer ERQ")]
		public virtual Decimal? TransferERQ
		{
			get
			{
				return this._TransferERQ;
			}
			set
			{
				this._TransferERQ = value;
			}
		}
		#endregion

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
	}

	[System.SerializableAttribute()]
    [PXHidden]
	public partial class INItemStats : PX.Data.IBqlTable
	{
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
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
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<InventoryItem.valMethod, Where<InventoryItem.inventoryID, Equal<Current<INItemStats.inventoryID>>>>))]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region StdCost
		public abstract class stdCost : PX.Data.BQL.BqlDecimal.Field<stdCost> { }
		protected Decimal? _StdCost;
		[PXDBCostScalar(typeof(Search<INItemSite.stdCost, Where<INItemSite.inventoryID, Equal<INItemStats.inventoryID>, And<INItemSite.siteID, Equal<INItemStats.siteID>>>>))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<InventoryItem.stdCost, Where<INItemSite.inventoryID, Equal<Current<INItemStats.inventoryID>>, And<INItemSite.siteID, Equal<Current<INItemStats.siteID>>>>>))]
		[PXUIField(DisplayName = "Current Cost", Enabled = false)]
		public virtual Decimal? StdCost
		{
			get
			{
				return this._StdCost;
			}
			set
			{
				this._StdCost = value;
			}
		}
		#endregion
		#region LastCost
		public abstract class lastCost : PX.Data.BQL.BqlDecimal.Field<lastCost> { }
		protected Decimal? _LastCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Cost", Enabled = false)]
		public virtual Decimal? LastCost
		{
			get
			{
				return this._LastCost;
			}
			set
			{
				this._LastCost = value;
			}
		}
		#endregion
		#region LastCostDate
		public abstract class lastCostDate : PX.Data.BQL.BqlDateTime.Field<lastCostDate> { }
		protected DateTime? _LastCostDate;
		[PXDBDate(PreserveTime = true)]
        [PXDefault(TypeCode.DateTime, "01/01/1900")]
        public virtual DateTime? LastCostDate
		{
			get
			{
				return this._LastCostDate;
			}
			set
			{
				this._LastCostDate = value;
			}
		}
		#endregion
		#region AvgCost
		public abstract class avgCost : PX.Data.BQL.BqlDecimal.Field<avgCost> { }
		protected Decimal? _AvgCost;
		[PXDBPriceCostCalced(typeof(Switch<Case<Where<INItemStats.qtyOnHand, Equal<decimal0>>, decimal0>, Div<INItemStats.totalCost, INItemStats.qtyOnHand>>), typeof(Decimal), CastToScale = 9, CastToPrecision = 25)]
		[PXPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Average Cost", Enabled = false)]
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
		}
		#endregion
		#region MinCost
		public abstract class minCost : PX.Data.BQL.BqlDecimal.Field<minCost> { }
		protected Decimal? _MinCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Minimal Cost", Enabled = false)]
		public virtual Decimal? MinCost
		{
			get
			{
				return this._MinCost;
			}
			set
			{
				this._MinCost = value;
			}
		}
		#endregion
		#region MaxCost
		public abstract class maxCost : PX.Data.BQL.BqlDecimal.Field<maxCost> { }
		protected Decimal? _MaxCost;
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Max. Cost", Enabled = false)]
		public virtual Decimal? MaxCost
		{
			get
			{
				return this._MaxCost;
			}
			set
			{
				this._MaxCost = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected decimal? _TotalCost;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region QtyReceived
		public abstract class qtyReceived : PX.Data.BQL.BqlDecimal.Field<qtyReceived> { }
		protected Decimal? _QtyReceived;
		[PXDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? QtyReceived
		{
			get
			{
				return this._QtyReceived;
			}
			set
			{
				this._QtyReceived = value;
			}
		}
		#endregion
		#region CostReceived
		public abstract class costReceived : PX.Data.BQL.BqlDecimal.Field<costReceived> { }
		protected decimal? _CostReceived;
		[PXDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Decimal? CostReceived
		{
			get
			{
				return this._CostReceived;
			}
			set
			{
				this._CostReceived = value;
			}
		}
		#endregion
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
	} 
}
