using PX.Commerce.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class CustomerPriceClassRestDataProvider : RestDataProviderV2
	{
		protected override string GetListUrl  { get; } = "v2/customer_groups";

		protected override string GetSingleUrl { get; } = "v2/customer_groups/{id}";

		protected override string GetCountUrl { get; } = "v2/customer_groups/count";
		public CustomerPriceClassRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public CustomerGroupData Create(CustomerGroupData group)
		{
			var newGroup = Create<CustomerGroupData>(group);
			return newGroup;
		}

		public CustomerGroupData Update(CustomerGroupData group, string id)
		{
			var segments = MakeUrlSegments(id);
			return Update(group, segments);
		}

		public IEnumerable<CustomerGroupData> GetAll(IFilter filter = null)
		{
			return base.GetAll<CustomerGroupData>(filter);
		}

		public CustomerGroupData GetByID(string id)
		{
			var segments = MakeUrlSegments(id);
			return GetByID<CustomerGroupData>(segments);
		}
	}
}
