using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{
	public class WebHookResponse : IEntityResponse<WebHookData>
	{
		[JsonProperty("webhook")]
		public WebHookData Data { get; set; }
	}

	public class WebHooksResponse : IEntitiesResponse<WebHookData>
	{
		[JsonProperty("webhooks")]
		public IEnumerable<WebHookData> Data { get; set; }
	}

	[JsonObject(Description = BCCaptions.WebHook)]
    public class WebHookData
	{
		/// <summary>
		/// Unique numeric identifier for the webhook subscription.
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public virtual long? Id { get; set; }

		/// <summary>
		/// URI where the webhook subscription should send the POST request when the event occurs.
		/// </summary>
		[JsonProperty("address")]
        public virtual string Address { get; set; }

		/// <summary>
		/// The Admin API version that Shopify uses to serialize webhook events. This value is set by the app that created the webhook.
		/// </summary>
		[JsonProperty("api_version", NullValueHandling = NullValueHandling.Ignore)]
		[ShouldNotSerialize]
		public virtual string ApiVersion { get; set; }

		/// <summary>
		/// Format in which the webhook subscription should send the data. Valid values are JSON and XML.
		/// </summary>
		[JsonProperty("format", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string Format { get; set; }

		/// <summary>
		/// Optional array of fields that should be included in the webhook subscription.
		/// </summary>
		[JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string[] Fields { get; set; }

		/// <summary>
		/// Optional array of namespaces for any metafields that should be included with each webhook.
		/// </summary>
		[JsonProperty("metafield_namespaces", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string[] MetaFieldNamespaces { get; set; }

		/// <summary>
		/// Optional array of namespaces for any private metafields that should be included with each webhook.
		/// </summary>
		[JsonProperty("private_metafield_namespaces", NullValueHandling = NullValueHandling.Ignore)]
        public virtual string[] PrivateMetaFieldNamespaces { get; set; }

		/// <summary>
		/// Event that triggers the webhook. Valid values are: 
		/// app/uninstalled, carts/create, carts/update, checkouts/create, checkouts/delete, checkouts/update, 
		/// collection_listings/add, collection_listings/remove, collection_listings/update, collections/create, collections/delete, collections/update, 
		/// customer_groups/create, customer_groups/delete, customer_groups/update, 
		/// customers/create, customers/delete, customers/disable, customers/enable, customers/update, 
		/// draft_orders/create, draft_orders/delete, draft_orders/update, 
		/// fulfillment_events/create, fulfillment_events/delete, fulfillments/create, fulfillments/update, 
		/// inventory_items/create, inventory_items/delete, inventory_items/update, inventory_levels/connect, inventory_levels/disconnect, inventory_levels/update, 
		/// locales/create, locales/update, locations/create, locations/delete, locations/update, 
		/// order_transactions/create, orders/cancelled, orders/create, orders/delete, orders/edited, orders/fulfilled, orders/paid, orders/partially_fulfilled, orders/updated, 
		/// product_listings/add, product_listings/remove, product_listings/update, products/create, products/delete, products/update,
		/// refunds/create, shop/update, tender_transactions/create, themes/create, themes/delete, themes/publish, themes/update
		/// </summary>
		[JsonProperty("topic", NullValueHandling = NullValueHandling.Ignore)]
		public virtual string Topic { get; set; }

		/// <summary>
		/// Date and time when the webhook subscription was created. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("created_at")]
		[ShouldNotSerialize]
        public virtual string DateCreatedAt { get; set; }

		/// <summary>
		/// Date and time when the webhook subscription was updated. The API returns this value in ISO 8601 format.
		/// </summary>
		[JsonProperty("updated_at")]
		[ShouldNotSerialize]
		public virtual string DateModifiedAt { get; set; }

		//Conditional Serialization
		public bool ShouldSerializeId()
        {
            return Id.HasValue && Id > 0;
        }
	}
}
