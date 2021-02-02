using System;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;

namespace PX.Objects.AM
{
    public sealed class PXTraceHelper
    {
        public enum ErrorLevel
        {
            Information,
            Warning,
            Error
        }

        public static void WriteInformation(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }

            PXTrace.WriteInformation(msg);
            AMDebug.TraceWriteLine(msg);
        }

        public static void WriteInformation(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return;
            }

            WriteInformation(string.Format(format, args));
        }

        /// <summary>
        /// Used to display execution times for certain processes
        /// </summary>
        /// <param name="timeSpan">Timespan object containing execution time</param>
        /// <param name="description">Description of run time</param>
        public static void WriteTimespan(TimeSpan timeSpan, string description = "")
        {
            var msg = CreateTimespanMessage(timeSpan, description);
            WriteInformation(msg);
        }

        /// <summary>
        /// Create a string message displaying the timespan time
        /// </summary>
        /// <param name="timeSpan">TimeSpan containing the runtime</param>
        /// <param name="description">Additional optional description to add to the time</param>
        /// <returns>message of use display of timespan timeframe and description if used</returns>
        public static string CreateTimespanMessage(TimeSpan timeSpan, string description = "")
        {
            var msg = RuntimeMessage(timeSpan);

            if (!string.IsNullOrWhiteSpace(description))
            {
                msg = $"{msg} [{description}]";
            }

            return msg;
        }

        public static string RuntimeMessage(TimeSpan timeSpan)
        {
            return AM.Messages.GetLocal(AM.Messages.Runtime, Common.Strings.TimespanMessage(timeSpan));
        }

        /// <summary>
        /// Only traces an OuterException details if the exception is an OuterException
        /// </summary>
        public static void TraceOuterExceptionOnly(Exception e, ErrorLevel errorLevel = ErrorLevel.Error)
        {
            var oe = e as PXOuterException;
            if (oe != null)
            {
                PxTraceOuterException(oe, errorLevel);
            }
        }

        public static void PxTraceException(Exception e)
        {
            PxTraceException(e, ErrorLevel.Error);
        }

        public static void PxTraceException(Exception e, ErrorLevel errorLevel)
        {
            PxTraceException(e, errorLevel, false);
        }

        public static void PxTraceException(Exception e, ErrorLevel errorLevel, bool traceInnerException)
        {
            if (e == null)
            {
                return;
            }

            if (traceInnerException)
            {
                PxTraceException(e.InnerException, errorLevel, false);
            }

            var oe = e as PXOuterException;
            if (oe != null)
            {
                PxTraceOuterException(oe, errorLevel);
                return;
            }

            TraceWrite(e, errorLevel);
        }

        /// <summary>
        /// Record all inner/field messages to the Acumatica trace window in a single trace if the exception is of type PXOuterException
        /// </summary>
        public static void PxTraceOuterException(Exception exception, ErrorLevel errorLevel)
        {
            if (exception != null
                && exception is PXOuterException)
            {
                PxTraceOuterException((PXOuterException)exception, errorLevel);
            }
        }

        /// <summary>
        /// Record all inner/field messages to the Acumatica trace window in a single trace. 
        /// Typical outer exceptions from Acumatica tell the user to review the details without providing the details.
        /// This call will trace print the details for the user.
        /// </summary>
        public static void PxTraceOuterException(PXOuterException pxOuterException, ErrorLevel errorLevel)
        {
            if (pxOuterException == null)
            {
                return;
            }

            if (pxOuterException.InnerMessages == null)
            {
                return;
            }

            try
            {
                string field = string.Empty;
                var sb = new StringBuilder();

                var cntr = 0;
                foreach (string innerMessage in pxOuterException.InnerMessages)
                {
                    if (pxOuterException.InnerFields != null)
                    {
                        if (pxOuterException.InnerFields.Count() == pxOuterException.InnerMessages.Count())
                        {
                            field = $"{pxOuterException.InnerFields[cntr]}: ";
                        }
                    }

                    sb.AppendLine($"{field}{innerMessage}");

                    cntr++;
                }

                if (cntr > 0)
                {
                    TraceWrite(sb.ToString(), errorLevel);
#if DEBUG
                    AMDebug.TraceWriteLine(sb.ToString());
#endif
                }
            }
            catch (Exception e)
            {
                AMDebug.TraceWriteMethodName(e.Message);
                PXTrace.WriteInformation($"Unable to print PXOuterException details: {e.Message}");
            }
        }

        public static void TraceWrite(string message)
        {
            TraceWrite(message, ErrorLevel.Information);
        }

        public static void TraceWrite(string message, ErrorLevel errorLevel)
        {
#if DEBUG
            AMDebug.TraceWriteLine(message);
#endif
            switch (errorLevel)
            {
                case ErrorLevel.Warning:
                    PXTrace.WriteWarning(message);
                    break;
                case ErrorLevel.Error:
                    PXTrace.WriteError(message);
                    break;
            }
            PXTrace.WriteInformation(message);
        }

        public static void TraceWrite(Exception e, ErrorLevel errorLevel)
        {
#if DEBUG
            AMDebug.TraceException(e);
#endif
            switch (errorLevel)
            {
                case ErrorLevel.Warning:
                    PXTrace.WriteWarning(e);
                    break;
                case ErrorLevel.Error:
                    PXTrace.WriteError(e);
                    break;
            }
            PXTrace.WriteInformation(e);
        }

        public static string GetExceptionMessage(Exception e)
        {
            return _GetExceptionMessage(e).ToString();
        }

        private static StringBuilder _GetExceptionMessage(Exception exception)
        {
            var sb = new StringBuilder();
            if (exception == null)
            {
                return sb;
            }

            if (exception is PXOuterException)
            {
                return _GetPXOuterExceptionMessage((PXOuterException)exception);
            }

            sb.AppendLine(exception.Message);
            var ie1 = exception.InnerException;
            if (ie1 != null)
            {
                sb.AppendLine(ie1.Message);
                var ie2 = ie1.InnerException;
                if (ie2 != null)
                {
                    sb.AppendLine(ie2.Message);
                }
            }
            return sb;
        }

        private static StringBuilder _GetPXOuterExceptionMessage(PXOuterException pxOuterException)
        {
            var sb = new StringBuilder();
            if (pxOuterException == null)
            {
                return sb;
            }

            var cntr = 0;
            if (!string.IsNullOrEmpty(pxOuterException.Message))
            {
                sb.AppendLine(pxOuterException.Message);
            }
            foreach (string innerMessage in pxOuterException.InnerMessages)
            {
                var field = string.Empty;
                if (pxOuterException.InnerFields != null)
                {
                    if (pxOuterException.InnerFields.Length == pxOuterException.InnerMessages.Length)
                    {
                        field = $"{pxOuterException.InnerFields[cntr]}: ";
                    }
                }

                sb.AppendLine($"{field}{innerMessage}");
                cntr++;
            }

            return sb;
        }

        /// <summary>
        /// Report the cached record changes
        /// </summary>
        /// <returns></returns>
        public static StringBuilder DirtyCacheRecordCounts(PXGraph graph)
        {
            return DirtyCacheRecordCounts(graph, new StringBuilder());
        }

        /// <summary>
        /// Report the cached record changes
        /// </summary>
        /// <returns></returns>
        public static StringBuilder DirtyCacheRecordCounts(PXGraph graph, StringBuilder sb)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }

            foreach (var cache in graph.Caches)
            {
                if (graph.Views.Caches.Contains(cache.Key) && cache.Value.IsDirty)
                {
                    sb.Append($"{Common.Cache.GetCacheName(cache.Value.GetItemType())}");

                    var iCntr = cache.Value?.Inserted?.Count();
                    if (iCntr.GetValueOrDefault() > 0)
                    {
                        sb.Append($"; Inserted: {iCntr}");
                    }

                    var uCntr = cache.Value?.Updated?.Count();
                    if (uCntr.GetValueOrDefault() > 0)
                    {
                        sb.Append($"; Updated: {uCntr}");
                    }

                    var dCntr = cache.Value?.Deleted?.Count();
                    if (dCntr.GetValueOrDefault() > 0)
                    {
                        sb.Append($"; Deleted: {dCntr}");
                    }

                    sb.AppendLine(".");
                }
            }

            return sb;
        }
    }
}
