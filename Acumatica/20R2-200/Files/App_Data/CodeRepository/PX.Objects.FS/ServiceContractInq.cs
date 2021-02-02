using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.FS.Scheduler;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.Objects.AR;

namespace PX.Objects.FS
{
    [PX.Objects.GL.TableAndChartDashboardType]
    public class ServiceContractInq : ContractGenerationEnqBase<ServiceContractInq, FSContractSchedule, ServiceContractFilter, FSContractGenerationHistory.recordType.ServiceContract>
    {
        #region Generation Functions
        #region Delegate
        public ServiceContractInq()
        {
            ServiceContractSchedules.ParallelProcessingOptions =
            settings =>
                {
                settings.IsEnabled = true;
                settings.BatchSize = 10;
            };

            ServiceContractFilter ServiceContractFilterCurrent = Filter.Current;

            ServiceContractSchedules.SetProcessDelegate<ServiceContractInq>(
                (graph, fsContractScheduleRow) => ProcessSchedules(graph, fsContractScheduleRow, ServiceContractFilterCurrent));
        }
        #endregion

        protected virtual void ProcessSchedules(ServiceContractInq processor, FSContractSchedule fsScheduleRow, ServiceContractFilter Filter)
        {
            try
            {
                DateTime? fromDate = fsScheduleRow.NextExecutionDate ?? fsScheduleRow.StartDate;

                processor.processServiceContract(processor.Filter.Cache, fsScheduleRow, fromDate, Filter.ToDate);

                PXProcessing<FSContractSchedule>.SetInfo(TX.Messages.RECORD_PROCESSED_SUCCESSFULLY);
            }
            catch (Exception e)
            {
                PXProcessing<FSContractSchedule>.SetError(e);
            }
        }

        /// <summary>
        /// Process all Schedules (FSSchedule) in each Contract (FSContract).
        /// </summary>
        public virtual void processServiceContract(PXCache cache, FSContractSchedule fsScheduleRow, DateTime? fromDate, DateTime? toDate)
        {
            List<Schedule> mapScheduleResults = new List<Schedule>();

            FSServiceContract fsServiceContractRow_Current = ServiceContractSelected.Select(fsScheduleRow.EntityID);

            if (fsServiceContractRow_Current != null)
            {
                fsScheduleRow.ContractDescr = fsServiceContractRow_Current.DocDesc;
            }

            mapScheduleResults = MapFSScheduleToSchedule.convertFSScheduleToSchedule(cache, fsScheduleRow, toDate, ID.RecordType_ServiceContract.SERVICE_CONTRACT);
            GenerateAPPSOUpdateContracts(mapScheduleResults, ID.RecordType_ServiceContract.SERVICE_CONTRACT, fromDate, toDate, fsScheduleRow);
        }

        #endregion

        [PXHidden]
        public PXSelect<FSServiceContract> ServiceContracts;

        #region Filter+Select
        [PXFilterable]
        [PXViewDetailsButton(typeof(ServiceContractFilter))]
        public PXFilteredProcessingJoin<FSContractSchedule, ServiceContractFilter,
                InnerJoin<FSServiceContract,
                    On<FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>,
                        And<FSSchedule.entityType, Equal<FSSchedule.entityType.Contract>>>,
                InnerJoinSingleTable<Customer,
                    On<Customer.bAccountID, Equal<FSServiceContract.customerID>,
                        And<Match<Customer, Current<AccessInfo.userName>>>>>>,
                Where2<
                    Where<FSServiceContract.status, Equal<ListField_Status_ServiceContract.Active>,
                        And<FSServiceContract.recordType, Equal<FSServiceContract.recordType.ServiceContract>>>,
                    And2<
                        Where<FSSchedule.active, Equal<True>>,
                    And2<
                        Where<
                            FSSchedule.nextExecutionDate, LessEqual<Current<ServiceContractFilter.toDate>>,
                            And<
                                Where<FSSchedule.enableExpirationDate, Equal<False>,
                                    Or<FSSchedule.endDate, Greater<FSSchedule.nextExecutionDate>>>>>,
                        //Start filter conditions
                        And2<Where<Current<ServiceContractFilter.customerID>, IsNull,
                            Or<FSServiceContract.customerID, Equal<Current<ServiceContractFilter.customerID>>>>,
                        And2<Where<Current<ServiceContractFilter.branchID>, IsNull,
                            Or<FSServiceContract.branchID, Equal<Current<ServiceContractFilter.branchID>>>>,
                        And2<Where<Current<ServiceContractFilter.branchLocationID>, IsNull,
                            Or<FSServiceContract.branchLocationID, Equal<Current<ServiceContractFilter.branchLocationID>>>>,
                        And2<Where<Current<ServiceContractFilter.customerLocationID>, IsNull,
                            Or<FSServiceContract.customerLocationID, Equal<Current<ServiceContractFilter.customerLocationID>>>>,
                        And2<Where<Current<ServiceContractFilter.scheduleID>, IsNull,
                            Or<FSSchedule.scheduleID, Equal<Current<ServiceContractFilter.scheduleID>>>>,
                        And<FSContractSchedule.startDate, LessEqual<Current<ServiceContractFilter.toDate>>>>>>>>>>>,
                        //End filter conditions
                OrderBy<
                        Asc<FSContractSchedule.customerID,
                        Asc<FSContractSchedule.entityID,
                        Asc<FSContractSchedule.refNbr>>>>>
                ServiceContractSchedules;

