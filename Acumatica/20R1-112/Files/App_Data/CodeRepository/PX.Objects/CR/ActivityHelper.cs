using System;
using System.Collections;
using PX.Data;
using PX.TM;

namespace PX.Objects.CR
{
	#region Attributes
	internal sealed class AddTaskPXUIFieldAttribute : PXUIFieldAttribute
	{
		public AddTaskPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.AddTask;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}
  
	internal sealed class AddEventPXUIFieldAttribute : PXUIFieldAttribute
	{
		public AddEventPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.AddEvent;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}

	internal sealed class LogEventPXUIFieldAttribute : PXUIFieldAttribute
	{
		public LogEventPXUIFieldAttribute()
			: base()
		{
            DisplayName = Messages.AddActivity;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}

  internal sealed class CompleteActivityPXUIFieldAttribute : PXUIFieldAttribute
	{
		public CompleteActivityPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.CompleteActivity;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}

	internal sealed class CancelActivityPXUIFieldAttribute : PXUIFieldAttribute
	{
		public CancelActivityPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.CancelActivity;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}

	internal sealed class ViewActivityPXUIFieldAttribute : PXUIFieldAttribute
	{
		public ViewActivityPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.ViewActivity;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}

	internal sealed class ViewCompletedActivityPXUIFieldAttribute : PXUIFieldAttribute
	{
		public ViewCompletedActivityPXUIFieldAttribute()
			: base()
		{
			DisplayName = Messages.ViewCompletedActivity;
			MapEnableRights = PXCacheRights.Select;
			MapViewRights = PXCacheRights.Select;
		}
	}
	#endregion
	internal class CRActivityHelper
	{
		#region Fields

		private readonly PXCache _cache;
		private readonly string _noteIdField;

		private readonly PXCache _complitedCache;
		private readonly PXCache _uncomplitedCache;

		#endregion

		#region Ctors
		public CRActivityHelper(PXCache cache, string noteIdField, PXCache complitedCache, PXCache uncomplitedCache)
		{
			if (cache == null) throw new ArgumentNullException("cache");
			if (string.IsNullOrEmpty(noteIdField)) throw new ArgumentNullException("noteIdField");

            CheckCache(complitedCache, "complitedCache");
            CheckCache(uncomplitedCache, "uncomplitedCache");

			_cache = cache;
			_noteIdField = noteIdField;

			_complitedCache = complitedCache;
			_uncomplitedCache = uncomplitedCache;
		}

    private static void CheckCache(PXCache cache, string name)
    {
        CRHelper.AssertInheritance(typeof(EPActivity), cache.GetItemType(), "cache.GetItemType()");
    }
		#endregion

		#region Button handlers

		#region Add new items
		public IEnumerable AddTask(PXAdapter adapter)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CRTaskActivityMaint graph = CreateSpecGraph<CRTaskActivityMaint>();
			graph.Tasks.Current = (EPTask)InsertActivity(graph.Tasks.Cache);

			throw new PXPopupRedirectException(true, graph, Messages.CRTaskMaint);
		}

		public IEnumerable AddEvent(PXAdapter adapter)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			EventMaintNotFiltered graph = CreateSpecGraph<EventMaintNotFiltered>();
			graph.Events.Current = (EPEvent)InsertActivity(graph.Events.Cache);

			throw new PXPopupRedirectException(true, graph, Messages.CREventMaint);
		}

