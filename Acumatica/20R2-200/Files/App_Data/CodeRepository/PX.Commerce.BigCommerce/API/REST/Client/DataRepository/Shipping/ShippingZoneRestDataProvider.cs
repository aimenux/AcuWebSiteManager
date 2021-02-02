using PX.Commerce.Core.Model;
using PX.Commerce.Objects;
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class ShippingZoneDataProvider : IRestDataReader<List<ShippingZone>>
	{
		private readonly IBigCommerceRestClient _restClient;
		protected string GetUrl { get; } = "v2/shipping/zones";
		protected string GetSubUrl { get; } = "v2/shipping/zones/{0}/methods";

		public ShippingZoneDataProvider(IBigCommerceRestClient restClient)
		{
			_restClient = restClient;
		}

		public List<ShippingZone> Get()
		{
			var request = _restClient.MakeRequest(GetUrl);
			var zones = _restClient.Get<List<ShippingZone>>(request);
			if(zones?.Count > 0)
			{
				foreach (ShippingZone zone in zones)
				{
					zone.ShippingMethods = new List<IShippingMethod>();

					request = _restClient.MakeRequest(string.Format(GetSubUrl, zone.Id.ToString()));
					var methods = _restClient.Get<List<ShippingMethod>>(request);
					foreach (ShippingMethod method in methods ?? new List<ShippingMethod>())
					{
						zone.ShippingMethods.Add(method);
						if (method.Settings?.FirstOrDefault()?.CarrierOptions?.DeliveryServices?.Count > 0)
						{
							method.ShippingServices = method.Settings.FirstOrDefault().CarrierOptions.DeliveryServices;
						} 
					}
				}
			}
			return zones;
		}
	}

}
