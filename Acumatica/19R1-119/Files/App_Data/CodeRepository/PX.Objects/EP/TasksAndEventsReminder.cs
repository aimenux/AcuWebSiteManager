using System;
using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.RichTextEdit;
using PX.Data.SQLTree;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.TX;
using PX.SM;
using PX.Web.UI;

namespace PX.Objects.EP
{   
	[PXHidden]
	public class TasksAndEventsReminder : PXGraph<TasksAndEventsReminder>
	{
		#region Additional Classes	
	       				

		#region ActivityStatistics        
		private class ActivityStatistics : IPrefetchable<ActivityStatistics.Source>
		{
		    public class Source
		    {
		        public readonly string SrceenID;
		        public readonly string ViewName;
                public readonly string ImageKey;
                public readonly string ImageSet;
		        public readonly Type[] Tables;
		        
		        public Source(string screenID, string viewName, string imageKey)
                    :this(screenID, viewName, imageKey, null)
		        {
		           
		        }

                public Source(string screenID, string viewName, string imageKey, Type[] tables)
		        {
                    this.SrceenID = screenID;
                    this.ViewName = viewName;
                    this.ImageKey = imageKey;
                    this.ImageSet = Sprite.AliasMain;
                    this.Tables = tables;
		        }
		    }

            public static readonly ActivityStatistics Empty = new ActivityStatistics();
		    		   
			private DateTime _day;
		    private int _count;
            private int _unreadCount;
            private int _readCount;

		    protected DateTime Day
		    {
		        get { return _day; }
		    }

			public int Count
			{
				get { return _count; }
			}           
            public int ReadCount
            {
                get { return _readCount; }
            }
            public int UnreadCount
            {
                get { return _unreadCount; }
            }

			public void Prefetch(Source source)
			{
				_day = DateTime.Today;

			    _count = RefreshTotal(source.SrceenID, source.ViewName, out _readCount, out _unreadCount);
			    /*
                _tasks = RefreshTotal("EP404000", null, out read, out _newTasks);                                
                _events = RefreshTotal("EP404100", "Events", out read, out _newEvents);
                _emails = RefreshTotal("CO409000", null, out read, out _newEmails);         
                */
			}

            
		    private int RefreshTotal(string screenID, string viewName, out int read, out int unread)
		    {
		        read = -1;
		        unread = -1;
                string actualScreenID = PXContext.GetScreenID();
                using (new PXPreserveScope())
                {
                    try
                    {
                        PXContext.SetScreenID(screenID);
                        PXGraph graph = CreateGraph(screenID);
                        if (graph == null || graph.PrimaryView == null) return -1;
                        if (viewName == null) viewName = graph.PrimaryView;
                        PXView view = graph.Views[viewName];
                        
                        PXView viewFilters;
                        PXFilterRow[] filters = null;
                        if (graph.Views.TryGetValue(viewName + PXFilterableAttribute.FilterHeaderName, out viewFilters))
                        {
                            foreach (FilterHeader fHeader in viewFilters.SelectMulti())
                            {
                                if (fHeader.IsDefault == true)
                                {
                                    int s = 0;
                                    int t = 0;
                                    IList f = graph.ExecuteSelect(viewName + PXFilterableAttribute.FilterRowName, new object[] { fHeader.FilterID },
                                        null, null, null,
                                        new PXFilterRow[] { new PXFilterRow(typeof(FilterRow.isUsed).Name, PXCondition.EQ, true, null) }, ref s, 0, ref t) as IList;
                                    if (f != null && f.Count > 0)
                                    {
                                        filters = new PXFilterRow[f.Count];

                                        PXCache frc = graph.Caches[typeof(FilterRow)];                                        
                                        for (int i = 0; i < f.Count; i++)
                                        {
                                            FilterRow r = (FilterRow)f[i];
                                            var filter = new PXFilterRow(r.DataField, (PXCondition)r.Condition, frc.GetValueExt(r, "ValueSt"), frc.GetValueExt(r, "ValueSt2"));
                                            if (r.OpenBrackets != null) filter.OpenBrackets = r.OpenBrackets.Value;
                                            if (r.CloseBrackets != null) filter.CloseBrackets = r.CloseBrackets.Value;
                                            filter.OrOperator = r.Operator == 1;
                                            filters[i] = filter;
                                        }
                                    }
                                    break;
                                }
                            }
                        }

                        int start = 0;
                        int total = 0;
                        read = -1;
                        unread = -1;

                        using (var scope = new PXFieldScope(view, typeof(CRActivity.noteID), typeof(EPView.status)))
                        foreach (object record in view.Select(null, null, null, null, null, filters, ref start, 101, ref total))
                        {
                            PXResult result = record as PXResult;
                            if (result != null)
                            {
                                EPView v = result.GetItem<EPView>();
                                if (v != null)
                                {
                                    if (v.Status != null && v.Status == EPViewStatusAttribute.VIEWED)
                                    {
                                        read += 1;
                                    }
                                    else
                                        unread += 1;
                                }
                            }
                        }
                        if (read != -1 || unread != -1)
                        {
                            read += 1;
                            unread += 1;
                        }
                        return total;
                    }
                    finally
                    {
                        PXContext.SetScreenID(actualScreenID);
                    }
                }		       
		    }

