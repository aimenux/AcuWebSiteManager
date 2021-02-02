using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Infrastructure;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Data;
using PX.CS.Contracts.Interfaces;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public abstract class WeatherIntegrationBaseStrategy<TModel> : IWeatherIntegrationStrategy
        where TModel : class, new()
    {
        protected Request Request;

        protected abstract string BaseUrl
        {
            get;
        }

        public void InitializeRequest(PXGraph graph, WeatherIntegrationSetup weatherIntegrationSetup)
        {
            Request = weatherIntegrationSetup.IsWeatherProcessingLogEnabled == true
                ? new RequestWithWeatherProcessingLog(graph, BaseUrl)
                : new Request(BaseUrl);
        }

        public DailyFieldReportWeather GetDailyFieldReportWeather()
        {
            var model = Request.Execute<TModel>();
            return CreateDailyFieldReportWeather(model);
        }

        public abstract void AddRequiredParameters(WeatherIntegrationSetup weatherIntegrationSetup);

        public abstract void AddCityAndCountryParameters(IAddressLocation location);

        public abstract void AddZipCodeAndCountryParameters(IAddressLocation location);

        public abstract void AddGeographicLocationParameters(IAddressLocation location);

        protected abstract DailyFieldReportWeather CreateDailyFieldReportWeather(TModel model);
    }
}