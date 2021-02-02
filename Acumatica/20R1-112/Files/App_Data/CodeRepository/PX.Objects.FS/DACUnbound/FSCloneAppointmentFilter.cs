using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    [System.SerializableAttribute]
    public class FSCloneAppointmentFilter : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXString(4, IsFixed = true)]
        [PXDefault(typeof(Coalesce<
            Search<FSxUserPreferences.dfltSrvOrdType,
            Where<
                PX.SM.UserPreferences.userID, Equal<CurrentValue<AccessInfo.userID>>>>,
            Search<FSSetup.dfltSrvOrdType>>))]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType,
            Where<FSSrvOrdType.active, Equal<True>>>),
            DescriptionField = typeof(FSSrvOrdType.descr))]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXString(20, IsUnicode = true, InputMask = "CCCCCCCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = true, Enabled = true)]
        [PXSelector(typeof(Search2<FSAppointment.refNbr,
                        LeftJoin<FSServiceOrder,
                            On<FSServiceOrder.sOID, Equal<FSAppointment.sOID>>,
                        LeftJoin<BAccount,
                            On<BAccount.bAccountID, Equal<FSServiceOrder.customerID>>>>,
                        Where<FSAppointment.srvOrdType, Equal<Current<FSCloneAppointmentFilter.srvOrdType>>>>),
                    new Type[] {
                                typeof(FSAppointment.refNbr),
                                typeof(FSServiceOrder.refNbr),
                                typeof(BAccount.acctName),
                                typeof(FSServiceOrder.docDesc),
                                typeof(FSAppointment.status),
                                typeof(FSAppointment.scheduledDateTimeBegin),
                                typeof(FSAppointment.actualDateTimeBegin)
                    })]
        public virtual string RefNbr { get; set; }
        #endregion
        #region SORefNbr
        public abstract class soRefNbr : PX.Data.BQL.BqlString.Field<soRefNbr> { }

        [PXDefault]
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Service Order Nbr.")]
        [PXSelector(
                typeof(Search<FSServiceOrder.refNbr,
                Where<
                    FSServiceOrder.srvOrdType, Equal<Current<FSCloneAppointmentFilter.srvOrdType>>>>))]
        public virtual string SORefNbr { get; set; }
        #endregion
        #region CloneScheduledDateTimeBegin
        public abstract class scheduledDate : PX.Data.BQL.BqlDateTime.Field<scheduledDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? ScheduledDate { get; set; }
        #endregion
        #region CloneScheduledTimeBegin
        public abstract class scheduledStartTime : PX.Data.BQL.BqlDateTime.Field<scheduledStartTime> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Start Time")]
        public virtual DateTime? ScheduledStartTime { get; set; }
        #endregion
        #region CloneScheduledTimeEnd
        public abstract class scheduledEndTime : PX.Data.BQL.BqlDateTime.Field<scheduledEndTime> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "End Time")]
        public virtual DateTime? ScheduledEndTime { get; set; }
        #endregion

        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXInt]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region KeepTimeDuration
        public abstract class keepTimeDuration : PX.Data.BQL.BqlBool.Field<keepTimeDuration> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Keep Original Appointment Duration")]
        public virtual bool? KeepTimeDuration { get; set; }
        #endregion
        #region CloningType
        public abstract class cloningType : ListField_CloningType_CloneAppointment
        {
        }

        [PXString(2, IsFixed = true)]
        [cloningType.ListAttribute]
        [PXDefault(ID.CloningType_CloneAppointment.SINGLE)]
        [PXUIField(DisplayName = "Cloning Type")]
        public virtual string CloningType { get; set; }
        #endregion
        #region CloneScheduledFromDateTime
        public abstract class scheduledFromDate : PX.Data.BQL.BqlDateTime.Field<scheduledFromDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "From Date")]
        public virtual DateTime? ScheduledFromDate { get; set; }
        #endregion
        #region CloneScheduledToDateTime
        public abstract class scheduledToDate : PX.Data.BQL.BqlDateTime.Field<scheduledToDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "To Date")]
        public virtual DateTime? ScheduledToDate { get; set; }
        #endregion
        #region ActiveOnMonday
        public abstract class activeOnMonday : PX.Data.BQL.BqlBool.Field<activeOnMonday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Monday")]
        public virtual bool? ActiveOnMonday { get; set; }
        #endregion
        #region ActiveOnTuesday
        public abstract class activeOnTuesday : PX.Data.BQL.BqlBool.Field<activeOnTuesday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Tuesday")]
        public virtual bool? ActiveOnTuesday { get; set; }
        #endregion
        #region ActiveOnWednesday
        public abstract class activeOnWednesday : PX.Data.BQL.BqlBool.Field<activeOnWednesday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Wednesday")]
        public virtual bool? ActiveOnWednesday { get; set; }
        #endregion
        #region ActiveOnThursday
        public abstract class activeOnThursday : PX.Data.BQL.BqlBool.Field<activeOnThursday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Thursday ")]
        public virtual bool? ActiveOnThursday { get; set; }
        #endregion
        #region ActiveOnFriday
        public abstract class activeOnFriday : PX.Data.BQL.BqlBool.Field<activeOnFriday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Friday")]
        public virtual bool? ActiveOnFriday { get; set; }
        #endregion
        #region ActiveOnSaturday
        public abstract class activeOnSaturday : PX.Data.BQL.BqlBool.Field<activeOnSaturday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Saturday")]
        public virtual bool? ActiveOnSaturday { get; set; }
        #endregion
        #region ActiveOnSunday
        public abstract class activeOnSunday : PX.Data.BQL.BqlBool.Field<activeOnSunday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Sunday")]
        public virtual bool? ActiveOnSunday { get; set; }
        #endregion
        #region OriginalDuration
        public abstract class originalDuration : PX.Data.BQL.BqlInt.Field<originalDuration> { }

        [PXInt]
        [PXFormula(typeof(DateDiff<FSCloneAppointmentFilter.scheduledStartTime, FSCloneAppointmentFilter.scheduledEndTime, DateDiff.minute>))]
        public virtual int? OriginalDuration { get; set; }
        #endregion
    }
}