            private static PXGraph CreateGraph(string screenID)
            {
                PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(screenID);
                if (node == null) return null;
                string graphName = node.GraphType;
                Type gt = System.Web.Compilation.PXBuildManager.GetType(graphName, false) ?? Type.GetType(graphName);

                if (gt == null) return null;

                gt = System.Web.Compilation.PXBuildManager.GetType(PX.Api.CustomizedTypeManager.GetCustomizedTypeFullName(gt), false) ?? gt;
                using (new PXPreserveScope())
                {
                    try
                    {
                        return (PXGraph)PXGraph.CreateInstance(gt);
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                        throw PXException.ExtractInner(ex);
                    }
                }
            }
           
            public static ActivityStatistics GetFromSlot(Source source)
            {
                Type[] tables = source.Tables ??
                                new Type[]{typeof (CRActivity), typeof (EPAttendee), typeof (EPView), typeof (UserPreferences)};
                string key = _ACTIVITY_SLOT_KEY_PREFIX + source.SrceenID + PXAccess.GetUserID();

                var slot = PXDatabase.GetSlot<ActivityStatistics, Source>(key, source, tables);
                if (slot != null && slot.Day != DateTime.Today)
                {
                    PXDatabase.ResetSlot<ActivityStatistics>(key, tables);
                    slot = PXDatabase.GetSlot<ActivityStatistics, Source>(key, source, tables);
                }
                return slot;
		    }
		}
        #endregion

        #region EPActivityReminder

        private sealed class EPActivityReminder : IPrefetchable<TasksAndEventsReminder>, IExpires
        {
            #region IExpires
            private bool _dbChanged;
			public bool DBChanged 
			{ 
				get 
				{
					if (DateTime.UtcNow >= ExpirationTime)
						_dbChanged = true;
					return _dbChanged; 
				}
				set { _dbChanged = value; }
			}

			public DateTime ExpirationTime { get; set; }
			#endregion

			public static readonly EPActivityReminder Empty = new EPActivityReminder();

			PXResult<CRActivity>[] _reminderList = new PXResult<CRActivity>[0];
			public PXResult<CRActivity>[] ReminderList
			{
				get { return _reminderList; }
			}

			public EPActivityReminder()
			{
			    ExpirationTime = DateTime.UtcNow;
                ExpirationTime = ExpirationTime.AddSeconds(60-ExpirationTime.Second);
			}

            public void Prefetch(TasksAndEventsReminder graph)
            {
                graph.SlotView.View.Clear();
                _reminderList = graph.SlotView.Select().ToArray();                
			}

			public void Expire()
			{
				DBChanged = true;
				ExpirationTime = DateTime.MinValue;
			}

            public static EPActivityReminder GetFromSlot(string key, TasksAndEventsReminder graph)
            {
                EPActivityReminder slot = PXDatabase.GetSlot<EPActivityReminder, TasksAndEventsReminder>(key, graph, typeof(CRActivity), typeof(EPView), typeof(EPAttendee), typeof(CRReminder), typeof(UserPreferences));
                if (slot.DBChanged)
                {
                    PXContext.SetSlot<EPActivityReminder>(key, null);
                    PXDatabase.ResetSlot<EPActivityReminder>(key, typeof(CRActivity), typeof(EPView), typeof(EPAttendee), typeof(CRReminder), typeof(UserPreferences));
                    slot = PXDatabase.GetSlot<EPActivityReminder, TasksAndEventsReminder>(key, graph, typeof(CRActivity), typeof(EPView), typeof(EPAttendee), typeof(CRReminder), typeof(UserPreferences));
                }
                return slot;
            }
		}

