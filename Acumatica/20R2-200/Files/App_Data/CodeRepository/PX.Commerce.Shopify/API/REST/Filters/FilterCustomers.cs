using System;
using System.ComponentModel;

namespace PX.Commerce.Shopify.API.REST
{
	public class FilterCustomers : FilterWithDateTimeAndLimit, IFilterWithIDs, IFilterWithFields, IFilterWithSinceID
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
	}
}
