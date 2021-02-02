using PX.AddressValidator;
using PX.SM;

namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;

	/// <summary>
	/// Represents a country, in which the organization has operations, customers or vendors, and provides
    /// information used for defining <see cref="GL.Branch">Branches</see> and creating
    /// <see cref="AP.Vendor">Vendors</see> and <see cref="AR.Customer">Customers</see>.
    /// Records of this type are created and edited through the Countries/States (CS.20.40.00) screen
    /// (corresponds to the <see cref="CountryMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Country, PXDacType.Catalogue, CacheGlobal = true)]
	[PXPrimaryGraph(
		new Type[] { typeof(CountryMaint)},
		new Type[] { typeof(Select<Country, 
			Where<Country.countryID, Equal<Current<Country.countryID>>>>)
		})]
	public partial class Country : PX.Data.IBqlTable
	{
		#region Keys
		/// <exclude/>
		public class PK : PrimaryKeyOf<Country>.By<countryID>
		{
			public static Country Find(PXGraph graph, string countryID) => FindBy(graph, countryID);
		}
		#endregion

		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;

        /// <summary>
        /// Key field.
        /// The unique two-letter identifier of the Country.
        /// </summary>
        /// <value>
        /// The identifiers of the countries are defined by the ISO 3166 standard.
        /// </value>
		[PXDBString(100, IsKey = true/*, IsFixed = true*/)]
		[PXDefault()]
		[PXUIField(DisplayName = "Country ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Country.countryID), CacheGlobal = true, DescriptionField = typeof(Country.description))]
		[PXReferentialIntegrityCheck]
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
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The complete name of the Country.
        /// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Country Name", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion


		#region CountryValidationMethod
		public abstract class countryValidationMethod : PX.Data.BQL.BqlString.Field<countryValidationMethod>
		{
			public const string ID = "I";
			public const string Name = "N";
			public const string NameRegex = "R";

			public class PXCountryValidationMethodAttribute : PXStringListAttribute
			{
				public PXCountryValidationMethodAttribute()
					: base(
					new [] { ID, Name, NameRegex },
					new [] { Messages.ValidationCountryID, Messages.ValidationCountryName, Messages.ValidationCountryNameRegex }) { }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[countryValidationMethod.PXCountryValidationMethod]
		[PXDefault(countryValidationMethod.ID)]
		[PXUIField(DisplayName = "Validation Mode")]
		public virtual string CountryValidationMethod { get; set; }
		#endregion
		#region CountryRegexp
		public abstract class countryRegexp : PX.Data.BQL.BqlString.Field<countryRegexp> { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Validation Regexp")]
		public virtual String CountryRegexp { get; set; }
		#endregion
		#region StateValidationMethod
		public abstract class stateValidationMethod : PX.Data.BQL.BqlString.Field<stateValidationMethod>
		{
			public const string No = "X";
			public const string ID = "I";
			public const string Name = "N";
			public const string NameRegex = "R";

			public class PXStateValidationMethodAttribute : PXStringListAttribute
			{
				public PXStateValidationMethodAttribute()
					: base(
					new[] { No, ID, Name, NameRegex },
					new[] { Messages.ValidationNoValidation, Messages.ValidationStateID, Messages.ValidationStateName, Messages.ValidationStateNameRegex })
				{ }
			}
		}
		[PXDBString(1, IsFixed = true)]
		[stateValidationMethod.PXStateValidationMethod]
		[PXDefault(stateValidationMethod.ID)]
		[PXUIField(DisplayName = "Validation Mode")]
		public virtual string StateValidationMethod { get; set; }
		#endregion

		#region ZipCodeMask
		public abstract class zipCodeMask : PX.Data.BQL.BqlString.Field<zipCodeMask> { }
		protected String _ZipCodeMask;

        /// <summary>
        /// A mask that is used to validate postal codes belonging to this Country, when they are entered.
        /// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Input Mask")]
		public virtual String ZipCodeMask
		{
			get
			{
				return this._ZipCodeMask;
			}
			set
			{
				this._ZipCodeMask = value;
			}
		}
		#endregion
		#region ZipCodeRegexp
		public abstract class zipCodeRegexp : PX.Data.BQL.BqlString.Field<zipCodeRegexp> { }
		protected String _ZipCodeRegexp;

        /// <summary>
        /// A regular expression that is used to validate postal codes belonging to this Country, when they are entered.
        /// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Validation Regexp")]
		public virtual String ZipCodeRegexp
		{
			get
			{
				return this._ZipCodeRegexp;
			}
			set
			{
				this._ZipCodeRegexp = value;
			}
		}
		#endregion
		#region PhoneCountryCode
		public abstract class phoneCountryCode : PX.Data.BQL.BqlString.Field<phoneCountryCode> { }
		protected String _PhoneCountryCode;

        /// <summary>
        /// The phone code of the Country.
        /// </summary>
		[PXDBString(5)]
		[PXUIField(DisplayName = "Country Phone Code")]
		public virtual String PhoneCountryCode
		{
			get
			{
				return this._PhoneCountryCode;
			}
			set
			{
				this._PhoneCountryCode = value;
			}
		}
		#endregion
		#region PhoneMask
		public abstract class phoneMask : PX.Data.BQL.BqlString.Field<phoneMask> { }
		protected String _PhoneMask;

        /// <summary>
        /// A mask that is used to validate phone numbers belonging to this Country, when they are entered.
        /// </summary>
		[PXDBString(50)]
		[PXUIField(DisplayName = "Input Mask")]
		public virtual String PhoneMask
		{
			get
			{
				return this._PhoneMask;
			}
			set
			{
				this._PhoneMask = value;
			}
		}
		#endregion
		#region PhoneRegexp
		public abstract class phoneRegexp : PX.Data.BQL.BqlString.Field<phoneRegexp> { }
        protected String _PhoneRegexp;

        /// <summary>
        /// A regular expression that is used to validate phone numbers belonging to this Country, when they are entered.
        /// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Phone Validation Reg. Exp.")]
		public virtual String PhoneRegexp
		{
			get
			{
				return this._PhoneRegexp;
			}
			set
			{
				this._PhoneRegexp = value;
			}
		}
		#endregion
		#region IsTaxRegistrationRequired
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class isTaxRegistrationRequired : PX.Data.BQL.BqlBool.Field<isTaxRegistrationRequired> { }
        /// <summary>
        /// !REV!
        /// Obsolete field.
        /// </summary>
		[PXDBBool]
		[PXUIField(DisplayName = "Tax Registration Required")]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual bool? IsTaxRegistrationRequired
		{
			get;
			set;
		}
		#endregion
		#region TaxRegistrationMask
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class taxRegistrationMask : PX.Data.BQL.BqlString.Field<taxRegistrationMask> { }
        /// <summary>
        /// !REV!
        /// Obsolete field.
        /// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Tax Registration Mask")]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual string TaxRegistrationMask
		{
			get;
			set;
		}
		#endregion
		#region TaxRegistrationRegexp
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class taxRegistrationRegexp : PX.Data.BQL.BqlString.Field<taxRegistrationRegexp> { }
        /// <summary>
        /// !REV!
        /// Obsolete field.
        /// </summary>
		[PXDBString(255)]
		[PXUIField(DisplayName = "Tax Reg. Validation Reg. Exp.")]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public virtual string TaxRegistrationRegexp
		{
			get;
			set;
		}
		#endregion
		#region AddressValidatorPluginID
		public abstract class addressValidatorPluginID : PX.Data.BQL.BqlString.Field<addressValidatorPluginID> { }
		protected String _AddressValidatorPluginID;

		/// <value>
		/// <see cref="PX.Objects.CS.AddressValidatorPlugin.addressValidatorPluginID"/> of a address validator which will bu used. 
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Address Verification Plug-In")]
		[PXSelector(typeof(AddressValidatorPlugin.addressValidatorPluginID),DescriptionField = typeof(AddressValidatorPlugin.description))]
		[PXRestrictor(typeof(Where<AddressValidatorPlugin.isActive, Equal<True>>), Messages.AddressVerificationPluginIsNotActive)]
		public virtual String AddressValidatorPluginID
		{
			get
			{
				return this._AddressValidatorPluginID;
			}
			set
			{
				this._AddressValidatorPluginID = value;
			}
		}
		#endregion
		#region AutoOverrideAddress
		/// <exclude/>
		public abstract class autoOverrideAddress: PX.Data.BQL.BqlBool.Field<autoOverrideAddress> { }
		/// <exclude/>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Override Address Automatically")]
		public virtual bool? AutoOverrideAddress
		{
			get;
			set;
		}
		#endregion
		#region LanguageID
		public abstract class languageID : PX.Data.BQL.BqlString.Field<languageID> { }
		protected String _LanguageID;
		[PXDBString(10, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Language/Locale")]
		[PXSelector(typeof(Locale.localeName), DescriptionField = typeof(Locale.description))]
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
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
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
		
	}
}
