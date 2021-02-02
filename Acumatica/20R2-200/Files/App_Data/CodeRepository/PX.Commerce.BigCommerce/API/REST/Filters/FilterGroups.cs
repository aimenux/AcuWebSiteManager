using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.BigCommerce.API.REST
{
	public class FilterGroups :Filter
	{

		[Description("name")]
		public string Name { get; set; }
	}
}
