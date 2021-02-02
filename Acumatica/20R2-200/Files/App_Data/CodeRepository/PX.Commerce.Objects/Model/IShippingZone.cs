using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	public interface IShippingZone
	{
		string Name { get; set; }
		string Type { get; set; }
		bool? Enabled { get; set; }

		List<IShippingMethod> ShippingMethods { get; set; }
	}

	public interface IShippingMethod
	{
		string Name { get; set; }
		string Type { get; set; }
		bool? Enabled { get; set; }

		List<String> ShippingServices { get; set; }
	}
}
