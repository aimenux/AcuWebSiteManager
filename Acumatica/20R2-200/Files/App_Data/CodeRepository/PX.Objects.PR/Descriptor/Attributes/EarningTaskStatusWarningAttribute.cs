using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PR
{
	public class EarningTaskStatusWarningAttribute : PXEventSubscriberAttribute, IPXFieldVerifyingSubscriber
	{
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			var row = (PREarningDetail)e.Row;
			PMTask task = PXSelect<PMTask>.Search<PMTask.projectID, PMTask.taskID>(sender.Graph, row.ProjectID, e.NewValue);
			if (task != null && task.Status != ProjectTaskStatus.Active)
			{
				var listAttribute = new ProjectTaskStatus.ListAttribute();
				string status = listAttribute.ValueLabelDic[task.Status];

				sender.RaiseExceptionHandling<PREarningDetail.projectTaskID>(e.Row, e.NewValue,
					new PXSetPropertyException(Messages.TaskStatusWarning, PXErrorLevel.Warning, status));
			}
		}
	}
}
