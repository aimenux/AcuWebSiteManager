using System;
using System.Collections.Generic;
using PX.Objects.FS;
using PX.Data;

namespace PX.Objects.FS.Scheduler
{
    /// <summary>
    /// This class allows to map a FSSchedule in the Service Management module to a Schedule in the Scheduler module.
    /// </summary>
    public static class MapFSScheduleToSchedule
    {
        /// <summary>
        /// SubSchedule types, defined to split a complex FSSchedule in multiple Schedules.
        /// </summary>
        public class SubScheduleType
        {
            public const int FIRST  = 1;
            public const int SECOND = 2;
            public const int THIRD  = 3;
            public const int FOURTH = 4;
        }

        /// <summary>
        /// This function converts a FSSchedule to a List[Schedule].
        /// </summary>
        public static List<Schedule> convertFSScheduleToSchedule(PXCache cache, FSSchedule fsScheduleRow, DateTime? toDate, string recordType, Period period = null)
        {
            List<Schedule> results = new List<Schedule>();
            switch (fsScheduleRow.FrequencyType)
            {
                #region dailyFreequency
                case ID.Schedule_FrequencyType.DAILY:
                    results.Add(mapDailyFrequency(cache, fsScheduleRow, toDate, recordType, period));
                    break;
                #endregion
                #region weeklyFreequency
                case ID.Schedule_FrequencyType.WEEKLY:
                    results.Add(mapWeeklyFrequency(cache, fsScheduleRow, toDate, recordType, period));
                    break;
                #endregion
                #region MonthlyFreequency
                case ID.Schedule_FrequencyType.MONTHLY:
                    results.Add(mapMonthlyFrequency(cache, fsScheduleRow, SubScheduleType.FIRST, toDate, recordType, fsScheduleRow.MonthlyRecurrenceType1, period));
                    if (fsScheduleRow.Monthly2Selected == true)
                    {
                        results.Add(mapMonthlyFrequency(cache, fsScheduleRow, SubScheduleType.SECOND, toDate, recordType, fsScheduleRow.MonthlyRecurrenceType2, period));
                    }

                    if (fsScheduleRow.Monthly3Selected == true)
                    {
                        results.Add(mapMonthlyFrequency(cache, fsScheduleRow, SubScheduleType.THIRD, toDate, recordType, fsScheduleRow.MonthlyRecurrenceType3, period));
                    }

                    if (fsScheduleRow.Monthly4Selected == true)
                    {
                        results.Add(mapMonthlyFrequency(cache, fsScheduleRow, SubScheduleType.FOURTH, toDate, recordType, fsScheduleRow.MonthlyRecurrenceType4, period));
                    }

                    break;
                case ID.Schedule_FrequencyType.ANNUAL:
                    results.Add(mapAnnualFrequency(fsScheduleRow, toDate, period));
                    break;
                #endregion
            }

            return results;
        }

