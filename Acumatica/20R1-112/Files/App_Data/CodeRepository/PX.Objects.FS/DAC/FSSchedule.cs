using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.FS
{	
	[System.SerializableAttribute]
    [PXPrimaryGraph(
       new Type[] {
                    typeof(ServiceContractScheduleEntry),
                    typeof(RouteServiceContractScheduleEntry)
            },
       new Type[] {
                    typeof(Search2<FSSchedule.refNbr,
                           InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSSchedule.srvOrdType>>>,
                           Where<FSSchedule.entityID, Equal<Current<FSSchedule.entityID>>,
                               And<FSSrvOrdType.behavior, NotEqual<ListField_Behavior_SrvOrdType.RouteAppointment>>>>),
                    typeof(Search2<FSSchedule.refNbr,
                            InnerJoin<FSSrvOrdType,
                                On<FSSrvOrdType.srvOrdType, Equal<FSSchedule.srvOrdType>>>,
                           Where<FSSchedule.entityID, Equal<Current<FSSchedule.entityID>>,
                               And<FSSrvOrdType.behavior, Equal<ListField_Behavior_SrvOrdType.RouteAppointment>>>>)
            })]
    public class FSSchedule : PX.Data.IBqlTable
    {
        #region Key Fields
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXDefault]
        [PXUIField(DisplayName = "Ref. Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search3<FSSchedule.refNbr, OrderBy<Desc<FSSchedule.refNbr>>>))]
        public virtual string RefNbr { get; set; }
        #endregion   
        #endregion

        #region ScheduleID
        public abstract class scheduleID : PX.Data.BQL.BqlInt.Field<scheduleID> { }
        [PXDBIdentity]
        [PXUIField(Enabled = false)]
        public virtual int? ScheduleID { get; set; }
        #endregion
        #region Attributes
        /// <summary>
        /// A service field, which is necessary for the <see cref="CSAnswers">dynamically 
        /// added attributes</see> defined at the <see cref="FSSrvOrdType">customer 
        /// class</see> level to function correctly.
        /// </summary>
        [CRAttributesField(typeof(FSSchedule.srvOrdType), typeof(FSSchedule.noteID))]
        public virtual string[] Attributes { get; set; }
        #endregion
        #region Active
        public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
        [PXDefault(true)]
        [PXDBBool]
        [PXUIField(DisplayName = "Active")]
        public virtual bool? Active { get; set; }
        #endregion
        #region AnnualFrequency
        public abstract class annualFrequency : PX.Data.BQL.BqlShort.Field<annualFrequency> { }
        [PXDefault((short)1)]
        [PXDBShort(MinValue = 0)]
        [PXUIField(DisplayName = "Every")]
        public virtual short? AnnualFrequency { get; set; }
        #endregion
        #region AnnualOnDay
        public abstract class annualOnDay : PX.Data.BQL.BqlShort.Field<annualOnDay> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On Day 1")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault((short)1)]
        public virtual short? AnnualOnDay { get; set; }
        #endregion
        #region AnnualOnDayOfWeek
        public abstract class annualOnDayOfWeek : ListField_WeekDaysNumber
        {
        }
        [PXDBShort]
        [PXUIField(DisplayName = "Day of Week 1")]
        [annualOnDayOfWeek.ListAtrribute]	
        [PXDefault((short)1)]
        public virtual short? AnnualOnDayOfWeek { get; set; }
        #endregion
        #region AnnualOnWeek
        public abstract class annualOnWeek : PX.Data.BQL.BqlShort.Field<annualOnWeek> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On the 1")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "First", "Second", "Third", "Fourth", "Last" })]
        [PXDefault((short)1)]
        public virtual short? AnnualOnWeek { get; set; }
        #endregion
        #region AnnualRecurrenceType
        public abstract class annualRecurrenceType : PX.Data.BQL.BqlString.Field<annualRecurrenceType> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Day Selection 1")]
        [PXStringList(new string[] { ID.ScheduleMonthlySelection.DAILY, ID.ScheduleMonthlySelection.WEEKLY }, new string[] { "Fixed Day of Month", "Fixed Day of Week" })]
        [PXDefault(ID.ScheduleMonthlySelection.DAILY)]
        public virtual string AnnualRecurrenceType { get; set; }
        #endregion
        #region AnnualOnJan
        public abstract class annualOnJan : PX.Data.BQL.BqlBool.Field<annualOnJan> { }
        [PXDBBool]
        [PXUIField(DisplayName = "January")]
        [PXDefault(false)]
        public virtual bool? AnnualOnJan { get; set; }
        #endregion
        #region AnnualOnFeb
        public abstract class annualOnFeb : PX.Data.BQL.BqlBool.Field<annualOnFeb> { }
        [PXDBBool]
        [PXUIField(DisplayName = "February")]
        [PXDefault(false)]
        public virtual bool? AnnualOnFeb { get; set; }
        #endregion
        #region AnnualOnMar
        public abstract class annualOnMar : PX.Data.BQL.BqlBool.Field<annualOnMar> { }
        [PXDBBool]
        [PXUIField(DisplayName = "March")]
        [PXDefault(false)]
        public virtual bool? AnnualOnMar { get; set; }
        #endregion
        #region AnnualOnApr
        public abstract class annualOnApr : PX.Data.BQL.BqlBool.Field<annualOnApr> { }
        [PXDBBool]
        [PXUIField(DisplayName = "April")]
        [PXDefault(false)]
        public virtual bool? AnnualOnApr { get; set; }
        #endregion
        #region AnnualOnMay
        public abstract class annualOnMay : PX.Data.BQL.BqlBool.Field<annualOnMay> { }
        [PXDBBool]
        [PXUIField(DisplayName = "May")]
        [PXDefault(false)]
        public virtual bool? AnnualOnMay { get; set; }
        #endregion
        #region AnnualOnJun
        public abstract class annualOnJun : PX.Data.BQL.BqlBool.Field<annualOnJun> { }
        [PXDBBool]
        [PXUIField(DisplayName = "June")]
        [PXDefault(false)]
        public virtual bool? AnnualOnJun { get; set; }
        #endregion
        #region AnnualOnJul
        public abstract class annualOnJul : PX.Data.BQL.BqlBool.Field<annualOnJul> { }
        [PXDBBool]
        [PXUIField(DisplayName = "July")]
        [PXDefault(false)]
        public virtual bool? AnnualOnJul { get; set; }
        #endregion
        #region AnnualOnAug
        public abstract class annualOnAug : PX.Data.BQL.BqlBool.Field<annualOnAug> { }
        [PXDBBool]
        [PXUIField(DisplayName = "August")]
        [PXDefault(false)]
        public virtual bool? AnnualOnAug { get; set; }
        #endregion
        #region AnnualOnSep
        public abstract class annualOnSep : PX.Data.BQL.BqlBool.Field<annualOnSep> { }
        [PXDBBool]
        [PXUIField(DisplayName = "September")]
        [PXDefault(false)]
        public virtual bool? AnnualOnSep { get; set; }
        #endregion
        #region AnnualOnOct
        public abstract class annualOnOct : PX.Data.BQL.BqlBool.Field<annualOnOct> { }
        [PXDBBool]
        [PXUIField(DisplayName = "October")]
        [PXDefault(false)]
        public virtual bool? AnnualOnOct { get; set; }
        #endregion
        #region AnnualOnNov
        public abstract class annualOnNov : PX.Data.BQL.BqlBool.Field<annualOnNov> { }
        [PXDBBool]
        [PXUIField(DisplayName = "November")]
        [PXDefault(false)]
        public virtual bool? AnnualOnNov { get; set; }
        #endregion
        #region AnnualOnDec
        public abstract class annualOnDec : PX.Data.BQL.BqlBool.Field<annualOnDec> { }
        [PXDBBool]
        [PXUIField(DisplayName = "December")]
        [PXDefault(false)]
        public virtual bool? AnnualOnDec { get; set; }
        #endregion
        #region BranchID
        public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
        [PXDBInt]
        [PXDefault(typeof(AccessInfo.branchID))]
        [PXUIField(DisplayName = "Branch ID")]
        [PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD), DescriptionField = typeof(Branch.acctName))]
        public virtual int? BranchID { get; set; }
        #endregion
        #region BranchLocationID
        public abstract class branchLocationID : PX.Data.BQL.BqlInt.Field<branchLocationID> { }
        [PXDBInt]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Branch Location ID")]
        [FSSelectorBranchLocationByFSSchedule]
        [PXFormula(typeof(Default<FSSchedule.branchID>))]
        public virtual int? BranchLocationID { get; set; }
        #endregion
        #region CustomerID
        public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Customer", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        [FSSelectorBAccountTypeCustomerOrCombined]
        public virtual int? CustomerID { get; set; }
        #endregion
        #region DailyFrequency
        public abstract class dailyFrequency : PX.Data.BQL.BqlShort.Field<dailyFrequency> { }
        [PXDefault((short)1)]
        [PXDBShort(MinValue = 0)]
        [PXUIField(DisplayName = "Every")]
        public virtual short? DailyFrequency { get; set; }
        #endregion
        #region EndDate
        //CacheAttached applied in RouteServiceContractScheduleEntry
        //CacheAttached applied in ServiceContractScheduleEntry
        //CacheAttached applied in RouteScheduleProcess
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
        protected DateTime? _EndDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Expiration Date")]
        [PXFormula(typeof(Default<FSSchedule.enableExpirationDate>))]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }

            set
            {
                this.EndDateUTC = value;
                this._EndDate = value;
            }
        }
        #endregion
        #region EntityID
        public abstract class entityID : PX.Data.BQL.BqlInt.Field<entityID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Entity ID", Enabled = false)]
        public virtual int? EntityID { get; set; }
        #endregion
        #region EntityType
        public abstract class entityType : ListField_Schedule_EntityType
        {
        }
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ID.Schedule_EntityType.CONTRACT)]
        [entityType.ListAtrribute]
        [PXUIField(DisplayName = "Entity Type", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
        public virtual string EntityType { get; set; }
        #endregion
		#region FrequencyType
        public abstract class frequencyType : ListField_FrequencyType
        {
        }
        [PXDBString(1, IsFixed = true)]
        [frequencyType.ListAtrribute]
        [PXDefault(ID.Schedule_FrequencyType.DAILY)]
        [PXUIField(DisplayName = "Frequency Type")]
        public virtual string FrequencyType { get; set; }
		#endregion
        #region LastGeneratedElementDate
        public abstract class lastGeneratedElementDate : PX.Data.BQL.BqlDateTime.Field<lastGeneratedElementDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Last Generated")]
        [PXFormula(typeof(Default<FSSchedule.startDate>))]
        public virtual DateTime? LastGeneratedElementDate { get; set; }
        #endregion
        #region LineCntr
        public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr>
		{
        }

        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntr { get; set; }
        #endregion
        #region Monthly2Selected
        public abstract class monthly2Selected : PX.Data.BQL.BqlBool.Field<monthly2Selected> { }
        [PXDefault(false)]
        [PXDBBool]
        [PXUIField(DisplayName = "Monthly 2 Selected")]
        public virtual bool? Monthly2Selected { get; set; }
        #endregion
        #region Monthly3Selected
        public abstract class monthly3Selected : PX.Data.BQL.BqlBool.Field<monthly3Selected> { }
        [PXDefault(false)]
        [PXDBBool]
        [PXUIField(DisplayName = "Monthly 3 Selected")]
        public virtual bool? Monthly3Selected { get; set; }
        #endregion
        #region Monthly4Selected
        public abstract class monthly4Selected : PX.Data.BQL.BqlBool.Field<monthly4Selected> { }
        [PXDefault(false)]
        [PXDBBool]
        [PXUIField(DisplayName = "Monthly 4 Selected")]
        public virtual bool? Monthly4Selected { get; set; }
        #endregion
        #region MonthlyFrequency
        public abstract class monthlyFrequency : PX.Data.BQL.BqlShort.Field<monthlyFrequency> { }
        [PXDBShort]
        [PXUIField(DisplayName = "Every")]
        [PXDefault((short)1)]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" })]
        public virtual short? MonthlyFrequency { get; set; }
        #endregion
        #region MonthlyRecurrenceType1
        public abstract class monthlyRecurrenceType1 : PX.Data.BQL.BqlString.Field<monthlyRecurrenceType1> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Day Selection 1")]
        [PXStringList(new string[] { ID.ScheduleMonthlySelection.DAILY, ID.ScheduleMonthlySelection.WEEKLY }, new string[] { "Fixed Day of Month", "Fixed Day of Week" })]
        [PXDefault(ID.ScheduleMonthlySelection.DAILY)]
        public virtual string MonthlyRecurrenceType1 { get; set; }
        #endregion
        #region MonthlyRecurrenceType2
        public abstract class monthlyRecurrenceType2 : PX.Data.BQL.BqlString.Field<monthlyRecurrenceType2> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Day Selection 2")]
        [PXStringList(new string[] { ID.ScheduleMonthlySelection.DAILY, ID.ScheduleMonthlySelection.WEEKLY }, new string[] { "Fixed Day of Month", "Fixed Day of Week" })]
        [PXDefault(ID.ScheduleMonthlySelection.DAILY)]
        public virtual string MonthlyRecurrenceType2 { get; set; }
        #endregion
        #region MonthlyRecurrenceType3
        public abstract class monthlyRecurrenceType3 : PX.Data.BQL.BqlString.Field<monthlyRecurrenceType3> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Day Selection 3")]
        [PXStringList(new string[] { ID.ScheduleMonthlySelection.DAILY, ID.ScheduleMonthlySelection.WEEKLY }, new string[] { "Fixed Day of Month", "Fixed Day of Week" })]
        [PXDefault(ID.ScheduleMonthlySelection.DAILY)]
        public virtual string MonthlyRecurrenceType3 { get; set; }
        #endregion
        #region MonthlyRecurrenceType4
        public abstract class monthlyRecurrenceType4 : PX.Data.BQL.BqlString.Field<monthlyRecurrenceType4> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Day Selection 4")]
        [PXStringList(new string[] { ID.ScheduleMonthlySelection.DAILY, ID.ScheduleMonthlySelection.WEEKLY }, new string[] { "Fixed Day of Month", "Fixed Day of Week" })]
        [PXDefault(ID.ScheduleMonthlySelection.DAILY)]
        public virtual string MonthlyRecurrenceType4 { get; set; }
        #endregion
        #region MonthlyOnDay1
        public abstract class monthlyOnDay1 : PX.Data.BQL.BqlShort.Field<monthlyOnDay1> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On Day 1")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDay1 { get; set; }
        #endregion
        #region MonthlyOnDay2
        public abstract class monthlyOnDay2 : PX.Data.BQL.BqlShort.Field<monthlyOnDay2> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On Day 2")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDay2 { get; set; }
        #endregion
        #region MonthlyOnDay3
        public abstract class monthlyOnDay3 : PX.Data.BQL.BqlShort.Field<monthlyOnDay3> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On Day 3")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDay3 { get; set; }
        #endregion
        #region MonthlyOnDay4
        public abstract class monthlyOnDay4 : PX.Data.BQL.BqlShort.Field<monthlyOnDay4> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On Day 4")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDay4 { get; set; }
        #endregion
        #region MonthlyOnWeek1
        public abstract class monthlyOnWeek1 : PX.Data.BQL.BqlShort.Field<monthlyOnWeek1> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On the 1")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "First", "Second", "Third", "Fourth", "Last" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnWeek1 { get; set; }
        #endregion
        #region MonthlyOnWeek2
        public abstract class monthlyOnWeek2 : PX.Data.BQL.BqlShort.Field<monthlyOnWeek2> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On the 2")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "First", "Second", "Third", "Fourth", "Last" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnWeek2 { get; set; }
        #endregion
        #region MonthlyOnWeek3
        public abstract class monthlyOnWeek3 : PX.Data.BQL.BqlShort.Field<monthlyOnWeek3> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On the 3")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "First", "Second", "Third", "Fourth", "Last" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnWeek3 { get; set; }
        #endregion
        #region MonthlyOnWeek4
        public abstract class monthlyOnWeek4 : PX.Data.BQL.BqlShort.Field<monthlyOnWeek4> { }
        [PXDBShort]
        [PXUIField(DisplayName = "On the 4")]
        [PXIntList(new int[] { 1, 2, 3, 4, 5 }, new string[] { "First", "Second", "Third", "Fourth", "Last" })]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnWeek4 { get; set; }
        #endregion
        #region MonthlyOnDayOfWeek1
        public abstract class monthlyOnDayOfWeek1 : ListField_WeekDaysNumber
        {
        }
        [PXDBShort]
        [PXUIField(DisplayName = "Day of Week 1")]
        [monthlyOnDayOfWeek1.ListAtrribute]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDayOfWeek1 { get; set; }
        #endregion
        #region MonthlyOnDayOfWeek2
        public abstract class monthlyOnDayOfWeek2 : ListField_WeekDaysNumber
        {
        }
        [PXDBShort]
        [PXUIField(DisplayName = "Day of Week 2")]
        [monthlyOnDayOfWeek2.ListAtrribute]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDayOfWeek2 { get; set; }
        #endregion
        #region MonthlyOnDayOfWeek3
        public abstract class monthlyOnDayOfWeek3 : ListField_WeekDaysNumber
        {
        }
        [PXDBShort]
        [PXUIField(DisplayName = "Day of Week 3")]
        [monthlyOnDayOfWeek3.ListAtrribute]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDayOfWeek3 { get; set; }
        #endregion
        #region MonthlyOnDayOfWeek4
        public abstract class monthlyOnDayOfWeek4 : ListField_WeekDaysNumber
        {
        }
        [PXDBShort]
        [PXUIField(DisplayName = "Day of Week 4")]
        [monthlyOnDayOfWeek4.ListAtrribute]
        [PXDefault((short)1)]
        public virtual short? MonthlyOnDayOfWeek4 { get; set; }
        #endregion
        #region NoRunLimit
        public abstract class noRunLimit : PX.Data.BQL.BqlBool.Field<noRunLimit> { }
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "No Limit")]
        public virtual bool? NoRunLimit { get; set; }
        #endregion
        #region RestrictionMax
        public abstract class restrictionMax : PX.Data.BQL.BqlBool.Field<restrictionMax> { }
        [PXDefault(false)]
        [PXDBBool]
        [PXUIField(DisplayName = "Enable Max. Restriction")]
        public virtual bool? RestrictionMax { get; set; }
        #endregion
		#region RestrictionMin
		public abstract class restrictionMin : PX.Data.BQL.BqlBool.Field<restrictionMin> { }
        [PXDefault(false)]
        [PXDBBool]
        [PXUIField(DisplayName = "Enable Min. Restriction")]
        public virtual bool? RestrictionMin { get; set; }
        #endregion
        #region RestrictionMaxTime
        public abstract class restrictionMaxTime : PX.Data.BQL.BqlDateTime.Field<restrictionMaxTime> { }
        protected DateTime? _RestrictionMaxTime;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Maximum Time Restriction", DisplayNameTime = "Maximum Time Restriction")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Maximum Time Restriction")]
        public virtual DateTime? RestrictionMaxTime
        {
            get
            {
                return this._RestrictionMaxTime;
            }

            set
            {
                this.RestrictionMaxTimeUTC = value;
                this._RestrictionMaxTime = value;
            }
        }
        #endregion
        #region RestrictionMinTime
        public abstract class restrictionMinTime : PX.Data.BQL.BqlDateTime.Field<restrictionMinTime> { }
        protected DateTime? _RestrictionMinTime;
        [PXDBDateAndTime(UseTimeZone = true, PreserveTime = true, DisplayNameDate = "Minimum Time Restriction", DisplayNameTime = "Minimum Time Restriction")]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Minimum Time Restriction")]
        public virtual DateTime? RestrictionMinTime
        {
            get
            {
                return this._RestrictionMinTime;
            }

            set
            {
                this.RestrictionMinTimeUTC = value;
                this._RestrictionMinTime = value;
            }
        }
        #endregion
        #region RunCntr
        public abstract class runCntr : PX.Data.BQL.BqlShort.Field<runCntr> { }
        [PXDefault((short)0)]
        [PXDBShort]
        [PXUIField(DisplayName = "Executed (times)")]
        public virtual short? RunCntr { get; set; } //deshabilitar en el ASPX
        #endregion 
        #region RunLimit
        public abstract class runLimit : PX.Data.BQL.BqlShort.Field<runLimit> { }      
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXDBShort(MinValue = 0)]
        [PXUIField(DisplayName = "Execution Limit (times)", Enabled = false)]
        public virtual short? RunLimit { get; set; }
        #endregion
        #region SrvOrdType
        public abstract class srvOrdType : PX.Data.BQL.BqlString.Field<srvOrdType> { }
        [PXDBString(4, IsFixed = true, InputMask = ">AAAA")]
        [PXUIField(DisplayName = "Service Order Type")]
        [PXDefault]
        public virtual string SrvOrdType { get; set; }
        #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Start Date")]
        [PXDefault(typeof(AccessInfo.businessDate))]
        public virtual DateTime? StartDate { get; set; }
        #endregion       
        #region WeeklyFrequency
        public abstract class weeklyFrequency : PX.Data.BQL.BqlShort.Field<weeklyFrequency> { }
        [PXDBShort(MinValue = 0)]
        [PXUIField(DisplayName = "Every")]
        [PXDefault((short)1)]
        public virtual short? WeeklyFrequency { get; set; }
        #endregion
        #region WeeklyOnSun
        public abstract class weeklyOnSun : PX.Data.BQL.BqlBool.Field<weeklyOnSun> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Sunday")]
        [PXDefault(true)]
        public virtual bool? WeeklyOnSun { get; set; }
        #endregion
        #region WeeklyOnMon
        public abstract class weeklyOnMon : PX.Data.BQL.BqlBool.Field<weeklyOnMon> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Monday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnMon { get; set; }
        #endregion
        #region WeeklyOnTue
        public abstract class weeklyOnTue : PX.Data.BQL.BqlBool.Field<weeklyOnTue> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Tuesday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnTue { get; set; }
        #endregion
        #region WeeklyOnWed
        public abstract class weeklyOnWed : PX.Data.BQL.BqlBool.Field<weeklyOnWed> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Wednesday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnWed { get; set; }
        #endregion
        #region WeeklyOnThu
        public abstract class weeklyOnThu : PX.Data.BQL.BqlBool.Field<weeklyOnThu> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Thursday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnThu { get; set; }
        #endregion
        #region WeeklyOnFri
        public abstract class weeklyOnFri : PX.Data.BQL.BqlBool.Field<weeklyOnFri> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Friday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnFri { get; set; }
        #endregion
        #region WeeklyOnSat
        public abstract class weeklyOnSat : PX.Data.BQL.BqlBool.Field<weeklyOnSat> { }
        [PXDBBool]
        [PXUIField(DisplayName = "Saturday")]
        [PXDefault(false)]
        public virtual bool? WeeklyOnSat { get; set; }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Vendor", Enabled = true)]
        [FSSelectorVendor]
        public virtual int? VendorID { get; set; }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        [PXUIField(DisplayName = "NoteID")]
        [PXNote(new Type[0])]
        public virtual Guid? NoteID { get; set; }
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
        [PXUIField(DisplayName = "Created On")]
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
        [PXUIField(DisplayName = "Last Modified On")]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
        [PXDBTimestamp]
        public virtual byte[] tstamp { get; set; }
        #endregion
        #region VehicleTypeID
        public abstract class vehicleTypeID : PX.Data.BQL.BqlInt.Field<vehicleTypeID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Vehicle Type ID")]
        [PXSelector(typeof(FSVehicleType.vehicleTypeID), SubstituteKey = typeof(FSVehicleType.vehicleTypeCD))]
        public virtual int? VehicleTypeID { get; set; }
        #endregion
        #region RecurrenceDescription
        public abstract class recurrenceDescription : PX.Data.BQL.BqlString.Field<recurrenceDescription> { }
        [PXDBString(int.MaxValue, IsUnicode = true)]
        [PXUIField(DisplayName = "Recurrence Description")]
        [PXDefault(TX.RecurrenceDescription.OCCURS_EVERY , PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string RecurrenceDescription { get; set; }
        #endregion
        #region EmployeeID
        public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
        [PXDBInt]
        [FSSelector_Employee_AllAttribute]
        [PXUIField(DisplayName = "Employee ID")]
        public virtual int? EmployeeID { get; set; }
        #endregion
        #region ScheduleType
        public abstract class scheduleType : ListField_ScheduleType
        {
        }
        [scheduleType.ListAtrribute]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Schedule Type")]
        public virtual string ScheduleType { get; set; }
        #endregion
        #region WeekCode
        public abstract class weekCode : PX.Data.BQL.BqlString.Field<weekCode> { }
        [PXDBString]
        [PXUIField(DisplayName = "Week Codes e.g.: 1, 2B, 1ACS")]
        public virtual string WeekCode { get; set; }
        #endregion
        #region SeasonOnJan
        public abstract class seasonOnJan : PX.Data.BQL.BqlBool.Field<seasonOnJan> { }
        [PXDBBool]
        [PXUIField(DisplayName = "January")]
        [PXDefault(true)]
        public virtual bool? SeasonOnJan { get; set; }
        #endregion
        #region SeasonOnFeb
        public abstract class seasonOnFeb : PX.Data.BQL.BqlBool.Field<seasonOnFeb> { }
        [PXDBBool]
        [PXUIField(DisplayName = "February")]
        [PXDefault(true)]
        public virtual bool? SeasonOnFeb { get; set; }
        #endregion
        #region SeasonOnMar
        public abstract class seasonOnMar : PX.Data.BQL.BqlBool.Field<seasonOnMar> { }
        [PXDBBool]
        [PXUIField(DisplayName = "March")]
        [PXDefault(true)]
        public virtual bool? SeasonOnMar { get; set; }
        #endregion
        #region SeasonOnApr
        public abstract class seasonOnApr : PX.Data.BQL.BqlBool.Field<seasonOnApr> { }
        [PXDBBool]
        [PXUIField(DisplayName = "April")]
        [PXDefault(true)]
        public virtual bool? SeasonOnApr { get; set; }
        #endregion
        #region SeasonOnMay
        public abstract class seasonOnMay : PX.Data.BQL.BqlBool.Field<seasonOnMay> { }
        [PXDBBool]
        [PXUIField(DisplayName = "May")]
        [PXDefault(true)]
        public virtual bool? SeasonOnMay { get; set; }
        #endregion
        #region SeasonOnJun
        public abstract class seasonOnJun : PX.Data.BQL.BqlBool.Field<seasonOnJun> { }
        [PXDBBool]
        [PXUIField(DisplayName = "June")]
        [PXDefault(true)]
        public virtual bool? SeasonOnJun { get; set; }
        #endregion
        #region SeasonOnJul
        public abstract class seasonOnJul : PX.Data.BQL.BqlBool.Field<seasonOnJul> { }
        [PXDBBool]
        [PXUIField(DisplayName = "July")]
        [PXDefault(true)]
        public virtual bool? SeasonOnJul { get; set; }
        #endregion
        #region SeasonOnAug
        public abstract class seasonOnAug : PX.Data.BQL.BqlBool.Field<seasonOnAug> { }
        [PXDBBool]
        [PXUIField(DisplayName = "August")]
        [PXDefault(true)]
        public virtual bool? SeasonOnAug { get; set; }
        #endregion
        #region SeasonOnSep
        public abstract class seasonOnSep : PX.Data.BQL.BqlBool.Field<seasonOnSep> { }
        [PXDBBool]
        [PXUIField(DisplayName = "September")]
        [PXDefault(true)]
        public virtual bool? SeasonOnSep { get; set; }
        #endregion
        #region SeasonOnOct
        public abstract class seasonOnOct : PX.Data.BQL.BqlBool.Field<seasonOnOct> { }
        [PXDBBool]
        [PXUIField(DisplayName = "October")]
        [PXDefault(true)]
        public virtual bool? SeasonOnOct { get; set; }
        #endregion
        #region SeasonOnNov
        public abstract class seasonOnNov : PX.Data.BQL.BqlBool.Field<seasonOnNov> { }
        [PXDBBool]
        [PXUIField(DisplayName = "November")]
        [PXDefault(true)]
        public virtual bool? SeasonOnNov { get; set; }
        #endregion
        #region SeasonOnDec
        public abstract class seasonOnDec : PX.Data.BQL.BqlBool.Field<seasonOnDec> { }
        [PXDBBool]
        [PXUIField(DisplayName = "December")]
        [PXDefault(true)]
        public virtual bool? SeasonOnDec { get; set; }
        #endregion 
        #region EnableExpirationDate
        public abstract class enableExpirationDate : PX.Data.BQL.BqlBool.Field<enableExpirationDate> { }

        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Enable Expiration Date")]
        public virtual bool? EnableExpirationDate { get; set; }
        #endregion
        #region Unbound Fields
        #region YearlyLabel
        public abstract class yearlyLabel : PX.Data.BQL.BqlString.Field<yearlyLabel> { }

        [PXString]
        [PXUIField(Visible = false)]
        public virtual string YearlyLabel
        {
            get
            {
                return SharedFunctions.Localize(TX.RecurrenceDescription.YEARS);
            }
        }
        #endregion
        #region MonthlyLabel
        public abstract class monthlyLabel : PX.Data.BQL.BqlString.Field<monthlyLabel> { }

        [PXString]
        [PXUIField(Visible = false)]
        public virtual string MonthlyLabel
        {
            get
            {
                return SharedFunctions.Localize(TX.RecurrenceDescription.MONTHS);
            }
        }
        #endregion
        #region WeeklyLabel
        public abstract class weeklyLabel : PX.Data.BQL.BqlString.Field<weeklyLabel> { }

        [PXString]
        [PXUIField(Visible = false)]
        public virtual string WeeklyLabel
        {
            get
            {
                return SharedFunctions.Localize(TX.RecurrenceDescription.WEEKS);
            }
        }
        #endregion
        #region DailyLabel
        public abstract class dailyLabel : PX.Data.BQL.BqlString.Field<dailyLabel> { }

        [PXString]
        [PXUIField(Visible = false)]
        public virtual string DailyLabel
        {
            get
            {
                return SharedFunctions.Localize(TX.RecurrenceDescription.DAYS);
            }
        }
        #endregion
        #region SrvOrdTypeMessage
        public abstract class srvOrdTypeMessage : PX.Data.BQL.BqlString.Field<srvOrdTypeMessage> { }
        [PXString]
        [PXUIField]
        [PXDefault("This Service Order Type will be used for the recurring appointments", PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string SrvOrdTypeMessage { get; set; }
        #endregion
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }

        #endregion
        #region ContractDesc
        public abstract class contractDescr : PX.Data.BQL.BqlString.Field<contractDescr> { }
        [PXString]
        [PXUIField]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string ContractDescr { get; set; }
        #endregion
        #region CustomerLocationID
        public abstract class customerLocationID : PX.Data.BQL.BqlInt.Field<customerLocationID> { }
        [PXDBInt]
        [PXUIField(DisplayName = "Location")]
        [PXDimensionSelector(LocationIDBaseAttribute.DimensionName,
                typeof(Search<Location.locationID,
                        Where<
                            Location.bAccountID, Equal<Current<FSSchedule.customerID>>>>),
                typeof(Location.locationCD),
                DescriptionField = typeof(Location.descr))]
        public virtual int? CustomerLocationID { get; set; }
        #endregion
        #endregion
        #region ScheduleGenType
        public abstract class scheduleGenType : ListField_ScheduleGenType_ContractSchedule
        {
        }

        [PXDBString(2, IsUnicode = true)]
        [scheduleGenType.ListAtrribute]
        [PXUIField(DisplayName = "Schedule Generation Type")]
        [PXDefault(typeof(FSServiceContract.scheduleGenType.None), PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual string ScheduleGenType { get; set; }
        #endregion
        #region ScheduleStartTime
        public abstract class scheduleStartTime : PX.Data.BQL.BqlDateTime.Field<scheduleStartTime> { }

        [PXDBDateAndTime(PreserveTime = true, DisplayNameTime = "Schedule Start Time")]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Schedule Start Time")]
        public virtual DateTime? ScheduleStartTime { get; set; }
        #endregion
        #region NextExecutionDate 
        public abstract class nextExecutionDate : PX.Data.BQL.BqlDateTime.Field<nextExecutionDate> { }
        [PXDBDate]
        [PXUIField(DisplayName = "Next Execution Date", Enabled = false)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual DateTime? NextExecutionDate { get; set; }
        #endregion  
        #region UTC Fields
        #region RestrictionMaxTimeUTC
        public abstract class restrictionMaxTimeUTC : PX.Data.BQL.BqlDateTime.Field<restrictionMaxTimeUTC> { }
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Maximum Time Restriction", DisplayNameTime = "Maximum Time Restriction")]
        [PXUIField(DisplayName = "Maximum Time Restriction")]
        public virtual DateTime? RestrictionMaxTimeUTC { get; set; }
        #endregion
        #region RestrictionMinTimeUTC
        public abstract class restrictionMinTimeUTC : PX.Data.BQL.BqlDateTime.Field<restrictionMinTimeUTC> { }
        [PXDBDateAndTime(UseTimeZone = false, PreserveTime = true, DisplayNameDate = "Minimum Time Restriction", DisplayNameTime = "Minimum Time Restriction")]
        [PXUIField(DisplayName = "Minimum Time Restriction")]
        public virtual DateTime? RestrictionMinTimeUTC { get; set; }
        #endregion
        #region EndDateUTC
        public abstract class endDateUTC : PX.Data.BQL.BqlDateTime.Field<endDateUTC> { }
        [PXDBDate(UseTimeZone = false)]
        [PXUIField(DisplayName = "Expiration Date")]
        public virtual DateTime? EndDateUTC { get; set; }
        #endregion
        #endregion

        #region ReportScheduleID
        public abstract class reportScheduleID : PX.Data.BQL.BqlInt.Field<reportScheduleID> { }

        [PXInt]
        [PXSelector(typeof(Search2<FSSchedule.refNbr,
                            InnerJoin<FSServiceContract, On<FSServiceContract.serviceContractID, Equal<FSSchedule.entityID>>>,
                           Where<
                                FSServiceContract.recordType, Equal<FSServiceContract.recordType.ServiceContract>,
                                And<FSServiceContract.refNbr, Equal<Optional<FSServiceContract.refNbr>>,
                                And<FSSchedule.customerID, Equal<Optional<FSSchedule.customerID>>>>>>),
                            new Type[]
                            {
                                typeof(FSSchedule.refNbr),
                                typeof(FSSchedule.customerID),
                                typeof(FSSchedule.active),
                                typeof(FSSchedule.customerLocationID)
                            })]

        [PXUIField(DisplayName = "Schedule ID", Enabled = false)]
        public virtual int? ReportScheduleID { get; set; }
        #endregion

        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [ProjectBase(typeof(FSSchedule.billCustomerID), Enabled = false)]
        [PXForeignReference(typeof(Field<projectID>.IsRelatedTo<PMProject.contractID>))]
        public virtual int? ProjectID { get; set; }
        #endregion
        #region DfltProjectTaskID
        public abstract class dfltProjectTaskID : PX.Data.BQL.BqlInt.Field<dfltProjectTaskID> { }

        [PXDBInt]
        [PXUIField(DisplayName = "Default Project Task", Enabled = false, FieldClass = ProjectAttribute.DimensionName)]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [FSSelectorActive_AR_SO_ProjectTask(typeof(Where<PMTask.projectID, Equal<Current<projectID>>>))]
        [PXForeignReference(typeof(Field<dfltProjectTaskID>.IsRelatedTo<PMTask.taskID>))]
        public virtual int? DfltProjectTaskID { get; set; }
        #endregion

        public abstract class billCustomerID : PX.Data.BQL.BqlInt.Field<billCustomerID> { }
        [PXInt]
        [PXDefault(typeof(Search<FSServiceContract.billCustomerID, 
                            Where<FSServiceContract.serviceContractID, Equal<Current<FSSchedule.entityID>>,
                                And<Current<FSSchedule.entityType>, Equal<FSSchedule.entityType.Contract>>>>), 
                            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Billing Customer")]
        public virtual int? BillCustomerID { get; set; }

        public static bool TryParse(object row, out FSSchedule fsScheduleRow)
        {
            fsScheduleRow = null;

            if (row is FSSchedule)
            {
                fsScheduleRow = (FSSchedule)row;
                return true;
            }

            return false;
        }
    }
}

