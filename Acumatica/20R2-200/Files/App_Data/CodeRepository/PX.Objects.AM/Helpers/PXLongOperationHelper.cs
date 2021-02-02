using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.SM;

namespace PX.Objects.AM
{
    public static class PXLongOperationHelper
    {
        public const string CompanyIDKey = "CompanyID";
        public const string UserKey = "User";
        public const string WorkTimeKey = "WorkTime";

        public static int CompanyID => PX.Data.Update.PXInstanceHelper.CurrentCompany;

        public static void SetCustomInfoCompanyID()
        {
            PXLongOperation.SetCustomInfo(CompanyID, CompanyIDKey);
        }

        public static List<PXProcessingMessage> GetProcessingInfoMessages<TTable>() where TTable : IBqlTable
        {
            var processingMessageList = new List<PXProcessingMessage>();
            var messages = GetProcessingInfo()?.Messages;
            if (messages == null)
            {
                return processingMessageList;
            }

            for (var i = 0; i < messages.Length; i++)
            {
                var processingMessage = messages.Get<TTable>(i);
                if (string.IsNullOrWhiteSpace(processingMessage?.Message))
                {
                    continue;
                }

                processingMessageList.Add(processingMessage);
            }

            return processingMessageList;
        }

        public static void TraceProcessingMessages<TTable>() 
            where TTable : IBqlTable
        {
            TraceProcessingMessages<TTable>(true, true, true);
        }

        public static void TraceProcessingMessages<TTable>(bool traceError, bool traceWarning, bool traceInfo) 
            where TTable : IBqlTable
        {
            TraceProcessingMessages(GetProcessingInfoMessages<TTable>(), traceError, traceWarning, traceInfo);
        }

        public static void TraceProcessingMessages(List<PXProcessingMessage> processingMessages, bool traceError, 
            bool traceWarning, bool traceInfo)
        {
            if (processingMessages == null)
            {
                return;
            }

            //we want to avoid tracing repeat messages
            var messageHashSet = new HashSet<string>();

            foreach (var processingMessage in processingMessages)
            {
                var hashMsg = $"{processingMessage.ErrorLevel}:{processingMessage.Message}";
                if (traceError && IsError(processingMessage.ErrorLevel))
                {
                    if (messageHashSet.Add(hashMsg))
                    {
                        PXTrace.WriteError(processingMessage.Message);
                    }
                    continue;
                }

                if (traceWarning && IsWarning(processingMessage.ErrorLevel) && messageHashSet.Add(hashMsg))
                {
                    if (messageHashSet.Add(hashMsg))
                    {
                        PXTrace.WriteWarning(processingMessage.Message);
                    }
                    continue;
                }

                if (!traceInfo || !messageHashSet.Add(hashMsg))
                {
                    continue;
                }

                PXTrace.WriteInformation(processingMessage.Message);
            }
        }

        private static bool IsError(PXErrorLevel errorLevel)
        {
            return errorLevel == PXErrorLevel.Error || errorLevel == PXErrorLevel.RowError;
        }

        private static bool IsWarning(PXErrorLevel errorLevel)
        {
            return errorLevel == PXErrorLevel.Warning || errorLevel == PXErrorLevel.RowWarning;
        }

        //private static bool IsInformation(PXErrorLevel errorLevel)
        //{
        //    return !IsWarning(errorLevel) && !IsError(errorLevel);
        //}

        private static PXProcessingInfo GetProcessingInfo()
        {
            //Copied from PXProcessing.GetProcessingInfo() which is internal to PX.Data
            return PXLongOperation.GetCustomInfoForCurrentThread("PXProcessingState") as PXProcessingInfo;
        }

        public static Exception GetLongOperationException(PXGraph graph)
        {
            var status = PXLongOperation.GetStatus(graph.UID, out _, out var ex);
            return status == PXLongRunStatus.Aborted || status == PXLongRunStatus.Completed ? ex : null;
        }

        /// <summary>
        /// Check to see if the current graph already has a process that is running and if so throw an exception
        /// </summary>
        /// <param name="graphOfProcess">Graph of process to check</param>
        public static void CheckForProcessIsRunning(PXGraph graphOfProcess)
        {
            var taskInfo = PXLongOperation.GetTaskList().FirstOrDefault(x => x.Screen == graphOfProcess.Accessinfo.ScreenID && !GraphIdsEqual(x.NativeKey, graphOfProcess.UID));
            if (taskInfo != null)
            {
                throw new PXException(AM.Messages.ProcessIsCurrentlyRunning, taskInfo.User, taskInfo.WorkTime);
            }
        }

