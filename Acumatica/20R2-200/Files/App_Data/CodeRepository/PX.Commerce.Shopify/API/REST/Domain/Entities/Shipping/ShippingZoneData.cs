using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PX.Commerce.Core;
using PX.Commerce.Objects;

namespace PX.Commerce.Shopify.API.REST
{
	public class ShippingZonesResponse : IEntitiesResponse<ShippingZoneData>
	{
		[JsonProperty("shipping_zones")]
		public IEnumerable<ShippingZoneData> Data { get; set; }
	}

	[JsonObject(Description = "Shipping Zone")]
	[Description(ShopifyCaptions.ShippingZone)]
	public class ShippingZoneData : BCAPIEntity, IShippingZone
	{
		public ShippingZoneData()
		{

		}

		/// <summary>
		/// The unique numeric identifier for the shipping zone.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public long? Id { get; set; }

		/// <summary>
		/// The name of the shipping zone, specified by the user.
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public string Name { get; set; }

		/// <summary>
		/// The ID of the shipping zone's delivery profile. Shipping profiles allow merchants to create product-based or location-based shipping rates.
		/// </summary>
		[JsonProperty("profile_id", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public string ProfileId { get; set; }

		/// <summary>
		/// The ID of the shipping zone's location group. 
		/// Location groups allow merchants to create shipping rates that apply only to the specific locations in the group.
		/// </summary>
		[JsonProperty("location_group_id", NullValueHandling = NullValueHandling.Ignore)]
		public string LocationGroupId { get; set; }

		/// <summary>
		/// A list of countries that belong to the shipping zone.
		/// </summary>
		[JsonProperty("countries")]
		public List<CountryData> Countries { get; set; }

		/// <summary>
		/// A list of provinces that belong to the shipping zone.
		/// </summary>
		[JsonProperty("provinces")]
		public List<ProvinceData> Provinces { get; set; }

		/// <summary>
		/// Information about carrier shipping providers and the rates used.
		/// </summary>
		[JsonProperty("carrier_shipping_rate_providers", NullValueHandling = NullValueHandling.Ignore)]
		public List<CarrierShippingRate> CarrierShippingRates { get; set; }

		/// <summary>
		/// Information about a price-based shipping rate.
		/// </summary>
		[JsonProperty("price_based_shipping_rates", NullValueHandling = NullValueHandling.Ignore)]
		public List<PriceBasedShippingRate> PriceBasedShippingRates { get; set; }

		/// <summary>
		/// Information about a weight-based shipping rate.
		/// </summary>
		[JsonProperty("weight_based_shipping_rates", NullValueHandling = NullValueHandling.Ignore)]
		public List<WeightBasedShippingRate> WeightBasedShippingRates { get; set; }

		public string Type { get; set; }
		public bool? Enabled { get; set; } = true;
		public List<IShippingMethod> ShippingMethods { get; set; }
	}

	public class ShippingMehtod : IShippingMethod
	{
		public long? Id { get; set; }
		public string Name { get; set; }
		public string Type { get; set; }
		public bool? Enabled { get; set; }
		public List<string> ShippingServices { get; set; }
	}

	public class PriceBasedShippingRate
	{
		/// <summary>
		/// The unique numeric identifier for the shipping rate.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The full name of the shipping rate.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The unique numeric identifier for the associated shipping zone.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		public long? ShippingZoneId { get; set; }

		/// <summary>
		/// The price of the shipping rate.
		/// </summary>
		[JsonProperty("price")]
		public decimal Price { get; set; }

		/// <summary>
		/// The minimum price of an order for it to be eligible for the shipping rate.
		/// </summary>
		[JsonProperty("min_order_subtotal")]
		public string MinOrderSubtotal { get; set; }

		/// <summary>
		/// The maximum  price of an order for it to be eligible for the shipping rate.
		/// </summary>
		[JsonProperty("max_order_subtotal")]
		public string MaxOrderSubtotal { get; set; }
	}

	public class WeightBasedShippingRate
	{
		/// <summary>
		/// The unique numeric identifier for the shipping rate.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The full name of the shipping rate.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The unique numeric identifier for the associated shipping zone.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		public long? ShippingZoneId { get; set; }

		/// <summary>
		/// The price of the shipping rate.
		/// </summary>
		[JsonProperty("price")]
		public decimal Price { get; set; }

		/// <summary>
		/// The minimum weight  of an order for it to be eligible for the shipping rate.
		/// </summary>
		[JsonProperty("weight_low")]
		public decimal? WeightLow { get; set; }

		/// <summary>
		/// The maximum weight  of an order for it to be eligible for the shipping rate.
		/// </summary>
		[JsonProperty("weight_high")]
		public decimal? WeightHigh { get; set; }
	}

	public class CarrierShippingRate
	{
		/// <summary>
		/// The unique numeric identifier for the shipping rate.
		/// </summary>
		[JsonProperty("id")]
		public long? Id { get; set; }

		/// <summary>
		/// The unique carrier identifier for the shipping rate.
		/// </summary>
		[JsonProperty("carrier_service_id")]
		public long? CarrierServiceId { get; set; }

		/// <summary>
		/// The Flat Modifier
		/// </summary>
		[JsonProperty("flat_modifier")]
		public string FlatModifier { get; set; }

		/// <summary>
		/// The percent modifier.
		/// </summary>
		[JsonProperty("percent_modifier")]
		public string PercentModifier { get; set; }

		/// <summary>
		/// The unique numeric identifier for the associated shipping zone.
		/// </summary>
		[JsonProperty("shipping_zone_id")]
		public long? ShippingZoneId { get; set; }

		/// <summary>
		/// The service filter of the shipping rate.
		/// </summary>
		[JsonProperty("service_filter")]
		public JToken ServiceFilter { get; set; }

		public List<string> GetService()
		{
			var ServiceList = new List<string>();
			DeserializeJson(ServiceFilter, null, ref ServiceList);
			return ServiceList;
		}

		private void DeserializeJson(JToken content, string name, ref List<string> serviceList)
		{
			if (content != null)
			{
				switch (content.Type)
				{
					case JTokenType.Object when content.HasValues:
						{
							foreach (var item in content.Children())
							{
								DeserializeJson(item, name, ref serviceList);
							}
							break;
						}
					case JTokenType.Array when ((JArray)content)?.Count > 0:
						{
							foreach (var arr in ((JArray)content).Children())
							{
								DeserializeJson(arr, name, ref serviceList);
							}
							break;
						}
					case JTokenType.Property:
						{
							var pContent = (JProperty)content;
							DeserializeJson(pContent?.Value, pContent?.Name ?? name, ref serviceList);
							break;
						}
					default:
						{
							var value = ((JValue)content)?.ToString();
							if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(name) && name != "*" && !serviceList.Contains(name))
							{
								serviceList.Add(name);
							}
							break;
						}
				}
			}

		}
	}
}
