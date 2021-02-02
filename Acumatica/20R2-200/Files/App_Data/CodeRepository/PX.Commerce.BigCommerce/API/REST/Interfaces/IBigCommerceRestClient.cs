using System.Collections.Generic;
using PX.Commerce.Core;
using RestSharp;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IBigCommerceRestClient
    {
        T Post<T>(IRestRequest request, T obj) where T : class, new();
        T Put<T>(IRestRequest request, T obj) where T : class, new();
        bool Delete(IRestRequest request);
        T Get<T>(IRestRequest request) where T : class, new();   
		
        RestRequest MakeRequest(string url, Dictionary<string, string> urlSegments = null);

		TE Post<T, TE>(IRestRequest request, T entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Post<T, TE>(IRestRequest request, TE entity) where T : class, new() where TE: IEntityResponse<T>, new();
		TE Post<T, TE>(IRestRequest request, List<T> entities) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE Put<T, TE>(IRestRequest request, T entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Put<T, TE>(IRestRequest request, TE entity) where T : class, new() where TE : IEntityResponse<T>, new();
		TE Put<T, TE>(IRestRequest request, List<T> entities) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE PutList<T, TE>(IRestRequest request, TE entity) where T : class, new() where TE : IEntitiesResponse<T>, new();
		TE Get<T, TE>(IRestRequest request) where T : class, new() where TE: IEntityResponse<T>, new();
		TE GetList<T, TE>(IRestRequest request) where T : class, new() where TE : IEntitiesResponse<T>, new();
		Serilog.ILogger Logger { set; get; }
	}
}