        /// <summary>
        /// This function maps a FSSchedule daily frequency to a DailySchedule in the Scheduler module.
        /// </summary>
        public static Schedule mapDailyFrequency(PXCache cache, FSSchedule fsScheduleRow, DateTime? toDate, string recordType, Period period = null)
        {
            bool applySeasonInSchedule = recordType != ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT ? SharedFunctions.GetEnableSeasonSetting(cache.Graph, fsScheduleRow) : false;

            var dailySchedule = new DailySchedule
            {
                Name = TX.FrecuencySchedule.DAILY,
                EntityID = fsScheduleRow.EntityID,
                EntityType = fsScheduleRow.EntityType,
                LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                ScheduleID = (int)fsScheduleRow.ScheduleID,
                SubScheduleID = 0,
                TimeOfDayBegin = new TimeSpan(5, 0, 0), //TODO SD-5493
                TimeOfDayEnd = new TimeSpan(11, 0, 0),
                SchedulingRange = period ?? new Period((DateTime)(fsScheduleRow.StartDate ?? toDate), fsScheduleRow.EndDate),
                Frequency = (int)fsScheduleRow.DailyFrequency,
                Descr = fsScheduleRow.ContractDescr,
                ApplySeason = applySeasonInSchedule,
                SeasonOnJan = fsScheduleRow.SeasonOnJan,
                SeasonOnFeb = fsScheduleRow.SeasonOnFeb,
                SeasonOnMar = fsScheduleRow.SeasonOnMar,
                SeasonOnApr = fsScheduleRow.SeasonOnApr,
                SeasonOnMay = fsScheduleRow.SeasonOnMay,
                SeasonOnJun = fsScheduleRow.SeasonOnJun,
                SeasonOnJul = fsScheduleRow.SeasonOnJul,
                SeasonOnAug = fsScheduleRow.SeasonOnAug,
                SeasonOnSep = fsScheduleRow.SeasonOnSep,
                SeasonOnOct = fsScheduleRow.SeasonOnOct,
                SeasonOnNov = fsScheduleRow.SeasonOnNov,
                SeasonOnDec = fsScheduleRow.SeasonOnDec
            };
            return dailySchedule;
        }

        /// <summary>
        /// This function maps a FSSchedule weekly frequency to a WeeklySchedule in the Scheduler module.
        /// </summary>
        public static Schedule mapWeeklyFrequency(PXCache cache, FSSchedule fsScheduleRow, DateTime? toDate, string recordType, Period period = null)
        {
            bool applySeasonInSchedule = recordType != ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT ? SharedFunctions.GetEnableSeasonSetting(cache.Graph, fsScheduleRow) : false;

            List<DayOfWeek> days = new List<DayOfWeek>();
            var weeklySchedule = new WeeklySchedule
            {
                Name = TX.FrecuencySchedule.WEEKLY,
                EntityID = fsScheduleRow.EntityID,
                EntityType = fsScheduleRow.EntityType,
                LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                ScheduleID = (int)fsScheduleRow.ScheduleID,
                SubScheduleID = 0,
                TimeOfDayBegin = new TimeSpan(5, 0, 0),
                TimeOfDayEnd = new TimeSpan(11, 0, 0),
                SchedulingRange = period ?? new Period((DateTime)fsScheduleRow.StartDate, fsScheduleRow.EndDate),
                Frequency = (int)fsScheduleRow.WeeklyFrequency,
                Descr = fsScheduleRow.ContractDescr,
                ApplySeason = applySeasonInSchedule,
                SeasonOnJan = fsScheduleRow.SeasonOnJan,
                SeasonOnFeb = fsScheduleRow.SeasonOnFeb,
                SeasonOnMar = fsScheduleRow.SeasonOnMar,
                SeasonOnApr = fsScheduleRow.SeasonOnApr,
                SeasonOnMay = fsScheduleRow.SeasonOnMay,
                SeasonOnJun = fsScheduleRow.SeasonOnJun,
                SeasonOnJul = fsScheduleRow.SeasonOnJul,
                SeasonOnAug = fsScheduleRow.SeasonOnAug,
                SeasonOnSep = fsScheduleRow.SeasonOnSep,
                SeasonOnOct = fsScheduleRow.SeasonOnOct,
                SeasonOnNov = fsScheduleRow.SeasonOnNov,
                SeasonOnDec = fsScheduleRow.SeasonOnDec
            };
            #region SetDaysToList
            if (fsScheduleRow.WeeklyOnFri == true)
            {
                days.Add(DayOfWeek.Friday);
            }

            if (fsScheduleRow.WeeklyOnMon == true)
            {
                days.Add(DayOfWeek.Monday);
            }

            if (fsScheduleRow.WeeklyOnSat == true)
            {
                days.Add(DayOfWeek.Saturday);
            }

            if (fsScheduleRow.WeeklyOnSun == true)
            {
                days.Add(DayOfWeek.Sunday);
            }

            if (fsScheduleRow.WeeklyOnThu == true)
            {
                days.Add(DayOfWeek.Thursday);
            }

            if (fsScheduleRow.WeeklyOnTue == true)
            {
                days.Add(DayOfWeek.Tuesday);
            }

            if (fsScheduleRow.WeeklyOnWed == true)
            {
                days.Add(DayOfWeek.Wednesday);
            }

            weeklySchedule.SetDays(days.ToArray());
            #endregion

            return weeklySchedule;
        }

