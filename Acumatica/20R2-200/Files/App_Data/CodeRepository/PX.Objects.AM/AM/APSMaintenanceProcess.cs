using System;
using System.Collections.Generic;
using System.Text;
using PX.Objects.AM.Attributes;
using PX.Data;

namespace PX.Objects.AM
{
    public class APSMaintenanceProcess : PXGraph<APSMaintenanceProcess>
    {
        public PXCancel<AMAPSMaintenanceSetup> Cancel;

        /// <summary>
        /// Processing records
        /// </summary>
        [PXFilterable]
        public PXProcessing<AMAPSMaintenanceSetup> ProcessingRecords;

        /// <summary>
        /// Processing page filter
        /// </summary>
        public PXFilter<AMAPSMaintenanceFilter> Filter;

        public PXSetup<AMPSetup> ProductionSetup;

        [PXHidden]
        public PXSelect<AMWCSchd> WorkCenterSchdRecs;
        [PXHidden]
        public PXSelect<AMWCSchdDetail> WorkCenterSchdDetailRecs;

        public PXSetup<AMPSetup> Setup;

        // Turn off the new UI processing window (19R1+)
        public override bool IsProcessing
        {
            get => false;
            set { }
        }

        public APSMaintenanceProcess()
        {
            var setup = Setup.Current;
            AMPSetup.CheckSetup(setup);

            var filter = Filter.Current;
            ProcessingRecords.SetProcessDelegate(delegate (List<AMAPSMaintenanceSetup> list)
            {
                RunProcess(list, filter);
            });
            ProcessingRecords.SetProcessEnabled(false);
            ProcessingRecords.SetProcessVisible(false);
            ProcessingRecords.SetProcessAllVisible(true);
            ProcessingRecords.SetProcessAllCaption(PX.Objects.IN.Messages.Process);

            PXUIFieldAttribute.SetDisplayName<AMAPSMaintenanceSetup.workCenterCalendarProcessLastRunDateTime>(ProcessingRecords.Cache, "Last Run Date");
            PXUIFieldAttribute.SetDisplayName<AMAPSMaintenanceSetup.blockSizeSyncProcessLastRunDateTime>(ProcessingRecords.Cache, "Last Run Date");
            PXUIFieldAttribute.SetDisplayName<AMAPSMaintenanceSetup.historyCleanupProcessLastRunDateTime>(ProcessingRecords.Cache, "Last Run Date");
            PXUIFieldAttribute.SetDisplayName<AMAPSMaintenanceSetup.workCalendarProcessLastRunDateTime>(ProcessingRecords.Cache, "Last Run Date");
        }

        protected DateTime CurrentDateTime => Common.Dates.Now;

        [PXDBCreatedByID(DisplayName = "Last Run By")]
        protected virtual void AMAPSMaintenanceSetup_WorkCenterCalendarProcessLastRunByID_CacheAttached(PXCache sender)
        {
        }

        [PXDBCreatedByID(DisplayName = "Last Run By")]
        protected virtual void AMAPSMaintenanceSetup_BlockSizeSyncProcessLastRunByID_CacheAttached(PXCache sender)
        {
        }

        [PXDBCreatedByID(DisplayName = "Last Run By")]
        protected virtual void AMAPSMaintenanceSetup_HistoryCleanupProcessLastRunByID_CacheAttached(PXCache sender)
        {
        }

        [PXDBCreatedByID(DisplayName = "Last Run By")]
        protected virtual void AMAPSMaintenanceSetup_WorkCalendarProcessLastRunByID_CacheAttached(PXCache sender)
        {
        }

        public static void RunProcess(List<AMAPSMaintenanceSetup> list, AMAPSMaintenanceFilter filter)
        {
            //Should only be a single item in the list...
            if (list == null || list.Count == 0)
            {
                return;
            }

            var maintSetup = list[0];
            if (maintSetup == null)
            {
                return;
            }

            var graph = CreateInstance<APSMaintenanceProcess>();
            graph.Filter.Current = filter;
            graph.RunProcess(maintSetup);
            graph.Persist();
        }

