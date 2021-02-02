using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;

namespace PX.Objects.AP
{
	using System;
	using PX.Data;
	using PX.Objects.GL;
	using PX.Objects.CS;
	using PX.Objects.CM;
	using PX.Objects.TX;
    using PX.Objects.CR;
	using PX.Objects.EP.Standalone;
    using PX.SM;
	using PX.Data.ReferentialIntegrity.Attributes;

	public static class RoundingTypes
    {
        public const string Mathematical = "R";
        public const string Ceil = "C";
        public const string Floor = "F";
    }

	[System.SerializableAttribute()]
	[PXTable(typeof(CR.BAccount.bAccountID))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(VendorMaint),
			typeof(EP.EmployeeMaint)
		},
		new Type[] {
			typeof(Select<
					AP.Vendor,
				Where2<
					Where<AP.Vendor.type, Equal<BAccountType.vendorType>,
						Or<AP.Vendor.type, Equal<BAccountType.combinedType>>>,
					And<AP.Vendor.bAccountID, Equal<Current<BAccount.bAccountID>>>>>),
			typeof(Select<
					EP.EPEmployee,
				Where<
					EP.EPEmployee.type, Equal<BAccountType.employeeType>,
					And<EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>>)
		})]
	[PXCacheName(Messages.Vendor, PXDacType.Catalogue, CacheGlobal = true)]
	public partial class Vendor : CR.BAccount, PX.SM.IIncludable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<Vendor>.By<bAccountID>
		{
			public static Vendor Find(PXGraph graph, int? bAccountID) => FindBy(graph, bAccountID);
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[VendorRaw(IsKey = true)]
		[PXDefault()]
		[PXFieldDescription]
		[PXPersonalDataWarning]
		public override String AcctCD
		{
			get
			{
				return this._AcctCD;
			}
			set
			{
				this._AcctCD = value;
			}
		}
		#endregion
		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
        [PXPersonalDataField]
		public override String AcctName
		{
			get
			{
				return this._AcctName;
			}
			set
			{
				this._AcctName = value;
			}
		}
		#endregion	
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BAccountType.VendorType)]
		[PXUIField(DisplayName = "Type")]
		[BAccountType.List()]
		public override String Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region VendorClassID
		public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
		protected String _VendorClassID;
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<APSetup.dfltVendorClassID>))]
		[PXSelector(typeof(Search2<VendorClass.vendorClassID, LeftJoin<EPEmployeeClass, On<EPEmployeeClass.vendorClassID, Equal<VendorClass.vendorClassID>>>, Where<EPEmployeeClass.vendorClassID, IsNull>>), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Vendor Class")]
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
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.vendor>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.termsID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms")]
		[PXForeignReference(typeof(Field<Vendor.termsID>.IsRelatedTo<Terms.termsID>))]
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
		#region DefPOAddressID
		public abstract class defPOAddressID : PX.Data.BQL.BqlInt.Field<defPOAddressID> { }
		protected Int32? _DefPOAddressID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefPOAddressID
		{
			get
			{
				return this._DefPOAddressID;
			}
			set
			{
				this._DefPOAddressID = value;
			}
		}
		#endregion
        #region Attributes

		[CRAttributesField(typeof(Vendor.vendorClassID), typeof(BAccount.noteID), new[] { typeof(BAccount.classID), typeof(Customer.customerClassID) })]
		public override string[] Attributes { get; set; }

		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.isFinancial, Equal<True>>>), CacheGlobal = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyRateTypeID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region PriceListCuryID
		public abstract class priceListCuryID : PX.Data.BQL.BqlString.Field<priceListCuryID> { }
		protected String _PriceListCuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.curyID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Currency ID")]
		public virtual String PriceListCuryID
		{
			get
			{
				return this._PriceListCuryID;
			}
			set
			{				
				this._PriceListCuryID = value;
			}
		}
		#endregion
		#region DefaultUOM
		public abstract class defaultUOM : PX.Data.BQL.BqlString.Field<defaultUOM> { }
		protected String _DefaultUOM;
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Default UOM")]
		public virtual String DefaultUOM
		{
			get
			{
				return this._DefaultUOM;
			}
			set
			{
				this._DefaultUOM = value;
			}
		}
		#endregion
		#region AllowOverrideCury
		public abstract class allowOverrideCury : PX.Data.BQL.BqlBool.Field<allowOverrideCury> { }
		protected Boolean? _AllowOverrideCury;
		[PXDBBool()]
		[PXUIField(DisplayName = "Enable Currency Override")]
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.allowOverrideCury))]
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
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.allowOverrideRate))]
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
		#region DiscTakenAcctID
		public abstract class discTakenAcctID : PX.Data.BQL.BqlInt.Field<discTakenAcctID> { }
		protected Int32? _DiscTakenAcctID;
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.discTakenAcctID))]
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
		[SubAccount(typeof(Vendor.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.discTakenSubID))]
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
		
		#region PrepaymentAcctID
		public abstract class prepaymentAcctID : PX.Data.BQL.BqlInt.Field<prepaymentAcctID> { }
		protected Int32? _PrepaymentAcctID;
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AP)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prepaymentAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[SubAccount(typeof(Vendor.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prepaymentSubID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        [Account(DisplayName = "PO Accrual Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.PO)]
		[PXDefault(typeof(Search<VendorClass.pOAccrualAcctID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
        [SubAccount(typeof(Vendor.pOAccrualAcctID), DisplayName = "PO Accrual Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<VendorClass.pOAccrualSubID, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prebookAcctID), PersistingCheck = PXPersistingCheck.Nothing)]
        [Account(DisplayName = "Reclassification Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
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
		
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.prebookSubID), PersistingCheck = PXPersistingCheck.Nothing)]
        [SubAccount(typeof(Vendor.prebookAcctID), DisplayName = "Reclassification Subaccount", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
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
		#region PayToParent
		public abstract class payToParent : PX.Data.BQL.BqlBool.Field<payToParent> { }
		protected Boolean? _PayToParent;
		[PXDBBool()]
		[PXUIField(DisplayName = "Pay To Parent Account")]
		[PXDefault(false)]
		public virtual Boolean? PayToParent
		{
			get
			{
				return this._PayToParent;
			}
			set
			{
				this._PayToParent = value;
			}
		}
		#endregion
		#region BaseRemitContactID
		public abstract class baseRemitContactID : PX.Data.BQL.BqlInt.Field<baseRemitContactID> { }
		protected Int32? _BaseRemitContactID;
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Contact.contactID>),
			DirtyRead = true)]
		public virtual Int32? BaseRemitContactID
		{
			get
			{
				return this._BaseRemitContactID;
			}
			set
			{
				this._BaseRemitContactID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		[Obsolete("The field is obsolete and will be removed in Acumatica 8.0.")]
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.taxZoneID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>), DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		public override String TaxZoneID
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
		#region DefLocationID
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		#endregion

		#region DefAddressID
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		#endregion
		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		#endregion
		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { BAccount.status.Active, BAccount.status.Hold, BAccount.status.HoldPayments, BAccount.status.Inactive, BAccount.status.OneTime },
					new string[] { CR.Messages.Active, CR.Messages.Hold, CR.Messages.HoldPayments, CR.Messages.Inactive, CR.Messages.OneTime })
				{ }
			}

			#region Avoid breaking changes
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string Active = BAccount.status.Active;
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string Hold = BAccount.status.Hold;
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string HoldPayments = BAccount.status.HoldPayments;
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string Inactive = BAccount.status.Inactive;
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string OneTime = BAccount.status.OneTime;
			[Obsolete(Common.Messages.FieldIsObsoleteAndWillBeRemoved2020R2)]
			public const string CreditHold = BAccount.status.CreditHold;

			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class active : BAccount.status.active { }
			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class hold : BAccount.status.hold { }
			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class holdPayments : BAccount.status.holdPayments { }
			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class inactive : BAccount.status.inactive { }
			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class oneTime : BAccount.status.oneTime { }
			[Obsolete(Common.Messages.ClassIsObsoleteRemoveInAcumatica2020R2)]
			public class creditHold : BAccount.status.creditHold { }
			#endregion
		}
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[status.List()]
		[PXDefault(BAccount.status.Active)]
		public override String Status
		{
			get
			{
				return this._Status;
			}
			set
			{
				this._Status = value;
			}
		}
		#endregion
		#region Vendor1099
		public abstract class vendor1099 : PX.Data.BQL.BqlBool.Field<vendor1099> { }
		protected Boolean? _Vendor1099;
		[PXDBBool()]
		[PXUIField(DisplayName = "1099 Vendor")]
		[PXDefault(false)]
		public virtual Boolean? Vendor1099
		{
			get
			{
				return this._Vendor1099;
			}
			set
			{
				this._Vendor1099 = value;
			}
		}
		#endregion
		#region Box1099
		public abstract class box1099 : PX.Data.BQL.BqlShort.Field<box1099> { }
		protected Int16? _Box1099;
		[PXDBShort()]
		[Box1099NumberSelector]
		[PXUIField(DisplayName = "1099 Box", Visibility = PXUIVisibility.Visible)]
		public virtual Int16? Box1099
		{
			get
			{
				return this._Box1099;
			}
			set
			{
				this._Box1099 = value;
			}
		}
		#endregion
		#region FATCA
		public abstract class fATCA : PX.Data.BQL.BqlBool.Field<fATCA> { }
		[PXDBBool]
		[PXUIField(DisplayName = "FATCA")]
		[PXUIEnabled(typeof(Vendor.vendor1099))]
		public virtual bool? FATCA { get; set; }
		#endregion
		#region TaxAgency
		public abstract class taxAgency : PX.Data.BQL.BqlBool.Field<taxAgency> { }
		protected Boolean? _TaxAgency;
		[PXDBBool()]
		[PXUIField(DisplayName = "Vendor is Tax Agency")]
		[PXDefault(false)]
		public virtual Boolean? TaxAgency
		{
			get
			{
				return this._TaxAgency;
			}
			set
			{
				this._TaxAgency = value;
			}
		}
		#endregion
        
        #region UpdClosedTaxPeriods
        public abstract class updClosedTaxPeriods : PX.Data.BQL.BqlBool.Field<updClosedTaxPeriods> { }
        protected Boolean? _UpdClosedTaxPeriods;
        [PXDBBool()]
        [PXUIField(DisplayName = "Update Closed Tax Periods")]
        [PXDefault(false)]
        public virtual Boolean? UpdClosedTaxPeriods
        {
            get
            {
                return this._UpdClosedTaxPeriods;
            }
            set
            {
                this._UpdClosedTaxPeriods = value;
            }
        }
        #endregion

        #region TaxReportPrecision
        public abstract class taxReportPrecision : PX.Data.BQL.BqlShort.Field<taxReportPrecision> { }
        protected Int16? _TaxReportPrecision;
        [PXDBShort(MaxValue = 9, MinValue = 0)]
        [PXDefault((short)2, typeof(Search<Currency.decimalPlaces, Where<Currency.curyID, Equal<Current<Vendor.curyID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Tax Report Precision")]
        public virtual Int16? TaxReportPrecision
        {
            get
            {
                return this._TaxReportPrecision;
            }
            set
            {
                this._TaxReportPrecision = value;
            }
        }
        #endregion

        #region TaxReportRounding
        public abstract class taxReportRounding : PX.Data.BQL.BqlString.Field<taxReportRounding> { }
        protected String _TaxReportRounding;
        [PXDBString(1)]
        [PXDefault(RoundingTypes.Mathematical)]
        [PXUIField(DisplayName = "Tax Report Rounding")]
        [PXStringList(new string[] { RoundingTypes.Mathematical, RoundingTypes.Ceil, RoundingTypes.Floor }, new string[] { "Mathematical", "Ceiling", "Floor" })]
        public virtual String TaxReportRounding
        {
            get
            {
                return this._TaxReportRounding;
            }
            set
            {
                this._TaxReportRounding = value;
            }
        }
        #endregion

        #region TaxUseVendorCurPrecision
        public abstract class taxUseVendorCurPrecision : PX.Data.BQL.BqlBool.Field<taxUseVendorCurPrecision> { }
        protected Boolean? _TaxUseVendorCurPrecision;
        [PXDBBool()]
        [PXUIField(DisplayName = "Use Currency Precision")]
        [PXDefault(false)]
        public virtual Boolean? TaxUseVendorCurPrecision
        {
            get
            {
                return this._TaxUseVendorCurPrecision;
            }
            set
            {
                this._TaxUseVendorCurPrecision = value;
            }
        }
        #endregion

		#region TaxReportFinPeriod
		public abstract class taxReportFinPeriod : PX.Data.BQL.BqlBool.Field<taxReportFinPeriod> { }
		protected Boolean? _TaxReportFinPeriod;
		[PXDBBool()]
		[PXUIField(DisplayName = "Define Tax Period by End Date of Financial Period")]
		[PXDefault(false)]
		public virtual Boolean? TaxReportFinPeriod
		{
			get
			{
				return this._TaxReportFinPeriod;
			}
			set
			{
				this._TaxReportFinPeriod = value;
			}
		}
		#endregion

		#region TaxPeriodType
		public abstract class taxPeriodType : PX.Data.BQL.BqlString.Field<taxPeriodType> { }
		protected String _TaxPeriodType;
		[PXDBString(1)]
		[PXDefault(VendorTaxPeriodType.Monthly)]
		[PXUIField(DisplayName = "Default Tax Period Type")]
		[VendorTaxPeriodType.List()]
		public virtual String TaxPeriodType
		{
			get
			{
				return this._TaxPeriodType;
			}
			set
			{
				this._TaxPeriodType = value;
			}
		}
		#endregion
		#region AutoGenerateTaxBill
		public abstract class autoGenerateTaxBill : PX.Data.BQL.BqlBool.Field<autoGenerateTaxBill> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Automatically Generate Tax Bill")]
		[PXDefault(true)]
		public virtual bool? AutoGenerateTaxBill { get; set; }

		#endregion

		#region SalesTaxAcctID
		public abstract class salesTaxAcctID : PX.Data.BQL.BqlInt.Field<salesTaxAcctID> { }
		protected Int32? _SalesTaxAcctID;
		[Account(DisplayName = "Tax Payable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		public virtual Int32? SalesTaxAcctID
		{
			get
			{
				return this._SalesTaxAcctID;
			}
			set
			{
				this._SalesTaxAcctID = value;
			}
		}
		#endregion
		#region SalesTaxSubID
		public abstract class salesTaxSubID : PX.Data.BQL.BqlInt.Field<salesTaxSubID> { }
		protected Int32? _SalesTaxSubID;
		[SubAccount(typeof(Vendor.salesTaxAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Payable Sub.")]
		public virtual Int32? SalesTaxSubID
		{
			get
			{
				return this._SalesTaxSubID;
			}
			set
			{
				this._SalesTaxSubID = value;
			}
		}
		#endregion
		#region PurchTaxAcctID
		public abstract class purchTaxAcctID : PX.Data.BQL.BqlInt.Field<purchTaxAcctID> { }
		protected Int32? _PurchTaxAcctID;
		[Account(DisplayName = "Tax Claimable Account", DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.TX)]
		public virtual Int32? PurchTaxAcctID
		{
			get
			{
				return this._PurchTaxAcctID;
			}
			set
			{
				this._PurchTaxAcctID = value;
			}
		}
		#endregion
		#region PurchTaxSubID
		public abstract class purchTaxSubID : PX.Data.BQL.BqlInt.Field<purchTaxSubID> { }
		protected Int32? _PurchTaxSubID;
		[SubAccount(typeof(Vendor.purchTaxAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Claimable Sub.")]
		public virtual Int32? PurchTaxSubID
		{
			get
			{
				return this._PurchTaxSubID;
			}
			set
			{
				this._PurchTaxSubID = value;
			}
		}
		#endregion
		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.BQL.BqlInt.Field<taxExpenseAcctID> { }
		protected Int32? _TaxExpenseAcctID;
		[Account(DisplayName = "Tax Expense Account", DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		public virtual Int32? TaxExpenseAcctID
		{
			get
			{
				return this._TaxExpenseAcctID;
			}
			set
			{
				this._TaxExpenseAcctID = value;
			}
		}
		#endregion
		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.BQL.BqlInt.Field<taxExpenseSubID> { }
		protected Int32? _TaxExpenseSubID;
		[SubAccount(typeof(Vendor.taxExpenseAcctID), DescriptionField = typeof(Sub.description), DisplayName = "Tax Expense Sub.")]
		public virtual Int32? TaxExpenseSubID
		{
			get
			{
				return this._TaxExpenseSubID;
			}
			set
			{
				this._TaxExpenseSubID = value;
			}
		}
		#endregion
		#region GroupMask
		public new abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected new Byte[] _GroupMask;
		[PXDBGroupMask(BqlTable = typeof(Vendor))]
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.groupMask), PersistingCheck = PXPersistingCheck.Nothing)]
		public new virtual Byte[] GroupMask
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
		#region OwnerID
		public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
		[PXDBGuid()]
		[PX.TM.PXOwnerSelector(typeof(workgroupID))]
		[PXUIField(DisplayName = "Owner", Visibility = PXUIVisibility.Invisible)]
		public override Guid? OwnerID
		{
			get
			{
				return this._OwnerID;
			}
			set
			{
				this._OwnerID = value;
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXSearchable(SM.SearchCategory.AP | SM.SearchCategory.PO | SM.SearchCategory.AR | SM.SearchCategory.SO | SM.SearchCategory.CR, Messages.SearchableTitleVendor, new Type[] { typeof(Vendor.acctName) },
		   new Type[] { typeof(Vendor.acctName), typeof(Vendor.acctCD), typeof(Vendor.acctName), typeof(Vendor.acctCD), typeof(Vendor.defContactID), typeof(Contact.displayName), typeof(Contact.eMail), typeof(Contact.phone1), typeof(Contact.phone2), typeof(Contact.phone3), typeof(Contact.webSite) },
			NumberFields = new Type[] { typeof(Vendor.acctCD) },
			 Line1Format = "{0}{2}{3}", Line1Fields = new Type[] { typeof(Vendor.acctCD), typeof(Vendor.defContactID), typeof(Contact.eMail), typeof(Contact.phone1), typeof(Contact.phone2), typeof(Contact.phone3) },
			 Line2Format = "{1}{2}{3}", Line2Fields = new Type[] { typeof(Vendor.defAddressID), typeof(Address.displayName), typeof(Address.city), typeof(Address.state) },
			SelectForFastIndexing = typeof(Select2<Vendor, InnerJoin<Contact, On<Contact.contactID, Equal<Vendor.defContactID>>>>),
			WhereConstraint = typeof(Where<Vendor.type.IsIn<BAccountType.vendorType, BAccountType.combinedType>>)
		 )]
		[PXUniqueNote(
			DescriptionField = typeof(Vendor.acctCD),
			Selector = typeof(VendorR.acctCD),
			ActivitiesCountByParent = true,
			ShowInReferenceSelector = true,
            PopupTextEnabled = true)]
		public override Guid? NoteID
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
		#region LandedCostVendor
		public abstract class landedCostVendor : PX.Data.BQL.BqlBool.Field<landedCostVendor> { }
		protected Boolean? _LandedCostVendor;
		[PXDBBool()]
		[PXUIField(DisplayName = "Landed Cost Vendor")]
		[PXDefault(false)]
		public virtual Boolean? LandedCostVendor
		{
			get
			{
				return this._LandedCostVendor;
			}
			set
			{
				this._LandedCostVendor = value;
			}
		}
		#endregion
		#region IsLaborUnion
		public abstract class isLaborUnion : PX.Data.BQL.BqlBool.Field<isLaborUnion> { }
		protected Boolean? _IsLaborUnion;
		[PXDBBool()]
		[PXUIField(DisplayName = "Vendor is Labor Union", Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXDefault(false)]
        [Obsolete(Common.Messages.ObsoletePayrollFieldToRemove)]
		public virtual Boolean? IsLaborUnion
		{
			get
			{
				return this._IsLaborUnion;
			}
			set
			{
				this._IsLaborUnion = value;
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

        #region LineDiscountTarget
        public abstract class lineDiscountTarget : PX.Data.BQL.BqlString.Field<lineDiscountTarget> { }
        protected String _LineDiscountTarget;
        [PXDBString(1, IsFixed = true)]
        [LineDiscountTargetType.List()]
        [PXDefault(LineDiscountTargetType.ExtendedPrice)]
        [PXUIField(DisplayName = "Apply Line Discounts to", Visibility = PXUIVisibility.Visible, Required = true)]
        public virtual String LineDiscountTarget
        {
            get
            {
                return this._LineDiscountTarget;
            }
            set
            {
                this._LineDiscountTarget = value;
            }
        }
        #endregion
        #region IgnoreConfiguredDiscounts
        public abstract class ignoreConfiguredDiscounts : PX.Data.BQL.BqlBool.Field<ignoreConfiguredDiscounts> { }
        protected Boolean? _IgnoreConfiguredDiscounts;
        [PXDBBool()]
        [PXUIField(DisplayName = "Ignore Configured Discounts When Vendor Price is Defined")]
        [PXDefault(false)]
        public virtual Boolean? IgnoreConfiguredDiscounts
        {
            get
            {
                return this._IgnoreConfiguredDiscounts;
            }
            set
            {
                this._IgnoreConfiguredDiscounts = value;
            }
        }
        #endregion

        #region ForeignEntity
        public abstract class foreignEntity : PX.Data.BQL.BqlBool.Field<foreignEntity> { }
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Foreign Entity")]
        public virtual bool? ForeignEntity { get; set; }
        #endregion
		#region ClassID
		[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public override String ClassID
		{
			get { return this.VendorClassID; }
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
		[PXDefault(typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.localeName), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string LocaleName { get; set; }
		#endregion

		#region SVATReversalMethod
		public abstract class sVATReversalMethod : PX.Data.BQL.BqlString.Field<sVATReversalMethod> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(SVATTaxReversalMethods.OnDocuments)]
		[SVATTaxReversalMethods.List]
		[PXUIField(DisplayName = "VAT Recognition Method")]
		public virtual string SVATReversalMethod
		{
			get;
			set;
		}
		#endregion
		#region SVATInputTaxEntryRefNbr
		public abstract class sVATInputTaxEntryRefNbr : PX.Data.BQL.BqlString.Field<sVATInputTaxEntryRefNbr> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(VendorSVATTaxEntryRefNbr.ManuallyEntered)]
		[VendorSVATTaxEntryRefNbr.InputList]
		[PXUIField(DisplayName = "Input Tax Entry Ref. Nbr.")]
		public virtual string SVATInputTaxEntryRefNbr
		{
			get;
			set;
		}
		#endregion
		#region SVATOutputTaxEntryRefNbr
		public abstract class sVATOutputTaxEntryRefNbr : PX.Data.BQL.BqlString.Field<sVATOutputTaxEntryRefNbr> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(VendorSVATTaxEntryRefNbr.ManuallyEntered)]
		[VendorSVATTaxEntryRefNbr.OutputList]
		[PXUIField(DisplayName = "Output Tax Entry Ref. Nbr.")]
		public virtual string SVATOutputTaxEntryRefNbr
		{
			get;
			set;
		}
		#endregion
		#region SVATTaxInvoiceNumberingID
		public abstract class sVATTaxInvoiceNumberingID : PX.Data.BQL.BqlString.Field<sVATTaxInvoiceNumberingID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(Numbering.numberingID), DescriptionField = typeof(Numbering.descr))]
		[PXUIField(DisplayName = "Tax Invoice Numbering")]
		public virtual string SVATTaxInvoiceNumberingID
		{
			get;
			set;
		}
		#endregion

		#region PayToVendorID
		public abstract class payToVendorID : PX.Data.BQL.BqlInt.Field<payToVendorID> { }
		/// <summary>
		/// A reference to the <see cref="Vendor"/>.
		/// </summary>
		/// <value>
		/// An integer identifier of the vendor, whom the AP bill will belong to. 
		/// </value>
		[PayToVendor(CacheGlobal = true, Filterable = true)]
		[PXForeignReference(typeof(Field<payToVendorID>.IsRelatedTo<BAccount.bAccountID>))]
		public virtual int? PayToVendorID { get; set; }
		#endregion

		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.retainageApply), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? RetainageApply
		{
			get;
			set;
		}
		#endregion
		#region RetainagePct
		public abstract class retainagePct : PX.Data.BQL.BqlDecimal.Field<retainagePct> { }

		[PXDBDecimal(6, MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Retainage Percent", Visibility = PXUIVisibility.Visible, FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<Vendor.retainageApply>))]
		public virtual decimal? RetainagePct
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
		[PXDefault(false, typeof(Select<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>>), SourceField = typeof(VendorClass.paymentsByLinesAllowed), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion
	}

	public static class LineDiscountTargetType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { ExtendedPrice, SalesPrice },
				new string[] { Messages.ExtendedPrice, Messages.SalesPrice })
			{; }
		}
		public const string ExtendedPrice = "E";
		public const string SalesPrice = "S";
	}	

	[Obsolete("This is an absolete attribute. It will be removed in 2019R2")]
	public class Box1099NumberAttribute : PXIntListAttribute
	{
		protected AP1099BoxDefinition Definition;

		public Box1099NumberAttribute()
			: base(new int[] { 0 }, new string[] { "undefined" })
		{
		}

		public override void CacheAttached(PXCache sender)
		{
			BuildLists();

			base.CacheAttached(sender);
		}

		private void BuildLists()
		{
			Definition = PXDatabase.GetSlot<AP1099BoxDefinition>(typeof(AP1099BoxDefinition).FullName, typeof(AP1099Box));

			if (!Definition.AP1099Boxes.Any())
				return;

			_AllowedValues = Definition.AP1099Boxes.Select(kvp => (int)kvp.Key).ToArray();
			_AllowedLabels = Definition.AP1099Boxes.Select(kvp => string.Concat(kvp.Key, "-", kvp.Value)).ToArray();
			_NeutralAllowedLabels = _AllowedLabels;
		}

		protected class AP1099BoxDefinition : IPrefetchable
		{
			public Dictionary<short, string> AP1099Boxes = new Dictionary<short, string>();

			public void Prefetch()
			{
				foreach (PXDataRecord record in PXDatabase.SelectMulti(typeof(AP1099Box),
																		new PXDataField(typeof(AP1099Box.boxNbr).Name),
																		new PXDataField(typeof(AP1099Box.descr).Name)))
				{
					AP1099Boxes[record.GetInt16(0).Value] = record.GetString(1);
				}
			}
		}
	}

	public class Box1099NumberSelectorAttribute : PXSelectorAttribute
	{
		public Box1099NumberSelectorAttribute()
			: base(typeof(AP1099Box.boxNbr), typeof(AP1099Box.boxNbr), typeof(AP1099Box.descr))
		{
			DescriptionField = typeof(AP1099Box.descr);
		}
	}
}
