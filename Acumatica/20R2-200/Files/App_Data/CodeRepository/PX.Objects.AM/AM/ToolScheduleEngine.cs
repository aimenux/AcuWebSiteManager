using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    public class ToolScheduleEngine
    {
        private PXGraph _Graph;
        private int _BlockSize;

        public ToolScheduleEngine(PXGraph graph, int blockSize)
        {
            _Graph = graph;

            if(blockSize <= 0)
            {
                throw new ArgumentException(nameof(blockSize));
            }
            _BlockSize = blockSize;
        }

        public PXResultset<AMProdTool> GetProductionTools(IProdOper prodOper)
        {
            if (string.IsNullOrWhiteSpace(prodOper?.ProdOrdID))
            {
                throw new PXArgumentException(nameof(prodOper));
            }

            return PXSelectJoin<AMProdTool,
                InnerJoin<AMToolMst, On<AMProdTool.toolID, Equal<AMToolMst.toolID>>>,
                Where<AMProdTool.orderType, Equal<Required<AMProdTool.orderType>>,
                    And<AMProdTool.prodOrdID, Equal<Required<AMProdTool.prodOrdID>>,
                    And<AMProdTool.operationID, Equal<Required<AMProdTool.operationID>>>>>
                    >.Select(_Graph, prodOper.OrderType, prodOper.ProdOrdID, prodOper.OperationID);
        }

        public IEnumerable<AMToolSchdDetail> GetToolSchdDetail(string toolID, DateTime? schdDate)
        {
            if (string.IsNullOrWhiteSpace(toolID)
                || schdDate == null)
            {
                return null;
            }

            return PXSelect<AMToolSchdDetail,
                Where<AMToolSchdDetail.toolID, Equal<Required<AMToolSchdDetail.toolID>>,
                And<AMToolSchdDetail.schdDate, Equal<Required<AMToolSchdDetail.schdDate>>,
                And<AMToolSchdDetail.schdQty, NotEqual<PX.Objects.CS.int0>>>>>.Select(_Graph, toolID, schdDate).ToFirstTable();
        }

        public virtual bool TrySchedule<T>(AMSchdOper schdOper, ReadDirection scheduleDirection, AMDateInfo dateInfo, List<T> schdDetail, DateTime schedulingDateTime,
            PXResultset<AMProdTool> tools, out DateTime? nextBestToolDateTime, out List<AMToolSchdDetail> newToolSchdDetail)
            where T : ISchdDetail<T>, ISchdReference
        {
            newToolSchdDetail = new List<AMToolSchdDetail>();
            nextBestToolDateTime = null;
            if (tools == null)
            {
                return false;
            }

            var scheduled = true;

            foreach(PXResult<AMProdTool, AMToolMst> prodMstTool in tools)
            {
                var tool = (AMToolMst)prodMstTool;
                if (tool?.ScheduleEnabled != true || tool?.ScheduleQty.GetValueOrDefault() == 0)
                {
                    continue;
                }

                if (TrySchedule(schdOper, scheduleDirection, dateInfo, schdDetail, schedulingDateTime, tool, prodMstTool, GetToolSchdDetail(tool?.ToolID, dateInfo.Date).ToList(), out var nextAvailDateTime, out var toolSchdDetail))
                {
                    if (toolSchdDetail != null)
                    {
                        newToolSchdDetail.AddRange(toolSchdDetail);
                    }
                    continue;
                }

                //Failed to schedule...
                //  We want to figure out what the next best date to calculate is so lets play with the dates...

                scheduled = false;
                if (nextAvailDateTime != null && (nextBestToolDateTime == null || Common.Dates.Compare(nextAvailDateTime, nextBestToolDateTime) == (scheduleDirection == ReadDirection.Forward ? 1 : -1)))
                {
                    nextBestToolDateTime = nextAvailDateTime;
                }
            }

            if (!scheduled)
            {
                newToolSchdDetail.Clear();
            }

            return scheduled;
        }

        protected int ConvertToolQty(decimal? toolQtyDecimal)
        {
            return toolQtyDecimal.ToCeilingInt();
        }

        /// <summary>
        /// Try to schedule a given tool for a given date and schd detail
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schdOper"></param>
        /// <param name="scheduleDirection"></param>
        /// <param name="dateInfo"></param>
        /// <param name="schdDetail">These are the schedule records we are going to plan against. Such as <see cref="AMWCSchdDetail"/> so we can use the calculated time from its calendar.</param>
        /// <param name="schedulingDateTime">The date time currently being scheduled. This date time is the time we need to start scheduling</param>
        /// <param name="tool"></param>
        /// <param name="prodTool"></param>
        /// <param name="existingToolSchdDetail">Any existing tool schedule detail</param>
        /// <param name="nextBestToolDateTime">date to use for next attempt in scheduling process</param>
        /// <param name="newToolSchdDetail">The new calculated tool schd detail to insert into cache (not yet inserted)</param>
        /// <returns>True of the tool can be scheduled with the given parameters</returns>
        public virtual bool TrySchedule<T>(AMSchdOper schdOper, ReadDirection scheduleDirection, AMDateInfo dateInfo, List<T> schdDetail, DateTime schedulingDateTime,
            AMToolMst tool, AMProdTool prodTool, List<AMToolSchdDetail> existingToolSchdDetail, out DateTime? nextBestToolDateTime, out List<AMToolSchdDetail> newToolSchdDetail)
            where T : ISchdDetail<T>, ISchdReference
        {
            newToolSchdDetail = new List<AMToolSchdDetail>();
            nextBestToolDateTime = null;
            if(schdDetail == null)
            {
                return false;
            }

            if (schedulingDateTime == null)
            {
                throw new ArgumentNullException(nameof(schedulingDateTime));
            }

            if(schdOper?.OperationID == null)
            {
                throw new ArgumentNullException(nameof(schdOper));
            }

            if (tool?.ToolID == null)
            {
                throw new ArgumentNullException(nameof(tool));
            }

            if (prodTool?.ToolID == null)
            {
                throw new ArgumentNullException(nameof(prodTool));
            }

            var toolSchd = new ToolSchdDateDetail(tool, _BlockSize, dateInfo, schdOper.SiteID, existingToolSchdDetail);
            var minMaxTimes = SchdDateDetail<T>.GetTimeRange(schdDetail);
            var startTime = minMaxTimes.Item1?.TimeOfDay ?? dateInfo.StartTime;
            var endTime = minMaxTimes.Item2?.TimeOfDay ?? dateInfo.EndTime;

            // We want the smallest window/range for start/end so we are working in the correct window when looking at the available block qty
            if (startTime < dateInfo.StartTime)
            {
                startTime = dateInfo.StartTime;
            }

            if (scheduleDirection == ReadDirection.Forward && startTime < schedulingDateTime.TimeOfDay)
            {
                startTime = schedulingDateTime.TimeOfDay;
            }

            if (endTime > dateInfo.EndTime)
            {
                endTime = dateInfo.EndTime;
            }

            if (scheduleDirection == ReadDirection.Backward && endTime > schedulingDateTime.TimeOfDay)
            {
                endTime = schedulingDateTime.TimeOfDay;
            }
#if DEBUG
            var sbDebug = new System.Text.StringBuilder();
            sbDebug.AppendLine($"ToolID = {tool.ToolID.TrimIfNotNullEmpty()}; TotalSchdQty = {tool.ScheduleQty}; Scheduling Qty = {ConvertToolQty(prodTool.QtyReq)}");
            sbDebug.AppendLine($"SchdOper = {schdOper.DebuggerDisplay}");
            sbDebug.AppendLine($"SchedulingDateTime = {schedulingDateTime.ToShortDateString()}; StartTime = {(schedulingDateTime.Date + startTime).ToLongTimeString()}; EndTime = {(schedulingDateTime.Date + endTime).ToLongTimeString()}");
            if (existingToolSchdDetail != null)
            {
                foreach (var ts in existingToolSchdDetail)
                {
                    sbDebug.AppendLine($"   Existing: {ts.DebuggerDisplay}");
                }
            }
            if (toolSchd.AvailableDateDetail != null)
            {
                foreach (var ts in toolSchd.AvailableDateDetail)
                {
                    sbDebug.AppendLine($"   Available: {ts.DebuggerDisplay}");
                }
            }
            AMDebug.TraceWriteMethodName(sbDebug.ToString());
#endif

            var schdDateDetail = toolSchd.GetQtyAvailableBlocks(ConvertToolQty(prodTool.QtyReq), scheduleDirection, startTime, endTime);
            if (schdDateDetail.Results == null || schdDateDetail.Results.Count == 0)
            {
                //To get a best date value what if we check the remainder of today for another time?
                if (scheduleDirection == ReadDirection.Forward)
                {
                    startTime = endTime;
                    endTime = dateInfo.EndTime;
                }
                else
                {
                    endTime = startTime;
                    startTime = dateInfo.StartTime;
                }
                schdDateDetail = toolSchd.GetQtyAvailableBlocks(ConvertToolQty(prodTool.QtyReq), scheduleDirection, startTime, endTime);
                if ((schdDateDetail.Results?.Count ?? 0) != 0)
                {
                    var nextTime = scheduleDirection == ReadDirection.Forward
                        ? schdDateDetail.Results[0].StartTime
                        : schdDateDetail.Results[schdDateDetail.Results.Count - 1].EndTime;
                    if (nextTime != null)
                    {
                        nextBestToolDateTime = schedulingDateTime.Date + nextTime.GetValueOrDefault().TimeOfDay;
                    }
                }

                return false;
            }

            if (!HasAvailableQty(schdDetail, schdDateDetail.Results, startTime, endTime, out var bestNextStartTime, out var bestNextEndTime))
            {
                nextBestToolDateTime = GetNextBestDateTime(scheduleDirection, schedulingDateTime, dateInfo, bestNextStartTime,
                    bestNextEndTime);
                return false;
            }

            newToolSchdDetail = CreateToolSchdDetail(schdDetail, prodTool, schdOper);

            return true;
        }

        protected DateTime? GetNextBestDateTime(ReadDirection scheduleDirection, DateTime schedulingDateTime, AMDateInfo dateInfo, DateTime? bestNextStartTime, DateTime? bestNextEndTime)
        {
            if (scheduleDirection == ReadDirection.Forward && bestNextEndTime != null)
            {
                //check for overnight
                var days = 0;
                if (bestNextEndTime.GetValueOrDefault().TimeOfDay < schedulingDateTime.TimeOfDay)
                {
                    days = 1;
                }

                return schedulingDateTime.Date.AddDays(days) +
                                bestNextEndTime.GetValueOrDefault().TimeOfDay;
            }

            if (scheduleDirection == ReadDirection.Backward && bestNextStartTime != null)
            {
                //check for overnight
                var days = 0;
                if (bestNextStartTime.GetValueOrDefault().TimeOfDay > schedulingDateTime.TimeOfDay)
                {
                    days = -1;
                }

                return schedulingDateTime.Date.AddDays(days) +
                                bestNextStartTime.GetValueOrDefault().TimeOfDay;
            }

            return null;
        }

        public static bool HasAvailableQty<T>(List<T> schdDetail, List<AMToolSchdDetail> availableToolSchdDetail, TimeSpan startTime, TimeSpan endTime, out DateTime? bestStartTime, out DateTime? bestEndTime)
            where T : ISchdDetail, ISchdReference
        {
            // Using the best start and end times to pass back to the schedule process for the next attempt date/times
            bestStartTime = null;
            bestEndTime = null;
            if (schdDetail == null || schdDetail.Count == 0)
            {
                return false;
            }

            var startDateTimeWindow = new DateTime(1900, 1, 1) + startTime;
            var endDatetimeWindow = new DateTime(1900, 1, 1) + endTime;
            var isQtyShort = false;

            foreach (var sd in schdDetail)
            {
                // Need to adjust the time to fit within the window we are looking for. Adjusted available qty detail is already adjusted
                sd.StartTime = sd.StartTime.GetValueOrDefault().LessThan(startDateTimeWindow)
                    ? startDateTimeWindow
                    : sd.StartTime;

                sd.EndTime = sd.EndTime.GetValueOrDefault().GreaterThan(endDatetimeWindow)
                    ? endDatetimeWindow
                    : sd.EndTime;

                if (sd.StartTime.GreaterThanOrEqualTo(sd.EndTime))
                {
                    continue;
                }

                if (!HasAvailableQty(sd, availableToolSchdDetail, out var bestNextStartDate, out var bestNextEndDate))
                {
                    isQtyShort = true;
                    bestStartTime = bestStartTime == null || bestStartTime.GreaterThan(bestNextStartDate) ? bestNextStartDate : bestStartTime;
                    bestEndTime = bestEndTime == null || bestEndTime.LessThan(bestNextEndDate) ? bestNextEndDate : bestEndTime;
                }
            }

            return !isQtyShort;
        }

        public static bool HasAvailableQty<T>(T schdDetail, List<AMToolSchdDetail> availableToolSchdDetail, out DateTime? bestStartTime, out DateTime? bestEndTime)
            where T : ISchdDetail, ISchdReference
        {
            bestStartTime = null;
            bestEndTime = null;
            if (schdDetail?.StartTime == null || schdDetail.EndTime == null)
            {
                return false;
            }

            if (schdDetail.IsBreak == true)
            {
                return true;
            }

            if (availableToolSchdDetail == null || availableToolSchdDetail.Count == 0)
            {
                return false;
            }

            var startTimeCovered = false;
            var endTimeCovered = false;
            var gapFound = false;
            AMToolSchdDetail lastToolSchd = null;

            foreach (var toolSchd in availableToolSchdDetail.OrderBy(x => x.OrderByDate))
            {
                if (toolSchd?.ToolID == null)
                {
                    continue;
                }

                if (!startTimeCovered &&
                    toolSchd.StartTime.LessThanOrEqualTo(schdDetail.StartTime) &&
                    toolSchd.EndTime.GreaterThan(schdDetail.StartTime))
                {
                    startTimeCovered = true;
                }

                var isGap = lastToolSchd != null && lastToolSchd.EndTime != toolSchd.StartTime;
                if (isGap && !gapFound && startTimeCovered && !endTimeCovered)
                {
                    gapFound = true;
                }

                if (!startTimeCovered && !isGap)
                {
                    bestStartTime = toolSchd.EndTime;
                }

                if (startTimeCovered && !endTimeCovered &&
                    toolSchd.EndTime.GreaterThanOrEqualTo(schdDetail.EndTime) &&
                    toolSchd.StartTime.LessThan(schdDetail.EndTime))
                {
                    endTimeCovered = true;
                }
                
                if (toolSchd.StartTime.GreaterThanOrEqualTo(schdDetail.EndTime) && (isGap || bestEndTime == null))
                {
                    bestEndTime = toolSchd.StartTime;
                }

                lastToolSchd = toolSchd;
            }

            return startTimeCovered && endTimeCovered && !gapFound;
        }

        protected virtual List<AMToolSchdDetail> CreateToolSchdDetail<T>(List<T> schdDetail, AMProdTool prodTool, AMSchdOper schdOper)
            where T : ISchdDetail, ISchdReference
        {
            var results = new List<AMToolSchdDetail>();

            if (schdDetail == null)
            {
                return results;
            }

            foreach (var sd in schdDetail)
            {
                var toolSchdDetail = CreateToolSchdDetail(sd, prodTool, schdOper);
                if (toolSchdDetail == null)
                {
                    continue;
                }

                results.Add(toolSchdDetail);
            }

            return results;
        }

        protected virtual AMToolSchdDetail CreateToolSchdDetail<T>(T schdDetail, AMProdTool prodTool, AMSchdOper schdOper)
            where T : ISchdDetail, ISchdReference
        {
            if (schdDetail == null || schdDetail.IsBreak == true || prodTool?.ToolID == null || schdOper == null)
            {
                return null;
            }

            var toolQty = ConvertToolQty(prodTool.QtyReq);

            return new AMToolSchdDetail
            {
                ToolID = prodTool.ToolID,
                ProdToolNoteID = prodTool.NoteID,
                SiteID = schdOper.SiteID,
                SchdQty = toolQty,
                PlanQty = toolQty,
                SchdDate = schdDetail.SchdDate,
                StartTime = schdDetail.StartTime,
                EndTime = schdDetail.EndTime,
                OrderByDate = schdDetail.OrderByDate,
                SchdTime = schdDetail.SchdTime,
                SchdEfficiencyTime = schdDetail.SchdEfficiencyTime,
                SchdBlocks = schdDetail.SchdBlocks,
                IsBreak = schdDetail.IsBreak,
                SchdKey = schdDetail.SchdKey
            };
        }
    }
}