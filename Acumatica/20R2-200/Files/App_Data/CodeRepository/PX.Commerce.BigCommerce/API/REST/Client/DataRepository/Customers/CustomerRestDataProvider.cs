using PX.Commerce.Core.Model;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class CustomerRestDataProviderV3 : RestDataProviderV3
	{
		protected override string GetListUrl { get; } = "v3/customers";

		protected override string GetSingleUrl { get; } = "v3/customers";

		protected override string GetCountUrl => throw new NotImplementedException();

		public CustomerRestDataProviderV3(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public IEnumerable<CustomerData> GetAll(IFilter filter = null)
		{
			return base.GetAll<CustomerData, CustomerList>(filter);
		}

		public CustomerData Create(CustomerData customer)
		{
			CustomerList resonse = base.Create<CustomerData, CustomerList>(new CustomerData[] { customer }.ToList());
			return resonse?.Data?.FirstOrDefault();
		}

		public CustomerData Update(CustomerData customer)
		{
			CustomerList resonse = base.Update<CustomerData, CustomerList>(new CustomerData[] { customer }.ToList());
			return resonse?.Data?.FirstOrDefault();
		}

	}
}
