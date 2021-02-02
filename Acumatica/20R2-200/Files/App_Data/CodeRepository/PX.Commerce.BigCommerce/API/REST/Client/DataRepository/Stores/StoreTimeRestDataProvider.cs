using System;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class StoreTimeRestDataProvider :  IRestDataReader<StoreTime>
    {
        private readonly IBigCommerceRestClient _restClient;
        
        public StoreTimeRestDataProvider(IBigCommerceRestClient restClient)
        {
            _restClient = restClient;
		}
        
        public StoreTime Get()
        {
            const string resourceUrl = "v2/time";

            var request = _restClient.MakeRequest(resourceUrl);
			StoreTime store = _restClient.Get<StoreTime>(request);
            return store;
        }       
    }
}
