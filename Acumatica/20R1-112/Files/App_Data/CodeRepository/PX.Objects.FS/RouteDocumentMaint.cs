using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.LicensePolicy;
using PX.Objects.CR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FS
{
    public class RouteDocumentMaint : PXGraph<RouteDocumentMaint, FSRouteDocument>, IGraphWithInitialization
    {
        public static bool IsReadyToBeUsed(PXGraph graph)
        {
            bool isSetupCompleted = PXSelect<FSRouteSetup, Where<FSRouteSetup.routeNumberingID, IsNotNull>>.Select(graph).Count > 0;

            return isSetupCompleted;
        }

        public AppointmentEntry globalAppointmentGraph = null;

        public RouteDocumentMaint()
            : base()
        {
            PXUIFieldAttribute.SetDisplayName<FSServiceOrder.locationID>(ServiceOrder.Cache, TX.CustomTextFields.CUSTOMER_LOCATION);
            PXUIFieldAttribute.SetDisplayName<FSAppointment.estimatedDurationTotal>(Appointment.Cache, TX.CustomTextFields.ESTIMATED_DURATION);

            FieldUpdating.AddHandler(typeof(FSRouteDocument),
                                     typeof(FSRouteDocument.timeBegin).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRouteDocument_TimeBegin_FieldUpdating);

            FieldUpdating.AddHandler(typeof(FSRouteDocument),
                                     typeof(FSRouteDocument.actualStartTime).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                     FSRouteDocument_ActualStartTime_FieldUpdating);

            actionsMenu.AddMenuAction(startRoute);
            actionsMenu.AddMenuAction(completeRoute);
            actionsMenu.AddMenuAction(cancelRoute);
            actionsMenu.AddMenuAction(reopenRoute);
            actionsMenu.AddMenuAction(uncloseRoute);

            AppointmentsInRoute.Cache.AllowUpdate = false;

            globalAppointmentGraph = PXGraph.CreateInstance<AppointmentEntry>();
        }

        private bool needAppointmentDateUpdate = false;
        private bool needAppointmentTimeBeginUpdate = false;
        private bool vehicleChanged = false;
        private int? oldDriverID = null;
        private int? oldAdditionalDriverID = null;
        private int minutesToAdd = 0;
        public bool AutomaticallyUncloseRoute = false;

        #region Select
        [PXHidden]
        public PXSetup<FSSetup> SetupRecord;

        // Baccount workaround
        [PXHidden]
        public PXSelect<BAccount> BAccount;
        [PXHidden]
        public PXSelect<Contact> Contact;

        // Baccount workaround
        [PXHidden]
        public PXSelect<FSServiceOrder> ServiceOrder;

        [PXHidden]
        public PXSelect<FSAppointment> Appointment;

        public PXSetup<FSRouteSetup> RouteSetupRecord;

        public PXSelect<FSRouteDocument> RouteRecords;

        public PXSelect<FSRouteDocument,
               Where<
                   FSRouteDocument.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>>> RouteSelected;

        public PXSelect<FSAppointmentInRoute,
               Where<
                   FSAppointmentInRoute.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>>,
               OrderBy<
                   Asc<FSAppointmentInRoute.routePosition>>> AppointmentsInRoute;

        public PXSelect<FSAppointmentInRoute,
               Where<
                   FSAppointmentInRoute.routeDocumentID, Equal<Current<FSRouteDocument.routeDocumentID>>,
                   And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Completed>,
                   And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Canceled>,
                   And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Closed>>>>>,
               OrderBy<
                   Asc<FSAppointmentInRoute.routePosition>>> AppointmentsMobileValidation;

        public SharedClasses.RouteSelected_view VehicleRouteSelected;
        public SharedClasses.RouteSelected_view DriverRouteSelected;

        public VehicleSelectionHelper.VehicleRecords_View VehicleRecords;

        public DriverSelectionHelper.DriverRecords_View DriverRecords;

        [PXViewName(CR.Messages.Answers)]
        public CRAttributeList<FSRouteDocument> Answers;

        public PXFilter<VehicleSelectionFilter> VehicleFilter;
        public PXFilter<DriverSelectionFilter> DriverFilter;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }

        #endregion

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSRouteDocument>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSAppointment), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSAppointment.routeDocumentID>(((RouteDocumentMaint)graph).RouteRecords.Current?.RouteDocumentID)
                        };
                    }));
            }
        }

        #region CacheAttached
        #region FSRouteDocument_TotalDuration

        [PXUIField(DisplayName = "Total Driving Duration [*]", Enabled = false)]
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        protected virtual void FSRouteDocument_TotalDuration_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteDocument_TotalDistanceFriendly

        [PXDBString]
        [PXUIField(DisplayName = "Total Distance [*]", Enabled = false)]
        protected virtual void FSRouteDocument_TotalDistanceFriendly_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region FSRouteDocument_TotalTravelTime

        [PXUIField(DisplayName = "Total Route Duration [*]", Enabled = false)]
        [PXDBTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        protected virtual void FSRouteDocument_TotalTravelTime_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Selectors

        #region Vehicle Selector
        protected virtual IEnumerable vehicleRecords()
        {
            bool isVehicleSelectorButtonEnabled = true;

            isVehicleSelectorButtonEnabled = OpenVehicleSelector.GetEnabled();

            if (isVehicleSelectorButtonEnabled)
            {
                return VehicleSelectionHelper.VehicleRecordsDelegate(this, VehicleRouteSelected, VehicleFilter);
            }

            return null;
        }

        public PXAction<FSRouteDocument> OpenVehicleSelector;
        [PXButton]
        [PXUIField(DisplayName = "Vehicle Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openVehicleSelector()
        {
            if (RouteRecords.Current != null)
            {
                VehicleFilter.Current.RouteDocumentID = RouteRecords.Current.RouteDocumentID;
                VehicleRouteSelected.Current = RouteRecords.Current;

                FSRoute fsRouteRow = PXSelectJoin<FSRoute,
                                     InnerJoin<FSRouteDocument,
                                     On<
                                         FSRoute.routeID, Equal<FSRouteDocument.routeID>>>,
                                     Where<
                                         FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                                     .Select(this, RouteRecords.Current.RouteDocumentID);

                if (VehicleRouteSelected.AskExt() == WebDialogResult.OK
                        && VehicleRecords.Current != null
                            && RouteRecords.Current.VehicleID != VehicleRecords.Current.SMEquipmentID)
                {
                    RouteRecords.Current.VehicleID = VehicleRecords.Current.SMEquipmentID;
                    RouteRecords.Update(RouteRecords.Current);
                }
            }
        }

        #endregion

        #region Driver Selector
        protected virtual IEnumerable driverRecords()
        {
            return DriverSelectionHelper.DriverRecordsDelegate(this, DriverRouteSelected, DriverFilter);
        }

        public PXAction<FSRouteDocument> OpenDriverSelector;
        [PXButton]
        [PXUIField(DisplayName = "Driver Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openDriverSelector()
        {
            if (RouteRecords.Current != null)
            {
                DriverRouteSelected.Current = RouteRecords.Current;

                if (DriverRouteSelected.AskExt() == WebDialogResult.OK
                        && DriverRecords.Current != null
                        && RouteRecords.Current.DriverID != DriverRecords.Current.BAccountID)
                {
                    RouteRecords.Current = this.RouteRecords.Search<FSRouteDocument.routeDocumentID>(RouteRecords.Current.RouteDocumentID);
                    RouteRecords.Current.DriverID = DriverRecords.Current.BAccountID;
                    RouteRecords.Update(RouteRecords.Current);
                }
            }
        }

        #endregion

        #region Reassign Appointment Selector

        public RouteAppointmentAssignmentHelper.RouteRecords_View RouteAppAssignmentRecords;
        public PXFilter<RouteAppointmentAssignmentFilter> RouteAppAssignmentFilter;

        public PXAction<FSRouteDocument> reassignAppointment;
        [PXUIField(DisplayName = "Reassign", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ReassignAppointment()
        {
            if (AppointmentsInRoute.Current != null && RouteRecords.Current != null)
            {
                var appointmentSelected = PXSelectReadonly2<FSAppointment,
                                          InnerJoin<FSServiceOrder,
                                          On<
                                              FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                                          InnerJoin<FSAddress, 
                                          On<
                                              FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>>>,
                                          Where<
                                              FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                                          .Select(this, AppointmentsInRoute.Current.AppointmentID);

                PXResult<FSAppointment, FSServiceOrder, FSAddress> bqlResult = (PXResult<FSAppointment, FSServiceOrder, FSAddress>)appointmentSelected;
                FSAppointment fsAppointmentRow = (FSAppointment)bqlResult;
                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)bqlResult;
                FSAddress fsAddressRow = (FSAddress)bqlResult;

                RouteAppAssignmentFilter.Current.AppointmentID = AppointmentsInRoute.Current.AppointmentID;
                RouteAppAssignmentFilter.Current.SrvOrdType = AppointmentsInRoute.Current.SrvOrdType;
                RouteAppAssignmentFilter.Current.RefNbr = AppointmentsInRoute.Current.RefNbr;
                RouteAppAssignmentFilter.Current.RouteDocumentID = AppointmentsInRoute.Current.RouteDocumentID;
                RouteAppAssignmentFilter.Current.RouteDate = RouteRecords.Current.Date;

                RouteAppAssignmentFilter.Current.AppRefNbr = fsAppointmentRow.RefNbr;
                RouteAppAssignmentFilter.Current.AppSrvOrdType = fsAppointmentRow.SrvOrdType;
                RouteAppAssignmentFilter.Current.EstimatedDurationTotal = fsAppointmentRow.EstimatedDurationTotal;
                RouteAppAssignmentFilter.Current.ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin;

                RouteAppAssignmentFilter.Current.AddressLine1 = fsAddressRow.AddressLine1;
                RouteAppAssignmentFilter.Current.AddressLine2 = fsAddressRow.AddressLine2;
                RouteAppAssignmentFilter.Current.City = fsAddressRow.City;
                RouteAppAssignmentFilter.Current.State = fsAddressRow.State;
                RouteAppAssignmentFilter.Current.CustomerID = fsServiceOrderRow.CustomerID;
                RouteAppAssignmentFilter.Current.LocationID = fsServiceOrderRow.LocationID;

                RouteAppAssignmentFilter.AskExt();
            }
        }

        protected virtual IEnumerable routeAppAssignmentRecords()
        {
            return RouteAppointmentAssignmentHelper.RouteRecordsDelegate(RouteAppAssignmentFilter,
                                                                         new RouteAppointmentAssignmentHelper.RouteRecords_View(this));
        }

        #region SelectCurrentRoute
        public PXAction<FSRouteDocument> selectCurrentRoute;
        [PXUIField(DisplayName = "Reassign to current Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void SelectCurrentRoute()
        {
            if (RouteAppAssignmentRecords.Current != null && RouteAppAssignmentFilter.Current != null)
            {
                RouteAppointmentAssignmentHelper.ReassignAppointmentToRoute(RouteAppAssignmentRecords.Current, RouteAppAssignmentFilter.Current.RefNbr, RouteAppAssignmentFilter.Current.SrvOrdType);
                this.Actions.PressCancel();
            }
        }
        #endregion

        #endregion

        #region Unassign Appointment Selector

        public PXFilter<RouteAppointmentAssignmentFilter> RouteAppUnassignmentFilter;

        public PXAction<FSRouteDocument> unassignAppointment;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Refresh)]
        public virtual void UnassignAppointment()
        {
            if (AppointmentsInRoute.Current == null)
            {
                return;
            }

            RouteAppAssignmentFilter.Current.SrvOrdType = AppointmentsInRoute.Current.SrvOrdType;
            RouteAppAssignmentFilter.Current.RefNbr = AppointmentsInRoute.Current.RefNbr;

            if (WebDialogResult.Yes == RouteAppUnassignmentFilter.Ask(TX.WebDialogTitles.CONFIRM_UNASSIGN_APPOINTMENT,
                                                                      TX.Messages.ASK_CONFIRM_DELETE_APPOINTMENT_FROM_DB,
                                                                      MessageButtons.YesNo))
            {
                RouteAppointmentAssignmentHelper.DeleteAppointmentRoute(RouteAppAssignmentFilter.Current.RefNbr, RouteAppAssignmentFilter.Current.SrvOrdType);
                this.Actions.PressCancel();
            }
        }

        #endregion

        #endregion

        #region Actions

        public PXAction<FSRouteDocument> DeleteRoute;
        [PXButton(ImageSet = "main", ImageKey = "Remove")]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void deleteRoute()
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;

            if (fsRouteDocumentRow != null)
            {
                if (WebDialogResult.Yes == RouteRecords.Ask(
                                                            TX.WebDialogTitles.CONFIRM_ROUTE_DELETING,
                                                            TX.Messages.ASK_CONFIRM_DELETE_CURRENT_ROUTE,
                                                            MessageButtons.YesNo))
                {
                    // Verifies if there are Appointments/ServiceOrders with status closed or completed.
                    var fsAppointmentSet = PXSelectJoin<FSAppointment,
                                           InnerJoin<FSServiceOrder, On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
                                           Where2<
                                                Where<
                                                    FSAppointment.status, Equal<ListField_Status_Appointment.Completed>,
                                                    Or<FSAppointment.status, Equal<ListField_Status_Appointment.Closed>, 
                                                    Or<FSServiceOrder.status, Equal<ListField_Status_ServiceOrder.Completed>,
                                                    Or<FSServiceOrder.status, Equal<ListField_Status_ServiceOrder.Closed>>>>>,
                                                And<
                                                    FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>>>
                                           .Select(this, fsRouteDocumentRow.RouteDocumentID);

                    // If at least one row is returned, does not let delete the route.
                    if (fsAppointmentSet.Count > 0)
                    {
                        throw new PXException(TX.Error.CANNOT_DELETE_ROUTE_APP_SO_STATUS);
                    }

                    using (var ts = new PXTransactionScope())
                    {
                        var appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();

                        foreach (FSAppointment fsAppointmentRow in AppointmentsInRoute.Select())
                        {
                            appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords
                                                                               .Search<FSAppointment.appointmentID>
                                                                               (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                            appointmentEntryGraph.NeedRecalculateRouteStats = false;
                            appointmentEntryGraph.CalculateGoogleStats = false;
                            appointmentEntryGraph.AvoidCalculateRouteStats = true;

                            appointmentEntryGraph.AppointmentRecords.Delete(fsAppointmentRow);

                            appointmentEntryGraph.Save.Press();
                        }

                        RouteRecords.Delete(fsRouteDocumentRow);
                        this.Save.Press();

                        ts.Complete();
                    }
                }
            }
        }

        #region ActionsMenu
        public PXMenuAction<FSRouteDocument> actionsMenu;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable ActionsMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }
        #endregion

        public PXAction<FSRouteDocument> openRoutesOnMap;
        [PXButton]
        [PXUIField(DisplayName = "Open Route On Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenRoutesOnMap()
        {
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("RefNbr", RouteRecords.Current.RefNbr),
                new KeyValuePair<string, string>("Date", RouteRecords.Current.Date.Value.ToString()),
                new KeyValuePair<string, string>("BranchID", RouteRecords.Current.BranchID.Value.ToString())
            };

            throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.ROUTES_ON_MAP, parameters);
        }

        public PXAction<FSRouteDocument> openDriverCalendar;
        [PXButton]
        [PXUIField(DisplayName = "Open Driver Calendar", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenDriverCalendar()
        {
            KeyValuePair<string, string>[] parameters = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("EmployeeID", RouteRecords.Current.DriverID.ToString()),
                new KeyValuePair<string, string>("Date", RouteRecords.Current.Date.Value.ToString())                
            };

            throw new PXRedirectToBoardRequiredException(Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH, parameters);
        }

        public PXAction<FSRouteDocument> openAppointment;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(DisplayName = "Open Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenAppointment()
        {
            if (AppointmentsInRoute.Current != null && AppointmentsInRoute.Current.RouteDocumentID != null)
            {
                AppointmentEntry appointmentEntryGraph = PXGraph.CreateInstance<AppointmentEntry>();
                appointmentEntryGraph.AppointmentRecords.Current = appointmentEntryGraph.AppointmentRecords
                                                                   .Search<FSAppointment.appointmentID>
                                                                   (AppointmentsInRoute.Current.AppointmentID, AppointmentsInRoute.Current.SrvOrdType);
                throw new PXRedirectRequiredException(appointmentEntryGraph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }

        [PXFilterable]
        public PXFilter<SrvOrderTypeRouteAux> ServiceOrderTypeSelector;

        public ServiceOrderCore.ServiceOrderTypeRouteRecords_View ServiceOrderTypeRecords;

        public PXAction<FSRouteDocument> addAppointment;
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void AddAppointment()
        {
            if (RouteRecords.Current == null || RouteRecords.Current.RouteDocumentID == null)
            {
                return;
            }

            if (ServiceOrderTypeRecords.Select().Count == 0)
            {
                throw new PXException(TX.Error.SO_TYPE_ROUTE_NOT_EXIST);
            }

            if (this.IsMobile != true)
            {
                if (ServiceOrderTypeSelector.AskExt() == WebDialogResult.OK)
                {
                    if (ServiceOrderTypeSelector.Current != null
                        && ServiceOrderTypeSelector.Current.SrvOrdType != null)
                    {
                        openAppointmentScreen(RouteRecords.Current, ServiceOrderTypeSelector.Current.SrvOrdType);
                    }
                }
            }
            else 
            {
                openAppointmentScreen(RouteRecords.Current, null);
            }
        }

        public PXAction<FSRouteDocument> up;
        [PXButton]
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void Up()
        {
            if (AppointmentsInRoute.Current != null && AppointmentsInRoute.Current.RouteDocumentID != null)
            {
                using (var ts = new PXTransactionScope())
                {
                    SharedFunctions.MoveAppointmentInRoute(this,
                                                           RouteRecords.Current,
                                                           AppointmentsInRoute.Current.RoutePosition,
                                                           AppointmentsInRoute.Current.RoutePosition - 1);
                    ts.Complete();
                }
            }

            this.Actions.PressCancel();

            this.RouteSelected.Current.MustRecalculateStats = true;
            this.RouteSelected.Update(this.RouteSelected.Current);
        }

        public PXAction<FSRouteDocument> down;
        [PXButton]
        [PXUIField(DisplayName = " ", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void Down()
        {
            if (AppointmentsInRoute.Current != null && AppointmentsInRoute.Current.RouteDocumentID != null)
            {
                using (var ts = new PXTransactionScope())
                {
                    SharedFunctions.MoveAppointmentInRoute(this,
                                                           RouteRecords.Current,
                                                           AppointmentsInRoute.Current.RoutePosition,
                                                           AppointmentsInRoute.Current.RoutePosition + 1);
                    ts.Complete();
                }
            }

            this.Actions.PressCancel();

            this.RouteSelected.Current.MustRecalculateStats = true;
            this.RouteSelected.Update(this.RouteSelected.Current);
        }

        public PXDBAction<FSRouteDocument> openCustomerLocation;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenCustomerLocation()
        {
            LocationHelper.OpenCustomerLocation(this, AppointmentsInRoute.Current.SOID);
        }

        #region StartRoute
        public PXAction<FSRouteDocument> startRoute;
        [PXUIField(DisplayName = "Start Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable StartRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;
            if (RouteRecords.Cache.GetStatus(fsRouteDocumentRow) != PXEntryStatus.Inserted)
            {
                string errorMessage = string.Empty;

                if (!IsThisStatusTransitionAllowed(fsRouteDocumentRow, ID.Status_Route.IN_PROCESS, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                if (!AreThereAnyAppointmentsInThisRoute(ID.Status_Route.IN_PROCESS, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                if (fsRouteDocumentRow.DriverID == null || fsRouteDocumentRow.VehicleID == null)
                {
                    string displayMessage = string.Empty;

                    if (fsRouteDocumentRow.DriverID == null)
                    {
                        displayMessage = TX.CustomTextFields.DRIVER_ID;

                        if (fsRouteDocumentRow.VehicleID == null)
                        {
                            displayMessage += TX.Messages.LIST_LAST_ITEM_PREFIX + TX.CustomTextFields.VEHICLE_ID;
                        }
                    }
                    else if (fsRouteDocumentRow.VehicleID == null)
                    {
                        displayMessage = TX.CustomTextFields.VEHICLE_ID;
                    }

                    if (RouteRecords.Ask(PXMessages.LocalizeFormatNoPrefix(TX.Warning.ROUTE_MISSING_DRIVER_OR_AND_VEHICLE, displayMessage), MessageButtons.YesNo) == WebDialogResult.No)
                    {
                        return adapter.Get();
                    }
                }

                DateTime? currentDate = new DateTime(Accessinfo.BusinessDate.Value.Year,
                                                     Accessinfo.BusinessDate.Value.Month,
                                                     Accessinfo.BusinessDate.Value.Day,
                                                     fsRouteDocumentRow.MemBusinessDateTime.Value.Hour,
                                                     fsRouteDocumentRow.MemBusinessDateTime.Value.Minute,
                                                     fsRouteDocumentRow.MemBusinessDateTime.Value.Second);

                RouteRecords.Cache.SetValueExt<FSRouteDocument.actualStartTime>(fsRouteDocumentRow, currentDate);

                fsRouteDocumentRow.Status = ID.Status_Route.IN_PROCESS;
                RouteRecords.Cache.SetStatus(fsRouteDocumentRow, PXEntryStatus.Updated);

                if (IsMobile == true
                        && RouteSetupRecord.Current != null 
                            && RouteSetupRecord.Current.TrackRouteLocation == true 
                                && string.IsNullOrEmpty(fsRouteDocumentRow.GPSLatitudeLongitude) == false)
                {
                    string[] parts = fsRouteDocumentRow.GPSLatitudeLongitude.Split(':');
                    fsRouteDocumentRow.GPSLatitudeStart = decimal.Parse(parts[0]);
                    fsRouteDocumentRow.GPSLongitudeStart = decimal.Parse(parts[1]);
                }

                Save.Press();
            }

            return adapter.Get();
        }
        #endregion

        #region CompleteRoute
        public PXAction<FSRouteDocument> completeRoute;
        [PXUIField(DisplayName = "Complete Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CompleteRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;

            if (RouteRecords.Cache.GetStatus(fsRouteDocumentRow) != PXEntryStatus.Inserted)
            {
                if (this.IsMobile
                        && AppointmentsMobileValidation.Select().Count() > 0)
                {
                    throw new PXException(TX.Error.ROUTE_CANT_BE_COMPLETED_APPOINTMENTS_NEED_TO_BE_COMPLETED);
                }

                string errorMessage = string.Empty;

                if (!IsThisStatusTransitionAllowed(fsRouteDocumentRow, ID.Status_Route.COMPLETED, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                if (!AreThereAnyAppointmentsInThisRoute(ID.Status_Route.COMPLETED, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                if (CompleteAppointmentsInRoute(ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (fsRouteDocumentRow.TimeBegin == null)
                {
                    RouteRecords.Cache.SetStatus(fsRouteDocumentRow, PXEntryStatus.Updated);
                    PXDefaultAttribute.SetPersistingCheck<FSRouteDocument.timeBegin>(RouteRecords.Cache, fsRouteDocumentRow, PXPersistingCheck.NullOrBlank);
                    this.Save.Press();
                }
                else
                {
                    DateTime? currentDate = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate.Value, fsRouteDocumentRow.MemBusinessDateTime.Value);

                    if (this.IsMobile == true &&
                            fsRouteDocumentRow.ActualStartTime < currentDate)
                    {
                        RouteRecords.Cache.SetValueExt<FSRouteDocument.actualEndTime>(fsRouteDocumentRow, currentDate);
                    }

                    if (IsMobile == true
                            && RouteSetupRecord.Current != null
                                && RouteSetupRecord.Current.TrackRouteLocation == true
                                    && string.IsNullOrEmpty(fsRouteDocumentRow.GPSLatitudeLongitude) == false)
                    {
                        string[] parts = fsRouteDocumentRow.GPSLatitudeLongitude.Split(':');
                        fsRouteDocumentRow.GPSLatitudeComplete = decimal.Parse(parts[0]);
                        fsRouteDocumentRow.GPSLongitudeComplete = decimal.Parse(parts[1]);
                    }

                    fsRouteDocumentRow.Status = ID.Status_Route.COMPLETED;
                    RouteRecords.Update(fsRouteDocumentRow);
                    this.Save.Press();
                }
            }

            return adapter.Get();
        }
        #endregion

        #region CancelRoute
        public PXAction<FSRouteDocument> cancelRoute;
        [PXUIField(DisplayName = "Cancel Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CancelRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;
            if (RouteRecords.Cache.GetStatus(fsRouteDocumentRow) != PXEntryStatus.Inserted)
            {
                string errorMessage = string.Empty;

                if (!IsThisStatusTransitionAllowed(fsRouteDocumentRow, ID.Status_Route.CANCELED, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                if (CancelAppointmentsInRoute(ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                this.Cancel.Press();
                fsRouteDocumentRow.Status = ID.Status_Route.CANCELED;
                RouteRecords.Update(fsRouteDocumentRow);
                this.Save.Press();
            }

            return adapter.Get();
        }
        #endregion

        #region ReopenRoute
        public PXAction<FSRouteDocument> reopenRoute;
        [PXUIField(DisplayName = "Reopen Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ReopenRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;
            if (RouteRecords.Cache.GetStatus(fsRouteDocumentRow) != PXEntryStatus.Inserted)
            {
                string errorMessage = string.Empty;

                if (!IsThisStatusTransitionAllowed(fsRouteDocumentRow, ID.Status_Route.OPEN, ref errorMessage))
                {
                    throw new PXException(errorMessage);
                }

                RouteRecords.Cache.AllowUpdate = true;
                fsRouteDocumentRow.Status = ID.Status_Route.OPEN;
                RouteRecords.Cache.SetStatus(fsRouteDocumentRow, PXEntryStatus.Updated);
                Save.Press();
            }

            return adapter.Get();
        }
        #endregion

        #region Unclose Route
        public PXAction<FSRouteDocument> uncloseRoute;
        [PXUIField(DisplayName = "Unclose Route", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable UncloseRoute(PXAdapter adapter)
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;

            if (fsRouteDocumentRow != null)
            {
                if (AutomaticallyUncloseRoute || WebDialogResult.Yes == RouteRecords.Ask(TX.WebDialogTitles.CONFIRM_ROUTE_UNCLOSING,
                                                                                         TX.Messages.ASK_CONFIRM_ROUTE_UNCLOSING,
                                                                                         MessageButtons.YesNo))
                {
                    SharedFunctions.UncloseRoute(RouteRecords.Cache, fsRouteDocumentRow);
                }
            }

            return adapter.Get();
        }
        #endregion

        public PXAction<FSRouteDocument> calculateRouteStats;
        [PXButton]
        [PXUIField(DisplayName = "Calculate Route Statistics", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void CalculateRouteStats()
        {
            RouteDocumentMaint graphRouteDocumentMaint = PXGraph.CreateInstance<RouteDocumentMaint>();

            graphRouteDocumentMaint.RouteRecords.Current = graphRouteDocumentMaint.RouteRecords.Search<FSRouteDocument.routeDocumentID>(RouteRecords.Current.RouteDocumentID);

            PXLongOperation.StartOperation(
             this,
             delegate
             {
                 AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
                 using (PXTransactionScope ts = new PXTransactionScope())
                 {
                     var fsAppointmentRows = graphRouteDocumentMaint.AppointmentsInRoute.SelectWindowed(0, 1);

                     if (fsAppointmentRows.Count > 0)
                     {
                         FSAppointment fsAppointmentRow = (FSAppointment)fsAppointmentRows;
                         AppointmentEntry.CalculateRouteStats(graphAppointmentEntry, fsAppointmentRow, SetupRecord.Current.MapApiKey);
                     }

                     graphRouteDocumentMaint.RouteRecords.Current.RouteStatsUpdated = true;

                     ts.Complete();
                 }
             });
        }

        #region OpenRouteSchedule
        public PXAction<FSRouteDocument> OpenRouteSchedule;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        protected virtual void openRouteSchedule()
        {
            if (AppointmentsInRoute.Current != null)
            {
                var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                         .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>
                                                                                         (AppointmentsInRoute.Current.ScheduleID,
                                                                                          AppointmentsInRoute.Current.ServiceContractID);

                throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        /// <summary>
        /// Open Appointment Screen with the given FSRouteDocument and Service Order Type.
        /// </summary>
        /// <param name="fsRouteDocumentRow"> Route Document.</param>
        /// <param name="srvOrdType"> Service Order Type.</param>
        public virtual void openAppointmentScreen(FSRouteDocument fsRouteDocumentRow, string srvOrdType)
        {
            AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            FSAppointment fsAppointmentRow = new FSAppointment();

            fsAppointmentRow.RouteDocumentID = fsRouteDocumentRow.RouteDocumentID;
            fsAppointmentRow.RouteID = fsRouteDocumentRow.RouteID;

            if (string.IsNullOrEmpty(srvOrdType) == false)
            {
                fsAppointmentRow.SrvOrdType = ServiceOrderTypeSelector.Current.SrvOrdType;
            }

            fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Insert(fsAppointmentRow);

            if (fsRouteDocumentRow.DriverID != null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                fsAppointmentEmployeeRow.EmployeeID = fsRouteDocumentRow.DriverID;
                fsAppointmentEmployeeRow.IsDriver = true;
                graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }

            if (fsRouteDocumentRow.AdditionalDriverID != null)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();
                fsAppointmentEmployeeRow.EmployeeID = fsRouteDocumentRow.AdditionalDriverID;
                fsAppointmentEmployeeRow.IsDriver = true;
                graphAppointmentEntry.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }

            if (fsRouteDocumentRow.VehicleID != null)
            {
                fsAppointmentRow.VehicleID = fsRouteDocumentRow.VehicleID;
                fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);
            }

            bool isAutoCalculateRouteStatsOn = RouteSetupRecord.Current != null && RouteSetupRecord.Current.AutoCalculateRouteStats == true;

            DateTime? appointmentScheduleStartTime = isAutoCalculateRouteStatsOn ? fsAppointmentRow.ScheduledDateTimeBegin : fsRouteDocumentRow.TimeBegin;

            graphAppointmentEntry.AppointmentRecords.SetValueExt<FSAppointment.scheduledDateTimeBegin>
                                                     (fsAppointmentRow, PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, appointmentScheduleStartTime));
            fsAppointmentRow = graphAppointmentEntry.AppointmentRecords.Update(fsAppointmentRow);

            throw new PXRedirectRequiredException(graphAppointmentEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }

        public PXAction<FSRouteDocument> viewStartGPSOnMap;
        [PXUIField(DisplayName = "View on Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewStartGPSOnMap(PXAdapter adapter)
        {
            if (RouteRecords.Current != null && RouteRecords.Current.RouteDocumentID != null)
            {
                var googleMap = new PX.Data.GoogleMapLatLongRedirector();

                googleMap.ShowAddressByLocation(RouteRecords.Current.GPSLatitudeStart, RouteRecords.Current.GPSLongitudeStart);
            }

            return adapter.Get();
        }

        public PXAction<FSRouteDocument> viewCompleteGPSOnMap;
        [PXUIField(DisplayName = "View on Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewCompleteGPSOnMap(PXAdapter adapter)
        {
            if (RouteRecords.Current != null && RouteRecords.Current.RouteDocumentID != null)
            {
                var googleMap = new PX.Data.GoogleMapLatLongRedirector();

                googleMap.ShowAddressByLocation(RouteRecords.Current.GPSLatitudeComplete, RouteRecords.Current.GPSLongitudeComplete);
            }

            return adapter.Get();
        }
        #endregion

        #region Virtual Functions
        
        /// <summary>
        /// Completes all appointments belonging to the current Route, in case an error occurs with any appointment,
        /// the route will not be completed and a message will be displayed alerting the user about the appointment's issue.
        /// The row of the appointment having problems is marked with its error.
        /// </summary>
        /// <param name="errorMessage">Error message to be displayed.</param>
        /// <returns>True in case all appointments are completed, otherwise False.</returns>
        public virtual bool CompleteAppointmentsInRoute(ref string errorMessage)
        {
            bool allAppointmentsCompleted = true;

            PXResultset<FSAppointmentInRoute> bqlResultSet = AppointmentsInRoute.Select();

            if (bqlResultSet.Count > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CompleteAppointments(this, bqlResultSet);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        AppointmentsInRoute.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(kvp.Key,
                                                                                               kvp.Key.RefNbr,
                                                                                               new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    allAppointmentsCompleted = false;
                    errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ROUTE_CANT_BE_COMPLETED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES);
                }
            }

            return allAppointmentsCompleted;
        }

        private bool CancelAppointmentsInRoute(ref string errorMessage)
        {
            bool allAppointmentThatCanBeCanceled = true;

            var appointments = AppointmentsInRoute.Select()
                                                  .Where(x => ((FSAppointment)x).Status != ID.Status_Appointment.COMPLETED
                                                                && ((FSAppointment)x).Status != ID.Status_Appointment.CLOSED
                                                                && ((FSAppointment)x).Status != ID.Status_Appointment.CANCELED)
                                                  .ToList();

            if (appointments.Count() > 0)
            {
                Dictionary<FSAppointment, string> appWithErrors = SharedFunctions.CancelAppointments(this, appointments);

                if (appWithErrors.Count > 0)
                {
                    foreach (KeyValuePair<FSAppointment, string> kvp in appWithErrors)
                    {
                        AppointmentsInRoute.Cache.RaiseExceptionHandling<FSAppointment.refNbr>(kvp.Key,
                                                                                               kvp.Key.RefNbr,
                                                                                               new PXSetPropertyException(kvp.Value, PXErrorLevel.RowError));
                    }

                    allAppointmentThatCanBeCanceled = false;
                    errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ROUTE_CANT_BE_CANCELED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES);
                }
            }

            return allAppointmentThatCanBeCanceled;
        }

        /// <summary>
        /// Enable/Disable the CalculateRouteStats button depending on FSRouteSetup.CalculateRouteStats flag.
        /// </summary>
        /// <param name="routeHasAppointments">Indicates if there are appointments in the grid.</param>
        /// <param name="fsRouteDocumentRow">Current Route Document instance.</param>
        public virtual void EnableDisableCalcRouteStatsButton(bool routeHasAppointments, FSRouteDocument fsRouteDocumentRow)
        {
            calculateRouteStats.SetEnabled(routeHasAppointments == true
                                                    && RouteRecords.Cache.AllowUpdate == true
                                                        && ((RouteSetupRecord.Current.AutoCalculateRouteStats == false
                                                                && fsRouteDocumentRow.RouteStatsUpdated == false)
                                                            || (fsRouteDocumentRow.TotalTravelTime == null
                                                                || fsRouteDocumentRow.TotalDistance == null
                                                                || string.IsNullOrEmpty(fsRouteDocumentRow.TotalDistanceFriendly) == true
                                                                || fsRouteDocumentRow.TotalDuration == null)));
        }

        /// <summary>
        /// Enable/Disable document fields and buttons.
        /// </summary>
        public virtual void EnableDisableDocument(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            bool isTheRouteDocumentSavedInTheDB = fsRouteDocumentRow.RouteDocumentID > 0;
            bool routeHasAppointments = AppointmentsInRoute.Select().Count > 0;

            openDriverCalendar.SetEnabled(isTheRouteDocumentSavedInTheDB && fsRouteDocumentRow.DriverID != null);

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.branchID>(cache,
                                                                    fsRouteDocumentRow,
                                                                    !routeHasAppointments && !isTheRouteDocumentSavedInTheDB);
            PXUIFieldAttribute.SetEnabled<FSRouteDocument.routeID>(cache, fsRouteDocumentRow, !isTheRouteDocumentSavedInTheDB);

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.actualStartTime>(cache,
                                                                           fsRouteDocumentRow,
                                                                           fsRouteDocumentRow.Status != ID.Status_Route.OPEN
                                                                           && fsRouteDocumentRow.Status != ID.Status_Route.CANCELED
                                                                           && fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);

            PXUIFieldAttribute.SetEnabled<FSRouteDocument.actualEndTime>(cache,
                                                                         fsRouteDocumentRow,
                                                                         fsRouteDocumentRow.Status != ID.Status_Route.OPEN
                                                                         && fsRouteDocumentRow.Status != ID.Status_Route.CANCELED
                                                                         && fsRouteDocumentRow.Status != ID.Status_Route.CLOSED);

            AllowUpdateRouteDocument(fsRouteDocumentRow);
            AllowDeleteRouteDocument(fsRouteDocumentRow);

            EnableDisableCalcRouteStatsButton(routeHasAppointments, fsRouteDocumentRow);
        }

        /// <summary>
        /// Enables/Disables the Update process in Route Document.
        /// </summary>
        public virtual void AllowUpdateRouteDocument(FSRouteDocument fsRouteDocumentRow)
        {
            RouteRecords.Cache.AllowUpdate = fsRouteDocumentRow.Status != ID.Status_Route.CANCELED
                                                    && fsRouteDocumentRow.Status != ID.Status_Route.CLOSED;
        }

        /// <summary>
        /// Enables/Disables the Delete process in Route Document.
        /// </summary>
        public virtual void AllowDeleteRouteDocument(FSRouteDocument fsRouteDocumentRow) 
        {
            bool allowDelete = fsRouteDocumentRow.Status != ID.Status_Route.COMPLETED
                                    && fsRouteDocumentRow.Status != ID.Status_Route.CANCELED
                                        && fsRouteDocumentRow.Status != ID.Status_Route.CLOSED;

            RouteRecords.Cache.AllowDelete = allowDelete;
            DeleteRoute.SetEnabled(allowDelete);
        }

        /// <summary>
        /// Enables/Disables the actions defined for ServiceContract
        /// It's called by RowSelected event of FSServiceContract.
        /// </summary>
        public virtual void EnableDisable_ActionButtons(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            bool enableAppointmentGridActions  = !(cache.GetStatus(fsRouteDocumentRow) == PXEntryStatus.Inserted)
                                                    && fsRouteDocumentRow.Status != ID.Status_Route.COMPLETED
                                                        && fsRouteDocumentRow.Status != ID.Status_Route.CANCELED
                                                            && fsRouteDocumentRow.Status != ID.Status_Route.CLOSED;

            bool enableCancelAndCompleteActions = !(cache.GetStatus(fsRouteDocumentRow) == PXEntryStatus.Inserted)
                                                    && (fsRouteDocumentRow.Status == ID.Status_Route.OPEN
                                                            || fsRouteDocumentRow.Status == ID.Status_Route.IN_PROCESS);

            bool enableUpAndDownActions = AppointmentsInRoute.Select().Count > 1;

            addAppointment.SetEnabled(enableAppointmentGridActions);
            unassignAppointment.SetEnabled(enableAppointmentGridActions);
            reassignAppointment.SetEnabled(enableAppointmentGridActions);
            up.SetEnabled(enableAppointmentGridActions && enableUpAndDownActions);
            down.SetEnabled(enableAppointmentGridActions && enableUpAndDownActions);

            openRoutesOnMap.SetEnabled(!(RouteRecords.Cache.GetStatus(fsRouteDocumentRow) == PXEntryStatus.Inserted));
            startRoute.SetEnabled(!(cache.GetStatus(fsRouteDocumentRow) == PXEntryStatus.Inserted)
                                    && fsRouteDocumentRow.Status == ID.Status_Route.OPEN);

            completeRoute.SetEnabled(enableCancelAndCompleteActions);
            cancelRoute.SetEnabled(enableCancelAndCompleteActions);
            reopenRoute.SetEnabled(!(cache.GetStatus(fsRouteDocumentRow) == PXEntryStatus.Inserted) 
                                        && (fsRouteDocumentRow.Status == ID.Status_Route.COMPLETED
                                            || fsRouteDocumentRow.Status == ID.Status_Route.CANCELED
                                                || fsRouteDocumentRow.Status == ID.Status_Route.IN_PROCESS));
            uncloseRoute.SetEnabled(fsRouteDocumentRow.Status == ID.Status_Route.CLOSED);
        }

        /// <summary>
        /// Sets (if it is defined) the TimeBegin of the Route Document depending on the execution day of the RouteID
        /// and the Day in which the Route is taking place.
        /// </summary>
        /// <param name="fsRouteDocumentRow">FSRouteDocument Row.</param>
        public virtual void SetRouteStartTimeByRouteID(FSRouteDocument fsRouteDocumentRow)
        {
            FSRoute fsRouteRow = PXSelect<FSRoute, Where<FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>.Select(this, fsRouteDocumentRow.RouteID);

            switch (fsRouteDocumentRow.Date.Value.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    if (fsRouteRow.ActiveOnMonday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnMonday);
                    }

                    break;
                case DayOfWeek.Tuesday:
                    if (fsRouteRow.ActiveOnTuesday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnTuesday);
                    }

                    break;
                case DayOfWeek.Wednesday:
                    if (fsRouteRow.ActiveOnWednesday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnWednesday);
                    }

                    break;
                case DayOfWeek.Thursday:
                    if (fsRouteRow.ActiveOnThursday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnThursday);
                    }

                    break;
                case DayOfWeek.Friday:
                    if (fsRouteRow.ActiveOnFriday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnFriday);
                    }

                    break;
                case DayOfWeek.Saturday:
                    if (fsRouteRow.ActiveOnSaturday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnSaturday);
                    }

                    break;
                case DayOfWeek.Sunday:
                    if (fsRouteRow.ActiveOnSunday == true)
                    {
                        fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.TimeBegin, fsRouteRow.BeginTimeOnSunday);
                    }

                    break;
            }
        }

        /// <summary>
        /// Validates if there are any appointments assigned for the current route
        /// Return false if the route has no appointments and sets a [errorMessage] to display.
        /// </summary>
        public virtual bool AreThereAnyAppointmentsInThisRoute(string routeStatus, ref string errorMessage)
        {
            errorMessage = string.Empty;

            if (AppointmentsInRoute.Select().Count == 0)
            {
                switch (routeStatus)
                { 
                    case ID.Status_Route.IN_PROCESS:
                        errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ROUTE_NEED_APPOINTMENTS_TO_CHANGE_STATUS, "started");
                        break;
                    case ID.Status_Route.COMPLETED:
                        errorMessage = PXMessages.LocalizeFormatNoPrefix(TX.Error.ROUTE_NEED_APPOINTMENTS_TO_CHANGE_STATUS, "completed");
                        break;
                    default:
                        break;
                }
                
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates basics fields for an appointment address showing a warning if there are incomplete fields.
        /// </summary>
        public virtual void CheckAppointmentsAddress(PXCache cache, FSAppointment fsAppointmentRow)
        {
            FSAddress fsAddressRow = PXSelectJoin<FSAddress,
                                     InnerJoin<FSServiceOrder, 
                                     On<
                                         FSServiceOrder.serviceOrderAddressID, Equal<FSAddress.addressID>>>,
                                     Where<
                                         FSServiceOrder.sOID, Equal<Required<FSAppointment.sOID>>>>
                                     .Select(this, fsAppointmentRow.SOID);

            if (fsAddressRow != null)
            {
                if ((string.IsNullOrEmpty(fsAddressRow.AddressLine1)
                        && string.IsNullOrEmpty(fsAddressRow.AddressLine2))
                    || string.IsNullOrEmpty(fsAddressRow.City)
                        || string.IsNullOrEmpty(fsAddressRow.State)
                            || string.IsNullOrEmpty(fsAddressRow.PostalCode))
                {
                    cache.RaiseExceptionHandling<FSAppointment.refNbr>(fsAppointmentRow,
                                                                       fsAppointmentRow.RefNbr,
                                                                       new PXSetPropertyException(TX.Warning.INCOMPLETE_APPOINTMENT_ADDRESS,
                                                                                                  PXErrorLevel.Warning));
                }
            }            
        }

        /// <summary>
        /// Updates appointment's time after modify Route Document time begin.
        /// </summary>
        /// <param name="fsRouteDocumentRow">FSRouteDocument object.</param>
        public virtual void UpdateAppointmentsTimeBegin(FSRouteDocument fsRouteDocumentRow)
        {
            if (minutesToAdd == 0)
            {
                return;
            }

            var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

            graphAppointmentEntry.CalculateGoogleStats = false;

            foreach (FSAppointment fsAppointmentRow in AppointmentsInRoute.Select())
            {
                if (fsAppointmentRow.Status != ID.Status_Appointment.CLOSED
                        && fsAppointmentRow.Status != ID.Status_Appointment.CANCELED)
                {
                    graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.appointmentID>
                                                                                (fsAppointmentRow.AppointmentID, fsAppointmentRow.SrvOrdType);

                    fsAppointmentRow.ScheduledDateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(minutesToAdd));
                    fsAppointmentRow.ScheduledDateTimeEnd = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, fsAppointmentRow.ScheduledDateTimeEnd.Value.AddMinutes(minutesToAdd));

                    graphAppointmentEntry.AppointmentRecords.Current.ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin;
                    graphAppointmentEntry.AppointmentRecords.Current.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd;

                    graphAppointmentEntry.AppointmentRecords.Update(graphAppointmentEntry.AppointmentRecords.Current);

                    graphAppointmentEntry.Save.Press();
                }
            }

            graphAppointmentEntry.CalculateGoogleStats = true;
            minutesToAdd = 0;
            needAppointmentDateUpdate = false;
            needAppointmentTimeBeginUpdate = false;
        }

        /// <summary>
        /// Check if the driver and additional driver are equal.
        /// </summary>
        /// <param name="cache">FSRouteDocument cache.</param>
        /// <param name="fsRouteDocumentRow">FSRouteDocument Row.</param>
        public virtual void ValidateDrivers(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            if (fsRouteDocumentRow.DriverID != null
                && fsRouteDocumentRow.DriverID == fsRouteDocumentRow.AdditionalDriverID)
            {
                cache.RaiseExceptionHandling<FSRouteDocument.additionalDriverID>(fsRouteDocumentRow, 
                                                                                 fsRouteDocumentRow.AdditionalDriverID,
                                                                                 new PXSetPropertyException(TX.Error.ADDITIONAL_DRIVER_EQUAL_MAIN_DRIVER, PXErrorLevel.Error));

                throw new PXException(TX.Error.ADDITIONAL_DRIVER_EQUAL_MAIN_DRIVER, PXErrorLevel.Error);
            }
        }

        /// <summary>
        /// Check if the vehicles given are not repeated.
        /// </summary>
        /// <param name="cache">FSRouteDocument cache.</param>
        /// <param name="fsRouteDocumentRow">FSRouteDocument Row.</param>
        public virtual void ValidateVehicles(PXCache cache, FSRouteDocument fsRouteDocumentRow)
        {
            bool throwException = false;

            if (fsRouteDocumentRow.VehicleID != null 
                && fsRouteDocumentRow.VehicleID == fsRouteDocumentRow.AdditionalVehicleID1)
            {
                cache.RaiseExceptionHandling<FSRouteDocument.additionalVehicleID1>(
                    fsRouteDocumentRow,
                    fsRouteDocumentRow.AdditionalVehicleID1,
                    new PXSetPropertyException(TX.Error.VEHICLES_CANNOT_BE_EQUAL, PXErrorLevel.Error));

                throwException = true;
            }

            if (fsRouteDocumentRow.VehicleID != null
                && fsRouteDocumentRow.VehicleID == fsRouteDocumentRow.AdditionalVehicleID2)
            {
                cache.RaiseExceptionHandling<FSRouteDocument.additionalVehicleID2>(
                    fsRouteDocumentRow,
                    fsRouteDocumentRow.AdditionalVehicleID2,
                    new PXSetPropertyException(TX.Error.VEHICLES_CANNOT_BE_EQUAL, PXErrorLevel.Error));

                throwException = true;
            }

            if (throwException == true)
            {
                throw new PXException(TX.Error.VEHICLES_CANNOT_BE_EQUAL, PXErrorLevel.Error);
            }
        }

        /// <summary>
        /// Validate if the Trip is Valid for the routeDate.
        /// </summary>
        /// <param name="fsRouteRow">FSRoute Row.</param>
        /// <returns>The name of the day if invalid, null otherwise.</returns>
        public virtual string InvalidDayTrip(FSRoute fsRouteRow) 
        {
            string dayOfWeek = null;

            PXResultset<FSRoute> bqlResultSet = PXSelectReadonly2<FSRoute,
                                                InnerJoin<FSRouteDocument,
                                                    On<FSRoute.routeID, Equal<FSRouteDocument.routeID>>>,
                                                Where<
                                                    FSRoute.routeID, Equal<Required<FSRoute.routeID>>,
                                                    And<FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>>>>
                                                .Select(this, fsRouteRow.RouteID, RouteRecords.Current.Date);

            int? tripsUsed = bqlResultSet.Count + 1;

            switch (RouteRecords.Current.Date.Value.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    if (tripsUsed > fsRouteRow.NbrTripOnMonday)
                    {
                        dayOfWeek = TX.WeekDays.MONDAY;
                    }

                    break;
                case DayOfWeek.Tuesday:
                    if (tripsUsed > fsRouteRow.NbrTripOnTuesday)
                    {
                        dayOfWeek = TX.WeekDays.TUESDAY;
                    }

                    break;
                case DayOfWeek.Wednesday:
                    if (tripsUsed > fsRouteRow.NbrTripOnWednesday)
                    {
                        dayOfWeek = TX.WeekDays.WEDNESDAY;
                    }

                    break;
                case DayOfWeek.Thursday:
                    if (tripsUsed > fsRouteRow.NbrTripOnThursday)
                    {
                        dayOfWeek = TX.WeekDays.THURSDAY;
                    }

                    break;
                case DayOfWeek.Friday:
                    if (tripsUsed > fsRouteRow.NbrTripOnFriday)
                    {
                        dayOfWeek = TX.WeekDays.FRIDAY;
                    }

                    break;
                case DayOfWeek.Saturday:
                    if (tripsUsed > fsRouteRow.NbrTripOnSaturday)
                    {
                        dayOfWeek = TX.WeekDays.SATURDAY;
                    }

                    break;
                case DayOfWeek.Sunday:
                    if (tripsUsed > fsRouteRow.NbrTripOnSunday)
                    {
                        dayOfWeek = TX.WeekDays.SUNDAY;
                    }

                    break;
            }

            return dayOfWeek;
        }

        /// <summary>
        /// Validate if the trip number already exist.
        /// </summary>
        /// <param name="fsRouteDocumentRow">Route Document Row.</param>
        /// <param name="tripNbr">Trip Number.</param>
        /// <returns>True is valid, false otherwise.</returns>
        public virtual bool TripIDAlreadyExist(FSRouteDocument fsRouteDocumentRow, int? tripNbr) 
        {
            FSRouteDocument fsRouterDocumentResult = PXSelectReadonly<FSRouteDocument,
                                                     Where<
                                                         FSRouteDocument.routeID, Equal<Required<FSRoute.routeID>>,
                                                         And<FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>,
                                                         And<FSRouteDocument.tripNbr, Equal<Required<FSRouteDocument.tripNbr>>>>>>
                                                     .Select(this, fsRouteDocumentRow.RouteID, fsRouteDocumentRow.Date, tripNbr);
            if (fsRouterDocumentResult != null) 
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the FuelType to the <c>fsRouteDocumentRow</c> using the FuelType in the <c>vechicleID</c> 
        /// </summary>
        public virtual void SetRouteFuelType(FSRouteDocument fsRouteDocumentRow, int? vechicleID)
        {
            FSVehicle fsVechicleRow = PXSelect<FSVehicle,
                                      Where<
                                          FSVehicle.SMequipmentID, Equal<Required<FSVehicle.SMequipmentID>>>>
                                      .Select(this, vechicleID);

            if (fsVechicleRow.FuelType != null)
            {
                fsRouteDocumentRow.FuelType = fsVechicleRow.FuelType;
            }
        }

        public virtual void UpdateAppointmentInRoute(FSRouteDocument fsRouteDocumentRow, bool dateTimeChanged, bool vehicleChanged, bool driverChanged, bool additionalDriverChanged)
        {
            if (dateTimeChanged == false && vehicleChanged == false && driverChanged == false && additionalDriverChanged == false)
            {
                return;
            }

            var graphAppointmentMaint = PXGraph.CreateInstance<AppointmentEntry>();

            PXResultset<FSAppointment> fsAppointmentResultSet = PXSelect<FSAppointment,
                                                                Where<
                                                                    FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>,
                                                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                                    And<FSAppointment.status, NotEqual<FSAppointment.status.Closed>>>>>
                                                                .Select(graphAppointmentMaint, fsRouteDocumentRow.RouteDocumentID);

            foreach (FSAppointment fsAppointmentRow in fsAppointmentResultSet)
            {
                graphAppointmentMaint.AppointmentRecords.Current = graphAppointmentMaint.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);

                if (dateTimeChanged == true)
                {
                    graphAppointmentMaint.AppointmentRecords.Current.ScheduledDateTimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(minutesToAdd));
                    graphAppointmentMaint.AppointmentRecords.Current.ScheduledDateTimeEnd = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, fsAppointmentRow.ScheduledDateTimeEnd.Value.AddMinutes(minutesToAdd));
                }

                if (vehicleChanged == true)
                {
                    graphAppointmentMaint.AppointmentRecords.Current.VehicleID = fsRouteDocumentRow.VehicleID;
                }

                if (dateTimeChanged == true || vehicleChanged == true)
                {
                    graphAppointmentMaint.AppointmentRecords.Update(graphAppointmentMaint.AppointmentRecords.Current);
                }

                if (driverChanged)
                {
                    AssignDriverToAppointmentsInRoute(graphAppointmentMaint, graphAppointmentMaint.AppointmentRecords.Current, fsRouteDocumentRow.DriverID, oldDriverID);
                }

                if (additionalDriverChanged)
                {
                    AssignDriverToAppointmentsInRoute(graphAppointmentMaint, graphAppointmentMaint.AppointmentRecords.Current, fsRouteDocumentRow.AdditionalDriverID, oldAdditionalDriverID);
                }

                graphAppointmentMaint.AvoidCalculateRouteStats = true;
                graphAppointmentMaint.SkipServiceOrderUpdate = true;
                graphAppointmentMaint.CalculateGoogleStats = false;

                try
                {
                    graphAppointmentMaint.RecalculateExternalTaxesSync = true;
                graphAppointmentMaint.Save.Press();
            }
                finally
                {
                    graphAppointmentMaint.RecalculateExternalTaxesSync = false;
                }
            }
        }

        /// <summary>
        /// Assigns driver of the route to the appointments in it.
        /// </summary>
        public virtual void AssignDriverToAppointmentsInRoute(AppointmentEntry graphAppointmentMaint, FSAppointment fsAppointmentRow, int? localNewDriverID, int? localOldDriverID)
        {
            PXResultset<FSAppointmentEmployee> fsAppointmentEmployeeOldResultSet = graphAppointmentMaint.AppointmentServiceEmployees.Search<FSAppointmentEmployee.employeeID, FSAppointmentEmployee.serviceLineRef>(localOldDriverID, null);

            PXResultset<FSAppointmentEmployee> fsAppointmentEmployeeNewResultSet = graphAppointmentMaint.AppointmentServiceEmployees.Search<FSAppointmentEmployee.employeeID, FSAppointmentEmployee.serviceLineRef>(localNewDriverID, null);

            if (localNewDriverID != null && fsAppointmentEmployeeNewResultSet.Count > 0)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeNewResultSet)
                {
                    fsAppointmentEmployeeRow.IsDriver = true;
                    graphAppointmentMaint.AppointmentServiceEmployees.Update(fsAppointmentEmployeeRow);
                }
            }

            if (localNewDriverID != null && fsAppointmentEmployeeNewResultSet.Count == 0)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();

                fsAppointmentEmployeeRow.AppointmentID = fsAppointmentRow.AppointmentID;
                fsAppointmentEmployeeRow.EmployeeID = localNewDriverID;
                fsAppointmentEmployeeRow.IsDriver = true;
                graphAppointmentMaint.AppointmentServiceEmployees.Insert(fsAppointmentEmployeeRow);
            }

            if (localOldDriverID != null && localOldDriverID != localNewDriverID && fsAppointmentEmployeeOldResultSet.Count > 0)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeOldResultSet)
                {
                    if (fsAppointmentEmployeeRow.IsDriver == true && fsAppointmentEmployeeRow.EmployeeID == localOldDriverID)
                    {
                        graphAppointmentMaint.AppointmentServiceEmployees.Delete(fsAppointmentEmployeeRow);
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if a Route Document [fsRouteDocumentRow] can change it's status to [newRouteStatus] based on the current status of the Route Document [fsRouteDocumentRow]
        /// If an error is detected is going to be assigned to the [errorMessage].
        /// </summary>
        public virtual bool IsThisStatusTransitionAllowed(FSRouteDocument fsRouteDocumentRow, string newRouteStatus, ref string errorMessage)
        {
            errorMessage = string.Empty;

            // Open => In Process
            if (fsRouteDocumentRow.Status == ID.Status_Route.OPEN
                        && newRouteStatus == ID.Status_Route.IN_PROCESS)
            {
                return true;
            }

            // Open/In Process => Completed
            if ((fsRouteDocumentRow.Status == ID.Status_Route.OPEN
                    || fsRouteDocumentRow.Status == ID.Status_Route.IN_PROCESS)
                && newRouteStatus == ID.Status_Route.COMPLETED)
            {
                return true;
            }

            // Open/In Process => Canceled
            if ((fsRouteDocumentRow.Status == ID.Status_Route.OPEN
                    || fsRouteDocumentRow.Status == ID.Status_Route.IN_PROCESS)
                && newRouteStatus == ID.Status_Route.CANCELED)
            {
                return true;
            }

            // Completed/Canceled/In Process => Open
            if ((fsRouteDocumentRow.Status == ID.Status_Route.COMPLETED
                    || fsRouteDocumentRow.Status == ID.Status_Route.CANCELED
                        || fsRouteDocumentRow.Status == ID.Status_Route.IN_PROCESS)
                && newRouteStatus == ID.Status_Route.OPEN)
            {
                return true;
            }

            errorMessage = TX.Error.INVALID_ROUTE_STATUS_TRANSITION;
            return false;
        }

        /// <summary>
        /// Normalize route position in the appointment list.
        /// </summary>
        public virtual void NormalizeAppointmentPosition()
        {
            var rows = this.AppointmentsInRoute.Select();
            int i = 1;

            using (var ts = new PXTransactionScope())
            {
                foreach (FSAppointmentInRoute bqlResult in rows)
                {
                    FSAppointment fsAppointmentRow = (FSAppointment)bqlResult;

                    PXUpdate<
                        Set<FSAppointment.routePosition, Required<FSAppointment.routePosition>>,
                    FSAppointment,
                    Where<
                        FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                    .Update(this, i, fsAppointmentRow.AppointmentID);

                    i++;
                }

                ts.Complete();
            }
        }

        public virtual void calculateSimpleRouteStatistic()
        {
            FSRouteDocument fsRouteDocumentRow = RouteRecords.Current;

            fsRouteDocumentRow.TotalNumAppointments = AppointmentsInRoute.Select().Count;

            PXResultset<FSAppointmentDet> bqlResultSet = PXSelectJoin<FSAppointmentDet,
                                                         InnerJoin<FSAppointment,
                                                         On<
                                                             FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>,
                                                         InnerJoin<FSSODet,
                                                         On<
                                                             FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>>,
                                                         Where<FSSODet.lineType, Equal<ListField_LineType_ALL.Service>,
                                                             And<FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>>>
                                                         .Select(this, fsRouteDocumentRow.RouteDocumentID);

            fsRouteDocumentRow.TotalServices = bqlResultSet.Count;
            fsRouteDocumentRow.TotalServicesDuration = 0;

            foreach (FSAppointmentDet fsAppointmentDetRow in bqlResultSet)
            {
                fsRouteDocumentRow.TotalServicesDuration += fsAppointmentDetRow.EstimatedDuration;
            }

            RouteRecords.Update(fsRouteDocumentRow);

            Save.Press();
        }

        /// <summary>
        /// @TODO: This is a temporal solution. It must be improved
        /// AC-107854
        /// AC-142850
        /// </summary>
        public virtual void CalculateStats()
        {
            PXLongOperation.StartOperation(
            this,
            delegate
            {
                using (PXTransactionScope ts = new PXTransactionScope())
                {
                    var fsAppointmentRows = this.AppointmentsInRoute.SelectWindowed(0, 1);

                    if (fsAppointmentRows.Count > 0)
                    {
                        FSAppointment fsAppointmentRow = (FSAppointment)fsAppointmentRows;
                        FSRouteDocument fsRouteDocumentRow = this.RouteSelected.Current;
                        AppointmentEntry.SetRouteMapStats(globalAppointmentGraph, fsAppointmentRow, fsRouteDocumentRow.RouteDocumentID, ref fsRouteDocumentRow, SetupRecord.Current.MapApiKey);
                    }

                    ts.Complete();
                }
            });
        }

        #endregion

        #region Events

        #region FSRouteDocument
        #region FieldSelecting
        #endregion
        #region FieldDefaulting
        protected virtual void _(Events.FieldDefaulting<FSRouteDocument, FSRouteDocument.actualStartTime> e)
        {
        }

        protected virtual void _(Events.FieldDefaulting<FSRouteDocument, FSRouteDocument.timeBegin> e)
        {
            e.NewValue = PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate.Value, PXTimeZoneInfo.Now);
        }

        protected virtual void _(Events.FieldDefaulting<FSRouteDocument, FSRouteDocument.tripNbr> e)
        {
            if (RouteRecords.Current == null)
            {
                return;
            }

            PXResultset<FSRouteDocument> bqlResultSet = PXSelectReadonly2<FSRouteDocument,
                                                        InnerJoin<FSRoute,
                                                            On<FSRouteDocument.routeID, Equal<FSRoute.routeID>>>,
                                                        Where<
                                                            FSRouteDocument.routeID, Equal<Required<FSRouteDocument.routeID>>,
                                                            And<FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>>>,
                                                        OrderBy<
                                                            Desc<FSRouteDocument.tripNbr>>>
                                                        .SelectSingleBound(this, null, RouteRecords.Current.RouteID, RouteRecords.Current.Date);

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)bqlResultSet;

            if (fsRouteDocumentRow != null)
            {
                e.NewValue = fsRouteDocumentRow.TripNbr + 1;
            }
            else
            {
                e.NewValue = 1;
            }
        }
        #endregion
        #region FieldUpdating
        //Cannot be changed to new event format
        protected virtual void FSRouteDocument_TimeBegin_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            DateTime? aux = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (aux.HasValue == true)
            {
                e.NewValue = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, aux);
            }
        }

        //Cannot be changed to new event format
        protected virtual void FSRouteDocument_ActualStartTime_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            fsRouteDocumentRow.ActualStartTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (fsRouteDocumentRow.ActualStartTime != null)
            {
                DateTime auxActualDateTimeBegin = new DateTime(fsRouteDocumentRow.Date.Value.Year,
                                                               fsRouteDocumentRow.Date.Value.Month,
                                                               fsRouteDocumentRow.Date.Value.Day,
                                                               fsRouteDocumentRow.ActualStartTime.Value.Hour,
                                                               fsRouteDocumentRow.ActualStartTime.Value.Minute,
                                                               fsRouteDocumentRow.ActualStartTime.Value.Second);

                fsRouteDocumentRow.ActualStartTime = auxActualDateTimeBegin;
            }

            e.NewValue = fsRouteDocumentRow.ActualStartTime;
            DateTime timeStart = (DateTime)fsRouteDocumentRow.ActualStartTime;

            if (fsRouteDocumentRow.ActualStartTime != null)
            {
                RouteRecords.Cache.SetValueExt<FSRouteDocument.actualEndTime>(fsRouteDocumentRow, timeStart.AddHours(1));
            }
            else
            {
                RouteRecords.Cache.SetValueExt<FSRouteDocument.actualEndTime>(fsRouteDocumentRow, null);
            }
        }

        protected virtual void _(Events.FieldUpdating<FSRouteDocument, FSRouteDocument.tripNbr> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            int? tripNbr = Convert.ToInt32(e.NewValue);

            fsRouteDocumentRow.TripNbr = tripNbr;

            if (TripIDAlreadyExist(fsRouteDocumentRow, tripNbr))
            {
                e.Cache.RaiseExceptionHandling<FSRouteDocument.tripNbr>(fsRouteDocumentRow,
                                                                        fsRouteDocumentRow.TripNbr,
                                                                        new PXException(TX.Error.ID_OF_TRIPS_ALREADY_EXIST));
            }
        }
        #endregion
        #region FieldVerifying
        #endregion
        #region FieldUpdated

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.vehicleID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.VehicleID != null)
            {
                SetRouteFuelType(fsRouteDocumentRow, fsRouteDocumentRow.VehicleID);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.additionalVehicleID1> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.AdditionalVehicleID1 != null)
            {
                SetRouteFuelType(fsRouteDocumentRow, fsRouteDocumentRow.AdditionalVehicleID1);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.additionalVehicleID2> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.AdditionalVehicleID2 != null)
            {
                SetRouteFuelType(fsRouteDocumentRow, fsRouteDocumentRow.AdditionalVehicleID2);
            }
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.routeID> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.Date != null)
            {
                SetRouteStartTimeByRouteID(fsRouteDocumentRow);
            }

            e.Cache.SetDefaultExt<FSRouteDocument.routeCD>(fsRouteDocumentRow);
        }

        protected virtual void _(Events.FieldUpdated<FSRouteDocument, FSRouteDocument.date> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (fsRouteDocumentRow.TimeBegin != null)
            {
                fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, fsRouteDocumentRow.TimeBegin);
            }
            else
            {
                fsRouteDocumentRow.TimeBegin = PXDBDateAndTimeAttribute.CombineDateTime(fsRouteDocumentRow.Date, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0));
            }

            if (fsRouteDocumentRow.RouteID != null)
            {
                SetRouteStartTimeByRouteID(fsRouteDocumentRow);
            }
        }
        #endregion

        protected virtual void _(Events.RowSelecting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowSelected<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            PXCache cache = e.Cache;

            EnableDisableDocument(cache, fsRouteDocumentRow);
            EnableDisable_ActionButtons(cache, fsRouteDocumentRow);
            SharedFunctions.CheckRouteActualDateTimes(cache, fsRouteDocumentRow, Accessinfo.BusinessDate);
        }

        protected virtual void _(Events.RowInserting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowInserted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdating<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowUpdated<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleting<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowDeleted<FSRouteDocument> e)
        {
        }

        protected virtual void _(Events.RowPersisting<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;
            PXCache cache = e.Cache;

            LicenseHelper.CheckVehiclesLicense(cache.Graph, null, null);

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                ValidateDrivers(cache, fsRouteDocumentRow);
                ValidateVehicles(cache, fsRouteDocumentRow);
            }

            switch (e.Operation)
            {
                case PXDBOperation.Insert:
                    FSRoute fsRouteRow = PXSelect<FSRoute, Where<FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>.Select(this, fsRouteDocumentRow.RouteID);

                    if (fsRouteRow == null)
                    {
                        return;
                    }

                    DateTime? dummyDate = new DateTime?();

                    if (!SharedFunctions.EvaluateExecutionDay(fsRouteRow, fsRouteDocumentRow.Date.Value.DayOfWeek, ref dummyDate))
                    {
                        cache.RaiseExceptionHandling<FSRouteDocument.date>(
                            fsRouteDocumentRow,
                            fsRouteDocumentRow.Date,
                            new PXException(TX.Error.ROUTE_DOCUMENT_DATE_NOT_MATCH_WITH_EXECUTION_DAYS, fsRouteDocumentRow.Date.Value.DayOfWeek, fsRouteRow.RouteCD));
                        return;
                    }

                    if (fsRouteRow.WeekCode != null
                            && !SharedFunctions.WeekCodeIsValid(fsRouteRow.WeekCode, fsRouteDocumentRow.Date, this))
                    {
                        cache.RaiseExceptionHandling<FSRouteDocument.date>(
                            fsRouteDocumentRow,
                            fsRouteDocumentRow.Date,
                            new PXException(TX.Error.ROUTE_DOCUMENT_DATE_NOT_MATCH_WITH_WEEKCODE, fsRouteRow.WeekCode, fsRouteRow.RouteCD));
                        return;
                    }

                    if (InvalidDayTrip(fsRouteRow) != null)
                    {
                        cache.RaiseExceptionHandling<FSRouteDocument.tripNbr>(
                            fsRouteDocumentRow,
                            fsRouteDocumentRow.TripNbr,
                            new PXException(TX.Error.MAX_NBR_TRIPS_PER_DAY, fsRouteDocumentRow.Date.Value.DayOfWeek));
                        return;
                    }

                    if (TripIDAlreadyExist(RouteRecords.Current, RouteRecords.Current.TripNbr))
                    {
                        cache.RaiseExceptionHandling<FSRouteDocument.tripNbr>(
                            fsRouteDocumentRow,
                            fsRouteDocumentRow.TripNbr,
                            new PXException(TX.Error.ID_OF_TRIPS_ALREADY_EXIST));
                        return;
                    }

                    break;
                case PXDBOperation.Update:

                    oldDriverID = (int?)cache.GetValueOriginal<FSRouteDocument.driverID>(fsRouteDocumentRow);
                    oldAdditionalDriverID = (int?)cache.GetValueOriginal<FSRouteDocument.additionalDriverID>(fsRouteDocumentRow);

                    int? oldVehicleID = (int?)cache.GetValueOriginal<FSRouteDocument.vehicleID>(fsRouteDocumentRow);
                    int? newVehicleID = fsRouteDocumentRow.VehicleID;

                    if (oldVehicleID != newVehicleID)
                    {
                        vehicleChanged = true;
                    }

                    DateTime? oldDate = (DateTime?)cache.GetValueOriginal<FSRouteDocument.date>(fsRouteDocumentRow);
                    DateTime? newDate = fsRouteDocumentRow.Date;

                    if (oldDate != newDate)
                    {
                        needAppointmentDateUpdate = true;
                    }

                    DateTime? oldTimeBegin = (DateTime?)cache.GetValueOriginal<FSRouteDocument.timeBegin>(fsRouteDocumentRow);
                    DateTime? newTimeBegin = fsRouteDocumentRow.TimeBegin;
                    TimeSpan span;

                    if (oldTimeBegin != newTimeBegin)
                    {
                        needAppointmentTimeBeginUpdate = true;
                        span = (TimeSpan)(newTimeBegin - oldTimeBegin);
                        minutesToAdd = (int)span.TotalMinutes;
                    }

                    bool mustCalculateStats = RouteSetupRecord.Current != null
                                                    && RouteSetupRecord.Current.AutoCalculateRouteStats == true
                                                        && fsRouteDocumentRow?.MustRecalculateStats == true;
                    if (mustCalculateStats == true)
                    {
                        CalculateStats();
                        fsRouteDocumentRow.MustRecalculateStats = false;
                    }

                    break;
                default:
                    break;
            }
        }

        protected virtual void _(Events.RowPersisted<FSRouteDocument> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSRouteDocument fsRouteDocumentRow = (FSRouteDocument)e.Row;

            if (e.Operation == PXDBOperation.Update
                    && e.TranStatus == PXTranStatus.Open)
            {
                UpdateAppointmentInRoute(fsRouteDocumentRow, needAppointmentTimeBeginUpdate || needAppointmentDateUpdate, vehicleChanged, fsRouteDocumentRow.DriverID != oldDriverID, fsRouteDocumentRow.AdditionalDriverID != oldAdditionalDriverID);

                needAppointmentTimeBeginUpdate = false;
                needAppointmentDateUpdate = false;
                vehicleChanged = false;
                oldAdditionalDriverID = null;
                oldDriverID = null;
            }
        }
        #endregion

        protected virtual void _(Events.RowSelected<FSAppointment> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            CheckAppointmentsAddress(e.Cache, fsAppointmentRow);
        }
        #endregion
    }
}