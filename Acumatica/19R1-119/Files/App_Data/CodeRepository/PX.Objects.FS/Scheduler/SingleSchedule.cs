using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for a single day Schedule.
    /// </summary>
    public class SingleSchedule : Schedule
    {
        /// <summary>
        /// Gets or sets the specific date of the Single Schedule.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Validates if the Schedule occurs in the parameter [date].
        /// </summary>
        public override bool OccursOnDate(DateTime date)
        {
            return Date.Date == date;
        }
    }
}