		#endregion

		#region Wrapped Where

		public class WrappedWhere<TWhere> : IBqlWhere
			where TWhere : IBqlWhere, new()
		{
			private readonly TWhere _realWhere;

			public WrappedWhere()
			{
				_realWhere = new TWhere();
			}

			public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
				=> _realWhere.AppendExpression(ref exp, graph, info, selection);

			public virtual void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
			{
				_realWhere.Verify(cache, item, pars, ref result, ref value);
			}
		}

		#endregion

		#region DeferFilterInfo

		[Serializable]
		[PXHidden]
		public partial class DeferFilterInfo : IBqlTable
		{
			#region Type

			public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

			[PXInt]
			[PXDefault(5)]
			[PXIntList(new [] { 5, 10, 15, 30, 60, 120, 240, 720, 1440 },
				new [] { Messages.Min5, Messages.Min10, Messages.Min15, 
						Messages.Min30, Messages.Min60, Messages.Min120, 
						Messages.Min240, Messages.Min720, Messages.Min1440 })]
			public virtual Int32? Type { get; set; }

			#endregion
		}

		#endregion

		#endregion

		#region Constants

		private const string _REMINDER_SLOT_KEY_PREFIX = "ReminderStatistics@";
		private const string _ACTIVITY_SLOT_KEY_PREFIX = "ActivityStatistics@";
		private const string _REMINDERLIST_SLOT_KEY_PREFIX = "ReminderList@";

		#endregion

		#region Selects

		[PXHidden]
		public PXFilter<DeferFilterInfo>
			DeferFilter;

		public PXSelectReadonly<CRActivity>
			ReminderListCurrent;

		public PXSelectReadonly2<CRActivity,
            LeftJoin<EPAttendee,
				On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>, 
				And<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>>,
            LeftJoin<CRReminder, 
				On<CRReminder.refNoteID, Equal<CRActivity.noteID>, 
				And<CRReminder.owner, Equal<Current<AccessInfo.userID>>>>>>,
            Where2<Where<CRActivity.classID, Equal<CRActivityClass.task>,
                Or<CRActivity.classID, Equal<CRActivityClass.events>>>,
                    And2<Where<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
                           And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.completed>,
                           And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>,
					And<CRReminder.reminderDate, IsNotNull,
					And2<Where<CRReminder.reminderDate, Less<Now>, And<CRReminder.dismiss, NotEqual<True>>>,
					And<Where<CRReminder.owner, Equal<Current<AccessInfo.userID>>,
                        Or<EPAttendee.userID, IsNotNull>>>>>>>,
			OrderBy<Asc<CRReminder.reminderDate>>>
			ReminderList;

		public PXSelectJoin<CRReminder,
			InnerJoin<CRActivity, 
				On<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>,
			Where<CRReminder.refNoteID, Equal<CRActivity.noteID>,
				And<CRReminder.owner, Equal<Current<AccessInfo.userID>>>>>
			RemindInfo;

		public PXSelectReadonly<CRActivity,
            Where2<Where<CRActivity.classID, Equal<CRActivityClass.task>,
                Or<CRActivity.classID, Equal<CRActivityClass.events>,
				Or<CRActivity.classID, Equal<CRActivityClass.activity>,
				Or<CRActivity.classID, Equal<CRActivityClass.email>>>>>,
                    And<Where<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
                          And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>>,
			OrderBy<Asc<CRActivity.startDate>>>
			ActivityList;

		public PXSelectGroupBy<CRActivity,
            Where2<Where<CRActivity.classID, Equal<CRActivityClass.task>,
                Or<CRActivity.classID, Equal<CRActivityClass.events>,
				Or<CRActivity.classID, Equal<CRActivityClass.activity>,
				Or<CRActivity.classID, Equal<CRActivityClass.email>>>>>,
                  And<Where<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
                        And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>>,
			Aggregate<Count>>
			ActivityCount;

