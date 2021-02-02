using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.CR
{
	public class CRActivitySetupMaint : PXGraph<CRActivitySetupMaint>
	{
		#region Selects
		[PXViewName(EP.Messages.ActivityTypes)]
		public PXSelect<EPActivityType> ActivityTypes;

		public PXSelect<EPActivityType, 
			Where<EPActivityType.isDefault, Equal<True>, 
				And<EPActivityType.application, Equal<Required<EPActivityType.application>>>>> 
			DefaultActivityTypes;
		#endregion

		#region Action

		public PXSave<EPActivityType> Save;
		public PXCancel<EPActivityType> Cancel;
		public PXSelect<EPSetup, Where<EPSetup.defaultActivityType, Equal<Current<EPActivityType.type>>>> epsetup;
		public PXSelect<CRActivity, Where<CRActivity.type, Equal<Current<EPActivityType.type>>>> Activities;
		#endregion

		protected virtual void EPActivityType_IsDefault_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as EPActivityType;
			if (row == null)
				return;

			if (row.IsDefault == true)
			{
				var defTypes = DefaultActivityTypes.Select(row.Application);
				foreach(EPActivityType def in defTypes)
				{
					if (def.Type == row.Type)
						continue;
					def.IsDefault = false;
					sender.Update(def);
				}
				ActivityTypes.View.RequestRefresh();
			}
		}

		protected virtual void EPActivityType_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			EPActivityType row = e.Row as EPActivityType;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled(sender, row, !(row.IsSystem ?? false));
			}
			PXUIFieldAttribute.SetEnabled<EPActivityType.classID>(ActivityTypes.Cache, null, false);
		}

		protected virtual void EPActivityType_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			ValidateIsSystem(e.NewRow);
			if (!sender.ObjectsEqual<EPActivityType.type>(e.Row, e.NewRow))
			{
				ValidateUsage(e.Row, epsetup, Messages.ActivityTypeUsageChanged);
				ValidateUsage(e.Row, Activities, Messages.ActivityTypeUsageChanged);
			}
		}
		protected virtual void EPActivityType_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			ValidateIsSystem(e.Row);
			ValidateUsage(e.Row, epsetup, Messages.ActivityTypeUsage);
			ValidateUsage(e.Row, Activities, Messages.ActivityTypeUsage);			
		}

		private void ValidateUsage(object row, PXSelectBase select, string message)
		{
			if (select.View.SelectSingleBound(new object[]{row}) != null)
				throw new PXException(message);
		}

		private void ValidateIsSystem(object row)
		{
			if (row != null && (row is EPActivityType))
			{
				var activityType = (row as EPActivityType);
				if (activityType.IsSystem ?? false)
				{
					throw new PXException(Messages.SystemActivityType);
				}
			}
		}
	}
}