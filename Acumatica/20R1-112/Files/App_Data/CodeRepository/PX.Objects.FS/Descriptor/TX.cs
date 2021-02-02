using PX.Common;

namespace PX.Objects.FS
{
    public static class TX
    {
        //FSEquipment - LocationType
        [PXLocalizable]
        public static class LocationType
        {
            public const string COMPANY  = "Company";
            public const string CUSTOMER = "Customer";
        }

        //FSEquipment - Condition
        [PXLocalizable]
        public static class Condition
        {
            public const string NEW  = "New";
            public const string USED = "Used";
        }

        //FSLicense - OwnerType
        [PXLocalizable]
        public static class OwnerType
        {
            public const string BUSINESS = "Business";
            public const string EMPLOYEE = "Staff Member";
        }

        //FSxService - BillingRule
        [PXLocalizable]
        public static class BillingRule
        {
            public const string TIME      = "Time";
            public const string FLAT_RATE = "Flat Rate";
            public const string NONE      = "None";
        }

        //FSEmployeeSchedule - ScheduleType
        [PXLocalizable]
        public static class ScheduleType
        {
            public const string AVAILABILITY   = "Availability";
            public const string UNAVAILABILITY = "Unavailability";
            public const string BUSY           = "Busy because of an Appointment";
        }

        [PXLocalizable]
        public class Schedule_EntityType
        {
            public const string CONTRACT = "Contract";
            public const string EMPLOYEE = "Employee";
        }

        [PXLocalizable]
        public class PostDoc_EntityType
        {
            public const string APPOINTMENT     = "Appointment";
            public const string SERVICE_ORDER   = "Service Order";
            public const string CONTRACT        = "Service Contract";
        }

        [PXLocalizable]
        public class ContractType_BillingFrequency
        {
            public const string NONE            = "None";
            public const string TIME_OF_SERVICE = "Time of Service";
            public const string BEG_OF_CONTRACT = "Beg. of Contract";
            public const string END_OF_CONTRACT = "End of Contract";
            public const string DAYS_30_60_90   = "30/60/90 Days";
            public const string MONTHLY         = "Monthly";
            public const string EVERY_4TH_MONTH = "Every 4th Month";
            public const string SEMI_ANNUAL     = "Semi Yearly";
            public const string ANNUAL          = "Yearly";
        }

        [PXLocalizable]
        public static class ContractPeriod_Actions
        {
            public const string SEARCH_BILLING_PERIOD           = "Search by Billing Periods";
            public const string MODIFY_UPCOMING_BILLING_PERIOD  = "Modify Upcoming Billing Period";
        }

        [PXLocalizable]
        public class Schedule_FrequencyType
        {
            public const string DAILY   = "Daily";
            public const string WEEKLY  = "Weekly";
            public const string MONTHLY = "Monthly";
            public const string ANNUAL  = "Yearly";
        }

        [PXLocalizable]
        public class Contract_BillingType
        {
            public const string AS_PERFORMED_BILLINGS = "As Performed Billings";
            public const string STANDARDIZED_BILLINGS = "Standardized Plus Usage/Overage Billings";
        }

        [PXLocalizable]
        public class Contract_BillTo
        {
            public const string CUSTOMERACCT = "Customer Account";
            public const string SPECIFICACCT = "Specific Account";
        }

        [PXLocalizable]
        public class Contract_ExpirationType
        {
            public const string EXPIRING  = "Expiring";
            public const string UNLIMITED = "Unlimited";
        }

        [PXLocalizable]
        public class Contract_BillingPeriod
        {
            public const string ONETIME  = "One-Time";
            public const string WEEK     = "Week";
            public const string MONTH    = "Month";
            public const string QUARTER  = "Quarter";
            public const string HALFYEAR = "Half a Year";
            public const string YEAR     = "Year";
        }

        [PXLocalizable]
        public class Contract_PostingPrefixPeriod
        {
            public const string CONTRACT_COVERAGE = "Contract Coverage: ";
            public const string CONTRACT_OVERAGE  = "Contract Overage: ";
            public const string CONTRACT_USAGE    = "Contract Usage: ";
        }

        //Cache Names
        [PXLocalizable]
        public static class TableName
        {
            public const string ZIP_CODE                        = "Zip Code";
            public const string CAUSE                           = "Cause";
            public const string SKILL                           = "Skill";
            public const string RESOLUTION                      = "Resolution";
            public const string EQUIPMENT                       = "Equipment";            
            public const string LICENSE                         = "License";
            public const string LICENSE_TYPE                    = "License Type";
            public const string PROBLEM                         = "Problem";
            public const string SERVICE_CODE                    = "Service";
            public const string SERVICE_CODE_PROBLEM            = "Service - Problem";
            public const string FAMILY                          = "Family";
            public const string FAMILY_SERVICE                  = "Family Service";
            public const string SERVICE_LICENSE_TYPE            = "Service - License Type";
            public const string SERVICE_STATUS                  = "Service Status";
            public const string REASON                          = "Reason";
            public const string GEOGRAPHIC_ZONE                 = "Service Area";
            public const string GEOGRAPHIC_ZONE_EMPLOYEE        = "Service Area - Employee";
            public const string GEOGRAPHIC_POSTAL_CODE          = "Service Area - Postal Code";
            public const string EMPLOYEE_SCHEDULE               = "Employee Schedule";
            public const string APPOINTMENT                     = "Appointment";
            public const string SERVICE_ORDER                   = "Service Order";
            public const string BILL_OF_MATERIALS               = "Bill of Materials";
            public const string BILL_OF_MATERIALS_DETAIL        = "Bill of Materials Detail";
            public const string SODET                           = "Service Order Line";
            public const string SODET_SERVICE                   = "Service Order Service";
            public const string SODET_PART                      = "Service Order Part";
            public const string APPOINTMENTDET                  = "Appointment Line";
            public const string APPOINTMENTDET_SERVICE          = "Appointment Service";
            public const string APPOINTMENTDET_PART             = "Appointment Part";
            public const string APPOINTMENTDET_PICKUPDELIVERY   = "Appointment Pickup/Delivery Line";
            public const string ORDER_STAGE                     = "Order Stage";
            public const string ORDER_TYPE                      = "Order Type";
            public const string EQUIPMENT_TYPE                  = "Equipment Type";
            public const string BRANCH_LOCATION                 = "Branch Location";
            public const string REPORT_CLASS                    = "Report Class";
            public const string ROOM                            = "Room";
            public const string REFERRAL_SRC_TYPE               = "Referral Source Type";
            public const string CUSTOMER_PRODUCT                = "Customer Product";
            public const string ROUTE_DOCUMENT                  = "Route Document";
            public const string SERVICE_CONTRACT                = "Service Contract";
            public const string ROUTES_SETUP                    = "Route Management Preferences";
            public const string ROUTE                           = "Route";
            public const string ROUTE_SHIFT                     = "Route Shift";
            public const string VEHICLE_TYPE                    = "Vehicle Type";
            public const string VEHICLE_TYPE_LICENSE            = "Vehicle Type License";
            public const string SHIFT                           = "Shift";
            public const string MANUFACTURER                    = "Manufacturer";
            public const string MANUFACTURER_MODEL              = "Manufacturer Model";
            public const string STOCK_ITEM_WARRANTY             = "Stock Item Warranty";
            public const string MODEL_WARRANTY                  = "Model Warranty";
            public const string CONTRACT_GENERATION_HISTORY     = "Contract Generation History";
            public const string SERVICEMANAGEMENT_SETUP         = TX.ModuleName.SERVICE_DISPATCH + " Preferences";
            public const string EQUIPMENT_SETUP                 = "Equipment Management Preferences";
            public const string WEEKCODE_DATE                   = "Contracts/Routes calendar Week Code";
            public const string GENERATION_LOG_ERROR            = "Generation Log Error";
            public const string MODEL_TEMPLATE_COMPONENT        = "Model Template Component";
            public const string BILLING_CYCLE                   = "Billing Cycle";
            public const string FSCONTACT                       = "Field Service Contact";
            public const string FSADDRESS                       = "Field Service Address";
            public const string FSShippingContact               = "Field Service Shipping Contact";
            public const string FSShippingAddress               = "Field Service Shipping Address";
            public const string FSAppointmentTax                = "Appointment Tax";
            public const string FSAppointmentTaxTran            = "Appointment Tax Detail";
            public const string FSServiceOrderTax               = "Service Order Tax";
            public const string FSServiceOrderTaxTran           = "Service Order Tax Detail";
            public const string FSLOG                           = "Log";
        }

        //Message Parameters
        [PXLocalizable]
        public static class MessageParm
        {
            //@TODO:
            // Move all other constants of message parameters to this class
            // But first try to create complete messages instead messages that require parameter constants
            public const string ADDRESS = "Address";
            public const string CONTACT = "Contact";
        }

