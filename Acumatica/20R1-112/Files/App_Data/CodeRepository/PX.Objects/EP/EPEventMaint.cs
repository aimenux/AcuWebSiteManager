using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Export.Imc;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;
using PX.TM;
using PX.Objects.PM;
using ResHandler = PX.TM.PXResourceScheduleAttribute.HandlerAttribute;
using PX.Objects.Common.GraphExtensions.Abstract;

namespace PX.Objects.EP
{
	#region NewAttendeeSelectorAttribute
	public sealed class NewAttendeeSelectorAttribute : PXCustomSelectorAttribute
	{
		private class Definition : IPrefetchable<object>
		{
			private List<object> _items;

			public IEnumerable<object> Items
			{
				get
				{
					return _items;
				}
			}

			public void Prefetch(object parameter)
			{
				_items = new List<object>();
				var res = PXSelect<EPAttendee, 
					Where<EPAttendee.eventNoteID, Equal<Required<EPAttendee.eventNoteID>>>>.
					Select(new PXGraph(), parameter);
				if (res != null)
					foreach (EPAttendee row in res)
					{
						_items.Add(row.UserID);
					}
			}
		}

		private BqlCommand _selectCommand;
		private PXView _selectAll;

		private readonly Type _eventKeyType;
		private readonly Type _eventOwnerType;
		private readonly Type _eventDACType;

		private object _reculc;
		private object[] _cachedAttendees;
		private Definition _definition;

		public NewAttendeeSelectorAttribute(Type eventKeyType, Type eventOwnerType)
			: this(eventKeyType, eventOwnerType, new Type[0]) { }

		public NewAttendeeSelectorAttribute(Type eventKeyType, Type eventOwnerType, params Type[] fieldList)
			: base(typeof(Users.pKID), fieldList)
		{
			_eventKeyType = eventKeyType;
			_eventOwnerType = eventOwnerType;
			_eventDACType = eventOwnerType.DeclaringType;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			_selectCommand = BqlCommand.CreateInstance(typeof(Select<,>), typeof(Users), 
				typeof(Where<,,>), typeof(Users.pKID), typeof(NotEqual<>), typeof(Current<>), _eventOwnerType,
					typeof(Or<,>), typeof(Current<>), _eventOwnerType, typeof(IsNull));

			_selectAll = new PXView(_Graph, true, new Select<Users>());

			sender.Graph.RowInserted.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
			sender.Graph.RowUpdated.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
			sender.Graph.RowDeleted.AddHandler(sender.GetItemType(), (cache, args) => _reculc = null);
		}

	    public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
	    {
            Guid g;
	        if (e.NewValue is string && Guid.TryParse((string) e.NewValue, out g))
	        {
	            var oldValue = e.NewValue;
                e.NewValue = (object)g;
                base.FieldVerifying(sender, e);
	            e.NewValue = oldValue;
	        }
	        else
	        {
                base.FieldVerifying(sender, e);
	        }
	    }

	    public IEnumerable GetRecords()
		{
			if (PXView.Searches != null && PXView.Searches.Length > 0)
			{
				var searchesAreSpecified = false;
				if (!searchesAreSpecified)
				{
					foreach (object val in PXView.Searches)
					{
						if (val != null)
						{
							searchesAreSpecified = true;
							break;
						}
					}
				}

                searchesAreSpecified = searchesAreSpecified || (_Graph.IsMobile && PXView.Searches.Length == 2); // HACK Very dirty trick to fix mobile

				if (searchesAreSpecified)
				{
					var start = PXView.StartRow;
					var total = 0;
					foreach (Users item in _selectAll.
						Select(null, null, PXView.Searches, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref start, PXView.MaximumRows, ref total))
					{
						yield return item;
					}
					PXView.StartRow = 0;
					yield break;
				}
			}

			var cache = _Graph.Caches[_eventDACType];
			var currentEventKey = cache.GetValue(cache.Current, _eventKeyType.Name);
			var currentEventOwner = cache.GetValue(cache.Current, _eventOwnerType.Name);

			var selectedAttendees = GetSelectedAttendees(currentEventKey, currentEventOwner);
			
			var command =
				selectedAttendees.Length > 0
					? _selectCommand.WhereAnd(NotInHelper<Users.pKID>.Create(selectedAttendees.Length))
					: _selectCommand;

		    Guid a;
			if (PXView.Filters.Cast<PXFilterRow>().Any(filter => (filter.DataField == typeof(Users.pKID).Name) && !GUID.TryParse(filter.Value as string, out a)))
				yield break;

			var startRow = PXView.StartRow;
			var totalRows = 0;
			foreach (Users row in new PXView(_Graph, true, command).
				Select(null, selectedAttendees, null, PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows))
			{
				yield return row;
			}
			PXView.StartRow = 0;
		}

		private object[] GetSelectedAttendees(object currentEventKey, object currentEventOwner)
		{
			Definition dbDef;
			if ((_reculc == null || !object.Equals(_reculc, currentEventKey)) | 
				(dbDef = PXDatabase.GetSlot<Definition, object>(this.GetType().Name + (Guid)currentEventKey, currentEventKey, typeof(Users))) != _definition)
			{
				var res = new List<object>(dbDef.Items);
				var attendeeCache = _Graph.Caches[typeof(EPAttendee)];
				foreach (EPAttendee row in attendeeCache.Inserted)
					if (row.UserID != null) res.Add(row.UserID);
				foreach (EPAttendee row in attendeeCache.Updated)
					if (row.UserID != null) res.Add(row.UserID);
				if (currentEventOwner != null)
				{
					res.Insert(0, currentEventOwner);
					res.Insert(0, currentEventOwner);
				}
				_cachedAttendees = res.ToArray();
				_reculc = currentEventKey;
				_definition = dbDef;
			}
			return _cachedAttendees;
		}
	}
	#endregion

	#region StringGuidAttribute

