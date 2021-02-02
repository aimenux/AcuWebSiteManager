using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.SM;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Services
{
    public class AutomationScheduleDataProvider
    {
        private readonly PXGraph graph;

        public AutomationScheduleDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public AUSchedule GetAutomationSchedule(string screenId)
        {
            return SelectFrom<AUSchedule>.Where<AUSchedule.screenID.IsEqual<P.AsString>>.View.Select(graph, screenId);
        }
    }
}