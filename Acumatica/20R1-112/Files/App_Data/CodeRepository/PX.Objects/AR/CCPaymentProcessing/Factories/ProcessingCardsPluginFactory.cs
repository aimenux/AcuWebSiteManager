using PX.Data;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;

namespace PX.Objects.AR.CCPaymentProcessing.Factories
{
	public class ProcessingCardsPluginFactory
	{
		string processingCenterId;
		ICCPaymentProcessingRepository paymentProcessingRepository;
		public ProcessingCardsPluginFactory(string processingCenterId)
		{
			this.processingCenterId = processingCenterId;
		}

		public ICCPaymentProcessingRepository GetPaymentProcessingRepository()
		{
			if(paymentProcessingRepository == null)
			{ 
				paymentProcessingRepository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			}
			return paymentProcessingRepository;
		}

		public CCProcessingCenter GetProcessingCenter()
		{
			ICCPaymentProcessingRepository repo = GetPaymentProcessingRepository();
			CCProcessingCenter processingCenter = paymentProcessingRepository.GetCCProcessingCenter(processingCenterId);
			return processingCenter;
		}

		public object GetPlugin()
		{
			CCProcessingCenter processingCenter = GetProcessingCenter();
			object plugin = GetProcessorPlugin(processingCenter);
			return plugin;
		}

		private object GetProcessorPlugin(CCProcessingCenter processingCenter)
		{
			object plugin = CCPluginTypeHelper.CreatePluginInstance(processingCenter);
			return plugin;
		}
	}
}
