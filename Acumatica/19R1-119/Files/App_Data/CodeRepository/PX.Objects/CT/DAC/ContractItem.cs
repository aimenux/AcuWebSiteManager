using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.CT
{
	[PXPrimaryGraph(typeof(ContractItemMaint))]
	[Serializable]
	[PXCacheName(Messages.ContractItem)]
	public class ContractItem : IBqlTable
	{
		#region ContractItemID
		public abstract class contractItemID : PX.Data.BQL.BqlInt.Field<contractItemID> { }
		[PXDBIdentity]
		[PXUIField(Visible=false, Visibility=PXUIVisibility.Invisible)]
		public int? ContractItemID
		{
			get;
			set;
		}
		#endregion
		#region ContractItemCD
		public abstract class contractItemCD : PX.Data.BQL.BqlString.Field<contractItemCD> { }
		[ContractItem(IsKey=true, Visibility=PXUIVisibility.SelectorVisible)]
		public string ContractItemCD
		{
			get;
			set;
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Required = true)]
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
		#region DefaultQty
		public abstract class defaultQty : PX.Data.BQL.BqlDecimal.Field<defaultQty> { }
		[PXDBQuantity(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Validate<ContractItem.maxQty, ContractItem.minQty>))]
		[PXUIField(DisplayName = "Default Quantity")]
		public decimal? DefaultQty
		{
			get;
			set;
		}
		#endregion
		#region MinQty
		public abstract class minQty : PX.Data.BQL.BqlDecimal.Field<minQty> { }
		[PXDBQuantity(MinValue=0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Minimum Allowed Quantity")]
		public decimal? MinQty
		{
			get;
			set;
		}
		#endregion
		#region MaxQty
		public abstract class maxQty : PX.Data.BQL.BqlDecimal.Field<maxQty> { }
		[PXDBQuantity(MinValue=0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Maximum Allowed Quantity")]
		public decimal? MaxQty
		{
			get;
			set;
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDefault()]
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Search<Currency.curyID>))]
		[PXUIField(DisplayName = "Currency ID")]
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
		#region BaseItemID
		public abstract class baseItemID : PX.Data.BQL.BqlInt.Field<baseItemID> { }
		[ContractInventoryItem(DisplayName = Messages.SetupItem)]
		[PXForeignReference(typeof(Field<baseItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public int? BaseItemID { get; set; }
		#endregion
		#region BasePriceOption
		public abstract class basePriceOption : PX.Data.BQL.BqlString.Field<basePriceOption> { }
		[PXDBString(1, IsFixed = true)]
		[PriceOption.List]
		[PXUIField(DisplayName = "Setup Pricing")]
		[PXDefault(PriceOption.ItemPrice)]
		public string BasePriceOption
		{
			get;
			set;
		}
		#endregion
		#region BasePrice
		public abstract class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Item Price/Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public decimal? BasePrice
		{
			get;
			set;
		}
		#endregion
		#region ProrateSetup
		public abstract class prorateSetup : PX.Data.BQL.BqlBool.Field<prorateSetup> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Prorate Setup")]
		public bool? ProrateSetup
		{
			get;
			set;
		}
		#endregion
		#region RetainRate
		public abstract class retainRate : PX.Data.BQL.BqlDecimal.Field<retainRate> { }
		[PXDBDecimal(2)]
		[PXUIField(DisplayName = "Retain Rate")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXFormula(typeof(Switch<Case<Where<deposit, NotEqual<True>>, decimal0>, retainRate>))]
		[PXUIEnabled(typeof(Where<deposit, Equal<True>>))]
		public decimal? RetainRate { get; set; }
		#endregion
		#region Refundable
		public abstract class refundable : PX.Data.BQL.BqlBool.Field<refundable> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIEnabled(typeof(Where<ContractItem.baseItemID, IsNotNull, Or<ContractItem.renewalItemID, IsNotNull>>))]
		[PXUIField(DisplayName = "Refundable")]
		public bool? Refundable
		{
			get;
			set;
		}
		#endregion
		#region Deposit
		public abstract class deposit : PX.Data.BQL.BqlBool.Field<deposit> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Deposit")]
		public bool? Deposit
		{
			get;
			set;
		}
		#endregion
		#region CollectRenewFeeOnActivation
		public abstract class collectRenewFeeOnActivation : PX.Data.BQL.BqlBool.Field<collectRenewFeeOnActivation> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Collect Renewal Fee on Activation")]
		public bool? CollectRenewFeeOnActivation { get; set; }
		#endregion
		#region RenewalItemID
		public abstract class renewalItemID : PX.Data.BQL.BqlInt.Field<renewalItemID> { }
		[ContractInventoryItem(DisplayName = Messages.RenewalItem)]
		[PXForeignReference(typeof(Field<renewalItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public int? RenewalItemID { get; set; }
		#endregion
		#region RenewalPriceOption
		public abstract class renewalPriceOption : PX.Data.BQL.BqlString.Field<renewalPriceOption> { }
		[PXDBString(1, IsFixed = true)]
		[RenewalOption.List]
		[PXDefault(PriceOption.BasePercent)]
		[PXUIField(DisplayName = "Renewal Pricing")]
		public string RenewalPriceOption
		{
			get;
			set;
		}
		#endregion
		#region RenewalPrice
		public abstract class renewalPrice : PX.Data.BQL.BqlDecimal.Field<renewalPrice> { }
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Item Price/Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public decimal? RenewalPrice
		{
			get;
			set;
		}
		#endregion
		#region RecurringType
		public abstract class recurringType : PX.Data.BQL.BqlString.Field<recurringType> { }
		[PXDBString(1, IsFixed = true)]
		[RecurringOption.List]
		[PXDefault(RecurringOption.None)]
		[PXUIField(DisplayName = "Billing Type")]
		public string RecurringType
		{
			get;
			set;
		}
		#endregion
		#region RecurringTypeForDeposits
		public abstract class recurringTypeForDeposits : PX.Data.BQL.BqlString.Field<recurringTypeForDeposits> { }
		protected String _RecurringTypeForDeposits;
		[PXString(1, IsFixed = true)]
		[RecurringOption.ListForDeposits]
		[PXUIField(DisplayName = "Billing Type")]
		public string RecurringTypeForDeposits
		{            
			get
			{
				return _RecurringTypeForDeposits;
			}
			set
			{
				_RecurringTypeForDeposits = value;
			}
		}
		#endregion
		#region UOMForDeposits
		public abstract class uOMForDeposits : PX.Data.BQL.BqlString.Field<uOMForDeposits> { }
		protected String _UOMForDeposits;
		[PXUIField(DisplayName = "UOM")]
		[PXString(10, IsFixed = true)]
		public virtual String UOMForDeposits
		{
			get
			{
				return this._UOMForDeposits;
			}
			set
			{
				this._UOMForDeposits = value;
			}
		}
		#endregion
		#region RecurringItemID
		public abstract class recurringItemID : PX.Data.BQL.BqlInt.Field<recurringItemID> { }
		[PXDefault()]
		[ContractInventoryItem(DisplayName = Messages.RecurringItem)]
		[PXForeignReference(typeof(Field<recurringItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public int? RecurringItemID { get; set; }
		#endregion
		#region ResetUsageOnBilling
		public abstract class resetUsageOnBilling : PX.Data.BQL.BqlBool.Field<resetUsageOnBilling> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Reset Usage on Billing")]
		[PXDefault(false)]
		public bool? ResetUsageOnBilling { get; set; }
		#endregion
		#region FixedRecurringPriceOption
		public abstract class fixedRecurringPriceOption : PX.Data.BQL.BqlString.Field<fixedRecurringPriceOption> { }
		[PXDBString(1, IsFixed = true)]
		[FixedRecurringOption.List]
		[PXDefault(PriceOption.ItemPercent)]
		[PXUIField(DisplayName = "Recurring Pricing")]
		public string FixedRecurringPriceOption
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPrice
		public abstract class fixedRecurringPrice : PX.Data.BQL.BqlDecimal.Field<fixedRecurringPrice> { }
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Item Price/Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public decimal? FixedRecurringPrice
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceOption
		public abstract class usagePriceOption : PX.Data.BQL.BqlString.Field<usagePriceOption> { }
		[PXDBString(1, IsFixed = true)]
		[UsageOption.List]
		[PXDefault(PriceOption.ItemPrice)]
		[PXUIField(DisplayName = "Extra Usage Pricing")]
		public string UsagePriceOption
		{
			get;
			set;
		}
		#endregion
		#region UsagePrice
		public abstract class usagePrice : PX.Data.BQL.BqlDecimal.Field<usagePrice> { }
		[PXDBDecimal(6)]
		[PXUIField(DisplayName = "Item Price/Percent")]
		[PXDefault(TypeCode.Decimal, "100.0")]
		public decimal? UsagePrice
		{
			get;
			set;
		}
		#endregion
		#region DiscontinueAfter
		public abstract class discontinueAfter : PX.Data.BQL.BqlDateTime.Field<discontinueAfter> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Discontinue After")]
		public DateTime? DiscontinueAfter
		{
			get;
			set;
		}
		#endregion
		#region ReplacementItemID
		public abstract class replacementItemID : PX.Data.BQL.BqlInt.Field<replacementItemID> { }
		[PXDBInt()]
		[PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID, Where<ContractItem.contractItemID, NotEqual<Current<ContractItem.contractItemID>>>>), typeof(ContractItem.contractItemCD))]
		[PXRestrictor(typeof(Where<ContractItem.deposit, Equal<Current<ContractItem.deposit>>>), Messages.ContractDoesNotMatchDeposit)]
		[PXUIField(DisplayName = "Replacement Item")]
		public int? ReplacementItemID
		{
			get;
			set;
		}
		#endregion
		#region DepositItemID
		public abstract class depositItemID : PX.Data.BQL.BqlInt.Field<depositItemID> { }
		[PXDBInt()]
		[PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID, Where<ContractItem.contractItemID, NotEqual<Current<ContractItem.contractItemID>>>>), typeof(ContractItem.contractItemCD))]
		[PXRestrictor(typeof(Where<ContractItem.deposit, Equal<True>>), Messages.ContractIsNotDeposit)]
		[PXUIField(DisplayName = "Deposit Item")]
		public int? DepositItemID
		{
			get;
			set;
		}
		#endregion

		#region BasePriceVal
		public abstract class basePriceVal : PX.Data.BQL.BqlDecimal.Field<basePriceVal> { }
		protected decimal? _BasePriceVal;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Setup Price", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<ContractItem.baseItemID, IsNotNull>, 
				Switch<
					Case<Where<ContractItem.basePriceOption, Equal<PriceOption.itemPrice>>, IsNull<NullIf<Selector<ContractItem.baseItemID, ARSalesPrice.salesPrice>, decimal0>, 
						Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.baseItemID, InventoryItem.basePrice>, decimal0>>, Null>>,
					Case<Where<ContractItem.basePriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractItem.basePrice, IsNull<NullIf<Selector<ContractItem.baseItemID, ARSalesPrice.salesPrice>, decimal0>, Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.baseItemID, InventoryItem.basePrice>, decimal0>>, Null>>>, decimal100>>>,
					ContractItem.basePrice>>,
			decimal0>))]
		public decimal? BasePriceVal
		{
			get
			{
				return this._BasePriceVal;
			}
			set
			{
				this._BasePriceVal = value;
			}
		}
		#endregion
		#region RenewalPriceVal
		public abstract class renewalPriceVal : PX.Data.BQL.BqlDecimal.Field<renewalPriceVal> { }
		protected decimal? _RenewalPriceVal;
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Renewal Price", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<ContractItem.renewalItemID, IsNotNull>,
				Switch<
					Case<Where<ContractItem.renewalPriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractItem.renewalPrice, ContractItem.basePriceVal>, decimal100>,
					Case<Where<ContractItem.renewalPriceOption, Equal<PriceOption.itemPrice>>, IsNull<NullIf<Selector<ContractItem.renewalItemID, ARSalesPrice.salesPrice>, decimal0>, 
						Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.renewalItemID, InventoryItem.basePrice>, decimal0>>, Null>>,
					Case<Where<ContractItem.renewalPriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractItem.renewalPrice, IsNull<NullIf<Selector<ContractItem.renewalItemID, ARSalesPrice.salesPrice>, decimal0>, Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.renewalItemID, InventoryItem.basePrice>, decimal0>>, Null>>>, decimal100>>>>,
					ContractItem.renewalPrice>>,
			decimal0>))]
		public decimal? RenewalPriceVal
		{
			get
			{
				return this._RenewalPriceVal;
			}
			set
			{
				this._RenewalPriceVal = value;
			}
		}
		#endregion
		#region FixedRecurringPriceVal
		public abstract class fixedRecurringPriceVal : PX.Data.BQL.BqlDecimal.Field<fixedRecurringPriceVal> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Recurring Price", Enabled = false)]
		[PXFormula(typeof(Switch<
			Case<Where<ContractItem.recurringItemID, IsNotNull>,
				Switch<
					Case<Where<ContractItem.fixedRecurringPriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractItem.fixedRecurringPrice, ContractItem.basePriceVal>, decimal100>,
					Case<Where<ContractItem.fixedRecurringPriceOption, Equal<PriceOption.itemPrice>>, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>, decimal0>, 
						Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.recurringItemID, InventoryItem.basePrice>, decimal0>>, Null>>,
					Case<Where<ContractItem.fixedRecurringPriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractItem.fixedRecurringPrice, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>, decimal0>, Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.recurringItemID, InventoryItem.basePrice>, decimal0>>, Null>>>, decimal100>>>>,
					ContractItem.fixedRecurringPrice>>,
			decimal0>))]
		public decimal? FixedRecurringPriceVal
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceVal
		public abstract class usagePriceVal : PX.Data.BQL.BqlDecimal.Field<usagePriceVal> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Extra Usage Price", Enabled = false)]
		[PXFormula(typeof(Switch<
				Case<Where<ContractItem.recurringItemID, IsNotNull>,
					Switch<
						Case<Where<ContractItem.usagePriceOption, Equal<PriceOption.basePercent>>, Div<Mult<ContractItem.usagePrice, ContractItem.basePriceVal>, decimal100>,
						Case<Where<ContractItem.usagePriceOption, Equal<PriceOption.itemPrice>>, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>, decimal0>, 
							Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.recurringItemID, InventoryItem.basePrice>, decimal0>>, Null>>,
						Case<Where<ContractItem.usagePriceOption, Equal<PriceOption.itemPercent>>, Div<Mult<ContractItem.usagePrice, IsNull<NullIf<Selector<ContractItem.recurringItemID, ARSalesPrice.salesPrice>, decimal0>, Switch<Case<Where<ContractItem.curyID, Equal<Current<Company.baseCuryID>>>, NullIf<Selector<ContractItem.recurringItemID, InventoryItem.basePrice>, decimal0>>, Null>>>, decimal100>>>>,
						ContractItem.usagePrice>>,
				decimal0>))]
		public decimal? UsagePriceVal
		{
			get;
			set;
		}
		#endregion
		
		#region IsBaseValid
		public abstract class isBaseValid : PX.Data.BQL.BqlBool.Field<isBaseValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractItem.baseItemID, IsNotNull, And<ContractItem.basePriceOption, Equal<PriceOption.manually>, And<ContractItem.basePrice, Equal<decimal0>>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsBaseValid
		{
			get;
			set;
		}
		#endregion
		#region IsRenewalValid
		public abstract class isRenewalValid : PX.Data.BQL.BqlBool.Field<isRenewalValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractItem.renewalItemID, IsNotNull, And<ContractItem.renewalPriceOption, Equal<PriceOption.manually>, And<ContractItem.renewalPrice, Equal<decimal0>>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsRenewalValid
		{
			get;
			set;
		}
		#endregion
		#region IsFixedRecurringValid
		public abstract class isFixedRecurringValid : PX.Data.BQL.BqlBool.Field<isFixedRecurringValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractItem.recurringItemID, IsNotNull, And<ContractItem.fixedRecurringPriceOption, Equal<PriceOption.manually>, And<ContractItem.fixedRecurringPrice, Equal<decimal0>>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsFixedRecurringValid
		{
			get;
			set;
		}
		#endregion
		#region IsUsageValid
		public abstract class isUsageValid : PX.Data.BQL.BqlBool.Field<isUsageValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractItem.recurringItemID, IsNotNull, And<ContractItem.usagePriceOption, Equal<PriceOption.manually>, And<ContractItem.usagePrice, Equal<decimal0>>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsUsageValid
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(contractItemCD))]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedDateTime]
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

	public static class PriceOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ItemPrice, ItemPercent,  Manually },
				new string[] { Messages.ItemPrice, Messages.PercentOfItemPrice, Messages.EnterManually }) { ; }
		}

		public const string ItemPrice = "I";
		public const string ItemPercent = "P";
		public const string Manually = "M";
		public const string BasePercent = "B";


		public class itemPrice : PX.Data.BQL.BqlString.Constant<itemPrice>
		{
			public itemPrice() : base(ItemPrice) { ;}
		}

		public class itemPercent : PX.Data.BQL.BqlString.Constant<itemPercent>
		{
			public itemPercent() : base(ItemPercent) { ;}
		}

		public class manually : PX.Data.BQL.BqlString.Constant<manually>
		{
			public manually() : base(Manually) { ;}
		}

		public class basePercent : PX.Data.BQL.BqlString.Constant<basePercent>
		{
			public basePercent() : base(BasePercent) { ;}
		}
	}

	public static class RenewalOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PriceOption.ItemPrice, PriceOption.ItemPercent, PriceOption.BasePercent, PriceOption.Manually },
				new string[] { Messages.ItemPrice, Messages.PercentOfItemPrice, Messages.PercentOfBasePrice, Messages.EnterManually }) { ; }
		}
	}

	public static class RecurringOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { None, Prepay, Usage },
				new string[] { Messages.None, Messages.Prepay, Messages.Usage }) { ; }
		}
		public class ListForDepositsAttribute : PXStringListAttribute
		{
			public ListForDepositsAttribute()
				: base(
				new string[] { None, Prepay, Usage,Deposits },
				new string[] { Messages.None, Messages.Prepay, Messages.Usage,Messages.Deposit }) { ; }
		}

		public const string None = "N";
		public const string Prepay = "P";
		public const string Usage = "U";
		public const string Deposits = "D";

		public class none : PX.Data.BQL.BqlString.Constant<none>
		{
			public none() : base(RecurringOption.None) { ;}
		}

		public class prepay : PX.Data.BQL.BqlString.Constant<prepay>
		{
			public prepay() : base(RecurringOption.Prepay) { ;}
		}

		public class usage : PX.Data.BQL.BqlString.Constant<usage>
		{
			public usage() : base(RecurringOption.Usage) { ;}
		}
		public class deposits : PX.Data.BQL.BqlString.Constant<deposits>
		{
			public deposits() : base(RecurringOption.Deposits) { ;}
		}
	}

	public static class FixedRecurringOption 
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PriceOption.ItemPrice, PriceOption.ItemPercent, PriceOption.BasePercent, PriceOption.Manually },
				new string[] { Messages.ItemPrice, Messages.PercentOfItemPrice, Messages.PercentOfBasePrice, Messages.EnterManually }) { ; }
		}


	}

	public static class UsageOption 
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PriceOption.ItemPrice, PriceOption.ItemPercent, PriceOption.BasePercent, PriceOption.Manually },
				new string[] { Messages.ItemPrice, Messages.PercentOfItemPrice, Messages.PercentOfBasePrice, Messages.EnterManually }) { ; }
		}
	}
}