        //Error messages
        [PXLocalizable]
        public static class Error
        {
            public const string ID_ALREADY_USED                                                                = "This ID is already in use.";
            public const string FIELD_MAY_NOT_BE_EMPTY                                                         = "\"{0}\" cannot be empty.";
            public const string STAFF_MEMBERS_COUNT_EXCEEDS_LICENSE_LIMIT                                      = "The number of staff members exceeds the license limit: existing staff members = {0}, license limit = {1}.";
            public const string VEHICLES_COUNT_EXCEEDS_LICENSE_LIMIT                                           = "The number of vehicles exceeds the license limit: existing vehicles = {0}, license limit = {1}.";
            public const string FIELD_EMPTY                                                                    = "This element cannot be empty.";
            public const string ZERO_OR_NEGATIVE_QTY                                                           = "The quantity must be greater than 0.";
            public const string LINE_HAS_INVALID_DATA                                                          = "Line has invalid data.";
            public const string FIELD_MUST_BE_EMPTY_FOR_LINE_TYPE                                              = "The column must be empty for the selected line type.";
            public const string DATA_REQUIRED_FOR_LINE_TYPE                                                    = "Data is required for the selected line type.";
            public const string ID_ALREADY_USED_SAME_LEVEL                                                     = "This ID is already in use for the current level.";
            public const string ID_ALREADY_USED_PARENT                                                         = "This ID is already in use for the parent level.";
            public const string NOT_EMPLOYEE_SELECTED                                                          = "Please select an Employee.";
            public const string CURRENT_DOCUMENT_NOT_SERVICES_TO_SCHEDULE                                      = "The current document does not have services to schedule.";
            public const string NEGATIVE_QTY                                                                   = "This value cannot be negative.";
            public const string POSITIVE_QTY                                                                   = "The quantity must be greater than 0.";
            public const string ISSUE_EXPIRATION_DATE_INCONSISTENCY                                            = "The issue date must be earlier than the expiration date.";
            public const string NULL_OR_ZERO_HOURS                                                             = "Hours should be greater than 0.";
            public const string END_DATE_LESSER_THAN_START_DATE                                                = "The dates are invalid. The end date cannot be earlier than the start date.";
            public const string END_TIME_LESSER_THAN_START_TIME                                                = "The times are invalid. The end time cannot be earlier than the start time.";
            public const string START_TIME_GREATER_THAN_END_TIME                                               = "The start time cannot be later than the end time. Correct the values.";
            public const string SLAETA_GREATER_EQUAL_PROMISEDDATE                                              = "SLA Date must be greater or equal than promised date.";
            public const string CUSTOMER_CHANGE_NOT_ALLOWED_APP_STATUS                                         = "The Customer cannot be changed because the Service Order already has ongoing Appointments.";
            public const string CUSTOMER_CHANGE_NOT_ALLOWED_SO_STATUS                                          = "The Customer cannot be changed because the Service Order status is already different from Open or Hold.";
            public const string ACTUAL_DATES_APPOINTMENT_MISSING                                               = "The Appointment cannot be completed. Please fill out the actual Date and Time fields.";
            public const string SERVICE_LICENSE_TYPES_REQUIREMENTS_MISSING                                     = "The Employees in this Appointment do not have the Licenses that the Service requires.";
            public const string SERVICE_SKILL_REQUIREMENTS_MISSING                                             = "The Employee does not have the Skills required to complete this Appointment Service(s).";
            public const string SERVICE_SKILL_REQUIREMENTS_MISSING_GENERAL                                     = "The Employees in this Appointment do not have the Skills that the Service requires.";
            public const string APPOINTMENT_EMPLOYEE_MISMATCH_GEOZONE                                          = "Employee not assigned to work on this service area. The postal code for this Appointment is not included in the service area where this employee could work.";
            public const string EMPLOYEES_MISSING_TO_VALIDATE_SERVICES                                         = "There are no Employees in order to validate the Services in this Appointment. Please add one or more Employees in the Employees tab.";
            public const string EMPLOYEES_MISSING_TO_VALIDATE_GEOZONE                                          = "There are no Employees in order to validate the Service Order Service Area. Please add one or more Employees in the Employees tab.";
            public const string APPOINTMENT_START_VALIDATE_SERVICE                                             = "An Appointment without Services cannot be started. At least one Service must be added in the Details tab.";
            public const string APPOINTMENT_COMPLETE_VALIDATE_SERVICE                                          = "An Appointment without Services cannot be completed. At least one Service must be added in the Details tab.";
            public const string SELECT_VALID_ACTION                                                            = "A valid action has to be selected.";
            public const string SIGNED_OFF_SERVICE                                                             = "This Service Order has been already signed off.";
            public const string CHECKOUT_NEEDED_BEFORE_SIGNOFF                                                 = "The Service Order must be checked out before signing it off.";
            public const string CHECKED_OUT_SERVICE                                                            = "This Service Order has been already checked out.";
            public const string SIGNED_OFF_NEEDED_FOR_SERVICE                                                  = "The Service Order must be signed off in order to create its invoice.";
            public const string CHECKOUT_AND_SIGNED_OFF_NEEDED_FOR_SERVICE                                     = "The Service Order must be checked out and signed off in order to create its invoice.";
            public const string SERVICE_ORDER_ALREADY_POSTED                                                   = "This Service Order has been already posted to Sales Order.";
            public const string SERVICE_ORDER_POSTING_INCONSISTENCY                                            = "Part of the detail lines in this Service Order were already posted, but the corresponding records in the control table are missing or corrupted. Please contact M5 technical support.";
            public const string SERVICE_ORDER_SOORDER_INCONSISTENCY                                            = "This Service Order has an invalid Sales Order reference. Please contact M5 technical support.";
            public const string SERVICE_ORDER_BILLCUSTOMER_MISSING                                             = "The Billing Customer has not been defined for the Service Order Number {0}. Please go to the Service Orders screen and assign it.";
            public const string SERVICE_ORDER_NOT_FOUND_IN_SERVICEORDERGRAPH                                   = "Error trying to update the ServiceOrder: the ServiceOrderGraph was not loaded.";
            public const string RESOURCES_MISSING_TO_VALIDATE_SERVICES                                         = "There are some services in the detail tab that require one or more missing resources. Check the detail tab to see the specific requirements for each service.";
            public const string QUOTE_SELECTION_NOT_ALLOWED                                                    = "The Quote functionality cannot be activated as there are already appointments scheduled for this Service Order.";
            public const string MINIMUN_VALUE                                                                  = "The minimum value allowed for this field is {0}";
            public const string MINIMUN_VALUE_NAME_FIELD                                                       = "The minimum value allowed for {0} is {1}";
            public const string APPOINTMENT_NOT_EDITABLE                                                       = "The appointment cannot be modified because either its service order has been closed or canceled. Click the number in the Appointment box to view the details on the Appointments (FS300200) form. ";
            public const string APPOINTMENT_NOT_DELETABLE                                                      = "The appointment cannot be deleted because it has either the status or the workflow stage that prohibits deleting. To view the details, in the appointment box, click the appointment number.";
            public const string INVALID_ACTION_FOR_CURRENT_SERVICE_ORDER_STATUS                                = "This action is invalid for the current service order status.";
            public const string SERVICE_ORDER_NOT_FOUND                                                        = "The service order was not found.";
            public const string SERVICE_ORDER_TYPE_X_NOT_FOUND                                                 = "The {0} service order type does not exist. Select another service order type or create a new one on the Service Order Types (FS202300) form.";
            public const string SERVICE_ORDER_CANNOT_BE_DELETED_BECAUSE_OF_ITS_STATUS                          = "The service order with the current status cannot be deleted.";
            public const string CUSTOMER_CANNOT_BE_CHANGED_BECAUSE_THERE_ARE_SERVICE_LINES_IN_THIS_APPOINTMENT = "The customer ID cannot be changed because the appointment contains service lines.";
            public const string CUSTOMER_CANNOT_BE_CHANGED_BECAUSE_THERE_ARE_SERVICE_LINES_IN_THE_SERVICEORDER = "The customer ID cannot be changed because the service order contains service lines.";
            public const string EQUIPMENT_ID_ALREADY_USED                                                      = "This ID is already in use by existing equipment.";
            public const string CONTRACT_INCORRECT_EXECUTION_LIMIT                                             = "The execution limit has to be greater than 0.";
            public const string CONTRACT_INCORRECT_DAILY_FREQUENCY                                             = "For a schedule type with the Daily recurrence, the frequency has to be greater than 0.";
            public const string CONTRACT_INCORRECT_WEEKLY_FREQUENCY                                            = "For a schedule type with the Weekly recurrence, the frequency has to be greater than 0.";
            public const string CONTRACT_UNDEFINED_EXPIRATION_DATE                                             = "Select the date in the Expiration Date box.";
            public const string CONTRACT_UNDEFINED_WEEK_DAY                                                    = "At least 1 day of the week has to be selected.";
            public const string CONTRACT_SCHEDULE_TYPE_CANT_BE_CHANGED                                         = "The schedule type for this contract cannot be changed because an upcoming execution has already been scheduled.";
            public const string EMPLOYEE_NOT_AVAILABLE_WITH_APPOINTMENTS                                       = "This employee has at least one appointment for the given date and time.";
            public const string ROOM_NOT_AVAILABLE_WITH_APPOINTMENTS                                           = "This room has at least one appointment for the given date and time.";
            public const string ROOM_REQUIRED_FOR_THIS_SRVORDTYPE                                              = "A room ID has to be specified for the selected service order type.";
            public const string WARRANTY_DURATION_ZERO_OR_NULL                                                 = "The warranty duration must be greater than 0.";
            public const string WARRANTY_DURATION_TYPE_NULL                                                    = "A duration type of the warranty has to be selected.";
            public const string WARRANTY_DFLT_VENDOR_NULL                                                      = "A vendor has to be selected";
            public const string WRKPROCESS_NOT_FOUND                                                           = "The FSWrkProcess record cannot be found.";
            public const string MAPS_MISSING_REQUIRED_PARAMETERS                                               = "At least one origin and one destination have to be included.";
            public const string MAPS_STATUS_CODE_OK                                                            = "The request was successful.";
            public const string MAPS_STATUS_CODE_CREATED                                                       = "A new resource has been created.";
            public const string MAPS_STATUS_CODE_ACCEPTED                                                      = "The request has been accepted for processing.";
            public const string MAPS_STATUS_CODE_BAD_REQUEST                                                   = "The request contains an error.";
            public const string MAPS_STATUS_CODE_UNAUTHORIZED                                                  = "Access was denied. You may have entered your credentials incorrectly, or you might not have access to the requested resource or operation.";
            public const string MAPS_STATUS_CODE_FORBIDDEN                                                     = "The request is for something forbidden. Authorization will not help.";
            public const string MAPS_STATUS_CODE_NOT_FOUND                                                     = "The requested resource was not found.";
            public const string MAPS_STATUS_CODE_TOO_MANY_REQUESTS                                             = "The user has sent too many requests in a given amount of time. The user account is rate-limited.";
            public const string MAPS_STATUS_CODE_INTERNAL_SERVER_ERROR                                         = "Your request could not be completed because there was a problem with the service.";
            public const string MAPS_STATUS_CODE_SERVICE_UNAVAILABLE                                           = "There is a problem with the service right now. Please try again later.";
            public const string MAPS_FAILED_REVERSE_GEOCODE                                                    = "The address could not be found.";
            public const string MAPS_FAILED_REVERSE_ADRESS                                                     = "{0} address cannot be interpreted as Latitude/Logitude.";
            public const string MAPS_CONNECTION_FAILED                                                         = "The system failed to connect to Bing API. Check your connection.";
            public const string CUSTOM_APPOINTMENT_FIELD_NOT_FOUND                                             = "The custom appointment field was not found.";
            public const string CUSTOM_APPOINTMENT_STATUS_NOT_FOUND                                            = "The custom appointment status was not found.";
            public const string VALIDATE_APPOINTMENT_ADDRESS_BEFORE_SAVING                                     = "You have to validate the appointment address on the General Info tab to save the changes you have made.";
            public const string SERVICE_ORDER_IS_NOT_OPEN                                                      = "This appointment cannot be cloned because the related service order has a status other than Open.";
            public const string SELECT_AT_LEAST_ONE_MONTH                                                      = "You must select at least one month.";
            public const string VEHICLE_NOT_MATCHING_APPOINTMENT_BRANCHLOCATION                                = "This vehicle cannot be assigned because its branch location does not match the branch location of any appointment in the route. Select another vehicle.";
            public const string VEHICLE_NOT_FOUND_IN_CURRENT_BRANCHLOCATION                                    = "This vehicle was not found in the current branch location.";
            public const string ROLLBACK_ROUTE_CONTRACT_GENERATION_FAILED                                      = "The last generation process cannot be rolled back because at least one appointment has a status other than " + TX.Status_Appointment.AUTOMATIC_SCHEDULED + ".";
            public const string ROLLBACK_SERVICE_CONTRACT_GENERATION_FAILED                                    = "The last generation process cannot be rolled back because at least one service order has an appointment.";
            public const string CUSTOMER_CONTACT_ADDRESS_OPTION_NOT_AVAILABLE                                  = "The appointment address cannot be copied from the customer contact because the Require Contact check box is not selected.";
            public const string INVALID_ACTUAL_APPOINTMENT_DURATION                                            = "The actual duration of the appointment cannot be zero. The start time must differ from the end time.";
            public const string INVALID_SCHEDULED_APPOINTMENT_DURATION                                         = "The scheduled duration of the appointment cannot be zero. The start time must differ from the end time.";
            public const string LINE_SERVICE_WAS_APPROVED_IN_ANOTHER_TIMECARD                                  = "This service line was already approved in the time card {0}. Remove the line from this document.";
            public const string INVALID_SO_STATUS_TRANSITION                                                   = "The transition of the service order status is invalid.";
            public const string INVALID_APPOINTMENT_STATUS_TRANSITION                                          = "The transition of the appointment status is invalid.";
            public const string PROJECT_IS_NOT_ACTIVE                                                          = "The project is not active. Remove it from this document.";
            public const string EXECUTION_DATE_MUST_BE_GREATHER_OR_EQUAL_THAN_SCHEDULED_DATE                   = "The actual date must be later than or equal to the scheduled date.";
            public const string CANNOT_CLOSED_APPOINTMENT_BECAUSE_TIME_REGISTERED_OR_ACTUAL_TIMES              = "At least one staff time has not been approved.";
            public const string CANNOT_UPDATE_APPOINTMENT_BECAUSE_STATUS_IS_CLOSED_OR_CANCELLED                = "The appointment cannot be updated because it has a status of Closed or Canceled.";
            public const string CANNOT_UPDATE_DATE_BECAUSE_ITS_SET_IN_AN_APPOINTMENT                           = "The time card date cannot be changed because the date is already specified in the appointment related.";
            public const string SALES_SUB_MASK_UNDEFINED_IN_SERVICE_ORDER_TYPE                                 = "The sales subaccount mask is not defined for the {0} service order type in the Combine Sales Sub. From box on the Service Order Types (FS202300) form.";
            public const string SOME_SUBACCOUNT_SEGMENT_SOURCE_IS_NOT_SPECIFIED                                = "Some subaccount segment source is not specified in the Combine Sales Sub. From box on the Service Order Types (FS202300) form.";
            public const string INVALID_APPOINTMENT_DURATION                                                   = "The appointment actual duration cannot be zero. The start time must differ from the end time.";
            public const string LINE_SERVICE_WAS_APPROVED_FROM_ANOTHER_TIMECARD                                = "This service line was already approved in the time card {0}. Remove the line from this document.";
            public const string CANNOT_DELETE_SALES_ORDER_BECAUSE_IT_HAS_A_SERVICE_ORDER                       = "The sales order cannot be deleted because a service order is linked to it. Delete the associated service order first.";
            public const string CANNOT_CLOSE_APPOINTMENT_BECAUSE_TIME_IS_NOT_APPROVED                          = "This appointment cannot be closed. The service time of at least one service line has not been approved. Approve the service time on the Employee Time Card (EP305000) form, or clear the Require Time Approval to Close Appointments check box in the Time & Expense Integration section located of the Service Order Types (FS202300) form.";
            public const string SRV_CLASS_REQUIRED_TO_CONVERT_NON_STOCKITEM_TO_SERVICE                         = "To convert this non-stock item to a service, assign an item class of the Service type in the Item Class box.";
            public const string SERVICE_ORDER_HAS_APPOINTMENTS                                                 = "The service order has at least one related appointment. Delete the appointments first.";
            public const string FSSODET_LINKED_TO_APPOINTMENTS                                                 = "This {0} is linked to at least one appointment. Delete the {0}s in the appointments before you delete the {0}.";
            public const string SERVICE_LINKED_TO_PICKUP_DELIVERY_ITEMS                                        = "This service is related to at least one picked up or delivered item. Delete picked up or delivered items before you delete the service.";
            public const string SPECIFY_LICENSE_NUMBERINGID_IN_X                                               = "The license numbering sequence has not been specified. Specify it in the License Numbering Sequence box on the {0} form.";
            public const string SCHEDULED_DATE_UNAVAILABLE                                                     = "No scheduled time is available.";
            public const string PROJECT_MUST_BELONG_TO_CUSTOMER                                                = "The project you select must belong to this customer.";
            public const string ROUTE_EMPLOYEE_PRIORITY_PREFERERENCE_GREATER_THAN_ZERO                         = "The priority preference must be greater than zero.";
            public const string START_DATE_LESSER_THAN_DEFAULT_DATE                                            = "The dates are invalid. The start date cannot be earlier than the default date.";
            public const string INVALID_WEEKCODE_GENERATION_OPTIONS                                            = "Inserting the generation options raised one or more errors. Please Review.";
            public const string ROUTE_MAX_APPOINTMENT_QTY_GREATER_THAN_ZERO                                    = "An entry must be greater than zero.";
            public const string WEEKCODE_MUST_NOT_BE_EMPTY                                                     = "Week codes cannot be empty.";
            public const string WEEKCODE_LENGTH_MUST_LESS_OR_EQUAL_THAN_4                                      = "The length of each week code must be less than or equal to 4.";
            public const string WEEKCODE_CHAR_NOT_ALLOWED                                                      = "The {0} character is not allowed to build valid week codes.";
            public const string WEEKCODE_BAD_FORMED                                                            = "At least one week code is not correctly structured. Check field label for examples.";
            public const string ROUTE_SHORT_NOT_DUPLICATED                                                     = "The route short ID cannot be duplicated.";
            public const string INVALID_ROUTE_EXECUTION_DAYS_FOR_APPOINTMENT                                   = "The appointment cannot be created. The scheduled day of the week for the appointment does not correspond to the execution days defined for the route: {0}. Review the recurrence of this schedule or modify the execution days of the {0} route on the Routes (FS203700) form.";
            public const string ROUTE_MAX_APPOINTMENT_QTY_EXCEEDED                                             = "The appointment cannot be created. The maximum number of appointments has been exceeded for the route.";
            public const string WEEKCODE_NOT_MATCH_WITH_SCHEDULE                                               = "The appointment cannot be created on {1}. The {0} schedule does not contain a week code related to this date.";
            public const string WEEKCODE_NOT_MATCH_WITH_ROUTE_SCHEDULE                                         = "The appointment cannot be created on {1}. The {0} schedule of the default route does not contain a week code related to this date.";
            public const string WEEKCODE_NOT_MATCH_WITH_ROUTE                                                  = "The appointment cannot be created on {1}. The {0} schedule of the route for the appointment does not contain a week code related to this date.";
            public const string ROUTE_DOCUMENT_DATE_NOT_MATCH_WITH_WEEKCODE                                    = "This date does not correspond to the {0} week code set for the {1} route.";
            public const string ROUTE_DOCUMENT_DATE_NOT_MATCH_WITH_EXECUTION_DAYS                              = "{0} is not set as an execution day for the {1} route on the Routes (FS203700) form.";
            public const string DRIVER_DELETION_ERROR                                                          = "This driver cannot be deleted on this form. To delete the driver, open the Route Document Details (FS304000) form, select the {0} route execution in the Route Nbr. box, and clear the Driver box.";
            public const string SCREENID_INCORRECT_FORMAT                                                      = "The provided screen ID does not follow the standard format.";
            public const string CUSTOMER_SIGNATURE_MISSING                                                     = "The appointment cannot be completed. Make sure that the appointment has been signed and saved and the signature has been uploaded to the appointment.";
            public const string CUSTOMER_FULLNAME_MISSING                                                      = "The appointment cannot be completed. Enter the full name on the Signature tab to complete the appointment.";
            public const string CUSTOMER_AGREEMENT_MISSING                                                     = "The appointment cannot be completed. Accept the agreement on the Signature tab to complete the appointment.";
            public const string CUSTOMER_SIGNATURE_INFO_INCOMPLETE                                             = "The appointment cannot be completed. You have to provide the required information on the Signature tab.";
            public const string QUOTE_HAS_SERVICE_ORDERS                                                       = "The quote is linked to at least one service order. Delete the service orders displayed on the Related Service Orders tab before you delete this quote.";
            public const string REQUIRED_CONTACT_MISSING                                                       = "A contact is required. Select another service order type for this schedule, or clear the Require Contact check box on the Service Order Types (FS202300) form for the service order type of the schedule.";
            public const string SO_TYPE_ROUTE_NOT_EXIST                                                        = "A new appointment cannot be created because a service order type of the Route behavior does not exist in the system. Create it on the Service Order Types (FS202300) form.";
            public const string INVALID_ROUTE_STATUS_TRANSITION                                                = "The transition of the route status is invalid.";
            public const string ROUTE_NEED_APPOINTMENTS_TO_CHANGE_STATUS                                       = "A route execution without appointments cannot be {0}. At least one appointment has to be added to the route execution.";
            public const string ROUTE_DOCUMENT_APPOINTMENTS_NOT_POSTED                                         = "The route execution cannot be closed because at least one appointment has not been posted to the Inventory module.";
            public const string ACTUAL_DATES_APPOINTMENT_MISSING_TO_POST                                       = "The appointment cannot be posted to the Inventory module. Fill in the boxes in the Actual Date and Time section of the Appointments (FS300200) form.";
            public const string POST_ORDER_TYPE_MISSING_IN_SETUP                                               = "The sales order cannot be generated because the sales order type has not been specified on the Service Order Types (FS202300) form.";
            public const string POST_ORDER_NEGATIVE_BALANCE_TYPE_MISSING_IN_SETUP                              = "The sales order cannot be generated because the sales order type for negative balances has not been specified on the Service Order Types (FS202300) form.";
            public const string NOTHING_TO_BE_POSTED                                                           = "There are no lines to be posted in the Appointment selection.";
            public const string BILLING_CYCLE_TYPE_NOT_VALID                                                   = "The billing cycle type is not valid.";
            public const string BACCOUNT_TYPE_DOES_NOT_MATCH_WITH_STAFF_MEMBERS_OPTIONS                        = "The business account is not of the Employee or Vendor type.";
            public const string STAFF_MEMBER_INCONSISTENCY                                                     = "The staff member selected has an invalid reference. Please contact support service for assistance.";
            public const string INVALID_METHOD_ARGUMENTS                                                       = "Technical error: Invalid arguments have been supplied.";
            public const string APPOINTMENT_SHARED                                                             = "This employee has already been assigned to this appointment.";
            public const string APPOINTMENT_NOT_FOUND                                                          = "The appointment you select cannot be found. Refresh the appointment and try again.";
            public const string TECHNICAL_ERROR                                                                = "Technical Error: Please retry the requested action.";
            public const string SERVICE_ORDER_SELECTED_IS_NULL                                                 = "Technical Error: ServiceOrderRelated.Current is NULL.";
            public const string RECORD_X_NOT_FOUND                                                             = "The {0} record was not found.";
            public const string APPOINTMENT_ITEM_CANNOT_BE_POSTED_TO_IN_NO_ITEMS_RELATED                       = "The record cannot be processed because No Items Related is selected in the Pickup/Deliver Items box on the Non-Stock Items (IN202000) form for the service.";
            public const string ROUTE_CANT_BE_COMPLETED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES                      = "The route execution cannot be completed. Some appointments have issues. See details below.";
            public const string ROUTE_CANT_BE_CANCELED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES                       = "The route execution cannot be canceled because some appointments have issues. See the Appointments tab.";
            public const string CANT_DEFINE_BILLING_CYCLE_BILLED_BY_SERVICE_ORDER_AND_GROUPED_BY_APPOINTMENT   = "The record cannot be saved. A billing cycle billed by service orders cannot be grouped by appointments. Select another invoice grouping option.";
            public const string ROUTE_CANT_BE_CLOSED_APPOINTMENTS_IN_ROUTE_HAVE_ISSUES                         = "The route execution cannot be closed because some appointments have issues. See details below.";
            public const string SERVICE_ORDER_CANT_BE_CLOSED_APPOINTMENTS_HAVE_ISSUES                          = "The service order cannot be closed because some appointments have issues. See details below.";
            public const string SERVICE_ORDER_CANT_BE_COMPLETED_APPOINTMENTS_HAVE_ISSUES                       = "The service order cannot be completed because some appointments have issues. See details below.";
            public const string SERVICE_ORDER_CANT_BE_CANCELED_APPOINTMENTS_HAVE_ISSUES                        = "The service order cannot be canceled because some appointments have issues. See details below.";
            public const string SERVICE_ORDER_CANT_BE_CANCELED_APPOINTMENTS_HAVE_INVALID_STATUS                = "The service order cannot be canceled because some appointments have invalid statuses. To cancel a service order, its appointments must have one of the following statuses: {0}, {1} or {2}.";
            public const string SRV_ORD_TYPE_ERROR_DELETING_SO_USING_IT                                        = "The service order type cannot be deleted because it is assigned to at least one service order.";
            public const string SRV_ORD_TYPE_ERROR_DELETING_CONTRACT_USING_IT                                  = "The service order type cannot be deleted because it is assigned to at least one service contract.";
            public const string SELECT_AT_LEAST_ONE_DAY_OF_WEEK                                                = "You must select at least one day of the week.";
            public const string BILLING_CYCLE_ERROR_DELETING_CUSTOMER_USING_IT                                 = "The billing cycle cannot be deleted because it is assigned to at least one customer.";
            public const string BILLING_CYCLE_ERROR_DELETING_VENDOR_USING_IT                                   = "The billing cycle cannot be deleted because it is assigned to at least one vendor.";
            public const string ADDITIONAL_DRIVER_EQUAL_MAIN_DRIVER                                            = "The driver and additional driver cannot be the same employee.";
            public const string VEHICLES_CANNOT_BE_EQUAL                                                       = "The vehicles cannot be repeated.";
            public const string INVENTORY_ITEM_UOM_INCONSISTENCY                                               = "There is an inconsistency in the UOM defined on the Sales Prices (AR202000) form and on the Non-Stock Items (IN202000) form for the {0} service.";
            public const string MAX_NBR_TRIPS_PER_DAY                                                          = "The route has reached the maximum number of trips on {0}.";
            public const string ID_OF_TRIPS_ALREADY_EXIST                                                      = "The trip number already exists. Define another number.";
            public const string CANNOT_DELETE_ROUTE_APP_SO_STATUS                                              = "This record cannot be deleted because at least one appointment or service order of the Completed or Closed status is related to the route execution.";
            public const string INVALID_WARRANTY_DURATION                                                      = "A warranty duration cannot be less than zero.";
            public const string EQUIPMENT_SOURCE_REFERENCE_DELETED                                             = "The source of this record has been deleted.";
            public const string EQUIPMENT_NUMBERING_SEQUENCE_MISSING_IN_X                                      = "The equipment numbering sequence has not been specified. Specify it in the Equipment Numbering Sequence box on the {0} form.";
            public const string SOLINE_HAS_RELATED_APPOINTMENT_DETAILS                                         = "This line cannot be modified because it is linked to at least one appointment.";
            public const string ERROR_DELETING_RELATED_SERVICE_ORDER                                           = "The related service order {0} cannot be deleted.";
            public const string SOME_SOLINES_HAVE_RELATED_APPOINTMENT_DETAILS                                  = "Some document details of the sales order are related to at least one appointment.";
            public const string RECURRENCE_DAYS_ROUTE_DAYS_MISMATCH                                            = "The specified recurrence dates do not match the dates specified for the route on the Routes (FS203700) form.";
            public const string FSSODET_LINE_IS_RELATED_TO_A_SOLINE                                            = "This line cannot be deleted because it is related to the {0} line of the sales order from which this service order has been created. Delete the line in the source sales order on the Sales Orders (SO301000) form.";
            public const string CANNOT_CHANGE_SEND_INVOICE_TYPE                                                = "The Service Order Address option for Send Invoice To is available only for billing cycles for which the Service Orders or Appointments option button is selected under Group Invoices By on the Billing Cycles (FS200600) form.";
            public const string ADDRESS_CONTACT_CANNOT_BE_NULL                                                 = "{0} must be specified to process this item.";
            public const string TIMECARDS_ACTUAL_TIMES_NOT_SPECIFIED                                           = "Define the actual times of this line in the Actual Start Time and Actual End Time boxes on the Appointments (FS300200) form.";
            public const string SELECT_AT_LEAST_ONE_OPTION                                                     = "You must select at least one option.";
            public const string START_TIME_GREATER_HEADER_ACTUAL_END_TIME                                      = "The {0}'s actual start time is greater than the appointment's actual end time. Please, correct that value.";
            public const string LOG_DATE_TIMES_ARE_REQUIRED_FOR_SERVICES                                       = "At least one service does not have a log. Make sure that all the necessary logs have been added on the Logs tab. If logs are not required for all services, clear the Require Service Logs on Appointment Completion check box on the Service Order Types (FS202300) form for the service order type.";
            public const string NO_AVAILABLE_BRANCH_LOCATION_IN_CURRENT_BRANCH                                 = "The service order cannot be created. There are no available branch locations in the current branch. Select another branch or create a branch location for the current branch on the Branch Locations (FS202500) form.";
            public const string DEFAULT_SERVICE_ORDER_TYPE_NOT_DEFINED                                         = "The default service order type has not been defined. Specify the default type in the Default Service Order Type box on the " + TableName.SERVICEMANAGEMENT_SETUP + " (FS100100) form.";
            public const string POSTING_MODULE_IS_INVALID                                                      = "The posting module '{0}' is invalid.";
            public const string UPDATING_FSSODET_PO_REFERENCES                                                 = "An error occurred during the update of the service order items related to this purchase order.";
            public const string MISSING_VENDOR_OR_LOCATION                                                     = "A vendor and vendor location must be defined.";
            public const string MISSING_LINK_ENTITY_STAFF_MEMBER                                               = "The time activities cannot be updated because at least one staff member is not associated with a user in the Linked Entity box on the Users (SM201010) form.";
            public const string INVALID_POSTING_TARGET                                                         = "The posting target is invalid.";
            public const string CANNOT_UPDATE_DOCUMENT_BECAUSE_BATCH_STATUS_IS_TEMPORARY                       = "The document cannot be updated because the batch has the Temporary status.";
            public const string DISTRIBUTION_MODULE_IS_DISABLED                                                = "The Distribution module is disabled.";
            public const string ADVANCED_SO_INVOICE_IS_DISABLED                                                = "The SO invoice cannot be generated because the Advanced SO Invoices feature is disabled on the Enable/Disable Features (CS100000) form. Please contact your system administrator.";
            public const string ROUTE_CANT_BE_COMPLETED_APPOINTMENTS_NEED_TO_BE_COMPLETED                      = "The route cannot be completed because some of its appointments have to be completed, canceled, or closed.";
            public const string MISSING_CUSTOMER_BILLING_ADDRESS_SOURCE                                        = "No billing address source has been specified for the customer. Specify the billing address source in the Bill-To box on the Billing Settings tab of the Customers (AR303000) form.";
            public const string DOCUMENT_MODULE_DIFERENT_T0_BATCH_MODULE                                       = "The module specified in the document {0} differs from the one specified in the batch {1}.";
            public const string INVENTORY_NOT_ALLOWED_AS_COMPONENT                                             = "The selected inventory ID should be the same as the class ID for this component. Select another one.";
            public const string EQUIPMENT_ACTION_MODEL_EQUIPMENT_REQUIRED                                      = "A model equipment identifier must be selected in the Inventory ID column for this action.";
            public const string EQUIPMENT_ACTION_COMPONENT_REQUIRED                                            = "A component identifier has to be selected in the Inventory ID column for this action.";
            public const string EQUIPMENT_ACTION_TARGET_EQUIP_OR_NEW_TARGET_EQUIP_REQUIRED                     = "You have to select an equipment entity in the Target Equipment ID or Model Equipment Line Nbr. column for this action.";
            public const string MISSING_CUSTOMER_BILLING_CYCLE                                                 = "Customer has no billing cycle set for the current Service Order Type. Please, assign it before saving";
            public const string CANNOT_DELETE_DOCUMENT_IT_HAS_A_SERVICE_ORDER                                  = "The current document cannot be deleted because there is a service order linked to it. Please delete the associated service order first.";
            public const string EQUIPMENT_COMPONENT_ROW_QTY_EXCEEDED                                           = "The quantity of this component exceeds the original quantity specified for the model equipment.";
            public const string EQUIPMENT_COMPONENTS_QTY_ERROR                                                 = "Some errors related to the quantity of the components occurred.";
            public const string ROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_NONROUTE_SRVORDTYPE                       = "Route service cannot be handled with current non-route Service Order Type.";
            public const string NONROUTE_SERVICE_CANNOT_BE_HANDLED_WITH_ROUTE_SRVORDTYPE                       = "Non-route service cannot be handled with current route Service Order Type.";
            public const string AP_POSTING_VENDOR_NOT_FOUND                                                    = "This customer is not defined as a vendor. To extend the customer account to a customer and vendor account, select the Extend to Vendor action for this account on the form toolbar of the Customers (AR303000) form.";
            public const string SERVICE_ORDER_TYPE_DOES_NOT_ALLOW_AUTONUMBERING                                = "The appointment cannot be saved because a manual numbering sequence is assigned to the service order type {0} and the service order cannot be created automatically for the appointment. Create the service order on the Service Orders (FS300100) form first or modify the numbering sequence of the service order type on the Service Order Types (FS202300) form.";
            public const string INVALID_SERVICE_CONTRACT_STATUS_TRANSITION                                     = "The transition of the service contract status is invalid.";
            public const string EXPIRATION_DATE_LOWER_UPCOMING_STATUS                                          = "The expiration date must be later than the upcoming status date.";
            public const string EXPIRATION_DATE_LOWER_BUSINESS_DATE                                            = "The expiration date must be later than the business date.";
            public const string EFFECTIVE_DATE_LOWER_ACTUAL_DATE                                               = "The effective date must be later than or the same as the actual date.";
            public const string EFFECTIVE_DATE_GREATER_END_DATE                                                = "The effective date must be earlier than the expiration date.";
            public const string SCHEDULE_DATE_LESSER_THAN_CONTRACT_DATE                                        = "The dates are invalid. The schedule start date must be the same as or later than the start date of the related contract.";
            public const string SCHEDULE_START_DATE_GREATER_THAN_CONTRACT_END_DATE                             = "The dates are invalid. The schedule start date must be earlier than the expiration date of the related contract.";
            public const string SCHEDULE_END_DATE_GREATER_THAN_CONTRACT_END_DATE                               = "The dates are invalid. The schedule expiration date must be earlier than or the same as the expiration date of the related contract. ";
            public const string CONTRACT_START_DATE_GREATER_THAN_SCHEDULE_START_DATE                           = "The dates are invalid. The contract start date cannot be later than the start date of any of the contract schedules.";
            public const string CONTRACT_END_DATE_LESSER_THAN_SCHEDULE_END_DATE                                = "The dates are invalid. The contract end date cannot be earlier than the end date of any of the contract schedules.";
            public const string NO_DELETION_ALLOWED_DOCLINE_LINKED_TO_APP_SO                                   = "The line cannot be deleted because it is related to an appointment or service order.";
            public const string NO_UPDATE_ALLOWED_DOCLINE_LINKED_TO_APP_SO                                     = "This value cannot be updated because it is related to an appointment or service order.";
            public const string DUPLICATING_POSTING_DOCUMENT                                                   = "Attempt to create duplicated Invoice.";
            public const string NO_UPDATE_BILLING_CYCLE_SERVICE_CONTRACT_RELATED                               = "The billing cycle cannot be modified because it has been assigned to at least one customer whose {0} are related to prepaid contracts.";
            public const string PROCESSOR_ALREADY_RUNNING_A_PROCESS                                            = "This Processor is already running a process.";
            public const string STEP_GROUP_ALREADY_ASSIGNED_TO_PROCESSOR                                       = "The Step Group is already assigned to a Processor.";
            public const string PROCESSOR_DOES_NOT_HAVE_A_STEP_GROUP_ASSIGNED                                  = "This Processor doesn't have a Step Group assigned.";
            public const string STEP_GROUP_DOES_NOT_HAVE_A_PROCESSOR_ASSIGNED                                  = "This Step Group doesn't have a Processor assigned.";
            public const string STEP_GROUP_STILLS_RUNNING                                                      = "This Step Group is still running.";
            public const string INVALID_STEP_STATUS_RUNNING_STEPMETHOD                                         = "Invalid StepStatus trying to run StepMethod for the Step '{0}'.";
            public const string INVALID_STEP_STATUS_RUNNING_ONSTEPMETHODCOMPLETED                              = "Invalid StepStatus running OnStepMethodCompleted for the Step '{0}'.";
            public const string INVALID_STEP_STATUS_RUNNING_SETERROR                                           = "Invalid StepStatus running SetError for the Step '{0}'.";
            public const string PERIOD_WITHOUT_DETAILS                                                         = "Period without Details.";
            public const string STOCKITEM_NOT_HANDLED_BY_SRVORDTYPE                                            = "The service order cannot be created because the {0} stock item cannot be added to orders of the selected service order type. Select a service order type that generates invoices in the Sales Orders module.";
            public const string CANNOT_CLONE_APPOINMENT_SERVICE_ORDER_COMPLETED                                = "The appointment cannot be cloned because the associated service order has been completed.";
            public const string QTY_POSTED_ERROR                                                               = "The quantity in the posted document does not match the quantity in the source document.";
            public const string QTY_APPOINTMENT_SERIAL_ERROR                                                   = "Quantity cannot be greater than one for stock items with tracked serial numbers.";
            public const string QTY_APPOINTMENT_GREATER_THAN_SERVICEORDER                                      = "The total quantity for the line has exceeded the quantity specified in the service order.";
            public const string REPEATED_APPOINTMENT_SERIAL_ERROR                                              = "A serial number cannot be repeated for a stock item in different appointments.";
            public const string CANNOT_ADD_INVENTORY_TYPE_LINES_BECAUSE_SO_POSTED                              = "A new {0} cannot be added to this appointment because an invoice has already been generated for the related service order.";
            public const string SCHEDULE_TYPE_NONE                                                             = "The document cannot be saved because the associated service contract has the None schedule type.";
            public const string TIME_ZONE_REQUIRED_LOCATION_TRACKING_ENABLED                                   = "The Time Zone box cannot be empty if the Track Location check box is selected on the Location Tracking tab.";
            public const string ACTUAL_DURATION_CANNOT_BE_GREATER_24HRS                                        = "The actual duration cannot be greater than 24 hours.";
            public const string SELECTED_COMPONENT_HAS_ALREADY_BEEN_CHOSEN_FOR_REPLACEMENT                     = "The selected component has already been selected for replacement.";
            public const string SRVORDTYPE_INACTIVE                                                            = "The service order type is inactive. Select another service order type or activate this type on the Service Order Types (FS202300) form.";
            public const string POSTING_PMTASK_ALREADY_COMPLETED                                               = "The {1} line of the {0} document cannot be processed because the {2} project task has already been completed.";
            public const string POST_TO_MISSING_FEATURES                                                       = "Documents of the {0} type cannot be generated because the {1} feature is disabled on the Enable/Disable Features (CS100000) form.";
            public const string SOINVOICE_FROM_CONTRACT_NOT_IMPLEMENTED_CHANGE_IN_X_OR_Y                       = "The ability to generate SO invoices for service contracts has not been implemented yet. Please select another option in the Billing Settings section on the {0} form, or if you generate billing documents for route service contracts, on the {1} form.";
            public const string SOINVOICE_FROM_CONTRACT_NOT_IMPLEMENTED                                        = "The ability to generate SO invoices for service contracts has not been implemented yet. Please select another option.";
            public const string CANNOT_USE_ALLOCATIONS_FOR_NONSTOCK_ITEMS                                      = "Non-stock items cannot be allocated and you cannot assign lot or serial numbers to them.";
            public const string SALE_SORDER_HAS_ONLY_NON_INVENTORY_LINES                                       = "The opportunity contains one or multiple product lines with no inventory item specified. If you want to create a service order, you need to first review the product lines and specify inventory items where necessary because only lines with inventory items can be included in service orders.";
            public const string LOG_ACTION_NOT_ALLOWED                                                         = "The {0} action is allowed only for items of type Service.";
            public const string LOG_ACTION_NOT_ALLOWED_BY_STATUS                                               = "The Start and Complete actions are available for travel items when the appointment status is one of the following: {0}, {1}, or {2}.";
            public const string NON_TRAVEL_ITEM_CAN_BE_ASSIGNED                                                = "This item cannot be selected as the default travel item. Make sure that the Is a Travel Item check box is selected on the Non-Stock Items (IN202000) form or select another item.";
            public const string CANNOT_PERFORM_LOG_ACTION_RECORD_NOT_SELECTED                                  = "Select at least one record in the table to perform this action.";
            public const string ACTION_NOT_VALID_FOR_LINE_X_WITH_STATUS_X                                      = "The action is not allowed for the {0} line with status {1}.";
            public const string CANNOT_CANCEL_FSSODET_LINE                                                     = "The {0} line cannot be canceled because one or multiple related lines are not canceled in the associated appointments. Cancel these lines on the Appointments (FS300200) form first.";
            public const string CANNOT_COMPLETE_FSSODET_LINE                                                   = "The {0} line cannot be completed because one or multiple related lines have the Not Started or In Progress status in the associated appointments. Select the appropriate status on the Appointments (FS300200) form first.";
            public const string CANNOT_CANCEL_SERVICE_ORDER_LINE_IN_STATUS_COMPLETE                            = "The service order cannot be canceled because at least one item has the Completed status.";
            public const string CANNOT_CANCEL_APPOINTMENT_WITH_LOG                                             = "An appointment with log lines cannot be canceled. To cancel this appointment, delete all log lines on the Log tab first.";
            public const string INVALID_NEW_STATUS                                                             = "The new status is invalid.";
            public const string CANNOT_COMPLETE_APPOINTMENT_WITH_NOTSTARTED_INPROCESS_ITEM_LINES               = "The appointment cannot be completed because it contains detail lines with the Not-Started or In-Process status.";
            public const string CANNOT_COMPLETE_APPOINTMENT_WITH_INPROCESS_LOG_LINES                           = "The appointment cannot be completed because it contains log lines with the In-Process status.";
            public const string COMPLETED_APPOINTMENT_CAN_ONLY_HAVE_COMPLETED_NON_TRAVEL_LINES                 = "Non-travel items can be added to a completed appointment only if they have the Completed status.";
            public const string SELECT_AT_LEAST_ONE_STAFF_MEMBER                                               = "You must select at least one staff member.";
            public const string APPOINTMENT_COULD_NOT_BE_REACH_SERVICED                                        = "This appointment could not be reached/serviced";
            public const string APPOINTMENT_COULD_NOT_BE_REACH_SERVICED_NO_DRIVER_AVAILABLE                    = "This appointment could not be reached/serviced because there is no driver available for the given time frame.";
            public const string WORKWAVE_INTERNAL_SERVER_ERROR                                                 = "WorkWave Internal Server Error. Please try again later and also contact WorkWave support team so that they may investigate.";
            public const string WORKWAVE_UNKNOWN_API_KEY                                                       = "Wrong authentication key for WorkWave API";
            public const string WORKWAVE_KEY_CONSTRAINT_VIOLATED                                               = "One or more constraints associated with the WorkWave authentication key, such as maximum number of Drivers or Waypoints, have been violated.";
            public const string WORKWAVE_KEY_DAILY_LIMIT_EXCEEDED                                              = "The maximum daily amount of requests has been reached. Further requests must wait until the next day.";
            public const string WORKWAVE_KEY_EXPIRED                                                           = "The WorkWave authentication key is expired.";
            public const string WORKWAVE_TOO_MANY_REQUESTS                                                     = "The given WorkWave key has submitted too many requests in a short period of time. Insert a pause between requests to avoid getting this error.";
            public const string WORKWAVE_TOO_MANY_CONCURRENT_REQUESTS                                          = "The given WorkWave key has submitted too many concurrent requests. Wait for previously submitted requests to complete to avoid getting this error.";
            public const string WORKWAVE_MALFORMED_REQUEST                                                     = "Malformed Request made to WorkWave. Something is wrong with the input. Either a query-string parameter is missing or malformed or the JSON data in the request body is malformed (typically due to a missing or overabundant parenthesis or comma or a missing mandatory field).";
            public const string DETAIL_LINE_CANNOT_BE_STARTED                                                  = "The {0} action is not allowed for the {1} line with the {2} status. To perform this action, the appointment must have the In Process status or the selected detail must be a travel item.";
            public const string DETAIL_LINE_CANNOT_BE_CANCELED                                                 = "The {0} action is not allowed for the {1} line with the {2} status. To perform this action, a selected line must have the Not Started or Not Performed status.";
            public const string DETAIL_LINE_CANNOT_BE_COMPLETED_OR_RESUMED                                     = "The {0} action is not allowed for the {1} line with the {2} status. To perform this action, a selected line must have the In Process status and the document must have the In Process or Paused status.";
            public const string DETAIL_LINE_CANNOT_BE_STARTED_FOR_SERVICE                                      = "The {0} action is not allowed for the {1} line with the {2} status. Use the {3} action.";
            public const string DETAIL_LINE_CANNOT_BE_COMPLETED_OR_RESUMED_FOR_SERVICE                         = "The {0} action is not allowed for the {1} line with the {2} status. Use the {3} action.";
            public const string ThereAreMoreLotSerialNumbersThanQuantitySpecifiedOnTheLine                     = "There are more Lot/Serial numbers in the shipment or sales order than the quantity of items specified in the invoice line.";
        }

