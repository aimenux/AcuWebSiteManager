using PX.Data;
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
			bool isV1Interface = CCProcessingHelper.IsV1ProcessingInterface(pluginObject.GetType());
			if(isV1Interface)
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2BaseProfileProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
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