        /// <summary>
        /// Check to see if the current graph already has a process that is running with matching key/value and if so throw an exception
        /// </summary>
        /// <param name="graphOfProcess">Graph of process to check</param>
        public static void CheckForProcessIsRunningByCompany(PXGraph graphOfProcess)
        {
            CheckForProcessIsRunning(graphOfProcess.Accessinfo.ScreenID, graphOfProcess.UID, CompanyIDKey, CompanyID);
        }

        /// <summary>
        /// Check to see if the current graph already has a process that is running with matching key/value and if so throw an exception
        /// </summary>
        /// <param name="graphType">type of graph related to running process</param>
        public static void CheckForProcessIsRunningByCompany(Type graphType)
        {
            CheckForProcessIsRunning(graphType, CompanyIDKey, CompanyID);
        }


        /// <summary>
        /// Check to see if the current graph already has a process that is running with matching key/value and if so throw an exception
        /// </summary>
        /// <param name="graphOfProcess">Graph of process to check</param>
        /// <param name="key">key custom data to find and match value</param>
        /// <param name="value">value to find in custom data of long running process</param>
        public static void CheckForProcessIsRunning(PXGraph graphOfProcess, string key, object value)
        {
            CheckForProcessIsRunning(graphOfProcess.Accessinfo.ScreenID, graphOfProcess.UID, key, value);
        }

        /// <summary>
        /// Check to see if the current graph already has a process that is running with matching key/value and if so throw an exception
        /// </summary>
        /// <param name="graphType">type of graph related to running process</param>
        /// <param name="key">key custom data to find and match value</param>
        /// <param name="value">value to find in custom data of long running process</param>
        public static void CheckForProcessIsRunning(Type graphType, string key, object value)
        {
            var screenID = Common.SiteMap.GetScreenID(graphType);
            if (string.IsNullOrWhiteSpace(screenID))
            {
                return;
            }
            CheckForProcessIsRunning(screenID, Guid.NewGuid(), key, value);
        }

        /// <summary>
        /// Check to see if the current graph already has a process that is running with matching key/value and if so throw an exception
        /// </summary>
        /// <param name="screenID">screen ID of process</param>
        /// <param name="UID">process ID related to graph</param>
        /// <param name="key">key custom data to find and match value</param>
        /// <param name="value">value to find in custom data of long running process</param>
        public static void CheckForProcessIsRunning(string screenID, object UID, string key, object value)
        {
            foreach (var taskInfo in PXLongOperation.GetTaskList().Where(rowTaskInfo => MatchingScreenIDs(rowTaskInfo.Screen, screenID) && !GraphIdsEqual(rowTaskInfo.NativeKey, UID)).ToList())
            {
                var customInfo = PXLongOperation.GetCustomInfo(taskInfo.NativeKey, key);
                if (customInfo != null && customInfo.Equals(value))
                {
                    throw new PXException(AM.Messages.GetLocal(AM.Messages.ProcessIsCurrentlyRunning, GetUserName(taskInfo), taskInfo.WorkTime))
                    {
                        Data = { { UserKey, taskInfo.User }, { WorkTimeKey, taskInfo.WorkTime } }
                    };
                }
            }
        }

        /// <summary>
        /// Compare the graph UIDs as GUIDs.
        /// Cannot compare as objects as not always equal.
        /// </summary>
        /// <param name="id1"></param>
        /// <param name="id2"></param>
        /// <returns></returns>
        private static bool GraphIdsEqual(object id1, object id2)
        {
            if (id1 is Guid guid && id2 is Guid)
            {
                return guid == (Guid) id2;
            }

            return id1.Equals(id2);
        }

        private static string GetUserName(RowTaskInfo taskInfo)
        {
            // When process runs in a schedule, the user name is empty...
            return string.IsNullOrWhiteSpace(taskInfo?.User) ? $"[{AM.Messages.GetLocal(AM.Messages.Schedule)}]" : taskInfo.User;
        }

        /// <summary>
        /// Condition and match the screen ID values to determine if they are equal
        /// </summary>
        /// <param name="screen1"></param>
        /// <param name="screen2"></param>
        /// <returns></returns>
        private static bool MatchingScreenIDs(string screen1, string screen2)
        {
            return !string.IsNullOrWhiteSpace(screen1) && !string.IsNullOrWhiteSpace(screen2)
                   && screen1.Replace(".", string.Empty).EqualsWithTrim(screen2.Replace(".", string.Empty));
        }

        public static bool ExceptionContainsDataKeys(Exception e)
        {
            return e != null 
                && e.Data.Count > 0 
                && e.Data.Contains(UserKey)
                   && e.Data.Contains(WorkTimeKey);
        }
    }
}