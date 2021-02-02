using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.EP;
using PX.Objects.CR;
using PX.SM;
using PX.TM;

namespace PX.Objects.CS
{
	public class EMailSyncPolicyMaintExt : PXGraphExtension<EMailSyncPolicyMaint>
	{
		public PXSelect<CRContactClass,
			Where<CRContactClass.classID, Equal<Current<EMailSyncPolicy.contactsClass>>>> ContactClass;

		public override void Initialize()
		{
			Base.Preferences.Cache.AllowSelect = true;
        }

		protected virtual void EMailSyncAccountPreferences_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as EMailSyncAccountPreferences;
			if (row == null) return;

			if (row.EmployeeID != null)
			{
				foreach (PXResult<EPEmployee, Contact> result in PXSelectJoin<EPEmployee,
					InnerJoin<Contact, On<EPEmployee.defContactID, Equal<Contact.contactID>, And<EPEmployee.parentBAccountID, Equal<Contact.bAccountID>>>>,
					Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>.Select(Base, row.EmployeeID))
				{
					row.EmployeeCD = ((EPEmployee)result).AcctName;
					row.Address = ((Contact)result).EMail;
				}
			}
		}

		protected virtual void EMailSyncPolicy_ContactsClass_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated sel)
		{
			if (sel != null)
				sel(cache, e);

			EMailSyncPolicy row = e.Row as EMailSyncPolicy;

			CRContactClass cClass = ContactClass.SelectSingle();

			if (cClass != null && !String.IsNullOrWhiteSpace(cClass.DefaultOwner))
			{
				cache.RaiseExceptionHandling<EMailSyncPolicy.contactsClass>(row, row.ContactsClass, new PXSetPropertyException(Messages.EmailExchangeContactClassWarning, PXErrorLevel.Warning));
			}
		}

		protected virtual void EMailSyncPolicy_ContactsFilter_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e, PXFieldUpdated sel)
		{
			if (sel != null)
				sel(cache, e);

			EMailSyncPolicy row = e.Row as EMailSyncPolicy;

			cache.RaiseFieldUpdated<EMailSyncPolicy.contactsClass>(row, row.ContactsClass);
		}
	}

	public class EMailSyncAccountPreferencesExt : PXCacheExtension<EMailSyncAccountPreferences>
	{
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Employee ID", Visibility = PXUIVisibility.SelectorVisible)]
		//[PXSubordinateSelector()]
		[PXEmployeeSingleSelectorAttribute]
		public virtual Int32? EmployeeID { get; set; }
		#endregion
		#region EmployeeCD
		[PXUIField(DisplayName = "Employee Name", Visibility = PXUIVisibility.SelectorVisible, IsReadOnly = true)]
		[PXDBScalar(typeof(Search<EPEmployee.acctName, Where<EPEmployee.bAccountID, Equal<employeeID>>>))]
		public virtual String EmployeeCD { get; set; }
		#endregion
	}
	public class EMailSyncPolicyExt : PXCacheExtension<EMailSyncPolicy>
	{
		#region ContactsClass
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Contact Class")]
		[PXSelector(typeof(CRContactClass.classID), DescriptionField = typeof(CRContactClass.description), CacheGlobal = true)]
		public virtual String ContactsClass { get; set; }
		#endregion
	}
	public class PXEmployeeSingleSelectorAttribute : PXAggregateAttribute
	{
		public Type DescriptionField
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().DescriptionField;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().DescriptionField = value;
			}
		}
		public Type SubstituteKey
		{
			get
			{
				return this.GetAttribute<PXSelectorAttribute>().SubstituteKey;
			}
			set
			{
				this.GetAttribute<PXSelectorAttribute>().SubstituteKey = value;
			}
		}

		public PXEmployeeSingleSelectorAttribute()
		{
			PXDimensionAttribute attr = new PXDimensionAttribute("BIZACCT");
			attr.ValidComboRequired = true;
			_Attributes.Add(attr);

			PXSelectorAttribute selattr = new PXSelectorAttribute(GetCommand(),
				typeof(EPEmployee.acctCD),
				typeof(EPEmployee.bAccountID), typeof(EPEmployee.acctName),
				typeof(EPEmployee.classID), typeof(EPEmployeePosition.positionID), typeof(EPEmployee.departmentID),
				typeof(EPEmployee.defLocationID), typeof(Users.username), typeof(Users.displayName));
			selattr.SubstituteKey = typeof(EPEmployee.acctCD);
			selattr.DescriptionField = typeof(EPEmployee.acctName);

			_Attributes.Add(selattr);
			_Attributes.Add(new PXRestrictorAttribute(typeof(Where<EPEmployee.status, NotEqual<BAccount.status.inactive>>), Objects.EP.Messages.InactiveEpmloyee, typeof(EPEmployee.acctCD), typeof(EPEmployee.status)));
		}

		private static Type GetCommand()
		{
			var whereType = typeof(Where<EMailSyncAccountPreferences.policyName, IsNull>);

			return BqlCommand.Compose(typeof(Search5<,,,>), typeof(EPEmployee.bAccountID),
				typeof(LeftJoin<Users, On<Users.pKID, Equal<EPEmployee.userID>>,
					LeftJoin<EMailSyncAccountPreferences, On<EMailSyncAccountPreferences.employeeID, Equal<EPEmployee.bAccountID>, 
						And<EMailSyncAccountPreferences.employeeID, NotEqual<Optional<EMailSyncAccountPreferences.employeeID>>>>,
					LeftJoin<EPEmployeePosition, On<EPEmployeePosition.employeeID, Equal<EPEmployee.bAccountID>, And<EPEmployeePosition.isActive, Equal<True>>>>>>),
				whereType,
				typeof(Aggregate<GroupBy<EPEmployee.acctCD>>));
		}
		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			sender.Graph.FieldUpdating.RemoveHandler(sender.GetItemType(), _FieldName, this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating);
			sender.Graph.FieldUpdating.AddHandler(sender.GetItemType(), _FieldName, FieldUpdating);
		}

		protected virtual void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			EPEmployee employee = PXSelect<EPEmployee, Where<EPEmployee.acctCD, Equal<Required<EPEmployee.acctCD>>>>
				.SelectWindowed(sender.Graph, 0, 1, e.NewValue);
			if (employee != null)
			{
				e.NewValue = employee.BAccountID;
				e.Cancel = true;
			}
			else
			{
				PXFieldUpdating fu = this.GetAttribute<PXDimensionAttribute>().FieldUpdating;
				fu(sender, e);
				e.Cancel = false;

				fu = this.GetAttribute<PXSelectorAttribute>().SubstituteKeyFieldUpdating;
				fu(sender, e);

			}
		}
		public override void GetSubscriber<ISubscriber>(List<ISubscriber> subscribers)
		{
			if (typeof(ISubscriber) != typeof(IPXFieldUpdatingSubscriber) &&
				  typeof(ISubscriber) != typeof(IPXRowPersistingSubscriber) &&
					typeof(ISubscriber) != typeof(IPXFieldDefaultingSubscriber))
			{
				base.GetSubscriber<ISubscriber>(subscribers);
			}
		}
	}
}
