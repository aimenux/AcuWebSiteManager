using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    public class MachineCalendarHelper : CalendarHelper
    {
        public readonly AMMach Machine;
        public readonly AMWC WorkCenter;

        public AMShift Shift;
        
        public MachineCalendarHelper(PXGraph graph, AMMach machine, AMWC workCenter) 
            : base(graph, SelectCalendar(graph, machine?.CalendarID))
        {
            if (machine?.MachID == null)
            {
                throw new ArgumentNullException(nameof(machine));
            }

            Machine = machine;

            if (workCenter?.WcID == null)
            {
                throw new ArgumentNullException(nameof(workCenter));
            }

            WorkCenter = workCenter;
        }

        public static IEnumerable<DateTime> GetNextWorkDays(List<MachineCalendarHelper> machineCalendars, DateTime dateTime)
        {
            return GetNextWorkDays(machineCalendars, dateTime, false);
        }

        public static IEnumerable<DateTime> GetNextWorkDays(List<MachineCalendarHelper> machineCalendars, DateTime dateTime, bool includeCurrentDate)
        {
            if (machineCalendars == null)
            {
                throw new ArgumentNullException(nameof(machineCalendars));
            }

            foreach (var machineCalendar in machineCalendars)
            {
                var machineWorkDay = machineCalendar.GetNextWorkDay(dateTime, includeCurrentDate);
                if (machineWorkDay == null)
                {
                    continue;
                }

                yield return machineWorkDay.GetValueOrDefault();
            }
        }

        public static DateTime? GetNextWorkDay(List<MachineCalendarHelper> machineCalendars, DateTime dateTime, bool includeCurrentDate)
        {
            if (machineCalendars == null || machineCalendars.Count == 0)
            {
                return dateTime;
            }

            //Flip read direction so we can stay with the earliest date
            var readDirection = machineCalendars.FirstOrDefault()?.CalendarReadDirection ?? ReadDirection.Forward;
            DateTime? bestWorkDay = null;
            foreach (var nextWorkDay in GetNextWorkDays(machineCalendars, dateTime, includeCurrentDate))
            {
                bestWorkDay = bestWorkDay == null
                    ? nextWorkDay
                    : LessDate(readDirection, bestWorkDay, nextWorkDay).GetValueOrDefault();
            }

            //Preserve the time...
            if (bestWorkDay.GetValueOrDefault().Date.Equals(dateTime.Date))
            {
                bestWorkDay = bestWorkDay.GetValueOrDefault().Date + dateTime.TimeOfDay;
            }

            return bestWorkDay;
        }

        public static AMDateInfo AddWorkingDateInfo(List<MachineCalendarHelper> machineCalendars, DateTime dateTime, out int totalWorkingMinutes)
        {
            totalWorkingMinutes = 0;
            if (machineCalendars == null)
            {
                throw new ArgumentNullException(nameof(machineCalendars));
            }

            if (machineCalendars.Count == 0)
            {
                return new AMDateInfo(dateTime, false, EmptyTime, EmptyTime, 0);
            }

            var dateInfo = machineCalendars[0].GetDateInfo(dateTime);
            totalWorkingMinutes = dateInfo.WorkingMinutes;

            if (machineCalendars.Count == 1)
            {
                return dateInfo;
            }

            for (var i = 1; i < machineCalendars.Count; i++)
            {
                var dateInfo2 = machineCalendars[i].GetDateInfo(dateTime);
                if (!dateInfo2.IsWorkingDay || dateInfo.WorkingMinutes == 0)
                {
                    continue;
                }

                dateInfo = AMDateInfo.AddDateInfo(dateInfo, dateInfo2);
                totalWorkingMinutes += dateInfo2.WorkingMinutes;
            }

            return dateInfo;
        }

        public static IEnumerable<string> ToMachineIds(List<MachineCalendarHelper> machineCalendars)
        {
            if (machineCalendars == null)
            {
                yield break;
            }

            foreach (var machineCalendar in machineCalendars)
            {
                if (machineCalendar?.Machine?.MachID == null)
                {
                    continue;
                }

                yield return machineCalendar.Machine.MachID;
            }
        }
    }
}