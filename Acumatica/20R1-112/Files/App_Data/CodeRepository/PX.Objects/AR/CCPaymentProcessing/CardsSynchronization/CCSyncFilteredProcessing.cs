using System;
using System.Collections.Generic;
using System.Collections;
using PX.SM;
using PX.Web.UI;
using PX.Data;
using PX.Common;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	///<summary>
	///Class allows to add schedule history row during credit cards synchronization. 
	///These rows are inserted into AUScheduleHistory table. This class adds hook methods on button "Add Schedule"
	///</summary>
	public class CCSyncFilteredProcessing<Table, FilterTable, Where, OrderBy> : PXFilteredProcessing<Table, FilterTable, Where, OrderBy> 
		where FilterTable : class, IBqlTable, new()
		where Table : class, IBqlTable, new()
		where Where : IBqlWhere, new()
		where OrderBy : IBqlOrderBy, new()
	{
		Action beforeScheduleAdd;
		Action afterScheduleAdd;
		Action beforeScheduleProcessAll;

		public CCSyncFilteredProcessing()
		{

		}

		public CCSyncFilteredProcessing(PXGraph graph)
			: base(graph, null)
		{

		}

		public CCSyncFilteredProcessing(PXGraph graph, Delegate handler)
			: base(graph,handler)
		{

		}

		[PXButton(ImageKey = Sprite.Main.AddNew)]
		[PXUIField(DisplayName = "Add", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Visible = false)]
		protected override IEnumerable _ScheduleAdd_(PXAdapter adapter)
		{
			beforeScheduleAdd?.Invoke();
			IEnumerable ret = base._ScheduleAdd_(adapter);
			afterScheduleAdd?.Invoke();
			return ret;
		}

		public void SetBeforeScheduleProcessAllAction(Action beforeAction)
		{
			this.beforeScheduleProcessAll = beforeAction;
		}

		public void SetBeforeScheduleAddAction(Action beforeScheduleAdd)
		{
			this.beforeScheduleAdd = beforeScheduleAdd;
		}

		public void SetAfterScheduleAddAction(Action afterScheduleAdd)
		{
			this.afterScheduleAdd = afterScheduleAdd;
		}
		 
		protected override bool startPendingProcess(List<Table> items)
		{
			AUSchedule schedule = PX.Common.PXContext.GetSlot<AUSchedule>();

			if (schedule == null)
			{
				return base.startPendingProcess(items);
			}
			
			PXCache cache = _OuterView.Cache;
			cache.IsDirty = false;
			List<Table> list = new List<Table>();

			foreach (Table item in items)
			{
				object sel = cache.GetValue(item, _SelectedField);

				if (sel != null && Convert.ToBoolean(sel))
				{
					PXEntryStatus status = cache.GetStatus(item);

					if (status == PXEntryStatus.Inserted
						|| status == PXEntryStatus.Updated)
					{
						list.Add(item);
					}
				}
			}

			PX.Common.PXContext.SetSlot<AUSchedule>(null);
			AUSchedule scheduleparam = schedule;

			if (_IsInstance)
			{
				ProcessSyncCC(_ProcessDelegate, list, scheduleparam);
			}
			else
			{
				PXLongOperation.StartOperation(_Graph, delegate () { ProcessSyncCC(_ProcessDelegate, list, scheduleparam); });
			}

			schedule = null;
			return true;
		}

		protected void ProcessSyncCC(ProcessListDelegate processor, List<Table> list, AUSchedule schedule)
		{
			beforeScheduleProcessAll?.Invoke();
			PXLongOperation.SetCustomInfo(new List<SyncCCProcessingInfoEntry>(), ProcessingInfo.processingKey);
			list.Clear();
			_InProc = new PXResultset<Table>();
			base._ProcessScheduled(processor,list,schedule);
			var histCache = _Graph.Caches[typeof(AUScheduleHistory)];
			List<SyncCCProcessingInfoEntry> infoList = PXLongOperation.GetCustomInfoForCurrentThread(ProcessingInfo.processingKey) as List<SyncCCProcessingInfoEntry>;

			if (infoList != null)
			{
				foreach (SyncCCProcessingInfoEntry infoEntry in infoList)
				{
					AUScheduleHistory hist = new PX.SM.AUScheduleHistory();
					hist.ExecutionResult = infoEntry.ProcessingMessage.Message;
					hist.ErrorLevel = (short)infoEntry.ProcessingMessage.ErrorLevel;
					hist.ScheduleID = schedule.ScheduleID;
					hist.ScreenID = schedule.ScreenID;
					var timeZone = PXTimeZoneInfo.FindSystemTimeZoneById(schedule.TimeZoneID);
					DateTime startUtc = PXTimeZoneInfo.UtcNow;
					DateTime start = PXTimeZoneInfo.ConvertTimeFromUtc(startUtc, timeZone);
					hist.ExecutionDate = start;
					hist.RefNoteID = infoEntry.NoteId;
					histCache.Insert(hist);
				}
			}

			histCache.Persist(PXDBOperation.Insert);
		}
	}
}
