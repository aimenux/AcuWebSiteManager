using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PX.CCProcessingBase.Interfaces.V2;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class CreditCardReceiver
	{ 
		public string CustomerProfileId { get; private set; }

		public List<CreditCardData> Result { get; private set; }

		public int AttempsCnt { get;set; } = 1;

		ICCProfileProcessor profileProcessor;

		public Func<CreditCardData, bool> ProcessFilter { get; set; }

		public CreditCardReceiver(ICCProfileProcessor profileProcessor,string customerProfileId)
		{
			this.profileProcessor = profileProcessor;
			this.CustomerProfileId = customerProfileId;
		}

		public void DoAction()
		{
			try
			{  
				AttempsCnt --;
				IEnumerable<CreditCardData> paymentsProfiles = profileProcessor.GetAllPaymentProfiles(CustomerProfileId);
				Result = new List<CreditCardData>();

				foreach (CreditCardData item in paymentsProfiles)
				{
					if (ProcessFilter != null)
					{
						if (ProcessFilter(item))
						{
							Result.Add(item);
						}
					}
					else
					{
						Result.Add(item);
					}
				}
			}
			catch(CCProcessingException)
			{

				if (AttempsCnt > 0)
				{
					Thread.Sleep(100);
					DoAction();
				}
				else
				{
					throw;
				}
			}
		}
	}
}
