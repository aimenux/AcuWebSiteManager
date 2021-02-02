using Autofac;
using PX.Common;
using PX.Data;
using PX.Data.DependencyInjection;
using PX.Data.Reports;
using PX.LicensePolicy;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CM.Extensions;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.CT;
using PX.Objects.Extensions.MultiCurrency;
using PX.Objects.Extensions.SalesTax;
using PX.Objects.FS.ParallelProcessing;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.TX;
using PX.Reports.Controls;
using PX.Reports.Data;
using PX.SM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Compilation;

namespace PX.Objects.FS
{
    public class AppointmentEntry : AppointmentEntryBase<AppointmentEntry>, IGraphWithInitialization
    {
        public class ServiceRequirement
        {
            public int serviceID;
            public List<int?> requirementIDList = new List<int?>();
        }

        public bool IsCloningAppointment = false;
        public bool NeedRecalculateRouteStats;
        public bool CalculateGoogleStats = true;
        public bool AvoidCalculateRouteStats = false;
        public bool AutomaticallyUncloseAppointment = false;
        public bool SkipServiceOrderUpdate = false;
        public bool SkipTimeCardUpdate = false;
        public bool UpdateSOStatusOnAppointmentUpdating = false;
        public string UpdateSOStatusOnAppointmentDeleting = string.Empty;
        public bool IsGeneratingAppointment = false;
        public bool SkipChangingContract = false;
        public bool SkipManualTimeFlagUpdate = false;
        public bool SkipCallSOAction = false;
        public bool DisableServiceOrderUnboundFieldCalc = false;
        public string UncloseDialogMessage = TX.Messages.ASK_CONFIRM_APPOINTMENT_UNCLOSING;
        protected bool RetakeGeoLocation = false;

        //// Store detail line status before saving
        private Dictionary<int?, string> originalPartStatus;
        private Dictionary<int?, string> originalServiceStatus;
        private PXGraph dummyGraph;

        #region Private Members
        private FSSelectorHelper fsSelectorHelper;

        private FSSelectorHelper GetFsSelectorHelperInstance
        {
            get 
            {
                if (this.fsSelectorHelper == null)
                {
                    this.fsSelectorHelper = new FSSelectorHelper();
                }

                return this.fsSelectorHelper;
            }
        }

        private ServiceOrderEntry _localServiceOrderEntryGraph;

        private void RefreshServiceOrderRelated()
        {
            ServiceOrderRelated.Cache.Clear();
            ServiceOrderRelated.View.Clear();
            ServiceOrderRelated.View.RequestRefresh();
        }

        public void clearLocalServiceOrder()
        {
            _localServiceOrderEntryGraph?.Clear(PXClearOption.ClearAll);
        }

        protected ServiceOrderEntry GetLocalServiceOrderEntryGraph(bool clearGraph)
        {
            if (_localServiceOrderEntryGraph == null)
            {
                _localServiceOrderEntryGraph = PXGraph.CreateInstance<ServiceOrderEntry>();
            }
            else if (clearGraph == true)
            {
                _localServiceOrderEntryGraph.Clear();
            }

            _localServiceOrderEntryGraph.RecalculateExternalTaxesSync = true;
            _localServiceOrderEntryGraph.GraphAppointmentEntryCaller = this;

            return _localServiceOrderEntryGraph;
        }

        protected Dictionary<object, object> _oldRows = null;
        protected bool updateContractPeriod = false;

        protected Dictionary<object, object> _appointmentDetSODetRelation = null;
        #endregion

        public static bool IsReadyToBeUsed(PXGraph graph, string callerScreenID)
        {
            bool isSetupCompleted = PXSelect<FSSetup, Where<FSSetup.calendarID, IsNotNull>>.Select(graph).Count > 0;
            bool currentUserCanEnterTimeCards = TimeCardHelper.CanCurrentUserEnterTimeCards(graph, callerScreenID);

            return isSetupCompleted && currentUserCanEnterTimeCards;
        }

        public void SkipTaxCalcAndSave()
        {
            var AppointmentEntryExternalTax = this.GetExtension<AppointmentEntryExternalTax>();

            if (AppointmentEntryExternalTax != null)
            {
                this.GetExtension<AppointmentEntryExternalTax>().PublicSkipTaxCalcAndSave();
            }
            else
            {
                Save.Press();
            }
        }
        public void ChangeStatusSave(FSAppointment fsAppointmentRow)
        {
            var AppointmentEntryExternalTax = this.GetExtension<AppointmentEntryExternalTax>();

            if (AppointmentEntryExternalTax != null)
            {
                bool previousValue = AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave;
                try
                {
                    AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave = true;
                    SharedFunctions.SaveUpdatedChanges(AppointmentRecords.Cache, fsAppointmentRow);
                }
                finally
                {
                    AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave = previousValue;
                }
            }
            else
            {
                SharedFunctions.SaveUpdatedChanges(AppointmentRecords.Cache, fsAppointmentRow);
            }
        }

        public AppointmentEntry()
            : base()
        {
            // Adding the start/complete/cancel buttons as part of the Action menu button
            FSSetup fsSetupRow = SetupRecord.Current;

            NeedRecalculateRouteStats = false;

            menuActions.AddMenuAction(startAppointment);
            menuActions.AddMenuAction(completeAppointment);
            menuActions.AddMenuAction(cancelAppointment);
            menuActions.AddMenuAction(reopenAppointment);
            menuActions.AddMenuAction(closeAppointment);
            menuActions.AddMenuAction(uncloseAppointment);
            menuActions.AddMenuAction(cloneAppointment);
            menuActions.AddMenuAction(invoiceAppointment);
            menuActions.AddMenuAction(openEmployeeBoard);
            menuActions.AddMenuAction(openUserCalendar);

            if (fsSetupRow != null && fsSetupRow.ManageRooms == true)
            {
                menuActions.AddMenuAction(openRoomBoard);
            }

            menuActions.AddMenuAction(emailSignedAppointment);
            menuActions.AddMenuAction(emailConfirmationToCustomer);
            menuActions.AddMenuAction(emailConfirmationToStaffMember);
            menuActions.AddMenuAction(emailConfirmationToGeoZoneStaff);
            reportsMenu.AddMenuAction(printAppointmentReport);
            reportsMenu.AddMenuAction(printServiceTimeActivityReport);

            FieldUpdated.AddHandler(
                                    AppointmentRecords.Name,
                                    typeof(FSAppointment.scheduledDateTimeBegin).Name + PXDBDateAndTimeAttribute.DATE_FIELD_POSTFIX,
                                    FSAppointment_ScheduledDateTimeBegin_FieldUpdated);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointment),
                                    typeof(FSAppointment.scheduledDateTimeEnd).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointment_ScheduledDateTimeEnd_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointment),
                                    typeof(FSAppointment.actualDateTimeBegin).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointment_ActualDateTimeBegin_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointment),
                                    typeof(FSAppointment.actualDateTimeEnd).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointment_ActualDateTimeEnd_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointmentDetService),
                                    typeof(FSAppointmentDetService.actualDateTimeBegin).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointmentDetService_ActualDateTimeBegin_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointmentDetService),
                                    typeof(FSAppointmentDetService.actualDateTimeEnd).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointmentDetService_ActualDateTimeEnd_Time_FieldUpdating);

            FieldUpdating.AddHandler(
                                    typeof(FSAppointmentEmployee),
                                    typeof(FSAppointmentEmployee.actualDateTimeBegin).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX,
                                    FSAppointmentEmployee_ActualDateTimeBegin_Time_FieldUpdating);

            if (TimeCardHelper.CanCurrentUserEnterTimeCards(this, this.Accessinfo.ScreenID) == false)
            {
                if (IsExport || IsImport || System.Web.HttpContext.Current.Request.Form["__CALLBACKID"] != null)
                {
                    throw new PXException(PX.Objects.EP.Messages.MustBeEmployee);
                }
                else
                {
                    Redirector.Redirect(
                                        System.Web.HttpContext.Current,
                                        string.Format(
                                            "~/Frames/Error.aspx?exceptionID={0}&typeID={1}",
                                            PX.Objects.EP.Messages.MustBeEmployee,
                                            "error"));
                }
            }

            if (originalPartStatus == null)
            {
                originalPartStatus = new Dictionary<int?, string>();
            }

            if (originalServiceStatus == null)
            {
                originalServiceStatus = new Dictionary<int?, string>();
            }

            if (dummyGraph == null)
            {
                dummyGraph = new PXGraph();
            }
            
            PXUIFieldAttribute.SetDisplayName<FSAppointment.mapLatitude>(AppointmentRecords.Cache, TX.RouteLocationInfo.APPOINTMENT_LOCATION);
            PXUIFieldAttribute.SetDisplayName<FSAppointment.gPSLatitudeStart>(AppointmentRecords.Cache, TX.RouteLocationInfo.START_LOCATION);
            PXUIFieldAttribute.SetDisplayName<FSAppointment.gPSLatitudeComplete>(AppointmentRecords.Cache, TX.RouteLocationInfo.END_LOCATION);
            PXUIFieldAttribute.SetDisplayName<FSxService.actionType>(InventoryItemHelper.Cache, "Pickup/Deliver Items");
        }

        #region CacheAttached

        #region BAccount CacheAttached
        #region BAccount_AcctName
        [PXDBString(60, IsUnicode = true)]
        [PXUIField(DisplayName = "Staff Member Name", Enabled = false)]
        protected virtual void BAccount_AcctName_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region ServiceOrder CacheAttached
        #region FSServiceOrder_CustomerID
        [PXDBInt]
        [PopupMessage]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Customer", Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<Customer.status, IsNull,
                Or<Customer.status, Equal<BAccount.status.active>,
                Or<Customer.status, Equal<BAccount.status.oneTime>>>>),
                PX.Objects.AR.Messages.CustomerIsInStatus, typeof(Customer.status))]
        [FSSelectorBAccountTypeCustomerOrCombined]
        protected virtual void FSServiceOrder_CustomerID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region SrvOrdType
        [Obsolete("Remove in 2019R2")]
        [PXDefault]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSServiceOrder_SrvOrdType_CacheAttached(PXCache sender)
        {

        }
        #endregion

        #region RefNbr
        [Obsolete("Remove in 2019R2")]
        [PXDBString(15, IsKey = false, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        public virtual string RefNbr { get; set; }

        protected virtual void FSServiceOrder_RefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region SOID
        [Obsolete("Remove in 2019R2")]
        [PXDBIdentity(IsKey = true)]
        public virtual int? SOID { get; set; }

        protected virtual void FSServiceOrder_SOID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region EnablePurchaseOrder
        [Obsolete("Remove in 2019R2")]
        public virtual bool? enablePO { get; set; }
 
        [PXDBBool]
        [PXUIField(DisplayName = "Mark for PO", Visible = false, FieldClass = "DISTINV")]
        protected virtual void FSSODet_EnablePO_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region PONbr
        [Obsolete("Remove in 2019R2")]
        public virtual string poNbr { get; set; }

        [PXDBString]
        [PXUIField(DisplayName = "PO Nbr.", Enabled = false, Visible = false, FieldClass = "DISTINV")]
        [PO.PO.RefNbr(typeof(
            Search2<POOrder.orderNbr,
            LeftJoinSingleTable<Vendor,
                On<POOrder.vendorID, Equal<Vendor.bAccountID>,
                And<Match<Vendor, Current<AccessInfo.userName>>>>>,
            Where<
                POOrder.orderType, Equal<POOrderType.regularOrder>,
                And<Vendor.bAccountID, IsNotNull>>,
            OrderBy<Desc<POOrder.orderNbr>>>), Filterable = true)]
        protected virtual void FSSODet_PONbr_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region POStatus
        [Obsolete("Remove in 2019R2")]
        public virtual string poStatus { get; set; }

        [PXDBString]
        [PXUIField(DisplayName = "PO Status", Enabled = false, Visible = false, FieldClass = "DISTINV")]
        protected virtual void FSSODet_POStatus_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region CustWorkOrderRefNbr
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "External Reference", Visible = false)]

        protected virtual void FSServiceOrder_CustWorkOrderRefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region CustPORefNbr
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Customer Order", Visible = false)]

        protected virtual void FSServiceOrder_CustPORefNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Priority
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Priority", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        protected virtual void FSServiceOrder_Priority_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region Severity
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Severity", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]

        protected virtual void FSServiceOrder_Severity_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region ProblemID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Problem", Visible = false)]
    
        protected virtual void FSServiceOrder_ProblemID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #region AssignedEmpID
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXUIField(DisplayName = "Supervisor", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        protected virtual void FSServiceOrder_AssignedEmpID_CacheAttached(PXCache sender)
        {
        }
        #endregion


        #endregion

        #region FSBillingCycle_BillingCycleCD
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXUIField(DisplayName = "Billing Cycle", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSBillingCycle.billingCycleCD))]
        [NormalizeWhiteSpace]
        protected virtual void FSBillingCycle_BillingCycleCD_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region ARPayment_CashAccountID
        [PXDBInt]
        [PXUIField(DisplayName = "Cash Account", Visibility = PXUIVisibility.Visible, Visible = false)]
        protected virtual void ARPayment_CashAccountID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region FSAppointmentDetService CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSAppointmentDetService_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region FSAppointmentDetPart CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSAppointmentDetPart_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion

        #region FSAppointmentInventoryItem CacheAttached
        [PopupMessage]
        [PXMergeAttributes(Method = MergeMethod.Merge)]
        protected virtual void FSAppointmentInventoryItem_InventoryID_CacheAttached(PXCache sender)
        {
        }
        #endregion
        #endregion

        #region Actions

        #region ViewDirectionOnMap
        public PXAction<FSAppointment> viewDirectionOnMap;
        [PXUIField(DisplayName = CR.Messages.ViewOnMap, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void ViewDirectionOnMap()
        {
            var address = ServiceOrder_Address.SelectSingle();
            if (address != null)
            {
                CR.BAccountUtility.ViewOnMap<FSAddress, FSAddress.countryID>(address);
            }
        }
        #endregion
        #region ViewStartGPSOnMap
        public PXAction<FSAppointment> viewStartGPSOnMap;
        [PXUIField(DisplayName = "View on Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewStartGPSOnMap(PXAdapter adapter)
        {
            if (AppointmentSelected.Current != null && AppointmentSelected.Current.SOID != null)
            {
                var googleMap = new PX.Data.GoogleMapLatLongRedirector();

                googleMap.ShowAddressByLocation(AppointmentSelected.Current.GPSLatitudeStart, AppointmentSelected.Current.GPSLongitudeStart);
            }

            return adapter.Get();
        }
        #endregion
        #region ViewCompleteGPSOnMap
        public PXAction<FSAppointment> viewCompleteGPSOnMap;
        [PXUIField(DisplayName = "View on Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ViewCompleteGPSOnMap(PXAdapter adapter)
        {
            if (AppointmentSelected.Current != null && AppointmentSelected.Current.SOID != null)
            {
                var googleMap = new PX.Data.GoogleMapLatLongRedirector();

                googleMap.ShowAddressByLocation(AppointmentSelected.Current.GPSLatitudeComplete, AppointmentSelected.Current.GPSLongitudeComplete);
            }

            return adapter.Get();
        }
        #endregion

        #region CloneAppointment
        public PXAction<FSAppointment> cloneAppointment;
        [PXUIField(DisplayName = "Clone Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        public virtual IEnumerable CloneAppointment(PXAdapter adapter)
        {
            if (!string.IsNullOrEmpty(AppointmentRecords.Current.RefNbr))
            {
                if (ServiceOrderRelated.Current != null
                        && ServiceOrderRelated.Current.Status == ID.Status_ServiceOrder.COMPLETED)
                {
                    ServiceOrderEntry graphServiceOrderEntry = GetLocalServiceOrderEntryGraph(true);

                    graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                                .Search<FSServiceOrder.refNbr>(ServiceOrderRelated.Current.RefNbr, ServiceOrderRelated.Current.SrvOrdType);
                    graphServiceOrderEntry.ReopenOrder();
                }

                CloneAppointmentProcess graphCloneAppointmentMaint = PXGraph.CreateInstance<CloneAppointmentProcess>();

                graphCloneAppointmentMaint.Filter.Current.SrvOrdType = AppointmentRecords.Current.SrvOrdType;
                graphCloneAppointmentMaint.Filter.Current.RefNbr = AppointmentRecords.Current.RefNbr;
                graphCloneAppointmentMaint.Filter.Current.ScheduledStartTime = AppointmentRecords.Current.ScheduledDateTimeBegin;
                graphCloneAppointmentMaint.Filter.Current.ScheduledEndTime = AppointmentRecords.Current.ScheduledDateTimeEnd;
                graphCloneAppointmentMaint.Filter.Current.AppointmentID = AppointmentRecords.Current.AppointmentID;
                graphCloneAppointmentMaint.Filter.Current.OriginalDuration = AppointmentRecords.Current.Mem_Duration;

                throw new PXRedirectRequiredException(graphCloneAppointmentMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }

            return adapter.Get();
        }
        #endregion

        #region MenuActions
        public PXMenuAction<FSAppointment> menuActions;
        [PXButton(MenuAutoOpen = true, SpecialType = PXSpecialButtonType.ActionsFolder)]
        [PXUIField(DisplayName = "Actions")]
        public virtual IEnumerable MenuActions(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public PXMenuAction<FSAppointment> reportsMenu;
        [PXButton(SpecialType = PXSpecialButtonType.ReportsFolder, MenuAutoOpen = true)]
        [PXUIField(DisplayName = "Reports")]
        public virtual IEnumerable ReportsMenu(PXAdapter adapter)
        {
            return adapter.Get();
        }

        #endregion
        #region Validate Address
        public PXAction<FSAppointment> validateAddress;
        [PXUIField(DisplayName = "Validate Address", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, FieldClass = CS.Messages.ValidateAddress)]
        [PXButton]
        public virtual IEnumerable ValidateAddress(PXAdapter adapter)
        {
            foreach (FSAppointment current in adapter.Get<FSAppointment>())
            {
                if (current != null)
                {
                    FSAddress address = this.ServiceOrder_Address.Select();
                    if (address != null && address.IsDefaultAddress == false && address.IsValidated == false)
                    {
                        PXAddressValidator.Validate<FSAddress>(this, address, true);
                    }
                }
                yield return current;
            }
        }
        #endregion

        #region StartAppointment
        public PXAction<FSAppointment> startAppointment;
        [PXUIField(DisplayName = "Start Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable StartAppointment(PXAdapter adapter)
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                this.SkipTaxCalcAndSave();

                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                string errorMessage = string.Empty;

                if (AppointmentCore.CheckNewAppointmentStatus(
                                                            this,
                                                            fsAppointmentRow,
                                                            ServiceOrderRelated.Current,
                                                            ID.Status_Appointment.IN_PROCESS,
                                                            ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                var fsAppointmentDetRows = AppointmentDetServices.SelectWindowed(0, 1);
                if (fsAppointmentDetRows.Count == 0)
                {
                    throw new PXException(TX.Error.APPOINTMENT_START_VALIDATE_SERVICE, PXErrorLevel.Error);
                }

                using (var ts = new PXTransactionScope())
                {
                    fsAppointmentRow.HandleManuallyActualTime = false;
                    AppointmentCore.UpdateAppointmentDetActualFields(AppointmentDetServices, AppointmentDetParts, AppointmentEmployees);

                    try
                    {
                        SkipManualTimeFlagUpdate = true;

                        AppointmentDateTimeHelper.UpdateActualDateTimeBegin(
                                                    ServiceOrderTypeSelected.Current,
                                                    fsAppointmentRow,
                                                    AppointmentDetServices,
                                                    AppointmentEmployees,
                                                    AppointmentDateTimeHelper.Action.OnStartAppointment,
                                                    AppointmentDateTimeHelper.Level.Both,
                                                    cacheAppointmentHeader: AppointmentSelected.Cache,
                                                    cacheAppointmentService: null);
                    }
                    finally
                    {
                        SkipManualTimeFlagUpdate = false;
                    }

                    fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.IN_PROCESS);

                    ///This update must be done after the status has been changed to update properly the 
                    ///Services' actual duration for those services linked to a staff member.
                    AppointmentDateTimeHelper.UpdateStaffActualDuration(this, AppointmentEmployees);

                    if (IsMobile == true
                            && SetupRecord.Current != null
                                && SetupRecord.Current.TrackAppointmentLocation == true
                                    && string.IsNullOrEmpty(fsAppointmentRow.Mem_GPSLatitudeLongitude) == false)
                    {
                        string[] parts = fsAppointmentRow.Mem_GPSLatitudeLongitude.Split(':');
                        fsAppointmentRow.GPSLatitudeStart = decimal.Parse(parts[0]);
                        fsAppointmentRow.GPSLongitudeStart = decimal.Parse(parts[1]);
                    }

                    
                    this.ChangeStatusSaveAndSkipExternalTaxCalc(fsAppointmentRow);

                    ts.Complete();
                    RefreshServiceOrderRelated();
                }

                LoadServiceOrderRelatedAfterStatusChange(fsAppointmentRow);
                ForceExternalTaxCalc();
            }

            return adapter.Get();
        }
        #endregion
        #region CancelAppointment
        public PXAction<FSAppointment> cancelAppointment;
        [PXUIField(DisplayName = "Cancel Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CancelAppointment()
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                this.SkipTaxCalcAndSave();

                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                string errorMessage = string.Empty;

                if (AppointmentCore.CheckNewAppointmentStatus(
                                                            this,
                                                            fsAppointmentRow,
                                                            ServiceOrderRelated.Current,
                                                            ID.Status_Appointment.CANCELED,
                                                            ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                string soFinalStatus = getFinalServiceOrderStatus(ServiceOrderRelated.Current, fsAppointmentRow, ID.Status_Appointment.CANCELED);

                using (var ts = new PXTransactionScope())
                {
                    updateContractPeriod = true;
                    fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.CANCELED);
                    AppointmentCore.UpdateAppointmentDetLinesStatus<FSAppointmentDetService>(AppointmentDetServices, ID.Status_AppointmentDet.CANCELED);
                    AppointmentCore.UpdateAppointmentDetLinesStatus<FSAppointmentDetPart>(AppointmentDetParts, ID.Status_AppointmentDet.CANCELED);

                    this.ChangeStatusSaveAndSkipExternalTaxCalc(fsAppointmentRow);

                    if (string.IsNullOrEmpty(soFinalStatus) == true)
                    { 
                        SetServiceOrderStatusFromAppointment(ServiceOrderRelated.Current, fsAppointmentRow);
                    }
                    else
                    { 
                        setLatestServiceOrderStatusBaseOnAppointmentStatus(ServiceOrderRelated.Current, soFinalStatus);
                    }

                    ts.Complete();
                    RefreshServiceOrderRelated();
                }

                LoadServiceOrderRelatedAfterStatusChange(fsAppointmentRow);
                ForceExternalTaxCalc();
            }
        }

        #endregion
        #region ReopenAppointment
        public PXAction<FSAppointment> reopenAppointment;
        [PXUIField(DisplayName = "Reopen Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable ReopenAppointment(PXAdapter adapter)
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                if (AppointmentRecords.Cache.AllowUpdate == true)
                {
                    this.SkipTaxCalcAndSave();
                }

                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                string errorMessage = string.Empty;

                if (AppointmentCore.CheckNewAppointmentStatus(
                                                            this,
                                                            fsAppointmentRow,
                                                            ServiceOrderRelated.Current,
                                                            ID.Status_Appointment.MANUAL_SCHEDULED,
                                                            ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (fsAppointmentRow.Status == ID.Status_Appointment.CANCELED)
                {
                    AppointmentRecords.Cache.AllowUpdate = true;
                }

                using (var ts = new PXTransactionScope())
                {
                    fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.MANUAL_SCHEDULED);
                    AppointmentRecords.Current = fsAppointmentRow;
                    AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsAppointmentRow);

                    AppointmentCore.UpdateAppointmentDetActualFields(AppointmentDetServices, AppointmentDetParts, AppointmentEmployees, true);
                    AppointmentCore.UpdateAppointmentDetLinesStatus<FSAppointmentDetService>(AppointmentDetServices, ID.Status_AppointmentDet.OPEN);
                    AppointmentCore.UpdateAppointmentDetLinesStatus<FSAppointmentDetPart>(AppointmentDetParts, ID.Status_AppointmentDet.OPEN);

                    fsAppointmentRow.LineTotal = fsAppointmentRow.EstimatedLineTotal;

                    UpdateAppointmentDetStatus<FSAppointmentDetService>(AppointmentDetServices);
                    UpdateAppointmentDetStatus<FSAppointmentDetPart>(AppointmentDetParts);

                    SetServiceOrderStatusFromAppointment(ServiceOrderRelated.Current, fsAppointmentRow);

                    fsAppointmentRow.HandleManuallyActualTime = false;
                    ChangeStatusSaveAndSkipExternalTaxCalc(fsAppointmentRow);

                    ts.Complete();
                    RefreshServiceOrderRelated();
                }

                LoadServiceOrderRelatedAfterStatusChange(fsAppointmentRow);
                ForceExternalTaxCalc();
            }

            return adapter.Get();
        }

        protected virtual void UpdateAppointmentDetStatus<AppointmentDetType>(PXSelectBase<AppointmentDetType> viewAppointmentDet)
            where AppointmentDetType : FSAppointmentDet, new()
        {
            foreach (AppointmentDetType fsAppointmentDetRow in viewAppointmentDet.Select())
            {
                if (fsAppointmentDetRow.SODetID != null)
                {
                    FSSODet fsSODetRow = PXSelectReadonly<FSSODet,
                                                            Where<
                                                                FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                        .SelectSingleBound(viewAppointmentDet.Cache.Graph, null, fsAppointmentDetRow.SODetID);

                    if (fsSODetRow == null)
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.RECORD_NOT_FOUND, TX.CustomTextFields.SERVICE_ORDER_DETAIL), PXErrorLevel.Error);
                    }

                    if (fsAppointmentDetRow.Status != fsSODetRow.Status)
                    {
                        fsAppointmentDetRow.Status = fsSODetRow.Status;
                        viewAppointmentDet.Update(fsAppointmentDetRow);
                    }
                }
            }
        }

        #endregion
        #region CompleteAppointment
        public PXAction<FSAppointment> completeAppointment;
        [PXUIField(DisplayName = "Complete Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CompleteAppointment(PXAdapter adapter)
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                this.SkipTaxCalcAndSave();

                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                string errorMessage = string.Empty;

                AppointmentDateTimeHelper.UpdateActualDateTimeEnd(
                                                                  ServiceOrderTypeSelected.Current, 
                                                                  fsAppointmentRow, 
                                                                  AppointmentDetServices,
                                                                  AppointmentEmployees,
                                                                  AppointmentDateTimeHelper.Action.OnCompleteAppointment,
                                                                  AppointmentDateTimeHelper.Level.Both);

                if (AppointmentCore.CheckNewAppointmentStatus(
                                                            this,
                                                            fsAppointmentRow,
                                                            ServiceOrderRelated.Current,
                                                            ID.Status_Appointment.COMPLETED,
                                                            ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (AppointmentDateTimeHelper.CheckServiceActualDateTimeBegin(fsAppointmentRow, AppointmentDetServices, AppointmentEmployees, ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (AppointmentDateTimeHelper.CheckServiceActualDateTimes(ServiceOrderTypeSelected.Current, AppointmentDetServices, AppointmentEmployees, ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }
                
                var fsAppointmentDetSet = AppointmentDetServices.SelectWindowed(0, 1);
                if (fsAppointmentDetSet.Count == 0)
                {
                    throw new PXException(TX.Error.APPOINTMENT_COMPLETE_VALIDATE_SERVICE, PXErrorLevel.Error);
                }

                if (ActualDateAndTimeValidation(fsAppointmentRow) == false)
                {
                    throw new PXException(TX.Error.ACTUAL_DATES_APPOINTMENT_MISSING);
                }

                if (IsMobile)
                {
                    ValidateSignatureFields(
                                            this.AppointmentSelected.Cache,
                                            fsAppointmentRow,
                                            SharedFunctions.GetRequireCustomerSignature(this, ServiceOrderTypeSelected.Current, ServiceOrderRelated.Current));
                }

                using (var ts = new PXTransactionScope())
                {
                    if (ServiceOrderRelated.Current.SourceType == ID.SourceType_ServiceOrder.SALES_ORDER)
                    {
                        SOOrder sOOrderRow = PXSelect<SOOrder,
                                    Where<
                                        SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                        And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                    .Select(this, ServiceOrderRelated.Current.SourceDocType, ServiceOrderRelated.Current.SourceRefNbr);

                        if (sOOrderRow == null)
                        {
                            throw new PXException(TX.Error.SERVICE_ORDER_SOORDER_INCONSISTENCY);
                        }

                        //Installed flag lift for Sales Order
                        PXUpdate<Set<FSxSOOrder.installed, True>, SOOrder, Where<
                                        SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
                                        And<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>>>>
                                        .Update(this, ServiceOrderRelated.Current.SourceDocType, ServiceOrderRelated.Current.SourceRefNbr);

                        PXResultset<SOOrderShipment> bqlResultSet = PXSelect<SOOrderShipment,
                                        Where<SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>,
                                        And<SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>>>>
                                        .Select(this, sOOrderRow.OrderType, sOOrderRow.OrderNbr);

                        foreach (SOOrderShipment soOrderShipmentRow in bqlResultSet)
                        {
                            //Installed flag lift for the Shipment
                            PXUpdate<Set<FSxSOShipment.installed, True>, SOShipment, 
                            Where
                                <SOShipment.shipmentNbr, Equal<Required<SOShipment.shipmentNbr>>>>
                            .Update(this, soOrderShipmentRow.ShipmentNbr);
                        }
                    }

                    AppointmentDateTimeHelper.UpdateStaffActualDateTimeOnCompleteApp(AppointmentEmployees, fsAppointmentRow);
					fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.COMPLETED);

                    if (IsMobile == true
                            && SetupRecord.Current != null
                                && SetupRecord.Current.TrackAppointmentLocation == true
                                    && string.IsNullOrEmpty(fsAppointmentRow.Mem_GPSLatitudeLongitude) == false)
                    {
                        string[] parts = fsAppointmentRow.Mem_GPSLatitudeLongitude.Split(':');
                        fsAppointmentRow.GPSLatitudeComplete = decimal.Parse(parts[0]);
                        fsAppointmentRow.GPSLongitudeComplete = decimal.Parse(parts[1]);
                    }

                    this.CalculateCosts();

                    ChangeStatusSaveAndSkipExternalTaxCalc(fsAppointmentRow);

                    ts.Complete();
                    RefreshServiceOrderRelated();
                }

                LoadServiceOrderRelatedAfterStatusChange(fsAppointmentRow);
                ForceExternalTaxCalc();
            }

            return adapter.Get();
        }
        #endregion
        #region CloseAppointment
        public PXAction<FSAppointment> closeAppointment;
        [PXUIField(DisplayName = "Close Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CloseAppointment()
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                this.SkipTaxCalcAndSave();

                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                string errorMessage = string.Empty;

                if (AppointmentCore.CheckNewAppointmentStatus(
                                                            this,
                                                            fsAppointmentRow,
                                                            ServiceOrderRelated.Current,
                                                            ID.Status_Appointment.CLOSED,
                                                            ref errorMessage) == false)
                {
                    throw new PXException(errorMessage);
                }

                if (fsAppointmentRow.TimeRegistered == false && ServiceOrderTypeSelected.Current.RequireTimeApprovalToInvoice == true)
                {
                    throw new PXException(TX.Error.CANNOT_CLOSED_APPOINTMENT_BECAUSE_TIME_REGISTERED_OR_ACTUAL_TIMES);
                }

                using (var ts = new PXTransactionScope())
                {
                    updateContractPeriod = true;
                    fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.CLOSED);
                    ChangeStatusSaveAndSkipExternalTaxCalc(fsAppointmentRow);

                    ts.Complete();
                    RefreshServiceOrderRelated();
                }

                LoadServiceOrderRelatedAfterStatusChange(fsAppointmentRow);
                ForceExternalTaxCalc();
            }
        }
        #endregion
        #region Unclose Appointment
        public PXAction<FSAppointment> uncloseAppointment;
        [PXUIField(DisplayName = "Unclose Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void UncloseAppointment()
        {
            FSAppointment fsAppointmentRow = AppointmentRecords.Current;

            if (fsAppointmentRow != null
                    && (AutomaticallyUncloseAppointment || WebDialogResult.Yes == AppointmentRecords.Ask(
                                                                TX.WebDialogTitles.CONFIRM_APPOINTMENT_UNCLOSING,
                                                                UncloseDialogMessage,
                                                                MessageButtons.YesNo)))
            {
                using (var ts = new PXTransactionScope())
                {
                    updateContractPeriod = true;
                    fsAppointmentRow = (FSAppointment)SharedFunctions.ChangeAppointmentStatus(AppointmentRecords.View, fsAppointmentRow, ID.Status_Appointment.COMPLETED);
                    SharedFunctions.SaveUpdatedChanges(AppointmentRecords.Cache, fsAppointmentRow);

                    SetServiceOrderStatusFromAppointment(ServiceOrderRelated.Current, fsAppointmentRow);

                    ts.Complete();
                }

                // This is to refresh the ServiceOrder graph
                Actions.PressCancel();

                if (this.IsExternalTax(""))
                {
                    this.Save.Press();
                }
            }
        }
        #endregion
        #region InvoiceAppointment
        public PXAction<FSAppointment> invoiceAppointment;
        [PXButton]
        [PXUIField(DisplayName = "Generate Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public IEnumerable InvoiceAppointment(PXAdapter adapter)
        {
            List<FSAppointment> list = adapter.Get<FSAppointment>().ToList();
            List<AppointmentToPost> rows = new List<AppointmentToPost>();

            if (!adapter.MassProcess)
            {
                try
                {
                    RecalculateExternalTaxesSync = true;
                    Save.Press();
                }
                finally
                {
                    RecalculateExternalTaxesSync = false;
                }
            }

            foreach (FSAppointment fsAppointmentRow in list)
            {
                PXLongOperation.StartOperation(
                this,
                delegate ()
                {
                    CreateInvoiceByAppointmentPost graphCreateInvoiceByAppointmentPost = PXGraph.CreateInstance<CreateInvoiceByAppointmentPost>();
                    graphCreateInvoiceByAppointmentPost.Filter.Current.PostTo = ServiceOrderTypeSelected.Current.PostTo == ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE ? ID.Batch_PostTo.AR_AP : ServiceOrderTypeSelected.Current.PostTo;
                    graphCreateInvoiceByAppointmentPost.Filter.Current.IgnoreBillingCycles = true;
                    graphCreateInvoiceByAppointmentPost.Filter.Current.BranchID = fsAppointmentRow.BranchID;
                    graphCreateInvoiceByAppointmentPost.Filter.Current.LoadData = true;
                    graphCreateInvoiceByAppointmentPost.Filter.Current.IsGenerateInvoiceScreen = false;
                    if(fsAppointmentRow.ActualDateTimeBegin > Accessinfo.BusinessDate)
                    {
                        graphCreateInvoiceByAppointmentPost.Filter.Current.UpToDate = fsAppointmentRow.ActualDateTimeBegin;
                        graphCreateInvoiceByAppointmentPost.Filter.Current.InvoiceDate = fsAppointmentRow.ActualDateTimeBegin;
                    }
                    graphCreateInvoiceByAppointmentPost.Filter.Insert(graphCreateInvoiceByAppointmentPost.Filter.Current);
                    AppointmentToPost appointmentToPostRow = graphCreateInvoiceByAppointmentPost.PostLines.Current =
                                graphCreateInvoiceByAppointmentPost.PostLines.Search<AppointmentToPost.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);
                    rows = new List<AppointmentToPost>
                    {
                        appointmentToPostRow
                    };

                    var jobExecutor = new JobExecutor<InvoicingProcessStepGroupShared>(processorCount: 1);
                    Guid currentProcessID = CreateInvoiceByAppointmentPost.CreateInvoices(graphCreateInvoiceByAppointmentPost, rows, graphCreateInvoiceByAppointmentPost.Filter.Current, null, jobExecutor, adapter.QuickProcessFlow);

                    if (graphCreateInvoiceByAppointmentPost.Filter.Current.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE)
                    {
                        foreach (PXResult<FSPostBatch> result in SharedFunctions.GetPostBachByProcessID(this, currentProcessID))
                        {
                            FSPostBatch fSPostBatchRow = (FSPostBatch)result;

                            CreateInvoiceByAppointmentPost.ApplyPrepayments(fSPostBatchRow);
                        }
                    }

                    if (!adapter.MassProcess || this.IsMobile == true)
                    {
                        using (new PXTimeStampScope(null))
                        {
                            AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();
                            graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);
                            graphAppointmentEntry.AppointmentPostedIn.Current = graphAppointmentEntry.AppointmentPostedIn.Select();

                            graphAppointmentEntry.openPostingDocument();
                        }
                    }
                });
            }

            return list;
        }
        #endregion

        #region OpenEmployeeBoard
        public PXAction<FSAppointment> openEmployeeBoard;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.MULTI_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenEmployeeBoard()
        {
            throw new PXRedirectToBoardRequiredException(
                    Paths.ScreenPaths.MULTI_EMPLOYEE_DISPATCH,
                    AppointmentCore.GetAppointmentUrlArguments(AppointmentRecords.Current));
        }
        #endregion
        #region OpenRoomBoard
        public PXAction<FSAppointment> openRoomBoard;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.ROOM_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenRoomBoard()
        {
            throw new PXRedirectToBoardRequiredException(
                    Paths.ScreenPaths.MULTI_ROOM_DISPATCH,
                    AppointmentCore.GetAppointmentUrlArguments(AppointmentRecords.Current));
        }
        #endregion
        #region OpenUserCalendar
        public PXAction<FSAppointment> openUserCalendar;
        [PXUIField(DisplayName = TX.CalendarBoardAccess.SINGLE_EMP_CALENDAR, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void OpenUserCalendar()
        {
            throw new PXRedirectToBoardRequiredException(
                Paths.ScreenPaths.SINGLE_EMPLOYEE_DISPATCH,
                AppointmentCore.GetAppointmentUrlArguments(AppointmentRecords.Current));
        }
        #endregion
        #region OpenBatch
        public PXAction<FSAppointment> openBatch;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable OpenBatch(PXAdapter adapter)
        {
            FSAppointment fsServiceOrderRow = AppointmentRecords.Current;
            if (fsServiceOrderRow != null)
            {
                FSPostDet fsPostDetRow = AppointmentPostedIn.Select();

                if (fsPostDetRow != null)
                {
                    var graphPostBatchEntry = PXGraph.CreateInstance<PostBatchMaint>();

                    graphPostBatchEntry.BatchRecords.Current = graphPostBatchEntry.BatchRecords
                                .Search<FSPostBatch.batchID>(fsPostDetRow.BatchID);

                    throw new PXRedirectRequiredException(graphPostBatchEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }

            return adapter.Get();
        }
        #endregion

        #region CreateNewCustomer
        public PXAction<FSAppointment> createNewCustomer;
        [PXUIField(DisplayName = "Create New Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void CreateNewCustomer()
        {
            var graph = PXGraph.CreateInstance<CustomerMaint>();
            Customer customer = new Customer();
            graph.CurrentCustomer.Insert(customer);
            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
        #endregion

        #region EmailConfirmationToStaffMember
        public PXAction<FSAppointment> emailConfirmationToStaffMember;
        [PXUIField(DisplayName = "Send Email Confirmation to Staff Member", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable EmailConfirmationToStaffMember(PXAdapter adapter)
        {
            if (AppointmentRecords.Current != null)
            {
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(AppointmentRecords.Current.RefNbr, AppointmentRecords.Current.SrvOrdType);

                PXLongOperation.StartOperation(
                    this,
                    delegate
                    {
                        graphAppointmentEntry.SendNotification(graphAppointmentEntry, AppointmentRecords.Cache, FSMailProcess.FSMailing.EMAIL_CONFIRMATION_TO_STAFF, ServiceOrderRelated.Current.BranchID);
                    });
            }

            return adapter.Get();
        }
        #endregion

        #region EmailConfirmationToCustomer
        public PXAction<FSAppointment> emailConfirmationToCustomer;
        [PXUIField(DisplayName = "Send Email Confirmation to Customer", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable EmailConfirmationToCustomer(PXAdapter adapter)
        {
            if (AppointmentRecords.Current != null)
            {
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(AppointmentRecords.Current.RefNbr, AppointmentRecords.Current.SrvOrdType);

                PXLongOperation.StartOperation(
                    this,
                    delegate
                    {
                        graphAppointmentEntry.SendNotification(graphAppointmentEntry, AppointmentRecords.Cache, FSMailProcess.FSMailing.EMAIL_CONFIRMATION_TO_CUSTOMER, ServiceOrderRelated.Current.BranchID);
                    });
            }

            return adapter.Get();
        }
        #endregion

        #region EmailConfirmationToGeoZoneStaff
        public PXAction<FSAppointment> emailConfirmationToGeoZoneStaff;
        [PXUIField(DisplayName = "Send Email Notification to Service Area Staff", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable EmailConfirmationToGeoZoneStaff(PXAdapter adapter)
        {
            if (AppointmentRecords.Current != null)
            {
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(AppointmentRecords.Current.RefNbr, AppointmentRecords.Current.SrvOrdType);

                PXLongOperation.StartOperation(
                    this,
                    delegate
                    {
                        graphAppointmentEntry.SendNotification(graphAppointmentEntry, AppointmentRecords.Cache, FSMailProcess.FSMailing.EMAIL_NOTIFICATION_TO_GEOZONE_STAFF, ServiceOrderRelated.Current.BranchID);
                    });
            }

            return adapter.Get();
        }
        #endregion

        #region EmailSignedAppointment
        public PXAction<FSAppointment> emailSignedAppointment;
        [PXUIField(DisplayName = "Send Email with Signed Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable EmailSignedAppointment(PXAdapter adapter)
        {
            if (AppointmentRecords.Current != null)
            {
                AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(AppointmentRecords.Current.RefNbr, AppointmentRecords.Current.SrvOrdType);

                PXLongOperation.StartOperation(
                    this,
                    delegate
                    {
                        List<Guid?> attachments = new List<Guid?>();

                        if (AppointmentRecords.Current.CustomerSignedReport != null)
                        {
                            attachments.Add(AppointmentRecords.Current.CustomerSignedReport);
                        }

                        graphAppointmentEntry.SendNotification(graphAppointmentEntry, AppointmentRecords.Cache, FSMailProcess.FSMailing.EMAIL_NOTIFICATION_SIGNED_APPOINTMENT, ServiceOrderRelated.Current.BranchID, attachments);
                    });
            }

            return adapter.Get();
        }
        #endregion

        #region OpenPostingDocument
        public PXAction<FSAppointment> OpenPostingDocument;
        [PXButton]
        [PXUIField(DisplayName = "Open Document", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openPostingDocument()
        {
            FSPostDet fsPostDetRow = AppointmentPostedIn.Select();

            if (fsPostDetRow == null)
            {
                return;
            }

            if (fsPostDetRow.SOPosted == true)
            {
                if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>())
                {
                    SOOrderEntry graphSOOrderEntry = PXGraph.CreateInstance<SOOrderEntry>();
                    graphSOOrderEntry.Document.Current = graphSOOrderEntry.Document.Search<SOOrder.orderNbr>(fsPostDetRow.SOOrderNbr, fsPostDetRow.SOOrderType);
                    throw new PXRedirectRequiredException(graphSOOrderEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
            else if (fsPostDetRow.ARPosted == true && IsMobile == false)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.SOInvPosted == true)
            {
                SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.SOInvRefNbr, fsPostDetRow.SOInvDocType);
                throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDetRow.APRefNbr, fsPostDetRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.INPosted == true)
            {
                if (fsPostDetRow.INDocType.Trim() == INDocType.Receipt)
                {
                    INReceiptEntry graphINReceiptEntry = PXGraph.CreateInstance<INReceiptEntry>();
                    graphINReceiptEntry.receipt.Current = graphINReceiptEntry.receipt.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINReceiptEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else
                {
                    INIssueEntry graphINIssueEntry = PXGraph.CreateInstance<INIssueEntry>();
                    graphINIssueEntry.issue.Current = graphINIssueEntry.issue.Search<INRegister.refNbr>(fsPostDetRow.INRefNbr);
                    throw new PXRedirectRequiredException(graphINIssueEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
        }
        #endregion
        
        #region OpenInvoiceDocument
        public PXAction<FSAppointment> OpenInvoiceDocument;
        [PXButton]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void openInvoiceDocument()
        {
            FSPostDet fsPostDetRow = AppointmentPostedIn.Select();

            if (fsPostDetRow == null || fsPostDetRow.InvoiceRefNbr == null)
            {
                return;
            }

            if (fsPostDetRow.SOPosted == true)
            {
                ARInvoice aRInvoiceRow = PXSelect<ARInvoice,
                                                Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
                                                    And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>,
                                                    And<ARInvoice.released, Equal<False>>>>>
                                           .Select(this, fsPostDetRow.InvoiceDocType, fsPostDetRow.InvoiceRefNbr);

                if (aRInvoiceRow != null)
                {
                    SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                    graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<SOInvoice.refNbr>(fsPostDetRow.InvoiceRefNbr, fsPostDetRow.InvoiceDocType);
                    throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }

                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.InvoiceRefNbr, fsPostDetRow.InvoiceDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.SOInvPosted == true)
            {
                    SOInvoiceEntry graphSOInvoiceEntry = PXGraph.CreateInstance<SOInvoiceEntry>();
                    graphSOInvoiceEntry.Document.Current = graphSOInvoiceEntry.Document.Search<SOInvoice.refNbr>(fsPostDetRow.SOInvRefNbr, fsPostDetRow.SOInvDocType);
                    throw new PXRedirectRequiredException(graphSOInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.APPosted == true)
            {
                APInvoiceEntry graphAPInvoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
                graphAPInvoiceEntry.Document.Current = graphAPInvoiceEntry.Document.Search<APInvoice.refNbr>(fsPostDetRow.APRefNbr, fsPostDetRow.APDocType);
                throw new PXRedirectRequiredException(graphAPInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
            else if (fsPostDetRow.ARPosted == true)
            {
                ARInvoiceEntry graphARInvoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
                graphARInvoiceEntry.Document.Current = graphARInvoiceEntry.Document.Search<ARInvoice.refNbr>(fsPostDetRow.ARRefNbr, fsPostDetRow.ARDocType);
                throw new PXRedirectRequiredException(graphARInvoiceEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion

        #region AppointmentsReports
        public PXAction<FSAppointment> printAppointmentReport;
        [PXUIField(DisplayName = "Print Appointment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void PrintAppointmentReport()
        {
            if (this.IsDirty)
            {
                Save.Press();
            }
            
            FSAppointment fsAppointmentRow = AppointmentRecords.Current;

            if (fsAppointmentRow != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                string srvOrdTypeFieldName = SharedFunctions.GetFieldName<FSAppointment.srvOrdType>();
                string refNbrFieldName = SharedFunctions.GetFieldName<FSAppointment.refNbr>();

                parameters[srvOrdTypeFieldName] = fsAppointmentRow.SrvOrdType;
                parameters[refNbrFieldName] = fsAppointmentRow.RefNbr;

                throw new PXReportRequiredException(parameters, ID.ReportID.APPOINTMENT, PXBaseRedirectException.WindowMode.NewWindow, string.Empty);
            }
        }

        public PXAction<FSAppointment> printServiceTimeActivityReport;
        [PXUIField(DisplayName = "Print Service Time Activity", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void PrintServiceTimeActivityReport()
        {
            if (this.IsDirty)
            {
                Save.Press();
            }

            FSAppointment fsAppointmentRow = AppointmentRecords.Current;

            if (fsAppointmentRow != null)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                string srvOrdTypeFieldName = SharedFunctions.GetFieldName<FSAppointment.srvOrdType>();
                string appRefNbrFieldName = SharedFunctions.GetFieldName<FSAppointment.refNbr>();
                string soRefNbrFieldName = SharedFunctions.GetFieldName<FSAppointment.soRefNbr>();
                ////This two parameters are for the Report ServiceTimeActivity
                string DateFromFieldName = "DateFrom";
                string DateToFieldName = "DateTo";

                parameters[srvOrdTypeFieldName] = fsAppointmentRow.SrvOrdType;
                parameters[appRefNbrFieldName] = fsAppointmentRow.RefNbr;
                parameters[soRefNbrFieldName] = fsAppointmentRow.SORefNbr;
                parameters[DateFromFieldName] = fsAppointmentRow.ExecutionDate.ToString();
                parameters[DateToFieldName] = fsAppointmentRow.ExecutionDate.ToString();

                throw new PXReportRequiredException(parameters, ID.ReportID.SERVICE_TIME_ACTIVITY, PXBaseRedirectException.WindowMode.NewWindow, string.Empty);
            }
        }
        #endregion

        #region StartService
        public PXAction<FSAppointment> startService;
        [PXUIField(DisplayName = "Start Service", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable StartService(PXAdapter adapter)
        {
            SkipTaxCalcAndSave();

            FSAppointment fsAppointmentRow = (FSAppointment)Caches[typeof(FSAppointment)].CreateCopy(AppointmentRecords.Current);

            using (var ts = new PXTransactionScope())
            {
                if (AppointmentRecords.Cache.GetStatus(fsAppointmentRow) != PXEntryStatus.Inserted)
                {
                    FSAppointmentDetService fsAppointmentDetRow = AppointmentDetServices.Current;
                    if (fsAppointmentDetRow != null && (fsAppointmentDetRow.LineType == ID.LineType_All.SERVICE || fsAppointmentDetRow.LineType == ID.LineType_All.NONSTOCKITEM))
                    {
                        fsAppointmentDetRow.ActualDateTimeBegin = AppointmentDateTimeHelper.GetServiceActualDateTimeBegin(fsAppointmentDetRow);
                        AppointmentDateTimeHelper.UpdateServiceActualDateTimeEndFromActualDuration(fsAppointmentDetRow);

                        if (fsAppointmentDetRow.StaffRelatedCount == 1)
                        {
                            AppointmentDateTimeHelper.UpdateStaffActualTimeFromService(fsAppointmentDetRow, AppointmentEmployees);
                        }

                        AppointmentDetServices.Update(fsAppointmentDetRow);
                        AppointmentDetServices.Cache.AllowUpdate = true;
                        AppointmentDetServices.Cache.SetStatus(fsAppointmentDetRow, PXEntryStatus.Updated);
                        SkipTaxCalcAndSave();
                    }
                }

                ts.Complete();
            }

            return adapter.Get();
        }
        #endregion
        #region CompleteService
        public PXAction<FSAppointment> completeService;
        [PXUIField(DisplayName = "Complete Service", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable CompleteService(PXAdapter adapter)
        {
            SkipTaxCalcAndSave();

            FSAppointment fsAppointmentRow = (FSAppointment)Caches[typeof(FSAppointment)].CreateCopy(AppointmentRecords.Current);

            using (var ts = new PXTransactionScope())
            {
                if (AppointmentRecords.Cache.GetStatus(fsAppointmentRow) != PXEntryStatus.Inserted)
                {
                    FSAppointmentDetService fsAppointmentDetRow = AppointmentDetServices.Current;
                    if (fsAppointmentDetRow != null && (fsAppointmentDetRow.LineType == ID.LineType_All.SERVICE || fsAppointmentDetRow.LineType == ID.LineType_All.NONSTOCKITEM))
                    {
                        fsAppointmentDetRow.ActualDateTimeEnd = AppointmentDateTimeHelper.GetServiceActualDateTimeEnd(fsAppointmentDetRow);
                        AppointmentDetServices.Update(fsAppointmentDetRow);

                        if (fsAppointmentDetRow.StaffRelatedCount == 1)
                        {
                            AppointmentDateTimeHelper.UpdateStaffActualTimeFromService(fsAppointmentDetRow, AppointmentEmployees);
                        }

                        AppointmentDetServices.Update(fsAppointmentDetRow);
                        AppointmentDetServices.Cache.AllowUpdate = true;
                        AppointmentDetServices.Cache.SetStatus(fsAppointmentDetRow, PXEntryStatus.Updated);
                        SkipTaxCalcAndSave();
                    }
                }

                ts.Complete();
            }

            return adapter.Get();
        }
        #endregion
        #region StartTimeStaff
        public PXAction<FSAppointment> startTimeStaff;
        [PXUIField(DisplayName = "Start Time", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable StartTimeStaff(PXAdapter adapter)
        {
            SkipTaxCalcAndSave();

            FSAppointment fsAppointmentRow = (FSAppointment)Caches[typeof(FSAppointment)].CreateCopy(AppointmentRecords.Current);

            using (var ts = new PXTransactionScope())
            {
                if (AppointmentRecords.Cache.GetStatus(fsAppointmentRow) != PXEntryStatus.Inserted)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = AppointmentEmployees.Current;

                    if (fsAppointmentEmployeeRow != null)
                    {
                        fsAppointmentEmployeeRow.ActualDateTimeBegin = AppointmentDateTimeHelper.GetStaffActualDateTimeBegin(fsAppointmentEmployeeRow);
                        AppointmentDateTimeHelper.UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);

                        AppointmentEmployees.Update(fsAppointmentEmployeeRow);
                        AppointmentEmployees.Cache.AllowUpdate = true;
                        AppointmentEmployees.Cache.SetStatus(fsAppointmentEmployeeRow, PXEntryStatus.Updated);
                        SkipTaxCalcAndSave();
                    }
                }

                ts.Complete();
            }

            return adapter.Get();
        }
        #endregion
        #region CompleteTimeStaff
        public PXAction<FSAppointment> completeTimeStaff;
        [PXUIField(DisplayName = "Complete Time", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        [PXButton]
        public virtual IEnumerable CompleteTimeStaff(PXAdapter adapter)
        {
            SkipTaxCalcAndSave();

            FSAppointment fsAppointmentRow = (FSAppointment)Caches[typeof(FSAppointment)].CreateCopy(AppointmentRecords.Current);

            using (var ts = new PXTransactionScope())
            {
                if (AppointmentRecords.Cache.GetStatus(fsAppointmentRow) != PXEntryStatus.Inserted)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = AppointmentEmployees.Current;

                    if (fsAppointmentEmployeeRow != null)
                    {
                        fsAppointmentEmployeeRow.ActualDateTimeEnd = AppointmentDateTimeHelper.GetStaffActualDateTimeEnd(fsAppointmentEmployeeRow);
                        AppointmentEmployees.Update(fsAppointmentEmployeeRow);
                        AppointmentEmployees.Cache.AllowUpdate = true;
                        AppointmentEmployees.Cache.SetStatus(fsAppointmentEmployeeRow, PXEntryStatus.Updated);
                        SkipTaxCalcAndSave();
                    }
                }

                ts.Complete();
            }
            return adapter.Get();
        }
        #endregion

        #region ViewPayment
        public PXAction<FSAppointment> viewPayment;
        [PXUIField(DisplayName = "View Payment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXLookupButton]
        public virtual void ViewPayment()
        {
            if (ServiceOrderRelated.Current != null && AppointmentRecords.Current != null && Adjustments.Current != null)
            {
                ARPaymentEntry graphARPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
                graphARPaymentEntry.Document.Current = graphARPaymentEntry.Document.Search<ARPayment.refNbr>(Adjustments.Current.RefNbr, Adjustments.Current.DocType);

                throw new PXRedirectRequiredException(graphARPaymentEntry, true, "Payment") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            }
        }
        #endregion
        #region CreatePrepayment
        public PXAction<FSAppointment> createPrepayment;
        [PXUIField(DisplayName = "Create Prepayment", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        protected virtual void CreatePrepayment()
        {
            if (ServiceOrderRelated.Current != null && AppointmentRecords.Current != null)
            {
                this.Save.Press();

                PXGraph target;

                ServiceOrderCore.CreatePrepayment(ServiceOrderRelated.Current, AppointmentRecords.Current, out target, ARPaymentType.Prepayment);

                throw new PXPopupRedirectException(target, "New Payment", true);
            }
        }
        #endregion

        #region QuickProcessMobile
        public PXAction<FSAppointment> quickProcessMobile;
        [PXButton]
        [PXUIField(DisplayName = "Quick Process", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public IEnumerable QuickProcessMobile(PXAdapter adapter)
        {
            if (AppointmentRecords.Cache.GetStatus(AppointmentRecords.Current) != PXEntryStatus.Inserted)
            {
                FSAppointment fsAppointmentRow = AppointmentRecords.Current;
                fsAppointmentRow.IsCalledFromQuickProcess = true;
                AppointmentEntry.AppointmentQuickProcess.InitQuickProcessPanel(this, "");

                if (this.AppointmentQuickProcessExt.QuickProcessParameters.Current.CloseAppointment == true
                    && fsAppointmentRow.Status == ID.Status_Appointment.CLOSED)
                {
                    this.AppointmentQuickProcessExt.QuickProcessParameters.Current.CloseAppointment = false;
                }

                PXQuickProcess.Start(this, fsAppointmentRow, this.AppointmentQuickProcessExt.QuickProcessParameters.Current);
            }

            return adapter.Get();
        }
        #endregion

        #region OpenScheduleScreen
        public PXAction<FSAppointment> OpenScheduleScreen;
        [PXButton(OnClosingPopup = PXSpecialButtonType.Cancel)]
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
        protected virtual void openScheduleScreen()
        {
            if (ServiceOrderRelated.Current != null && ServiceOrderTypeSelected.Current != null)
            {
                if(ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT)
                {
                    var graphRouteServiceContractScheduleEntry = PXGraph.CreateInstance<RouteServiceContractScheduleEntry>();

                    graphRouteServiceContractScheduleEntry.ContractScheduleRecords.Current = graphRouteServiceContractScheduleEntry
                                                                                            .ContractScheduleRecords.Search<FSRouteContractSchedule.scheduleID>(
                                                                                                ServiceOrderRelated.Current.ScheduleID,
                                                                                                ServiceOrderRelated.Current.ServiceContractID,
                                                                                                ServiceOrderRelated.Current.CustomerID);

                    throw new PXRedirectRequiredException(graphRouteServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
                else
                {
                    var graphServiceContractScheduleEntry = PXGraph.CreateInstance<ServiceContractScheduleEntry>();

                    graphServiceContractScheduleEntry.ContractScheduleRecords.Current = graphServiceContractScheduleEntry
                                                                                            .ContractScheduleRecords.Search<FSContractSchedule.scheduleID>(
                                                                                                ServiceOrderRelated.Current.ScheduleID,
                                                                                                ServiceOrderRelated.Current.ServiceContractID,
                                                                                                ServiceOrderRelated.Current.CustomerID);

                    throw new PXRedirectRequiredException(graphServiceContractScheduleEntry, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
                }
            }
        }
        #endregion

        #endregion

        #region Selects / Views
        [PXHidden]
        public PXSelect<InventoryItem> InventoryItemHelper;
        
        [PXCopyPasteHiddenView]
        public PXSetup<FSBillingCycle,
                InnerJoin<FSCustomerBillingSetup,
                    On<FSBillingCycle.billingCycleID, Equal<FSCustomerBillingSetup.billingCycleID>>>,
                Where<FSCustomerBillingSetup.cBID, Equal<Current<FSServiceOrder.cBID>>>> BillingCycleRelated;

        public PXSetup<FSServiceContract>.Where<
                        Where<FSServiceContract.serviceContractID, Equal<Current<FSAppointment.billServiceContractID>>,
                        And<FSServiceContract.billCustomerID, Equal<Current<FSServiceOrder.billCustomerID>>>>> StandardContractRelated;

        public PXSetup<FSContractPeriod>.Where<
                        Where<
                        FSContractPeriod.contractPeriodID, Equal<Current<FSAppointment.billContractPeriodID>>,
                        And<FSContractPeriod.serviceContractID, Equal<Current<FSAppointment.billServiceContractID>>,
                        And<Current<FSBillingCycle.billingBy>, Equal<FSBillingCycle.billingBy.Appointment>>>>> StandardContractPeriod;

        public PXSelect<FSContractPeriodDet,
                    Where<FSContractPeriodDet.contractPeriodID, Equal<Current<FSContractPeriod.contractPeriodID>>,
                        And<FSContractPeriodDet.serviceContractID, Equal<Current<FSContractPeriod.serviceContractID>>>>> StandardContractPeriodDetail;
        [PXHidden]
        public PXSelect<Contract,
        Where<
            Contract.contractID, Equal<Required<Contract.contractID>>>> ContractRelatedToProject;

        public PXSelect<FSWFStage,
        Where<
            FSWFStage.wFStageID, Equal<Required<FSWFStage.wFStageID>>>> WFStageRelated;


        [PXViewName(TX.FriendlyViewName.Appointment.APPOINTMENT_POSTED_IN)]
        [PXCopyPasteHiddenView]
        public PXSelectJoinGroupBy<FSPostDet,
                LeftJoin<FSAppointmentDet,
                    On<FSAppointmentDet.postID, Equal<FSPostDet.postID>>,
                LeftJoin<FSPostBatch,
                    On<FSPostBatch.batchID, Equal<FSPostDet.batchID>>>>,
                Where<
                    FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>>,
                Aggregate<
                    GroupBy<FSPostDet.batchID,
                    GroupBy<FSPostDet.aRPosted,
                    GroupBy<FSPostDet.aPPosted,
                    GroupBy<FSPostDet.iNPosted,
                    GroupBy<FSPostDet.sOPosted>>>>>>> AppointmentPostedIn;

        [PXCopyPasteHiddenView]
        public PXSelect<FSSchedule,
                Where<FSSchedule.scheduleID, Equal<Current<FSAppointment.scheduleID>>>> ScheduleRecord;

        [PXViewName(PX.Objects.CR.Messages.MainContact)]
        public PXSelect<Contact> DefaultCompanyContact;

        [PXCopyPasteHiddenView]
        public PXSelectReadonly<FSProfitability> ProfitabilityRecords;

        [PXHidden]
        public PXSelectReadonly<FSSrvOrdTypeAttributeList> AttributeListRecords;

        protected virtual IEnumerable attributeListRecords()
        {
            FSSrvOrdTypeAttributeList fsSrvOrdTypeAttributeRow = AttributeListRecords.Current;

            if (fsSrvOrdTypeAttributeRow == null)
            {
                fsSrvOrdTypeAttributeRow = new FSSrvOrdTypeAttributeList();
            }

            if (AppointmentRecords.Current != null)
            {
                fsSrvOrdTypeAttributeRow.SOID = AppointmentRecords.Current.SOID;
                fsSrvOrdTypeAttributeRow.AppointmentID = AppointmentRecords.Current.AppointmentID;
                fsSrvOrdTypeAttributeRow.SrvOrdType = AppointmentRecords.Current.SrvOrdType;
                fsSrvOrdTypeAttributeRow.NoteID = AppointmentRecords.Current.NoteID;

                yield return fsSrvOrdTypeAttributeRow;
            }
            else
            {
                yield return null;
            }
        }

        [PXViewName(CR.Messages.Answers)]
        public FSAttributeList<FSSrvOrdTypeAttributeList, FSAppointment> Answers;

        [InjectDependency]
        protected ILicenseLimitsService _licenseLimits { get; set; }
        #endregion

        void IGraphWithInitialization.Initialize()
        {
            if (_licenseLimits != null)
            {
                OnBeforeCommit += _licenseLimits.GetCheckerDelegate<FSAppointment>(
                    new TableQuery(TransactionTypes.LinesPerMasterRecord, typeof(FSAppointmentDet), (graph) =>
                    {
                        return new PXDataFieldValue[]
                        {
                                new PXDataFieldValue<FSAppointmentDet.srvOrdType>(PXDbType.Char, ((AppointmentEntry)graph).AppointmentRecords.Current?.SrvOrdType),
                                new PXDataFieldValue<FSAppointmentDet.refNbr>(((AppointmentEntry)graph).AppointmentRecords.Current?.RefNbr)
                        };
                    }));
            }
        }

        #region Workflow Stages tree
        public TreeWFStageHelper.TreeWFStageView TreeWFStages;

        protected virtual IEnumerable treeWFStages(
                [PXInt]
                int? wFStageID)
        {
            if (AppointmentRecords.Current == null)
            {
                return null;
            }
            

            return TreeWFStageHelper.treeWFStages(this, AppointmentRecords.Current.SrvOrdType, wFStageID);
        }
        #endregion

        /// <summary>
        /// Gets the license types related for the given appointment services. Also sets a list with the License Type identifiers 
        /// related to the appointment services.
        /// </summary>
        /// <param name="bqlResultSet">Set of appointment detail services.</param>
        /// <param name="serviceLicenseIDs">This list contains the union of all license types related to the given appointment services.</param>
        /// <returns>List of services with their respective related license types.</returns>
        public List<ServiceRequirement> GetAppointmentDetServiceRowLicenses(
                                                                            PXResultset<FSAppointmentDetService> bqlResultSet,
                                                                            ref List<int?> serviceLicenseIDs)
        {
            List<ServiceRequirement> serviceLicensesList = new List<ServiceRequirement>();
            List<object> args = new List<object>();

            BqlCommand fsServiceLicenseRows = new Select2<FSServiceLicenseType,
                                                  InnerJoin<InventoryItem,
                                                    On<FSServiceLicenseType.serviceID, Equal<InventoryItem.inventoryID>>>,
                                                  Where<True, Equal<True>>>();

            fsServiceLicenseRows = fsServiceLicenseRows.WhereAnd(InHelper<InventoryItem.inventoryID>.Create(bqlResultSet.Count));
            foreach (FSAppointmentDetService fsAppointmentDetServiceRow in bqlResultSet)
            {
                args.Add(fsAppointmentDetServiceRow.InventoryID);
            }

            PXView serviceLicensesView = new PXView(this, true, fsServiceLicenseRows);
            var fsServiceLicenseTypeSet = serviceLicensesView.SelectMulti(args.ToArray());

            if (fsServiceLicenseTypeSet.Count == 0)
            {
                return serviceLicensesList;
            }

            foreach (PXResult<FSServiceLicenseType, InventoryItem> bqlResult in fsServiceLicenseTypeSet)
            {
                InventoryItem fsServiceRow = (InventoryItem)bqlResult;
                FSServiceLicenseType fsServiceLicenseTypeRow = (FSServiceLicenseType)bqlResult;
                serviceLicenseIDs.Add(fsServiceLicenseTypeRow.LicenseTypeID);

                var serviceLicenses = serviceLicensesList.Where(list => list.serviceID == fsServiceRow.InventoryID).FirstOrDefault();
                if (serviceLicenses != null)
                {
                    serviceLicenses.requirementIDList.Add((int)fsServiceLicenseTypeRow.LicenseTypeID);
                }
                else
                {
                    ServiceRequirement newServiceLicenses = new ServiceRequirement()
                    {
                        serviceID = (int)fsServiceRow.InventoryID
                    };
                    newServiceLicenses.requirementIDList.Add(fsServiceLicenseTypeRow.LicenseTypeID);
                    serviceLicensesList.Add(newServiceLicenses);
                }
            }

            return serviceLicensesList;
        }

        /// <summary>
        /// Gets the license types related for the given appointment employees.  
        /// </summary>
        /// <param name="bqlResultSet">Set of appointment employees.</param>
        /// <returns>List of unexpired license identifiers owned by the given appointment employees.</returns>
        public List<int?> GetAppointmentEmpoyeeLicenseIDs(PXResultset<FSAppointmentEmployee> bqlResultSet)
        {
            List<int?> appointmentEmployeeLicenseIDList = new List<int?>();
            List<object> args = new List<object>();
            DateTime tempDate = new DateTime(
                                            AppointmentSelected.Current.ScheduledDateTimeBegin.Value.Year,
                                            AppointmentSelected.Current.ScheduledDateTimeBegin.Value.Month,
                                            AppointmentSelected.Current.ScheduledDateTimeBegin.Value.Day);

            BqlCommand fsAppointmentEmployeeLicenseRows = new Select4<FSLicense,
                                                                Where<FSLicense.expirationDate, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>>,
                                                                Aggregate<GroupBy<FSLicense.licenseTypeID>>,
                                                                OrderBy<Asc<FSLicense.licenseID>>>();
            args.Add(tempDate);
            fsAppointmentEmployeeLicenseRows = fsAppointmentEmployeeLicenseRows.WhereAnd(
                InHelper<FSLicense.employeeID>.Create(bqlResultSet.Count));
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in bqlResultSet)
            {
                args.Add(fsAppointmentEmployeeRow.EmployeeID);
            }

            PXView appointmentEmployeeLicenseView = new PXView(this, true, fsAppointmentEmployeeLicenseRows);
            var fsLicenseSet = appointmentEmployeeLicenseView.SelectMulti(args.ToArray());

            foreach (FSLicense fsLicenseRow in fsLicenseSet)
            {
                appointmentEmployeeLicenseIDList.Add(fsLicenseRow.LicenseTypeID);
            }
           
            return appointmentEmployeeLicenseIDList;
        }

        /// <summary>
        /// Gets the skills related for the given appointment services. Also sets a list with the skills identifiers 
        /// related to the appointment services.
        /// </summary>
        /// <param name="bqlResultSet">Set of appointment detail services.</param>
        /// <param name="serviceSkillIDs">This list contains the union of all skills related to the given appointment services.</param>
        /// <returns>List of services with their respective related skills.</returns>
        public List<ServiceRequirement> GetAppointmentDetServiceRowSkills(
                                                                        PXResultset<FSAppointmentDetService> bqlResultSet,
                                                                        ref List<int?> serviceSkillIDs)
        {
            List<ServiceRequirement> serviceSkillsList = new List<ServiceRequirement>();
            List<object> args = new List<object>();

            BqlCommand fsServiceSkillsRows = new Select2<FSServiceSkill,
                                                 InnerJoin<InventoryItem,
                                                    On<FSServiceSkill.serviceID, Equal<InventoryItem.inventoryID>>>,
                                                 Where<True, Equal<True>>>();

            fsServiceSkillsRows = fsServiceSkillsRows.WhereAnd(InHelper<InventoryItem.inventoryID>.Create(bqlResultSet.Count));
            foreach (FSAppointmentDet fsAppointmentDetRow in bqlResultSet)
            {
                args.Add(fsAppointmentDetRow.InventoryID);
            }

            PXView serviceSkillsView = new PXView(this, true, fsServiceSkillsRows);
            var fsServiceSkillSet  = serviceSkillsView.SelectMulti(args.ToArray());

            if (fsServiceSkillSet.Count == 0)
            {
                return serviceSkillsList;
            }

            foreach (PXResult<FSServiceSkill, InventoryItem> bqlResult in fsServiceSkillSet)
            {
                InventoryItem fsServiceRow = (InventoryItem)bqlResult;
                FSServiceSkill fsServiceSkillRow = (FSServiceSkill)bqlResult;
                serviceSkillIDs.Add((int)fsServiceSkillRow.SkillID);

                var serviceSkills = serviceSkillsList.Where(list => list.serviceID == fsServiceRow.InventoryID).FirstOrDefault();
                if (serviceSkills != null)
                {
                    serviceSkills.requirementIDList.Add((int)fsServiceSkillRow.SkillID);
                }
                else
                {
                    ServiceRequirement newServiceSkills = new ServiceRequirement()
                    {
                        serviceID = (int)fsServiceRow.InventoryID
                    };
                    newServiceSkills.requirementIDList.Add((int)fsServiceSkillRow.SkillID);
                    serviceSkillsList.Add(newServiceSkills);
                }
            }

            return serviceSkillsList;
        }

        /// <summary>
        /// Gets the skills related for the given appointment employees.  
        /// </summary>
        /// <param name="bqlResultSet">Set of appointment employees.</param>
        /// <returns>List of skill identifiers owned by the given appointment employees.</returns>
        public List<int?> GetAppointmentEmpoyeeSkillIDs(PXResultset<FSAppointmentEmployee> bqlResultSet)
        {
            List<int?> appointmentEmployeeSkillIDList = new List<int?>();
            List<object> args = new List<object>();

            BqlCommand fsAppointmentEmployeeSkillRows = new Select4<FSEmployeeSkill,
                                                            Where<True, Equal<True>>,
                                                            Aggregate<GroupBy<FSEmployeeSkill.skillID>>,
                                                            OrderBy<Asc<FSEmployeeSkill.skillID>>>();

            fsAppointmentEmployeeSkillRows = fsAppointmentEmployeeSkillRows.WhereAnd(InHelper<FSEmployeeSkill.employeeID>.Create(bqlResultSet.Count));
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in bqlResultSet)
            {
                args.Add(fsAppointmentEmployeeRow.EmployeeID);
            }

            PXView appointmentEmployeeSkillView = new PXView(this, true, fsAppointmentEmployeeSkillRows);
            var fsEmployeeSkillSet = appointmentEmployeeSkillView.SelectMulti(args.ToArray());

            foreach (FSEmployeeSkill fsEmployeeSkillRow in fsEmployeeSkillSet)
            {
                appointmentEmployeeSkillIDList.Add(fsEmployeeSkillRow.SkillID);
            }

            return appointmentEmployeeSkillIDList;
        }

        /// <summary>
        /// Updates ProjectID in the Lines of the Appointment using the project in the <c>fsServiceOrderRow</c>. Also, sets ProjectTaskID to null.
        /// </summary>
        public void UpdateDetailsFromProjectID(FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow.ProjectID == null)
            {
                return;
            }

            if (AppointmentDetServices != null && AppointmentDetParts != null)
            {
                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in AppointmentDetServices.Select())
                {
                    fsAppointmentDetServiceRow.ProjectID = fsServiceOrderRow.ProjectID;
                    fsAppointmentDetServiceRow.ProjectTaskID = null;
                    AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                }

                foreach (FSAppointmentDetPart fsAppointmentDetPartRow in AppointmentDetParts.Select())
                {
                    fsAppointmentDetPartRow.ProjectID = fsServiceOrderRow.ProjectID;
                    fsAppointmentDetPartRow.ProjectTaskID = null;
                    AppointmentDetParts.Update(fsAppointmentDetPartRow);
                }

                foreach (FSAppointmentInventoryItem fsAppointmentInventoryItemRow in PickupDeliveryItems.Select())
                {
                    fsAppointmentInventoryItemRow.ProjectID = fsServiceOrderRow.ProjectID;
                    fsAppointmentInventoryItemRow.ProjectTaskID = null;
                    PickupDeliveryItems.Update(fsAppointmentInventoryItemRow);
                }
            }
        }

        /// <summary>
        /// Updates BranchID in the Lines of the Appointment using the branch in the <c>fsServiceOrderRow</c>.
        /// </summary>
        public void UpdateDetailsFromBranchID(FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow.BranchID == null)
            {
                return;
            }

            if (AppointmentDetServices != null)
            {
                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in AppointmentDetServices.Select())
                {
                    fsAppointmentDetServiceRow.BranchID = fsServiceOrderRow.BranchID;
                    AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                }
            }

            if (AppointmentDetParts != null)
            {
                foreach (FSAppointmentDetPart fsAppointmentDetPartRow in AppointmentDetParts.Select())
                {
                    fsAppointmentDetPartRow.BranchID = fsServiceOrderRow.BranchID;
                    AppointmentDetParts.Update(fsAppointmentDetPartRow);
                }
            }

            if (PickupDeliveryItems != null)
            {
                foreach (FSAppointmentInventoryItem fsAppointmentInventoryItemRow in PickupDeliveryItems.Select())
                {
                    fsAppointmentInventoryItemRow.BranchID = fsServiceOrderRow.BranchID;
                    PickupDeliveryItems.Update(fsAppointmentInventoryItemRow);
                }
            }
        }

        public void CalculateLaborCosts()
        {
            object unitcost;

            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in this.AppointmentEmployees.Select())
            {
                this.AppointmentEmployees.Cache.RaiseFieldDefaulting<FSAppointmentEmployee.unitCost>(fsAppointmentEmployeeRow, out unitcost);
                fsAppointmentEmployeeRow.CuryUnitCost = (decimal)unitcost;
                this.AppointmentEmployees.Update(fsAppointmentEmployeeRow);
            }
        }

        public void CalculateCosts()
        {
            object unitcost;

            foreach (FSAppointmentDetService fsAppointmentDetServiceRow in this.AppointmentDetServices.Select().Where(x => ((FSAppointmentDetService)x).LineType == ID.LineType_All.NONSTOCKITEM && ((FSAppointmentDetService)x).Status != ID.Status_AppointmentDet.CANCELED))
            {
                this.AppointmentDetServices.Cache.RaiseFieldDefaulting<FSAppointmentDetService.unitCost>(fsAppointmentDetServiceRow, out unitcost);
                fsAppointmentDetServiceRow.CuryUnitCost = (decimal)unitcost;
                this.AppointmentDetServices.Update(fsAppointmentDetServiceRow);
            }

            this.CalculateLaborCosts();
        }

        public decimal? CalculateLaborCost(PXCache cache, FSAppointmentEmployee fsAppointmentEmployeeRow, FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentEmployeeRow.LaborItemID == null)
            {
                return null;
            }

            PMLaborCostRate laborCostRate = null;

            if (fsAppointmentEmployeeRow.Type == BAccountType.EmployeeType)
            {
                laborCostRate = PXSelect<PMLaborCostRate,
                                    Where<
                                        PMLaborCostRate.type, Equal<PMLaborCostRateType.employee>,
                                    And<
                                        PMLaborCostRate.employeeID, Equal<Required<PMLaborCostRate.employeeID>>,
                                    And<
                                        PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
                                    And<
                                        PMLaborCostRate.employmentType, Equal<EP.RateTypesAttribute.hourly>,
                                    And<
                                        PMLaborCostRate.curyID, Equal<Required<PMLaborCostRate.curyID>>,
                                    And<
                                        PMLaborCostRate.effectiveDate, LessEqual<Required<PMLaborCostRate.effectiveDate>>>>>>>>,
                                    OrderBy<
                                        Desc<PMLaborCostRate.effectiveDate>>>
                                    .Select(this, fsAppointmentEmployeeRow.EmployeeID, fsAppointmentEmployeeRow.LaborItemID, fsAppointmentRow.CuryID, fsAppointmentRow.ExecutionDate)
									.AsEnumerable()
                                    .FirstOrDefault();

                if (laborCostRate == null)
                {
                    laborCostRate = PXSelect<PMLaborCostRate,
                                Where<
                                    PMLaborCostRate.type, Equal<PMLaborCostRateType.item>,
                                And<
                                    PMLaborCostRate.inventoryID, Equal<Required<PMLaborCostRate.inventoryID>>,
                                And<
                                    PMLaborCostRate.employmentType, Equal<EP.RateTypesAttribute.hourly>,
                                And<
                                    PMLaborCostRate.curyID, Equal<Required<PMLaborCostRate.curyID>>,
                                And<
                                    PMLaborCostRate.effectiveDate, LessEqual<Required<PMLaborCostRate.effectiveDate>>>>>>>,
                                OrderBy<
                                    Desc<PMLaborCostRate.effectiveDate>>>
                                .Select(this, fsAppointmentEmployeeRow.LaborItemID, fsAppointmentRow.CuryID, fsAppointmentRow.ExecutionDate)
								.AsEnumerable()
                                .FirstOrDefault();
                }
            }

            return laborCostRate != null ? laborCostRate.Rate : null;
        }

        public virtual IEnumerable profitabilityRecords()
        {
            return Delegate_ProfitabilityRecords(AppointmentRecords.Cache, AppointmentRecords.Current);
        }

        public List<FSProfitability> Delegate_ProfitabilityRecords(PXCache cache, FSAppointment fsAppointmentRow)
        {
            List<FSProfitability> returnList = new List<FSProfitability>();

            if (fsAppointmentRow == null)
            {
                return returnList;
            }

            foreach (FSAppointmentDetPart fsAppDetPartRow in AppointmentDetParts.Select().AsEnumerable().Where(x => ((FSAppointmentDetPart)x).LineType == ID.LineType_All.INVENTORY_ITEM && ((FSAppointmentDetPart)x).Status != ID.Status_AppointmentDet.CANCELED))
            {
                FSProfitability fsProfitabilityRow = new FSProfitability(fsAppDetPartRow);
                returnList.Add(fsProfitabilityRow);
            }

            foreach (FSAppointmentEmployee fsAppEmployee in AppointmentEmployees.Select().AsEnumerable().Where(x => ((FSAppointmentEmployee)x).CuryUnitCost != 0m))
            {
                FSProfitability fsProfitabilityRow = new FSProfitability(fsAppEmployee);
                InventoryItem inventoyItemRow = SharedFunctions.GetInventoryItemRow(this, fsAppEmployee.LaborItemID);
                fsProfitabilityRow.Descr = inventoyItemRow.Descr;
                returnList.Add(fsProfitabilityRow);
            }

            foreach (FSAppointmentDetService fsAppDetServiceRow in AppointmentDetServices.Select().AsEnumerable().Where(x => ((FSAppointmentDetService)x).LineType == ID.LineType_All.NONSTOCKITEM && ((FSAppointmentDetService)x).Status != ID.Status_AppointmentDet.CANCELED))
            {
                FSProfitability fsProfitabilityRow = new FSProfitability(fsAppDetServiceRow);
                returnList.Add(fsProfitabilityRow);
            }

            return returnList;
        }

        #region Private Methods
        /// <summary>
        /// Sends Mail.
        /// </summary>
        private void SendNotification(AppointmentEntry graph, PXCache cache, string mailing, int? branchID, IList<Guid?> attachments = null)
        {
            FSMailProcess.SendNotification(graph, cache, mailing, branchID, null, attachments);
        }

        private void FillDocDesc(FSAppointment fsAppointmentRow)
        {
            FSAppointmentDet fsAppointmentDetRow_InDB = PXSelect<FSAppointmentDet,
                                                    Where<
                                                        FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>>,
                                                    OrderBy<
                                                        Asc<FSAppointmentDet.sODetID>>>
                                                    .SelectWindowed(this, 0, 1, fsAppointmentRow.AppointmentID);

            fsAppointmentRow.DocDesc = fsAppointmentDetRow_InDB?.TranDesc;
        }

        /// <summary>
        /// Sets the TimeRegister depending on <c>Setup.RequireTimeApprovalToInvoice</c> and ActualTime.
        /// </summary>
        private void SetTimeRegister(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow, PXDBOperation operation)
        {
            bool? timeRegisteredNewValue = true;

            if (fsSrvOrdTypeRow.RequireTimeApprovalToInvoice == true
                    && operation == PXDBOperation.Update)
            {
                var result = 
                        PXSelect<FSAppointmentEmployee,
                        Where<
                            FSAppointmentEmployee.approvedTime, Equal<False>,
                            And<FSAppointmentEmployee.trackTime, Equal<True>,
                            And<FSAppointmentEmployee.type, Equal<BAccountType.employeeType>,
                            And<FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>>>>>>
                            .SelectWindowed(this, 0, 1, AppointmentSelected.Current.AppointmentID);
                                
                if (result.Count > 0 || AppointmentEmployees.Select().Count == 0)
                {
                    timeRegisteredNewValue = false;
                }
            }
            else if (fsAppointmentRow.ActualDateTimeBegin == null)
            {
                timeRegisteredNewValue = false;
            }

            fsAppointmentRow.TimeRegistered = timeRegisteredNewValue;
        }

        protected void CalculateEndTimeWithLinesDuration(PXCache cache, FSAppointment fsAppointmentRow,
                                            AppointmentDateTimeHelper.DateFieldType dateFieldType,
                                            bool forceUpdate = false)
        {
            if (forceUpdate == false
                    && (AppointmentRecords.Current == null || AppointmentRecords.Current.isBeingCloned == true)
            )
            {
                return;
            }

            bool handleTimeManually = (bool)fsAppointmentRow.HandleManuallyScheduleTime;
            DateTime? dateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin;
            int duration = (int)fsAppointmentRow.EstimatedDurationTotal;

            if (dateFieldType == AppointmentDateTimeHelper.DateFieldType.ActualField)
            {
                handleTimeManually = (bool)fsAppointmentRow.HandleManuallyActualTime;
                dateTimeBegin = fsAppointmentRow.ActualDateTimeBegin;
                duration = (int)fsAppointmentRow.ActualDurationTotal;
            }

            if (dateTimeBegin != null
                    && (forceUpdate == true || handleTimeManually == false)
            )
            {
                if (duration <= 0)
                {
                    duration = AppointmentDateTimeHelper.DEFAULT_DURATION_MINUTES;
                }

                DateTime? dateTimeEnd = dateTimeBegin.Value.AddMinutes(duration);

                bool originalFlag = SkipManualTimeFlagUpdate;

                try
                {
                    SkipManualTimeFlagUpdate = true;

                    if (dateFieldType == AppointmentDateTimeHelper.DateFieldType.ActualField)
                    {
                        cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeEnd>(fsAppointmentRow, dateTimeEnd);
                        cache.SetValuePending(fsAppointmentRow, typeof(FSAppointment.actualDateTimeEnd).Name, PXCache.NotSetValue);
                        cache.SetValuePending(fsAppointmentRow, typeof(FSAppointment.actualDateTimeEnd).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX, PXCache.NotSetValue);
                    }
                    else
                    {
                        cache.SetValueExtIfDifferent<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow, dateTimeEnd);
                        cache.SetValuePending(fsAppointmentRow, typeof(FSAppointment.scheduledDateTimeEnd).Name, PXCache.NotSetValue);
                        cache.SetValuePending(fsAppointmentRow, typeof(FSAppointment.scheduledDateTimeEnd).Name + PXDBDateAndTimeAttribute.TIME_FIELD_POSTFIX, PXCache.NotSetValue);
                    }
                }
                finally
                {
                    SkipManualTimeFlagUpdate = originalFlag;
                }
            }
        }

        private bool ActualDateAndTimeValidation(FSAppointment fsAppointmentRow)
        { 
            return fsAppointmentRow.ActualDateTimeBegin != null && fsAppointmentRow.ActualDateTimeEnd != null;
        }

        private bool CanTheCustomerBeEmailed() 
        {
            if (ServiceOrderRelated.Current != null)
            {
                Customer customerRow = 
                                        PXSelect<Customer, 
                                        Where<
                                            Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
                                        .Select(this, ServiceOrderRelated.Current.CustomerID);
                
                if (customerRow != null)
                {
                    FSxCustomer fsxCustomerRow = PXCache<Customer>.GetExtension<FSxCustomer>(customerRow);
                    return (bool)fsxCustomerRow.SendAppNotification;
                }
            }

            return false;
        }

        private void EnableDisable_MobileActions(PXCache cache, FSAppointment fsAppointmentRow, object row)
        {
            if (fsAppointmentRow == null || row == null)
            {
                return;
            }

            bool enableStartAction = false;
            bool enableAction = fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS || fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED;

            if (IsMobile == true && fsAppointmentRow.Hold == false)
            {
                if (cache.GetStatus(row) != PXEntryStatus.Inserted)
                {
                    if (row is FSAppointmentDet)
                    {
                        FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;

                        enableStartAction = fsAppointmentDetRow.ActualDateTimeBegin == null;

                        enableAction = enableAction
                                            && fsAppointmentDetRow.StaffRelatedCount < 2
                                                    &&  (fsAppointmentDetRow.LineType == ID.LineType_All.SERVICE ||
                                                fsAppointmentDetRow.LineType == ID.LineType_All.NONSTOCKITEM);
                    }
                    else if (row is FSAppointmentEmployee)
                    {
                        FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)row;

                        enableStartAction = fsAppointmentEmployeeRow.ActualDateTimeBegin == null;
                    }
                }

                if (row is FSAppointmentDet)
                {
                    FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)row;

                    startService.SetEnabled(enableAction && enableStartAction);
                    startService.SetVisible(enableAction && enableStartAction);
                    completeService.SetEnabled(enableAction && !enableStartAction);
                    completeService.SetVisible(enableAction && !enableStartAction);
        }
                else if (row is FSAppointmentEmployee)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)row;

                    startTimeStaff.SetEnabled(enableAction && enableStartAction);
                    startTimeStaff.SetVisible(enableAction && enableStartAction);
                    completeTimeStaff.SetEnabled(enableAction && !enableStartAction);
                    completeTimeStaff.SetVisible(enableAction && !enableStartAction);
                }
            }
        }

        #region EnableDisable

        private void EnableDisable_ActionButtons(PXCache cache, FSAppointment fsAppointmentRow,
                                                    FSServiceOrder fsServiceOrderRow, FSSrvOrdType fsSrvOrdTypeRow, FSBillingCycle fsBillingCycleRow)
        {
            if (cache.GetStatus(fsAppointmentRow) == PXEntryStatus.Inserted)
            {
                startAppointment.SetEnabled(false);
                completeAppointment.SetEnabled(false);
                cancelAppointment.SetEnabled(false);
                reopenAppointment.SetEnabled(false);
                closeAppointment.SetEnabled(false);
                uncloseAppointment.SetEnabled(false);
                invoiceAppointment.SetEnabled(false);
                cloneAppointment.SetEnabled(false);
                emailConfirmationToCustomer.SetEnabled(false);
                emailConfirmationToStaffMember.SetEnabled(false);
                emailConfirmationToGeoZoneStaff.SetEnabled(false);
                emailSignedAppointment.SetEnabled(false);
                startService.SetEnabled(false);
                completeService.SetEnabled(false);
                startTimeStaff.SetEnabled(false);
                completeTimeStaff.SetEnabled(false);
                openUserCalendar.SetEnabled(false);
                openEmployeeBoard.SetEnabled(false);
                openRoomBoard.SetEnabled(false);
            }
            else
            {
                string dummyErrorMessage = string.Empty;
                bool isOnHold = fsAppointmentRow.Hold == true;

                bool canStart    = AppointmentCore.CheckNewAppointmentStatus(this, fsAppointmentRow, fsServiceOrderRow, ID.Status_Appointment.IN_PROCESS, ref dummyErrorMessage);
                bool canComplete = AppointmentCore.CheckNewAppointmentStatus(this, fsAppointmentRow, fsServiceOrderRow, ID.Status_Appointment.COMPLETED, ref dummyErrorMessage);
                bool canCancel   = AppointmentCore.CheckNewAppointmentStatus(this, fsAppointmentRow, fsServiceOrderRow, ID.Status_Appointment.CANCELED, ref dummyErrorMessage);
                bool canReopen   = AppointmentCore.CheckNewAppointmentStatus(this, fsAppointmentRow, fsServiceOrderRow, ID.Status_Appointment.MANUAL_SCHEDULED, ref dummyErrorMessage);
                bool canClose    = AppointmentCore.CheckNewAppointmentStatus(this, fsAppointmentRow, fsServiceOrderRow, ID.Status_Appointment.CLOSED, ref dummyErrorMessage);                

                bool enableQuickProcessMobile = (ServiceOrderTypeSelected.Current == null ? false : ServiceOrderTypeSelected.Current.AllowQuickProcess.Value)
                                                    && fsAppointmentRow.PendingAPARSOPost.Value
                                                        && BillingCycleRelated.Current?.BillingBy == ID.Billing_By.APPOINTMENT
                                                            && fsAppointmentRow.Hold == false
                                                                && this.IsMobile == true
                                                                    && (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED || fsAppointmentRow.Status == ID.Status_Appointment.CLOSED);

                startAppointment.SetEnabled(canStart && isOnHold == false);
                completeAppointment.SetEnabled(canComplete && isOnHold == false);
                cancelAppointment.SetEnabled(canCancel);
                reopenAppointment.SetEnabled(canReopen);
                closeAppointment.SetEnabled(canClose && isOnHold == false);
                cloneAppointment.SetEnabled(!SharedFunctions.IsServiceOrderClosed(ServiceOrderRelated.Current));
                quickProcessMobile.SetEnabled(enableQuickProcessMobile);
                quickProcessMobile.SetVisible(enableQuickProcessMobile);

                openUserCalendar.SetEnabled(canStart && isOnHold == false);
                openEmployeeBoard.SetEnabled(canStart && isOnHold == false);
                openRoomBoard.SetEnabled(canStart && isOnHold == false);

                bool enableUncloseAction = fsAppointmentRow.Status == ID.Status_Appointment.CLOSED
                                                && fsAppointmentRow.PostingStatusAPARSO != ID.Status_Posting.POSTED
                                                    && fsAppointmentRow.PostingStatusIN != ID.Status_Posting.POSTED;

                uncloseAppointment.SetEnabled(enableUncloseAction);
                invoiceAppointment.SetEnabled(AppointmentCore.IsAppointmentReadyToBeInvoiced(fsAppointmentRow, fsServiceOrderRow, fsBillingCycleRow, fsSrvOrdTypeRow));

                //Checking if there is at least one employee
                bool atLeastOneEmployee = AppointmentEmployees.SelectWindowed(0, 1).Count > 0;
                emailConfirmationToCustomer.SetEnabled(atLeastOneEmployee && fsAppointmentRow.Confirmed == true && CanTheCustomerBeEmailed() && isOnHold == false);
                emailSignedAppointment.SetEnabled(atLeastOneEmployee && fsAppointmentRow.CustomerSignedReport != null && CanTheCustomerBeEmailed() && isOnHold == false);
                emailConfirmationToStaffMember.SetEnabled(atLeastOneEmployee && fsAppointmentRow.Confirmed == true && isOnHold == false);
                emailConfirmationToGeoZoneStaff.SetEnabled((canStart || canComplete) && atLeastOneEmployee == false && isOnHold == false); //Disable Action if there are records in the Employee Grid

                startService.SetEnabled(isOnHold == false && fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS);
                completeService.SetEnabled(isOnHold == false && fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS);
                startTimeStaff.SetEnabled(isOnHold == false && fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS);
                completeTimeStaff.SetEnabled(isOnHold == false && fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS);
            }

            bool enableEmployeeSelector, enableServiceSelector;
            bool isSrvOrdTypeActive = fsSrvOrdTypeRow?.Active == true;

            enableEmployeeSelector = enableServiceSelector
                = fsAppointmentRow.Status != ID.Status_Appointment.CANCELED
                    && fsAppointmentRow.Status != ID.Status_Appointment.CLOSED
                     && isSrvOrdTypeActive;

            if (fsSrvOrdTypeRow != null 
                    && fsSrvOrdTypeRow.Behavior != ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
                enableServiceSelector = enableServiceSelector
                                                && (fsServiceOrderRow != null && fsServiceOrderRow.CustomerID != null);
            }

            openServiceSelector.SetEnabled(enableServiceSelector);

            openStaffSelectorFromServiceTab.SetEnabled(enableEmployeeSelector);
            openStaffSelectorFromStaffTab.SetEnabled(enableEmployeeSelector);

            FSAddress fsAddressRow = ServiceOrder_Address.Select();
            if (fsServiceOrderRow != null)
            {
                bool enableValidateAddress = fsAddressRow.CountryID != null
                                           && fsAddressRow.City != null
                                               && fsAddressRow.State != null
                                                   && fsAddressRow.PostalCode != null
                                                       && fsAddressRow.IsValidated == false;
                validateAddress.SetEnabled(enableValidateAddress);
            }
        }

        /// <summary>
        /// Check the ManageRooms value on Setup to check/hide the Rooms Values options.
        /// </summary>
        private void HideRooms(FSAppointment fsAppointmentRow, FSSetup fSSetupRow)
        {
            bool isRoomManagementActive = ServiceManagementSetup.IsRoomManagementActive(this, fSSetupRow);
            fsAppointmentRow.Mem_ShowAttendees = (bool)ServiceManagementSetup.IsAttendeesManagementActive(this, fSSetupRow);
            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.SelectSingle();
            PXUIFieldAttribute.SetVisible<FSServiceOrder.roomID>(this.ServiceOrderRelated.Cache, fsServiceOrderRow, isRoomManagementActive);
            openRoomBoard.SetVisible(isRoomManagementActive);
        }

        /// <summary>
        /// Enable/Disable the Tabs if the selected Service Order Type post to AR (Parts Tab) and if the customerID is null or not (Services and Parts).
        /// </summary>
        private void EnableDisableAppointmentDetTabs(FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow != null)
            {
                bool enablePartsTab = fsServiceOrderRow.CustomerID != null && ServiceOrderTypeSelected.Current.PostTo != ID.SrvOrdType_PostTo.ACCOUNTS_RECEIVABLE_MODULE;
                bool enableServicesTab = fsServiceOrderRow.CustomerID != null;

                AppointmentDetParts.Cache.AllowInsert = enablePartsTab;
                AppointmentDetParts.Cache.AllowUpdate = enablePartsTab;
                AppointmentDetParts.Cache.AllowDelete = enablePartsTab;
                AppointmentDetServices.Cache.AllowInsert = enableServicesTab;
                AppointmentDetServices.Cache.AllowUpdate = enableServicesTab;
                AppointmentDetServices.Cache.AllowDelete = enableServicesTab;
            }
        }

        private void EnableDisableDocumentByWorkflowStage(PXCache cache, FSWFStage fsWStageRow)
        {
            List<PXCache> caches = new List<PXCache>();
            Dictionary<string, PXAction> headerActions = new Dictionary<string, PXAction>();
            List<PXAction> detailsActions = new List<PXAction>();
            List<PXView> views = new List<PXView>();

            caches.Add(cache);

            headerActions.Add(typeof(FSWFStage.allowComplete).Name, this.completeAppointment);
            headerActions.Add(typeof(FSWFStage.allowClose).Name, this.closeAppointment);
            headerActions.Add(typeof(FSWFStage.allowCancel).Name, this.cancelAppointment);
            headerActions.Add(typeof(FSWFStage.allowReopen).Name, this.reopenAppointment);

            detailsActions.Add(openServiceSelector);
            detailsActions.Add(openStaffSelectorFromServiceTab);
            detailsActions.Add(openStaffSelectorFromStaffTab);
            detailsActions.Add(createPrepayment);

            views.Add(AppointmentDetServices.View);
            views.Add(AppointmentDetParts.View);
            views.Add(AppointmentEmployees.View);
            views.Add(AppointmentResources.View);
            views.Add(PickupDeliveryItems.View);
            views.Add(ServiceOrderRelated.View);
            views.Add(AppointmentSelected.View);
            views.Add(Adjustments.View);
            views.Add(Taxes.View);
            views.Add(ServiceOrder_Contact.View);
            views.Add(ServiceOrder_Address.View);

            TreeWFStageHelper.EnableDisableDocumentByWorkflowStage(fsWStageRow, caches, headerActions, detailsActions, views);
        }
        #endregion

        private void SetServiceOrderStatusFromAppointment(FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow)
        {
            if (isTheLastestActiveAppointmentInServicesOrder(fsServiceOrderRow, fsAppointmentRow, fsAppointmentRow.Status == ID.Status_Appointment.CLOSED))
            {
                ServiceOrderEntry graphServiceOrderEntry = GetLocalServiceOrderEntryGraph(true);

                graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                            .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

                switch (fsAppointmentRow.Status)
                {
                    case ID.Status_Appointment.COMPLETED:
                        if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.OPEN)
                        {
                            graphServiceOrderEntry.CompleteOrder();
                        }

                        if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.CLOSED)
                        {
                            graphServiceOrderEntry.UncloseOrderWithOptions(false);
                        }

                        break;
                    case ID.Status_Appointment.CLOSED:
                        if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED
                                && this.SkipCallSOAction == false)
                        {
                            graphServiceOrderEntry.CloseOrder();
                        }

                        break;
                    case ID.Status_Appointment.MANUAL_SCHEDULED:
                        if (fsServiceOrderRow.Status == ID.Status_ServiceOrder.CANCELED
                            || fsServiceOrderRow.Status == ID.Status_ServiceOrder.COMPLETED)
                        {
                            graphServiceOrderEntry.ReopenOrder();
                        }

                        break;
                }
            }
        }

        private void setLatestServiceOrderStatusBaseOnAppointmentStatus(FSServiceOrder fsServiceOrderRow, string lastestServiceOrderStatus)
        {
            ServiceOrderEntry graphServiceOrderEntry = GetLocalServiceOrderEntryGraph(true);

            graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                        .Search<FSServiceOrder.refNbr>(fsServiceOrderRow.RefNbr, fsServiceOrderRow.SrvOrdType);

            switch (lastestServiceOrderStatus)
            {
                case ID.Status_Appointment.CANCELED:
                    graphServiceOrderEntry.CancelOrder();
                    break;
                case ID.Status_Appointment.COMPLETED:
                        graphServiceOrderEntry.CompleteOrder();
                    break;
                case ID.Status_Appointment.MANUAL_SCHEDULED:
                    graphServiceOrderEntry.ReopenOrder();
                    break;
                case ID.Status_Appointment.CLOSED:
                    if (this.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT))
                    {
                        graphServiceOrderEntry.CloseOrder();
                    }
                    break;
            }
        }

        private bool isTheLastestActiveAppointmentInServicesOrder(FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow, bool includeCompleteStatus = false)
        {
            if (ServiceOrderTypeSelected.Current.CompleteSrvOrdWhenSrvDone == false)
            {
                return false;
            }

            BqlCommand appointmentView = new Select<FSAppointment,
                                                    Where<
                                                        FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>,
                                                        And<
                                                            FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>>();
            List<object> args = new List<object>();
            List<string> appointmentStatusesList = new List<string>();

            args.Add(fsAppointmentRow.AppointmentID);
            args.Add(fsAppointmentRow.SOID);

            appointmentStatusesList.Add(ID.Status_Appointment.AUTOMATIC_SCHEDULED);
            appointmentStatusesList.Add(ID.Status_Appointment.MANUAL_SCHEDULED);
            appointmentStatusesList.Add(ID.Status_Appointment.IN_PROCESS);

            if (includeCompleteStatus == true)
            {
                appointmentStatusesList.Add(ID.Status_Appointment.COMPLETED);
            }

            if (appointmentStatusesList.Count > 0)
            {
                appointmentView = appointmentView.WhereAnd(InHelper<FSAppointment.status>.Create(appointmentStatusesList.Count));
            }

            foreach (string appointmentStatus in appointmentStatusesList)
            {
                args.Add(appointmentStatus);
            }

            PXView appointmentEmployeeSkillView = new PXView(this, true, appointmentView);

            var fsAppointmentRow_tmp = appointmentEmployeeSkillView.SelectMulti(args.ToArray());

            if (fsAppointmentRow_tmp != null && fsAppointmentRow_tmp.Count > 0)
            {
                return false;
            }

            return true;
        }

        private WebDialogResult ShowAsk(PXView view, string wdTitle, string wdMessage, MessageButtons mbOption)
        {
            WebDialogResult wdr = WebDialogResult.Yes;

            if (this.SkipCallSOAction == false)
            {
                wdr = view.Ask(wdTitle, wdMessage, mbOption);
            }

            return wdr;
        }

        private string getFinalServiceOrderStatus(FSServiceOrder fsServiceOrderRow, FSAppointment fsAppointmentRow, string finalAppointmentStatus)
        {
            bool lastAppointment = false;

            lastAppointment = isTheLastestActiveAppointmentInServicesOrder(fsServiceOrderRow, fsAppointmentRow);

            if (lastAppointment == true)
            {
                if (finalAppointmentStatus == ID.Status_Appointment.CANCELED)
                {
                    var fsAppointmentRow_tmp = PXSelect<FSAppointment,
                            Where<
                                FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>,
                                And<
                                    FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                And<
                                    Where<
                                        FSAppointment.status, NotEqual<FSAppointment.status.Canceled>>>>>>
                            .SelectWindowed(this, 0, 1, fsAppointmentRow.AppointmentID, fsAppointmentRow.SOID);

                    if (fsAppointmentRow_tmp.Count == 0 
                            && WebDialogResult.Yes == ShowAsk(AppointmentRecords.View, 
                                                                                    TX.WebDialogTitles.CONFIRM_SERVICE_ORDER_CANCELING,
                                                                                    TX.Messages.ASK_CONFIRM_SERVICE_ORDER_CANCELING,
                                                                                    MessageButtons.YesNo))
                    {
                        return ID.Status_ServiceOrder.CANCELED;
                    }
                }

                if (fsServiceOrderRow.Status != ID.Status_ServiceOrder.COMPLETED
                     && fsServiceOrderRow.Status != ID.Status_ServiceOrder.CLOSED
                     && WebDialogResult.Yes == ShowAsk(AppointmentRecords.View,
                                                 TX.WebDialogTitles.CONFIRM_SERVICE_ORDER_COMPLETING,
                                                 TX.Messages.ASK_CONFIRM_SERVICE_ORDER_COMPLETING,
                                                 MessageButtons.YesNo))
                {
                    return ID.Status_ServiceOrder.COMPLETED;
                }
            }

            return string.Empty;
        }

        private void UpdateAttributes(FSAppointment fsAppointmentRow)
        {
            bool updateAnswers = false;

            if (AttributeListRecords.Current == null
                    && fsAppointmentRow != null
                        && fsAppointmentRow.AppointmentID < 0)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }
            else if (fsAppointmentRow.AppointmentID > 0
                        && AttributeListRecords.Current == null)
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }
            else if (AttributeListRecords.Current != null
                        && (AttributeListRecords.Current.AppointmentID != fsAppointmentRow.AppointmentID
                            || AttributeListRecords.Current.SOID != fsAppointmentRow.SOID
                                || AttributeListRecords.Current.SrvOrdType != fsAppointmentRow.SrvOrdType))
            {
                AttributeListRecords.Current = AttributeListRecords.Select();
                updateAnswers = true;
            }

            if (updateAnswers == true  && fsAppointmentRow.CreatedByScreenID != ID.ScreenID.APPOINTMENT)
            {
                Answers.Select();
            }
        }

        private void CheckScheduledDateTimes(
                                            PXCache cache, 
                                            FSAppointment fsAppointmentRow,
                                            PXDBOperation operation)
        {
            if (SetupRecord.Current.AppointmentInPast == false
                    && operation == PXDBOperation.Insert && fsAppointmentRow.GeneratedBySystem == false)
            {
                if (fsAppointmentRow.ScheduledDateTimeBegin < DateTime.Now.AddMinutes(-5))
                {
                    cache.RaiseExceptionHandling
                        <FSAppointment.scheduledDateTimeBegin>(
                                                            fsAppointmentRow,
                                                            fsAppointmentRow.ScheduledDateTimeBegin,
                                                            new PXSetPropertyException(
                                                                TX.Error.START_DATE_LESSER_THAN_PRESENT,
                                                                PXErrorLevel.Error));
                    return;
                }
                else
                {
                    cache.RaiseExceptionHandling
                        <FSAppointment.scheduledDateTimeBegin>(
                                                            fsAppointmentRow,
                                                            fsAppointmentRow.ScheduledDateTimeBegin,
                                                            null);
                }
            }

            if (fsAppointmentRow.ScheduledDateTimeBegin == null
                || fsAppointmentRow.ScheduledDateTimeEnd == null
                    || fsAppointmentRow.HandleManuallyScheduleTime == false)
            {
                return;
            }

            PXSetPropertyException exception = CheckDateTimes(
                                                            (DateTime)fsAppointmentRow.ScheduledDateTimeBegin,
                                                            (DateTime)fsAppointmentRow.ScheduledDateTimeEnd,
                                                            true);
            if (exception != null)
            {
                cache.RaiseExceptionHandling
                        <FSAppointment.scheduledDateTimeBegin>(
                                                            fsAppointmentRow,
                                                            fsAppointmentRow.ScheduledDateTimeBegin,
                                                            exception);
                cache.RaiseExceptionHandling
                    <FSAppointment.scheduledDateTimeEnd>(
                                                        fsAppointmentRow,
                                                        fsAppointmentRow.ScheduledDateTimeEnd,
                                                        exception);
            }
            else
            {
                cache.RaiseExceptionHandling
                        <FSAppointment.scheduledDateTimeBegin>(
                                                            fsAppointmentRow,
                                                            fsAppointmentRow.ScheduledDateTimeBegin,
                                                            null);
                cache.RaiseExceptionHandling
                    <FSAppointment.scheduledDateTimeEnd>(
                                                        fsAppointmentRow,
                                                        fsAppointmentRow.ScheduledDateTimeEnd,
                                                        null);
            }
        }

        private PXSetPropertyException CheckDateTimes(
                                                    DateTime actualDateTimeBegin, 
                                                    DateTime actualDateTimeEnd,
                                                    bool isScheduled)
        {
            PXSetPropertyException exception = null;
            DateTime auxActualDateTimeEnd = new DateTime(
                                                actualDateTimeBegin.Year,
                                                actualDateTimeBegin.Month,
                                                actualDateTimeBegin.Day,
                                                actualDateTimeEnd.Hour,
                                                actualDateTimeEnd.Minute,
                                                actualDateTimeEnd.Second);

            if (actualDateTimeBegin.Hour == actualDateTimeEnd.Hour
                    && actualDateTimeBegin.Minute == actualDateTimeEnd.Minute)
            {
                if (isScheduled == true)
                {
                    exception = new PXSetPropertyException(TX.Error.INVALID_SCHEDULED_APPOINTMENT_DURATION, PXErrorLevel.RowError);
                }
                else
                {
                    exception = new PXSetPropertyException(TX.Error.INVALID_ACTUAL_APPOINTMENT_DURATION, PXErrorLevel.RowError);
                }
            }
            else if (actualDateTimeBegin > auxActualDateTimeEnd)
            {
                exception = new PXSetPropertyException(
                                                    TX.Warning.END_TIME_PRIOR_TO_START_TIME_APPOINTMENT,
                                                    PXErrorLevel.Warning);
            }

            return exception;
        }

        private void CheckActualDateTimes(
                                        PXCache cache,
                                        FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow.ActualDateTimeBegin == null
                || fsAppointmentRow.ActualDateTimeEnd == null)
            {
                return;
            }

            PXSetPropertyException exception = CheckDateTimes(
                                                            (DateTime)fsAppointmentRow.ActualDateTimeBegin, 
                                                            (DateTime)fsAppointmentRow.ActualDateTimeEnd,
                                                            false);

            PXFieldState actualDateTimeBeginField = (PXFieldState)cache.GetStateExt<FSAppointment.actualDateTimeBegin>(fsAppointmentRow);
            if (actualDateTimeBeginField.Error == null)
            {
            cache.RaiseExceptionHandling<FSAppointment.actualDateTimeBegin>(fsAppointmentRow, fsAppointmentRow.ActualDateTimeBegin, exception);
            }

            PXFieldState actualDateTimeEndField = (PXFieldState)cache.GetStateExt<FSAppointment.actualDateTimeEnd>(fsAppointmentRow);
            if (actualDateTimeEndField.Error == null)
            {
            cache.RaiseExceptionHandling<FSAppointment.actualDateTimeEnd>(fsAppointmentRow, fsAppointmentRow.ActualDateTimeEnd, exception);
        }
        }

        /// <summary>
        /// Raises a warning to notify the changes in ActualEndTime and ScheduleEndTime.
        /// </summary>
        private void TurnOnKeepTimeFlagsAndNotifyEndTimesChange(PXCache cache, FSAppointment fsAppointmentRow, bool notifySchedule = true, bool notifyActual = true)
        {
            if (notifySchedule == true
                    && fsAppointmentRow.ScheduledDateTimeBegin != null
                        && fsAppointmentRow.ScheduledDateTimeEnd != null
                            && fsAppointmentRow.HandleManuallyScheduleTime == false)
            {
                cache.RaiseExceptionHandling
                                <FSAppointment.scheduledDateTimeEnd>(
                                                                    fsAppointmentRow,
                                                                    fsAppointmentRow.ScheduledDateTimeEnd,
                                                                    new PXSetPropertyException(
                                                                        PXMessages.LocalizeFormatNoPrefix(TX.Warning.END_TIME_AUTOMATICALLY_CALCULATED_NOTIFICATION, TX.AppointmentTotalTimesLabels.ESTIMATED),
                                                                        PXErrorLevel.Warning));
            }

            if (notifyActual == true
                    && fsAppointmentRow.ActualDateTimeBegin != null 
                        && fsAppointmentRow.ActualDateTimeEnd != null
                            && fsAppointmentRow.HandleManuallyActualTime == false)
            {
                cache.RaiseExceptionHandling
                                <FSAppointment.actualDateTimeEnd>(
                                                                fsAppointmentRow,
                                                                fsAppointmentRow.ActualDateTimeEnd,
                                                                new PXSetPropertyException(
                                                                    PXMessages.LocalizeFormatNoPrefix(TX.Warning.END_TIME_AUTOMATICALLY_CALCULATED_NOTIFICATION, TX.AppointmentTotalTimesLabels.ACTUAL),
                                                                    PXErrorLevel.Warning));
            }
        }

        private void AutoConfirm(FSAppointment fsAppointmentRow)
        {
            if (SetupRecord.Current != null
                    && SetupRecord.Current.AppAutoConfirmGap != null)
            {
                TimeSpan? diffTimeToStart = fsAppointmentRow.ScheduledDateTimeBegin - PXTimeZoneInfo.Now;

                if (diffTimeToStart.Value.TotalMinutes <= SetupRecord.Current.AppAutoConfirmGap)
                {
                    fsAppointmentRow.Confirmed = true;
                }
            }
        }

        /// <summary>
        /// Validates if the required information in the Signature tab is complete.
        /// </summary>
        /// <param name="cache">PXCache instance.</param>
        /// <param name="fsAppointmentRow">Current FSAppointment object.</param>
        /// <param name="mustValidateSignature">Indicates if the validation process will be applied.</param>
        private void ValidateSignatureFields(PXCache cache, FSAppointment fsAppointmentRow, bool mustValidateSignature)
        {
            if (mustValidateSignature 
                && (fsAppointmentRow.CustomerSignedReport == null
                    || fsAppointmentRow.CustomerSignedReport == Guid.Empty))
            {
                throw new PXException(TX.Error.CUSTOMER_SIGNATURE_MISSING);
            }
        }

        private void ValidateLicenses<fieldType>(PXCache currentCache, object currentRow)
            where fieldType : IBqlField
        {
            if (SetupRecord.Current.DenyWarnByLicense == ID.ValidationType.NOT_VALIDATE)
            {
                return;
            }

            var fsAppointmentDetServiceSet = AppointmentDetServices.Select();

            if (fsAppointmentDetServiceSet.Count == 0)
            {
                return;
            }

            var fsAppointmentEmployeeSet = AppointmentEmployees.Select();
            List<int?> serviceLicenseIDs = new List<int?>();
            List<int?> employeeLicenseIDs = GetAppointmentEmpoyeeLicenseIDs(fsAppointmentEmployeeSet);
            List<ServiceRequirement> serviceLicensesList = GetAppointmentDetServiceRowLicenses(fsAppointmentDetServiceSet, ref serviceLicenseIDs);

            //verify if appointmentDetLicenseIDs list is a subset of employeeLicenseIDs list
            serviceLicenseIDs = serviceLicenseIDs.Distinct().ToList();
            bool isSubset = !serviceLicenseIDs.Except(employeeLicenseIDs).Any();

            if (!isSubset)
            {
                List<int?> missingLicenseIDs = serviceLicenseIDs.Except(employeeLicenseIDs).ToList();
                bool throwException = false;
                PXErrorLevel errorLevel;
                bool licenseIsContained = false;

                foreach (FSAppointmentDet fsAppointmentDetRow in fsAppointmentDetServiceSet)
                {
                    licenseIsContained = false;

                    ServiceRequirement serviceLicenses = serviceLicensesList.Where(list => list.serviceID == fsAppointmentDetRow.InventoryID).FirstOrDefault();

                    if (serviceLicenses != null)
                    {
                        licenseIsContained = missingLicenseIDs.Intersect(serviceLicenses.requirementIDList).Any();
                    }

                    if (licenseIsContained)
                    {
                        errorLevel = PXErrorLevel.Warning;

                        if (SetupRecord.Current.DenyWarnByLicense == ID.ValidationType.PREVENT)
                        {
                            throwException = true;
                            errorLevel = PXErrorLevel.RowError;
                        }

                        currentCache.RaiseExceptionHandling<fieldType>(
                                currentRow,
                                null,
                                new PXSetPropertyException(TX.Error.SERVICE_LICENSE_TYPES_REQUIREMENTS_MISSING, errorLevel));
                    }
                }

                if (throwException)
                {
                    throw new PXException(TX.Error.SERVICE_LICENSE_TYPES_REQUIREMENTS_MISSING, PXErrorLevel.Error);
                }
            }
        }

        private void ValidateSkills<fieldType>(PXCache currentCache, object currentRow)
            where fieldType : IBqlField
        {
            if (SetupRecord.Current.DenyWarnBySkill == ID.ValidationType.NOT_VALIDATE)
            {
                return;
            }

            var fsAppointmentDetServicesSet = AppointmentDetServices.Select();

            if (fsAppointmentDetServicesSet.Count == 0)
            {
                return;
            }

            var fsAppointmentEmployeeSet = AppointmentEmployees.Select();
            List<object> args = new List<object>();
            List<int?> serviceSkillIDs  = new List<int?>();

            List<int?> employeeSkillIDs = GetAppointmentEmpoyeeSkillIDs(fsAppointmentEmployeeSet);
            List<ServiceRequirement> serviceSkillsList = GetAppointmentDetServiceRowSkills(fsAppointmentDetServicesSet, ref serviceSkillIDs);

            //verify if appointmentDetSkillIDs list is a subset of employeeSkillIDs list
            serviceSkillIDs = serviceSkillIDs.Distinct().ToList();
            bool isSubset   = !serviceSkillIDs.Except(employeeSkillIDs).Any();

            if (!isSubset)
            {
                List<int?> missingSkillIDs = serviceSkillIDs.Except(employeeSkillIDs).ToList();
                bool throwException   = false;
                PXErrorLevel errorLevel;
                bool skillIsContained = false;

                foreach (FSAppointmentDet fsAppointmentDetRow in fsAppointmentDetServicesSet)
                {
                    skillIsContained = false;

                    ServiceRequirement serviceSkills = serviceSkillsList.Where(list => list.serviceID == fsAppointmentDetRow.InventoryID).FirstOrDefault();

                    if (serviceSkills != null)
                    {
                        skillIsContained = missingSkillIDs.Intersect(serviceSkills.requirementIDList).Any();
                    }

                    if (skillIsContained)
                    {
                        errorLevel = PXErrorLevel.Warning;

                        if (SetupRecord.Current.DenyWarnBySkill == ID.ValidationType.PREVENT)
                        {
                            throwException = true;
                            errorLevel = PXErrorLevel.RowError;
                        }

                        currentCache.RaiseExceptionHandling<fieldType>(
                                currentRow,
                                null,
                                new PXSetPropertyException(TX.Error.SERVICE_SKILL_REQUIREMENTS_MISSING_GENERAL, errorLevel));
                    }
                }

                if (throwException)
                {
                    throw new PXException(TX.Error.SERVICE_SKILL_REQUIREMENTS_MISSING_GENERAL, PXErrorLevel.Error);
                }
            }       
        }

        private void ValidateGeoZones<fieldType>(PXCache currentCache, object currentRow)
            where fieldType : IBqlField
        {
            if (SetupRecord.Current.DenyWarnByGeoZone == ID.ValidationType.NOT_VALIDATE)
            {
                return;
            }

            FSAddress fsAddressRow = ServiceOrder_Address.Select();

            if (fsAddressRow == null 
                    || string.IsNullOrEmpty(fsAddressRow.PostalCode) == true)
            {
                return;
            }

            var fsAppointmentEmployeeSet = AppointmentEmployees.Select();
            bool throwException = false;
            PXErrorLevel errorLevel;
            List<object> args = new List<object>();
            List<int?> employeeIDList = new List<int?>();
            List<int?> employeesBelongingToTheGeozone = new List<int?>();

            BqlCommand fsGeoZoneEmpBql = new Select2<FSGeoZoneEmp,
                                             InnerJoin<FSGeoZonePostalCode,
                                                On<FSGeoZonePostalCode.geoZoneID, Equal<FSGeoZoneEmp.geoZoneID>>>>();

            fsGeoZoneEmpBql = fsGeoZoneEmpBql.WhereAnd(InHelper<FSGeoZoneEmp.employeeID>.Create(fsAppointmentEmployeeSet.Count));
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeSet)
            {
                args.Add(fsAppointmentEmployeeRow.EmployeeID);
                employeeIDList.Add(fsAppointmentEmployeeRow.EmployeeID);
            }

            PXView fsGeoZoneEmpView = new PXView(this, true, fsGeoZoneEmpBql);
            var fsGeoZoneEmpSet = fsGeoZoneEmpView.SelectMulti(args.ToArray());

            foreach (PXResult<FSGeoZoneEmp, FSGeoZonePostalCode> bqlResult in fsGeoZoneEmpSet)
            {
                FSGeoZoneEmp fsGeoZoneEmpRow = (FSGeoZoneEmp)bqlResult;
                FSGeoZonePostalCode fsGeoZonePostalCodeRow = (FSGeoZonePostalCode)bqlResult;

                if (Regex.Match(fsAddressRow.PostalCode.Trim(), @fsGeoZonePostalCodeRow.PostalCode.Trim()).Success)
                {
                    employeesBelongingToTheGeozone.Add((int?)fsGeoZoneEmpRow.EmployeeID);
                }
            }

            List<int?> employeesMissingFromGeozone = employeeIDList.Except(employeesBelongingToTheGeozone).ToList();

            if (employeesMissingFromGeozone.Count > 0)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeSet)
                {
                    if (employeesMissingFromGeozone.IndexOf(fsAppointmentEmployeeRow.EmployeeID) != -1)
                    {
                        errorLevel = PXErrorLevel.Warning;

                        if (SetupRecord.Current.DenyWarnByGeoZone == ID.ValidationType.PREVENT)
                        {
                            throwException = true;
                            errorLevel = PXErrorLevel.RowError;
                        }

                        currentCache.RaiseExceptionHandling<fieldType>(
                                currentRow,
                                null,
                                new PXSetPropertyException(TX.Error.APPOINTMENT_EMPLOYEE_MISMATCH_GEOZONE, errorLevel));
                    }
                }

                if (throwException)
                {
                    throw new PXException(TX.Error.APPOINTMENT_EMPLOYEE_MISMATCH_GEOZONE, PXErrorLevel.Error);
                }
            }
        }

        private void ClearEmployeesGrid()
        {
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in AppointmentEmployees.Select())
            {
                AppointmentEmployees.Delete(fsAppointmentEmployeeRow);
            }
        }

        public void GetEmployeesFromServiceOrder(FSAppointment fsAppointmentRow)
        {
            ClearEmployeesGrid();

            var fsSOEmployeeSet = PXSelectJoin<FSSOEmployee,
                                  InnerJoin<FSServiceOrder,
                                    On<
                                        FSServiceOrder.sOID, Equal<FSSOEmployee.sOID>>>,
                                  Where<
                                        FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                  .Select(this, fsAppointmentRow.SOID);

            foreach (FSSOEmployee fsSOEmployeeRow in fsSOEmployeeSet)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = new FSAppointmentEmployee();

                fsAppointmentEmployeeRow.EmployeeID = fsSOEmployeeRow.EmployeeID;
                fsAppointmentEmployeeRow.ServiceLineRef = fsSOEmployeeRow.ServiceLineRef;
                fsAppointmentEmployeeRow.Comment = fsSOEmployeeRow.Comment;

                AppointmentEmployees.Insert(fsAppointmentEmployeeRow);
            }
        }

        private void ClearResourceGrid()
        {
            foreach (FSAppointmentResource fsAppointmentResourceRow in AppointmentResources.Select())
            {
                AppointmentResources.Delete(fsAppointmentResourceRow);
            }
        }

        public void ClearAttendeeGrid()
        {
            foreach (FSAppointmentAttendee fsAppointmentAttendeeRow in AppointmentAttendees.Select())
            {
                AppointmentAttendees.Delete(fsAppointmentAttendeeRow);
            }
        }

        private void ClearPrepayment(FSAppointment fsAppointmentRow)
        {
            ARPaymentEntry graphARPaymentEntry = PXGraph.CreateInstance<ARPaymentEntry>();
            SM_ARPaymentEntry graphSM_ARPaymentEntry = graphARPaymentEntry.GetExtension<SM_ARPaymentEntry>();

            foreach (FSAdjust fsAdjustRow in PXSelect<FSAdjust,
                                        Where<FSAdjust.adjdOrderType, Equal<Required<FSAdjust.adjdOrderType>>,
                                            And<FSAdjust.adjdOrderNbr, Equal<Required<FSAdjust.adjdOrderNbr>>,
                                            And<FSAdjust.adjdAppRefNbr, Equal<Required<FSAdjust.adjdAppRefNbr>>>>
                                        >>.Select(graphARPaymentEntry, fsAppointmentRow.SrvOrdType, fsAppointmentRow.SORefNbr, fsAppointmentRow.RefNbr))
            {
                graphARPaymentEntry.Document.Current = graphARPaymentEntry.Document.Search<ARPayment.refNbr>(fsAdjustRow.AdjgRefNbr, fsAdjustRow.AdjgDocType);

                if (graphARPaymentEntry.Document.Current != null)
                { 
                    fsAdjustRow.AdjdAppRefNbr = String.Empty;

                    graphSM_ARPaymentEntry.FSAdjustments.Update(fsAdjustRow);
                    graphARPaymentEntry.Save.Press();
                }
            }
        }

        private void GetResourcesFromServiceOrder(FSAppointment fsAppointmentRow)
        {
            ClearResourceGrid();

            var fsSOResourceSet = PXSelectJoin<FSSOResource,
                            InnerJoin<FSServiceOrder,
                                On<
                                    FSServiceOrder.sOID, Equal<FSSOResource.sOID>>>,
                            Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                            .Select(this, fsAppointmentRow.SOID);

            foreach (FSSOResource fsSOResourceRow in fsSOResourceSet)
            {
                FSAppointmentResource fsAppointmentResourceRow = new FSAppointmentResource();

                fsAppointmentResourceRow.SMEquipmentID = fsSOResourceRow.SMEquipmentID;
                fsAppointmentResourceRow.Comment = fsSOResourceRow.Comment;

                AppointmentResources.Insert(fsAppointmentResourceRow);
            }
        }

        public void GetAttendeesFromServiceOrder(FSAppointment fsAppointmentRow, FSSelectorHelper helper)
        {
            int? currentCustomer = helper.Mem_int;

            ClearAttendeeGrid();
            
            var fsSOAttendeeSet = PXSelectJoin<FSSOAttendee,
                            InnerJoin<FSServiceOrder,
                                On<
                                    FSServiceOrder.sOID, Equal<FSSOAttendee.sOID>>>,
                            Where<FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                            .Select(this, fsAppointmentRow.SOID);

            foreach (FSSOAttendee fsSOAttendeeRow in fsSOAttendeeSet)
            {
                FSAppointmentAttendee fsAppointmentAttendeeRow = new FSAppointmentAttendee();
                fsAppointmentAttendeeRow.CustomerID = fsSOAttendeeRow.CustomerID;
                fsAppointmentAttendeeRow.ContactID = fsSOAttendeeRow.ContactID;
                fsAppointmentAttendeeRow.Comment = fsSOAttendeeRow.Comment;

                helper.Mem_int = fsAppointmentAttendeeRow.CustomerID;
                AppointmentAttendees.Insert(fsAppointmentAttendeeRow);
            }

            helper.Mem_int = currentCustomer;
        }

        private int? getActualDurationTimeDiff(FSAppointment fsAppointmentRow)
        {
            // get and cast to Datetime the ActualDateTimeBegin & ActualDateTimeEnd
            DateTime actualDateTimeBegin = fsAppointmentRow.ActualDateTimeBegin ?? DateTime.Now;
            DateTime actualDateTimeEnd   = fsAppointmentRow.ActualDateTimeEnd   ?? DateTime.Now;

            // substract ActualDateTimeEnd - ActualDateTimeBegin
            TimeSpan diff = actualDateTimeEnd.Subtract(actualDateTimeBegin);

            // Update the field ActualDurationTotal with the difference
            return (int?)diff.TotalMinutes;
        }

        private void UncheckUnreachedCustomerByScheduledDate(DateTime? oldValue, DateTime? currentValue, FSAppointment fsAppointmentRow)
        {
            if (currentValue != oldValue)
            {
                fsAppointmentRow.UnreachedCustomer = false;
            }
        }

        private void ValidateEmployeeAvailability<fieldType>(FSAppointment fsAppointmentRow, PXCache currentCache, object currentRow)
            where fieldType : IBqlField
        {
            if (SetupRecord.Current.DenyWarnByAppOverlap == ID.ValidationType.NOT_VALIDATE)
            {
                return;
            }

            var fsAppointmentEmployeeSet = AppointmentEmployees.Select();

            if (fsAppointmentEmployeeSet.Count == 0)
            {
                return;
            }

            List<object> args = new List<object>(); 
            List<int?> employeeIDList = new List<int?>();
            List<int?> notAvailableEmployees = new List<int?>();

            BqlCommand fsAppointmentBql = new Select2<FSAppointment,
                                              InnerJoin<FSAppointmentEmployee,
                                                On<FSAppointmentEmployee.appointmentID, Equal<FSAppointment.appointmentID>>>,
                                              Where2<
                                                Where<
                                                    FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>,
                                                    And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Canceled>>>,
                                                    And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Completed>,
                                                    And<FSAppointment.status, NotEqual<ListField_Status_Appointment.Closed>,
                                                And<
                                                    Where<
                                                        FSAppointment.scheduledDateTimeEnd, Greater<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                        And<FSAppointment.scheduledDateTimeBegin, Less<Required<FSAppointment.scheduledDateTimeEnd>>>>>>>>>();
           
            args.Add(fsAppointmentRow.AppointmentID);
            args.Add(fsAppointmentRow.ScheduledDateTimeBegin);
            args.Add(fsAppointmentRow.ScheduledDateTimeEnd);

            fsAppointmentBql = fsAppointmentBql.WhereAnd(InHelper<FSAppointmentEmployee.employeeID>.Create(fsAppointmentEmployeeSet.Count));
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeSet)
            {
                args.Add(fsAppointmentEmployeeRow.EmployeeID);
                employeeIDList.Add(fsAppointmentEmployeeRow.EmployeeID);
            }

            PXView fsAppointmentView = new PXView(this, true, fsAppointmentBql);
            var fsAppointmentSet = fsAppointmentView.SelectMulti(args.ToArray());

            foreach (PXResult<FSAppointment, FSAppointmentEmployee> bqlResult in fsAppointmentSet)
            {
                FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)bqlResult;
                notAvailableEmployees.Add((int?)fsAppointmentEmployeeRow.EmployeeID);
            }

            if (fsAppointmentSet.Count > 0)
            {
                bool throwException = false;
                PXErrorLevel errorLevel;

                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeSet)
                {
                    if (notAvailableEmployees.IndexOf(fsAppointmentEmployeeRow.EmployeeID) != -1)
                    {
                        errorLevel = PXErrorLevel.Warning;

                        if (SetupRecord.Current.DenyWarnByAppOverlap == ID.ValidationType.PREVENT)
                        {
                            throwException = true;
                            errorLevel = PXErrorLevel.RowError;
                        }

                        currentCache.RaiseExceptionHandling<fieldType>(
                                currentRow,
                                null,
                                new PXSetPropertyException(TX.Error.EMPLOYEE_NOT_AVAILABLE_WITH_APPOINTMENTS, errorLevel));
                    }
                }

                if (throwException)
                {
                    throw new PXException(TX.Error.EMPLOYEE_NOT_AVAILABLE_WITH_APPOINTMENTS, PXErrorLevel.Error);
                }
            }
        }

        private void ValidateRoomAvailability(PXCache cache, FSAppointment fsAppointmentRow)
        {
            //TODO SD-6208 need to be reimplemented taking now in consideration the roomID in FSServiceOrder Table
            return;
            /*
            if (SetupRecord.Current.DenyWarnEmpAvailability == ID.ValidationType.NONE)
            {
                return;
            }
            
            // IF there are no rooms assigned to the appointment 
            // THEN return
            if (!String.IsNullOrEmpty(fsAppointmentRow.RoomID))
            {
                var fsAppointmentRows = PXSelectJoin<FSAppointment,
                                                    InnerJoin<
                                                        FSServiceOrder,
                                                            On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>
                                                    >,
                                                    Where2<
                                                         Where<
                                                             FSServiceOrder.cpnyLocationID, Equal<Required<FSServiceOrder.cpnyLocationID>>,
                                                                And<
                                                                    FSAppointment.roomID, Equal<Required<FSAppointment.roomID>>,
                                                                And<
                                                                    FSAppointment.appointmentID,NotEqual<Required<FSAppointment.appointmentID>>>>>,
                                                         And<
                                                            Where<
                                                                    FSAppointment.scheduledDateTimeBegin, Less<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                                And<
                                                                    FSAppointment.scheduledDateTimeEnd, Greater<Required<FSAppointment.scheduledDateTimeEnd>>>>>>>
                                                    .Select(this,
                                                            ServiceOrderRelated.Current.CpnyLocationID,
                                                            fsAppointmentRow.RoomID,
                                                            fsAppointmentRow.AppointmentID,
                                                            fsAppointmentRow.ScheduledDateTimeEnd,
                                                            fsAppointmentRow.ScheduledDateTimeBegin);

                if (fsAppointmentRows.Count() > 0)
                {
                    if (SetupRecord.Current.DenyWarnEmpAvailability == ID.ValidationType.DENY)
                    {
                        messages.ErrorMessages.Add(TX.Error.ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS);
                            
                        AppointmentRecords.Cache.RaiseExceptionHandling<FSAppointment.roomID>(fsAppointmentRow,
                                fsAppointmentRow.RoomID,
                                new PXSetPropertyException(TX.Error.ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS, PXErrorLevel.RowError));

                        throw new PXException(TX.Error.ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS, PXErrorLevel.Error);
                    }
                    else
                    {
                        messages.WarningMessages.Add(TX.Error.ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS);

                        AppointmentRecords.Cache.RaiseExceptionHandling<FSAppointmentEmployee.employeeID>(fsAppointmentRow,
                                fsAppointmentRow.RoomID,
                                new PXSetPropertyException(TX.Error.ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS, PXErrorLevel.Warning));
                    }
                }
            }   */
        }

        private void ValidateRoom(FSAppointment fsAppointmentRow)
        {
            FSSrvOrdType fsSrvOrdTypeRow = this.ServiceOrderTypeSelected.SelectSingle(fsAppointmentRow.SrvOrdType);
            FSServiceOrder fsServiceOrder = this.ServiceOrderRelated.Current;

            if (fsSrvOrdTypeRow.RequireRoom == true && string.IsNullOrEmpty(fsServiceOrder.RoomID))
            {
                this.ServiceOrderRelated.Cache.RaiseExceptionHandling<FSServiceOrder.roomID>(
                                                                                            fsServiceOrder,
                                                                                            fsServiceOrder.RoomID,
                                                                                            new PXSetPropertyException(TX.Error.ROOM_REQUIRED_FOR_THIS_SRVORDTYPE, PXErrorLevel.Error));
            }
        }

        /// <summary>
        /// Validates if the maximum amount of appointments it is exceed for a specific route.
        /// </summary>
        private void ValidateMaxAppointmentQty(FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow.RouteID == null)
            { 
                return; 
            }

            FSRoute fsRouteRow = PXSelect<FSRoute, 
                                    Where<
                                        FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>
                                .Select(this, fsAppointmentRow.RouteID);

            if (fsRouteRow != null 
                    && fsRouteRow.NoAppointmentLimit == false)
            {
                DateTime? appointmentScheduledDate = fsAppointmentRow.ScheduledDateTimeBegin;
                DateTime? begin = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 0, 0, 0); //12:00 AM
                DateTime? end = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 23, 59, 59); //23:59 AM

                PXResultset<FSAppointment> bqlResultSet = PXSelectReadonly<FSAppointment,
                                                                Where<
                                                                    FSAppointment.routeID, Equal<Required<FSAppointment.routeID>>, 
                                                                And<
                                                                    FSAppointment.scheduledDateTimeBegin, Between<Required<FSAppointment.scheduledDateTimeBegin>, Required<FSAppointment.scheduledDateTimeBegin>>>>>
                                                          .Select(this, fsAppointmentRow.RouteID, begin, end);

                if (bqlResultSet.Count >= fsRouteRow.MaxAppointmentQty)
                {
                    throw new PXException(TX.Error.ROUTE_MAX_APPOINTMENT_QTY_EXCEEDED, PXErrorLevel.Error);
                }
            }
        }

        /// <summary>
        /// Validates if the appointment Week Code is valid with the <c>datetime</c> of the appointment.
        /// </summary>
        private void ValidateWeekCode(FSAppointment fsAppointmentRow)
        {
            if (fsAppointmentRow.ScheduleID == null)
            {
                return;
            }

            FSSchedule fsScheduleRow = PXSelect<FSSchedule,
                                    Where<
                                        FSSchedule.scheduleID, Equal<Required<FSSchedule.scheduleID>>>>
                                .Select(this, fsAppointmentRow.ScheduleID);

            DateTime? scheduleTimeModified = new DateTime(
                                                        fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                        fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                        fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                        0,
                                                        0,
                                                        0); //12:00 AM

            if (fsScheduleRow != null && fsScheduleRow.WeekCode != null)
            {
                if (!SharedFunctions.WeekCodeIsValid(fsScheduleRow.WeekCode, scheduleTimeModified, this))
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.WEEKCODE_NOT_MATCH_WITH_SCHEDULE, fsScheduleRow.RefNbr, scheduleTimeModified.Value.ToShortDateString()), PXErrorLevel.Error);
                }
            }

            PXResult<FSScheduleRoute, FSRoute> bqlResult = (PXResult<FSScheduleRoute, FSRoute>)
                                                             PXSelectJoin<FSScheduleRoute,
                                                             InnerJoin<FSRoute, On<FSRoute.routeID, Equal<FSScheduleRoute.dfltRouteID>>>,
                                                             Where<
                                                                 FSScheduleRoute.scheduleID, Equal<Required<FSScheduleRoute.scheduleID>>>>
                                                             .Select(this, fsScheduleRow.ScheduleID);

            if (bqlResult != null)
            {
                FSRoute fsRouteRow = (FSRoute)bqlResult;
                if (fsRouteRow != null && fsRouteRow.WeekCode != null)
                {
                    if (!SharedFunctions.WeekCodeIsValid(fsRouteRow.WeekCode, scheduleTimeModified, this))
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.WEEKCODE_NOT_MATCH_WITH_ROUTE_SCHEDULE, fsScheduleRow.RefNbr, scheduleTimeModified.Value.ToShortDateString()), PXErrorLevel.Error);
                    }
                }
            }

            if (fsAppointmentRow.RouteID != null)
            {
                FSRoute fsRouteRow = PXSelect<FSRoute,
                                        Where<
                                            FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>
                                    .Select(this, fsAppointmentRow.RouteID);

                if (fsRouteRow != null && fsRouteRow.WeekCode != null)
                {
                    if (!SharedFunctions.WeekCodeIsValid(fsRouteRow.WeekCode, scheduleTimeModified, this))
                    {
                        throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.WEEKCODE_NOT_MATCH_WITH_ROUTE, fsScheduleRow.RefNbr, scheduleTimeModified.Value.ToShortDateString()), PXErrorLevel.Error);
                    }
                }
            }
        }
        
        /// <summary>
        /// Assign the [fsAppointmentRow] position on the current [fsRouteDocumentRow].
        /// </summary>
        private void SetRoutePosition(FSRouteDocument fsRouteDocumentRow, FSAppointment fsAppointmentRow)
        {
            if (RouteSetupRecord.Current == null)
            {
                return;
            }

            if (fsAppointmentRow.RoutePosition == null || fsAppointmentRow.RoutePosition < 0)
            {
                int? newRoutePosition;
                FSAppointment fsAppointmentRow_local = PXSelectReadonly<FSAppointment,
                                                            Where<
                                                                FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>,
                                                            OrderBy<Desc<FSAppointment.routePosition>>>
                                                            .Select(this, fsRouteDocumentRow.RouteDocumentID);

                if (fsAppointmentRow_local == null || fsAppointmentRow_local.RoutePosition == null)
                {
                    newRoutePosition = 1;
                }
                else
                {
                    if ((fsAppointmentRow.IsReassigned == true
                                || fsAppointmentRow.Status == ID.Status_Appointment.MANUAL_SCHEDULED)
                                            && RouteSetupRecord.Current.SetFirstManualAppointment == true)
                    {
                        newRoutePosition = 1;
                        UpdateRouteAppointmentsOrder(this, fsRouteDocumentRow.RouteDocumentID, fsAppointmentRow.AppointmentID, newRoutePosition + 1);
                    }
                    else
                    {
                        newRoutePosition = fsAppointmentRow_local.RoutePosition + 1;
                    }
                }

                fsAppointmentRow.RoutePosition = newRoutePosition;
            }
        }

        /// <summary>
        /// Updates the appointments' order in a route in ascending order setting the initial order.
        /// </summary>
        private void UpdateRouteAppointmentsOrder(PXGraph graph, int? routeDocumentID, int? appointmentID, int? firstPositionToSet)
        {
            PXResultset<FSAppointment> fsAppointmentsInRoute = PXSelectReadonly<FSAppointment,
                                                                Where<
                                                                    FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>,
                                                                    And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>,
                                                                OrderBy<Asc<FSAppointment.routePosition>>>
                                                                .Select(graph, routeDocumentID, appointmentID);

            foreach (FSAppointment fsAppointmentInRoute in fsAppointmentsInRoute)
            {
                PXUpdate<
                    Set<FSAppointment.routePosition, Required<FSAppointment.routePosition>>,
                    FSAppointment,
                    Where<
                        FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>
                        >
                >
                .Update(
                        this,
                        firstPositionToSet++,
                        fsAppointmentInRoute.AppointmentID);
            }
        }

        /// <summary>
        /// Set the route info necessary to the [fsAppointmentRow] using the [fsAppointmentRow].RouteID, [fsAppointmentRow].RouteDocumentID and [fsServiceOrderRow].BranchID.
        /// </summary>
        private void SetAppointmentRouteInfo(PXCache cache, FSAppointment fsAppointmentRow, FSServiceOrder fsServiceOrderRow)
        {
            FSRouteDocument fsRouteDocumentRow = GetOrGenerateRoute(fsAppointmentRow.RouteID, fsAppointmentRow.RouteDocumentID, fsAppointmentRow.ScheduledDateTimeBegin, fsServiceOrderRow.BranchID);
            SetRoutePosition(fsRouteDocumentRow, fsAppointmentRow);
            fsAppointmentRow.RouteDocumentID = fsRouteDocumentRow.RouteDocumentID;
            
            PXUpdate<
                Set<FSAppointment.routeDocumentID, Required<FSAppointment.routeDocumentID>,
                Set<FSAppointment.routeID, Required<FSAppointment.routeID>,
                Set<FSAppointment.routePosition, Required<FSAppointment.routePosition>>>>,
                FSAppointment,
                Where<
                    FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>
                    >
            >
            .Update(
                    this, 
                    fsRouteDocumentRow.RouteDocumentID, 
                    fsRouteDocumentRow.RouteID,
                    fsAppointmentRow.RoutePosition,
                    fsAppointmentRow.AppointmentID);

            cache.Graph.SelectTimeStamp();
        }

        /// <summary>
        /// Set schedule times to the [fsAppointmentRow] using Route and Schedule.
        /// </summary>
        private void SetScheduleTimesByRouteAndContract(FSRouteDocument fsRouteDocumentRow, FSAppointment fsAppointmentRow)
        {
            DateTime? routeBegin, routeEnd, slotBegin, slotEnd;

            bool timeBeginHasValue = fsRouteDocumentRow.TimeBegin.HasValue;

            routeBegin = slotBegin = new DateTime(
                                      fsRouteDocumentRow.Date.Value.Year,
                                      fsRouteDocumentRow.Date.Value.Month,
                                      fsRouteDocumentRow.Date.Value.Day,
                                      timeBeginHasValue ? fsRouteDocumentRow.TimeBegin.Value.Hour   : 0,
                                      timeBeginHasValue ? fsRouteDocumentRow.TimeBegin.Value.Minute : 0,
                                      0);

            routeEnd   = fsRouteDocumentRow.TimeEnd;

            List<object> appointmentsArgs = new List<object>();
            PXResultset<FSAppointment> bqlResultSet;
            SharedFunctions.SlotIsContained isContained = SharedFunctions.SlotIsContained.NotContained;

            DateTime dayBegin;
            DateTime dayEnd;
           
            dayBegin = routeBegin.Value.Date;

            if (routeEnd != null)
            {
                dayEnd = routeEnd.Value.Date.AddDays(1);
            }
            else 
            {
                dayEnd = dayBegin.Date.AddDays(1);
            }
            
            double slotDuration = (double)fsAppointmentRow.EstimatedDurationTotal;
            slotEnd = slotBegin.Value.AddMinutes(slotDuration);

            PXSelectBase<FSAppointment> appointmentsBase = 
                                    new PXSelectReadonly<FSAppointment,
                                                      Where<FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>,
                                                        And<FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                        And<FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>,
                                                        And<FSAppointment.scheduledDateTimeEnd, Greater<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                        And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>>>,
                                                       OrderBy<Asc<FSAppointment.routePosition>>>(this);

            appointmentsArgs.Add(fsRouteDocumentRow.RouteDocumentID);
            appointmentsArgs.Add(dayBegin);
            appointmentsArgs.Add(dayEnd);
            appointmentsArgs.Add(routeBegin);
            appointmentsArgs.Add(fsAppointmentRow.AppointmentID);

            bqlResultSet = appointmentsBase.Select(appointmentsArgs.ToArray());

            //Resolve collision with other appointments
            foreach (FSAppointment fsAppointmentRow_local in bqlResultSet)
            {
                if (fsAppointmentRow_local != null && fsAppointmentRow_local.AppointmentID != null)
                {
                    if (fsAppointmentRow_local.ScheduledDateTimeEnd > slotBegin)
                    {
                        isContained = SharedFunctions.SlotIsContainedInSlot(
                                                                            slotBegin, 
                                                                            slotEnd, 
                                                                            fsAppointmentRow_local.ScheduledDateTimeBegin, 
                                                                            fsAppointmentRow_local.ScheduledDateTimeEnd);

                        if (isContained == SharedFunctions.SlotIsContained.Contained
                                || isContained == SharedFunctions.SlotIsContained.PartiallyContained
                                    || isContained == SharedFunctions.SlotIsContained.ExceedsContainment)
                        {
                            slotBegin = fsAppointmentRow_local.ScheduledDateTimeEnd;
                            slotEnd = slotBegin.Value.AddMinutes(slotDuration);
                        }

                        if (isContained == SharedFunctions.SlotIsContained.NotContained)
                        {
                            break;
                        }
                    }
                }
            }

            //Set Times
            fsAppointmentRow.ScheduledDateTimeBegin = slotBegin;
            fsAppointmentRow.ScheduledDateTimeEnd = slotEnd;

            //Time Restriction Verification
            if (fsAppointmentRow.ScheduleID != null)
            {
                FSContractSchedule fsContractScheduleRow = PXSelect<FSContractSchedule, Where<FSContractSchedule.scheduleID, Equal<Required<FSContractSchedule.scheduleID>>>>.Select(this, fsAppointmentRow.ScheduleID);
                if (fsContractScheduleRow != null && (fsContractScheduleRow.RestrictionMax == true || fsContractScheduleRow.RestrictionMin == true) && IsAppointmentInValidRestriction(fsAppointmentRow, fsContractScheduleRow) == false)
                {
                    if (fsContractScheduleRow.RestrictionMin == true)
                    {
                        fsAppointmentRow.ScheduledDateTimeBegin = new DateTime(
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Hour,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Minute,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Second);
                    }
                    else
                    {
                        fsAppointmentRow.ScheduledDateTimeBegin = new DateTime(
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Hour,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Minute,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Second);
                    }

                    fsAppointmentRow.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(slotDuration);
                }
            }
        }

        /// <summary>
        /// Set schedule times to the [fsAppointmentRow] using Contract and Schedule.
        /// </summary>
        private void SetScheduleTimesByContract(FSAppointment fsAppointmentRow)
        {
            DateTime? slotBegin, slotEnd;
            List<object> appointmentsArgs = new List<object>();
            PXResultset<FSAppointment> bqlResultSet;
            SharedFunctions.SlotIsContained isContained = SharedFunctions.SlotIsContained.NotContained;
            DateTime dayBegin = fsAppointmentRow.ScheduledDateTimeBegin.Value.Date;
            DateTime dayEnd   = fsAppointmentRow.ScheduledDateTimeEnd.Value.Date.AddDays(1);

            FSContractSchedule fsContractScheduleRow = PXSelect<FSContractSchedule, Where<FSContractSchedule.scheduleID, Equal<Required<FSContractSchedule.scheduleID>>>>.Select(this, fsAppointmentRow.ScheduleID);

            //The appointment should be created in the time restriction
            if (fsContractScheduleRow != null && (fsContractScheduleRow.RestrictionMax == true || fsContractScheduleRow.RestrictionMin == true))
            {
                if (fsContractScheduleRow.RestrictionMin == true)
                {
                    slotBegin = new DateTime(
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Hour,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Minute,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Second);
                }
                else
                {
                    slotBegin = new DateTime(
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Hour,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Minute,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Second);
                }
            }
            else
            {
                slotBegin = dayBegin;
            }

            double slotDuration = (double)fsAppointmentRow.EstimatedDurationTotal;

            slotEnd = slotBegin.Value.AddMinutes(slotDuration);

            PXSelectBase<FSAppointment> appointmentsBase = new PXSelectReadonly<FSAppointment,
                                                               Where<
                                                                    FSAppointment.scheduledDateTimeBegin, Less<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                                    And<FSAppointment.scheduledDateTimeEnd, Greater<Required<FSAppointment.scheduledDateTimeEnd>>>>,
                                                               OrderBy<Asc<FSAppointment.scheduledDateTimeBegin>>>(this);
            appointmentsArgs.Add(dayEnd);
            appointmentsArgs.Add(dayBegin);

            bqlResultSet = appointmentsBase.Select(appointmentsArgs.ToArray());

            //Resolve collision with other appointments
            foreach (FSAppointment fsAppointmentRow_local in bqlResultSet)
            {
                if (fsAppointmentRow_local != null && fsAppointmentRow_local.AppointmentID != null)
                {
                    if (fsAppointmentRow_local.ScheduledDateTimeEnd > slotBegin)
                    {
                        isContained = SharedFunctions.SlotIsContainedInSlot(
                                                                            slotBegin, 
                                                                            slotEnd, 
                                                                            fsAppointmentRow_local.ScheduledDateTimeBegin, 
                                                                            fsAppointmentRow_local.ScheduledDateTimeEnd);

                        if (isContained == SharedFunctions.SlotIsContained.Contained
                                || isContained == SharedFunctions.SlotIsContained.PartiallyContained
                                    || isContained == SharedFunctions.SlotIsContained.ExceedsContainment)
                        {
                            slotBegin = fsAppointmentRow_local.ScheduledDateTimeEnd;
                            slotEnd = slotBegin.Value.AddMinutes(slotDuration);
                        }

                        if (isContained == SharedFunctions.SlotIsContained.NotContained)
                        {
                            break;
                        }
                    }
                }
            }

            //Set Times
            fsAppointmentRow.ScheduledDateTimeBegin = slotBegin;
            fsAppointmentRow.ScheduledDateTimeEnd = slotEnd;

            //Time Restriction verifcation
            if (fsAppointmentRow.ScheduleID != null)
            {
                if (fsContractScheduleRow != null && (fsContractScheduleRow.RestrictionMax == true || fsContractScheduleRow.RestrictionMin == true) && IsAppointmentInValidRestriction(fsAppointmentRow, fsContractScheduleRow) == false)
                {
                    if (fsContractScheduleRow.RestrictionMin == true)
                    {
                        fsAppointmentRow.ScheduledDateTimeBegin = new DateTime(
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Hour,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Minute,
                                                                            fsContractScheduleRow.RestrictionMinTime.Value.Second);
                    }
                    else
                    {
                        fsAppointmentRow.ScheduledDateTimeBegin = new DateTime(
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Hour,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Minute,
                                                                            fsContractScheduleRow.RestrictionMaxTime.Value.Second);
                    }

                    fsAppointmentRow.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(slotDuration);
                }
            }
        }

        /// <summary>
        /// Verifies if the [fsAppointmentRow].ScheduleTimeBegin and [fsAppointmentRow].ScheduleTimeEnd are valid in the fsContractScheduleRow restrictions.
        /// </summary>
        private bool IsAppointmentInValidRestriction(FSAppointment fsAppointmentRow, FSContractSchedule fsContractScheduleRow)
        {
            DateTime compareDateTime;

            if (fsContractScheduleRow.RestrictionMax == true)
            {
                compareDateTime = new DateTime(
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Hour,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Minute,
                                            fsContractScheduleRow.RestrictionMaxTime.Value.Second);
                
                if (fsAppointmentRow.ScheduledDateTimeBegin > compareDateTime)
                {
                    return false;
                }
            }

            if (fsContractScheduleRow.RestrictionMin == true)
            {
                compareDateTime = new DateTime(
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                            fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Hour,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Minute,
                                            fsContractScheduleRow.RestrictionMinTime.Value.Second);
                
                if (fsAppointmentRow.ScheduledDateTimeBegin < compareDateTime)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the specific route in the Routes Module using the [routeID], [routeDocumentID] and [appointmentScheduledDate].
        /// </summary>
        private FSRouteDocument GetOrGenerateRoute(int? routeID, int? routeDocumentID, DateTime? appointmentScheduledDate, int? branchID)
        {
            FSRouteDocument fsRouteDocumentRow;

            DateTime? begin = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 0, 0, 0); //12:00 AM
            DateTime? end = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 23, 59, 59); //23:59 AM

            if (routeDocumentID == null)
            {
                fsRouteDocumentRow = PXSelect<FSRouteDocument,
                                     Where<
                                        FSRouteDocument.routeID, Equal<Required<FSRouteDocument.routeID>>,
                                        And<FSRouteDocument.timeBegin, Between<Required<FSRouteDocument.timeBegin>, Required<FSRouteDocument.timeBegin>>>>>.Select(this, routeID, begin, end);
            }
            else
            {
                fsRouteDocumentRow = PXSelect<FSRouteDocument,
                                     Where<
                                        FSRouteDocument.routeID, Equal<Required<FSRouteDocument.routeID>>,
                                        And<FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>,
                                     OrderBy<Desc<FSRouteDocument.routeDocumentID>>>.Select(this, routeID, routeDocumentID);
            }

            if (fsRouteDocumentRow == null)
            {
                FSRoute fsRouteRow = PXSelect<FSRoute, Where<FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>.Select(this, routeID);

                DateTime? beginTimeOnWeekDay = new DateTime();

                SharedFunctions.ValidateExecutionDay(fsRouteRow, appointmentScheduledDate.Value.DayOfWeek, ref beginTimeOnWeekDay);

                if (!beginTimeOnWeekDay.HasValue) 
                { 
                    beginTimeOnWeekDay = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 0, 0, 0);
                }

                RouteDocumentMaint graphRouteMaint = PXGraph.CreateInstance<RouteDocumentMaint>();
                FSRouteDocument fsRouteDocumentRow_Local = new FSRouteDocument();

                fsRouteDocumentRow_Local.GeneratedBySystem = true;
                fsRouteDocumentRow_Local.RouteID = routeID;
                fsRouteDocumentRow_Local.BranchID = branchID;
                fsRouteDocumentRow_Local.Date = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 0, 0, 0);
                fsRouteDocumentRow_Local.TimeBegin = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, beginTimeOnWeekDay.Value.Hour, beginTimeOnWeekDay.Value.Minute, 0);
                fsRouteDocumentRow_Local.TimeEnd = new DateTime(appointmentScheduledDate.Value.Year, appointmentScheduledDate.Value.Month, appointmentScheduledDate.Value.Day, 23, 59, 59);
                fsRouteDocumentRow_Local.Status = ID.Status_Route.OPEN;

                graphRouteMaint.RouteRecords.Insert(fsRouteDocumentRow_Local);
                graphRouteMaint.Save.Press();
                fsRouteDocumentRow = graphRouteMaint.RouteRecords.Current;
            }

            return fsRouteDocumentRow;
        }

        /// <summary>
        /// Calculate all the statistics for the routes involving the given appointment.
        /// </summary>
        /// <param name="graph">Context graph instance.</param>
        /// <param name="fsAppointmentRow">FSAppointment Row.</param>
        /// <param name="simpleStatsOnly">Boolean flag that controls whereas only single statistics need to be calculated or not.</param>
        public static void CalculateRouteStats(PXGraph graph, FSAppointment fsAppointmentRow, string apiKey, bool simpleStatsOnly = false) 
        {
            //TODO: SD-7859
            RouteDocumentMaint graphRouteMaint = PXGraph.CreateInstance<RouteDocumentMaint>();
             
            FSRouteDocument fsRouteDocumentRow_Current = PXSelect<FSRouteDocument,
                                            Where<
                                                FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                                            .Select(graph, fsAppointmentRow.RouteDocumentID);

            FSRouteDocument fsRouteDocumentRow_Previous = PXSelect<FSRouteDocument,
                                        Where<
                                            FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                                        .Select(graph, fsAppointmentRow.Mem_LastRouteDocumentID);

            if (fsRouteDocumentRow_Current != null)
            {
                fsRouteDocumentRow_Current.RouteStatsUpdated = simpleStatsOnly == false;
                SetRouteSimpleStats(graph, fsAppointmentRow.RouteDocumentID, ref fsRouteDocumentRow_Current);

                if (simpleStatsOnly == false)
                {                   
                    SetRouteMapStats(graph, fsAppointmentRow, fsAppointmentRow.RouteDocumentID, ref fsRouteDocumentRow_Current, apiKey);
                }

                graphRouteMaint.RouteRecords.Update(fsRouteDocumentRow_Current);
                graphRouteMaint.Save.Press();
            }

            if (fsRouteDocumentRow_Previous != null)
            {
                SetRouteSimpleStats(graph, fsRouteDocumentRow_Previous.RouteDocumentID, ref fsRouteDocumentRow_Previous);

                if (simpleStatsOnly != true)
                {
                    SetRouteMapStats(graph, fsAppointmentRow, fsRouteDocumentRow_Previous.RouteDocumentID, ref fsRouteDocumentRow_Previous, apiKey);
                }

                graphRouteMaint.RouteRecords.Update(fsRouteDocumentRow_Previous);
                graphRouteMaint.Save.Press();
            }
        }

        /// <summary>
        /// Return the total duration of the services within a given route.
        /// </summary>
        private static int? CalculateRouteTotalServicesDuration(PXResultset<FSAppointmentDet> bqlResultSet)
        {
            //TODO: SD-7859
            int? servicesDurationTotal = 0;

            foreach (FSAppointmentDet fsAppointmentDetRow in bqlResultSet)
            {
                servicesDurationTotal += fsAppointmentDetRow.EstimatedDuration;
            }

            return servicesDurationTotal;
        }

        /// <summary>
        /// Return the total duration of the appointments within a given route.
        /// </summary>
        /// <param name="graph">Context graph instance.</param>
        /// <param name="routeDocumentID">Id for Route Document.</param>
        /// <param name="fsAppointmentRow">FSAppointment object.</param>
        /// <returns>RowCount of appointments.</returns>
        private static int? CalculateRouteTotalAppointmentsDuration(PXGraph graph, int? routeDocumentID, FSAppointment fsAppointmentRow)
        {
            //TODO: SD-7859
            int? appointmentsDurationTotal = 0;
            appointmentsDurationTotal += fsAppointmentRow.EstimatedDurationTotal;
            foreach (FSAppointment fsAppointmentRowInRoute in
                                PXSelectReadonly<FSAppointment,
                                Where<
                                    FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>,
                                    And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>
                                .Select(graph, routeDocumentID, fsAppointmentRow.AppointmentID))
            {
                appointmentsDurationTotal += fsAppointmentRowInRoute.Mem_Duration;
            }

            return appointmentsDurationTotal;
        }

        /// <summary>
        /// Return the total number of appointments for a given route.
        /// </summary>
        private static int? GetRouteTotalAppointments(PXGraph graph, int? routeDocumentID)
        {
            //TODO: SD-7859
            var fsAppointmentSet = PXSelectGroupBy<FSAppointment,
                                   Where<
                                        FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>,
                                   Aggregate<Count<FSAppointment.appointmentID>>>
                                   .Select(graph, routeDocumentID);

            return fsAppointmentSet.RowCount;
        }

        /// <summary>
        /// Return the services for a given route.
        /// </summary>
        private static PXResultset<FSAppointmentDet> GetRouteServices(PXGraph graph, int? routeDocumentID)
        {
            //TODO: SD-7859
            PXResultset<FSAppointmentDet> bqlResultSet = PXSelectJoin<FSAppointmentDet,
                                      InnerJoin<FSAppointment,
                                      On<
                                        FSAppointmentDet.appointmentID, Equal<FSAppointment.appointmentID>>,
                                      InnerJoin<FSSODet,
                                          On<
                                              FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>>,
                                      Where<FSSODet.lineType, Equal<FSSODet.lineType.Service>,
                                          And<FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>>>
                                      .Select(graph, routeDocumentID);

            return bqlResultSet;
        }

        /// <summary>
        /// Split an array [geoLocationArray] in a list of array of [length] element.
        /// </summary>
        public static List<GLocation[]> SplitArrayInList(GLocation[] geoLocationArray, int length)
        {
            List<GLocation[]> listGeoLocationArray = new List<GLocation[]>();
            int totalSplit = 1 + (int)Math.Ceiling(((float)geoLocationArray.Length - (float)length) / ((float)length - 1));
            int totalLength = 0;
            int totalElement = 0;

            if (totalSplit == 1)
            {
                listGeoLocationArray.Add(geoLocationArray);

                return listGeoLocationArray;
            }

            for (int i = 0; i < totalSplit; i++)
            {
                totalLength = length;
                GLocation[] locationAuxiliar;

                if (i == totalSplit - 1 && i != 0)
                {
                    totalLength = geoLocationArray.Length - totalElement;
                    locationAuxiliar = new GLocation[totalLength + 1];
                }
                else
                {
                    locationAuxiliar = new GLocation[totalLength];
                }

                for (int j = 0; j < totalLength; j++)
                {
                    if (i != 0)
                    {
                        locationAuxiliar[j] = geoLocationArray[totalElement - 1];
                        if (j != totalLength - 1)
                        {
                            totalElement++;
                        }
                    }
                    else
                    {
                        locationAuxiliar[j] = geoLocationArray[totalElement];
                        totalElement++;
                    }
                }

                if (i == totalSplit - 1 && i != 0)
                {
                    locationAuxiliar[totalLength] = geoLocationArray[geoLocationArray.Length - 1];
                }

                listGeoLocationArray.Add(locationAuxiliar);
            }

            return listGeoLocationArray;
        }

        /// <summary>
        /// Calculate the google map statistics for a given route.
        /// </summary>
        /// <param name="graph">Context graph instance.</param>
        /// <param name="routeDocumentID">ID for the route.</param>
        /// <param name="totalDistance">Total driving distance in meters.</param>
        /// <param name="totalDistanceFriendly">Total driving distance user friendly.</param>
        /// <param name="totalDuration">Total driving duration in seconds.</param>
        private static void CalculateRouteMapStats(
                                            PXGraph graph,
                                            int? routeDocumentID, 
                                            ref decimal? totalDistance, 
                                            ref string totalDistanceFriendly,
                                            ref int? totalDuration,
                                            string apiKey)
        {
            //TODO: SD-7859
            FSRouteDocument fsRouteDocumentRow = 
                PXSelect<FSRouteDocument,
                Where<
                    FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                .Select(graph, routeDocumentID);

            var fsAppointmentSet = PXSelectReadonly2<FSAppointment,
                                   InnerJoin<FSServiceOrder,
                                        On<FSAppointment.sOID, Equal<FSServiceOrder.sOID>>,
                                   InnerJoin<FSContact,
                                        On<FSContact.contactID, Equal<FSServiceOrder.serviceOrderContactID>>,
                                   InnerJoin<FSAddress,
                                        On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>>>>,
                                   Where<
                                        FSAppointment.routeDocumentID, Equal<Required<FSAppointment.routeDocumentID>>>,
                                   OrderBy<
                                        Asc<FSAppointment.routePosition>>>
                                   .Select(graph, routeDocumentID);

            List<GLocation> geoLocationList = new List<GLocation>();
            string nodeAddress;
  
            //Gets the Begin Branch Location Row
            FSBranchLocation fsBranchLocationRow_Begin = PXSelectJoin<FSBranchLocation,
                                                        InnerJoin<FSRoute,
                                                            On<
                                                                FSRoute.beginBranchLocationID, Equal<FSBranchLocation.branchLocationID>>>,
                                                        Where<
                                                            FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>
                                                        .Select(graph, fsRouteDocumentRow.RouteID);

            //Gets the End Branch Location Row
            FSBranchLocation fsBranchLocationRow_End = PXSelectJoin<FSBranchLocation,
                                                      InnerJoin<FSRoute,
                                                        On<
                                                            FSRoute.endBranchLocationID, Equal<FSBranchLocation.branchLocationID>>>,
                                                      Where<
                                                        FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>
                                                      .Select(graph, fsRouteDocumentRow.RouteID);

            //setting fsBeginBranchLocationRow as first address
            nodeAddress = SharedFunctions.GetBranchLocationAddress(graph, fsBranchLocationRow_Begin);

            if (!string.IsNullOrEmpty(nodeAddress))
            {
                geoLocationList.Add(new GLocation(nodeAddress));
            }

            var graphAppointmentMaint = PXGraph.CreateInstance<AppointmentEntry>();
            graphAppointmentMaint.CalculateGoogleStats = false;

            List<FSAppointment> routeAppointmentList = new List<FSAppointment>();

            foreach (PXResult<FSAppointment, FSServiceOrder, FSContact, FSAddress> bqlResult in fsAppointmentSet)
            {
                routeAppointmentList.Add((FSAppointment)bqlResult);
                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)bqlResult;
                FSAddress fsAddressRow = (FSAddress)bqlResult;

                nodeAddress = SharedFunctions.GetAppointmentAddress(fsAddressRow);

                if (!string.IsNullOrEmpty(nodeAddress))
                {
                    geoLocationList.Add(new GLocation(nodeAddress));
                }
            }

            nodeAddress = SharedFunctions.GetBranchLocationAddress(graph, fsBranchLocationRow_End);

            //setting fsBeginBranchLocationRow as first address
            if (!string.IsNullOrEmpty(nodeAddress))
            {
                geoLocationList.Add(new GLocation(nodeAddress));
            }

            GLocation[] geoLocationArray = geoLocationList.ToArray();

            List<GLocation[]> geoLocationArrayList = SplitArrayInList(geoLocationArray, 10);

            // Ignoring appointments without address
            try 
            {
                int i = 0;
                double distanceTotal = 0;
                totalDuration = 0;
                totalDistance = 0;
                FSAppointment fsAppointmentRow;
                double servicesDuration;
                DateTime? lastAppoinmentEndTime = DateTime.Now;

                foreach (GLocation[] glocationArray in geoLocationArrayList)
                {
                    Route route = RouteDirections.GetRoute("distance", apiKey, glocationArray);
                    if (route != null)
                    {
                        string firstLegDistanceDescr = route.Legs[0].DistanceDescr;
                        int indexOfBlank = firstLegDistanceDescr.IndexOf(" ");
                        string metric = firstLegDistanceDescr.Substring(indexOfBlank);

                        foreach (RouteLeg leg in route.Legs)
                        {
                            if (routeAppointmentList.ElementAtOrDefault(i) != null)
                            {
                                graphAppointmentMaint.AppointmentRecords.Current = graphAppointmentMaint.AppointmentRecords.Search<FSAppointment.refNbr>(
                                routeAppointmentList.ElementAt(i).RefNbr, routeAppointmentList.ElementAt(i).SrvOrdType);
                                fsAppointmentRow = graphAppointmentMaint.AppointmentRecords.Current;

                                double estimatedDurationTotal = (double)fsAppointmentRow.EstimatedDurationTotal;
                                
                                // assign one minute to the services duration if the appointment has NO services.
                                servicesDuration = (estimatedDurationTotal == 0) ? 1 : estimatedDurationTotal;

                                if (i == 0)
                                {
                                    fsAppointmentRow.ScheduledDateTimeBegin = new DateTime(
                                                                                fsAppointmentRow.ScheduledDateTimeBegin.Value.Year,
                                                                                fsAppointmentRow.ScheduledDateTimeBegin.Value.Month,
                                                                                fsAppointmentRow.ScheduledDateTimeBegin.Value.Day,
                                                                                fsRouteDocumentRow.TimeBegin.Value.Hour,
                                                                                fsRouteDocumentRow.TimeBegin.Value.Minute,
                                                                                fsRouteDocumentRow.TimeBegin.Value.Second);

                                    fsAppointmentRow.ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddSeconds(leg.Duration);
                                }
                                else
                                {
                                    fsAppointmentRow.ScheduledDateTimeBegin = lastAppoinmentEndTime;
                                    fsAppointmentRow.ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddSeconds(leg.Duration);
                                }

                                fsAppointmentRow.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeBegin;
                                fsAppointmentRow.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd.Value.AddMinutes(servicesDuration);
                                TimeSpan? diffTime = fsAppointmentRow.ScheduledDateTimeEnd - fsAppointmentRow.ScheduledDateTimeEnd;

                                PXUpdate<
                                Set<FSAppointment.scheduledDateTimeBegin, Required<FSAppointment.scheduledDateTimeBegin>,
                                Set<FSAppointment.scheduledDateTimeEnd, Required<FSAppointment.scheduledDateTimeEnd>,
                                Set<FSAppointment.routePosition, Required<FSAppointment.routePosition>>>>,
                                FSAppointment,
                                Where<
                                    FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>
                                    >
                                >
                                .Update(
                                        graph,
                                        fsAppointmentRow.ScheduledDateTimeBegin,
                                        fsAppointmentRow.ScheduledDateTimeEnd,
                                        fsAppointmentRow.RoutePosition,
                                        fsAppointmentRow.AppointmentID);

                                lastAppoinmentEndTime = fsAppointmentRow.ScheduledDateTimeEnd;
                            }

                            indexOfBlank = leg.DistanceDescr.IndexOf(" ");
                            distanceTotal += Convert.ToDouble(leg.DistanceDescr.Substring(0, indexOfBlank));
                            i++;
                        }
                        
                        totalDistance += (decimal)route.Distance;
                        totalDistanceFriendly = distanceTotal.ToString() + " " + metric;
                        totalDuration += route.Duration / 60;
                    }
                }

                graph.SelectTimeStamp();
            }
            catch (Exception) 
            {
                //@TODO SD-5806 Handle google maps exceptions
            }
        }

        /// <summary>
        /// Set the simple stats for a given route.
        /// </summary>
        /// <param name="graph">Context graph instance.</param>
        /// <param name="routeDocumentID">ID of the route.</param>
        /// <param name="fsRouteDocumentRow">FSRoute object.</param>
        private static void SetRouteSimpleStats(PXGraph graph, int? routeDocumentID, ref FSRouteDocument fsRouteDocumentRow)
        {
            //TODO: SD-7859
            fsRouteDocumentRow.TotalNumAppointments = GetRouteTotalAppointments(graph, routeDocumentID);

            PXResultset<FSAppointmentDet> bqlResultSet = GetRouteServices(graph, routeDocumentID);
            fsRouteDocumentRow.TotalServices = bqlResultSet.Count;
            fsRouteDocumentRow.TotalServicesDuration = CalculateRouteTotalServicesDuration(bqlResultSet);
        }

        public static void SetRouteMapStats(PXGraph graph, FSAppointment fsAppointmentRow, int? routeDocumentID, ref FSRouteDocument fsRouteDocumentRow, string apiKey)
        {
            //TODO: SD-7859
            decimal? totalDistance = null;
            string totalDistanceFriendly = null;
            int? totalDuration = null;

            using (var ts = new PXTransactionScope())
            {
                try
                {
                    CalculateRouteMapStats(graph, routeDocumentID, ref totalDistance, ref totalDistanceFriendly, ref totalDuration, apiKey);
                    ts.Complete();
                }
                catch
                {
                    ts.Dispose();
                }
            }

            if (totalDistance != null)
            {
                fsRouteDocumentRow.TotalDistance = totalDistance;
            }

            if (totalDistanceFriendly != null)
            {
                fsRouteDocumentRow.TotalDistanceFriendly = totalDistanceFriendly;
            }

            if (totalDuration != null)
            {
                fsRouteDocumentRow.TotalDuration = totalDuration;
                fsRouteDocumentRow.TotalTravelTime = CalculateRouteTotalAppointmentsDuration(graph, routeDocumentID, fsAppointmentRow) + fsRouteDocumentRow.TotalDuration;
            }
        }

        /// <summary>
        /// Adjust the DateTime-End for the appointment Scheduled/Actual DateTimes.
        /// </summary>
        /// <param name="startDate">Scheduled/Actual Begin.</param>
        /// <param name="endDate">Scheduled/Actual End.</param>
        /// <returns>Adjusted date for Scheduled/Actual-End DateTimes.</returns>
        private DateTime? AdjustDateTimeEnd(DateTime? startDate, DateTime? endDate)
        {
            if (startDate != null
                    && endDate != null)
            {
                DateTime timeStart = (DateTime)startDate;
                DateTime timeEnd = (DateTime)endDate;
                DateTime auxDateTimeEnd;

                if (startDate > endDate)
                {
                    auxDateTimeEnd = new DateTime(
                                            timeStart.Year,
                                            timeStart.Month,
                                            timeStart.Day,
                                            timeEnd.Hour,
                                            timeEnd.Minute,
                                            timeEnd.Second);

                    return auxDateTimeEnd.AddDays(1);
                }
                else if (timeStart.Day != timeEnd.Day)
                {
                    auxDateTimeEnd = new DateTime(
                                            timeStart.Year,
                                            timeStart.Month,
                                            timeStart.Day,
                                            timeEnd.Hour,
                                            timeEnd.Minute,
                                            timeEnd.Second);

                    if (startDate < auxDateTimeEnd)
                    {
                        return auxDateTimeEnd;
                    }
                    else
                    {
                        return auxDateTimeEnd.AddDays(1);
                    }
                }
            }

            return endDate;
        }

        /// <summary>
        /// Sets the date part of Actual-Start-time and Actual-End-time fields in appointment lines.
        /// </summary>
        private void SetAppointmentLineActualDates(PXCache cache, FSAppointmentDet fsAppointmentDetRow)
        {
            if (AppointmentRecords.Current.ExecutionDate == null)
            {
                return;
            }

            DateTime? newDateTime = null;

            if (fsAppointmentDetRow.ActualDateTimeBegin != null)
            {
                newDateTime = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentDetRow.ActualDateTimeBegin, AppointmentRecords.Current.ExecutionDate);
                cache.SetValueExtIfDifferent<FSAppointmentDetService.actualDateTimeBegin>(fsAppointmentDetRow, newDateTime);

                if (fsAppointmentDetRow.ActualDateTimeEnd != null)
                {
                    newDateTime = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentDetRow.ActualDateTimeEnd, AppointmentRecords.Current.ExecutionDate);
                    cache.SetValueExtIfDifferent<FSAppointmentDetService.actualDateTimeEnd>(fsAppointmentDetRow, newDateTime);
                }
            }
            else
            {
                fsAppointmentDetRow.ActualDateTimeEnd = null;
            }
        }

        private void ResetLatLong(FSSrvOrdType fsSrvOrdTypeRow)
        {
            FSAppointment fsAppointmentRow = AppointmentSelected.Current;

            if (fsSrvOrdTypeRow != null
                    && fsAppointmentRow != null
                        && fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT)
            {
                fsAppointmentRow.MapLatitude = null;
                fsAppointmentRow.MapLongitude = null;
            }
        }

        private void SetGeoLocation(FSAddress fsAddressRow, FSSrvOrdType fsSrvOrdTypeRow)
        {
            if (fsSrvOrdTypeRow != null
                    && fsSrvOrdTypeRow.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT)
            {
                FSAppointment fsAppointmentRow = AppointmentSelected.Current;

                if (fsAppointmentRow != null
                    && (fsAppointmentRow.MapLatitude == null
                        || fsAppointmentRow.MapLongitude == null))
                {
                    try
                    { 
                        GLocation[] results = Geocoder.Geocode(SharedFunctions.GetAppointmentAddress(fsAddressRow), SetupRecord.Current.MapApiKey);

                        if (results != null
                            && results.Length > 0)
                        {
                            //If there are many locations, we just pick first one
                            fsAppointmentRow.MapLatitude = (decimal)results[0].LatLng.Latitude;
                            fsAppointmentRow.MapLongitude = (decimal)results[0].LatLng.Longitude;
                        }
                    }
                    catch
                    {
                        // For this case don't do nothing.
                    }
                }
            }
        }

        /// <summary>
        /// Return true if the current appointment has at least one <c>FSAppointmentEmployee</c> row with employee or employee combined type.
        /// </summary>
        private bool AreThereAnyEmployees()
        {
            FSAppointmentEmployee fsAppointmentEmployeeRow =
                PXSelect<FSAppointmentEmployee,
                Where<
                    Where<
                        FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                    And<
                        Where<FSAppointmentEmployee.type, Equal<BAccountType.employeeType>,
                        Or<FSAppointmentEmployee.type, Equal<BAccountType.empCombinedType>>>>>>>
                .SelectWindowed(this, 0, 1);

            return fsAppointmentEmployeeRow != null;
        }

        /// <summary>
        /// Hides or shows Appointments, Pickup Delivery Items Tabs & Invoice Info section.
        /// </summary>
        /// <param name="fsAppointmentRow">Appointment Row.</param>
        private void HideOrShowTabs(FSAppointment fsAppointmentRow)
        {
            this.AppointmentAttendees.AllowSelect = fsAppointmentRow.Mem_ShowAttendees == true;
            this.PickupDeliveryItems.AllowSelect = fsAppointmentRow.IsRouteAppoinment == true;

            bool isBillingByApp = BillingCycleRelated.Current != null && BillingCycleRelated.Current.BillingBy == ID.Billing_By.APPOINTMENT ? true : false;
            this.AppointmentPostedIn.AllowSelect = isBillingByApp;
        }

        private void HideOrShowRouteInfo(FSAppointment fsAppointmentRow)
        {
            PXUIFieldAttribute.SetVisible<FSAppointment.routeID>(AppointmentRecords.Cache, fsAppointmentRow, fsAppointmentRow.IsRouteAppoinment == true);
            PXUIFieldAttribute.SetVisible<FSAppointment.routeDocumentID>(AppointmentRecords.Cache, fsAppointmentRow, fsAppointmentRow.IsRouteAppoinment == true);
        }

        /// <summary>
        /// Hides or shows fields related to the Employee Time Cards Integration.
        /// </summary>
        private void HideOrShowTimeCardsIntegration(PXCache cache, FSAppointment fsAppointmentRow)
        {
            if (this.SetupRecord.Current != null
                    && ServiceOrderTypeSelected.Current != null)
            {
                bool enableEmpTimeCardIntegration = (bool)this.SetupRecord.Current.EnableEmpTimeCardIntegration 
                                                        && ServiceOrderTypeSelected.Current.RequireTimeApprovalToInvoice == true;
                PXCache cacheFSAppointmentEmployee = this.AppointmentEmployees.Cache;

                PXUIFieldAttribute.SetVisible<FSAppointment.timeRegistered>(cache, fsAppointmentRow, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.timeCardCD>(cacheFSAppointmentEmployee, null, enableEmpTimeCardIntegration);
                PXUIFieldAttribute.SetVisible<FSAppointmentEmployee.approvedTime>(cacheFSAppointmentEmployee, null, enableEmpTimeCardIntegration);
            }
        }

        /// <summary>
        /// Sets the BillServiceContractID field from the ServiceOrder's ServiceContractID field for prepaid contracts.
        /// </summary>
        private void SetBillServiceContractIDFromSO(PXCache cache, FSAppointment fsAppointmentRow, int? serviceContractID)
        {
            if(serviceContractID == null)
            {
                return;
            }

            FSServiceContract fsServiceContractRow = PXSelect<FSServiceContract,
                                                         Where<
                                                               FSServiceContract.serviceContractID, Equal<Required<FSServiceContract.serviceContractID>>>>
                                                         .Select(cache.Graph, serviceContractID);

            if (fsServiceContractRow != null)
            {
                bool isPrepaidContract = fsServiceContractRow.BillingType == ID.Contract_BillingType.STANDARDIZED_BILLINGS;

                if (isPrepaidContract == true
                        && BillingCycleRelated.Current != null
                            && BillingCycleRelated.Current.BillingBy == ID.Billing_By.APPOINTMENT)
                {
                    cache.SetValueExt<FSAppointment.billServiceContractID>(fsAppointmentRow, fsServiceContractRow.ServiceContractID);
                }
            }
        }

        protected void SetServiceOrderRelatedBySORefNbr(PXCache cache, FSAppointment fsAppointmentRow)
        {
            // Loading an existing FSServiceOrder record
            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)PXSelectorAttribute.Select<FSAppointment.soRefNbr>(cache, fsAppointmentRow);

            fsAppointmentRow.SOID = fsServiceOrderRow.SOID;

            LoadServiceOrderRelated(fsAppointmentRow);

            cache.SetValueExt<FSAppointment.curyID>(fsAppointmentRow, fsServiceOrderRow.CuryID);
            cache.SetValueExt<FSAppointment.taxZoneID>(fsAppointmentRow, fsServiceOrderRow.TaxZoneID);
            cache.SetValueExt<FSAppointment.dfltProjectTaskID>(fsAppointmentRow, fsServiceOrderRow.DfltProjectTaskID);
            cache.SetDefaultExt<FSAppointment.salesPersonID>(fsAppointmentRow);
            cache.SetDefaultExt<FSAppointment.commissionable>(fsAppointmentRow);

            SetBillServiceContractIDFromSO(cache, fsAppointmentRow, fsServiceOrderRow.ServiceContractID);

            if (fsAppointmentRow.DocDesc == null)
            {
                fsAppointmentRow.DocDesc = fsServiceOrderRow.DocDesc;
            }
        }

        protected virtual bool CanExecuteAppointmentRowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            //SrvOrdType is key field
            if (string.IsNullOrWhiteSpace(fsAppointmentRow.SrvOrdType))
            {
                GraphHelper.RaiseRowPersistingException<FSAppointment.srvOrdType>(cache, e.Row);
            }

            LoadServiceOrderRelated(fsAppointmentRow);

            if (ServiceOrderRelated.Current == null)
            {
                throw new PXException(TX.Error.SERVICE_ORDER_SELECTED_IS_NULL);
            }

            BackupOriginalValues(cache, e);

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
            {
                if (fsAppointmentRow.SOID < 0)
                {
                    fsAppointmentRow.SOID = ServiceOrderRelated.Current.SOID;
                    fsAppointmentRow.SORefNbr = ServiceOrderRelated.Current.RefNbr;
                }
            }

            return true;
        }

        protected bool UpdateServiceOrder(
                                            FSAppointment fsAppointmentRow,
                                            AppointmentEntry graphAppointmentEntry,
                                            object rowInProcessing,
                                            PXDBOperation operation,
                                            PXTranStatus? tranStatus)
        {
            if (serviceOrderIsAlreadyUpdated == true || SkipServiceOrderUpdate == true)
            {
                return true;
            }

            // tranStatus is null when the caller is a RowPersisting event.
            if (tranStatus != null && tranStatus == PXTranStatus.Aborted)
            {
                return false;
            }

            bool deletingAppointment = false;
            bool forceAppointmentCheckings = false;
            PXEntryStatus appointmentRowEntryStatus = graphAppointmentEntry.AppointmentRecords.Cache.GetStatus(fsAppointmentRow);

            if (appointmentRowEntryStatus == PXEntryStatus.Deleted)
            {
                // When the Appointment is being deleted, the ServiceOrder is not updated in any RowPersisting event
                // but in the RowPersisted event of FSAppointment.
                if (tranStatus == null || 
                        tranStatus != PXTranStatus.Completed || 
                        operation != PXDBOperation.Delete ||
                        (rowInProcessing is FSAppointment) == false)
                {
                    return true;
                }
                else
                {
                    deletingAppointment = true;
                }
            }

            ServiceOrderEntry graphServiceOrderEntry = GetLocalServiceOrderEntryGraph(true);

            FSServiceOrder fsServiceOrderRow = graphServiceOrderEntry.ServiceOrderRecords.Current = graphServiceOrderEntry.ServiceOrderRecords
                        .Search<FSServiceOrder.refNbr>(fsAppointmentRow.SORefNbr, fsAppointmentRow.SrvOrdType);

            if (fsServiceOrderRow == null || 
                    fsServiceOrderRow.SrvOrdType != fsAppointmentRow.SrvOrdType || 
                    fsServiceOrderRow.RefNbr != fsAppointmentRow.SORefNbr)
            {
                throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.RECORD_NOT_FOUND, TX.CustomTextFields.SERVICE_ORDER), PXErrorLevel.Error);
            }

            if (deletingAppointment == false)
            {
                if (appointmentRowEntryStatus != PXEntryStatus.Inserted)
                {
                    string oldStatus = (string)graphAppointmentEntry.AppointmentRecords.Cache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow);

                    if (oldStatus != fsAppointmentRow.Status)
                    {
                        forceAppointmentCheckings = ID.Status_Appointment.IsOpen(oldStatus) != ID.Status_Appointment.IsOpen(fsAppointmentRow.Status);
                    }

                    if (fsAppointmentRow.Finished != (bool?)graphAppointmentEntry.AppointmentRecords.Cache.GetValueOriginal<FSAppointment.finished>(fsAppointmentRow))
                    {
                        forceAppointmentCheckings = true;
                    }
                }
                else
                {
                    if (ID.Status_Appointment.IsOpen(fsAppointmentRow.Status) && fsAppointmentRow.Finished == false)
                    { 
                        forceAppointmentCheckings = true;
                    }
                }

                if (forceAppointmentCheckings == false)
                {
                    forceAppointmentCheckings = IsThereAnySODetReferenceBeingDeleted<FSAppointmentDetService.sODetID>(graphAppointmentEntry.AppointmentDetServices.Cache);
                }

                if (forceAppointmentCheckings == false)
                {
                    forceAppointmentCheckings = IsThereAnySODetReferenceBeingDeleted<FSAppointmentDetPart.sODetID>(graphAppointmentEntry.AppointmentDetParts.Cache);
                }
            }

            if (graphServiceOrderEntry.ServiceOrderRecords.Current.CuryInfoID < 0)
            {
                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetValueExt<FSServiceOrder.curyInfoID>(graphServiceOrderEntry.ServiceOrderRecords.Current, fsAppointmentRow.CuryInfoID);
            }

            if (deletingAppointment == true || 
                    forceAppointmentCheckings == true ||
                    ServiceOrderRelated.Cache.GetStatus(ServiceOrderRelated.Current) != PXEntryStatus.Notchanged)
            {
                // There is not need to copy ServiceOrderRelated's current values
                // to graphServiceOrderEntry.ServiceOrderRecords' current
                // because this graph (AppointmentEntry) just finished persisting the ServiceOrderRelated record.
                // However we mark the graphServiceOrderEntry.ServiceOrderRecords' current record as Updated
                // in order to graphServiceOrderEntry runs all its validations.
                graphServiceOrderEntry.ServiceOrderRecords.Cache.SetStatus(fsServiceOrderRow, PXEntryStatus.Updated);
                graphServiceOrderEntry.ServiceOrderRecords.Cache.IsDirty = true;
            }

            if (deletingAppointment == true)
            {
                // Save the ServiceOrder to recalculate its service counts.
                try
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = null;
                    graphServiceOrderEntry.ForceAppointmentCheckings = true;

                    graphServiceOrderEntry.Save.Press();

                    serviceOrderIsAlreadyUpdated = true;
                }
                finally
                {
                    graphServiceOrderEntry.ForceAppointmentCheckings = false;
                }
            }
            else
            {
                PXResultset<FSAppointmentDetService> fsAppointmentDetServiceSet = graphAppointmentEntry.AppointmentDetServices.Select();

                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in fsAppointmentDetServiceSet)
                {
                    InsertUpdateSODet<FSSODetService, FSAppointmentDetService>(graphAppointmentEntry.AppointmentDetServices.Cache, fsAppointmentDetServiceRow, graphServiceOrderEntry.ServiceOrderDetServices, fsAppointmentRow.Status);
                }

                PXResultset<FSAppointmentDetPart> fsAppointmentDetPartSet = graphAppointmentEntry.AppointmentDetParts.Select();

                foreach (FSAppointmentDetPart fsAppointmentDetPartRow in fsAppointmentDetPartSet)
                {
                    InsertUpdateSODet<FSSODetPart, FSAppointmentDetPart>(graphAppointmentEntry.AppointmentDetParts.Cache, fsAppointmentDetPartRow, graphServiceOrderEntry.ServiceOrderDetParts, fsAppointmentRow.Status);

                    if (fsAppointmentDetPartRow.Mem_SODetRow.SODetID < 0
                        && string.IsNullOrEmpty(fsAppointmentDetPartRow.LotSerialNbr) == false
                        && fsAppointmentDetPartRow.Mem_SODetRow.LotSerialNbr?.Trim() != fsAppointmentDetPartRow.LotSerialNbr?.Trim()
                        )
                    {
                        graphServiceOrderEntry.ServiceOrderDetParts.Current = graphServiceOrderEntry.ServiceOrderDetParts.Search<FSSODetPart.sODetID>(fsAppointmentDetPartRow.Mem_SODetRow.SODetID);
                        FSSODetPartSplit fsSODetSplitRow = graphServiceOrderEntry.partSplits.Current = graphServiceOrderEntry.partSplits.Select();
                        FSSODetPartSplit fsSODetSplitRowCopy = (FSSODetPartSplit)graphServiceOrderEntry.partSplits.Cache.CreateCopy(fsSODetSplitRow);
                        fsSODetSplitRowCopy.LotSerialNbr = fsAppointmentDetPartRow.LotSerialNbr;
                        graphServiceOrderEntry.partSplits.Update(fsSODetSplitRowCopy);
                    }
                    else if (fsAppointmentDetPartRow.SODetID > 0
                        && string.IsNullOrEmpty(fsAppointmentDetPartRow.LotSerialNbr) == false
                        && fsAppointmentDetPartRow.Mem_SODetRow.LotSerialNbr?.Trim() != fsAppointmentDetPartRow.LotSerialNbr?.Trim())
                    {
                        graphServiceOrderEntry.ServiceOrderDetParts.Current = graphServiceOrderEntry.ServiceOrderDetParts.Search<FSSODetPart.sODetID>(fsAppointmentDetPartRow.SODetID);
                        FSSODetPartSplit fsSODetSplitRow = graphServiceOrderEntry.partSplits.Current = graphServiceOrderEntry.partSplits.Select().Where(x => ((FSSODetPartSplit)x).LotSerialNbr == fsAppointmentDetPartRow.LotSerialNbr).FirstOrDefault();
                        if (fsSODetSplitRow == null)
                        {
                            fsSODetSplitRow = graphServiceOrderEntry.partSplits.Current = graphServiceOrderEntry.partSplits.Select()?.Where(x => string.IsNullOrEmpty(((FSSODetPartSplit)x).LotSerialNbr) == true).FirstOrDefault();

                            if (fsSODetSplitRow != null)
                            {
                                FSSODetPartSplit fsSODetSplitRowCopy = (FSSODetPartSplit)graphServiceOrderEntry.partSplits.Cache.CreateCopy(fsSODetSplitRow);
                                if (fsSODetSplitRow.Qty > 1)
                                {
                                    FSSODetPartSplit newFSSODetSplitRow = (FSSODetPartSplit)graphServiceOrderEntry.partSplits.Cache.CreateCopy(fsSODetSplitRow);
                                    newFSSODetSplitRow.SplitLineNbr = int.MinValue;

                                    fsSODetSplitRowCopy.Qty -= fsAppointmentDetPartRow.BillableQty;
                                    graphServiceOrderEntry.partSplits.Update(fsSODetSplitRowCopy);

                                    newFSSODetSplitRow.LotSerialNbr = fsAppointmentDetPartRow.LotSerialNbr;
                                    newFSSODetSplitRow.Qty = fsAppointmentDetPartRow.BillableQty;
                                    graphServiceOrderEntry.partSplits.Insert(newFSSODetSplitRow);
                                }
                                else
                                {
                                    fsSODetSplitRowCopy.LotSerialNbr = fsAppointmentDetPartRow.LotSerialNbr;
                                    graphServiceOrderEntry.partSplits.Update(fsSODetSplitRowCopy);
                                }
                            }
                        }
                    }
                }

                try
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = graphAppointmentEntry;
                    graphServiceOrderEntry.ForceAppointmentCheckings = forceAppointmentCheckings;

                    //SharedFunctions.CopyAllAttributes(graphServiceOrderEntry, graphServiceOrderEntry.ServiceOrderRecords.Current, graphAppointmentEntry, graphAppointmentEntry.AppointmentRecords.Current, true);

                    if (insertingServiceOrder == true)
                    { 
                        graphServiceOrderEntry.Answers.Select();
                        graphServiceOrderEntry.Answers.CopyAttributes(graphServiceOrderEntry, graphServiceOrderEntry.ServiceOrderRecords.Current, graphAppointmentEntry, graphAppointmentEntry.AppointmentRecords.Current, true);
                        insertingServiceOrder = false;
                    }

                    if ((graphServiceOrderEntry.ForceAppointmentCheckings == true || graphServiceOrderEntry.IsDirty == true))
                    {
                        graphServiceOrderEntry.SelectTimeStamp();
                        graphServiceOrderEntry.Save.Press();
                    }

                    serviceOrderIsAlreadyUpdated = true;
                }
                catch (PXException ex)
                {
                    ReplicateServiceOrderExceptions(graphAppointmentEntry, graphServiceOrderEntry);

                    throw ex;
                }
                finally
                {
                    graphServiceOrderEntry.GraphAppointmentEntryCaller = null;
                    graphServiceOrderEntry.ForceAppointmentCheckings = false;
                }

                if (_appointmentDetSODetRelation == null)
                {
                    _appointmentDetSODetRelation = new Dictionary<object, object>();
                }
                else
                {
                    _appointmentDetSODetRelation.Clear();
                }

                // Take references from FSSODetServices
                foreach (FSAppointmentDetService fsAppointmentDetServiceRow in fsAppointmentDetServiceSet)
                {
                    if (fsAppointmentDetServiceRow.Mem_SODetRow != null)
                    {
                        _appointmentDetSODetRelation[fsAppointmentDetServiceRow] = fsAppointmentDetServiceRow.Mem_SODetRow;
                    }
                }

                // Take references from FSSODetParts
                foreach (FSAppointmentDetPart fsAppointmentDetPartRow in fsAppointmentDetPartSet)
                {
                    if (fsAppointmentDetPartRow.Mem_SODetRow != null)
                    {
                        _appointmentDetSODetRelation[fsAppointmentDetPartRow] = fsAppointmentDetPartRow.Mem_SODetRow;
                    }
                }
            }

            if (deletingAppointment == false)
            {
                if (ServiceOrderRelated.Current != null && graphServiceOrderEntry.ServiceOrderRecords.Current != null
                    && ServiceOrderRelated.Current.SOID == graphServiceOrderEntry.ServiceOrderRecords.Current.SOID)
                {
                    foreach (var fieldName in ServiceOrderRelated.Cache.Fields)
                    {
                        ServiceOrderRelated.Cache.SetValue(ServiceOrderRelated.Current, fieldName,
                                graphServiceOrderEntry.ServiceOrderRecords.Cache.GetValue(graphServiceOrderEntry.ServiceOrderRecords.Current, fieldName));
                    }
                }
            }

            return true;
        }

        protected void InsertUpdateSODet<SODetType, AppointmentDetType>(PXCache cacheAppointmentDet, FSAppointmentDet fsAppointmentDetRow, PXSelectBase<SODetType> viewSODet, string appointmentStatus)
            where SODetType : FSSODet, new()
            where AppointmentDetType : FSAppointmentDet, new()
        {
            // If the AppointmentDet row is being deleted does not update the related SODet neither the ServiceOrder.
            if (cacheAppointmentDet.GetStatus(fsAppointmentDetRow) == PXEntryStatus.Deleted ||
                cacheAppointmentDet.GetStatus(fsAppointmentDetRow) == PXEntryStatus.InsertedDeleted)
            {
                return;
            }

            SODetType fsSODetRow = null;

            if (fsAppointmentDetRow.SODetID != null)
            {
                if (viewSODet is PXSelectBase<FSSODetService>)
                {
                    fsSODetRow = PXSelect<SODetType,
                                                Where<FSSODetService.sODetID, Equal<Required<FSSODetService.sODetID>>>>
                                            .Select(viewSODet.Cache.Graph, fsAppointmentDetRow.SODetID);
                }
                else if (viewSODet is PXSelectBase<FSSODetPart>)
                {
                    fsSODetRow = PXSelect<SODetType,
                                                Where<FSSODetPart.sODetID, Equal<Required<FSSODetPart.sODetID>>>>
                                            .Select(viewSODet.Cache.Graph, fsAppointmentDetRow.SODetID);
                }

                if (fsSODetRow == null || fsSODetRow.SODetID != fsAppointmentDetRow.SODetID)
                {
                    throw new PXException(PXMessages.LocalizeFormatNoPrefix(TX.Error.RECORD_NOT_FOUND, TX.CustomTextFields.SERVICE_ORDER_DETAIL), PXErrorLevel.Error);
                }
            }

            bool insertedUpdated = false;

            if (fsSODetRow == null)
            {
                fsSODetRow = new SODetType();

                fsSODetRow = InsertServicePartLine<SODetType, AppointmentDetType>(
                                                        viewSODet.Cache,
                                                        fsSODetRow,
                                                        cacheAppointmentDet,
                                                        fsAppointmentDetRow,
                                                        noteID: null,
                                                        soDetID: null,
                                                        copyTranDate: true,
                                                        tranDate: fsAppointmentDetRow.TranDate,
                                                        SetValuesAfterAssigningSODetID: true,
                                                        copyingFromQuote: false);
                insertedUpdated = true;
            }
            else
            {
                // When the line is not being inserted, only Status, BranchID, SiteID and SiteLocationID are replicated to FSSODet.

                if (fsSODetRow.BranchID != fsAppointmentDetRow.BranchID)
                {
                    viewSODet.Cache.SetValue<FSSODet.branchID>(fsSODetRow, fsAppointmentDetRow.BranchID);
                    insertedUpdated = true;
                }

                if (fsSODetRow.SiteID != fsAppointmentDetRow.SiteID)
                {
                    fsSODetRow.SiteID = fsAppointmentDetRow.SiteID;
                    insertedUpdated = true;
                }

                if (fsSODetRow.SiteLocationID != fsAppointmentDetRow.SiteLocationID)
                {
                    fsSODetRow.SiteLocationID = fsAppointmentDetRow.SiteLocationID;
                    insertedUpdated = true;
                }

                if(insertedUpdated)
                {
                    viewSODet.Update(fsSODetRow);
                }

                // TODO: and the Status???
            }

            if (fsSODetRow.LineType == ID.LineType_All.SERVICE
                && ID.Status_AppointmentDet.CanBeScheduled(fsSODetRow.Status) == true
                    && ID.Status_Appointment.IsOpen(appointmentStatus))
            {
                // Inserting or updating, this SODet line is being Scheduled in this Appointment.
                if (fsSODetRow.Scheduled != true)
                {
                    viewSODet.Cache.SetValue<FSSODet.scheduled>(fsSODetRow, true);
                    insertedUpdated = true;
                }
            }
            
            if (insertedUpdated == true)
            {
                viewSODet.Cache.Update(fsSODetRow);
            }

            fsAppointmentDetRow.Mem_SODetRow = fsSODetRow;
        }

        protected virtual bool IsThereAnySODetReferenceBeingDeleted<SODetIDType>(PXCache cache)
            where SODetIDType : IBqlField
        {
            // Check if some line is being deleted.
            foreach (object row in cache.Deleted)
            {
                return true;
            }

            // Check if some line is changing its SODet reference.
            foreach (object row in cache.Updated)
            {
                if ((int?)cache.GetValue<SODetIDType>(row) != (int?)cache.GetValueOriginal<SODetIDType>(row))
                {
                    return true;
                }
            }

            // Check if some line is changing its SODet reference.
            foreach (object row in cache.Inserted)
            {
                if ((int?)cache.GetValue<SODetIDType>(row) != (int?)cache.GetValueOriginal<SODetIDType>(row))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual bool IsAppointmentBeingDeleted(int? appointmentID, PXCache cache)
        {
            foreach (FSAppointment fsAppointmentRow in cache.Deleted)
            {
                if (fsAppointmentRow.AppointmentID == appointmentID)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual int ReplicateServiceOrderExceptions(AppointmentEntry graphAppointmentEntry, ServiceOrderEntry graphServiceOrderEntry)
        {
            int errorCount = 0;

            errorCount += SharedFunctions.ReplicateCacheExceptions(
                                graphAppointmentEntry.ServiceOrderRelated.Cache,
                                graphAppointmentEntry.ServiceOrderRelated.Current,
                                graphServiceOrderEntry.ServiceOrderRecords.Cache,
                                graphServiceOrderEntry.ServiceOrderRecords.Current);

            // Replicate exceptions in FSSODetServices
            foreach (FSAppointmentDetService fsAppointmentDetServiceRow in graphAppointmentEntry.AppointmentDetServices.Select())
            {
                if (fsAppointmentDetServiceRow.Mem_SODetRow != null)
                {
                    errorCount += SharedFunctions.ReplicateCacheExceptions(
                                        graphAppointmentEntry.AppointmentDetServices.Cache,
                                        fsAppointmentDetServiceRow,
                                        graphServiceOrderEntry.ServiceOrderDetServices.Cache,
                                        fsAppointmentDetServiceRow.Mem_SODetRow);
                }
            }

            // Replicate exceptions in FSSODetParts
            foreach (FSAppointmentDetPart fsAppointmentDetPartRow in graphAppointmentEntry.AppointmentDetParts.Select())
            {
                if (fsAppointmentDetPartRow.Mem_SODetRow != null)
                {
                    errorCount += SharedFunctions.ReplicateCacheExceptions(
                                        graphAppointmentEntry.AppointmentDetParts.Cache,
                                        fsAppointmentDetPartRow,
                                        graphServiceOrderEntry.ServiceOrderDetParts.Cache,
                                        fsAppointmentDetPartRow.Mem_SODetRow);
                }
            }

            return errorCount;
        }
        
        protected void RestoreOriginalValues(PXCache cache, PXRowPersistedEventArgs e)
        {
            if (_oldRows == null)
            {
                return;
            }

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete && e.TranStatus == PXTranStatus.Aborted)
            {
                object oldRow;

                if (_oldRows.TryGetValue(e.Row, out oldRow) && e.Row.GetType() == oldRow.GetType())
                {
                    cache.RestoreCopy(e.Row, oldRow);
                }
            }
        }
        
        protected void BackupOriginalValues(PXCache cache, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
            {
                if (_oldRows == null)
                {
                    _oldRows = new Dictionary<object, object>();
                }
                
                object oldRow;

                // This is to avoid create multiple DAC instances for the same e.Row.
                if (_oldRows.TryGetValue(e.Row, out oldRow) && oldRow.GetType() == e.Row.GetType())
                {
                    cache.RestoreCopy(oldRow, e.Row);
                }
                else
                {
                    _oldRows[e.Row] = cache.CreateCopy(e.Row);
                }
            }
        }

        private void ValidateRouteDriverDeletionFromRouteDocument(FSAppointmentEmployee fsAppointmentEmployeeRow)
        {
            if (IsAppointmentBeingDeleted(fsAppointmentEmployeeRow.AppointmentID, AppointmentRecords.Cache))
            {
                return;
            }

            if (fsAppointmentEmployeeRow.IsDriver == true
                    && Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.APPOINTMENT)
                         && AppointmentSelected.Current.RouteDocumentID != null)
            {
                FSRouteDocument fsRouteDocumentRow = 
                                    PXSelect<FSRouteDocument,
                                        Where<
                                            FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>
                                    .Select(this, AppointmentSelected.Current.RouteDocumentID);

                throw new PXException(TX.Error.DRIVER_DELETION_ERROR, fsRouteDocumentRow.RefNbr);
            }
        }

        private void SetRequireSerialWarning(PXCache cache, FSAppointmentDet fsAppointmentDetServiceRow)
        {
            if (fsAppointmentDetServiceRow.SMEquipmentID != null)
            {
                FSEquipmentComponent fsEquipmentComponentRows = PXSelect<FSEquipmentComponent,
                                                              Where<
                                                                  FSEquipmentComponent.requireSerial, Equal<True>,
                                                                  And<FSEquipmentComponent.serialNumber, IsNull,
                                                                  And<FSEquipmentComponent.SMequipmentID, Equal<Required<FSEquipmentComponent.SMequipmentID>>>>>>
                                                              .Select(cache.Graph, fsAppointmentDetServiceRow.SMEquipmentID);

                if (fsEquipmentComponentRows != null)
                {
                    cache.RaiseExceptionHandling<FSAppointmentDet.SMequipmentID>(
                                                                            fsAppointmentDetServiceRow,
                                                                            null,
                                                                            new PXSetPropertyException(TX.Warning.REQUIRES_SERIAL_NUMBER, PXErrorLevel.Warning));
                }
            }
        }

        protected virtual void InsertServiceOrderServicesInAppointment(
                                                            PXResultset<FSSODetService> bqlResultSet_SODetService,
                                                            PXCache cacheAppointmentDetService)
        {
            foreach (FSSODetService fsSODetServiceRow in bqlResultSet_SODetService)
            {
                var fsAppointmentDetRow = new FSAppointmentDetService();

                AppointmentEntry.InsertServicePartLine<FSAppointmentDetService, FSSODetService>(
                                                            this.AppointmentDetServices.Cache,
                                                            fsAppointmentDetRow,
                                                            this.Caches[typeof(FSSODetService)],
                                                            fsSODetServiceRow,
                                                            null,
                                                            fsSODetServiceRow.SODetID,
                                                            copyTranDate: false,
                                                            tranDate: fsSODetServiceRow.TranDate,
                                                            SetValuesAfterAssigningSODetID: false,
                                                            copyingFromQuote: false);
            }
        }

        protected virtual void InsertServiceOrderPartsInAppointment(
                                                            PXResultset<FSSODetPart> bqlResultSet_SODetPart,
                                                            PXCache cacheAppointmentDetPart)
        {
            foreach (FSSODetPart fsSODetPartRow in bqlResultSet_SODetPart)
            {
                    var fsAppointmentDetRow = new FSAppointmentDetPart();

                    AppointmentEntry.InsertServicePartLine<FSAppointmentDetPart, FSSODetPart>(
                                                                this.AppointmentDetParts.Cache,
                                                                fsAppointmentDetRow,
                                                                this.Caches[typeof(FSSODetPart)],
                                                                fsSODetPartRow,
                                                                null,
                                                                fsSODetPartRow.SODetID,
                                                                copyTranDate: false,
                                                                tranDate: fsSODetPartRow.TranDate,
                                                                SetValuesAfterAssigningSODetID: false,
                                                                copyingFromQuote: false);
                }
            }

        private void UpdateAppointmentDetService_StaffID(string serviceLineRef, string oldServiceLineRef)
        {
            //Process current lineRef selection
            if (string.IsNullOrEmpty(serviceLineRef) == false)
            {
                var empReferencingLineRef = AppointmentEmployees.Select().AsEnumerable()
                                                                .Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == serviceLineRef);

                var fsAppointmentDetRow = AppointmentDetServices.Select().AsEnumerable()
                                                                .Where(y => ((FSAppointmentDetService)y).LineRef == serviceLineRef);
                
                if (fsAppointmentDetRow.Count() == 1)
                {
                    FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)fsAppointmentDetRow.ElementAt(0);

                    int? numEmployeeLinkedToService = empReferencingLineRef.Count();

                    if (numEmployeeLinkedToService == 1)
                    {
                        fsAppointmentDetServiceRow.StaffID = ((FSAppointmentEmployee)empReferencingLineRef.ElementAt(0)).EmployeeID;
                        AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                    }
                    else
                    {
                        fsAppointmentDetServiceRow.StaffID = null;
                        AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                    }

                    AppointmentCore.UpdateStaffRelatedUnboundFields(AppointmentDetServices.Current, AppointmentEmployees, numEmployeeLinkedToService);
                }
            }

            //Clean old lineRef selection
            if (string.IsNullOrEmpty(oldServiceLineRef) == false)
            {
                var empReferencingOldLineRef = AppointmentEmployees.Select().AsEnumerable()
                                                                   .Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == oldServiceLineRef);

                var serviceOldLineRef = AppointmentDetServices.Select().AsEnumerable()
                                                              .Where(y => ((FSAppointmentDetService)y).LineRef == oldServiceLineRef);

                if (serviceOldLineRef.Count() == 1)
                {
                    FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)serviceOldLineRef.ElementAt(0);

                    int? numOldEmployeeLinkedToService = empReferencingOldLineRef.Count();

                    if (numOldEmployeeLinkedToService == 1)
                    {
                        fsAppointmentDetServiceRow.StaffID = ((FSAppointmentEmployee)empReferencingOldLineRef.ElementAt(0)).EmployeeID;
                        AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                    }
                    else
                    {
                        fsAppointmentDetServiceRow.StaffID = null;
                        AppointmentDetServices.Update(fsAppointmentDetServiceRow);
                    }

                    AppointmentCore.UpdateStaffRelatedUnboundFields(AppointmentDetServices.Current, AppointmentEmployees, numOldEmployeeLinkedToService);
                }
            }
        }

        private void EnableDisable_StaffID(PXCache cache, FSAppointmentDet fsAppointmentDetRow)
        {
            bool enableStaffID = false;

            if (fsAppointmentDetRow.EnableStaffID == true
                    && fsAppointmentDetRow.LineType == ID.LineType_All.SERVICE)
            {
                enableStaffID = true;
            }

            if (fsAppointmentDetRow.SODetID <= 0)
            {
                enableStaffID = false;
            }

            PXUIFieldAttribute.SetEnabled<FSAppointmentDetService.staffID>(
                                                                    cache,
                                                                    fsAppointmentDetRow,
                                                                    enableStaffID);
        }

        private bool IsAnySignatureAttached(PXCache cache, FSAppointment fsAppointmentRow)
        {
            Guid[] files = PXNoteAttribute.GetFileNotes(cache, fsAppointmentRow);
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();

            foreach (Guid fileID in files)
            {
                FileInfo fileInfoRow = uploadFileMaintenance.GetFileWithNoData(fileID);
                if (fileInfoRow != null && fileInfoRow.FullName.Contains("signature") == true)
                {
                    return true;
                }
            }

            return false;
        }

        private void GenerateSignedReport(PXCache cache, FSAppointment fsAppointmentRow)
        {
            if (IsAnySignatureAttached(cache, fsAppointmentRow) == false)
            {
                return;
            }

            string reportID = ID.ReportID.APPOINTMENT;

            Dictionary<string, string> parameters = new Dictionary<string, string>();

            string srvOrdTypeFieldName = SharedFunctions.GetFieldName<FSAppointment.srvOrdType>();
            string refNbrFieldName = SharedFunctions.GetFieldName<FSAppointment.refNbr>();

            parameters[srvOrdTypeFieldName] = fsAppointmentRow.SrvOrdType;
            parameters[refNbrFieldName] = fsAppointmentRow.RefNbr;

            using (PXTransactionScope ts = new PXTransactionScope())
            {
                using (Report report = PXReportTools.LoadReport(reportID, null))
                {
                    if (report == null)
                    {
                        throw new Exception("Unable to call Acumatica report writter for specified report : " + reportID);
                    }

                    PXReportTools.InitReportParameters(report, parameters, PXSettingProvider.Instance.Default);
                    ReportNode repNode = ReportProcessor.ProcessReport(report);
                    IRenderFilter renderFilter = ReportProcessor.GetRenderer(ReportProcessor.FilterPdf);

                    using (StreamManager streamMgr = new StreamManager())
                    {
                        renderFilter.Render(repNode, null, streamMgr);
                        UploadFileMaintenance graphUploadFile = PXGraph.CreateInstance<UploadFileMaintenance>();

                        string name = string.Format("{0} - {1} - {2}.pdf", fsAppointmentRow.RefNbr, "Signed", DateTime.Now.ToString("MM_dd_yy_hh_mm_ss"));

                        var file = new FileInfo(Guid.NewGuid(), name, null, streamMgr.MainStream.GetBytes());
                        graphUploadFile.SaveFile(file, FileExistsAction.CreateVersion);

                        PXCache<NoteDoc> noteDocCache = new PXCache<NoteDoc>(this);

                        var fileNote = new NoteDoc { NoteID = fsAppointmentRow.NoteID, FileID = file.UID };

                        noteDocCache.Insert(fileNote);
                        noteDocCache.Persist(PXDBOperation.Insert);

                        Guid[] files = PXNoteAttribute.GetFileNotes(cache, fsAppointmentRow);
                        foreach (Guid fileID in files)
                        {
                            FileInfo fileInfoRow = graphUploadFile.GetFileWithNoData(fileID);

                            if (fileInfoRow != null && (fileInfoRow.UID == fsAppointmentRow.CustomerSignedReport || fileInfoRow.FullName.Contains("signature") == true))
                            {
                                UploadFileMaintenance.DeleteFile(fileInfoRow.UID);
                            }
                        }

                        fsAppointmentRow.CustomerSignedReport = file.UID;

                        PXUpdate<
                            Set<FSAppointment.customerSignedReport, Required<FSAppointment.customerSignedReport>>,
                            FSAppointment,
                            Where<
                                FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>
                        .Update(
                                this,
                                fsAppointmentRow.CustomerSignedReport,
                                fsAppointmentRow.AppointmentID);

                        cache.Graph.SelectTimeStamp();
                    }
                }

                ts.Complete();
            }
        }

        private void HandleServiceLineStatusChange(
            ref List<FSAppointmentEmployee> deleteReleatedTimeActivity,
            ref List<FSAppointmentEmployee> createReleatedTimeActivity)
        {
            foreach (FSAppointmentDetService fsAppointmentDetService in AppointmentDetServices.Cache.Updated)
            {
                string oldServiceStatus
                    = AppointmentDetServices.Cache.GetValueOriginal<FSAppointmentDetService.status>(fsAppointmentDetService).ToString();

                if (oldServiceStatus == fsAppointmentDetService.Status)
                {
                    continue;
                }

                if (fsAppointmentDetService.Status == ID.Status_AppointmentDet.CANCELED)
                {
                    this.FillRelatedTimeActivityList(fsAppointmentDetService, ref deleteReleatedTimeActivity);
                }
                else if (oldServiceStatus == ID.Status_AppointmentDet.CANCELED)
                {
                    this.FillRelatedTimeActivityList(fsAppointmentDetService, ref createReleatedTimeActivity);
                }
            }
        }

        private void FillRelatedTimeActivityList(
            FSAppointmentDetService fsAppointmentDetService,
            ref List<FSAppointmentEmployee> fsAppointmentDetEmployeeList)
        {
            var fsAppointmentEmployeeRows = AppointmentEmployees.Select()
                .RowCast<FSAppointmentEmployee>()
                .Where(row => row.ServiceLineRef == fsAppointmentDetService.LineRef
                    && row.Type == BAccountType.EmployeeType);

            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in fsAppointmentEmployeeRows)
            {
                fsAppointmentDetEmployeeList.Add(fsAppointmentEmployeeRow);
            }
        }

        private void SetCurrentAppointmentSalesPersonID(FSServiceOrder fsServiceOrderRow)
        {
            if (fsServiceOrderRow == null)
            {
                return;
            }

            FSSrvOrdType fsSrvOrdTypeRow = ServiceOrderTypeSelected.Current;

            if (fsSrvOrdTypeRow != null
                    && fsSrvOrdTypeRow.SalesPersonID == null)
            {
                CustDefSalesPeople custDefSalesPeopleRow =
                PXSelect<CustDefSalesPeople,
                  Where<CustDefSalesPeople.bAccountID, Equal<Required<CustDefSalesPeople.bAccountID>>,
                  And<CustDefSalesPeople.locationID, Equal<Required<CustDefSalesPeople.locationID>>,
                  And<CustDefSalesPeople.isDefault, Equal<True>>>>>.Select(this, fsServiceOrderRow.CustomerID, fsServiceOrderRow.LocationID);

                if (custDefSalesPeopleRow != null)
                {
                    AppointmentRecords.Current.SalesPersonID = custDefSalesPeopleRow.SalesPersonID;
                    AppointmentRecords.Current.Commissionable = false;
                }
                else
                {
                    AppointmentRecords.Current.SalesPersonID = null;
                    AppointmentRecords.Current.Commissionable = false;
                }
            }
        }

        private void CheckQtyAndLotSerial(PXCache cache, FSAppointmentDetPart fsAppointmentDetPartRow, object newValue)
        {
            if (newValue == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(fsAppointmentDetPartRow.LotSerialNbr) == false
                    && fsAppointmentDetPartRow.LotSerTrack == INLotSerTrack.SerialNumbered
                    && (decimal)newValue > 1m)
            {
                cache.RaiseExceptionHandling<FSAppointmentDetPart.billableQty>(fsAppointmentDetPartRow, newValue, new PXSetPropertyException(TX.Error.QTY_APPOINTMENT_SERIAL_ERROR, PXErrorLevel.Error));
            }
            else if (fsAppointmentDetPartRow.SODetID != null && fsAppointmentDetPartRow.SODetID > 0)
            {
                FSSODet fsSODetRow = PXSelect<FSSODet,
                                     Where<
                                        FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>
                                     .Select(dummyGraph, fsAppointmentDetPartRow.SODetID);

                FSAppointmentDet fsAppointmentDetRowTotal = PXSelectJoinGroupBy<FSAppointmentDet,
                                                            InnerJoin<FSAppointment, On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                                                            Where<
                                                                FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Inventory_Item>,
                                                                And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                                                And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                                And<FSAppointmentDet.appDetID, NotEqual<Required<FSAppointmentDet.appDetID>>>>>>>,
                                                            Aggregate<GroupBy<FSAppointmentDet.sODetID, Sum<FSAppointmentDet.billableQty>>>>
                                                            .Select(dummyGraph, fsAppointmentDetPartRow.SODetID, fsAppointmentDetPartRow.AppDetID);

                decimal? partTotal = fsAppointmentDetRowTotal != null ? fsAppointmentDetRowTotal.BillableQty : 0m;

                if (partTotal + (decimal)newValue > fsSODetRow.BillableQty)
                {
                    cache.RaiseExceptionHandling<FSAppointmentDetPart.billableQty>(fsAppointmentDetPartRow, fsAppointmentDetPartRow.BillableQty, new PXSetPropertyException(TX.Error.QTY_APPOINTMENT_GREATER_THAN_SERVICEORDER, PXErrorLevel.Error));
                }
            }
        }

        private void LoadServiceOrderRelatedAfterStatusChange(FSAppointment fsAppointmentRow)
        {
            LoadServiceOrderRelated(fsAppointmentRow);
            ServiceOrderCore.UpdateServiceOrderUnboundFields(this, ServiceOrderRelated.Current, BillingCycleRelated.Current, dummyGraph, fsAppointmentRow, DisableServiceOrderUnboundFieldCalc);
        }

        public virtual void ChangeStatusSaveAndSkipExternalTaxCalc(FSAppointment fsAppointmentRow)
        {
            var AppointmentEntryExternalTax = this.GetExtension<AppointmentEntryExternalTax>();

            if (AppointmentEntryExternalTax != null)
            {
                bool previousValue = AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave;

                try
                {
                    AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave = true;
                    SharedFunctions.SaveUpdatedChanges(AppointmentRecords.Cache, fsAppointmentRow);
                }
                finally
                {
                    AppointmentEntryExternalTax.SkipExternalTaxCalcOnSave = previousValue;
                }
            }
            else
            {
                SharedFunctions.SaveUpdatedChanges(AppointmentRecords.Cache, fsAppointmentRow);
            }
        }

        /// <summary>
        /// Force calculate external taxes.
        /// When changing status is a good practice to calculate again the taxes. This is because line Qty can be modified.
        /// Also, new lines can be inserted on Details or Logs.
        /// </summary>
        public virtual void ForceExternalTaxCalc()
        {
            this.AppointmentRecords.Cache.SetValueExt<FSAppointment.isTaxValid>(this.AppointmentRecords.Current, false);
            RecalculateExternalTaxes();
        }
        #endregion

        #region Appointment Event Handlers

        protected virtual void FSAppointment_SORefNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (string.IsNullOrEmpty(fsAppointmentRow.SORefNbr))
            {
                if (fsAppointmentRow.SOID == null || fsAppointmentRow.SOID >= 0)
                {
                    fsAppointmentRow.SOID = null;
                    InitServiceOrderRelated(fsAppointmentRow);
                }
            }
            else
            {
                DeleteUnpersistedServiceOrderRelated(fsAppointmentRow);
                SetServiceOrderRelatedBySORefNbr(cache, fsAppointmentRow);
            }

            FSAddress fsAddressInserted = ServiceOrder_Address.Cache.Inserted?.RowCast<FSAddress>().FirstOrDefault();
            FSContact fsContactInserted = ServiceOrder_Contact.Cache.Inserted?.RowCast<FSContact>().FirstOrDefault();

            if (fsAddressInserted != null && fsAddressInserted.AddressID < 0)
                ServiceOrder_Address.Delete(fsAddressInserted);

            if (fsContactInserted != null && fsContactInserted.ContactID < 0)
                ServiceOrder_Contact.Delete(fsContactInserted);


            if (IsCloningAppointment == false && IsGeneratingAppointment == false && fsAppointmentRow.SORefNbr != null)
            {
                Helper.Current = GetFsSelectorHelperInstance;
                PXResultset<FSSODetService> bqlResultSet_SODetService = new PXResultset<FSSODetService>();
                PXResultset<FSSODetPart> bqlResultSet_SODetPart = new PXResultset<FSSODetPart>();

                ServiceOrderCore.GetPendingLines(this, fsAppointmentRow.SOID, ref bqlResultSet_SODetService, ref bqlResultSet_SODetPart);

                InsertServiceOrderServicesInAppointment(bqlResultSet_SODetService, AppointmentDetServices.Cache);
                InsertServiceOrderPartsInAppointment(bqlResultSet_SODetPart, AppointmentDetParts.Cache);
                GetEmployeesFromServiceOrder(fsAppointmentRow);
                GetResourcesFromServiceOrder(fsAppointmentRow);
                GetAttendeesFromServiceOrder(fsAppointmentRow, Helper.Current);

                AttributeListRecords.Current = AttributeListRecords.Select();
                Answers.Current = Answers.Select();
                Answers.CopyAllAttributes(AppointmentRecords.Current, ServiceOrderRelated.Current);
            }
        }

        protected virtual void FSAppointment_ScheduledDateTimeBegin_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = SharedFunctions.GetCustomDateTime(Accessinfo.BusinessDate.Value, PXTimeZoneInfo.Now);
        }

        protected virtual void FSAppointment_ScheduledDateTimeEnd_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || e.NewValue != null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            if (fsAppointmentRow.ScheduledDateTimeBegin.HasValue)
            {
                e.NewValue = fsAppointmentRow.ScheduledDateTimeBegin.Value.AddMinutes(AppointmentDateTimeHelper.DEFAULT_DURATION_MINUTES);
            }
        }

        protected virtual void FSAppointment_ScheduledDateTimeEnd_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (newTime != null)
            {
                // This is to avoid appointments that last more than 1 day
                newTime = SharedFunctions.GetTimeWithSpecificDate(newTime, fsAppointmentRow.ScheduledDateTimeBegin);
                newTime = AdjustDateTimeEnd(fsAppointmentRow.ScheduledDateTimeBegin, newTime);

                e.NewValue = newTime;
            }
        }

        protected virtual void FSAppointment_ExecutionDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || e.NewValue != null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (fsAppointmentRow.ScheduledDateTimeBegin.HasValue)
            {
                e.NewValue = fsAppointmentRow.ScheduledDateTimeBegin.Value.Date;
            }
        }

        protected virtual void FSAppointment_ExecutionDate_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            
            if (fsAppointmentRow.ScheduledDateTimeBegin.Value.Date > fsAppointmentRow.ExecutionDate)
            {
                cache.RaiseExceptionHandling<FSAppointment.executionDate>(
                                                fsAppointmentRow, 
                                                fsAppointmentRow.ExecutionDate, 
                                                new PXSetPropertyException(
                                                            TX.Error.EXECUTION_DATE_MUST_BE_GREATHER_OR_EQUAL_THAN_SCHEDULED_DATE, 
                                                            PXErrorLevel.Warning));
            }

            if ((DateTime?)e.OldValue != fsAppointmentRow.ExecutionDate)
            {
                if (fsAppointmentRow.ActualDateTimeBegin != null)
                {
                    // Assign the date part to ActualDateTimeBegin
                    DateTime? newBegin = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentRow.ActualDateTimeBegin, fsAppointmentRow.ExecutionDate);

                    cache.SetValueExtIfDifferent<FSAppointment.actualDateTimeBegin>(e.Row, newBegin);
                }

                foreach (FSAppointmentDetService fsAppointmentDetRow in AppointmentDetServices.Select())
                {
                    SetAppointmentLineActualDates(AppointmentDetServices.Cache, fsAppointmentDetRow);

                    ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetRow, AppointmentRecords.Current.ExecutionDate);
                    AppointmentDetServices.Update(fsAppointmentDetRow);
                }

                foreach (FSAppointmentDetPart fsAppointmentDetRow in AppointmentDetParts.Select())
                {
                    ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetRow, AppointmentRecords.Current.ExecutionDate);
                    AppointmentDetParts.Update(fsAppointmentDetRow);
                }

                this.CalculateLaborCosts();
            }
        }

        protected virtual void FSAppointment_RouteDocumentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            fsAppointmentRow.Mem_LastRouteDocumentID = (int?)e.OldValue;
        }

        protected virtual void FSAppointment_WFStageID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            WFStageRelated.Current = WFStageRelated.Select(fsAppointmentRow.WFStageID);
        }

        protected virtual void FSAppointment_ActualDateTimeBegin_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (newTime != null)
            {
                // Set the date part equal to Execution date
                e.NewValue = SharedFunctions.GetTimeWithSpecificDate(newTime, fsAppointmentRow.ExecutionDate);
                cache.SetValuePending(e.Row, typeof(FSAppointment.actualDateTimeBegin).Name + "headerNewTime", e.NewValue);
            }
        }

        protected virtual void FSAppointment_HandleManuallyScheduleTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ScheduleField);
        }

        protected virtual void FSAppointment_HandleManuallyActualTime_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ActualField);
        }

        protected virtual void FSAppointment_ActualDateTimeBegin_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            DateTime? newTime = (DateTime?)cache.GetValuePending(e.Row, typeof(FSAppointment.actualDateTimeBegin).Name + "headerNewTime");
            if (newTime != null)
            {
                fsAppointmentRow.ActualDateTimeBegin = newTime;
            }

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeBegin(
                                                                    ServiceOrderTypeSelected.Current,
                                                                    fsAppointmentRow,
                                                                    AppointmentDetServices,
                                                                    AppointmentEmployees,
                                                                    AppointmentDateTimeHelper.Action.Always,
                                                                    AppointmentDateTimeHelper.Level.ServiceLine,
                                                                    cacheAppointmentHeader: null,
                                                                    cacheAppointmentService: null);
            }

            if (fsAppointmentRow.HandleManuallyActualTime == false)
            {
                // This is to move the actual-time block before to turn on the ManualTime flag
                CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ActualField, forceUpdate: true);
                if (SkipManualTimeFlagUpdate == false)
                {
                    fsAppointmentRow.HandleManuallyActualTime = true;
                }
            }

            if (fsAppointmentRow.ActualDateTimeBegin != null && fsAppointmentRow.ActualDateTimeEnd != null)
            {
                // This is to avoid appointments that last more than 1 day
                fsAppointmentRow.ActualDateTimeEnd = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentRow.ActualDateTimeEnd, fsAppointmentRow.ActualDateTimeBegin);
                fsAppointmentRow.ActualDateTimeEnd = AdjustDateTimeEnd(fsAppointmentRow.ActualDateTimeBegin, fsAppointmentRow.ActualDateTimeEnd);
            }
        }

        protected virtual void FSAppointment_ActualDateTimeEnd_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (newTime != null)
            {
                // This is to avoid appointments that last more than 1 day
                newTime = SharedFunctions.GetTimeWithSpecificDate(newTime, fsAppointmentRow.ActualDateTimeBegin);
                newTime = AdjustDateTimeEnd(fsAppointmentRow.ActualDateTimeBegin, newTime);

                e.NewValue = newTime;
                cache.SetValuePending(e.Row, typeof(FSAppointment.actualDateTimeEnd).Name + "newTime", newTime);
            }
        }

        protected virtual void FSAppointment_ActualDateTimeEnd_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            DateTime? newTime = (DateTime?)cache.GetValuePending(e.Row, typeof(FSAppointment.actualDateTimeEnd).Name + "newTime");
            if(newTime != null)
            {
                fsAppointmentRow.ActualDateTimeEnd = newTime;
            }

            if (e.ExternalCall == true)
            {
                if (SkipManualTimeFlagUpdate == false)
                {
                AppointmentDateTimeHelper.UpdateActualDateTimeEnd(
                                                                  ServiceOrderTypeSelected.Current, 
                                                                  fsAppointmentRow, 
                                                                  AppointmentDetServices,
                                                                  AppointmentEmployees,
                                                                  AppointmentDateTimeHelper.Action.Always,
                                                                  AppointmentDateTimeHelper.Level.ServiceLine);
            }
        }
        }

        protected virtual void FSAppointment_Hold_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            if (fsAppointmentRow.Hold == true)
            {
                fsAppointmentRow.Status = ID.Status_Appointment.ON_HOLD;
            }
            else if((bool?)e.OldValue == true && fsAppointmentRow.Hold == false)
            {
                fsAppointmentRow.Status = fsAppointmentRow.GeneratedBySystem == true ? 
                                                    ID.Status_Appointment.AUTOMATIC_SCHEDULED : ID.Status_Appointment.MANUAL_SCHEDULED;
            }
        }

        protected virtual void FSAppointment_Confirmed_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            #region Confirmed & UnreachedCustomer flag exlusivity
            if (fsAppointmentRow.Confirmed == true && fsAppointmentRow.UnreachedCustomer == true)
            {
                fsAppointmentRow.UnreachedCustomer = false;
            }
            #endregion
        }

        protected virtual void FSAppointment_UnreachedCustomer_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            #region Confirmed & UnreachedCustomer flag exclusivity
            if (fsAppointmentRow.UnreachedCustomer == true && fsAppointmentRow.Confirmed == true)
            {
                fsAppointmentRow.Confirmed = false;
            }
            #endregion
        }

        protected virtual void UpdateManualFlag(PXCache cache, PXFieldUpdatingEventArgs e,
                                                DateTime? currentDateTime, ref bool? manualFlag)
        {
            if (SkipManualTimeFlagUpdate == false)
            {
                // Turning on ManualFlag after any DateTime edition.
                // This is done in the FieldUpdating event instead of the FieldUpdated one
                // to ensure the ManualFlag update before the processing of
                // another FieldUpdated event triggered by PXFormula(typeof(Default<...>)).

                DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

                if (newTime != null && currentDateTime != null)
                {
                    if (SharedFunctions.AreTimePartsEqual(newTime, currentDateTime) == false)
                    {
                        manualFlag = true;
                    }
                }
            }
        }

        protected virtual void FSAppointment_ScheduledDateTimeEnd_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            bool? manualFlag = fsAppointmentRow.HandleManuallyScheduleTime;
            UpdateManualFlag(cache, e, fsAppointmentRow.ScheduledDateTimeEnd, ref manualFlag);
            fsAppointmentRow.HandleManuallyScheduleTime = manualFlag;
        }

        protected virtual void FSAppointment_ActualDateTimeEnd_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            bool? manualFlag = fsAppointmentRow.HandleManuallyActualTime;
            UpdateManualFlag(cache, e, fsAppointmentRow.ActualDateTimeEnd, ref manualFlag);
            fsAppointmentRow.HandleManuallyActualTime = manualFlag;
        }

        protected virtual void FSAppointment_ScheduledDateTimeBegin_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (fsAppointmentRow.ScheduledDateTimeBegin != null && fsAppointmentRow.ScheduledDateTimeEnd != null)
            {
                // This is to avoid appointments that last more than 1 day
                fsAppointmentRow.ScheduledDateTimeEnd = SharedFunctions.GetTimeWithSpecificDate(fsAppointmentRow.ScheduledDateTimeEnd, fsAppointmentRow.ScheduledDateTimeBegin);
            }

            DateTime? oldDate = (DateTime?)e.OldValue;
            if (oldDate != fsAppointmentRow.ScheduledDateTimeBegin)
            {
                UncheckUnreachedCustomerByScheduledDate(
                                                    oldDate,
                                                    fsAppointmentRow.ScheduledDateTimeBegin,
                                                    fsAppointmentRow);

                // If the date part changed
                if (oldDate == null || fsAppointmentRow.ScheduledDateTimeBegin == null
                            || oldDate.Value.Date != fsAppointmentRow.ScheduledDateTimeBegin.Value.Date)
                {
                    cache.SetDefaultExt<FSAppointment.executionDate>(e.Row);
                    cache.SetDefaultExt<FSAppointment.billContractPeriodID>(e.Row);

                    AppointmentCore.RefreshSalesPricesInTheWholeDocument(
                                                AppointmentDetServices,
                                                AppointmentDetParts,
                                                PickupDeliveryItems);
                }
            }
        }

        protected virtual void FSAppointment_ScheduledDateTimeEnd_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            UncheckUnreachedCustomerByScheduledDate(
                                                    (DateTime?)e.OldValue,
                                                    fsAppointmentRow.ScheduledDateTimeEnd,
                                                    fsAppointmentRow);
        }

        protected virtual void FSAppointment_BillContractPeriodID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || e.NewValue != null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            FSContractPeriod fsContractPeriodRow = PXSelect<FSContractPeriod,
                                                    Where2<
                                                        Where<
                                                            FSContractPeriod.startPeriodDate, LessEqual<Required<FSContractPeriod.startPeriodDate>>,
                                                                And<FSContractPeriod.endPeriodDate, GreaterEqual<Required<FSContractPeriod.startPeriodDate>>>>,
                                                        And<
                                                            FSContractPeriod.serviceContractID, Equal<Current<FSAppointment.billServiceContractID>>,
                                                            And2<
                                                                Where<FSContractPeriod.status, Equal<FSContractPeriod.status.Active>,
                                                                    Or<FSContractPeriod.status, Equal<FSContractPeriod.status.Pending>>>,
                                                            And<Current<FSBillingCycle.billingBy>, Equal<FSBillingCycle.billingBy.Appointment>>>>>>
                                                    .Select(this, fsAppointmentRow.ScheduledDateTimeBegin?.Date, fsAppointmentRow.ScheduledDateTimeBegin?.Date);

            e.NewValue = fsContractPeriodRow?.ContractPeriodID;
        }

        protected virtual void FSAppointment_BillContractPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            StandardContractPeriod.Current = StandardContractPeriod.Select();
            StandardContractPeriodDetail.Current = StandardContractPeriodDetail.Select();

            foreach (FSAppointmentDetService fsAppointmentDetServiceRow in AppointmentDetServices.Select())
            {
                AppointmentDetServices.Cache.SetDefaultExt<FSAppointmentDetService.contractRelated>(fsAppointmentDetServiceRow);
                AppointmentDetServices.Cache.Update(fsAppointmentDetServiceRow);
            }

            foreach (FSAppointmentDetPart fsAppointmentDetPartRow in AppointmentDetParts.Select())
            {
                AppointmentDetParts.Cache.SetDefaultExt<FSAppointmentDetPart.contractRelated>(fsAppointmentDetPartRow);
                AppointmentDetParts.Cache.Update(fsAppointmentDetPartRow);
            }

            cache.SetValueExt<FSAppointment.hold>(fsAppointmentRow, fsAppointmentRow.BillServiceContractID != null
                                                                        && fsAppointmentRow.BillContractPeriodID == null);
        }

        protected virtual void FSAppointment_BillServiceContractID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
            cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsAppointmentRow);

            FSServiceContract fsServiceContractRow = (FSServiceContract)PXSelectReadonly<FSServiceContract,
                                        Where<
                                            FSServiceContract.serviceContractID, Equal<Required<FSAppointment.billServiceContractID>>>>
                                        .Select(cache.Graph, fsAppointmentRow.BillServiceContractID);

            if (fsServiceContractRow != null && fsServiceContractRow.ServiceContractID != null)
            {
                SkipChangingContract = true;
                ServiceOrderRelated.Cache.SetValueExt<FSServiceOrder.billCustomerID>(ServiceOrderRelated.Current, fsServiceContractRow.BillCustomerID);
                ServiceOrderRelated.Cache.SetValueExt<FSServiceOrder.billLocationID>(ServiceOrderRelated.Current, fsServiceContractRow.BillLocationID);
            }
        }

        protected override void FSAppointment_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            base.FSAppointment_RowInserted(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (fsAppointmentRow.AppointmentID < 0)
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, ServiceOrderRelated.Current, BillingCycleRelated.Current, dummyGraph, fsAppointmentRow, DisableServiceOrderUnboundFieldCalc);
            }
        }

        protected virtual void FSAppointment_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            using (new PXConnectionScope())
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, ServiceOrderRelated.Current, BillingCycleRelated.Current, dummyGraph, fsAppointmentRow, DisableServiceOrderUnboundFieldCalc);
                if(AppointmentSelected.Current != null)
                {
                    AppointmentSelected.Current.AppCompletedBillableTotal = fsAppointmentRow.AppCompletedBillableTotal;
                }  
            }
        }

        protected override void FSAppointment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            base.FSAppointment_RowSelected(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            LoadServiceOrderRelated(fsAppointmentRow);

            if (WFStageRelated?.Current == null)
            {
                WFStageRelated.Current = WFStageRelated.Select(fsAppointmentRow.WFStageID);
            }

            if (cache.GetStatus(fsAppointmentRow) == PXEntryStatus.Updated && fsAppointmentRow.AppointmentID < 0)
            {
                //// TODO: Review this. When the Row is marked as updated the Autonumber is not setting the new number.
                cache.SetStatus(fsAppointmentRow, PXEntryStatus.Inserted);
            }

            CheckActualDateTimes(cache, fsAppointmentRow);
            CheckScheduledDateTimes(cache, fsAppointmentRow, PXDBOperation.Normal);

            PXDefaultAttribute.SetPersistingCheck<FSAppointment.soRefNbr>(cache, fsAppointmentRow, PXPersistingCheck.Nothing);

            EnableDisable_ActionButtons(cache, fsAppointmentRow, ServiceOrderRelated.Current, ServiceOrderTypeSelected.Current, BillingCycleRelated.Current);

            AppointmentCore.EnableDisable_Document(
                                                this,
                                                fsAppointmentRow,
                                                ServiceOrderRelated.Current,
                                                SetupRecord.Current,
                                                BillingCycleRelated.Current,
                                                ServiceOrderRelated,
                                                AppointmentRecords,
                                                AppointmentSelected,
                                                AppointmentDetServices,
                                                AppointmentDetParts,
                                                AppointmentEmployees,
                                                AppointmentResources,
                                                AppointmentAttendees,
                                                PickupDeliveryItems,
                                                ServiceOrder_Contact,
                                                ServiceOrder_Address,
                                                ServiceOrderTypeSelected.Current,
                                                this.SkipTimeCardUpdate,
                                                this.AppointmentRecords.Current.IsCalledFromQuickProcess);

            //The IsRouteAppoinment flag shows/hides the delivery notes tab
            if (ServiceOrderTypeSelected != null && ServiceOrderTypeSelected.Current != null)
            {
                fsAppointmentRow.IsRouteAppoinment = ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT;
            }
            else
            {
                fsAppointmentRow.IsRouteAppoinment = false;
            }

            PXSetPropertyException exception = null;

            if (fsAppointmentRow != null
                    && ServiceOrderRelated.Current != null
                        && fsAppointmentRow.ExecutionDate > ServiceOrderRelated.Current.SLAETA)
            {
                exception = new PXSetPropertyException(TX.Warning.ACTUAL_DATE_AFTER_SLA, PXErrorLevel.Warning);
            }
            
            if (fsAppointmentRow.WaitingForParts == true 
                    && fsAppointmentRow.Status == ID.Status_Appointment.IN_PROCESS)
            {
                cache.RaiseExceptionHandling<FSAppointment.waitingForParts>(
                                        fsAppointmentRow,
                                        fsAppointmentRow.WaitingForParts,
                                        new PXSetPropertyException(
                                            TX.Warning.WAITING_FOR_PARTS,
                                            PXErrorLevel.Warning));
            }

            if (fsAppointmentRow.Finished == false
                    && fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED)
            {
                cache.RaiseExceptionHandling<FSAppointment.finished>(
                                        fsAppointmentRow,
                                        fsAppointmentRow.Finished,
                                        new PXSetPropertyException(
                                            TX.Warning.APPOINTMENT_WAS_NOT_FINISHED,
                                            PXErrorLevel.Warning));
            }

            cache.RaiseExceptionHandling<FSAppointment.executionDate>(fsAppointmentRow, fsAppointmentRow.ExecutionDate, exception);

            HideRooms(fsAppointmentRow, SetupRecord?.Current);
            HideOrShowTabs(fsAppointmentRow);
            HideOrShowTimeCardsIntegration(cache, fsAppointmentRow);
            HideOrShowRouteInfo(fsAppointmentRow);

            ServiceOrderCore.HidePrepayments(Adjustments.View, ServiceOrderRelated.Cache, ServiceOrderRelated.Current, fsAppointmentRow, ServiceOrderTypeSelected.Current);
            createPrepayment.SetEnabled(ServiceOrderRelated.Current != null 
                                        && ServiceOrderRelated.Current.IsPrepaymentEnable == true 
                                        && cache.GetStatus(e.Row) != PXEntryStatus.Inserted
                                        && ServiceOrderTypeSelected.Current?.Active == true);

            AppointmentDateTimeHelper.SetVisible_KeepServiceActualDateTimesCheckBox(ServiceOrderTypeSelected.Current, AppointmentDetServices.Cache);
            AppointmentDateTimeHelper.SetVisible_KeepStaffActualDateTimesCheckBox(ServiceOrderTypeSelected.Current, AppointmentEmployees.Cache);

            if (RouteSetupRecord.Current == null)
            {
                RouteSetupRecord.Current = RouteSetupRecord.Select();
            }

            UpdateAttributes(fsAppointmentRow);

            Caches[typeof(FSContact)].AllowUpdate = ServiceOrderRelated.Current?.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;
            Caches[typeof(FSAddress)].AllowUpdate = ServiceOrderRelated.Current?.AllowOverrideContactAddress == true && Caches[typeof(FSContact)].AllowUpdate == true;

            PXUIFieldAttribute.SetEnabled<FSManufacturer.allowOverrideContactAddress>(ServiceOrderRelated.Cache, ServiceOrderRelated.Current, !(ServiceOrderRelated.Current?.CustomerID == null && ServiceOrderRelated.Current?.ContactID == null));

            EnableDisableDocumentByWorkflowStage(cache, WFStageRelated?.Current);
        }

        protected virtual void FSAppointment_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if(this.IsMobile == true)
            {
                FSAppointment fsAppointmentRow = (FSAppointment)e.Row;
                FSAppointment newFSAppointmentRow = (FSAppointment)e.NewRow;

                if (fsAppointmentRow.SrvOrdType != newFSAppointmentRow.SrvOrdType &&
                    (fsAppointmentRow.SOID < 0 || 
                        fsAppointmentRow.SOID == null))
                {
                    if (ServiceOrderRelated.Current != null)
                    {
                        ServiceOrderRelated.Delete(ServiceOrderRelated.Current);
                    }
                    fsAppointmentRow.SOID = null;
                    newFSAppointmentRow.SOID = null;
                    InitServiceOrderRelated(newFSAppointmentRow);
                }
            }
        }

        protected override void FSAppointment_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            base.FSAppointment_RowUpdated(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ScheduleField);
            CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ActualField);
        }

        protected virtual void FSAppointment_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (CanExecuteAppointmentRowPersisting(cache, e) == false)
            {
                return;
            }

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            LicenseHelper.CheckStaffMembersLicense(cache.Graph, null, null, null);

            if (e.Operation == PXDBOperation.Insert)
            {
                AutoConfirm(fsAppointmentRow);
            }

            if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
            {
                if (AreThereAnyEmployees())
                {
                    ValidateLicenses<FSAppointment.docDesc>(cache, e.Row);
                    ValidateSkills<FSAppointment.docDesc>(cache, e.Row);
                    ValidateGeoZones<FSAppointment.docDesc>(cache, e.Row);
                }

                ValidateRoom(fsAppointmentRow);
                ValidateMaxAppointmentQty(fsAppointmentRow);
                ValidateWeekCode(fsAppointmentRow);

                if (UpdateServiceOrder(fsAppointmentRow, this, e.Row, e.Operation, null) == false)
                {
                    return;
                }

                //Validate Execution Day
                if (fsAppointmentRow.RouteID != null)
                {
                    FSRoute fsRouteRow = PXSelect<FSRoute, Where<FSRoute.routeID, Equal<Required<FSRoute.routeID>>>>.Select(this, fsAppointmentRow.RouteID);
                    DateTime? dummyDateTime = DateTime.Now;
                    bool runValidation = false;

                    if (e.Operation == PXDBOperation.Update)
                    {
                        int? oldRouteID = (int?)cache.GetValueOriginal<FSAppointment.routeID>(fsAppointmentRow);
                        runValidation = oldRouteID != fsAppointmentRow.RouteID;      
                    }

                    if (runValidation || e.Operation == PXDBOperation.Insert)
                    {
                        SharedFunctions.ValidateExecutionDay(fsRouteRow, fsAppointmentRow.ScheduledDateTimeBegin.Value.DayOfWeek, ref dummyDateTime);
                    }                    
                }

                CheckActualDateTimes(cache, fsAppointmentRow);
                CheckScheduledDateTimes(cache, fsAppointmentRow, e.Operation);

                if (string.IsNullOrEmpty(fsAppointmentRow.DocDesc))
                {
                    FillDocDesc(fsAppointmentRow);
                }

                CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ScheduleField);
                CalculateEndTimeWithLinesDuration(cache, fsAppointmentRow, AppointmentDateTimeHelper.DateFieldType.ActualField);

                fsAppointmentRow.ScheduledDateTimeEnd = AdjustDateTimeEnd(fsAppointmentRow.ScheduledDateTimeBegin, fsAppointmentRow.ScheduledDateTimeEnd);
                fsAppointmentRow.ActualDateTimeEnd = AdjustDateTimeEnd(fsAppointmentRow.ActualDateTimeBegin, fsAppointmentRow.ActualDateTimeEnd);

                //When updating from Service Order the ServiceOrderRelated view contains not saved BranchID
                if (SkipServiceOrderUpdate == false)
                {
                    fsAppointmentRow.BranchID = ServiceOrderRelated.Current?.BranchID;
                }                   
                SetTimeRegister(fsAppointmentRow, ServiceOrderTypeSelected.Current, e.Operation);

                ValidateRoomAvailability(cache, fsAppointmentRow);
                ValidateEmployeeAvailability<FSAppointment.docDesc>(fsAppointmentRow, cache, e.Row);

                TimeCardHelper.CheckTimeCardAppointmentApprovalsAndComplete(this, cache, fsAppointmentRow);

                if (SkipTimeCardUpdate == false)
                {
                    var deleteReleatedTimeActivity = new List<FSAppointmentEmployee>();
                    var createReleatedTimeActivity = new List<FSAppointmentEmployee>();

                    if (TimeCardHelper.IsTheTimeCardIntegrationEnabled(this))
                    {
                        this.HandleServiceLineStatusChange(ref deleteReleatedTimeActivity, ref createReleatedTimeActivity);
                    }

                    TimeCardHelper.InsertUpdateDeleteTimeActivities(
                        this,
                        cache,
                        fsAppointmentRow,
                        ServiceOrderRelated.Current,
                        AppointmentEmployees,
                        deleteReleatedTimeActivity,
                        createReleatedTimeActivity);
                }

                bool updateCutOffDate = false;

                if (e.Operation == PXDBOperation.Insert)
                {
                    //Appointment generation by schedule
                    if (ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT
                            && fsAppointmentRow.ScheduleID != null
                                && fsAppointmentRow.RouteID == null)
                    {
                        SetScheduleTimesByContract(fsAppointmentRow);
                    }

                    if (ServiceOrderRelated.Current.ScheduleID != null || ServiceOrderRelated.Current.ServiceContractID != null)
                    {
                        fsAppointmentRow.ScheduleID = ServiceOrderRelated.Current.ScheduleID;
                        fsAppointmentRow.ServiceContractID = ServiceOrderRelated.Current.ServiceContractID;
                    }

                    updateCutOffDate = true;
                }

                if (e.Operation == PXDBOperation.Update)
                {
                    if ((DateTime?)cache.GetValueOriginal<FSAppointment.executionDate>(fsAppointmentRow) != fsAppointmentRow.ExecutionDate)
                    {
                        updateCutOffDate = true;
                    }
                }

                if (updateCutOffDate)
                {
                    fsAppointmentRow.CutOffDate = ServiceOrderCore.GetCutOffDate(this, ServiceOrderRelated.Current.CBID, fsAppointmentRow.ExecutionDate);
                }

                if (AppointmentRecords.Cache.GetStatus(fsAppointmentRow) == PXEntryStatus.Updated)
                {
                    string lastStatus = (string)AppointmentRecords.Cache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow);
                    if (lastStatus != fsAppointmentRow.Status)
                    {
                        AppointmentRecords.Cache.AllowUpdate = true;
                        ServiceOrderRelated.Cache.AllowUpdate = true;
                    }
                }

                AppointmentCore.UpdatePendingPostFlags(fsAppointmentRow, ServiceOrderRelated.Current, AppointmentDetServices, AppointmentDetParts, PickupDeliveryItems);
                AppointmentCore.UpdateWaitingForPartsFlag(fsAppointmentRow, ServiceOrderRelated.Current, AppointmentDetServices, AppointmentDetParts);
            }

            if ((e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update || e.Operation == PXDBOperation.Delete) && AvoidCalculateRouteStats == false)
            {
                int? routeIDOriginal                    = (int?)cache.GetValueOriginal<FSAppointment.routeID>(fsAppointmentRow);
                int? routeID                            = (int?)cache.GetValue<FSAppointment.routeID>(fsAppointmentRow);
                int? routePositionOriginal              = (int?)cache.GetValueOriginal<FSAppointment.routePosition>(fsAppointmentRow);
                int? routePosition                      = (int?)cache.GetValue<FSAppointment.routePosition>(fsAppointmentRow);
                int? routeDocumentOriginal              = (int?)cache.GetValueOriginal<FSAppointment.routeDocumentID>(fsAppointmentRow);
                int? routeDocument                      = (int?)cache.GetValue<FSAppointment.routeDocumentID>(fsAppointmentRow);
                int? estimatedTotalDurationOriginal     = (int?)cache.GetValueOriginal<FSAppointment.estimatedDurationTotal>(fsAppointmentRow);
                int? estimatedTotalDuration             = (int?)cache.GetValue<FSAppointment.estimatedDurationTotal>(fsAppointmentRow);                
                DateTime? scheduleDateTimeBegin         = (DateTime?)cache.GetValue<FSAppointment.scheduledDateTimeBegin>(fsAppointmentRow);
                DateTime? scheduleDateTimeBeginOriginal = (DateTime?)cache.GetValueOriginal<FSAppointment.scheduledDateTimeBegin>(fsAppointmentRow);
                DateTime? scheduleDateTimeEnd           = (DateTime?)cache.GetValue<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow);
                DateTime? scheduleDateTimeEndOriginal   = (DateTime?)cache.GetValueOriginal<FSAppointment.scheduledDateTimeEnd>(fsAppointmentRow);

                if (routeIDOriginal != routeID
                    || routePositionOriginal != routePosition
                    || routeDocumentOriginal != routeDocument
                    || scheduleDateTimeBeginOriginal != scheduleDateTimeBegin
                    || scheduleDateTimeEndOriginal != scheduleDateTimeEnd
                    || estimatedTotalDurationOriginal != estimatedTotalDuration
                    || (e.Operation == PXDBOperation.Delete && routeIDOriginal != null))
                {
                    NeedRecalculateRouteStats = true;
                }
            }

            fsAppointmentRow.TotalAttendees = AppointmentAttendees.Select().Count;

            string statusOriginal = (string)cache.GetValueOriginal<FSAppointment.status>(fsAppointmentRow);
            if ((statusOriginal == ID.Status_Appointment.IN_PROCESS
                    && fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED)
                        || (statusOriginal == ID.Status_Appointment.COMPLETED
                            && fsAppointmentRow.Status == ID.Status_Appointment.CLOSED))
            {
                UpdateSOStatusOnAppointmentUpdating = true;
            }

            if (e.Operation == PXDBOperation.Insert)
            {
                SharedFunctions.CopyNotesAndFiles(cache,
                                                  ServiceOrderTypeSelected.Current,
                                                  fsAppointmentRow,
                                                  ServiceOrderRelated.Current.CustomerID,
                                                  ServiceOrderRelated.Current.LocationID);
            }

            if ((e.Operation & PXDBOperation.Command) != PXDBOperation.Delete)
            {
                if (RetakeGeoLocation == true 
                        || fsAppointmentRow.MapLatitude == null 
                        || fsAppointmentRow.MapLongitude == null)
                {
                    RetakeGeoLocation = false;
                    ResetLatLong(ServiceOrderTypeSelected.Current);

                    if (ServiceOrder_Address.Current != null 
                        && ServiceOrder_Address.Current.AddressID < 0 
                        && ServiceOrderRelated.Current != null 
                        && ServiceOrderRelated.Current.ServiceOrderAddressID != ServiceOrder_Address.Current.AddressID)
                    {
                        ServiceOrder_Address.Current = ServiceOrder_Address.Select();
                    }

                    SetGeoLocation(ServiceOrder_Address.Current, ServiceOrderTypeSelected.Current);
                }
            }
        }

        protected virtual void FSAppointment_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            RestoreOriginalValues(cache, e);

            FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

            if (e.TranStatus == PXTranStatus.Completed)
            {
                if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update || e.Operation == PXDBOperation.Delete)
                {
                    //Route generation or assignment
                    if (fsAppointmentRow.RouteID != null && (fsAppointmentRow.RouteDocumentID == null || fsAppointmentRow.RoutePosition == null))
                    {
                        SetAppointmentRouteInfo(cache, fsAppointmentRow, ServiceOrderRelated.Current);
                    }

                    if (e.Operation != PXDBOperation.Delete)
                    {
                        GenerateSignedReport(cache, fsAppointmentRow);
                    }
                }

                if (ServiceOrderTypeSelected.Current != null && ServiceOrderTypeSelected.Current.Behavior == ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT)
                {
                    if (NeedRecalculateRouteStats)
                    {
                        NeedRecalculateRouteStats = false;

                        bool singleStatsOnly = (RouteSetupRecord.Current != null
                                                    && RouteSetupRecord.Current.AutoCalculateRouteStats == false)
                                                        || CalculateGoogleStats == false;

                        CalculateRouteStats(this, fsAppointmentRow, SetupRecord.Current.MapApiKey, singleStatsOnly);
                    }
                }

                if (UpdateSOStatusOnAppointmentUpdating)
                {
                    SetServiceOrderStatusFromAppointment(ServiceOrderRelated.Current, fsAppointmentRow);
                    UpdateSOStatusOnAppointmentUpdating = false;
                }
            }

            if (e.Operation == PXDBOperation.Delete && e.TranStatus == PXTranStatus.Completed)
            {
                bool serviceOrderDeleted = false;

                if (ServiceOrderRelated.Current == null)
                {
                    throw new PXException(TX.Error.SERVICE_ORDER_SELECTED_IS_NULL);
                }

                if (ServiceOrderRelated.Current.CreatedByScreenID == fsAppointmentRow.CreatedByScreenID
                        && (fsAppointmentRow.CreatedByScreenID == ID.ScreenID.APPOINTMENT
                            || fsAppointmentRow.CreatedByScreenID == ID.ScreenID.GENERATE_SERVICE_CONTRACT_APPOINTMENT
                            || fsAppointmentRow.CreatedByScreenID == ID.ScreenID.ROUTE_DOCUMENT_DETAIL
                            || fsAppointmentRow.CreatedByScreenID == ID.ScreenID.WRKPROCESS))
                {
                    PXResultset<FSAppointment> bqlResultSet = PXSelect<FSAppointment,
                                                              Where<
                                                                    FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                                                              .Select(this, fsAppointmentRow.SOID);

                    if (bqlResultSet.Count == 0 && ServiceOrderCore.CanDeleteServiceOrder(this, ServiceOrderRelated.Current))
                    {
                        ServiceOrderCore.DeleteServiceOrder(ServiceOrderRelated.Current, GetLocalServiceOrderEntryGraph(true));
                        serviceOrderDeleted = true;
                    }
                }

                if (serviceOrderDeleted == false)
                {
                    ClearPrepayment(fsAppointmentRow);

                    UpdateServiceOrder(fsAppointmentRow, this, e.Row, e.Operation, e.TranStatus);

                    if (string.IsNullOrEmpty(UpdateSOStatusOnAppointmentDeleting) == false)
                    {
                        setLatestServiceOrderStatusBaseOnAppointmentStatus(ServiceOrderRelated.Current, UpdateSOStatusOnAppointmentDeleting);
                        UpdateSOStatusOnAppointmentDeleting = string.Empty;
                    }
                }
            }

            if (e.TranStatus == PXTranStatus.Open)
            {
                if (updateContractPeriod == true && fsAppointmentRow.BillContractPeriodID != null)
                {
                    int multSign = fsAppointmentRow.Status == ID.Status_Appointment.CLOSED ? 1 : -1;
                    FSServiceContract currentStandardContract = StandardContractRelated.Current;
                    if (currentStandardContract != null && currentStandardContract.RecordType == ID.RecordType_ServiceContract.SERVICE_CONTRACT)
                    {
                        ServiceContractEntry graphServiceContract = PXGraph.CreateInstance<ServiceContractEntry>();
                        graphServiceContract.ServiceContractRecords.Current = graphServiceContract.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsAppointmentRow.BillServiceContractID, ServiceOrderRelated.Current.BillCustomerID);
                        graphServiceContract.ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.contractPeriodID>(graphServiceContract.ContractPeriodFilter.Current);

                        if (graphServiceContract.ContractPeriodFilter.Current != null
                                && graphServiceContract.ContractPeriodFilter.Current.ContractPeriodID != fsAppointmentRow.BillContractPeriodID)
                        {
                            graphServiceContract.ContractPeriodFilter.Cache.SetValueExt<FSContractPeriodFilter.contractPeriodID>(graphServiceContract.ContractPeriodFilter.Current, fsAppointmentRow.BillContractPeriodID);
                        }

                        FSContractPeriodDet fsContractPeriodDetRow;
                        decimal? usedQty = 0;
                        int? usedTime = 0;

                        foreach (FSAppointmentDetService fsAppointmentDetServiceRow in AppointmentDetServices.Select().AsEnumerable().Where(x => ((FSAppointmentDetService)x).ContractRelated == true 
                                                                                                                                    && ((FSAppointmentDetService)x).Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphServiceContract.ContractPeriodDetRecords
                                                        .Search<FSContractPeriodDet.inventoryID,
                                                            FSContractPeriodDet.SMequipmentID,
                                                            FSContractPeriodDet.billingRule>(
                                                            fsAppointmentDetServiceRow.InventoryID,
                                                            fsAppointmentDetServiceRow.SMEquipmentID,
                                                            fsAppointmentDetServiceRow.BillingRule).AsEnumerable().FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCache();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsAppointmentDetServiceRow.CoveredQty) 
                                            + (multSign * fsAppointmentDetServiceRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsAppointmentDetServiceRow.CoveredQty * 60)
                                            + (int?)(multSign * fsAppointmentDetServiceRow.ExtraUsageQty * 60);

                                fsContractPeriodDetRow.UsedQty = fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE ? usedQty : 0m;
                                fsContractPeriodDetRow.UsedTime = fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME ? usedTime : 0;
                            }

                            graphServiceContract.ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                        }

                        graphServiceContract.Save.PressButton();
                    }
                    else if (currentStandardContract != null && currentStandardContract.RecordType == ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
                    {
                        RouteServiceContractEntry graphRouteServiceContractEntry = PXGraph.CreateInstance<RouteServiceContractEntry>();
                        graphRouteServiceContractEntry.ServiceContractRecords.Current = graphRouteServiceContractEntry.ServiceContractRecords.Search<FSServiceContract.serviceContractID>(fsAppointmentRow.BillServiceContractID, ServiceOrderRelated.Current.BillCustomerID);
                        graphRouteServiceContractEntry.ContractPeriodFilter.Cache.SetDefaultExt<FSContractPeriodFilter.contractPeriodID>(graphRouteServiceContractEntry.ContractPeriodFilter.Current);

                        FSContractPeriodDet fsContractPeriodDetRow;
                        decimal? usedQty = 0;
                        int? usedTime = 0;

                        foreach (FSAppointmentDetService fsAppointmentDetServiceRow in AppointmentDetServices.Select().AsEnumerable().Where(x => ((FSAppointmentDetService)x).ContractRelated == true
                                                                                                                                    && ((FSAppointmentDetService)x).Status != ID.Status_AppointmentDet.CANCELED))
                        {
                            fsContractPeriodDetRow = graphRouteServiceContractEntry.ContractPeriodDetRecords
                                                        .Search<FSContractPeriodDet.inventoryID,
                                                            FSContractPeriodDet.SMequipmentID,
                                                            FSContractPeriodDet.billingRule>(
                                                            fsAppointmentDetServiceRow.InventoryID,
                                                            fsAppointmentDetServiceRow.SMEquipmentID,
                                                            fsAppointmentDetServiceRow.BillingRule).AsEnumerable().FirstOrDefault();

                            StandardContractPeriodDetail.Cache.Clear();
                            StandardContractPeriodDetail.Cache.ClearQueryCache();
                            StandardContractPeriodDetail.View.Clear();
                            StandardContractPeriodDetail.Select();

                            if (fsContractPeriodDetRow != null)
                            {
                                usedQty = fsContractPeriodDetRow.UsedQty + (multSign * fsAppointmentDetServiceRow.CoveredQty)
                                            + (multSign * fsAppointmentDetServiceRow.ExtraUsageQty);

                                usedTime = fsContractPeriodDetRow.UsedTime + (int?)(multSign * fsAppointmentDetServiceRow.CoveredQty * 60)
                                            + (int?)(multSign * fsAppointmentDetServiceRow.ExtraUsageQty * 60);

                                fsContractPeriodDetRow.UsedQty = fsContractPeriodDetRow.BillingRule == ID.BillingRule.FLAT_RATE ? usedQty : 0m;
                                fsContractPeriodDetRow.UsedTime = fsContractPeriodDetRow.BillingRule == ID.BillingRule.TIME ? usedTime : 0;
                            }

                            graphRouteServiceContractEntry.ContractPeriodDetRecords.Update(fsContractPeriodDetRow);
                        }

                        graphRouteServiceContractEntry.Save.PressButton();
                    }

                    AppointmentEntry graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                    var appointmentsRelatedToSameBillingPeriod = PXSelectJoin<FSAppointment,
                                                                    InnerJoin<FSServiceOrder,
                                                                        On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>,
                                                                    Where<
                                                                        FSServiceOrder.billCustomerID, Equal<Required<FSServiceOrder.billCustomerID>>,
                                                                        And<FSAppointment.billServiceContractID, Equal<Required<FSAppointment.billServiceContractID>>,
                                                                        And<FSAppointment.billContractPeriodID, Equal<Required<FSAppointment.billContractPeriodID>>,
                                                                        And<FSAppointment.status, NotEqual<FSAppointment.status.Closed>,
                                                                        And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                                        And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>>>>>
                                                                        .Select(graphAppointmentEntry,
                                                                                ServiceOrderRelated.Current?.BillCustomerID,
                                                                                fsAppointmentRow.BillServiceContractID,
                                                                                fsAppointmentRow.BillContractPeriodID,
                                                                                fsAppointmentRow.AppointmentID);

                    foreach (FSAppointment fsRelatedAppointmentRow in appointmentsRelatedToSameBillingPeriod)
                    {
                        graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords
                                    .Search<FSServiceOrder.refNbr>(fsRelatedAppointmentRow.RefNbr, fsRelatedAppointmentRow.SrvOrdType);
                        graphAppointmentEntry.AppointmentRecords.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(fsRelatedAppointmentRow);
                        graphAppointmentEntry.Save.PressButton();
                    }
                }
            }

            if (e.TranStatus == PXTranStatus.Completed
                    && (e.Operation == PXDBOperation.Insert
                        || e.Operation == PXDBOperation.Update))
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, ServiceOrderRelated.Current, BillingCycleRelated.Current, dummyGraph, fsAppointmentRow, DisableServiceOrderUnboundFieldCalc);
            }
        }

        protected virtual void FSAppointment_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
        {
            if (AppointmentRecords == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = AppointmentRecords.Current;

            PXResultset<FSAppointment> bqlResultSet = PXSelect<FSAppointment,
                                          Where<
                                                FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>,
                                                And<FSAppointment.appointmentID, NotEqual<Required<FSAppointment.appointmentID>>>>>
                                          .Select(this, fsAppointmentRow.SOID, fsAppointmentRow.AppointmentID);

            if (bqlResultSet.Count > 0 && !ServiceOrderCore.CanDeleteServiceOrder(this, ServiceOrderRelated.Current))
            {
                UpdateSOStatusOnAppointmentDeleting = getFinalServiceOrderStatus(ServiceOrderRelated.Current, fsAppointmentRow, string.Empty);
            }
        }
        #endregion

        #region AppointmentEmployeeEventHandlers

        protected virtual void FSAppointmentEmployee_ServiceLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            InventoryItem inventoryItemRow = null;
            FSxService fsxServiceRow = null;
            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            FSAppointment fsAppointmentRow = (FSAppointment)AppointmentRecords.Current;
            string oldServiceLineRef = (string)e.OldValue;

            string pivot = fsAppointmentEmployeeRow.ServiceLineRef;
            FSAppointmentDetService fsAppointmentDetRow = PXSelectJoin<FSAppointmentDetService,
                                                           InnerJoin<FSSODet,
                                                               On<FSSODet.sODetID, Equal<FSAppointmentDetService.sODetID>>>,
                                                           Where<
                                                               FSSODet.lineRef, Equal<Required<FSSODet.lineRef>>,
                                                               And<FSSODet.sOID, Equal<Current<FSAppointment.sOID>>>>>
                                                           .Select(cache.Graph, pivot);

            fsAppointmentEmployeeRow.ActualDateTimeBegin = fsAppointmentDetRow == null ? null : fsAppointmentDetRow.ActualDateTimeBegin;
            fsAppointmentEmployeeRow.ActualDateTimeEnd = fsAppointmentDetRow == null ? null : fsAppointmentDetRow.ActualDateTimeEnd;
            fsAppointmentEmployeeRow.ProjectTaskID = fsAppointmentDetRow?.ProjectTaskID;
            fsAppointmentEmployeeRow.CostCodeID = fsAppointmentDetRow?.CostCodeID;


            if (fsAppointmentDetRow != null)
            {
                inventoryItemRow = SharedFunctions.GetInventoryItemRow(this, fsAppointmentDetRow.InventoryID);
                fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);

                if (SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false)
                {
                    fsAppointmentEmployeeRow.ActualDuration = fsAppointmentEmployeeRow.ServiceLineRef != null ? fsAppointmentDetRow.ActualDuration : 0;
                }

                fsAppointmentEmployeeRow.KeepActualDateTimes = fsAppointmentDetRow.KeepActualDateTimes;
            }

            if (e.ExternalCall == true)
            {
                UpdateAppointmentDetService_StaffID(fsAppointmentEmployeeRow.ServiceLineRef, oldServiceLineRef);

                AppointmentDateTimeHelper.UpdateStaffActualDateTimeBegin(ServiceOrderTypeSelected.Current, AppointmentRecords.Current, AppointmentDetServices, AppointmentEmployees);
            }
        }

        protected virtual void FSAppointmentEmployee_EmployeeID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointment fsAppointmentRow = AppointmentRecords.Current;
            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            fsAppointmentEmployeeRow.Type = SharedFunctions.GetBAccountType(this, fsAppointmentEmployeeRow.EmployeeID);
            fsAppointmentEmployeeRow.ProjectTaskID = fsAppointmentRow?.DfltProjectTaskID;

            if (fsAppointmentEmployeeRow.Type != BAccountType.EmployeeType)
            {
                fsAppointmentEmployeeRow.TrackTime = false;
                fsAppointmentEmployeeRow.EarningType = null;
            }

            cache.SetDefaultExt<FSAppointmentEmployee.laborItemID>(e.Row);
        }

        protected virtual void FSAppointmentEmployee_UnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            decimal? laborCost = CalculateLaborCost(cache, fsAppointmentEmployeeRow, this.AppointmentRecords.Current);

            if (laborCost != null)
            {
                e.NewValue = laborCost;
                e.Cancel = true;
            }
        }

        protected virtual void FSAppointmentEmployee_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            object unitcost;
            cache.RaiseFieldDefaulting<FSAppointmentDetPart.unitCost>(e.Row, out unitcost);

            if (unitcost != null && (decimal)unitcost != 0m)
            {
                e.NewValue = (decimal)unitcost;
                e.Cancel = true;
            }
        }

        protected virtual void FSAppointmentEmployee_LaborItemID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            cache.SetDefaultExt<FSAppointmentEmployee.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentEmployee_ActualDateTimeBegin_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);
            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            
            if (newTime != null)
            {
                if (AppointmentRecords.Current != null)
                {
                    // Set the date part equal to ExecutionDate
                    newTime = SharedFunctions.GetTimeWithSpecificDate(newTime, AppointmentRecords.Current.ExecutionDate);

                    e.NewValue = newTime;
                    cache.SetValuePending(e.Row, typeof(FSAppointmentEmployee.actualDateTimeBegin).Name + "newTime", newTime);
                    cache.SetValuePending(e.Row, typeof(FSAppointmentEmployee.actualDuration).Name + "newTimeDuration", fsAppointmentEmployeeRow.ActualDuration);
                }
            }
        }

        protected virtual void FSAppointmentEmployee_ActualDateTimeBegin_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            if (fsAppointmentEmployeeRow.ActualDateTimeBegin == null)
            {
                DateTime? newTime = (DateTime?)cache.GetValuePending(e.Row, typeof(FSAppointmentEmployee.actualDateTimeBegin).Name + "newTime");
                if (newTime != null)
                {
                    fsAppointmentEmployeeRow.ActualDateTimeBegin = newTime;
                }
            }

            AppointmentDateTimeHelper.UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);

                AppointmentDateTimeHelper.UpdateServiceActualTimeBeginFromStaff(cache, AppointmentDetServices, fsAppointmentEmployeeRow);
            }
        }

        protected virtual void FSAppointmentEmployee_ActualDateTimeEnd_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);

                AppointmentDateTimeHelper.UpdateServiceActualTimeEndFromStaff(cache, AppointmentDetServices, fsAppointmentEmployeeRow);
            }
        }

        protected virtual void FSAppointmentEmployee_ActualDuration_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
        }

            int? newTimeDuration = (int?)cache.GetValuePending(e.Row, typeof(FSAppointmentEmployee.actualDuration).Name + "newTimeDuration");

            if (newTimeDuration.HasValue == true)
            {
                e.NewValue = newTimeDuration;
                e.Cancel = true;
            }
        }
        protected virtual void FSAppointmentEmployee_ActualDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateStaffActualDateTimeEndFromActualDuration(fsAppointmentEmployeeRow);

                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);
            }
        }

        protected virtual void FSAppointmentEmployee_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            if (fsAppointmentEmployeeRow.LineRef == null)
            {
                fsAppointmentEmployeeRow.LineRef = fsAppointmentEmployeeRow.LineNbr.Value.ToString("000");
            }
        }

        protected virtual void FSAppointmentEmployee_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;

            if (AppointmentRecords.Current != null
                        && fsAppointmentEmployeeRow.ActualDateTimeBegin != null)
            {
                fsAppointmentEmployeeRow.ActualDateTimeBegin = AppointmentRecords.Current.ActualDateTimeBegin != null ?
                                                                SharedFunctions.GetCustomDateTime(AppointmentRecords.Current.ActualDateTimeBegin, fsAppointmentEmployeeRow.ActualDateTimeBegin) :
                                                                SharedFunctions.GetCustomDateTime(AppointmentRecords.Current.ScheduledDateTimeBegin, fsAppointmentEmployeeRow.ActualDateTimeBegin);
            }

            if (AppointmentRecords.Current != null
                        && fsAppointmentEmployeeRow.ActualDateTimeEnd != null)
            {
                fsAppointmentEmployeeRow.ActualDateTimeEnd = AppointmentRecords.Current.ActualDateTimeEnd != null ?
                                                                fsAppointmentEmployeeRow.ActualDateTimeEnd :
                                                                SharedFunctions.GetCustomDateTime(AppointmentRecords.Current.ScheduledDateTimeEnd, fsAppointmentEmployeeRow.ActualDateTimeEnd);
            }

            AppointmentCore.SetPersisting_TimeRelatedFields(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, AppointmentRecords.Current, fsAppointmentEmployeeRow);
        }

        protected virtual void FSAppointmentEmployee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            FSAppointment fsAppointmentRow = AppointmentSelected.Current;

            AppointmentCore.EnableDisable_StaffRelatedFields(cache, fsAppointmentEmployeeRow);
            AppointmentCore.EnableDisable_TimeRelatedFields(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, AppointmentRecords.Current, fsAppointmentEmployeeRow);
            AppointmentCore.SetVisible_TimeRelatedFields(cache, ServiceOrderTypeSelected.Current, AppointmentRecords.Current, fsAppointmentEmployeeRow);
            AppointmentCore.SetPersisting_TimeRelatedFields(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, AppointmentRecords.Current, fsAppointmentEmployeeRow);

            EnableDisable_MobileActions(cache, fsAppointmentRow, fsAppointmentEmployeeRow);
        }

        protected virtual void FSAppointmentEmployee_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            ValidateRouteDriverDeletionFromRouteDocument(fsAppointmentEmployeeRow);
            UpdateAppointmentDetService_StaffID(fsAppointmentEmployeeRow.ServiceLineRef, null);
        }

        protected virtual void FSAppointmentEmployee_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            MarkHeaderAsUpdated(cache, e.Row);

            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            cache.SetDefaultExt<FSAppointmentEmployee.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentEmployee_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentEmployee fsAppointmentEmployeeRow = (FSAppointmentEmployee)e.Row;
            UpdateAppointmentDetService_StaffID(fsAppointmentEmployeeRow.ServiceLineRef, null);
        }

        #endregion

        #region AppointmentResourceEventHandlers

        protected virtual void FSAppointmentResource_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentResource fsAppointmentResourceRow = (FSAppointmentResource)e.Row;

            bool enableDisableFields = fsAppointmentResourceRow.SMEquipmentID == null;
                        
            PXUIFieldAttribute.SetEnabled<FSAppointmentResource.SMequipmentID>
                    (cache, fsAppointmentResourceRow, enableDisableFields);

            PXUIFieldAttribute.SetEnabled<FSAppointmentResource.qty>
                    (cache, fsAppointmentResourceRow, !enableDisableFields);

            PXUIFieldAttribute.SetEnabled<FSAppointmentResource.comment>
                    (cache, fsAppointmentResourceRow, !enableDisableFields);
        }

        [Obsolete("If this method is empty on 2019r2 please delete it.")]
        protected virtual void FSAppointmentResource_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }
        }
        #endregion

        #region AppointmentDetEventHandlers

        #region AppointmentDetService EventHandlers
        protected virtual void FSAppointmentDetService_ActualDateTimeBegin_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (AppointmentRecords.Current != null && newTime != null)
            {
                // Set the date part equal to ExecutionDate
                newTime = SharedFunctions.GetTimeWithSpecificDate(newTime, AppointmentRecords.Current.ExecutionDate);

                e.NewValue = newTime;

                cache.SetValuePending(e.Row, typeof(FSAppointmentDetService.actualDateTimeBegin).Name + "serviceNewTime", e.NewValue);
            }
        }

        protected virtual void FSAppointmentDetService_ActualDateTimeEnd_Time_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.Row == null || e.NewValue == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;

            DateTime? newTime = SharedFunctions.TryParseHandlingDateTime(cache, e.NewValue);

            if (newTime != null)
            {
                // This is to avoid appointments that last more than 1 day
                e.NewValue = SharedFunctions.GetTimeWithSpecificDate(newTime, fsAppointmentDetRow.ActualDateTimeBegin);
            }
        }

        protected virtual void FSAppointmentDetService_IsPrepaid_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSAppointmentDetService,
                                FSAppointmentDetService.manualPrice, FSAppointmentDetService.isBillable,
                                FSAppointmentDetService.estimatedDuration, FSAppointmentDetService.actualDuration>(
                                            cache,
                                            e,
                                            useActualField: true);
        }

        protected virtual void FSAppointmentDetPart_IsPrepaid_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSAppointmentDetPart,
                                FSAppointmentDetPart.manualPrice, FSAppointmentDetPart.isBillable,
                                FSAppointmentDetPart.estimatedDuration, FSAppointmentDetPart.actualDuration>(
                                            cache,
                                            e,
                                            useActualField: true);
        }

        protected virtual void FSAppointmentInventoryItem_IsPrepaid_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_IsPrepaid_FieldUpdated<FSAppointmentInventoryItem,
                                FSAppointmentInventoryItem.manualPrice, FSAppointmentInventoryItem.isBillable,
                                FSAppointmentInventoryItem.estimatedDuration, FSAppointmentInventoryItem.actualDuration>(
                                            cache,
                                            e,
                                            useActualField: true);
        }

        protected virtual void FSAppointmentDetService_ManualPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSAppointmentDetService, FSAppointmentDetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetPart_ManualPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSAppointmentDetPart, FSAppointmentDetPart.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentInventoryItem_ManualPrice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_ManualPrice_FieldUpdated<FSAppointmentInventoryItem, FSAppointmentInventoryItem.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetService_ContractRelated_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null || ServiceOrderTypeSelected.Current == null || AppointmentSelected.Current == null)
            {
                return;
            }

            if (BillingCycleRelated.Current == null ||
               BillingCycleRelated.Current.BillingBy != ID.Billing_By.APPOINTMENT ||
               AppointmentRecords.Current.BillServiceContractID == null ||
               AppointmentRecords.Current.BillContractPeriodID == null)
            {
                e.NewValue = false;
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            FSAppointmentDetService fsSODetDuplicatedByContract = AppointmentDetServices.Search<FSAppointmentDet.inventoryID, FSAppointmentDet.SMequipmentID, FSAppointmentDet.billingRule, FSAppointmentDet.contractRelated>
                (fsAppointmentDetServiceRow.InventoryID, fsAppointmentDetServiceRow.SMEquipmentID, fsAppointmentDetServiceRow.BillingRule, true);
            bool duplicatedContractLine = fsSODetDuplicatedByContract != null && fsSODetDuplicatedByContract.LineID != fsAppointmentDetServiceRow.LineID;

            e.NewValue = duplicatedContractLine == false
                            && StandardContractPeriodDetail.Select().AsEnumerable().Where(x => ((FSContractPeriodDet)x).InventoryID == fsAppointmentDetServiceRow.InventoryID
                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsAppointmentDetServiceRow.SMEquipmentID
                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                && ((FSContractPeriodDet)x).BillingRule == fsAppointmentDetServiceRow.BillingRule).Count() == 1;
        }

        protected virtual void FSAppointmentDetService_CoveredQty_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (BillingCycleRelated.Current == null ||
                BillingCycleRelated.Current.BillingBy != ID.Billing_By.APPOINTMENT ||
                fsAppointmentDetServiceRow.ContractRelated == false)
            {
                e.NewValue = 0m;
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().AsEnumerable().Where(x => ((FSContractPeriodDet)x).InventoryID == fsAppointmentDetServiceRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsAppointmentDetServiceRow.SMEquipmentID
                                                                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == fsAppointmentDetServiceRow.SMEquipmentID)
                                                                                                                                && ((FSContractPeriodDet)x).BillingRule == fsAppointmentDetServiceRow.BillingRule).FirstOrDefault();

            if (fsContractPeriodDetRow != null)
            {
                int? pivotDuration = AppointmentSelected.Current.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                                        || AppointmentSelected.Current.Status == ID.Status_Appointment.MANUAL_SCHEDULED ? fsAppointmentDetServiceRow.EstimatedDuration : fsAppointmentDetServiceRow.ActualDuration;

                decimal? pivotQty = AppointmentSelected.Current.Status == ID.Status_Appointment.AUTOMATIC_SCHEDULED
                                        || AppointmentSelected.Current.Status == ID.Status_Appointment.MANUAL_SCHEDULED ? fsAppointmentDetServiceRow.EstimatedQty : fsAppointmentDetServiceRow.Qty;

                if (fsAppointmentDetServiceRow.BillingRule == ID.BillingRule.TIME)
                {
                    e.NewValue = fsContractPeriodDetRow.RemainingTime - pivotDuration >= 0 ? pivotQty : fsContractPeriodDetRow.RemainingTime / 60;
                }
                else
                {
                    e.NewValue = fsContractPeriodDetRow.RemainingQty - pivotQty >= 0 ? pivotQty : fsContractPeriodDetRow.RemainingQty;
                }
            }
            else
            {
                e.NewValue = 0m;
            }
        }

        protected virtual void FSAppointmentDetService_CuryExtraUsageUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (BillingCycleRelated.Current == null ||
                BillingCycleRelated.Current.BillingBy != ID.Billing_By.APPOINTMENT ||
                fsAppointmentDetServiceRow.ContractRelated == false)
            {
                e.NewValue = 0m;
                return;
            }

            FSContractPeriodDet fsContractPeriodDetRow = (FSContractPeriodDet)StandardContractPeriodDetail.Select().AsEnumerable().Where(x => ((FSContractPeriodDet)x).InventoryID == fsAppointmentDetServiceRow.InventoryID
                                                                                                                                && (((FSContractPeriodDet)x).SMEquipmentID == fsAppointmentDetServiceRow.SMEquipmentID
                                                                                                                                        || ((FSContractPeriodDet)x).SMEquipmentID == null)
                                                                                                                                && ((FSContractPeriodDet)x).BillingRule == fsAppointmentDetServiceRow.BillingRule).FirstOrDefault();

            if (fsContractPeriodDetRow != null)
            {
                e.NewValue = fsContractPeriodDetRow.OverageItemPrice;
            }
            else
            {
                e.NewValue = 0m;
            }
        }

        protected virtual void FSAppointmentDetService_BillingRule_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_BillingRule_FieldVerifying<FSAppointmentDetService>(cache, e);
        }

        protected virtual void FSAppointmentDetService_BillingRule_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_BillingRule_FieldUpdated<FSAppointmentDetService,
                                    FSAppointmentDetService.estimatedDuration, FSAppointmentDetService.actualDuration,
                                    FSAppointmentDetService.curyUnitPrice>(
                                                cache,
                                                e,
                                                useActualField: true);
        }

        protected virtual void FSAppointmentDetService_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSAppointmentDetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetPart_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSAppointmentDetPart.curyUnitPrice>(cache, e);
            cache.SetDefaultExt<FSAppointmentDetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentInventoryItem_UOM_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldUpdated<FSAppointmentInventoryItem.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetService_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSAppointmentDetService.curyUnitPrice, FSAppointmentDetService.acctID, FSAppointmentDetService.subID>(cache, e);
        }

        protected virtual void FSAppointmentDetPart_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSAppointmentDetPart.curyUnitPrice, FSAppointmentDetPart.acctID, FSAppointmentDetPart.subID>(cache, e);
            cache.SetDefaultExt<FSAppointmentDetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentInventoryItem_SiteID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SiteID_FieldUpdated<FSAppointmentInventoryItem.curyUnitPrice, FSAppointmentInventoryItem.acctID, FSAppointmentInventoryItem.subID>(cache, e);
        }

        protected virtual void FSAppointmentDetService_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSAppointmentDetService)e.Row;
            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.Current;

            DateTime? docDate = AppointmentSelected.Current.ScheduledDateTimeBegin;
            if (docDate != null)
            {
                // Remove the time part
                docDate = new DateTime(docDate.Value.Year, docDate.Value.Month, docDate.Value.Day);
            }

            var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, AppointmentSelected.Current.CuryInfoID);

            // Appointment Service lines handle EstimatedQty so the price is based on EstimatedQty and not on Qty.
            ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSAppointmentDetService, FSAppointmentDetService.curyUnitPrice>(
                                                    cache,
                                                    e,
                                                    row.EstimatedQty,
                                                    docDate,
                                                    fsServiceOrderRow,
                                                    AppointmentRecords.Current,
                                                    currencyInfo);
        }

        protected virtual void FSAppointmentDetPart_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSAppointmentDetPart)e.Row;
            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.Current;

            DateTime? docDate = AppointmentSelected.Current.ScheduledDateTimeBegin;
            if (docDate != null)
            {
                // Remove the time part
                docDate = new DateTime(docDate.Value.Year, docDate.Value.Month, docDate.Value.Day);
            }

            var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, AppointmentSelected.Current.CuryInfoID);

            // Appointment Part lines handle EstimatedQty so the price is based on EstimatedQty and not on Qty.
            ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSAppointmentDetPart, FSAppointmentDetPart.curyUnitPrice>(
                                                    cache,
                                                    e,
                                                    row.EstimatedQty,
                                                    docDate,
                                                    fsServiceOrderRow,
                                                    AppointmentRecords.Current,
                                                    currencyInfo);
        }

        protected virtual void FSAppointmentInventoryItem_CuryUnitPrice_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var row = (FSAppointmentInventoryItem)e.Row;
            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.Current;

            DateTime? docDate = AppointmentSelected.Current.ScheduledDateTimeBegin;
            if (docDate != null)
            {
                // Remove the time part
                docDate = new DateTime(docDate.Value.Year, docDate.Value.Month, docDate.Value.Day);
            }

            var currencyInfo = ExtensionHelper.SelectCurrencyInfo(currencyInfoView, AppointmentSelected.Current.CuryInfoID);

            // Currently PickupDelivery lines don't handle EstimatedQty so the price is based on Qty.
            ServiceOrderAppointmentHandlers.X_CuryUnitPrice_FieldDefaulting<FSAppointmentInventoryItem, FSAppointmentInventoryItem.curyUnitPrice>(
                                                    cache,
                                                    e,
                                                    row.Qty,
                                                    docDate,
                                                    fsServiceOrderRow,
                                                    AppointmentRecords.Current,
                                                    currencyInfo);
        }

        protected virtual void FSAppointmentDetService_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            FSServiceOrder fsServiceOrderRow = ServiceOrderRelated.Current;

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSAppointmentDetService, FSAppointmentDetService.subItemID,
                                                FSAppointmentDetService.siteID, FSAppointmentDetService.siteLocationID, FSAppointmentDetService.uOM,
                                                FSAppointmentDetService.estimatedDuration, FSAppointmentDetService.estimatedQty, FSAppointmentDetService.billingRule,
                                                FSAppointmentDetService.actualDuration, FSAppointmentDetService.qty>(
                                                        cache,
                                                        e,
                                                        fsServiceOrderRow.BranchLocationID,
                                                        TaxCustomer.Current,
                                                        useActualFields: true);

            fsAppointmentDetServiceRow.ServiceType = null;
            if (fsAppointmentDetServiceRow.LineType == ID.LineType_All.SERVICE)
            {
                InventoryItem inventoryItemRow = SharedFunctions.GetInventoryItemRow(cache.Graph, fsAppointmentDetServiceRow.InventoryID);
                if (inventoryItemRow != null)
                {
                    FSxService fsxServiceRow = PXCache<InventoryItem>.GetExtension<FSxService>(inventoryItemRow);
                    fsAppointmentDetServiceRow.ServiceType = fsxServiceRow.ActionType;
                }
            }

            TurnOnKeepTimeFlagsAndNotifyEndTimesChange(AppointmentRecords.Cache, AppointmentRecords.Current);
            
            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateServiceActualDateTimeBegin(ServiceOrderTypeSelected.Current, AppointmentRecords.Current, AppointmentDetServices);
                AppointmentDateTimeHelper.UpdateServiceActualDateTimeEnd(ServiceOrderTypeSelected.Current, AppointmentRecords.Current, AppointmentDetServices);
            }
            else
            {
                //In case ExternalCall == false (any change not coming from Appointment's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSAppointmentDetService.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }

            cache.SetDefaultExt<FSAppointmentDetService.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentDetService_TranDesc_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;

            if (fsAppointmentDetRow.StaffRelated == true)
            {
                AppointmentEmployees.Cache.SetStatus(AppointmentEmployees.Current, PXEntryStatus.Updated);
            }
        }

        protected virtual void FSAppointmentDetService_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            
            foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in AppointmentEmployees.Select().AsEnumerable().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsAppointmentDetRow.LineRef))
            {
                if (fsAppointmentDetRow.Status == ID.Status_AppointmentDet.CANCELED)
                {
                    fsAppointmentEmployeeRow.TrackTime = false;
                    AppointmentEmployees.Update(fsAppointmentEmployeeRow);
                }
            }

            cache.SetDefaultExt<FSAppointmentDetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentDetPart_Status_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            cache.SetDefaultExt<FSAppointmentDetPart.curyUnitCost>(e.Row);
        }

        protected virtual void FSAppointmentDetService_ProjectTaskID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;

            if (fsAppointmentDetRow.StaffRelated == true)
            {
                AppointmentEmployees.Cache.SetStatus(AppointmentEmployees.Current, PXEntryStatus.Updated);
            }
        }

        protected virtual void FSAppointmentDetService_SODetID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetRow = (FSAppointmentDetService)e.Row;

            AppointmentCore.GetSODetValues<FSAppointmentDetService, FSSODetService>(cache, fsAppointmentDetRow, ServiceOrderRelated.Current, AppointmentRecords.Current);
        }

        protected virtual void FSAppointmentDetService_EstimatedQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSAppointmentDetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetService_Qty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSAppointmentDetService.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetService_ActualDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            var fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            ServiceOrderAppointmentHandlers.X_Duration_FieldUpdated<FSAppointmentDetService, FSAppointmentDetService.qty>(cache, e, fsAppointmentDetServiceRow.ActualDuration);

            AppointmentDateTimeHelper.UpdateServiceActualDateTimeEndFromActualDuration(fsAppointmentDetServiceRow);

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);

                if (fsAppointmentDetServiceRow.StaffRelatedCount == 1)
                {
                    AppointmentDateTimeHelper.UpdateStaffActualTimeFromService(fsAppointmentDetServiceRow, AppointmentEmployees);
                }
            }
        }

        protected virtual void FSAppointmentDetService_ActualDateTimeBegin_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            DateTime? newTime = (DateTime?)cache.GetValuePending(e.Row, typeof(FSAppointmentDetService.actualDateTimeBegin).Name + "serviceNewTime");
            if (newTime != null)
            {
                fsAppointmentDetServiceRow.ActualDateTimeBegin = newTime;
            }

            AppointmentDateTimeHelper.UpdateServiceActualDateTimeEndFromActualDuration(fsAppointmentDetServiceRow);

            if (SharedFunctions.IsAppointmentNotStarted(AppointmentRecords.Current) == false)
            {
                AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(cache, fsAppointmentDetServiceRow);
            }

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);

                if (fsAppointmentDetServiceRow.StaffRelatedCount == 1)
                {
                    AppointmentDateTimeHelper.UpdateStaffActualTimeFromService(fsAppointmentDetServiceRow, AppointmentEmployees);
                }
            }
        }

        protected virtual void FSAppointmentDetService_ActualDateTimeEnd_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (SharedFunctions.IsAppointmentNotStarted(AppointmentRecords.Current) == false)
            {
                AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(cache, fsAppointmentDetServiceRow);
            }

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);

                if (fsAppointmentDetServiceRow.StaffRelatedCount == 1)
                {
                    AppointmentDateTimeHelper.UpdateStaffActualTimeFromService(fsAppointmentDetServiceRow, AppointmentEmployees);
                }
            }
        }

        protected virtual void FSAppointmentDetService_EstimatedDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            ServiceOrderAppointmentHandlers.X_Duration_FieldUpdated<FSAppointmentDetService, FSAppointmentDetService.estimatedQty>(cache, e, fsAppointmentDetServiceRow.EstimatedDuration);

            if (SharedFunctions.IsAppointmentNotStarted(AppointmentRecords.Current) == false)
            {
                if (fsAppointmentDetServiceRow.ActualDateTimeBegin.HasValue)
                {
                    fsAppointmentDetServiceRow.ActualDateTimeEnd = fsAppointmentDetServiceRow.ActualDateTimeBegin.Value.AddMinutes(fsAppointmentDetServiceRow.EstimatedDuration.Value);
                }

                AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(cache, fsAppointmentDetServiceRow);
            }
        }

        protected virtual void FSAppointmentDetService_StaffActualDuration_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (fsAppointmentDetServiceRow.StaffRelatedCount > 1)
            {
                cache.SetValue<FSAppointmentDet.actualDateTimeBegin>(fsAppointmentDetServiceRow, null);
                cache.SetValue<FSAppointmentDet.actualDateTimeEnd>(fsAppointmentDetServiceRow, null);
                cache.SetValue<FSAppointmentDet.keepActualDateTimes>(fsAppointmentDetServiceRow, false);
            }
            else
            {
                if (fsAppointmentDetServiceRow.StaffRelated == true)
                {
                    FSAppointmentEmployee fsAppointmentEmployeeRow = AppointmentEmployees.Select().RowCast<FSAppointmentEmployee>()
                                                                                         .Where(_ => _.ServiceLineRef != null &&
                                                                                                     _.ServiceLineRef.Trim() == fsAppointmentDetServiceRow.LineRef.Trim())
                                                                                         .FirstOrDefault();

                    if (fsAppointmentEmployeeRow != null)
                    {
                        fsAppointmentDetServiceRow.ActualDateTimeBegin = fsAppointmentEmployeeRow.ActualDateTimeBegin;
                        fsAppointmentDetServiceRow.ActualDateTimeEnd = fsAppointmentEmployeeRow.ActualDateTimeEnd;
                    }
                }
            }

            if (SharedFunctions.IsAppointmentNotStarted(AppointmentRecords.Current) == false)
            {
                AppointmentDateTimeHelper.CalculateAppointmentDetServiceActualDuration(cache, fsAppointmentDetServiceRow);
            }
        }

        protected virtual void FSAppointmentDetService_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            if (e.ExternalCall == true)
            {
                ServiceOrderAppointmentHandlers.X_LineType_FieldUpdated<FSAppointmentDetService>(cache, e);
            }
        }

        protected virtual void FSAppointmentDetService_StaffID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (e.ExternalCall == true)
            {
                AppointmentCore.InsertUpdateDelete_AppointmentDetService_StaffID(cache, fsAppointmentDetServiceRow, AppointmentEmployees, (int?)e.OldValue);
            }
        }

        protected virtual void FSAppointmentDetService_SMEquipmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetServiceRow, AppointmentRecords.Current.ExecutionDate);
        }

        protected virtual void FSAppointmentDetService_ComponentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetServiceRow, AppointmentRecords.Current.ExecutionDate);
        }

        protected virtual void FSAppointmentDetService_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetServiceRow, AppointmentRecords.Current.ExecutionDate);

            if (fsAppointmentDetServiceRow.ComponentID == null)
            {
                fsAppointmentDetServiceRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsAppointmentDetServiceRow.SMEquipmentID, fsAppointmentDetServiceRow.EquipmentLineRef);
            }
        }

        protected override void FSAppointmentDetService_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            base.FSAppointmentDetService_RowSelecting(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            AppointmentCore.UpdateStaffRelatedUnboundFields(fsAppointmentDetServiceRow, AppointmentEmployees);
        }

        protected virtual void FSAppointmentDetService_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            FSAppointment fsAppointmentRow = AppointmentSelected.Current;

            AppointmentCore.FSAppointmentDet_RowSelected_PartialHandler(
                                                                        cache,
                                                                        fsAppointmentDetRow,
                                                                        SetupRecord.Current,
                                                                        ServiceOrderTypeSelected.Current,
                                                                        ServiceOrderRelated.Current,
                                                                        fsAppointmentRow);

            SetRequireSerialWarning(cache, fsAppointmentDetRow);
            EnableDisable_StaffID(cache, fsAppointmentDetRow);

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSAppointmentDetService>(
                                            cache,
                                            e,
                                            ServiceOrderRelated.Current,
                                            ServiceOrderTypeSelected.Current,
                                            disableSODetReferenceFields: fsAppointmentDetRow.AppDetID > 0,
                                            docAllowsActualFieldEdition: SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);

            EnableDisable_MobileActions(cache, fsAppointmentRow, fsAppointmentDetRow);
        }

        protected virtual void FSAppointmentDetService_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            //If there is not header
            if (AppointmentRecords.Current == null)
            {
                return;
            }

            BackupOriginalValues(cache, e);

            if (UpdateServiceOrder(AppointmentRecords.Current, this, e.Row, e.Operation, null) == false)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;

            if (_appointmentDetSODetRelation != null)
            {
                object newFSSODet;

                if (_appointmentDetSODetRelation.TryGetValue(fsAppointmentDetRow, out newFSSODet))
                {
                    fsAppointmentDetRow.SODetID = ((FSSODet)newFSSODet).SODetID;
                    fsAppointmentDetRow.LineRef = ((FSSODet)newFSSODet).LineRef;
                }
            }

            AppointmentCore.FSAppointmentDet_RowPersisting_PartialHandler(
                                                                        cache,
                                                                        fsAppointmentDetRow,
                                                                        AppointmentRecords.Current,
                                                                        ServiceOrderTypeSelected.Current);
            string oldValue;
            string originalValue = (string)cache.GetValueOriginal<FSAppointmentDetService.status>(fsAppointmentDetRow);

            if (originalServiceStatus.TryGetValue(fsAppointmentDetRow.SODetID, out oldValue))
            {
                originalServiceStatus[fsAppointmentDetRow.SODetID] = originalValue;
            }
            else
            {
                originalServiceStatus.Add(fsAppointmentDetRow.SODetID, originalValue);
            }

            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSAppointmentDetService>(cache, e, ServiceOrderRelated.Current, ServiceOrderTypeSelected.Current);
        }

        protected virtual void FSAppointmentDetService_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            RestoreOriginalValues(cache, e);

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            AppointmentCore.UpdateStaffRelatedUnboundFields(fsAppointmentDetRow, AppointmentEmployees);

            if (e.TranStatus == PXTranStatus.Completed)
            {
                AppointmentCore.UpdateAppointmentsInfoInServiceOrder(cache, fsAppointmentDetRow, ServiceOrderRelated, originalServiceStatus, e.Operation);
            }
        }

        protected override void FSAppointmentDetService_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }
            
            TurnOnKeepTimeFlagsAndNotifyEndTimesChange(AppointmentSelected.Cache, AppointmentSelected.Current);

            MarkHeaderAsUpdated(cache, e.Row);

            base.FSAppointmentDetService_RowDeleted(cache, e);

            if (e.ExternalCall == true)
            {
                AppointmentDateTimeHelper.UpdateActualDateTimeWithServiceMinMax(
                                                                                ServiceOrderTypeSelected.Current,
                                                                                AppointmentRecords,
                                                                                AppointmentDetServices,
                                                                                AppointmentEmployees);
            }
        }

        protected override void FSAppointmentDetService_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            TurnOnKeepTimeFlagsAndNotifyEndTimesChange(AppointmentSelected.Cache, AppointmentSelected.Current);

            MarkHeaderAsUpdated(cache, e.Row);

            base.FSAppointmentDetService_RowInserted(cache, e);
        }

        protected override void FSAppointmentDetService_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            var newRow = (FSAppointmentDetService)e.Row;
            var oldRow = (FSAppointmentDetService)e.OldRow;

            if (oldRow == null
                    || oldRow.InventoryID != newRow.InventoryID
                    || oldRow.EstimatedDuration != newRow.EstimatedDuration
                    || oldRow.ActualDuration != newRow.ActualDuration)
            {
                bool baseNotifyCondition = oldRow == null || oldRow.InventoryID != newRow.InventoryID;
                TurnOnKeepTimeFlagsAndNotifyEndTimesChange(
                                                           AppointmentSelected.Cache, 
                                                           AppointmentSelected.Current, 
                                                           notifySchedule: baseNotifyCondition || oldRow?.EstimatedDuration != newRow?.EstimatedDuration,
                                                           notifyActual: baseNotifyCondition || oldRow.ActualDuration != newRow.ActualDuration);
            }

            MarkHeaderAsUpdated(cache, e.Row);
            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSAppointmentDetService, FSAppointmentDetService.estimatedQty>(cache, e);

            base.FSAppointmentDetService_RowUpdated(cache, e);
        }

        protected virtual void FSAppointmentDetService_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;
            if (IsAppointmentBeingDeleted(fsAppointmentDetServiceRow.AppointmentID, AppointmentRecords.Cache) == false 
                    && AppointmentCore.ServiceLinkedToPickupDeliveryItem(this, fsAppointmentDetServiceRow, AppointmentRecords.Current) == true)
            {
                throw new PXException(TX.Error.SERVICE_LINKED_TO_PICKUP_DELIVERY_ITEMS);
            }

            FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(cache.Graph, fsAppointmentDetServiceRow);

            if (fsSODetRow != null)
            {
                foreach (FSAppointmentEmployee fsAppointmentEmployeeRow in AppointmentEmployees.Select().AsEnumerable().Where(y => ((FSAppointmentEmployee)y).ServiceLineRef == fsSODetRow.LineRef))
                {
                    AppointmentEmployees.Delete(fsAppointmentEmployeeRow);
                }
            }
        }

        protected virtual void FSAppointmentDetService_AcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSAppointmentDetService>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentDetService_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSAppointmentDetService>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentDetService_UOM_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSAppointmentDetService>(cache, e);
        }

        protected virtual void FSAppointmentDetService_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetService fsAppointmentDetServiceRow = (FSAppointmentDetService)e.Row;

            if (string.IsNullOrEmpty(fsAppointmentDetServiceRow.UOM) == false)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<FSAppointmentDetService.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<FSAppointmentDetService.inventoryID, FSAppointmentDetService.uOM>(cache, fsAppointmentDetServiceRow, (decimal)unitcost, INPrecision.NOROUND);

                    IPXCurrencyHelper currencyHelper = this.FindImplementation<IPXCurrencyHelper>();

                    if (currencyHelper != null)
                    {
                        currencyHelper.CuryConvCury((decimal)unitcost, out newval);
                    }
                    else
                    {
                        CM.PXDBCurrencyAttribute.CuryConvCury(cache, fsAppointmentDetServiceRow, newval, out newval, true);
                    }

                    e.NewValue = Math.Round(newval, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #region AppointmentDetPart EventHandlers
        protected virtual void FSAppointmentDetPart_SODetID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetRow = (FSAppointmentDetPart)e.Row;

            AppointmentCore.GetSODetValues<FSAppointmentDetPart, FSSODetPart>(cache, fsAppointmentDetRow, ServiceOrderRelated.Current, AppointmentRecords.Current);
        }

        protected virtual void FSAppointmentDetPart_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSAppointmentDetPart, FSAppointmentDetPart.subItemID,
                                                FSAppointmentDetPart.siteID, FSAppointmentDetPart.siteLocationID, FSAppointmentDetPart.uOM,
                                                FSAppointmentDetPart.estimatedDuration, FSAppointmentDetPart.estimatedQty, FSAppointmentDetPart.billingRule,
                                                FSAppointmentDetPart.actualDuration, FSAppointmentDetPart.qty>(
                                                        cache,
                                                        e,
                                                        ServiceOrderRelated.Current.BranchLocationID,
                                                        TaxCustomer.Current,
                                                        useActualFields: true);

            SharedFunctions.UpdateEquipmentFields(this, cache, fsAppointmentDetPartRow, fsAppointmentDetPartRow.InventoryID, SharedFunctions.IsAppointmentNotStarted(AppointmentRecords.Current) == false);
            cache.SetDefaultExt<FSAppointmentDetPart.curyUnitCost>(e.Row);

            if (e.ExternalCall == false)
            {
                //In case ExternalCall == false (any change not coming from Appointment's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSAppointmentDetPart.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }
        }

        protected virtual void FSAppointmentDetPart_EstimatedQty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSAppointmentDetPart.curyUnitPrice>(cache, e);
        }

        protected virtual void FSAppointmentDetPart_LineType_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDet fsAppointmentDetRow = (FSAppointmentDet)e.Row;
            if (e.ExternalCall == true)
            {
                ServiceOrderAppointmentHandlers.X_LineType_FieldUpdated<FSAppointmentDetPart>(cache, e);
            }
        }

        protected virtual void FSAppointmentDetPart_EquipmentAction_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;

            SharedFunctions.ResetEquipmentFields(cache, fsAppointmentDetPartRow);
        }

        protected virtual void FSAppointmentDetPart_SMEquipmentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetPartRow, AppointmentRecords.Current.ExecutionDate);
        }

        protected virtual void FSAppointmentDetPart_ComponentID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetPartRow, AppointmentRecords.Current.ExecutionDate);
        }

        protected virtual void FSAppointmentDetPart_EquipmentLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            ServiceOrderCore.UpdateWarrantyFlag(cache, fsAppointmentDetPartRow, AppointmentRecords.Current.ExecutionDate);
            if (fsAppointmentDetPartRow.ComponentID == null)
            {
                fsAppointmentDetPartRow.ComponentID = SharedFunctions.GetEquipmentComponentID(this, fsAppointmentDetPartRow.SMEquipmentID, fsAppointmentDetPartRow.EquipmentLineRef);
            }
        }

        protected virtual void FSAppointmentDetPart_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            FSAppointment fsAppointmentRow = AppointmentSelected.Current;

            AppointmentCore.FSAppointmentDet_RowSelected_PartialHandler(
                                                                        cache, 
                                                                        fsAppointmentDetPartRow, 
                                                                        SetupRecord.Current, 
                                                                        ServiceOrderTypeSelected.Current, 
                                                                        ServiceOrderRelated.Current,
                                                                        fsAppointmentRow);

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSAppointmentDetPart>(
                                            cache,
                                            e,
                                            ServiceOrderRelated.Current,
                                            ServiceOrderTypeSelected.Current,
                                            disableSODetReferenceFields: fsAppointmentDetPartRow.AppDetID > 0,
                                            docAllowsActualFieldEdition: SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
        }

        protected override void FSAppointmentDetPart_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);

            base.FSAppointmentDetPart_RowDeleted(cache, e);
        }

        protected override void FSAppointmentDetPart_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);

            base.FSAppointmentDetPart_RowInserted(cache, e);
        }

        protected virtual void FSAppointmentDetPart_RowUpdating(PXCache cache, PXRowUpdatingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            EquipmentHelper.CheckReplaceComponentLines<FSAppointmentDetPart, FSAppointmentDetPart.equipmentLineRef>(cache, AppointmentDetParts.Select(), (FSAppointmentDetPart)e.NewRow);
        }

        protected override void FSAppointmentDetPart_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);

            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSAppointmentDetPart, FSAppointmentDetPart.estimatedQty>(cache, e);

            base.FSAppointmentDetPart_RowUpdated(cache, e);
            base.VerifyIsAlreadyPosted<FSAppointmentDetPart.inventoryID>(cache, (FSAppointmentDetPart)e.Row, null, BillingCycleRelated.Current);
        }

        protected virtual void FSAppointmentDetPart_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            //If there is not header
            if (AppointmentSelected.Current == null)
            {
                return;
            }

            base.VerifyIsAlreadyPosted<FSAppointmentDetPart.inventoryID>(cache, (FSAppointmentDetPart)e.Row, null, BillingCycleRelated.Current);

            BackupOriginalValues(cache, e);

            if (UpdateServiceOrder(AppointmentSelected.Current, this, e.Row, e.Operation, null) == false)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            FSAppointmentDet fsAppointmentDetRow = fsAppointmentDetPartRow;
            
            string errorMessage = string.Empty;

            if (_appointmentDetSODetRelation != null)
            {
                object newFSSODet;

                if (_appointmentDetSODetRelation.TryGetValue(fsAppointmentDetRow, out newFSSODet))
                {
                    fsAppointmentDetRow.SODetID = ((FSSODet)newFSSODet).SODetID;
                    fsAppointmentDetRow.LineRef = ((FSSODet)newFSSODet).LineRef;
                }
            }

            AppointmentCore.FSAppointmentDet_RowPersisting_PartialHandler(
                                                                        cache,
                                                                        fsAppointmentDetRow,
                                                                        AppointmentRecords.Current,
                                                                        ServiceOrderTypeSelected.Current);
            if (e.Operation != PXDBOperation.Delete
                    && !SharedFunctions.AreEquipmentFieldsValid(cache, fsAppointmentDetRow.InventoryID, fsAppointmentDetRow.SMEquipmentID, fsAppointmentDetRow.NewTargetEquipmentLineNbr, fsAppointmentDetRow.EquipmentAction, ref errorMessage))
            {
                cache.RaiseExceptionHandling<FSAppointmentDetPart.equipmentAction>(fsAppointmentDetRow, fsAppointmentDetRow.EquipmentAction, new PXSetPropertyException(errorMessage));
            }

            if (EquipmentHelper.CheckReplaceComponentLines<FSAppointmentDetPart, FSAppointmentDetPart.equipmentLineRef>(cache, AppointmentDetParts.Select(), (FSAppointmentDetPart)e.Row) == false)
            {
                return;
            }

            string oldValue;
            string originalValue = (string)cache.GetValueOriginal<FSAppointmentDetPart.status>(fsAppointmentDetRow);

            if (originalPartStatus.TryGetValue(fsAppointmentDetRow.SODetID, out oldValue))
            {
                originalPartStatus[fsAppointmentDetRow.SODetID] = originalValue;
            }
            else
            {
                originalPartStatus.Add(fsAppointmentDetRow.SODetID, originalValue);
            }

            CheckQtyAndLotSerial(cache, fsAppointmentDetPartRow, fsAppointmentDetPartRow.BillableQty);
            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSAppointmentDetPart>(cache, e, ServiceOrderRelated.Current, ServiceOrderTypeSelected.Current);
        }

        protected virtual void FSAppointmentDetPart_RowPersisted(PXCache cache, PXRowPersistedEventArgs e)
        {
            RestoreOriginalValues(cache, e);
            FSAppointmentDetPart fsAppointmentDetRow = (FSAppointmentDetPart)e.Row;

            if (e.TranStatus == PXTranStatus.Completed)
            {
                AppointmentCore.UpdateAppointmentsInfoInServiceOrder(cache, fsAppointmentDetRow, ServiceOrderRelated, originalPartStatus, e.Operation);
            }
        }

        protected virtual void FSAppointmentDetPart_AcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSAppointmentDetPart>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentInventoryItem_AcctID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_AcctID_FieldDefaulting<FSAppointmentInventoryItem>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentDetPart_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSAppointmentDetPart>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentInventoryItem_SubID_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_SubID_FieldDefaulting<FSAppointmentInventoryItem>(cache, e,
                                                    ServiceOrderTypeSelected.Current,
                                                    ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentDetPart_UOM_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_UOM_FieldDefaulting<FSAppointmentDetPart>(cache, e);
        }

        protected virtual void FSAppointmentDetPart_LotSerialNbr_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;
            if (string.IsNullOrEmpty(fsAppointmentDetPartRow.LotSerialNbr) == false 
                    &&  fsAppointmentDetPartRow.LotSerTrack == INLotSerTrack.SerialNumbered)
            {
                cache.SetValueExt<FSAppointmentDetPart.estimatedQty>(e.Row, 1m);
            }
        }

        protected virtual void FSAppointmentDetPart_CuryUnitCost_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;

            if (string.IsNullOrEmpty(fsAppointmentDetPartRow.UOM) == false)
            {
                object unitcost;
                cache.RaiseFieldDefaulting<FSAppointmentDetPart.unitCost>(e.Row, out unitcost);

                if (unitcost != null && (decimal)unitcost != 0m)
                {
                    decimal newval = INUnitAttribute.ConvertToBase<FSAppointmentDetPart.inventoryID, FSAppointmentDetPart.uOM>(cache, fsAppointmentDetPartRow, (decimal)unitcost, INPrecision.NOROUND);

                    IPXCurrencyHelper currencyHelper = this.FindImplementation<IPXCurrencyHelper>();

                    if (currencyHelper != null)
                    {
                        currencyHelper.CuryConvCury((decimal)unitcost, out newval);
                    }
                    else
                    {
                        CM.PXDBCurrencyAttribute.CuryConvCury(cache, fsAppointmentDetPartRow, newval, out newval, true);
                    }

                    e.NewValue = Math.Round(newval, CommonSetupDecPl.PrcCst, MidpointRounding.AwayFromZero);
                    e.Cancel = true;
                }
            }
        }

        protected virtual void FSAppointmentDetPart_LotSerialNbr_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentDetPart fsAppointmentDetPartRow = (FSAppointmentDetPart)e.Row;

            int? serialRepeated = PXSelectJoin<FSAppointmentDet,
                                                            InnerJoin<FSAppointment, On<FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
                                                            Where<
                                                                FSAppointmentDet.lineType, Equal<FSAppointmentDet.lineType.Inventory_Item>,
                                                                And<FSAppointment.status, NotEqual<FSAppointment.status.Canceled>,
                                                                And<FSAppointmentDet.status, NotEqual<FSAppointmentDet.status.Canceled>,
                                                                And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
                                                                And<FSAppointmentDet.appDetID, NotEqual<Required<FSAppointmentDet.appDetID>>,
                                                                And<FSAppointmentDet.lotSerialNbr, Equal<Required<FSAppointmentDet.lotSerialNbr>>>>>>>>>
                                                            .Select(new PXGraph(), fsAppointmentDetPartRow.SODetID, fsAppointmentDetPartRow.AppDetID, (string)e.NewValue).Count();

            if (serialRepeated != null && serialRepeated > 0)
            {
                cache.RaiseExceptionHandling<FSAppointmentDetPart.lotSerialNbr>(fsAppointmentDetPartRow, null, new PXSetPropertyException(TX.Error.REPEATED_APPOINTMENT_SERIAL_ERROR, PXErrorLevel.Error));
                e.NewValue = null;
            }
        }
        #endregion

        #endregion

        #region AppointmentAttendeesEvents

        protected virtual void FSAppointmentAttendee_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentAttendee fsAppointmentAttendeeRow = (FSAppointmentAttendee)e.Row;          

            Helper.Current = GetFsSelectorHelperInstance;
            Helper.Current.Mem_int = fsAppointmentAttendeeRow.CustomerID;

            ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsAppointmentAttendeeRow.CustomerID, fsAppointmentAttendeeRow.ContactID);
            fsAppointmentAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
            fsAppointmentAttendeeRow.Mem_EMail = attendeeInfo.EMail;
            fsAppointmentAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
        }

        protected virtual void FSAppointmentAttendee_ContactID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentAttendee fsAppointmentAttendeeRow = (FSAppointmentAttendee)e.Row;

            ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsAppointmentAttendeeRow.CustomerID, fsAppointmentAttendeeRow.ContactID);
            fsAppointmentAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
            fsAppointmentAttendeeRow.Mem_EMail = attendeeInfo.EMail;
            fsAppointmentAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
        }

        protected virtual void FSAppointmentAttendee_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (IsCloningAppointment == false)
            {
                FSAppointmentAttendee fsAppointmentAttendeeRow = (FSAppointmentAttendee)e.Row;

                ServiceOrderCore.AttendeeInfo attendeeInfo = ServiceOrderCore.GetAttendeeInfo(this, fsAppointmentAttendeeRow.CustomerID, fsAppointmentAttendeeRow.ContactID);
                fsAppointmentAttendeeRow.Mem_CustomerContactName = attendeeInfo.CustomerContactName;
                fsAppointmentAttendeeRow.Mem_EMail = attendeeInfo.EMail;
                fsAppointmentAttendeeRow.Mem_Phone1 = attendeeInfo.Phone1;
            }
        }

        protected virtual void FSAppointmentAttendee_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.SetServiceOrderRecord_AsUpdated_IfItsNotchanged(ServiceOrderRelated.Cache, ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentAttendee_RowDeleted(PXCache cache, PXRowDeletedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.SetServiceOrderRecord_AsUpdated_IfItsNotchanged(ServiceOrderRelated.Cache, ServiceOrderRelated.Current);
        }

        protected virtual void FSAppointmentAttendee_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.SetServiceOrderRecord_AsUpdated_IfItsNotchanged(ServiceOrderRelated.Cache, ServiceOrderRelated.Current);
        }

        [Obsolete("If this method is empty on 2019r2 please delete it.")]
        protected virtual void FSAppointmentAttendee_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }
        }
        #endregion

        #region AppointmentInventoryItemEvents

        protected override void FSAppointmentInventoryItem_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            if (e.Row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            MarkHeaderAsUpdated(cache, e.Row);

            ServiceOrderAppointmentHandlers.CheckIfManualPrice<FSAppointmentInventoryItem, FSAppointmentInventoryItem.qty>(cache, e);

            base.FSAppointmentInventoryItem_RowUpdated(cache, e);
            base.VerifyIsAlreadyPosted<FSAppointmentInventoryItem.inventoryID>(cache, null, (FSAppointmentInventoryItem)e.Row, BillingCycleRelated.Current);
        }

        protected virtual void FSAppointmentInventoryItem_RowInserting(PXCache cache, PXRowInsertingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentInventoryItem fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;

            if (fsAppointmentInventoryItemRow.SODetID != null)
            {
                return;
            }

            var fsAppointmentDetServiceRows = AppointmentDetServices.Select();

            if (fsAppointmentDetServiceRows.Count == 1)
            {
                FSAppointmentDetService fsAppointmentDetServiceRow = fsAppointmentDetServiceRows[0];

                cache.SetValueExt<FSAppointmentInventoryItem.sODetID>(fsAppointmentInventoryItemRow, fsAppointmentDetServiceRow.SODetID);
            }
        }

        protected virtual void FSAppointmentInventoryItem_InventoryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderAppointmentHandlers.X_InventoryID_FieldUpdated<FSAppointmentInventoryItem, FSAppointmentInventoryItem.subItemID,
                                                FSAppointmentInventoryItem.siteID, FSAppointmentInventoryItem.siteLocationID, FSAppointmentInventoryItem.uOM,
                                                FSAppointmentInventoryItem.estimatedDuration, FSAppointmentInventoryItem.estimatedQty, FSAppointmentInventoryItem.billingRule, 
                                                FSAppointmentInventoryItem.actualDuration, FSAppointmentInventoryItem.qty>(
                                                        cache,
                                                        e,
                                                        ServiceOrderRelated.Current.BranchLocationID,
                                                        TaxCustomer.Current,
                                                        useActualFields: true);

            FSAppointmentInventoryItem fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;

            if (e.ExternalCall == false)
            {
                //In case ExternalCall == false (any change not coming from Appointment's UI), ShowPopupMessage = false to avoid
                //showing the Popup Message for the InventoryID field. By default it's set to true.

                foreach (PXSelectorAttribute s in cache.GetAttributes(typeof(FSAppointmentInventoryItem.inventoryID).Name).OfType<PXSelectorAttribute>())
                {
                    s.ShowPopupMessage = false;
                }
            }
        }

        protected virtual void FSAppointmentInventoryItem_SODetID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentInventoryItem fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;
            string oldServiceLineRef = (string)e.OldValue;

            string pivot = fsAppointmentInventoryItemRow.LineRef ?? oldServiceLineRef;
            FSAppointmentDetService fsAppointmentDetRow = PXSelectJoin<FSAppointmentDetService,
                                                           InnerJoin<FSSODet,
                                                               On<FSSODet.sODetID, Equal<FSAppointmentDetService.sODetID>>>,
                                                           Where<
                                                               FSSODet.lineRef, Equal<Required<FSSODet.lineRef>>,
                                                               And<FSSODet.sOID, Equal<Current<FSAppointment.sOID>>>>>
                                                           .Select(cache.Graph, pivot);

            fsAppointmentInventoryItemRow.ProjectTaskID = fsAppointmentDetRow?.ProjectTaskID;
            fsAppointmentInventoryItemRow.CostCodeID = fsAppointmentDetRow?.CostCodeID;
        }

        protected virtual void FSAppointmentInventoryItem_Qty_FieldUpdating(PXCache cache, PXFieldUpdatingEventArgs e)
        {
            if (e.NewValue == null || e.Row == null)
            {
                return;
            }

            FSAppointmentInventoryItem fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;

            decimal newValue = Convert.ToDecimal(e.NewValue);

            if (newValue < (decimal?)0.0)
            {
                cache.RaiseExceptionHandling<FSAppointmentInventoryItem.qty>(e.Row, fsAppointmentInventoryItemRow.Qty, new PXSetPropertyException(TX.Error.POSITIVE_QTY, PXErrorLevel.Warning));
                e.NewValue = fsAppointmentInventoryItemRow.Qty;
            }
        }

        protected virtual void FSAppointmentInventoryItem_Qty_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderAppointmentHandlers.X_Qty_FieldUpdated<FSAppointmentInventoryItem.curyUnitPrice>(cache, e);
        }
        
        protected virtual void FSAppointmentInventoryItem_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            FSAppointment fsAppointmentRow = AppointmentSelected.Current;

            // Move the old code of SetEnabled and SetPersistingCheck in previous methods to this new generic method
            // keeping the generic convention.
            ServiceOrderAppointmentHandlers.X_RowSelected<FSAppointmentInventoryItem>(
                                            cache,
                                            e,
                                            ServiceOrderRelated.Current,
                                            ServiceOrderTypeSelected.Current,
                                            disableSODetReferenceFields: false,
                                            docAllowsActualFieldEdition: SharedFunctions.IsAppointmentNotStarted(fsAppointmentRow) == false);
        }

        protected virtual void FSAppointmentInventoryItem_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAppointmentInventoryItem fsAppointmentInventoryItemRow = (FSAppointmentInventoryItem)e.Row;

            ServiceOrderAppointmentHandlers.X_SetPersistingCheck<FSAppointmentInventoryItem>(cache, e, ServiceOrderRelated.Current, ServiceOrderTypeSelected.Current);
            base.VerifyIsAlreadyPosted<FSAppointmentInventoryItem.inventoryID>(cache, null, fsAppointmentInventoryItemRow, BillingCycleRelated.Current);
        }
        #endregion

        #region FSPostDetEvents
        protected virtual void FSPostDet_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSPostDet fsPostDetRow = e.Row as FSPostDet;

            if (fsPostDetRow.SOPosted == true)
            {
                using (new PXConnectionScope())
                {
                    var soOrderShipment = (SOOrderShipment)PXSelectReadonly<SOOrderShipment,
                                          Where<
                                              SOOrderShipment.orderNbr, Equal<Required<SOOrderShipment.orderNbr>>,
                                          And<
                                              SOOrderShipment.orderType, Equal<Required<SOOrderShipment.orderType>>>>>
                                    .Select(cache.Graph, fsPostDetRow.SOOrderNbr, fsPostDetRow.SOOrderType);

                    fsPostDetRow.InvoiceRefNbr = soOrderShipment?.InvoiceNbr;
                    fsPostDetRow.InvoiceDocType = soOrderShipment?.InvoiceType;
                }
            }
            else if (fsPostDetRow.ARPosted == true || fsPostDetRow.SOInvPosted == true)
            {
                fsPostDetRow.InvoiceRefNbr = fsPostDetRow.Mem_DocNbr;
                fsPostDetRow.InvoiceDocType = fsPostDetRow.ARDocType; 
            }
            else if (fsPostDetRow.APPosted == true)
            {
                fsPostDetRow.InvoiceRefNbr = fsPostDetRow.Mem_DocNbr;
                fsPostDetRow.InvoiceDocType = fsPostDetRow.APDocType;
            }

            using (new PXConnectionScope())
            {
                FSPostBatch fsPostBatchRow = PXSelect<FSPostBatch,
                                                       Where<FSPostBatch.batchID, Equal<Required<FSPostBatch.batchID>>>>
                                             .Select(cache.Graph, fsPostDetRow.BatchID);
                fsPostDetRow.BatchNbr = fsPostBatchRow?.BatchNbr;
            }

        }
        #endregion

        #region ServiceOrderHandlers
        protected virtual void FSServiceOrder_ProjectID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            UpdateDetailsFromProjectID(fsServiceOrderRow);
            AppointmentRecords.Current.DfltProjectTaskID = null;

            ContractRelatedToProject.Current = ContractRelatedToProject.Select(fsServiceOrderRow.ProjectID);
        }

        protected override void FSServiceOrder_BranchID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            base.FSServiceOrder_BranchID_FieldUpdated(cache, e);

            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            UpdateDetailsFromBranchID(fsServiceOrderRow);
        }

        protected virtual void FSServiceOrder_BranchLocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.FSServiceOrder_BranchLocationID_FieldUpdated_Handler(
                                                                                this,
                                                                                e,
                                                                                ServiceOrderTypeSelected.Current,
                                                                                ServiceOrderRelated);
        }

        protected virtual void FSServiceOrder_LocationID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ServiceOrderCore.FSServiceOrder_LocationID_FieldUpdated_Handler(cache, e, ServiceOrderTypeSelected.Current);

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;
            FSSrvOrdType fsSrvOrdTypeRow = ServiceOrderTypeSelected.Current;

            SetCurrentAppointmentSalesPersonID(fsServiceOrderRow);
        }

        protected virtual void FSServiceOrder_ContactID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            ServiceOrderCore.FSServiceOrder_ContactID_FieldUpdated_Handler(this, e, ServiceOrderTypeSelected.Current);
        }

        protected override void FSServiceOrder_BillCustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            base.FSServiceOrder_BillCustomerID_FieldUpdated(cache, e);

            ServiceOrderCore.FSServiceOrder_BillCustomerID_FieldUpdated_Handler(cache, e);
            BillingCycleRelated.Current = BillingCycleRelated.Select();
            if (SkipChangingContract == false)
            {
                AppointmentSelected.Cache.SetDefaultExt<FSAppointment.billServiceContractID>(AppointmentSelected.Current);
                AppointmentSelected.Cache.SetDefaultExt<FSAppointment.billContractPeriodID>(AppointmentSelected.Current);
            }
            SkipChangingContract = false;
        }

        protected virtual void FSServiceOrder_CustomerID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            if (cache.Graph.IsCopyPasteContext == true)
            {
                AppointmentDetServices.Cache.AllowInsert = true;
                AppointmentDetServices.Cache.AllowUpdate = true;

                AppointmentDetParts.Cache.AllowInsert = true;
                AppointmentDetParts.Cache.AllowUpdate = true;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            PXResultset<FSAppointment> bqlResultSet = PXSelect<FSAppointment, 
                                                      Where<
                                                            FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                                                      .Select(this, fsServiceOrderRow.SOID);

            ServiceOrderCore.FSServiceOrder_CustomerID_FieldUpdated_Handler(
                                                                            cache,
                                                                            e,
                                                                            ServiceOrderTypeSelected.Current,
                                                                            null,
                                                                            null,
                                                                            AppointmentDetServices,
                                                                            AppointmentDetParts,
                                                                            PickupDeliveryItems,
                                                                            bqlResultSet,
                                                                            AppointmentRecords.Current.ScheduledDateTimeBegin,
                                                                            allowCustomerChange: false,
                                                                            customerRow: TaxCustomer.Current);

            SetCurrentAppointmentSalesPersonID(fsServiceOrderRow);

            ServiceOrderCore.ValidateCustomerBillingCycle(cache, SetupRecord.Current, ServiceOrderTypeSelected.Current, (FSServiceOrder)e.Row);
        }

        protected virtual void FSServiceOrder_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SharedFunctions.InitializeNote(cache, e);
        }

        protected virtual void FSServiceOrder_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            if (string.IsNullOrEmpty(fsServiceOrderRow.SrvOrdType))
            {
                return;
            }

            if (ContractRelatedToProject?.Current == null)
            {
                ContractRelatedToProject.Current = ContractRelatedToProject.Select(fsServiceOrderRow.ProjectID);
            }

            int appointmentCount = PXSelect<FSAppointment,
                                        Where<
                                            FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>
                                        .SelectWindowed(cache.Graph, 0, 2, fsServiceOrderRow.SOID).Count;

            ServiceOrderCore.FSServiceOrder_RowSelected_PartialHandler(
                                                                    cache.Graph,
                                                                    cache,
                                                                    fsServiceOrderRow,
                                                                    AppointmentRecords.Current,
                                                                    ServiceOrderTypeSelected.Current,
                                                                    BillingCycleRelated.Current,
                                                                    ContractRelatedToProject.Current,
                                                                    appointmentCount,
                                                                    AppointmentDetServices.Select().Count,
                                                                    AppointmentDetParts.Select().Count,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null,
                                                                    null);

            PXUIFieldAttribute.SetEnabled<FSServiceOrder.projectID>(
                                                                    cache, 
                                                                    fsServiceOrderRow, 
                                                                    cache.GetStatus(fsServiceOrderRow) == PXEntryStatus.Inserted);
        }

        protected virtual void FSAddress_CountryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSAddress fsAddressRow = (FSAddress)e.Row;

            if (fsAddressRow.CountryID != (string)e.OldValue)
            {
                fsAddressRow.State = null;
                fsAddressRow.PostalCode = null;
            }
        }

        protected virtual void FSServiceOrder_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

            using (new PXConnectionScope())
            {
                ServiceOrderCore.UpdateServiceOrderUnboundFields(this, fsServiceOrderRow, BillingCycleRelated.Current, dummyGraph, AppointmentRecords.Current, DisableServiceOrderUnboundFieldCalc);

                PXResultset<FSAppointment> fsAppointmentSet = PXSelectReadonly<FSAppointment,
                                                                Where2<
                                                                    Where<
                                                                        FSAppointment.status, Equal<ListField_Status_Appointment.Closed>,
                                                                        Or<FSAppointment.status, Equal<ListField_Status_Appointment.Completed>>>,
                                                                    And<FSAppointment.sOID, Equal<Required<FSAppointment.sOID>>>>>
                                                                .Select(dummyGraph, fsServiceOrderRow.SOID);

                FSAppointment currentAppointment = AppointmentRecords.Current;
                fsServiceOrderRow.AppointmentsCompletedOrClosedCntr = 0;
                fsServiceOrderRow.AppointmentsCompletedCntr = 0;

                foreach (FSAppointment fsAppointmentRow in fsAppointmentSet)
                {
                    if(currentAppointment == null || 
                        (currentAppointment != null 
                            && currentAppointment.AppointmentID != fsAppointmentRow.AppointmentID))
                    { 
                        fsServiceOrderRow.AppointmentsCompletedOrClosedCntr += 1;

                        if (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED)
                        { 
                            fsServiceOrderRow.AppointmentsCompletedCntr += 1;
                        }
                    }
                }

                if (currentAppointment != null)
                {
                    if (currentAppointment.Status == ID.Status_Appointment.COMPLETED)
                    {
                        fsServiceOrderRow.AppointmentsCompletedOrClosedCntr += 1;
                        fsServiceOrderRow.AppointmentsCompletedCntr += 1;
                    }
                    else if (currentAppointment.Status == ID.Status_Appointment.CLOSED)
                    {
                        fsServiceOrderRow.AppointmentsCompletedOrClosedCntr += 1;
                    }
                }
            }
        }

        protected virtual void FSServiceOrder_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Insert)
            {
                FSServiceOrder fsServiceOrderRow = (FSServiceOrder)e.Row;

                // SrvOrdType is key field
                if (string.IsNullOrWhiteSpace(fsServiceOrderRow.SrvOrdType))
                {
                    GraphHelper.RaiseRowPersistingException<FSAppointment.srvOrdType>(AppointmentRecords.Cache, AppointmentRecords.Current);
                }

                // Initial values from appointment
                fsServiceOrderRow.CuryID            = AppointmentRecords.Current.CuryID;
                fsServiceOrderRow.TaxZoneID         = AppointmentRecords.Current.TaxZoneID;
                fsServiceOrderRow.DfltProjectTaskID = AppointmentRecords.Current.DfltProjectTaskID;
                fsServiceOrderRow.DocDesc           = AppointmentRecords.Current.DocDesc;
                fsServiceOrderRow.OrderDate         = AppointmentRecords.Current.ScheduledDateTimeBegin;
                fsServiceOrderRow.SalesPersonID     = AppointmentRecords.Current.SalesPersonID;
                fsServiceOrderRow.Commissionable    = AppointmentRecords.Current.Commissionable;
                fsServiceOrderRow.WFStageID         = null;
                fsServiceOrderRow.CBID              = ServiceOrderCore.GetCBIDFromCustomer(this, fsServiceOrderRow.BillCustomerID, fsServiceOrderRow.SrvOrdType);
                fsServiceOrderRow.CutOffDate        = ServiceOrderCore.GetCutOffDate(this, fsServiceOrderRow.CBID, fsServiceOrderRow.OrderDate);

                SharedFunctions.ValidateSrvOrdTypeNumberingSequence(this, fsServiceOrderRow.SrvOrdType);

                insertingServiceOrder = true;
            }
        }

        protected virtual void FSAddress_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        {
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Update)
            {
                FSAddress fsAddressRow = (FSAddress)e.Row;

                string postalCode = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.postalCode>(fsAddressRow);
                string addressLine1 = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.addressLine1>(fsAddressRow);
                string addressLine2 = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.addressLine2>(fsAddressRow);
                string city = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.city>(fsAddressRow);
                string state = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.state>(fsAddressRow);
                string countryID = (string)ServiceOrder_Address.Cache.GetValueOriginal<FSAddress.countryID>(fsAddressRow);

                if (fsAddressRow.PostalCode != postalCode
                        || fsAddressRow.AddressLine1 != addressLine1
                        || fsAddressRow.AddressLine2 != addressLine2
                        || fsAddressRow.City != city
                        || fsAddressRow.State != state
                        || fsAddressRow.CountryID != countryID
                )
                {
                    RetakeGeoLocation = true;
                }
            }
        }

        #endregion

        #region ARPaymentEvents
        protected virtual void ARPayment_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            ARPayment arPaymentRow = (ARPayment)e.Row;

            using (new PXConnectionScope())
            {
                ServiceOrderCore.RecalcSOApplAmounts(this, arPaymentRow);
            }
        }
        #endregion

        #region Selector Methods

        #region Staff Selector

        [PXCopyPasteHiddenView]
        public PXFilter<StaffSelectionFilter> StaffSelectorFilter;
        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.SkillRecords_View SkillGridFilter;
        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.LicenseTypeRecords_View LicenseTypeGridFilter;
        [PXCopyPasteHiddenView]
        public StaffSelectionHelper.StaffRecords_View StaffRecords;

        public IEnumerable skillGridFilter()
        {
            return StaffSelectionHelper.SkillFilterDelegate(this, AppointmentDetServices, StaffSelectorFilter, SkillGridFilter); 
        }

        public IEnumerable licenseTypeGridFilter()
        {
            return StaffSelectionHelper.LicenseTypeFilterDelegate(this, AppointmentDetServices, StaffSelectorFilter, LicenseTypeGridFilter);
        }

        protected virtual IEnumerable staffRecords()
        {
            return StaffSelectionHelper.StaffRecordsDelegate(
                                                                (object)AppointmentEmployees,
                                                                SkillGridFilter,
                                                                LicenseTypeGridFilter,
                                                                StaffSelectorFilter);
        }

        protected virtual void StaffSelectionFilter_ServiceLineRef_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();
        }

        protected virtual void BAccountStaffMember_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            BAccountStaffMember bAccountStaffMemberRow = (BAccountStaffMember)e.Row;

            if (StaffSelectorFilter.Current != null)
            {
                if (bAccountStaffMemberRow.Selected == true)
                {
                    FSAppointmentEmployee fsFSAppointmentEmployeeRow = new FSAppointmentEmployee
                    {
                        EmployeeID = bAccountStaffMemberRow.BAccountID,
                        ServiceLineRef = StaffSelectorFilter.Current.ServiceLineRef
                    };
                    AppointmentEmployees.Insert(fsFSAppointmentEmployeeRow);
                }
                else
                {
                    FSAppointmentEmployee fsFSAppointmentEmployeeRow = PXSelectJoin<FSAppointmentEmployee,
                                                                        LeftJoin<FSAppointment,
                                                                            On<FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                                                                        LeftJoin<FSSODet,
                                                                            On<
                                                                                FSSODet.sOID, Equal<FSAppointment.sOID>, 
                                                                                And<FSSODet.lineRef, Equal<FSAppointmentEmployee.serviceLineRef>>>>>,
                                                                        Where2<
                                                                            Where<
                                                                                FSSODet.lineRef, Equal<Required<FSSODet.lineRef>>,
                                                                                Or<
                                                                                    Where<
                                                                                        FSSODet.lineRef, IsNull,
                                                                                        And<Required<FSSODet.lineRef>, IsNull>>>>,
                                                                            And<
                                                                                Where<
                                                                                    FSAppointmentEmployee.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                                                                    And<FSAppointmentEmployee.employeeID, Equal<Required<FSAppointmentEmployee.employeeID>>>>>>>
                                                                          .Select(cache.Graph, StaffSelectorFilter.Current.ServiceLineRef, StaffSelectorFilter.Current.ServiceLineRef, bAccountStaffMemberRow.BAccountID);

                    fsFSAppointmentEmployeeRow = (FSAppointmentEmployee)AppointmentEmployees.Cache.Locate(fsFSAppointmentEmployeeRow);
                    if (fsFSAppointmentEmployeeRow != null)
                    {
                        AppointmentEmployees.Delete(fsFSAppointmentEmployeeRow);
                    }
                }
            }

            StaffRecords.View.RequestRefresh();
        }

        #region OpenStaffSelectorFromServiceTab
        public PXAction<FSAppointment> openStaffSelectorFromServiceTab;
        [PXButton]
        [PXUIField(DisplayName = "Staff Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromServiceTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();

            if (ServiceOrderRelated.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ProjectID = ServiceOrderRelated.Current.ProjectID;
            }

            if (AppointmentSelected.Current != null)
            {
                StaffSelectorFilter.Current.ScheduledDateTimeBegin = AppointmentSelected.Current.ScheduledDateTimeBegin;
            }

            FSAppointmentDetService serviceRow = AppointmentDetServices.Current;

            if (serviceRow != null && serviceRow.LineType == ID.LineType_All.SERVICE
                && serviceRow.SODetID != null)
            {
                FSSODet fsSODetRow = ServiceOrderCore.GetSODetFromAppointmentDet(this, serviceRow);

                StaffSelectorFilter.Current.ServiceLineRef = fsSODetRow?.LineRef;
            }
            else
            {
                StaffSelectorFilter.Current.ServiceLineRef = null;
            }

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();

            StaffSelectionHelper appStaffSelector = new StaffSelectionHelper();
            appStaffSelector.LaunchStaffSelector(this, StaffSelectorFilter);
        }
        #endregion

        #region OpenStaffSelectorFromStaffTab
        public PXAction<FSAppointment> openStaffSelectorFromStaffTab;
        [PXButton]
        [PXUIField(DisplayName = "Staff Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenStaffSelectorFromStaffTab()
        {
            ServiceOrder_Contact.Current = ServiceOrder_Contact.SelectSingle();
            ServiceOrder_Address.Current = ServiceOrder_Address.SelectSingle();

            if (ServiceOrderRelated.Current != null)
            {
                StaffSelectorFilter.Current.PostalCode = ServiceOrder_Address.Current.PostalCode;
                StaffSelectorFilter.Current.ProjectID = ServiceOrderRelated.Current.ProjectID;
            }

            if (AppointmentRecords.Current != null)
            {
                StaffSelectorFilter.Current.ScheduledDateTimeBegin = AppointmentRecords.Current.ScheduledDateTimeBegin;
            }

            StaffSelectorFilter.Current.ServiceLineRef = null;

            SkillGridFilter.Cache.Clear();
            LicenseTypeGridFilter.Cache.Clear();
            StaffRecords.Cache.Clear();

            StaffSelectionHelper appStaffSelector = new StaffSelectionHelper();
            appStaffSelector.LaunchStaffSelector(this, StaffSelectorFilter);
        }
        #endregion

        #endregion

        #region Service Selector

        [PXCopyPasteHiddenView]
        public PXFilter<ServiceSelectionFilter> ServiceSelectorFilter;
        [PXCopyPasteHiddenView]
        public PXSelect<EmployeeGridFilter> EmployeeGridFilter;
        [PXCopyPasteHiddenView]
        public ServiceSelectionHelper.ServiceRecords_View ServiceRecords;

        protected virtual IEnumerable employeeGridFilter()
        {
            return ServiceSelectionHelper.EmployeeRecordsDelegate((object)AppointmentEmployees, EmployeeGridFilter);
        }

        protected virtual IEnumerable serviceRecords()
        {
            return ServiceSelectionHelper.ServiceRecordsDelegate(
                                                                AppointmentDetServices,
                                                                EmployeeGridFilter,
                                                                ServiceSelectorFilter);
        }

        #region OpenServicesSelector
        public PXAction<FSAppointment> openServiceSelector;
        [PXButton]
        [PXUIField(DisplayName = "Service Selector", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        public virtual void OpenServiceSelector()
        {
            ServiceSelectionHelper.OpenServiceSelector<FSAppointment.scheduledDateTimeBegin>(
                                                    AppointmentSelected.Cache,
                                                    ServiceSelectorFilter,
                                                    EmployeeGridFilter);
        }
        #endregion

        #region SelectCurrentService
        public PXAction<FSAppointment> selectCurrentService;
        [PXUIField(DisplayName = "Select Current Service", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual void SelectCurrentService()
        {
            ServiceSelectionHelper.InsertCurrentService<FSAppointmentDetService>(ServiceRecords, AppointmentDetServices);
        }
        #endregion

        #endregion

        #endregion

        protected virtual void MarkHeaderAsUpdated(PXCache cache, object row)
        {
            if (row == null || AppointmentSelected.Current == null)
            {
                return;
            }

            if (AppointmentSelected.Cache.GetStatus(AppointmentSelected.Current) == PXEntryStatus.Notchanged
                && AppointmentSelected.Current.RefNbr != null)
            {
                AppointmentSelected.Cache.SetStatus(AppointmentSelected.Current, PXEntryStatus.Updated);
            }
        }

        #region Methods to handle line inserts and updates
        public virtual void CopyAppointmentLineValues<TargetRowType, SourceRowType>(
                                                                            PXCache targetCache,
                                                                            object objTargetRow,
                                                                            PXCache sourceCache,
                                                                            object objSourceRow,
                                                                            bool copyTranDate,
                                                                            DateTime? tranDate,
                                                                            bool ForceFormulaCalculation)
            where TargetRowType : class, IBqlTable, IFSSODetBase, new()
            where SourceRowType : class, IBqlTable, IFSSODetBase, new()
        {
            var targetRow = (TargetRowType)objTargetRow;
            var sourceRow = (SourceRowType)objSourceRow;
            FSSODet fsSODetRow = null;
            FSAppointmentDet fsAppointmentDetRow = null;
            TargetRowType oldRow = null;


            if (ForceFormulaCalculation == true)
            {
                // This row copy is to be used with cache.RaiseRowUpdated in order to accumulate totals with Formulas.
                oldRow = (TargetRowType)targetCache.CreateCopy(targetRow);
            }


            if (targetRow is FSSODet)
            {
                fsSODetRow = (FSSODet)objTargetRow;

                if (copyTranDate)
                {
                    fsSODetRow.TranDate = tranDate;
                }
            }
            else
            {
                fsAppointmentDetRow = (FSAppointmentDet)objTargetRow;

                if (copyTranDate)
                {
                    fsAppointmentDetRow.TranDate = tranDate;
                }
            }


            // **********************************************************************************************************
            // The sequence of fields in this method is VERY IMPORTANT because
            // it determines the proper validation and assignment of values
            // **********************************************************************************************************

            targetRow.Status = sourceRow.Status;
            targetCache.SetValueExtIfDifferent<FSSODet.lineType>(targetRow, sourceRow.LineType);


            targetRow = CopyDependentFieldsOfSODet<TargetRowType, SourceRowType>(
                                                                            targetCache,
                                                                            targetRow,
                                                                            sourceCache,
                                                                            sourceRow,
                                                                            copyingFromQuote: false);

            if (ForceFormulaCalculation == true)
            {
                // This cache.RaiseRowUpdated is required to accumulate totals with Formulas
                // ONLY when this method is called from outside any row event.
                targetCache.RaiseRowUpdated(targetRow, oldRow);
            }
        }

        public static NewRowType InsertServicePartLine<NewRowType, SourceRowType>(
                                                                            PXCache newRowCache,
                                                                            object objNewRow,
                                                                            PXCache sourceCache,
                                                                            object objSourceRow,
                                                                            Guid? noteID,
                                                                            int? soDetID,
                                                                            bool copyTranDate,
                                                                            DateTime? tranDate,
                                                                            bool SetValuesAfterAssigningSODetID,
                                                                            bool copyingFromQuote)
            where NewRowType : class, IBqlTable, IFSSODetBase, new()
            where SourceRowType : class, IBqlTable, IFSSODetBase, new()
        {
            var newRow = (NewRowType)objNewRow;
            var sourceRow = (SourceRowType)objSourceRow;
            FSSODet fsSODetRow = null;
            FSAppointmentDet fsAppointmentDetRow = null;

            if (newRow is FSSODet)
            {
                fsSODetRow = (FSSODet)objNewRow;
            }
            else
            {
                fsAppointmentDetRow = (FSAppointmentDet)objNewRow;
            }

            //*****************************************************************************************
            // You can specify before cache.Insert only fields that are not calculated from other ones.
            newRow.Status = sourceRow.Status;
            newRow.LineType = sourceRow.LineType;

            if (fsSODetRow != null)
            {
                // Insert the new row with the key fields cleared
                fsSODetRow.RefNbr = null;
                fsSODetRow.SOID = null;
                fsSODetRow.LineRef = null;
                fsSODetRow.SODetID = null;
                fsSODetRow.LotSerialNbr = null;

                if (copyTranDate)
                {
                    fsSODetRow.TranDate = tranDate;
                }

                fsSODetRow.NoteID = noteID;
            }
            else
            {
                // Insert the new row with the key fields cleared
                fsAppointmentDetRow.RefNbr = null;
                fsAppointmentDetRow.AppointmentID = null;
                fsAppointmentDetRow.LineRef = null;
                fsAppointmentDetRow.AppDetID = null;
                fsAppointmentDetRow.ApptLineNbr = null;

                // Insert the new row with special fields cleared
                fsAppointmentDetRow.ActualDateTimeBegin = null;
                fsAppointmentDetRow.ActualDateTimeEnd = null;
                fsAppointmentDetRow.LotSerialNbr = null;
                fsAppointmentDetRow.StaffActualDuration = 0;

                if (copyTranDate)
                {
                    fsAppointmentDetRow.TranDate = tranDate;
                }

                fsAppointmentDetRow.NoteID = noteID;

                if (sourceCache.Graph.Accessinfo.ScreenID == SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.CLONE_APPOINTMENT))
                {
                    fsAppointmentDetRow.EquipmentAction = ID.Equipment_Action.NONE;
                    fsAppointmentDetRow.SMEquipmentID = null;
                    fsAppointmentDetRow.NewTargetEquipmentLineNbr = null;
                    fsAppointmentDetRow.ComponentID = null;
                    fsAppointmentDetRow.EquipmentLineRef = null;
                }
            }

            newRow = (NewRowType)newRowCache.Insert(newRow);
            //*****************************************************************************************

            // This row copy is to be used with cache.RaiseRowUpdated in order to accumulate totals with Formulas
            var oldRow = (NewRowType)newRowCache.CreateCopy(newRow);


            if (fsAppointmentDetRow != null)
            {
                newRowCache.SetValueExtIfDifferent<FSSODet.sODetID>(newRow, soDetID);
            }

            if (SetValuesAfterAssigningSODetID == true || fsSODetRow != null)
            {
                newRow = CopyDependentFieldsOfSODet<NewRowType, SourceRowType>(
                                                                        newRowCache,
                                                                        newRow,
                                                                        sourceCache,
                                                                        sourceRow,
                                                                        copyingFromQuote);
            }


            // This cache.RaiseRowUpdated is required to accumulate totals with Formulas
            newRowCache.RaiseRowUpdated(newRow, oldRow);


            return newRow;
        }

        protected static TargetRowType CopyDependentFieldsOfSODet<TargetRowType, SourceRowType>(
                                                                            PXCache targetCache,
                                                                            object objTargetRow,
                                                                            PXCache sourceCache,
                                                                            object objSourceRow,
                                                                            bool copyingFromQuote)
            where TargetRowType : class, IBqlTable, IFSSODetBase, new()
            where SourceRowType : class, IBqlTable, IFSSODetBase, new()
        {
            var targetRow = (TargetRowType)objTargetRow;
            var sourceRow = (SourceRowType)objSourceRow;


            // **********************************************************************************************************
            // The sequence of fields in this method is VERY IMPORTANT because
            // it determines the proper validation and assignment of values
            // **********************************************************************************************************

            targetCache.SetValueExtIfDifferent<FSSODet.branchID>(targetRow, sourceRow.BranchID);

            targetCache.SetValueExtIfDifferent<FSSODet.inventoryID>(targetRow, sourceRow.InventoryID);

            if (targetRow.InventoryID != null)
            {
                targetCache.SetValueExtIfDifferent<FSSODet.manualPrice>(targetRow, sourceRow.ManualPrice);
                targetCache.SetValueExtIfDifferent<FSSODet.isBillable>(targetRow, sourceRow.IsBillable);
                targetCache.SetValueExtIfDifferent<FSSODet.isPrepaid>(targetRow, sourceRow.IsPrepaid);

                targetCache.SetValueExtIfDifferent<FSSODet.subItemID>(targetRow, sourceRow.SubItemID);
                targetCache.SetValueExtIfDifferent<FSSODet.billingRule>(targetRow, sourceRow.BillingRule);

                targetCache.SetValueExtIfDifferent<FSSODet.uOM>(targetRow, sourceRow.UOM);

                targetCache.SetValueExtIfDifferent<FSSODet.siteID>(targetRow, sourceRow.SiteID);
                targetCache.SetValueExtIfDifferent<FSSODet.siteLocationID>(targetRow, sourceRow.SiteLocationID);

                targetCache.SetValueExtIfDifferent<FSSODet.estimatedQty>(targetRow, sourceRow.GetQty(FieldType.EstimatedField));
                targetCache.SetValueExtIfDifferent<FSSODet.estimatedDuration>(targetRow, sourceRow.GetDuration(FieldType.EstimatedField));

                if (targetRow.ManualPrice == true)
                {
                    decimal targetCuryUnitPrice = 0m;

                    if (sourceRow.UnitPrice != null)
                    {
                        targetCache.Graph.FindImplementation<IPXCurrencyHelper>().CuryConvCury((decimal)(sourceRow.UnitPrice), out targetCuryUnitPrice);
                    }

                    targetCache.SetValueExtIfDifferent<FSSODet.curyUnitPrice>(targetRow, targetCuryUnitPrice);

                    if (targetCuryUnitPrice != 0)
                    {
                        PXUIFieldAttribute.SetWarning<FSSODet.curyUnitPrice>(targetCache, targetRow, null);
                    }
                }

                targetCache.SetValueExtIfDifferent<FSSODet.enablePO>(targetRow, sourceRow.EnablePO);

                if (sourceRow.EnablePO == true)
                {
                    decimal targetCuryUnitCost = 0m;

                    if (sourceRow.UnitCost != null)
                    {
                        targetCache.Graph.FindImplementation<IPXCurrencyHelper>().CuryConvCury((decimal)(sourceRow.UnitCost), out targetCuryUnitCost);
                    }

                    targetCache.SetValueExtIfDifferent<FSSODet.curyUnitCost>(targetRow, targetCuryUnitCost);

                    if (targetCuryUnitCost != 0)
                    {
                        PXUIFieldAttribute.SetWarning<FSSODet.curyUnitCost>(targetCache, targetRow, null);
                    }
                }

                targetCache.SetValueExtIfDifferent<FSSODet.taxCategoryID>(targetRow, sourceRow.TaxCategoryID);
                targetCache.SetValueExtIfDifferent<FSSODet.projectID>(targetRow, sourceRow.ProjectID);
                targetCache.SetValueExtIfDifferent<FSSODet.projectTaskID>(targetRow, sourceRow.ProjectTaskID);

                if (sourceRow.AcctID != null || copyingFromQuote == false)
                {
                    targetCache.SetValueExtIfDifferent<FSSODet.acctID>(targetRow, sourceRow.AcctID);

                    if (sourceRow.SubID != null || copyingFromQuote == false)
                    {
                        targetCache.SetValueExtIfDifferent<FSSODet.subID>(targetRow, sourceRow.SubID);
                    }
                }

                targetCache.SetValueExtIfDifferent<FSSODet.costCodeID>(targetRow, sourceRow.CostCodeID);
                targetCache.SetValueExtIfDifferent<FSSODet.lotSerialNbr>(targetRow, sourceRow.LotSerialNbr);
            }

            if (targetCache.Graph.Accessinfo.ScreenID != SharedFunctions.SetScreenIDToDotFormat(ID.ScreenID.CLONE_APPOINTMENT))
            {
                targetCache.SetValueExtIfDifferent<FSSODet.equipmentAction>(targetRow, sourceRow.EquipmentAction);
                targetCache.SetValueExtIfDifferent<FSSODet.SMequipmentID>(targetRow, sourceRow.SMEquipmentID);
                targetCache.SetValueExtIfDifferent<FSSODet.newTargetEquipmentLineNbr>(targetRow, sourceCache.GetValue<FSAppointmentDet.newTargetEquipmentLineNbr>(sourceRow));
                targetCache.SetValueExtIfDifferent<FSSODet.componentID>(targetRow, sourceRow.ComponentID);
                targetCache.SetValueExtIfDifferent<FSSODet.equipmentLineRef>(targetRow, sourceRow.EquipmentLineRef);
            }
            else
            {
                targetCache.SetValueExt<FSSODet.equipmentAction>(targetRow, ID.Equipment_Action.NONE);
                targetCache.SetValueExt<FSSODet.SMequipmentID>(targetRow, null);
                targetCache.SetValueExt<FSSODet.newTargetEquipmentLineNbr>(targetRow, null);
                targetCache.SetValueExt<FSSODet.componentID>(targetRow, null);
                targetCache.SetValueExt<FSSODet.equipmentLineRef>(targetRow, null);
            }

            targetCache.SetValueExtIfDifferent<FSSODet.tranDesc>(targetRow, sourceRow.TranDesc);

            return targetRow;
        }
        #endregion

        #region Extensions
        #region QuickProcess
        public PXQuickProcess.Action<FSAppointment>.ConfiguredBy<FSAppQuickProcessParams> quickProcess;

        [PXButton(CommitChanges = true), PXUIField(DisplayName = "Quick Process")]
        protected virtual IEnumerable QuickProcess(PXAdapter adapter) => AppointmentQuickProcessExt.ButtonHandler(adapter);

        public AppointmentQuickProcess AppointmentQuickProcessExt => GetExtension<AppointmentQuickProcess>();

        public class AppointmentQuickProcess : PXGraphExtension<AppointmentEntry>
        {
            public PXSelect<SOOrderTypeQuickProcess, Where<SOOrderTypeQuickProcess.orderType, Equal<Current<FSSrvOrdType.postOrderType>>>> currentSOOrderType;
            public static bool isSOInvoice;


            public virtual IEnumerable<FSAppointment> ButtonHandler(PXAdapter adapter)
            {
                QuickProcessParameters.AskExt(InitQuickProcessPanel);

                if (Base.AppointmentRecords.AllowUpdate == true)
                {
                    Base.Save.Press();
                }

                PXQuickProcess.Start(Base, Base.AppointmentRecords.Current, QuickProcessParameters.Current);

                return new[] { Base.AppointmentRecords.Current };
            }

            public PXAction<FSAppointment> quickProcessOk;
            [PXButton, PXUIField(DisplayName = "OK")]
            public virtual IEnumerable QuickProcessOk(PXAdapter adapter)
            {
                Base.AppointmentRecords.Current.IsCalledFromQuickProcess = true;
                return adapter.Get();
            }

            public PXFilter<FSAppQuickProcessParams> QuickProcessParameters;

            protected virtual void FSAppointment_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSAppointment fsAppointmentRow = (FSAppointment)e.Row;

                bool enableQuickProcess = (Base.ServiceOrderTypeSelected.Current == null ? false : Base.ServiceOrderTypeSelected.Current.AllowQuickProcess.Value)
                                            && fsAppointmentRow.PendingAPARSOPost.Value
                                                && Base.BillingCycleRelated.Current?.BillingBy == ID.Billing_By.APPOINTMENT
                                                    && fsAppointmentRow.Hold == false
                                                        && (fsAppointmentRow.Status == ID.Status_Appointment.COMPLETED || fsAppointmentRow.Status == ID.Status_Appointment.CLOSED);

                if (currentSOOrderType.Current == null)
                {
                    currentSOOrderType.Current = currentSOOrderType.Select();
                }

                isSOInvoice = Base.ServiceOrderTypeSelected.Current?.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;

                Base.quickProcess.SetEnabled(enableQuickProcess);
                Base.quickProcess.SetVisible(enableQuickProcess);
            }

            protected virtual void FSAppQuickProcessParams_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                quickProcessOk.SetEnabled(true);

                FSAppQuickProcessParams fsQuickProcessParametersRow = (FSAppQuickProcessParams)e.Row;
                SetQuickProcessSettingsVisibility(cache, Base.ServiceOrderTypeSelected.Current, Base.AppointmentRecords.Current, fsQuickProcessParametersRow);

                if (isSOInvoice == true)
                {
                    PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.generateInvoiceFromAppointment>(cache, fsQuickProcessParametersRow, true);
                    PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.releaseInvoice>(cache, fsQuickProcessParametersRow, fsQuickProcessParametersRow.GenerateInvoiceFromAppointment == true);
                    PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.emailInvoice>(cache, fsQuickProcessParametersRow, fsQuickProcessParametersRow.GenerateInvoiceFromAppointment == true);
                }
            }

            private void SetQuickProcessSettingsVisibility(PXCache cache, FSSrvOrdType fsSrvOrdTypeRow, FSAppointment fsAppointmentRow, FSAppQuickProcessParams fsQuickProcessParametersRow)
            {
                if (fsSrvOrdTypeRow != null)
                {
                    bool isInvoiceBehavior = false;
                    bool orderTypeQuickProcessIsEnabled = false;
                    bool postToSO = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_MODULE;
                    bool postToSOInvoice = fsSrvOrdTypeRow.PostTo == ID.SrvOrdType_PostTo.SALES_ORDER_INVOICE;
                    bool enableSOQuickProcess = postToSO;

                    if (postToSO && currentSOOrderType.Current?.AllowQuickProcess != null)
                    {
                        isInvoiceBehavior = currentSOOrderType.Current.Behavior == SOBehavior.IN;
                        orderTypeQuickProcessIsEnabled = (bool)currentSOOrderType.Current.AllowQuickProcess;
                    }
                    else if (postToSOInvoice)
                    {
                        isInvoiceBehavior = true;
                    }

                    enableSOQuickProcess = orderTypeQuickProcessIsEnabled 
                                                && fsQuickProcessParametersRow.GenerateInvoiceFromAppointment == true 
                                                    && (fsQuickProcessParametersRow.PrepareInvoice == false || fsQuickProcessParametersRow.SOQuickProcess == true);

                    PXUIFieldAttribute.SetVisible<FSAppQuickProcessParams.sOQuickProcess>(cache, fsQuickProcessParametersRow, postToSO && orderTypeQuickProcessIsEnabled);
                    PXUIFieldAttribute.SetVisible<FSAppQuickProcessParams.emailSalesOrder>(cache, fsQuickProcessParametersRow, postToSO);
                    PXUIFieldAttribute.SetVisible<FSAppQuickProcessParams.prepareInvoice>(cache, fsQuickProcessParametersRow, postToSO && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);
                    PXUIFieldAttribute.SetVisible<FSAppQuickProcessParams.releaseInvoice>(cache, fsQuickProcessParametersRow, (postToSO || postToSOInvoice) && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);
                    PXUIFieldAttribute.SetVisible<FSAppQuickProcessParams.emailInvoice>(cache, fsQuickProcessParametersRow, (postToSO || postToSOInvoice) && isInvoiceBehavior && fsQuickProcessParametersRow.SOQuickProcess == false);

                    PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.sOQuickProcess>(cache, fsQuickProcessParametersRow, enableSOQuickProcess);
                    PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.emailSignedAppointment>(cache, fsQuickProcessParametersRow, Base.emailSignedAppointment.GetEnabled());

                    if (fsQuickProcessParametersRow.ReleaseInvoice == false
                        && fsQuickProcessParametersRow.EmailInvoice == false
                            && fsQuickProcessParametersRow.SOQuickProcess == false
                                && fsQuickProcessParametersRow.GenerateInvoiceFromAppointment == true)
                    {
                        PXUIFieldAttribute.SetEnabled<FSAppQuickProcessParams.prepareInvoice>(cache, fsQuickProcessParametersRow, true);
                    }

                    if (Base.emailSignedAppointment.GetEnabled() == false)
                    {
                        cache.RaiseExceptionHandling<FSQuickProcessParameters.emailSignedAppointment>(fsQuickProcessParametersRow, 
                                                                                                      false, 
                                                                                                      new PXSetPropertyException(TX.Warning.SIGNED_APP_EMAIL_ACTION_IS_DISABLED, PXErrorLevel.Warning));
                    }
                }
            }

            private static string[] GetExcludeFiels()
            {
                string[] excludeFields = {
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.closeAppointment>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.generateInvoiceFromAppointment>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.sOQuickProcess>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.emailSalesOrder>(),
                    SharedFunctions.GetFieldName<FSQuickProcessParameters.srvOrdType>()
                };

                return excludeFields;
            }

            private static void SetQuickProcessOptions(PXGraph graph, PXCache targetCache, FSAppQuickProcessParams fsAppQuickProcessParamsRow, bool ignoreUpdateSOQuickProcess)
            {
                var ext = ((AppointmentEntry)graph).AppointmentQuickProcessExt;

                if (string.IsNullOrEmpty(ext.QuickProcessParameters.Current.OrderType))
                {
                    ext.QuickProcessParameters.Cache.Clear();
                    ResetSalesOrderQuickProcessValues(ext.QuickProcessParameters.Current);
                }

                if (fsAppQuickProcessParamsRow != null)
                {
                    ResetSalesOrderQuickProcessValues(fsAppQuickProcessParamsRow);
                }

                FSQuickProcessParameters fsQuickProcessParametersRow =
                    PXSelectReadonly<
                        FSQuickProcessParameters,
                    Where<
                        FSQuickProcessParameters.srvOrdType, Equal<Current<FSAppointment.srvOrdType>>>>
                    .Select(ext.Base);

                bool isSOQuickProcess = (fsAppQuickProcessParamsRow != null && fsAppQuickProcessParamsRow.SOQuickProcess == true)
                                            || (fsAppQuickProcessParamsRow == null && fsQuickProcessParametersRow?.SOQuickProcess == true);

                var cache = targetCache ?? ext.QuickProcessParameters.Cache;
                var row = fsAppQuickProcessParamsRow ?? ext.QuickProcessParameters.Current;

                if (ext.currentSOOrderType.Current?.AllowQuickProcess == true && isSOQuickProcess)
                {
                    var cacheFSAppQuickProcessParams = new PXCache<FSAppQuickProcessParams>(ext.Base);

                    FSAppQuickProcessParams fsAppQuickProcessParamsFromDB =
                        PXSelectReadonly<FSAppQuickProcessParams,
                        Where<
                            FSAppQuickProcessParams.orderType, Equal<Current<FSSrvOrdType.postOrderType>>>>
                        .Select(ext.Base);

                    SharedFunctions.CopyCommonFields(cache,
                                                     row,
                                                     cacheFSAppQuickProcessParams,
                                                     fsAppQuickProcessParamsFromDB,
                                                     GetExcludeFiels());
                }
                else
                {
                    SetCommonValues(row, fsQuickProcessParametersRow);
                }

                if (ignoreUpdateSOQuickProcess == false)
                {
                    SetServiceOrderTypeValues(graph, row, fsQuickProcessParametersRow);
                }
            }

            public static void InitQuickProcessPanel(PXGraph graph, string viewName)
            {
                SetQuickProcessOptions(graph, null, null, false);
            }

            protected virtual void FSAppQuickProcessParams_SOQuickProcess_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSAppQuickProcessParams fsAppQuickProcessParamsRow = (FSAppQuickProcessParams)e.Row;

                if (fsAppQuickProcessParamsRow.SOQuickProcess != (bool)e.OldValue)
                {
                    SetQuickProcessOptions(Base, cache, fsAppQuickProcessParamsRow, true);
                }
            }

            protected virtual void FSAppQuickProcessParams_GenerateInvoiceFromAppointment_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSAppQuickProcessParams fsAppQuickProcessParamsRow = (FSAppQuickProcessParams)e.Row;

                if (isSOInvoice
                        && fsAppQuickProcessParamsRow.GenerateInvoiceFromAppointment != (bool)e.OldValue)
                {
                    cache.SetValueExt<FSAppQuickProcessParams.prepareInvoice>(fsAppQuickProcessParamsRow, fsAppQuickProcessParamsRow.GenerateInvoiceFromAppointment == true);
                }
            }

            protected virtual void FSAppQuickProcessParams_PrepareInvoice_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
            {
                if (e.Row == null)
                {
                    return;
                }

                FSAppQuickProcessParams fsAppQuickProcessParamsRow = (FSAppQuickProcessParams)e.Row;

                if (isSOInvoice
                        && fsAppQuickProcessParamsRow.PrepareInvoice != (bool)e.OldValue
                            && fsAppQuickProcessParamsRow.PrepareInvoice == false)
                {
                    fsAppQuickProcessParamsRow.ReleaseInvoice = false;
                    fsAppQuickProcessParamsRow.EmailInvoice = false;
                }
            }

            private static void ResetSalesOrderQuickProcessValues(FSAppQuickProcessParams fsAppQuickProcessParamsRow)
            {
                fsAppQuickProcessParamsRow.CreateShipment = false;
                fsAppQuickProcessParamsRow.ConfirmShipment = false;
                fsAppQuickProcessParamsRow.UpdateIN = false;
                fsAppQuickProcessParamsRow.PrepareInvoiceFromShipment = false;
                fsAppQuickProcessParamsRow.PrepareInvoice = false;
                fsAppQuickProcessParamsRow.EmailInvoice = false;
                fsAppQuickProcessParamsRow.ReleaseInvoice = false;
                fsAppQuickProcessParamsRow.AutoRedirect = false;
                fsAppQuickProcessParamsRow.AutoDownloadReports = false;
            }

            private static void SetCommonValues(FSAppQuickProcessParams fsAppQuickProcessParamsRowTarget, FSQuickProcessParameters FSQuickProcessParametersRowSource)
            {
                if (isSOInvoice && fsAppQuickProcessParamsRowTarget.GenerateInvoiceFromAppointment == true)
                {
                    fsAppQuickProcessParamsRowTarget.PrepareInvoice = true;
                }
                else
                {
                    fsAppQuickProcessParamsRowTarget.PrepareInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment.Value ? FSQuickProcessParametersRowSource.PrepareInvoice : false;
                }

                fsAppQuickProcessParamsRowTarget.ReleaseInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment.Value ? FSQuickProcessParametersRowSource.ReleaseInvoice : false;
                fsAppQuickProcessParamsRowTarget.EmailInvoice = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment.Value ? FSQuickProcessParametersRowSource.EmailInvoice : false;
            }

            private static void SetServiceOrderTypeValues(PXGraph graph, FSAppQuickProcessParams fsAppQuickProcessParamsRowTarget, FSQuickProcessParameters FSQuickProcessParametersRowSource)
            {
                fsAppQuickProcessParamsRowTarget.CloseAppointment = FSQuickProcessParametersRowSource.CloseAppointment;
                fsAppQuickProcessParamsRowTarget.EmailSignedAppointment = ((AppointmentEntry)graph).emailSignedAppointment.GetEnabled() && (FSQuickProcessParametersRowSource.EmailSignedAppointment.HasValue && FSQuickProcessParametersRowSource.EmailSignedAppointment.Value);
                fsAppQuickProcessParamsRowTarget.GenerateInvoiceFromAppointment = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment;
                fsAppQuickProcessParamsRowTarget.EmailSalesOrder = FSQuickProcessParametersRowSource.GenerateInvoiceFromAppointment.Value ? FSQuickProcessParametersRowSource.EmailSalesOrder : false;
                fsAppQuickProcessParamsRowTarget.SOQuickProcess = FSQuickProcessParametersRowSource.SOQuickProcess;
                fsAppQuickProcessParamsRowTarget.SrvOrdType = FSQuickProcessParametersRowSource.SrvOrdType;

                if (isSOInvoice == true && fsAppQuickProcessParamsRowTarget.GenerateInvoiceFromAppointment == true)
                {
                    fsAppQuickProcessParamsRowTarget.PrepareInvoice = true;
                }
            }
        }
        #endregion

        #region Multi Currency
        public class MultiCurrency : SMMultiCurrencyGraph<AppointmentEntry, FSAppointment>
        {
            protected override PXSelectBase[] GetChildren()
            {
                return new PXSelectBase[]
                {
                    Base.AppointmentRecords,
                    Base.AppointmentDetParts,
                    Base.AppointmentDetServices,
                    Base.PickupDeliveryItems,
                    Base.ServiceOrderRelated
                };
            }

            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSAppointment))
                {
                    BAccountID = typeof(FSAppointment.billCustomerID),
                    DocumentDate = typeof(FSAppointment.executionDate)
                };
            }

            protected override void _(Events.FieldUpdated<Extensions.MultiCurrency.Document, Extensions.MultiCurrency.Document.bAccountID> e)
            {
                if (e.Row == null) return;

                var doc = Documents.Cache.GetMain<Extensions.MultiCurrency.Document>(e.Row);

                if (doc is FSAppointment)
                {
                    AppointmentEntry graph = (AppointmentEntry)Documents.Cache.Graph;

                    if (e.ExternalCall || e.Row?.CuryID == null || graph.recalculateCuryID == true)
                    {
                        //TODO Delete this content in 2019R2 and use SourceFieldUpdated method.
                        if (PXAccess.FeatureInstalled<FeaturesSet.multicurrency>())
                        {
                            CurrencyInfo info = GetCurrencyInfo(e.Row);
                            if (info != null)
                            {
                                CurrencyInfo old = PXCache<CurrencyInfo>.CreateCopy(info);
                                currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyID>(info);
                                currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyRateTypeID>(info);
                                currencyinfo.Cache.SetDefaultExt<CurrencyInfo.curyEffDate>(info);
                                if (currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Notchanged || currencyinfo.Cache.GetStatus(info) == PXEntryStatus.Held)
                                {
                                    currencyinfo.Cache.SetStatus(info, PXEntryStatus.Updated);
                                }
                                currencyinfo.Cache.RaiseRowUpdated(info, old);
                            }
                            string message = PXUIFieldAttribute.GetError<CurrencyInfo.curyEffDate>(e.Cache.Graph.Caches[typeof(CurrencyInfo)], info);
                            if (string.IsNullOrEmpty(message) == false)
                            {
                                Documents.Cache.RaiseExceptionHandling<Extensions.MultiCurrency.Document.documentDate>(e.Row,
                                    e.Row.DocumentDate,
                                    new PXSetPropertyException(message, PXErrorLevel.Warning));
                            }

                            if (info != null)
                            {
                                Documents.Cache.SetValue<Extensions.MultiCurrency.Document.curyID>(e.Row, info.CuryID);
                            }
                        }
                    }
                }
                else 
                {
                    base._(e);
                }
            }
        }
        #endregion

        #region Sales Taxes
        public class SalesTax : TaxGraph<AppointmentEntry, FSAppointment>
        {
            protected override DocumentMapping GetDocumentMapping()
            {
                return new DocumentMapping(typeof(FSAppointment))
                {
                    DocumentDate = typeof(FSAppointment.executionDate),
                    CuryDocBal = typeof(FSAppointment.curyDocTotal),
                    CuryDiscountLineTotal = typeof(FSAppointment.curyDiscTot),
                    CuryDiscTot = typeof(FSAppointment.curyDiscTot),
                    BranchID = typeof(FSAppointment.branchID),
                    FinPeriodID = typeof(FSAppointment.finPeriodID),
                    TaxZoneID = typeof(FSAppointment.taxZoneID),
                    CuryLinetotal = typeof(FSAppointment.curyBillableLineTotal),
                    CuryTaxTotal = typeof(FSAppointment.curyTaxTotal)
                };
            }

            protected override DetailMapping GetDetailMapping()
            {
                return new DetailMapping(typeof(FSAppointmentDetUNION))
                {
                    CuryTranAmt = typeof(FSAppointmentDetUNION.curyBillableTranAmt),
                    TaxCategoryID = typeof(FSAppointmentDetUNION.taxCategoryID),
                    DocumentDiscountRate = typeof(FSAppointmentDetUNION.documentDiscountRate),
                    GroupDiscountRate = typeof(FSAppointmentDetUNION.groupDiscountRate)
                };
            }

            protected override void CurrencyInfo_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
            {
                TaxCalc taxCalc = CurrentDocument.TaxCalc ?? TaxCalc.NoCalc;
                if (taxCalc == TaxCalc.Calc || taxCalc == TaxCalc.ManualLineCalc)
                {
                    if (e.Row != null && ((CurrencyInfo)e.Row).CuryRate != null && (e.OldRow == null || !sender.ObjectsEqual<CurrencyInfo.curyRate, CurrencyInfo.curyMultDiv>(e.Row, e.OldRow)))
                    {
                        if (Base.AppointmentDetServices.SelectSingle() != null
                                || Base.AppointmentDetParts.SelectSingle() != null
                                || Base.PickupDeliveryItems.SelectSingle() != null)
                        {
                            CalcTaxes(null);
                        }
                    }
                }
            }

            protected override TaxDetailMapping GetTaxDetailMapping()
            {
                return new TaxDetailMapping(typeof(FSAppointmentTax), typeof(FSAppointmentTax.taxID));
            }

            protected override TaxTotalMapping GetTaxTotalMapping()
            {
                return new TaxTotalMapping(typeof(FSAppointmentTaxTran), typeof(FSAppointmentTaxTran.taxID));
            }

            protected virtual void Document_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                var row = sender.GetExtension<Extensions.SalesTax.Document>(e.Row);
                if (row.TaxCalc == null)
                    row.TaxCalc = TaxCalc.Calc;
            }

            protected override void CalcDocTotals(object row, decimal CuryTaxTotal, decimal CuryInclTaxTotal, decimal CuryWhTaxTotal)
            {
                base.CalcDocTotals(row, CuryTaxTotal, CuryInclTaxTotal, CuryWhTaxTotal);
                FSAppointment doc = (FSAppointment)this.Documents.Cache.GetMain<Extensions.SalesTax.Document>(this.Documents.Current);

                decimal CuryLineTotal = (decimal)(ParentGetValue<FSAppointment.curyBillableLineTotal>() ?? 0m);
                decimal CuryDiscTotal = (decimal)(ParentGetValue<FSAppointment.curyDiscTot>() ?? 0m);

                decimal CuryDocTotal = GetCuryDocTotal(CuryLineTotal, CuryDiscTotal,
                                                CuryTaxTotal, CuryInclTaxTotal);

                if (object.Equals(CuryDocTotal, (decimal)(ParentGetValue<FSAppointment.curyDocTotal>() ?? 0m)) == false)
                {
                    ParentSetValue<FSAppointment.curyDocTotal>(CuryDocTotal);
                }
            }

            protected override List<object> SelectTaxes<Where>(PXGraph graph, object row, PXTaxCheck taxchk, params object[] parameters)
            {
                Dictionary<string, PXResult<Tax, TaxRev>> tail = new Dictionary<string, PXResult<Tax, TaxRev>>();
                var currents = new[]
                {
                    row != null && row is Extensions.SalesTax.Detail ? Details.Cache.GetMain((Extensions.SalesTax.Detail)row):null,
                    ((AppointmentEntry)graph).AppointmentSelected.Current
                };

                foreach (PXResult<Tax, TaxRev> record in PXSelectReadonly2<Tax,
                    LeftJoin<TaxRev, On<TaxRev.taxID, Equal<Tax.taxID>,
                        And<TaxRev.outdated, Equal<False>,
                            And<TaxRev.taxType, Equal<TaxType.sales>,
                            And<Tax.taxType, NotEqual<CSTaxType.withholding>,
                            And<Tax.taxType, NotEqual<CSTaxType.use>,
                            And<Tax.reverseTax, Equal<False>,
                            And<Current<FSAppointment.executionDate>, Between<TaxRev.startDate, TaxRev.endDate>>>>>>>>>,
                    Where>
                    .SelectMultiBound(graph, currents, parameters))
                {
                    tail[((Tax)record).TaxID] = record;
                }
                List<object> ret = new List<object>();
                switch (taxchk)
                {
                    case PXTaxCheck.Line:
                        foreach (FSAppointmentTax record in PXSelect<FSAppointmentTax,
                            Where<FSAppointmentTax.entityID, Equal<Current<FSAppointment.appointmentID>>,
                                And<FSAppointmentTax.lineNbr, Equal<Current<FSAppointmentDetUNION.apptLineNbr>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && String.Compare(((Tax)(PXResult<FSAppointmentTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSAppointmentTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcLine:
                        foreach (FSAppointmentTax record in PXSelect<FSAppointmentTax,
                            Where<FSAppointmentTax.entityID, Equal<Current<FSAppointment.appointmentID>>,
                                And<FSAppointmentTax.lineNbr, Less<intMax>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && ((FSAppointmentTax)(PXResult<FSAppointmentTax, Tax, TaxRev>)ret[idx - 1]).LineNbr == record.LineNbr
                                    && String.Compare(((Tax)(PXResult<FSAppointmentTax, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSAppointmentTax, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    case PXTaxCheck.RecalcTotals:
                        foreach (FSAppointmentTaxTran record in PXSelect<FSAppointmentTaxTran,
                            Where<FSAppointmentTaxTran.entityID, Equal<Current<FSAppointment.appointmentID>>>,
                            OrderBy<Asc<FSAppointmentTaxTran.entityID, Asc<FSAppointmentTaxTran.lineNbr, Asc<FSAppointmentTaxTran.taxID>>>>>
                            .SelectMultiBound(graph, currents))
                        {
                            PXResult<Tax, TaxRev> line;
                            if (record.TaxID != null && tail.TryGetValue(record.TaxID, out line))
                            {
                                int idx;
                                for (idx = ret.Count;
                                    (idx > 0)
                                    && String.Compare(((Tax)(PXResult<FSAppointmentTaxTran, Tax, TaxRev>)ret[idx - 1]).TaxCalcLevel, ((Tax)line).TaxCalcLevel) > 0;
                                    idx--) ;
                                ret.Insert(idx, new PXResult<FSAppointmentTaxTran, Tax, TaxRev>(record, (Tax)line, (TaxRev)line));
                            }
                        }
                        return ret;
                    default:
                        return ret;
                }
            }

            #region FSAppointmentTaxTran
            protected virtual void FSAppointmentTaxTran_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
            {
                if (e.Row == null)
                    return;

                PXUIFieldAttribute.SetEnabled<FSAppointmentTaxTran.taxID>(sender, e.Row, sender.GetStatus(e.Row) == PXEntryStatus.Inserted);
            }
            #endregion

            #region FSAppointmentTax
            protected virtual void FSAppointmentTaxTran_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
            {
                FSAppointmentTaxTran row = e.Row as FSAppointmentTaxTran;
                if (row == null) return;

                if (e.Operation == PXDBOperation.Delete)
                {
                    FSAppointmentTax tax = (FSAppointmentTax)Base.TaxLines.Cache.Locate(FindFSAppointmentTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Deleted ||
                         Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.InsertedDeleted)
                        e.Cancel = true;
                }
                if (e.Operation == PXDBOperation.Update)
                {
                    FSAppointmentTax tax = (FSAppointmentTax)Base.TaxLines.Cache.Locate(FindFSAppointmentTax(row));
                    if (Base.TaxLines.Cache.GetStatus(tax) == PXEntryStatus.Updated)
                        e.Cancel = true;
                }
            }

            internal static FSAppointmentTax FindFSAppointmentTax(FSAppointmentTaxTran tran)
            {
                var list = PXSelect<FSAppointmentTax,
                    Where<FSAppointmentTax.entityID, Equal<Required<FSAppointmentTax.entityID>>,
                        And<FSAppointmentTax.lineNbr, Equal<Required<FSAppointmentTax.lineNbr>>,
                        And<FSAppointmentTax.taxID, Equal<Required<FSAppointmentTax.taxID>>>>>>
                        .SelectSingleBound(new PXGraph(), new object[] { }).RowCast<FSAppointmentTax>();
                return list.FirstOrDefault();
            }
            #endregion

            #region FSAppointment
            protected virtual void FSAppointment_taxZoneID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
            {
                var row = e.Row as FSAppointment;
                if (row == null) return;

                FSServiceOrder fsServiceOrderRow = ((AppointmentEntry)sender.Graph).ServiceOrderRelated.Current;
                if (fsServiceOrderRow == null)
                {
                    return;
                }

                var customerLocation = (Location)PXSelect<Location,
                    Where<Location.bAccountID, Equal<Required<Location.bAccountID>>,
                        And<Location.locationID, Equal<Required<Location.locationID>>>>>.
                    Select(sender.Graph, row.BillCustomerID, fsServiceOrderRow.BillLocationID);
                if (customerLocation != null)
                {
                    if (!string.IsNullOrEmpty(customerLocation.CTaxZoneID))
                    {
                        e.NewValue = customerLocation.CTaxZoneID;
                    }
                    else
                    {
                        var address = (Address)PXSelect<Address,
                            Where<Address.addressID, Equal<Required<Address.addressID>>>>.
                            Select(sender.Graph, customerLocation.DefAddressID);
                        if (address != null && !string.IsNullOrEmpty(address.PostalCode))
                        {
                            e.NewValue = TaxBuilderEngine.GetTaxZoneByZip(sender.Graph, address.PostalCode);
                        }
                    }
                }
                if (e.NewValue == null)
                {
                    var branchLocationResult = (PXResult<PX.Objects.GL.Branch, BAccount, Location>)PXSelectJoin<PX.Objects.GL.Branch,
                            InnerJoin<BAccount, On<BAccount.bAccountID, Equal<PX.Objects.GL.Branch.bAccountID>>,
                            InnerJoin<Location, On<Location.locationID, Equal<BAccount.defLocationID>>>>,
                        Where<
                            PX.Objects.GL.Branch.branchID, Equal<Required<PX.Objects.GL.Branch.branchID>>>>
                        .Select(sender.Graph, row.BranchID);

                    var branchLocation = (Location)branchLocationResult;

                    if (branchLocation != null && branchLocation.VTaxZoneID != null)
                        e.NewValue = branchLocation.VTaxZoneID;
                    else
                        e.NewValue = row.TaxZoneID;
                }
            }
            #endregion
        }
        #endregion

        #region Contact/Address
        public class ContactAddress : SrvOrdContactAddressGraph<AppointmentEntry>
        {
        }
        #endregion

        #region Service Registration
        public class ServiceRegistration : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.ActivateOnApplicationStart<ExtensionSorting>();
            }
            private class ExtensionSorting
            {
                private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
                {
                    {typeof(ContactAddress), 1},
                    {typeof(MultiCurrency), 2},
                    /*{typeof(SalesPrice), 3},
                    {typeof(Discount), 4},*/
                    {typeof(SalesTax), 5},
                };
                public ExtensionSorting()
                {
                    PXBuildManager.SortExtensions += (list) => PXBuildManager.PartialSort(list, _order);
                }
            }
        }
        #endregion
        #endregion
    }
}
