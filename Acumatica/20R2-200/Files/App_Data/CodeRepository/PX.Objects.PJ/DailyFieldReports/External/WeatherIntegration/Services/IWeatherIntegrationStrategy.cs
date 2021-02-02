using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.CS.Contracts.Interfaces;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public interface IWeatherIntegrationStrategy
    {
        DailyFieldReportWeather GetDailyFieldReportWeather();

        void InitializeRequest(PXGraph graph, WeatherIntegrationSetup weatherIntegrationSetup);

        void AddRequiredParameters(WeatherIntegrationSetup weatherIntegrationSetup);

        void AddCityAndCountryParameters(IAddressLocation location);

        void AddZipCodeAndCountryParameters(IAddressLocation location);

        void AddGeographicLocationParameters(IAddressLocation location);
    }
}