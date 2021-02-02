using System;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PX.Common;
using PX.Data.RichTextEdit;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.GL;
using PX.Objects.CR;
using PX.Objects.CS.DAC;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL.Attributes;
using PX.Objects.GL.DAC;
using PX.Objects.IN;
using PX.Objects.TX;
using PX.SM;
using Branch = PX.Objects.GL.Branch;
using UploadFile = PX.SM.UploadFile;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CS
{
	[Serializable]
	public class BranchMaint : OrganizationUnitMaintBase<BranchMaint.BranchBAccount, Where<True, Equal<True>>>
	{

		#region Repository methods

		public static Branch FindBranchByCD(PXGraph graph, string branchCD, bool isReadonly = true)
		{
			if (isReadonly)
			{
				return PXSelectReadonly<Branch,
							Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>
							.Select(graph, branchCD);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		
		public static Branch FindBranchByID(PXGraph graph, int? branchID)
		{
			return FindBranchesByID(graph, branchID.SingleToArray()).SingleOrDefault();
		}

		public static IEnumerable<Branch> FindBranchesByID(PXGraph graph, int?[] branchIDs)
		{
			if (branchIDs == null || !branchIDs.Any())
				return new Branch[0];

			return PXSelect<Branch,
							Where<Branch.branchID, In<Required<Branch.branchID>>>>
							.Select(graph, branchIDs)
						.RowCast<Branch>();
		}

		public static IEnumerable<GL.Branch> GetSeparateChildBranches(PXGraph graph, Organization organization)
		{
			if (organization.OrganizationID == null
			    || organization.BAccountID == null)
				return new GL.Branch[0];

			return PXSelectReadonly<GL.Branch,
						Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>,
							And<GL.Branch.bAccountID, NotEqual<Required<GL.Branch.bAccountID>>>>>
						.Select(graph, organization.OrganizationID, organization.BAccountID)
						.RowCast<GL.Branch>();
		}

		public static IEnumerable<GL.Branch> GetChildBranches(PXGraph graph, int? organizationID)
		{
			return PXSelectReadonly<GL.Branch,
						Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>
						.Select(graph, organizationID)
						.RowCast<GL.Branch>();
		}

		public static GL.Branch GetChildBranch(PXGraph graph, int? organizationID)
		{
			return PXSelectReadonly<GL.Branch,
						Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>
						.SelectWindowed(graph, 0, 1, organizationID);
		}

		public static bool MoreThenOneBranchExist(PXGraph graph, int? organizationID)
		{
			GL.Branch[] childBranches = PXSelectReadonly<GL.Branch,
											Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>
											.SelectWindowed(graph, 0, 2, organizationID)
											.RowCast<GL.Branch>()
											.ToArray();

			return childBranches.Length > 1;
		}

		#endregion

		public static void RedirectTo(int? baccountID)
		{
			var branchMaint = CreateInstance<BranchMaint>();

			if (baccountID != null)
			{
				branchMaint.BAccount.Current = branchMaint.BAccount.Search<BranchBAccount.bAccountID>(baccountID);
			}

			throw new PXRedirectRequiredException(branchMaint, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		#region InternalTypes
		[PXProjection(typeof(Select2<BAccount,
			InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>, Where<True, Equal<True>>>), Persistent = true)]
		[PXCacheName(Messages.Branch)]
		[Serializable]
		public partial class BranchBAccount : BAccount
		{
			public new abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			public new abstract class defContactID : PX.Data.BQL.BqlInt.Field<defContactID> { }
			public new abstract class defAddressID : PX.Data.BQL.BqlInt.Field<defAddressID> { }
			public new abstract class defLocationID : PX.Data.BQL.BqlInt.Field<defLocationID> { }

			#region BranchBranchCD
			public abstract class branchBranchCD : PX.Data.BQL.BqlString.Field<branchBranchCD> { }
			[PXDBString(30, IsUnicode = true, BqlField = typeof(Branch.branchCD))]
			[PXExtraKey()]
			public virtual String BranchBranchCD
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
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			/// <summary>
			/// Reference to <see cref="Organization"/> record to which the Branch belongs.
			/// </summary>
			[PXRestrictor(typeof(Where<Organization.organizationType, NotEqual<OrganizationTypes.withoutBranches>>), GL.Messages.OnlyACompanyWithBranchesCanBeSpecified)]
			[Organization(true,
				typeof(Search<Organization.organizationID>),
				typeof(IsNull<Current<RedirectBranchParameters.organizationID>, DefaultOrganizationID>),
				BqlField = typeof(Branch.organizationID))]
			public virtual Int32? OrganizationID { get; set; }
			#endregion
			#region BranchRoleName
			public abstract class branchRoleName : PX.Data.BQL.BqlString.Field<branchRoleName> { }
			private string _BranchRoleName;
			[PXDBString(64, IsUnicode = true, InputMask = "", BqlField = typeof(Branch.roleName))]
			[PXSelector(typeof(Search<PX.SM.Roles.rolename, Where<PX.SM.Roles.guest, Equal<False>>>), DescriptionField = typeof(PX.SM.Roles.descr))]
			[PXUIField(DisplayName = "Access Role")]
			public string BranchRoleName
			{
				get
				{
					return _BranchRoleName;
				}
				set
				{
					_BranchRoleName = value;
				}
			}
			#endregion
			#region BranchLogoName
			public abstract class branchLogoName : PX.Data.BQL.BqlString.Field<branchLogoName> { }

			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(Branch.logoName))]
			[PXUIField(DisplayName = "Logo File")]
			public string BranchLogoName { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Logo File")]
			public string BranchLogoNameGetter { get { return BranchLogoName; } set {} }
			#endregion
			#region MainBranchLogoName
			public abstract class mainLogoName : PX.Data.BQL.BqlString.Field<mainLogoName> { }

			[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(Branch.mainLogoName))]
			public string BranchMainLogoName { get; set; }
			#endregion
			#region BranchLogoNameReport
			public abstract class branchLogoNameReport : PX.Data.BQL.BqlString.Field<branchLogoNameReport> { }

			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(Branch.logoNameReport))]
			[PXUIField(DisplayName = "Report Logo File")]
			public string BranchLogoNameReport { get; set; }

			[PXString]
			[PXUIField(DisplayName = "Logo File")]
			public string BranchLogoNameReportGetter { get { return BranchLogoNameReport; } set { } }
			#endregion
			#region OrganizationLogoNameReport
			public abstract class organizationLogoNameReport : PX.Data.BQL.BqlString.Field<organizationLogoNameReport> { }

			/// <summary>
			/// The name of the organization report logo image file.
			/// </summary>
			[PXDefault(typeof(Search<Organization.logoNameReport, Where<Organization.organizationID, Equal<Optional2<organizationID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXDBString(IsUnicode = true, InputMask = "", BqlField = typeof(Branch.organizationLogoNameReport))]
			[PXUIField(DisplayName = "Organization Logo File")]
			public string OrganizationLogoNameReport { get; set; }
			#endregion
			#region BranchLedgerID
			public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
			protected Int32? _LedgerID;

			[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
			[PXDBInt(BqlField = typeof(Branch.ledgerID))]
			[PXUIField(DisplayName = "Posting Ledger")]
			[PXDBDefault(typeof(Organization.actualLedgerID), PersistingCheck = PXPersistingCheck.Nothing)]
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
			#region BranchBAccountID
			public abstract class branchBAccountID : PX.Data.BQL.BqlInt.Field<branchBAccountID> { }
			[PXDBInt(BqlField = typeof(Branch.bAccountID))]
			[PXUIField(Visible = true, Enabled = false)]
			[PXExtraKey()]
			public virtual Int32? BranchBAccountID
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
			#region BranchActive
			public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
			[PXDBBool(BqlField = typeof(Branch.active))]
			[PXUIField(DisplayName = "Active", FieldClass="BRANCH")]
			[PXDefault(true)]
			public bool? Active
			{
				get;
				set;
			}
			#endregion
			#region AcctCD
			public new abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
			[PXDimensionSelector("BRANCH", typeof(Search2<BAccount.acctCD, InnerJoin<Branch, On<Branch.bAccountID, Equal<BAccount.bAccountID>>>>), typeof(BAccount.acctCD),
				typeof(BAccount.acctCD), typeof(BAccount.acctName))]
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXDefault()]
			[PXUIField(DisplayName = "Branch ID", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AcctCD
			{
				get
				{
					return base._AcctCD;
				}
				set
				{
					base._AcctCD = value;
				}
			}
			#endregion
			#region AcctName
			public new abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
			
			[PXDBString(60, IsUnicode = true)]
			[PXDefault()]
			[PXUIField(DisplayName = "Branch Name", Visibility = PXUIVisibility.SelectorVisible)]
			public override String AcctName
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
			public new abstract class type : PX.Data.BQL.BqlString.Field<type> { }
			[PXDBString(2, IsFixed = true, BqlField=typeof(BAccount.type))]
			public override String Type
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
			#region AcctReferenceNbr
			public new abstract class acctReferenceNbr : PX.Data.BQL.BqlString.Field<acctReferenceNbr> { }
			[PXDBString(50, IsUnicode = true, BqlField = typeof(BAccount.acctReferenceNbr))]
			public override String AcctReferenceNbr
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
			public new abstract class parentBAccountID : PX.Data.BQL.BqlInt.Field<parentBAccountID> { }
			[PXDBInt(BqlField = typeof(BAccount.parentBAccountID))]
			public override Int32? ParentBAccountID
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
			#region OwnerID
			public new abstract class ownerID : PX.Data.BQL.BqlGuid.Field<ownerID> { }
			[PXDBGuid(BqlField = typeof(BAccount.ownerID))]
			public override Guid? OwnerID
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
			#region BranchPhoneMask
			public abstract class branchPhoneMask : PX.Data.BQL.BqlString.Field<branchPhoneMask> { }
			protected String _BranchPhoneMask;

			[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
			[PXDBString(50, BqlField = typeof(Branch.phoneMask))]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Phone Mask")]
			public virtual String BranchPhoneMask
			{
				get
				{
					return this._BranchPhoneMask;
				}
				set
				{
					this._BranchPhoneMask = value;
				}
			}
			#endregion
			#region BranchCountryID
			public abstract class branchCountryID : PX.Data.BQL.BqlString.Field<branchCountryID> { }
			protected String _BranchCountryID;
			[PXDBString(2, IsFixed = true, BqlField = typeof(Branch.countryID))]
			[PXUIField(DisplayName = "Default Country")]
			[PXSelector(typeof(Country.countryID), DescriptionField = typeof(Country.description))]
			public virtual String BranchCountryID
			{
				get
				{
					return this._BranchCountryID;
				}
				set
				{
					this._BranchCountryID = value;
				}
			}
			#endregion


			#region TCC
			public abstract class tCC : PX.Data.BQL.BqlString.Field<tCC> { }

			/// <summary>
			/// Transmitter Control Code (TCC) for the 1099 form.
			/// </summary>
			[PXDBString(5, IsUnicode = true, BqlTable = typeof(Branch))]
			[PXUIField(DisplayName = "Transmitter Control Code (TCC)")]
			public virtual string TCC { get; set; }
			#endregion

			#region ForeignEntity
			public abstract class foreignEntity : PX.Data.BQL.BqlBool.Field<foreignEntity> { }

			/// <summary>
			/// Indicates whether the Branch is considered a Foreign Entity in the context of 1099 form.
			/// </summary>
			[PXDBBool(BqlTable = typeof(Branch))]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Foreign Entity")]
			public virtual bool? ForeignEntity { get; set; }
			#endregion

			#region CFSFiler
			public abstract class cFSFiler : PX.Data.BQL.BqlBool.Field<cFSFiler> { }

			/// <summary>
			/// Combined Federal/State Filer for the 1099 form.
			/// </summary>
			[PXDBBool(BqlTable = typeof(Branch))]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "Combined Federal/State Filer")]
			public virtual bool? CFSFiler { get; set; }
			#endregion

			#region ContactName
			public abstract class contactName : PX.Data.BQL.BqlString.Field<contactName> { }

			/// <summary>
			/// Contact Name for the 1099 form.
			/// </summary>
			[PXDBString(40, IsUnicode = true, BqlTable = typeof(Branch))]
			[PXUIField(DisplayName = "Contact Name")]
			public virtual string ContactName { get; set; }
			#endregion

			#region CTelNumber
			public abstract class cTelNumber : PX.Data.BQL.BqlString.Field<cTelNumber> { }

			/// <summary>
			/// Contact Phone Number for the 1099 form.
			/// </summary>
			[PXDBString(15, IsUnicode = true, BqlTable = typeof(Branch))]
			[PXUIField(DisplayName = "Contact Telephone Number")]
			public virtual string CTelNumber { get; set; }
			#endregion

			#region CEmail
			public abstract class cEmail : PX.Data.BQL.BqlString.Field<cEmail> { }

			/// <summary>
			/// Contact E-mail for the 1099 form.
			/// </summary>
			[PXDBString(50, IsUnicode = true, BqlTable = typeof(Branch))]
			[PXUIField(DisplayName = "Contact E-mail")]
			public virtual string CEmail { get; set; }
			#endregion

			#region NameControl
			public abstract class nameControl : PX.Data.BQL.BqlString.Field<nameControl> { }

			/// <summary>
			/// Name Control for the 1099 form.
			/// </summary>
			[PXDBString(4, IsUnicode = true, BqlTable = typeof(Branch))]
			[PXUIField(DisplayName = "Name Control")]
			public virtual string NameControl { get; set; }
			#endregion

			#region Reporting1099
			public abstract class reporting1099 : PX.Data.BQL.BqlBool.Field<reporting1099> { }
			[PXDBBool(BqlTable = typeof(Branch))]
			[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = "1099-MISC Reporting Entity")]
			[PXUIVisible(typeof(Where<Selector<organizationID, Organization.reporting1099ByBranches>, Equal<True>>))]
			public virtual bool? Reporting1099 { get; set; }
			#endregion

			#region GroupMask
			public new abstract class groupMask : PX.Data.BQL.BqlByteArray.Field<groupMask> { }
			protected new Byte[] _GroupMask;
			[SingleGroup(BqlTable = typeof(Branch))]
			public virtual new Byte[] GroupMask
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
			[PXDBInt(BqlTable = typeof(Branch))]
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

			#region DefaultPrinterID
			public abstract class defaultPrinterID : PX.Data.BQL.BqlGuid.Field<defaultPrinterID> { }
			protected Guid? _DefaultPrinterID;
			[PXPrinterSelector(DisplayName = "Default Printer", Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(Branch.defaultPrinterID))]
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
			[PXDBBool(BqlField = typeof(Branch.overrideThemeVariables))]
			[PXDefault(false)]
			[PXUIVisible(typeof(PXThemeVariableAttribute.ThemeHasVariables))]
			[PXUIField(DisplayName = "Override Colors for the Selected Branch")]
			public bool? OverrideThemeVariables { get; set; }
			public abstract class overrideThemeVariables : IBqlField { }
			#endregion

			#region PrimaryColor
			public abstract class primaryColor : PX.Data.BQL.BqlString.Field<primaryColor> { }

			[PXString(30, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Primary Color")]
			[PXThemeVariable("--primary-color", PersistDefaultValue = typeof(overrideThemeVariables))]
			[PXUIEnabled(typeof(overrideThemeVariables))]
			public string PrimaryColor { get; set; }
			#endregion

			#region BackgroundColor
			public abstract class backgroundColor : PX.Data.BQL.BqlString.Field<backgroundColor> { }

			[PXString(30, IsUnicode = true, InputMask = "")]
			[PXUIField(DisplayName = "Background Color")]
			[PXThemeVariable("--background-color", PersistDefaultValue = typeof(overrideThemeVariables))]
			[PXUIEnabled(typeof(overrideThemeVariables))]
			public string BackgroundColor { get; set; }
			#endregion
		}

		#endregion

		#region CTor + Public members

		public PXSelect<BranchBAccount, Where<BranchBAccount.bAccountID, Equal<Current<BranchBAccount.bAccountID>>>> CurrentBAccount;

		public PXSelectJoin<EPEmployee, InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>, And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
							LeftJoin<Address, On<Address.addressID, Equal<EPEmployee.defAddressID>,
							And<Address.bAccountID, Equal<EPEmployee.parentBAccountID>>>>>,
							Where<EPEmployee.parentBAccountID, Equal<Current<BranchBAccount.bAccountID>>>> Employees;

		public override PXSelectBase<EPEmployee> EmployeesAccessor => Employees;

		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<BranchBAccount.noteID>>>> Notedocs;

		public PXSelect<Organization,
						Where<Organization.organizationID, Equal<Current2<BranchBAccount.organizationID>>>>
						CurrentOrganizationView;

		public PXSelectReadonly2<Ledger,
									InnerJoin<OrganizationLedgerLink,
										On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>>,
									Where<OrganizationLedgerLink.organizationID, Equal<Current2<BranchBAccount.organizationID>>>>
									LedgersView;

		#region Buttons

		public new PXAction<BranchBAccount> viewContact;
		public new PXAction<BranchBAccount> newContact;
		public new PXAction<BranchBAccount> viewLocation;
		public new PXAction<BranchBAccount> newLocation;
		public new PXAction<BranchBAccount> setDefault;
		

		public new PXAction<BranchBAccount> viewMainOnMap;
		public new PXAction<BranchBAccount> viewDefLocationOnMap;
		public new PXAction<BranchBAccount> validateAddresses;
		public PXAction<BranchBAccount> ViewLedger;
		public PXChangeID<BranchBAccount, BranchBAccount.acctCD> ChangeID;
		
		#endregion

		public BranchMaint()
			{
			BAccount.Cache.AllowInsert = PXAccess.FeatureInstalled<FeaturesSet.branch>();

			ActionsMenu.AddMenuAction(validateAddresses);
			ActionsMenu.AddMenuAction(ChangeID);

			// Creates BranchParameters cache if not exists
			this.Caches<RedirectBranchParameters>();
				}

		protected virtual BranchValidator GetBranchValidator()
				{
			return new BranchValidator(this);
				}

		#endregion

		#region Events Handlers

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
			Where<LocationExtAddress.bAccountID,
			Equal<Current<BranchBAccount.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void BranchBAccount_DefLocationID_CacheAttached(PXCache sender) { }

		protected virtual void BranchBAccount_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null) return;

			bool isOrganizationFieldEnabled = true;
			bool isOrganizationFieldVisible = true;
			bool canBeBranchDeleted = true;

			CurrentOrganizationView.Current = CurrentOrganizationView.Select();

			Organization curOrganization = CurrentOrganizationView.Current;

			bool canEditBranch = true;

			if (curOrganization?.OrganizationType == OrganizationTypes.WithoutBranches)
			{
				isOrganizationFieldVisible = false;
				canBeBranchDeleted = false;
				canEditBranch = false;
		}
			else
			{
				Branch branch = FindBranchByCD(this, branchBAccount.BranchBranchCD);

				if (branch != null)
		{
					isOrganizationFieldEnabled = GLUtility.GetRelatedToBranchGLHistory(this, branch.BranchID.SingleToArray()) == null;
			}
		}

			bool isPersistedBranch = !this.IsPrimaryObjectInserted();

			BAccount.AllowUpdate = canEditBranch;
			CurrentBAccount.AllowUpdate = canEditBranch;
			DefAddress.AllowUpdate = canEditBranch;
			DefContact.AllowUpdate = canEditBranch;
			DefLocation.AllowUpdate = canEditBranch;
			DefLocationContact.AllowUpdate = canEditBranch;
			IntLocations.AllowUpdate = canEditBranch;
			CurrentOrganizationView.AllowUpdate = canEditBranch;
			Employees.AllowUpdate = canEditBranch;
			newContact.SetEnabled(canEditBranch && isPersistedBranch);
			viewContact.SetEnabled(canEditBranch);
			validateAddresses.SetEnabled(canEditBranch && isPersistedBranch);

			PXUIFieldAttribute.SetEnabled<BranchBAccount.organizationID>(sender, null, isOrganizationFieldEnabled);
			PXUIFieldAttribute.SetEnabled<BranchBAccount.branchRoleName>(sender, null, curOrganization != null && curOrganization.RoleName == null);

			BAccount.AllowDelete = canBeBranchDeleted;
			Delete.SetEnabled(canBeBranchDeleted);
			ChangeID.SetEnabled(!(sender.GetStatus(branchBAccount) == PXEntryStatus.Inserted && branchBAccount.AcctCD == null)
			                    && curOrganization?.OrganizationType != OrganizationTypes.WithoutBranches);

			PXUIFieldAttribute.SetVisible<BranchBAccount.organizationID>(sender, null, isOrganizationFieldVisible);
		}

		protected virtual void BranchBAccount_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null)
				return;

			int? oldOrganizationID = (int?)sender.GetValueOriginal<BranchBAccount.organizationID>(branchBAccount);

			if (e.Operation == PXDBOperation.Update
			    && oldOrganizationID != branchBAccount.OrganizationID)
		{
				Organization oldOrganization = OrganizationMaint.FindOrganizationByID(this, oldOrganizationID, isReadonly: false);
				CurrentOrganizationView.Cache.SetStatus(oldOrganization, PXEntryStatus.Updated);

				Branch branch = FindBranchByCD(this, branchBAccount.BranchBranchCD);

				if (GLUtility.RelatedForBranchReleasedTransactionExists(this, branch.BranchID))
		{
					throw new PXException(GL.Messages.TheOfTheBranchCannotBeChangedBecauseAtLeastOneReleasedGLTranExistsForTheBranch,
											PXUIFieldAttribute.GetDisplayName<BranchBAccount.organizationID>(sender),
											branchBAccount.BranchBranchCD);
		}
		}

			if (e.Operation == PXDBOperation.Insert
				|| e.Operation == PXDBOperation.Update && oldOrganizationID != branchBAccount.OrganizationID)
		{
				Organization newOrganization = OrganizationMaint.FindOrganizationByID(this, branchBAccount.OrganizationID, isReadonly: false);
				CurrentOrganizationView.Cache.SetStatus(newOrganization, PXEntryStatus.Updated);
		}
		}

		protected virtual void BranchBAccount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			this.OnBAccountRowInserted(sender, e);
		}

		protected virtual void BranchBAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null)
				return;

			if (!UnattendedMode)
		{
				Branch branch = FindBranchByCD(this, branchBAccount.BranchBranchCD);

				if (branch != null)
		{
					GetBranchValidator().CanBeBranchesDeletedSeparately(branch.SingleToArray());
		}
		}

			CurrentOrganizationView.Cache.SetStatus(CurrentOrganizationView.Current, PXEntryStatus.Updated);

			// TODO: Workaround awaiting AC-109645. Replace it to PXReferenceIntegrityCheckAttribute .
			CR.BAccount2 bAccount = PXSelectReadonly2<
				CR.BAccount2,
				InnerJoin<Location, 
					On<CR.BAccount2.bAccountID, Equal<Location.bAccountID>>,
				InnerJoin<Branch,
					On<Location.cBranchID, Equal<Branch.branchID>>>>,
				Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>
				.SelectSingleBound(this, null, branchBAccount.BranchBranchCD);

			if (bAccount != null)
			{
				throw new PXSetPropertyException(Messages.CustomerAccountsForBranch, bAccount.AcctCD.Trim());
			}

			bAccount = PXSelectReadonly2<
				CR.BAccount2,
				InnerJoin<Location,
					On<CR.BAccount2.bAccountID, Equal<Location.bAccountID>>,
				InnerJoin<Branch,
					On<Location.vBranchID, Equal<Branch.branchID>>>>,
				Where<Branch.branchCD, Equal<Required<Branch.branchCD>>>>
				.SelectSingleBound(this, null, branchBAccount.BranchBranchCD);

			if (bAccount != null)
			{
				throw new PXSetPropertyException(Messages.VendorAccountsForBranch, bAccount.AcctCD.Trim());
			}
		}

		protected virtual void BranchBAccount_Active_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			BranchBAccount item = e.Row as BranchBAccount;

			if (item == null)
				return;

			if (!UnattendedMode)
		{
				Branch branch = FindBranchByCD(this, item.BranchBranchCD);

				if (branch != null)
		{
					GetBranchValidator().ValidateActiveField(branch.BranchID.SingleToArray(), (bool?)e.NewValue, CurrentOrganizationView.Current);
		}
		}
		}

		protected virtual void BranchBAccount_AcctName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.OnBAccountAcctNameFieldUpdated(sender, e);
		}

		protected virtual void BranchBAccount_OrganizationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null)
				return;

			BAccount.Cache.SetDefaultExt<BranchBAccount.branchRoleName>(branchBAccount);
			BAccount.Cache.SetDefaultExt<BranchBAccount.branchCountryID>(branchBAccount);
			BAccount.Cache.SetDefaultExt<BranchBAccount.organizationLogoNameReport>(branchBAccount);
		}

		protected virtual void BranchBAccount_BranchRoleName_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null)
				return;

			Organization organization = OrganizationMaint.FindOrganizationByID(this, branchBAccount.OrganizationID);

			if (organization == null)
		{
				e.NewValue = null;
		}
			else if (organization.RoleName != null)
				{
				e.NewValue = organization.RoleName;
				}
			else
				{
				e.NewValue = branchBAccount.BranchRoleName;
			}
		}

		protected virtual void BranchBAccount_BranchCountryID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var branchBAccount = e.Row as BranchBAccount;

			if (branchBAccount == null)
				return;
			
			Organization organization = OrganizationMaint.FindOrganizationByID(this, branchBAccount.OrganizationID);
			
			if (organization?.CountryID != null && branchBAccount.BranchCountryID == null)
		{
				e.NewValue = organization.CountryID;
			}
			else
			{
				e.NewValue = branchBAccount.BranchCountryID;
			}
		}

        protected virtual void BranchBAccount_AcctMapNbr_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            var branchBAccount = e.Row as BranchBAccount;
            if (branchBAccount?.BranchBranchCD ==null)
                return;

            using (PXReadDeletedScope rds = new PXReadDeletedScope())
            {
                Branch branch = FindBranchByCD(this, branchBAccount.BranchBranchCD);
                if (branch != null)
                {
                    e.NewValue = PXSelectGroupBy<BranchAcctMap,
                            Where<BranchAcctMap.branchID, Equal<Required<BranchAcctMap.branchID>>>,
                            Aggregate<Max<BranchAcctMap.lineNbr>>>
                            .Select(this, new object[] { branch.BranchID })
                            .RowCast<BranchAcctMap>().FirstOrDefault()
                            ?.LineNbr;
                }
            }
        }

		#endregion

		#region Button Handlers
		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewLedger(PXAdapter adapter)
		{
			Ledger ledger = LedgersView.Current;

			if (ledger != null)
				{
				GeneralLedgerMaint.RedirectTo(ledger.LedgerID);
				}

			return adapter.Get();
				}

		#endregion

		public override void Clear()
		{
			RedirectBranchParameters parameters = this.Caches<RedirectBranchParameters>().Current as RedirectBranchParameters;
			base.Clear();
			if (parameters != null)
				{
				this.Caches<RedirectBranchParameters>().Insert(parameters);
			}
		}

		public override void Persist()
		{
			bool requestRelogin = Accessinfo.BranchID == null;
			bool resetPageCache = false;

			if (!IsImport && !IsExport && !IsContractBasedAPI && !IsMobile)
			{
				resetPageCache = BAccount.Cache.Inserted.Any_() || BAccount.Cache.Deleted.Any_();
				if (!resetPageCache)
				{
					foreach (object updated in BAccount.Cache.Updated) {
						if (BAccount.Cache.IsValueUpdated<string, BranchBAccount.acctName>(updated, StringComparer.CurrentCulture)) {
							resetPageCache = true;
						}

						if (BAccount.Cache.IsValueUpdated<bool?, BranchBAccount.overrideThemeVariables>(updated)
							|| BAccount.Cache.IsValueUpdated<string, BranchBAccount.backgroundColor>(updated, StringComparer.OrdinalIgnoreCase)
							|| BAccount.Cache.IsValueUpdated<string, BranchBAccount.primaryColor>(updated, StringComparer.OrdinalIgnoreCase)) {
							resetPageCache = true;
						}
						else if (BAccount.Cache.IsValueUpdated<string, BranchBAccount.branchRoleName>(updated, StringComparer.OrdinalIgnoreCase)) {
							resetPageCache = true;
							requestRelogin = true;
						}
					}
				}
			}

			BranchBAccount branchBAccount = BAccount.Current;

			PXEntryStatus branchBAccountStatus = BAccount.Cache.GetStatus(branchBAccount);

			if (branchBAccount?.OrganizationID != null)
			{
				if (branchBAccountStatus == PXEntryStatus.Updated
					|| branchBAccountStatus == PXEntryStatus.Inserted)
				{
					Branch branch = FindBranchByCD(this, branchBAccount.BranchBranchCD);

					ProcessActiveChange(branchBAccount, branch?.BranchID);

					string oldRoleName = (string)BAccount.Cache.GetValueOriginal<BranchBAccount.branchRoleName>(branchBAccount);

					if (oldRoleName != branchBAccount.BranchRoleName)
					{
						CurrentOrganizationView.Cache.SetStatus(CurrentOrganizationView.Current, PXEntryStatus.Updated);
					}
					}
				}

			foreach (BranchBAccount deletedBranchBAccount in BAccount.Cache.Deleted)
				{
				Branch branch = FindBranchByCD(this, deletedBranchBAccount.BranchBranchCD);

				GetBranchValidator().CanBeBranchesDeletedSeparately(branch.SingleToArray());

				requestRelogin = Accessinfo.BranchID == branch.BranchID;
			}

			base.Persist();
				
			using (PXTransactionScope tran = new PXTransactionScope())
			{
				this.Caches[typeof(Branch)].Clear();
				bool clearRoleNames = true;
				foreach (Branch b in PXSelect<Branch>.Select(this))
				{
					if (b.RoleName != null) clearRoleNames = false;
				}
				using (PXReadDeletedScope rds = new PXReadDeletedScope())
				{
					if (clearRoleNames)
					{
						PXDatabase.Update<Branch>(new PXDataFieldAssign<Branch.roleName>(null));
					}
				}
				tran.Complete();
			}

			var currentBranchId = PXContext.GetBranchID();
			PXAccess.ResetBranchSlot();
			if (!PXAccess.GetBranches().Where(b => b.Deleted == false && b.Id == currentBranchId).Any())
			{
				PXLogin.SetBranchID(CreateInstance<PX.SM.SMAccessPersonalMaint>().GetDefaultBranchId());
			}

			if (resetPageCache) // to refresh branch combo
			{
				PXPageCacheUtils.InvalidateCachedPages();
				PXDatabase.SelectTimeStamp(); //clear db slots (for PXAccess.AvailableBranches)
				if (!this.UnattendedMode) {
					bool isPopup = PX.Web.UI.PXBaseDataSource.RedirectHelper.IsPopupPage(System.Web.HttpContext.Current?.Handler as System.Web.UI.Page);
					if(!isPopup) throw new PXRedirectRequiredException(this, "Branch", true);
				}
			}

			if (resetPageCache && requestRelogin)
			{
				// otherwise page will be refreshed on the next request by PXPage.NeedRefresh
				if (System.Web.HttpContext.Current == null)
				{
					throw new PXRedirectRequiredException(this, "Branch", true);
				}
			}
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			int res = base.Persist(cacheType, operation);
			if (cacheType == typeof(BranchBAccount) && operation == PXDBOperation.Update)
			{
				foreach (PXResult<NoteDoc, UploadFile> rec in PXSelectJoin<NoteDoc, InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
														Where<NoteDoc.noteID, Equal<Current<BranchBAccount.noteID>>>>.Select(this))
				{
					UploadFile file = (UploadFile)rec;
					if (file.IsPublic != true)
					{
						this.SelectTimeStamp();
						file.IsPublic = true;
						file = (UploadFile)this.Caches[typeof(UploadFile)].Update(file);
						this.Caches[typeof(UploadFile)].PersistUpdated(file);
					}
				}
			}
			return res;
		}

		protected virtual void BranchBAccount_Type_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = BAccountType.BranchType;
		}

		protected virtual void BranchBAccount_BranchBranchCD_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			//PXDBChildIdentity() hack
			if (e.Table != null && e.Operation == PXDBOperation.Update)
			{
				e.IsRestriction = false;
				e.Cancel = true;
			}
		}

		#region Service methods

		protected virtual void ProcessActiveChange(BranchBAccount branchBAccount, int? branchID)
				{
			bool? origActiveValue = (bool?)BAccount.Cache.GetValueOriginal<BranchBAccount.active>(branchBAccount);

			if (origActiveValue != branchBAccount.Active)
		{
				GetBranchValidator().ValidateActiveField(branchID.SingleToArray(),
															branchBAccount.Active,
															CurrentOrganizationView.Current);
			
				if (branchBAccount.Active == true)
			{
					CurrentOrganizationView.Cache.SetStatus(CurrentOrganizationView.Current, PXEntryStatus.Updated);
			}
			}
		}

		#endregion

		protected override int? BaccountIDForNewEmployee()
		{
			return BAccount.Current?.BAccountID;
		}
	}

	[PXProjection(typeof(Select5<Organization,
		InnerJoin<Branch,
			On<Organization.organizationID, Equal<Branch.organizationID>>,
		InnerJoin<BAccountR,
			On<BAccountR.bAccountID, Equal<Organization.bAccountID>>,
		InnerJoin<Address,
			On<Address.bAccountID, Equal<BAccountR.bAccountID>,
				And<Address.addressID, Equal<BAccountR.defAddressID>>>,
		InnerJoin<Contact,
			On<Contact.bAccountID, Equal<BAccountR.bAccountID>,
				And<Contact.contactID, Equal<BAccountR.defContactID>>>>>>>,
		Where<Organization.active, Equal<True>,
			And<MatchWithBranch<Branch.branchID, Current<AccessInfo.branchID>>>>,
		Aggregate<GroupBy<Organization.organizationID>>>))]
	[Serializable]
	[PXCacheName(Messages.CompanyBAccount)]
	public partial class CompanyBAccount : IBqlTable
{
	#region AcctName
	public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

