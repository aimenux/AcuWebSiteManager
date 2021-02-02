using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.Common.EntityInUse;
using PX.Objects.Common.Extensions;
using PX.Objects.CR;
using PX.Objects.CS.DAC;
using PX.Objects.EP;
using PX.Objects.FA;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.IN;
using PX.SM;

using Branch = PX.Objects.GL.Branch;

namespace PX.Objects.CS
{
	public class OrganizationMaint : OrganizationUnitMaintBase<OrganizationBAccount, 
		Where<OrganizationBAccount.type, Equal<BAccountType.organizationType>, 
				Or<OrganizationBAccount.type, Equal<BAccountType.organizationBranchCombinedType>>>>
	{
		#region Repository methods

		public static Organization FinOrganizationByBAccountID(PXGraph graph, int? bAccountID)
		{
			return PXSelectReadonly<Organization,
									Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>
									.Select(graph, bAccountID);
		}

		public static Organization FindOrganizationByID(PXGraph graph, int? organizationID, bool isReadonly = true)
		{
			return FindOrganizationByIDs(graph, 
											organizationID != null ? organizationID.SingleToArray() : null, 
											isReadonly)
										.SingleOrDefault();
		}

		public static IEnumerable<Organization> FindOrganizationByIDs(PXGraph graph, int?[] organizationIDs, bool isReadonly = true)
		{
			if (organizationIDs == null || !organizationIDs.Any())
				return new Organization[0];

			if (isReadonly)
			{
				return PXSelectReadonly<Organization,
										Where<Organization.organizationID, In<Required<Organization.organizationID>>>>
										.Select(graph, organizationIDs)
										.RowCast<Organization>(); 
			}
			else
			{
				return PXSelect<Organization,
								Where<Organization.organizationID, In<Required<Organization.organizationID>>>>
								.Select(graph, organizationIDs)
								.RowCast<Organization>();
			}
		}

		public static Organization FindOrganizationByCD(PXGraph graph, string organizationCD, bool isReadonly = true)
		{
			if (isReadonly)
			{
				return PXSelectReadonly<Organization,
						Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>
					.Select(graph, organizationCD); 
			}
			else
			{
				return PXSelect<Organization,
						Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>
					.Select(graph, organizationCD);
			}
		}

		public static Contact GetDefaultContact(PXGraph graph, int? organizationID)
		{
			foreach (PXResult<Organization, BAccountR, Contact> res in
				PXSelectJoin<Organization,
						LeftJoin<BAccountR,
							On<Organization.bAccountID, Equal<BAccountR.bAccountID>>,
						LeftJoin<Contact,
							On<BAccountR.defContactID, Equal<Contact.contactID>>>>,
						Where<Organization.organizationID, Equal<Required<Organization.organizationID>>>>
					.Select(graph, organizationID))
			{
				return (Contact)res;
			}

			return null;
		}

		public static IEnumerable<Contact> GetDefaultContactForCurrentOrganization(PXGraph graph)
		{
			int? organizationID = PXAccess.GetParentOrganizationID(graph.Accessinfo.BranchID);

			Contact contact = GetDefaultContact(graph, organizationID);

			return contact != null ? new Contact[] {contact} : new Contact[] { };
		}

		#endregion

		#region Public Helpers

		public static void RedirectTo(int? organizationID)
		{
			var organizationMaint = CreateInstance<OrganizationMaint>();

			Organization organization = FindOrganizationByID(organizationMaint, organizationID);

			if (organization == null)
				return;

			organizationMaint.BAccount.Current = organizationMaint.BAccount.Search<OrganizationBAccount.bAccountID>(organization.BAccountID);

			throw new PXRedirectRequiredException(organizationMaint, true, string.Empty)
			{
				Mode = PXBaseRedirectException.WindowMode.NewWindow
			};
		}

		#endregion

		#region Graph Extensions

		public class OrganizationLedgerLinkMaint : OrganizationLedgerLinkMaintBase<OrganizationMaint, OrganizationBAccount>
		{
			public PXAction<OrganizationBAccount> ViewLedger;

			public PXSelectJoin<OrganizationLedgerLink,
								LeftJoin<Ledger,
									On<OrganizationLedgerLink.ledgerID, Equal<Ledger.ledgerID>>>,
								Where<OrganizationLedgerLink.organizationID, Equal<Current<Organization.organizationID>>>>
								OrganizationLedgerLinkWithLedgerSelect;

			public override PXSelectBase<OrganizationLedgerLink> OrganizationLedgerLinkSelect => OrganizationLedgerLinkWithLedgerSelect;
			public override PXSelectBase<Organization> OrganizationViewBase => Base.OrganizationView;

			public PXSelect<Ledger> LedgerView;

			public override PXSelectBase<Ledger> LedgerViewBase => LedgerView;

			protected override Organization GetUpdatingOrganization(int? organizationID)
			{
				return Base.OrganizationView.Current;
			}

			protected override Type VisibleField => typeof(OrganizationLedgerLink.ledgerID);

			//Overridden because PXDBDefault is not compatible with PXDimesionSelector
			[PXDBInt(IsKey = true)]
			[PXDBDefault(typeof(Organization.organizationID))]
			[PXParent(typeof(Select<Organization, Where<Organization.organizationID, Equal<Current<OrganizationLedgerLink.organizationID>>>>))]
			protected virtual void OrganizationLedgerLink_OrganizationID_CacheAttached(PXCache sender)
			{
			}

			[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
			[PXButton]
			public virtual IEnumerable viewLedger(PXAdapter adapter)
			{
				OrganizationLedgerLink link = OrganizationLedgerLinkSelect.Current;

				if (link != null)
				{
					GeneralLedgerMaint.RedirectTo(link.LedgerID);
				}

				return adapter.Get();
			}
		}
		#endregion

		#region Custom Graphs

		protected class SeparateBranchMaint : PXGraph<BranchMaint>
		{
			public PXSelect<GL.Branch> BranchView;
		}

		#endregion

		#region Custom Actions

		public class OrganizationChangeID : PXChangeID<OrganizationBAccount, OrganizationBAccount.acctCD>
		{
			public OrganizationChangeID(PXGraph graph, string name)	: base(graph, name) {}

			public OrganizationChangeID(PXGraph graph, Delegate handler) : base(graph, handler) { }

			protected override void Initialize()
			{
				DuplicatedKeyMessage = EP.Messages.BAccountExists;

				_Graph.FieldUpdated.AddHandler<OrganizationBAccount.acctCD>((sender, e) =>
				{
					string oldCD = (string)e.OldValue;
					string newCD = ((OrganizationBAccount)e.Row).AcctCD;
					int? id = ((OrganizationBAccount)e.Row).BAccountID;
					if (oldCD == null || newCD == null) return;

					Organization org = PXSelect<Organization, Where<Organization.organizationCD, Equal<Required<Organization.organizationCD>>>>.Select(_Graph, oldCD);
					if (org?.OrganizationType == OrganizationTypes.WithoutBranches && id > 0)
					{
						ChangeCD<GL.Branch.branchCD>(_Graph.Caches<GL.Branch>(), oldCD, newCD);
						_Graph.Caches<GL.Branch>().Normalize();
					}
					ChangeCD<Organization.organizationCD>(_Graph.Caches<Organization>(), oldCD, newCD);
					_Graph.Caches<Organization>().Normalize();
				});

				base.Initialize();
			}
		}

		#endregion

		#region Types

		[Serializable]
		public class State: IBqlTable
		{
			public abstract class clearAccessRoleOnChildBranches : PX.Data.BQL.BqlBool.Field<clearAccessRoleOnChildBranches> { }
			[PXBool]
			public bool? ClearAccessRoleOnChildBranches { get; set; }

			public abstract class isBranchTabVisible : PX.Data.BQL.BqlBool.Field<isBranchTabVisible> { }
			[PXBool]
			public bool? IsBranchTabVisible { get; set; }

			public abstract class isDeliverySettingsTabVisible : PX.Data.BQL.BqlBool.Field<isDeliverySettingsTabVisible> { }
			[PXBool]
			public bool? IsDeliverySettingsTabVisible { get; set; }

			public abstract class isGLAccountsTabVisible : PX.Data.BQL.BqlBool.Field<isGLAccountsTabVisible> { }
			[PXBool]
			public bool? IsGLAccountsTabVisible { get; set; }

			public abstract class isRutRotTabVisible : PX.Data.BQL.BqlBool.Field<isRutRotTabVisible> { }
			[PXBool]
			public bool? IsRutRotTabVisible { get; set; }
		}

		#endregion

		#region CTor + Public members

		public PXSelect<Organization, Where<Organization.bAccountID, Equal<Current<OrganizationBAccount.bAccountID>>>> OrganizationView;

		public PXSelect<OrganizationBAccount, Where<OrganizationBAccount.bAccountID, Equal<Current<OrganizationBAccount.bAccountID>>>> CurrentBAccount;

		public PXSelectJoin<GL.Branch,
								LeftJoin<Roles,
									On<GL.Branch.roleName, Equal<Roles.rolename>>>,
								Where<GL.Branch.organizationID, Equal<Current<Organization.organizationID>>>>
								BranchesView;

		public PXSelectJoin<EPEmployee,
								InnerJoin<BAccount2,
									On<EPEmployee.parentBAccountID, Equal<BAccount2.bAccountID>>,
								InnerJoin<GL.BranchAlias,
									On<BAccount2.bAccountID, Equal<GL.BranchAlias.bAccountID>>,
								InnerJoin<Contact, 
									On<Contact.contactID, Equal<EPEmployee.defContactID>, 
										And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
								LeftJoin<Address, 
									On<Address.addressID, Equal<EPEmployee.defAddressID>,
										And<Address.bAccountID, Equal<EPEmployee.parentBAccountID>>>>>>>,
								Where<GL.BranchAlias.organizationID, Equal<Current<Organization.organizationID>>>>
								Employees;

		public override PXSelectBase<EPEmployee> EmployeesAccessor => Employees;

		public PXSelect<NoteDoc, Where<NoteDoc.noteID, Equal<Current<OrganizationBAccount.noteID>>>> Notedocs;

		
		public PXSelect<CommonSetup> Commonsetup;
		public PXSelect<CurrencyList, Where<CurrencyList.curyID, Equal<Current<Company.baseCuryID>>>> CompanyCurrency;
		public PXSelect<Currency, Where<Currency.curyID, Equal<Current<CurrencyList.curyID>>>> FinancinalCurrency;

		public PXFilter<State> StateView;

		/// <summary>
		/// The obviously declared view which provides cache for SetVisible function in <see cref="OrganizationBAccount_RowSelected"/>.
		/// </summary>
		public PXSelect<BranchAlias> BranchAliasView;

		public PXSelect<OrganizationFinYear> OrganizationYear;
		public PXSelect<OrganizationFinPeriod> OrganizationPeriods;

		public PXSelect<FABookYear> FaBookYear;
		public PXSelect<FABookPeriod> FaBookPeriod;

		#region Actions

		public OrganizationChangeID ChangeID;
		public new PXAction<OrganizationBAccount> validateAddresses;
		public new PXAction<OrganizationBAccount> viewContact;
		public new PXAction<OrganizationBAccount> newContact;
		public new PXAction<OrganizationBAccount> viewLocation;
		public new PXAction<OrganizationBAccount> newLocation;
		public new PXAction<OrganizationBAccount> setDefault;

		public new PXAction<OrganizationBAccount> viewMainOnMap;
		public new PXAction<OrganizationBAccount> viewDefLocationOnMap;

		public PXAction<OrganizationBAccount> AddLedger;
		public PXAction<OrganizationBAccount> AddBranch;
		public PXAction<OrganizationBAccount> ViewBranch;

		#endregion

		public OrganizationMaint()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.branch>())
			{
				Organization anyOrganization = PXSelectReadonly<Organization>.SelectWindowed(this, 0, 1);

				BAccount.Cache.AllowInsert = anyOrganization == null;
				BAccount.Cache.AllowDelete = anyOrganization == null;
			}

			BranchesView.Cache.AllowDelete = false;

			PXUIFieldAttribute.SetEnabled<Organization.organizationType>(OrganizationView.Cache, null, PXAccess.FeatureInstalled<FeaturesSet.branch>());

			PXUIFieldAttribute.SetReadOnly(BranchesView.Cache, null, true);

			PXDimensionAttribute.SuppressAutoNumbering<Branch.branchCD>(BranchesView.Cache, true);
			PXDimensionAttribute.SuppressAutoNumbering<Organization.organizationCD>(OrganizationView.Cache, true);

			ActionsMenu.AddMenuAction(validateAddresses);
			ActionsMenu.AddMenuAction(ChangeID);

			if (EntityInUseHelper.IsEntityInUse<CurrencyInUse>())
			{
				PXUIFieldAttribute.SetEnabled< CurrencyList.decimalPlaces>(CompanyCurrency.Cache, null, false);
			}
		}

