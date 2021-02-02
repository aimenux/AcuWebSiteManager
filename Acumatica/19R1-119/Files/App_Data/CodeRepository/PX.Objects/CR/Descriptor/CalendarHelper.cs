using System;
using System.Collections.Generic;
using System.Text;
using PX.Objects.CS;
using System.Diagnostics;
using PX.Data;

namespace PX.Objects.CR
{
    public class CalendarHelper
    {
        private CSCalendar calendar;
        private CSCalendarExceptions calendarException;
        private DateTime date;

        public CalendarHelper(CSCalendar calendar, CSCalendarExceptions calendarException, DateTime date)
        {
            if (calendar == null)
                throw new ArgumentNullException("calendar");


            this.calendar = calendar;
            this.calendarException = calendarException;
            this.date = date;
        }

        public virtual DateInfo GetInfo()
        {
            if (calendarException != null)
            {
                return new DateInfo(calendarException.WorkDay, calendarException.StartTime, calendarException.EndTime);
            }
            else
            {
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Friday:
						return new DateInfo(calendar.FriWorkDay, calendar.FriStartTime, calendar.FriEndTime);
                    case DayOfWeek.Monday:
						return new DateInfo(calendar.MonWorkDay, calendar.MonStartTime, calendar.MonEndTime);
                    case DayOfWeek.Saturday:
						return new DateInfo(calendar.SatWorkDay, calendar.SatStartTime, calendar.SatEndTime);
                    case DayOfWeek.Sunday:
						return new DateInfo(calendar.SunWorkDay, calendar.SunStartTime, calendar.SunEndTime);
                    case DayOfWeek.Thursday:
						return new DateInfo(calendar.ThuWorkDay, calendar.ThuStartTime, calendar.ThuEndTime);
                    case DayOfWeek.Tuesday:
						return new DateInfo(calendar.TueWorkDay, calendar.TueStartTime, calendar.TueEndTime);
                    case DayOfWeek.Wednesday:
						return new DateInfo(calendar.WedWorkDay, calendar.WedStartTime, calendar.WedEndTime);
                }
                throw new ArgumentOutOfRangeException();
			}
        }

