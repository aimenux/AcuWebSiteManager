using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Objects.Extensions.PaymentProfile;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing
{
	public interface ICustomerPaymentMethodAdapter
	{
		CustomerPaymentMethod Current { get; }
		PXCache Cache { get; }
	}
}
