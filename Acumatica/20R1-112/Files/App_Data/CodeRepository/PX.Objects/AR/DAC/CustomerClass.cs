using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.EP;

using PX.SM;

using PX.Objects.Common;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	public static class CreditRuleTypes
	{
		public const string CS_DAYS_PAST_DUE = "D";
		public const string CS_CREDIT_LIMIT = "C";
		public const string CS_BOTH = "B";
		public const string CS_NO_CHECKING = "N";
	}

    /// <summary>
    /// Fixed List Selector. Defines a name-value pair list of a possible Statement types <br/>
    /// compatible with <see cref="ARStatementType"/>.<br/> 
    /// </summary>
	public class StatementTypeAttribute: PXStringListAttribute
	{
		public StatementTypeAttribute() : base(
			new string[] { ARStatementType.OpenItem, ARStatementType.BalanceBroughtForward }, 
			new string[] { Messages.OpenItem, Messages.BalanceBroughtForward })
		{ }
	}

    /// <summary>
    /// Fixed List Selector. Defines a name-value pair list of a possible CreditRules <br/>
    /// compatible with <see cref="CreditRuleTypes"/><br/>    
    /// </summary>	
	public class CreditRuleAttribute: PXStringListAttribute
	{
		public CreditRuleAttribute() : base(new string[] { CreditRuleTypes.CS_DAYS_PAST_DUE, CreditRuleTypes.CS_CREDIT_LIMIT, CreditRuleTypes.CS_BOTH, CreditRuleTypes.CS_NO_CHECKING }, new string[] { "Days Past Due", "Credit Limit", "Limit and Days Past Due", "Disabled" }) { }
	}

	[PXCacheName(Messages.CustomerClass)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(CustomerClassMaint))]
	public partial class CustomerClass : PX.Data.IBqlTable
	{
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		protected String _CustomerClassID;
		
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Class ID", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(CustomerClass.customerClassID), CacheGlobal = true)]
		[PXFieldDescription]
		[PXReferentialIntegrityCheck]
		public virtual String CustomerClassID
		{
			get
			{
				return this._CustomerClassID;
			}
			set
			{
				this._CustomerClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDefault(typeof(Search2<CustomerClass.termsID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		public virtual String TermsID
		{
			get
			{
				return this._TermsID;
			}
			set
			{
				this._TermsID = value;
			}
		}
			#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDefault(typeof(Search2<CustomerClass.taxZoneID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		[PXForeignReference(typeof(Field<CustomerClass.taxZoneID>.IsRelatedTo<TaxZone.taxZoneID>))]
		public virtual String TaxZoneID
		{
			get
			{
				return this._TaxZoneID;
			}
			set
			{
				this._TaxZoneID = value;
			}
		}
		#endregion
		#region RequireTaxZone
		public abstract class requireTaxZone : PX.Data.BQL.BqlBool.Field<requireTaxZone> { }
		protected Boolean? _RequireTaxZone;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.requireTaxZone, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Require Tax Zone")]
		public virtual Boolean? RequireTaxZone
		{
			get
			{
				return this._RequireTaxZone;
			}
			set
			{
				this._RequireTaxZone = value;
			}
		}
		#endregion
		#region TaxCalcMode
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(TaxCalculationMode.taxSetting))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
		#endregion
		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : PX.Data.BQL.BqlString.Field<avalaraCustomerUsageType> { }
		protected String _AvalaraCustomerUsageType;
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Entity Usage Type", Required = true)]
		[PXDefault(TXAvalaraCustomerUsageType.Default)]
		[TX.TXAvalaraCustomerUsageType.List]
		public virtual String AvalaraCustomerUsageType
		{
			get
			{
				return this._AvalaraCustomerUsageType;
			}
			set
			{
				this._AvalaraCustomerUsageType = value;
			}
		}
		#endregion
		#region RequireAvalaraCustomerUsageType
		public abstract class requireAvalaraCustomerUsageType : PX.Data.BQL.BqlBool.Field<requireAvalaraCustomerUsageType> { }
		protected Boolean? _RequireAvalaraCustomerUsageType;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.requireAvalaraCustomerUsageType, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Require Entity Usage Type")]
		public virtual Boolean? RequireAvalaraCustomerUsageType
		{
			get
			{
				return this._RequireAvalaraCustomerUsageType;
			}
			set
			{
				this._RequireAvalaraCustomerUsageType = value;
			}
		}
		#endregion
        #region PriceClassID
        public abstract class priceClassID : PX.Data.BQL.BqlString.Field<priceClassID> { }
        protected String _PriceClassID;
		[PXDefault(typeof(Search2<CustomerClass.priceClassID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
        [PXUIField(DisplayName = "Price Class ID")]
        [PXSelector(typeof(Search<ARPriceClass.priceClassID>), CacheGlobal = true)]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDefault(typeof(Search2<CustomerClass.curyID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
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
		#region CuryRateTypeID
		public abstract class curyRateTypeID : PX.Data.BQL.BqlString.Field<curyRateTypeID> { }
		protected String _CuryRateTypeID;
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXDefault(typeof(Coalesce<Search2<CustomerClass.curyRateTypeID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
								  Search<CMSetup.aRRateTypeDflt>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Currency Rate Type")]
		public virtual String CuryRateTypeID
		{
			get
			{
				return this._CuryRateTypeID;
			}
			set
			{
				this._CuryRateTypeID = value;
			}
		}
		#endregion
		#region AllowOverrideCury
		public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury> { }
		protected Boolean? _AllowOverrideCury;
		[PXDBBool()]
		[PXUIField(DisplayName = "Enable Currency Override")]
		[PXDefault(false, typeof(Coalesce<Search2<CustomerClass.allowOverrideCury, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
										  Search<CMSetup.aRCuryOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AllowOverrideCury
		{
			get
			{
				return this._AllowOverrideCury;
			}
			set
			{
				this._AllowOverrideCury = value;
			}
		}
		#endregion
		#region AllowOverrideRate
		public abstract class allowOverrideRate : PX.Data.BQL.BqlBool.Field<allowOverrideRate> { }
		protected Boolean? _AllowOverrideRate;
		[PXDBBool()]
		[PXUIField(DisplayName = "Enable Rate Override")]
		[PXDefault(false, typeof(Coalesce<Search2<CustomerClass.allowOverrideRate, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>,
										  Search<CMSetup.aRRateTypeOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual Boolean? AllowOverrideRate
		{
			get
			{
				return this._AllowOverrideRate;
			}
			set
			{
				this._AllowOverrideRate = value;
			}
		}
		#endregion
		#region ARAcctID
		public abstract class aRAcctID : PX.Data.BQL.BqlInt.Field<aRAcctID> { }
		protected Int32? _ARAcctID;
		[PXDefault(typeof(Search2<CustomerClass.aRAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "AR Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description), ControlAccountForModule = ControlAccountModule.AR)]
		[PXForeignReference(typeof(Field<CustomerClass.aRAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? ARAcctID
		{
			get
			{
				return this._ARAcctID;
			}
			set
			{
				this._ARAcctID = value;
			}
		}
		#endregion
		#region ARSubID
		public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
		protected Int32? _ARSubID;
		[PXDefault(typeof(Search2<CustomerClass.aRSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.aRAcctID), DisplayName = "AR Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.aRSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ARSubID
		{
			get
			{
				return this._ARSubID;
			}
			set
			{
				this._ARSubID = value;
			}
		}
		#endregion
        #region DiscountAcctID
        public abstract class discountAcctID : PX.Data.BQL.BqlInt.Field<discountAcctID> { }
        protected Int32? _DiscountAcctID;
        [PXDefault(typeof(Search2<CustomerClass.discountAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), Required = false, AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<CustomerClass.discountAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? DiscountAcctID
        {
            get
            {
                return this._DiscountAcctID;
            }
            set
            {
                this._DiscountAcctID = value;
            }
        }
        #endregion
        #region DiscountSubID
        public abstract class discountSubID : PX.Data.BQL.BqlInt.Field<discountSubID> { }
        protected Int32? _DiscountSubID;
        [PXDefault(typeof(Search2<CustomerClass.discountSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(CustomerClass.discountAcctID), DisplayName = "Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description), Required = false)]
        [PXForeignReference(typeof(Field<CustomerClass.discountSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? DiscountSubID
        {
            get
            {
                return this._DiscountSubID;
            }
            set
            {
                this._DiscountSubID = value;
            }
        }
        #endregion
		#region DiscTakenAcctID
		public abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }
		protected Int32? _DiscTakenAcctID;
		[PXDefault(typeof(Search2<CustomerClass.discTakenAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<CustomerClass.discTakenAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? DiscTakenAcctID
		{
			get
			{
				return this._DiscTakenAcctID;
			}
			set
			{
				this._DiscTakenAcctID = value;
			}
		}
				#endregion
		#region DiscTakenSubID
		public abstract class discTakenSubID : PX.Data.BQL.BqlInt.Field<discTakenSubID> { }
		protected Int32? _DiscTakenSubID;
		[PXDefault(typeof(Search2<CustomerClass.discTakenSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.discTakenSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? DiscTakenSubID
		{
			get
			{
				return this._DiscTakenSubID;
			}
			set
			{
				this._DiscTakenSubID = value;
			}
		}
		#endregion
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[PXDefault(typeof(Search2<CustomerClass.salesAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<CustomerClass.salesAcctID>.IsRelatedTo<Account.accountID>))]
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
		[PXDefault(typeof(Search2<CustomerClass.salesSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.salesSubID>.IsRelatedTo<Sub.subID>))]
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
		#region COGSAcctID
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		protected Int32? _COGSAcctID;
		[PXDefault(typeof(Search2<CustomerClass.cOGSAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "COGS Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
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
		[PXDefault(typeof(Search2<CustomerClass.cOGSSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
		#region FreightAcctID
		public abstract class freightAcctID : PX.Data.BQL.BqlInt.Field<freightAcctID> { }
		protected Int32? _FreightAcctID;
		[PXDefault(typeof(Search2<CustomerClass.freightAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Freight Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<CustomerClass.freightAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? FreightAcctID
		{
			get
			{
				return this._FreightAcctID;
			}
			set
			{
				this._FreightAcctID = value;
			}
		}
		#endregion
		#region FreightSubID
		public abstract class freightSubID : PX.Data.BQL.BqlInt.Field<freightSubID> { }
		protected Int32? _FreightSubID;
		[PXDefault(typeof(Search2<CustomerClass.freightSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.freightAcctID), DisplayName = "Freight Sub.",
			Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.freightSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? FreightSubID
		{
			get
			{
				return this._FreightSubID;
			}
			set
			{
				this._FreightSubID = value;
			}
		}
		#endregion
		#region MiscAcctID
		public abstract class miscAcctID : PX.Data.BQL.BqlInt.Field<miscAcctID> { }
		protected Int32? _MiscAcctID;
		[PXDefault(typeof(Search2<CustomerClass.miscAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Misc. Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		
		public virtual Int32? MiscAcctID
		{
			get
			{
				return this._MiscAcctID;
			}
			set
			{
				this._MiscAcctID = value;
			}
		}
		#endregion
		#region MiscSubID
		public abstract class miscSubID : PX.Data.BQL.BqlInt.Field<miscSubID> { }
		protected Int32? _MiscSubID;
		[PXDefault(typeof(Search2<CustomerClass.miscSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.miscAcctID), DisplayName = "Misc. Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		
		public virtual Int32? MiscSubID
		{
			get
			{
				return this._MiscSubID;
			}
			set
			{
				this._MiscSubID = value;
			}
		}
		#endregion
		#region PrepaymentAcctID
		public abstract class prepaymentAcctID : PX.Data.BQL.BqlInt.Field<prepaymentAcctID> { }
		protected int? _PrepaymentAcctID;
		[PXDefault(typeof(Search2<CustomerClass.prepaymentAcctID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AR)]
		[PXForeignReference(typeof(Field<CustomerClass.prepaymentAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual int? PrepaymentAcctID
		{
			get
			{
				return _PrepaymentAcctID;
			}
			set
			{
				_PrepaymentAcctID = value;
			}
		}
		#endregion
		#region PrepaymentSubID
		public abstract class prepaymentSubID : PX.Data.BQL.BqlInt.Field<prepaymentSubID> { }
		protected int? _PrepaymentSubID;
		[PXDefault(typeof(Search2<CustomerClass.prepaymentSubID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(CustomerClass.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.prepaymentSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? PrepaymentSubID
		{
			get
			{
				return _PrepaymentSubID;
			}
			set
			{
				_PrepaymentSubID = value;
			}
		}
		#endregion
        #region UnrealizedGainAcctID
        public abstract class unrealizedGainAcctID : PX.Data.BQL.BqlInt.Field<unrealizedGainAcctID> { }
        protected Int32? _UnrealizedGainAcctID;
        [Account(null,
            DisplayName = "Unrealized Gain Account",
            Visibility = PXUIVisibility.Visible,
            DescriptionField = typeof(Account.description),
            AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<CustomerClass.unrealizedGainAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? UnrealizedGainAcctID
        {
            get
            {
                return this._UnrealizedGainAcctID;
            }
            set
            {
                this._UnrealizedGainAcctID = value;
            }
        }
        #endregion
        #region UnrealizedGainSubID
        public abstract class unrealizedGainSubID : PX.Data.BQL.BqlInt.Field<unrealizedGainSubID> { }
        protected Int32? _UnrealizedGainSubID;
        [SubAccount(typeof(CustomerClass.unrealizedGainAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Gain Sub.",
            Visibility = PXUIVisibility.Visible)]
        [PXForeignReference(typeof(Field<CustomerClass.unrealizedGainSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? UnrealizedGainSubID
        {
            get
            {
                return this._UnrealizedGainSubID;
            }
            set
            {
                this._UnrealizedGainSubID = value;
            }
        }
        #endregion
        #region UnrealizedLossAcctID
        public abstract class unrealizedLossAcctID : PX.Data.BQL.BqlInt.Field<unrealizedLossAcctID> { }
        protected Int32? _UnrealizedLossAcctID;
        [Account(null,
            DisplayName = "Unrealized Loss Account",
            Visibility = PXUIVisibility.Visible,
            DescriptionField = typeof(Account.description),
            AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<CustomerClass.unrealizedLossAcctID>.IsRelatedTo<Account.accountID>))]
        public virtual Int32? UnrealizedLossAcctID
        {
            get
            {
                return this._UnrealizedLossAcctID;
            }
            set
            {
                this._UnrealizedLossAcctID = value;
            }
        }
        #endregion
        #region UnrealizedLossSubID
        public abstract class unrealizedLossSubID : PX.Data.BQL.BqlInt.Field<unrealizedLossSubID> { }
        protected Int32? _UnrealizedLossSubID;
        [SubAccount(typeof(CustomerClass.unrealizedLossAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Loss Sub.",
            Visibility = PXUIVisibility.Visible)]
        [PXForeignReference(typeof(Field<CustomerClass.unrealizedLossSubID>.IsRelatedTo<Sub.subID>))]
        public virtual Int32? UnrealizedLossSubID
        {
            get
            {
                return this._UnrealizedLossSubID;
            }
            set
            {
                this._UnrealizedLossSubID = value;
            }
        }
        #endregion
		#region AutoApplyPayments
		public abstract class autoApplyPayments : PX.Data.BQL.BqlBool.Field<autoApplyPayments> { }
		protected Boolean? _AutoApplyPayments;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.autoApplyPayments, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Auto-Apply Payments")]
		public virtual Boolean? AutoApplyPayments
		{
			get
			{
				return this._AutoApplyPayments;
			}
			set
			{
				this._AutoApplyPayments = value;
			}
		}
		#endregion
		#region PrintStatements
		public abstract class printStatements : PX.Data.BQL.BqlBool.Field<printStatements> { }
		protected Boolean? _PrintStatements;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printStatements, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Print Statements")]
		public virtual Boolean? PrintStatements
		{
			get
			{
				return this._PrintStatements;
			}
			set
			{
				this._PrintStatements = value;
			}
		}
		#endregion
		#region PrintCuryStatements
		public abstract class printCuryStatements : PX.Data.BQL.BqlBool.Field<printCuryStatements> { }
		protected Boolean? _PrintCuryStatements;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printCuryStatements, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Multi-Currency Statements")]
		public virtual Boolean? PrintCuryStatements
		{
			get
			{
				return this._PrintCuryStatements;
			}
			set
			{
				this._PrintCuryStatements = value;
			}
		}
		#endregion
		#region SendStatementByEmail
		public abstract class sendStatementByEmail : PX.Data.BQL.BqlBool.Field<sendStatementByEmail> { }
		[PXDBBool]
		[PXDefault(false, typeof(Search2<
			CustomerClass.sendStatementByEmail, 
				InnerJoin<ARSetup, 
					On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Send Statements By Email")]
		public virtual bool? SendStatementByEmail
		{
			get;
			set;
		}
		#endregion
		#region CreditRule
		public abstract class creditRule : PX.Data.BQL.BqlString.Field<creditRule> { }
		protected String _CreditRule;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(CreditRuleTypes.CS_DAYS_PAST_DUE, typeof(Search2<CustomerClass.creditRule, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[CreditRule()]
		[PXUIField(DisplayName = "Credit Verification")]
		public virtual String CreditRule
		{
			get
			{
				return this._CreditRule;
			}
			set
			{
				this._CreditRule = value;
			}
		}
		#endregion
		#region CreditLimit
		public abstract class creditLimit : PX.Data.BQL.BqlDecimal.Field<creditLimit> { }
		protected Decimal? _CreditLimit;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.creditLimit, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Credit Limit")]
		public virtual Decimal? CreditLimit
		{
			get
			{
				return this._CreditLimit;
			}
			set
			{
				this._CreditLimit = value;
			}
		}
		#endregion
		#region CreditDaysPastDue
		public abstract class creditDaysPastDue : PX.Data.BQL.BqlShort.Field<creditDaysPastDue> { }
		protected Int16? _CreditDaysPastDue;
		[PXDefault((short)0, typeof(Search2<CustomerClass.creditDaysPastDue, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXDBShort(MinValue =0, MaxValue =3650)]
		[PXUIField(DisplayName = "Credit Days Past Due")]
		public virtual Int16? CreditDaysPastDue
		{
			get
			{
				return this._CreditDaysPastDue;
			}
			set
			{
				this._CreditDaysPastDue = value;
			}
		}
		#endregion
		#region StatementType
		public abstract class statementType : PX.Data.BQL.BqlString.Field<statementType> { }
		protected String _StatementType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(ARStatementType.OpenItem, typeof(Search2<CustomerClass.statementType, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[LabelList(typeof(ARStatementType))]
		[PXUIField(DisplayName = "Statement Type")]
		public virtual String StatementType
		{
			get
			{
				return this._StatementType;
			}
			set
			{
				this._StatementType = value;
			}
		}
		#endregion
		#region StatementCycleId
		public abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
		protected String _StatementCycleId;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search2<CustomerClass.statementCycleId, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Statement Cycle ID")]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		public virtual String StatementCycleId
		{
			get
			{
				return this._StatementCycleId;
			}
			set
			{
				this._StatementCycleId = value;
			}
		}
		#endregion
		#region SmallBalanceAllow
		public abstract class smallBalanceAllow : PX.Data.BQL.BqlBool.Field<smallBalanceAllow> { }
		protected Boolean? _SmallBalanceAllow;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.smallBalanceAllow, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Enable Write-Offs")]
		public virtual Boolean? SmallBalanceAllow
		{
			get
			{
				return this._SmallBalanceAllow;
			}
			set
			{
				this._SmallBalanceAllow = value;
			}
		}
		#endregion
		#region SmallBalanceLimit
		public abstract class smallBalanceLimit : PX.Data.BQL.BqlDecimal.Field<smallBalanceLimit> { }
		protected Decimal? _SmallBalanceLimit;
		[PXDBCury(typeof(CustomerClass.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.smallBalanceLimit, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Write-Off Limit")]
		public virtual Decimal? SmallBalanceLimit
		{
			get
			{
				return this._SmallBalanceLimit;
			}
			set
			{
				this._SmallBalanceLimit = value;
			}
		}
		#endregion
		#region FinChargeApply
		public abstract class finChargeApply : PX.Data.BQL.BqlBool.Field<finChargeApply> { }
		protected Boolean? _FinChargeApply;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.finChargeApply, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Apply Overdue Charges")]
		public virtual Boolean? FinChargeApply
		{
			get
			{
				return this._FinChargeApply;
			}
			set
			{
				this._FinChargeApply = value;
			}
		}
		#endregion
		#region FinChargeID
		public abstract class finChargeID : PX.Data.BQL.BqlString.Field<finChargeID> { }
		protected String _FinChargeID;
		[PXDefault(typeof(Search2<CustomerClass.finChargeID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Overdue Charge ID")]
		[PXSelector(typeof(ARFinCharge.finChargeID), DescriptionField = typeof(ARFinCharge.finChargeDesc))]
		public virtual String FinChargeID
		{
			get
			{
				return this._FinChargeID;
			}
			set
			{
				this._FinChargeID = value;
			}
		}
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		public virtual String CountryID
		{
			get
			{
				return this._CountryID;
			}
			set
			{
				this._CountryID = value;
			}
		}
			#endregion
		#region OverLimitAmount
		public abstract class overLimitAmount : PX.Data.BQL.BqlDecimal.Field<overLimitAmount> { }
		protected Decimal? _OverLimitAmount;
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search2<CustomerClass.overLimitAmount, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Over-Limit Amount")]
		public virtual Decimal? OverLimitAmount
		{
			get
			{
				return this._OverLimitAmount;
			}
			set
			{
				this._OverLimitAmount = value;
			}
		}
		#endregion

		#region DefPaymentMethodID
		public abstract class defPaymentMethodID : PX.Data.BQL.BqlString.Field<defPaymentMethodID> { }
		protected String _DefPaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXDefault(typeof(Search2<CustomerClass.defPaymentMethodID, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID, Where<PaymentMethod.useForAR, Equal<True>,And<PaymentMethod.isActive, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr), CacheGlobal = true)]
		public virtual String DefPaymentMethodID
		{
			get
			{
				return this._DefPaymentMethodID;
			}
			set
			{
				this._DefPaymentMethodID = value;
			}
		}
		#endregion
		#region PrintInvoices
		public abstract class printInvoices : PX.Data.BQL.BqlBool.Field<printInvoices> { }
		protected Boolean? _PrintInvoices;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.printInvoices, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Print Invoices")]
		public virtual Boolean? PrintInvoices
		{
			get
			{
				return this._PrintInvoices;
			}
			set
			{
				this._PrintInvoices = value;
			}
		}
		#endregion
		#region MailInvoices
		public abstract class mailInvoices : PX.Data.BQL.BqlBool.Field<mailInvoices> { }
		protected Boolean? _MailInvoices;
		[PXDBBool()]
		[PXDefault(false, typeof(Search2<CustomerClass.mailInvoices, InnerJoin<ARSetup, On<CustomerClass.customerClassID, Equal<ARSetup.dfltCustomerClassID>>>>))]
		[PXUIField(DisplayName = "Send Invoices by Email")]
		public virtual Boolean? MailInvoices
		{
			get
			{
				return this._MailInvoices;
			}
			set
			{
				this._MailInvoices = value;
			}
		}
		#endregion
        #region PrintDunningLetters
        public abstract class printDunningLetters : PX.Data.BQL.BqlBool.Field<printDunningLetters> { }
        protected Boolean? _PrintDunningLetters;
        [PXDBBool()]
        [PXUIField(DisplayName = "Print Dunning Letters")]
        [PXDefault(true)]
        public virtual Boolean? PrintDunningLetters
        {
            get
            {
                return this._PrintDunningLetters;
            }
            set
            {
                this._PrintDunningLetters = value;
            }
        }
        #endregion
        #region MailDunningLetters
        public abstract class mailDunningLetters : PX.Data.BQL.BqlBool.Field<mailDunningLetters> { }
        protected Boolean? _MailDunningLetters;
        [PXDBBool()]
        [PXUIField(DisplayName = "Send Dunning Letters by Email")]
        [PXDefault(false)]
        public virtual Boolean? MailDunningLetters
        {
            get
            {
                return this._MailDunningLetters;
            }
            set
            {
                this._MailDunningLetters = value;
            }
        }
        #endregion
		#region DefaultLocationCDFromBranch
		public abstract class defaultLocationCDFromBranch : PX.Data.BQL.BqlBool.Field<defaultLocationCDFromBranch> { }
		protected bool? _DefaultLocationCDFromBranch;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default Location ID from Branch")]
		public virtual bool? DefaultLocationCDFromBranch
		{
			get
			{
				return _DefaultLocationCDFromBranch;
			}
			set
			{
				_DefaultLocationCDFromBranch = value;
			}
		}
		#endregion

		#region ShipVia
		public abstract class shipVia : PX.Data.BQL.BqlString.Field<shipVia> { }
		protected string _ShipVia;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Ship Via")]
		[PXSelector(typeof(Search<Carrier.carrierID>), DescriptionField = typeof(Carrier.description), CacheGlobal = true)]
		public virtual string ShipVia
		{
			get
			{
				return _ShipVia;
			}
			set
			{
				_ShipVia = value;
			}
		}
		#endregion
		#region ShipComplete
		public abstract class shipComplete : PX.Data.BQL.BqlString.Field<shipComplete> { }
		protected string _ShipComplete;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SOShipComplete.CancelRemainder)]
		[SOShipComplete.List]
		[PXUIField(DisplayName = "Shipping Rule")]
		public virtual string ShipComplete
		{
			get
			{
				return _ShipComplete;
			}
			set
			{
				_ShipComplete = value;
			}
		}
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID> { }
		protected string _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(ShipTerms.shipTermsID), DescriptionField = typeof(ShipTerms.description), CacheGlobal = true)]
		public virtual string ShipTermsID
		{
			get
			{
				return _ShipTermsID;
			}
			set
			{
				_ShipTermsID = value;
			}
		}
		#endregion
		#region SalesPersonID
		public abstract class salesPersonID : PX.Data.BQL.BqlInt.Field<salesPersonID> { }
		protected int? _SalesPersonID;
		[SalesPerson]
		[PXDefault(typeof(Search<CustSalesPeople.salesPersonID, Where<CustSalesPeople.isDefault, Equal<True>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXForeignReference(typeof(Field<CustomerClass.salesPersonID>.IsRelatedTo<SalesPerson.salesPersonID>))]
		public virtual int? SalesPersonID
		{
			get
			{
				return _SalesPersonID;
			}
			set
			{
				_SalesPersonID = value;
			}
		}
		#endregion

        #region DiscountLimit
        public abstract class discountLimit : PX.Data.BQL.BqlDecimal.Field<discountLimit> { }
        protected Decimal? _DiscountLimit;
        [PXDBDecimal(MaxValue = 100, MinValue = 0)]
        [PXDefault(TypeCode.Decimal, "50.0")]
        [PXUIField(DisplayName = "Group/Document Discount Limit (%)")]
        public virtual Decimal? DiscountLimit
        {
            get
            {
                return this._DiscountLimit;
            }
            set
            {
                this._DiscountLimit = value;
            }
        }
		#endregion

		#region LocaleName
		public abstract class localeName : PX.Data.BQL.BqlString.Field<localeName> { }
		[PXSelector(typeof(
			Search<Locale.localeName,
			Where<Locale.isActive, Equal<True>>>),
			DescriptionField = typeof(Locale.translatedName))]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Locale")]
		public virtual string LocaleName { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(CustomerClass.customerClassID))]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;
		[SingleGroup]
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

		#region RetainageAcctID
		public abstract class retainageAcctID : PX.Data.BQL.BqlInt.Field<retainageAcctID> { }

		[Account(DisplayName = "Retainage Receivable Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description),
			ControlAccountForModule = ControlAccountModule.AR)]
		[PXForeignReference(typeof(Field<CustomerClass.retainageAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual int? RetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region RetainageSubID
		public abstract class retainageSubID : PX.Data.BQL.BqlInt.Field<retainageSubID> { }

		[SubAccount(typeof(CustomerClass.retainageAcctID), 
			DisplayName = "Retainage Receivable Sub.", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<CustomerClass.retainageSubID>.IsRelatedTo<Sub.subID>))]
		public virtual int? RetainageSubID
		{
			get;
			set;
		}
		#endregion
		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(false)]
		public virtual bool? RetainageApply
		{
			get;
			set;
		}
		#endregion
		#region PaymentsByLinesAllowed
		public abstract class paymentsByLinesAllowed : PX.Data.BQL.BqlBool.Field<paymentsByLinesAllowed> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Pay by Line",
			Visibility = PXUIVisibility.Visible,
			FieldClass = nameof(FeaturesSet.PaymentsByLines))]
		[PXDefault(false)]
		public virtual bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion
	}

	// Used in AR675000.rpx
	public class FilterCustomerByClass : IBqlTable
	{
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		protected String _CustomerClassID;

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CustomerClass.customerClassID>))]
		public virtual String CustomerClassID
		{
			get
			{
				return this._CustomerClassID;
			}
			set
			{
				this._CustomerClassID = value;
			}
		}
		#endregion

		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[PXDBInt]
		[PXDimensionSelector(CustomerAttribute.DimensionName, typeof(Search<Customer.bAccountID, Where<Customer.customerClassID, Equal<Optional<FilterCustomerByClass.customerClassID>>, Or<Optional<FilterCustomerByClass.customerClassID>, IsNull>>>),
				typeof(BAccountR.acctCD), typeof(BAccountR.acctCD), typeof(Customer.acctName), typeof(Customer.customerClassID), typeof(Customer.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID))]
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

	}


}
