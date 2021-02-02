using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class DefaultDateFromDailyFieldReportAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        private static TimeSpan DefaultTime => new TimeSpan(9, 0, 0);

        public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row != null)
            {
                args.NewValue = cache.Graph.Caches<DailyFieldReport>().Current?
                    .Cast<DailyFieldReport>().Date?.Date.Add(DefaultTime);
            }
        }
    }
}
