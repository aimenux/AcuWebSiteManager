using System;
using System.Linq;
using PX.CS.Contracts.Interfaces;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Models.OpenWeatherMapModels;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services
{
    public class OpenWeatherMapIntegrationStrategy : WeatherIntegrationBaseStrategy<OpenWeatherMapResponse>
    {
        private const string IconUrl = "http://openweathermap.org/img/wn/{0}@2x.png";

        protected override string BaseUrl => "http://api.openweathermap.org/data/2.5/weather";

        public override void AddCityAndCountryParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("q", string.Concat(location.City, ",", location.CountryID));
        }

        public override void AddZipCodeAndCountryParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("zip", string.Concat(location.PostalCode, ",", location.CountryID));
        }

        public override void AddGeographicLocationParameters(IAddressLocation location)
        {
            Request.AddQueryParameter("lat", location.Latitude);
            Request.AddQueryParameter("lon", location.Longitude);
        }

        public override void AddRequiredParameters(WeatherIntegrationSetup weatherIntegrationSetup)
        {
            Request.AddQueryParameter("APPID", weatherIntegrationSetup.WeatherApiKey);
            Request.AddQueryParameter("units", WeatherIntegrationConstants.UnitOfMeasures.Fahrenheit);
        }

        protected override DailyFieldReportWeather CreateDailyFieldReportWeather(OpenWeatherMapResponse model)
        {
            var precipitationAmount = model.Rain?.PrecipitationAmount ?? model.Snow?.PrecipitationAmount;
            var weather = model.Weather.First();
            return new DailyFieldReportWeather
            {
                Icon = string.Format(IconUrl, weather.Icon),
                TimeObserved = DateTimeOffset.FromUnixTimeSeconds(model.TimeObserved).ToUniversalTime().DateTime,
                TemperatureLevel = model.Main.TemperatureLevel,
                WindSpeed = model.Wind.Speed,
                Cloudiness = model.Clouds.Cloudiness,
                Humidity = model.Main.Humidity,
                PrecipitationAmount =
                    precipitationAmount / WeatherIntegrationConstants.UnitOfMeasures.NumberOfMillimetersPerInch,
                LocationCondition = weather.SiteCondition
            };
        }
    }
}