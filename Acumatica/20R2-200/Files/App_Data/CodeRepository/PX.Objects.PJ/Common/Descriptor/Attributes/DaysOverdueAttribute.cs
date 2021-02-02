using PX.Data;
using System;

namespace PX.Objects.PJ.Common.Descriptor.Attributes
{
	public class DaysOverdueAttribute : PXEventSubscriberAttribute, IPXFieldSelectingSubscriber
	{
		private readonly Type SourceDateField;
		private readonly Type FinalDateField;

		public DaysOverdueAttribute(Type sourceDateField, Type finalDateField)
		{
			SourceDateField = sourceDateField;
			FinalDateField = finalDateField;
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			DateTime? sourceDate = (DateTime?)sender.GetValue(e.Row, SourceDateField.Name);
			if (sourceDate != null)
			{
				DateTime? closedDate = (DateTime?)sender.GetValue(e.Row, FinalDateField.Name);

				DateTime oppositeDate = closedDate ?? sender.Graph.Accessinfo.BusinessDate.Value;

				int deltaDays = oppositeDate.Subtract(sourceDate.Value).Days;

				e.ReturnValue = deltaDays > 0 ? (int?)deltaDays : null;
			}
		}
	}
}
