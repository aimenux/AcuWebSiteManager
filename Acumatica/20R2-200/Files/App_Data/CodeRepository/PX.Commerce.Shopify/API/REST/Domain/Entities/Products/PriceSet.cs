using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace PX.Commerce.Shopify.API.REST
{
	[Description(ShopifyCaptions.PriceSet)]
	public class PriceSet
	{
		[JsonProperty("shop_money", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.ShopMoney)]
		public PresentmentPrice ShopMoney { get; set; }

		[JsonProperty("presentment_money", NullValueHandling = NullValueHandling.Ignore)]
		[Description(ShopifyCaptions.PresentmentMoney)]
		public PresentmentPrice PresentmentMoney { get; set; }
	}
}