        /// <summary>
        /// This function maps a FSSchedule Monthly frequency to a MonthlyScheduleSpecificDay or MonthlyScheduleWeekDay in the Scheduler module depending of the 
        /// <c>fsScheduleRow.MonthlyDaySel</c>. The [SubScheduleID] correspond to which of the four types of Monthly is specified in the [fsScheduleRow].
        /// </summary>
        public static Schedule mapMonthlyFrequency(PXCache cache, FSSchedule fsScheduleRow, int subScheduleID, DateTime? toDate, string recordType, string monthlyRecurrenceType, Period period = null)
        {
            bool applySeasonInSchedule = recordType != ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT ? SharedFunctions.GetEnableSeasonSetting(cache.Graph, fsScheduleRow) : false;

            Period schedulingRange = period ?? new Period((DateTime)fsScheduleRow.StartDate, fsScheduleRow.EndDate);

            if (monthlyRecurrenceType == ID.ScheduleMonthlySelection.DAILY)
            {
                var monthlyScheduleSpecificDay = new MonthlyScheduleSpecificDay
                {
                    Name = TX.FrecuencySchedule.MONTHSPECIFICDATE,
                    EntityID = fsScheduleRow.EntityID,
                    EntityType = fsScheduleRow.EntityType,
                    LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                    ScheduleID = (int)fsScheduleRow.ScheduleID,
                    SubScheduleID = subScheduleID,
                    TimeOfDayBegin = new TimeSpan(5, 0, 0), //TODO SD-5493
                    TimeOfDayEnd = new TimeSpan(11, 0, 0),
                    Frequency = (int)fsScheduleRow.MonthlyFrequency,
                    SchedulingRange = schedulingRange,
                    Descr = fsScheduleRow.ContractDescr,
                    ApplySeason = applySeasonInSchedule,
                    SeasonOnJan = fsScheduleRow.SeasonOnJan,
                    SeasonOnFeb = fsScheduleRow.SeasonOnFeb,
                    SeasonOnMar = fsScheduleRow.SeasonOnMar,
                    SeasonOnApr = fsScheduleRow.SeasonOnApr,
                    SeasonOnMay = fsScheduleRow.SeasonOnMay,
                    SeasonOnJun = fsScheduleRow.SeasonOnJun,
                    SeasonOnJul = fsScheduleRow.SeasonOnJul,
                    SeasonOnAug = fsScheduleRow.SeasonOnAug,
                    SeasonOnSep = fsScheduleRow.SeasonOnSep,
                    SeasonOnOct = fsScheduleRow.SeasonOnOct,
                    SeasonOnNov = fsScheduleRow.SeasonOnNov,
                    SeasonOnDec = fsScheduleRow.SeasonOnDec
                };
                switch (subScheduleID)
                {
                    case SubScheduleType.FIRST:
                        monthlyScheduleSpecificDay.DayOfMonth = (int)fsScheduleRow.MonthlyOnDay1;
                        break;
                    case SubScheduleType.SECOND:
                        monthlyScheduleSpecificDay.DayOfMonth = (int)fsScheduleRow.MonthlyOnDay2;
                        break;
                    case SubScheduleType.THIRD:
                        monthlyScheduleSpecificDay.DayOfMonth = (int)fsScheduleRow.MonthlyOnDay3;
                        break;
                    case SubScheduleType.FOURTH:
                        monthlyScheduleSpecificDay.DayOfMonth = (int)fsScheduleRow.MonthlyOnDay4;
                        break;
                }

                return monthlyScheduleSpecificDay;
            }
            else
            {
                var monthlyScheduleWeekDay = new MonthlyScheduleWeekDay
                {
                    Name = TX.FrecuencySchedule.MONTHWEEKDAY,
                    EntityID = fsScheduleRow.EntityID,
                    EntityType = fsScheduleRow.EntityType,
                    LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                    ScheduleID = (int)fsScheduleRow.ScheduleID,
                    SubScheduleID = subScheduleID,
                    TimeOfDayBegin = new TimeSpan(5, 0, 0), //TODO SD-5493
                    TimeOfDayEnd = new TimeSpan(11, 0, 0),
                    Frequency = (int)fsScheduleRow.MonthlyFrequency,
                    SchedulingRange = schedulingRange,
                    Descr = fsScheduleRow.ContractDescr,
                    ApplySeason = applySeasonInSchedule,
                    SeasonOnJan = fsScheduleRow.SeasonOnJan,
                    SeasonOnFeb = fsScheduleRow.SeasonOnFeb,
                    SeasonOnMar = fsScheduleRow.SeasonOnMar,
                    SeasonOnApr = fsScheduleRow.SeasonOnApr,
                    SeasonOnMay = fsScheduleRow.SeasonOnMay,
                    SeasonOnJun = fsScheduleRow.SeasonOnJun,
                    SeasonOnJul = fsScheduleRow.SeasonOnJul,
                    SeasonOnAug = fsScheduleRow.SeasonOnAug,
                    SeasonOnSep = fsScheduleRow.SeasonOnSep,
                    SeasonOnOct = fsScheduleRow.SeasonOnOct,
                    SeasonOnNov = fsScheduleRow.SeasonOnNov,
                    SeasonOnDec = fsScheduleRow.SeasonOnDec
                };
                switch (subScheduleID)
                {
                    case SubScheduleType.FIRST:
                        monthlyScheduleWeekDay.MonthlyOnWeek = (short)fsScheduleRow.MonthlyOnWeek1;
                        monthlyScheduleWeekDay.MonthlyOnDayOfWeek = SharedFunctions.getDayOfWeekByID((int)fsScheduleRow.MonthlyOnDayOfWeek1);
                        break;
                    case SubScheduleType.SECOND:
                        monthlyScheduleWeekDay.MonthlyOnWeek = (short)fsScheduleRow.MonthlyOnWeek2;
                        monthlyScheduleWeekDay.MonthlyOnDayOfWeek = SharedFunctions.getDayOfWeekByID((int)fsScheduleRow.MonthlyOnDayOfWeek2);
                        break;
                    case SubScheduleType.THIRD:
                        monthlyScheduleWeekDay.MonthlyOnWeek = (short)fsScheduleRow.MonthlyOnWeek3;
                        monthlyScheduleWeekDay.MonthlyOnDayOfWeek = SharedFunctions.getDayOfWeekByID((int)fsScheduleRow.MonthlyOnDayOfWeek3);
                        break;
                    case SubScheduleType.FOURTH:
                        monthlyScheduleWeekDay.MonthlyOnWeek = (short)fsScheduleRow.MonthlyOnWeek4;
                        monthlyScheduleWeekDay.MonthlyOnDayOfWeek = SharedFunctions.getDayOfWeekByID((int)fsScheduleRow.MonthlyOnDayOfWeek4);
                        break;
                }

                return monthlyScheduleWeekDay;
            }
        }

