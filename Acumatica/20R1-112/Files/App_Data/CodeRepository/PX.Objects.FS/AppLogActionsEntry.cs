using PX.Common;
using PX.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class AppLogActionsEntry : PXGraph<AppLogActionsEntry, FSLogActionMobileFilter>
    {
        public AppLogActionsEntry()
        {
        }

        #region Selects
        [PXCopyPasteHiddenView]
        public PXFilter<FSLogActionMobileFilter> Filter;

        // Start Travel/Service
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSAppointmentEmployeeFSLogStart,
               Where<
                   FSAppointmentEmployeeFSLogStart.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>>,
               OrderBy<
                   Desc<FSAppointmentEmployeeFSLogStart.selected>>> StaffMemberLogStartAction;

        // Start Staff and Service
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSStaffLogActionDetail,
               Where<
                   FSStaffLogActionDetail.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
                   And<
                       Where<
                           Current<FSLogActionMobileFilter.me>, Equal<True>,
                           And<FSStaffLogActionDetail.userID, Equal<Current<AccessInfo.userID>>,
                           Or<Current<FSLogActionMobileFilter.me>, Equal<False>>>>>>,
               OrderBy<
                   Desc<FSStaffLogActionDetail.selected>>> LogStaffActionDetails;

        // Start Service and Assigned Staff
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSDetailFSLogAction,
               Where<
                   FSDetailFSLogAction.appointmentID, Equal<Current<FSLogActionMobileFilter.appointmentID>>>> ServicesLogAction;

        // Complete Travel
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSLogActionTravelDetail,
               Where<
                   FSLogActionTravelDetail.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
                   And<
                       Where<
                           Current<FSLogActionMobileFilter.me>, Equal<True>,
                           And<FSLogActionTravelDetail.userID, Equal<Current<AccessInfo.userID>>,
                           Or<Current<FSLogActionMobileFilter.me>, Equal<False>>>>>>,
               OrderBy<
                   Desc<NoTravelLogInProcess.selected>>> LogActionTravelDetails;

        // Complete Service
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<NoTravelLogInProcess,
               Where<
                   NoTravelLogInProcess.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
                   And<
                       Where<
                           Current<FSLogActionMobileFilter.me>, Equal<True>,
                           And<NoTravelLogInProcess.userID, Equal<Current<AccessInfo.userID>>,
                           Or<Current<FSLogActionMobileFilter.me>, Equal<False>>>>>>,
               OrderBy<
                   Desc<NoTravelLogInProcess.selected>>> LogActionServiceDetails;
        #endregion

        #region Methods
        public virtual void PressActionButton(AppointmentEntry graph, string action, string type)
        {
            if (action == ID.LogActions.START)
            {
                if (type == ID.Type_Log.SERVICE)
                {
                    graph.startItemLine.PressButton();
                }
                else if (type == ID.Type_Log.TRAVEL)
                {
                    graph.startTravel.PressButton();
                }
                else if (type == ID.Type_Log.STAFF_ASSIGMENT)
                {
                    graph.startStaff.PressButton();
                }
                else if (type == ID.Type_Log.SERV_BASED_ASSIGMENT)
                {
                    graph.startItemLine.PressButton();
                }
            }
            else if (action == ID.LogActions.COMPLETE)
            {
                if (type == ID.Type_Log.SERVICE)
                {
                    graph.completeItemLine.PressButton();
                }
                else if (type == ID.Type_Log.TRAVEL)
                {
                    graph.completeTravel.PressButton();
                }
            }

            throw new PXRedirectRequiredException(graph, string.Empty);
        }
        #endregion

        #region Actions
        public PXAction<FSLogActionMobileFilter> process;
        [PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable Process(PXAdapter adapter)
        {
            if (Filter.Current != null 
                && Filter.Current.AppointmentID != null)
            {
                AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>(Filter.Current.AppointmentID, Filter.Current.SrvOrdType);
                graphAppointment.SkipLongOperation = this.IsMobile;
                graphAppointment.IsMobile = this.IsMobile;

                graphAppointment.LogActionFilter.Current = Filter.Current;
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;

                if (Filter.Current.Me == true)
                {
                    PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
                }
                else
                {
                    throw new PXRowPersistingException(null, null, TX.Error.CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED);
                }
            }

            return adapter.Get();
        }

        public PXAction<FSLogActionMobileFilter> processDetail;
        [PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable ProcessDetail(PXAdapter adapter)
        {
            if (Filter.Current != null
                && Filter.Current.AppointmentID != null)
            {
                AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>(Filter.Current.AppointmentID, Filter.Current.SrvOrdType);
                graphAppointment.SkipLongOperation = this.IsMobile;
                graphAppointment.IsMobile = this.IsMobile;

                if (Filter.Current.Me == true)
                {
                    Filter.Current.Me = false;
                }

                graphAppointment.LogActionFilter.Current = Filter.Current;
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;

                bool isThereARecordSelected = false;

                if (Filter.Current.Action == ID.LogActions.START)
                {
                    if (Filter.Current.Type == ID.Type_Log.SERVICE || Filter.Current.Type == ID.Type_Log.TRAVEL)
                    {
                        FSAppointmentEmployeeFSLogStart selectedItem = StaffMemberLogStartAction.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSAppointmentEmployeeFSLogStart row in graphAppointment.StaffMemberLogStartAction
                                                                                            .Select()
                                                                                            .RowCast<FSAppointmentEmployeeFSLogStart>()
                                                                                            .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.StaffMemberLogStartAction.Update(row);
                            }

                            FSAppointmentEmployeeFSLogStart rowInApp = graphAppointment.StaffMemberLogStartAction
                                                                                       .Select()
                                                                                       .RowCast<FSAppointmentEmployeeFSLogStart>()
                                                                                       .Where(x => x.DocID == selectedItem?.DocID && x.BAccountID == selectedItem?.BAccountID)
                                                                                       .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.StaffMemberLogStartAction.Update(rowInApp);
                            }
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.STAFF_ASSIGMENT)
                    {
                        FSStaffLogActionDetail selectedItem = LogStaffActionDetails.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSStaffLogActionDetail row in graphAppointment.LogStaffActionDetails
                                                                                   .Select()
                                                                                   .RowCast<FSStaffLogActionDetail>()
                                                                                   .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.LogStaffActionDetails.Update(row);
                            }

                            FSStaffLogActionDetail rowInApp = graphAppointment.LogStaffActionDetails
                                                                              .Select()
                                                                              .RowCast<FSStaffLogActionDetail>()
                                                                              .Where(x => x.DocID == selectedItem?.DocID && x.LineRef == selectedItem?.LineRef)
                                                                              .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.LogStaffActionDetails.Update(rowInApp);
                            }
                        }
                        else
                        {
                            FSStaffLogActionDetail row = graphAppointment.LogStaffActionDetails
                                                                         .Select()
                                                                         .RowCast<FSStaffLogActionDetail>()
                                                                         .Where(_ => _.LineRef == Filter.Current.EmployeeLineRef)
                                                                         .FirstOrDefault();
                            if (row != null)
                            {
                                row.Selected = true;

                                if (LogStaffActionDetails.Cache.GetStatus(row) == PXEntryStatus.Notchanged)
                                {
                                    LogStaffActionDetails.Cache.SetStatus(row, PXEntryStatus.Updated);
                                }
                            }
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.SERV_BASED_ASSIGMENT)
                    {
                        FSDetailFSLogAction selectedItem = ServicesLogAction.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSDetailFSLogAction row in graphAppointment.ServicesLogAction
                                                                                .Select()
                                                                                .RowCast<FSDetailFSLogAction>()
                                                                                .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.ServicesLogAction.Update(row);
                            }

                            FSDetailFSLogAction rowInApp = graphAppointment.ServicesLogAction
                                                                           .Select()
                                                                           .RowCast<FSDetailFSLogAction>()
                                                                           .Where(x => x.AppDetID == selectedItem?.AppDetID && x.LineRef == selectedItem?.LineRef)
                                                                           .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.ServicesLogAction.Update(rowInApp);
                            }
                        }
                    }

                    if (isThereARecordSelected)
                    {
                        PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
                    }
                    else
                    {
                        throw new PXRowPersistingException(null, null, TX.Error.CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED);
                    }
                }
                else if (Filter.Current.Action == ID.LogActions.COMPLETE)
                {
                    if (Filter.Current.Type == ID.Type_Log.SERVICE)
                    {
                        NoTravelLogInProcess selectedItem = LogActionServiceDetails.Current;

                        if (selectedItem != null)
                        {
                            foreach (NoTravelLogInProcess row in graphAppointment.LogActionServiceDetails
                                                                                 .Select()
                                                                                 .RowCast<NoTravelLogInProcess>()
                                                                                 .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.LogActionServiceDetails.Update(row);
                            }

                            NoTravelLogInProcess rowInApp = graphAppointment.LogActionServiceDetails
                                                                            .Select()
                                                                            .RowCast<NoTravelLogInProcess>()
                                                                            .Where(x => x.DocID == selectedItem?.DocID && x.LineRef == selectedItem?.LineRef)
                                                                            .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.LogActionServiceDetails.Update(rowInApp);
                            }
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.TRAVEL)
                    {
                        FSLogActionTravelDetail selectedItem = LogActionTravelDetails.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSLogActionTravelDetail row in graphAppointment.LogActionTravelDetails
                                                                                    .Select()
                                                                                    .RowCast<FSLogActionTravelDetail>()
                                                                                    .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.LogActionTravelDetails.Update(row);
                            }

                            FSLogActionTravelDetail rowInApp = graphAppointment.LogActionTravelDetails
                                                                               .Select()
                                                                               .RowCast<FSLogActionTravelDetail>()
                                                                               .Where(x => x.DocID == selectedItem?.DocID && x.LineRef == selectedItem?.LineRef)
                                                                               .FirstOrDefault();
                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.LogActionTravelDetails.Update(rowInApp);
                            }
                        }
                    }

                    if (isThereARecordSelected)
                    {
                        PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
                    }
                    else
                    {
                        throw new PXRowPersistingException(null, null, TX.Error.CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED);
                    }
                }
            }

            return adapter.Get();
        }

        public PXAction<FSLogActionMobileFilter> processAllRecords;
        [PXUIField(DisplayName = "Process All", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable ProcessAllRecords(PXAdapter adapter)
        {
            if (Filter.Current != null
                && Filter.Current.AppointmentID != null)
            {
                AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>(Filter.Current.AppointmentID, Filter.Current.SrvOrdType);
                graphAppointment.SkipLongOperation = this.IsMobile;
                graphAppointment.IsMobile = this.IsMobile;

                if (Filter.Current.Me == true)
                {
                    Filter.Current.Me = false;
                }

                graphAppointment.LogActionFilter.Current = Filter.Current;
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;

                if (Filter.Current.Action == ID.LogActions.START)
                {
                    if (Filter.Current.Type == ID.Type_Log.SERVICE || Filter.Current.Type == ID.Type_Log.TRAVEL)
                    {
                        foreach (FSAppointmentEmployeeFSLogStart row in graphAppointment.StaffMemberLogStartAction.Select()
                                                                                        .RowCast<FSAppointmentEmployeeFSLogStart>())
                        {
                            row.Selected = true;
                            graphAppointment.StaffMemberLogStartAction.Update(row);
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.STAFF_ASSIGMENT)
                    {
                        foreach (FSStaffLogActionDetail row in graphAppointment.LogStaffActionDetails.Select()
                                                                               .RowCast<FSStaffLogActionDetail>())
                        {
                            row.Selected = true;
                            graphAppointment.LogStaffActionDetails.Update(row);
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.SERV_BASED_ASSIGMENT)
                    {
                        foreach (FSDetailFSLogAction row in graphAppointment.ServicesLogAction.Select()
                                                                            .RowCast<FSDetailFSLogAction>())
                        {
                            row.Selected = true;
                            graphAppointment.ServicesLogAction.Update(row);
                        }
                    }

                    PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
                }
                else if (Filter.Current.Action == ID.LogActions.COMPLETE)
                {
                    if (Filter.Current.Type == ID.Type_Log.SERVICE)
                    {
                        foreach (NoTravelLogInProcess row in graphAppointment.LogActionServiceDetails.Select()
                                                                             .RowCast<NoTravelLogInProcess>())
                        {
                            row.Selected = true;
                            graphAppointment.LogActionServiceDetails.Update(row);
                        }
                    }
                    else if (Filter.Current.Type == ID.Type_Log.TRAVEL)
                    {
                        foreach (FSLogActionTravelDetail row in graphAppointment.LogActionTravelDetails.Select()
                                                                                .RowCast<FSLogActionTravelDetail>())
                        {
                            row.Selected = true;
                            graphAppointment.LogActionTravelDetails.Update(row);
                        }
                    }

                    PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
                }
            }

            return adapter.Get();
        }

        public PXAction<FSLogActionMobileFilter> processAllForMeRecords;
        [PXUIField(DisplayName = "Process All for Me", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXProcessButton]
        public virtual IEnumerable ProcessAllForMeRecords(PXAdapter adapter)
        {
            if (Filter.Current != null
                && Filter.Current.AppointmentID != null)
            {
                AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>(Filter.Current.AppointmentID, Filter.Current.SrvOrdType);
                graphAppointment.SkipLongOperation = this.IsMobile;
                graphAppointment.IsMobile = this.IsMobile;

                graphAppointment.LogActionFilter.Current = (FSLogActionFilter)Filter.Cache.CreateCopy(Filter.Current);
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;
                graphAppointment.LogActionFilter.Cache.SetValueExt<FSLogActionFilter.me>(graphAppointment.LogActionFilter.Current, true);

                PressActionButton(graphAppointment, Filter.Current.Action, Filter.Current.Type);
            }

            return adapter.Get();
        }
        #endregion

        #region Events
        #region FSLogActionMobileFilter
        protected virtual void _(Events.RowSelected<FSLogActionMobileFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSLogActionMobileFilter fsLogActionMobileFilterRow = e.Row;

            if (fsLogActionMobileFilterRow.LogTime == null)
            {
                fsLogActionMobileFilterRow.LogTime = SharedFunctions.GetTimeWithSpecificDate(PXTimeZoneInfo.Now, Accessinfo.BusinessDate, true);
            }
        }
        #endregion
        #endregion
    }
}
