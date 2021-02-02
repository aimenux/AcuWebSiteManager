using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class CustomerCreditCard
	{
		public string CustomerProfileId { get; set; }
		public List<CreditCardData> CreditCards { get;set; }
		public CustomerCreditCard()
		{

		}
	}
}
