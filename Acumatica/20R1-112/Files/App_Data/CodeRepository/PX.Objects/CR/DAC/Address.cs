using System;
using System.Diagnostics;
using PX.CS.Contracts.Interfaces;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR.MassProcess;
using PX.Objects.CS;

namespace PX.Objects.CR
{

	[Serializable]
	[PXCacheName(Messages.Address)]
	[PXPersonalDataTable(typeof(
		Select<
			Address,
		Where<
			Address.addressID, Equal<Current<Contact.defAddressID>>>>))]
	[DebuggerDisplay("Address: AddressID = {AddressID,nq}, DisplayName = {DisplayName}")]
	public partial class Address : PX.Data.IBqlTable, IAddressBase, IValidatedAddress, IPXSelectable, INotable
	{
		#region Keys
		public class PK : PrimaryKeyOf<Address>.By<addressID>
		{
			public static Address Find(PXGraph graph, int? addressID) => FindBy(graph, addressID);
		}
		public static class FK
		{
			public class Country : CS.Country.PK.ForeignKeyOf<Address>.By<countryID> { }
			public class State : CS.State.PK.ForeignKeyOf<Address>.By<countryID, state> { }
		}
		#endregion
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion
		#region AddressID
		public abstract class addressID : PX.Data.BQL.BqlInt.Field<addressID> { }
		protected Int32? _AddressID;
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Address ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		public virtual Int32? AddressID
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
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = false)]
		[PXDBLiteDefault(typeof(BAccount.bAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Business Account ID", Visible = false, Enabled = false, Visibility = PXUIVisibility.Invisible)]
		[PXParent(typeof(Select<BAccount,
			Where<BAccount.bAccountID,
				Equal<Current<Address.bAccountID>>, 
			And<BAccount.type, NotEqual<BAccountType.combinedType>>>>)
			)]
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
		#region RevisionID
		public abstract class revisionID : PX.Data.BQL.BqlInt.Field<revisionID> { }
		protected Int32? _RevisionID;
		[PXDBInt()]
		[PXDefault(0)]
		[AddressRevisionID()]
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
		#region AddressType
		public abstract class addressType : PX.Data.BQL.BqlString.Field<addressType> { }
		protected String _AddressType;
		[PXDBString(2, IsFixed = true)]
		[PXDefault(AddressTypes.BusinessAddress)]
		[AddressTypes.List()]
		[PXUIField(DisplayName = "AddressType", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		public virtual String AddressType
		{
			get
			{
				return this._AddressType;
			}
			set
			{
				this._AddressType = value;
			}
		}
		#endregion
		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		[PXString]
		[PXUIField(DisplayName = "Address", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDBCalced(typeof(Add<IsNull<addressLine1, Empty>,
								Add<Switch<Case<Where<addressLine1, IsNotNull, And<addressLine2, IsNotNull>>, Space>, Empty>,
									Add<IsNull<addressLine2, Empty>,
										Add<Switch<Case<Where<addressLine3, IsNotNull, And<addressLine1, IsNotNull, Or<addressLine2, IsNotNull>>>, Space>, Empty>,
											Add<IsNull<addressLine3, Empty>, Empty>>>>>), typeof(string))]
		public virtual String DisplayName { get; set; }
		
		#endregion
		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		protected String _AddressLine1;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 1", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String AddressLine1
		{
			get
			{
				return this._AddressLine1;
			}
			set
			{
				this._AddressLine1 = value;
			}
		}
		#endregion
		#region AddressLine2
		public abstract class addressLine2 : PX.Data.BQL.BqlString.Field<addressLine2> { }
		protected String _AddressLine2;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 2")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String AddressLine2
		{
			get
			{
				return this._AddressLine2;
			}
			set
			{
				this._AddressLine2 = value;
			}
		}
		#endregion
		#region AddressLine3
		public abstract class addressLine3 : PX.Data.BQL.BqlString.Field<addressLine3> { }
		protected String _AddressLine3;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Line 3")]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String AddressLine3
		{
			get
			{
				return this._AddressLine3;
			}
			set
			{
				this._AddressLine3 = value;
			}
		}
		#endregion
		#region City
		public abstract class city : PX.Data.BQL.BqlString.Field<city> { }
		protected String _City;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "City", Visibility = PXUIVisibility.SelectorVisible)]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String City
		{
			get
			{
				return this._City;
			}
			set
			{
				this._City = value;
			}
		}
		#endregion
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;
		[PXDefault(typeof(Search<GL.Branch.countryID, Where<GL.Branch.branchID, Equal<Current<AccessInfo.branchID>>>>))]
		[PXDBString(100)]
		[PXUIField(DisplayName = "Country")]
		[Country]
		[PXMassMergableField]
		[PXForeignReference(typeof(FK.Country))]
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
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		protected String _State;
		[PXDBString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "State")]
		[State(typeof(Address.countryID))]
		[PXMassMergableField]
		[PXForeignReference(typeof(FK.State))]
		public virtual String State
		{
			get
			{
				return this._State;
			}
			set
			{
				this._State = value;
			}
		}
		#endregion
		#region PostalCode
		public abstract class postalCode : PX.Data.BQL.BqlString.Field<postalCode> { }
		protected String _PostalCode;
		[PXDBString(20)]
		[PXUIField(DisplayName = "Postal Code")]
		[PXZipValidation(typeof(Country.zipCodeRegexp), typeof(Country.zipCodeMask), countryIdField: typeof(Address.countryID))]
		[PXMassMergableField]
		[PXPersonalDataField]
		public virtual String PostalCode
		{
			get
			{
				return this._PostalCode;
			}
			set
			{
				this._PostalCode = value;
			}
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXUniqueNote(DescriptionField = typeof(Address.displayName))]
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
		#region IsValidated
		public abstract class isValidated : PX.Data.BQL.BqlBool.Field<isValidated> { }
		protected Boolean? _IsValidated;

		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBBool()]
		[CS.ValidatedAddress()]
		[PXUIField(DisplayName = "Validated", Enabled = false, FieldClass = CS.Messages.ValidateAddress)]
		public virtual Boolean? IsValidated
		{
			get
			{
				return this._IsValidated;
			}
			set
			{
				this._IsValidated = value;
			}
		}
		#endregion
        #region TaxLocationCode
        public abstract class taxLocationCode : PX.Data.BQL.BqlString.Field<taxLocationCode> { }
        protected String _TaxLocationCode;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Location Code")]
        [PXMassMergableField]
        public virtual String TaxLocationCode
        {
            get
            {
                return this._TaxLocationCode;
            }
            set
            {
                this._TaxLocationCode = value;
            }
        }
        #endregion
        #region TaxMunicipalCode
        public abstract class taxMunicipalCode : PX.Data.BQL.BqlString.Field<taxMunicipalCode> { }
        protected String _TaxMunicipalCode;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax Municipal Code")]
        [PXMassMergableField]
        public virtual String TaxMunicipalCode
        {
            get
            {
                return this._TaxMunicipalCode;
            }
            set
            {
                this._TaxMunicipalCode = value;
            }
        }
        #endregion
        #region TaxSchoolCode
        public abstract class taxSchoolCode : PX.Data.BQL.BqlString.Field<taxSchoolCode> { }
        protected String _TaxSchoolCode;
        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Tax School Code")]
        [PXMassMergableField]
        public virtual String TaxSchoolCode
        {
            get
            {
                return this._TaxSchoolCode;
            }
            set
            {
                this._TaxSchoolCode = value;
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

		#region AddressTypes
		public class AddressTypes
		{
			public class ListAttribute : PXStringListAttribute
			{
				public ListAttribute()
					: base(
					 new string[] { BusinessAddress, HomeAddress, OtherAddress },
					 new string[] { Messages.BusinessAddress, Messages.HomeAddress, Messages.OtherAddress }) { ; }
			}

			public const string BusinessAddress = "BS";
			public const string HomeAddress = "HM";
			public const string OtherAddress = "UN";

			public class businessAddress : PX.Data.BQL.BqlString.Constant<businessAddress>
			{
				public businessAddress() : base(BusinessAddress) { ;}
			}
			public class homeAddress : PX.Data.BQL.BqlString.Constant<homeAddress>
			{
				public homeAddress() : base(HomeAddress) { ;}
			}
			public class otherAddress : PX.Data.BQL.BqlString.Constant<otherAddress>
			{
				public otherAddress() : base(OtherAddress) { ;}
			}
		}
		#endregion
		
	}
}
