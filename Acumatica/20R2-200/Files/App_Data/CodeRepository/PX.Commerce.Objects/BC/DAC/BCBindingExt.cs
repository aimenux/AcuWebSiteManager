using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Objects.GL;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Commerce.Core;
using PX.Objects.CA;
using PX.Objects.CR;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Api;

namespace PX.Commerce.Objects
{

	[Serializable]
	[PXCacheName("Store Settings")]
	public class BCBindingExt : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BCBindingExt>.By<BCBindingExt.bindingID>
		{
			public static BCBindingExt Find(PXGraph graph, int? binding) => FindBy(graph, binding);
		}
		public static class FK
		{
			public class CustomerGuest : Customer.PK.ForeignKeyOf<BCBinding>.By<guestCustomerID> { }
			public class OrderType : SOOrderType.PK.ForeignKeyOf<BCBinding>.By<orderType> { }
			public class ReturnOrderType : SOOrderType.PK.ForeignKeyOf<BCBinding>.By<returnorderType> { }
			public class GiftCertificate : InventoryItem.PK.ForeignKeyOf<BCBinding>.By<giftCertificateItemID> { }
			public class NonStockItemClass : INItemClass.PK.ForeignKeyOf<BCBinding>.By<nonStockItemClassID> { }
			public class StockItemClass : INItemClass.PK.ForeignKeyOf<BCBinding>.By<stockItemClassID> { }
		}
		#endregion

