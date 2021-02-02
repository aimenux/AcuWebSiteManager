using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class StoreCurrencyDataProvider : IRestDataReader<List<Currency>>
	{
		private readonly IBigCommerceRestClient _restClient;

		public StoreCurrencyDataProvider(IBigCommerceRestClient restClient)
		{
			_restClient = restClient;
		}

		public List<Currency> Get()
		{
			const string resourceUrl = "v2/currencies";
			var request = _restClient.MakeRequest(resourceUrl);
			var store = _restClient.Get<List<Currency>>(request);
			return store;
		}
	}
}
