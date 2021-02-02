using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.FS
{
    public class SM_TimeCardMaint : PXGraphExtension<TimeCardMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>()
                    && PXAccess.FeatureInstalled<FeaturesSet.timeReportingModule>();
        }

        public PXSelect<FSSetup> SetupRecord;

        public PXSelect<PMSetup> PMSetupRecord;

        #region Virtual Functions
        /// <summary>
        /// Update ApprovedTime and actualDuration fields in the <c>AppointmentDetInfo</c> lines.
        /// </summary>
        public virtual void UpdateAppointmentFromApprovedTimeCard(PXCache cache)
        {
            FSxPMTimeActivity fsxPMTimeActivityRow = null;

            var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            foreach (TimeCardMaint.EPTimecardDetail ePTimeCardDetailRow in this.Base.Activities.Select())
            {
                fsxPMTimeActivityRow = this.Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(ePTimeCardDetailRow);

                if (fsxPMTimeActivityRow.LogLineNbr.HasValue == false)
                {
                    continue;
                }

                FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                                 Where<
                                                     FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                 .Select(Base, fsxPMTimeActivityRow.AppointmentID);

                FSAppointmentLog fsAppointmentLogRow = PXSelect<FSAppointmentLog,
                                                                 Where<
                                                                     FSAppointmentLog.docID, Equal<Required<FSAppointmentLog.docID>>,
                                                                     And<FSAppointmentLog.lineNbr, Equal<Required<FSAppointmentLog.lineNbr>>>>>
                                                                 .Select(Base, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.LogLineNbr);

                if (fsAppointmentLogRow != null)
                {
                graphAppointmentEntry.SkipTimeCardUpdate = true;

                fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                        (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                fsAppointmentLogRow.TimeCardCD = ePTimeCardDetailRow.TimeCardCD;
                fsAppointmentLogRow.ApprovedTime = true;
                fsAppointmentLogRow = graphAppointmentEntry.LogRecords.Update(fsAppointmentLogRow);

                graphAppointmentEntry.Save.Press();
            }
        }
        }
        #endregion

        #region Actions

        #region OpenAppointment
        public PXAction<EPTimeCard> OpenAppointment;
        [PXUIField(DisplayName = "Open Appointment")]
        [PXLookupButton]
        protected virtual void openAppointment()
        {
            if (Base.Activities.Current != null)
            {
                FSxPMTimeActivity fsxPMTimeActivityRow = Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(Base.Activities.Current);

                AppointmentEntry graph = PXGraph.CreateInstance<AppointmentEntry>();
                FSAppointment fsAppointmentRow = (FSAppointment)PXSelect<FSAppointment,
                                                Where<
                                                    FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                .Select(Base, fsxPMTimeActivityRow.AppointmentID);

                if (fsAppointmentRow != null)
                {
                    graph.AppointmentRecords.Current = graph.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);

                    if (graph.AppointmentRecords.Current != null)
                    {
                        throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                    }
                }
            }
        }
        #endregion

        public PXAction<EPTimeCard> normalizeTimecard;
        [PXUIField(DisplayName = EP.Messages.NormalizeTimecard)]
        [PXButton(Tooltip = EP.Messages.NormalizeTimecard)]
        protected virtual void NormalizeTimecard()
        {
            foreach (TimeCardMaint.EPTimecardDetail item in Base.Activities.Select())
            {
                FSxPMTimeActivity fsxPMTimeActivityRow = Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(item);

                if (fsxPMTimeActivityRow.AppointmentID != null)
                {
                    Base.Activities.Cache.SetValue<FSxPMTimeActivity.lastBillable>(item, item.IsBillable);
                    Base.Activities.Cache.SetValue<TimeCardMaint.EPTimecardDetail.isBillable>(item, true);
                    Base.Activities.Update(item);
                }
            }

            Base.normalizeTimecard.Press();

            foreach (TimeCardMaint.EPTimecardDetail item in Base.Activities.Select())
            {
                FSxPMTimeActivity fsxPMTimeActivityRow = Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(item);

                if (fsxPMTimeActivityRow.AppointmentID != null)
                {
                    Base.Activities.Cache.SetValue<TimeCardMaint.EPTimecardDetail.isBillable>(item, fsxPMTimeActivityRow.LastBillable);
                    Base.Activities.Cache.SetValue<FSxPMTimeActivity.lastBillable>(item, false);
                    Base.Activities.Update(item);
                }
                }

            if (Base.Activities.Cache.IsDirty == true)
            {
                Base.Save.Press();
            }
        }

        #endregion

        #region Event Handlers

        #region EPTimeCardDetail

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

        protected virtual void _(Events.RowSelecting<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowSelected<TimeCardMaint.EPTimecardDetail> e)
        {
            if (e.Row == null
                    || !TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base))
            {
                return;
            }

            TimeCardMaint.EPTimecardDetail epTimeCardDetailRow = (TimeCardMaint.EPTimecardDetail)e.Row;
            TimeCardHelper.PMTimeActivity_RowSelected_Handler(e.Cache, epTimeCardDetailRow);
        }

        protected virtual void _(Events.RowInserting<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowInserted<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowUpdating<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowUpdated<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowDeleting<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowDeleted<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        protected virtual void _(Events.RowPersisting<TimeCardMaint.EPTimecardDetail> e)
        {
            if (e.Row == null)
            {
                return;
            }

            TimeCardHelper.PMTimeActivity_RowPersisting_Handler(e.Cache, Base, (PMTimeActivity)e.Row, e.Args);
        }

        protected virtual void _(Events.RowPersisted<TimeCardMaint.EPTimecardDetail> e)
        {
        }

        #endregion

        #region EPTimeCard

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

        protected virtual void _(Events.RowSelecting<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowSelected<EPTimeCard> e)
        {
            if (e.Row == null)
            {
                return;
            }

            EPTimeCard epTimeCardRow = (EPTimeCard)e.Row;

            bool enableEmpTimeCardIntegration = (bool)TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appointmentID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appointmentCustomerID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.sOID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.logLineNbr>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.serviceID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
        }

        protected virtual void _(Events.RowInserting<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowInserted<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowUpdating<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowUpdated<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowDeleting<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowDeleted<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowPersisting<EPTimeCard> e)
        {
        }

        protected virtual void _(Events.RowPersisted<EPTimeCard> e)
        {
            if (e.Row == null
                    || !TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base))
            {
                return;
            }

            EPTimeCard epTimeCardRow = (EPTimeCard)e.Row;
            PXCache cache = e.Cache;

            if (epTimeCardRow.IsApproved == true
                    && (bool)cache.GetValueOriginal<EPTimeCard.isApproved>(epTimeCardRow) == false
                        && e.TranStatus == PXTranStatus.Open)
            {
                UpdateAppointmentFromApprovedTimeCard(cache);
            }
        }
        
        #endregion

        #endregion
    }
}
