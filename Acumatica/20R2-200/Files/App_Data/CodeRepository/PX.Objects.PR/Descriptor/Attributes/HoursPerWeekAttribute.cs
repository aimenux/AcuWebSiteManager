using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[PXDecimal]
	[PXUIField(DisplayName = "Working Hours per Week", Enabled = false)]
	public class HoursPerWeekAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber
	{
		private readonly Type _CalendarIDField;
		private readonly Type _CalendarIDUseDefaultField;
		private readonly Type _HoursPerYearField;
		private readonly Type _WeeksPerYearField;

		public HoursPerWeekAttribute(Type calendarID, Type hoursPerYear, Type weeksPerYear, Type calendarIDUseDefault = null)
		{
			_CalendarIDField = calendarID;
			_CalendarIDUseDefaultField = calendarIDUseDefault;
			_HoursPerYearField = hoursPerYear;
			_WeeksPerYearField = weeksPerYear;
		}

		public override void CacheAttached(PXCache sender)
		{
			base.CacheAttached(sender);

			if (_CalendarIDUseDefaultField != null)
			{
				sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CalendarIDUseDefaultField.Name, (cache, e) =>
				{
					object retVal = null;
					cache.RaiseFieldSelecting(_FieldName, e.Row, ref retVal, false);
				});
			}

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _CalendarIDField.Name, (cache, e) =>
			{
				object retVal = null;
				cache.RaiseFieldSelecting(_FieldName, e.Row, ref retVal, false);
			});

			sender.Graph.FieldUpdated.AddHandler(sender.GetItemType(), _WeeksPerYearField.Name, (cache, e) =>
			{
				object retVal = null;
				cache.RaiseFieldSelecting(_HoursPerYearField.Name, e.Row, ref retVal, false);
			});

			sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(), _HoursPerYearField.Name, (cache, e) =>
			{
				if (e.Row == null)
					return;

				decimal hoursPerWeek = GetWorkingWeekHours(cache, e.Row);
				e.ReturnValue = GetHoursPerYear(cache, e.Row, hoursPerWeek);
			});
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			if (e.Row == null)
				return;

			decimal hoursPerWeek = GetWorkingWeekHours(sender, e.Row);
			e.ReturnValue = hoursPerWeek;
			sender.SetValue(e.Row, _HoursPerYearField.Name, GetHoursPerYear(sender, e.Row, hoursPerWeek));
		}

		private decimal GetWorkingWeekHours(PXCache sender, object row)
		{
			string calendarID = (string)sender.GetValue(row, _CalendarIDField.Name);
			CSCalendar calendar = SelectFrom<CSCalendar>.
				Where<CSCalendar.calendarID.IsEqual<P.AsString>>.View.Select(sender.Graph, calendarID);

			decimal workingWeekHours = 0.0m;

			if (calendar == null)
				return workingWeekHours;

			for (int weekDay = 0; weekDay < 7; weekDay++)
				workingWeekHours += CalendarHelper.GetHoursWorkedOnDay(calendar, (DayOfWeek)weekDay);

			return workingWeekHours;
		}

		private decimal? GetHoursPerYear(PXCache sender, object row, decimal hoursPerWeek)
		{
			byte? weeksPerYear = null;
			object weeksPerYearRaw = sender.GetValueExt(row, _WeeksPerYearField.Name);
			if (weeksPerYearRaw is PXFieldState)
			{
				weeksPerYear = (weeksPerYearRaw as PXFieldState).Value as byte?;
			}
			else
			{
				weeksPerYear = weeksPerYearRaw as byte?;
			}

			return weeksPerYear * hoursPerWeek;
		}
	}
}
