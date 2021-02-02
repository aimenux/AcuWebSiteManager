using PX.Common;
using PX.Data;
using PX.Objects.IN.Matrix.DAC.Unbound;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.IN.Matrix.Attributes
{
	public class MatrixInventoryItemAttribute : PXEventSubscriberAttribute, IPXRowSelectedSubscriber
	{
		protected virtual Type[] GetInventoryVisibleFields() => new Type[]
		{
			typeof(InventoryItem.selected),
			typeof(InventoryItem.inventoryCD),
			typeof(InventoryItem.descr),
			typeof(InventoryItem.stkItem),
			typeof(InventoryItem.itemClassID),
			typeof(InventoryItem.itemType),
			typeof(InventoryItem.valMethod),
			typeof(InventoryItem.lotSerClassID),
			typeof(InventoryItem.dfltSiteID),
			typeof(InventoryItem.taxCategoryID),
		};

		protected Lazy<HashSet<string>> inventoryVisibleFields;

		public MatrixInventoryItemAttribute()
		{
			inventoryVisibleFields = new Lazy<HashSet<string>>(
				() => GetInventoryVisibleFields()
					.Select(t => t.Name)
					.ToHashSet(StringComparer.OrdinalIgnoreCase));
		}

		public override void CacheAttached(PXCache sender)
		{
			foreach (var fieldName in sender.Fields)
			{
				sender.Graph.FieldSelecting.AddHandler(sender.GetItemType(),
					fieldName,
					(s, e) => AnyFieldSelecting(s, e, fieldName));

			}
		}

		protected virtual void AnyFieldSelecting(PXCache sender, PXFieldSelectingEventArgs e, string fieldName)
		{
			if (sender.GetFieldOrdinal(fieldName) < 0)
			{
				e.ReturnState = PXFieldState.CreateInstance(e.ReturnState, typeof(string), visible: false);
			}
		}

		public virtual void RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			sender.GetAttributesOfType<PXUIFieldAttribute>(null, null).ForEach(attr =>
			{
				attr.Visible = inventoryVisibleFields.Value.Contains(attr.FieldName);
				if (!attr.FieldName.Equals(typeof(MatrixInventoryItem.selected).Name, StringComparison.OrdinalIgnoreCase))
					attr.Enabled = false;
			});

			var row = (MatrixInventoryItem)e.Row;
			if (row != null)
			{
				PXUIFieldAttribute.SetEnabled<MatrixInventoryItem.selected>(sender, row, row.Exists != true && row.Duplicate != true);
				string warnMessage = (row.Exists == true) ? Messages.InventoryIDExists
					: (row.Duplicate == true) ? Messages.InventoryIDDuplicates : null;
				var warnExc = (warnMessage == null) ? null : new PXSetPropertyException(warnMessage, PXErrorLevel.Warning);
				sender.RaiseExceptionHandling<MatrixInventoryItem.selected>(row, false, warnExc);
			}
		}
	}
}
