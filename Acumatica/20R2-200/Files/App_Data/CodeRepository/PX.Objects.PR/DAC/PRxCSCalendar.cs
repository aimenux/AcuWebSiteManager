using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	public sealed class PRxCSCalendar : PXCacheExtension<CSCalendar>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		public abstract class weekWorkTime : Data.BQL.BqlDecimal.Field<weekWorkTime> { }
		[PXDecimal]
		public decimal? WeekWorkTime
		{
			[PXDependsOnFields(
				typeof(CSCalendar.sunWorkDay), typeof(CSCalendar.sunStartTime), typeof(CSCalendar.sunEndTime), typeof(CSCalendar.sunUnpaidTime),
				typeof(CSCalendar.monWorkDay), typeof(CSCalendar.monStartTime), typeof(CSCalendar.monEndTime), typeof(CSCalendar.monUnpaidTime),
				typeof(CSCalendar.tueWorkDay), typeof(CSCalendar.tueStartTime), typeof(CSCalendar.tueEndTime), typeof(CSCalendar.tueUnpaidTime),
				typeof(CSCalendar.wedWorkDay), typeof(CSCalendar.wedStartTime), typeof(CSCalendar.wedEndTime), typeof(CSCalendar.wedUnpaidTime),
				typeof(CSCalendar.thuWorkDay), typeof(CSCalendar.thuStartTime), typeof(CSCalendar.thuEndTime), typeof(CSCalendar.thuUnpaidTime),
				typeof(CSCalendar.friWorkDay), typeof(CSCalendar.friStartTime), typeof(CSCalendar.friEndTime), typeof(CSCalendar.friUnpaidTime),
				typeof(CSCalendar.satWorkDay), typeof(CSCalendar.satStartTime), typeof(CSCalendar.satEndTime), typeof(CSCalendar.satUnpaidTime))]
			get
			{
				decimal workingWeekHours = 0.0m;

				for (int weekDay = 0; weekDay < 7; weekDay++)
					workingWeekHours += CalendarHelper.GetHoursWorkedOnDay(Base, (DayOfWeek)weekDay);

				return workingWeekHours;
			}
		}
	}
}