		#region BindingID
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(BCBinding.bindingID))]
		[PXUIField(DisplayName = "Store", Visible = false)]
		[PXParent(typeof(Select<BCBinding, Where<BCBinding.bindingID, Equal<Current<BCBindingExt.bindingID>>>>))]
		public int? BindingID { get; set; }
		public abstract class bindingID : IBqlField { }
		#endregion

		//Numberings & Keys
		#region CustomerNumberingID
		[PXUIField(DisplayName = "Customer Autonumbering", Visibility = PXUIVisibility.Visible)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Address, BCEntitiesAttribute.Order })]

		public virtual string CustomerNumberingID { get; set; }
		public abstract class customerNumberingID : IBqlField { }
		#endregion
		#region LocationNumberingID
		[PXUIField(DisplayName = "Location Autonumbering", Visibility = PXUIVisibility.Visible)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Address })]
		public virtual string LocationNumberingID { get; set; }
		public abstract class locationNumberingID : IBqlField { }
		#endregion
		#region InventoryNumberingID
		[PXUIField(DisplayName = "Inventory Numbering", Visible = false)]
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXRestrictor(typeof(Where<Numbering.userNumbering, NotEqual<True>>), "Manual numbering sequences are not supported")]
		public virtual string InventoryNumberingID { get; set; }
		public abstract class inventoryNumberingID : IBqlField { }
		#endregion
		#region CustomerTemplate
		[PXUIField(DisplayName = "Customer Numbering Template", Visibility = PXUIVisibility.Visible)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(CustomerRawAttribute.DimensionName, typeof(BCBindingExt.locationNumberingID))]
		public virtual string CustomerTemplate { get; set; }
		public abstract class customerTemplate : IBqlField { }
		#endregion
		#region LocationTemplate
		[PXUIField(DisplayName = "Location Numbering Template", Visibility = PXUIVisibility.Visible)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(LocationIDAttribute.DimensionName, typeof(BCBindingExt.locationNumberingID))]
		public virtual string LocationTemplate { get; set; }
		public abstract class locationTemplate : IBqlField { }
		#endregion
		#region InventoryTemplate
		[PXUIField(DisplayName = "Inventory Template", Visible = false)]
		[PXDBString(30, IsUnicode = true)]
		[BCDimensionMaskAttribute(InventoryAttribute.DimensionName, typeof(BCBindingExt.inventoryNumberingID))]
		public virtual string InventoryTemplate { get; set; }
		public abstract class inventoryTemplate : IBqlField { }
		#endregion

		//Customer
		#region CustomerClassID
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Customer Class")]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr))]
		[PXDefault(typeof(ARSetup.dfltCustomerClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Address, BCEntitiesAttribute.Order})]
		public virtual string CustomerClassID { get; set; }
		public abstract class customerClassID : IBqlField { }
		#endregion
		#region GuestCustomerID
		[PXDBInt()]
		[PXUIField(DisplayName = "Generic Guest Customer")]
		[PXDimensionSelector("BIZACCT", typeof(Search<Customer.bAccountID>), typeof(Customer.acctCD),
			typeof(Customer.acctCD), 
			typeof(Customer.acctName),
			typeof(Customer.customerClassID), 
			typeof(Customer.status),
			DescriptionField = typeof(Customer.acctName), 
			DirtyRead = true)]
		[PXRestrictor(typeof(Where<Customer.status, IsNull,
						Or<Customer.status, Equal<BAccount.status.active>,
						Or<Customer.status, Equal<BAccount.status.oneTime>>>>), PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Customer, BCEntitiesAttribute.Order })]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? GuestCustomerID { get; set; }
		public abstract class guestCustomerID : IBqlField { }
		#endregion

		//Inventory
		#region StockItemClassID
		[PXDBInt]
		[PXUIField(DisplayName = "Stock Item Class", Visible = false)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolTrue>>>),
			SubstituteKey = typeof(INItemClass.itemClassCD),
			DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(INSetup.dfltStkItemClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? StockItemClassID { get; set; }
		public abstract class stockItemClassID : IBqlField { }
		#endregion
		#region NonStockItemClassID
		[PXDBInt]
		[PXUIField(DisplayName = "Non-Stock Item Class", Visible = false)]
		[PXSelector(typeof(Search<INItemClass.itemClassID, Where<INItemClass.stkItem, Equal<boolFalse>>>),
			SubstituteKey = typeof(INItemClass.itemClassCD),
			DescriptionField = typeof(INItemClass.descr))]
		[PXDefault(typeof(INSetup.dfltNonStkItemClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? NonStockItemClassID { get; set; }
		public abstract class nonStockItemClassID : IBqlField { }
		#endregion
		#region StockSalesCategoriesIDs
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Stock Categories")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SalesCategories()]
		public virtual string StockSalesCategoriesIDs { get; set; }
		public abstract class stockSalesCategoriesIDs : IBqlField { }
		#endregion
		#region NonStockSalesCategoryID
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Non-Stock Categories")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[SalesCategories()]
		public virtual string NonStockSalesCategoriesIDs { get; set; }
		public abstract class nonStockSalesCategoriesIDs : IBqlField { }
		#endregion
		#region RelatedItems
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Related Items")]
		[RelatedItemsAttribute()]
		public virtual string RelatedItems { get; set; }
		public abstract class relatedItems : IBqlField { }
		#endregion

		//Availability
		#region Availability
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Availability")]
		[BCItemAvailabilities.List]
		[PXDefault(BCItemAvailabilities.AvailableSkip)]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string Availability { get; set; }
		public abstract class availability : IBqlField { }
		#endregion
		#region NotAvailMode
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "When Qty Unavailable")]
		[BCItemNotAvailModes.List]
		[PXDefault(BCItemNotAvailModes.DoNothing)]
		[PXUIEnabled(typeof(Where<BCBindingExt.availability, Equal<BCItemAvailabilities.availableTrack>>))]
		[PXFormula(typeof(Default<BCBindingExt.availability>))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string NotAvailMode { get; set; }
		public abstract class notAvailMode : IBqlField { }
		#endregion
		#region AvailabilityCalcRule
		[PXDBString(1)]
		[PXUIField(DisplayName = "Availability Mode")]
		[PXDefault(BCAvailabilityLevelsAttribute.AvailableForShipping,
			PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[BCAvailabilityLevels]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.ProductAvailability })]
		public virtual string AvailabilityCalcRule { get; set; }
		public abstract class availabilityCalcRule : IBqlField { }
        #endregion
        #region WarehouseMode
        [PXDBString(1)]
        [PXUIField(DisplayName = "Warehouse Mode")]
        [PXDefault(BCWarehouseModeAttribute.AllWarehouse, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [BCWarehouseMode]
        public virtual string WarehouseMode { get; set; }
        public abstract class warehouseMode : IBqlField { }
        #endregion

		//Orders
		#region OrderType
		public class orderRMAType : PX.Data.BQL.BqlString.Constant<orderRMAType>
		{
			public const string OrderRmaType = "RM";
			public orderRMAType() : base(OrderRmaType) { }
		}

		public class orderCRType : PX.Data.BQL.BqlString.Constant<orderCRType>
		{
			public const string OrderCRType = "CR";
			public orderCRType() : base(OrderCRType) { }
		}

		[PXDBString(2, IsFixed = true, InputMask = "")]
		[PXSelector(
			typeof(Search<SOOrderType.orderType, 
				Where<SOOrderType.active, Equal<True>,
					And<SOOrderType.behavior, NotEqual<orderRMAType>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[PXUIField(DisplayName = "Order Type")]
		[PXDefault(typeof(SOSetup.defaultOrderType), PersistingCheck = PXPersistingCheck.Nothing)]
		[BCSettingsChecker(new string[] {  BCEntitiesAttribute.Order, BCEntitiesAttribute.Shipment })]
		public virtual string OrderType { get; set; }
		public abstract class orderType : IBqlField { }
		#endregion
		#region ReturnOrderType
		[PXDBString(2, IsFixed = true, InputMask = "")]
		[PXUIField(DisplayName = "Return Order Type")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(
			typeof(Search<SOOrderType.orderType,
				Where<SOOrderType.active, Equal<True>,
					And<SOOrderType.template, Equal<orderCRType>>>>),
			DescriptionField = typeof(SOOrderType.descr))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.OrderRefunds })]
		public virtual string ReturnOrderType { get; set; }
		public abstract class returnorderType : IBqlField { }
		#endregion
		#region OrderTimeZone
		public abstract class orderTimeZone : PX.Data.BQL.BqlString.Field<orderTimeZone> { }
		[PXDBString(32)]
		[PXUIField(DisplayName = "Order Time Zone")]
		[PXTimeZone]
		[PXDefault(typeof(Search<PX.SM.PreferencesGeneral.timeZone>), PersistingCheck = PXPersistingCheck.Nothing )]
		public virtual String OrderTimeZone { get; set; }

		#endregion

		#region GiftCertificateItemID
		[PXDBInt()]
		[PXUIField(DisplayName = "Gift Certificate Item")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
				And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		public virtual Int32? GiftCertificateItemID { get; set; }
		public abstract class giftCertificateItemID : IBqlField { }
		#endregion

		#region RefundAmountItemID
		[PXDBInt()]
		[PXUIField(DisplayName = "Refund Amount Item")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<InventoryItem.inventoryID,
			Where<InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
				And<InventoryItem.itemType, Equal<INItemTypes.nonStockItem>,
				And<InventoryItem.nonStockReceipt,Equal<False>,
					And<InventoryItem.nonStockShip,Equal<False>>>>>>),
			SubstituteKey = typeof(InventoryItem.inventoryCD),
			DescriptionField = typeof(InventoryItem.descr))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.OrderRefunds })]
		public virtual Int32? RefundAmountItemID { get; set; }
		public abstract class refundAmountItemID : IBqlField { }
		#endregion

		#region ReasonCode
		[PXDBString()]
		[PXUIField(DisplayName = "Refund Reason Code")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<ReasonCode.reasonCodeID, Where<ReasonCode.usage, Equal<ReasonCodeUsages.issue>
				>>),
			DescriptionField = typeof(ReasonCode.descr))]
		[BCSettingsChecker(new string[] { BCEntitiesAttribute.Order, BCEntitiesAttribute.OrderRefunds })]
		public virtual string ReasonCode { get; set; }
		public abstract class reasonCode : IBqlField { }
		#endregion

		#region PostDiscounts
		[PXDBString(1)]
		[PXUIField(DisplayName = "Show Discounts In")]
		[PXDefault(BCPostDiscountAttribute.DocumentDiscount)]
		[BCPostDiscount]
		public virtual string PostDiscounts { get; set; }
		public abstract class postDiscounts : IBqlField { }
		#endregion

		//Taxes Synchronization
		#region SyncTaxes
		[PXDBString(1)]
		[PXUIField(DisplayName = "Synchronize Taxes")]
		[PXDefault(BCTaxSyncAttribute.NoSync)]
		[BCTaxSyncAttribute()]
		public virtual string SyncTaxes { get; set; }
		public abstract class syncTaxes : IBqlField { }
		#endregion
		#region DefaultTaxZoneID
		[PXDBString]
		[PXUIField(DisplayName = "Default Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<Current<BCBindingExt.primaryTaxZoneID>, IsNull, And<Current<BCBindingExt.syncTaxes>, NotEqual<BCTaxSyncAttribute.noSync>>>))]
		[PXFormula(typeof(Default<BCBindingExt.primaryTaxZoneID>))]
		[PXFormula(typeof(Default<BCBindingExt.syncTaxes>))]
		public virtual string DefaultTaxZoneID { get; set; }
		public abstract class defaultTaxZoneID : IBqlField { }
		#endregion
		#region PrimaryTaxZoneID
		[PXDBString]
		[PXUIField(DisplayName = "Primary Tax Zone")]
		[PXSelector(typeof(TaxZone.taxZoneID), DescriptionField = typeof(TaxZone.descr))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<Current<BCBindingExt.syncTaxes>, NotEqual<BCTaxSyncAttribute.noSync>>))]
		[PXFormula(typeof(Default<BCBindingExt.syncTaxes>))]
		public virtual string PrimaryTaxZoneID { get; set; }
		public abstract class primaryTaxZoneID : IBqlField { }
		#endregion
		#region Tax
		public abstract class taxSubstitutionListID : PX.Data.BQL.BqlString.Field<taxSubstitutionListID> { }
		[PXDBString(16)]
		[PXUIField(DisplayName = "Tax Substitution List")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual String TaxSubstitutionListID { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategorySubstitutionListID : PX.Data.BQL.BqlString.Field<taxCategorySubstitutionListID> { }
		[PXDBString(16)]
		[PXUIField(DisplayName = "Tax Category Substitution List")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual String TaxCategorySubstitutionListID { get; set; }
		#endregion
	}
}