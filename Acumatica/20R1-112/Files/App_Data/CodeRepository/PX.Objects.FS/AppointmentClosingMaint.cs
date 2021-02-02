using PX.Data;
using System.Collections;

namespace PX.Objects.FS
{
    public class AppointmentClosingMaint : AppointmentEntry
    {
        public AppointmentClosingMaint()
            : base()
        {
            appClosingMenuActions.AddMenuAction(completeAppointment);
            appClosingMenuActions.AddMenuAction(closeAppointment);

            menuActions.SetVisible(false);

            FieldUpdating.AddHandler(typeof(FSAppointment),
                                     typeof(FSAppointment.actualDateTimeBegin).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSAppointment_ActualDateTimeBegin_Time_FieldUpdating);

            // This is needed for Apppointment Closing Screen because the navigation functionality 
            // fails if the insert for this view it is enabled.
            ClosingAppointmentRecords.Cache.AllowInsert = false;
        }

        #region Selects
        public PXSelect<FSAppointment,
               Where<
                   FSAppointment.srvOrdType, Equal<Optional<FSAppointment.srvOrdType>>,
                   And<FSAppointment.routeDocumentID, Equal<Optional<FSAppointment.routeDocumentID>>>>>
               ClosingAppointmentRecords;
        #endregion

        #region CacheAttached
        #region FSAppointment_RefNbr
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.")]
        protected virtual void FSAppointment_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions
        public PXMenuAction<FSAppointment> appClosingMenuActions;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable AppClosingMenuActions(PXAdapter adapter)
        {
            return adapter.Get();
        }

        #region PostToInventory
        public PXAction<FSAppointment> postToInventory;
        [PXUIField(DisplayName = "Update Inventory", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable PostToInventory(PXAdapter adapter)
        {
            FSAppointment fsAppointmentRow = ClosingAppointmentRecords.Current;

            if (fsAppointmentRow != null)
            {
                UpdateInventoryPost graphUpdateInventoryPost = PXGraph.CreateInstance<UpdateInventoryPost>();

                graphUpdateInventoryPost.Filter.Current.RouteDocumentID = fsAppointmentRow.RouteDocumentID;
                graphUpdateInventoryPost.Filter.Current.AppointmentID = fsAppointmentRow.AppointmentID;
                graphUpdateInventoryPost.Filter.Current.CutOffDate = fsAppointmentRow.ExecutionDate;

                throw new PXRedirectRequiredException(graphUpdateInventoryPost, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }
        #endregion
        #region OpenAppointment
        public PXAction<FSAppointment> OpenAppointment;
        [PXButton]
        [PXUIField(Visible = false)]
        protected virtual void openAppointment()
        {
            AppointmentEntry graphAppointment = PXGraph.CreateInstance<AppointmentEntry>();
            if (ClosingAppointmentRecords.Current != null && ClosingAppointmentRecords.Current.RefNbr != null)
            {
                graphAppointment.AppointmentRecords.Current = graphAppointment.AppointmentRecords.Search<FSAppointment.refNbr>
                    (ClosingAppointmentRecords.Current.RefNbr, ClosingAppointmentRecords.Current.SrvOrdType);

                throw new PXRedirectRequiredException(graphAppointment, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Verifies if the status of the appointment is CANCELED or CLOSED.
        /// </summary>
        public virtual bool AreServicesDBActionsAllowed(FSAppointment fsAppointmentRow)
        {
            return !(fsAppointmentRow.Status == ID.Status_Appointment.CANCELED 
                || fsAppointmentRow.Status == ID.Status_Appointment.CLOSED);
        }

        /// <summary>
        /// Allow Or Forbid Insert, Update and Delete operations in the Services tab.
        /// </summary>
        public virtual void AllowOrForbidDetailsDBactions(FSAppointment fsAppointmentRow)
        {
            AppointmentDetails.AllowInsert = AreServicesDBActionsAllowed(fsAppointmentRow);
            AppointmentDetails.AllowDelete = AreServicesDBActionsAllowed(fsAppointmentRow);
            AppointmentDetails.AllowUpdate = AreServicesDBActionsAllowed(fsAppointmentRow);
        }
        #endregion

        #region Event Handlers
        #region Appointment Events
        protected override void _(Events.RowSelected<FSAppointment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            PXCache cache = e.Cache;

            base._(e);

            OpenAppointment.SetEnabled(true);

            postToInventory.SetEnabled(fsAppointmentRow.Status == ID.Status_Appointment.CLOSED && ServiceOrderTypeSelected.Current.EnableINPosting == true);

            AllowOrForbidDetailsDBactions(fsAppointmentRow);

            PXUIFieldAttribute.SetEnabled<FSAppointment.refNbr>(cache, fsAppointmentRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointment.srvOrdType>(cache, fsAppointmentRow, false);
            PXUIFieldAttribute.SetEnabled<FSAppointment.routeID>(cache, fsAppointmentRow, false);
            PXUIFieldAttribute.SetEnabled<FSServiceOrder.customerID>(cache, fsAppointmentRow, false);
        }
        #endregion
        #endregion
    }
}