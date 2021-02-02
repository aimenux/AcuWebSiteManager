using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.Common.Extensions;
using PX.Objects.TX;
using PX.TM;
using PX.Objects.EP;
using PX.Objects.DR;
using PX.Objects.CR;
using PX.Objects.IN.Matrix.Graphs;
using PX.Objects.IN.Matrix.Attributes;

namespace PX.Objects.IN
{
	/// <summary>
	/// Represents Stock Items (have value and stored in a warehouse) and
	/// Non-Stock Items (not kept in a warehouse and immediately available for purchase).
	/// Whether the item is Stock or Non-Stock is determined by the value of the <see cref="InventoryItem.StkItem"/> field.
	/// The records of this type are created and edited through the Stock Items (IN.20.25.00)
	/// (corresponds to the <see cref="InventoryItemMaint"/> graph) and
	/// the Non-Stock Items (IN.20.20.00) (corresponds to the <see cref="NonStockItemMaint"/> graph) screens.
	/// </summary>
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] {
					typeof(NonStockItemMaint),
					typeof(InventoryItemMaint),
					typeof(TemplateInventoryItemMaint)},
				new Type[] {
					typeof(Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.isTemplate, Equal<False>>>),
					typeof(Where<InventoryItem.stkItem, Equal<True>, And<InventoryItem.isTemplate, Equal<False>>>),
					typeof(Where<InventoryItem.isTemplate, Equal<True>>)
					})]
	[PXCacheName(Messages.InventoryItem, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class InventoryItem : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		#region Keys
		public class PK : PrimaryKeyOf<InventoryItem>.By<inventoryID>
		{
			public static InventoryItem Find(PXGraph graph, int? inventoryID) => FindBy(graph, inventoryID);
			public static InventoryItem FindDirty(PXGraph graph, int? inventoryID)
				=> (InventoryItem)PXSelect<InventoryItem, Where<inventoryID, Equal<Required<inventoryID>>>>.SelectWindowed(graph, 0, 1, inventoryID);
		}
		public class UK : PrimaryKeyOf<InventoryItem>.By<inventoryCD>
		{
			public static InventoryItem Find(PXGraph graph, string inventoryCD) => FindBy(graph, inventoryCD);
		}
		public static class FK
		{
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<InventoryItem>.By<taxCategoryID> { }
			public class SalesSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<salesSubID> { }
			public class InvtSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<invtSubID> { }
			public class COGSSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<cOGSSubID> { }
			public class StdCstRevSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<stdCstRevSubID> { }
			public class PPVSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<pPVSubID> { }
			public class DeferralSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<deferralSubID> { }
			public class LastSite : INSite.PK.ForeignKeyOf<InventoryItem>.By<lastSiteID> { }
			public class StdCstVarSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<stdCstVarSubID> { }
			public class POAccrualSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<pOAccrualSubID> { }
			public class ItemClass : INItemClass.PK.ForeignKeyOf<InventoryItem>.By<itemClassID> { }
			public class PostClass : INPostClass.PK.ForeignKeyOf<InventoryItem>.By<postClassID> { }
			public class DfltSite : INSite.PK.ForeignKeyOf<InventoryItem>.By<dfltSiteID> { }
			public class DfltShipLocation : INLocation.PK.ForeignKeyOf<InventoryItem>.By<dfltShipLocationID> { }
			public class DfltReceiptLocation : INLocation.PK.ForeignKeyOf<InventoryItem>.By<dfltReceiptLocationID> { }
			public class DefaultSubItem : INSubItem.PK.ForeignKeyOf<InventoryItem>.By<defaultSubItemID> { }
			public class PriceClass : INPriceClass.PK.ForeignKeyOf<InventoryItem>.By<priceClassID> { }
			public class ReasonCodeSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<reasonCodeSubID> { }
			public class PICycle : INPICycle.PK.ForeignKeyOf<InventoryItem>.By<cycleID> { }
			public class LotSerClass : INLotSerClass.PK.ForeignKeyOf<InventoryItem>.By<lotSerClassID> { }
			public class LCVarianceSub : Sub.PK.ForeignKeyOf<InventoryItem>.By<lCVarianceSubID> { }
			public class ABCCode : INABCCode.PK.ForeignKeyOf<InventoryItem>.By<aBCCodeID> { }
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected Boolean? _Selected;

		/// <summary>
		/// Indicates whether the record is selected for mass processing.
		/// </summary>
		[PXBool()]
		[PXUIField(DisplayName = "Selected")]
		[PXFormula(typeof(False))]
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

		/// <summary>
		/// Database identity.
		/// The unique identifier of the Inventory Item.
		/// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXReferentialIntegrityCheck]
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
		protected String _InventoryCD;

		/// <summary>
		/// Key field.
		/// The user-friendly unique identifier of the Inventory Item.
		/// The structure of the identifier is determined by the <i>INVENTORY</i> <see cref="CS.Dimension">Segmented Key</see>.
		/// </summary>
		[PXDefault()]
		[InventoryRaw(IsKey = true, DisplayName = "Inventory ID")]
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
		#region StkItem
		public abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }
		protected Boolean? _StkItem;

		/// <summary>
		/// When set to <c>true</c>, indicates that this item is a Stock Item.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Stock Item")]
		public virtual Boolean? StkItem
		{
			get
			{
				return this._StkItem;
			}
			set
			{
				this._StkItem = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;

		/// <summary>
		/// The description of the Inventory Item.
		/// </summary>
		[DBMatrixLocalizableDescription(Common.Constants.TranDescLength, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
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

		/// <summary>
		/// The identifier of the <see cref="INItemClass">Item Class</see>, to which the Inventory Item belongs.
		/// Item Classes provide default settings for items, which belong to them, and are used to group items.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INItemClass.ItemClassID"/> field.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr),
			CacheGlobal = true)]
		[PXDefault(typeof(Search2<INItemClass.itemClassID, InnerJoin<INSetup,
			On<Current<InventoryItem.stkItem>, Equal<boolFalse>,
				And<INSetup.dfltNonStkItemClassID, Equal<INItemClass.itemClassID>,
				Or<Current<InventoryItem.stkItem>, Equal<boolTrue>,
					And<INSetup.dfltStkItemClassID, Equal<INItemClass.itemClassID>>>>>>>))]
		[PXUIRequired(typeof(INItemClass.stkItem))]
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
		#region ParentItemClassID
		public abstract class parentItemClassID : PX.Data.BQL.BqlInt.Field<parentItemClassID> { }

		protected int? _ParentItemClassID;
		/// <summary>
		/// The field is used to populate standard settings of <see cref="InventoryItem">Inventory Item</see> from
		/// <see cref="INItemClass">Item Class</see> when it's created On-The-Fly and not yet persisted.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INItemClass.ItemClassID"/> field.
		/// </value>
		[PXInt]
		[PXDefault(typeof(Search2<INItemClass.itemClassID, InnerJoin<INSetup,
			On<Current<InventoryItem.stkItem>, Equal<boolFalse>,
				And<INSetup.dfltNonStkItemClassID, Equal<INItemClass.itemClassID>,
				Or<Current<InventoryItem.stkItem>, Equal<boolTrue>,
					And<INSetup.dfltStkItemClassID, Equal<INItemClass.itemClassID>>>>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(InventoryItem.itemClassID), typeof(int), Persistent = true)]
		public virtual int? ParentItemClassID
		{
			get { return _ParentItemClassID; }
			set { _ParentItemClassID = value; }
		}
		#endregion
		#region ItemStatus
		public abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus> { }
		protected String _ItemStatus;

		/// <summary>
		/// The status of the Inventory Item.
		/// </summary>
		/// <value>
		/// Possible values are:
		/// <c>"AC"</c> - Active (can be used in inventory operations, such as issues and receipts),
		/// <c>"NS"</c> - No Sales (cannot be sold),
		/// <c>"NP"</c> - No Purchases (cannot be purchased),
		/// <c>"NR"</c> - No Request (cannot be used on requisition requests),
		/// <c>"IN"</c> - Inactive,
		/// <c>"DE"</c> - Marked for Deletion.
		/// Defaults to Active (<c>"AC"</c>).
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault("AC")]
		[PXUIField(DisplayName = "Item Status", Visibility = PXUIVisibility.SelectorVisible)]
		[InventoryItemStatus.List]
		public virtual String ItemStatus
		{
			get
			{
				return this._ItemStatus;
			}
			set
			{
				this._ItemStatus = value;
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
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INItemTypes.FinishedGood, typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			SourceField = typeof(INItemClass.itemType), CacheGlobal = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;

		/// <summary>
		/// The method used for inventory valuation of the item (Stock Items only).
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"T"</c> - Standard,
		/// <c>"A"</c> - Average,
		/// <c>"F"</c> - FIFO,
		/// <c>"S"</c> - Specific.
		/// Defaults to the <see cref="INItemClass.ValMethod">Valuation Method</see> associated with the <see cref="ItemClassID">Item Class</see>
		/// of the item if it's specified, or to Standard (<c>"T"</c>) otherwise.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Standard, typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.valMethod), CacheGlobal = true)]
		[PXUIField(DisplayName = "Valuation Method")]
		[INValMethod.List()]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }

		/// <summary>
		/// Identifier of the <see cref="TaxCategory"/> associated with the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.TaxCategoryID">Tax Category</see> associated with the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="TaxCategory.TaxCategoryID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.taxCategoryID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get;
			set;
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		/// <summary>
		/// The tax calculation mode, which defines which amounts (tax-inclusive or tax-exclusive) 
		/// should be entered in the detail lines of a document. 
		/// This field is displayed only if the <see cref="FeaturesSet.NetGrossEntryMode"/> field is set to <c>true</c>.
		/// </summary>
		/// <value>
		/// The field can have one of the following values:
		/// <c>"T"</c> (Tax Settings): The tax amount for the document is calculated according to the settings of the applicable tax or taxes.
		/// <c>"G"</c> (Gross): The amount in the document detail line includes a tax or taxes.
		/// <c>"N"</c> (Net): The amount in the document detail line does not include taxes.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<Case<Where<InventoryItem.itemClassID, IsNotNull>,
							Selector<InventoryItem.itemClassID, INItemClass.taxCalcMode>>,
							TaxCalculationMode.taxSetting>))]
		[TaxCalculationMode.List]
		[PXDefault(TaxCalculationMode.TaxSetting)]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region WeightItem
		/// <summary>
		/// When set to <c>true</c>, indicates that this item is a Weight Item.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIVisible(typeof(stkItem))]
		[PXUIEnabled(typeof(stkItem.IsEqual<True>.And<Data.BQL.Use<Selector<lotSerClassID, INLotSerClass.lotSerTrack>>.AsString.IsNotEqual<INLotSerTrack.serialNumbered>>))]
		[PXFormula(typeof(False.When<Data.BQL.Use<Selector<lotSerClassID, INLotSerClass.lotSerTrack>>.AsString.IsEqual<INLotSerTrack.serialNumbered>>.Else<weightItem>))]
		[PXUIField(DisplayName = "Weight Item")]
		public virtual Boolean? WeightItem { get; set; }
		public abstract class weightItem : PX.Data.BQL.BqlBool.Field<weightItem> { }
		#endregion
		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit>
		{
			public class PreventEditIfExists<TSelect> : PreventEditOf<baseUnit>.On<InventoryItemMaintBase>.IfExists<TSelect>
				where TSelect : BqlCommand, new()
			{
				protected override String CreateEditPreventingReason(GetEditPreventingReasonArgs arg, Object firstPreventingEntity, String fieldName, String currentTableName, String foreignTableName)
				{
					PXCache cache = Base.Caches[firstPreventingEntity.GetType()];
					return PXMessages.Localize(Messages.BaseUnitCouldNotBeChanged) + Environment.NewLine + cache.GetRowDescription(firstPreventingEntity);
				}
			}
		}
		protected String _BaseUnit;

		/// <summary>
		/// The <see cref="INUnit">Unit of Measure</see> used as the base unit for the Inventory Item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.BaseUnit">Base Unit</see> associated with the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="INUnit.FromUnit"/> field.
		/// </value>
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.baseUnit), CacheGlobal = true)]
		[INSyncUoms(typeof(salesUnit), typeof(purchaseUnit))]
		[INUnit(DisplayName = "Base Unit", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region SalesUnit
		public abstract class salesUnit : PX.Data.BQL.BqlString.Field<salesUnit> { }
		protected String _SalesUnit;

		/// <summary>
		/// The <see cref="INUnit">Unit of Measure</see> used as the sales unit for the Inventory Item.
		/// This field can be changed only if the <see cref="FeaturesSet.MultipleUnitMeasure">Multiple Units of Measure feature</see> is enabled.
		/// Otherwise, the sales unit is assumed to be the same as the <see cref="BaseUnit">Base Unit</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.SalesUnit">Sales Unit</see> associated with the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="INUnit.FromUnit"/> field.
		/// </value>
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.salesUnit), CacheGlobal = true)]
		[INUnit(typeof(InventoryItem.inventoryID), DisplayName = "Sales Unit", Visibility = PXUIVisibility.SelectorVisible, DirtyRead = true)]
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
		#region PurchaseUnit
		public abstract class purchaseUnit : PX.Data.BQL.BqlString.Field<purchaseUnit> { }
		protected String _PurchaseUnit;

		/// <summary>
		/// The <see cref="INUnit">Unit of Measure</see> used as the purchase unit for the Inventory Item.
		/// This field can be changed only if the <see cref="FeaturesSet.MultipleUnitMeasure">Multiple Units of Measure feature</see> is enabled.
		/// Otherwise, the purchase unit is assumed to be the same as the <see cref="BaseUnit">Base Unit</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.PurchaseUnit">Purchase Unit</see> associated with the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="INUnit.FromUnit"/> field.
		/// </value>
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.purchaseUnit), CacheGlobal = true)]
		[INUnit(typeof(InventoryItem.inventoryID), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.SelectorVisible, DirtyRead = true)]
		public virtual String PurchaseUnit
		{
			get
			{
				return this._PurchaseUnit;
			}
			set
			{
				this._PurchaseUnit = value;
			}
		}
		#endregion
		#region DecimalBaseUnit
		public abstract class decimalBaseUnit : PX.Data.BQL.BqlBool.Field<decimalBaseUnit> { }
		protected bool? _DecimalBaseUnit;

		/// <summary>
		/// When set to <c>false</c>, indicates that the system will prevent enter of non-integer values into the quantity field for choosed <see cref="BaseUnit">Base Unit</see> value.
		/// <value>
		/// Defaults to <c>true</c></value>
		/// </summary>
		[PXDBBool()]
		[INSyncUoms(typeof(decimalSalesUnit), typeof(decimalPurchaseUnit))]
		[PXDefault(true, typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.decimalBaseUnit), CacheGlobal = true)]
		[PXUIField(DisplayName = "Divisible Unit", Visibility = PXUIVisibility.Visible)]
		public virtual bool? DecimalBaseUnit
		{
			get { return this._DecimalBaseUnit; }
			set { _DecimalBaseUnit = value; }
		}
		#endregion
		#region DecimalSalesUnit
		public abstract class decimalSalesUnit : PX.Data.BQL.BqlBool.Field<decimalSalesUnit> { }
		protected bool? _DecimalSalesUnit;

		/// <summary>
		/// When set to <c>false</c>, indicates that the system will prevent enter of non-integer values into the quantity field for choosed <see cref="SalesUnit">Sales Unit</see> value.
		/// <value>
		/// Defaults to <c>true</c></value>
		/// </summary>
		[PXDBBool()]
		[PXDefault(true, typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.decimalSalesUnit), CacheGlobal = true)]
		[PXUIField(DisplayName = "Divisible Unit", Visibility = PXUIVisibility.Visible)]
		public virtual bool? DecimalSalesUnit
		{
			get { return this._DecimalSalesUnit; }
			set { _DecimalSalesUnit = value; }
		}
		#endregion
		#region DecimalPurchaseUnit
		public abstract class decimalPurchaseUnit : PX.Data.BQL.BqlBool.Field<decimalPurchaseUnit> { }
		protected bool? _DecimalPurchaseUnit;

		/// <summary>
		/// When set to <c>false</c>, indicates that the system will prevent enter of non-integer values into the quantity field for choosed <see cref="PurchaseUnit">Purchase Unit</see> value.
		/// <value>
		/// Defaults to <c>true</c></value>
		/// </summary>
		[PXDBBool()]
		[PXDefault(true, typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.decimalPurchaseUnit), CacheGlobal = true)]
		[PXUIField(DisplayName = "Divisible Unit", Visibility = PXUIVisibility.Visible)]
		public virtual bool? DecimalPurchaseUnit
		{
			get { return this._DecimalPurchaseUnit; }
			set { _DecimalPurchaseUnit = value; }
		}
		#endregion
		#region Commisionable
		public abstract class commisionable : PX.Data.BQL.BqlBool.Field<commisionable> { }
		protected Boolean? _Commisionable;

		/// <summary>
		/// When set to <c>true</c>, indicates that the system must calculate commission on the sale of this item.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Subject to Commission", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Commisionable
		{
			get
			{
				return this._Commisionable;
			}
			set
			{
				this._Commisionable = value;
			}
		}
		#endregion
		#region ReasonCodeSubID
		public abstract class reasonCodeSubID : PX.Data.BQL.BqlInt.Field<reasonCodeSubID> { }
		protected Int32? _ReasonCodeSubID;

		/// <summary>
		/// The identifier of the <see cref="Sub">Suabaccount</see> defined by the <see cref="ReasonCode">Reason Code</see>, associated with this item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.ReasonCodeSubID">Reason Code Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXDefault(typeof(Search<INPostClass.reasonCodeSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(DisplayName = "Reason Code Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<InventoryItem.reasonCodeSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ReasonCodeSubID
		{
			get
			{
				return this._ReasonCodeSubID;
			}
			set
			{
				this._ReasonCodeSubID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;

		/// <summary>
		/// The income <see cref="Account"/> used to record sales of the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.SalesAcctID">Sales Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.salesAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.salesAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? SalesAcctID
		{
			get
			{
				return this._SalesAcctID;
			}
			set
			{
				this._SalesAcctID = value;
			}
		}
		#endregion
		#region SalesSubID
		public abstract class salesSubID : PX.Data.BQL.BqlInt.Field<salesSubID> { }
		protected Int32? _SalesSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record sales of the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.SalesSubID">Sales Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.salesSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.salesSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? SalesSubID
		{
			get
			{
				return this._SalesSubID;
			}
			set
			{
				this._SalesSubID = value;
			}
		}
		#endregion
		#region InvtAcctID
		public abstract class invtAcctID : PX.Data.BQL.BqlInt.Field<invtAcctID> { }
		protected Int32? _InvtAcctID;

		/// <summary>
		/// The asset <see cref="Account"/> used to keep the inventory balance, resulting from the transactions with this item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.InvtAcctID">Inventory Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Inventory Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.IN)]
		[PXDefault(typeof(Search<INPostClass.invtAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.invtAcctID>.IsRelatedTo<Account.accountID>))]
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

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to keep the inventory balance, resulting from the transactions with this item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.InvtSubID">Inventory Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.invtAcctID), DisplayName = "Inventory Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.invtSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.invtSubID>.IsRelatedTo<Sub.subID>))]
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
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		protected Int32? _COGSAcctID;

		/// <summary>
		/// The expense <see cref="Account"/> used to record the cost of goods sold for this item when a sales order for it is released.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.COGSAcctID">COGS Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "COGS Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.cOGSAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.cOGSAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? COGSAcctID
		{
			get
			{
				return this._COGSAcctID;
			}
			set
			{
				this._COGSAcctID = value;
			}
		}
		#endregion
		#region COGSSubID
		public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
		protected Int32? _COGSSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record the cost of goods sold for this item when a sales order for it is released.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.COGSSubID">COGS Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.cOGSSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.cOGSSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? COGSSubID
		{
			get
			{
				return this._COGSSubID;
			}
			set
			{
				this._COGSSubID = value;
			}
		}
		#endregion
		#region ExpenseAccrualAcctID
		public abstract class expenseAccrualAcctID : PX.Data.BQL.BqlInt.Field<expenseAccrualAcctID> { }
		/// <summary>
		/// The asset <see cref="Account"/> used to keep the Expense Accrual Account, resulting from the transactions with this item.
		/// Applicable only for Non-Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.InvtAcctID">Inventory Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXInt]
		public virtual Int32? ExpenseAccrualAcctID
		{
			get;
			set;
		}
		#endregion
		#region ExpenseAccrualSubID
		public abstract class expenseAccrualSubID : PX.Data.BQL.BqlInt.Field<expenseAccrualSubID> { }
		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to keep the Expense Accrual Account, resulting from the transactions with this item.
		/// Applicable only for Non-Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.InvtSubID">Inventory Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXInt]
		public virtual Int32? ExpenseAccrualSubID
		{
			get;
			set;
		}
		#endregion
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		/// <summary>
		/// The expense <see cref="Account"/> used to record the cost of goods sold for this item when a sales order for it is released.
		/// Applicable only for Non-Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.COGSAcctID">COGS Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[PXInt]
		public virtual Int32? ExpenseAcctID
		{
			get;
			set;
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record the cost of goods sold for this item when a sales order for it is released.
		/// Applicable only for Non-Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.COGSSubID">COGS Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[PXInt]
		public virtual Int32? ExpenseSubID
		{
			get;
			set;
		}
		#endregion
		#region StdCstRevAcctID
		public abstract class stdCstRevAcctID : PX.Data.BQL.BqlInt.Field<stdCstRevAcctID> { }
		protected Int32? _StdCstRevAcctID;

		/// <summary>
		/// The expense <see cref="Account"/> used to record the differences in inventory value of this item estimated
		/// by using the pending standard cost and the currently effective standard cost for the quantities on hand.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under Standard <see cref="ValMethod">Valuation Method</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.StdCstRevAcctID">Standard Cost Revaluation Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Standard Cost Revaluation Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.stdCstRevAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.stdCstRevAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? StdCstRevAcctID
		{
			get
			{
				return this._StdCstRevAcctID;
			}
			set
			{
				this._StdCstRevAcctID = value;
			}
		}
		#endregion
		#region StdCstRevSubID
		public abstract class stdCstRevSubID : PX.Data.BQL.BqlInt.Field<stdCstRevSubID> { }
		protected Int32? _StdCstRevSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record the differences in inventory value of this item estimated
		/// by using the pending standard cost and the currently effective standard cost for the quantities on hand.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under Standard <see cref="ValMethod">Valuation Method</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.StdCstRevSubID">Standard Cost Revaluation Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.stdCstRevAcctID), DisplayName = "Standard Cost Revaluation Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstRevSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.stdCstRevSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? StdCstRevSubID
		{
			get
			{
				return this._StdCstRevSubID;
			}
			set
			{
				this._StdCstRevSubID = value;
			}
		}
		#endregion
		#region StdCstVarAcctID
		public abstract class stdCstVarAcctID : PX.Data.BQL.BqlInt.Field<stdCstVarAcctID> { }
		protected Int32? _StdCstVarAcctID;

		/// <summary>
		/// The expense <see cref="Account"/> used to record the differences between the currently effective standard cost 
		/// and the cost on the inventory receipt of the item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under Standard <see cref="ValMethod">Valuation Method</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.StdCstVarAcctID">Standard Cost Variance Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Standard Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.stdCstVarAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.stdCstVarAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? StdCstVarAcctID
		{
			get
			{
				return this._StdCstVarAcctID;
			}
			set
			{
				this._StdCstVarAcctID = value;
			}
		}
		#endregion
		#region StdCstVarSubID
		public abstract class stdCstVarSubID : PX.Data.BQL.BqlInt.Field<stdCstVarSubID> { }
		protected Int32? _StdCstVarSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record the differences between the currently effective standard cost 
		/// and the cost on the inventory receipt of the item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under Standard <see cref="ValMethod">Valuation Method</see>.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.StdCstVarSubID">Standard Cost Variance Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.stdCstVarAcctID), DisplayName = "Standard Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.stdCstVarSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.stdCstVarSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? StdCstVarSubID
		{
			get
			{
				return this._StdCstVarSubID;
			}
			set
			{
				this._StdCstVarSubID = value;
			}
		}
		#endregion
		#region PPVAcctID
		public abstract class pPVAcctID : PX.Data.BQL.BqlInt.Field<pPVAcctID> { }
		protected Int32? _PPVAcctID;

		/// <summary>
		/// The expense <see cref="Account"/> used to record the differences between the extended price on the purchase receipt
		/// and the extended price on the Accounts Payable bill for this item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under any <see cref="ValMethod">Valuation Method</see> except Standard.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.PPVAcctID">Purchase Price Variance Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Purchase Price Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.pPVAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.pPVAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PPVAcctID
		{
			get
			{
				return this._PPVAcctID;
			}
			set
			{
				this._PPVAcctID = value;
			}
		}
		#endregion
		#region PPVSubID
		public abstract class pPVSubID : PX.Data.BQL.BqlInt.Field<pPVSubID> { }
		protected Int32? _PPVSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record the differences between the extended price on the purchase receipt
		/// and the extended price on the Accounts Payable bill for this item.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) under any <see cref="ValMethod">Valuation Method</see> except Standard.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.PPVSubID">Purchase Price Variance Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.pPVAcctID), DisplayName = "Purchase Price Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.pPVSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.pPVSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PPVSubID
		{
			get
			{
				return this._PPVSubID;
			}
			set
			{
				this._PPVSubID = value;
			}
		}
		#endregion
		#region POAccrualAcctID
		public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
		protected Int32? _POAccrualAcctID;

		/// <summary>
		/// The liability <see cref="Account"/> used to accrue amounts on purchase orders related to this item.
		/// Applicable for all Stock Items (see <see cref="StkItem"/>) and for Non-Stock Items, for which a receipt is required.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.POAccrualAcctID">PO Accrual Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
		[PXDefault(typeof(Search<INPostClass.pOAccrualAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.pOAccrualAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? POAccrualAcctID
		{
			get
			{
				return this._POAccrualAcctID;
			}
			set
			{
				this._POAccrualAcctID = value;
			}
		}
		#endregion
		#region POAccrualSubID
		public abstract class pOAccrualSubID : PX.Data.BQL.BqlInt.Field<pOAccrualSubID> { }
		protected Int32? _POAccrualSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to accrue amounts on purchase orders related to this item.
		/// Applicable for all Stock Items (see <see cref="StkItem"/>) and for Non-Stock Items, for which a receipt is required.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.POAccrualSubID">PO Accrual Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.pOAccrualAcctID), DisplayName = "PO Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.pOAccrualSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.pOAccrualSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? POAccrualSubID
		{
			get
			{
				return this._POAccrualSubID;
			}
			set
			{
				this._POAccrualSubID = value;
			}
		}
		#endregion
		#region LCVarianceAcctID
		public abstract class lCVarianceAcctID : PX.Data.BQL.BqlInt.Field<lCVarianceAcctID> { }
		protected Int32? _LCVarianceAcctID;

		/// <summary>
		/// The expense <see cref="Account"/> used to record differences between the landed cost amounts specified on purchase receipts
		/// and the amounts on inventory receipts.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.LCVarianceAcctID">Landed Cost Variance Account</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Account.AccountID"/> field.
		/// </value>
		[Account(DisplayName = "Landed Cost Variance Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<INPostClass.lCVarianceAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.lCVarianceAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? LCVarianceAcctID
		{
			get
			{
				return this._LCVarianceAcctID;
			}
			set
			{
				this._LCVarianceAcctID = value;
			}
		}
		#endregion
		#region LCVarianceSubID
		public abstract class lCVarianceSubID : PX.Data.BQL.BqlInt.Field<lCVarianceSubID> { }
		protected Int32? _LCVarianceSubID;

		/// <summary>
		/// The <see cref="Sub">Subaccount</see> used to record differences between the landed cost amounts specified on purchase receipts
		/// and the amounts on inventory receipts.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INPostClass.LCVarianceSubID">Landed Cost Variance Sub.</see> set on the <see cref="PostClassID">Posting Class</see> of the item.
		/// Corresponds to the <see cref="Sub.SubID"/> field.
		/// </value>
		[SubAccount(typeof(InventoryItem.lCVarianceAcctID), DisplayName = "Landed Cost Variance Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.lCVarianceSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.lCVarianceSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? LCVarianceSubID
		{
			get
			{
				return this._LCVarianceSubID;
			}
			set
			{
				this._LCVarianceSubID = value;
			}
		}
		#endregion
		#region DeferralAcctID
		public abstract class deferralAcctID : PX.Data.BQL.BqlInt.Field<deferralAcctID> { }

		[Account(DisplayName = "Deferral Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.DR)]
		[PXDefault(typeof(Search<INPostClass.deferralAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.deferralAcctID>.IsRelatedTo<Account.accountID>))]
		public int? DeferralAcctID { get; set; }
		#endregion
		#region DeferralSubID
		public abstract class deferralSubID : PX.Data.BQL.BqlInt.Field<deferralSubID> { }

		[SubAccount(typeof(InventoryItem.deferralAcctID), DisplayName = "Deferral Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<INPostClass.deferralSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<InventoryItem.deferralSubID>.IsRelatedTo<Sub.subID>))]
		public int? DeferralSubID { get; set; }
		#endregion
		#region LastSiteID
		public abstract class lastSiteID : PX.Data.BQL.BqlInt.Field<lastSiteID> { }
		protected Int32? _LastSiteID;

		/// <summary>
		/// Reserved for internal use.
		/// </summary>
		[PXDBInt()]
		public virtual Int32? LastSiteID
		{
			get
			{
				return this._LastSiteID;
			}
			set
			{
				this._LastSiteID = value;
			}
		}
		#endregion
		#region LastStdCost
		public abstract class lastStdCost : PX.Data.BQL.BqlDecimal.Field<lastStdCost> { }
		protected Decimal? _LastStdCost;

		/// <summary>
		/// The standard cost assigned to the item before the current standard cost was set.
		/// </summary>
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

		/// <summary>
		/// The standard cost to be assigned to the item when the costs are updated.
		/// </summary>
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

		/// <summary>
		/// The date when the <see cref="PendingStdCost">Pending Cost</see> becomes effective.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Pending Cost Date")]
		[PXFormula(typeof(Switch<Case<Where<InventoryItem.pendingStdCost, NotEqual<CS.decimal0>>, Current<AccessInfo.businessDate>>, InventoryItem.pendingStdCostDate>))]
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

		/// <summary>
		/// The current standard cost of the item.
		/// </summary>
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

		/// <summary>
		/// The date when the <see cref="StdCost">Current Cost</see> became effective.
		/// </summary>
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
		#region BasePrice
		public abstract class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		protected Decimal? _BasePrice;

		/// <summary>
		/// The price used as the default price, if there are no other prices defined for this item in any price list in the Accounts Receivable module.
		/// </summary>
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Default Price", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region BaseWeight
		public abstract class baseWeight : PX.Data.BQL.BqlDecimal.Field<baseWeight> { }
		protected Decimal? _BaseWeight;

		/// <summary>
		/// The weight of the <see cref="BaseUnit">Base Unit</see> of the item.
		/// </summary>
		/// <value>
		/// Given in the <see cref="CommonSetup.WeightUOM">default Weight Unit of the Inventory module</see>.
		/// </value>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseWeight
		{
			get
			{
				return this._BaseWeight;
			}
			set
			{
				this._BaseWeight = value;
			}
		}
		#endregion
		#region BaseVolume
		public abstract class baseVolume : PX.Data.BQL.BqlDecimal.Field<baseVolume> { }
		protected Decimal? _BaseVolume;

		/// <summary>
		/// The volume of the item.
		/// </summary>
		/// <value>
		/// Given in the <see cref="CommonSetup.VolumeUOM">default Volume Unit of the Inventory module</see>.
		/// </value>
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume")]
		public virtual Decimal? BaseVolume
		{
			get
			{
				return this._BaseVolume;
			}
			set
			{
				this._BaseVolume = value;
			}
		}
		#endregion
		#region BaseItemWeight
		public abstract class baseItemWeight : PX.Data.BQL.BqlDecimal.Field<baseItemWeight> { }
		protected Decimal? _BaseItemWeight;

		/// <summary>
		/// The weight of the <see cref="BaseUnit">Base Unit</see> of the item.
		/// </summary>
		/// <value>
		/// Given in the <see cref="WeightUOM">Weight Unit of this item</see>.
		/// </value>
		[PXDBQuantity(6, typeof(InventoryItem.weightUOM), typeof(InventoryItem.baseWeight), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Weight")]
		public virtual Decimal? BaseItemWeight
		{
			get
			{
				return this._BaseItemWeight;
			}
			set
			{
				this._BaseItemWeight = value;
			}
		}
		#endregion
		#region BaseItemVolume
		public abstract class baseItemVolume : PX.Data.BQL.BqlDecimal.Field<baseItemVolume> { }
		protected Decimal? _BaseItemVolume;

		/// <summary>
		/// The volume of the <see cref="BaseUnit">Base Unit</see> of the item.
		/// </summary>
		/// <value>
		/// Given in the <see cref="VolumeUOM">Volume Unit of this item</see>.
		/// </value>
		[PXDBQuantity(6, typeof(InventoryItem.volumeUOM), typeof(InventoryItem.baseVolume), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Volume")]
		public virtual Decimal? BaseItemVolume
		{
			get
			{
				return this._BaseItemVolume;
			}
			set
			{
				this._BaseItemVolume = value;
			}
		}
		#endregion
		#region WeightUOM
		public abstract class weightUOM : PX.Data.BQL.BqlString.Field<weightUOM> { }
		protected String _WeightUOM;

		/// <summary>
		/// The <see cref="INUnit">Unit of Measure</see> used for the <see cref="BaseItemWeight">Weight</see> of the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INUnit.FromUnit"/> field.
		/// </value>
		[INUnit(null, typeof(CommonSetup.weightUOM), DisplayName = "Weight UOM")]
		public virtual String WeightUOM
		{
			get
			{
				return this._WeightUOM;
			}
			set
			{
				this._WeightUOM = value;
			}
		}
		#endregion
		#region VolumeUOM
		public abstract class volumeUOM : PX.Data.BQL.BqlString.Field<volumeUOM> { }
		protected String _VolumeUOM;

		/// <summary>
		/// The <see cref="INUnit">Unit of Measure</see> used for the <see cref="BaseItemVolume">Volume</see> of the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INUnit.FromUnit"/> field.
		/// </value>
		[INUnit(null, typeof(CommonSetup.volumeUOM), DisplayName = "Volume UOM")]
		public virtual String VolumeUOM
		{
			get
			{
				return this._VolumeUOM;
			}
			set
			{
				this._VolumeUOM = value;
			}
		}
		#endregion
		#region PackSeparately
		public abstract class packSeparately : PX.Data.BQL.BqlBool.Field<packSeparately> { }
		protected Boolean? _PackSeparately;

		/// <summary>
		/// When set to <c>true</c>, indicates that the item must be packaged separately from other items.
		/// This field is automatically set to <c>true</c> if <i>By Quantity</i> is selected as the <see cref="PackageOption">PackageOption</see>.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Pack Separately", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? PackSeparately
		{
			get
			{
				return this._PackSeparately;
			}
			set
			{
				this._PackSeparately = value;
			}
		}
		#endregion
		#region PackageOption
		public abstract class packageOption : PX.Data.BQL.BqlString.Field<packageOption> { }
		protected String _PackageOption;

		/// <summary>
		/// The option that governs the system in the process of determining the optimal set of boxes for the item on each sales order.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"N"</c> - Manual,
		/// <c>"W"</c> - By Weight (the system will take into account the <see cref="INItemBoxEx.MaxWeight">Max. Weight</see> allowed for each box specififed for the item),
		/// <c>"Q"</c> - By Quantity (the system will take into account the <see cref="INItemBoxEx.MaxQty">Max. Quantity</see> allowed for each box specififed for the item.
		///	With this option items of this kind are always packages separately from other items),
		/// <c>"V"</c> - By Weight and Volume (the system will take into account the <see cref="INItemBoxEx.MaxWeight">Max. Weight</see> and
		/// <see cref="INItemBoxEx.MaxVolume">Max. Volume</see> allowed for each box specififed for the item).
		/// Defaults to Manual (<c>"M"</c>).
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INPackageOption.Manual)]
		[PXUIField(DisplayName = "Packaging Option")]
		[INPackageOption.List]
		public virtual String PackageOption
		{
			get
			{
				return this._PackageOption;
			}
			set
			{
				this._PackageOption = value;
			}
		}
		#endregion
		#region PreferredVendorID
		public abstract class preferredVendorID : PX.Data.BQL.BqlInt.Field<preferredVendorID> { }
		protected Int32? _PreferredVendorID;

		/// <summary>
		/// Preferred (default) <see cref="AP.Vendor">Vendor</see> for purchases of this item. 
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="BAccount.BAccountID"/> field.
		/// </value>
		[AP.VendorNonEmployeeActive(DisplayName = "Preferred Vendor", Required = false, DescriptionField = typeof(AP.Vendor.acctName))]
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

		/// <summary>
		/// The <see cref="Location"/> of the <see cref="PreferredVendorID">Preferred (default) Vendor</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Location.LocationID"/> field.
		/// </value>
		[LocationID(typeof(Where<Location.bAccountID, Equal<Current<InventoryItem.preferredVendorID>>>),
			DescriptionField = typeof(Location.descr), DisplayName = "Preferred Location")]
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
		#region DefaultSubItemID
		public abstract class defaultSubItemID : PX.Data.BQL.BqlInt.Field<defaultSubItemID> { }
		protected Int32? _DefaultSubItemID;

		/// <summary>
		/// The default <see cref="INSubItem">Subitem</see> for this item, which is used when there are no subitems
		/// or when specifying subitems is not important.
		/// This field is relevant only if the <see cref="FeaturesSet.SubItem">Inventory Subitems</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INSubItem.SubItemID"/> field.
		/// </value>
		[IN.SubItem(typeof(InventoryItem.inventoryID), DisplayName = "Default Subitem")]
		public virtual Int32? DefaultSubItemID
		{
			get
			{
				return this._DefaultSubItemID;
			}
			set
			{
				this._DefaultSubItemID = value;
			}
		}
		#endregion
		#region DefaultSubItemOnEntry
		public abstract class defaultSubItemOnEntry : PX.Data.BQL.BqlBool.Field<defaultSubItemOnEntry> { }
		protected Boolean? _DefaultSubItemOnEntry;

		/// <summary>
		/// When set to <c>true</c>, indicates that the system must set the <see cref="DefaultSubItemID">Default Subitem</see>
		/// for the lines involving this item by default on data entry forms.
		/// This field is relevant only if the <see cref="FeaturesSet.SubItem">Inventory Subitems</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use On Entry")]
		public virtual Boolean? DefaultSubItemOnEntry
		{
			get
			{
				return this._DefaultSubItemOnEntry;
			}
			set
			{
				this._DefaultSubItemOnEntry = value;
			}
		}
		#endregion
		#region DfltSiteID
		public abstract class dfltSiteID : PX.Data.BQL.BqlInt.Field<dfltSiteID> { }
		protected Int32? _DfltSiteID;

		/// <summary>
		/// The default <see cref="INSite">Warehouse</see> used to store the items of this kind.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) and when the <see cref="FeaturesSet.Warehouse">Warehouses</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INSite.SiteID"/> field.
		/// Defaults to the <see cref="INItemClass.DfltSiteID">Default Warehouse</see> specified for the <see cref="ItemClassID">Class of the item</see>.
		/// </value>
		[IN.Site(DisplayName = "Default Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.dfltSiteID),
			PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
		[PXForeignReference(typeof(FK.DfltSite))]
		public virtual Int32? DfltSiteID
		{
			get
			{
				return this._DfltSiteID;
			}
			set
			{
				this._DfltSiteID = value;
			}
		}
		#endregion
		#region DfltShipLocationID
		public abstract class dfltShipLocationID : PX.Data.BQL.BqlInt.Field<dfltShipLocationID> { }
		protected Int32? _DfltShipLocationID;

		/// <summary>
		/// The <see cref="INLocation">Location of warehouse</see> used by default to issue items of this kind.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) when the <see cref="FeaturesSet.WarehouseLocation">Warehouse Locations</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INLocation.LocationID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(InventoryItem.dfltSiteID), DisplayName = "Default Issue From", KeepEntry = false, ResetEntry = false, DescriptionField = typeof(INLocation.descr))]
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

		/// <summary>
		/// The <see cref="INLocation">Location of warehouse</see> used by default to receive items of this kind.
		/// Applicable only for Stock Items (see <see cref="StkItem"/>) when the <see cref="FeaturesSet.WarehouseLocation">Warehouse Locations</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INLocation.LocationID"/> field.
		/// </value>
		[PXRestrictor(typeof(Where<INLocation.active, Equal<True>>), Messages.LocationIsNotActive)]
		[IN.Location(typeof(InventoryItem.dfltSiteID), DisplayName = "Default Receipt To", KeepEntry = false, ResetEntry = false, DescriptionField = typeof(INLocation.descr))]
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
		#region ProductWorkgroupID
		public abstract class productWorkgroupID : PX.Data.BQL.BqlInt.Field<productWorkgroupID> { }
		protected Int32? _ProductWorkgroupID;

		/// <summary>
		/// The workgroup that is responsible for the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPCompanyTree.WorkGroupID"/> field.
		/// </value>
		[PXDBInt()]
		[PXWorkgroupSelector]
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

		/// <summary>
		/// The <see cref="EPEmployee">product manager</see> responsible for this item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDBGuid()]
		[PXOwnerSelector(typeof(InventoryItem.productWorkgroupID))]
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

		/// <summary>
		/// The workgroup that is responsible for the pricing of this item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPCompanyTree.WorkGroupID"/> field.
		/// </value>
		[PXDBInt()]
		[PXWorkgroupSelector]
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

		/// <summary>
		/// The <see cref="EPEmployee">manager</see> responsible for the pricing of this item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="EPEmployee.PKID"/> field.
		/// </value>
		[PXDBGuid()]
		[PXOwnerSelector(typeof(InventoryItem.priceWorkgroupID))]
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
		#region NegQty
		public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
		protected bool? _NegQty;

		/// <summary>
		/// An unbound field that, when equal to <c>true</c>, indicates that negative quantities are allowed for this item.
		/// </summary>
		/// <value>
		/// The value of this field is taken from the <see cref="INItemClass.NegQty"/> field of the <see cref="ItemClass">Class</see>, to which the item belongs.
		/// </value>
		[PXBool()]
		[PXFormula(typeof(Selector<itemClassID, INItemClass.negQty>))]
		public virtual bool? NegQty
		{
			get
			{
				return this._NegQty;
			}
			set
			{
				this._NegQty = value;
			}
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;

		/// <summary>
		/// The <see cref="INLotSerClass">lot/serial class</see>, to which the item is assigned.
		/// This field is relevant only if the <see cref="FeaturesSet.LotSerialTracking">Lot/Serial Tracking</see> feature is enabled.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INLotSerClass.LotSerClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INLotSerClass.lotSerClassID), DescriptionField = typeof(INLotSerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Lot/Serial Class")]
		public virtual String LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region LotSerNumberResult
		public abstract class lotSerNumberResult : PX.Data.BQL.BqlString.Field<lotSerNumberResult> { }
		protected String _LotSerNumberResult;

		/// <summary>
		/// The Lot/Serial Number generated for the item by the system.
		/// The Lot/Serial Numbers are generated by the <see cref="InventoryItemMaint.GenerateLotSerialNumber"/> action of the <see cref="InventoryItemMaint"/> graph
		/// (corresponds to the Stock Items (IN.20.25.00) screen).
		/// </summary>
		[PXString(30, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Result of Generation Lot/Serial Number", Enabled = false)]
		public virtual String LotSerNumberResult
		{
			get
			{
				return this._LotSerNumberResult;
			}
			set
			{
				this._LotSerNumberResult = value;
			}
		}
		#endregion
		#region PostClassID
		public abstract class postClassID : PX.Data.BQL.BqlString.Field<postClassID> { }
		protected String _PostClassID;

		/// <summary>
		/// Identifier of the <see cref="INPostClass">Posting Class</see> associated with the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.PostClassID">Posting Class</see> selected for the <see cref="ItemClassID">item class</see>.
		/// Corresponds to the <see cref="INPostClass.PostClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Posting Class")]
		public virtual String PostClassID
		{
			get
			{
				return this._PostClassID;
			}
			set
			{
				this._PostClassID = value;
			}
		}
		#endregion
		#region DeferredCode
		public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
		protected String _DeferredCode;

		/// <summary>
		/// The deferral code used to perform deferrals on sale of the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.DeferredCode">Deferral Code</see> selected for the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="DRDeferredCode.DeferredCodeID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code")]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXRestrictor(typeof(Where<DRDeferredCode.active, Equal<True>>), DR.Messages.InactiveDeferralCode, typeof(DRDeferredCode.deferredCodeID))]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.deferredCode),
			PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
		public virtual String DeferredCode
		{
			get
			{
				return this._DeferredCode;
			}
			set
			{
				this._DeferredCode = value;
			}
		}
		#endregion
		#region DefaultTerm
		public abstract class defaultTerm : PX.Data.BQL.BqlDecimal.Field<defaultTerm> { }

		protected decimal? _DefaultTerm;

		[PXDBDecimal(0, MinValue = 0.0, MaxValue = 10000.0)]
		[PXUIField(DisplayName = "Default Term")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DefaultTerm
		{
			get { return this._DefaultTerm; }
			set { this._DefaultTerm = value; }
		}
		#endregion

		#region DefaultTermUOM
		public abstract class defaultTermUOM : PX.Data.BQL.BqlString.Field<defaultTermUOM> { }

		protected string _DefaultTermUOM;

		[PXDBString(1, IsFixed = true, IsUnicode = false)]
		[PXUIField(DisplayName = "Default Term UOM")]
		[DRTerms.UOMList]
		[PXDefault(DRTerms.Year)]
		public virtual string DefaultTermUOM
		{
			get { return this._DefaultTermUOM; }
			set { this._DefaultTermUOM = value; }
		}
		#endregion
		#region PriceClassID
		public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
		protected String _PriceClassID;

		/// <summary>
		/// The <see cref="INPriceClass">Item Price Class</see> associated with the item.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.PriceClassID">Price Class</see> selected for the <see cref="ItemClassID">Item Class</see>.
		/// Corresponds to the <see cref="INPriceClass.PriceClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(INPriceClass.priceClassID), DescriptionField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.priceClassID),
			PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
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
		#region IsSplitted
		public abstract class isSplitted : PX.Data.BQL.BqlBool.Field<isSplitted> { }
		protected Boolean? _IsSplitted;

		/// <summary>
		/// When set to <c>true</c>, indicates that the system should split the revenue from sale of the item among its components.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Split into Components")]
		public virtual Boolean? IsSplitted
		{
			get
			{
				return this._IsSplitted;
			}
			set
			{
				this._IsSplitted = value;
			}
		}
		#endregion
		#region UseParentSubID
		public abstract class useParentSubID : PX.Data.BQL.BqlBool.Field<useParentSubID> { }
		protected Boolean? _UseParentSubID;

		/// <summary>
		/// When set to <c>true</c>, indicates that the system should use the component subaccounts in the component-associated deferrals.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Component Subaccounts", FieldClass = SubAccountAttribute.DimensionName)]
		public virtual Boolean? UseParentSubID
		{
			get
			{
				return this._UseParentSubID;
			}
			set
			{
				this._UseParentSubID = value;
			}
		}
		#endregion
		#region TotalPercentage
		public abstract class totalPercentage : PX.Data.BQL.BqlDecimal.Field<totalPercentage> { }
		protected Decimal? _TotalPercentage;

		/// <summary>
		/// The total percentage of the item price as split among components.
		/// </summary>
		/// <value>
		/// The value is calculated automatically from the <see cref="INComponent.Percentage">percentages</see>
		/// of the <see cref="INComponent">components</see> associated with the item.
		/// Set to <c>100</c> if the item is not a package.
		/// </value>
		[PXDecimal()]
		[PXUIField(DisplayName = "Total Percentage", Enabled = false)]
		public virtual Decimal? TotalPercentage
		{
			get
			{
				return this._TotalPercentage;
			}
			set
			{
				this._TotalPercentage = value;
			}
		}
		#endregion
		#region KitItem
		public abstract class kitItem : PX.Data.BQL.BqlBool.Field<kitItem> { }
		protected Boolean? _KitItem;

		/// <summary>
		/// When set to <c>true</c>, indicates that the item is a kit.
		/// Kits are stock or non-stock items that consist of other items and are sold as a whole.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is a Kit", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? KitItem
		{
			get
			{
				return this._KitItem;
			}
			set
			{
				this._KitItem = value;
			}
		}
		#endregion
		#region MinGrossProfitPct
		public abstract class minGrossProfitPct : PX.Data.BQL.BqlDecimal.Field<minGrossProfitPct> { }
		protected Decimal? _MinGrossProfitPct;

		/// <summary>
		/// The minimum markup percentage for the item.
		/// See the <see cref="MarkupPct"/> field.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.MinGrossProfitPct">Min. Margup %</see> defined for the <see cref="ItemClassID">Item Class</see>.
		/// </value>
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			SourceField = typeof(INItemClass.minGrossProfitPct), CacheGlobal = true)]
		[PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
		[PXUIField(DisplayName = "Min. Markup %", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? MinGrossProfitPct
		{
			get
			{
				return this._MinGrossProfitPct;
			}
			set
			{
				this._MinGrossProfitPct = value;
			}
		}
		#endregion
		#region NonStockReceipt
		public abstract class nonStockReceipt : PX.Data.BQL.BqlBool.Field<nonStockReceipt> { }
		protected Boolean? _NonStockReceipt;

		/// <summary>
		/// Reserved for internal use.
		/// Indicates whether the item (assumed Non-Stock) requires receipt.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Receipt")]
		public virtual Boolean? NonStockReceipt
		{
			get
			{
				return this._NonStockReceipt;
			}
			set
			{
				this._NonStockReceipt = value;
			}
		}
		#endregion
		#region NonStockShip
		public abstract class nonStockShip : PX.Data.BQL.BqlBool.Field<nonStockShip> { }
		protected Boolean? _NonStockShip;

		/// <summary>
		/// Reserved for internal use.
		/// Indicates whether the item (assumed Non-Stock) requires shipment.
		/// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Require Shipment")]
		public virtual Boolean? NonStockShip
		{
			get
			{
				return this._NonStockShip;
			}
			set
			{
				this._NonStockShip = value;
			}
		}
		#endregion
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost> { }
		protected Boolean? _AccrueCost;

		/// <summary>
		/// When set to <c>true</c>, indicates that cost will be processed using expense accrual account.
		/// </summary>
		[PXDBBool()]
		[PXUIEnabled(typeof(Where<kitItem, NotEqual<True>>))]
		[PXDefault(false, typeof(Search<INItemClass.accrueCost, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>))]
		[PXUIField(DisplayName = "Accrue Cost")]
		public virtual Boolean? AccrueCost
		{
			get
			{
				return this._AccrueCost;
			}
			set
			{
				this._AccrueCost = value;
			}
		}
		#endregion
		#region CostBasis
		public abstract class costBasis : PX.Data.BQL.BqlString.Field<costBasis> { }
		protected String _CostBasis;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Cost Based On")]
		[PXUIEnabled(typeof(Where<accrueCost, Equal<True>, And<kitItem, NotEqual<True>>>))]
		[PXDefault(CostBasisOption.StandardCost)]
		[CostBasisOption.List()]
		public virtual String CostBasis
		{
			get
			{
				return this._CostBasis;
			}
			set
			{
				this._CostBasis = value;
			}
		}
		#endregion
		#region PercentOfSalesPrice
		public abstract class percentOfSalesPrice : PX.Data.BQL.BqlDecimal.Field<percentOfSalesPrice> { }
		protected Decimal? _PercentOfSalesPrice;
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
		[PXUIEnabled(typeof(Where<accrueCost, Equal<True>, And<costBasis, Equal<CostBasisOption.percentOfSalesPrice>>>))]
		[PXUIField(DisplayName = "Percent of Sales Price", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? PercentOfSalesPrice
		{
			get
			{
				return this._PercentOfSalesPrice;
			}
			set
			{
				this._PercentOfSalesPrice = value;
			}
		}
		#endregion
		#region CompletePOLine
		public abstract class completePOLine : PX.Data.BQL.BqlString.Field<completePOLine> { }
		protected String _CompletePOLine;
		[PXDBString(1, IsFixed = true)]
		[PXDefault]
		[PXFormula(typeof(Switch<Case<Where<itemType, Equal<INItemTypes.laborItem>, Or<itemType, Equal<INItemTypes.serviceItem>, Or<itemType, Equal<INItemTypes.chargeItem>, Or<itemType, Equal<INItemTypes.expenseItem>>>>>, CompletePOLineTypes.amount>, CompletePOLineTypes.quantity>))]
		[PXUIField(DisplayName = "Close PO Line")]
		[CompletePOLineTypes.List()]
		public virtual String CompletePOLine
		{
			get
			{
				return this._CompletePOLine;
			}
			set
			{
				this._CompletePOLine = value;
			}
		}
		#endregion

		#region ABCCodeID
		public abstract class aBCCodeID : PX.Data.BQL.BqlString.Field<aBCCodeID> { }
		protected String _ABCCodeID;

		/// <summary>
		/// The <see cref="INABCCode">ABC code</see>, to which the item is assigned for the purpose of physical inventories.
		/// The field is relevant only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INABCCode.ABCCodeID"/> field.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "ABC Code")]
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

		/// <summary>
		/// When set to <c>true</c>, indicates that the system must not change the <see cref="ABCCodeID">ABC Code</see>
		/// assigned to the item when ABC code assignments are updated.
		/// The field is relevant only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
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

		/// <summary>
		/// The <see cref="INMovementClass">Movement Class</see>, to which the item is assigned for the purpose of physical inventories.
		/// The field is relevant only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INMovementClass.MovementClassID"/> field.
		/// </value>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Movement Class")]
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

		/// <summary>
		/// When set to <c>true</c>, indicates that the system must not change the <see cref="MovementClassID">Movement Class</see>
		/// assigned to the item when movement class assignments are updated.
		/// The field is relevant only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
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

		#region MarkupPct
		public abstract class markupPct : PX.Data.BQL.BqlDecimal.Field<markupPct> { }
		protected Decimal? _MarkupPct;

		/// <summary>
		/// The percentage that is added to the item cost to get the selling price for it.
		/// </summary>
		/// <value>
		/// Defaults to the <see cref="INItemClass.MarkupPct">Markup %</see> specified for the <see cref="ItemClassID">Item Class</see>.
		/// </value>
		[PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.markupPct), CacheGlobal = true)]
		[PXUIField(DisplayName = "Markup %")]
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
		#region RecPrice
		public abstract class recPrice : PX.Data.BQL.BqlDecimal.Field<recPrice> { }
		protected Decimal? _RecPrice;

		/// <summary>
		/// The manufacturer's suggested retail price of the item.
		/// </summary>
		[PXDBPriceCost()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "MSRP")]
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
		#region ImageUrl
		public abstract class imageUrl : PX.Data.BQL.BqlString.Field<imageUrl> { }
		protected String _ImageUrl;

		/// <summary>
		/// The URL of the image associated with the item.
		/// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Image")]
		public virtual String ImageUrl
		{
			get
			{
				return this._ImageUrl;
			}
			set
			{
				this._ImageUrl = value;
			}
		}
		#endregion

		#region HSTariffCode
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tariff Code")]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>), SourceField = typeof(INItemClass.hSTariffCode),
			PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
		[PXFormula(typeof(Default<InventoryItem.itemClassID>))]
		public virtual string HSTariffCode { get; set; }

		public abstract class hSTariffCode : PX.Data.BQL.BqlString.Field<hSTariffCode> { }
		#endregion

		#region UndershipThreshold
		public abstract class undershipThreshold : PX.Data.BQL.BqlDecimal.Field<undershipThreshold> { }
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			SourceField = typeof(INItemClass.undershipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Undership Threshold (%)")]
		public virtual decimal? UndershipThreshold
		{
			get;
			set;
		}
		#endregion
		#region OvershipThreshold
		public abstract class overshipThreshold : PX.Data.BQL.BqlDecimal.Field<overshipThreshold> { }
		[PXDBDecimal(2, MinValue = 100.0, MaxValue = 999.0)]
		[PXDefault(TypeCode.Decimal, "100.0",
			typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			SourceField = typeof(INItemClass.overshipThreshold), CacheGlobal = true)]
		[PXUIField(DisplayName = "Overship Threshold (%)")]
		public virtual decimal? OvershipThreshold
		{
			get;
			set;
		}
		#endregion

		#region CountryOfOrigin
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Country Of Origin")]
		[PXDefault(typeof(Select<INItemClass, Where<INItemClass.itemClassID, Equal<Current<InventoryItem.itemClassID>>>>),
			SourceField = typeof(INItemClass.countryOfOrigin), PersistingCheck = PXPersistingCheck.Nothing, CacheGlobal = true)]
		[PXFormula(typeof(Default<InventoryItem.itemClassID>))]
		[Country]
		public virtual string CountryOfOrigin { get; set; }

		public abstract class countryOfOrigin : PX.Data.BQL.BqlString.Field<countryOfOrigin> { }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXSearchable(SM.SearchCategory.IN, "{0}: {1}", new Type[] { typeof(InventoryItem.itemType), typeof(InventoryItem.inventoryCD) },
		  new Type[] { typeof(InventoryItem.descr) },
		   NumberFields = new Type[] { typeof(InventoryItem.inventoryCD) },
		   Line1Format = "{0}{1}{2}", Line1Fields = new Type[] { typeof(INItemClass.itemClassCD), typeof(INItemClass.descr), typeof(InventoryItem.baseUnit) },
		   Line2Format = "{0}", Line2Fields = new Type[] { typeof(InventoryItem.descr) },
		   WhereConstraint = typeof(Where<Current<InventoryItem.itemStatus>, NotEqual<InventoryItemStatus.unknown>>)
		)]
		[PXNote(PopupTextEnabled = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
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
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		/// <summary>
		/// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the item belongs to.
		/// </summary>
		[PXDBGroupMask()]
		public virtual Byte[] GroupMask
		{
			get
			{
				return this._GroupMask;
			}
			set
			{
				this._GroupMask = value;
			}
		}
		#endregion
		#region CycleID
		public abstract class cycleID : PX.Data.BQL.BqlString.Field<cycleID> { }
		protected String _CycleID;

		/// <summary>
		/// The <see cref="INPICycle">Physical Inventory Cycle</see> assigned to the item.
		/// The cycle defines how often the physical inventory counts will be performed for the item.
		/// The field is relevant only for Stock Items (see <see cref="StkItem"/>).
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INPICycle.CycleID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "PI Cycle")]
		[PXSelector(typeof(INPICycle.cycleID), DescriptionField = typeof(INPICycle.descr))]
		public virtual String CycleID
		{
			get
			{
				return this._CycleID;
			}
			set
			{
				this._CycleID = value;
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

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }
		protected string[] _Attributes;

		/// <summary>
		/// Reserved for internal use.
		/// Provides the values of attributes associated with the item.
		/// For more information see the <see cref="CSAnswers"/> class.
		/// </summary>
		[CRAttributesField(typeof(InventoryItem.parentItemClassID))]
		public virtual string[] Attributes { get; set; }
		/// <summary>
		/// Reserved for internal use.
		/// <see cref="IAttributeSupport"/> implementation. The record class ID for attributes retrieval.
		/// </summary>
		public virtual int? ClassID
		{
			get { return ItemClassID; }
		}

		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;

		/// <summary>
		/// An unbound field used in the User Interface to include the item into a <see cref="PX.SM.RelationGroup">restriction group</see>.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Included")]
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? Included
		{
			get
			{
				return this._Included;
			}
			set
			{
				this._Included = value;
			}
		}
		#endregion

		#region Body
		public abstract class body : PX.Data.BQL.BqlString.Field<body> { }
		protected string _Body;

		/// <summary>
		/// Rich text description of the item.
		/// </summary>
		[PXDBLocalizableString(IsUnicode = true)]
		[PXUIField(DisplayName = "Content")]
		public virtual string Body
		{
			get
			{
				return this._Body;
			}
			set
			{
				this._Body = value;
			}
		}
		#endregion

		#region Matrix

		#region IsTemplate
		public abstract class isTemplate : Data.BQL.BqlBool.Field<isTemplate> { }

		/// <summary>
		/// When set to <c>true</c>, indicates that the item is a template for other matrix items.
		/// </summary>
		/// <value>
		/// Defaults to <c>false</c>.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsTemplate
		{
			get;
			set;
		}
		#endregion
		#region TemplateItemID
		public abstract class templateItemID : PX.Data.BQL.BqlInt.Field<templateItemID> { }
		/// <summary>
		/// References to parent Inventory Item, its database identifier, if this item was created from template.
		/// </summary>
		[PXUIField(DisplayName = "Template Item", FieldClass = nameof(FeaturesSet.MatrixItem))]
		[TemplateInventory]
		[PXForeignReference(typeof(Field<templateItemID>.IsRelatedTo<inventoryID>))]
		public virtual int? TemplateItemID
		{
			get;
			set;
		}
		#endregion
		#region DefaultRowMatrixAttributeID
		public abstract class defaultRowMatrixAttributeID : PX.Data.BQL.BqlString.Field<defaultRowMatrixAttributeID>
		{
		}

		/// <summary>
		/// References to Attribute which will be put as Row Attribute in Inventory Matrix by default.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Row Attribute ID", FieldClass = nameof(FeaturesSet.MatrixItem))]
		[PXSelector(typeof(Search2<CSAttribute.attributeID,
			InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
			Where<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.parentItemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
				And<Where2<Where<CSAttributeGroup.attributeID, NotEqual<Current<InventoryItem.defaultColumnMatrixAttributeID>>>,
					Or<Where<Current<InventoryItem.defaultColumnMatrixAttributeID>, IsNull>>>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		[PXRestrictor(typeof(Where<CSAttributeGroup.isActive, Equal<True>>), Messages.AttributeIsInactive, typeof(CSAttributeGroup.attributeID))]
		public virtual string DefaultRowMatrixAttributeID
		{
			get;
			set;
		}
		#endregion
		#region DefaultColumnMatrixAttributeID
		public abstract class defaultColumnMatrixAttributeID : PX.Data.BQL.BqlString.Field<defaultColumnMatrixAttributeID> { }

		/// <summary>
		/// References to Attribute which will be put as Column Attribute in Inventory Matrix by default.
		/// </summary>
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Default Column Attribute ID", FieldClass = nameof(FeaturesSet.MatrixItem))]
		[PXSelector(typeof(Search2<CSAttribute.attributeID,
			InnerJoin<CSAttributeGroup, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
			Where<CSAttributeGroup.entityClassID, Equal<Current<InventoryItem.parentItemClassID>>,
				And<CSAttributeGroup.entityType, Equal<Constants.DACName<InventoryItem>>,
				And<CSAttributeGroup.attributeCategory, Equal<CSAttributeGroup.attributeCategory.variant>,
				And<Where2<Where<CSAttributeGroup.attributeID, NotEqual<Current<InventoryItem.defaultRowMatrixAttributeID>>>,
					Or<Where<Current<InventoryItem.defaultRowMatrixAttributeID>, IsNull>>>>>>>>),
			typeof(CSAttributeGroup.attributeID), DescriptionField = typeof(CSAttributeGroup.description))]
		[PXRestrictor(typeof(Where<CSAttributeGroup.isActive, Equal<True>>), Messages.AttributeIsInactive, typeof(CSAttributeGroup.attributeID))]
		public virtual string DefaultColumnMatrixAttributeID
		{
			get;
			set;
		}
		#endregion
		#region GenerationRuleCntr
		public abstract class generationRuleCntr : PX.Data.BQL.BqlInt.Field<generationRuleCntr> { }
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? GenerationRuleCntr
		{
			get;
			set;
		}
		#endregion
		#region HasChild
		public abstract class hasChild : PX.Data.BQL.BqlString.Field<hasChild> { }

		/// <summary>
		/// The flag is true if there is Inventory Item which has TemplateItemId equals current InventoryID value.
		/// </summary>
		[PXBool]
		public virtual bool? HasChild
		{
			get;
			set;
		}
		#endregion
		#region AttributeDescriptionGroupID
		public abstract class attributeDescriptionGroupID : PX.Data.BQL.BqlInt.Field<attributeDescriptionGroupID> { }
		/// <summary>
		/// References to parent Group <see cref="INAttributeDescriptionGroup.GroupID"/>
		/// </summary>
		[PXDBInt]
		public virtual int? AttributeDescriptionGroupID
		{
			get;
			set;
		}
		#endregion
		#region ColumnAttributeValue
		public abstract class columnAttributeValue : PX.Data.BQL.BqlString.Field<columnAttributeValue> { }
		/// <summary>
		/// Value of column matrix attribute of template item.
		/// </summary>
		[PXAttributeValue]
		public virtual string ColumnAttributeValue
		{
			get;
			set;
		}
		#endregion
		#region RowAttributeValue
		public abstract class rowAttributeValue : PX.Data.BQL.BqlString.Field<rowAttributeValue> { }
		/// <summary>
		/// Value of row matrix attribute of template item.
		/// </summary>
		[PXAttributeValue]
		public virtual string RowAttributeValue
		{
			get;
			set;
		}
		#endregion
		#region SampleID
		public abstract class sampleID : PX.Data.BQL.BqlString.Field<sampleID> { }
		/// <summary>
		/// Contains Inventory ID Example <see cref="Matrix.DAC.Projections.IDGenerationRule" />
		/// </summary>
		[PXString]
		public virtual string SampleID
		{
			get;
			set;
		}
		#endregion
		#region SampleDescription
		public abstract class sampleDescription : PX.Data.BQL.BqlString.Field<sampleDescription> { }
		/// <summary>
		/// Contains Inventory Description Example <see cref="Matrix.DAC.Projections.DescriptionGenerationRule" />
		/// </summary>
		[PXString]
		public virtual string SampleDescription
		{
			get;
			set;
		}
		#endregion

		#endregion

		#region DiscAcctID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class discAcctID : Data.BQL.BqlInt.Field<discAcctID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? DiscAcctID
		{
			get { return null; }
			set { }
		}
		#endregion
		#region DiscSubID
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public abstract class discSubID : Data.BQL.BqlInt.Field<discSubID> { }
		[PXInt]
		[Obsolete("The field is preserved only for the support of the Default Endpoints.")]
		public virtual int? DiscSubID
		{
			get { return null; }
			set { }
		}
		#endregion
	}

	public class InventoryItemStatus
	{
		public const string Active = "AC";
		public const string NoSales = "NS";
		public const string NoPurchases = "NP";
		public const string NoRequest = "NR";
		public const string Inactive = "IN";
		public const string MarkedForDeletion = "DE";
		public const string Unknown = "XX";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Active, Messages.Active),
					Pair(NoSales, Messages.NoSales),
					Pair(NoPurchases, Messages.NoPurchases),
					Pair(NoRequest, Messages.NoRequest),
					Pair(Inactive, Messages.Inactive),
					Pair(MarkedForDeletion, Messages.ToDelete),
				}) {}
		}

		public class SubItemListAttribute : PXStringListAttribute
		{
			public SubItemListAttribute() : base(
				new[]
				{
					Pair(Active, Messages.Active),
					Pair(NoSales, Messages.NoSales),
					Pair(NoPurchases, Messages.NoPurchases),
					Pair(NoRequest, Messages.NoRequest),
					Pair(Inactive, Messages.Inactive),
				}) {}
		}

		public class active : PX.Data.BQL.BqlString.Constant<active> { public active() : base(Active) { } }
		public class noSales : PX.Data.BQL.BqlString.Constant<noSales> { public noSales() : base(NoSales) { } }
		public class noPurchases : PX.Data.BQL.BqlString.Constant<noPurchases> { public noPurchases() : base(NoPurchases) { } }
		public class noRequest : PX.Data.BQL.BqlString.Constant<noRequest> { public noRequest() : base(NoRequest) { } }
		public class inactive : PX.Data.BQL.BqlString.Constant<inactive> { public inactive() : base(Inactive) { } }
		public class markedForDeletion : PX.Data.BQL.BqlString.Constant<markedForDeletion> { public markedForDeletion() : base(MarkedForDeletion) { } }
		public class unknown : PX.Data.BQL.BqlString.Constant<unknown> { public unknown() : base(Unknown) { } }
	}

    public class CompletePOLineTypes
    {
        public const string Amount = "A";
        public const string Quantity = "Q";
        
        public class ListAttribute : PXStringListAttribute
        {
		    public ListAttribute() : base(
			    new[]
				{
					Pair(Amount, Messages.ByAmount),
					Pair(Quantity, Messages.ByQuantity),
				}) {}
        }

        public class amount : PX.Data.BQL.BqlString.Constant<amount> { public amount() : base(Amount) { } }
        public class quantity : PX.Data.BQL.BqlString.Constant<quantity> { public quantity() : base(Quantity) { } }
    }

	#region Attributes
	public class INItemTypes
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;
			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] AllowedValues, string[] AllowedLabels) : base(AllowedValues, AllowedLabels) {}
			public CustomListAttribute(Tuple<string,string>[] valuesToLabels) : base(valuesToLabels) {}
		}

		public class StockListAttribute : CustomListAttribute
		{
			public StockListAttribute() : base(
				new[]
				{
					Pair(FinishedGood, Messages.FinishedGood),
					Pair(Component, Messages.Component),
					Pair(SubAssembly, Messages.SubAssembly),
				}) {}
		}

		public class NonStockListAttribute : CustomListAttribute
		{
			public NonStockListAttribute() : base(
				new[]
				{
					Pair(NonStockItem, Messages.NonStockItem),
					Pair(LaborItem, Messages.LaborItem),
					Pair(ServiceItem, Messages.ServiceItem),
					Pair(ChargeItem, Messages.ChargeItem),
					Pair(ExpenseItem, Messages.ExpenseItem),
				}) {}
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(FinishedGood, Messages.FinishedGood),
					Pair(Component, Messages.Component),
					Pair(SubAssembly, Messages.SubAssembly),
					Pair(NonStockItem, Messages.NonStockItem),
					Pair(LaborItem, Messages.LaborItem),
					Pair(ServiceItem, Messages.ServiceItem),
					Pair(ChargeItem, Messages.ChargeItem),
					Pair(ExpenseItem, Messages.ExpenseItem),
				}) {}
		}

		public const string NonStockItem = "N";
		public const string LaborItem = "L";
		public const string ServiceItem = "S";
		public const string ChargeItem = "C";
		public const string ExpenseItem = "E";

		public const string FinishedGood = "F";
		public const string Component = "M";
		public const string SubAssembly = "A";


		public class nonStockItem : PX.Data.BQL.BqlString.Constant<nonStockItem>
		{
			public nonStockItem() : base(NonStockItem) {}
		}

		public class laborItem : PX.Data.BQL.BqlString.Constant<laborItem>
		{
			public laborItem() : base(LaborItem) {}
		}

		public class serviceItem : PX.Data.BQL.BqlString.Constant<serviceItem>
		{
			public serviceItem() : base(ServiceItem) {}
		}

		public class chargeItem : PX.Data.BQL.BqlString.Constant<chargeItem>
		{
			public chargeItem() : base(ChargeItem) {}
		}

		public class expenseItem : PX.Data.BQL.BqlString.Constant<expenseItem>
		{
			public expenseItem() : base(ExpenseItem) {}
		}

		public class finishedGood : PX.Data.BQL.BqlString.Constant<finishedGood>
		{
			public finishedGood() : base(FinishedGood) {}
		}

		public class component : PX.Data.BQL.BqlString.Constant<component>
		{
			public component() : base(Component) {}
		}

		public class subAssembly : PX.Data.BQL.BqlString.Constant<subAssembly>
		{
			public subAssembly() : base(SubAssembly) {}
		}


	}

	public class INValMethod
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Standard, Messages.Standard),
					Pair(Average, Messages.Average),
					Pair(FIFO, Messages.FIFO),
					Pair(Specific, Messages.Specific),
				}) {}
		}

		public const string Standard = "T";
		public const string Average = "A";
		public const string FIFO = "F";
		public const string Specific = "S";

		public class standard : PX.Data.BQL.BqlString.Constant<standard>
		{
			public standard() : base(Standard) {}
		}

		public class average : PX.Data.BQL.BqlString.Constant<average>
		{
			public average() : base(Average) {}
		}

		public class fIFO : PX.Data.BQL.BqlString.Constant<fIFO>
		{
			public fIFO() : base(FIFO) {}
		}

		public class specific : PX.Data.BQL.BqlString.Constant<specific>
		{
			public specific() : base(Specific) {}
		}
	}

	public class INItemStatus
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
			{
					Pair(Active, Messages.Active),
					Pair(NoSales, Messages.NoSales),
					Pair(NoPurchases, Messages.NoPurchases),
					Pair(Inactive, Messages.Inactive),
					Pair(ToDelete, Messages.ToDelete),
				}) {}
		}

		public const string Active = "AC";
		public const string Inactive = "IN";
		public const string NoSales = "NS";
		public const string NoPurchases = "NP";
		public const string ToDelete = "DE";

		public class active : PX.Data.BQL.BqlString.Constant<active>
		{
			public active() : base(Active) {}
		}

		public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
		{
			public inactive() : base(Inactive) {}
		}

		public class noSales : PX.Data.BQL.BqlString.Constant<noSales>
		{
			public noSales() : base(NoSales) {}
		}

		public class noPurchases : PX.Data.BQL.BqlString.Constant<noPurchases>
		{
			public noPurchases() : base(NoPurchases) {}
		}

		public class toDelete : PX.Data.BQL.BqlString.Constant<toDelete>
		{
			public toDelete() : base(ToDelete) {}
		}
	}

	public class INPackageOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Manual, Messages.Manual),
					Pair(Weight, Messages.Weight),
					Pair(Quantity, Messages.Quantity),
					Pair(WeightAndVolume, Messages.WeightAndVolume),
				}) {}
		}

		public const string Manual = "N";
		public const string Weight = "W";
		public const string Quantity = "Q";
		public const string WeightAndVolume = "V";

		public class weight : PX.Data.BQL.BqlString.Constant<weight>
		{
			public weight() : base(Weight) {}
		}
	}
	
	public class CostBasisOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(StandardCost, Messages.StandardCost),
					Pair(PriceMarkupPercent, Messages.PriceMarkupPercent),
					Pair(PercentOfSalesPrice, Messages.PercentOfSalesPrice)
				}) {}
		}

		public const string StandardCost = "S";
		public const string PriceMarkupPercent = "M";
		public const string PercentOfSalesPrice = "P";
		public const string UndefinedCostBasis = "U";

		public class standardCost : PX.Data.BQL.BqlString.Constant<standardCost>
		{
			public standardCost() : base(StandardCost) {}
		}
		public class priceMarkupPercent : PX.Data.BQL.BqlString.Constant<priceMarkupPercent>
		{
			public priceMarkupPercent() : base(PriceMarkupPercent) {}
		}
		public class percentOfSalesPrice : PX.Data.BQL.BqlString.Constant<percentOfSalesPrice>
		{
			public percentOfSalesPrice() : base(PercentOfSalesPrice) { }
		}
		public class undefinedCostBasis : PX.Data.BQL.BqlString.Constant<undefinedCostBasis>
		{
			public undefinedCostBasis() : base(UndefinedCostBasis) { }
		}
	}

	#endregion
}
