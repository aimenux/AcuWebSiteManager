using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PX.Common;
using PX.Common.Collection;
using PX.Common.Mail;
using PX.Data;
using PX.Data.EP;
using PX.Data.Maintenance.GI;
using PX.Data.MassProcess;
using PX.Data.Reports;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CR.MassProcess;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.SM;
using PX.TM;
using PX.Objects.CS;
using PX.Objects.SO;
using ActivityService = PX.Data.EP.ActivityService;
using PX.Reports;
using PX.Reports.Data;
using System.Web.Compilation;
using PX.Api;
using PX.Data.SQLTree;
using FieldValue = PX.Data.MassProcess.FieldValue;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR
{
	#region ActivityContactFilter

	[Serializable]
	[PXHidden]
	public partial class ActivityContactFilter : IBqlTable
	{
		#region ContactID

		public abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }

		[PXInt]
		[PXUIField(DisplayName = "Select Contact")]
		[PXSelector(typeof(Search<Contact.contactID,
			Where<Contact.bAccountID, Equal<Current<BAccount.bAccountID>>,
			And<Contact.contactType, NotEqual<ContactTypesAttribute.bAccountProperty>>>>),
			DescriptionField = typeof(Contact.displayName),
			Filterable = true)]
		public virtual Int32? ContactID { get; set; }

		#endregion

		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		public virtual Guid? NoteID { get; set; }

		#endregion
	}

	#endregion

	#region CRActivityList

	public abstract class CRActivityListBaseAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _command;

		private PXView _view;

		protected string _hostViewName;
		private PXSelectBase _select;

		protected CRActivityListBaseAttribute() { }

		protected CRActivityListBaseAttribute(Type select)
		{
			if (select == null) throw new ArgumentNullException("select");

			if (typeof(IBqlSelect).IsAssignableFrom(select))
			{
				_command = BqlCommand.CreateInstance(select);
			}
			else
			{
				throw new PXArgumentException("@select", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectSelectExpression, select.Name));
			}
		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			Initialize(graph, viewName);

			AttachHandlers(graph);
		}

		private void Initialize(PXGraph graph, string viewName)
		{
			_hostViewName = viewName;
			_select = GetSelectView(graph);

			if (_command != null)
				_view = new PXView(graph, true, _command);
		}

		protected PXSelectBase GraphSelector
		{
			get { return _select; }
		}

		protected abstract void AttachHandlers(PXGraph graph);

		protected static void CorrectView(PXSelectBase select, BqlCommand command)
		{
			var graph = select.View.Graph;
			var newView = new PXView(graph, select.View.IsReadOnly, command);
			var oldView = select.View;
			select.View = newView;
			string viewName;
			if (graph.ViewNames.TryGetValue(oldView, out viewName))
			{
				graph.Views[viewName] = newView;
				graph.ViewNames.Remove(oldView);
				graph.ViewNames[newView] = viewName;
			}
		}

		protected object SelectRecord()
		{
			if (_view == null)
				throw new InvalidOperationException(Messages.CommandNotSpecified);

			var dataRecord = _view.SelectSingle();
			if (dataRecord == null) return null;

			var res = dataRecord as PXResult;
			if (res == null) return dataRecord;

			return res[0];
		}

		protected virtual PXSelectBase GetSelectView(PXGraph graph)
		{
			var selectView = graph.GetType().GetField(_hostViewName).GetValue(graph);
			var selectViewType = selectView.GetType();
			Type typeDefinition = selectViewType;

			while (typeDefinition != typeof(object))
			{
				if (typeDefinition.IsGenericType)
					typeDefinition = typeDefinition.GetGenericTypeDefinition();

				if (typeof(CRActivityList<>).IsAssignableFrom(typeDefinition))
				{
					return (PXSelectBase)selectView;
				}
				else
				{
					typeDefinition = typeDefinition.BaseType;
				}
			}

			var attributeTypeName = GetType().Name;
			throw new PXArgumentException((string)null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.AttributeCanOnlyUsedOnView, attributeTypeName, selectViewType.Name));
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class CRReferenceAttribute : PXViewExtensionAttribute
	{
		private readonly BqlCommand _bAccountCommand;
		private readonly BqlCommand _contactCommand;
		private PXView _bAccountView;
		private PXView _contactView;

		private string BAccountRefFieldName
		{
			get { return BAccountRefField != null ? BAccountRefField.Name : EntityHelper.GetIDField(_bAccountView.CacheGetItemType()); }
		}

		public Type BAccountRefField { get; set; }


		private string ContactRefFieldName
		{
			get { return ContactRefField != null ? ContactRefField.Name : EntityHelper.GetIDField(_contactView.CacheGetItemType()); }
		}

		public Type ContactRefField { get; set; }

		public bool Persistent { get; set; }

		public CRReferenceAttribute(Type bAccountSelect, Type contactSelect = null)
		{
			Persistent = false;

			if (bAccountSelect == null) throw new ArgumentNullException("bAccountSelect");

			if (typeof(IBqlSelect).IsAssignableFrom(bAccountSelect))
			{
				_bAccountCommand = BqlCommand.CreateInstance(bAccountSelect);
			}
			else if (typeof(IBqlField).IsAssignableFrom(bAccountSelect))
			{
				BAccountRefField = bAccountSelect;
			}
			else
			{
				throw new PXArgumentException("sel", PXMessages.LocalizeFormatNoPrefixNLA(Messages.IncorrectSelectExpression, bAccountSelect.Name));
			}

			if (contactSelect != null && typeof(IBqlSelect).IsAssignableFrom(contactSelect))
			{
				_contactCommand = BqlCommand.CreateInstance(contactSelect);
			}
			else if (typeof(IBqlField).IsAssignableFrom(contactSelect))
			{
				ContactRefField = contactSelect;
			}

		}

		public override void ViewCreated(PXGraph graph, string viewName)
		{
			if (_bAccountCommand != null)
				_bAccountView = new PXView(graph, true, _bAccountCommand);

			graph.FieldDefaulting.AddHandler<CRActivity.bAccountID>(BAccountID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRPMTimeActivity.bAccountID>(BAccountID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler<CRSMEmail.bAccountID>(BAccountID_FieldDefaulting);

			if (_contactCommand != null || ContactRefField != null)
			{
				if (_contactCommand != null)
					_contactView = new PXView(graph, true, _contactCommand);

				graph.FieldDefaulting.AddHandler<CRActivity.contactID>(ContactID_FieldDefaulting);
				graph.FieldDefaulting.AddHandler<CRPMTimeActivity.contactID>(ContactID_FieldDefaulting);
				graph.FieldDefaulting.AddHandler<CRSMEmail.contactID>(ContactID_FieldDefaulting);
			}

			if (Persistent)
			{
				graph.Views.Caches.Remove(typeof(CRActivity));
				graph.Views.Caches.Add(typeof(CRActivity));
				graph.RowPersisting.AddHandler(typeof(CRActivity), RowPersisting);

				graph.Views.Caches.Remove(typeof(CRPMTimeActivity));
				graph.Views.Caches.Add(typeof(CRPMTimeActivity));
				graph.RowPersisting.AddHandler(typeof(CRPMTimeActivity), RowPersisting);

				graph.Views.Caches.Remove(typeof(CRSMEmail));
				graph.Views.Caches.Add(typeof(CRSMEmail));
				graph.RowPersisting.AddHandler(typeof(CRSMEmail), RowPersisting);
			}
		}

		private int? GetBAccIDRef(PXCache sender)
		{
			if (_bAccountView != null)
			{
				object record = _bAccountView.SelectSingle();
				return GetRecordValue(sender, record, BAccountRefFieldName);
			}
			else if (BAccountRefField != null)
				return CetCurrentValue(sender, BAccountRefField);
			return null;
		}

		private int? GetContactIDRef(PXCache sender)
		{
			if (_contactView != null)
			{
				object record = _contactView.SelectSingle();
				return GetRecordValue(sender, record, ContactRefFieldName);
			}
			else if (ContactRefFieldName != null)
				return CetCurrentValue(sender, ContactRefField);
			return null;
		}

		private static int? GetRecordValue(PXCache sender, object record, string field)
		{
			return record != null ? (int?)sender.Graph.Caches[record.GetType()].GetValue(record, field) : null;
		}

		private static int? CetCurrentValue(PXCache sender, Type field)
		{
			if (field == null) return null;
			var cache = sender.Graph.Caches[field.DeclaringType];
			return (cache.Current != null)
				? cache.GetValue(cache.Current, field.Name) as int?
				: null;
		}

		private void BAccountID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetBAccIDRef(sender);
		}

		private void ContactID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = GetContactIDRef(sender);
		}

		private void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (_bAccountView != null || BAccountRefField != null)
				sender.SetValue(e.Row, typeof(CRActivity.bAccountID).Name, GetBAccIDRef(_bAccountView?.Cache ?? sender.Graph.Caches[BAccountRefField.DeclaringType]));
			if (_contactView != null || ContactRefField != null)
				sender.SetValue(e.Row, typeof(CRActivity.contactID).Name, GetContactIDRef(_contactView?.Cache ?? sender.Graph.Caches[ContactRefField.DeclaringType]));
		}
	}

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public class CRDefaultMailToAttribute : CRActivityListBaseAttribute
	{
		public interface IEmailMessageTarget
		{
			string DisplayName { get; }
			string Address { get; }
		}

		private readonly bool _takeCurrent;

		public CRDefaultMailToAttribute()
			: base()
		{
			_takeCurrent = true;
		}

		public CRDefaultMailToAttribute(Type select)
			: base(select)
		{
		}

		protected override void AttachHandlers(PXGraph graph)
		{
			graph.RowInserting.AddHandler<CRSMEmail>(RowInserting);
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRSMEmail;
			if (row == null) return;

			string emailAddress = null;

			IEmailMessageTarget record;
			if (_takeCurrent)
			{
				var primaryDAC = ((IActivityList)GraphSelector).PrimaryViewType;
				var primaryCache = sender.Graph.Caches[primaryDAC];
				record = primaryCache.Current as IEmailMessageTarget;
			}
			else
			{
				record = SelectRecord() as IEmailMessageTarget;
			}

			if (record != null)
			{
				var displayName = record.DisplayName.With(_ => _.Trim());
				var addresses = record.Address.With(_ => _.Trim());
				if (!string.IsNullOrEmpty(addresses))
				{
					emailAddress = PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(addresses, displayName);
				}
			}
			row.MailTo = emailAddress;
		}
	}

	public sealed class ProjectTaskActivities : PMActivityList<PMTask>
	{
		public ProjectTaskActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowSelected.AddHandler<PMTask>(RowSelected);
			_Graph.RowInserting.AddHandler<CRPMTimeActivity>(RowInserting);
		}

		private void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PM.PMTask row = (PM.PMTask)e.Row;
			if (row == null || View == null || View.Cache == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMTask.projectID>>>>.Select(_Graph);
			bool userCanAddActivity = true;
			if (project != null && project.RestrictToEmployeeList == true)
			{
				var select = new PXSelectJoin<EPEmployeeContract,
					InnerJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
					Where<EPEmployeeContract.contractID, Equal<Current<PMTask.projectID>>,
					And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>(_Graph);

				EPEmployeeContract record = select.SelectSingle();
				userCanAddActivity = record != null;
			}

			View.Cache.AllowInsert = userCanAddActivity;
		}

		private void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			var row = (CRPMTimeActivity)e.Row;
			if (row == null) return;

			row.ProjectID = ((PMTask)sender.Graph.Caches[typeof(PMTask)].Current).ProjectID;
			row.ProjectTaskID = ((PMTask)sender.Graph.Caches[typeof(PMTask)].Current).TaskID;
		}

		protected override void CreateTimeActivity(PXCache cache, int classId)
		{
			PXCache timeCache = cache.Graph.Caches[typeof(PMTimeActivity)];
			if (timeCache == null) return;

			PMTimeActivity timeActivity = (PMTimeActivity)timeCache.Current;
			if (timeActivity == null) return;

			bool withTimeTracking = classId != CRActivityClass.Task && classId != CRActivityClass.Event;

			timeCache.SetValue<PMTimeActivity.trackTime>(timeActivity, withTimeTracking);
			timeCache.SetValueExt<PMTimeActivity.projectID>(timeActivity, ((PMTask)_Graph.Caches[typeof(PMTask)].Current)?.ProjectID);
			timeCache.SetValueExt<PMTimeActivity.projectTaskID>(timeActivity, ((PMTask)_Graph.Caches[typeof(PMTask)].Current)?.TaskID);
		}

		public new static BqlCommand GenerateOriginalCommand()
		{
			return BqlCommand.CreateInstance(
				typeof(Select2<
					PMCRActivity,
				LeftJoin<CRReminder,
					On<CRReminder.refNoteID, Equal<PMCRActivity.noteID>>>,
				Where<
					PMCRActivity.projectTaskID, Equal<Current<PMTask.taskID>>>,
				OrderBy<
					Desc<PMCRActivity.timeActivityCreatedDateTime>>>));
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = ProjectTaskActivities.GenerateOriginalCommand();

			if (handler == null)
			View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}
	}

	public sealed class ProjectActivities : PMActivityList<PMProject>
	{
		public ProjectActivities(PXGraph graph)
			: base(graph)
		{
			_Graph.RowSelected.AddHandler<PMProject>(PMProject_RowSelected);

			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectID>(ProjectID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.trackTime>(TrackTime_FieldDefaulting);
		}
		private void ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(PMProject)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((PMProject)primaryCache.Current).ContractID;
			}

		}

		private void TrackTime_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = true;
		}

		private void PMProject_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			PM.PMProject row = (PM.PMProject)e.Row;
			if (row == null || View == null )
				return;

		    //if(View.Cache == null)
            if (View.Graph.Caches.TryGetValue(View.GetItemType(), out var v) && v == null)
                return;

			bool userCanAddActivity = row.Status != ProjectStatus.Completed;
			if (row.RestrictToEmployeeList == true && !sender.Graph.IsExport)
			{
				var select = new PXSelectJoin<EPEmployeeContract,
					InnerJoin<EPEmployee,
						On<EPEmployee.bAccountID, Equal<EPEmployeeContract.employeeID>>>,
					Where<EPEmployeeContract.contractID, Equal<Current<PMProject.contractID>>,
					And<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>>(_Graph);

				EPEmployeeContract record = select.SelectSingle();
				userCanAddActivity = userCanAddActivity && record != null;
			}

		    View.Graph.Caches.SubscribeCacheCreated(View.GetItemType(), delegate
		    {
		        View.Cache.AllowInsert = userCanAddActivity;
            });


		}

		protected override void CreateTimeActivity(PXCache cache, int classId)
		{
			PXCache timeCache = cache.Graph.Caches[typeof(PMTimeActivity)];
			if (timeCache == null) return;

			PMTimeActivity timeActivity = (PMTimeActivity)timeCache.Current;
			if (timeActivity == null) return;

			bool withTimeTracking = classId != CRActivityClass.Task && classId != CRActivityClass.Event;

			timeActivity.TrackTime = withTimeTracking;
			timeActivity.ProjectID = ((PMProject)_Graph.Caches[typeof(PMProject)].Current)?.ContractID;

			timeCache.Update(timeActivity);
		}
		
		public new static BqlCommand GenerateOriginalCommand()
		{
			return BqlCommand.CreateInstance(
				typeof(Select2<
					PMCRActivity,
				LeftJoin<CRReminder, 
					On<CRReminder.refNoteID, Equal<PMCRActivity.noteID>>>,
				Where<
					PMCRActivity.projectID, Equal<Current<PMProject.contractID>>>,
				OrderBy<
					Desc<PMCRActivity.timeActivityCreatedDateTime>>>));
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = ProjectActivities.GenerateOriginalCommand();
			
			if (handler == null)
			View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}
	}

	public sealed class OpportunityActivities : CRActivityList<CROpportunity>
	{
		public OpportunityActivities(PXGraph graph)
			: base(graph)
		{
		}

		
		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<CRPMTimeActivity>
				.LeftJoin<Standalone.CROpportunityRevision>
					.On<Standalone.CROpportunityRevision.noteID.IsEqual<CRPMTimeActivity.refNoteID>
						.And<Standalone.CROpportunityRevision.opportunityID.IsEqual<CROpportunity.opportunityID.FromCurrent>>>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.noteID.FromCurrent>
					.Or<CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.quoteNoteID.FromCurrent>>
					.Or<
						Brackets<
							CROpportunityClass.showContactActivities.FromCurrent.IsEqual<True>
							.And<CRPMTimeActivity.contactID.IsEqual<Contact.contactID.FromCurrent>>
						>
					>
					.Or<CRPMTimeActivity.refNoteID.IsEqual<CROpportunity.leadID.FromCurrent>>
				>
				.OrderBy<CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

	}

	/// <exclude/>
	public sealed class LeadActivities : CRActivityList<CRLead>
	{
		public LeadActivities(PXGraph graph)
			: base(graph) { }

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<
					CRPMTimeActivity>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.refNoteID.IsEqual<CRLead.noteID.FromCurrent>
				>
				.OrderBy<
					CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

	}

	public class PMActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, PMCRActivity>
		where TPrimaryView : class, IBqlTable, new()
	{
		public PMActivityList(PXGraph graph)
			: base(graph)
		{
		}
	}

	public class CRActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, CRPMTimeActivity>
		where TPrimaryView : class, IBqlTable, new()
	{
		public CRActivityList(PXGraph graph)
			: base(graph)
		{
		}

		public CRActivityList(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}
	}

	public class CRActivityListReadonly<TPrimaryView> : CRActivityListBase<TPrimaryView, CRPMTimeActivity>
			where TPrimaryView : class, IBqlTable, new()
	{
		public CRActivityListReadonly(PXGraph graph)
			: base(graph)
		{
			var cache = _Graph.Caches[typeof(CRPMTimeActivity)];
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.subject).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.priority).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.uistatus).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.startDate).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.endDate).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.noteID).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.createdByID).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.body).Name, false);
			PXUIFieldAttribute.SetEnabled(cache, null, typeof(CRPMTimeActivity.categoryID).Name, false);

			PXUIFieldAttribute.SetVisible(cache, null, typeof(CRPMTimeActivity.priority).Name, false);
			cache.AllowUpdate = false;
		}
	}

	public class CRCampaignMembersActivityList<TPrimaryView> : CRActivityListBase<TPrimaryView, CRPMTimeActivity>
		where TPrimaryView : class, IBqlTable, INotable, new()
	{
		protected internal const string _NEW_CAMPAIGNMEMBER_ACTIVITY_COMMAND = "NewCampaignMemberActivity";
		protected internal const string _NEW_CAMPAIGNMEMBER_TASK_COMMAND = "NewCampaignMemberTask";
		protected internal const string _NEW_CAMPAIGNMEMBER_EVENT_COMMAND = "NewCampaignMemberEvent";
		protected internal const string _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND = "NewCampaignMemberMailActivity";

		public CRCampaignMembersActivityList(PXGraph graph)
			: base(graph)
		{
			AddCampginMembersActivityQuickActionsAsMenu(graph);
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var command = new SelectFrom<
					CRPMTimeActivity>
				.LeftJoin<CRReminder>
					.On<CRReminder.refNoteID.IsEqual<CRPMTimeActivity.noteID>>
				.Where<
					CRPMTimeActivity.documentNoteID.IsEqual<CRCampaign.noteID.FromCurrent>
					.Or<CRPMTimeActivity.refNoteID.IsEqual<CRCampaign.noteID.FromCurrent>>>
				.OrderBy<
					CRPMTimeActivity.createdDateTime.Desc>();

			if (handler == null)
				View = new PXView(View.Graph, View.IsReadOnly, command);
			else
				View = new PXView(View.Graph, View.IsReadOnly, command, handler);
		}

		private void AddCampginMembersActivityQuickActionsAsMenu(PXGraph graph)
		{
			List<ActivityService.IActivityType> types = null;
			try
			{
				types = new List<ActivityService.IActivityType>(new EP.ActivityService().GetActivityTypes());
			}
			catch (Exception) {/* #46997 */}



            PXButtonAttribute btAtt = new PXButtonAttribute { OnClosingPopup = PXSpecialButtonType.Refresh };

            PXAction btn = this.AddAction(graph, _NEW_CAMPAIGNMEMBER_ACTIVITY_COMMAND,
									PXMessages.LocalizeNoPrefix(Messages.AddActivity),
									types != null && types.Count > 0,
									NewCampaignMemberActivity, btAtt);

            if (types != null && types.Count > 0)
			{
				List<ButtonMenu> menuItems = new List<ButtonMenu>(types.Count);
				foreach (ActivityService.IActivityType type in types)
				{
					ButtonMenu menuItem = new ButtonMenu(type.Type,
						PXMessages.LocalizeFormatNoPrefix(Messages.AddTypedActivityFormat, type.Description), null);
					if (type.IsDefault == true)
						menuItems.Insert(0, menuItem);
					else
						menuItems.Add(menuItem);
				}
				var taskCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_TASK_COMMAND, Messages.AddTask, null);
				menuItems.Add(taskCommand);
				var eventCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_EVENT_COMMAND, Messages.AddEvent, null);
				menuItems.Add(eventCommand);
				var newEmailActivityCommand = new ButtonMenu(_NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND, Messages.AddEmail, null);
				menuItems.Add(newEmailActivityCommand);

				btn.SetMenu(menuItems.ToArray());
			}
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'A', 'C')]
		public virtual IEnumerable NewCampaignMemberActivity(PXAdapter adapter)
		{
			string type = null;
			int clasId = CRActivityClass.Activity;
			switch (adapter.Menu)
			{
				case _NEW_CAMPAIGNMEMBER_TASK_COMMAND:
					clasId = CRActivityClass.Task;
					break;
				case _NEW_CAMPAIGNMEMBER_EVENT_COMMAND:
					clasId = CRActivityClass.Event;
					break;
				case _NEW_CAMPAIGNMEMBER_MAILACTIVITY_COMMAND:
					clasId = CRActivityClass.Email;
					break;
				default:
					type = adapter.Menu;
					break;
			}
			this.CreateCampaignMemberActivity(clasId, type);
			return adapter.Get();
		}

		private void CreateCampaignMemberActivity(int classId, string type)
		{
			var memberCache = this._Graph.Caches<CRCampaignMembers>();
			if (memberCache.Current is CRCampaignMembers currentMember)
			{
				var graph = CreateNewActivity(classId, type);

				if (graph == null)
					return;

                PXCache activityCache = null;
                if (classId == CRActivityClass.Email)
                {
                    activityCache = graph.Caches<CRSMEmail>();
                }
                else
                {
                    activityCache = graph.Caches<CRActivity>();
                }

				var result = SelectFrom<Contact>
					.LeftJoin<Standalone.CRLead>
						.On<Standalone.CRLead.contactID.IsEqual<Contact.contactID>>
					.Where<
						Contact.contactID.IsEqual<@P.AsInt>>
					.View
					.Select<PXResultset<Contact, Standalone.CRLead>>(this._Graph, currentMember.ContactID);

				var contact = (Contact)result;
				var lead = (Standalone.CRLead)result;

				if (lead?.ContactID != null)
				{
					activityCache.SetValue<CRActivity.refNoteID>(activityCache.Current, contact.NoteID);
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, lead.RefContactID);
				}
				else if(contact.ContactType == ContactTypesAttribute.Person)
				{
					activityCache.SetValue<CRActivity.contactID>(activityCache.Current, contact.ContactID);
				}

				var primaryCurrent = this._Graph.Caches[typeof(TPrimaryView)].Current as TPrimaryView;

				activityCache.SetValue<CRActivity.documentNoteID>(activityCache.Current, primaryCurrent?.NoteID);

				activityCache.SetValue<CRActivity.bAccountID>(activityCache.Current, contact.BAccountID);
                activityCache.SetValue<CRSMEmail.mailTo>(activityCache.Current, contact.EMail);
                activityCache.SetValue<CRSMEmail.mailReply>(activityCache.Current, contact.EMail);

				memberCache.ClearQueryCacheObsolete();
				memberCache.Clear();

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);               
           }
		}
	}

		public class CRChildActivityList<TParentActivity> : CRActivityListBase<TParentActivity, CRChildActivity>
		where TParentActivity : CRActivity, new()
	{
		public CRChildActivityList(PXGraph graph)
			: base(graph)
		{
			_Graph.FieldDefaulting.AddHandler<CRActivity.parentNoteID>(ParentNoteID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<CRChildActivity.parentNoteID>(ParentNoteID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<CRSMEmail.parentNoteID>(ParentNoteID_FieldDefaulting);

			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectID>(ProjectID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.projectTaskID>(ProjectTaskID_FieldDefaulting);
			_Graph.FieldDefaulting.AddHandler<PMTimeActivity.costCodeID>(ProjectCostCodeID_FieldDefaulting);
		}

		private void ProjectID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).ProjectID;
			}

		}

		private void ProjectTaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).ProjectTaskID;
			}

		}

		private void ProjectCostCodeID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var primaryCache = sender.Graph.Caches[typeof(CRPMTimeActivity)];

			if (primaryCache.Current != null)
			{
				e.NewValue = ((CRPMTimeActivity)primaryCache.Current).CostCodeID;
			}

		}

		private void ParentNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var parentCache = sender.Graph.Caches[typeof(TParentActivity)];

			if (parentCache.Current != null)
			{
				e.NewValue = ((TParentActivity)parentCache.Current).NoteID;
			}
		}

		public IEnumerable SelectByParentNoteID(object parentNoteId)
		{
			return PXSelect<CRChildActivity,
				Where<CRChildActivity.parentNoteID, Equal<Required<CRChildActivity.parentNoteID>>>>.
				Select(_Graph, parentNoteId).RowCast<CRChildActivity>();
		}

		protected override void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			noteField = typeof(CRActivity.refNoteID).Name;
			noteBqlField = _Graph.Caches[typeof(TParentActivity)].GetBqlField(noteField);
		}

		protected override void SetCommandCondition(Delegate handler = null)
		{
			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,,>),
					typeof(CRChildActivity.parentNoteID),
					typeof(Equal<>),
					typeof(Optional<>),
					typeof(TParentActivity).GetNestedType(typeof(CRActivity.noteID).Name),
					typeof(And<>),
					typeof(Where<,,>),
					typeof(CRChildActivity.isCorrected),
					typeof(NotEqual<True>),
					typeof(Or<,>),
					typeof(CRChildActivity.isCorrected),
					typeof(IsNull)));

			if (handler == null)
			View = new PXView(View.Graph, View.IsReadOnly, newCmd);
			else
				View = new PXView(View.Graph, View.IsReadOnly, newCmd, handler);
		}
	}

	public delegate long? CalculateAlternativeNoteIdHandler(object row);

	public interface IActivityList
	{
		bool CheckActivitiesForDelete(params object[] currents);
		Type PrimaryViewType { get; }
	}

	public abstract class CRActivityListBase<TActivity> : PXSelectBase<TActivity>
		where TActivity : CRPMTimeActivity, new()
	{
		protected internal const string _NEWTASK_COMMAND = "NewTask";
		protected internal const string _NEWEVENT_COMMAND = "NewEvent";
		protected internal const string _VIEWACTIVITY_COMMAND = "ViewActivity";
		protected internal const string _VIEWALLACTIVITIES_COMMAND = "ViewAllActivities";
		protected internal const string _NEWACTIVITY_COMMAND = "NewActivity";
		protected internal const string _NEWMAILACTIVITY_COMMAND = "NewMailActivity";
		protected internal const string _REGISTERACTIVITY_COMMAND = "RegisterActivity";
		protected internal const string _OPENACTIVITYOWNER_COMMAND = "OpenActivityOwner";

		public static BqlCommand GenerateOriginalCommand()
		{
			var noteIdField = typeof(TActivity).GetNestedType(typeof(CRActivity.noteID).Name);
			var classIdField = typeof(TActivity).GetNestedType(typeof(CRActivity.classID).Name);
			var createdDateTimeField = typeof(TActivity).GetNestedType(typeof(CRActivity.createdDateTime).Name);

			return BqlCommand.CreateInstance(
				typeof(Select2<,,,>), 
					typeof(TActivity),
				typeof(LeftJoin<,>), typeof(CRReminder),
					typeof(On<,>), typeof(CRReminder.refNoteID), typeof(Equal<>), noteIdField,
				typeof(Where<,>), 
					classIdField, typeof(GreaterEqual<>), typeof(Zero),
				//typeof(And<,>), typeof(CRActivity.mpstatus), typeof(NotEqual<>), typeof(MailStatusListAttribute.deleted),
				typeof(OrderBy<>),
				typeof(Desc<>), createdDateTimeField);
		}
	}

	[PXDynamicButton(new string[] { "NewTask", "NewEvent", "ViewActivity", "NewMailActivity", "RegisterActivity", "OpenActivityOwner", "ViewAllActivities", "NewActivity" },
					 new string[] { Messages.AddTask, Messages.AddEvent, Messages.Details, Messages.AddEmail, Messages.RegisterActivity, Messages.OpenActivityOwner, Messages.ViewAllActivities, Messages.AddActivity },
					 TranslationKeyType = typeof(Messages))]
	public class CRActivityListBase<TPrimaryView, TActivity> : CRActivityListBase<TActivity>, IActivityList
		where TPrimaryView : class, IBqlTable, new()
		where TActivity : CRPMTimeActivity, new()
	{
		#region Constants

		protected const string _PRIMARY_WORKGROUP_ID = "WorkgroupID";

		#endregion

		#region Fields

		public delegate string GetEmailHandler();

		private int? _defaultEMailAccountId;

		private readonly BqlCommand _originalCommand;
		private readonly string _refField;
		private readonly Type _refBqlField;

		private bool _enshureTableData = true;

		#endregion

		#region Ctor

		public CRActivityListBase(PXGraph graph) : this(graph, null)
		{
		}

		public CRActivityListBase(PXGraph graph, Delegate handler)
		{
			_Graph = graph;

			_Graph.EnsureCachePersistence(typeof(TActivity));
			_Graph.EnsureCachePersistence(typeof(CRReminder));

			var cache = _Graph.Caches[typeof(TActivity)];

			ReadNoteIDFieldInfo(out _refField, out _refBqlField);

			graph.RowSelected.AddHandler(typeof(TPrimaryView), Table_RowSelected);
			if (typeof(CRActivity).IsAssignableFrom(typeof(TPrimaryView)))
				graph.RowPersisted.AddHandler<TPrimaryView>(Table_RowPersisted);
			graph.RowDeleting.AddHandler<TActivity>(Activity_RowDeleting);
			graph.RowDeleted.AddHandler<TActivity>(Activity_RowDeleted);
			graph.RowPersisting.AddHandler<TActivity>(Activity_RowPersisting);
			graph.RowPersisted.AddHandler<TActivity>(Activity_RowPersisted);
			graph.RowSelected.AddHandler<TActivity>(Activity_RowSelected);

			graph.FieldDefaulting.AddHandler(typeof(CRActivity), typeof(CRActivity.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler(typeof(CRSMEmail), typeof(CRSMEmail.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);
			graph.FieldDefaulting.AddHandler(typeof(TActivity), typeof(CRActivity.refNoteID).Name, Activity_RefNoteID_FieldDefaulting);

			graph.FieldSelecting.AddHandler(typeof(TActivity), typeof(CRActivity.body).Name, Activity_Body_FieldSelecting);

			AddActions(graph);
			AddPreview(graph);

			PXUIFieldAttribute.SetVisible(cache, null, typeof(CRActivity.noteID).Name, false);

			_originalCommand = GenerateOriginalCommand();

			if (handler == null)
			View = new PXView(graph, false, OriginalCommand);
			else
				View = new PXView(graph, false, OriginalCommand, handler);


			SetCommandCondition(handler);
		}

		#endregion

		#region Implementation

		#region Preview

		private void AddPreview(PXGraph graph)
		{
			graph.Initialized += sender =>
			{
				string viewName;
				if (graph.ViewNames.TryGetValue(this.View, out viewName))
				{
					var att = new CRPreviewAttribute(typeof(TPrimaryView), typeof(TActivity));
					att.Attach(graph, viewName, null);
				}
			};
		}

		#endregion

		#region Add Actions
		protected void AddActions(PXGraph graph)
		{
			AddAction(graph, _NEWTASK_COMMAND, Messages.AddTask, NewTask);
			AddAction(graph, _NEWEVENT_COMMAND, Messages.AddEvent, NewEvent);
			AddAction(graph, _VIEWACTIVITY_COMMAND, Messages.Details, ViewActivity);
			AddAction(graph, _NEWMAILACTIVITY_COMMAND, Messages.AddEmail, NewMailActivity);
			AddAction(graph, _OPENACTIVITYOWNER_COMMAND, string.Empty, OpenActivityOwner);
			AddAction(graph, _VIEWALLACTIVITIES_COMMAND, Messages.ViewAllActivities, ViewAllActivities);

			AddActivityQuickActionsAsMenu(graph);
		}

		private void AddActivityQuickActionsAsMenu(PXGraph graph)
		{
			List<ActivityService.IActivityType> types = null;
			try
			{
				types = new List<ActivityService.IActivityType>(new EP.ActivityService().GetActivityTypes());
			}
			catch (Exception) {/* #46997 */}

			PXAction btn = AddAction(graph, _NEWACTIVITY_COMMAND,
								PXMessages.LocalizeNoPrefix(Messages.AddActivity),
								types != null && types.Count > 0,
								NewActivityByType);

			if (types != null && types.Count > 0)
			{
				List<ButtonMenu> menuItems = new List<ButtonMenu>(types.Count);
				foreach (ActivityService.IActivityType type in types)
				{
					ButtonMenu menuItem = new ButtonMenu(type.Type,
						PXMessages.LocalizeFormatNoPrefix(Messages.AddTypedActivityFormat, type.Description), null);
					if (type.IsDefault == true)
						menuItems.Insert(0, menuItem);
					else
						menuItems.Add(menuItem);
				}
				btn.SetMenu(menuItems.ToArray());
			}
		}

		internal void AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			AddAction(graph, name, displayName, true, handler, null);
		}

		internal PXAction AddAction(PXGraph graph, string name, string displayName, bool visible, PXButtonDelegate handler, params PXEventSubscriberAttribute[] attrs)
		{
			PXUIFieldAttribute uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Select
			};
			if (!visible) uiAtt.Visible = false;
			List<PXEventSubscriberAttribute> addAttrs = new List<PXEventSubscriberAttribute> { uiAtt };
			if (attrs != null)
				addAttrs.AddRange(attrs.Where(attr => attr != null));
			PXNamedAction<TPrimaryView> res = new PXNamedAction<TPrimaryView>(graph, name, handler, addAttrs.ToArray());
			graph.Actions[name] = res;
			return res;
		}
		#endregion

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				if (EnshureTableData && _Graph.IsDirty)
				{
					if (_Graph.AutomationView != null)
						PXAutomation.GetStep(_Graph,
							new object[] { _Graph.Views[_Graph.AutomationView].Cache.Current },
							BqlCommand.CreateInstance(typeof(Select<>), _Graph.Views[_Graph.AutomationView].Cache.GetItemType()));
					_Graph.Actions.PressSave();
				}

				var graph = PXGraph.CreateInstance(graphType);
				graph.Clear();
				foreach (Type type in graph.Views.Caches)
				{
					var cache = graph.Caches[type];
					if (typeof(TNode).IsAssignableFrom(cache.GetItemType()))
						return cache;
				}
			}
			return null;
		}

		public Type PrimaryViewType
		{
			get { return typeof(TPrimaryView); }
		}

		#region Delete Record
		public virtual void DeleteActivities(params object[] currents)
		{
			if (!typeof(CRActivity).IsAssignableFrom(typeof(CRActivity))) return;

			foreach (var item in View.SelectMultiBound(currents))
				Cache.Delete(item is PXResult ? ((PXResult)item)[typeof(CRActivity)] : item);
		}

		public virtual bool CheckActivitiesForDelete(params object[] currents)
		{
			return CheckActivitiesForDeleteInternal("act_cannot_delete", Messages.ConfirmDeleteActivities, currents);
		}

		protected virtual bool CheckActivitiesForDeleteInternal(string key, string msg, params object[] currents)
		{
			if (!typeof(CRPMTimeActivity).IsAssignableFrom(typeof(CRPMTimeActivity))) return true;

			foreach (object item in View.SelectMultiBound(currents))
			{
				var row = (CRPMTimeActivity)item;

				if (row.Billed == true || !string.IsNullOrEmpty(row.TimeCardCD))
				{
					return View.Ask(key, msg, MessageButtons.YesNoCancel) == WebDialogResult.Yes;
				}
			}
			return true;
		}

		private void DeleteActivity(CRActivity current)
		{
			var graphType = CRActivityPrimaryGraphAttribute.GetGraphType(current);
			var cache = CreateInstanceCache<CRActivity>(graphType);
			if (cache != null)
			{
				if (!cache.AllowDelete)
					throw new PXException(ErrorMessages.CantDeleteRecord);

				var searchView = new PXView(
					cache.Graph,
					false,
					BqlCommand.CreateInstance(typeof(Select<>), cache.GetItemType()));
				var startRow = 0;
				var totalRows = 0;
				var acts = searchView.
					Select(null, null,
						new object[] { current.NoteID },
						new string[] { typeof(CRActivity.noteID).Name },
						null, null, ref startRow, 1, ref totalRows);

				if (acts != null && acts.Count > 0)
				{
					var act = acts[0];
					cache.Current = act;
					cache.Delete(act);
					cache.Graph.Actions.PressSave();
				}
			}
		}
		#endregion

		protected void CreateActivity(int classId, string type)
		{
			var graph = CreateNewActivity(classId, type);

			if (graph != null)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
		}

		protected virtual PXGraph CreateNewActivity(int classId, string type)
		{
			Type graphType = CRActivityPrimaryGraphAttribute.GetGraphType(classId);

			// TODO: NEED REFACTOR!

			if (!PXAccess.VerifyRights(graphType))
			{
				_Graph.Views[_Graph.PrimaryView].Ask(null, Messages.AccessDenied, Messages.FormNoAccessRightsMessage(graphType),
					MessageButtons.OK, MessageIcon.Error);
				return null;
			}
			else
			{
				PXCache cache;

				if (classId == CRActivityClass.Email)
				{
					cache = CreateInstanceCache<CRSMEmail>(graphType);
					if (cache == null) return null;

					var localActivity = (CRSMEmail)_Graph.Caches[typeof(CRSMEmail)].CreateInstance();

					localActivity.ClassID = classId;
					localActivity.Type = type;

					CRSMEmail email = ((PXCache<CRSMEmail>)_Graph.Caches[typeof(CRSMEmail)]).InitNewRow(localActivity);


					Guid? owner = EmployeeMaint.GetCurrentEmployeeID(_Graph);
					int? workgroup = GetParentGroup();
					email.OwnerID = owner;
					if (email.OwnerID != null && PXOwnerSelectorAttribute.BelongsToWorkGroup(_Graph, workgroup, email.OwnerID))
						email.WorkgroupID = workgroup;


					email.MailAccountID = DefaultEMailAccountId;
					FillMailReply(email);
					FillMailTo(email);
					if (email.RefNoteID != null)
						FillMailCC(email, email.RefNoteID);
					FillMailSubject(email);
					email.Body = GenerateMailBody();

					email.ClassID = classId;
					_Graph.Caches[typeof(CRSMEmail)].SetValueExt(email, typeof(CRActivity.type).Name,
						!string.IsNullOrEmpty(type) ? type : email.Type);

					if (_Graph.IsDirty)
					{
						if (_Graph.IsMobile) // ensure that row will be persisted with Note when call from mobile
						{
							var rowCache = _Graph.Views[_Graph.PrimaryView].Cache;
							if (rowCache.Current != null)
							{
								rowCache.SetStatus(rowCache.Current, PXEntryStatus.Updated);
							}
						}
						_Graph.Actions.PressSave();
					}

					cache.Insert(email);
				}
				else
				{
					CRActivity activity;

					cache = CreateInstanceCache<CRActivity>(graphType);
					if (cache == null) return null;

					var localActivity = (CRActivity)_Graph.Caches[typeof(CRActivity)].CreateInstance();

					localActivity.ClassID = classId;
					localActivity.Type = type;

					activity = ((PXCache<CRActivity>)_Graph.Caches[typeof(CRActivity)]).InitNewRow(localActivity);

					Guid? owner = EmployeeMaint.GetCurrentEmployeeID(_Graph);
					int? workgroup = GetParentGroup();
					activity.OwnerID = owner;
					if (activity.OwnerID != null && PXOwnerSelectorAttribute.BelongsToWorkGroup(_Graph, workgroup, activity.OwnerID))
						activity.WorkgroupID = workgroup;


					if (_Graph.IsDirty)
					{
						if (_Graph.IsMobile) // ensure that row will be persisted with Note when call from mobile
						{
							var rowCache = _Graph.Views[_Graph.PrimaryView].Cache;
							if (rowCache.Current != null)
							{
								rowCache.SetStatus(rowCache.Current, PXEntryStatus.Updated);
							}
						}
					_Graph.Actions.PressSave();
				}

					cache.Insert(activity);
				}

				CreateTimeActivity(cache, classId);

				foreach (PXCache dirtycache in cache.Graph.Caches.Caches.Where(c => c.IsDirty))
				{
					dirtycache.IsDirty = false;
				}
				return cache.Graph;
			}
		}

		protected virtual void CreateTimeActivity(PXCache graphType, int classId)
		{

		}

		private int? GetParentGroup()
		{
			PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
			return (int?)cache.GetValue(cache.Current, _PRIMARY_WORKGROUP_ID);
		}

		protected TActivity CurrentActivity
		{
			get
			{
				var tableCache = _Graph.Caches[typeof(TActivity)];
				return tableCache.With(_ => (TActivity)_.Current);
			}
		}

		protected PMTimeActivity CurrentTimeActivity
		{
			get
			{
				var tableCache = _Graph.Caches[typeof(TActivity)];
				return tableCache
					.With(_ => (TActivity)_.Current)
					.With(_ => _.TimeActivityNoteID ?? Guid.Empty)
					.With(_ => PXSelect<PMTimeActivity, Where<PMTimeActivity.noteID, Equal<Required<CRPMTimeActivity.timeActivityNoteID>>>>.Select(_Graph, _));
			}
		}
		
	    public void SendNotification(string sourceType, string notifications, int? branchID,
	        IDictionary<string, string> parameters, IList<Guid?> attachments = null)
	    {
	        var sender = CreateNotificationProvider(sourceType,
	            notifications,
	            branchID,
	            parameters,
	            attachments);

	        if (sender == null || !sender.Send().Any())
	            throw new PXException(Messages.EmailNotificationError);
        }

        public virtual void SendNotification(string sourceType, IList<string> notificationCDs, int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null)
		{
			var sender = CreateNotificationProvider(sourceType, 
                notificationCDs, 
                branchID, 
                parameters, 
                attachments);

		    if (sender == null || !sender.Send().Any())
		        throw new PXException(Messages.EmailNotificationError);
        }

	    public virtual NotificationGenerator CreateNotificationProvider(string sourceType, string notifications, int? branchID,
	        IDictionary<string, string> parameters, IList<Guid?> attachments = null)
	    {
	        if (notifications == null) return null;
	        IList<string> list = notifications.Split(',')
	            .Select(n => n?.Trim())
	            .Where(cd => !string.IsNullOrEmpty(cd)).ToList();
	        return CreateNotificationProvider(sourceType, list, branchID, parameters, attachments);
	    }


        public virtual NotificationGenerator CreateNotificationProvider(string sourceType, IList<string> notificationCDs,
	        int? branchID, IDictionary<string, string> parameters, IList<Guid?> attachments = null)
	    {
	        PXCache sourceCache = _Graph.Caches[typeof(TPrimaryView)];
	        if (sourceCache.Current == null)
	            throw new PXException(Messages.EmailNotificationObjectNotFound);

	        IList<NotificationSetup> setupIDs = GetSetupNotifications(sourceType, notificationCDs);
	        PXCache cache = _Graph.Caches[typeof(TPrimaryView)];
	        TPrimaryView row = (TPrimaryView)cache.Current;

			object correctTypeRow = cache.CreateInstance();
			cache.RestoreCopy(correctTypeRow, row);
			row = (TPrimaryView)correctTypeRow;

	        TActivity activity = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow();
	        var sourceRow = GetSourceRow(sourceType, activity);
			
	        var utility = new NotificationUtility(_Graph);
	        RecipientList recipients = null;
	        TemplateNotificationGenerator sender = null;
	        for (int i = 0; i < setupIDs.Count; i++)
	        {
		        NotificationSetup setup = setupIDs[i];
		        NotificationSource source =
			        sourceRow != null
				        ? utility.GetSource(sourceType, sourceRow, setup.SetupID.Value, branchID ?? this._Graph.Accessinfo.BranchID)
				        : utility.GetSource(setup);

				if (source == null && sourceType == PMNotificationSource.Project)
				{
					source = utility.GetSource(sourceType, row, setup.SetupID.Value, branchID ?? this._Graph.Accessinfo.BranchID);
				}

				if (source == null)
				{
					throw new PXException(PX.SM.Messages.NotificationSourceNotFound);
				}

				if (sender == null)
	            {
	                var accountId = source.EMailAccountID ?? DefaultEMailAccountId;
	                if (accountId == null)
	                    throw new PXException(ErrorMessages.EmailNotConfigured);

	                if (recipients == null)
	                    recipients = utility.GetRecipients(sourceType, sourceRow, source);

	                sender = TemplateNotificationGenerator.Create(row, source.NotificationID);
					
	                sender.MailAccountId = accountId;
	                sender.RefNoteID = activity.RefNoteID;
	                sender.DocumentNoteID = activity.DocumentNoteID;
	                sender.BAccountID = activity.BAccountID;
		            sender.ContactID = activity.ContactID;
	                sender.Watchers = recipients;
	            }
	            if (source.ReportID != null)
	            {
	                var _report = PXReportTools.LoadReport(source.ReportID, null);

	                if (_report == null) throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(EP.Messages.ReportCannotBeFound, source.ReportID), "reportId");
	                PXReportTools.InitReportParameters(_report, parameters ?? KeyParameters(sourceCache), SettingsProvider.Instance.Default);
	                _report.MailSettings.Format = ReportNotificationGenerator.ConvertFormat(source.Format);
	                var reportNode = ReportProcessor.ProcessReport(_report);

	                reportNode.SendMailMode = true;
	                PX.Reports.Mail.Message message =
	                (from msg in reportNode.Groups.Select(g => g.MailSettings)
	                    where msg != null && msg.ShouldSerialize()
	                    select new PX.Reports.Mail.Message(msg, reportNode, msg)).FirstOrDefault();

	                if (message == null)
					{
						if(i == 0)
							throw new InvalidOperationException(
								PXMessages.LocalizeFormatNoPrefixNLA(
									EP.Messages.EmailFromReportCannotBeCreated, source.ReportID));
						continue;
					}
	                if (i == 0)
	                {
	                    bool bodyWasNull = false;
	                    if (sender.Body == null)
	                    {
							string body = message.Content.Body;
							bodyWasNull = body == null;
							if(bodyWasNull || !IsHtml(body))
							{
								body = Tools.ConvertSimpleTextToHtml(message.Content.Body);
							}
							sender.Body = body;
	                        sender.BodyFormat = NotificationFormat.Html;
	                    }
	                    sender.Subject = string.IsNullOrEmpty(sender.Subject) ? message.Content.Subject : sender.Subject;
	                    sender.To = string.IsNullOrEmpty(sender.To) ? message.Addressee.To : sender.To;
	                    sender.Cc = string.IsNullOrEmpty(sender.Cc) ? message.Addressee.Cc : sender.Cc;
	                    sender.Bcc = string.IsNullOrEmpty(sender.Bcc) ? message.Addressee.Bcc : sender.Bcc;

	                    if (!string.IsNullOrEmpty(message.TemplateID))
	                    {
	                        TemplateNotificationGenerator generator =
	                            TemplateNotificationGenerator.Create(row, message.TemplateID);

	                        var template = generator.ParseNotification();

	                        if (string.IsNullOrEmpty(sender.Body) || bodyWasNull)
	                            sender.Body = template.Body;
	                        if (string.IsNullOrEmpty(sender.Subject))
	                            sender.Subject = template.Subject;
	                        if (string.IsNullOrEmpty(sender.To))
	                            sender.To = template.To;
	                        if (string.IsNullOrEmpty(sender.Cc))
	                            sender.Cc = template.Cc;
	                        if (string.IsNullOrEmpty(sender.Bcc))
	                            sender.Bcc = template.Bcc;
	                    }
	                    if (string.IsNullOrEmpty(sender.Subject))
	                        sender.Subject = reportNode.Report.Name;
	                }
	                foreach (var attachment in message.Attachments)
	                {
	                    if (sender.Body == null && sender.BodyFormat == NotificationFormat.Html && attachment.MimeType == "text/html")
	                    {
	                        sender.Body = attachment.Encoding.GetString(attachment.GetBytes());
	                    }
	                    else
							sender.AddAttachment(attachment.Name, attachment.GetBytes(), attachment.CID);
	                }

	                if (attachments != null)
	                    foreach (var attachment in attachments)
	                        if (attachment != null)
	                            sender.AddAttachmentLink(attachment.Value);
	            }
	        }
	        return sender;
	    }

		protected virtual object GetSourceRow(string sourceType, TActivity activity)
		{
			var sourceRow = activity.BAccountID.With(_ => new EntityHelper(_Graph).GetEntityRowByID(typeof(BAccountR), _.Value));

			if (sourceRow == null)
				sourceRow = activity.RefNoteID.With(_ => new EntityHelper(_Graph).GetEntityRow(typeof(BAccountR), _.Value));

			return sourceRow;
		}
				
	    protected static IDictionary<string, string> KeyParameters(PXCache sourceCache)
	    {
	        IDictionary<string, string> parameters = new Dictionary<string, string>();
	        foreach (string key in sourceCache.Keys)
	        {
	            object value = sourceCache.GetValueExt(sourceCache.Current, key);
	            parameters[key] = value?.ToString();
	        }
	        return parameters;
	    }

		protected Guid[] GetNotifications(string sourceType, IList<string> notificationCDs)
		{
			Guid[] setupIDs = new Guid[notificationCDs.Count];
			for (int i = 0; i < notificationCDs.Count; i++)
			{
				Guid? SetupID = new NotificationUtility(_Graph).SearchSetupID(sourceType, notificationCDs[i]);
				if (SetupID == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);
				setupIDs[i] = SetupID.Value;
			}
			return setupIDs;
		}

		protected List<NotificationSetup> GetSetupNotifications(string sourceType, IList<string> notificationCDs)
		{
			var setups = new List<NotificationSetup>();
			for (int i = 0; i < notificationCDs.Count; i++)
			{
				NotificationSetup setup = new NotificationUtility(_Graph).SearchSetup(sourceType, notificationCDs[i]);				
				if (setup == null)
					throw new PXException(Messages.EmailNotificationSetupNotFound, notificationCDs[i]);
				setups.Add(setup);
			}
			return setups;
		}

		private static Guid? GetNoteId(PXGraph graph, object row)
		{
			if (row == null) return null;

			var rowType = row.GetType();
			var noteField = EntityHelper.GetNoteField(rowType);
			var cache = graph.Caches[rowType];
			return PXNoteAttribute.GetNoteID(cache, row, noteField);
		}

		private static bool IsHtml(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				var htmlIndex = text.IndexOf("<html", StringComparison.CurrentCultureIgnoreCase);
				var bodyIndex = text.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);
				return htmlIndex > -1 && bodyIndex > -1 && bodyIndex > htmlIndex;
			}
			return false;
		}

		#endregion

		#region Email Methods

		protected virtual void FillMailReply(CRSMEmail message)
		{
			Mailbox mailAddress = null;

			var isCorrect = message.MailReply != null 
				&& Mailbox.TryParse(message.MailReply, out mailAddress) 
				&& !string.IsNullOrEmpty(mailAddress.Address);

			if (isCorrect)
			{
				isCorrect = PXSelect<EMailAccount,
					Where<EMailAccount.address, Equal<Required<EMailAccount.address>>>>
					.Select(_Graph, mailAddress.Address)
					.Count > 0;
			}

			var result = message.MailReply;

			if (!isCorrect)
			{
				result = DefaultEMailAccountId
					.With(_ => (EMailAccount)PXSelect<EMailAccount,
						Where<EMailAccount.emailAccountID, Equal<Required<EMailAccount.emailAccountID>>>>
						.Select(_Graph, _.Value))
					.With(_ => _.Address);
			}

			if (string.IsNullOrEmpty(result))
			{
				var firstAcct = (EMailAccount)PXSelect<EMailAccount>.SelectWindowed(_Graph, 0, 1);
				if (firstAcct != null) result = firstAcct.Address;
			}

			message.MailReply = result;
		}

		protected virtual void FillMailTo(CRSMEmail message)
		{
			string customMailTo = GetNewEmailAddress?.Invoke();

			if (!string.IsNullOrEmpty(customMailTo))
				message.MailTo = customMailTo.With(_ => _.Trim());
		}

		protected virtual void FillMailCC(CRSMEmail message, Guid? refNoteId)
		{
			if (refNoteId == null) return;
			
			message.MailCc = PXDBEmailAttribute.AppendAddresses(message.MailCc, CRRelationsList<CRRelation.refNoteID>.GetEmailsForCC(_Graph, refNoteId.Value));
		}

		protected virtual void FillMailSubject(CRSMEmail message)
		{
			if (!string.IsNullOrEmpty(DefaultSubject))
				message.Subject = DefaultSubject;
		}

		protected virtual string GenerateMailBody()
		{
			string res = null;
			var signature = ((UserPreferences)PXSelect<UserPreferences>.
				Search<UserPreferences.userID>(_Graph, PXAccess.GetUserID())).
				With(pref => pref.MailSignature);
			if (signature != null && (signature = signature.Trim()) != string.Empty)
				res += "<br />" + signature;
			return PX.Web.UI.PXRichTextConverter.NormalizeHtml(res);
		}

		#endregion

		#region Actions

		[PXButton]
		[PXUIField(Visible = false)]
		public virtual IEnumerable OpenActivityOwner(PXAdapter adapter)
		{
			var act = Cache.Current as CRActivity;
			if (act != null)
			{
				var empl = (EPEmployee)PXSelectReadonly<EPEmployee,
					Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(_Graph, act.OwnerID);
				if (empl != null)
					PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(EPEmployee)], empl, string.Empty, PXRedirectHelper.WindowMode.NewWindow);

				var usr = (Users)PXSelectReadonly<Users,
					Where<Users.pKID, Equal<Required<Users.pKID>>>>.
					Select(_Graph, act.OwnerID);
				if (usr != null)
					PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(Users)], usr, string.Empty, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Task, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'K', 'C')]
		public virtual IEnumerable NewTask(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Task, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Event, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'E', 'C')]
		public virtual IEnumerable NewEvent(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Event, null);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'A', 'C')]
		public virtual IEnumerable NewActivity(PXAdapter adapter)
		{
			return NewActivityByType(adapter);
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual IEnumerable NewActivityByType(PXAdapter adapter)
		{
			return NewActivityByType(adapter, adapter.Menu);
		}

		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		protected virtual IEnumerable NewActivityByType(PXAdapter adapter, string type)
		{
			CreateActivity(CRActivityClass.Activity, type);
			return adapter.Get();
		}

		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.MailSend, OnClosingPopup = PXSpecialButtonType.Refresh)]
		[PXShortCut(true, false, false, 'A', 'M')]
		public virtual IEnumerable NewMailActivity(PXAdapter adapter)
		{
			CreateActivity(CRActivityClass.Email, null);
			return adapter.Get();
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable ViewAllActivities(PXAdapter adapter)
		{
			var gr = new ActivitiesMaint();
			gr.Filter.Current.NoteID = ((PXCache<TActivity>)_Graph.Caches[typeof(TActivity)]).InitNewRow().RefNoteID;
			throw new PXPopupRedirectException(gr, string.Empty, true);
		}

		[PXUIField(Visible = false, MapEnableRights = PXCacheRights.Select)]
		[PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			NavigateToActivity(CurrentActivity);

			return adapter.Get();
		}

		public string DefaultActivityType
		{
			get
			{
				string result = null;

				EPSetup setup = PXSelect<EPSetup>.Select(_Graph);
				if (setup != null && !string.IsNullOrEmpty(setup.DefaultActivityType))
				{
					result = setup.DefaultActivityType;
				}

				return result;
			}
		}

		private void NavigateToActivity(object row)
		{
			Type primaryGraph = CRActivityPrimaryGraphAttribute.GetGraphType(row as CRActivity);
			PXGraph graph = PXGraph.CreateInstance(primaryGraph);

			PXCache rowCache = graph.Caches[row.GetType()];

			if (rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityNoteID).Name) != null 
				&& rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityNoteID).Name).Equals(rowCache.GetValue(row, typeof(CRPMTimeActivity.timeActivityRefNoteID).Name)))
			{
			var timeAct = CurrentTimeActivity;

				PXCache parentCache = graph.Caches[typeof(CRActivity)];
				PXCache childCache = graph.Caches[typeof(PMTimeActivity)];

				var parentEntity = parentCache.NonDirtyInsert();

				parentCache.SetValue(parentEntity, typeof(CRActivity.noteID).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.refNoteID).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.type).Name, DefaultActivityType);
				parentCache.SetValue(parentEntity, typeof(CRActivity.subject).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.summary).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.ownerID).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.ownerID).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.startDate).Name, childCache.GetValue(timeAct, typeof(CRPMTimeActivity.date).Name));
				parentCache.SetValue(parentEntity, typeof(CRActivity.uistatus).Name, ActivityStatusAttribute.Completed);
				parentCache.Normalize();

				childCache.SetValue(timeAct, typeof(PMTimeActivity.summary).Name, parentCache.GetValue(parentEntity, typeof(CRActivity.subject).Name));
				childCache.Current = timeAct;

				childCache.SetStatus(childCache.Current, PXEntryStatus.Updated);

				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			}
			else
			{
				PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		#endregion

		#region Buttons

		public PXAction ButtonViewAllActivities
		{
			get { return _Graph.Actions[_VIEWALLACTIVITIES_COMMAND]; }
		}

		#endregion

		#region Properties

		public bool EnshureTableData
		{
			get { return _enshureTableData; }
			set { _enshureTableData = value; }
		}

		public virtual GetEmailHandler GetNewEmailAddress { get; set; }

		public string DefaultSubject { get; set; }

		public int? DefaultEMailAccountId
		{
			get
			{
				return _defaultEMailAccountId ?? MailAccountManager.DefaultAnyMailAccountID;
			}
			set { _defaultEMailAccountId = value; }
		}

		#endregion

		#region Event Handlers
		protected virtual void Table_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			object row = e.Row;
			if (sender.Graph.GetType() != typeof(PXGraph) && sender.Graph.GetType() != typeof(PXGenericInqGrph))
			{
				Type itemType = sender.GetItemType();
				DynamicRowSelected rs = new DynamicRowSelected(itemType, row, sender, this);
				//will be called after graph event
				sender.Graph.RowSelected.AddHandler(itemType, rs.RowSelected);
			}

		}

		private class DynamicRowSelected
		{
			private Type ItemType;
			private object Row;
			private PXCache Cache;
			private CRActivityListBase<TPrimaryView, TActivity> BaseClass;
			public DynamicRowSelected(Type itemType, object row, PXCache cache, CRActivityListBase<TPrimaryView, TActivity> baseClass)
			{
				ItemType = itemType;
				Row = row;
				Cache = cache;
				BaseClass = baseClass;
			}
			public void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
			{
				BaseClass.CorrectButtons(Cache, Row, Cache.GetStatus(Row));
				sender.Graph.RowSelected.RemoveHandler(ItemType, RowSelected);
			}
		}

		internal void CorrectButtons(PXCache sender, object row, PXEntryStatus status)
		{
			if (!EnshureTableData) return;

			row = row ?? sender.Current;
			var viewButtonsEnabled = row != null;

			viewButtonsEnabled = viewButtonsEnabled && Array.IndexOf(NotEditableStatuses, status) < 0;
			var editButtonEnabled = viewButtonsEnabled && this.View.Cache.AllowInsert;
			PXActionCollection actions = sender.Graph.Actions;

			actions[_NEWTASK_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWEVENT_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWMAILACTIVITY_COMMAND].SetEnabled(editButtonEnabled);
			actions[_NEWACTIVITY_COMMAND].SetEnabled(editButtonEnabled);

			PXButtonState state = actions[_NEWACTIVITY_COMMAND].GetState(row) as PXButtonState;
			if (state != null && state.Menus != null)
				foreach (var button in state.Menus)
				{
					button.Enabled = editButtonEnabled;
				}
		}

		protected virtual void Table_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Completed)
			{
				object row = e.Row;
				CorrectButtons(sender, row, PXEntryStatus.Notchanged);
			}
		}

		protected virtual void Activity_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (e.Row != null)
			{
				//Emails with empty recipient should only remove from cache
				if (sender.GetStatus(e.Row) != PXEntryStatus.InsertedDeleted)
					DeleteActivity((CRActivity)e.Row);

				sender.SetStatus(e.Row, PXEntryStatus.Notchanged);
				sender.Remove(e.Row);

				sender.SetValuePending(e.Row, "cacheIsDirty", sender.IsDirty);
			}
		}

		protected virtual void Activity_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (e.Row != null)
				sender.IsDirty = true.Equals(sender.GetValuePending(e.Row, "cacheIsDirty"));
		}

		protected virtual void Activity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((e.Operation & PXDBOperation.Delete) == PXDBOperation.Delete) e.Cancel = true;

			var row = e.Row as TActivity;
			if (row == null) return;

			if (row.ChildKey == null)
			{
				// means no Child

				var tables = sender.BqlSelect.GetTables();

				foreach (var field in _Graph.Caches[tables.Length > 1 ? tables[1] : typeof(CRActivity)].Fields)
				{
					PXDefaultAttribute.SetPersistingCheck(sender, field, row, PXPersistingCheck.Nothing);
				}

				PXProjectionAttribute projection = (PXProjectionAttribute)(row.GetType()).GetCustomAttributes(typeof(PXProjectionAttribute), true)[0];

				projection.Persistent = false;
			}
		}

		protected virtual void Activity_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			var row = e.Row as TActivity;
			if (row == null) return;

			PXProjectionAttribute projection = (PXProjectionAttribute)(row.GetType()).GetCustomAttributes(typeof(PXProjectionAttribute), true)[0];

			projection.Persistent = true;
		}

		protected virtual void Activity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as TActivity;
			if (row == null) return;

			if (row.ClassID == CRActivityClass.Task || row.ClassID == CRActivityClass.Event)
			{
				int timespent = 0;
				int overtimespent = 0;
				int timebillable = 0;
				int overtimebillable = 0;

				foreach (PXResult<CRChildActivity, PMTimeActivity> child in
					PXSelectJoin<CRChildActivity,
						InnerJoin<PMTimeActivity,
							On<PMTimeActivity.refNoteID, Equal<CRChildActivity.noteID>>>,
						Where<CRChildActivity.parentNoteID, Equal<Required<CRChildActivity.parentNoteID>>,
							And<
								Where<PMTimeActivity.isCorrected, NotEqual<True>, Or<PMTimeActivity.isCorrected, IsNull>>>>>.
						Select(_Graph, row.NoteID))
				{
					var childTime = (PMTimeActivity)child;

					timespent += (childTime.TimeSpent ?? 0);
					overtimespent += (childTime.OvertimeSpent ?? 0);
					timebillable += (childTime.TimeBillable ?? 0);
					overtimebillable += (childTime.OvertimeBillable ?? 0);
				}

				row.TimeSpent = timespent;
				row.OvertimeSpent = overtimespent;
				row.TimeBillable = timebillable;
				row.OvertimeBillable = overtimebillable;
			}
		}

		protected virtual void Activity_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			GetNoteId(sender.Graph, sender.Graph.Caches[typeof(TPrimaryView)].Current);
			PXCache cache = sender.Graph.Caches[_refBqlField.DeclaringType];
			e.NewValue = cache.GetValue(cache.Current, _refBqlField.Name);
		}

		protected virtual void Activity_Body_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as TActivity;
			if (row == null) return;

			if (row.ClassID == CRActivityClass.Email)
			{
				var entity = (SMEmailBody)PXSelect<SMEmailBody, Where<SMEmailBody.refNoteID, Equal<Required<CRPMTimeActivity.noteID>>>>.Select(sender.Graph, row.NoteID);

				e.ReturnValue = entity.Body;
			}
		}

		#endregion
		
		protected virtual void ReadNoteIDFieldInfo(out string noteField, out Type noteBqlField)
		{
			var cache = _Graph.Caches[typeof(TPrimaryView)];
			noteField = EntityHelper.GetNoteField(typeof(TPrimaryView));
			if (string.IsNullOrEmpty(_refField))
				throw new ArgumentException(
					string.Format("Type '{0}' must contain field with PX.Data.NoteIDAttribute on it",
								  typeof(TPrimaryView).GetLongName()));
			noteBqlField = cache.GetBqlField(_refField);
		}

		protected virtual void SetCommandCondition(Delegate handler = null)
		{
			Type refID;
			Type sourceID = _refBqlField;

			if (PrimaryViewType != null && typeof(BAccount).IsAssignableFrom(PrimaryViewType))
			{
				refID = typeof(CRPMTimeActivity.bAccountID);
				sourceID = View.Graph.Caches[PrimaryViewType].GetBqlField(typeof(BAccount.bAccountID).Name);
			}
			else if (PrimaryViewType != null && typeof(Contact).IsAssignableFrom(PrimaryViewType))
			{
				refID = typeof(CRPMTimeActivity.contactID);
				sourceID = View.Graph.Caches[PrimaryViewType].GetBqlField(typeof(Contact.contactID).Name);
			}
			else
			{
				refID = typeof(CRPMTimeActivity.refNoteID);
			}

			var newCmd = OriginalCommand.WhereAnd(
				BqlCommand.Compose(typeof(Where<,>),
					refID,                  // Activity
					typeof(Equal<>),
					typeof(Current<>),
					sourceID));         // Primary

			if (handler == null)
			View = new PXView(View.Graph, View.IsReadOnly, newCmd);
			else
				View = new PXView(View.Graph, View.IsReadOnly, newCmd, handler);
		}

		protected virtual PXEntryStatus[] NotEditableStatuses
		{
			get
			{
				return new[] { PXEntryStatus.Inserted, PXEntryStatus.InsertedDeleted };
			}
		}

		protected BqlCommand OriginalCommand
		{
			get { return _originalCommand; }
		}
	}

	#endregion

	#region PMTimeActivityList

	public class PMTimeActivityList<TMasterActivity> : PXSelectBase
		where TMasterActivity : CRActivity, new()
	{
		#region Constants

		private static readonly EPSetup EmptyEpSetup = new EPSetup();

		private const string _DELETE_ACTION_NAME = "Delete";
		private const string _MARKASCOMPLETED_ACTION_NAME = "MarkAsCompleted";
		#endregion


		#region Ctor

		public PMTimeActivityList(PXGraph graph)
		{
			_Graph = graph;

			graph.RowSelected.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowSelected);
			graph.RowDeleting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowDeleting);
			graph.RowInserted.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowInserted);
			graph.RowInserting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowInserting);
			graph.RowPersisting.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowPersisting);
			graph.RowUpdated.AddHandler(typeof(PMTimeActivity), PMTimeActivity_RowUpdated);

			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.timeSpent).Name, PMTimeActivity_TimeSpent_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.trackTime).Name, PMTimeActivity_TrackTime_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(PMTimeActivity), typeof(PMTimeActivity.approvalStatus).Name, PMTimeActivity_ApprovalStatus_FieldUpdated);

			graph.RowInserted.AddHandler<TMasterActivity>(Master_RowInserted);
			graph.RowPersisting.AddHandler<TMasterActivity>(Master_RowPersisting);

			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.type).Name, Master_Type_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.ownerID).Name, Master_OwnerID_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.startDate).Name, Master_StartDate_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.subject).Name, Master_Subject_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.parentNoteID).Name, Master_ParentNoteID_FieldUpdated);

			View = new PXView(graph, false, GenerateOriginalCommand());
			ApprovalStatusAttribute.SetRestictedMode<PMTimeActivity.approvalStatus>(View.Cache, true);
		}

		#endregion

		public static BqlCommand GenerateOriginalCommand()
		{
			var createdDateTimeField = typeof(PMTimeActivity).GetNestedType(typeof(PMTimeActivity.createdDateTime).Name);
			var noteIDField = typeof(TMasterActivity).GetNestedType(typeof(CRActivity.noteID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select<,,>),
					typeof(PMTimeActivity),
				typeof(Where<,,>),
					typeof(PMTimeActivity.refNoteID), typeof(Equal<>), typeof(Current<>), noteIDField,
				typeof(And<PMTimeActivity.isCorrected, Equal<False>>),
				typeof(OrderBy<>),
					typeof(Desc<>), createdDateTimeField);
		}
		public virtual object SelectSingle(params object[] parameters)
		{
			return View.Cache.Current = View.SelectSingle(parameters);
		}

		#region Event Handlers

		protected virtual void Master_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			using (var s = new ReadOnlyScope(MainCache))
			{
				this.Current = (PMTimeActivity)MainCache.Insert();
				this.Current.ApprovalStatus = ActivityStatusListAttribute.Open;
			}
		}
		protected virtual void Master_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			if (timeActivity.TrackTime != true && e.Operation != PXDBOperation.Delete)
			{
				if (row.ClassID != CRActivityClass.Email)
				{
					var status = row.ClassID != CRActivityClass.Event && row.ClassID != CRActivityClass.Task
						? ActivityStatusAttribute.Completed
						: row.UIStatus;

					cache.SetValueExt(row, typeof(CRActivity.uistatus).Name, status);
					cache.RaiseRowUpdated(row, row);
				}
			}
		}

		protected virtual void Master_Type_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)(Current ?? MainCache.Insert());
			if (row == null || timeActivity == null) return;

			timeActivity.TrackTime = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.trackTime>(MainCache, timeActivity);

			MainCache.Update(timeActivity);
		}

		protected virtual void Master_OwnerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			timeActivity.OwnerID = row.OwnerID;

			MainCache.Update(timeActivity);
		}

		protected virtual void Master_StartDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			timeActivity.Date = (DateTime?)PXFormulaAttribute.Evaluate<PMTimeActivity.date>(MainCache, timeActivity);
			MainCache.SetDefaultExt<PMTimeActivity.weekID>(timeActivity);
			MainCache.Update(timeActivity);
		}

		protected virtual void Master_Subject_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (timeActivity == null) return;

			MainCache.MarkUpdated(timeActivity);
		}

		protected virtual void Master_ParentNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			PMTimeActivity timeActivity = (PMTimeActivity)Current;
			if (row == null || timeActivity == null) return;

			var item = (PXResult<CRActivity, PMTimeActivity>)
				PXSelectJoin<CRActivity,
					InnerJoin<PMTimeActivity,
						On<PMTimeActivity.isCorrected, Equal<False>,
						And<CRActivity.noteID, Equal<PMTimeActivity.refNoteID>>>>,
					Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
					.Select(_Graph, row.ParentNoteID);

			CRActivity parent = item;
			PMTimeActivity timeParent = item;
			if (timeParent != null)
			{
				timeActivity.ProjectID = timeParent.ProjectID;
				timeActivity.ProjectTaskID = timeParent.ProjectTaskID;
			}

			timeActivity.ParentTaskNoteID =
				parent != null && parent.ClassID == CRActivityClass.Task
					? parent.NoteID
					: null;

			MainCache.Update(timeActivity);
		}

		protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PMTimeActivity row = (PMTimeActivity)e.Row;
			TMasterActivity masterAct = (TMasterActivity)MasterCache.Current;
            PXUIFieldAttribute.SetDisplayName<PMTimeActivity.approvalStatus>(cache, Data.EP.Messages.Status);

            if (row == null || masterAct == null) return;

            // TimeActivity
            bool wasUsed = !string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true;

			string origTimeStatus;

			if (masterAct.ClassID == CRActivityClass.Task || masterAct.ClassID == CRActivityClass.Event)
			{
				origTimeStatus =
					(string)MasterCache.GetValueOriginal<CRActivity.uistatus>(masterAct)
					?? ActivityStatusListAttribute.Open;
			}
			else
			{
				origTimeStatus =
					(string)cache.GetValueOriginal<PMTimeActivity.approvalStatus>(row)
					?? ActivityStatusListAttribute.Open;
			}

			if (origTimeStatus == ActivityStatusListAttribute.Open)
			{
				PXUIFieldAttribute.SetEnabled(cache, row, true);

				PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, !wasUsed && row?.IsBillable == true);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectID>(cache, row, !wasUsed);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectTaskID>(cache, row, !wasUsed);
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.trackTime>(cache, row, !wasUsed);
			}
			else
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
			}

            PXUIFieldAttribute.SetEnabled<PMTimeActivity.approvalStatus>(cache, row, row.TrackTime == true && !wasUsed && row.Released != true);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.released>(cache, row, false);

			if (row.Released == true && row.ARRefNbr == null)
			{
				CRCase crCase = PXSelect<CRCase,
					Where<CRCase.noteID, Equal<Required<CRActivity.refNoteID>>>>.Select(_Graph, masterAct.RefNoteID);

				if (crCase != null && crCase.ARRefNbr != null)
				{
					ARInvoice invoice = (ARInvoice)PXSelectorAttribute.Select<CRCase.aRRefNbr>(_Graph.Caches<CRCase>(), crCase);
					row.ARRefNbr = invoice.RefNbr;
					row.ARDocType = invoice.DocType;
				}
				if (row.ARRefNbr == null)
				{
					PMTran pmTran = PXSelect<PMTran,
						Where<PMTran.origRefID, Equal<Required<CRActivity.noteID>>>>.Select(_Graph, masterAct.NoteID);

					if (pmTran != null)
					{
						row.ARDocType = pmTran.ARTranType;
						row.ARRefNbr = pmTran.ARRefNbr;
					}
				}
			}
		}

		protected virtual void PMTimeActivity_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			var row = (PMTimeActivity)e.Row;
			if (row == null) return;

			if (!string.IsNullOrEmpty(row.TimeCardCD) || row.Billed == true)
				throw new PXException(EP.Messages.ActivityIsBilled);
		}

		protected virtual void PMTimeActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			row.RefNoteID = activity.NoteID;
			row.OwnerID = activity.OwnerID;
			cache.RaiseFieldUpdated<PMTimeActivity.approvalStatus>(row, null);
			cache.RaiseFieldUpdated<PMTimeActivity.ownerID>(row, null);
			if (activity.ParentNoteID != null)
			{
				var item = (PXResult<CRActivity, PMTimeActivity>)
					PXSelectJoin<CRActivity,
						InnerJoin<PMTimeActivity,
							On<PMTimeActivity.isCorrected, Equal<False>,
							And<CRActivity.noteID, Equal<PMTimeActivity.refNoteID>>>>,
						Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>
						.Select(_Graph, activity.ParentNoteID);

				CRActivity parent = item;
				PMTimeActivity timeParent = item;

				if (timeParent != null && timeParent.RefNoteID != null &&
					(timeParent.ProjectID != null || timeParent.ProjectTaskID != null) && 
					(row.ProjectID == null || ProjectDefaultAttribute.IsNonProject(row.ProjectID)))
				{
					row.ProjectID = timeParent.ProjectID;
					row.ProjectTaskID = timeParent.ProjectTaskID;
					row.CostCodeID = timeParent.CostCodeID;

					cache.RaiseFieldUpdated<PMTimeActivity.projectTaskID>(row, null);
				}

				row.ParentTaskNoteID =
					parent != null && parent.ClassID == CRActivityClass.Task
						? parent.NoteID
						: null;
			}
		}

		protected virtual void PMTimeActivity_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null) return;

			if ((cache.Inserted.GetEnumerator() is IEnumerator insEnum) && insEnum.MoveNext())
			{
				cache.SetStatus(insEnum.Current, PXEntryStatus.InsertedDeleted);
			}
			else if ((cache.Updated.GetEnumerator() is IEnumerator updEnum) && updEnum.MoveNext())
			{
				cache.SetStatus(updEnum.Current, PXEntryStatus.InsertedDeleted);
			}
			else if ((cache.Deleted.GetEnumerator() is IEnumerator delEnum) && delEnum.MoveNext())
			{
				row.NoteID = ((PMTimeActivity)delEnum.Current).NoteID;
			}
		}

		protected virtual void PMTimeActivity_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			cache.SetValue<PMTimeActivity.summary>(row, activity.Subject);

			if (row.NoteID == row.RefNoteID)
			{
				cache.SetValue<PMTimeActivity.noteID>(row, SequentialGuid.Generate());
				cache.Normalize();
			}

			if (activity.ClassID == CRActivityClass.Task || activity.ClassID == CRActivityClass.Event)
				cache.SetValue<PMTimeActivity.trackTime>(row, false);

			row.NeedToBeDeleted = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.needToBeDeleted>(cache, row);

			if (row.NeedToBeDeleted == true && e.Operation != PXDBOperation.Delete)
			{
				e.Cancel = true;
			}
			else
			{
				if (row.TrackTime != true)
				{
					if (activity.UIStatus == ActivityStatusListAttribute.Completed)
						row.ApprovalStatus = ActivityStatusListAttribute.Completed;
					else if (activity.UIStatus == ActivityStatusListAttribute.Canceled)
						row.ApprovalStatus = ActivityStatusListAttribute.Canceled;
					else
					row.ApprovalStatus = ActivityStatusListAttribute.Open;
				}
				else
					if (row.ApprovalStatus == ActivityStatusListAttribute.Completed &&
						row.ApproverID != null)
					row.ApprovalStatus = ActivityStatusListAttribute.PendingApproval;
			}
		}

		protected virtual void PMTimeActivity_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null || _Graph.IsContractBasedAPI) return;

			var isInDB = cache.GetValueOriginal<PMTimeActivity.noteID>(row) != null;

			row.NeedToBeDeleted = (bool?)PXFormulaAttribute.Evaluate<PMTimeActivity.needToBeDeleted>(cache, row);

			if (row.NeedToBeDeleted == true)
			{
				if (!isInDB)
					cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
				else
					cache.SetStatus(row, PXEntryStatus.Deleted);
			}
			else if (cache.GetStatus(row) == PXEntryStatus.Updated && !isInDB)
			{
				// means "is not in DB", so move from updated to inserted
				cache.SetStatus(row, PXEntryStatus.Inserted);
			}
		}

		protected virtual void PMTimeActivity_ApprovalStatus_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity master = (TMasterActivity)MasterCache.Current;
			if (row == null || master == null) return;
			if (row.TrackTime == true && master.IsLocked != true && master.UIStatus != ActivityStatusListAttribute.Draft)
			{
				TMasterActivity activity = (TMasterActivity)MasterCache.CreateCopy(master);
				switch (row.ApprovalStatus)
				{
					case ActivityStatusListAttribute.Open:
						activity.UIStatus = ActivityStatusListAttribute.Open;
						break;
					case ActivityStatusListAttribute.Canceled:
						activity.UIStatus = ActivityStatusListAttribute.Canceled;
						break;
					default:
						activity.UIStatus = ActivityStatusListAttribute.Completed;
						break;
				}
				if (master.UIStatus != activity.UIStatus)
					MasterCache.Update(activity);
			}
		}
		protected virtual void PMTimeActivity_TimeSpent_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (activity.StartDate != null)
			{
				MasterCache.SetValue(activity, typeof(CRActivity.endDate).Name, row.TimeSpent != null
					? (DateTime?)((DateTime)activity.StartDate).AddMinutes((int)row.TimeSpent)
					: null);
			}
		}

		protected virtual void PMTimeActivity_TrackTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (row.TrackTime != true)
			{
				cache.SetValue<PMTimeActivity.timeSpent>(row, 0);
				cache.SetValue<PMTimeActivity.timeBillable>(row, 0);
				cache.SetValue<PMTimeActivity.overtimeSpent>(row, 0);
				cache.SetValue<PMTimeActivity.overtimeBillable>(row, 0);
				cache.SetValue<PMTimeActivity.approvalStatus>(row, ActivityStatusAttribute.Completed);
				MasterCache.SetValue(activity, typeof(CRActivity.uistatus).Name, ActivityStatusAttribute.Completed);
			}
		}

		#endregion

		private PXCache MainCache
		{
			get { return _Graph.Caches[typeof(PMTimeActivity)]; }
		}

		private PXCache MasterCache
		{
			get { return _Graph.Caches[typeof(TMasterActivity)]; }
		}

		public PMTimeActivity Current
		{
			get { return (PMTimeActivity)View.Cache.Current; }
			set { View.Cache.Current = value; }
		}

		private EPSetup EPSetupCurrent
		{
			get
			{
				var res = (EPSetup)PXSelect<EPSetup>.
					SelectWindowed(_Graph, 0, 1);
				return res ?? EmptyEpSetup;
			}
		}

	}

	#endregion

	#region CRReminderList

	public class CRReminderList<TMasterActivity> : PXSelectBase
		where TMasterActivity : CRActivity, new()
	{
		#region Ctor

		public CRReminderList(PXGraph graph)
		{
			_Graph = graph;

			View = new PXView(graph, false, GenerateOriginalCommand());

			graph.RowInserted.AddHandler<TMasterActivity>(Master_RowInserted);

			graph.FieldUpdated.AddHandler(typeof(TMasterActivity), typeof(CRActivity.ownerID).Name, Master_OwnerID_FieldUpdated);

			graph.RowSelected.AddHandler<CRReminder>(CRReminder_RowSelected);
			graph.RowInserting.AddHandler<CRReminder>(CRReminder_RowInserting);
			graph.RowInserted.AddHandler<CRReminder>(CRReminder_RowInserted);
			graph.RowPersisting.AddHandler<CRReminder>(CRReminder_RowPersisting);
			graph.RowUpdated.AddHandler<CRReminder>(CRReminder_RowUpdated);

			graph.FieldUpdated.AddHandler<CRReminder.isReminderOn>(CRReminder_IsReminderOn_FieldUpdated);

			PXUIFieldAttribute.SetEnabled<CRReminder.reminderDate>(MainCache, null, false);

			graph.EnsureCachePersistence(typeof(CRReminder));
		}

		public virtual object SelectSingle(params object[] parameters)
		{
			return View.SelectSingle(parameters);
		}

		#endregion

		protected virtual void Master_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			using (var r = new ReadOnlyScope(MainCache))
				this.Current = (CRReminder)MainCache.Insert();
		}

		protected virtual void Master_OwnerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as TMasterActivity;
			CRReminder reminder = (CRReminder)Current;
			if (row == null || reminder == null) return;

            MainCache.SetValueExt<CRReminder.owner>(reminder, row.OwnerID);

            if (!cache.Graph.UnattendedMode)
			{
				var value = row.CreatedByID != row.OwnerID;

				MainCache.SetValueExt<CRReminder.isReminderOn>(reminder, value);
			}
		}

		protected virtual void CRReminder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if ((string)MasterCache.GetValueOriginal(activity, typeof(CRActivity.uistatus).Name) != ActivityStatusAttribute.Completed)
			{
				PXUIFieldAttribute.SetEnabled<CRReminder.reminderDate>(cache, row, row.IsReminderOn == true);
				return;
			}

			PXUIFieldAttribute.SetEnabled(cache, row, false);
		}

		protected virtual void CRReminder_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			var delEnum = cache.Deleted.GetEnumerator();
			if (delEnum.MoveNext())
			{
				row.NoteID = ((CRReminder)delEnum.Current).NoteID;
			}
		}

		protected virtual void CRReminder_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			row.RefNoteID = activity.NoteID;
			row.Owner = activity.OwnerID;
		}

		protected virtual void CRReminder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRReminder;
			if (row == null) return;



			if (row.IsReminderOn != true && e.Operation != PXDBOperation.Delete)
			{
				e.Cancel = true;
			}

			if (row.IsReminderOn == true && row.ReminderDate == null)
			{
				var reminderDateDisplayName = PXUIFieldAttribute.GetDisplayName<CRReminder.reminderDate>(MainCache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				if (MainCache.RaiseExceptionHandling<CRReminder.reminderDate>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(CRReminder.reminderDate).Name, null, ErrorMessages.FieldIsEmpty, reminderDateDisplayName);
				}
			}
		}

		protected virtual void CRReminder_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRReminder;
			if (row == null) return;

			var isInDB = cache.GetValueOriginal<CRReminder.noteID>(row) != null;

			if (row.IsReminderOn != true)
			{
				if (!isInDB)
					cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
				else
					cache.SetStatus(row, PXEntryStatus.Deleted);
			}
			else if (cache.GetStatus(row) == PXEntryStatus.Updated && !isInDB)
			{
				// means "is not in DB", so move from updated to inserted
				cache.SetStatus(row, PXEntryStatus.Inserted);
			}
		}

		protected virtual void CRReminder_IsReminderOn_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as CRReminder;
			TMasterActivity activity = (TMasterActivity)MasterCache.Current;
			if (row == null || activity == null) return;

			if (activity.ClassID == CRActivityClass.Task)
			{
				if (row.IsReminderOn == true)
				{
					cache.SetValue<CRReminder.reminderDate>(row, row.ReminderDate ?? activity.StartDate?.AddMinutes(15) ?? row.LastModifiedDateTime?.AddMinutes(15));
				}
			}

			if (activity.ClassID == CRActivityClass.Event)
			{
				if (row.IsReminderOn == true)
				{
					cache.SetValue<CRReminder.reminderDate>(row, row.ReminderDate ?? activity.StartDate?.AddMinutes(-15) ?? row.LastModifiedDateTime?.AddMinutes(15));
				}
			}
		}

		public static BqlCommand GenerateOriginalCommand()
		{
			var createdDateTimeField = typeof(CRReminder).GetNestedType(typeof(CRReminder.createdDateTime).Name);
			var noteIDField = typeof(TMasterActivity).GetNestedType(typeof(CRActivity.noteID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select<,,>),
					typeof(CRReminder),
				typeof(Where<,>),
					typeof(CRReminder.refNoteID), typeof(Equal<>), typeof(Current<>), noteIDField,
				typeof(OrderBy<>),
					typeof(Desc<>), createdDateTimeField);
		}

		private PXCache MainCache
		{
			get { return _Graph.Caches[typeof(CRReminder)]; }
		}

		private PXCache MasterCache
		{
			get { return _Graph.Caches[typeof(TMasterActivity)]; }
		}

		public CRReminder Current
		{
			get { return (CRReminder)View.Cache.Current; }
			set { View.Cache.Current = value; }
		}
	}

	#endregion

	#region CRReferencedTaskList

	public class CRReferencedTaskList<TPrimaryView> : PXSelectBase
		where TPrimaryView : class, IBqlTable
	{
		private const string _VIEWTASK_COMMAND = "ViewTask";

		public CRReferencedTaskList(PXGraph graph)
		{
			_Graph = graph;

			AddActions();
			AddEventHandlers();

			View = new PXView(graph, false, GetCommand(), new PXSelectDelegate(Handler));
			_Graph.EnsureCachePersistence(typeof(CRActivityRelation));
		}

		private void AddEventHandlers()
		{
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.subject>((sender, e) => e.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.startDate>((sender, e) => e.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.endDate>((sender, e) => e.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.completedDateTime>((sender, e) => e.Cancel = true);
			_Graph.FieldUpdating.AddHandler<CRActivityRelation.status>((sender, e) => e.Cancel = true);
			_Graph.FieldDefaulting.AddHandler<CRActivityRelation.parentNoteID>(
				(sender, e) =>
				{
					var cache = _Graph.Caches[typeof(CRActivity)];
					var noteIDFieldName = cache.GetField(typeof(CRActivity).GetNestedType(typeof(CRActivity.noteID).Name));
					e.NewValue = cache.GetValue(cache.Current, noteIDFieldName);
				});
			_Graph.RowInserted.AddHandler<CRActivityRelation>((sender, e) => FillReadonlyValues(e.Row as CRActivityRelation));
			_Graph.RowUpdated.AddHandler<CRActivityRelation>((sender, e) => FillReadonlyValues(e.Row as CRActivityRelation));
			_Graph.RowPersisting.AddHandler<CRActivityRelation>(
				(sender, e) =>
				{
					if ((e.Row as CRActivityRelation).With(_ => _.RefNoteID) == null) e.Cancel = true;
				});
			_Graph.RowSelected.AddHandler<TPrimaryView>(
				(sender, e) =>
				{
					var tasksCache = sender.Graph.Caches[typeof(CRActivityRelation)];
					var isInserted = sender.GetStatus(e.Row) == PXEntryStatus.Inserted;
					var row = e.Row as CRActivity;
					var isEditable = row != null && row.UIStatus == ActivityStatusAttribute.Open;

					tasksCache.AllowDelete =
						tasksCache.AllowUpdate =
							tasksCache.AllowInsert =
								!isInserted && isEditable;
				});
		}

		private void FillReadonlyValues(CRActivityRelation row)
		{
			if (row == null || row.RefNoteID == null) return;

			var act = (CRActivity)PXSelect<CRActivity,
						Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.
						Select(_Graph, row.RefNoteID);
			if (act != null && act.NoteID != null)
			{
				row.Subject = act.Subject;
				row.StartDate = act.StartDate;
				row.EndDate = act.EndDate;
				row.Status = act.UIStatus;
				row.CompletedDateTime = act.CompletedDate;
			}
		}

		private void AddActions()
		{
			AddAction(_Graph, _VIEWTASK_COMMAND, Messages.ViewActivity, ViewTask);
		}

		private PXAction AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			var uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Select
			};
			var res = (PXAction)Activator.CreateInstance(typeof(PXNamedAction<>).MakeGenericType(
				new[] { typeof(TPrimaryView) }), new object[] { graph, name, handler, new PXEventSubscriberAttribute[] { uiAtt } });
			graph.Actions[name] = res;
			return res;
		}

		private IEnumerable Handler()
		{
			BqlCommand command = View.BqlSelect;
			var startRow = PXView.StartRow;
			int totalRows = 0;
			var list = new PXView(PXView.CurrentGraph, false, command).
				Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
					   ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;
			foreach (PXResult<CRActivityRelation, CRActivity> record in list)
			{
				var row = (CRActivityRelation)record[typeof(CRActivityRelation)];
				var act = (CRActivity)record[typeof(CRActivity)];
				var status = View.Cache.GetStatus(row);

				if (status == PXEntryStatus.Inserted || status == PXEntryStatus.Updated)
				{
					act = PXSelect<CRActivity,
						Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.
						Select(_Graph, row.RefNoteID);
				}

				if (act != null && act.NoteID != null)
				{
					row.Subject = act.Subject;
					row.StartDate = act.StartDate;
					row.EndDate = act.EndDate;
					row.Status = act.UIStatus;
					row.CompletedDateTime = act.CompletedDate;
				}
				yield return row;
			}
		}

		private static BqlCommand GetCommand()
		{
			Type noteID = typeof(TPrimaryView).GetNestedType(typeof(CRActivity.noteID).Name);

			return BqlCommand.CreateInstance(
				typeof(Select2<,,,>),
					typeof(CRActivityRelation),
				typeof(InnerJoin<,>),
					typeof(CRActivity), typeof(On<,>), typeof(CRActivity.noteID), typeof(Equal<>), typeof(CRActivityRelation.refNoteID),
				typeof(Where<,>),
					typeof(CRActivityRelation.parentNoteID), typeof(Equal<>), typeof(Current<>), noteID,
				typeof(OrderBy<>),
					typeof(Asc<>), typeof(CRActivityRelation.refNoteID));
		}

		[PXUIField(Visible = false)]
		[PXButton]
		public virtual IEnumerable ViewTask(PXAdapter adapter)
		{
			var current = (CRActivity)PXSelect<CRActivity,
				Where<CRActivity.noteID, Equal<Current<CRActivityRelation.refNoteID>>>>.
				Select(_Graph);
			if (current != null)
				PXRedirectHelper.TryRedirect(_Graph.Caches[typeof(CRActivity)].Graph, current, PXRedirectHelper.WindowMode.Same);

			return adapter.Get();
		}

		protected PXCache CreateInstanceCache<TNode>(Type graphType)
			where TNode : IBqlTable
		{
			if (graphType != null)
			{
				if (_Graph.IsDirty)
					_Graph.Actions.PressSave();

				var graph = PXGraph.CreateInstance(graphType);
				graph.Clear();
				foreach (Type type in graph.Views.Caches)
				{
					var cache = graph.Caches[type];
					if (typeof(TNode).IsAssignableFrom(cache.GetItemType()))
						return cache;
				}
			}
			return null;
		}
	}

	#endregion

	#region Email

	[Obsolete("Will be removed in 7.0 version")]
	public struct Email
	{
		private static Regex _emailRegex = new Regex("(?<Name>[^;]+?)\\s*\\((?<Address>[^;]*?)\\)",
			RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

		//private readonly string _address;
		private readonly string[] _addresses;
		private readonly string _displayName;

		private string _toString;

		public static readonly Email Empty = new Email(null, null);

		public Email(string displayName, string address)
		{
			_displayName = displayName;
			if (_displayName != null) _displayName = _displayName.Replace(';', ' '); //.Replace(',', ' ');
			_addresses = address != null ? address.Trim('<', '>').Split(';').Select(_ => _.Trim()).ToArray() : null ;

			_toString = null;
		}

		public string Address
		{
			get { return (_addresses?.Length??0)>1? "<" +string.Join("; ", _addresses) + ">" : _addresses.FirstOrDefault(); }
		}

		public string DisplayName
		{
			get { return _displayName; }
		}

		public override string ToString()
		{
			if (_toString == null)
			{
				//var isAddressComplex = _emailRegex.IsMatch(_address);
				//_toString = isAddressComplex || string.IsNullOrEmpty(_displayName)
				//				? CorrectEmailAddress(_address)
				//				: TextUtils.QuoteString(_displayName) + " <" + _address + ">";
				string dName = _displayName;
				_toString = string.Join("; ",  _addresses.Select(_ => dName + "<" + _ + ">"));
			}
			return _toString;
		}

		private static string CorrectEmailAddress(string address)
		{
			var sb = new StringBuilder(address.Length * 2);
			foreach (string str in address.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var cStr = str.Trim();
				if (string.IsNullOrEmpty(cStr)) continue;
				if (_emailRegex.IsMatch(address))
				{
					sb.Append(cStr);
				}
				else
				{
					var value = cStr.Replace('<', '(').Replace('>', ')');
					sb.AppendFormat("{0} <{0}>", value);
				}
				sb.Append(';');
			}
			return sb.ToString();
		}
	}

	#endregion

	#region NotificationUtility

	public sealed class NotificationUtility
	{
		private readonly PXGraph _graph;

		public NotificationUtility(PXGraph graph)
		{
			_graph = graph;
		}

		public Guid? SearchSetupID(string source, string notificationCD)
		{
			return SearchSetup(source,notificationCD)?.SetupID;
		}

		public NotificationSetup SearchSetup(string source, string notificationCD)
		{
			if (source == null || notificationCD == null) return null;
			NotificationSetup setup =
				PXSelect<NotificationSetup,
						Where<NotificationSetup.sourceCD, Equal<Required<NotificationSetup.sourceCD>>,
							And<NotificationSetup.notificationCD, Equal<Required<NotificationSetup.notificationCD>>>>>
					.SelectWindowed(_graph, 0, 1, source, notificationCD);
			return setup;
		}


		public string SearchReport(string source, object row, string reportID, int? branchID)
		{
			if (source == null) return reportID;
			NotificationSetup setup = GetSetup(source, reportID, branchID, false);

			if (setup == null) return reportID;

			NotificationSource notification = GetSource(source, row, (Guid)setup.SetupID, branchID);

			return notification == null || notification.ReportID == null
				? reportID :
				notification.ReportID;
		}

		public Guid? SearchPrinter(string source, string reportID, int? branchID)
		{
			NotificationSetupUserOverride userSetup =
				SelectFrom<NotificationSetupUserOverride>
				.InnerJoin<NotificationSetup>.On<NotificationSetupUserOverride.FK.DefaultSetup>
				.Where<NotificationSetupUserOverride.userID.IsEqual<AccessInfo.userID.FromCurrent>
					.And<NotificationSetupUserOverride.active.IsEqual<True>>
					.And<NotificationSetupUserOverride.shipVia.IsNull>
					.And<NotificationSetup.active.IsEqual<True>>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.Select(_graph, source, reportID, branchID);
			if (userSetup?.DefaultPrinterID != null)
				return userSetup.DefaultPrinterID;

			UserPreferences userPreferences = SelectFrom<UserPreferences>.Where<UserPreferences.userID.IsEqual<@P.AsGuid>>.View.Select(_graph, _graph.Accessinfo.UserID);
			if (userPreferences?.DefaultPrinterID != null)
				return userPreferences.DefaultPrinterID;

			if (source != null && reportID != null)
			{
				NotificationSetup setup = GetSetup(source, reportID, branchID);
				if (setup?.DefaultPrinterID != null)
					return setup.DefaultPrinterID;
			}

			GL.Branch branch = SelectFrom<GL.Branch>.Where<GL.Branch.branchID.IsEqual<@P.AsInt>>.View.Select(_graph, branchID ?? _graph.Accessinfo.BranchID);
			if (branch != null)
			{
				if (branch.DefaultPrinterID != null)
					return branch.DefaultPrinterID;

				GL.DAC.Organization organization = SelectFrom<GL.DAC.Organization>.Where<GL.DAC.Organization.organizationID.IsEqual<@P.AsInt>>.View.Select(_graph, branch.OrganizationID);
					if (organization != null)
						return organization.DefaultPrinterID;
				}

			return null;
		}

		public NotificationSetup GetSetup(string source, string reportID, int? branchID)
		{
			NotificationSetup setup =
				SelectFrom<NotificationSetup>
				.Where<NotificationSetup.active.IsEqual<True>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.shipVia.IsNull>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.SelectWindowed(_graph, 0, 1, source, reportID, branchID);
			return setup;
		}

		public NotificationSetup GetSetup(string source, string reportID, int? branchID, bool shipViaIsNull = true)
		{
			NotificationSetup setup;
			setup = shipViaIsNull ?
				SelectFrom<NotificationSetup>
				.Where<NotificationSetup.active.IsEqual<True>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.shipVia.IsNull>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.SelectWindowed(_graph, 0, 1, source, reportID, branchID) 
				: SelectFrom<NotificationSetup>
				.Where<NotificationSetup.active.IsEqual<True>
					.And<NotificationSetup.sourceCD.IsEqual<@P.AsString>>
					.And<NotificationSetup.reportID.IsEqual<@P.AsString>>
					.And<NotificationSetup.nBranchID.IsEqual<@P.AsInt>.Or<NotificationSetup.nBranchID.IsNull>>>
				.OrderBy<NotificationSetup.nBranchID.Desc>
				.View.SelectWindowed(_graph, 0, 1, source, reportID, branchID); ;
			return setup;
		}
		public NotificationSource GetSource(NotificationSetup setup)
		{
			NotificationSource result = new NotificationSource();
			result.Active = setup.Active;
			result.EMailAccountID = setup.EMailAccountID;
			result.ReportID = setup.ReportID;
			result.NotificationID = setup.NotificationID;
			result.Format = setup.Format;
			return result;
		}
		public NotificationSource GetSource(object row, Guid setupID, int? branchID)
		{
			return GetSource(null, row, setupID, branchID);
		}

		public NotificationSource GetSource(string sourceType, object row, Guid setupID, int? branchID)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(sourceType, row);
			NavigateRow(graph, row);

			PXView notificationView = null;
			graph.Views.TryGetValue("NotificationSources", out notificationView);

			if (notificationView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof(NotificationSource).IsAssignableFrom(view.GetItemType())))
				{
					notificationView = view;
					break;
				}
			}
			if (notificationView == null) return null;
			NotificationSource result = null;
			foreach (NotificationSource rec in notificationView.SelectMulti().RowCast<NotificationSource>())
			{
				if (rec.SetupID == setupID && rec.NBranchID == branchID)
					return rec;
				if (rec.SetupID == setupID && rec.NBranchID == null)
					result = rec;
			}
			return result;
		}

		public RecipientList GetRecipients(object row, NotificationSource source)
		{
			return GetRecipients(null, row, source);
		}

		public RecipientList GetRecipients(string type, object row, NotificationSource source)
		{
			if (row == null) return null;
			PXGraph graph = CreatePrimaryGraph(type, row);
			NavigateRow(graph, row);
			NavigateRow(graph, source, false);


			PXView recipientView;
			graph.Views.TryGetValue("NotificationRecipients", out recipientView);

			if (recipientView == null)
			{
				foreach (PXView view in graph.GetViewNames().Select(name => graph.Views[name]).Where(view => typeof(NotificationRecipient).IsAssignableFrom(view.GetItemType())))
				{
					recipientView = view;
					break;
				}
			}
			if (recipientView != null)
			{
				RecipientList recipient = null;
				Dictionary<string, string> errors = new Dictionary<string, string>();
				int count = 0;
				foreach (NotificationRecipient item in recipientView.SelectMulti())
				{
					NavigateRow(graph, item, false);
					if (item.Active == true)
					{
						count++;
						if (string.IsNullOrWhiteSpace(item.Email))
						{
							string currEmail = ((NotificationRecipient)graph.Caches[typeof(NotificationRecipient)].Current).Email;
							if (string.IsNullOrWhiteSpace(currEmail))
							{
								Contact contact = PXSelect<Contact, Where<Contact.contactID, Equal<Current<NotificationRecipient.contactID>>>>.SelectSingleBound(_graph, new object[] { item });
								NotificationContactType.ListAttribute list = new NotificationContactType.ListAttribute();
								StringBuilder display = new StringBuilder(list.ValueLabelDic[item.ContactType]);
								if (contact != null)
								{
									display.Append(" ");
									display.Append(contact.DisplayName);
								}
								errors.Add(count.ToString(CultureInfo.InvariantCulture), PXMessages.LocalizeFormatNoPrefix(Messages.EmptyEmail, display));
							}
							else
							{
								item.Email = currEmail;
							}
						}
						if (!string.IsNullOrWhiteSpace(item.Email))
						{
							if (recipient == null)
								recipient = new RecipientList();
							recipient.Add(item);
						}
					}
				}
				if (errors.Any())
				{
					NotificationSetup nsetup = PXSelect<NotificationSetup, Where<NotificationSetup.setupID, Equal<Current<NotificationSource.setupID>>>>.SelectSingleBound(_graph, new object[] { source });
					throw new PXOuterException(errors, _graph.GetType(), row, Messages.InvalidRecipients, errors.Count, count, nsetup.NotificationCD, nsetup.Module);
				}
				else
				{
					return recipient;
				}
			}
			return null;
		}

		private PXGraph CreatePrimaryGraph(string source, object row)
		{
			Type graphType = null;
			if (source == ARNotificationSource.Customer)
				graphType = typeof(CustomerMaint);
			else if (source == APNotificationSource.Vendor)
			{
				graphType = typeof(VendorMaint);
				if (row != null)
				{
					PXCache cache = _graph.Caches[row.GetType()];
					if(cache.GetValue<BAccount.type>(row) as string == BAccountType.EmployeeType)
						graphType = typeof(EmployeeMaint);
				}
			}
			else
				graphType = new EntityHelper(_graph).GetPrimaryGraphType(row, false);

			if (graphType == null)
				throw new PXException(PX.SM.Messages.NotificationGraphNotFound);

			var res = graphType == _graph.GetType()
				? _graph
				: (PXGraph)PXGraph.CreateInstance(graphType);
			return res;
		}

		private static void NavigateRow(PXGraph graph, object row, bool primaryView = true)
		{
			Type type = row.GetType();
			PXCache primary = graph.Views[graph.PrimaryView].Cache;
			if (primary.GetItemType().IsAssignableFrom(row.GetType()))
			{
				graph.Caches[type].Current = row;
				graph.Caches[primary.GetItemType()].Current = row;
			}
			else if (row.GetType().IsAssignableFrom(primary.GetItemType()))
			{
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(row.GetType()), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
			else
			if (primaryView)
			{
				object[] searches = new object[primary.Keys.Count];
				string[] sortcolumns = new string[primary.Keys.Count];
				for (int i = 0; i < primary.Keys.Count; i++)
				{
					searches[i] = graph.Caches[type].GetValue(row, primary.Keys[i]);
					sortcolumns[i] = primary.Keys[i];
				}
				int startRow = 0, totalRows = 0;
				var list = graph.Views[graph.PrimaryView].Select(null, null, searches, sortcolumns,null, null, ref startRow, 1, ref totalRows);				
				graph.Views[graph.PrimaryView].Cache.Current = 
					(list != null && list.Count > 0)
					? PXResult.Unwrap(list[0], graph.Views[graph.PrimaryView].Cache.GetItemType())
					: null;				
			}
			else
			{
				primary = graph.Caches[type];
				object current = primary.CreateInstance();
				PXCache parent = (PXCache)Activator.CreateInstance(typeof(PXCache<>).MakeGenericType(type), primary.Graph);
				parent.RestoreCopy(current, row);
				primary.Current = current;
			}
		}
	}

	#endregion

	#region CRNotificationSetupList

	public class CRNotificationSetupList<Table> : PXSelect<Table>
		where Table : NotificationSetup, new()
	{
		public CRNotificationSetupList(PXGraph graph)
			: base(graph)
		{
			graph.Views.Caches.Add(typeof(NotificationSource));
			graph.Views.Caches.Add(typeof(NotificationRecipient));
			graph.RowDeleted.AddHandler(typeof(Table), OnRowDeleted);
			graph.RowPersisting.AddHandler(typeof(Table), OnRowPersisting);
		}

		protected virtual void OnRowDeleted(PXCache cache, PXRowDeletedEventArgs e)
		{
			NotificationSetup row = (NotificationSetup)e.Row;

			PXCache source = cache.Graph.Caches[typeof(NotificationSource)];
			foreach (NotificationSource item in
				PXSelect<NotificationSource,
			Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				source.Delete(item);
			}

			PXCache recipient = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationRecipient item in
				PXSelect<NotificationRecipient,
			Where<NotificationRecipient.setupID, Equal<Required<NotificationRecipient.setupID>>>>.Select(cache.Graph, row.SetupID))
			{
				recipient.Delete(item);
			}
		}

		protected virtual void OnRowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			NotificationSetup row = (NotificationSetup)e.Row;
			if (row != null && row.NotificationCD == null)
			{
				cache.RaiseExceptionHandling<NotificationSetup.notificationCD>(e.Row, row.NotificationCD,
					new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(Messages.EmptyValueErrorFormat, PXUIFieldAttribute.GetDisplayName<NotificationSetup.notificationCD>(cache)),
						PXErrorLevel.RowError, typeof(NotificationSetup.notificationCD).Name));
			}
		}

	}

	#endregion

	#region CRClassNotificationSourceList

	public class CRClassNotificationSourceList<Table, ClassID, SourceCD> : PXSelect<Table>
		where Table : NotificationSource, new()
		where ClassID : IBqlField
		where SourceCD : IConstant<string>, IBqlOperand
	{
		public CRClassNotificationSourceList(PXGraph graph)
			: base(graph)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select2<,,,>), typeof(Table),
				typeof(InnerJoin<,>), typeof(NotificationSetup),
				typeof(On<,,>), typeof(NotificationSetup.setupID), typeof(Equal<>), typeof(Table).GetNestedType(typeof(NotificationSource.setupID).Name),
				typeof(And<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD),
				typeof(Where<,>), typeof(Table).GetNestedType(typeof(NotificationSource.classID).Name), typeof(Equal<>), typeof(Optional<>), typeof(ClassID),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(NotificationSetup.notificationCD))));

			this.setupNotifications =
				new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSetup),
				typeof(Where<,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(SourceCD))));

			graph.RowInserted.AddHandler(BqlCommand.GetItemType(typeof(ClassID)), OnClassRowInserted);
			graph.RowUpdated.AddHandler(BqlCommand.GetItemType(typeof(ClassID)), OnClassRowUpdated);
			graph.RowInserted.AddHandler<NotificationSource>(OnSourceRowInseted);
		}
		private PXView setupNotifications;
		protected virtual void OnClassRowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (e.Row == null || cache.GetValue(e.Row, typeof(ClassID).Name) == null) return;

			foreach (NotificationSetup n in setupNotifications.SelectMulti())
			{
				Table source = new Table();
				source.SetupID = n.SetupID;
				this.Cache.Insert(source);
			}
		}
		public virtual void OnClassRowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			if (cache.Graph.IsCopyPasteContext)
			{
				foreach (object source in this.Select())
				{
					this.Cache.Delete(source);
				}
			}
		}
		protected virtual void OnSourceRowInseted(PXCache cache, PXRowInsertedEventArgs e)
		{
			if (cache.Graph.IsCopyPasteContext)
				return;
			NotificationSource source = (NotificationSource)e.Row;
			PXCache rCache = cache.Graph.Caches[typeof(NotificationRecipient)];
			foreach (NotificationSetupRecipient r in
					PXSelect<NotificationSetupRecipient,
					Where<NotificationSetupRecipient.setupID, Equal<Required<NotificationSetupRecipient.setupID>>>>
					.Select(cache.Graph, source.SetupID))
			{
				try
				{
					NotificationRecipient rec = (NotificationRecipient)rCache.CreateInstance();
					rec.SetupID = source.SetupID;
					rec.ContactType = r.ContactType;
					rec.ContactID = r.ContactID;
					rec.Active = r.Active;
					rec.Hidden = r.Hidden;
					rCache.Insert(rec);
				}
				catch (Exception ex)
				{
					PXTrace.WriteError(ex);
				}
			}
		}
	}

	#endregion

	#region CRNotificationSourceList

	public class CRNotificationSourceList<Source, SourceClass, NotificationType> : EPDependNoteList<NotificationSource, NotificationSource.refNoteID, Source>
		where Source : class, IBqlTable
		where SourceClass : class, IBqlField
		where NotificationType : class, IBqlOperand
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationSourceList(PXGraph graph)
			: base(graph)
		{
			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), typeof(NotificationSource),
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				typeof(NotificationSource).GetNestedType(typeof(NotificationSource.setupID_description).Name))),
				new PXSelectDelegate(NotificationSources));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationSource), ComposeWhere)));



			_ClassView = new PXView(graph, true,
									BqlCommand.CreateInstance(BqlCommand.Compose(
										typeof(Select2<,,>),
										typeof(NotificationSource),
										typeof(InnerJoin<NotificationSetup, On<NotificationSetup.setupID, Equal<NotificationSource.setupID>>>),
										typeof(Where<,,>), typeof(NotificationSetup.sourceCD), typeof(Equal<>), typeof(NotificationType),
																typeof(And<,>), typeof(NotificationSource.classID), typeof(Equal<>), typeof(Optional<>), typeof(SourceClass))));

			graph.RowPersisting.AddHandler<NotificationSource>(OnRowPersisting);
			graph.RowDeleting.AddHandler<NotificationSource>(OnRowDeleting);
			graph.RowUpdated.AddHandler<NotificationSource>(OnRowUpdated);
			graph.RowSelected.AddHandler<NotificationSource>(OnRowSelected);

		}
		protected virtual IEnumerable NotificationSources()
		{
			List<NotificationSource> result = new List<NotificationSource>();
			foreach (NotificationSource item in _SourceView.SelectMulti())
			{
				result.Add(item);
			}
			foreach (NotificationSource classItem in GetClassItems())
			{
				if (result.Find(i => i.SetupID == classItem.SetupID && i.NBranchID == classItem.NBranchID) == null)
					result.Add(classItem);
			}
			return result;
		}

        [Obsolete(PX.Objects.Common.InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected virtual void OnRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
        { }

		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			NotificationSource row = (NotificationSource)e.Row;
			foreach (NotificationSource classItem in GetClassItems())
				if (classItem.SetupID == row.SetupID && classItem.NBranchID == row.NBranchID && row.OverrideSource == false)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(typeof(NotificationSource).Name, null, Messages.DeleteClassNotification);
				}
			if (!e.Cancel)
				this.View.RequestRefresh();
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			NotificationSource row = (NotificationSource)e.Row;
			if (row.ClassID != null)
			{
				if (e.Operation == PXDBOperation.Delete)
					e.Cancel = true;

				if (e.Operation == PXDBOperation.Update)
				{
					sender.SetStatus(row, PXEntryStatus.Deleted);
					NotificationSource ins = (NotificationSource)sender.CreateInstance();
					ins.SetupID = row.SetupID;
					ins.NBranchID = row.NBranchID;
					ins = (NotificationSource)sender.Insert(ins);

					NotificationSource clone = PXCache<NotificationSource>.CreateCopy(row);
					clone.NBranchID = ins.NBranchID;
					clone.SourceID = ins.SourceID;
					clone.RefNoteID = ins.RefNoteID;
					clone.ClassID = null;
					clone = (NotificationSource)sender.Update(clone);
					if (clone != null)
					{
						sender.PersistInserted(clone);
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);
						PXCache source = sender.Graph.Caches[BqlCommand.GetItemType(SourceNoteID)];
						Guid? refNoteID = (Guid?)source.GetValue(source.Current, SourceNoteID.Name);
						if (refNoteID != null)
						{
							foreach (NotificationRecipient r in PXSelect<NotificationRecipient,
							Where<NotificationRecipient.sourceID, Equal<Required<NotificationRecipient.sourceID>>,
							  And<NotificationRecipient.refNoteID, Equal<Required<NotificationRecipient.refNoteID>>,
								And<NotificationRecipient.classID, IsNotNull>>>>
							.Select(sender.Graph, row.SourceID, refNoteID))
							{
								PXCache cache = sender.Graph.Caches[typeof(NotificationRecipient)];
								if (cache.GetStatus(r) == PXEntryStatus.Inserted)
								{
									NotificationRecipient u1 = (NotificationRecipient)cache.CreateCopy(r);
									u1.SourceID = clone.SourceID;
									cache.Update(u1);
									cache.PersistInserted(u1);
								}

								if (cache.GetStatus(r) == PXEntryStatus.Updated ||
									cache.GetStatus(r) == PXEntryStatus.Inserted) continue;
								NotificationRecipient u = (NotificationRecipient)cache.CreateCopy(r);
								u.SourceID = clone.SourceID;
								u.ClassID = null;
								cache.Update(u);
							}
						}
					}
					e.Cancel = true;
				}
			}
		}
		public virtual void OnRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;
			NotificationSource row = (NotificationSource)e.Row;
			bool classitem = GetClassItems().Any(cs => cs.SourceID == row.SourceID);
			PXUIFieldAttribute.SetEnabled<NotificationSource.overrideSource>(sender, row, !classitem);
			PXUIFieldAttribute.SetEnabled<NotificationSource.setupID>(sender, row, !classitem);
		}

		private IEnumerable<NotificationSource> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationSource classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationSource) ? (NotificationSource)((PXResult)rec)[0] :
					(rec is NotificationSource ? (NotificationSource)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
	}

	#endregion

	#region CRNotificationRecipientList

	public class CRNotificationRecipientList<Source, SourceClassID> : EPDependNoteList<NotificationRecipient, NotificationRecipient.refNoteID, Source>
		where Source : class, IBqlTable
		where SourceClassID : class, IBqlField
	{
		protected readonly PXView _SourceView;
		protected readonly PXView _ClassView;

		public CRNotificationRecipientList(PXGraph graph)
			: base(graph)
		{
			Type table = typeof(NotificationRecipient);
			Type where = BqlCommand.Compose(
				typeof(Where<,,>), typeof(NotificationRecipient.sourceID), typeof(Equal<Optional<NotificationSource.sourceID>>),
				typeof(And<,>), typeof(NotificationRecipient.refNoteID), typeof(Equal<>), typeof(Current<>), this.SourceNoteID);

			this.View = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(
				typeof(Select<,,>), table,
				typeof(Where<boolTrue, Equal<boolTrue>>),
				typeof(OrderBy<>),
				typeof(Asc<>),
				table.GetNestedType(typeof(NotificationRecipient.orderID).Name))),
				new PXSelectDelegate(NotificationRecipients));

			_SourceView = new PXView(graph, false,
				BqlCommand.CreateInstance(
				BqlCommand.Compose(typeof(Select<,>), typeof(NotificationRecipient), where)));



			_ClassView = new PXView(graph, true,
									BqlCommand.CreateInstance(BqlCommand.Compose(
										typeof(Select<,>), typeof(NotificationRecipient),
										typeof(Where<,,>), typeof(NotificationRecipient.classID), typeof(Equal<>), typeof(Current<>), typeof(SourceClassID),
										typeof(And<,,>), typeof(NotificationRecipient.setupID), typeof(Equal<Current<NotificationSource.setupID>>),
										typeof(And<NotificationRecipient.refNoteID, IsNull>)
																  )));

			graph.RowPersisting.AddHandler<NotificationRecipient>(OnRowPersisting);
			graph.RowSelected.AddHandler<NotificationRecipient>(OnRowSeleted);
			graph.RowDeleting.AddHandler<NotificationRecipient>(OnRowDeleting);
			graph.RowInserting.AddHandler<NotificationRecipient>(OnRowInserting);
			graph.RowUpdating.AddHandler<NotificationRecipient>(OnRowUpdating);
		}


		protected virtual IEnumerable NotificationRecipients()
		{
			var result = new List<NotificationRecipient>();
			foreach (NotificationRecipient item in _SourceView.SelectMulti())
			{
				item.OrderID = item.NotificationID;
				result.Add(item);
			}

			foreach (NotificationRecipient classItem in GetClassItems())
			{
				NotificationRecipient item = result.Find(i =>
					i.ContactType == classItem.ContactType &&
					i.ContactID == classItem.ContactID);
				if (item == null)
				{
					item = classItem;
					result.Add(item);
				}
				item.OrderID = int.MinValue + classItem.NotificationID;
			}
			return result;
		}

		protected override void Source_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			sender.Current = e.Row;
			internalDelete = true;
			try
			{
				foreach (NotificationRecipient item in _SourceView.SelectMulti())
				{
					this._SourceView.Cache.Delete(item);
				}
			}
			finally
			{
				internalDelete = false;
			}
		}
		private bool internalDelete;

		protected virtual void OnRowSeleted(PXCache sender, PXRowSelectedEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;
			if (row == null) return;
			bool updatableContractID =
				(row.ContactType == NotificationContactType.Contact ||
			   row.ContactType == NotificationContactType.Employee);
			bool updatableContactType = !GetClassItems().Any(classItem => row.ContactType == classItem.ContactType &&
																		  row.ContactID == classItem.ContactID);

			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactID).Name, updatableContactType && updatableContractID);
			PXUIFieldAttribute.SetEnabled(sender, row, typeof(NotificationRecipient.contactType).Name, updatableContactType);
		}
		protected virtual void OnRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			NotificationRecipient row = (NotificationRecipient)e.Row;

			if (e.Operation == PXDBOperation.Insert)
			{
				NotificationSource source =
						PXSelectReadonly<NotificationSource,
						Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>,
							And<NotificationSource.refNoteID, Equal<Required<NotificationSource.refNoteID>>>>>.
							Select(sender.Graph, row.SetupID, row.RefNoteID);
				if (source != null)
				{
					if (sender.Graph.Caches[typeof(NotificationSource)].GetStatus(source) == PXEntryStatus.Updated)
					{
						e.Cancel = true;
					}
				}
			}


			if (row.RefNoteID == null)
			{
				if (e.Operation == PXDBOperation.Update)
				{
					sender.Remove(row);
					NotificationRecipient ins = (NotificationRecipient)sender.Insert();
					NotificationRecipient clone = PXCache<NotificationRecipient>.CreateCopy(row);
					clone.NotificationID = ins.NotificationID;
					clone.RefNoteID = ins.RefNoteID;
					clone.ClassID = null;
					NotificationSource source =
						PXSelectReadonly<NotificationSource,
						Where<NotificationSource.setupID, Equal<Required<NotificationSource.setupID>>,
							And<NotificationSource.refNoteID, Equal<Required<NotificationSource.refNoteID>>>>>.
							Select(sender.Graph, row.SetupID, row.RefNoteID);
					if (source != null)
						clone.SourceID = source.SourceID;

					clone = (NotificationRecipient)sender.Update(clone);
					if (clone != null)
					{
						sender.PersistInserted(clone);
						sender.Normalize();
						sender.SetStatus(clone, PXEntryStatus.Notchanged);
					}
					e.Cancel = true;
				}
			}
			else
			{
				var sourceCache = sender.Graph.Caches<NotificationSource>();
				NotificationSource source = SelectFrom<NotificationSource>
								.Where<NotificationSource.sourceID.IsEqual<@P.AsInt>
									.And<NotificationSource.refNoteID.IsNull>>
								.View
								.SelectSingleBound(sender.Graph, null, row.SourceID);
				// changed base cases marked as deleted (but prevented from deletion ?\(�_o)/? )
				// and new related to current document are created
				// if mark as updated - it will have override
				if (sourceCache.GetStatus(source).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Held))
				{
					sourceCache.SetStatus(source, PXEntryStatus.Updated);
				}
			}
		}
		protected virtual void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			if (internalDelete) return;
			NotificationRecipient row = (NotificationRecipient)e.Row;
			foreach (NotificationRecipient classItem in GetClassItems())
				if (classItem.SetupID == row.SetupID &&
					  classItem.ContactType == row.ContactType &&
						classItem.ContactID == row.ContactID)
				{
					if (row.RefNoteID == null)
					{
						e.Cancel = true;
						throw new PXRowPersistingException(typeof(NotificationRecipient).Name, null, Messages.DeleteClassNotification);
					}
				}
			if (!e.Cancel)
				this.View.RequestRefresh();
		}

		protected virtual void OnRowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			if (e.Row != null)
			{
				e.Cancel = !sender.Graph.IsImport && !ValidateDuplicates(sender, (NotificationRecipient)e.Row, null);
				if (e.Cancel != true)
				{
					NotificationRecipient r = (NotificationRecipient)e.Row;
					NotificationSource source = (NotificationSource)sender.Graph.Caches[typeof(NotificationSource)].Current;
					r.ClassID = source != null ? source.ClassID : null;
				}
			}
		}

		protected virtual void OnRowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			if (e.Row != null && e.NewRow != null && CheckUpdated(sender, (NotificationRecipient)e.Row, (NotificationRecipient)e.NewRow))
				e.Cancel = !ValidateDuplicates(sender, (NotificationRecipient)e.NewRow, (NotificationRecipient)e.Row);
		}
		private IEnumerable<NotificationRecipient> GetClassItems()
		{
			foreach (object rec in _ClassView.SelectMulti())
			{
				NotificationRecipient classItem =
					(rec is PXResult && ((PXResult)rec)[0] is NotificationRecipient) ? (NotificationRecipient)((PXResult)rec)[0] :
					(rec is NotificationRecipient ? (NotificationRecipient)rec : null);
				if (classItem == null) continue;
				yield return classItem;
			}
		}
		private bool CheckUpdated(PXCache sender, NotificationRecipient row, NotificationRecipient newRow)
		{
			return row.ContactType != newRow.ContactType || row.ContactID != newRow.ContactID;
		}

		private bool ValidateDuplicates(PXCache sender, NotificationRecipient row, NotificationRecipient oldRow)
		{
			foreach (NotificationRecipient sibling in this.View.SelectMulti())
			{
				if (!CheckUpdated(sender, sibling, row) && row != sibling)
				{

					if (oldRow == null || row.ContactType != oldRow.ContactType)
						sender.RaiseExceptionHandling<NotificationRecipient.contactType>(
							row, row.ContactType,
							new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));

					if (oldRow == null || row.ContactID != oldRow.ContactID)
						sender.RaiseExceptionHandling<NotificationRecipient.contactID>(
							row, row.ContactType,
							new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));
					return false;
				}
			}
			sender.RaiseExceptionHandling<NotificationRecipient.contactType>(
							row, row.ContactType, null);
			sender.RaiseExceptionHandling<NotificationRecipient.contactID>(
							row, row.ContactID, null);
			return true;
		}

	}

	#endregion

	#region EPActivityPreview

	//TODO: move to corresponding control PX.Web.Controls.dll
	/*[Serializable]
	public class EPActivityPreview : EPGenericActivity
	{
		#region TaskID

		public new abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

		#endregion

		#region Subject

		public new abstract class subject : PX.Data.BQL.BqlString.Field<subject> { }

		[PXDBString(255, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "Summary", IsReadOnly = true)]
		public override string Subject
		{
			get
			{
				return base.Subject;
			}
			set
			{
				base.Subject = value;
			}
		}

		#endregion

		#region ColoredCategory

		public abstract class coloredCategory : PX.Data.BQL.BqlString.Field<coloredCategory> { }

		[PXString]
		[PXUIField(DisplayName = "Category")]
		public virtual String ColoredCategory { get; set; }

		#endregion

		#region DueDateDescription

		public abstract class dueDateDescription : PX.Data.BQL.BqlString.Field<dueDateDescription> { }

		private string _dueDateDescription;
		[PXString]
		[PXUIField(DisplayName = "Due Date")]
		public virtual String DueDateDescription
		{
			get
			{
				if (_dueDateDescription == null)
				{
					_dueDateDescription = string.Empty;
					if (StartDate != null) _dueDateDescription = "Start " + ((DateTime)StartDate).ToString("g");
					if (EndDate != null)
					{
						if (_dueDateDescription.Length > 0) _dueDateDescription += ", end ";
						else _dueDateDescription += "End ";
						_dueDateDescription += ((DateTime)EndDate).ToString("g");
					}
				}
				return _dueDateDescription;
			}
		}

		#endregion

		public new abstract class owner : PX.Data.BQL.BqlGuid.Field<owner> { }

		[PXDBGuid]
		[PXUIField(DisplayName = "Performed by")]
		public override Guid? Owner
		{
			get
			{
				return base.Owner;
			}
			set
			{
				base.Owner = value;
			}
		}
	}*/

	#endregion

	#region PXActionExt<TNode>
	[Obsolete("Will be removed in 7.0 version")]
	public class PXActionExt<TNode> : PXAction<TNode>
		where TNode : class, IBqlTable, new()
	{
		public delegate void AdapterPrepareHandler(PXAdapter adapter);
		public delegate void AdapterPrepareInsertHandler(PXAdapter adapter, object previousCurrent);

		private PXView _view;
		private AdapterPrepareHandler _selectPrepareHandler;
		private AdapterPrepareInsertHandler _insertPrepareHandler;

		protected PXActionExt(PXGraph graph)
			: base(graph)
		{
		}

		public PXActionExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}

		public PXActionExt(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		public virtual void SetPrepareHandlers(AdapterPrepareHandler selectPrepareHandler, AdapterPrepareInsertHandler insertPrepareHandler)
		{
			SetPrepareHandlers(selectPrepareHandler, insertPrepareHandler, null);
		}

		public virtual void SetPrepareHandlers(AdapterPrepareHandler selectPrepareHandler, AdapterPrepareInsertHandler insertPrepareHandler, PXView view)
		{
			if (view != null && !typeof(TNode).IsAssignableFrom(view.GetItemType()))
				throw new ArgumentException(string.Format("Item of view must inherit '{0}'", typeof(TNode).Name), "view");
			_view = view;
			_selectPrepareHandler = selectPrepareHandler;
			_insertPrepareHandler = insertPrepareHandler;
		}

		public override IEnumerable Press(PXAdapter adapter)
		{
			var newAdapter = new PXAdapter(_view ?? adapter.View);
			PXAdapter.Copy(adapter, newAdapter);
			if (_selectPrepareHandler != null) _selectPrepareHandler(adapter);
			var result = new List<object>(base.Press(newAdapter).Cast<object>());
			PXAdapter.Copy(newAdapter, adapter);
			return result;
		}

		protected virtual void InsertExt(PXAdapter adapter, object previousCurrent)
		{
			if (_insertPrepareHandler != null) _insertPrepareHandler(adapter, previousCurrent);
			Insert(adapter);
		}

		protected override void Insert(PXAdapter adapter)
		{
			var vals = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			if (adapter.Searches != null)
				for (int i = 0; i < adapter.Searches.Length && i < adapter.SortColumns.Length; i++)
					vals[adapter.SortColumns[i]] = adapter.Searches[i];
			foreach (string key in adapter.View.Cache.Keys)
				if (!vals.ContainsKey(key)) vals.Add(key, null);
			if (adapter.View.Cache.Insert(vals) == 1)
			{
				if (adapter.SortColumns == null)
					adapter.SortColumns = adapter.View.Cache.Keys.ToArray();
				else
				{
					var cols = new List<string>(adapter.SortColumns);
					foreach (string key in adapter.View.Cache.Keys)
						if (!CompareIgnoreCase.IsInList(cols, key))
							cols.Add(key);
					adapter.SortColumns = cols.ToArray();
				}
				adapter.Searches = new object[adapter.SortColumns.Length];
				for (int i = 0; i < adapter.Searches.Length; i++)
				{
					object val;
					if (vals.TryGetValue(adapter.SortColumns[i], out val))
						adapter.Searches[i] = val is PXFieldState ? ((PXFieldState)val).Value : val;
				}
				adapter.StartRow = 0;
			}
		}
	}

	#endregion

	#region PXFirstExt

	public class PXFirstExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXFirstExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}

		[PXUIField(DisplayName = ActionsMessages.First, MapEnableRights = PXCacheRights.Select)]
		[PXFirstButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.SelectTimeStamp();
			adapter.StartRow = 0;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXLastExt
	[Obsolete("Will be removed in 7.0 version")]
	public class PXLastExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXLastExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Last, MapEnableRights = PXCacheRights.Select)]
		[PXLastButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.SelectTimeStamp();
			adapter.StartRow = -adapter.MaximumRows;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXPreviousExt

	public class PXPreviousExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXPreviousExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select)]
		[PXPreviousButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			bool inserted = adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) == PXEntryStatus.Inserted;
			if (inserted) return MoveToLast(adapter);

			var previousCurrent = adapter.View.Cache.Current;
			adapter.StartRow -= adapter.MaximumRows;
			_Graph.SelectTimeStamp();
			List<object> ret = (List<object>)adapter.Get();
			object curr = adapter.View.Cache.Current;
			adapter.Currents = new object[] { curr };
			_Graph.Clear(PXClearOption.PreserveTimeStamp);
			if (ret.Count == 0)
			{
				if (adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
				{
					adapter.Currents = null;
					InsertExt(adapter, previousCurrent);
					ret = (List<object>)adapter.Get();
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = -adapter.MaximumRows;
					adapter.Searches = null;
					ret = (List<object>)adapter.Get();
					if (ret.Count == 0 && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
					{
						InsertExt(adapter, previousCurrent);
						ret = (List<object>)adapter.Get();
						adapter.View.Cache.IsDirty = false;
					}
				}
			}
			return ret;
		}

		private IEnumerable MoveToLast(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			adapter.StartRow = -adapter.MaximumRows;
			adapter.Searches = null;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1 && adapter.View.Cache.AllowInsert)
			{
				InsertExt(adapter, previousCurrent);
				foreach (object ret in adapter.Get())
				{
					yield return ret;
				}
				adapter.View.Cache.IsDirty = false;
			}
		}
	}

	#endregion

	#region PXNextExt
	[Obsolete("Will be removed in 7.0 version")]
	public class PXNextExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXNextExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select)]
		[PXNextButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			bool inserted = adapter.View.Cache.Current != null && adapter.View.Cache.GetStatus(adapter.View.Cache.Current) == PXEntryStatus.Inserted;
			var previousCurrent = adapter.View.Cache.Current;
			adapter.StartRow += adapter.MaximumRows;
			_Graph.SelectTimeStamp();
			List<object> ret = (List<object>)adapter.Get();
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			_Graph.Clear(PXClearOption.PreserveTimeStamp);
			if (ret.Count == 0)
			{
				if (!inserted && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
				{
					InsertExt(adapter, previousCurrent);
					ret = (List<object>)adapter.Get();
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = 0;
					adapter.Searches = null;
					ret = (List<object>)adapter.Get();
					if (ret.Count == 0 && adapter.View.Cache.AllowInsert && adapter.MaximumRows == 1)
					{
						InsertExt(adapter, previousCurrent);
						ret = (List<object>)adapter.Get();
						adapter.View.Cache.IsDirty = false;
					}
				}
			}
			return ret;
		}
	}

	#endregion

	#region PXInsertExt
	[Obsolete("Will be removed in 7.0 version")]
	public class PXInsertExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXInsertExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Insert, MapEnableRights = PXCacheRights.Insert, MapViewRights = PXCacheRights.Insert)]
		[PXInsertButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (!adapter.View.Cache.AllowInsert)
			{
				throw new PXException(ErrorMessages.CantInsertRecord);
			}
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			InsertExt(adapter, previousCurrent);
			var newSearches = new ArrayList();
			var newSortColumns = new List<string>();
			for (int i = 0; i < adapter.Searches.Length && i < adapter.SortColumns.Length; i++)
			{
				var sortColumn = adapter.SortColumns[i];
				if (adapter.View.Cache.Keys.Contains(sortColumn, StringComparer.OrdinalIgnoreCase))
				{
					newSearches.Add(adapter.Searches[i]);
					newSortColumns.Add(sortColumn);
				}
			}
			adapter.SortColumns = newSortColumns.ToArray();
			adapter.Searches = newSearches.ToArray();
			foreach (object ret in adapter.Get())
			{
				yield return ret;
			}
			adapter.View.Cache.IsDirty = false;
		}
	}

	#endregion

	#region PXDeleteExt
	[Obsolete("Will be removed in 7.0 version")]
	public class PXDeleteExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXDeleteExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		public override object GetState(object row)
		{
			object state = base.GetState(row);
			PXButtonState bs = state as PXButtonState;
			if (bs != null && !String.IsNullOrEmpty(bs.ConfirmationMessage))
			{
				if (typeof(TNode).IsDefined(typeof(PXCacheNameAttribute), true))
				{
					PXCacheNameAttribute attr = (PXCacheNameAttribute)(typeof(TNode).GetCustomAttributes(typeof(PXCacheNameAttribute), true)[0]);
					bs.ConfirmationMessage = String.Format(bs.ConfirmationMessage, attr.GetName());
				}
				else
				{
					bs.ConfirmationMessage = String.Format(bs.ConfirmationMessage, typeof(TNode).Name);
				}
			}
			return state;
		}
		[PXUIField(DisplayName = ActionsMessages.Delete, MapEnableRights = PXCacheRights.Delete, MapViewRights = PXCacheRights.Delete)]
		[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit)]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			if (!adapter.View.Cache.AllowDelete)
			{
				throw new PXException(ErrorMessages.CantDeleteRecord);
			}
			var previousCurrent = adapter.View.Cache.Current;
			int startRow = adapter.StartRow;
			foreach (object item in adapter.Get())
			{
				adapter.View.Cache.Delete(item);
			}
			try
			{
				_Graph.Actions.PressSave();
			}
			catch
			{
				_Graph.Clear();
				throw;
			}
			_Graph.SelectTimeStamp();
			adapter.StartRow = startRow;
			bool anyFound = false;
			foreach (object item in adapter.Get())
			{
				yield return item;
				anyFound = true;
			}
			if (!anyFound && adapter.MaximumRows == 1)
			{
				if (adapter.View.Cache.AllowInsert)
				{
					InsertExt(adapter, previousCurrent);
					foreach (object ret in adapter.Get())
					{
						yield return ret;
					}
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.StartRow = 0;
					adapter.Searches = null;
					foreach (object item in adapter.Get())
					{
						yield return item;
					}
				}
			}
		}
	}

	#endregion

	#region PXCancelExt
	[Obsolete("Will be removed in 7.0 version")]
	public class PXCancelExt<TNode> : PXActionExt<TNode>
		where TNode : class, IBqlTable, new()
	{
		public PXCancelExt(PXGraph graph, string name)
			: base(graph, name)
		{
		}
		[PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select)]
		[PXCancelButton]
		protected override IEnumerable Handler(PXAdapter adapter)
		{
			adapter.Currents = new object[] { adapter.View.Cache.Current };
			var previousCurrent = adapter.View.Cache.Current;
			_Graph.Clear();
			_Graph.SelectTimeStamp();
			bool anyFound = false;
			bool perfromSearch = true;
			if (adapter.MaximumRows == 1)
			{
				perfromSearch = adapter.View.Cache.Keys.Count == 0;
				if (adapter.Searches != null)
				{
					for (int i = 0; i < adapter.Searches.Length; i++)
					{
						if (adapter.Searches[i] != null)
						{
							perfromSearch = true;
							break;
						}
					}
				}
			}
			if (perfromSearch)
			{
				foreach (object item in adapter.Get())
				{
					yield return item;
					anyFound = true;
				}
			}
			if (!anyFound && adapter.MaximumRows == 1)
			{
				if (adapter.View.Cache.AllowInsert)
				{
					_Graph.Clear();
					_Graph.SelectTimeStamp();
					InsertExt(adapter, previousCurrent);
					foreach (object ret in adapter.Get())
					{
						yield return ret;
					}
					adapter.View.Cache.IsDirty = false;
				}
				else
				{
					adapter.Currents = null;
					adapter.StartRow = 0;
					adapter.Searches = null;
					foreach (object item in adapter.Get())
					{
						yield return item;
					}
				}
			}
		}
	}

	#endregion

	#region CRRelationsList<TNoteField>

	public class CRRelationsList<TNoteField> : PXSelect<CRRelation>
		where TNoteField : IBqlField
	{
		public CRRelationsList(PXGraph graph)
			: base(graph, GetHandler())
		{
			VerifyParameters();
			AttacheEventHandlers(graph);
		}

		private static void VerifyParameters()
		{
			if (BqlCommand.GetItemType(typeof(TNoteField)) == null)
				throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.IBqlFieldMustBeNested,
					typeof(TNoteField).GetLongName()), "TNoteField");
			if (!typeof(IBqlTable).IsAssignableFrom(BqlCommand.GetItemType(typeof(TNoteField))))
				throw new ArgumentException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.IBqlTableMustBeInherited,
					BqlCommand.GetItemType(typeof(TNoteField)).GetLongName()), "TNoteField");
		}

		private void AttacheEventHandlers(PXGraph graph)
		{
			PXDBDefaultAttribute.SetSourceType<CRRelation.refNoteID>(graph.Caches[typeof(CRRelation)], typeof(TNoteField));
			graph.FieldDefaulting.AddHandler(typeof(CRRelation), typeof(CRRelation.refNoteID).Name, CRRelation_RefNoteID_FieldDefaulting);
			graph.FieldUpdated.AddHandler(typeof(CRRelation), typeof(CRRelation.isPrimary).Name, CRRelation_IsPrimary_FieldUpdated);
			graph.FieldUpdated.AddHandler(typeof(CRRelation), typeof(CRRelation.targetType).Name, CRRelation_TargetType_FieldUpdated);
			graph.RowInserted.AddHandler(typeof(CRRelation), CRRelation_RowInserted);
			graph.RowUpdated.AddHandler(typeof(CRRelation), CRRelation_RowUpdated);
			graph.RowSelected.AddHandler(typeof(CRRelation), CRRelation_RowSelected);
			graph.RowPersisting.AddHandler(typeof(CRRelation), CRRelation_RowPersisting);

			graph.Initialized +=
				sender =>
				{
					sender.Views.Caches.Remove(typeof(TNoteField));
					sender.EnsureCachePersistence(typeof(CRRelation));
					AppendActions(graph);
				};
		}

		private void AppendActions(PXGraph graph)
		{
			var viewName = graph.ViewNames[View];
			var primaryDAC = BqlCommand.GetItemType(typeof(TNoteField));
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "_TargetDetails", null, TargetDetailsHandler);
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "_EntityDetails", null, EntityDetailsHandler);
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "_ContactDetails", null, ContactDetailsHandler);
		}

		private IEnumerable TargetDetailsHandler(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var cache = graph.Caches[typeof(CRRelation)];
			var row = (cache.Current as CRRelation);
			if (row != null)
			{
				var h = new EntityHelper(graph);
				h.NavigateToRow(row.TargetType, row.TargetNoteID, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		private IEnumerable EntityDetailsHandler(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var cache = graph.Caches[typeof(CRRelation)];
			var row = (cache.Current as CRRelation).
				With(_ => _.EntityID).
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(graph, _.Value));

			if (row != null)
			{
				if (row.Type == BAccountType.EmployeeType)
					PXRedirectHelper.TryRedirect(PXGraph.CreateInstance<EmployeeMaint>(), row, PXRedirectHelper.WindowMode.NewWindow);
				else
					PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		private IEnumerable ContactDetailsHandler(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var cache = graph.Caches[typeof(CRRelation)];
			var row = (cache.Current as CRRelation).
				With(_ => _.ContactID).
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(graph, _.Value));
			if (row != null)
				PXRedirectHelper.TryRedirect(graph, row, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		protected virtual void CRRelation_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			var refCache = sender.Graph.Caches[BqlCommand.GetItemType(typeof(TNoteField))];
			e.NewValue = refCache.GetValue(refCache.Current, typeof(TNoteField).Name);
		}

		protected virtual void CRRelation_IsPrimary_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var relation = (CRRelation)e.Row;
			if (relation.IsPrimary == true && !String.IsNullOrEmpty(relation.TargetType))
			{
				ClearOtherPrimarys(sender, relation);
			}
		}

		protected virtual void CRRelation_TargetType_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var relation = (CRRelation)e.Row;
			if (relation.IsPrimary == true && !String.IsNullOrEmpty(relation.TargetType))
			{
				ClearOtherPrimarys(sender, relation);
			}
		}

		protected void ClearOtherPrimarys(PXCache sender, CRRelation relation)
		{
			foreach (CRRelation rel in PXSelect<CRRelation,
				Where<CRRelation.role, Equal<Required<CRRelation.role>>,
					And<CRRelation.targetType, Equal<Required<CRRelation.targetType>>,
					And<CRRelation.isPrimary, Equal<True>, And<CRRelation.refNoteID, Equal<Required<CRRelation.refNoteID>>,
					And<CRRelation.relationID, NotEqual<Required<CRRelation.relationID>>>>>>>>
					.Select(_Graph, relation.Role, relation.TargetType, relation.RefNoteID, relation.RelationID))
			{
				rel.IsPrimary = false;
				sender.Update(rel);
			}
			this.View.RequestRefresh();
		}

		protected virtual void CRRelation_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = (CRRelation)e.Row;

			FillRow(sender.Graph, row);
		}

		protected virtual void CRRelation_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = (CRRelation)e.Row;
			var oldRow = (CRRelation)e.OldRow;

			if (!sender.ObjectsEqual<CRRelation.targetNoteID>(row, oldRow))
			{
			    Type cachetype = row.TargetType != null
			        ? GraphHelper.GetType(row.TargetType)
                    : null;

				if (cachetype != null && 
                    !typeof(BAccount).IsAssignableFrom(cachetype) &&
				    !typeof(Contact).IsAssignableFrom(cachetype))
					row.DocNoteID = row.TargetNoteID;
				else
					row.DocNoteID = null;
			}

			if (sender.ObjectsEqual<CRRelation.contactID>(row, oldRow) &&
				!sender.ObjectsEqual<CRRelation.entityID>(row, oldRow))
				row.ContactID = null;

			FillRow(sender.Graph, row);
		}

		protected virtual void CRRelation_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as CRRelation;
			if (row == null) return;

			FillRow(sender.Graph, row);

			var enableContactID = 
				row.EntityID.
				With(id => (BAccount)PXSelect<BAccount>.
					Search<BAccount.bAccountID>(sender.Graph, id.Value)).
				Return(acct => acct.Type != BAccountType.EmployeeType,
				true);
			PXUIFieldAttribute.SetEnabled(sender, row, typeof(CRRelation.contactID).Name, enableContactID);
		}

		protected virtual void CRRelation_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{

			CRRelation row = (CRRelation)e.Row;
			
			if (row.TargetType == typeof(Contact).FullName && row.ContactID == null)
				sender.RaiseExceptionHandling<CRRelation.contactID>(row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CRRelation.contactID).Name));
			else if ((row.TargetType == typeof(BAccount).FullName
					|| row.TargetType == typeof(Customer).FullName
					|| row.TargetType == typeof(Vendor).FullName)
				&& row.EntityID == null)
				sender.RaiseExceptionHandling<CRRelation.entityID>(row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CRRelation.entityID).Name));
			else if (row.TargetType != typeof(Contact).FullName
					&& row.TargetType != typeof(BAccount).FullName
					&& row.TargetType != typeof(Customer).FullName
					&& row.TargetType != typeof(Vendor).FullName
					&& row.TargetNoteID == null)
				sender.RaiseExceptionHandling<CRRelation.targetNoteID>(row, null,
					new PXSetPropertyException(ErrorMessages.FieldIsEmpty, typeof(CRRelation.targetNoteID).Name));
		}

		private static void FillRow(PXGraph graph, CRRelation row)
		{
			var search = row.EntityID.
					With(id => PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(graph, id.Value));
			string acctType = null;
			if (search == null || search.Count == 0)
			{
				row.Name = null;
				row.EntityCD = null;
			}
			else
			{
				var account = (BAccount)search[0][typeof(BAccount)];
				row.Name = account.AcctName;
				row.EntityCD = account.AcctCD;
				acctType = account.Type;
			}

			Contact contactSearch;
			PXResultset<EPEmployeeSimple> employeeSearch;
			if (acctType == BAccountType.EmployeeType &&
				(employeeSearch = row.EntityID.
					With(eId => PXSelectJoin<EPEmployeeSimple,
					LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployeeSimple.defContactID>>,
					LeftJoin<Users, On<Users.pKID, Equal<EPEmployeeSimple.userID>>>>,
					Where<EPEmployeeSimple.bAccountID, Equal<Required<EPEmployeeSimple.bAccountID>>>>.
					Select(graph, eId.Value))) != null)
			{
				row.ContactName = null;
				var contact = (Contact)employeeSearch[0][typeof(Contact)];
				row.Email = contact.EMail;
				var user = (Users)employeeSearch[0][typeof(Users)];
				if (string.IsNullOrEmpty(row.Name))
					row.Name = user.FullName;
				if (string.IsNullOrEmpty(row.Email))
					row.Email = user.Email;
			}
			else if ((contactSearch = row.ContactID.
						With(cId => (Contact)PXSelect<Contact>.
							Search<Contact.contactID>(graph, cId.Value))) != null)
			{
				row.ContactName = contactSearch.DisplayName;
				row.Email = contactSearch.EMail;
			}
			else
			{
				row.ContactName = null;
				row.Email = null;
			}
		}

		private static PXSelectDelegate GetHandler()
		{
			return () =>
			{
				var command = new Select2<CRRelation,
					LeftJoin<BAccount, On<BAccount.bAccountID, Equal<CRRelation.entityID>>,
					LeftJoin<Contact,
							  On<Contact.contactID, Equal<Switch<Case<Where<BAccount.type, Equal<BAccountType.employeeType>>, BAccount.defContactID>, CRRelation.contactID>>>,
					LeftJoin<Users, On<Users.pKID, Equal<Contact.userID>>>>>,
					Where<CRRelation.refNoteID, Equal<Current<TNoteField>>>>();
				var startRow = PXView.StartRow;
				int totalRows = 0;
				var list = new PXView(PXView.CurrentGraph, false, command).
					Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters,
						   ref startRow, PXView.MaximumRows, ref totalRows);
				PXView.StartRow = 0;
				foreach (PXResult<CRRelation, BAccount, Contact, Users> row in list)
				{
					var relation = (CRRelation)row[typeof(CRRelation)];
					var account = (BAccount)row[typeof(BAccount)];
					relation.Name = account.AcctName;
					relation.EntityCD = account.AcctCD;
					var contact = (Contact)row[typeof(Contact)];
					if (contact.ContactID == null && relation.ContactID != null &&
						account.Type != BAccountType.EmployeeType)
					{
						var directContact = (Contact)PXSelect<Contact>.
														 Search<Contact.contactID>(PXView.CurrentGraph, relation.ContactID);
						if (directContact != null) contact = directContact;
					}
					relation.Email = contact.EMail;
					var user = (Users)row[typeof(Users)];
					if (account.Type != BAccountType.EmployeeType)
						relation.ContactName = contact.DisplayName;
					else
					{
						if (string.IsNullOrEmpty(relation.Name))
							relation.Name = user.FullName;
						if (string.IsNullOrEmpty(relation.Email))
							relation.Email = user.Email;
					}
				}
				return list;
			};
		}

		public static string GetEmailsForCC(PXGraph graph, Guid? refNoteID)
		{
			string result = String.Empty;

			var command = new Select2<CRRelation,
							LeftJoin<BAccount, 
								On<BAccount.bAccountID, Equal<CRRelation.entityID>>,
							LeftJoin<Contact,
								On<Contact.contactID, Equal<Switch<Case<Where<BAccount.type, Equal<BAccountType.employeeType>>, BAccount.defContactID>, CRRelation.contactID>>>,
							LeftJoin<Users, 
								On<Users.pKID, Equal<Contact.userID>>>>>,
							Where<CRRelation.refNoteID, Equal<Required<CRRelation.refNoteID>>>>();

			var list = new PXView(graph, false, command).SelectMulti(refNoteID);
			foreach (PXResult<CRRelation, BAccount, Contact, Users> row in list)
			{
				var relation = row.GetItem<CRRelation>();
				if (relation.AddToCC == true)
				{
					var account = row.GetItem<BAccount>();
					var contact = row.GetItem<Contact>();
					var user = row.GetItem<Users>();

					var name = account.AcctName;
					if (contact.ContactID == null && relation.ContactID != null &&
						account.Type != BAccountType.EmployeeType)
					{
						var directContact = (Contact)PXSelect<Contact>.
							Search<Contact.contactID>(graph, relation.ContactID);
						if (directContact != null) contact = directContact;
					}
					var email = contact.EMail;
					if (account.Type != BAccountType.EmployeeType)
						name = contact.DisplayName;
					else
					{
						if (string.IsNullOrEmpty(name))
							name = user.FullName;
						if (string.IsNullOrEmpty(email))
							email = user.Email;
					}
					if (email != null && (email = email.Trim()) != string.Empty)
						result = PXDBEmailAttribute.AppendAddresses(result, PXDBEmailAttribute.FormatAddressesWithSingleDisplayName(email, name));
				}
			}

			return result;
		}

	}

	#endregion

	#region CRSubscriptionsSelect

	public sealed class CRSubscriptionsSelect
	{
		public static IEnumerable Select(PXGraph graph, int? mailListID)
		{
			var startRow = PXView.StartRow;
			int totalRows = 0;

			var list = Select(graph, mailListID, PXView.Searches, PXView.SortColumns, PXView.Descendings,
								   ref startRow, PXView.MaximumRows, ref totalRows);

			PXView.StartRow = 0;
			return list;
		}

		public static IEnumerable Select(PXGraph graph, int? mailListID, object[] searches, string[] sortColumns, bool[] descendings, ref int startRow, int maxRows, ref int totalRows)
		{
			CRMarketingList list;
			if (mailListID == null ||
				(list = (CRMarketingList)PXSelect<CRMarketingList>.Search<CRMarketingList.marketingListID>(graph, mailListID)) == null)
			{
				return new PXResultset<Contact, BAccount, BAccountParent, Address, State>();
			}

			MergeFilters(graph, mailListID);

			BqlCommand command = new Select2<Contact,
				LeftJoin<CRMarketingListMember,
					On<CRMarketingListMember.contactID, Equal<Contact.contactID>,
					And<CRMarketingListMember.marketingListID, Equal<Current<CRMarketingList.marketingListID>>>>,
				LeftJoin<BAccount,
					On<BAccount.bAccountID, Equal<Contact.bAccountID>>,
				LeftJoin<BAccountParent,
					On<BAccountParent.bAccountID, Equal<Contact.parentBAccountID>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>,
				LeftJoin<State,
					On<State.countryID, Equal<Address.countryID>,
						And<State.stateID, Equal<Address.state>>>,
                LeftJoin<GL.Branch,
                    On<GL.Branch.bAccountID, Equal<Contact.bAccountID>,
                    And<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>>>,
				LeftJoin<CRLead,
					On<CRLead.contactID.IsEqual<Contact.contactID>>>>>>>>>,
				Where<
					GL.Branch.branchID, IsNull>>();

            var view = new PXView(graph, true, command);

			var sorts = new List<string>()
			{
				nameof(CRMarketingListMember) + "__" + nameof(CRMarketingListMember.IsSubscribed),
				nameof(Contact.MemberName),
				nameof(Contact.ContactID)
			};

			var descs = new List<bool>()
			{
				false,
				false,
				false
			};

			var search = new List<object>()
			{
				null,
				null,
				null
			};
			search.AddRange(searches);

			return view.Select(null, null, search.ToArray(), sorts.ToArray(), descs.ToArray(), PXView.Filters, ref startRow, maxRows, ref totalRows);
		}

		public static void MergeFilters(PXGraph graph, int? mailListID)
		{
			var filters = PXView.Filters; //new PXView.PXFilterRowCollection(new PXFilterRow[]{});
			CRMarketingList list = PXSelect<CRMarketingList,
				Where<CRMarketingList.marketingListID, Equal<Required<CRMarketingList.marketingListID>>>>.
				Select(graph, mailListID);

			Guid? mailListNoteID = PXNoteAttribute.GetNoteID<CRMarketingList.noteID>(graph.Caches[typeof(CRMarketingList)], list);

			PXCache targetCache = graph.Caches[typeof(Contact)];
			var filterRows = new List<PXFilterRow>();
			foreach (CRFixedFilterRow filter in
				PXSelect<CRFixedFilterRow,
					Where<CRFixedFilterRow.refNoteID, Equal<Required<CRFixedFilterRow.refNoteID>>>>.
				Select(graph, mailListNoteID))
			{
                if (filter.IsUsed == true && !string.IsNullOrEmpty(filter.DataField))
				{
					var f = new PXFilterRow
					{
						OpenBrackets = filter.OpenBrackets ?? 0,
						DataField = filter.DataField,
						Condition = (PXCondition)(filter.Condition ?? 0),
						Value = targetCache.ValueFromString(filter.DataField, filter.ValueSt) ?? filter.ValueSt,
						Value2 = targetCache.ValueFromString(filter.DataField, filter.ValueSt2) ?? filter.ValueSt2,
						CloseBrackets = filter.CloseBrackets ?? 0,
						OrOperator = filter.Operator == 1
					};
					filterRows.Add(f);
				}
			}
			filters.Add(filterRows.ToArray());
		}
	}

	#endregion

	#region _100Percents

	public sealed class _100Percents : PX.Data.BQL.BqlDecimal.Constant<_100Percents>
	{
		public _100Percents() : base(100m) { }
	}

	#endregion

	#region CSAttributeSelector<TAnswer, TEntityId, TEntityType, TEntityClassId>
	[Obsolete("Will be removed in 7.0 version")]
	public class CSAttributeSelector<TAnswer, TEntityNoteId, TEntityClassId>
		: PXSelect<TAnswer>
		where TAnswer : class, IBqlTable, new()
	{
		public CSAttributeSelector(PXGraph graph)
			: base(graph)
		{
			Intialize();
		}

		public CSAttributeSelector(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
			Intialize();
		}

		private void Intialize()
		{
			var graph = View.Graph;
			var isReadonly = View.IsReadOnly;
			var command = CreateCommand();
			View = new PXView(graph, isReadonly, command);
		}

		private BqlCommand CreateCommand()
		{
			var cache = _Graph.Caches[typeof(TAnswer)];
			var attIdField = cache.GetBqlField(typeof(CSAnswers.attributeID).Name);

			var entityNoteIdField = cache.GetBqlField(typeof(CSAnswers.refNoteID).Name);
			var res = BqlCommand.CreateInstance(
				typeof(Select2<,,,>), typeof(TAnswer),
				typeof(InnerJoin<,,>), typeof(CSAttributeGroup),
					typeof(On<,>), typeof(CSAttributeGroup.attributeID), typeof(Equal<>), attIdField,
				typeof(LeftJoin<,>), typeof(CSAttribute),
					typeof(On<,>), typeof(CSAttribute.attributeID), typeof(Equal<>), attIdField,
				typeof(Where<,,>), entityNoteIdField, typeof(Equal<>), typeof(TEntityNoteId),
					typeof(And<,,>), typeof(CSAttributeGroup.entityClassID), typeof(Equal<>), typeof(TEntityClassId),
				typeof(OrderBy<Asc<CSAttributeGroup.sortOrder>>));
			res = CorrectCommand(res);
			return res;
		}

		protected virtual BqlCommand CorrectCommand(BqlCommand command)
		{
			return command;
		}
	}

	#endregion

	#region CSAttributeSelector
	[Obsolete("Will be removed in 7.0 version")]
	public class CSAttributeSelector<TAnswer, TEntityNoteId, TEntityClassId, TJoin, TWhere>
		: CSAttributeSelector<TAnswer, TEntityNoteId, TEntityClassId>
		where TAnswer : class, IBqlTable, new()
	{
		public CSAttributeSelector(PXGraph graph)
			: base(graph)
		{

		}

		public CSAttributeSelector(PXGraph graph, Delegate handler)
			: base(graph, handler)
		{
		}

		protected override BqlCommand CorrectCommand(BqlCommand command)
		{
			var res = base.CorrectCommand(command);
			res = BqlCommand.CreateInstance(BqlCommand.AppendJoin(res.GetType(), typeof(TJoin)));
			res = res.WhereAnd(typeof(TWhere));
			return res;
		}
	}
	#endregion

	#region CRDBTimeSpanAttribute
	[Obsolete("Will be removed in 7.0 version")]
	public sealed class CRDBTimeSpanAttribute : PXDBTimeSpanAttribute
	{
		private const string _FIELD_POSTFIX = "_byString";
		private const string _INPUTMASK = "##### hrs ## mins";
		private const int _SHARPS_COUNT = 7;
		private const string _DEFAULT_VALUE = "      0";

		private string _byStringFieldName;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_byStringFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_byStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byStringFieldName, _FieldName_byString_FieldSelecting);
		}

		private void _FieldName_byString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = _DEFAULT_VALUE;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = ((int)ts.TotalHours).ToString() + ((int)ts.Minutes).ToString("00");
				if (valStr.Length < _SHARPS_COUNT)
					valStr = new string(' ', _SHARPS_COUNT - valStr.Length) + valStr;
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byStringFieldName, false, null,
															_INPUTMASK, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}
	}

	#endregion

	#region CRTimeSpanAttribute
	[Obsolete("Will be removed in 7.0 version")]
	public sealed class CRTimeSpanAttribute : PXTimeSpanAttribute
	{
		private const string _FIELD_POSTFIX = "_byString";
		private const string _TIME_FIELD_POSTFIX = "_byTimeString";
		private const string _INPUTMASK = "##### hrs ## mins";
		private const int _SHARPS_COUNT = 7;
		private const string _DEFAULT_VALUE = "      0";

		private string _byStringFieldName;
		private string _byTimeStringFieldName;

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_byStringFieldName = _FieldName + _FIELD_POSTFIX;
			sender.Fields.Add(_byStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byStringFieldName, _FieldName_byString_FieldSelecting);

			_byTimeStringFieldName = _FieldName + _TIME_FIELD_POSTFIX;
			sender.Fields.Add(_byTimeStringFieldName);
			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _byTimeStringFieldName, _FieldName_byTimeString_FieldSelecting);
		}

		private void _FieldName_byString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = _DEFAULT_VALUE;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = ((int)ts.TotalHours).ToString() + ((int)ts.Minutes).ToString("00");
				if (valStr.Length < _SHARPS_COUNT)
					valStr = new string(' ', _SHARPS_COUNT - valStr.Length) + valStr;
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byStringFieldName, false, null,
															_INPUTMASK, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}

		private void _FieldName_byTimeString_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs args)
		{
			var mainState = sender.GetStateExt(args.Row, _FieldName) as PXFieldState;
			var displayName = mainState.With(_ => _.DisplayName);
			var visible = mainState.With(_ => _.Visible);
			var val = sender.GetValue(args.Row, _FieldOrdinal);
			var valStr = string.Empty;
			if (val != null)
			{
				var ts = TimeSpan.FromMinutes((int)val);
				valStr = string.Format("{0:00}:{1:00}", (int)ts.TotalHours, ts.Minutes);
			}

			var newState = PXStringState.CreateInstance(valStr, null, false, _byTimeStringFieldName, false, null,
															null, null, null, null, null);
			newState.Enabled = false;
			newState.Visible = visible;
			newState.DisplayName = displayName;

			args.ReturnState = newState;
		}
	}

	#endregion

	#region CRCaseActivityHelper<TableRefNoteID>

	public class CRCaseActivityHelper
	{
		#region Ctor

		public static CRCaseActivityHelper Attach(PXGraph graph)
		{
			var res = new CRCaseActivityHelper();

			graph.RowInserted.AddHandler<PMTimeActivity>(res.ActivityRowInserted);
			graph.RowSelected.AddHandler<PMTimeActivity>(res.ActivityRowSelected);
			graph.RowUpdated.AddHandler<PMTimeActivity>(res.ActivityRowUpdated);

			return res;
		}
		#endregion

		#region Event Handlers

		protected virtual void ActivityRowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			if (item == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			var oldItem = e.OldRow as PMTimeActivity;
			if (item == null || oldItem == null) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true) item.IsBillable = false;
		}

		protected virtual void ActivityRowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var item = e.Row as PMTimeActivity;
			if (item == null || !string.IsNullOrEmpty(item.TimeCardCD)) return;

			var graph = sender.Graph;
			var @case = GetCase(graph, item.RefNoteID);
			if (@case != null && @case.Released == true)
				PXUIFieldAttribute.SetEnabled<PMTimeActivity.isBillable>(sender, item, false);
		}

		#endregion

		#region Private Methods

		private CRCase GetCase(PXGraph graph, object refNoteID)
		{
			if (refNoteID == null) return null;
			return (CRCase)PXSelectJoin<CRCase,
				InnerJoin<CRActivityLink,
					On<CRActivityLink.refNoteID, Equal<CRCase.noteID>>>,
				Where<CRActivityLink.noteID, Equal<Required<PMTimeActivity.refNoteID>>>>.
				SelectWindowed(graph, 0, 1, refNoteID);
		}

		#endregion
	}

	#endregion

	#region CRAttribute

	public class CRAttribute
	{
		public class Attribute
		{
			public readonly string ID;
			public readonly string Description;
			public readonly int? ControlType;
			public readonly string EntryMask;
			public readonly string RegExp;
			public readonly string List;
			public readonly bool IsInternal;

			public Attribute(PXDataRecord record)
			{
				ID = record.GetString(0);
				Description = record.GetString(1);
				ControlType = record.GetInt32(2);
				EntryMask = record.GetString(3);
				RegExp = record.GetString(4);
				List = record.GetString(5);
				IsInternal = record.GetBoolean(6) == true;
				Values = new List<AttributeValue>();
			}

			protected Attribute(Attribute clone)
			{
				this.ID = clone.ID;
				this.Description = clone.Description;
				this.ControlType = clone.ControlType;
				this.EntryMask = clone.EntryMask;
				this.RegExp = clone.RegExp;
				this.List = clone.List;
				this.IsInternal = clone.IsInternal;
				this.Values = clone.Values;
			}

			public void AddValue(AttributeValue value)
			{
				Values.Add(value);
			}

			public readonly List<AttributeValue> Values;
		}

		public class AttributeValue
		{
			public readonly string ValueID;
			public readonly string Description;
			public readonly bool Disabled;

			public AttributeValue(PXDataRecord record)
			{
				ValueID = record.GetString(1);
				Description = record.GetString(2);
				Disabled = record.GetBoolean(3) == true;
			}
		}

		[DebuggerDisplay("ID={ID} Description={Description} Required={Required} IsActive={IsActive}")]
		public class AttributeExt : Attribute
		{
			public readonly string DefaultValue;
			public readonly bool Required;
			public readonly bool IsActive;
			public readonly string AttributeCategory;

			public AttributeExt(Attribute attr, string defaultValue, bool required, bool isActive)
				: this(attr, defaultValue, required, isActive, null)
			{
			}

			public AttributeExt(Attribute attr, string defaultValue, bool required, bool isActive, string attributeCategory)
				: base(attr)
			{
				DefaultValue = defaultValue;
				Required = required;
				IsActive = isActive;
				AttributeCategory = attributeCategory;
			}
		}

		public class AttributeList : DList<string, Attribute>
		{
			private readonly bool useDescriptionAsKey;
			public AttributeList(bool useDescriptionAsKey = false)
				: base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
			{
				this.useDescriptionAsKey = useDescriptionAsKey;
			}
			protected override string GetKeyForItem(Attribute item)
			{
				return useDescriptionAsKey ? item.Description : item.ID;
			}

			public override Attribute this[string key]
			{
				get
				{
					Attribute e;
					return TryGetValue(key, out e) ? e : null;

				}
			}
		}

		public class ClassAttributeList : DList<string, AttributeExt>
		{
			public ClassAttributeList()
				: base(StringComparer.InvariantCultureIgnoreCase, DefaultCapacity, DefaultDictionaryCreationThreshold, true)
			{

			}
			protected override string GetKeyForItem(AttributeExt item)
			{
				return item.ID;
			}
			public override AttributeExt this[string key]
			{
				get
				{
					AttributeExt e;
					return TryGetValue(key, out e) ? e : null;
				}
			}
		}

		private class Definition : IPrefetchable
		{
			public readonly AttributeList Attributes;
			public readonly AttributeList AttributesByDescr;
			public readonly Dictionary<string, AttributeList> EntityAttributes;
			public readonly Dictionary<string, Dictionary<string, ClassAttributeList>> ClassAttributes;

			public Definition()
			{
				Attributes = new AttributeList();
				AttributesByDescr = new AttributeList(true);
				ClassAttributes = new Dictionary<string, Dictionary<string, ClassAttributeList>>(StringComparer.InvariantCultureIgnoreCase);
				EntityAttributes = new Dictionary<string, AttributeList>(StringComparer.InvariantCultureIgnoreCase);
			}
			public void Prefetch()
			{
				using (new PXConnectionScope())
				{
					Attributes.Clear();
					AttributesByDescr.Clear();
					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttribute>(
						new PXDataField(typeof(CSAttribute.attributeID).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttribute).Name, typeof(CSAttribute.description).Name, false),
						new PXDataField(typeof(CSAttribute.controlType).Name),
						new PXDataField(typeof(CSAttribute.entryMask).Name),
						new PXDataField(typeof(CSAttribute.regExp).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttribute).Name, typeof(CSAttribute.list).Name, true),
						new PXDataField(typeof(CSAttribute.isInternal).Name)
						))
					{
						Attribute attr = new Attribute(record);
						Attributes.Add(attr);
						AttributesByDescr.Add(attr);
					}

					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeDetail>(
						new PXDataField(typeof(CSAttributeDetail.attributeID).Name),
						new PXDataField(typeof(CSAttributeDetail.valueID).Name),
						PXDBLocalizableStringAttribute.GetValueSelect(typeof(CSAttributeDetail).Name, typeof(CSAttributeDetail.description).Name, false),
						new PXDataField(typeof(CSAttributeDetail.disabled).Name),
						new PXDataFieldOrder(typeof(CSAttributeDetail.attributeID).Name),
						new PXDataFieldOrder(typeof(CSAttributeDetail.sortOrder).Name)
						))
					{
						string id = record.GetString(0);
						Attribute attr;
						if (Attributes.TryGetValue(id, out attr))
						{
							attr.AddValue(new AttributeValue(record));
						}
					}

					foreach (PXDataRecord record in PXDatabase.SelectMulti<CSAttributeGroup>(
					   new PXDataField(typeof(CSAttributeGroup.entityType).Name),
					   new PXDataField(typeof(CSAttributeGroup.entityClassID).Name),
					   new PXDataField(typeof(CSAttributeGroup.attributeID).Name),
					   new PXDataField(typeof(CSAttributeGroup.defaultValue).Name),
					   new PXDataField(typeof(CSAttributeGroup.required).Name),
					   new PXDataField(typeof(CSAttributeGroup.isActive).Name),
					   new PXDataField(typeof(CSAttributeGroup.attributeCategory).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.entityType).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.entityClassID).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.sortOrder).Name),
					   new PXDataFieldOrder(typeof(CSAttributeGroup.attributeID).Name)))
					{
						string type = record.GetString(0);
						string classID = record.GetString(1);
						string id = record.GetString(2);

						Dictionary<string, ClassAttributeList> dict;
						AttributeList list;

						if (!EntityAttributes.TryGetValue(type, out list))
							EntityAttributes[type] = list = new AttributeList();

						if (!ClassAttributes.TryGetValue(type, out dict))
							ClassAttributes[type] = dict = new Dictionary<string, ClassAttributeList>(StringComparer.InvariantCultureIgnoreCase);

						ClassAttributeList group;
						if (!dict.TryGetValue(classID, out group))
							dict[classID] = group = new ClassAttributeList();

						Attribute attr;
						if (Attributes.TryGetValue(id, out attr))
						{
							list.Add(attr);
							group.Add(new AttributeExt(attr, record.GetString(3), record.GetBoolean(4) ?? false, record.GetBoolean(5) ?? true, record.GetString(6)));
						}
					}
				}
			}
		}

		private static Definition Definitions
		{
			get
			{
				Definition defs = PX.Common.PXContext.GetSlot<Definition>();
				if (defs == null)
				{
					string currentLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
					if (!PXDBLocalizableStringAttribute.IsEnabled)
					{
						currentLanguage = "";
					}
					defs = PX.Common.PXContext.SetSlot<Definition>(PXDatabase.GetSlot<Definition>("CSAttributes" + currentLanguage, typeof(CSAttribute), typeof(CSAttributeDetail), typeof(CSAttributeGroup)));
				}
				return defs;
			}
		}

		public static AttributeList Attributes
		{
			get
			{
				return Definitions.Attributes;
			}
		}
		public static AttributeList AttributesByDescr
		{
			get
			{
				return Definitions.AttributesByDescr;
			}
		}

		public static AttributeList EntityAttributes(string type)
		{
			AttributeList list;
			return Definitions.EntityAttributes.TryGetValue(type, out list) ? list : new AttributeList();
		}

		private static ClassAttributeList EntityAttributes(string type, string classID)
		{
			Dictionary<string, ClassAttributeList> typeList;
			ClassAttributeList list;
			if (type != null && classID != null &&
				Definitions.ClassAttributes.TryGetValue(type, out typeList) &&
				typeList.TryGetValue(classID, out list))
				return list;

			return new ClassAttributeList();
		}

		public static ClassAttributeList EntityAttributes(Type entityType, string classID)
		{
			return EntityAttributes(entityType.FullName, classID);
		}
	}

	#endregion

	#region CSAttributeGroupList

	public class CSAttributeGroupList<TClass, TEntity> : PXSelectBase<CSAttributeGroup>
		where TClass : class
	{
		private readonly string _classIdFieldName;
	    private readonly Type _class;

		public CSAttributeGroupList(PXGraph graph)
		{
			_Graph = graph;

			var command = new Select3<CSAttributeGroup,
				InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
				OrderBy<Asc<CSAttributeGroup.entityClassID,
					Asc<CSAttributeGroup.entityType, Asc<CSAttributeGroup.sortOrder>>>>>();

			View = new PXView(graph, false, command, new PXSelectDelegate(SelectDelegate));

		    if (typeof(IBqlTable).IsAssignableFrom(typeof(TClass)))
		    {
		        _class = typeof(TClass);
                _classIdFieldName = _Graph.Caches[_class].BqlKeys.Single().Name;
            }
            else  if (typeof(IBqlField).IsAssignableFrom(typeof(TClass)))
            {
                _classIdFieldName = typeof(TClass).Name;
                _class = typeof(TClass).DeclaringType;                
            }
            else             
			{
                throw new PXArgumentException(typeof(TClass).Name);
            }

            _Graph.FieldDefaulting.AddHandler<CSAttributeGroup.entityType>((sender, e) =>
			{
				if (e.Row == null)
					return;
				e.NewValue = typeof(TEntity).FullName;
			});
			_Graph.FieldDefaulting.AddHandler<CSAttributeGroup.entityClassID>((sender, e) =>
			{
				if (e.Row == null)
					return;
				var entityClassCache = _Graph.Caches[_class];
				e.NewValue = entityClassCache.GetValue(entityClassCache.Current, _classIdFieldName)?.ToString();
			});
		    _Graph.RowDeleted.AddHandler(_class, (sender, e) =>
		    {
		        foreach (PXResult<CSAttributeGroup> rec in SelectDelegate())
		            this.Cache.Delete((CSAttributeGroup)rec);
		    });


            _Graph.FieldSelecting.AddHandler<CSAttributeGroup.defaultValue>(CSAttributeGroup_DefaultValue_FieldSelecting);			
			_Graph.RowDeleting.AddHandler<CSAttributeGroup>(OnRowDeleting);


			if (!graph.Views.Caches.Contains(typeof(CSAnswers)))
				graph.Views.Caches.Add(typeof(CSAnswers));
		}

		private void OnRowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			var attributeGroup = (CSAttributeGroup)e.Row;
			if (attributeGroup == null 
                || sender.GetStatus(e.Row) == PXEntryStatus.Inserted                                 
			    || sender.GetStatus(e.Row) == PXEntryStatus.InsertedDeleted
                || (_Graph.Caches[_class].Current != null && _Graph.Caches[_class].GetStatus(_Graph.Caches[_class].Current) == PXEntryStatus.Deleted))
                return;

			if (attributeGroup.IsActive == true)
				throw new PXSetPropertyException(Messages.AttributeCannotDeleteActive);
			
			if (!_Graph.IsContractBasedAPI && Ask("Warning", Messages.AttributeDeleteWarning, MessageButtons.OKCancel) != WebDialogResult.OK)
			{
				e.Cancel = true;
				return;
			}
			DeleteAttributesForGroup(_Graph, attributeGroup);
		}

		public static void DeleteAttributesForGroup(PXGraph graph,CSAttributeGroup attributeGroup)
		{
            Type entityType = PXBuildManager.GetType(attributeGroup.EntityType, false);         
			if (entityType == null)
				throw new ArgumentNullException(nameof(entityType), $"Could not locate entity type {attributeGroup.EntityType}");
			var noteIdField = EntityHelper.GetNoteType(entityType);
			if (noteIdField == null)
				throw new ArgumentNullException(nameof(noteIdField), $"Could not locate NoteId field for {attributeGroup.EntityType}");
			var classIdField =
				graph.Caches[entityType].GetAttributes(null).OfType<CRAttributesFieldAttribute>().FirstOrDefault()?.ClassIdField;
			if (classIdField == null)
				throw new ArgumentNullException(nameof(classIdField), $"Could not locate ClassId field for {attributeGroup.EntityType}");

			var queryParameterValues = new List<object>
			{
				attributeGroup.EntityClassID,
				attributeGroup.EntityClassID,
				attributeGroup.AttributeID,
				attributeGroup.EntityType,				
			};
			if (classIdField != null)
			{
                queryParameterValues[0] = graph.Caches[entityType].ValueFromString(classIdField.Name, attributeGroup.EntityClassID);
            }

			var classIdIsDbField =
				graph.Caches[entityType].GetAttributes(classIdField.Name)
					.Any(x =>
					{
						var type = x.GetType();
						return type.IsSubclassOf(typeof(PXDBFieldAttribute)) ||
							type == typeof(PXDBCalcedAttribute) ||
							type.IsSubclassOf(typeof(PXDBCalcedAttribute));
					});

			//ClassId can be constant
			if (!classIdIsDbField)			
				classIdField = typeof(CSAttributeGroup.entityClassID);									

			var graph2 = new PXGraph();

            Type requiredClassIdField = BqlCommand.Compose(typeof(Equal<>), typeof(Required<>), classIdField);

			var view = new PXView(graph2, false, BqlCommand.CreateInstance(
				BqlCommand.Compose(
					typeof(Select2<,,>), typeof(CSAttributeGroup),
					typeof(InnerJoin<,,>), typeof(CSAnswers), typeof(On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>),
					typeof(InnerJoin<,>), entityType, typeof(On<,,>), noteIdField, typeof(Equal<CSAnswers.refNoteID>),typeof(And<,>), 
						classIdField, requiredClassIdField,
					typeof(Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
							And<CSAttributeGroup.attributeID, Equal<Required<CSAttributeGroup.attributeID>>,
							And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>)
					)));

			var answers = view.SelectMultiBound(null, queryParameterValues.ToArray());

			if (answers.Count == 0)
				return;

			foreach (var resultSet in answers)
			{
				var answer = PXResult.Unwrap<CSAnswers>(resultSet);
				graph.Caches<CSAnswers>().Delete(answer);
			}
		}
		
		private void CSAttributeGroup_DefaultValue_FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			CSAttributeGroup row = e.Row as CSAttributeGroup;

			if (row == null)
				return;

			const string answerValueField = "DefaultValue";
			const int answerValueLength = 60;

			CSAttribute question = new PXSelect<CSAttribute>(_Graph).Search<CSAttribute.attributeID>(row.AttributeID);

			PXResultset<CSAttributeDetail> options = PXSelect<CSAttributeDetail,
				Where<CSAttributeDetail.attributeID, Equal<Required<CSAttributeGroup.attributeID>>>,
				OrderBy<Asc<CSAttributeDetail.sortOrder>>>.Select(_Graph, row.AttributeID);

			int required = row.Required.GetValueOrDefault() ? 1 : -1;

			//if (options.Count > 0)
			if (options.Count > 0 &&
				(question == null || question.ControlType == CSAttribute.Combo ||
				 question.ControlType == CSAttribute.MultiSelectCombo))
			{
				//ComboBox:

				List<string> allowedValues = new List<string>();
				List<string> allowedLabels = new List<string>();

				foreach (CSAttributeDetail option in options)
				{
					allowedValues.Add(option.ValueID);
					allowedLabels.Add(option.Description);
				}

				string mask = question != null ? question.EntryMask : null;

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
					true, answerValueField, false, required, mask, allowedValues.ToArray(), allowedLabels.ToArray(),
					true, null);

				if (question.ControlType == CSAttribute.MultiSelectCombo)
				{
					((PXStringState)e.ReturnState).MultiSelect = true;
				}
			}
			else if (question != null)
			{
				if (question.ControlType.GetValueOrDefault() == CSAttribute.CheckBox)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
						null, null, false, answerValueField, null, null, null, PXErrorLevel.Undefined, true, true,
						null, PXUIVisibility.Visible, null, null, null);
				}
				else if (question.ControlType.GetValueOrDefault() == CSAttribute.Datetime)
				{
					e.ReturnState = PXDateState.CreateInstance(e.ReturnState, answerValueField, false, required,
						question.EntryMask, question.EntryMask, null, null);
				}
				else
				{
					//TextBox:
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, answerValueLength, null,
						answerValueField, false, required, question.EntryMask, null, null, true, null);
				}
			}
		}				

		protected virtual IEnumerable SelectDelegate()
		{
			var entityClassCache = _Graph.Caches[_class];
			var row = entityClassCache.Current;

			if (row == null)
				yield break;

			var classIdValue = entityClassCache.GetValue(row, _classIdFieldName);

			if (classIdValue == null)
				yield break;

			var resultSet =
				new PXSelectJoin
					<CSAttributeGroup,
						InnerJoin<CSAttribute, On<CSAttributeGroup.attributeID, Equal<CSAttribute.attributeID>>>,
						Where<CSAttributeGroup.entityClassID, Equal<Required<CSAttributeGroup.entityClassID>>,
							 And<CSAttributeGroup.entityType, Equal<Required<CSAttributeGroup.entityType>>>>>(_Graph)
					.Select(classIdValue.ToString(), typeof(TEntity).FullName);

			foreach (var record in resultSet)
			{
				var attributeGroup = PXResult.Unwrap<CSAttributeGroup>(record);

				if (attributeGroup != null)
					yield return record;
			}
		}
	}

	#endregion

	#region CRAttributeList

	public class CRAttributeList<TEntity> : PXSelectBase<CSAnswers>
	{
		#region TypeNameConst
		public class TypeNameConst : PX.Data.BQL.BqlString.Constant<TypeNameConst>
		{
			public TypeNameConst() : base(typeof(TEntity).FullName) { }
		}
		#endregion

		private readonly EntityHelper _helper;

		public CRAttributeList(PXGraph graph)
		{
			_Graph = graph;
			_helper = new EntityHelper(graph);

			View = graph.IsExport
				? GetExportOptimizedView()
				: new PXView(graph, false,
				new Select3<CSAnswers, OrderBy<Asc<CSAnswers.order>>>(),
				new PXSelectDelegate(SelectDelegate));

			PXDependToCacheAttribute.AddDependencies(View, new[] { typeof(TEntity) });

			_Graph.EnsureCachePersistence(typeof(CSAnswers));
			PXDBAttributeAttribute.Activate(_Graph.Caches[typeof(TEntity)]);
			_Graph.FieldUpdating.AddHandler<CSAnswers.value>(FieldUpdatingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.value>(FieldSelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.isRequired>(IsRequiredSelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.attributeCategory>(AttributeCategorySelectingHandler);
			_Graph.FieldSelecting.AddHandler<CSAnswers.attributeID>(AttrFieldSelectingHandler);
			_Graph.RowPersisting.AddHandler<CSAnswers>(RowPersistingHandler);
			_Graph.RowPersisting.AddHandler<TEntity>(ReferenceRowPersistingHandler);
			_Graph.RowUpdating.AddHandler<TEntity>(ReferenceRowUpdatingHandler);
			_Graph.RowDeleted.AddHandler<TEntity>(ReferenceRowDeletedHandler);
			_Graph.RowInserted.AddHandler<TEntity>(RowInsertedHandler);
		}

		private PXView GetExportOptimizedView()
		{
			var instance = _Graph.Caches[typeof(TEntity)].CreateInstance();
			var classIdField = GetClassIdField(instance);
			var noteIdField = typeof(TEntity).GetNestedType(nameof(CSAttribute.noteID));

			var command = BqlTemplate.OfCommand<
					Select2<CSAnswers,
						InnerJoin<CSAttribute,
							On<CSAnswers.attributeID, Equal<CSAttribute.attributeID>>,
						InnerJoin<CSAttributeGroup,
							On<CSAnswers.attributeID, Equal<CSAttributeGroup.attributeID>>>>,
						Where<CSAttributeGroup.isActive, Equal<True>,
							And<CSAttributeGroup.entityType, Equal<TypeNameConst>,
							And<CSAttributeGroup.entityClassID, Equal<Current<BqlPlaceholder.A>>,
							And<CSAnswers.refNoteID, Equal<Current<BqlPlaceholder.B>>>>>>>>
					.Replace<BqlPlaceholder.A>(classIdField)
					.Replace<BqlPlaceholder.B>(noteIdField)
					.ToCommand();

			return new PXView(_Graph, true, command);
		}

        protected virtual IEnumerable SelectDelegate()
		{
			var currentObject = _Graph.Caches[typeof(TEntity)].Current;
			return SelectInternal(currentObject);
		}

		protected Guid? GetNoteId(object row)
		{
			return _helper.GetEntityNoteID(row);
		}

        protected Type GetClassIdField(object row)
		{
			if (row == null)
				return null;


			var fieldAttribute =
				_Graph.Caches[row.GetType()].GetAttributes(row, null)
					.OfType<CRAttributesFieldAttribute>()
					.FirstOrDefault();

			if (fieldAttribute == null)
				return null;

			return fieldAttribute.ClassIdField;
		}

        protected Type GetEntityTypeFromAttribute(object row)
		{
			var classIdField = GetClassIdField(row);
			if (classIdField == null)
				return null;

			return classIdField.DeclaringType;
		}

        protected string GetClassId(object row)
		{
			var classIdField = GetClassIdField(row);
			if (classIdField == null)
				return null;

			var entityCache = _Graph.Caches[row.GetType()];

			var classIdValue = entityCache.GetValue(row, classIdField.Name);

			return classIdValue?.ToString();
		}

		protected IEnumerable<CSAnswers> SelectInternal(object row)
		{
			if (row == null)
				yield break;

			var noteId = GetNoteId(row);

			if (!noteId.HasValue)
				yield break;

			var answerCache = _Graph.Caches[typeof(CSAnswers)];
			var entityCache = _Graph.Caches[row.GetType()];

			List<CSAnswers> answerList;

			var status = entityCache.GetStatus(row);

			if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
			{
				answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
			}
			else
			{
				answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
					.Select(_Graph, noteId).FirstTableItems.ToList();
			}

			var classId = GetClassId(row);

			CRAttribute.ClassAttributeList classAttributeList = new CRAttribute.ClassAttributeList();

			if (classId != null)
			{
				classAttributeList = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), classId);
			}
			//when coming from Import scenarios there might be attributes which don't belong to entity's current attribute class or the entity might not have any attribute class at all
			if (_Graph.IsImport && PXView.SortColumns.Any() && PXView.Searches.Any())
			{
				var columnIndex = Array.FindIndex(PXView.SortColumns,
					x => x.Equals(typeof(CSAnswers.attributeID).Name, StringComparison.OrdinalIgnoreCase));

				if (columnIndex >= 0 && columnIndex < PXView.Searches.Length)
				{
					var searchValue = PXView.Searches[columnIndex];

					if (searchValue != null)
					{
						//searchValue can be either AttributeId or Description
						var attributeDefinition = CRAttribute.Attributes[searchValue.ToString()] ??
											 CRAttribute.AttributesByDescr[searchValue.ToString()];

						if (attributeDefinition == null)
						{
							throw new PXSetPropertyException(Messages.AttributeNotValid);
						}
						//avoid duplicates
						else if (classAttributeList[attributeDefinition.ToString()] == null)
						{
							classAttributeList.Add(new CRAttribute.AttributeExt(attributeDefinition, null, false, true));
						}
					}
				}
			}

			if (answerList.Count == 0 && classAttributeList.Count == 0)
				yield break;

			//attribute identifiers that are contained in CSAnswers cache/table but not in class attribute list
			List<string> attributeIdListAnswers =
				answerList.Select(x => x.AttributeID)
					.Except(classAttributeList.Select(x => x.ID))
					.Distinct()
					.ToList();

			//attribute identifiers that are contained in class attribute list but not in CSAnswers cache/table
			List<string> attributeIdListClass =
				classAttributeList.Select(x => x.ID)
					.Except(answerList.Select(x => x.AttributeID))
					.ToList();

			//attribute identifiers which belong to both lists
			List<string> attributeIdListIntersection =
				classAttributeList.Select(x => x.ID)
					.Intersect(answerList.Select(x => x.AttributeID))
					.Distinct()
					.ToList();


			var cacheIsDirty = answerCache.IsDirty;

			List<CSAnswers> output = new List<CSAnswers>();

			//attributes contained only in CSAnswers cache/table should be added "as is"
			output.AddRange(answerList.Where(x => attributeIdListAnswers.Contains(x.AttributeID)));

			//attributes contained only in class attribute list should be created and initialized with default value
			foreach (var attributeId in attributeIdListClass)
			{
				var classAttributeDefinition = classAttributeList[attributeId];

				if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
					continue;

				if (!classAttributeDefinition.IsActive)
					continue;

				CSAnswers answer = (CSAnswers)answerCache.CreateInstance();
				answer.AttributeID = classAttributeDefinition.ID;
				answer.RefNoteID = noteId;
				answer.Value = GetDefaultAnswerValue(classAttributeDefinition);
				if (classAttributeDefinition.ControlType == CSAttribute.CheckBox)
				{
					bool value;
					if (bool.TryParse(answer.Value, out value))
						answer.Value = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
					else if (answer.Value == null)
						answer.Value = 0.ToString();
				}

				answer.IsRequired = classAttributeDefinition.Required;
                
			    Dictionary<string, object> keys = new Dictionary<string, object>();
			    foreach (string key in answerCache.Keys.ToArray())
			    {
			        keys[key] = answerCache.GetValue(answer, key);
			    }

				answerCache.Locate(keys);

			        answer = (CSAnswers)(answerCache.Locate(answer) ?? answerCache.Insert(answer));
				output.Add(answer);
			}

			//attributes belonging to both lists should be selected from CSAnswers cache/table with and additional IsRequired check against class definition
			foreach (CSAnswers answer in answerList.Where(x => attributeIdListIntersection.Contains(x.AttributeID)).ToList())
			{
				var classAttributeDefinition = classAttributeList[answer.AttributeID];

				if (PXSiteMap.IsPortal && classAttributeDefinition.IsInternal)
					continue;

				if (!classAttributeDefinition.IsActive)
					continue;

				if (answer.Value == null && classAttributeDefinition.ControlType == CSAttribute.CheckBox)
					answer.Value = bool.FalseString;

				if (answer.IsRequired == null || classAttributeDefinition.Required != answer.IsRequired)
				{
					answer.IsRequired = classAttributeDefinition.Required;

					var fieldState = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
					var fieldValue = fieldState != null && ((bool?)fieldState.Value).GetValueOrDefault();

					answer.IsRequired = classAttributeDefinition.Required || fieldValue;
				}



				output.Add(answer);
			}

			answerCache.IsDirty = cacheIsDirty;

			output =
				output.OrderBy(
					x =>
						classAttributeList.Contains(x.AttributeID)
							? classAttributeList.IndexOf(x.AttributeID)
							: (x.Order ?? 0))
					.ThenBy(x => x.AttributeID)
					.ToList();

			short attributeOrder = 0;

			foreach (CSAnswers answer in output)
			{
				answer.Order = attributeOrder++;
				yield return answer;
			}
		}

		protected virtual void FieldUpdatingHandler(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			var row = e.Row as CSAnswers;

            if (row == null || row.AttributeID == null)
				return;

			var attr = CRAttribute.Attributes[row.AttributeID];
			if (attr == null)
				return;

            if (e.NewValue is DateTime v&&attr.ControlType==CSAttribute.Datetime)
            {
                e.NewValue = v.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                return;
            }

            if( !(e.NewValue is string))
                return;
            
			var newValue = (string)e.NewValue;
			switch (attr.ControlType)
			{
				case CSAttribute.CheckBox:
					bool value;
					if (bool.TryParse(newValue, out value))
					{
						e.NewValue = Convert.ToInt32(value).ToString(CultureInfo.InvariantCulture);
					}
					break;
				case CSAttribute.Datetime:
					DateTime dt;
					if (sender.Graph.IsMobile)
					{
						newValue = newValue.Replace("Z", "");
						if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
						{
							e.NewValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
						}
					}
					else
					{
						if (DateTime.TryParse(newValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
						{
							e.NewValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
						}
					}
					break;
			}
		}

		protected virtual void FieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			if (row == null) return;

			var question = CRAttribute.Attributes[row.AttributeID];

			var options = question != null ? question.Values : null;

			var required = row.IsRequired == true ? 1 : -1;

			if (options != null && options.Count > 0)
			{
				//ComboBox:
				var allowedValues = new List<string>();
				var allowedLabels = new List<string>();

				foreach (var option in options)
				{
					if (option.Disabled && row.Value != option.ValueID) continue;

					allowedValues.Add(option.ValueID);
					allowedLabels.Add(option.Description);
				}

				e.ReturnState = PXStringState.CreateInstance(e.ReturnState, CSAttributeDetail.ParameterIdLength,
					true, typeof(CSAnswers.value).Name, false, required, question.EntryMask, allowedValues.ToArray(),
					allowedLabels.ToArray(), true, null);
				if (question.ControlType == CSAttribute.MultiSelectCombo)
				{
					((PXStringState)e.ReturnState).MultiSelect = true;
				}
			}
			else if (question != null)
			{
				if (question.ControlType == CSAttribute.CheckBox)
				{
					e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(bool), false, false, required,
						null, null, false, typeof(CSAnswers.value).Name, null, null, null, PXErrorLevel.Undefined, true,
						true, null,
						PXUIVisibility.Visible, null, null, null);
					if (e.ReturnValue is string)
					{
						int value;
						if (int.TryParse((string)e.ReturnValue, NumberStyles.Integer, CultureInfo.InvariantCulture,
							out value))
						{
							e.ReturnValue = Convert.ToBoolean(value);
						}
					}
				}
				else if (question.ControlType == CSAttribute.Datetime)
				{
					e.ReturnState = PXDateState.CreateInstance(e.ReturnState, typeof(CSAnswers.value).Name, false,
						required, question.EntryMask, question.EntryMask,
						null, null);
				}
				else
				{
					//TextBox:					
					var vstate = sender.GetStateExt<CSAnswers.value>(null) as PXStringState;
					e.ReturnState = PXStringState.CreateInstance(e.ReturnState, vstate.With(_ => _.Length), null,
						typeof(CSAnswers.value).Name,
						false, required, question.EntryMask, null, null, true, null);
				}
			}
			if (e.ReturnState is PXFieldState)
			{
					var state = (PXFieldState)e.ReturnState;
				var errorState = sender.GetAttributes(row, typeof(CSAnswers.value).Name)
					.OfType<IPXInterfaceField>()
					.FirstOrDefault();
				if (errorState != null && errorState.ErrorLevel != PXErrorLevel.Undefined && !string.IsNullOrEmpty(errorState.ErrorText))
				{
					state.Error = errorState.ErrorText;
					state.ErrorLevel = errorState.ErrorLevel;
				}

				string category = (string)(sender.GetValueExt<CSAnswers.attributeCategory>(row) as PXFieldState)?.Value;
				state.Enabled = (category != CSAttributeGroup.attributeCategory.Variant);
			}
		}

		protected virtual void AttrFieldSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			PXUIFieldAttribute.SetEnabled<CSAnswers.attributeID>(sender, e.Row, false);
		}

		protected virtual void IsRequiredSelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			var current = sender.Graph.Caches[typeof(TEntity)].Current;

			if (row == null || current == null)
				return;
			var currentNoteId = GetNoteId(current);

			if (e.ReturnValue != null || row.RefNoteID != currentNoteId)
				return;

			//when importing data - make all attributes nonrequired (otherwise import might fail)
			if (sender.Graph.IsImport)
			{
				e.ReturnValue = false;
				return;
			}

			var currentClassId = GetClassId(current);

			var attribute = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(current), currentClassId)[row.AttributeID];

			if (attribute == null)
			{
				e.ReturnValue = false;
			}
			else
			{
				if (PXSiteMap.IsPortal && attribute.IsInternal)
					e.ReturnValue = false;
				else
					e.ReturnValue = attribute.Required;
			}
		}

		protected virtual void AttributeCategorySelectingHandler(PXCache sender, PXFieldSelectingEventArgs e)
		{
			var row = e.Row as CSAnswers;
			var current = sender.Graph.Caches[typeof(TEntity)].Current;

			if (row == null || current == null)
				return;
			var currentNoteId = GetNoteId(current);

			if (e.ReturnValue != null || row.RefNoteID != currentNoteId)
				return;

			var currentClassId = GetClassId(current);

			var attribute = CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(current), currentClassId)[row.AttributeID];

			e.ReturnValue = attribute?.AttributeCategory ?? CSAttributeGroup.attributeCategory.Attribute;
		}

		protected virtual void RowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation != PXDBOperation.Insert && e.Operation != PXDBOperation.Update) return;

			var row = e.Row as CSAnswers;
			if (row == null) return;

			if (!row.RefNoteID.HasValue)
			{
				e.Cancel = true;
				RowPersistDeleted(sender, row);
			}
			else if (string.IsNullOrEmpty(row.Value))
			{
				var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty,
					sender.GetStateExt<CSAnswers.value>(null).With(_ => _ as PXFieldState).With(_ => _.DisplayName));
				if (row.IsRequired == true &&
					sender.RaiseExceptionHandling<CSAnswers.value>(e.Row, row.Value,
						new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name)))
				{
					throw new PXRowPersistingException(typeof(CSAnswers.value).Name, row.Value, mayNotBeEmpty,
						typeof(CSAnswers.value).Name);
				}
				e.Cancel = true;
				if (sender.GetStatus(row) != PXEntryStatus.Inserted)
					RowPersistDeleted(sender, row);
			}
		}

		protected virtual void RowPersistDeleted(PXCache cache, object row)
		{
			try
			{
				cache.PersistDeleted(row);
				cache.SetStatus(row, PXEntryStatus.InsertedDeleted);
			}
			catch (PXLockViolationException)
			{
			}
			cache.ResetPersisted(row);
		}
		protected virtual void ReferenceRowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			object row = e.Row;
			if (row == null) return;

			var noteId = GetNoteId(row);

			if (!noteId.HasValue) return;

			var answerCache = _Graph.Caches[typeof(CSAnswers)];
			var entityCache = _Graph.Caches[row.GetType()];

			List<CSAnswers> answerList;

			var status = entityCache.GetStatus(row);

			if (status == PXEntryStatus.Inserted || status == PXEntryStatus.InsertedDeleted)
			{
				answerList = answerCache.Inserted.Cast<CSAnswers>().Where(x => x.RefNoteID == noteId).ToList();
			}
			else
			{
				answerList = PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
					.Select(_Graph, noteId).FirstTableItems.ToList();
			}
			foreach (var answer in answerList)
				this.Cache.Delete(answer);
		}

		protected virtual void ReferenceRowPersistingHandler(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row;

			if (row == null) return;

			var answersCache = _Graph.Caches[typeof(CSAnswers)];

			var emptyRequired = new List<string>();
			foreach (CSAnswers answer in answersCache.Cached)
			{
				if (answer.IsRequired == null)
				{
					var state = View.Cache.GetValueExt<CSAnswers.isRequired>(answer) as PXFieldState;
					if (state != null)
						answer.IsRequired = state.Value as bool?;
				}

				if (e.Operation == PXDBOperation.Delete)
				{
					answersCache.Delete(answer);
				}
				else if (string.IsNullOrEmpty(answer.Value) && answer.IsRequired == true && !_Graph.UnattendedMode)
				{
					var displayName = "";

					var attributeDefinition = CRAttribute.Attributes[answer.AttributeID];
					if (attributeDefinition != null)
						displayName = attributeDefinition.Description;

					emptyRequired.Add(displayName);
					var mayNotBeEmpty = PXMessages.LocalizeFormatNoPrefix(ErrorMessages.FieldIsEmpty, displayName);
					answersCache.RaiseExceptionHandling<CSAnswers.value>(answer, answer.Value,
						new PXSetPropertyException(mayNotBeEmpty, PXErrorLevel.RowError, typeof(CSAnswers.value).Name));
					PXUIFieldAttribute.SetError<CSAnswers.value>(answersCache, answer, mayNotBeEmpty);
				}
			}
			if (emptyRequired.Count > 0)
				throw new PXException(Messages.RequiredAttributesAreEmpty,
					string.Join(", ", emptyRequired.Select(s => string.Format("'{0}'", s))));
		}

		protected virtual void ReferenceRowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.Row;
			var newRow = e.NewRow;

			if (row == null || newRow == null)
				return;

			var rowNoteId = GetNoteId(row);

			var rowClassId = GetClassId(row);
			var newRowClassId = GetClassId(newRow);

			if (string.Equals(rowClassId, newRowClassId, StringComparison.InvariantCultureIgnoreCase))
			{
				// SelectInternal fills Answers caches...
				// but this line is called more than one time from cb api
				// so need to execute it only ones
				if (_Graph.IsContractBasedAPI
					&& !_Graph.Caches[typeof(CSAnswers)].Inserted.Any_())
					SelectInternal(newRow).ToList();

				return;
			}
			else if (_Graph.IsContractBasedAPI)
			{
				// workaround to clear cache if class wasn't default
				// (reverts SelectInternal above in previous call, because first call happens with default class)
				// otherwise it will not fill required attributes
				// AC-130095
				_Graph.Caches[typeof(CSAnswers)].Clear();
			}

			var newAttrList = new HashSet<string>();

			if (newRowClassId != null)
			{
				foreach (var attr in CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(newRow), newRowClassId))
				{
					newAttrList.Add(attr.ID);
				}
			}
			var relatedEntityTypes =
				sender.GetAttributesOfType<CRAttributesFieldAttribute>(newRow, null).FirstOrDefault()?.RelatedEntityTypes;

			PXGraph entityGraph = new PXGraph();
			var entityHelper = new EntityHelper(entityGraph);

			if (relatedEntityTypes != null)
				foreach (var classField in relatedEntityTypes)
				{
					object entity = entityHelper.GetEntityRow(classField.DeclaringType, rowNoteId);
					if (entity == null) continue;
					string entityClass = (string)entityGraph.Caches[classField.DeclaringType].GetValue(entity, classField.Name);
					if (entityClass == null) continue;
					CRAttribute.EntityAttributes(classField.DeclaringType, entityClass)
						.Where(x => !newAttrList.Contains(x.ID)).Select(x => x.ID)
						.ForEach(x => newAttrList.Add(x));
				}

			foreach (CSAnswers answersRow in
				PXSelect<CSAnswers,
					Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>
					.SelectMultiBound(sender.Graph, null, rowNoteId))
			{
				var copy = PXCache<CSAnswers>.CreateCopy(answersRow);
				View.Cache.Delete(answersRow);
				if (newAttrList.Contains(copy.AttributeID))
				{
					string rowDefaultValue =
						CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(row), rowClassId)
							.Where(x => x.ID == copy.AttributeID && x.IsActive == true)
							.FirstOrDefault()
							?.DefaultValue;
					if (string.Equals(rowDefaultValue, copy.Value, StringComparison.InvariantCultureIgnoreCase))
					{
						string newDefaultValue =
						CRAttribute.EntityAttributes(GetEntityTypeFromAttribute(newRow), newRowClassId)
							.Where(x => x.ID == copy.AttributeID && x.IsActive == true)
							.FirstOrDefault()
							?.DefaultValue;
						copy.Value = newDefaultValue;
					}
					View.Cache.Insert(copy);
				}
			}

			if (newRowClassId != null)
				SelectInternal(newRow).ToList();

			sender.IsDirty = true;
		}

		protected virtual void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (sender != null && sender.Graph != null && !sender.Graph.IsImport)
				SelectInternal(e.Row).ToList();
		}

		protected virtual void CopyAttributes(object destination, object source, bool copyall)
		{
			if (destination == null || source == null) return;

			var sourceAttributes = SelectInternal(source).RowCast<CSAnswers>().ToList();
			var targetAttributes = SelectInternal(destination).RowCast<CSAnswers>().ToList();

			var answerCache = _Graph.Caches<CSAnswers>();


			foreach (var targetAttribute in targetAttributes)
			{
				var sourceAttr = sourceAttributes.SingleOrDefault(x => x.AttributeID == targetAttribute.AttributeID);

				if (sourceAttr == null || string.IsNullOrEmpty(sourceAttr.Value) ||
					sourceAttr.Value == targetAttribute.Value)
					continue;

				if (string.IsNullOrEmpty(targetAttribute.Value) || copyall)
				{
					var answer = PXCache<CSAnswers>.CreateCopy(targetAttribute);
					answer.Value = sourceAttr.Value;
					answerCache.Update(answer);
				}
			}
		}

		public void CopyAllAttributes(object row, object src)
		{
			CopyAttributes(row, src, true);
		}

		public void CopyAttributes(object row, object src)
		{
			CopyAttributes(row, src, false);
		}

		protected virtual string GetDefaultAnswerValue(CRAttribute.AttributeExt attr)
		{
			return attr.DefaultValue;
		}
	}

	#endregion

	#region CRAttributeSourceList

	public class CRAttributeSourceList<TReference, TSourceField> : CRAttributeList<TReference>
		where TReference : class, IBqlTable, new()
		where TSourceField : IBqlField
	{
		public CRAttributeSourceList(PXGraph graph)
			: base(graph)
		{
			_Graph.FieldUpdated.AddHandler<TSourceField>(ReferenceSourceFieldUpdated);
		}


		private object _AttributeSource;

		protected object AttributeSource
		{
			get
			{
				var cache = _Graph.Caches<TReference>();

				var noteFieldName = EntityHelper.GetNoteField(typeof(TReference));

				if (_AttributeSource == null ||
					GetNoteId(_AttributeSource) != (Guid?)cache.GetValue(cache.Current, noteFieldName))
				{
					_AttributeSource = PXSelectorAttribute.Select<TSourceField>(cache, cache.Current);
				}
				return _AttributeSource;
			}
		}

		protected override string GetDefaultAnswerValue(CRAttribute.AttributeExt attr)
		{
			if (AttributeSource == null)
				return base.GetDefaultAnswerValue(attr);

			var sourceNoteId = GetNoteId(AttributeSource);

			var answers =
				PXSelect<CSAnswers, Where<CSAnswers.refNoteID, Equal<Required<CSAnswers.refNoteID>>>>.Select(_Graph, sourceNoteId);

			foreach (CSAnswers answer in answers)
			{
				if (answer.AttributeID == attr.ID && !string.IsNullOrEmpty(answer.Value))
					return answer.Value;
			}

			return base.GetDefaultAnswerValue(attr);
		}

		protected void ReferenceSourceFieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CopyAttributes(e.Row, AttributeSource);
		}
	}
	#endregion

	#region AddressSelectBase

	public abstract class AddressSelectBase : PXSelectBase<Address>, ICacheType<Address>
	{
		private const string OBSOLETE_USE_VIEW = " Use " + nameof(GetView) + " instead."
			+ " Construct valid View or add new SelectDelegate there.";

		internal const string _BUTTON_ACTION = "ValidateAddress"; //TODO: need concat with graph selector name;
		private const string _VIEWONMAP_ACTION = "ViewOnMap";

		protected Type _itemType;

		protected int _addressIdFieldOrdinal;
		protected int _asMainFieldOrdinal;
		private int _accountIdFieldOrdinal;

		private PXAction _action;
		private PXAction _mapAction;

		protected AddressSelectBase(PXGraph graph)
		{
			Initialize(graph);
			View = GetView(graph);
			AttacheHandlers(graph);
			AppendButton(graph);
		}

		public bool DoNotCorrectUI { get; set; }

		private void AppendButton(PXGraph graph)
		{
			_action = PXNamedAction.AddAction(graph, _itemType, _BUTTON_ACTION, CS.Messages.ValidateAddress, CS.Messages.ValidateAddress, ValidateAddress);
			_mapAction = PXNamedAction.AddAction(graph, _itemType, _VIEWONMAP_ACTION, Messages.ViewOnMap, ViewOnMap);
		}

		private void Initialize(PXGraph graph)
		{
			_Graph = graph;
			_Graph.EnsureCachePersistence(typeof(Address));
			_Graph.Initialized += sender => sender.Views.Caches.Remove(IncorrectPersistableDAC);

			var addressIdDAC = GetDAC(AddressIdField);
			var asMainDAC = GetDAC(AsMainField);
			var accounDAC = GetDAC(AccountIdField);
			if (addressIdDAC != asMainDAC || asMainDAC != accounDAC)
				throw new Exception(string.Format("Fields '{0}', '{1}' and '{2}' are defined in different DACs",
					addressIdDAC.Name, asMainDAC.Name, accounDAC));
			_itemType = addressIdDAC;

			var cache = _Graph.Caches[_itemType];
			_addressIdFieldOrdinal = cache.GetFieldOrdinal(AddressIdField.Name);
			_asMainFieldOrdinal = cache.GetFieldOrdinal(AsMainField.Name);
			_accountIdFieldOrdinal = cache.GetFieldOrdinal(AccountIdField.Name);
		}

		protected abstract Type AccountIdField { get; }

		protected abstract Type AsMainField { get; }

		protected abstract Type AddressIdField { get; }

		protected abstract Type IncorrectPersistableDAC { get; }

		protected virtual PXView GetView(PXGraph graph)
		{
			return new PXView(graph, false, new Select<Address>(), new PXSelectDelegate(SelectDelegate));
		}

		private static Type GetDAC(Type type)
		{
			var res = type.DeclaringType;
			if (res == null)
				throw new Exception(string.Format("DAC for field '{0}' can not be found", type.Name));
			return res;
		}

		private void AttacheHandlers(PXGraph graph)
		{
			graph.RowInserted.AddHandler(_itemType, RowInsertedHandler);
			graph.RowUpdating.AddHandler(_itemType, RowUpdatingHandler);
			graph.RowUpdated.AddHandler(_itemType, RowUpdatedHandler);
			graph.RowSelected.AddHandler(_itemType, RowSelectedHandler);
			graph.RowDeleted.AddHandler(_itemType, RowDeletedHandler);
		}

		private void RowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal);
			var isMainAddress = sender.GetValue(e.Row, AsMainField.Name) as bool?;
			if (isMainAddress == true) return;

			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			if (currentAddress != null)
			{
				var addressCache = sender.Graph.Caches[typeof(Address)];
				addressCache.Delete(currentAddress);
			}
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var addressCache = sender.Graph.Caches[typeof(Address)];
			var asMain = false;
			var isValidated = false;

			var accountId = sender.GetValue(e.Row, _accountIdFieldOrdinal);
			var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var containsAccount = accountAddressId != null;

			var currentAddressId = sender.GetValue(e.Row, _addressIdFieldOrdinal) ?? accountAddressId;
			var currentAddress = currentAddressId.
				With(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				SelectSingleBound(sender.Graph, null, _));
			if (currentAddress != null)
			{
				isValidated = currentAddress.IsValidated == true;

				if (currentAddressId.Equals(accountAddressId))
					asMain = true;
			}
			else
			{
				PXEntryStatus status = sender.GetStatus(e.Row);
				if (status != PXEntryStatus.Inserted && status != PXEntryStatus.Deleted && status != PXEntryStatus.InsertedDeleted)
				{
					sender.SetValue(e.Row, _addressIdFieldOrdinal, null);
					RowInsertedHandler(sender, new PXRowInsertedEventArgs(e.Row, true));
					sender.SetStatus(e.Row, PXEntryStatus.Updated);
				}
			}

			sender.SetValue(e.Row, _asMainFieldOrdinal, asMain);
			if (!DoNotCorrectUI)
			{
				PXUIFieldAttribute.SetEnabled(addressCache, currentAddress, !asMain);
				PXUIFieldAttribute.SetEnabled(sender, e.Row, AsMainField.Name, containsAccount);
			}
			_action.SetEnabled(!isValidated);
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2 + OBSOLETE_USE_VIEW)]
		protected virtual IEnumerable SelectDelegate()
		{
			var primaryCache = _Graph.Caches[_itemType];
			var primaryRecord = GetPrimaryRow();
			var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);

			var result = (Address)PXSelect<Address,
				Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(_Graph, currentAddressId);
			yield return result;
		}

		protected Type ItemType => _itemType;

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2 + OBSOLETE_USE_VIEW)]
		protected virtual object GetPrimaryRow() => throw new NotImplementedException();

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row;
			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));
			var currentAddressId = sender.GetValue(row, _addressIdFieldOrdinal);

			if (accountAddress == null)
			{
				asMain = false;
				sender.SetValue(row, _asMainFieldOrdinal, false);
			}

			var addressCache = sender.Graph.Caches[typeof(Address)];
			if (accountAddress != null && true.Equals(asMain))
			{
				if (currentAddressId != null && !object.Equals(currentAddressId, accountAddressId))
				{
					var currentAddress = (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(sender.Graph, currentAddressId);
					var oldDirty = addressCache.IsDirty;
					addressCache.Delete(currentAddress);
					addressCache.IsDirty = oldDirty;
				}
				sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
			}
			else
			{
				if (currentAddressId == null || object.Equals(currentAddressId, accountAddressId))
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					if (accountAddress != null)
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					addr.AddressID = null;
					addr.BAccountID = (int?)accountId;
					addr = (Address)addressCache.Insert(addr);

					sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
					addressCache.IsDirty = oldDirty;
				}
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.NewRow;
			var oldRow = e.Row;

			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var oldAsMain = sender.GetValue(oldRow, _asMainFieldOrdinal);

			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountAddressId = account.With(_ => _.DefAddressID);
			var accountAddress = accountAddressId.
				With<int?, Address>(_ => (Address)PXSelect<Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>.
				Select(sender.Graph, _));

			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);
			if (!object.Equals(accountId, oldAccountId))
			{
				var oldAddressId = sender.GetValue(row, _addressIdFieldOrdinal);
				var oldAccount = oldAccountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var oldAccountAddressId = oldAccount.With(_ => _.DefAddressID);
				var oldAccountAddress = oldAccountAddressId.
					With<int?, Address>(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
					Select(sender.Graph, _));
				oldAsMain = oldAccountAddress != null && object.Equals(oldAddressId, oldAccountAddressId);
				if (true.Equals(oldAsMain))
				{
					if (accountAddressId == null)
					{
						var oldDirty = _Graph.Caches[typeof(Address)].IsDirty;

						Address addr = (Address)_Graph.Caches[typeof(Address)].CreateCopy(oldAccountAddress);

						addr.AddressID = null;
						addr.BAccountID = (int?)accountId;
						addr = (Address)_Graph.Caches[typeof(Address)].Insert(addr);

						sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						_Graph.Caches[typeof(Address)].IsDirty = oldDirty;
						addressId = addr.AddressID;
					}
					else
					{
						asMain = true;
						addressId = accountAddressId;
						sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
					}
				}
			}

			if (true.Equals(asMain))
			{

				if (accountAddress == null)
				{
					asMain = false;
					sender.SetValue(row, _asMainFieldOrdinal, false);
				}
			}

			if (!object.Equals(asMain, oldAsMain))
			{
				if (true.Equals(asMain))
				{
					var oldAddressId = sender.GetValue(row, _addressIdFieldOrdinal);
					var oldAddress = ((int?)oldAddressId).
						With<int?, Address>(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(sender.Graph, _));
					if(sender.Graph.Caches<Address>().GetStatus(oldAddress) == PXEntryStatus.Inserted)
						sender.Graph.Caches<Address>().Delete(oldAddress);

					sender.SetValue(row, _addressIdFieldOrdinal, accountAddressId);
				}
				else
				{
					if (object.Equals(accountAddressId, addressId))
						sender.SetValue(row, _addressIdFieldOrdinal, null);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row;
			var oldRow = e.OldRow;

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);

			var addressId = sender.GetValue(row, _addressIdFieldOrdinal);
			var oldAddressId = sender.GetValue(oldRow, _addressIdFieldOrdinal);

			var addressCache = _Graph.Caches[typeof(Address)];
			if (!object.Equals(addressId, oldAddressId))
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountAddressId = account.With(_ => _.DefAddressID);
				var accountWithDefAddress = oldAddressId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.defAddressID, Equal<Required<BAccount.defAddressID>>>>.
					Select(_Graph, _));
				if (accountWithDefAddress == null)
				{
					var oldAddress = oldAddressId.
						With(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (oldAddress != null)
					{
						var oldIsDirty = addressCache.IsDirty;
						addressCache.Delete(oldAddress);
						addressCache.IsDirty = oldIsDirty;
					}
				}

				if (addressId == null)
				{
					var oldDirty = addressCache.IsDirty;
					Address addr;
					var accountAddress = accountAddressId.
						With<int?, Address>(_ => (Address)PXSelect<Address,
							Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));
					if (accountAddress != null && object.Equals(accountAddressId, oldAddressId))
					{
						addr = (Address)addressCache.CreateCopy(accountAddress);
					}
					else
					{
						addr = (Address)addressCache.CreateInstance();
					}
					if (addr != null)
					{
						addr.AddressID = null;
						addr.BAccountID = (int?)accountId;
						addr = (Address)addressCache.Insert(addr);

						sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						addressCache.IsDirty = oldDirty;
						addressId = addr.AddressID;
					}
				}
			}
			else if (addressId == null)
			{
				var oldDirty = addressCache.IsDirty;
				var addr = (Address)addressCache.CreateInstance();
				addr.AddressID = null;
				addr.BAccountID = (int?)accountId;
				addr = (Address)addressCache.Insert(addr);

				sender.SetValue(row, _addressIdFieldOrdinal, addr.AddressID);
				sender.SetValue(row, _asMainFieldOrdinal, false);
				addressCache.IsDirty = oldDirty;
			}
			else if (!object.Equals(accountId, oldAccountId))
			{
				bool oldIsDirty = addressCache.IsDirty;

				Address address = addressId.
					With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(_Graph, _));

				address.BAccountID = (int?)accountId;
				addressCache.Update(address);

				addressCache.IsDirty = oldIsDirty;
			}
		}

		[PXUIField(DisplayName = CS.Messages.ValidateAddress, FieldClass = CS.Messages.ValidateAddress)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process)]
		protected virtual IEnumerable ValidateAddress(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var addressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var address = addressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (address != null && address.IsValidated != true)
					PXAddressValidator.Validate<Address>(graph, address, true);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton()]
		protected virtual IEnumerable ViewOnMap(PXAdapter adapter)
		{
			var graph = adapter.View.Graph;
			var primaryCache = graph.Caches[_itemType];
			var primaryRecord = primaryCache.Current;
			if (primaryRecord != null)
			{
				var currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
				var currentAddress = currentAddressId.With(_ => (Address)PXSelect<Address,
						Where<Address.addressID, Equal<Required<Address.addressID>>>>.
						Select(graph, _));
				if (currentAddress != null)
					BAccountUtility.ViewOnMap(currentAddress);
			}
			return adapter.Get();
		}
	}

	#endregion

	#region BusinessAccountLocationAddressSelect

	public sealed class BusinessAccountLocationAddressSelect : AddressSelect<Location.defAddressID, Location.isAddressSameAsMain, Location.bAccountID>
	{
		private readonly PXView _locationView;
		private readonly PXView _locationAddressView;

		public BusinessAccountLocationAddressSelect(PXGraph graph) : base(graph)
		{
			_locationView = new PXView(_Graph, false,
				new Select<
						Location,
					Where<
							Location.bAccountID, Equal<Current<BAccount.bAccountID>>,
						And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>());
			_locationAddressView = new PXView(_Graph, false,
				new Select<
						Address,
					Where<Address.addressID, Equal<Required<Address.addressID>>>>());
		}

		protected override PXView GetView(PXGraph graph)
		{
			return new PXView(graph, false,
				new Select2<
						Address,
					InnerJoin<Location,
						On<Location.defAddressID, Equal<Address.addressID>>,
					InnerJoin<BAccount,
						On<BAccount.bAccountID, Equal<Location.bAccountID>>>>,
					Where<
							Location.bAccountID, Equal<Current<BAccount.bAccountID>>,
						And<Location.locationID, Equal<Current<BAccount.defLocationID>>>>>(),
				new PXSelectDelegate(SelectDelegate));
		}

		[Api.Export.PXOptimizationBehavior(IgnoreBqlDelegate = true)]
		private new IEnumerable SelectDelegate()
		{
			var location = (Location)_locationView.SelectSingle();
			if(location == null)
			{
				yield break;
		}
			// need copy, because if set address disabled - main address may be disabled too (if they are the same)
			var address = PXCache<Address>.CreateCopy((Address)_locationAddressView.SelectSingle(location.DefAddressID));
			PXUIFieldAttribute.SetEnabled(_Graph.Caches<Address>(), address, location.IsAddressSameAsMain != true);

			yield return address;
		}
	}

	#endregion

	#region AddressSelect

	public class AddressSelect<TAddressIdField, TAsMainField, TAccountIdField> : AddressSelectBase
		where TAddressIdField : IBqlField
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		public AddressSelect(PXGraph graph)
			: base(graph)
		{
		}

		protected override Type AccountIdField => typeof(TAccountIdField);

		protected override Type AsMainField => typeof(TAsMainField);

		protected override Type AddressIdField => typeof(TAddressIdField);

		protected override Type IncorrectPersistableDAC => typeof(TAddressIdField);

		protected override PXView GetView(PXGraph graph)
		{
			return new PXView(graph, false,
				new Select<Address, Where<Address.addressID, Equal<Current<TAddressIdField>>>>());
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected override object GetPrimaryRow()
		{
			return _Graph.Caches[ItemType].Current;
		}
	}

	#endregion

	#region AddressSelect2

	[Obsolete(InternalMessages.ClassIsObsoleteAndWillBeRemoved2019R2
		+ " Use " + nameof(BusinessAccountLocationAddressSelect) + " or "
		+ nameof(AddressSelect<IBqlField, IBqlField, IBqlField>) + " instead.")]
	public sealed class AddressSelect2<TAddressIdFieldSearch, TAsMainField, TAccountIdField> : AddressSelectBase
		where TAddressIdFieldSearch : IBqlSearch
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		private Type _addressIdField;
		private PXView _select;

		public AddressSelect2(PXGraph graph)
			: base(graph)
		{
		}

		private void Initialize()
		{
			if (_addressIdField != null) return;

			var search = BqlCommand.CreateInstance(typeof(TAddressIdFieldSearch));
			_addressIdField = ((IBqlSearch)search).GetField();
			_select = new PXView(_Graph, false, GetSelectCommand(search));
		}

		private BqlCommand GetSelectCommand(BqlCommand search)
		{
			var arr = BqlCommand.Decompose(search.GetType());
			if (arr.Length < 2)
				throw new Exception("Unsupported search command detected");

			Type oldCommand = arr[0];
			Type newCommand = null;
			if (oldCommand == typeof(Search<,>))
				newCommand = typeof(Select<,>);
			if (oldCommand == typeof(Search2<,>))
				newCommand = typeof(Select2<,>);
			if (oldCommand == typeof(Search2<,,>))
				newCommand = typeof(Select2<,,>);

			if (newCommand == null)
				throw new Exception("Unsupported search command detected");

			arr[0] = newCommand;
			arr[1] = arr[1].DeclaringType;
			return BqlCommand.CreateInstance(arr);
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type AddressIdField
		{
			get
			{
				Initialize();
				return _addressIdField;
			}
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TAddressIdFieldSearch); }
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected override object GetPrimaryRow()
		{
			var record = _select.SelectSingle();
			if (record is PXResult)
				record = ((PXResult)record)[ItemType];
			return record;
		}

		[Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2019R2)]
		protected override IEnumerable SelectDelegate()
		{
			PXCache primaryCache = _Graph.Caches[_itemType];
			object primaryRecord = GetPrimaryRow();
			object currentAddressId = primaryCache.GetValue(primaryRecord, _addressIdFieldOrdinal);
			PXCache addrCache = _Graph.Caches<Address>();
			foreach (Address addr in PXSelect<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(_Graph, currentAddressId))
			{
				Address a = addr;
				bool? asMain = (bool?)primaryCache.GetValue(primaryRecord, _asMainFieldOrdinal);
				if (asMain == true)
				{
					a = PXCache<Address>.CreateCopy(a);
					PXUIFieldAttribute.SetEnabled(addrCache, a, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled(addrCache, a, true);
				}
				yield return a;
			}
		}

	}

	#endregion

	#region ContactSelectBase

	public abstract class ContactSelectBase : PXSelectBase
	{
		protected Type _itemType;

		protected int _contactIdFieldOrdinal;
		protected int _asMainFieldOrdinal;
		private int _accountIdFieldOrdinal;

		protected ContactSelectBase(PXGraph graph)
		{
			Initialize(graph);
			CreateView(graph);
			AttacheHandlers(graph);
		}

		public bool DoNotCorrectUI { get; set; }

		private void Initialize(PXGraph graph)
		{
			_Graph = graph;
			_Graph.EnsureCachePersistence(typeof(Contact));
			_Graph.Initialized += sender => sender.Views.Caches.Remove(IncorrectPersistableDAC);

			var contactIdDAC = GetDAC(ContactIdField);
			var asMainDAC = GetDAC(AsMainField);
			var accounDAC = GetDAC(AccountIdField);
			if (contactIdDAC != asMainDAC || asMainDAC != accounDAC)
				throw new Exception(string.Format("Fields '{0}', '{1}' and '{2}' are defined in different DACs",
					contactIdDAC.Name, asMainDAC.Name, accounDAC));
			_itemType = contactIdDAC;

			var cache = _Graph.Caches[_itemType];
			_contactIdFieldOrdinal = cache.GetFieldOrdinal(ContactIdField.Name);
			_asMainFieldOrdinal = cache.GetFieldOrdinal(AsMainField.Name);
			_accountIdFieldOrdinal = cache.GetFieldOrdinal(AccountIdField.Name);
		}

		protected abstract Type AccountIdField { get; }

		protected abstract Type AsMainField { get; }

		protected abstract Type ContactIdField { get; }

		protected abstract Type IncorrectPersistableDAC { get; }

		private static Type GetDAC(Type type)
		{
			var res = type.DeclaringType;
			if (res == null)
				throw new Exception(string.Format("DAC for field '{0}' can not be found", type.Name));
			return res;
		}

		private void CreateView(PXGraph graph)
		{
			var command = new Select<Contact>();
			View = new PXView(graph, false, command, new PXSelectDelegate(SelectDelegate));
		}

		private void AttacheHandlers(PXGraph graph)
		{
			graph.RowInserted.AddHandler(_itemType, RowInsertedHandler);
			graph.RowUpdating.AddHandler(_itemType, RowUpdatingHandler);
			graph.RowUpdated.AddHandler(_itemType, RowUpdatedHandler);
			graph.RowSelected.AddHandler(_itemType, RowSelectedHandler);
			graph.RowDeleted.AddHandler(_itemType, RowDeletedHandler);
		}

		private void RowDeletedHandler(PXCache sender, PXRowDeletedEventArgs e)
		{
			var currentContactId = sender.GetValue(e.Row, _contactIdFieldOrdinal);
			var isMainContact = sender.GetValue(e.Row, AsMainField.Name) as bool?;
			if (isMainContact == true) return;

			var currentContact = currentContactId.
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			if (currentContact != null)
			{
				var contactCache = sender.Graph.Caches[typeof(Contact)];
				contactCache.Delete(currentContact);
			}
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var contactCache = sender.Graph.Caches[typeof(Contact)];
			var asMain = false;

			var accountId = sender.GetValue(e.Row, _accountIdFieldOrdinal);

			var currentContactId = sender.GetValue(e.Row, _contactIdFieldOrdinal);
			var currentContact = currentContactId.
				With(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			if (currentContactId != null && currentContact != null)
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountContactId = account.With(_ => _.DefContactID);
				if (currentContactId.Equals(accountContactId))
				{
					asMain = true;
				}
			}

			sender.SetValue(e.Row, _asMainFieldOrdinal, asMain);
			if (!DoNotCorrectUI) PXUIFieldAttribute.SetEnabled(contactCache, currentContact, !asMain);
		}

		protected virtual IEnumerable SelectDelegate()
		{
			var primaryCache = _Graph.Caches[_itemType];
			var primaryRecord = GetPrimaryRow();
			var currentContactId = primaryCache.GetValue(primaryRecord, _contactIdFieldOrdinal);

			yield return (Contact)PXSelect<Contact,
				Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(_Graph, currentContactId);
		}

		protected Type ItemType
		{
			get { return _itemType; }
		}

		protected abstract object GetPrimaryRow();

		private void RowInsertedHandler(PXCache sender, PXRowInsertedEventArgs e)
		{
			var row = e.Row;
			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.With(_ => (BAccount)PXSelect<BAccount,
				Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountContactId = account.With(_ => _.DefContactID);
			var accountContact = accountContactId.
				With<int?, Contact>(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));
			var currentContactId = sender.GetValue(row, _contactIdFieldOrdinal);

			if (accountContact == null)
			{
				asMain = false;
				sender.SetValue(row, _asMainFieldOrdinal, false);
			}

			var contactCache = sender.Graph.Caches[typeof(Contact)];
			if (accountContact != null && true.Equals(asMain))
			{
				if (currentContactId != null && !object.Equals(currentContactId, accountContactId))
				{
					var currentContact = (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(sender.Graph, currentContactId);
					var oldDirty = contactCache.IsDirty;
					contactCache.Delete(currentContact);
					contactCache.IsDirty = oldDirty;
				}
				sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
			}
			else
			{
				if (currentContactId == null || object.Equals(currentContactId, accountContactId))
				{
					var oldDirty = contactCache.IsDirty;
					Contact cnt;
					if (accountContact != null)
					{
						cnt = (Contact)contactCache.CreateCopy(accountContact);
					}
					else
					{
						cnt = (Contact)contactCache.CreateInstance();
					}
					cnt.ContactID = null;
					cnt.BAccountID = (int?)accountId;
					cnt = (Contact)contactCache.Insert(cnt);

					sender.SetValue(row, _contactIdFieldOrdinal, cnt.ContactID);
					contactCache.IsDirty = oldDirty;
				}
			}
		}

		private void RowUpdatingHandler(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var row = e.NewRow;
			var oldRow = e.Row;

			var asMain = sender.GetValue(row, _asMainFieldOrdinal);
			var oldAsMain = sender.GetValue(oldRow, _asMainFieldOrdinal);

			var contactId = sender.GetValue(row, _contactIdFieldOrdinal);

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var account = accountId.
				With(_ => (BAccount)PXSelect<BAccount,
					Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
				Select(_Graph, _));
			var accountContactId = account.With(_ => _.DefContactID);
			var accountContact = accountContactId.
				With<int?, Contact>(_ => (Contact)PXSelect<Contact,
					Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
				Select(sender.Graph, _));

			var oldAccountId = sender.GetValue(oldRow, _accountIdFieldOrdinal);
			if (!object.Equals(accountId, oldAccountId))
			{
				var oldContactId = sender.GetValue(row, _contactIdFieldOrdinal);
				var oldAccount = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var oldAccountContactId = oldAccount.With(_ => _.DefContactID);
				var oldAccountContact = oldAccountContactId.
					With<int?, Contact>(_ => (Contact)PXSelect<Contact,
						Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
					Select(sender.Graph, _));
				oldAsMain = oldAccountContact != null && object.Equals(oldContactId, oldAccountContactId);
				if (true.Equals(oldAsMain))
				{
					asMain = true;
					contactId = accountContactId;
					sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
				}
			}

			if (true.Equals(asMain))
			{

				if (accountContact == null)
				{
					asMain = false;
					sender.SetValue(row, _asMainFieldOrdinal, false);
				}
			}

			if (!object.Equals(asMain, oldAsMain))
			{
				if (true.Equals(asMain))
				{
					sender.SetValue(row, _contactIdFieldOrdinal, accountContactId);
				}
				else
				{
					if (object.Equals(accountContactId, contactId))
						sender.SetValue(row, _contactIdFieldOrdinal, null);
				}
			}
		}

		private void RowUpdatedHandler(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row;
			var oldRow = e.OldRow;

			var accountId = sender.GetValue(row, _accountIdFieldOrdinal);
			var contactId = sender.GetValue(row, _contactIdFieldOrdinal);
			var oldContactId = sender.GetValue(oldRow, _contactIdFieldOrdinal);
			var contactCache = _Graph.Caches[typeof(Contact)];
			if (!object.Equals(contactId, oldContactId))
			{
				var account = accountId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>.
					Select(_Graph, _));
				var accountContactId = account.With(_ => _.DefContactID);
				var accountWithDefContact = oldContactId.
					With(_ => (BAccount)PXSelect<BAccount,
						Where<BAccount.defContactID, Equal<Required<BAccount.defContactID>>>>.
					Select(_Graph, _));
				if (accountWithDefContact == null)
				{
					var oldContact = oldContactId.
						With(_ => (Contact)PXSelect<Contact,
							Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(_Graph, _));
					if (oldContact != null)
					{
						var oldIsDirty = contactCache.IsDirty;
						if (contactCache.GetStatus(oldContact) == PXEntryStatus.Inserted)
						contactCache.Delete(oldContact);
						contactCache.IsDirty = oldIsDirty;
					}
				}

				if (contactId == null)
				{
					var oldDirty = contactCache.IsDirty;
					Contact cnt;
					var accountContact = accountContactId.
						With<int?, Contact>(_ => (Contact)PXSelect<Contact,
							Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.
						Select(_Graph, _));
					if (accountContact != null && object.Equals(accountContactId, oldContactId))
					{
						cnt = (Contact)contactCache.CreateCopy(accountContact);
					}
					else
					{
						cnt = (Contact)contactCache.CreateInstance();
					}
					if (cnt != null)
					{
						cnt.ContactID = null;
						cnt.DefAddressID = null;
						cnt.BAccountID = (int?)accountId;
						cnt = (Contact)contactCache.Insert(cnt);

						sender.SetValue(row, _contactIdFieldOrdinal, cnt.ContactID);
						sender.SetValue(row, _asMainFieldOrdinal, false);
						contactCache.IsDirty = oldDirty;
						contactId = cnt.ContactID;
					}
				}
			}
			if (contactId == null)
			{
				var oldDirty = contactCache.IsDirty;
				var cnt = (Contact)contactCache.CreateInstance();
				cnt.ContactID = null;
				cnt.BAccountID = (int?)accountId;
				cnt = (Contact)contactCache.Insert(cnt);

				sender.SetValue(row, _contactIdFieldOrdinal, cnt.ContactID);
				sender.SetValue(row, _asMainFieldOrdinal, false);
				contactCache.IsDirty = oldDirty;
			}
		}
	}

	#endregion

	#region ContactSelect

	public sealed class ContactSelect<TContactIdField, TAsMainField, TAccountIdField> : ContactSelectBase
		where TContactIdField : IBqlField
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		public ContactSelect(PXGraph graph)
			: base(graph)
		{
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type ContactIdField
		{
			get { return typeof(TContactIdField); }
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TContactIdField); }
		}

		protected override object GetPrimaryRow()
		{
			return _Graph.Caches[ItemType].Current;
		}
	}

	#endregion

	#region ContactSelect2

	public sealed class ContactSelect2<TContactIdFieldSearch, TAsMainField, TAccountIdField> : ContactSelectBase
		where TContactIdFieldSearch : IBqlSearch
		where TAsMainField : IBqlField
		where TAccountIdField : IBqlField
	{
		private Type _addressIdField;
		private PXView _select;

		public ContactSelect2(PXGraph graph)
			: base(graph)
		{
		}

		private void Initialize()
		{
			if (_addressIdField != null) return;

			var search = BqlCommand.CreateInstance(typeof(TContactIdFieldSearch));
			_addressIdField = ((IBqlSearch)search).GetField();
			_select = new PXView(_Graph, false, GetSelectCommand(search));
		}

		private BqlCommand GetSelectCommand(BqlCommand search)
		{
			var arr = BqlCommand.Decompose(search.GetType());
			if (arr.Length < 2)
				throw new Exception("Unsupported search command detected");

			Type oldCommand = arr[0];
			Type newCommand = null;
			if (oldCommand == typeof(Search<,>))
				newCommand = typeof(Select<,>);
			if (oldCommand == typeof(Search2<,>))
				newCommand = typeof(Select2<,>);
			if (oldCommand == typeof(Search2<,,>))
				newCommand = typeof(Select2<,,>);

			if (newCommand == null)
				throw new Exception("Unsupported search command detected");

			arr[0] = newCommand;
			arr[1] = arr[1].DeclaringType;
			return BqlCommand.CreateInstance(arr);
		}

		protected override Type AccountIdField
		{
			get { return typeof(TAccountIdField); }
		}

		protected override Type AsMainField
		{
			get { return typeof(TAsMainField); }
		}

		protected override Type ContactIdField
		{
			get
			{
				Initialize();
				return _addressIdField;
			}
		}

		protected override Type IncorrectPersistableDAC
		{
			get { return typeof(TContactIdFieldSearch); }
		}

		protected override object GetPrimaryRow()
		{
			object record = _select.SelectSingle();
			if (record is PXResult)
				record = ((PXResult)record)[ItemType];
			return record;
		}

		protected override IEnumerable SelectDelegate()
		{
			PXCache primaryCache = _Graph.Caches[_itemType];
			object primaryRecord = GetPrimaryRow();
			object currentContactId = primaryCache.GetValue(primaryRecord, _contactIdFieldOrdinal);
			PXCache contactCache = _Graph.Caches<Contact>();
			foreach (Contact cnt in PXSelect<Contact, Where<Contact.contactID, Equal<Required<Contact.contactID>>>>.Select(_Graph, currentContactId))
			{
				Contact c = cnt;
				bool? asMain = (bool?)primaryCache.GetValue(primaryRecord, _asMainFieldOrdinal);
				if (asMain == true)
				{
					c = PXCache<Contact>.CreateCopy(c);
					PXUIFieldAttribute.SetEnabled(contactCache, c, false);
				}
				else
				{
					PXUIFieldAttribute.SetEnabled(contactCache, c, true);
				}
				yield return c;
			}
		}
	}

	#endregion

	#region PXOwnerFilteredSelect

	public class PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID> : PXSelectBase
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		private BqlCommand _command;
		private Type _selectTarget;
		private Type _newRecordTarget;

		public PXOwnerFilteredSelect(PXGraph graph)
			: this(graph, false)
		{

		}

		protected PXOwnerFilteredSelect(PXGraph graph, bool readOnly)
			: base()
		{
			_Graph = graph;

			InitializeView(readOnly);
			InitializeSelectTarget();
			AppendActions();
			AppendEventHandlers();
		}

		public Type NewRecordTarget
		{
			get { return _newRecordTarget; }
			set
			{
				if (value != null)
				{
					if (!typeof(PXGraph).IsAssignableFrom(value))
						throw new ArgumentException(string.Format("{0} is excpected", typeof(PXGraph).GetLongName()), "value");
					if (value.GetConstructor(new Type[0]) == null)
						throw new ArgumentException("Default constructor is excpected", "value");
				}
				_newRecordTarget = value;
			}
		}

		private void AppendEventHandlers()
		{
			_Graph.RowSelected.AddHandler<TFilter>(RowSelectedHandler);
		}

		private void RowSelectedHandler(PXCache sender, PXRowSelectedEventArgs e)
		{
			var me = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myOwner).Name));
			var myGroup = true.Equals(sender.GetValue(e.Row, typeof(OwnedFilter.myWorkGroup).Name));

			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.ownerID).Name, !me);
			PXUIFieldAttribute.SetEnabled(sender, e.Row, typeof(OwnedFilter.workGroupID).Name, !myGroup);
		}

		private void AppendActions()
		{
			_Graph.Initialized += sender =>
			{
				var name = _Graph.ViewNames[View] + "_AddNew";
				PXNamedAction.AddAction(_Graph, typeof(TFilter), name, Messages.AddNew, new PXButtonDelegate(AddNewHandler));
			};
		}

		[PXButton(Tooltip = Messages.AddNewRecordToolTip, CommitChanges = true)]
		private IEnumerable AddNewHandler(PXAdapter adapter)
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (NewRecordTarget != null && _selectTarget != null && currentFilter != null)
			{
				var currentOwnerId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.ownerID).Name);
				var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.workGroupID).Name);

				var targetGraph = (PXGraph)PXGraph.CreateInstance(NewRecordTarget);
				var targetCache = targetGraph.Caches[_selectTarget];
				var row = targetCache.Insert();
				var newRow = targetCache.CreateCopy(row);

				EPCompanyTreeMember member = PXSelect<EPCompanyTreeMember,
												Where<EPCompanyTreeMember.userID, Equal<Required<OwnedFilter.ownerID>>,
												  And<EPCompanyTreeMember.workGroupID, Equal<Required<OwnedFilter.workGroupID>>>>>.
				Select(targetGraph, currentOwnerId, currentWorkgroupId);
				if (member == null) currentOwnerId = null;

				targetCache.SetValue(newRow, typeof(TGroupID).Name, currentWorkgroupId);
				targetCache.SetValue(newRow, typeof(TOwnerID).Name, currentOwnerId);
				targetCache.Update(newRow);
				PXRedirectHelper.TryRedirect(targetGraph, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}

		private void InitializeSelectTarget()
		{
			var selectTabels = _command.GetTables();
			if (selectTabels == null || selectTabels.Length == 0)
				throw new Exception("Primary table of given select command cannot be found");

			_selectTarget = selectTabels[0];
			_Graph.EnsureCachePersistence(_selectTarget);
		}

		private void InitializeView(bool readOnly)
		{
			_command = CreateCommand();
			View = new PXView(_Graph, readOnly, _command, new PXSelectDelegate(Handler));
		}

		private IEnumerable Handler()
		{
			var filterCache = _Graph.Caches[typeof(TFilter)];
			var currentFilter = filterCache.Current;
			if (filterCache.Current == null) return new object[0];

			var parameters = GetParameters(filterCache, currentFilter);

			return _Graph.QuickSelect(_command, parameters);
		}

		private static object[] GetParameters(PXCache filterCache, object currentFilter)
		{
			var currentOwnerId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.ownerID).Name);
			var currentWorkgroupId = filterCache.GetValue(currentFilter, typeof(OwnedFilter.workGroupID).Name);
			var currentMyWorkgroup = filterCache.GetValue(currentFilter, typeof(OwnedFilter.myWorkGroup).Name);
			var parameters = new object[]
				{
					currentOwnerId, currentOwnerId,
					currentMyWorkgroup, currentMyWorkgroup,
					currentWorkgroupId, currentWorkgroupId, currentMyWorkgroup
				};
			return parameters;
		}

		private static BqlCommand CreateCommand()
		{
			var command = BqlCommand.CreateInstance(typeof(TSelect));
			var additionalCondition = BqlCommand.Compose(
				typeof(Where2<Where<Required<OwnedFilter.ownerID>, IsNull,
								Or<Required<OwnedFilter.ownerID>, Equal<TOwnerID>>>,
							And<
								Where2<
									Where<Required<OwnedFilter.myWorkGroup>, IsNull,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<False>>>,
									And2<
										Where<Required<OwnedFilter.workGroupID>, IsNull,
											Or<TGroupID, Equal<Required<OwnedFilter.workGroupID>>>>,
										Or<Required<OwnedFilter.myWorkGroup>, Equal<True>,
									And<TGroupID, InMember<Current<AccessInfo.userID>>>>>>>>));
			return command.WhereAnd(additionalCondition);
		}
	}

	#endregion

	#region PXOwnerFilteredSelectReadonly

	public class PXOwnerFilteredSelectReadonly<TFilter, TSelect, TGroupID, TOwnerID>
		: PXOwnerFilteredSelect<TFilter, TSelect, TGroupID, TOwnerID>
		where TFilter : OwnedFilter, new()
		where TSelect : IBqlSelect
		where TGroupID : IBqlField
		where TOwnerID : IBqlField
	{
		public PXOwnerFilteredSelectReadonly(PXGraph graph)
			: base(graph, true)
		{
		}
	}

	#endregion

	#region CRLastNameDefaultAttribute

	internal sealed class CRLastNameDefaultAttribute : PXEventSubscriberAttribute, IPXRowPersistingSubscriber
	{
		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var contactType = sender.GetValue(e.Row, typeof(Contact.contactType).Name);
			var val = sender.GetValue(e.Row, _FieldOrdinal) as string;
			if (contactType != null && (contactType.Equals(ContactTypesAttribute.Lead) || contactType.Equals(ContactTypesAttribute.Person)) && string.IsNullOrWhiteSpace(val))
			{
				if (sender.RaiseExceptionHandling(_FieldName, e.Row, null, new PXSetPropertyKeepPreviousException(PXMessages.LocalizeFormat(ErrorMessages.FieldIsEmpty, _FieldName))))
				{
					throw new PXRowPersistingException(_FieldName, null, ErrorMessages.FieldIsEmpty, _FieldName);
				}
			}
		}
	}

	#endregion

	#region CRContactBAccountDefaultAttribute

	internal sealed class CRContactBAccountDefaultAttribute : PXEventSubscriberAttribute, IPXRowInsertingSubscriber, IPXRowUpdatingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber
	{
		private Dictionary<object, object> _persistedItems;

		public override void CacheAttached(PXCache sender)
		{
			_persistedItems = new Dictionary<object, object>();
			sender.Graph.RowPersisting.AddHandler(typeof(BAccount), SourceRowPersisting);
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			SetDefaultValue(sender, e.Row);
		}

		private void SetDefaultValue(PXCache sender, object row)
		{
			if (IsLeadOrPerson(sender, row)) return;

			var val = sender.GetValue(row, _FieldOrdinal);
			if (val != null) return;

			PXCache cache = sender.Graph.Caches[typeof(BAccount)];
			if (cache.Current != null)
			{
				var newValue = cache.GetValue(cache.Current, typeof(BAccount.bAccountID).Name);
				sender.SetValue(row, _FieldOrdinal, newValue);
			}
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				(e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						key = sender.Graph.Caches[typeof(BAccount)].GetValue(parent, typeof(BAccount.bAccountID).Name);
						sender.SetValue(e.Row, _FieldOrdinal, key);
						if (key != null)
						{
							_persistedItems[key] = parent;
						}
					}
				}
			}
			if (((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && true ||
				 (e.Operation & PXDBOperation.Command) == PXDBOperation.Update && true) &&
				sender.GetValue(e.Row, _FieldOrdinal) == null)
			{
				throw new PXRowPersistingException(_FieldName, null, PXMessages.LocalizeFormatNoPrefixNLA(Messages.EmptyValueErrorFormat, _FieldName));
			}
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Aborted)
			{
				object key = sender.GetValue(e.Row, _FieldOrdinal);
				if (key != null)
				{
					object parent;
					if (_persistedItems.TryGetValue(key, out parent))
					{
						var sourceField = typeof(BAccount.bAccountID).Name;
						sender.SetValue(e.Row, _FieldOrdinal, sender.Graph.Caches[typeof(BAccount)].GetValue(parent, sourceField));
					}
				}
			}
		}

		private void SourceRowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (IsLeadOrPerson(sender, e.Row)) return;

			var sourceField = typeof(BAccount.bAccountID).Name;
			object key = sender.GetValue(e.Row, sourceField);
			if (key != null)
				_persistedItems[key] = e.Row;
		}

		private bool IsLeadOrPerson(PXCache sender, object row)
		{
			var contactType = sender.GetValue(row, typeof(Contact.contactType).Name);
			return contactType != null &&
				(contactType.Equals(ContactTypesAttribute.Lead) ||
					contactType.Equals(ContactTypesAttribute.Person));
		}
	}

	#endregion

	#region BAccountType Attribute
	public class BAccountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { VendorType, CustomerType, CombinedType, ProspectType },
				new string[] { Messages.VendorType, Messages.CustomerType, Messages.CombinedType, Messages.ProspectType })
			{; }
		}

		public class SalesPersonTypeListAttribute : PXStringListAttribute
		{
			public SalesPersonTypeListAttribute()
				: base(
				new string[] { VendorType, EmployeeType },
				new string[] { Messages.VendorType, Messages.EmployeeType })
			{; }
		}

		public const string VendorType = "VE";
		public const string CustomerType = "CU";
		public const string CombinedType = "VC";
		public const string EmployeeType = "EP";
		public const string EmpCombinedType = "EC";
		public const string ProspectType = "PR";
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string CompanyType = "CP";
		public const string BranchType = "CP";
		public const string OrganizationType = "OR";
		public const string OrganizationBranchCombinedType = "OB";

		public class vendorType : PX.Data.BQL.BqlString.Constant<vendorType>
		{
			public vendorType() : base(VendorType) {; }
		}
		public class customerType : PX.Data.BQL.BqlString.Constant<customerType>
		{
			public customerType() : base(CustomerType) {; }
		}
		public class combinedType : PX.Data.BQL.BqlString.Constant<combinedType>
		{
			public combinedType() : base(CombinedType) {; }
		}
		public class employeeType : PX.Data.BQL.BqlString.Constant<employeeType>
		{
			public employeeType() : base(EmployeeType) {; }
		}
		public class empCombinedType : PX.Data.BQL.BqlString.Constant<empCombinedType>
		{
			public empCombinedType() : base(EmpCombinedType) {; }
		}
		public class prospectType : PX.Data.BQL.BqlString.Constant<prospectType>
		{
			public prospectType() : base(ProspectType) {; }
		}
		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public class companyType : PX.Data.BQL.BqlString.Constant<companyType>
		{
			public companyType() : base(CompanyType) {; }
		}
		public class branchType : PX.Data.BQL.BqlString.Constant<branchType>
		{
			public branchType() : base(BranchType) {; }
		}
		public class organizationType : PX.Data.BQL.BqlString.Constant<organizationType>
		{
			public organizationType() : base(OrganizationType) {; }
		}
		public class organizationBranchCombinedType : PX.Data.BQL.BqlString.Constant<organizationBranchCombinedType>
		{
			public organizationBranchCombinedType() : base(OrganizationBranchCombinedType) {; }
		}
	}
	#endregion

	#region IActivityMaint

	public interface IActivityMaint
	{
		void CancelRow(CRActivity row);
		void CompleteRow(CRActivity row);
	}

	#endregion

	#region SelectContactEmailSync

	public class SelectContactEmailSync<TWhere> : PXSelectBase<Contact>
		where TWhere : IBqlWhere, new()
	{
		public SelectContactEmailSync(PXGraph graph, Delegate handler)
			: this(graph)
		{
			View = new PXView(_Graph, false, View.BqlSelect, handler);
		}

		public SelectContactEmailSync(PXGraph graph)
		{
			_Graph = graph;
			View = new PXView(_Graph, false, new Select<Contact>());
			View.WhereAnd<TWhere>();

			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.eMail).Name, FieldUpdated<Contact.eMail, Users.email>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.firstName).Name, FieldUpdated<Contact.firstName, Users.firstName>);
			_Graph.FieldUpdated.AddHandler(typeof(Contact), typeof(Contact.lastName).Name, FieldUpdated<Contact.lastName, Users.lastName>);
		}

		protected virtual void FieldUpdated<TSrcField, TDstField>(PXCache sender, PXFieldUpdatedEventArgs e)
			where TSrcField : IBqlField
			where TDstField : IBqlField
		{
			Contact row = (Contact)e.Row;
			Users user = PXSelect<Users, Where<Users.pKID, Equal<Current<Contact.userID>>>>.SelectSingleBound(_Graph, new object[] { row });
			if (user != null)
			{
				PXCache usercache = _Graph.Caches[typeof(Users)];
				usercache.SetValue<TDstField>(user, sender.GetValue<TSrcField>(row));
				usercache.Update(user);
			}
		}
	}

	#endregion

	#region CRCampaignMembersList
	
	public class CRMarketingListMembersList : CRMembersList<CRMarketingList, CRMarketingListMember, OperationParam>
	{
		private readonly PXView helperView;

		private PXView filteredView
		{
			get
			{
				return _Graph.Views[nameof(CRMarketingListMaint.FilteredItems)];
			}
		}

		#region Ctor

		public CRMarketingListMembersList(PXGraph graph) : base(graph)
		{
			View = new PXView(_Graph, false,
				new Select2<CRMarketingListMember,
					InnerJoin<Contact, 
						On<Contact.contactID, Equal<CRMarketingListMember.contactID>>,
					LeftJoin<BAccount, On<
						Where2<Where<Contact.contactType, NotEqual<ContactTypesAttribute.employee>, And<BAccount.bAccountID, Equal<Contact.bAccountID>>>,
							Or<Where<Contact.contactType, Equal<ContactTypesAttribute.employee>,
								And<Contact.contactID, Equal<BAccount.defContactID>,
								And<Contact.bAccountID, Equal<BAccount.parentBAccountID>>>>>>>,
					LeftJoin<CRLead, On<CRLead.contactID.IsEqual<Contact.contactID>>>>>,
					Where<
						CRMarketingListMember.marketingListID, Equal<Optional<CRMarketingList.marketingListID>>>,
					OrderBy<
						Desc<CRMarketingListMember.isSubscribed,
						Asc<Contact.memberName>>>>(), new PXSelectDelegate(mailRecipients));

			helperView = new PXView(_Graph, false, View.BqlSelect);
		}

		#endregion

		#region Event Handlers

		protected override void TMain_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (primaryCurrent != null)
			{
				var canAddAndRemove = primaryCache.GetStatus(primaryCache.Current) != PXEntryStatus.Inserted && primaryCurrent.IsDynamic != true;
				_Graph.Actions[addAction].SetEnabled(canAddAndRemove);
			}

			var row = e.Row as CRMarketingList;
			if (row == null)
				return;
			_Graph.Actions[deleteAction].SetEnabled(row.IsDynamic != true && _Graph.Views[nameof(CRMarketingListMaint.MailRecipients)].SelectSingle() != null);
		}

		#endregion

		#region Delegates

		protected virtual IEnumerable mailRecipients()
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;
			var recipients = new PXResultset<CRMarketingListMember>();
			if (primaryCurrent.IsDynamic != true)
			{
				foreach (PXResult<CRMarketingListMember, Contact, BAccount> item in
					helperView.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					recipients.Add(item);
				}
			}
			else if (primaryCurrent.GIDesignID == null)
			{

				List<string> sorts = new List<string>();
				sorts.Add(nameof(CRMarketingListMember) + "__" + nameof(CRMarketingListMember.IsSubscribed));
				sorts.Add(nameof(Contact.MemberName));
				sorts.Add(nameof(Contact.ContactID));

				List<bool> descs = new List<bool>();
				descs.Add(true);
				descs.Add(false);
				descs.Add(false);


				var search = new List<object>()
				{
					null,
					null,
					null
				};
				search.AddRange(PXView.Searches);


				foreach (PXResult item in
					filteredView.Select(PXView.Currents, new object[] { }, search.ToArray(), sorts.ToArray(), descs.ToArray(), PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
					var contact = PXResult.Unwrap<Contact>(item);
					var member = PXResult.Unwrap<CRMarketingListMember>(item);
					var res = new PXResult<CRMarketingListMember, Contact, BAccount, CRLead>(
						member.ContactID != null
							? member
							: new CRMarketingListMember
								{
									MarketingListID = primaryCurrent.MarketingListID,
									ContactID = contact.ContactID,
									Format = NotificationFormat.Html,
									IsSubscribed = true,
									CreatedDateTime = contact.CreatedDateTime
								},
						item.GetItem<Contact>(),
						item.GetItem<BAccount>(),
						item.GetItem<CRLead>());
					recipients.Add(res);
				}
			}
			else
			{
				var descr = PXGenericInqGrph.Def.FirstOrDefault(x => x.DesignID == primaryCurrent.GIDesignID.Value);
				var contact = MainContactTableFromGI(descr);
                descr = (GIDescription)descr.Clone();
				
				List<string> sorts = new List<string>(PXView.SortColumns);
				sorts.Insert(0, nameof(CRMarketingListMember) + "_" + nameof(CRMarketingListMember.IsSubscribed));
				sorts.Insert(1, nameof(Contact) + "_" + nameof(Contact.DisplayName));
				for (int i = 0; i < sorts.Count; i++)
				{
					if (sorts[i].StartsWith("Contact__"))
						sorts[i] = contact.Alias + '_' + sorts[i].Substring("Contact__".Length);
				}
				List<bool> descs = new List<bool>(PXView.Descendings);
				descs.Insert(0, true);
				descs.Insert(1, false);

				PXFilterRow[] filters = null;
			    if (PXView.Filters != null)
			    {
			        filters = new PXFilterRow[PXView.Filters.Count()];
			        for (int i = 0; i < PXView.Filters.Count(); i++)
			        {
			            filters[i] = PXView.Filters[i];
                        if(filters[i].DataField.StartsWith("Contact__"))
			                filters[i].DataField = contact.Alias + '_' + filters[i].DataField.Substring("Contact__".Length);
			        }
			    }

				descr.Tables = new List<GITable>(descr.Tables)
				{
					new GITable()
					{
						DesignID = descr.DesignID,
						Alias = nameof(CRMarketingListMember),
						Name = typeof(CRMarketingListMember).FullName
					},
					new GITable()
					{
						DesignID = descr.DesignID,
						Alias = nameof(CRLead),
						Name = typeof(CRLead).FullName,
					},
				};

				descr.Relations = new List<GIRelation>(descr.Relations)
				{
					new GIRelation()
					{
						DesignID = descr.DesignID,
						LineNbr = 666,
						ParentTable = contact.Alias,
						ChildTable = nameof(CRMarketingListMember),
						IsActive = true,
						JoinType = "L"
					},
					new GIRelation()
					{
						DesignID = descr.DesignID,
						LineNbr = 667,
						ParentTable = contact.Alias,
						ChildTable = nameof(CRLead),
						IsActive = true,
						JoinType = "L",
					},
				};

				descr.Ons = new List<GIOn>(descr.Ons)
				{
					new GIOn()
					{
						DesignID = descr.DesignID,
						RelationNbr = 666,
						ParentField = nameof(Contact.contactID),
						Condition = "E",
						ChildField = nameof(CRMarketingListMember.contactID),
						Operation = "A"
					},
					new GIOn()
					{
						DesignID = descr.DesignID,
						RelationNbr = 667,
						ParentField = nameof(Contact.contactID),
						Condition = "E",
						ChildField = nameof(CRLead.contactID),
						Operation = "A"
					},
				};

				descr.GroupBys = new List<GIGroupBy>(descr.GroupBys)
				{
					new GIGroupBy()
					{
						DataFieldName = contact.Alias + "." + nameof(Contact.contactID)
					}
				};

				descr.Wheres = CreateGIFilter(this._Graph, contact.Alias, primaryCurrent.SharedGIFilter, descr.Wheres);

				var pxFilters = CreateFilter(this._Graph, null, filters).ToArray();

				descr.Results = new List<GIResult>()
				{
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.contactID)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.contactType)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.salutation)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.eMail)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.displayName)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.fullName)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.bAccountID)},
					new GIResult() {ObjectName = contact.Alias, Field = nameof(Contact.isActive)},

					new GIResult() {ObjectName = nameof(CRMarketingListMember), Field = nameof(CRMarketingListMember.contactID)},
					new GIResult() {ObjectName = nameof(CRMarketingListMember), Field = nameof(CRMarketingListMember.isSubscribed)},
					new GIResult() {ObjectName = nameof(CRMarketingListMember), Field = nameof(CRMarketingListMember.createdDateTime)},
				};
				
				var graph = PXGenericInqGrph.CreateInstance(descr);

				helperView.OrderByNew<OrderBy<Desc<CRMarketingListMember.isSubscribed, Asc<Contact.displayName>>>>();

				foreach (var item in graph.Results.View.Select(null, null, PXView.Searches, sorts.ToArray(), descs.ToArray(), pxFilters, ref startRow, PXView.MaximumRows, ref totalRows)
					.Select(x => (GenericResult)x))
				{
					var contactRec= (Contact)item.Values[contact.Alias];
					var memberRec = (CRMarketingListMember)item.Values[typeof(CRMarketingListMember).Name];
					var leadRec = (CRLead)item.Values[typeof(CRLead).Name];

					var bAccount = item.Values.ContainsKey(typeof(BAccount).Name) ? (BAccount)item.Values[typeof(BAccount).Name] : null;

					var res = new PXResult<CRMarketingListMember, Contact, BAccount, CRLead>(
						memberRec.ContactID != null
							? memberRec
							: new CRMarketingListMember
							{
								MarketingListID = primaryCurrent.MarketingListID,
								ContactID = contactRec.ContactID,
								Format = NotificationFormat.Html,
								IsSubscribed = true
							},
						contactRec, bAccount, leadRec);
					recipients.Add(res);
				}
			}
			PXView.StartRow = 0;
			return recipients;
		}

		private static double GetDayOfWeek(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Monday: return 0D;
				case DayOfWeek.Tuesday: return 1D;
				case DayOfWeek.Wednesday: return 2D;
				case DayOfWeek.Thursday: return 3D;
				case DayOfWeek.Friday: return 4D;
				case DayOfWeek.Saturday: return 5D;
				case DayOfWeek.Sunday: return 6D;
				default: return 0D;
			}
		}

		public static GITable MainContactTableFromGI(GIDescription descr)
		{
			var tblContact = descr.Tables.FirstOrDefault(_ => _.Name == typeof(Contact).FullName);
			var tblCRLead = descr.Tables.FirstOrDefault(_ => _.Name == typeof(CRLead).FullName);

			if (tblContact != null && !isTableLeftJoin(descr, tblContact.Alias))
			{
				return tblContact;
			}
			else if (tblCRLead != null && !isTableLeftJoin(descr, tblCRLead.Alias))
			{
				return tblCRLead;
			}
			else if(tblContact != null)
			{
				return tblContact;
			}
			else if (tblCRLead != null)
			{
				return tblCRLead;
			}
			else
			{
				return null;
			}
		}

		private static bool isTableLeftJoin(GIDescription descr, string alias)
		{
			if (alias == null) { return false; }

			foreach(GIRelation relation in descr.Relations)
			{
				if (relation.ChildTable == alias && relation.JoinType == "L")
				{
					return true;
				}
			}

			return false;
		}

		public static IEnumerable<GIWhere> CreateGIFilter(PXGraph graph, string contactAlias, long? sharedGIFilter, IEnumerable<GIWhere> where)
		{
			var filter = new List<GIWhere>();
			if (where.Any())
			{
				filter = where.ToList();
				filter[0].OpenBrackets += "(";
				filter[filter.Count() - 1].CloseBrackets += ")";
				filter[filter.Count() - 1].Operation = "A";
			}
			var giFilter = CreateFilter(graph, sharedGIFilter, null).ToArray();
			for (int i = 0; i < giFilter.Count(); i++)
			{
				var f = giFilter[i];
				var businessDate = graph.Accessinfo.BusinessDate ?? DateTime.Today;
				DateTime? startDate = null;
				DateTime? endDate = null;
				string condition = null;
				switch (f.Condition)
				{
					case PXCondition.EQ:
						condition = "E"; break;
					case PXCondition.NE:
						condition = "NE"; break;
					case PXCondition.GT:
						condition = "G"; break;
					case PXCondition.GE:
						condition = "GE"; break;
					case PXCondition.LT:
						condition = "L"; break;
					case PXCondition.LE:
						condition = "LE"; break;
					case PXCondition.BETWEEN:
						condition = "B"; break;
					case PXCondition.LIKE:
						condition = "LI"; break;
					case PXCondition.NOTLIKE:
						condition = "NL"; break;
					case PXCondition.RLIKE:
						condition = "RL"; break;
					case PXCondition.LLIKE:
						condition = "LL"; break;
					case PXCondition.ISNULL:
						condition = "NU"; break;
					case PXCondition.ISNOTNULL:
						condition = "NN"; break;
					case PXCondition.IN:
						condition = "IN"; break;

					case PXCondition.TODAY:
						startDate = businessDate;
						endDate = startDate.Value.AddDays(1D);
						condition = "B";
						break;
					case PXCondition.OVERDUE:
						startDate = null;
						endDate = businessDate;
						condition = "LE";
						break;
					case PXCondition.TODAY_OVERDUE:
						startDate = null;
						endDate = ((DateTime)businessDate).AddDays(1D);
						condition = "LE";
						break;
					case PXCondition.TOMMOROW:
						startDate = ((DateTime)businessDate).AddDays(1D);
						endDate = startDate.Value.AddDays(1D);
						condition = "B";
						break;
					case PXCondition.THIS_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(-GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						condition = "B";
						break;
					case PXCondition.NEXT_WEEK:
						startDate = businessDate;
						startDate = startDate.Value.AddDays(7D - GetDayOfWeek(startDate.Value.DayOfWeek));
						endDate = startDate.Value.AddDays(7D);
						condition = "B";
						break;
					case PXCondition.THIS_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						endDate = startDate.Value.AddMonths(1);
						condition = "B";
						break;
					case PXCondition.NEXT_MONTH:
						startDate = businessDate;
						startDate = new DateTime(startDate.Value.Year, startDate.Value.Month, 1);
						startDate = startDate.Value.AddMonths(1);
						endDate = startDate.Value.AddMonths(1);
						condition = "B";
						break;

				}
				if (endDate != null) endDate = endDate.Value.AddSeconds(-1D);

				var fieldName = f.DataField;
				//Replace only first char '_' 
				var ind = fieldName.IndexOf('_');
				if (ind >= 0)
					fieldName = fieldName.Substring(0, ind) + "." + fieldName.Substring(ind + 1, fieldName.Length - ind - 1);

				filter.Add(new GIWhere()
				{					
					LineNbr = 0,
					OpenBrackets = new string('(', f.OpenBrackets + (i == 0 ? 1 : 0)),
					CloseBrackets = new string(')', f.CloseBrackets + (i == giFilter.Count() - 1 ? 1 : 0)),
					DataFieldName = fieldName,
					Operation = f.OrOperator ? "O" : "A",
					Condition = condition,
					Value1 = startDate?.ToString("MM/dd/YYYY") ?? endDate?.ToString("MM/dd/YYYY") ?? f.Value?.ToString(),
					Value2 = endDate?.ToString("MM/dd/YYYY") ?? f.Value2?.ToString()
				});				
			}
			if (contactAlias != null)
				filter.Add(new GIWhere()
					{
						LineNbr = 0,
						DataFieldName = contactAlias + "." + nameof(Contact.contactID),
						Condition = "NN"
					}
				);
			return filter;
		}

		public static IEnumerable<PXFilterRow> CreateFilter(PXGraph graph, long? sharedGIFilter, IEnumerable clientFilter = null )
		{
			var pxFilters = new List<PXFilterRow>();
            
		    var filters = PXSelect<FilterRow, Where<FilterRow.filterID, Equal<Required<FilterRow.filterID>>,
				And<FilterRow.isUsed, Equal<True>>>>
		        .Select(graph, sharedGIFilter);
		    foreach (FilterRow f in filters)
		    {
		        pxFilters.Add(
		            new PXFilterRow
		            {
		                DataField = f.DataField,
		                OpenBrackets = f.OpenBrackets ?? 0,
		                CloseBrackets = f.CloseBrackets ?? 0,
		                OrigValue = f.ValueSt,
		                OrigValue2 = f.ValueSt2,
		                OrOperator = f.Operator == 1,
		                Value = f.ValueSt,
		                Value2 = f.ValueSt2,
		                Condition = (PXCondition) f.Condition,
		            });
		    }

		    if (clientFilter == null) return pxFilters;

		    int last = pxFilters.Count - 1;
		    if (last > 0)
		    {
		        pxFilters[0].OpenBrackets += 1;
		        pxFilters[last].CloseBrackets += 1;
		        pxFilters[last].OrOperator = false;
		    }
		    pxFilters.AddRange(clientFilter.Cast<PXFilterRow>());

		    last += 1;
		    if (pxFilters.Count > last)
		    {
		        pxFilters[last].OpenBrackets += 1;
		        pxFilters[pxFilters.Count - 1].CloseBrackets += 1;
		    }
		    return pxFilters;
		}

		#endregion

		#region Buttons

		public override IEnumerable MultipleInsert(PXAdapter adapter)
		{
			var row = primaryCache.Current as CRMarketingList;
			if (row == null) return adapter.Get();

			if (row.MarketingListID != null && primaryCache.GetStatus(row) != PXEntryStatus.Inserted)
			{
				filterCurrent.MarketingListID = row.MarketingListID;
				filterCurrent.MarketingListMemberAction = OperationParam.ActionList.Add;

			    AskExt((g, name) => g.Caches[typeof(Contact)].Clear());
            }
			return adapter.Get();
		}

		public override IEnumerable MultipleDelete(PXAdapter adapter)
		{
			var row = primaryCache.Current as CRMarketingList;
			if (row == null) return adapter.Get();

			if (row == null || row.MarketingListID == null || primaryCache.GetStatus(row) == PXEntryStatus.Inserted)
				return adapter.Get();

			filterCurrent.MarketingListMemberAction = OperationParam.ActionList.Delete;
			List<CRMarketingListMember> membersToDelete = new List<CRMarketingListMember>();
			var mailRecipientsCache = _Graph.Caches[typeof(CRMarketingListMember)];
			foreach (CRMarketingListMember member in ((IEnumerable<CRMarketingListMember>)mailRecipientsCache.Updated)
						.Concat<CRMarketingListMember>((IEnumerable<CRMarketingListMember>)mailRecipientsCache.Inserted))
			{
				if (member.Selected == true) membersToDelete.Add(member);
			}

			if (!membersToDelete.Any() && mailRecipientsCache.Current != null)
				membersToDelete.Add((CRMarketingListMember)mailRecipientsCache.Current);

			foreach (CRMarketingListMember member in membersToDelete)
				mailRecipientsCache.Delete(member);

			return adapter.Get();
		}

		#endregion

		#region Process

		protected override int ProcessList(List<Contact> list)
		{
			return PXProcessing.ProcessItems(list, item =>
			{
				OrderedDictionary doc = new OrderedDictionary();
				doc.Add("ContactID", item.ContactID);
				doc.Add("MarketingListID", filterCurrent.MarketingListID);
			    try
			    {
			        if (filterCurrent.MarketingListMemberAction == OperationParam.ActionList.Add)
			        {
			            membersCache.Update(doc, doc);
			        }
			        else
			        {
			            membersCache.Delete(doc, doc);
			        }
			    }
			    catch (Exception e)
			    {
                    PXTrace.WriteError(e);
			    }
			});
		}

		#endregion

	}
	public class CRCampaignMembersList : CRMembersList<CRCampaign, CRCampaignMembers, OperationParam>
	{
		#region Ctor

		public CRCampaignMembersList(PXGraph graph) : base(graph)
		{
			View = new PXView(_Graph, false,
				new Select2<CRCampaignMembers,
				LeftJoin<Contact,
					On<CRCampaignMembers.contactID, Equal<Contact.contactID>,
					And<CRCampaignMembers.campaignID, Equal<Current<CRCampaign.campaignID>>>>,
				LeftJoin<Address,
					On<Address.addressID, Equal<Contact.defAddressID>>>>,
				Where2<
					Where<Contact.contactType, Equal<ContactTypesAttribute.lead>,
						Or<Contact.contactType, Equal<ContactTypesAttribute.person>, 
						Or<Contact.contactType, Equal<ContactTypesAttribute.bAccountProperty>, 
						Or<Contact.contactType, Equal<ContactTypesAttribute.employee>>>>>,
					And<CRCampaignMembers.campaignID, Equal<Current<CRCampaign.campaignID>>>>>());
		}

		#endregion

		#region Event Handlers
		protected override void TMain_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			_Graph.Actions[deleteAction].SetEnabled(_Graph.Views[nameof(CampaignMaint.CampaignMembers)].SelectSingle() != null);
		}
		#endregion

		#region Buttons

		public override IEnumerable MultipleInsert(PXAdapter adapter)
		{
			var row = primaryCache.Current as CRCampaign;
			if (row == null) return adapter.Get();

			if (row.CampaignID != null && primaryCache.GetStatus(row) != PXEntryStatus.Inserted)
			{
				filterCurrent.CampaignID = row.CampaignID;
				filterCurrent.Action = OperationParam.ActionList.Add;

				AskExt( (g,name)=>g.Caches[typeof(Contact)].Clear() );
			}
			return adapter.Get();
		}

		public override IEnumerable MultipleDelete(PXAdapter adapter)
		{
			var row = primaryCache.Current as CRCampaign;
			if (row == null) return adapter.Get();

			if (row == null || row.CampaignID == null || primaryCache.GetStatus(row) == PXEntryStatus.Inserted)
				return adapter.Get();

			List<CRCampaignMembers> membersToDelete = new List<CRCampaignMembers>();

			var cacheMember = _Graph.Caches[typeof(CRCampaignMembers)];
			foreach (CRCampaignMembers member in ((IEnumerable<CRCampaignMembers>)cacheMember.Updated)
						.Concat<CRCampaignMembers>((IEnumerable<CRCampaignMembers>)cacheMember.Inserted))
			{
				if (member.Selected == true) membersToDelete.Add(member);
			}
		    
            if (!membersToDelete.Any() && cacheMember.Current != null)
				membersToDelete.Add((CRCampaignMembers)cacheMember.Current);

			foreach (CRCampaignMembers member in membersToDelete)
				cacheMember.Delete(member);

			return adapter.Get();
		}

		#endregion

		#region Process

		protected override int ProcessList(List<Contact> list)
		{
			return PXProcessing.ProcessItems(list, item =>
			{
				OrderedDictionary doc = new OrderedDictionary();
				doc.Add("CampaignID", filterCurrent.CampaignID);
				doc.Add("ContactID", item.ContactID);
				if (filterCurrent.Action == OperationParam.ActionList.Add)
				{
					membersCache.Update(doc, doc);
				}
				else
				{
					membersCache.Delete(doc, doc);
				}
			});
		}

		#endregion

	}

	public abstract class CRMembersList<TPrimary, TMember, TFilter> : PXSelectBase<TMember>
		where TPrimary : class, IBqlTable, new()
		where TMember : class, IBqlTable, new()
		where TFilter : OperationParam, new()
	{
		#region MainScreen

		public const string addAction = "AddAction";
		public const string deleteAction = "DeleteAction";

		#region Ctor

		protected CRMembersList(PXGraph graph)
		{
			_Graph = graph;

			_Graph.RowSelected.AddHandler<TPrimary>(TMain_RowSelected);

			AddAction(_Graph, addAction, Messages.AddNewMembers, MultipleInsert);
			AddAction(_Graph, deleteAction, Messages.DeleteSelected, MultipleDelete);
			
			InitAddMemberPanel();
		}

		#endregion

		internal PXAction AddAction(PXGraph graph, string name, string displayName, PXButtonDelegate handler)
		{
			PXUIFieldAttribute uiAtt = new PXUIFieldAttribute
			{
				DisplayName = PXMessages.LocalizeNoPrefix(displayName),
				MapEnableRights = PXCacheRights.Select
			};

			List<PXEventSubscriberAttribute> addAttrs = new List<PXEventSubscriberAttribute> { uiAtt };
			PXNamedAction<TPrimary> res = new PXNamedAction<TPrimary>(graph, name, handler, addAttrs.ToArray());
			graph.Actions[name] = res;
			return res;
		}

		#region Cache Handling

		protected PXCache primaryCache
		{
			get
			{
				return _Graph.Caches[typeof(TPrimary)];
			}
		}

		protected TPrimary primaryCurrent
		{
			get
			{
				return _Graph.Caches[typeof(TPrimary)].Current as TPrimary;
			}
		}
		
		protected PXCache membersCache
		{
			get
			{
				return _Graph.Caches[typeof(TMember)];
			}
		}

		protected PXCache filterCache
		{
			get
			{
				return _Graph.Caches[typeof(TFilter)];
			}
		}

		protected TFilter filterCurrent
		{
			get
			{
				return _Graph.Caches[typeof(TFilter)].Current as TFilter;
			}
		}

		#endregion

		#region Buttons

		public abstract IEnumerable MultipleInsert(PXAdapter adapter);

		public abstract IEnumerable MultipleDelete(PXAdapter adapter);

		#endregion

		#region Event Handlers

		protected virtual void TMain_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var canAddAndRemove = primaryCache.GetStatus(primaryCache.Current) != PXEntryStatus.Inserted;
			_Graph.Actions[addAction].SetEnabled(canAddAndRemove);
		}

		#endregion

		#endregion

		#region AddMemberPanel

		private PXView Items;
		
		private const string innerProcess = "InnerProcess";
		private const string innerProcessAll = "InnerProcessAll";
		private const string linkToContact = "LinkToContact";
		private const string linkToBAccount = "LinkToBAccount";

		#region Ctor

		protected void InitAddMemberPanel()
		{
			Items = new PXView(_Graph, false, BqlCommand.CreateInstance(typeof(Select<Contact>)), (PXSelectDelegate)items);

			_Graph.Views.Add(nameof(Items), Items);

			AddAction(_Graph, innerProcess, Messages.Add, InnerProcess);
			AddAction(_Graph, innerProcessAll, Messages.AddAll, InnerProcessAll);
			AddAction(_Graph, linkToContact, linkToContact, RedirectToContact);
			AddAction(_Graph, linkToBAccount, linkToBAccount, RedirectToBAccount);
			_Graph.Actions[linkToContact].SetEnabled(false);
			_Graph.Actions[linkToBAccount].SetEnabled(false);
			_Graph.Actions[linkToBAccount].SetVisible(false);


			_Graph.FieldUpdated.AddHandler<OperationParam.dataSource>(CampaignOperationParam_DataSource_FieldUpdated);
			_Graph.RowSelected.AddHandler<OperationParam>(CampaignOperationParam_RowSelected);
			_Graph.RowPersisting.AddHandler<Contact>(Contact_RowPersisting);
		}

		protected virtual IEnumerable RedirectToContact(PXAdapter adapter)
		{
			PXRedirectHelper.TryRedirect(_Graph, Items.Cache.Current, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}

		protected virtual IEnumerable RedirectToBAccount(PXAdapter adapter)
		{
			var graph = PXGraph.CreateInstance<BusinessAccountMaint>();
			var bAccount = PXSelect<BAccount, Where<BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
				.Select(_Graph, ((Contact)Items.Cache.Current).BAccountID);
			graph.BAccount.Current = bAccount;
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);
			return adapter.Get();
		}
		#endregion

		#region Process

		protected virtual IEnumerable InnerProcess(PXAdapter adapter)
		{
			var a = Items.Cache.Cached.Cast<Contact>().Where(_ => _.Selected == true).ToList();
			ProcessImpl(a.Any() ? a : ((Contact)Items.Cache.Current).SingleToList());
			return adapter.Get();
		}

		protected virtual IEnumerable InnerProcessAll(PXAdapter adapter)
		{
		    var filter = Items.GetExternalFilters();
		    int start = 0, total = 0;
            var a = Items.Select(null,null,null,null,null,filter, ref start, 0, ref total) .Cast<Contact>().ToList();
			ProcessImpl(a);
			return adapter.Get();
		}

		protected virtual void ProcessImpl(List<Contact> list)
		{
			int result = ProcessList(list);

			try
			{
				if (result > 0)
					_Graph.Actions.PressSave();
				Items.Cache.Clear();
				if (result != list.Count)
					throw new PXOperationCompletedWithErrorException(ErrorMessages.SeveralItemsFailed);
			}
			finally
			{
				membersCache.Clear();
			}
		}

		protected abstract int ProcessList(List<Contact> list);

		#endregion

		#region Data Handlers

	    protected virtual IEnumerable items()
	    {
	        foreach (Contact contact in itemSource())
	        {                
	            Contact cached = (Contact)this.Items.Cache.Locate(contact);
	            if (cached != null)
	                contact.Selected = cached.Selected;
	            yield return contact;
	        }
	    }
	    protected virtual IEnumerable itemSource()
        {
			int startRow = PXView.StartRow;
			int totalRows = 0;		  
			switch (filterCurrent?.DataSource)
			{
				case OperationParam.DataSourceList.Inquiry:
					return this.GetInquiryItems();
				case OperationParam.DataSourceList.MarketingLists:
					if (filterCurrent.SourceMarketingListID == null)
						return Enumerable.Empty<Contact>();

					var marketingGraph = new CRMarketingListMaint();
					marketingGraph.MailLists.Current = PXSelect<CRMarketingList,
						Where<CRMarketingList.marketingListID, Equal<Required<CRMarketingList.marketingListID>>>>
						.Select(_Graph, filterCurrent.SourceMarketingListID);

					var res = marketingGraph.MailRecipients.View.Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows); 
					PXView.StartRow = 0;
					return res.RowCast<Contact>();
				default:
					return Enumerable.Empty<Contact>();
			}
		}

		private IEnumerable<object> GetInquiryItems()
		{
			int startRow = PXView.StartRow;
			int totalRows = 0;
			using (new PXPreserveScope())
			{
				var designID = filterCurrent?.ContactGI;
				if (designID == null)
					return Enumerable.Empty<Contact>();

				var descr = PXGenericInqGrph.Def.FirstOrDefault(x => x.DesignID == designID.Value);
				descr = (GIDescription)descr.Clone();
				var gIResults = new List<Data.Maintenance.GI.GIResult>();
				var contact = CRMarketingListMembersList.MainContactTableFromGI(descr);

				if (contact == null) return null;

                foreach (var field in Items.Cache.BqlFields)
				{
					gIResults.Add(new Data.Maintenance.GI.GIResult
					{ ObjectName = contact.Alias, Field = field.Name, });
				}
				descr.Results = gIResults;
			    string[] sorts = null;
                if (PXView.SortColumns != null)
                {
                    sorts = new string[PXView.SortColumns.Count()];
                    for (int i = 0; i < PXView.SortColumns.Count(); i++)                    
                        sorts[i] = contact.Alias + '_' + PXView.SortColumns[i];                    
                }
			    PXFilterRow[] filters = null;
			    if (PXView.Filters != null)
			    {
			        filters = new PXFilterRow[PXView.Filters.Count()];
			        for (int i = 0; i < PXView.Filters.Count(); i++)
			        {
			            filters[i] = PXView.Filters[i];
                        filters[i].DataField = contact.Alias + '_' + filters[i].DataField;
                    }
			    }
				descr.GroupBys = new List<GIGroupBy>(descr.GroupBys)
				{
					new GIGroupBy()
					{
						DataFieldName = contact.Alias + "." + nameof(Contact.contactID)
					}
				};


				descr.Wheres = CRMarketingListMembersList.CreateGIFilter(this._Graph, contact.Alias, filterCurrent.SharedGIFilter, descr.Wheres);
				var pxFilters = CRMarketingListMembersList.CreateFilter(this._Graph, null, filters).ToArray();

				var graph = PXGenericInqGrph.CreateInstance(descr);                
                var result = graph.Results.View.Select(null, null, PXView.Searches, sorts, PXView.Descendings, pxFilters, ref startRow, PXView.MaximumRows, ref totalRows);
				PXView.StartRow = 0;
				graph.Results.View.RequestRefresh();
				return result
					.Select(_ => (GenericResult)_)
					.Select(_ => _.Values[contact.Alias]);
			}
		}
		#endregion

		#region Event Handlers

		protected virtual void CampaignOperationParam_DataSource_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			this.Items.Cache.Clear();
		}

		protected virtual void CampaignOperationParam_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			var row = e.Row as OperationParam;
			if (row == null) return;

			PXUIFieldAttribute.SetVisible<OperationParam.contactGI>(filterCache, row, row.DataSource == OperationParam.DataSourceList.Inquiry);
			PXUIFieldAttribute.SetVisible<OperationParam.sharedGIFilter>(filterCache, row, row.DataSource == OperationParam.DataSourceList.Inquiry);
			PXUIFieldAttribute.SetVisible<OperationParam.sourceMarketingListID>(filterCache, row, row.DataSource == OperationParam.DataSourceList.MarketingLists);
			_Graph.Actions[linkToContact].SetEnabled(Items.Cache.Current != null);
			_Graph.Actions[linkToBAccount].SetEnabled(Items.Cache.Current != null);
		}

		protected virtual void Contact_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#endregion
	}

	#endregion

	#region CRMMarketingSubscriptions

	/// <exclude/>
	public class CRMMarketingContactSubscriptions<TPrimary, TKey> : CRMMarketingSubscriptions
		where TPrimary : IBqlTable, new()
		where TKey : IBqlField
	{
		public CRMMarketingContactSubscriptions(PXGraph graph)
			: base(graph)
		{
			View = new PXView(_Graph, false,
				new Select2<
						CRMarketingListMember,
					InnerJoin<CRMarketingList,
						On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>>>>(),
			new PXSelectDelegate(subscriptions));
		}

		protected override PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			PXSelectJoin<
					CRMarketingListMember,
				InnerJoin<CRMarketingList,
					On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>,
					And<CRMarketingList.isDynamic, Equal<False>,
					And<CRMarketingList.isActive, Equal<True>>>>,
				InnerJoin<Contact,
					On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
				Where<
					CRMarketingListMember.contactID, Equal<Current<TKey>>>>
				.Select(_Graph)
				.ForEach(_ => result.Add(_));

			return result;
		}

		protected override void PrepareFilter()
		{
			var current = (Contact)_Graph.Caches[typeof(TPrimary)].Current;

			if (current == null)
				return;
			
			PXView.Filters.Add(new PXFilterRow()
			{
				OrOperator = false,
				OpenBrackets = 1,
				DataField = nameof(Contact) + "__" + nameof(Contact.ContactID),
				Condition = PXCondition.EQ,
				Value = current.ContactID,
				CloseBrackets = 1
			});
		}
	}

	public class CRMMarketingBAccountSubscriptions : CRMMarketingSubscriptions
	{
		public CRMMarketingBAccountSubscriptions(PXGraph graph)
			: base(graph)
		{
			View = new PXView(_Graph, false,
				new Select3<
						CRMarketingListMember,
					InnerJoin<CRMarketingList,
						On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>>,
					InnerJoin<Contact,
						On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
					OrderBy<
						Desc<Contact.contactPriority>>>(), 
			new PXSelectDelegate(subscriptions));
		}

		protected override PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			PXSelectJoin<
					CRMarketingListMember,
				InnerJoin<CRMarketingList,
					On<CRMarketingList.marketingListID, Equal<CRMarketingListMember.marketingListID>,
					And<CRMarketingList.isDynamic, Equal<False>,
					And<CRMarketingList.isActive, Equal<True>>>>,
				InnerJoin<Contact,
					On<Contact.contactID, Equal<CRMarketingListMember.contactID>>>>,
				Where<
					Contact.bAccountID, Equal<Current<BAccount.bAccountID>>>>
				.Select(_Graph)
				.ForEach(_ => result.Add(_));

			return result;
		}
		
		protected override void PrepareFilter()
		{
			var current = (BAccount)_Graph.Caches[typeof(BAccount)].Current;

			if (current == null)
				return;

			PXView.Filters.Add(new PXFilterRow()
			{
				OrOperator = false,
				OpenBrackets = 1,
				DataField = nameof(Contact) + "__" + nameof(Contact.BAccountID),
				Condition = PXCondition.EQ,
				Value = current.BAccountID,
				CloseBrackets = 1
			});
		}
	}

	public abstract class CRMMarketingSubscriptions : PXSelectBase<CRMarketingListMember>
	{
		public CRMMarketingSubscriptions(PXGraph graph)
		{
			_Graph = graph;
		}

		protected virtual IEnumerable subscriptions()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> subscriptions = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();
            GetFromStatic().ForEach(_ => subscriptions.Add(_));
            bool oldDirty = _Graph.Caches[typeof(CRMarketingListMember)].IsDirty;
            GetFromDynamic().ForEach(_ => subscriptions.Add(_));
            _Graph.Caches[typeof(CRMarketingListMember)].IsDirty = oldDirty;
            return subscriptions;
		}

		protected abstract PXResultset<CRMarketingListMember, CRMarketingList> GetFromStatic();

		private PXResultset<CRMarketingListMember, CRMarketingList> GetFromDynamic()
		{
			PXResultset<CRMarketingListMember, CRMarketingList, Contact> result = new PXResultset<CRMarketingListMember, CRMarketingList, Contact>();

			List<CRMarketingList> lists = new List<CRMarketingList>();

			PXSelect<
					CRMarketingList,
				Where<
					CRMarketingList.isDynamic, Equal<True>,
					And<CRMarketingList.isActive, Equal<True>>>>
				.Select(_Graph)
				.ForEach(_ => lists.Add(_));

			foreach (CRMarketingList list in lists)
			{
				var graph = PXGraph.CreateInstance<CRMarketingListMaint>();
				graph.MailLists.Current = list;

				PrepareFilter();

				int startRow = PXView.StartRow;
				int totalRows = 0;
				foreach (PXResult<CRMarketingListMember, Contact> res in graph.MailRecipients.View.Select(PXView.Currents, new object[] { }, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
				{
                    CRMarketingListMember _CRMarketingListMember = res[typeof(CRMarketingListMember)] as CRMarketingListMember;
                    result.Add(new PXResult<CRMarketingListMember, CRMarketingList, Contact>((CRMarketingListMember)res, list, (Contact)res));
                    if (_Graph.Caches[typeof(CRMarketingListMember)].Locate(_CRMarketingListMember) == null)
                    {
                        _Graph.Caches[typeof(CRMarketingListMember)].SetStatus(_CRMarketingListMember, PXEntryStatus.Held);                        
                    }
                }
			}
			
			return result;
		}

		protected abstract void PrepareFilter();
	}

	#endregion

	[Serializable]
	[PXHidden]
	public partial class CRRelation2 : CRRelation
	{
		public new abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }
		public new abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }
		public new abstract class role : PX.Data.BQL.BqlString.Field<role> { }
	}

	[Serializable]
	[PXHidden]
	public partial class CRMarketingListMember2 : CRMarketingListMember
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }
	}

	[Serializable]
	[PXHidden]
	public partial class CRCampaignMembers2 : CRCampaignMembers
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }
	}

	[Serializable]
	[PXHidden]
	public partial class ContactNotification2 : ContactNotification
	{
		public new abstract class contactID : PX.Data.BQL.BqlInt.Field<contactID> { }
		public new abstract class setupID : PX.Data.BQL.BqlGuid.Field<setupID> { }
	}

	public class PXVirtualTableView<TTable> : PXView
		where TTable : IBqlTable
	{
		public PXVirtualTableView(PXGraph graph)
			: base(graph, false, new Select<TTable>())
		{
			_Delegate = (PXSelectDelegate)Get;
			_Graph.Defaults[_Graph.Caches[typeof(TTable)].GetItemType()] = getFilter;
			_Graph.RowPersisting.AddHandler(typeof(TTable), persisting);
		}
		public IEnumerable Get()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];
			cache.AllowInsert = true;
			cache.AllowUpdate = true;
			object curr = cache.Current;
			if (curr != null && cache.Locate(curr) == null)
			{
				try
				{
					curr = cache.Insert(curr);
				}
				catch
				{
					cache.SetStatus(curr, PXEntryStatus.Inserted);
				}
			}
			yield return curr;
			cache.IsDirty = false;
		}

		private TTable current;
		private bool _inserting = false;
		private object getFilter()
		{
			PXCache cache = _Graph.Caches[typeof(TTable)];

			if (!_inserting)
			{
				try
				{
					_inserting = true;
					if (current == null)
					{
						current = (TTable)(cache.Insert() ?? cache.Locate(cache.CreateInstance()));
						cache.IsDirty = false;
					}
					else if (cache.Locate(current) == null)
					{
						try
						{
							current = (TTable)cache.Insert(current);
						}
						catch
						{
							cache.SetStatus(current, PXEntryStatus.Inserted);
						}
						cache.IsDirty = false;
					}
				}
				finally
				{
					_inserting = false;
				}
			}
			return current;
		}
		private static void persisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		public bool VerifyRequired()
		{
			return VerifyRequired(false);
		}
		public virtual bool VerifyRequired(bool suppressError)
		{
			Cache.RaiseRowSelected(Cache.Current);
			bool result = true;
			PXRowPersistingEventArgs e = new PXRowPersistingEventArgs(PXDBOperation.Insert, Cache.Current);
			foreach (string field in Cache.Fields)
			{
				foreach (PXDefaultAttribute defAttr in Cache.GetAttributes(Cache.Current, field).OfType<PXDefaultAttribute>())
				{
					defAttr.RowPersisting(Cache, e);
					bool error = !string.IsNullOrEmpty(PXUIFieldAttribute.GetError(Cache, Cache.Current, field));
					if (error) result = false;

					if (suppressError && error)
					{
						Cache.RaiseExceptionHandling(field, Cache.Current, null, null);
						return false;
					}
				}
			}
			return result;
		}

	}

	public class WhereEqualNotNull<TField, TFieldCurrent> : IBqlWhere
			where TField : IBqlOperand
			where TFieldCurrent : IBqlField
	{
		private IBqlCreator whereEqual = new Where<TField, Equal<Current2<TFieldCurrent>>>();
		private IBqlCreator whereNull = new Where<Current2<TFieldCurrent>, IsNull>();

		private Type cacheType;

		public WhereEqualNotNull()
		{
			cacheType = typeof(TFieldCurrent).DeclaringType;
		}

		private IBqlCreator GetWhereClause(PXCache cache)
		{
			return cache?.GetValue<TFieldCurrent>(cache.Current) == null ? whereNull : whereEqual;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			var clause = GetWhereClause(graph?.Caches[cacheType]);
			return clause.AppendExpression(ref exp, graph, info, selection);
		}

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			var clause = GetWhereClause(cache?.Graph.Caches[cacheType]);
			clause.Verify(cache, item, pars, ref result, ref value);
		}
	}
}


