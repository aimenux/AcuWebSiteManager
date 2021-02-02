using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class WorkingHoursReferenceAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is DailyFieldReportSubcontractorActivity subcontractor
            && !cache.HasError<DailyFieldReportSubcontractorActivity.timeDeparted>(subcontractor,
                DailyFieldReportMessages.DepartureTimeMustBeLaterThanArrivalTime))
            {
                cache.SetValueExt<DailyFieldReportSubcontractorActivity.workingTimeSpent>(subcontractor,
                    subcontractor.DefaultWorkingTimeSpent);
            }
        }
    }
}