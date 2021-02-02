using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Monthly Schedule in a specific weekday of the month.
    /// </summary>
    public class MonthlyScheduleWeekDay : MonthlySchedule
    {
        /// <summary>
        /// Gets or sets attribute to specify the number of the week in the month.
        /// </summary>
        public short MonthlyOnWeek { get; set; }

        /// <summary>
        /// Gets or sets attribute to specify the day of the week in which applies the Schedule.
        /// </summary>
        public DayOfWeek MonthlyOnDayOfWeek { get; set; }

        /// <summary>
        /// Validates if the [date] matches with the [MonthlyOnWeek] and [MonthlyOnDayOfWeek] specified in the Schedule.
        /// </summary>
        public override bool IsOnCorrectDate(DateTime date)
        {
            DateTime checkDate = new DateTime(date.Year, date.Month, 1);

            // find the first <DayOfWeek>
            while (checkDate.DayOfWeek != MonthlyOnDayOfWeek)
            {
                checkDate = checkDate.AddDays(1);
            }

            // add the corresponding weeks specified in <MonthlyOnWeek>
            int dayOfMonth = checkDate.Day + ((MonthlyOnWeek - 1) * 7);
            if (dayOfMonth > DateTime.DaysInMonth(checkDate.Year, checkDate.Month))
            {
                dayOfMonth -= 7;
            }

            if (date.Day == dayOfMonth)
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
