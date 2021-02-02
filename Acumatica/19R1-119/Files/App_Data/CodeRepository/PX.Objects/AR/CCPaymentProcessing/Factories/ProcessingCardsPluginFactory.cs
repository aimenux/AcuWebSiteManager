using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using PX.Data;
using PX.Objects.CA;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
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
			Type processorType = PXBuildManager.GetType(processingCenter.ProcessingTypeName, true);
			object plugin = Activator.CreateInstance(processorType);
			return plugin;
		}
	}
}
