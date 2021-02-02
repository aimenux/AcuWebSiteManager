using System;
using PX.Data;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
    public abstract class DefaultWorkingTimeAttribute : PXEventSubscriberAttribute, IPXFieldDefaultingSubscriber
    {
        protected abstract TimeSpan DefaultTime
        {
            get;
        }

        public void FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs args)
        {
            if (args.Row != null)
            {
                args.NewValue = cache.Graph.Accessinfo.BusinessDate?.Add(DefaultTime);
            }
        }
    }
}