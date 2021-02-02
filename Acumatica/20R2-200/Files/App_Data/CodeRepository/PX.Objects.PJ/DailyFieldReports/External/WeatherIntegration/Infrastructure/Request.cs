using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Descriptor;
using PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Exceptions;
using RestSharp;

namespace PX.Objects.PJ.DailyFieldReports.External.WeatherIntegration.Infrastructure
{
    public class Request
    {
        internal Request(string baseUrl)
        {
            BaseUrl = baseUrl;
        }

        protected IRestResponse Response
        {
            get;
            set;
        }

        private Dictionary<string, object> QueryParameters
        {
            get;
        } = new Dictionary<string, object>();

        private string BaseUrl
        {
            get;
        }

        public virtual TModel Execute<TModel>()
            where TModel : new()
        {
            var request = BuildRestRequest();
            Response = new RestClient(BaseUrl).Execute<TModel>(request);
            HandleStatusCode();
            ValidateResponseContent();
            return Deserialize<TModel>();
        }

        public Request AddQueryParameter(string name, object value)
        {
            if (value != null)
            {
                QueryParameters.Add(name, value);
            }
            return this;
        }

        private RestRequest BuildRestRequest()
        {
            var request = new RestRequest(Method.GET);
            AddQueryParameters(request);
            return request;
        }

        private void AddQueryParameters(IRestRequest request)
        {
            foreach (var parameter in QueryParameters)
            {
                request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
            }
        }

        private void ValidateResponseContent()
        {
            if (Response.Content == WeatherIntegrationConstants.NoContentResponse)
            {
                Response.Content = NoContentException.TheServerIsNotReturningAnyContent;
                throw new NoContentException();
            }
        }

        private TModel Deserialize<TModel>()
        {
	        using (var stringReader = new StringReader(Response.Content))
	        {
		        using (var jsonTextReader = new JsonTextReader(stringReader))
		        {
			        return new JsonSerializer().Deserialize<TModel>(jsonTextReader);
				}
	        }
        }

        private void HandleStatusCode()
        {
            switch (Response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    throw new WeatherApiKeyIsNotCorrectException();
                case HttpStatusCode.BadRequest:
                    throw new NothingToGeocodeException();
                case HttpStatusCode.NotFound:
                    throw new CountryOrCityNotFoundException();
                case HttpStatusCode.Forbidden:
                    throw new WeatherApiKeyIsNotCorrectException();
                case HttpStatusCode.ServiceUnavailable:
                    throw new NumberOfRequestsHasBeenExceededException();
                case HttpStatusCode.NoContent:
                    Response.Content = NoContentException.TheServerIsNotReturningAnyContent;
                    throw new NoContentException();
                case (HttpStatusCode) 429:
                    throw new NumberOfRequestsHasBeenExceededException();
            }
        }
    }
}