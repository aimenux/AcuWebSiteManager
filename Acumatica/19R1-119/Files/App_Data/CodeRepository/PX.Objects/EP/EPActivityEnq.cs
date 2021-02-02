using System.Collections;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.TM;
using PX.Objects.CR;
using PX.SM;

namespace PX.Objects.EP
{
	[PXGraphName(Messages.EPActivityInq, typeof(EPActivityFilter), typeof(EPActivity))]
	public class ActivityEnq: PXGraph<ActivityEnq>		
	{
		public PXFilter<EPActivityFilter> SelectionFilter;

		[PXFilterable]
		public PXSelectRedirect<CRActivity,
			Where<CRActivity.refNoteID, Equal<Current<EPActivityFilter.refNoteID>>>, EPActivityFilter> 
			Tasks;

		[PXFilterable]
		public PXSelectRedirect<CRActivity, 
			Where<CRActivity.refNoteID, Equal<Current<EPActivityFilter.refNoteID>>>, EPActivityFilter> 
			Activities;


		#region Buttons						
		public PXAction<EPActivityFilter> CompleteActivity;
		[PXUIField(DisplayName = PX.TM.Messages.CompleteEvent, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageUrl = "~/Icons/Complete_Active.gif", DisabledImageUrl = "~/Icons/Complete_NotActive.gif", Tooltip = PX.TM.Messages.CompleteEvent)]
		protected virtual IEnumerable completeActivity(PXAdapter adapter)
		{
			if (Activities.Current != null && 
				Activities.Current.Status != ActivityStatusListAttribute.Completed &&
				Activities.Current.Status != ActivityStatusListAttribute.Canceled)
			{
				ActivityEnq.TryChangeType(Activities.Cache, ActivityStatusListAttribute.Completed);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> CancelActivity;
		[PXUIField(DisplayName = PX.TM.Messages.CancelEvent, MapEnableRights = PXCacheRights.Update)]
		[PXButton(ImageUrl = "~/Icons/Cancel_Active.gif", DisabledImageUrl = "~/Icons/Cancel_NotActive.gif", Tooltip = PX.TM.Messages.CancelTask)]
		protected virtual IEnumerable cancelActivity(PXAdapter adapter)
		{
			if (Activities.Current != null &&
				Activities.Current.Status != ActivityStatusListAttribute.Completed &&
				Activities.Current.Status != ActivityStatusListAttribute.Canceled)
			{
				ActivityEnq.TryChangeType(Activities.Cache, ActivityStatusListAttribute.Canceled);
			}
			return adapter.Get();
		}		

		public PXAction<EPActivityFilter> CompleteTask;
		[PXUIField(DisplayName = PX.TM.Messages.CompleteEvent, MapEnableRights = PXCacheRights.Update)]
		//TODO: Change DisabledImageUrl
		[PXButton(ImageUrl = "PX.Data.Images.Activity.CompleteTask.gif", DisabledImageUrl = "PX.Data.Images.Activity.CompleteTask.gif", Tooltip = PX.TM.Messages.CompleteTask)]		
		protected virtual IEnumerable completeTask(PXAdapter adapter)
		{
			if (Tasks.Current != null &&
				Tasks.Current.Status != ActivityStatusListAttribute.Completed &&
				Tasks.Current.Status != ActivityStatusListAttribute.Canceled)
			{
				ActivityEnq.TryChangeType(Tasks.Cache, ActivityStatusListAttribute.Completed);
			}
			return adapter.Get();
		}

		public PXAction<EPActivityFilter> CancelTask;
		[PXUIField(DisplayName = PX.TM.Messages.CancelEvent, MapEnableRights = PXCacheRights.Update)]
		//TODO: Change DisabledImageUrl
		[PXButton(ImageUrl = "PX.Data.Images.Activity.CancelTask.gif", DisabledImageUrl = "PX.Data.Images.Activity.CancelTask.gif", Tooltip = PX.TM.Messages.CancelTask)]		
		protected virtual IEnumerable cancelTask(PXAdapter adapter)
		{
			if (Tasks.Current != null &&
				Tasks.Current.Status != ActivityStatusListAttribute.Completed &&
				Tasks.Current.Status != ActivityStatusListAttribute.Canceled)
			{
				ActivityEnq.TryChangeType(Tasks.Cache, ActivityStatusListAttribute.Canceled);
			}
			return adapter.Get();
		}

		public static void TryChangeType(PXCache cache, int newStatus)
		{
			if (cache.Current != null)
			{
				EPActivity activity = (EPActivity)cache.Current;
				if (activity != null)
				{
					activity.Status = newStatus;
					switch (activity.Status)
					{
						case ActivityStatusListAttribute.Completed:
							activity.EndDate = PXTimeZoneInfo.Now;
							activity.PercentCompletion = 100;
							break;
						case ActivityStatusListAttribute.Canceled:
							activity.PercentCompletion = 0;
							break;
					}
					cache.Update(activity);
					cache.Graph.Persist();
				}
			}
		}
		#endregion			

		#region Events
		protected virtual void CRActivity_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = SelectionFilter.Current != null ? SelectionFilter.Current.RefNoteID : null;
			e.Cancel = true;
		}
		protected virtual void CRActivity_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CRActivity row = (CRActivity)e.Row;
			if (row != null)
			{
				var isEditable = row.Status != ActivityStatusListAttribute.Completed &&
					row.Status != ActivityStatusListAttribute.Canceled;
				this.CompleteActivity.SetEnabled(isEditable);
				this.CancelActivity.SetEnabled(isEditable);
			}
		}

		protected virtual void EPGenericTask_RefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = SelectionFilter.Current != null ? SelectionFilter.Current.RefNoteID : null;
			e.Cancel = true;
		}
		protected virtual void EPGenericTask_ParentRefNoteID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = SelectionFilter.Current != null ? SelectionFilter.Current.ParentRefNoteID : null;
			e.Cancel = true;
		}

		#endregion
	}
}
