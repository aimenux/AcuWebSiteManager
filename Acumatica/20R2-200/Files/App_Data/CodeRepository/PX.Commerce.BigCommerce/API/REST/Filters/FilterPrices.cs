using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST.Filters
{
	public class FilterPrices : Filter
	{
		[Description("include")]
		public string Include { get; set; }

	}
}