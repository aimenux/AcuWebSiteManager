using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.FS;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a Annual Schedule.
    /// </summary>
    public abstract class AnnualSchedule : RepeatingSchedule
    {
        /// <summary>
        /// The list of the months of the year in which the Schedule applies.
        /// </summary>
        public List<SharedFunctions.MonthsOfYear> months;

        /// <summary>
        /// Set the months of the year to the [_Months] Attribute.
        /// </summary>
        public void SetMonths(IEnumerable<SharedFunctions.MonthsOfYear> months)
        {
            this.months = months.Distinct().ToList();
        }

        /// <summary>
        /// Handles if the rule applies in the [date] using the Frequency of the Schedule.
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            if (DateIsInPeriodAndIsANewDate(date) && IsOnCorrectDate(date))
            {
                int yearsBetweenLastAndCheckDate = date.Year - StartOrLastDate.Year;

                return yearsBetweenLastAndCheckDate % Frequency == 0;
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
