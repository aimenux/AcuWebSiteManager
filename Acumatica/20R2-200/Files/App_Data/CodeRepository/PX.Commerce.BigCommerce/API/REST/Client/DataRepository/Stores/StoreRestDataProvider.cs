using System;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class StoreRestDataProvider :  IRestDataReader<Store>
    {
        private readonly IBigCommerceRestClient _restClient;
        
        public StoreRestDataProvider(IBigCommerceRestClient restClient)
        {
            _restClient = restClient;
		}
        
        public Store Get()
        {
            const string resourceUrl = "v2/store";
            var request = _restClient.MakeRequest(resourceUrl);
            var store = _restClient.Get<Store>(request);
            return store;
        }       
    }
}
