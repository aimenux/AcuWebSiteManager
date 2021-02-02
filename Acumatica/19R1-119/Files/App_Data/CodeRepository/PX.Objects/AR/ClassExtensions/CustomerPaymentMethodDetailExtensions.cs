using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR
{
	public static class CustomerPaymentMethodDetailExtensions
	{
		public static CustomerPaymentMethodDetail CreateCopy(this CustomerPaymentMethodDetail cpmd, PXCache cache)
		{
			CustomerPaymentMethodDetail ret = cache.CreateCopy(cpmd) as CustomerPaymentMethodDetail;
			var test = cache.CreateCopy(cpmd).GetType();
			return ret;
		}
	}
}
