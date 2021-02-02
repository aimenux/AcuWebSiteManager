using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Monthly Schedule.
    /// </summary>
    public abstract class MonthlySchedule : RepeatingSchedule
    {
        /// <summary>
        /// Handles if the rule applies in the [date] using the Frequency of the Schedule.
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            if (DateIsInPeriodAndIsANewDate(date) && IsOnCorrectDate(date))
            {
                int monthsBetweenLastAndCheckDate = ((date.Year - StartOrLastDate.Year) * 12) + date.Month - StartOrLastDate.Month;

                return monthsBetweenLastAndCheckDate % Frequency == 0;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Method to be implemented in child classes. Handles if the rule applies in the [date] depending of the monthly Schedule type.
        /// </summary>
        public abstract bool IsOnCorrectDate(DateTime date);
    }
}
