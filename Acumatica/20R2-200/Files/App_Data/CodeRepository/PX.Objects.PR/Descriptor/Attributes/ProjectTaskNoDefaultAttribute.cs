using PX.Data;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	public class ProjectTaskNoDefaultAttribute : ProjectTaskAttribute
	{
		public ProjectTaskNoDefaultAttribute(Type projectField) : base(projectField)
		{
			AllowNull = true;
		}

		protected override void OnProjectUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			object taskCD = (sender.GetValuePending(e.Row, _FieldName) as string) ?? (sender.GetValueExt(e.Row, _FieldName) as PXSegmentedState).Value;
			if (taskCD != null && taskCD != PXCache.NotSetValue)
			{
				base.OnProjectUpdated(sender, e);
			}
		}
	}
}
