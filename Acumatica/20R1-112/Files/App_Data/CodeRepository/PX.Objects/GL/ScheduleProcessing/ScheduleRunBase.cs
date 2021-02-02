using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

using PX.Objects.Common;
using PX.Objects.Common.Extensions;

namespace PX.Objects.GL
{
	public class ScheduleRunBase
	{
		public static void SetProcessDelegate<ProcessGraph>(PXGraph graph, ScheduleRun.Parameters filter, PXProcessing<Schedule> view)
			where ProcessGraph : PXGraph<ProcessGraph>, IScheduleProcessing, new()
		{
			if (filter == null) return;

			short times = filter.LimitTypeSel == ScheduleRunLimitType.StopAfterNumberOfExecutions
				? filter.RunLimit ?? 1
				: short.MaxValue;

			DateTime executionDate = filter.LimitTypeSel == ScheduleRunLimitType.StopOnExecutionDate
				? filter.ExecutionDate ?? graph.Accessinfo.BusinessDate.Value
				: DateTime.MaxValue;

			Dictionary<string, string> parametersErrors = PXUIFieldAttribute.GetErrors(
				graph.Caches[typeof(ScheduleRun.Parameters)],
				filter);

			view.SetProcessDelegate(schedules =>
			{
				if (parametersErrors.Any())
				{
					throw new PXException(parametersErrors.First().Value);
				}

				ProcessGraph processGraph = PXGraph.CreateInstance<ProcessGraph>();
				bool failed = false;
				foreach (Schedule schedule in schedules)
				{
					try
					{
						PXProcessing<Schedule>.SetCurrentItem(schedule);
						processGraph.Clear();
						processGraph.GenerateProc(schedule, times, executionDate);
						PXProcessing<Schedule>.SetInfo(ActionsMessages.RecordProcessed);
					}
					catch (Exception e)
					{
						failed = true;
						PXProcessing<Schedule>.SetError(e);
					}
				}
				if (failed)
				{
					//Clean current to prevent set exception to the last item
					PXProcessing<Schedule>.SetCurrentItem(null);
					throw new PXException(AR.Messages.OneOrMoreItemsAreNotProcessed);
				}
			});
		}
	}

	/// <summary>
	/// Encapsulates the common logic for generating transactions according to
	/// a schedule. Module-specific schedule running graphs derive from it.
	/// </summary>
	/// <typeparam name="TGraph">The specific graph type.</typeparam>
	/// <typeparam name="TMaintenanceGraph">The type of the graph used to create and edit schedules for relevant entity type.</typeparam>
	/// <typeparam name="TProcessGraph">The type of the graph used to run schedules for relevant entity type.</typeparam>
	public class ScheduleRunBase<TGraph, TMaintenanceGraph, TProcessGraph> : PXGraph<TGraph>
		where TGraph : PXGraph
		where TMaintenanceGraph : ScheduleMaintBase<TMaintenanceGraph, TProcessGraph>, new()
		where TProcessGraph : PXGraph<TProcessGraph>, IScheduleProcessing, new()
	{
		public PXFilter<ScheduleRun.Parameters> Filter;
		public PXCancel<ScheduleRun.Parameters> Cancel;

		[PXFilterable]
		public PXFilteredProcessing<
			Schedule,
			ScheduleRun.Parameters,
			Where<
				Schedule.active, Equal<True>,
				And<Schedule.nextRunDate, LessEqual<Current<ScheduleRun.Parameters.executionDate>>>>>
			Schedule_List;

		protected virtual bool checkAnyScheduleDetails => true; // to compatable with children of ScheduleRunBase which need AnyScheduleDetails in schedule_List

		protected virtual IEnumerable schedule_List()
		{
			IEnumerable<Schedule> schedules = new PXView(this, false, Schedule_List.View.BqlSelect)
				.QuickSelect()
				.RowCast<Schedule>();

			if (checkAnyScheduleDetails)
			{
			// Checking for the presence of schedule details is delegated to
			// the concrete maintenance graph.
			// -
			ScheduleMaintBase<TMaintenanceGraph, TProcessGraph> maintenanceGraph =
				PXGraph.CreateInstance<TMaintenanceGraph>();

			foreach (Schedule schedule in schedules)
			{
				maintenanceGraph.Schedule_Header.Current = schedule;

				if (maintenanceGraph.AnyScheduleDetails())
						yield return (Schedule)schedule;
				}
			}
			else
				{
				foreach (Schedule schedule in schedules)
					yield return schedule;
				}
			}

		public ScheduleRunBase()
		{
			Schedule_List.SetProcessCaption(Messages.ProcRunSelected);
			Schedule_List.SetProcessAllCaption(Messages.ProcRunAll);
		}

		public override void Clear(PXClearOption option)
		{
			if (this.Caches.ContainsKey(typeof(Schedule)))
			{
				this.Caches[typeof(Schedule)].ClearQueryCache();
			}

			base.Clear(option);
		}

		protected IEnumerable ViewScheduleAction(PXAdapter adapter)
		{
			if (Schedule_List.Current == null) return adapter.Get();

			PXRedirectHelper.TryRedirect(
				Schedule_List.Cache,
				Schedule_List.Current,
				nameof(Schedule),
				PXRedirectHelper.WindowMode.NewWindow);

			return adapter.Get();
		}

		#region Cache Attached
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = Common.Messages.ScheduleID, Visibility = PXUIVisibility.Visible, Enabled = false)]
		protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
		{
		}
		#endregion

		#region Event Handlers
		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			Schedule task = (Schedule)e.Row;

			if (task == null || PXLongOperation.Exists(UID)) return;

			if ((task.NoRunLimit ?? false) == false && task.RunCntr == task.RunLimit)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleExecutionLimitExceeded, PXErrorLevel.RowError));
			}
			else if (task.NextRunDate > Accessinfo.BusinessDate)
			{
				cache.RaiseExceptionHandling<Schedule.scheduleID>(e.Row, task.ScheduleID, new PXSetPropertyException(Messages.SheduleNextExecutionDateExceeded, PXErrorLevel.RowWarning));
			}
		}

		protected virtual void Parameters_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ScheduleRun.Parameters parameters = e.Row as ScheduleRun.Parameters;

			PXUIFieldAttribute.SetRequired<ScheduleRun.Parameters.executionDate>(
				sender, 
				parameters.LimitTypeSel == ScheduleRunLimitType.StopOnExecutionDate);

			sender.ClearFieldErrors<ScheduleRun.Parameters.executionDate>(parameters);

			if (parameters.LimitTypeSel == ScheduleRunLimitType.StopOnExecutionDate
				&& parameters.ExecutionDate == null)
			{
				sender.DisplayFieldError<ScheduleRun.Parameters.executionDate>(
					parameters,
					Common.Messages.ExecutionDateBoxCannotBeEmptyIfStopOnExecutionDate);
			}

			ScheduleRunBase.SetProcessDelegate<TProcessGraph>(
				this,
				(ScheduleRun.Parameters)e.Row,
				Schedule_List);
		}

		[Obsolete("This handler is not used anymore and will be removed in Acumatica 8.0")]
		protected virtual void Parameters_StartDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) { }
		#endregion
	}
}
