using System;
using System.Activities.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.IN;

namespace PX.Objects.PO.LandedCosts.Attributes
{
	public class LandedCostDetailInventoryAttribute : PXDimensionSelectorAttribute
	{
		public LandedCostDetailInventoryAttribute()
			: base(AnyInventoryAttribute.DimensionName, 
				typeof(Search<InventoryItem.inventoryID>), 
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
		{
			this.DescriptionField = typeof(InventoryItem.descr);
			_Attributes[_Attributes.Count - 1] = new CustomSelector();
		}

		public class CustomSelector : PXCustomSelectorAttribute
		{

			public CustomSelector() : base(typeof(Search<InventoryItem.inventoryID>),
				typeof(InventoryItem.inventoryCD),
				typeof(InventoryItem.descr))
			{
				this.SubstituteKey = typeof(InventoryItem.inventoryCD);
				this.DescriptionField = typeof(InventoryItem.descr);
			}

			public override void CacheAttached(PXCache sender)
			{
				base.CacheAttached(sender);
				var fn = this._FieldName;
				//PXSelectorAttribute.SetColumns(sender, _FieldName, selFields, selHeaders);
			}

			protected virtual IEnumerable GetRecords()
			{
				var details = PXSelect<POLandedCostReceiptLine,
						Where<POLandedCostReceiptLine.docType, Equal<Current<POLandedCostDoc.docType>>,
							And<POLandedCostReceiptLine.refNbr, Equal<Current<POLandedCostDoc.refNbr>>>>>
					.Select(_Graph).RowCast<POLandedCostReceiptLine>();

				var inventoryIds = details.Select(t => t.InventoryID).Distinct().ToArray();

				if (!inventoryIds.Any())
					return new List<InventoryItem>();

				var inventoryItems = PXSelectReadonly<InventoryItem,
						Where<InventoryItem.inventoryID, In<Required<InventoryItem.inventoryID>>>>
					.Select(_Graph, inventoryIds).RowCast<InventoryItem>();

				return inventoryItems;
			}

			public override void SubstituteKeyFieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
			{
				//base.SubstituteKeyFieldUpdating(sender, e);

				InventoryItem inventoryItem = null;
				if (e.NewValue != null)
				{
					inventoryItem = PXSelectReadonly<InventoryItem, Where<InventoryItem.inventoryCD, Equal<Required<InventoryItem.inventoryCD>>>>.SelectWindowed(sender.Graph, 0, 1, e.NewValue);

					if (inventoryItem == null)
					{
						inventoryItem = InventoryItem.PK.Find(sender.Graph, (int?)e.NewValue);
					}
				}
				if (inventoryItem != null)
				{
					e.NewValue = inventoryItem.InventoryID;
					e.Cancel = true;
				}
				else if (e.NewValue != null)
					throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ValueDoesntExist, _FieldName, e.NewValue));
			}

			public override void SubstituteKeyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				object value = e.ReturnValue;
				e.ReturnValue = null;
				base.FieldSelecting(sender, e);

				InventoryItem inventoryItem = InventoryItem.PK.Find(sender.Graph, (int?)value);
				if (inventoryItem != null)
				{
					e.ReturnValue = inventoryItem.InventoryCD;
				}
				else
				{
					if (e.Row != null)
						e.ReturnValue = null;
				}
			}
		}
	}
}
