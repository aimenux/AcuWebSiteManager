using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN.Matrix.Utility;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
	public class ApplyToMatrixItemsExt : PXGraphExtension<ItemsGridExt, Graphs.TemplateInventoryItemMaint>
	{
		public PXAction<InventoryItem> applyToItems;
		[PXUIField(DisplayName = "Apply to Matrix Items", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update, Enabled = true)]
		[PXProcessButton]
		public virtual IEnumerable ApplyToItems(PXAdapter adapter)
		{
			Base.Save.Press();

			InventoryItem templateItem = Base.Item.Current;
			var childrenItems = Base1.MatrixItems.SelectMain();

			PXLongOperation.StartOperation(Base, delegate ()
			{
				var graph = (templateItem.StkItem == true)
					? (InventoryItemMaintBase)PXGraph.CreateInstance<InventoryItemMaint>()
					: PXGraph.CreateInstance<NonStockItemMaint>();
				var helper = this.GetHelper(graph);

				helper.CreateUpdateMatrixItems(graph, templateItem, childrenItems, false);
			});

			return adapter.Get();
		}

		protected virtual void _(Events.RowSelected<InventoryItem> e)
		{
			applyToItems.SetEnabled(e.Cache.GetStatus(e.Row).IsIn(PXEntryStatus.Notchanged, PXEntryStatus.Updated));
		}

		protected virtual CreateMatrixItemsHelper GetHelper(PXGraph graph)
		{
			return new CreateMatrixItemsHelper(graph);
		}
	}
}
