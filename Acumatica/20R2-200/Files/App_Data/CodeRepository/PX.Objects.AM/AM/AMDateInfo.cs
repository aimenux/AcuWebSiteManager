using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PX.Data;

namespace PX.Objects.AM
{
    [DebuggerDisplay("Date={Date.ToShortDateString()}, WorkingMinutes={WorkingMinutes}, Start={_startTime.ToShortTimeString()}, End={_endTime.ToShortTimeString()}")]
    public struct AMDateInfo
    {
        private const int MINUTES_PER_DAY_INT = 1440;
        private const double MINUTES_PER_HOUR_DOUBLE = 60;
        private readonly bool _isWorkingDay;
        private readonly bool _isExceptionDate;
        private DateTime _date;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _workingTimeNoBreak;
        private readonly int _workingTime;
        private readonly int _breakTime;
        private readonly double _workingHours;
        private double _breakTimeHours;
        private readonly List<AMCalendarBreakTime> _breakTimeList;
        private readonly string _dateDescription;

        /// <summary>
        /// Indicates if the date is a working day
        /// </summary>
        public bool IsWorkingDay => this._isWorkingDay;

        /// <summary>
        /// Date of date info
        /// </summary>
        public DateTime Date => this._date.Date;

        /// <summary>
        /// Date Start Time
        /// </summary>
        public TimeSpan StartTime => this._startTime.TimeOfDay;

        /// <summary>
        /// Date End Time. 
        /// (Could be an overnight time)
        /// </summary>
        public TimeSpan EndTime => this._endTime.TimeOfDay;

        /// <summary>
        /// Indicates the start/end times span overnight. (End time is before start time)
        /// </summary>
        public bool OvernightTime { get; }

        /// <summary>
        /// Indicates the date information came from a calendar exception and not standard date info.
        /// </summary>
        public bool IsExceptionDate => this._isExceptionDate;

        /// <summary>
        /// Total working minutes
        /// (start/end minus break time)
        /// </summary>
        public int WorkingMinutes => this._workingTime;

        /// <summary>
        /// Total working hours
        /// (start/end minus break time)
        /// </summary>
        public double WorkingHours => this._workingHours;

        /// <summary>
        /// Total working hours
        /// (start/end minus break time)
        /// </summary>
        public decimal WorkingHoursDecimal => Convert.ToDecimal(this._workingHours);

        /// <summary>
        /// Break time in minutes
        /// </summary>
        public int BreakMinutes => this._breakTime;

        /// <summary>
        /// List of break times
        /// </summary>
        public List<AMCalendarBreakTime> BreakTimes => _breakTimeList;

        /// <summary>
        /// Description of the date such as exception date description
        /// </summary>
        public string DateDescription => _dateDescription;

        /// <summary>
        /// Convert/calculate minutes to hours
        /// </summary>
        /// <param name="minutes">minutes</param>
        /// <param name="roundPrecision">decimal rounding precision</param>
        /// <returns>Number of Hours</returns>
        public static double MinutesToHours(int minutes, int roundPrecision = 2)
        {
            return Math.Round(minutes / MINUTES_PER_HOUR_DOUBLE, roundPrecision, MidpointRounding.ToEven);
        }

        /// <summary>
        /// Calculate the minutes between the two dates.
        /// (Handles end time greater than start time which indicates an overnight shift)
        /// </summary>
        /// <param name="startDate">Start Date Time</param>
        /// <param name="endDate">End Date Time</param>
        /// <returns>Total Minutes</returns>
        public static int GetDateMinutes(DateTime startDate, DateTime endDate)
        {
            int minutes = Convert.ToInt32(Math.Ceiling((endDate - startDate).TotalMinutes));

            if (minutes < 0 && minutes >= MINUTES_PER_DAY_INT * -1
                && DateTime.Compare(endDate, DateTime.MaxValue.AddDays(-1)) <= 0)
            {
                return Convert.ToInt32(Math.Ceiling((endDate.AddDays(1) - startDate).TotalMinutes));
            }

            return minutes;
        }

        /// <summary>
        /// Selects the earlier start time from the two date info objects
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The earlier start time</returns>
        public static TimeSpan EarlierStartTime(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return dateInfo1.StartTime.CompareTo(dateInfo2.StartTime) <= 0 ? dateInfo1.StartTime : dateInfo2.StartTime;
        }

        /// <summary>
        /// Selects the earlier start time from the two date info objects
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The earlier start time</returns>
        public static DateTime EarlierStartTimeAsDate(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return new DateTime(1900,1,1) + EarlierStartTime(dateInfo1, dateInfo2);
        }

