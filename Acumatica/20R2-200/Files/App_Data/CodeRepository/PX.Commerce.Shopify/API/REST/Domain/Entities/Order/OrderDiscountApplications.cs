using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.Shopify.API.REST
{

	[JsonObject(Description = "discount_applications")]
	public class OrderDiscountApplications : BCAPIEntity
	{
		/// <summary>
		/// The method by which the discount application value has been allocated to entitled lines. 
		/// </summary>
		[JsonProperty("allocation_method")]
		public DiscountAllocationMethods? AllocationMethod { get; set; }

		/// <summary>
		/// The discount code that was used to apply the discount. Available only for discount code applications.
		/// </summary>
		[JsonProperty("code")]
		public string Code { get; set; }

		/// <summary>
		/// The description of the discount application, as defined by the merchant or the Shopify Script. Available only for manual and script discount applications.
		/// </summary>
		[JsonProperty("description")]
		public string Description { get; set; }

		/// <summary>
		/// The lines on the order, of the type defined by target_type, that the discount is allocated over. 
		/// </summary>
		[JsonProperty("target_selection")]
		public DiscountTargetSelection? TargetSelection { get; set; }

		/// <summary>
		/// The type of line on the order that the discount is applicable on.
		/// </summary>
		[JsonProperty("target_type")]
		public DiscountTargetType? TargetType { get; set; }

		/// <summary>
		/// The title of the discount application, as defined by the merchant. Available only for manual discount applications.
		/// </summary>
		[JsonProperty("title")]
		public string Title { get; set; }

		/// <summary>
		/// The discount application type.
		/// </summary>
		[JsonProperty("type")]
		public DiscountApplicationType? Type { get; set; }

		/// <summary>
		/// The value of the discount application as a decimal. This represents the intention of the discount application. 
		/// For example, if the intent was to apply a 20% discount, then the value will be 20.0. If the intent was to apply a $15 discount, then the value will be 15.0.
		/// </summary>
		[JsonProperty("value")]
		public string Value { get; set; }

		/// <summary>
		/// The type of the value.
		/// </summary>
		[JsonProperty("value_type")]
		public DiscountValueType? ValueType { get; set; }
	}

}
