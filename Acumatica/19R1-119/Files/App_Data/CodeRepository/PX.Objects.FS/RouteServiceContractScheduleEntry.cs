using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;

namespace PX.Objects.FS
{
    public class RouteServiceContractScheduleEntry : ServiceContractScheduleEntryBase<RouteServiceContractScheduleEntry, 
                                                    FSRouteContractSchedule, FSRouteContractSchedule.scheduleID, 
                                                    FSRouteContractSchedule.entityID>, IGraphWithInitialization
    {
        #region Selects
        [PXCopyPasteHiddenFields(typeof(FSScheduleRoute.globalSequence))]
        public PXSelect<FSScheduleRoute,
                    Where<
                        FSScheduleRoute.scheduleID, Equal<Current<FSRouteContractSchedule.scheduleID>>>> ScheduleRoutes;

        public PXFilter<WeekCodeFilter> WeekCodeFilter;

        public PXSelectReadonly<FSWeekCodeDate> WeekCodeDateRecords;

        [PXHidden]
        public PXSelect<FSScheduleDetPart> ScheduleDetPart;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }
        #endregion

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSServiceContract>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSSchedule), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSSchedule.customerID>(((RouteServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.CustomerID),
                                new PXDataFieldValue<FSSchedule.entityID>(((RouteServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.EntityID)
                        };
                    }));

                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSSchedule>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSScheduleDet), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSScheduleDet.scheduleID>(((RouteServiceContractScheduleEntry)graph).ContractScheduleRecords.Current?.ScheduleID)
                        };
                    }));
            }
        }

        #region CacheAttached
        #region FSScheduleDet_ServiceTemplateID
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Service Template ID", Enabled = false)]
        [PXSelector(typeof(Search<FSServiceTemplate.serviceTemplateID,
                            Where<FSServiceTemplate.srvOrdType,
                                Equal<Current<FSSchedule.srvOrdType>>>>),
                SubstituteKey = typeof(FSServiceTemplate.serviceTemplateCD),
                DescriptionField = typeof(FSServiceTemplate.descr))]
        protected virtual void FSScheduleDetService_ServiceTemplateID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteContractSchedule_BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch ID", Enabled = false)]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        protected virtual void FSRouteContractSchedule_BranchID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteContractSchedule_BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Branch Location ID", Enabled = false)]
        [FSSelectorBranchLocationByFSSchedule]
        [PXFormula(typeof(Default<FSSchedule.branchID>))]
        protected virtual void FSRouteContractSchedule_BranchLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteContractSchedule_CreatedByScreenID
        //Needed to manage properly the show/hide actions of the Season settings
        [PXUIField(Visible = false)]
        [PXDBCreatedByScreenID]
        protected virtual void FSRouteContractSchedule_CreatedByScreenID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSScheduleDetPart_InventoryID
        [StockItem]
        protected virtual void FSScheduleDetPart_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions

        #region OpenRouteScheduleProcess
        public PXAction<FSRouteContractSchedule> openRouteScheduleProcess;
        [PXUIField(DisplayName = "Generate Route Appointments", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual void OpenRouteScheduleProcess()
        {
            RouteScheduleProcess routeScheduleProcessGraph = PXGraph.CreateInstance<RouteScheduleProcess>();

            RouteServiceContractFilter filter = new RouteServiceContractFilter();
            filter.ScheduleID = ContractScheduleRecords.Current.ScheduleID;
            filter.FromDate = ContractScheduleRecords.Current.StartDate;
            filter.ToDate = ContractScheduleRecords.Current.EndDate ?? ContractScheduleRecords.Current.StartDate;
            routeScheduleProcessGraph.Filter.Insert(filter);

            throw new PXRedirectRequiredException(routeScheduleProcessGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #endregion

        #region Delegates
        public virtual IEnumerable weekCodeDateRecords()
        {
            FSRouteContractSchedule fsRouteContractScheduleRow = ContractScheduleRecords.Current;
            List<object> returnList = new List<object>();
            List<object> weekCodeArgs = new List<object>();
            List<int> dayOfWeekDays = new List<int>();

            BqlCommand commandFilter = new Select<FSWeekCodeDate>();

            Regex rgxP1 = new Regex(@"^[1-4]$");
            Regex rgxP2 = new Regex(@"^[a-bA-B]$");
            Regex rgxP3 = new Regex(@"^[c-fC-F]$");
            Regex rgxP4 = new Regex(@"^[s-zS-Z]$");

            if (fsRouteContractScheduleRow != null && string.IsNullOrEmpty(fsRouteContractScheduleRow.WeekCode) == false)
            {
                List<string> weekcodes = SharedFunctions.SplitWeekcodeByComma(fsRouteContractScheduleRow.WeekCode);

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

                    commandFilter = commandFilter.WhereOr(typeof(
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
                                                Or<FSWeekCodeDate.weekCodeP4, IsNull>>>>>>>));

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
                    DateTime? dateBegin = filter.DateBegin;
                    DateTime? dateEnd = null;

                    if (filter.DateEnd.HasValue)
                    {
                        dateEnd = filter.DateEnd;
                    }

                    if (dateBegin.HasValue == true && dateEnd.HasValue == false)
                    {
                        dateEnd = filter.DateEnd.HasValue ? filter.DateEnd : dateBegin.Value.AddYears(1);  
                    } 

                    if (dateBegin != null)
                    {
                        commandFilter = commandFilter.WhereAnd(typeof(Where<FSWeekCodeDate.weekCodeDate, GreaterEqual<Required<WeekCodeFilter.dateBegin>>>));
                        weekCodeArgs.Add(dateBegin);
                    }

                    if (dateEnd != null)
                    {
                        commandFilter = commandFilter.WhereAnd(typeof(Where<FSWeekCodeDate.weekCodeDate, LessEqual<Required<WeekCodeFilter.dateEnd>>>));
                        weekCodeArgs.Add(dateEnd);
                    }
                }

                if (fsRouteContractScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY)
                {
                    if (fsRouteContractScheduleRow.WeeklyOnSun == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.SUNDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnMon == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.MONDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnTue == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.TUESDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnWed == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.WEDNESDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnThu == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.THURSDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnFri == true)
                    {
                        dayOfWeekDays.Add(ID.WeekDaysNumber.FRIDAY);
                    }

                    if (fsRouteContractScheduleRow.WeeklyOnSat == true)
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
                }
                
                PXView weekCodeRecordsView = new PXView(this, true, commandFilter);
                return weekCodeRecordsView.SelectMulti(weekCodeArgs.ToArray());
            }
                
            return returnList;
        }

        public virtual IEnumerable scheduleProjectionRecords()
        {
            return Delegate_ScheduleProjectionRecords(ContractScheduleRecords.Cache, ContractScheduleRecords.Current, FromToFilter.Current, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT);
        }
        #endregion

        #region Private Functions

        /// <summary>
        /// Allows to reset the Route field values when the VehicleTypeID selected in the header changes.
        /// </summary>
        private void ResetRouteFields(FSRouteContractSchedule fsScheduleRow)
        {
            if (ScheduleRoutes.Current == null)
            {
                return;
            }

            FSRoute fsRouteRow = PXSelect<FSRoute>
                                     .Select(this);

            if (fsRouteRow == null || ScheduleRoutes.Current.DfltRouteID      != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.dfltRouteID>(ScheduleRoutes.Current,      null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDSunday    != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDSunday>(ScheduleRoutes.Current,    null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDMonday    != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDMonday>(ScheduleRoutes.Current,    null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDTuesday   != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDTuesday>(ScheduleRoutes.Current,   null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDWednesday != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDWednesday>(ScheduleRoutes.Current, null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDThursday  != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDThursday>(ScheduleRoutes.Current,  null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDFriday    != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDFriday>(ScheduleRoutes.Current,    null); }
            if (fsRouteRow == null || ScheduleRoutes.Current.RouteIDSaturday  != fsRouteRow.RouteID) { ScheduleRoutes.Cache.SetValueExt<FSScheduleRoute.routeIDSaturday>(ScheduleRoutes.Current,  null); }
        }

        /// <summary>
        /// Validates if a Week Code is well formatted. (Creates an exception in the cache parameter).
        /// </summary>
        private void ValidateWeekCode(PXCache cache, FSRouteContractSchedule fsRouteContractScheduleRow)
        {            
            List<object> weekCodeArgs = new List<object>();
            bool errorOnWeekCode = false;
            Regex rgx = new Regex(@"^[1-4]?[a-bA-B]?[c-fC-F]?[s-zS-Z]?$");

            if (fsRouteContractScheduleRow.WeekCode != null)
            {
                List<string> weekcodes = SharedFunctions.SplitWeekcodeByComma(fsRouteContractScheduleRow.WeekCode);
                foreach (string weekcode in weekcodes)
                {
                    if (string.IsNullOrEmpty(weekcode))
                    {
                        cache.RaiseExceptionHandling<FSRouteContractSchedule.weekCode>(
                            fsRouteContractScheduleRow,
                            fsRouteContractScheduleRow.WeekCode,
                            new PXSetPropertyException(TX.Error.WEEKCODE_MUST_NOT_BE_EMPTY, PXErrorLevel.Error));

                        errorOnWeekCode = true;
                    }
                    else if (SharedFunctions.IsAValidWeekCodeLength(weekcode) == false)
                    {
                        cache.RaiseExceptionHandling<FSRouteContractSchedule.weekCode>(
                            fsRouteContractScheduleRow,
                            fsRouteContractScheduleRow.WeekCode,
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
                                cache.RaiseExceptionHandling<FSRouteContractSchedule.weekCode>(
                                    fsRouteContractScheduleRow,
                                    fsRouteContractScheduleRow.WeekCode,
                                    new PXSetPropertyException(PXMessages.LocalizeFormatNoPrefix(TX.Error.WEEKCODE_CHAR_NOT_ALLOWED, charToCompare), PXErrorLevel.Error));
                                errorOnWeekCode = true;
                                continue;
                            }
                        }

                        if (rgx.IsMatch(weekcode) == false && errorOnWeekCode == false)
                        {
                            cache.RaiseExceptionHandling<FSRouteContractSchedule.weekCode>(
                                fsRouteContractScheduleRow,
                                fsRouteContractScheduleRow.WeekCode,
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
        /// Verifies if the specified recurrence dates match the Route's definition.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="fsRouteContractScheduleRow">FSRouteContractSchedule object row.</param>
        private void ValidateDays(PXCache cache, FSRouteContractSchedule fsRouteContractScheduleRow)
        {
            FSScheduleRoute fsScheduleRouteRow = ScheduleRoutes.Current;

            if (fsScheduleRouteRow == null || fsScheduleRouteRow.DfltRouteID == null)
            {
                return;
            }

            FSRoute fsRouteRow = PXSelect<FSRoute, 
                                 Where<
                                    FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>
                                 .Select(this, fsScheduleRouteRow.DfltRouteID);

            if (fsRouteRow != null && fsRouteContractScheduleRow.FrequencyType == ID.Schedule_FrequencyType.WEEKLY
                    && ((fsRouteContractScheduleRow.WeeklyOnMon == true  && fsRouteRow.ActiveOnMonday == false)
                        || (fsRouteContractScheduleRow.WeeklyOnThu == true && fsRouteRow.ActiveOnThursday == false)
                           || (fsRouteContractScheduleRow.WeeklyOnWed == true && fsRouteRow.ActiveOnWednesday == false)
                                || (fsRouteContractScheduleRow.WeeklyOnTue == true && fsRouteRow.ActiveOnTuesday == false)
                                    || (fsRouteContractScheduleRow.WeeklyOnFri == true && fsRouteRow.ActiveOnFriday == false)
                                        || (fsRouteContractScheduleRow.WeeklyOnSat == true && fsRouteRow.ActiveOnSaturday == false)
                                           || (fsRouteContractScheduleRow.WeeklyOnSun == true && fsRouteRow.ActiveOnSunday == false)))
            {
                throw new PXException(TX.Error.RECURRENCE_DAYS_ROUTE_DAYS_MISMATCH);
            }
        }

        /// <summary>
        /// Force 'required fields' in Route tab to be filled when a new record is inserted.
        /// </summary>
        private void ForceFilling_RequiredFields_RouteTab(PXCache cache, FSRouteContractSchedule fsRouteContractScheduleRow)
        {
            if (cache.GetStatus(fsRouteContractScheduleRow) == PXEntryStatus.Inserted)
            {
                //Making sure that required field (DfltRouteID) in Route tab is going to be filled
                FSScheduleRoute fsScheduleRouteRow = ScheduleRoutes.Current;

                if (fsScheduleRouteRow == null)
                {
                    fsScheduleRouteRow = new FSScheduleRoute();
                    ScheduleRoutes.Insert(fsScheduleRouteRow);
                }
            }
        }

        #endregion

        #region Events

        #region FSRouteContractSchedule Events

        protected virtual void FSRouteContractSchedule_VehicleTypeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteContractSchedule fsScheduleRow = (FSRouteContractSchedule)e.Row;
            ResetRouteFields(fsScheduleRow);
        }

        protected virtual void FSRouteContractSchedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteContractSchedule fsContractScheduleRow = (FSRouteContractSchedule)e.Row;
            ContractSchedule_RowSelected_PartialHandler(cache, fsContractScheduleRow);

            bool allowUpdateRoute = fsContractScheduleRow.EntityID != null;

            ScheduleRoutes.Cache.AllowDelete = allowUpdateRoute;
            ScheduleRoutes.Cache.AllowUpdate = allowUpdateRoute;

            bool existAnyGenerationProcess = SharedFunctions.ShowWarningScheduleNotProcessed(cache, fsContractScheduleRow);
            openRouteScheduleProcess.SetEnabled(existAnyGenerationProcess == false);
        }

        protected virtual void FSRouteContractSchedule_EntityID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteContractSchedule fsRouteContractScheduleRow = (FSRouteContractSchedule)e.Row;

            if (fsRouteContractScheduleRow.EntityID != null)
            {
                FSServiceContract fsServiceContract =
                PXSelect<FSServiceContract,
                    Where<
                        FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                .Select(this, fsRouteContractScheduleRow.EntityID);

                if (fsServiceContract != null)
                {
                    fsRouteContractScheduleRow.CustomerID = fsServiceContract.CustomerID;
                    fsRouteContractScheduleRow.CustomerLocationID = fsServiceContract.CustomerLocationID;
                    fsRouteContractScheduleRow.BranchID = fsServiceContract.BranchID;
                    fsRouteContractScheduleRow.BranchLocationID = fsServiceContract.BranchLocationID;
                    fsRouteContractScheduleRow.StartDate = fsServiceContract.StartDate;
                    fsRouteContractScheduleRow.EndDate = fsServiceContract.EndDate;
                }
            }
        }

        protected virtual void FSRouteContractSchedule_WeekCode_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteContractSchedule fsRouteContractScheduleRow = (FSRouteContractSchedule)e.Row;
            ValidateWeekCode(cache, fsRouteContractScheduleRow);            
        }

        protected virtual void FSRouteContractSchedule_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteContractSchedule fsRouteContractScheduleRow = (FSRouteContractSchedule)e.Row;
            FSServiceContract fsServiceContractRow = (FSServiceContract)ContractSelected.Current;

            ValidateWeekCode(cache, fsRouteContractScheduleRow);
            ValidateDays(cache, fsRouteContractScheduleRow);

            ForceFilling_RequiredFields_RouteTab(cache, fsRouteContractScheduleRow);
            ContractSchedule_RowPersisting_PartialHandler(cache, fsServiceContractRow, fsRouteContractScheduleRow, e.Operation, TX.ModuleName.ROUTES_MODULE);
        }

        protected virtual void FSRouteContractSchedule_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            FSSchedule_Row_Deleted_PartialHandler(cache, e);
        }

        #endregion

        #region FSScheduleRoute Events

        protected virtual void FSScheduleRoute_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleRoute fsScheduleRouteRow = (FSScheduleRoute)e.Row;

            if (e.Operation == PXDBOperation.Insert && fsScheduleRouteRow.DfltRouteID.HasValue == true)
            {
                if (string.IsNullOrEmpty(fsScheduleRouteRow.GlobalSequence))
                {
                    FSScheduleRoute fsScheduleRouteRow_Local = PXSelectGroupBy<FSScheduleRoute,
                                                        Aggregate<
                                                            Max<FSScheduleRoute.globalSequence>>>.Select(this);

                    if (fsScheduleRouteRow_Local == null || string.IsNullOrEmpty(fsScheduleRouteRow_Local.GlobalSequence))
                    {
                        fsScheduleRouteRow.GlobalSequence = "00010";
                    }
                    else
                    {
                        fsScheduleRouteRow.GlobalSequence = (int.Parse(fsScheduleRouteRow_Local.GlobalSequence) + 10).ToString("00000");
                    }
                }
                else 
                {
                    fsScheduleRouteRow.GlobalSequence = int.Parse(fsScheduleRouteRow.GlobalSequence).ToString("00000");
                }

                fsScheduleRouteRow.SequenceSunday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceMonday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceTuesday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceWednesday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceThursday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceFriday = fsScheduleRouteRow.GlobalSequence;
                fsScheduleRouteRow.SequenceSaturday = fsScheduleRouteRow.GlobalSequence;
            }
        }
        #endregion

        #endregion
    }
}
