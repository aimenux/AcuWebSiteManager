using System;
using System.Web.Compilation;
using PX.Objects.AR.CCPaymentProcessing.Common;
using PX.Objects.CA;
using V1 = PX.CCProcessingBase;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using PX.Data;
using System.Linq;
using System.Reflection;
namespace PX.Objects.AR.CCPaymentProcessing.Helpers
{
	public class CCProcessingFeatureHelper
	{
		public static bool IsFeatureSupported(Type pluginType, CCProcessingFeature feature)
		{
			bool result = false;
			if (typeof(V1.ICCPaymentProcessing).IsAssignableFrom(pluginType))
			{
				switch (feature)
				{
					case CCProcessingFeature.ProfileManagement:
						result = typeof(V1.ICCTokenizedPaymentProcessing).IsAssignableFrom(pluginType);
						break;
					case CCProcessingFeature.HostedForm:
						result = typeof(V1.ICCProcessingHostedForm).IsAssignableFrom(pluginType);
						break;
					case CCProcessingFeature.ExtendedProfileManagement:
						result = false;
						break;
					case CCProcessingFeature.PaymentHostedForm:
						result = false;
						break;
					case CCProcessingFeature.WebhookManagement:
						result = false;
						break;
					case CCProcessingFeature.TransactionGetter:
						result = false;
						break;
					default:
						result = false;
						break;
				}
			}
			else if (typeof(V2.ICCProcessingPlugin).IsAssignableFrom(pluginType))
			{
				V2.ICCProcessingPlugin plugin = (V2.ICCProcessingPlugin)Activator.CreateInstance(pluginType);
				Func<object>[] checkFuncArr = null;
				switch (feature)
				{
					case CCProcessingFeature.ProfileManagement:
						checkFuncArr = new Func<object>[] { () => plugin.CreateProcessor<V2.ICCProfileProcessor>(null) };
						break;
					case CCProcessingFeature.HostedForm:
						checkFuncArr = new Func<object>[] { () => plugin.CreateProcessor<V2.ICCHostedFormProcessor>(null) };
						break;
					case CCProcessingFeature.ExtendedProfileManagement:
						checkFuncArr = new Func<object>[] { () => plugin.CreateProcessor<V2.ICCProfileProcessor>(null) };
						break;
					case CCProcessingFeature.PaymentHostedForm:
						checkFuncArr = new Func<object>[] {
							() => plugin.CreateProcessor<V2.ICCHostedPaymentFormProcessor>(null),
							() => plugin.CreateProcessor<V2.ICCTransactionGetter>(null),
							() => plugin.CreateProcessor<V2.ICCProfileCreator>(null)
						};
						break;
					case CCProcessingFeature.WebhookManagement:
						checkFuncArr = new Func<object>[] {
							() => plugin.CreateProcessor<V2.ICCWebhookProcessor>(null),
							() => plugin.CreateProcessor<V2.ICCWebhookResolver>(null)
						};
						break;
					case CCProcessingFeature.TransactionGetter:
						checkFuncArr = new Func<object>[] { () => plugin.CreateProcessor<V2.ICCTransactionGetter>(null) };
						break;
				}

				if (checkFuncArr != null)
				{
					result = checkFuncArr.All(f => CheckV2TypeWrapper(f));
				}
			}
	
			return result;
		}

		private static bool CheckV2TypeWrapper(Func<object> check)
		{
			bool ret = true;
			try
			{
				object res = check();
				ret = res != null;
			}
			catch
			{}
			return ret;
		}

		public static bool IsFeatureSupported(CCProcessingCenter ccProcessingCenter, CCProcessingFeature feature)
		{
			return ccProcessingCenter != null && !string.IsNullOrEmpty(ccProcessingCenter.ProcessingTypeName) && IsFeatureSupported(PXBuildManager.GetType(ccProcessingCenter.ProcessingTypeName, true), feature);
		}
		public static void CheckProcessing(CCProcessingCenter processingCenter, CCProcessingFeature feature, CCProcessingContext newContext)
		{
			CheckProcessingCenter(processingCenter);
			newContext.processingCenter = processingCenter;
			if (feature != CCProcessingFeature.Base && !CCProcessingFeatureHelper.IsFeatureSupported(processingCenter, feature))
			{
				throw new PXException(Messages.FeatureNotSupportedByProcessing, feature.ToString());
			}
		}

		private static void CheckProcessingCenter(CCProcessingCenter processingCenter)
		{
			if (processingCenter == null)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterNotFound);
			}
			if (processingCenter.IsActive != true)
			{
				throw new PXException(Messages.ERR_CCProcessingCenterIsNotActive);
			}
			if (string.IsNullOrEmpty(processingCenter.ProcessingTypeName))
			{
				throw new PXException(Messages.ERR_ProcessingCenterForCardNotConfigured);
			}
		}

	}
}