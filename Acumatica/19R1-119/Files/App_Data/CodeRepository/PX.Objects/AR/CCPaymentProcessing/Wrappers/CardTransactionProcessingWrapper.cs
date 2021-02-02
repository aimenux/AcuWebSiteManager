using PX.Data;
using System;
using System.Collections.Generic;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class CardTransactionProcessingWrapper
	{
		public static ICardTransactionProcessingWrapper GetTransactionProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider)
		{
			ICardTransactionProcessingWrapper wrapper = GetTransactionProcessingWrapper(pluginObject);
			ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
			if(setProviderBehaviour == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
			}
			setProviderBehaviour.SetProvider(provider);
			return wrapper;
		}

		private static ICardTransactionProcessingWrapper GetTransactionProcessingWrapper(object pluginObject)
		{
			var v1ProcessingInterFace = CCProcessingHelper.IsV1ProcessingInterface(pluginObject);
			if(v1ProcessingInterFace != null)
			{
				return new V1CardTransactionProcessor(v1ProcessingInterFace);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2CardTransactionProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		protected class V1CardTransactionProcessor : V1ProcessorBase,ICardTransactionProcessingWrapper,ISetCardProcessingReadersProvider
		{
			public V1CardTransactionProcessor(V1.ICCPaymentProcessing v1Plugin) : base(v1Plugin)
			{
				_plugin = v1Plugin;
			}

			public V1.ProcessingResult DoTransaction(V1.CCTranType aTranType, V1.ProcessingInput inputData)
			{
				ICardProcessingReadersProvider provider = GetProvider();
				_plugin.Initialize(
					provider.GetProcessingCenterSettingsStorage(),
					provider.GetCardDataReader(),
					provider.GetCustomerDataReader(),
					provider.GetDocDetailsDataReader());
				V1.ProcessingResult result = new V1.ProcessingResult();
				_plugin.DoTransaction(aTranType, inputData, result);
				return result;
			}

			public void ExportSettings(IList<V1.ISettingsDetail> aSettings)
			{
				_plugin.Initialize(
					_provider.GetProcessingCenterSettingsStorage(), null, null);
					_plugin.ExportSettings(aSettings);
			}

			public void TestCredentials(V1.APIResponse apiResponse)
			{
				_plugin.Initialize(_provider.GetProcessingCenterSettingsStorage(), null, null);
				_plugin.TestCredentials(apiResponse);
			}

			public V2.TransactionData GetTransaction(string transactionId)
			{
				throw new PXException(V1.Messages.Version1PluginNotSupported);
			}

			public IEnumerable<V2.TransactionData> GetTransactionsByCustomer(string customerProfileId, V2.TransactionSearchParams searchParams = null)
			{
				throw new PXException(V1.Messages.Version1PluginNotSupported);
			}

			public V2.TranProfile GetOrCreateCustomerProfileFromTransaction(string transactionId, string customerProfileId)
			{
				throw new PXException(V1.Messages.Version1PluginNotSupported);
			}

			public IEnumerable<V2.TransactionData> GetUnsettledTransactions(V2.TransactionSearchParams searchParams = null)
			{
				throw new PXException(V1.Messages.Version1PluginNotSupported);
			}

			public V1.CCErrors ValidateSettings(V1.ISettingsDetail setting)
			{
				_plugin.Initialize(
					_provider.GetProcessingCenterSettingsStorage(), null, null);
				return _plugin.ValidateSettings(setting);
			}

			public void SetProvider( ICardProcessingReadersProvider provider )
			{
				_provider = provider;
			}
		}

		protected class V2CardTransactionProcessor : V2ProcessorBase,ICardTransactionProcessingWrapper,ISetCardProcessingReadersProvider
		{
			private readonly V2.ICCProcessingPlugin _plugin;

			public V2CardTransactionProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			public V1.ProcessingResult DoTransaction(V1.CCTranType aTranType, V1.ProcessingInput inputData)
			{
				V2.ICCTransactionProcessor processor = GetProcessor<V2.ICCTransactionProcessor>();
				var inputGenerator = new V2ProcessingInputGenerator(_provider);
				var processingInput = inputGenerator.GetProcessingInput(aTranType, inputData);
				V2.ProcessingResult v2Result = processor.DoTransaction(processingInput);
				V1.ProcessingResult result = V1ProcessingDTOGenerator.GetProcessingResult(v2Result);
				V2.ICCTranStatusGetter tranStatusGetter = _plugin.CreateProcessor<V2.ICCTranStatusGetter>(null);
				if (tranStatusGetter != null)
				{
					V2.CCTranStatus tranStatus = tranStatusGetter.GetTranStatus(v2Result);
					result.TranStatus = V1ProcessingDTOGenerator.ToV1(tranStatus);
				}
				return result;
			}

			public V2.TransactionData GetTransaction(string transactionId)
			{
				V2.ICCTransactionGetter processor = GetProcessor<V2.ICCTransactionGetter>();
				V2.TransactionData result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetTransaction(transactionId));
				return result;
			}

			public IEnumerable<V2.TransactionData> GetTransactionsByCustomer(string customerProfileId, V2.TransactionSearchParams searchParams = null)
			{
				V2.ICCTransactionGetter processor = GetProcessor<V2.ICCTransactionGetter>();
				IEnumerable<V2.TransactionData> result = V2PluginErrorHandler.ExecuteAndHandleError(()=> processor.GetTransactionsByCustomer(customerProfileId, searchParams));
				return result;
			}

			public IEnumerable<V2.TransactionData> GetUnsettledTransactions(V2.TransactionSearchParams searchParams = null)
			{
				V2.ICCTransactionGetter processor = GetProcessor<V2.ICCTransactionGetter>();
				IEnumerable<V2.TransactionData> result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetUnsettledTransactions(searchParams));
				return result;
			}

			private T GetProcessor<T>() where T : class
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(_provider);
				T processor = _plugin.CreateProcessor<T>(seetingsGen.GetSettings());
				V1.ProcessingResult result = null;
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.Base);
					result = V1ProcessingDTOGenerator.GetProcessingResult(errorMessage);
				}
				return processor;
			}

			public void ExportSettings(IList<V1.ISettingsDetail> aSettings)
			{
				var v2Settings = _plugin.ExportSettings();
				V1ProcessingDTOGenerator.FillV1Settings(aSettings, v2Settings);
			}

			public void TestCredentials(V1.APIResponse apiResponse)
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(GetProvider());
				try
				{
					_plugin.TestCredentials(seetingsGen.GetSettings());
					V1ProcessingDTOGenerator.ApiResponseSetSuccess(apiResponse);
				}
				catch (V2.CCProcessingException e)
				{
					V1ProcessingDTOGenerator.ApiResponseSetError(apiResponse, e);
				}
			}

			public V1.CCErrors ValidateSettings(V1.ISettingsDetail setting)
			{
				string result = _plugin.ValidateSettings(V1ProcessingDTOGenerator.ToV2(setting));
				return V1ProcessingDTOGenerator.GetCCErrors(result);
			}
		}
	}
}
