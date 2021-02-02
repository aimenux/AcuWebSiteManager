using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.PM;

namespace PX.Objects.FS
{
    public class SM_EmployeeActivitiesEntry : PXGraphExtension<EmployeeActivitiesEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.serviceManagementModule>();
        }

        public override void Initialize()
        {
            FSSetup fsSetupRow = this.SetupRecord.Select();

            if (fsSetupRow != null)
            {
                bool enableEmpTimeCardIntegration = (bool)fsSetupRow.EnableEmpTimeCardIntegration;
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appointmentID>(Base.Activity.Cache, Base.Activity.Current, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appointmentCustomerID>(Base.Activity.Cache, Base.Activity.Current, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.sOID>(Base.Activity.Cache, Base.Activity.Current, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.appEmpID>(Base.Activity.Cache, Base.Activity.Current, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSxPMTimeActivity.serviceID>(Base.Activity.Cache, Base.Activity.Current, enableEmpTimeCardIntegration);
            }
        }

        public PXSelect<FSSetup> SetupRecord;

        public PXSelect<PMSetup> PMSetupRecord;
        
        #region Actions

        #region OpenAppointment
        public PXAction<EmployeeActivitiesEntry.PMTimeActivityFilter> OpenAppointment;
        [PXUIField(DisplayName = "Open Appointment")]
        [PXLookupButton]
        protected virtual void openAppointment()
        {
            if (Base.Activity.Current != null)
            {
                FSxPMTimeActivity fsxPMTimeActivityRow = Base.Activity.Cache.GetExtension<FSxPMTimeActivity>(Base.Activity.Current);

                AppointmentEntry graph = PXGraph.CreateInstance<AppointmentEntry>();
                FSAppointment fsAppointmentRow = PXSelect<FSAppointment,
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

        #endregion

        #region EPActivityApprove Events

        protected virtual void EPActivityApprove_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null
                    || !TimeCardHelper.IsTheTimeCardIntegrationEnabled(Base))
            {
                return;
            }

            EPActivityApprove epActivityApproveRow = (EPActivityApprove)e.Row;
            TimeCardHelper.PMTimeActivity_RowSelected_Handler(cache, epActivityApproveRow);
        }

        protected virtual void EPActivityApprove_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            TimeCardHelper.PMTimeActivity_RowPersisting_Handler(cache, Base, (PMTimeActivity)e.Row, e);
        }

        #endregion
    }    
}
