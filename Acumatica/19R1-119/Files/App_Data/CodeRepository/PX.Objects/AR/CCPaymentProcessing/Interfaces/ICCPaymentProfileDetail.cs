using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCPaymentProfileDetail
	{
		int? PMInstanceID { get; set; }
		string PaymentMethodID { get; set; }
		string DetailID { get; set; }
		string Value { get; set; }
	}
}
