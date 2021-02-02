using PX.Commerce.Core.Model;
using PX.Commerce.Objects;
using System;
using System.Collections.Generic;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class TaxDataProvider : RestDataProviderV3 
	{
		protected override string GetListUrl { get; } = "v2/tax_classes";

		protected override string GetSingleUrl { get; } = "v2/tax_classes/{id}";

		protected override string GetCountUrl { get; } = string.Empty;

		public TaxDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}
		public ProductsTax GetByID(int id)
		{

			var segments = MakeUrlSegments(id.ToString());
			var result = base.GetByID<ProductsTaxData, ProductsTax>(segments);
			return result;
		}
		public List<ProductsTaxData> GetAll()
		{
			var request = _restClient.MakeRequest(GetListUrl);
			var result = _restClient.Get<List<ProductsTaxData>>(request);
			result.Add(new ProductsTaxData() { Id = 0, Name = BCObjectsConstants.DefaultTaxClass });
			return result;
		}


		#region Not Implemented
		public ProductsVideo Update(ProductsVideo productsVideo, string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public int Count(string parentId)
		{
			throw new NotImplementedException();
		}


		public bool Delete(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		public List<ProductsVideo> Get(string parentId)
		{
			throw new NotImplementedException();
		}

		public ProductsVideo GetByID(string id, string parentId)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
