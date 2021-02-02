using System;
using PX.Data;
using PX.Objects.CR;

namespace PX.Objects.CN.Common.Helpers
{
	public static class DateTimeHelper
	{
		public static DateTime CalculateBusinessDate(DateTime originalBusinessDate, int timeSpan,
			string calendarId)
		{
			var businessDaysDifference = GetBusinessDaysDifference(originalBusinessDate, timeSpan, calendarId);
			return originalBusinessDate.AddDays(businessDaysDifference);
		}

		private static int GetBusinessDaysDifference(DateTime originalBusinessDate, int timeFrame,
			string calendarId)
		{
			var daysDifference = 1;
			var graph = PXGraph.CreateInstance<PXGraph>();
			while (true)
			{
				var newBusinessDate = originalBusinessDate.AddDays(daysDifference);
				if (CalendarHelper.IsWorkDay(graph, calendarId, newBusinessDate) &&
				    --timeFrame < 1)
				{
					break;
				}
				daysDifference++;
			}
			return daysDifference;
		}
	}
}
