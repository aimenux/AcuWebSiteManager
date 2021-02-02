using PX.Commerce.Core;
using System;
using System.Collections.Generic;

namespace PX.Commerce.Shopify.API.REST
{
	public class FulfillmentRestDataProvider : RestDataProviderBase, IChildRestDataProvider<FulfillmentData>
	{
		protected override string GetListUrl { get; } = "orders/{parent_id}/fulfillments.json";
		protected override string GetSingleUrl { get; } = "orders/{parent_id}/fulfillments/{id}.json";
		protected override string GetCountUrl { get; } = "orders/{parent_id}/fulfillments/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetCompleteUrl { get; } = "orders/{parent_id}/fulfillments/{id}/complete.json";
		private string GetUpdateTrackingUrl { get; } = "orders/{parent_id}/fulfillments/{id}/update_tracking.json";
		private string GetOpenUrl { get; } = "orders/{parent_id}/fulfillments/{id}/open.json";
		private string GetCancelUrl { get; } = "orders/{parent_id}/fulfillments/{id}/cancel.json";

		public FulfillmentRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public FulfillmentData Create(FulfillmentData entity, string orderId)
		{
			var segments = MakeParentUrlSegments(orderId);
			return base.Create<FulfillmentData, FulfillmentResponse>(entity, segments);
		}

		public FulfillmentData Update(FulfillmentData entity, string orderId, string fulfillmentId)
		{
			var segments = MakeUrlSegments(fulfillmentId, orderId);
			return Update<FulfillmentData, FulfillmentResponse>(entity, segments);
		}

		public bool Delete(string orderId, string fulfillmentId)
		{
			var segments = MakeUrlSegments(fulfillmentId, orderId);
			return Delete(segments);
		}

		public int Count(string orderId)
		{
			var segments = MakeParentUrlSegments(orderId);
			return base.GetCount(segments).Count;
		}

		public IEnumerable<FulfillmentData> GetCurrentList(string orderId, out string previousList, out string nextList, IFilter filter = null)
		{
			var segments = MakeParentUrlSegments(orderId);
			return GetCurrentList<FulfillmentData, FulfillmentsResponse>(out previousList, out nextList, filter, segments);
		}

		public IEnumerable<FulfillmentData> GetAll(string orderId, IFilter filter = null)
		{
			var segments = MakeParentUrlSegments(orderId);
			return GetAll<FulfillmentData, FulfillmentsResponse>(filter, segments);
		}

		public FulfillmentData GetByID(string orderId, string fulfillmentId)
		{
			var segments = MakeUrlSegments(fulfillmentId, orderId);
			return GetByID<FulfillmentData, FulfillmentResponse>(segments);
		}

		public IEnumerable<FulfillmentData> GetAllWithoutParent(IFilter filter = null)
		{
			throw new NotImplementedException();
		}

		public FulfillmentData CompleteFulfillment(string orderId, string fulfillmentId)
		{
			var request = BuildRequest(GetCompleteUrl, nameof(CompleteFulfillment), MakeUrlSegments(fulfillmentId, orderId), null);
			return ShopifyRestClient.Post<FulfillmentData, FulfillmentResponse>(request, null, false);
		}

		public FulfillmentData UpdateFulfillmentTracking(FulfillmentData entity, string orderId, string fulfillmentId)
		{
			var request = BuildRequest(GetUpdateTrackingUrl, nameof(UpdateFulfillmentTracking), MakeUrlSegments(fulfillmentId, orderId), null);
			return ShopifyRestClient.Post<FulfillmentData, FulfillmentResponse>(request, entity);
		}

		public FulfillmentData OpenFulfillment(string orderId, string fulfillmentId)
		{
			var request = BuildRequest(GetOpenUrl, nameof(OpenFulfillment), MakeUrlSegments(fulfillmentId, orderId), null);
			return ShopifyRestClient.Post<FulfillmentData, FulfillmentResponse>(request, null, false);
		}

		public FulfillmentData CancelFulfillment(string orderId, string fulfillmentId)
		{
			var request = BuildRequest(GetCancelUrl, nameof(CancelFulfillment), MakeUrlSegments(fulfillmentId, orderId), null);
			return ShopifyRestClient.Post<FulfillmentData, FulfillmentResponse>(request,new FulfillmentData() { }, false);
		}
	}
}
