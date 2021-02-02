using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.CT
{
	using System;
	using PX.Data;
	using PX.Objects.CM;
	using PX.Objects.IN;
	using PX.Objects.CS;

	[System.SerializableAttribute()]
	[PXCacheName(Messages.ContractDetail)]
	[PXProjection(typeof(Select2<ContractDetail,
		InnerJoin<Contract, On<ContractDetail.contractID, Equal<Contract.contractID>, And<ContractDetail.revID, Equal<Contract.revID>>>,
		LeftJoin<ContractDetailExt, On<ContractDetailExt.contractID, Equal<ContractDetail.contractID>, And<ContractDetailExt.lineNbr, Equal<ContractDetail.lineNbr>, And<ContractDetailExt.revID, Equal<Contract.lastActiveRevID>>>>>>>), new Type[] { typeof(ContractDetail) },
		Persistent = true)]
	public partial class ContractDetail : PX.Data.IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Visible)]
		public bool? Selected
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

		#region ContractDetailID
		public abstract class contractDetailID : PX.Data.BQL.BqlInt.Field<contractDetailID> { }
		protected Int32? _ContractDetailID;
		[PXDBIdentity()]
		public virtual Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		protected Int32? _ContractID;
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(Contract.contractID))]
		[PXParent(typeof(Select<Contract, Where<Contract.contractID, Equal<Current<ContractDetail.contractID>>>>))]
		[PXParent(typeof(Select<ContractBillingSchedule, Where<ContractBillingSchedule.contractID, Equal<Current<ContractDetail.contractID>>>>))]
		public virtual Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected Int32? _LineNbr;
		[PXDBInt(IsKey = true)]
		[ContractLineNbr(typeof(Contract.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
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
		#region RevID
		public abstract class revID : PX.Data.BQL.BqlInt.Field<revID> { }
		[PXDBInt(MinValue = 1)]
		[PXDefault(typeof(Contract.revID), PersistingCheck = PXPersistingCheck.Null)]
		public virtual int? RevID { get; set; }
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt()]
		[PXUIField(DisplayName = "Non-Stock Item")]
		[PXDimensionSelector(InventoryAttribute.DimensionName, typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, Equal<False>, And<InventoryItem.itemStatus, NotEqual<InventoryItemStatus.unknown>, And<InventoryItem.isTemplate, Equal<False>, And<Match<Current<AccessInfo.userName>>>>>>>), typeof(InventoryItem.inventoryCD))]
		[PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
		#region ContractItemID
		public abstract class contractItemID : PX.Data.BQL.BqlInt.Field<contractItemID> { }
		protected Int32? _ContractItemID;
		[PXDBInt()]
		[PXDefault()]
		[PXDimensionSelector(ContractItemAttribute.DimensionName, typeof(Search<ContractItem.contractItemID>),
																	typeof(ContractItem.contractItemCD),
																	typeof(ContractItem.contractItemCD), typeof(ContractItem.descr), DescriptionField = typeof(ContractItem.descr))]
		[PXUIField(DisplayName = "Item Code")]
		public virtual Int32? ContractItemID
		{
			get
			{
				return this._ContractItemID;
			}
			set
			{
				this._ContractItemID = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        protected Int64? _CuryInfoID;
        [PXLong()]
        [CurrencyInfo(ModuleCode = CTPRType.ContractModule)]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBLocalizableString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.descr>))]
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
		#region ResetUsage
		public abstract class resetUsage : PX.Data.BQL.BqlString.Field<resetUsage> { }
		protected string _ResetUsage;
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage", Required = true)]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.List()]
		public virtual string ResetUsage
		{
			get
			{
				return this._ResetUsage;
			}
			set
			{
				this._ResetUsage = value;
			}
		}
		#endregion
		#region Included
		public abstract class included : PX.Data.BQL.BqlDecimal.Field<included> { }
		protected Decimal? _Included;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.Visible)]
		public virtual Decimal? Included
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
		#region Used
		public abstract class used : PX.Data.BQL.BqlDecimal.Field<used> { }
		protected Decimal? _Used;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? Used
		{
			get
			{
				return this._Used;
			}
			set
			{
				this._Used = value;
			}
		}
		#endregion
		#region UsedTotal
		public abstract class usedTotal : PX.Data.BQL.BqlDecimal.Field<usedTotal> { }
		protected Decimal? _UsedTotal;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? UsedTotal
		{
			get
			{
				return this._UsedTotal;
			}
			set
			{
				this._UsedTotal = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ContractDetail.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(ContractDetail.inventoryID))]
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
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty
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
		#region LastQty
		public abstract class lastQty : PX.Data.BQL.BqlDecimal.Field<lastQty> { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.qty))]
		public virtual Decimal? LastQty { get; set; }
		#endregion
		#region Change
		public abstract class change : PX.Data.BQL.BqlDecimal.Field<change> { }
		protected Decimal? _Change;
		[PXDecimal]
		[PXFormula(typeof(Sub<ContractDetail.qty, Switch<Case<Where<ContractDetail.lastQty, IsNotNull>, ContractDetail.lastQty>, int0>>))]
		[PXUIField(DisplayName = "Difference", Enabled = false)]
		public virtual Decimal? Change
		{
			get
			{
				return this._Change;
			}
			set
			{
				this._Change = value;
			}
		}
		#endregion
		#region Deposit
		public abstract class deposit : PX.Data.BQL.BqlBool.Field<deposit> { }
		[PXBool]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.deposit>))]
		public virtual bool? Deposit
		{
			get;
			set;
		}
		#endregion
		#region DepositAmt
		public abstract class depositAmt : PX.Data.BQL.BqlDecimal.Field<depositAmt> { }
		[PXDBCury(typeof(ContractDetail.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Amount", Enabled = false)]
		public virtual decimal? DepositAmt
		{
			get;
			set;
		}
		#endregion
		#region DepositUsed
		public abstract class depositUsed : PX.Data.BQL.BqlDecimal.Field<depositUsed> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Used", Enabled = false)]
		public virtual decimal? DepositUsed
		{
			get;
			set;
		}
		#endregion
		#region DepositUsedTotal
		public abstract class depositUsedTotal : PX.Data.BQL.BqlDecimal.Field<depositUsedTotal> { }
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Deposit Used Total", Enabled = false)]
		public virtual decimal? DepositUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region Recurring Included
		public abstract class recurringIncluded : PX.Data.BQL.BqlDecimal.Field<recurringIncluded> { }
		[PXUIField(DisplayName = "Included", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositAmt>, ContractDetail.qty>))]
		public virtual decimal? RecurringIncluded
		{
			get;
			set;
		}
		#endregion
		#region Recurring Used
		public abstract class recurringUsed : PX.Data.BQL.BqlDecimal.Field<recurringUsed> { }
		[PXUIField(DisplayName = "Unbilled", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositUsed>, ContractDetail.used>))]
		public virtual decimal? RecurringUsed
		{
			get;
			set;
		}
		#endregion
		#region Recurring UsedTotal
		public abstract class recurringUsedTotal : PX.Data.BQL.BqlDecimal.Field<recurringUsedTotal> { }
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetail.deposit, Equal<True>>, ContractDetail.depositUsedTotal>, ContractDetail.usedTotal>))]
		public virtual decimal? RecurringUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region LastBilledDate
		public abstract class lastBilledDate : PX.Data.BQL.BqlDateTime.Field<lastBilledDate> { }
		protected DateTime? _LastBilledDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Billed Date", Enabled = false)]
		public virtual DateTime? LastBilledDate
		{
			get
			{
				return this._LastBilledDate;
			}
			set
			{
				this._LastBilledDate = value;
			}
		}
		#endregion
		#region LastBilledQty
		public abstract class lastBilledQty : PX.Data.BQL.BqlDecimal.Field<lastBilledQty> { }
		protected Decimal? _LastBilledQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Billed Qty.", Enabled = false)]
		public virtual Decimal? LastBilledQty
		{
			get
			{
				return this._LastBilledQty;
			}
			set
			{
				this._LastBilledQty = value;
			}
		}
		#endregion
		#region BaseDiscountID
		public abstract class baseDiscountID : PX.Data.BQL.BqlString.Field<baseDiscountID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String BaseDiscountID
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountSeq
		public abstract class baseDiscountSeq : PX.Data.BQL.BqlString.Field<baseDiscountSeq> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String BaseDiscountSeq
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountID
		public abstract class recurringDiscountID : PX.Data.BQL.BqlString.Field<recurringDiscountID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String RecurringDiscountID
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountSeq
		public abstract class recurringDiscountSeq : PX.Data.BQL.BqlString.Field<recurringDiscountSeq> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String RecurringDiscountSeq
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountID
		public abstract class renewalDiscountID : PX.Data.BQL.BqlString.Field<renewalDiscountID> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String RenewalDiscountID
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountSeq
		public abstract class renewalDiscountSeq : PX.Data.BQL.BqlString.Field<renewalDiscountSeq> { }
		[PXDBString(10, IsUnicode = true)]
		public virtual String RenewalDiscountSeq
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountPct
		public abstract class baseDiscountPct : PX.Data.BQL.BqlDecimal.Field<baseDiscountPct> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Setup Discount,%", Enabled = false)]
		public decimal? BaseDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountPct
		public abstract class recurringDiscountPct : PX.Data.BQL.BqlDecimal.Field<recurringDiscountPct> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Recurring Discount,%", Enabled = false)]
		public decimal? RecurringDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountPct
		public abstract class renewalDiscountPct : PX.Data.BQL.BqlDecimal.Field<renewalDiscountPct> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Renewal Discount,%", Enabled = false)]
		public decimal? RenewalDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastBaseDiscountPct
		public abstract class lastBaseDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastBaseDiscountPct> { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.baseDiscountPct))]
		public virtual decimal? LastBaseDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRecurringDiscountPct
		public abstract class lastRecurringDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastRecurringDiscountPct> { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.recurringDiscountPct))]
		public virtual decimal? LastRecurringDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRenewalDiscountPct
		public abstract class lastRenewalDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastRenewalDiscountPct> { }
		[PXDBDecimal(BqlField = typeof(ContractDetailExt.renewalDiscountPct))]
		public virtual decimal? LastRenewalDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountAmt
		public abstract class baseDiscountAmt : PX.Data.BQL.BqlDecimal.Field<baseDiscountAmt> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Setup Discount", Enabled = false)]
		public decimal? BaseDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region RecurringDiscountAmt
		public abstract class recurringDiscountAmt : PX.Data.BQL.BqlDecimal.Field<recurringDiscountAmt> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Recurring Discount", Enabled = false)]
		public decimal? RecurringDiscountAmt
		{
			get;
			set;
		}
		#endregion
		#region RenewalDiscountAmt
		public abstract class renewalDiscountAmt : PX.Data.BQL.BqlDecimal.Field<renewalDiscountAmt> { }
		[PXDecimal(6)]
		[PXUIField(DisplayName = "Renewal Discount", Enabled = false)]
		public decimal? RenewalDiscountAmt
		{
			get;
			set;
		}
		#endregion

		#region BasePrice
		public abstract class basePrice : PX.Data.BQL.BqlDecimal.Field<basePrice> { }
		[PXUIField(DisplayName = "Price/Percent")]
		[PXDBDecimal(6)]
		[PXFormula(typeof(Switch<Case<Where<Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, IsNull>, decimal0>, Selector<ContractDetail.contractItemID, ContractItem.basePrice>>))]
		public decimal? BasePrice
		{
			get;
			set;
		}
		#endregion
		#region BasePriceOption
		public abstract class basePriceOption : PX.Data.BQL.BqlString.Field<basePriceOption> { }
		[PriceOption.List]
		[PXUIField(DisplayName = "Setup Pricing")]
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<Case<Where<Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, IsNull>, PriceOption.manually>, Selector<ContractDetail.contractItemID, ContractItem.basePriceOption>>))]
		public string BasePriceOption
		{
			get;
			set;
		}
		#endregion
		#region RenewalPrice
		public abstract class renewalPrice : PX.Data.BQL.BqlDecimal.Field<renewalPrice> { }
		[PXUIField(DisplayName = "Price/Percent")]
		[PXDBDecimal(6)]
		[PXFormula(typeof(Switch<Case<Where<Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, IsNull>, decimal0>, Selector<ContractDetail.contractItemID, ContractItem.renewalPrice>>))]
		public decimal? RenewalPrice
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceOption
		public abstract class renewalPriceOption : PX.Data.BQL.BqlString.Field<renewalPriceOption> { }
		[RenewalOption.List]
		[PXUIField(DisplayName = "Renewal Pricing")]
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Switch<Case<Where<Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, IsNull>, PriceOption.manually>, Selector<ContractDetail.contractItemID, ContractItem.renewalPriceOption>>))]
		public string RenewalPriceOption
		{
			get; 
			set;
		}
		#endregion
		#region FixedRecurringPrice
		public abstract class fixedRecurringPrice : PX.Data.BQL.BqlDecimal.Field<fixedRecurringPrice> { }
		[PXUIField(DisplayName = "Price/Percent")]
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.fixedRecurringPrice>))]
		public decimal? FixedRecurringPrice
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceOption
		public abstract class fixedRecurringPriceOption : PX.Data.BQL.BqlString.Field<fixedRecurringPriceOption> { }
		[RecurringOption.List]
		[PXUIField(DisplayName = "Fixed Recurring")]
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.fixedRecurringPriceOption>))]
		public string FixedRecurringPriceOption
		{
			get;
			set;
		}
		#endregion
		#region UsagePrice
		public abstract class usagePrice : PX.Data.BQL.BqlDecimal.Field<usagePrice> { }
		[PXUIField(DisplayName = "Price/Percent")]
		[PXDBDecimal(6)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.usagePrice>))]
		public decimal? UsagePrice
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceOption
		public abstract class usagePriceOption : PX.Data.BQL.BqlString.Field<usagePriceOption> { }
		[PriceOption.List]
		[PXUIField(DisplayName = "Usage Price")]
		[PXDBString(1, IsFixed = true)]
		[PXFormula(typeof(Selector<ContractDetail.contractItemID, ContractItem.usagePriceOption>))]
		public string UsagePriceOption
		{
			get;
			set;
		}
		#endregion
		#region BasePriceVal
		public abstract class basePriceVal : PX.Data.BQL.BqlDecimal.Field<basePriceVal> { }
		[PXDecimal(6)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetail.contractID, 
			ContractDetail.contractItemID, 
			ContractDetailType.ContractDetailSetup, 
			ContractDetail.basePriceOption, 
			Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, 
			ContractDetail.basePrice, 
			ContractDetail.basePriceVal, 
			ContractDetail.qty,
			Parent<Contract.startDate>>))]
		[PXUIField(DisplayName = "Setup Price")]
		public decimal? BasePriceVal
		{
			get;
			set;
		}
		#endregion
		#region BasePriceEditable
		public abstract class basePriceEditable : PX.Data.BQL.BqlBool.Field<basePriceEditable> { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.baseItemID>, IsNull, Or<ContractDetail.basePriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? BasePriceEditable
		{
			get;
			set;
		}
		#endregion
		#region IsBaseValid
		public abstract class isBaseValid : PX.Data.BQL.BqlBool.Field<isBaseValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractDetail.basePriceOption, Equal<PriceOption.manually>, And<ContractDetail.basePrice, Equal<decimal0>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsBaseValid
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceVal
		public abstract class renewalPriceVal : PX.Data.BQL.BqlDecimal.Field<renewalPriceVal> { }
		[PXDecimal(6)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetail.contractID, 
			ContractDetail.contractItemID, 
			ContractDetailType.ContractDetailRenewal, 
			ContractDetail.renewalPriceOption, 
			Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, 
			ContractDetail.renewalPrice, 
			ContractDetail.basePriceVal,
			ContractDetail.qty,
			Switch<
				Case<Where<Parent<Contract.status>, Equal<Contract.status.draft>,
					Or<Parent<Contract.status>, Equal<Contract.status.pendingActivation>>>,
					IsNull<Parent<Contract.activationDate>, Parent<Contract.startDate>>,
				Case<Where<Parent<Contract.status>, Equal<Contract.status.active>,
					Or<Parent<Contract.status>, Equal<Contract.status.inUpgrade>,
					Or<Parent<Contract.status>, Equal<Contract.status.expired>>>>,
					IsNull<Add<Parent<Contract.expireDate>, int1>, Current<AccessInfo.businessDate>>,
				Case<Where<Parent<Contract.status>, Equal<Contract.status.canceled>>,
					IsNull<Parent<Contract.terminationDate>, Current<AccessInfo.businessDate>>>>>,
				Current<AccessInfo.businessDate>>>))]
		[PXUIField(DisplayName = "Renewal Price")]
		public decimal? RenewalPriceVal
		{
			get;
			set;
		}
		#endregion
		#region RenewalPriceEditable
		public abstract class renewalPriceEditable : PX.Data.BQL.BqlBool.Field<renewalPriceEditable> { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.renewalItemID>, IsNull, Or<ContractDetail.renewalPriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? RenewalPriceEditable
		{
			get;
			set;
		}
		#endregion
		#region IsRenewalValid
		public abstract class isRenewalValid : PX.Data.BQL.BqlBool.Field<isRenewalValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractDetail.renewalPriceOption, Equal<PriceOption.manually>, And<ContractDetail.renewalPrice, Equal<decimal0>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsRenewalValid
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceVal
		public abstract class fixedRecurringPriceVal : PX.Data.BQL.BqlDecimal.Field<fixedRecurringPriceVal> { }
		[PXDecimal(6)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetail.contractID, 
			ContractDetail.contractItemID, 
			ContractDetailType.ContractDetail, 
			ContractDetail.fixedRecurringPriceOption, 
			Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, 
			ContractDetail.fixedRecurringPrice, 
			ContractDetail.basePriceVal,
			ContractDetail.qty,
			Switch<
				Case<Where<Parent<Contract.status>, Equal<Contract.status.draft>,
					Or<Parent<Contract.status>, Equal<Contract.status.pendingActivation>>>,
					IsNull<Parent<Contract.activationDate>, Parent<Contract.startDate>>,
				Case<Where<Parent<Contract.status>, Equal<Contract.status.active>,
					Or<Parent<Contract.status>, Equal<Contract.status.inUpgrade>>>,
					IsNull<Parent<ContractBillingSchedule.nextDate>, Current<AccessInfo.businessDate>>, 
				Case<Where<Parent<Contract.status>, Equal<Contract.status.expired>>, 
					IsNull<Parent<ContractBillingSchedule.nextDate>, Parent<Contract.expireDate>>,
				Case<Where<Parent<Contract.status>, Equal<Contract.status.canceled>>,
					IsNull<Parent<Contract.terminationDate>, Current<AccessInfo.businessDate>>>>>>,
				Current<AccessInfo.businessDate>>>))]
		[PXUIField(DisplayName = "Recurring Price")]
		public decimal? FixedRecurringPriceVal
		{
			get;
			set;
		}
		#endregion
		#region FixedRecurringPriceEditable
		public abstract class fixedRecurringPriceEditable : PX.Data.BQL.BqlBool.Field<fixedRecurringPriceEditable> { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull, Or<ContractDetail.fixedRecurringPriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? FixedRecurringPriceEditable
		{
			get;
			set;
		}
		#endregion
		#region IsFixedRecurringValid
		public abstract class isFixedRecurringValid : PX.Data.BQL.BqlBool.Field<isFixedRecurringValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractDetail.fixedRecurringPriceOption, Equal<PriceOption.manually>, And<ContractDetail.fixedRecurringPrice, Equal<decimal0>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsFixedRecurringValid
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceVal
		public abstract class usagePriceVal : PX.Data.BQL.BqlDecimal.Field<usagePriceVal> { }
		[PXDecimal(6)]
		[PXFormula(typeof(GetItemPriceValue<
			ContractDetail.contractID, 
			ContractDetail.contractItemID, 
			ContractDetailType.ContractDetailUsagePrice, 
			ContractDetail.usagePriceOption, 
			Selector<ContractDetail.contractItemID, 
			ContractItem.recurringItemID>, 
			ContractDetail.usagePrice, 
			ContractDetail.basePriceVal, 
			ContractDetail.qty,
			Parent<Contract.activationDate>>))]
		[PXUIField(DisplayName = Messages.ExtraUsagePrice)]
		public decimal? UsagePriceVal
		{
			get;
			set;
		}
		#endregion
		#region UsagePriceEditable
		public abstract class usagePriceEditable : PX.Data.BQL.BqlBool.Field<usagePriceEditable> { }
		[PXBool()]
		[PXFormula(typeof(
			Switch<
				Case<Where<Selector<ContractDetail.contractItemID, ContractItem.recurringItemID>, IsNull, Or<ContractDetail.usagePriceOption, NotEqual<PriceOption.manually>>>, False>,
				True>))]
		public bool? UsagePriceEditable
		{
			get;
			set;
		}
		#endregion
		#region IsUsageValid
		public abstract class isUsageValid : PX.Data.BQL.BqlBool.Field<isUsageValid> { }
		[PXBool]
		[PXDBCalced(typeof(Switch<Case<Where<ContractDetail.usagePriceOption, Equal<PriceOption.manually>, And<ContractDetail.usagePrice, Equal<decimal0>>>, False>, True>), typeof(Boolean))]
		public virtual bool? IsUsageValid
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote()]
		public virtual Guid? NoteID
		{ get; set; }
		#endregion
		#region WarningAmountForDeposit
		public abstract class warningAmountForDeposit : PX.Data.BQL.BqlBool.Field<warningAmountForDeposit> { }
		protected bool? _WarningAmountForDeposit;
		[PXBool()]
		public virtual bool? WarningAmountForDeposit
		{
			get { return _WarningAmountForDeposit; }
			set { _WarningAmountForDeposit = value; }
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

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select<ContractDetailExt>), Persistent = true)]
	[PXBreakInheritance()]
	[PXHidden]
	public partial class ContractDetailExt : ContractDetail
	{
		#region ContractDetailID
		new public abstract class contractDetailID : PX.Data.BQL.BqlInt.Field<contractDetailID> { }
		new protected Int32? _ContractDetailID;
		[PXDBIdentity(IsKey = true)]
		public override Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		new public abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		new protected Int32? _ContractID;
		#endregion
		#region LineNbr
		new public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		new protected Int32? _LineNbr;
		[PXDBInt()]
		[PXLineNbr(typeof(Contract.lineCtr))]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		public override Int32? LineNbr
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
		#region RevID
		new public abstract class revID : PX.Data.BQL.BqlInt.Field<revID> { }
		[PXDBInt(MinValue = 1, IsKey = true)]
		[PXDefault(typeof(Contract.revID), PersistingCheck = PXPersistingCheck.Null)]
		public override int? RevID { get; set; }
		#endregion
		#region ContractItemID
		new public abstract class contractItemID : PX.Data.BQL.BqlInt.Field<contractItemID> { }
		#endregion
		#region Qty
		new public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region Used
		new public abstract class used : PX.Data.BQL.BqlDecimal.Field<used> { }
		#endregion
		#region UsedTotal
		new public abstract class usedTotal : PX.Data.BQL.BqlDecimal.Field<usedTotal> { }
		new protected Decimal? _UsedTotal;
		#endregion
		#region LastQty
		new public abstract class lastQty : PX.Data.BQL.BqlDecimal.Field<lastQty> { }
		[PXQuantity()]
		public override Decimal? LastQty { get; set; }
		#endregion
		#region Deposit
		new public abstract class deposit : PX.Data.BQL.BqlBool.Field<deposit> { }
		[PXBool]
		[PXFormula(typeof(Selector<ContractDetailExt.contractItemID, ContractItem.deposit>))]
		public override bool? Deposit
		{
			get;
			set;
		}
		#endregion
		#region DepositAmt
		new public abstract class depositAmt : PX.Data.BQL.BqlDecimal.Field<depositAmt> { }
		#endregion
		#region DepositUsed
		new public abstract class depositUsed : PX.Data.BQL.BqlDecimal.Field<depositUsed> { }
		#endregion
		#region DepositUsedTotal
		new public abstract class depositUsedTotal : PX.Data.BQL.BqlDecimal.Field<depositUsedTotal> { }
		#endregion
		#region Recurring Included
		new public abstract class recurringIncluded : PX.Data.BQL.BqlDecimal.Field<recurringIncluded> { }
		[PXUIField(DisplayName = "Included", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositAmt>, ContractDetailExt.qty>))]
		public override decimal? RecurringIncluded
		{
			get;
			set;
		}
		#endregion
		#region Recurring Used
		new public abstract class recurringUsed : PX.Data.BQL.BqlDecimal.Field<recurringUsed> { }
		[PXUIField(DisplayName = "Used", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositUsed>, ContractDetailExt.used>))]
		public override decimal? RecurringUsed
		{
			get;
			set;
		}
		#endregion
		#region Recurring UsedTotal
		new public abstract class recurringUsedTotal : PX.Data.BQL.BqlDecimal.Field<recurringUsedTotal> { }
		[PXUIField(DisplayName = "Used Total", Enabled = false)]
		[PXDecimal]
		[PXFormula(typeof(Switch<Case<Where<ContractDetailExt.deposit, Equal<True>>, ContractDetailExt.depositUsedTotal>, ContractDetailExt.usedTotal>))]
		public override decimal? RecurringUsedTotal
		{
			get;
			set;
		}
		#endregion
		#region BaseDiscountPct
		new public abstract class baseDiscountPct : PX.Data.BQL.BqlDecimal.Field<baseDiscountPct> { }
		#endregion
		#region RecurringDiscountPct
		new public abstract class recurringDiscountPct : PX.Data.BQL.BqlDecimal.Field<recurringDiscountPct> { }
		#endregion
		#region RenewalDiscountPct
		new public abstract class renewalDiscountPct : PX.Data.BQL.BqlDecimal.Field<renewalDiscountPct> { }
		#endregion
		#region LastBaseDiscountPct
		new public abstract class lastBaseDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastBaseDiscountPct> { }
		[PXDecimal()]
		public override decimal? LastBaseDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRecurringDiscountPct
		new public abstract class lastRecurringDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastRecurringDiscountPct> { }
		[PXDecimal()]
		public override decimal? LastRecurringDiscountPct
		{
			get;
			set;
		}
		#endregion
		#region LastRenewalDiscountPct
		new public abstract class lastRenewalDiscountPct : PX.Data.BQL.BqlDecimal.Field<lastRenewalDiscountPct> { }
		[PXDecimal()]
		public override decimal? LastRenewalDiscountPct
		{
			get;
			set;
		}
		#endregion
	}


	[System.SerializableAttribute()]
	[ContractDetailAccumAttribute]
	[PXBreakInheritance()]
	public partial class ContractDetailAcum : ContractDetail
	{
		#region ContractDetailID
		public new abstract class contractDetailID : PX.Data.BQL.BqlInt.Field<contractDetailID> { }
		[PXDBInt(IsKey = true)]
		public override Int32? ContractDetailID
		{
			get
			{
				return this._ContractDetailID;
			}
			set
			{
				this._ContractDetailID = value;
			}
		}
		#endregion
		#region ContractID
		public new abstract class contractID : PX.Data.BQL.BqlInt.Field<contractID> { }
		[PXDBInt()]
		public override Int32? ContractID
		{
			get
			{
				return this._ContractID;
			}
			set
			{
				this._ContractID = value;
			}
		}
		#endregion
		#region LineNbr
		public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt()]
		public override Int32? LineNbr
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
		#region RevID
		public new abstract class revID : PX.Data.BQL.BqlInt.Field<revID> { }
		[PXDBInt()]
		public override int? RevID { get; set; }
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[PXDBInt()]
		public override Int32? InventoryID
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
		#region Description
		public new abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public override String Description
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
		#region ResetUsage
		public new abstract class resetUsage : PX.Data.BQL.BqlString.Field<resetUsage> { }
		[PXDefault(ResetUsageOption.Never)]
		[PXUIField(DisplayName = "Reset Usage")]
		[PXDBString(1, IsFixed = true)]
		[ResetUsageOption.List()]
		public override string ResetUsage
		{
			get
			{
				return this._ResetUsage;
			}
			set
			{
				this._ResetUsage = value;
			}
		}
		#endregion
		#region Included
		public new abstract class included : PX.Data.BQL.BqlDecimal.Field<included> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Included", Visibility = PXUIVisibility.Visible)]
		public override Decimal? Included
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
		#region Used
		public new abstract class used : PX.Data.BQL.BqlDecimal.Field<used> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? Used
		{
			get
			{
				return this._Used;
			}
			set
			{
				this._Used = value;
			}
		}
		#endregion
		#region UsedTotal
		public new abstract class usedTotal : PX.Data.BQL.BqlDecimal.Field<usedTotal> { }
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Used Total", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public override Decimal? UsedTotal
		{
			get
			{
				return this._UsedTotal;
			}
			set
			{
				this._UsedTotal = value;
			}
		}
		#endregion
		#region UOM
		public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		[PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<ContractDetail.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[INUnit(typeof(ContractDetail.inventoryID))]
		public override String UOM
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
		#region LastBilledDate
		public new abstract class lastBilledDate : PX.Data.BQL.BqlDateTime.Field<lastBilledDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Last Billed Date", Enabled = false)]
		public override DateTime? LastBilledDate
		{
			get
			{
				return this._LastBilledDate;
			}
			set
			{
				this._LastBilledDate = value;
			}
		}
		#endregion
		#region LastBilledQty
		public new abstract class lastBilledQty : PX.Data.BQL.BqlDecimal.Field<lastBilledQty> { }
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Last Billed Qty.", Enabled = false)]
		public override Decimal? LastBilledQty
		{
			get
			{
				return this._LastBilledQty;
			}
			set
			{
				this._LastBilledQty = value;
			}
		}
		#endregion

		#region System Columns
		#region tstamp
		public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public override Byte[] tstamp
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
		public new abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public override Guid? CreatedByID
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
		public new abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public override String CreatedByScreenID
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
		public new abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public override DateTime? CreatedDateTime
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
		public new abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public override Guid? LastModifiedByID
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
		public new abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public override String LastModifiedByScreenID
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
		public new abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public override DateTime? LastModifiedDateTime
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

	public static class ContractDetailType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Setup, Renewal, Billing, UsagePrice, Reinstallment },
				new string[] { Messages.Setup, Messages.Renewal, Messages.Billing, Messages.UsagePrice, Messages.Reinstallment }) { ; }
		}
		public const string Setup = "S";
		public const string Renewal = "R";
		public const string Billing = "B";
		public const string UsagePrice = "U";
		public const string Reinstallment = "I";

		public class ContractDetailSetup : PX.Data.BQL.BqlString.Constant<ContractDetailSetup>
		{
			public ContractDetailSetup() : base(ContractDetailType.Setup) { ;}
		}
		public class ContractDetailRenewal : PX.Data.BQL.BqlString.Constant<ContractDetailRenewal>
		{
			public ContractDetailRenewal() : base(ContractDetailType.Renewal) { ;}
		}
		public partial class ContractDetail : PX.Data.BQL.BqlString.Constant<ContractDetail>
		{
			public ContractDetail() : base(ContractDetailType.Billing) { ;}
		}
		public class ContractDetailUsagePrice : PX.Data.BQL.BqlString.Constant<ContractDetailUsagePrice>
		{
			public ContractDetailUsagePrice() : base(ContractDetailType.UsagePrice) { ;}
		}
		public class ContractDetailReinstallment : PX.Data.BQL.BqlString.Constant<ContractDetailReinstallment>
		{
			public ContractDetailReinstallment() : base(ContractDetailType.Reinstallment) { ;}
		}
	}

	public static class ResetUsageOption
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Never, OnBilling, OnRenewal },
				new string[] { Messages.Never, Messages.OnBilling, Messages.OnRenewal }) { ; }
		}

		public class ListForProjectAttribute : PXStringListAttribute
		{
			public ListForProjectAttribute()
				: base(
				new string[] { Never, OnBilling },
				new string[] { Messages.Never, Messages.OnBilling }) { ; }
		}

		public const string Never = "N";
		public const string OnBilling = "B";
		public const string OnRenewal = "R";

		public class onBilling : PX.Data.BQL.BqlString.Constant<onBilling>
		{
			public onBilling() : base(ResetUsageOption.OnBilling) { ;}
		}

		public class onRenewal : PX.Data.BQL.BqlString.Constant<onRenewal>
		{
			public onRenewal() : base(ResetUsageOption.OnRenewal) { ;}
		}
	}

	public static class ContractDetailTypeSetupAndRenewal
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ContractDetailType.Setup, ContractDetailType.Renewal, ContractDetailType.Reinstallment },
				new string[] { Messages.Setup, Messages.Renewal, Messages.Reinstallment }) { ; }
		}
	}
}
