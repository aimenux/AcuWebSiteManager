using System;
using PX.Objects.CA;
using PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class CreditCardReceiverFactory
	{
		CCSynchronizeCards.CreditCardsFilter filter;
		public CreditCardReceiverFactory(CCSynchronizeCards.CreditCardsFilter filter)
		{
			this.filter = filter;
		}

		public CreditCardReceiver GetCreditCardReceiver(ICCProfileProcessor profileProcessor, string customerProfileId)
		{
			CreditCardReceiver receiver = new CreditCardReceiver(profileProcessor, customerProfileId) { AttempsCnt = 3 };

			if (filter.LoadExpiredCards.GetValueOrDefault() != true)
			{
				receiver.ProcessFilter = (CreditCardData item) => {
					bool ret = true;

					if (item.CardExpirationDate.HasValue)
					{
						DateTime dt = item.CardExpirationDate.Value.Date.AddMonths(1);
						ret = DateTime.Now.Date < dt ? true : false;
					}
					return ret;
				};
			}
			return receiver;
		}
	}
}