		protected virtual BranchValidator GetBranchValidator()
		{
			return new BranchValidator(this);
		}

		protected BranchMaint BranchMaint;
		public virtual BranchMaint GetBranchMaint()
		{
			if (BranchMaint != null)
			{
				return BranchMaint;
			}
			else
			{
				BranchMaint = CreateInstance<BranchMaint>();

				return BranchMaint;
			}
		}
		
		#endregion

		#region Cache Attached Events

		[PXDBInt()]
		[PXDBChildIdentity(typeof(LocationExtAddress.locationID))]
		[PXUIField(DisplayName = "Default Location", Visibility = PXUIVisibility.Invisible)]
		[PXSelector(typeof(Search<LocationExtAddress.locationID,
				Where<LocationExtAddress.bAccountID,
					Equal<Current<OrganizationBAccount.bAccountID>>>>),
			DescriptionField = typeof(LocationExtAddress.locationCD),
			DirtyRead = true)]
		protected virtual void OrganizationBAccount_DefLocationID_CacheAttached(PXCache sender)
		{

		}

		#region Currency
		#region CuryID

		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL")]
		[PXDefault()]
		[PXUIField(DisplayName = "Currency ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CM.Currency.curyID>), CacheGlobal = true)]
		protected virtual void Currency_CuryID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RealLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RealLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RevalLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RevalLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TranslationGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationGainAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region TranslationGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationGainSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region TranslationLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationLossAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region TranslationLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_TranslationLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedGainAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedGainSubID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region UnrealizedLossAcctID

		[PXUIField(Required = false)]
		[PXDBInt()]
		protected virtual void Currency_UnrealizedLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region UnrealizedLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_UnrealizedLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingGainAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingGainAcctID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region RoundingGainSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingGainSubID_CacheAttached(PXCache sender)
		{
		}

		#endregion
		#region RoundingLossAcctID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingLossAcctID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#region RoundingLossSubID

		[PXDBInt()]
		[PXUIField(Required = false)]
		protected virtual void Currency_RoundingLossSubID_CacheAttached(PXCache sender)
		{
		}
		#endregion
		#endregion
		#endregion

		#region Events Handlers

		#region OrganizationBAccount

		protected virtual void OrganizationBAccount_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			this.OnBAccountRowInserted(sender, e);

			Company rec = Company.Select();
			Company.Cache.SetStatus(rec, PXEntryStatus.Updated);

			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
				return;

			OrganizationView.Insert(new Organization()
			{
				OrganizationCD = orgBAccount.AcctCD,
				OrganizationName = orgBAccount.AcctName,
				NoteID = orgBAccount.NoteID
			});
			OrganizationView.Cache.IsDirty = false;

			orgBAccount.Type = GetBAccountType(OrganizationView.Current);
		}

		protected virtual void OrganizationBAccount_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
				return;

			CanBeOrganizationDeleted(OrganizationView.Current);
		}

