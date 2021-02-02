using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    [Serializable]
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

        #region ScheduledStartTime
        public abstract class scheduledStartTime : PX.Data.BQL.BqlDateTime.Field<scheduledStartTime> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Scheduled Start Time", Required = true)]
        [PXDefault(typeof(Current<FSAppointment.scheduledDateTimeBegin>))]
        [PXFormula(typeof(Default<srvOrdType, refNbr>))]
        public virtual DateTime? ScheduledStartTime { get; set; }
        #endregion
        #region OverrideApptDuration
        public abstract class overrideApptDuration : PX.Data.BQL.BqlBool.Field<overrideApptDuration> { }

        [PXBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Override")]
        public virtual bool? OverrideApptDuration { get; set; }
        #endregion
        #region ApptDuration
        public abstract class apptDuration : PX.Data.BQL.BqlInt.Field<apptDuration> { }

        [PXTimeSpanLong(Format = TimeSpanFormatType.LongHoursMinutes)]
        [PXUIField(DisplayName = "Scheduled Duration")]
        [PXUIEnabled(typeof(Where<overrideApptDuration, Equal<True>>))]
        [PXDefault(typeof(Current<FSAppointment.scheduledDuration>))]
        [PXFormula(typeof(Default<srvOrdType, refNbr, overrideApptDuration>))]
        public virtual int? ApptDuration { get; set; }
        #endregion

        #region CloningType
        public abstract class cloningType : PX.Data.BQL.BqlString.Field<cloningType>
        {
            public abstract class Values : ListField.CloningType_CloneAppointment { }
        }

        [PXString(2, IsFixed = true)]
        [cloningType.Values.List]
        [PXDefault(cloningType.Values.Single)]
        [PXUIField(DisplayName = "Cloning Type")]
        public virtual string CloningType { get; set; }
        #endregion
        #region SingleGenerationDate
        public abstract class singleGenerationDate : PX.Data.BQL.BqlDateTime.Field<singleGenerationDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "Date", Required = true)]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.single>>))]
        public virtual DateTime? SingleGenerationDate { get; set; }
        #endregion
        #region MultGenerationFromDate
        public abstract class multGenerationFromDate : PX.Data.BQL.BqlDateTime.Field<multGenerationFromDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "From Date", Required = true)]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual DateTime? MultGenerationFromDate { get; set; }
        #endregion
        #region MultGenerationToDate
        public abstract class multGenerationToDate : PX.Data.BQL.BqlDateTime.Field<multGenerationToDate> { }

        [PXDateAndTime(UseTimeZone = true)]
        [PXUIField(DisplayName = "To Date", Required = true)]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual DateTime? MultGenerationToDate { get; set; }
        #endregion
        #region ActiveOnMonday
        public abstract class activeOnMonday : PX.Data.BQL.BqlBool.Field<activeOnMonday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Monday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnMonday { get; set; }
        #endregion
        #region ActiveOnTuesday
        public abstract class activeOnTuesday : PX.Data.BQL.BqlBool.Field<activeOnTuesday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Tuesday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnTuesday { get; set; }
        #endregion
        #region ActiveOnWednesday
        public abstract class activeOnWednesday : PX.Data.BQL.BqlBool.Field<activeOnWednesday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Wednesday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnWednesday { get; set; }
        #endregion
        #region ActiveOnThursday
        public abstract class activeOnThursday : PX.Data.BQL.BqlBool.Field<activeOnThursday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Thursday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnThursday { get; set; }
        #endregion
        #region ActiveOnFriday
        public abstract class activeOnFriday : PX.Data.BQL.BqlBool.Field<activeOnFriday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Friday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnFriday { get; set; }
        #endregion
        #region ActiveOnSaturday
        public abstract class activeOnSaturday : PX.Data.BQL.BqlBool.Field<activeOnSaturday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Saturday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnSaturday { get; set; }
        #endregion
        #region ActiveOnSunday
        public abstract class activeOnSunday : PX.Data.BQL.BqlBool.Field<activeOnSunday> { }

        [PXBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Sunday")]
        [PXUIVisible(typeof(Where<cloningType, Equal<cloningType.Values.multiple>>))]
        public virtual bool? ActiveOnSunday { get; set; }
        #endregion
    }
}
