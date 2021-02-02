using PX.Data;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.IN;
using System;

namespace PX.Objects.FS
{
    [Serializable]
    public class FSAppointmentEmployee : PX.Data.IBqlTable
    {
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }

        [PXDBString(4, IsKey = true, IsFixed = true)]
        [PXUIField(DisplayName = "Service Order Type", Visible = false, Enabled = false)]
        [PXDefault(typeof(FSAppointment.srvOrdType))]
        [PXSelector(typeof(Search<FSSrvOrdType.srvOrdType>), CacheGlobal = true)]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Appointment Nbr.", Visible = false, Enabled = false)]
        [PXDBDefault(typeof(FSAppointment.refNbr), DefaultForUpdate = false)]
        [PXParent(typeof(Select<FSAppointment,
                            Where<FSAppointment.srvOrdType, Equal<Current<FSAppointmentEmployee.srvOrdType>>,
                                And<FSAppointment.refNbr, Equal<Current<FSAppointmentEmployee.refNbr>>>>>))]
        public virtual string RefNbr { get; set; }
        #endregion
        #region AppointmentID
        public abstract class appointmentID : PX.Data.BQL.BqlInt.Field<appointmentID> { }

        [PXDBInt]
        [PXDBLiteDefault(typeof(FSAppointment.appointmentID))]
        [PXUIField(DisplayName = "Appointment Ref. Nbr.")]
        public virtual int? AppointmentID { get; set; }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        [PXDBInt(IsKey = true)]
        [PXLineNbr(typeof(FSAppointment))]
        public virtual int? LineNbr { get; set; }
        #endregion
        #region LineRef
        public abstract class lineRef : PX.Data.BQL.BqlString.Field<lineRef> { }

        [PXDBString(3, IsFixed = true)]
        [PXUIField(DisplayName = "Line Ref.", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual string LineRef { get; set; }
        #endregion
        #region ServiceLineRef
        public abstract class serviceLineRef : PX.Data.BQL.BqlString.Field<serviceLineRef> { }

        [PXDBString(4, IsFixed = true)]
        [PXParent(typeof(Select<FSAppointmentDet,
                            Where<
                                FSAppointmentDet.lineRef, Equal<Current<FSAppointmentEmployee.serviceLineRef>>,
                                And<FSAppointmentDet.appointmentID, Equal<Current<FSAppointmentEmployee.appointmentID>>>>>))]
        [PXUIField(DisplayName = "Service Line Ref.")]
        [FSSelectorAppointmentSODetID]        
        public virtual string ServiceLineRef { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

        [PXDBInt]
        [PXDefault]
        [FSSelector_StaffMember_ServiceOrderProjectID]
        [PXUIField(DisplayName = "Staff Member", TabOrder = 0)]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region Comment
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }

        [PXDBString(255)]
        [PXUIField(DisplayName = "Comment", Enabled = false)]
        public virtual string Comment { get; set; }
        #endregion
        #region CreatedByID
        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        [PXDBCreatedByID]
        public virtual Guid? CreatedByID { get; set; }
        #endregion
        #region CreatedByScreenID
        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }

        [PXDBCreatedByScreenID]
        public virtual string CreatedByScreenID { get; set; }
        #endregion
        #region CreatedDateTime
        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

        [PXDBCreatedDateTime]
        public virtual DateTime? CreatedDateTime { get; set; }
        #endregion
        #region LastModifiedByID
        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }

