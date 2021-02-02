using System;

using PX.Data;

using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.AR
{
	/// <summary>
	/// Represents a contact that is specified in a customer document,
	/// such as <see cref="ARInvoice.BillContactID">an invoice's billing contact</see>.
	/// An <see cref="ARContact"/> record is a copy of customer location's
	/// <see cref="Contact"/> and can be used to override the contact at the document level.
	/// The record is independent of changes to the original <see cref="Contact"/> record.
	/// The entities of this type are created and edited on the Invoices and Memos
	/// (AR301000) form, which corresponds to the <see cref="ARInvoiceEntry"/> graph.
	/// </summary>
	[Serializable()]
	[PXCacheName(Messages.ARContact)]
	public partial class ARContact : IBqlTable, IContact, CRDefaultMailToAttribute.IEmailMessageTarget
	{
		#region ContactID
		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		protected Int32? _ContactID;
		/// <summary>
		/// The unique integer identifier of the record.
		/// This field is the primary key field.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Contact ID", Visible = false)]
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		/// <summary>
		/// The identifier of the <see cref="Customer"/> record,
		/// which is specified in the document to which the contact belongs. 
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Customer.BAccountID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDBDefault(typeof(ARRegister.customerID))]
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
		/// <summary>
		/// An alias for <see cref="CustomerID"/>, which exists
		/// for the purpose of implementing the <see cref="IContact"/> 
		/// interface.
		/// </summary>
		public virtual Int32? BAccountID
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
		#region CustomerContactID
		public abstract class customerContactID : PX.Data.BQL.BqlInt.Field<customerContactID> { }
		protected Int32? _CustomerContactID;
		/// <summary>
		/// The identifier of the <see cref="Contact"/> record
		/// from which the contact was originally created.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Contact.ContactID"/> field.
		/// </value>
		[PXDBInt()]
		public virtual Int32? CustomerContactID
		{
			get
			{
				return this._CustomerContactID;
			}
			set
			{
				this._CustomerContactID = value;
			}
		}
		/// <summary>
		/// An alias for <see cref="CustomerContactID"/>,
		/// which exists for the purpose of 
		/// implementing the <see cref="IContact"/> interface.
		/// </summary>
		public virtual Int32? BAccountContactID
		{
			get
			{
				return this._CustomerContactID;
			}
			set
			{
				this._CustomerContactID = value;
			}
		}
		#endregion
		#region IsDefaultContact
		public abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }
		protected Boolean? _IsDefaultContact;
		/// <summary>
		/// If set to <c>true</c>, indicates that the contact record
		/// is identical to the original <see cref="Contact"/> record
		/// referenced by the <see cref="CustomerContactID"/> field.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Default Customer Contact", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual Boolean? IsDefaultContact
		{
			get
			{
				return this._IsDefaultContact;
			}
			set
			{
				this._IsDefaultContact = value;
			}
		}
		#endregion
		#region OverrideContact
		public abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }
		/// <summary>
		/// If set to <c>true</c>, indicates that the contact
		/// overrides the default <see cref="Contact"/> record
		/// referenced by the <see cref="CustomerContactID"/> field.
		/// </summary>
		/// <value>
		/// This field is the inverse of <see cref="IsDefaultContact"/>.
		/// </value>
		[PXBool()]
		[PXUIField(DisplayName = "Override Contact", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? OverrideContact
		{
			[PXDependsOnFields(typeof(isDefaultContact))]
			get
			{
				return (this._IsDefaultContact == null ? this._IsDefaultContact : this._IsDefaultContact == false);
			}
			set
			{
				this._IsDefaultContact = (value == null ? value : value == false);
			}
		}
		#endregion
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		/// <summary>
		/// The revision ID of the original <see cref="Contact"/>
		/// record from which the record originates.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="Contact.RevisionID"/> field.
		/// </value>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? RevisionID
		{
			get
			{
				return this._RevisionID;
			}
			set
			{
				this._RevisionID = value;
			}
		}
		#endregion
		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		protected String _Title;
		/// <summary>
		/// The contact's title.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined 
		/// by <see cref="TitlesAttribute"/>.
		/// </value>
		[PXDBString(50, IsUnicode = true)]
		[CR.Titles]
		[PXUIField(DisplayName = "Title")]
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
		/// <summary>
		/// The name of the contact person. This field is usually
		/// used in notification templates, as shown in the following 
		/// example: <c>Dear {Contact.Salutation}!</c>.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Job Title", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
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
		#region Attention
		public abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Attention", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String Attention { get; set; }
		#endregion
		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		protected String _FullName;
		/// <summary>
		/// The business name or the company name of the contact.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Company Name")]
		[PXPersonalDataField]
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
		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		protected String _Email;
		/// <summary>
		/// The e-mail address of the contact.
		/// </summary>
		[PXDBEmail]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXPersonalDataField]
		public virtual String Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
			}
		}
		#endregion
		#region Fax
		public abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		protected String _Fax;
		/// <summary>
		/// The fax number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Fax")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
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
		/// <summary>
		/// The type of the fax number of the contact.
		/// </summary>
		/// <value>
		/// This field can have one of the values
		/// defined by <see cref="PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.BusinessFax, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Fax")]
		[CR.PhoneTypes]
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
		/// <summary>
		/// The first phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 1", Visibility = PXUIVisibility.SelectorVisible)]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
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
		/// <summary>
		/// The type of the first phone number of the contact.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined 
		/// by <see cref="PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Business1, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 1")]
		[CR.PhoneTypes]
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
		/// <summary>
		/// The second phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 2")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
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
		/// <summary>
		/// The type of the second phone number of the contact.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined 
		/// by <see cref="PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Business2, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 2")]
		[CR.PhoneTypes]
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
		/// <summary>
		/// The third phone number of the contact.
		/// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Phone 3")]
		[CR.PhoneValidation()]
		[PXPersonalDataField]
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
		/// <summary>
		/// The type of the third phone number of the contact.
		/// </summary>
		/// <value>
		/// This field can have one of the values defined 
		/// by <see cref="PhoneTypesAttribute"/>.
		/// </value>
		[PXDBString(3)]
		[PXDefault(CR.PhoneTypesAttribute.Home, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone 3")]
		[CR.PhoneTypes]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXDBGuidNotNull]
		public virtual Guid? NoteID { get; set; }
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
		[PXDBLastModifiedByID]
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

		#region IEmailMessageTarget Members
		public string Address
		{
			get { return Email; }
		}
		public string DisplayName
		{
			get { return FullName; }
		}
		#endregion
	}

	[Serializable]
	[PXCacheName(Messages.ARContact)]
	[PXBreakInheritance]
	public partial class ARShippingContact : ARContact
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		public new abstract class customerContactID : PX.Data.BQL.BqlInt.Field<customerContactID> { }
		public new abstract class isDefaultContact : PX.Data.BQL.BqlBool.Field<isDefaultContact> { }
		public new abstract class overrideContact : PX.Data.BQL.BqlBool.Field<overrideContact> { }
		public new abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		public new abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		public new abstract class salutation : PX.Data.BQL.BqlString.Field<salutation> { }
		public new abstract class attention : PX.Data.BQL.BqlString.Field<attention> { }
		public new abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		public new abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		public new abstract class fax : PX.Data.BQL.BqlString.Field<fax> { }
		public new abstract class faxType : PX.Data.BQL.BqlString.Field<faxType> { }
		public new abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		public new abstract class phone1Type : PX.Data.BQL.BqlString.Field<phone1Type> { }
		public new abstract class phone2 : PX.Data.BQL.BqlString.Field<phone2> { }
		public new abstract class phone2Type : PX.Data.BQL.BqlString.Field<phone2Type> { }
		public new abstract class phone3 : PX.Data.BQL.BqlString.Field<phone3> { }
		public new abstract class phone3Type : PX.Data.BQL.BqlString.Field<phone3Type> { }
		public new abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
	}
}
