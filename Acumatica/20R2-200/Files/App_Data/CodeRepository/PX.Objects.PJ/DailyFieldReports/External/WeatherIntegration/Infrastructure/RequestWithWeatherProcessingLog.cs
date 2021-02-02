using System.Net;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.PJ.DAC;
using PX.Common;
using PX.Data;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Infrastructure
{
    public class RequestWithWeatherProcessingLog : Request
    {
        private readonly PXGraph graph;

        internal RequestWithWeatherProcessingLog(PXGraph graph, string baseUrl)
            : base(baseUrl)
        {
            this.graph = graph;
        }

        public override TModel Execute<TModel>()
        {
            var cache = graph.Caches<WeatherProcessingLog>();
            var weatherProcessingLog = InsertWeatherProcessingLog(cache);
            try
            {
                return base.Execute<TModel>();
            }
            finally
            {
                UpdateWeatherProcessingLog(weatherProcessingLog);
                cache.Persist(weatherProcessingLog, PXDBOperation.Insert);
                cache.Clear();
            }
        }

        private static WeatherProcessingLog InsertWeatherProcessingLog(PXCache cache)
        {
            var weatherProcessingLog = (WeatherProcessingLog) cache.Insert();
            weatherProcessingLog.RequestTime = PXTimeZoneInfo.Now;
            return weatherProcessingLog;
        }

        private void UpdateWeatherProcessingLog(WeatherProcessingLog weatherProcessingLog)
        {
            weatherProcessingLog.ResponseTime = PXTimeZoneInfo.Now;
            weatherProcessingLog.RequestBody = Response.ResponseUri.AbsoluteUri;
            weatherProcessingLog.ResponseBody = Response.Content;
            weatherProcessingLog.RequestStatusIcon = GetRequestStatusIcon();
        }

        private string GetRequestStatusIcon()
        {
            return Response.StatusCode == HttpStatusCode.OK
                ? WeatherIntegrationConstants.RequestStatusIcons.RequestStatusSuccessIcon
                : WeatherIntegrationConstants.RequestStatusIcons.RequestStatusFailIcon;
        }
    }
}