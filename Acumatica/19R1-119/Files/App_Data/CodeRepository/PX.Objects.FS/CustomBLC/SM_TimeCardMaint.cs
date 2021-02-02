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

        #region PrivateFunctions
        /// <summary>
        /// Update ApprovedTime and actualDuration fields in the <c>AppointmentDetInfo</c> lines.
        /// </summary>
        private void UpdateAppointmentFromApprovedTimeCard(PXCache cache)
        {
            FSxPMTimeActivity fsxPMTimeActivityRow = null;

            var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            foreach (TimeCardMaint.EPTimecardDetail ePTimeCardDetailRow in this.Base.Activities.Select())
            {
                fsxPMTimeActivityRow = this.Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(ePTimeCardDetailRow);

                if (fsxPMTimeActivityRow.AppEmpID.HasValue == false)
                {
                    continue;
                }

                FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
                                                        Where<
                                                        FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                                        .Select(Base, fsxPMTimeActivityRow.AppointmentID);

                FSAppointmentEmployee fsAppointmentEmployeeRow = PXSelect<FSAppointmentEmployee,
                                                                    Where<
                                                                        FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
                                                                        And<FSAppointmentEmployee.lineNbr, Equal<Required<FSAppointmentEmployee.lineNbr>>>>>
                                                                    .Select(Base, fsxPMTimeActivityRow.AppointmentID, fsxPMTimeActivityRow.AppEmpID);

                graphAppointmentEntry.SkipTimeCardUpdate = true;

                fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                        (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                fsAppointmentEmployeeRow.TimeCardCD = ePTimeCardDetailRow.TimeCardCD;
                fsAppointmentEmployeeRow.ApprovedTime = true;
                fsAppointmentEmployeeRow = graphAppointmentEntry.AppointmentEmployees.Update(fsAppointmentEmployeeRow);

                graphAppointmentEntry.Save.Press();
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
        [PXUIField(DisplayName = PX.Objects.EP.Messages.NormalizeTimecard)]
        [PXButton(Tooltip = PX.Objects.EP.Messages.NormalizeTimecard)]
        protected virtual void NormalizeTimecard()
        {
            foreach (TimeCardMaint.EPTimecardDetail item in Base.Activities.Select())
            {
                FSxPMTimeActivity fsxPMTimeActivityRow = Base.Activities.Cache.GetExtension<FSxPMTimeActivity>(item);

                if (fsxPMTimeActivityRow.AppointmentID != null)
                {
                    Base.Activities.Cache.SetValue<FSxPMTimeActivity.lastBillable>(item, item.IsBillable);
                    Base.Activities.Cache.SetValue<TimeCardMaint.EPTimecardDetail.isBillable>(item, true);
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
                    Base.Activities.Cache.SetStatus(item, PXEntryStatus.Updated);
                    Base.Save.Press();
                }
            }
        }

        #endregion

        #region EPTimeCardDetail Events
        protected virtual void EPTimeCardDetail_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null
                    || !TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base))
            {
                return;
            }

            TimeCardMaint.EPTimecardDetail epTimeCardDetailRow = (TimeCardMaint.EPTimecardDetail)e.Row;
            TimeCardHelper.PMTimeActivity_RowSelected_Handler(cache, epTimeCardDetailRow);
        }

        protected virtual void EPTimeCardDetail_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            TimeCardHelper.PMTimeActivity_RowPersisting_Handler(cache, Base, (PMTimeActivity)e.Row, e);
        }
        #endregion

        #region EPTimeCard Events
        protected virtual void EPTimeCard_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
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
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appEmpID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
            PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.serviceID>(Base.Activities.Cache, Base.Activities.Current, enableEmpTimeCardIntegration);
        }
        
        protected virtual void EPTimeCard_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (e.Row == null
                    || !TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base))
            {
                return;
            }

            EPTimeCard epTimeCardRow = (EPTimeCard)e.Row;

            if (epTimeCardRow.IsApproved == true
                    && (bool)cache.GetValueOriginal<EPTimeCard.isApproved>(epTimeCardRow) == false
                        && e.TranStatus == PXTranStatus.Open)
            {
                UpdateAppointmentFromApprovedTimeCard(cache);
            }
        }
        #endregion
    }    
}
