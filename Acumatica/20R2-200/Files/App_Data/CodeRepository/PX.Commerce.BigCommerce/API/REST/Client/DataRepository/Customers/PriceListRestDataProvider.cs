using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class PriceListRestDataProvider : RestDataProviderV3
	{
		protected override string GetListUrl { get; } = "/v3/pricelists";

		protected override string GetSingleUrl { get; } = "v3/pricelists/{id}";

		protected override string GetCountUrl => throw new NotImplementedException();
		public PriceListRestDataProvider(IBigCommerceRestClient restClient) : base()
		{
			_restClient = restClient;
		}

		public PriceList Create(PriceList priceList)
		{
			return Create<PriceList, PriceListResponse>(priceList)?.Data;
		}

		public IEnumerable<PriceList> GetAll()
		{
			return GetAll<PriceList, PriceListsResponse>();
		}

		public bool DeletePriceList(string priceListId)
		{
			var segment = MakeUrlSegments(priceListId);
			return Delete(segment);
		}
	}

	public class PriceListRecordRestDataProvider : PriceListRestDataProvider
	{
		protected override string GetListUrl { get; } = "v3/pricelists/{parent_id}/records";
		protected override string GetSingleUrl { get; } = "v3/pricelists/{parent_id}/records/{id}/{other_param}";
		public PriceListRecordRestDataProvider(IBigCommerceRestClient restClient) : base(restClient) { }
		public void Upsert(List<PriceListRecord> priceListRecords, string priceListId, Action<ItemProcessCallback<PriceListRecord>> callback)
		{
			var segment = MakeParentUrlSegments(priceListId);
			UpdateAll<PriceListRecord, PriceListRecordResponse>(new PriceListRecordResponse() { Data = priceListRecords }, segment, callback);
		}
		public IEnumerable<PriceListRecord> GetAllRecords(string priceListId, IFilter filter = null)
		{
			var segment = MakeParentUrlSegments(priceListId);
			return GetAll<PriceListRecord, PriceListRecordResponse>(filter: filter, urlSegments: segment);
		}

		public bool DeleteRecords(string priceListId, string id, string currency)
		{
			var segment = MakeUrlSegments(id, priceListId, currency);
			return Delete(segment);
		}
	}

}