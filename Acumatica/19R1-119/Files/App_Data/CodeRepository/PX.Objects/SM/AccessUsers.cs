using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.TM;
using PX.Data.Access;
using PX.Data.Services;
using Microsoft.Practices.ServiceLocation;
using PX.Data.MultiFactorAuth;

namespace PX.SM
{
	public class PXSelectAllowedRoles:PXSelectBase<EPLoginTypeAllowsRole>
	{
		bool isLoginTypeUpdated;

		public PXSelectAllowedRoles(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false, new Select<EPLoginTypeAllowsRole, Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<Users.loginTypeID>>>>(), new PXSelectDelegate(ViewDelegate));

			_Graph.FieldVerifying.AddHandler(typeof(EPLoginTypeAllowsRole), typeof(EPLoginTypeAllowsRole.selected).Name, SelectedFieldVerifying);
			_Graph.FieldUpdated.AddHandler(typeof(EPLoginTypeAllowsRole), typeof(EPLoginTypeAllowsRole.selected).Name, SelectedFieldUpdated);
			_Graph.FieldUpdated.AddHandler(typeof(Users), typeof(Users.loginTypeID).Name, UsersLoginTypeIDFieldUpdated);
			_Graph.RowPersisting.AddHandler(typeof(EPLoginTypeAllowsRole), RowPersisting);
			_Graph.RowPersisted.AddHandler(typeof(Users), UsersRowPersisted);
			_Graph.RowSelected.AddHandler(typeof(Users), UsersRowSelected);
		}

		protected virtual IEnumerable ViewDelegate()
		{
			PXCache userCache = _Graph.Caches[typeof (Users)];
			Users user = (Users) userCache.Current;
			if (user == null || user.Username == null) yield break;

			if (user.Source != PXUsersSourceListAttribute.ActiveDirectory || user.OverrideADRoles == true) // editable roles for native users
			{
				bool IsUserInserted = userCache.GetStatus(user) == PXEntryStatus.Inserted;

				Dictionary<string, UsersInRoles> assigned = PXSelect<UsersInRoles, Where<UsersInRoles.username, Equal<Current<Users.username>>>>.Select(_Graph).RowCast<UsersInRoles>().ToDictionary(ur => ur.Rolename);
				Dictionary<string, EPLoginTypeAllowsRole> allowed = new Dictionary<string, EPLoginTypeAllowsRole>();
				if (user.LoginTypeID == null) // all roles
				{
					foreach (EPLoginTypeAllowsRole arole in
						PXSelect<Roles, Where<Roles.guest, Equal<Current<Users.guest>>, Or<Current<Users.guest>, Equal<False>>>>.Select(_Graph).RowCast<Roles>()
						.Select(r => new EPLoginTypeAllowsRole { Rolename = r.Rolename, IsDefault = false}))
					{
						allowed.Add(arole.Rolename, arole);
						Insert(arole);
						Cache.IsDirty = false;
					}
				}
				else // from appropriate EPLoginTypeAllowsRole
				{
					allowed = PXSelectJoin<EPLoginTypeAllowsRole, InnerJoin<Roles, On<EPLoginTypeAllowsRole.rolename, Equal<Roles.rolename>>>, 
						Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<Users.loginTypeID>>>>.Select(_Graph).RowCast<EPLoginTypeAllowsRole>().ToDictionary(ar => ar.Rolename);
				}

				HashSet<string> all = new HashSet<string>(assigned.Keys);
				all.UnionWith(allowed.Keys);

                IUserService userService = ServiceLocator.Current.GetInstance<IUserService>();
                IEnumerable<string> allowedRoles = userService.FilterRoles(all);

				foreach (string rolename in allowedRoles.Where(PXAccess.IsRoleEnabled))
				{
					EPLoginTypeAllowsRole role;
					allowed.TryGetValue(rolename, out role);

					UsersInRoles urole;
					assigned.TryGetValue(rolename, out urole);

					if (urole != null && role != null)
					{
						role.Selected = true;
						yield return role;
					}
					else if (urole == null && role != null && role.IsDefault == true && (isLoginTypeUpdated || IsUserInserted)) // add user role
					{
						_Graph.Caches[typeof(UsersInRoles)].Insert(new UsersInRoles { Rolename = role.Rolename });
						role.Selected = true;
						yield return role;
					}
					else if (urole != null) // delete user role
					{
						_Graph.Caches[typeof(UsersInRoles)].Delete(urole);
					}
					else if (role != null)
					{
						role.Selected = false;
						yield return role;
					}
				}
			}
			else // readonly mapped roles for AD users
			{
				foreach (string role in (PXUsersSelectorAttribute.GetADMappedRolesBySID(user.ExtRef) ?? new string[0]))
				{
					yield return new EPLoginTypeAllowsRole {Rolename = role, Selected = true};
				}
			}
		}

