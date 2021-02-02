using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This Class specifies the structure for an daily Schedule.
    /// </summary>
    public class DailySchedule : RepeatingSchedule
    {
        /// <summary>
        /// Handles if the rule applies in the [date] using the Frequency of the Schedule.
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            if (DateIsInPeriodAndIsANewDate(date))
            {
                return DateIsValidForSchedule(date);
            }

            return false;
        }

        /// <summary>
        /// Validate if the [date] is valid for the Schedule using the Frequency.
        /// </summary>
        private bool DateIsValidForSchedule(DateTime date)
        {
            int daysBetweenFirstAndCheckDate = (int)date.Subtract(StartOrLastDate).TotalDays;
            return daysBetweenFirstAndCheckDate % Frequency == 0;
        }
    }
}
