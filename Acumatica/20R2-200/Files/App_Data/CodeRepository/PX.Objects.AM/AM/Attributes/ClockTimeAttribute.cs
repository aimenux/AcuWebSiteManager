using PX.Data;
using System;

namespace PX.Objects.AM.Attributes
{
    public class ClockTimeAttribute : PXIntAttribute, IPXFieldSelectingSubscriber
    {
        public static DateTime PunchDateTime => Common.Dates.NowTimeOfDay;

        private Type _PunchInDateTime;
        private Type _PunchOutDateTime;

        public ClockTimeAttribute(Type punchInDateTime, Type punchOutDateTime) : this(punchInDateTime)
        {
            _PunchOutDateTime = punchOutDateTime;
        }

        public ClockTimeAttribute(Type punchInDateTime) : base()
        {
            _PunchInDateTime = punchInDateTime;
        }

        public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var punchInDateTimeValue = sender.GetValue(e.Row, _PunchInDateTime.Name) as DateTime?;

            if (punchInDateTimeValue == null)
            {
                return;
            }

            var punchOutDateTimeValue = _PunchOutDateTime != null
                ? (sender.GetValue(e.Row, _PunchOutDateTime.Name) as DateTime? ?? Common.Current.BusinessTimeOfDay(sender.Graph))
                : Common.Current.BusinessTimeOfDay(sender.Graph);

            e.ReturnValue = GetTimeBetween(punchInDateTimeValue, punchOutDateTimeValue);
        }

        public static int GetTimeBetween(DateTime? startDateTime, DateTime? endDateTime)
        {
            if (startDateTime == null || endDateTime == null)
            {
                return 0;
            }
            TimeSpan timeSpan;
            //this code was used to calculate time going over midnight when date was not stored
            //use this calculation for legacy data that still has 1/1/1900
            if (startDateTime.Value.Year == 1900 || endDateTime.Value.Year == 1900)
            {
                if (StartBeforeEnd(startDateTime.Value, endDateTime.Value))
                    timeSpan = Common.Dates.BeginOfTimeDate.Add(endDateTime.Value.TimeOfDay)
                    - Common.Dates.BeginOfTimeDate.Add(startDateTime.Value.TimeOfDay);
                else
                    timeSpan = Common.Dates.BeginOfTimeDate.AddDays(1).Add(endDateTime.Value.TimeOfDay)
                    - Common.Dates.BeginOfTimeDate.Add(startDateTime.Value.TimeOfDay);
            }
            else
                timeSpan = endDateTime.Value - startDateTime.Value;
            return Convert.ToInt32(timeSpan.TotalMinutes);


        }

        public static bool StartBeforeEnd(DateTime startDateTime, DateTime endDateTime)
        {
            return TimeSpan.Compare(startDateTime.TimeOfDay, endDateTime.TimeOfDay) <= 0;
        }
    }
}