using RestSharp;

namespace PX.Commerce.BigCommerce.API.REST
{
    public interface IFilter
    {
        void AddFilter(IRestRequest request);
        int? Limit { get; set; }
        int? Page { get; set; }
    }
}
