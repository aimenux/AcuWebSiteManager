using PX.Data;
using System;
using System.Net;
using V2 = PX.CCProcessingBase.Interfaces.V2;
using V1 = PX.CCProcessingBase;

namespace PX.Objects.AR.CCPaymentProcessing.Wrappers
{
	public static class V2PluginErrorHandler
	{
		public static void ExecuteAndHandleError(Action pluginAction)
		{
			try
			{
				pluginAction();
			}
			catch (V2.CCProcessingException e)
			{
				throw new PXException(e.InnerException, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.ProcessingCenter, e.Message);
			}
			catch (WebException e)
			{
				throw new PXException(e, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.Network, e.Message);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.Internal, e.Message);
			}
		}

		public static T ExecuteAndHandleError<T>(Func<T> pluginAction)
		{
			T result = default(T);
			try
			{
				result = pluginAction();
			}
			catch (V2.CCProcessingException e)
			{
				throw new PXException(e.InnerException, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.ProcessingCenter, e.Message);
			}
			catch (WebException e)
			{
				throw new PXException(e, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.Network, e.Message);
			}
			catch (Exception e)
			{
				throw new PXException(e, Messages.CardProcessingError, V1.CCErrors.CCErrorSource.Internal, e.Message);
			}
			return result;
		}
	}
}
