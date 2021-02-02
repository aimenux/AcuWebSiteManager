using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class TemperatureLevelConversionAttribute : UnitOfMeasureConversionBaseAttribute
    {
        protected override string WeatherFieldName => nameof(DailyFieldReportWeather.TemperatureLevel);

        protected override Func<decimal?, bool, decimal?> ConvertFunction =>
            UnitOfMeasureService.ConvertTemperatureLevel;
    }
}