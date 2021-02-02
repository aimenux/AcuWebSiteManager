using PX.Data;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.AR.CCPaymentProcessing.Repositories;
using System.Collections.Generic;
using System.Web;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using System.Linq;
using PX.Objects.AR.CCPaymentProcessing.Helpers;
using System.Text.RegularExpressions;
namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public class HostedFromProcessingWrapper
	{
		private static readonly Regex checkUrl = new Regex("^(https://[\\w-.]+?)(?=\\.\\w+/)([\\w-./?%&=]+)$", RegexOptions.Compiled);

		public static IHostedFromProcessingWrapper GetHostedFormProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider)
		{
			IHostedFromProcessingWrapper wrapper = GetHostedFormProcessingWrapper(pluginObject);
			ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
			if(setProviderBehaviour == null)
			{
				throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
			}
			setProviderBehaviour.SetProvider(provider);
			return wrapper;
		}

		private static IHostedFromProcessingWrapper GetHostedFormProcessingWrapper(object pluginObject)
		{
			CCProcessingHelper.CheckHttpsConnection();
			var isV1Interface = CCProcessingHelper.IsV1ProcessingInterface(pluginObject.GetType());
			if(isV1Interface)
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{
				return new V2HostedFormProcessor(v2ProcessingInterface);
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		public static IHostedPaymentFormProcessingWrapper GetPaymentFormProcessingWrapper(object pluginObject, ICardProcessingReadersProvider provider, CCProcessingContext context)
		{
			CCProcessingHelper.CheckHttpsConnection();
			var isV1Interface = CCProcessingHelper.IsV1ProcessingInterface(pluginObject.GetType());
			if (isV1Interface)
			{
				throw new PXException(Messages.TryingToUseNotSupportedPlugin);
			}
			var v2ProcessingInterface = CCProcessingHelper.IsV2ProcessingInterface(pluginObject);
			if(v2ProcessingInterface != null)
			{ 
				V2HostedFormProcessor wrapper = new V2HostedFormProcessor(v2ProcessingInterface);
				wrapper.ProcessingCenterId = context?.processingCenter.ProcessingCenterID;
				wrapper.CompanyName = context?.callerGraph.Accessinfo.CompanyName;
				ISetCardProcessingReadersProvider setProviderBehaviour = wrapper as ISetCardProcessingReadersProvider;
				if (setProviderBehaviour == null)
				{
					throw new PXException(NotLocalizableMessages.ERR_CardProcessingReadersProviderSetting);
				}
				setProviderBehaviour.SetProvider(provider);
				return wrapper;
			}
			throw new PXException(V1.Messages.UnknownPluginType, pluginObject.GetType().Name);
		}

		private static IEnumerable<V2.CreditCardData> GetExistingProfiles(ICardProcessingReadersProvider _provider)
		{
			return _provider
				.GetCustomerCardsDataReaders()
				.Select(reader => V2ProcessingInputGenerator.GetCardData(reader))
				.Where(card => card.PaymentProfileID != null);
		}

		private class V2HostedFormProcessor : V2ProcessorBase, IHostedFromProcessingWrapper, IHostedPaymentFormProcessingWrapper
		{
			private readonly V2.ICCProcessingPlugin _plugin;

			public string ProcessingCenterId { get; set; }

			public string CompanyName { get; set; }

			public V2HostedFormProcessor(V2.ICCProcessingPlugin v2Plugin)
			{
				_plugin = v2Plugin;
			}

			private T GetProcessor<T>(CCProcessingFeature feature = CCProcessingFeature.HostedForm) where T : class
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(_provider);
				T processor = _plugin.CreateProcessor<T>(seetingsGen.GetSettings());
				if (processor == null)
				{
					string errorMessage = PXMessages.LocalizeFormatNoPrefixNLA(
						Messages.FeatureNotSupportedByProcessing,
						CCProcessingFeature.HostedForm);
					throw new PXException(errorMessage);
				}
				return processor;
			}

			public void GetCreateForm()
			{
				var processor = GetProcessor<V2.ICCHostedFormProcessor>();
				V2.CustomerData customerData = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader());
				V2.AddressData addressData = V2ProcessingInputGenerator.GetAddressData(_provider.GetCustomerDataReader());
				var result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetDataForCreateForm(customerData, addressData));
				throw new PXPaymentRedirectException(result.Caption, result.Url, result.UseGetMethod, result.Token, result.Parameters)
					{ DisableTopLevelNavigation = result.DisableTopLevelNavigation };
			}

			public void GetManageForm()
			{
				var processor = GetProcessor<V2.ICCHostedFormProcessor>();
				V2.CustomerData customerData = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader());
				V2.CreditCardData cardData = V2ProcessingInputGenerator.GetCardData(_provider.GetCardDataReader());
				var result = V2PluginErrorHandler.ExecuteAndHandleError(() => processor.GetDataForManageForm(customerData, cardData));
				throw new PXPaymentRedirectException(result.Caption, result.Url, result.UseGetMethod, result.Token, result.Parameters)
					{ DisableTopLevelNavigation = result.DisableTopLevelNavigation };
			}

			public IEnumerable<V2.CreditCardData> GetMissingPaymentProfiles()
			{
				var processor = GetProcessor<V2.ICCProfileProcessor>(CCProcessingFeature.ProfileManagement);
				string customerProfileId = V2ProcessingInputGenerator.GetCustomerData(_provider.GetCustomerDataReader()).CustomerProfileID;
				var result = V2PluginErrorHandler
								.ExecuteAndHandleError(
									() => V2.InterfaceExtensions
											.GetMissingPaymentProfiles(processor, customerProfileId, GetExistingProfiles(_provider)));
				return result;
			}

			public void GetPaymentForm(V2.ProcessingInput inputData)
			{
				var formProcessor = GetProcessor<V2.ICCHostedPaymentFormProcessor>();
				var result = V2PluginErrorHandler.ExecuteAndHandleError(() => {
					CheckWebhook();
					return formProcessor.GetDataForPaymentForm(inputData);
				});
				PXTrace.WriteInformation($"Perform PaymentRedirectException. Url: {result.Url}");
				throw new PXPaymentRedirectException(result.Caption, result.Url, result.UseGetMethod, result.Token, result.Parameters);
			}

			public HostedFormResponse ParsePaymentFormResponse(string response)
			{
				var parser = GetProcessor<V2.ICCHostedPaymentFormResponseParser>();
				var result = V2PluginErrorHandler.ExecuteAndHandleError(() => {
					return parser.Parse(response);
				});
				return V2Converter.ConvertHostedFormResponse(result);
			}

			private void CheckWebhook()
			{
				V2SettingsGenerator seetingsGen = new V2SettingsGenerator(_provider);
				if (!CCProcessingFeatureHelper.IsFeatureSupported(_plugin.GetType(), CCProcessingFeature.WebhookManagement))
				{
					PXTrace.WriteInformation("Skip check webhook. Plugin doesn't implement this feature.");
					return;
				}

				V2.ICCWebhookProcessor processor = _plugin.CreateProcessor<V2.ICCWebhookProcessor>(seetingsGen.GetSettings());
				if (!processor.WebhookEnabled)
				{
					PXTrace.WriteInformation("Skip check webhook. This feature is disabled.");
					return;
				}
				string eCompanyName = V2.CCServiceEndpointHelper.EncodeUrlSegment(CompanyName);
				string eProcCenter = V2.CCServiceEndpointHelper.EncodeUrlSegment(ProcessingCenterId);
				string url = V2.CCServiceEndpointHelper.GetEndpointUrl(eCompanyName, eProcCenter);
				if (url == null || url.Contains("localhost"))
				{
					PXTrace.WriteInformation($"Skip check webhook. Not valid Url: {url}");
					return;
				}

				bool result = checkUrl.IsMatch(url);
				if (!result)
				{
					PXTrace.WriteInformation($"Skip check webhook. Not valid Url: {url}");
					return;
				}

				IEnumerable<V2.Webhook> list = processor.GetAttachedWebhooks();
				V2.Webhook res = list.Where(i => i.Url == url).FirstOrDefault();
				if (res == null)
				{
					string name = "AcumaticaWebhook";
					PXTrace.WriteInformation($"Webhook not found. Performing add webhook with name = {name}, url = {url}");
					V2.Webhook webhook = new V2.Webhook();
					webhook.Enable = true;
					webhook.Events = new List<V2.WebhookEvent>()
					{
						V2.WebhookEvent.CreateAuthTran,
						V2.WebhookEvent.CreateAuthCaptureTran,
					};
					webhook.Name = name;
					webhook.Url = url;
					processor.AddWebhook(webhook);
				}
			}
		}
	}
}
