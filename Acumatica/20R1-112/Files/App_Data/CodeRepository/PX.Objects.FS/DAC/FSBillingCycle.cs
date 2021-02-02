using System;
using PX.Data;
﻿
namespace PX.Objects.FS
{

    [Serializable]
    [PXCacheName(TX.TableName.BILLING_CYCLE)]
    [PXPrimaryGraph(typeof(BillingCycleMaint))]
    public class FSBillingCycle : PX.Data.IBqlTable
    {
        #region BillingCycleID
        public abstract class billingCycleID : PX.Data.BQL.BqlInt.Field<billingCycleID> { }
        [PXDBIdentity]
        public virtual int? BillingCycleID { get; set; }
        #endregion
        #region BillingCycleCD
        public abstract class billingCycleCD : PX.Data.BQL.BqlString.Field<billingCycleCD> { }
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAA")]
        [PXDefault]
        [PXUIField(DisplayName = "Billing Cycle ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(FSBillingCycle.billingCycleCD))]
        [NormalizeWhiteSpace]
        public virtual string BillingCycleCD { get; set; }
        #endregion
        #region BillingCycleType
        public abstract class billingCycleType : ListField_Billing_Cycle_Type
        {
        }
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Group Billing Documents By")]
        [billingCycleType.ListAtrribute]
        [PXDefault(ID.Billing_Cycle_Type.TIME_FRAME)]
        public virtual string BillingCycleType { get; set; }
        #endregion
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }
        [PXDBLocalizableString(60, IsUnicode = true)]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string Descr { get; set; }
        #endregion
        #region InvoiceOnlyCompletedServiceOrder
        public abstract class invoiceOnlyCompletedServiceOrder : PX.Data.BQL.BqlBool.Field<invoiceOnlyCompletedServiceOrder> { }
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Bill Only Completed or Closed Service Orders")]
        public virtual bool? InvoiceOnlyCompletedServiceOrder { get; set; }
        #endregion
        #region TimeCycleType
        public abstract class timeCycleType : ListField_Time_Cycle_Type
        {
        }
        [PXDBString(2, IsFixed = true)]
        [PXDefault(ID.Time_Cycle_Type.DAY_OF_MONTH)]
        [timeCycleType.ListAtrribute]
        [PXUIField(DisplayName = "Time Cycle Type")]
        public virtual string TimeCycleType { get; set; }
        #endregion
        #region TimeCycleWeekDay
        public abstract class timeCycleWeekDay : ListField_WeekDaysNumber
        {
        }
        [PXDBInt]
        [PXUIField(DisplayName = "Day of Week", Visible = false)]
        [timeCycleWeekDay.ListAtrribute]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? TimeCycleWeekDay { get; set; }
        #endregion
        #region TimeCycleDayOfMonth
        public abstract class timeCycleDayOfMonth : PX.Data.BQL.BqlInt.Field<timeCycleDayOfMonth> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Day of Month")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        public virtual int? TimeCycleDayOfMonth { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
        [PXDBCreatedByID]
        [PXUIField(DisplayName = "Created By")]
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
        [PXUIField(DisplayName = "Created On")]
        public virtual DateTime? CreatedDateTime { get; set; }

        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
        [PXDBLastModifiedByID]
        [PXUIField(DisplayName = "Last Modified By")]
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
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }

        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp]
        [PXUIField(DisplayName = "tstamp")]
        public virtual byte[] tstamp { get; set; }

        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
        #endregion
        #region BillingBy
        public abstract class billingBy : ListField_Billing_By
        {
        }
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Run Billing For")]
        [billingBy.ListAtrribute]
        [PXDefault(ID.Billing_By.APPOINTMENT)]
        public virtual string BillingBy { get; set; }
        #endregion
        #region GroupBillByLocations
        public abstract class groupBillByLocations : PX.Data.BQL.BqlBool.Field<groupBillByLocations> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Separate Billing Documents by Customer Location")]
        public virtual bool? GroupBillByLocations { get; set; }
        #endregion

        #region UpdateCutOffDate
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UpdateCutOffDate { get; set; }
        #endregion
    }
}
