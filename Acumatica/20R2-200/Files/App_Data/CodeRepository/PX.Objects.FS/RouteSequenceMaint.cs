using PX.Data;
using PX.Objects.CR;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class RouteSequenceMaint : PXGraph<RouteSequenceMaint>
    {
        #region CacheAttached
        #region FSSchedule_RefNbr
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search3<FSSchedule.refNbr, OrderBy<Desc<FSSchedule.refNbr>>>))]
        protected virtual void FSSchedule_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSServiceContract_Status
        [PXDBString(1, IsFixed = true)]
        [FSServiceContract.status.ListAtrribute]
        [PXUIField(DisplayName = "Service Contract Status", Visibility = PXUIVisibility.SelectorVisible)]
        protected virtual void FSServiceContract_Status_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Address_AddressLine1
        [PXDBString(50, IsUnicode = true)]
        [PXUIField(DisplayName = "Address")]
        protected virtual void Address_AddressLine1_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Filter+Select
        public PXFilter<ServiceContractsByRouteFilter> Filter;
        public PXCancel<ServiceContractsByRouteFilter> Cancel;
        public PXSave<ServiceContractsByRouteFilter> Save;
        public PXSelect<BAccount> bAccountRecords;

        [PXHidden]
        public PXSelect<FSServiceContract> ServiceContractsDummy;
        [PXHidden]
        public PXSelect<FSSchedule> SchedulesDummy;
        [PXHidden]
        public PXSelect<Location> LocationDummy;
        [PXHidden]
        public PXSelect<Address> AddressDummy;

        public PXSelectOrderBy<FSScheduleRoute,
               OrderBy<
                   Asc<FSScheduleRoute.sortingIndex, Asc<FSScheduleRoute.scheduleID>>>> ServiceContracts;
        #endregion

        #region Actions
        public PXAction<ServiceContractsByRouteFilter> resequence;
        public PXAction<ServiceContractsByRouteFilter> openContract;
        public PXAction<ServiceContractsByRouteFilter> openSchedule;
        #endregion

        #region ActionButtons
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        public virtual void OpenContract()
        {
            RouteServiceContractEntry graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();

            FSServiceContract fsServiceContractRow_Local = PXSelectJoin<FSServiceContract,
                                                           InnerJoin<FSSchedule,
                                                           On<
                                                               FSSchedule.entityID, Equal<FSServiceContract.serviceContractID>,
                                                               And<FSSchedule.customerID, Equal<FSServiceContract.customerID>>>>,
                                                           Where<
                                                               FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                                           .Select(this, ServiceContracts.Current.ScheduleID);

            graphRouteServiceContractEntry.ServiceContractRecords.Current = fsServiceContractRow_Local;

            throw new PXRedirectRequiredException(graphRouteServiceContractEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        public virtual void OpenSchedule()
        {
            RouteServiceContractScheduleEntry graphContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

            FSRouteContractSchedule fsScheduleRow_Local = PXSelect<FSRouteContractSchedule,
                                                          Where<
                                                              FSRouteContractSchedule.scheduleID, Equal<Required<FSRouteContractSchedule.scheduleID>>>>
                                                          .Select(this, ServiceContracts.Current.ScheduleID);

            graphContractScheduleEntry.ContractScheduleRecords.Current = fsScheduleRow_Local;

            throw new PXRedirectRequiredException(graphContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        [PXUIField(DisplayName = "Reset Sequence", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Enabled = true)]
        public virtual void Resequence()
        {
            int startSequence = 10;

            foreach (FSScheduleRoute fsScheduleRouteRow in ServiceContracts.Select())
            {
                fsScheduleRouteRow.GlobalSequence = startSequence.ToString().PadLeft(5, '0');
                startSequence = startSequence + 10;
            }

            Save.Press();
            ServiceContracts.View.RequestRefresh();
        }
        #endregion

        #region Delegate
        public virtual IEnumerable serviceContracts()
        {
            PXResultset<FSScheduleRoute> bqlResultSet = new PXResultset<FSScheduleRoute>();
            List<object> scheduleRouteArgs = new List<object>();
            bool hasArguments = false;

            if (Filter.Current.RouteID == null || Filter.Current.WeekDay == null)
            {
                return bqlResultSet;
            }            

            PXSelectBase<FSScheduleRoute> scheduleRouteBase =
            new PXSelectJoin<FSScheduleRoute,
                InnerJoin<FSSchedule,
                    On<
                        FSSchedule.scheduleID, Equal<FSScheduleRoute.scheduleID>>,
                InnerJoin<FSServiceContract,
                    On<FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>,
                        And<FSSchedule.entityType, Equal<FSSchedule.entityType.Contract>>>,
                LeftJoin<Location,
                    On<Location.bAccountID, Equal<FSServiceContract.customerID>,
                        And<Location.locationID, Equal<FSServiceContract.customerLocationID>>>,
                LeftJoin<Address,
                    On<Address.bAccountID, Equal<Location.bAccountID>,
                        And<Address.addressID, Equal<Location.defAddressID>>>>>>>>(this);

            if (Filter.Current.ScheduleFlag == true)
            {
                scheduleRouteBase.WhereAnd<Where<FSSchedule.active, Equal<Required<FSSchedule.active>>>>();
                scheduleRouteArgs.Add(Filter.Current.ScheduleFlag);
            }

            if (Filter.Current.ServiceContractFlag == true)
            {
                scheduleRouteBase.WhereAnd<Where<FSServiceContract.status, Equal<Required<FSServiceContract.status>>>>();
                scheduleRouteArgs.Add(ID.Status_ServiceContract.ACTIVE);
            }

            switch (Filter.Current.WeekDay)
            {
                case ID.WeekDays.ANYDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.dfltRouteID, Equal<Required<FSScheduleRoute.dfltRouteID>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.SUNDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDSunday, Equal<Required<FSScheduleRoute.routeIDSunday>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.MONDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDMonday, Equal<Required<FSScheduleRoute.routeIDMonday>>>>();
                    hasArguments = true;
                    break;
                    
                case ID.WeekDays.TUESDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDTuesday, Equal<Required<FSScheduleRoute.routeIDTuesday>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.WEDNESDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDWednesday, Equal<Required<FSScheduleRoute.routeIDWednesday>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.THURSDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDThursday, Equal<Required<FSScheduleRoute.routeIDThursday>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.FRIDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDFriday, Equal<Required<FSScheduleRoute.routeIDFriday>>>>();
                    hasArguments = true;
                    break;

                case ID.WeekDays.SATURDAY:
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.routeIDSaturday, Equal<Required<FSScheduleRoute.routeIDSaturday>>>>();
                    hasArguments = true;
                    break;

                default: //Default values
                    scheduleRouteBase.WhereAnd<Where<FSScheduleRoute.dfltRouteID, Equal<Required<FSScheduleRoute.dfltRouteID>>>>();
                    hasArguments = true;

                    break;
            }

            if (hasArguments == true)
            {
                scheduleRouteArgs.Add(Filter.Current.RouteID);
            }

            var startRow = PXView.StartRow;
            int totalRows = 0;
            var list = scheduleRouteBase.View.Select(PXView.Currents,
                                                     scheduleRouteArgs.ToArray(),
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
        #endregion

        #region Event Handlers

        #region ServiceContractsByRouteFilter

        protected virtual void _(Events.RowSelected<ServiceContractsByRouteFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            //The SortingIndex gets updated
            foreach (FSScheduleRoute fsScheduleRouteRow in ServiceContracts.Select())
            {
                switch (Filter.Current.WeekDay)
                {
                    case ID.WeekDays.ANYDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.GlobalSequence))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.GlobalSequence);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.SUNDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceSunday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceSunday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.MONDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceMonday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceMonday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.TUESDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceTuesday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceTuesday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.WEDNESDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceWednesday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceWednesday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.THURSDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceThursday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceThursday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.FRIDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceFriday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceFriday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;

                    case ID.WeekDays.SATURDAY:
                        if (fsScheduleRouteRow.SortingIndex != int.Parse(fsScheduleRouteRow.SequenceSaturday))
                        {
                            fsScheduleRouteRow.SortingIndex = int.Parse(fsScheduleRouteRow.SequenceSaturday);
                            ServiceContracts.Update(fsScheduleRouteRow);
                        }

                        break;
                }
            }
        }

        #endregion

        #region FSScheduleRoute

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSScheduleRoute, FSScheduleRoute.globalSequence> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleRoute fsScheduleRouteRow = (FSScheduleRoute)e.Row;
            fsScheduleRouteRow.GlobalSequence = fsScheduleRouteRow.GlobalSequence.PadLeft(5, '0');
            ServiceContracts.View.RequestRefresh();
        }

        #endregion

        protected virtual void _(Events.RowSelecting<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowInserting<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSScheduleRoute> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSScheduleRoute> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSScheduleRoute fsScheduleRouteRow = (FSScheduleRoute)e.Row;
            fsScheduleRouteRow.GlobalSequence = fsScheduleRouteRow.GlobalSequence.PadLeft(5, '0');
        }

        protected virtual void _(Events.RowPersisted<FSScheduleRoute> e)
        {
        }

        #endregion

        #endregion
    }
}