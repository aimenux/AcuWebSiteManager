using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.Common;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public interface IHostedPaymentFormProcessingWrapper
	{
		void GetPaymentForm(V2.ProcessingInput inputData);
		HostedFormResponse ParsePaymentFormResponse(string response);
	}
}
