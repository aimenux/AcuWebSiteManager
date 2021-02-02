using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.EP;
using PX.SM;

namespace PX.Objects.CR
{
	public class CRTaskMaint : CRBaseActivityMaint<CRTaskMaint, CRActivity>
	{
		#region Extensions

		public class EmbeddedImagesExtractor : EmbeddedImagesExtractorExtension<CRTaskMaint, CRActivity, CRActivity.body>
		{
		}

		#endregion

		#region Selects

		[PXHidden]
		public PXSelect<CT.Contract>
				BaseContract;

		[PXRefNoteSelector(typeof(CRActivity), typeof(CRActivity.refNoteID))]
		[PXCopyPasteHiddenFields(typeof(CRActivity.body))]
		public PXSelect<CRActivity,
			Where<CRActivity.classID, Equal<CRActivityClass.task>>>
			Tasks;

		[PXHidden]
		public PXSelect<PMTimeActivity>
			TimeActivitiesOld;

		public PMTimeActivityList<CRActivity>
			TimeActivity;

		[PXFilterable]
		[CRReference(typeof(CRActivity.bAccountID), typeof(CRActivity.contactID))]
		[PXViewDetailsButton(typeof(CRActivity), OnClosingPopup = PXSpecialButtonType.Refresh)]
		public CRChildActivityList<CRActivity>
			ChildActivities;

		[PXFilterable]
		public CRReferencedTaskList<CRActivity>
			ReferencedTasks;

		public CRReminderList<CRActivity>
			Reminder;

		#endregion

		#region Ctors

		public CRTaskMaint()
			: base()
		{
			ChildActivities.Cache.AllowUpdate = false;

			this.EnsureCachePersistence(ChildActivities.Cache.GetItemType());
			var view = this.ReferencedTasks.View;
			PXDBDateAndTimeAttribute.SetDateDisplayName<CRActivity.startDate>(Tasks.Cache, null, Messages.StartDate);
			PXDBDateAndTimeAttribute.SetDateDisplayName<CRActivity.endDate>(Tasks.Cache, null, Messages.DueDate);

			PXView relEntityView = new PXView(this, true, new Select<CRSMEmail>(), new PXSelectDelegate(GetRelatedEntity));
			Views.Add("RelatedEntity", relEntityView);
			ActivityStatusAttribute.SetRestictedMode<CRActivity.uistatus>(Tasks.Cache, true);
		}

		#endregion

		#region Actions

		public PXDelete<CRActivity> Delete;

