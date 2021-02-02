using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using PX.Data;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using RestSharp.Serializers;

namespace PX.Commerce.Shopify.API.REST
{
	public abstract class SPRestClientBase : RestClient
	{
		protected static int ApiCallLimit;
		protected static int AvailableCallLimit;
		protected static int TotalCallLimit;
		protected static int ApiTimeLimit;
		protected static bool ControlTimeLimit;
		private static readonly Object obj = new Object();

		protected ISerializer _serializer;
		protected IDeserializer _deserializer;
		public Serilog.ILogger Logger { get; set; } = null;
		protected SPRestClientBase(IDeserializer deserializer, ISerializer serializer, IRestOptions options, Serilog.ILogger logger)
		{
			_serializer = serializer;
			_deserializer = deserializer;
			AddHandler("application/json", deserializer);
			AddHandler("text/json", deserializer);
			AddHandler("text/x-json", deserializer);
			if (TotalCallLimit == 0)
			{
				lock (obj)
				{
					TotalCallLimit = 40;
					AvailableCallLimit = TotalCallLimit;
					ControlTimeLimit = false;
				}
			}
			Authenticator = new HttpBasicAuthenticator(options.XAuthClient, options.XAuthTocken);
			try
			{
				BaseUrl = new Uri(options.BaseUri);
			}
			catch (UriFormatException e)
			{
				throw new UriFormatException("Invalid URL: The format of the URL could not be determined.", e);
			}
			Logger = logger;
		}

		public RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null)
		{
			var request = new RestRequest(url) { JsonSerializer = _serializer, RequestFormat = DataFormat.Json };

			if (urlSegments != null)
			{
				foreach (var urlSegment in urlSegments)
				{
					request.AddUrlSegment(urlSegment.Key, urlSegment.Value);
				}
			}

			return request;
		}

		protected IRestResponse ExecuteRequest(IRestRequest request)
		{
			IRestResponse response = null;
			bool released = false;
			if (SPConnector.ApiCallsPool == null)
			{
				response = Execute(request);
			}
			else
			{
				SPConnector.ApiCallsPool.Wait();
				if (AvailableCallLimit > 5)
				{
					lock (obj)
					{
						AvailableCallLimit--;
					}
					if (SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
					{
						SPConnector.ApiCallsPool.Release();
						released = true;
					}
				}
				try
				{
					if (ControlTimeLimit == true)
					{
						Thread.Sleep(ApiTimeLimit);
					}
					response = Execute(request);
				}
				catch (Exception ex)
				{
					//Make sure system release the Api call limit if it has error.
					if (released == false && SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
					{
						SPConnector.ApiCallsPool.Release();
					}
					throw ex;
				}
			}
			UnLockApiCall(response.Headers, released);
			return response;
		}

		protected IRestResponse<TR> ExecuteRequest<TR>(IRestRequest request) where TR : class, new()
		{
			IRestResponse<TR> response = null;
			bool released = false;
			if (SPConnector.ApiCallsPool == null)
			{
				response = Execute<TR>(request);
			}
			else
			{
				SPConnector.ApiCallsPool.Wait();

				if (AvailableCallLimit > 5)
				{
					lock (obj)
					{
						AvailableCallLimit--;
					}
					if (SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
					{
						SPConnector.ApiCallsPool.Release();
						released = true;
					}
				}
				try
				{
					if (ControlTimeLimit == true)
					{
						Thread.Sleep(ApiTimeLimit);
					}
					response = Execute<TR>(request);
				}
				catch (Exception ex)
				{
					//Make sure system release the Api call limit if it has error.
					if (released == false && SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
					{
						SPConnector.ApiCallsPool.Release();
					}
					throw ex;
				}
			}
			UnLockApiCall(response.Headers, released);
			return response;
		}

		protected void LogError(Uri baseUrl, IRestRequest request, IRestResponse response)
		{
			//Get the values of the parameters passed to the API
			var parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + (x.Value ?? "NULL")).ToArray());

			//Set up the information message with the URL, the status code, and the parameters.
			var info = "Request to " + baseUrl.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ", parameters: " + parameters;
			var description = "Response content: " + response.Content;

			//Acquire the actual exception
			var ex = (response.ErrorException?.Message) ?? info;

			//Log the exception and info message
			Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.Error(response.ErrorException, "{CommerceCaption}: {ResponseError}", BCCaptions.CommerceLogCaption, description);


		}

		protected void UnLockApiCall(IList<Parameter> header, bool released = false)
		{
			if (header == null)
			{
				lock (obj)
				{
					AvailableCallLimit++;
				}
				//Allow half Api calls to be used
				if (released == false && SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
				{
					SPConnector.ApiCallsPool.Release();
				}
				return;
			}
			var paraLimit = header.FirstOrDefault(x => x.Name.Equals("X-Shopify-Shop-Api-Call-Limit", StringComparison.OrdinalIgnoreCase));
			if (paraLimit != null && paraLimit.Value != null)
			{
				var numStr = paraLimit.Value.ToString().Split('/');
				if (numStr != null && numStr.Length == 2)
				{
					var usedCalls = int.Parse(numStr[0]);
					var totalCalls = int.Parse(numStr[1]);
					lock (obj)
					{
						AvailableCallLimit = totalCalls - usedCalls;
						TotalCallLimit = totalCalls;
						ApiCallLimit = totalCalls / 20;
						ApiTimeLimit = 1000 / ApiCallLimit;
						ControlTimeLimit = AvailableCallLimit <= 1 ? true : false;
					}
				}
				else
				{
					lock (obj)
					{
						AvailableCallLimit++;
					}
				}
			}
			else
			{
				lock (obj)
				{
					AvailableCallLimit++;
				}
			}
			if (released == false && SPConnector.ApiCallsPool != null && SPConnector.ApiCallsPool.CurrentCount < 1)
			{
				SPConnector.ApiCallsPool.Release();
			}
			return;
		}
	}
}