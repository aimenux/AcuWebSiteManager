using System;
using System.Collections.Generic;
using PX.CCProcessingBase.Interfaces.V2;
using PX.Objects.CA;
using PX.Objects.AR.CCPaymentProcessing.Wrappers;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Web.Compilation;
using System.Threading.Tasks;
using System.Linq;
using PX.Data;

namespace PX.Objects.AR.CCPaymentProcessing.CardsSynchronization
{
	public class CCSynchronizeCardManager
	{
		private readonly string processingCenterId;
		private readonly List<string> customerProfileIds;
		private readonly PXGraph graph;
		private readonly CreditCardReceiverFactory receiverFactory;
		private ICCProcessingPlugin processingPlugin;
		private V2SettingsGenerator settingsGenerator;
		private CCProcessingContext context;

		public CCSynchronizeCardManager(PXGraph graph, string processingCenterId, CreditCardReceiverFactory receiverFactory)
		{
			this.graph = graph;
			this.processingCenterId = processingCenterId;
			this.receiverFactory = receiverFactory;
			InitializeContext();
			InitializeProcessingPlugin();
			InitializeSettingsGenerator();
			customerProfileIds = new List<string>();
		}

		private void InitializeContext()
		{
			context = new CCProcessingContext();
			ICCPaymentProcessingRepository repository = CCPaymentProcessingRepository.GetCCPaymentProcessingRepository();
			CCProcessingCenter processingCenter = repository.GetCCProcessingCenter(processingCenterId);
			context.processingCenter = processingCenter;
			context.callerGraph = graph;
		}

		private void InitializeSettingsGenerator()
		{
			ICardProcessingReadersProvider provider = new CardProcessingReadersProvider(context);
			settingsGenerator = new V2SettingsGenerator(provider);
		}

		private void InitializeProcessingPlugin()
		{
			processingPlugin = GetProcessorPlugin(context.processingCenter) as ICCProcessingPlugin;
			if(processingPlugin == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterPluginNotFound,processingCenterId);
			}
		}

		private object GetProcessorPlugin(CCProcessingCenter processingCenter)
		{
			object plugin = CCPluginTypeHelper.CreatePluginInstance(processingCenter);
			return plugin;
		}

		public void SetCustomerProfileIds(IEnumerable<string> customerProfileIds)
		{
			this.customerProfileIds.Clear();
			this.customerProfileIds.AddRange(customerProfileIds);
		}

		public Dictionary<string,CustomerCreditCard> GetPaymentProfilesFromService()
		{
			IEnumerable<SettingsValue> settings = settingsGenerator.GetSettings();
			ICCProfileProcessor processor = processingPlugin.CreateProcessor<ICCProfileProcessor>(settings);
			List<CreditCardReceiver> syncCards = customerProfileIds.Select(i =>
				receiverFactory.GetCreditCardReceiver(processingPlugin.CreateProcessor<ICCProfileProcessor>(settings), i)).ToList();
			try
			{
				Parallel.ForEach(syncCards, (CreditCardReceiver task) => {
					task.DoAction();
				});
			}
			catch(AggregateException ex)
			{
				Exception innerEx = ex.InnerExceptions[0];
				throw new PXException(innerEx.Message);
			}
			Dictionary<string,CustomerCreditCard> ret = syncCards.Select(i=> new CustomerCreditCard() {CreditCards = i.Result, CustomerProfileId = i.CustomerProfileId } )
				.ToDictionary(i=>i.CustomerProfileId);
			return ret;
		}

		public Dictionary<string,CustomerData> GetCustomerProfilesFromService() 
		{
			IEnumerable<SettingsValue> settings = settingsGenerator.GetSettings();
			ICCProfileProcessor profileProcessor = processingPlugin.CreateProcessor<ICCProfileProcessor>(settings);
			Dictionary<string,CustomerData> ret = profileProcessor.GetAllCustomerProfiles().ToDictionary(i=>i.CustomerProfileID);
			return ret;
		}

		public Dictionary<string,CustomerData> GetUnsynchronizedCustomerProfiles()
		{
			Dictionary<string,CustomerData> customerProfiles = GetCustomerProfilesFromService();
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectJoinGroupBy<CustomerPaymentMethod,
				InnerJoin<CCProcessingCenterPmntMethod,On<CustomerPaymentMethod.paymentMethodID,Equal<CCProcessingCenterPmntMethod.paymentMethodID>>>,
				Where<CCProcessingCenterPmntMethod.processingCenterID,Equal<Required<CCProcessingCenterPmntMethod.processingCenterID>>,
					And<CustomerPaymentMethod.customerCCPID, IsNotNull>>,
				Aggregate<
					GroupBy<CustomerPaymentMethod.customerCCPID>>>(graph);
			PXResultset<CustomerPaymentMethod> lines = query.Select(processingCenterId);

			foreach (CustomerPaymentMethod line in lines)
			{
				string checkCustomerProfile = line.CustomerCCPID; 
				if(customerProfiles.ContainsKey(checkCustomerProfile))
				{
					customerProfiles.Remove(checkCustomerProfile);
				}
			}
			return customerProfiles;
		}

		public Dictionary<string,CustomerCreditCard> GetUnsynchronizedPaymentProfiles()
		{
			Dictionary<string,CustomerCreditCard> paymentProfiles = GetPaymentProfilesFromService();
			PXSelectBase<CustomerPaymentMethod> query = new PXSelectReadonly2<CustomerPaymentMethod,
				InnerJoin<CustomerPaymentMethodDetail, On<CustomerPaymentMethod.pMInstanceID,Equal<CustomerPaymentMethodDetail.pMInstanceID>>,
				InnerJoin<PaymentMethodDetail,On<CustomerPaymentMethodDetail.detailID,Equal<PaymentMethodDetail.detailID>,
					And<CustomerPaymentMethodDetail.paymentMethodID,Equal<PaymentMethodDetail.paymentMethodID>>>>>,
				Where<CustomerPaymentMethod.cCProcessingCenterID,Equal<Required<CustomerPaymentMethod.cCProcessingCenterID>>,
					And<PaymentMethodDetail.isCCProcessingID,Equal<True>,
					And<PaymentMethodDetail.useFor,Equal<PaymentMethodDetailUsage.useForARCards>>>>>(graph);
			PXResultset<CustomerPaymentMethod> lines = query.Select(processingCenterId);

			foreach (PXResult<CustomerPaymentMethod,CustomerPaymentMethodDetail> line in lines)
			{
				FilterPaymentsProfilesFromService(paymentProfiles, line);
			}
			return paymentProfiles;
		}

		private void FilterPaymentsProfilesFromService(Dictionary<string,CustomerCreditCard> paymentProfiles, PXResult<CustomerPaymentMethod,CustomerPaymentMethodDetail> line)
		{
			CustomerPaymentMethod paymentMethod = line;
			CustomerPaymentMethodDetail paymentMethodDetail = line;
			string checkCustomerProfileId = paymentMethod.CustomerCCPID;
			string checkPaymentProfileId = paymentMethodDetail.Value;

			if (checkCustomerProfileId != null && paymentProfiles.ContainsKey(checkCustomerProfileId))
			{
				CustomerCreditCard customerCreditCard = paymentProfiles[checkCustomerProfileId];
				int index = customerCreditCard.CreditCards.FindIndex(i=>i.PaymentProfileID == checkPaymentProfileId);

				if (index != -1)
				{
					customerCreditCard.CreditCards.RemoveAt(index);
				}
			}
		}
	}
}
