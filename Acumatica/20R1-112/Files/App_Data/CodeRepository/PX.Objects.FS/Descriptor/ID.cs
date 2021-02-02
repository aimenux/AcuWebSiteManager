using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public static class ID
    {        
        //FSEquipment - LocationType
        public class LocationType
        {
            public const string COMPANY     = "CO";
            public const string CUSTOMER    = "CU";

            public readonly string[] ID_LIST = { ID.LocationType.COMPANY, ID.LocationType.CUSTOMER };
            public readonly string[] TX_LIST = { TX.LocationType.COMPANY, TX.LocationType.CUSTOMER };
        }

        //FSEquipment - Condition
        public class Condition
        {
            public const string NEW     = "N";
            public const string USED    = "U";

            public readonly string[] ID_LIST = { ID.Condition.NEW, ID.Condition.USED };
            public readonly string[] TX_LIST = { TX.Condition.NEW, TX.Condition.USED };
        }

        //FSLicense - OwnerType
        public class OwnerType
        {
            public const string BUSINESS    = "B";
            public const string EMPLOYEE    = "E";

            public readonly string[] ID_LIST = { ID.OwnerType.BUSINESS, ID.OwnerType.EMPLOYEE };
            public readonly string[] TX_LIST = { TX.OwnerType.BUSINESS, TX.OwnerType.EMPLOYEE };
        }

        //FSServiceCode - Charges To Apply
        public class BillingRule
        {
            public const string TIME                    = "TIME";
            public const string FLAT_RATE               = "FLRA";
            public const string NONE                    = "NONE";

            public readonly string[] ID_LIST = { ID.BillingRule.TIME, ID.BillingRule.FLAT_RATE, ID.BillingRule.NONE };
            public readonly string[] TX_LIST = { TX.BillingRule.TIME, TX.BillingRule.FLAT_RATE, TX.BillingRule.NONE };
        }

        public class ContractPeriod_BillingRule
        {
            public const string TIME      = "TIME";
            public const string FLAT_RATE = "FLRA";

            public readonly string[] ID_LIST = { ID.BillingRule.TIME, ID.BillingRule.FLAT_RATE };
            public readonly string[] TX_LIST = { TX.BillingRule.TIME, TX.BillingRule.FLAT_RATE };
        }

        public class ContractPeriod_Actions
        {
            public const string SEARCH_BILLING_PERIOD           = "SBP";
            public const string MODIFY_UPCOMING_BILLING_PERIOD  = "MBP";

            public readonly string[] ID_LIST = { ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD, ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD };
            public readonly string[] TX_LIST = { TX.ContractPeriod_Actions.SEARCH_BILLING_PERIOD, TX.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD };
        }

        //FSEmployeeSchedule - ScheduleType
        public class ScheduleType
        {
            public const string AVAILABILITY   = "A";
            public const string UNAVAILABILITY = "U";
            public const string BUSY           = "B";

            public readonly string[] ID_LIST = { ID.ScheduleType.AVAILABILITY, ID.ScheduleType.UNAVAILABILITY, ID.ScheduleType.BUSY };
            public readonly string[] TX_LIST = { TX.ScheduleType.AVAILABILITY, TX.ScheduleType.UNAVAILABILITY, TX.ScheduleType.BUSY };
        }

        //FSSchedule - Frequency Type
        public class Schedule_FrequencyType
        {
            public const string DAILY   = "D";
            public const string WEEKLY  = "W";
            public const string MONTHLY = "M";
            public const string ANNUAL  = "A";

            public readonly string[] ID_LIST = { ID.Schedule_FrequencyType.DAILY, ID.Schedule_FrequencyType.WEEKLY, ID.Schedule_FrequencyType.MONTHLY, ID.Schedule_FrequencyType.ANNUAL };
            public readonly string[] TX_LIST = { TX.Schedule_FrequencyType.DAILY, TX.Schedule_FrequencyType.WEEKLY, TX.Schedule_FrequencyType.MONTHLY, TX.Schedule_FrequencyType.ANNUAL };
        }

        //FSSchedule - Entity Type
        public class Schedule_EntityType
        {
            public const string CONTRACT = "C";
            public const string EMPLOYEE = "E";

            public readonly string[] ID_LIST = { ID.Schedule_EntityType.CONTRACT, ID.Schedule_EntityType.EMPLOYEE };
            public readonly string[] TX_LIST = { TX.Schedule_EntityType.CONTRACT, TX.Schedule_EntityType.EMPLOYEE };
        }

        //FSPostDoc - Entity Type
        public class PostDoc_EntityType
        {
            public const string APPOINTMENT     = "AP";
            public const string SERVICE_ORDER   = "SO";

            public readonly string[] ID_LIST = { ID.PostDoc_EntityType.APPOINTMENT, ID.PostDoc_EntityType.SERVICE_ORDER };
            public readonly string[] TX_LIST = { TX.PostDoc_EntityType.APPOINTMENT, TX.PostDoc_EntityType.SERVICE_ORDER };
        }
        
        public class PostRegister_Type
        {
            public const string INVOICE_POST = "INVCP";
        }

        //FSSetup - TimeConstants        
        public class TimeConstants
        {
            public const int HOURS_12   = 720;
            public const int MINUTES_0 = 0;
            public const int MINUTES_10 = 10;
            public const int MINUTES_15 = 15;
            public const int MINUTES_30 = 30;  
            public const int MINUTES_60 = 60;
        }

        public class WeekDays
        {
            public const string ANYDAY       = "NT";
            public const string SUNDAY       = "SU";
            public const string MONDAY       = "MO";
            public const string TUESDAY      = "TU";
            public const string WEDNESDAY    = "WE";
            public const string THURSDAY     = "TH";
            public const string FRIDAY       = "FR";
            public const string SATURDAY     = "SA";
            
            public readonly string[] ID_LIST = { ID.WeekDays.ANYDAY, ID.WeekDays.SUNDAY, ID.WeekDays.MONDAY, ID.WeekDays.TUESDAY, ID.WeekDays.WEDNESDAY, ID.WeekDays.THURSDAY, ID.WeekDays.FRIDAY, ID.WeekDays.SATURDAY };
            public readonly string[] TX_LIST = { TX.WeekDays.ANYDAY, TX.WeekDays.SUNDAY, TX.WeekDays.MONDAY, TX.WeekDays.TUESDAY, TX.WeekDays.WEDNESDAY, TX.WeekDays.THURSDAY, TX.WeekDays.FRIDAY, TX.WeekDays.SATURDAY };
        }

        public class WeekDaysNumber
        {
            public const int SUNDAY    = 0;
            public const int MONDAY    = 1;
            public const int TUESDAY   = 2;
            public const int WEDNESDAY = 3;
            public const int THURSDAY  = 4;
            public const int FRIDAY    = 5;
            public const int SATURDAY  = 6;

            public readonly int[] ID_LIST = { ID.WeekDaysNumber.SUNDAY, ID.WeekDaysNumber.MONDAY, ID.WeekDaysNumber.TUESDAY, ID.WeekDaysNumber.WEDNESDAY, ID.WeekDaysNumber.THURSDAY, ID.WeekDaysNumber.FRIDAY, ID.WeekDaysNumber.SATURDAY };
            public readonly string[] TX_LIST = { TX.WeekDays.SUNDAY, TX.WeekDays.MONDAY, TX.WeekDays.TUESDAY, TX.WeekDays.WEDNESDAY, TX.WeekDays.THURSDAY, TX.WeekDays.FRIDAY, TX.WeekDays.SATURDAY };
        }

        public class Months
        {
            public const string JANUARY   = "JAN";
            public const string FEBRUARY  = "FEB";
            public const string MARCH     = "MAR";
            public const string APRIL     = "APR";
            public const string MAY       = "MAY";
            public const string JUNE      = "JUN";
            public const string JULY      = "JUL";
            public const string AUGUST    = "AUG";
            public const string SEPTEMBER = "SEP";
            public const string OCTOBER   = "OCT";
            public const string NOVEMBER  = "NOV";
            public const string DECEMBER  = "DEC";

            public readonly string[] ID_LIST =
            { 
                ID.Months.JANUARY, 
                ID.Months.FEBRUARY, 
                ID.Months.MARCH, 
                ID.Months.APRIL, 
                ID.Months.MAY, 
                ID.Months.JUNE, 
                ID.Months.JULY, 
                ID.Months.AUGUST,
                ID.Months.SEPTEMBER, 
                ID.Months.OCTOBER, 
                ID.Months.NOVEMBER, 
                ID.Months.DECEMBER
            };

            public readonly string[] TX_LIST =
            {
                TX.Months.JANUARY,
                TX.Months.FEBRUARY,
                TX.Months.MARCH,
                TX.Months.APRIL,
                TX.Months.MAY,
                TX.Months.JUNE,
                TX.Months.JULY,
                TX.Months.AUGUST,
                TX.Months.SEPTEMBER,
                TX.Months.OCTOBER,
                TX.Months.NOVEMBER,
                TX.Months.DECEMBER
            };
        }

        public class SourceType_ServiceOrder
        {
            public const string CASE             = "CR";
            public const string OPPORTUNITY      = "OP";
            public const string SALES_ORDER      = "SO";
            public const string SERVICE_DISPATCH = "SD";

            public readonly string[] ID_LIST = { ID.SourceType_ServiceOrder.CASE, ID.SourceType_ServiceOrder.OPPORTUNITY, ID.SourceType_ServiceOrder.SALES_ORDER, ID.SourceType_ServiceOrder.SERVICE_DISPATCH };
            public readonly string[] TX_LIST = { PX.Objects.CR.Messages.Case, PX.Objects.CR.Messages.Opportunities, PX.Objects.IN.Messages.qadSOOrder, PX.Objects.IN.Messages.qadFSServiceOrder };            
        }

        public class AppResizePrecision_Setup
        {
            public readonly int[] ID_LIST = { ID.TimeConstants.MINUTES_10, ID.TimeConstants.MINUTES_15, ID.TimeConstants.MINUTES_30, ID.TimeConstants.MINUTES_60 };
            public readonly string[] TX_LIST = { TX.AppResizePrecision_Setup.MINUTES_10, TX.AppResizePrecision_Setup.MINUTES_15, TX.AppResizePrecision_Setup.MINUTES_30, TX.AppResizePrecision_Setup.MINUTES_60 };
        }

        public class DfltCalendarViewMode_Setup
        {
            public const string VERTICAL = "VE";
            public const string HORIZONTAL = "HO";

            public readonly string[] ID_LIST = { ID.DfltCalendarViewMode_Setup.VERTICAL, ID.DfltCalendarViewMode_Setup.HORIZONTAL };
            public readonly string[] TX_LIST = { TX.DfltCalendarViewMode_Setup.VERTICAL, TX.DfltCalendarViewMode_Setup.HORIZONTAL };
        }

        #region Priority+Severity
        public abstract class Priority_ALL
        {
            public const string LOW     = "L";
            public const string MEDIUM  = "M";
            public const string HIGH    = "H";

            public readonly string[] ID_LIST = { ID.Priority_ALL.LOW, ID.Priority_ALL.MEDIUM, ID.Priority_ALL.HIGH };
            public readonly string[] TX_LIST = { TX.Priority_ALL.LOW, TX.Priority_ALL.MEDIUM, TX.Priority_ALL.HIGH };
        }

        public class Priority_ServiceOrder : Priority_ALL
        {
        }

        public class Severity_ServiceOrder : Priority_ALL
        {
        }
        #endregion

        #region LineType

        public class LineType_ALL
        {
            public const string SERVICE             = "SERVI";
            public const string INVENTORY_ITEM      = "SLPRO";
            public const string SERVICE_TEMPLATE    = "TEMPL";
            public const string NONSTOCKITEM        = "NSTKI";
            public const string PICKUP_DELIVERY     = "PU_DL";
            public const string LABOR_ITEM          = "LABOR";
            public const string COMMENT             = "CM_LN";
            public const string INSTRUCTION         = "IT_LN";

            public readonly string[] ID_LIST_ALL = 
            { 
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.SERVICE_TEMPLATE,
                ID.LineType_ALL.INVENTORY_ITEM,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.COMMENT,
                ID.LineType_ALL.INSTRUCTION
            };

            public readonly string[] TX_LIST_ALL = 
            { 
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.SERVICE_TEMPLATE,
                TX.LineType_ALL.INVENTORY_ITEM,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.COMMENT,
                TX.LineType_ALL.INSTRUCTION
            };
        }

        public class LineType_AppSrvOrd : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL =
            {
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.INVENTORY_ITEM,
                ID.LineType_ALL.COMMENT,
                ID.LineType_ALL.INSTRUCTION,
            };

            public new readonly string[] TX_LIST_ALL =
            {
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.INVENTORY_ITEM,
                TX.LineType_ALL.COMMENT,
                TX.LineType_ALL.INSTRUCTION,
            };
        }

        public class LineType_ServiceTemplate : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL =
            {
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.INVENTORY_ITEM,
                ID.LineType_ALL.COMMENT,
                ID.LineType_ALL.INSTRUCTION,
            };

            public new readonly string[] TX_LIST_ALL =
            {
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.INVENTORY_ITEM,
                TX.LineType_ALL.COMMENT,
                TX.LineType_ALL.INSTRUCTION,
            };
        }

        public class LineType_Schedule : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL =
            {
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.SERVICE_TEMPLATE,
                ID.LineType_ALL.INVENTORY_ITEM,
                ID.LineType_ALL.COMMENT,
                ID.LineType_ALL.INSTRUCTION,
            };

            public new readonly string[] TX_LIST_ALL =
            {
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.SERVICE_TEMPLATE,
                TX.LineType_ALL.INVENTORY_ITEM,
                TX.LineType_ALL.COMMENT,
                TX.LineType_ALL.INSTRUCTION,
            };
        }

        public class LineType_Pickup_Delivery : LineType_ALL
        {
            public readonly string[] ID_LIST_SERVICE =
            {
                ID.LineType_ALL.PICKUP_DELIVERY
            };

            public readonly string[] TX_LIST_SERVICE =
            {
                TX.LineType_ALL.PICKUP_DELIVERY
            };
        }

        public class LineType_ServiceContract : LineType_ALL
        {
            public readonly string[] ID_LIST = 
            { 
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.COMMENT, 
                ID.LineType_ALL.INSTRUCTION,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.SERVICE_TEMPLATE 
            };

            public readonly string[] TX_LIST = 
            { 
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.COMMENT, 
                TX.LineType_ALL.INSTRUCTION,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.SERVICE_TEMPLATE 
            };            
        }

        public class LineType_SalesPrice : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL = 
            { 
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.INVENTORY_ITEM, 
                ID.LineType_ALL.NONSTOCKITEM
            };

            public new readonly string[] TX_LIST_ALL = 
            { 
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.INVENTORY_ITEM, 
                TX.LineType_ALL.NONSTOCKITEM
            };
        }

        public class LineType_ContractPeriod : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL =
            {
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.NONSTOCKITEM
            };

            public new readonly string[] TX_LIST_ALL =
            {
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.NONSTOCKITEM
            };
        }

        public class LineType_Profitability : LineType_ALL
        {
            public new readonly string[] ID_LIST_ALL =
            {
                ID.LineType_ALL.SERVICE,
                ID.LineType_ALL.NONSTOCKITEM,
                ID.LineType_ALL.INVENTORY_ITEM,
                ID.LineType_ALL.LABOR_ITEM
            };

            public new readonly string[] TX_LIST_ALL =
            {
                TX.LineType_ALL.SERVICE,
                TX.LineType_ALL.NONSTOCKITEM,
                TX.LineType_ALL.INVENTORY_ITEM,
                TX.LineType_ALL.LABOR_ITEM
            };
        }

        #endregion

        #region PriceType
        public class PriceType
        {
            public const string CONTRACT    = "CONTR";
            public const string CUSTOMER    = "CUSTM";
            public const string PRICE_CLASS = "PRCLS";
            public const string BASE        = "BASEP";
            public const string DEFAULT     = "DEFLT";

            public readonly string[] ID_LIST_PRICETYPE = { ID.PriceType.CONTRACT, ID.PriceType.CUSTOMER, ID.PriceType.PRICE_CLASS, ID.PriceType.BASE, ID.PriceType.DEFAULT };
            public readonly string[] TX_LIST_PRICETYPE = { TX.PriceType.CONTRACT, TX.PriceType.CUSTOMER, TX.PriceType.PRICE_CLASS, TX.PriceType.BASE, TX.PriceType.DEFAULT };
        }
        #endregion

        #region Status
        public class Status_SODet
        {
            public const string SCHEDULED_NEEDED    = "SN";
            public const string SCHEDULED           = "SC";
            public const string CANCELED            = "CC";
            public const string COMPLETED           = "CP";

            public static bool CanBeScheduled(string serviceStatus)
            {
                if (serviceStatus == Status_SODet.SCHEDULED_NEEDED)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public class Status_AppointmentDet
        {
            public const string NOT_STARTED     = "NS";
            public const string IN_PROCESS      = "IP";
            public const string NOT_FINISHED    = "NF";
            public const string NOT_PERFORMED   = "NP";
            public const string CANCELED        = Status_SODet.CANCELED;
            public const string COMPLETED       = Status_SODet.COMPLETED;
        }

        public class Status_Appointment
        {
            public const string AUTOMATIC_SCHEDULED  = "A";
            public const string MANUAL_SCHEDULED     = "S";
            public const string IN_PROCESS           = "P";
            public const string CANCELED             = "X";
            public const string COMPLETED            = "C";
            public const string CLOSED               = "Z";
            public const string ON_HOLD              = "H";

            public readonly string[] ID_LIST = 
            {
                ID.Status_Appointment.AUTOMATIC_SCHEDULED, ID.Status_Appointment.MANUAL_SCHEDULED,
                    ID.Status_Appointment.IN_PROCESS, ID.Status_Appointment.CANCELED,
                        ID.Status_Appointment.COMPLETED, ID.Status_Appointment.CLOSED,
                            ID.Status_Appointment.ON_HOLD
            };

            public readonly string[] TX_LIST = 
            {
                TX.Status_Appointment.AUTOMATIC_SCHEDULED, TX.Status_Appointment.MANUAL_SCHEDULED,
                    TX.Status_Appointment.IN_PROCESS, TX.Status_Appointment.CANCELED,
                        TX.Status_Appointment.COMPLETED, TX.Status_Appointment.CLOSED,
                            TX.Status_Appointment.ON_HOLD
            };

            public static bool IsOpen(string appointmentStatus)
            {
                if (appointmentStatus == Status_Appointment.AUTOMATIC_SCHEDULED || 
                        appointmentStatus == Status_Appointment.MANUAL_SCHEDULED || 
                        appointmentStatus == Status_Appointment.IN_PROCESS)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
             
        public class Status_ServiceOrder
        {
            public const string OPEN               = "O";
            public const string QUOTE              = "Q";
            public const string ON_HOLD            = "H";
            public const string CANCELED           = "X";
            public const string CLOSED             = "Z";
            public const string COMPLETED          = "C";

            public readonly string[] ID_LIST = { ID.Status_ServiceOrder.OPEN, ID.Status_ServiceOrder.QUOTE, ID.Status_ServiceOrder.ON_HOLD, ID.Status_ServiceOrder.CANCELED, ID.Status_ServiceOrder.CLOSED, ID.Status_ServiceOrder.COMPLETED };
            public readonly string[] TX_LIST = { TX.Status_ServiceOrder.OPEN, TX.Status_ServiceOrder.QUOTE, TX.Status_ServiceOrder.ON_HOLD, TX.Status_ServiceOrder.CANCELED, TX.Status_ServiceOrder.CLOSED, TX.Status_ServiceOrder.COMPLETED };
        }

        public class Status_ServiceContract
        {
            public const string DRAFT       = "D";
            public const string ACTIVE      = "A";
            public const string SUSPENDED   = "S";
            public const string CANCELED    = "X";
            public const string EXPIRED     = "E";

            public readonly string[] ID_LIST = { ID.Status_ServiceContract.DRAFT, ID.Status_ServiceContract.ACTIVE, ID.Status_ServiceContract.SUSPENDED, ID.Status_ServiceContract.CANCELED, ID.Status_ServiceContract.EXPIRED };
            public readonly string[] TX_LIST = { TX.Status_ServiceContract.DRAFT, TX.Status_ServiceContract.ACTIVE, TX.Status_ServiceContract.SUSPENDED, TX.Status_ServiceContract.CANCELED, TX.Status_ServiceContract.EXPIRED };
        }

        public class Status_ContractPeriod
        {
            public const string ACTIVE   = "A";
            public const string PENDING  = "P";
            public const string INACTIVE = "I";
            public const string INVOICED = "N";

            public readonly string[] ID_LIST = { ID.Status_ContractPeriod.ACTIVE, ID.Status_ContractPeriod.PENDING, ID.Status_ContractPeriod.INACTIVE, ID.Status_ContractPeriod.INVOICED };
            public readonly string[] TX_LIST = { TX.Status_ContractPeriod.ACTIVE, TX.Status_ContractPeriod.PENDING, TX.Status_ContractPeriod.INACTIVE, TX.Status_ContractPeriod.INVOICED };
        }

        public class Status_Route
        {
            public const string OPEN        = "O";
            public const string IN_PROCESS  = "P";
            public const string CANCELED    = "X";
            public const string COMPLETED   = "C";
            public const string CLOSED      = "Z";

            public readonly string[] ID_LIST = { ID.Status_Route.OPEN, ID.Status_Route.IN_PROCESS, ID.Status_Route.CANCELED, ID.Status_Route.COMPLETED, ID.Status_Route.CLOSED };
            public readonly string[] TX_LIST = { TX.Status_Route.OPEN, TX.Status_Route.IN_PROCESS, TX.Status_Route.CANCELED, TX.Status_Route.COMPLETED, TX.Status_Route.CLOSED };
        }

        public class Status_Posting
        {
            public const string NOTHING_TO_POST = "NP";
            public const string PENDING_TO_POST = "PP";
            public const string POSTED          = "PT";

            public readonly string[] ID_LIST = { ID.Status_Posting.NOTHING_TO_POST, ID.Status_Posting.PENDING_TO_POST, ID.Status_Posting.POSTED };
            public readonly string[] TX_LIST = { TX.Status_Posting.NOTHING_TO_POST, TX.Status_Posting.PENDING_TO_POST, TX.Status_Posting.POSTED };
        }

        public class Status_Log
        {
            public const string IN_PROCESS  = "P";
            public const string COMPLETED   = "C";

            public readonly string[] ID_LIST = { ID.Status_Log.IN_PROCESS, ID.Status_Log.COMPLETED };
            public readonly string[] TX_LIST = { TX.Status_Log.IN_PROCESS, TX.Status_Log.COMPLETED };
        }
        #endregion

        public class Type_Log
        {
            public const string TRAVEL                  = "TR";
            public const string SERVICE                 = "SE";
            public const string NON_STOCK               = "NS";
            public const string STAFF_ASSIGMENT         = "SA";
            public const string SERV_BASED_ASSIGMENT    = "SB";

            public readonly string[] ID_LIST = { ID.Type_Log.TRAVEL, ID.Type_Log.STAFF_ASSIGMENT, ID.Type_Log.SERV_BASED_ASSIGMENT, ID.Type_Log.SERVICE, ID.Type_Log.NON_STOCK };
            public readonly string[] TX_LIST = { TX.Type_Log.TRAVEL, TX.Type_Log.STAFF_ASSIGMENT, TX.Type_Log.SERV_BASED_ASSIGMENT, TX.Type_Log.SERVICE, TX.Type_Log.NON_STOCK };
        }

        public class LogActions
        {
            public const string START       = "ST";
            public const string COMPLETE    = "CP";

            public readonly string[] ID_LIST = { START, COMPLETE };
            public readonly string[] TX_LIST = { TX.LogActions.START, TX.LogActions.COMPLETE };
        }

        public class FuelType_Equipment
        {
            public const string REGULAR_UNLEADED    = "R";
            public const string PREMIUM_UNLEADED    = "P";
            public const string DIESEL              = "D";
            public const string OTHER               = "O";

            public readonly string[] ID_LIST = { ID.FuelType_Equipment.REGULAR_UNLEADED, ID.FuelType_Equipment.PREMIUM_UNLEADED, ID.FuelType_Equipment.DIESEL, ID.FuelType_Equipment.OTHER };
            public readonly string[] TX_LIST = { TX.FuelType_Equipment.REGULAR_UNLEADED, TX.FuelType_Equipment.PREMIUM_UNLEADED, TX.FuelType_Equipment.DIESEL, TX.FuelType_Equipment.OTHER };
        }

        #region Inquiry
        public class Confirmed_Appointment
        {
            public const string ALL           = "A";
            public const string CONFIRMED     = "C";
            public const string NOT_CONFIRMED = "N";

            public readonly string[] ID_LIST = { ID.Confirmed_Appointment.ALL, ID.Confirmed_Appointment.CONFIRMED, ID.Confirmed_Appointment.NOT_CONFIRMED };
            public readonly string[] TX_LIST = { TX.Confirmed_Appointment.ALL, TX.Confirmed_Appointment.CONFIRMED, TX.Confirmed_Appointment.NOT_CONFIRMED };
        }

        public class PeriodType
        {
            public const string DAY   = "D";
            public const string WEEK  = "W";
            public const string MONTH = "M";

            public readonly string[] ID_LIST = { ID.PeriodType.DAY, ID.PeriodType.WEEK, ID.PeriodType.MONTH };
            public readonly string[] TX_LIST = { TX.PeriodType.DAY, TX.PeriodType.WEEK, TX.PeriodType.MONTH };
        }
        #endregion
        
        public class LicenseType_ValidIn
        {
            public const string ALL_LOCATIONS       = "ALL";
            public const string COUNTRY_STATE_CITY  = "CSC";
            public const string GEOGRAPHICAL_ZONE   = "GZN";
        }

        public class ReasonType
        {
            public const string CANCEL_SERVICE_ORDER = "CSOR";
            public const string CANCEL_APPOINTMENT   = "CAPP";
            public const string WORKFLOW_STAGE       = "WSTG";
            public const string APPOINTMENT_DETAIL   = "APPD";
            public const string GENERAL              = "GNRL";

            public readonly string[] ID_LIST = 
            {
                ID.ReasonType.CANCEL_SERVICE_ORDER, ID.ReasonType.CANCEL_APPOINTMENT, 
                    ID.ReasonType.WORKFLOW_STAGE, ID.ReasonType.APPOINTMENT_DETAIL, ID.ReasonType.GENERAL
            };

            public readonly string[] TX_LIST = 
            {
                TX.ReasonType.CANCEL_SERVICE_ORDER, TX.ReasonType.CANCEL_APPOINTMENT, 
                    TX.ReasonType.WORKFLOW_STAGE, TX.ReasonType.APPOINTMENT_DETAIL, TX.ReasonType.GENERAL
            };
        }

        public class Setup_CopyTimeSpentFrom
        {
            public const string NONE                              = "N";
            public const string ACTUAL_DURATION                   = "A";
            public const string ESTIMATED_DURATION                = "E";

            public readonly string[] ID_LIST = 
            {                
                ID.Setup_CopyTimeSpentFrom.ACTUAL_DURATION, 
                ID.Setup_CopyTimeSpentFrom.ESTIMATED_DURATION,
                ID.Setup_CopyTimeSpentFrom.NONE
            };

            public readonly string[] TX_LIST =
            {                
                TX.Setup_CopyTimeSpentFrom.ACTUAL_DURATION, 
                TX.Setup_CopyTimeSpentFrom.ESTIMATED_DURATION,
                TX.Setup_CopyTimeSpentFrom.NONE
            };
        }

        public class ContractType_BillingFrequency
        {
            public const string EVERY_4TH_MONTH     = "F";
            public const string SEMI_ANNUAL         = "S";
            public const string ANNUAL              = "A";
            public const string BEG_OF_CONTRACT     = "B";
            public const string END_OF_CONTRACT     = "E";
            public const string DAYS_30_60_90       = "D";
            public const string TIME_OF_SERVICE     = "T";
            public const string NONE                = "N";
            public const string MONTHLY             = "M";

            public readonly string[] ID_LIST =
            {
                ID.ContractType_BillingFrequency.EVERY_4TH_MONTH, ID.ContractType_BillingFrequency.SEMI_ANNUAL, ID.ContractType_BillingFrequency.ANNUAL,
                    ID.ContractType_BillingFrequency.BEG_OF_CONTRACT, ID.ContractType_BillingFrequency.END_OF_CONTRACT, ID.ContractType_BillingFrequency.DAYS_30_60_90,
                    ID.ContractType_BillingFrequency.TIME_OF_SERVICE, ID.ContractType_BillingFrequency.NONE, ID.ContractType_BillingFrequency.MONTHLY
            };

            public readonly string[] TX_LIST =
            {
                TX.ContractType_BillingFrequency.EVERY_4TH_MONTH, TX.ContractType_BillingFrequency.SEMI_ANNUAL, TX.ContractType_BillingFrequency.ANNUAL,
                    TX.ContractType_BillingFrequency.BEG_OF_CONTRACT, TX.ContractType_BillingFrequency.END_OF_CONTRACT, TX.ContractType_BillingFrequency.DAYS_30_60_90,
                    TX.ContractType_BillingFrequency.TIME_OF_SERVICE, TX.ContractType_BillingFrequency.NONE, TX.ContractType_BillingFrequency.MONTHLY
            };
        }

        public class Contract_BillingType
        {
            public const string AS_PERFORMED_BILLINGS = "APFB";
            public const string STANDARDIZED_BILLINGS = "STDB";

            public readonly string[] ID_LIST = { ID.Contract_BillingType.AS_PERFORMED_BILLINGS, ID.Contract_BillingType.STANDARDIZED_BILLINGS };
            public readonly string[] TX_LIST = { TX.Contract_BillingType.AS_PERFORMED_BILLINGS, TX.Contract_BillingType.STANDARDIZED_BILLINGS };
        }

        public class Contract_BillTo
        {
            public const string CUSTOMERACCT = "C";
            public const string SPECIFICACCT = "S";

            public readonly string[] ID_LIST = { ID.Contract_BillTo.CUSTOMERACCT, ID.Contract_BillTo.SPECIFICACCT };
            public readonly string[] TX_LIST = { TX.Contract_BillTo.CUSTOMERACCT, TX.Contract_BillTo.SPECIFICACCT };
        }

        public class Contract_ExpirationType
        {
            public const string EXPIRING  = "E";
            public const string UNLIMITED = "U";

            public readonly string[] ID_LIST = { ID.Contract_ExpirationType.EXPIRING, ID.Contract_ExpirationType.UNLIMITED };
            public readonly string[] TX_LIST = { TX.Contract_ExpirationType.EXPIRING, TX.Contract_ExpirationType.UNLIMITED };
        }

        public class Contract_BillingPeriod
        {
            //public const string ONETIME  = "O";
            public const string WEEK     = "W";
            public const string MONTH    = "M";
            public const string QUARTER  = "Q";
            public const string HALFYEAR = "H";
            public const string YEAR     = "Y";

            public readonly string[] ID_LIST = { /*ID.Contract_BillingPeriod.ONETIME, */ID.Contract_BillingPeriod.WEEK, ID.Contract_BillingPeriod.MONTH, ID.Contract_BillingPeriod.QUARTER, ID.Contract_BillingPeriod.HALFYEAR, ID.Contract_BillingPeriod.YEAR };
            public readonly string[] TX_LIST = { /*TX.Contract_BillingPeriod.ONETIME, */TX.Contract_BillingPeriod.WEEK, TX.Contract_BillingPeriod.MONTH, TX.Contract_BillingPeriod.QUARTER, TX.Contract_BillingPeriod.HALFYEAR, TX.Contract_BillingPeriod.YEAR };
        }

        #region SrvOrdType
        public class SrvOrdType_RecordType
        {
            //public const string TRAVEL             = "TV";
            //public const string TRAINING           = "TR";
            //public const string DOWNTIME           = "DT";
            public const string SERVICE_ORDER      = "SO";

            public readonly string[] ID_LIST =
            {
                ID.SrvOrdType_RecordType.SERVICE_ORDER/*, ID.SrvOrdType_RecordType.TRAVEL*//*, ID.SrvOrdType_RecordType.TRAINING*/,
                                                    /*ID.SrvOrdType_RecordType.DOWNTIME*/
            };

            public readonly string[] TX_LIST =
            {
                TX.SrvOrdType_RecordType.SERVICE_ORDER/*, TX.SrvOrdType_RecordType.TRAVEL*//*, TX.SrvOrdType_RecordType.TRAINING*/,
                                                    /*TX.SrvOrdType_RecordType.DOWNTIME*/
            };
        }

        public class SrvOrdType_PostTo : Contract_PostTo
        {
            public const string NONE = "NA";
            public const string PROJECTS = "PM";
        }

        public class Contract_PostTo
        {
            public const string ACCOUNTS_RECEIVABLE_MODULE = "AR";
            public const string SALES_ORDER_MODULE = "SO";
            public const string SALES_ORDER_INVOICE = "SI";
        }

        public class SrvOrdType_SalesAcctSource 
        {
            public const string INVENTORY_ITEM      = "II";
            public const string WAREHOUSE           = "WH";
            public const string POSTING_CLASS       = "PC";
            public const string CUSTOMER_LOCATION   = "CL";

            public readonly string[] ID_LIST = { ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM, ID.SrvOrdType_SalesAcctSource.WAREHOUSE
                                                    ,ID.SrvOrdType_SalesAcctSource.POSTING_CLASS, ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION };
            public readonly string[] TX_LIST = { TX.SrvOrdType_SalesAcctSource.INVENTORY_ITEM, TX.SrvOrdType_SalesAcctSource.WAREHOUSE
                                                    ,TX.SrvOrdType_SalesAcctSource.POSTING_CLASS, TX.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION };
        }

        public class Contract_SalesAcctSource
        {
            public const string INVENTORY_ITEM      = "II";
            public const string POSTING_CLASS       = "PC";
            public const string CUSTOMER_LOCATION   = "CL";
            
            public readonly string[] ID_LIST = { ID.Contract_SalesAcctSource.INVENTORY_ITEM, ID.Contract_SalesAcctSource.POSTING_CLASS, ID.Contract_SalesAcctSource.CUSTOMER_LOCATION};
            public readonly string[] TX_LIST = { TX.Contract_SalesAcctSource.INVENTORY_ITEM, TX.Contract_SalesAcctSource.POSTING_CLASS, TX.Contract_SalesAcctSource.CUSTOMER_LOCATION};
        }

        public class SrvOrdType_GenerateInvoiceBy
        {
            public const string CRM_AR       = "CRAR";
            public const string SALES_ORDER  = "SORD";
            public const string PROJECT      = "PROJ";
            public const string NOT_BILLABLE = "NBIL";

            public readonly string[] ID_LIST =
            {
                ID.SrvOrdType_GenerateInvoiceBy.CRM_AR, ID.SrvOrdType_GenerateInvoiceBy.SALES_ORDER,
                    ID.SrvOrdType_GenerateInvoiceBy.PROJECT, ID.SrvOrdType_GenerateInvoiceBy.NOT_BILLABLE
            };

            public readonly string[] TX_LIST =
            {
                TX.SrvOrdType_GenerateInvoiceBy.CRM_AR, TX.SrvOrdType_GenerateInvoiceBy.SALES_ORDER,
                    TX.SrvOrdType_GenerateInvoiceBy.PROJECT, TX.SrvOrdType_GenerateInvoiceBy.NOT_BILLABLE
            };
        }
        
        public class SrvOrdType_BillingType
        {
            public const string COST_AS_COST    = "CC";
            public const string COST_AS_REVENUE = "CR";

            public readonly string[] ID_LIST = { ID.SrvOrdType_BillingType.COST_AS_COST, ID.SrvOrdType_BillingType.COST_AS_REVENUE };
            public readonly string[] TX_LIST = { TX.SrvOrdType_BillingType.COST_AS_COST, TX.SrvOrdType_BillingType.COST_AS_REVENUE };
        }

        public class BusinessAcctType
        {
            public const string CUSTOMER = "C";
            public const string PROSPECT = "P";

            public readonly string[] ID_LIST = { ID.BusinessAcctType.CUSTOMER, ID.BusinessAcctType.PROSPECT };
            public readonly string[] TX_LIST = { TX.BusinessAcctType.CUSTOMER, TX.BusinessAcctType.PROSPECT };
        }

        public class Source_Info
        {
            public const string BUSINESS_ACCOUNT = "BA";
            public const string CUSTOMER_CONTACT = "CC";
            public const string BRANCH_LOCATION  = "BL";
        }

        public class SrvOrdType_AppAddressSource : Source_Info
        {
            public readonly string[] ID_LIST = { ID.Source_Info.BUSINESS_ACCOUNT, ID.Source_Info.CUSTOMER_CONTACT, ID.Source_Info.BRANCH_LOCATION };
            public readonly string[] TX_LIST = { TX.Source_Info.BUSINESS_ACCOUNT, TX.Source_Info.CUSTOMER_CONTACT, TX.Source_Info.BRANCH_LOCATION };
        }

        public class SrvOrdType_AppContactInfoSource : Source_Info
        {
            public readonly string[] ID_LIST = { ID.Source_Info.BUSINESS_ACCOUNT, ID.Source_Info.CUSTOMER_CONTACT };
            public readonly string[] TX_LIST = { TX.Source_Info.BUSINESS_ACCOUNT, TX.Source_Info.CUSTOMER_CONTACT };
        }

        #endregion

        public class ValidationType
        {
            public const string PREVENT      = "D";
            public const string WARN         = "W";
            public const string NOT_VALIDATE = "N";

            public readonly string[] ID_LIST =
            {
                ID.ValidationType.NOT_VALIDATE, ID.ValidationType.WARN,  ID.ValidationType.PREVENT
            };

            public readonly string[] TX_LIST =
            {
                TX.ValidationType.NOT_VALIDATE, TX.ValidationType.WARN, TX.ValidationType.PREVENT
            };
        }

        public class SourcePrice
        {
            public const string CONTRACT   = "C";
            public const string PRICE_LIST = "P";

            public readonly string[] ID_LIST = { ID.SourcePrice.CONTRACT, ID.SourcePrice.PRICE_LIST };
            public readonly string[] TX_LIST = { TX.SourcePrice.CONTRACT, TX.SourcePrice.PRICE_LIST };
        }

        public class RecordType_ContractAction
        {
            public const string CONTRACT = "C";
            public const string SCHEDULE = "S";

            public readonly string[] ID_LIST = { ID.RecordType_ContractAction.CONTRACT, ID.RecordType_ContractAction.SCHEDULE };
            public readonly string[] TX_LIST = { TX.RecordType_ContractAction.CONTRACT, TX.RecordType_ContractAction.SCHEDULE };
        }

        public class Action_ContractAction
        {
            public const string CREATE              = "N";
            public const string ACTIVATE            = "A";
            public const string SUSPEND             = "S";
            public const string CANCEL              = "X";
            public const string EXPIRE              = "E";
            public const string INACTIVATE_SCHEDULE = "I";
            public const string DELETE_SCHEDULE     = "D";

            public readonly string[] ID_LIST = { ID.Action_ContractAction.CREATE, ID.Action_ContractAction.ACTIVATE, ID.Action_ContractAction.SUSPEND, ID.Action_ContractAction.CANCEL, ID.Action_ContractAction.EXPIRE, ID.Action_ContractAction.INACTIVATE_SCHEDULE, ID.Action_ContractAction.DELETE_SCHEDULE };
            public readonly string[] TX_LIST = { TX.Action_ContractAction.CREATE, TX.Action_ContractAction.ACTIVATE, TX.Action_ContractAction.SUSPEND, TX.Action_ContractAction.CANCEL, TX.Action_ContractAction.EXPIRE, TX.Action_ContractAction.INACTIVATE_SCHEDULE, TX.Action_ContractAction.DELETE_SCHEDULE };
        }

        public class WarrantyDurationType
        {
            public const string DAY   = "D";
            public const string MONTH = "M";
            public const string YEAR  = "Y";

            public readonly string[] ID_LIST = { ID.WarrantyDurationType.DAY, ID.WarrantyDurationType.MONTH, ID.WarrantyDurationType.YEAR };
            public readonly string[] TX_LIST = { TX.WarrantyDurationType.DAY, TX.WarrantyDurationType.MONTH, TX.WarrantyDurationType.YEAR };
        }

        public class WarrantyApplicationOrder
        {
            public const string COMPANY = "C";
            public const string VENDOR  = "V";

            public readonly string[] ID_LIST = { ID.WarrantyApplicationOrder.COMPANY, ID.WarrantyApplicationOrder.VENDOR };
            public readonly string[] TX_LIST = { TX.WarrantyApplicationOrder.COMPANY, TX.WarrantyApplicationOrder.VENDOR };
        }

        public class ModelType
        {
            public const string EQUIPMENT   = "EQ";
            public const string REPLACEMENT = "RP";

            public readonly string[] ID_LIST = { ID.ModelType.EQUIPMENT, ID.ModelType.REPLACEMENT };
            public readonly string[] TX_LIST = { TX.ModelType.EQUIPMENT, TX.ModelType.REPLACEMENT };
        }

        public class SourceType_Equipment
        {
            public const string SM_EQUIPMENT        = "SME";
            public const string VEHICLE             = "VEH";
            public const string EP_EQUIPMENT        = "EPE";
            public const string AR_INVOICE          = "ARI";

            public readonly string[] ID_LIST = { ID.SourceType_Equipment.SM_EQUIPMENT, ID.SourceType_Equipment.VEHICLE, ID.SourceType_Equipment.EP_EQUIPMENT, ID.SourceType_Equipment.AR_INVOICE };
            public readonly string[] TX_LIST = { TX.SourceType_Equipment.SM_EQUIPMENT, TX.SourceType_Equipment.VEHICLE, TX.SourceType_Equipment.EP_EQUIPMENT, TX.SourceType_Equipment.AR_INVOICE };
        }

        //This class is used for filtering purposes only
        public class SourceType_Equipment_ALL : SourceType_Equipment
        {
            public const string ALL = "ALL";

            public readonly new string[] ID_LIST = { ID.SourceType_Equipment_ALL.SM_EQUIPMENT, ID.SourceType_Equipment_ALL.ALL };
            public readonly new string[] TX_LIST = { TX.SourceType_Equipment_ALL.SM_EQUIPMENT, TX.SourceType_Equipment_ALL.ALL };
        }

        public class ScreenID
        {
            public const string WRKPROCESS                            = "FS200000";
            public const string SERVICE_ORDER                         = "FS300100";
            public const string APPOINTMENT                           = "FS300200";
            public const string EMPLOYEE_SCHEDULE                     = "FS202200";
            public const string SALES_ORDER                           = "SO301000";
            public const string SO_INVOICE                            = "SO303000";
            public const string GENERATE_SERVICE_CONTRACT_APPOINTMENT = "FS500200";
            public const string BRANCH_LOCATION                       = "FS202500";
            public const string ROUTE_CLOSING                         = "FS304010";
            public const string WEB_METHOD                            = "FS300000";
            public const string INVOICE_BY_APPOINTMENT                = "FS500100";
            public const string INVOICE_BY_SERVICE_ORDER              = "FS500600";
            public const string ROUTE_DOCUMENT_DETAIL                 = "FS304000";
            public const string APPOINTMENT_INQUIRY                   = "FS400100";
            public const string SERVICE_CONTRACT                      = "FS305800";
            public const string CLONE_APPOINTMENT                     = "FS500201";
            public const string APPOINTMENT_DETAILS                   = "FS400500";

            // Mobile screens for log actions
            public const string LOG_ACTION_START_TRAVEL_SRV_MOBILE     = "FS300220";
            public const string LOG_ACTION_START_STAFF_MOBILE          = "FS300230";
            public const string LOG_ACTION_START_STAFF_ASSIGNED_MOBILE = "FS300240";
            public const string LOG_ACTION_COMPLETE_SERVICE_MOBILE     = "FS300250";
            public const string LOG_ACTION_COMPLETE_TRAVEL_MOBILE      = "FS300260";
        }
        
        public class ReportID
        {
            public const string SERVICE_ORDER           = "FS641000";
            public const string APPOINTMENT             = "FS642000";
            public const string SERVICE_TIME_ACTIVITY   = "FS654500";
            public const string APP_IN_SERVICE_ORDER    = "FS642500";
            public const string ROUTE_APP_GPS_LOCATION  = "FS643000";
        }

        public class OwnerType_Equipment
        {
            public const string CUSTOMER        = "TP";
            public const string OWN_COMPANY     = "OW";

            public readonly string[] ID_LIST = { ID.OwnerType_Equipment.CUSTOMER, ID.OwnerType_Equipment.OWN_COMPANY };
            public readonly string[] TX_LIST = { TX.OwnerType_Equipment.CUSTOMER, TX.OwnerType_Equipment.OWN_COMPANY };
        }

        public class AcumaticaErrorNumber
        { 
            public const string SAVE_BUTTON_DISABLED = "#106" /*Error number in Acumatica when you try to save a document and the save button is disabled*/;
        }

        public class MapsStatusCodes
        {
            //Defined by API
            public const string OK                     = "OK";
            public const string CREATED                = "Created";
            public const string ACCEPTED               = "Accepted";
            public const string BAD_REQUEST            = "Bad Request";
            public const string UNAUTHORIZED           = "Unauthorized";
            public const string FORBIDDEN              = "Forbidden";
            public const string NOT_FOUND              = "Not Found";
            public const string TOO_MANY_REQUESTS      = "Too Many Requests";
            public const string INTERNAL_SERVER_ERROR  = "Internal Server Error";
            public const string SERVICE_UNAVAILABLE    = "Service Unavailable";
            //Defined by developer
            public const string MAX_WAYPOINTS_EXCEEDED = "MAX_WAYPOINTS_EXCEEDED";
        }

        public class MapsConsts
        {
            public const string API_PREFIX = "https://dev.virtualearth.net/REST/v1/";
            public const string XML_SCHEMA = "bingSchema";
            public const string XML_SCHEMA_TAG = "bingSchema:";
            public const string XML_SCHEMA_URI = "http://schemas.microsoft.com/search/local/ws/rest/v1";
        }

        public class ScheduleMonthlySelection 
        { 
            public const string DAILY       = "D";
            public const string WEEKLY      = "W";
        }

        public class RecordType_ServiceContract
        {
            public const string SERVICE_CONTRACT           = "NRSC";
            public const string ROUTE_SERVICE_CONTRACT     = "IRSC";
            public const string EMPLOYEE_SCHEDULE_CONTRACT = "EPSC";

            public readonly string[] ID_LIST =
            {
                ID.RecordType_ServiceContract.SERVICE_CONTRACT, ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT,
                    ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT
            };

            public readonly string[] TX_LIST =
            {
                TX.RecordType_ServiceContract.SERVICE_CONTRACT, TX.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT,
                    TX.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT
            };
        }

        public class ScheduleGenType_ServiceContract
        {
            public const string SERVICE_ORDER = "SO";
            public const string APPOINTMENT   = "AP";
            public const string NONE = "NA";

            public readonly string[] ID_LIST =
            {
                ID.ScheduleGenType_ServiceContract.SERVICE_ORDER, ID.ScheduleGenType_ServiceContract.APPOINTMENT,
                ID.ScheduleGenType_ServiceContract.NONE
            };

            public readonly string[] TX_LIST =
            {
                TX.ScheduleGenType_ServiceContract.SERVICE_ORDER, TX.ScheduleGenType_ServiceContract.APPOINTMENT,
                TX.ScheduleGenType_ServiceContract.NONE
            };
        }

        public class ActionType_ProcessServiceContracts
        {
            public const string STATUS = "CS";
            public const string PERIOD = "CP";

            public readonly string[] ID_LIST =
            {
                ID.ActionType_ProcessServiceContracts.STATUS, ID.ActionType_ProcessServiceContracts.PERIOD
            };

            public readonly string[] TX_LIST =
            {
                TX.ActionType_ProcessServiceContracts.STATUS, TX.ActionType_ProcessServiceContracts.PERIOD
            };
        }

        public class Behavior_SrvOrderType
        {
            public const string REGULAR_APPOINTMENT     = "RE";
            public const string ROUTE_APPOINTMENT       = "RO";
            public const string INTERNAL_APPOINTMENT    = "IN";
            public const string QUOTE                   = "QT";

            public readonly string[] ID_LIST =
            {
                ID.Behavior_SrvOrderType.REGULAR_APPOINTMENT, ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT, 
                ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT, ID.Behavior_SrvOrderType.QUOTE
            };

            public readonly string[] TX_LIST =
            {
                TX.Behavior_SrvOrderType.REGULAR_APPOINTMENT, TX.Behavior_SrvOrderType.ROUTE_APPOINTMENT, 
                TX.Behavior_SrvOrderType.INTERNAL_APPOINTMENT, TX.Behavior_SrvOrderType.QUOTE
            };
        }

        public class PreAcctSource_Setup
        {
            public const string CUSTOMER_LOCATION = "CL";
            public const string INVENTORY_ITEM = "IN";

            public readonly string[] ID_LIST =
            {
                ID.PreAcctSource_Setup.CUSTOMER_LOCATION, ID.PreAcctSource_Setup.INVENTORY_ITEM
            };

            public readonly string[] TX_LIST =
            {
                TX.PreAcctSource_Setup.CUSTOMER_LOCATION, TX.PreAcctSource_Setup.INVENTORY_ITEM
            };
        }

        public class ContactType_ApptMail
        {
            public const string VENDOR         = "X";
            public const string CUSTOMER       = "U";
            public const string STAFF_MEMBERS  = "F";           
            public const string GEOZONE_STAFF  = "G";
            public const string SALESPERSON    = "L";

            public readonly string[] ID_LIST =
            {
                NotificationContactType.Billing, NotificationContactType.Employee,
                ID.ContactType_ApptMail.CUSTOMER, ID.ContactType_ApptMail.STAFF_MEMBERS, ID.ContactType_ApptMail.VENDOR,
                ID.ContactType_ApptMail.GEOZONE_STAFF, ID.ContactType_ApptMail.SALESPERSON
            };

            public readonly string[] TX_LIST =
            {
                PX.Objects.AR.Messages.Billing, PX.Objects.EP.Messages.Employee,
                TX.ContactType_ApptMail.CUSTOMER, TX.ContactType_ApptMail.STAFF_MEMBERS, TX.ContactType_ApptMail.VENDOR,
                TX.ContactType_ApptMail.GEOZONE_STAFF, TX.ContactType_ApptMail.SALESPERSON
            };
        }

        public class Calendar_ExceptionType
        {
            public const string AVAILABILITY   = "CA";
            public const string UNAVAILABILITY = "CE";
        }

        public class AppointmentAssignment_Status
        {
            public const string DELETE_APPOINTMENT_FROM_DB  = "D";
            public const string UNASSIGN_APPOINTMENT_ONLY   = "U";
        }

        public class EmployeeTimeSlotLevel
        {
            public const int BASE = 0;
            public const int COMPRESS = 1;

            public readonly int[] ID_LIST = { ID.EmployeeTimeSlotLevel.BASE, ID.EmployeeTimeSlotLevel.COMPRESS };
            public readonly string[] TX_LIST = { TX.EmployeeTimeSlotLevel.BASE, TX.EmployeeTimeSlotLevel.COMPRESS };
        }

        public class Service_Action_Type
        {
            public const string NO_ITEMS_RELATED    = "N";
            public const string PICKED_UP_ITEMS     = "P";
            public const string DELIVERED_ITEMS     = "D";

            public readonly string[] ID_LIST =
            {
                ID.Service_Action_Type.NO_ITEMS_RELATED, ID.Service_Action_Type.PICKED_UP_ITEMS, 
                ID.Service_Action_Type.DELIVERED_ITEMS
            };

            public readonly string[] TX_LIST =
            {
                TX.Service_Action_Type.NO_ITEMS_RELATED, TX.Service_Action_Type.PICKED_UP_ITEMS, 
                TX.Service_Action_Type.DELIVERED_ITEMS
            };
        }

        public class Appointment_Service_Action_Type : Service_Action_Type
        {
            public new readonly string[] TX_LIST =
            {
                TX.Service_Action_Type.NO_ITEMS_RELATED, TX.Appointment_Service_Action_Type.PICKED_UP_ITEMS,
                TX.Appointment_Service_Action_Type.DELIVERED_ITEMS
            };
        }

        public class DocumentSource
        {
            public const string INVOICE_FROM_APPOINTMENT = "AP";
            public const string INVOICE_FROM_SERVICEORDER = "SO";
            public const string INVOICE_FROM_SERVICECONTRACT = "CO";
        }

        public class Billing_By
        {
            public const string APPOINTMENT = DocumentSource.INVOICE_FROM_APPOINTMENT;
            public const string SERVICE_ORDER = DocumentSource.INVOICE_FROM_SERVICEORDER;
            public const string CONTRACT = "CO";

            public readonly string[] ID_LIST =
            {
                ID.Billing_By.APPOINTMENT, ID.Billing_By.SERVICE_ORDER
            };

            public readonly string[] TX_LIST =
            {
                TX.Billing_By.APPOINTMENT, TX.Billing_By.SERVICE_ORDER
            };
        }

        public class Billing_Cycle_Type
        {
            public const string APPOINTMENT     = "AP";
            public const string SERVICE_ORDER   = "SO";
            public const string TIME_FRAME      = "TC";
            public const string PURCHASE_ORDER  = "PO";
            public const string WORK_ORDER      = "WO";

            public readonly string[] ID_LIST = { ID.Billing_Cycle_Type.APPOINTMENT, ID.Billing_Cycle_Type.SERVICE_ORDER, ID.Billing_Cycle_Type.TIME_FRAME, ID.Billing_Cycle_Type.PURCHASE_ORDER, ID.Billing_Cycle_Type.WORK_ORDER };
            public readonly string[] TX_LIST = { TX.Billing_Cycle_Type.APPOINTMENT, TX.Billing_Cycle_Type.SERVICE_ORDER, TX.Billing_Cycle_Type.TIME_FRAME, TX.Billing_Cycle_Type.PURCHASE_ORDER, TX.Billing_Cycle_Type.WORK_ORDER };
        }

        public class Time_Cycle_Type
        {
            public const string WEEKDAY      = "WK";
            public const string DAY_OF_MONTH = "MT";

            public readonly string[] ID_LIST = { ID.Time_Cycle_Type.WEEKDAY, ID.Time_Cycle_Type.DAY_OF_MONTH };
            public readonly string[] TX_LIST = { TX.Time_Cycle_Type.WEEKDAY, TX.Time_Cycle_Type.DAY_OF_MONTH };
        }

        public class Frequency_Type
        {
            public const string WEEKLY  = "WK";
            public const string MONTHLY = "MT";
            public const string NONE    = "NO";

            public readonly string[] ID_LIST = { ID.Frequency_Type.WEEKLY, ID.Frequency_Type.MONTHLY, ID.Frequency_Type.NONE };
            public readonly string[] TX_LIST = { TX.Frequency_Type.WEEKLY, TX.Frequency_Type.MONTHLY, TX.Frequency_Type.NONE };
        }

        public class Send_Invoices_To
        {
            public const string BILLING_CUSTOMER_BILL_TO          = "BT";
            public const string DEFAULT_BILLING_CUSTOMER_LOCATION = "DF";
            public const string SO_BILLING_CUSTOMER_LOCATION      = "LC";
            public const string SERVICE_ORDER_ADDRESS             = "SO";

            public readonly string[] ID_LIST = { ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO, ID.Send_Invoices_To.DEFAULT_BILLING_CUSTOMER_LOCATION, ID.Send_Invoices_To.SO_BILLING_CUSTOMER_LOCATION, ID.Send_Invoices_To.SERVICE_ORDER_ADDRESS };
            public readonly string[] TX_LIST = { TX.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO, TX.Send_Invoices_To.DEFAULT_BILLING_CUSTOMER_LOCATION, TX.Send_Invoices_To.SO_BILLING_CUSTOMER_LOCATION, TX.Send_Invoices_To.SERVICE_ORDER_ADDRESS };
        }

        public class Ship_To 
        {
            public const string BILLING_CUSTOMER_BILL_TO     = "BT";
            public const string SO_BILLING_CUSTOMER_LOCATION = "BL";
            public const string SO_CUSTOMER_LOCATION         = "LC";
            public const string SERVICE_ORDER_ADDRESS        = "SO";

            public readonly string[] ID_LIST = { ID.Ship_To.BILLING_CUSTOMER_BILL_TO, ID.Ship_To.SO_BILLING_CUSTOMER_LOCATION, ID.Ship_To.SO_CUSTOMER_LOCATION, ID.Ship_To.SERVICE_ORDER_ADDRESS };
            public readonly string[] TX_LIST = { TX.Ship_To.BILLING_CUSTOMER_BILL_TO, TX.Ship_To.SO_BILLING_CUSTOMER_LOCATION, TX.Ship_To.SO_CUSTOMER_LOCATION, TX.Ship_To.SERVICE_ORDER_ADDRESS };
        }

        public class Default_Billing_Customer_Source
        {
            public const string SERVICE_ORDER_CUSTOMER = "SO";
            public const string DEFAULT_CUSTOMER = "DC";
            public const string SPECIFIC_CUSTOMER = "LC";

            public readonly string[] ID_LIST = { ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER, ID.Default_Billing_Customer_Source.DEFAULT_CUSTOMER, ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER };
            public readonly string[] TX_LIST = { TX.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER, TX.Default_Billing_Customer_Source.DEFAULT_CUSTOMER, TX.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER };
        }

        public class Batch_PostTo : Batch_PostTo_Filter
        {
            public new const string SO      = "SO";
            public new const string AR_AP   = "AA";
            public new const string SI      = "SI";
            public new const string PM      = "PM";
            public const string AR          = "AR";
            public const string AP          = "AP";
            public const string IN          = "IN";
            

            public new readonly string[] ID_LIST = { ID.Batch_PostTo.AR, ID.Batch_PostTo.SO, ID.Batch_PostTo.SI, ID.Batch_PostTo.AP, ID.Batch_PostTo.IN, ID.Batch_PostTo.AR_AP, ID.Batch_PostTo.PM };
            public new readonly string[] TX_LIST = { TX.Batch_PostTo.AR, TX.Batch_PostTo.SO, TX.Batch_PostTo.SI, TX.Batch_PostTo.AP, TX.Batch_PostTo.IN, TX.Batch_PostTo.AR_AP, TX.Batch_PostTo.PM };
        }

        public class Batch_PostTo_Filter
        {
            public const string AR_AP = "AA";
            public const string SO = "SO";
            public const string SI = "SI";
            public const string PM = "PM";

            public readonly string[] ID_LIST = { ID.Batch_PostTo.AR_AP, ID.Batch_PostTo.SO, ID.Batch_PostTo.SI, ID.Batch_PostTo.PM };
            public readonly string[] TX_LIST = { TX.Batch_PostTo.AR_AP, TX.Batch_PostTo.SO, TX.Batch_PostTo.SI, TX.Batch_PostTo.PM };
        }

        public class TablePostSource
        {
            public const string FSAPPOINTMENT_DET               = "FSAppointmentDet";
            public const string FSSO_DET                        = "FSSODet";
        }

        public class PriceErrorCode
        {
            public const string OK                = "OK";
            public const string UOM_INCONSISTENCY = "UOM_INCONSISTENCY";
        }

        public class AcumaticaFolderName
        {
            public const string PAGE = "Pages";
        }

        public class Module
        {
            public const string SERVICE_DISPATCH = "FS";
        }

        public class EquipmentWarrantyFrom
        {
            public const string SALES_ORDER_DATE    = "SD";
            public const string INSTALLATION_DATE   = "AD";
            public const string EARLIEST_BOTH_DATE  = "ED";
            public const string LATEST_BOTH_DATE    = "LD";
        }

        public class WarratySource
        {
            public const string COMPANY = "C";
            public const string VENDOR  = "V";
        }

        public class Equipment_Item_Class
        {
            public const string PART_OTHER_INVENTORY    = "OI";
            public const string MODEL_EQUIPMENT         = "ME";
            public const string COMPONENT               = "CT";
            public const string CONSUMABLE              = "CE";

            public readonly string[] ID_LIST = { ID.Equipment_Item_Class.PART_OTHER_INVENTORY, ID.Equipment_Item_Class.MODEL_EQUIPMENT, ID.Equipment_Item_Class.COMPONENT, ID.Equipment_Item_Class.CONSUMABLE };
            public readonly string[] TX_LIST = { TX.Equipment_Item_Class.PART_OTHER_INVENTORY, TX.Equipment_Item_Class.MODEL_EQUIPMENT, TX.Equipment_Item_Class.COMPONENT, TX.Equipment_Item_Class.CONSUMABLE };
        }

        public class Equipment_Status
        {
            public const string ACTIVE      = "A";
            public const string SUSPENDED   = "S";
            public const string DISPOSED    = "D";

            public readonly string[] ID_LIST = { ID.Equipment_Status.ACTIVE, ID.Equipment_Status.SUSPENDED, ID.Equipment_Status.DISPOSED };
            public readonly string[] TX_LIST = { TX.Equipment_Status.ACTIVE, TX.Equipment_Status.SUSPENDED, TX.Equipment_Status.DISPOSED };
        }

        public class Equipment_Action_Base
        {
            public const string NONE                         = "NO";
            public const string SELLING_TARGET_EQUIPMENT     = "ST";
            public const string REPLACING_TARGET_EQUIPMENT   = "RT";
            public const string CREATING_COMPONENT           = "CC";
            public const string UPGRADING_COMPONENT          = "UC";
            public const string REPLACING_COMPONENT          = "RC";

            public readonly string[] ID_LIST = { ID.Equipment_Action.NONE, ID.Equipment_Action.SELLING_TARGET_EQUIPMENT, ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT,
                                                    ID.Equipment_Action.REPLACING_COMPONENT};
            public readonly string[] TX_LIST = { TX.Equipment_Action.NONE, TX.Equipment_Action.SELLING_TARGET_EQUIPMENT, TX.Equipment_Action.REPLACING_TARGET_EQUIPMENT,
                                                    TX.Equipment_Action.REPLACING_COMPONENT};
        }

        public class Equipment_Action : Equipment_Action_Base
        {
            public new readonly string[] ID_LIST = { ID.Equipment_Action.NONE, ID.Equipment_Action.SELLING_TARGET_EQUIPMENT, ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT,
                                                    ID.Equipment_Action.CREATING_COMPONENT, ID.Equipment_Action.UPGRADING_COMPONENT, ID.Equipment_Action.REPLACING_COMPONENT};
            public new readonly string[] TX_LIST = { TX.Equipment_Action.NONE, TX.Equipment_Action.SELLING_TARGET_EQUIPMENT, TX.Equipment_Action.REPLACING_TARGET_EQUIPMENT,
                                                    TX.Equipment_Action.CREATING_COMPONENT, TX.Equipment_Action.UPGRADING_COMPONENT, TX.Equipment_Action.REPLACING_COMPONENT};
        }

        public class Schedule_Equipment_Action : Equipment_Action_Base
        {
            public new readonly string[] ID_LIST = { ID.Equipment_Action.NONE, ID.Equipment_Action.SELLING_TARGET_EQUIPMENT};
            public new readonly string[] TX_LIST = { TX.Equipment_Action.NONE, TX.Equipment_Action.SELLING_TARGET_EQUIPMENT};
        }

        public class CloningType_CloneAppointment
        {
            public const string SINGLE = "SI";
            public const string MULTIPLE = "MU";

            public readonly string[] ID_LIST =
            {
                ID.CloningType_CloneAppointment.SINGLE,
                ID.CloningType_CloneAppointment.MULTIPLE
            };

            public readonly string[] TX_LIST =
            {
                TX.CloningType_CloneAppointment.SINGLE,
                TX.CloningType_CloneAppointment.MULTIPLE
            };
        }

        /// <summary>
        /// EntityType for FSAddress and FSContact tables
        /// </summary>
        public class ACEntityType
        {
            public const string MANUFACTURER = "MNFC";
            public const string BRANCH_LOCATION = "BLOC";
            public const string SERVICE_ORDER = "SROR";
            public const string APPOINTMENT = "APPT";

            public class Manufacturer : PX.Data.BQL.BqlString.Constant<Manufacturer>
            {
                public Manufacturer() : base(MANUFACTURER)
                {
                }
            }

            public class BranchLocation : PX.Data.BQL.BqlString.Constant<BranchLocation>
            {
                public BranchLocation() : base(BRANCH_LOCATION)
                {
                }
            }

            public class ServiceOrder : PX.Data.BQL.BqlString.Constant<ServiceOrder>
            {
                public ServiceOrder() : base(SERVICE_ORDER)
                {
                }
            }

            public class Appointment : PX.Data.BQL.BqlString.Constant<Appointment>
            {
                public Appointment() : base(APPOINTMENT)
                {
                }
            }
        }

        public class ServiceOrder_Action_Filter
        {
            public const string UNDEFINED = "UD";
            public const string COMPLETE = "CO";
            public const string CANCEL = "CA";
            public const string REOPEN = "RE";
            public const string CLOSE = "CL";
            public const string UNCLOSE = "UN";
            public const string ALLOWINVOICE = "AL";

            public readonly string[] ID_LIST =
            {
                ID.ServiceOrder_Action_Filter.UNDEFINED,
                ID.ServiceOrder_Action_Filter.COMPLETE,
                ID.ServiceOrder_Action_Filter.CANCEL,
                ID.ServiceOrder_Action_Filter.REOPEN,
                ID.ServiceOrder_Action_Filter.CLOSE,
                ID.ServiceOrder_Action_Filter.UNCLOSE,
                ID.ServiceOrder_Action_Filter.ALLOWINVOICE
            };

            public readonly string[] TX_LIST =
            {
                TX.ServiceOrder_Action_Filter.UNDEFINED,
                TX.ServiceOrder_Action_Filter.COMPLETE,
                TX.ServiceOrder_Action_Filter.CANCEL,
                TX.ServiceOrder_Action_Filter.REOPEN,
                TX.ServiceOrder_Action_Filter.CLOSE,
                TX.ServiceOrder_Action_Filter.UNCLOSE,
                TX.ServiceOrder_Action_Filter.ALLOWINVOICE
            };
        }

        public class TimeRange_Setup
        {
            public const string DAY = "D";
            public const string WEEK = "W";
            public const string MONTH = "M";

            public readonly string[] ID_LIST =
            {
                ID.TimeRange_Setup.DAY, ID.TimeRange_Setup.WEEK, ID.TimeRange_Setup.MONTH
            };

            public readonly string[] TX_LIST =
            {
                TX.TimeRange_Setup.DAY, TX.TimeRange_Setup.WEEK, TX.TimeRange_Setup.MONTH
            };
        }

        public class TimeFilter_Setup
        {
            public const string CLEARED_FILTER = "CF";
            public const string WORKING_TIME = "WT";
            public const string WEEKDAYS = "WD";
            public const string WORKING_TIME_WEEKDAYS = "WW";
            public const string BOOKED_DAYS = "BD";

            public readonly string[] ID_LIST =
            {
                ID.TimeFilter_Setup.CLEARED_FILTER, ID.TimeFilter_Setup.WORKING_TIME,
                ID.TimeFilter_Setup.WEEKDAYS, ID.TimeFilter_Setup.WORKING_TIME_WEEKDAYS,
                ID.TimeFilter_Setup.BOOKED_DAYS
            };

            public readonly string[] TX_LIST =
            {
                TX.TimeFilter_Setup.CLEARED_FILTER, TX.TimeFilter_Setup.WORKING_TIME,
                TX.TimeFilter_Setup.WEEKDAYS, TX.TimeFilter_Setup.WORKING_TIME_WEEKDAYS,
                TX.TimeFilter_Setup.BOOKED_DAYS
            };
        }

        public class DayResolution_Setup
        {
            public const int D13 = 13;
            public const int D14 = 14;
            public const int D15 = 15;
            public const int D16 = 16;
            public const int D17 = 17;
            public const int D18 = 18;
            public const int D19 = 19;

            public readonly int[] ID_LIST =
            {
                ID.DayResolution_Setup.D14, ID.DayResolution_Setup.D15,
                ID.DayResolution_Setup.D16, ID.DayResolution_Setup.D17,
                ID.DayResolution_Setup.D18, ID.DayResolution_Setup.D19
            };

            public readonly string[] TX_LIST =
            {
                TX.DayResolution_Setup.D14, TX.DayResolution_Setup.D15,
                TX.DayResolution_Setup.D16, TX.DayResolution_Setup.D17,
                TX.DayResolution_Setup.D18, TX.DayResolution_Setup.D19
            };
        }

        public class WeekResolution_Setup
        {
            public const int W10 = 10;
            public const int W11 = 11;
            public const int W12 = 12;
            public const int W13 = 13;
            public const int W14 = 14;
            public const int W15 = 15;
            public const int W16 = 16;
            public const int W17 = 17;
            public const int W18 = 18;
            public const int W19 = 19;

            public readonly int[] ID_LIST =
            {
                ID.WeekResolution_Setup.W12, ID.WeekResolution_Setup.W13,
                ID.WeekResolution_Setup.W14, ID.WeekResolution_Setup.W15,
                ID.WeekResolution_Setup.W16, ID.WeekResolution_Setup.W17,
                ID.WeekResolution_Setup.W18, ID.WeekResolution_Setup.W19
            };

            public readonly string[] TX_LIST =
            {
                TX.WeekResolution_Setup.W12, TX.WeekResolution_Setup.W13,
                TX.WeekResolution_Setup.W14, TX.WeekResolution_Setup.W15,
                TX.WeekResolution_Setup.W16, TX.WeekResolution_Setup.W17,
                TX.WeekResolution_Setup.W18, TX.WeekResolution_Setup.W19
            };
        }

        public class MonthResolution_Setup
        {
            public const int M06 = 6;
            public const int M07 = 7;
            public const int M08 = 8;
            public const int M09 = 9;
            public const int M10 = 10;
            public const int M11 = 11;
            public const int M12 = 12;
            public const int M13 = 13;
            public const int M14 = 14;
            public const int M15 = 15;
            public const int M16 = 16;
            public const int M17 = 17;
            public const int M18 = 18;
            public const int M19 = 19;

            public readonly int[] ID_LIST =
            {
                ID.MonthResolution_Setup.M10, ID.MonthResolution_Setup.M11,
                ID.MonthResolution_Setup.M12, ID.MonthResolution_Setup.M13,
                ID.MonthResolution_Setup.M14, ID.MonthResolution_Setup.M15,
                ID.MonthResolution_Setup.M16, ID.MonthResolution_Setup.M17,
                ID.MonthResolution_Setup.M18, ID.MonthResolution_Setup.M19
            };

            public readonly string[] TX_LIST =
            {
                TX.MonthResolution_Setup.M10, TX.MonthResolution_Setup.M11,
                TX.MonthResolution_Setup.M12, TX.MonthResolution_Setup.M13,
                TX.MonthResolution_Setup.M14, TX.MonthResolution_Setup.M15,
                TX.MonthResolution_Setup.M16, TX.MonthResolution_Setup.M17,
                TX.MonthResolution_Setup.M18, TX.MonthResolution_Setup.M19
            };
        }

        public class Status_ROOptimization
        {
            public const string NOT_OPTIMIZED    = "NO";
            public const string OPTIMIZED        = "OP";
            public const string NOT_ABLE         = "NA";
            public const string ADDRESS_ERROR    = "AE";

            public readonly string[] ID_LIST = { ID.Status_ROOptimization.NOT_OPTIMIZED, ID.Status_ROOptimization.OPTIMIZED, ID.Status_ROOptimization.NOT_ABLE, ID.Status_ROOptimization.ADDRESS_ERROR };
            public readonly string[] TX_LIST = { TX.Status_ROOptimization.NOT_OPTIMIZED, TX.Status_ROOptimization.OPTIMIZED, TX.Status_ROOptimization.NOT_ABLE, TX.Status_ROOptimization.ADDRESS_ERROR };
        }

        public class Type_ROOptimization
        {
            public const string ASSIGNED_APP   = "AP";
            public const string UNASSIGNED_APP = "UA";

            public readonly string[] ID_LIST = { ID.Type_ROOptimization.ASSIGNED_APP, ID.Type_ROOptimization.UNASSIGNED_APP };
            public readonly string[] TX_LIST = { TX.Type_ROOptimization.ASSIGNED_APP, TX.Type_ROOptimization.UNASSIGNED_APP };
        }
    }
}