        //Warning messages
        [PXLocalizable]
        public static class Warning
        {
            public const string END_TIME_PRIOR_TO_START_TIME_SHIFT                                        = "The end time is earlier than the start time. Thus, this shift will end the following day.";
            public const string END_TIME_PRIOR_TO_START_TIME_APPOINTMENT                                  = "The end time is earlier than the start time. This appointment will end the following day.";
            public const string NO_EXECUTION_DAYS_SELECTED_FOR_ROUTE                                      = "Execution dates are not selected for this route. Appointments cannot be generated for the route.";
            public const string EMAIL_ACCOUNT_NOT_CONFIGURED_FOR_MAILING                                  = "The email cannot be sent because an email account has not been specified for the {0} mailing of the {1} service order type. Specify the email account on the Service Order Types (FS202300) form or configure the default email account on the {2} form.";
            public const string SALES_ORDER_NOT_INVOICE                                                   = "The recommended option is Invoice. Otherwise, sales orders will require shipments.";
            public const string END_TIME_AUTOMATICALLY_CALCULATED_NOTIFICATION                            = "The end time has been updated based on the {0} duration total of the appointment because the services have been modified.";
            public const string CPNY_WARRANTY_DURATION_LESSTHAN_VENDOR_WARANTY_DURATION                   = "The company warranty duration should not be less than the vendor warranty duration.";
            public const string CANT_CREATE_A_SERVICE_ORDER_FROM_AN_INVOICED_SALES_ORDER                  = "This sales order originated from a service order. A new service order cannot be created from the sales order.";
            public const string INCOMPLETE_APPOINTMENT_ADDRESS                                            = "The address of the appointment is incomplete. This missing information can produce inconsistencies in statistics and routes.";
            public const string ROUTE_MISSING_DRIVER_OR_AND_VEHICLE                                       = "This route is missing a {0}. Do you want to proceed anyway?";
            public const string INVALID_SERVICE_DURATION                                                  = "A service duration cannot be less than one minute.";
            public const string REQUIRES_SERIAL_NUMBER                                                    = "The serial number is required.";
            public const string CANNOT_MODIFY_FIELD                                                       = "This element cannot be modified because there are {0} related to this {1}.";
            public const string ITEM_WITH_NO_WARRANTIES_CONFIGURED                                        = "Components and warranties are not defined for this item.";
            public const string SCHEDULE_WILL_NOT_AFFECT_SYSTEM_UNTIL_GENERATION_OCCURS                   = "This schedule will not affect the system until a generation process takes place.";
            public const string SCHEDULE_BEGIN_RESET                                                      = "The beginning of the schedule has been reset. All new generations will now start on the custom start date. Are you sure you want to continue?";
            public const string CUSTOMER_DOES_NOT_MATCH_PROJECT                                           = "The customer in the line is not the same as the customer in the project or contract.";
            public const string ACTUAL_DATE_AFTER_SLA                                                     = "The actual date is later than the SLA date.";
            public const string SHELL_FUNCTION_DEPRECATED                                                 = "ShellFunction is deprecated, please use OnTransactionInserted instead.";
            public const string CUSTOMER_CLASS_BILLING_SETTINGS                                           = "Please confirm if you want to update the current customer billing settings with the customer class defaults. Otherwise, the original billing settings will be preserved.";
            public const string CUSTOMER_MULTIPLE_BILLING_OPTIONS_CHANGING                                = "If you change this setting, the billing information will be updated in all related documents. This may take awhile.";
            public const string WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE              = "The warranty cannot be calculated because Installation Date is empty. Specify the installation date on the General Info tab.";
            public const string WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_SALEDATE                      = "The warranty cannot be calculated because Sales Date is empty. Specify the sales date on the General Info tab.";
            public const string WARRANTY_CANNOT_BE_CALCULATED_BECAUSE_EMPTY_INSTALLATIONDATE_AND_SALEDATE = "The warranty cannot be calculated because both Installation Date and Sales Date are empty.";
            public const string DOCUMENTS_WITHOUT_BILLING_INFO                                            = "Some Documents are not visible on this screen, they have not billing settings associated.";
            public const string USE_FIX_SERVICE_ORDERS_BUTTON                                             = "Use Fix Service Orders Without Billing Settings button.";
            public const string SCHEDULES_WITHOUT_NEXT_EXECUTION                                          = "Some schedules are not displayed on the form because the next execution dates are not defined for them. Click the Fix Schedules Without Next Execution button to define the dates and view the schedules on the form.";
            public const string RETRYING_CREATE_INVOICE_AFTER_ERROR                                       = "Technical Error: retrying CreateInvoice with the BatchNbr {0}.";
            public const string WAITING_FOR_PARTS                                                         = "The receipt of at least one item is needed.";
            public const string APPOINTMENT_WAS_NOT_FINISHED                                              = "The appointment was not finished.";
            public const string SIGNED_APP_EMAIL_ACTION_IS_DISABLED                                       = "The Send Email with Signed Appointment action cannot be performed. To perform the action, sign the appointment by using the mobile app.";
			public const string ROUTE_APPROXIMATE_VALUES                                    			  = "[*] Approximate values. Use for reference only.";
        }

