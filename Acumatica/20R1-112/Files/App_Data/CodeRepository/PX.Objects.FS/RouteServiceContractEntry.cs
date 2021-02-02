using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class RouteServiceContractEntry : ServiceContractEntryBase<RouteServiceContractEntry, FSServiceContract,
        Where<FSServiceContract.recordType, Equal<ListField_RecordType_ContractSchedule.RouteServiceContract>>>
    {
        public RouteServiceContractEntry()
            : base()
        {
            ContractSchedules.AllowUpdate = false;
        }

        #region CacheAttached
        #region FSServiceContract_RefNbr
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Service Contract ID", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [FSSelectorContractRefNbrAttribute(typeof(ListField_RecordType_ContractSchedule.RouteServiceContract))]
        [AutoNumber(typeof(Search<FSSetup.serviceContractNumberingID>), typeof(AccessInfo.businessDate))]
        [PX.Data.EP.PXFieldDescription]
        protected virtual void FSServiceContract_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceContract_CustomerContractNbr
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Customer Contract Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
        [FSSelectorCustomerContractNbrAttributeAttribute(typeof(ListField_RecordType_ContractSchedule.RouteServiceContract), typeof(FSServiceContract.customerID))]
        [ServiceContractAutoNumber]
        [PX.Data.EP.PXFieldDescription]
        protected virtual void FSServiceContract_CustomerContractNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceContract_RecordType
        [PXDBString(4, IsUnicode = true)]
        [PXDefault(ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)]
        protected virtual void FSServiceContract_RecordType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteContractSchedule_SrvOrdType
        [PXDBString(4, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault]
        [FSSelectorRouteContractSrvOrdTypeAttribute]
        protected virtual void FSContractSchedule_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions

        #region OpenScheduleScreen
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(DisplayName = "Add Schedule")]
        public override void AddSchedule()
        {
            var graph = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

            FSRouteContractSchedule fsContractScheduleRow = new FSRouteContractSchedule()
            {
                EntityID = ServiceContractRecords.Current.ServiceContractID,
                ScheduleGenType = ServiceContractRecords.Current.ScheduleGenType,
                ProjectID = ServiceContractRecords.Current.ProjectID
            };

            graph.ContractScheduleRecords.Insert(fsContractScheduleRow);

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #region OpenRouteScheduleScreen
        public PXAction<FSServiceContract> OpenRouteScheduleScreen;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openRouteScheduleScreen()
        {
            if (ContractSchedules.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (ContractSchedules.Current.ScheduleID,
                                                                                          ServiceContractRecords.Current.ServiceContractID,
                                                                                          ServiceContractRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenRouteScheduleScreenByScheduleDetService
        public PXAction<FSServiceContract> OpenRouteScheduleScreenByScheduleDetService;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openRouteScheduleScreenByScheduleDetService()
        {
            if (ScheduleDetServicesByContract.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (ScheduleDetServicesByContract.Current.ScheduleID, 
                                                                                          ServiceContractRecords.Current.ServiceContractID,
                                                                                          ServiceContractRecords.Current.CustomerID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region OpenRouteScheduleScreenByScheduleDetPart
        public PXAction<FSServiceContract> OpenRouteScheduleScreenByScheduleDetPart;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openRouteScheduleScreenByScheduleDetPart()
        {
            if (ScheduleDetPartsByContract.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (ScheduleDetPartsByContract.Current.ScheduleID, 
                                                                                          ServiceContractRecords.Current.ServiceContractID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #endregion

        #region Event Handlers

        #region FSContractSchedule Events

        protected virtual void _(Events.RowSelected<FSContractSchedule> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSContractSchedule fsContractScheduleRow = (FSContractSchedule)e.Row;
            SharedFunctions.ShowWarningScheduleNotProcessed(e.Cache, fsContractScheduleRow);
        }

        #endregion

        #region FSServiceContract Events

        protected override void _(Events.RowSelected<FSServiceContract> e)
        {
            base._(e);

            if (e.Row == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = (FSServiceContract)e.Row;

            SharedFunctions.ServiceContractDynamicDropdown(e.Cache, fsServiceContractRow);
        }
        #endregion

        #endregion
    }
}