		protected virtual void SelectedFieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool)e.NewValue == true ||
				PXAccess.GetUserName() != ((Users)_Graph.Caches[typeof(Users)].Current).Username)
				return;

			EPLoginTypeAllowsRole role = (EPLoginTypeAllowsRole)e.Row;

			if(Access.WillSelfLock(sender, role.Rolename))
				if (Ask(Messages.Warning, Messages.UserLockingHimselfOut,
					MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
				{
					e.NewValue = true;
					e.Cancel = true;
				}
		}

		protected virtual void SelectedFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPLoginTypeAllowsRole role = (EPLoginTypeAllowsRole) e.Row;
			UsersInRoles urole = PXSelect<UsersInRoles, 
				Where<UsersInRoles.rolename, Equal<Required<UsersInRoles.rolename>>,
					And<UsersInRoles.username, Equal<Required<UsersInRoles.username>>>>>.Select(sender.Graph, role.Rolename, ((Users)_Graph.Caches[typeof(Users)].Current).Username);
			if (role.Selected == true && urole == null) // add user role
			{
				sender.Graph.Caches[typeof(UsersInRoles)].Insert(new UsersInRoles { Rolename = role.Rolename });
			}
			if (role.Selected != true && urole != null) // delete user role
			{
				sender.Graph.Caches[typeof(UsersInRoles)].Delete(urole);
			}
		}

		protected virtual void UsersLoginTypeIDFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			isLoginTypeUpdated = true;
		}

		protected virtual void UsersRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.selected>(_Graph.Caches[typeof(EPLoginTypeAllowsRole)], null, user.Source == PXUsersSourceListAttribute.Application || user.OverrideADRoles == true);
			PXUIFieldAttribute.SetEnabled<EPLoginTypeAllowsRole.rolename>(_Graph.Caches[typeof(EPLoginTypeAllowsRole)], null, false);
		}

		protected virtual void UsersRowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			isLoginTypeUpdated = false;
		}

		protected virtual void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}
	}

	[PXPrimaryGraph(typeof(Users))]
	public class AccessUsers : Access
	{
        [InjectDependency]
	    protected IMultiFactorService _multiFactorService { get; set; }
		public AccessUsers()
		{
			Cancel.SetVisible(false);
			Save.SetVisible(false);
			if (ActiveDirectoryProvider.Instance == ActiveDirectoryProvider.Empty) // hide "Add Active Directory User" button if AD is not enabled
				AddADUser.SetVisible(false);

			int adLimit = PX.Common.WebConfig.GetInt(_ADGroupCacheLimit, 100);
			bool shouldCache = ActiveDirectoryProvider.Instance.GetUsers().Count() > adLimit;
			this.ReloadADUsers.SetVisible(shouldCache);
			foreach (var attr in this.GetAttributes("ADUser", "Username"))
				if (attr is PXADUsersSelectorAttribute)
					((PXADUsersSelectorAttribute)attr).UseCached = shouldCache;
		}

		public new PXCancel<Users> Cancel;
		public new PXSave<Users> Save;

		public PXSelect<Contact, Where<Contact.contactID, Equal<Current<Users.contactID>>>> contact;
	    public PXSelect<PreferencesSecurity> Preferences;
		public PXSelectAllowedRoles AllowedRoles;

		[PXHidden]
		public PXSelectJoin<EPEmployee, LeftJoin<Contact,
				On<EPEmployee.defContactID, Equal<Contact.contactID>,
					And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>,
				Where<Contact.contactID, Equal<Optional<Contact.contactID>>>> Employee;


        [PXHidden]
        public PXSelect<EPCompanyTreeMember,
                Where<EPCompanyTreeMember.userID, Equal<Optional<Users.pKID>>>> Members;

		public PXFilter<ADUserFilter> ADUser;
		
		[PXDBInt]
		[PXUIField(DisplayName = "User Type")]
		[PXSelector(typeof(Search<EPLoginType.loginTypeID>), SubstituteKey = typeof(EPLoginType.loginTypeName))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		protected virtual void Users_LoginTypeID_CacheAttached(PXCache sender) { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "First Name")]
		[PXFormula(typeof(Switch<Case<Where<Users.contactID, IsNotNull>, Selector<Users.contactID, Contact.firstName>>, Users.firstName>))]
		[PXPersonalDataField]
		protected virtual void Users_FirstName_CacheAttached(PXCache sender) { }

		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Last Name")]
		[PXFormula(typeof(Switch<Case<Where<Users.contactID, IsNotNull>, Selector<Users.contactID, Contact.lastName>>, Users.lastName>))]
		[PXPersonalDataField]
		protected virtual void Users_LastName_CacheAttached(PXCache sender) { }

		[PXDBEmail]
		[PXUIField(DisplayName = "Email", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.contactID, Contact.eMail>, IsNotNull>, Selector<Users.contactID, Contact.eMail>>, Users.email>))]
		[PXDefault]
		[PXUIRequired(typeof(Where<Users.source, NotEqual<PXUsersSourceListAttribute.activeDirectory>>))]
		[PXPersonalDataField]
		protected virtual void Users_Email_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXFormula(typeof(Selector<Users.loginTypeID, EPLoginType.requireLoginActivation>))]
		protected virtual void Users_IsPendingActivation_CacheAttached(PXCache sender) { }

		[PXDBBool]
		[PXUIField(DisplayName = "Guest Account", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFormula(typeof(IsNull<Selector<Users.loginTypeID, EPLoginType.isExternal>, False>))]
		protected virtual void Users_Guest_CacheAttached(PXCache sender) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXSelector(typeof(Search2<Contact.contactID,
				LeftJoin<Users, On<Contact.userID, Equal<Users.pKID>>,
				LeftJoin<BAccount, On<BAccount.defContactID, Equal<Contact.contactID>>>>,
					Where<Current<Users.guest>, Equal<True>, And<Contact.contactType, Equal<ContactTypesAttribute.person>,
						Or<Current<Users.guest>, NotEqual<True>, And<Contact.contactType, Equal<ContactTypesAttribute.employee>, And<BAccount.bAccountID, IsNotNull>>>>>>),
			typeof(Contact.displayName),
			typeof(Contact.salutation),
			typeof(Contact.fullName),
			typeof(Contact.eMail),
			typeof(Users.username),
			DescriptionField = typeof(Contact.displayName))]
		[PXRestrictor(typeof(Where<Contact.userID, IsNull, Or<Contact.userID, Equal<Current<Users.pKID>>>>), PX.Objects.CR.Messages.ContactWithUser, typeof(Contact.displayName))]
		protected virtual void Users_ContactID_CacheAttached(PXCache sender)
		{
		}

		[PXDBBool]
		[PXUIField(DisplayName = "Force User to Change Password on Next Login")]
		[PXFormula(typeof(Switch<Case<Where<Selector<Users.loginTypeID, EPLoginType.resetPasswordOnLogin>, Equal<True>>, True>, False>))] // guarantee not null
		protected virtual void Users_PasswordChangeOnNextLogin_CacheAttached(PXCache sender) { }

		[PXBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Generate Password")]
		protected virtual void Users_GeneratePassword_CacheAttached(PXCache sender) { }

		[PXDBGuid(IsKey = true)]
		[PXDefault(typeof(Users.pKID))]
		protected virtual void UserPreferences_UserID_CacheAttached(PXCache sender) { }

		protected virtual void Contact_ContactType_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = ContactTypesAttribute.Person;
			EPLoginType t = PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<Users.loginTypeID>>>>.Select(this);
			if (t == null) return;

			switch (t.Entity)
			{
				case EPLoginType.entity.Employee:
					e.NewValue = ContactTypesAttribute.Employee;
					break;
				default:
					e.NewValue = ContactTypesAttribute.Person;
					break;
			}
		}

		protected virtual void Users_ContactID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			Users user = e.Row as Users;
			if (user == null) return;

			Contact c = PXSelect<Contact, Where<Contact.contactID, Equal<Required<Users.contactID>>>>.Select(this, e.NewValue);
			if (c != null && string.IsNullOrEmpty(c.EMail) && user.Source != PXUsersSourceListAttribute.ActiveDirectory)
			{
				throw new PXSetPropertyException(PX.Objects.CR.Messages.ContactWithoutEmail, c.DisplayName);
			}
		}

		protected virtual IEnumerable roleList()
		{
			yield break;
		}

		public PXAction<Users> AddADUser;
		[PXUIField(DisplayName = "Add Active Directory User", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable addADUser(PXAdapter adapter)
		{
			ADUser.AskExt();
			return adapter.Get();
		}

		public PXAction<Users> AddADUserOK;
		[PXUIField(DisplayName = "OK", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable addADUserOK(PXAdapter adapter)
		{
			ADUser.VerifyRequired();
			ADUserFilter filter = ADUser.Current;
			Users aduser = PXUsersSelectorAttribute.GetADUserByName(UserList.Cache, filter.Username);
			if (aduser != null)
			{
                PXActiveDirectorySyncMembershipProvider.CheckAndRenameDeletedADUser(aduser.Username, aduser.ExtRef);
				if (adapter.ImportFlag)
				{
					UserList.Insert(aduser);
				}
				else
				{
					AccessUsers graph = CreateInstance<AccessUsers>();
					graph.UserList.Insert(aduser);
					throw new PXRedirectRequiredException(graph, "New AD User");
				}
			}
			return adapter.Get();
		}

		public PXAction<Users> ReloadADUsers;
		[PXUIField(DisplayName = "Reload AD Users", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXButton]
		public IEnumerable reloadADUsers(PXAdapter adapter)
		{
			ActiveDirectoryProvider.Instance.Reset();
			ActiveDirectoryProvider.Instance.GetUsers();
			return adapter.Get();
		}

        public PXAction<Users> GenerateOneTimeCodes;

	    [PXUIField(DisplayName = "Generate Access Codes", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = true)]
	    [PXButton]
	    public IEnumerable generateOneTimeCodes(PXAdapter adapter)
	    {
	        var currentUser = UserList.Current;
	        if (currentUser == null)
	            return adapter.Get();
	        _multiFactorService.GenerateCodesAndShowReport(currentUser.PKID.Value);
	        return adapter.Get();
	    }

	    protected virtual void Users_MultiFactorOverride_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
	    {
	        var row = e.Row as Users;
            //if there is no security preferences in current company (inherited from parent) we should create it now.
	        if (row?.MultiFactorOverride == true)
	        {
	            var currentpreferences = Preferences.Select().FirstTableItems.FirstOrDefault();
	            Preferences.Update(currentpreferences);
	        }
	    }

		protected virtual void Users_OverrideADRoles_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			Users user = (Users) e.Row;
			bool oldval = user.OverrideADRoles == true;
			bool newval = e.NewValue != null ? Convert.ToBoolean(e.NewValue) : false;

			if (oldval != newval && !newval
				&& user.Source == PXUsersSourceListAttribute.ActiveDirectory
				&& RolesByUser.SelectSingle() != null)
			{
				if (UserList.Ask(PX.Objects.CR.Messages.Confirmation,
                    PXMessages.LocalizeFormatNoPrefixNLA(PX.Objects.CR.Messages.DeleteLocalRoles, user.Username), 
					MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
				{
					e.NewValue = true;
					e.Cancel = true;
				}
				else
				{
					//delete UsersInRoles records if overridead is disabled.
					foreach (UsersInRoles role in RolesByUser.Select())
						RolesByUser.Delete(role);
				}
			}
		}

		protected override void Users_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			base.Users_RowSelected(sender, e);
            GenerateOneTimeCodes.SetVisible(user.MultiFactorType>0);
			AllowedRoles.Cache.AllowInsert = false;
			PXDefaultAttribute.SetPersistingCheck<Users.contactID>(sender, user, (user.Guest == true && !Common.Anonymous.IsAnonymous(user.Username)) ? PXPersistingCheck.Null : PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetWarning<Users.overrideADRoles>(sender, user, user.OverrideADRoles == true ? Messages.IgnoredADRoles : null);
            PXUIFieldAttribute.SetEnabled<Users.multiFactorType>(sender, user, user.MultiFactorOverride==true);

			if(user.ContactID == null)
			{
				user.ContactID = PXSelect<Contact, Where<Contact.userID, Equal<Required<Contact.userID>>>>
					.SelectSingleBound(this, null, user.PKID)
					.RowCast<Contact>()
					.FirstOrDefault()?.ContactID;
			}
		}

        protected virtual void Users_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
        {
            Users oldRow = e.Row as Users;
            Users row = e.NewRow as Users;

			if (row == null || oldRow == null) return;

			if (oldRow.ContactID != row.ContactID)
			{ 
				foreach (PXResult<Contact, EPEmployee> set in PXSelectJoin<Contact,
					LeftJoin<EPEmployee, On<Contact.contactID, Equal<EPEmployee.defContactID>,
					And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>>,
					Where<Contact.userID, Equal<Current<Users.pKID>>>>.SelectMultiBound(this, new object[] { oldRow }))
				{
					var (cont, emp) = set;
					cont.UserID = null;
					contact.Update(cont);

					if (emp != null)
					{
						emp.UserID = null;
						Employee.Update(emp);
					}
				}

				Contact newContact = PXSelect<Contact, Where<Contact.contactID, Equal<Current<Users.contactID>>>>.SelectSingleBound(this, new object[] { row });
				if (newContact != null)
				{
					newContact.UserID = row.PKID;
					contact.Update(newContact);

					EPEmployee emp = PXSelectJoin<EPEmployee,
						InnerJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>,
						And<Contact.bAccountID, Equal<EPEmployee.parentBAccountID>>>>,
						Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(this, newContact.ContactID);
					if (emp != null)
					{
						emp = PXCache<EPEmployee>.CreateCopy(emp);
						emp.UserID = row.PKID;
						Employee.Update(emp);
					}
				}
			}

            if (row.Guest == true && oldRow.Guest != true)
            {
                if (contact.View.Ask(
                    MyMessages.EmployeeContactWouldBeCleared,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
                    e.Cancel = true;
            }
            else if (row.Guest != true && oldRow.Guest == true)
            {
                if (contact.View.Ask(
                    MyMessages.ExternalUserContactWouldBeCleared,
                    MessageButtons.YesNo) != WebDialogResult.Yes)
                    e.Cancel = true;
            }
        }

		protected virtual void Users_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			Users oldRow = e.OldRow as Users;
			Users row = e.Row as Users;
			Contact c = PXSelect<Contact, Where<Contact.userID, Equal<Current<Users.pKID>>>>.SelectSingleBound(this, new object[] { row });

			if (row == null || oldRow == null || row.Guest == oldRow.Guest || c == null) return;

			foreach (EPCompanyTreeMember member in Members.View.SelectMultiBound(new[] { e.Row }))
			{
				Members.Delete(member);
			}
		}

		protected virtual void Users_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			Contact c = (Contact)contact.View.SelectSingleBound(new object[] {e.Row});
			if (c != null)
			{
			c.UserID = null;
			contact.Update(c);

			EPEmployee emp = Employee.Select(c.ContactID);
			if (emp != null)
			{
				emp = PXCache<EPEmployee>.CreateCopy(emp);
				emp.UserID = null;
				Employee.Update(emp);
			}
			}
		    foreach (EPCompanyTreeMember member in Members.View.SelectMultiBound(new[] {e.Row}))
		    {
		        Members.Delete(member);
		    }
		}

		protected virtual void Users_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			Users user = (Users)e.Row;
			if (user == null) return;

			PXResultset<EPAssignmentRoute> res = PXSelectJoin<EPAssignmentRoute, InnerJoin<EPAssignmentMap, On<EPAssignmentRoute.assignmentMapID, Equal<EPAssignmentMap.assignmentMapID>>>, Where<EPAssignmentRoute.ownerID, Equal<Current<Users.pKID>>>>.Select(this, new object[] { user });
			foreach (PXResult<EPAssignmentRoute, EPAssignmentMap> result in res)
			{
				throw new PXSetPropertyException(PX.Objects.EP.Messages.UserParticipateInAssignmentMap, user.Username, ((EPAssignmentMap)result).Name);
			}
		}

		protected virtual void Users_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Users user = (Users) e.Row;
			Contact c = (Contact)contact.View.SelectSingleBound(new object[] { user });
			if (user == null || c == null || user.PKID == c.UserID) return;

			c = (Contact)contact.Cache.CreateCopy(c);
			c.UserID = user.PKID;
			contact.Update(c);

			EPEmployee emp = Employee.Select(c.ContactID);
			if (emp != null)
			{
				emp = PXCache<EPEmployee>.CreateCopy(emp);
				emp.UserID = user.PKID;
				Employee.Update(emp);
			}
		}
        protected override void SendUserNotification(int? accountId, Notification notification)
        {
            var gen = TemplateNotificationGenerator.Create(this.UserList.Current, notification);
            gen.MailAccountId = accountId;
            gen.To = this.UserList.Current.Email;
            gen.Body = gen.Body.Replace("((UserList.Password))", this.UserList.Current.Password);			
            gen.LinkToEntity = true;
            gen.Send();
            
        }
		public override void Persist()
		{
			foreach (Users user in UserList.Cache.Deleted)
			{
				Contact cnt = (Contact)contact.View.SelectSingleBound(new object[] {user});
				if (cnt != null)
				{
					cnt.UserID = null;
					contact.Update(cnt);

					EPEmployee emp = Employee.Select(cnt.ContactID);
					if (emp != null)
					{
						emp = PXCache<EPEmployee>.CreateCopy(emp);
						emp.UserID = null;
						Employee.Update(emp);
					}

				}
			}

			if (UserList.Current != null && UserList.Current.OverrideADRoles != true && UserList.Current.Source == PXUsersSourceListAttribute.ActiveDirectory)
			{
				//notice that this select returns nothing if called on AccessUsers. AccessUsers::roleList() returns empty list;
				foreach (UsersInRoles userrole in RoleList.Select())
				{
					RoleList.Delete(userrole);
				}
			}

			//if no local roles selected - disable override.
			if (UserList.Current != null && UserList.Current.OverrideADRoles == true && UserList.Current.Source == PXUsersSourceListAttribute.ActiveDirectory)
			{
				if (RolesByUser.SelectSingle() == null)
					UserListCurrent.Current.OverrideADRoles = false;
			}

			base.Persist();
		} 
		
		public override void ClearDependencies()
		{
			//nothing to clear
		}       
	}
}