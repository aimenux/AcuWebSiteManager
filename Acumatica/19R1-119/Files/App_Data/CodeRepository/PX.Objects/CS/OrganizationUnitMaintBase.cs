using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.CS
{
	public abstract class OrganizationUnitMaintBase<TOrgUnit, WhereClause> : BusinessAccountGraphBase<TOrgUnit, TOrgUnit, WhereClause>
		where TOrgUnit : BAccount, new()
		where WhereClause : class, IBqlWhere, new()
	{
		#region Views

		public abstract PXSelectBase<EPEmployee> EmployeesAccessor { get; }

		public PXSelect<PX.SM.UploadFile> Files;
		public PXSelect<GLSetup> glsetup;
		public PXSelect<Company> Company;

		#endregion

		#region Actions

		public PXMenuAction<TOrgUnit> ActionsMenu;

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
			Prev.SetVisible(branchFeatureEnabled);
			Last.SetVisible(branchFeatureEnabled);
			First.SetVisible(branchFeatureEnabled);
			Insert.SetVisible(branchFeatureEnabled);
		}

		#region Buttons

		[PXUIField(DisplayName = Messages.ValidateAddresses, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton]
		public new virtual IEnumerable ValidateAddresses(PXAdapter adapter)
		{
			BAccount bacct = this.BAccount.Current;
			if (bacct != null)
			{
				Address address = this.DefAddress.Current;
				if (address != null && address.IsValidated == false)
				{
					PXAddressValidator.Validate<Address>(this, address, true);
				}
				LocationExtAddress locAddress = this.DefLocation.Current;
				if (locAddress != null && locAddress.IsValidated == false && locAddress.AddressID != address.AddressID)
				{
					PXAddressValidator.Validate<LocationExtAddress>(this, locAddress, true);
				}

			}
			return adapter.Get();
		}
		#endregion

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
		public new IEnumerable NewContact(PXAdapter adapter)
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

		[PXUIField(DisplayName = CR.Messages.SetDefault, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton()]
		public new IEnumerable SetDefault(PXAdapter adapter)
		{
			TOrgUnit acct = this.BAccount.Current;
			if (Locations.Current != null && acct != null && Locations.Current.LocationID != acct.DefLocationID)
			{
				acct.DefLocationID = Locations.Current.LocationID;
				this.BAccount.Update(acct);
			}
			return adapter.Get();
		}

		[PXCancelButton]
		[PXUIField(MapEnableRights = PXCacheRights.Select)]
		protected override IEnumerable Cancel(PXAdapter a)
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

		protected abstract int? BaccountIDForNewEmployee();
	}
}
