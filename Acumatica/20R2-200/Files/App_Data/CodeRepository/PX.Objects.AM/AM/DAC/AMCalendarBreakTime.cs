using System;
using System.Diagnostics;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Calendar Break Time")]
#if DEBUG
    [DebuggerDisplay("DayOfWeek={DayOfWeek}, StartTime={_StartTime.GetValueOrDefault().ToShortTimeString()}, EndTime={_EndTime.GetValueOrDefault().ToShortTimeString()}, CalendarID={CalendarID}")]
#endif
    public class AMCalendarBreakTime : IBqlTable
    {
        #region CalendarID

        public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }

        protected String _CalendarID;
        [PXDBString(10, IsUnicode = true, IsKey = true)]
        [PXDBDefault(typeof(CSCalendar.calendarID))]
        [PXUIField(DisplayName = "Calendar ID", Enabled = false, Visible = false)]
        [PXParent(typeof(Select<CSCalendar, Where<CSCalendar.calendarID, Equal<Current<AMCalendarBreakTime.calendarID>>>>))]
        public virtual String CalendarID
        {
            get { return this._CalendarID; }
            set { this._CalendarID = value; }
        }
        #endregion
        #region DayOfWeek

	    public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }

	    protected Int32? _DayOfWeek;
	    [PXDBInt(IsKey = true)]
	    [PXDefault(AMDayOfWeek.All)]
	    [PXUIField(DisplayName = "Day Of Week")]
	    [AMDayOfWeek.List]
	    public virtual Int32? DayOfWeek
	    {
		    get { return this._DayOfWeek; }
		    set { this._DayOfWeek = value; }
	    }
	    #endregion
        #region StartTime

        public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }


        protected DateTime? _StartTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false, IsKey = true)]
        [PXUIField(DisplayName = "Start Time")]
        [PXDefault]
        public virtual DateTime? StartTime
        {
            get { return this._StartTime; }
            set { this._StartTime = value; }
        }

        #endregion
        #region EndTime

        public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }


        protected DateTime? _EndTime;

        [PXDBTime(DisplayMask = "t", UseTimeZone = false)]
        [PXUIField(DisplayName = "End Time")]
        [PXDefault]
        public virtual DateTime? EndTime
        {
            get { return this._EndTime; }
            set { this._EndTime = value; }
        }

        #endregion
        #region BreakTime
        public abstract class breakTime : PX.Data.BQL.BqlInt.Field<breakTime> { }

        protected Int32? _BreakTime;
        [PXDBTimeSpanLong]
        [BreakTimeDefault]
        [PXUIField(DisplayName = "Break Time", Enabled = false)]
        [PXFormula(typeof(Default<AMCalendarBreakTime.startTime,AMCalendarBreakTime.endTime>))]
        public virtual Int32? BreakTime
        {
            get
            {
                return _BreakTime;
            }
            set
            {
                _BreakTime = value;
            }
        }
        #endregion
        #region Description

        public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

        protected String _Description;
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        public virtual String Description
        {
            get { return this._Description; }
            set { this._Description = value; }
        }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        protected Guid? _LastModifiedByID;
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get
            {
                return this._LastModifiedByID;
            }
            set
            {
                this._LastModifiedByID = value;
            }
        }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        protected String _LastModifiedByScreenID;
        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get
            {
                return this._LastModifiedByScreenID;
            }
            set
            {
                this._LastModifiedByScreenID = value;
            }
        }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        protected DateTime? _LastModifiedDateTime;
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get
            {
                return this._LastModifiedDateTime;
            }
            set
            {
                this._LastModifiedDateTime = value;
            }
        }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        protected Byte[] _tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get
            {
                return this._tstamp;
            }
            set
            {
                this._tstamp = value;
            }
        }
        #endregion
        
        public class AMDayOfWeek
        {
            /// <summary>
            /// Indicates all day of week days apply (value = 10)
            /// </summary>
            public const int All = 10;

            /// <summary>
            /// List following standard date enum DayOfWeek (0-6 for Sunday-Saturday) plus an "All" for al day of weeks.
            /// </summary>
            public class List : PXIntListAttribute
            {
                public List() : base(
                    new int[] { 0, 1, 2, 3, 4, 5, 6, All },
                    new string[] { EP.Messages.Sunday, EP.Messages.Monday, EP.Messages.Tuesday, EP.Messages.Wednesday, EP.Messages.Thursday, EP.Messages.Friday, EP.Messages.Saturday, Messages.All }){}
            }
        }

        /// <summary>
        /// Makes a copy of the object excluding last mod by and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use CalendarHelper.Copy(AMCalendarBreakTime). " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
#pragma warning disable PX1031 // DACs cannot contain instance methods
        public virtual AMCalendarBreakTime Copy()
#pragma warning restore PX1031
        {
            return new AMCalendarBreakTime
            {
                CalendarID = this.CalendarID,
                DayOfWeek = this.DayOfWeek,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Description = this.Description
            };
        }

        public class BreakTimeDefaultAttribute : PXDefaultAttribute
        {
            public override void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
            {
                var row = (AMCalendarBreakTime) e.Row;
                if (row == null)
                {
                    e.NewValue = 0;
                    return;
                }

                e.NewValue = Convert.ToInt32(GetTotalMinutes(row.StartTime, row.EndTime));
            }

            public static double GetTotalMinutes(DateTime? startDateTime, DateTime? endDatetime)
            {
                if (startDateTime == null || endDatetime == null)
                {
                    return 0;
                }

                return Math.Max((endDatetime.GetValueOrDefault() - startDateTime.GetValueOrDefault()).TotalMinutes, 0);
            }
            
        }
    }
}