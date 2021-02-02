using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;
using System.Collections.Generic;

namespace PX.Objects.FS
{
    #region LineType

    public abstract class ListField_LineType_ALL : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_ALL().ID_LIST_ALL,
                    new ID.LineType_ALL().TX_LIST_ALL)
            {
            }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
		{
            public Inventory_Item() : base(ID.LineType_ALL.INVENTORY_ITEM)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
		{
            public Service() : base(ID.LineType_ALL.SERVICE)
            {
            }
        }

        public class Service_Template : PX.Data.BQL.BqlString.Constant<Service_Template>
		{
            public Service_Template() : base(ID.LineType_ALL.SERVICE_TEMPLATE)
            {
            }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
		{
            public NonStockItem() : base(ID.LineType_ALL.NONSTOCKITEM) 
            {
            }
        }

        public class Comment : PX.Data.BQL.BqlString.Constant<Comment>
        {
            public Comment() : base(ID.LineType_ALL.COMMENT)
            {
            }
        }

        public class Instruction : PX.Data.BQL.BqlString.Constant<Instruction>
        {
            public Instruction() : base(ID.LineType_ALL.INSTRUCTION)
            {
            }
        }
    }

    public abstract class ListField_LineType_UnifyTabs : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_AppSrvOrd().ID_LIST_ALL,
                    new ID.LineType_AppSrvOrd().TX_LIST_ALL)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(ID.LineType_AppSrvOrd.SERVICE)
            {
            }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
        {
            public NonStockItem() : base(ID.LineType_AppSrvOrd.NONSTOCKITEM)
            {
            }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
        {
            public Inventory_Item() : base(ID.LineType_AppSrvOrd.INVENTORY_ITEM)
            {
            }
        }

        public class Comment : PX.Data.BQL.BqlString.Constant<Comment>
        {
            public Comment() : base(ID.LineType_AppSrvOrd.COMMENT)
            {
            }
        }

        public class Instruction : PX.Data.BQL.BqlString.Constant<Instruction>
        {
            public Instruction() : base(ID.LineType_AppSrvOrd.INSTRUCTION)
            {
            }
        }
    }

    public abstract class ListField_LineType_Pickup_Delivery : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_Pickup_Delivery().ID_LIST_SERVICE,
                    new ID.LineType_Pickup_Delivery().TX_LIST_SERVICE)
            {
            }
        }

        public class Pickup_Delivery : PX.Data.BQL.BqlString.Constant<Pickup_Delivery>
		{
            public Pickup_Delivery() : base(ID.LineType_Pickup_Delivery.PICKUP_DELIVERY)
            {
            }
        }
    }

    public abstract class ListField_LineType_Schedule : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_Schedule().ID_LIST_ALL,
                    new ID.LineType_Schedule().TX_LIST_ALL)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(ID.LineType_ALL.SERVICE) { }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
        {
            public NonStockItem() : base(ID.LineType_ALL.NONSTOCKITEM) { }
        }

        public class Service_Template : PX.Data.BQL.BqlString.Constant<Service_Template>
        {
            public Service_Template() : base(ID.LineType_ALL.SERVICE_TEMPLATE) { }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
        {
            public Inventory_Item() : base(ID.LineType_ALL.INVENTORY_ITEM) { }
        }

        public class Comment : PX.Data.BQL.BqlString.Constant<Comment>
        {
            public Comment() : base(ID.LineType_ALL.COMMENT) { }
        }

        public class Instruction : PX.Data.BQL.BqlString.Constant<Instruction>
        {
            public Instruction() : base(ID.LineType_ALL.INSTRUCTION) { }
        }
    }

    public abstract class ListField_LineType_Profitability : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_Profitability().ID_LIST_ALL,
                    new ID.LineType_Profitability().TX_LIST_ALL)
            {
            }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
		{
            public Inventory_Item() : base(ID.LineType_Profitability.INVENTORY_ITEM)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
		{
            public Service() : base(ID.LineType_Profitability.SERVICE)
            {
            }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
		{
            public NonStockItem() : base(ID.LineType_Profitability.NONSTOCKITEM)
            {
            }
        }

        public class LaborItem : PX.Data.BQL.BqlString.Constant<LaborItem>
		{
            public LaborItem() : base(ID.LineType_Profitability.LABOR_ITEM)
            {
            }
        }
    }

    public abstract class ListField_LineType_SalesPrices : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_SalesPrice().ID_LIST_ALL,
                    new ID.LineType_SalesPrice().TX_LIST_ALL)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
		{
            public Service()
                : base(ID.LineType_SalesPrice.SERVICE)
            {
            }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
		{
            public NonStockItem()
                : base(ID.LineType_SalesPrice.NONSTOCKITEM)
            {
            }
        }

        public class Inventory_Item : PX.Data.BQL.BqlString.Constant<Inventory_Item>
		{
            public Inventory_Item()
                : base(ID.LineType_SalesPrice.INVENTORY_ITEM)
            {
            }
        }
    }

    public abstract class ListField_LineType_ContractPeriod : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LineType_ContractPeriod().ID_LIST_ALL,
                    new ID.LineType_ContractPeriod().TX_LIST_ALL)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
		{
            public Service()
                : base(ID.LineType_ContractPeriod.SERVICE)
            {
            }
        }

        public class NonStockItem : PX.Data.BQL.BqlString.Constant<NonStockItem>
		{
            public NonStockItem()
                : base(ID.LineType_ContractPeriod.NONSTOCKITEM)
            {
            }
        }
    }
    #endregion

    #region PriceType
    public abstract class ListField_PriceType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.PriceType().ID_LIST_PRICETYPE,
                    new ID.PriceType().TX_LIST_PRICETYPE)
            {
            }
        }

        public class Contract : PX.Data.BQL.BqlString.Constant<Contract>
		{
            public Contract() : base(ID.PriceType.CONTRACT)
            {
            }
        }

        public class Customer : PX.Data.BQL.BqlString.Constant<Customer>
		{
            public Customer() : base(ID.PriceType.CUSTOMER)
            {
            }
        }

        public class PriceClass : PX.Data.BQL.BqlString.Constant<PriceClass>
		{
            public PriceClass() : base(ID.PriceType.PRICE_CLASS)
            {
            }
        }

        public class Base : PX.Data.BQL.BqlString.Constant<Base>
		{
            public Base() : base(ID.PriceType.BASE)
            {
            }
        }

        public class Default : PX.Data.BQL.BqlString.Constant<Default>
		{
            public Default() : base(ID.PriceType.DEFAULT)
            {
            }
        }
    }
    #endregion

    #region Status_SODet
    public class ListField_Status_SODet : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(new[]
                {
                    Pair(ID.Status_SODet.SCHEDULED_NEEDED, TX.Status_SODet.SCHEDULED_NEEDED),
                    Pair(ID.Status_SODet.SCHEDULED, TX.Status_SODet.SCHEDULED),
                    Pair(ID.Status_SODet.CANCELED, TX.Status_SODet.CANCELED),
                    Pair(ID.Status_SODet.COMPLETED, TX.Status_SODet.COMPLETED),
                })
            {
            }
        }

        public class ScheduleNeeded : PX.Data.BQL.BqlString.Constant<ScheduleNeeded>
		{
            public ScheduleNeeded() : base(ID.Status_SODet.SCHEDULED_NEEDED)
            {
            }
        }

        public class Scheduled : PX.Data.BQL.BqlString.Constant<Scheduled>
        {
            public Scheduled() : base(ID.Status_SODet.SCHEDULED)
            {
            }
        }

        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
		{
            public Completed() : base(ID.Status_SODet.COMPLETED)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
        {
            public Canceled() : base(ID.Status_SODet.CANCELED)
            {
            }
        }
    }
    #endregion

    #region Status_AppointmentDet
    public class ListField_Status_AppointmentDet : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(new[]
                {
                    Pair(ID.Status_AppointmentDet.NOT_STARTED, TX.Status_AppointmentDet.NOT_STARTED),
                    Pair(ID.Status_AppointmentDet.IN_PROCESS, TX.Status_AppointmentDet.IN_PROCESS),
                    Pair(ID.Status_AppointmentDet.CANCELED, TX.Status_AppointmentDet.CANCELED),
                    Pair(ID.Status_AppointmentDet.COMPLETED, TX.Status_AppointmentDet.COMPLETED),
                    Pair(ID.Status_AppointmentDet.NOT_FINISHED, TX.Status_AppointmentDet.NOT_FINISHED),
                    Pair(ID.Status_AppointmentDet.NOT_PERFORMED, TX.Status_AppointmentDet.NOT_PERFORMED),
                })
            {
            }
        }

        public class NotStarted : PX.Data.BQL.BqlString.Constant<NotStarted>
        {
            public NotStarted() : base(ID.Status_AppointmentDet.NOT_STARTED)
            {
            }
        }

        public class InProcess : PX.Data.BQL.BqlString.Constant<InProcess>
        {
            public InProcess() : base(ID.Status_AppointmentDet.IN_PROCESS)
            {
            }
        }

        public class NotFinished : PX.Data.BQL.BqlString.Constant<NotFinished>
		{
            public NotFinished() : base(ID.Status_AppointmentDet.NOT_FINISHED)
            {
            }
        }

        public class NotPerformed : PX.Data.BQL.BqlString.Constant<NotPerformed>
        {
            public NotPerformed() : base(ID.Status_AppointmentDet.NOT_PERFORMED)
            {
            }
        }

        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
        {
            public Completed() : base(ID.Status_AppointmentDet.COMPLETED)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
        {
            public Canceled() : base(ID.Status_AppointmentDet.CANCELED)
            {
            }
        }
    }
    #endregion

    #region Condition
    public abstract class ListField_Condition_Equipment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute() : base(
                new ID.Condition().ID_LIST,
                new ID.Condition().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class New : PX.Data.BQL.BqlString.Constant<New>
		{
            public New() : base(ID.Condition.NEW)
            {
            }
        }

        public class Used : PX.Data.BQL.BqlString.Constant<Used>
		{
            public Used() : base(ID.Condition.USED)
            {
            }
        }
    }
    #endregion

    #region LocationType
    public abstract class ListField_LocationType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute() : base(
                    new ID.LocationType().ID_LIST,
                    new ID.LocationType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Company : PX.Data.BQL.BqlString.Constant<Company>
		{
            public Company() : base(ID.LocationType.COMPANY)
            {
            }
        }

        public class Customer : PX.Data.BQL.BqlString.Constant<Customer>
		{
            public Customer() : base(ID.LocationType.CUSTOMER)
            {
            }
        }
    }
    #endregion

    #region Status_Appointment
    public abstract class ListField_Status_Appointment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_Appointment().ID_LIST,
                    new ID.Status_Appointment().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class AutomaticScheduled : PX.Data.BQL.BqlString.Constant<AutomaticScheduled>
		{
            public AutomaticScheduled() : base(ID.Status_Appointment.AUTOMATIC_SCHEDULED)
            {
            }
        }

        public class ManualScheduled : PX.Data.BQL.BqlString.Constant<ManualScheduled>
		{
            public ManualScheduled() : base(ID.Status_Appointment.MANUAL_SCHEDULED)
            {
            }
        }

        public class InProcess : PX.Data.BQL.BqlString.Constant<InProcess>
		{
            public InProcess() : base(ID.Status_Appointment.IN_PROCESS)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
		{
            public Canceled() : base(ID.Status_Appointment.CANCELED)
            {
            }
        }

        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
		{
            public Completed() : base(ID.Status_Appointment.COMPLETED)
            {
            }
        }

        public class Closed : PX.Data.BQL.BqlString.Constant<Closed>
		{
            public Closed() : base(ID.Status_Appointment.CLOSED)
            {
            }
        }

        public class OnHold : PX.Data.BQL.BqlString.Constant<OnHold>
        {
            public OnHold() : base(ID.Status_Appointment.ON_HOLD)
            {
            }
        }
    }
    #endregion

    #region Status_Posting
    public abstract class ListField_Status_Posting : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_Posting().ID_LIST,
                    new ID.Status_Posting().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class NothingToPost : PX.Data.BQL.BqlString.Constant<NothingToPost>
		{
            public NothingToPost()
                : base(ID.Status_Posting.NOTHING_TO_POST)
            {
            }
        }

        public class PendingToPost : PX.Data.BQL.BqlString.Constant<PendingToPost>
		{
            public PendingToPost()
                : base(ID.Status_Posting.PENDING_TO_POST)
            {
            }
        }

        public class Posted : PX.Data.BQL.BqlString.Constant<Posted>
		{
            public Posted()
                : base(ID.Status_Posting.POSTED)
            {
            }
        }
    }
    #endregion

    #region Status_Log
    //List attribute
    public abstract class ListField_Status_Log : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_Log().ID_LIST,
                    new ID.Status_Log().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
        {
            public Completed() : base(ID.Status_Log.COMPLETED)
            {
            }
        }

        public class InProcess : PX.Data.BQL.BqlString.Constant<InProcess>
        {
            public InProcess() : base(ID.Status_Log.IN_PROCESS)
            {
            }
        }
    }
    #endregion

    #region Log ListFields
    #region Type_Log
    //List attribute
    public abstract class ListField_Type_Log : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Type_Log().ID_LIST,
                    new ID.Type_Log().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Travel : PX.Data.BQL.BqlString.Constant<Travel>
        {
            public Travel() : base(ID.Type_Log.TRAVEL)
            {
            }
        }

        public class Service : PX.Data.BQL.BqlString.Constant<Service>
        {
            public Service() : base(ID.Type_Log.SERVICE)
            {
            }
        }

        public class NonStock : PX.Data.BQL.BqlString.Constant<NonStock>
        {
            public NonStock() : base(ID.Type_Log.NON_STOCK)
            {
            }
        }

        public class StaffAssignment : PX.Data.BQL.BqlString.Constant<StaffAssignment>
        {
            public StaffAssignment() : base(ID.Type_Log.STAFF_ASSIGMENT)
            {
            }
        }

        public class ServiceBasedAssignment : PX.Data.BQL.BqlString.Constant<ServiceBasedAssignment>
        {
            public ServiceBasedAssignment() : base(ID.Type_Log.SERV_BASED_ASSIGMENT)
            {
            }
        }
    }
    #endregion

    #region ListField_LogActions
    public abstract class ListField_LogActions : IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.LogActions().ID_LIST,
                    new ID.LogActions().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Start : Data.BQL.BqlString.Constant<Start>
        {
            public Start() : base(ID.LogActions.START)
            {
            }
        }

        public class Complete : Data.BQL.BqlString.Constant<Complete>
        {
            public Complete() : base(ID.LogActions.COMPLETE)
            {
            }
        }
    }
    #endregion
    #endregion

    #region Contract_Billing
    public abstract class ListField_Contract_BillingFrequency : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ContractType_BillingFrequency().ID_LIST,
                    new ID.ContractType_BillingFrequency().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Every_4th_Month : PX.Data.BQL.BqlString.Constant<Every_4th_Month>
		{
            public Every_4th_Month() : base(ID.ContractType_BillingFrequency.EVERY_4TH_MONTH)
            {
            }
        }

        public class Semi_Annual : PX.Data.BQL.BqlString.Constant<Semi_Annual>
		{
            public Semi_Annual() : base(ID.ContractType_BillingFrequency.SEMI_ANNUAL)
            {
            }
        }

        public class Annual : PX.Data.BQL.BqlString.Constant<Annual>
		{
            public Annual() : base(ID.ContractType_BillingFrequency.ANNUAL)
            {
            }
        }

        public class Beg_Of_Contract : PX.Data.BQL.BqlString.Constant<Beg_Of_Contract>
		{
            public Beg_Of_Contract() : base(ID.ContractType_BillingFrequency.BEG_OF_CONTRACT)
            {
            }
        }

        public class End_Of_Contract : PX.Data.BQL.BqlString.Constant<End_Of_Contract>
		{
            public End_Of_Contract() : base(ID.ContractType_BillingFrequency.END_OF_CONTRACT)
            {
            }
        }

        public class Days_30_60_90 : PX.Data.BQL.BqlString.Constant<Days_30_60_90>
		{
            public Days_30_60_90() : base(ID.ContractType_BillingFrequency.DAYS_30_60_90)
            {
            }
        }

        public class Time_Of_Service : PX.Data.BQL.BqlString.Constant<Time_Of_Service>
		{
            public Time_Of_Service() : base(ID.ContractType_BillingFrequency.TIME_OF_SERVICE)
            {
            }
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
		{
            public None() : base(ID.ContractType_BillingFrequency.NONE)
            {
            }
        }

        public class Monthly : PX.Data.BQL.BqlString.Constant<Monthly>
		{
            public Monthly() : base(ID.ContractType_BillingFrequency.MONTHLY)
            {
            }
        }
    }

    public abstract class ListField_Contract_BillingType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Contract_BillingType().ID_LIST,
                    new ID.Contract_BillingType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class PerformedBillings : PX.Data.BQL.BqlString.Constant<PerformedBillings>
		{
            public PerformedBillings() : base(ID.Contract_BillingType.AS_PERFORMED_BILLINGS)
            {
            }
        }

        public class StandardizedBillings : PX.Data.BQL.BqlString.Constant<StandardizedBillings>
		{
            public StandardizedBillings() : base(ID.Contract_BillingType.STANDARDIZED_BILLINGS)
            {
            }
        }
    }

    public abstract class ListField_Contract_BillTo : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Contract_BillTo().ID_LIST,
                    new ID.Contract_BillTo().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class CustomerAcct : PX.Data.BQL.BqlString.Constant<CustomerAcct>
		{
            public CustomerAcct() : base(ID.Contract_BillTo.CUSTOMERACCT)
            {
            }
        }

        public class SpecificAcct : PX.Data.BQL.BqlString.Constant<SpecificAcct>
		{
            public SpecificAcct() : base(ID.Contract_BillTo.SPECIFICACCT)
            {
            }
        }
    }

    public abstract class ListField_Contract_ExpirationType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Contract_ExpirationType().ID_LIST,
                    new ID.Contract_ExpirationType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Expiring : PX.Data.BQL.BqlString.Constant<Expiring>
		{
            public Expiring() : base(ID.Contract_ExpirationType.EXPIRING)
            {
            }
        }

        public class Unlimited : PX.Data.BQL.BqlString.Constant<Unlimited>
		{
            public Unlimited() : base(ID.Contract_ExpirationType.UNLIMITED)
            {
            }
        }
    }

    public abstract class ListField_Contract_BillingPeriod : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Contract_BillingPeriod().ID_LIST,
                    new ID.Contract_BillingPeriod().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Week : PX.Data.BQL.BqlString.Constant<Week>
		{
            public Week() : base(ID.Contract_BillingPeriod.WEEK)
            {
            }
        }

        public class Month : PX.Data.BQL.BqlString.Constant<Month>
		{
            public Month() : base(ID.Contract_BillingPeriod.MONTH)
            {
            }
        }

        public class Quarter : PX.Data.BQL.BqlString.Constant<Quarter>
		{
            public Quarter() : base(ID.Contract_BillingPeriod.QUARTER)
            {
            }
        }

        public class HalfYear : PX.Data.BQL.BqlString.Constant<HalfYear>
		{
            public HalfYear() : base(ID.Contract_BillingPeriod.HALFYEAR)
            {
            }
        }

        public class Year : PX.Data.BQL.BqlString.Constant<Year>
		{
            public Year() : base(ID.Contract_BillingPeriod.YEAR)
            {
            }
        }
    }

    public abstract class ListField_Contract_SourcePrice : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SourcePrice().ID_LIST,
                    new ID.SourcePrice().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Contract : PX.Data.BQL.BqlString.Constant<Contract>
		{
            public Contract() : base(ID.SourcePrice.CONTRACT)
            {
            }
        }

        public class PriceList : PX.Data.BQL.BqlString.Constant<PriceList>
		{
            public PriceList() : base(ID.SourcePrice.PRICE_LIST)
            {
            }
        }
    }

    #endregion

    #region FuelType_Equipment
    public abstract class ListField_FuelType_Equipment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.FuelType_Equipment().ID_LIST,
                    new ID.FuelType_Equipment().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class RegularUnleaded : PX.Data.BQL.BqlString.Constant<RegularUnleaded>
		{
            public RegularUnleaded() : base(ID.FuelType_Equipment.REGULAR_UNLEADED)
            {
            }
        }

        public class PremiumUnleaded : PX.Data.BQL.BqlString.Constant<PremiumUnleaded>
		{
            public PremiumUnleaded() : base(ID.FuelType_Equipment.PREMIUM_UNLEADED)
            {
            }
        }

        public class Diesel : PX.Data.BQL.BqlString.Constant<Diesel>
		{
            public Diesel() : base(ID.FuelType_Equipment.DIESEL)
            {
            }
        }

        public class Other : PX.Data.BQL.BqlString.Constant<Other>
		{
            public Other() : base(ID.FuelType_Equipment.OTHER)
            {
            }
        }
    }
    #endregion

    #region ScheduleType
    public abstract class ListField_ScheduleType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ScheduleType().ID_LIST,
                    new ID.ScheduleType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Availability : PX.Data.BQL.BqlString.Constant<Availability>
		{
                public Availability() : base(ID.ScheduleType.AVAILABILITY)
            {
            }
            }

            public class Unavailability : PX.Data.BQL.BqlString.Constant<Unavailability>
		{
                public Unavailability() : base(ID.ScheduleType.UNAVAILABILITY)
            {
            }
            }

            public class Busy : PX.Data.BQL.BqlString.Constant<Busy>
		{
                public Busy() : base(ID.ScheduleType.BUSY)
            {
            }
            }
    }
    #endregion

    #region FrequencyType
    public abstract class ListField_FrequencyType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Schedule_FrequencyType().ID_LIST,
                    new ID.Schedule_FrequencyType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Daily : PX.Data.BQL.BqlString.Constant<Daily>
		{
            public Daily() : base(ID.Schedule_FrequencyType.DAILY)
            {
            }
        }

        public class Weekly : PX.Data.BQL.BqlString.Constant<Weekly>
		{
            public Weekly() : base(ID.Schedule_FrequencyType.WEEKLY)
            {
            }
        }

        public class Montly : PX.Data.BQL.BqlString.Constant<Montly>
		{
            public Montly() : base(ID.Schedule_FrequencyType.MONTHLY)
            {
            }
        }
    }
    #endregion

    #region OwnerType
    public abstract class ListField_OwnerType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.OwnerType().ID_LIST,
                    new ID.OwnerType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Business : PX.Data.BQL.BqlString.Constant<Business>
		{
            public Business() : base(ID.OwnerType.BUSINESS)
            {
            }
        }

        public class Employee : PX.Data.BQL.BqlString.Constant<Employee>
		{
            public Employee() : base(ID.OwnerType.EMPLOYEE)
            {
            }
        }    
    }
    #endregion

    #region EmployeeType
    public class EmployeeType
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                new string[] { CR.BAccountType.VendorType, CR.BAccountType.CustomerType, CR.BAccountType.EmployeeType, CR.BAccountType.CombinedType, CR.BAccountType.ProspectType },
                new string[] { CR.Messages.VendorType, CR.Messages.CustomerType, CR.Messages.EmployeeType, CR.Messages.CombinedType, CR.Messages.ProspectType })
            {; }
        }
    }
    #endregion

    #region BillingRule
    public abstract class ListField_BillingRule : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.BillingRule().ID_LIST,
                    new ID.BillingRule().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Time : PX.Data.BQL.BqlString.Constant<Time>
		{
            public Time() : base(ID.BillingRule.TIME)
            {
            }
        }

        public class FlatRate : PX.Data.BQL.BqlString.Constant<FlatRate>
		{
            public FlatRate() : base(ID.BillingRule.FLAT_RATE)
            {
            }
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
		{
            public None() : base(ID.BillingRule.NONE)
            {
            }
        }
    }

    public abstract class ListField_BillingRule_ContractPeriod : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ContractPeriod_BillingRule().ID_LIST,
                    new ID.ContractPeriod_BillingRule().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Time : PX.Data.BQL.BqlString.Constant<Time>
		{
            public Time() : base(ID.BillingRule.TIME)
            {
            }
        }

        public class FlatRate : PX.Data.BQL.BqlString.Constant<FlatRate>
		{
            public FlatRate() : base(ID.BillingRule.FLAT_RATE)
            {
            }
        }
    }

    public abstract class ListField_ContractPeriod_Actions : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ContractPeriod_Actions().ID_LIST,
                    new ID.ContractPeriod_Actions().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class SearchBillingPeriod : PX.Data.BQL.BqlString.Constant<SearchBillingPeriod>
		{
            public SearchBillingPeriod() : base(ID.ContractPeriod_Actions.SEARCH_BILLING_PERIOD)
            {
            }
        }

        public class ModifyBillingPeriod : PX.Data.BQL.BqlString.Constant<ModifyBillingPeriod>
		{
            public ModifyBillingPeriod() : base(ID.ContractPeriod_Actions.MODIFY_UPCOMING_BILLING_PERIOD)
            {
            }
        }
    }
    #endregion

    #region Priority_ServiceOrder
    public abstract class ListField_Priority_ServiceOrder : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Priority_ServiceOrder().ID_LIST,
                    new ID.Priority_ServiceOrder().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Low : PX.Data.BQL.BqlString.Constant<Low>
		{
            public Low() : base(ID.Priority_ServiceOrder.LOW)
            {
            }
        }

        public class Medium : PX.Data.BQL.BqlString.Constant<Medium>
		{
            public Medium() : base(ID.Priority_ServiceOrder.MEDIUM)
            {
            }
        }

        public class High : PX.Data.BQL.BqlString.Constant<High>
		{
            public High() : base(ID.Priority_ServiceOrder.HIGH)
            {
            }
        }
    }
    #endregion

    #region Severity_ServiceOrder
    public abstract class ListField_Severity_ServiceOrder : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Severity_ServiceOrder().ID_LIST,
                    new ID.Severity_ServiceOrder().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Low : PX.Data.BQL.BqlString.Constant<Low>
		{
            public Low() : base(ID.Severity_ServiceOrder.LOW)
            {
            }
        }

        public class Medium : PX.Data.BQL.BqlString.Constant<Medium>
		{
            public Medium() : base(ID.Severity_ServiceOrder.MEDIUM)
            {
            }
        }

        public class High : PX.Data.BQL.BqlString.Constant<High>
		{
            public High() : base(ID.Severity_ServiceOrder.HIGH)
            {
            }
        }
    }
    #endregion

    #region SourceType_ServiceOrder
    public abstract class ListField_SourceType_ServiceOrder : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SourceType_ServiceOrder().ID_LIST,
                    new ID.SourceType_ServiceOrder().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Case : PX.Data.BQL.BqlString.Constant<Case>
		{
            public Case() : base(ID.SourceType_ServiceOrder.CASE)
            {
            }
        }

        public class Opportunity : PX.Data.BQL.BqlString.Constant<Opportunity>
		{
            public Opportunity() : base(ID.SourceType_ServiceOrder.OPPORTUNITY)
            {
            }
        }

        public class SalesOrder : PX.Data.BQL.BqlString.Constant<SalesOrder>
		{
            public SalesOrder() : base(ID.SourceType_ServiceOrder.SALES_ORDER)
            {
            }
        }

        public class ServiceDispatch : PX.Data.BQL.BqlString.Constant<ServiceDispatch>
		{
            public ServiceDispatch() : base(ID.SourceType_ServiceOrder.SERVICE_DISPATCH)
            {
            }
        }
    }
    #endregion

    #region Status_ServiceOrder
    public abstract class ListField_Status_ServiceOrder : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_ServiceOrder().ID_LIST,
                    new ID.Status_ServiceOrder().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Open : PX.Data.BQL.BqlString.Constant<Open>
		{
            public Open() : base(ID.Status_ServiceOrder.OPEN)
            {
            }
        }

        public class Quote : PX.Data.BQL.BqlString.Constant<Quote>
		{
            public Quote() : base(ID.Status_ServiceOrder.QUOTE)
            {
            }
        }

        public class Hold : PX.Data.BQL.BqlString.Constant<Hold>
		{
            public Hold() : base(ID.Status_ServiceOrder.ON_HOLD)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
		{
            public Canceled() : base(ID.Status_ServiceOrder.CANCELED)
            {
            }
        }

        public class Closed : PX.Data.BQL.BqlString.Constant<Closed>
		{
            public Closed() : base(ID.Status_ServiceOrder.CLOSED)
            {
            }
        }

        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
		{
            public Completed() : base(ID.Status_ServiceOrder.COMPLETED)
            {
            }
        }
    }
    #endregion

    #region Status_ServiceContract
    public abstract class ListField_Status_ServiceContract : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_ServiceContract().ID_LIST,
                    new ID.Status_ServiceContract().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Draft : PX.Data.BQL.BqlString.Constant<Draft>
		{
            public Draft() : base(ID.Status_ServiceContract.DRAFT)
            {
            }
        }

        public class Active : PX.Data.BQL.BqlString.Constant<Active>
		{
            public Active() : base(ID.Status_ServiceContract.ACTIVE)
            {
            }
        }

        public class Suspended : PX.Data.BQL.BqlString.Constant<Suspended>
		{
            public Suspended() : base(ID.Status_ServiceContract.SUSPENDED)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
		{
            public Canceled() : base(ID.Status_ServiceContract.CANCELED)
            {
            }
        }

        public class Expired : PX.Data.BQL.BqlString.Constant<Expired>
		{
            public Expired() : base(ID.Status_ServiceContract.EXPIRED)
            {
            }
        }
    }
    #endregion

    #region Status_ContractPeriod
    public abstract class ListField_Status_ContractPeriod : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_ContractPeriod().ID_LIST,
                    new ID.Status_ContractPeriod().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Active : PX.Data.BQL.BqlString.Constant<Active>
		{
            public Active() : base(ID.Status_ContractPeriod.ACTIVE)
            {
            }
        }

        public class Inactive : PX.Data.BQL.BqlString.Constant<Inactive>
		{
            public Inactive() : base(ID.Status_ContractPeriod.INACTIVE)
            {
            }
        }

        public class Pending : PX.Data.BQL.BqlString.Constant<Pending>
		{
            public Pending() : base(ID.Status_ContractPeriod.PENDING)
            {
            }
        }

        public class Invoiced : PX.Data.BQL.BqlString.Constant<Invoiced>
		{
            public Invoiced() : base(ID.Status_ContractPeriod.INVOICED)
            {
            }
        }
    }
    #endregion

    #region RecordType_ContractAction
    public abstract class ListField_RecordType_ContractAction : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.RecordType_ContractAction().ID_LIST,
                    new ID.RecordType_ContractAction().TX_LIST)
            {
            }
        }
    }
    #endregion

    #region RecordType_ContractAction
    public abstract class ListField_Action_ContractAction : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Action_ContractAction().ID_LIST,
                    new ID.Action_ContractAction().TX_LIST)
            {
            }
        }
    }
    #endregion

    #region Confirmed_Appointment
    public abstract class ListField_Confirmed_Appointment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Confirmed_Appointment().ID_LIST,
                    new ID.Confirmed_Appointment().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class All : PX.Data.BQL.BqlString.Constant<All>
		{
            public All() : base(ID.Confirmed_Appointment.ALL)
            {
            }
        }

        public class Confirmed : PX.Data.BQL.BqlString.Constant<Confirmed>
		{
            public Confirmed() : base(ID.Confirmed_Appointment.CONFIRMED)
            {
            }
        }

        public class Not_Confirmed : PX.Data.BQL.BqlString.Constant<Not_Confirmed>
		{
            public Not_Confirmed() : base(ID.Confirmed_Appointment.NOT_CONFIRMED)
            {
            }
        }
    }
    #endregion

    #region Period_Appointment
    public abstract class ListField_Period_Appointment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.PeriodType().ID_LIST,
                    new ID.PeriodType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Day : PX.Data.BQL.BqlString.Constant<Day>
		{
            public Day() : base(ID.PeriodType.DAY)
            {
            }
        }

        public class Week : PX.Data.BQL.BqlString.Constant<Week>
		{
            public Week() : base(ID.PeriodType.WEEK)
            {
            }
        }

        public class Month : PX.Data.BQL.BqlString.Constant<Month>
		{
            public Month() : base(ID.PeriodType.MONTH)
            {
            }
        }
    }
    #endregion

    #region ReasonType
    public abstract class ListField_ReasonType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ReasonType().ID_LIST,
                    new ID.ReasonType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Cancel_Service_Order : PX.Data.BQL.BqlString.Constant<Cancel_Service_Order>
		{
            public Cancel_Service_Order() : base(ID.ReasonType.CANCEL_SERVICE_ORDER)
            {
            }
        }

        public class Cancel_Appointment : PX.Data.BQL.BqlString.Constant<Cancel_Appointment>
		{
            public Cancel_Appointment() : base(ID.ReasonType.CANCEL_APPOINTMENT)
            {
            }
        }

        public class Workflow_Stage : PX.Data.BQL.BqlString.Constant<Workflow_Stage>
		{
            public Workflow_Stage() : base(ID.ReasonType.WORKFLOW_STAGE)
            {
            }
        }

        public class Appointment_Detail : PX.Data.BQL.BqlString.Constant<Appointment_Detail>
		{
            public Appointment_Detail() : base(ID.ReasonType.APPOINTMENT_DETAIL)
            {
            }
        }

        public class General : PX.Data.BQL.BqlString.Constant<General>
		{
            public General() : base(ID.ReasonType.GENERAL)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_Behavior
    public abstract class ListField_SrvOrdType_RecordType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SrvOrdType_RecordType().ID_LIST,
                    new ID.SrvOrdType_RecordType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class ServiceOrder : PX.Data.BQL.BqlString.Constant<ServiceOrder>
		{
            public ServiceOrder() : base(ID.SrvOrdType_RecordType.SERVICE_ORDER)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_GenerateInvoiceBy
    public abstract class ListField_SrvOrdType_GenerateInvoiceBy : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SrvOrdType_GenerateInvoiceBy().ID_LIST,
                    new ID.SrvOrdType_GenerateInvoiceBy().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class CrmAr : PX.Data.BQL.BqlString.Constant<CrmAr>
		{
            public CrmAr() : base(ID.SrvOrdType_GenerateInvoiceBy.CRM_AR)
            {
            }
        }

        public class SalesOrder : PX.Data.BQL.BqlString.Constant<SalesOrder>
		{
            public SalesOrder() : base(ID.SrvOrdType_GenerateInvoiceBy.SALES_ORDER)
            {
            }
        }

        public class Project : PX.Data.BQL.BqlString.Constant<Project>
		{
            public Project() : base(ID.SrvOrdType_GenerateInvoiceBy.PROJECT)
            {
            }
        }

        public class NotBillable : PX.Data.BQL.BqlString.Constant<NotBillable>
		{
            public NotBillable() : base(ID.SrvOrdType_GenerateInvoiceBy.NOT_BILLABLE)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_BillingType
    public abstract class ListField_SrvOrdType_BillingType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SrvOrdType_BillingType().ID_LIST,
                    new ID.SrvOrdType_BillingType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class CostAsCost : PX.Data.BQL.BqlString.Constant<CostAsCost>
        {
            public CostAsCost() : base(ID.SrvOrdType_BillingType.COST_AS_COST)
            {
            }
        }

        public class CostAsRevenue : PX.Data.BQL.BqlString.Constant<CostAsRevenue>
        {
            public CostAsRevenue() : base(ID.SrvOrdType_BillingType.COST_AS_REVENUE)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_NewBusinessAcctType
    public abstract class ListField_SrvOrdType_NewBusinessAcctType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.BusinessAcctType().ID_LIST,
                    new ID.BusinessAcctType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Customer : PX.Data.BQL.BqlString.Constant<Customer>
		{
            public Customer() : base(ID.BusinessAcctType.CUSTOMER)
            {
            }
        }

        public class Prospect : PX.Data.BQL.BqlString.Constant<Prospect>
		{
            public Prospect() : base(ID.BusinessAcctType.PROSPECT)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_SalesAcctSource
    public abstract class ListField_SrvOrdType_SalesAcctSource : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public static string[] GetIDList()
            {
                if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>()
                        && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                {
                    return new string[] { ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM, ID.SrvOrdType_SalesAcctSource.POSTING_CLASS, ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION };
                }
                else
                {
                    return new ID.SrvOrdType_SalesAcctSource().ID_LIST;
                }
        }

            public static string[] GetTXList()
            {
                    if (!PXAccess.FeatureInstalled<FeaturesSet.warehouse>()
                            && !PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>())
                    {
                        return new string[] { TX.SrvOrdType_SalesAcctSource.INVENTORY_ITEM, TX.SrvOrdType_SalesAcctSource.POSTING_CLASS, TX.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION };
                    }
                    else
                {
                        return new ID.SrvOrdType_SalesAcctSource().TX_LIST;
                }
            }

            public ListAtrribute()
                : base(
                    GetIDList(),
                    GetTXList())
            {
            }

            public override void CacheAttached(PXCache sender)
            {
                _AllowedValues = GetIDList();
                _AllowedLabels = GetTXList();
                _NeutralAllowedLabels = _AllowedLabels;

                base.CacheAttached(sender);
            }
        }

        //BQL constant declaration
        public class INVENTORY_ITEM : PX.Data.BQL.BqlString.Constant<INVENTORY_ITEM>
		{
            public INVENTORY_ITEM() : base(ID.SrvOrdType_SalesAcctSource.INVENTORY_ITEM)
            {
            }
        }
        public class WAREHOUSE : PX.Data.BQL.BqlString.Constant<WAREHOUSE>
		{
            public WAREHOUSE() : base(ID.SrvOrdType_SalesAcctSource.WAREHOUSE)
            {
            }
        }
        public class POSTING_CLASS : PX.Data.BQL.BqlString.Constant<POSTING_CLASS>
		{
            public POSTING_CLASS() : base(ID.SrvOrdType_SalesAcctSource.POSTING_CLASS)
            {
            }
        }
        public class CUSTOMER_LOCATION : PX.Data.BQL.BqlString.Constant<CUSTOMER_LOCATION>
		{
            public CUSTOMER_LOCATION() : base(ID.SrvOrdType_SalesAcctSource.CUSTOMER_LOCATION)
            {
            }
        }
    }
    #endregion

    #region SrvOrdType_AddressSource
    public abstract class ListField_SrvOrdType_AddressSource : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SrvOrdType_AppAddressSource().ID_LIST,
                    new ID.SrvOrdType_AppAddressSource().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class BUSINESS_ACCOUNT : PX.Data.BQL.BqlString.Constant<BUSINESS_ACCOUNT>
		{
            public BUSINESS_ACCOUNT() : base(ID.SrvOrdType_AppAddressSource.BUSINESS_ACCOUNT)
            {
            }
        }

        public class CUSTOMER_CONTACT : PX.Data.BQL.BqlString.Constant<CUSTOMER_CONTACT>
		{
            public CUSTOMER_CONTACT() : base(ID.SrvOrdType_AppAddressSource.CUSTOMER_CONTACT)
            {
            }
        }

        public class BRANCH_LOCATION : PX.Data.BQL.BqlString.Constant<BRANCH_LOCATION>
		{
            public BRANCH_LOCATION() : base(ID.SrvOrdType_AppAddressSource.BRANCH_LOCATION)
            {
            }
        }
    }
    #endregion

    #region OnCompleteApptSetInProcessItemsAs
    public abstract class ListField_OnCompleteApptSetInProcessItemsAs
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(new[]
                {
                    Pair(NotFinished, TX.OnCompleteApptSetInProcessItemsAs.NotFinished),
                    Pair(Completed, TX.OnCompleteApptSetInProcessItemsAs.Completed),
                    Pair(DoNothing, TX.OnCompleteApptSetInProcessItemsAs.DoNothing),
                })
            {
            }
        }

        public const string DoNothing = "DN";
        public const string Completed = "CP";
        public const string NotFinished = "NF";

        public class doNothing : PX.Data.BQL.BqlString.Constant<doNothing>
        {
            public doNothing() : base(DoNothing)
            {
            }
        }

        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed)
            {
            }
        }

        public class notFinished : PX.Data.BQL.BqlString.Constant<notFinished>
        {
            public notFinished() : base(NotFinished)
            {
            }
        }
    }
    #endregion

    #region OnCompleteApptSetNotStartedItemsAs
    public abstract class ListField_OnCompleteApptSetNotStartedItemsAs
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(new[]
                {
                    Pair(NotPerformed, TX.OnCompleteApptSetNotStartedItemsAs.NotPerformed),
                    Pair(Completed, TX.OnCompleteApptSetNotStartedItemsAs.Completed),
                    Pair(DoNothing, TX.OnCompleteApptSetNotStartedItemsAs.DoNothing),
                })
            {
            }
        }

        public const string NotPerformed = "NP";
        public const string Completed = ListField_OnCompleteApptSetInProcessItemsAs.Completed;
        public const string DoNothing = ListField_OnCompleteApptSetInProcessItemsAs.DoNothing;

        public class doNothing : PX.Data.BQL.BqlString.Constant<doNothing>
        {
            public doNothing() : base(DoNothing)
            {
            }
        }

        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(Completed)
            {
            }
        }

        public class notPerformed : PX.Data.BQL.BqlString.Constant<notPerformed>
        {
            public notPerformed() : base(NotPerformed)
            {
            }
        }
    }
    #endregion

    #region Setup_AppResizePrecision
    public abstract class ListField_AppResizePrecision : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.AppResizePrecision_Setup().ID_LIST,
                    new ID.AppResizePrecision_Setup().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class MINUTES_10 : PX.Data.BQL.BqlInt.Constant<MINUTES_10>
		{
            public MINUTES_10() : base(ID.TimeConstants.MINUTES_10)
            {
            }
        }

        public class MINUTES_15 : PX.Data.BQL.BqlInt.Constant<MINUTES_15>
		{
            public MINUTES_15() : base(ID.TimeConstants.MINUTES_15)
            {
            }
        }

        public class MINUTES_30 : PX.Data.BQL.BqlInt.Constant<MINUTES_30>
		{
            public MINUTES_30() : base(ID.TimeConstants.MINUTES_30)
            {
            }
        }

        public class MINUTES_60 : PX.Data.BQL.BqlInt.Constant<MINUTES_60>
		{
            public MINUTES_60() : base(ID.TimeConstants.MINUTES_60)
            {
            }
        }
    }
    #endregion

    #region Setup_AppointmentValidation
    public abstract class ListField_AppointmentValidation : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.ValidationType().ID_LIST,
                    new ID.ValidationType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class NOT_VALIDATE : PX.Data.BQL.BqlString.Constant<NOT_VALIDATE>
		{
            public NOT_VALIDATE() : base(ID.ValidationType.NOT_VALIDATE)
            {
            }
        }

        public class WARN : PX.Data.BQL.BqlString.Constant<WARN>
		{
            public WARN() : base(ID.ValidationType.WARN)
            {
            }
        }

        public class PREVENT : PX.Data.BQL.BqlString.Constant<PREVENT>
		{
            public PREVENT() : base(ID.ValidationType.PREVENT)
            {
            }
        }
    }
    #endregion

    #region Setup_DfltCalendarMode
    public abstract class ListField_DfltCalendarViewMode : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.DfltCalendarViewMode_Setup().ID_LIST,
                    new ID.DfltCalendarViewMode_Setup().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class VERTICAL : PX.Data.BQL.BqlString.Constant<VERTICAL>
        {
            public VERTICAL() : base(ID.DfltCalendarViewMode_Setup.VERTICAL)
            {
            }
        }

        public class HORIZONTAL : PX.Data.BQL.BqlString.Constant<HORIZONTAL>
        {
            public HORIZONTAL() : base(ID.DfltCalendarViewMode_Setup.HORIZONTAL)
            {
            }
        }
    }
    #endregion

    #region Schedule_EntityType
    public abstract class ListField_Schedule_EntityType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Schedule_EntityType().ID_LIST,
                    new ID.Schedule_EntityType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Contract : PX.Data.BQL.BqlString.Constant<Contract>
		{
            public Contract() : base(ID.Schedule_EntityType.CONTRACT)
            {
            }
        }

        public class Employee : PX.Data.BQL.BqlString.Constant<Employee>
		{
            public Employee() : base(ID.Schedule_EntityType.EMPLOYEE)
            {
            }
        }
    }
    #endregion

    #region PostDoc_EntityType
    public abstract class ListField_PostDoc_EntityType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.PostDoc_EntityType().ID_LIST,
                    new ID.PostDoc_EntityType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Appointment : PX.Data.BQL.BqlString.Constant<Appointment>
		{
            public Appointment() : base(ID.PostDoc_EntityType.APPOINTMENT)
            {
            }
        }

        public class Service_Order : PX.Data.BQL.BqlString.Constant<Service_Order>
		{
            public Service_Order() : base(ID.PostDoc_EntityType.SERVICE_ORDER)
            {
            }
        }
    }
    #endregion

    #region WeekDays
    public abstract class ListField_WeekDays : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.WeekDays().ID_LIST,
                    new ID.WeekDays().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class ANY : PX.Data.BQL.BqlString.Constant<ANY>
		{
            public ANY() : base(ID.WeekDays.ANYDAY)
            {
            }
        }

        public class SUNDAY : PX.Data.BQL.BqlString.Constant<SUNDAY>
		{
            public SUNDAY() : base(ID.WeekDays.SUNDAY)
            {
            }
        }

        public class MONDAY : PX.Data.BQL.BqlString.Constant<MONDAY>
		{
            public MONDAY() : base(ID.WeekDays.MONDAY)
            {
            }
        }

        public class TUESDAY : PX.Data.BQL.BqlString.Constant<TUESDAY>
		{
            public TUESDAY() : base(ID.WeekDays.TUESDAY)
            {
            }
        }

        public class WEDNESDAY : PX.Data.BQL.BqlString.Constant<WEDNESDAY>
		{
            public WEDNESDAY() : base(ID.WeekDays.WEDNESDAY)
            {
            }
        }

        public class THURSDAY : PX.Data.BQL.BqlString.Constant<THURSDAY>
		{
            public THURSDAY() : base(ID.WeekDays.THURSDAY)
            {
            }
        }

        public class FRIDAY : PX.Data.BQL.BqlString.Constant<FRIDAY>
		{
            public FRIDAY() : base(ID.WeekDays.FRIDAY)
            {
            }
        }

        public class SATURDAY : PX.Data.BQL.BqlString.Constant<SATURDAY>
		{
            public SATURDAY() : base(ID.WeekDays.SATURDAY)
            {
            }
        }
    }

    public abstract class ListField_WeekDaysNumber : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.WeekDaysNumber().ID_LIST,
                    new ID.WeekDaysNumber().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class SUNDAY : PX.Data.BQL.BqlInt.Constant<SUNDAY>
		{
            public SUNDAY() : base(ID.WeekDaysNumber.SUNDAY)
            {
            }
        }

        public class MONDAY : PX.Data.BQL.BqlInt.Constant<MONDAY>
		{
            public MONDAY() : base(ID.WeekDaysNumber.MONDAY)
            {
            }
        }

        public class TUESDAY : PX.Data.BQL.BqlInt.Constant<TUESDAY>
		{
            public TUESDAY() : base(ID.WeekDaysNumber.TUESDAY)
            {
            }
        }

        public class WEDNESDAY : PX.Data.BQL.BqlInt.Constant<WEDNESDAY>
		{
            public WEDNESDAY() : base(ID.WeekDaysNumber.WEDNESDAY)
            {
            }
        }

        public class THURSDAY : PX.Data.BQL.BqlInt.Constant<THURSDAY>
		{
            public THURSDAY() : base(ID.WeekDaysNumber.THURSDAY)
            {
            }
        }

        public class FRIDAY : PX.Data.BQL.BqlInt.Constant<FRIDAY>
		{
            public FRIDAY() : base(ID.WeekDaysNumber.FRIDAY)
            {
            }
        }

        public class SATURDAY : PX.Data.BQL.BqlInt.Constant<SATURDAY>
		{
            public SATURDAY() : base(ID.WeekDaysNumber.SATURDAY)
            {
            }
        }
    }
    #endregion

    #region WarrantyDurationType
    public abstract class ListField_WarrantyDurationType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.WarrantyDurationType().ID_LIST,
                    new ID.WarrantyDurationType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Day : PX.Data.BQL.BqlString.Constant<Day>
		{
            public Day() : base(ID.WarrantyDurationType.DAY)
            {
            }
        }

        public class Month : PX.Data.BQL.BqlString.Constant<Month>
		{
            public Month() : base(ID.WarrantyDurationType.MONTH)
            {
            }
        }

        public class Year : PX.Data.BQL.BqlString.Constant<Year>
		{
            public Year() : base(ID.WarrantyDurationType.YEAR)
            {
            }
        }
    }
    #endregion

    #region WarrantyApplicationOrder
    public abstract class ListField_WarrantyApplicationOrder : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.WarrantyApplicationOrder().ID_LIST,
                    new ID.WarrantyApplicationOrder().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Company : PX.Data.BQL.BqlString.Constant<Company>
		{
            public Company() : base(ID.WarrantyApplicationOrder.COMPANY)
            {
            }
        }

        public class Vendor : PX.Data.BQL.BqlString.Constant<Vendor>
		{
            public Vendor() : base(ID.WarrantyApplicationOrder.VENDOR)
            {
            }
        }
    }
    #endregion

    #region ModelType
    public abstract class ListField_ModelType : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ModelType().ID_LIST,
                    new ID.ModelType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Equipment : PX.Data.BQL.BqlString.Constant<Equipment>
		{
            public Equipment() : base(ID.ModelType.EQUIPMENT)
            {
            }
        }

        public class Replacement : PX.Data.BQL.BqlString.Constant<Replacement>
		{
            public Replacement() : base(ID.ModelType.REPLACEMENT)
            {
            }
        }
    }
    #endregion

    #region SourceType_Equipment
    public abstract class ListField_SourceType_Equipment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SourceType_Equipment().ID_LIST,
                    new ID.SourceType_Equipment().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class SM_Equipment : PX.Data.BQL.BqlString.Constant<SM_Equipment>
		{
            public SM_Equipment()
                : base(ID.SourceType_Equipment.SM_EQUIPMENT)
            {
            }
        }

        public class Vehicle : PX.Data.BQL.BqlString.Constant<Vehicle>
		{
            public Vehicle()
                : base(ID.SourceType_Equipment.VEHICLE)
            {
            }
        }

        public class EP_Equipment : PX.Data.BQL.BqlString.Constant<EP_Equipment>
		{
            public EP_Equipment()
                : base(ID.SourceType_Equipment.EP_EQUIPMENT)
            {
            }
        }

        public class AR_INVOICE : PX.Data.BQL.BqlString.Constant<AR_INVOICE>
		{
            public AR_INVOICE()
                : base(ID.SourceType_Equipment.AR_INVOICE)
            {
            }
        }
    }

    public abstract class ListField_SourceType_Equipment_ALL : ListField_SourceType_Equipment
    {
        public new class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.SourceType_Equipment_ALL().ID_LIST,
                    new ID.SourceType_Equipment_ALL().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class All : PX.Data.BQL.BqlString.Constant<All>
		{
            public All() : base(ID.SourceType_Equipment_ALL.ALL)
            {
            }
        }
    }    
    #endregion

    #region OwnerType_Equiment

    public abstract class ListField_OwnerType_Equipment : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.OwnerType_Equipment().ID_LIST,
                    new ID.OwnerType_Equipment().TX_LIST)
            {
            }
        }

        public class OwnCompany : PX.Data.BQL.BqlString.Constant<OwnCompany>
		{
            public OwnCompany()
                : base(ID.OwnerType_Equipment.OWN_COMPANY)
            {
            }
        }

        public class Customer : PX.Data.BQL.BqlString.Constant<Customer>
		{
            public Customer()
                : base(ID.OwnerType_Equipment.CUSTOMER)
            {
            }
        }
    }

    #endregion

    #region Routes
    #region Status
    public abstract class ListField_Status_Route : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_Route().ID_LIST,
                    new ID.Status_Route().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Open : PX.Data.BQL.BqlString.Constant<Open>
		{
            public Open() : base(ID.Status_Route.OPEN)
            {
            }
        }

        public class InProcess : PX.Data.BQL.BqlString.Constant<InProcess>
		{
            public InProcess() : base(ID.Status_Route.IN_PROCESS)
            {
            }
        }

        public class Canceled : PX.Data.BQL.BqlString.Constant<Canceled>
		{
            public Canceled() : base(ID.Status_Route.CANCELED)
            {
            }
        }

        public class Completed : PX.Data.BQL.BqlString.Constant<Completed>
		{
            public Completed() : base(ID.Status_Route.COMPLETED)
            {
            }
        }

        public class Closed : PX.Data.BQL.BqlString.Constant<Closed>
		{
            public Closed() : base(ID.Status_Route.CLOSED)
            {
            }
        }
    }
    #endregion
    #endregion

    #region ListField_RecordType_ContractSchedule
    public abstract class ListField_RecordType_ContractSchedule : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.RecordType_ServiceContract().ID_LIST,
                    new ID.RecordType_ServiceContract().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class ServiceContract : PX.Data.BQL.BqlString.Constant<ServiceContract>
		{
            public ServiceContract() : base(ID.RecordType_ServiceContract.SERVICE_CONTRACT)
            {
            }
        }

        public class RouteServiceContract : PX.Data.BQL.BqlString.Constant<RouteServiceContract>
		{
            public RouteServiceContract() : base(ID.RecordType_ServiceContract.ROUTE_SERVICE_CONTRACT)
            {
            }
        }

        public class EmployeeScheduleContract : PX.Data.BQL.BqlString.Constant<EmployeeScheduleContract>
		{
            public EmployeeScheduleContract() : base(ID.RecordType_ServiceContract.EMPLOYEE_SCHEDULE_CONTRACT)
            {
            }
        }
    }
    #endregion

    #region ListField_ScheduleGenType_ContractSchedule
    public abstract class ListField_ScheduleGenType_ContractSchedule : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ScheduleGenType_ServiceContract().ID_LIST,
                    new ID.ScheduleGenType_ServiceContract().TX_LIST)
            {
            }
        }

        public class ServiceOrder : PX.Data.BQL.BqlString.Constant<ServiceOrder>
		{
            public ServiceOrder() : base(ID.ScheduleGenType_ServiceContract.SERVICE_ORDER)
            {
            }
        }

        public class Appointment : PX.Data.BQL.BqlString.Constant<Appointment>
		{
            public Appointment() : base(ID.ScheduleGenType_ServiceContract.APPOINTMENT)
            {
            }
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
		{
            public None() : base(ID.ScheduleGenType_ServiceContract.NONE)
            {
            }
        }
    }
    #endregion

    #region ListField_Behavior_SrvOrdType
    public abstract class ListField_Behavior_SrvOrdType : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.Behavior_SrvOrderType().ID_LIST,
                    new ID.Behavior_SrvOrderType().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class RegularAppointment : PX.Data.BQL.BqlString.Constant<RegularAppointment>
		{
            public RegularAppointment() : base(ID.Behavior_SrvOrderType.REGULAR_APPOINTMENT)
            {
            }
        }

        public class RouteAppointment : PX.Data.BQL.BqlString.Constant<RouteAppointment>
		{
            public RouteAppointment() : base(ID.Behavior_SrvOrderType.ROUTE_APPOINTMENT)
            {
            }
        }

        public class InternalAppointment : PX.Data.BQL.BqlString.Constant<InternalAppointment>
		{
            public InternalAppointment() : base(ID.Behavior_SrvOrderType.INTERNAL_APPOINTMENT)
            {
            }
        }

        public class Quote : PX.Data.BQL.BqlString.Constant<Quote>
		{
            public Quote() : base(ID.Behavior_SrvOrderType.QUOTE)
            {
            }
        }
    }
    #endregion

    #region ListField_SlotLevel_TimeSlot
    public abstract class ListField_SlotLevel_TimeSlot : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.EmployeeTimeSlotLevel().ID_LIST,
                    new ID.EmployeeTimeSlotLevel().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Base : PX.Data.BQL.BqlInt.Constant<Base>
		{
            public Base() : base(ID.EmployeeTimeSlotLevel.BASE)
            {
            }
        }

        public class Compress : PX.Data.BQL.BqlInt.Constant<Compress>
		{
            public Compress() : base(ID.EmployeeTimeSlotLevel.COMPRESS)
            {
            }
        }
    }
    #endregion

    #region ListField_CloningType_CloneAppointment
    public abstract class ListField_CloningType_CloneAppointment : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.CloningType_CloneAppointment().ID_LIST,
                    new ID.CloningType_CloneAppointment().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class Single : PX.Data.BQL.BqlString.Constant<Single>
		{
            public Single() : base(ID.CloningType_CloneAppointment.SINGLE)
            {
            }
        }

        public class Multiple : PX.Data.BQL.BqlString.Constant<Multiple>
		{
            public Multiple() : base(ID.CloningType_CloneAppointment.MULTIPLE)
            {
            }
        }
    }
    #endregion

    #region ListField_PreAcctSource_Setup
    public abstract class ListField_PreAcctSource_Setup : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.PreAcctSource_Setup().ID_LIST,
                    new ID.PreAcctSource_Setup().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class CustomerLocation : PX.Data.BQL.BqlString.Constant<CustomerLocation>
		{
            public CustomerLocation() : base(ID.PreAcctSource_Setup.CUSTOMER_LOCATION)
            {
            }
        }

        public class InventoryItem : PX.Data.BQL.BqlString.Constant<InventoryItem>
		{
            public InventoryItem() : base(ID.PreAcctSource_Setup.INVENTORY_ITEM)
            {
            }
        }
    }
    #endregion

	#region ListField_ActionType_ProcessServiceContract
    public abstract class ListField_ActionType_ProcessServiceContracts : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ActionType_ProcessServiceContracts().ID_LIST,
                    new ID.ActionType_ProcessServiceContracts().TX_LIST)
            {
            }
        }
    }
    #endregion

	#region ListField_Contract_SalesAcctSource
    public abstract class ListField_Contract_SalesAcctSource : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Contract_SalesAcctSource().ID_LIST,
                    new ID.Contract_SalesAcctSource().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class CUSTOMER_LOCATION : PX.Data.BQL.BqlString.Constant<CUSTOMER_LOCATION>
		{
            public CUSTOMER_LOCATION() : base(ID.Contract_SalesAcctSource.CUSTOMER_LOCATION)
            {
            }
        }

        public class POSTING_CLASS : PX.Data.BQL.BqlString.Constant<POSTING_CLASS>
        {
            public POSTING_CLASS() : base(ID.Contract_SalesAcctSource.POSTING_CLASS)
            {
            }
        }

        public class INVENTORY_ITEM : PX.Data.BQL.BqlString.Constant<INVENTORY_ITEM>
		{
            public INVENTORY_ITEM() : base(ID.Contract_SalesAcctSource.INVENTORY_ITEM)
            {
            }
        }
    }
    #endregion

    #region EmailContactType
    public class ApptContactType : NotificationContactType
    {
        /// <summary>
        /// Defines a list of the possible ContactType for the AR Customer <br/>
        /// Namely: Primary, Billing, Employee, Customer, Employee Staff, Vendor Staff <br/>
        /// Mostly, this attribute serves as a container <br/>
        /// </summary>		
        public class ClassListAttribute : PXStringListAttribute
        {
            public ClassListAttribute()
                : base(
                        new ID.ContactType_ApptMail().ID_LIST,
                        new ID.ContactType_ApptMail().TX_LIST)
            {
            }
        }

        public new class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.ContactType_ApptMail().ID_LIST,
                    new ID.ContactType_ApptMail().TX_LIST)
            {
            }
        }
    }
    #endregion

    #region Service_Action_Type
    public abstract class ListField_Service_Action_Type : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.Service_Action_Type().ID_LIST,
                    new ID.Service_Action_Type().TX_LIST)
            {
            }
        }

        public class No_Items_Related : PX.Data.BQL.BqlString.Constant<No_Items_Related>
		{
            public No_Items_Related() : base(ID.Service_Action_Type.NO_ITEMS_RELATED)
            {
            }
        }

        public class Picked_Up_Items : PX.Data.BQL.BqlString.Constant<Picked_Up_Items>
		{
            public Picked_Up_Items() : base(ID.Service_Action_Type.PICKED_UP_ITEMS)
            {
            }
        }

        public class Delivered_Items : PX.Data.BQL.BqlString.Constant<Delivered_Items>
		{
            public Delivered_Items() : base(ID.Service_Action_Type.DELIVERED_ITEMS)
            {
            }
        }
    }

    public abstract class ListField_Appointment_Service_Action_Type : ListField_Service_Action_Type
    {
        public new class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new ID.Service_Action_Type().ID_LIST,
                    new ID.Appointment_Service_Action_Type().TX_LIST)
            {
            }
        }

        public new class No_Items_Related : PX.Data.BQL.BqlString.Constant<No_Items_Related>
		{
            public No_Items_Related() : base(ID.Appointment_Service_Action_Type.NO_ITEMS_RELATED)
            {
            }
        }

        public new class Picked_Up_Items : PX.Data.BQL.BqlString.Constant<Picked_Up_Items>
		{
            public Picked_Up_Items() : base(ID.Appointment_Service_Action_Type.PICKED_UP_ITEMS)
            {
            }
        }

        public new class Delivered_Items : PX.Data.BQL.BqlString.Constant<Delivered_Items>
		{
            public Delivered_Items() : base(ID.Appointment_Service_Action_Type.DELIVERED_ITEMS)
            {
            }
        }
    }
    #endregion

    #region Months
    public abstract class ListField_Month : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Months().ID_LIST,
                    new ID.Months().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class JANUARY : PX.Data.BQL.BqlString.Constant<JANUARY>
		{
            public JANUARY() : base(ID.Months.JANUARY)
            {
            }
        }

        public class FEBRUARY : PX.Data.BQL.BqlString.Constant<FEBRUARY>
		{
            public FEBRUARY() : base(ID.Months.FEBRUARY)
            {
            }
        }

        public class MARCH : PX.Data.BQL.BqlString.Constant<MARCH>
		{
            public MARCH() : base(ID.Months.MARCH)
            {
            }
        }

        public class APRIL : PX.Data.BQL.BqlString.Constant<APRIL>
		{
            public APRIL() : base(ID.Months.APRIL)
            {
            }
        }

        public class MAY : PX.Data.BQL.BqlString.Constant<MAY>
		{
            public MAY() : base(ID.Months.MAY)
            {
            }
        }

        public class JUNE : PX.Data.BQL.BqlString.Constant<JUNE>
		{
            public JUNE() : base(ID.Months.JUNE)
            {
            }
        }

        public class JULY : PX.Data.BQL.BqlString.Constant<JULY>
		{
            public JULY() : base(ID.Months.JULY)
            {
            }
        }

        public class AUGUST : PX.Data.BQL.BqlString.Constant<AUGUST>
		{
            public AUGUST() : base(ID.Months.AUGUST)
            {
            }
        }

        public class SEPTEMBER : PX.Data.BQL.BqlString.Constant<SEPTEMBER>
		{
            public SEPTEMBER() : base(ID.Months.SEPTEMBER)
            {
            }
        }

        public class OCTOBER : PX.Data.BQL.BqlString.Constant<OCTOBER>
		{
            public OCTOBER() : base(ID.Months.OCTOBER)
            {
            }
        }

        public class NOVEMBER : PX.Data.BQL.BqlString.Constant<NOVEMBER>
		{
            public NOVEMBER() : base(ID.Months.NOVEMBER)
            {
            }
        }

        public class DECEMBER : PX.Data.BQL.BqlString.Constant<DECEMBER>
		{
            public DECEMBER() : base(ID.Months.DECEMBER)
            {
            }
        }
    }
    #endregion

    #region Status_Service
    public abstract class ListField_Status_Service : PX.Data.IBqlField
    {
        public class ListAttribute : PXStringListAttribute
        {
            public ListAttribute()
                : base(
                    new string[] { PX.Objects.IN.InventoryItemStatus.Active, PX.Objects.IN.InventoryItemStatus.Inactive },
                    new string[] { PX.Objects.IN.Messages.Active, PX.Objects.IN.Messages.Inactive })
            {
            }
        }
    }
    #endregion

    #region Billing_By
    public abstract class ListField_Billing_By : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Billing_By().ID_LIST,
                    new ID.Billing_By().TX_LIST)
            {
            }
        }

        public class Appointment : PX.Data.BQL.BqlString.Constant<Appointment>
		{
            public Appointment()
                : base(ID.Billing_By.APPOINTMENT)
            {
            }
        }

        public class ServiceOrder : PX.Data.BQL.BqlString.Constant<ServiceOrder>
		{
            public ServiceOrder()
                : base(ID.Billing_By.SERVICE_ORDER)
            {
            }
        }
    }
    #endregion

    #region Billing_Cycle_Type
    public abstract class ListField_Billing_Cycle_Type : PX.Data.IBqlField
    {
         public class ListAtrribute : PXStringListAttribute
        {
             public ListAtrribute()
                : base(
                    new ID.Billing_Cycle_Type().ID_LIST,
                    new ID.Billing_Cycle_Type().TX_LIST)
             {
             }
        }

        public class Appointment : PX.Data.BQL.BqlString.Constant<Appointment>
		{
            public Appointment()
                : base(ID.Billing_Cycle_Type.APPOINTMENT)
            {
            }
        }

        public class ServiceOrder : PX.Data.BQL.BqlString.Constant<ServiceOrder>
		{
            public ServiceOrder()
                : base(ID.Billing_Cycle_Type.SERVICE_ORDER)
            {
            }
        }

        public class TimeFrame : PX.Data.BQL.BqlString.Constant<TimeFrame>
		{
            public TimeFrame() : base(ID.Billing_Cycle_Type.TIME_FRAME)
            {
            }
        }

        public class PurchaseOrder : PX.Data.BQL.BqlString.Constant<PurchaseOrder>
		{
            public PurchaseOrder() : base(ID.Billing_Cycle_Type.PURCHASE_ORDER)
            {
            }
        }

        public class WorkOrder : PX.Data.BQL.BqlString.Constant<WorkOrder>
		{
            public WorkOrder() : base(ID.Billing_Cycle_Type.WORK_ORDER)
            {
            }
        }
    }
    #endregion

    #region Time_Cycle_Type
    public abstract class ListField_Time_Cycle_Type : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Time_Cycle_Type().ID_LIST,
                    new ID.Time_Cycle_Type().TX_LIST)
            {
            }
        }

        public class Weekday : PX.Data.BQL.BqlString.Constant<Weekday>
		{
            public Weekday() : base(ID.Time_Cycle_Type.WEEKDAY)
            {
            }
        }

        public class DayOfMonth : PX.Data.BQL.BqlString.Constant<DayOfMonth>
		{
            public DayOfMonth() : base(ID.Time_Cycle_Type.DAY_OF_MONTH)
            {
            }
        }
    }
    #endregion

    #region Frequency_Type
    public abstract class ListField_Frequency_Type : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Frequency_Type().ID_LIST,
                    new ID.Frequency_Type().TX_LIST)
            {
            }
        }

        public class Weekly : PX.Data.BQL.BqlString.Constant<Weekly>
		{
            public Weekly()
                : base(ID.Frequency_Type.WEEKLY)
            {
            }
        }

        public class Monthly : PX.Data.BQL.BqlString.Constant<Monthly>
		{
            public Monthly()
                : base(ID.Frequency_Type.MONTHLY)
            {
            }
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
		{
            public None()
                : base(ID.Frequency_Type.NONE)
            {
            }
        }
    }
    #endregion

    #region Send_Invoices_To
    public abstract class ListField_Send_Invoices_To : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Send_Invoices_To().ID_LIST,
                    new ID.Send_Invoices_To().TX_LIST)
            {
            }
        }

        public class BillingCustomerBillTo : PX.Data.BQL.BqlString.Constant<BillingCustomerBillTo>
		{
            public BillingCustomerBillTo()
                : base(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO)
            {
            }
        }

        public class DefaultBillingCustomerLocation : PX.Data.BQL.BqlString.Constant<DefaultBillingCustomerLocation>
		{
            public DefaultBillingCustomerLocation()
                : base(ID.Send_Invoices_To.DEFAULT_BILLING_CUSTOMER_LOCATION)
            {
            }
        }

        public class SOBillingCustomerLocation : PX.Data.BQL.BqlString.Constant<SOBillingCustomerLocation>
		{
            public SOBillingCustomerLocation()
                : base(ID.Send_Invoices_To.SO_BILLING_CUSTOMER_LOCATION)
            {
            }
        }

        public class ServiceOrderAddress : PX.Data.BQL.BqlString.Constant<ServiceOrderAddress>
		{
            public ServiceOrderAddress()
                : base(ID.Send_Invoices_To.SERVICE_ORDER_ADDRESS)
            {
            }
        }
    }
    #endregion

    #region Ship-To
    public abstract class ListField_Ship_To : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Ship_To().ID_LIST,
                    new ID.Ship_To().TX_LIST)
            {
            }
        }

        public class BillingCustomerBillTo : PX.Data.BQL.BqlString.Constant<BillingCustomerBillTo>
		{
            public BillingCustomerBillTo()
                : base(ID.Ship_To.BILLING_CUSTOMER_BILL_TO)
            {
            }
        }

        public class SOBillingCustomerLocation : PX.Data.BQL.BqlString.Constant<SOBillingCustomerLocation>
		{
            public SOBillingCustomerLocation()
                : base(ID.Ship_To.SO_BILLING_CUSTOMER_LOCATION)
            {
            }
        }

        public class SOCustomerLocation : PX.Data.BQL.BqlString.Constant<SOCustomerLocation>
		{
            public SOCustomerLocation()
                : base(ID.Ship_To.SO_CUSTOMER_LOCATION)
            {
            }
        }

        public class ServiceOrderAddress : PX.Data.BQL.BqlString.Constant<ServiceOrderAddress>
		{
            public ServiceOrderAddress()
                : base(ID.Ship_To.SERVICE_ORDER_ADDRESS)
            {
            }
        }
    }
    #endregion

    #region Default_Billing_Customer_Source
    public abstract class ListField_Default_Billing_Customer_Source : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Default_Billing_Customer_Source().ID_LIST,
                    new ID.Default_Billing_Customer_Source().TX_LIST)
            {
            }
        }

        public class Service_Order_Customer : PX.Data.BQL.BqlString.Constant<Service_Order_Customer>
		{
            public Service_Order_Customer()
                : base(ID.Default_Billing_Customer_Source.SERVICE_ORDER_CUSTOMER)
            {
            }
        }

        public class Default_Customer : PX.Data.BQL.BqlString.Constant<Default_Customer>
		{
            public Default_Customer()
                : base(ID.Default_Billing_Customer_Source.DEFAULT_CUSTOMER)
            {
            }
        }

        public class Specific_Customer : PX.Data.BQL.BqlString.Constant<Specific_Customer>
		{
            public Specific_Customer()
                : base(ID.Default_Billing_Customer_Source.SPECIFIC_CUSTOMER)
            {
            }
        }
    }
    #endregion

    #region PostBatch
    public abstract class ListField_PostTo : ListField_PostTo_CreateInvoice
    {
        public new class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Batch_PostTo().ID_LIST,
                    new ID.Batch_PostTo().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public new class AR : PX.Data.BQL.BqlString.Constant<AR>
		{
            public AR() : base(ID.Batch_PostTo.AR)
            {
            }
        }

        public new class SO : PX.Data.BQL.BqlString.Constant<SO>
		{
            public SO() : base(ID.Batch_PostTo.SO)
            {
            }
        }

        public new class SI : PX.Data.BQL.BqlString.Constant<SI>
        {
            public SI() : base(ID.Batch_PostTo.SI)
            {
            }
        }

        public class AP : PX.Data.BQL.BqlString.Constant<AP>
		{
            public AP() : base(ID.Batch_PostTo.AP)
            {
            }
        }

        public class IN : PX.Data.BQL.BqlString.Constant<IN>
		{
           public IN() : base(ID.Batch_PostTo.IN)
           {
           }
        }

        public class AR_AP : PX.Data.BQL.BqlString.Constant<AR_AP>
		{
            public AR_AP()
                : base(ID.Batch_PostTo.AR_AP)
            {
            }
        }
    }

    public abstract class ListField_PostTo_CreateInvoice : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Batch_PostTo_Filter().ID_LIST,
                    new ID.Batch_PostTo_Filter().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class AR : PX.Data.BQL.BqlString.Constant<AR>
		{
            public AR() : base(ID.Batch_PostTo.AR)
            {
            }
        }

        public class SO : PX.Data.BQL.BqlString.Constant<SO>
		{
            public SO() : base(ID.Batch_PostTo.SO)
            {
            }
        }

        public class SI : PX.Data.BQL.BqlString.Constant<SI>
        {
            public SI() : base(ID.Batch_PostTo.SI)
            {
            }
        }
        public class PM : PX.Data.BQL.BqlString.Constant<PM>
        {
            public PM() : base(ID.Batch_PostTo.PM)
            {
            }
        }
    }
    #endregion

    #region EquipmentItemClass
    public abstract class ListField_EquipmentItemClass : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Equipment_Item_Class().ID_LIST,
                    new ID.Equipment_Item_Class().TX_LIST)
            {
            }
        }

        public class PartOtherInventory : PX.Data.BQL.BqlString.Constant<PartOtherInventory>
		{
            public PartOtherInventory() : base(ID.Equipment_Item_Class.PART_OTHER_INVENTORY)
            {
            }
        }

        public class ModelEquipment : PX.Data.BQL.BqlString.Constant<ModelEquipment>
		{
            public ModelEquipment() : base(ID.Equipment_Item_Class.MODEL_EQUIPMENT)
            {
            }
        }

        public class Component : PX.Data.BQL.BqlString.Constant<Component>
		{
            public Component() : base(ID.Equipment_Item_Class.COMPONENT)
            {
            }
        }

        public class Consumable : PX.Data.BQL.BqlString.Constant<Consumable>
		{
            public Consumable() : base(ID.Equipment_Item_Class.CONSUMABLE)
            {
            }
        }
    }
    #endregion

    #region EquipmentStatus
    public abstract class ListField_Equipment_Status : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Equipment_Status().ID_LIST,
                    new ID.Equipment_Status().TX_LIST)
            {
            }
        }

        public class Active : PX.Data.BQL.BqlString.Constant<Active>
		{
            public Active() : base(ID.Equipment_Status.ACTIVE)
            {
            }
        }

        public class Suspended : PX.Data.BQL.BqlString.Constant<Suspended>
		{
            public Suspended() : base(ID.Equipment_Status.SUSPENDED)
            {
            }
        }

        public class Dispose : PX.Data.BQL.BqlString.Constant<Dispose>
		{
            public Dispose() : base(ID.Equipment_Status.DISPOSED)
            {
            }
        }
    }
    #endregion

    #region EquipmentAction
    public abstract class ListField_EquipmentActionBase : PX.Data.IBqlField
    {
        //BQL constant declaration
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Equipment_Action_Base().ID_LIST,
                    new ID.Equipment_Action_Base().TX_LIST)
            {
            }
        }

        public class None : PX.Data.BQL.BqlString.Constant<None>
		{
            public None()
                : base(ID.Equipment_Action.NONE)
            {
            }
        }

        public class SellingTargetEquipment : PX.Data.BQL.BqlString.Constant<SellingTargetEquipment>
		{
            public SellingTargetEquipment()
                : base(ID.Equipment_Action.SELLING_TARGET_EQUIPMENT)
            {
            }
        }

        public class ReplacingComponent : PX.Data.BQL.BqlString.Constant<ReplacingComponent>
		{
            public ReplacingComponent()
                : base(ID.Equipment_Action.REPLACING_COMPONENT)
            {
            }
        }
    }
    public abstract class ListField_EquipmentAction : ListField_EquipmentActionBase
    {
        public new class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Equipment_Action().ID_LIST,
                    new ID.Equipment_Action().TX_LIST)
            {
            }
        }

        public class ReplacingTargetEquipment : PX.Data.BQL.BqlString.Constant<ReplacingTargetEquipment>
		{
            public ReplacingTargetEquipment()
                : base(ID.Equipment_Action.REPLACING_TARGET_EQUIPMENT)
            {
            }
        }

        public class CreatingComponent : PX.Data.BQL.BqlString.Constant<CreatingComponent>
		{
            public CreatingComponent()
                : base(ID.Equipment_Action.CREATING_COMPONENT)
            {
            }
        }

        public class UpgradingComponent : PX.Data.BQL.BqlString.Constant<UpgradingComponent>
		{
            public UpgradingComponent()
                : base(ID.Equipment_Action.UPGRADING_COMPONENT)
            {
            }
        }
    }

    public abstract class ListField_Schedule_EquipmentAction : ListField_EquipmentActionBase
    {
        public new class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Schedule_Equipment_Action().ID_LIST,
                    new ID.Schedule_Equipment_Action().TX_LIST)
            {
            }
        }
    }
    #endregion

    #region ServiceOrder_Action_Filter
    public abstract class ListField_ServiceOrder_Action_Filter : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.ServiceOrder_Action_Filter().ID_LIST,
                    new ID.ServiceOrder_Action_Filter().TX_LIST)
            {
            }
        }
        public class Undefined : PX.Data.BQL.BqlString.Constant<Undefined>
		{
            public Undefined()
                : base(ID.ServiceOrder_Action_Filter.UNDEFINED)
            {
            }
        }

        public class Complete : PX.Data.BQL.BqlString.Constant<Complete>
		{
            public Complete()
                : base(ID.ServiceOrder_Action_Filter.COMPLETE)
            {
            }
        }

        public class Cancel : PX.Data.BQL.BqlString.Constant<Cancel>
		{
            public Cancel()
                : base(ID.ServiceOrder_Action_Filter.CANCEL)
            {
            }
        }

        public class Reopen : PX.Data.BQL.BqlString.Constant<Reopen>
		{
            public Reopen()
                : base(ID.ServiceOrder_Action_Filter.REOPEN)
            {
            }
        }

        public class Close : PX.Data.BQL.BqlString.Constant<Close>
		{
            public Close()
                : base(ID.ServiceOrder_Action_Filter.CLOSE)
            {
            }
        }

        public class Unclose : PX.Data.BQL.BqlString.Constant<Unclose>
		{
            public Unclose()
                : base(ID.ServiceOrder_Action_Filter.UNCLOSE)
            {
            }
        }

        public class AllowInvoice : PX.Data.BQL.BqlString.Constant<AllowInvoice>
		{
            public AllowInvoice()
                : base(ID.ServiceOrder_Action_Filter.ALLOWINVOICE)
            {
            }
        }
    }
    #endregion
    public abstract class ListField
    {
        #region Address/Contact Entity Type
        /// <summary>
        /// EntityType for FSAddress and FSContact tables
        /// </summary>
        public abstract class ACEntityType : PX.Data.IBqlField
        {
            public class ListAttribute : PXStringListAttribute
            {
                public ListAttribute()
                    : base(
                        new[] { ID.ACEntityType.MANUFACTURER, ID.ACEntityType.BRANCH_LOCATION, ID.ACEntityType.SERVICE_ORDER, ID.ACEntityType.APPOINTMENT },
                        new[] { TX.ACEntityType.MANUFACTURER, TX.ACEntityType.BRANCH_LOCATION, TX.ACEntityType.SERVICE_ORDER, TX.ACEntityType.APPOINTMENT })
            {
            }
            }

            //BQL constants
            public class Manufacturer : ID.ACEntityType.Manufacturer { }

            public class BranchLocation : ID.ACEntityType.BranchLocation { }

            public class ServiceOrder : ID.ACEntityType.ServiceOrder { }

            public class Appointment : ID.ACEntityType.Appointment { }
        }
        #endregion
    }

    #region TimeRange_Setup
    public abstract class ListField_TimeRange_Setup : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.TimeRange_Setup().ID_LIST,
                    new ID.TimeRange_Setup().TX_LIST)
            {
            }
        }
    }
    #endregion
    #region TimeFilter_Setup
    public abstract class ListField_TimeFilter_Setup : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.TimeFilter_Setup().ID_LIST,
                    new ID.TimeFilter_Setup().TX_LIST)
            {
            }
        }
    }
    #endregion
    #region DayResolution_Setup
    public abstract class ListField_DayResolution_Setup : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.DayResolution_Setup().ID_LIST,
                    new ID.DayResolution_Setup().TX_LIST)
            {
            }
        }
    }
    #endregion
    #region WeekResolution_Setup
    public abstract class ListField_WeekResolution_Setup : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.WeekResolution_Setup().ID_LIST,
                    new ID.WeekResolution_Setup().TX_LIST)
            {
            }
        }
    }
    #endregion
    #region MonthResolution_Setup
    public abstract class ListField_MonthResolution_Setup : PX.Data.IBqlField
    {
        public class ListAtrribute : PXIntListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.MonthResolution_Setup().ID_LIST,
                    new ID.MonthResolution_Setup().TX_LIST)
            {
            }
        }
    }
    #endregion

    #region Status_ROOptimization
    public abstract class ListField_Status_ROOptimization : PX.Data.IBqlField
    {
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Status_ROOptimization().ID_LIST,
                    new ID.Status_ROOptimization().TX_LIST)
            {
            }
        }

        //BQL constant declaration
        public class NotOptimized : PX.Data.BQL.BqlString.Constant<NotOptimized>
        {
            public NotOptimized() : base(ID.Status_ROOptimization.NOT_OPTIMIZED)
            {
            }
        }

        public class Optimized : PX.Data.BQL.BqlString.Constant<Optimized>
        {
            public Optimized() : base(ID.Status_ROOptimization.OPTIMIZED)
            {
            }
        }

        public class NotAble : PX.Data.BQL.BqlString.Constant<NotAble>
        {
            public NotAble() : base(ID.Status_ROOptimization.NOT_ABLE)
            {
            }
        }

        public class AddressError : PX.Data.BQL.BqlString.Constant<AddressError>
        {
            public AddressError() : base(ID.Status_ROOptimization.ADDRESS_ERROR)
            {
            }
        }
    }
    #endregion

    #region EquipmentAction
    public abstract class ListField_ROType : PX.Data.IBqlField
    {
        //BQL constant declaration
        public class ListAtrribute : PXStringListAttribute
        {
            public ListAtrribute()
                : base(
                    new ID.Type_ROOptimization().ID_LIST,
                    new ID.Type_ROOptimization().TX_LIST)
            {
            }
        }

        public class AssignedApp : Data.BQL.BqlString.Constant<AssignedApp>
        {
            public AssignedApp()
                : base(ID.Type_ROOptimization.ASSIGNED_APP)
            {
            }
        }

        public class UnassignedApp : Data.BQL.BqlString.Constant<UnassignedApp>
        {
            public UnassignedApp()
                : base(ID.Type_ROOptimization.UNASSIGNED_APP)
            {
            }
        }
    }
    #endregion

}
