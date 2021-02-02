using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public interface IWeatherIntegrationService
    {
        DailyFieldReportWeather GetDailyFieldReportWeather();
    }
}