        protected virtual void RunProcess(AMAPSMaintenanceSetup maintSetup)
        {
            if (Filter.Current == null)
            {
                return;
            }

            if (Filter.Current.IsWorkCenterCalendarProcess.GetValueOrDefault())
            {
                RunWorkCenterCalendarProcess(180);
                maintSetup.WorkCenterCalendarProcessLastRunByID = Accessinfo.UserID;
                maintSetup.WorkCenterCalendarProcessLastRunDateTime = CurrentDateTime;
            }

            if (Filter.Current.IsHistoryCleanupProcess.GetValueOrDefault())
            {
                RunHistoryCleanupProcess(ref maintSetup);
            }

            ProcessingRecords.Update(maintSetup);
        }

        protected virtual bool CanSkipHistoryCleanupProcess(AMAPSMaintenanceSetup maintSetup, int hoursFromLastRun)
        {
            return maintSetup?.HistoryCleanupProcessLastRunDateTime != null && hoursFromLastRun > 0 &&
                   (CurrentDateTime - maintSetup.HistoryCleanupProcessLastRunDateTime.GetValueOrDefault()).TotalHours <= hoursFromLastRun;
        }

        protected virtual DateTime GetHistoryCleanupDate()
        {
            return CurrentDateTime.Date.AddMonths(-1);
        }

        public static void RunHistoryCleanupProcess()
        {
            RunHistoryCleanupProcess(10);
        }

        public static void RunHistoryCleanupProcess(int hoursFromLastRun)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var graph = CreateInstance<APSMaintenanceProcess>();

                var setup = (AMAPSMaintenanceSetup)PXSelect<AMAPSMaintenanceSetup>.SelectWindowed(graph, 0, 1) ??
                            graph.ProcessingRecords.Insert();

                if (setup == null)
                {
                    return;
                }

                if (!graph.CanSkipHistoryCleanupProcess(setup, hoursFromLastRun))
                {
                    graph.RunHistoryCleanupProcess(ref setup);
                    graph.ProcessingRecords.Update(setup);
                }

