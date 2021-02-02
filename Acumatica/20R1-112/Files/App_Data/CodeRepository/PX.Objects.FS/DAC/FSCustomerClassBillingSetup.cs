using PX.Data;
using PX.Objects.AR;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSCustomerClassBillingSetup : PX.Data.IBqlTable
    {
        #region CustomerClassID
        public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }

        [PXDBString(10, IsKey = true)]
        [PXParent(typeof(
            Select<CustomerClass, 
            Where<
                CustomerClass.customerClassID, Equal<Current<FSCustomerClassBillingSetup.customerClassID>>>>))]
        [PXDBLiteDefault(typeof(CustomerClass.customerClassID))]
        public virtual string CustomerClassID { get; set; }
        #endregion
        #region CBID
        public abstract class cBID : PX.Data.BQL.BqlInt.Field<cBID> { }

        [PXDBIdentity(IsKey = true)]
        public virtual int? CBID { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
            Search<FSSetup.dfltSrvOrdType>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorSrvOrdTypeNOTQuote]
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