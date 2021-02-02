using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CA;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface ICustomerPaymentMethodDetailAdapter
	{
		CustomerPaymentMethodDetail Current { get; }
		PXCache Cache { get; }
		PXResultset<CustomerPaymentMethodDetail,PaymentMethodDetail> Select(params object[] argumetns);
	}
}