                graph.Persist();
            }
            finally
            {
                sw.Stop();
                var msg = PXTraceHelper.CreateTimespanMessage(sw.Elapsed, "Run History Cleanup Process");
                PXTrace.WriteInformation(msg);
#if DEBUG
                AMDebug.TraceWriteMethodName(msg);
#endif
            }
        }

        private void RunHistoryCleanupProcess(ref AMAPSMaintenanceSetup maintSetup)
        {
            PXTrace.WriteInformation(Messages.GetLocal(Messages.CleanupScheduleHistory));
            var historyCleanupDate = GetHistoryCleanupDate();
            RunHistoryCleanupProcess(historyCleanupDate);
            maintSetup.HistoryCleanupProcessLastRunByID = Accessinfo.UserID;
            maintSetup.HistoryCleanupProcessLastRunDateTime = CurrentDateTime;
        }

        protected virtual void RunHistoryCleanupProcess(DateTime cleanupDate)
        {
            TryDelete<AMMachSchd>(FieldRestrictByDateLessThanEqualTo<AMMachSchd.schdDate>(cleanupDate));
            TryDelete<AMMachSchdDetail>(FieldRestrictByDateLessThanEqualTo<AMMachSchdDetail.schdDate>(cleanupDate));
            TryDelete<AMToolSchdDetail>(FieldRestrictByDateLessThanEqualTo<AMToolSchdDetail.schdDate>(cleanupDate));
            TryDelete<AMWCSchd>(FieldRestrictByDateLessThanEqualTo<AMWCSchd.schdDate>(cleanupDate));
            TryDelete<AMWCSchdDetail>(FieldRestrictByDateLessThanEqualTo<AMWCSchdDetail.schdDate>(cleanupDate));
        }

        protected bool TryDelete<Table>(params PXDataFieldRestrict[] pars)
            where Table : IBqlTable
        {
            try
            {
                return PXDatabase.Delete<Table>(pars);
            }
            catch
            {
                PXTrace.WriteError(Messages.GetLocal(Messages.UnableToDelete, Common.Cache.GetCacheName(typeof(Table))));
                throw;
            }
        }

        protected PXDataFieldRestrict FieldRestrictByDateLessThanEqualTo<TField>(DateTime date)
            where TField : IBqlField
        {
            return new PXDataFieldRestrict(typeof(TField).Name, PXDbType.DateTime, 8, date, PXComp.LE);
        }

        public static void UpdateBlockSizeProcess(int newBlockSize, int oldBlockSize)
        {
            var maintGraph = CreateInstance<APSMaintenanceProcess>();
            var apsMaintSetup = (AMAPSMaintenanceSetup)PXSelect<AMAPSMaintenanceSetup>.Select(maintGraph) ?? maintGraph.ProcessingRecords.Insert();

            var productionSetup = (AMPSetup)PXSelect<AMPSetup>.Select(maintGraph);

            maintGraph.RunBlockSizeSyncProcess(newBlockSize, oldBlockSize);

            apsMaintSetup.LastBlockSize = SchdBlockSizeListAttribute.Contains(oldBlockSize) ? oldBlockSize : newBlockSize;
            apsMaintSetup.BlockSizeSyncProcessLastRunDateTime = Common.Dates.UtcNow;
            apsMaintSetup.BlockSizeSyncProcessLastRunByID = maintGraph.Accessinfo.UserID;
            maintGraph.ProcessingRecords.Update(apsMaintSetup);

            if (productionSetup != null)
            {
                Common.Cache.AddCacheView<AMPSetup>(maintGraph);
                productionSetup.SchdBlockSize = newBlockSize;
                maintGraph.ProductionSetup.Update(productionSetup);
            }

            var cacheCountSb = PXTraceHelper.DirtyCacheRecordCounts(maintGraph);
            if (cacheCountSb != null && cacheCountSb.Length > 0)
            {
                PXTrace.WriteInformation($"Sync Block Size Changes: {System.Environment.NewLine}{cacheCountSb}");
#if DEBUG
                AMDebug.TraceWriteMethodName($"Sync Block Size Changes: {System.Environment.NewLine}{cacheCountSb}");
#endif
            }
            
            maintGraph.Persist();
        }

        protected virtual void RunBlockSizeSyncProcess(int blockSize, int oldBlockSize)
        {
            var workCenters = PXSelect<AMWC>.Select(this).ToFirstTableList();

            foreach (var workCenter in workCenters)
            {
                RunBlockSizeSyncProcess(blockSize, workCenter);
            }
        }

        protected virtual void RunBlockSizeSyncProcess(int blockSize, AMWC workCenter)
        {
            if (workCenter?.WcID == null)
            {
                return;
            }

            if (!SchdBlockSizeListAttribute.Contains(blockSize))
            {
                throw new ArgumentException(nameof(blockSize));
            }

            var syncDateFrom = Common.Dates.UtcToday.AddDays(-15);

            foreach (AMWCSchd schd in PXSelect<
                AMWCSchd, 
                Where<AMWCSchd.schdDate, GreaterEqual<Required<AMWCSchd.schdDate>>,
                    And<AMWCSchd.wcID, Equal<Required<AMWCSchd.wcID>>>>>
                .Select(this, syncDateFrom, workCenter.WcID))
            {
                var workTimeBlocks = ProductionScheduleEngine.MinutesToBlocks(
                    schd.WorkTime.GetValueOrDefault(),
                    blockSize, false);

                if (schd.TotalBlocks.GetValueOrDefault() == workTimeBlocks)
                {
                    continue;
                }

                schd.TotalBlocks = workTimeBlocks;
                WorkCenterSchdRecs.Update(schd);
            }

            foreach (AMWCSchdDetail schdDetail in PXSelect<
                AMWCSchdDetail, 
                Where<AMWCSchdDetail.schdDate, GreaterEqual<Required<AMWCSchd.schdDate>>,
                    And<AMWCSchdDetail.wcID, Equal<Required<AMWCSchdDetail.wcID>>>>>
                .Select(this, syncDateFrom, workCenter.WcID))
            {
                var schdTimeBlocks = ProductionScheduleEngine.MinutesToBlocks(
                    schdDetail.SchdTime.GetValueOrDefault(),
                    blockSize, true);

                if (schdDetail.PlanBlocks.GetValueOrDefault() == schdTimeBlocks)
                {
                    continue;
                }

                schdDetail.PlanBlocks = schdTimeBlocks;
                schdDetail.SchdBlocks = schdDetail.SchdBlocks.GetValueOrDefault() == 0 ? 0 : schdTimeBlocks;
                WorkCenterSchdDetailRecs.Update(schdDetail);
            }
        }

        /// <summary>
        /// Generate work center work days for X number of days
        /// </summary>
        /// <param name="numberOfDays"></param>
        protected virtual void RunWorkCenterCalendarProcess(int numberOfDays)
        {
            if (numberOfDays <= 0)
            {
                throw new ArgumentException(nameof(numberOfDays));
            }

            var fromDate = Common.Dates.UtcToday;
            var toDate = fromDate.AddDays(numberOfDays);

            var sb = new StringBuilder();
            foreach (AMWC workCenter in PXSelect<AMWC>.Select(this))
            {
                try
                {
                    ProductionScheduleEngineAdv.CreateWorkSchdDays(workCenter, fromDate, toDate);
                }
                catch (Exception exception)
                {
                    PXTrace.WriteError(exception);
                    sb.AppendLine(
                        $"{PXUIFieldAttribute.GetDisplayName<AMWC.wcID>(this.Caches<AMWC>())} '{workCenter?.WcID}': {exception.Message}");
                }
            }

            if (sb.Length > 0)
            {
                var esb = new StringBuilder();
                esb.AppendLine(Messages.GetLocal(Messages.UnableToCreateWorkDaysForWC, fromDate.ToShortDateString(),
                    toDate.ToShortDateString()));
                esb.AppendLine(sb.ToString());
                throw new PXException(esb.ToString());
            }
        }
    }

    [PXCacheName("APS Maintenance Setup")]
    [Serializable]
    public class AMAPSMaintenanceFilter : IBqlTable
    {
        #region IsWorkCenterCalendarProcess
        /// <summary>
        /// Update Work Center Schedule from Calendar
        /// </summary>
        public abstract class isWorkCenterCalendarProcess : PX.Data.BQL.BqlBool.Field<isWorkCenterCalendarProcess> { }

        /// <summary>
        /// Update Work Center Schedule from Calendar
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Update Work Center Schedule from Calendar")]
        public virtual bool? IsWorkCenterCalendarProcess { get; set; }
        #endregion

        #region IsBlockSizeSyncProcess
        /// <summary>
        /// Sync Block Size with Schedules
        /// </summary>
        public abstract class isBlockSizeSyncProcess : PX.Data.BQL.BqlBool.Field<isBlockSizeSyncProcess> { }
        /// <summary>
        /// Sync Block Size with Schedules
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Sync Block Size with Schedules")]
        public virtual bool? IsBlockSizeSyncProcess { get; set; }
        #endregion

        #region IsHistoryCleanupProcess
        /// <summary>
        /// Cleanup History
        /// </summary>
        public abstract class isHistoryCleanupProcess : PX.Data.BQL.BqlBool.Field<isHistoryCleanupProcess> { }
        /// <summary>
        /// Cleanup History
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Cleanup History")]
        public virtual bool? IsHistoryCleanupProcess { get; set; }
        #endregion

        #region IsWorkCalendarProcess
        /// <summary>
        /// Sync Work Calendar Changes
        /// </summary>
        public abstract class isWorkCalendarProcess : PX.Data.BQL.BqlBool.Field<isWorkCalendarProcess> { }
        /// <summary>
        /// Sync Work Calendar Changes
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Sync Work Calendar Changes")]
        public virtual bool? IsWorkCalendarProcess { get; set; }
        #endregion

        #region IsWorkCenterUpdatedProcess
        /// <summary>
        /// Sync Work Center Changes
        /// </summary>
        public abstract class isWorkCenterUpdatedProcess : PX.Data.BQL.BqlBool.Field<isWorkCenterUpdatedProcess> { }

        /// <summary>
        /// Sync Work Center Changes
        /// </summary>
        [PXBool]
        [PXUnboundDefault(false)]
        [PXUIField(DisplayName = "Sync Work Center Changes")]
        public virtual bool? IsWorkCenterUpdatedProcess { get; set; }
        #endregion
    }
}