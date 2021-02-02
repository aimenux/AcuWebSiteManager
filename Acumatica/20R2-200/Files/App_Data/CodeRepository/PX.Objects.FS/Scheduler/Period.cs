using System;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the Period time for the generation of the Time Slots.
    /// </summary>
    public class Period
    {
        /// <summary>
        /// Gets the beginning date for the Time Slot generation.
        /// </summary>
        public DateTime Start { get; private set; }

        /// <summary>
        /// Gets the end date for the Time Slot generation.
        /// </summary>
        public DateTime? End { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Period class which validates if the start Period time > end Period time.
        /// </summary>
        public Period(DateTime start, DateTime? end)
        {
            this.Start = start.Date;
            this.End = end;

            if (this.End != null && this.Start > this.End.Value)
            {
                throw new ArgumentException(PX.Data.PXMessages.LocalizeFormatNoPrefix(PX.Objects.FS.TX.Error.END_DATE_LESSER_THAN_START_DATE));
            }
        }
    }
}
