using System.Collections.Generic;
using PX.SM;
using PX.Data;
using System.Collections;
using System.Linq;
using PX.Data.Services;
using Microsoft.Practices.ServiceLocation;

namespace PX.Objects.EP
{
    public class EPLoginTypeMaint : PXGraph<EPLoginTypeMaint, EPLoginType, EPLoginType.loginTypeName>
    {
		#region Selects

		public PXSelect<EPLoginType> LoginType;
        public PXSelect<EPLoginType, Where<EPLoginType.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> CurrentLoginType;

        public PXSelectJoin<EPLoginTypeAllowsRole, InnerJoin<Roles, On<Roles.rolename, Equal<EPLoginTypeAllowsRole.rolename>>>,
            Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> AllowedRoles;

		public PXSelect<Users, Where<Users.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> Users;

		public PXSelectJoin<EPManagedLoginType,
                        InnerJoin<EPLoginType, On<EPLoginType.loginTypeID, Equal<EPManagedLoginType.loginTypeID>>>,
                        Where<EPManagedLoginType.parentLoginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> ManagedLoginTypes;

	    public PXSelectUsersInRoles UserRoles;

		public PXSelectJoin<UsersInRoles, LeftJoin<Users, On<UsersInRoles.username, Equal<Users.username>>>,
									Where<Users.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>,
									And<UsersInRoles.rolename, In<Required<UsersInRoles.rolename>>>>> assignedRoles;

		public PXSelectJoin<EPLoginTypeAllowsRole, InnerJoin<Roles, On<Roles.rolename, Equal<EPLoginTypeAllowsRole.rolename>>>,
			Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>,
				And<EPLoginTypeAllowsRole.isDefault, Equal<True>>>> DefaultAllowedRoles;

		public PXSelectJoin<EPLoginTypeAllowsRole, InnerJoin<Roles, On<Roles.rolename, Equal<EPLoginTypeAllowsRole.rolename>>>,
			Where<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>,
				And<EPLoginTypeAllowsRole.isDefault, Equal<False>>>> NonDefaultAllowedRoles;

		public PXSelect<Users, Where<Users.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>> defaultedUsers;
		#endregion

		#region Actions
		public PXAction<EPLoginType> UpdateUsers;
		[PXButton]
		[PXUIField(DisplayName = "Apply to Users")]
		protected virtual void updateUsers()
		{
			var defaultRoles = DefaultAllowedRoles.Select().RowCast<Roles>();
			var nonDefaultRoles = NonDefaultAllowedRoles.Select().RowCast<Roles>();
			if ((defaultRoles == null || !defaultRoles.Any()) && (nonDefaultRoles == null || !nonDefaultRoles.Any()))
				return;

			this.Persist();
			PXLongOperation.StartOperation(this, delegate ()
			{
				var graph = PXGraph.CreateInstance<EPLoginTypeMaint>();
				graph.LoginType.Current = this.LoginType.Current;
				var users = graph.defaultedUsers.Select().AsEnumerable();

				if (defaultRoles != null && defaultRoles.Any())
				{
					var roleNames = defaultRoles.ToList().Select(x => x.Rolename).ToArray(); ;
					var assigned = graph.assignedRoles.Select(new object[] { roleNames }).RowCast<UsersInRoles>();
					var assignedDict = assigned.GroupBy(x => x.Rolename).ToDictionary(x => x.Key, v => v.ToList());

					foreach (Roles role in defaultRoles)
					{
						var userRoles = assignedDict.ContainsKey(role.Rolename) ? assignedDict[role.Rolename] : new List<UsersInRoles>();

						var emptyUsers = users.Where(x => !userRoles.Select(u => u.Username.ToUpper()).Contains(((Users)x).Username.ToUpper()));
						foreach (Users user in emptyUsers)
							graph.Caches[typeof(UsersInRoles)].Insert(new UsersInRoles { Rolename = role.Rolename, Username = user.Username, ApplicationName = "/" });
					}
				}

				if (nonDefaultRoles != null && nonDefaultRoles.Any())
				{
					var roleNames = nonDefaultRoles.ToList().Select(x => x.Rolename).ToArray(); ;
					var assigned = graph.assignedRoles.Select(new object[] { roleNames }).RowCast<UsersInRoles>();
					var assignedDict = assigned.GroupBy(x => x.Rolename).ToDictionary(x => x.Key, v => v.ToList());

					foreach (Roles role in nonDefaultRoles)
					{
						var userRoles = assignedDict.ContainsKey(role.Rolename) ? assignedDict[role.Rolename] : new List<UsersInRoles>();

						var usersWithNonDefaultedRole = users.Where(x => userRoles.Select(u => u.Username.ToUpper()).Contains(((Users)x).Username.ToUpper()));
						foreach (Users user in usersWithNonDefaultedRole)
						{
							var userInRole = PXSelect<UsersInRoles,
								Where<UsersInRoles.username, Equal<Required<UsersInRoles.username>>,
									And<UsersInRoles.rolename, Equal<Required<UsersInRoles.rolename>>,
									And<UsersInRoles.applicationName, Equal<Required<UsersInRoles.applicationName>>>>>>
									.Select(graph, user.Username, role.Rolename, "/")
									.RowCast<UsersInRoles>()
									.FirstOrDefault();
							if (userInRole != null)
								graph.Caches[typeof(UsersInRoles)].Delete(userInRole);
						}
					}
				}
				graph.Persist();
			});
		}
		#endregion

		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXSelector(typeof(Search<Users.username>),
								DescriptionField = typeof(Users.comment), DirtyRead = true)]
		protected virtual void UsersInRoles_Username_CacheAttached(PXCache sender)
		{
		}

