using PX.Data;
using PX.Data.EP;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;
using System;
using PX.CS.Contracts.Interfaces;
using PX.TM;
using PX.SM;
using PX.Objects.EP;
using System.Diagnostics;
using PX.Objects.GDPR;
using PX.Data.BQL;

namespace PX.Objects.CR
{
	/// <summary>
	/// Depending on the type, represents a real person or the contact information of other entities like <see cref="CRLead"/>,
	/// <see cref="EPEmployee"/>, <see cref="BAccount"/>, an so on.
	/// The records of this type are mostly created and edited on the <i>Contacts (CR.30.20.00)</i> form,
	/// which corresponds to the <see cref="ContactMaint"/> graph.
	/// Also, records of the derived class <see cref="CRLead"/> are mostly created and edited on the <i>Leads (CR.30.10.00)</i> form,
	/// which corresponds to the <see cref="LeadMaint"/> graph.
	/// </summary>
	[Serializable]
	[CRCacheIndependentPrimaryGraph(
		typeof(EmployeeMaint),
		typeof(Where<Contact.contactType, Equal<ContactTypesAttribute.employee>, And<Contact.contactID, Less<int0>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(EmployeeMaint),
		typeof(Select<EPEmployee,
			Where<EPEmployee.defContactID, Equal<Current<Contact.contactID>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(BusinessAccountMaint),
		typeof(Select<BAccount,
			Where<BAccount.defContactID, Equal<Current<Contact.contactID>>, And<BAccount.type, NotEqual<ContactTypesAttribute.employee>>>>))]
	[CRCacheIndependentPrimaryGraph(
		typeof(ContactMaint),
		typeof(Select<Contact,
		  Where<Contact.contactID, IsNull, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
			Or<Where<Contact.contactID, Equal<Current<Contact.contactID>>,
					 And<Where<Contact.contactType, Equal<ContactTypesAttribute.person>,
								Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>>>>),
		VerifyRightsBy = new[] { typeof(ContactMaint) })]
	[CRCacheIndependentPrimaryGraph(
		typeof(LeadMaint),
		typeof(Select<CRLead,
			Where<CRLead.contactID, Equal<Current<Contact.contactID>>>>))]
	[CRContactCacheName(Messages.Contact)]
	[CREmailContactsView(typeof(Select2<Contact, 
		LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>, 
		Where<Contact.contactID, Equal<Optional<Contact.contactID>>,
			  Or2<Where<Optional<Contact.bAccountID>, IsNotNull, And<BAccount.bAccountID, Equal<Optional<Contact.bAccountID>>>>,
				Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>))]
	[PXEMailSource]//NOTE: for assignment map
	[DebuggerDisplay("{GetType().Name,nq}: ContactID = {ContactID,nq}, ContactType = {ContactType}, MemberName = {MemberName}")]
	public partial class Contact : IBqlTable, IContactBase, IPersonalContact, IAssign, IPXSelectable, CRDefaultMailToAttribute.IEmailMessageTarget, IConsentable, INotable
	{
        #region Keys
        public class PK : PrimaryKeyOf<Contact>.By<contactID>
        {
            public static Contact Find(PXGraph graph, int? contactID) => FindBy(graph, contactID);
        }
        #endregion
        #region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }

		/// <summary>
		/// The display name of the contact.
		/// Its value is made up of the <see cref="LastName"/>, <see cref="FirstName"/>, <see cref="MidName"/>, and
		/// <see cref="Title"/> values. The format depends on the <see cref="PreferencesGeneral.PersonNameFormat"/> site setting.
		/// </summary>
		/// <remarks>
		/// This field is changed when the fields it depends on are changed.
		/// </remarks>
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDependsOnFields(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PersonDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PXFieldDescription]
		[PXDefault]
		[PXUIRequired(typeof(Where<Contact.contactType, Equal<ContactTypesAttribute.lead>, Or<Contact.contactType, Equal<ContactTypesAttribute.person>>>))]
		[PXNavigateSelector(typeof(Search2<Contact.displayName,
			LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.contactID>>>,
			Where2<
				Where<Contact.contactType, Equal<ContactTypesAttribute.lead>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.person>,
					Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>,
				And<Where<BAccount.bAccountID, IsNull, Or<Match<BAccount, Current<AccessInfo.userName>>>>>
			>>))]
		[PXPersonalDataField]
		public virtual String DisplayName { get;set; }

		#endregion

		#region MemberName
		public abstract class memberName : PX.Data.BQL.BqlString.Field<memberName> { }

		/// <summary>
		/// Similar to <see cref="DisplayName"/>, but unlike <see cref="DisplayName"/>, it is a calculated field.
		/// It is equal to <see cref="DisplayName"/> if the latter is not <tt>null</tt>, and to <see cref="FullName"/> otherwise.
		/// </summary>
        [PXDBCalced(typeof(Switch<
				Case<Where<displayName, Equal<Empty>>, fullName>,
	        displayName>), typeof(string))]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
		[PXString(255, IsUnicode = true)]
		public virtual string MemberName { get; set; }

		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		/// <summary>
		/// The identifier of the contact.
		/// This field is the key field.
		/// </summary>
		/// <value>
		/// This field is an identity auto-increment database field that in normal conditions cannot be duplicated in different tenants.
		/// This value is present in the URI of the open document: "ScreenId=CR302000&amp;ContactID=100044".
		/// </value>
		[PXDBIdentity(IsKey = true, BqlField = typeof(contactID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXPersonalDataWarning]
		public virtual Int32? ContactID { get; set; }
		#endregion
		
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

		/// <summary>
		/// The identifier of the revision of the contact.
		/// </summary>
		/// <value>
		/// It is increased at each update of the contact.
		/// This field is used to check whether the original contact was changed.
		/// Tables in <tt>PX.Objects</tt> that rely on this field:
		/// <see cref="APContact"/>,
		/// <see cref="AR.ARContact"/>,
		/// <see cref="CRContact"/>,
		/// <see cref="PM.PMContact"/>,
		/// <see cref="PO.POContact"/>,
		/// <see cref="SO.SOContact"/>,
		/// </value>
		[PXDBInt]
		[PXDefault(0)]
		[AddressRevisionID()]
		public virtual Int32? RevisionID { get; set; }
		#endregion


		#region IsAddressSameAsMain
		[Obsolete("Use OverrideAddress instead")]
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }

		[Obsolete("Use OverrideAddress instead")]
		[PXBool]
		[PXUIField(DisplayName = "Same as in Account")]
		public virtual bool? IsAddressSameAsMain { get; set; }
		#endregion

		#region Override Address
		public abstract class overrideAddress : PX.Data.BQL.BqlBool.Field<overrideAddress> { }

		/// <summary>
		/// Specifies whether the <see cref="CR.Address">address</see> 
		/// information of this contact differs from the <see cref="CR.Address">address</see> information
		/// of the <see cref="BAccount">business account</see> associated with this contact.
		/// IF it is so, the <see cref="CR.Address">address</see> information is synchronized with the associated
		/// <see cref="BAccount">business account</see>.
		/// </summary>
		/// <remarks>
		/// The behavior is controlled by the <see cref="ContactMaint.ContactBAccountSharedAddressOverrideGraphExt"/>
		/// graph extension.
		/// </remarks>
		[PXBool]
		[PXUIField(DisplayName = "Override")]
		public virtual bool? OverrideAddress { get; set; }
		#endregion

		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
	    protected Int32? _defAddressID;

		/// <summary>
		/// The identifier of the <see cref="CR.Address"/> object linked with the current document.
		/// The field can be empty for the contacts of the following types:
		/// <see cref="ContactTypesAttribute.BAccountProperty"/> and <see cref="ContactTypesAttribute.Employee"/>.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.Address.AddressID"/> field.
		/// </value>
		[PXDBInt]
		[PXSelector(typeof(Address.addressID), DirtyRead = true)]
		[PXUIField(DisplayName = "Address")]
        [PXDBChildIdentity(typeof(Address.addressID))]
		public virtual Int32? DefAddressID 
        {
		    get
		    {
		        return _defAddressID;
		    }
		    set
		    {
		        _defAddressID = value;
		    }
        }
		#endregion

		#region IsPrimary
		public abstract class isPrimary : PX.Data.BQL.BqlBool.Field<isPrimary> { }

		/// <summary>
		/// Specifies whether the current contact is the primary contact for the current <see cref="BAccount"/> object.
		/// </summary>
		/// <remarks>
		/// It is used only by the <see cref="BusinessAccountMaint"/> graph.
		/// </remarks>
		[PXBool]
		[PXUIField(DisplayName = "Primary", Enabled = false)]
		public virtual bool? IsPrimary { get; set; }
		#endregion

		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }

		/// <summary>
		/// The title of the person.
		/// </summary>
		/// <value>
		/// The predefined values are listed in the <see cref="TitlesAttribute"/> class,
		/// but this field can have any value.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[Titles]
		[PXUIField(DisplayName = "Title")]
		[PXMassMergableField]
		public virtual String Title { get; set; }
		#endregion

		#region FirstName
		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

		/// <summary>
		/// The first name of the person.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
        public virtual String FirstName { get; set; }
		#endregion

		#region MidName
		public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }

		/// <summary>
		/// The middle name of the person.
		/// </summary>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Middle Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String MidName { get; set; }
		#endregion

		#region LastName
		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

		/// <summary>
		/// The last name of the person.
		/// </summary>
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name")]
		[CRLastNameDefault]
		[PXMassMergableField]
		[PXPersonalDataField]
        public virtual String LastName { get; set; }
		#endregion

		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }

		/// <summary>
		/// The job title of the person.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXMassUpdatableField]
		[PXPersonalDataField]
        public virtual String Salutation { get; set; }
		#endregion

		#region Attention
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }

