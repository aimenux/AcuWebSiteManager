using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.EP;
using PX.Objects.FS.Scheduler;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PX.Objects.FS
{
    public static class SharedFunctions
    {
        public enum MonthsOfYear
        {
            January = 1, February, March, April, May, June, July, August, September, October, November, December
        }

        public enum SlotIsContained
        {
            NotContained = 1, Contained, PartiallyContained, ExceedsContainment
        }

        public enum SOAPDetOriginTab
        {
            Services, InventoryItems
        }

        #region Acumatica Related Functions
        /// <summary>
        /// Retrieves an InventoryItem row by its ID.
        /// </summary>
        public static InventoryItem GetInventoryItemRow(PXGraph graph, int? inventoryID)
        {
            if (inventoryID == null)
            {
                return null;
            }

            InventoryItem inventoryItemRow = PXSelect<InventoryItem,
                                             Where<
                                                 InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                                             .Select(graph, inventoryID);

            return inventoryItemRow;
        }
        #endregion
        #region Date and Time Functions

        public enum TimeFormat
        {
            Hours = 1, Minutes = 2, Seconds = 3
        }

        public struct DateRange
        {
            public DateTime startDate;
            public DateTime endDate;
        }

        public static DateRange GetDateRange(DateTime startDate, string period)
        {
            DateRange dateRange;

            DateTime date = startDate;
            DateTime dtBegin = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            DateTime dtEnd = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

            switch (period)
            {
                case ID.PeriodType.DAY:
                    break;
                case ID.PeriodType.WEEK:

                    int nowDayOfWeek = Convert.ToInt32(startDate.DayOfWeek);

                    dtBegin = dtBegin.AddDays((int)DayOfWeek.Sunday - nowDayOfWeek);
                    dtEnd = dtEnd.AddDays((int)DayOfWeek.Saturday - nowDayOfWeek);

                    break;
                case ID.PeriodType.MONTH:

                    dtBegin = new DateTime(date.Year, date.Month, 1, 0, 0, 0);
                    dtEnd = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month), 23, 59, 59);

                    break;
            }

            dateRange.startDate = dtBegin;
            dateRange.endDate = dtEnd;

            return dateRange;
        }
        #endregion
        #region ServiceOrder
        public static void UpdateBillingInfoInDocsLO(PXGraph callerGraph, int? currentCustomerID, int? currentBillingCycleID)
        {
            PXLongOperation.StartOperation(
                callerGraph,
                delegate
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        var graph = PXGraph.CreateInstance<BillingCycleMaint>();
                        if (callerGraph is SetupMaint
                                && ((SetupMaint)callerGraph).SetupRecord.Current.CustomerMultipleBillingOptions == true)
                        {
                            graph.KeepCustomerMultBillCyleSettings(callerGraph);
                        }
                        graph.UpdateBillingInfoInDocs(currentCustomerID, currentBillingCycleID);

                        ts.Complete();
                    }
                });
        }

        public static void WarnUserWithServiceOrdersWithoutBillingSettings(PXCache cache, CreateInvoiceFilter createInvoiceFilterRow, PXAction<CreateInvoiceFilter> fixServiceOrdersWithoutBillingInfo)
        {
            PXGraph graph = new PXGraph();

            WarnUserWithServiceOrdersWithoutBillingSettings(graph, cache, createInvoiceFilterRow, fixServiceOrdersWithoutBillingInfo);
        }

        public static void WarnUserWithServiceOrdersWithoutBillingSettings(PXGraph graph, PXCache cache, CreateInvoiceFilter createInvoiceFilterRow, PXAction<CreateInvoiceFilter> fixServiceOrdersWithoutBillingInfo)
        {
            if (graph == null)
            {
                graph = new PXGraph();
            }

            int serviceOrdersWithoutBillingInfo = ServiceOrdersWithoutBillingSettings(graph);

            if (serviceOrdersWithoutBillingInfo > 0)
            {
                int serviceOrdersWithoutBillingInfo_PossibleRepair = ServiceOrdersWithoutBillingSettings_PossibleFix(graph);

                fixServiceOrdersWithoutBillingInfo.SetVisible(serviceOrdersWithoutBillingInfo_PossibleRepair > 0);

                string warningMessage = TX.Warning.DOCUMENTS_WITHOUT_BILLING_INFO;
                warningMessage += serviceOrdersWithoutBillingInfo_PossibleRepair > 0 ? TX.Warning.USE_FIX_SERVICE_ORDERS_BUTTON : string.Empty;

                cache.RaiseExceptionHandling<CreateInvoiceFilter.postTo>(
                         createInvoiceFilterRow,
                         createInvoiceFilterRow.PostTo,
                         new PXSetPropertyException(warningMessage, PXErrorLevel.Warning));
            }
        }

        public static int ServiceOrdersWithoutBillingSettings_PossibleFix(PXGraph tempGraph)
        {
            return PXSelectReadonly2<FSServiceOrder,
                   CrossJoinSingleTable<FSSetup,
                   LeftJoin<FSCustomerBillingSetup,
                   On<
                       FSCustomerBillingSetup.customerID, Equal<FSServiceOrder.billCustomerID>,
                       And<
                           Where2<
                               Where<FSSetup.customerMultipleBillingOptions, Equal<False>,
                               And<FSCustomerBillingSetup.srvOrdType, IsNull,
                               And<FSCustomerBillingSetup.active, Equal<True>>>>,
                               Or<
                                   Where<FSSetup.customerMultipleBillingOptions, Equal<True>,
                                   And<FSCustomerBillingSetup.srvOrdType, Equal<FSServiceOrder.srvOrdType>,
                                   And<FSCustomerBillingSetup.active, Equal<True>>>>>>>>>>,
                   Where<
                       FSServiceOrder.postedBy, IsNull,
                       And<FSServiceOrder.customerID, IsNotNull,
                       And<
                           Where2<
                               Where2<
                                   Where<
                                       FSServiceOrder.cBID, IsNull,
                                       And<FSCustomerBillingSetup.cBID, IsNotNull>>,
                                   Or<
                                       Where<
                                           FSServiceOrder.cBID, IsNotNull,
                                           And<FSCustomerBillingSetup.cBID, IsNull>>>>,
                               Or<
                                   Where2<
                                       Where<
                                           FSServiceOrder.cBID, NotEqual<FSCustomerBillingSetup.cBID>>,
                                       Or<
                                           Where<
                                               FSCustomerBillingSetup.cBID, IsNotNull,
                                               And<FSServiceOrder.cutOffDate, IsNull>>>>>>>>>>
                   .SelectWindowed(tempGraph, 0, 1).Count;
        }

        public static int ServiceOrdersWithoutBillingSettings(PXGraph tempGraph)
        {
            return PXSelectReadonly2<FSServiceOrder,
                   InnerJoin<FSSrvOrdType, 
                   On<
                       FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>>,
                   Where<
                       FSServiceOrder.postedBy, IsNull,
                       And<FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.Quote>,
                       And<FSServiceOrder.customerID, IsNotNull,
                       And<
                           Where2<
                               Where<
                                   FSServiceOrder.cBID, IsNull>,
                               Or<
                                   FSServiceOrder.cutOffDate, IsNull>>>>>>>
                   .SelectWindowed(tempGraph, 0, 1).Count;
        }

        public static void PreUpdateBillingInfoDocs(PXGraph callerGraph, int? currentCustomerID, int? currentBillingCycleID)
        {
            if (currentBillingCycleID != null)
            {
                PXUpdateJoin<
                    Set<FSServiceOrder.cBID, Null>,
                FSServiceOrder,
                InnerJoin<FSCustomerBillingSetup,
                On<
                    FSCustomerBillingSetup.cBID, Equal<FSServiceOrder.cBID>>>,
                Where<
                    FSCustomerBillingSetup.billingCycleID, Equal<Required<FSCustomerBillingSetup.billingCycleID>>,
                    And<FSServiceOrder.postedBy, IsNull>>>
                .Update(callerGraph, currentBillingCycleID);
            }
            else if (currentCustomerID != null)
            {
                PXUpdate<
                    Set<FSServiceOrder.cBID, Null>,
                FSServiceOrder,
                Where<
                    FSServiceOrder.customerID, Equal<Required<FSServiceOrder.customerID>>,
                    And<FSServiceOrder.postedBy, IsNull>>>
                .Update(callerGraph, currentCustomerID);
            }
            else
            {
                PXUpdate<
                        Set<FSServiceOrder.cBID, Null>,
                FSServiceOrder,
                Where<FSServiceOrder.postedBy, IsNull>>
                .Update(callerGraph);
            }
        }

        #endregion
        #region Appointments

        /// <summary>
        /// Evaluates whether the Employee's slot can contain the Appointment's duration.
        /// </summary>
        /// <param name="slotBegin">DateTime of Start of the Employee Schedule.</param>
        /// <param name="slotEnd">DateTime of End of the Employee Schedule.</param>
        /// <param name="beginTime">Begin DateTime of the possible overlap Slot.</param>
        /// <param name="endTime">End DateTime of the possible overlap Slot.</param>
        /// <returns><c>Enum</c> indicating if the appointment is contained, partially contained or not contained in the Employee's work slot.</returns>
        public static SlotIsContained SlotIsContainedInSlot(DateTime? slotBegin, DateTime? slotEnd, DateTime? beginTime, DateTime? endTime)
        {
            if (beginTime <= slotBegin && endTime >= slotEnd)
            {
                return SlotIsContained.ExceedsContainment;
            }

            if (beginTime >= slotBegin && endTime <= slotEnd)
            {
                return SlotIsContained.Contained;
            }

            if ((beginTime < slotBegin && endTime > slotBegin) || (beginTime < slotEnd && endTime > slotEnd))
            {
                return SlotIsContained.PartiallyContained;
            }

            return SlotIsContained.NotContained;
        }

        public static void GetBillableFieldValues(PXGraph graph,
                                                  IFSSODetBase fsSODetBaseRow,
                                                  int? duration,
                                                  out decimal? billableQty,
                                                  out int? billableTime)
        {
            billableQty = 0;
            billableTime = 0;

            if (fsSODetBaseRow.IsBillable == false)
            {
                return;
            }

            if (fsSODetBaseRow.LineType == ID.LineType_ServiceTemplate.SERVICE)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, fsSODetBaseRow.InventoryID);

                if (inventoryItemRow != null)
                {
                    string billingRule = fsSODetBaseRow.BillingRule;

                    if (billingRule == null)
                    {
                        FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);
                        billingRule = fsxServiceRow.BillingRule;
                    }

                    if (billingRule == ID.BillingRule.TIME)
                    {
                        billableTime = duration;
                    }
                }
            }

            billableQty = fsSODetBaseRow.GetQty(FieldType.BillableField);
        }

        /// <summary>
        /// Completes all appointments belonging to appointmentList, in case an error occurs with any appointment,
        /// the method will return a Dictionary listing each appointment with its error.
        /// </summary>
        public static Dictionary<FSAppointment, string> CompleteAppointments(PXGraph graph, PXResultset<FSAppointmentInRoute> bqlResultSet)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();

            Dictionary<FSAppointment, string> appointmentsWithErrors = new Dictionary<FSAppointment, string>();

            foreach (FSAppointment fsAppointmentRow in bqlResultSet)
            {
                if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED
                        || fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                            || fsAppointmentRow.Status == ID.Status_Appointment.CANCELED)
                {
                    continue;
                }

                appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                DateTime? scheduleDateTimeBegin = appointmentEntryGraph.AppointmentRecords.Current.ScheduledDateTimeBegin;
                DateTime? scheduleDateTimeEnd = appointmentEntryGraph.AppointmentRecords.Current.ScheduledDateTimeEnd;

                DateTime? actualDateTimeBegin = appointmentEntryGraph.AppointmentRecords.Current.ActualDateTimeBegin;
                DateTime? actualDateTimeEnd = appointmentEntryGraph.AppointmentRecords.Current.ActualDateTimeEnd;

                if (actualDateTimeBegin.HasValue == false)
                {
                    appointmentEntryGraph.AppointmentRecords.Current.ActualDateTimeBegin = scheduleDateTimeBegin;
                }

                if (actualDateTimeEnd.HasValue == false)
                {
                    appointmentEntryGraph.AppointmentRecords.Current.ActualDateTimeEnd = scheduleDateTimeEnd;
                }

                try
                {
                    appointmentEntryGraph.completeAppointment.Press();
                }
                catch (PXException e)
                {
                    appointmentsWithErrors.Add(fsAppointmentRow, e.Message);
                }
            }

            return appointmentsWithErrors;
        }

        public static Dictionary<FSAppointment, string> CancelAppointments(PXGraph graph, List<PXResult<FSAppointmentInRoute>> appointments)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
            appointmentEntryGraph.SkipCallSOAction = true;

            Dictionary<FSAppointment, string> appointmentsWithErrors = new Dictionary<FSAppointment, string>();

            foreach (FSAppointment fsAppointmentRow in appointments)
            {
                appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                try
                {
                    if (appointmentEntryGraph.AppointmentRecords.Current.Status == ID.Status_Appointment.IN_PROCESS)
                    {
                        appointmentEntryGraph.reopenAppointment.Press();
                    }

                    appointmentEntryGraph.cancelAppointment.Press();
                }
                catch (PXException e)
                {
                    appointmentsWithErrors.Add(fsAppointmentRow, e.Message);
                }
            }

            return appointmentsWithErrors;
        }

        /// <summary>
        /// Closes all appointments belonging to appointmentList, in case an error occurs with any appointment,
        /// the method will return a Dictionary listing each appointment with its error.
        /// </summary>
        public static Dictionary<FSAppointment, string> CloseAppointments(PXResultset<FSAppointment> bqlResultSet)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
            Dictionary<FSAppointment, string> appointmentsWithErrors = new Dictionary<FSAppointment, string>();

            foreach (FSAppointment fsAppointmentRow in bqlResultSet)
            {
                if (fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                        || fsAppointmentRow.Status == ID.Status_Appointment.CANCELED)
                {
                    continue;
                }

                appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                            (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                appointmentEntryGraph.SkipCallSOAction = true;

                try
                {
                    appointmentEntryGraph.closeAppointment.Press();
                }
                catch (PXException e)
                {
                    appointmentsWithErrors.Add(fsAppointmentRow, e.Message);
                }
            }

            return appointmentsWithErrors;
        }

        /// <summary>
        /// Cancel all appointments belonging to appointmentList, in case an error occurs with any appointment,
        /// the method will return a Dictionary listing each appointment with its error.
        /// </summary>
        public static Dictionary<FSAppointment, string> CancelAppointments(PXGraph graph, PXResultset<FSAppointment> bqlResultSet)
        {
            AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
            Dictionary<FSAppointment, string> appointmentsWithErrors = new Dictionary<FSAppointment, string>();

            foreach (FSAppointment fsAppointmentRow in bqlResultSet)
            {
                try
                {
                    if (fsAppointmentRow.Status != ID.Status_Appointment.MANUAL_SCHEDULED
                            && fsAppointmentRow.Status != ID.Status_Appointment.AUTOMATIC_SCHEDULED)
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(
                                        TX.Error.SERVICE_ORDER_CANT_BE_CANCELED_APPOINTMENTS_HAVE_INVALID_STATUS,
                                        TX.Status_Appointment.AUTOMATIC_SCHEDULED,
                                        TX.Status_Appointment.MANUAL_SCHEDULED,
                                        TX.Status_Appointment.CANCELED));
                    }
                    else
                    {
                        appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                                (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);
                        appointmentEntryGraph.SkipCallSOAction = true;
                        appointmentEntryGraph.cancelAppointment.Press();
                    }
                }
                catch (PXException e)
                {
                    appointmentsWithErrors.Add(fsAppointmentRow, e.Message);
                }
            }

            return appointmentsWithErrors;
        }

        #endregion

        #region WeekCodes
        /// <summary>
        /// Split a string by commas and returns the result as a list of strings.
        /// </summary>
        public static List<string> SplitWeekcodeByComma(string weekCodes)
        {
            List<string> returnListWeekCodes = new List<string>();
            var weekCodesArray = weekCodes.Split(',');
            foreach (string weekcode in weekCodesArray)
            {
                returnListWeekCodes.Add(weekcode.Trim());
            }

            return returnListWeekCodes;
        }

        /// <summary>
        /// Split a string in chars and returns the result as a list of strings.
        /// </summary>
        public static List<string> SplitWeekcodeInChars(string weekcode)
        {
            List<string> returnListWeekCodeByLetters = new List<string>();
            for (int i = 0; i < weekcode.Length; i++)
            {
                returnListWeekCodeByLetters.Add(weekcode.Substring(i, 1));
            }

            return returnListWeekCodeByLetters;
        }

        /// <summary>
        /// Validates if a Week Code is less than or equal to 4.
        /// </summary>
        public static bool IsAValidWeekCodeLength(string weekcode)
        {
            if (weekcode.Length <= 4)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates if a specific Char is valid for a Week Code (1-4), (A-B), (C-F), (S-Z).
        /// </summary>
        public static bool IsAValidCharForWeekCode(string charToCompare)
        {
            Regex rgx = new Regex(@"^[1-4]?[a-bA-B]?[c-fC-F]?[s-zS-Z]?$");

            return rgx.IsMatch(charToCompare);
        }

        /// <summary>
        /// Validates if a Week Code is valid for a schedule time and list of Week Code(s) given.
        /// </summary>
        public static bool WeekCodeIsValid(string sourceWeekcodes, DateTime? scheduleTime, PXGraph graph)
        {
            PXResultset<FSWeekCodeDate> bqlResultSet = new PXResultset<FSWeekCodeDate>();
            List<object> weekCodeArgs = new List<object>();
            PXSelectBase<FSWeekCodeDate> commandFilter = new PXSelect<FSWeekCodeDate,
                                                             Where<
                                                                   FSWeekCodeDate.weekCodeDate, Equal<Required<FSWeekCodeDate.weekCodeDate>>>>
                                                             (graph);

            weekCodeArgs.Add(scheduleTime);

            Regex rgxP1 = new Regex(@"^[1-4]$");
            Regex rgxP2 = new Regex(@"^[a-bA-B]$");
            Regex rgxP3 = new Regex(@"^[c-fC-F]$");
            Regex rgxP4 = new Regex(@"^[s-zS-Z]$");

            List<string> weekcodes = SharedFunctions.SplitWeekcodeByComma(sourceWeekcodes);

            foreach (string weekcode in weekcodes)
            {
                List<string> charsInWeekCode = SharedFunctions.SplitWeekcodeInChars(weekcode);
                string p1, p2, p3, p4;
                p1 = p2 = p3 = p4 = "%";

                foreach (string letter in charsInWeekCode)
                {
                    string letterAux = letter.ToUpper();

                    if (rgxP1.IsMatch(letterAux))
                    {
                        p1 = letterAux;
                    }
                    else if (rgxP2.IsMatch(letterAux))
                    {
                        p2 = letterAux;
                    }
                    else if (rgxP3.IsMatch(letterAux))
                    {
                        p3 = letterAux;
                    }
                    else if (rgxP4.IsMatch(letterAux))
                    {
                        p4 = letterAux;
                    }
                }

                commandFilter.WhereOr<
                            Where2<
                                Where<
                                    FSWeekCodeDate.weekCodeP1, Like<Required<FSWeekCodeDate.weekCodeP1>>,
                                    Or<FSWeekCodeDate.weekCodeP1, Like<Required<FSWeekCodeDate.weekCodeP1>>,
                                    Or<FSWeekCodeDate.weekCodeP1, IsNull>>>,
                                And2<
                                    Where<
                                        FSWeekCodeDate.weekCodeP2, Like<Required<FSWeekCodeDate.weekCodeP2>>,
                                        Or<FSWeekCodeDate.weekCodeP2, Like<Required<FSWeekCodeDate.weekCodeP2>>,
                                        Or<FSWeekCodeDate.weekCodeP2, IsNull>>>,
                                And2<
                                    Where<
                                        FSWeekCodeDate.weekCodeP3, Like<Required<FSWeekCodeDate.weekCodeP3>>,
                                        Or<FSWeekCodeDate.weekCodeP3, Like<Required<FSWeekCodeDate.weekCodeP3>>,
                                        Or<FSWeekCodeDate.weekCodeP3, IsNull>>>,
                                And<
                                    Where<
                                        FSWeekCodeDate.weekCodeP4, Like<Required<FSWeekCodeDate.weekCodeP4>>,
                                        Or<FSWeekCodeDate.weekCodeP4, Like<Required<FSWeekCodeDate.weekCodeP4>>,
                                        Or<FSWeekCodeDate.weekCodeP4, IsNull>>>>>>>
                                >();

                weekCodeArgs.Add(p1);
                weekCodeArgs.Add(p1.ToLower());
                weekCodeArgs.Add(p2);
                weekCodeArgs.Add(p2.ToLower());
                weekCodeArgs.Add(p3);
                weekCodeArgs.Add(p3.ToLower());
                weekCodeArgs.Add(p4);
                weekCodeArgs.Add(p4.ToLower());
            }

            bqlResultSet = commandFilter.Select(weekCodeArgs.ToArray());

            return bqlResultSet.Count > 0;
        }

        #endregion

        /// <summary>
        /// Returns the day of the week depending on the ID [dayID]. Sunday is (0) and Monday (6).
        /// </summary>
        public static DayOfWeek getDayOfWeekByID(int dayID)
        {
            switch (dayID)
            {
                case 0:
                    return DayOfWeek.Sunday;
                case 1:
                    return DayOfWeek.Monday;
                case 2:
                    return DayOfWeek.Tuesday;
                case 3:
                    return DayOfWeek.Wednesday;
                case 4:
                    return DayOfWeek.Thursday;
                case 5:
                    return DayOfWeek.Friday;
                case 6:
                    return DayOfWeek.Saturday;
                default:
                    return DayOfWeek.Monday;
            }
        }

        /// <summary>
        /// Returns the month of the year depending on the ID [dayID]. January is (1) and December (12).
        /// </summary>
        public static MonthsOfYear getMonthOfYearByID(int monthID)
        {
            switch (monthID)
            {
                case 1:
                    return MonthsOfYear.January;
                case 2:
                    return MonthsOfYear.February;
                case 3:
                    return MonthsOfYear.March;
                case 4:
                    return MonthsOfYear.April;
                case 5:
                    return MonthsOfYear.May;
                case 6:
                    return MonthsOfYear.June;
                case 7:
                    return MonthsOfYear.July;
                case 8:
                    return MonthsOfYear.August;
                case 9:
                    return MonthsOfYear.September;
                case 10:
                    return MonthsOfYear.October;
                case 11:
                    return MonthsOfYear.November;
                default:
                    return MonthsOfYear.December;
            }
        }

        /// <summary>
        /// Returns the month in string of the year depending on the ID [dayID]. January is (JAN) and December (DEC).
        /// </summary>
        public static string GetMonthOfYearInStringByID(int monthID)
        {
            switch (monthID)
            {
                case 1:
                    return TX.ShortMonths.JANUARY;
                case 2:
                    return TX.ShortMonths.FEBRUARY;
                case 3:
                    return TX.ShortMonths.MARCH;
                case 4:
                    return TX.ShortMonths.APRIL;
                case 5:
                    return TX.ShortMonths.MAY;
                case 6:
                    return TX.ShortMonths.JUNE;
                case 7:
                    return TX.ShortMonths.JULY;
                case 8:
                    return TX.ShortMonths.AUGUST;
                case 9:
                    return TX.ShortMonths.SEPTEMBER;
                case 10:
                    return TX.ShortMonths.OCTOBER;
                case 11:
                    return TX.ShortMonths.NOVEMBER;
                default:
                    return TX.ShortMonths.DECEMBER;
            }
        }

        /// <summary>
        /// Calculates the beginning of the week for the specific <c>date</c> using the <c>startOfWeek</c> as reference.
        /// </summary>
        public static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            int diff = date.DayOfWeek - startOfWeek;

            if (diff < 0)
            {
                diff += 7;
            }

            return date.AddDays(-1 * diff).Date;
        }

        /// <summary>
        /// Calculates the end of the week for the specific <c>date</c> using the <c>startOfWeek</c> as reference.
        /// </summary>
        public static DateTime EndOfWeek(DateTime date, DayOfWeek startOfWeek)
        {
            DateTime start = StartOfWeek(date, startOfWeek);

            return start.AddDays(6);
        }

        /// <summary>
        /// Verifies that the EndDate is greater than the StartDate.
        /// </summary>
        public static bool IsValidDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate == null
                        || endDate == null)
            {
                return false;
            }

            if (startDate > endDate)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the ordinal number according to the day specified.
        /// </summary>
        private static string SetOrdinalNumberString(short? ordinal)
        {
            switch (ordinal)
            {
                case 1: return Localize(TX.RecurrenceDescription.ST);
                case 2: return Localize(TX.RecurrenceDescription.ND);
                case 3: return Localize(TX.RecurrenceDescription.RD);
                default: return Localize(TX.RecurrenceDescription.TH);
            }
        }

        public static string Localize(string word)
        {
            return PXMessages.LocalizeFormatNoPrefix(word);
        }

        /// <summary>
        /// Sets the recurrence description in [fsScheduleRow.RecurrenceDescription] depending on the recurrence selected by the user.
        /// </summary>
        public static void SetRecurrenceDescription(FSSchedule fsScheduleRow)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Localize(TX.RecurrenceDescription.OCCURS_EVERY) + " ");

            switch (fsScheduleRow.FrequencyType)
            {
                case ID.Schedule_FrequencyType.DAILY:

                    builder.Append(fsScheduleRow.DailyFrequency + " " +
                        Localize(TX.RecurrenceDescription.DAYS) + ".");

                    break;
                case ID.Schedule_FrequencyType.WEEKLY:

                    builder.Append(
                        fsScheduleRow.WeeklyFrequency + " " +
                        Localize(TX.RecurrenceDescription.WEEKS) + " " +
                        Localize(TX.RecurrenceDescription.ON) + " ");

                    if (fsScheduleRow.WeeklyOnSun == true) { builder.Append(Localize(TX.WeekDays.SUNDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnMon == true) { builder.Append(Localize(TX.WeekDays.MONDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnTue == true) { builder.Append(Localize(TX.WeekDays.TUESDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnWed == true) { builder.Append(Localize(TX.WeekDays.WEDNESDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnThu == true) { builder.Append(Localize(TX.WeekDays.THURSDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnFri == true) { builder.Append(Localize(TX.WeekDays.FRIDAY) + ", "); }
                    if (fsScheduleRow.WeeklyOnSat == true) { builder.Append(Localize(TX.WeekDays.SATURDAY) + ", "); }
                    builder.Replace(",", ".", builder.Length - 2, 1);
                    break;
                case ID.Schedule_FrequencyType.MONTHLY:

                    builder.Append(
                        fsScheduleRow.MonthlyFrequency + " " +
                        Localize(TX.RecurrenceDescription.MONTHS) + " " +
                        Localize(TX.RecurrenceDescription.ON) + " " +
                        Localize(TX.RecurrenceDescription.THE) + " ");

                    if (fsScheduleRow.MonthlyRecurrenceType1.Equals(ID.ScheduleMonthlySelection.DAILY))
                    {
                        builder.Append(fsScheduleRow.MonthlyOnDay1);
                        builder.Append(SetOrdinalNumberString(fsScheduleRow.MonthlyOnDay1));

                        builder.Append(" " + Localize(TX.RecurrenceDescription.DAY));
                    }
                    else if (fsScheduleRow.MonthlyRecurrenceType1.Equals(ID.ScheduleMonthlySelection.WEEKLY))
                    {
                        builder.Append(Localize(TX.RecurrencyFrecuency.counters[(int)fsScheduleRow.MonthlyOnWeek1 - 1]) + " " +
                            Localize(TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek1]));
                    }

                    if (fsScheduleRow.Monthly2Selected == true)
                    {
                        builder.Append(" " + Localize(TX.RecurrenceDescription.AND) + " ");

                        if (fsScheduleRow.MonthlyRecurrenceType2.Equals(ID.ScheduleMonthlySelection.DAILY))
                        {
                            builder.Append(Localize(TX.RecurrenceDescription.ON) + " " +
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                fsScheduleRow.MonthlyOnDay2);

                            builder.Append(SetOrdinalNumberString(fsScheduleRow.MonthlyOnDay2));

                            builder.Append(" " + Localize(TX.RecurrenceDescription.DAY));
                        }
                        else if (fsScheduleRow.MonthlyRecurrenceType2.Equals(ID.ScheduleMonthlySelection.WEEKLY))
                        {
                            builder.Append(
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                Localize(TX.RecurrencyFrecuency.counters[(int)fsScheduleRow.MonthlyOnWeek2 - 1]) + " " +
                                Localize(TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek2]));
                        }
                    }

                    if (fsScheduleRow.Monthly3Selected == true)
                    {
                        builder.Append(" " + Localize(TX.RecurrenceDescription.AND) + " ");

                        if (fsScheduleRow.MonthlyRecurrenceType3.Equals(ID.ScheduleMonthlySelection.DAILY))
                        {
                            builder.Append(
                                Localize(TX.RecurrenceDescription.ON) + " " +
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                fsScheduleRow.MonthlyOnDay3);

                            builder.Append(SetOrdinalNumberString(fsScheduleRow.MonthlyOnDay3));

                            builder.Append(" " + Localize(TX.RecurrenceDescription.DAY));
                        }
                        else if (fsScheduleRow.MonthlyRecurrenceType3.Equals(ID.ScheduleMonthlySelection.WEEKLY))
                        {
                            builder.Append(
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                Localize(TX.RecurrencyFrecuency.counters[(int)fsScheduleRow.MonthlyOnWeek3 - 1]) + " " +
                                Localize(TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek3]));
                        }
                    }

                    if (fsScheduleRow.Monthly4Selected == true)
                    {
                        builder.Append(" " + Localize(TX.RecurrenceDescription.AND) + " ");

                        if (fsScheduleRow.MonthlyRecurrenceType4.Equals(ID.ScheduleMonthlySelection.DAILY))
                        {
                            builder.Append(
                                Localize(TX.RecurrenceDescription.ON) + " " +
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                fsScheduleRow.MonthlyOnDay4);
                            builder.Append(SetOrdinalNumberString(fsScheduleRow.MonthlyOnDay4));

                            builder.Append(" " + Localize(TX.RecurrenceDescription.DAY));
                        }
                        else if (fsScheduleRow.MonthlyRecurrenceType4.Equals(ID.ScheduleMonthlySelection.WEEKLY))
                        {
                            builder.Append(
                                Localize(TX.RecurrenceDescription.THE) + " " +
                                Localize(TX.RecurrencyFrecuency.counters[(int)fsScheduleRow.MonthlyOnWeek4 - 1]) + " " +
                                Localize(TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.MonthlyOnDayOfWeek4]));
                        }
                    }

                    builder.Append(
                        " " + Localize(TX.RecurrenceDescription.OF) + " " +
                        Localize(TX.RecurrenceDescription.THAT) + " " +
                        Localize(TX.RecurrenceDescription.MONTH) + ".");

                    break;
                case ID.Schedule_FrequencyType.ANNUAL:

                    builder.Append(
                        fsScheduleRow.AnnualFrequency + " " +
                        Localize(TX.RecurrenceDescription.YEARS) + " " +
                        Localize(TX.RecurrenceDescription.ON) + " ");

                    if (fsScheduleRow.AnnualRecurrenceType == ID.ScheduleMonthlySelection.WEEKLY)
                    {
                        builder.Append(
                            " " + Localize(TX.RecurrenceDescription.THE) + " " +
                            Localize(TX.RecurrencyFrecuency.counters[(int)fsScheduleRow.AnnualOnWeek - 1]) + " " +
                            Localize(TX.RecurrencyFrecuency.daysOfWeek[(int)fsScheduleRow.AnnualOnDayOfWeek]) + " " +
                            Localize(TX.RecurrenceDescription.OF) + " ");

                        if (fsScheduleRow.AnnualOnJan == true) { builder.Append(Localize(TX.Months.JANUARY) + ", "); }
                        if (fsScheduleRow.AnnualOnFeb == true) { builder.Append(Localize(TX.Months.FEBRUARY) + ", "); }
                        if (fsScheduleRow.AnnualOnMar == true) { builder.Append(Localize(TX.Months.MARCH) + ", "); }
                        if (fsScheduleRow.AnnualOnApr == true) { builder.Append(Localize(TX.Months.APRIL) + ", "); }
                        if (fsScheduleRow.AnnualOnMay == true) { builder.Append(Localize(TX.Months.MAY) + ", "); }
                        if (fsScheduleRow.AnnualOnJun == true) { builder.Append(Localize(TX.Months.JUNE) + ", "); }
                        if (fsScheduleRow.AnnualOnJul == true) { builder.Append(Localize(TX.Months.JULY) + ", "); }
                        if (fsScheduleRow.AnnualOnAug == true) { builder.Append(Localize(TX.Months.AUGUST) + ", "); }
                        if (fsScheduleRow.AnnualOnSep == true) { builder.Append(Localize(TX.Months.SEPTEMBER) + ", "); }
                        if (fsScheduleRow.AnnualOnOct == true) { builder.Append(Localize(TX.Months.OCTOBER) + ", "); }
                        if (fsScheduleRow.AnnualOnNov == true) { builder.Append(Localize(TX.Months.NOVEMBER) + ", "); }
                        if (fsScheduleRow.AnnualOnDec == true) { builder.Append(Localize(TX.Months.DECEMBER) + ", "); }
                        builder.Replace(",", ".", builder.Length - 1, 1);
                    }
                    else if (fsScheduleRow.AnnualRecurrenceType == ID.ScheduleMonthlySelection.DAILY)
                    {
                        if (fsScheduleRow.AnnualOnJan == true) { builder.Append(Localize(TX.Months.JANUARY) + ", "); }
                        if (fsScheduleRow.AnnualOnFeb == true) { builder.Append(Localize(TX.Months.FEBRUARY) + ", "); }
                        if (fsScheduleRow.AnnualOnMar == true) { builder.Append(Localize(TX.Months.MARCH) + ", "); }
                        if (fsScheduleRow.AnnualOnApr == true) { builder.Append(Localize(TX.Months.APRIL) + ", "); }
                        if (fsScheduleRow.AnnualOnMay == true) { builder.Append(Localize(TX.Months.MAY) + ", "); }
                        if (fsScheduleRow.AnnualOnJun == true) { builder.Append(Localize(TX.Months.JUNE) + ", "); }
                        if (fsScheduleRow.AnnualOnJul == true) { builder.Append(Localize(TX.Months.JULY) + ", "); }
                        if (fsScheduleRow.AnnualOnAug == true) { builder.Append(Localize(TX.Months.AUGUST) + ", "); }
                        if (fsScheduleRow.AnnualOnSep == true) { builder.Append(Localize(TX.Months.SEPTEMBER) + ", "); }
                        if (fsScheduleRow.AnnualOnOct == true) { builder.Append(Localize(TX.Months.OCTOBER) + ", "); }
                        if (fsScheduleRow.AnnualOnNov == true) { builder.Append(Localize(TX.Months.NOVEMBER) + ", "); }
                        if (fsScheduleRow.AnnualOnDec == true) { builder.Append(Localize(TX.Months.DECEMBER) + ", "); }
                        builder.Replace(",", " ", builder.Length - 1, 1);
                        builder.Append(fsScheduleRow.AnnualOnDay);
                        builder.Append(SetOrdinalNumberString(fsScheduleRow.AnnualOnDay) + ".");
                    }

                    builder.Replace(",", ".", builder.Length - 2, 1);
                    break;
                default:
                    break;
            }

            fsScheduleRow.RecurrenceDescription = builder.ToString();
        }

        /// <summary>
        /// Validates if the appointment scheduled day of the week belongs to the defined executions days of the given route.
        /// Also if it is valid sets the begin time of the route for the given week day.
        /// </summary>
        /// <param name="fsRouteRow">FSRoute row.</param>
        /// <param name="appointmentScheduledDayOfWeek">Monday Sunday.</param>
        /// <param name="beginTimeOnWeekDay">Begin time of the route in a specific day of week.</param>
        /// <returns>true if the route runs in the given week day, otherwise returns false.</returns>
        public static bool EvaluateExecutionDay(FSRoute fsRouteRow, DayOfWeek appointmentScheduledDayOfWeek, ref DateTime? beginTimeOnWeekDay)
        {
            if (fsRouteRow == null)
            {
                return false;
            }

            switch (appointmentScheduledDayOfWeek)
            {
                case DayOfWeek.Monday:
                    if ((bool)fsRouteRow.ActiveOnMonday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnMonday;
                        return true;
                    }

                    break;
                case DayOfWeek.Tuesday:
                    if ((bool)fsRouteRow.ActiveOnTuesday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnTuesday;
                        return true;
                    }

                    break;
                case DayOfWeek.Wednesday:
                    if ((bool)fsRouteRow.ActiveOnWednesday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnWednesday;
                        return true;
                    }

                    break;
                case DayOfWeek.Thursday:
                    if ((bool)fsRouteRow.ActiveOnThursday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnThursday;
                        return true;
                    }

                    break;
                case DayOfWeek.Friday:
                    if ((bool)fsRouteRow.ActiveOnFriday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnFriday;
                        return true;
                    }

                    break;
                case DayOfWeek.Saturday:
                    if ((bool)fsRouteRow.ActiveOnSaturday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnSaturday;
                        return true;
                    }

                    break;
                case DayOfWeek.Sunday:
                    if ((bool)fsRouteRow.ActiveOnSunday)
                    {
                        beginTimeOnWeekDay = fsRouteRow.BeginTimeOnSunday;
                        return true;
                    }

                    break;
            }

            return false;
        }

        /// <summary>
        /// Throw the Exception depending on the result of the EvaluateExecutionDay function.
        /// </summary>
        /// <param name="fsRouteRow">FSRoute row.</param>
        /// <param name="appointmentScheduledDayOfWeek">Monday Sunday.</param>
        /// <param name="beginTimeOnWeekDay">Begin time of the route in a specific day of week.</param>
        public static void ValidateExecutionDay(FSRoute fsRouteRow, DayOfWeek appointmentScheduledDayOfWeek, ref DateTime? beginTimeOnWeekDay)
        {
            if (!SharedFunctions.EvaluateExecutionDay(fsRouteRow, appointmentScheduledDayOfWeek, ref beginTimeOnWeekDay))
            {
                throw new PXException(TX.Error.INVALID_ROUTE_EXECUTION_DAYS_FOR_APPOINTMENT, fsRouteRow.RouteCD);
            }
        }

        /// <summary>
        /// Sets the given ScreenID to a format separated by dots
        /// SetScreenIDToDotFormat("SD300200") will return  "SD.300.200".
        /// </summary>
        /// <param name="screenID">8 characters ScreenID.</param>
        /// <returns>The given ScreenID in a dot separated format.</returns>
        public static string SetScreenIDToDotFormat(string screenID)
        {
            if (screenID.Length < 8 || screenID.Length > 8)
            {
                throw new PXException(TX.Error.SCREENID_INCORRECT_FORMAT);
            }

            return screenID.Substring(0, 2) + "." + screenID.Substring(2, 2) + "." + screenID.Substring(4, 2) + "." + screenID.Substring(6);
        }

        /// <summary>
        /// Move appointment from original position to new position and recalculate route statistics.
        /// </summary>
        public static void MoveAppointmentInRoute(PXGraph graph, FSRouteDocument fsRouteDocumentRow, int? positionFrom, int? positionTo)
        {
            FSAppointment fsAppointmentRow_From = PXSelectReadonly<FSAppointment,
                                                  Where<
                                                      FSAppointment.routePosition, Equal<Required<FSAppointment.routePosition>>,
                                                      And<FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>>>
                                                  .Select(graph, positionFrom, fsRouteDocumentRow.RouteDocumentID);

            FSAppointment fsAppoitmentRow_To = PXSelectReadonly<FSAppointment,
                                               Where<
                                                   FSAppointment.routePosition, Equal<Required<FSAppointment.routePosition>>,
                                                   And<FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>>>
                                               .Select(graph, positionTo, fsRouteDocumentRow.RouteDocumentID);

            if (fsAppointmentRow_From != null && fsAppoitmentRow_To != null)
            {
                PXCache cache = new PXCache<FSAppointment>(graph);

                fsAppointmentRow_From.RoutePosition = positionTo;
                cache.Update(fsAppointmentRow_From);

                fsAppoitmentRow_To.RoutePosition = positionFrom;
                cache.Update(fsAppoitmentRow_To);

                cache.Persist(PXDBOperation.Update);
            }
        }

        /// <summary>
        /// Get an appointment complete address from its service order.
        /// </summary>
        /// <returns>Returns a string containing the complete address of the appointment.</returns>
        public static string GetAppointmentAddress(FSAddress fsAddressRow)
        {
            if (fsAddressRow == null)
            {
                return string.Empty;
            }

            return SharedFunctions.GetAddressForGeolocation(fsAddressRow.PostalCode,
                                                            fsAddressRow.AddressLine1,
                                                            fsAddressRow.AddressLine2,
                                                            fsAddressRow.City,
                                                            fsAddressRow.State,
                                                            fsAddressRow.CountryID);
        }

        /// <summary>
        /// Get a complete address from a branch location.
        /// </summary>
        /// <returns>Returns a string containing the complete address of the branch location.</returns>
        public static string GetBranchLocationAddress(PXGraph graph, FSBranchLocation fsBranchLocationRow)
        {
            if (fsBranchLocationRow == null)
            {
                return string.Empty;
            }

            FSAddress fsAddressRow = PXSelect<FSAddress, 
                                     Where<
                                         FSAddress.addressID, Equal<Required<FSAddress.addressID>>>>
                                     .Select(graph, fsBranchLocationRow.BranchLocationAddressID);

            return SharedFunctions.GetAddressForGeolocation(fsAddressRow.PostalCode,
                                                            fsAddressRow.AddressLine1,
                                                            fsAddressRow.AddressLine2,
                                                            fsAddressRow.City,
                                                            fsAddressRow.State,
                                                            fsAddressRow.CountryID);
        }

        public static string GetAddressForGeolocation(string postalCode, string addressLine1, string addressLine2, string city, string state, string countryID)
        {
            string addressText = string.Empty;
            bool firstValue = true;

            if (!string.IsNullOrEmpty(addressLine1))
            {
                addressText = (firstValue == true) ? addressLine1.Trim() : addressText + ", " + addressLine1.Trim();
                firstValue = false;
            }

            if (!string.IsNullOrEmpty(addressLine2))
            {
                addressText = (firstValue == true) ? addressLine2.Trim() : addressText + ", " + addressLine2.Trim();
                firstValue = false;
            }

            if (!string.IsNullOrEmpty(city))
            {
                addressText = (firstValue == true) ? city.Trim() : addressText + ", " + city.Trim();
                firstValue = false;
            }

            if (!string.IsNullOrEmpty(state))
            {
                addressText = (firstValue == true) ? state.Trim() : addressText + ", " + state.Trim();
                firstValue = false;
            }

            if (!string.IsNullOrEmpty(postalCode))
            {
                addressText = (firstValue == true) ? postalCode.Trim() : addressText + ", " + postalCode.Trim();
                firstValue = false;
            }

            if (!string.IsNullOrEmpty(countryID))
            {
                addressText = (firstValue == true) ? countryID.Trim() : addressText + ", " + countryID.Trim();
                firstValue = false;
            }

            return addressText;
        }

        /// <summary>
        /// Extracts time info from 'date' field.
        /// </summary>
        /// <param name="date">DateTime field from where the time info is extracted.</param>
        /// <returns>A string with the following format: HH:MM AM/PM.</returns>
        public static string GetTimeStringFromDate(DateTime? date)
        {
            if (date.HasValue == false)
            {
                return TX.Error.SCHEDULED_DATE_UNAVAILABLE;
            }

            return date.Value.ToString("hh:mm tt");
        }

        /// <summary>
        /// Get the BAccountType based on the staffMemberID.
        /// </summary>
        public static string GetBAccountType(PXGraph graph, int? staffMemberID)
        {
            if (staffMemberID == null)
            {
                throw new PXException(TX.Error.STAFF_MEMBER_INCONSISTENCY);
            }

            BAccount bAccountRow = PXSelect<BAccount,
                                   Where<
                                        BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                                   .Select(graph, staffMemberID);

            if (bAccountRow != null)
            {
                if (bAccountRow.Type == BAccountType.CombinedType || bAccountRow.Type == BAccountType.VendorType)
                {
                    return BAccountType.VendorType;
                }
                else if (bAccountRow.Type == BAccountType.EmpCombinedType || bAccountRow.Type == BAccountType.EmployeeType)
                {
                    return BAccountType.EmployeeType;
                }
                else
                {
                    throw new PXException(TX.Error.BACCOUNT_TYPE_DOES_NOT_MATCH_WITH_STAFF_MEMBERS_OPTIONS);
                }
            }
            else
            {
                throw new PXException(TX.Error.STAFF_MEMBER_INCONSISTENCY);
            }
        }

        #region GetItemWithList Methods

        /// <summary>
        /// Gets a SharedClasses.ItemList type list of recordID's of a field (FieldSearch) belonging to a list of items (FieldList) 
        /// from a table (Table) with a Join condition (Join) and a where clause (TWhere).
        /// </summary>
        /// <typeparam name="Table">Main table for the BQL.</typeparam>
        /// <typeparam name="Join">Join for the BQL.</typeparam>
        /// <typeparam name="FieldList">Search field for the select in BQL.</typeparam>
        /// <typeparam name="FieldSearch">Row filter field.</typeparam>
        /// <typeparam name="TWhere">Where BQL conditions.</typeparam>
        public static List<SharedClasses.ItemList> GetItemWithList<Table, Join, FieldList, FieldSearch, TWhere>(PXGraph graph, List<int?> fieldList, params object[] paramObjects)
            where Table : class, IBqlTable, new()
            where Join : IBqlJoin, new()
            where FieldList : IBqlField
            where FieldSearch : IBqlField
            where TWhere : IBqlWhere, new()
        {
            if (fieldList.Count == 0)
            {
                return new List<SharedClasses.ItemList>();
            }

            List<object> objectList = fieldList.Cast<object>().ToList();

            if (paramObjects.Count() > 0)
            {
                objectList = paramObjects.Concat(objectList).ToList();
            }

            BqlCommand fsTableBql = new Select2<Table, Join>();
            fsTableBql = fsTableBql.WhereAnd<TWhere>();
            fsTableBql = fsTableBql.WhereAnd(InHelper<FieldList>.Create(objectList.Count));

            return PopulateItemList<Table, FieldList, FieldSearch>(graph, fsTableBql, objectList);
        }

        /// <summary>
        /// Gets a SharedClasses.ItemList type list of recordID's of a field (FieldSearch) belonging to a list of items (FieldList) 
        /// from a table (Table) with a where clause (TWhere).
        /// </summary>
        /// <typeparam name="Table">Main table for the BQL.</typeparam>
        /// <typeparam name="FieldList">Search field for the select in BQL.</typeparam>
        /// <typeparam name="FieldSearch">Row filter field.</typeparam>
        /// <typeparam name="TWhere">Where BQL conditions.</typeparam>
        public static List<SharedClasses.ItemList> GetItemWithList<Table, FieldList, FieldSearch, TWhere>(PXGraph graph, List<int?> fieldList, params object[] paramObjects)
            where Table : class, IBqlTable, new()
            where FieldList : IBqlField
            where FieldSearch : IBqlField
            where TWhere : IBqlWhere, new()
        {
            if (fieldList.Count == 0)
            {
                return new List<SharedClasses.ItemList>();
            }

            List<object> objectList = fieldList.Cast<object>().ToList();

            if (paramObjects.Count() > 0)
            {
                objectList = paramObjects.Concat(objectList).ToList();
            }

            BqlCommand fsTableBql = new Select<Table>();
            fsTableBql = fsTableBql.WhereAnd<TWhere>();
            fsTableBql = fsTableBql.WhereAnd(InHelper<FieldList>.Create(objectList.Count));

            return PopulateItemList<Table, FieldList, FieldSearch>(graph, fsTableBql, objectList);
        }

        /// <summary>
        /// Gets a SharedClasses.ItemList type list of recordID's of a field (FieldSearch) belonging to a list of items (FieldList) 
        /// from a table (Table).
        /// </summary>
        /// <typeparam name="Table">Main table for the BQL.</typeparam>
        /// <typeparam name="FieldList">Search field for the select in BQL.</typeparam>
        /// <typeparam name="FieldSearch">Row filter field.</typeparam>
        public static List<SharedClasses.ItemList> GetItemWithList<Table, FieldList, FieldSearch>(PXGraph graph, List<int?> fieldList)
            where Table : class, IBqlTable, new()
            where FieldList : IBqlField
            where FieldSearch : IBqlField
        {
            if (fieldList.Count == 0)
            {
                return new List<SharedClasses.ItemList>();
            }

            List<object> objectList = fieldList.Cast<object>().ToList();

            BqlCommand fsTableBql = new Select<Table>();
            fsTableBql = fsTableBql.WhereAnd(InHelper<FieldList>.Create(objectList.Count));

            return PopulateItemList<Table, FieldList, FieldSearch>(graph, fsTableBql, objectList);
        }

        /// <summary>
        /// Populate a SharedClasses.ItemList type list with recordID's of a field (FieldSearch) belonging to a list of items (FieldList) 
        /// from a table (Table).
        /// </summary>
        /// <typeparam name="Table">Main table for the BQL.</typeparam>
        /// <typeparam name="FieldList">Search field for the select in BQL.</typeparam>
        /// <typeparam name="FieldSearch">Row filter field.</typeparam>
        private static List<SharedClasses.ItemList> PopulateItemList<Table, FieldList, FieldSearch>(PXGraph graph, BqlCommand fsTableBql, List<object> fieldList)
            where Table : class, IBqlTable, new()
            where FieldList : IBqlField
            where FieldSearch : IBqlField
        {
            List<SharedClasses.ItemList> itemList = new List<SharedClasses.ItemList>();

            PXView fsTableView = new PXView(graph, true, fsTableBql);
            var fsTableRows = fsTableView.SelectMulti(fieldList.ToArray());

            string fieldListName = Regex.Replace(typeof(FieldList).Name, "^[a-z]", m => m.Value.ToUpper());
            string fieldSearchName = Regex.Replace(typeof(FieldSearch).Name, "^[a-z]", m => m.Value.ToUpper());

            Type[] tables = fsTableBql.GetTables();
            bool withJoin = tables.Count() > 1;

            foreach (var row in fsTableRows)
            {
                Table objectRow;

                if (withJoin)
                {
                    PXResult<Table> bqlResult = (PXResult<Table>)row;
                    objectRow = (Table)bqlResult;
                }
                else
                {
                    objectRow = (Table)row;
                }

                var fieldListValue = typeof(Table).GetProperty(fieldListName).GetValue(objectRow);
                var fieldSearchValue = typeof(Table).GetProperty(fieldSearchName).GetValue(objectRow);

                var item = itemList.Where(list => list.itemID == (int?)fieldListValue).FirstOrDefault();
                if (item != null)
                {
                    item.list.Add(fieldSearchValue);
                }
                else
                {
                    SharedClasses.ItemList newItem = new SharedClasses.ItemList((int?)fieldListValue);
                    newItem.list.Add(fieldSearchValue);
                    itemList.Add(newItem);
                }
            }

            return itemList;
        }
        #endregion

        /// <summary>
        /// Checks if the given Business Account identifier is a prospect type.
        /// </summary>
        /// <param name="graph">Context graph.</param>
        /// <param name="bAccountID">Business Account identifier.</param>
        /// <returns>True is the Business Account is a Prospect.</returns>
        public static bool isThisAProspect(PXGraph graph, int? bAccountID)
        {
            BAccount bAccountRow = PXSelect<BAccount,
                                   Where<
                                       BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>,
                                       And<BAccount.type, Equal<BAccountType.prospectType>>>>
                                   .Select(graph, bAccountID);

            return bAccountRow != null;
        }

        /// <summary>
        /// Validates the Actual Start/End date times for a given Route Document.
        /// </summary>
        /// <param name="cache">Route Document Cache.</param>
        /// <param name="fsRouteDocumentRow">Route Document row.</param>
        /// <param name="businessDate">Current graph business date.</param>
        public static void CheckRouteActualDateTimes(PXCache cache, FSRouteDocument fsRouteDocumentRow, DateTime? businessDate)
        {
            if (fsRouteDocumentRow.ActualStartTime == null
                || fsRouteDocumentRow.ActualEndTime == null)
            {
                return;
            }

            PXSetPropertyException exception = null;
            DateTime startTime = (DateTime)fsRouteDocumentRow.ActualStartTime;
            DateTime endTime = (DateTime)fsRouteDocumentRow.ActualEndTime;

            if (startTime.Hour > endTime.Hour
                || (startTime.Hour == endTime.Hour && startTime.Minute > endTime.Minute))
            {
                exception = new PXSetPropertyException(TX.Error.END_TIME_LESSER_THAN_START_TIME, PXErrorLevel.RowError);
            }

            cache.RaiseExceptionHandling<FSRouteDocument.actualStartTime>(fsRouteDocumentRow, startTime, exception);
            cache.RaiseExceptionHandling<FSRouteDocument.actualEndTime>(fsRouteDocumentRow, endTime, exception);

            if (exception != null)
            {
                fsRouteDocumentRow.MemActualDuration = null;
            }

            if (fsRouteDocumentRow.ActualStartTime != null
                && fsRouteDocumentRow.ActualEndTime != null)
            {
                fsRouteDocumentRow.ActualStartTime = new DateTime(businessDate.Value.Year,
                                                                  businessDate.Value.Month,
                                                                  businessDate.Value.Day,
                                                                  fsRouteDocumentRow.ActualStartTime.Value.Hour,
                                                                  fsRouteDocumentRow.ActualStartTime.Value.Minute,
                                                                  fsRouteDocumentRow.ActualStartTime.Value.Second);

                fsRouteDocumentRow.ActualEndTime = new DateTime(businessDate.Value.Year,
                                                                businessDate.Value.Month,
                                                                businessDate.Value.Day,
                                                                fsRouteDocumentRow.ActualEndTime.Value.Hour,
                                                                fsRouteDocumentRow.ActualEndTime.Value.Minute,
                                                                fsRouteDocumentRow.ActualEndTime.Value.Second);
            }
        }

        public static DateTime? GetCustomDateTime(DateTime? datePart, DateTime? timePart)
        {
            // TODO: remove this method and use GetTimeWithSpecificDate directly.
            return GetTimeWithSpecificDate(timePart, datePart);
        }

        public static DateTime? GetTimeWithSpecificDate(DateTime? time, DateTime? specificDate, bool ignoreSeconds = false)
        {
            if (specificDate == null)
            {
                return time;
            }

            if (time == null)
            {
                return null;
            }

            return new DateTime(specificDate.Value.Year,
                                specificDate.Value.Month,
                                specificDate.Value.Day,
                                time.Value.Hour,
                                time.Value.Minute,
                                ignoreSeconds == true ? 0 : time.Value.Second);
        }

        /// <summary>
        /// Tries to parse the <c>newValue</c> to DateTime?. When the <c>newValue</c> is string and the DateTime TryParse is not possible returns null. Otherwise returns (DateTime?) <c>newValue</c>.
        /// </summary>
        public static DateTime? TryParseHandlingDateTime(PXCache cache, object newValue)
        {
            if (newValue == null)
            {
                return null;
            }

            DateTime valFromString;

            if (newValue is string)
            {
                if (DateTime.TryParse((string)newValue, cache.Graph.Culture, DateTimeStyles.None, out valFromString))
                {
                    return valFromString;
                }
                else
                {
                    return null;
                }
            }

            return (DateTime?)newValue;
        }

        /// <summary>
        /// Create an Equipment from a sold Inventory Item.
        /// </summary>
        /// <param name="graphSMEquipmentMaint"> Equipment graph.</param>
        /// <param name="soldInventoryItemRow">Sold Inventory Item data.</param>
        public static FSEquipment CreateSoldEquipment(SMEquipmentMaint graphSMEquipmentMaint, SoldInventoryItem soldInventoryItemRow, ARTran arTranRow, FSxARTran fsxARTranRow, SOLine soLineRow, string action, InventoryItem inventoryItemRow)
        {
            FSEquipment fsEquipmentRow = new FSEquipment();
            fsEquipmentRow.OwnerType = ID.OwnerType_Equipment.CUSTOMER;
            fsEquipmentRow.RequireMaintenance = true;
            
            fsEquipmentRow.Descr = (soldInventoryItemRow.Descr == null) ? soldInventoryItemRow.InventoryCD : soldInventoryItemRow.Descr;
            fsEquipmentRow.SiteID = soldInventoryItemRow.SiteID;

            if (inventoryItemRow != null)
            {
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);
                fsEquipmentRow.EquipmentTypeID = fsxEquipmentModelRow?.EquipmentTypeID;
            }

            //Sales Info
            fsEquipmentRow.SalesOrderType = soldInventoryItemRow.SOOrderType;
            fsEquipmentRow.SalesOrderNbr = soldInventoryItemRow.SOOrderNbr;

            //Customer Info    
            fsEquipmentRow.OwnerID = soldInventoryItemRow.CustomerID;
            fsEquipmentRow.CustomerID = soldInventoryItemRow.CustomerID;
            fsEquipmentRow.CustomerLocationID = soldInventoryItemRow.CustomerLocationID;

            //Lot/Serial Info.
            fsEquipmentRow.INSerialNumber = soldInventoryItemRow.LotSerialNumber;
            fsEquipmentRow.SerialNumber = soldInventoryItemRow.LotSerialNumber;

            //Source info.
            fsEquipmentRow.SourceType = ID.SourceType_Equipment.AR_INVOICE;
            fsEquipmentRow.SourceDocType = soldInventoryItemRow.DocType;
            fsEquipmentRow.SourceRefNbr = soldInventoryItemRow.InvoiceRefNbr;
            fsEquipmentRow.ARTranLineNbr = soldInventoryItemRow.InvoiceLineNbr;

            //Installation Info
            if (fsxARTranRow != null)
            {
                fsEquipmentRow.InstServiceOrderID = fsxARTranRow.SOID;
                fsEquipmentRow.InstAppointmentID = fsxARTranRow.AppointmentID;
            }

            if (action == ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT)
            {
                if (fsxARTranRow != null)
                {
                    //Equipment Replaced
                    fsEquipmentRow.EquipmentReplacedID = fsxARTranRow.SMEquipmentID;
                }
            }

            fsEquipmentRow = graphSMEquipmentMaint.EquipmentRecords.Insert(fsEquipmentRow);
            graphSMEquipmentMaint.EquipmentRecords.SetValueExt<FSEquipment.inventoryID>(fsEquipmentRow, soldInventoryItemRow.InventoryID);
            graphSMEquipmentMaint.EquipmentRecords.SetValueExt<FSEquipment.dateInstalled>(fsEquipmentRow, soldInventoryItemRow.DocDate);
            graphSMEquipmentMaint.EquipmentRecords.SetValueExt<FSEquipment.salesDate>(fsEquipmentRow, soldInventoryItemRow.SOOrderDate != null ? soldInventoryItemRow.SOOrderDate : soldInventoryItemRow.DocDate);

            //Attributes
            graphSMEquipmentMaint.Answers.CopyAllAttributes(graphSMEquipmentMaint.EquipmentRecords.Current, inventoryItemRow);

            //Image
            PXNoteAttribute.CopyNoteAndFiles(graphSMEquipmentMaint.Caches[typeof(InventoryItem)], inventoryItemRow, graphSMEquipmentMaint.Caches[typeof(FSEquipment)], graphSMEquipmentMaint.EquipmentRecords.Current, false, true);
            fsEquipmentRow.ImageUrl = inventoryItemRow.ImageUrl;

            graphSMEquipmentMaint.Save.Press();
            fsEquipmentRow = graphSMEquipmentMaint.EquipmentRecords.Current;

            if (fsEquipmentRow != null && arTranRow != null && fsxARTranRow != null)
            {
                foreach (FSEquipmentComponent fsEquipmentComponentRow in graphSMEquipmentMaint.EquipmentWarranties.Select())
                {
                    fsEquipmentComponentRow.SalesOrderNbr = arTranRow.SOOrderNbr;
                    fsEquipmentComponentRow.SalesOrderType = arTranRow.SOOrderType;
                    fsEquipmentComponentRow.InstServiceOrderID = fsxARTranRow.SOID;
                    fsEquipmentComponentRow.InstAppointmentID = fsxARTranRow.AppointmentID;
                    fsEquipmentComponentRow.InvoiceRefNbr = soldInventoryItemRow.InvoiceRefNbr;
                    graphSMEquipmentMaint.EquipmentWarranties.Update(fsEquipmentComponentRow);
                    graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.installationDate>(fsEquipmentComponentRow, soldInventoryItemRow.DocDate);
                    graphSMEquipmentMaint.EquipmentWarranties.SetValueExt<FSEquipmentComponent.salesDate>(fsEquipmentComponentRow, soLineRow != null && soLineRow.OrderDate != null ? soLineRow.OrderDate : arTranRow.TranDate);
                }

                graphSMEquipmentMaint.Save.Press();
            }

            return fsEquipmentRow;
        }

        /// <summary>
        /// Retrieves an Equipment row by its ID.
        /// </summary>
        public static FSEquipment GetEquipmentRow(PXGraph graph, int? smEquipmentID)
        {
            if (smEquipmentID == null)
            {
                return null;
            }

            FSEquipment fSEquipmentRow = PXSelect<FSEquipment,
                                         Where<
                                             FSEquipment.SMequipmentID, Equal<Required<FSEquipment.SMequipmentID>>>>
                                         .Select(graph, smEquipmentID);

            return fSEquipmentRow;
        }

        /// <summary>
        /// Checks whether there is or not any generation process associated with scheduleID.
        /// </summary>
        /// <returns>True if there is a generation process, otherwise it returns False.</returns>
        public static bool isThereAnyGenerationProcessForThisSchedule(PXCache cache, int? scheduleID)
        {
            bool anyGenerationProcess = true;

            if (scheduleID > 0)
            {
                int scheduleProcessed = PXSelect<FSContractGenerationHistory,
                                        Where<
                                              FSContractGenerationHistory.scheduleID, Equal<Required<FSContractGenerationHistory.scheduleID>>>>
                                        .SelectWindowed(cache.Graph, 0, 1, scheduleID).Count;

                anyGenerationProcess = scheduleProcessed != 0;
            }

            return anyGenerationProcess;
        }

        /// <summary>
        /// Shows a warning message if the current schedule has not been processed yet.
        /// </summary>
        /// <returns>True if there is a generation process, otherwise it returns False.</returns>
        public static bool ShowWarningScheduleNotProcessed(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool anyGenerationProcess = SharedFunctions.isThereAnyGenerationProcessForThisSchedule(cache, fsScheduleRow.ScheduleID);

            if (anyGenerationProcess == false)
            {
                cache.RaiseExceptionHandling<FSSchedule.refNbr>(fsScheduleRow,
                                                                fsScheduleRow.RefNbr,
                                                                new PXSetPropertyException(TX.Warning.SCHEDULE_WILL_NOT_AFFECT_SYSTEM_UNTIL_GENERATION_OCCURS, PXErrorLevel.RowWarning));
            }

            return anyGenerationProcess;
        }

        /// <summary>
        /// Change Route status from Closed to Completed.
        /// </summary>
        /// <param name="viewCache">PXCache instance of the view.</param>
        /// <param name="fsRouteDocumentRow">Current fsRouteDocumentRow object row.</param>
        public static void UncloseRoute(PXCache viewCache, FSRouteDocument fsRouteDocumentRow)
        {
            fsRouteDocumentRow.Status = ID.Status_Route.COMPLETED;
            SaveUpdatedChanges(viewCache, fsRouteDocumentRow);
        }

        /// <summary>
        /// Save updated changes existing in row.
        /// </summary>
        public static void SaveUpdatedChanges(PXCache viewCache, object row)
        {
            viewCache.AllowUpdate = true;
            viewCache.SetStatus(row, PXEntryStatus.Updated);
            viewCache.Graph.GetSaveAction().Press();
        }

        public static object ChangeAppointmentStatus(PXView view, object row, string newStatus)
        {
            object copy = view.Cache.CreateCopy(row);
            ((FSAppointment)copy).Status = newStatus;
            return view.Cache.Update(copy);
        }

        /// <summary>
        /// Propagates the change of Base Unit to the Service Contract Details.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="inventoryItemRow">InventoryItem object row.</param>
        /// <param name="tranStatus">Transaction status.</param>
        /// <param name="operation">Operation committed to DB.</param>
        /// <param name="baseUnitChanged">True if the Base unit has been changed, otherwise false.</param>
        public static void PropagateBaseUnitToContracts(PXCache cache, InventoryItem inventoryItemRow, PXTranStatus tranStatus, PXDBOperation operation, bool baseUnitChanged)
        {
            if (tranStatus == PXTranStatus.Completed && operation == PXDBOperation.Update && baseUnitChanged == true)
            {
                PXUpdate<
                    Set<FSSalesPrice.uOM, Required<FSSalesPrice.uOM>>,
                    FSSalesPrice,
                    Where<
                        FSSalesPrice.inventoryID, Equal<Required<FSSalesPrice.inventoryID>>>>
                .Update(cache.Graph, inventoryItemRow.BaseUnit, inventoryItemRow.InventoryID);
            }
        }

        /// <summary>
        /// Verifies if there is at least one Service Contract detail related to the specified Service.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="inventoryID">Inventory ID related to the service.</param>
        /// <returns>True if at least there is one detail related to, false otherwise.</returns>
        public static bool IsServiceRelatedToAnyContract(PXCache cache, int? inventoryID)
        {
            int rowCount = PXSelectJoin<FSSalesPrice,
                           InnerJoin<InventoryItem,
                                On<InventoryItem.inventoryID, Equal<FSSalesPrice.inventoryID>>>,
                           Where<
                                InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>
                           .SelectWindowed(cache.Graph, 0, 1, inventoryID).Count;

            return rowCount > 0;
        }

        /// <summary>
        /// Gets the name of the specified field with the default option to capitalize its first letter.
        /// </summary>
        /// <typeparam name="field">Field from where to get the name.</typeparam>
        /// <param name="capitalizedFirstLetter">Flag to indicate if the first letter is capital.</param>
        /// <returns>Returns the field's name.</returns>
        public static string GetFieldName<field>(bool capitalizedFirstLetter = true)
            where field : IBqlField
        {
            string fieldName = typeof(field).Name;

            if (capitalizedFirstLetter == true)
            {
                fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
            }

            return fieldName;
        }

        /// <summary>
        /// Copy all common fields from a source row to a target row skipping special fields like key fields and Acumatica creation/update fields.
        /// Optionally you can pass a list of field names to exclude of the copy.
        /// </summary>
        /// <param name="cacheTarget">The cache of the target row.</param>
        /// <param name="rowTarget">The target row.</param>
        /// <param name="cacheSource">The cache of the source row.</param>
        /// <param name="rowSource">The source row.</param>
        /// <param name="excludeFields">List of field names to exclude of the copy.</param>
        public static void CopyCommonFields(PXCache cacheTarget, IBqlTable rowTarget, PXCache cacheSource, IBqlTable rowSource, params string[] excludeFields)
        {
            bool skipField;
            string fieldName;

            foreach (Type bqlField in cacheTarget.BqlFields)
            {
                fieldName = bqlField.Name;

                if (excludeFields != null && Array.Exists<string>(excludeFields, element => element.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (fieldName.Contains("_"))
                {
                    continue;
                }

                if (cacheSource.Fields.Exists(element => element.Equals(fieldName, StringComparison.OrdinalIgnoreCase)))
                {
                    skipField = false;

                    foreach (PXEventSubscriberAttribute attr in cacheTarget.GetAttributes(fieldName))
                    {
                        if (attr is PXDBIdentityAttribute
                            || (attr is PXDBFieldAttribute && ((PXDBFieldAttribute)attr).IsKey == true)
                            || attr is PXDBCreatedByIDAttribute || attr is PXDBCreatedByScreenIDAttribute || attr is PXDBCreatedDateTimeAttribute
                            || attr is PXDBLastModifiedByIDAttribute || attr is PXDBLastModifiedByScreenIDAttribute || attr is PXDBLastModifiedDateTimeAttribute
                            || attr is PXDBTimestampAttribute)
                        {
                            skipField = true;
                            break;
                        }
                    }

                    if (skipField)
                    {
                        continue;
                    }

                    cacheTarget.SetValue(rowTarget, fieldName, cacheSource.GetValue(rowSource, fieldName));
                }
            }
        }

        public static int ReplicateCacheExceptions(PXCache cache, IBqlTable row, PXCache cacheWithExceptions, IBqlTable rowWithExceptions)
        {
            int errorCount = 0;
            PXFieldState fieldState;

            foreach (string field in cache.Fields)
            {
                try
                {
                    fieldState = (PXFieldState)cacheWithExceptions.GetStateExt(rowWithExceptions, field);
                }
                catch
                {
                    fieldState = null;
                }

                if (fieldState != null && fieldState.Error != null)
                {
                    cache.RaiseExceptionHandling(field, row, cache.GetValue(row, field), new PXSetPropertyException(fieldState.Error, fieldState.ErrorLevel));

                    if (fieldState.ErrorLevel != PXErrorLevel.RowInfo && fieldState.ErrorLevel != PXErrorLevel.RowWarning && fieldState.ErrorLevel != PXErrorLevel.Warning)
                    {
                        errorCount++;
                    }
                }
            }

            return errorCount;
        }

        /// <summary>
        /// Get the current instance URL.
        /// </summary>
        public static string GetInstanceUrl(string scheme, string authority, string application)
        {
            if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(authority))
            {
                return string.Empty;
            }

            return scheme + "://" + authority + application + "/(W(10000))/";
        }

        /// <summary>
        /// Get the web methods file path.
        /// </summary>
        public static string GetWebMethodPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            string pagePrefix = "/" + ID.AcumaticaFolderName.PAGE + "/";
            string modulePrefix = ID.Module.SERVICE_DISPATCH + "/";

            int pageIndex = path.LastIndexOf(pagePrefix.ToLower()) != -1 ? path.LastIndexOf(pagePrefix.ToLower()) : path.LastIndexOf(pagePrefix);

            if (pageIndex == -1)
            {
                pageIndex = 0;
            }
            else
            {
                pageIndex = pageIndex + pagePrefix.Length;
            }

            int index = path.IndexOf(modulePrefix.ToLower(), pageIndex) != -1 ? path.IndexOf(modulePrefix.ToLower(), pageIndex) : path.IndexOf(modulePrefix, pageIndex);

            if (index == -1)
            {
                return string.Empty;
            }

            return path.Substring(0, index + ID.Module.SERVICE_DISPATCH.Length) + "/" + ID.ScreenID.WEB_METHOD + ".aspx";
        }

        public static string GetMapApiKey(PXGraph graph)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(graph);

            return fsSetupRow != null ? fsSetupRow.MapApiKey : string.Empty;
        }

        public static XmlNamespaceManager GenerateXmlNameSpace(ref XmlNamespaceManager nameSpace)
        {
            nameSpace.AddNamespace(string.Format("{0}", ID.MapsConsts.XML_SCHEMA),
                string.Format("{0}", ID.MapsConsts.XML_SCHEMA_URI));
            return nameSpace;
        }

        public static string parseSecsDurationToString(int duration)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
            string totalDuration = null;

            if (timeSpan.Hours > 0)
            {
                totalDuration += timeSpan.Hours.ToString() + " hour";

                if (timeSpan.Hours > 1)
                    totalDuration += "s ";
                else
                    totalDuration += " ";

                if (timeSpan.Seconds >= 30)
                    totalDuration += (timeSpan.Minutes + 1).ToString() + " min";
                else
                    totalDuration += timeSpan.Minutes.ToString() + " min";

                if (timeSpan.Minutes > 1)
                    totalDuration += "s";
            }
            else
            {
                if (timeSpan.Seconds >= 30)
                    totalDuration += (timeSpan.Minutes + 1).ToString() + " min";
                else
                    totalDuration += timeSpan.Minutes.ToString() + " min";

                if (timeSpan.Minutes > 1)
                    totalDuration += "s";
            }

            return totalDuration;
        }

        public static bool isFSSetupSet(PXGraph graph)
        {
            if (PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>() == false)
            {
                return false;
            }

            FSSetup fsSetupRow = PXSelect<FSSetup>.SelectWindowed(graph, 0, 1);

            return fsSetupRow != null;
        }

        /// <summary>
        /// Evaluates if the current user has the access right requested in accessRight for the screen provided.
        /// </summary>
        /// <param name="screenName">Screen name to evaluate.</param>
        /// <param name="graphType">Graph type of the screen.</param>
        /// <param name="cacheType">Main <c>DAC</c> type of the screen.</param>
        /// <param name="accessRight">Access right level to evaluate.</param>
        /// <returns>True if users has the access right level requested, False, otherwise.</returns>
        public static bool CheckAccessRights(string screenName, Type graphType, Type cacheType, PXCacheRights accessRight)
        {
            if (PXAccess.VerifyRights(graphType))
            {
                PXCacheRights rights;
                List<string> invisible;
                List<string> disable;
                PXAccess.GetRights(screenName, graphType.Name, cacheType, out rights, out invisible, out disable);

                return rights >= accessRight;
            }

            return false;
        }

        public static string Right(this string value, int maxLength)
        {
            if (value == null)
            {
                return null;
            }

            string newString;

            if (value.Length > maxLength)
            {
                newString = value.Substring(value.Length - maxLength);
            }
            else
            {
                newString = value;
            }

            return newString;
        }

        /// <summary>
        /// Gets the <c>CustomerID</c> of the contract related to the given <c>projectID</c>.
        /// </summary>
        public static int? GetCustomerIDFromProjectID(PXGraph graph, int? projectID)
        {
            if (projectID != null
                    && ProjectDefaultAttribute.IsProject(graph, projectID))
            {
                Contract contractRow = PXSelect<Contract,
                                       Where<
                                           Contract.contractID, Equal<Required<Contract.contractID>>>>
                                       .Select(graph, projectID);

                if (contractRow != null)
                {
                    return contractRow.CustomerID;
                }
            }

            return null;
        }

        public static Customer GetCustomerRow(PXGraph graph, int? customerID)
        {
            if (customerID == null)
            {
                return null;
            }

            return PXSelect<Customer,
                   Where<
                        Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                   .Select(graph, customerID);
        }

        public static Vendor GetVendorRow(PXGraph graph, int? vendorID)
        {
            if (vendorID == null)
            {
                return null;
            }

            return PXSelect<Vendor,
                   Where<
                        Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>
                   .Select(graph, vendorID);
        }

        public static TimeSpan GetTimeSpanDiff(DateTime begin, DateTime end)
        {
            begin = new DateTime(begin.Year, begin.Month, begin.Day, begin.Hour, begin.Minute, 0);
            end = new DateTime(end.Year, end.Month, end.Day, end.Hour, end.Minute, 0);

            return (TimeSpan)(end - begin);
        }

        public static DateTime? RemoveTimeInfo(DateTime? date)
        {
            if (date != null)
            {
                int hour = date.Value.Hour;
                date = date.Value.AddHours(-hour);

                int minute = date.Value.Minute;
                date = date.Value.AddMinutes(-minute);
            }

            return date;
        }

        public static string GetCompleteCoordinate(decimal? longitude, decimal? latitude)
        {
            if (longitude == null || latitude == null)
            {
                return string.Empty;
            }

            return "[" + longitude + "],[" + latitude + "]";
        }

        public static bool IsNotAllowedBillingOptionsModification(FSBillingCycle fsBillingCycleRow)
        {
            if (fsBillingCycleRow != null)
            {
                return (fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.PURCHASE_ORDER && fsBillingCycleRow.GroupBillByLocations == false)
                            || (fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.WORK_ORDER && fsBillingCycleRow.GroupBillByLocations == false)
                                || (fsBillingCycleRow.BillingCycleType == ID.Billing_Cycle_Type.TIME_FRAME && fsBillingCycleRow.GroupBillByLocations == false);
            }

            return true;
        }

        #region Equipment
        public static bool AreEquipmentFieldsValid(PXCache cache, int? inventoryID, int? targetEQ, object newTargetEQLineNbr, string equipmentAction, ref string errorMessage)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                return true;
            }

            errorMessage = string.Empty;

            if (inventoryID != null)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, inventoryID);
                FSxEquipmentModel fsxEquipmentModel = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);

                switch (equipmentAction)
                {
                    case ID.Equipment_Action.SELLING_TARGET_EQUIPMENT:

                        if (fsxEquipmentModel.EquipmentItemClass != ID.Equipment_Item_Class.MODEL_EQUIPMENT)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_MODEL_EQUIPMENT_REQUIRED;
                            return false;
                        }

                        break;

                    case ID.Equipment_Action.CREATING_COMPONENT:

                        if (fsxEquipmentModel.EquipmentItemClass != ID.Equipment_Item_Class.COMPONENT)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_COMPONENT_REQUIRED;
                            return false;
                        }
                        else if (newTargetEQLineNbr == null && targetEQ == null)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_TARGET_EQUIP_OR_NEW_TARGET_EQUIP_REQUIRED;
                            return false;
                        }

                        break;

                    case ID.Equipment_Action.REPLACING_COMPONENT:

                        if (fsxEquipmentModel.EquipmentItemClass != ID.Equipment_Item_Class.COMPONENT)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_COMPONENT_REQUIRED;
                            return false;
                        }
                        else if (targetEQ == null)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_TARGET_EQUIP_OR_NEW_TARGET_EQUIP_REQUIRED;
                            return false;
                        }

                        break;

                    case ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT:

                        if (fsxEquipmentModel.EquipmentItemClass != ID.Equipment_Item_Class.MODEL_EQUIPMENT)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_MODEL_EQUIPMENT_REQUIRED;
                            return false;
                        }
                        else if (newTargetEQLineNbr == null && targetEQ == null)
                        {
                            errorMessage = TX.Error.EQUIPMENT_ACTION_TARGET_EQUIP_OR_NEW_TARGET_EQUIP_REQUIRED;
                            return false;
                        }

                        break;
                }
            }

            return true;
        }

        public static void UpdateEquipmentFields(PXGraph graph, PXCache cache, Object row, int? inventoryID, bool updateQty = true)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                return;
            }

            SetEquipmentActionFromInventory(graph, row, inventoryID);

            UpdateEquipmentAction(cache, row, updateQty: updateQty);
        }

        public static void SetEquipmentActionFromInventory(PXGraph graph, object row, int? inventoryID)
        {
            if (inventoryID == null || PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>() == false)
            {
                return;
            }

            string equipmentItemClass = null;
            FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(SharedFunctions.GetInventoryItemRow(graph, inventoryID));

            string equipmentAction = ID.Equipment_Action.NONE;

            if (fsxEquipmentModelRow != null && fsxEquipmentModelRow.EQEnabled == true)
            {
                equipmentAction = getEquipmentModelAction(graph, fsxEquipmentModelRow);
            }

            equipmentItemClass = fsxEquipmentModelRow?.EquipmentItemClass;

            if (row is SOLine)
            {
                SOLine soLineRow = row as SOLine;
                FSxSOLine fsxSOLineRow = PXCache<SOLine>.GetExtension<FSxSOLine>(soLineRow);
                fsxSOLineRow.EquipmentAction = equipmentAction;
                fsxSOLineRow.Comment = string.Empty;
                fsxSOLineRow.SMEquipmentID = null;
                fsxSOLineRow.NewTargetEquipmentLineNbr = null;
                fsxSOLineRow.ComponentID = null;
                fsxSOLineRow.EquipmentLineRef = null;
                fsxSOLineRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSSODet)
            {
                FSSODet fsSODetRow = row as FSSODet;
                fsSODetRow.EquipmentAction = equipmentAction;
                fsSODetRow.SMEquipmentID = null;
                fsSODetRow.NewTargetEquipmentLineNbr = null;
                fsSODetRow.ComponentID = null;
                fsSODetRow.EquipmentLineRef = null;
                fsSODetRow.Comment = null;
                fsSODetRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSAppointmentDet)
            {
                FSAppointmentDet fsAppointmentDetRow = row as FSAppointmentDet;
                fsAppointmentDetRow.EquipmentAction = equipmentAction;
                fsAppointmentDetRow.SMEquipmentID = null;
                fsAppointmentDetRow.NewTargetEquipmentLineNbr = null;
                fsAppointmentDetRow.ComponentID = null;
                fsAppointmentDetRow.EquipmentLineRef = null;
                fsAppointmentDetRow.Comment = null;
                fsAppointmentDetRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSScheduleDet)
            {
                FSScheduleDet fsScheduleDetRow = row as FSScheduleDet;
                equipmentAction = equipmentAction != ID.Equipment_Action_Base.SELLING_TARGET_EQUIPMENT ? ID.Equipment_Action_Base.NONE : equipmentAction;

                fsScheduleDetRow.EquipmentAction = equipmentAction;
                fsScheduleDetRow.SMEquipmentID = null;
                fsScheduleDetRow.ComponentID = null;
                fsScheduleDetRow.EquipmentItemClass = equipmentItemClass;
            }
        }

        public static string getEquipmentModelAction(PXGraph graph, FSxEquipmentModel fsxEquipmentModelRow)
        {
            if (fsxEquipmentModelRow != null)
            {
                switch (fsxEquipmentModelRow.EquipmentItemClass)
                {
                    case ID.Equipment_Item_Class.COMPONENT:
                        return ID.Equipment_Action.CREATING_COMPONENT;
                    case ID.Equipment_Item_Class.MODEL_EQUIPMENT:
                        return ID.Equipment_Action.SELLING_TARGET_EQUIPMENT;
                    default:
                        return ID.Equipment_Action.NONE;
                }
            }

            return null;
        }

        public static void UpdateEquipmentAction(PXCache cache, object row, bool updateQty = true)
        {
            if (!PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                return;
            }

            if (row is SOLine)
            {
                SOLine soLineRow = row as SOLine;
                FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

                UpdateEquipmentAction(cache,
                                      soLineRow,
                                      soLineRow.InventoryID,
                                      fsxSOLineRow.EquipmentItemClass,
                                      null,
                                      fsxSOLineRow.EquipmentAction,
                                      fsxSOLineRow.NewTargetEquipmentLineNbr,
                                      fsxSOLineRow.SMEquipmentID,
                                      fsxSOLineRow.ComponentID,
                                      typeof(FSxSOLine.equipmentAction).Name,
                                      typeof(FSxSOLine.sMEquipmentID).Name,
                                      typeof(FSxSOLine.newTargetEquipmentLineNbr).Name,
                                      typeof(FSxSOLine.componentID).Name,
                                      typeof(FSxSOLine.equipmentLineRef).Name,
                                      typeof(FSxSOLine.comment).Name,
                                      typeof(SOLine.orderQty).Name,
                                      updateQty);
            }
            else if (row is FSSODet)
            {
                FSSODet fsSODetRow = row as FSSODet;

                UpdateEquipmentAction(cache,
                                      fsSODetRow,
                                      fsSODetRow.InventoryID,
                                      fsSODetRow.EquipmentItemClass,
                                      fsSODetRow.LineType,
                                      fsSODetRow.EquipmentAction,
                                      fsSODetRow.NewTargetEquipmentLineNbr,
                                      fsSODetRow.SMEquipmentID,
                                      fsSODetRow.ComponentID,
                                      typeof(FSSODet.equipmentAction).Name,
                                      typeof(FSSODet.SMequipmentID).Name,
                                      typeof(FSSODet.newTargetEquipmentLineNbr).Name,
                                      typeof(FSSODet.componentID).Name,
                                      typeof(FSSODet.equipmentLineRef).Name,
                                      typeof(FSSODet.comment).Name,
                                      typeof(FSSODet.estimatedQty).Name);
            }
            else if (row is FSAppointmentDet)
            {
                FSAppointmentDet fsAppointmentDet = row as FSAppointmentDet;

                UpdateEquipmentAction(cache,
                                      fsAppointmentDet,
                                      fsAppointmentDet.InventoryID,
                                      fsAppointmentDet.EquipmentItemClass,
                                      fsAppointmentDet.LineType,
                                      fsAppointmentDet.EquipmentAction,
                                      fsAppointmentDet.NewTargetEquipmentLineNbr,
                                      fsAppointmentDet.SMEquipmentID,
                                      fsAppointmentDet.ComponentID,
                                      typeof(FSAppointmentDet.equipmentAction).Name,
                                      typeof(FSAppointmentDet.SMequipmentID).Name,
                                      typeof(FSAppointmentDet.newTargetEquipmentLineNbr).Name,
                                      typeof(FSAppointmentDet.componentID).Name,
                                      typeof(FSAppointmentDet.equipmentLineRef).Name,
                                      typeof(FSAppointmentDet.comment).Name,
                                      typeof(FSAppointmentDet.qty).Name,
                                      updateQty);
            }
            else if (row is FSScheduleDet)
            {
                FSScheduleDet fsScheduleDet = row as FSScheduleDet;

                UpdateEquipmentAction(cache,
                                      fsScheduleDet,
                                      fsScheduleDet.InventoryID,
                                      fsScheduleDet.EquipmentItemClass,
                                      fsScheduleDet.LineType,
                                      fsScheduleDet.EquipmentAction,
                                      null,
                                      fsScheduleDet.SMEquipmentID,
                                      fsScheduleDet.ComponentID,
                                      typeof(FSScheduleDet.equipmentAction).Name,
                                      typeof(FSScheduleDet.SMequipmentID).Name,
                                      null,
                                      typeof(FSScheduleDet.componentID).Name,
                                      typeof(FSScheduleDet.equipmentLineRef).Name,
                                      null,
                                      typeof(FSScheduleDet.qty).Name);
            }
        }

        public static void ResetEquipmentFields(PXCache cache, object row)
        {
            if (row is SOLine)
            {
                SOLine soLineRow = row as SOLine;
                FSxSOLine fsxSOLineRow = cache.GetExtension<FSxSOLine>(soLineRow);

                fsxSOLineRow.SMEquipmentID = null;
                fsxSOLineRow.NewTargetEquipmentLineNbr = null;
                fsxSOLineRow.ComponentID = null;
                fsxSOLineRow.EquipmentLineRef = null;
            }
            else if (row is FSSODet)
            {
                FSSODet fsSODetRow = row as FSSODet;

                fsSODetRow.SMEquipmentID = null;
                fsSODetRow.NewTargetEquipmentLineNbr = null;
                fsSODetRow.ComponentID = null;
                fsSODetRow.EquipmentLineRef = null;
            }
            else if (row is FSAppointmentDet)
            {
                FSAppointmentDet fsAppointmentDetRow = row as FSAppointmentDet;

                fsAppointmentDetRow.SMEquipmentID = null;
                fsAppointmentDetRow.NewTargetEquipmentLineNbr = null;
                fsAppointmentDetRow.ComponentID = null;
                fsAppointmentDetRow.EquipmentLineRef = null;
            }
            else if (row is FSScheduleDet)
            {
                FSScheduleDet fsScheduleDetRow = row as FSScheduleDet;

                fsScheduleDetRow.SMEquipmentID = null;
                fsScheduleDetRow.ComponentID = null;
                fsScheduleDetRow.EquipmentLineRef = null;
            }
        }

        public static void UpdateEquipmentAction(PXCache cache, object row, int? inventoryID, string EquipmentItemClass, string lineType, string eQAction, object newTargetEQLineNbr, int? sMEquipmentID, int? componentID, string eQActionFieldName, string sMEquipmentIDFieldName, string newTargetEQFieldName, string componentIDFieldName, string equipmentLineRefFieldName, string commentFieldName, string quantityFieldName, bool updateQty = true)
        {
            if (inventoryID != null || lineType == ID.LineType_ALL.SERVICE)
            {
                bool enableComponentID = false;
                bool enableTargetEquipment = false;
                bool enableNewTargetEquipmentNbr = false;
                bool enableEquipmentLineRef = sMEquipmentID != null;
                bool enableQty = true;
                bool isComponentIDRequired = false;
                bool isEquipmentLineRefRequired = false;
                bool enableComment = false;

                if (EquipmentItemClass != null)
                {
                    switch (eQAction)
                    {
                        case ID.Equipment_Action.SELLING_TARGET_EQUIPMENT:

                            if (EquipmentItemClass != ID.Equipment_Item_Class.MODEL_EQUIPMENT)
                            {
                                cache.RaiseExceptionHandling(eQActionFieldName, row, eQAction, new PXSetPropertyException(TX.Error.EQUIPMENT_ACTION_MODEL_EQUIPMENT_REQUIRED));
                            }

                            break;

                        case ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT:

                            if (EquipmentItemClass != ID.Equipment_Item_Class.MODEL_EQUIPMENT)
                            {
                                cache.RaiseExceptionHandling(eQActionFieldName, row, eQAction, new PXSetPropertyException(TX.Error.EQUIPMENT_ACTION_MODEL_EQUIPMENT_REQUIRED));
                            }
                            else
                            {
                                enableTargetEquipment = true;
                                enableNewTargetEquipmentNbr = enableEquipmentLineRef = enableQty = false;
                            }

                            break;

                        case ID.Equipment_Action.CREATING_COMPONENT:
                        case ID.Equipment_Action.UPGRADING_COMPONENT:
                        case ID.Equipment_Action.REPLACING_COMPONENT:

                            if (EquipmentItemClass != ID.Equipment_Item_Class.COMPONENT)
                            {
                                cache.RaiseExceptionHandling(eQActionFieldName, row, eQAction, new PXSetPropertyException(TX.Error.EQUIPMENT_ACTION_COMPONENT_REQUIRED));
                            }
                            else
                            {
                                enableComponentID = enableTargetEquipment = enableNewTargetEquipmentNbr = isComponentIDRequired = enableComment = true;
                                enableQty = false;

                                if (eQAction == ID.Equipment_Action.UPGRADING_COMPONENT)
                                {
                                    enableTargetEquipment = false;
                                }
                                else if (eQAction == ID.Equipment_Action.REPLACING_COMPONENT)
                                {
                                    enableNewTargetEquipmentNbr = false;
                                    enableEquipmentLineRef = true;
                                    isEquipmentLineRefRequired = true;
                                }
                            }

                            break;

                        case ID.Equipment_Action.NONE:
                            if (EquipmentItemClass == ID.Equipment_Item_Class.CONSUMABLE
                                    || EquipmentItemClass == ID.Equipment_Item_Class.PART_OTHER_INVENTORY)
                            {
                            enableTargetEquipment = enableNewTargetEquipmentNbr = true;
                            enableComponentID = newTargetEQLineNbr != null || sMEquipmentID != null;
                            }

                            isComponentIDRequired = false;
                            break;
                    }
                }

                if (eQAction == ID.Equipment_Action.NONE
                        && (lineType == ID.LineType_ALL.SERVICE ||
                            lineType == ID.LineType_ALL.NONSTOCKITEM ||
                            lineType == ID.LineType_ALL.INVENTORY_ITEM))
                {
                    enableComponentID = enableTargetEquipment = enableNewTargetEquipmentNbr = true;
                    isEquipmentLineRefRequired = enableEquipmentLineRef && componentID != null;
                }

                PXUIFieldAttribute.SetEnabled(cache, row, sMEquipmentIDFieldName, enableTargetEquipment && newTargetEQLineNbr == null);
                PXUIFieldAttribute.SetEnabled(cache, row, componentIDFieldName, enableComponentID);

                if (row is FSSODet && cache.Graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SERVICE_ORDER)
                    || row is FSAppointmentDet && cache.Graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT))
                {
                    PXDefaultAttribute.SetPersistingCheck(cache, componentIDFieldName, row, PXPersistingCheck.Nothing);
                }
                else
                {
                    PXDefaultAttribute.SetPersistingCheck(cache, componentIDFieldName, row, isComponentIDRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                }

                if (newTargetEQFieldName != null)
                {
                    PXUIFieldAttribute.SetEnabled(cache, row, newTargetEQFieldName, enableNewTargetEquipmentNbr
                                                                                        && sMEquipmentID == null);
                }

                if (equipmentLineRefFieldName != null)
                {
                    PXUIFieldAttribute.SetEnabled(cache, row, equipmentLineRefFieldName, enableEquipmentLineRef);
                    if (row is FSSODet && cache.Graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.SERVICE_ORDER)
                        || row is FSAppointmentDet && cache.Graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT))
                    {
                        PXDefaultAttribute.SetPersistingCheck(cache, equipmentLineRefFieldName, row, PXPersistingCheck.Nothing);
                    }
                    else
                    {
                        PXDefaultAttribute.SetPersistingCheck(cache, equipmentLineRefFieldName, row, isEquipmentLineRefRequired ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
                    }

                }

                if (commentFieldName != null)
                {
                    PXUIFieldAttribute.SetEnabled(cache, row, commentFieldName, enableComment && componentID != null);
                }

                if (lineType != ID.LineType_ALL.SERVICE
                        && enableQty == false 
                        && cache.Graph.IsCopyPasteContext == false)
                {
                    if (updateQty)
                    {
                        cache.SetValueExt(row, quantityFieldName, 1.0m);
                    }
                    PXUIFieldAttribute.SetEnabled(cache, row, quantityFieldName, enableQty);
                }
            }

            if (newTargetEQFieldName != null &&
                (lineType == ID.LineType_ALL.COMMENT
                || lineType == ID.LineType_ALL.INSTRUCTION))
            {
                PXUIFieldAttribute.SetEnabled(cache, row, newTargetEQFieldName, sMEquipmentID == null);
            }
        }

        public static void SetInventoryItemExtensionInfo(PXGraph graph, int? inventoryID, object row)
        {
            if (inventoryID == null)
            {
                return;
            }
            string equipmentItemClass = null;

            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);
            if (PXAccess.FeatureInstalled<FeaturesSet.equipmentManagementModule>())
            {
                FSxEquipmentModel fsxEquipmentModelRow = PXCache<InventoryItem>.GetExtension<FSxEquipmentModel>(inventoryItemRow);
                equipmentItemClass = fsxEquipmentModelRow?.EquipmentItemClass;
            }

            if (row is FSxSOLine)
            {
                FSxSOLine fsxSOLineRow = (FSxSOLine)row;
                fsxSOLineRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSScheduleDet)
            {
                FSScheduleDet fsScheduleDetRow = (FSScheduleDet)row;
                fsScheduleDetRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSSODet)
            {
                FSSODet fsSODetRow = (FSSODet)row;
                fsSODetRow.EquipmentItemClass = equipmentItemClass;
            }
            else if (row is FSAppointmentDet)
            {
                FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;
                FSxService fsxServicerow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                fsAppointmentDetRow.EquipmentItemClass = equipmentItemClass;
                fsAppointmentDetRow.BillingRule = fsxServicerow?.BillingRule;
            }
        }

        #endregion

        /// <summary>
        /// Creates note record in Note table in the RowInserted event.
        /// </summary>
        public static void InitializeNote(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (string.IsNullOrEmpty(cache.Graph.PrimaryView))
            {
                return;
            }

            var noteCache = cache.Graph.Caches[typeof(Note)];
            var oldDirty = noteCache.IsDirty;
            PXNoteAttribute.GetNoteID(cache, e.Row, EntityHelper.GetNoteField(cache.Graph.Views[cache.Graph.PrimaryView].Cache.GetItemType()));
            noteCache.IsDirty = oldDirty;
        }

        public static int? GetCurrentEmployeeID(PXCache cache)
        {
            EPEmployee epEmployeeRow = EmployeeMaint.GetCurrentEmployee(cache.Graph);
            if (epEmployeeRow == null)
            {
                return null;
            }

            FSxEPEmployee fsxepEmployeeRow = PXCache<EPEmployee>.GetExtension<FSxEPEmployee>(epEmployeeRow);

            if (fsxepEmployeeRow.SDEnabled == true && epEmployeeRow.Status != BAccount.status.Inactive)
            {
                return epEmployeeRow.BAccountID;
            }

            return null;
        }

        /// <summary>
        /// Compares Time parts of DateTime variables without seconds or milliseconds.
        /// </summary>
        public static bool AreTimePartsEqual(DateTime? firstDateTime, DateTime? secondDateTime)
        {
            if (firstDateTime == null || secondDateTime == null)
            {
                return false;
            }

            int time1 = firstDateTime.Value.Hour * 60 + firstDateTime.Value.Minute;
            int time2 = secondDateTime.Value.Hour * 60 + secondDateTime.Value.Minute;

            return time1 == time2;
        }

        public static bool IsAppointmentNotStarted(FSAppointment fsAppointmentRow)
        {
            return fsAppointmentRow != null
                        && (fsAppointmentRow.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                                || fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED);
        }

        public static bool IsServiceOrderClosed(FSServiceOrder fsServiceOrderRow)
        {
            return fsServiceOrderRow != null
                        && fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED;
        }

        public static DateTime? GetNextExecution(PXCache cache, FSSchedule fsScheduleRow)
        {
            bool expired = false;
            var generator = new TimeSlotGenerator();
            List<Scheduler.Schedule> mapScheduleResults = new List<Scheduler.Schedule>();

            mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, fsScheduleRow.LastGeneratedElementDate, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT);
            return generator.GenerateNextOccurrence(mapScheduleResults, fsScheduleRow.LastGeneratedElementDate ?? fsScheduleRow.StartDate.Value, fsScheduleRow.EndDate, out expired);
        }

        public static DateTime? GetContractPeriodEndDate(FSServiceContract fsServiceContractRow, DateTime? lastGeneratedElementDate)
        {
            bool expired = false;
            var generator = new TimeSlotGenerator();
            List<Scheduler.Schedule> mapScheduleResults = new List<Scheduler.Schedule>();

            mapScheduleResults = MapFSServiceContractToSchedule.convertFSServiceContractToSchedule(fsServiceContractRow, lastGeneratedElementDate);

            DateTime? endate = generator.GenerateNextOccurrence(mapScheduleResults, lastGeneratedElementDate ?? fsServiceContractRow.StartDate.Value, null, out expired);

            if (fsServiceContractRow.BillingPeriod != ID.Contract_BillingPeriod.WEEK)
            {
                endate = endate?.AddDays(-1);
            }

            if (fsServiceContractRow.ExpirationType == ID.Contract_ExpirationType.EXPIRING
                && fsServiceContractRow.EndDate != null
                    && fsServiceContractRow.EndDate < endate)
            {
                return null;
            }

            return endate;
        }

        public static bool? IsDisableFixScheduleAction(PXGraph graph)
        {
            FSSetup fsSetupRow = PXSelect<FSSetup>.Select(graph);

            return fsSetupRow.DisableFixScheduleAction;
        }

        public static string WarnUserWithSchedulesWithoutNextExecution(PXGraph graph, PXCache cache, PXAction fixButton, out bool warning)
        {
            PXGraph tempGraph = new PXGraph();
            warning = false;

            string warningMessage = "";

            if (IsDisableFixScheduleAction(graph) == false)
            {
                int count = SchedulesWithoutNextExecution(tempGraph);

                if (count > 0)
                {
                    warning = true;
                    fixButton.SetVisible(true);
                    warningMessage = TX.Warning.SCHEDULES_WITHOUT_NEXT_EXECUTION;
                }
            }

            return warningMessage;
        }

        public static int SchedulesWithoutNextExecution(PXGraph tempGraph)
        {
            return PXSelectReadonly<FSSchedule,
                   Where<
                       FSSchedule.nextExecutionDate, IsNull,
                       And<FSSchedule.active, Equal<True>>>>
                   .Select(tempGraph).Count;
        }

        public static void UpdateSchedulesWithoutNextExecution(PXGraph callerGraph, PXCache cache)
        {
            PXLongOperation.StartOperation(
                callerGraph,
                delegate
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        var resultSet = PXSelectReadonly<FSSchedule,
                                        Where<
                                            FSSchedule.nextExecutionDate, IsNull,
                                            And<FSSchedule.active, Equal<True>>>>
                                        .Select(callerGraph);

                        foreach (FSSchedule fsScheduleRow in resultSet)
                        {
                            PXUpdate<
                                Set<FSSchedule.nextExecutionDate, Required<FSSchedule.nextExecutionDate>>,
                            FSSchedule,
                            Where<
                                FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                            .Update(callerGraph, SharedFunctions.GetNextExecution(cache, fsScheduleRow), fsScheduleRow.ScheduleID);
                        }

                        PXUpdate<
                            Set<FSSetup.disableFixScheduleAction, Required<FSSetup.disableFixScheduleAction>>,
                        FSSetup>
                        .Update(callerGraph, true);

                        ts.Complete();
                    }
                });
        }

        public static bool GetRequireCustomerSignature(PXGraph graph, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow)
        {
            if (fsSrvOrdTypeRow == null || fsServiceOrderRow == null)
            {
                return false;
            }

            Customer customerRow = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>.Select(graph, fsServiceOrderRow.CustomerID);
            FSxCustomer fsxCustomerRow = customerRow != null ? PXCache<Customer>.GetExtension<FSxCustomer>(customerRow) : null;

            return fsSrvOrdTypeRow.RequireCustomerSignature == true
                        || (fsxCustomerRow != null
                            && fsxCustomerRow.RequireCustomerSignature == true);
        }

        public static void ValidateSrvOrdTypeNumberingSequence(PXGraph graph, string srvOrdType)
        {
            FSSrvOrdType fsSrvOrdTypeRow = PXSelect<FSSrvOrdType,
                                           Where<
                                               FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>>
                                           .Select(graph, srvOrdType);

            Numbering numbering = PXSelect<Numbering, Where<Numbering.numberingID, Equal<Required<Numbering.numberingID>>>>.Select(graph, fsSrvOrdTypeRow?.SrvOrdNumberingID);

            if (numbering == null)
            {
                throw new PXSetPropertyException(PX.Objects.CS.Messages.NumberingIDNull);
            }

            if (numbering.UserNumbering == true)
            {
                throw new PXSetPropertyException(TX.Error.SERVICE_ORDER_TYPE_DOES_NOT_ALLOW_AUTONUMBERING, srvOrdType);
            }
        }

        public static bool GetEnableSeasonSetting(PXGraph graph, FSSchedule fsScheduleRow, FSSetup fsSetupRow = null)
        {
            bool enableSeasons = false;

            if (fsScheduleRow is FSRouteContractSchedule)
            {
                FSRouteSetup fsRouteSetupRow = ServiceManagementSetup.GetServiceManagementRouteSetup(graph);

                if (fsRouteSetupRow != null)
                {
                    enableSeasons = fsRouteSetupRow.EnableSeasonScheduleContract == true;
                }
            }
            else
            {
                if (fsSetupRow == null)
                {
                    fsSetupRow = ServiceManagementSetup.GetServiceManagementSetup(graph);
                }

                if (fsSetupRow != null)
                {
                    enableSeasons = fsSetupRow.EnableSeasonScheduleContract == true;
                }
            }

            return enableSeasons;
        }

        public static SharedClasses.SubAccountIDTupla GetSubAccountIDs(PXGraph graph, FSSrvOrdType fsSrvOrdTypeRow, int? inventoryID, int? branchID, int? locationID, int? branchLocationID, int? salesPersonID, bool isService = true)
        {
            INSite inSite = null;
            INPostClass inPostClassRow = null;
            SalesPerson salesPersonRow = null;

            InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(graph, inventoryID);

            Location companyLocationRow = PXSelectJoin<Location,
                                          InnerJoin<BAccountR,
                                          On<
                                              Location.bAccountID, Equal<BAccountR.bAccountID>,
                                              And<Location.locationID, Equal<BAccountR.defLocationID>>>,
                                          InnerJoin<GL.Branch,
                                          On<
                                              BAccountR.bAccountID, Equal<GL.Branch.bAccountID>>>>,
                                          Where<
                                              GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>>>
                                          .Select(graph, branchID);

            Location customerLocationRow = PXSelect<Location,
                                           Where<
                                               Location.locationID, Equal<Required<Location.locationID>>>>
                                           .Select(graph, locationID);

            FSBranchLocation fsBranchLocationRow = PXSelect<FSBranchLocation,
                                                   Where<
                                                       FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                                   .Select(graph, branchLocationID);

            int? inSiteID = isService ? fsBranchLocationRow?.DfltSiteID : inventoryItemRow?.DfltSiteID;

            inSite = PXSelect<INSite,
                     Where<
                         INSite.siteID, Equal<Required<INSite.siteID>>>>
                     .Select(graph, inSiteID);

            inPostClassRow = PXSelect<INPostClass,
                             Where<
                                 INPostClass.postClassID, Equal<Required<INPostClass.postClassID>>>>
                             .Select(graph, inventoryItemRow?.PostClassID);

            salesPersonRow = PXSelect<SalesPerson,
                             Where<
                                 SalesPerson.salesPersonID, Equal<Required<SalesPerson.salesPersonID>>>>
                             .Select(graph, salesPersonID);

            if (customerLocationRow == null
                    || inventoryItemRow == null
                        || fsSrvOrdTypeRow == null
                            || companyLocationRow == null
                                || fsBranchLocationRow == null)
            {
                return null;
            }
            else
            {
                int? branchLocation_SubID = fsBranchLocationRow?.SubID;
                int? branch_SubID = companyLocationRow?.CMPSalesSubID;
                int? inventoryItem_SubID = inventoryItemRow?.SalesSubID;
                int? customerLocation_SubID = customerLocationRow?.CSalesSubID;
                int? postingClass_SubID = inPostClassRow?.SalesSubID;
                int? salesPerson_SubID = salesPersonRow?.SalesSubID;
                int? srvOrdType_SubID = fsSrvOrdTypeRow?.SubID;
                int? warehouse_SubID = inSite?.SalesSubID;

                return new SharedClasses.SubAccountIDTupla(branchLocation_SubID, branch_SubID, inventoryItem_SubID, customerLocation_SubID, postingClass_SubID, salesPerson_SubID, srvOrdType_SubID, warehouse_SubID);
            }
        }

        public static bool ConcatenateNote(PXCache srcCache, PXCache dstCache, object srcObj, object dstObj)
        {
            string dstNote = PXNoteAttribute.GetNote(dstCache, dstObj);

            if (dstNote != string.Empty && dstNote != null)
            {
                string srcNote = PXNoteAttribute.GetNote(srcCache, srcObj);
                dstNote += System.Environment.NewLine + System.Environment.NewLine + srcNote;
                PXNoteAttribute.SetNote(dstCache, dstObj, dstNote);

                return false;
            }

            return true;
        }

        public static void CopyNotesAndFiles(PXCache srcCache, PXCache dstCache, object srcObj, object dstObj, bool? copyNotes, bool? copyFiles)
        {
            if (copyNotes == true)
            {
                copyNotes = ConcatenateNote(srcCache, dstCache, srcObj, dstObj);
            }

            PXNoteAttribute.CopyNoteAndFiles(srcCache, srcObj, dstCache, dstObj, copyNotes: copyNotes, copyFiles: copyFiles);
        }

        public static void CopyNotesAndFiles(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow, object document, int? customerID, int? locationID)
        {
            bool alreadyAssignedNotesAndAttachments = false;

            if (fsSrvOrdTypeRow.CopyNotesFromCustomer == true
                    || fsSrvOrdTypeRow.CopyAttachmentsFromCustomer == true)
            {
                CustomerMaint customerMaintGraph = PXGraph.CreateInstance<CustomerMaint>();
                customerMaintGraph.BAccount.Current = customerMaintGraph.BAccount.Search<Customer.bAccountID>(customerID);

                CopyNotesAndFiles(customerMaintGraph.BAccount.Cache,
                                  cache,
                                  customerMaintGraph.BAccount.Current,
                                  document,
                                  fsSrvOrdTypeRow.CopyNotesFromCustomer,
                                  fsSrvOrdTypeRow.CopyAttachmentsFromCustomer);

                alreadyAssignedNotesAndAttachments = true;
            }

            if (fsSrvOrdTypeRow.CopyNotesFromCustomerLocation == true
                || fsSrvOrdTypeRow.CopyAttachmentsFromCustomerLocation == true)
            {
                CustomerLocationMaint customerLocationMaintGraph = PXGraph.CreateInstance<CustomerLocationMaint>();
                Customer customerRow = GetCustomerRow(cache.Graph, customerID);
                customerLocationMaintGraph.Location.Current = customerLocationMaintGraph.Location.Search<Location.locationID>
                                                        (locationID, customerRow.AcctCD);

                CopyNotesAndFiles(customerLocationMaintGraph.Location.Cache,
                                  cache,
                                  customerLocationMaintGraph.Location.Current,
                                  document,
                                  fsSrvOrdTypeRow.CopyNotesFromCustomerLocation,
                                  fsSrvOrdTypeRow.CopyAttachmentsFromCustomerLocation);

                alreadyAssignedNotesAndAttachments = true;
            }

            if (document.GetType() == typeof(FSAppointment)
                && (fsSrvOrdTypeRow.CopyNotesToAppoinment == true
                    || fsSrvOrdTypeRow.CopyAttachmentsToAppoinment == true))
            {
                FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                                   Where<
                                                       FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                                   .Select(cache.Graph, ((FSAppointment)document).SOID);

                string note = null;

                if (alreadyAssignedNotesAndAttachments == true)
                {
                    note = PXNoteAttribute.GetNote(cache, document);
                }

                bool needCopyNotes = (note == string.Empty || note == null) && fsSrvOrdTypeRow.CopyNotesToAppoinment.Value;

                CopyNotesAndFiles(new PXCache<FSServiceOrder>(cache.Graph),
                                  cache,
                                  fsServiceOrderRow,
                                  document,
                                  needCopyNotes,
                                  fsSrvOrdTypeRow.CopyAttachmentsToAppoinment);
            }
        }

        public static void CopyNotesAndFiles(PXCache dstCache, object lineDocument, IDocLine srcLineDocument, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsSrvOrdTypeRow.CopyLineNotesToInvoice == true
                || fsSrvOrdTypeRow.CopyLineAttachmentsToInvoice == true)
            {
                if (srcLineDocument.SourceTable == ID.TablePostSource.FSSO_DET)
                {
                    PXCache<FSSODet> cacheFSSODet = new PXCache<FSSODet>(dstCache.Graph);
                    CopyNotesAndFiles(cacheFSSODet, dstCache, srcLineDocument, lineDocument, fsSrvOrdTypeRow.CopyLineNotesToInvoice, fsSrvOrdTypeRow.CopyLineAttachmentsToInvoice);
                }
                else
                {
                    PXCache<FSAppointmentDet> cacheApp = new PXCache<FSAppointmentDet>(dstCache.Graph);
                    CopyNotesAndFiles(cacheApp, dstCache, srcLineDocument, lineDocument, fsSrvOrdTypeRow.CopyLineNotesToInvoice, fsSrvOrdTypeRow.CopyLineAttachmentsToInvoice);
                }
            }
        }

        public static PXResultset<FSPostBatch> GetPostBachByProcessID(PXGraph graph, Guid currentProcessID)
        {
            return (PXResultset<FSPostBatch>)PXSelectJoinGroupBy<FSPostBatch,
                                             InnerJoin<FSPostDoc,
                                             On<
                                                 FSPostDoc.batchID, Equal<FSPostBatch.batchID>>>,
                                             Where<
                                                 FSPostDoc.processID, Equal<Required<FSPostDoc.processID>>>,
                                             Aggregate<
                                                 GroupBy<FSPostBatch.batchID>>>
                                             .Select(graph, currentProcessID);
        }

        public static string GetLineTypeFromInventoryItem(InventoryItem inventoryItemRow)
        {
            return inventoryItemRow.StkItem == true ? ID.LineType_ALL.INVENTORY_ITEM :
                            inventoryItemRow.ItemType == INItemTypes.ServiceItem ? ID.LineType_ALL.SERVICE : ID.LineType_ALL.NONSTOCKITEM;
        }

        public static void UpdateFSSODetReferences(PXGraph graph, PXCache soCache, POOrder poOrderRow, List<POLine> poLineUpdatedList)
        {
            try
            {
                foreach (POLine poLineRow in poLineUpdatedList)
                {
                    PXUpdate<
                        Set<FSSODet.poCompleted, Required<FSSODet.poCompleted>>,
                    FSSODet,
                    Where<
                        FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>,
                        And<FSSODet.poType, Equal<Required<FSSODet.poType>>,
                        And<FSSODet.poLineNbr, Equal<Required<FSSODet.poLineNbr>>>>>>
                    .Update(graph, poLineRow.Completed, poLineRow.OrderNbr, poLineRow.OrderType, poLineRow.LineNbr);
                }

                PXResultset<FSServiceOrder> results = PXSelectJoinGroupBy<FSServiceOrder,
                                                      InnerJoin<FSSODet,
                                                      On<
                                                          FSSODet.srvOrdType, Equal<FSServiceOrder.srvOrdType>,
                                                          And<FSSODet.refNbr, Equal<FSServiceOrder.refNbr>>>>,
                                                      Where<
                                                          FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>,
                                                          And<FSSODet.poType, Equal<Required<FSSODet.poType>>>>,
                                                      Aggregate<
                                                          GroupBy<FSServiceOrder.sOID>>>
                                                      .Select(graph, poOrderRow.OrderNbr, poOrderRow.OrderType);

                foreach (FSServiceOrder fsServiceOrderRow in results)
                {
                    PXUpdate<
                        Set<FSSODet.poStatus, Required<FSSODet.poStatus>>,
                    FSSODet,
                    Where<
                        FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
                        And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                        And<FSSODet.poNbr, Equal<Required<FSSODet.poNbr>>,
                        And<FSSODet.poType, Equal<Required<FSSODet.poType>>>>>>>
                    .Update(graph, poOrderRow.Status, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr, poOrderRow.OrderNbr, poOrderRow.OrderType);

                    bool isServiceOrderWaitingForParts = false;

                    foreach (FSSODet fsSODetRow in PXSelect<FSSODet,
                                                   Where<
                                                       FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
                                                       And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                                                       And<FSSODet.enablePO, Equal<True>>>>>
                                                   .Select(graph, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr))
                    {
                        isServiceOrderWaitingForParts = IsFSSODetWaitingForPart(fsSODetRow, poLineUpdatedList);

                        if (isServiceOrderWaitingForParts == true)
                        {
                            break;
                        }
                    }

                    fsServiceOrderRow.WaitingForParts = isServiceOrderWaitingForParts;

                    soCache.Update(fsServiceOrderRow);

                    if (isServiceOrderWaitingForParts == false)
                    {
                        PXUpdate<
                            Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                        FSAppointment,
                        Where<
                            FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                        .Update(graph, 0, fsServiceOrderRow.SOID);
                    }
                    else
                    {
                        PXResultset<FSAppointment> appointmentRows = PXSelect<FSAppointment,
                                                                     Where<
                                                                         FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                                                                     .Select(graph, fsServiceOrderRow.SOID);

                        foreach (FSAppointment fsAppointmentRow in appointmentRows)
                        {
                            bool isAppointmentWaitingForParts = false;

                            foreach (PXResult<FSAppointmentDet, FSSODet> result in PXSelectJoin<FSAppointmentDet,
                                                                                   InnerJoin<FSSODet,
                                                                                   On<
                                                                                       FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>,
                                                                                   Where<
                                                                                       FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>,
                                                                                       And<FSSODet.srvOrdType, Equal<Required<FSSODet.srvOrdType>>,
                                                                                       And<FSSODet.refNbr, Equal<Required<FSSODet.refNbr>>,
                                                                                       And<FSSODet.enablePO, Equal<True>>>>>>
                                                                                   .Select(graph, fsAppointmentRow.AppointmentID, fsServiceOrderRow.SrvOrdType, fsServiceOrderRow.RefNbr))
                            {
                                FSSODet fsSODetRow = (FSSODet)result;

                                isAppointmentWaitingForParts = IsFSSODetWaitingForPart(fsSODetRow, poLineUpdatedList);

                                if (isAppointmentWaitingForParts == true)
                                {
                                    break;
                                }
                            }

                            PXUpdate<
                                Set<FSAppointment.waitingForParts, Required<FSAppointment.waitingForParts>>,
                            FSAppointment,
                            Where<
                                FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                            .Update(graph, (isAppointmentWaitingForParts ? 1 : 0), fsAppointmentRow.AppointmentID);
                        }

                    }
                }

                graph.SelectTimeStamp();
            }
            catch (Exception exception)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.UPDATING_FSSODET_PO_REFERENCES) + exception.Message);
            }
        }

        public static POLine ConvertToPOLine(POLineUOpen row)
        {
            return new POLine
            {
                OrderType = row.OrderType,
                OrderNbr = row.OrderNbr,
                LineNbr = row.LineNbr,
                CuryInfoID = row.CuryInfoID,
                Completed = row.Completed
            };

        }

        public static bool IsFSSODetWaitingForPart(FSSODet fsSODetRow, List<POLine> poLineUpdatedList)
        {
            bool isWaitingForPart = false;
            bool hasBeenUpdated = false;

            if (fsSODetRow.POCompleted == false)
            {
                foreach (POLine poLineRow in poLineUpdatedList)
                {
                    if (fsSODetRow.POLineNbr == poLineRow.LineNbr
                            && fsSODetRow.PONbr == poLineRow.OrderNbr
                            && fsSODetRow.POType == poLineRow.OrderType
                            && poLineRow.Completed == false)
                    {
                        isWaitingForPart = true;
                        hasBeenUpdated = true;
                        break;
                    }
                }

                if(hasBeenUpdated == false)
                {
                    isWaitingForPart = true;
                }

                hasBeenUpdated = false;
            }
            else
            {
                foreach (POLine poLineRow in poLineUpdatedList)
                {
                    if (fsSODetRow.POLineNbr == poLineRow.LineNbr
                            && fsSODetRow.PONbr == poLineRow.OrderNbr
                            && fsSODetRow.POType == poLineRow.OrderType
                            && poLineRow.Completed == false)
                    {
                        isWaitingForPart = true;
                        break;
                    }
                }
            }

            return isWaitingForPart;
        }

        public static void ServiceContractDynamicDropdown(PXCache cache, FSServiceContract fsServiceContractRow)
        {
            if(fsServiceContractRow != null)
            {
                switch (fsServiceContractRow.RecordType)
                {
                    case ID.RecordType_ServiceContract.SERVICE_CONTRACT:

                        if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
                        {
                            PXStringListAttribute.SetList<FSServiceContract.scheduleGenType>(cache, fsServiceContractRow, new Tuple<string, string>[]
                            {
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.SERVICE_ORDER, TX.ScheduleGenType_ServiceContract.SERVICE_ORDER),
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.APPOINTMENT, TX.ScheduleGenType_ServiceContract.APPOINTMENT),
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.NONE, TX.ScheduleGenType_ServiceContract.NONE)
                            });
                        }
                        else
                        {
                            PXStringListAttribute.SetList<FSServiceContract.scheduleGenType>(cache, fsServiceContractRow, new Tuple<string, string>[]
                            {
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.SERVICE_ORDER, TX.ScheduleGenType_ServiceContract.SERVICE_ORDER),
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.APPOINTMENT, TX.ScheduleGenType_ServiceContract.APPOINTMENT)
                            });
                        }
                        break;

                    case ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT:

                        if (fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS)
                        {
                            PXStringListAttribute.SetList<FSServiceContract.scheduleGenType>(cache, fsServiceContractRow, new Tuple<string, string>[]
                            {
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.APPOINTMENT, TX.ScheduleGenType_ServiceContract.APPOINTMENT),
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.NONE, TX.ScheduleGenType_ServiceContract.NONE)
                            });
                        }
                        else
                        {
                            PXStringListAttribute.SetList<FSServiceContract.scheduleGenType>(cache, fsServiceContractRow, new Tuple<string, string>[]
                            {
                                new Tuple<string, string> (ID.ScheduleGenType_ServiceContract.APPOINTMENT, TX.ScheduleGenType_ServiceContract.APPOINTMENT)
                            });
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        public static void DefaultGenerationType(PXCache cache, FSServiceContract fsServiceContractRow, PXFieldDefaultingEventArgs e)
        {
            if (fsServiceContractRow != null)
            {
                if (fsServiceContractRow.RecordType != null)
                {
                    switch (fsServiceContractRow.RecordType)
                    {
                        case ID.RecordType_ServiceContract.SERVICE_CONTRACT:
                            e.NewValue = ID.ScheduleGenType_ServiceContract.SERVICE_ORDER;
                            e.Cancel = true;
                            break;

                        case ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT:
                            e.NewValue = ID.ScheduleGenType_ServiceContract.APPOINTMENT;
                            e.Cancel = true;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        internal static int? GetEquipmentComponentID(PXGraph graph, int? smEquipmentID, int? equipmentLineNbr)
        {
            FSEquipmentComponent FSEquipmentComponentRow = PXSelect<FSEquipmentComponent,
                                                           Where<
                                                               FSEquipmentComponent.SMequipmentID, Equal<Required<FSEquipmentComponent.SMequipmentID>>,
                                                               And<FSEquipmentComponent.lineNbr, Equal<Required<FSEquipmentComponent.lineNbr>>>>>
                                                           .Select(graph, smEquipmentID, equipmentLineNbr);

            if (FSEquipmentComponentRow == null)
            {
                return null;
            }
            else
            {
                return FSEquipmentComponentRow.ComponentID;
            }
        }

        public static void SetVisibleEnableProjectTask<projectTaskField>(PXCache cache, object row, int? projectID)
            where projectTaskField : IBqlField
        {
            bool nonProject = ProjectDefaultAttribute.IsNonProject(projectID);
            PXUIFieldAttribute.SetVisible<projectTaskField>(cache, row, !nonProject);
            PXUIFieldAttribute.SetEnabled<projectTaskField>(cache, row, !nonProject);
            PXUIFieldAttribute.SetRequired<projectTaskField>(cache, !nonProject);
            PXDefaultAttribute.SetPersistingCheck<projectTaskField>(cache, row, !nonProject ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        public static void SetEnableCostCodeProjectTask<projectTaskField, costCodeField>(PXCache cache, object row, string lineType, int? projectID)
            where projectTaskField : IBqlField
            where costCodeField : IBqlField
        {
            bool nonProject = ProjectDefaultAttribute.IsNonProject(projectID);

            bool enableField = lineType != ID.LineType_ServiceContract.COMMENT
                                && lineType != ID.LineType_ServiceContract.INSTRUCTION;

            PXUIFieldAttribute.SetEnabled<projectTaskField>(cache, row, !nonProject && enableField);
            PXUIFieldAttribute.SetEnabled<costCodeField>(cache, row, projectID != null && enableField);

            PXUIFieldAttribute.SetRequired<projectTaskField>(cache, !nonProject && enableField);
            PXDefaultAttribute.SetPersistingCheck<projectTaskField>(cache, row, !nonProject && enableField ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
        }

        public static Dictionary<string, string> GetCalendarMessages()
        {
            var dict = new Dictionary<string, string>();

            foreach (var prop in typeof(TX.CalendarMessages).GetFields())
            {
                dict[prop.Name] = PXLocalizer.Localize(prop.GetValue(null).ToString(), typeof(TX.CalendarMessages).FullName);
            }

            int index = 0;
            string invariantName;

            foreach (string dayName in DateTimeFormatInfo.CurrentInfo.DayNames)
            {
                invariantName = DateTimeFormatInfo.InvariantInfo.DayNames[index];
                dict[invariantName] = dayName;
                index++;
            }

            index = 0;
            foreach (string abbrDayName in DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames)
            {
                invariantName = "abbr_" + DateTimeFormatInfo.InvariantInfo.DayNames[index];
                dict[invariantName] = abbrDayName;
                index++;
            }

            index = 0;
            foreach (string monthName in DateTimeFormatInfo.CurrentInfo.MonthNames)
            {
                invariantName = DateTimeFormatInfo.InvariantInfo.MonthNames[index];
                dict[invariantName] = monthName;
                index++;
            }

            index = 0;
            foreach (string abbrMonthName in DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames)
            {
                invariantName = "abbr_" + DateTimeFormatInfo.InvariantInfo.MonthNames[index];
                dict[invariantName] = abbrMonthName;
                index++;
            }

            return dict;
        }

        public static void ValidatePostToByFeatures<postToField>(PXCache cache, object row, string postTo)
            where postToField : IBqlField
        {
            PXCache<FeaturesSet> featureSetCache = new PXCache<FeaturesSet>(cache.Graph);

            if (postTo == ID.Contract_PostTo.SALES_ORDER_MODULE
                && PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() == false)
            {
                cache.RaiseExceptionHandling<postToField>(
                    row,
                    postTo,
                    new PXSetPropertyException(PXMessages.LocalizeFormat(
                        TX.Error.POST_TO_MISSING_FEATURES,
                        TX.Contract_PostTo.SALES_ORDER_MODULE,
                        PXUIFieldAttribute.GetDisplayName<FeaturesSet.distributionModule>(featureSetCache)),
                    PXErrorLevel.Error));
            }
            else if (postTo == ID.Contract_PostTo.SALES_ORDER_INVOICE
                        && PXAccess.FeatureInstalled<FeaturesSet.advancedSOInvoices>() == false)
            {
                cache.RaiseExceptionHandling<postToField>(
                    row,
                    postTo,
                    new PXSetPropertyException(PXMessages.LocalizeFormat(
                        TX.Error.POST_TO_MISSING_FEATURES,
                        TX.Contract_PostTo.SALES_ORDER_INVOICE,
                        PXUIFieldAttribute.GetDisplayName<FeaturesSet.advancedSOInvoices>(featureSetCache)),
                    PXErrorLevel.Error));
            }
        }

        public static void ValidateDuplicateLineNbr(PXSelectBase<FSSODet> srvOrdDetails, PXSelectBase<FSAppointmentDet> apptDetails)
        {
            var lineNbrs = new List<int?>();

            if (srvOrdDetails != null)
            {
                ValidateDuplicateLineNbr<FSSODet>(lineNbrs,
                                                  srvOrdDetails.Select().ToList().Select(e => (FSSODet)e).ToList(),
                                                  srvOrdDetails.Cache);
            }

            if (apptDetails != null)
            {
                ValidateDuplicateLineNbr<FSAppointmentDet>(lineNbrs,
                                                           apptDetails.Select().ToList().Select(e => (FSAppointmentDet)e).ToList(),
                                                           apptDetails.Cache);
            }
        }

        public static void ValidateDuplicateLineNbr<DetailType>(List<int?> lineNbrs, List<DetailType> list, PXCache cache)
            where DetailType : IBqlTable, IFSSODetBase
        {
            foreach (DetailType row in list)
            {
                if (lineNbrs.Find(lineNbr => lineNbr == row.LineNbr) == default(int?))
                {
                    lineNbrs.Add(row.LineNbr);
                }
                else
                {
                    PXFieldState state = cache.GetValueExt<FSSODet.lineNbr>(row) as PXFieldState;
                    cache.RaiseExceptionHandling<FSSODet.lineNbr>(
                        row, state != null ? state.Value : cache.GetValue<FSSODet.lineNbr>(row),
                        new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded));

                    throw new PXRowPersistingException(typeof(FSSODet.lineNbr).Name, null, ErrorMessages.DuplicateEntryAdded);
                }
            }
        }

        public static string GetLineType(string lineType, bool lower = false)
        {
            string lineTypeTX = "";

            switch (lineType)
            {
                case ID.LineType_ALL.SERVICE: lineTypeTX = TX.LineType_ALL.SERVICE; break;
                case ID.LineType_ALL.NONSTOCKITEM: lineTypeTX = TX.LineType_ALL.NONSTOCKITEM; break;
                case ID.LineType_ALL.INVENTORY_ITEM: lineTypeTX = TX.LineType_ALL.INVENTORY_ITEM; break;
                case ID.LineType_ALL.PICKUP_DELIVERY: lineTypeTX = TX.LineType_ALL.PICKUP_DELIVERY; break;
                case ID.LineType_ALL.COMMENT: lineTypeTX = TX.LineType_ALL.COMMENT; break;
                case ID.LineType_ALL.INSTRUCTION: lineTypeTX = TX.LineType_ALL.INSTRUCTION; break;
            }

            if (lower)
            {
                lineTypeTX.ToLower();
            }

            return lineTypeTX;
        }

        public static string GetItemLineRef(PXGraph graph, int? appointmentID, bool isTravel = false)
        {
            var appoinmentDetRow = (FSAppointmentDet)
                                   PXSelectJoin<FSAppointmentDet,
                                   InnerJoin<InventoryItem,
                                   On<
                                        InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>>,
                                   Where<
                                       FSxService.isTravelItem, Equal<Required<FSxService.isTravelItem>>,
                                   And<
                                       FSAppointmentDet.appointmentID, Equal<Required<FSAppointment.appointmentID>>,
                                       And<
                                           Where<FSAppointmentDet.status, Equal<FSAppointmentDet.status.NotStarted>,
                                           Or<FSAppointmentDet.status, Equal<FSAppointmentDet.status.InProcess>>>>>>>
                                   .Select(graph, isTravel, appointmentID);

            return appoinmentDetRow != null && appoinmentDetRow.LineType == ID.LineType_ALL.SERVICE ? appoinmentDetRow.LineRef : null;
        }

        public static void DisableDetailFieldsByCancelledNotPerformed<Field>(PXCache cache, object row)
            where Field : class, IBqlField
        {
            foreach (string field in cache.Fields)
            {
                if (field.ToLower().Equals(typeof(Field).Name.ToLower()) == false)
                {
                    PXUIFieldAttribute.SetEnabled(cache, row, field, false);
                }
            }
        }
    }
}