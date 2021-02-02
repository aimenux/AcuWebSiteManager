using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using PX.Common;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Used for Manufacturing Development
    /// </summary>
    public static class AMDebug
    {
        public static void TraceWriteLine(string message)
        {
#if DEBUG
            Trace.WriteLine(message);
#endif
        }

        public static void TraceWriteLine(string message, params object[] args)
        {
            TraceWriteLine(string.Format(message, args));
        }

        public static string GetExceptionMessageForTrace(Exception exception, string additionalMessage = "")
        {
#if DEBUG
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("!!!---------------------------------------------!!!");
            sb.AppendLine(exception.GetType().Name);
            sb.AppendLine(exception.Source);
            sb.AppendLine(exception.Message);

            if (exception.InnerException != null)
            {
                sb.AppendLine(string.Format("Inner Exception: {0}", exception.InnerException.GetType().Name));
                sb.AppendLine(exception.InnerException.Source);
                sb.AppendLine(exception.InnerException.Message);
            }

            if (!string.IsNullOrWhiteSpace(additionalMessage))
            {
                sb.AppendLine("---------------------------------------------------");
                sb.AppendLine(additionalMessage);
            }

            sb.AppendLine("!!!---------------------------------------------!!!");

            return sb.ToString();
#else
            return string.Empty;
#endif
        }

        public static void TraceException(Exception exception)
        {
#if DEBUG
            TraceWriteLine(GetExceptionMessageForTrace(exception));
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TraceExceptionMethodName(Exception exception, string additionalMessage = "")
        {
#if DEBUG
            var sb = new StringBuilder("Exception:");

            try
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                if (sf != null)
                {
                    sb.AppendLine($"[{sf.GetMethod().DeclaringType}.{sf.GetMethod().Name}]");

                    if (!string.IsNullOrWhiteSpace(additionalMessage))
                    {
                        sb.Append(additionalMessage);
                    }
                }
                TraceWriteLine(GetExceptionMessageForTrace(exception, sb.ToString()));
            }
            catch (Exception e)
            {
                TraceWriteLine($"[Exception in AMDebug.TraceWriteMethodName] {e.Message}");
            }
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TraceWriteMethodName(string format, params object[] args)
        {
#if DEBUG
            TraceWithMethodName(string.Format(format, args));
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TraceWriteMethodName(string additionalMessage = "")
        {
#if DEBUG
            TraceWithMethodName(additionalMessage);
#endif
        }

        private static void TraceWithMethodName(string additionalMessage = "")
        {
#if DEBUG
            string traceMessage = string.Empty;

            try
            {
                StackTrace st = new StackTrace();
                StackFrame sf = st.GetFrame(2);

                if (sf != null)
                {
                    traceMessage = string.Format("[{0}.{1}]", sf.GetMethod().DeclaringType, sf.GetMethod().Name);

                    if (!string.IsNullOrWhiteSpace(additionalMessage))
                    {
                        traceMessage = string.Format("{0}  {1}", traceMessage, additionalMessage);
                    }
                }

            }
            catch (Exception e)
            {
                traceMessage = string.Format("[Exception in AMDebug.TraceWriteMethodName] {0}", e.Message);
            }

            TraceWriteLine(traceMessage);
#endif
        }

        public static void PrintChangedValuesNoExcludes<T>(T objA, T objB) where T : class
        {
            PrintChangedValues<T>(objA, objB, new List<string>(), false);
        }

        public static void PrintChangedValues<T>(T objA, T objB) where T : class
        {
            PrintChangedValues<T>(objA, objB,
                new List<string>
                {
                    "CreatedDateTime",
                    "CreatedByScreenID",
                    "CreatedByID",
                    "LastModifiedDateTime",
                    "LastModifiedByScreenID",
                    "LastModifiedByID",
                    "NoteID",
                    "tstamp"
                },
                false);
        }

        public static void PrintAllValues<T>(T objA, T objB) where T : class
        {
            PrintChangedValues<T>(objA, objB, new List<string>(), true);
        }

        private static void PrintChangedValues<T>(T fromObject, T toObject, List<string> fixedExcludedList, bool printAllValues) where T : class
        {
#if DEBUG
            var sw = Stopwatch.StartNew();
            if (fromObject != null && toObject != null)
            {
                Type type = typeof(T);
                foreach (PropertyInfo pi in type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!printAllValues && fixedExcludedList.Contains(pi.Name))
                    {
                        continue;
                    }

                    object fromValue = type.GetProperty(pi.Name).GetValue(fromObject, null);
                    object toValue = type.GetProperty(pi.Name).GetValue(toObject, null);

                    string debugMsg = null;
                    if (fromValue != toValue && (fromValue == null || !fromValue.Equals(toValue)))
                    {
                        debugMsg = $"{fromObject.GetType()}.{pi.Name} changed from {fromValue} to {toValue}";
                    }
                    else if(printAllValues)
                    {
                        debugMsg = string.Format("{2}.{0} unchanged = {1}", pi.Name,  fromValue, fromObject.GetType());
                    }

                    if (!string.IsNullOrWhiteSpace(debugMsg))
                    {
                        TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, debugMsg));
                    }
                }

                sw.Stop();
            }
#endif
        }

        public static void PrintIfUpdatesFound<T>(PXGraph graph, string msg = "") where T : class, IBqlTable, new()
        {
            if (graph.ContainsUpdatedCache<T>())
            {
                TraceWriteLine($"Graph {graph.GetType().Name} contains updates to {typeof(T).Name}. {msg}");
            }
        }

        public static void MeasureElapsed(Action func, string msg = "")
        {
            var sw = new Stopwatch();
            sw.Start();

            func?.Invoke();

            sw.Stop();

            TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, msg));
        }

        /// <summary>
        /// Run a process and measure its processing time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="msg">trace message</param>
        /// <example>
        /// var x = AMDebug.MeasureElapsed(() => GraphUIDEquals(taskInfo.NativeKey, UID), "GraphUIDEquals");
        /// var y = AMDebug.MeasureElapsed(() => taskInfo.NativeKey == UID);
        /// </example>
        /// <returns></returns>
        public static T MeasureElapsed<T>(Func<T> func, string msg = "")
        {
            var sw = new Stopwatch();
            sw.Start();
            //for (int i = 1; i < 1000000; i++)
            //{
            //    func();
            //}
            var result = func();

            sw.Stop();

            TraceWriteMethodName(PXTraceHelper.CreateTimespanMessage(sw.Elapsed, msg));

            return result;
        }

        /// <summary>
        /// Run a process and measure its processing ticks
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="msg">trace message</param>
        /// <example>
        /// var x = AMDebug.MeasureElapsedTicks(() => GraphUIDEquals(taskInfo.NativeKey, UID), "GraphUIDEquals");
        /// var y = AMDebug.MeasureElapsedTicks(() => taskInfo.NativeKey == UID);
        /// </example>
        /// <returns></returns>
        public static T MeasureElapsedTicks<T>(Func<T> func, string msg = "")
        {
            var sw = new Stopwatch();
            sw.Start();
            //for (int i = 1; i < 1000000; i++)
            //{
            //    func();
            //}
            var result = func();

            sw.Stop();

            if (!string.IsNullOrWhiteSpace(msg))
            {
                msg = $"[{msg}]";
            }

            TraceWriteMethodName($"ElapsedTicks = {sw.ElapsedTicks}  {msg}");

            return result;
        }

        internal static void TraceModifiedCaches(PXGraph graph)
        {
            TraceDirtyCaches(graph, false, true);
        }

        internal static void TraceDirtyCaches(PXGraph graph)
        {
            TraceDirtyCaches(graph, true, false);
        }

        internal static void TraceDirtyCaches(PXGraph graph, bool hasViewOnly, bool includeInsertUpdateDeleted)
        {
            if (graph == null || !graph.IsDirty)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Dirty Caches...");
            foreach (var cache in graph.Caches)
            {
                var inserted = cache.Value?.Inserted?.Count() ?? 0;
                var updated = cache.Value?.Updated?.Count() ?? 0;
                var deleted = cache.Value?.Deleted?.Count() ?? 0;
                var hasInsertUpdateDeleted = inserted != 0 || updated != 0 || deleted != 0;
                var isDirty = cache.Value?.IsDirty == true;
                if (!isDirty && !(hasInsertUpdateDeleted && includeInsertUpdateDeleted))
                {
                    continue;
                }

                var hasView = graph.Views.Caches.Contains(cache.Key);

                if ((hasViewOnly && hasView) || !hasViewOnly)
                {
                    sb.AppendLine($"Cache: {cache.Key.Name}; IsDirty = {isDirty}; Inserted: {inserted}; Updated: {updated}; Deleted: {deleted}; Has View: {hasView}".Indent(1));
                }
            }

            if (sb.Length > 0)
            {
                TraceWithMethodName(sb.ToString());
            }
        }

        internal static void TraceDirtyCacheValues(PXGraph graph, params string[] fieldNames)
        {
            if (graph == null || !graph.IsDirty)
            {
                return;
            }

            var entityHelper = new EntityHelper(graph);
            var sb = new StringBuilder();
            sb.AppendLine(fieldNames.Length > 0 ? $"Dirty Caches with field name values of {string.Join(",", fieldNames)}" : "Dirty Caches");
            var initLength = sb.Length;

            foreach (var cache in graph.Caches)
            {
                if (graph.Views.Caches.Contains(cache.Key) && cache.Value.IsDirty)
                {
                    TraceDirtyCacheValues(entityHelper, cache.Value, ref sb, fieldNames);
                }
            }

            if (sb.Length > initLength)
            {
                TraceWithMethodName(sb.ToString());
            }
        }

        internal static void TraceDirtyCacheValues(PXCache cache, params string[] fieldNames)
        {
            var sb = new StringBuilder();
            TraceDirtyCacheValues(new EntityHelper(cache?.Graph), cache, ref sb, fieldNames);
            if (sb.Length > 0)
            {
                TraceWithMethodName(sb.ToString());
            }
        }

        internal static void TraceDirtyCacheValues(EntityHelper entityHelper, PXCache cache, ref StringBuilder sb, params string[] fieldNames)
        {
            if (cache == null || !cache.IsDirty || entityHelper == null)
            {
                return;
            }

            var fieldNamesProvided = fieldNames != null && fieldNames.Length > 0;
            if (fieldNamesProvided)
            {
                var containsField = fieldNames.Aggregate(false, (current, fieldName) => current | cache.Fields.Contains(fieldName));
                if (!containsField)
                {
                    return;
                }
            }

            foreach (var row in cache.Cached)
            {
                sb.Append($"Cache {cache.GetItemType().Name} status {Enum.GetName(typeof(PXEntryStatus), cache.GetStatus(row))} keys {string.Join(":", entityHelper.GetEntityRowKeys(cache.GetItemType(), row))};");
                if (fieldNamesProvided)
                {
                    foreach (var fieldName in fieldNames)
                    {
                        sb.Append($" {fieldName}='{cache.GetValue(row, fieldName)}';");
                    }
                }
                sb.AppendLine();
            }
        }

        public static void TraceFieldNames(PXCache cache)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Fields of cache: {cache.GetItemType().FullName}");
            var fieldList = new List<string>();
            var fields = cache.Fields;
            if (fields == null || fields.Count == 0)
            {
                return;
            }

            foreach (var cacheField in fields.OrderBy(x => x))
            {
                if (cacheField.Contains("_"))
                {
                    continue;
                }

                fieldList.Add(cacheField);

                if (fieldList.Count >= 10)
                {
                    sb.Append("".Indent(1));
                    sb.AppendLine(string.Join(",", fieldList.ToArray()));
                    fieldList.Clear(); 
                }
            }

            TraceWriteLine(sb.ToString());
        }
    }
}
