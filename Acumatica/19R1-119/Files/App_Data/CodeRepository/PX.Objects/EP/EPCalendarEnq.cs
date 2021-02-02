using System;
using System.Collections;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.EP;
using Attendee = PX.Objects.EP.EPAttendee;
using PX.SM;

namespace PX.TM
{
	#region EPCalendarFilter
	[Serializable]
	[PXHidden]
	public partial class EPCalendarFilter : IBqlTable
	{
		public const int DASHBOARD_TYPE = 7;

		#region CalendarTypeAttribute
		public class CalendarTypeAttribute : PXStringListAttribute
		{
			private static string[] weekDayNames =
				new string[] { Messages.Sunday, Messages.Monday, Messages.Tuesday, Messages.Wednesday, 
				Messages.Thursday, Messages.Friday, Messages.Saturday };
			public const string DAY = "Day";
			public const string WEEK = "Week";
			public const string MONTH = "Month";

			public static string[] WeekDayNames
			{
				get
				{
					string[] res = new string[weekDayNames.Length];
					for (int i = 0; i < weekDayNames.Length; i++)
						res[i] = PXMessages.LocalizeNoPrefix(weekDayNames[i]);
					return res;
				}
			}

			public static string GetDayName(DayOfWeek dayOfWeek)
			{
				return WeekDayNames[GetDayIndexInWeek(dayOfWeek)];
			}

			public static int GetDayIndexInWeek(DayOfWeek dayOfWeek)
			{
				switch (dayOfWeek)
				{
					case DayOfWeek.Monday: return 1;
					case DayOfWeek.Tuesday: return 2;
					case DayOfWeek.Wednesday: return 3;
					case DayOfWeek.Thursday: return 4;
					case DayOfWeek.Friday: return 5;
					case DayOfWeek.Saturday: return 6;
					default: return 0;
				}
			}

			public CalendarTypeAttribute()
				: base(
				new string[] { DAY, WEEK, MONTH },
				new string[] { Messages.Day, Messages.Week, Messages.Month }) { }

			public class Month : PX.Data.BQL.BqlString.Constant<Month>
			{
				public Month() : base(MONTH) { }
			}
			public class Day : PX.Data.BQL.BqlString.Constant<Day>
			{
				public Day() : base(DAY) { }
			}
			public class Week : PX.Data.BQL.BqlString.Constant<Week>
			{
				public Week() : base(WEEK) { }
			}
		}
		#endregion

