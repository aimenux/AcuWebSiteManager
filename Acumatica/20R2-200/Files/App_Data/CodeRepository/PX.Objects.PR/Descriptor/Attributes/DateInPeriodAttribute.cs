using System;
using PX.Common;
using PX.Data;

namespace PX.Objects.PR
{
	public class DateInPeriodAttribute : PXDBDateAttribute, IPXFieldVerifyingSubscriber, IPXFieldDefaultingSubscriber
	{
		private readonly Type _Parent;
		private readonly Type _PeriodStartDate;
		private readonly Type _PeriodEndDate;
		private readonly string _ExistingRowsViewName;
		
		public DateInPeriodAttribute(Type parent, Type periodStartDate, Type periodEndDate, string existingRowsViewName)
		{
			_Parent = parent;
			_PeriodStartDate = periodStartDate;
			_PeriodEndDate = periodEndDate;
			_ExistingRowsViewName = existingRowsViewName;
		}

		public override void CacheAttached(PXCache sender)
		{
			SetMinAndMaxDate(sender);

			if (sender.Graph.Caches.TryGetValue(_Parent, out PXCache parentCache))
			{
				parentCache.Graph.FieldUpdated.AddHandler(parentCache.GetItemType(), _PeriodStartDate.Name, (cache, e) => SetMinAndMaxDate(cache));
				parentCache.Graph.FieldUpdated.AddHandler(parentCache.GetItemType(), _PeriodEndDate.Name, (cache, e) => SetMinAndMaxDate(cache));
			}

			base.CacheAttached(sender);
		}

		private void SetMinAndMaxDate(PXCache sender)
		{
			if (!sender.Graph.Caches.TryGetValue(_Parent, out PXCache parentCache) || parentCache?.Current == null)
				return;

			DateTime? startDate = parentCache.GetValue(parentCache.Current, _PeriodStartDate.Name) as DateTime?;
			DateTime? endDate = parentCache.GetValue(parentCache.Current, _PeriodEndDate.Name) as DateTime?;

			if (startDate != null && endDate != null)
			{
				_MinValue = startDate.Value;
				_MaxValue = endDate.Value;
			}
		}
		
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (e.Row == null)
				return;

			DateTime minDate = _MinValue.GetValueOrDefault();
			DateTime maxDate = _MaxValue.GetValueOrDefault();
			DateTime? newDate = e.NewValue as DateTime?;
			if (newDate == null || newDate < minDate || newDate > maxDate)
			{
				e.NewValue = null;
				string exceptionMessage = GetIncorrectDateMessage(minDate, maxDate);
				PXUIFieldAttribute.SetError(sender, e.Row, _FieldName, exceptionMessage);
			}
		}

		public void FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
		{
			if (e.Row == null || _MinValue == null)
				return;
			
			if (!sender.Graph.Views.TryGetValue(_ExistingRowsViewName, out PXView existingRowsView) || existingRowsView.SelectSingle() != null)
				return;

			e.NewValue = _MinValue.Value;
		}

		public static string GetIncorrectDateMessage(DateTime minDate, DateTime maxDate)
		{
			return string.Format(Messages.IncorrectPeriodDate, GetShortDateString(minDate), GetShortDateString(maxDate));
		}

		private static string GetShortDateString(DateTime date)
		{
			try
			{
				return date.ToString(LocaleInfo.GetCulture().DateTimeFormat.ShortDatePattern);
			}
			catch
			{
				return date.ToShortDateString();
			}
		}
	}
}
