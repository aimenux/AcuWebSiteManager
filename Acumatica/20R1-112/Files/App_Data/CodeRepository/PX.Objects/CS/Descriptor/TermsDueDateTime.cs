using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CS
{
	public static class TermsDueDateTime
	{
		public static DateTime GetEndOfMonth(this DateTime date) => new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
		public static DateTime SetDayInMonthAfter(this DateTime date, int dayNumber)
		{
			var nextDate = date.AddMonths(1);
			return new DateTime(nextDate.Year, nextDate.Month, dayNumber);
		}
	}
}
