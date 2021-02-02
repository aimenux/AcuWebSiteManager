using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    /// <summary>
    /// Manage a dates scheduled detail (used and available time)
    /// </summary>
    /// <typeparam name="TSchdDetail"></typeparam>
    public class SchdDateDetail<TSchdDetail> where TSchdDetail : ISchdDetail<TSchdDetail>
    {
        public readonly AMDateInfo DateInfo;
        public readonly List<TSchdDetail> UsedDateDetail;
        public List<TSchdDetail> AvailableDateDetail { get; protected set; }
        protected DateTime _BestStartTimeAvailable;
        protected DateTime _BestEndTimeAvailable;
        protected int _BlockSize;

        public SchdDateDetail(int blockSize, AMDateInfo dateInfo) 
            : this(blockSize, dateInfo, null)
        {
        }

        public SchdDateDetail(int blockSize, AMDateInfo dateInfo, List<TSchdDetail> dateDetail)
        {
            if (blockSize < 0)
            {
                throw new PXArgumentException(nameof(blockSize));
            }

            _BlockSize = blockSize;
            DateInfo = dateInfo;

            if (dateDetail != null)
            {
                //make sure list is ordered
                UsedDateDetail = dateDetail.OrderBy(d => d.OrderByDate.GetValueOrDefault()).Where(d => d.SchdTime.GetValueOrDefault() > 0).ToList();
            }

            _BestStartTimeAvailable = Common.Dates.EndOfTimeDate;
            _BestEndTimeAvailable = Common.Dates.EndOfTimeDate;

            if (UsedDateDetail == null)
            {
                UsedDateDetail = new List<TSchdDetail>();
            }

            //make sure the date detail matches the date info date
            if (UsedDateDetail.Any(schdDetail => schdDetail.SchdDate.GetValueOrDefault().CompareTo(DateInfo.Date) != 0))
            {
                throw new PXException(Messages.SchdDateDetailDateMismatch);
            }

            AvailableDateDetail = BuildAvailableDateDetail(UsedDateDetail);
        }

        protected virtual DateTime RoundStartTime(DateTime startTime)
        {
            return ProductionScheduleEngineAdv.BlockRoundedDate(startTime, _BlockSize, false);
        }

        protected virtual DateTime RoundEndTime(DateTime endTime)
        {
            return ProductionScheduleEngineAdv.BlockRoundedDate(endTime, _BlockSize, true);
        }

        protected virtual int OrderByDateAdd(DateTime date)
        {
            return DateInfo.OvernightTime && DateInfo.StartTime.CompareTo(date.TimeOfDay) > 0 ? 1 : 0;
        }

        protected virtual DateTime StartDateTime { get { return RoundStartTime(new DateTime(1900, 1, 1) + DateInfo.StartTime); } }
        protected virtual DateTime EndDateTime { get { return RoundEndTime(new DateTime(1900, 1, 1) + DateInfo.EndTime); } }

        protected virtual TSchdDetail MakeSchdDetail(DateTime startTime, DateTime endTime)
        {
            var x = Activator.CreateInstance<TSchdDetail>();
            x.StartTime = startTime;
            x.EndTime = endTime;
            return UpdateSchdDetail(x);
        }

        protected virtual TSchdDetail UpdateSchdDetail(TSchdDetail schDetail)
        {
            schDetail.OrderByDate = DateInfo.Date.Date.AddDays(OrderByDateAdd(schDetail.StartTime.GetValueOrDefault())) + schDetail.StartTime.GetValueOrDefault().TimeOfDay;
            schDetail.IsBreak = false;
            schDetail.SchdDate = DateInfo.Date;
            schDetail.SchdTime = AMDateInfo.GetDateMinutes(schDetail.StartTime.GetValueOrDefault(), schDetail.EndTime.GetValueOrDefault());
            schDetail.SchdEfficiencyTime = schDetail.SchdTime;
            schDetail.SchdBlocks = ProductionScheduleEngine.MinutesToBlocks(schDetail.SchdEfficiencyTime.GetValueOrDefault(), _BlockSize, true);
            return schDetail;
        }

        protected virtual TSchdDetail AdjustAvailableRow(TSchdDetail row)
        {
            return row;
        }

        protected virtual List<TSchdDetail> OrderList(List<TSchdDetail> list)
        {
            return OrderList(list, ReadDirection.Forward);
        }

        protected virtual List<TSchdDetail> OrderList(List<TSchdDetail> list, ReadDirection direction)
        {
            return OrderByDirection(list, direction).ToList();
        }

        protected static IEnumerable<TSchdDetail> OrderByDirection(IEnumerable<TSchdDetail> ie, ReadDirection direction)
        {
            if (ie == null)
            {
                return null;
            }

            return direction == ReadDirection.Forward
                ? ie.OrderBy(d => d.OrderByDate)
                : ie.OrderByDescending(d => d.OrderByDate);
        }

        public virtual IEnumerable<TSchdDetail> GetWindowedSchdDetail(IEnumerable<TSchdDetail> ie, DateTime startDateTime, DateTime endDateTime)
        {
            if (ie == null)
            {
                yield break;
            }

            foreach (var schdDetail in ie)
            {
                if (schdDetail == null || schdDetail.EndTime.LessThanOrEqualTo(startDateTime) || schdDetail.StartTime.GreaterThanOrEqualTo(endDateTime))
                {
                    continue;
                }

                yield return TrimStartEnd(schdDetail, startDateTime, endDateTime);
            }
        }

        public virtual TSchdDetail TrimStartEnd(TSchdDetail row, DateTime start, DateTime end)
        {
            if (row == null)
            {
                return row;
            }

            var updated = false;
            if (start.GreaterThan(row.StartTime.GetValueOrDefault()))
            {
                row.StartTime = start;
                updated = true;
            }

            if (end.LessThan(row.EndTime.GetValueOrDefault()))
            {
                row.EndTime = end;
                updated = true;
            }

            if (updated)
            {
                UpdateSchdDetail(row);
            }

            return row;
        }

        protected IEnumerable<TimePeriod> ToTimePeriod(IEnumerable<TSchdDetail> schdDetail)
        {
            if (schdDetail == null)
            {
                yield break;
            }

            foreach (var detail in schdDetail)
            {
                yield return ToTimePeriod(detail);
            }
        }

        protected TimePeriod ToTimePeriod(TSchdDetail schdDetail)
        {
            return TimePeriodHelper.MakeSafeTimePeriod(schdDetail.StartTime.GetValueOrDefault(),
                schdDetail.EndTime.GetValueOrDefault());
        }
        
        protected virtual List<TSchdDetail> BuildAvailableDateDetail(List<TSchdDetail> unavailSchdDetails)
        {
            return BuildAvailableDateDetail(unavailSchdDetails, false);
        }

        protected virtual List<TSchdDetail> BuildAvailableDateDetail(List<TSchdDetail> unavailSchdDetails, bool includeAll)
        {
            var list = new List<TSchdDetail>();
            if (unavailSchdDetails == null || unavailSchdDetails.Count == 0)
            {
                list.Add(MakeSchdDetail(StartDateTime, EndDateTime));
                return list;
            }

            var orderedUnavailSchdDetails = OrderList(unavailSchdDetails);

            var first = default(TSchdDetail);
            var last = default(TSchdDetail);

            TSchdDetail lastSchdDetail = orderedUnavailSchdDetails[0];
            if (lastSchdDetail != null && lastSchdDetail.StartTime.GetValueOrDefault(StartDateTime).CompareTo(StartDateTime) > 0)
            {
                _BestStartTimeAvailable = StartDateTime;
                list.Add(MakeSchdDetail(StartDateTime, RoundEndTime(lastSchdDetail.StartTime.GetValueOrDefault(StartDateTime))));
            }

            var adjustedRow = AdjustAvailableRow(orderedUnavailSchdDetails[0]);
            if (includeAll)
            {
                list.Add(adjustedRow);
            }

            if (lastSchdDetail != null && !lastSchdDetail.IsBreak.GetValueOrDefault())
            {
                if (first == null)
                {
                    first = lastSchdDetail;
                }
                last = lastSchdDetail;
            }
            for (var s = 1; s < orderedUnavailSchdDetails.Count; s++)
            {
                var curSchdDetail = AdjustAvailableRow(orderedUnavailSchdDetails[s]);
                var current = BuildAvailableBetween(lastSchdDetail, curSchdDetail);
                lastSchdDetail = curSchdDetail;
                if (current != null)
                {
                    list.Add(current);
                }
                if (includeAll)
                {
                    list.Add(curSchdDetail);
                }
                if (lastSchdDetail != null && !lastSchdDetail.IsBreak.GetValueOrDefault())
                {
                    if (first == null)
                    {
                        first = lastSchdDetail;
                    }
                    last = lastSchdDetail;
                }
            }
            if (lastSchdDetail != null && lastSchdDetail.EndTime.GetValueOrDefault(EndDateTime).CompareTo(EndDateTime) < 0)
            {
                _BestEndTimeAvailable = EndDateTime;
                list.Add(MakeSchdDetail(RoundStartTime(lastSchdDetail.EndTime.GetValueOrDefault(EndDateTime)), EndDateTime));
            }

            if (_BestStartTimeAvailable.CompareTo(Common.Dates.EndOfTimeDate) == 0 
                && first != null && first.StartTime != null)
            {
                _BestStartTimeAvailable = RoundStartTime(first.StartTime.GetValueOrDefault());
            }
            if (_BestEndTimeAvailable.CompareTo(Common.Dates.EndOfTimeDate) == 0
                && last != null && last.EndTime != null)
            {
                _BestEndTimeAvailable = RoundEndTime(last.EndTime.GetValueOrDefault());
            }

            return list;
        }

        protected virtual TSchdDetail BuildAvailableBetween(TSchdDetail value1, TSchdDetail value2)
        {
            if (value1.StartTime == null || value1.EndTime == null
                || value2.StartTime == null || value2.EndTime == null)
            {
                return default(TSchdDetail);
            }

            DateTime v1Start = RoundStartTime(value1.StartTime.GetValueOrDefault());
            DateTime v1End = RoundEndTime(value1.EndTime.GetValueOrDefault());
            DateTime v2Start = RoundStartTime(value2.StartTime.GetValueOrDefault());
            DateTime v2End = RoundEndTime(value2.EndTime.GetValueOrDefault());


            bool overnight2 = AMDateInfo.IsOvernightTime(v2Start, v2End);

            if (v1Start.CompareTo(v2End) > 0 && !overnight2)
            {
                return MakeSchdDetail(v2End, v1Start);
            }

            bool overnight1 = AMDateInfo.IsOvernightTime(v1Start, v1End);

            if (v2Start.CompareTo(v1End) > 0 && !overnight1)
            {
                return MakeSchdDetail(v1End, v2Start);
            }

            return default(TSchdDetail);
        }

        protected virtual bool IsRequestedTime(TSchdDetail schdDetail, ReadDirection direction, DateTime startingTime)
        {
            if (direction == ReadDirection.Forward)
            {
                return startingTime.CompareTo(schdDetail.StartTime.GetValueOrDefault()) >= 0 && startingTime.CompareTo(schdDetail.EndTime.GetValueOrDefault()) < 0;
            }

            return startingTime.CompareTo(schdDetail.StartTime.GetValueOrDefault()) > 0 && startingTime.CompareTo(schdDetail.EndTime.GetValueOrDefault()) <= 0;
        }

        /// <summary>
        /// Determines if the schdDetail is on or after the starting time
        /// </summary>
        protected virtual bool IsStartingPoint(TSchdDetail schdDetail, ReadDirection direction, DateTime startingTime)
        {
            if (direction == ReadDirection.Forward)
            {
                var endTime = DateInfo.OvernightTime
                    ? schdDetail.EndTime.GetValueOrDefault().AddDays(1)
                    : schdDetail.EndTime.GetValueOrDefault();
                return startingTime.CompareTo(endTime) <= 0;
            }

            return startingTime.CompareTo(schdDetail.StartTime.GetValueOrDefault()) >= 0;
        }

        /// <summary>
        /// Returns the available blocks based on the requested total block size and the direction. 
        /// If less block times found it will return them all and the results will indicate as such
        /// </summary>
        /// <param name="requestedBlockSize">Total requested blocks</param>
        /// <param name="direction">Read direction (getting blocks from the start when forward or end of the day when backwards)</param>
        /// <returns>List of available blocks. The blocks will include all available time which could be more or less than the requested size based on block records.</returns>
        public virtual SchdDetailResults<TSchdDetail> GetAvailableBlocks(int requestedBlockSize, ReadDirection direction)
        {
            return GetAvailableBlocks(requestedBlockSize, direction, direction == ReadDirection.Forward ? StartDateTime : EndDateTime);
        }

        public virtual SchdDetailResults<TSchdDetail> GetAvailableBlocks(int requestedBlockSize, ReadDirection direction, TimeSpan startingTime)
        {
            return GetAvailableBlocks(requestedBlockSize, direction, new DateTime(1900,1,1) + startingTime);
        }

        /// <summary>
        /// Returns the available blocks based on the requested total block size and the direction. 
        /// If less block times found it will return them all and the results will indicate as such
        /// </summary>
        /// <param name="requestedBlockSize">Total requested blocks</param>
        /// <param name="direction">Read direction (getting blocks from the start when forward or end of the day when backwards)</param>
        /// <param name="startingTime">The first time the available blocks should be returned from.</param>
        /// <returns>List of available blocks. The blocks will include all available time which could be more or less than the requested size based on block records.</returns>
        public virtual SchdDetailResults<TSchdDetail> GetAvailableBlocks(int requestedBlockSize, ReadDirection direction, DateTime startingTime)
        {
            var summary = Activator.CreateInstance<TSchdDetail>();
            summary.IsBreak = false;
            summary.SchdDate = DateInfo.Date;

            var schdDetailList = new List<TSchdDetail>();

            var orderedList = direction == ReadDirection.Forward
                ? AvailableDateDetail.OrderBy(d => d.OrderByDate).ToList()
                : AvailableDateDetail.OrderByDescending(d => d.OrderByDate).ToList();

            var requestedTimeAvailable = false;
            var firstDateAvailableTime = false;
            var bestAvailDate = direction == ReadDirection.Forward ? _BestStartTimeAvailable : _BestEndTimeAvailable;
            var first = true;
            var foundStartingValue = false;
            foreach (var det in orderedList)
            {
                if (!foundStartingValue)
                {
                    foundStartingValue = IsStartingPoint(det, direction, startingTime);
                }

                if (det.IsBreak.GetValueOrDefault() 
                    || det.SchdBlocks.GetValueOrDefault() == 0
                    || !foundStartingValue)
                {
                    continue;
                }

                var newSchdDetail = det.Copy();

                if (first)
                {
                    first = false;
                    
                    //here we need to make a chopped schd detail based on the starting time if starting time is in the middle of the current sched time
                    if(startingTime.BetweenExclusive(newSchdDetail.StartTime.GetValueOrDefault(), newSchdDetail.EndTime.GetValueOrDefault()))
                    {
                        newSchdDetail.StartTime = direction == ReadDirection.Forward ? startingTime : newSchdDetail.StartTime;
                        newSchdDetail.EndTime = direction == ReadDirection.Forward ? newSchdDetail.EndTime : startingTime;
                        newSchdDetail = UpdateSchdDetail(newSchdDetail);
                    }

                    summary.OrderByDate = newSchdDetail.OrderByDate;
                    summary.StartTime = newSchdDetail.StartTime;
                    summary.EndTime = newSchdDetail.EndTime;

                    firstDateAvailableTime =
                        bestAvailDate.CompareTo(direction == ReadDirection.Forward
                            ? newSchdDetail.StartTime.GetValueOrDefault()
                            : newSchdDetail.EndTime.GetValueOrDefault()) == 0;

                    requestedTimeAvailable = IsRequestedTime(newSchdDetail, direction, startingTime);
                }

                summary.SchdBlocks = summary.SchdBlocks.GetValueOrDefault() + newSchdDetail.SchdBlocks.GetValueOrDefault();
                summary.SchdTime = summary.SchdTime.GetValueOrDefault() + newSchdDetail.SchdTime.GetValueOrDefault();
                summary.SchdEfficiencyTime = summary.SchdTime;

                schdDetailList.Add(newSchdDetail);

                if (summary.SchdBlocks.GetValueOrDefault() >= requestedBlockSize)
                {
                    if (direction == ReadDirection.Forward)
                    {
                        summary.EndTime = newSchdDetail.EndTime;
                    }
                    if (direction == ReadDirection.Backward)
                    {
                        summary.StartTime = newSchdDetail.StartTime;
                    }
                    break;
                }
            }

            return new SchdDetailResults<TSchdDetail>(summary, schdDetailList, firstDateAvailableTime, requestedTimeAvailable);
        }

        /// <summary>
        /// Returns the Schd Detail containing the largest available blocks
        /// </summary>
        /// <returns></returns>
        public TSchdDetail LargestAvailBlocks()
        {
            if (AvailableDateDetail == null)
            {
                return default(TSchdDetail);
            }
            TSchdDetail maxObject = default(TSchdDetail);
            using (var sourceIterator = AvailableDateDetail.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    return maxObject;
                }
                maxObject = sourceIterator.Current;
                int maxValue = 0;
                while (sourceIterator.MoveNext())
                {
                    if (sourceIterator.Current == null)
                    {
                        continue;
                    }

                    if (maxValue.CompareTo(sourceIterator.Current.SchdBlocks.GetValueOrDefault()) < 0)
                    {
                        maxObject = sourceIterator.Current;
                        maxValue = sourceIterator.Current.SchdBlocks.GetValueOrDefault();
                    }
                }
            }
            return maxObject;
        }

        /// <summary>
        /// Get the min and max times from the given detail
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schdDetail"></param>
        /// <returns>Item1 = Min Time; Item2 = Max Time</returns>
        public static Tuple<DateTime?,DateTime?> GetTimeRange(IEnumerable<TSchdDetail> schdDetail)
        {
            DateTime? minDateTime = null;
            DateTime? maxDateTime = null;

            if(schdDetail != null)
            {
                // Using OrderByDate as overnight should sort correctly. Then in case of overnight the max date time might be less than the min date because the date is difference
                var orderedSchdDetail = schdDetail.OrderBy(d => d.OrderByDate).ToList();
                if (orderedSchdDetail.Count > 0)
                {
                    minDateTime = orderedSchdDetail[0]?.StartTime;
                    maxDateTime = orderedSchdDetail[orderedSchdDetail.Count - 1]?.EndTime;
                }
            }

            return new Tuple<DateTime?, DateTime?>(minDateTime, maxDateTime);
        }
        
        public struct SchdDetailResults<T> where T : ISchdDetail<T>
        {
            /// <summary>
            /// Indicates if the results contains the first available schd detail row for the given date and direction requested.
            /// </summary>
            public readonly bool IsDateFirstAvailable;
            /// <summary>
            /// Indicates if the results contains the requested time in the results.
            /// This is helpful when the schedule process needs to know if there is a skip in a sequence and the first available is not the requested starting time slot
            /// (most likely due to another order being scheduled in the first available time slot).
            /// </summary>
            public readonly bool IsTimeRequestedAvailable;
            /// <summary>
            /// Summary values of the results
            /// </summary>
            public readonly T SchdDetailSummary;
            /// <summary>
            /// Schd Detail results related to the requested information
            /// </summary>
            public readonly List<T> Results;

            public SchdDetailResults(T schdDetailSummary, List<T> results, bool isDateFirstAvailable, bool isTimeRequestedAvailable)
            {
                IsDateFirstAvailable = isDateFirstAvailable;
                IsTimeRequestedAvailable = isTimeRequestedAvailable;
                SchdDetailSummary = schdDetailSummary;
                if (schdDetailSummary == null)
                {
                    SchdDetailSummary = Activator.CreateInstance<T>();
                }
                
                Results = results ?? new List<T>();
            }
        }
    }
}