        public new ContractHistoryRecords_View ContractHistoryRecords;

        public new ErrorMessageRecords_View ErrorMessageRecords;
        #endregion

        #region CacheAttached
        #region FSServiceContract_CustomerID
        [PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSServiceContract_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSSchedule_RefNbr
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSSchedule_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion 
        #region FSContractSchedule_RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search3<FSSchedule.refNbr, OrderBy<Desc<FSSchedule.refNbr>>>))]
        protected virtual void FSContractSchedule_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_LastGeneratedElementDate
        [PXDBDate]
        [PXUIField(DisplayName = "Last Generated Service Order Date")]
        protected virtual void FSContractSchedule_LastGeneratedElementDate_CacheAttached(PXCache sender)
        {
        }
        #endregion        
        #region FSContractSchedule_CustomerLocationID
        [PXDBInt]
        [PXUIField(DisplayName = "Schedule Location ID")]
        [PXDimensionSelector(LocationIDBaseAttribute.DimensionName,
                typeof(Search<Location.locationID,
                        Where<
                            Location.bAccountID, Equal<Current<FSSchedule.customerID>>>>),
                typeof(Location.locationCD),
                DescriptionField = typeof(Location.descr))]
        protected virtual void FSContractSchedule_CustomerLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceContract_CustomerLocationID
        [PXDefault]
        [LocationID(typeof(Where<Location.bAccountID, Equal<Current<FSServiceContract.customerID>>>),
                      DescriptionField = typeof(Location.descr), DisplayName = "Contract Location", DirtyRead = true)]
        protected virtual void FSServiceContract_CustomerLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions
        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openScheduleScreenBySchedules()
        {
            ServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

            FSContractSchedule fsContractScheduleRow = PXSelect<FSContractSchedule,
                                                       Where<
                                                           FSContractSchedule.refNbr, Equal<Required<FSContractSchedule.refNbr>>,
                                                           And<FSContractSchedule.entityType, Equal<ListField_Schedule_EntityType.Contract>,
                                                           And<FSContractSchedule.entityID, Equal<Required<FSContractSchedule.entityID>>,
                                                           And<FSContractSchedule.customerID, Equal<Required<FSContractSchedule.customerID>>>>>>>
                                                       .Select(this, ServiceContractSchedules.Current.RefNbr, ServiceContractSchedules.Current.EntityID, ServiceContractSchedules.Current.CustomerID);
            
            graphContractScheduleEntry.ContractScheduleRecords.Current = fsContractScheduleRow;

            throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openScheduleScreenByGenerationLogError()
        {
            ServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

            FSContractSchedule fsContractScheduleRow = PXSelect<FSContractSchedule,
                                                       Where<
                                                           FSContractSchedule.scheduleID, Equal<Required<FSContractSchedule.scheduleID>>>>
                                                       .Select(this, ErrorMessageRecords.Current.ScheduleID);

            graphContractScheduleEntry.ContractScheduleRecords.Current = fsContractScheduleRow;

            throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openServiceContractScreenBySchedules()
        {
            ServiceContractEntry graphServiceContractEntry = PXGraph.CreateInstance<ServiceContractEntry>();

            FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                     Where<
                                                         FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                     .Select(this, ServiceContractSchedules.Current.EntityID);

            graphServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow;

            throw new PXRedirectRequiredException(graphServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public override void openServiceContractScreenByGenerationLogError()
        {
            ServiceContractEntry graphServiceContractEntry = PXGraph.CreateInstance<ServiceContractEntry>();

            FSServiceContract fsServiceContractRow = PXSelectJoin<FSServiceContract,
                                                     InnerJoin<FSSchedule,
                                                     On<
                                                         FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>,
                                                         And<FSSchedule.customerID, Equal<FSServiceContract.customerID>>>>,
                                                     Where<
                                                         FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                                     .Select(this, ErrorMessageRecords.Current.ScheduleID);

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
                                                             .Select(this, ID.RecordType_ServiceContract.SERVICE_CONTRACT);

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

        public PXAction<ServiceContractFilter> fixSchedulesWithoutNextExecutionDate;
        [PXButton]
        [PXUIField(DisplayName = "Fix Schedules Without Next Execution Date", Visible = false)]
        public virtual IEnumerable FixSchedulesWithoutNextExecutionDate(PXAdapter adapter)
        {
            SharedFunctions.UpdateSchedulesWithoutNextExecution(this, this.Filter.Cache);
            return adapter.Get();
        }
        #endregion

        #region Event Handlers

        protected virtual void _(Events.RowSelected<ServiceContractFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceContractFilter filter = (ServiceContractFilter)e.Row;
            PXCache cache = e.Cache;

            string warningMessage = "";
            bool warning = false;

            warningMessage = SharedFunctions.WarnUserWithSchedulesWithoutNextExecution(this, cache, fixSchedulesWithoutNextExecutionDate, out warning);

            if (warning == true)
            {
                cache.RaiseExceptionHandling<StaffScheduleFilter.toDate>(filter,
                                                                         filter.ToDate,
                                                                         new PXSetPropertyException(warningMessage, PXErrorLevel.Warning));
            }
        }

        #endregion
    }
}