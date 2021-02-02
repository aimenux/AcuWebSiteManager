using PX.CCProcessingBase.Interfaces.V2;
using System.Collections.Generic;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface IBaseProfileProcessingWrapper
	{
		string CreateCustomerProfile();
		void DeleteCustomerProfile();
		string CreatePaymentProfile();
		CreditCardData GetPaymentProfile();
		void DeletePaymentProfile();
	}
}
