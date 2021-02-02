using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    public class CloneAppointmentProcess : PXGraph<CloneAppointmentProcess>
    {
        // TODO: Delete this variable for the next major release
        PXGraph dummyGraph;

        PXGraph ReadingGraph = null;

        public CloneAppointmentProcess() : base()
        {
        }

        #region Selects

        public PXFilter<FSCloneAppointmentFilter> Filter;
        public PXAction<FSCloneAppointmentFilter> cancel;

        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        public PXSelectJoin<FSAppointment,
               InnerJoin<FSServiceOrder,
                   On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
               Where<
                   FSAppointment.srvOrdType, Equal<Current<FSCloneAppointmentFilter.srvOrdType>>,
                   And<FSAppointment.refNbr, Equal<Current<FSCloneAppointmentFilter.refNbr>>>>> AppointmentSelected;

        public PXSelectReadonly<FSAppointmentFSServiceOrder,
               Where<
                   FSAppointmentFSServiceOrder.originalAppointmentID, Equal<Current<FSAppointment.appointmentID>>>,
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
        [PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable Clone(PXAdapter adapter)
        {
            if (AppointmentSelected.Current == null)
                throw new PXException(TX.Error.RECORD_X_NOT_FOUND, DACHelper.GetDisplayName(typeof(FSAppointment)));

            bool validationError = false;

            if (Filter.Current.CloningType == FSCloneAppointmentFilter.cloningType.Values.Multiple)
            {
                if (Filter.Current.MultGenerationFromDate == null)
                {
                    FieldCannotBeEmptyError<FSCloneAppointmentFilter.multGenerationFromDate>(Filter.Cache, Filter.Current);
                    validationError = true;
                }

                if (Filter.Current.MultGenerationToDate == null)
                {
                    FieldCannotBeEmptyError<FSCloneAppointmentFilter.multGenerationToDate>(Filter.Cache, Filter.Current);
                    validationError = true;
                }

                if (validationError == false
                    && Filter.Current.MultGenerationFromDate > Filter.Current.MultGenerationToDate)
                {
                    Filter.Cache.RaiseExceptionHandling<FSCloneAppointmentFilter.multGenerationToDate>(
                        Filter.Current,
                        Filter.Current.MultGenerationToDate,
                        new PXSetPropertyException(
                            PXMessages.LocalizeFormatNoPrefix(TX.Error.END_DATE_LESSER_THAN_START_DATE),
                            PXErrorLevel.Error));
                    validationError = true;
                }
            }
            else
            {
                if (Filter.Current.SingleGenerationDate == null)
                {
                    FieldCannotBeEmptyError<FSCloneAppointmentFilter.singleGenerationDate>(Filter.Cache, Filter.Current);
                    validationError = true;
                }
            }

            if (validationError == true)
                return adapter.Get();

            FSServiceOrder fsServiceOrderRow = PXSelectJoin<FSServiceOrder,
                    InnerJoin<FSAppointment,
                        On<FSAppointment.sOID, Equal<FSServiceOrder.sOID>>>,
                    Where<
                        FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                    .Select(this, AppointmentSelected.Current.AppointmentID);

            if (fsServiceOrderRow == null)
                return adapter.Get();

            if (fsServiceOrderRow != null
                    && fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED)
            {
                throw new PXException(TX.Error.CANNOT_CLONE_APPOINMENT_SERVICE_ORDER_COMPLETED);
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)Filter.Cache.CreateCopy(Filter.Current);

            PXLongOperation.StartOperation(this, delegate ()
            {
                CloneAppointmentProcess cloneApptGraph = PXGraph.CreateInstance<CloneAppointmentProcess>();
                cloneApptGraph.Filter.Cache.RestoreCopy(cloneApptGraph.Filter.Current, fsCloneAppointmentFilterRow);

                cloneApptGraph.AppointmentSelected.Current = cloneApptGraph.AppointmentSelected.Select();

                var filter = cloneApptGraph.Filter.Current;
                bool ignoreWeekDay = false;

                if (filter.CloningType == FSCloneAppointmentFilter.cloningType.Values.Single)
                {
                    filter.CloningType = FSCloneAppointmentFilter.cloningType.Values.Multiple;
                    filter.MultGenerationFromDate = filter.SingleGenerationDate;
                    filter.MultGenerationToDate = filter.SingleGenerationDate;

                    ignoreWeekDay = true;
                }

                AppointmentEntry originalApptGraph = PXGraph.CreateInstance<AppointmentEntry>();

                originalApptGraph.AppointmentRecords.Current =
                    originalApptGraph.AppointmentRecords.Search<FSAppointment.appointmentID>
                        (AppointmentSelected.Current.AppointmentID, AppointmentSelected.Current.SrvOrdType);

                AppointmentEntry newApptGraph = PXGraph.CreateInstance<AppointmentEntry>();

                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    for (DateTime i = (DateTime)filter.MultGenerationFromDate; i <= filter.MultGenerationToDate; i = i.AddDays(1))
                    {
                        if (ignoreWeekDay == true
                            || (i.DayOfWeek == DayOfWeek.Monday && (bool)filter.ActiveOnMonday)
                            || (i.DayOfWeek == DayOfWeek.Tuesday && (bool)filter.ActiveOnTuesday)
                            || (i.DayOfWeek == DayOfWeek.Wednesday && (bool)filter.ActiveOnWednesday)
                            || (i.DayOfWeek == DayOfWeek.Thursday && (bool)filter.ActiveOnThursday)
                            || (i.DayOfWeek == DayOfWeek.Friday && (bool)filter.ActiveOnFriday)
                            || (i.DayOfWeek == DayOfWeek.Saturday && (bool)filter.ActiveOnSaturday)
                            || (i.DayOfWeek == DayOfWeek.Sunday && (bool)filter.ActiveOnSunday))
                        {
                            cloneApptGraph.CloneAppointment(originalApptGraph, newApptGraph, i, (int)filter.ApptDuration);
                        }
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
            Filter.Cache.SetDefaultExt<FSCloneAppointmentFilter.scheduledStartTime>(Filter.Current);
            Filter.Cache.SetDefaultExt<FSCloneAppointmentFilter.overrideApptDuration>(Filter.Current);

            return new object[] { Filter.Current };
        }

        #endregion

        #region Virtual Methods

        public virtual void CloneAppointment(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, DateTime newApptBeginDate, int newApptDuration)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            gNewAppt.Clear(PXClearOption.ClearAll);
            gNewAppt.ClearServiceOrderEntry();

            FSAppointment fsAppointmentRow = PXCache<FSAppointment>.CreateCopy(AppointmentSelected.Current);
            var itemLineRefNbr = new Dictionary<string, string>();

            //Clear key and special fields
            fsAppointmentRow.RefNbr = null;
            fsAppointmentRow.AppointmentID = null;
            fsAppointmentRow.NoteID = null;
            fsAppointmentRow.CuryInfoID = null;
            fsAppointmentRow.FullNameSignature = null;
            fsAppointmentRow.customerSignaturePath = null;
            fsAppointmentRow.BillServiceContractID = null;
            fsAppointmentRow.HandleManuallyScheduleTime = (bool)fsAppointmentRow.HandleManuallyScheduleTime || (bool)Filter.Current.OverrideApptDuration;
            fsAppointmentRow.HandleManuallyActualTime = false;
            fsAppointmentRow.LogLineCntr = 0;
            fsAppointmentRow.FinPeriodID = null;
            fsAppointmentRow.PostingStatusAPARSO = ID.Status_Posting.PENDING_TO_POST;
            fsAppointmentRow.PendingAPARSOPost = true;
            fsAppointmentRow.ActualDateTimeBegin = null;
            fsAppointmentRow.ActualDateTimeEnd = null;

            fsAppointmentRow.OriginalAppointmentID = AppointmentSelected.Current.AppointmentID;

            fsAppointmentRow.ScheduledDateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(
                                newApptBeginDate, Filter.Current.ScheduledStartTime);

            DateTime? newApptEnd = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(newApptDuration);

            fsAppointmentRow.ExecutionDate = fsAppointmentRow.ScheduledDateTimeBegin.Value.Date;

            fsAppointmentRow.CutOffDate = null;

            fsAppointmentRow.Status = ID.Status_Appointment.MANUAL_SCHEDULED;
            fsAppointmentRow.Hold = false;

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
            fsAppointmentRow.LineCntr = 0;
            //----------------------------------------------------------------------

            gNewAppt.IsCloningAppointment = true;
            fsAppointmentRow = gNewAppt.AppointmentRecords.Insert(fsAppointmentRow);

            gNewAppt.Answers.Current = gNewAppt.Answers.Select();
            gNewAppt.Answers.CopyAllAttributes(gNewAppt.AppointmentRecords.Current, AppointmentSelected.Current);

            PXNoteAttribute.CopyNoteAndFiles(AppointmentSelected.Cache,
                                             AppointmentSelected.Current,
                                             gNewAppt.AppointmentSelected.Cache,
                                             fsAppointmentRow,
                                             copyNotes: true,
                                             copyFiles: false);


            foreach (FSAppointmentDet originalItemRow in gOriginalAppt.AppointmentDetails.Select())
            {
                if (originalItemRow.IsInventoryItem == true)
                {
                    this.CloneParts(gOriginalAppt, gNewAppt, originalItemRow);
                }
                else if (originalItemRow.IsInventoryItem == false && originalItemRow.IsPickupDelivery == false)
                {
                    this.CloneServices(gOriginalAppt, gNewAppt, originalItemRow, itemLineRefNbr);
                }
            }

            this.CloneStaff(gOriginalAppt, gNewAppt, itemLineRefNbr);
            this.CloneResources(gOriginalAppt, gNewAppt);

            if (gNewAppt.AppointmentRecords.Current.ScheduledDateTimeEnd != newApptEnd)
            {
                if (fsAppointmentRow.HandleManuallyScheduleTime == false)
                {
                    gNewAppt.AppointmentRecords.Cache.SetValueExt<FSAppointment.handleManuallyScheduleTime>(fsAppointmentRow, true);
                }
                gNewAppt.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow, newApptEnd);
            }

            gNewAppt.AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsAppointmentRow);
            gNewAppt.Save.Press();
        }

        public virtual void ClearSourceLineBeforeCopy(FSAppointmentDet apptDet)
        {
            //TODO - This sould be delete in 2020R2
            if (apptDet.Status == ID.Status_AppointmentDet.NOT_PERFORMED 
                && apptDet.IsFree == false
                && apptDet.InventoryID != null) 
            {
                apptDet.IsBillable = true;
            }

            apptDet.Status = ID.Status_AppointmentDet.NOT_STARTED;

            apptDet.ActualDuration = 0;
            apptDet.ActualQty = 0;

            apptDet.CuryTranAmt = 0;

            apptDet.CuryBillableExtPrice = 0;

            apptDet.CuryBillableTranAmt = 0;

            apptDet.PostID = null;
            apptDet.NoteID = null;

            apptDet.LogActualDuration = 0;
        }

        public virtual void CloneServices(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointmentDet sourceRow, Dictionary<string, string> itemLineRef)
        {
            if (sourceRow == null || sourceRow.Status == ID.Status_AppointmentDet.CANCELED)
            {
                return;
            }

            // Creates a copy to not edit the original row.
            FSAppointmentDet sourceRowCopy = PXCache<FSAppointmentDet>.CreateCopy(sourceRow);
            ClearSourceLineBeforeCopy(sourceRowCopy);

            FSAppointmentDet newRow = new FSAppointmentDet();
            newRow = AppointmentEntry.InsertDetailLine<FSAppointmentDet, FSAppointmentDet>(
                                                        gNewAppt.AppointmentDetails.Cache,
                                                        newRow,
                                                        gOriginalAppt.AppointmentDetails.Cache,
                                                        sourceRowCopy,
                                                        null,
                                                        sourceRowCopy.SODetID,
                                                        copyTranDate: false,
                                                        tranDate: sourceRowCopy.TranDate,
                                                        SetValuesAfterAssigningSODetID: true,
                                                        copyingFromQuote: false);

            PXNoteAttribute.CopyNoteAndFiles(
                                            gOriginalAppt.AppointmentDetails.Cache,
                                            sourceRow,
                                            gNewAppt.AppointmentDetails.Cache,
                                            newRow,
                                            copyNotes: true,
                                            copyFiles: false);


            gNewAppt.AppointmentDetails.SetValueExt<FSAppointmentDet.acctID>(newRow, sourceRowCopy.AcctID);
            gNewAppt.AppointmentDetails.SetValueExt<FSAppointmentDet.subID>(newRow, sourceRowCopy.SubID);

            itemLineRef.Add(sourceRow.LineRef, newRow.LineRef);
        }

        [Obsolete("Remove in major release")]
        public virtual void CloneServices(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointmentDet sourceRow)
        {
            if (sourceRow == null || sourceRow.Status == ID.Status_AppointmentDet.CANCELED)
            {
                return;
            }

            // Creates a copy to not edit the original row.
            FSAppointmentDet sourceRowCopy = PXCache<FSAppointmentDet>.CreateCopy(sourceRow);
            ClearSourceLineBeforeCopy(sourceRowCopy);

            FSAppointmentDet newRow = new FSAppointmentDet();
            newRow = AppointmentEntry.InsertDetailLine<FSAppointmentDet, FSAppointmentDet>(
                                                        gNewAppt.AppointmentDetails.Cache,
                                                        newRow,
                                                        gOriginalAppt.AppointmentDetails.Cache,
                                                        sourceRowCopy,
                                                        null,
                                                        sourceRowCopy.SODetID,
                                                        copyTranDate: false,
                                                        tranDate: sourceRowCopy.TranDate,
                                                        SetValuesAfterAssigningSODetID: true,
                                                        copyingFromQuote: false);

            PXNoteAttribute.CopyNoteAndFiles(
                                            gOriginalAppt.AppointmentDetails.Cache,
                                            sourceRow,
                                            gNewAppt.AppointmentDetails.Cache,
                                            newRow,
                                            copyNotes: true,
                                            copyFiles: false);


            gNewAppt.AppointmentDetails.SetValueExt<FSAppointmentDet.acctID>(newRow, sourceRowCopy.AcctID);
            gNewAppt.AppointmentDetails.SetValueExt<FSAppointmentDet.subID>(newRow, sourceRowCopy.SubID);
        }

        public virtual void CloneParts(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointmentDet sourceRow)
        {
            if (sourceRow == null || sourceRow.Status == ID.Status_AppointmentDet.CANCELED)
            {
                return;
            }

            FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(gOriginalAppt, sourceRow);
            if (fsSODetRow == null)
            {
                return;
            }

            if (ReadingGraph == null)
            {
                ReadingGraph = new PXGraph();
            }

            FSAppointmentDet takenQtyByAppointments = 
                PXSelectJoinGroupBy<FSAppointmentDet,
                InnerJoin<FSAppointment, 
                On<
                    FSAppointment.srvOrdType, Equal<FSAppointmentDet.srvOrdType>,
                    And<FSAppointment.refNbr, Equal<FSAppointmentDet.refNbr>>>>,
                Where<
                    FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                    And<FSAppointmentDet.isCanceledNotPerformed, NotEqual<True>>>>,
                Aggregate<
                    GroupBy<FSAppointmentDet.sODetID, Sum<FSAppointmentDet.billableQty>>>>
                .Select(ReadingGraph, sourceRow.SODetID);

            decimal? openQty = fsSODetRow.BillableQty - (takenQtyByAppointments?.BillableQty ?? 0m);
            decimal? qtyFromBal = openQty >= sourceRow.BillableQty ? sourceRow.BillableQty :
                                    openQty > 0m ? openQty : 0m;
            decimal? missingQty = sourceRow.BillableQty - qtyFromBal;
            FSAppointmentDet noteLine = null;

            // Creates a copy to not edit the original row.
            FSAppointmentDet sourceRowCopy = PXCache<FSAppointmentDet>.CreateCopy(sourceRow);
            ClearSourceLineBeforeCopy(sourceRowCopy);

            if (qtyFromBal > 0)
            {
                sourceRowCopy.EstimatedQty = qtyFromBal;
                sourceRowCopy.BillableQty = qtyFromBal;

                FSAppointmentDet newRow = new FSAppointmentDet();
                newRow = AppointmentEntry.InsertDetailLine<FSAppointmentDet, FSAppointmentDet>(
                                                            gNewAppt.AppointmentDetails.Cache,
                                                            newRow,
                                                            gOriginalAppt.AppointmentDetails.Cache,
                                                            sourceRowCopy,
                                                            null,
                                                            sourceRowCopy.SODetID,
                                                            copyTranDate: false,
                                                            tranDate: sourceRowCopy.TranDate,
                                                            SetValuesAfterAssigningSODetID: true,
                                                            copyingFromQuote: false);
                noteLine = newRow;
            }

            if (missingQty > 0)
            {
                sourceRowCopy.EstimatedQty = missingQty;
                sourceRowCopy.BillableQty = missingQty;

                FSAppointmentDet newRow = new FSAppointmentDet();
                newRow = AppointmentEntry.InsertDetailLine<FSAppointmentDet, FSAppointmentDet>(
                                                        gNewAppt.AppointmentDetails.Cache,
                                                        newRow,
                                                        gOriginalAppt.AppointmentDetails.Cache,
                                                        sourceRowCopy,
                                                        null,
                                                        null,
                                                        copyTranDate: false,
                                                        tranDate: sourceRowCopy.TranDate,
                                                        SetValuesAfterAssigningSODetID: true,
                                                        copyingFromQuote: false);

                if (noteLine == null)
                {
                    noteLine = newRow;
                }
            }

            if (noteLine != null)
            {
                PXNoteAttribute.CopyNoteAndFiles(gOriginalAppt.AppointmentDetails.Cache,
                                                 sourceRow,
                                                 gNewAppt.AppointmentDetails.Cache,
                                                 noteLine,
                                                 copyNotes: true,
                                                 copyFiles: false);
            }
        }

        public virtual void CloneStaff(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, Dictionary<string, string> itemLineRef)
        {
            foreach (FSAppointmentEmployee originalStaffRow in gOriginalAppt.AppointmentServiceEmployees.Select())
            {
                FSAppointmentEmployee newStaffRow = PXCache<FSAppointmentEmployee>.CreateCopy(originalStaffRow);

                newStaffRow.RefNbr = null;
                newStaffRow.AppointmentID = gNewAppt.AppointmentSelected.Current.AppointmentID;
                newStaffRow.NoteID = null;

                if (string.IsNullOrEmpty(newStaffRow.ServiceLineRef) == false
                        && itemLineRef.ContainsKey(newStaffRow.ServiceLineRef) == true)
                {
                    newStaffRow.ServiceLineRef = itemLineRef[newStaffRow.ServiceLineRef];
                }

                gNewAppt.AppointmentServiceEmployees.Insert(newStaffRow);
            }
        }

        [Obsolete("Remove in major release")]
        public virtual void CloneStaff(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt)
        {
            foreach (FSAppointmentEmployee originalStaffRow in gOriginalAppt.AppointmentServiceEmployees.Select())
            {
                FSAppointmentEmployee newStaffRow = PXCache<FSAppointmentEmployee>.CreateCopy(originalStaffRow);

                newStaffRow.RefNbr = null;
                newStaffRow.AppointmentID = gNewAppt.AppointmentSelected.Current.AppointmentID;
                newStaffRow.NoteID = null;

                gNewAppt.AppointmentServiceEmployees.Insert(newStaffRow);
            }
        }

        public virtual void CloneResources(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt)
        {
            foreach (FSAppointmentResource originalResourceRow in gOriginalAppt.AppointmentResources.Select())
            {
                FSAppointmentResource newResourceRow = PXCache<FSAppointmentResource>.CreateCopy(originalResourceRow);

                newResourceRow.RefNbr = null;
                newResourceRow.AppointmentID = gNewAppt.AppointmentSelected.Current.AppointmentID;

                gNewAppt.AppointmentResources.Insert(newResourceRow);
            }
        }

        public virtual void FieldCannotBeEmptyError<Field>(PXCache cache, object row)
            where Field : IBqlField
        {
            cache.RaiseExceptionHandling<Field>(
                row,
                null,
                new PXSetPropertyException(
                    PXMessages.LocalizeFormatNoPrefix(
                        PX.Data.ErrorMessages.FieldIsEmpty,
                        PXUIFieldAttribute.GetDisplayName<Field>(cache)),
                    PXErrorLevel.Error));
        }

        /// <summary>
        /// Check the ManageRooms value on Setup to check/hide the Rooms Values options.
        /// </summary>
        public virtual void HideRooms()
        {
            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(this, SetupRecord?.Current);

            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.SelectSingle();

            PXUIFieldAttribute.SetVisible<FSServiceOrder.roomID>(this.ServiceOrderRelated.Cache, fsServiceOrderRow, isRoomManagementActive);
        }

        #endregion

        #region Events Handlers
        #region FSCloneAppointmentFilter

        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        #endregion
        #region FieldUpdating
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated
        #endregion

        protected virtual void _(Events.RowSelecting<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSCloneAppointmentFilter> e)
        {
            if (e.Row == null)
            {
                return;
            }

            HideRooms();
        }

        protected virtual void _(Events.RowInserting<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSCloneAppointmentFilter> e)
        {
        }

        protected virtual void _(Events.RowPersisted<FSCloneAppointmentFilter> e)
        {
        }

        #endregion
        #endregion
    }
}
