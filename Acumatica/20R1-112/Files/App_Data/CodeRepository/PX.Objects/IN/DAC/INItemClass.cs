using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.DR;
using PX.Objects.TX;
using PX.TM;
using SelectParentItemClass = PX.Data.Select<PX.Objects.IN.INItemClass, PX.Data.Where<PX.Objects.IN.INItemClass.itemClassID, PX.Data.Equal<PX.Data.Current<PX.Objects.IN.INItemClass.parentItemClassID>>>>;

namespace PX.Objects.IN
{
	

	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(INItemClassMaint))]
	[PXCacheName(Messages.ItemClass, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class INItemClass : PX.Data.IBqlTable, PX.SM.IIncludable
	{
		public const string Dimension = "INITEMCLASS";
		public class dimension : PX.Data.BQL.BqlString.Constant<dimension> { public dimension() : base(Dimension) { } }

		#region Keys
		public class PK : PrimaryKeyOf<INItemClass>.By<itemClassID>
		{
			public static INItemClass Find(PXGraph graph, int? itemClassID) => FindBy(graph, itemClassID);
		}
		public static class FK
		{
			public class DfltSite : INSite.PK.ForeignKeyOf<INItemClass>.By<dfltSiteID> { }
			public class AvailabilityScheme : INAvailabilityScheme.PK.ForeignKeyOf<INItemClass>.By<availabilitySchemeID> { }
			public class PostClass : INPostClass.PK.ForeignKeyOf<INItemClass>.By<postClassID> { }
			public class PriceClass : INPriceClass.PK.ForeignKeyOf<INItemClass>.By<priceClassID> { }
			public class LotSerClass : INLotSerClass.PK.ForeignKeyOf<INItemClass>.By<lotSerClassID> { }
			public class TaxCategory : TX.TaxCategory.PK.ForeignKeyOf<INItemClass>.By<taxCategoryID> { }
		}
		#endregion
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual int? ItemClassID { get; set; }
		#endregion
		#region ItemClassCD
		public abstract class itemClassCD : PX.Data.BQL.BqlString.Field<itemClassCD> { }
		protected String _ItemClassCD;
		[PXDefault()]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(Dimension, typeof(Search<itemClassCD, Where<stkItem, Equal<False>, Or<Where<stkItem, Equal<True>, And<FeatureInstalled<FeaturesSet.distributionModule>>>>>>), DescriptionField = typeof(descr))]
		[PX.Data.EP.PXFieldDescription]
		public virtual String ItemClassCD
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
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(Common.Constants.TranDescLength, IsUnicode = true)]
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
		#region StkItem
		public abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem> { }
		protected Boolean? _StkItem;
		[PXDBBool()]
		[PXDefault]
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
		#region ParentItemClassID
		public abstract class parentItemClassID : PX.Data.BQL.BqlInt.Field<parentItemClassID> { }
		/// <summary>
		/// The field is used to populate standard settings of <see cref="INItemClass">Item Class</see> from it's parent or default one.
		/// </summary>
		[PXInt]
		public virtual int? ParentItemClassID { get; set; }
		#endregion
		#region NegQty
		public abstract class negQty : PX.Data.BQL.BqlBool.Field<negQty> { }
		protected Boolean? _NegQty;
		[PXDBBool()]
		[PXDefault(false, typeof(SelectParentItemClass), SourceField = typeof(INItemClass.negQty), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Allow Negative Quantity")]
		public virtual Boolean? NegQty
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
		#region AvailabilitySchemeID
		public abstract class availabilitySchemeID : PX.Data.BQL.BqlString.Field<availabilitySchemeID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.availabilitySchemeID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Availability Calculation Rule")]
		[PXSelector(typeof(INAvailabilityScheme.availabilitySchemeID), DescriptionField = typeof(INAvailabilityScheme.description))]
		public virtual string AvailabilitySchemeID { get; set; }
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(INValMethod.Average, typeof(SelectParentItemClass), SourceField = typeof(INItemClass.valMethod), PersistingCheck = PXPersistingCheck.Nothing)]
		[INValMethod.List]
		[PXUIEnabled(typeof(stkItem))]
		[PXFormula(typeof(IIf<Where<stkItem, NotEqual<True>>, INValMethod.standard, valMethod>))]
		[PXUIField(DisplayName = "Valuation Method")]
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
		#region BaseUnit
		public abstract class baseUnit : PX.Data.BQL.BqlString.Field<baseUnit> { }
		protected String _BaseUnit;
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.baseUnit))]
		[INSyncUoms(typeof(salesUnit), typeof(purchaseUnit))]
		[INUnit(DisplayName = "Base Unit")]
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
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.salesUnit))]
		[INUnit(null, typeof(INItemClass.baseUnit), DisplayName = "Sales Unit", Visibility = PXUIVisibility.Visible)]
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
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.purchaseUnit))]
		[INUnit(null, typeof(INItemClass.baseUnit), DisplayName = "Purchase Unit", Visibility = PXUIVisibility.Visible)]
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
		[PXDefault(true)]
		[INSyncUoms(typeof(decimalSalesUnit), typeof(decimalPurchaseUnit))]
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
		[PXDefault(true)]
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
		[PXDefault(true)]
		[PXUIField(DisplayName = "Divisible Unit", Visibility = PXUIVisibility.Visible)]
		public virtual bool? DecimalPurchaseUnit
		{
			get { return this._DecimalPurchaseUnit; }
			set { _DecimalPurchaseUnit = value; }
		}
		#endregion
		#region PostClassID
		public abstract class postClassID : PX.Data.BQL.BqlString.Field<postClassID> { }
		protected String _PostClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<INPostClass.postClassID>), DescriptionField = typeof(INPostClass.descr))]
		[PXUIField(DisplayName = "Posting Class")]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.postClassID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region LotSerClassID
		public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }
		protected String _LotSerClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<INLotSerClass.lotSerClassID>), DescriptionField = typeof(INLotSerClass.descr))]
		[PXUIField(DisplayName = "Lot/Serial Class")]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.lotSerClassID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
		protected String _TaxCategoryID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category")]
        [PXSelector(typeof(TaxCategory.taxCategoryID), DescriptionField = typeof(TaxCategory.descr))]
        [PXRestrictor(typeof(Where<TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TaxCategory.taxCategoryID))]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.taxCategoryID), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
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
		[PXDefault(TaxCalculationMode.TaxSetting)]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode
		{
			get;
			set;
		}
		#endregion
		#region DeferredCode
		public abstract class deferredCode : PX.Data.BQL.BqlString.Field<deferredCode> { }
		protected string _DeferredCode;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Deferral Code")]
		[PXSelector(typeof(Search<DRDeferredCode.deferredCodeID>))]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.deferredCode), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region ItemType
		public abstract class itemType : PX.Data.BQL.BqlString.Field<itemType> { }
		protected String _ItemType;
		[PXDBString(1, IsFixed = true)]
		[INItemTypes.List()]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.itemType), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Item Type")]
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
		protected String _PriceClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.priceClassID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(INPriceClass.priceClassID), DescriptionField = typeof(INPriceClass.description))]
		[PXUIField(DisplayName = "Price Class", Visibility = PXUIVisibility.Visible)]
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
		#region PriceWorkgroupID
		public abstract class priceWorkgroupID : PX.Data.BQL.BqlInt.Field<priceWorkgroupID> { }
		protected Int32? _PriceWorkgroupID;
		[PXDBInt()]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.priceWorkgroupID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.priceManagerID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region DfltSiteID
		public abstract class dfltSiteID : PX.Data.BQL.BqlInt.Field<dfltSiteID> { }
		protected Int32? _DfltSiteID;
		[IN.Site(DisplayName = "Default Warehouse", DescriptionField = typeof(INSite.descr))]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.dfltSiteID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region MinGrossProfitPct
		public abstract class minGrossProfitPct : PX.Data.BQL.BqlDecimal.Field<minGrossProfitPct> { }
		protected Decimal? _MinGrossProfitPct;
		[PXDefault(TypeCode.Decimal, "0.0", typeof(SelectParentItemClass), SourceField = typeof(INItemClass.minGrossProfitPct), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBDecimal(2, MinValue = 0)]
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
		#region MarkupPct
		public abstract class markupPct : PX.Data.BQL.BqlDecimal.Field<markupPct> { }
		protected Decimal? _MarkupPct;
		[PXDBDecimal(6, MinValue = 0, MaxValue = 1000)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(SelectParentItemClass), SourceField = typeof(INItemClass.markupPct), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(INItemClass.itemClassCD),
			Selector = typeof(INItemClass.itemClassCD))]
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
		#region DemandCalculation
		public abstract class demandCalculation : PX.Data.BQL.BqlString.Field<demandCalculation> { }
		protected string _DemandCalculation;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Demand Calculation")]
		[PXDefault(INDemandCalculation.ItemClassSettings, typeof(SelectParentItemClass), SourceField = typeof(INItemClass.demandCalculation))]
		[INDemandCalculation.List]
		public virtual string DemandCalculation
		{
			get
			{
				return this._DemandCalculation;
			}
			set
			{
				this._DemandCalculation = value;
			}
		}
		#endregion
		#region HSTariffCode
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Tariff Code")]
		public virtual string HSTariffCode { get; set; }

		public abstract class hSTariffCode : PX.Data.BQL.BqlString.Field<hSTariffCode> { }
		#endregion

		#region UndershipThreshold
		public abstract class undershipThreshold : PX.Data.BQL.BqlDecimal.Field<undershipThreshold> { }
		[PXDBDecimal(2, MinValue = 0.0, MaxValue = 100.0)]
		[PXDefault(TypeCode.Decimal, "100.0")]
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
		[PXDefault(TypeCode.Decimal, "100.0")]
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
		[Country]
		public virtual string CountryOfOrigin { get; set; }

		public abstract class countryOfOrigin : PX.Data.BQL.BqlString.Field<countryOfOrigin> { }
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
		#region AccrueCost
		public abstract class accrueCost : PX.Data.BQL.BqlBool.Field<accrueCost> { }
		protected Boolean? _AccrueCost;

		/// <summary>
		/// When set to <c>true</c>, indicates that cost will be processed using expense accrual account.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIEnabled(typeof(Where<stkItem, Equal<False>>))]
		[PXFormula(typeof(IIf<Where<stkItem, Equal<True>>, False, accrueCost>))]
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
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[PXDBGroupMask()]
		[PXDefault(typeof(SelectParentItemClass), SourceField = typeof(INItemClass.groupMask), PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;
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
		#region ItemClassStrID
		public abstract class itemClassStrID : PX.Data.BQL.BqlString.Field<itemClassStrID> { }
		[PXString]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual string ItemClassStrID => this.ItemClassID.ToString();
		#endregion
		#region ItemClassCDWildcard
		public abstract class itemClassCDWildcard : PX.Data.BQL.BqlString.Field<itemClassCDWildcard> { }
		[PXString(IsUnicode = true)]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDimension(Dimension, ParentSelect = typeof(Select<INItemClass>), ParentValueField = typeof(itemClassCD))]
		public virtual string ItemClassCDWildcard
		{
			get { return ItemClassTree.MakeWildcard(ItemClassCD); }
			set { }
		}
		#endregion
	}

	public class INDemandCalculation
	{
		public const string ItemClassSettings = "I";
		public const string HardDemand = "H";

		public class List : PXStringListAttribute
		{
			public List() : base(
				new[]
				{
					Pair(ItemClassSettings, Messages.ItemClassSettings),
					Pair(HardDemand, Messages.HardDemand),
				}) {}
		}

		public class itemClassSettings : PX.Data.BQL.BqlString.Constant<itemClassSettings>
		{
			public itemClassSettings() : base(ItemClassSettings) { }
		}

		public class hardDemand : PX.Data.BQL.BqlString.Constant<hardDemand>
		{
			public hardDemand() : base(HardDemand) { }
		}
	}

	// Used in AR674000.rpx
	public class FilterItemByClass : IBqlTable
	{
		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }
		protected int? _ItemClassID;
		[PXDBInt]
		[PXUIField(DisplayName = "Item Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where2<Match<Current<AccessInfo.userName>>,
			And<Where<InventoryItem.itemClassID, Equal<Optional<FilterItemByClass.itemClassID>>, Or<Optional<FilterItemByClass.itemClassID>, IsNull>>>>>),
			typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
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

	}
}
