using System.Collections;
using System.Linq;

using PX.Data;

namespace PX.Objects.GL
{
	/// <summary>
	/// Represents the base BLC for module-specific Recurring Transactions controllers.
	/// </summary>
	/// <typeparam name="TGraph">The specific type of the derived graph.</typeparam>
	/// <typeparam name="TProcessGraph">The type of the graph used for the documents generation.</typeparam>
	public class ScheduleMaintBase<TGraph, TProcessGraph> : PXGraph<TGraph, Schedule>
		where TGraph : PXGraph
		where TProcessGraph : PXGraph<TProcessGraph>, IScheduleProcessing, new()
	{
		public PXSelect<Schedule> Schedule_Header;
		protected virtual IEnumerable schedule_Header()
		{
			return (new PXView(this, false, Schedule_Header.View.BqlSelect)).QuickSelect();
		}

		public PXAction<Schedule> RunNow;
		/// <summary>
		/// Starts document generation according to the schedule.
		/// </summary>
		[PXUIField(DisplayName = Messages.ProcRunNow, MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable runNow(PXAdapter adapter)
		{
			this.Save.Press();

			Schedule schedule = Schedule_Header.Current;

			if (schedule.NextRunDate > Accessinfo.BusinessDate)
			{
				throw new PXException(Messages.SheduleNextExecutionDateExceeded);
			}
			else
			{
				if (schedule.NoRunLimit == false && schedule.RunCntr >= schedule.RunLimit)
				{
					throw new PXException(Messages.SheduleExecutionLimitExceeded);
				}
				else
				{
					PXLongOperation.StartOperation(this, () =>
					{
						TProcessGraph processGraph = PXGraph.CreateInstance<TProcessGraph>();
						processGraph.GenerateProc(schedule);
					});
				}
			}

			yield return schedule;
		}

		public override bool CanClipboardCopyPaste() => false;

		/// <summary>
		/// Identifies the module of the deriving schedule graph.
		/// </summary>
		protected virtual string Module => BatchModule.GL;

		/// <summary>
		/// Returns a value indicating whether the current schedule
		/// contains any details.
		/// </summary>
		internal virtual bool AnyScheduleDetails() => false;

		#region Event Handlers

		#region Schedule

		protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) return;

			SetControlsState(cache, (Schedule)e.Row);
		}

		protected virtual void Schedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			Schedule schedule = (Schedule)e.Row;

			bool nextRunDate = !PXUIFieldAttribute
				.GetErrors(sender, schedule, PXErrorLevel.Error, PXErrorLevel.RowError)
				.Any();

			if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && nextRunDate)
			{
				schedule.NextRunDate = new Scheduler(this).GetNextRunDate(schedule);
			}
		}

		protected virtual void Schedule_Module_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			e.NewValue = this.Module;
		}

		protected virtual void Schedule_NoEndDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if (!(bool)e.OldValue && (bool)((Schedule)e.Row).NoEndDate != (bool)e.OldValue)
			{
				Schedule s = (Schedule)e.Row;
				s.EndDate = null;
			}
		}

		protected virtual void Schedule_NoRunLimit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			if ((bool)((Schedule)e.Row).NoRunLimit != (bool)e.OldValue)
			{
				Schedule s = (Schedule)e.Row;
				if ((bool)e.OldValue)
					s.RunLimit = 1;
				else
					s.RunLimit = 0;
			}
		}

		protected virtual void Schedule_ScheduleType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			Schedule s = (Schedule)e.Row;
			if (s.NextRunDate != null && !object.Equals(e.OldValue, s.ScheduleType))
			{
				throw new PXException(Messages.ScheduleTypeCantBeChanged);
			}
		}

		#endregion

		#endregion

		#region Helper Methods

		protected virtual void SetControlsState(PXCache cache, Schedule s)
		{
			bool isDaily = s.ScheduleType == GLScheduleType.Daily;
			bool isWeekly = s.ScheduleType == GLScheduleType.Weekly;
			bool isMonthly = s.ScheduleType == GLScheduleType.Monthly;
			bool isPeriodically = s.ScheduleType == GLScheduleType.Periodically;
			bool isNotProcessed = s.LastRunDate == null;

			SetDailyControlsState(cache, s, isDaily);
			SetWeeklyControlsState(cache, s, isWeekly);
			SetMonthlyControlsState(cache, s, isMonthly);
			SetPeriodicallyControlsState(cache, s, isPeriodically);

			PXUIFieldAttribute.SetEnabled<Schedule.runLimit>(cache, s, s.NoRunLimit != true);
			PXUIFieldAttribute.SetEnabled<Schedule.endDate>(cache, s, s.NoEndDate != true);

			PXDefaultAttribute.SetPersistingCheck<Schedule.endDate>(cache, s, (s.NoEndDate == true ? PXPersistingCheck.Nothing : PXPersistingCheck.Null));

			cache.AllowDelete = isNotProcessed;

			RunNow.SetEnabled(
				cache.GetStatus(s) != PXEntryStatus.Inserted &&
				s.Active == true &&
				this.AnyScheduleDetails());
		}

		private void SetDailyControlsState(PXCache cache, Schedule s, bool isDaily)
		{
			PXUIFieldAttribute.SetVisible<Schedule.dailyFrequency>(cache, s, isDaily);
			PXUIFieldAttribute.SetVisible<Schedule.days>(cache, s, isDaily);
		}

		private void SetWeeklyControlsState(PXCache cache, Schedule s, bool isWeekly)
		{
			PXUIFieldAttribute.SetVisible<Schedule.weeklyFrequency>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay1>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay2>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay3>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay4>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay5>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay6>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay7>(cache, s, isWeekly);
			PXUIFieldAttribute.SetVisible<Schedule.weeks>(cache, s, isWeekly);
		}

		private void SetMonthlyControlsState(PXCache cache, Schedule s, bool isMonthly)
		{
			PXUIFieldAttribute.SetVisible<Schedule.monthlyFrequency>(cache, s, isMonthly);
			PXUIFieldAttribute.SetVisible<Schedule.monthlyDaySel>(cache, s, isMonthly);

			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDay>(cache, s, isMonthly && s.MonthlyDaySel == "D");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
			PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDayOfWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
		}

		private void SetPeriodicallyControlsState(PXCache cache, Schedule s, bool isPeriodically)
		{
			PXUIFieldAttribute.SetVisible<Schedule.periodFrequency>(cache, s, isPeriodically);
			PXUIFieldAttribute.SetVisible<Schedule.periodDateSel>(cache, s, isPeriodically);
			PXUIFieldAttribute.SetVisible<Schedule.periodFixedDay>(cache, s, isPeriodically);
			PXUIFieldAttribute.SetVisible<Schedule.periods>(cache, s, isPeriodically);

			PXUIFieldAttribute.SetEnabled<Schedule.periodFixedDay>(cache, s, isPeriodically && s.PeriodDateSel == "D");
		}

		#endregion
	}
}
