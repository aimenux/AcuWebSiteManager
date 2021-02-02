using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCPaymentProfile
	{
		int? BAccountID { get; set; }
		int? PMInstanceID { get; set; }
		string CCProcessingCenterID { get; set; }
		string CustomerCCPID { get; set; }
		string PaymentMethodID { get; set; }
		int? CashAccountID { get; set; }
		string Descr { get; set; }
		DateTime? ExpirationDate { get; set; }
	}
}