        public static Schedule mapAnnualFrequency(FSSchedule fsScheduleRow, DateTime? toDate, Period period = null)
        {
            Period schedulingRange = period ?? new Period((DateTime)fsScheduleRow.StartDate, fsScheduleRow.EndDate);

            if (fsScheduleRow.AnnualRecurrenceType == ID.ScheduleMonthlySelection.DAILY)
            {
                var annualScheduleSpecificDay = new AnnualScheduleSpecificDay
                {
                    Name = TX.FrecuencySchedule.MONTHSPECIFICDATE,
                    EntityID = fsScheduleRow.EntityID,
                    EntityType = fsScheduleRow.EntityType,
                    LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                    ScheduleID = (int)fsScheduleRow.ScheduleID,
                    TimeOfDayBegin = new TimeSpan(5, 0, 0), //TODO SD-5493
                    TimeOfDayEnd = new TimeSpan(11, 0, 0),
                    Frequency = (int)fsScheduleRow.AnnualFrequency,
                    SchedulingRange = schedulingRange,
                    Descr = fsScheduleRow.ContractDescr,
                    ApplySeason = false
                };

                annualScheduleSpecificDay.DayOfMonth = (int)fsScheduleRow.AnnualOnDay;
                annualScheduleSpecificDay.SetMonths(SetMonthsToList(fsScheduleRow).ToArray());

                return annualScheduleSpecificDay;
            } 
            else
            {
                var annualScheduleWeekDay = new AnnualScheduleWeekDay
                {
                    Name = TX.FrecuencySchedule.MONTHWEEKDAY, 
                    EntityID = fsScheduleRow.EntityID,
                    EntityType = fsScheduleRow.EntityType,
                    LastGeneratedTimeSlotDate = fsScheduleRow.LastGeneratedElementDate,
                    ScheduleID = (int)fsScheduleRow.ScheduleID,
                    TimeOfDayBegin = new TimeSpan(5, 0, 0), //TODO SD-5493
                    TimeOfDayEnd = new TimeSpan(11, 0, 0),
                    Frequency = (int)fsScheduleRow.AnnualFrequency,
                    SchedulingRange = schedulingRange,
                    Descr = fsScheduleRow.ContractDescr,
                    ApplySeason = false
                };

                annualScheduleWeekDay.MonthlyOnWeek = (short)fsScheduleRow.AnnualOnWeek;
                annualScheduleWeekDay.MonthlyOnDayOfWeek = SharedFunctions.getDayOfWeekByID((int)fsScheduleRow.AnnualOnDayOfWeek);
                annualScheduleWeekDay.SetMonths(SetMonthsToList(fsScheduleRow).ToArray());
                return annualScheduleWeekDay;
            }
        }

