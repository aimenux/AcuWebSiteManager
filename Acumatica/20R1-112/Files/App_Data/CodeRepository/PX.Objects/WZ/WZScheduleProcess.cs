using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.WZ
{
    public class WZScheduleProcess : PXGraph<WZScheduleProcess>, IScheduleProcessing
    {
        public PXSelect<Schedule> Running_Schedule;


        public virtual void GenerateProc(Schedule s)
        {
            GenerateProc(s, 1, Accessinfo.BusinessDate.Value);
        }

        public virtual void GenerateProc(Schedule s, short Times, DateTime runDate)
        {
            IEnumerable<ScheduleDet> sd = new Scheduler(this).MakeSchedule(s, Times, runDate);
            WZTaskEntry graph = PXGraph.CreateInstance<WZTaskEntry>();

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                foreach (ScheduleDet d in sd)
                {
                    foreach (WZScenario scenario in PXSelect<WZScenario, Where<WZScenario.scheduleID, Equal<Required<Schedule.scheduleID>>,And<WZScenario.scheduled,Equal<True>>>>.
                                                    Select(this, s.ScheduleID))
                    {            
                        scenario.ExecutionDate = d.ScheduledDate;
                        graph.Scenario.Current = scenario;
                        if (scenario.Status != WizardScenarioStatusesAttribute._SUSPEND)
                        {
                            graph.activateScenarioWithoutRefresh.Press();
                        }
                    }
                    s.LastRunDate = d.ScheduledDate;
                    Running_Schedule.Cache.Update(s);
                }
                Running_Schedule.Cache.Persist(PXDBOperation.Update);
                ts.Complete(this);
            }
        }
    }
}