		public PXSelectReadonly2<CRActivity,
			LeftJoin<EPView,
				On<EPView.noteID, Equal<CRActivity.noteID>,
					And<EPView.userID, Equal<Current<AccessInfo.userID>>>>,
				LeftJoin<EPAttendee,
					On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>,
						And<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>>,
					LeftJoin<CRReminder,
						On<CRReminder.refNoteID, Equal<CRActivity.noteID>,
							And<CRReminder.owner, Equal<Current<AccessInfo.userID>>>>>>>,
			Where2<Where<CRActivity.classID, Equal<CRActivityClass.task>,
				Or<CRActivity.classID, Equal<CRActivityClass.events>>>,
				And2<Where<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
					And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.completed>,
						And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>,
					And<CRReminder.reminderDate, IsNotNull,
						And2<Where<CRReminder.reminderDate, Less<Now>, And<CRReminder.dismiss, NotEqual<True>>>,
							And<Where<CRReminder.owner, Equal<Current<AccessInfo.userID>>,
                                Or<EPAttendee.userID, IsNotNull>>>>>>>,
			OrderBy<Asc<CRReminder.reminderDate>>> SlotView;

        public PXSelect<PX.Data.EP.ActivityService.Total> Counters;

	    public virtual IEnumerable counters()
	    {            
	        ActivityStatistics.Source[] sources = new[]
	        {
	            new ActivityStatistics.Source("EP404000", null, Sprite.Main.Task),
	            new ActivityStatistics.Source("EP404100", "Events",  Sprite.Main.Event),
	            new ActivityStatistics.Source("CO409000", null,  Sprite.Main.MailReceive),
                new ActivityStatistics.Source("EP503010", null,  Sprite.Main.Roles, new[]{typeof(EPApproval),typeof(UserPreferences)}),
	        };
            var result = new List<PX.Data.EP.ActivityService.Total>();
                       
            foreach (var source in sources)
            {
                ActivityStatistics statistics = ActivityStatistics.GetFromSlot(source);
                if (statistics != null && statistics.Count > 0)
                {
                    var node = PXSiteMap.Provider.FindSiteMapNodeByScreenID(source.SrceenID);
                    if( node == null || PXAccess.VerifyRights(node.ScreenID) != true) continue;

                    var rec = new PX.Data.EP.ActivityService.Total();
                    rec.ScreenID = source.SrceenID;
                    rec.ImageKey = source.ImageKey;
                    rec.ImageSet = source.ImageSet;
                    rec.Url = node.Url;
                    rec.Title = node.Title;
                    rec.Count = statistics.Count;
                    rec.NewCount = statistics.UnreadCount;
                    yield return rec;
                }

            }
	    }

        [PXHidden]
		public PXSetup<EPSetup>
			Setup;
		
		#endregion

		#region Data Handlers

		public virtual IEnumerable reminderListCurrent([PXGuid] Guid? noteID)
		{
			if (noteID == null)
			{
				if (ReminderList.Current == null) yield break;

				noteID = ReminderList.Current.NoteID;
			}

			yield return (CRActivity)PXSelectReadonly<CRActivity>.Search<CRActivity.noteID>(this, noteID);
		}

		public virtual IEnumerable reminderList()
		{
			foreach (PXResult<CRActivity, EPView, EPAttendee> item in ActivityReminder.ReminderList)
			{
				var viewInfo = (EPView)item;
				var activity = (CRActivity)item;

				// TODO: not sure that it's usefull
				//if (viewInfo.Status != null && viewInfo.Status == EPViewStatusAttribute.VIEWED)
				//	activity.IsViewed = true;

				yield return new PXResult<CRActivity, EPAttendee>(activity, (EPAttendee)item);
			}
		}

		#endregion

		#region Actions

		public PXAction<CRActivity> navigate;
		[PXButton]
		public virtual IEnumerable Navigate(PXAdapter adapter)
		{
			var current = ReminderList.Current;
			NavigateToItem(current);
			return adapter.Get();
		}

		public virtual void NavigateToItem(CRActivity current)
					{
			PXRedirectHelper.TryRedirect(this, current, PXRedirectHelper.WindowMode.NewWindow);
		}

		public PXAction<CRActivity> viewActivity;
		[PXButton]
		[PXUIField(DisplayName = Messages.ViewDetails)]
		public virtual IEnumerable ViewActivity(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
			{
				PXRedirectHelper.TryOpenPopup(ReminderList.Cache, ReminderList.Current, "Open");
			}
			return adapter.Get();
		}

		public PXAction<CRActivity> dismiss;
		[PXButton]
		public virtual IEnumerable Dismiss(PXAdapter adapter)
		{
			var id = ExtractActivityID(adapter);
			if (id != null) UpdateAcitivtyRemindInfo(id, reminder => reminder.Dismiss = true);
			RefreshActivityReminder();
			return adapter.Get();
		}