        [PXDBLastModifiedByID]
        public virtual Guid? LastModifiedByID { get; set; }
        #endregion
        #region LastModifiedByScreenID
        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }

        [PXDBLastModifiedByScreenID]
        public virtual string LastModifiedByScreenID { get; set; }
        #endregion
        #region LastModifiedDateTime
        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }

        [PXDBLastModifiedDateTime]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region IsDriver
        public abstract class isDriver : PX.Data.BQL.BqlBool.Field<isDriver> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Driver", Enabled = false)]
        public virtual bool? IsDriver { get; set; }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlString.Field<type> { }

        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        [BAccountType.List]
        public virtual string Type { get; set; }
        #endregion

        #region KeepActualDateTimes
        public abstract class keepActualDateTimes : PX.Data.BQL.BqlBool.Field<keepActualDateTimes> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIEnabled(typeof(IIf<Where<Current<FSSrvOrdType.keepActualDateTimes>, Equal<True>,
                                And<Current<FSAppointment.status>, NotEqual<FSAppointment.status.ManualScheduled>,
                                    And<Current<FSAppointment.status>, NotEqual<FSAppointment.status.AutomaticScheduled>>>>, True, False>))]
        [PXUIField(DisplayName = "Manage Time Manually")]
        public virtual bool? KeepActualDateTimes { get; set; }
        #endregion
        #region ActualDuration
        public abstract class actualDuration : PX.Data.BQL.BqlInt.Field<actualDuration> { }

        [PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutes)]
        [PXUIField(DisplayName = "Actual Duration")]
        [PXFormula(typeof(IsNull<Sub<FSAppointmentEmployee.actualDateTimeEnd, FSAppointmentEmployee.actualDateTimeBegin>, SharedClasses.int_0>),
            typeof(SumCalc<FSAppointmentDetService.staffActualDuration>))]
        [PXFormula(typeof(Default<serviceLineRef>))]
        [PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIVerify(typeof(Where<actualDuration, Less<AppointmentDateTimeHelper.int_24hrs>>),
                    PXErrorLevel.Error, TX.Error.ACTUAL_DURATION_CANNOT_BE_GREATER_24HRS)]
        public virtual int? ActualDuration { get; set; }
        #endregion
        #region ActualDateTimeBegin
        public abstract class actualDateTimeBegin : PX.Data.BQL.BqlDateTime.Field<actualDateTimeBegin> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual Start Time")]
        [PXUIVerify(typeof(Where<actualDateTimeBegin, IsNull,
                               Or<actualDateTimeEnd, IsNull,
                                  Or<actualDateTimeBegin, Less<actualDateTimeEnd>>>>), PXErrorLevel.Error, TX.Error.START_TIME_GREATER_THAN_END_TIME)]
        [PXUIField(DisplayName = "Actual Start Time")]
        public virtual DateTime? ActualDateTimeBegin { get; set; }
        #endregion
        #region ActualDateTimeEnd
        public abstract class actualDateTimeEnd : PX.Data.BQL.BqlDateTime.Field<actualDateTimeEnd> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Date", DisplayNameTime = "Actual End Time")]
        [PXUIVerify(typeof(Where<actualDateTimeBegin, IsNull,
                               Or<actualDateTimeEnd, IsNull,
                                  Or<actualDateTimeEnd, Greater<actualDateTimeBegin>>>>), PXErrorLevel.Error, TX.Error.END_TIME_LESSER_THAN_START_TIME)]
        [PXUIField(DisplayName = "Actual End Time")]
        public virtual DateTime? ActualDateTimeEnd { get; set; }
        #endregion

        #region EarningType
        public abstract class earningType : PX.Data.BQL.BqlString.Field<earningType> { }

        [PXDBString(2, IsFixed = true, IsUnicode = false, InputMask = ">LL")]
        [PXDefault(typeof(Coalesce<Search2<FSxService.dfltEarningType, 
                            InnerJoin<FSAppointmentDet, 
                                On<FSAppointmentDet.appointmentID, Equal<Current<FSAppointment.appointmentID>>,
                                    And<FSAppointmentDet.lineRef, Equal<Current<FSAppointmentEmployee.serviceLineRef>>>>>,
                            Where<InventoryItem.inventoryID, Equal<FSAppointmentDet.inventoryID>>>,
                                   Search<FSSrvOrdType.dfltEarningType, 
                            Where<FSSrvOrdType.srvOrdTypeID, Equal<Current<FSSrvOrdType.srvOrdTypeID>>>>>), 
             PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<FSAppointmentEmployee.serviceLineRef>))]
        [PXSelector(typeof(EPEarningType.typeCD))]
        [PXUIField(DisplayName = "Earning Type")]
        public virtual string EarningType { get; set; }
        #endregion
        #region TrackTime
        public abstract class trackTime : PX.Data.BQL.BqlBool.Field<trackTime> { }

        [PXDBBool]
        [PXDefault(typeof(Search<FSSrvOrdType.createTimeActivitiesFromAppointment,
            Where<FSSrvOrdType.srvOrdTypeID, Equal<Current<FSSrvOrdType.srvOrdTypeID>>>>))]
        [PXUIField(DisplayName = "Track Time")]
        public virtual bool? TrackTime { get; set; }
        #endregion

        #region ApprovedTime
        public abstract class approvedTime : PX.Data.BQL.BqlBool.Field<approvedTime> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Approved Time", Enabled = false)]
        public virtual bool? ApprovedTime { get; set; }
        #endregion
        #region TimeCardCD
        public abstract class timeCardCD : PX.Data.BQL.BqlString.Field<timeCardCD> { }

        [PXUIField(DisplayName = "Time Card Ref. Nbr.", Enabled = false)]
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXSelector(typeof(Search<EPTimeCard.timeCardCD>),
            typeof(EPTimeCard.timeCardCD),
            typeof(EPTimeCard.employeeID),
            typeof(EPTimeCard.weekDescription),
            typeof(EPTimeCard.status))]
        public virtual string TimeCardCD { get; set; }
        #endregion
        #region CostCodeID
        public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

        [SMCostCode(typeof(skipCostCodeValidation), null, typeof(projectTaskID), Visible = false)]
        public virtual int? CostCodeID { get; set; }
        #endregion
        #region SkipCostCodeValidation
        public abstract class skipCostCodeValidation : PX.Data.BQL.BqlBool.Field<skipCostCodeValidation> { }

        [PXBool]
        [PXFormula(typeof(IIf<Where<Current<FSSrvOrdType.createTimeActivitiesFromAppointment>, Equal<True>,
                                And<Current<FSSetup.enableEmpTimeCardIntegration>, Equal<True>>>, False, True>))]
        public virtual bool? SkipCostCodeValidation { get; set; }
        #endregion

        #region ProjectTaskID
        public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }

        [PXInt]
        [PXUIField(DisplayName = "Project Task")]
        public virtual int? ProjectTaskID { get; set; }
        #endregion
        
        #region CuryInfoID
        public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
        [PXDBLong]
        [CurrencyInfo(typeof(FSAppointment.curyInfoID))]
        public virtual Int64? CuryInfoID { get; set; }
        #endregion
        #region CuryUnitCost
        public abstract class curyUnitCost : PX.Data.BQL.BqlDecimal.Field<curyUnitCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(unitCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Default<FSAppointmentEmployee.laborItemID>))]
        public virtual Decimal? CuryUnitCost { get; set; }
        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnitCost { get; set; }
        #endregion
        #region CuryExtCost
        public abstract class curyExtCost : PX.Data.BQL.BqlDecimal.Field<curyExtCost> { }

        [PXDBCurrency(typeof(curyInfoID), typeof(extCost))]
        [PXUIField(Visible = false, Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXFormula(typeof(Div<Mult<curyUnitCost, actualDuration>, SharedClasses.decimal_60>), typeof(SumCalc<FSAppointment.curyCostTotal>))]
        public virtual Decimal? CuryExtCost { get; set; }
        #endregion
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }

        [PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? ExtCost { get; set; }
        #endregion
        #region LaborItem
        public abstract class laborItemID : PX.Data.BQL.BqlInt.Field<laborItemID> { }

        [PXDBInt]
        [PXSelector(typeof(Search<InventoryItem.inventoryID,
                           Where<
                               InventoryItem.itemType, Equal<INItemTypes.laborItem>>>)
            , SubstituteKey = typeof(InventoryItem.inventoryCD))]
        [PXUIField(DisplayName = "Labor Item")]
        [PXDefault(typeof(Search<EPEmployee.labourItemID, Where<EPEmployee.bAccountID, Equal<Current<FSAppointmentEmployee.employeeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual int? LaborItemID { get; set; }
        #endregion

        #region Mem_Selected
        public abstract class mem_Selected : PX.Data.BQL.BqlBool.Field<mem_Selected> { }

        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Mem_Selected { get; set; }
        #endregion
        #region ActualDurationReport
        public abstract class actualDurationReport : PX.Data.BQL.BqlInt.Field<actualDurationReport> { }

        [PXInt]
        [PXFormula(typeof(FSAppointmentEmployee.actualDuration))]
        public virtual int? ActualDurationReport { get; set; }
        #endregion
    }
}