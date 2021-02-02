using PX.Common;
using PX.Data;
using PX.FS;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.IN;
using PX.SM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PX.Objects.FS
{
    public class ExternalControls : PXGraph<ExternalControls>
    {
        #region CalendarsSelect

        #region Setup

        [PXHidden]
        public PXSelect<BAccount> BAccounts;

        public PXSelect<FSSetup> SetupRecords;

        public PXSelect<UserPreferences,
               Where<
                   PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>> UserPreferencesRecords;

        #region Calendar
        public PXSelect<CSCalendar,
               Where<
                   CSCalendar.calendarID, Equal<Required<CSCalendar.calendarID>>>> CSCalendar;

        public PXSelect<CSCalendarExceptions,
               Where<
                   CSCalendarExceptions.calendarID, Equal<Required<CSCalendarExceptions.calendarID>>,
                   And<CSCalendarExceptions.date, Equal<Required<CSCalendarExceptions.date>>>>> CSCalendarExceptions;

        public PXSelect<CSCalendarExceptions,
               Where<
                   CSCalendarExceptions.calendarID, Equal<Required<CSCalendarExceptions.calendarID>>,
                   And<CSCalendarExceptions.date, GreaterEqual<Required<CSCalendarExceptions.date>>,
                   And<CSCalendarExceptions.date, LessEqual<Required<CSCalendarExceptions.date>>>>>> FromToCSCalendarExceptions;
        #endregion

        #region CalendarPreferences

        #region CustomFieldAppointment
        public PXSelect<FSCustomFieldAppointment,
                Where<
                    FSCustomFieldAppointment.customFieldAppointmentID, Equal<Required<FSCustomFieldAppointment.customFieldAppointmentID>>>> CustomFieldAppointment;

        public PXSelectOrderBy<FSCustomFieldAppointment,
               OrderBy<
                   Asc<FSCustomFieldAppointment.position>>> CustomFieldAppointments;

        public PXSelect<FSCustomFieldAppointment,
               Where<
                   FSCustomFieldAppointment.active, Equal<Required<FSCustomFieldAppointment.active>>>,
               OrderBy<
                   Asc<FSCustomFieldAppointment.position>>> ActiveCustomFieldAppointments;
        #endregion

        #region CustomAppointmentStatus
        public PXSelect<FSCustomAppointmentStatus> CustomAppointmentStatuses;

        public PXSelect<FSCustomAppointmentStatus,
               Where<
                   FSCustomAppointmentStatus.customAppointmentStatusID, Equal<Required<FSCustomAppointmentStatus.customAppointmentStatusID>>>> CustomAppointmentStatus;

        public PXSelect<FSCustomAppointmentStatus,
               Where<
                   FSCustomAppointmentStatus.fieldName, Equal<Required<FSCustomAppointmentStatus.fieldName>>>> CustomAppointmentStatusName;
        #endregion

        #endregion

        #endregion

        #region FSRoom

        public PXSelect<FSRoom,
               Where<
                   FSRoom.branchLocationID, Equal<Required<FSRoom.branchLocationID>>,
                   And<FSRoom.roomID, Equal<Required<FSRoom.roomID>>>>,
               OrderBy<
                   Asc<FSRoom.descr>>> RoomByBranchLocation;

        public PXSelect<FSRoom,
               Where<
                   FSRoom.branchLocationID, Equal<Required<FSRoom.branchLocationID>>>,
               OrderBy<
                   Asc<FSRoom.descr>>> RoomRecordsByBranchLocation;

        #endregion

        #region SingleSelects

        public PXSelect<FSSkill> SkillRecords;
        public PXSelect<FSGeoZone> GeoZoneRecords;
        public PXSelect<FSProblem> ProblemRecords;
        public PXSelect<FSLicenseType> LicenseTypeRecords;

        #region Extensions

        public PXSelect<FSEquipment,
               Where<
                   FSEquipment.resourceEquipment, Equal<True>>> Resources;

        public PXSelect<INItemClass,
               Where<
                   INItemClass.itemType, Equal<INItemTypes.serviceItem>>> ServiceClasses;

        public PXSelect<InventoryItem,
               Where<
                   InventoryItem.itemStatus, Equal<InventoryItemStatus.active>,
                   And<InventoryItem.itemType, Equal<INItemTypes.serviceItem>,
                   And<Match<Current<AccessInfo.userName>>>>>> Services;
        #endregion

        #region BranchLocation

        public PXSelect<FSBranchLocation,
               Where<
                   FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>> BranchLocationRecord;

        public PXSelect<FSBranchLocation,
               Where<
                   FSBranchLocation.branchID, Equal<Required<FSBranchLocation.branchID>>,
                   Or<Required<FSBranchLocation.branchID>, IsNull>>> BranchLocationRecordsByBranch;

        #endregion

        #endregion

        #region ServiceOrderType

        public PXSelect<FSSrvOrdType,
               Where<
                   FSSrvOrdType.srvOrdType, Equal<Required<FSSrvOrdType.srvOrdType>>>> SrvOrdTypeRecord;

        public PXSelect<FSSrvOrdType, 
               Where<
                   FSSrvOrdType.behavior, NotEqual<FSSrvOrdType.behavior.Quote>, 
                   And<FSSrvOrdType.active, Equal<True>>>> SrvOrdTypeRecords;

        #endregion

        #region Appointment

        public PXSelect<FSAppointment,
               Where<
                   FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>> AppointmentRecord;

        public PXSelectJoin<FSAppointment,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<FSAppointmentEmployee,
                    On<
                        FSAppointmentEmployee.appointmentID, Equal<FSAppointment.appointmentID>>,
                LeftJoin<EPEmployee,
                    On<
                        EPEmployee.bAccountID, Equal<FSAppointmentEmployee.employeeID>>,
                LeftJoin<Location,
                    On<
                        Location.locationID, Equal<FSServiceOrder.locationID>>,
                InnerJoin<FSContact,
                    On<
                        FSContact.contactID, Equal<FSServiceOrder.serviceOrderContactID>>,
                LeftJoin<BAccount,
                    On<
                                            BAccount.bAccountID, Equal<FSServiceOrder.customerID>>,
                                    LeftJoinSingleTable<Customer,
                                        On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>>>>>,
                Where<
                    FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                And<
                    FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                And<
                                        FSAppointment.scheduledDateTimeEnd, Less<Required<FSAppointment.scheduledDateTimeEnd>>,
                                    And<
                                        Where<Customer.bAccountID, IsNull,
                                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>,
                OrderBy<
                    Asc<
                        FSAppointmentEmployee.employeeID>>> AppointmentRecords;

        public PXSelectJoin<EPEmployee,
                InnerJoin<FSAppointmentEmployee,
                    On<
                        FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
                InnerJoin<FSAppointment,
                    On<
                        FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                InnerJoin<FSSrvOrdType,
                    On<
                        FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                InnerJoin<BAccountStaffMember,
                    On<
                        BAccountStaffMember.bAccountID, Equal<EPEmployee.bAccountID>>,
                LeftJoin<Location,
                    On<
                        Location.locationID, Equal<FSServiceOrder.locationID>>,
                InnerJoin<FSAddress,
                    On<
                        FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<Country,
                    On
                        <Country.countryID, Equal<FSAddress.countryID>>,
                LeftJoin<State,
                    On<
                        State.countryID, Equal<FSAddress.countryID>,
                        And<State.stateID, Equal<FSAddress.state>>>,
                                LeftJoin<Customer,
                    On<
                                        Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>>>>>>>>,
                Where<
                    FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                And<
                    FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                And<
                    FSAppointment.scheduledDateTimeEnd, Less<Required<FSAppointment.scheduledDateTimeEnd>>,
                And<
                                    FSAppointmentEmployee.employeeID, Equal<Required<FSAppointmentEmployee.employeeID>>,
                                And<
                                    Where<Customer.bAccountID, IsNull,
                                    Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>,
                OrderBy<
                    Asc<FSAppointment.scheduledDateTimeBegin>>> AppointmentsByEmployee;

        public PXSelect<FSAppointmentEmployee,
               Where<
                   FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>,
               And<
                   FSAppointmentEmployee.employeeID, Equal<Required<FSAppointmentEmployee.employeeID>>>>> AppointmentEmployeeByEmployee;

        public PXSelect<FSAppointmentEmployee,
               Where<
                   FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>>> AppointmentEmployees;

        public PXSelectJoin<EPEmployee,
               InnerJoin<FSAppointmentEmployee,
               On<
                   FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>>,
               Where<
                   FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>>> AppointmentEPEmployees;

        public PXSelectJoin<BAccountStaffMember,
               InnerJoin<FSAppointmentEmployee,
               On<
                   FSAppointmentEmployee.employeeID, Equal<BAccountStaffMember.bAccountID>>>,
               Where<
                   FSAppointmentEmployee.appointmentID, Equal<Required<FSAppointmentEmployee.appointmentID>>>> AppointmentBAccountStaffMember;

        #region AppointmentDets

        public PXSelectJoin<FSAppointmentDet,
               InnerJoin<FSSODet,
               On<
                   FSSODet.sODetID, Equal<FSAppointmentDet.sODetID>>>,
               Where<
                   FSSODet.lineType, Equal<ListField_LineType_UnifyTabs.Service>,
                   And<FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>>>> AppointmentDets;

        public PXSelectJoin<FSAppointmentDet,
               InnerJoin<FSAppointment,
               On<
                   FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
               Where<
                   FSAppointmentDet.sODetID, Equal<Required<FSAppointmentDet.sODetID>>,
               And<
                   Where<
                       FSAppointment.status, Equal<FSAppointment.status.ManualScheduled>,
                       Or<FSAppointment.status, Equal<FSAppointment.status.AutomaticScheduled>,
                       Or<FSAppointment.status, Equal<FSAppointment.status.InProcess>>>>>>> ActiveAppointmentDetsBySO;

        public PXSelectJoin<FSAppointmentDet,
               InnerJoin<FSAppointment,
               On<
                   FSAppointment.appointmentID, Equal<FSAppointmentDet.appointmentID>>>,
               Where<
                   FSAppointment.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>,
               And<
                   Where<
                       FSAppointmentDet.status, Equal<FSAppointmentDet.status.NotStarted>,
                       Or<FSAppointmentDet.status, Equal<FSAppointmentDet.status.InProcess>>>>>> ActiveAppointmentDets;

        public PXSelectJoin<InventoryItem,
               InnerJoin<FSSODet,
               On<
                   FSSODet.inventoryID, Equal<InventoryItem.inventoryID>>,
               InnerJoin<FSAppointmentDet,
               On<
                   FSAppointmentDet.sODetID, Equal<FSSODet.sODetID>>,
               LeftJoin<INItemClass,
               On<
                   INItemClass.itemClassID, Equal<InventoryItem.itemClassID>>>>>,
               Where<
                   FSSODet.lineType, Equal<ListField_LineType_UnifyTabs.Service>,
                   And<FSAppointmentDet.appointmentID, Equal<Required<FSAppointmentDet.appointmentID>>>>> AppointmentServices;

        public PXSelectJoin<FSEquipment,
               InnerJoin<FSAppointmentResource,
               On<
                   FSAppointmentResource.SMequipmentID, Equal<FSEquipment.SMequipmentID>>>,
               Where<
                   FSAppointmentResource.appointmentID, Equal<Required<FSAppointmentResource.appointmentID>>>> AppointmentResources;

        #endregion

        #endregion

        #region Service Order

        public PXSelect<FSServiceOrder,
               Where<
                   FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>> ServiceOrderRecord;

        public PXSelectJoin<FSServiceOrder,
               InnerJoin<FSAppointment,
               On<
                   FSAppointment.sOID, Equal<FSServiceOrder.sOID>>>,
               Where<
                   FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                   And<FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>>>> ServiceOrderByAppointment;

        #endregion

        #region Contact Info

        public PXSelect<FSContact,
               Where<
                   FSContact.contactID, Equal<Required<FSContact.contactID>>>> ServiceOrderContact;

		public PXSelect<Contact,
               Where<
                   Contact.bAccountID, Equal<Optional<EPEmployee.parentBAccountID>>,
               And<
                   Contact.contactID, Equal<Optional<EPEmployee.defContactID>>>>> EmployeeContact;

        public PXSelect<Contact,
               Where<
                   Contact.contactID, Equal<Optional<Vendor.defContactID>>>> VendorContact;

        public PXSelect<Customer,
               Where<
                   Customer.bAccountID, Equal<Required<Customer.bAccountID>>>> Customer;

        #endregion
 
        #region InventoryItem

        public PXSelect<InventoryItem,
               Where<
                   InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>> InventoryItem;

        public PXSelectJoin<InventoryItem,
               InnerJoin<FSSODet,
               On<
                   FSSODet.inventoryID, Equal<InventoryItem.inventoryID>>>,
               Where<
                   FSSODet.lineType, Equal<ListField_LineType_UnifyTabs.Service>,
               And<
                   FSSODet.sODetID, Equal<Required<FSSODet.sODetID>>>>> InventoryItemBySODet;

        #endregion

        #region Employee

        public PXSelectJoin<EPEmployee,
               LeftJoin<Contact,
               On<
                   Contact.contactID, Equal<EPEmployee.defContactID>>>,
               Where<
                   EPEmployee.userID, Equal<Current<AccessInfo.userID>>>> EmployeeSelected;

        public PXSelect<BAccount,
               Where<
                   BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>> EmployeeBAccount;
            
        public PXSelectJoinGroupBy<EPEmployee,
                InnerJoin<FSAppointmentEmployee,
                    On<
                        FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
                InnerJoin<FSAppointment,
                    On<
                        FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<GL.Branch,
                    On<
                                GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>,
                        LeftJoin<BAccount,
                            On<
                                BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
                        LeftJoinSingleTable<Customer,
                            On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>>>>,
                Where<
                    FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                And<
                    FSAppointment.scheduledDateTimeEnd, Less<Required<FSAppointment.scheduledDateTimeEnd>>,
                And<
                            Where2<
                                Where<
                                    GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>,
                                        Or<GL.Branch.branchID, IsNull>>,
                            And<
                                Where<Customer.bAccountID, IsNull,
                                Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>,
                Aggregate<
                    GroupBy<EPEmployee.bAccountID>>,
                OrderBy<
                    Asc<EPEmployee.acctName>>> EmployeeAppointmentsByDate;

        public PXSelectJoinGroupBy<EPEmployee,
                InnerJoin<FSAppointmentEmployee,
                    On<
                        FSAppointmentEmployee.employeeID, Equal<EPEmployee.bAccountID>>,
                InnerJoin<FSAppointment,
                    On<
                        FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<GL.Branch,
                    On<
                                GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>,
                        LeftJoin<BAccount,
                            On<
                                BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
                        LeftJoinSingleTable<Customer,
                            On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>>>>,
                Where<
                    FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                And<
                    FSAppointment.scheduledDateTimeEnd, Less<Required<FSAppointment.scheduledDateTimeEnd>>,
                And<
                    FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>,
                And<
                            Where2<
                                Where<
                                     GL.Branch.branchID, Equal<Required<GL.Branch.branchID>>,
                                     Or<GL.Branch.branchID, IsNull>>,
                                And<
                                    Where<Customer.bAccountID, IsNull,
                                    Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>,
                Aggregate<
                    GroupBy<EPEmployee.bAccountID>>,
                OrderBy<
                    Asc<EPEmployee.acctName>>> EmployeeAppointmentsByDateAndBranchLocation;
          
        #endregion

        #region SODetServices

        public PXSelect<FSSODet,
               Where<
                   FSSODet.sOID, Equal<Required<FSSODet.sOID>>,
                   And<FSSODet.lineType, Equal<ListField_LineType_UnifyTabs.Service>>>> SODetServices;
							
        public PXSelect<FSSODet,
               Where<
                       FSSODet.sOID, Equal<Required<FSSODet.sOID>>,
                       And<FSSODet.lineType, Equal<ListField_LineType_UnifyTabs.Service>,
                       And<FSSODet.status, Equal<FSSODet.status.ScheduleNeeded>>>>> SODetPendingLines;

        #endregion

        #region TimeSlot

        public PXSelect<FSTimeSlot,
               Where<
                   FSTimeSlot.timeSlotID, Equal<Required<FSTimeSlot.timeSlotID>>>> TimeSlotRecord;

        #endregion

        #region RouteSelect

        #region Route

        public PXSelect<FSRouteDocument,
               Where<
                   FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>> RouteRecord;

        public PXSelectJoin<FSRouteDocument,
                LeftJoin<FSRoute,
                    On<
                        FSRoute.routeID, Equal<FSRouteDocument.routeID>>,
                LeftJoin<FSEquipment,
                    On<
                        FSEquipment.SMequipmentID, Equal<FSRouteDocument.vehicleID>>,
                LeftJoin<EPEmployee,
                    On<
                        EPEmployee.bAccountID, Equal<FSRouteDocument.driverID>>>>>,
                Where<
                        FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>,
                    And<
                        FSRouteDocument.branchID, Equal<Required<FSRouteDocument.branchID>>>>> RouteRecordsByDate;

        public PXSelectJoin<FSRouteDocument,
                LeftJoin<FSRoute,
                    On<
                        FSRoute.routeID, Equal<FSRouteDocument.routeID>>,
                LeftJoin<FSEquipment,
                    On<
                        FSEquipment.SMequipmentID, Equal<FSRouteDocument.vehicleID>>,
                LeftJoin<EPEmployee,
                    On<
                        EPEmployee.bAccountID, Equal<FSRouteDocument.driverID>>>>>,
                Where<
                        FSRouteDocument.date, Equal<Required<FSRouteDocument.date>>,
                    And<
                        FSRouteDocument.refNbr, Equal<Required<FSRouteDocument.refNbr>>,
                    And<
                        FSRouteDocument.branchID, Equal<Required<FSRouteDocument.branchID>>>>>> RouteRecordsByDateAndRefNbr;

        public PXSelectJoin<FSRouteDocument,
                InnerJoin<FSRoute,
                    On<
                        FSRoute.routeID, Equal<FSRouteDocument.routeID>>,
                InnerJoin<EPEmployee,
                    On<
                        EPEmployee.bAccountID, Equal<FSRouteDocument.driverID>>,
                LeftJoin<BAccount,
                    On<
                        BAccount.bAccountID, Equal<EPEmployee.bAccountID>>,
                LeftJoin<FSEquipment,
                    On<
                        FSEquipment.SMequipmentID, Equal<FSRouteDocument.vehicleID>>>>>>,
                Where<
                    FSRouteDocument.date, GreaterEqual<Required<FSRouteDocument.date>>,
                And<
                    FSRouteDocument.date, Less<Required<FSRouteDocument.date>>,
                And<
                    FSRouteDocument.branchID, Equal<Required<FSRouteDocument.branchID>>>>>,
                OrderBy<
                    Asc<FSRouteDocument.timeBegin>>> RoutesAndDriversByDate;

        public PXSelectJoin<FSGPSTrackingRequest,
                InnerJoin<Users,
                    On<
                        Users.username, Equal<FSGPSTrackingRequest.userName>>,
                InnerJoin<EPEmployee,
                    On<
                        EPEmployee.userID, Equal<Users.pKID>>>>,
                Where<
                    EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>> GPSTrackingRequestByEmployee;

        public PXSelect<FSGPSTrackingHistory,
               Where<
                   FSGPSTrackingHistory.trackingID, Equal<Required<FSGPSTrackingHistory.trackingID>>>,
               OrderBy<
                   Desc<FSGPSTrackingHistory.executionDate>>> FSGPSTrackingHistoryByTrackingID;

        public PXSelect<FSGPSTrackingHistory,
                Where<
                    FSGPSTrackingHistory.trackingID, Equal<Required<FSGPSTrackingHistory.trackingID>>,
                And<
                    FSGPSTrackingHistory.executionDate, GreaterEqual<Required<FSGPSTrackingHistory.executionDate>>,
                And<
                    FSGPSTrackingHistory.executionDate, LessEqual<Required<FSGPSTrackingHistory.executionDate>>>>>,
                OrderBy<
                    Asc<FSGPSTrackingHistory.executionDate>>> FSGPSTrackingHistoryByTrackingIDAndDate;
        #endregion

        #region AppointmentRoute

        public PXSelectJoin<FSAppointment,
                InnerJoin<FSServiceOrder,
                    On<
                        FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                LeftJoin<FSSrvOrdType,
                    On<
                        FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                LeftJoin<Location,
                    On<
                        Location.locationID, Equal<FSServiceOrder.locationID>>,
                InnerJoin<FSAddress,
                    On<
                        FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                LeftJoin<Country,
                    On
                        <Country.countryID, Equal<FSAddress.countryID>>,
                LeftJoin<State,
                    On<
                        State.countryID, Equal<FSAddress.countryID>,
                        And<State.stateID, Equal<FSAddress.state>>>,
                        LeftJoin<Customer,
                    On<
                                Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>>>>>,
                Where<
                    FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                And<
                    FSAppointment.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>,
                OrderBy<
                    Asc<FSAppointment.routePosition>>> AppointmentRecordsByRoute;

        #endregion

        #endregion

        #endregion

        #region Functions

        #region Calendar Preferences

        public DispatchBoardAppointmentMessages CreateCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customFieldAppointments != null)
                {
                    var graphCustomFieldAppointmentsMaint = PXGraph.CreateInstance<CustomFieldAppointmentsMaint>();

                    foreach (FSCustomFieldAppointment fsCustomFieldAppointmentRow_InList in customFieldAppointments)
                    {
                        FSCustomFieldAppointment fsCustomFieldAppointmentRow = this.CustomFieldAppointment.Select(fsCustomFieldAppointmentRow_InList.CustomFieldAppointmentID);

                        if (fsCustomFieldAppointmentRow != null)
                        {
                            fsCustomFieldAppointmentRow.Active = fsCustomFieldAppointmentRow_InList.Active;
                            fsCustomFieldAppointmentRow.Position = fsCustomFieldAppointmentRow_InList.Position;
                            fsCustomFieldAppointmentRow.FieldDescr = fsCustomFieldAppointmentRow_InList.FieldDescr;
                            graphCustomFieldAppointmentsMaint.CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow);

                            if (graphCustomFieldAppointmentsMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                        else
                        {
                            fsCustomFieldAppointmentRow = new FSCustomFieldAppointment()
                            {
                                Active = fsCustomFieldAppointmentRow_InList.Active,
                                Position = fsCustomFieldAppointmentRow_InList.Position,
                                FieldDescr = fsCustomFieldAppointmentRow_InList.FieldDescr,
                                CustomFieldAppointmentID = fsCustomFieldAppointmentRow_InList.CustomFieldAppointmentID
                            };

                            graphCustomFieldAppointmentsMaint.CustomFieldAppointmentRecords.Insert(fsCustomFieldAppointmentRow);

                            if (graphCustomFieldAppointmentsMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages PutCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customFieldAppointments != null)
                {
                    var graphCustomFieldAppointmentsMaint = PXGraph.CreateInstance<CustomFieldAppointmentsMaint>();

                    foreach (FSCustomFieldAppointment fsCustomFieldAppointmentRow_InList in customFieldAppointments)
                    {
                        FSCustomFieldAppointment fsCustomFieldAppointmentRow = this.CustomFieldAppointment.Select(fsCustomFieldAppointmentRow_InList.CustomFieldAppointmentID);

                        if (fsCustomFieldAppointmentRow != null)
                        {
                            fsCustomFieldAppointmentRow.CustomFieldAppointmentID = fsCustomFieldAppointmentRow_InList.CustomFieldAppointmentID;
                            fsCustomFieldAppointmentRow.Active = fsCustomFieldAppointmentRow_InList.Active;
                            fsCustomFieldAppointmentRow.Position = fsCustomFieldAppointmentRow_InList.Position;
                            fsCustomFieldAppointmentRow.FieldDescr = fsCustomFieldAppointmentRow_InList.FieldDescr;
                            fsCustomFieldAppointmentRow.FieldImg = fsCustomFieldAppointmentRow_InList.FieldImg;

                            graphCustomFieldAppointmentsMaint.CustomFieldAppointmentRecords.Update(fsCustomFieldAppointmentRow);

                            if (graphCustomFieldAppointmentsMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                        else
                        {
                            messages.ErrorMessages.Add(TX.Error.CUSTOM_APPOINTMENT_FIELD_NOT_FOUND);
                            return messages;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages DeleteCustomFieldAppointments(FSCustomFieldAppointment[] customFieldAppointments)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customFieldAppointments != null)
                {
                    var graphCustomFieldAppointmentsMaint = PXGraph.CreateInstance<CustomFieldAppointmentsMaint>();

                    using (var ts = new PXTransactionScope())
                    {
                        foreach (FSCustomFieldAppointment fsCustomFieldAppointmentRow_InList in customFieldAppointments)
                        {
                            FSCustomFieldAppointment fsCustomFieldAppointmentRow = this.CustomFieldAppointment.Select(fsCustomFieldAppointmentRow_InList.CustomFieldAppointmentID);

                            if (fsCustomFieldAppointmentRow != null)
                            {
                                graphCustomFieldAppointmentsMaint.CustomFieldAppointmentRecords.Delete(fsCustomFieldAppointmentRow);
                            }
                            else
                            {
                                messages.ErrorMessages.Add(TX.Error.CUSTOM_APPOINTMENT_FIELD_NOT_FOUND);
                                return messages;
                            }
                        }

                        if (graphCustomFieldAppointmentsMaint.PressSave(messages) == false)
                        {
                            return messages;
                        }

                        ts.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages CreateCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customAppointmentStatuses != null)
                {
                    var graphCustomAppointmentStatusMaint = PXGraph.CreateInstance<CustomAppointmentStatusMaint>();

                    foreach (FSCustomAppointmentStatus fsCustomAppointmentStatusRow_InList in customAppointmentStatuses)
                    {
                        FSCustomAppointmentStatus fsCustomAppointmentStatusRow = this.CustomAppointmentStatusName.Select(fsCustomAppointmentStatusRow_InList.FieldName);

                        if (fsCustomAppointmentStatusRow != null)
                        {
                            graphCustomAppointmentStatusMaint.Clear();
                            fsCustomAppointmentStatusRow.BackgroundColor = fsCustomAppointmentStatusRow_InList.BackgroundColor;
                            fsCustomAppointmentStatusRow.TextColor = fsCustomAppointmentStatusRow_InList.TextColor;
                            fsCustomAppointmentStatusRow.FieldName = fsCustomAppointmentStatusRow_InList.FieldName;
                            fsCustomAppointmentStatusRow.FieldDescr = fsCustomAppointmentStatusRow_InList.FieldDescr;
                            fsCustomAppointmentStatusRow.HideStatus = fsCustomAppointmentStatusRow_InList.HideStatus;

                            graphCustomAppointmentStatusMaint.CustomAppointmentStatusRecords.Update(fsCustomAppointmentStatusRow);

                            if (graphCustomAppointmentStatusMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                        else
                        {
                            fsCustomAppointmentStatusRow = new FSCustomAppointmentStatus();

                            graphCustomAppointmentStatusMaint.Clear();
                            fsCustomAppointmentStatusRow.BackgroundColor = fsCustomAppointmentStatusRow_InList.BackgroundColor;
                            fsCustomAppointmentStatusRow.TextColor = fsCustomAppointmentStatusRow_InList.TextColor;
                            fsCustomAppointmentStatusRow.FieldName = fsCustomAppointmentStatusRow_InList.FieldName;
                            fsCustomAppointmentStatusRow.FieldDescr = fsCustomAppointmentStatusRow_InList.FieldDescr;
                            fsCustomAppointmentStatusRow.HideStatus = fsCustomAppointmentStatusRow_InList.HideStatus;

                            graphCustomAppointmentStatusMaint.CustomAppointmentStatusRecords.Insert(fsCustomAppointmentStatusRow);

                            if (graphCustomAppointmentStatusMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages PutCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customAppointmentStatuses != null)
                {
                    var graphCustomAppointmentStatusMaint = PXGraph.CreateInstance<CustomAppointmentStatusMaint>();

                    foreach (FSCustomAppointmentStatus fsCustomAppointmentStatusRow_InList in customAppointmentStatuses)
                    {
                        FSCustomAppointmentStatus fsCustomAppointmentStatusRow = this.CustomAppointmentStatusName.Select(fsCustomAppointmentStatusRow_InList.FieldName);

                        if (fsCustomAppointmentStatusRow != null)
                        {
                            fsCustomAppointmentStatusRow.BackgroundColor = fsCustomAppointmentStatusRow_InList.BackgroundColor;
                            fsCustomAppointmentStatusRow.TextColor = fsCustomAppointmentStatusRow_InList.TextColor;
                            fsCustomAppointmentStatusRow.FieldName = fsCustomAppointmentStatusRow_InList.FieldName;
                            fsCustomAppointmentStatusRow.FieldDescr = fsCustomAppointmentStatusRow_InList.FieldDescr;
                            fsCustomAppointmentStatusRow.HideStatus = fsCustomAppointmentStatusRow_InList.HideStatus;

                            graphCustomAppointmentStatusMaint.CustomAppointmentStatusRecords.Update(fsCustomAppointmentStatusRow);

                            if (graphCustomAppointmentStatusMaint.PressSave(messages) == false)
                            {
                                return messages;
                            }
                        }
                        else
                        {
                            messages.ErrorMessages.Add(TX.Error.CUSTOM_APPOINTMENT_STATUS_NOT_FOUND);
                            return messages;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages DeleteCustomAppointmentStatuses(FSCustomAppointmentStatus[] customAppointmentStatuses)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                if (customAppointmentStatuses != null)
                {
                    var graphCustomAppointmentStatusMaint = PXGraph.CreateInstance<CustomAppointmentStatusMaint>();

                    using (var ts = new PXTransactionScope())
                    {
                        foreach (FSCustomAppointmentStatus fsCustomAppointmentStatusesRow_InList in customAppointmentStatuses)
                        {
                            FSCustomAppointmentStatus fsCustomAppointmentStatusesRow = this.CustomAppointmentStatus.Select(fsCustomAppointmentStatusesRow_InList.CustomAppointmentStatusID);

                            if (fsCustomAppointmentStatusesRow != null)
                            {
                                graphCustomAppointmentStatusMaint.CustomAppointmentStatusRecords.Delete(fsCustomAppointmentStatusesRow);
                            }
                            else
                            {
                                messages.ErrorMessages.Add(TX.Error.CUSTOM_APPOINTMENT_STATUS_NOT_FOUND);
                                return messages;
                            }
                        }

                        if (graphCustomAppointmentStatusMaint.PressSave(messages) == false)
                        {
                            return messages;
                        }

                        ts.Complete();
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        #endregion

        #region Service Order

        public static PXResultset<FSServiceOrder> ServiceOrderRecords(int? branchID, int? branchLocationID, DispatchBoardFilters[] filters, DateTime? scheduledDateStart = null, DateTime? scheduledDateEnd = null, bool? isRoomCalendar = false)
        {
            var graph = PXGraph.CreateInstance<ExternalControls>();
            List<object> args = new List<object>();
            List<object> retList = new List<object>();

            //Get all ServiceOrders
            PXSelectBase<FSServiceOrder> servicesOrdersBase = new PXSelectJoin<FSServiceOrder,
                                                                  InnerJoin<FSSrvOrdType,
                                                                  On<
                                                                      FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                                                                  InnerJoin<BAccount,
                                                                  On<
                                                                            BAccount.bAccountID, Equal<FSServiceOrder.customerID>>,
                                                                    LeftJoinSingleTable<Customer,
                                                                        On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>>>>,
                                                                  Where<
                                                                      FSServiceOrder.status, NotEqual<FSServiceOrder.status.Quote>,
                                                                  And<
                                                                      FSServiceOrder.status, NotEqual<FSServiceOrder.status.Hold>,
                                                                  And<
                                                                      FSServiceOrder.status, NotEqual<FSServiceOrder.status.Closed>,
                                                                  And<
                                                                      FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>,
                                                                  And<
                                                                      FSServiceOrder.status, NotEqual<FSServiceOrder.status.Completed>,
                                                                  And<
                                                                      FSServiceOrder.appointmentsNeeded, Equal<True>,
                                                                  And<
                                                                        BAccount.type, NotEqual<BAccountType.prospectType>,
                                                                    And<
                                                                        Where<Customer.bAccountID, IsNull,
                                                                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>>>>
                                                                  (graph);

            if (branchID != null)
            {
                servicesOrdersBase.WhereAnd<Where<FSServiceOrder.branchID, Equal<Required<FSServiceOrder.branchID>>>>();
                args.Add(branchID);
            }

            if (scheduledDateStart != null)
            {
                servicesOrdersBase.WhereAnd<Where2<
                                                Where<
                                                    FSServiceOrder.sLAETA, IsNull,
                                                And<
                                                    FSServiceOrder.orderDate, GreaterEqual<Required<FSServiceOrder.orderDate>>>>,
                                            Or<
                                                Where<
                                                    FSServiceOrder.sLAETA, IsNotNull,
                                                And<
                                                    FSServiceOrder.sLAETA, GreaterEqual<Required<FSServiceOrder.sLAETA>>>>>>>();

                args.Add(scheduledDateStart);
                args.Add(scheduledDateStart);
            }

            if (scheduledDateEnd != null)
            {
                servicesOrdersBase.WhereAnd<Where2<
                                                Where<
                                                    FSServiceOrder.sLAETA, IsNotNull,
                                                And<
                                                    FSServiceOrder.sLAETA, LessEqual<Required<FSServiceOrder.sLAETA>>>>,
                                            Or<
                                                FSServiceOrder.orderDate, LessEqual<Required<FSServiceOrder.orderDate>>>>>();

                args.Add(scheduledDateEnd);
                args.Add(scheduledDateEnd);
            }

            if (branchLocationID != null)
            {
                servicesOrdersBase.WhereAnd<Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>>();

                args.Add(branchLocationID);
            }

            if (isRoomCalendar == true)
            {
                servicesOrdersBase.WhereAnd<Where<FSServiceOrder.roomID, IsNull>>();
            }

            PXResultset<FSServiceOrder> bqlResultSet_FSServicesOrdersToFilter;

            if (filters != null)
            {
                bool auxLike = false;
                bool auxAssignedEmployee = false;
                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i].property == TX.Dispatch_Board.LIKETEXT_FILTER)
                    {
                        servicesOrdersBase.WhereAnd<Where<
                                                        FSServiceOrder.refNbr, Like<Required<FSServiceOrder.memRefNbr>>,
                                                        Or<BAccount.acctName, Like<Required<FSServiceOrder.memAcctName>>>>>();

                        args.Add(string.Concat("%", filters[i].value[0], "%"));
                        args.Add(string.Concat("%", filters[i].value[0], "%"));
                        auxLike = true;

                        if (auxAssignedEmployee)
                        {
                            break;
                        }
                    }

                    if (filters[i].property == TX.Dispatch_Board.ASSIGNED_EMPLOYEE_FILTER)
                    {
                        servicesOrdersBase.WhereAnd<Where<FSServiceOrder.assignedEmpID, Equal<Required<FSServiceOrder.assignedEmpID>>>>();
                        args.Add(filters[i].value[0]);
                        auxAssignedEmployee = true;

                        if (auxLike)
                        {
                            break;
                        }
                    }
                }

                bqlResultSet_FSServicesOrdersToFilter = (args.Count > 0) ? servicesOrdersBase.Select(args.ToArray()) : servicesOrdersBase.Select();

                for (int i = 0; i < bqlResultSet_FSServicesOrdersToFilter.Count; i++)
                {
                    FSServiceOrder fsServiceOrderRow = (FSServiceOrder)bqlResultSet_FSServicesOrdersToFilter[i];
                    
                    var bqlResultSet = graph.SODetPendingLines.Select(fsServiceOrderRow.SOID).ToList();

                    List<int> listFilter = new List<int>();
                    List<int> listSkills = new List<int>();
                    List<int> listLicenseTypes = new List<int>();
                    List<int> listProblem = new List<int>();
                    List<int> listServiceClasses = new List<int>();

                    bool removeServiceOrder = false;

                    if (bqlResultSet.Count == 0)
                    {
                        bqlResultSet_FSServicesOrdersToFilter.RemoveAt(i--);
                    }
                    else
                    {
                        List<int?> serviceIDList = bqlResultSet.Select(y => y.GetItem<FSSODet>().InventoryID).ToList();
                        List<int?> soDetIDList = bqlResultSet.Select(y => y.GetItem<FSSODet>().SODetID).ToList();

                        List<SharedClasses.ItemList> serviceSkills = SharedFunctions.GetItemWithList<FSServiceSkill,
                                                                                                     FSServiceSkill.serviceID,
                                                                                                     FSServiceSkill.skillID>
                                                                                                     (graph, serviceIDList);

                        List<SharedClasses.ItemList> serviceLicenseTypes = SharedFunctions.GetItemWithList<FSServiceLicenseType,
                                                                                                           FSServiceLicenseType.serviceID,
                                                                                                           FSServiceLicenseType.licenseTypeID>
                                                                                                           (graph, serviceIDList);

                        List<SharedClasses.ItemList> serviceItemClass = SharedFunctions.GetItemWithList<InventoryItem,
                                                                                                        InventoryItem.inventoryID,
                                                                                                        InventoryItem.itemClassID>
                                                                                                        (graph, serviceIDList);

                        //Iterate to get Skills, LicenseTypes and ServiceClasses for each SODet
                        foreach (FSSODet fsSODetRow in bqlResultSet)
                        {
                            SharedClasses.ItemList serviceSkillList = serviceSkills.FirstOrDefault(y => y.itemID == fsSODetRow.InventoryID);
                            SharedClasses.ItemList serviceLicenseTypeList = serviceLicenseTypes.FirstOrDefault(y => y.itemID == fsSODetRow.InventoryID);
                            SharedClasses.ItemList serviceItemClassList = serviceItemClass.FirstOrDefault(y => y.itemID == fsSODetRow.InventoryID);

                            if (serviceSkillList != null)
                            {
                                foreach (int serviceSkillID in serviceSkillList.list)
                                {
                                    listSkills.Add(serviceSkillID);
                                }
                            }

                            if (serviceLicenseTypeList != null)
                            {
                                foreach (int serviceLicenseTypeID in serviceLicenseTypeList.list)
                                {
                                    listLicenseTypes.Add(serviceLicenseTypeID);
                                }
                            }

                            if (serviceItemClassList != null)
                            {
                                foreach (int serviceItemClassID in serviceItemClassList.list)
                                {
                                    listServiceClasses.Add(serviceItemClassID);
                                }
                            }

                            if (fsServiceOrderRow.ProblemID != null)
                            {
                                listProblem.Add((int)fsServiceOrderRow.ProblemID);
                            }
                        }

                        //Iterate for each filter and try to filter in each List above.
                        foreach (ExternalControls.DispatchBoardFilters filter in filters)
                        {
                            switch (filter.property)
                            {
                                case TX.Dispatch_Board.SKILL_FILTER:
                                    listFilter = filter.value.Select(int.Parse).ToList();
                                    if (listFilter.Except(listSkills).Any())
                                    {
                                        removeServiceOrder = true;
                                        break;
                                    }

                                    break;
                                case TX.Dispatch_Board.LICENSE_TYPE_FILTER:
                                    listFilter = filter.value.Select(int.Parse).ToList();
                                    if (listFilter.Except(listLicenseTypes).Any())
                                    {
                                        removeServiceOrder = true;
                                        break;
                                    }

                                    break;
                                case TX.Dispatch_Board.PROBLEM_FILTER:
                                    listFilter = filter.value.Select(int.Parse).ToList();
                                    if (listFilter.Except(listProblem).Any())
                                    {
                                        removeServiceOrder = true;
                                        break;
                                    }

                                    break;
                                case TX.Dispatch_Board.SERVICE_CLASS_FILTER:
                                    listFilter = filter.value.Select(int.Parse).ToList();
                                    if (listFilter.Except(listServiceClasses).Any())
                                    {
                                        removeServiceOrder = true;
                                        break;
                                    }

                                    break;
                                default:

                                    break;
                            }
                        }

                        //If ServiceOrder does not pass the filters it is removed from the List.
                        if (removeServiceOrder)
                        {
                            bqlResultSet_FSServicesOrdersToFilter.RemoveAt(i--);
                        }
                    }
                }
            }
            else
            {
                bqlResultSet_FSServicesOrdersToFilter = (args.Count > 0) ? servicesOrdersBase.Select(args.ToArray()) : servicesOrdersBase.Select();
            }

            return bqlResultSet_FSServicesOrdersToFilter;
        }
        
        #endregion

        #region Appointment

        /// <summary>
        /// Gets the appointment records related for the given dates.
        /// </summary>
        /// <param name="timeBegin">Schedule Start Date.</param>
        /// <param name="timeEnd">Schedule End Date.</param>
        /// <param name="branchID">Current Branch ID.</param>
        /// <param name="branchLocationID">Branch Location ID.</param>
        /// <param name="employeeIDList">Employee id list.</param>
        /// <returns>Appointment list.</returns>
        public List<FSAppointmentStaffScheduleBoard> GetAppointmentRecords(DateTime timeBegin, DateTime timeEnd, int? branchID, int? branchLocationID, int[] employeeIDList)
        {
            List<object> args = new List<object>();
            List<FSAppointmentStaffScheduleBoard> appointmentList = new List<FSAppointmentStaffScheduleBoard>();

            BqlCommand appointmentRecords = new Select2<FSAppointmentStaffScheduleBoard,
                                                        LeftJoinSingleTable<Customer,
                                                            On<Customer.bAccountID, Equal<FSAppointmentStaffScheduleBoard.customerID>>>,
                                                        Where<
                                                        FSAppointmentStaffScheduleBoard.status, NotEqual<FSAppointmentStaffScheduleBoard.status.Canceled>,
                                                        And2<
                                                Where<
                                                    Where2<
                                                        Where<
                                                            FSAppointmentStaffScheduleBoard.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointmentStaffScheduleBoard.scheduledDateTimeBegin>>,
                                                            And<FSAppointmentStaffScheduleBoard.scheduledDateTimeBegin, LessEqual<Required<FSAppointmentStaffScheduleBoard.scheduledDateTimeEnd>>>>,
                                                        Or<
                                                            Where<
                                                                FSAppointmentStaffScheduleBoard.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointmentStaffScheduleBoard.scheduledDateTimeBegin>>,
                                                                And<FSAppointmentStaffScheduleBoard.scheduledDateTimeEnd, LessEqual<Required<FSAppointmentStaffScheduleBoard.scheduledDateTimeEnd>>>>>>>,
                                                        And<
                                                            Where<Customer.bAccountID, IsNull,
                                                            Or<Match<Customer, Current<AccessInfo.userName>>>>>>>,
                                                OrderBy<
                                                    Asc<FSAppointmentStaffScheduleBoard.appointmentID>>>();
            args.Add(timeBegin);
            args.Add(timeEnd);
            args.Add(timeBegin);
            args.Add(timeEnd);

            if (branchID != null)
            {
                appointmentRecords = appointmentRecords.WhereAnd(typeof(Where<FSAppointmentStaffScheduleBoard.branchID, Equal<Required<FSAppointmentStaffScheduleBoard.branchID>>>));
                args.Add(branchID);
            }

            if (branchLocationID != null)
            {
                appointmentRecords = appointmentRecords.WhereAnd(typeof(Where<FSAppointmentStaffScheduleBoard.branchLocationID, Equal<Required<FSAppointmentStaffScheduleBoard.branchLocationID>>>));
                args.Add(branchLocationID);
            }

            if (employeeIDList != null && employeeIDList.Length > 0)
            {
                appointmentRecords = appointmentRecords.WhereAnd(InHelper<FSAppointmentStaffScheduleBoard.employeeID>.Create(employeeIDList.Length));
                foreach (int employeeID in employeeIDList)
                {
                    args.Add(employeeID);
                }
            }

            PXResultset<FSCustomAppointmentStatus> bqlResultSet = PXSelect<FSCustomAppointmentStatus,
                                                                  Where<
                                                                        FSCustomAppointmentStatus.hideStatus, Equal<False>>>
                                                                  .Select(this);

            List<string> appointmentStatusesList = new List<string>();

            foreach (FSCustomAppointmentStatus fsCustomAppointmentStatusRow in bqlResultSet)
            {
                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.CANCELED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.CANCELED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.COMPLETED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.COMPLETED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.IN_PROCESS && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.IN_PROCESS);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.SCHEDULED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.MANUAL_SCHEDULED);
                    appointmentStatusesList.Add(ID.Status_Appointment.AUTOMATIC_SCHEDULED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.CLOSED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.CLOSED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.ON_HOLD && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.ON_HOLD);
                }
            }

            if (appointmentStatusesList.Count > 0)
            {
                appointmentRecords = appointmentRecords.WhereAnd(InHelper<FSAppointmentStaffScheduleBoard.status>.Create(appointmentStatusesList.Count));
            }
            else
            {
                return appointmentList;
            }

            foreach (string appointmentStatus in appointmentStatusesList)
            {
                args.Add(appointmentStatus);
            }

            PXView appointmentRecordsView = new PXView(this, true, appointmentRecords);
            var results = appointmentRecordsView.SelectMulti(args.ToArray());

            foreach (PXResult<FSAppointmentStaffScheduleBoard, Customer> result in results)
            {
                appointmentList.Add(GetStaffCalendarAppointment((FSAppointmentStaffScheduleBoard)result));
            }

            return appointmentList;
        }

        /// <summary>
        /// Gets the appointment records related for the given dates.
        /// </summary>
        /// <param name="timeBegin">Schedule Start Date.</param>
        /// <param name="timeEnd">Schedule End Date.</param>
        /// <param name="branchID">Current Branch ID.</param>
        /// <param name="branchLocationID">Branch Location ID.</param>
        /// <param name="roomIDList">Room id list.</param>
        /// <returns>Appointment list.</returns>
        public List<FSAppointmentScheduleBoard> GetAppointmentRecordsByRooms(DateTime timeBegin, DateTime timeEnd, int? branchID, int? branchLocationID, int[] roomIDList)
        {
            List<object> args = new List<object>();
            List<FSAppointmentScheduleBoard> appointmentList = new List<FSAppointmentScheduleBoard>();

            BqlCommand appointmentRecords = new Select2<FSAppointmentScheduleBoard,
                                                    LeftJoinSingleTable<Customer,
                                                        On<Customer.bAccountID, Equal<FSAppointmentScheduleBoard.customerID>>>,
                                                    Where<
                                                    FSAppointmentScheduleBoard.status, NotEqual<FSAppointmentScheduleBoard.status.Canceled>,
                                                    And2<
                                                Where<
                                                    Where2<
                                                        Where<
                                                            FSAppointmentScheduleBoard.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeBegin>>,
                                                            And<FSAppointmentScheduleBoard.scheduledDateTimeBegin, LessEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeEnd>>>>,
                                                        Or<
                                                            Where<FSAppointmentScheduleBoard.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeBegin>>,
                                                            And<FSAppointmentScheduleBoard.scheduledDateTimeEnd, LessEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeEnd>>>>>>>,
                                                    And<
                                                        Where<Customer.bAccountID, IsNull,
                                                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>,
                                                OrderBy<
                                                    Asc<FSAppointmentScheduleBoard.appointmentID>>>();
            args.Add(timeBegin);
            args.Add(timeEnd);
            args.Add(timeBegin);
            args.Add(timeEnd);

            if (branchID != null)
            {
                appointmentRecords = appointmentRecords.WhereAnd(typeof(Where<FSAppointmentScheduleBoard.branchID, Equal<Required<FSAppointmentStaffScheduleBoard.branchID>>>));
                args.Add(branchID);
            }

            if (branchLocationID != null)
            {
                appointmentRecords = appointmentRecords.WhereAnd(typeof(Where<FSAppointmentScheduleBoard.branchLocationID, Equal<Required<FSAppointmentStaffScheduleBoard.branchLocationID>>>));
                args.Add(branchLocationID);
            }

            if (roomIDList != null && roomIDList.Length > 0)
            {
                appointmentRecords = appointmentRecords.WhereAnd(InHelper<FSAppointmentScheduleBoard.roomID>.Create(roomIDList.Length));
                foreach (int employeeID in roomIDList)
                {
                    args.Add(employeeID);
                }
            }

            PXResultset<FSCustomAppointmentStatus> bqlResultSet = PXSelect<FSCustomAppointmentStatus,
                                                                  Where<
                                                                        FSCustomAppointmentStatus.hideStatus, Equal<False>>>
                                                                  .Select(this);

            List<string> appointmentStatusesList = new List<string>();

            foreach (FSCustomAppointmentStatus fsCustomAppointmentStatusRow in bqlResultSet)
            {
                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.CANCELED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.CANCELED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.COMPLETED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.COMPLETED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.IN_PROCESS && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.IN_PROCESS);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.SCHEDULED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.MANUAL_SCHEDULED);
                    appointmentStatusesList.Add(ID.Status_Appointment.AUTOMATIC_SCHEDULED);
                }

                if (fsCustomAppointmentStatusRow.FieldName == TX.Status_Appointment.CLOSED && fsCustomAppointmentStatusRow.HideStatus == false)
                {
                    appointmentStatusesList.Add(ID.Status_Appointment.CLOSED);
                }
            }

            if (appointmentStatusesList.Count > 0)
            {
                appointmentRecords = appointmentRecords.WhereAnd(InHelper<FSAppointmentScheduleBoard.status>.Create(appointmentStatusesList.Count));
            }

            foreach (string appointmentStatus in appointmentStatusesList)
            {
                args.Add(appointmentStatus);
            }

            PXView appointmentRecordsView = new PXView(this, true, appointmentRecords);
            var results = appointmentRecordsView.SelectMulti(args.ToArray());

            foreach (PXResult<FSAppointmentScheduleBoard, Customer> result in results)
            {
                appointmentList.Add(GetCalendarAppointment((FSAppointmentScheduleBoard)result));
            }

            return appointmentList;
        }

        public List<FSAppointmentScheduleBoard> UnassignedAppointmentRecords(DateTime timeBegin, DateTime timeEnd, int? branchID, int? branchLocationID, bool? unassignedAppointmentByRoom, DispatchBoardFilters[] filters)
        {
            List<object> args = new List<object>();
            List<FSAppointmentScheduleBoard> appointmentList = new List<FSAppointmentScheduleBoard>();
            var graph = new PXGraph();

            PXSelectBase<FSAppointmentScheduleBoard> appointmentRecords = new PXSelectJoin<FSAppointmentScheduleBoard,
                                                LeftJoin<FSAddress,
                                                    On<FSAddress.addressID, Equal<FSAppointmentScheduleBoard.addressID>>,
                                                LeftJoinSingleTable<Customer,
                                                    On<Customer.bAccountID, Equal<FSAppointmentScheduleBoard.customerID>>>>,
                                                                               Where<
                                                                                   FSAppointmentScheduleBoard.status, NotEqual<FSAppointmentScheduleBoard.status.Canceled>,
                                                                                   And<FSAppointmentScheduleBoard.status, NotEqual<FSAppointmentScheduleBoard.status.Completed>,
                                                                                   And<FSAppointmentScheduleBoard.status, NotEqual<FSAppointmentScheduleBoard.status.Closed>,
                                                   And2<Where<
                                                                                           Where2<
                                                                                               Where<FSAppointmentScheduleBoard.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeBegin>>,
                                                                                                   And<FSAppointmentScheduleBoard.scheduledDateTimeBegin, LessEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeEnd>>>>,
                                                                                           Or<
                                                                                               Where<FSAppointmentScheduleBoard.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeBegin>>,
                                                                   And<FSAppointmentScheduleBoard.scheduledDateTimeEnd, LessEqual<Required<FSAppointmentScheduleBoard.scheduledDateTimeEnd>>>>>>>,
                                                    And<
                                                        Where<Customer.bAccountID, IsNull,
                                                        Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>>(this);

            args.Add(timeBegin);
            args.Add(timeEnd);
            args.Add(timeBegin);
            args.Add(timeEnd);

            if (unassignedAppointmentByRoom == false)
            {
                appointmentRecords.Join<LeftJoin<FSAppointmentEmployee,
                                        On<
                                            FSAppointmentEmployee.appointmentID, Equal<FSAppointmentScheduleBoard.appointmentID>>>>();

                appointmentRecords.WhereAnd<Where<FSAppointmentEmployee.employeeID, IsNull>>();
            }
            else
            {
                appointmentRecords.Join<LeftJoin<FSRoom,
                                        On<
                                            FSRoom.roomID, Equal<FSAppointmentScheduleBoard.roomID>>>>();

                appointmentRecords.WhereAnd<Where<FSAppointmentScheduleBoard.roomID, IsNull>>();
            }

            if (branchID != null)
            {
                appointmentRecords.WhereAnd<Where<FSAppointmentScheduleBoard.branchID, Equal<Required<FSAppointmentScheduleBoard.branchID>>>>();
                args.Add(branchID);
            }

            if (branchLocationID != null)
            {
                appointmentRecords.WhereAnd<Where<FSAppointmentScheduleBoard.branchLocationID, Equal<Required<FSAppointmentScheduleBoard.branchLocationID>>>>();
                args.Add(branchLocationID);
            }

            if (filters != null)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i].property == TX.Dispatch_Board.LIKETEXT_FILTER)
                    {
                        appointmentRecords.WhereAnd<Where<
                                                        FSAppointmentScheduleBoard.refNbr, Like<Required<FSAppointmentScheduleBoard.memRefNbr>>,
                                                        Or<FSAppointmentScheduleBoard.customerName, Like<Required<FSAppointmentScheduleBoard.memAcctName>>>>>();

                        args.Add(string.Concat("%", filters[i].value[0], "%"));
                        args.Add(string.Concat("%", filters[i].value[0], "%"));
                    }
                }
            }

            var fsAppointmentSet = appointmentRecords.View.SelectMulti(args.ToArray());

            foreach (PXResult<FSAppointmentScheduleBoard> result in fsAppointmentSet)
            {
                FSAppointmentScheduleBoard fsAppointmentRow = (FSAppointmentScheduleBoard)result;

                List<int> listSkills = new List<int>();
                List<int> listFilter = new List<int>();
                bool removeAppointment = false;

                PXResultset<FSAppointmentDet> activeAppointmentServices = this.ActiveAppointmentDets.Select(fsAppointmentRow.AppointmentID);
                List<int?> serviceIDList = activeAppointmentServices.Select(y => y.GetItem<FSAppointmentDet>().InventoryID).ToList();
                List<SharedClasses.ItemList> serviceSkill = SharedFunctions.GetItemWithList<FSServiceSkill,
                                                                                            FSServiceSkill.serviceID,
                                                                                            FSServiceSkill.skillID>
                                                                                            (graph, serviceIDList);

                foreach (SharedClasses.ItemList item in serviceSkill)
                {
                    foreach (int serviceSkillID in item.list)
                    {
                        listSkills.Add(serviceSkillID);
                    }
                }

                if (filters != null)
                {
                    foreach (ExternalControls.DispatchBoardFilters filter in filters)
                    {
                        switch (filter.property)
                        {
                            case TX.Dispatch_Board.SKILL_FILTER:
                                listFilter = filter.value.Select(int.Parse).ToList();
                                if (listFilter.Except(listSkills).Any())
                                {
                                    removeAppointment = true;
                                    break;
                                }

                                break;
                            default:

                                break;
                        }
                    }
                }
                
                //If Appointment does not pass the filters it is removed from the List.
                if (removeAppointment == false)
                {
                    PXResultset<InventoryItem> bqlResultSet = this.AppointmentServices.Select(fsAppointmentRow.AppointmentID);
                    fsAppointmentRow.ServiceCount = bqlResultSet.Count;

                    appointmentList.Add(fsAppointmentRow);
                }
            }

            return appointmentList;
        }

        public int? DBPutAppointments(FSAppointmentScheduleBoard updatedAppointment, out bool isAppointment, out ExternalControls.DispatchBoardAppointmentMessages response)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            isAppointment = true;

            if (SharedFunctions.CheckAccessRights(ID.ScreenID.APPOINTMENT, typeof(AppointmentEntry), typeof(FSAppointment), PXCacheRights.Update) == false)
            {
                messages.ErrorMessages.Add(TX.Messages.ACCESS_RIGHTS_NOTIFICATION);
            }

            try
            {
                if (updatedAppointment != null)
                {
                    using (new PXScreenIDScope(ID.ScreenID.WEB_METHOD))
                    {
                        var graphAppointmentEntry = PXGraph.CreateInstance<AppointmentEntry>();

                        graphAppointmentEntry.AppointmentRecords.Current = graphAppointmentEntry.AppointmentRecords.Search<FSAppointment.refNbr>(updatedAppointment.RefNbr, updatedAppointment.SrvOrdType);
                        FSAppointment appointment = graphAppointmentEntry.AppointmentRecords.Current;

                        if (appointment != null)
                        {
                            if (appointment.ScheduledDateTimeBegin != updatedAppointment.ScheduledDateTimeBegin
                                    || appointment.ScheduledDateTimeEnd != updatedAppointment.ScheduledDateTimeEnd)
                            {
                                appointment.HandleManuallyScheduleTime = true;
                            }

                            appointment.Confirmed = updatedAppointment.Confirmed;
                            appointment.ValidatedByDispatcher = updatedAppointment.ValidatedByDispatcher;
                            appointment.ScheduledDateTimeBegin = updatedAppointment.ScheduledDateTimeBegin;
                            appointment.ScheduledDateTimeEnd = updatedAppointment.ScheduledDateTimeEnd;

                            graphAppointmentEntry.AppointmentRecords.Update(appointment);
                            WrkProcess.AssignAppointmentEmployee(updatedAppointment, this, graphAppointmentEntry, ref messages);

                            graphAppointmentEntry.SelectTimeStamp();

                            if (graphAppointmentEntry.PressSave(messages) == false)
                            {
                                response = messages;
                            }
                        }
                        else
                        {
                            messages.ErrorMessages.Add(GetErrorMessage(ErrorCode.APPOINTMENT_NOT_FOUND));
                            response = messages;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            response = messages;

            return -1;
        }

        public int? DBUnassignAppointmentBridge(FSAppointmentScheduleBoard fsAppointmentRow, out bool isAppointment, out ExternalControls.DispatchBoardAppointmentMessages response)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            FSWrkProcess fsWrkProcessRow = new FSWrkProcess()
            {
                RoomID = fsAppointmentRow.RoomID,
                SOID = fsAppointmentRow.SOID,
                AppointmentID = fsAppointmentRow.AppointmentID,
                SrvOrdType = fsAppointmentRow.SrvOrdType,
                TargetScreenID = ID.ScreenID.APPOINTMENT,
                SMEquipmentID = fsAppointmentRow.SMEquipmentID,
                EmployeeIDList = fsAppointmentRow.EmployeeID.ToString(),
                LineRefList = string.Empty,
                EquipmentIDList = string.Empty
            };

            int? entityID = null;
            try
            {

                var graphAppointmentMaint = PXGraph.CreateInstance<AppointmentEntry>();
                graphAppointmentMaint.AppointmentRecords.Current = graphAppointmentMaint.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);

                WrkProcess.AssignAppointmentEmployee(fsAppointmentRow, this, graphAppointmentMaint, ref messages);

                graphAppointmentMaint.Actions.PressSave();
                isAppointment = true;
            }
            catch
            {
                entityID = WrkProcess.SaveWrkProcessParameters(fsWrkProcessRow);
                isAppointment = false;
            }

            response = messages;

            return entityID;
        }
            

        public DispatchBoardAppointmentMessages DBDeleteAppointments(FSAppointment fsAppointmentRow)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            if (SharedFunctions.CheckAccessRights(ID.ScreenID.APPOINTMENT, typeof(AppointmentEntry), typeof(FSAppointment), PXCacheRights.Delete) == false)
            {
                messages.ErrorMessages.Add(TX.Messages.ACCESS_RIGHTS_NOTIFICATION);
                return messages;
            }

            try
            {
                var graphAppointmentMaint = PXGraph.CreateInstance<AppointmentEntry>();

                graphAppointmentMaint.AppointmentRecords.Current = graphAppointmentMaint.AppointmentRecords.Search<FSAppointment.refNbr>(fsAppointmentRow.RefNbr, fsAppointmentRow.SrvOrdType);
                fsAppointmentRow = graphAppointmentMaint.AppointmentRecords.Current;

                if (graphAppointmentMaint.PressDelete(messages) == false)
                {
                    return messages;
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages CheckApppointmentCreationByAccessRights()
        {
            DispatchBoardAppointmentMessages messages;
            messages = new DispatchBoardAppointmentMessages();

            if (SharedFunctions.CheckAccessRights(ID.ScreenID.APPOINTMENT, typeof(AppointmentEntry), typeof(FSAppointment), PXCacheRights.Insert) == false)
            {
                messages.ErrorMessages.Add(TX.Messages.ACCESS_RIGHTS_NOTIFICATION);
            }

            return messages;
        }

        public int? DBCreateAppointmentBridge(FSAppointmentScheduleBoard fsAppointmentRow, List<int> sODetIDList, List<int> employeeIDList, out bool isAppointment)
        {
            FSWrkProcess fsWrkProcessRow = new FSWrkProcess()
            {
                RoomID = fsAppointmentRow.RoomID,
                SOID = fsAppointmentRow.SOID,
                SrvOrdType = fsAppointmentRow.SrvOrdType,
                BranchID = fsAppointmentRow.BranchID == -1 ? null : fsAppointmentRow.BranchID,
                BranchLocationID = fsAppointmentRow.BranchLocationID,
                CustomerID = fsAppointmentRow.CustomerID,
                SMEquipmentID = fsAppointmentRow.SMEquipmentID,
                ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin,
                ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd,
                TargetScreenID = ID.ScreenID.APPOINTMENT,
                EmployeeIDList = string.Join(",", employeeIDList.ToArray()),
                LineRefList = string.Join(",", sODetIDList.ToArray()),
                EquipmentIDList = string.Empty
            };

            int? entityID = null;
            try
            {
                entityID = WrkProcess.LaunchAppointmentEntryScreen(this, fsWrkProcessRow, false);
                isAppointment = true;
            }
            catch
            {
                entityID = WrkProcess.SaveWrkProcessParameters(fsWrkProcessRow);
                isAppointment = false;
            }

            return entityID;
        }

        public FSAppointmentScheduleBoard GetCalendarAppointment(FSAppointmentScheduleBoard fsAppointmentScheduleBoardRow)
        {
            PXResultset<InventoryItem> inventoryItemRows = this.AppointmentServices.Select(fsAppointmentScheduleBoardRow.AppointmentID);
            if (inventoryItemRows != null && inventoryItemRows.Count > 0)
            {
                InventoryItem inventoryItemRow = inventoryItemRows;
                fsAppointmentScheduleBoardRow.FirstServiceDesc = inventoryItemRow.Descr;
            }

            FSRoom fsRoomRow = this.RoomByBranchLocation.Select(fsAppointmentScheduleBoardRow.BranchLocationID, fsAppointmentScheduleBoardRow.RoomID);
            if (fsRoomRow != null)
            {
                fsAppointmentScheduleBoardRow.RoomDesc = fsRoomRow.Descr;
            }

            var fsAppointmentEmployeeRows = (PXResultset<FSAppointmentEmployee>)this.AppointmentEmployees
                            .Select(fsAppointmentScheduleBoardRow.AppointmentID);

            fsAppointmentScheduleBoardRow.EmployeeCount = fsAppointmentEmployeeRows.Count;

            fsAppointmentScheduleBoardRow.CanDeleteAppointment = SharedFunctions.CheckAccessRights(ID.ScreenID.APPOINTMENT, typeof(AppointmentEntry), typeof(FSAppointment), PXCacheRights.Delete);
            return fsAppointmentScheduleBoardRow;
        }

        public FSAppointmentStaffScheduleBoard GetStaffCalendarAppointment(FSAppointmentStaffScheduleBoard fsAppointmentScheduleBoardRow)
        {
            fsAppointmentScheduleBoardRow = (FSAppointmentStaffScheduleBoard)GetCalendarAppointment(fsAppointmentScheduleBoardRow);
            fsAppointmentScheduleBoardRow.OldEmployeeID = fsAppointmentScheduleBoardRow.EmployeeID;

            fsAppointmentScheduleBoardRow.CustomDateID = fsAppointmentScheduleBoardRow.ScheduledDateTimeBegin.Value.Month.ToString("00") +
                                                                  fsAppointmentScheduleBoardRow.ScheduledDateTimeBegin.Value.Day.ToString("00") +
                                                                  fsAppointmentScheduleBoardRow.ScheduledDateTimeBegin.Value.Year.ToString("0000") +
                                                                  "-" +
                                                                  fsAppointmentScheduleBoardRow.EmployeeID;

            return fsAppointmentScheduleBoardRow;
        }

        public List<RouteNode> GetTreeAppointmentNodesByRoute(int routeDocumentID, FSRoute fsRouteRow)
        {
            var results = this.AppointmentRecordsByRoute.Select(routeDocumentID);
            List<RouteNode> tmpLeaf = new List<RouteNode>();

            if (fsRouteRow == null)
            {
                fsRouteRow = PXSelectJoin<FSRoute,
                             InnerJoin<FSRouteDocument,
                             On<
                                 FSRouteDocument.routeDocumentID, Equal<Required<FSRouteDocument.routeDocumentID>>>>,
                             Where<
                                 FSRoute.routeID, Equal<FSRouteDocument.routeID>>>
                             .Select(this, routeDocumentID);
            }

            FSAddress fsAddressRow_Begin = PXSelectJoin<FSAddress,
                                           InnerJoin<FSBranchLocation, 
                                           On<
                                               FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                           Where<
                                               FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                           .Select(this, fsRouteRow.BeginBranchLocationID);

            //Start Location Tree Node
            tmpLeaf.Add(new RouteNode(fsRouteRow, fsAddressRow_Begin, PXMessages.LocalizeFormatNoPrefix(TX.Route_Location.START_LOCATION), routeDocumentID.ToString()));

            foreach (PXResult<FSAppointment, FSServiceOrder, FSSrvOrdType, Location, FSAddress, Country, State, Customer> bqlResult in results)
            {
                FSAppointment fsAppointmentRow = bqlResult;
                FSServiceOrder fsServiceOrderRow = bqlResult;
                FSSrvOrdType fsSrvOrdTypeRow = bqlResult;
                Country countryRow = bqlResult;
                State stateRow = bqlResult;
                FSAddress fsAddressRow = bqlResult;
                Customer customerRow = bqlResult;
                Location locationRow = bqlResult;

                //Appointment Tree Node 
                tmpLeaf.Add(new RouteNode(fsAppointmentRow, fsSrvOrdTypeRow, fsServiceOrderRow, customerRow, locationRow, fsAddressRow));
            }

            FSAddress fsAddressRow_End = PXSelectJoin<FSAddress,
                                         InnerJoin<FSBranchLocation,
                                         On<
                                             FSBranchLocation.branchLocationAddressID, Equal<FSAddress.addressID>>>,
                                         Where<
                                             FSBranchLocation.branchLocationID, Equal<Required<FSBranchLocation.branchLocationID>>>>
                                         .Select(this, fsRouteRow.EndBranchLocationID);

            //End Location Tree Node 
            tmpLeaf.Add(new RouteNode(fsRouteRow, fsAddressRow_End, PXMessages.LocalizeFormatNoPrefix(TX.Route_Location.END_LOCATION), routeDocumentID.ToString()));

            return tmpLeaf;
        }

        public List<RouteNode> GetTreeAppointmentNodesByEmployee(int employeeID, DateTime calendarDate)
        {
            DateHandler requestDate = new DateHandler(calendarDate);
            DateTime timeBegin = requestDate.StartOfDay();
            DateTime timeEnd = requestDate.BeginOfNextDay();

            var results = this.AppointmentsByEmployee.Select(timeBegin, timeEnd, employeeID);
            List<RouteNode> tmpLeaf = new List<RouteNode>();

            foreach (PXResult<EPEmployee,
                              FSAppointmentEmployee,
                              FSAppointment,
                              FSServiceOrder,
                              FSSrvOrdType,
                              BAccountStaffMember,
                              Location,
                              FSAddress,
                              Country,
                              State,
                              Customer> bqlResult in results)
            {
                FSAppointment fsAppointmentRow = bqlResult;
                FSServiceOrder fsServiceOrderRow = bqlResult;
                FSSrvOrdType fsSrvOrdTypeRow = bqlResult;
                Country countryRow = bqlResult;
                State stateRow = bqlResult;
                FSAddress fsAddressRow = bqlResult;
                Customer customerRow = bqlResult;
                BAccountStaffMember bAccountStaffMemberRow = bqlResult;
                Location locationRow = bqlResult;

                //Appointment Tree Node 
                tmpLeaf.Add(new RouteNode(fsAppointmentRow, fsSrvOrdTypeRow, fsServiceOrderRow, customerRow, bAccountStaffMemberRow, locationRow, fsAddressRow));
            }

            return tmpLeaf;
        }

        // TODO AC-142850 Revisar para usa projection
        public List<object> GetUnassignedAppointmentNode(DateTime calendarDate, int branchID, int? branchLocationID)
        {
            DateHandler requestDate = new DateHandler(calendarDate);
            DateTime timeBegin = requestDate.StartOfDay();
            DateTime timeEnd = requestDate.BeginOfNextDay();
            List<object> args = new List<object>();

            BqlCommand appointmentRecords = new Select2<FSAppointment,
                                                InnerJoin<FSServiceOrder,
                                                    On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                                                InnerJoin<FSSrvOrdType,
                                                        On<FSSrvOrdType.srvOrdType, Equal<FSServiceOrder.srvOrdType>>,
                                                LeftJoin<Customer,
                                                        On<Customer.bAccountID, Equal<FSServiceOrder.customerID>>,
                                                LeftJoin<FSAppointmentEmployee,
                                                        On<FSAppointmentEmployee.appointmentID, Equal<FSAppointment.appointmentID>>,
                                                LeftJoin<Location,
                                                        On<Location.locationID, Equal<FSServiceOrder.locationID>>,
                                                InnerJoin<FSAddress,
                                                        On<FSAddress.addressID, Equal<FSServiceOrder.serviceOrderAddressID>>,
                                                LeftJoin<Country,
                                                        On<Country.countryID, Equal<FSAddress.countryID>>,
                                                LeftJoin<State,
                                                        On<State.countryID, Equal<FSAddress.countryID>,
                                                        And<State.stateID, Equal<FSAddress.state>>>>>>>>>>>,
                                                Where2<
                                                    Where<FSServiceOrder.branchID, Equal<Required<FSServiceOrder.branchID>>>,
                                                    And2<
                                                        Where<
                                                            Where2<
                                                                Where<FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                                And<FSAppointment.scheduledDateTimeBegin, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>,
                                                            Or<
                                                                Where<FSAppointment.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                                And<FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>>>>,
                                                    And<
                                                            FSAppointmentEmployee.employeeID, IsNull,
                                                        And<
                                                            Where<Customer.bAccountID, IsNull,
                                                            Or<Match<Customer, Current<AccessInfo.userName>>>>>>>>>();

            args.Add(branchID);
            args.Add(timeBegin);
            args.Add(timeEnd);
            args.Add(timeBegin);
            args.Add(timeEnd);

            if (branchLocationID != null)
            {
                appointmentRecords = appointmentRecords.WhereAnd(typeof(Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>));
                args.Add(branchLocationID);
            }

            PXView appointmentRecordsView = new PXView(this, true, appointmentRecords);
            var results = appointmentRecordsView.SelectMulti(args.ToArray());

            List<object> tmpLeaf = new List<object>();

            foreach (PXResult<FSAppointment, FSServiceOrder, FSSrvOrdType, Customer, FSAppointmentEmployee, Location, FSAddress, Country, State> bqlResult in results)
            {
                FSAppointment fsAppointmentRow = bqlResult;
                FSServiceOrder fsServiceOrderRow = bqlResult;
                FSSrvOrdType fsSrvOrdTypeRow = bqlResult;
                Country countryRow = bqlResult;
                State stateRow = bqlResult;
                FSAddress fsAddressRow = bqlResult;
                Customer customer = bqlResult;

                //Appointment Tree Node 
                tmpLeaf.Add(new
                {
                    NodeID = "Unassigned-" + fsAppointmentRow.RefNbr,
                    Text = fsAppointmentRow.RefNbr,
                    CustomerName = customer.AcctName,
                    Duration = fsAppointmentRow.EstimatedDurationTotal,
                    ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin,
                    ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd,
                    AutoDocDesc = fsAppointmentRow.AutoDocDesc,
                    Address = ExternalControlsHelper.GetLongAddressText(fsAddressRow),
                    CustomerLocation = ExternalControlsHelper.GetShortAddressText(fsAddressRow),
                    PostalCode = fsAddressRow.PostalCode,
                    SrvOrdType = fsSrvOrdTypeRow.SrvOrdType,
                    SrvOrdTypeDescr = fsSrvOrdTypeRow.Descr,
                    Leaf = true,
                    Checked = true,
                    Latitude = fsAppointmentRow.MapLatitude,
                    Longitude = fsAppointmentRow.MapLongitude
                });
            }

            return tmpLeaf;
        }

        public DispatchBoardAppointmentMessages DBPutRoutes(RouteNode[] routeNodes)
        {
            DispatchBoardAppointmentMessages messages;
            messages = new DispatchBoardAppointmentMessages();

            try
            {
                for (int i = 0; i < routeNodes.Length; i++)
                {
                    if (routeNodes[i].Leaf == true && routeNodes[i].AppointmentID != null && routeNodes[i].RouteDocumentID != routeNodes[i].OriginalRouteDocumentID)
                    {
                        FSRouteDocument fsRouteDocumentRow = RouteRecord.Select(routeNodes[i].RouteDocumentID);
                        RouteAppointmentAssignmentHelper.ReassignAppointmentToRoute(fsRouteDocumentRow, routeNodes[i].RefNbr, routeNodes[i].SrvOrdType);
                    }
                }
                    
                for (int i = 0; i < routeNodes.Length; i++)
                {
                    if (routeNodes[i].Leaf == true)
                    {
                        PXUpdate<
                            Set<FSAppointment.routePosition, Required<FSAppointment.routePosition>>,
                            FSAppointment,
                            Where<
                                FSAppointment.appointmentID, Equal<Required<FSAppointment.appointmentID>>
                                >
                            >.Update(
                                    this,
                                    routeNodes[i].RoutePosition,
                                    routeNodes[i].AppointmentID);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);

                return messages;
            }
        }
        #endregion

        #region Staff 

        public List<PXResult<EPEmployee>> EmployeeRecords(int? branchID, int? branchLocationID, bool? ignoreActiveFlag, bool? ignoreAvailabilityFlag, DateTime? scheduledStartDate, DateTime? scheduledEndDate, DispatchBoardFilters[] filters)
        {
            List<object> args = new List<object>();
            List<object> employeeScheduleArgs = new List<object>();
            List<object> appointmentArgs = new List<object>();

            var graph = new PXGraph();
            bool auxByScheduled = false;
            PXResultset<FSTimeSlot> bqlResultSet_EmployeeSchedule = null;
            PXResultset<FSAppointmentEmployee> bqlResultSet_FSAppointmentEmployee = null;

            PXSelectBase<EPEmployee> employeesBase = new PXSelectJoin<EPEmployee,
                                                         LeftJoin<GL.Branch,
                                                         On<
                                                             GL.Branch.bAccountID, Equal<EPEmployee.parentBAccountID>>>,
                                                         Where<
                                                             EPEmployee.parentBAccountID, IsNotNull,
                                                                And<EPEmployee.status, NotEqual<BAccount.status.inactive>>>,
                                                         OrderBy<
                                                             Asc<EPEmployee.acctName>>>
                                                         (graph);
            if (branchID != null)
            {
                employeesBase.WhereAnd<Where<PX.Objects.GL.Branch.branchID, Equal<Required<PX.Objects.GL.Branch.branchID>>>>();
                args.Add(branchID);
            }

            if (filters != null)
            {
                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i].property == TX.Dispatch_Board.DEFINED_SCHEDULER_FILTER)
                    {
                        auxByScheduled = true;
                        break;
                    }
                }
            }

            if (ignoreAvailabilityFlag != true)
            {
                if (auxByScheduled)
                {
                    PXSelectBase<FSTimeSlot> fsTimeSlotBase = new PXSelectGroupBy<FSTimeSlot,
                                                                  Where<
                                                                      FSTimeSlot.timeStart, GreaterEqual<Required<FSTimeSlot.timeStart>>,
                                                                      And<FSTimeSlot.timeEnd, LessEqual<Required<FSTimeSlot.timeEnd>>>>,
                                                                  Aggregate<Max<FSTimeSlot.employeeID, GroupBy<FSTimeSlot.employeeID>>>>
                                                                  (this);

                    employeeScheduleArgs.Add(scheduledStartDate);
                    employeeScheduleArgs.Add(scheduledEndDate);

                    if (branchID != null)
                    {
                        fsTimeSlotBase.WhereAnd<Where<FSTimeSlot.branchID, Equal<Required<FSTimeSlot.branchID>>>>();
                        employeeScheduleArgs.Add(branchID);
                    }

                    if (branchLocationID != null)
                    {
                        fsTimeSlotBase.WhereAnd<Where<FSTimeSlot.branchLocationID, Equal<Required<FSTimeSlot.branchLocationID>>>>();
                        employeeScheduleArgs.Add(branchLocationID);
                    }

                    bqlResultSet_EmployeeSchedule = fsTimeSlotBase.Select(employeeScheduleArgs.ToArray());
                }
                else
                {
                    PXSelectBase<FSTimeSlot> employeeScheduleBase = new PXSelectGroupBy<FSTimeSlot,
                                                                        Where<
                                                                            FSTimeSlot.timeEnd, GreaterEqual<Required<FSTimeSlot.timeEnd>>>,
                                                                        Aggregate<Max<FSTimeSlot.employeeID, GroupBy<FSTimeSlot.employeeID>>>>
                                                                        (this);

                    employeeScheduleArgs.Add(scheduledStartDate);

                    if (branchID != null)
                    {
                        employeeScheduleBase.WhereAnd<Where<FSTimeSlot.branchID, Equal<Required<FSTimeSlot.branchID>>>>();
                        employeeScheduleArgs.Add(branchID);
                    }

                    if (branchLocationID != null)
                    {
                        employeeScheduleBase.WhereAnd<Where<FSTimeSlot.branchLocationID, Equal<Required<FSTimeSlot.branchLocationID>>>>();
                        employeeScheduleArgs.Add(branchLocationID);
                    }

                    bqlResultSet_EmployeeSchedule = employeeScheduleBase.Select(employeeScheduleArgs.ToArray());
                }

                PXSelectBase<FSAppointmentEmployee> appointmentBase =
                                        new PXSelectJoinGroupBy<FSAppointmentEmployee,
                                            InnerJoin<FSAppointment,
                                                On<
                                                    FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                                            InnerJoin<FSServiceOrder,
                                                On<
                                                    FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>>,
                                                Where2<
                                                    Where<
                                                        FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>>,
                                                    And<
                                                        Where2<
                                                            Where<
                                                                FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                                And<
                                                                    FSAppointment.scheduledDateTimeBegin, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>,
                                                        Or<
                                                            Where<
                                                                FSAppointment.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                            And<
                                                                FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>>>>>,
                                                Aggregate<Max<FSAppointmentEmployee.employeeID, GroupBy<FSAppointmentEmployee.employeeID>>>>(this);

                appointmentArgs.Add(scheduledStartDate);
                appointmentArgs.Add(scheduledEndDate);
                appointmentArgs.Add(scheduledStartDate);
                appointmentArgs.Add(scheduledEndDate);

                if (branchID != null)
                {
                    appointmentBase.WhereAnd<Where<FSServiceOrder.branchID, Equal<Required<FSServiceOrder.branchID>>>>();
                    appointmentArgs.Add(branchID);
                }

                if (branchLocationID != null)
                {
                    appointmentBase.WhereAnd<Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>>();
                    appointmentArgs.Add(branchLocationID);
                }

                bqlResultSet_FSAppointmentEmployee = appointmentBase.Select(appointmentArgs.ToArray());
            }

            if (!ignoreActiveFlag.HasValue || !ignoreActiveFlag.Value)
            {
                employeesBase.WhereAnd<Where<FSxEPEmployee.sDEnabled, Equal<True>>>();
            }

            List<PXResult<EPEmployee>> bqlResultSet_EPEmployeesToFilter;

            if (filters != null)
            {
                bool auxDisplayName = false;
                bool auxEmployeeID = false;
                bool auxReportsTo = false;

                for (int i = 0; i < filters.Length; i++)
                {
                    if (filters[i].property == TX.Dispatch_Board.DISPLAY_NAME_FILTER)
                    {
                        employeesBase.WhereAnd<Where<EPEmployee.acctName, Like<Required<EPEmployee.acctName>>>>();
                        args.Add(string.Concat("%", filters[i].value[0], "%"));
                        auxDisplayName = true;
                        if (auxEmployeeID && auxReportsTo)
                        {
                            break;
                        }
                    }

                    if (filters[i].property == TX.Dispatch_Board.EMPLOYEE_ID_FILTER)
                    {
                        employeesBase.WhereAnd<Where<EPEmployee.bAccountID, Equal<Required<EPEmployee.bAccountID>>>>();
                        args.Add(filters[i].value[0]);
                        auxEmployeeID = true;
                        if (auxDisplayName && auxReportsTo)
                        {
                            break;
                        }
                    }

                    if (filters[i].property == TX.Dispatch_Board.REPORT_TO_EMPLOYEE_FILTER)
                    {
                        employeesBase.WhereAnd<Where<EPEmployee.supervisorID, Equal<Required<EPEmployee.supervisorID>>>>();
                        args.Add(filters[i].value[0]);
                        auxReportsTo = true;
                        if (auxEmployeeID && auxDisplayName)
                        {
                            break;
                        }
                    }
                }

                bqlResultSet_EPEmployeesToFilter = (args.Count > 0) ? employeesBase.Select(args.ToArray())?.ToList() : employeesBase.Select()?.ToList();

                //Remove duplicate employees
                int? lastEmployeeID = -1;
                for (int i = 0; i < bqlResultSet_EPEmployeesToFilter.Count; i++)
                {
                    EPEmployee employee = (EPEmployee)bqlResultSet_EPEmployeesToFilter[i];

                    if (employee.BAccountID == lastEmployeeID)
                    {
                        bqlResultSet_EPEmployeesToFilter.RemoveAt(i--);
                    }

                    lastEmployeeID = employee.BAccountID;
                }

                if (ignoreAvailabilityFlag != true)
                {
                    if (auxByScheduled)
                    {
                        bqlResultSet_EPEmployeesToFilter = FilterEmployeesByBranchLocationAndScheduled(bqlResultSet_EPEmployeesToFilter, bqlResultSet_EmployeeSchedule?.ToList(), bqlResultSet_FSAppointmentEmployee?.ToList());
                    }
                    else
                    {
                        bqlResultSet_EPEmployeesToFilter = FilterEmployeesByBranchLocation(bqlResultSet_EPEmployeesToFilter, bqlResultSet_EmployeeSchedule?.ToList(), bqlResultSet_FSAppointmentEmployee?.ToList());
                    }
                }

                List<int?> employeeList = bqlResultSet_EPEmployeesToFilter.Select(y => y.GetItem<EPEmployee>().BAccountID).ToList();

                List<SharedClasses.ItemList> employeeSkills = SharedFunctions.GetItemWithList<FSEmployeeSkill,
                                                                                              FSEmployeeSkill.employeeID,
                                                                                              FSEmployeeSkill.skillID>
                                                                                              (graph, employeeList);

                List<SharedClasses.ItemList> employeeLicenseTypes = SharedFunctions.GetItemWithList<FSLicense,
                                                                                                    FSLicense.employeeID,
                                                                                                    FSLicense.licenseTypeID,
                                                                                                    Where<FSLicense.expirationDate, GreaterEqual<Required<FSLicense.expirationDate>>,
                                                                                                          Or<FSLicense.expirationDate, IsNull>>>
                                                                                                    (graph, employeeList, scheduledStartDate);

                List<SharedClasses.ItemList> employeeGeoZones = SharedFunctions.GetItemWithList<FSGeoZoneEmp,
                                                                                                FSGeoZoneEmp.employeeID,
                                                                                                FSGeoZoneEmp.geoZoneID>
                                                                                                (graph, employeeList);

                for (int i = 0; i < bqlResultSet_EPEmployeesToFilter.Count; i++)
                {
                    EPEmployee employee = (EPEmployee)bqlResultSet_EPEmployeesToFilter[i];
                    List<object> listFilter = new List<object>();

                    SharedClasses.ItemList employeeSkillList = employeeSkills.FirstOrDefault(y => y.itemID == employee.BAccountID);
                    SharedClasses.ItemList employeeLicenseTypeList = employeeLicenseTypes.FirstOrDefault(y => y.itemID == employee.BAccountID);
                    SharedClasses.ItemList employeeGeoZoneList = employeeGeoZones.FirstOrDefault(y => y.itemID == employee.BAccountID);

                    bool removeEmployee = false;

                    //Iterate for each filter and try to filter in each List above.
                    foreach (DispatchBoardFilters filter in filters)
                    {
                        switch (filter.property)
                        {
                            case TX.Dispatch_Board.SKILL_FILTER:
                                listFilter = filter.value.Select(int.Parse).Cast<object>().ToList();
                                if (listFilter.Count > 0
                                        && (employeeSkillList == null
                                        || listFilter.Except(employeeSkillList.list).Any()))
                                {
                                    removeEmployee = true;
                                }

                                break;
                            case TX.Dispatch_Board.LICENSE_TYPE_FILTER:
                                listFilter = filter.value.Select(int.Parse).Cast<object>().ToList();
                                if (listFilter.Count > 0
                                        && (employeeLicenseTypeList == null
                                        || listFilter.Except(employeeLicenseTypeList.list).Any()))
                                {
                                    removeEmployee = true;
                                }

                                break;
                            case TX.Dispatch_Board.GEO_ZONE_FILTER:
                                listFilter = filter.value.Select(int.Parse).Cast<object>().ToList();
                                if (listFilter.Count > 0
                                        && (employeeGeoZoneList == null
                                        || listFilter.Except(employeeGeoZoneList.list).Any()))
                                {
                                    removeEmployee = true;
                                }

                                break;
                            case TX.Dispatch_Board.SERVICE_FILTER:

                                List<int?> serviceIDList = filter.value.Select(int.Parse).Cast<int?>().ToList();
                                List<SharedClasses.ItemList> serviceSkills = SharedFunctions.GetItemWithList<FSServiceSkill,
                                                                                                             FSServiceSkill.serviceID,
                                                                                                             FSServiceSkill.skillID>(graph, serviceIDList);

                                List<SharedClasses.ItemList> serviceLicenseTypes = SharedFunctions.GetItemWithList<FSServiceLicenseType,
                                                                                                                   FSServiceLicenseType.serviceID,
                                                                                                                   FSServiceLicenseType.licenseTypeID>(graph, serviceIDList);

                                foreach (string value in filter.value)
                                {
                                    int serviceID = int.Parse(value);

                                    SharedClasses.ItemList serviceSkillList = serviceSkills.FirstOrDefault(y => y.itemID == serviceID);
                                    SharedClasses.ItemList serviceLicenseTypeList = serviceLicenseTypes.FirstOrDefault(y => y.itemID == serviceID);

                                    /*Checking if Employee have all skills*/
                                    if (serviceSkillList != null
                                            && (employeeSkillList == null
                                                || serviceSkillList.list.Except(employeeSkillList.list).Any()))
                                    {
                                        removeEmployee = true;
                                        break;
                                    }

                                    /*Checking if Employee have all licenseTypes*/
                                    if (serviceLicenseTypeList != null
                                            && (employeeLicenseTypeList == null
                                                || serviceLicenseTypeList.list.Except(employeeLicenseTypeList.list).Any()))
                                    {
                                        removeEmployee = true;
                                        break;
                                    }
                                }

                                break;
                            default:

                                break;
                        }
                    }

                    //If ServiceOrder does not pass the filters it is removed from the List.
                    if (removeEmployee)
                    {
                        bqlResultSet_EPEmployeesToFilter.RemoveAt(i--);
                    }
                }
            }
            else
            {
                bqlResultSet_EPEmployeesToFilter = (args.Count > 0) ? employeesBase.Select(args.ToArray()).ToList() : employeesBase.Select().ToList();

                if (ignoreAvailabilityFlag != true)
                {
                    if (auxByScheduled)
                    {
                        bqlResultSet_EPEmployeesToFilter = FilterEmployeesByBranchLocationAndScheduled(bqlResultSet_EPEmployeesToFilter, bqlResultSet_EmployeeSchedule?.ToList(), bqlResultSet_FSAppointmentEmployee?.ToList());
                    }
                    else
                    {
                        bqlResultSet_EPEmployeesToFilter = FilterEmployeesByBranchLocation(bqlResultSet_EPEmployeesToFilter, bqlResultSet_EmployeeSchedule?.ToList(), bqlResultSet_FSAppointmentEmployee?.ToList());
                    }
                }
            }

            return bqlResultSet_EPEmployeesToFilter;
        }
  
        public List<PXResult<EPEmployee>> FilterEmployeesByBranchLocation(List<PXResult<EPEmployee>> bqlResultSet_EPEmployee, List<PXResult<FSTimeSlot>> bqlResultSet_FSTimeSlot, List<PXResult<FSAppointmentEmployee>> bqlResultSet_FSAppointmentEmployee)
        {
            bool employeeHasRules;
            bool employeeHasAppointment;

            if (bqlResultSet_EPEmployee != null)
            {
                for (int i = 0; i < bqlResultSet_EPEmployee.Count; i++)
                {
                    employeeHasRules = false;
                    employeeHasAppointment = false;
                    EPEmployee ePEmployeeRow = (EPEmployee)bqlResultSet_EPEmployee[i];

                    if (bqlResultSet_FSTimeSlot != null)
                    {
                        for (int j = 0; j < bqlResultSet_FSTimeSlot.Count; j++)
                        {
                            FSTimeSlot fsTimeSlotRow = bqlResultSet_FSTimeSlot[j];
                            if (ePEmployeeRow.BAccountID == fsTimeSlotRow.EmployeeID)
                            {
                                bqlResultSet_FSTimeSlot.RemoveAt(j);
                                employeeHasRules = true;
                                break;
                            }
                        }
                    }

                    if (bqlResultSet_FSAppointmentEmployee != null)
                    {
                        for (int j = 0; j < bqlResultSet_FSAppointmentEmployee.Count; j++)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = bqlResultSet_FSAppointmentEmployee[j];

                            if (ePEmployeeRow.BAccountID == fsAppointmentEmployeeRow.EmployeeID)
                            {
                                bqlResultSet_FSAppointmentEmployee.RemoveAt(j);
                                employeeHasAppointment = true;
                                break;
                            }
                        }
                    }

                    if (employeeHasRules == false && employeeHasAppointment == false)
                    {
                        bqlResultSet_EPEmployee.RemoveAt(i--);
                    }
                }
            }

            return bqlResultSet_EPEmployee;
        }

        public List<PXResult<EPEmployee>> FilterEmployeesByBranchLocationAndScheduled(List<PXResult<EPEmployee>> bqlResultSet_EPEmployee, List<PXResult<FSTimeSlot>> bqlResultSet_FSEmployeeSchedule, List<PXResult<FSAppointmentEmployee>> bqlResultSet_FSAppointmentEmployee)
        {
            bool employeeHasRules;
            bool employeeHasAppointment;

            if (bqlResultSet_EPEmployee != null)
            {
                for (int i = 0; i < bqlResultSet_EPEmployee.Count; i++)
                {
                    employeeHasRules = false;
                    employeeHasAppointment = false;
                    EPEmployee ePEmployeeRow = (EPEmployee)bqlResultSet_EPEmployee[i];

                    if (bqlResultSet_FSEmployeeSchedule != null)
                    {
                        for (int j = 0; j < bqlResultSet_FSEmployeeSchedule.Count; j++)
                        {
                            FSTimeSlot fsTimeSlotRow_Employee = bqlResultSet_FSEmployeeSchedule[j];
                            if (ePEmployeeRow.BAccountID == fsTimeSlotRow_Employee.EmployeeID)
                            {
                                bqlResultSet_FSEmployeeSchedule.RemoveAt(j);
                                employeeHasRules = true;
                                break;
                            }
                        }
                    }

                    if (bqlResultSet_FSAppointmentEmployee != null)
                    {
                        for (int j = 0; j < bqlResultSet_FSAppointmentEmployee.Count; j++)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = bqlResultSet_FSAppointmentEmployee[j];

                            if (ePEmployeeRow.BAccountID == fsAppointmentEmployeeRow.EmployeeID)
                            {
                                bqlResultSet_FSAppointmentEmployee.RemoveAt(j);
                                employeeHasRules = true;
                                break;
                            }
                        }
                    }

                    if (employeeHasRules == false && employeeHasAppointment == false)
                    {
                        bqlResultSet_EPEmployee.RemoveAt(i--);
                    }
                }
            }

            return bqlResultSet_EPEmployee;
        }

        public PXResultset<Vendor> VendorRecords(int? branchID, int? branchLocationID, bool? ignoreActiveFlag, bool? ignoreAvailabilityFlag, DateTime? scheduledStartDate, DateTime? scheduledEndDate, DispatchBoardFilters[] filters)
        {
            List<object> args = new List<object>();
            List<object> employeeScheduleArgs = new List<object>();
            List<object> appointmentArgs = new List<object>();
            PXResultset<FSAppointmentEmployee> bqlResultSet_FSAppointmentEmployee = null;

            var graph = new PXGraph();

            PXSelectBase<Vendor> vendorsBase = new PXSelect<Vendor,
                                                   Where<
                                                       FSxVendor.sDEnabled, Equal<True>,
                                                                And<Vendor.status, NotEqual<BAccount.status.inactive>>>,
                                                   OrderBy<
                                                       Asc<Vendor.acctName>>>
                                                   (graph);

            PXSelectBase<FSAppointmentEmployee> appointmentBase =
                                    new PXSelectJoinGroupBy<FSAppointmentEmployee,
                                        InnerJoin<FSAppointment,
                                            On<
                                                FSAppointment.appointmentID, Equal<FSAppointmentEmployee.appointmentID>>,
                                        InnerJoin<FSServiceOrder,
                                            On<
                                                FSServiceOrder.sOID, Equal<FSAppointment.sOID>>>>,
                                            Where2<
                                                Where<
                                                    FSServiceOrder.status, NotEqual<FSServiceOrder.status.Canceled>>,
                                                And<
                                                    Where2<
                                                        Where<
                                                            FSAppointment.scheduledDateTimeBegin, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                            And<
                                                                FSAppointment.scheduledDateTimeBegin, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>,
                                                    Or<
                                                        Where<
                                                            FSAppointment.scheduledDateTimeEnd, GreaterEqual<Required<FSAppointment.scheduledDateTimeBegin>>,
                                                        And<
                                                            FSAppointment.scheduledDateTimeEnd, LessEqual<Required<FSAppointment.scheduledDateTimeEnd>>>>>>>>,
                                            Aggregate<Max<FSAppointmentEmployee.employeeID, GroupBy<FSAppointmentEmployee.employeeID>>>>(this);

            appointmentArgs.Add(scheduledStartDate);
            appointmentArgs.Add(scheduledEndDate);
            appointmentArgs.Add(scheduledStartDate);
            appointmentArgs.Add(scheduledEndDate);

            if (branchID != null)
            {
                appointmentBase.WhereAnd<Where<FSServiceOrder.branchID, Equal<Required<FSServiceOrder.branchID>>>>();
                appointmentArgs.Add(branchID);
            }

            if (branchLocationID != null)
            {
                appointmentBase.WhereAnd<Where<FSServiceOrder.branchLocationID, Equal<Required<FSServiceOrder.branchLocationID>>>>();
                appointmentArgs.Add(branchLocationID);
            }

            bqlResultSet_FSAppointmentEmployee = appointmentBase.Select(appointmentArgs.ToArray());

            PXResultset<Vendor> vendorsToFilter;

            vendorsToFilter = (args.Count > 0) ? vendorsBase.Select(args.ToArray()) : vendorsBase.Select();

            if (ignoreAvailabilityFlag != true)
            {
                vendorsToFilter = FilterVendorByBranchLocation(vendorsToFilter, bqlResultSet_FSAppointmentEmployee);
            }

            return vendorsToFilter;
        }

        public PXResultset<Vendor> FilterVendorByBranchLocation(PXResultset<Vendor> bqlResultSet_vendorRows, PXResultset<FSAppointmentEmployee> bqlResultSet_fsAppointmentEmployeeRows)
        {
            bool vendorHasAppointment;
            if (bqlResultSet_vendorRows != null)
            {
                for (int i = 0; i < bqlResultSet_vendorRows.Count; i++)
                {
                    vendorHasAppointment = false;
                    Vendor vendorRow = (Vendor)bqlResultSet_vendorRows[i];

                    if (bqlResultSet_fsAppointmentEmployeeRows != null)
                    {
                        for (int j = 0; j < bqlResultSet_fsAppointmentEmployeeRows.Count; j++)
                        {
                            FSAppointmentEmployee fsAppointmentEmployeeRow = bqlResultSet_fsAppointmentEmployeeRows[j];
                            if (vendorRow.BAccountID == fsAppointmentEmployeeRow.EmployeeID)
                            {
                                bqlResultSet_fsAppointmentEmployeeRows.RemoveAt(j);
                                vendorHasAppointment = true;
                                break;
                            }
                        }
                    }

                    if (vendorHasAppointment == false)
                    {
                        bqlResultSet_vendorRows.RemoveAt(i--);
                    }
                }
            }

            return bqlResultSet_vendorRows;
        }

        #region Working Schedulers

        public List<FSTimeSlot> GetWorkingScheduleRecords(DateTime timeBegin, DateTime timeEnd, int? branchID, int? branchLocationID, bool compressSlot, int[] employeeIDList)
        {
            List<object> args = new List<object>();
            List<FSTimeSlot> workingScheduleList = new List<FSTimeSlot>();

            BqlCommand workingScheduleRecords = new Select2<FSTimeSlot,
                                                    LeftJoin<FSBranchLocation,
                                                        On<FSBranchLocation.branchLocationID, Equal<FSTimeSlot.branchLocationID>>>,
                                                    Where<
                                                        FSTimeSlot.timeStart, GreaterEqual<Required<FSTimeSlot.timeStart>>,
                                                        And<FSTimeSlot.timeEnd, Less<Required<FSTimeSlot.timeEnd>>,
                                                        And<FSTimeSlot.slotLevel, Equal<Required<FSTimeSlot.slotLevel>>>>>,
                                                    OrderBy<
                                                        Asc<FSAppointment.appointmentID>>>();
            args.Add(timeBegin);
            args.Add(timeEnd);
            args.Add(compressSlot ? 1 : 0);

            if (branchID != null)
            {
                workingScheduleRecords = workingScheduleRecords.WhereAnd(typeof(Where<FSTimeSlot.branchID, Equal<Required<FSTimeSlot.branchID>>,
                                                                                    Or<FSTimeSlot.branchID, IsNull>>));
                args.Add(branchID);
            }
            else
            {
                var branches = PXSelect<GL.Branch, Where<GL.Branch.active, Equal<True>>>.Select(new PXGraph()).RowCast<GL.Branch>().ToList();

                if (branches.Count() > 0 && compressSlot)
                {
                    workingScheduleRecords = workingScheduleRecords.WhereAnd(InHelper<FSTimeSlot.branchID>.Create(branches.Count()));

                    foreach (var branchRow in branches)
                    {
                        args.Add(branchRow.BranchID);
                    }
                }
            }

            if (branchLocationID != null)
            {
                workingScheduleRecords = workingScheduleRecords.WhereAnd(typeof(Where<FSTimeSlot.branchLocationID, Equal<Required<FSTimeSlot.branchLocationID>>,
                                                                                    Or<FSTimeSlot.branchLocationID, IsNull>>));
                args.Add(branchLocationID);
            }
            else
            {
                PXResultset<FSBranchLocation> bqlResultSet = this.BranchLocationRecordsByBranch.Select(branchID);

                if (bqlResultSet.Count > 0 && compressSlot)
                {
                    workingScheduleRecords = workingScheduleRecords.WhereAnd(InHelper<FSTimeSlot.branchLocationID>.Create(bqlResultSet.Count));
                    foreach (FSBranchLocation fsBranchLocationRow in bqlResultSet)
                    {
                        args.Add(fsBranchLocationRow.BranchLocationID);
                    }
                }
            }

            if (employeeIDList != null && employeeIDList.Length > 0)
            {
                workingScheduleRecords = workingScheduleRecords.WhereAnd(InHelper<FSTimeSlot.employeeID>.Create(employeeIDList.Length));
                foreach (int employeeID in employeeIDList)
                {
                    args.Add(employeeID);
                }
            }

            PXView workingScheduleRecordsView = new PXView(this, true, workingScheduleRecords);
            var fsTimeSlotSet = workingScheduleRecordsView.SelectMulti(args.ToArray());
            DateTime? minTimeBegin = null;
            DateTime? maxTimeEnd = null;
            int count = 0;

            foreach (PXResult<FSTimeSlot, FSBranchLocation> bqlResult in fsTimeSlotSet)
            {
                FSTimeSlot fsTimeSlotRow = bqlResult;
                FSBranchLocation fsBranchLocationRow = bqlResult;

                fsTimeSlotRow.CustomID = fsTimeSlotRow.TimeStart.Value.Month.ToString("00") +
                                         fsTimeSlotRow.TimeStart.Value.Day.ToString("00") +
                                         fsTimeSlotRow.TimeStart.Value.Year.ToString("0000");

                fsTimeSlotRow.CustomDateID = fsTimeSlotRow.CustomID + "-" + fsTimeSlotRow.EmployeeID;

                if (fsBranchLocationRow != null)
                {
                    fsTimeSlotRow.BranchLocationDesc = fsBranchLocationRow.Descr;
                    fsTimeSlotRow.BranchLocationCD = fsBranchLocationRow.BranchLocationCD;
                }

                fsTimeSlotRow.WrkEmployeeScheduleID = count.ToString();

                ++count;

                if (minTimeBegin == null || minTimeBegin > fsTimeSlotRow.TimeStart)
                {
                    minTimeBegin = fsTimeSlotRow.TimeStart;
                }

                if (maxTimeEnd == null || maxTimeEnd < fsTimeSlotRow.TimeEnd)
                {
                    maxTimeEnd = fsTimeSlotRow.TimeEnd;
                }

                workingScheduleList.Add(fsTimeSlotRow);
            }

            return workingScheduleList;
        }

        public DispatchBoardAppointmentMessages DBPutAvailability(FSTimeSlot fsTimeSlotRow)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                TimeSlotMaint timeSlotMaintGraph = PXGraph.CreateInstance<TimeSlotMaint>();
                FSTimeSlot fsTimeSlotRow_Aux = this.TimeSlotRecord.Select(fsTimeSlotRow.TimeSlotID);

                if (fsTimeSlotRow_Aux != null)
                {
                    fsTimeSlotRow_Aux.TimeStart = fsTimeSlotRow.TimeStart;
                    fsTimeSlotRow_Aux.TimeEnd = fsTimeSlotRow.TimeEnd;
                    TimeSpan duration = (DateTime)fsTimeSlotRow_Aux.TimeEnd - (DateTime)fsTimeSlotRow_Aux.TimeStart;
                    fsTimeSlotRow_Aux.TimeDiff = (decimal?)duration.TotalMinutes;
                    timeSlotMaintGraph.TimeSlotRecords.Update(fsTimeSlotRow_Aux);

                    if (timeSlotMaintGraph.PressSave(messages) == false)
                    {
                        return messages;
                    }
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public DispatchBoardAppointmentMessages DBDeleteAvailability(FSTimeSlot fsTimeSlotRow)
        {
            DispatchBoardAppointmentMessages messages = new DispatchBoardAppointmentMessages();

            try
            {
                TimeSlotMaint timeSlotMaintGraph = PXGraph.CreateInstance<TimeSlotMaint>();
                timeSlotMaintGraph.TimeSlotRecords.Current = timeSlotMaintGraph.TimeSlotRecords.Search<FSTimeSlot.timeSlotID>(fsTimeSlotRow.TimeSlotID);
                timeSlotMaintGraph.TimeSlotRecords.Delete(timeSlotMaintGraph.TimeSlotRecords.Current);

                if (timeSlotMaintGraph.PressSave(messages) == false)
                {
                    return messages;
                }
            }
            catch (Exception e)
            {
                messages.ErrorMessages.Add(e.Message);
            }

            return messages;
        }

        public int? DBCreateWrkSchedulerBridge(FSWrkEmployeeSchedule fsWrkEmployeeScheduleRow)
        {
            FSWrkProcess fsWrkProcessRow = new FSWrkProcess()
            {
                BranchID = fsWrkEmployeeScheduleRow.BranchID,
                BranchLocationID = fsWrkEmployeeScheduleRow.BranchLocationID,
                EmployeeIDList = fsWrkEmployeeScheduleRow.EmployeeID.ToString(),
                ScheduledDateTimeBegin = fsWrkEmployeeScheduleRow.TimeStart,
                ScheduledDateTimeEnd = fsWrkEmployeeScheduleRow.TimeEnd,
                TargetScreenID = ID.ScreenID.EMPLOYEE_SCHEDULE
            };
            return WrkProcess.SaveWrkProcessParameters(fsWrkProcessRow);
        }
        #endregion

        #endregion

        #region BusinessDate

        //Get the current Bussiness Date for the Calendars
        public string GetBusinessDate()
        {
            if (Accessinfo.BusinessDate != null && PXTimeZoneInfo.Now != null)
            { 
                return ((DateTime)PXDBDateAndTimeAttribute.CombineDateTime(Accessinfo.BusinessDate.Value, PXTimeZoneInfo.Now)).ToString("MM/dd/yyyy h:mm:ss tt", new CultureInfo("en-US"));
            }

            return string.Empty;
        }
        #endregion
        #endregion

        #region Filters and Messages

        public class DispatchBoardFilters
        {
            public string property { get; set; }

            public string[] value { get; set; }
        }

        public class DispatchBoardAppointmentMessages
        {
            public List<string> ErrorMessages;
            public List<string> WarningMessages;

            public DispatchBoardAppointmentMessages()
            {
                this.ErrorMessages = new List<string>();
                this.WarningMessages = new List<string>();
            }
        }

        #endregion

        #region Route Class
        [Serializable]
        public class RouteNode
        {
            public string RefNbr;
            public string NodeID;
            public string Text;
            public string CustomerName;
            public string CustomerLocation;

            public int? AppointmentID;
            public int? RouteDocumentID;
            public int? RouteID;
            public string RouteCD;
            public int? RoutePosition;

            public string LocationCD;
            public string LocationDesc;

            public DateTime? ScheduledDateTimeBegin;
            public DateTime? ScheduledDateTimeEnd;
            public string ActualStartTime;
            public string ActualEndTime;
            public int? Duration;
            public string SrvOrdType;
            public string SrvOrdTypeDescr;
            public int? ServicesDuration;

            public string Vehicle;
            public string Driver;
            public string DriverName;

            public string AutoDocDesc;
            public string Status;
            public string Description;
            public string Address;
            public string PostalCode;
            
            public decimal? Latitude;
            public decimal? Longitude;
            public string TrackingID;

            public int? OriginalRouteDocumentID;
            public int? OriginalRouteID;
            public int? OriginalRoutePosition;

            public bool? Leaf = true;
            public bool? Checked = true;
            public bool allowDrag = true;
            public bool allowDrop = true;
            public List<RouteNode> Rows;

            public RouteNode()
            {
            }

            /// <summary>
            /// Initializes a new instance of the RouteNode class for a Begin or End Route.
            /// </summary>
            /// <param name="fsRouteRow"> Route Record. </param>
            /// <param name="fsAddressRow"> ServiceOrder Address Record. </param>
            /// <param name="locationName"> Display Text Node. </param>
            public RouteNode(FSRoute fsRouteRow, FSAddress fsAddressRow, string locationName, string nodeID)
            {
                this.NodeID = locationName + " (" + nodeID + ")";
                this.Text = locationName + " ( " + fsRouteRow.RouteCD.Trim() + ")";
                this.ServicesDuration = 0;
                this.Address = ExternalControlsHelper.GetLongAddressText(fsAddressRow);
                this.CustomerLocation = ExternalControlsHelper.GetShortAddressText(fsAddressRow);
                this.PostalCode = fsAddressRow.PostalCode;
                this.allowDrag = false;
                this.allowDrop = false;
            }

            /// <summary>
            /// Initializes a new instance of the RouteNode class for an Appointment-Employee Tree Node.
            /// </summary>
            /// <param name="fsAppointmentRow"> Appointment Record. </param>
            /// <param name="fsSrvOrdTypeRow"> Service Order Type Record. </param>
            /// <param name="fsServiceOrderRow"> Service Order Record. </param>
            /// <param name="customerRow"> Customer Record. </param>
            /// <param name="bAccountStaffMemberRow"> Staff Member Record. </param>
            public RouteNode(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow, Customer customerRow, BAccountStaffMember bAccountStaffMemberRow, Location locationRow, FSAddress fsAddressRow)
            {
                this.NodeID = bAccountStaffMemberRow.BAccountID + "-" + fsAppointmentRow.AppointmentID;
                this.Text = bAccountStaffMemberRow.AcctCD.Trim() + "-" + fsAppointmentRow.RefNbr;
                this.SetCommonAttributes(fsAppointmentRow, fsSrvOrdTypeRow, fsServiceOrderRow, customerRow, locationRow, fsAddressRow);
            }

            /// <summary>
            /// Initializes a new instance of the RouteNode class for an Appointment Tree Node.
            /// </summary>
            /// <param name="fsAppointmentRow"> Appointment Record. </param>
            /// <param name="fsSrvOrdTypeRow"> Service Order Type Record. </param>
            /// <param name="fsServiceOrderRow"> Service Order Record. </param>
            /// <param name="customerRow"> Customer Record. </param>
            public RouteNode(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow, Customer customerRow, Location locationRow, FSAddress fsAddressRow)
            {
                this.NodeID = fsAppointmentRow.RefNbr;
                this.Text = fsAppointmentRow.RefNbr;
                this.SetCommonAttributes(fsAppointmentRow, fsSrvOrdTypeRow, fsServiceOrderRow, customerRow, locationRow, fsAddressRow);
            }

            /// <summary>
            /// Initializes a new instance of the RouteNode class for a Parent Tree Node.
            /// </summary>
            /// <param name="fsRouteRow"> Route Record. </param>
            /// <param name="fsRouteDocumentRow"> Route Document Record. </param>
            /// <param name="childNodes"> List of Route Nodes. </param>
            /// <param name="driver"> Driver Record. </param>
            /// <param name="vehicle"> Vehicle Record. </param>
            /// <param name="displayText"> Display Text Node. </param>
            public RouteNode(FSRoute fsRouteRow, FSRouteDocument fsRouteDocumentRow, List<RouteNode> childNodes, EPEmployee driver, FSEquipment vehicle, string displayText, PXResultset<FSGPSTrackingRequest> fsGPSTrackingRequestRows)
            {
                this.NodeID = fsRouteDocumentRow.RefNbr;
                this.Text = displayText;

                if(fsGPSTrackingRequestRows != null && fsGPSTrackingRequestRows.Count > 0) { 
                    this.TrackingID = ((FSGPSTrackingRequest)fsGPSTrackingRequestRows[0]).TrackingID.ToString();
                }

                this.RouteDocumentID = fsRouteDocumentRow.RouteDocumentID;
                this.RouteID = fsRouteDocumentRow.RouteID;
                this.RouteCD = fsRouteRow.RouteCD;
                this.CustomerName = string.Empty;
                this.DriverName = (driver != null) ? driver.AcctName : string.Empty;
                this.Duration = fsRouteDocumentRow.TotalDuration;
                this.ServicesDuration = fsRouteDocumentRow.TotalServicesDuration;
                this.Leaf = childNodes.Count == 0;
                this.Rows = childNodes;
                this.Vehicle = (vehicle != null) ? vehicle.RefNbr : string.Empty;
                this.Driver = (driver != null) ? driver.AcctName : string.Empty;
                this.Description = fsRouteRow.Descr;
                this.Status = fsRouteDocumentRow.Status;
                this.Checked = true;
            }

            private void SetCommonAttributes(FSAppointment fsAppointmentRow, FSSrvOrdType fsSrvOrdTypeRow, FSServiceOrder fsServiceOrderRow, Customer customerRow, Location locationRow, FSAddress fsAddressRow)
            {
                this.RefNbr = fsAppointmentRow.RefNbr;
                this.CustomerName = customerRow.AcctName;
                this.ScheduledDateTimeBegin = fsAppointmentRow.ScheduledDateTimeBegin;
                this.AppointmentID = fsAppointmentRow.AppointmentID;
                this.ScheduledDateTimeEnd = fsAppointmentRow.ScheduledDateTimeEnd;
                this.ServicesDuration = fsAppointmentRow.EstimatedDurationTotal;
                this.AutoDocDesc = fsAppointmentRow.AutoDocDesc;
                this.LocationDesc = locationRow.Descr;
                this.LocationCD = locationRow.LocationCD;
                this.Address = ExternalControlsHelper.GetLongAddressText(fsAddressRow);
                this.CustomerLocation = ExternalControlsHelper.GetShortAddressText(fsAddressRow);
                this.PostalCode = fsAddressRow.PostalCode;
                this.SrvOrdType = fsSrvOrdTypeRow.SrvOrdType;
                this.SrvOrdTypeDescr = fsSrvOrdTypeRow.Descr;
                this.RoutePosition = fsAppointmentRow.RoutePosition;
                this.RouteID = fsAppointmentRow.RouteID;
                this.RouteDocumentID = fsAppointmentRow.RouteDocumentID;
                this.OriginalRoutePosition = fsAppointmentRow.RoutePosition;
                this.OriginalRouteID = fsAppointmentRow.RouteID;
                this.OriginalRouteDocumentID = fsAppointmentRow.RouteDocumentID;
                this.Leaf = true;
                this.Checked = true;
                this.Latitude = fsAppointmentRow.MapLatitude;
                this.Longitude = fsAppointmentRow.MapLongitude;
                this.ActualStartTime = fsAppointmentRow.ActualDateTimeBegin?.ToString("MM/dd/yyyy h:mm:ss tt");
                this.ActualEndTime = fsAppointmentRow.ActualDateTimeEnd?.ToString("MM/dd/yyyy h:mm:ss tt");
            }
        }

        #endregion

        #region ErrorMessages

        /// <summary> 
        /// Gets the error message for a given error code.
        /// </summary>
        /// <returns>String with the error message and the error code.</returns>
        public static string GetErrorMessage(ErrorCode code)
        {
            string message = (int)code + ":";
            switch (code)
            {
                case ErrorCode.APPOINTMENT_SHARED:
                    message = message + TX.Error.APPOINTMENT_SHARED;
                    break;
                case ErrorCode.APPOINTMENT_NOT_FOUND:
                    message = message + TX.Error.APPOINTMENT_NOT_FOUND;
                    break;
                default:
                    message = TX.Error.TECHNICAL_ERROR;
                    break;
            }

            return message;
        }

        public enum ErrorCode
        {
            APPOINTMENT_SHARED = 0,
            APPOINTMENT_NOT_FOUND = 1
        }

        #endregion
    }

    public static class ExternalControlsHelper
    {
        public static bool PressSave(this PXGraph graph, ExternalControls.DispatchBoardAppointmentMessages messages)
        {
            bool saveSucceeded = true;
            bool exceptionFlag = false;
            string exceptionMessage = null;

            try
            {
                graph.GetSaveAction().Press();
            }
            catch (Exception e)
            {
                exceptionFlag = true;
                exceptionMessage = e.Message;

                if (graph.GetType() == typeof(AppointmentEntry))
                {
                    if (exceptionMessage.IndexOf(ID.AcumaticaErrorNumber.SAVE_BUTTON_DISABLED) > 0)
                    {
                        exceptionMessage = TX.Error.APPOINTMENT_NOT_EDITABLE;
                    }
                }
            }

            foreach (PXCache cache in graph.Caches.Caches)
            {
                if (GetRowMessages(cache, cache.Current, messages) > 0)
                {
                    saveSucceeded = false;
                }

                foreach (object row in cache.Deleted)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            saveSucceeded = false;
                        }
                    }
                }

                foreach (object row in cache.Inserted)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            saveSucceeded = false;
                        }
                    }
                }

                foreach (object row in cache.Updated)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            saveSucceeded = false;
                        }
                    }
                }
            }

            if (saveSucceeded == true && exceptionFlag == true)
            {
                messages.ErrorMessages.Add(exceptionMessage);
                saveSucceeded = false;
            }

            return saveSucceeded;
        }

        public static bool PressDelete(this PXGraph graph, ExternalControls.DispatchBoardAppointmentMessages messages)
        {
            bool deleteSucceeded = true;
            bool exceptionFlag = false;
            string exceptionMessage = null;

            try
            {
                graph.GetDeleteAction().Press();
            }
            catch (Exception e)
            {
                exceptionFlag = true;
                exceptionMessage = e.Message;
            }

            foreach (PXCache cache in graph.Caches.Caches)
            {
                if (GetRowMessages(cache, cache.Current, messages) > 0)
                {
                    deleteSucceeded = false;
                }

                foreach (object row in cache.Deleted)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            deleteSucceeded = false;
                        }
                    }
                }

                foreach (object row in cache.Inserted)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            deleteSucceeded = false;
                        }
                    }
                }

                foreach (object row in cache.Updated)
                {
                    if (row != cache.Current)
                    {
                        if (GetRowMessages(cache, row, messages) > 0)
                        {
                            deleteSucceeded = false;
                        }
                    }
                }
            }

            if (deleteSucceeded == true && exceptionFlag == true)
            {
                messages.ErrorMessages.Add(exceptionMessage);
                deleteSucceeded = false;
            }

            return deleteSucceeded;
        }

        private static int GetRowMessages(PXCache cache, object row, ExternalControls.DispatchBoardAppointmentMessages messages)
        {
            return MessageHelper.GetRowMessages(cache, row, messages.ErrorMessages, messages.WarningMessages, true);
        }

        public static string GetLongAddressText(FSAddress row)
        {
            string addressText;

            addressText = row.AddressLine1?.Trim();

            if (!string.IsNullOrEmpty(row.AddressLine2))
            {
                if (!string.IsNullOrEmpty(addressText))
                {
                    addressText += " ";
                }
                addressText += row.AddressLine2.Trim();
            }

            addressText += ", "
                    + row.State + ", "
                    + row.City + " "
                    + row.PostalCode + ", "
                    + row.CountryID;

            return addressText;
        }

        public static string GetShortAddressText(FSAddress row)
        {
            string addressText;

            addressText = row.AddressLine1?.Trim();

            if (!string.IsNullOrEmpty(row.AddressLine2))
            {
                if (!string.IsNullOrEmpty(addressText)){
                    addressText += " ";
                }
                addressText += row.AddressLine2.Trim();
            }

            return addressText;
        }
    }
}
