using PX.Common;
using PX.Data;
using System;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.SO;
using PX.Objects.Common.Bql;
using PX.Objects.CM;
using PX.Objects.Common;

namespace PX.Objects.FS
{
	[System.SerializableAttribute()]
	[PXProjection(typeof(Select2<InventoryItem,
		LeftJoin<INSiteStatus,
						On<INSiteStatus.inventoryID, Equal<InventoryItem.inventoryID>,
						And<InventoryItem.stkItem, Equal<boolTrue>,
						And<INSiteStatus.siteID, NotEqual<SiteAttribute.transitSiteID>>>>,
		LeftJoin<INSubItem,
						On<INSiteStatus.FK.SubItem>,
		LeftJoin<INSite,
						On<INSiteStatus.FK.Site>,
		LeftJoin<INItemXRef,
						On<INItemXRef.inventoryID, Equal<InventoryItem.inventoryID>,
						And2<Where<INItemXRef.subItemID, Equal<INSiteStatus.subItemID>,
								Or<INSiteStatus.subItemID, IsNull>>,
						And<Where<CurrentValue<FSSiteStatusFilter.barCode>, IsNotNull,
						And<INItemXRef.alternateType, Equal<INAlternateType.barcode>>>>>>,
		LeftJoin<INItemPartNumber,
						On<INItemPartNumber.inventoryID, Equal<InventoryItem.inventoryID>,
						And<INItemPartNumber.alternateID, Like<CurrentValue<FSSiteStatusFilter.inventory_Wildcard>>,
						And2<Where<INItemPartNumber.bAccountID, Equal<Zero>,
									  Or<INItemPartNumber.bAccountID, Equal<CurrentValue<FSServiceOrder.customerID>>,
								      Or<INItemPartNumber.bAccountID, Equal<CurrentValue<FSSchedule.customerID>>,
									  Or<INItemPartNumber.alternateType, Equal<INAlternateType.vPN>>>>>,
						And<Where<INItemPartNumber.subItemID, Equal<INSiteStatus.subItemID>,
								   Or<INSiteStatus.subItemID, IsNull>>>>>>,
		LeftJoin<INItemClass,
						On<InventoryItem.FK.ItemClass>,
		LeftJoin<INPriceClass,
						On<INPriceClass.priceClassID, Equal<InventoryItem.priceClassID>>,
		LeftJoin<BAccountR,
						On<BAccountR.bAccountID, Equal<InventoryItem.preferredVendorID>>,
		LeftJoin<INItemCustSalesStats,
				  On<CurrentValue<FSSiteStatusFilter.mode>, Equal<SOAddItemMode.byCustomer>,
							And<INItemCustSalesStats.inventoryID, Equal<InventoryItem.inventoryID>,
							And<INItemCustSalesStats.subItemID, Equal<INSiteStatus.subItemID>,
							And<INItemCustSalesStats.siteID, Equal<INSiteStatus.siteID>,
							And2<
								Where<INItemCustSalesStats.bAccountID, Equal<CurrentValue<FSServiceOrder.customerID>>,
									Or<INItemCustSalesStats.bAccountID, Equal<CurrentValue<FSSchedule.customerID>>>>,
							And<INItemCustSalesStats.lastDate, GreaterEqual<CurrentValue<FSSiteStatusFilter.historyDate>>>>>>>>,
	LeftJoin<INUnit,
					On<INUnit.inventoryID, Equal<InventoryItem.inventoryID>,
					And<INUnit.unitType, Equal<INUnitType.inventoryItem>,
					And<INUnit.fromUnit, Equal<InventoryItem.salesUnit>,
					And<INUnit.toUnit, Equal<InventoryItem.baseUnit>>>>>
							>>>>>>>>>>,
		Where2<
			Where<CurrentValue<FSServiceOrder.customerID>, IsNotNull,
				Or<CurrentValue<FSSchedule.customerID>, IsNotNull,
				Or<CurrentValue<FSSrvOrdType.behavior>, Equal<FSSrvOrdType.behavior.InternalAppointment>>>>,
		  And2<CurrentMatch<InventoryItem, AccessInfo.userName>,
			And2<Where<INSiteStatus.siteID, IsNull, Or<INSite.branchID, IsNotNull, And2<CurrentMatch<INSite, AccessInfo.userName>,
				And<Where2<FeatureInstalled<FeaturesSet.interBranch>,
					Or2<SameOrganizationBranch<INSite.branchID, Current<FSServiceOrder.branchID>>,
						Or<SameOrganizationBranch<INSite.branchID, Current<FSSchedule.branchID>>>>>>>>>,
			And2<Where<INSiteStatus.subItemID, IsNull, Or<CurrentMatch<INSubItem, AccessInfo.userName>>>,
			And2<Where<CurrentValue<FSSiteStatusFilter.onlyAvailable>, Equal<boolFalse>,
				   Or<INSiteStatus.qtyAvail, Greater<CS.decimal0>>>,
		  And2<Where<CurrentValue<FSSiteStatusFilter.mode>, Equal<SOAddItemMode.bySite>,
					Or<INItemCustSalesStats.lastQty, Greater<decimal0>>>,
	     And2<Where<CurrentValue<FSSiteStatusFilter.lineType>, Equal<FSLineType.All>,
					Or2<
						Where<CurrentValue<FSSiteStatusFilter.lineType>, Equal<FSLineType.Service>,
							And<InventoryItem.stkItem, Equal<False>,
							And<InventoryItem.itemType, Equal<INItemTypes.serviceItem>>>>,
						Or2<
							Where<CurrentValue<FSSiteStatusFilter.lineType>, Equal<FSLineType.NonStockItem>,
							And<InventoryItem.stkItem, Equal<False>,
							And<InventoryItem.itemType, NotEqual<INItemTypes.serviceItem>>>>,
						Or<Where<CurrentValue<FSSiteStatusFilter.lineType>, Equal<FSLineType.Inventory_Item>,
							And<InventoryItem.stkItem, Equal<True>>>>>>>,
		 And2<Where2<
				Where<CurrentValue<FSSiteStatusFilter.includeIN>, Equal<True>>,
				Or<InventoryItem.stkItem, Equal<False>>>,
			And<InventoryItem.isTemplate, Equal<False>,
			And<InventoryItem.itemStatus, NotIn3<
				 InventoryItemStatus.unknown,
				 InventoryItemStatus.inactive,
				 InventoryItemStatus.markedForDeletion,
				 InventoryItemStatus.noSales>>>>>>>>>>>>), Persistent = false)]
	public partial class FSSiteStatusSelected : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(BqlField = typeof(InventoryItem.inventoryID), IsKey = true)]
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

		#region InventoryCD
		public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }
		protected string _InventoryCD;
		[PXDefault()]
		[InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
		public virtual String InventoryCD
		{
			get
			{
				return this._InventoryCD;
			}
			set
			{
				this._InventoryCD = value;
			}
		}
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected string _Descr;
		[PXDBLocalizableString(60, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
		[PXUIField(DisplayName = "Item Class ID", Visible = false)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(INItemClass.itemClassID), typeof(INItemClass.itemClassCD), ValidComboRequired = true)]
		public virtual int? ItemClassID
		{
			get
			{
				return this._ItemClassID;
			}
			set
			{
				this._ItemClassID = value;
			}
		}
		#endregion

		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected string _ItemClassCD;
		[PXDBString(30, IsUnicode = true, BqlField = typeof(INItemClass.itemClassCD))]
		public virtual string ItemClassCD
		{
			get
			{
				return this._ItemClassCD;
			}
			set
			{
				this._ItemClassCD = value;
			}
		}
		#endregion

		#region ItemClassDescription
		public abstract class itemClassDescription : PX.Data.BQL.BqlString.Field<itemClassDescription> { }
		protected String _ItemClassDescription;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INItemClass.descr), IsProjection = true)]
		[PXUIField(DisplayName = "Item Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String ItemClassDescription
		{
			get
			{
				return this._ItemClassDescription;
			}
			set
			{
				this._ItemClassDescription = value;
			}
		}
		#endregion

		#region ItemType
		public abstract class itemType : PX.Data.BQL.BqlString.Field<itemType> { }
		protected String _ItemType;

		/// <summary>
		/// The type of the Inventory Item.
		/// </summary>
		/// <value>
		/// Possible values are:
		/// <c>"F"</c> - Finished Good (Stock Items only),
		/// <c>"M"</c> - Component Part (Stock Items only),
		/// <c>"A"</c> - Subassembly (Stock Items only),
		/// <c>"N"</c> - Non-Stock Item (a general type of Non-Stock Item),
		/// <c>"L"</c> - Labor (Non-Stock Items only),
		/// <c>"S"</c> - Service (Non-Stock Items only),
		/// <c>"C"</c> - Charge (Non-Stock Items only),
		/// <c>"E"</c> - Expense (Non-Stock Items only).
		/// Defaults to the <see cref="INItemClass.ItemType">Type</see> associated with the <see cref="ItemClassID">Item Class</see>
		/// of the item if it's specified, or to Finished Good (<c>"F"</c>) otherwise.
		/// </value>
		[PXDBString(1, IsFixed = true, BqlField = typeof(InventoryItem.itemType))]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		[INItemTypes.List()]
		public virtual String ItemType
		{
			get
			{
				return this._ItemType;
			}
			set
			{
				this._ItemType = value;
			}
		}
		#endregion

		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }

		protected string _PriceClassID;
		[PXDBString(10, IsUnicode = true, BqlField = typeof(InventoryItem.priceClassID))]
		[PXUIField(DisplayName = "Price Class ID", Visible = false)]
		public virtual String PriceClassID
		{
			get
			{
				return this._PriceClassID;
			}
			set
			{
				this._PriceClassID = value;
			}
		}
		#endregion

		#region PriceClassDescription
		public abstract class priceClassDescription : PX.Data.BQL.BqlString.Field<priceClassDescription> { }
		protected String _PriceClassDescription;
		[PXDBString(Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class Description", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PriceClassDescription
		{
			get
			{
				return this._PriceClassDescription;
			}
			set
			{
				this._PriceClassDescription = value;
			}
		}
		#endregion

		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }

		protected Int32? _PreferredVendorID;
		[AP.VendorNonEmployeeActive(DisplayName = "Preferred Vendor ID", Required = false, DescriptionField = typeof(BAccountR.acctName), BqlField = typeof(InventoryItem.preferredVendorID), Visible = false, ErrorHandling = PXErrorHandling.Always)]
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

		#region PreferredVendorDescription
		public abstract class preferredVendorDescription : PX.Data.BQL.BqlString.Field<preferredVendorDescription> { }
		protected String _PreferredVendorDescription;
		[PXDBString(250, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Preferred Vendor Name", Visible = false, ErrorHandling = PXErrorHandling.Always)]
		public virtual String PreferredVendorDescription
		{
			get
			{
				return this._PreferredVendorDescription;
			}
			set
			{
				this._PreferredVendorDescription = value;
			}
		}
		#endregion

		#region BarCode
		public abstract class barCode : PX.Data.BQL.BqlString.Field<barCode> { }
		protected String _BarCode;
		[PXDBString(255, BqlField = typeof(INItemXRef.alternateID), IsUnicode = true)]
		[PXUIField(DisplayName = "Barcode", Visible = false)]
		public virtual String BarCode
		{
			get
			{
				return this._BarCode;
			}
			set
			{
				this._BarCode = value;
			}
		}
		#endregion

		#region AlternateID
		public abstract class alternateID : PX.Data.BQL.BqlString.Field<alternateID> { }
		protected String _AlternateID;
		[PXDBString(225, IsUnicode = true, InputMask = "", BqlField = typeof(INItemPartNumber.alternateID))]
		[PXUIField(DisplayName = "Alternate ID", Visible = false)]
		[PXExtraKey]
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

		#region AlternateType
		public abstract class alternateType : PX.Data.BQL.BqlString.Field<alternateType> { }
		protected String _AlternateType;
		[PXDBString(4, BqlField = typeof(INItemPartNumber.alternateType))]
		[INAlternateType.List()]
		[PXDefault(INAlternateType.Global)]
		[PXUIField(DisplayName = "Alternate Type", Visible = false)]
		public virtual String AlternateType
		{
			get
			{
				return this._AlternateType;
			}
			set
			{
				this._AlternateType = value;
			}
		}
		#endregion

		#region Descr
		public abstract class alternateDescr : PX.Data.BQL.BqlString.Field<alternateDescr> { }
		protected String _AlternateDescr;
		[PXDBString(60, IsUnicode = true, BqlField = typeof(INItemPartNumber.descr))]
		[PXUIField(DisplayName = "Alternate Description", Visible = false)]
		public virtual String AlternateDescr
		{
			get
			{
				return this._AlternateDescr;
			}
			set
			{
				this._AlternateDescr = value;
			}
		}
		#endregion

		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected int? _SiteID;
		[PXUIField(DisplayName = "Warehouse")]
		[SiteAttribute(BqlField = typeof(INSiteStatus.siteID), IsKey = true)]
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

		#region SiteCD
		public abstract class siteCD : PX.Data.BQL.BqlString.Field<siteCD> { }
		protected String _SiteCD;
		[PXDBString(IsUnicode = true, BqlField = typeof(INSite.siteCD))]
		[PXDimension(SiteAttribute.DimensionName)]
		public virtual String SiteCD
		{
			get
			{
				return this._SiteCD;
			}
			set
			{
				this._SiteCD = value;
			}
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected int? _SubItemID;
		[SubItem(typeof(FSSiteStatusSelected.inventoryID), BqlField = typeof(INSubItem.subItemID), IsKey = true)]
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

		#region SubItemCD
		public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
		protected String _SubItemCD;
		[PXDBString(BqlField = typeof(INSubItem.subItemCD), IsUnicode = true)]
		[PXDimension(SubItemAttribute.DimensionName)]
		public virtual String SubItemCD
		{
			get
			{
				return this._SubItemCD;
			}
			set
			{
				this._SubItemCD = value;
			}
		}
		#endregion

		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }

		protected string _BaseUnit;
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.Visible, BqlField = typeof(InventoryItem.baseUnit))]
		public virtual String BaseUnit
		{
			get
			{
				return this._BaseUnit;
			}
			set
			{
				this._BaseUnit = value;
			}
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXString(5, IsUnicode = true, InputMask = ">LLLLL")]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CuryID
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

		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		protected Int64? _CuryInfoID;
		[PXLong()]
		[CurrencyInfo()]
		public virtual Int64? CuryInfoID
		{
			get
			{
				return this._CuryInfoID;
			}
			set
			{
				this._CuryInfoID = value;
			}
		}
		#endregion

		#region SalesUnit
		public abstract class salesUnit : PX.Data.BQL.BqlString.Field<salesUnit> { }
		protected string _SalesUnit;
		[INUnit(typeof(FSSiteStatusSelected.inventoryID), DisplayName = "Sales Unit", BqlField = typeof(InventoryItem.salesUnit))]
		public virtual String SalesUnit
		{
			get
			{
				return this._SalesUnit;
			}
			set
			{
				this._SalesUnit = value;
			}
		}
		#endregion

		#region BillingRule
		public abstract class billingRule : ListField_BillingRule
		{
		}

		[PXDBString(4, IsFixed = true, BqlField = typeof(FSxService.billingRule))]
		[billingRule.List]
		[PXUIField(DisplayName = "Billing Rule")]
		public virtual string BillingRule { get; set; }
		#endregion
		#region DfltDuration
		public abstract class dfltDuration : PX.Data.BQL.BqlInt.Field<dfltDuration> { }

		[PXUIField(DisplayName = "Estimated Duration")]
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes, BqlField = typeof(FSxService.estimatedDuration))]
		public virtual int? DfltDuration { get; set; }
		#endregion

		#region QtySelected
		public abstract class qtySelected : PX.Data.BQL.BqlDecimal.Field<qtySelected> { }
		protected Decimal? _QtySelected;
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Selected")]
		[PXUIEnabled(typeof(Where<billingRule, NotEqual<ListField_BillingRule.Time>>))]
		public virtual Decimal? QtySelected
		{
			get
			{
				return this._QtySelected ?? 0m;
			}
			set
			{
				if (value != null && value != 0m)
					this._Selected = true;
				this._QtySelected = value;
			}
		}
		#endregion
		#region DurationSelected
		public abstract class durationSelected : PX.Data.BQL.BqlInt.Field<durationSelected> { }
		protected int? _DurationSelected;
		[PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
		[PXUIField(DisplayName = "Duration Selected", Enabled = true)]
		[PXUIEnabled(typeof(Where<billingRule, Equal<ListField_BillingRule.Time>>))]
		public virtual int? DurationSelected
		{
			get
			{
				return this._DurationSelected ?? 0;
			}
			set
			{
				if (value != null && value != 0)
					this._Selected = true;
				this._DurationSelected = value;
			}
		}
		#endregion

		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyOnHand))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
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

		#region QtyAvail
		public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }
		protected Decimal? _QtyAvail;
		[PXDBQuantity(BqlField = typeof(INSiteStatus.qtyAvail))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion

		#region QtyLast
		public abstract class qtyLast : PX.Data.BQL.BqlDecimal.Field<qtyLast> { }
		protected Decimal? _QtyLast;
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.lastQty))]
		public virtual Decimal? QtyLast
		{
			get
			{
				return this._QtyLast;
			}
			set
			{
				this._QtyLast = value;
			}
		}
		#endregion

		#region BaseUnitPrice
		public abstract class baseUnitPrice : PX.Data.BQL.BqlDecimal.Field<baseUnitPrice> { }
		protected Decimal? _BaseUnitPrice;
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.lastUnitPrice))]
		public virtual Decimal? BaseUnitPrice
		{
			get
			{
				return this._BaseUnitPrice;
			}
			set
			{
				this._BaseUnitPrice = value;
			}
		}
		#endregion

		#region CuryUnitPrice
		public abstract class curyUnitPrice : PX.Data.BQL.BqlDecimal.Field<curyUnitPrice> { }
		protected Decimal? _CuryUnitPrice;
		[PXUnitPriceCuryConv(typeof(FSSiteStatusSelected.curyInfoID), typeof(FSSiteStatusSelected.baseUnitPrice))]
		[PXUIField(DisplayName = "Last Unit Price", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? CuryUnitPrice
		{
			get
			{
				return this._CuryUnitPrice;
			}
			set
			{
				this._CuryUnitPrice = value;
			}
		}
		#endregion

		#region QtyAvailSale
		public abstract class qtyAvailSale : PX.Data.BQL.BqlDecimal.Field<qtyAvailSale> { }
		protected Decimal? _QtyAvailSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyAvail, INUnit.unitRate>>,
			Div<INSiteStatus.qtyAvail, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. Available")]
		public virtual Decimal? QtyAvailSale
		{
			get
			{
				return this._QtyAvailSale;
			}
			set
			{
				this._QtyAvailSale = value;
			}
		}
		#endregion

		#region QtyOnHandSale
		public abstract class qtyOnHandSale : PX.Data.BQL.BqlDecimal.Field<qtyOnHandSale> { }
		protected Decimal? _QtyOnHandSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INSiteStatus.qtyOnHand, INUnit.unitRate>>,
			Div<INSiteStatus.qtyOnHand, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHandSale
		{
			get
			{
				return this._QtyOnHandSale;
			}
			set
			{
				this._QtyOnHandSale = value;
			}
		}
		#endregion

		#region QtyLastSale
		public abstract class qtyLastSale : PX.Data.BQL.BqlDecimal.Field<qtyLastSale> { }
		protected Decimal? _QtyLastSale;
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.lastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.lastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. Last Sales")]
		public virtual Decimal? QtyLastSale
		{
			get
			{
				return this._QtyLastSale;
			}
			set
			{
				this._QtyLastSale = value;
			}
		}
		#endregion

		#region LastSalesDate
		public abstract class lastSalesDate : PX.Data.BQL.BqlDateTime.Field<lastSalesDate> { }
		protected DateTime? _LastSalesDate;
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.lastDate))]
		[PXUIField(DisplayName = "Last Sales Date")]
		public virtual DateTime? LastSalesDate
		{
			get
			{
				return this._LastSalesDate;
			}
			set
			{
				this._LastSalesDate = value;
			}
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastBaseQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastBaseQty> { }
		[PXDBQuantity(BqlField = typeof(INItemCustSalesStats.dropShipLastQty))]
		public virtual Decimal? DropShipLastBaseQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastQty
		public abstract class dropShipLastQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastQty> { }
		[PXDBCalced(typeof(Switch<Case<Where<INUnit.unitMultDiv, Equal<MultDiv.divide>>,
			Mult<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>,
			Div<INItemCustSalesStats.dropShipLastQty, INUnit.unitRate>>), typeof(decimal))]
		[PXQuantity()]
		[PXUIField(DisplayName = "Qty. of Last Drop Ship")]
		public virtual Decimal? DropShipLastQty
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastUnitPrice
		public abstract class dropShipLastUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipLastUnitPrice> { }
		[PXDBPriceCost(true, BqlField = typeof(INItemCustSalesStats.dropShipLastUnitPrice))]
		public virtual Decimal? DropShipLastUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipCuryUnitPrice
		public abstract class dropShipCuryUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipCuryUnitPrice> { }
		[PXUnitPriceCuryConv(typeof(FSSiteStatusSelected.curyInfoID), typeof(FSSiteStatusSelected.dropShipLastUnitPrice))]
		[PXUIField(DisplayName = "Unit Price of Last Drop Ship", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? DropShipCuryUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region DropShipLastDate
		public abstract class dropShipLastDate : PX.Data.BQL.BqlDateTime.Field<dropShipLastDate> { }
		[PXDBDate(BqlField = typeof(INItemCustSalesStats.dropShipLastDate))]
		[PXUIField(DisplayName = "Date of Last Drop Ship")]
		public virtual DateTime? DropShipLastDate
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(BqlField = typeof(InventoryItem.noteID))]
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
	}
}