		#region Fields
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected String _Type;
		[PXDefault(EPCalendarFilter.CalendarTypeAttribute.WEEK)]
		[PXDBString]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.Visible)]
		[CalendarType]
		public virtual String Type
		{
			get
			{
				return _Type ?? EPCalendarFilter.CalendarTypeAttribute.WEEK;
			}
			set
			{
				_Type = value;
			}
		}
		#endregion
		#region CentralDate
		public abstract class centralDate : PX.Data.BQL.BqlDateTime.Field<centralDate> { }
		protected DateTime? _CentralDate;
		[PXDBDate(PreserveTime = true)]
		[EPNowDefault]
		public virtual DateTime? CentralDate
		{
			get
			{
				return _CentralDate ?? (DateTime?)DateTime.Today;
			}
			set
			{
				_CentralDate = value;
			}
		}

		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate(PreserveTime = true)]
		public virtual DateTime? StartDate
		{
			get
			{
				return GetStartDate(Type, CentralDate.Value);
			}
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate(PreserveTime = true)]
		public virtual DateTime? EndDate
		{
			get
			{
				return GetEndDate(Type, CentralDate.Value);
			}
		}
		#endregion
		#region DisplayPeriod
		public abstract class displayPeriod : PX.Data.BQL.BqlString.Field<displayPeriod> { }
		[PXDBString]
		public virtual String DisplayPeriod
		{
			get
			{
				string format = "{0:dd MMMM yyyy}";
				switch (Type)
				{
					case CalendarTypeAttribute.WEEK:
						format = "{0:dd MMMM yyyy} - {1:dd MMMM yyyy}";
						break;
					case CalendarTypeAttribute.MONTH:
						format = "{2:MMMM yyyy}";
						break;
				}
				return string.Format(format, StartDate, EndDate, CentralDate);
			}
		}
		#endregion
		#endregion

		#region Methods

		public static DateTime GetStartDate(string periodType, DateTime centralDate)
		{
			DateTime date = centralDate.Date;
			switch (periodType)
			{
				case CalendarTypeAttribute.WEEK:
					return date.AddDays(-CalendarTypeAttribute.GetDayIndexInWeek(date.DayOfWeek));
				case CalendarTypeAttribute.MONTH:
					return GetStartDate(CalendarTypeAttribute.WEEK, date.AddDays(1 - date.Day));
				default: return date;
			}
		}

		public static DateTime GetEndDate(string periodType, DateTime centralDate)
		{
			DateTime date = centralDate.Date;
			switch (periodType)
			{
				case CalendarTypeAttribute.WEEK:
					return GetStartDate(periodType, date).AddDays(7D);
				case CalendarTypeAttribute.MONTH:
					return GetStartDate(periodType, date).AddDays(35D);
				default: return date.AddDays(1D);
			}
		}
		#endregion
	}
	#endregion

	[DashboardType(EPCalendarFilter.DASHBOARD_TYPE)]
	public class EPCalendarEnq : PXGraph<EPCalendarEnq>
	{
		#region Constants

		private const int _30MINUTES = 30;

		#endregion

		#region Selects

		public PXFilter<EPCalendarFilter> 
			Filter;

		[PXViewDetailsButton(typeof(EPCalendarFilter))]
		public PXSelectJoin<CRActivity,
			LeftJoin<Attendee, 
				On<Attendee.userID, Equal<Current<AccessInfo.userID>>,
				And<Attendee.eventNoteID, Equal<CRActivity.noteID>>>>,
			Where2<Where<CRActivity.classID, Equal<CRActivityClass.events>>,
				And<Where<CRActivity.startDate, GreaterEqual<Current<EPCalendarFilter.startDate>>,
				And<CRActivity.startDate, LessEqual<Current<EPCalendarFilter.endDate>>,
				And<Where<CRActivity.createdByID, Equal<Current<AccessInfo.userID>>, 
					Or<Attendee.userID, IsNotNull>>>>>>>,
			OrderBy<
				Asc<CRActivity.startDate>>> 
			Events;

		#endregion

		#region Buttons
		public PXAction<EPCalendarFilter> NextList;
		[PXUIField(DisplayName = Messages.NextList)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable nextList(PXAdapter adapter)
		{
			MoveDateList(Filter.Current, true);
			return adapter.Get();
		}

		public PXAction<EPCalendarFilter> PreviousList;
		[PXUIField(DisplayName = Messages.PreviousList)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable previousList(PXAdapter adapter)
		{
			MoveDateList(Filter.Current, false);
			return adapter.Get();
		}

		public PXAction<EPCalendarFilter> ToDayList;
		[PXUIField(DisplayName = Messages.ToDayList)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable toDayList(PXAdapter adapter)
		{
			Filter.Current.CentralDate = DateTime.Today;
			return adapter.Get();
		}

		public PXAction<EPCalendarFilter> createNew;
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public virtual IEnumerable CreateNew(PXAdapter adapter)
		{
			var eventMain = PXGraph.CreateInstance<PX.Objects.EP.EPEventMaint>();
			var newevent = (CRActivity)eventMain.Events.Cache.Insert();
			newevent.StartDate = Filter.Current.CentralDate;
			if (newevent.StartDate.HasValue) newevent.EndDate = newevent.StartDate.Value.AddMinutes(_30MINUTES);
			throw new PXPopupRedirectException(eventMain, string.Empty, true);
		}
		#endregion

		#region Private Methods

		private static void MoveDateList(EPCalendarFilter filter, bool forward)
		{
			switch (filter.Type)
			{
				case EPCalendarFilter.CalendarTypeAttribute.DAY:
					filter.CentralDate = AddDays(filter.CentralDate, 1, forward);
					break;
				case EPCalendarFilter.CalendarTypeAttribute.WEEK:
					filter.CentralDate = AddDays(filter.CentralDate, 7, forward);
					break;
				case EPCalendarFilter.CalendarTypeAttribute.MONTH:
					DateTime centralDate = filter.CentralDate.Value;
					DateTime nextMonth = centralDate.AddMonths(1);
					int daysStep = DateTime.DaysInMonth(centralDate.Year, centralDate.Month) - centralDate.Day +
						DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month) / 2;
					filter.CentralDate = AddDays(filter.CentralDate, daysStep, forward);
					break;
			}
		}

		private static DateTime? AddDays(DateTime? date, int diff, bool forward)
		{
			if (date.HasValue) return date.Value.AddDays((forward ? 1 : -1) * diff);
			return date;
		}
		#endregion
	}
}