        [PXLocalizable]
        public static class ModuleName
        {
            public const string SERVICE_DISPATCH    = "Service Management";
            public const string SERVICE_DESCRIPTOR  = "(SERVICE)";
            public const string EQUIPMENT_MODULE    = "Equipment Management";
            public const string ROUTES_MODULE       = "Routes Management";
        }


        [PXLocalizable]
        public static class ButtonDisplays
        {
            public const string DeleteProc    = "Delete";
            public const string DeleteAllProc = "Delete All";
        }

        [PXLocalizable]
        public static class SourceType_ServiceOrder
        {
            public const string CASE                = "CR - Case";
            public const string OPPORTUNITY         = "CR - Opportunity";
            public const string SALES_ORDER         = "SO - Sales Order";
            public const string SERVICE_DISPATCH    = "SD - Service Dispatch";
            public const string QUOTE               = "SD - Quote";
        }

        #region Priority+Severity
        [PXLocalizable]
        public static class Priority_ALL
        {
            public const string LOW = "Low";
            public const string MEDIUM = "Medium";
            public const string HIGH = "High";
        }
        #endregion

        #region LineType
        [PXLocalizable]
        public static class LineType_ALL
        {
            public const string INVENTORY_ITEM      = "Inventory Item";
            public const string SERVICE             = "Service";
            public const string NONSTOCKITEM        = "Non-Stock Item";
            public const string SERVICE_TEMPLATE    = "Service Template";
            public const string PICKUP_DELIVERY     = "Pickup/Delivery Item";
            public const string LABOR_ITEM          = "Labor";
            public const string COMMENT             = "Comment";
            public const string INSTRUCTION         = "Instruction";
        }
        #endregion

