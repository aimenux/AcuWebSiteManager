using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	class PRPayGroupPeriodEndDateUIAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber, IPXFieldUpdatingSubscriber
	{
		private Type _StartDateField;
		private Type _EndDateField;

		public PRPayGroupPeriodEndDateUIAttribute(Type startDateField, Type endDateField)
		{
			_StartDateField = startDateField;
			_EndDateField = endDateField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			DateTime? startDate = sender.GetValue(e.Row, _StartDateField.Name) as DateTime?;
			DateTime? endDate = sender.GetValue(e.Row, _EndDateField.Name) as DateTime?;

			bool isEmpty = endDate.HasValue && startDate.HasValue && startDate == endDate;
			e.ReturnValue = endDate.HasValue && !isEmpty ?
				endDate.Value.AddDays(-1) : endDate;
		}

		public void FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
				DateTime? startDate = sender.GetValue(e.Row, _StartDateField.Name) as DateTime?;
				DateTime? newValue = e.NewValue as DateTime?;
				object oldValue = sender.GetValue(e.Row, _FieldName);

			if (newValue != null || oldValue != null)
			{
				bool isEmpty = newValue.HasValue && startDate.HasValue && startDate == newValue;
				DateTime? endDate = newValue.HasValue && !isEmpty ?
					newValue.Value.AddDays(1) : newValue;

				sender.SetValue(e.Row, _EndDateField.Name, endDate); 
			}
		}
	}
}
