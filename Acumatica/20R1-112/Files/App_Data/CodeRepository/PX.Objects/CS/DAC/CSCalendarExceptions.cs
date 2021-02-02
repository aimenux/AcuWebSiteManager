namespace PX.Objects.CS
{
	using System;
	using PX.Data;
	
	[System.SerializableAttribute()]
	[PXCacheName(Messages.CalendarException)]
	public partial class CSCalendarExceptions : PX.Data.IBqlTable
	{
		#region CalendarID
		public abstract class calendarID : PX.Data.BQL.BqlString.Field<calendarID> { }
		protected String _CalendarID;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Calendar ID")]
		public virtual String CalendarID
		{
			get
			{
				return this._CalendarID;
			}
			set
			{
				this._CalendarID = value;
			}
		}
		#endregion
		#region YearID
		public abstract class yearID : PX.Data.BQL.BqlInt.Field<yearID> { }
		protected Int32? _YearID;
		[PXDBInt()]
		[PXDefault(2008)]
		[PXUIField(DisplayName = "Year", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<CSCalendarExceptions.yearID>))]
		public virtual Int32? YearID
		{
			get
			{
				return this._YearID;
			}
			set
			{
				this._YearID = value;
			}
		}
		#endregion
		#region Date
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		protected DateTime? _Date;
		[PXDBDate(IsKey = true)]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? Date
		{
			get
			{
				return this._Date;
			}
			set
			{
				this._Date = value;
			}
		}
		#endregion
		#region DayOfWeek
		public abstract class dayOfWeek : PX.Data.BQL.BqlInt.Field<dayOfWeek> { }
		protected Int32? _DayOfWeek;
		[PXDBInt()]
		[PXDefault(1)]
		[PXUIField(DisplayName = "Day Of Week", Enabled = false)]
		[PXIntList(new int[] { 1, 2, 3, 4, 5, 6, 7}, new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"})]
		public virtual Int32? DayOfWeek
		{
			get
			{
				return this._DayOfWeek;
			}
			set
			{
				this._DayOfWeek = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region WorkDay
		public abstract class workDay : PX.Data.BQL.BqlBool.Field<workDay> { }
		protected Boolean? _WorkDay;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Work Day")]
		public virtual Boolean? WorkDay
		{
			get
			{
				return this._WorkDay;
			}
			set
			{
				this._WorkDay = value;
			}
		}
		#endregion
		#region StartTime
		public abstract class startTime : PX.Data.BQL.BqlDateTime.Field<startTime> { }
		protected DateTime? _StartTime;
		[PXDBTime(DisplayMask = "t", UseTimeZone = false)]
		[PXDefault(TypeCode.DateTime, "01/01/2008 09:00:00")]
		[PXUIField(DisplayName = "Start Time")]
		public virtual DateTime? StartTime
		{
			get
			{
				return this._StartTime;
			}
			set
			{
				this._StartTime = value;
			}
		}
		#endregion
		#region EndTime
		public abstract class endTime : PX.Data.BQL.BqlDateTime.Field<endTime> { }
		protected DateTime? _EndTime;
		[PXDBTime(DisplayMask = "t", UseTimeZone = false)]
		[PXDefault(TypeCode.DateTime, "01/01/2008 18:00:00")]
		[PXUIField(DisplayName = "End Time")]
		public virtual DateTime? EndTime
		{
			get
			{
				return this._EndTime;
			}
			set
			{
				this._EndTime = value;
			}
		}
		#endregion
		#region GoodsMoved
		public abstract class goodsMoved : PX.Data.BQL.BqlBool.Field<goodsMoved> { }
		protected Boolean? _GoodsMoved;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Goods Are Moved")]
		public virtual Boolean? GoodsMoved
		{
			get
			{
				return this._GoodsMoved;
			}
			set
			{
				this._GoodsMoved = value;
			}
		}
		#endregion
		#region UnpaidTime
		[PXDBTimeSpanLong(Format = TimeSpanFormatType.ShortHoursMinutesCompact)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Unpaid Break Time", FieldClass = nameof(FeaturesSet.PayrollModule))]
		public virtual int? UnpaidTime { get; set; }
		public abstract class unpaidTime : Data.BQL.BqlInt.Field<unpaidTime> { }
		#endregion
	}
}
