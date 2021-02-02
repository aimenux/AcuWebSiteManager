using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN.Matrix.DAC.Accumulators;

namespace PX.Objects.IN.GraphExtensions.InventoryItemMaintBaseExt
{
	public class TemplateItemLastModifiedUpdateExt : PXGraphExtension<InventoryItemMaintBase>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.matrixItem>();
		}

		public PXSelect<TemplateItemLastModifiedUpdate> TemplateItemLastModifiedUpdate;

		protected virtual void _(Events.RowPersisting<InventoryItem> eventArgs)
		{
			if (eventArgs.Row?.TemplateItemID != null &&
				eventArgs.Cache.GetStatus(eventArgs.Row).IsIn(PXEntryStatus.Inserted, PXEntryStatus.Deleted))
			{
				TemplateItemLastModifiedUpdate.Insert(new TemplateItemLastModifiedUpdate()
				{
					InventoryID = eventArgs.Row.TemplateItemID
				});
			}
		}
	}
}
