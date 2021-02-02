using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.PM;

namespace PX.Objects.GL
{
	public class AccountType : ILabelProvider
	{
		public const string Asset = "A";
		public const string Liability = "L";
		public const string Income = "I";
		public const string Expense = "E";

		public static string[] COAOrderOptions = new string[] { "1233", "1234", "3412", "2311" };

		public IEnumerable<ValueLabelPair> ValueLabelPairs => new ValueLabelList
		{
			{ Asset, Messages.Asset },
			{ Liability, Messages.Liability },
			{ Income, Messages.Income },
			{ Expense, Messages.Expense },
		};

		public static int Ordinal(string Type)
		{
			switch (Type)
			{ 
				case Asset:
					return 0;
				case Liability:
					return 1;
				case Income:
					return 2;
				case Expense:
					return 3;
				default:
					throw new PXArgumentException();
			}
		}

		public static string Literal(short Ordinal)
		{
			switch (Ordinal)
			{
				case 0:
					return Asset;
				case 1:
					return Liability;
				case 2:
					return Income;
				case 3:
					return Expense;
				default:
					throw new PXArgumentException();
			}
		}

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Asset, Liability, Income, Expense },
				new string[] { Messages.Asset, Messages.Liability, Messages.Income, Messages.Expense }) { }
		}
		
		public class asset : PX.Data.BQL.BqlString.Constant<asset>
		{
			public asset() : base(Asset)
			{
			}
		}

		public class liability : PX.Data.BQL.BqlString.Constant<liability>
		{
			public liability() : base(Liability)
			{
			}
		}

		public class income : PX.Data.BQL.BqlString.Constant<income>
		{
			public income() : base(Income)
			{
			}
		}

		public class expense : PX.Data.BQL.BqlString.Constant<expense>
		{
			public expense() : base(Expense)
			{
			}
		}
	}

	public static class AccountPostOption 
	{
		public const string Summary ="S";
		public const string Detail = "D";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Summary, Detail},
				new string[] { Messages.PostSummary, Messages.PostDetail }) { }
		}
	}
	
    /// <summary>
    /// Represents an account of the General Ledger.
    /// The records of this type are edited through the Chart Of Accounts (GL.20.25.00) screen
    /// (corresponds to the <see cref="AccountMaint"/> graph).
    /// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.Account)]
	[PXPrimaryGraph(typeof(AccountHistoryByYearEnq), Filter = typeof(AccountByYearFilter))]
	public partial class Account : PX.Data.IBqlTable, PX.SM.IIncludable
	{
	    #region Keys
	    public class PK : PrimaryKeyOf<Account>.By<accountID>
	    {
	        public static Account Find(PXGraph graph, int? accountID) => FindBy(graph, accountID);
	    }
	    #endregion

        #region AccountID
        public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;

        /// <summary>
        /// Unique identifier of the account. Database identity.
        /// </summary>
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region AccountCD
		public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
		protected String _AccountCD;

        /// <summary>
        /// Key field.
        /// The user-friendly unique identifier of the account.
        /// </summary>
		[PXDefault()]
		[AccountRaw(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String AccountCD
		{
			[PXDependsOnFields(typeof(accountID))] // for correct navigation in reports
			get
			{
				return this._AccountCD;
			}
			set
			{
				this._AccountCD = value;
			}
		}
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.BQL.BqlString.Field<accountClassID> { }
		protected string _AccountClassID;

        /// <summary>
        /// Identifier of the <see cref="AccountClass">account class</see>, to which the account is assigned.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="AccountClass.AccountClassID"/> field.
        /// </value>
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Class", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(AccountClass.accountClassID), DescriptionField=typeof(AccountClass.descr))]
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
        protected string _Type;

        /// <summary>
        /// The type of the account.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"A"</c> - Asset,
        /// <c>"L"</c> - Liability,
        /// <c>"I"</c> - Income,
        /// <c>"E"</c> - Expense.
        /// Defaults to the <see cref="AccountClass.Type">type</see>, specified in the <see cref="AccountClassID">account class</see>.
        /// </value>
        [PXDBString(1, IsFixed = true)]
        [PXDefault(AccountType.Asset, typeof(Search<AccountClass.type, Where<AccountClass.accountClassID, Equal<Current<Account.accountClassID>>>>))]
        [AccountType.List()]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Type
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
		#region ControlAccountModule
		public abstract class controlAccountModule : PX.Data.BQL.BqlString.Field<controlAccountModule> { }
		/// <summary>
		/// Acumatica module for witch the Account is control account.
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"AP"</c> - Accounts Payable,
		/// <c>"AR"</c> - Accounts Receivable,
		/// <c>"TX"</c> - TX,
		/// <c>"FA"</c> - Fixed Assets,
		/// <c>"DR"</c> - Deferred Revenue,
		/// <c>"SO"</c> - Sales Order,
		/// <c>"PO"</c> - Purchase Order,
		/// <c>"IN"</c> - Inventory.
		/// Defaults to the <see cref="AccountClass.Type">type</see>, specified in the <see cref="AccountClassID">account class</see>.
		/// </value>
		[PXDBString(2, IsFixed = true)]
		[ControlAccountModule.List]
		[PXUIField(DisplayName = "Control Account Module", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string ControlAccountModule { get; set; }
		#endregion
		#region AllowManualEntry
		public abstract class allowManualEntry : PX.Data.BQL.BqlBool.Field<allowManualEntry> { }
		/// <summary>
		/// Are direct transations from all subledgers available to this account.
		/// </summary>
		/// <value>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Allow Manual Entry")]
		public virtual bool? AllowManualEntry { get; set; }
		#endregion
		#region COAOrder
		public abstract class cOAOrder : PX.Data.BQL.BqlShort.Field<cOAOrder> { }
		protected short? _COAOrder;

        /// <summary>
        /// The relative order of the account in the chart of accounts.
        /// </summary>
        /// <value>
        /// The value of this field is used to order accounts in the reports of the General Ledger module,
        /// when Custom Order option is selected in the <see cref="GLSetup.COAOrder"/> field of the <see cref="GLSetup">GL Preferences</see>.
        /// </value>
		[PXDBShort(MinValue = 0, MaxValue = 255)]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = Messages.COAOrder, Visibility = PXUIVisibility.Visible)]
		public virtual short? COAOrder
		{
			get
			{
				return this._COAOrder;
			}
			set
			{
				this._COAOrder = value;
			}
		}
		#endregion
        #region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		protected Boolean? _Active;

        /// <summary>
        /// Indicates whether the Account is active.
        /// </summary>
		[PXDBBool()]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? Active
		{
			get
			{
				return this._Active;
			}
			set
			{
				this._Active = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;

        /// <summary>
        /// The description of the account.
        /// </summary>
		[PXDBLocalizableString(60, IsUnicode = true)]
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
		#region PostOption
		public abstract class postOption : PX.Data.BQL.BqlString.Field<postOption> { }
		protected string _PostOption;

        /// <summary>
        /// Defines how the transactions created in other modules are posted to this account.
        /// In the scope of the account overrides the <see cref="PX.Objects.AP.APSetup.SummaryPost">APSetup.SummaryPost</see>,
        /// <see cref="PX.Objects.AR.ARSetup.SummaryPost">ARSetup.SummaryPost</see> and similar settings in other modules.
        /// </summary>
        /// <value>
        /// Allowed values are:
        /// <c>"S"</c> - Summary (transactions amounts are summarized for each account and subaccount pair),
        /// <c>"D"</c> - Details (each transaction from other module results in a <see cref="GLTran">journal transaction</see>.
        /// Defaults to <c>"D"</c> - Detail.
        /// </value>
		[PXDBString(1)]
		[PXDefault(AccountPostOption.Summary)]
		[AccountPostOption.List()]
		[PXUIField(DisplayName = "Post Option", Visibility = PXUIVisibility.Visible)]
		public virtual string PostOption
		{
			get
			{
				return this._PostOption;
			}
			set
			{
				this._PostOption = value;
			}
		}
		#endregion
		#region DirectPost
		public abstract class directPost : PX.Data.BQL.BqlBool.Field<directPost> { }
		protected Boolean? _DirectPost;

        /// <summary>
        /// Reserved for backward compatibility.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Direct Post", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? DirectPost
		{
			get
			{
				return this._DirectPost;
			}
			set
			{
				this._DirectPost = value;
			}
		}
		#endregion
		#region NoSubDetail
		public abstract class noSubDetail : PX.Data.BQL.BqlBool.Field<noSubDetail> { }
		protected Boolean? _NoSubDetail;

        /// <summary>
        /// If set to <c>true</c>, indicates that the system must set the subaccount to the <see cref="GLSetup.DefaultSubID">default subaccount</see>,
        /// when this account is selected for a document or transaction.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Use Default Subaccount", Visibility = PXUIVisibility.Visible)]
		public virtual Boolean? NoSubDetail
		{
			get
			{
				return this._NoSubDetail;
			}
			set
			{
				this._NoSubDetail = value;
			}
		}
		#endregion
		#region RequireUnits
		public abstract class requireUnits : PX.Data.BQL.BqlBool.Field<requireUnits> { }
		protected Boolean? _RequireUnits;

        /// <summary>
        /// When set to <c>true</c>, indicates that every transaction posted to this account must have
        /// <see cref="GLTran.Qty">Qunatity</see> and <see cref="GLTran.UOM">Units of Measure</see> specified.
        /// </summary>
        /// <value>
        /// Defaults to <c>false</c>.
        /// </value>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Require Units", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual Boolean? RequireUnits
		{
			get
			{
				return this._RequireUnits;
			}
			set
			{
				this._RequireUnits = value;
			}
		}
		#endregion
		#region GLConsolAccountCD
		public abstract class gLConsolAccountCD : PX.Data.BQL.BqlString.Field<gLConsolAccountCD> { }
		protected String _GLConsolAccountCD;

        /// <summary>
        /// The identifier of the external General Ledger account in the chart of accounts of the parent company,
        /// to which the balance of this account will be exported in the process of consolidation.
        /// This field is relevant only if the company is a consolidation unit in the parent company.
        /// </summary>
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Account", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(GLConsolAccount.accountCD), DescriptionField = typeof(GLConsolAccount.description))]
		public virtual String GLConsolAccountCD
		{
			get
			{
				return this._GLConsolAccountCD;
			}
			set
			{
				this._GLConsolAccountCD = value;
			}
		}
		#endregion
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected string _CuryID;

        /// <summary>
        /// Identifier of the <see cref="Currency"/> of the account.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Currency.CuryID"/> field.
        /// </value>
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Currency.curyID))]
		public virtual string CuryID
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
		#region AccountGroupID
		public abstract class accountGroupID : PX.Data.BQL.BqlInt.Field<accountGroupID> { }
		protected Int32? _AccountGroupID;

        /// <summary>
        /// Identifier of the <see cref="PMAccountGroup">Account Group</see>, that includes this account.
        /// Used only if the Projects module has been activated.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PMAccountGroup.GroupID"/> field.
        /// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Account Group", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDimensionSelector(AccountGroupAttribute.DimensionName, typeof(Search<PMAccountGroup.groupID, 
			Where<PMAccountGroup.isActive, Equal<True>,
				And2<
					Where2<Where<Current<Account.type>, Equal<AccountType.asset>,
							Or<Current<Account.type>, Equal<AccountType.liability>>>,
						And<Where<PMAccountGroup.type, Equal<AccountType.asset>, 
							Or<PMAccountGroup.type, Equal<AccountType.liability>>>>>,
					Or2<Where<PMAccountGroup.type, Equal<AccountType.expense>,
						And<Current<Account.type>, In3<AccountType.expense, AccountType.income, AccountType.asset, AccountType.liability>>>,
					Or<Where<PMAccountGroup.type, Equal<AccountType.income>>>
						>>>>),
				typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.groupCD), typeof(PMAccountGroup.description), typeof(PMAccountGroup.type), typeof(PMAccountGroup.isActive))]
		public virtual Int32? AccountGroupID
		{
			get
			{
				return this._AccountGroupID;
			}
			set
			{
				this._AccountGroupID = value;
			}
		}
		#endregion
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
		protected Byte[] _GroupMask;

        /// <summary>
        /// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the Account belongs to.
        /// To learn more about the way restriction groups are managed, see the documentation for the GL Account Access (GL.10.40.00) screen
        /// (corresponds to the <see cref="GLAccess"/> graph).
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
		#region RevalCuryRateTypeId
		public abstract class revalCuryRateTypeId : PX.Data.BQL.BqlString.Field<revalCuryRateTypeId> { }
		protected String _RevalCuryRateTypeId;

        /// <summary>
        /// The identifier of the <see cref="CurrencyRateType">Exchange Rate Type</see>
        /// that is used for the account in the process of revaluation.
        /// This field is required only for the accounts denominated to a foreign currency.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="CurrencyRateType.CuryRateTypeID"/> field.
        /// </value>
		[PXDBString(6, IsUnicode = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXSelector(typeof(CurrencyRateType.curyRateTypeID))]
		[PXUIField(DisplayName = "Revaluation Rate Type")]
		public virtual String RevalCuryRateTypeId
		{
			get
			{
				return this._RevalCuryRateTypeId;
			}
			set
			{
				this._RevalCuryRateTypeId = value;
			}
		}
		#endregion
		#region Box1099
		public abstract class box1099 : PX.Data.BQL.BqlShort.Field<box1099> { }
		protected Int16? _Box1099;

        /// <summary>
        /// The box on the 1099 form associated with this account.
        /// </summary>
		[PXDBShort()]
		[PXIntList(new int[] { 0 }, new string[] { Messages.Undefined })]
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
		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

        /// <summary>
        /// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the document.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
        /// </value>
		[PXNote(DescriptionField = typeof(Account.accountCD))]
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
		#region IsCashAccount
		public abstract class isCashAccount : PX.Data.BQL.BqlBool.Field<isCashAccount> { }
        protected bool? _IsCashAccount;

        /// <summary>
        /// Indicates whether the accounts has on or several <see cref="CashAccount">Cash Accounts</see> associated with it.
        /// </summary>
		[PXDBBool()]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual bool? IsCashAccount
		{
			get
			{
				return this._IsCashAccount;
			}
			set
			{
                this._IsCashAccount = value;
			}
		}
		#endregion

		#region TypeTotal
		public abstract class typeTotal : PX.Data.BQL.BqlString.Field<typeTotal> { }

        /// <summary>
        /// The read-only label used for totals of the <see cref="Type">account type</see> in the reports.
        /// </summary>
        /// <value>
        /// The value of this field depends solely on the <see cref="Type">type of the account</see>.
        /// </value>
		[PXString(1)]
		[PXDefault(AccountType.Asset)]
		[PXStringList(new string[]{AccountType.Asset, AccountType.Liability, AccountType.Income, AccountType.Expense},
					new string[]{"Assets Total", "Liability Total", "Income Total", "Expense Total"})]
		[PXUIField]
		public virtual string TypeTotal
		{
			get { return this.Type; }
			set { }
		}
		#endregion

		#region ReadableActive
		public abstract class readableActive : PX.Data.BQL.BqlInt.Field<readableActive> { }

        /// <summary>
        /// The user-friendly label indicating whether the account is active. Used in the reports.
        /// </summary>
        /// <value>
        /// The value of this field depends solely on the <see cref="Active"/> flag.
        /// </value>
		[PXInt]
		[PXDefault(1)]
		[PXIntList(new int[] { 1, 0 },
					new string[] { "Yes", "No" })]
		[PXUIField]
		public virtual int? ReadableActive
		{
			[PXDependsOnFields(typeof(active))]
			get
			{
				return this.Active == null || this.Active.Value == false ? 0 : 1;
			}
			set
			{
			}
		}
		#endregion
		
		#region TransactionsForGivenCurrencyExists
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
		public abstract class transactionsForGivenCurrencyExists : PX.Data.BQL.BqlBool.Field<transactionsForGivenCurrencyExists> { }
	    
		/// <summary>
	    /// Obsolete field.
	    /// </summary>
	    [PXBool]
	    [Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica7)]
	    public virtual bool? TransactionsForGivenCurrencyExists { get; set; }
		#endregion

		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		protected bool? _Included;

        /// <summary>
        /// An unbound field used in the User Interface to include the Account into a <see cref="PX.SM.RelationGroup">restriction group</see>.
        /// To learn more about the way restriction groups are managed, see the documentation for the GL Account Access (GL.10.40.00) screen
        /// (corresponds to the <see cref="GLAccess"/> graph).
        /// </summary>
		[PXUnboundDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXBool]
		[PXUIField(DisplayName = "Included")]
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
		#region TaxCategoryID
		public abstract class taxCategoryID : PX.Data.BQL.BqlString.Field<taxCategoryID> { }
        protected String _TaxCategoryID;

        /// <summary>
        /// Identifier of the <see cref="TX.TaxCategory">Tax Category</see> associated with the account.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="TX.TaxCategory.TaxCategoryID"/> field.
        /// </value>
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Tax Category", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(TX.TaxCategory.taxCategoryID), DescriptionField = typeof(TX.TaxCategory.descr))]
		[PXRestrictor(typeof(Where<TX.TaxCategory.active, Equal<True>>), TX.Messages.InactiveTaxCategory, typeof(TX.TaxCategory.taxCategoryID))]
		public virtual String TaxCategoryID
		{
			get
			{
				return this._TaxCategoryID;
			}
			set
			{
				this._TaxCategoryID = value;
			}
		}
		#endregion

		public const string Default = "0";
		public class main : PX.Data.BQL.BqlString.Constant<main>
		{
			public main()
				: base(Default)
			{
			}
		}
	}
}
