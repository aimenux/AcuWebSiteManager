using System;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions
{
    public class WeatherApiKeyIsNotCorrectException : Exception
    {
        private const string WeatherApiKeyIsNotCorrect = "Weather API key is not correct.";

        public WeatherApiKeyIsNotCorrectException()
            : base(WeatherApiKeyIsNotCorrect)
        {
        }
    }
}