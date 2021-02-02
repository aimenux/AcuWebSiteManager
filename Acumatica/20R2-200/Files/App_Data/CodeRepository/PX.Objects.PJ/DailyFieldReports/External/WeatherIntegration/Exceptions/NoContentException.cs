using System;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions
{
    public class NoContentException : Exception
    {
        public const string TheServerIsNotReturningAnyContent =
            "The server successfully processed the request, but is not returning any content.";

        public NoContentException()
            : base(TheServerIsNotReturningAnyContent)
        {
        }
    }
}