		public PXAction<CRActivity> dismissAll;
		[PXButton(ClosePopup = true)]
		[PXUIField(DisplayName = Messages.DismissAll)]
		public virtual IEnumerable DismissAll(PXAdapter adapter)
		{
			foreach (CRActivity row in ReminderList.Select())
				UpdateAcitivtyRemindInfo(row.NoteID, reminder => reminder.Dismiss = true);
			RefreshActivityReminder();
			return adapter.Get();
		}

		public PXAction<CRActivity> dismissCurrent;
		[PXButton]
		[PXUIField(DisplayName = Messages.Dismiss)]
		public virtual IEnumerable DismissCurrent(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
				UpdateAcitivtyRemindInfo(ReminderList.Current.NoteID, reminder => reminder.Dismiss = true);
			RefreshActivityReminder();

            if (ReminderList.Select().Count == 0)
                throw new PXClosePopupException("");

			return adapter.Get();
		}

		public PXAction<CRActivity> defer;
		[PXButton]
		public virtual IEnumerable Defer(PXAdapter adapter)
		{
			var id = ExtractActivityID(adapter);
			if (id != null)
				UpdateAcitivtyRemindInfo(id,
					reminder =>
					{
						reminder.ReminderDate = PXTimeZoneInfo.Now.AddMinutes(Convert.ToInt32(adapter.Parameters[1]));
					});
			return adapter.Get();
		}

		public PXAction<CRActivity> deferCurrent;
		[PXButton]
		[PXUIField(DisplayName = Messages.Snooze)]
		public virtual IEnumerable DeferCurrent(PXAdapter adapter)
		{
			if (ReminderList.Current != null)
			{
				var minutes = DeferFilter.Current.Type == null ? 5 : (int)DeferFilter.Current.Type;
				UpdateAcitivtyRemindInfo(ReminderList.Current.NoteID,
					reminder =>
					{
						reminder.ReminderDate = PXTimeZoneInfo.Now.AddMinutes(minutes);
					});
				RefreshActivityReminder();
			}

            if (ReminderList.Select().Count == 0)
                throw new PXClosePopupException("");

			return adapter.Get();
		}

		public PXAction<CRActivity> completeRow;
		[PXButton(Tooltip = Messages.CompleteTooltipS)]
		[PXUIField(DisplayName = Messages.Complete)]
		public virtual IEnumerable CompleteRow(PXAdapter adapter)
		{
			var activity = ReadActivity(ExtractActivityID(adapter));
			var graphType = activity.With(_ => _.ClassID).With(_ => CRActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				graph.CompleteRow(activity);
			}
			adapter.Parameters = new object[0];
			return adapter.Get();
		}

		public PXAction<CRActivity> cancelRow;
		[PXButton(Tooltip = Messages.CancelTooltipS)]
		[PXUIField(DisplayName = Messages.Cancel)]
		public virtual IEnumerable CancelRow(PXAdapter adapter)
		{
			var activity = ReadActivity(ExtractActivityID(adapter));
			var graphType = activity.With(_ => _.ClassID).With(_ => CRActivityPrimaryGraphAttribute.GetGraphType(_.Value));
			if (graphType != null)
			{
				var graph = Activator.CreateInstance(graphType) as IActivityMaint;
				graph.CancelRow(activity);
			}
			adapter.Parameters = new object[0];
			return adapter.Get();
		}

		public PXAction<CRActivity> openInquiry;
		[PXButton(Tooltip = Messages.CancelTooltipS)]
		[PXUIField(DisplayName = Messages.Cancel)]
		public virtual IEnumerable OpenInquiry(PXAdapter adapter)
		{
			if (adapter.Parameters.Length > 0 && adapter.Parameters[0] != null)
			{
				var refNoteID = (Guid)adapter.Parameters[0];
				OpenInquiryScreen(refNoteID);
			}
			return adapter.Get();
		}

		public virtual void OpenInquiryScreen(Guid refNoteID)
		{
			if (refNoteID != Guid.Empty)
			{
				var gr = PXGraph.CreateInstance<ActivitiesMaint>();
				gr.Filter.Current.NoteID = refNoteID;
				PXRedirectHelper.TryRedirect(gr, PXRedirectHelper.WindowMode.NewWindow);
			}
		}

		#endregion

