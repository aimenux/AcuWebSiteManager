using PX.Data;
using System;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using System.Web;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class ExtendedProfileProcessingWrapper
	{
		public static IExtendedProfileProcessingWrapper GetExtendedProfileProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider)
		{
			IExtendedProfileProcessingWrapper wrapper = GetExtendedProfileProcessingWrapper(pluginObject);
			ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
			if(setProviderBehaviour == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
			}
			setProviderBehaviour.SetProvider(provider);
			return wrapper;
		}

		private static IExtendedProfileProcessingWrapper GetExtendedProfileProcessingWrapper(object pluginObject)
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
				return new V2ExtendedProfileProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		class V2ExtendedProfileProcessor : ISetCardProcessingReadersProvider, IExtendedProfileProcessingWrapper
		{
			private V2.ICCProcessingPlugin _plugin;
			private ICardProcessingReadersProvider _provider;

			public V2ExtendedProfileProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			private T GetProcessor<T>() where T : class
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(_provider);
				T processor = _plugin.CreateProcessor<T>(seetingsGen.GetSettings());
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.ExtendedProfileManagement);
					throw new PXException(errorMessage);
				}
				return processor;
			}
			public IEnumerable<V2.CustomerData> GetAllCustomerProfiles()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<V2.CreditCardData> GetAllPaymentProfiles()
			{
				V2.ICCProfileProcessor processor = GetProcessor<V2.ICCProfileProcessor>();
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader()).CustomerProfileID;
				IEnumerable<V2.CreditCardData> result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetAllPaymentProfiles(customerProfileId));
				return result;
			}

			public V2.TranProfile GetOrCreatePaymentProfileFromTransaction(string transactionId, V2.CreateTranPaymentProfileParams cParams)
			{
				V2.ICCProfileCreator processor = GetProcessor<V2.ICCProfileCreator>();
				V2.TranProfile result = V2PluginErrorHandler.ExecuteAndHandleError(()=>processor.GetOrCreatePaymentProfileFromTransaction(transactionId, cParams));
				return result;
			}

			public V2.CustomerData GetCustomerProfile()
			{
				throw new NotImplementedException();
			}

			public void UpdateCustomerProfile()
			{
				throw new NotImplementedException();
			}

			public void UpdatePaymentProfile()
			{
				throw new NotImplementedException();
			}

			public void SetProvider( ICardProcessingReadersProvider provider )
			{
				_provider = provider;
			}

			protected ICardProcessingReadersProvider GetProvider()
			{
				if(_provider == null)
				{
					throw new PXInvalidOperationException("Could not set CardProcessingReaderProvider");
				}
				return _provider;
			}
		}
	}
}
