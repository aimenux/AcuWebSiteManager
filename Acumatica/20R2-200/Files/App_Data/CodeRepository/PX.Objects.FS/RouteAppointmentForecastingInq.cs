using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.FS.Scheduler;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RouteAppointmentForecastingInq : PXGraph<RouteAppointmentForecastingInq>
    {
        #region Actions
        #region Cancel
        public PXCancel<RouteAppointmentForecastingFilter> Cancel;
        #endregion

        #region OpenServiceContractScreen
        public PXAction<RouteAppointmentForecastingFilter> OpenServiceContractScreen;
        [PXButton]
        [PXUIField(Visible = false)]
        protected virtual void openServiceContractScreen()
        {
            if (RouteAppointmentForecastingRecords.Current != null)
            {
                RouteServiceContractEntry graphServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

                FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                         Where<
                                                             FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                         .Select(this, RouteAppointmentForecastingRecords.Current.ServiceContractID);

                graphServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow;
                throw new PXRedirectRequiredException(graphServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenScheduleScreen
        public PXAction<RouteAppointmentForecastingFilter> OpenScheduleScreen;
        [PXButton]
        [PXUIField(Visible = false)]
        protected virtual void openScheduleScreen()
        {
            if (RouteAppointmentForecastingRecords.Current != null)
            {
                RouteServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                FSRouteContractSchedule fsContractScheduleRow = PXSelect<FSRouteContractSchedule,
                                                                Where<
                                                                    FSRouteContractSchedule.scheduleID, Equal<Required<FSRouteContractSchedule.scheduleID>>,
                                                                    And<FSRouteContractSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                                                    And<FSRouteContractSchedule.entityID, Equal<Required<FSRouteContractSchedule.entityID>>,
                                                                    And<FSRouteContractSchedule.customerID, Equal<Required<FSRouteContractSchedule.customerID>>>>>>>
                                                                .Select(this, RouteAppointmentForecastingRecords.Current.ScheduleID, RouteAppointmentForecastingRecords.Current.ServiceContractID, RouteAppointmentForecastingRecords.Current.CustomerID);

                graphContractScheduleEntry.ContractScheduleRecords.Current = fsContractScheduleRow;
                throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenLocationScreen
        public PXAction<RouteAppointmentForecastingFilter> OpenLocationScreen;
        [PXButton]
        [PXUIField(Visible = false)]
        protected virtual void openLocationScreen()
        {
            if (RouteAppointmentForecastingRecords.Current != null)
            {
                CustomerLocationMaint graphCustomerLocationMaint = PXGraph.CreateInstance<CustomerLocationMaint>();

                BAccount bAccountRow = PXSelect<BAccount,
                                       Where<
                                           BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                                       .Select(this, RouteAppointmentForecastingRecords.Current.CustomerID);

                graphCustomerLocationMaint.Location.Current = graphCustomerLocationMaint.Location.Search<Location.locationID>
                                                                (RouteAppointmentForecastingRecords.Current.CustomerLocationID, bAccountRow.AcctCD);

                throw new PXRedirectRequiredException(graphCustomerLocationMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region GenerateProjection
        public PXAction<RouteAppointmentForecastingFilter> generateProjection;
        [PXUIField(DisplayName = "Generate A Projection", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable GenerateProjection(PXAdapter adapter)
        {
            if (RouteAppointmentForecastingFilter.Current != null
                    && RouteAppointmentForecastingFilter.Current.DateBegin != null
                    && RouteAppointmentForecastingFilter.Current.DateEnd != null
                    && RouteAppointmentForecastingFilter.Current.DateBegin < RouteAppointmentForecastingFilter.Current.DateEnd)
            {
                DateTime beginFromFilter = RouteAppointmentForecastingFilter.Current.DateBegin.Value;
                DateTime endToFilter = RouteAppointmentForecastingFilter.Current.DateEnd.Value;
                string recordType = ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT;

                int? serviceID = RouteAppointmentForecastingFilter.Current.ServiceID;
                int? customerID = RouteAppointmentForecastingFilter.Current.CustomerID;
                int? customerLocationID = RouteAppointmentForecastingFilter.Current.CustomerLocationID;
                int? routeID = RouteAppointmentForecastingFilter.Current.RouteID;

                PXLongOperation.StartOperation(
                    this,
                    delegate
                    {
                        using (PXTransactionScope ts = new PXTransactionScope())
                        {
                            if (beginFromFilter != null && endToFilter != null)
                            {
                                DateHandler requestDate = new DateHandler(endToFilter);

                                var period = new Period((DateTime)beginFromFilter, (DateTime)endToFilter);

                                List<Scheduler.Schedule> scheduleListToProcess = new List<Scheduler.Schedule>();
                                var generator = new TimeSlotGenerator();

                                var fsRouteContractScheduleRecords =
                                    PXSelectJoinGroupBy<FSRouteContractSchedule,
                                        InnerJoin<FSServiceContract,
                                            On<FSServiceContract.serviceContractID, Equal<FSRouteContractSchedule.entityID>>,
                                        InnerJoin<FSScheduleRoute,
                                            On<FSScheduleRoute.scheduleID, Equal<FSRouteContractSchedule.scheduleID>>,
                                        InnerJoin<FSScheduleDet,
                                            On<FSScheduleDet.scheduleID, Equal<FSRouteContractSchedule.scheduleID>>>>>,
                                    Where<
                                        FSRouteContractSchedule.entityType, Equal<FSSchedule.entityType.Contract>,
                                        And<FSServiceContract.recordType, Equal<FSServiceContract.recordType.RouteServiceContract>,
                                        And<FSRouteContractSchedule.active, Equal<True>,
                                        And<FSServiceContract.status, Equal<FSServiceContract.status.Active>,
                                        And<FSRouteContractSchedule.startDate, LessEqual<Required<FSRouteContractSchedule.startDate>>,
                                        And2<
                                            Where<
                                                Required<FSScheduleDet.inventoryID>, IsNull,
                                                Or<FSScheduleDet.inventoryID, Equal<Required<FSScheduleDet.inventoryID>>>>,
                                        And2<
                                            Where<
                                                Required<FSRouteContractSchedule.customerID>, IsNull,
                                                Or<FSRouteContractSchedule.customerID, Equal<Required<FSRouteContractSchedule.customerID>>>>,
                                        And2<
                                            Where<
                                                Required<FSRouteContractSchedule.customerLocationID>, IsNull,
                                                Or<FSRouteContractSchedule.customerLocationID, Equal<Required<FSRouteContractSchedule.customerLocationID>>>>,
                                        And2<
                                            Where<
                                                Required<FSScheduleRoute.dfltRouteID>, IsNull,
                                                Or<FSScheduleRoute.dfltRouteID, Equal<Required<FSScheduleRoute.dfltRouteID>>>>,
                                        And<
                                            Where<FSRouteContractSchedule.endDate, IsNull,
                                                Or<FSRouteContractSchedule.endDate, GreaterEqual<Required<FSRouteContractSchedule.startDate>>>>>>>>>>>>>>,
                                    Aggregate<
                                        GroupBy<FSRouteContractSchedule.scheduleID>>>
                                    .Select(this, beginFromFilter, serviceID, serviceID, customerID, customerID, customerLocationID, customerLocationID, routeID, routeID, endToFilter);

                                foreach (FSRouteContractSchedule fsRouteContractScheduleRecord in fsRouteContractScheduleRecords)
                                {
                                    List<Scheduler.Schedule> mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(RouteAppointmentForecastingFilter.Cache, fsRouteContractScheduleRecord, endToFilter, recordType);

                                    foreach (Scheduler.Schedule mapSchedule in mapScheduleResults)
                                    {
                                        scheduleListToProcess.Add(mapSchedule);
                                    }
                                }

                                if (recordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                                {
                                    foreach (Scheduler.Schedule schedule in scheduleListToProcess)
                                    {
                                        schedule.Priority = (int?)RouteScheduleProcess.SetSchedulePriority(schedule, this);
                                        schedule.RouteInfoList = RouteScheduleProcess.getRouteListFromSchedule(schedule, this);
                                    }
                                }

                                List<TimeSlot> timeSlots = generator.GenerateCalendar(period, scheduleListToProcess);

                                var fsRouteAppointmentForecastingRecords =
                                    PXSelect<FSRouteAppointmentForecasting,
                                    Where<
                                        FSRouteAppointmentForecasting.startDate, GreaterEqual<Required<FSRouteAppointmentForecasting.startDate>>,
                                        And<
                                            FSRouteAppointmentForecasting.startDate, LessEqual<Required<FSRouteAppointmentForecasting.startDate>>>>>
                                    .Select(this, beginFromFilter, endToFilter);

                                PXCache<FSRouteAppointmentForecasting> cacheRouteForecasting = new PXCache<FSRouteAppointmentForecasting>(this);

                                foreach (FSRouteAppointmentForecasting fsRouteAppointmentDetailProjectionRecord in fsRouteAppointmentForecastingRecords)
                                {
                                    cacheRouteForecasting.PersistDeleted(fsRouteAppointmentDetailProjectionRecord);
                                }

                                foreach (TimeSlot timeSlot in timeSlots)
                                {
                                    requestDate.SetDate(timeSlot.DateTimeBegin);

                                    FSSchedule fsScheduleRow = ScheduleSelected.Select(timeSlot.ScheduleID);

                                    FSScheduleRoute fsScheduleRouteRow = ScheduleRouteSelected.Select(fsScheduleRow.ScheduleID);

                                    FSServiceContract fsServiceContractRow = ServiceContractSelected.Select(fsScheduleRow.EntityID);

                                    FSRouteAppointmentForecasting fsRouteAppointmentForecastingRow = new FSRouteAppointmentForecasting();

                                    fsRouteAppointmentForecastingRow.RouteID = fsScheduleRouteRow.DfltRouteID;
                                    fsRouteAppointmentForecastingRow.StartDate = requestDate.StartOfDay();
                                    fsRouteAppointmentForecastingRow.ScheduleID = fsScheduleRow.ScheduleID;
                                    fsRouteAppointmentForecastingRow.ServiceContractID = fsServiceContractRow.ServiceContractID;
                                    fsRouteAppointmentForecastingRow.CustomerLocationID = fsServiceContractRow.CustomerLocationID;
                                    fsRouteAppointmentForecastingRow.CustomerID = fsServiceContractRow.CustomerID;
                                    fsRouteAppointmentForecastingRow.SequenceOrder = int.Parse(fsScheduleRouteRow.GlobalSequence);

                                    cacheRouteForecasting.PersistInserted(fsRouteAppointmentForecastingRow);
                                }
                            }

                            ts.Complete();
                        }
                    });
            }

            return adapter.Get();
        }
        #endregion
        #endregion

        #region CacheAttached

        #region FSSchedule CacheAttached
        #region RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSSchedule_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #endregion

        #region Selects
        public PXFilter<RouteAppointmentForecastingFilter> RouteAppointmentForecastingFilter;

        public PXSelectJoinGroupBy<FSRouteAppointmentForecasting,
                LeftJoin<FSServiceContract,
                    On<
                        FSServiceContract.serviceContractID, Equal<FSRouteAppointmentForecasting.serviceContractID>>,
                LeftJoin<FSSchedule,
                    On<
                        FSSchedule.scheduleID, Equal<FSRouteAppointmentForecasting.scheduleID>>,
                LeftJoin<FSScheduleDet,
                    On<
                        FSScheduleDet.scheduleID, Equal<FSRouteAppointmentForecasting.scheduleID>>>>>,
                Where2<
                    Where<
                        Current2<RouteAppointmentForecastingFilter.dateBegin>, IsNull,
                            Or<FSRouteAppointmentForecasting.startDate, GreaterEqual<Current2<RouteAppointmentForecastingFilter.dateBegin>>>>,
                    And2<
                        Where<Current2<RouteAppointmentForecastingFilter.dateEnd>, IsNull,
                            Or<FSRouteAppointmentForecasting.startDate, LessEqual<Current2<RouteAppointmentForecastingFilter.dateEnd>>>>,
                    And2<
                        Where<Current2<RouteAppointmentForecastingFilter.customerID>, IsNull,
                            Or<FSRouteAppointmentForecasting.customerID, Equal<Current2<RouteAppointmentForecastingFilter.customerID>>>>,
                    And2<
                        Where<Current2<RouteAppointmentForecastingFilter.customerLocationID>, IsNull,
                            Or<FSRouteAppointmentForecasting.customerLocationID, Equal<Current2<RouteAppointmentForecastingFilter.customerLocationID>>>>,
                    And2<
                        Where<Current2<RouteAppointmentForecastingFilter.routeID>, IsNull,
                            Or<FSRouteAppointmentForecasting.routeID, Equal<Current2<RouteAppointmentForecastingFilter.routeID>>>>,
                    And<
                        Where<Current2<RouteAppointmentForecastingFilter.serviceID>, IsNull,
                            Or<FSScheduleDet.inventoryID, Equal<Current2<RouteAppointmentForecastingFilter.serviceID>>>>>>>>>>,
                Aggregate<
                        GroupBy<FSRouteAppointmentForecasting.startDate,
                        GroupBy<FSRouteAppointmentForecasting.scheduleID>>>,
                OrderBy<
                    Asc<FSRouteAppointmentForecasting.startDate,
                    Asc<FSRouteAppointmentForecasting.sequenceOrder>>>> RouteAppointmentForecastingRecords;

        public PXSelect<FSSchedule,
               Where<
                   FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>> ScheduleSelected;

        public PXSelect<FSServiceContract,
               Where<
                   FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>> ServiceContractSelected;

        public PXSelect<FSScheduleRoute,
               Where<
                   FSScheduleRoute.scheduleID, Equal<Required<FSScheduleRoute.scheduleID>>>> ScheduleRouteSelected;
        #endregion

        #region Events

        protected virtual void _(Events.RowSelected<RouteAppointmentForecastingFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            generateProjection.SetEnabled(RouteAppointmentForecastingFilter.Current.DateBegin != null && RouteAppointmentForecastingFilter.Current.DateEnd != null);
        }
        #endregion
    }
}