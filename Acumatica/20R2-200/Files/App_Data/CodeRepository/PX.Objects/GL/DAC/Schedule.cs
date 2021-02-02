using System;

using PX.Data;
using PX.Data.EP;

using PX.Objects.CS;

namespace PX.Objects.GL
{

	[PXCacheName(Messages.Schedule)]
	[System.SerializableAttribute()]
	[PXPrimaryGraph(new Type[] { 
		typeof(GL.ScheduleMaint), 
		typeof(AP.APScheduleMaint), 
		typeof(AR.ARScheduleMaint),
        typeof(WZ.WZScheduleMaint)
	},
		new Type[] { 
			typeof(Where<Schedule.module, Equal<BatchModule.moduleGL>>), 
			typeof(Where<Schedule.module, Equal<BatchModule.moduleAP>>),
			typeof(Where<Schedule.module, Equal<BatchModule.moduleAR>>),
            typeof(Where<Schedule.module, Equal<BatchModule.moduleWZ>>)
	})]
	public partial class Schedule : PX.Data.IBqlTable
	{
        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
        #endregion	
		#region ScheduleID
		public abstract class scheduleID : PX.Data.BQL.BqlString.Field<scheduleID> { }
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXUIField(DisplayName = "Schedule ID", Visibility = PXUIVisibility.SelectorVisible)]
		[AutoNumber(typeof(GLSetup.scheduleNumberingID), typeof(AccessInfo.businessDate))]
		[PXSelector(typeof(Search<Schedule.scheduleID, Where<Schedule.module, Equal<Current<Schedule.module>>>>))]
		[PXFieldDescription]
		[PXDefault]
		public virtual string ScheduleID
		{
			get;
			set;
		}
		#endregion
		#region ScheduleName
		public abstract class scheduleName : PX.Data.BQL.BqlString.Field<scheduleName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual string ScheduleName
		{
			get;
			set;
		}
		#endregion
		#region Active
		public abstract class active : PX.Data.BQL.BqlBool.Field<active> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Active", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(true)]
		public virtual bool? Active
		{
			get;
			set;
		}
		#endregion
		#region ScheduleType
		public abstract class scheduleType : PX.Data.BQL.BqlString.Field<scheduleType> { }
		[PXDBString(1, IsFixed = true)]
		[PXDefault(GLScheduleType.Periodically)]
		[PXUIField(DisplayName = "Schedule Type", Visibility = PXUIVisibility.SelectorVisible, Enabled=false)]
		[GLScheduleType.List]
		public virtual string ScheduleType
		{
			get;
			set;
		}
		#endregion
		#region FormScheduleType
		public abstract class formScheduleType : PX.Data.BQL.BqlString.Field<formScheduleType> { }
		[PXString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Schedule Type", Visibility = PXUIVisibility.Visible)]
		[GLScheduleType.List]
		public virtual string FormScheduleType
		{
			get
			{
				return this.ScheduleType;
			}
			set
			{
				this.ScheduleType = value;
			}
		}
		#endregion
		#region DailyFrequency
		public abstract class dailyFrequency : PX.Data.BQL.BqlShort.Field<dailyFrequency> { }
		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = Messages.Every, Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual short? DailyFrequency
		{
			get;
			set;
		}
		#endregion
		#region WeeklyFrequency
		public abstract class weeklyFrequency : PX.Data.BQL.BqlShort.Field<weeklyFrequency> { }
		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = Messages.Every, Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual short? WeeklyFrequency
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay1
		public abstract class weeklyOnDay1 : PX.Data.BQL.BqlBool.Field<weeklyOnDay1> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Sunday", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		[PXUIVerify(
			typeof(Where<
				Schedule.scheduleType, NotEqual<GLScheduleType.weekly>,
				Or<Schedule.weeklyOnDay1, Equal<True>,
				Or<Schedule.weeklyOnDay2, Equal<True>,
				Or<Schedule.weeklyOnDay3, Equal<True>,
				Or<Schedule.weeklyOnDay4, Equal<True>,
				Or<Schedule.weeklyOnDay5, Equal<True>,
				Or<Schedule.weeklyOnDay6, Equal<True>,
				Or<Schedule.weeklyOnDay7, Equal<True>>>>>>>>>),
			PXErrorLevel.Error,
			Messages.DayOfWeekNotSelected)]
		public virtual bool? WeeklyOnDay1
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay2
		public abstract class weeklyOnDay2 : PX.Data.BQL.BqlBool.Field<weeklyOnDay2> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Monday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay2
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay3
		public abstract class weeklyOnDay3 : PX.Data.BQL.BqlBool.Field<weeklyOnDay3> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Tuesday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay3
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay4
		public abstract class weeklyOnDay4 : PX.Data.BQL.BqlBool.Field<weeklyOnDay4> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Wednesday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay4
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay5
		public abstract class weeklyOnDay5 : PX.Data.BQL.BqlBool.Field<weeklyOnDay5> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Thursday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay5
		{
			get;
			set;
		}
		#endregion
		#region WeeklyOnDay6
		public abstract class weeklyOnDay6 : PX.Data.BQL.BqlBool.Field<weeklyOnDay6> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Friday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay6
		{
			get;
			set;

		}
		#endregion
		#region WeeklyOnDay7
		public abstract class weeklyOnDay7 : PX.Data.BQL.BqlBool.Field<weeklyOnDay7> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Saturday", Visibility = PXUIVisibility.Visible)]
        [PXDefault(false)]
		public virtual bool? WeeklyOnDay7
		{
			get;
			set;
		}
		#endregion
		#region MonthlyFrequency
		public abstract class monthlyFrequency : PX.Data.BQL.BqlShort.Field<monthlyFrequency> { }
		[PXDBShort]
		[PXUIField(DisplayName = Messages.Every, Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		[PXIntList(
			new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 
			new [] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" })]
		public virtual short? MonthlyFrequency
		{
			get;
			set;
		}
		#endregion
		#region MonthlyDaySel
		public abstract class monthlyDaySel : PX.Data.BQL.BqlString.Field<monthlyDaySel> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Day Selection", Visibility = PXUIVisibility.Visible)]
		[GLScheduleMonthlyType.List]
		[PXDefault(GLScheduleMonthlyType.OnDay)]
		public virtual string MonthlyDaySel
		{
			get;
			set;
		}
		#endregion
		#region MonthlyOnDay
		public abstract class monthlyOnDay : PX.Data.BQL.BqlShort.Field<monthlyOnDay> { }
		[PXDBShort]
		[PXUIField(DisplayName = Messages.OnDay, Visibility = PXUIVisibility.Visible)]
		[PXIntList(
			new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, 
			new [] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31" })]
		[PXDefault((short)1)]
		public virtual short? MonthlyOnDay
		{
			get;
			set;
		}
		#endregion
		#region MonthlyOnWeek
		public abstract class monthlyOnWeek : PX.Data.BQL.BqlShort.Field<monthlyOnWeek> { }
		[PXDBShort]
		[PXUIField(DisplayName = Messages.OnThe, Visibility = PXUIVisibility.Visible)]
		[PXIntList(
			new [] { 1, 2, 3, 4, 5 }, 
			new [] { Messages.OnFirstWeekOfMonth,Messages.OnSecondWeekOfMonth, Messages.OnThirdWeekOfMonth,Messages.OnFourthWeekOfMonth,Messages.OnLastWeekOfMonth })]
		[PXDefault((short)1)]
		public virtual short? MonthlyOnWeek
		{
			get;
			set;
		}
		#endregion
		#region MonthlyOnDayOfWeek
		public abstract class monthlyOnDayOfWeek : PX.Data.BQL.BqlShort.Field<monthlyOnDayOfWeek> { }
		[PXDBShort]
		[PXDefault((short)1)]
		[PXUIField(DisplayName = "Day of Week", Visibility = PXUIVisibility.Visible)]
		[PXIntList(
			new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 
			new [] {Messages.MonthlyOnSunday, Messages.MonthlyOnMonday, Messages.MonthlyOnTuesday, Messages.MonthlyOnWednesday, Messages.MonthlyOnThursday, Messages.MonthlyOnFriday, Messages.MonthlyOnSaturday, Messages.MonthlyOnWeekday, Messages.MonthlyOnWeekend })]
		public virtual short? MonthlyOnDayOfWeek
		{
			get;
			set;
		}
		#endregion
		#region PeriodFrequency
		public abstract class periodFrequency : PX.Data.BQL.BqlShort.Field<periodFrequency> { }

		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = Messages.Every, Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual short? PeriodFrequency
		{
			get;
			set;
		}
		#endregion
		#region PeriodDateSel
		public abstract class periodDateSel : PX.Data.BQL.BqlString.Field<periodDateSel> { }
		[PXDBString(1, IsFixed = true)]
		[PXUIField(DisplayName = "Date Based On", Visibility = PXUIVisibility.Visible)]
		[PXDefault(PeriodDateSelOption.PeriodStart)]
		[PeriodDateSelOption.List]
		public virtual string PeriodDateSel
		{
			get;
			set;
		}
		#endregion
		#region PeriodFixedDay
		public abstract class periodFixedDay : PX.Data.BQL.BqlShort.Field<periodFixedDay> { }
		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = "Fixed Day of the Period", Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual short? PeriodFixedDay
		{
			get;
			set;
		}
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "Start Date", Visibility = PXUIVisibility.Visible)]
		[PXDefault(typeof(AccessInfo.businessDate))]

		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region NoEndDate
		public abstract class noEndDate : PX.Data.BQL.BqlBool.Field<noEndDate> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Never Expires", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]

		public virtual bool? NoEndDate
		{
			get;
			set;
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Expiration Date", Visibility = PXUIVisibility.Visible)]
		[PXUIVerify(
			typeof(Where<
				Schedule.noEndDate, Equal<True>,
				Or<Schedule.endDate, IsNotNull>>),
			PXErrorLevel.Error,
			ErrorMessages.FieldIsEmpty, typeof(Schedule.endDate),
			MessageArgumentsAreFieldNames = true)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion
		#region NoRunLimit
		public abstract class noRunLimit : PX.Data.BQL.BqlBool.Field<noRunLimit> { }
		[PXDBBool]
		[PXUIField(DisplayName = "No Limit", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public virtual bool? NoRunLimit
		{
			get;
			set;
		}
		#endregion
		#region RunLimit
		public abstract class runLimit : PX.Data.BQL.BqlShort.Field<runLimit> { }

		[PXDBShort(MinValue = 1)]
		[PXUIField(DisplayName = Messages.ExecutionLimitTimes, Visibility = PXUIVisibility.Visible)]
		[PXDefault((short)1)]
		public virtual short? RunLimit
		{
			get;
			set;
		}
		#endregion
		#region RunCntr
		public abstract class runCntr : PX.Data.BQL.BqlShort.Field<runCntr> { }
		[PXDBShort]
		[PXDefault((short)0)]
		[PXUIField(DisplayName = Messages.ExecutedTimes, Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual short? RunCntr
		{
			get;
			set;
		}
		#endregion
		#region NextRunDate
		public abstract class nextRunDate : PX.Data.BQL.BqlDateTime.Field<nextRunDate> { }
		[PXDBDate]
        [PXRequiredExpr(typeof(Where<Schedule.active, Equal<True>>))]
        [PXDefault]
        [PXUIField(DisplayName = "Next Execution", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? NextRunDate
		{
			get;
			set;
		}
		#endregion
		#region LastRunDate
		public abstract class lastRunDate : PX.Data.BQL.BqlDateTime.Field<lastRunDate> { }
		[PXDBDate]
		[PXUIField(DisplayName = "Last Executed", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual DateTime? LastRunDate
		{
			get;
			set;
		}
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote(DescriptionField = typeof(Schedule.scheduleID))]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected string _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region Module
		public abstract class module : PX.Data.BQL.BqlString.Field<module> { }
		[PXDBString(2, IsFixed = true)]
		[PXDefault(BatchModule.GL)]
		public virtual string Module
		{
			get;
			set;
		}
		#endregion
        #region Days
        public abstract class days : PX.Data.BQL.BqlString.Field<days> { }
        protected string _Days;
        [PXString(IsUnicode = true)]
        [PXUIField]
        public virtual string Days 
			=> PXLocalizer.Localize(Messages.ScheduleDays, typeof(Messages).FullName);
        #endregion
        #region Weeks
        public abstract class weeks : PX.Data.BQL.BqlString.Field<weeks> { }
        protected string _Weeks;
        [PXString(IsUnicode = true)]
        [PXUIField]
        public virtual string Weeks 
			=> PXLocalizer.Localize(Messages.ScheduleWeeks, typeof(Messages).FullName);
        #endregion
        #region Months
        public abstract class months : PX.Data.BQL.BqlString.Field<months> { }
        protected string _Months;
        [PXString(IsUnicode = true)]
        [PXUIField]
        public virtual string Months =>
			PXLocalizer.Localize(Messages.ScheduleMonths, typeof(Messages).FullName);
        #endregion
        #region Periods
        public abstract class periods : PX.Data.BQL.BqlString.Field<periods> { }
        protected string _Periods;
        [PXString(IsUnicode = true)]
        [PXUIField]
        public virtual string Periods => 
			PXLocalizer.Localize(Messages.SchedulePeriods, typeof(Messages).FullName);
        #endregion
	}

	public class GLScheduleType
	{
		public class CustomListAttribute : PXStringListAttribute
		{
			public string[] AllowedValues => _AllowedValues;

			public string[] AllowedLabels => _AllowedLabels;

			public CustomListAttribute(string[] allowedValues, string[] allowedLabels)
				: base(allowedValues, allowedLabels)
			{ }
		}

		public class ListAttribute : CustomListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { Daily, Weekly, Monthly, Periodically },
				new string[] { Messages.Daily, Messages.Weekly, Messages.Monthly, Messages.Periodically }) { }

		}

		public const string Daily = "D";
		public const string Weekly = "W";
		public const string Monthly = "M";
		public const string Periodically = "P";

		public class daily : PX.Data.BQL.BqlString.Constant<daily>
		{
			public daily() : base(Daily) { }
		}

		public class weekly : PX.Data.BQL.BqlString.Constant<weekly>
		{
			public weekly() : base(Weekly) { }
		}

		public class monthly : PX.Data.BQL.BqlString.Constant<monthly>
		{
			public monthly() : base(Monthly) { }
		}

		public class periodically : PX.Data.BQL.BqlString.Constant<periodically>
		{
			public periodically() : base(Periodically) { }
		}
	}

	public class GLScheduleMonthlyType
	{
		public const string OnDay = "D";
		public const string OnDayOfWeek = "W";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new [] { OnDay, OnDayOfWeek },
				new [] { Messages.OnDay, Messages.OnThe })
			{ }
		}
	}

	public static class PeriodDateSelOption 
	{
		public const string PeriodStart = "S";
		public const string PeriodEnd = "E";
		public const string PeriodFixedDate = "D";

		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute()
				: base(
				new string[] { PeriodStart, PeriodEnd, PeriodFixedDate},
				new string[] { Messages.PeriodStartDate, Messages.PeriodEndDate, Messages.PeriodFixedDate}) { }
		}

		public class periodStart : PX.Data.BQL.BqlString.Constant<periodStart>
		{
			public periodStart() : base(PeriodStart) { }
		}

		public class periodEnd : PX.Data.BQL.BqlString.Constant<periodEnd>
		{
			public periodEnd() : base(PeriodEnd) { }
		}

		public class periodFixedDate : PX.Data.BQL.BqlString.Constant<periodFixedDate>
		{
			public periodFixedDate() : base(PeriodFixedDate) { }
		}
	} 

}
