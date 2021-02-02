using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.CR
{
	public interface IDefAddressAccessor
	{
		int? DefAddressID { get;set;}
	};

	[System.SerializableAttribute()]
	[PXProjection(typeof(Select2<Contact,
		LeftJoin<Address, On<Contact.bAccountID,
		Equal<Address.bAccountID>,
		And<Contact.defAddressID, Equal<Address.addressID>>>>>),
		Persistent = true)]
	[PXCacheName(Messages.ContactExtAddress)]
	public partial class ContactExtAddress : Address, IDefAddressAccessor
	{
		#region ContactBAccountID
		public abstract class contactBAccountID : PX.Data.BQL.BqlInt.Field<contactBAccountID> { }
		protected Int32? _ContactBAccountID;
		[PXDBInt(IsKey = true, BqlField = typeof(Contact.bAccountID))]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXUIField(DisplayName = "Business Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
				Equal<Current<ContactExtAddress.bAccountID>>,
			And<BAccount.type, NotEqual<BAccountType.combinedType>>>>),
			LeaveChildren = true)]
		public virtual Int32? ContactBAccountID
		{
			get
			{
				return this._ContactBAccountID;
			}
			set
			{
				this._ContactBAccountID = value;
			}
		}
		#endregion
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		[PXDBInt(IsKey = true, BqlField = typeof(Contact.contactID))]
		[PXDBLiteDefault(typeof(Contact.contactID))]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? ContactID
		{
			get
			{
				return this._ContactID;
			}
			set
			{
				this._ContactID = value;
			}
		}
		#endregion
		#region DefAddressID
		public abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
		protected Int32? _DefAddressID;
		[PXDBInt(BqlField = typeof(Contact.defAddressID))]
		[PXDBLiteDefault(typeof(Address.addressID))]
		[PXUIField(DisplayName = "Default Address", Visible = false, Visibility = PXUIVisibility.Invisible)]
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
		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		protected String _Title;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
		[Titles]
		[PXUIField(DisplayName = Messages.Position, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this._Title = value;
			}
		}
		#endregion
		#region Salutation
		public abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		protected String _Salutation;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = Messages.JobTitle)]
		public virtual String Salutation
		{
			get
			{
				return this._Salutation;
			}
			set
			{
				this._Salutation = value;
			}
		}
				#endregion
		#region FirstName
		public abstract class firstName : PX.Data.BQL.BqlString.Field<firstName> { }
		protected String _FirstName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.firstName))]
		[PXUIField(DisplayName = "First Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FirstName
		{
			get
			{
				return this._FirstName;
			}
			set
			{
				this._FirstName = value;
			}
		}
		#endregion
		#region MidName
		public abstract class midName : PX.Data.BQL.BqlString.Field<midName> { }
		protected String _MidName;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.midName))]
		[PXUIField(DisplayName = "Middle Name")]
		public virtual String MidName
		{
			get
			{
				return this._MidName;
			}
			set
			{
				this._MidName = value;
			}
		}
		#endregion
		#region LastName
		public abstract class lastName : PX.Data.BQL.BqlString.Field<lastName> { }
		protected String _LastName;
		[PXDBString(100, IsUnicode = true, BqlField = typeof(Contact.lastName))]
		[PXUIField(DisplayName = "Last Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String LastName
		{
			get
			{
				return this._LastName;
			}
			set
			{
				this._LastName = value;
			}
		}
		#endregion
		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		protected String _FullName;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.fullName))]
		public virtual String FullName
		{
			get
			{
				return this._FullName;
			}
			set
			{
				this._FullName = value;
			}
		}
		#endregion
		#region EMail
		public abstract class eMail : PX.Data.BQL.BqlString.Field<eMail> { }
		protected String _EMail;
		[PXDBEmail(BqlField = typeof(Contact.eMail))]
		[PXUIField(DisplayName = "Email")]
		public virtual String EMail
		{
			get
			{
				return this._EMail;
			}
			set
			{
				this._EMail = value;
			}
		}
		#endregion
		#region WebSite
		public abstract class webSite : PX.Data.BQL.BqlString.Field<webSite> { }
		protected String _WebSite;
		[PXDBWeblink(BqlField = typeof(Contact.webSite))]
		[PXUIField(DisplayName = "Web")]
		public virtual String WebSite
		{
			get
			{
				return this._WebSite;
			}
			set
			{
				this._WebSite = value;
			}
		}
		#endregion
		#region Fax
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		protected String _Fax;
		[PXDBString(50, BqlField = typeof(Contact.fax))]
		[PXUIField(DisplayName = "Fax")]
		[PhoneValidation()]
		public virtual String Fax
		{
			get
			{
				return this._Fax;
			}
			set
			{
				this._Fax = value;
			}
		}
		#endregion
		#region FaxType
		public abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }
		protected String _FaxType;
		[PXDBString(3,BqlField = typeof(Contact.faxType))]
		[PXDefault(PhoneTypesAttribute.BusinessFax)]
		[PXUIField(DisplayName = "Fax")]
		[PhoneTypes]
		public virtual String FaxType
		{
			get
			{
				return this._FaxType;
			}
			set
			{
				this._FaxType = value;
			}
		}
		#endregion
		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		protected String _Phone1;
		[PXDBString(50, BqlField = typeof(Contact.phone1))]
		[PXUIField(DisplayName = "Phone 1")]
		[PhoneValidation()]
		public virtual String Phone1
		{
			get
			{
				return this._Phone1;
			}
			set
			{
				this._Phone1 = value;
			}
		}
		#endregion
		#region Phone1Type
		public abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }
		protected String _Phone1Type;
		[PXDBString(3, BqlField = typeof(Contact.phone1Type))]
		[PXDefault(PhoneTypesAttribute.Business1)]
		[PXUIField(DisplayName = "Phone 1")]
		[PhoneTypes]
		public virtual String Phone1Type
		{
			get
			{
				return this._Phone1Type;
			}
			set
			{
				this._Phone1Type = value;
			}
		}
				#endregion
		#region Phone2
		public abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }
		protected String _Phone2;
		[PXDBString(50, BqlField = typeof(Contact.phone2))]
		[PXUIField(DisplayName = "Phone 2")]
		[PhoneValidation()]
		public virtual String Phone2
		{
			get
			{
				return this._Phone2;
			}
			set
			{
				this._Phone2 = value;
			}
		}
		#endregion
		#region Phone2Type
		public abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }
		protected String _Phone2Type;
		[PXDBString(3, BqlField = typeof(Contact.phone2Type))]
		[PXDefault(PhoneTypesAttribute.Business2)]
		[PXUIField(DisplayName = "Phone 2")]
		[PhoneTypes]
		public virtual String Phone2Type
		{
			get
			{
				return this._Phone2Type;
			}
			set
			{
				this._Phone2Type = value;
			}
		}
				#endregion
		#region Phone3
		public abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }
		protected String _Phone3;
		[PXDBString(50, BqlField = typeof(Contact.phone3))]
		[PXUIField(DisplayName = "Phone 3")]
		[PhoneValidation()]
		public virtual String Phone3
		{
			get
			{
				return this._Phone3;
			}
			set
			{
				this._Phone3 = value;
			}
		}
		#endregion
		#region Phone3Type
		public abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }
		protected String _Phone3Type;
		[PXDBString(3, BqlField = typeof(Contact.phone3Type))]
		[PXDefault(PhoneTypesAttribute.Business3)]
		[PXUIField(DisplayName = "Phone 3")]
		[PhoneTypes]
		public virtual String Phone3Type
		{
			get
			{
				return this._Phone3Type;
			}
			set
			{
				this._Phone3Type = value;
			}
		}
		#endregion
		#region DateOfBirth
		public abstract class dateOfBirth : PX.Data.BQL.BqlDateTime.Field<dateOfBirth> { }
		protected DateTime? _DateOfBirth;
		[PXDBDate(BqlField = typeof(Contact.dateOfBirth))]
		[PXUIField(DisplayName = "Date Of Birth")]
		public virtual DateTime? DateOfBirth
		{
			get
			{
				return this._DateOfBirth;
			}
			set
			{
				this._DateOfBirth = value;
			}
		}
		#endregion
		#region ContactType
		public abstract class contactType : PX.Data.BQL.BqlString.Field<contactType> { }
		protected String _ContactType;
		[PXDBString(2, IsFixed = true, BqlField = typeof(Contact.contactType))]
		[PXDefault(ContactTypesAttribute.Person)]
		public virtual String ContactType
		{
			get
			{
				return this._ContactType;
			}
			set
			{
				this._ContactType = value;
			}
		}
			#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		protected bool? _IsActive;
		[PXDBBool(BqlField = typeof(Contact.isActive))]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive
		{
			get
			{
				return this._IsActive;
			}
			set
			{
				this._IsActive = value;
			}
		}
		#endregion
		#region ContactNoteID
		public abstract class contactNoteID : PX.Data.BQL.BqlGuid.Field<contactNoteID> { }
		protected Guid? _ContactNoteID;
		[PXNote(BqlField = typeof(Contact.noteID))]
		public virtual Guid? ContactNoteID
		{
			get
			{
				return this._ContactNoteID;
			}
			set
			{
				this._ContactNoteID = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class contactCreatedByID : PX.Data.BQL.BqlGuid.Field<contactCreatedByID> { }
		protected Guid? _ContactCreatedByID;
		[PXDBCreatedByID(BqlField = typeof(Contact.createdByID))]
		public virtual Guid? ContactCreatedByID
		{
			get
			{
				return this._ContactCreatedByID;
			}
			set
			{
				this._ContactCreatedByID = value;
			}
		}
		#endregion
		#region ContactCreatedByScreenID
		public abstract class contactCreatedByScreenID : PX.Data.BQL.BqlString.Field<contactCreatedByScreenID> { }
		protected String _ContactCreatedByScreenID;
		[PXDBCreatedByScreenID(BqlField = typeof(Contact.createdByScreenID))]
		public virtual String ContactCreatedByScreenID
		{
			get
			{
				return this._ContactCreatedByScreenID;
			}
			set
			{
				this._ContactCreatedByScreenID = value;
			}
		}
		#endregion
		#region ContactCreatedDateTime
		public abstract class contactCreatedDateTime : PX.Data.BQL.BqlDateTime.Field<contactCreatedDateTime> { }
		protected DateTime? _ContactCreatedDateTime;
		[PXDBCreatedDateTime(BqlField = typeof(Contact.createdDateTime))]
		public virtual DateTime? ContactCreatedDateTime
		{
			get
			{
				return this._ContactCreatedDateTime;
			}
			set
			{
				this._ContactCreatedDateTime = value;
			}
		}
		#endregion
		#region ContactLastModifiedByID
		public abstract class contactLastModifiedByID : PX.Data.BQL.BqlGuid.Field<contactLastModifiedByID> { }
		protected Guid? _ContactLastModifiedByID;
		[PXDBLastModifiedByID(BqlField = typeof(Contact.lastModifiedByID))]
		public virtual Guid? ContactLastModifiedByID
		{
			get
			{
				return this._ContactLastModifiedByID;
			}
			set
			{
				this._ContactLastModifiedByID = value;
			}
		}
		#endregion
		#region ContactLastModifiedByScreenID
		public abstract class contactLastModifiedByScreenID : PX.Data.BQL.BqlString.Field<contactLastModifiedByScreenID> { }
		protected String _ContactLastModifiedByScreenID;
		[PXDBLastModifiedByScreenID(BqlField = typeof(Contact.lastModifiedByScreenID))]
		public virtual String ContactLastModifiedByScreenID
		{
			get
			{
				return this._ContactLastModifiedByScreenID;
			}
			set
			{
				this._ContactLastModifiedByScreenID = value;
			}
		}
		#endregion
		#region ContactLastModifiedDateTime
		public abstract class contactLastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<contactLastModifiedDateTime> { }
		protected DateTime? _ContactLastModifiedDateTime;
		[PXDBLastModifiedDateTime(BqlField = typeof(Contact.lastModifiedDateTime))]
		public virtual DateTime? ContactLastModifiedDateTime
		{
			get
			{
				return this._ContactLastModifiedDateTime;
			}
			set
			{
				this._ContactLastModifiedDateTime = value;
			}
		}
		#endregion

		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected bool? _IsDefault;
		[PXBool()]
		[PXUIField(DisplayName = "Is Default", Visible = true)]
		public virtual bool? IsDefault
		{
			get
			{
				return this._IsDefault;
			}
			set
			{
				this._IsDefault = value;
			}
		}
		#endregion
        #region IsAddressSameAsMain
        public abstract class isAddressSameAsMain : PX.Data.BQL.BqlBool.Field<isAddressSameAsMain> { }
        protected bool? _IsAddressSameAsMain;
		[PXBool()]
		[PXUIField(DisplayName = "Same as Main")]
        public virtual bool? IsAddressSameAsMain
		{
			get
			{
                return this._IsAddressSameAsMain;
			}
			set
			{
                this._IsAddressSameAsMain = value;
			}
		}
		#endregion
		#region BAccountID
		public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXExtraKey()]
		[PXDBInt()]
		[PXDBLiteDefault(typeof(BAccount.bAccountID))]
		[PXUIField(DisplayName = "Business Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
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
		#region AddressID
		public new abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		[PXExtraKey()]
		[PXDBInt()]
		[PXUIField(DisplayName = "Address ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		public override Int32? AddressID
		{
			get
			{
				return this._AddressID;
			}
			set
			{
				this._AddressID = value;
			}
		}
		#endregion
		#region NoteID
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXDBGuid()]
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
        #region ExtraFields
        public abstract class contactDisplayName : PX.Data.BQL.BqlString.Field<contactDisplayName> { }

        protected String _ContactDisplayName;
        [PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [ContactDisplayName(typeof(ContactExtAddress.lastName), typeof(ContactExtAddress.firstName),
            typeof(ContactExtAddress.midName), typeof(ContactExtAddress.title), true,
            BqlField = typeof(Contact.displayName))]
		[PXNavigateSelector(typeof(Contact.displayName))]
        public virtual String ContactDisplayName
        {
            get { return _ContactDisplayName; }
            set { _ContactDisplayName = value; }
        }
        #endregion
    }
}
