using System;
using System.ComponentModel;

namespace PX.Commerce.BigCommerce.API.REST
{
    public class FilterCustomers : Filter
    {
        [Description("id:in")]
        public string Id { get; set; }
      
        [Description("date_modified:min")]
        public DateTime? MinDateModified { get; set; }

		[Description("date_modified:max")]
		public DateTime? MaxDateModified { get; set; }

		[Description("email:in")]
		public string Email { get; set; }

		[Description("include")]
		public string Include { get; set; }
	}
}
