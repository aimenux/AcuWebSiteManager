using System.Collections.Generic;
using Newtonsoft.Json;

namespace PX.Commerce.BigCommerce.API.REST
{
    [JsonObject(Description = "Product (total  BigCommerce API v3 response)")]
    public class Product : IEntityResponse<ProductData>
    {
        [JsonProperty("data")]
        public ProductData Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    [JsonObject(Description = "Product list (total  BigCommerce API v3 response)")]
    public class ProductList : IEntitiesResponse<ProductData>
    {
        private List<ProductData> _data;

        [JsonProperty("data")]
        public List<ProductData> Data
        {
            get => _data ?? (_data = new List<ProductData>());
            set => _data = value;
        }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

	[JsonObject(Description = "Product list (total  BigCommerce API v3 response)")]
	public class ProductQtyList : IEntitiesResponse<ProductQtyData>
	{
		private List<ProductQtyData> _data;

		[JsonProperty("data")]
		public List<ProductQtyData> Data
		{
			get => _data ?? (_data = new List<ProductQtyData>());
			set => _data = value;
		}

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}

	[JsonObject(Description = "Related Products List (total  BigCommerce API v3 response)")]
	public class RelatedProductsList : IEntitiesResponse<RelatedProductsData>
	{
		private List<RelatedProductsData> _data;

		[JsonProperty("data")]
		public List<RelatedProductsData> Data
		{
			get => _data ?? (_data = new List<RelatedProductsData>());
			set => _data = value;
		}

		[JsonProperty("meta")]
		public Meta Meta { get; set; }
	}
}




