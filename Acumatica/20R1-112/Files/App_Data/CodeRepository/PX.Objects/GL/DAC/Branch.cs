using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.SM;
using PX.Objects.AP;

namespace PX.Objects.GL
{
    /// <summary>
    /// Represents a Branch of the company.
    /// Records of this type are added and edited through the Branches (CS.10.20.00) screen
    /// (corresponds to the <see cref="BranchMaint"/> graph).
    /// </summary>
	[System.SerializableAttribute()]
	[CRCacheIndependentPrimaryGraphList(
        new Type[] { typeof(BranchMaint), typeof(BranchMaint), typeof(BranchMaint) },
		new Type[] { typeof(Select<BranchMaint.BranchBAccount, Where<BranchMaint.BranchBAccount.branchBAccountID, Equal<Current<Branch.bAccountID>>>>),
                     typeof(Where<Branch.branchID, Less<Zero>>),
                     typeof(Where<True, Equal<True>>)})]
    [PXCacheName(CS.Messages.Branch)]
    public partial class Branch : IBqlTable, IIncludable, I1099Settings
	{
		#region Keys
		public class PK : PrimaryKeyOf<Branch>.By<branchID>
		{
			public static Branch Find(PXGraph graph, int? branchID) => FindBy(graph, branchID);
		}
		#endregion
        #region OrganizationID
        public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

        /// <summary>
        /// Reference to <see cref="Organization"/> record to which the Branch belongs.
        /// </summary>
        [Organization(true, typeof(Search<Organization.organizationID>), null)]
        public virtual Int32? OrganizationID { get; set; }
        #endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;

        /// <summary>
        /// Database identity.
        /// Unique identifier of the Branch.
        /// </summary>
		[PXDBIdentity()]
		public virtual Int32? BranchID
		{
			get
			{
				return this._BranchID;
			}
			set
			{
				this._BranchID = value;
			}
		}
		#endregion
		#region BranchCD
		public abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
		protected String _BranchCD;

