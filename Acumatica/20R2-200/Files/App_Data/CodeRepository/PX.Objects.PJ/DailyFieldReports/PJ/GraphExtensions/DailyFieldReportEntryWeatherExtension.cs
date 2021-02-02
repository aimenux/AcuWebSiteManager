using System;
using System.Linq;
using PX.Objects.PJ.DailyFieldReports.Common.GenericGraphExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.MappedCacheExtensions;
using PX.Objects.PJ.DailyFieldReports.Common.Mappings;
using PX.Objects.PJ.DailyFieldReports.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Services;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Objects.PJ.DailyFieldReports.PJ.Graphs;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.PJ.DailyFieldReports.PJ.GraphExtensions
{
    public class DailyFieldReportEntryWeatherExtension : DailyFieldReportEntryExtension<DailyFieldReportEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<DailyFieldReportWeather>
            .Where<DailyFieldReportWeather.dailyFieldReportId
                .IsEqual<DailyFieldReport.dailyFieldReportId.FromCurrent>>.View Weather;

        public PXSetup<WeatherIntegrationSetup> WeatherIntegrationSetup;

        public PXAction<DailyFieldReport> LoadWeatherConditions;
        protected IWeatherIntegrationUnitOfMeasureService UnitOfMeasureService;

        [InjectDependency]
        public IWeatherIntegrationService WeatherIntegrationService
        {
            get;
            set;
        }

        [InjectDependency]
        public IWeatherIntegrationUnitOfMeasureService WeatherIntegrationUnitOfMeasureService
        {
            get;
            set;
        }

        public override void Initialize()
        {
	        base.Initialize();

	        var setup = WeatherIntegrationSetup.Current;
        }

        protected override (string Entity, string View) Name =>
            (DailyFieldReportEntityNames.WeatherObservation, ViewNames.Weather);

        [PXButton]
        [PXUIField(DisplayName = "Load weather conditions")]
        [PXUIVisible(typeof(WeatherIntegrationSetup.isConfigurationEnabled.FromCurrent.IsEqual<True>))]
        public virtual void loadWeatherConditions()
        {
            var dailyFieldReportWeather = WeatherIntegrationService.GetDailyFieldReportWeather();
            Weather.Insert(dailyFieldReportWeather);
        }

        public virtual void _(Events.FieldDefaulting<DailyFieldReportWeather.timeObserved> args)
        {
            args.NewValue = PXTimeZoneInfo.UtcNow;
        }

        public override void _(Events.RowSelected<DailyFieldReport> args)
        {
            base._(args);
            if (args.Row is DailyFieldReport dailyFieldReport)
            {
                var isActionAvailable = IsCreationActionAvailable(dailyFieldReport);
                var isWeatherIntegrationEnabled = WeatherIntegrationSetup.Current
                    .IsConfigurationEnabled.GetValueOrDefault();
                var isEnabled = Base.IsMobile
                    ? isActionAvailable && isWeatherIntegrationEnabled
                    : isActionAvailable;
                LoadWeatherConditions.SetEnabled(isEnabled);
                SetDailyFieldReportWeatherConditions(dailyFieldReport);
                SetUnitOfMeasureFieldDisplayName(Weather.Cache, nameof(DailyFieldReportWeather.TemperatureLevel));
                SetUnitOfMeasureFieldDisplayName(Weather.Cache, nameof(DailyFieldReportWeather.WindSpeed));
                SetUnitOfMeasureFieldDisplayName(Weather.Cache, nameof(DailyFieldReportWeather.PrecipitationAmount));
            }
        }

        public virtual void _(Events.FieldSelecting<DailyFieldReportWeather.timeObserved> args)
        {
            if (args.ReturnValue is DateTime timeObserved)
            {
                args.ReturnValue = ConvertTimeObserved(timeObserved);
            }
        }

        public virtual void _(Events.FieldUpdating<DailyFieldReportWeather.timeObserved> args)
        {
            if (args.NewValue != null)
            {
                var currentDateTime = (DateTime) args.NewValue;
                var systemTimeZone = LocaleInfo.GetTimeZone();
                args.NewValue = PXTimeZoneInfo.ConvertTimeToUtc(currentDateTime, systemTimeZone);
            }
        }

        public virtual void _(Events.RowSelected<DailyFieldReportWeather> args)
        {
            if (args.Row is DailyFieldReportWeather weather && Base.IsMobile)
            {
                ConfigureUnitOfMeasureFieldOnMobile(args.Cache,
                    args.Row, nameof(DailyFieldReportWeather.TemperatureLevelMobile));
                ConfigureUnitOfMeasureFieldOnMobile(args.Cache,
                    args.Row, nameof(DailyFieldReportWeather.PrecipitationAmountMobile));
                ConfigureUnitOfMeasureFieldOnMobile(args.Cache,
                    args.Row, nameof(DailyFieldReportWeather.WindSpeedMobile));
                weather.LastModifiedDateTime = weather.LastModifiedDateTime.GetValueOrDefault().Date;
            }
        }

        protected override DailyFieldReportRelationMapping GetDailyFieldReportRelationMapping()
        {
            return new DailyFieldReportRelationMapping(typeof(DailyFieldReportWeather))
            {
                RelationId = typeof(DailyFieldReportWeather.dailyFieldReportWeatherId)
            };
        }

        protected override PXSelectExtension<DailyFieldReportRelation> CreateRelationsExtension()
        {
            return new PXSelectExtension<DailyFieldReportRelation>(Weather);
        }

        private void SetDailyFieldReportWeatherConditions(DailyFieldReport dailyFieldReport)
        {
            if (WeatherIntegrationSetup.Current.IsConfigurationEnabled != true)
            {
                return;
            }
            var lastWeather = Weather.SelectMain().LastOrDefault();
            if (lastWeather != null)
            {
                dailyFieldReport.TemperatureLevel = lastWeather.TemperatureLevel;
                dailyFieldReport.Humidity = lastWeather.Humidity;
                dailyFieldReport.Icon = lastWeather.Icon;
                dailyFieldReport.TimeObserved = ConvertTimeObserved(lastWeather.TimeObserved.GetValueOrDefault());
            }
        }

        private static DateTime ConvertTimeObserved(DateTime timeObserved)
        {
            var systemTimeZone = LocaleInfo.GetTimeZone();
            return PXTimeZoneInfo.ConvertTimeFromUtc(timeObserved, systemTimeZone);
        }

        private void SetUnitOfMeasureFieldDisplayName(PXCache cache, string fieldName)
        {
            var displayName = WeatherIntegrationUnitOfMeasureService.GetUnitOfMeasureDisplayName(fieldName);
            PXUIFieldAttribute.SetDisplayName(cache, fieldName, displayName);
        }

        private static void ConfigureUnitOfMeasureFieldOnMobile(PXCache cache,
            DailyFieldReportWeather dailyFieldReportWeather, string fieldName)
        {
            PXUIFieldAttribute.SetVisible(cache, dailyFieldReportWeather, fieldName, true);
            PXUIFieldAttribute.SetVisibility(cache, dailyFieldReportWeather, fieldName, PXUIVisibility.Visible);
        }
    }
}