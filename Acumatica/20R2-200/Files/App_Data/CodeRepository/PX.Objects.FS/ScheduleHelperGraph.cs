using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using System;
using System.Text;

namespace PX.Objects.FS
{
    public class ScheduleHelperGraph : PXGraph<ScheduleHelperGraph>
    {
        private static readonly Lazy<ScheduleHelperGraph> _scheduleHelperGraph = new Lazy<ScheduleHelperGraph>(CreateInstance<ScheduleHelperGraph>);

        public static ScheduleHelperGraph SingleScheduleHelperGraph => _scheduleHelperGraph.Value;

        public static string GetRecurrenceDescription(FSSchedule fsScheduleRow) => SingleScheduleHelperGraph.GetRecurrenceDescriptionInt(fsScheduleRow);

        #region Virtual Methods

        public virtual string GetDaysOnWeeklyFrequency(FSSchedule fsScheduleRow)
        {
            StringBuilder builder = new StringBuilder();
            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY)
            {
                if (fsScheduleRow.WeeklyOnSun == true) { builder.Append(PXMessages.LocalizeNoPrefix(TX.WeekDays.SUNDAY)); }
                if (fsScheduleRow.WeeklyOnMon == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.MONDAY)); }
                if (fsScheduleRow.WeeklyOnTue == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.TUESDAY)); }
                if (fsScheduleRow.WeeklyOnWed == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.WEDNESDAY)); }
                if (fsScheduleRow.WeeklyOnThu == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.THURSDAY)); }
                if (fsScheduleRow.WeeklyOnFri == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.FRIDAY)); }
                if (fsScheduleRow.WeeklyOnSat == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.WeekDays.SATURDAY)); }
            }

            return builder.ToString();
        }

        public virtual string GetMonthsOnAnnualFrequency(FSSchedule fsScheduleRow)
        {
            StringBuilder builder = new StringBuilder();
            if (fsScheduleRow.FrequencyType == ID.Schedule_FrequencyType.ANNUAL)
            {
                if (fsScheduleRow.AnnualOnJan == true) { builder.Append(PXMessages.LocalizeNoPrefix(TX.Months.JANUARY)); }
                if (fsScheduleRow.AnnualOnFeb == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.FEBRUARY)); }
                if (fsScheduleRow.AnnualOnMar == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.MARCH)); }
                if (fsScheduleRow.AnnualOnApr == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.APRIL)); }
                if (fsScheduleRow.AnnualOnMay == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.MAY)); }
                if (fsScheduleRow.AnnualOnJun == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.JUNE)); }
                if (fsScheduleRow.AnnualOnJul == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.JULY)); }
                if (fsScheduleRow.AnnualOnAug == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.AUGUST)); }
                if (fsScheduleRow.AnnualOnSep == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.SEPTEMBER)); }
                if (fsScheduleRow.AnnualOnOct == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.OCTOBER)); }
                if (fsScheduleRow.AnnualOnNov == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.NOVEMBER)); }
                if (fsScheduleRow.AnnualOnDec == true) { builder.Append(((builder.Length > 0) ? ", " : "") + PXMessages.LocalizeNoPrefix(TX.Months.DECEMBER)); }
            }

            return builder.ToString();
        }

        public virtual string GetRecurrenceDescriptionInt(FSSchedule fsScheduleRow)
        {
            string strMessage = string.Empty;
            string parm0, parm1, parm2, parm3, parm4, parm5, parm6, parm7, parm8, parm9;
            object[] args = null;

            switch (fsScheduleRow.FrequencyType)
            {
                case ID.Schedule_FrequencyType.DAILY:
                    strMessage = TX.RecurrenceDescription.DAILY;
                    args = new object[] { fsScheduleRow.DailyFrequency?.ToString(), TX.RecurrenceDescription.DAYS };
                    break;
                case ID.Schedule_FrequencyType.WEEKLY:
                    strMessage = TX.RecurrenceDescription.WEEKLY;
                    parm2 = GetDaysOnWeeklyFrequency(fsScheduleRow);
                    args = new object[] { fsScheduleRow.WeeklyFrequency?.ToString(), TX.RecurrenceDescription.WEEKS, parm2 };
                    break;
                case ID.Schedule_FrequencyType.MONTHLY:

                    bool isDaily1 = fsScheduleRow.MonthlyRecurrenceType1 == ID.ScheduleMonthlySelection.DAILY;
                    parm0 = fsScheduleRow.MonthlyFrequency?.ToString();
                    parm1 = TX.RecurrenceDescription.MONTHS;
                    parm2 = isDaily1 ? TX.RecurrencyFrecuency.dayOfMonthOrdinals[(short)fsScheduleRow.MonthlyOnDay1 - 1]
                                     : TX.RecurrencyFrecuency.weeksOfMonth[(int)fsScheduleRow.MonthlyOnWeek1 - 1];
                    parm3 = isDaily1 ? TX.RecurrenceDescription.DAY
                                     : TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek1];

                    if (fsScheduleRow.Monthly2Selected == true)
                    {
                        bool isDaily2 = fsScheduleRow.MonthlyRecurrenceType2 == ID.ScheduleMonthlySelection.DAILY;
                        parm4 = isDaily2 ? TX.RecurrencyFrecuency.dayOfMonthOrdinals[(short)fsScheduleRow.MonthlyOnDay2 - 1]
                                         : TX.RecurrencyFrecuency.weeksOfMonth[(int)fsScheduleRow.MonthlyOnWeek2 - 1];
                        parm5 = isDaily2 ? TX.RecurrenceDescription.DAY
                                         : TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek2];

                        if (fsScheduleRow.Monthly3Selected == true)
                        {
                            bool isDaily3 = fsScheduleRow.MonthlyRecurrenceType3 == ID.ScheduleMonthlySelection.DAILY;
                            parm6 = isDaily3 ? TX.RecurrencyFrecuency.dayOfMonthOrdinals[(short)fsScheduleRow.MonthlyOnDay3 - 1]
                                             : TX.RecurrencyFrecuency.weeksOfMonth[(int)fsScheduleRow.MonthlyOnWeek3 - 1];
                            parm7 = isDaily3 ? TX.RecurrenceDescription.DAY
                                             : TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek3];

                            if (fsScheduleRow.Monthly4Selected == true)
                            {
                                bool isDaily4 = fsScheduleRow.MonthlyRecurrenceType4 == ID.ScheduleMonthlySelection.DAILY;
                                parm8 = isDaily4 ? TX.RecurrencyFrecuency.dayOfMonthOrdinals[(short)fsScheduleRow.MonthlyOnDay4 - 1]
                                                 : TX.RecurrencyFrecuency.weeksOfMonth[(int)fsScheduleRow.MonthlyOnWeek4 - 1];
                                parm9 = isDaily4 ? TX.RecurrenceDescription.DAY
                                                 : TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek4];

                                strMessage = TX.RecurrenceDescription.MONTHLY4;
                                args = new object[] { parm0, parm1, parm2, parm3, parm4, parm5, parm6, parm7, parm8, parm9 };
                            }
                            else
                            {
                                strMessage = TX.RecurrenceDescription.MONTHLY3;
                                args = new object[] { parm0, parm1, parm2, parm3, parm4, parm5, parm6, parm7 };
                            }
                        }
                        else
                        {
                            strMessage = TX.RecurrenceDescription.MONTHLY2;
                            args = new object[] { parm0, parm1, parm2, parm3, parm4, parm5 };
                        }
                    }
                    else
                    {
                        strMessage = TX.RecurrenceDescription.MONTHLY1;
                        args = new object[] { parm0, parm1, parm2, parm3 };
                    }
                    break;
                case ID.Schedule_FrequencyType.ANNUAL:

                    parm0 = fsScheduleRow.AnnualFrequency?.ToString();
                    parm1 = TX.RecurrenceDescription.YEARS;

                    if (fsScheduleRow.AnnualRecurrenceType == ID.ScheduleMonthlySelection.WEEKLY)
                    {
                        strMessage = TX.RecurrenceDescription.ANNUAL_WEEK;
                        parm2 = TX.RecurrencyFrecuency.weeksOfMonth[(int)fsScheduleRow.AnnualOnWeek - 1];
                        parm3 = TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.AnnualOnDayOfWeek];
                        parm4 = GetMonthsOnAnnualFrequency(fsScheduleRow);
                        args = new object[] { parm0, parm1, parm2, parm3, parm4 };
                    }
                    else
                    {
                        strMessage = TX.RecurrenceDescription.ANNUAL_MONTH;
                        parm2 = GetMonthsOnAnnualFrequency(fsScheduleRow);
                        parm3 = TX.RecurrencyFrecuency.dayOfMonthOrdinals[(short)fsScheduleRow.AnnualOnDay - 1];
                        args = new object[] { parm0, parm1, parm2, parm3 };
                    }
                    break;
                default:
                    throw new PXSetPropertyException(ErrorMessages.NotSupportedException);
            }

            return PXMessages.LocalizeFormatNoPrefix(strMessage, args);
        }
        #endregion
    }
}