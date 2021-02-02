using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Monthly Schedule in a specific day of the month.
    /// </summary>
    public class MonthlyScheduleSpecificDay : MonthlySchedule
    {
        /// <summary>
        /// Gets or sets the number of the specific day of the month.
        /// </summary>
        public int DayOfMonth { get; set; }

        /// <summary>
        /// Handles if the rule applies in the specific [date] using the [DayOfMonth]. It will return the last day if the [DayOfMonth] is incorrect for that month.
        /// </summary>
        public override bool IsOnCorrectDate(DateTime date)
        {
            if (date.Day == DayOfMonth)
            {
                return true;
            }
            else if (date.Day == DateTime.DaysInMonth(date.Year, date.Month)
                              && DayOfMonth > date.Day)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
