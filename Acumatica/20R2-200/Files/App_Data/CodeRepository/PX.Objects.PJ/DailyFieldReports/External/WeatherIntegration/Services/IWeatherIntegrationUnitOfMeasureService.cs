namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public interface IWeatherIntegrationUnitOfMeasureService
    {
        string GetUnitOfMeasureDisplayName(string fieldName);

        decimal? ConvertTemperatureLevel(decimal? value, bool isConversionToImperialNeeded);

        decimal? ConvertWindSpeed(decimal? value, bool isConversionToImperialNeeded);

        decimal? ConvertPrecipitationAmount(decimal? value, bool isConversionToImperialNeeded);

        decimal? ConvertReportWindSpeed(int? dailyFieldReportWeatherId);

        decimal? ConvertReportPrecipitationAmount(int? dailyFieldReportWeatherId);

        decimal? ConvertReportTemperatureLevel(int? dailyFieldReportWeatherId);
    }
}