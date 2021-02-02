using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.PO;

namespace PX.Objects.AP
{
    using System;
    using PX.Data;
    using PX.Objects.GL;
    using PX.Objects.CS;
    using PX.Objects.CA;
    using PX.Objects.CM;
    using PX.Objects.TX;
    using PX.Objects.EP.Standalone;
    using PX.SM;
	using PX.Data.ReferentialIntegrity.Attributes;

	[PXCacheName(Messages.VendorClass, CacheGlobal = true)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(typeof(VendorClassMaint))]
	public partial class VendorClass : PX.Data.IBqlTable
	{
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Class ID", Visibility=PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>), CacheGlobal = true)]
		[PXFieldDescription]
		public virtual String VendorClassID
		{
			get
			{
				return this._VendorClassID;
			}
			set
			{
				this._VendorClassID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
		protected String _Descr;
		[PXDBString(60, IsUnicode = true)]
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
		[PXDefault(typeof(Search2<VendorClass.termsID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Terms", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.vendor>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXForeignReference(typeof(Field<VendorClass.termsID>.IsRelatedTo<Terms.termsID>))]
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
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		protected String _PaymentMethodID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search2<VendorClass.paymentMethodID, 
					InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>),
					PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.useForAP, Equal<True>,
								And<PaymentMethod.isActive,Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public virtual String PaymentMethodID
		{
			get
			{
				return this._PaymentMethodID;
			}
			set
			{
				this._PaymentMethodID = value;
			}
		}
		#endregion
		#region CashAcctID
		public abstract class cashAcctID : PX.Data.BQL.BqlInt.Field<cashAcctID> { }
		protected Int32? _CashAcctID;
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[CashAccount(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<VendorClass.paymentMethodID>>,
                            And<PaymentMethodAccount.useForAP, Equal<True>>>>>>))]
		public virtual Int32? CashAcctID
		{
			get
			{
				return this._CashAcctID;
			}
			set
			{
				this._CashAcctID = value;
			}
		}
		#endregion		
		#region VPaymentByType
		public abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		protected int? _PaymentByType;
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate, typeof(Search2<VendorClass.paymentByType, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>))]		
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public virtual int? PaymentByType
		{
			get
			{
				return this._PaymentByType;
			}
			set
			{
				this._PaymentByType = value;
			}
		}
		#endregion
		#region APAcctID
		public abstract class aPAcctID : PX.Data.BQL.BqlInt.Field<aPAcctID> { }
		protected Int32? _APAcctID;
		[PXDefault(typeof(Search2<VendorClass.aPAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "AP Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description), ControlAccountForModule = ControlAccountModule.AP)]
		[PXForeignReference(typeof(Field<VendorClass.aPAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? APAcctID
		{
			get
			{
				return this._APAcctID;
			}
			set
			{
				this._APAcctID = value;
			}
		}
		#endregion
		#region APSubID
		public abstract class aPSubID : PX.Data.BQL.BqlInt.Field<aPSubID> { }
		protected Int32? _APSubID;
		[PXDefault(typeof(Search2<VendorClass.aPSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(VendorClass.aPAcctID), DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.aPSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? APSubID
		{
			get
			{
				return this._APSubID;
			}
			set
			{
				this._APSubID = value;
			}
		}
		#endregion
		#region DiscTakenAcctID
		public abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }
		protected Int32? _DiscTakenAcctID;
		[PXDefault(typeof(Search2<VendorClass.discTakenAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<VendorClass.discTakenAcctID>.IsRelatedTo<Account.accountID>))]
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
		[PXDefault(typeof(Search2<VendorClass.discTakenSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(VendorClass.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.discTakenSubID>.IsRelatedTo<Sub.subID>))]
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
		#region ExpenseAcctID
		public abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		protected Int32? _ExpenseAcctID;
		[PXDefault(typeof(Search2<VendorClass.expenseAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<VendorClass.expenseAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? ExpenseAcctID
		{
			get
			{
				return this._ExpenseAcctID;
			}
			set
			{
				this._ExpenseAcctID = value;
			}
		}
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		protected Int32? _ExpenseSubID;
		[PXDefault(typeof(Search2<VendorClass.expenseSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(VendorClass.expenseAcctID), DisplayName = "Expense Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.expenseSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ExpenseSubID
		{
			get
			{
				return this._ExpenseSubID;
			}
			set
			{
				this._ExpenseSubID = value;
			}
		}
		#endregion
        #region DiscountAcctID
        public abstract class discountAcctID : PX.Data.BQL.BqlInt.Field<discountAcctID> { }
        protected Int32? _DiscountAcctID;
        [PXDefault(typeof(Search2<VendorClass.discountAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
        [PXForeignReference(typeof(Field<VendorClass.discountAcctID>.IsRelatedTo<Account.accountID>))]
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
        [PXDefault(typeof(Search2<VendorClass.discountSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(VendorClass.discountAcctID), DisplayName = "Discount Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXForeignReference(typeof(Field<VendorClass.discountSubID>.IsRelatedTo<Sub.subID>))]
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
        #region FreightAcctID
        public abstract class freightAcctID : PX.Data.BQL.BqlInt.Field<freightAcctID> { }
        protected Int32? _FreightAcctID;
        [PXDefault(typeof(Search2<VendorClass.freightAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Freight Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
        [PXForeignReference(typeof(Field<VendorClass.freightAcctID>.IsRelatedTo<Account.accountID>))]
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
        [PXDefault(typeof(Search2<VendorClass.freightSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(VendorClass.freightAcctID), DisplayName = "Freight Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXForeignReference(typeof(Field<VendorClass.freightSubID>.IsRelatedTo<Sub.subID>))]
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

		#region PrepaymentAcctID
		public abstract class prepaymentAcctID : PX.Data.BQL.BqlInt.Field<prepaymentAcctID> { }
		protected Int32? _PrepaymentAcctID;
		[PXDefault(typeof(Search2<VendorClass.prepaymentAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AP)]
		[PXForeignReference(typeof(Field<VendorClass.prepaymentAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PrepaymentAcctID
		{
			get
			{
				return this._PrepaymentAcctID;
			}
			set
			{
				this._PrepaymentAcctID = value;
			}
		}
		#endregion
		#region PrepaymentSubID
		public abstract class prepaymentSubID : PX.Data.BQL.BqlInt.Field<prepaymentSubID> { }
		protected Int32? _PrepaymentSubID;
		[PXDefault(typeof(Search2<VendorClass.prepaymentSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(VendorClass.prepaymentAcctID), DisplayName = "Prepayment Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.prepaymentSubID>.IsRelatedTo<Sub.subID>))]

		public virtual Int32? PrepaymentSubID
		{
			get
			{
				return this._PrepaymentSubID;
			}
			set
			{
				this._PrepaymentSubID = value;
			}
		}
		#endregion
        #region POAccrualAcctID
        public abstract class pOAccrualAcctID : PX.Data.BQL.BqlInt.Field<pOAccrualAcctID> { }
        protected Int32? _POAccrualAcctID;
        [PXDefault(typeof(Search2<VendorClass.pOAccrualAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
        [PXForeignReference(typeof(Field<VendorClass.pOAccrualAcctID>.IsRelatedTo<Account.accountID>))]
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
        [PXDefault(typeof(Search2<VendorClass.pOAccrualSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(VendorClass.expenseAcctID), DisplayName = "PO Accrual Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
        [PXForeignReference(typeof(Field<VendorClass.pOAccrualSubID>.IsRelatedTo<Sub.subID>))]
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
       

		#region PrebookAcctID
		public abstract class prebookAcctID : PX.Data.BQL.BqlInt.Field<prebookAcctID> { }
		protected Int32? _PrebookAcctID;
		[PXDefault(typeof(Search2<VendorClass.prebookAcctID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<VendorClass.prebookAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? PrebookAcctID
		{
			get
			{
				return this._PrebookAcctID;
			}
			set
			{
				this._PrebookAcctID = value;
			}
		}
		#endregion
		#region PrebookSubID
		public abstract class prebookSubID : PX.Data.BQL.BqlInt.Field<prebookSubID> { }
		protected Int32? _PrebookSubID;
		[PXDefault(typeof(Search2<VendorClass.prebookSubID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(VendorClass.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.prebookSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? PrebookSubID
		{
			get
			{
				return this._PrebookSubID;
			}
			set
			{
				this._PrebookSubID = value;
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
        [PXForeignReference(typeof(Field<VendorClass.unrealizedGainAcctID>.IsRelatedTo<Account.accountID>))]
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
        [SubAccount(typeof(VendorClass.unrealizedGainAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Gain Sub.",
            Visibility = PXUIVisibility.Visible)]
        [PXForeignReference(typeof(Field<VendorClass.unrealizedGainSubID>.IsRelatedTo<Sub.subID>))]
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
        [PXForeignReference(typeof(Field<VendorClass.unrealizedLossAcctID>.IsRelatedTo<Account.accountID>))]
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
        [SubAccount(typeof(VendorClass.unrealizedLossAcctID),
            DescriptionField = typeof(Sub.description),
            DisplayName = "Unrealized Loss Sub.",
            Visibility = PXUIVisibility.Visible)]
        [PXForeignReference(typeof(Field<VendorClass.unrealizedLossSubID>.IsRelatedTo<Sub.subID>))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXDefault(typeof(Search2<VendorClass.curyID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>), CacheGlobal = true)]
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
		[PXDefault(typeof(Search2<CurrencyRateType.curyRateTypeID, 
								LeftJoin<VendorClass, On<CurrencyRateType.curyRateTypeID, Equal<VendorClass.curyRateTypeID>>, 
								InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>,
								LeftJoin<CMSetup, On<CurrencyRateType.curyRateTypeID, Equal<CMSetup.aPRateTypeDflt>>>>>, 
								Where<VendorClass.vendorClassID, NotEqual<Current<VendorClass.vendorClassID>>,
									Or<Current<VendorClass.vendorClassID>, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Curr. Rate Type")]
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
		
		[PXDefault(false, typeof(Coalesce<Search<VendorClass.allowOverrideCury, 
										   Where<VendorClass.vendorClassID, Equal<Current<APSetup.dfltVendorClassID>>, 
											 And<Current<APSetup.dfltVendorClassID>, NotEqual<Current<VendorClass.vendorClassID>>>>>, 
										  Search<CMSetup.aPCuryOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		
		 
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
		
		[PXDefault(false, typeof(Coalesce<Search<VendorClass.allowOverrideRate, 
										   Where<VendorClass.vendorClassID, Equal<Current<APSetup.dfltVendorClassID>>, 
											 And<Current<APSetup.dfltVendorClassID>, NotEqual<Current<VendorClass.vendorClassID>>>>>, 
										  Search<CMSetup.aPRateTypeOverride>>), PersistingCheck = PXPersistingCheck.Nothing)]
		
		
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
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;
		[PXDefault(typeof(Search2<VendorClass.taxZoneID, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), CacheGlobal = true)]
		[PXForeignReference(typeof(Field<VendorClass.taxZoneID>.IsRelatedTo<TaxZone.taxZoneID>))]
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
		[PXDefault(false, typeof(Search2<VendorClass.requireTaxZone, InnerJoin<APSetup, On<VendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		public abstract class taxCalcMode : PX.Data.BQL.BqlString.Field<taxCalcMode>
		{
			[Obsolete("taxCalcMode.List attribute is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.ListAttribute instead.")]
			public class List : TaxCalculationMode.ListAttribute
			{
			}

			[Obsolete("taxCalcMode.gross is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.gross instead.")]
			public class gross : TaxCalculationMode.gross
			{
			}

			[Obsolete("taxCalcMode.net is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.net instead.")]
			public class net : TaxCalculationMode.net
			{
			}

			[Obsolete("taxCalcMode.taxSetting is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.taxSetting instead.")]
			public class taxSetting : TaxCalculationMode.taxSetting
			{
			}

			[Obsolete("taxCalcMode.Net is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.Net instead.")]
			public const string Net = TaxCalculationMode.Net;
			[Obsolete("taxCalcMode.Gross is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.Gross instead.")]
			public const string Gross = TaxCalculationMode.Gross;
			[Obsolete("taxCalcMode.taxSetting is obsolete and will be removed in Acumatica 2018 R1. Please use PX.Objects.TX.TaxCalculationMode.TaxSetting instead.")]
			public const string TaxSetting = TaxCalculationMode.TaxSetting;
		}
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(TaxCalculationMode.taxSetting))]
		[TaxCalculationMode.List]
		[PXUIField(DisplayName = "Tax Calculation Mode")]
		public virtual string TaxCalcMode { get; set; }
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

		#region ShipTermsID
		public abstract class shipTermsID : PX.Data.BQL.BqlString.Field<shipTermsID> { }
		protected string _ShipTermsID;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Shipping Terms")]
		[PXSelector(typeof(Search<ShipTerms.shipTermsID>), CacheGlobal = true, DescriptionField = typeof(ShipTerms.description))]
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
		#region RcptQtyAction
		public abstract class rcptQtyAction : PX.Data.BQL.BqlString.Field<rcptQtyAction> { }
		protected string _RcptQtyAction;
		[PXDBString(1, IsFixed = true)]
		[PXDefault(POReceiptQtyAction.AcceptButWarn)]
		[POReceiptQtyAction.List]
		[PXUIField(DisplayName = "Receipt Action")]
		public virtual string RcptQtyAction
		{
			get
			{
				return _RcptQtyAction;
			}
			set
			{
				_RcptQtyAction = value;
			}
		}
		#endregion
		#region PrintPO
		public abstract class printPO : PX.Data.BQL.BqlBool.Field<printPO> { }
		protected bool? _PrintPO;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Print Orders")]
		public virtual bool? PrintPO
		{
			get
			{
				return _PrintPO;
			}
			set
			{
				_PrintPO = value;
			}
		}
		#endregion
		#region EmailPO
		public abstract class emailPO : PX.Data.BQL.BqlBool.Field<emailPO> { }
		protected bool? _EmailPO;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Send Orders by Email")]
		public virtual bool? EmailPO
		{
			get
			{
				return _EmailPO;
			}
			set
			{
				_EmailPO = value;
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
		[PXNote(DescriptionField = typeof(VendorClass.vendorClassID))]
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
		#region RetainageAcctID
		public abstract class retainageAcctID : PX.Data.BQL.BqlInt.Field<retainageAcctID> { }

		[Account(DisplayName = "Retainage Payable Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AP)]
		[PXForeignReference(typeof(Field<VendorClass.retainageAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual int? RetainageAcctID
		{
			get;
			set;
		}
		#endregion
		#region RetainageSubID
		public abstract class retainageSubID : PX.Data.BQL.BqlInt.Field<retainageSubID> { }

		[SubAccount(typeof(VendorClass.retainageAcctID), DisplayName = "Retainage Payable Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<VendorClass.retainageSubID>.IsRelatedTo<Sub.subID>))]
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
}