		protected virtual void EPLoginType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPLoginType ut = (EPLoginType) e.Row;
			ManagedLoginTypes.Cache.AllowInsert = ut != null && ut.Entity == EPLoginType.entity.Contact;
			ManagedLoginTypes.Cache.AllowUpdate = ut != null && ut.Entity == EPLoginType.entity.Contact;
		}

        protected virtual void EPLoginType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            EPLoginType row = (EPLoginType) e.Row;
            if (row == null) return;

            Users u = PXSelect<Users,
                    Where<Users.loginTypeID, Equal<Required<EPLoginType.loginTypeID>>>>.
                    SelectWindowed(this, 0, 1, row.LoginTypeID);
            if (u != null)
            {
                throw new PXException(Messages.RecordIsReferenced);
            }
        }

		protected virtual void EPLoginType_Entity_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			string newVal = (string) e.NewValue;
			EPLoginTypeAllowsRole allowed = PXSelectJoin<EPLoginTypeAllowsRole, LeftJoin<Roles, On<EPLoginTypeAllowsRole.rolename, Equal<Roles.rolename>>>, Where<Roles.guest, NotEqual<True>, And<EPLoginTypeAllowsRole.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>>>.Select(this);
			if (newVal == EPLoginType.entity.Contact && allowed != null)
			{
				throw new PXSetPropertyException(Messages.CantLoginTypeEntityChange );
			}
		}

	    public override void Persist()
	    {
			List<EPLoginTypeAllowsRole> allowed = new List<EPLoginTypeAllowsRole>(AllowedRoles.Select().RowCast<EPLoginTypeAllowsRole>());
			List<UsersInRoles> assigned = new List<UsersInRoles>(PXSelectJoin<UsersInRoles, LeftJoin<Users, On<UsersInRoles.username, Equal<Users.username>>>, Where<Users.loginTypeID, Equal<Current<EPLoginType.loginTypeID>>>>.Select(this).RowCast<UsersInRoles>());

		    assigned.RemoveAll(ur => allowed.Exists(ar => ar.Rolename == ur.Rolename));

            List<string> assignedRoles = assigned
                                         .Select(a => a.Rolename)
                                         .ToList();
            IUserService userService = ServiceLocator.Current.GetInstance<IUserService>();
            IEnumerable<string> allowedRoles = userService.FilterRoles(assignedRoles);
            assigned.RemoveAll(a => !allowedRoles.Contains(a.Rolename));

			if (assigned.Count > 0 && AllowedRoles.View.Ask(null, null, string.Empty, Messages.ConfirmDeleteNotAllowedRoles, MessageButtons.YesNo, MessageIcon.Warning) != WebDialogResult.Yes)
			{
				return;
			}

		    foreach (UsersInRoles role in assigned)
		    {
			    UserRoles.Delete(role);
		    }

		    base.Persist();
	    }
    }
}