using System;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Descriptor.Attributes;

namespace PX.Objects.PJ.DailyFieldReports.PJ.Services
{
    public class WeatherIntegrationUnitOfMeasureService : IWeatherIntegrationUnitOfMeasureService
    {
		protected readonly PXGraph Graph;

		private WeatherIntegrationSetup weatherIntegrationSetup;
		public WeatherIntegrationSetup WeatherIntegrationSetup
		{
			get
			{
				return weatherIntegrationSetup ?? (weatherIntegrationSetup = Graph != null
					       ? Graph.Caches<WeatherIntegrationSetup>().Current as WeatherIntegrationSetup
					       : PXDatabase.Select<WeatherIntegrationSetup>().SingleOrDefault());
			}
			set { weatherIntegrationSetup = value; }
		}

		public WeatherIntegrationUnitOfMeasureService(PXGraph graph)
		{
			Graph = graph;
		}

		public WeatherIntegrationUnitOfMeasureService()
		{
		}

		public string GetUnitOfMeasureDisplayName(string fieldName)
        {
	        switch (fieldName)
	        {
		        case nameof(DailyFieldReportWeather.TemperatureLevel):
			        return IsImperialFormat()
				        ? UnitOfMeasureLabels.TemperatureLevelInFahrenheit
				        : UnitOfMeasureLabels.TemperatureLevelInCelsius;
		        case nameof(DailyFieldReportWeather.WindSpeed):
			        return IsImperialFormat()
				        ? UnitOfMeasureLabels.WindSpeedInMilePerHour
				        : UnitOfMeasureLabels.WindSpeedInMeterPerSecond;
		        case nameof(DailyFieldReportWeather.PrecipitationAmount):
			        return IsImperialFormat()
				        ? UnitOfMeasureLabels.PrecipitationAmountInInches
				        : UnitOfMeasureLabels.PrecipitationAmountInMillimeters;
		        default:
			        return null;
	        }
        }

        public decimal? ConvertWindSpeed(decimal? value, bool isConversionToImperialNeeded)
        {
            if (IsImperialFormat())
            {
                return value;
            }
            return isConversionToImperialNeeded
                ? value * WeatherIntegrationConstants.UnitOfMeasures.NumberOfMetersPerMile
                : value / WeatherIntegrationConstants.UnitOfMeasures.NumberOfMetersPerMile;
        }

        public decimal? ConvertPrecipitationAmount(decimal? value, bool isConversionToImperialNeeded)
        {
            if (IsImperialFormat())
            {
                return value;
            }
            return isConversionToImperialNeeded
                ? value / WeatherIntegrationConstants.UnitOfMeasures.NumberOfMillimetersPerInch
                : value * WeatherIntegrationConstants.UnitOfMeasures.NumberOfMillimetersPerInch;
        }

        public decimal? ConvertTemperatureLevel(decimal? value, bool isConversionToImperialNeeded)
        {
            if (IsImperialFormat())
            {
                return value;
            }
            return isConversionToImperialNeeded
                ? ConvertCelsiusToFahrenheit(value)
                : ConvertFahrenheitToCelsius(value);
        }

        public decimal? ConvertReportWindSpeed(int? dailyFieldReportWeatherId)
        {
            var dailyFieldReportWeather = GetDailyFieldReportWeather(dailyFieldReportWeatherId);
            return IsImperialFormat()
                ? dailyFieldReportWeather.WindSpeed
                : dailyFieldReportWeather.WindSpeed / WeatherIntegrationConstants.UnitOfMeasures.NumberOfMetersPerMile;
        }

        public decimal? ConvertReportPrecipitationAmount(int? dailyFieldReportWeatherId)
        {
            var dailyFieldReportWeather = GetDailyFieldReportWeather(dailyFieldReportWeatherId);
            return IsImperialFormat()
                ? dailyFieldReportWeather.PrecipitationAmount
                : dailyFieldReportWeather.PrecipitationAmount
                * WeatherIntegrationConstants.UnitOfMeasures.NumberOfMillimetersPerInch;
        }

        public decimal? ConvertReportTemperatureLevel(int? dailyFieldReportWeatherId)
        {
            var dailyFieldReportWeather = GetDailyFieldReportWeather(dailyFieldReportWeatherId);
            return IsImperialFormat()
                ? dailyFieldReportWeather.TemperatureLevel
                : ConvertFahrenheitToCelsius(dailyFieldReportWeather.TemperatureLevel);
        }

        public DateTime ConvertReportTimeObserved(int? dailyFieldReportWeatherId)
        {
            var dailyFieldReportWeather = GetDailyFieldReportWeather(dailyFieldReportWeatherId);
            var systemTimeZone = LocaleInfo.GetTimeZone();
            var timeObserved = dailyFieldReportWeather.TimeObserved.GetValueOrDefault();
            return PXTimeZoneInfo.ConvertTimeFromUtc(timeObserved, systemTimeZone);
        }

        private bool IsImperialFormat()
        {
            return WeatherIntegrationSetup?.UnitOfMeasureFormat == UnitOfMeasureFormat.Imperial;
        }

        private static decimal? ConvertFahrenheitToCelsius(decimal? value)
        {
            return (value - 32) * 5 / 9;
        }

        private static decimal? ConvertCelsiusToFahrenheit(decimal? value)
        {
            return value * 9 / 5 + 32;
        }

        private static DailyFieldReportWeather GetDailyFieldReportWeather(int? dailyFieldReportWeatherId)
        {
            return PXDatabase.Select<DailyFieldReportWeather>()
                .Single(w => w.DailyFieldReportWeatherId == dailyFieldReportWeatherId);
        }
    }
}