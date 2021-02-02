using Newtonsoft.Json;
using PX.Commerce.Core;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class OrderStatus : BCAPIEntity
	{
		[JsonProperty("id")]
		public virtual int? Id { get; set; }

		[JsonProperty("status_id")]
		public virtual int StatusId { get; set; }

		[JsonProperty("date_modified")]
		public virtual string DateModifiedUT { get; set; }
	}
}