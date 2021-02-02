using PX.Data;
using System;

namespace PX.Objects.FS
{
    public static class LongOperationHelper
    {
        public static void RunLongOperationKeepingCurrentMessages(PXGraph graph, PXToggleAsyncDelegate method)
        {
            Exception longOperationException = null;

            #if (Acumatica_5_10 || Acumatica_5_20)
                object currentInfo = PXLongOperation.GetCustomInfo();
            #else
                PXProcessingInfo currentInfo = PXLongOperation.GetCustomInfo() as PXProcessingInfo;
            #endif

            try
            {
                method();
            }
            catch (Exception e)
            {
                longOperationException = e;
            }

            try
            {
                PXLongOperation.WaitCompletion(graph.UID);
            }
            catch (Exception e)
            {
                if (longOperationException == null)
                {
                    longOperationException = e;
                }
            }

            PXLongOperation.SetCustomInfo(currentInfo);

            if (longOperationException != null)
            {
                throw longOperationException;
            }
        }
    }
}
