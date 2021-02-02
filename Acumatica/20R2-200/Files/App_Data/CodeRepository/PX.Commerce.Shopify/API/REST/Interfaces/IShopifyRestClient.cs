using System.Collections.Generic;
using PX.Commerce.Core;
using RestSharp;

namespace PX.Commerce.Shopify.API.REST
{
    public interface IShopifyRestClient
    {
		/// <summary>
		/// Post data to Shopify
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="request"></param>
		/// <param name="obj">The data obj that posts to Shopify API, it should be a T object</param>
		/// <param name="usingTRasBodyObj">True : system will auto convert T data obj to TR response obj first, and then posts to Shopify API; False : system will post T data obj to Shopify API directly. Please follow the Shopify API documents to determine this value, default is true.</param>
		/// <returns>The response data from Shopify API, it is a TR object</returns>
		T Post<T, TR>(IRestRequest request, T obj, bool usingTRasBodyObj = true) where T : class, new() where TR : class, IEntityResponse<T>, new();

		/// <summary>
		/// Update data to Shopify
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TR"></typeparam>
		/// <param name="request"></param>
		/// <param name="obj">The data obj that udpates to Shopify API, it should be a T object</param>
		/// <param name="usingTRasBodyObj">True : system will auto convert T data obj to TR response obj first, and then posts to Shopify API; False : system will post T data obj to Shopify API directly. Please follow the Shopify API documents to determine this value, default is true.</param>
		/// <returns>The response data from Shopify API, it is a TR object</returns>
		T Put<T, TR>(IRestRequest request, T obj, bool usingTRasBodyObj = true) where T : class, new() where TR : class, IEntityResponse<T>, new();
		bool Delete(IRestRequest request);
        T Get<T, TR>(IRestRequest request) where T : class, new() where TR : class, IEntityResponse<T>, new();
		T Get<T, TR>(IRestRequest request, out IList<Parameter> responseHeader) where T : class, new() where TR : class, IEntityResponse<T>, new();
		T GetCount<T>(IRestRequest request) where T : class, new();
		IEnumerable<T> GetCurrentList<T, TR>(IRestRequest request, out string previousList, out string nextList) where T : class, new() where TR : class, IEntitiesResponse<T>, new();
        IEnumerable<T> GetAll<T, TR>(IRestRequest request) where T : class, new() where TR : class, IEntitiesResponse<T>, new();
		RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null);
		bool Post(IRestRequest request);
		Serilog.ILogger Logger { set; get; }
	}
}