        private bool IsWorkingDay(CSCalendar calendar)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return calendar.FriWorkDay ?? false;
                case DayOfWeek.Monday:
                    return calendar.MonWorkDay ?? false;
                case DayOfWeek.Saturday:
                    return calendar.SatWorkDay ?? false;
                case DayOfWeek.Sunday:
                    return calendar.SunWorkDay ?? false;
                case DayOfWeek.Thursday:
                    return calendar.ThuWorkDay ?? false;
                case DayOfWeek.Tuesday:
                    return calendar.TueWorkDay ?? false;
                case DayOfWeek.Wednesday:
                    break;
                default:
                    break;
            }

            throw new ArgumentOutOfRangeException();
		}

		public virtual void CalculateStartEndTime(out DateTime startDate, out DateTime endDate)
		{
			var info = GetInfo();
			if (info.IsWorkingDay)
			{
				startDate = date.Date + info.StartTime;
				endDate = date.Date + info.EndTime;
			}
			else
			{
				startDate = date.Date;
				endDate = date.Date.AddDays(1D);
			}
		}

		public static void CalculateStartEndTime(PXGraph graph, string calendarID, DateTime date, out DateTime startDate, out DateTime endDate)
		{
			CSCalendar calendar = PXSelect<CSCalendar>.Search<CSCalendar.calendarID>(graph, calendarID);

			if (calendar == null)
				throw new InvalidOperationException(Messages.FailedToSelectCalenderId);

			CSCalendarExceptions cse = PXSelect<CSCalendarExceptions>.
				Search<CSCalendarExceptions.calendarID, CSCalendarExceptions.date>(graph, calendarID, date);
			CalendarHelper helper = new CalendarHelper(calendar, cse, date);
			helper.CalculateStartEndTime(out startDate, out endDate);
		}

        public virtual TimeSpan CalculateOvertimeForOneDay(TimeSpan startTime, TimeSpan endTime)
        {
            TimeSpan overtime = endTime - startTime;
            DateInfo date = GetInfo();

            //Exclude working hours:
            if (startTime >= date.StartTime && endTime <= date.EndTime)
            {
                overtime = overtime - (endTime - startTime);
            }
			else if (startTime >= date.EndTime && endTime >= date.EndTime)
			{
				//full overtime
			}
			else if (startTime <= date.StartTime && endTime <= date.StartTime)
			{
				//full overtime
			}
			else if (startTime >= date.StartTime && endTime > date.EndTime)
            {
                overtime = overtime - (date.EndTime - startTime);
            }
            else if (startTime < date.StartTime && endTime <= date.EndTime)
            {
                overtime = overtime - (endTime - date.StartTime);
            }
            else //if (startTime < date.StartTime && endTime > date.EndTime)
            {
                overtime = overtime - (date.EndTime - date.StartTime);
            }

            return overtime;
        }

		public static TimeSpan CalculateOvertime(PXGraph graph, DateTime start, DateTime end, string calendarId)
		{
			if ((end - start).TotalHours > 24) //NOTE: maybe need to cancel this limitation
				throw new Exception(Messages.DurationActivityExceed24Hours);

			if (end < start)
				throw new Exception(Messages.EndTimeLTStartTime);

			if (string.IsNullOrEmpty(calendarId)) 
				throw new ArgumentNullException("calendarId");

			CSCalendar calendar = PXSelect<CSCalendar>.
				Search<CSCalendar.calendarID>(graph, calendarId);

			if (calendar == null)
				throw new InvalidOperationException(Messages.FailedToSelectCalenderId);

			var overtime = new TimeSpan();

			if (start.Date == end.Date)//within a day
			{
				var startOvertime = CalculateOvertimeForDay(graph, calendar, start.Date, start.TimeOfDay, end.TimeOfDay);
				overtime = overtime.Add(startOvertime);
			}
			else
			{
				var startOvertime = CalculateOvertimeForDay(graph, calendar, start, start.TimeOfDay, new TimeSpan(24, 0, 0));
				overtime = overtime.Add(startOvertime);

				DateTime currentDate = start.Date.AddDays(1D);
				while (currentDate < end.Date)
				{
					var middleOvertime = CalculateOvertimeForDay(graph, calendar, currentDate, currentDate.TimeOfDay, new TimeSpan(24, 0, 0));
					overtime = overtime.Add(middleOvertime);
				}
				var endOvertime = CalculateOvertimeForDay(graph, calendar, currentDate, currentDate.TimeOfDay, end.TimeOfDay);
				overtime = overtime.Add(endOvertime);
			}

			return overtime;
		}

		private static TimeSpan CalculateOvertimeForDay(PXGraph graph, CSCalendar calendar, DateTime date, TimeSpan startTime, TimeSpan endTime)
		{
			var calendarExceptionsStart = (CSCalendarExceptions)PXSelect<CSCalendarExceptions>.
				Search<CSCalendarExceptions.calendarID, CSCalendarExceptions.date>(graph, calendar.CalendarID, date);
			var helperStart = new CalendarHelper(calendar, calendarExceptionsStart, date);
			return helperStart.CalculateOvertimeForOneDay(startTime, endTime);
		}

		public static bool IsHoliday(PXGraph graph, string calendarID, DateTime date)
		{
			var select = SelectCalendar(graph, calendarID, date);
			if (select == null) return false;

			CSCalendar calendar = select;
			CSCalendarExceptions exc = select;

			var result = exc.Date.HasValue && exc.WorkDay == false && calendar.IsWorkDay(date);
			return result;
		}

		public static bool IsWorkDay(PXGraph graph, string calendarID, DateTime date)
		{
			var select = SelectCalendar(graph, calendarID, date);
			if (select == null) return true;

			CSCalendar calendar = select;
			CSCalendarExceptions exc = select;

			var result = exc.Date.HasValue ? exc.WorkDay == true : calendar.IsWorkDay(date);
			return result;
		}

		private static PXResult<CSCalendar, CSCalendarExceptions> SelectCalendar(PXGraph graph, string calendarID, DateTime date)
		{
			var result = (PXResult<CSCalendar, CSCalendarExceptions>)
				PXSelectJoin<
					CSCalendar
					, LeftJoin<CSCalendarExceptions
						, On<CSCalendarExceptions.calendarID, Equal<CSCalendar.calendarID>, And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>
						>
					, Where<CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>
					>.SelectWindowed(graph, 0, 1, date, calendarID);
			return result;
		}

		public static List<DayOfWeek> GetHolydaiByWeek(PXGraph graph, string CalendarID, int year, int WeekNumber)
	    {
		    List<DayOfWeek> holydayList = null;
			DateTime startWeek = PX.Data.EP.PXDateTimeInfo.GetWeekStart(year, WeekNumber);
			
			for (DateTime dateOfWeek = startWeek; dateOfWeek <= startWeek.AddDays(7); dateOfWeek = dateOfWeek.AddDays(1))
			{
				if (!IsWorkDay(graph, CalendarID, dateOfWeek))
					holydayList.Add(dateOfWeek.DayOfWeek);
			}

		    return holydayList;
	    }
    }

    [DebuggerDisplay("WorkingDay={IsWorkingDay} StartTime={StartTime.ToShortTimeString()} EndTime={EndTime.ToShortTimeString()}")]
    public struct DateInfo
    {
        public DateInfo(bool? isWorkingDay, DateTime? startTime, DateTime? endTime)
        {
            this.isWorkingDay = isWorkingDay ?? false;
			this.startTime = this.isWorkingDay ? startTime.Value : new DateTime(2008, 1, 1, 0, 0, 0);
			this.endTime = this.isWorkingDay ? endTime.Value : new DateTime(2008, 1, 1, 0, 0, 0);
        }

        private bool isWorkingDay;
        public bool IsWorkingDay
        {
            get { return isWorkingDay; }
        }

        private DateTime startTime;
        public TimeSpan StartTime
        {
            get { return startTime.TimeOfDay; }
        }

        private DateTime endTime;
        public TimeSpan EndTime
        {
            get { return endTime.TimeOfDay; }
        }

    }
}
