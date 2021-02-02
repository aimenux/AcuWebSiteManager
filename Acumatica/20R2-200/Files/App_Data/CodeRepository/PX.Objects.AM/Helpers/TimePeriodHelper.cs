using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PX.Objects.AM
{
    [DebuggerDisplay("{Start.ToShortDateString()} {Start.ToLongTimeString()} - {End.ToShortDateString()} {End.ToLongTimeString()} ")]
    public struct TimePeriod
    {
        public TimePeriod(DateTime start, DateTime end)
        {
            Start = start;
            End = end;

            if (StartAfterEnd(Start, End))
            {
#if DEBUG
                AMDebug.TraceWriteMethodName($"Start {Start.ToShortDateString()} {Start.ToLongTimeString()} is after the End {End.ToShortDateString()} {End.ToLongTimeString()}");
#endif
                throw new ArgumentException("Start DateTime is after the End DateTime");
            }
        }

        public DateTime Start;
        public DateTime End;

        private bool StartAfterEnd(DateTime start, DateTime end)
        {
            return DateTime.Compare(start, end) > 0;
        }
    }

    public static class TimePeriodHelper
    {
        /// <summary>
        /// Make a time period where the end date is always greater than the start date.
        /// This is helpful when working with only times and the end time is an overnight time which makes the time less than the start.
        /// </summary>
        public static TimePeriod MakeSafeTimePeriod(DateTime start, DateTime end)
        {
            var newEnd = end;
            if (DateTime.Compare(start, end) > 0)
            {
                newEnd = start.Date.AddDays(1) + end.TimeOfDay;
            }

            return new TimePeriod(start, newEnd);
        }

        public static bool PeriodsOverlap(params TimePeriod[] timePeriods)
        {
            if (timePeriods == null || timePeriods.Length <= 1)
            {
                return false;
            }

            //var orderedPeriods = timePeriods.OrderBy(x=> x.Start).ToArray();
            var lastPeriod = timePeriods[0];
            for (var i = 1; i < timePeriods.Length; i++)
            {
                var curPeriod = timePeriods[i];
                if (lastPeriod.PeriodsOverlap(curPeriod))
                {
                    return true;
                }

                lastPeriod = curPeriod;
            }

            return false;
        }

        public static bool PeriodsOverlap(this TimePeriod p1, TimePeriod p2)
        {
            return p1.Start < p2.End && p2.Start < p1.End;
        }

        /// <summary>
        /// Does one period start when the other ends?
        /// </summary>
        /// <returns>True if the start of the period is the same as the end of the other</returns>
        public static bool PeriodsAreConsecutiveSequence(this TimePeriod p1, TimePeriod p2)
        {
            return p1.Start == p2.End || p2.Start == p1.End;
        }

        public static bool TryGetTimePeriodBetween(this TimePeriod p1, TimePeriod p2, out TimePeriod pBetween)
        {
            if (PeriodsOverlap(p1, p2) || PeriodsAreConsecutiveSequence(p1, p2))
            {
                pBetween = p2;
                return false;
            }

            pBetween = new TimePeriod(
                p1.End.LessThan(p2.End) ? p1.End : p2.End,
                p1.Start.GreaterThan(p2.Start) ? p1.Start : p2.Start);
            return true;
        }

        public static bool StartLessThan(this TimePeriod p1, TimePeriod p2)
        {
            return p1.Start.LessThan(p2.Start);
        }

        public static bool EndLessThan(this TimePeriod p1, TimePeriod p2)
        {
            return p1.End.LessThan(p2.End);
        }

        public static List<TimePeriod> ConvertToConsecutiveSequence(params TimePeriod[] timePeriods)
        {
            if (timePeriods == null || !PeriodsOverlap(timePeriods))
            {
                if (timePeriods != null)
                {
                    return timePeriods.ToList();
                }

                return new List<TimePeriod>();
            }

            var list = new List<TimePeriod>();

            var orderedPeriods = timePeriods.OrderBy(x=> x.Start).ToArray();
            var lastPeriod = orderedPeriods[0];
            for (var i = 1; i < orderedPeriods.Length; i++)
            {
                var curPeriod = orderedPeriods[i];
                
                list.AddRange(lastPeriod.ToConsecutiveSequence(curPeriod));

                lastPeriod = curPeriod;
                if (list.Count > 0 && i < orderedPeriods.Length-1)
                {
                    lastPeriod = list[list.Count - 1];
                    list.RemoveAt(list.Count-1);
                }
            }

            return list;
        }

        /// <summary>
        /// Given 2 periods we want to transform the periods so they do not overlap
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static IEnumerable<TimePeriod> ToConsecutiveSequence(this TimePeriod p1, TimePeriod p2)
        {
            if (!PeriodsOverlap(p1, p2))
            {
                yield return p1;
                yield return p2;
                yield break;
            }

            //var p1EarlierStart = p1.StartLessThan(p2);

            // When P2 starts before P1
            var firstStart = p2.Start;
            var firstEnd = p1.Start;
            
            // Else When P2 starts after P1
            if (p1.StartLessThan(p2))
            {
                firstStart = p1.Start;
                firstEnd = p2.Start;
            }

            // When P2 ends before P1
            var lastStart = p2.End;
            var lastEnd = p1.End;

            if (p1.EndLessThan(p2))
            {
                lastStart = p1.End;
                lastEnd = p2.End;
            }

            //Avoids same day period which we don't want
            var goodStart = firstStart.LessThanOrEqualTo(firstEnd);
            var goodFinish = lastStart.LessThanOrEqualTo(lastEnd);

            if (goodStart && !firstStart.Equals(firstEnd))
            {
                yield return new TimePeriod(firstStart, firstEnd);
            }

            if (goodStart && goodFinish && TryGetTimePeriodBetween(new TimePeriod(firstStart, firstEnd),
                    new TimePeriod(lastStart, lastEnd), out var between))
            {
                yield return between;
            }

            if (goodFinish && !lastStart.Equals(lastEnd))
            {
                yield return new TimePeriod(lastStart, lastEnd);
            }
        }
    }
}
