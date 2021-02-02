using PX.Common;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor
{
    [PXLocalizable]
    public static class WeatherIntegrationMessages
    {
        public const string ToLoadWeatherConditionsMustBeSpecifiedForDailyFieldReport =
            "To load weather conditions {0} and {1} must be specified for the daily field report.";

        public const string ToLoadWeatherConditionsMustBeSpecifiedOnWeatherIntegrationSettingsTab =
            "To load weather conditions weather integration settings must be specified on Weather Integration" +
            " Settings tab (Project Management Preferences screen).";

        public const string WeatherLogIsAvailableIfSettingsAreEnabled =
            "The \"Daily Field Report Weather Processing Log\" is available if \"Enable Weather Service Integration" +
            " for Daily Field Reports\" and \"Enable Weather Processing Log\" settings are enabled on \"Weather" +
            " Service Integration Settings\" tab, Project Management Preferences screen.";

        public const string ClearWeatherLogIsAvailableIfSettingsAreEnabled =
            "The \"Clear Daily Field Report Weather Processing Log\" is available if \"Enable Weather Service Integration" +
            " for Daily Field Reports\" and \"Enable Weather Processing Log\" settings are enabled on \"Weather" +
            " Service Integration Settings\" tab, Project Management Preferences screen.";

        public const string WeatherApiServiceMustBeSelected = "Weather API Service must be selected.";
    }
}