        #region PriceType
        [PXLocalizable]
        public static class PriceType
        {
            public const string CONTRACT    = "Contract";
            public const string CUSTOMER    = "Customer";
            public const string PRICE_CLASS = "Customer Price Class";
            public const string BASE        = "Base";
            public const string DEFAULT     = "Default";
        }
        #endregion

        #region Status
        [PXLocalizable]
        public class Status_SODet
        {
            public const string SCHEDULED_NEEDED    = "Requiring Scheduling";
            public const string SCHEDULED           = "Scheduled";
            public const string COMPLETED           = "Completed";
            public const string CANCELED            = "Canceled";
        }

        [PXLocalizable]
        public class Status_AppointmentDet
        {
            public const string NOT_FINISHED    = "Not Finished";
            public const string NOT_PERFORMED   = "Not Performed";
            public const string COMPLETED       = "Completed";
            public const string IN_PROCESS      = "In Process";
            public const string NOT_STARTED     = "Not Started";
            public const string CANCELED        = "Canceled";
        }

        [PXLocalizable]
        public static class Status_Appointment
        {
            public const string AUTOMATIC_SCHEDULED = "Scheduled by System";
            public const string MANUAL_SCHEDULED    = "Not Started";
            public const string IN_PROCESS          = "In Process";
            public const string CANCELED            = "Canceled";
            public const string COMPLETED           = "Completed";
            public const string CLOSED              = "Closed";
            public const string ON_HOLD             = "On Hold";

            // This status contains both AUTOMATIC_SCHEDULED and MANUAL_SCHEDULED statuses for External Control
            public const string SCHEDULED           = "Scheduled";
        }

        [PXLocalizable]
        public static class Status_ServiceOrder
        {
            public const string OPEN      = "Open";
            public const string QUOTE     = "Quote";
            public const string ON_HOLD   = "On Hold";
            public const string CANCELED  = "Canceled";
            public const string CLOSED    = "Closed";
            public const string COMPLETED = "Completed";
        }

        [PXLocalizable]
        public static class Status_ServiceContract
        {
            public const string DRAFT = "Draft";
            public const string ACTIVE   = "Active";
            public const string SUSPENDED = "Suspended";
            public const string CANCELED = "Canceled";
            public const string EXPIRED = "Expired";
        }

        [PXLocalizable]
        public static class Status_ContractPeriod
        {
            public const string ACTIVE   = "Active";
            public const string PENDING  = "Pending for Invoice";
            public const string INACTIVE = "Inactive";
            public const string INVOICED = "Invoiced";
        }

        [PXLocalizable]
        public static class Status_Route
        {
            public const string OPEN       = "Open";
            public const string IN_PROCESS = "In Process";
            public const string CANCELED   = "Canceled";
            public const string COMPLETED  = "Completed";
            public const string CLOSED     = "Closed";
        }

        [PXLocalizable]
        public static class Status_Posting
        {
            public const string NOTHING_TO_POST = "Nothing to Post";
            public const string PENDING_TO_POST = "Pending to Post";
            public const string POSTED          = "Posted";
        }

        [PXLocalizable]
        public static class Status_Log
        {
            public const string IN_PROCESS = "In Process";
            public const string COMPLETED = "Completed";
        }
        #endregion

        [PXLocalizable]
        public static class Type_Log
        {
            public const string TRAVEL = "Travel";
            public const string SERVICE = "Service";
            public const string NON_STOCK = "Non-Stock";
            public const string STAFF_ASSIGMENT = "Staff and Service (If Any)";
            public const string SERV_BASED_ASSIGMENT = "Services and Assigned Staff (If Any)";
        }

        [PXLocalizable]
        public class LogActions
        {
            public const string START = "Start";
            public const string COMPLETE = "Complete";
        }

        [PXLocalizable]
        public static class FuelType_Equipment
        {
            public const string REGULAR_UNLEADED = "Regular Unleaded";
            public const string PREMIUM_UNLEADED = "Premium Unleaded";
            public const string DIESEL           = "Diesel";
            public const string OTHER            = "Other";
        }

        #region Confirmed_Appointment
        [PXLocalizable]
        public class Confirmed_Appointment
        {
            public const string ALL = "<ALL>";
            public const string CONFIRMED = "Confirmed";
            public const string NOT_CONFIRMED = "Not Confirmed";
        }

        [PXLocalizable]
        public class ValidationType
        {
            public const string PREVENT = "Prevent";
            public const string WARN = "Warn";
            public const string NOT_VALIDATE = "Do Not Validate";
        }

        [PXLocalizable]
        public class SourcePrice
        {
            public const string CONTRACT   = "Contract";
            public const string PRICE_LIST = "Regular Price";
        }

        [PXLocalizable]
        public class RecordType_ContractAction
        {
            public const string CONTRACT = "Contract";
            public const string SCHEDULE = "Schedule";
        }

        [PXLocalizable]
        public class Action_ContractAction
        {
            public const string CREATE              = "Create (New)";
            public const string ACTIVATE            = "Activate";
            public const string SUSPEND             = "Suspend";
            public const string CANCEL              = "Cancel";
            public const string EXPIRE              = "Expire";
            public const string INACTIVATE_SCHEDULE = "Inactivate - Schedule";
            public const string DELETE_SCHEDULE     = "Delete - Schedule";
        }

        [PXLocalizable]
        public static class PeriodType
        {
            public const string DAY = "Day";
            public const string WEEK = "Week";
            public const string MONTH = "Month";
        }

        #endregion

        #region ServiceOrderType
        [PXLocalizable]
        public static class SrvOrdType_RecordType
        {
            //public const string TRAVEL              = "Travel Time";
            //public const string TRAINING            = "Training Time";
            //public const string DOWNTIME            = "Down Time";
            public const string SERVICE_ORDER       = "Service Order";
            public const string RECURRING_TEMPLATE  = "Recurring Template";
        }

        [PXLocalizable]
        public static class SrvOrdType_SalesAcctSource
        {
            public const string INVENTORY_ITEM      = "Inventory Item";
            public const string WAREHOUSE           = "Warehouse";
            public const string POSTING_CLASS       = "Posting Class";
            public const string CUSTOMER_LOCATION   = "Customer/Vendor Location";
        }

        [PXLocalizable]
        public static class Contract_SalesAcctSource
        {
            public const string CUSTOMER_LOCATION   = "Customer/Vendor Location";
            public const string INVENTORY_ITEM      = "Inventory Item";
            public const string POSTING_CLASS       = "Posting Class";
        }

        [PXLocalizable]
        public static class SrvOrdType_GenerateInvoiceBy
        {
            public const string CRM_AR = "CRM/AR";
            public const string SALES_ORDER = "Sales Order";
            public const string PROJECT = "Project";
            public const string NOT_BILLABLE = "Not Billable";
        }

        [PXLocalizable]
        public static class SrvOrdType_BillingType
        {
            public const string COST_AS_COST    = "Cost as Cost";
            public const string COST_AS_REVENUE = "Revenue as Cost";
        }

        [PXLocalizable]
        public class SrvOrdType_PostTo
        {
            public const string NONE = "None";
            public const string ACCOUNTS_RECEIVABLE_MODULE = "AR Documents";
            public const string SALES_ORDER_MODULE = "Sales Orders";
            public const string SALES_ORDER_INVOICE = "SO Invoices";
            public const string PROJECTS = "Project Transactions";
        }

        [PXLocalizable]
        public class OnCompleteApptSetInProcessItemsAs
        {
            public const string DoNothing = "In Process";
            public const string Completed = "Completed";
            public const string NotFinished = "Not Finished";
        }

        [PXLocalizable]
        public class OnCompleteApptSetNotStartedItemsAs
        {
            public const string NotPerformed = "Not Performed";
            public const string Completed = OnCompleteApptSetInProcessItemsAs.Completed;
            public const string DoNothing = "Not Started";
        }

        [PXLocalizable]
        public class Contract_PostTo
        {
            public const string ACCOUNTS_RECEIVABLE_MODULE = "Accounts Receivable";
            public const string SALES_ORDER_MODULE = "Sales Order";
            public const string SALES_ORDER_INVOICE = "SO Invoice";
        }

        //FSSrvOrdType - NewBusinessAcctType
        [PXLocalizable]
        public static class BusinessAcctType
        {
            public const string CUSTOMER = "Customer";
            public const string PROSPECT = "Prospect";
        }

        [PXLocalizable]
        public static class Source_Info
        {
            public const string BUSINESS_ACCOUNT = "Business Account";
            public const string CUSTOMER_CONTACT = "Contact";
            public const string BRANCH_LOCATION = "Branch Location";
        }
        #endregion

        //ReasonType - FSReason
        [PXLocalizable]
        public class ReasonType
        {
            public const string CANCEL_SERVICE_ORDER = "Cancel Service Order";
            public const string CANCEL_APPOINTMENT   = "Cancel Appointment";
            public const string WORKFLOW_STAGE       = "Workflow Stage";
            public const string APPOINTMENT_DETAIL   = "Appointment Detail";
            public const string GENERAL              = "General";
        }

        [PXLocalizable]
        public static class Setup_CopyTimeSpentFrom
        {            
            public const string ACTUAL_DURATION     = "Actual Service Duration";
            public const string ESTIMATED_DURATION  = "Estimated Service Duration";
            public const string NONE                = "None";
        }

        [PXLocalizable]
        public static class CreateInvoice_ActionType
        {
            public const string AR    = "Create invoice(s) in AR";
            public const string SO    = "Create invoice(s) in Sales Order";
            public const string AR_SO = "Create invoice(s) in AR & Sales Order";
        }

        [PXLocalizable]
        public static class Dispatch_Board
        {
            public const string DISPLAY_NAME_FILTER       = "DisplayName";
            public const string EMPLOYEE_ID_FILTER        = "EmployeeID";
            public const string ASSIGNED_EMPLOYEE_FILTER  = "AssignedEmpID";
            public const string REPORT_TO_EMPLOYEE_FILTER = "ReportsTo";
            public const string LIKETEXT_FILTER           = "LikeText";
            public const string SKILL_FILTER              = "Skill";
            public const string LICENSE_TYPE_FILTER       = "LicenseType";
            public const string PROBLEM_FILTER            = "Problem";
            public const string SERVICE_CLASS_FILTER      = "ServiceClass";
            public const string GEO_ZONE_FILTER           = "GeoZone";
            public const string SERVICE_FILTER            = "Service";
            public const string DEFINED_SCHEDULER_FILTER  = "DefinedScheduler";
        }

        [PXLocalizable]
        public static class CustomTextFields
        {
            public const string DRIVER_ID            = "Driver ID";
            public const string VEHICLE_ID           = "Vehicle ID";
            public const string CUSTOMER_LOCATION    = "Customer Location";
            public const string CUSTOMER_ID          = "Customer ID";
            public const string DESCRIPTION          = "Description";
            public const string ESTIMATED_DURATION   = "Estimated Duration";
            public const string CUSTOMER_NAME        = "Customer Name";
            public const string STAFF_MEMBER_ID      = "Staff Member ID";
            public const string STAFF_MEMBER_NAME    = "Staff Member Name";
        }

        [PXLocalizable]
        public static class RecurrenceDescription
        {
            public const string ST           = "st";
            public const string ND           = "nd";
            public const string RD           = "rd";
            public const string TH           = "th";
            public const string ON           = "on";
            public const string OF           = "of";
            public const string THAT         = "that";
            public const string THE          = "the";
            public const string AND          = "and";
            public const string DAYS         = "Days";
            public const string WEEKS        = "Weeks";
            public const string MONTHS       = "Months";
            public const string YEARS        = "Years";
            public const string DAY          = "day";
            public const string MONTH        = "month";
            public const string OCCURS_EVERY = "Occurs every";
        }

        [PXLocalizable]
        public static class WildCards
        {
            public const string STAFF = "Staff";
            public const string SERVICE = "Service";
            public const string SHIPPING_ADDRESS = "Shipping Address";
            public const string SHIPPING_CONTACT = "Shipping Contact";
        }

