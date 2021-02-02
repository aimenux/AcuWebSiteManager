using PX.Data;
using System;
using System.Collections;

namespace PX.Objects.FS
{
    public class CloneAppointmentProcess : PXGraph<CloneAppointmentProcess>
    {
        PXGraph dummyGraph;
        public CloneAppointmentProcess() : base()
        {
            if (dummyGraph == null)
            {
                dummyGraph = new PXGraph();
            }

            FieldUpdating.AddHandler(
                    typeof(FSCloneAppointmentFilter),
                    typeof(FSCloneAppointmentFilter.scheduledEndTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                    FSCloneAppointmentFilter_ScheduledEndTime_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                    typeof(FSCloneAppointmentFilter),
                    typeof(FSCloneAppointmentFilter.scheduledStartTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                    FSCloneAppointmentFilter_ScheduledStartTime_Time_FieldUpdating);
        }

        #region Selects

        public PXFilter<FSCloneAppointmentFilter> Filter;
        public PXAction<FSCloneAppointmentFilter> cancel;
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXSelectJoin<FSAppointment,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
                Where<
                    FSAppointment.srvOrdType, Equal<Current<FSCloneAppointmentFilter.srvOrdType>>,
                    And<FSAppointment.refNbr, Equal<Current<FSCloneAppointmentFilter.refNbr>>>>> AppointmentSelected;


        public PXSelectReadonly<FSAppointmentFSServiceOrder,
                   Where<
                FSAppointmentFSServiceOrder.originalAppointmentID, Equal<Current<FSCloneAppointmentFilter.appointmentID>>>,
                OrderBy<
                    Desc<FSAppointmentFSServiceOrder.appointmentID>>> AppointmentClones;

        public AppointmentCore.ServiceOrderRelated_View ServiceOrderRelated;

        #endregion

        #region CacheAttached
        #region FSAppointment_RefNbr
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = false)]
        protected virtual void FSAppointment_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_SORefNbr
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.", Enabled = false)]
        protected virtual void FSAppointment_SORefNbr_CacheAttached(PXCache sender)
        { 
        }
        #endregion
        #region FSAppointment_SrvOrdType
        [PXDBString(4, IsFixed = true, IsKey = true)]
        [PXUIField(DisplayName = "Service Order Type", Enabled = false)]
        protected virtual void FSAppointment_SrvOrdType_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSAppointment_Confirmed
        [PXDBBool]
        [PXUIField(DisplayName = "Confirmed", Enabled = false)]
        protected virtual void FSAppointment_Confirmed_CacheAttached(PXCache sender) 
        {
        }
        #endregion
        #endregion

        #region Actions

