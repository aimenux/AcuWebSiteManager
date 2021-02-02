using System;
using System.Diagnostics;
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR.MassProcess;
using PX.Objects.EP;
using PX.Objects.TX;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
	/// <summary>
	/// Represents a business account used as a prospect, customer, or vendor.
	/// Also, this is the base class for derived DACs: <see cref="Customer"/>, <see cref="Vendor"/>, <see cref="EPEmployee"/>.
	/// The records of this type are created and edited on the <i>Business Accounts (CR.30.30.00)</i> form
	/// (corresponds to the <see cref="BusinessAccountMaint"/> graph).
	/// <see cref="Customer">Customers</see> are created and edited on the <i>Customers (AR.30.30.00)</i> form
	/// (corresponds to the <see cref="CustomerMaint"/> graph).
	/// <see cref="Vendor">Vendors</see> are created and edited on the <i>Vendors (AP.30.30.00)</i> form
	/// (corresponds to the <see cref="VendorMaint"/> graph).
	/// <see cref="EPEmployee">Employees</see> are created and edited on the <i>Employees (EP.20.30.00)</i> form
	/// (corresponds to the <see cref="EmployeeMaint"/> graph).
	/// <see cref="CS.DAC.OrganizationBAccount">Companies</see> are created and edited on the <i>Companies (CS.10.15.00)</i> form
	/// (corresponds to the <see cref="CS.DAC.OrganizationBAccount"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[CRCacheIndependentPrimaryGraphList(new Type[]{
        typeof(CR.BusinessAccountMaint),
		typeof(EP.EmployeeMaint),
		typeof(AP.VendorMaint),
		typeof(AP.VendorMaint),
		typeof(AR.CustomerMaint),
		typeof(AR.CustomerMaint),
		typeof(AP.VendorMaint),
		typeof(AR.CustomerMaint),
		typeof(CR.BusinessAccountMaint)},
		new Type[]{
            typeof(Select<CR.BAccount, Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
                    And<Current<BAccount.viewInCrm>, Equal<True>>>>),
			typeof(Select<EP.EPEmployee, Where<EP.EPEmployee.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<AP.VendorR, Where<AP.VendorR.bAccountID, Equal<Current<BAccount.bAccountID>>>>), 
			typeof(Select<AP.Vendor, Where<AP.Vendor.bAccountID, Equal<Current<BAccountR.bAccountID>>>>), 
			typeof(Select<AR.Customer, Where<AR.Customer.bAccountID, Equal<Current<BAccount.bAccountID>>>>),
			typeof(Select<AR.Customer, Where<AR.Customer.bAccountID, Equal<Current<BAccountR.bAccountID>>>>),
			typeof(Where<CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.vendorType>>>), 
			typeof(Where<CR.BAccountR.bAccountID, Less<Zero>,
					And<BAccountR.type, Equal<BAccountType.customerType>>>), 
			typeof(Select<CR.BAccount, 
				Where2<Where<
					CR.BAccount.type, Equal<BAccountType.prospectType>,
					Or<CR.BAccount.type, Equal<BAccountType.customerType>,
					Or<CR.BAccount.type, Equal<BAccountType.vendorType>,
					Or<CR.BAccount.type, Equal<BAccountType.combinedType>>>>>,
						And<Where<CR.BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
						Or<Current<BAccount.bAccountID>, Less<Zero>>>>>>)
		},
		VerifyRightsBy = new [] { typeof(CR.BusinessAccountMaint) })]
	[PXCacheName(Messages.BusinessAccount, PXDacType.Catalogue, CacheGlobal = true)]
	[CREmailContactsView(typeof(Select2<Contact,
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>, 
		Where<Contact.bAccountID, Equal<Optional<BAccount.bAccountID>>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	[DebuggerDisplay("{GetType().Name,nq}: BAccountID = {BAccountID,nq}, AcctCD = {AcctCD}, AcctName = {AcctName}")]
	public partial class BAccount : IBqlTable, IAssign, IPXSelectable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BAccount>.By<bAccountID>
		{
			public static BAccount Find(PXGraph graph, int bAccountID) => FindBy(graph, bAccountID);
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
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

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

		/// <summary>
		/// The identifier of the business account.</summary>
		/// <remarks>This field is auto-incremental.
		/// This field is a surrogate key, as opposed to the natural key <see cref="AcctCD"/>.
		/// </remarks>
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible, DisplayName = "Account ID")]
		[PXReferentialIntegrityCheck]
		public virtual Int32? BAccountID
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
		#region AcctDC
		public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		protected String _AcctCD;

		/// <summary>
		/// The human-readable identifier of the business account that is
		/// specified by the user or defined by the auto-numbering sequence during the
		/// creation of the account. This field is a natural key, as opposed
		/// to the surrogate key <see cref="BAccountID"/>.
		/// </summary>
		[PXDimensionSelector("BIZACCT", typeof(Search<BAccount.acctCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask="")]
		[PXDefault()]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXPersonalDataWarning]
		public virtual String AcctCD
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
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;

		/// <summary>
		/// The full business account name (as opposed to the
		/// short identifier <see cref="AcctCD"/>).
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String AcctName
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
		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		/// <summary>
		/// Identifier of the <see cref="CRCustomerClass">business acccount class</see> 
		/// to which the business account belongs.
		/// </summary>
		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID), DescriptionField = typeof(CRCustomerClass.description), CacheGlobal = true)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String ClassID { get; set; }
		#endregion

		#region LegalName
		public abstract class legalName : PX.Data.BQL.BqlString.Field<legalName> { }

		/// <summary>
		/// The legal name of the company that is used by the
		/// <see cref="FeaturesSet.Reporting1099">1099 Reporting</see> feature only (see <see cref="GL.DAC.Organization"/>).
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXDefault]
		[PXFormula(typeof(acctName))]
		[PXUIField(DisplayName = "Legal Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String LegalName
		{
			get;
			set;
		}
		#endregion
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;

		/// <summary>
		/// Represents the type of the business account.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="BAccountType"/> class.
		/// The default value is <see cref="BAccountType.ProspectType"/> for a prospect,
		/// <see cref="BAccountType.CustomerType"/> for a customer,
		/// and <see cref="BAccountType.VendorType"/> for a vendor.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[BAccountType.List()]
		public virtual String Type
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
		public abstract class isCustomerOrCombined : PX.Data.BQL.BqlBool.Field<isCustomerOrCombined> { }

		/// <summary>
		/// A calculated field that indicates (if set to <c>true</c>) that <see cref="BAccount.Type"/> is either
		/// <see cref="BAccountType.CustomerType"/>  or <see cref="BAccountType.CombinedType"/>.
		/// </summary>
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBCalced(typeof(Switch<Case<Where<BAccount.type, Equal<BAccountType.customerType>, Or<BAccount.type, Equal<BAccountType.combinedType>>>, True>, False>), typeof(bool))]
		public virtual bool? IsCustomerOrCombined { get; set; }
		#endregion
		#region IsBranch
		public abstract class isBranch : PX.Data.BQL.BqlBool.Field<isBranch> { }

		[PXDBBool]
		[PXDefault(
			typeof(Switch<Case<Where<type.IsEqual<BAccountType.branchType>>, True>, False>),
			PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual bool? IsBranch { get; set; }
		#endregion
		#region AcctReferenceNbr
		public abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
		protected String _AcctReferenceNbr;

		/// <summary>
		/// The external reference number of the business account.</summary>
		/// <remarks>It can be an additional number of the business account used in external integration.
		/// </remarks>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Ext Ref Nbr", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		public virtual String AcctReferenceNbr
		{
			get
			{
				return this._AcctReferenceNbr;
			}
			set
			{
				this._AcctReferenceNbr = value;
			}
		}
		#endregion
		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		protected Int32? _ParentBAccountID;

		/// <summary>
		/// The identifier of the parent business account.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.BAccount.BAccountID"/> field of the parent account.
		/// This field is used for consolidating customer account balances on the parent account from child accounts.
		/// </value>
		[PXDBInt]
		[PXUIField(DisplayName = "Parent Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountR.bAccountID,
			LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>,
			LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>>>,
				Where<Current<BAccount.bAccountID>, NotEqual<BAccountR.bAccountID>,
				And2<Where<BAccountR.type, Equal<BAccountType.customerType>,
					Or<BAccountR.type, Equal<BAccountType.prospectType>,
					Or<BAccountR.type, Equal<BAccountType.combinedType>,
					Or<BAccountR.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>>),
			typeof(BAccountR.acctCD),
			typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type), typeof(BAccountR.classID),
			typeof(BAccountR.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail), 
			DescriptionField = typeof(BAccountR.acctName))]
		[PXForeignReference(typeof(Field<parentBAccountID>.IsRelatedTo<bAccountID>))]
		[PXMassMergableField]
		public virtual Int32? ParentBAccountID
		{
			get
			{
				return this._ParentBAccountID;
			}
			set
			{
				this._ParentBAccountID = value;
			}
		}
		#endregion
		#region ConsolidateToParent
		public abstract class consolidateToParent : PX.Data.BQL.BqlBool.Field<consolidateToParent> { }

		/// <summary>
		/// The total balance of the parent customer account
		/// including balances of its child accounts for which the value of this field is <tt>true</tt>
		/// on the <b>Billing Info</b> tab of this form. The amount includes the balances of all open documents and prepayments.
		/// </summary>
		/// <value>
		/// This field is used in the <see cref="Customer"/> class
		/// and showed on the <i>Customers (AR.30.30.00)</i> form only.
		/// </value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Consolidate Balance")]
		public virtual bool? ConsolidateToParent { get; set; }
		#endregion
		#region ConsolidatingBAccountID
		public abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }

		/// <summary>
		/// The field is preserved for internal use.
		/// </summary>
		[PXDBInt]
		[PXFormula(typeof(Switch<
			Case<Where<BAccount.parentBAccountID, IsNotNull, And<BAccount.consolidateToParent, Equal<True>>>, BAccount.parentBAccountID>,
			BAccount.bAccountID>))]
		public virtual Int32? ConsolidatingBAccountID { get; set; }
		#endregion
		#region COrgBAccountID
		public abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }

		// todo: documentate
		[CR.RestrictOrganization()]
		[PXUIField(DisplayName = "Customer Restriction Group", FieldClass = nameof(FeaturesSet.VisibilityRestriction))]
		[PXDefault(0)]
		public virtual int? COrgBAccountID { get; set; }
		#endregion
		#region VOrgBAccountID
		public abstract class vOrgBAccountID : PX.Data.BQL.BqlInt.Field<vOrgBAccountID> { }

		// todo: documentate
		[CR.RestrictOrganization()]
		[PXUIField(DisplayName = "Vendor Restriction Group", FieldClass = nameof(FeaturesSet.VisibilityRestriction))]
		[PXDefault(0)]
		public int? VOrgBAccountID { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					new string[] { Active, Hold, HoldPayments, Inactive, OneTime, CreditHold },
					new string[] { Messages.Active, Messages.Hold, Messages.HoldPayments, Messages.Inactive, Messages.OneTime, Messages.CreditHold }) { }
			}

			public const string Active = "A";
			public const string Hold = "H";
			public const string HoldPayments = "P";
			public const string Inactive = "I";
			public const string OneTime = "T";
			public const string CreditHold = "C";

			public class active : PX.Data.BQL.BqlString.Constant<active>
			{
				public active() : base(Active) { ;}
			}
			public class hold : PX.Data.BQL.BqlString.Constant<hold>
			{
				public hold() : base(Hold) { ;}
			}
			public class holdPayments : PX.Data.BQL.BqlString.Constant<holdPayments>
			{
				public holdPayments() : base(HoldPayments) { ;}
			}
			public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) { ;}
			}
			public class oneTime : PX.Data.BQL.BqlString.Constant<oneTime>
			{
				public oneTime() : base(OneTime) { ;}
			}
			public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
			{
				public creditHold() : base(CreditHold) { ;}
			}
		}
		protected String _Status;

		/// <summary>
		/// The status of the business account.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the related BQL nested class <see cref="status"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Required = true)]
		[status.List()]
		[PXDefault(status.Active)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String Status
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
		#region CampaignSourceID
		public abstract class campaignSourceID : PX.Data.BQL.BqlString.Field<campaignSourceID> { }

		/// <summary>
		/// The identifier of the marketing or sales campaign that resulted in creation of the business account.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CRCampaign.CampaignID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Source Campaign")]
		[PXSelector(typeof(Search3<CRCampaign.campaignID, OrderBy<Desc<CRCampaign.campaignID>>>),
			DescriptionField = typeof(CRCampaign.campaignName), Filterable = true)]
		public virtual String CampaignSourceID { get; set; }
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;

		/// <summary>
		/// The identifier of the <see cref="CR.Address"/> record used to store address data of the business account.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Address.AddressID"/> field.
		/// </value>
		/// <remarks>
		/// Also, the <see cref="Address.BAccountID">Address.BAccountID</see> value must be equal to
		/// the <see cref="BAccount.BAccountID">BAccount.BAccountID</see> value of the current business account.
		/// </remarks>
		[PXDBInt]
		[PXDBChildIdentity(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(
			typeof(Search<Address.addressID>),
			DescriptionField = typeof(Address.displayName),
			DirtyRead = true)]
        public virtual Int32? DefAddressID
		{
			get
			{
				return this._DefAddressID;
			}
			set
			{
				this._DefAddressID = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;

		/// <summary>
		/// The identifier of the <see cref="CR.Contact"/> object used to store additional contact data of the business account.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Contact.ContactID"/> field.
		/// </value>
		/// <remarks>
		/// Also, the <see cref="Contact.BAccountID">Contact.BAccountID</see> value must be equal to
		/// the <see cref="BAccount.BAccountID">BAccount.BAccountID</see> value of the current business account.
		/// </remarks>
		[PXDBInt()]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>), DirtyRead = true)]
		public virtual Int32? DefContactID
		{
			get
			{
				return this._DefContactID;
			}
			set
			{
				this._DefContactID = value;
			}
		}
		#endregion
		#region DefLocationID
		public abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		protected Int32? _DefLocationID;

		/// <summary>
		/// The identifier of the <see cref="Location"/> object linked with the business account and marked as default.
		/// The linked location is shown on the <b>Shipping</b> tab.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="Location.LocationID"/> field.
		/// </value>
		/// <remarks>
		/// Also, the <see cref="Location.BAccountID">Location.BAccountID</see> value must also equal to
		/// the <see cref="BAccount.BAccountID">BAccount.BAccountID</see> value of the current business account.
		/// </remarks>
		[PXDBInt()]
		[PXDBChildIdentity(typeof(Location.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<Location.locationID,
			Where<Location.bAccountID,
			Equal<Current<BAccount.bAccountID>>>>),
			DescriptionField = typeof(Location.locationCD), 
			DirtyRead = true)]
		public virtual Int32? DefLocationID
		{
			get
			{
				return this._DefLocationID;
			}
			set
			{
				this._DefLocationID = value;
			}
		}
		#endregion
		#region TaxZoneID
		public abstract class taxZoneID : PX.Data.BQL.BqlString.Field<taxZoneID> { }
		protected String _TaxZoneID;

		[Obsolete("The field is obsolete and will be removed in Acumatica 8.0.")]
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Zone ID")]
		[PXSelector(typeof(Search<TaxZone.taxZoneID>),DescriptionField = typeof(TaxZone.descr), CacheGlobal = true)]
		[PXMassMergableField]
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
		#region TaxRegistrationID
		public abstract class taxRegistrationID : PX.Data.BQL.BqlString.Field<taxRegistrationID> { }
		protected String _TaxRegistrationID;

		/// <summary>
		/// The registration ID of the company in the state tax authority.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Registration ID")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String TaxRegistrationID
		{
			get
			{
				return this._TaxRegistrationID;
			}
			set
			{
				this._TaxRegistrationID = value;
			}
		}
		#endregion
		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }
		protected int? _WorkgroupID;

		/// <inheritdoc/>
		[PXDBInt]
		[PXCompanyTreeSelector]
		[PXUIField(DisplayName = "Workgroup", Visibility = PXUIVisibility.Visible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual int? WorkgroupID
		{
			get
			{
				return this._WorkgroupID;
			}
			set
			{
				this._WorkgroupID = value;
			}
		}
		#endregion
		#region PrimaryContactID
		public abstract class primaryContactID : PX.Data.BQL.BqlInt.Field<primaryContactID> { }

		/// <summary>
		/// The identifier of the <see cref="CR.Contact"/> object linked with the business account and marked as primary.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Contact.ContactID"/> field.
		/// </value>
		/// <remarks>
		/// Also, the <see cref="Contact.BAccountID">Contact.BAccountID</see> value must equal to
		/// the <see cref="BAccount.BAccountID">BAccount.BAccountID</see> value of the current business account.
		/// </remarks>
		[PXDBInt]
		[PXUIField(DisplayName = "Primary Contact")]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		public virtual int? PrimaryContactID { get; set; }
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

		/// <summary>
		/// The group mask that indicates which <see cref="PX.SM.RelationGroup">restriction groups</see> the business account belongs to.
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

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		protected int? _OwnerID;
		/// <inheritdoc/>
		[Owner(typeof(BAccount.workgroupID), Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual int? OwnerID
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
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;

		/// <inheritdoc/>
		[PXSearchable(SM.SearchCategory.CR, "{0} {1}", new Type[] { typeof(BAccount.type), typeof(BAccount.acctName) },
			new Type[] { typeof(BAccount.acctCD), typeof(BAccount.defContactID), typeof(Contact.displayName), typeof(Contact.eMail), 
                        typeof(Contact.phone1), typeof(Contact.phone2), typeof(Contact.phone3), typeof(Contact.webSite) },
			NumberFields = new Type[] { typeof(BAccount.acctCD) },
			  Line1Format = "{0}{1}{3}{4}{5}", Line1Fields = new Type[] {  typeof(BAccount.type), typeof(BAccount.acctCD), typeof(BAccount.defContactID), typeof(Contact.displayName), typeof(Contact.phone1), typeof(Contact.eMail) },
			  Line2Format = "{1}{2}{3}", Line2Fields = new Type[] { typeof(BAccount.defAddressID), typeof(Address.displayName), typeof(Address.city), typeof(Address.state) },
			  WhereConstraint = typeof(Where<BAccount.type, Equal<BAccountType.prospectType>>),
			  SelectForFastIndexing = typeof(Select2<BAccount, InnerJoin<Contact, On<Contact.contactID, Equal<BAccount.defContactID>>>, Where<BAccount.type, Equal<BAccountType.prospectType>>>)
		  )]
		[PXUniqueNote(
			DescriptionField = typeof(BAccount.acctCD),
			Selector = typeof(Search<BAccount.acctCD, Where<BAccount.type, Equal<BAccountType.prospectType>>>),
			ActivitiesCountByParent = true,
			ShowInReferenceSelector = true,
            PopupTextEnabled = true)]
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

		/// <summary>
		/// The attributes list available for the current business account.
		/// The field is preserved for internal use.
		/// </summary>
		[CRAttributesField(typeof (BAccount.classID), new[] {typeof (Customer.customerClassID), typeof (Vendor.vendorClassID)})]
		public virtual string[] Attributes { get; set; }

		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
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

		#region CasesCount

		[Obsolete("This field is not used anymore")]
		public abstract class casesCount : PX.Data.BQL.BqlInt.Field<casesCount> { }

		[PXInt]
		[Obsolete("This field is not used anymore")]
		public virtual Int32? CasesCount { get; set; }

		#endregion

		#region Count

		[Obsolete("This field is not used anymore")]
		public abstract class count : PX.Data.BQL.BqlInt.Field<count> { }

		[PXInt]
		[PXUIField(DisplayName = "Count")]
		[Obsolete("This field is not used anymore")]
		public virtual Int32? Count { get; set; }

		#endregion

		#region LastActivity

		[Obsolete("This field is not used anymore")]
		public abstract class lastActivity : PX.Data.BQL.BqlDateTime.Field<lastActivity> { }

		[PXDate]
		[PXUIField(DisplayName = "Last Activity")]
		[Obsolete("This field is not used anymore")]
		public DateTime? LastActivity { get; set; }

		#endregion

		#region PreviewHtml
		[Obsolete("This field is not used anymore")]
		public abstract class previewHtml : PX.Data.BQL.BqlString.Field<previewHtml> { }
		[PXString]
		//[PXUIField(Visible = false)]
		[Obsolete("This field is not used anymore")]
		public virtual String PreviewHtml { get; set; }
		#endregion

        #region ViewInCrm
		[Obsolete("This field is not used anymore")]
        public abstract class viewInCrm : PX.Data.BQL.BqlBool.Field<viewInCrm> { }
        protected bool? _ViewInCrm;
        [PXBool]
        [PXUIField(DisplayName = "View In CRM")]
		[Obsolete("This field is not used anymore")]
        public virtual bool? ViewInCrm
        {
            get
            {
                return this._ViewInCrm;
            }
            set
            {
                this._ViewInCrm = value;
            }
        }
		#endregion
	}

	#region BAccount2

	/// <exclude/>
	[Serializable]
	public sealed class BAccount2 : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		
		public new abstract class isBranch : PX.Data.BQL.BqlBool.Field<isBranch> { }
	}

	#endregion

	#region BAccountParent

	/// <summary>
	/// The class derived from <see cref="BAccount"/> that is used in BQL queries
	/// when it is needed to use the join operation for the <see cref="BAccount"/> table twice.
	/// <see cref="BAccountParent"/> is used for the join clause that has the condition involving the parent ID
	/// (for example, <see cref="Contact.ParentBAccountID">Contact.ParentBAccountID</see>,
	/// <see cref="BAccount.ParentBAccountID">Contact.ParentBAccountID</see>).
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.ParentBusinessAccount)]        
    [CRCacheIndependentPrimaryGraphList(new Type[]{
    typeof(CR.BusinessAccountMaint),    
		},
        new Type[]{      
			typeof(Select<BAccountCRM, 
				Where<BAccountCRM.bAccountID, Equal<Current<BAccountParent.bAccountID>>, 
					Or<Current<BAccountParent.bAccountID>, Less<Zero>>>>),            
		})]    
	public sealed class BAccountParent : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		[PXDimensionSelector("BIZACCT", typeof(BAccount.acctCD), typeof(BAccount.acctCD))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Parent Business Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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

		#region AcctName
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Parent Business Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMassMergableField]
		public override string AcctName
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
	}

	#endregion

	#region BAccountItself

	//This type is used internally to prevent extension of BAccont to derived classes in SQL selects
	[Serializable]
	[PXHidden]
	public class BAccountItself : BAccount { }

	#endregion

	#region BAccountR

	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class BAccountR : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class isCustomerOrCombined : PX.Data.BQL.BqlBool.Field<isCustomerOrCombined> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public new abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }
		public new abstract class vOrgBAccountID : PX.Data.BQL.BqlInt.Field<vOrgBAccountID> { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountR.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountR.bAccountID>, And<Contact.contactID, Equal<BAccountR.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccountR.bAccountID>, And<Address.addressID, Equal<BAccountR.defAddressID>>>>>,
				Where2<Where<BAccountR.type, Equal<BAccountType.customerType>,
					Or<BAccountR.type, Equal<BAccountType.prospectType>,
					Or<BAccountR.type, Equal<BAccountType.combinedType>,
					Or<BAccountR.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccountR.acctCD),
			typeof(BAccountR.acctCD), typeof(BAccountR.acctName), typeof(BAccountR.type), typeof(BAccountR.classID), typeof(BAccountR.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail),
			DescriptionField = typeof(BAccountR.acctName))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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

		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>))]
		[PXUIField(DisplayName = "Default Contact", Visibility = PXUIVisibility.Invisible)]
		public override Int32? DefContactID { get; set; }

		#endregion

		#region ViewInCrm
		public new abstract class viewInCrm : PX.Data.BQL.BqlBool.Field<viewInCrm> { }

		[PXBool]
		[PXUIField(DisplayName = "View In CRM")]
		public new virtual bool? ViewInCrm { get; set; }
		#endregion

		#region Status
		public new abstract class status : PX.Data.BQL.BqlString.Field<status>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
						new string[] { Active, Hold, HoldPayments, Inactive, OneTime, CreditHold },
						new string[] { Messages.Active, Messages.Hold, Messages.HoldPayments, Messages.Inactive, Messages.OneTime, Messages.CreditHold })
				{ }
			}

			public const string Active = "A";
			public const string Hold = "H";
			public const string HoldPayments = "P";
			public const string Inactive = "I";
			public const string OneTime = "T";
			public const string CreditHold = "C";

			public class active : PX.Data.BQL.BqlString.Constant<active>
			{
				public active() : base(Active) {; }
			}
			public class hold : PX.Data.BQL.BqlString.Constant<hold>
			{
				public hold() : base(Hold) {; }
			}
			public class holdPayments : PX.Data.BQL.BqlString.Constant<holdPayments>
			{
				public holdPayments() : base(HoldPayments) {; }
			}
			public class inactive : PX.Data.BQL.BqlString.Constant<inactive>
			{
				public inactive() : base(Inactive) {; }
			}
			public class oneTime : PX.Data.BQL.BqlString.Constant<oneTime>
			{
				public oneTime() : base(OneTime) {; }
			}
			public class creditHold : PX.Data.BQL.BqlString.Constant<creditHold>
			{
				public creditHold() : base(CreditHold) {; }
			}
		}
		#endregion
	}

	#endregion

	#region BAccountCRM

	/// <exclude/>
	[PXPrimaryGraph(typeof(BusinessAccountMaint))]
	[CRCacheIndependentPrimaryGraphList(
		new Type[] {typeof(BusinessAccountMaint)},

		new Type[] {typeof(Select<
				BAccount,
			Where<
				BAccount.bAccountID, Equal<Current<BAccount.bAccountID>>,
				Or<Current<BAccount.bAccountID>, Less<Zero>>>>)
		})]
	[Serializable]
	[PXHidden]
	public class BAccountCRM : BAccount
	{
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }
		public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
		public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		public new abstract class cOrgBAccountID : PX.Data.BQL.BqlInt.Field<cOrgBAccountID> { }
		public new abstract class vOrgBAccountID : PX.Data.BQL.BqlInt.Field<vOrgBAccountID> { }

		#region AcctCD
		public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }

		[PXDimensionSelector("BIZACCT",
			typeof(Search2<BAccountCRM.acctCD,
					LeftJoin<Contact, On<Contact.bAccountID, Equal<BAccountCRM.bAccountID>, And<Contact.contactID, Equal<BAccountCRM.defContactID>>>,
					LeftJoin<Address, On<Address.bAccountID, Equal<BAccountCRM.bAccountID>, And<Address.addressID, Equal<BAccountCRM.defAddressID>>>>>,
				Where2<Where<BAccountCRM.type, Equal<BAccountType.customerType>,
					Or<BAccountCRM.type, Equal<BAccountType.prospectType>,
					Or<BAccountCRM.type, Equal<BAccountType.combinedType>,
					Or<BAccountCRM.type, Equal<BAccountType.vendorType>>>>>,
					And<Match<Current<AccessInfo.userName>>>>>),
			typeof(BAccountCRM.acctCD),
			typeof(BAccountCRM.acctCD), typeof(BAccountCRM.acctName), typeof(BAccountCRM.type), typeof(BAccountCRM.classID), typeof(BAccountCRM.status), typeof(Contact.phone1), typeof(Address.city), typeof(Address.countryID), typeof(Contact.eMail))]
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public override String AcctCD { get; set; }
		#endregion

		#region DefContactID
		public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }

		[PXDBInt()]
		[PXDBChildIdentity(typeof(Contact.contactID))]
		[PXSelector(typeof(Search<Contact.contactID>))]
		public override Int32? DefContactID { get; set; }
		#endregion
	}

	#endregion
}