        [PXLocalizable]
        public static class Messages
        {
            public const string UPDATE_SCHEDULEDTIME_WITH_SERVICESTIME             = "The Sum of the Services Estimated Duration does not match the duration defined in the Scheduled Time section. Do you want to update the End Scheduled Time?";
            public const string ASK_UPDATE_ACTUAL_DURATION_TOTAL                   = "The actual duration of the appointmet is lesser than the services actual duration summatory. Would you like to replace it anyway?";
            public const string NO_CUSTOMER_LOCATION                               = "No Customer Location defined";
            public const string POSITION_PROPAGATE_CONFIRM                         = "Changes will be saved. Do you want to propagate the changes to the associated Employees?";
            public const string ASK_CONFIRM_ROLLBACK_ADVANCED_CONTRACT_GENERATION  = "All the Service Orders generated in the latest generation process will be deleted. Do you want to continue?";
            public const string ASK_CONFIRM_ROLLBACK_ROUTES_CONTRACT_GENERATION    = "All Routes and Appointments generated in the latest generation process will be deleted. Do you want to continue?";                                    
            public const string ASK_CONFIRM_ROLLBACK_SCHEDULE_GENERATION           = "All schedule rules generated in the last run process are going to be deleted. Do you want to continue?";
            public const string ASK_CONFIRM_UNASSIGN_APPOINTMENT                   = "The selected appointment will be unassigned. Are you sure?";
            public const string ASK_CONFIRM_DELETE_APPOINTMENT_FROM_DB             = "The selected appointment will be deleted from all the records. Are you sure?";          
            public const string MASKCUSTOMERLOCATION                               = "Customer Location";
            public const string MASKITEM                                           = "Inventory Item";
            public const string MASKSERVICEORDERTYPE                               = "Service Order Type";
            public const string MASKCOMPANY                                        = "Branch";
            public const string MASKBRANCHLOCATION                                 = "Branch Location";
            public const string MASKPOSTINGCLASS                                   = "Posting Class";
            public const string MASKSALESPERSON                                    = "Salesperson";
            public const string MASKWAREHOUSE                                      = "Warehouse";
            public const string LIST_LAST_ITEM_PREFIX                              = " and ";
            public const string NO_STAFF_ASSIGNED_FOR_THE_APPOINTMENT              = "THERE IS NO STAFF ASSIGNED FOR THIS APPOINTMENT";
            public const string NO_CONTACT_FOR_THE_CUSTOMER                        = "CONTACT NAME MISSING";
            public const string NO_CONTACT_CELL_FOR_THE_CUSTOMER                   = "CONTACT CELL MISSING";
            public const string NO_CONTACT_CELL_FOR_THE_STAFF                      = "STAFF CONTACT CELL MISSING";
            public const string ASK_CONFIRM_CALENDAR_WEEKCODE_GENERATION           = "Calendar Week Code will be automatically generated from {0} to {1}. Do you want to proceed?";
            public const string CANNOT_HIDE_ROOMS_IN_SM                            = "Currently there is at least one Service Order Type requiring rooms. Turning off this feature, will also disable the rooms requirement for the Service Order Types. Would you like to proceed with this change?";
            public const string RECORD_PROCESSED_SUCCESSFULLY                      = "Record processed successfully.";
            public const string COULD_NOT_PROCESS_RECORD_BY_BILLING_GROUP_WITHDOC  = "Could not process this record. Its billing group is having an error with document ( Service Order Type: {0}, Reference Nbr.: {1} ).";
            public const string COULD_NOT_PROCESS_RECORD_BY_BILLING_GROUP          = "Could not process this record. Its billing group is having an error.";
            public const string COULD_NOT_PROCESS_RECORD                           = "Could not process this record.";
			public const string CLEAR_ALL_FILTERS                                  = "Clear All Filters";
            public const string SERVICE_ORDER_TYPE_USED_FOR_RECURRING_APPOINTMENTS = "This Service Order Type will be used for the recurring appointments";
            public const string ASK_CONFIRM_ROUTE_CLOSING                          = "The current route will be closed. Do you want to proceed?";
            public const string ASK_CONFIRM_ROUTE_UNCLOSING                        = "The current route will be unclosed. Do you want to proceed?";
            public const string ASK_CONFIRM_APPOINTMENT_UNCLOSING                  = "The current appointment will be unclosed. Do you want to proceed?";
            public const string ASK_CONFIRM_SERVICE_ORDER_UNCLOSING                = "The current service order will be unclosed. Do you want to proceed?";
            public const string ASK_CONFIRM_CHANGE_UOM_FSSALESPRICE                = "Do you want to replicate this Base Unit change to the details of Service Contracts?";
            public const string ASK_CONFIRM_SERVICE_ORDER_CLOSING                  = "This Service Order still has open Appointments. If you close the Service Order its appointments will also be closed. Do you want to proceed?";
            public const string ASK_CONFIRM_DELETE_CURRENT_ROUTE                   = "The Appointments and Service Orders will be also deleted with this action. Are you sure?";
            public const string EQUIPMENT_IS_INSTATUS                              = "Equipment is {0}.";
            public const string VEHICLE_IS_INSTATUS                                = "Vehicle is {0}.";
            public const string ACCESS_RIGHTS_NOTIFICATION                         = "You have insufficient access rights to perform this action.";
            public const string ASK_CONFIRM_MODEL_AND_COMPONETS_RESET              = "Please confirm if you want to update current Manufacturer Model and Components with the Model defaults. Original values will be preserved otherwise.";
            public const string ERROR_CREATING_POSTING_BATCH                       = "An error occurred creating the Posting Batch {0}. The batch will be deleted.";
            public const string ASK_CONFIRM_SERVICE_ORDER_COMPLETING               = "This Service Order does not have any other Appointment to be completed. Do you want to complete this Service Order?";
            public const string ASK_CONFIRM_SERVICE_ORDER_CANCELING                = "All appointments of this service order have been canceled. Do you want to cancel this service order?";
            public const string EMPLOYEE_IS_IN_STATUS                              = "Employee is {0}.";
            public const string COMPONENT_ALREADY_REPLACED                         = "Component selected was already replaced.";
            public const string ServiceOrderTax                                    = "Service Order Tax";
            public const string AppointmentTax                                     = "Appointment Tax";
            public const string CREATE_INVOICE_BILLING_CYCLE                       = "Create invoice document for Billing Cycle '{0}', GroupKey '{1}'.";
            public const string APPLY_PREPAYMENT_BILLING_CYCLE                     = "Apply Prepayments on the invoice documents generated for Billing Cycle '{0}'.";
            public const string CREATE_FSPOSTBATCH                                 = "Create FSPostBatch for Billing Cycle '{0}'.";
            public const string COMPLETE_FSPOSTBATCH_BILLING_CYCLE                 = "Complete FSPostBatch for Billing Cycle '{0}'.";
            public const string RETRY_CREATE_INVOICE                               = "Retrying CreateInvoice with the BatNbr = {0} and GroupKey = {1} after getting error: {2}.";
            public const string DONT_APPROVED_DOCUMENTS_CANNOT_BE_SELECTED         = "A Prepayment cannot be created for a Service Order with one of the following statuses: On Hold, Quote, or Canceled.";
            public const string ASK_SALES_ORDER_HAS_NON_INVENTORY_LINES            = "The opportunity contains one or multiple product lines with no inventory item specified. Click OK if you want to ignore these product lines and proceed with creating a service order. Click Cancel if you want to review the product lines and specify inventory items where necessary.";
            public const string ERRORS_IN_OTHER_SERVICEORDER                       = "The line has not been processed because errors occurred in other service orders. See the Errors tab.";
            public const string ERRORS_IN_OTHER_APPOINTMENT                        = "The line has not been processed because errors occurred in other appointments. See the Errors tab.";
            public const string ERROR_TRYING_TO_SET_A_NEWVALUE                     = "The following error occurred on specifying a value in {0}: {1}";
            public const string CONTRACT_WITH_STANDARDIZED_BILLING                 = "Contract with Standardized Billing: {0} {1}";
            public const string MISSING_PRIMARY_DRIVER                             = "Select the primary driver on the Staff tab.";
        }

        [PXLocalizable]
        public static class WebDialogTitles
        {
            public const string SRVORDER_NOTE_WINDOW                    = "Service Order Note";
            public const string POSITION_PROPAGATE_CONFIRM              = "Propagation Confirmation";
            public const string CONFIRM_ROLLBACK_CONTRACT_GENERATION    = "Confirm roll back generation";
            public const string CONFIRM_UNASSIGN_APPOINTMENT            = "Confirm Unassign Appointment";
            public const string CONFIRM_CALENDAR_WEEKCODE_GENERATION    = "Confirm Calendar Week Code Generation";
            public const string CONFIRM_MANAGE_ROOMS                    = "Confirm Manage Rooms change";
            public const string CONFIRM_ROUTE_CLOSING                   = "Confirm Route Closing";
            public const string CONFIRM_ROUTE_UNCLOSING                 = "Confirm Route Unclosing";
            public const string CONFIRM_APPOINTMENT_UNCLOSING           = "Confirm Appointment Unclosing";
            public const string CONFIRM_SERVICE_ORDER_UNCLOSING         = "Confirm Service Order Unclosing";
            public const string CONFIRM_SERVICE_ORDER_CLOSING           = "Confirm Service Order Closing";
            public const string CONFIRM_ROUTE_DELETING                  = "Confirm Delete Current Route";
            public const string CONFIRM_CHANGE_FSSALESPRICE_UOM         = "Confirm Base Unit change for Service Contract";
            public const string CONFIRM_SERVICE_ORDER_COMPLETING        = "Complete Service Order";
            public const string CONFIRM_SERVICE_ORDER_CANCELING         = "Cancel Service Order";
            public const string UPDATE_BILLING_SETTINGS                 = "Update Billing Settings";
        }

        [PXLocalizable]
        public static class AppResizePrecision_Setup
        {
            public const string MINUTES_10 = "10 MINUTES";
            public const string MINUTES_15 = "15 MINUTES";
            public const string MINUTES_30 = "30 MINUTES";
            public const string MINUTES_60 = "60 MINUTES";
        }

        [PXLocalizable]
        public static class DfltCalendarViewMode_Setup
        {
            public const string VERTICAL   = "Vertical";
            public const string HORIZONTAL = "Horizontal";
        }

        // FSModelWarranty - WarrantyDurationType
        [PXLocalizable]
        public static class WarrantyDurationType
        {
            public const string DAY   = "Days";
            public const string MONTH = "Months";
            public const string YEAR  = "Years";
        }

        // FSModelWarranty - DfltWarrantyApplicationOrder
        [PXLocalizable]
        public static class WarrantyApplicationOrder
        {
            public const string COMPANY = "Company";
            public const string VENDOR  = "Vendor";
        }

        [PXLocalizable]
        public static class ModelType
        {
            public const string EQUIPMENT   = "Equipment";
            public const string REPLACEMENT = "Replacement Part";
        }

        // SourceType_Equipment
        [PXLocalizable]
        public class SourceType_Equipment
        {
            public const string SM_EQUIPMENT       = "SD - Equipment";
            public const string VEHICLE            = "SD - Vehicle";
            public const string EP_EQUIPMENT       = "EP - Equipment";
            public const string AR_INVOICE         = "AR - Invoice";
        }

        // SourceType_Equipment_ALL
        //This class is used for filtering purposes only
        [PXLocalizable]
        public class SourceType_Equipment_ALL : SourceType_Equipment
        {
            public const string ALL = "All";
        }

        [PXLocalizable]
        public class OwnerType_Equipment
        {
            public const string CUSTOMER        = "Customer";
            public const string OWN_COMPANY     = "Company";
        }       

        public class MapsWebService
        {
            public const string URL_PREFIX = "https://dev.virtualearth.net/REST/v1/Routes/Driving?distanceUnit=mi&o=xml&";
        }

        public class ViewNames
        {
            public const string ServiceContractAnswers = "Service Contract Answers";
        }

        [PXLocalizable]
        public static class FrecuencySchedule
        {
            public const string DAILY             = "Daily";
            public const string WEEKLY            = "Weekly";
            public const string MONTHSPECIFICDATE = "Specific Date in a Month";
            public const string MONTHWEEKDAY      = "Specific Week Day of the Month";
        }

        [PXLocalizable]
        public static class ActionType_ProcessServiceContracts
        {
            public const string STATUS = "Update to Upcoming Status";
            public const string PERIOD = "Activate Upcoming Billing Period";
        }

        #region Almanac

        #region WeekDays
        [PXLocalizable]
        public class WeekDays
        {
            public const string SUNDAY = "Sunday";
            public const string MONDAY = "Monday";
            public const string TUESDAY = "Tuesday";
            public const string WEDNESDAY = "Wednesday";
            public const string THURSDAY = "Thursday";
            public const string FRIDAY = "Friday";
            public const string SATURDAY = "Saturday";
            public const string WEEKEND = "Weekend";
            public const string WEEKDAY = "Weekday";
            public const string ANYDAY = "Any";
        }
        #endregion

        #region Month
        [PXLocalizable]
        public class Months
        {
            public const string JANUARY = "January";
            public const string FEBRUARY = "February";
            public const string MARCH = "March";
            public const string APRIL = "April";
            public const string MAY = "May";
            public const string JUNE = "June";
            public const string JULY = "July";
            public const string AUGUST = "August";
            public const string SEPTEMBER = "September";
            public const string OCTOBER = "October";
            public const string NOVEMBER = "November";
            public const string DECEMBER = "December";
        }
        #endregion

        #region ShortMonth
        [PXLocalizable]
        public class ShortMonths
        {
            public const string JANUARY = "JAN";
            public const string FEBRUARY = "FEB";
            public const string MARCH = "MAR";
            public const string APRIL = "APR";
            public const string MAY = "MAY";
            public const string JUNE = "JUN";
            public const string JULY = "JUL";
            public const string AUGUST = "AUG";
            public const string SEPTEMBER = "SEP";
            public const string OCTOBER = "OCT";
            public const string NOVEMBER = "NOV";
            public const string DECEMBER = "DEC";
        }
        #endregion

        #endregion

        #region TimePositioning
        #region Counting
        [PXLocalizable]
        public class Counting
        {
            public const string FIRST = "First";
            public const string SECOND = "Second";
            public const string THIRD = "Third";
            public const string FOURTH = "Fourth";
            public const string FIFTH = "Fifth";
            public const string SIXTH = "Sixth";
            public const string SEVENTH = "Seventh";
            public const string EIGHTH = "Eighth";
            public const string NINTH = "Ninth";
            public const string LAST = "Last";
        }
        #endregion

        #endregion
        [PXLocalizable]
        public static class RecordType_ServiceContract
        {
            public const string SERVICE_CONTRACT           = "Service Contract";
            public const string ROUTE_SERVICE_CONTRACT     = "Route Service Contract";
            public const string EMPLOYEE_SCHEDULE_CONTRACT = "Employee Schedule Contract";
        }

        [PXLocalizable]
        public static class ScheduleGenType_ServiceContract
        {
            public const string SERVICE_ORDER = "Service Orders";
            public const string APPOINTMENT   = "Appointments";
            public const string NONE = "None";
        }

        [PXLocalizable]
        public static class Behavior_SrvOrderType
        {
            public const string REGULAR_APPOINTMENT  = "Regular";
            public const string ROUTE_APPOINTMENT    = "Route";
            public const string INTERNAL_APPOINTMENT = "Internal";
            public const string QUOTE                = "Quote";
        }

        [PXLocalizable]
        public static class PreAcctSource_Setup
        {
            public const string CUSTOMER_LOCATION = "Customer Location";
            public const string INVENTORY_ITEM = "Inventory Item";
        }

        [PXLocalizable]
        public static class ContactType_ApptMail
        {
            public const string CUSTOMER        = "Customer";
            public const string STAFF_MEMBERS   = "Staff Members";

            public const string VENDOR          = "Vendor";
            public const string GEOZONE_STAFF   = "Service Area Staff";
            public const string SALESPERSON     = "Salesperson";
        }

        [PXLocalizable]
        public class EmployeeTimeSlotLevel
        {
            public const string BASE     = "Base";
            public const string COMPRESS = "Compressed";
        }

        [PXLocalizable]
        public static class Service_Action_Type
        {
            public const string NO_ITEMS_RELATED = "N/A";
            public const string PICKED_UP_ITEMS  = "Pick Up Items";
            public const string DELIVERED_ITEMS  = "Deliver Items";
        }

        [PXLocalizable]
        public static class Appointment_Service_Action_Type
        {
            public const string PICKED_UP_ITEMS     = "Picked Up";
            public const string DELIVERED_ITEMS     = "Delivered";
        }

        [PXLocalizable]
        public static class CalendarBoardAccess
        {
            public const string MULTI_EMP_CALENDAR  = "Schedule on the Calendar Board";
            public const string SINGLE_EMP_CALENDAR = "Schedule on the Staff Calendar Board";
            public const string ROOM_CALENDAR       = "Schedule on the Room Calendar Board";
        }

