using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    public class ToolSchdDateDetail : SchdDateDetail<AMToolSchdDetail>
    {
        public readonly AMToolMst Tool;
        public readonly int? SiteID;

        /// <summary>
        /// Total Tool Schedule Qty
        /// </summary>
        protected virtual int ToolScheduleQty =>
            Tool == null || !Tool.ScheduleEnabled.GetValueOrDefault()
                ? 0
                : Tool.ScheduleQty.GetValueOrDefault();

        public ToolSchdDateDetail(AMToolMst tool, int blockSize, AMDateInfo dateInfo, int? siteID)
            : this(tool, blockSize, dateInfo, siteID, null)
        {
        }

        public ToolSchdDateDetail(AMToolMst tool, int blockSize, AMDateInfo dateInfo, int? siteID, List<AMToolSchdDetail> dateDetail)
            : base(blockSize, dateInfo, null)
        {
            if (tool == null || string.IsNullOrWhiteSpace(tool.ToolID))
            {
                throw new PXArgumentException(nameof(tool));
            }
            Tool = tool;
            SiteID = siteID ?? throw new ArgumentNullException(nameof(siteID));

            AvailableDateDetail = BuildAvailableDateDetail(dateDetail, true);
        }

        protected override AMToolSchdDetail MakeSchdDetail(DateTime startTime, DateTime endTime)
        {
            var toolSchd = base.MakeSchdDetail(startTime, endTime);
            if (Tool != null)
            {
                toolSchd.ToolID = Tool.ToolID;
                toolSchd.SiteID = SiteID;
                toolSchd.SchdQty = ToolScheduleQty;
            }
            return toolSchd;
        }

        /// <summary>
        /// Remove any overlaps times for easier processing later. Make the list a consecutive set of time periods
        /// </summary>
        public virtual List<AMToolSchdDetail> FlattenSchdDetail(List<AMToolSchdDetail> toolSchdDetail)
        {
            var list = new List<AMToolSchdDetail>();
            if (toolSchdDetail == null || toolSchdDetail.Count <= 1)
            {
                return toolSchdDetail ?? new List<AMToolSchdDetail>();
            }

            var timePeriods = TimePeriodHelper.ConvertToConsecutiveSequence(ToTimePeriod(toolSchdDetail).ToArray());

            if (timePeriods == null || timePeriods.Count == 0)
            {
                return toolSchdDetail ?? new List<AMToolSchdDetail>();
            }

            // Here we need to translate the time period back to a AMToolSchdDetail and total the schd qty
            foreach (var timePeriod in timePeriods)
            {
                var newToolSchdDetail = MakeToolSchdDetail(timePeriod, toolSchdDetail);
                if (newToolSchdDetail?.ToolID != null)
                {
                    list.Add(newToolSchdDetail);
                }
            }

            return list;
        }

        protected AMToolSchdDetail MakeToolSchdDetail(TimePeriod tp, List<AMToolSchdDetail> toolSchdDetail)
        {
            if (toolSchdDetail == null || toolSchdDetail.Count == 0)
            {
                return null;
            }

            var start = new DateTime(1900, 1, 1) + tp.Start.TimeOfDay;
            var end = new DateTime(1900, 1, 1) + tp.End.TimeOfDay;

            var tsd = new AMToolSchdDetail { StartTime = start, EndTime = end, PlanQty = 0, SchdQty = 0};

            foreach (var sd in toolSchdDetail)
            {
                var sdTimePeriod = ToTimePeriod(sd);
                if (tp.PeriodsOverlap(sdTimePeriod) && !sd.IsBreak.GetValueOrDefault())
                {
                    tsd.ToolID = sd.ToolID;
                    tsd.Description = sd.Description;
                    tsd.SchdDate = sd.SchdDate;
                    tsd.IsBreak = false;
                    tsd.PlanQty += sd.PlanQty.GetValueOrDefault();
                    tsd.SchdQty += sd.SchdQty.GetValueOrDefault();
                }
            }

            if (tsd.ToolID == null)
            {
                return null;
            }

            tsd.OrderByDate = tsd.SchdDate.GetValueOrDefault().Date +
                              tsd.StartTime.GetValueOrDefault().TimeOfDay;
            return tsd;
        }

        protected override AMToolSchdDetail AdjustAvailableRow(AMToolSchdDetail row)
        {
            var copy = row.Copy();
            copy.SchdQty = copy.IsBreak == true ? 0 : (ToolScheduleQty - copy.SchdQty.GetValueOrDefault()).NotLessZero();
            return copy;
        }

        protected override List<AMToolSchdDetail> BuildAvailableDateDetail(List<AMToolSchdDetail> unavailSchdDetails)
        {
            return BuildAvailableDateDetail(unavailSchdDetails, true);
        }

        protected override List<AMToolSchdDetail> BuildAvailableDateDetail(List<AMToolSchdDetail> unavailSchdDetails, bool includeAll)
        {
            if (Tool == null)
            {
                return new List<AMToolSchdDetail>();
            }

            return BuildAvailableQtyDetail(base.BuildAvailableDateDetail(FlattenSchdDetail(unavailSchdDetails), includeAll));
        }

        protected virtual AMToolSchdDetail Merge(AMToolSchdDetail detail1, AMToolSchdDetail detail2)
        {
            var merge = detail1.Copy();

            merge.StartTime = Common.Dates.Compare(detail1.StartTime, detail2.StartTime) <= 0
                ? detail1.StartTime
                : detail2.StartTime;

            merge.EndTime = Common.Dates.Compare(detail1.EndTime, detail2.EndTime) >= 0
                ? detail1.EndTime
                : detail2.EndTime;

            return UpdateSchdDetail(merge);
        }

        protected virtual List<AMToolSchdDetail> BuildAvailableQtyDetail(List<AMToolSchdDetail> allToolDetail)
        {
            if (Tool == null)
            {
                return new List<AMToolSchdDetail>();
            }

            var list = new List<AMToolSchdDetail>();
            if (allToolDetail == null || allToolDetail.Count == 0)
            {
                list.Add(MakeSchdDetail(StartDateTime, EndDateTime));
                return list;
            }

            if (allToolDetail.Count == 1)
            {
                if (allToolDetail[0].SchdQty.GetValueOrDefault() > 0)
                {
                    list.Add(allToolDetail[0]);
                }
                return list;
            }

            var orderedUnavailSchdDetail = OrderList(allToolDetail);
            var previous = allToolDetail[0];

            for (var i = 1; i < orderedUnavailSchdDetail.Count; i++)
            {
                var current = allToolDetail[i];
                if (current.SchdQty.GetValueOrDefault() <= 0)
                {
                    if (previous != null && previous.SchdQty.GetValueOrDefault() > 0)
                    {
                        list.Add(previous);
                    }
                    previous = null;
                    current = null;
                }

                if (previous != null
                    && previous.SchdQty.GetValueOrDefault() == current.SchdQty.GetValueOrDefault()
                    && previous.IsBreak.GetValueOrDefault() == current.IsBreak.GetValueOrDefault())
                {
                    previous = Merge(previous, current);
                }
                else
                {
                    if (previous != null && previous.SchdQty.GetValueOrDefault() > 0)
                    {
                        list.Add(previous);
                    }
                    previous = current;
                }

                // Capture last
                if (i == orderedUnavailSchdDetail.Count - 1
                    && previous != null
                    && previous.SchdQty.GetValueOrDefault() > 0)
                {
                    list.Add(previous);
                }
            }

            return list;
        }

        public virtual SchdDetailResults<AMToolSchdDetail> GetQtyAvailableBlocks(int requestedQty, ReadDirection direction)
        {
            return GetQtyAvailableBlocks(requestedQty, direction, StartDateTime, EndDateTime);
        }

        public virtual SchdDetailResults<AMToolSchdDetail> GetQtyAvailableBlocks(int requestedQty, ReadDirection direction, TimeSpan startingTime, TimeSpan endingTime)
        {
            return GetQtyAvailableBlocks(requestedQty, direction, new DateTime(1900, 1, 1) + startingTime, new DateTime(1900, 1, 1) + endingTime);
        }

        public virtual SchdDetailResults<AMToolSchdDetail> GetQtyAvailableBlocks(int requestedQty, ReadDirection direction, DateTime startDateTime, DateTime endDateTime)
        {
            var summary = new AMToolSchdDetail {IsBreak = false, SchdDate = DateInfo.Date};

            var availableSortedWindowedDetail =
                OrderByDirection(GetWindowedSchdDetail(AvailableDateDetail, startDateTime, endDateTime), direction)
                    .ToList();

            var schdDetailList = new List<AMToolSchdDetail>();

            var requestedTimeAvailable = false;
            var firstDateAvailableTime = false;
            var directionBestAvailTime = direction == ReadDirection.Forward ? StartDateTime : EndDateTime;
            var directionStartingTime = direction == ReadDirection.Forward ? startDateTime : endDateTime;
            var first = true;
            var foundStartingValue = false;
            foreach (var det in availableSortedWindowedDetail)
            {
                if (!foundStartingValue && det.SchdQty.GetValueOrDefault() >= requestedQty)
                {
                    foundStartingValue = IsStartingPoint(det, direction, directionStartingTime);
                }

                if (det.IsBreak.GetValueOrDefault()
                    || !foundStartingValue
                    || det.SchdQty.GetValueOrDefault() < requestedQty)
                {
                    continue;
                }

                var newSchdDetail = det.Copy();

                if (first)
                {
                    first = false;
                    summary.OrderByDate = newSchdDetail.OrderByDate;
                    summary.StartTime = newSchdDetail.StartTime;
                    summary.EndTime = newSchdDetail.EndTime;

                    firstDateAvailableTime =
                        directionBestAvailTime.CompareTo(direction == ReadDirection.Forward
                            ? newSchdDetail.StartTime.GetValueOrDefault()
                            : newSchdDetail.EndTime.GetValueOrDefault()) == 0;

                    requestedTimeAvailable = IsRequestedTime(newSchdDetail, direction, directionStartingTime);
                }

                summary.SchdBlocks = summary.SchdBlocks.GetValueOrDefault() +
                                     newSchdDetail.SchdBlocks.GetValueOrDefault();
                summary.SchdTime = summary.SchdTime.GetValueOrDefault() + newSchdDetail.SchdTime.GetValueOrDefault();
                summary.SchdEfficiencyTime = summary.SchdTime;

                schdDetailList.Add(newSchdDetail);

                if (direction == ReadDirection.Forward)
                {
                    summary.EndTime = newSchdDetail.EndTime;
                }

                if (direction == ReadDirection.Backward)
                {
                    summary.StartTime = newSchdDetail.StartTime;
                }
            }

            return new SchdDetailResults<AMToolSchdDetail>(summary, schdDetailList, firstDateAvailableTime, requestedTimeAvailable);
        }
    }
}