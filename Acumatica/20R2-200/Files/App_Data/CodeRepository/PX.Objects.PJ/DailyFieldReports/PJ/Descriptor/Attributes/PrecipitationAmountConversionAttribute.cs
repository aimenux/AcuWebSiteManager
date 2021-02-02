using System;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes
{
    public class PrecipitationAmountConversionAttribute : UnitOfMeasureConversionBaseAttribute
    {
        protected override string WeatherFieldName => nameof(DailyFieldReportWeather.PrecipitationAmount);

        protected override Func<decimal?, bool, decimal?> ConvertFunction =>
            UnitOfMeasureService.ConvertPrecipitationAmount;
    }
}