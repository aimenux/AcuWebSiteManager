using PX.Commerce.Core.Model;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ShippingMethodDataProvider : RestDataProviderV2, IChildRestDataProvider<ShippingMethod>
	{
		protected override string GetListUrl { get; } = "v2/shipping/zones/{parent_id}/methods";
		protected override string GetSingleUrl { get; } = "v2/shipping/zones/{parent_id}/methods/{id}";
		protected override string GetCountUrl { get; } = "";

		public ShippingMethodDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		#region IParentDataRestClient

		public ShippingMethod Create(ShippingMethod entity, string parentId)
		{
			throw new NotImplementedException();
		}

		public ShippingMethod Update(ShippingMethod entity, string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public List<ShippingMethod> Get(string parentId)
		{
			if (string.IsNullOrEmpty(parentId))
				return null;
			var segments = MakeParentUrlSegments(parentId);
			return Get<ShippingMethod>(null, segments);
		}

		public ShippingMethod GetByID(string id, string parentId)
		{
			if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(parentId))
				return null;
			var segments = MakeUrlSegments(id, parentId);
			return GetByID<ShippingMethod>(segments);
		}

		public bool Delete(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public int Count(string parentId)
		{
			throw new NotImplementedException();
		}
		#endregion
	}

}