        /// <summary>
        /// Selects the later start time from the two date info objects
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The later start time</returns>
        public static TimeSpan LaterStartTime(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return dateInfo1.StartTime.CompareTo(dateInfo2.StartTime) > 0 ? dateInfo1.StartTime : dateInfo2.StartTime;
        }

        /// <summary>
        /// Selects the later start time from the two date info objects
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The later start time</returns>
        public static DateTime LaterStartTimeAsDate(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return new DateTime(1900, 1, 1) + LaterStartTime(dateInfo1, dateInfo2);
        }

        /// <summary>
        /// Selects the earlier end time from the two date info objects. Overnight end times are treated as later dates (next morning)
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The earlier end time</returns>
        public static TimeSpan EarlierEndTime(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            var d1OvernightAdder = dateInfo1.OvernightTime ? new TimeSpan(24, 0, 0) : new TimeSpan(0, 0, 0);
            var d2OvernightAdder = dateInfo2.OvernightTime ? new TimeSpan(24, 0, 0) : new TimeSpan(0, 0, 0);
            return dateInfo1.EndTime.Add(d1OvernightAdder).CompareTo(dateInfo2.EndTime.Add(d2OvernightAdder)) <= 0 ? dateInfo1.EndTime : dateInfo2.EndTime;
        }

        /// <summary>
        /// Selects the earlier end time from the two date info objects. Overnight end times are treated as later dates (next morning)
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The earlier end time</returns>
        public static DateTime EarlierEndTimeAsDate(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return new DateTime(1900, 1, 1) + EarlierEndTime(dateInfo1, dateInfo2);
        }

        /// <summary>
        /// Selects the later end time from the two date info objects. Overnight end times are treated as later dates (next morning)
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The later end time</returns>
        public static TimeSpan LaterEndTime(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            var d1OvernightAdder = dateInfo1.OvernightTime ? new TimeSpan(24, 0, 0) : new TimeSpan(0, 0, 0);
            var d2OvernightAdder = dateInfo2.OvernightTime ? new TimeSpan(24, 0, 0) : new TimeSpan(0, 0, 0);
            return dateInfo1.EndTime.Add(d1OvernightAdder).CompareTo(dateInfo2.EndTime.Add(d2OvernightAdder)) > 0 ? dateInfo1.EndTime : dateInfo2.EndTime;
        }

        /// <summary>
        /// Selects the later end time from the two date info objects. Overnight end times are treated as later dates (next morning)
        /// </summary>
        /// <param name="dateInfo1">Date 1</param>
        /// <param name="dateInfo2">Date 2</param>
        /// <returns>The later end time</returns>
        public static DateTime LaterEndTimeAsDate(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            return new DateTime(1900, 1, 1) + LaterEndTime(dateInfo1, dateInfo2);
        }

        public static bool IsOvernightTime(DateTime startDate, DateTime endDate)
        {
            int minutes = Convert.ToInt32(Math.Ceiling((endDate - startDate).TotalMinutes));

            return minutes < 0 && minutes >= MINUTES_PER_DAY_INT*-1;
        }

        private static int CalculateBreakTime(List<AMCalendarBreakTime> breakTimes)
        {
            if (breakTimes == null || breakTimes.Count == 0)
            {
                return 0;
            }
            return breakTimes.Sum(amCalendarBreakTime => GetDateMinutes(amCalendarBreakTime.StartTime.GetValueOrDefault(), amCalendarBreakTime.EndTime.GetValueOrDefault()));
        }

        public static AMDateInfo AddDateInfo(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            if (dateInfo1.Date.CompareTo(dateInfo2.Date) != 0)
            {
                throw new ArgumentException($"{nameof(dateInfo1)} = {dateInfo1.Date}; {nameof(dateInfo2)} = {dateInfo2.Date}");
            }

            if (dateInfo1.IsWorkingDay && !dateInfo2.IsWorkingDay)
            {
                return dateInfo1;
            }

            if (!dateInfo1.IsWorkingDay && dateInfo2.IsWorkingDay)
            {
                return dateInfo2;
            }

            if (!dateInfo1.IsWorkingDay && !dateInfo2.IsWorkingDay)
            {
                return CalendarHelper.MakeNonWorkingDateInfo(dateInfo1.Date);
            }

            int gapTime = GapTime(dateInfo1, dateInfo2);

            return new AMDateInfo(dateInfo1.Date,
                dateInfo1.IsWorkingDay && dateInfo2.IsWorkingDay,
                AMDateInfo.EarlierStartTimeAsDate(dateInfo1, dateInfo2),
                AMDateInfo.LaterEndTimeAsDate(dateInfo1, dateInfo2),
                dateInfo1.BreakMinutes + dateInfo2.BreakMinutes + gapTime,
                dateInfo1.IsExceptionDate || dateInfo2.IsExceptionDate);
            //adding up the break time above assumes the break times to no overlap.
            //  Once we have actual time in this calculation (defined break time of day) then we can determine break time overlaps
        }

