using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class PaymentMethodsRestDataProvider : IRestDataReader<List<PaymentMethods>>
	{
		private readonly IBigCommerceRestClient _restClient;

		public PaymentMethodsRestDataProvider(IBigCommerceRestClient restClient)
		{
			_restClient = restClient;
		}

		public List<PaymentMethods> Get()
		{
			const string resourceUrl = "v2/payments/methods";
			var request = _restClient.MakeRequest(resourceUrl);
			var store = _restClient.Get<List<PaymentMethods>>(request);
			return store;
		}


	}
}
