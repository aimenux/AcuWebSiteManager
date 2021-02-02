using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions;
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Graphs
{
    public class DailyFieldReportWeatherProcess : PXGraph<DailyFieldReportWeatherProcess>
    {
        public PXFilter<DailyFieldReportWeatherFilter> Filter;

        public PXCancel<DailyFieldReportWeatherFilter> Cancel;

        [PXFilterable]
        public PXFilteredProcessing<DailyFieldReport, DailyFieldReportWeatherFilter,
            Where<DailyFieldReport.hold.IsEqual<True>
                .And<DailyFieldReport.projectId.IsEqual<DailyFieldReportWeatherFilter.projectId.FromCurrent>
                    .Or<DailyFieldReportWeatherFilter.projectId.FromCurrent.IsNull>>
                .And<DailyFieldReport.date.IsEqual<AccessInfo.businessDate.FromCurrent>>>,
            OrderBy<Desc<DailyFieldReport.dailyFieldReportId>>> DailyFieldReports;

        public virtual void _(Events.RowSelected<DailyFieldReportWeatherFilter> args)
        {
            DailyFieldReports.SetProcessDelegate(CreateWeatherForDailyFieldReport);
        }

        private static void CreateWeatherForDailyFieldReport(DailyFieldReport dailyFieldReport)
        {
            var dailyFieldReportGraph = CreateInstance<DailyFieldReportEntry>();
            var weatherExtension = dailyFieldReportGraph
                .GetExtension<DailyFieldReportEntryWeatherExtension>();
            dailyFieldReportGraph.DailyFieldReport.Current = dailyFieldReport;
            weatherExtension.LoadWeatherConditions.Press();
            weatherExtension.Weather.Cache.Persist(PXDBOperation.Insert);
        }
    }
}
