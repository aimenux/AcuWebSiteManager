using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR
{
	public static class CustomerPaymentMethodExtensions
	{
		public static CustomerPaymentMethod CreateCopy(this CustomerPaymentMethod cpm, PXCache cache)
		{
			CustomerPaymentMethod ret = cache.CreateCopy(cpm) as CustomerPaymentMethod;
			return ret;
		}
	}
}
