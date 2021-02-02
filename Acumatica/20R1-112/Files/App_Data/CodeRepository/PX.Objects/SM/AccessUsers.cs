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
using PX.Common;
using PX.Data.MultiFactorAuth;
using PX.EP;
using PX.Objects.CS;

namespace PX.SM
{
	[PXPrimaryGraph(typeof(Users))]
	public class AccessUsers : Access
	{
		[InjectDependency]
		public IAdvancedAuthenticationRestrictor AdvancedAuthenticationRestrictor { get; set; }

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

        public PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<Users.loginTypeID>>>> LoginType;

		public PXFilter<ADUserFilter> ADUser;

		protected override IEnumerable identities()
		{
			return base.identities()
				.OfType<UserIdentity>()
				.Where(i => AdvancedAuthenticationRestrictor.IsAllowedProviderName(i.ProviderName));
		}

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

            if (user.Source == PXUsersSourceListAttribute.ActiveDirectory)
            {
                string warning = null;
                if (user.OverrideADRoles == true)
                    warning = Messages.IgnoredADRoles;
                else
                {
                    var roles = PXUsersSelectorAttribute.GetADMappedRolesBySID(user.ExtRef);
                    if (roles == null || roles.Count() == 0)
                        warning = Messages.NoMappedADRoles;
                }
                PXUIFieldAttribute.SetWarning<Users.overrideADRoles>(sender, user, warning);
            }
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

        protected virtual void _(Events.RowSelected<Users> e)
        {
            if (e.Row != null)
            {
                EPLoginType t = LoginType.SelectSingle();
                if (t != null && (t.DisableTwoFactorAuth == true || t.AllowedLoginType == EPLoginType.allowedLoginType.API ))
                {
                    PXUIFieldAttribute.SetEnabled<Users.multiFactorOverride>(e.Cache, e.Row, false);
                    PXUIFieldAttribute.SetEnabled<Users.multiFactorType>(e.Cache, e.Row, false);
                }
                else
                {
                    PXUIFieldAttribute.SetEnabled<Users.multiFactorOverride>(e.Cache, e.Row, true);
                }
            }
        }

        protected virtual void _(Events.FieldSelecting<Users, Users.allowedSessions> e)
        {
            if (e.ReturnValue == null && e.Row != null && e.Row.LoginTypeID.HasValue)
            {
                e.ReturnValue = LoginType.SelectSingle()?.AllowedSessions;
            }
        }

        protected virtual void _(Events.FieldUpdating<Users, Users.allowedSessions> e)
        {
            if (e.NewValue != null && e.Row.LoginTypeID.HasValue && LoginType.SelectSingle()?.AllowedSessions == (int)e.NewValue)
            {
                e.NewValue = null;
            }
        }

        protected virtual void _(Events.FieldUpdated<Users, Users.loginTypeID> e)
        {
            if (e.NewValue != null)
            {
                var loginType = LoginType.SelectSingle();
                //var allowedSessionUserValue = e.Cache.GetValue<Users.allowedSessions>(e.Row);
                if (loginType != null /*&& allowedSessionUserValue != null && (int)allowedSessionUserValue == loginType.AllowedSessions*/)
                {
                    e.Cache.SetValueExt<Users.allowedSessions>(e.Row, null);
                }
            }
        }

        protected virtual void _(Events.FieldVerifying<Users, Users.allowedSessions> e)
        {
            if (e.Row != null && WebConfig.MaximumAllowedSessionsCount != null && e.NewValue != null &&
                (int) e.NewValue > WebConfig.MaximumAllowedSessionsCount.Value)
            {
                throw new PXSetPropertyException(PX.SM.Messages.CantSetAllowedSessions);
            }

            if (e.Row != null )
            {
                var loginType = LoginType.SelectSingle();

                int minValue = 1;
                int maxValue = int.MaxValue;
               // bool nullable = true;
                if (loginType?.AllowedLoginType == EPLoginType.allowedLoginType.UI)
                {
                    minValue = 1;
                    maxValue = Math.Min(WebConfig.MaximumAllowedSessionsCount ?? int.MaxValue,
                        PXLicenseHelper.License.UsersAllowed);                   
                    //nullable = false;
                }
                else if (loginType?.AllowedLoginType == EPLoginType.allowedLoginType.API)
                {
                    minValue = 1;
                    maxValue = Math.Min(WebConfig.MaximumAllowedSessionsCount ?? int.MaxValue,
                        PXLicenseHelper.License.MaxApiUsersAllowed);
                    //nullable = false;
                }
                else 
                {
                    minValue = 1;
                    maxValue = Math.Min(WebConfig.MaximumAllowedSessionsCount ?? int.MaxValue, Math.Max(
                        PXLicenseHelper.License.UsersAllowed,
                        PXLicenseHelper.License.MaxApiUsersAllowed));
                }

                if (maxValue < 3)
                    maxValue = 3;

                //if (!nullable && e.NewValue == null)
                //    throw new PXSetPropertyException(PX.SM.Messages.CantSetAllowedSessions, maxValue, minValue  );

                if (e.NewValue != null && ((int)e.NewValue < minValue || (int)e.NewValue  > maxValue))
                {
                    throw new PXSetPropertyException(PX.SM.Messages.CantSetAllowedSessions, maxValue, minValue  );
                }
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
			PXDatabase.SelectTimeStamp();
			PXPageCacheUtils.InvalidateCachedPages();
		}       
	}
}