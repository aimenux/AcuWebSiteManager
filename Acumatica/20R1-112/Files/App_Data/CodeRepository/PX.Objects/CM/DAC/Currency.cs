using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CM
{
	using System;
	using PX.Data;
	using PX.Data.ReferentialIntegrity.Attributes;
	using PX.Objects.GL;
	using PX.Objects.BQLConstants;

	/// <summary>
	/// Stores financial settings associated with currencies, thus complementing the <see cref="CurrencyList"/> DAC type.
	/// While <see cref="CurrencyList"/> holds only general information, such as code and precision, the <see cref="Currency"/> DAC provides information
	/// on all accounts and subaccounts associated with a particular currency, such as the Realized Gain and Loss account and subaccount.
	/// The <see cref="Currency"/> DAC also exposes fields with general currency information (such as <see cref="Description"/>),
	/// which are mapped to the corresponding fields in the <see cref="CurrencyList"/> DAC by means of <see cref="PXDBScalarAttribute"/>.
    /// The records of this type (as well as the <see cref="CurrencyList"/> records) are edited on the Currencies (CM202000) form (which corresponds to the <see cref="CurrencyMaint"/> graph).
	/// </summary>
	[PXPrimaryGraph(
		new Type[] { typeof(CurrencyMaint)},
		new Type[] { typeof(Select<Currency, 
			Where<Currency.curyID, Equal<Current<Currency.curyID>>>>)
		})]
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Currency)]
	public partial class Currency : PX.Data.IBqlTable
	{
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

        /// <summary>
        /// Key field.
        /// Unique identifier of the currency.
        /// </summary>
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDBDefault(typeof(CurrencyList.curyID))]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CurrencyList.curyID, Where<CurrencyList.curyID, NotEqual<Current<Company.baseCuryID>>>>), CacheGlobal = true)]
		[PXParent(typeof(Select<CurrencyList, Where<CurrencyList.curyID, Equal<Current<curyID>>>>))]
		[PX.Data.EP.PXFieldDescription]
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
		#region RealGainAcctID
		public abstract class realGainAcctID : PX.Data.BQL.BqlInt.Field<realGainAcctID> { }
		protected Int32? _RealGainAcctID;

        /// <summary>
        /// Identifier of the Realized Gain <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null,
			DisplayName = "Realized Gain Account", 
			Visibility = PXUIVisibility.Visible, 
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.realGainAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RealGainAcctID
		{
			get
			{
				return this._RealGainAcctID;
			}
			set
			{
				this._RealGainAcctID = value;
			}
		}
		#endregion
		#region RealGainSubID
		public abstract class realGainSubID : PX.Data.BQL.BqlInt.Field<realGainSubID> { }
		protected Int32? _RealGainSubID;

        /// <summary>
        /// Identifier of the Realized Gain <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.realGainAcctID), 
			DescriptionField = typeof(Sub.description), 
			DisplayName = "Realized Gain Subaccount", 
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.realGainSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RealGainSubID
		{
			get
			{
				return this._RealGainSubID;
			}
			set
			{
				this._RealGainSubID = value;
			}
		}
		#endregion
		#region RealLossAcctID
		public abstract class realLossAcctID : PX.Data.BQL.BqlInt.Field<realLossAcctID> { }
        protected Int32? _RealLossAcctID;

        /// <summary>
        /// Identifier of the Realized Loss <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Realized Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.realLossAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RealLossAcctID
		{
			get
			{
				return this._RealLossAcctID;
			}
			set
			{
				this._RealLossAcctID = value;
			}
		}
		#endregion
		#region RealLossSubID
		public abstract class realLossSubID : PX.Data.BQL.BqlInt.Field<realLossSubID> { }
		protected Int32? _RealLossSubID;

        /// <summary>
        /// Identifier of the Realized Loss <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.realLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Realized Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.realLossSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RealLossSubID
		{
			get
			{
				return this._RealLossSubID;
			}
			set
			{
				this._RealLossSubID = value;
			}
		}
		#endregion
		#region RevalGainAcctID
		public abstract class revalGainAcctID : PX.Data.BQL.BqlInt.Field<revalGainAcctID> { }
		protected Int32? _RevalGainAcctID;

        /// <summary>
        /// Identifier of the Revaluation Gain <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Revaluation Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.revalGainAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RevalGainAcctID
		{
			get
			{
				return this._RevalGainAcctID;
			}
			set
			{
				this._RevalGainAcctID = value;
			}
		}
		#endregion
		#region RevalGainSubID
		public abstract class revalGainSubID : PX.Data.BQL.BqlInt.Field<revalGainSubID> { }
		protected Int32? _RevalGainSubID;

        /// <summary>
        /// Identifier of the Revaluation Gain <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.revalGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Revaluation Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.revalGainSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RevalGainSubID
		{
			get
			{
				return this._RevalGainSubID;
			}
			set
			{
				this._RevalGainSubID = value;
			}
		}
		#endregion
		#region RevalLossAcctID
		public abstract class revalLossAcctID : PX.Data.BQL.BqlInt.Field<revalLossAcctID> { }
		protected Int32? _RevalLossAcctID;

        /// <summary>
        /// Identifier of the Revaluation Loss <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Revaluation Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.revalLossAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RevalLossAcctID
		{
			get
			{
				return this._RevalLossAcctID;
			}
			set
			{
				this._RevalLossAcctID = value;
			}
		}
		#endregion
		#region RevalLossSubID
		public abstract class revalLossSubID : PX.Data.BQL.BqlInt.Field<revalLossSubID> { }
		protected Int32? _RevalLossSubID;

        /// <summary>
        /// Identifier of the Revaluation Loss <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.revalLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Revaluation Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.revalLossSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RevalLossSubID
		{
			get
			{
				return this._RevalLossSubID;
			}
			set
			{
				this._RevalLossSubID = value;
			}
		}
		#endregion
		#region ARProvAcctID
		public abstract class aRProvAcctID : PX.Data.BQL.BqlInt.Field<aRProvAcctID> { }
		protected Int32? _ARProvAcctID;

        /// <summary>
        /// Identifier of the Accounts Receivable Provisioning <see cref="Account"/> associated with the currency.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account(null,
			DisplayName = "AR Provisioning Account",
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.aRProvAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? ARProvAcctID
		{
			get
			{
				return this._ARProvAcctID;
			}
			set
			{
				this._ARProvAcctID = value;
			}
		}
		#endregion
		#region ARProvSubID
		public abstract class aRProvSubID : PX.Data.BQL.BqlInt.Field<aRProvSubID> { }
		protected Int32? _ARProvSubID;

        /// <summary>
        /// Identifier of the Accounts Receivable Provisioning <see cref="Sub">Subaccount</see> associated with the currency.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount(typeof(Currency.aRProvAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "AR Provisioning Subaccount")]
		[PXForeignReference(typeof(Field<Currency.aRProvSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? ARProvSubID
		{
			get
			{
				return this._ARProvSubID;
			}
			set
			{
				this._ARProvSubID = value;
			}
		}
		#endregion
		#region APProvAcctID
		public abstract class aPProvAcctID : PX.Data.BQL.BqlInt.Field<aPProvAcctID> { }
		protected Int32? _APProvAcctID;

        /// <summary>
        /// Identifier of the Accounts Payable Provisioning <see cref="Account"/> associated with the currency.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[Account(null,
			DisplayName = "AP Provisioning Account",
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.aPProvAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? APProvAcctID
		{
			get
			{
				return this._APProvAcctID;
			}
			set
			{
				this._APProvAcctID = value;
			}
		}
		#endregion
		#region APProvSubID
		public abstract class aPProvSubID : PX.Data.BQL.BqlInt.Field<aPProvSubID> { }
        protected Int32? _APProvSubID;

        /// <summary>
        /// Identifier of the Accounts Payable Provisioning <see cref="Sub">Subaccount</see> associated with the currency.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[SubAccount(typeof(Currency.aPProvAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "AP Provisioning Subaccount")]
		[PXForeignReference(typeof(Field<Currency.aPProvSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? APProvSubID
		{
			get
			{
				return this._APProvSubID;
			}
			set
			{
				this._APProvSubID = value;
			}
		}
		#endregion
		#region TranslationGainAcctID
		public abstract class translationGainAcctID : PX.Data.BQL.BqlInt.Field<translationGainAcctID> { }
		protected Int32? _TranslationGainAcctID;

        /// <summary>
        /// Identifier of the Translation Gain <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[PXUIRequired(typeof(FeatureInstalled<FeaturesSet.finStatementCurTranslation>))]
		[Account(null, 
			DisplayName = "Translation Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.translationGainAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? TranslationGainAcctID
		{
			get
			{
				return this._TranslationGainAcctID;
			}
			set
			{
				this._TranslationGainAcctID = value;
			}
		}
		#endregion
		#region TranslationGainSubID
		public abstract class translationGainSubID : PX.Data.BQL.BqlInt.Field<translationGainSubID> { }
		protected Int32? _TranslationGainSubID;

        /// <summary>
        /// Identifier of the Translation Gain <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[PXUIRequired(typeof(FeatureInstalled<FeaturesSet.finStatementCurTranslation>))]
		[SubAccount(typeof(Currency.translationGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Translation Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.translationGainSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? TranslationGainSubID
		{
			get
			{
				return this._TranslationGainSubID;
			}
			set
			{
				this._TranslationGainSubID = value;
			}
		}
		#endregion
		#region TranslationLossAcctID
		public abstract class translationLossAcctID : PX.Data.BQL.BqlInt.Field<translationLossAcctID> { }
		protected Int32? _TranslationLossAcctID;

        /// <summary>
        /// Identifier of the Translation Loss <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[PXUIRequired(typeof(FeatureInstalled<FeaturesSet.finStatementCurTranslation>))]
		[Account(null, 
			DisplayName = "Translation Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.translationLossAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? TranslationLossAcctID
		{
			get
			{
				return this._TranslationLossAcctID;
			}
			set
			{
				this._TranslationLossAcctID = value;
			}
		}
		#endregion
		#region TranslationLossSubID
		public abstract class translationLossSubID : PX.Data.BQL.BqlInt.Field<translationLossSubID> { }
        protected Int32? _TranslationLossSubID;

        /// <summary>
        /// Identifier of the Translation Loss <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[PXUIRequired(typeof(FeatureInstalled<FeaturesSet.finStatementCurTranslation>))]
		[SubAccount(typeof(Currency.translationLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Translation Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.translationLossSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? TranslationLossSubID
		{
			get
			{
				return this._TranslationLossSubID;
			}
			set
			{
				this._TranslationLossSubID = value;
			}
		}
		#endregion
		#region UnrealizedGainAcctID
		public abstract class unrealizedGainAcctID : PX.Data.BQL.BqlInt.Field<unrealizedGainAcctID> { }
		protected Int32? _UnrealizedGainAcctID;

        /// <summary>
        /// Identifier of the Unrealized Gain <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Unrealized Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.unrealizedGainAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? UnrealizedGainAcctID
		{
			get
			{
				return this._UnrealizedGainAcctID;
			}
			set
			{
				this._UnrealizedGainAcctID = value;
			}
		}
		#endregion
		#region UnrealizedGainSubID
		public abstract class unrealizedGainSubID : PX.Data.BQL.BqlInt.Field<unrealizedGainSubID> { }
		protected Int32? _UnrealizedGainSubID;

        /// <summary>
        /// Identifier of the Unrealized Gain <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.unrealizedGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Unrealized Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.unrealizedGainSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? UnrealizedGainSubID
		{
			get
			{
				return this._UnrealizedGainSubID;
			}
			set
			{
				this._UnrealizedGainSubID = value;
			}
		}
		#endregion
		#region UnrealizedLossAcctID
		public abstract class unrealizedLossAcctID : PX.Data.BQL.BqlInt.Field<unrealizedLossAcctID> { }
        protected Int32? _UnrealizedLossAcctID;

        /// <summary>
        /// Identifier of the Unrealized Loss <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Unrealized Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.unrealizedLossAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? UnrealizedLossAcctID
		{
			get
			{
				return this._UnrealizedLossAcctID;
			}
			set
			{
				this._UnrealizedLossAcctID = value;
			}
		}
		#endregion
		#region UnrealizedLossSubID
		public abstract class unrealizedLossSubID : PX.Data.BQL.BqlInt.Field<unrealizedLossSubID> { }
        protected Int32? _UnrealizedLossSubID;

        /// <summary>
        /// Identifier of the Unrealized Loss <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.unrealizedLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Unrealized Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.unrealizedLossSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? UnrealizedLossSubID
		{
			get
			{
				return this._UnrealizedLossSubID;
			}
			set
			{
				this._UnrealizedLossSubID = value;
			}
		}
		#endregion
		#region RoundingGainAcctID
		public abstract class roundingGainAcctID : PX.Data.BQL.BqlInt.Field<roundingGainAcctID> { }
		protected Int32? _RoundingGainAcctID;

        /// <summary>
        /// Identifier of the Rounding Gain <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Rounding Gain Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.roundingGainAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RoundingGainAcctID
		{
			get
			{
				return this._RoundingGainAcctID;
			}
			set
			{
				this._RoundingGainAcctID = value;
			}
		}
		#endregion
		#region RoundingGainSubID
		public abstract class roundingGainSubID : PX.Data.BQL.BqlInt.Field<roundingGainSubID> { }
		protected Int32? _RoundingGainSubID;

        /// <summary>
        /// Identifier of the Rounding Gain <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.roundingGainAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Gain Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.roundingGainSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RoundingGainSubID
		{
			get
			{
				return this._RoundingGainSubID;
			}
			set
			{
				this._RoundingGainSubID = value;
			}
		}
		#endregion
		#region RoundingLossAcctID
		public abstract class roundingLossAcctID : PX.Data.BQL.BqlInt.Field<roundingLossAcctID> { }
        protected Int32? _RoundingLossAcctID;

        /// <summary>
        /// Identifier of the Rounding Loss <see cref="Account"/> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Account.AccountID"/> field.
        /// </value>
		[PXDefault]
		[Account(null, 
			DisplayName = "Rounding Loss Account",
			Visibility = PXUIVisibility.Visible,
			DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<Currency.roundingLossAcctID>.IsRelatedTo<Account.accountID>))]
		public virtual Int32? RoundingLossAcctID
		{
			get
			{
				return this._RoundingLossAcctID;
			}
			set
			{
				this._RoundingLossAcctID = value;
			}
		}
		#endregion
		#region RoundingLossSubID
		public abstract class roundingLossSubID : PX.Data.BQL.BqlInt.Field<roundingLossSubID> { }
		protected Int32? _RoundingLossSubID;

        /// <summary>
        /// Identifier of the Rounding Loss <see cref="Sub">Subaccount</see> associated with the currency.
        /// Required field.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Sub.SubID"/> field.
        /// </value>
		[PXDefault]
		[SubAccount(typeof(Currency.roundingLossAcctID),
			DescriptionField = typeof(Sub.description),
			DisplayName = "Rounding Loss Subaccount",
			Visibility = PXUIVisibility.Visible)]
		[PXForeignReference(typeof(Field<Currency.roundingLossSubID>.IsRelatedTo<Sub.subID>))]
		public virtual Int32? RoundingLossSubID
		{
			get
			{
				return this._RoundingLossSubID;
			}
			set
			{
				this._RoundingLossSubID = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The user-defined description of the currency.
        /// </summary>
		[PXString(IsUnicode = true)]
		[PXDBScalar(typeof(Search<CurrencyList.description, Where<CurrencyList.curyID, Equal<curyID>>>))]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region CurySymbol
		public abstract class curySymbol : PX.Data.BQL.BqlString.Field<curySymbol> { }
		protected String _CurySymbol;

        /// <summary>
        /// The symbol of the currency.
        /// </summary>
		[PXString(IsUnicode = true)]
		[PXDBScalar(typeof(Search<CurrencyList.curySymbol, Where<CurrencyList.curyID, Equal<curyID>>>))]
		[PXUIField(DisplayName = "Currency Symbol")]
		public virtual String CurySymbol
		{
			get
			{
				return this._CurySymbol;
			}
			set
			{
				this._CurySymbol = value;
			}
		}
		#endregion
		#region CuryCaption
		public abstract class curyCaption : PX.Data.BQL.BqlString.Field<curyCaption> { }
		protected String _CuryCaption;

        /// <summary>
        /// The caption (the name) of the currency.
        /// </summary>
		[PXString(IsUnicode = true)]
		[PXDBScalar(typeof(Search<CurrencyList.curyCaption, Where<CurrencyList.curyID, Equal<curyID>>>))]
		[PXUIField(DisplayName = "Currency Caption")]
		public virtual String CuryCaption
		{
			get
			{
				return this._CuryCaption;
			}
			set
			{
				this._CuryCaption = value;
			}
		}
		#endregion
		#region DecimalPlaces
		public abstract class decimalPlaces : PX.Data.BQL.BqlShort.Field<decimalPlaces> { }
		protected Int16? _DecimalPlaces;

        /// <summary>
        /// The number of digits after the decimal point used in operations with the currency.
        /// </summary>
        /// <value>
        /// Minimum allowed value is 0, maximum - 4.
        /// </value>
		[PXShort(MinValue = 0, MaxValue = 4)]
		[PXDBScalar(typeof(Search<CurrencyList.decimalPlaces, Where<CurrencyList.curyID, Equal<curyID>>>))]
		[PXUIField(DisplayName = "Decimal Precision")]
		public virtual short? DecimalPlaces
		{
			get
			{
				return this._DecimalPlaces;
			}
			set
			{
				this._DecimalPlaces = value;
			}
		}
        #endregion

        #region UseARPreferencesSettings
        public abstract class useARPreferencesSettings : PX.Data.BQL.BqlBool.Field<useARPreferencesSettings> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Use AR Preferences Settings")]
        public virtual bool? UseARPreferencesSettings { get; set; }
        #endregion
        #region ARInvoicePrecision
        public abstract class aRInvoicePrecision : PX.Data.BQL.BqlDecimal.Field<aRInvoicePrecision> { }

        [PXDBDecimalString(2)]
        [InvoicePrecision.List]
        [PXDefault(TypeCode.Decimal, InvoicePrecision.m005, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Rounding Precision")]
        public virtual decimal? ARInvoicePrecision { get; set; }
        #endregion
        #region ARInvoiceRounding
        public abstract class aRInvoiceRounding : PX.Data.BQL.BqlString.Field<aRInvoiceRounding> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(RoundingType.Currency, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Rounding Rule for Invoices")]
        [InvoiceRounding.List]
        public virtual string ARInvoiceRounding { get; set; }
        #endregion
        #region UseAPPreferencesSettings
        public abstract class useAPPreferencesSettings : PX.Data.BQL.BqlBool.Field<useAPPreferencesSettings> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Use AP Preferences Settings")]
        public virtual bool? UseAPPreferencesSettings { get; set; }
        #endregion
        #region APInvoicePrecision
        public abstract class aPInvoicePrecision : PX.Data.BQL.BqlDecimal.Field<aPInvoicePrecision> { }

        [PXDBDecimalString(2)]
        [InvoicePrecision.List]
        [PXDefault(TypeCode.Decimal, InvoicePrecision.m005, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Rounding Precision")]
        public virtual decimal? APInvoicePrecision { get; set; }
        #endregion
        #region APInvoiceRounding
        public abstract class aPInvoiceRounding : PX.Data.BQL.BqlString.Field<aPInvoiceRounding> { }

        [PXDBString(1, IsFixed = true)]
        [PXDefault(RoundingType.Currency, PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Rounding Rule for Bills")]
        [InvoiceRounding.List]
        public virtual string APInvoiceRounding { get; set; }
        #endregion

        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXNote(DescriptionField = typeof(Currency.curyID))]
		public virtual Guid? NoteID { get; set; }
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
	}
    [Serializable]
    [PXHidden]
	public partial class Currency2 : Currency
	{
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

		public new abstract class decimalPlaces : PX.Data.BQL.BqlShort.Field<decimalPlaces> { }
	}
}
