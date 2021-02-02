namespace PX.Objects.EP
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.CA;
	using PX.Objects.CM;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.AP;

	[System.SerializableAttribute()]
    [PXHidden]
	public partial class EPVendorClass : VendorClass
	{
		public new abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		public new abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }
		public new abstract class discTakenSubID : PX.Data.BQL.BqlInt.Field<discTakenSubID> { }
		public new abstract class expenseAcctID : PX.Data.BQL.BqlInt.Field<expenseAcctID> { }
		public new abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
	}

	[PXPrimaryGraph(typeof(EmployeeClassMaint))]
	[System.SerializableAttribute()]
	[PXTable]
	[PXCacheName(Messages.EmployeeClass)]
	public partial class EPEmployeeClass : VendorClass
	{
		#region VendorClassID
		public new abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		[PXSelector(typeof(EPEmployeeClass.vendorClassID), CacheGlobal = true)]
		public override String VendorClassID
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
		#region DiscTakenAcctID
		[PXDefault(typeof(Search2<EPVendorClass.discTakenAcctID, InnerJoin<APSetup, On<EPVendorClass.vendorClassID, Equal<APSetup.dfltVendorClassID>>>>))]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discTakenAcctID>.IsRelatedTo<Account.accountID>))]
		public override Int32? DiscTakenAcctID
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
		[PXDefault(typeof(Search2<EPVendorClass.discTakenSubID,InnerJoin<APSetup,On<EPVendorClass.vendorClassID,Equal<APSetup.dfltVendorClassID>>>>))]
		[SubAccount(typeof(VendorClass.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Invisible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.discTakenSubID>.IsRelatedTo<Sub.subID>))]
		public override Int32? DiscTakenSubID
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
		#region PaymentMethodID
		public new abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(Search<PaymentMethod.paymentMethodID,
							Where<PaymentMethod.useForAP, Equal<True>,
								And<PaymentMethod.isActive, Equal<True>>>>), DescriptionField = typeof(PaymentMethod.descr))]
		public override String PaymentMethodID
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
		public new abstract class cashAcctID : PX.Data.BQL.BqlInt.Field<cashAcctID> { }
		
		[CashAccount(typeof(Search2<CashAccount.cashAccountID,
						InnerJoin<PaymentMethodAccount,
							On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
						Where2<Match<Current<AccessInfo.userName>>,
							And<CashAccount.clearingAccount, Equal<False>,
							And<PaymentMethodAccount.paymentMethodID, Equal<Current<EPEmployeeClass.paymentMethodID>>,
							And<PaymentMethodAccount.useForAP, Equal<True>>>>>>))]
		public override Int32? CashAcctID
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
		#region SalesAcctID
		public abstract class salesAcctID : PX.Data.BQL.BqlInt.Field<salesAcctID> { }
		protected Int32? _SalesAcctID;
		[Account(DisplayName = "Sales Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<EPEmployeeClass.salesAcctID>.IsRelatedTo<Account.accountID>))]
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
		[SubAccount(typeof(EPEmployeeClass.salesAcctID), DisplayName = "Sales Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<EPEmployeeClass.salesSubID>.IsRelatedTo<Sub.subID>))]
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
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Calendar", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendar.calendarID>), DescriptionField = typeof(CSCalendar.description))]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
        #region HoursValidation
        public abstract class hoursValidation : PX.Data.BQL.BqlString.Field<hoursValidation> { }
        protected String _HoursValidation;
        [PXDBString(1)]
        [PXUIField(DisplayName = "Regular Hours Validation")]
        [HoursValidationOption.List]
        [PXDefault(HoursValidationOption.Validate)]
        public virtual String HoursValidation
        {
            get
            {
                return this._HoursValidation;
            }
            set
            {
                this._HoursValidation = value;
            }
        }
        #endregion
		#region VPaymentByType
		public new abstract class paymentByType : PX.Data.BQL.BqlInt.Field<paymentByType> { }
		[PXDBInt()]
		[PXDefault(APPaymentBy.DueDate, PersistingCheck = PXPersistingCheck.Nothing)]
		[APPaymentBy.List]
		[PXUIField(DisplayName = "Payment By")]
		public override int? PaymentByType
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
		#region DefaultDateInActivity
		public abstract class defaultDateInActivity : PX.Data.BQL.BqlString.Field<defaultDateInActivity>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { NextWorkDay, LastDay },
					new string[] { Messages.NextWorkDay, Messages.LastDay }) { ; }
			}

			public const string LastDay = "LD";
			public const string NextWorkDay = "NW";
		}
		protected String _DefaultDateInActivity;
		[PXDBString(2)]
		[PXUIField(DisplayName = "Default Date in Time Cards")]
		[defaultDateInActivity.List]
		[PXDefault(defaultDateInActivity.NextWorkDay)]
		public virtual String DefaultDateInActivity
		{
			get
			{
				return this._DefaultDateInActivity;
			}
			set
			{
				this._DefaultDateInActivity = value;
			}
		}
		#endregion

	}

    public static class HoursValidationOption
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { Validate, WarningOnly, None },
                new string[] { Messages.Validate, Messages.WarningOnly, Messages.None }) { ; }
        }
        
        public const string Validate = "V";
        public const string WarningOnly = "W";
        public const string None = "N";
    }
}

namespace PX.Objects.EP.Standalone
{
	using System;
	using PX.Data;

    [Serializable]
	public partial class EPEmployeeClass : PX.Data.IBqlTable
	{
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected string _VendorClassID;
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = ">aaaaaaaaaa")]
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
	}
}