        /// <summary>
        /// Set a new list with the selected months of the Schedule.
        /// </summary>
        /// <param name="fsScheduleRow">Instance of FSSchedule DAC.</param>
        /// <returns>List with the selected months of the Schedule.</returns>
        private static List<SharedFunctions.MonthsOfYear> SetMonthsToList(FSSchedule fsScheduleRow) 
        {
            List<SharedFunctions.MonthsOfYear> months = new List<SharedFunctions.MonthsOfYear>();
            if (fsScheduleRow.AnnualOnJan == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.January);
            }

            if (fsScheduleRow.AnnualOnFeb == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.February);
            }

            if (fsScheduleRow.AnnualOnMar == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.March);
            }

            if (fsScheduleRow.AnnualOnApr == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.April);
            }

            if (fsScheduleRow.AnnualOnMay == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.May);
            }

            if (fsScheduleRow.AnnualOnJun == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.June);
            }

            if (fsScheduleRow.AnnualOnJul == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.July);
            }

            if (fsScheduleRow.AnnualOnAug == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.August);
            }

            if (fsScheduleRow.AnnualOnSep == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.September);
            }

            if (fsScheduleRow.AnnualOnOct == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.October);
            }

            if (fsScheduleRow.AnnualOnNov == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.November);
            }

            if (fsScheduleRow.AnnualOnDec == true)
            {
                months.Add(SharedFunctions.MonthsOfYear.December);
            }

            return months;
        }
    }
}
