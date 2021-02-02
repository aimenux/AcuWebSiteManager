using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class WindSpeedConversionAttribute : UnitOfMeasureConversionBaseAttribute
    {
        protected override string WeatherFieldName => nameof(DailyFieldReportWeather.WindSpeed);

        protected override Func<decimal?, bool, decimal?> ConvertFunction => UnitOfMeasureService.ConvertWindSpeed;
    }
}