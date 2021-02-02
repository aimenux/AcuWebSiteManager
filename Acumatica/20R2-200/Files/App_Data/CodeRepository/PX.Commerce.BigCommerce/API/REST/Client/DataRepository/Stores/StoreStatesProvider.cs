using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class StoreStatesProvider : IRestDataReader<List<States>>
    {
        private readonly IBigCommerceRestClient _restClient;

        public StoreStatesProvider(IBigCommerceRestClient restClient)
        {
            _restClient = restClient;
        }

        
        public List<States> Get()
        {
            const string resourceUrl = "v2/countries/states";
          
			var filter =  new Filter();
			var needGet = true;

			filter.Page = 1;
			filter.Limit = 250;
			
			List<States> States = new List<States>();
			while (needGet)
			{
				var request = _restClient.MakeRequest(resourceUrl);
				filter?.AddFilter(request);
				var state = _restClient.Get<List<States>>(request);
				States.AddRange(state);
				if(state.Count<250) needGet = false;
				filter.Page++;
			}
			
            return States;
        }

		
	}
}