using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.DAC;
using CRLocation = PX.Objects.CR.Standalone.Location;

namespace PX.Objects.CS
{
	public interface IActionsMenuGraph
	{
		PXAction ActionsMenuItem { get; }
	}

	public abstract class OrganizationUnitMaintBase<TOrgUnit, WhereClause> : PXGraph<OrganizationUnitMaintBase<TOrgUnit, WhereClause>, TOrgUnit>, IActionsMenuGraph
		where TOrgUnit : BAccount, new()
		where WhereClause : class, IBqlWhere, new()
	{
		#region Views

		[PXViewName(CR.Messages.BAccount)]
		public PXSelect<
				TOrgUnit,
			Where2<
				Match<Current<AccessInfo.userName>>,
				And<WhereClause>>>
			BAccount;

		public virtual PXSelectBase<TOrgUnit> BAccountAccessor => BAccount;

		[PXHidden]
		public PXSelect<CRLocation>
			BaseLocations;

		[PXHidden]
		public PXSelect<Address>
			AddressDummy;

		[PXHidden]
		public PXSelect<Contact>
			ContactDummy;

		public PXSelect<BAccountItself, Where<BAccount.bAccountID, Equal<Optional<BAccount.bAccountID>>>> CurrentBAccountItself;

		public PXSetup<GL.Company> cmpany;

		public abstract PXSelectBase<EPEmployee> EmployeesAccessor { get; }

		public PXSelect<PX.SM.UploadFile> Files;
		public PXSelect<GLSetup> glsetup;
		public PXSelect<Company> Company;

		#endregion

		#region Actions

		public PXMenuAction<TOrgUnit> ActionsMenu;
		public PXAction ActionsMenuItem => ActionsMenu;
		public PXAction<TOrgUnit> newContact;

		#endregion

		protected OrganizationUnitMaintBase()
		{
			EmployeesAccessor.Cache.AllowInsert = false;
			EmployeesAccessor.Cache.AllowDelete = false;
			EmployeesAccessor.Cache.AllowUpdate = false;
			PXUIFieldAttribute.SetDisplayName<Contact.salutation>(Caches[typeof(Contact)], CR.Messages.Attention);

			PXUIFieldAttribute.SetEnabled<Contact.fullName>(Caches[typeof(Contact)], null);

			bool branchFeatureEnabled = PXAccess.FeatureInstalled<FeaturesSet.branch>();

			Next.SetVisible(branchFeatureEnabled);
			Previous.SetVisible(branchFeatureEnabled);
			Last.SetVisible(branchFeatureEnabled);
			First.SetVisible(branchFeatureEnabled);
			Insert.SetVisible(branchFeatureEnabled);
		}

		#region Button Delegates

		[PXUIField(DisplayName = CS.Messages.ViewEmployee, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		public new IEnumerable ViewContact(PXAdapter adapter)
		{
			if (this.EmployeesAccessor.Current != null && this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				EPEmployee current = this.EmployeesAccessor.Current;
				EmployeeMaint graph = PXGraph.CreateInstance<EmployeeMaint>();
				if ((graph.Employee.Current = graph.Employee.Search<EPEmployee.bAccountID>(current.BAccountID)) == null)
				{
					throw new PXSetPropertyException(Messages.YouDoNotHaveSufficientAccessRightsToViewOrModifyAnEmployee,
														PXUIFieldAttribute.GetItemName(BAccount.Cache), 
														BAccountAccessor.Current.AcctCD);
				}
				throw new PXRedirectRequiredException(graph, CR.Messages.ContactMaint);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = CS.Messages.NewEmployee)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		public IEnumerable NewContact(PXAdapter adapter)
		{
			if (this.BAccountAccessor.Cache.GetStatus(this.BAccountAccessor.Current) != PXEntryStatus.Inserted)
			{
				EmployeeMaint graph = CreateInstance<EmployeeMaint>();
				try
				{
					int? ParentBAccountID = BaccountIDForNewEmployee();
					graph.Employee.Insert(new EPEmployee
					{
						RouteEmails = true,
						ParentBAccountID = ParentBAccountID
					});
					graph.Employee.Cache.IsDirty = false;

					graph.Caches<RedirectEmployeeParameters>().Insert(new RedirectEmployeeParameters
					{
						RouteEmails = true,
						ParentBAccountID = ParentBAccountID
					});
				}
				catch (PXFieldProcessingException ex)
				{
					if (graph.Employee.Cache.GetBqlField(ex.FieldName) == typeof(EPEmployee.parentBAccountID))
					{
						throw new PXSetPropertyException(Messages.YouDoNotHaveSufficientAccessRightsToViewOrModifyAnEmployee,
															PXUIFieldAttribute.GetItemName(BAccount.Cache),
															BAccountAccessor.Current.AcctCD);
					}
					throw;
				}

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected virtual IEnumerable cancel(PXAdapter a)
		{
			foreach (TOrgUnit r in (new PXCancel<TOrgUnit>(this, "Cancel")).Press(a))
			{
				if (r != null && BAccount.Cache.GetStatus(r) == PXEntryStatus.Inserted)
				{
					BAccountItself acct = PXSelectReadonly<BAccountItself, Where<BAccountItself.acctCD, Equal<Required<BAccountItself.acctCD>>>>.Select(this, r.AcctCD);
					if (acct != null && acct.BAccountID != r.BAccountID)
					{
						BAccount.Cache.RaiseExceptionHandling<BAccount.acctCD>(r, r.AcctCD,
							new PXSetPropertyException(Messages.ItemExistsReenter));
					}

				}
				yield return r;
			}
		}

		#endregion

		#region Cache Attached

		[PXUIField(DisplayName = "Name", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[ContactDisplayName(typeof(Contact.lastName), typeof(Contact.firstName), typeof(Contact.midName), typeof(Contact.title), true)]
		protected virtual void Contact_DisplayName_CacheAttached(PXCache sender)
		{

		}

		#endregion

		#region Events
		protected virtual void _(Events.RowPersisted<TOrgUnit> e)
		{
			if (e.TranStatus == PXTranStatus.Open && (e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete)
			{
				PXUpdate<
						Set<BAccountR.cOrgBAccountID, Zero>, 
						BAccountR,
						Where <BAccountR.cOrgBAccountID, Equal<Required<BAccountR.cOrgBAccountID>>>>
					.Update(this, e.Row.BAccountID);
				PXUpdate<
						Set<CustomerClass.orgBAccountID, Zero>,
						CustomerClass,
						Where<CustomerClass.orgBAccountID, Equal<Required<BAccount.cOrgBAccountID>>>>
					.Update(this, e.Row.BAccountID);
			}
		}
		#endregion
		protected abstract int? BaccountIDForNewEmployee();

		protected void ClearRoleNameInBranches()
		{
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
		}

		protected void RefreshBranch()
		{
			var currentBranchId = PXContext.GetBranchID();
			PXAccess.ResetBranchSlot();
			if (!PXAccess.GetBranches().Where(b => b.Deleted == false && b.Id == currentBranchId).Any())
			{
				PXLogin.SetBranchID(CreateInstance<PX.SM.SMAccessPersonalMaint>().GetDefaultBranchId());
			}
		}
	}
}
