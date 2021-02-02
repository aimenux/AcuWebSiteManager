using PX.Data;
using System.Linq;
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
			bool isV1Interface = CCProcessingHelper.IsV1ProcessingInterface(pluginObject.GetType());
			if(isV1Interface)
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2CardTransactionProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		protected class V2CardTransactionProcessor : V2ProcessorBase,ICardTransactionProcessingWrapper,ISetCardProcessingReadersProvider
		{
			private readonly V2.ICCProcessingPlugin _plugin;

			public V2CardTransactionProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			public TranProcessingResult DoTransaction(CCTranType aTranType, TranProcessingInput inputData)
			{
				V2.ICCTransactionProcessor processor = GetProcessor<V2.ICCTransactionProcessor>();
				var inputGenerator = new V2ProcessingInputGenerator(_provider);
				var processingInput = inputGenerator.GetProcessingInput(aTranType, inputData);
				V2.ProcessingResult v2Result = processor.DoTransaction(processingInput);
				TranProcessingResult result = V2Converter.ConvertTranProcessingResult(v2Result);
				V2.ICCTranStatusGetter tranStatusGetter = _plugin.CreateProcessor<V2.ICCTranStatusGetter>(null);
				if (tranStatusGetter != null)
				{
					V2.CCTranStatus tranStatus = tranStatusGetter.GetTranStatus(v2Result);
					result.TranStatus = V2Converter.ConvertTranStatus(tranStatus);
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
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.Base);
					throw new PXException(errorMessage);
				}
				return processor;
			}

			public void ExportSettings(IList<PluginSettingDetail> aSettings)
			{
				var v2Settings = _plugin.ExportSettings();

				foreach (var item in v2Settings)
				{
					aSettings.Add(V2Converter.ConvertSettingsDetail(item));
				}
			}

			public void TestCredentials(APIResponse apiResponse)
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(GetProvider());
				try
				{
					_plugin.TestCredentials(seetingsGen.GetSettings());
					apiResponse.isSucess = true;
					apiResponse.ErrorSource = CCError.CCErrorSource.None;
					apiResponse.Messages = null;
				}
				catch (V2.CCProcessingException e)
				{
					apiResponse.ErrorSource = CCError.CCErrorSource.ProcessingCenter;
					apiResponse.isSucess = false;
					if (apiResponse.Messages.Keys.Contains("Exception"))
					{
						apiResponse.Messages["Exception"] = e.Message;
					}
					else
					{
						apiResponse.Messages.Add("Exception", e.Message);
					}
				}
			}

			public CCError ValidateSettings(PluginSettingDetail setting)
			{
				string result = _plugin.ValidateSettings(V2Converter.ConvertSettingDetailToV2(setting));

				CCError ret = new CCError()
				{
					source = string.IsNullOrEmpty(result) ? CCError.CCErrorSource.None : CCError.CCErrorSource.Internal,
					ErrorMessage = result
				};
				return ret;
			}
		}
	}
}
