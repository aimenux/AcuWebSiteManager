using Newtonsoft.Json;
using PX.Commerce.Core;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace PX.Commerce.Shopify.API.REST
{
	public class SPRestClient : SPRestClientBase, IShopifyRestClient
	{
		public SPRestClient(IDeserializer deserializer, ISerializer serializer, IRestOptions options, Serilog.ILogger logger) : base(deserializer, serializer, options, logger)
		{
		}

		#region API Request

		public T Post<T, TR>(IRestRequest request, T obj, bool usingTRasBodyObj = true)
			where T : class, new() 
			where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.POST;
			object _obj = usingTRasBodyObj ? (object)(new TR() { Data = obj }) : (object)obj;
			request.AddBody(_obj);
			var response = ExecuteRequest<TR>(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				T result = response.Data?.Data;
                if (result == null && response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);

			throw new RestException(response);
		}

		public bool Post(IRestRequest request)
		{
			request.Method = Method.POST;
			var response = ExecuteRequest(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				return true;
			}

			LogError(BaseUrl, request, response);

			throw new RestException(response);
		}

		public T Put<T, TR>(IRestRequest request, T obj, bool usingTRasBodyObj = true)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.PUT;
			object _obj = usingTRasBodyObj ? (object)(new TR() { Data = obj }) : (object)obj;
			request.AddBody(_obj);
			var response = ExecuteRequest<TR>(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
			{
				T result = response.Data?.Data;
                if (result == null && response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);

			throw new RestException(response);
		}

		public bool Delete(IRestRequest request)
		{
			request.Method = Method.DELETE;
			var response = ExecuteRequest(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
			{
				return true;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public T Get<T, TR>(IRestRequest request)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			return Get<T, TR>(request, out _);
		}

		public T Get<T, TR>(IRestRequest request, out IList<Parameter> headers)
			where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			request.Method = Method.GET;
			var response = ExecuteRequest<TR>(request);
			headers = response.Headers;
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
			{
				T result = response.Data?.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;
                if(result == null && response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public T GetCount<T>(IRestRequest request)
			where T : class, new()
		{
			request.Method = Method.GET;
			var response = ExecuteRequest<T>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
			{
				return response.Data;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public IEnumerable<T> GetCurrentList<T, TR>(IRestRequest request, out string previousList, out string nextList) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			request.Method = Method.GET;
			var response = ExecuteRequest<TR>(request);
			previousList = nextList = default;
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var responseHeader = response.Headers;
				var entities = response.Data?.Data;
                if (entities == null && response.ErrorException != null)
                {
                    throw response.ErrorException;
                }
                if (entities != null && entities.Any())
				{
					if (TryGetNextPageUrl(responseHeader, out var previousUrl, out var nextUrl))
					{
						previousList = previousUrl;
						nextList = nextUrl;
					}
				}
				return entities;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public IEnumerable<T> GetAll<T, TR>(IRestRequest request) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			request.Method = Method.GET;
            bool needGet = true;
            while(needGet)
            {
                var response = ExecuteRequest<TR>(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseHeader = response.Headers;
                    var entities = response.Data?.Data;
                    if (entities == null && response.ErrorException != null)
                    {
                        throw response.ErrorException;
                    }
                    if (entities != null && entities.Any())
                    {
                        foreach (T entity in entities)
                        {
                            yield return entity;
                        }
                        if (TryGetNextPageUrl(responseHeader, out _, out var nextUrl))
                        {
                            request = MakeRequest(nextUrl);
                            request.Method = Method.GET;
                            needGet = true;
                        }
                        else
                            needGet = false;
                    }
                    else
                        yield break;
                }
                else
                {
                    LogError(BaseUrl, request, response);
                    throw new RestException(response);
                }
            }	
		}

		private bool TryGetNextPageUrl(IList<Parameter> header, out string previousUrl, out string nextUrl)
		{
			previousUrl = nextUrl = default;
			if (header == null || header.Count == 0) return false;
			var paraLink = header.FirstOrDefault(x => x.Name.Equals("Link", StringComparison.InvariantCultureIgnoreCase));
			if (paraLink != null && paraLink.Value != null && !string.IsNullOrWhiteSpace(paraLink.Value.ToString()))
			{
				var linkStr = paraLink.Value.ToString();
				Match previousMatch = Regex.Match(linkStr, $@"<{BaseUrl}([^\s]*)>;\s*rel=""previous""", RegexOptions.IgnoreCase);
				if (previousMatch.Success && !string.IsNullOrWhiteSpace(previousMatch.Groups[1].Value))
				{
					nextUrl = previousMatch.Groups[1].Value;
				}
				Match nextMatch = Regex.Match(linkStr, $@"<{BaseUrl}([^\s]*)>;\s*rel=""next""", RegexOptions.IgnoreCase);
				if (nextMatch.Success && !string.IsNullOrWhiteSpace(nextMatch.Groups[1].Value))
				{
					nextUrl = nextMatch.Groups[1].Value;
					nextUrl = Regex.Replace(nextUrl, $@"limit=\d+", "limit=250");
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}
