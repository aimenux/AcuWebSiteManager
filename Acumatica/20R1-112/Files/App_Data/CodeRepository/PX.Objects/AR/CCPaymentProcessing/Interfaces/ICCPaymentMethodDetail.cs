using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
namespace PX.Objects.AR.CCPaymentProcessing.Interfaces
{
	public interface ICCPaymentMethodDetail
	{
		string PaymentMethodID { get; set; }
		string DetailID { get; set; }
		bool? IsCCProcessingID{ get; set; }
		bool? IsIdentifier{ get; set; }
		bool? IsRequired { get; set; }
		bool? IsExpirationDate { get; set; }
		bool? IsCVV { get; set; }
		bool? IsOwnerName { get; set; }
	}
}
