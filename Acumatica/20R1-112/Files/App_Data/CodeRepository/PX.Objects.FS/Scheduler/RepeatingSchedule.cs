using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure for repeating Schedules.
    /// </summary>
    public abstract class RepeatingSchedule : Schedule
    {
        /// <summary>
        /// Used to specify the frequency of a Schedule rule.
        /// </summary>
        private int frequency;

        /// <summary>
        /// Gets or sets the period to be consider in order to generate the Time Slots for the specific repeating Schedule.
        /// </summary>
        public Period SchedulingRange { get; set; }

        /// <summary>
        /// Gets or sets attribute to specify the frequency. It also validates if the value > 0.
        /// </summary>
        public int Frequency
        {
            get
            {
                return this.frequency;
            }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("There must be at least one day between time-slots");
                }

                this.frequency = value;
            }
        }

        /// <summary>
        /// Gets attribute to specify the Date of the last successful Time Slot generated with this Schedule. It will be set as SchedulingRange if the [LastGeneratedTimeSlotDate] is null.
        /// </summary>
        protected DateTime StartOrLastDate 
        {
            get
            {
                if (this.LastGeneratedTimeSlotDate == null)
                {
                    return SchedulingRange.Start;
                }
                else
                {
                    return (DateTime)this.LastGeneratedTimeSlotDate;
                }
            }
        }

        /// <summary>
        /// Validates if the [date] is within the Scheduling Range and if it has not been already generated in a previous run of the Schedule.
        /// </summary>
        protected bool DateIsInPeriodAndIsANewDate(DateTime date)
        {
            if (SchedulingRange == null)
            {
                return false;
            }

            return (date >= SchedulingRange.Start && (SchedulingRange.End == null || date <= SchedulingRange.End))
                    && (this.LastGeneratedTimeSlotDate == null || date > this.LastGeneratedTimeSlotDate);
        }
    }
}
