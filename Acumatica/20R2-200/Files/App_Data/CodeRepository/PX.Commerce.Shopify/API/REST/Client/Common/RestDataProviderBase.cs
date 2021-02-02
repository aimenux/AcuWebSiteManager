using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using PX.Data;
using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
	public abstract class RestDataProviderBase
	{
		protected const string ID_STRING = "id";
		protected const string PARENT_ID_STRING = "parent_id";
		protected const string ApiPrefix = "api";

		protected IShopifyRestClient ShopifyRestClient;
		//Default Shopify API version, if you want to use different API version, please overwrite in your detail DataProvider
		protected string ApiVersion { get => ShopifyCaptions.ApiVersion_202001; }  
		protected abstract string GetListUrl { get; }
		protected abstract string GetSingleUrl { get; }
		protected abstract string GetCountUrl { get; }
		protected abstract string GetSearchUrl { get; }
		public RestDataProviderBase()
		{

		}
		public virtual T Create<T, TR>(T entity, UrlSegments urlSegments = null) where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			var request = BuildRequest(GetListUrl, nameof(this.Create), urlSegments, null);
			ShopifyRestClient.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Object", entity)
				.Verbose("{CommerceCaption}: creating new {EntityType} entry", BCCaptions.CommerceLogCaption, entity.GetType().ToString());
			HandleRequesetHeader<T>(request);
			return ShopifyRestClient.Post<T, TR>(request, entity);
		}

		public virtual T Update<T, TR>(T entity, UrlSegments urlSegments) where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			var request = BuildRequest(GetSingleUrl, nameof(this.Update), urlSegments, null);
			ShopifyRestClient.Logger?.ForContext("Scope", new BCLogTypeScope(GetType()))
				.ForContext("Object", entity)
				.Verbose("{CommerceCaption}: updating {EntityType} entry with an ID", BCCaptions.CommerceLogCaption, entity.GetType().ToString());
			HandleRequesetHeader<T>(request);
			return ShopifyRestClient.Put<T, TR>(request, entity);
		}

		public virtual bool Delete(UrlSegments urlSegments)
		{
			var request = BuildRequest(GetSingleUrl, nameof(this.Delete), urlSegments, null);
			return ShopifyRestClient.Delete(request);
		}

		public virtual ItemCount GetCount(UrlSegments urlSegments = null) => GetCount(null, urlSegments);

		public virtual ItemCount GetCount(IFilter filter, UrlSegments urlSegments = null)
		{
			var request = BuildRequest(GetCountUrl, nameof(this.GetCount), urlSegments, filter);
			var count = ShopifyRestClient.GetCount<ItemCount>(request);
			return count;
		}

		public virtual T GetByID<T, TR>(UrlSegments urlSegments) where T : class, new() where TR : class, IEntityResponse<T>, new()
		{
			var request = BuildRequest(GetSingleUrl, nameof(this.GetByID), urlSegments, null);
			var entity = ShopifyRestClient.Get<T, TR>(request);
			return entity;
		}

		public virtual IEnumerable<T> GetCurrentList<T, TR>(out string previousList, out string nextList, IFilter filter = null, UrlSegments urlSegments = null) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			var request = BuildRequest(GetListUrl, nameof(this.GetCurrentList), urlSegments, filter);
			var entities = ShopifyRestClient.GetCurrentList<T, TR>(request,out previousList, out nextList);
			return entities;
		}

		public virtual IEnumerable<T> GetAll<T, TR>(IFilter filter = null, UrlSegments urlSegments = null) where T : class, new() where TR : class, IEntitiesResponse<T>, new()
		{
			var request = BuildRequest(GetListUrl, nameof(this.GetAll), urlSegments, filter);
			var entities = ShopifyRestClient.GetAll<T, TR>(request);
			return entities;
		}

		protected static UrlSegments MakeUrlSegments(long id) => MakeUrlSegments(id.ToString());

		protected static UrlSegments MakeUrlSegments(string id)
		{
			var segments = new UrlSegments();
			segments.Add(ID_STRING, id);
			return segments;
		}

		protected static UrlSegments MakeParentUrlSegments(long parentId) => MakeParentUrlSegments(parentId.ToString());

		protected static UrlSegments MakeParentUrlSegments(string parentId)
		{
			var segments = new UrlSegments();
			segments.Add(PARENT_ID_STRING, parentId);
			return segments;
		}

		protected static UrlSegments MakeUrlSegments(long id, long parentId) => MakeUrlSegments(id.ToString(), parentId.ToString());

		protected static UrlSegments MakeUrlSegments(string id, string parentId)
		{
			var segments = new UrlSegments();
			segments.Add(PARENT_ID_STRING, parentId);
			segments.Add(ID_STRING, id);
			return segments;
		}

		protected void ValidationUrl(string url, string methodName)
		{
			if (string.IsNullOrWhiteSpace(url))
				throw new PXNotSupportedException(ShopifyMessages.DataProviderNotSupportMethod, this.GetType().Name, methodName);
		}

		protected virtual string BuildUrl(string url)
		{
			return ApiPrefix + "/" + ApiVersion + "/" + url.TrimStart('/');
		}

		protected void HandleRequesetHeader<T>(IRestRequest request) where T : class
		{
			foreach (var propertyInfo in typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
			{
				var attr = propertyInfo.GetAttribute<ApiHeaderRequestAttribute>();
				if (attr == null) continue;
				String name = attr.HeaderParameterName;
				String value = attr.HeaderParameterValue;
				if (!string.IsNullOrEmpty(name))
				{
					request.AddHeader(name, value);
				}
			}
		}

		protected RestRequest BuildRequest(string url, string methodName, UrlSegments urlSegments = null, IFilter filter = null)
		{
			var builtUrl = BuildUrl(url);
			ValidationUrl(builtUrl, methodName);
			var request = ShopifyRestClient.MakeRequest(builtUrl, urlSegments?.GetUrlSegments());
			if(filter != null)
				filter?.AddFilter(request);
			return request;
		}
	}

}