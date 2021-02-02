using PX.Common;
using PX.Data;
using System;
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
        public PXSelect<FSAppointmentStaffDistinct,
               Where<
                   FSAppointmentStaffDistinct.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>>,
               OrderBy<
                   Desc<FSAppointmentStaffDistinct.selected>>> LogActionStaffDistinctRecords;

        // Start Staff and Service
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSAppointmentStaffExtItemLine,
               Where<
                   FSAppointmentStaffExtItemLine.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
                   And<
                       Where<
                           Current<FSLogActionMobileFilter.me>, Equal<True>,
                           And<FSAppointmentStaffExtItemLine.userID, Equal<Current<AccessInfo.userID>>,
                           Or<Current<FSLogActionMobileFilter.me>, Equal<False>>>>>>,
               OrderBy<
                   Desc<FSAppointmentStaffExtItemLine.selected>>> LogActionStaffRecords;

        // Start Service and Assigned Staff
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSDetailFSLogAction,
               Where<
                   FSDetailFSLogAction.appointmentID, Equal<Current<FSLogActionMobileFilter.appointmentID>>>> ServicesLogAction;

        // Complete/Resume Travel/Service
        [PXFilterable]
        [PXCopyPasteHiddenView]
        public PXSelect<FSAppointmentLogExtItemLine,
            Where2<
                Where<
                   FSAppointmentLogExtItemLine.docID, Equal<Current<FSLogActionMobileFilter.appointmentID>>,
                And<
                    Where<Current<FSLogActionMobileFilter.me>, Equal<False>,
                    Or<FSAppointmentLogExtItemLine.userID, Equal<Current<AccessInfo.userID>>>>>>,
            And2<
                Where2<
                        Where<Current<FSLogActionMobileFilter.type>, Equal<FSLogTypeAction.Travel>,
                        And<FSAppointmentLogExtItemLine.itemType, Equal<ListField_LogAction_Type.travel>>>,
                    Or<
                        Where<Current<FSLogActionMobileFilter.type>, NotEqual<FSLogTypeAction.Travel>,
                        And<FSAppointmentLogExtItemLine.itemType, NotEqual<ListField_LogAction_Type.travel>>>>>,
                And<
                    Where2<
                        Where<
                            Current<FSLogActionMobileFilter.action>, Equal<FSLogActionMobileFilter.action.Pause>,
                            And<FSAppointmentLogExtItemLine.status, Equal<FSAppointmentLogExtItemLine.status.InProcess>>>,
                        Or<
                            Where2<
                                Where<
                                    Current<FSLogActionMobileFilter.action>, Equal<FSLogActionMobileFilter.action.Resume>,
                                    And<FSAppointmentLogExtItemLine.status, Equal<FSAppointmentLogExtItemLine.status.Paused>>>,
                                Or<
                                    Where<
                                        Current<FSLogActionMobileFilter.action>, Equal<FSLogActionMobileFilter.action.Complete>,
                                        And<
                                            Where<FSAppointmentLogExtItemLine.status, Equal<FSAppointmentLogExtItemLine.status.InProcess>,
                                            Or<FSAppointmentLogExtItemLine.status, Equal<FSAppointmentLogExtItemLine.status.Paused>>>>>>>>>>>>,
            OrderBy<
                   Desc<FSAppointmentLogExtItemLine.selected>>> LogActionLogRecords;
        #endregion

        #region Methods
        public virtual void PressActionButton(AppointmentEntry graph, string action, string type)
        {
            if (action == ID.LogActions.START)
            {
                if (type == FSLogActionMobileFilter.type.Values.Service)
                {
                    graph.startItemLine.PressButton();
                }
                else if (type == FSLogActionMobileFilter.type.Values.Travel)
                {
                    graph.startTravel.PressButton();
                }
                else if (type == FSLogActionMobileFilter.type.Values.Staff)
                {
                    graph.startStaff.PressButton();
                }
                else if (type == FSLogActionMobileFilter.type.Values.ServBasedAssignment)
                {
                    graph.startItemLine.PressButton();
                }
            }
            else if (action == ID.LogActions.PAUSE)
            {
                graph.pauseItemLine.PressButton();
            }
            else if (action == ID.LogActions.RESUME)
            {
                graph.resumeItemLine.PressButton();
            }
            else if (action == ID.LogActions.COMPLETE)
            {
                if (type == FSLogActionMobileFilter.type.Values.Service)
                {
                    graph.completeItemLine.PressButton();
                }
                else if (type == FSLogActionMobileFilter.type.Values.Travel)
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

                if (Filter.Current.Me == true)
                {
                    Filter.Current.Me = false;
                }

                graphAppointment.LogActionFilter.Current = Filter.Current;
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;

                bool isThereARecordSelected = false;

                if (Filter.Current.Action == ID.LogActions.START)
                {
                    if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.Service || Filter.Current.Type == FSLogActionMobileFilter.type.Values.Travel)
                    {
                        FSAppointmentStaffDistinct selectedItem = LogActionStaffDistinctRecords.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSAppointmentStaffDistinct row in graphAppointment.LogActionStaffDistinctRecords
                                                                                            .Select()
                                                                                            .RowCast<FSAppointmentStaffDistinct>()
                                                                                            .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.LogActionStaffDistinctRecords.Update(row);
                            }

                            FSAppointmentStaffDistinct rowInApp = graphAppointment.LogActionStaffDistinctRecords
                                                                                       .Select()
                                                                                       .RowCast<FSAppointmentStaffDistinct>()
                                                                                       .Where(x => x.DocID == selectedItem?.DocID && x.BAccountID == selectedItem?.BAccountID)
                                                                                       .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.LogActionStaffDistinctRecords.Update(rowInApp);
                            }
                        }
                    }
                    else if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.Staff)
                    {
                        FSAppointmentStaffExtItemLine selectedItem = LogActionStaffRecords.Current;

                        if (selectedItem != null)
                        {
                            foreach (FSAppointmentStaffExtItemLine row in graphAppointment.LogActionStaffRecords
                                                                                   .Select()
                                                                                   .RowCast<FSAppointmentStaffExtItemLine>()
                                                                                   .Where(x => x.Selected == true))
                            {
                                row.Selected = false;
                                graphAppointment.LogActionStaffRecords.Update(row);
                            }

                            FSAppointmentStaffExtItemLine rowInApp = graphAppointment.LogActionStaffRecords
                                                                              .Select()
                                                                              .RowCast<FSAppointmentStaffExtItemLine>()
                                                                              .Where(x => x.DocID == selectedItem?.DocID && x.LineRef == selectedItem?.LineRef)
                                                                              .FirstOrDefault();

                            if (rowInApp != null)
                            {
                                isThereARecordSelected = true;
                                rowInApp.Selected = true;
                                graphAppointment.LogActionStaffRecords.Update(rowInApp);
                            }
                        }
                        else
                        {
                            FSAppointmentStaffExtItemLine row = graphAppointment.LogActionStaffRecords
                                                                         .Select()
                                                                         .RowCast<FSAppointmentStaffExtItemLine>()
                                                                         .Where(_ => _.LineRef == Filter.Current.EmployeeLineRef)
                                                                         .FirstOrDefault();
                            if (row != null)
                            {
                                row.Selected = true;
                                LogActionStaffRecords.Update(row);
                            }
                        }
                    }
                    else if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.ServBasedAssignment)
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
                else if (Filter.Current.Action == ID.LogActions.PAUSE || Filter.Current.Action == ID.LogActions.COMPLETE || Filter.Current.Action == ID.LogActions.RESUME)
                {
                    FSAppointmentLogExtItemLine selectedItem = LogActionLogRecords.Current;

                    if (selectedItem != null)
                    {
                        foreach (FSAppointmentLogExtItemLine row in graphAppointment.LogActionLogRecords
                                                                            .Select()
                                                                            .RowCast<FSAppointmentLogExtItemLine>()
                                                                            .Where(x => x.Selected == true))
                        {
                            row.Selected = false;
                            graphAppointment.LogActionLogRecords.Update(row);
                        }

                        FSAppointmentLogExtItemLine rowInApp = graphAppointment.LogActionLogRecords
                                                                       .Select()
                                                                       .RowCast<FSAppointmentLogExtItemLine>()
                                                                       .Where(x => x.DocID == selectedItem?.DocID && x.LineRef == selectedItem?.LineRef)
                                                                       .FirstOrDefault();

                        if (rowInApp != null)
                        {
                            isThereARecordSelected = true;
                            rowInApp.Selected = true;
                            graphAppointment.LogActionLogRecords.Update(rowInApp);
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

                if (Filter.Current.Me == true)
                {
                    Filter.Current.Me = false;
                }

                graphAppointment.LogActionFilter.Current = Filter.Current;
                graphAppointment.LogActionFilter.View.Answer = WebDialogResult.OK;

                if (Filter.Current.Action == ID.LogActions.START)
                {
                    if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.Service || Filter.Current.Type == FSLogActionMobileFilter.type.Values.Travel)
                    {
                        PXResultset<FSAppointmentStaffDistinct> rows = graphAppointment.LogActionStaffDistinctRecords.Select();

                        if (rows.Count == 0)
                        {
                            graphAppointment.LogActionFilter.Cache.SetValueExt<FSLogActionFilter.me>(graphAppointment.LogActionFilter.Current, true);
                        }
                        else
                        {
                            foreach (FSAppointmentStaffDistinct row in graphAppointment.LogActionStaffDistinctRecords.Select()
                                                                                            .RowCast<FSAppointmentStaffDistinct>())
                            {
                                row.Selected = true;
                                graphAppointment.LogActionStaffDistinctRecords.Update(row);
                            }
                        }
                    }
                    else if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.Staff)
                    {
                        foreach (FSAppointmentStaffExtItemLine row in graphAppointment.LogActionStaffRecords.Select()
                                                                               .RowCast<FSAppointmentStaffExtItemLine>())
                        {
                            row.Selected = true;
                            graphAppointment.LogActionStaffRecords.Update(row);
                        }
                    }
                    else if (Filter.Current.Type == FSLogActionMobileFilter.type.Values.ServBasedAssignment)
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
                else if (Filter.Current.Action == ID.LogActions.COMPLETE || Filter.Current.Action == ID.LogActions.PAUSE || Filter.Current.Action == ID.LogActions.RESUME)
                {
                    foreach (FSAppointmentLogExtItemLine row in graphAppointment.LogActionLogRecords.Select().RowCast<FSAppointmentLogExtItemLine>())
                    {
                        row.Selected = true;
                        graphAppointment.LogActionLogRecords.Update(row);
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

            if (fsLogActionMobileFilterRow.LogDateTime == null)
            {
                fsLogActionMobileFilterRow.LogDateTime = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate, PXTimeZoneInfo.Now);
            }
        }
        #endregion
        #endregion
    }

    public class AppStartTravelServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Events
        protected override void _(Events.RowSelected<FSLogActionMobileFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            base._(e);

            FSLogActionMobileFilter filterRow = e.Row;

            FSLogTypeAction.SetLineTypeList<FSLogActionMobileFilter.type>(e.Cache, filterRow, string.Empty);
        }
        #endregion
    }

    public class AppStartStaffAndServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Type
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionMobileFilter.type.Values.Staff)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region DetLineRef
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDBString(4, IsFixed = true, BqlField = typeof(FSAppointmentDet.lineRef))]
        [PXUIField(DisplayName = "Details Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        protected virtual void FSAppointmentStaffExtItemLine_DetLineRef_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }

    public class AppStartServiceAssignedStaffEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Type
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionMobileFilter.type.Values.ServBasedAssignment)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }

    public class AppCompleteServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region FSLogActionMobileFilter_Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.COMPLETE)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_Type
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionMobileFilter.type.Values.Service)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_LogStatus
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUnboundDefault(ID.Status_Log.IN_PROCESS)]
        protected virtual void FSLogActionMobileFilter_LogStatus_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }

    public class AppCompleteTravelEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region FSLogActionMobileFilter_Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.COMPLETE)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_Type
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Logging", Enabled = false)]
        [PXUnboundDefault(FSLogActionMobileFilter.type.Values.Travel)]
        protected virtual void FSLogActionMobileFilter_Type_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_LogStatus
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUnboundDefault(ID.Status_Log.IN_PROCESS)]
        protected virtual void FSLogActionMobileFilter_LogStatus_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }

    public class AppPauseServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region FSLogActionMobileFilter_Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.PAUSE)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_LogStatus
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUnboundDefault(ID.Status_Log.IN_PROCESS)]
        protected virtual void FSLogActionMobileFilter_LogStatus_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }

    public class AppResumeServiceEntry : AppLogActionsEntry
    {
        #region CacheAttached
        #region FSLogActionMobileFilter_Action
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Action", Enabled = false)]
        [PXUnboundDefault(ID.LogActions.RESUME)]
        protected virtual void FSLogActionMobileFilter_Action_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSLogActionMobileFilter_LogStatus
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUnboundDefault(ID.Status_Log.PAUSED)]
        protected virtual void FSLogActionMobileFilter_LogStatus_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion
    }
}
