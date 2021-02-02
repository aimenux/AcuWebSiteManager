using System;

using PX.Data;
using PX.Data.EP;

using PX.SM;

using PX.Objects.Common;
using PX.Objects.AP;
using PX.Objects.CR.MassProcess;
using PX.Objects.GL;
using PX.Objects.CA;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.CR;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AR
{
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CustomerMaster)]
	public partial class CustomerMaster : Customer
	{
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[Customer(IsKey = true, DisplayName = "Customer ID")]
		public override Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDBString(30, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region StatementCycleID
		public new abstract class statementCycleId : PX.Data.BQL.BqlString.Field<statementCycleId> { }
		#endregion
	}


	[System.SerializableAttribute()]
	[PXTable(typeof(BAccount.bAccountID))]
	[CRCacheIndependentPrimaryGraphList(new Type[]{
		typeof(CR.BusinessAccountMaint),
		typeof(AR.CustomerMaint),
		typeof(AR.CustomerMaint),
		typeof(AR.CustomerMaint),
		typeof(CR.BusinessAccountMaint)},
		new Type[]{
            typeof(Select<CR.BAccount, Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
                    And<Current<BAccount.viewInCrm>, Equal<True>>>>),
			typeof(Select<AR.Customer, Where<AR.Customer.bAccountID, Equal<Current<BAccount.bAccountID>>, Or<AR.Customer.bAccountID, Equal<Current<BAccountR.bAccountID>>>>>),
			typeof(Select<AR.Customer, Where<AR.Customer.acctCD, Equal<Current<BAccount.acctCD>>, Or<AR.Customer.acctCD, Equal<Current<BAccountR.acctCD>>>>>),			
			typeof(Where<CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.customerType>>>),
			typeof(Select<CR.BAccount,
				Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
					Or<Current<BAccount.bAccountID>, Less<Zero>>>>)
		})]
	[PXCacheName(Messages.Customer)]
	[PXEMailSource]
	public partial class Customer : BAccount, PX.SM.IIncludable
	{
		#region Keys
		public new class PK : PrimaryKeyOf<Customer>.By<bAccountID>
		{
			public static Customer Find(PXGraph graph, int? bAccountID) => FindBy(graph, bAccountID);
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion
		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		/// <summary>
		/// The human-readable identifier of the customer account, which is
		/// specified by the user or defined by the auto-numbering sequence during
		/// creation of the customer. This field is a natural key, as opposed
		/// to the surrogate key <see cref="BAccount.BAccountID"/>.
		/// </summary>
		[CustomerRaw(IsKey = true)]
		[PXDefault]
		[PXFieldDescription]
		[PXPersonalDataWarning]
		public override string AcctCD
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
		#region Type
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		/// <summary>
		/// Represents the type of the business account of the customer.
		/// The field defaults to <see cref="BAccountType.CustomerType"/>;
		/// however, the field can have a value of <see cref="BAccountType.CombinedType"/> 
		/// if the customer account has been extended to this type.
		/// </summary>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BAccountType.CustomerType)]
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
		#region IsCustomerOrCombined
		public new abstract class isCustomerOrCombined : PX.Data.BQL.BqlBool.Field<isCustomerOrCombined> { }

		/// <summary>
		/// A calculated field that indicates (if set to <c>true</c>) that <see cref="Customer.Type"/> is
		/// either <see cref="BAccountType.CustomerType"/> or <see cref="BAccountType.CombinedType"/>.
		/// The field is inherited from the <see cref="BAccount"/> class and must always return
		/// <c>true</c> for a customer account.
		/// </summary>
		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(Switch<Case<Where<BAccount.type, Equal<BAccountType.customerType>, Or<BAccount.type, Equal<BAccountType.combinedType>>>, True>, False>), typeof(bool))]
		public override bool? IsCustomerOrCombined { get; set; }
		#endregion
		#region ParentBAccountID
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		#endregion
		#region ConsolidateToParent
		public new abstract class consolidateToParent : PX.Data.BQL.BqlBool.Field<consolidateToParent> { }
		#endregion
		#region ConsolidateStatements
		public abstract class consolidateStatements : PX.Data.BQL.BqlBool.Field<consolidateStatements> { }

		/// <summary>
		/// When set to true indicates that consolidated statements are prepared for the customer and
		/// its parent and siblings. Otherwise, individual statements are prepared.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Statements")]
		public virtual bool? ConsolidateStatements { get; set; }
		#endregion
		#region SharedCreditPolicy
		public abstract class sharedCreditPolicy : PX.Data.BQL.BqlBool.Field<sharedCreditPolicy> { }

		/// <summary>
		/// If set to <c>true</c>, indicates that:
		/// <list type="number"> 
		/// <item><description>
		/// Credit control is enabled at the parent level; that is, the group 
		/// credit verification settings are specified for the parent account.
		/// </description></item>
		/// <item><description>
		/// Dunning letters are consolidated to the parent account.
		/// </description></item>
		/// </list>
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Share Credit Policy")]
		public virtual bool? SharedCreditPolicy { get; set; }
		#endregion
		#region ConsolidatingBAccountID
		public new abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }
		#endregion
		#region StatementCustomerID
		public abstract class statementCustomerID : PX.Data.BQL.BqlInt.Field<statementCustomerID> { }

        /// <summary>
        /// Identifier of the customer, whose statements include data for this customer.
        /// </summary>
        /// <value>
        /// When <see cref="Customer.ConsolidateStatements"/> is true, this field holds the ID of the parent customer (if present).
        /// When <see cref="Customer.ConsolidateStatements"/> is false, individual statements are prepared
        /// and this field is equal to the ID of this customer.
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// The field is populated by a formula, working only in the scope of the Customers (AR303000) form. 
        /// See <see cref="CustomerMaint.Customer_StatementCustomerID_CacheAttached"/>"
        /// </value>
        [PXDBInt]
		public virtual int? StatementCustomerID { get; set; }
		#endregion
		#region SharedCreditCustomerID
		public abstract class sharedCreditCustomerID : PX.Data.BQL.BqlInt.Field<sharedCreditCustomerID> { }

        /// <summary>
        /// Identifier of the customer, through which the credit control is set up and maintained for this customer.
        /// </summary>
        /// <value>
        /// When <see cref="Customer.SharedCreditPolicy"/> is true, this field holds the ID of the parent customer (if present).
        /// When <see cref="Customer.SharedCreditPolicy"/> is false, credit control is executed individually for this customer
        /// and this field is equal to its ID.
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// The field is populated by a formula, working only in the scope of the Customers (AR303000) form. 
        /// See <see cref="CustomerMaint.Customer_SharedCreditCustomerID_CacheAttached"/>"
        /// </value>
        [PXDBInt]
		public virtual int? SharedCreditCustomerID { get; set; }
		#endregion
        #region Attributes
		/// <summary>
		/// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
		/// added attributes</see> defined at the <see cref="CustomerClass">customer 
		/// class</see> level to function correctly.
		/// </summary>
	    [CRAttributesField(typeof (Customer.customerClassID), typeof (BAccount.noteID), new[] { typeof(BAccount.classID), typeof(Vendor.vendorClassID)})]
		public override string[] Attributes { get; set; }
		#endregion
		#region CustomerClassID
		public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
		protected String _CustomerClassID;
		/// <summary>
		/// Identifier of the <see cref="CustomerClass">customer class</see> 
		/// to which the customer belongs.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXDefault(typeof(Search<ARSetup.dfltCustomerClassID>))]
		[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
		[PXUIField(DisplayName = "Customer Class")]
		[PXForeignReference(typeof(Field<Customer.customerClassID>.IsRelatedTo<CustomerClass.customerClassID>))]
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
		#region LanguageID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class languageID : PX.Data.BQL.BqlString.Field<languageID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDBString(4, IsFixed = true)]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual string LanguageID
		{
			get;
			set;
		}
		#endregion
		#region DefSOAddressID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class defSOAddressID : PX.Data.BQL.BqlInt.Field<defSOAddressID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual int? DefSOAddressID
		{
			get;
			set;
		}
		#endregion
		#region DefBillAddressID
		public abstract class defBillAddressID : PX.Data.BQL.BqlInt.Field<defBillAddressID> { }
		protected Int32? _DefBillAddressID;
		/// <summary>
		/// The billing <see cref="Address"/> associated with the customer.
		/// </summary>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefBillAddressID
		{
			get
			{
				return this._DefBillAddressID;
			}
			set
			{
				this._DefBillAddressID = value;
			}
		}
		#endregion
		#region DefBillContactID
		public abstract class defBillContactID : PX.Data.BQL.BqlInt.Field<defBillContactID> { }
		protected Int32? _DefBillContactID;
		/// <summary>
		/// The billing <see cref="Contact"/> associated with the customer.
		/// </summary>
		[PXDBInt()]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>), DirtyRead = true)]
		public virtual Int32? DefBillContactID
		{
			get
			{
				return this._DefBillContactID;
			}
			set
			{
				this._DefBillContactID = value;
			}
		}
		#endregion
		#region BaseBillContactID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class baseBillContactID : PX.Data.BQL.BqlInt.Field<baseBillContactID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID, Where<Contact.bAccountID,
					Equal<Current<Customer.bAccountID>>,
					And<Contact.contactType, Equal<ContactTypesAttribute.person>>>>))]
       	[PXUIField(DisplayName = "Default Contact")]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual int? BaseBillContactID
		{
			get;
			set;
		}
		#endregion
		#region TaxZoneID
		public new abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		
		[Obsolete("The field is obsolete and will be removed in Acumatica 8.0.")]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXDefault(typeof(Search<CustomerClass.taxZoneID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		#region TermsID
		public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
		protected String _TermsID;
		/// <summary>
		/// The identifier of the default <see cref="Terms">terms</see>, 
		/// which are applied to the documents of the customer.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXSelector(typeof(Search<Terms.termsID, Where<Terms.visibleTo, Equal<TermsVisibleTo.customer>, Or<Terms.visibleTo, Equal<TermsVisibleTo.all>>>>), DescriptionField = typeof(Terms.descr), CacheGlobal = true)]
		[PXDefault(typeof(Search<CustomerClass.termsID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Terms")]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		/// <summary>
		/// The identifier of the <see cref="Currency"/>,
		/// which is applied to the documents of the customer.
		/// </summary>
		[PXDBString(5, IsUnicode = true)]
		[PXSelector(typeof(Currency.curyID), CacheGlobal = true)]
		[PXUIField(DisplayName = "Currency ID")]
		[PXDefault(typeof(Search<CustomerClass.curyID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <summary>
		/// The identifier of the currency rate type,
		/// which is applied to the documents of the customer.
		/// </summary>
		[PXDBString(6, IsUnicode = true)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXDefault(typeof(Search<CustomerClass.curyRateTypeID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that the currency 
		/// of customer documents (which is specified by <see cref="Customer.CuryID"/>)
		/// can be overridden by a user during document entry.
		/// /// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.allowOverrideCury, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Enable Currency Override")]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that the currency rate
		/// for customer documents (which is calculated by the system 
		/// from the currency rate history) can be overridden by a user 
		/// during document entry.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.allowOverrideRate, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Enable Rate Override")]
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
		/// <summary>
		/// The account that is used to process the amounts of 
		/// cash discount taken by the customer.
		/// </summary>
		[PXDefault(typeof(Search<CustomerClass.discTakenAcctID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
		[Account(DisplayName = "Cash Discount Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
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
		/// <summary>
		/// The subaccount that is used to process the amounts of
		/// cash discount taken by the customer.
		/// </summary>
		[PXDefault(typeof(Search<CustomerClass.discTakenSubID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>))]
		[SubAccount(typeof(Customer.discTakenAcctID), DisplayName = "Cash Discount Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]

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
		/// <summary>
		/// The identifier of the <see cref="Account">account</see> that serves
		/// as the default value of the <see cref="ARRegister.ARAccountID"/> 
		/// field for the prepayment documents.
		/// </summary>
		[Account(DisplayName = "Prepayment Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), ControlAccountForModule = ControlAccountModule.AR)]
		[PXDefault(typeof(Search<CustomerClass.prepaymentAcctID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <summary>
		/// The identifier of the <see cref="Sub">subaccount</see>
		/// that serves as the default value of the <see cref="ARRegister.ARSubID"/>
		/// field for the prepayment documents.
		/// </summary>
		[SubAccount(typeof(Customer.prepaymentAcctID), DisplayName = "Prepayment Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<CustomerClass.prepaymentSubID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region COGSAcctID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class cOGSAcctID : PX.Data.BQL.BqlInt.Field<cOGSAcctID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDefault(typeof(Search<CustomerClass.cOGSAcctID,Where<CustomerClass.customerClassID,Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[Account(DisplayName = "COGS Account", Visibility=PXUIVisibility.Visible, DescriptionField=typeof(Account.description))]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual int? COGSAcctID
		{
			get;
			set;
		}
		#endregion
		#region COGSSubID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class cOGSSubID : PX.Data.BQL.BqlInt.Field<cOGSSubID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDefault(typeof(Search<CustomerClass.cOGSSubID, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[SubAccount(typeof(Customer.cOGSAcctID), DisplayName = "COGS Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual int? COGSSubID
		{
			get;
			set;
		}
		#endregion

		#region AutoApplyPayments
		public abstract class autoApplyPayments : PX.Data.BQL.BqlBool.Field<autoApplyPayments> { }
		protected Boolean? _AutoApplyPayments;
		/// <summary>
		/// If set to <c>true</c>, indicates that the payments of the customer
		/// should be automatically applied to the open invoices upon release.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.autoApplyPayments, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that customer
		/// statements should be printed for the customer.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Print Statements")]
		[PXDefault(false, typeof(Search<CustomerClass.printStatements, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that customer
		/// statements should be generated for the customer in 
		/// multi-currency format.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Multi-Currency Statements")]
		[PXDefault(false, typeof(Search<CustomerClass.printCuryStatements, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that customer
		/// statements should be sent to the customer by email.
		/// </summary>
		public abstract class sendStatementByEmail : PX.Data.BQL.BqlBool.Field<sendStatementByEmail> { }
		[PXDBBool]
		[PXDefault(false, typeof(Search<
			CustomerClass.sendStatementByEmail, 
			Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		[PXUIField(DisplayName = "Send Statements by Email")]
		public virtual bool? SendStatementByEmail
		{
			get;
			set;
		}
		#endregion
		#region CreditRule
		public abstract class creditRule : PX.Data.BQL.BqlString.Field<creditRule> { }
		protected String _CreditRule;
		/// <summary>
		/// The type of credit verification for the customer.
		/// The list of possible values of the field is determined
		/// by <see cref="CreditRuleAttribute"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[CreditRule()]
		[PXDefault(typeof(Search<CustomerClass.creditRule, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If <see cref="Customer.CreditRule"/> enables verification by credit limit,
		/// this field determines the maximum amount of credit allowed for the customer.
		/// </summary>
		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<CustomerClass.creditLimit, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <summary>
		/// If <see cref="Customer.CreditRule"/> enables verification by days past due,
		/// this field determines the maximum number of credit days past due 
		/// allowed for the customer. The actual number of days past due is 
		/// calculated from the due date of the earliest open customer invoice 
		/// (which is specified by <see cref="ARBalances.OldInvoiceDate"/>).
		/// </summary>
		[PXDBShort(MinValue = 0, MaxValue = 3650)]
		[PXUIField(DisplayName = "Credit Days Past Due")]
		[PXDefault(TypeCode.Int16, "0", typeof(Search<CustomerClass.creditDaysPastDue, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
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
		/// <summary>
		/// The type of customer statements generated for the customer.
		/// The list of possible values of the field is determined by 
		/// <see cref="StatementTypeAttribute"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(typeof(Search<CustomerClass.statementType, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// The identifier of the <see cref="ARStatementCycle">statement cycle</see>
		/// to which the customer is assigned.
		/// </summary>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Statement Cycle ID")]
		[PXSelector(typeof(ARStatementCycle.statementCycleId))]
		[PXDefault(typeof(Search<CustomerClass.statementCycleId, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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

		#region StatementLastDate
		public abstract class statementLastDate : PX.Data.BQL.BqlDateTime.Field<statementLastDate> { }
		protected DateTime? _StatementLastDate;
		/// <summary>
		/// The date when the statements were last generated for the customer.
		/// </summary>
		[PXDBDate()]
		[PXUIField(DisplayName = "Statement Last Date",Enabled = false)]
		public virtual DateTime? StatementLastDate
		{
			get
			{
				return this._StatementLastDate;
			}
			set
			{
				this._StatementLastDate = value;
			}
		}
		#endregion

		#region SmallBalanceAllow
		public abstract class smallBalanceAllow : PX.Data.BQL.BqlBool.Field<smallBalanceAllow> { }
		protected Boolean? _SmallBalanceAllow;
		/// <summary>
		/// If set to <c>true</c>, indicates that small balance
		/// write-offs are allowed for the customer.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.smallBalanceAllow, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If <see cref="SmallBalanceAllow"/> is set to <c>true</c>, the
		/// field determines the maximum small balance write-off limit for 
		/// customer documents.
		/// </summary>
		[PXDBCury(typeof(Customer.curyID), MinValue = 0.0)]
		[PXDefault(TypeCode.Decimal, "0.0", typeof(Search<CustomerClass.smallBalanceLimit, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
		[PXUIField(DisplayName = "Write-Off Limit", Enabled = false)]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that financial charges
		/// can be calculated for the customer.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false, typeof(Search<CustomerClass.finChargeApply, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		#region PayToParent
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class payToParent : PX.Data.BQL.BqlBool.Field<payToParent> { }
		protected Boolean? _PayToParent;
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Bill To Parent")]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
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
		#region IsBillAddressSameAsMain
		public abstract class isBillSameAsMain : PX.Data.BQL.BqlBool.Field<isBillSameAsMain> { }
        /// <summary>
        /// A calculated field. If set to <c>true</c>, indicates that
        /// the customer's billing address is the same as the customer's
        /// default address.
        /// The field is populated by a formula, working only in the scope of the Customers (AR303000) form. 
        /// See <see cref="CustomerMaint.Customer_IsBillSameAsMain_CacheAttached"/>"
        /// </summary>
        [PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsBillSameAsMain
		{
			get;
			set;
		}
		#endregion
		#region IsBillContSameAsMain
		public abstract class isBillContSameAsMain : PX.Data.BQL.BqlBool.Field<isBillContSameAsMain> { }
        /// <summary>
        /// A calculated field. If set to <c>true</c>, indicates that the 
        /// customer's billing contact is the same as the customer's
        /// default contact.
        /// The field is populated by a formula, working only in the scope of the Customers (AR303000) form. 
        /// See <see cref="CustomerMaint.Customer_IsBillContSameAsMain_CacheAttached"/>"
        /// </summary>
        [PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
		public virtual bool? IsBillContSameAsMain
		{
			get;
			set;
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
					new string[] { BAccount.status.Active, BAccount.status.Hold, BAccount.status.CreditHold, BAccount.status.Inactive, BAccount.status.OneTime },
					new string[] { CR.Messages.Active, CR.Messages.Hold, CR.Messages.CreditHold, CR.Messages.Inactive, CR.Messages.OneTime }) { }
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
		/// <summary>
		/// The status of the business account. The field can have one of the 
		/// values specified by <see cref="status.ListAttribute"/>.
		/// </summary>
		[PXDBString(1, IsFixed = true)]
		[PXDefault(BAccount.status.Active)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[status.List()]
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

		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		/// <summary>
		/// The full business account name (as opposed to the 
		/// short identifier provided by <see cref="Customer.AcctCD"/>).
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region GroupMask
		public new abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected new Byte[] _GroupMask;
		/// <summary>
		/// The group mask of the customer. The value of the field 
		/// is used for the purposes of access control.
		/// </summary>
		[PXDBGroupMask(BqlTable = typeof(Customer))]
		[PXDefault(typeof(Select<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), SourceField = typeof(CustomerClass.groupMask), PersistingCheck = PXPersistingCheck.Nothing)]
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

        #region PaymentMethodID
        public abstract class defPaymentMethodID : PX.Data.BQL.BqlString.Field<defPaymentMethodID> { }
        protected String _DefPaymentMethodID;
		/// <summary>
		/// The identifier of the customer's default <see cref="PaymentMethod"/>.
		/// </summary>
        [PXDBString(10, IsUnicode = true)]
        [PXDefault(typeof(Search<CustomerClass.defPaymentMethodID,
                                   Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXSelector(typeof(Search2<PaymentMethod.paymentMethodID, LeftJoin<CustomerPaymentMethod, On<CustomerPaymentMethod.paymentMethodID, Equal<PaymentMethod.paymentMethodID>,
									And<CustomerPaymentMethod.bAccountID, Equal<Current<Customer.bAccountID>>>>>,
                                Where<Where<PaymentMethod.isActive, Equal<True>,
                                And<PaymentMethod.useForAR, Equal<True>,                                
                                    Or<Where<CustomerPaymentMethod.pMInstanceID, IsNotNull>>>>>>), DescriptionField = typeof(PaymentMethod.descr))]
        [PXUIField(DisplayName = "Default Payment Method", Enabled = false)]
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
		#region CCProcessingID
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public abstract class cCProcessingID : PX.Data.BQL.BqlString.Field<cCProcessingID> { }
		/// <summary>
		/// An obsolete field.
		/// </summary>
		[PXDBString(1024, IsUnicode = true)]
		[Obsolete("This field is not used anymore and will be removed in Acumatica 8.0.")]
		public virtual string CCProcessingID { get; set; }
		#endregion
		#region DefPMInstanceID
		public abstract class defPMInstanceID : PX.Data.BQL.BqlInt.Field<defPMInstanceID> { }
		protected Int32? _DefPMInstanceID;
		/// <summary>
		/// The unique identifier of the <see cref="PMInstance"/> object
		/// associated with the customer's <see cref="Customer.DefPaymentMethodID">
		/// default payment method</see>.
		/// </summary>
		[PXDefault(typeof(Search<PaymentMethod.pMInstanceID, Where<PaymentMethod.paymentMethodID, Equal<Current<Customer.defPaymentMethodID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBInt()]
		[PXDBChildIdentity(typeof(CustomerPaymentMethod.pMInstanceID))]
		public virtual Int32? DefPMInstanceID
		{
			get
			{
				return this._DefPMInstanceID;
			}
			set
			{
				this._DefPMInstanceID = value;
			}
		}
		#endregion
		#region PrintInvoices
		public abstract class printInvoices : PX.Data.BQL.BqlBool.Field<printInvoices> { }
		protected Boolean? _PrintInvoices;
		/// <summary>
		/// If set to <c>true</c>, indicates that invoices
		/// should be printed for the customer.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Print Invoices")]
		[PXDefault(false, typeof(Search<CustomerClass.printInvoices, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that invoices
		/// should be sent to the customer by email.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Send Invoices by Email")]
		[PXDefault(false, typeof(Search<CustomerClass.mailInvoices, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		/// <summary>
		/// The unique identifier of the <see cref="Note">note</see> 
		/// associated with the customer account.
		/// </summary>
		[PXSearchable(SM.SearchCategory.AP | SM.SearchCategory.PO | SM.SearchCategory.AR | SM.SearchCategory.SO | SM.SearchCategory.CR, Messages.SearchableTitleCustomer, new Type[] { typeof(Customer.acctName) },
			new Type[] { typeof(Customer.acctName), typeof(Customer.acctCD), typeof(Customer.acctName), typeof(Customer.acctCD), typeof(Customer.defContactID), typeof(Contact.displayName), typeof(Contact.eMail),
                         typeof(Contact.phone1), typeof(Contact.phone2), typeof(Contact.phone3), typeof(Contact.webSite)},
			NumberFields = new Type[] { typeof(Customer.acctCD) },
              Line1Format = "{0}{2}{3}{4}", Line1Fields = new Type[] { typeof(Customer.acctCD), typeof(Customer.defContactID), typeof(Contact.displayName), typeof(Contact.phone1), typeof(Contact.eMail) },
			  Line2Format = "{1}{2}{3}", Line2Fields = new Type[] { typeof(Customer.defAddressID), typeof(Address.displayName), typeof(Address.city), typeof(Address.state)},
			SelectForFastIndexing = typeof(Select2<Customer, InnerJoin<Contact, On<Contact.contactID, Equal<Customer.defContactID>>>>)
		  )]
		[PXUniqueNote(
			DescriptionField = typeof(Customer.acctCD),
			Selector = typeof(Customer.acctCD),
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

         #region PrintDunningLetters
         public abstract class printDunningLetters : PX.Data.BQL.BqlBool.Field<printDunningLetters> { }
         protected Boolean? _PrintDunningLetters;
		/// <summary>
		/// If set to <c>true</c>, indicates that dunning letters 
		/// should be printed for the customer.
		/// </summary>
         [PXDBBool()]
         [PXUIField(DisplayName = "Print Dunning Letters")]
         [PXDefault(false, typeof(Search<CustomerClass.printDunningLetters, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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
		/// <summary>
		/// If set to <c>true</c>, indicates that dunning letters 
		/// should be sent to the customer by email.
		/// </summary>
         [PXDBBool()]
         [PXUIField(DisplayName = "Send Dunning Letters by Email")]
         [PXDefault(false, typeof(Search<CustomerClass.mailDunningLetters, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>))]
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

		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;
		/// <summary>
		/// An unbound Boolean field that is provided for implementation
		/// of the <see cref="IIncludable"/> interface, which is
		/// a part of the row-level security mechanism of Acumatica.
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
		#region SharedCreditChild
		public abstract class sharedCreditChild : PX.Data.BQL.BqlBool.Field<sharedCreditChild> { }

        /// <summary>
        /// When <c>true</c>, indicates that the customer is a child 
        /// with the selected 'Share Credit Policy' option
        /// </summary>
        [PXBool]
		[PXDefault(false)]
        [PXFormula(typeof(IIf<Where<Customer.parentBAccountID, IsNotNull,
            And<Customer.sharedCreditPolicy, Equal<True>,
            And<FeatureInstalled<FeaturesSet.parentChildAccount>>>>, True, False>))]
		public virtual bool? SharedCreditChild { get; set; }
		#endregion
		#region StatementChild
		public abstract class statementChild : PX.Data.BQL.BqlBool.Field<statementChild> { }

        /// <summary>
        /// When <c>true</c>, indicates that the customer is a child 
        /// with the selected 'Consolidate Statements' option
        /// </summary>
        [PXBool]
		[PXDefault(false)]
        [PXFormula(typeof(IIf<Where<Customer.parentBAccountID, IsNotNull,
                    And<Customer.consolidateStatements, Equal<True>,
                    And<FeatureInstalled<FeaturesSet.parentChildAccount>>>>, True, False>))]
		public virtual bool? StatementChild { get; set; }
		#endregion

		/// <summary>
		/// A read-only equivalent of the <see cref="Customer.CustomerClassID"/> 
		/// field, which is used for internal purposes.
		/// </summary>
		[PXString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.Invisible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public override String ClassID
		{
			get { return this.CustomerClassID; }
		}

		#region LocaleName
		public abstract class localeName : PX.Data.BQL.BqlString.Field<localeName> { }
		/// <summary>
		/// The name of the customer's locale.
		/// </summary>
		[PXSelector(typeof(
			Search<Locale.localeName,
			Where<Locale.isActive, Equal<True>>>),
			DescriptionField = typeof(Locale.translatedName))]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Locale")]
		[PXDefault(typeof(Search<CustomerClass.localeName, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string LocaleName { get; set; }
		#endregion

		#region RetainageApply
		public abstract class retainageApply : PX.Data.BQL.BqlBool.Field<retainageApply> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Apply Retainage", FieldClass = nameof(FeaturesSet.Retainage))]
		[PXDefault(false, typeof(Select<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), 
			SourceField = typeof(CustomerClass.retainageApply), 
			PersistingCheck = PXPersistingCheck.Nothing)]
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
		[PXFormula(typeof(Default<Customer.retainageApply>))]
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
		[PXDefault(false, typeof(Select<CustomerClass, Where<CustomerClass.customerClassID, Equal<Current<Customer.customerClassID>>>>), 
			SourceField = typeof(CustomerClass.paymentsByLinesAllowed), 
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? PaymentsByLinesAllowed
		{
			get;
			set;
		}
		#endregion
	}
}