        [PXLocalizable]
        public static class ActionCalendarBoardAccess
        {
            public const string MULTI_EMP_CALENDAR  = "Schedule on the Calendar Board";
            public const string SINGLE_EMP_CALENDAR = "Schedule on the Staff Calendar Board";
        }

        [PXLocalizable]
        public static class AppointmentTotalTimesLabels
        {
            public const string ESTIMATED = "Estimated";
            public const string ACTUAL    = "Actual";
        }

        [PXLocalizable]
        public static class Billing_By
        {
            public const string APPOINTMENT     = "Appointments";
            public const string SERVICE_ORDER   = "Service Orders";

            //TODO: Remove in 2019R2
            public const string CONTRACT        = "Contracts";
        }

        [PXLocalizable]
        public static class Billing_Cycle_Type
        {
            public const string APPOINTMENT     = "Appointments";
            public const string SERVICE_ORDER   = "Service Orders";
            public const string TIME_FRAME      = "Time Frame";
            public const string PURCHASE_ORDER  = "Customer Order";
            public const string WORK_ORDER      = "External Reference";
        }

        [PXLocalizable]
        public static class Time_Cycle_Type
        {
            public const string WEEKDAY      = "Fixed Day of Week";
            public const string DAY_OF_MONTH = "Fixed Day of Month";
        }

        [PXLocalizable]
        public static class Frequency_Type
        {
            public const string WEEKLY  = "Weekly";
            public const string MONTHLY = "Monthly";
            public const string NONE    = "None";
        }

        [PXLocalizable]
        public static class Send_Invoices_To
        {
            public const string BILLING_CUSTOMER_BILL_TO          = "Billing Customer";
            public const string DEFAULT_BILLING_CUSTOMER_LOCATION = "Default Billing Customer Location";
            public const string SO_BILLING_CUSTOMER_LOCATION      = "Specific Billing Customer Location";
            public const string SERVICE_ORDER_ADDRESS             = "Service Order";
        }

        [PXLocalizable]
        public static class Ship_To
        {
            public const string BILLING_CUSTOMER_BILL_TO     = "Billing Customer";
            public const string SO_BILLING_CUSTOMER_LOCATION = "Specific Billing Customer Location";
            public const string SO_CUSTOMER_LOCATION         = "Specific Customer Location";
            public const string SERVICE_ORDER_ADDRESS        = "Service Order";
        }

        [PXLocalizable]
        public static class Default_Billing_Customer_Source
        {
            public const string SERVICE_ORDER_CUSTOMER = "Service Order Customer";
            public const string DEFAULT_CUSTOMER = "Default Customer";
            public const string SPECIFIC_CUSTOMER = "Specific Customer";
        }

        [PXLocalizable]
        public static class RouteLocationType
        {
            public const string BRANCH_LOCATION     = "BRLC";
            public const string EMPLOYEE_LOCATION   = "EMLC"; //For future use
        }
        
        [PXLocalizable]
        public static class Batch_PostTo
        {
            public const string AR_AP   = "AR Documents and/or AP Bills";
            public const string AR      = "Accounts Receivable";
            public const string SO      = "Sales Orders";
            public const string SI      = "SO Invoices";
            public const string AP      = "Accounts Payable";
            public const string IN      = "Inventory";
            public const string PM      = "Projects";
        }

        [PXLocalizable]
        public static class Route_Location
        {
            public const string START_LOCATION = "START LOCATION";
            public const string END_LOCATION = "END LOCATION";
        }

        [PXLocalizable]
        public static class RouteLocationInfo
        {
            public const string START_LOCATION          = "Start Location";
            public const string END_LOCATION            = "End Location";
            public const string APPOINTMENT_LOCATION    = "Appointment Location";
        }

        [PXLocalizable]
        public static class Status_Batch
        {
            public const string Temporary = "Temporary";
            public const string Completed = "Completed";
        }

		[PXLocalizable]
        public static class Equipment_Item_Class
        {
            public const string PART_OTHER_INVENTORY = "Part or Other Inventory";
            public const string MODEL_EQUIPMENT = "Model Equipment";
            public const string COMPONENT = "Component";
            public const string CONSUMABLE = "Consumable";
        }

        [PXLocalizable]
        public static class CloningType_CloneAppointment
        {
            public const string SINGLE      = "Single";
            public const string MULTIPLE    = "Multiple";
        }

        #region RecurrencyFrecuency
        public class RecurrencyFrecuency
        {
            public static string[] counters = { Counting.FIRST, Counting.SECOND, Counting.THIRD, Counting.FOURTH, Counting.LAST };
            public static string[] daysOfWeek = { WeekDays.SUNDAY, WeekDays.MONDAY, WeekDays.TUESDAY, WeekDays.WEDNESDAY, WeekDays.THURSDAY, WeekDays.FRIDAY, WeekDays.SATURDAY };
        }
        #endregion

        [PXLocalizable]
        public static class Equipment_Status
        {
            public const string ACTIVE      = "Active";
            public const string SUSPENDED   = "Suspended";
            public const string DISPOSED    = "Disposed";
        }

        [PXLocalizable]
        public static class Equipment_Action
        {
            public const string NONE                         = "N/A";
            public const string SELLING_TARGET_EQUIPMENT     = "Selling Model Equipment";
            public const string REPLACING_TARGET_EQUIPMENT   = "Replacing Target Equipment";
            public const string CREATING_COMPONENT           = "Selling Optional Component";
            public const string UPGRADING_COMPONENT          = "Upgrading Component";
            public const string REPLACING_COMPONENT          = "Replacing Component";
        }

        [PXLocalizable]
        public static class ServiceOrder_Action_Filter
        {
            public const string UNDEFINED     = "<SELECT>";
            public const string COMPLETE      = "Complete Order";
            public const string CANCEL        = "Cancel Order";
            public const string REOPEN        = "Reopen Order";
            public const string CLOSE         = "Close Order";
            public const string UNCLOSE       = "Unclose Order";
            public const string ALLOWINVOICE  = "Allow Billing";
        }

        public static class GenericInquiries_GUID
        {
            public const string EQUIPMENT_SUMMARY = "e850784d-9b5c-45f9-a7ca-085aa07cdcdb";
            public const string SERVICE_CLASSES = "1a963056-485f-42c8-81d9-2a06610128da";
            public const string LICENSES = "715aebe2-d1e7-4d45-9ba3-41a1251141e9";
            public const string SERVICE_ORDER_HISTORY = "84b92648-c42e-41e8-855c-4aa9144b9eda";
            public const string SERVICES = "cfde9093-83c9-49b3-a04b-6d08e3b75b10";
            public const string STAFF_SCHEDULE_RULES = "ae872579-713f-4b93-95ad-89d8dc51a7e6";
            public const string SERVICE_ORDER_DETAILS_HISTORY = "dd5375be-b8f0-43cb-870c-900371df5942";
            public const string CONTRACT_SUMMARY = "4c33d513-ef82-4b7a-aafc-913e856bf89c";
            public const string MODEL_EQUIPMENT_SUMMARY = "686044fa-e926-4282-8dd1-ac2d943dd33b";
            public const string CONTRACT_SCHEDULE_DETAILS_SUMMARY = "5566eca3-a20a-4d9c-8b1d-bb6b32ae6e9f";
            public const string CONTRACT_SCHEDULE_SUMMARY = "09c88688-263d-426a-a19e-de1d0c3d3ad3";
            public const string COMPONENT_SUMMARY = "8bc77dbd-a02e-41b1-89ef-e7e498b612ec";
            public const string APPOINTMENT_DETAILS_HISTORY = "6e27759d-b6ac-4105-ba3e-fa62a6cd0c67";
        }

        public static class FriendlyViewName
        {
            [PXLocalizable]
            public static class Appointment
            {
                public const string SERVICEORDER_RELATED        = "Service Order";
                public const string APPOINTMENT_SELECTED        = "Appointment";
                public const string APPOINTMENT_DET_SERVICES    = "Appointment Services";
                public const string APPOINTMENT_DET_PARTS       = "Appointment Parts";
                public const string APPOINTMENT_DETAILS         = "Appointment Details";
                public const string APPOINTMENT_RESOURCES       = "Appointment Resources";
                public const string APPOINTMENT_EMPLOYEES       = "Appointment Employees";
                public const string PICKUP_DELIVERY_ITEMS       = "Pickup Delivery Items";
                public const string APPOINTMENT_POSTED_IN       = "Appointment Posting Info";
            }

            [PXLocalizable]
            public static class ServiceOrder
            {
                public const string SERVICEORDER_RELATED        = "Service Order";
                public const string SERVICEORDER_DET_SERVICES   = "Service Order Services";
                public const string SERVICEORDER_DET_PARTS      = "Service Order Parts";
                public const string SERVICEORDER_DETAILS        = "Service Order Details";
                public const string SERVICEORDER_APPOINTMENTS   = "Appointments in Service Order";
                public const string SERVICEORDER_RESOURCES      = "Service Order Resources";
                public const string SERVICEORDER_EMPLOYEES      = "Service Order Employees";
                public const string SERVICEORDER_EQUIPMENT      = "Service Order Equipment";
                public const string SERVICEORDER_POSTED_IN      = "Service Order Posting Info";
            }

            [PXLocalizable]
            public static class Common
            {
                public const string SERVICEORDERTYPE_SELECTED = "Service Order Type";
            }
        }

        /// <summary>
        /// EntityType for FSAddress and FSContact tables
        /// </summary>
        [PXLocalizable]
        public class ACEntityType
        {
            public const string MANUFACTURER = "Manufacturer";
            public const string BRANCH_LOCATION = "Branch Location";
            public const string SERVICE_ORDER = "Service Order";
            public const string APPOINTMENT = "Appointment";
        }

        [PXLocalizable]
        public static class TimeRange_Setup
        {
            public const string DAY = "Day";
            public const string WEEK = "Week";
            public const string MONTH = "Month";
        }

        [PXLocalizable]
        public static class TimeFilter_Setup
        {
            public const string CLEARED_FILTER = "Cleared Filter";
            public const string WORKING_TIME = "Working Time";
            public const string WEEKDAYS = "Weekdays";
            public const string WORKING_TIME_WEEKDAYS = "Working Time and Weekdays";
            public const string BOOKED_DAYS = "Booked Days";
        }

        [PXLocalizable]
        public static class DayResolution_Setup
        {
            public const string D13 = "13";
            public const string D14 = "14";
            public const string D15 = "15";
            public const string D16 = "16";
            public const string D17 = "17";
            public const string D18 = "18";
            public const string D19 = "19";
        }

        [PXLocalizable]
        public static class WeekResolution_Setup
        {
            public const string W10 = "10";
            public const string W11 = "11";
            public const string W12 = "12";
            public const string W13 = "13";
            public const string W14 = "14";
            public const string W15 = "15";
            public const string W16 = "16";
            public const string W17 = "17";
            public const string W18 = "18";
            public const string W19 = "19";
        }

        [PXLocalizable]
        public static class MonthResolution_Setup
        {
            public const string M06 = "6";
            public const string M07 = "7";
            public const string M08 = "8";
            public const string M09 = "9";
            public const string M10 = "10";
            public const string M11 = "11";
            public const string M12 = "12";
            public const string M13 = "13";
            public const string M14 = "14";
            public const string M15 = "15";
            public const string M16 = "16";
            public const string M17 = "17";
            public const string M18 = "18";
            public const string M19 = "19";
        }

        #region CalendarMessages

        [PXLocalizable]
        public static class CalendarMessages
        {
            public const string LOADING_TEXT = "Loading, please wait...";

            /*Button Messages*/
            public const string BUTTON_FILTER_RESOURCE = "Filter Staff";
            public const string BUTTON_FILTER = "Filter";
            public const string BUTTON_CLOSE = "Close";
            public const string BUTTON_FILTER_CLOSE = "Filter & Close";
            public const string BUTTON_NEW_APPOINTMENT = "New Appointment";
            public const string BUTTON_OPEN_APPOINTMENT = "View";
            public const string BUTTON_EDIT_APPOINTMENT = "Edit";
            public const string BUTTON_DELETE_APPOINTMENT = "Delete";
            public const string BUTTON_CLONE_APPOINTMENT = "Clone";
            public const string BUTTON_CLEAR_VALIDATION = "Clear Validation";
            public const string BUTTON_CONFIRM_APPOINTMENT = "Confirm";
            public const string BUTTON_UNCONFIRM_APPOINTMENT = "Unconfirm";
            public const string BUTTON_VALIDATE_BY_DISPATCH = "Validate by Dispatcher";
            public const string BUTTON_CLEARED_FILTER = "Cleared Filter";
            public const string BUTTON_SHOW_ROUTE_INFO = "Show or Hide Info on the Map";
            public const string BUTTON_EXPAND_ALL_ROUTES = "Expand All Routes";
            public const string BUTTON_SHOW_ALL_APPOINTMENTS = "Show All Appointments";
            public const string BUTTON_SHOW_ALL_APPOINTMENTS_INFO = "Show All Appointments Info on the Map";
            public const string BUTTON_SHOW_ALL_DEVICES = "Show All Devices on the Map";
            public const string BUTTON_PREVIOUS_DAY = "Previous day";
            public const string BUTTON_PREVIOUS = "Previous";
            public const string BUTTON_NEXT_DAY = "Next day";
            public const string BUTTON_NEXT = "Next";
            public const string BUTTON_PREVIOUS_WEEK = "Previous week";
            public const string BUTTON_NEXT_WEEK = "Next week";
            public const string BUTTON_MERGE_RULES = "Merge Rules";
            public const string BUTTON_OK = "OK";
            public const string BUTTON_DELETE = "Delete";

            /*Empty Field Messages*/
            public const string EMPTY_EMPLOYEE = "Select a Staff";
            public const string EMPTY_BRANCH_LOCATION = "Select a Branch Location";
            public const string EMPTY_EMPLOYEE_NAME_FILTER = "Type Name";
            public const string EMPTY_SRVORDTYPE = "Select a Service Order Type";
            public const string EMPLOYEE_NOT_FOUND = "Staff not found";
            public const string BRANCH_NOT_FOUND = "Branch not found";
            public const string BRANCH_LOCATION_NOT_FOUND = "Branch Location not found";

            /*Alert Messages*/
            public const string ALERT_DELETE_APPOINTMENT = "Are you sure you want to delete this Appointment?";
            public const string ALERT_DELETE_TIMESLOT = "Are you sure you want to delete this Time Slot?";
            public const string ALERT_DELETE_SHARED = "This appointment is scheduled to more than one staff. Are you sure you want to remove it?";
            public const string ALERT_SLATIME_EXCEEDED = "The SLA assigned to this Service Order has already expired.";
            public const string ALERT_SETUP_NOT_DEFINED = "It seems that the Calendar Board preferences have not been set. Please go to the Service Management Preferences and check the Calendar Board options.";

            /*Error Messages*/
            public const string ERROR_CONNECTION_FAILURE = "Error connecting to the server, please try again.";
            
            /*Tooltip Warning Messages*/
            public const string WARNING_PHONE_NUMBER_MISSING = "Phone number missing.";
            public const string WARNING_APPOINTMENT_VALIDATED_BY_DISPATCHER = "This appointment is not validated by dispatcher";

