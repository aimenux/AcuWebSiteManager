using System;
using PX.Data;
using System.Collections.Generic;
using System.Web;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class BaseProfileProcessingWrapper
	{

		public static IBaseProfileProcessingWrapper GetBaseProfileProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider)
		{
			IBaseProfileProcessingWrapper wrapper = GetBaseProfileProcessingWrapper(pluginObject);
			ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
			if(setProviderBehaviour == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
			}
			setProviderBehaviour.SetProvider(provider);
			return wrapper;
		}

		private static IBaseProfileProcessingWrapper GetBaseProfileProcessingWrapper(object pluginObject)
		{
			CCProcessingHelper.CheckHttpsConnection();
			var v1ProcessingInterFace = CCProcessingHelper.IsV1ProcessingInterface(pluginObject);
			if(v1ProcessingInterFace != null)
			{
				return new V1BaseProfileProcessor(v1ProcessingInterFace);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2BaseProfileProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		class V1BaseProfileProcessor : V1ProcessorBase, IBaseProfileProcessingWrapper,ISetCardProcessingReadersProvider
		{
			private V1.ICCTokenizedPaymentProcessing _processor;
			public V1BaseProfileProcessor(V1.ICCPaymentProcessing v1Plugin) : base(v1Plugin)
			{
		
			}
			
			private V1.ICCTokenizedPaymentProcessing GetProcessor()
			{
				if(_processor == null)
				{
					ICardProcessingReadersProvider provider = GetProvider();
					_plugin.Initialize(
					provider.GetProcessingCenterSettingsStorage(),
					provider.GetCardDataReader(),
					provider.GetCustomerDataReader());
					V1.ICCTokenizedPaymentProcessing profileProcessor = _plugin as V1.ICCTokenizedPaymentProcessing;
					if (profileProcessor == null)
					{
						string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
							Messages.FeatureNotSupportedByProcessing,
							CCProcessingFeature.ProfileManagement);
						throw new PXException(errorMessage);
					}
					_processor = profileProcessor;
				}
				return _processor;
			}

			public string CreateCustomerProfile()
			{
				V1.APIResponse response = new V1.APIResponse();
				string customerId;
				GetProcessor().CreateCustomer(response, out customerId);
				ProcessAPIResponse(response);
				return customerId;
			}

			public string CreatePaymentProfile()
			{ 
				V1.APIResponse response = new V1.APIResponse();
				string profileId;
				GetProcessor().CreatePMI(response, out profileId);
				ProcessAPIResponse(response);
				return profileId;
			}

			public void DeleteCustomerProfile()
			{
				V1.APIResponse response = new V1.APIResponse();
				GetProcessor().DeleteCustomer(response);
				ProcessAPIResponse(response);
			}

			public void DeletePaymentProfile()
			{
				V1.APIResponse response = new V1.APIResponse();
				GetProcessor().DeletePMI(response);
				ProcessAPIResponse(response);
			}

			public V2.CreditCardData GetPaymentProfile()
			{
				V1.APIResponse response = new V1.APIResponse();
				V1.SyncPMResponse syncResponse = new V1.SyncPMResponse();
				GetProcessor().GetPMI(response, syncResponse);
				ProcessAPIResponse(response);
				List<V2.CreditCardData> cardList = GetCardData(syncResponse);
				if (cardList.Count != 1)
				{
					throw new PXException(V1.Messages.UnexpectedResult, _plugin.GetType().Name);
				}
				return cardList[0];
			}

			public void SetProvider( ICardProcessingReadersProvider provider )
			{
				_provider = provider;
			}
		}

		class V2BaseProfileProcessor : V2ProcessorBase, IBaseProfileProcessingWrapper
		{
			private V2.ICCProcessingPlugin _plugin;

			public V2BaseProfileProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			private V2.ICCProfileProcessor GetProcessor()
			{
				V2SettingsGenerator settingsGen = new V2SettingsGenerator(GetProvider());
				V2.ICCProfileProcessor processor = _plugin.CreateProcessor<V2.ICCProfileProcessor>(settingsGen.GetSettings());
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.ProfileManagement);
					throw new PXException(errorMessage);
				}
				return processor;
			}
			
			public string CreateCustomerProfile()
			{
				V2.CustomerData customerData = V2ProcessingInputGenerator.GetCustomerData(GetProvider().GetCustomerDataReader());
				string result = V2PluginErrorHandler.ExecuteAndHandleError(() => GetProcessor().CreateCustomerProfile(customerData));
				return result;
			}

			public string CreatePaymentProfile()
			{
				ICardProcessingReadersProvider provider = GetProvider();
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(provider.GetCustomerDataReader()).CustomerProfileID;
				V2.CreditCardData cardData = V2ProcessingInputGenerator.GetCardData(provider.GetCardDataReader(), provider.GetExpirationDateConverter());
				V2.AddressData addressData = V2ProcessingInputGenerator.GetAddressData(provider.GetCustomerDataReader());
				cardData.AddressData = addressData;
				string result = V2PluginErrorHandler.ExecuteAndHandleError(() => GetProcessor().CreatePaymentProfile(customerProfileId, cardData));
				return result;
			}

			public void DeleteCustomerProfile()
			{
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(GetProvider().GetCustomerDataReader()).CustomerProfileID;
				V2PluginErrorHandler.ExecuteAndHandleError(() => GetProcessor().DeleteCustomerProfile(customerProfileId));
			}

			public void DeletePaymentProfile()
			{
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader()).CustomerProfileID;
				string paymentProfileId = V2ProcessingInputGenerator.GetCardData(_provider.GetCardDataReader()).PaymentProfileID;
				V2PluginErrorHandler.ExecuteAndHandleError(() => GetProcessor().DeletePaymentProfile(customerProfileId, paymentProfileId));
			}

			public V2.CreditCardData GetPaymentProfile()
			{
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader()).CustomerProfileID;
				string paymentProfileId = V2ProcessingInputGenerator.GetCardData(_provider.GetCardDataReader()).PaymentProfileID;
				V2.CreditCardData result = V2PluginErrorHandler.ExecuteAndHandleError(() => GetProcessor().GetPaymentProfile(customerProfileId, paymentProfileId));
				return result;
			}
		}
	}
}
