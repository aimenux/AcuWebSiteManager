using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class specifies the structure of the Time Slot Generation for a list of Schedules and a Period.
    /// </summary>
    public class TimeSlotGenerator
    {
        /// <summary>
        /// Iterates for every day in the range of the period and check if the Schedules applies.
        /// </summary>
        public List<TimeSlot> GenerateCalendar(Period period, IEnumerable<Schedule> schedules, int? generationID = null)
        {
            List<TimeSlot> timeSlots = new List<TimeSlot>();
            for (DateTime checkDate = period.Start; checkDate <= period.End; checkDate = checkDate.AddDays(1))
            {
                this.AddTimeSlotsForDate(checkDate, schedules, timeSlots, generationID);
            }

            return timeSlots.OrderBy(a => a.Priority).ThenBy(a => a.Sequence).ThenBy(a => a.DateTimeBegin).ThenBy(a => a.DateTimeEnd).ToList<TimeSlot>();
        }

        /// <summary>
        /// Validates if a Schedule applies in a specific day and add every Time Slot generated to the [timeSlots] list.
        /// </summary>
        private void AddTimeSlotsForDate(DateTime checkDate, IEnumerable<Schedule> schedules, List<TimeSlot> timeSlots, int? generationID = null)
        {
            foreach (Schedule schedule in schedules)
            {
                if (schedule.OccursOnDate(checkDate) && schedule.OccursOnSeason(checkDate))
                {
                    if (schedule.Priority == (int)Schedule.ScheduleGenerationPriority.Sequence)
                    {
                        schedule.Sequence = this.setRouteSequence(checkDate, schedule);
                    }

                    timeSlots.Add(this.GenerateTimeSlot(checkDate, schedule, generationID));
                    schedule.LastGeneratedTimeSlotDate = checkDate;
                }
            }
        }

        /// <summary>
        /// Generates a Time Slot using the day of the [checkDate] and the hours of the [schedule.TimeOfDayBegin] and [schedule.TimeOfDayEnd].
        /// </summary>
        private TimeSlot GenerateTimeSlot(DateTime checkDate, Schedule schedule, int? generationID = null)
        {
            return new TimeSlot
            {
                Priority = schedule.Priority,
                Descr = string.IsNullOrEmpty(schedule.Descr) ? schedule.Name : schedule.Descr,
                ScheduleID = schedule.ScheduleID,
                Sequence = schedule.Sequence,
                DateTimeBegin = checkDate.Add(schedule.TimeOfDayBegin),
                DateTimeEnd = checkDate.Add(schedule.TimeOfDayEnd),
                GenerationID = generationID
            };
        }

        /// <summary>
        /// Set sequence to the Schedule using the List of routes defined in the [schedule].RouteInfoList.
        /// </summary>
        private int? setRouteSequence(DateTime checkDate, Schedule schedule)
        {
            RouteInfo routeInfoPivot = schedule.RouteInfoList.ElementAt((int)checkDate.Date.DayOfWeek);

            if (routeInfoPivot.RouteID == null || routeInfoPivot.ShiftID == null)
            {
                routeInfoPivot = schedule.RouteInfoList.Last();
            }

            return routeInfoPivot.Sequence;
        }

        public DateTime? GenerateNextOccurrence(IEnumerable<Schedule> schedules, DateTime fromDate, DateTime? expirationDate, out bool expired)
        {
            DateTime? nextDate = null;
            expired = false;

            DateTime checkDate = fromDate;

            try
            {
                while (nextDate == null && (expirationDate == null || expirationDate.Value >= checkDate))
                {
                    foreach (Scheduler.Schedule schedule in schedules)
                    {
                        if (schedule.OccursOnDate(checkDate) && schedule.OccursOnSeason(checkDate))
                        {
                            if (nextDate == null || nextDate.Value > checkDate)
                            {
                                nextDate = checkDate;
                            }
                        }
                    }

                    checkDate = checkDate.AddDays(1);
                }

                if (expirationDate != null && expirationDate.Value < checkDate)
                {
                    expired = true;
                }

                return nextDate;
            }
            catch
            {
                return null;
            }
        }
    }
}