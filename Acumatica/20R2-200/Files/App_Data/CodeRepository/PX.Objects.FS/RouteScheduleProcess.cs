using PX.Data;
using PX.Objects.AR;
using PX.Objects.FS.Scheduler;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RouteScheduleProcess : ContractGenerationEnqBase<RouteScheduleProcess, FSRouteContractScheduleFSServiceContract, RouteServiceContractFilter, FSContractGenerationHistory.recordType.RouteServiceContract>
    {
        #region Generation Functions

        #region Delegate
        public RouteScheduleProcess()
        {
            RouteScheduleProcess processor = null;

            RouteContractSchedules.SetProcessDelegate(
                delegate (List<FSRouteContractScheduleFSServiceContract> fsScheduleRowList)
                {
                    processor = PXGraph.CreateInstance<RouteScheduleProcess>();

                    int index = 0;
                    foreach (FSRouteContractScheduleFSServiceContract fsScheduleRow in fsScheduleRowList)
                    {
                        try
                        {
                            processor.processServiceContract(Filter.Cache, fsScheduleRow, Filter.Current.FromDate, Filter.Current.ToDate);

                            PXProcessing<FSRouteContractScheduleFSServiceContract>.SetInfo(index, TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
                        }
                        catch (Exception e)
                        {
                            PXProcessing<FSRouteContractScheduleFSServiceContract>.SetError(index, e);
                        }

                        index++;
                    }

                    updateRoutes(processor.nextGenerationID);
                });
        }
        #endregion

        /// <summary>
        /// Process all Schedules (FSSchedule) in each Contract (FSContract).
        /// </summary>
        protected virtual void processServiceContract(PXCache cache, FSRouteContractScheduleFSServiceContract fsScheduleRow, DateTime? fromDate, DateTime? toDate)
        {
            List<Schedule> mapScheduleResults = new List<Schedule>();

            fsScheduleRow.ContractDescr = fsScheduleRow.DocDesc;
            mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, toDate, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT);

            foreach (Schedule schedule in mapScheduleResults)
            {
                schedule.Priority = (int?)SetSchedulePriority(schedule, this);
                schedule.RouteInfoList = getRouteListFromSchedule(schedule, this);
            }

            GenerateAPPSOUpdateContracts(mapScheduleResults, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT, fromDate, toDate, fsScheduleRow);
        }

        /// <summary>
        /// Update all routes by generation ID.
        /// </summary>
        protected virtual void updateRoutes(int? generationID)
        {
            var fsRouteDocumentSet = PXSelectJoinGroupBy<FSRouteDocument,
                                     InnerJoin<FSAppointment,
                                         On<FSAppointment.routeDocumentID, Equal<FSRouteDocument.routeDocumentID>>>,
                                     Where<
                                         FSAppointment.generationID, Equal<Required<FSAppointment.generationID>>>,
                                     Aggregate<
                                         GroupBy<FSRouteDocument.routeDocumentID>>>
                                     .Select(this, generationID);

            RouteDocumentMaint graphRouteDocumentMaint = CreateInstance<RouteDocumentMaint>();

            foreach (FSRouteDocument fsRouteDocumentRow in fsRouteDocumentSet)
            {
                try
                {
                    graphRouteDocumentMaint.RouteRecords.Current = graphRouteDocumentMaint.RouteRecords.Search<FSRouteDocument.routeDocumentID>(fsRouteDocumentRow.RouteDocumentID);
                    graphRouteDocumentMaint.NormalizeAppointmentPosition();

                    if (RouteSetupRecord.Current != null)
                    {
                        if (RouteSetupRecord.Current.AutoCalculateRouteStats == true)
                        {
                            graphRouteDocumentMaint.calculateRouteStats.PressButton();
                        }
                        else
                        {
                            graphRouteDocumentMaint.calculateSimpleRouteStatistic();
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        #endregion

        #region Private Members

        /// <summary>
        /// Returns the priority of the Schedule depending on its Time Restrictions or Route information.
        /// </summary>
        public static Schedule.ScheduleGenerationPriority SetSchedulePriority(Schedule schedule, PXGraph graph)
        {
            var fsContractScheduleSet = PXSelectJoin<FSContractSchedule,
                                        LeftJoin<
                                            FSScheduleRoute,
                                            On<FSScheduleRoute.scheduleID, Equal<FSContractSchedule.scheduleID>>>,
                                        Where<
                                            FSContractSchedule.entityID, Equal<Required<FSContractSchedule.entityID>>>>
                                        .Select(graph, schedule.EntityID);

            foreach (PXResult<FSContractSchedule, FSScheduleRoute> bqlResult in fsContractScheduleSet)
            {
                FSContractSchedule fsContractScheduleRow = (FSContractSchedule)bqlResult;
                FSScheduleRoute fsScheduleRouteRow = (FSScheduleRoute)bqlResult;

                if (fsContractScheduleRow.RestrictionMax == true || fsContractScheduleRow.RestrictionMin == true)
                {
                    return Schedule.ScheduleGenerationPriority.TimeRestriction;
                }
                else if (fsScheduleRouteRow.DfltRouteID != null)
                {
                    return Schedule.ScheduleGenerationPriority.Sequence;
                }
            }

            return Schedule.ScheduleGenerationPriority.Nothing;
        }

        /// <summary>
        /// Prepares a new List[RouteInfo] with the information of the routes defined in a particular FSScheduleRoute.
        /// </summary>
        public static List<RouteInfo> getRouteListFromSchedule(Schedule schedule, PXGraph graph)
        {
            List<RouteInfo> returnList = new List<RouteInfo>();

            var fsContractScheduleSet = PXSelectJoin<FSContractSchedule,
                                        LeftJoin<
                                             FSScheduleRoute,
                                             On<FSScheduleRoute.scheduleID, Equal<FSContractSchedule.scheduleID>>>,
                                        Where<
                                             FSContractSchedule.entityID, Equal<Required<FSContractSchedule.entityID>>>>
                                        .Select(graph, schedule.EntityID);

            foreach (PXResult<FSContractSchedule, FSScheduleRoute> bqlResult in fsContractScheduleSet)
            {
                FSScheduleRoute fsScheduleRouteRow = (FSScheduleRoute)bqlResult;

                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDSunday, int.Parse(fsScheduleRouteRow.SequenceSunday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDMonday, int.Parse(fsScheduleRouteRow.SequenceMonday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDTuesday, int.Parse(fsScheduleRouteRow.SequenceTuesday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDWednesday, int.Parse(fsScheduleRouteRow.SequenceWednesday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDThursday, int.Parse(fsScheduleRouteRow.SequenceThursday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDFriday, int.Parse(fsScheduleRouteRow.SequenceFriday)));
                returnList.Add(new RouteInfo(fsScheduleRouteRow.RouteIDSaturday, int.Parse(fsScheduleRouteRow.SequenceSaturday)));
            }

            return returnList;
        }
        #endregion 

        #region CacheAttached
        #region FSSchedule_RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search3<FSSchedule.refNbr, OrderBy<Desc<FSSchedule.refNbr>>>))]
        protected virtual void FSSchedule_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSSchedule_LastGeneratedElementDate
        [PXDBDate]
        [PXUIField(DisplayName = "Last Generated Appointment Date")]
        protected virtual void FSRouteContractSchedule_FSServiceContract_LastGeneratedElementDate_CacheAttached(PXCache sender)
        {
        }
        #endregion        
        #region FSSchedule_StartDate
        [PXDBDate]
        [PXUIField(DisplayName = "Schedule Start Date")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        protected virtual void FSRouteContractScheduleFSServiceContract_StartDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSSchedule_EndDate
        [PXDBDate]
        [PXUIField(DisplayName = "Schedule Expiration Date")]
        protected virtual void FSRouteContractScheduleFSServiceContract_EndDate_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Filter+Select
        [PXFilterable]
        [PXViewDetailsButton(typeof(RouteServiceContractFilter))]
        public PXFilteredProcessingJoin<FSRouteContractScheduleFSServiceContract, RouteServiceContractFilter,
               InnerJoin<FSScheduleRoute,
                    On<FSScheduleRoute.scheduleID, Equal<FSRouteContractScheduleFSServiceContract.scheduleID>>,
                InnerJoin<Customer,
                    On<Customer.bAccountID, Equal<FSRouteContractScheduleFSServiceContract.customerID>,
                        And<Match<Customer, Current<AccessInfo.userName>>>>>>,
               Where2<
                    Where<
                        FSRouteContractScheduleFSServiceContract.active, Equal<True>,
                        And<
                            Where<
                                FSRouteContractScheduleFSServiceContract.nextExecutionDate, LessEqual<Current<RouteServiceContractFilter.toDate>>,
                                And<
                                    Where<FSRouteContractScheduleFSServiceContract.enableExpirationDate, Equal<False>,
                                        Or<FSRouteContractScheduleFSServiceContract.endDate, Greater<FSRouteContractScheduleFSServiceContract.nextExecutionDate>>>>>>>,
                    //Start filter conditions
                    And2<
                        Where<Current<RouteServiceContractFilter.routeID>, IsNull,
                                Or<FSScheduleRoute.routeIDMonday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDTuesday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDWednesday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDThursday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDFriday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDSaturday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.routeIDSunday, Equal<Current<RouteServiceContractFilter.routeID>>,
                                Or<FSScheduleRoute.dfltRouteID, Equal<Current<RouteServiceContractFilter.routeID>>>>>>>>>>>,
                        And2<
                            Where<Current<RouteServiceContractFilter.scheduleID>, IsNull,
                                Or<FSSchedule.scheduleID, Equal<Current<RouteServiceContractFilter.scheduleID>>>>,
                        And<FSRouteContractScheduleFSServiceContract.startDate, LessEqual<Current<RouteServiceContractFilter.toDate>>>>>>,
                //End filter conditions
                OrderBy<
                    Asc<FSRouteContractScheduleFSServiceContract.customerID,
                    Asc<FSRouteContractScheduleFSServiceContract.entityID,
                    Asc<FSRouteContractScheduleFSServiceContract.refNbr>>>>> RouteContractSchedules;

        public new ContractHistoryRecords_View ContractHistoryRecords;

        public new ErrorMessageRecords_View ErrorMessageRecords;

        // Allow override fields that don't belong to main DAC
        public PXSelect<FSRoute> Routes;
        public PXSelect<FSServiceContract> ServiceContractsRecords;
        [PXHidden]
        public PXSetup<FSRouteSetup> RouteSetupRecord;
        #endregion

        #region Action
        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openScheduleScreenBySchedules()
        {
            RouteServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

            FSRouteContractSchedule fsContractScheduleRow = PXSelect<FSRouteContractSchedule,
                                                            Where<
                                                                FSRouteContractSchedule.refNbr, Equal<Required<FSRouteContractSchedule.refNbr>>,
                                                                And<FSRouteContractSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                                                And<FSRouteContractSchedule.entityID, Equal<Required<FSRouteContractSchedule.entityID>>,
                                                                And<FSRouteContractSchedule.customerID, Equal<Required<FSRouteContractSchedule.customerID>>>>>>>
                                                            .Select(this, RouteContractSchedules.Current.RefNbr, RouteContractSchedules.Current.EntityID, RouteContractSchedules.Current.CustomerID);

            graphContractScheduleEntry.ContractScheduleRecords.Current = fsContractScheduleRow;
            throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openScheduleScreenByGenerationLogError()
        {
            RouteServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

            FSRouteContractSchedule fsContractScheduleRow = PXSelect<FSRouteContractSchedule,
                                                            Where<
                                                                FSRouteContractSchedule.scheduleID, Equal<Required<FSRouteContractSchedule.scheduleID>>>>
                                                            .Select(this, ErrorMessageRecords.Current.ScheduleID);

            graphContractScheduleEntry.ContractScheduleRecords.Current = fsContractScheduleRow;
            throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openServiceContractScreenBySchedules()
        {
            RouteServiceContractEntry graphServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

            FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                     Where<
                                                         FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                     .Select(this, RouteContractSchedules.Current.EntityID);

            graphServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow;
            throw new PXRedirectRequiredException(graphServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openServiceContractScreenByGenerationLogError()
        {
            RouteServiceContractEntry graphServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

            FSSchedule fsScheduleRow = PXSelect<FSSchedule,
                                       Where<
                                           FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                       .Select(this, ErrorMessageRecords.Current.ScheduleID);

            FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                     Where<
                                                         FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                     .Select(this, fsScheduleRow.EntityID);

            graphServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow;

            throw new PXRedirectRequiredException(graphServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        //This Action can't be moved to the Shared Graph (ContractGenerationEnqBase) because the toolbar icon in the interface won't display correctly
        [PXButton]
        [PXUIField(DisplayName = "Clear all errors", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void clearAll()
        {
            var graphGenerationLogError = PXGraph.CreateInstance<GenerationLogErrorMaint>();

            PXResultset<FSGenerationLogError> bqlResultSet = PXSelect<FSGenerationLogError,
                                                             Where<
                                                                FSGenerationLogError.ignore, Equal<False>,
                                                                And<FSGenerationLogError.processType, Equal<Required<FSGenerationLogError.processType>>>>>
                                                             .Select(this, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT);

            PXLongOperation.StartOperation(
                this,
                delegate
                {
                    foreach (FSGenerationLogError fsGenerationLogErrorRow in bqlResultSet)
                    {
                        graphGenerationLogError.Clear();
                        graphGenerationLogError.LogErrorMessageRecords.Current = graphGenerationLogError.LogErrorMessageRecords.Search<FSGenerationLogError.logID>(fsGenerationLogErrorRow.LogID);
                        fsGenerationLogErrorRow.Ignore = true;
                        graphGenerationLogError.LogErrorMessageRecords.Update(fsGenerationLogErrorRow);
                        graphGenerationLogError.Save.Press();
                    }
                });
        }

        public PXAction<RouteServiceContractFilter> fixSchedulesWithoutNextExecutionDate;
        [PXButton]
        [PXUIField(DisplayName = "Fix Schedules Without Next Execution Date", Visible = false)]
        public virtual IEnumerable FixSchedulesWithoutNextExecutionDate(PXAdapter adapter)
        {
            SharedFunctions.UpdateSchedulesWithoutNextExecution(this, this.Filter.Cache);
            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        protected virtual void _(Events.RowSelected<RouteServiceContractFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            RouteServiceContractFilter filter = (RouteServiceContractFilter)e.Row;

            //TODO AC-142850 SD-6478 [TS- S. Generate Route App] - Implement Pre Assigned Options
            PXUIFieldAttribute.SetVisible<RouteServiceContractFilter.preassignedDriver>(Filter.Cache, filter, false);
            PXUIFieldAttribute.SetVisible<RouteServiceContractFilter.preassignedVehicle>(Filter.Cache, filter, false);

            string warningMessage = "";
            bool warning = false;

            warningMessage = SharedFunctions.WarnUserWithSchedulesWithoutNextExecution(this, Filter.Cache, fixSchedulesWithoutNextExecutionDate, out warning);

            if (warning == true)
            {
                e.Cache.RaiseExceptionHandling<StaffScheduleFilter.toDate>(filter,
                                                                           filter.ToDate,
                                                                           new PXSetPropertyException(warningMessage, PXErrorLevel.Warning));
            }
        }

        #endregion
    }
}