	public sealed class StringGuidAttribute : PXEventSubscriberAttribute, PX.Data.IPXFieldSelectingSubscriber
	{
		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			Guid val;
			if (e.ReturnValue != null && e.ReturnValue is string && 
				GUID.TryParse(e.ReturnValue as string, out val))
			{
				e.ReturnValue = val;
			}
		}
	}

	#endregion

	#region AddAttendeeFilter

	[Serializable]
	[PXHidden]
	[DebuggerDisplay("Index = {Index}, PKID = {PKID}")]
	public partial class AddAttendeeFilter : IBqlTable
	{
		public abstract class index : PX.Data.BQL.BqlInt.Field<index> { }
		
		[PXDBIdentity(IsKey = true)]
		[PXUIField(Visible = false)]
		public int? Index { get; set; }

		public abstract class pkID : PX.Data.BQL.BqlGuid.Field<pkID> { }
		
		[PXDBGuid]
		[PXUIField(DisplayName = "User Name")]
		[NewAttendeeSelector(typeof(CRActivity.noteID), typeof(CRActivity.ownerID), 
			typeof(Users.displayName), 
			typeof(Users.email))]
		public Guid? PKID { get; set; }
	}

	#endregion

	#region DeleteOtherAttendeeFilter

	[Serializable]
	[PXHidden]
	public partial class DeleteOtherAttendeeFilter : IBqlTable
	{
		#region AttendeeID

		public abstract class attendeeID : PX.Data.BQL.BqlInt.Field<attendeeID> { }

		protected int? _AttendeeID;

		[PXDBInt]
		[PXUIField(Visible = false)]
		public virtual int? AttendeeID
		{
			get { return _AttendeeID; }
			set { _AttendeeID = value; }
		}

		#endregion

		#region WithNotification

		public abstract class withNotification : PX.Data.BQL.BqlBool.Field<withNotification> { }

		protected Boolean? _WithNotification;

		[PXDBBool]
		[PXUIField(DisplayName = "With Notification")]
		public virtual Boolean? WithNotification
		{
			get { return _WithNotification; }
			set { _WithNotification = value; }
		}

		#endregion
	}

	#endregion

	#region EPOtherAttendeeWithNotification

	[Serializable]
	public partial class EPOtherAttendeeWithNotification : EPOtherAttendee
	{
		public new abstract class eventNoteID : PX.Data.BQL.BqlGuid.Field<eventNoteID> { }
		public new abstract class attendeeID : PX.Data.BQL.BqlInt.Field<attendeeID> { }

		#region NotifyOnRowUpdated
		public abstract class notifyOnRowUpdated : PX.Data.BQL.BqlBool.Field<notifyOnRowUpdated> { }
		
		[PXBool]
		[PXDefault(false)]
		[PXUIField(Visible = false)]
		public virtual bool? NotifyOnRowUpdated { get; set; }
		#endregion
	}

	#endregion

	#region AllTypeAttendee

	[Serializable]
	[PXHidden]
	public partial class AllTypeAttendee : IBqlTable
	{
		#region Key
		public abstract class key : PX.Data.BQL.BqlString.Field<key> { }
		
		[PXString(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual string Key { get; set; }
		#endregion

		#region Type
		public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }
		
		[PXInt(IsKey = true)]
		[PXUIField(Visible = false)]
		public virtual int? Type { get; set; }

		#endregion

		#region EventNoteID
		public abstract class eventNoteID : PX.Data.BQL.BqlGuid.Field<eventNoteID> { }
		
		[PXGuid(IsKey = true)]
		[PXDBDefault(typeof(CRActivity.noteID))]
		public virtual Guid? EventNoteID { get; set; }
		#endregion

		#region Email
		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		
		[PXString]
		[PXUIField(DisplayName = "Email")]
		public virtual string Email { get; set; }
		#endregion

		#region Name
		public abstract class name : PX.Data.BQL.BqlString.Field<name> { }
		
		[PXString(50, IsUnicode = true)]
		[PXUIField(DisplayName = "Name")]
		[NewAttendeeSelector(typeof(CRActivity.noteID), typeof(CRActivity.ownerID), 
			typeof(Users.displayName), 
			typeof(Users.email), 
			typeof(Users.fullName), 
			DescriptionField = typeof(Users.displayName), SelectorMode = PXSelectorMode.NoAutocomplete)]
		[StringGuid]
		public virtual string Name { get; set; }

		#endregion

		#region DisplayName
		public abstract class displayName : PX.Data.BQL.BqlString.Field<displayName> { }
		
		[PXString(50, IsUnicode = true)]
        [PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false, DisplayName = "Attendee Name")]
        [PXSelector(typeof(Users.fullName))]
		public virtual string DisplayName { get; set; }
		#endregion

		#region Comment
		public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
		
		[PXString(255)]
		[PXUIField(DisplayName = "Comment")]
		public virtual string Comment { get; set; }
		#endregion

		#region Invitation
		public abstract class invitation : PX.Data.BQL.BqlInt.Field<invitation> { }
		
		[PXInt]
		[PXUIField(DisplayName = "Invitation", Enabled = false)]
		[PXDefault(PXInvitationStatusAttribute.NOTINVITED)]
		[PXInvitationStatus]
		public virtual int? Invitation { get; set; }

		#endregion
	}

	#endregion

	#region EPAttendeeForAcceptReject

	[Serializable]
    [PXHidden]
	public partial class EPAttendeeForAcceptReject : EPAttendee
	{
		public new abstract class eventNoteID : PX.Data.BQL.BqlGuid.Field<eventNoteID> { }
		public new abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }
	}

	#endregion

	#region SendCardFilter

	[Serializable]
	[PXHidden]
	public partial class SendCardFilter : IBqlTable
	{
		#region Email

		public abstract class email : PX.Data.BQL.BqlString.Field<email> { }
		
		[PXUIField(DisplayName = "Email")]
		[PXDefault]
		[PXDBEmail]
		public virtual string Email { get; set; }

		#endregion
	}

	#endregion

	public class EPEventMaint : CRBaseActivityMaint<EPEventMaint, CRActivity>
	{
		#region Extensions

		public class EmbeddedImagesExtractor : EmbeddedImagesExtractorExtension<EPEventMaint, CRActivity, CRActivity.body>
		{
		}

		#endregion

		#region PXSelectEvents
		public sealed class PXSelectEvents<owner> : PXSelectJoin<CRActivity,
			InnerJoin<Users, 
				On<Users.pKID, Equal<CRActivity.ownerID>>>,
			Where<CRActivity.ownerID, Equal<owner>,
				And<CRActivity.noteID, NotEqual<Current<CRActivity.noteID>>,
				And<Where<CRActivity.startDate, GreaterEqual<Optional<CRActivity.startDate>>,
				And<CRActivity.startDate, LessEqual<Optional<CRActivity.startDate>>,
				Or<Where<CRActivity.startDate, Less<Optional<CRActivity.startDate>>,
					And<CRActivity.endDate, Greater<Optional<CRActivity.endDate>>>>>>>>>>,
			OrderBy<
				Asc<Users.username, 
				Asc<CRActivity.startDate, 
				Asc<CRActivity.endDate>>>>>
			where owner : IBqlOperand
		{
			public PXSelectEvents(PXGraph graph) : base(graph) { }
			public PXSelectEvents(PXGraph graph, Delegate handler) : base(graph, handler) { }

			public static PXResultset<CRActivity> ExecuteSelect(PXGraph graph, DateTime? start, DateTime? end, params object[] parameters)
			{
				List<object> pList = new List<object>(parameters);
				pList.Add(start);
				pList.Add(end);
				pList.Add(start);
				pList.Add(start);
				return Select(graph, pList.ToArray());
			}
		}
		#endregion

		#region ConfirmNotificationResult

		private enum ConfirmNotificationResult
		{
			No,
			Yes,
			YesForAll
		}

		#endregion

		#region ConfirmNotificationHandler

		private delegate ConfirmNotificationResult ConfirmNotificationHandler(PXAdapter adapter);

		#endregion

		#region NotificationHandler

		private delegate void NotificationHandler(ConfirmNotificationResult target);

		#endregion

		#region NotificationOnAction

		private class NotificationOnAction
		{
			public delegate IEnumerable ActionHandler(PXAdapter adapter);

			private readonly ConfirmNotificationHandler _confirmHandler;
			private readonly NotificationHandler _notificationHandler;

			public NotificationOnAction(ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
			{
				if (confirmHandler == null) throw new ArgumentNullException("confirmHandler");
				if (notificationHandler == null) throw new ArgumentNullException("notificationHandler");

				_confirmHandler = confirmHandler;
				_notificationHandler = notificationHandler;
			}

			public virtual IEnumerable HandleAction(ActionHandler action, PXAdapter adapter)
			{
				if (action == null) throw new ArgumentNullException("action");

				ConfirmNotificationResult confirm = _confirmHandler(adapter);
				IEnumerable result = action(adapter);
				if (confirm != ConfirmNotificationResult.No) _notificationHandler(confirm);
				foreach (object item in result)
					yield return item;
				adapter.View.ClearDialog();
				yield break;
			}
		}

		#endregion

		#region SaveActionWithNotification

		private class SaveActionWithNotification<TNode> : PXAction<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public SaveActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.Save,
				MapEnableRights = PXCacheRights.Update,
				MapViewRights = PXCacheRights.Update)]
			[PXSaveButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(
					a =>
					{
						var res = a.Get();
						var graph = a.View.Graph;
						graph.Persist();
						graph.SelectTimeStamp();
						return res;
					}, 
					adapter);
			}
		}

		#endregion

		#region SaveCloseActionWithNotification

		private class SaveCloseActionWithNotification<TNode> : PXAction<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public SaveCloseActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.SaveClose,
				MapEnableRights = PXCacheRights.Update,
				MapViewRights = PXCacheRights.Update)]
			[PXSaveCloseButton]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(
					a =>
					{
						var res = a.Get();
						var graph = a.View.Graph;
						graph.Persist();
						graph.SelectTimeStamp();
						return res;
					},
					adapter);
			}
		}

		#endregion

		#region DeleteActionWithNotification

		private class DeleteActionWithNotification<TNode> : PXDelete<TNode>
			where TNode : class, IBqlTable, new()
		{
			private readonly NotificationOnAction _notificationHandler;

			public DeleteActionWithNotification(PXGraph graph, string name,
				ConfirmNotificationHandler confirmHandler, NotificationHandler notificationHandler)
				: base(graph, name)
			{
				_notificationHandler = new NotificationOnAction(confirmHandler, notificationHandler);
			}

			[PXUIField(DisplayName = ActionsMessages.Delete,
				MapEnableRights = PXCacheRights.Delete,
				MapViewRights = PXCacheRights.Delete)]
			[PXDeleteButton(ConfirmationMessage = ActionsMessages.ConfirmDeleteExplicit)]
			protected override IEnumerable Handler(PXAdapter adapter)
			{
				return _notificationHandler.HandleAction(base.Handler, adapter);
			}
		}

		#endregion

		#region NotificationTypes

		private enum NotificationTypes
		{
			Invitation,
			Reschedule,
			Cancel
		}

		#endregion

		#region Constants
		private const string _ATTENDEE_VIEW_NAME = "AttendeeResources";

		private const int _OTHER_ATTENDEE_TYPE = 0;
		private const int _USER_ATTENDEE_TYPE = 1;

		private const string _SAVE_ACTION_NAME = "Save";
		private const string _SAVECLOSE_ACTION_NAME = "SaveClose";
		private const string _DELETE_ACTION_NAME = "Delete";

		private const string _ASK_KEY_1 = "key1";
		private const string _ASK_KEY_2 = "key2";
		private const string _ASK_KEY_3 = "key3";
		private const string _ASK_KEY_5 = "key5";
		private const string _ASK_KEY_6 = "key6";
		private const string _ASK_KEY_7 = "key7";

		#endregion

		#region Fields

		private static readonly string _WIKI_NEW_LINE;

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
				BaseContract;

		[PXHidden]
		public PXSelect<PMTimeActivity>
			TimeActivitiesOld;

		public PMTimeActivityList<CRActivity>
			TimeActivity;
		
		[PXViewName(Messages.Events)]
		[PXRefNoteSelector(typeof(CRActivity), typeof(CRActivity.refNoteID))]
		[PXCopyPasteHiddenFields(typeof(CRActivity.body))]
		public PXSelect<CRActivity, 
				Where<CRActivity.classID, Equal<CRActivityClass.events>>> 
			Events;

		[PXHidden]
		public PXSelect<CRChildActivity>
				ChildAct;

		[PXFilterable]
		[CRReference(typeof(CRActivity.bAccountID),typeof(CRActivity.contactID))]
		[PXViewDetailsButton(typeof(CRActivity), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public CRChildActivityList<CRActivity>
			ChildActivities;

		public PXSetup<EPSetup> 
			Setup;

		public PXSelect<AddAttendeeFilter> NewAttendeeCurrent;

		public PXSelectJoin<EPAttendee,
			InnerJoin<Users, On<Users.pKID, Equal<EPAttendee.userID>>>,
			Where<EPAttendee.eventNoteID, Equal<Current<CRActivity.noteID>>>> 
			Attendees;

		public PXSelectOrderBy<Users,
			OrderBy<Asc<Users.fullName, Asc<Users.email>>>> 
			NotAttendees;

		[PXResourceSchedule(typeof(CRActivity), typeof(CRActivity), typeof(CRActivity.startDate), typeof(CRActivity.endDate),
			DescriptionBqlField = typeof(Users.fullName),
			ItemDescriptionBqlField = typeof(CRActivity.subject),
			TargetTable = typeof(CRActivity),
			TargetStartBqlField = typeof(CRActivity.startDate),
			TargetEndBqlField = typeof(CRActivity.endDate))]
		public PXSelectJoin<Users,
			InnerJoin<EPAttendee, 
				On<EPAttendee.userID, Equal<Users.pKID>>,
			LeftJoin<CRActivity, 
				On<CRActivity.ownerID, Equal<EPAttendee.userID>>>>,
			Where<EPAttendee.eventNoteID, Equal<Current<CRActivity.noteID>>>> 
			AttendeeResources;

		public PXSelect<Users,
			Where<Users.pKID, Equal<Current<CRActivity.ownerID>>>> 
			CurrentOwner;

		public PXSelect<EPOtherAttendeeWithNotification,
			Where<EPOtherAttendeeWithNotification.eventNoteID, Equal<Current<CRActivity.noteID>>>> OtherAttendees;

		[PXFilterable]
		public PXSelect<AllTypeAttendee> 
			AllAttendees;

		[PXFilterable]
		public PXSelect<AllTypeAttendee> 
			AllAttendeesAndOwner;

		public PXFilter<DeleteOtherAttendeeFilter> 
			ConfirmDeleteOtherAttendees;

		public PXSelect<EPAttendeeMessage,
			Where<EPAttendeeMessage.eventNoteID, Equal<Current<CRActivity.noteID>>>> 
			AttendeeMessages;

		public PXSelect<EPOtherAttendeeMessage,
			Where<EPOtherAttendeeMessage.eventNoteID, Equal<Current<CRActivity.noteID>>>> 
			OtherAttendeeMessages;

		public PXSelect<Users> 
			UsersSearch;

		public PXFilter<SendCardFilter> 
			SendCardSettings;

		public PXSelectJoin<CSCalendar,
			InnerJoin<EPEmployee, 
				On<EPEmployee.calendarID, Equal<CSCalendar.calendarID>>>,
			Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>> 
			WorkCalendar;

		[PXHidden]
		public PXSelect<EPAttendeeForAcceptReject,
			Where<EPAttendeeForAcceptReject.userID, Equal<Current<AccessInfo.userID>>, 
				And<EPAttendeeForAcceptReject.eventNoteID, Equal<Current<CRActivity.noteID>>>>> 
			CurrentAttendee;

		public CRReminderList<CRActivity>
			Reminder;

		#endregion

		#region Ctors

		static EPEventMaint()
		{
			_WIKI_NEW_LINE = Environment.NewLine + Environment.NewLine;
		}

		public EPEventMaint()
			: base()
		{
			CorrectActions();

			PXUIFieldAttribute.SetVisible(Caches[typeof(Users)], typeof(Users.username).Name, true);
			var activitiesCache = Caches[typeof (CRActivity)];
			PXUIFieldAttribute.SetDisplayName<CRActivity.ownerID>(activitiesCache, Messages.CreatedBy);
			PXUIFieldAttribute.SetDisplayName<CRActivity.startDate>(activitiesCache, Messages.StartTime);

			Action.AddMenuAction(ExportCard);
			Action.AddMenuAction(sendCard);
			
			PXView relEntityView = new PXView(this, true, new Select<CRSMEmail>(), new PXSelectDelegate(GetRelatedEntity));
			Views.Add("RelatedEntity", relEntityView);
            ActivityStatusAttribute.SetRestictedMode<CRActivity.uistatus>(Events.Cache, true);
        }

		private void CorrectActions()
		{
			Actions[_SAVE_ACTION_NAME] = new SaveActionWithNotification<CRActivity>(this, _SAVE_ACTION_NAME,
				ConfirmAttendeeInvitations, InviteAttendees);
			Actions[_SAVECLOSE_ACTION_NAME] = new SaveCloseActionWithNotification<CRActivity>(this, _SAVECLOSE_ACTION_NAME,
				ConfirmAttendeeInvitations, InviteAttendees);
			Actions[_DELETE_ACTION_NAME] = new DeleteActionWithNotification<CRActivity>(this, _DELETE_ACTION_NAME,
				ConfirmCancelAttendeeInvitations, CancelAttendeeInvitations);
		}

		#endregion

		#region Data Handlers

		public IEnumerable GetRelatedEntity()
		{
			var current = Events.Current;
			if (current != null && current.RefNoteID != null)
			{
				var row = new EntityHelper(this).GetEntityRow(current.RefNoteID);
				if (row != null) yield return row;
			}
		}

		protected virtual IEnumerable newAttendeeCurrent()
		{
			if (NewAttendeeCurrent.Cache.Current == null)
			{
				NewAttendeeCurrent.Cache.Insert();
				NewAttendeeCurrent.Cache.IsDirty = false;
			}
			yield return NewAttendeeCurrent.Cache.Current;
		}

		protected virtual IEnumerable notAttendees()
		{
			foreach (Users user in PXSelect<Users>.Select(this))
				if (Attendees.Search<EPAttendee.userID>(user.PKID).Count == 0)
					yield return user;
		}

		protected virtual IEnumerable allAttendees()
		{
			foreach (EPOtherAttendeeWithNotification otherAttendee in OtherAttendees.Select())
			{
				var a = new AllTypeAttendee
				{
					Type = _OTHER_ATTENDEE_TYPE,
					Key = ((int)otherAttendee.AttendeeID).ToString(),
					EventNoteID = otherAttendee.EventNoteID
				};
				a = AllAttendees.Locate(a) ?? a;
				a.Invitation = otherAttendee.Invitation;
				a.Name = otherAttendee.Name;
				a.DisplayName = otherAttendee.Name;
				a.Email = otherAttendee.Email;
				a.Comment = otherAttendee.Comment;
				AllAttendees.Cache.Hold(a);
				yield return a;
			}
			foreach (PXResult<EPAttendee, Users> item in Attendees.Select())
			{
				var epAttendee = (EPAttendee)item;
				var user = (Users)item;

				var displayName = user.DisplayName;
				var email = user.Email;

				var employeeContactPair = user.PKID.
					With(_ => PXSelectJoin<EPEmployee,
						LeftJoin<Contact, On<Contact.contactID, Equal<EPEmployee.defContactID>>>,
						Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>>>.
					Select(this, _.Value));
				if (employeeContactPair != null && employeeContactPair.Count > 0)
				{
					var employee = (EPEmployee)employeeContactPair[0][typeof(EPEmployee)];
					var contact = (Contact)employeeContactPair[0][typeof(Contact)];
					if (!string.IsNullOrWhiteSpace(employee.AcctName)) displayName = employee.AcctName;
					if (!string.IsNullOrWhiteSpace(contact.EMail)) email = contact.EMail;
				}
				var a = new AllTypeAttendee
				{
					Type = _USER_ATTENDEE_TYPE,
					Key = ((Guid)epAttendee.UserID).ToString(),
					EventNoteID = epAttendee.EventNoteID
				};
				a = AllAttendees.Locate(a) ?? a;
				a.Invitation = epAttendee.Invitation;
				a.Name = Convert.ToString(user.PKID);
				a.DisplayName = displayName;
				a.Email = email;
				a.Comment = user.Comment;
				AllAttendees.Cache.Hold(a);
				yield return a;
			}
		}

		public virtual IEnumerable allAttendeesAndOwner()
		{
            foreach (var attendee in AllAttendees.Select())
				yield return attendee;

		    var owner = EventOwner;
			if (owner == null) yield break;

			var ownerAsAttendee = new AllTypeAttendee()
			{
				Type = _USER_ATTENDEE_TYPE,
				Key = ((Guid)owner.PKID).ToString(),
				EventNoteID = Events.Current == null ? null : Events.Current.NoteID
			};
			ownerAsAttendee = AllAttendeesAndOwner.Locate(ownerAsAttendee) ?? ownerAsAttendee;
			ownerAsAttendee.Invitation = PXInvitationStatusAttribute.ACCEPTED;
			ownerAsAttendee.Name = Convert.ToString(owner.PKID);
			ownerAsAttendee.DisplayName = owner.DisplayName;
			ownerAsAttendee.Email = owner.Email;
			ownerAsAttendee.Comment = owner.Comment;
			AllAttendeesAndOwner.Cache.Hold(ownerAsAttendee);
			yield return new PXResult<AllTypeAttendee>(ownerAsAttendee);

            AllAttendeesAndOwner.AllowInsert =
                AllAttendeesAndOwner.AllowUpdate =
                AllAttendeesAndOwner.AllowDelete = Events.Current != null && Events.Current.OwnerID == Accessinfo.UserID;
        }

        [ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetAdditionalRecords)]
		public virtual IEnumerable OwnerEvents(DateTime? start, DateTime? end)
		{
			Users currentOwner = EventOwner;
			if (currentOwner == null) yield break;

			if (GetAttendeesByEventOwner((CRActivity)Events.Current).GetEnumerator().MoveNext())
				yield break;

			PXResultset<CRActivity> eventsDBResult = PXSelectEvents<Current<CRActivity.ownerID>>.ExecuteSelect(this, start, end);
			foreach (PXResult<CRActivity> row in eventsDBResult)
			{
				CRActivity _event = (CRActivity)row;
				EPAttendee attendee = new EPAttendee();
				attendee.EventNoteID = _event.NoteID;
				attendee.UserID = currentOwner.PKID;
				yield return new PXResult<Users, EPAttendee, CRActivity>(currentOwner, attendee, _event);
			}
			if (eventsDBResult.Count == 0)
				yield return new PXResult<Users, EPAttendee, CRActivity>(currentOwner, new EPAttendee(), new CRActivity());
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetAdditionalRecords)]
		public virtual IEnumerable InsertedAttendeeEvents(DateTime? start, DateTime? end)
		{
			foreach (EPAttendee attendee in this.Caches[typeof(EPAttendee)].Inserted)
				if (attendee.UserID.HasValue)
				{
					PXResultset<CRActivity> eventsDBResult =
						PXSelectEvents<Required<CRActivity.ownerID>>.ExecuteSelect(this, start, end, attendee.UserID);
					foreach (PXResult<CRActivity, Users> row in eventsDBResult)
					{
						CRActivity _event = (CRActivity)row;
						Users user = (Users)row;
						yield return new PXResult<Users, EPAttendee, CRActivity>(user, attendee, _event);
					}
					if (eventsDBResult.Count == 0)
					{
						PXResultset<Users> usersDBResult =
							PXSelect<Users, Where<Users.pKID, Equal<Required<Users.pKID>>>>.Select(this, attendee.UserID);
						if (usersDBResult.Count > 0)
							yield return new PXResult<Users, EPAttendee, CRActivity>((Users)usersDBResult, attendee, new CRActivity());
					}
				}
		}

		#endregion

		#region Actions

		public PXDelete<CRActivity> Delete;

		public PXAction<CRActivity> Complete;
		[PXUIField(DisplayName = PX.TM.Messages.CompleteEvent, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.Objects.EP.Messages.CompleteEventTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		protected virtual void complete()
		{
			var row = Events.Current;
			if (row == null) return;

			CompleteEvent(Events.Current);
		}

		public PXAction<CRActivity> CompleteAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteAndFollowUpEvent, MapEnableRights = PXCacheRights.Select, Visible = false)]
		[PXButton(Tooltip = Messages.CompleteAndFollowUpEventTooltip, ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		protected virtual void completeAndFollowUp()
		{
			CRActivity row = Events.Current;
			if (row == null) return;

			CompleteEvent(row);

			EPEventMaint graph = CreateInstance<EPEventMaint>();

			CRActivity followUpActivity = (CRActivity)graph.Events.Cache.CreateCopy(row);
			followUpActivity.NoteID = null;
			followUpActivity.ParentNoteID = row.ParentNoteID;
			followUpActivity.UIStatus = null;
			followUpActivity.NoteID = null;
			followUpActivity.PercentCompletion = null;

			if (followUpActivity.StartDate != null)
			{
				followUpActivity.StartDate = ((DateTime) followUpActivity.StartDate).AddDays(1D);
			}
			if (followUpActivity.EndDate != null)
				followUpActivity.EndDate = ((DateTime)followUpActivity.EndDate).AddDays(1D);
			
			graph.Events.Cache.Insert(followUpActivity);

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXAction<CRActivity> CancelActivity;

		[PXUIField(DisplayName = PX.TM.Messages.CancelEvent, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.TM.Messages.CancelTask)]
		protected virtual IEnumerable cancelActivity(PXAdapter adapter)
		{
			var notificator = new NotificationOnAction(ConfirmCancelAttendeeInvitations, CancelAttendeeInvitations);
			return notificator.HandleAction(ad =>
			{
				if (Events.Current != null)
					CancelEvent(Events.Current);
				return ad.Get();
			}, adapter);
		}

		public PXAction<CRActivity> acceptInvitation;
		[PXButton(Tooltip = Messages.AcceptInvitationTooltip)]
		[PXUIField(DisplayName = Messages.AcceptInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable AcceptInvitation(PXAdapter adapter)
		{
			AcceptParticipation(true);
			return adapter.Get();
		}

		public PXAction<CRActivity> rejectInvitation;
		[PXButton(Tooltip = Messages.RejectInvitationTooltip)]
		[PXUIField(DisplayName = Messages.RejectInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable RejectInvitation(PXAdapter adapter)
		{
			AcceptParticipation(false);
			return adapter.Get();
		}

		public PXAction<CRActivity> ExportCard;
		[PXUIField(DisplayName = PX.TM.Messages.ExportCard, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = PX.TM.Messages.ExportCardTooltip)]
		public virtual void exportCard()
		{
			var row = Events.Current;
			if (row != null)
			{
				var vCard = VCalendarProcessor.CreateVEvent(row);
				throw new EPIcsExportRedirectException(vCard);
			}
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.AddRecord)]
		public virtual void AddAttendee()
		{
			Users currentOwner = EventOwner;
			AddAttendeeFilter newAttendeeParams = NewAttendeeCurrent.Current;
			if (newAttendeeParams != null &&
				(currentOwner == null || newAttendeeParams.PKID != currentOwner.PKID) &&
				Attendees.Search<EPAttendee.userID>(newAttendeeParams.PKID).Count == 0)
			{
				EPAttendee newAttendee = new EPAttendee();
				newAttendee.EventNoteID = Events.Current.NoteID;
				newAttendee.UserID = newAttendeeParams.PKID;
				Attendees.Insert(newAttendee);

				newAttendeeParams.PKID = null;
				NewAttendeeCurrent.Cache.IsDirty = false;
			}
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.DeleteRecord)]
		public virtual void RemoveAttendee(object item)
		{
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.ViewDetails)]
		public virtual void ViewAttendeeEventDetails(object item)
		{
			CRActivity activity = item as CRActivity;
			if (activity != null)
			{
				var noteID = activity.NoteID;
				CRActivity row;
				if (noteID.HasValue && (row = Events.Search<CRActivity.noteID>(noteID)) != null)
					PXRedirectHelper.TryOpenPopup(Events.Cache, row, null);
			}
		}

		public PXDBAction<CRActivity> sendInvitations;
		[PXButton(Tooltip = Messages.SendInvitationsTooltip)]
		[PXUIField(DisplayName = Messages.SendInvitations, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendInvitations(PXAdapter adapter)
		{
			List<AllTypeAttendee> attendees = AllAttendees.Select().RowCast<AllTypeAttendee>().ToList();
			List<AllTypeAttendee> notInvitedAttendees = attendees.Where(a => a.Invitation == PXInvitationStatusAttribute.NOTINVITED ).ToList();

			List<AllTypeAttendee> forSend = null;
			WebDialogResult res;
			if (!notInvitedAttendees.Any()) // all invited
			{
				if (!IsImport)
			    {
			        res = AllAttendees.Ask(Messages.SendInvitations, Messages.NotifyAllInvitedAttendees, MessageButtons.YesNo);
			        switch (res)
			        {
			            case WebDialogResult.Yes:
			                forSend = attendees;
			                break;
			        }
			    }
			    else
			    {
			        forSend = attendees;
			    }
			}
			else if (notInvitedAttendees.Count != attendees.Count) // exist not invited
			{
				if (!IsImport)
			    {
			        res = AllAttendees.Ask(Messages.SendInvitations, Messages.NotifyNotInvitedAttendees,
			            MessageButtons.YesNoCancel);
			        switch (res)
			        {
			            case WebDialogResult.Yes:
			                forSend = notInvitedAttendees;
			                break;
			            case WebDialogResult.No:
			                forSend = attendees;
			                break;
			        }
			    }
			    else
			    {
			        forSend = notInvitedAttendees;
			    }
			}
			else // all not invited
			{
				forSend = notInvitedAttendees;
			}

			if (forSend != null && forSend.Any())
			{
				AssertEventStatus();
				EPEventMaint graph = this.Clone();
				PXLongOperation.StartOperation(this, () => graph.SendEMails(NotificationTypes.Invitation, forSend));
			}

			adapter.View.ClearDialog();
			AllAttendeesAndOwner.View.RequestRefresh();
			return adapter.Get();
		}

		public PXDBAction<CRActivity> sendPersonalInvitation;
		[PXButton(Tooltip = Messages.SendPersonalInvitationTooltip)]
		[PXUIField(DisplayName = Messages.SendPersonalInvitation, Visible = false, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendPersonalInvitation(PXAdapter adapter)
		{
			AssertEventStatus();

			Users eventOwner;
			if (AllAttendeesAndOwner.Current != null && (eventOwner = EventOwner) != null && eventOwner.PKID != null && 
				string.Compare(AllAttendeesAndOwner.Current.Key, ((Guid)eventOwner.PKID).ToString(), StringComparison.OrdinalIgnoreCase) != 0)
			{
				WebDialogResult confirm = WebDialogResult.Yes;
			    if (AllAttendeesAndOwner.Current.Invitation != PXInvitationStatusAttribute.NOTINVITED)
			    {
					if (!IsImport)
			        {
			            confirm = AllAttendees.Ask(
			                _ASK_KEY_1,
			                Messages.SendPersonalInvitation,
			                Messages.ResendPersonalInvitation,
			                MessageButtons.YesNo);
			        }
			        else
			        {
			            confirm = WebDialogResult.Yes;			            
			        }
			    }
			    if (confirm == WebDialogResult.Yes)
				{
					EPEventMaint graph = this.Clone();
					PXLongOperation.StartOperation(this, ()=> graph.SendEMail(NotificationTypes.Invitation, graph.AllAttendeesAndOwner.Current));
				}
			}
			adapter.View.ClearDialog();
			AllAttendeesAndOwner.View.RequestRefresh();
			return adapter.Get();
		}

		public PXAction<CRActivity> sendCard;
		[PXButton(Tooltip = Messages.SendCardTooltip)]
		[PXUIField(DisplayName = Messages.SendCard, MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable SendCard(PXAdapter adapter)
		{
			if (Events.Current == null) return adapter.Get();

			if (SendCardSettings.AskExtRequired())
			{
				CRActivity _event = Events.Current;
				string newLine = Environment.NewLine + Environment.NewLine;
				string mailTo = SendCardSettings.Current.Email;
				string mailBody = PXLocalizer.Localize(Messages.EventNumber, typeof(Messages).FullName) + ": " + _event.NoteID.Value + newLine +
							PXLocalizer.Localize(Messages.Subject, typeof(Messages).FullName) + ": " + _event.Subject + newLine +
							GetEventStringInfo(_event, newLine, string.Empty);
				string mailSubject = PXLocalizer.Localize(Messages.Event, typeof(Messages).FullName) + ": " + _event.Subject;
				NotificationGenerator sender = new NotificationGenerator
								{
									To = mailTo,
									Subject = mailSubject,
									Body = mailBody,
									Owner = Accessinfo.UserID
								};
				using (MemoryStream buffer = new MemoryStream())
				{
					CreateVEvent().Write(buffer);
					sender.AddAttachment("event.ics", buffer.ToArray());
				}
				sender.Send();
			}
			return adapter.Get();
		}

		public PXMenuAction<CRActivity> Action;

		#endregion

		#region Graph Event Handlers

		public override int ExecuteInsert(string viewName, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
			{
				Guid userId;
				Users foundUser = !GUID.TryParse(Convert.ToString(values["Name"]), out userId) ? null :
				UsersSearch.SearchWindowed<Asc<Users.pKID>>(new object[] { userId }, 0, 1);
				if (foundUser == null)
				{
					OrderedDictionary newOtherValues = new OrderedDictionary();
					newOtherValues["EventNoteID"] = Events.Current.NoteID;
					newOtherValues["Name"] = values["Name"];
					newOtherValues["Email"] = values["Email"];
					newOtherValues["Comment"] = values["Comment"];
					newOtherValues["Invitation"] = values["Invitation"] ?? PXInvitationStatusAttribute.NOTINVITED;
					AllAttendees.View.Clear();
					int insertOtherResult = 0;
					EPOtherAttendee otherAttendee = PXSelect<EPOtherAttendee,
						Where<EPOtherAttendee.eventNoteID, Equal<Required<CRActivity.noteID>>,
							And<EPOtherAttendee.email, Equal<Required<EPOtherAttendee.email>>>>>.Select(this, Events.Current.NoteID, values["Email"]);
					if (otherAttendee == null)
					{
						insertOtherResult = base.ExecuteInsert("OtherAttendees", newOtherValues);
					}
					if (insertOtherResult > 0) GetOtherAttendeeValuesExt(values, OtherAttendees.Current.AttendeeID);
					return insertOtherResult;
				}
				OrderedDictionary newValues = new OrderedDictionary();
				newValues["EventNoteID"] = Events.Current.NoteID;
				newValues["UserID"] = foundUser.PKID;
				newValues["Comment"] = values["Comment"];
				newValues["Invitation"] = values["Invitation"] ?? PXInvitationStatusAttribute.NOTINVITED;
				AllAttendees.View.Clear();
				int insertResult = base.ExecuteInsert("Attendees", newValues);
				if (insertResult > 0) GetAttendeeValuesExt(values, Attendees.Current.UserID);
				return insertResult;
			}
			return base.ExecuteInsert(viewName, values, parameters);
		}

		public override int ExecuteUpdate(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
			{
				bool oldRowIsOtherAttendee = _OTHER_ATTENDEE_TYPE.Equals(Convert.ToInt32(keys["Type"]));
				Guid userId;
				Users foundUser = !GUID.TryParse(Convert.ToString(values["Name"]), out userId) ? null :
				UsersSearch.SearchWindowed<Asc<Users.pKID>>(new object[] { userId }, 0, 1);
				if (foundUser == null)
				{
					OrderedDictionary newOtherKeys = new OrderedDictionary();
					newOtherKeys["AttendeeID"] = oldRowIsOtherAttendee ? keys["Key"] : null;
					newOtherKeys["EventNoteID"] = keys["EventNoteID"];
					OrderedDictionary newOtherValues = new OrderedDictionary();
					newOtherValues["Name"] = values["Name"];
					newOtherValues["Email"] = values["Email"];
					newOtherValues["Comment"] = values["Comment"];
					newOtherValues["Invitation"] = values["Invitation"] ?? PXInvitationStatusAttribute.NOTINVITED;
					AllAttendees.View.Clear();
				    if (!oldRowIsOtherAttendee)
				    {
                        OrderedDictionary keysForDelete = new OrderedDictionary();
                        keysForDelete["Key"] = keys["Key"];
                        keysForDelete["EventNoteID"] = keys["EventNoteID"];
                        base.ExecuteDelete("Attendees", keysForDelete, null);
				    }                    
					int updateOtherResult = base.ExecuteUpdate("OtherAttendees", newOtherKeys, newOtherValues, parameters);
					if (updateOtherResult > 0) GetOtherAttendeeValuesExt(values, newOtherKeys["AttendeeID"]);
					return updateOtherResult;
				}

				OrderedDictionary newKeys = new OrderedDictionary();

                newKeys["UserID"] = keys["Key"];		        
				newKeys["EventNoteID"] = keys["EventNoteID"];
                newKeys["Type"] = keys["Type"];
				OrderedDictionary newValues = new OrderedDictionary();
				newValues["Comment"] = values["Comment"];
				newValues["Invitation"] = PXInvitationStatusAttribute.NOTINVITED;
				newValues["UserID"] = values["Name"];
				AllAttendees.View.Clear();

			    if (oldRowIsOtherAttendee)
			    {
                    OrderedDictionary keysForDelete = new OrderedDictionary();
                    keysForDelete["AttendeeID"] = keys["Key"];
                    keysForDelete["EventNoteID"] = keys["EventNoteID"];
                    base.ExecuteDelete("OtherAttendees", keysForDelete, null);
			    }
				int updateResult = base.ExecuteUpdate("Attendees", newKeys, newValues, parameters);
				if (updateResult > 0) GetAttendeeValuesExt(values, newKeys["UserID"]);
				return updateResult;
			}
			return base.ExecuteUpdate(viewName, keys, values, parameters);
		}

		public override int ExecuteDelete(string viewName, IDictionary keys, IDictionary values, params object[] parameters)
		{
			if (viewName == "AllAttendees" || viewName == "AllAttendeesAndOwner")
				switch (Convert.ToInt32(keys["Type"]))
				{
					case _OTHER_ATTENDEE_TYPE:
						var otherAttKeys = new OrderedDictionary();
						otherAttKeys["AttendeeID"] = keys["Key"];
						otherAttKeys["EventNoteID"] = keys["EventNoteID"];
						OtherAttendees.View.Clear();
						return base.ExecuteDelete("OtherAttendees", otherAttKeys, null);
					case _USER_ATTENDEE_TYPE:
						if (IsOwner(keys["Key"])) return 0;
						var attKeys = new OrderedDictionary();
						attKeys["UserID"] = keys["Key"];
						attKeys["EventNoteID"] = keys["EventNoteID"];
						Attendees.View.Clear();
						return base.ExecuteDelete("Attendees", attKeys, null);
					default:
						return 0;
				}
			return base.ExecuteDelete(viewName, keys, values, parameters);
		}

		#endregion

		#region Event Handlers

		#region CRActivity
		[EPStartDate(AllDayField = typeof(CRActivity.allDay), DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time", IgnoreRequireTimeOnActivity = true)]		
		[PXFormula(typeof(Round30Minutes<TimeZoneNow>))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_StartDate_CacheAttached(PXCache cache) {}
		
		[PXDefault(typeof(CRActivityClass.events))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ClassID_CacheAttached(PXCache cache) { }
				
		[PXFormula(typeof(False))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_NeedToBeDeleted_CacheAttached(PXCache cache) { }

		protected virtual void PMTimeActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as PMTimeActivity;
			if (row == null) return;

			int timespent = 0;
			int overtimespent = 0;
			int timebillable = 0;
			int overtimebillable = 0;

			foreach (CRPMTimeActivity child in ChildActivities.Select(row.RefNoteID))
			{
				timespent += (child.TimeSpent ?? 0);
				overtimespent += (child.OvertimeSpent ?? 0);
				timebillable += (child.TimeBillable ?? 0);
				overtimebillable += (child.OvertimeBillable ?? 0);
			}

			row.TimeSpent = timespent;
			row.OvertimeSpent = overtimespent;
			row.TimeBillable = timebillable;
			row.OvertimeBillable = overtimebillable;

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeSpent>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(cache, row, false);
		}

		protected virtual void CRActivity_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PXResourceScheduleAttribute.AssumeWorkingTime(AttendeeResources.View, Setup.Current.SearchOnlyInWorkingTime == true);

			CRActivity row = e.Row as CRActivity;
			if (row == null) return;

			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			string origStatus = (string)cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;			

			bool editable = origStatus == ActivityStatusListAttribute.Open;
			PXUIFieldAttribute.SetEnabled(cache, row, editable);
			PXUIFieldAttribute.SetEnabled(tActCache, tAct, editable);

			PXUIFieldAttribute.SetEnabled<CRActivity.noteID>(cache, row);			
			PXUIFieldAttribute.SetEnabled<CRActivity.createdByID>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.completedDate>(cache, row, false);
            PXUIFieldAttribute.SetEnabled<CRActivity.source>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.workgroupID>(cache, e.Row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.ownerID>(cache, e.Row, false);
			
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeSpent>(tActCache, tAct, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeSpent>(tActCache, tAct, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.timeBillable>(tActCache, tAct, false);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.overtimeBillable>(tActCache, tAct, false);
			
			GotoParentActivity.SetEnabled(row.ParentNoteID != null);

			this.ChildActivities.View.AllowUpdate = false;

			this.ChildActivities.Cache.AllowInsert =
				this.ChildActivities.Cache.AllowDelete =
				this.Attendees.Cache.AllowInsert =
				this.Attendees.Cache.AllowUpdate =
				this.Attendees.Cache.AllowDelete = editable;
			
			PXUIFieldAttribute.SetEnabled<CRReminder.isReminderOn>(Reminder.Cache, Reminder.SelectSingle(), editable);

			PXResourceScheduleAttribute.AllowUpdate(AttendeeResources.View, cache.AllowUpdate);
			AllowOperationsWithView(Attendees, cache.AllowUpdate);
			AllowOperationsWithView(OtherAttendees, cache.AllowUpdate);
			AllowOperationsWithView(AllAttendees, cache.AllowUpdate);

			bool isOwner = IsCurrentUserOwnerOfEvent(row);
			if (!isOwner)
			{
				cache.AllowInsert = false;
				cache.AllowUpdate = false;
				cache.AllowDelete = false;
				PXUIFieldAttribute.SetEnabled(cache, e.Row, false);
				
				Actions[_SAVE_ACTION_NAME].SetEnabled(false);
				Actions[_SAVECLOSE_ACTION_NAME].SetEnabled(false);

				acceptInvitation.SetVisible(true);
				rejectInvitation.SetVisible(true);
			}
			else
			{
				sendPersonalInvitation.SetVisible(true);
				sendInvitations.SetVisible(true);
			}
            
			Complete.SetVisible(isOwner);
			CancelActivity.SetVisible(isOwner);

			Complete.SetEnabled(row.UIStatus != ActivityStatusListAttribute.Completed && row.UIStatus != ActivityStatusListAttribute.Canceled);
			CancelActivity.SetEnabled(row.UIStatus != ActivityStatusListAttribute.Completed && row.UIStatus != ActivityStatusListAttribute.Canceled);

			if (CurrentAttendee.Current == null)
			{
				PXResultset<EPAttendeeForAcceptReject> res = CurrentAttendee.Select();
				if (res != null && res.Count > 0) CurrentAttendee.Current = (EPAttendeeForAcceptReject)res[0];
			}

		    if (CurrentAttendee.Current == null && !isOwner)
		    {
                acceptInvitation.SetEnabled(false);
                rejectInvitation.SetEnabled(false);
		    }
		    else
		    {
                int? attendeeInvitation = CurrentAttendee.Current == null ? PXInvitationStatusAttribute.NOTINVITED : CurrentAttendee.Current.Invitation;

                acceptInvitation.SetEnabled(attendeeInvitation != PXInvitationStatusAttribute.ACCEPTED && attendeeInvitation != PXInvitationStatusAttribute.CANCELED
                    && row.UIStatus != ActivityStatusListAttribute.Completed && row.UIStatus != ActivityStatusListAttribute.Canceled);
                rejectInvitation.SetEnabled(attendeeInvitation != PXInvitationStatusAttribute.REJECTED && attendeeInvitation != PXInvitationStatusAttribute.CANCELED
                    && row.UIStatus != ActivityStatusListAttribute.Completed && row.UIStatus != ActivityStatusListAttribute.Canceled);    
		    }
			
			MarkAs(cache, row, Accessinfo.UserID, EPViewStatusAttribute.VIEWED);
		}

		protected virtual void CRActivity_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			var command = e.Operation & PXDBOperation.Command;
			if (command != PXDBOperation.Insert && command != PXDBOperation.Update && command != PXDBOperation.Delete) return;

			var row = e.Row as CRActivity;
			if (row == null) return;
			
			var dbEvent = ReadDBEvent(row.NoteID);
			if (dbEvent == null) return;

			bool isDateChanged = dbEvent.StartDate != row.StartDate || dbEvent.EndDate != row.EndDate;
			if (isDateChanged && !IsEventInThePast(row) && IsEventEditable(row))
			{
				List<AllTypeAttendee> invitedAttendees = GetInvitedAttendees().ToList();
				if (invitedAttendees.Any())
				{
					if (!IsImport)
				    {
				        WebDialogResult confirm = Events.Ask(
				            _ASK_KEY_7,
				            Messages.ConfirmRescheduleNotificationHeader,
				            Messages.ConfirmRescheduleNotificationText,
				            MessageButtons.YesNo);
				        if (confirm == WebDialogResult.Yes) SendEMails(NotificationTypes.Reschedule, invitedAttendees);
				    }
				    else
				    {
				        SendEMails(NotificationTypes.Reschedule, invitedAttendees);
				    }
				}
			}					
		}

		protected virtual void CRActivity_RefNoteID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CRActivity activity = (CRActivity)e.Row;
			if (activity == null) return;

			if (Caches[typeof(RelatedEntity)].Current != null)
			{
				var related = Views["RelatedEntity"].SelectSingle();
				if (related == null) return;

				var relatedType = related.GetType();

				if (typeof(Contact).IsAssignableFrom(relatedType))
				{
					Contact contact = related as Contact;
					activity.ContactID = contact?.ContactID;
					activity.BAccountID = contact?.BAccountID;
				}
				else if (typeof(BAccount).IsAssignableFrom(relatedType))
				{
					BAccount contact = related as BAccount;
					activity.ContactID = null;
					activity.BAccountID = contact?.BAccountID;
				}
			}
		}
		
		#endregion

		#region AddAttendeeFilter

		protected virtual void AddAttendeeFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			cache.IsDirty = false;
		}

		protected virtual void AddAttendeeFilter_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		#endregion

		#region EPOtherAttendeeWithNotification

		protected virtual void EPOtherAttendeeWithNotification_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && Events.Current != null) row.EventNoteID = Events.Current.NoteID;
		}

		protected virtual void EPOtherAttendeeWithNotification_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable)
			{
				ConfirmDeleteOtherAttendees.Current.AttendeeID = row.AttendeeID;
				ConfirmDeleteOtherAttendees.Current.WithNotification = true;
				if (!IsImport)
			    {
			        WebDialogResult confirmDelete = ConfirmDeleteOtherAttendees.Ask(
			            _ASK_KEY_2,
			            Messages.ConfirmDeleteAttendeeHeader,
			            string.Format("{0} {1} {2}?", Messages.ConfirmDeleteAttendeeText, row.Email, row.Name),
			            MessageButtons.YesNo);
			        if (confirmDelete != WebDialogResult.Yes)
			            e.Cancel = true;
			        else if (row.Invitation == PXInvitationStatusAttribute.INVITED &&
			                 row.Invitation == PXInvitationStatusAttribute.ACCEPTED)
			        {
			            SendEMail(NotificationTypes.Cancel, new AllTypeAttendee
			            {
			                Type = _OTHER_ATTENDEE_TYPE,
			                Key = row.AttendeeID.ToString(),
			                EventNoteID = row.EventNoteID,
			                Name = row.Name,
			                DisplayName = row.Name,
			                Email = row.Email,
			                Comment = row.Comment,
			                Invitation = row.Invitation
			            });
			        }
			    }
			    else
			    {
                    if (row.Invitation == PXInvitationStatusAttribute.INVITED &&
                             row.Invitation == PXInvitationStatusAttribute.ACCEPTED)
                    {
                        SendEMail(NotificationTypes.Cancel, new AllTypeAttendee
                        {
                            Type = _OTHER_ATTENDEE_TYPE,
                            Key = row.AttendeeID.ToString(),
                            EventNoteID = row.EventNoteID,
                            Name = row.Name,
                            DisplayName = row.Name,
                            Email = row.Email,
                            Comment = row.Comment,
                            Invitation = row.Invitation
                        });
                    }
			    }
			}
		}

		protected virtual void EPOtherAttendeeWithNotification_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			EPOtherAttendeeWithNotification newRow = e.NewRow as EPOtherAttendeeWithNotification;
			if (row == null || newRow == null ||
				!IsCurrentEventPersisted || IsCurrentEventInThePast || !IsCurrentEventEditable) return;

			newRow.NotifyOnRowUpdated = false;
			if (newRow.Email != row.Email && row.Invitation != PXInvitationStatusAttribute.NOTINVITED)
			{
				WebDialogResult confirm;

				if (!IsImport)
			    {
			        confirm = AllAttendees.Ask(
			            _ASK_KEY_3,
			            Messages.EMailWasChanged,
			            string.Format("{0} ({1})?", Messages.SendInvitationToNewEMail, newRow.Email),
			            MessageButtons.YesNo);
			    }
			    else
			    {
			        confirm = WebDialogResult.Yes;
			    }

			    switch (confirm)
				{
					case WebDialogResult.Yes:
						newRow.NotifyOnRowUpdated = true;
						break;
					case WebDialogResult.Cancel:
						e.Cancel = true;
						break;
				}
				newRow.Invitation = PXInvitationStatusAttribute.INVITED;
			}
		}

		protected virtual void EPOtherAttendeeWithNotification_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			EPOtherAttendeeWithNotification row = e.Row as EPOtherAttendeeWithNotification;
			if (row != null && row.NotifyOnRowUpdated == true)
				SendEMail(NotificationTypes.Invitation, SearchAllTypeAttendee(_OTHER_ATTENDEE_TYPE, row.AttendeeID));
			AllAttendees.ClearDialog();
		}

		#endregion

		#region DeleteOtherAttendeeFilter

		protected virtual void DeleteOtherAttendeeFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			DeleteOtherAttendeeFilter row = e.Row as DeleteOtherAttendeeFilter;
			if (row != null && row.AttendeeID.HasValue)
			{
				EPOtherAttendeeWithNotification attendee =
					OtherAttendees.Search<EPOtherAttendeeWithNotification.attendeeID>(row.AttendeeID);
				PXUIFieldAttribute.SetEnabled<DeleteOtherAttendeeFilter.withNotification>(cache, row,
					attendee == null || attendee.Invitation != PXInvitationStatusAttribute.NOTINVITED);
			}
		}

		#endregion

		#region AllTypeAttendee

		protected virtual void AllTypeAttendee_Comment_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			_allTypeAttendee_FieldSelecting(e, "Comment");
		}

		protected virtual void AllTypeAttendee_Email_FieldSelecting(PXCache cache, PXFieldSelectingEventArgs e)
		{
			_allTypeAttendee_FieldSelecting(e, "Email");
		}
		
		protected virtual void AllTypeAttendee_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void AllTypeAttendee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			var row = e.Row as AllTypeAttendee;
			if (row != null && row.Type == _USER_ATTENDEE_TYPE && IsOwner(row.Key))
			{
				PXUIFieldAttribute.SetEnabled(cache, row, false);
				//PXUIFieldAttribute.SetEnabled<AllTypeAttendee.name>(cache, row, false);
				//PXUIFieldAttribute.SetEnabled<AllTypeAttendee.displayName>(cache, row, false);
			}
		}

		#endregion

		#region Users

		[PXDBGuidMaintainDeleted]
		[PXDefault]
		[PXUIField(Visibility = PXUIVisibility.Invisible, Visible = false)]
		protected virtual void Users_PKID_CacheAttached(PXCache cache){}

		#endregion

		#endregion

		#region Public Methods

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetWorkStartTime)]
		public DateTime? GetWorkStartTime(object row, DateTime date)
		{
			var user = row as Users;
			if (user == null) return date.Date;

			DateTime? result = null;
			PXResultset<CSCalendar> calendarSet = WorkCalendar.SelectWindowed(0, 1, user.PKID);
			CSCalendar calendar = calendarSet.Count > 0 ? (CSCalendar)calendarSet[0] : null;
			if (calendar != null)
				switch (date.DayOfWeek)
				{
					case DayOfWeek.Monday:
						if (calendar.MonWorkDay == true) result = calendar.MonStartTime;
						break;
					case DayOfWeek.Tuesday:
						if (calendar.TueWorkDay == true) result = calendar.TueStartTime;
						break;
					case DayOfWeek.Wednesday:
						if (calendar.WedWorkDay == true) result = calendar.WedStartTime;
						break;
					case DayOfWeek.Thursday:
						if (calendar.ThuWorkDay == true) result = calendar.ThuStartTime;
						break;
					case DayOfWeek.Friday:
						if (calendar.FriWorkDay == true) result = calendar.FriStartTime;
						break;
					case DayOfWeek.Saturday:
						if (calendar.SatWorkDay == true) result = calendar.SatStartTime;
						break;
					case DayOfWeek.Sunday:
						if (calendar.SunWorkDay == true) result = calendar.SunStartTime;
						break;
				}
			if (result != null) result = AddTimeToDate(date, result);
			return result;
		}

		[ResHandler(_ATTENDEE_VIEW_NAME, ResHandler.Types.GetWorkEndTime)]
		public DateTime? GetWorkEndTime(object row, DateTime date)
		{
			var user = row as Users;
			if (user == null) return date.Date.AddDays(1D);

			DateTime? result = null;
			PXResultset<CSCalendar> calendarSet = WorkCalendar.SelectWindowed(0, 1, user.PKID);
			CSCalendar calendar = calendarSet.Count > 0 ? (CSCalendar)calendarSet[0] : null;
			if (calendar != null)
				switch (date.DayOfWeek)
				{
					case DayOfWeek.Monday:
						if (calendar.MonWorkDay == true) result = calendar.MonEndTime;
						break;
					case DayOfWeek.Tuesday:
						if (calendar.TueWorkDay == true) result = calendar.TueEndTime;
						break;
					case DayOfWeek.Wednesday:
						if (calendar.WedWorkDay == true) result = calendar.WedEndTime;
						break;
					case DayOfWeek.Thursday:
						if (calendar.ThuWorkDay == true) result = calendar.ThuEndTime;
						break;
					case DayOfWeek.Friday:
						if (calendar.FriWorkDay == true) result = calendar.FriEndTime;
						break;
					case DayOfWeek.Saturday:
						if (calendar.SatWorkDay == true) result = calendar.SatEndTime;
						break;
					case DayOfWeek.Sunday:
						if (calendar.SunWorkDay == true) result = calendar.SunEndTime;
						break;
				}
			if (result != null) result = AddTimeToDate(date, result);
			return result;
		}

		public override void CompleteRow(CRActivity row)
		{
			if (row != null) CompleteEvent(row);
		}

		public override void CancelRow(CRActivity row)
		{
			if (row != null) CancelEvent(row);
		}

		#endregion

		#region Private Methods

		private DateTime? RoundTo30Min(DateTime? rawDate)
		{
			if (rawDate == null) return null;

			var date = (DateTime) rawDate;
			var minutes = date.Minute;
			if (minutes != 0)
				minutes = minutes <= 30 ? 30 : 60;

			return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0).AddMinutes(minutes);
		}
		
		private void AcceptParticipation(bool accept)
		{
			if (Events.Current == null) return;
			foreach (EPAttendeeForAcceptReject attendee in CurrentAttendee.Select().RowCast<EPAttendeeForAcceptReject>())
			{
				attendee.Invitation = accept ? PXInvitationStatusAttribute.ACCEPTED : PXInvitationStatusAttribute.REJECTED;
				CurrentAttendee.Update(attendee);
			}
			Actions.PressSave();
		}

		private void CompleteEvent(CRActivity row)
		{
			string origStatus = (string)Events.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;

			if (row == null ||
				origStatus == ActivityStatusListAttribute.Completed ||
				origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}

			CRActivity activityCopy = (CRActivity)Events.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusListAttribute.Completed;
			Events.Cache.Update(activityCopy);
			Actions.PressSave();
		}

		private void CancelEvent(CRActivity row)
		{
			string origStatus = (string)Events.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;
			if (row == null ||
				origStatus == ActivityStatusListAttribute.Completed ||
				origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}

			CRActivity activityCopy = (CRActivity)Events.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusListAttribute.Canceled;
			Events.Cache.Update(activityCopy);
			Actions.PressSave();
		}

		private static DateTime AddTimeToDate(DateTime date, DateTime? result)
		{
			return new DateTime(date.Year, date.Month, date.Day, result.Value.Hour, result.Value.Minute, result.Value.Second);
		}

		private static void _allTypeAttendee_FieldSelecting(PXFieldSelectingEventArgs e, string fieldName)
		{
			AllTypeAttendee row = e.Row as AllTypeAttendee;
			if (row != null && _USER_ATTENDEE_TYPE.Equals(row.Type))
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, null, null, null,
					null, null, null, null, fieldName, null, null, null, PXErrorLevel.Undefined, false, null, false,
					PXUIVisibility.Undefined, null, null, null);
			}
		}

		private Users EventOwner
		{
			get
			{
				PXResultset<Users> currentOwnerDBResult = CurrentOwner.Select();
				return currentOwnerDBResult.Count > 0 ? (Users)currentOwnerDBResult[0] : null;
			}
		}

		private IEnumerable<EPAttendee> GetAttendeesByEventOwner(CRActivity _event)
		{
			if (_event == null) yield break;

			foreach (PXResult<EPAttendee> item in PXSelect<EPAttendee,
				Where<EPAttendee.eventNoteID, Equal<Required<EPAttendee.eventNoteID>>,
					And<EPAttendee.userID, Equal<Required<EPAttendee.userID>>>>>.
					Select(this, _event.NoteID, _event.OwnerID))
			{
				yield return (EPAttendee)item;
			}
		}

		private void SendEMail(NotificationTypes invite, object attendee)
		{
			SendEMails(invite, new object[] { attendee });
		}

		private void SendEMails(NotificationTypes invite, IEnumerable attendees)
		{
			// Check
			List<AllTypeAttendee> mails = (attendees.Cast<object>().Select(item => item is PXResult ? item as PXResult<AllTypeAttendee> : (AllTypeAttendee) item)).ToList();

			//Send
			byte[] card;
			using (var buffer = new MemoryStream())
			{
				CreateVEvent().Write(buffer);
				card = buffer.ToArray();
			}

			var owner = EventOwner;
			var _event = Events.Current;
			var newLine = Environment.NewLine + Environment.NewLine;
			string contactInfo = null;
			var settings = Setup.Current;
			if (owner != null && settings.AddContactInformation == true)
			{
				contactInfo = PXLocalizer.Localize(Messages.ContactPerson, typeof(Messages).FullName) + ": " + newLine;
				contactInfo += PXLocalizer.Localize(Messages.Name, typeof(Messages).FullName) + ": " + owner.DisplayName + newLine;
				if (!string.IsNullOrWhiteSpace(owner.Email))
					contactInfo += PXLocalizer.Localize(Messages.Email, typeof(Messages).FullName) + ": " + owner.Email + newLine;
				if (!string.IsNullOrWhiteSpace(owner.Phone))
					contactInfo += PXLocalizer.Localize(Messages.Phone, typeof(Messages).FullName) + ": " + owner.Phone + newLine;
			}
			foreach (var attendee in mails)
			{
				// body
				var sender = settings.IsSimpleNotification == true ?
					SimpleFillMail(invite, _event, owner, contactInfo) :
					TemplateFillMail(invite, _event);

				// address
				sender.MailAccountId = MailAccountManager.DefaultMailAccountID;
				sender.To = attendee.Email;

				// subject
				string subjectPrefix;
				switch (invite)
				{
					case NotificationTypes.Cancel:
						subjectPrefix = PXLocalizer.Localize(Messages.CancelInvitationTo, typeof(Messages).FullName);
						break;
					case NotificationTypes.Reschedule:
						subjectPrefix = PXLocalizer.Localize(Messages.RescheduleOf, typeof(Messages).FullName);
						break;
					case NotificationTypes.Invitation:
					default:
						subjectPrefix = PXLocalizer.Localize(Messages.InvitationTo, typeof(Messages).FullName);
						break;
				}
				sender.Subject = subjectPrefix + _event.Subject;

				// attachments
				sender.AddAttachment("event.ics", card);

				sender.ParentNoteID = _event.NoteID;
				
				foreach (CRSMEmail item in sender.Send())
				{
					var msg = item;
					// Update attendees flags
					switch ((int)attendee.Type)
					{
						case _OTHER_ATTENDEE_TYPE:
							var otherMessage = new EPOtherAttendeeMessage();
							otherMessage.EventNoteID = _event.NoteID;
							otherMessage.AttendeeID = Convert.ToInt32(attendee.Key);
							otherMessage.MessageID = msg.ImcUID;
							OtherAttendeeMessages.Insert(otherMessage);
							break;
						case _USER_ATTENDEE_TYPE:
							var message = new EPAttendeeMessage();
							message.EventNoteID = _event.NoteID;
							message.UserID = new Guid(attendee.Key);
							message.MessageID = msg.ImcUID;
							AttendeeMessages.Insert(message);
							break;
					}
				}
				switch (invite)
				{
					case NotificationTypes.Invitation:
						attendee.Invitation = PXInvitationStatusAttribute.INVITED;
						break;
					case NotificationTypes.Reschedule:
						attendee.Invitation = PXInvitationStatusAttribute.RESCHEDULED;
						break;
					case NotificationTypes.Cancel:
						attendee.Invitation = PXInvitationStatusAttribute.CANCELED;
						break;
				}
				UpdateAllTypeAttendee(attendee);
			}
            if (mails.Count > 0)
            {
                SafetyPersist(Attendees, AttendeeMessages, OtherAttendees, OtherAttendeeMessages);
            }
        }

		private NotificationGenerator SimpleFillMail(NotificationTypes invite, CRActivity _event, Users owner, string contactInfo)
		{
			var sender = new NotificationGenerator();
			string headerAddInfo;
			string bodyAddInfo = string.Empty;
			switch (invite)
			{
				case NotificationTypes.Cancel:
					headerAddInfo = PXLocalizer.Localize(Messages.EventWasCanceled, typeof(Messages).FullName);
					break;
				case NotificationTypes.Reschedule:
					headerAddInfo = PXLocalizer.Localize(Messages.EventWasRescheduled, typeof(Messages).FullName);
					bodyAddInfo = GetEventStringInfo(_event, _WIKI_NEW_LINE, PXLocalizer.Localize(Messages.CreateNew, typeof(Messages).FullName) +  " ");
					break;
				case NotificationTypes.Invitation:
				default:
					if (owner != null) headerAddInfo = owner.DisplayName + " " + PXLocalizer.Localize(Messages.InvitedYouToAnEvent, typeof(Messages).FullName);
					else headerAddInfo = PXLocalizer.Localize(Messages.YouAreInvitedToAnEvent, typeof(Messages).FullName);
					bodyAddInfo = GetEventStringInfo(_event, _WIKI_NEW_LINE, string.Empty);
					break;
			}

			var body = headerAddInfo + _WIKI_NEW_LINE;
			body += PXLocalizer.Localize(Messages.Subject, typeof(Messages).FullName) + ": " + _event.Subject.Trim() + _WIKI_NEW_LINE;
			if (!string.IsNullOrWhiteSpace(_event.Location))
				body += PXLocalizer.Localize(Messages.Location, typeof(Messages).FullName) + ": " + _event.Location.Trim() + _WIKI_NEW_LINE;
			body += bodyAddInfo + _WIKI_NEW_LINE;
			body += _WIKI_NEW_LINE + contactInfo;

			sender.Body = body;
			return sender;
		}

		private TemplateNotificationGenerator TemplateFillMail(NotificationTypes invite, CRActivity _event)
		{
			var settings = Setup.Current;
			int? templateId;
			switch (invite)
			{
				case NotificationTypes.Cancel:
					templateId = settings.CancelInvitationTemplateID;
					break;
				case NotificationTypes.Reschedule:
					templateId = settings.RescheduleTemplateID;
					break;
				case NotificationTypes.Invitation:
				default:
					templateId = settings.InvitationTemplateID;
					break;
			}

			if (templateId == null)
				throw new Exception(Messages.EmailTemplateIsNotConfigured);

			var sender = TemplateNotificationGenerator.Create(_event, (int)templateId);
			return sender;
		}

		private void SafetyPersist(params PXSelectBase[] views)
		{
			using (var tscope = new PXTransactionScope())
			{
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Insert);
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Update);
				foreach (var view in views)
					view.Cache.Persist(PXDBOperation.Delete);
				tscope.Complete(this);
			}
			foreach (var view in views)
			{
				view.View.Clear();
				view.Cache.Clear();
				view.Cache.IsDirty = false;
			}
		}

		private void UpdateAllTypeAttendee(AllTypeAttendee row)
		{
			if (row == null) return;

			switch (Convert.ToInt32(row.Type))
			{
				case _USER_ATTENDEE_TYPE:
					EPAttendee attendee = Attendees.Search<EPAttendee.userID>(row.Key);
					if (attendee != null)
					{
						attendee.Invitation = row.Invitation;
						Attendees.Update(attendee);
					}
					break;
				case _OTHER_ATTENDEE_TYPE:
					EPOtherAttendeeWithNotification otherAttendee =
						OtherAttendees.Search<EPOtherAttendeeWithNotification.attendeeID>(row.Key);
					if (otherAttendee != null)
					{
						otherAttendee.Name = row.Name;
						otherAttendee.Email = row.Email;
						otherAttendee.Comment = row.Comment;
						otherAttendee.Invitation = row.Invitation;
						OtherAttendees.Update(otherAttendee);
					}
					break;
			}
		}

		private CRActivity ReadDBEvent(object taskId)
		{
			PXResultset<CRActivity> resultset =
				PXSelectReadonly<CRActivity,
					Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.
				Select(this, taskId);
			return resultset.Count > 0 ? (CRActivity)resultset[0] : null;
		}

		private void AssertEventStatus()
		{
			if (!IsCurrentEventPersisted) throw new PXException(Messages.EventIsNotSaved);
			if (IsCurrentEventInThePast) throw new PXException(Messages.EventIsThePast);
			if (!IsCurrentEventEditable) throw new PXException(Messages.EventIsNotEditable);
		}

		private ConfirmNotificationResult ConfirmCancelAttendeeInvitations(PXAdapter adapter)
		{
			if (IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable && adapter.ExternalCall)
			{
				IEnumerable<AllTypeAttendee> invitedAttendees = GetInvitedAttendees();
				if (invitedAttendees.Any())
				{
					if (!IsImport)
				    {
				        WebDialogResult confirmResult = AllAttendees.Ask(
				            _ASK_KEY_5,
				            Messages.CancelInvitation,
				            Messages.ConfirmCancelAttendeeInvitations,
				            MessageButtons.YesNoCancel);
				        if (confirmResult == WebDialogResult.Yes) return ConfirmNotificationResult.Yes;
				    }
				    else
				    {
				        return ConfirmNotificationResult.Yes;
				    }
				}
			}
			return ConfirmNotificationResult.No;
		}

		private IEnumerable<AllTypeAttendee> GetInvitedAttendees()
		{
			return AllAttendees.Select()
				.RowCast<AllTypeAttendee>()
				.Where(attendee => attendee.Invitation != PXInvitationStatusAttribute.NOTINVITED 
				                && attendee.Invitation != PXInvitationStatusAttribute.REJECTED);
		}

		private void CancelAttendeeInvitations(ConfirmNotificationResult target)
		{
			SendEMails(NotificationTypes.Cancel, GetInvitedAttendees());
		}

		private ConfirmNotificationResult ConfirmAttendeeInvitations(PXAdapter adapter)
		{
            if (Events.Current != null && IsCurrentEventPersisted && !IsCurrentEventInThePast && IsCurrentEventEditable && !IsCurrentEventSynchronized && adapter.ExternalCall)
			{
				PXResultset<AllTypeAttendee> attendees = AllAttendees.Select();
				PXResultset<AllTypeAttendee> notInvitedAttendees =
					AllAttendees.SearchAll<Asc<AllTypeAttendee.invitation>>(new object[] { PXInvitationStatusAttribute.NOTINVITED });
				if (attendees.Count > 0 && notInvitedAttendees.Count > 0)
				{
					if (!IsImport)
				    {
				        WebDialogResult confirmResult = AllAttendees.Ask(
				            _ASK_KEY_6,
				            Messages.SendInvitations,
				            Messages.NotifyAttendees,
				            MessageButtons.YesNoCancel);
				        if (confirmResult == WebDialogResult.Yes)
				        {
				            return notInvitedAttendees.Count == attendees.Count
				                ? ConfirmNotificationResult.YesForAll
				                : ConfirmNotificationResult.Yes;
				        }
				    }
				    else
				    {
                        return notInvitedAttendees.Count == attendees.Count
                            ? ConfirmNotificationResult.YesForAll
                            : ConfirmNotificationResult.Yes;			        
				    }
				}
			}
			return ConfirmNotificationResult.No;
		}

		private void InviteAttendees(ConfirmNotificationResult target)
		{
			switch (target)
			{
				case ConfirmNotificationResult.YesForAll:
					SendEMails(NotificationTypes.Invitation, AllAttendees.Select());
					break;
				case ConfirmNotificationResult.Yes:
					SendEMails(NotificationTypes.Invitation, 
						AllAttendees.SearchAll<Asc<AllTypeAttendee.invitation>>(new object[] { PXInvitationStatusAttribute.NOTINVITED }));
					break;
			}
		}

		private static void AllowOperationsWithView(PXSelectBase view, bool canModifyAttendees)
		{
			view.Cache.AllowDelete = canModifyAttendees;
			view.Cache.AllowInsert = canModifyAttendees;
			view.Cache.AllowUpdate = canModifyAttendees;
		}

		private static void GetValuesExt(PXCache cache, IDictionary values)
		{
			var list = new List<string>(values.Count);
			foreach (string key in values.Keys)
				list.Add(key);
			foreach (string key in list)
				values[key] = cache.GetValueExt(cache.Current, key);
		}

		private void GetAttendeeValuesExt(IDictionary values, object userId)
		{
			AllAttendees.Current = SearchAllTypeAttendee(_USER_ATTENDEE_TYPE, userId);
			GetValuesExt(AllAttendees.Cache, values);
		}

		private void GetOtherAttendeeValuesExt(IDictionary values, object attendeeId)
		{
			AllAttendees.Current = SearchAllTypeAttendee(_OTHER_ATTENDEE_TYPE, attendeeId);
			GetValuesExt(AllAttendees.Cache, values);
		}

		private AllTypeAttendee SearchAllTypeAttendee(int type, object key)
		{
			return AllAttendees.SearchWindowed<Asc<AllTypeAttendee.type, Asc<AllTypeAttendee.key>>>(
				new object[] { type, Convert.ToString(key) }, 0, 1);
		}

		private string GetEventStringInfo(CRActivity _event, string newLineString, string prefix)
		{
			var start = _event.StartDate.Value;
			var end = _event.EndDate.Value;
			var timeZone = LocaleInfo.GetTimeZone().DisplayName;
			string bodyAddInfo = prefix + PXLocalizer.Localize(Messages.StartDate, typeof(Messages).FullName) + ": " + start.ToLongDateString() + " " + start.ToShortTimeString() + " " + timeZone + newLineString +
								 prefix + PXLocalizer.Localize(Messages.EndDate, typeof(Messages).FullName) + ": " + end.ToLongDateString() + " " + end.ToShortTimeString() + " " + timeZone;
			CRActivity gEvent = _event as CRActivity;
			if (gEvent != null)
			{
				PXStringState valueExt = Events.Cache.GetValueExt(gEvent, PXLocalizer.Localize(Messages.Duration, typeof(Messages).FullName)) as PXStringState;
				if (valueExt != null)
				{
					bodyAddInfo += newLineString + prefix + PXLocalizer.Localize(Messages.Duration, typeof(Messages).FullName) + ": ";
					string valueText = valueExt.Value.ToString();
					bodyAddInfo += string.IsNullOrEmpty(valueExt.InputMask) ? valueText :
						PX.Common.Mask.Format(valueExt.InputMask, valueText);
				}
			}
			if (!string.IsNullOrEmpty(_event.Body))
			{
				var description = Tools.ConvertHtmlToSimpleText(_event.Body);
				description = description.Replace(Environment.NewLine, newLineString);
				bodyAddInfo += newLineString + description;
			}
			return bodyAddInfo;
		}

		private bool IsCurrentUserOwnerOfEvent(CRActivity row)
		{
			object currentLoginUser = Caches[typeof(AccessInfo)].Current;
			return row == null || row.OwnerID == null || currentLoginUser == null ||
				row.OwnerID == ((AccessInfo)currentLoginUser).UserID;
		}
		private bool IsEventEditable(CRActivity row)
		{
			string origStatus = (string)this.Events.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusAttribute.Open;

			return row == null ||
			       origStatus != ActivityStatusAttribute.Completed ||
			       origStatus != ActivityStatusAttribute.Canceled ;
		}
		private bool IsEventInThePast(CRActivity row)
		{
			return false;
			// TODO: need implementation
		}
		private bool IsEventSynchronized(CRActivity row)
		{
			if (row.Synchronize != true) return false;
			if (PXTimeTagAttribute.SyncScope.IsScoped( )) return true;

			foreach (PXResult<EPEmployee> result in PXSelectReadonly2<EPEmployee,
				InnerJoin<EMailSyncAccount, 
					On<EPEmployee.bAccountID, Equal<EMailSyncAccount.employeeID>>>,
				Where<EPEmployee.userID, Equal<Required<EPEmployee.userID>>, 
					And<EMailSyncAccount.eventsExportDate, IsNotNull>>>.Select(this, row.OwnerID))
			{
				return true;
			}
			return false;
		}

		private bool IsCurrentEventEditable
		{
			get
			{
				return IsEventEditable(Events.Current);
			}
		}
		private bool IsCurrentEventInThePast
		{
			get
			{
				return IsEventInThePast(Events.Current);
			}
		}
		private bool IsCurrentEventPersisted
		{
			get
			{
				PXEntryStatus status = Events.Cache.GetStatus(Events.Current);
				return status != PXEntryStatus.Inserted && status != PXEntryStatus.InsertedDeleted;
			}
		}
		private bool IsCurrentEventSynchronized
		{
			get
			{
				CRActivity row = Events.Current;
				return IsEventSynchronized(row);
			}
		}

		private vEvent CreateVEvent()
		{
			var vevent = VCalendarProcessor.CreateVEvent(Events.Current);
			vevent.Method = "REQUEST";
			return vevent;
		}

		private bool IsOwner(object pkId)
		{
			Guid userId;
			if (pkId == null || !GUID.TryParse(pkId.ToString(), out userId)) return false;

			var owner = EventOwner;
			return owner != null && (Guid)owner.PKID == userId;
		}
		#endregion
	}
}