        /// <summary>
        /// Find the gap between the two date info in units of minutes.
        /// A gap exists when the end date of one date info is before the start of the other. With
        /// overlapping end/start times the result is zero.
        /// </summary>
        /// <returns>Total Gap Minutes</returns>
        public static int GapTime(AMDateInfo dateInfo1, AMDateInfo dateInfo2)
        {
            if (dateInfo1.StartTime.CompareTo(dateInfo2.EndTime) > 0 && !dateInfo2.OvernightTime)
            {
                return TotalMinutesBetween(dateInfo2.EndTime, dateInfo1.StartTime);
            }

            if (dateInfo2.StartTime.CompareTo(dateInfo1.EndTime) > 0 && !dateInfo1.OvernightTime)
            {
                return TotalMinutesBetween(dateInfo1.EndTime, dateInfo2.StartTime);
            }

            return 0;
        }

        /// <summary>
        /// Total minutes between the two timespans
        /// </summary>
        /// <param name="startTimeSpan">start time</param>
        /// <param name="endTimeSpan">end time</param>
        /// <returns>total minutes (rounded up)</returns>
        public static int TotalMinutesBetween(TimeSpan startTimeSpan, TimeSpan endTimeSpan)
        {
            return Convert.ToInt32(Math.Ceiling((endTimeSpan - startTimeSpan).TotalMinutes));
        }

        /// <summary>
        /// Condition date time for hours/minutes only
        /// </summary>
        public static DateTime ConditionDateTime(DateTime dateTime)
        {
            return new DateTime(1900, 1, 1, dateTime.Hour, dateTime.Minute, 0);
        }

        public AMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, List<AMCalendarBreakTime> breakTimes, bool isExceptionDate, string dateDescription)
            : this(date, isWorkingDay, startTime, endTime, breakTimes)
        {
            this._dateDescription = dateDescription;
            this._isExceptionDate = isExceptionDate;
        }

        public AMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, List<AMCalendarBreakTime> breakTimes) 
            : this(date, isWorkingDay, startTime, endTime, CalculateBreakTime(breakTimes))
        {
            if (breakTimes != null)
            {
                this._breakTimeList = breakTimes;
            }
        }

        public AMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, int breakTime, bool isExceptionDate, string dateDescription)
            : this(date, isWorkingDay, startTime, endTime, breakTime, isExceptionDate)
        {
            this._dateDescription = dateDescription;
        }

        public AMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, int breakTime, bool isExceptionDate)
            : this(date, isWorkingDay, startTime, endTime, breakTime)
        {
            this._isExceptionDate = isExceptionDate;
        }

        public AMDateInfo(DateTime date, bool? isWorkingDay, DateTime? startTime, DateTime? endTime, int breakTime)
        {
            this._breakTimeList = new List<AMCalendarBreakTime>();
            this._dateDescription = string.Empty;
            this._isExceptionDate = false;

            this._date = date;
            bool? nullable = isWorkingDay;
            this._isWorkingDay = nullable.HasValue && nullable.GetValueOrDefault();
            this._startTime = Common.Dates.BeginOfTimeDate;
            this._endTime = Common.Dates.BeginOfTimeDate;
            if (startTime != null && endTime != null)
            {
                this._startTime = this._isWorkingDay ? ConditionDateTime(startTime.Value) : Common.Dates.BeginOfTimeDate;
                this._endTime = this._isWorkingDay ? ConditionDateTime(endTime.Value) : Common.Dates.BeginOfTimeDate;
            }

            this._breakTime = breakTime < 0 ? 0 : breakTime;
            OvernightTime = IsOvernightTime(this._startTime, this._endTime);
            _workingTimeNoBreak = GetDateMinutes(this._startTime, this._endTime);

            //check before looking at working day as we still need to make sure we are within a valid "day" of time
            if (_workingTimeNoBreak > MINUTES_PER_DAY_INT)
            {
                throw new PXException(Messages.StartEndTimeMoreThan24Hours);
            }

            this._workingTime = this._isWorkingDay ? _workingTimeNoBreak - this._breakTime : 0;

            if (this._workingTime < 0)
            {
                this._workingTime = 0;
            }

            // the only way to get "24 hours" is to enter 23 hours and 59 minutes of time on the calendar.
            // If the times are the same it would result in a zero hour day. Bump up to 24
            if (this._workingTime == MINUTES_PER_DAY_INT - 1)
            {
                this._workingTime = MINUTES_PER_DAY_INT;
            }

            this._workingHours = MinutesToHours(_workingTime);
            this._breakTimeHours = MinutesToHours(_breakTime);
        }
    }
}