            /*Note Messages*/
            public const string NOTE_GEOZONE_FILTER = "This specific tab filters by any of the selected options.";

            /*Warning Messages*/
            public const string WARNING_APPOINTMENT_VALID = "This appointment is shared between {0} staff members.";
            public const string WARNING_RESOURCE_AVAILABLE = "This appointment is scheduled out of the working time.";
            public const string WARNING_APPOINTMENT_OVERLAPPING = "There is more than one appointment scheduled at the same time.";
            public const string WARNING_AVAILABILITY_OVERLAPPING = "There are more than 1 rule affecting this availability.";
            

            /*Service Order Grid Columns*/
            public const string SERVICEORDER_REFNBR = "Service Order";
            public const string SERVICEORDER_SRVORDTYPE = "Service Order Type";
            public const string SERVICEORDER_CUSTOMER = "Customer";
            public const string SERVICEORDER_ORDER_DATE = "Date";
            public const string SERVICEORDER_SALES_ORDER = "Sales Order";
            public const string SERVICEORDER_CASES = "Case";
            public const string SERVICEORDER_STAGE = "Stage";
            public const string SERVICEORDER_PRIORITY = "Priority";
            public const string SERVICEORDER_SEVERITY = "Severity";
            public const string SERVICEORDER_ASSIGNED_EMPLOYEEID = "Assigned Staff ID";
            public const string SERVICEORDER_SLA = "SLA";
            public const string SERVICEORDER_SERVICES_REMANING = "Service Remaning";
            public const string SERVICEORDER_SERVICES_COUNT = "Quantity";
            public const string SERVICEORDER_SOURCE_TYPE = "Source Type";
            public const string SERVICEORDER_ESTIMATEDTIME = "Estimated Duration";
            public const string SERVICEORDER_SOURCEREF = "Source Ref.";
            public const string SERVICEORDER_SUPERVISOR = "Supervisor";
            public const string SERVICEORDER_TIMEEXCEEDED = "Time Exceeded";
            public const string SERVICEORDER_DATE = "Date";
            public const string SERVICEORDER_TIMESTART = "Start Time";
            public const string SERVICEORDER_TIMEEND = "End Time";
            public const string SERVICEORDER_REPORTSTO = "Reports To"; // Revisar
            public const string SERVICEORDER_PRIORITY_LOW = "Low";
            public const string SERVICEORDER_PRIORITY_MEDIUM = "Medium";
            public const string SERVICEORDER_PRIORITY_HIGH = "High";
            public const string SERVICEORDER_ORDERDATE = "Creation Date";

            /*Service Order Grid Columns*/
            public const string APPOINTMENT_ACTUAL_START_TIME = "Actual Start Time";
            public const string APPOINTMENT_ACTUAL_END_TIME = "Actual End Time";

            /*Titles*/
            public const string TITLE_DASHBOARD = "Dashboard";
            public const string TITLE_INACTIVE_PREFERENCES = "Inactive Fields";
            public const string TITLE_ACTIVE_PREFERENCES = "Active Fields";
            public const string TITLE_PREVIEW_PREFERENCES = "Preview";
            public const string TITLE_STATUS_PREFERENCES = "Display Preferences";
            public const string TITLE_ROUTE_INFO = "Route Info";
            public const string TITLE_ROUTE_TREE = "Routes";
            public const string TITLE_ROUTE_INFORMATION = "Route Information";
            public const string TITLE_ROUTE_LIST = "Route List";
            public const string TITLE_APPOINTMENT = "Appointments";
            public const string TITLE_SERVICEORDER = "Service Orders";
            public const string TITLE_UNASSIGNED_APPOINTMENT = "Unassigned Appointments";
            public const string TITLE_PROBLEMS = "Problems";
            public const string TITLE_SKILLS = "Skills";
            public const string TITLE_SERVICESTYPE = "Service Classes";
            public const string TITLE_ASSIGNEDEMPLOYEE = "Assigned Staff";
            public const string TITLE_SERVICES = "Services";
            public const string TITLE_GEOZONES = "Service Areas";
            public const string TITLE_STAFF = "Staff";
            public const string TITLE_ROOMS = "Rooms";
            public const string TITLE_LICENSESTYPE = "License Types";
            public const string TITLE_STAFF_FILTERS = "Staff Filters";
            public const string TITLE_SERVICEORDER_FILTERS = "Service Order Filters";
            public const string TITLE_UNASSIGNED_APPOINTMENT_FILTERS = "Unassigned Appointment Filters";
            public const string TITLE_STAFF_CALENDARS = "Staff Calendar";
            public const string TITLE_APPOINTMENT_INFO = "Appointment Information";
            public const string TITLE_APPOINTMENT_FOR = "Appointments for ";
            public const string TITLE_ROUTE_FOR = "Route for ";
            public const string TITLE_CREATE_APPOINTMENT = "Create Appointment";

            /*Preference Grid column*/
            public const string PREFERENCE_STATUS = "Status";
            public const string PREFERENCE_BACKGROUND_COLOR = "Background Color";
            public const string PREFERENCE_TEXT_COLOR = "Text Color";
            public const string PREFERENCE_HIDE = "Hide";
            public const string PREFERENCE_NAME = "Name";

            /*Misc.*/
            public const string ERROR = "Error";
            public const string WARNING = "Warning";
            public const string DELETE = "Delete";
            public const string ALERT = "Alert";
            public const string DAY = "Day";
            public const string WEEK = "Week";
            public const string MONTH = "Month";
            public const string YEAR = "Year";
            public const string AGENDA = "Agenda";
            public const string CALENDAR = "Calendar";
            public const string MAP = "Map";
            public const string ALL = "All";
            public const string NUMBER = "Number";

            /*Filters*/
            public const string LABEL_BRANCH_LOCATION = "Branch Location";
            public const string LABEL_BRANCH = "Branch";
            public const string LABEL_SRVORDTYPE = "Service Order Type";
            public const string LABEL_SEARCH = "Search";
            public const string LABEL_ASSIGNED_EMPLOYEE = "Assigned Staff";
            public const string LABEL_SUPERVISOR = "Supervisor";

            public const string STAFF_NAME_FILTER = "Name Filter";
            public const string FILTER_WORKING_TIME = "Working Time";
            public const string FILTER_WEEKDAYS = "Weekdays";
            public const string FILTER_WORKING_TIME_WEEKDAYS = "Working Time and Weekdays";
            public const string FILTER_BOOKED_DAYS = "Booked Days";

            /* Paging Toolbar Controller*/
            public const string PAGINGTOOLBAR_BEFORE_PAGE_TEXT = "Page";
            public const string PAGINGTOOLBAR_AFTER_PAGE_TEXT = "of {0}";
            public const string PAGINGTOOLBAR_FIRST_TEXT = "First Page";
            public const string PAGINGTOOLBAR_PREV_TEXT = "Previous Page";
            public const string PAGINGTOOLBAR_NEXT_TEXT = "Next Page";
            public const string PAGINGTOOLBAR_LAST_TEXT = "Last Page";
            public const string PAGINGTOOLBAR_REFRESH_TEXT = "Refresh";
            public const string PAGINGTOOLBAR_DISPLAY_MSG = "Displaying {0} - {1} of {2}";
            public const string PAGINGTOOLBAR_EMPTY_MSG = "No data to display";

            /* Date Picker Controller*/
            public const string DATEPICKER_TODAY_TEXT = "Today";
            public const string DATEPICKER_NEXT_TEXT = "Next Month (Control+Right)";
            public const string DATEPICKER_PREV_TEXT = "Previous Month (Control+Left)";
            public const string DATEPICKER_MONTH_YEAR_TEXT = "Choose a month (Control+Up/Down to move years)";
            public const string DATEPICKER_TODAY_TIP = "{0} (Spacebar)";
            public const string DATEPICKER_OK_TEXT = "Ok";
            public const string DATEPICKER_CANCEL_TEXT = "Cancel";

            /*PANEL*/
            public const string PANEL_COLLAPSE_TOOL_TEXT = "Collapse panel";
            public const string PANEL_EXPAND_TOOL_TEXT = "Expand panel";

            /*Route Grid Columns*/
            public const string ROUTEGRID_POSTAL_CODE = "Postal Code";
            public const string ROUTEGRID_LOCATION = "Location";
            public const string ROUTEGRID_TRAVEL_TIME = "Travel Time";
            public const string ROUTEGRID_ADDRESS = "Address";
            public const string ROUTEGRID_CUSTOMER_ROUTE = "Route/Customer";
            public const string ROUTEGRID_SERVICE_TYPE = "Service Type";
            public const string ROUTEGRID_SERVICES_DURATION = "Service Duration";

            /*Grid Columns Controller*/
            public const string COLUMNGRID_SORT_ASC_TEXT = "Sort Ascending";
            public const string COLUMNGRID_SORT_DESC_TEXT = "Sort Descending";
            public const string COLUMNGRID_COLUMNS_TEXT = "Columns";

            /*ToolTip Messages*/
            public const string TOOLTIP_APPOINTMENT_INFO = "Open Appointment Information";
            public const string TOOLTIP_SHOW_ROUTE_INFO_WINDOW = "Show or Hide All Information Tooltips of this Route on the Map.";
            public const string TOOLTIP_SHOW_ROUTE_DEVICE = "Show or Hide GPS Location on the Map";
            public const string TOOLTIP_SHOW_LOCATION_TRACKING = "Show Location Tracking on the Map";
            public const string TOOLTIP_SHOW_SUGGESTED_ROUTE = "Show Suggested Route on Map";
            public const string TOOLTIP_CLOSE_TOOL_TEXT = "Close dialog";

            /*INFO Messages*/
            public const string INFO_CALCULATE = "(Calculate)";
            public const string INFO_MAP_API_ERROR = "This route cannot be traced with the provided data. Please revise the appointments addresses, some are not recognized by the system.";
            public const string INFO_PREFIX_APPOINTMENT_CREATION = "Appointment";
            public const string INFO_SUFFIX_APPOINTMENT_CREATION = "has been successfully created.";
            public const string INFO_PREFIX_APPOINTMENT_ASSIGN = "Appointment";
            public const string INFO_SUFFIX_APPOINTMENT_ASSIGN = "has been successfully assigned.";

            /*Appointment Calendar Box*/
            public const string TOOLTIP_BOX_SERVICE_ORDER_INFO = "Service Order Info";
            public const string TOOLTIP_BOX_SERVICE_ORDER_ID = "Service Order ID";
            public const string TOOLTIP_BOX_SERVICE_ORDER_TYPE = "Service Order Type";
            public const string TOOLTIP_BOX_DESCRIPTION = "Description";
            public const string TOOLTIP_BOX_CUSTOMER = "Customer";
            public const string TOOLTIP_BOX_CONTACT = "Contact";
            public const string TOOLTIP_BOX_PHONE = "Phone";
            public const string TOOLTIP_BOX_DETAILS = "Details";
            public const string TOOLTIP_BOX_BRANCH_LOCATION = "Branch Location";
            public const string TOOLTIP_BOX_ROOM = "Room";
            public const string TOOLTIP_BOX_STATUS = "Status";
            public const string TOOLTIP_BOX_CONFIRMATION = "Confirmation";
            public const string TOOLTIP_BOX_START = "Start";
            public const string TOOLTIP_BOX_END = "End";
            public const string TOOLTIP_BOX_SERVICES = "Services";
            public const string TOOLTIP_BOX_NONE = "None";
            public const string TOOLTIP_BOX_ITEM_CLASS = "Item Class";
            public const string TOOLTIP_BOX_DURATION = "Duration";
            public const string TOOLTIP_BOX_STAFF = "Staff";
            public const string TOOLTIP_BOX_WARNINGS = "Warnings";
            public const string TOOLTIP_BOX_RESOURCES = "Resources";
            public const string TOOLTIP_BOX_CONFIRMED = "Confirmed";
            public const string TOOLTIP_BOX_NOT_CONFIRMED = "Not Confirmed";
            public const string TOOLTIP_BOX_EMAIL = "Email";
            public const string TOOLTIP_BOX_PRIORITY = "Priority";

            public const string APPOINTMENT_STATUS_AUTOMATIC_SCHEDULED = "Scheduled by System";
            public const string APPOINTMENT_STATUS_MANUAL_SCHEDULED = "Not Started";
            public const string APPOINTMENT_STATUS_IN_PROCESS = "In Process";
            public const string APPOINTMENT_STATUS_CANCELED = "Canceled";
            public const string APPOINTMENT_STATUS_COMPLETED = "Completed";
            public const string APPOINTMENT_STATUS_CLOSED = "Closed";
            public const string APPOINTMENT_STATUS_ON_HOLD = "On Hold";
            public const string APPOINTMENT_STATUS_SCHEDULED = "Scheduled";

            public const string APPOINTMENT_BOX_CUSTOMER = "Customer";
            public const string APPOINTMENT_BOX_CONTACT = "Contact";
            public const string APPOINTMENT_BOX_CUSTOMER_NAME = "Customer Name";
            public const string APPOINTMENT_BOX_CUSTOMER_LOCATION = "Customer Location";
            public const string APPOINTMENT_BOX_FIRST_SERVICE = "First Service";
            public const string APPOINTMENT_BOX_ROOM = "Room";
            public const string APPOINTMENT_BOX_NUMBER_ATTENDEES = "Number of attendees";
            public const string APPOINTMENT_SERVICE_ORDER = "Service Order";
            public const string APPOINTMENT_BOX_CONFIRMATION = "Confirmation";
            public const string APPOINTMENT_BOX_STATUS = "Status";
            public const string APPOINTMENT_BOX_WORKFLOW_STAGE = "Workflow Stage";
            public const string APPOINTMENT_BOX_ATTENDEES = "Attendee(s)";

            public const string ROUTE_BOX_ROUTE_TIME = "Route Time";
            public const string ROUTE_BOX_ROUTE_DISTANCE = "Route Distance";
            public const string ROUTE_BOX_MILES = "miles";
            public const string ROUTE_BOX_NUMBER_APPOINTMENTS = "Number of Appointments";
            public const string ROUTE_BOX_DRIVER = "Driver";
            public const string ROUTE_BOX_VEHICLE_TYPE = "Vehicle Type";
            public const string ROUTE_BOX_VEHICLE = "Vehicle";
            public const string ROUTE_BOX_STATUS = "Status";

            public const string AGENDA_HEADER_DATE = "Date";
            public const string AGENDA_HEADER_TIME = "Time";
            public const string AGENDA_HEADER_EVENT = "Event";
            public const string EVENTLIST_NO_EVENTS = "No Appointments";
            public const string EVENTLIST_MORE = "+{0} More";
        }

        #endregion

        [PXLocalizable]
        public static class Status_ROOptimization
        {
            public const string NOT_OPTIMIZED    = "Has Not Been Optimized";
            public const string OPTIMIZED        = "Optimized";
            public const string NOT_ABLE         = "Could Not Be Optimized";
            public const string ADDRESS_ERROR    = "Encountered Address Error";
        }

        [PXLocalizable]
        public class Type_ROOptimization
        {
            public const string ASSIGNED_APP   = "Assigned Appointments";
            public const string UNASSIGNED_APP = "Unassigned Appointments";
        }
    }
}