	[PXDBString(60, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
	[PXUIField(DisplayName = "Company")]
	[PXDefault()]
	public virtual String AcctName
	{
		get;
		set;
	}
	#endregion

		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

	[PXDefault()]
		[Organization(BqlField = typeof(Organization.organizationID))]
		public virtual int? OrganizationID
	{
		get;
		set;
	}
	#endregion

	#region BAccountID
	public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
	[PXDefault()]
	[PXDBInt(BqlField = typeof(BAccountR.bAccountID))]
	public virtual Int32? BAccountID
	{
		get;
		set;
	}
	#endregion
	#region LogoName
	public abstract class logoName : PX.Data.BQL.BqlString.Field<logoName> { }

		[PXDBString(IsUnicode = true, BqlField = typeof(Organization.logoName))]
	[PXDefault()]
	[PXUIField(DisplayName = "Logo")]
	public virtual String LogoName
	{
		get;
		set;
	}
	#endregion
	#region AddressLine1
	public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
	protected String _AddressLine1;
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine1))]
	[PXUIField(DisplayName = "Address Line 1")]
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
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine2))]
	[PXUIField(DisplayName = "Address Line 2")]
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
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine3))]
	[PXUIField(DisplayName = "Address Line 3")]
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
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.city))]
	[PXUIField(DisplayName = "City")]
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
	[PXDBString(2, IsFixed = true, BqlField = typeof(Address.countryID))]
	[PXUIField(DisplayName = "Country")]
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
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.state))]
	[PXUIField(DisplayName = "State")]
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
	[PXDBString(20, BqlField = typeof(Address.postalCode))]
	[PXUIField(DisplayName = "Postal Code")]
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
	#region Title
	public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
	protected String _Title;
	[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
	[Titles]
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
	[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
	[PXUIField(DisplayName = "Position")]
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
	#region FullName
	public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
	protected String _FullName;
	[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.fullName))]
	[PXUIField(DisplayName = "Business Name")]
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
			return this._EMail == null ? null : _EMail.Trim();
		}
		set
		{
			this._EMail = value;
		}
	}
	#endregion
	#region Phone1
	public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
	protected String _Phone1;
	[PhoneValidation]
	[PXDBString(50, BqlField = typeof(Contact.phone1))]
	[PXUIField(DisplayName = "Phone 1")]
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
}

	[PXProjection(typeof(SelectFrom<Organization>
		.LeftJoin<Branch>
			.On<Organization.organizationID.IsEqual<Branch.organizationID>
				.And<Organization.reporting1099ByBranches.IsEqual<True>>>
		.Where<Organization.active.IsEqual<True>
			.And<MatchWithBranch<Branch.branchID>>>))]
	[Serializable]
	[PXHidden]
	public class Entity1099 : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(BqlTable = typeof(Organization))]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(BqlTable = typeof(Branch))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion

		#region OrganizationBAccountID
		public abstract class organizationBAccountID : PX.Data.BQL.BqlInt.Field<organizationBAccountID> { }

		[PXDBInt(BqlField = typeof(Organization.bAccountID))]
		public virtual int? OrganizationBAccountID { get; set; }
		#endregion

		#region BranchBAccountID
		public abstract class branchBAccountID : PX.Data.BQL.BqlInt.Field<branchBAccountID> { }

		[PXDBInt(BqlField = typeof(Branch.bAccountID))]
		public virtual int? BranchBAccountID { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

		[PXInt]
		[PXDBCalced(typeof(IIf<Where<Organization.reporting1099ByBranches.IsEqual<True>>, Branch.bAccountID, Organization.bAccountID>), typeof(int?))]
		public virtual int? BAccountID { get; set; }
		#endregion
	}

	[PXProjection(typeof(SelectFrom<Entity1099>
		.InnerJoin<BAccountR>
			.On<BAccountR.bAccountID.IsEqual<Entity1099.bAccountID>>
		.InnerJoin<Address>
			.On<Address.bAccountID.IsEqual<BAccountR.bAccountID>
				.And<Address.addressID.IsEqual<BAccountR.defAddressID>>> 
		.InnerJoin<Contact>
			.On<Contact.bAccountID.IsEqual<BAccountR.bAccountID>
				.And<Contact.contactID.IsEqual<BAccountR.defContactID>>>))]
	[Serializable]
	[PXCacheName(AP.Messages.CompanyBAccount1099)]
	public partial class CompanyBranchBAccount1099 : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[Organization(BqlField = typeof(Entity1099.organizationID))]
		public virtual int? OrganizationID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		[Branch(BqlField = typeof(Entity1099.branchID))]
		public virtual int? BranchID
		{
			get;
			set;
		}
		#endregion

		#region AcctName
		public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

		[PXDBString(60, IsUnicode = true, BqlField = typeof(BAccountR.acctName))]
		[PXUIField(DisplayName = "Legal Name")]
		[PXDefault]
		public virtual string AcctName
		{
			get;
			set;
		}
		#endregion

		#region LegalName
		public abstract class legalName : PX.Data.BQL.BqlString.Field<legalName> { }

		[PXDBString(60, IsUnicode = true, BqlField = typeof(BAccountR.legalName))]
		[PXUIField(DisplayName = "Legal Name")]
		[PXDefault]
		public virtual string LegalName
		{
			get;
			set;
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDefault()]
		[PXDBInt(BqlField = typeof(BAccountR.bAccountID))]
		public virtual Int32? BAccountID
		{
			get;
			set;
		}
		#endregion
		#region AddressLine1
		public abstract class addressLine1 : PX.Data.BQL.BqlString.Field<addressLine1> { }
		protected String _AddressLine1;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine1))]
		[PXUIField(DisplayName = "Address Line 1")]
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
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine2))]
		[PXUIField(DisplayName = "Address Line 2")]
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
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.addressLine3))]
		[PXUIField(DisplayName = "Address Line 3")]
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
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.city))]
		[PXUIField(DisplayName = "City")]
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
		[PXDBString(2, IsFixed = true, BqlField = typeof(Address.countryID))]
		[PXUIField(DisplayName = "Country")]
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
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Address.state))]
		[PXUIField(DisplayName = "State")]
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
		[PXDBString(20, BqlField = typeof(Address.postalCode))]
		[PXUIField(DisplayName = "Postal Code")]
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
		#region Title
		public abstract class title : PX.Data.BQL.BqlString.Field<title> { }
		protected String _Title;
		[PXDBString(50, IsUnicode = true, BqlField = typeof(Contact.title))]
		[Titles]
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
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.salutation))]
		[PXUIField(DisplayName = "Position")]
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
		#region FullName
		public abstract class fullName : PX.Data.BQL.BqlString.Field<fullName> { }
		protected String _FullName;
		[PXDBString(255, IsUnicode = true, BqlField = typeof(Contact.fullName))]
		[PXUIField(DisplayName = "Business Name")]
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
				return this._EMail == null ? null : _EMail.Trim();
			}
			set
			{
				this._EMail = value;
			}
		}
		#endregion
		#region Phone1
		public abstract class phone1 : PX.Data.BQL.BqlString.Field<phone1> { }
		protected String _Phone1;
		[PhoneValidation]
		[PXDBString(50, BqlField = typeof(Contact.phone1))]
		[PXUIField(DisplayName = "Phone 1")]
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
}

	[Serializable]
	[PXHidden]
	public class RedirectBranchParameters : IBqlTable
	{
		#region OrganizationID
		public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

		[PXInt]
		public virtual Int32? OrganizationID
		{
			get;
			set;
		}
		#endregion
	}
}
