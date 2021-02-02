using PX.Data;
using PX.Objects.CR;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PX.Objects.FS
{
    public class RouteMaint : PXGraph<RouteMaint, FSRoute>
    {
        public RouteMaint()
            : base()
        {
            // We are using the FieldUpdating event because we are only using the [TIME] part of the DateTime.
            // the value of the field is sent to the server in the e.NewValue only when you edit the time.
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnMonday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnMonday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnTuesday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnTuesday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnWednesday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnWednesday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnThursday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnThursday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnFriday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnFriday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnSaturday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnSaturday__Time_FieldUpdating);
            FieldUpdating.AddHandler(typeof(FSRoute),
                                     typeof(FSRoute.beginTimeOnSunday).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRoute_BeginTimeOnSunday__Time_FieldUpdating);
        }

        #region Selects
        // Baccount workaround
        [PXHidden]
        public PXSelect<BAccount> BAccount;

        public PXSelect<FSRoute> RouteRecords;

        public PXSelect<FSRoute, Where<FSRoute.routeID, Equal<Current<FSRoute.routeID>>>> RouteSelected; 
        
        public PXSelectJoin<FSRouteEmployee,
               InnerJoin<BAccount,
               On<
                   FSRouteEmployee.employeeID, Equal<BAccount.bAccountID>>>,
               Where<
                   FSRouteEmployee.routeID, Equal<Current<FSRoute.routeID>>>> RouteEmployeeRecords;

        public PXFilter<WeekCodeFilter> WeekCodeFilter;
        
        public PXSelectReadonly<FSWeekCodeDate> WeekCodeDateRecords;  

        [PXViewName(CR.Messages.Attributes)]
        public CSAttributeGroupList<FSRoute, FSRouteDocument> Mapping;

        #endregion

        #region Delegates
        public virtual IEnumerable weekCodeDateRecords()
        {
            FSRoute fsRouteRow = RouteRecords.Current;
            List<object> returnList = new List<object>();
            List<object> weekCodeArgs = new List<object>();
            List<int> dayOfWeekDays = new List<int>();

            BqlCommand commandFilter = new Select<FSWeekCodeDate>();

            Regex rgxP1 = new Regex(@"^[1-4]$");
            Regex rgxP2 = new Regex(@"^[a-bA-B]$");
            Regex rgxP3 = new Regex(@"^[c-fC-F]$");
            Regex rgxP4 = new Regex(@"^[s-zS-Z]$");

            if (fsRouteRow != null && string.IsNullOrEmpty(fsRouteRow.WeekCode) == false)
            {
                List<string> weekcodes = SharedFunctions.SplitWeekcodeByComma(fsRouteRow.WeekCode);

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

                    commandFilter = commandFilter.WhereOr<
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

                WeekCodeFilter filter = WeekCodeFilter.Current;

                if (filter != null)
                {
                    if (filter.DateBegin != null) 
                    {
                        commandFilter = commandFilter.WhereAnd(typeof(Where<FSWeekCodeDate.weekCodeDate, GreaterEqual<Current<WeekCodeFilter.dateBegin>>>));      
                    }

                    if (filter.DateEnd != null)
                    {
                        commandFilter = commandFilter.WhereAnd(typeof(Where<FSWeekCodeDate.weekCodeDate, LessEqual<Current<WeekCodeFilter.dateEnd>>>));
                    }
                }

                if (fsRouteRow.ActiveOnSunday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.SUNDAY);
                }

                if (fsRouteRow.ActiveOnMonday == true) 
                {
                   dayOfWeekDays.Add(ID.WeekDaysNumber.MONDAY);
                }

                if (fsRouteRow.ActiveOnTuesday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.TUESDAY);
                }

                if (fsRouteRow.ActiveOnWednesday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.WEDNESDAY);
                }

                if (fsRouteRow.ActiveOnThursday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.THURSDAY);
                }

                if (fsRouteRow.ActiveOnFriday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.FRIDAY);
                }

                if (fsRouteRow.ActiveOnSaturday == true)
                {
                    dayOfWeekDays.Add(ID.WeekDaysNumber.SATURDAY);
                }

                if (dayOfWeekDays != null && dayOfWeekDays.Count > 0)
                {
                    commandFilter = commandFilter.WhereAnd(InHelper<FSWeekCodeDate.dayOfWeek>.Create(dayOfWeekDays.Count));
                    foreach (int dayOfWeekDay in dayOfWeekDays)
                    {
                        weekCodeArgs.Add(dayOfWeekDay);
                    }
                }

                PXView weekCodeRecordsView = new PXView(this, true, commandFilter);
                var startRow = PXView.StartRow;
                int totalRows = 0;
                var list = weekCodeRecordsView.Select(PXView.Currents,
                                                      weekCodeArgs.ToArray(),
                                                      PXView.Searches,
                                                      PXView.SortColumns,
                                                      PXView.Descendings,
                                                      PXView.Filters,
                                                      ref startRow,
                                                      PXView.MaximumRows,
                                                      ref totalRows);
                PXView.StartRow = 0;
                return list;
            }

            return returnList;
        }
        #endregion

        #region CacheAttached
        #region BAccount_AcctName
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Employee Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region EntityClassID
        [PXDBString(15, IsUnicode = true, IsKey = true, IsFixed = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Entity Class ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void CSAttributeGroup_EntityClassID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Virtual Functions

        /// <summary>
        /// Enables/Disables the document fields depending on several factors.
        /// </summary>
        public virtual void EnableDisableDocument(PXCache cache, FSRoute fsRouteRow)
        {
            PXUIFieldAttribute.SetEnabled<FSRoute.maxAppointmentQty>(cache, fsRouteRow, !(bool)fsRouteRow.NoAppointmentLimit);

            #region ExecutionDays
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnMonday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnMonday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnTuesday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnTuesday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnWednesday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnWednesday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnThursday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnThursday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnFriday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnFriday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnSaturday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnSaturday);
            PXUIFieldAttribute.SetEnabled<FSRoute.beginTimeOnSunday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnSunday);

            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnMonday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnMonday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnTuesday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnTuesday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnWednesday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnWednesday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnThursday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnThursday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnFriday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnFriday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnSaturday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnSaturday);
            PXUIFieldAttribute.SetEnabled<FSRoute.nbrTripOnSunday>(cache, fsRouteRow, (bool)fsRouteRow.ActiveOnSunday);

            CleanInactiveDayFields(fsRouteRow);

            #endregion

            if (!isThereAnyActiveDay(fsRouteRow) && cache.GetStatus(fsRouteRow) != PXEntryStatus.Inserted)
            {
                cache.RaiseExceptionHandling<FSRoute.routeCD>(
                    fsRouteRow,
                    fsRouteRow.RouteCD,
                    new PXSetPropertyException(TX.Warning.NO_EXECUTION_DAYS_SELECTED_FOR_ROUTE, PXErrorLevel.Warning));
            }
            else 
            {
                cache.RaiseExceptionHandling<FSRoute.routeCD>(fsRouteRow, fsRouteRow.RouteCD, null);
            }
        }

        /// <summary>
        /// Checks if any execution day flag is active for a given route.
        /// </summary>
        /// <param name="fsRouteRow">FSRoute row.</param>
        /// <returns>True if at least one flag is on else returns false.</returns>
        public virtual bool isThereAnyActiveDay(FSRoute fsRouteRow)
        {
            return (bool)fsRouteRow.ActiveOnMonday || (bool)fsRouteRow.ActiveOnTuesday || (bool)fsRouteRow.ActiveOnWednesday 
                       || (bool)fsRouteRow.ActiveOnThursday || (bool)fsRouteRow.ActiveOnFriday || (bool)fsRouteRow.ActiveOnSaturday 
                           || (bool)fsRouteRow.ActiveOnSunday;
        }

        /// <summary>
        /// Checks the execution flag for a given day and return the the corresponding PXPersistingCheck to be assigned.
        /// </summary>
        /// <param name="enableForWeekDay">Execution flag for a given day.</param>
        /// <returns>PXPersistingCheck.NullOrBlank if the input is required, PXPersistingCheck.Nothing otherwise.</returns>
        public virtual PXPersistingCheck EnableDisableDayPersistingCheck(bool enableForWeekDay)
        {
            return enableForWeekDay ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing;
        }

        /// <summary>
        /// Checks if the current RouteShort already exists to ensure is unique.
        /// </summary>
        public virtual bool isRouteShortDuplicated(FSRoute fsRouteRow)
        {
            FSRoute fsRouteRow_InDB = PXSelect<FSRoute,
                                      Where<
                                          FSRoute.routeID, NotEqual<Required<FSRoute.routeID>>,
                                          And<FSRoute.routeShort, Equal<Required<FSRoute.routeShort>>>>>
                                      .Select(this, fsRouteRow.RouteID, fsRouteRow.RouteShort);

            return fsRouteRow_InDB != null;
        }

        /// <summary>
        /// Validates if a Week Code is well formatted. (Creates an exception in the cache parameter).
        /// </summary>
        public virtual void ValidateWeekCode(PXCache cache, FSRoute fsRouteRow)
        {
            List<object> weekCodeArgs = new List<object>();
            bool errorOnWeekCode = false;
            Regex rgx = new Regex(@"^[1-4]?[a-bA-B]?[c-fC-F]?[s-zS-Z]?$");

            if (fsRouteRow.WeekCode != null)
            {
                List<string> weekcodes = SharedFunctions.SplitWeekcodeByComma(fsRouteRow.WeekCode);
                foreach (string weekcode in weekcodes)
                {
                    if (string.IsNullOrEmpty(weekcode))
                    {
                        cache.RaiseExceptionHandling<FSRoute.weekCode>(
                            fsRouteRow,
                            fsRouteRow.WeekCode,
                            new PXSetPropertyException(TX.Error.WEEKCODE_MUST_NOT_BE_EMPTY, PXErrorLevel.Error));

                        errorOnWeekCode = true;
                    }
                    else if (SharedFunctions.IsAValidWeekCodeLength(weekcode) == false)
                    {
                        cache.RaiseExceptionHandling<FSRoute.weekCode>(
                            fsRouteRow,
                            fsRouteRow.WeekCode,
                            new PXSetPropertyException(TX.Error.WEEKCODE_LENGTH_MUST_LESS_OR_EQUAL_THAN_4, PXErrorLevel.Error));
                        errorOnWeekCode = true;
                    }
                    else
                    {
                        List<string> charsInWeekCode = SharedFunctions.SplitWeekcodeInChars(weekcode);

                        foreach (string charToCompare in charsInWeekCode)
                        {
                            if (SharedFunctions.IsAValidCharForWeekCode(charToCompare) == false)
                            {
                                cache.RaiseExceptionHandling<FSRoute.weekCode>(
                                    fsRouteRow,
                                    fsRouteRow.WeekCode,
                                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.WEEKCODE_CHAR_NOT_ALLOWED, charToCompare), PXErrorLevel.Error));
                                errorOnWeekCode = true;
                                continue;
                            }
                        }

                        if (rgx.IsMatch(weekcode) == false && errorOnWeekCode == false)
                        {
                            cache.RaiseExceptionHandling<FSRoute.weekCode>(
                                fsRouteRow,
                                fsRouteRow.WeekCode,
                                new PXSetPropertyException(TX.Error.WEEKCODE_BAD_FORMED, PXErrorLevel.Error));
                            errorOnWeekCode = true;
                        }
                    }

                    if (errorOnWeekCode)
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Clean <c>StartTime</c> and <c>Nbr.</c> of Trip(s) per Day fields when Day is set inactive.
        /// </summary>
        public virtual void CleanInactiveDayFields(FSRoute fsRouteRow)
        {
            if (fsRouteRow.ActiveOnMonday == false)
            {
                fsRouteRow.BeginTimeOnMonday = null;
                fsRouteRow.NbrTripOnMonday = null;
            }

            if (fsRouteRow.ActiveOnTuesday == false)
            {
                fsRouteRow.BeginTimeOnTuesday = null;
                fsRouteRow.NbrTripOnTuesday = null;
            }

            if (fsRouteRow.ActiveOnWednesday == false)
            {
                fsRouteRow.BeginTimeOnWednesday = null;
                fsRouteRow.NbrTripOnWednesday = null;
            }

            if (fsRouteRow.ActiveOnThursday == false)
            {
                fsRouteRow.BeginTimeOnThursday = null;
                fsRouteRow.NbrTripOnThursday = null;
            }

            if (fsRouteRow.ActiveOnFriday == false)
            {
                fsRouteRow.BeginTimeOnFriday = null;
                fsRouteRow.NbrTripOnFriday = null;
            }

            if (fsRouteRow.ActiveOnSaturday == false)
            {
                fsRouteRow.BeginTimeOnSaturday = null;
                fsRouteRow.NbrTripOnSaturday = null;
            }

            if (fsRouteRow.ActiveOnSunday == false)
            {
                fsRouteRow.BeginTimeOnSunday = null;
                fsRouteRow.NbrTripOnSunday = null;
            }
        }

        #endregion

        #region Event Handlers

        #region FSRoute

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnMonday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnMonday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnTuesday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnTuesday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnWednesday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnWednesday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnThursday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnThursday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnFriday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnFriday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnSaturday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnSaturday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }

        //Cannot be changed to new event format
        protected virtual void FSRoute_BeginTimeOnSunday__Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.BeginTimeOnSunday = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
        }
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnMonday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnMonday = (bool)fsRouteRow.ActiveOnMonday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnTuesday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnTuesday = (bool)fsRouteRow.ActiveOnTuesday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnWednesday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnWednesday = (bool)fsRouteRow.ActiveOnWednesday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnThursday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnThursday = (bool)fsRouteRow.ActiveOnThursday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnFriday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnFriday = (bool)fsRouteRow.ActiveOnFriday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnSaturday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnSaturday = (bool)fsRouteRow.ActiveOnSaturday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.activeOnSunday> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            fsRouteRow.NbrTripOnSunday = (bool)fsRouteRow.ActiveOnSunday ? 1 : 0;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.noAppointmentLimit> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;

            fsRouteRow.MaxAppointmentQty = (fsRouteRow.NoAppointmentLimit == true) ? 0 : 1;
        }

        protected virtual void _(Events.FieldUpdated<FSRoute, FSRoute.weekCode> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;
            ValidateWeekCode(e.Cache, fsRouteRow);
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSRoute> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;

            EnableDisableDocument(e.Cache, fsRouteRow);

            if ((bool)fsRouteRow.NoAppointmentLimit)
            {
                fsRouteRow.MaxAppointmentQty = 0;
            }
        }

        protected virtual void _(Events.RowInserting<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSRoute> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSRoute> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRoute fsRouteRow = (FSRoute)e.Row;

            if (fsRouteRow.NoAppointmentLimit == false && fsRouteRow.MaxAppointmentQty < 1)
            {
                e.Cache.RaiseExceptionHandling<FSRoute.maxAppointmentQty>(
                    fsRouteRow,
                    null,
                    new PXSetPropertyException(TX.Error.ROUTE_MAX_APPOINTMENT_QTY_GREATER_THAN_ZERO, PXErrorLevel.Error));
            }

            if (isRouteShortDuplicated(fsRouteRow))
            {
                e.Cache.RaiseExceptionHandling<FSRoute.routeShort>(
                    fsRouteRow,
                    null,
                    new PXSetPropertyException(TX.Error.ROUTE_SHORT_NOT_DUPLICATED, PXErrorLevel.Error));
            }

            ValidateWeekCode(e.Cache, fsRouteRow);
        }

        protected virtual void _(Events.RowPersisted<FSRoute> e)
        {
        }

        #endregion

        #region FSRouteEmployee

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        protected virtual void _(Events.FieldUpdated<FSRouteEmployee, FSRouteEmployee.priorityPreference> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteEmployee fsRouteEmployeeRow = (FSRouteEmployee)e.Row;

            if (fsRouteEmployeeRow.PriorityPreference < 1)
            {
                e.Cache.RaiseExceptionHandling<FSRouteEmployee.priorityPreference>(
                    fsRouteEmployeeRow,
                    fsRouteEmployeeRow.PriorityPreference,
                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.MINIMUN_VALUE, 1), PXErrorLevel.Error));
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSRouteEmployee> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSRouteEmployee> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteEmployee fsRouteEmployeeRow = (FSRouteEmployee)e.Row;

            if (fsRouteEmployeeRow.PriorityPreference < 1)
            {
                e.Cache.RaiseExceptionHandling<FSRouteEmployee.priorityPreference>(
                    fsRouteEmployeeRow,
                    fsRouteEmployeeRow.PriorityPreference,
                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.MINIMUN_VALUE, 1), PXErrorLevel.Error));

                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.MINIMUN_VALUE_NAME_FIELD, "Priority Option", 1), PXErrorLevel.Error);
            }
        }

        protected virtual void _(Events.RowPersisted<FSRouteEmployee> e)
        {
        }
        #endregion

        #endregion
    }
}