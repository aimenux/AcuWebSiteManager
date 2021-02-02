using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	public class EarningTypeProjectTaskDefaultAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
	{
		private Type _ProjectField;
		private Type _ProjectTaskField;

		public EarningTypeProjectTaskDefaultAttribute(Type projectField, Type projectTaskField)
		{
			_ProjectField = projectField;
			_ProjectTaskField = projectTaskField;
		}

		public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			EPEarningType earningType = PXSelectorAttribute.Select(sender, e.Row, _FieldName) as EPEarningType;
			if (earningType?.ProjectID != null)
			{
				sender.SetValue(e.Row, _ProjectField.Name, earningType.ProjectID);
				sender.SetValue(e.Row, _ProjectTaskField.Name, earningType.TaskID);
			}
		}
	}
}
