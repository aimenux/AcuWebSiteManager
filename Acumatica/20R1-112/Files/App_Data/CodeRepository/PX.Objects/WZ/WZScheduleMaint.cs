using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.WZ
{
    public class WZScheduleMaint : PXGraph<WZScheduleMaint, Schedule>
    {
        public PXSelect<Schedule, Where<Schedule.module, Equal<BatchModule.moduleWZ>>> Schedule_Header;
        public PXSelect<WZScenario, Where<WZScenario.scheduled, Equal<True>,And<WZScenario.scheduleID, Equal<Current<Schedule.scheduleID>>>>> Scenarios;
        public PXSetup<GLSetup> GLSetup;

        public WZScheduleMaint()
        {
            GLSetup gls = GLSetup.Current;
        }
		public override bool CanClipboardCopyPaste() { return false; }

        [PXDBGuid(IsKey = true)]
        [PXUIField(DisplayName = "Scenario ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(WZScenario.scenarioID), SubstituteKey = typeof(WZScenario.name))]
        protected virtual void WZScenario_ScenarioID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(15, IsUnicode = true)]
        [PXDBDefault(typeof (Schedule.scheduleID))]
        [PXParent(typeof(Select<Schedule, Where<Schedule.scheduleID, Equal<Current<WZScenario.scheduleID>>>>), LeaveChildren = true)]
        protected virtual void WZScenario_ScheduleID_CacheAttached(PXCache sender)
        {
        }

        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
        [AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
        [PXSelector(typeof(Search<Schedule.scheduleID, Where<Schedule.module, Equal<BatchModule.moduleWZ>>>))]
        [PXDefault]
        protected virtual void Schedule_ScheduleID_CacheAttached(PXCache sender)
        {
        }

        protected virtual void Schedule_Module_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = BatchModule.WZ;
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

        protected virtual void Schedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            SetControlsState(cache, (Schedule)e.Row);
        }

        protected virtual void Schedule_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {

            Schedule schedule = (Schedule)e.Row;
            bool nextRunDate = true;
            if (schedule.RunLimit <= 0 && !(bool)schedule.NoRunLimit)
            {
                sender.RaiseExceptionHandling<Schedule.runLimit>(schedule, schedule.RunLimit, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "D" && schedule.DailyFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.dailyFrequency>(schedule, schedule.DailyFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "W" && schedule.WeeklyFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.weeklyFrequency>(schedule, schedule.WeeklyFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "P" && schedule.PeriodFrequency <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.periodFrequency>(schedule, schedule.PeriodFrequency, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.ScheduleType == "P" && schedule.PeriodDateSel == "D" && schedule.PeriodFixedDay <= 0)
            {
                sender.RaiseExceptionHandling<Schedule.periodFixedDay>(schedule, schedule.PeriodFixedDay, new PXSetPropertyException(CS.Messages.Entry_GT, "0"));
                nextRunDate = false;
            }
            if (schedule.EndDate == null && schedule.NoEndDate == false)
            {
                sender.RaiseExceptionHandling<Schedule.endDate>(schedule, null, new PXSetPropertyException(CR.Messages.EmptyValueErrorFormat, typeof(Schedule.endDate).Name));
                nextRunDate = false;
            }
            if (schedule.WeeklyOnDay1 != true && schedule.WeeklyOnDay2 != true && schedule.WeeklyOnDay3 != true && schedule.WeeklyOnDay4 != true && schedule.WeeklyOnDay5 != true && schedule.WeeklyOnDay6 != true && schedule.WeeklyOnDay7 != true)
            {
                sender.RaiseExceptionHandling<Schedule.weeklyOnDay1>(schedule, null, new PXSetPropertyException(GL.Messages.DayOfWeekNotSelected));
                nextRunDate = false;
            }

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && nextRunDate)
            {
                ((Schedule)e.Row).NextRunDate = new Scheduler(this).GetNextRunDate((Schedule)e.Row);
            }
        }
        
        public PXAction<Schedule> RunNow;

        [PXUIField(DisplayName = "Run Now", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton]
        public virtual IEnumerable runNow(PXAdapter adapter)
        {
            this.Save.Press();

            Schedule schedule = Schedule_Header.Current;
            if (schedule.NextRunDate > Accessinfo.BusinessDate)
            {
                throw new PXException(GL.Messages.SheduleNextExecutionDateExceeded);
            }
            else
            {
                if (schedule.NoRunLimit == false && schedule.RunCntr >= schedule.RunLimit)
                {
                    throw new PXException(GL.Messages.SheduleExecutionLimitExceeded);
                }
                else
                {
                    if (schedule.NoEndDate == false && schedule.EndDate < Accessinfo.BusinessDate)
                    {
                        throw new PXException(GL.Messages.SheduleHasExpired);
                    }
                    else
                    {
                        PXLongOperation.StartOperation(this,
                            delegate()
                            {
                                WZScheduleProcess sp = PXGraph.CreateInstance<WZScheduleProcess>();
                                sp.GenerateProc(schedule);
                                PXSiteMap.Provider.Clear();
								throw new PXRefreshException();
                            }
                        );

                    }
                }
            }

            return adapter.Get();
        }

        protected virtual void WZScenario_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            WZScenario row = (WZScenario)e.Row;
            if (row != null && row.ScenarioID != null)
            {
                row = PXSelectReadonly<WZScenario, 
                                        Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>
                                        .Select(this, row.ScenarioID);
                if (row != null)
                {
                    if (row.ScheduleID != null)
                    {
                        cache.RaiseExceptionHandling<WZScenario.scenarioID>(row, row.ScenarioID,
                            new PXSetPropertyException(Messages.ScenarioAlreadyUsed, row.ScheduleID));
                    }
                    Scenarios.Delete(row);
                    Scenarios.Update(row);
                }
                else
                {
                    row = (WZScenario)e.Row;
                    Scenarios.Delete(row);
                    cache.RaiseExceptionHandling<WZScenario.scenarioID>(row, row.ScenarioID, new PXSetPropertyException(Messages.ScenarioReferenceIsNotValid));
                }
            }
        }

        protected virtual void WZScenario_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            WZScenario scenario = e.Row as WZScenario;
            if (scenario != null && scenario.Scheduled != false)
            {
                if (scenario.ScheduleID == null)
                {
                    scenario.ScheduleID = Schedule_Header.Current.ScheduleID;
                    scenario.Scheduled = true;
                }
                else
                {
                    WZScenario dbScenario = PXSelectReadonly<WZScenario,
                                        Where<WZScenario.scenarioID, Equal<Required<WZScenario.scenarioID>>>>
                                        .Select(this, scenario.ScenarioID);
                    if (dbScenario.ScheduleID != null && dbScenario.ScheduleID != scenario.ScheduleID)
                    {
                        cache.RaiseExceptionHandling<WZScenario.scenarioID>(scenario, scenario.Name,
                            new PXSetPropertyException(Messages.ScenarioAlreadyUsed, dbScenario.ScheduleID));
                    }
                }
            }
        }

        public override void Persist()
        {
            foreach (WZScenario b in Scenarios.Cache.Updated)
            {
                b.Scheduled = true;
                b.ScheduleID = Schedule_Header.Current.ScheduleID;
                Scenarios.Cache.Update(b);
            }

            foreach (WZScenario b in Scenarios.Cache.Deleted)
            {
                PXDBDefaultAttribute.SetDefaultForUpdate<WZScenario.scheduleID>(Scenarios.Cache, b, false);
                b.Scheduled = false;
                b.ScheduleID = null;
                Scenarios.Cache.SetStatus(b, PXEntryStatus.Updated);
                Scenarios.Cache.Update(b);
            }

            base.Persist();
        }

        private void SetControlsState(PXCache cache, Schedule s)
        {
            Boolean isMonthly = (s.ScheduleType == "M");
            Boolean isPeriodically = (s.ScheduleType == "P");
            Boolean isWeekly = (s.ScheduleType == "W");
            Boolean isDaily = (s.ScheduleType == "D");
            Boolean isNotProcessed = (s.LastRunDate == null);
            bool isActive = (s.Active ?? false);

            if (isActive)
            {
                RunNow.SetEnabled(true);
            }
            else
            {
                RunNow.SetEnabled(false);
            }

            PXUIFieldAttribute.SetVisible<Schedule.dailyFrequency>(cache, s, isDaily);
            PXUIFieldAttribute.SetVisible<Schedule.days>(cache, s, isDaily);
            SetMonthlyControlsState(cache, s);

            PXUIFieldAttribute.SetVisible<Schedule.weeklyFrequency>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay1>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay2>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay3>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay4>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay5>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay6>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeklyOnDay7>(cache, s, isWeekly);
            PXUIFieldAttribute.SetVisible<Schedule.weeks>(cache, s, isWeekly);

            PXUIFieldAttribute.SetVisible<Schedule.monthlyFrequency>(cache, s, isMonthly);
            PXUIFieldAttribute.SetVisible<Schedule.monthlyDaySel>(cache, s, isMonthly);

            PXUIFieldAttribute.SetVisible<Schedule.periodFrequency>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodDateSel>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periodFixedDay>(cache, s, isPeriodically);
            PXUIFieldAttribute.SetVisible<Schedule.periods>(cache, s, isPeriodically);
            SetPeriodicallyControlsState(cache, s);

            PXUIFieldAttribute.SetEnabled<Schedule.endDate>(cache, s, !(bool)s.NoEndDate);
            PXUIFieldAttribute.SetEnabled<Schedule.runLimit>(cache, s, !(bool)s.NoRunLimit);
            PXDefaultAttribute.SetPersistingCheck<Schedule.endDate>(cache, s, (s.NoEndDate == true ? PXPersistingCheck.Nothing : PXPersistingCheck.Null));
            
            cache.AllowDelete = isNotProcessed;
        }

        private void SetMonthlyControlsState(PXCache cache, Schedule s)
        {
            Boolean isMonthly = (s.ScheduleType == "M");
            PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDay>(cache, s, isMonthly && s.MonthlyDaySel == "D");
            PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
            PXUIFieldAttribute.SetEnabled<Schedule.monthlyOnDayOfWeek>(cache, s, isMonthly && s.MonthlyDaySel == "W");
        }

        private void SetPeriodicallyControlsState(PXCache cache, Schedule s)
        {
            Boolean isPeriodically = (s.ScheduleType == "P");
            PXUIFieldAttribute.SetEnabled<Schedule.periodFixedDay>(cache, s, isPeriodically && s.PeriodDateSel == "D");
        }
    }
}
