using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.FS
{
    public class ServiceContractEntry : ServiceContractEntryBase<ServiceContractEntry, FSServiceContract, 
            Where<FSServiceContract.recordType, Equal<ListField_RecordType_ContractSchedule.ServiceContract>>>
    {        
        public ServiceContractEntry()
            : base()
        {
            ContractSchedules.AllowUpdate = false;
        }

        #region CacheAttached
        #region FSContractSchedule_SrvOrdType
        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault]
        [FSSelectorContractSrvOrdTypeAttribute]
        protected virtual void FSContractSchedule_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSContractSchedule_CustomerLocationID
        [PXUIField(DisplayName = "Location ID")]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSContractSchedule_CustomerLocationID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Events
        protected override void FSServiceContract_EndDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            base.FSServiceContract_EndDate_FieldUpdated(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            foreach (FSSchedule fsScheduleRow in ContractSchedules.Select())
            {
                fsScheduleRow.EndDate = fsServiceContractRow.EndDate;
                ContractSchedules.Cache.SetStatus(fsScheduleRow, PXEntryStatus.Updated);
            }
        }

        protected virtual void FSServiceContract_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {

            if (e.Row == null)
            {
                return;
            }

            var fsServiceContractRow = (FSServiceContract)e.Row;
            fsServiceContractRow.HasProcessedSchedule = false;

            using (new PXConnectionScope())
            {
                int processedCount = PXSelectReadonly<FSSchedule,
                    Where<
                        FSSchedule.entityType, Equal<FSSchedule.entityType.Contract>,
                        And<FSSchedule.entityID, Equal<Required<FSSchedule.entityID>>,
                        And<FSSchedule.lastGeneratedElementDate, IsNotNull>>>>
                    .Select(this, fsServiceContractRow.ServiceContractID).Count;

                fsServiceContractRow.HasProcessedSchedule = processedCount > 0;
            }
        }

        protected override void FSServiceContract_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            base.FSServiceContract_RowSelected(cache, e);

            if (e.Row == null)
            {
                return;
            }

            var fsServiceContractRow = (FSServiceContract)e.Row;

            PXUIFieldAttribute.SetEnabled<FSServiceContract.scheduleGenType>(cache, e.Row, fsServiceContractRow.HasProcessedSchedule == false);
            SharedFunctions.ServiceContractDynamicDropdown(cache, fsServiceContractRow);
        }

        protected virtual void FSServiceContract_ScheduleGenType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsServiceContractRow = (FSServiceContract)e.Row;

            foreach(FSContractSchedule fsContractScheduleRow in ContractSchedules.Select())
            {
                if (fsContractScheduleRow.LastGeneratedElementDate == null && fsContractScheduleRow.ScheduleGenType != fsServiceContractRow.ScheduleGenType)
                {
                    ContractSchedules.SetValueExt<FSSchedule.scheduleGenType>(fsContractScheduleRow, fsServiceContractRow.ScheduleGenType);
                }
            }
        }

        #region FSContractSchedule Events

        protected virtual void FSContractSchedule_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsContractScheduleRow = (FSContractSchedule)e.Row;
            SharedFunctions.ShowWarningScheduleNotProcessed(cache, fsContractScheduleRow);
        }

        #endregion

        #endregion

        #region Actions

        #region OpenScheduleScreen
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(DisplayName = "Add Schedule")]
        public override void AddSchedule()
        {
            var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();
            FSContractSchedule fsContractScheduleRow = new FSContractSchedule()
            {
                EntityID = ServiceContractRecords.Current.ServiceContractID,
                ScheduleGenType = ServiceContractRecords.Current.ScheduleGenType
            };

            graphServiceContractScheduleEntry.ContractScheduleRecords.Insert(fsContractScheduleRow);

            throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #region OpenScheduleScreen
        public PXAction<FSServiceContract> OpenScheduleScreen;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openScheduleScreen()
        {
            if (ContractSchedules.Current != null)
            {
                var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

                graphServiceContractScheduleEntry.ContractScheduleRecords.Current = graphServiceContractScheduleEntry
                                                                                        .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>(
                                                                                            ContractSchedules.Current.ScheduleID,
                                                                                            ServiceContractRecords.Current.ServiceContractID,
                                                                                            ServiceContractRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenScheduleScreenByScheduleDetService
        public PXAction<FSServiceContract> OpenScheduleScreenByScheduleDetService;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openScheduleScreenByScheduleDetService()
        {
            if (ScheduleDetServicesByContract.Current != null)
            {
                var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

                graphServiceContractScheduleEntry.ContractScheduleRecords.Current = graphServiceContractScheduleEntry
                                                                                        .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>(
                                                                                            ScheduleDetServicesByContract.Current.ScheduleID,
                                                                                            ServiceContractRecords.Current.ServiceContractID);

                throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenScheduleScreenByScheduleDetPart
        public PXAction<FSServiceContract> OpenScheduleScreenByScheduleDetPart;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openScheduleScreenByScheduleDetPart()
        {
            if (ScheduleDetPartsByContract.Current != null)
            {
                var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

                graphServiceContractScheduleEntry.ContractScheduleRecords.Current = graphServiceContractScheduleEntry
                                                                                        .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>(
                                                                                            ScheduleDetPartsByContract.Current.ScheduleID, 
                                                                                            ServiceContractRecords.Current.ServiceContractID);

                throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #endregion
    }
}