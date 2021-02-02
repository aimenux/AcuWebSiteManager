using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This Class specifies the structure for a weekly Schedule.
    /// </summary>
    public class WeeklySchedule : RepeatingSchedule
    {
        /// <summary>
        /// The list of the days of the week in which the Schedule applies.
        /// </summary>
        private List<DayOfWeek> days;

        /// <summary>
        /// Set the days of the week to the [days] Attribute.
        /// </summary>
        public void SetDays(IEnumerable<DayOfWeek> days)
        {
            this.days = days.Distinct().ToList();
        }

        /// <summary>
        /// Handles if the rule applies in the [date] using the List [days] and the Frequency of the Schedule.
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            if (DateIsInPeriodAndIsANewDate(date) && this.days.Contains(date.DayOfWeek))
            {
                int daysBetweenMondayAndStartOrLastDate = DayOfWeek.Monday - StartOrLastDate.DayOfWeek;
                int daysBetweenMondayAndDate = DayOfWeek.Monday - date.DayOfWeek;

                DateTime mondayOfStartOrLastDate = StartOrLastDate.AddDays(daysBetweenMondayAndStartOrLastDate);
                DateTime mondayOfDate            = date.AddDays(daysBetweenMondayAndDate);

                int daysBetweenLastAndCheckDate = (int)mondayOfStartOrLastDate.Subtract(mondayOfDate).TotalDays / 7;
                return daysBetweenLastAndCheckDate % Frequency == 0;
            }
            else
            {
                return false;
            }
        }
    }
}