		#region Public Methods
		public virtual int GetListCount()
		{
            return (int)(PXSelectJoinGroupBy<CRActivity,
            LeftJoin<EPAttendee, 
				On<EPAttendee.userID, Equal<Current<AccessInfo.userID>>, 
				And<EPAttendee.eventNoteID, Equal<CRActivity.noteID>>>,
            LeftJoin<CRReminder, 
				On<CRReminder.refNoteID, Equal<CRActivity.noteID>, 
				And<CRReminder.owner, Equal<Current<AccessInfo.userID>>>>>>,
            Where2<Where<CRActivity.classID, Equal<CRActivityClass.task>,
                Or<CRActivity.classID, Equal<CRActivityClass.events>>>,
                    And2<Where<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.canceled>,
                           And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.completed>,
                           And<CRActivity.uistatus, NotEqual<ActivityStatusAttribute.released>>>>,
					And<CRReminder.reminderDate, IsNotNull,
					And2<Where<CRReminder.reminderDate, Less<Now>, And<CRReminder.dismiss, NotEqual<True>>>,
					And<Where<CRReminder.owner, Equal<Current<AccessInfo.userID>>,
                        Or<EPAttendee.userID, IsNotNull>>>>>>>,
            Aggregate<Count>>.Select(this).RowCount);
		}		
		#endregion

		#region Private Methods

		private EPSetup SetupCurrent
		{
			get
			{
				try
				{
					return Setup.Current;
				}
				catch (OutOfMemoryException) { }
				catch (OverflowException) { }
				catch (PXSetPropertyException) { }
				return null;
			}
		}

		private delegate void UpdateRemindInfo(CRReminder info);

		private void UpdateAcitivtyRemindInfo(Guid? id, UpdateRemindInfo handler)
		{
			CRReminder remindInfo = RemindInfo.Select(id);
			if (remindInfo == null && id is Guid)
			{
				remindInfo = (CRReminder)RemindInfo.Cache.Insert();
				remindInfo.RefNoteID = (Guid)id;
				remindInfo.Owner = PXAccess.GetUserID();
				remindInfo.ReminderDate = PXTimeZoneInfo.Now;
				RemindInfo.Cache.Normalize();
			}
			handler(remindInfo);
			RemindInfo.Update(remindInfo);
			using (var ts = new PXTransactionScope())
			{
				RemindInfo.Cache.Persist(PXDBOperation.Insert);
				RemindInfo.Cache.Persist(PXDBOperation.Update);
				ts.Complete(this);
			}
			RemindInfo.Cache.Persisted(false);
			ActivityList.Cache.Clear();
			ActivityList.View.Clear();
			ActivityCount.Cache.Clear();
			ActivityCount.View.Clear();
			ReminderList.Cache.Clear();
			ReminderList.View.Clear();
			ReminderListCurrent.View.Clear();
		}

		private CRActivity ReadActivity(object noteID)
		{
			if (noteID == null) return null;
			return PXSelect<CRActivity,
				Where<CRActivity.noteID, Equal<Required<CRActivity.noteID>>>>.
				Select(this, noteID);
		}

		private static Guid? ExtractActivityID(PXAdapter adapter)
		{
			if (adapter == null || adapter.Parameters == null ||
				adapter.Parameters.Length < 1)
			{
				return null;
			}

			var data = adapter.Parameters[0];
			var strData = data as string[];
			if (strData != null && strData.Length > 0) 
				return Guid.Parse(strData[0]);

			return Guid.Parse(data.ToString());
		}
		
		private EPActivityReminder ActivityReminder
		{
			get
			{
				string key = _REMINDERLIST_SLOT_KEY_PREFIX + PXAccess.GetUserID();
				return PXContext.GetSlot<EPActivityReminder>(key) ??
					   PXContext.SetSlot<EPActivityReminder>(key, EPActivityReminder.GetFromSlot(key, this) ?? EPActivityReminder.Empty);
			}
		}

		private void RefreshActivityReminder()
		{
			ActivityReminder.Expire();
			string key = _REMINDERLIST_SLOT_KEY_PREFIX + PXAccess.GetUserID();
			PXContext.SetSlot<EPActivityReminder>(key, null);
			PXDatabase.ResetSlot<EPActivityReminder>(key, typeof(CRActivity), typeof(EPView), typeof(EPAttendee), typeof(CRReminder), typeof(UserPreferences));
		}

		#endregion
	}
}