        /// <summary>
        /// Key field.
        /// User-friendly unique identifier of the Branch.
        /// </summary>
		[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDBLiteDefault(typeof(BAccount.acctCD))]
		[PXDimensionSelector("BRANCH", typeof(Search<Branch.branchCD, Where<Match<Current<AccessInfo.userName>>>>), typeof(Branch.branchCD))]
		[PXUIField(DisplayName = "Branch ID", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String BranchCD
		{
			get
			{
				return this._BranchCD;
			}
			set
			{
				this._BranchCD = value;
			}
		}
		#endregion
		#region RoleName
		public abstract class roleName : PX.Data.BQL.BqlString.Field<roleName> { }
		private string _RoleName;

        /// <summary>
        /// The name of the <see cref="Roles">Role</see> to be used to grant users access to the data of the Branch. 
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Roles.Rolename"/> field.
        /// </value>
		[PXDBString(64, IsUnicode = true, InputMask = "")]
		[PXSelector(typeof(Search<Roles.rolename, Where<Roles.guest, Equal<False>>>), DescriptionField = typeof(Roles.descr))]
		[PXUIField(DisplayName = "Access Role")]
		public string RoleName
		{
			get
			{
				return _RoleName;
			}
			set
			{
				_RoleName = value;
			}
		}
		#endregion
		#region LogoName
		public abstract class logoName : PX.Data.BQL.BqlString.Field<logoName> { }
        
        /// <summary>
        /// The name of the logo image file.
        /// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Logo File")]
		public string LogoName { get; set; }
		#endregion
		#region LogoNameReport
		public abstract class logoNameReport : PX.Data.BQL.BqlString.Field<logoNameReport> { }

		/// <summary>
		/// The name of the report logo image file.
		/// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Report Logo File")]
		public string LogoNameReport { get; set; }
		#endregion
		#region MainLogoName
		public abstract class mainLogoName : PX.Data.BQL.BqlString.Field<mainLogoName> { }

		/// <summary>
		/// The name of the main logo image file.
		/// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Logo File")]
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public string MainLogoName { get; set; }
		#endregion

		#region OrganizationLogoNameReport
		public abstract class organizationLogoNameReport : PX.Data.BQL.BqlString.Field<organizationLogoNameReport> { }

		/// <summary>
		/// The name of the main logo image file.
		/// </summary>
		[PXDBString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Organization Logo File")]
		public string OrganizationLogoNameReport { get; set; }
		#endregion

		#region BranchOrOrganizationLogoNameReport
		public abstract class branchOrOrganizationLogoNameReport : PX.Data.BQL.BqlString.Field<branchOrOrganizationLogoNameReport> { }

		/// <summary>
		/// The name of the main logo image file.
		/// </summary>
		[PXString(IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Branch or Organization Report Logo File")]
		[PXFormula(typeof(IsNull<logoNameReport, organizationLogoNameReport>))]
		public string BranchOrOrganizationLogoNameReport { get; set; }
		#endregion

		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;

        /// <summary>
        /// Identifier of the <see cref="Ledger"/>, to which the transactions belonging to this Branch are posted by default.
        /// </summary>
        /// <value>
        /// Corresonds to the <see cref="Ledger.LedgerID"/> field.
        /// </value>
		[PXDBInt()]
		[PXUIField(DisplayName = "Posting Ledger", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<Ledger.ledgerID, Where<Ledger.balanceType, Equal<ActualLedger>>>), DescriptionField = typeof(Ledger.descr), SubstituteKey = typeof(Ledger.ledgerCD))]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region LedgerCD
		public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
        protected string _LedgerCD;

        /// <summary>
        /// User-friendly identifier of the <see cref="Ledger"/>, to which the transactions belonging to this Branch are posted by default.
        /// </summary>
        /// <value>
        /// Corresonds to the <see cref="Ledger.LedgerCD"/> field.
        /// </value>
		[PXString(10, IsUnicode = true)]
		public virtual string LedgerCD
		{
			get
			{
				return this._LedgerCD;
			}
			set
			{
				this._LedgerCD = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;

        /// <summary>
        /// Identifier of the <see cref="BAccount">Business Account</see> of the Branch.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="BAccount.BAccountID"/> field.
        /// </value>
		[PXDBInt()]
		[PXUIField(Visible = true, Enabled = false)]
		[PXSelector(typeof(CR.BAccountR.bAccountID), ValidateValue = false)]		
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
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        /// <summary>
        /// Indicates whether the Branch is active.
        /// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Active")]
		[PXDefault(true)]
		public bool? Active
		{
			get;
			set;
		}
		#endregion
		#region PhoneMask
		public abstract class phoneMask : PX.Data.BQL.BqlString.Field<phoneMask> { }
		protected String _PhoneMask;

        /// <summary>
        /// The mask used to display phone numbers for the objects, which belong to this Branch.
        /// See also the <see cref="Company.PhoneMask"/>.
        /// </summary>
		[PXDBString(50)]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Phone Mask")]
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
		#region CountryID
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		protected String _CountryID;

        /// <summary>
        /// Identifier of the default Country of the Branch.
        /// </summary>
        /// <value>
        /// Corresponds to the <see cref="Country.CountryID"/> field.
        /// </value>
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "Default Country")]
		[PXSelector(typeof(Country.countryID), DescriptionField = typeof(Country.description))]
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
		#region GroupMask
		public abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
        protected Byte[] _GroupMask;

        /// <summary>
        /// The group mask showing which <see cref="PX.SM.RelationGroup">restriction groups</see> the Branch belongs to.
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
		#region AcctMapNbr
		public abstract class acctMapNbr : PX.Data.BQL.BqlInt.Field<acctMapNbr> { }
		protected int? _AcctMapNbr;

        /// <summary>
        /// The counter of the Branch Account Mapping records associated with this Branch.
        /// This field is used to assign consistent values to the <see cref="BranchAcctMap.LineNbr"/>,
        /// <see cref="BranchAcctMapFrom.LineNbr"/> and <see cref="BranchAcctMapTo.LineNbr"/>.
        /// </summary>
		[PXDBInt()]
		[PXDefault(0)]
		public virtual int? AcctMapNbr
		{
			get
			{
				return this._AcctMapNbr;
			}
			set
			{
				this._AcctMapNbr = value;
			}
		}
		#endregion
		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
		protected String _AcctName;

        /// <summary>
        /// The name of the branch.
        /// </summary>
        /// <value>
        /// This unbound field corresponds to the <see cref="BAccount.AcctName"/> field.
        /// </value>
		[PXDBScalar(typeof(Search<BAccount.acctName, Where<BAccount.bAccountID, Equal<Branch.bAccountID>>>))]
		[PXString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region BaseCuryID
		public abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
		protected String _BaseCuryID;

        /// <summary>
        /// The base <see cref="Currency"/> of the Branch.
        /// </summary>
        /// <value>
        /// This unbound field corresponds to the <see cref="Company.BaseCuryID"/>.
        /// </value>
		[PXDBScalar(typeof(Search<Company.baseCuryID>))]
		[PXString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Base Currency ID", Enabled = false)]
		[PXSelector(typeof(Search<Currency.curyID>))]
		public virtual String BaseCuryID
		{
			get
			{
				return this._BaseCuryID;
			}
			set
			{
				this._BaseCuryID = value;
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

		#region Included
		public abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
        protected bool? _Included;

        /// <summary>
        /// An unbound field used in the User Interface to include the Branch into a <see cref="PX.SM.RelationGroup">restriction group</see>.
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

        #region TCC
        public abstract class tCC : PX.Data.BQL.BqlString.Field<tCC> { }

        /// <summary>
        /// Transmitter Control Code (TCC) for the 1099 form.
        /// </summary>
        [PXDBString(5, IsUnicode = true)]
        [PXUIField(DisplayName = "Transmitter Control Code (TCC)")]
        public virtual string TCC { get; set; }        
        #endregion

        #region ForeignEntity
        public abstract class foreignEntity : PX.Data.BQL.BqlBool.Field<foreignEntity> { }

        /// <summary>
        /// Indicates whether the Branch is considered a Foreign Entity in the context of 1099 form.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Foreign Entity")]
        public virtual bool? ForeignEntity { get; set; }
        #endregion

        #region CFSFiler
        public abstract class cFSFiler : PX.Data.BQL.BqlBool.Field<cFSFiler> { }

        /// <summary>
        /// Combined Federal/State Filer for the 1099 form.
        /// </summary>
        [PXDBBool()]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Combined Federal/State Filer")]
        public virtual bool? CFSFiler { get; set; }
        #endregion

        #region ContactName
        public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

        /// <summary>
        /// Contact Name for the 1099 form.
        /// </summary>
		[PXDBString(40, IsUnicode = true)]
        [PXUIField(DisplayName = "Contact Name")]
        public virtual string ContactName { get; set; }
        #endregion

        #region CTelNumber
        public abstract class cTelNumber : PX.Data.BQL.BqlString.Field<cTelNumber> { }

        /// <summary>
        /// Contact Phone Number for the 1099 form.
        /// </summary>
		[PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Contact Telephone Number")]
        public virtual string CTelNumber { get; set; }
        #endregion

        #region CEmail
        public abstract class cEmail : PX.Data.BQL.BqlString.Field<cEmail> { }

        /// <summary>
        /// Contact E-mail for the 1099 form.
        /// </summary>
		[PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Contact E-mail")]
        public virtual string CEmail { get; set; }
        #endregion

        #region NameControl
        public abstract class nameControl : PX.Data.BQL.BqlString.Field<nameControl> { }

        /// <summary>
        /// Name Control for the 1099 form.
        /// </summary>
        [PXDBString(4, IsUnicode = true)]
        [PXUIField(DisplayName = "Name Control")]
        public virtual string NameControl { get; set; }
        #endregion

		#region Reporting1099
		public abstract class reporting1099 : PX.Data.BQL.BqlBool.Field<reporting1099> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "1099-MISC Reporting Entity")]
		public virtual bool? Reporting1099 { get; set; }
		#endregion

		#region ParentBranchID
		public abstract class parentBranchID : PX.Data.BQL.BqlInt.Field<parentBranchID> { }

		/// <summary>
		/// The identifier of the consolidation branch.
		/// </summary>
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		[PXDBInt]
		public virtual Int32? ParentBranchID { get; set; }
		#endregion

		#region DefaultPrinterID
		public abstract class defaultPrinterID : PX.Data.BQL.BqlGuid.Field<defaultPrinterID> { }
		protected Guid? _DefaultPrinterID;
		[PXPrinterSelector(DisplayName = "Default Printer", Visibility = PXUIVisibility.SelectorVisible)]
		[PXForeignReference(typeof(Field<defaultPrinterID>.IsRelatedTo<SMPrinter.printerID>))]
		public virtual Guid? DefaultPrinterID
		{
			get
			{
				return this._DefaultPrinterID;
			}
			set
			{
				this._DefaultPrinterID = value;
			}
		}
		#endregion

		#region OverrideThemeVariables
		[PXDBBool]
		[PXDefault(false)]
		[PXUIVisible(typeof(PXThemeVariableAttribute.ThemeHasVariables))]
		[PXUIField(DisplayName = "Override Colors for the Selected Branch")]
		public bool? OverrideThemeVariables { get; set; }
		public abstract class overrideThemeVariables : IBqlField { }
		#endregion

		[PXHidden]
		public class BAccount : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			protected Int32? _BAccountID;
			[PXDBIdentity()]
			[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
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
			#region AcctCD
			public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
			protected String _AcctCD;
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.SelectorVisible)]
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
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Account Name", Visibility = PXUIVisibility.SelectorVisible)]
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
		}
	}

    [PXHidden]
    public class BranchAlias : Branch
    {
		#region OrganizationID
		public new abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        #endregion
        #region BranchCD
        public new abstract class branchCD : PX.Data.BQL.BqlString.Field<branchCD> { }
        #endregion
        #region RoleName
        public new abstract class roleName : PX.Data.BQL.BqlString.Field<roleName> { }
       #endregion
        #region LogoName
        public new abstract class logoName : PX.Data.BQL.BqlString.Field<logoName> { }
        #endregion
        #region LedgerID
        public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
        #endregion
        #region LedgerCD
        public new abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }
        #endregion
        #region BAccountID
        public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
        #endregion
        #region Active
        public new abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        #endregion
        #region PhoneMask
        public new abstract class phoneMask : PX.Data.BQL.BqlString.Field<phoneMask> { }
        #endregion
        #region CountryID
        public new abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
        #endregion
        #region GroupMask
        public new abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
        #endregion
        #region AcctMapNbr
        public new abstract class acctMapNbr : PX.Data.BQL.BqlInt.Field<acctMapNbr> { }
        #endregion
        #region AcctName
        public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
        #endregion
        #region BaseCuryID
        public new abstract class baseCuryID : PX.Data.BQL.BqlString.Field<baseCuryID> { }
        #endregion
        #region tstamp
        public new abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        #endregion
        #region Included
        public new abstract class included : PX.Data.BQL.BqlBool.Field<included> { }
		#endregion
		#region ParentBranchID
		public new abstract class parentBranchID : PX.Data.BQL.BqlInt.Field<parentBranchID> { }
		#endregion
	}
}
