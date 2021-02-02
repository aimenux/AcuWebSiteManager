using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Commerce.Objects
{
	public interface IPaymentMethod
	{
		string Name { get; set; }
		bool CreatePaymentfromOrder { get; set; }
	}

}
