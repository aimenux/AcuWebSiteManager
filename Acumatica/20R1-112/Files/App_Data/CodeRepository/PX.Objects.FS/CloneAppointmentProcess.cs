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

            FieldUpdating.AddHandler(typeof(FSCloneAppointmentFilter),
                                     typeof(FSCloneAppointmentFilter.scheduledEndTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSCloneAppointmentFilter_ScheduledEndTime_Time_FieldUpdating);

            FieldUpdating.AddHandler(typeof(FSCloneAppointmentFilter),
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
        [PXUIField(DisplayName = "Process", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
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

        #region Virtual Functions

        public virtual void CloneMultipleAppointments(CloneAppointmentProcess graph)
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

        public virtual void CloneAppointment(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment)
        {
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            graphNewAppointment.Clear(PXClearOption.ClearAll);
            graphNewAppointment.clearLocalServiceOrder();

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
            fsAppointmentRow.HandleManuallyActualTime = null;
            fsAppointmentRow.HandleManuallyScheduleTime = null;
            fsAppointmentRow.LogLineCntr = 0;
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
                = ServiceOrderCore.GetCutOffDate(graphNewAppointment, graphOriginalAppointment.ServiceOrderRelated.Current.CBID, fsAppointmentRow.ExecutionDate, fsAppointmentRow.SrvOrdType);

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

            graphNewAppointment.IsCloningAppointment = true;
            fsAppointmentRow = graphNewAppointment.AppointmentRecords.Insert(fsAppointmentRow);

            graphNewAppointment.Answers.Current = graphNewAppointment.Answers.Select();
            graphNewAppointment.Answers.CopyAllAttributes(graphNewAppointment.AppointmentRecords.Current, AppointmentSelected.Current);

            PXNoteAttribute.CopyNoteAndFiles(AppointmentSelected.Cache,
                                             AppointmentSelected.Current,
                                             graphNewAppointment.AppointmentSelected.Cache,
                                             fsAppointmentRow,
                                             copyNotes: true,
                                             copyFiles: false);


            foreach (FSAppointmentDet sourceRow in graphOriginalAppointment.AppointmentDetails.Select())
            {
                if (sourceRow.IsInventoryItem == true)
                {
                    this.CloneParts(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow, sourceRow);
                }
                else if (sourceRow.IsInventoryItem == false && sourceRow.IsPickupDelivery == false)
                {
                    this.CloneServices(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow, sourceRow, itemLineRefNbr);
                }
            }

            this.CloneEmployees(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow, itemLineRefNbr);
            this.CloneResources(graphOriginalAppointment, graphNewAppointment, fsAppointmentRow);

            if (graphNewAppointment.AppointmentRecords.Current.ScheduledDateTimeEnd != scheduledDateTimeEnd)
            {
                graphNewAppointment.AppointmentRecords.Cache.SetValueExt<FSAppointment.handleManuallyScheduleTime>(fsAppointmentRow, true);
                graphNewAppointment.AppointmentRecords.Cache.SetValueExt<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow, scheduledDateTimeEnd);
            }

            graphNewAppointment.AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsAppointmentRow);
            graphNewAppointment.Save.Press();
        }

        public virtual void ClearSourceLineBeforeCopy(FSAppointmentDet apptDet)
        {
            //TODO - This sould be delete in 2020R2
            if (apptDet.Status == ID.Status_AppointmentDet.NOT_PERFORMED 
                && apptDet.ManualPrice == true)
            {
                apptDet.ManualPrice = false;
            }

            apptDet.Status = ID.Status_AppointmentDet.NOT_STARTED;

            apptDet.ActualDuration = 0;
            apptDet.Qty = 0;

            apptDet.CuryTranAmt = 0;

            apptDet.CuryBillableExtPrice = 0;

            apptDet.CuryBillableTranAmt = 0;

            apptDet.PostID = null;
            apptDet.NoteID = null;

            apptDet.LogActualDuration = 0;
        }

        [Obsolete("Remove in major release")]
        public virtual void CloneServices(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointment newAppointmentRow, FSAppointmentDet sourceRow)
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

        public virtual void CloneServices(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointment newAppointmentRow, FSAppointmentDet sourceRow, Dictionary<string, string> itemLineRef)
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

        public virtual void CloneParts(AppointmentEntry gOriginalAppt, AppointmentEntry gNewAppt, FSAppointment newAppointmentRow, FSAppointmentDet sourceRow)
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

        [Obsolete("Remove in major release")]
        public virtual void CloneEmployees(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow)
        {
            foreach (FSAppointmentEmployee originalAppointmentEmployee in graphOriginalAppointment.AppointmentServiceEmployees.Select())
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = PXCache<FSAppointmentEmployee>.CreateCopy(originalAppointmentEmployee);

                fsAppointmentEmployeeRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentEmployeeRow.RefNbr = null;
                fsAppointmentEmployeeRow.NoteID = null;

                graphNewAppointment.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }
        }

        public virtual void CloneEmployees(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow, Dictionary<string, string> itemLineRef)
        {
            foreach (FSAppointmentEmployee originalAppointmentEmployee in graphOriginalAppointment.AppointmentServiceEmployees.Select())
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = PXCache<FSAppointmentEmployee>.CreateCopy(originalAppointmentEmployee);

                fsAppointmentEmployeeRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentEmployeeRow.RefNbr = null;
                fsAppointmentEmployeeRow.NoteID = null;

                if(string.IsNullOrEmpty(fsAppointmentEmployeeRow.ServiceLineRef) == false
                        && itemLineRef.ContainsKey(fsAppointmentEmployeeRow.ServiceLineRef) == true)
                {
                    fsAppointmentEmployeeRow.ServiceLineRef = itemLineRef[fsAppointmentEmployeeRow.ServiceLineRef];
                }

                graphNewAppointment.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }
        }

        public virtual void CloneResources(AppointmentEntry graphOriginalAppointment, AppointmentEntry graphNewAppointment, FSAppointment newFSAppointmentRow)
        {
            foreach (FSAppointmentResource originalAppointmentResource in graphOriginalAppointment.AppointmentResources.Select())
            {
                FSAppointmentResource fsAppointmentResourceRow = PXCache<FSAppointmentResource>.CreateCopy(originalAppointmentResource);

                fsAppointmentResourceRow.AppointmentID = newFSAppointmentRow.AppointmentID;
                fsAppointmentResourceRow.RefNbr = null;
                graphNewAppointment.AppointmentResources.Insert(fsAppointmentResourceRow);
            }
        }

        public virtual void EnableDisableFields(PXCache cache, FSCloneAppointmentFilter fsCloneAppointmentFilterRow)
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

        // Cant change to new format
        protected virtual void FSCloneAppointmentFilter_ScheduledEndTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            fsCloneAppointmentFilterRow.ScheduledEndTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
            fsCloneAppointmentFilterRow.ScheduledEndTime = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentSelected.Current.ScheduledDateTimeEnd, fsCloneAppointmentFilterRow.ScheduledEndTime);
        }

        // Cant change to new format
        protected virtual void FSCloneAppointmentFilter_ScheduledStartTime_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            fsCloneAppointmentFilterRow.ScheduledStartTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            DateTime EndTimeValue = fsCloneAppointmentFilterRow.ScheduledStartTime.Value.AddMinutes((double)fsCloneAppointmentFilterRow.OriginalDuration);
            fsCloneAppointmentFilterRow.ScheduledEndTime = PXDBDateAndTimeAttribute.CombineDateTime(AppointmentSelected.Current.ScheduledDateTimeEnd, EndTimeValue);
        }
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

            FSCloneAppointmentFilter fsCloneAppointmentFilterRow = (FSCloneAppointmentFilter)e.Row;

            HideRooms();
            EnableDisableFields(e.Cache, fsCloneAppointmentFilterRow);
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
