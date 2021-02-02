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
								Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>>>>>>>))]
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
	public partial class Contact : IBqlTable, IContactBase, IPersonalContact, IAssign, IPXSelectable, CRDefaultMailToAttribute.IEmailMessageTarget, IConsentable
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

		[PXUIField(DisplayName = "Display Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDependsOnFields(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PersonDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title))]
		[PXFieldDescription]
		[PXDefault(Messages.New)]
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
        [PXDBCalced(typeof(Switch<
				Case<Where<displayName, Equal<Empty>>, fullName>,
	        displayName>), typeof(string))]
		[PXUIField(DisplayName = "Member Name", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
		[PXString(255, IsUnicode = true)]
		public virtual string MemberName { get; set; }

		#endregion

		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXDBIdentity(IsKey = true, BqlField = typeof(contactID))]
		[PXUIField(DisplayName = "Contact ID", Visibility = PXUIVisibility.Invisible)]
		[PXPersonalDataWarning]
		public virtual Int32? ContactID { get; set; }
		#endregion
		
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }

		[PXDBInt]
		[PXDefault(0)]
		[AddressRevisionID()]
		public virtual Int32? RevisionID { get; set; }
		#endregion

		#region IsAddressSameAsMain
		public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }

		[PXBool]
		[PXUIField(DisplayName = "Same as in Account")]
		public virtual bool? IsAddressSameAsMain { get; set; }
		#endregion

		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
	    protected Int32? _defAddressID;
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

		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }

		[PXDBString(50, IsUnicode = true)]
		[Titles]
		[PXUIField(DisplayName = "Title")]
		[PXMassMergableField]
		public virtual String Title { get; set; }
		#endregion

		#region FirstName
		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
        public virtual String FirstName { get; set; }
		#endregion

		#region MidName
		public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Middle Name")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String MidName { get; set; }
		#endregion

		#region LastName
		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }

		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name")]
		[CRLastNameDefault]
		[PXMassMergableField]
		[PXPersonalDataField]
        public virtual String LastName { get; set; }
		#endregion

		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXMassUpdatableField]
		[PXPersonalDataField]
        public virtual String Salutation { get; set; }
		#endregion

		#region Attention
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Attention")]
		[PXMassMergableField]
		[PXMassUpdatableField]
		[PXPersonalDataField]
		public virtual String Attention { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXDBInt]
		[CRContactBAccountDefault]
		[PXParent(typeof(Select<BAccount, 
			Where<BAccount.bAccountID, Equal<Current<Contact.bAccountID>>, 
			And<BAccount.type, NotEqual<BAccountType.combinedType>>>>))]
		[PXUIField(DisplayName = "Business Account")]
		[PXSelector(typeof(BAccount.bAccountID), SubstituteKey = typeof(BAccount.acctCD), DescriptionField = typeof(BAccount.acctName), DirtyRead = true)]
		[PXMassUpdatableField]
		public virtual Int32? BAccountID { get; set; }
		#endregion

		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
	    private string _fullName;

	    [PXMassMergableField]
	    [PXDBString(255, IsUnicode = true)]
	    [PXUIField(DisplayName = "Company Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXPersonalDataField]
	    public virtual String FullName
	    {
	        get { return _fullName; }
	        set { _fullName = value; }
	    }
		#endregion

		#region ParentBAccountID
		public abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }		
		[CustomerProspectVendor(DisplayName = "Parent Account")]
		[PXMassUpdatableField]
		public virtual Int32? ParentBAccountID { get; set; }
		#endregion

		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
        private string _eMail;

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

		[PXDBWeblink]
		[PXUIField(DisplayName = "Web")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String WebSite { get; set; }
		#endregion

		#region Fax
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }

		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Fax")]
		[PhoneValidation()]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String Fax { get; set; }
		#endregion

		#region FaxType
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.BusinessFax, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Fax Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String FaxType { get; set; }
		#endregion

		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }

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

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 1 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone1Type { get; set; }
		#endregion

		#region Phone2
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }

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

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Cell, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 2 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone2Type { get; set; }
		#endregion

		#region Phone3
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }

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

		[PXDBString(3)]
		[PXDefault(PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 3 Type")]
		[PhoneTypes]
		[PXMassMergableField]
		public virtual String Phone3Type { get; set; }
		#endregion

		#region DateOfBirth
		public abstract class dateOfBirth : PX.Data.BQL.BqlDateTime.Field<dateOfBirth> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Date Of Birth")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual DateTime? DateOfBirth { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

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

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion

		#region IsNotEmployee
		public abstract class isNotEmployee : PX.Data.BQL.BqlBool.Field<isNotEmployee> { }
		
		[PXBool]
		[PXUIField(DisplayName = "Is Not Employee", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual bool? IsNotEmployee
		{
			get { return !(ContactType == ContactTypesAttribute.Employee); }
		}
		#endregion

		#region NoFax
		public abstract class noFax : PX.Data.BQL.BqlBool.Field<noFax> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Fax", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoFax { get; set; }
		#endregion

		#region NoMail
		public abstract class noMail : PX.Data.BQL.BqlBool.Field<noMail> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Mail", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMail { get; set; }
		#endregion

		#region NoMarketing
		public abstract class noMarketing : PX.Data.BQL.BqlBool.Field<noMarketing> { }

		[PXDBBool]
		[PXUIField(DisplayName = "No Marketing", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMarketing { get; set; }
		#endregion

		#region NoCall
		public abstract class noCall : PX.Data.BQL.BqlBool.Field<noCall> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Call", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoCall { get; set; }
		#endregion

		#region NoEMail
		public abstract class noEMail : PX.Data.BQL.BqlBool.Field<noEMail> { }

		[PXDBBool]
		[PXUIField(DisplayName = "Do Not Email", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
        [PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
		public virtual bool? NoEMail { get; set; }
		#endregion

		#region NoMassMail
		public abstract class noMassMail : PX.Data.BQL.BqlBool.Field<noMassMail> { }

		[PXDBBool]
		[PXUIField(DisplayName = "No Mass Mail", FieldClass = FeaturesSet.customerModule.FieldClass)]
		[PXMassUpdatableField]
		[PXDefault(false)]
		[PXPersonalDataField(DefaultValue = false)]
        public virtual bool? NoMassMail { get; set; }
		#endregion

		#region Gender
		public abstract class gender : PX.Data.BQL.BqlString.Field<gender> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Gender")]
		[Genders(typeof(Contact.title))]
		[PXPersonalDataField]
		public virtual String Gender { get; set; }
		#endregion

		#region MaritalStatus
		public abstract class maritalStatus : PX.Data.BQL.BqlString.Field<maritalStatus> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Marital Status")]
		[MaritalStatuses]
		[PXPersonalDataField]
		public virtual String MaritalStatus { get; set; }
		#endregion

	    #region Anniversary
		public abstract class anniversary : PX.Data.BQL.BqlDateTime.Field<anniversary> { }

		[PXDBDate]
		[PXUIField(DisplayName = "Wedding Date")]
		public virtual DateTime? Anniversary { get; set; }
		#endregion

		#region Spouse
		public abstract class spouse : PX.Data.BQL.BqlString.Field<spouse> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Spouse/Partner Name")]
		[PXPersonalDataField]
		public virtual String Spouse { get; set; }
		#endregion

		#region Img
		public abstract class img : PX.Data.BQL.BqlString.Field<img> { }
		[PXUIField(DisplayName = "Image", Visibility = PXUIVisibility.Invisible)]
		[PXDBString(IsUnicode = true)]
		public string Img { get; set; }	
		#endregion

		#region Synchronize
		public abstract class synchronize : PX.Data.BQL.BqlBool.Field<synchronize> { }

		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Synchronize to Exchange", FieldClass = FeaturesSet.exchangeIntegration.FieldClass)]
		public virtual bool? Synchronize { get; set; }
		#endregion

		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }

		[PXDBString(2, IsFixed = true)]
		[PXDefault(ContactTypesAttribute.Person)]
		[ContactTypes]
		[PXUIField(DisplayName = "Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String ContactType { get; set; }
		#endregion

		#region ContactPriority
		public abstract class contactPriority : PX.Data.BQL.BqlInt.Field<contactPriority> { }
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
		[PXBool]
		[PXUIField(DisplayName = "Duplicate Found")]
		[PXDBCalced(typeof(Switch<Case<Where<Contact.duplicateStatus, Equal<DuplicateStatusAttribute.possibleDuplicated>,
			And<FeatureInstalled<FeaturesSet.contactDuplicate>>>, True>,False>), typeof(Boolean))]
		public virtual bool? DuplicateFound { get; set; }
		#endregion

		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Status")]
		[PXStringList(new string[0], new string[0])]
		public virtual String Status { get; set; }

		#endregion

		#region Resolution
		public abstract class resolution : PX.Data.BQL.BqlString.Field<resolution> { }

		[PXDBString(2, IsFixed = true)]
		[PXStringList(new string[0], new string[0])]
		[PXUIField(DisplayName = "Reason")]
		public virtual String Resolution { get; set; }
		#endregion

		#region AssignDate
		public abstract class assignDate : PX.Data.BQL.BqlDateTime.Field<assignDate> { }

		private DateTime? _assignDate;
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

		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Class ID")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		[PXMassMergableField]
		[PXMassUpdatableField]
		public virtual String ClassID { get; set; }
		#endregion

		#region Source
		public abstract class source : PX.Data.BQL.BqlString.Field<source> { }

		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Source")]
		[PXMassMergableField]
		[CRMSources]
		public virtual String Source { get; set; }
		#endregion


		#region WorkgroupID
		public abstract class workgroupID : PX.Data.BQL.BqlInt.Field<workgroupID> { }

		[PXDBInt]
		[PXUIField(DisplayName = "Workgroup")]
		[PXCompanyTreeSelector]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual int? WorkgroupID { get; set; }
		#endregion

		#region OwnerID
		public abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }

		[PXDBGuid]
		[PXOwnerSelector(typeof(Contact.workgroupID))]
		[PXUIField(DisplayName = "Owner")]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual Guid? OwnerID { get; set; }

		#endregion

		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		[PXDBGuid]
		public virtual Guid? UserID { get; set; }
		#endregion

		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
		protected String _CampaignID;
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

		[PXDBString(1, IsFixed = true)]
		[CRContactMethods]
		[PXDefault(CRContactMethodsAttribute.Any, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contact Method")]
		[PXMassUpdatableField]
		[PXMassMergableField]
		public virtual String Method { get; set; }

		#endregion

		#region IsConvertable
		public abstract class isConvertable : PX.Data.BQL.BqlBool.Field<isConvertable> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Can Be Converted", Visible = false)]
		public virtual bool? IsConvertable { get; set; }
		#endregion

		#region GrammValidationDateTime
		public abstract class grammValidationDateTime : PX.Data.BQL.BqlDateTime.Field<grammValidationDateTime> { }
		protected DateTime? _GrammValidationDateTime;

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

		[PXConsentAgreementField]
		public virtual bool? ConsentAgreement { get; set; }
		#endregion

		#region ConsentDate
		public abstract class consentDate : PX.Data.BQL.BqlDateTime.Field<consentDate> { }

		[PXConsentDateField]
		public virtual DateTime? ConsentDate { get; set; }
		#endregion

		#region ConsentExpirationDate
		public abstract class consentExpirationDate : PX.Data.BQL.BqlDateTime.Field<consentExpirationDate> { }

		[PXConsentExpirationDateField]
		public virtual DateTime? ConsentExpirationDate { get; set; }
		#endregion

		#region Attributes
		public abstract class attributes : BqlAttributes.Field<attributes> { }

		[CRAttributesField(typeof(Contact.classID))]
		public virtual string[] Attributes { get; set; }
		#endregion

		#region IEmailMessageTarget Members

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
        public abstract class searchSuggestion : PX.Data.BQL.BqlString.Field<searchSuggestion> { }

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

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? DeletedDatabaseRecord { get; set; }
		#endregion
		
		#region NOT SUPPORTED
		bool? IContact.IsDefaultContact { get => null; set { } }
		int? IContact.BAccountContactID { get => null; set { } }
		#endregion

	}

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

	[Serializable]
	[PXProjection(typeof(Select2<Contact, LeftJoin<BAccount, On<BAccount.bAccountID, Equal<Contact.bAccountID>>>>), Persistent = false)]
	public class ContactAccount : Contact
	{
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;
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