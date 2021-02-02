using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
	#region WebHookCustomer
	[JsonObject()]
	public class WebHookCustomer
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookCustomerAddress
	[JsonObject()]
	public class WebHookCustomerAddress
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		[JsonProperty("address")]
		public virtual WebHookCustomerAddressParent Customer { get; set; }

		public override string ToString()
		{
			if(Customer?.CustomerId != null)
				return new Object[] { Customer?.CustomerId, Id }.KeyCombine();
			else
				return Id.ToString();

		}
	}
	[JsonObject()]
	public class WebHookCustomerAddressParent
	{
		[JsonProperty("customer_id")]
		public virtual int? CustomerId { get; set; }
	}
	#endregion
	#region WebHookProduct
	[JsonObject()]
	public class WebHookProduct
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookProductCategory
	[JsonObject()]
	public class WebHookProductCategory
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
	#region WebHookOrder
	[JsonObject()]
	public class WebHookOrder
	{
		[JsonProperty("type")]
		public virtual string Type { get; set; }

		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		public override string ToString()
		{
			return Id?.ToString();
		}
	}
	#endregion
}
