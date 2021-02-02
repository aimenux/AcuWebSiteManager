using System;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services;
using PX.Data;
using PX.Objects.CN.Common.Extensions;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public abstract class UnitOfMeasureConversionBaseAttribute : PXEventSubscriberAttribute,
        IPXFieldUpdatingSubscriber, IPXFieldSelectingSubscriber
    {
        protected IWeatherIntegrationUnitOfMeasureService UnitOfMeasureService;

        protected abstract string WeatherFieldName
        {
            get;
        }

        protected abstract Func<decimal?, bool, decimal?> ConvertFunction
        {
            get;
        }

        public void FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs args)
        {
            if (args.Row != null)
            {
                UnitOfMeasureService = cache.Graph.GetService<IWeatherIntegrationUnitOfMeasureService>();
                args.NewValue = ConvertFunction(args.NewValue as decimal?, true);
            }
        }

        public void FieldSelecting(PXCache cache, PXFieldSelectingEventArgs args)
        {
            if (args.Row != null)
            {
                UnitOfMeasureService = cache.Graph.GetService<IWeatherIntegrationUnitOfMeasureService>();
                var value = cache.GetValue(args.Row, WeatherFieldName) as decimal?;
                args.ReturnValue = ConvertFunction(value, false);
            }
        }
    }
}