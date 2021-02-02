using PX.Data;
using PX.Objects.AR;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    //// Remember to use Active property when you are searching for Customer billing setup records. Unless you want historical values.
    public class FSCustomerBillingSetup : PX.Data.IBqlTable
    {
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

        [PXDBInt(IsKey = true)]
        [PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<FSCustomerBillingSetup.customerID>>>>))]
        [PXDBLiteDefault(typeof(Customer.bAccountID))]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region CBID
        public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? CBID { get; set; }
        #endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }

        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorSrvOrdTypeNOTQuoteInternal]
        [PXUIField(DisplayName = "Service Order Type")]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }

        [PXDBInt]
        [PXDefault]
        [PXSelector(
                    typeof(Search<FSBillingCycle.billingCycleID>),
                    SubstituteKey = typeof(FSBillingCycle.billingCycleCD),
                    DescriptionField = typeof(FSBillingCycle.descr))]
        [PXUIField(DisplayName = "Billing Cycle")]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region FrequencyType
        public abstract class frequencyType : ListField_Frequency_Type
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Frequency_Type.NONE)]
        [frequencyType.ListAtrribute]
        [PXUIField(DisplayName = "Frequency Type")]
        public virtual string FrequencyType { get; set; }
        #endregion
        #region WeeklyFrequency
        public abstract class weeklyFrequency : ListField_WeekDaysNumber
        {
        }

        [PXDBInt]
        [PXUIField(DisplayName = "Frequency Week Day", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [weeklyFrequency.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? WeeklyFrequency { get; set; }
        #endregion
        #region MonthlyFrequency
        public abstract class monthlyFrequency : PX.Data.BQL.BqlInt.Field<monthlyFrequency> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Frequency Month Day", Visible = false, Visibility = PXUIVisibility.Invisible)]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? MonthlyFrequency { get; set; }
        #endregion
        #region SendInvoicesTo
        public abstract class sendInvoicesTo : ListField_Send_Invoices_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Send_Invoices_To.BILLING_CUSTOMER_BILL_TO)]
        [sendInvoicesTo.ListAtrribute]
        [PXUIField(DisplayName = "Bill-To Address")]
        public virtual string SendInvoicesTo { get; set; }
        #endregion
        #region BillShipmentSource
        public abstract class billShipmentSource : ListField_Ship_To
        {
        }

        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Ship_To.SERVICE_ORDER_ADDRESS)]
        [billShipmentSource.ListAtrribute]
        [PXUIField(DisplayName = "Ship-To Address")]
        public virtual string BillShipmentSource { get; set; }
        #endregion
        #region IsBeingDeleted
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? IsBeingDeleted { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        [PXUIField(DisplayName = "CreatedByID")]
        public virtual Guid? CreatedByID { get; set; }

        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        [PXUIField(DisplayName = "CreatedByScreenID")]
        public virtual string CreatedByScreenID { get; set; }

        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        [PXUIField(DisplayName = "CreatedDateTime")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "LastModifiedByID")]
        public virtual Guid? LastModifiedByID { get; set; }

        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        [PXUIField(DisplayName = "LastModifiedByScreenID")]
        public virtual string LastModifiedByScreenID { get; set; }

        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        [PXUIField(DisplayName = "LastModifiedDateTime")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

        #endregion 
    }
}
