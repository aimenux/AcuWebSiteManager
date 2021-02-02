using PX.Common;
using PX.Commerce.Core;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class BCRestClient : BCRestClientBase, IBigCommerceRestClient
	{
		public BCRestClient(IDeserializer deserializer, ISerializer serializer, IRestOptions options, Serilog.ILogger logger) : base(deserializer, serializer, options, logger)
		{
		}

		#region API version 2
		public T Post<T>(IRestRequest request, T obj)
			where T : class, new()
		{
			request.Method = Method.POST;
			request.AddBody(obj);
			var response = Execute<T>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new Exception($"Cannot insert {obj.GetType().Name}");
			}

			throw new RestException(response);
		}

		public T Put<T>(IRestRequest request, T obj)
			where T : class, new()
		{
			request.Method = Method.PUT;
			request.AddBody(obj);

			var response = Execute<T>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);

			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				throw new Exception($"Cannot update {obj.GetType().Name}");
			}

			throw new RestException(response);
		}

		public bool Delete(IRestRequest request)
		{
			request.Method = Method.DELETE;
			var response = Execute(request);
			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				return true;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public T Get<T>(IRestRequest request) 
			where T : class, new()
		{
			request.Method = Method.GET;
			var response = Execute<T>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				T result = response.Data;

				if (result != null && result is BCAPIEntity) (result as BCAPIEntity).JSON = response.Content;
				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(BaseUrl, request, response);

			if (response.StatusCode == HttpStatusCode.InternalServerError && string.IsNullOrEmpty(response.Content))
			{
				throw new Exception(BigCommerceMessages.InternalServerError);
			}
			throw new RestException(response);
		}
		#endregion

		#region API version 3
		public TE Post<T, TE>(IRestRequest request, T entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.POST;
			request.AddBody(entity);
			IRestResponse<TE> response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Post<T, TE>(IRestRequest request, List<T> entities)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.POST;
			request.AddBody(entities);
			IRestResponse<TE> response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is IEnumerable<BCAPIEntity>) (result?.Data as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Post<T, TE>(IRestRequest request, TE entity) 
			where T : class, new() 
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.POST;
			request.AddBody(entity.Data);
			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE Put<T, TE>(IRestRequest request, T entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.PUT;
			request.AddBody(entity);

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Put<T, TE>(IRestRequest request, List<T> entities)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.PUT;
			request.AddBody(entities);

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is IEnumerable<BCAPIEntity>) (result?.Data as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE Put<T, TE>(IRestRequest request, TE entity)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.PUT;
			request.AddBody(entity);

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE PutList<T, TE>(IRestRequest request, TE entity)
			where T : class, new()
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.PUT;
			request.AddBody(entity.Data);

			var response = Execute<TE>(request);
			if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}

		public TE Get<T, TE>(IRestRequest request)
			where T : class, new()
			where TE : IEntityResponse<T>, new()
		{
			request.Method = Method.GET;
			var response = Execute<TE>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.NotFound)
			{
				TE result = response.Data;

				if (result?.Data != null && result?.Data is BCAPIEntity) (result?.Data as BCAPIEntity).JSON = response.Content;

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		public TE GetList<T, TE>(IRestRequest request) 
			where T : class, new() 
			where TE : IEntitiesResponse<T>, new()
		{
			request.Method = Method.GET;
			var response = Execute<TE>(request);

			if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent)
			{
				TE result = response.Data;

				//if (result != null && result is IEnumerable<BCAPIEntity>) (result as IEnumerable<BCAPIEntity>).ForEach(e => e.JSON = response.Content);

				return result;
			}

			LogError(BaseUrl, request, response);
			throw new RestException(response);
		}
		#endregion
	}
}
