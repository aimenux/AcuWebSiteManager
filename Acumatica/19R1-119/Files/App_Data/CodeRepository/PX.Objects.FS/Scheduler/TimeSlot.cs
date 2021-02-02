using System;

namespace PX.Objects.FS.Scheduler
{
    public class Slot
    {
        /// <summary>
        /// Gets or sets date and time for the beginning of the Slot.
        /// </summary>
        public DateTime DateTimeBegin { get; set; }

        /// <summary>
        /// Gets or sets date and time for the ending of the Slot.
        /// </summary>
        public DateTime DateTimeEnd { get; set; }

        /// <summary>
        /// Gets or sets type of the Slot (Availability, Unavailability).
        /// </summary>
        public string SlotType { get; set; }
    }

    /// <summary>
    /// This class specifies a Time Slot for the recurrence module.
    /// </summary>
    public class TimeSlot : Slot
    {
        /// <summary>
        /// Gets or sets ID of the Appointment in the Database.
        /// </summary>
        public int AppointmentID { get; set; }

        /// <summary>
        /// Gets or sets ID of the Schedule in the Database.
        /// </summary>
        public int ScheduleID { get; set; }
        
        /// <summary>
        /// Gets or sets an additional description for the Slot.
        /// </summary>
        public string Descr { get; set; }
        
        /// <summary>
        /// Gets or sets the priority for the Slot the highest priority is 1.
        /// </summary>
        public int? Priority { get; set; }
        
        /// <summary>
        /// Gets or sets the sequence for the Slot (Routes module).
        /// </summary>
        public int? Sequence { get; set; }
        
        /// <summary>
        /// Gets or sets the GenerationID for the Slot.
        /// </summary>
        public int? GenerationID { get; set; }
    }
}