        #region OpenAppointment
        public PXAction<FSCloneAppointmentFilter> OpenAppointment;
        [PXButton]
        [PXUIField(Visible = false)]
        protected void openAppointment()
        {
            AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();
            if (AppointmentClones.Current != null && AppointmentClones.Current.RefNbr != null)
            {
                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.refNbr>
                    (AppointmentClones.Current.RefNbr, AppointmentClones.Current.SrvOrdType);

                throw new PXRedirectRequiredException(graphAppointment, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        public PXAction<FSCloneAppointmentFilter> clone;
        [PXUIField(DisplayName = "Clone Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable Clone(PXAdapter adapter)
        {
            if (this.Filter.Current.ScheduledEndTime == null)
            {
                Filter.Cache.RaiseExceptionHandling<FSCloneAppointmentFilter.scheduledEndTime>(
                    Filter.Current,
                    null,
                    new PXSetPropertyException(
                        PXMessages.LocalizeFormatNoPrefix(
                            TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                            PXUIFieldAttribute.GetDisplayName<FSCloneAppointmentFilter.scheduledEndTime>(Filter.Cache)), 
                        PXErrorLevel.Error));

                return adapter.Get();
            }

            if (this.Filter.Current.CloningType == ID.CloningType_CloneAppointment.MULTIPLE)
            {
                if (this.Filter.Current.ScheduledFromDate == null)
                {
                    Filter.Cache.RaiseExceptionHandling<FSCloneAppointmentFilter.scheduledFromDate>(
                        Filter.Current,
                        null,
                        new PXSetPropertyException(
                            PXMessages.LocalizeFormatNoPrefix(
                                TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                PXUIFieldAttribute.GetDisplayName<FSCloneAppointmentFilter.scheduledFromDate>(Filter.Cache)),
                            PXErrorLevel.Error));

                    return adapter.Get();
                }

                if (this.Filter.Current.ScheduledToDate == null)
                {
                    Filter.Cache.RaiseExceptionHandling<FSCloneAppointmentFilter.scheduledToDate>(
                        Filter.Current,
                        null,
                        new PXSetPropertyException(
                            PXMessages.LocalizeFormatNoPrefix(
                                TX.Error.FIELD_MAY_NOT_BE_EMPTY,
                                PXUIFieldAttribute.GetDisplayName<FSCloneAppointmentFilter.scheduledToDate>(Filter.Cache)),
                            PXErrorLevel.Error));

                    return adapter.Get();
                }
            }

            FSServiceOrder fsServiceOrderRow = PXSelectJoin<FSServiceOrder,
                                               InnerJoin<
                                                   FSAppointment, On<FSAppointment.sOID, Equal<FSServiceOrder.sOID>>>,
                                               Where<
                                                   FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                               .Select(this, this.Filter.Current.AppointmentID);

            if (fsServiceOrderRow != null 
                    && fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED)
            {
                throw new PXException(TX.Error.CANNOT_CLONE_APPOINMENT_SERVICE_ORDER_COMPLETED);
            }

            CloneAppointmentProcess graphCloneAppointmentProcess = PXGraph.CreateInstance<CloneAppointmentProcess>();

            graphCloneAppointmentProcess.Filter.Current.SrvOrdType = this.Filter.Current.SrvOrdType;
            graphCloneAppointmentProcess.Filter.Current.RefNbr = this.Filter.Current.RefNbr;
            graphCloneAppointmentProcess.Filter.Current.KeepTimeDuration = this.Filter.Current.KeepTimeDuration;
            graphCloneAppointmentProcess.Filter.Current.ScheduledDate = this.Filter.Current.ScheduledDate;
            graphCloneAppointmentProcess.Filter.Current.ScheduledStartTime = this.Filter.Current.ScheduledStartTime;
            graphCloneAppointmentProcess.Filter.Current.ScheduledEndTime = this.Filter.Current.ScheduledEndTime;

            graphCloneAppointmentProcess.Filter.Current.ScheduledFromDate = this.Filter.Current.ScheduledFromDate;
            graphCloneAppointmentProcess.Filter.Current.ScheduledToDate = this.Filter.Current.ScheduledToDate;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnMonday = this.Filter.Current.ActiveOnMonday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnThursday = this.Filter.Current.ActiveOnThursday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnWednesday = this.Filter.Current.ActiveOnWednesday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnTuesday = this.Filter.Current.ActiveOnTuesday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnFriday = this.Filter.Current.ActiveOnFriday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnSaturday = this.Filter.Current.ActiveOnSaturday;
            graphCloneAppointmentProcess.Filter.Current.ActiveOnSunday = this.Filter.Current.ActiveOnSunday;

            graphCloneAppointmentProcess.AppointmentSelected.Current = this.AppointmentSelected.Current;

            PXLongOperation.StartOperation(
                this,
                delegate
                {
                    using (PXTransactionScope ts = new PXTransactionScope())
                    {
                        if (this.Filter.Current.CloningType == ID.CloningType_CloneAppointment.SINGLE)
                        {
                            AppointmentEntry graphOriginalAppointment = PXGraph.CreateInstance<AppointmentEntry>();

                            graphOriginalAppointment.AppointmentSelected.Current = graphOriginalAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                        (AppointmentSelected.Current.AppointmentID, AppointmentSelected.Current.SrvOrdType);

                            graphCloneAppointmentProcess.CloneAppointment(graphOriginalAppointment, PXGraph.CreateInstance<AppointmentEntry>());
                        }

                        if (this.Filter.Current.CloningType == ID.CloningType_CloneAppointment.MULTIPLE)
                        {
                            graphCloneAppointmentProcess.CloneMultipleAppointments(graphCloneAppointmentProcess);
                        }

                        ts.Complete();
                    }
                });

            return adapter.Get();
        }

        [PXUIField(DisplayName = ActionsMessages.Cancel, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXCancelButton]
        protected virtual IEnumerable Cancel(PXAdapter adapter)
        {
            Filter.Current.ScheduledDate = null;
            Filter.Current.ScheduledEndTime = null;

            return new object[] { Filter.Current };
        }

        #endregion

        #region Private Functions

        protected virtual void CloneMultipleAppointments(CloneAppointmentProcess graph)
        {
            DateTime fromDate = (DateTime)graph.Filter.Current.ScheduledFromDate;
            DateTime toDate = (DateTime)graph.Filter.Current.ScheduledToDate;

            if (fromDate > toDate)
            {
                throw new ArgumentException(PX.Data.PXMessages.LocalizeFormatNoPrefix(TX.Error.END_DATE_LESSER_THAN_START_DATE));
            }

            AppointmentEntry graphOriginalAppointment = PXGraph.CreateInstance<AppointmentEntry>();

            graphOriginalAppointment.AppointmentSelected.Current = graphOriginalAppointment.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                        (AppointmentSelected.Current.AppointmentID, AppointmentSelected.Current.SrvOrdType);

            AppointmentEntry graphNewAppointment = PXGraph.CreateInstance<AppointmentEntry>();
            for (DateTime i = (DateTime)fromDate; i <= toDate; i = i.AddDays(1))
            {
                if ((i.DayOfWeek == DayOfWeek.Monday && (bool)graph.Filter.Current.ActiveOnMonday)
                    || (i.DayOfWeek == DayOfWeek.Thursday && (bool)graph.Filter.Current.ActiveOnThursday)
                    || (i.DayOfWeek == DayOfWeek.Wednesday && (bool)graph.Filter.Current.ActiveOnWednesday)
                    || (i.DayOfWeek == DayOfWeek.Tuesday && (bool)graph.Filter.Current.ActiveOnTuesday)
                    || (i.DayOfWeek == DayOfWeek.Friday && (bool)graph.Filter.Current.ActiveOnFriday)
                    || (i.DayOfWeek == DayOfWeek.Saturday && (bool)graph.Filter.Current.ActiveOnSaturday)
                    || (i.DayOfWeek == DayOfWeek.Sunday && (bool)graph.Filter.Current.ActiveOnSunday))
                {
                    graph.Filter.Current.ScheduledDate = i;
                    graph.CloneAppointment(graphOriginalAppointment, graphNewAppointment);
                }
            }
        }

        protected virtual void CloneAppointment(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            graphNewAppointment.Clear(PXClearOption.ClearAll);
            graphNewAppointment.clearLocalServiceOrder();

            FSAppointment fsAppointmentRow = PXCache<FSAppointment>.CreateCopy(AppointmentSelected.Current);

            //Clear key and special fields
            fsAppointmentRow.RefNbr = null;
            fsAppointmentRow.AppointmentID = null;
            fsAppointmentRow.NoteID = null;
            fsAppointmentRow.CuryInfoID = null;
            fsAppointmentRow.AgreementSignature = false;
            fsAppointmentRow.FullNameSignature = null;
            fsAppointmentRow.customerSignaturePath = null;
            fsAppointmentRow.BillServiceContractID = null;
            fsAppointmentRow.HandleManuallyActualTime = null;
            fsAppointmentRow.HandleManuallyScheduleTime = null;
            fsAppointmentRow.FinPeriodID = null;
            fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.PENDING_TO_POST;
            fsAppointmentRow.PendingAPARSOPost = true;

            fsAppointmentRow.OriginalAppointmentID = AppointmentSelected.Current.AppointmentID;

            fsAppointmentRow.ScheduledDateTimeBegin = AppointmentCore.GetDateTimeEnd(
                                Filter.Current.ScheduledDate, Filter.Current.ScheduledStartTime);

            DateTime? scheduledDateTimeEnd = null;
            fsAppointmentRow.ScheduledDateTimeEnd =
                        scheduledDateTimeEnd =
                        AppointmentCore.GetDateTimeEnd(
                                Filter.Current.ScheduledDate, Filter.Current.ScheduledEndTime);

            fsAppointmentRow.ExecutionDate = fsAppointmentRow.ScheduledDateTimeBegin.Value.Date;

            fsAppointmentRow.CutOffDate
                = ServiceOrderCore.GetCutOffDate(graphNewAppointment, graphOriginalAppointment.ServiceOrderRelated.Current.CBID, fsAppointmentRow.ExecutionDate);

            fsAppointmentRow.Status = ID.Status_Appointment.MANUAL_SCHEDULED;
            fsAppointmentRow.Hold = false;

            fsAppointmentRow.AdditionalCommentsCustomer = null;
            fsAppointmentRow.AdditionalCommentsStaff = null;

            //++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //Clean total fields
            fsAppointmentRow.EstimatedDurationTotal = 0;
            fsAppointmentRow.ActualDurationTotal = 0;

            fsAppointmentRow.CuryEstimatedLineTotal = 0;
            fsAppointmentRow.CuryLineTotal = 0;
            fsAppointmentRow.CuryBillableLineTotal = 0;
            fsAppointmentRow.CuryCostTotal = 0;

            fsAppointmentRow.EstimatedLineTotal = 0;
            fsAppointmentRow.LineTotal = 0;
            fsAppointmentRow.BillableLineTotal = 0;
            fsAppointmentRow.CostTotal = 0;
            //----------------------------------------------------------------------

            graphNewAppointment.IsCloningAppointment = true;
            fsAppointmentRow = graphNewAppointment.AppointmentRecords.Insert(fsAppointmentRow);

            graphNewAppointment.AttributeListRecords.Current = graphNewAppointment.AttributeListRecords.Select();
            graphNewAppointment.Answers.Current = graphNewAppointment.Answers.Select();
            graphNewAppointment.Answers.CopyAllAttributes(graphNewAppointment.AppointmentRecords.Current, AppointmentSelected.Current);

            PXNoteAttribute.CopyNoteAndFiles(
                                            AppointmentSelected.Cache,
                                            AppointmentSelected.Current,
                                            graphNewAppointment.AppointmentSelected.Cache, 
                                            fsAppointmentRow,
                                            copyNotes: true, 
                                            copyFiles: false);


            this.CloneParts(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);
            this.CloneServices(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);
            this.CloneEmployees(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);
            this.CloneAttendees(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);
            this.CloneResources(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);

            if (graphNewAppointment.AppointmentRecords.Current.ScheduledDateTimeEnd != scheduledDateTimeEnd)
            {
                graphNewAppointment.AppointmentRecords.Cache.SetValueExt<FSAppointment.handleManuallyScheduleTime>(fsAppointmentRow, true);
                graphNewAppointment.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow, scheduledDateTimeEnd);
            }

            graphNewAppointment.AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsAppointmentRow);
            graphNewAppointment.Save.Press();
        }

        private void CloneServices(AppointmentEntry sourceAppointmentGraph, AppointmentEntry newAppointmentGraph, FSAppointment newAppointmentRow)
        {
            foreach (FSAppointmentDetService sourceRow in sourceAppointmentGraph.AppointmentDetServices.Select())
            {
                FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(sourceAppointmentGraph, sourceRow);

                if (fsSODetRow != null
                        && (fsSODetRow.Status == ID.Status_AppointmentDet.CANCELED
                                || fsSODetRow.Status == ID.Status_AppointmentDet.COMPLETED))
                {
                    continue;
                }

                FSAppointmentDetService newRow = PXCache<FSAppointmentDetService>.CreateCopy(sourceRow);

                newRow.ActualDuration = 0;
                newRow.ActualDateTimeBegin = null;
                newRow.ActualDateTimeEnd = null;
                newRow.Qty = 0;
                newRow.PostID = null;

                newRow = AppointmentEntry.InsertServicePartLine<FSAppointmentDetService, FSAppointmentDetService>(
                                                            newAppointmentGraph.AppointmentDetServices.Cache,
                                                            newRow,
                                                            sourceAppointmentGraph.AppointmentDetServices.Cache,
                                                            sourceRow,
                                                            null,
                                                            sourceRow.SODetID,
                                                            copyTranDate: false,
                                                            tranDate: sourceRow.TranDate,
                                                            SetValuesAfterAssigningSODetID: true,
                                                            copyingFromQuote: false);

                PXNoteAttribute.CopyNoteAndFiles(
                                                sourceAppointmentGraph.AppointmentDetServices.Cache,
                                                sourceRow,
                                                newAppointmentGraph.AppointmentDetServices.Cache,
                                                newRow,
                                                copyNotes: true,
                                                copyFiles: false);


                newAppointmentGraph.AppointmentDetServices.SetValueExt<FSAppointmentDetService.acctID>(newRow, sourceRow.AcctID);
                newAppointmentGraph.AppointmentDetServices.SetValueExt<FSAppointmentDetService.subID>(newRow, sourceRow.SubID);
            }
        }

        private void CloneParts(AppointmentEntry sourceAppointmentGraph, AppointmentEntry newAppointmentGraph, FSAppointment newAppointmentRow)
        {
            foreach (FSAppointmentDetPart sourceRow in sourceAppointmentGraph.AppointmentDetParts.Select())
            {
                FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(sourceAppointmentGraph, sourceRow);

                if (fsSODetRow != null
                        && (fsSODetRow.Status == ID.Status_AppointmentDet.CANCELED
                                || fsSODetRow.Status == ID.Status_AppointmentDet.COMPLETED))
                {
                    continue;
                }

                FSAppointmentDet sumFSAppointmentDetPartBillable = 
                    PXSelectJoinGroupBy<FSAppointmentDet,
                    InnerJoin<FSAppointment, On<FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                    And<FSAppointment.refNbr, Equal<FSAppointmentDetPart.refNbr>>>>,
                    Where<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                    And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>>>>,
                    Aggregate<GroupBy<FSAppointmentDet.sODetID, Sum<FSAppointmentDet.billableQty>>>>
                    .Select(dummyGraph, sourceRow.SODetID);

                decimal? openQty = fsSODetRow.BillableQty - sumFSAppointmentDetPartBillable.BillableQty;
                bool? lineCloned = false;

                FSAppointmentDetPart newRow = PXCache<FSAppointmentDetPart>.CreateCopy(sourceRow);
                newRow.PostID = null;
                if (openQty >= sourceRow.BillableQty)
                {
                    newRow = AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSAppointmentDetPart>(
                                                                newAppointmentGraph.AppointmentDetParts.Cache,
                                                                newRow,
                                                                sourceAppointmentGraph.AppointmentDetParts.Cache,
                                                                sourceRow,
                                                                null,
                                                                sourceRow.SODetID,
                                                                copyTranDate: false,
                                                                tranDate: sourceRow.TranDate,
                                                                SetValuesAfterAssigningSODetID: true,
                                                                copyingFromQuote: false);
                    lineCloned = true;
                }
                else
                {
                    if (openQty > 0)
                    {
                        decimal? remainingQty = sourceRow.BillableQty - openQty;

                        newRow.Qty = 0;
                        sourceRow.EstimatedQty = openQty;
                        sourceRow.BillableQty = openQty;
                        newRow = AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSAppointmentDetPart>(
                                                            newAppointmentGraph.AppointmentDetParts.Cache,
                                                            newRow,
                                                            sourceAppointmentGraph.AppointmentDetParts.Cache,
                                                            sourceRow,
                                                            null,
                                                            sourceRow.SODetID,
                                                            copyTranDate: false,
                                                            tranDate: sourceRow.TranDate,
                                                            SetValuesAfterAssigningSODetID: true,
                                                            copyingFromQuote: false);
                        lineCloned = true;

                        FSAppointmentDetPart secondNewRow = PXCache<FSAppointmentDetPart>.CreateCopy(sourceRow);

                        secondNewRow.Qty = 0;
                        sourceRow.EstimatedQty = remainingQty;
                        sourceRow.BillableQty = remainingQty;
                        secondNewRow.SODetID = null;
                        secondNewRow = AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSAppointmentDetPart>(
                                                                newAppointmentGraph.AppointmentDetParts.Cache,
                                                                secondNewRow,
                                                                sourceAppointmentGraph.AppointmentDetParts.Cache,
                                                                sourceRow,
                                                                null,
                                                                null,
                                                                copyTranDate: false,
                                                                tranDate: sourceRow.TranDate,
                                                                SetValuesAfterAssigningSODetID: false,
                                                                copyingFromQuote: false);

                    }
                    else
                    {
                        newRow.SODetID = null;
                        newRow.Qty = 0;
                        newRow = AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSAppointmentDetPart>(
                                                                newAppointmentGraph.AppointmentDetParts.Cache,
                                                                newRow,
                                                                sourceAppointmentGraph.AppointmentDetParts.Cache,
                                                                sourceRow,
                                                                null,
                                                                null,
                                                                copyTranDate: false,
                                                                tranDate: sourceRow.TranDate,
                                                                SetValuesAfterAssigningSODetID: false,
                                                                copyingFromQuote: false);
                    }
                }


                if (lineCloned == true)
                {
                    PXNoteAttribute.CopyNoteAndFiles(
                                                    sourceAppointmentGraph.AppointmentDetParts.Cache,
                                                    sourceRow,
                                                    newAppointmentGraph.AppointmentDetParts.Cache,
                                                    newRow,
                                                    copyNotes: true,
                                                    copyFiles: false);
                }
            }
        }

        private void CloneEmployees(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow)
        {
            foreach (FSAppointmentEmployee originalAppointmentEmployee in graphOriginalAppointment.AppointmentEmployees.Select())
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = PXCache<FSAppointmentEmployee>.CreateCopy(originalAppointmentEmployee);

                fsAppointmentEmployeeRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentEmployeeRow.RefNbr = null;
                fsAppointmentEmployeeRow.ActualDuration = null;
                fsAppointmentEmployeeRow.ActualDateTimeBegin = null;
                fsAppointmentEmployeeRow.ActualDateTimeEnd = null;

                graphNewAppointment.AppointmentEmployees.Insert(fsAppointmentEmployeeRow);
            }
        }

        private void CloneAttendees(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow)
        {
            foreach (FSAppointmentAttendee originalAppointmentAttendee in graphOriginalAppointment.AppointmentAttendees.Select())
            {
                FSAppointmentAttendee fsAppointmentAttendeeRow = PXCache<FSAppointmentAttendee>.CreateCopy(originalAppointmentAttendee);

                fsAppointmentAttendeeRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentAttendeeRow.RefNbr = null;
                fsAppointmentAttendeeRow.NoteID = null;
                fsAppointmentAttendeeRow = graphNewAppointment.AppointmentAttendees.Insert(fsAppointmentAttendeeRow);

                PXNoteAttribute.CopyNoteAndFiles(
                                                graphOriginalAppointment.AppointmentAttendees.Cache,
                                                originalAppointmentAttendee,
                                                graphNewAppointment.AppointmentAttendees.Cache,
                                                fsAppointmentAttendeeRow,
                                                copyNotes: true,
                                                copyFiles: false);
            }
        }

        private void CloneResources(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow)
        {
            foreach (FSAppointmentResource originalAppointmentResource in graphOriginalAppointment.AppointmentResources.Select())
            {
                FSAppointmentResource fsAppointmentResourceRow = PXCache<FSAppointmentResource>.CreateCopy(originalAppointmentResource);

                fsAppointmentResourceRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentResourceRow.RefNbr = null;
                graphNewAppointment.AppointmentResources.Insert(fsAppointmentResourceRow);
            }
        }

        private void EnableDisableFields(PXCache cache, FSCloneAppointmentFilter fsCloneAppointmentFilterRow)
        {
            bool isSingleAppointment = fsCloneAppointmentFilterRow.CloningType == ID.CloningType_CloneAppointment.SINGLE;

            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.scheduledDate>(cache, fsCloneAppointmentFilterRow, isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.scheduledDate>(cache, fsCloneAppointmentFilterRow, isSingleAppointment);

            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.scheduledFromDate>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.scheduledFromDate>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetRequired<FSCloneAppointmentFilter.scheduledFromDate>(cache, !isSingleAppointment);

            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.scheduledToDate>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.scheduledToDate>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetRequired<FSCloneAppointmentFilter.scheduledToDate>(cache, !isSingleAppointment);

            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnMonday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnMonday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnThursday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnThursday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnWednesday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnWednesday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnTuesday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnTuesday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnFriday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnFriday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnSaturday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnSaturday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetEnabled<FSCloneAppointmentFilter.activeOnSunday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
            PXUIFieldAttribute.SetVisible<FSCloneAppointmentFilter.activeOnSunday>(cache, fsCloneAppointmentFilterRow, !isSingleAppointment);
        }

        /// <summary>
        /// Check the ManageRooms value on Setup to check/hide the Rooms Values options.
        /// </summary>
        private void HideRooms()
        {
            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(this, SetupRecord?.Current);

            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.SelectSingle();

            PXUIFieldAttribute.SetVisible<FSServiceOrder.roomID>(this.ServiceOrderRelated.Cache, fsServiceOrderRow, isRoomManagementActive);
        }

        #endregion

        #region Events

        protected void FSCloneAppointmentFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            HideRooms();
            EnableDisableFields(cache, fsCloneAppointmentFilterRow);
        }

        protected void FSCloneAppointmentFilter_ScheduledEndTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            fsCloneAppointmentFilterRow.ScheduledEndTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
            fsCloneAppointmentFilterRow.ScheduledEndTime = SharedFunctions.GetCustomDateTime(AppointmentSelected.Current.ScheduledDateTimeEnd, fsCloneAppointmentFilterRow.ScheduledEndTime);
        }

        protected void FSCloneAppointmentFilter_ScheduledStartTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            fsCloneAppointmentFilterRow.ScheduledStartTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime EndTimeValue = fsCloneAppointmentFilterRow.ScheduledStartTime.Value.AddMinutes((double)fsCloneAppointmentFilterRow.OriginalDuration);
            fsCloneAppointmentFilterRow.ScheduledEndTime = SharedFunctions.GetCustomDateTime(AppointmentSelected.Current.ScheduledDateTimeEnd, EndTimeValue);
        }

        #endregion
    }
}
