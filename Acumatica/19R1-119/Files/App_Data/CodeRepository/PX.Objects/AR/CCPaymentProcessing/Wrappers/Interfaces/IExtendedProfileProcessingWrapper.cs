using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface IExtendedProfileProcessingWrapper
	{
		CustomerData GetCustomerProfile();
		void UpdateCustomerProfile();
		IEnumerable<CustomerData> GetAllCustomerProfiles();
		void UpdatePaymentProfile();
		IEnumerable<CreditCardData> GetAllPaymentProfiles();
		TranProfile GetOrCreatePaymentProfileFromTransaction(string transactionId, CreateTranPaymentProfileParams cParams);
	}
}
