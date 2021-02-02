using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;
using PX.CCProcessingBase;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface IHostedFromProcessingWrapper
	{
		void GetCreateForm();
		IEnumerable<CreditCardData> GetMissingPaymentProfiles();
		void GetManageForm();
	}
}