		public IEnumerable AddActivity(PXAdapter adapter)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CRActivityMaint graph = CreateSpecGraph<CRActivityMaint>();
			CRActivity var1 = CreateActivity();
			graph.Activites.Current = graph.Activites.Insert(var1);
			throw new PXPopupRedirectException(true, graph, Messages.CRActivityMaint);
		}

		public IEnumerable AddCaseActivity(PXAdapter adapter, bool IsBillable)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CRCaseActivityMaint graph = CreateSpecGraph<CRCaseActivityMaint>();
			CRCaseActivity var1 = CreateSpecActivity<CRCaseActivity>();
			var1.IsBillable = IsBillable;
			graph.Activites.Current = graph.Activites.Insert(var1);
			throw new PXPopupRedirectException(true, graph, Messages.CRCaseActivityMaint);
		}

		public IEnumerable AddMailActivity(PXAdapter adapter, string emailAddress)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CRActivityMaint graph = CreateSpecGraph<CRActivityMaint>();
			CRActivity activity = CreateActivity();
			CRSetup setup = PXSelect<CRSetup>.Select(this.Graph);
			activity.Type = setup.EMailActivityType;
			activity.Status = ActivityStatus.NotStarted;
			graph.Activites.Current = graph.Activites.Insert(activity);
			PXAdapter mailCreate = new PXAdapter(graph.Activites);
			mailCreate.StartRow = 0;
			mailCreate.MaximumRows = 1;
			foreach (object e in graph.CreateMail.Press(mailCreate));
			if (!string.IsNullOrEmpty(emailAddress))
				graph.Message.Current.MailTo = emailAddress;

			throw new PXPopupRedirectException(true, graph, Messages.CRActivityMaint);
		}

		public IEnumerable AddCaseMailActivity(PXAdapter adapter, string emailAddress)
		{
			CRCaseActivityMaint graph = CreateSpecGraph<CRCaseActivityMaint>();
			CRCaseActivity activity = CreateSpecActivity<CRCaseActivity>();
			CRSetup setup = PXSelect<CRSetup>.Select(this.Graph);
			activity.Type = setup.EMailActivityType;
			activity.Status = ActivityStatus.NotStarted;
			graph.Activites.Current = graph.Activites.Insert(activity);
			PXAdapter mailCreate = new PXAdapter(graph.Activites);
			mailCreate.StartRow = 0;
			mailCreate.MaximumRows = 1;
			foreach (object e in graph.CreateMail.Press(mailCreate)) ;
			if (!string.IsNullOrEmpty(emailAddress))
				graph.Message.Current.MailTo = emailAddress;

			throw new PXPopupRedirectException(true, graph, Messages.CRCaseActivityMaint);
		}

		public IEnumerable AddCaseTask(PXAdapter adapter, bool billable)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CRTaskMaint graph = CreateSpecGraph<CRTaskMaint>();

			CRTask ñaseActivity = graph.Tasks.Current = (CRTask)InsertActivity(graph.Tasks.Cache);
			ñaseActivity.IsBillable = billable;
			ñaseActivity.IsLog = false;
			throw new PXPopupRedirectException(true, graph, Messages.CRCaseActivityMaint);
		}

		public IEnumerable AddCaseEvent(PXAdapter adapter, bool billable)
		{
			if (!IsCurrentRedy) return adapter.Get();
			PrepareGraph();

			CREventMaint graph = CreateSpecGraph<CREventMaint>();

			CREvent ñaseActivity = graph.Activites.Current = (CREvent)InsertActivity(graph.Activites.Cache);
			ñaseActivity.IsBillable = billable;
			ñaseActivity.IsLog = false;
			throw new PXPopupRedirectException(true, graph, Messages.CRCaseActivityMaint);
		}
		        
		#endregion

		#region View item details
		public IEnumerable ViewActivity(PXAdapter adapter)
		{
			return ViewActivity(adapter, _uncomplitedCache);
		}

		public IEnumerable ViewCompletedActivity(PXAdapter adapter)
		{
			return ViewActivity(adapter, _complitedCache);
		}

		public IEnumerable CancelActivity(PXAdapter adapter)
		{
			return TryChangeActivityType(adapter, _uncomplitedCache, ActivityStatus.Canceled);
		}

		public IEnumerable CompliteActivity(PXAdapter adapter)
		{
			return TryChangeActivityType(adapter, _uncomplitedCache, ActivityStatus.Completed);
		}

		public IEnumerable CompleteCaseActivity(PXAdapter adapter)
		{
			if (_uncomplitedCache.Current != null
				&& ((EPActivity)_uncomplitedCache.Current).Status != ActivityStatus.Completed
				&& ((EPActivity)_uncomplitedCache.Current).Status != ActivityStatus.Canceled
				&& ((EPActivity)_uncomplitedCache.Current).Status != ActivityStatus.Deferred)
			{
				if (((EPActivity)_uncomplitedCache.Current).ClassID == CRActivityClass.CRCaseTask)
				{
					CRTaskMaint maint = new CRTaskMaint();
					maint.Tasks.Current = maint.Tasks.Search<CRTask.taskID>(((EPActivity)_uncomplitedCache.Current).TaskID);
					maint.CompleteCurrentTask();
					maint.Persist();
				}
				else if (((EPActivity)_uncomplitedCache.Current).ClassID == CRActivityClass.CRCaseEvent)
				{
					CREventMaint maint = new CREventMaint();
					maint.Activites.Current = maint.Activites.Search<CREvent.taskID>(((EPActivity)_uncomplitedCache.Current).TaskID);
					maint.CompleteCurrentTask();
					maint.Persist();
				}

			}
			return adapter.Get();
		}
		#endregion
		#endregion

		#region Public Methods
		public void SetEnabled()
		{
			EP.EPActivityMaint.SetEnabled(_complitedCache);
			EP.EPActivityMaint.SetEnabled(_uncomplitedCache);
		}

		public CRActivity CreateActivity()
		{
			CRActivity addedActivity = CreateSpecActivity<CRActivity>();
			addedActivity.Status = ActivityStatus.Completed;
			return addedActivity;
		}
		private bool Delete(EPEvent current)
		{
			if (current == null) return false;
			PrepareGraph();
			EventMaintNotFiltered graph = CreateSpecGraph<EventMaintNotFiltered>();
			graph.Events.Current = current;			
			graph.Events.Delete(current);
			graph.Persist();
			return true;
		}
		private bool Delete(EPTask current)
		{
			if(current == null) return false;
			PrepareGraph();
			CRTaskActivityMaint graph = CreateSpecGraph<CRTaskActivityMaint>();
			graph.Tasks.Current = current;
			graph.Tasks.Delete(current);
			graph.Persist();
			return true;
		}
		
		public bool Delete(EPActivity current)
		{
			if(current == null) return false;

			switch (current.ClassID.Value)
			{
				case 1:
				case CRActivityClass.CRCaseEvent:
					return Delete(PXSelect<CREvent, Where<CREvent.taskID, Equal<Required<CREvent.taskID>>>>.Select(this.Graph, current.TaskID));					
				case 0:
				case CRActivityClass.CRCaseTask:
					return Delete(PXSelect<CRTask, Where<CRTask.taskID, Equal<Required<CRTask.taskID>>>>.Select(this.Graph, current.TaskID));					
				case CRActivityClass.CRActivity:
					break;				
				default:
					return false;
			}

			if (current.IsHold == false)			
				throw new PXException(Messages.ActivityNotOnHoldCannotBeDeleted);			
			PrepareGraph();
			CRActivityMaint graph = CreateSpecGraph<CRActivityMaint>();
			graph.Activites.Current = (CRActivity)current;
			if(!graph.Activites.Cache.AllowDelete)				
					throw new PXException(Messages.ActivityNotOnHoldCannotBeDeleted);			
			graph.Activites.Delete((CRActivity)current);
			graph.Persist();
			return true;		
		}
		#endregion

		#region Private Methods

		private static TGraph CreateSpecGraph<TGraph>()
			where TGraph : PXGraph, new()
		{
			TGraph graph = new TGraph();
			graph.Clear();
			return graph;
		}

		private TSpecActivity CreateSpecActivity<TSpecActivity>()
			where TSpecActivity : EPActivity, new()
		{
			TSpecActivity activity = new TSpecActivity();
			activity.RefNoteID = NoteID;
			return activity;
		}

		private EPActivity InsertActivity(PXCache cache)
		{
			CRHelper.AssertNull(cache, "cache");
			CRHelper.AssertInheritance(typeof(EPActivity), cache.GetItemType(), "cache.GetItemType()");

			EPActivity activity = (EPActivity)cache.Insert();
			activity.RefNoteID = NoteID;
			return activity;
		}

		private PXGraph Graph
		{
			get
			{
				return _cache.Graph;
			}
		}

		public long NoteID
		{
			get
			{
				if (PXNoteAttribute.GetNoteID(_cache, _cache.Current, _noteIdField) < 0L)
				{
					_cache.Update(_cache.Current);
					Graph.Persist();
				}

				return Convert.ToInt64(_cache.GetValue(_cache.Current, _noteIdField));
			}
		}

		private void PrepareGraph()
		{
			if (Graph.IsDirty) Graph.Persist();
		}

		private bool IsCurrentRedy
		{
			get
			{
				if (_cache.Current == null) return false;
				PXEntryStatus currentStatus = _cache.GetStatus(_cache.Current);
				if (currentStatus == PXEntryStatus.Inserted ||
					currentStatus == PXEntryStatus.InsertedDeleted) return false;
				return true;
			}
		}

		private static IEnumerable TryChangeActivityType(PXAdapter adapter, PXCache activitiesCache, int newStatus)
		{
			EPActivityGraphHelper.TryChangeType(activitiesCache, newStatus);
			return adapter.Get();
		}

		#region View item details
		private static IEnumerable ViewActivity(PXAdapter adapter, PXCache activitiesCache)
		{
			EPActivity currentActivity;
			if (activitiesCache != null && (currentActivity = activitiesCache.Current as EPActivity) != null)
				switch (CRActivityClass.GetActivityType(currentActivity))
				{
					case CRActivityClass.Task: ViewTask(currentActivity); break;
					case CRActivityClass.Event: ViewEvent(currentActivity); break;
					case CRActivityClass.CRActivity: ViewCRActivity(currentActivity); break;
					case CRActivityClass.CRCaseTask: ViewCRCaseTask(currentActivity); break;
					case CRActivityClass.CRCaseEvent: ViewCRCaseEvent(currentActivity); break;
					case CRActivityClass.CRCaseActivity: ViewCRCaseActivity(currentActivity); break;
				}
			return adapter.Get();
		}

		private static void ViewActivity<TCurrentActivity, TSearchTaskID>(PXCache cache, EPActivity currentActivity, string message)
			where TCurrentActivity : EPActivity, new()
			where TSearchTaskID : IBqlField
		{
			TCurrentActivity task = PXSelect<TCurrentActivity>.Search<TSearchTaskID>(cache.Graph, currentActivity.TaskID);
			if (task != null)
			{
				cache.Current = task;
				throw new PXPopupRedirectException(true, cache.Graph, message);
			}
		}

		private static void ViewCRCaseTask(EPActivity currentActivity)
		{
			ViewActivity<CRTask, CRTask.taskID>(
				CreateSpecGraph<CRTaskMaint>().Tasks.Cache,
					currentActivity, Messages.CRCaseTaskMaint);
		}

		private static void ViewCRCaseEvent(EPActivity currentActivity)
		{
			ViewActivity<CREvent, CREvent.taskID>(
				CreateSpecGraph<CREventMaint>().Activites.Cache,
					currentActivity, Messages.CREventMaint);
		}

		private static void ViewCRCaseActivity(EPActivity currentActivity)
		{
			ViewActivity<CRCaseActivity, CRCaseActivity.taskID>(
				CreateSpecGraph<CRCaseActivityMaint>().Activites.Cache,
					currentActivity, Messages.CRCaseActivityMaint);
		}

		private static void ViewCRActivity(EPActivity currentActivity)
		{
			ViewActivity<CRActivity, CRActivity.taskID>(
				CreateSpecGraph<CRActivityMaint>().Activites.Cache,
					currentActivity, Messages.CRActivityMaint);
		}

		private static void ViewEvent(EPActivity currentActivity)
		{
			ViewActivity<EPEvent, EPEvent.taskID>(
				CreateSpecGraph<EventMaintNotFiltered>().Events.Cache,
					currentActivity, Messages.CREventMaint);
		}

		private static void ViewTask(EPActivity currentActivity)
		{
			ViewActivity<EPTask, EPTask.taskID>(
				CreateSpecGraph<CRTaskActivityMaint>().Tasks.Cache,
					currentActivity, Messages.CRTaskMaint);
		}
		#endregion

		#endregion
	}
}