		public PXAction<CRActivity> Complete;
		[PXUIField(DisplayName = TM.Messages.CompleteTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskTooltip,
			ShortcutCtrl = true, ShortcutChar = (char)75)] //Ctrl + K
		protected virtual void complete()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);
		}

		public PXAction<CRActivity> CompleteAndFollowUp;
		[PXUIField(DisplayName = Messages.CompleteTaskAndFollowUp, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = Messages.CompleteTaskAndFollowUpTooltip,
			ShortcutCtrl = true, ShortcutShift = true, ShortcutChar = (char)75)] //Ctrl + Shift + K
		protected virtual void completeAndFollowUp()
		{
			CRActivity row = Tasks.Current;
			if (row == null) return;

			CompleteTask(row);

			CRTaskMaint graph = CreateInstance<CRTaskMaint>();

			CRActivity followUpTask = (CRActivity)graph.Tasks.Cache.CreateCopy(row);
			followUpTask.NoteID = null;
			followUpTask.ParentNoteID = row.ParentNoteID;
			followUpTask.UIStatus = null;
			followUpTask.PercentCompletion = null;

			followUpTask = (CRActivity)graph.Tasks.Cache.Insert(followUpTask);

			CRReminder oldReminder = Reminder.Current;
			CRReminder newReminder = graph.Reminder.Current;
			if (oldReminder != null && newReminder != null)
			{
				newReminder.ReminderDate = oldReminder.ReminderDate;
				newReminder.RefNoteID = followUpTask.NoteID;

				graph.Reminder.Cache.Update(newReminder);
			}

			if (!this.IsContractBasedAPI)
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.NewWindow);

			graph.Save.Press();
		}

		public PXAction<CRActivity> CancelActivity;
		[PXUIField(DisplayName = TM.Messages.CancelTask, MapEnableRights = PXCacheRights.Select)]
		[PXButton(Tooltip = TM.Messages.CancelTask)]
		protected virtual void cancelActivity()
		{
			var row = Tasks.Current;
			if (row == null) return;

			CancelTask(row);
		}

		public PXAction<CRActivity> AddNewRelatedTask;
		[PXUIField(DisplayName = "Add Related Task", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual void addNewRelatedTask()
		{
			CRTaskMaint graph = CreateInstance<CRTaskMaint>();
			var task = graph.Tasks.Insert();
			task.ParentNoteID = Tasks.Current.NoteID;
			CRActivityRelation rel = (CRActivityRelation)graph.ReferencedTasks.Cache.Insert();
			rel.ParentNoteID = Tasks.Current.NoteID;
			rel.RefNoteID = task.NoteID;
			graph.ReferencedTasks.Cache.Current = rel;
			Tasks.Cache.ClearQueryCacheObsolete();
			PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Popup);
		}

		#endregion

		#region Event Handlers

		[TaskStatus]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_UIStatus_CacheAttached(PXCache cache) { }

		[EPStartDate(DisplayName = "Start Date", DisplayNameDate = "Date", DisplayNameTime = "Start Time", PreserveTime = false, BqlField = typeof(CRActivity.startDate), AllDayField = typeof(CRActivity.allDay))]
		[PXMergeAttributes(Method = MergeMethod.Replace)]
		protected virtual void CRActivity_StartDate_CacheAttached(PXCache sender) { }

		[EPEndDate(typeof(CRActivity.classID), typeof(CRActivity.startDate), PreserveTime = false, BqlField = typeof(CRActivity.endDate), AllDayField = typeof(CRActivity.allDay))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_EndDate_CacheAttached(PXCache sender) { }

		[PXUIField(DisplayName = "Track Time", Visible = false)]
		[PXDefault(false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_TrackTime_CacheAttached(PXCache cache) { }

		[PXUIField(DisplayName = "Billable", Visible = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_IsBillable_CacheAttached(PXCache cache) { }


		[PXFormula(typeof(False))]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void PMTimeActivity_NeedToBeDeleted_CacheAttached(PXCache cache) { }

		[PXDefault(CRActivityClass.Task)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ClassID_CacheAttached(PXCache cache) { }


		protected virtual void _(Events.FieldUpdated<CRActivity, CRActivity.uistatus> e)
		{
			PMTimeActivity timeActivity = (PMTimeActivity)TimeActivity.SelectSingle();
			if (timeActivity != null)
			{
				Caches[typeof(PMTimeActivity)].MarkUpdated(timeActivity);//For Persisting event handler to sync Status.
			}
		}

		protected virtual void CRActivity_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			var row = (CRActivity)e.Row;
			if (row == null) return;
			
			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
			}		
		}
		
		protected virtual void CRActivity_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			var row = e.Row as CRActivity;
			var oldRow = (CRActivity)e.OldRow;
			if (row == null || oldRow == null) return;
			
			if (row.UIStatus == ActivityStatusListAttribute.Completed)
			{
				row.PercentCompletion = 100;
				if (!object.Equals(sender.GetValueOriginal<CRActivity.uistatus>(row), ActivityStatusListAttribute.Completed))
					row.CompletedDate = PXTimeZoneInfo.Now;
			}			
		}

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
			var row = e.Row as CRActivity;
			if (row == null) return;

			string status = ((string) cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open);
			bool editable = status == ActivityStatusListAttribute.Open || status == ActivityStatusListAttribute.Draft || status == ActivityStatusListAttribute.InProcess;
			bool deleteable = ChildActivities.SelectByParentNoteID(row.NoteID).RowCast<CRPMTimeActivity>().All(o => o.IsBillable != true);

			PXUIFieldAttribute.SetEnabled(cache, row, editable);
			Delete.SetEnabled(deleteable);
			Complete.SetEnabled(editable);			
			CompleteAndFollowUp.SetEnabled(editable);
			CancelActivity.SetEnabled(editable);
			AddNewRelatedTask.SetEnabled(cache.GetStatus(row) != PXEntryStatus.Inserted);

			PXUIFieldAttribute.SetEnabled<CRActivity.noteID>(cache, row);
			PXUIFieldAttribute.SetEnabled<CRActivity.uistatus>(cache, row);
            PXUIFieldAttribute.SetEnabled<CRActivity.source>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.createdByID>(cache, row, false);
			PXUIFieldAttribute.SetEnabled<CRActivity.completedDate>(cache, row, false);
			
			GotoParentActivity.SetEnabled(row.ParentNoteID != null);

			ChildActivities.Cache.AllowDelete = 
			ReferencedTasks.Cache.AllowInsert =
			ReferencedTasks.Cache.AllowUpdate =
            ReferencedTasks.Cache.AllowDelete = editable;


			var tAct = (PMTimeActivity)TimeActivity.SelectSingle();
			var tActCache = TimeActivity.Cache;

			PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectID>(tActCache, tAct, editable);
			PXUIFieldAttribute.SetEnabled<PMTimeActivity.projectTaskID>(tActCache, tAct, editable);
			
			PXUIFieldAttribute.SetEnabled<CRReminder.isReminderOn>(Reminder.Cache, Reminder.SelectSingle(), editable);
			PXUIFieldAttribute.SetEnabled<CRActivity.parentNoteID>(cache, row, false);

			MarkAs(cache, row, Accessinfo.UserID, EPViewStatusAttribute.VIEWED);
		}
		
		protected virtual void CRActivity_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			var row = e.Row as CRActivity;
			if (row == null) return;
			
			if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update) && row.OwnerID == null && row.WorkgroupID == null)
			{
				var displayName = PXUIFieldAttribute.GetDisplayName<CRActivity.ownerID>(Tasks.Cache);
				var exception = new PXSetPropertyException(ErrorMessages.FieldIsEmpty, displayName);
				if (Tasks.Cache.RaiseExceptionHandling<CRActivity.ownerID>(row, null, exception))
				{
					throw new PXRowPersistingException(typeof(CRActivity.ownerID).Name, null, ErrorMessages.FieldIsEmpty, displayName);
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

		[PXUIField(DisplayName = "Parent", Enabled = false)]
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		protected virtual void CRActivity_ParentNoteID_CacheAttached(PXCache cache) { }

		#endregion

		#region Data Handlers

		public IEnumerable GetRelatedEntity()
		{
			var current = Tasks.Current;
			if (current != null && current.RefNoteID != null)
			{
				var row = new EntityHelper(this).GetEntityRow(current.RefNoteID);
				if (row != null) yield return row;
			}
		}

		#endregion

		#region Private Methods

		private void CompleteTask(CRActivity row)
		{
			string origStatus = (string)Tasks.Cache.GetValueOriginal<CRActivity.uistatus>(row) ?? ActivityStatusListAttribute.Open;			
			if (origStatus == ActivityStatusListAttribute.Completed ||
					origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}
						
			CRActivity activityCopy = (CRActivity)Tasks.Cache.CreateCopy(row);
			activityCopy.UIStatus = ActivityStatusListAttribute.Completed;			
			Tasks.Cache.Update(activityCopy);
			Actions.PressSave();
		}

		private void CancelTask(CRActivity row)
		{
			string origStatus = (string)Tasks.Cache.GetValueOriginal<CRActivity.uistatus>((CRActivity)row) ?? ActivityStatusListAttribute.Open;
			if (origStatus == ActivityStatusListAttribute.Completed ||
					origStatus == ActivityStatusListAttribute.Canceled)
			{
				return;
			}


			CRActivity activityCopy = (CRActivity)Tasks.Cache.CreateCopy((CRActivity)row);			
			activityCopy.UIStatus = ActivityStatusListAttribute.Canceled;
			Tasks.Cache.Update(activityCopy);
			Actions.PressSave();
		}
		
		#endregion

		#region Public Methods

		public override void CompleteRow(CRActivity row)
		{
			if (row != null) CompleteTask(row);
		}

		public override void CancelRow(CRActivity row)
		{
			if (row != null) CancelTask(row);
		}

		#endregion
	}
}