		protected virtual void OrganizationBAccount_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			OrganizationView.Delete(OrganizationView.Current);
		}

		#endregion

		#region Organization
		protected virtual void OrganizationBAccount_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var orgBAccount = e.Row as OrganizationBAccount;

			if (orgBAccount == null)
			{
				// TODO: Redesign to persist before actions and eliminate this code
				createLedger.SetEnabled(false);
				AddBranch.SetEnabled(false);
				return;
			}

			if (orgBAccount.AcctCD?.Trim() != OrganizationView.Current?.OrganizationCD?.Trim())
			{
				OrganizationView.Current = OrganizationView.Select();
			}

			Ledger actualLedger = PXSelectJoin<Ledger,
				InnerJoin<OrganizationLedgerLink,
					On<Ledger.ledgerID, Equal<OrganizationLedgerLink.ledgerID>>,
				InnerJoin<Organization, 
					On<Organization.organizationID, Equal<OrganizationLedgerLink.organizationID>>>>,
				Where<Ledger.balanceType, Equal<LedgerBalanceType.actual>,
					And<Organization.organizationID, Equal<Current<Organization.organizationID>>>>>.Select(this);

			// TODO: Redesign to persist before actions and eliminate this code (except existing actual ledger check)
			bool isPersistedOrganization = !this.IsPrimaryObjectInserted();
			createLedger.SetEnabled(actualLedger == null && isPersistedOrganization);
			AddBranch.SetEnabled(isPersistedOrganization);
			newContact.SetEnabled(isPersistedOrganization);

			State state = StateView.Current;

			Organization org = OrganizationView.Current;

			PXUIFieldAttribute.SetVisible<Organization.fileTaxesByBranches>(OrganizationView.Cache, null, org != null && org.OrganizationType == OrganizationTypes.WithBranchesBalancing);

			PXUIFieldAttribute.SetVisible<BranchAlias.branchCD>(BranchAliasView.Cache, null, org != null && org.OrganizationType != OrganizationTypes.WithoutBranches);

			if (org != null)
			{
				state.IsBranchTabVisible = org.OrganizationType != OrganizationTypes.WithoutBranches &&
				                           OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(org) as string != OrganizationTypes.WithoutBranches;

				state.IsDeliverySettingsTabVisible = org.OrganizationType == OrganizationTypes.WithoutBranches;
				state.IsGLAccountsTabVisible = PXAccess.FeatureInstalled<FeaturesSet.subAccount>() 
												&& org.OrganizationType == OrganizationTypes.WithoutBranches;
				state.IsRutRotTabVisible = PXAccess.FeatureInstalled<FeaturesSet.rutRotDeduction>() && org.OrganizationType == OrganizationTypes.WithoutBranches;
			}
			else
			{
				state.IsBranchTabVisible = false;
				state.IsDeliverySettingsTabVisible = false;
				state.IsGLAccountsTabVisible = false;
				state.IsRutRotTabVisible = false;
			}
		}
		
		
		protected virtual void Organization_OrganizationType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var organization = e.Row as Organization;
			var orgBAccount = PXCache<OrganizationBAccount>.CreateCopy(BAccount.Current);

			if (organization != null && orgBAccount != null)
			{
				string origOrganizationType = (string) sender.GetValueOriginal<Organization.organizationType>(organization);

				if (organization.OrganizationType != origOrganizationType)
				{
					orgBAccount.Type = GetBAccountType(organization);

					BAccount.Update(orgBAccount);
				}

				if (organization.OrganizationType != OrganizationTypes.WithBranchesBalancing)
				{
					organization.FileTaxesByBranches = false;
					organization.Reporting1099ByBranches = false;
				}
			}
		}

		protected virtual void Organization_OrganizationType_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Organization organization = e.Row as Organization;

			if (organization == null)
				return;

			VerifyOrganizationType((string)e.NewValue, 
									organization.OrganizationType, 
									organization);
		}

		protected virtual void _(Events.FieldUpdated<Organization, Organization.reporting1099ByBranches> e)
		{
			if(e.Row.Reporting1099ByBranches == true)
			{
				e.Row.Reporting1099 = false;
			}
		}

		#endregion

		protected virtual void Organization_Active_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Organization item = e.Row as Organization;

			if (item == null)
				return;

			GL.Branch[] branch = BranchMaint.GetChildBranches(this, OrganizationView.Current.OrganizationID).ToArray();

			if (branch.Any())
			{
				GetBranchValidator().ValidateActiveField(branch.Select(b => b.BranchID).ToArray(), (bool?)e.NewValue, item, skipActivateValidation:true);
			}
		}

		protected virtual void Organization_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open)
			{
				var rec =
				(PXResult < Organization, BAccount > )
				PXSelectJoin<Organization,
						InnerJoin<BAccount, On<BAccount.bAccountID, Equal<Organization.bAccountID>>>,
						Where<Organization.organizationID, Equal<Current<Organization.organizationID>>>>
					.SelectSingleBound(this, new[] {e.Row});
				if(rec != null)
				{
					Organization org = rec;
					BAccount acct = rec;
					if (org.OrganizationCD != acct.AcctCD)
					{
						throw new PXException(PX.Objects.CS.Messages.CantAutoNumber);
					}
				}
			}
		}
		protected virtual void OrganizationBAccount_AcctName_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.OnBAccountAcctNameFieldUpdated(sender, e);

			OrganizationBAccount organizationBAccount = e.Row as OrganizationBAccount;

			if (organizationBAccount == null)
				return;

			if (OrganizationView.Current != null)
			{
				OrganizationView.Current.OrganizationName = organizationBAccount.AcctName;
				OrganizationView.Cache.Update(OrganizationView.Current);
			}
		}

		public virtual void CommonSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation.Command() == PXDBOperation.Delete)
				return;

			PXDefaultAttribute.SetPersistingCheck<CommonSetup.weightUOM>(sender, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
			PXDefaultAttribute.SetPersistingCheck<CommonSetup.volumeUOM>(sender, e.Row,
				PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);
		}

		public virtual void CommonSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CommonSetup commonsetup = e.Row as CommonSetup;
			if (commonsetup == null) return;

			bool weightEnabled = true;
			bool volumeEnabled = true;
			if (!String.IsNullOrEmpty(commonsetup.WeightUOM))
			{
				InventoryItem itemWeight = PXSelect<InventoryItem, Where<InventoryItem.weightUOM, IsNotNull,
					And<InventoryItem.baseItemWeight, Greater<decimal0>>>>.SelectWindowed(this, 0, 1);
				weightEnabled = (itemWeight == null);
			}

			if (!String.IsNullOrEmpty(commonsetup.VolumeUOM))
			{
				InventoryItem itemVolume = PXSelect<InventoryItem, Where<InventoryItem.volumeUOM, IsNotNull,
					And<InventoryItem.baseItemVolume, Greater<decimal0>>>>.SelectWindowed(this, 0, 1);
				volumeEnabled = (itemVolume == null);
			}
			PXUIFieldAttribute.SetEnabled<CommonSetup.weightUOM>(sender, commonsetup, weightEnabled);
			PXUIFieldAttribute.SetEnabled<CommonSetup.volumeUOM>(sender, commonsetup, volumeEnabled);

			if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>() && commonsetup.DecPlQty == 0m)
			{
				sender.RaiseExceptionHandling<CommonSetup.decPlQty>(commonsetup, commonsetup.DecPlQty,
					new PXSetPropertyException(Messages.LowQtyDecimalPrecision, PXErrorLevel.Warning));
			}
			else
			{
				sender.RaiseExceptionHandling<CommonSetup.decPlQty>(commonsetup, commonsetup.DecPlQty,null);
			}

		}

		
		[PXDBDefault(typeof(Organization.organizationID))]
		[PXDBInt(IsKey = true, BqlTable = typeof(GL.FinPeriods.TableDefinition.FinYear))]
		[PXParent(typeof(Select<
			Organization,
			Where<Organization.organizationID, Equal<Current<OrganizationFinYear.organizationID>>>>))]
		protected virtual void OrganizationFinYear_OrganizationID_CacheAttached(PXCache sender) { }

		[PXDBDefault(typeof(Organization.organizationID))]
		[PXDBInt(IsKey = true, BqlTable = typeof(GL.FinPeriods.TableDefinition.FinPeriod))]
		protected virtual void OrganizationFinPeriod_OrganizationID_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(Organization.organizationID))]
		protected virtual void _(Events.CacheAttached<FABookYear.organizationID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDBDefault(typeof(Organization.organizationID))]
		protected virtual void _(Events.CacheAttached<FABookPeriod.organizationID> e) { }

		protected void CreateOrganizationCalendar(Organization organization, PXEntryStatus orgStatus)
		{
			if (orgStatus != PXEntryStatus.Inserted ||
				PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>()) return;

			#region Generate financial calendar

			PXCache<OrganizationFinYear> orgYearCache = this.Caches<OrganizationFinYear>();
			PXCache<OrganizationFinPeriod> orgPeriodCache = this.Caches<OrganizationFinPeriod>();

			// Preventing the re-insertion of the year and periods after the previous failed Persist()
			orgYearCache.Clear();
			orgPeriodCache.Clear();

			IEnumerable<MasterFinYear> masterYears = PXSelect<MasterFinYear>.Select(this).RowCast<MasterFinYear>();
			IEnumerable<MasterFinPeriod> masterPeriods = PXSelect<MasterFinPeriod>.Select(this).RowCast<MasterFinPeriod>();

			// TODO: Share code fragment with MasterFinPeriodMaint.SynchronizeBaseAndOrganizationPeriods()
			foreach (MasterFinYear masterYear in masterYears)
			{
				OrganizationFinYear insertedOrgYear = (OrganizationFinYear)orgYearCache.Insert(new OrganizationFinYear
				{
					OrganizationID = organization.OrganizationID,
					Year = masterYear.Year,
					FinPeriods = masterYear.FinPeriods,
					StartMasterFinPeriodID = FinPeriodUtils.GetFirstFinPeriodIDOfYear(masterYear),
					StartDate = masterYear.StartDate,
					EndDate = masterYear.EndDate
				});

				if (insertedOrgYear == null)
				{
					throw new PXException(Messages.FailedToGenerateFinYear, masterYear.Year, organization.OrganizationCD?.Trim());
				}
			}

			bool isCentralizedManagement = PXAccess.FeatureInstalled<FeaturesSet.centralizedPeriodsManagement>();
			foreach (MasterFinPeriod masterPeriod in masterPeriods)
			{
				OrganizationFinPeriod insertedOrgPeriod = (OrganizationFinPeriod)orgPeriodCache.Insert(new OrganizationFinPeriod
				{
					OrganizationID = organization.OrganizationID,
					FinPeriodID = masterPeriod.FinPeriodID,
					MasterFinPeriodID = masterPeriod.FinPeriodID,
					FinYear = masterPeriod.FinYear,
					PeriodNbr = masterPeriod.PeriodNbr,
					Custom = masterPeriod.Custom,
					DateLocked = masterPeriod.DateLocked,
					StartDate = masterPeriod.StartDate,
					EndDate = masterPeriod.EndDate,

					Status = isCentralizedManagement ? masterPeriod.Status : FinPeriod.status.Inactive,
					ARClosed = isCentralizedManagement ? masterPeriod.ARClosed : false,
					APClosed = isCentralizedManagement ? masterPeriod.APClosed : false,
					FAClosed = isCentralizedManagement ? masterPeriod.FAClosed : false,
					CAClosed = isCentralizedManagement ? masterPeriod.CAClosed : false,
					INClosed = isCentralizedManagement ? masterPeriod.INClosed : false,

					Descr = masterPeriod.Descr,
				});

				PXDBLocalizableStringAttribute.CopyTranslations<MasterFinPeriod.descr, OrganizationFinPeriod.descr>(
					this.Caches<MasterFinPeriod>(),
					masterPeriod,
					orgPeriodCache,
					insertedOrgPeriod);

				if (insertedOrgPeriod == null)
				{
					throw new PXException(
						Messages.FailedToGenerateFinPeriod, 
						FinPeriodIDFormattingAttribute.FormatForDisplay(masterPeriod.FinPeriodID), 
						organization.OrganizationCD?.Trim());
				}
			}
			#endregion

			#region Generate fixed asset book calendar
			PXCache<FABookYear> faBookYearCache = this.Caches<FABookYear>();
			PXCache<FABookPeriod> faBookPeriodCache = this.Caches<FABookPeriod>();

			// Preventing the re-insertion of the year and periods after the previous failed Persist()
			faBookYearCache.Clear();
			faBookPeriodCache.Clear();

			foreach (FABook postingBook in SelectFrom<FABook>.Where<FABook.updateGL.IsEqual<True>>.View.Select(this))
			{
				IEnumerable<FABookYear> faBookMasterYears = SelectFrom<FABookYear>
					.Where<FABookYear.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
						.And<FABookYear.bookID.IsEqual<@P.AsInt>>>
					.View
					.Select(this, postingBook.BookID)
					.RowCast<FABookYear>();

				IEnumerable<FABookPeriod> faBookMasterPeriods = SelectFrom<FABookPeriod>
					.Where<FABookPeriod.organizationID.IsEqual<FinPeriod.organizationID.masterValue>
						.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>>
					.View
					.Select(this, postingBook.BookID)
					.RowCast<FABookPeriod>();

				foreach (FABookYear faBookMasterYear in faBookMasterYears)
				{
					FABookYear insertedYear = (FABookYear)faBookYearCache.Insert(new FABookYear
					{
						OrganizationID = organization.OrganizationID,
						BookID = postingBook.BookID,
						Year = faBookMasterYear.Year,
						FinPeriods = faBookMasterYear.FinPeriods,
						StartMasterFinPeriodID = FinPeriodUtils.GetFirstFinPeriodIDOfYear(faBookMasterYear),
						StartDate = faBookMasterYear.StartDate,
						EndDate = faBookMasterYear.EndDate
					});

					if (insertedYear == null)
					{
						throw new PXException(
							Messages.FailedToGenerateFABookYear, 
							faBookMasterYear.Year, 
							postingBook.BookCode?.Trim(), 
							organization.OrganizationCD?.Trim());
				}
			}

				foreach (FABookPeriod faBookMasterPeriod in faBookMasterPeriods)
				{
					FABookPeriod insertedPeriod = (FABookPeriod)faBookPeriodCache.Insert(new FABookPeriod
					{
						OrganizationID = organization.OrganizationID,
						BookID = postingBook.BookID,
						FinPeriodID = faBookMasterPeriod.FinPeriodID,
						MasterFinPeriodID = faBookMasterPeriod.FinPeriodID,
						FinYear = faBookMasterPeriod.FinYear,
						PeriodNbr = faBookMasterPeriod.PeriodNbr,
						Custom = faBookMasterPeriod.Custom,
						DateLocked = faBookMasterPeriod.DateLocked,
						StartDate = faBookMasterPeriod.StartDate,
						EndDate = faBookMasterPeriod.EndDate,

						Descr = faBookMasterPeriod.Descr,
					});

					if (insertedPeriod == null)
					{
						throw new PXException(
							Messages.FailedToGenerateFABookPeriod, 
							FinPeriodIDFormattingAttribute.FormatForDisplay(faBookMasterPeriod.FinPeriodID),
							postingBook.BookCode?.Trim(),
							organization.OrganizationCD?.Trim());
					}
				}
			}
			#endregion
		}

		public override void Persist()
		{
			Organization organization = OrganizationView.Select();
			PXEntryStatus orgBAccountStatus = BAccount.Cache.GetStatus(BAccount.Current);
			PXEntryStatus orgStatus = OrganizationView.Cache.GetStatus(OrganizationView.Current);

			Organization origOrganization = (Organization)OrganizationView.Cache.GetOriginal(organization);
			bool requestRelogin = Accessinfo.BranchID == null;

			if (organization != null)
			{
				CreateOrganizationCalendar(organization, orgStatus);

				if (organization.OrganizationType != OrganizationTypes.WithoutBranches 
					&& origOrganization?.RoleName != organization.RoleName)
				{
					if (organization.RoleName != null)
					{
						if (!IsImport)
						{
							BAccount.Ask(string.Empty,
								PXMessages.LocalizeFormatNoPrefix(GL.Messages.TheAccessRoleWillBeAssignedToAllBranchesOfTheCompany, organization.RoleName, BAccount.Current.AcctCD.Trim()),
								MessageButtons.OK);
						}
						
					}
					else
					{
						if (BAccount.Cache.GetStatus(BAccount.Current) != PXEntryStatus.Inserted)
						{
							if (!IsImport)
							{
								WebDialogResult dialogResult = BAccount.Ask(string.Empty,
									PXMessages.LocalizeFormatNoPrefix(GL.Messages.RemoveTheAccessRoleFromTheSettingsOfAllBranchesOfTheCompany,
										BAccount.Current.AcctCD.Trim()),
									MessageButtons.YesNo);

								StateView.Current.ClearAccessRoleOnChildBranches = dialogResult == WebDialogResult.Yes;
							}
							else
							{
								StateView.Current.ClearAccessRoleOnChildBranches = true;
							}
						}
					}
				}

				VerifyOrganizationType(organization.OrganizationType, origOrganization?.OrganizationType, organization);
				int? organizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);
				if (organizationID == organization.OrganizationID && organization.Active == false &&
					origOrganization.Active == true)
					requestRelogin = true;
			}

			List<Organization> deletedOrganizations = OrganizationView.Cache.Deleted.Cast<Organization>().ToList();

			foreach (Organization deletedOrganization in deletedOrganizations)
			{
				CanBeOrganizationDeleted(deletedOrganization);
			}

			bool resetPageCache = false;

			if (!IsImport && !IsExport && !IsContractBasedAPI && !IsMobile)
			{
				resetPageCache = BAccount.Cache.Inserted.Any_() || BAccount.Cache.Deleted.Any_();
				if (!resetPageCache)
				{
					foreach (object updated in BAccount.Cache.Updated)
					{
						if (BAccount.Cache.IsValueUpdated<string, OrganizationBAccount.acctName>(updated, StringComparer.CurrentCulture))
						{
							resetPageCache = true;
							break;
						}
					}

					foreach (object updated in OrganizationView.Cache.Updated)
					{
						if (OrganizationView.Cache.IsValueUpdated<bool?, Organization.overrideThemeVariables>(updated)
							|| OrganizationView.Cache.IsValueUpdated<string, Organization.backgroundColor>(updated, StringComparer.OrdinalIgnoreCase)
						    || OrganizationView.Cache.IsValueUpdated<string, Organization.primaryColor>(updated, StringComparer.OrdinalIgnoreCase))
						{
							resetPageCache = true;
						}
						else if (OrganizationView.Cache.IsValueUpdated<string, Organization.roleName>(updated, StringComparer.OrdinalIgnoreCase)) {
							resetPageCache = true;
							requestRelogin = true;
							break;
						}
					}


				}
			}

			using (var tranScope = new PXTransactionScope())
			{
				base.Persist();

				try
				{
					ProcessOrganizationTypeChanging(orgBAccountStatus, origOrganization, organization);

					int? organizationID = PXAccess.GetParentOrganizationID(Accessinfo.BranchID);

					foreach (Organization deletedOrganization in deletedOrganizations)
					{
						ProcessOrganizationBAccountDeletion(deletedOrganization);

						requestRelogin = organizationID == deletedOrganization.OrganizationID;
					}

					SyncLedgerBaseCuryID();
				}
				finally
				{
					BranchesView.Cache.Clear();
				}

				ProcessPublicableToBranchesFieldsChanging(origOrganization, organization, orgStatus);

				tranScope.Complete();
			}

			//using (PXTransactionScope tran = new PXTransactionScope())
			//{
			//	this.Caches[typeof(Organization)].Clear();
			//	bool clearRoleNames = true;
			//	foreach (Organization org in PXSelect<Organization>.Select(this))
			//	{
			//		if (org.RoleName != null) clearRoleNames = false;
			//	}
			//	using (PXReadDeletedScope rds = new PXReadDeletedScope())
			//	{
			//		if (clearRoleNames)
			//		{
			//			PXDatabase.Update<Organization>(new PXDataFieldAssign<Organization.roleName>(null));
			//		}
			//	}
			//	tran.Complete();
			//}

			SelectTimeStamp();
			OrganizationView.Cache.Clear();
			OrganizationView.Cache.ClearQueryCacheObsolete();
			OrganizationView.Current = OrganizationView.Select();

			if (requestRelogin)
			{
				PXAccess.ResetBranchSlot();
				PXLogin.SetBranchID(CreateInstance<PX.SM.SMAccessPersonalMaint>().GetDefaultBranchId());
			}

			if (resetPageCache) // to refresh branch combo
			{
				PXPageCacheUtils.InvalidateCachedPages();
				PXDatabase.SelectTimeStamp(); //clear db slots
				if(!this.UnattendedMode) throw new PXRedirectRequiredException(this, "Organization", true);
			}

			if (resetPageCache && requestRelogin)
			{
				// otherwise page will be refreshed on the next request by PXPage.NeedRefresh
				if (System.Web.HttpContext.Current == null)
				{
					throw new PXRedirectRequiredException(this, "Organization", true);
				}
			}
		}

		protected void SyncLedgerBaseCuryID()
		{
			GeneralLedgerMaint ledgerMaint = CreateInstance<GeneralLedgerMaint>();
			string organizationBaseCuryID = this.Company.Current.BaseCuryID;

			foreach (Ledger ledger in ledgerMaint.LedgerRecords.Select())
			{
				if (ledger.BalanceType == LedgerBalanceType.Actual && ledger.BaseCuryID != organizationBaseCuryID)
				{
					ledger.BaseCuryID = organizationBaseCuryID;
					ledgerMaint.LedgerRecords.Update(ledger);
				}
			}

			ledgerMaint.Actions.PressSave();
		}

		protected virtual void CanBeOrganizationDeleted(Organization organization)
		{
			CheckBranchesForDeletion(organization);
		}

		// TODO: Rework to RIC on Delete engine
		protected virtual void CheckBranchesForDeletion(Organization organization)
		{
			GL.Branch[] childBranches = PXSelectReadonly<GL.Branch,
														Where<GL.Branch.organizationID, Equal<Required<GL.Branch.organizationID>>>>
														.Select(this, organization.OrganizationID)
														.RowCast<GL.Branch>()
														.ToArray();

			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(organization);

			if (origOrgType == OrganizationTypes.WithoutBranches)
			{
				BranchValidator branchValidator = new BranchValidator(this);

				branchValidator.CanBeBranchesDeleted(childBranches, isOrganizationWithoutBranchesDeletion:true);
			}
			else
			{
				if (childBranches.Any())
				{
					throw new PXException(GL.Messages.CompanyCannotBeDeletedBecauseBranchOrBranchesExistForThisCompany,
						organization.OrganizationCD.Trim(),
						childBranches.Select(b => b.BranchCD.Trim()).ToArray().JoinIntoStringForMessage());
				}
			}
		}

		private void ProcessOrganizationBAccountDeletion(Organization organization)
		{
			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(organization);

			if (origOrgType == OrganizationTypes.WithoutBranches)
			{
				GL.Branch childBranch = BranchMaint.GetChildBranches(this, organization.OrganizationID).SingleOrDefault();

				if (childBranch != null)
				{
					if (childBranch.BAccountID != organization.BAccountID)
					{
						BranchMaint branchMaint = GetBranchMaint();
						branchMaint.Clear(PXClearOption.ClearAll);

						branchMaint.BAccount.Current =
							branchMaint.BAccount.Search<BranchMaint.BranchBAccount.bAccountID>(childBranch.BAccountID);

						branchMaint.BAccount.Delete(branchMaint.BAccount.Current);

						branchMaint.Actions.PressSave();
					}
					else
					{
						BranchesView.Delete(childBranch);
						BranchesView.Cache.Persist(PXDBOperation.Delete);
					}
				}
			}
		}

		private void ProcessOrganizationTypeChanging(PXEntryStatus orgBAccountStatus, Organization origOrganization, Organization organization)
		{
			if (organization == null)
				return;

			if (orgBAccountStatus == PXEntryStatus.Inserted ||
			    orgBAccountStatus == PXEntryStatus.Updated && BranchMaint.GetChildBranch(this, organization.OrganizationID) == null)
			{
				if (organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					CreateSingleBranchRecord(organization);
				}
			}
			else if (orgBAccountStatus == PXEntryStatus.Updated)
			{
				if (origOrganization?.OrganizationType == OrganizationTypes.WithoutBranches
				    && organization.OrganizationType != OrganizationTypes.WithoutBranches)
				{
					MakeBranchSeparate(BAccount.Current);
				}
				else if (origOrganization?.OrganizationType != OrganizationTypes.WithoutBranches
				         && organization.OrganizationType == OrganizationTypes.WithoutBranches)
				{
					MergeToSingleBAccount(organization);
				}
			}
		}

		protected virtual void CreateSingleBranchRecord(Organization organization)
		{			
			var branch = new GL.Branch()
			{
				OrganizationID = organization.OrganizationID,
				BAccountID = organization.BAccountID,
				BranchCD = organization.OrganizationCD,
			};

			branch = BranchesView.Cache.Update(branch) as GL.Branch;			

			BranchesView.Cache.Persist(PXDBOperation.Insert);
		}

		protected virtual void MakeBranchSeparate(OrganizationBAccount orgBAccount)
		{
			string newAcctCD = GetNewBranchCD();

			BAccount newBAccount = CreateSeparateBAccountForBranch(newAcctCD, orgBAccount.AcctName);

			MapBranchToNewBAccountAndChangeBranchCD(orgBAccount.AcctCD, newBAccount.AcctCD, newBAccount.BAccountID);

			AssignNewBranchToEmployees(orgBAccount.BAccountID, newBAccount.BAccountID, newBAccount.AcctCD);
		}

		protected virtual void MergeToSingleBAccount(Organization organization)
		{
			GL.Branch branch = BranchMaint.GetSeparateChildBranches(this, organization).Single();

			MapBranchToNewBAccountAndChangeBranchCD(branch.BranchCD, organization.OrganizationCD, organization.BAccountID);

			AssignNewBranchToEmployees(branch.BAccountID, organization.BAccountID, organization.OrganizationCD);

			DeleteBranchBAccount(branch.BAccountID);
		}

		protected virtual void AssignNewBranchToEmployees(int? oldBAccountID, int? newBAccountID, string branchCD)
		{
			IEnumerable<EPEmployee> employees = PXSelectReadonly<EPEmployee, 
														Where<EPEmployee.parentBAccountID, Equal<Required<EPEmployee.parentBAccountID>>>>
														.Select(this, oldBAccountID)
														.RowCast<EPEmployee>();

			var employeeMaint = CreateInstance<EmployeeMaint>();

			GL.Branch branch = PXSelectReadonly<GL.Branch, 
												Where<GL.Branch.branchCD, Equal<Required<GL.Branch.branchCD>>>>
										.Select(this, branchCD);

			foreach (var employee in employees)
			{
				employeeMaint.Clear();
				employeeMaint.Employee.Current = employee;				
				Address defAddress = employeeMaint.Address.Select();				
				Contact defContact = employeeMaint.Contact.Select();

				var employeeCopy = PXCache<EPEmployee>.CreateCopy(employee);
				defAddress = PXCache<Address>.CreateCopy(defAddress);
				defContact = PXCache<Contact>.CreateCopy(defContact);				

				employeeCopy.ParentBAccountID = newBAccountID;
				employeeMaint.Employee.Cache.SetStatus(employeeCopy, PXEntryStatus.Updated);											
				defAddress.BAccountID = newBAccountID;								
				defContact.BAccountID = newBAccountID;
				employeeMaint.Address.Update(defAddress);
				employeeMaint.Contact.Update(defContact);
				employeeMaint.Actions.PressSave();
			}
		}

		protected virtual BAccount CreateSeparateBAccountForBranch(string acctCD, string acctName)
		{
			var baccountMaint = CreateInstance<SeparateBAccountMaint>();

			var baccount = new BAccount()
			{
				AcctCD = acctCD,
				AcctName = acctName,
				Type = BAccountType.BranchType
			};

			baccountMaint.BAccount.Insert(baccount);
			CopyGeneralInfoToBranch(baccountMaint, baccountMaint.DefContact.Current, baccountMaint.DefAddress.Current);
			CopyLocationDataToBranch(baccountMaint, baccountMaint.DefLocation.Current, baccountMaint.DefLocationContact.Current);
			baccountMaint.Actions.PressSave();

			return baccountMaint.BAccount.Current;
		}

		protected virtual void DeleteBranchBAccount(int? baccountID)
		{
			var baccountMaint = CreateInstance<SeparateBAccountMaint>();

			baccountMaint.BAccount.Current = baccountMaint.BAccount.Search<CR.BAccount.bAccountID>(baccountID);

			baccountMaint.BAccount.Delete(baccountMaint.BAccount.Current);

			baccountMaint.Actions.PressSave();
		}

		protected virtual void MapBranchToNewBAccountAndChangeBranchCD(string oldBranchCD, string newBranchCD, int? newBAccountID)
		{
			SeparateBranchMaint branchMaint = CreateInstance<SeparateBranchMaint>();

			GL.Branch branch = PXSelect<GL.Branch,
										Where<GL.Branch.branchCD, Equal<Required<GL.Branch.branchCD>>>>
										.Select(branchMaint, oldBranchCD);

			branch.BAccountID = newBAccountID;
			branchMaint.BranchView.Update(branch);

			PXChangeID<GL.Branch, GL.Branch.branchCD>.ChangeCD(branchMaint.BranchView.Cache, branch.BranchCD, newBranchCD);

			branchMaint.Actions.PressSave();
		}

		public override int Persist(Type cacheType, PXDBOperation operation)
		{
			int res = base.Persist(cacheType, operation);
			if (cacheType == typeof(OrganizationBAccount) && operation == PXDBOperation.Update)
			{
				foreach (PXResult<NoteDoc, UploadFile> rec in PXSelectJoin<NoteDoc, InnerJoin<UploadFile, On<NoteDoc.fileID, Equal<UploadFile.fileID>>>,
					Where<NoteDoc.noteID, Equal<Current<OrganizationBAccount.noteID>>>>.Select(this))
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

		protected virtual void OrganizationBAccount_OrganizationAcctCD_CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			//PXDBChildIdentity() hack
			if (e.Table != null && e.Operation == PXDBOperation.Update)
			{
				e.IsRestriction = false;
				e.Cancel = true;
			}
		}

		protected virtual IEnumerable commonsetup()
		{
			PXCache cache = Commonsetup.Cache;
			PXResultset<CommonSetup> ret = PXSelect<CommonSetup>.SelectSingleBound(this, null);

			if (ret.Count == 0)
			{
				CommonSetup setup = (CommonSetup)cache.Insert(new CommonSetup());
				cache.IsDirty = false;
				ret.Add(new PXResult<CommonSetup>(setup));
			}
			else if (cache.Current == null)
			{
				cache.SetStatus((CommonSetup)ret, PXEntryStatus.Notchanged);
			}

			return ret;
		}
		#endregion

		#region Events Company
		protected virtual void Company_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Company row = e.Row as Company;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<Company.baseCuryID>(sender, row, (glsetup.Select().Count == 0));
			}
		}

		protected virtual void Company_BaseCuryID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!string.IsNullOrEmpty((string)e.NewValue))
			{
				Currency currency = PXSelect<
					Currency,
					Where<Currency.curyID, Equal<Required<CurrencyList.curyID>>>>
					.SelectSingleBound(this, null, e.NewValue);

				if (currency == null)
				{
					CurrencyList bc = (CurrencyList)PXSelectorAttribute.Select<Company.baseCuryID>(sender, e.Row, e.NewValue);

					if (bc != null)
					{
						bc.IsActive = true;
						bc.IsFinancial = true;
						bc = CompanyCurrency.Update(bc);

						Currency finRow = (Currency)FinancinalCurrency.Cache.CreateInstance();
						finRow.CuryID = bc.CuryID;
						FinancinalCurrency.Insert(finRow);

						e.Cancel = true;
					}
				}
			}
		}

		protected virtual void Company_BaseCuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			CompanyCurrency.View.RequestRefresh();
		}

		protected virtual void CurrencyList_DecimalPlaces_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			CurrencyList currencyList = e.Row as CurrencyList;
			if (currencyList == null) return;

			WebDialogResult wdr = 
				CompanyCurrency.Ask(
					Messages.Warning,
					Messages.ChangingCurrencyPrecisionWarning,
					MessageButtons.YesNo,
					MessageIcon.Warning);

			e.NewValue = wdr == WebDialogResult.Yes 
				? e.NewValue 
				: currencyList.DecimalPlaces;
		}

		#endregion

		#region Action Handlers

		[PXButton]
		[PXUIField(DisplayName = "Add Ledger", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		public virtual IEnumerable addLedger(PXAdapter adapter)
		{
			GeneralLedgerMaint.RedirectTo(null);

			return adapter.Get();
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXUIField(DisplayName = "Add Branch", MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		public virtual IEnumerable addBranch(PXAdapter adapter)
		{
			BranchMaint graph = CreateInstance<BranchMaint>();
			graph.BAccount.Insert(new BranchMaint.BranchBAccount { OrganizationID = OrganizationView.Current.OrganizationID });
			graph.BAccount.Cache.IsDirty = false;
			graph.Caches<RedirectBranchParameters>().Insert(new RedirectBranchParameters { OrganizationID = OrganizationView.Current.OrganizationID });
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		[PXButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable viewBranch(PXAdapter adapter)
		{
			var branch = BranchesView.Current;

			if (branch != null)
			{
				BranchMaint.RedirectTo(branch.BAccountID);
			}

			return adapter.Get();
		}

		#endregion

		#region Service

		protected virtual void VerifyOrganizationType(string newOrgType, string oldOrgType, Organization organization)
		{
			if (OrganizationView.Cache.GetStatus(organization) == PXEntryStatus.Inserted)
				return;

			string errorMessage = null;

			if (oldOrgType != OrganizationTypes.WithoutBranches
			    && newOrgType == OrganizationTypes.WithoutBranches)
			{
				if (BranchMaint.MoreThenOneBranchExist(this, organization.OrganizationID))
				{
					errorMessage = GL.Messages.TheCompanyTypeCannotBeChangedToBecauseMoreThanOneBranchExistsForTheCompany;
				}
			}
			else if (oldOrgType == OrganizationTypes.WithBranchesNotBalancing && newOrgType == OrganizationTypes.WithBranchesBalancing)
			{
				if (BranchMaint.MoreThenOneBranchExist(this, organization.OrganizationID))
				{
					if (GLUtility.RelatedForOrganizationGLHistoryExists(this, organization.OrganizationID))
					{
						errorMessage = GL.Messages.TheCompanyTypeCannotBeChangedToBecauseDataHasBeenPostedForTheCompany;
					}
				}
			}

			if (errorMessage != null)
			{
				string localizedOrgType =
					PXStringListAttribute.GetLocalizedLabel<Organization.organizationType>(OrganizationView.Cache,
																									organization,
																									newOrgType);

				throw new PXSetPropertyException(errorMessage, localizedOrgType);
			}
		}

		public virtual bool ShouldInvokeOrganizationBranchSync(PXEntryStatus status)
		{
			return false;
		}

		public virtual void OnOrganizationBranchSync(BranchMaint branchMaint, Organization organization, BranchMaint.BranchBAccount branchBaccountCopy)
		{
		}

		protected virtual void ProcessPublicableToBranchesFieldsChanging(Organization origOrganization, Organization organization, PXEntryStatus status)
		{
			if (organization == null)
				return;
			bool organizationWithoutBranches = (origOrganization?.OrganizationType == OrganizationTypes.WithoutBranches);
			bool forceCopy = organization?.OrganizationType == OrganizationTypes.WithoutBranches &&
			                 (origOrganization?.OrganizationType != OrganizationTypes.WithoutBranches || status == PXEntryStatus.Inserted);

			bool shouldAdjustRoleName = origOrganization?.RoleName != organization.RoleName 
											&& (organization.RoleName == null && StateView.Current.ClearAccessRoleOnChildBranches == true
													|| organization.RoleName != null
													|| organization.OrganizationType == OrganizationTypes.WithoutBranches);

			bool shouldAdjustActive = origOrganization?.Active != organization.Active &&
			                          (organization.Active != true || organization.OrganizationType == OrganizationTypes.WithoutBranches);

			bool shouldAdjustLedgerID = origOrganization?.ActualLedgerID != organization.ActualLedgerID;

			bool shouldAdjustCountryID = organization.OrganizationType == OrganizationTypes.WithoutBranches && origOrganization?.CountryID != organization.CountryID;

			bool shouldInvokeSyncEx = ShouldInvokeOrganizationBranchSync(status);

			bool shouldLogoNameReport = origOrganization?.LogoNameReport != organization.LogoNameReport;
			bool shouldLogoName = origOrganization?.LogoName != organization.LogoName;

			if (forceCopy || shouldAdjustRoleName || shouldAdjustActive || shouldAdjustLedgerID || shouldAdjustCountryID || shouldLogoNameReport || shouldInvokeSyncEx)
			{
				BranchMaint branchMaint = GetBranchMaint();

				branchMaint.Clear(PXClearOption.ClearAll);

				IEnumerable<BranchMaint.BranchBAccount> childBranchBAccounts = 
					PXSelect<BranchMaint.BranchBAccount,
									Where<BranchMaint.BranchBAccount.organizationID, Equal<Required<BranchMaint.BranchBAccount.organizationID>>>>
									.Select(branchMaint, organization.OrganizationID)
									.RowCast<BranchMaint.BranchBAccount>();

				foreach (BranchMaint.BranchBAccount childBranchBAccount in childBranchBAccounts)
				{
					branchMaint.Clear();
					BranchMaint.BranchBAccount branchBaccountCopy = PXCache<BranchMaint.BranchBAccount>.CreateCopy(childBranchBAccount);

					if (shouldAdjustRoleName || forceCopy)
					{
						branchBaccountCopy.BranchRoleName = organization.RoleName;
					}

					if (shouldAdjustActive || forceCopy)
					{
						branchBaccountCopy.Active = organization.Active;
					}

					if (shouldAdjustLedgerID || forceCopy)
					{
						branchBaccountCopy.LedgerID = organization.ActualLedgerID;
					}

					if (shouldAdjustCountryID || forceCopy)
					{
						branchBaccountCopy.BranchCountryID = organization.CountryID;
					}

					branchBaccountCopy = BranchMaint.BAccount.Update(branchBaccountCopy);

					if (shouldInvokeSyncEx || forceCopy)
					{
						OnOrganizationBranchSync(branchMaint, organization, branchBaccountCopy);
					}

					if (forceCopy || shouldLogoNameReport)
					{
						branchBaccountCopy.OrganizationLogoNameReport = organization.LogoNameReport;
					}
					if ((organizationWithoutBranches && shouldLogoNameReport) || forceCopy)
					{
						branchBaccountCopy.BranchLogoNameReport = organization.LogoNameReport;
					}
					if ((organizationWithoutBranches && shouldLogoName) || forceCopy)
					{
						branchBaccountCopy.BranchLogoName = organization.LogoName;
					}

					BranchMaint.Actions.PressSave();

					BAccount.Cache.Clear();
					BAccount.Cache.ClearQueryCacheObsolete();
					BAccount.Current = BAccount.Search<OrganizationBAccount.acctCD>(organization.OrganizationCD);
				}
			}

			StateView.Current.ClearAccessRoleOnChildBranches = false;
		}

		#region Copying from Org to Branch

		protected virtual void CopyGeneralInfoToBranch(PXGraph graph, Contact destContact, Address destAddress)
		{
			CopyContactData(graph, DefContact.Select(), destContact);

			int? oldAddressID = destAddress.AddressID;
			int? oldAddressdBAccountID = destAddress.BAccountID;
			Guid? oldNoteID = destAddress.NoteID;
			var timeStamp = destAddress.tstamp;

			PXCache<Address>.RestoreCopy(destAddress, PXCache<Address>.CreateCopy(DefAddress.Select()));

			destAddress.AddressID = oldAddressID;
			destAddress.BAccountID = oldAddressdBAccountID;
			destAddress.NoteID = oldNoteID;
			destAddress.tstamp = timeStamp;

			graph.Caches<Address>().Update(destAddress);
		}

		protected virtual void CopyLocationDataToBranch(PXGraph graph, LocationExtAddress destLocation, Contact destLocationContact)
		{
			CopyContactData(graph, DefLocationContact.Select(), destLocationContact);

			int? oldLocationID = destLocation.LocationID;
			int? oldLocationBAccountID = destLocation.LocationBAccountID;
			int? oldAddressID = destLocation.AddressID;
			Guid? oldNoteID = destLocation.NoteID;
			int? oldBAccountBAccountID = destLocation.BAccountBAccountID;
			int? oldBAccountID = destLocation.BAccountID;
			int? oldDefAddressID = destLocation.DefAddressID;
			int? oldDefContactID = destLocation.DefContactID;
			int? oldVDefAddressID = destLocation.VDefAddressID;
			int? oldVDefContactID = destLocation.VDefContactID;
			int? oldVAPAccountLocationID = destLocation.VAPAccountLocationID;
			int? oldCARAccountLocationID = destLocation.CARAccountLocationID;
			int? oldVPaymentInfoLocationID = destLocation.VPaymentInfoLocationID;
			var timeStamp = destLocation.tstamp;

			PXCache<LocationExtAddress>.RestoreCopy(destLocation, PXCache<LocationExtAddress>.CreateCopy(DefLocation.Current ?? DefLocation.Select()));

			destLocation.LocationID = oldLocationID;
			destLocation.LocationBAccountID = oldLocationBAccountID;
			destLocation.AddressID = oldAddressID;
			destLocation.NoteID = oldNoteID;
			destLocation.BAccountBAccountID = oldBAccountBAccountID;
			destLocation.BAccountID = oldBAccountID;
			destLocation.DefAddressID = oldDefAddressID;
			destLocation.DefContactID = oldDefContactID;
			destLocation.VDefAddressID = oldVDefAddressID;
			destLocation.VDefContactID = oldVDefContactID;
			destLocation.VAPAccountLocationID = oldVAPAccountLocationID;
			destLocation.CARAccountLocationID= oldCARAccountLocationID;
			destLocation.VPaymentInfoLocationID = oldVPaymentInfoLocationID;
			destLocation.tstamp = timeStamp;

			graph.Caches<LocationExtAddress>().Update(destLocation);
		}

		protected virtual void CopyContactData(PXGraph graph, Contact contactSrc, Contact contactDest)
		{
			int? oldContactID = contactDest.ContactID;
			int? oldContactBAccountID = contactDest.BAccountID;
			int? oldDefAddress = contactDest.DefAddressID;
			Guid? oldNoteID = contactDest.NoteID;
			var timeStamp = contactDest.tstamp;

			PXCache<Contact>.RestoreCopy(contactDest, PXCache<Contact>.CreateCopy(contactSrc));

			contactDest.ContactID = oldContactID;
			contactDest.BAccountID = oldContactBAccountID;
			contactDest.DefAddressID = oldDefAddress;
			contactDest.NoteID = oldNoteID;
			contactDest.tstamp = timeStamp;

			graph.Caches<Contact>().Update(contactDest);
		}

		protected virtual void CopyRutRot(Organization organization, BranchMaint.BranchBAccount branchBAccount)
		{
			
		}

		#endregion

		protected virtual string GetBAccountType(Organization organization)
		{
			return organization.OrganizationType == OrganizationTypes.WithoutBranches
					? BAccountType.OrganizationBranchCombinedType
					: BAccountType.OrganizationType;
		}

		protected virtual string GetNewBranchCD()
		{
			string currentCD = PXMessages.LocalizeFormatNoPrefix(GL.Messages.NewBranchNameTemplate, string.Empty);
			int currentNumber = 0;
			do
			{
				BAccount bAccount;
				using (new PXReadDeletedScope()) // to prevent duplicate with deleted business account (DeletedDatabaseRecord = 1)
				{
					bAccount = PXSelectReadonly<
						BAccount,
													Where<BAccount.acctCD, Equal<Required<CR.BAccount.acctCD>>>>
													.Select(this, currentCD);
				}

				if (bAccount == null)
				{
					return currentCD;
				}
				else
				{
					currentNumber++;

					if (currentNumber == int.MaxValue)
						throw new PXException(GL.Messages.TheDefaultBranchNameCannotBeAssigned);

					currentCD = PXMessages.LocalizeFormatNoPrefix(GL.Messages.NewBranchNameTemplate, currentNumber);
				}

			} while (true);
		}

		#endregion

		protected override int? BaccountIDForNewEmployee()
		{
			if (OrganizationView.Current == null)
				return null;

			string origOrgType = (string)OrganizationView.Cache.GetValueOriginal<Organization.organizationType>(OrganizationView.Current);

			return origOrgType == OrganizationTypes.WithoutBranches
			       && OrganizationView.Current.OrganizationType == origOrgType
				? BAccount.Current.BAccountID
				: null;
		}

		#region CREATE LEDGER

		[Serializable]
		public class LedgerCreateParameters : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[PXInt]
			public virtual int? OrganizationID { get; set; }
			#endregion

			#region LedgerCD
			public abstract class ledgerCD : PX.Data.BQL.BqlString.Field<ledgerCD> { }

			[PXString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA")]
			[PXDefault]
			[PXUIField(DisplayName = "Ledger ID")]
			public virtual string LedgerCD { get; set; }
			#endregion

			#region Descr
			public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

			[PXString(60, IsUnicode = true)]
			[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
			[PXUIField(DisplayName = Messages.Description)]
			public virtual String Descr { get; set; }
			#endregion
		}

		public PXFilter<LedgerCreateParameters> CreateLedgerView;

		#region CreateLedgerSmartPanel
		public PXAction<OrganizationBAccount> createLedger;

		[PXUIField(DisplayName = "Create Ledger", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public virtual IEnumerable CreateLedger(PXAdapter adapter)
		{
			if (OrganizationView.Current == null) return adapter.Get();

			if (CreateLedgerView.AskExtFullyValid(
				(graph, viewName) =>
				{
					CreateLedgerView.Current.OrganizationID = OrganizationView.Current.OrganizationID;
					CreateLedgerView.Current.Descr = String.Format(Messages.ActualLedgerDescription, OrganizationView.Current.OrganizationCD.Trim());
				},
				DialogAnswerType.Positive))
			{
				Save.Press();
				CreateLeadgerProc(CreateLedgerView.Current);
				throw new PXRefreshException();
			}

			return adapter.Get();
		}
		#endregion

		protected virtual void LedgerCreateParameters_LedgerCD_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Ledger ledger = PXSelectReadonly<Ledger, Where<Ledger.ledgerCD, Equal<Required<Ledger.ledgerCD>>>>.Select(this, e.NewValue);
			if (ledger != null)
			{
				throw new PXSetPropertyException(GL.Messages.LedgerAlreadyExists, e.NewValue);
			}
		}

		public static void CreateLeadgerProc(LedgerCreateParameters ledgerParamters)
		{
			GeneralLedgerMaint ledgerMaint;
			ledgerMaint = CreateInstance<GeneralLedgerMaint>();

			ledgerMaint.LedgerRecords.Insert(new Ledger()
			{
				LedgerCD = ledgerParamters.LedgerCD,
				BalanceType = LedgerBalanceType.Actual,
				Descr = ledgerParamters.Descr
			});

			ledgerMaint.Caches<OrganizationLedgerLink>().Insert(new OrganizationLedgerLink()
			{
				OrganizationID = ledgerParamters.OrganizationID
			});

			ledgerMaint.Save.Press();
		}

		#endregion
	}
}