		/// <summary>
		/// The name of the document recipient (a person or team) used in the documents.
		/// </summary>
		/// <remarks>
		/// Not used in primary graph, only in documents, for instance, <see cref="CROpportunity"/>, <see cref="SO.SOOrder"/>, and so on.
		/// </remarks>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Attention")]
		[PXMassMergableField]
		[PXMassUpdatableField]
		[PXPersonalDataField]
		public virtual String Attention { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		/// <summary>
		/// The identifier of the related <see cref="BAccount">business account</see>.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.BAccount.BAccountID"/> field.
		/// </value>
		[PXDBInt]
		[CRContactBAccountDefault]
		[PXParent(typeof(Select<BAccount, 
			Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>, 
			And<BAccount.type, NotEqual<BAccountType.combinedType>,
			And<Where<BAccount.isBranch, NotEqual<True>, 
				Or<BAccount.type, Equal<BAccountType.branchType>>>>>>>))]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), DirtyRead = true)]
		[PXMassUpdatableField]
		public virtual Int32? BAccountID { get; set; }
		#endregion

		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
	    private string _fullName;

		/// <summary>
		/// The name of the company the contact works for.
		/// </summary>
	    [PXMassMergableField]
	    [PXDBString(255, IsUnicode = true)]
	    [PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXPersonalDataField]
	    public virtual String FullName
	    {
	        get { return _fullName; }
	        set { _fullName = value; }
	    }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

		/// <summary>
		/// The identifier of the account that is considered as parent for the current account (<see cref="BAccountID"/>).
		/// </summary>
		/// <remarks>There is no business logic in the application for this field.</remarks>
		/// <value>
		/// Corresponds to the value of the <see cref="CR.BAccount.BAccountID"/> field.
		/// </value>
		[CustomerProspectVendor(DisplayName = "Parent Account")]
		[PXMassUpdatableField]
		public virtual Int32? ParentBAccountID { get; set; }
		#endregion

		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
        private string _eMail;

		/// <summary>
		/// The email address of the contact.
		/// </summary>
		/// <value>
		/// The field should be a valid email address, or a list of email addresses separated by semicolons.
		/// The email addresses will be validated with the <see cref="PX.Common.Mail.EmailParser.ParseAddresses(string)"/> method.
		/// </value>
	    [PXDBEmail]
	    [PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
	    [PXMassMergableField]
	    [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]        
		[PXPersonalDataField]
		public virtual String EMail
	    {
	        get { return _eMail; }
	        set { _eMail = value != null ? value.Trim() : null; }
	    }

		string IContact.Email { get => EMail; set => EMail = value; }
		#endregion

		#region WebSite
		public abstract class webSite : PX.Data.BQL.BqlString.Field<webSite> { }

		/// <summary>
		/// The URL of the contact website.
		/// </summary>
		/// <value>
		/// The field should contain a valid URL.
		/// </value>
		[PXDBWeblink]
		[PXUIField(DisplayName = "Web")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String WebSite { get; set; }
		#endregion

		#region Fax
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }

		/// <summary>
		/// The fax number.
		/// </summary>
		/// <value>
		/// The type of the number can be set by the <see cref="FaxType"/> field.
		/// The value should match the phone validation pattern specified for the current company.
		/// See <see cref="GL.Company.PhoneMask"/> for details.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Fax")]
		[PhoneValidation()]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String Fax { get; set; }
		#endregion

		#region FaxType
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }

		/// <summary>
		/// The phone type for the <see cref="Fax"/> field.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="PhoneTypesAttribute"/> class.
		/// </value>
		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.BusinessFax, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Fax Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String FaxType { get; set; }
		#endregion

		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

		/// <summary>
		/// The phone number.
		/// </summary>
		/// <value>
		/// The type of the number can be set by the <see cref="Phone1Type"/> field.
		/// The value should match the phone validation pattern specified for the current company.
		/// See <see cref="GL.Company.PhoneMask"/> for details.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PhoneValidation()]
		[PXMassMergableField]
		[PXPersonalDataField]
        [PXPhone]
        public virtual String Phone1 { get; set; }
		#endregion

		#region Phone1Type
		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }

		/// <summary>
		/// The phone type for the <see cref="Phone1"/> field.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="PhoneTypesAttribute"/> class.
		/// </value>
		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 1 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone1Type { get; set; }
		#endregion

		#region Phone2
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

		/// <summary>
		/// The second phone number.
		/// </summary>
		/// <value>
		/// The type of the number can be set by the <see cref="Phone2Type"/> field.
		/// The value should match the phone validation pattern specified for the current company.
		/// See <see cref="GL.Company.PhoneMask"/> for details.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Phone 2")]
		[PhoneValidation()]
		[PXMassMergableField]
		[PXPersonalDataField]
        [PXPhone]
        public virtual String Phone2 { get; set; }
		#endregion

		#region Phone2Type
		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }

		/// <summary>
		/// The phone type for the <see cref="Phone2"/> field.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="PhoneTypesAttribute"/> class.
		/// </value>
		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Cell, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 2 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone2Type { get; set; }
		#endregion

		#region Phone3
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

		/// <summary>
		/// The third phone number.
		/// </summary>
		/// <value>
		/// The type of the number can be set by the <see cref="Phone3Type"/> field.
		/// The value should match the phone validation pattern specified for the current company.
		/// See <see cref="GL.Company.PhoneMask"/> for details.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Phone 3")]
		[PhoneValidation()]
		[PXMassMergableField]
		[PXPersonalDataField]
        [PXPhone]
        public virtual String Phone3 { get; set; }
		#endregion

		#region Phone3Type
		public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }

		/// <summary>
		/// The phone type for the <see cref="Phone3"/> field.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="PhoneTypesAttribute"/> class.
		/// </value>
		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 3 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone3Type { get; set; }
		#endregion

		#region DateOfBirth
		public abstract class dateOfBirth : PX.Data.BQL.BqlDateTime.Field<dateOfBirth> { }

		/// <summary>
		/// The date of birth.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Date Of Birth")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual DateTime? DateOfBirth { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <inheritdoc/>
		[PXSearchable(
			category: SM.SearchCategory.CR,
			titlePrefix: "{0}: {1}",
			titleFields: new []
			{
				typeof(Contact.contactType),
				typeof(Contact.displayName)
			},
			fields: new []
			{
				typeof(Contact.fullName),
				typeof(Contact.eMail),
				typeof(Contact.phone1),
				typeof(Contact.phone2),
				typeof(Contact.phone3),
				typeof(Contact.webSite)
			},
			WhereConstraint = typeof(Where<
				Contact.contactType
					.IsNotIn<
						ContactTypesAttribute.bAccountProperty,
						ContactTypesAttribute.employee>>),
			MatchWithJoin = typeof(LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>),
			Line1Format = "{0}{1}{2}{3}",
			Line1Fields = new []
			{
				typeof(Contact.fullName),
				typeof(Contact.salutation),
				typeof(Contact.phone1),
				typeof(Contact.eMail)
			},
			Line2Format = "{1}{2}{3}",
			Line2Fields = new []
			{
				typeof(Contact.defAddressID),
				typeof(Address.displayName),
				typeof(Address.city),
				typeof(Address.state),
				typeof(Address.countryID)
			})]
		[PXUniqueNote(
			DescriptionField = typeof(Contact.displayName),
			Selector = typeof(SelectFrom<Contact>
				.LeftJoin<BAccount>
					.On<BAccount.bAccountID.IsEqual<Contact.bAccountID>>
				.Where<
					Contact.contactType.IsEqual<ContactTypesAttribute.person>
					.And<Brackets<
						BAccount.bAccountID.IsNull
						.Or<Match<BAccount, Current<AccessInfo.userName>>>
					>>
				>
				.SearchFor<Contact.contactID>),
			ShowInReferenceSelector = true)]
		[PXTimeTagAttribute(typeof(Contact.noteID))]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// Specifies whether the current contact is active and can be specified in documents.
		/// </summary>
		/// <remarks>
		/// Only active contacts can be specified in such documents as
		/// <see cref="CROpportunity"/>, <see cref="CRCase"/>, <see cref="CRQuote"/>, <see cref="PM.PMQuote"/>.
		/// The duplicate validation feature <see cref="FeaturesSet.ContactDuplicate"/> works only with active contacts.
		/// </remarks>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region IsNotEmployee
		public abstract class isNotEmployee : PX.Data.BQL.BqlBool.Field<isNotEmployee> { }
		
		/// <summary>
		/// Specifies whether the current contact's type is not the <see cref="ContactTypesAttribute.Employee">employee type</see>.
		/// </summary>
		/// <value>
		/// The value is <see langword="true"/> when the value of <see cref="ContactType"/>
		/// is not equal to <see cref="ContactTypesAttribute.Employee"/>.
		/// </value>
		[PXBool]
		[PXUIField(DisplayName = "Is Not Employee", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsNotEmployee
		{
			get { return !(ContactType == ContactTypesAttribute.Employee); }
		}
		#endregion

		#region NoFax
		public abstract class noFax : PX.Data.BQL.BqlBool.Field<noFax> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that no fax should be sent to the contact.
		/// </summary>
		/// <value>
		/// The system does not rely on this field, but the value can be used by an external system or as an indicator.
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Fax", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoFax { get; set; }
		#endregion

		#region NoMail
		public abstract class noMail : PX.Data.BQL.BqlBool.Field<noMail> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that no mail should be sent to the contact.
		/// </summary>
		/// <value>
		/// The system does not rely on this field, but the value can be used by an external system or as an indicator.
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Mail", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMail { get; set; }
		#endregion

		#region NoMarketing
		public abstract class noMarketing : PX.Data.BQL.BqlBool.Field<noMarketing> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that the email of the contact will not be involved in the mass email process.
		/// </summary>
		/// <value>
		/// Contacts with this field set to <see langword="true"/> will not receive mass emails
		/// sent by the <see cref="CRMassMailMaint"/> graph (the <i>Mass Emails (CR.30.80.00)</i> form).
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "No Marketing", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMarketing { get; set; }
		#endregion

		#region NoCall
		public abstract class noCall : PX.Data.BQL.BqlBool.Field<noCall> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that the contact should not be called.
		/// </summary>
		/// <value>
		/// The system does not rely on this field, but the value can be used by an external system or as an indicator.
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Call", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoCall { get; set; }
		#endregion

		#region NoEMail
		public abstract class noEMail : PX.Data.BQL.BqlBool.Field<noEMail> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that the email of the contact will not be involved in the mass email process.
		/// This contact will not receive any notification emails.
		/// </summary>
		/// <value>
		/// Contacts with this field set to <see langword="true"/> will not receive mass emails
		/// sent by the <see cref="CRMassMailMaint"/> graph (the <i>Mass Emails (CR.30.80.00)</i> form).
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Email", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
        [PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
		public virtual bool? NoEMail { get; set; }
		#endregion

		#region NoMassMail
		public abstract class noMassMail : PX.Data.BQL.BqlBool.Field<noMassMail> { }

		/// <summary>
		/// Specifies (if set to <see langword="true"/>) that the email of the contact will not be involved in the mass email process.
		/// </summary>
		/// <value>
		/// Contacts with this field set to <see langword="true"/> will not receive mass emails
		/// sent by the <see cref="CRMassMailMaint"/> graph (the <i>Mass Emails (CR.30.80.00)</i> form).
		/// </value>
		[PXDBBool]
		[PXUIField(DisplayName = "No Mass Mail", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMassMail { get; set; }
		#endregion

		#region Gender
		public abstract class gender : PX.Data.BQL.BqlString.Field<gender> { }

		/// <summary>
		/// The gender of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="GendersAttribute"/> class,
		/// and a default value can be set depending on the value of <see cref="Title"/>.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Gender")]
		[Genders(typeof(Contact.title))]
		[PXPersonalDataField]
		public virtual String Gender { get; set; }
		#endregion

		#region MaritalStatus
		public abstract class maritalStatus : PX.Data.BQL.BqlString.Field<maritalStatus> { }

		/// <summary>
		/// The marital status of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="MaritalStatusesAttribute"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Marital Status")]
		[MaritalStatuses]
		[PXPersonalDataField]
		public virtual String MaritalStatus { get; set; }
		#endregion

	    #region Anniversary
		[Obsolete("The field is not used anymore")]
		public abstract class anniversary : PX.Data.BQL.BqlDateTime.Field<anniversary> { }

		/// <summary>
		/// The wedding date of the contact.
		/// </summary>
		[Obsolete("The field is not used anymore")]
		[PXDBDate]
		[PXUIField(DisplayName = "Wedding Date")]
		public virtual DateTime? Anniversary { get; set; }
		#endregion

		#region Spouse
		public abstract class spouse : PX.Data.BQL.BqlString.Field<spouse> { }

		/// <summary>
		/// The name of the spouse or partner of the contact.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Spouse/Partner Name")]
		[PXPersonalDataField]
		public virtual String Spouse { get; set; }
		#endregion

		#region Img
		public abstract class img : PX.Data.BQL.BqlString.Field<img> { }

		/// <summary>
		/// The image attached to the contact.
		/// </summary>
		/// <value>
		/// The value can be fetched from the exchange server during contacts synchronization (see <see cref="FeaturesSet.ExchangeIntegration"/>).
		/// </value>
		[PXUIField(DisplayName = "Image", Visibility = PXUIVisibility.Invisible)]
		[PXDBString(IsUnicode = true)]
		public string Img { get; set; }	
		#endregion

		#region Synchronize
		public abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }

		/// <summary>
		/// Specifies whether the contact should be included in exchange synchronization.
		/// </summary>
		/// <value>
		/// The value is used in the exchange integration (see <see cref="FeaturesSet.ExchangeIntegration"/>).
		/// The default value is <see langword="true"/>.
		/// </value>
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Synchronize to Exchange", FieldClass = FeaturesSet.exchangeIntegration.FieldClass)]
		public virtual bool? Synchronize { get; set; }
		#endregion

		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		/// <summary>
		/// The type of the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="ContactTypesAttribute"/> class.
		/// The default value is <see cref="ContactTypesAttribute.Person"/>.
		/// This field must be specified at the initialization stage and not be changed afterwards.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Person)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ContactType { get; set; }
		#endregion

		#region ContactPriority
		public abstract class contactPriority : PX.Data.BQL.BqlInt.Field<contactPriority> { }

		/// <summary>
		/// The numeric representation of <see cref="ContactType"/> used to sort a grid containing contacts of different types.
		/// </summary>
		/// <value>
		/// The value is used in the following graphs: <see cref="OUSearchMaint"/>, <see cref="CampaignMaint"/>, <see cref="CRMarketingListMaint"/>.
		/// The possible values match <see cref="ContactType"/> values as follows:
		/// <list type="table">
		///   <listheader>
		///     <term>ContactType</term>
		///     <description>Priority</description>
		///   </listheader>
		///   <item>
		///     <term><see cref="ContactTypesAttribute.BAccountProperty"/></term>
		///     <description><see cref="ContactTypesAttribute.bAccountPriority"/></description>
		///   </item>
		///   <item>
		///     <term><see cref="ContactTypesAttribute.SalesPerson"/></term>
		///     <description><see cref="ContactTypesAttribute.salesPersonPriority"/></description>
		///   </item>
		///   <item>
		///     <term><see cref="ContactTypesAttribute.Employee"/></term>
		///     <description><see cref="ContactTypesAttribute.employeePriority"/></description>
		///   </item>
		///   <item>
		///     <term><see cref="ContactTypesAttribute.Person"/></term>
		///     <description><see cref="ContactTypesAttribute.personPriority"/></description>
		///   </item>
		///   <item>
		///     <term><see cref="ContactTypesAttribute.Lead"/></term>
		///     <description><see cref="ContactTypesAttribute.leadPriority"/></description>
		///   </item>
		/// </list>
		/// </value>
		[PXDBCalced(typeof(
			Switch<Case<Where<contactType, Equal<ContactTypesAttribute.bAccountProperty>>, ContactTypesAttribute.bAccountPriority,
				   Case<Where<Where<contactType, Equal<ContactTypesAttribute.salesPerson>>>, ContactTypesAttribute.salesPersonPriority,
				   Case<Where<Where<contactType, Equal<ContactTypesAttribute.employee>>>, ContactTypesAttribute.employeePriority,
				   Case<Where<Where<contactType, Equal<ContactTypesAttribute.person>>>, ContactTypesAttribute.personPriority,
				   Case<Where<Where<contactType, Equal<ContactTypesAttribute.lead>>>, ContactTypesAttribute.leadPriority>>>>>>
			), typeof(int))]
		[PXUIField(DisplayName = "Type", Enabled = false)]
		public virtual int? ContactPriority { get; set; }
		#endregion

		#region DuplicateStatus
		public abstract class duplicateStatus : PX.Data.BQL.BqlString.Field<duplicateStatus> { }

		protected String _DuplicateStatus;

		/// <summary>
		/// The duplicate status of the contact.
		/// </summary>
		/// <value>
		/// The field indicates whether the contact was validated by the duplicate validation (see <see cref="FeaturesSet.ContactDuplicate"/>).
		/// The field can have one of the values listed in the <see cref="DuplicateStatusAttribute"/> class.
		/// The default value is <see cref="DuplicateStatusAttribute.NotValidated"/>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[DuplicateStatusAttribute]
		[PXDefault(DuplicateStatusAttribute.NotValidated)]
		[PXUIField(DisplayName = "Duplicate", FieldClass = FeaturesSet.contactDuplicate.FieldClass)]
		public virtual String DuplicateStatus
		{
			get
			{
				return this._DuplicateStatus;
			}
			set
			{
				this._DuplicateStatus = value;
			}
		}
		#endregion

		#region DuplicateFound
		public abstract class duplicateFound : PX.Data.BQL.BqlBool.Field<duplicateFound> { }

		/// <summary>
		/// Specifies whether <see cref="DuplicateStatus"/> is equal to <see cref="DuplicateStatusAttribute.PossibleDuplicated"/>
		/// when the <see cref="FeaturesSet.ContactDuplicate"/> feature is enabled.
		/// </summary>
		[PXBool]
		[PXUIField(DisplayName = "Duplicate Found")]
		[PXDBCalced(typeof(Switch<Case<Where<Contact.duplicateStatus, Equal<DuplicateStatusAttribute.possibleDuplicated>,
			And<FeatureInstalled<FeaturesSet.contactDuplicate>>>, True>,False>), typeof(Boolean))]
		public virtual bool? DuplicateFound { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		/// <summary>
		/// The status of the contact.
		/// </summary>
		/// <value>
		/// The field values are controlled by the automation engine, and the field is not used by the application logic directly.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		[PXStringList(new string[0], new string[0])]
		public virtual String Status { get; set; }

		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		/// <summary>
		/// The reason why the <see cref="Status"/> of this contact has been changed.
		/// </summary>
		/// <value>
		/// The property is used by the <see cref="LeadMaint"/> graph, displayed on the <i>Leads (CR.30.10.00)</i>
		/// form for <see cref="CRLead"/> objects, and controlled by the workflow engine.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason")]
		public virtual String Resolution { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : PX.Data.BQL.BqlDateTime.Field<assignDate> { }

		private DateTime? _assignDate;

		/// <summary>
		/// The date and time when <see cref="OwnerID"/> or <see cref="WorkgroupID"/> were last changed.
		/// </summary>
		[PXUIField(DisplayName = "Assignment Date")]
		[AssignedDate(typeof(Contact.workgroupID), typeof(Contact.ownerID))]
		public virtual DateTime? AssignDate 
		{
			get { return _assignDate ?? CreatedDateTime; }
			set { _assignDate = value;}
		}
		#endregion

		#region ClassID
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID> { }

		/// <summary>
		/// The identifier of the class.
		/// </summary>
		/// <value>
		/// Corresponds to the value of the <see cref="CRContactClass.ClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		[PXMassMergableField]
		[PXMassUpdatableField]
		public virtual String ClassID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		/// <summary>
		/// The source of the contact. If a contact was created from a <see cref="CRLead">lead</see>,
		/// the value is copied from the lead related to the contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRMSourcesAttribute"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[PXMassMergableField]
		[CRMSources]
		public virtual String Source { get; set; }
		#endregion


		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		/// <inheritdoc/>
		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlInt.Field<ownerID> { }

		/// <inheritdoc/>
		[Owner(typeof(Contact.workgroupID))]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual int? OwnerID { get; set; }

		#endregion

		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		/// <summary>
		/// The identifier of the user associated with the contact.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Users.PKID">Users.PKID</see> field.
		/// </value>
		[PXDBGuid]
		[PXUser]
		public virtual Guid? UserID { get; set; }
		#endregion

		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		protected String _CampaignID;

		/// <summary>
		/// The identifier of the campaign that resulted in creation of the contact.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="CRCampaign.CampaignID">CRCampaign.campaignID</see> field.
		/// The property is used by the <see cref="LeadMaint"/> graph and displayed on the <i>Leads (CR.30.10.00)</i> form.
		/// </value>
		[PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Source Campaign")]
		[PXSelector(typeof(CRCampaign.campaignID), DescriptionField = typeof(CRCampaign.campaignName))]
		[PXMassMergableField]
		public virtual String CampaignID
		{
			get
			{
				return this._CampaignID;
			}
			set
			{
				this._CampaignID = value;
			}
		}
		#endregion

		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }

		/// <summary>
		/// The person's preferred method of contact.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="CRContactMethodsAttribute"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[CRContactMethods]
		[PXDefault(CRContactMethodsAttribute.Any, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contact Method")]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String Method { get; set; }

		#endregion

		#region IsConvertable
		[Obsolete("The field is not used anymore.")]
		public abstract class isConvertable : PX.Data.BQL.BqlBool.Field<isConvertable> { }

		[Obsolete("The field is not used anymore.")]
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Can Be Converted", Visible = false)]
		public virtual bool? IsConvertable { get; set; }
		#endregion

		#region GrammValidationDateTime
		public abstract class grammValidationDateTime : PX.Data.BQL.BqlDateTime.Field<grammValidationDateTime> { }
		protected DateTime? _GrammValidationDateTime;

		/// <summary>
		/// The date and time of the last gramm validation.
		/// The field is preserved for internal use.
		/// </summary>
	    [CRValidateDate]		
		public virtual DateTime? GrammValidationDateTime
		{
			get
			{
				return this._GrammValidationDateTime;
			}
			set
			{
				this._GrammValidationDateTime = value;
			}
		}
		#endregion

		#region ConsentAgreement
		public abstract class consentAgreement : PX.Data.BQL.BqlBool.Field<consentAgreement> { }

		// IConsentable
		/// <inheritdoc/>
		[PXConsentAgreementField]
		public virtual bool? ConsentAgreement { get; set; }
		#endregion

		#region ConsentDate
		public abstract class consentDate : PX.Data.BQL.BqlDateTime.Field<consentDate> { }

		// IConsentable
		/// <inheritdoc/>
		[PXConsentDateField]
		public virtual DateTime? ConsentDate { get; set; }
		#endregion

		#region ConsentExpirationDate
		public abstract class consentExpirationDate : PX.Data.BQL.BqlDateTime.Field<consentExpirationDate> { }

		// IConsentable
		/// <inheritdoc/>
		[PXConsentExpirationDateField]
		public virtual DateTime? ConsentExpirationDate { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		/// <summary>
		/// The attributes list available for the current contact.
		/// The field is preserved for internal use.
		/// </summary>
		[CRAttributesField(typeof(Contact.classID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region IEmailMessageTarget Members

		/// <summary>
		/// This field is used to implement the <see cref="CRDefaultMailToAttribute.IEmailMessageTarget.Address"/>
		/// interface only, it always returns <see cref="EMail"/>.
		/// </summary>
		public string Address
		{
			get { return EMail; }
		}

		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		#endregion

		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion

		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion

		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion

		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

		[PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion

        #region SuggestField
		[Obsolete("The field is not used anymore")]
        public abstract class searchSuggestion : PX.Data.BQL.BqlString.Field<searchSuggestion> { }

		[Obsolete("The field is not used anymore")]
        [PXString(150, IsUnicode = true)]
        [PXUIField(DisplayName = "Search Suggestion")]
		[PXDBCalced(typeof(Add<Quotes, Add<Contact.displayName, Add<Quotes, Add<Space, Add<OpenBracket, Add<Contact.eMail, CloseBracket>>>>>>), typeof(string))]
        public virtual String SearchSuggestion 
		  {
	        get { return _SearchSuggestion; }
	        set { _SearchSuggestion = value; }
        }
			protected string _SearchSuggestion;
		#endregion

		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
        protected String _ExtRefNbr;
		/// <summary>
		/// The external reference number of the contact.
		/// It can be an additional number of the contact used in external integration.
		/// </summary>
        [PXDBString(40, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Ext Ref Nbr", Visibility = PXUIVisibility.Dynamic, Visible = true)]
        [PXMassMergableField]
        public virtual String ExtRefNbr
        {
            get
            {
                return this._ExtRefNbr;
            }
            set
            {
                this._ExtRefNbr = value;
            }
        }
		#endregion

		#region LanguageID
		public abstract class languageID : PX.Data.BQL.BqlString.Field<languageID> { }
		protected String _LanguageID;

		/// <summary>
		/// The language in which the contact prefers to communicate.
		/// </summary>
		/// <value>
		/// By default, the system fills in the box with the locale specified for the contact's country.
		/// This field is displayed on the form only if there are multiple active locales
		/// configured on the <i>System Locales (SM.20.05.50)</i> form
		/// (corresponds to the <see cref="LocaleMaintenance"/> graph).
		/// </value>
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Language/Locale")]
		[PXSelector(typeof(
			Search<Locale.localeName,
			Where<Locale.isActive, Equal<True>>>),
			DescriptionField = typeof(Locale.description))]
		[ContacLanguageDefault(typeof(Address.countryID))]
		public virtual String LanguageID
		{
			get
			{
				return this._LanguageID;
			}
			set
			{
				this._LanguageID = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		#endregion

		#region DeletedDatabaseRecord
		public abstract class deletedDatabaseRecord : PX.Data.BQL.BqlBool.Field<deletedDatabaseRecord> { }

		/// <exclude/>
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DeletedDatabaseRecord { get; set; }
		#endregion
		
		#region NOT SUPPORTED
		bool? IContact.IsDefaultContact { get => null; set { } }
		int? IContact.BAccountContactID { get => null; set { } }
		#endregion

	}

	[Obsolete("This class is not used anymore")]
	[PXProjection(typeof(Select<Contact>), Persistent = false)]
	public class ContactBAccount : Contact
	{
		#region ContactID
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBInt(IsKey = true, BqlTable = typeof(Contact))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		public override Int32? ContactID { get; set; }
		#endregion		

		#region Contact
		public abstract class contact : PX.Data.BQL.BqlString.Field<contact> { }
		[PXString]
		[PXUIField(DisplayName = "Contact", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
		[PXDependsOnFields(typeof(ContactBAccount.displayName), typeof(ContactBAccount.fullName))]
		public virtual String Contact { get { return string.IsNullOrEmpty(DisplayName) ?  FullName : DisplayName; }}
		#endregion		

        public new abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }

	}

	/// <summary>
	/// The projection of the <see cref="Contact"/> table joined with the <see cref="BAccount"/> table
	/// (on the condition that <see cref="Contact.BAccountID"/> is equal to <see cref="BAccount.BAccountID"/>).
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select2<Contact, LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>), Persistent = false)]
	public class ContactAccount : Contact
	{
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;

		/// <inheritdoc cref="BAccount.AcctName"/>
		[PXDBString(60, IsUnicode = true, BqlTable = typeof(BAccount))]
		[PXDefault()]
		[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		[PXMassMergableField]
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
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;

		/// <inheritdoc cref="BAccount.Type"/>
		[PXDBString(2, IsFixed = true, BqlTable = typeof(BAccount))]
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
		#region AccountStatus
		public abstract class accountStatus : PX.Data.BQL.BqlString.Field<accountStatus> { }

		protected String _AccountStatus;

		/// <inheritdoc cref="BAccount.Status"/>
		[PXDBString(1, IsFixed = true, BqlField = typeof(BAccount.status))]
		[PXUIField(DisplayName = "Status")]
		[BAccount.status.List()]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String AccountStatus
		{
			get
			{
				return this._AccountStatus;
			}
			set
			{
				this._AccountStatus = value;
			}
		}
		#endregion
		#region DefContactID
		public abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
		protected Int32? _DefContactID;

		/// <inheritdoc cref="BAccount.DefContactID"/>
		[PXDBInt(BqlTable = typeof(BAccount))]				
		[PXMassMergableField]
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
	}
}