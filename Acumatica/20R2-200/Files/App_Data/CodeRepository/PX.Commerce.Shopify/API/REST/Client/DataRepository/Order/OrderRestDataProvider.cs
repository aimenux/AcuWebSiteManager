using Newtonsoft.Json;
using PX.Commerce.Core;
using PX.Commerce.Core.Model;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Commerce.Shopify.API.REST
{
	public class OrderRestDataProvider : RestDataProviderBase, IParentRestDataProvider<OrderData>
	{
		protected override string GetListUrl { get; } = "orders.json";
		protected override string GetSingleUrl { get; } = "orders/{id}.json";
		protected override string GetCountUrl { get; } = "orders/count.json";
		protected override string GetSearchUrl => throw new NotImplementedException();
		private string GetMetafieldsUrl { get; } = "orders/{id}/metafields.json";
		private string GetCloseOrderUrl { get; } = "orders/{id}/close.json";
		private string GetCancelOrderUrl { get; } = "orders/{id}/cancel.json";
		private string GetReOpenOrderUrl { get; } = "orders/{id}/open.json";
		private string GetTransactionsUrl { get; } = "orders/{id}/transactions.json";
		private string GetSingleTransactionUrl { get; } = "orders/{parent_id}/transactions/{id}.json";
		private string GetCustomerUrl { get; } = "customers/{id}.json";
		private string GetOrderRiskUrl { get; } = "orders/{id}/risks.json";
		private string GetOrderRefundsurl { get; } = "orders/{id}/refunds.json";
		private string GetSingleRefundsUrl { get; } = "orders/{parent_id}/refunds/{id}.json";

		public OrderRestDataProvider(IShopifyRestClient restClient) : base()
		{
			ShopifyRestClient = restClient;
		}

		public OrderData Create(OrderData entity)
		{
			return base.Create<OrderData, OrderResponse>(entity);
		}

		public OrderData Update(OrderData entity) => Update(entity, entity.Id.ToString());
		public OrderData Update(OrderData entity, string id)
		{
			var segments = MakeUrlSegments(id);
			return base.Update<OrderData, OrderResponse>(entity, segments);
		}

		public bool Delete(OrderData entity, string id) => Delete(id);

		public bool Delete(string id)
		{
			var segments = MakeUrlSegments(id);
			return Delete(segments);
		}

		public IEnumerable<OrderData> GetCurrentList(out string previousList, out string nextList, IFilter filter = null)
		{
			return GetCurrentList<OrderData, OrdersResponse>(out previousList, out nextList, filter);
		}

		public IEnumerable<OrderData> GetAll(IFilter filter = null)
		{
			return GetAll<OrderData, OrdersResponse>(filter);
		}

		public OrderData GetByID(string id) => GetByID(id, false, false, false, false);

		public OrderData GetByID(string id, bool includedMetafields = false, bool includedTransactions = false, bool includedCustomer = true, bool includedOrderRisk = false)
		{
			var segments = MakeUrlSegments(id);
			var entity = base.GetByID<OrderData, OrderResponse>(segments);
            if (entity != null)
            {
                if (includedTransactions == true)
                    entity.Transactions = GetOrderTransactions(id);
                if (includedCustomer == true && entity.Customer != null && entity.Customer.Id > 0)
                {
                    entity.Customer = GetOrderCustomer(entity.Customer.Id.ToString());
                }
                if (includedMetafields == true)
                    entity.Metafields = GetMetafieldsById(id);
                if (includedOrderRisk == true)
                    entity.OrderRisks = GetOrderRisks(id);
            }
			return entity;
		}

		public ItemCount Count()
		{
			return base.GetCount();
		}

		public ItemCount Count(IFilter filter)
		{
			return base.GetCount(filter);
		}

		public List<MetafieldData> GetMetafieldsById(string id)
		{
			var request = BuildRequest(GetMetafieldsUrl, nameof(GetMetafieldsById), MakeUrlSegments(id), null);
			return ShopifyRestClient.GetAll<MetafieldData, MetafieldsResponse>(request).ToList();
		}

		public OrderData CloseOrder(string orderId)
		{
			var request = BuildRequest(GetCloseOrderUrl, nameof(CloseOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public OrderData ReopenOrder(string orderId)
		{
			var request = BuildRequest(GetReOpenOrderUrl, nameof(ReopenOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public OrderData CancelOrder(string orderId)
		{
			var request = BuildRequest(GetCancelOrderUrl, nameof(CancelOrder), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderData, OrderResponse>(request, null, false);
		}

		public List<OrderTransaction> GetOrderTransactions(string orderId)
		{
			var request = BuildRequest(GetTransactionsUrl, nameof(GetOrderTransactions), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.GetAll<OrderTransaction, OrderTransactionsResponse>(request).ToList();
		}

		public List<OrderRefund> GetOrderRefunds(string orderId)
		{
			var request = BuildRequest(GetOrderRefundsurl, nameof(GetOrderRefunds), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.GetAll<OrderRefund, OrderRefundsResponse>(request).ToList();
		}
		public OrderRefund GetOrderSingleRefund(string orderId, string refundID)
		{
			var request = BuildRequest(GetSingleRefundsUrl, nameof(GetOrderSingleRefund), MakeUrlSegments(refundID, orderId), null);
			return ShopifyRestClient.Get<OrderRefund, OrderRefundResponse>(request);
		}
		public OrderTransaction GetOrderSingleTransaction(string orderId, string transactionId)
		{
			var request = BuildRequest(GetSingleTransactionUrl, nameof(GetOrderSingleTransaction), MakeUrlSegments(transactionId, orderId), null);
			return ShopifyRestClient.Get<OrderTransaction, OrderTransactionResponse>(request);
		}

		public CustomerData GetOrderCustomer(string orderId)
		{
			var request = BuildRequest(GetCustomerUrl, nameof(GetOrderCustomer), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Get<CustomerData, CustomerResponse>(request);
		}

		public List<OrderRisk> GetOrderRisks(string orderId)
		{
			var request = BuildRequest(GetMetafieldsUrl, nameof(GetOrderRisks), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.GetAll<OrderRisk, OrderRisksResponse>(request).ToList();
		}

		public OrderTransaction PostPaymentToCapture(OrderTransaction entity, string orderId)
		{
			var request = BuildRequest(GetTransactionsUrl, nameof(PostPaymentToCapture), MakeUrlSegments(orderId), null);
			return ShopifyRestClient.Post<OrderTransaction, OrderTransactionResponse>(request, entity);
		}


	}
}
