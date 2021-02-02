using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PX.Commerce.Shopify.API.REST
{
	public class FilterOrders : FilterWithDateTimeAndLimit, IFilterWithIDs, IFilterWithFields, IFilterWithSinceID
	{
		/// <summary>
		/// Restrict results to customers specified by a comma-separated list of IDs.
		/// </summary>
		[Description("ids")]
		public string IDs { get; set; }

		/// <summary>
		/// Restrict results to those after the specified ID.
		/// </summary>
		[Description("since_id")]
		public string SinceID { get; set; }

		/// <summary>
		/// Show only certain fields, specified by a comma-separated list of field names.
		/// </summary>
		[Description("fields")]
		public string Fields { get; set; }

		/// <summary>
		/// Filter results by status.
		/// </summary>
		[Description("status")]
		public OrderStatus? Status { get; set; }

		/// <summary>
		/// Filter results by name.
		/// </summary>
		[Description("name")]
		public string Name { get; set; }

		/// <summary>
		/// Filter results by financial_status.
		/// </summary>
		[Description("financial_status")]
		public string FinancialStatus { get; set; }

		/// <summary>
		/// Filter results by fulfillment_status.
		/// </summary>
		[Description("fulfillment_status")]
		public string FulfillmentStatus { get; set; }

		/// <summary>
		/// Show orders imported at or after date (format: 2014-04-25T16:15:47-04:00).
		/// </summary>
		[Description("processed_at_min")]
		public string ProcessedAtMin { get; set; }

		/// <summary>
		/// Show orders imported at or before date (format: 2014-04-25T16:15:47-04:00).
		/// </summary>
		[Description("processed_at_max")]
		public string ProcessedAtMax { get; set; }
	}
}
